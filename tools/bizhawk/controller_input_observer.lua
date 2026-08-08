local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))

local f,a=config.static.functionEntries,config.static.addresses
local callback_expectations=config.callbackExpectations
local probe_base,call_pc,return_pc,gate_pc,stack_top=0xFF6800,0xFF6820,0xFF6826,0xFF6830,0xFFFF00
local callbacks,event_ids,records,steps={}, {}, {}, {}
local bootstrap_armed,bootstrapped,active,observer_failed,session_cleaned=false,false,false,false,false
local step_index=1
local current_phase,current_role,current_pc,current_expectation="registration","registration",nil,nil
local direct_call_seen,direct_target_seen,source_call_seen,source_return_seen=false,false,false,false
local write_probe

local function status(value) local file=assert(io.open(config.statusPath,"a"));file:write(value.."\n");file:close() end
local function bool(value) return value and "true" or "false" end
local function nullable(value) return value==nil and "null" or tostring(value) end
local function quote(value) return string.format("%q",value) end
local function target_for(case) return case.kind=="sample" and f.UpdatePlayerInputs or f.ApplyZ80BusUpdates end
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
local function expectation(case,role)
  for _,entry in ipairs(assert(callback_expectations[case.kind],"missing callback expectations for case kind")) do
    if entry.role==role then return {callPc=entry.callSiteAddress,targetPc=entry.targetAddress,returnPc=entry.returnAddress} end
  end
  error("missing callback expectation for "..case.kind.."/"..role)
