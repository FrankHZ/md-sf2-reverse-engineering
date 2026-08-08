local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))

local f,a=config.static.functionEntries,config.static.addresses
local callback_expectations=config.callbackExpectations
local probe_base,call_pc,return_pc,gate_pc,stack_top=0xFF6800,0xFF6820,0xFF6826,0xFF6830,0xFFFF00
local callbacks,event_ids,records,steps={}, {}, {}, {}
local bootstrap_armed,bootstrapped,active,observer_failed,session_cleaned=false,false,false,false,false
local step_index=1
local current_phase,current_role,current_pc,current_expectation="registration","registration",nil,nil
local direct_call_seen,direct_target_seen,source_call_seen,source_return_seen=false,false,false,false
local wait_state=nil
local write_probe

local function status(value) local file=assert(io.open(config.statusPath,"a"));file:write(value.."\n");file:close() end
local function bool(value) return value and "true" or "false" end
local function nullable(value) return value==nil and "null" or tostring(value) end
local function quote(value) return string.format("%q",value) end
local function target_for(case)
  if case.kind=="sample" then return f.UpdatePlayerInputs end
  if case.kind=="repeat" then return f.ApplyZ80BusUpdates end
  if case.kind=="wait" then return assert(f[case.helper],"unknown wait helper") end
  error("unknown controller-input case kind")