end
local function input_table(buttons) local result={};for _,button in ipairs(buttons) do result[button]=true end;return result end
local function set_inputs(frame) joypad.set(input_table(frame.player1Buttons),1);joypad.set(input_table(frame.player2Buttons),2) end
local function raw_state_bytes() return {memory.read_u8(a.PLAYER_1_INPUT,"M68K BUS"),memory.read_u8(a.PLAYER_1_INPUT+1,"M68K BUS"),memory.read_u8(a.PLAYER_2_INPUT,"M68K BUS"),memory.read_u8(a.PLAYER_2_INPUT+1,"M68K BUS")} end
local function raw_json(values) return "["..table.concat(values,",").."]" end
local function frame_json(frame) return "{\"rawStateBytes\":"..raw_json(frame.rawStateBytes)..",\"currentPlayerInput\":"..frame.currentPlayerInput..",\"lastPlayerInput\":"..frame.lastPlayerInput..",\"inputRepeatDelayer\":"..frame.inputRepeatDelayer.."}" end
local function record_json(record)
  if record.kind=="sample" then return "{\"id\":"..quote(record.id)..",\"result\":{\"rawStateBytes\":"..raw_json(record.result.rawStateBytes).."}}" end
  local frames={};for _,frame in ipairs(record.result.frames) do frames[#frames+1]=frame_json(frame) end
  return "{\"id\":"..quote(record.id)..",\"result\":{\"frames\":["..table.concat(frames,",").."]}}"
end
local function write_output()
  local file=assert(io.open(config.outputPath,"w"));local rows,order={},{}
  for _,record in ipairs(records) do rows[#rows+1]=record_json(record) end
  for _,id in ipairs(config.caseOrder) do order[#order+1]=quote(id) end
  file:write("{\"system\":"..quote(emu.getsystemid())..",\"core\":"..quote(config.core)..",\"id\":"..quote(config.id)..",\"caseOrder\":["..table.concat(order,",").."],\"records\":["..table.concat(rows,",").."],\"callbacksCleared\":0}");file:close()
end
local function reset_repeat_state(step)
  if step.case.kind~="repeat" or step.frameIndex~=1 then return end
  memory.write_u8(a.CURRENT_PLAYER_INPUT,0,"M68K BUS");memory.write_u8(a.LAST_PLAYER_INPUT,0,"M68K BUS");memory.write_u8(a.INPUT_REPEAT_DELAYER,0,"M68K BUS")
end
local function write_call(target)
  memory.write_u16_be(call_pc,0x4EB9,"M68K BUS");memory.write_u32_be(call_pc+2,target,"M68K BUS")
  memory.write_u16_be(return_pc,0x4E71,"M68K BUS");memory.write_u16_be(return_pc+2,0x4EF9,"M68K BUS");memory.write_u32_be(return_pc+4,gate_pc,"M68K BUS")
  if memory.read_u16_be(call_pc,"M68K BUS")~=0x4EB9 or memory.read_u32_be(call_pc+2,"M68K BUS")~=target or memory.read_u16_be(return_pc,"M68K BUS")~=0x4E71 or memory.read_u16_be(return_pc+2,"M68K BUS")~=0x4EF9 or memory.read_u32_be(return_pc+4,"M68K BUS")~=gate_pc then error("direct input probe JSR/loop write drift") end
end
local function require_pc(expected,label) if emu.getregister("M68K PC")~=expected then error(label.." PC drift") end end
local function arm_step()
  if not bootstrapped or active or step_index>#steps then return end
  local step=current_step();local case,frame=step.case,step.frame
  current_phase,current_role,current_expectation="case-arm","direct-call",expectation(case,"direct-call")
  reset_repeat_state(step);set_inputs(frame);write_call(target_for(case))
  memory.write_u16_be(gate_pc,0x4EF9,"M68K BUS");memory.write_u32_be(gate_pc+2,call_pc,"M68K BUS")
  if memory.read_u16_be(gate_pc,"M68K BUS")~=0x4EF9 or memory.read_u32_be(gate_pc+2,"M68K BUS")~=call_pc then error("direct input probe gate arm drift") end
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
  write_probe()
  bootstrap_armed=true
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
  local case=current_case();if not active or target_for(case)~=f.ApplyZ80BusUpdates or not direct_call_seen then return end
  current_phase,current_role,current_expectation="apply-target","apply-target",expectation(case,"apply-target");require_pc(current_expectation.targetPc,"VInt input-stage target");direct_target_seen=true
end
local function on_source_call()
  local case=current_case();if not active or target_for(case)~=f.ApplyZ80BusUpdates or not direct_target_seen then return end
  current_phase,current_role,current_expectation="source-call","source-call",expectation(case,"source-call");require_pc(current_expectation.callPc,"VInt UpdatePlayerInputs call");source_call_seen=true
end
local function on_update_target()
  local case=current_case();if not active or not direct_call_seen then return end
  local expected_nested=target_for(case)==f.ApplyZ80BusUpdates;if expected_nested and not source_call_seen then return end
  current_phase,current_role,current_expectation="update-target","update-target",expectation(case,"update-target");require_pc(current_expectation.targetPc,"UpdatePlayerInputs target");direct_target_seen=true
end
local function on_source_return()
  local case=current_case();if not active or target_for(case)~=f.ApplyZ80BusUpdates or not source_call_seen or not direct_target_seen then return end
  current_phase,current_role,current_expectation="source-return","source-return",expectation(case,"source-return");require_pc(current_expectation.returnPc,"VInt UpdatePlayerInputs return");source_return_seen=true
end
local function finish_session()
  cleanup_session();if #event_ids~=0 then error("residual registered callback") end;write_output();status("milestone:callbacks-cleared:0");status("milestone:observer-finished");client.exitCode(0)
end
local function on_direct_return()
  local case=current_case();if not active or not case then error("direct input return without active case") end
  current_phase,current_role,current_expectation="direct-return","direct-return",expectation(case,"direct-return");require_pc(current_expectation.returnPc,"direct input return")
  if not direct_target_seen then error("direct input target callback missing") end
  if case.kind=="repeat" and (not source_call_seen or not source_return_seen) then error("VInt input-stage callback chronology drift") end
  local states=raw_state_bytes()
  if case.kind=="sample" then records[#records+1]={id=case.id,kind=case.kind,result={rawStateBytes=states}}
  else
    local record=records[#records];if not record or record.id~=case.id then record={id=case.id,kind=case.kind,result={frames={}}};records[#records+1]=record end
    record.result.frames[#record.result.frames+1]={rawStateBytes=states,currentPlayerInput=memory.read_u8(a.CURRENT_PLAYER_INPUT,"M68K BUS"),lastPlayerInput=memory.read_u8(a.LAST_PLAYER_INPUT,"M68K BUS"),inputRepeatDelayer=memory.read_u8(a.INPUT_REPEAT_DELAYER,"M68K BUS")}
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
write_probe=function()
  for case_index,case in ipairs(config.cases) do for frame_index,frame in ipairs(case.frames) do steps[#steps+1]={caseIndex=case_index,frameIndex=frame_index,case=case,frame=frame} end end
  if #steps==0 then error("controller-input fixture input exhausted") end
  register_exec(probe_base,"bootstrap-return",0);register_exec(call_pc,"direct-call",0);register_exec(return_pc,"direct-return",0);register_exec(f.ApplyZ80BusUpdates,"apply-target",0);register_exec(config.static.flow.applyInputCall[1],"source-call",0);register_exec(f.UpdatePlayerInputs,"update-target",0);register_exec(config.static.flow.applyInputCall[2],"source-return",0)
end

status("milestone:observer-loaded")
local ok,message=pcall(function()
  register_exec(f.CheckSram,"bootstrap-entry",0)
end)
if not ok then fail_callback(message) end
local frames=0
while true do
  local loop_ok,loop_message=pcall(function()
    frames=frames+1;if bootstrapped then arm_step() elseif not bootstrap_armed then joypad.set({Start=true},1);joypad.set({},2) end;emu.frameadvance()
    if frames%600==0 then status("frame="..frames..",pc="..string.format("%X",emu.getregister("M68K PC"))..",step="..step_index) end
  end)
  if not loop_ok then fail_callback(loop_message) end
end