end
local function current_step() return steps[step_index] end
local function current_case() local step=current_step();return step and step.case or nil end
local function current_frame() local step=current_step();return step and step.frame or nil end
local function unregister_events() for index=#event_ids,1,-1 do event.unregisterbyid(event_ids[index]);event_ids[index]=nil end end
local function cleanup_session() if session_cleaned then return end;session_cleaned=true;unregister_events() end
local function roles_json(address)
  local values={};for _,entry in ipairs(callbacks[address] or {}) do values[#values+1]=quote(entry.role) end
  return "["..table.concat(values,",").."]"
end
local function pending_callback_state()
  local step=current_step();local case=step and step.case
  return "{\"active\":"..bool(active)..",\"caseIndex\":"..(step and step.caseIndex-1 or 0)..",\"frameIndex\":"..(step and step.frameIndex-1 or 0)..",\"expectedFunctionPc\":"..nullable(case and target_for(case) or nil)..",\"pendingReturnPc\":"..nullable(active and return_pc or nil)..",\"rolesAtPc\":"..roles_json(current_pc).."}"
end
local function fail_callback(message)
  if observer_failed then return end
  observer_failed=true
  local case=current_case();local expected=current_expectation or {}
  local payload="{\"owner\":"..quote(config.observerFailureContract.owner)..",\"caseId\":"..(case and quote(case.id) or "null")..",\"phase\":"..quote(current_phase)..",\"role\":"..quote(current_role)..",\"actualPc\":"..nullable(emu.getregister("M68K PC"))..",\"expectedCallPc\":"..nullable(expected.callPc)..",\"expectedTargetPc\":"..nullable(expected.targetPc)..",\"expectedReturnPc\":"..nullable(expected.returnPc)..",\"pendingCallback\":"..pending_callback_state()..",\"error\":"..quote(tostring(message)).."}"
  local diagnostic=config.observerFailureContract.statusPrefix..payload
  status(diagnostic);print(diagnostic);os.remove(config.outputPath);cleanup_session();client.exitCode(config.observerFailureContract.exitCode)
end
local function expectation_key(case) return case.kind=="wait" and case.helper or case.kind end
local function expectation(case,role,flow_index)
  local matches={}
  for _,entry in ipairs(assert(callback_expectations[expectation_key(case)],"missing callback expectations")) do
    if entry.role==role and entry.callbackAddress==current_pc and (flow_index==nil or entry.flowIndex==flow_index) then matches[#matches+1]=entry end
  end
  if #matches~=1 then error("missing or ambiguous configured callback expectation for "..expectation_key(case).."/"..role.." at "..tostring(current_pc)) end
  local entry=matches[1]
  return {callPc=entry.callSiteAddress,targetPc=entry.targetAddress,returnPc=entry.returnAddress,flowIndex=entry.flowIndex}
end
local function input_table(buttons) local result={};for _,button in ipairs(buttons) do result[button]=true end;return result end
local function set_inputs(frame) joypad.set(input_table(frame.player1Buttons),1);joypad.set(input_table(frame.player2Buttons),2) end
local button_symbols={Up="INPUT_UP",Down="INPUT_DOWN",Left="INPUT_LEFT",Right="INPUT_RIGHT",B="INPUT_B",C="INPUT_C",A="INPUT_A",Start="INPUT_START"}
local function button_value(buttons)
  local result=0;for _,button in ipairs(buttons) do result=result|assert(config.static.buttonMasks[button_symbols[button]],"unknown controller button") end;return result
end
local function raw_state_bytes() return {memory.read_u8(a.PLAYER_1_INPUT,"M68K BUS"),memory.read_u8(a.PLAYER_1_INPUT+1,"M68K BUS"),memory.read_u8(a.PLAYER_2_INPUT,"M68K BUS"),memory.read_u8(a.PLAYER_2_INPUT+1,"M68K BUS")} end
local function raw_json(values) return "["..table.concat(values,",").."]" end
local function frame_json(frame) return "{\"rawStateBytes\":"..raw_json(frame.rawStateBytes)..",\"currentPlayerInput\":"..frame.currentPlayerInput..",\"lastPlayerInput\":"..frame.lastPlayerInput..",\"inputRepeatDelayer\":"..frame.inputRepeatDelayer.."}" end
local function wait_result_json(result)
  local frames={};for _,frame in ipairs(result.frames) do frames[#frames+1]=frame_json(frame) end
  local d5=result.d5After==nil and "" or ",\"d5After\":"..result.d5After
  return "{\"helperEntryCount\":"..result.helperEntryCount..",\"helperReturnCount\":"..result.helperReturnCount..",\"waitForVIntEntryCount\":"..result.waitForVIntEntryCount..",\"waitForVIntReturnCount\":"..result.waitForVIntReturnCount..",\"vIntEntryCount\":"..result.vIntEntryCount..",\"vIntInputStageCount\":"..result.vIntInputStageCount..",\"frames\":["..table.concat(frames,",").."]"..d5.."}"
end
local function record_json(record)
  if record.kind=="sample" then return "{\"id\":"..quote(record.id)..",\"result\":{\"rawStateBytes\":"..raw_json(record.result.rawStateBytes).."}}" end
  if record.kind=="wait" then return "{\"id\":"..quote(record.id)..",\"result\":"..wait_result_json(record.result).."}" end
  local frames={};for _,frame in ipairs(record.result.frames) do frames[#frames+1]=frame_json(frame) end
  return "{\"id\":"..quote(record.id)..",\"result\":{\"frames\":["..table.concat(frames,",").."]}}"
end
local function write_output()
  local file=assert(io.open(config.outputPath,"w"));local rows,order={},{ }
  for _,record in ipairs(records) do rows[#rows+1]=record_json(record) end
  for _,id in ipairs(config.caseOrder) do order[#order+1]=quote(id) end
  file:write("{\"system\":"..quote(emu.getsystemid())..",\"core\":"..quote(config.core)..",\"id\":"..quote(config.id)..",\"caseOrder\":["..table.concat(order,",").."],\"records\":["..table.concat(rows,",").."],\"callbacksCleared\":0}");file:close()
end
local function reset_repeat_state(step)
  if step.case.kind~="repeat" or step.frameIndex~=1 then return end
  memory.write_u8(a.CURRENT_PLAYER_INPUT,0,"M68K BUS");memory.write_u8(a.LAST_PLAYER_INPUT,0,"M68K BUS");memory.write_u8(a.INPUT_REPEAT_DELAYER,0,"M68K BUS")
end
local function begin_wait_state(case)
  set_inputs(case.initial)
  wait_state={helperEntryCount=0,helperReturnCount=0,waitForVIntCallCount=0,waitForVIntEntryCount=0,waitForVIntReturnCount=0,vIntEntryCount=0,vIntInputStageCount=0,frames={},flowIndex=nil,waitForVIntActive=false,ownedVInt=false,firstWaitForVIntSetupConsumed=false,waitForVIntCallSeen=false,waitForVIntTargetSeen=false,waitForVIntRtsSeen=false,vintInputCallSeen=false,vintInputStageSeen=false}
end
local function seed_helper_entry_inputs(case)
  local p1=button_value(case.initial.player1Buttons)
  if case.helper=="WaitForPlayerInput" then
    memory.write_u8(a.CURRENT_PLAYER_INPUT,p1,"M68K BUS")
    if memory.read_u8(a.CURRENT_PLAYER_INPUT,"M68K BUS")~=p1 then error("wait helper entry input readback drift") end
  else
    memory.write_u8(a.PLAYER_1_INPUT,p1,"M68K BUS")
    if memory.read_u8(a.PLAYER_1_INPUT,"M68K BUS")~=p1 then error("wait helper entry input readback drift") end
  end
end
local function seed_first_wait_for_vint_state(case)
  local p1=button_value(case.initial.player1Buttons)
  memory.write_u8(a.CURRENT_PLAYER_INPUT,p1,"M68K BUS");memory.write_u8(a.LAST_PLAYER_INPUT,p1,"M68K BUS");memory.write_u8(a.INPUT_REPEAT_DELAYER,0,"M68K BUS")
  if memory.read_u8(a.CURRENT_PLAYER_INPUT,"M68K BUS")~=p1 or memory.read_u8(a.LAST_PLAYER_INPUT,"M68K BUS")~=p1 or memory.read_u8(a.INPUT_REPEAT_DELAYER,"M68K BUS")~=0 then error("wait helper first-call setup readback drift") end
end
local function wait_input(case,index)
  if #case.vintInputs==0 then return {player1Buttons={},player2Buttons={}} end
  return case.vintInputs[math.min(index,#case.vintInputs)]
end
local function write_call(target,wait_case,enable_vint)
  local entry_pc=call_pc
  if wait_case then
    memory.write_u16_be(call_pc-6,0x2A3C,"M68K BUS");memory.write_u32_be(call_pc-4,config.probeD5,"M68K BUS")
    if memory.read_u16_be(call_pc-6,"M68K BUS")~=0x2A3C or memory.read_u32_be(call_pc-4,"M68K BUS")~=config.probeD5 then error("direct input probe D5 preamble write drift") end
    entry_pc=call_pc-6
    if enable_vint then
      memory.write_u16_be(call_pc-10,0x46FC,"M68K BUS");memory.write_u16_be(call_pc-8,0x2000,"M68K BUS")
      if memory.read_u16_be(call_pc-10,"M68K BUS")~=0x46FC or memory.read_u16_be(call_pc-8,"M68K BUS")~=0x2000 then error("direct input probe VInt SR preamble write drift") end
      entry_pc=call_pc-10
    end
  end
  memory.write_u16_be(call_pc,0x4EB9,"M68K BUS");memory.write_u32_be(call_pc+2,target,"M68K BUS")
  memory.write_u16_be(return_pc,0x4E71,"M68K BUS");memory.write_u16_be(return_pc+2,0x4EF9,"M68K BUS");memory.write_u32_be(return_pc+4,gate_pc,"M68K BUS")
  if memory.read_u16_be(call_pc,"M68K BUS")~=0x4EB9 or memory.read_u32_be(call_pc+2,"M68K BUS")~=target or memory.read_u16_be(return_pc,"M68K BUS")~=0x4E71 or memory.read_u16_be(return_pc+2,"M68K BUS")~=0x4EF9 or memory.read_u32_be(return_pc+4,"M68K BUS")~=gate_pc then error("direct input probe JSR/loop write drift") end
  return entry_pc
end
local function require_pc(expected,label) if emu.getregister("M68K PC")~=expected then error(label.." PC drift") end end
local function arm_step()
  if not bootstrapped or active or step_index>#steps then return end
  local step=current_step();local case,frame=step.case,step.frame
  current_phase,current_role,current_expectation="case-arm","direct-call",nil
  if case.kind=="wait" then begin_wait_state(case) else reset_repeat_state(step);set_inputs(frame) end
  local waits_vint=case.kind=="wait" and config.waitExpectations[case.id].waitForVIntEntryCount>0
  local entry_pc=write_call(target_for(case),case.kind=="wait",waits_vint)
  memory.write_u16_be(gate_pc,0x4EF9,"M68K BUS");memory.write_u32_be(gate_pc+2,entry_pc,"M68K BUS")
  if memory.read_u16_be(gate_pc,"M68K BUS")~=0x4EF9 or memory.read_u32_be(gate_pc+2,"M68K BUS")~=entry_pc then error("direct input probe gate arm drift") end
  active=true;direct_call_seen,direct_target_seen,source_call_seen,source_return_seen=false,false,false,false
end
local function on_bootstrap_entry()
  if bootstrap_armed then return end
  current_phase,current_role,current_expectation="bootstrap-entry","bootstrap-return-redirect",{callPc=nil,targetPc=f.CheckSram,returnPc=probe_base}
  local stack=emu.getregister("M68K A7")&0xFFFFFF
  if stack<0xFF0000 or stack>0xFFFFFF then error("CheckSram return stack outside work RAM") end
  memory.write_u32_be(stack,probe_base,"M68K BUS");if memory.read_u32_be(stack,"M68K BUS")~=probe_base then error("CheckSram return thunk write drift") end
  memory.write_u16_be(probe_base,0x46FC,"M68K BUS");memory.write_u16_be(probe_base+2,0x2700,"M68K BUS");memory.write_u16_be(probe_base+4,0x2E7C,"M68K BUS");memory.write_u32_be(probe_base+6,stack_top,"M68K BUS");memory.write_u16_be(probe_base+10,0x4EF9,"M68K BUS");memory.write_u32_be(probe_base+12,gate_pc,"M68K BUS");memory.write_u16_be(gate_pc,0x60FE,"M68K BUS")
  if memory.read_u16_be(probe_base,"M68K BUS")~=0x46FC or memory.read_u16_be(probe_base+2,"M68K BUS")~=0x2700 or memory.read_u16_be(probe_base+4,"M68K BUS")~=0x2E7C or memory.read_u32_be(probe_base+6,"M68K BUS")~=stack_top or memory.read_u16_be(probe_base+10,"M68K BUS")~=0x4EF9 or memory.read_u32_be(probe_base+12,"M68K BUS")~=gate_pc or memory.read_u16_be(gate_pc,"M68K BUS")~=0x60FE then error("direct input probe bootstrap write drift") end
  write_probe();bootstrap_armed=true
end
local function on_bootstrap_return()
  if not bootstrap_armed then error("direct input probe returned before bootstrap arm") end
  current_phase,current_role,current_expectation="bootstrap-return","direct-input-probe",{callPc=nil,targetPc=f.CheckSram,returnPc=probe_base}
  require_pc(probe_base,"CheckSram return");bootstrapped=true;status("milestone:direct-input-probe")
end
local function on_direct_call()
  local case=current_case();if not active or not case then return end
  current_phase,current_role,current_expectation="direct-call","direct-call",expectation(case,"direct-call")
  require_pc(current_expectation.callPc,"direct input call");if memory.read_u32_be(call_pc+2,"M68K BUS")~=current_expectation.targetPc then error("direct input call target drift") end;direct_call_seen=true
end
local function on_apply_target()
  local case=current_case();if not active or case.kind~="repeat" or not direct_call_seen then return end
  current_phase,current_role,current_expectation="apply-target","apply-target",expectation(case,"apply-target");require_pc(current_expectation.targetPc,"direct ApplyZ80BusUpdates target");direct_target_seen=true
end
local function on_source_call()
  local case=current_case();if not active or case.kind~="repeat" or not direct_target_seen then return end
  current_phase,current_role,current_expectation="source-call","source-call",expectation(case,"source-call");require_pc(current_expectation.callPc,"ApplyZ80BusUpdates UpdatePlayerInputs call");source_call_seen=true
end
local function on_update_target()
  local case=current_case();if not active or case.kind=="wait" or not direct_call_seen then return end
  local expected_nested=case.kind=="repeat";if expected_nested and not source_call_seen then return end
  current_phase,current_role,current_expectation="update-target","update-target",expectation(case,"update-target");require_pc(current_expectation.targetPc,"UpdatePlayerInputs target");direct_target_seen=true
end
local function on_source_return()
  local case=current_case();if not active or case.kind~="repeat" or not source_call_seen or not direct_target_seen then return end
  current_phase,current_role,current_expectation="source-return","source-return",expectation(case,"source-return");require_pc(current_expectation.returnPc,"ApplyZ80BusUpdates UpdatePlayerInputs return");source_return_seen=true
end
local function on_wait_helper_target()
  local case=current_case();if not active or case.kind~="wait" then return end
  current_phase,current_role,current_expectation="wait-helper-target","wait-helper-target",nil;current_expectation=expectation(case,"wait-helper-target")
  if not direct_call_seen then error("wait helper target before direct call") end
  require_pc(current_expectation.targetPc,"wait helper target")
  if wait_state.helperEntryCount==0 then seed_helper_entry_inputs(case) end
  wait_state.helperEntryCount=wait_state.helperEntryCount+1;direct_target_seen=true
end
local function on_wait_helper_return()
  local case=current_case();if not active or case.kind~="wait" then return end
  current_phase,current_role,current_expectation="wait-helper-return","wait-helper-return",nil;current_expectation=expectation(case,"wait-helper-return")
  if not direct_target_seen then error("wait helper return before target") end
  require_pc(current_expectation.callbackPc or current_pc,"wait helper return");wait_state.helperReturnCount=wait_state.helperReturnCount+1
end
local function on_wait_for_vint_call()
  local case=current_case();if not active or case.kind~="wait" then return end
  current_phase,current_role,current_expectation="wait-for-vint-call","wait-for-vint-call",nil;current_expectation=expectation(case,"wait-for-vint-call")
  if not direct_target_seen then error("wait-for-vint call before wait helper target") end
  if wait_state.waitForVIntActive or wait_state.flowIndex~=nil then error("wait-for-vint call before prior cycle return") end
  require_pc(current_expectation.callPc,"WaitForVInt call")
  if not wait_state.firstWaitForVIntSetupConsumed then seed_first_wait_for_vint_state(case);wait_state.firstWaitForVIntSetupConsumed=true end
  wait_state.flowIndex=current_expectation.flowIndex;wait_state.waitForVIntActive=false;wait_state.ownedVInt=false;wait_state.waitForVIntCallSeen=true;wait_state.waitForVIntTargetSeen=false;wait_state.waitForVIntRtsSeen=false;wait_state.vintInputCallSeen=false;wait_state.vintInputStageSeen=false;wait_state.waitForVIntCallCount=wait_state.waitForVIntCallCount+1;set_inputs(wait_input(case,wait_state.waitForVIntCallCount))
end
local function on_wait_for_vint_target()
  local case=current_case();if not active or case.kind~="wait" then return end
  current_phase,current_role,current_expectation="wait-for-vint-target","wait-for-vint-target",nil
  if wait_state.flowIndex~=nil then current_expectation=expectation(case,"wait-for-vint-target",wait_state.flowIndex) end
  if wait_state.flowIndex==nil or not wait_state.waitForVIntCallSeen then error("wait-for-vint target before call") end
  if wait_state.waitForVIntTargetSeen then error("duplicate wait-for-vint target") end
  require_pc(current_expectation.targetPc,"WaitForVInt target");wait_state.waitForVIntTargetSeen=true;wait_state.waitForVIntActive=true;wait_state.waitForVIntEntryCount=wait_state.waitForVIntEntryCount+1
end
local function on_wait_for_vint_rts()
  local case=current_case();if not active or case.kind~="wait" then return end
  current_phase,current_role,current_expectation="wait-for-vint-rts","wait-for-vint-rts",nil
  if wait_state.flowIndex~=nil then current_expectation=expectation(case,"wait-for-vint-rts",wait_state.flowIndex) end
  if wait_state.flowIndex==nil or not wait_state.waitForVIntCallSeen or not wait_state.waitForVIntTargetSeen then error("wait-for-vint rts before target") end
  if wait_state.waitForVIntRtsSeen then error("duplicate wait-for-vint rts") end
  require_pc(current_pc,"WaitForVInt rts");wait_state.waitForVIntRtsSeen=true
end
local function on_wait_for_vint_return()
  local case=current_case();if not active or case.kind~="wait" then return end
  current_phase,current_role,current_expectation="wait-for-vint-return","wait-for-vint-return",nil
  if wait_state.flowIndex~=nil then current_expectation=expectation(case,"wait-for-vint-return",wait_state.flowIndex) end
  if wait_state.flowIndex==nil or not wait_state.waitForVIntCallSeen or not wait_state.waitForVIntTargetSeen or not wait_state.waitForVIntRtsSeen then error("wait-for-vint return before rts") end
  require_pc(current_expectation.returnPc,"WaitForVInt return");wait_state.waitForVIntReturnCount=wait_state.waitForVIntReturnCount+1
  wait_state.waitForVIntActive=false;wait_state.flowIndex=nil;wait_state.waitForVIntCallSeen=false;wait_state.waitForVIntTargetSeen=false;wait_state.waitForVIntRtsSeen=false
end
local function on_vint_target()
  local case=current_case();if not active or case.kind~="wait" or not wait_state.waitForVIntActive then return end
  if memory.read_u8(config.static.flow.waitingNextVIntAddress,"M68K BUS")==0 then wait_state.ownedVInt=false;return end
  current_phase,current_role,current_expectation="vint-target","vint-target",nil;current_expectation=expectation(case,"vint-target")
  if wait_state.ownedVInt then error("duplicate owned VInt entry") end
  require_pc(f.VInt,"VInt entry");wait_state.vIntEntryCount=wait_state.vIntEntryCount+1
  wait_state.ownedVInt=true;wait_state.vintInputCallSeen=false;wait_state.vintInputStageSeen=false
end
local function on_vint_input_call()
  local case=current_case();if not active or case.kind~="wait" or not wait_state.ownedVInt then return end
  current_phase,current_role,current_expectation="vint-input-call","vint-input-call",nil;current_expectation=expectation(case,"vint-input-call")
  if wait_state.vintInputCallSeen then error("duplicate VInt ApplyZ80BusUpdates call") end
  require_pc(current_expectation.callPc,"VInt ApplyZ80BusUpdates call");wait_state.vintInputCallSeen=true
end
local function on_vint_input_stage()
  local case=current_case();if not active or case.kind~="wait" or not wait_state.ownedVInt then return end
  current_phase,current_role,current_expectation="vint-input-stage","vint-input-stage",nil;current_expectation=expectation(case,"vint-input-stage")
  if not wait_state.vintInputCallSeen then error("VInt ApplyZ80BusUpdates target before call") end
  if wait_state.vintInputStageSeen then error("duplicate VInt ApplyZ80BusUpdates target") end
  require_pc(current_expectation.targetPc,"VInt ApplyZ80BusUpdates target");wait_state.vintInputStageSeen=true;wait_state.vIntInputStageCount=wait_state.vIntInputStageCount+1
end
local function on_vint_input_return()
  local case=current_case();if not active or case.kind~="wait" or not wait_state.ownedVInt then return end
  current_phase,current_role,current_expectation="vint-input-return","vint-input-return",nil;current_expectation=expectation(case,"vint-input-return")
  if not wait_state.vintInputCallSeen or not wait_state.vintInputStageSeen then error("VInt ApplyZ80BusUpdates return before stage") end
  require_pc(current_expectation.returnPc,"VInt ApplyZ80BusUpdates return")
  wait_state.frames[#wait_state.frames+1]={rawStateBytes=raw_state_bytes(),currentPlayerInput=memory.read_u8(a.CURRENT_PLAYER_INPUT,"M68K BUS"),lastPlayerInput=memory.read_u8(a.LAST_PLAYER_INPUT,"M68K BUS"),inputRepeatDelayer=memory.read_u8(a.INPUT_REPEAT_DELAYER,"M68K BUS")}
  wait_state.ownedVInt=false
end
local function finish_session()
  cleanup_session();if #event_ids~=0 then error("residual registered callback") end;write_output();status("milestone:callbacks-cleared:0");status("milestone:observer-finished");client.exitCode(0)
end
local function on_direct_return()
  local case=current_case();if not active or not case then error("direct input return without active case") end
  current_phase,current_role,current_expectation="direct-return","direct-return",expectation(case,"direct-return");require_pc(current_expectation.returnPc,"direct input return")
  if not direct_target_seen then error("direct input target callback missing") end
  if case.kind=="repeat" and (not source_call_seen or not source_return_seen) then error("ApplyZ80BusUpdates callback chronology drift") end
  local states=raw_state_bytes()
  if case.kind=="sample" then records[#records+1]={id=case.id,kind=case.kind,result={rawStateBytes=states}}
  elseif case.kind=="repeat" then
    local record=records[#records];if not record or record.id~=case.id then record={id=case.id,kind=case.kind,result={frames={}}};records[#records+1]=record end
    record.result.frames[#record.result.frames+1]={rawStateBytes=states,currentPlayerInput=memory.read_u8(a.CURRENT_PLAYER_INPUT,"M68K BUS"),lastPlayerInput=memory.read_u8(a.LAST_PLAYER_INPUT,"M68K BUS"),inputRepeatDelayer=memory.read_u8(a.INPUT_REPEAT_DELAYER,"M68K BUS")}
  else
    local expected=assert(config.waitExpectations[case.id],"missing derived wait expectation")
    for name,value in pairs(expected) do if wait_state[name]~=value then error("wait callback count drift for "..name..": expected "..value..", actual "..tostring(wait_state[name])) end end
    if #wait_state.frames~=wait_state.vIntInputStageCount then error("wait VInt frame chronology drift") end
    if case.helper=="WaitForInputFor1Second" or case.helper=="WaitForInputFor3Seconds" then wait_state.d5After=emu.getregister("M68K D5")&0xFFFFFFFF end
    records[#records+1]={id=case.id,kind=case.kind,result=wait_state};wait_state=nil
  end
  memory.write_u16_be(gate_pc,0x60FE,"M68K BUS");if memory.read_u16_be(gate_pc,"M68K BUS")~=0x60FE then error("direct input probe gate pause drift") end
  active=false;step_index=step_index+1;if step_index>#steps then finish_session() end
end
local function dispatch(address,role,index)
  current_pc=address
  if role~="bootstrap-entry" and role~="bootstrap-return" and not active then return end
  if role=="bootstrap-entry" then on_bootstrap_entry()
  elseif role=="bootstrap-return" then on_bootstrap_return()
  elseif role=="direct-call" then on_direct_call()
  elseif role=="apply-target" then on_apply_target()
  elseif role=="source-call" then on_source_call()
  elseif role=="update-target" then on_update_target()
  elseif role=="source-return" then on_source_return()
  elseif role=="wait-helper-target" then on_wait_helper_target()
  elseif role=="wait-helper-return" then on_wait_helper_return()
  elseif role=="wait-for-vint-call" then on_wait_for_vint_call()
  elseif role=="wait-for-vint-target" then on_wait_for_vint_target()
  elseif role=="wait-for-vint-rts" then on_wait_for_vint_rts()
  elseif role=="wait-for-vint-return" then on_wait_for_vint_return()
  elseif role=="vint-target" then on_vint_target()
  elseif role=="vint-input-call" then on_vint_input_call()
  elseif role=="vint-input-stage" then on_vint_input_stage()
  elseif role=="vint-input-return" then on_vint_input_return()
  elseif role=="direct-return" then on_direct_return()
  else error("unknown deterministic dispatch role: "..role) end
end
local function register_exec(address,role,index)
  if not callbacks[address] then
    callbacks[address]={}
    event_ids[#event_ids+1]=event.on_bus_exec(function()
      if observer_failed then return end
      local ok,message=pcall(function() for _,entry in ipairs(callbacks[address]) do dispatch(address,entry.role,entry.index) end end)
      if not ok then fail_callback(message) end
    end,address,"controller-input-"..address,"M68K BUS")
  end
  for _,entry in ipairs(callbacks[address]) do if entry.role==role and entry.index==index then error("duplicate physical-PC callback role: "..role) end end
  callbacks[address][#callbacks[address]+1]={role=role,index=index}
end
local function register_exec_once(address,role)
  for _,entry in ipairs(callbacks[address] or {}) do if entry.role==role then return end end
  register_exec(address,role,0)
end
write_probe=function()
  for case_index,case in ipairs(config.cases) do
    if case.kind=="wait" then steps[#steps+1]={caseIndex=case_index,frameIndex=1,case=case,frame=nil}
    else for frame_index,frame in ipairs(case.frames) do steps[#steps+1]={caseIndex=case_index,frameIndex=frame_index,case=case,frame=frame} end end
  end
  if #steps==0 then error("controller-input fixture input exhausted") end
  register_exec(probe_base,"bootstrap-return",0);register_exec(call_pc,"direct-call",0);register_exec(return_pc,"direct-return",0)
  register_exec(f.ApplyZ80BusUpdates,"apply-target",0);register_exec(config.static.flow.applyInputCall[1],"source-call",0);register_exec(f.UpdatePlayerInputs,"update-target",0);register_exec(config.static.flow.applyInputCall[2],"source-return",0)
  for _,helper in ipairs({"WaitForPlayerInput","WaitForPlayer1NewInput","WaitForInputFor1Second","WaitForInputFor3Seconds"}) do
    local flow=config.static.flow.waitHelper[helper]
    register_exec_once(f[helper],"wait-helper-target")
    register_exec_once(flow.rtsPc,"wait-helper-return")
    for _,pair in ipairs(flow.vintCalls) do register_exec_once(pair[1],"wait-for-vint-call");register_exec_once(pair[2],"wait-for-vint-return") end
  end
  register_exec(f.WaitForVInt,"wait-for-vint-target",0);register_exec(config.static.flow.waitForVIntRtsPc,"wait-for-vint-rts",0)
  register_exec(f.VInt,"vint-target",0);register_exec(config.static.flow.vIntApplyInput[1],"vint-input-call",0);register_exec(f.ApplyZ80BusUpdates,"vint-input-stage",0);register_exec(config.static.flow.vIntApplyInput[2],"vint-input-return",0)
end

status("milestone:observer-loaded")
local ok,message=pcall(function() register_exec(f.CheckSram,"bootstrap-entry",0) end)
if not ok then fail_callback(message) end
local frames=0
while true do
  local loop_ok,loop_message=pcall(function()
    frames=frames+1;if bootstrapped then arm_step() elseif not bootstrap_armed then joypad.set({Start=true},1);joypad.set({},2) end;emu.frameadvance()
    if frames%600==0 then status("frame="..frames..",pc="..string.format("%X",emu.getregister("M68K PC"))..",step="..step_index) end
  end)
  if not loop_ok then fail_callback(loop_message) end
end
