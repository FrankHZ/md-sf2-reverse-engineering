local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local bootstrap=assert(dofile(config.bootstrapLibraryPath))
local f,i=config["function"],config.instrumentation
local case_index,active,observer_failed,session_cleaned=1,false,false,false
local records,event_ids,registered_addresses={},{},{}
local current_phase,current_role,current_pc="registration","registration",nil
local current_expectation,seed_copy_at_helper_return=nil,nil
local entry_seen,return_seen,instruction_target,effective_target,source_copy_seen=false,false,false,false,false
local generator_outputs,generator_states,return_path={},{},nil
local host_redirected,prompt_count,frame_count=false,0,0
local stage,queue="cheat",{}
local names={ [1]="Up",[2]="Down",[4]="Left",[8]="Right",[16]="B",[32]="C" }
local cheat={ 1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4 }

local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
status("milestone:observer-loaded")
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local buttons={};if name and name~="" then buttons[name]=true end;joypad.set(buttons,1) end
local function current_case() return config.cases[case_index] end
local function nullable(value) if value==nil then return "null" end return tostring(value) end
local function bool(value) if value then return "true" end return "false" end
local function json_string(value) return string.format("%q",value) end
local function json_numbers(values) local out={};for _,value in ipairs(values) do out[#out+1]=tostring(value) end;return "["..table.concat(out,",").."]" end
local function unregister_events() for i=#event_ids,1,-1 do event.unregisterbyid(event_ids[i]);event_ids[i]=nil end end
local function cleanup_session() if session_cleaned then return end;session_cleaned=true;unregister_events() end
local function pending_callback_state()
  return "{\"active\":"..bool(active)..",\"caseIndex\":"..case_index..",\"generatorCallCount\":"..#generator_outputs..",\"entrySeen\":"..bool(entry_seen)..",\"returnSeen\":"..bool(return_seen)..",\"instructionTargetObserved\":"..bool(instruction_target)..",\"effectiveTargetObserved\":"..bool(effective_target)..",\"sourceCopyWriteSeen\":"..bool(source_copy_seen).."}"
end
local function callback_case()
  if active or current_phase=="case-entry" then return current_case() end
  return nil
end
local function expectation_for(phase)
  local static=config.callbackExpectations.static[phase]
  if static then return static end
  local c=callback_case();if not c then return nil end
  return config.callbackExpectations.cases[case_index][phase]
end
local function helper_instruction_target(c)
  if c.service=="base" then return f.baseEntryAddress end
  if c.service=="unsigned-bounded" then return f.unsignedBoundedEntryAddress end
  return f.thinkingAliasEntryAddress
end
local function helper_effective_target(c)
  if c.service=="thinking-bounded" then return f.thinkingBoundedEntryAddress end
  return helper_instruction_target(c)
end
local function helper_return_pc(c)
  if c.service=="base" then return f.baseReturnAddress end
  if c.service=="unsigned-bounded" then return c.expected.returnPath=="early" and f.unsignedEarlyReturnAddress or f.unsignedNormalReturnAddress end
  return c.expected.returnPath=="early" and f.thinkingEarlyReturnAddress or f.thinkingNormalReturnAddress
end
local function fail_callback(message)
  if observer_failed then return end
  observer_failed=true
  local c=callback_case();local e=current_expectation or {}
  local payload="{\"caseId\":"..(c and json_string(c.id) or "null")..",\"phase\":"..json_string(current_phase)..",\"role\":"..json_string(current_role)..",\"actualPc\":"..nullable(emu.getregister("M68K PC"))..",\"expectedEventPc\":"..nullable(e.expectedEventPc)..",\"expectedCallPc\":"..nullable(e.expectedCallPc)..",\"expectedTargetPc\":"..nullable(e.expectedTargetPc)..",\"expectedReturnPc\":"..nullable(e.expectedReturnPc)..",\"pendingCallback\":"..pending_callback_state()..",\"error\":"..json_string(tostring(message)).."}"
  local diagnostic="failure:observer-callback:"..payload
  status(diagnostic);print(diagnostic);os.remove(config.outputPath);cleanup_session();client.exitCode(1)
end
local function set_role(role) current_role=role end
local function require_equal(actual,expected,label) if actual~=expected then error(label..": expected="..tostring(expected)..", actual="..tostring(actual)) end end
local function require_expectation(call_pc,target_pc,return_pc,label)
  require_equal(current_expectation.expectedCallPc,call_pc,label.." call PC")
  require_equal(current_expectation.expectedTargetPc,target_pc,label.." target PC")
  require_equal(current_expectation.expectedReturnPc,return_pc,label.." return PC")
end
local function register_exec(address,phase,callback)
  if registered_addresses[address] then error("random-services duplicate physical-PC callback: "..address) end
  registered_addresses[address]=phase
  event_ids[#event_ids+1]=event.on_bus_exec(function()
    if observer_failed then return end
    local ok,message=pcall(function()
      current_phase=phase;current_role="unresolved:"..phase;current_pc=address
      if not config.callbackExpectations.static[phase] and not active and phase~="case-entry" then
        current_role="inactive:"..phase
        return
      end
      current_expectation=expectation_for(phase)
      if not current_expectation then error("missing callback expectation for "..phase) end
      current_role=current_expectation.role
      require_equal(current_expectation.phase,phase,"callback expectation phase")
      require_equal(current_expectation.expectedEventPc,address,"callback expectation event PC")
      require_equal(current_expectation.allowed,true,"callback phase allowed")
      callback()
    end)
    if not ok then fail_callback(message) end
  end,address,"random-services-"..phase,"M68K BUS")
end
local function reset_case()
  entry_seen,return_seen,instruction_target,effective_target,source_copy_seen=false,false,false,false,false
  generator_outputs,generator_states,return_path={}, {}, nil;seed_copy_at_helper_return=nil
end
local function begin_case()
  set_role("case-entry")
  if not host_redirected then error("probe entered before post-start host redirect") end
  if active then error("nested case entry") end
  local c=current_case();if not c then error("case table exhausted") end
  reset_case();local scratch=config.instrumentation.scratchRamBase
  memory.write_u16_be(scratch,c.rangeWord,"M68K BUS")
  local target=helper_instruction_target(c)
  require_expectation(i.helperCallPc,target,helper_return_pc(c),"case entry")
  memory.write_u32_be(scratch+2,target,"M68K BUS")
  memory.write_u16_be(config.ram.randomSeedAddress,c.randomSeedBefore,"M68K BUS")
  memory.write_u16_be(config.ram.randomSeedCopyAddress,c.seedCopyBefore,"M68K BUS")
  active=true;status("milestone:probe-entered")
end
local function helper_entry(service,alias)
  local c=current_case();if not active or not c then error("helper entry without active case") end
  require_equal(c.service,service,"helper service")
  if alias then
    require_expectation(i.helperCallPc,helper_instruction_target(c),helper_return_pc(c),"alias entry")
    instruction_target=true
  else
    require_expectation(i.helperCallPc,helper_effective_target(c),helper_return_pc(c),"effective entry")
    effective_target=true;entry_seen=true;if service~="thinking-bounded" then instruction_target=true end
  end
end
local function generator_entry(service)
  local c=current_case();if not active or c.service~=service then error("generator service mismatch") end
  set_role(service.."-generator-entry")
  if service=="unsigned-bounded" then
    require_expectation(f.unsignedGeneratorCallAddress,f.unsignedGeneratorEntryAddress,f.unsignedGeneratorReturnToCallerAddress,"unsigned generator entry")
  else
    require_expectation(f.thinkingGeneratorCallAddress,f.thinkingGeneratorEntryAddress,f.thinkingGeneratorReturnToCallerAddress,"thinking generator entry")
  end
end
local function generator_return(service)
  local c=current_case();if not active or c.service~=service then error("generator return mismatch") end
  set_role(service.."-generator-return")
  if service=="unsigned-bounded" then
    require_expectation(f.unsignedGeneratorCallAddress,f.unsignedGeneratorEntryAddress,f.unsignedGeneratorReturnToCallerAddress,"unsigned generator return")
  else
    require_expectation(f.thinkingGeneratorCallAddress,f.thinkingGeneratorEntryAddress,f.thinkingGeneratorReturnToCallerAddress,"thinking generator return")
  end
  generator_outputs[#generator_outputs+1]=emu.getregister("M68K D7")&0xFF
  generator_states[#generator_states+1]=memory.read_u16_be(config.ram.randomSeedCopyAddress,"M68K BUS")
end
local function helper_return(service,path)
  local c=current_case();if not active or c.service~=service then error("helper return mismatch") end
  set_role(service.."-"..path.."-return");return_seen=true;return_path=path
  require_expectation(i.helperCallPc,helper_effective_target(c),helper_return_pc(c),"helper return")
end
local function base_return()
  local c=current_case();if not active or c.service~="base" then error("base return mismatch") end
  set_role("base-return");instruction_target=true;effective_target=true;entry_seen=true;return_seen=true;return_path="base"
  require_expectation(i.helperCallPc,helper_effective_target(c),helper_return_pc(c),"base return")
end
local function copy_write()
  set_role("source-shaped-copy-write")
  if not active then error("source-shaped copy write without active case") end
  local c=current_case();require_expectation(i.helperCallPc,helper_effective_target(c),helper_return_pc(c),"source-shaped copy")
  seed_copy_at_helper_return=memory.read_u16_be(config.ram.randomSeedCopyAddress,"M68K BUS")
  source_copy_seen=true
end
local function write_probe()
  local hex=i.probeStubHex
  for offset=0,#hex/2-1 do
    local expected=tonumber(hex:sub(offset*2+1,offset*2+2),16)
    local address=i.workRamProbePc+offset
    memory.write_u8(address,expected,"M68K BUS")
    require_equal(memory.read_u8(address,"M68K BUS"),expected,"work-RAM probe byte")
  end
end
local function redirect_host_return()
  set_role("host-turn-order-return-redirect")
  if host_redirected then return end
  require_expectation(nil,nil,i.workRamProbePc,"host turn-order redirect")
  local stack=emu.getregister("M68K A7")&0xFFFFFF
  if stack<0xFF0000 or stack>0xFFFFFF then error("host return stack outside work RAM: "..stack) end
  write_probe();memory.write_u32_be(stack,i.workRamProbePc,"M68K BUS")
  require_equal(memory.read_u32_be(stack,"M68K BUS"),i.workRamProbePc,"host return target")
  host_redirected=true;stage="host-redirected";status("milestone:host-turn-order-redirect")
end
local function host_battle_test()
  set_role("host-battle-test");require_expectation(nil,nil,nil,"host Battle Test")
  stage="ui";status("milestone:host-battle-test")
end
local function host_number_prompt()
  set_role("host-number-prompt");require_expectation(nil,nil,nil,"host number prompt")
  prompt_count=prompt_count+1;status("milestone:host-number-prompt:"..prompt_count)
  bootstrap.battle01_intro_skip(config.bootstrap.profile,prompt_count,pulse)
end
local function host_flag_prompt() set_role("host-flag-prompt");require_expectation(nil,nil,nil,"host flag prompt");pulse("B");status("milestone:host-flag-prompt") end
local function finish_case()
  set_role("case-result")
  local c=current_case();if not active or not c then error("case result without active case") end
  require_expectation(i.helperCallPc,helper_effective_target(c),helper_return_pc(c),"case result")
  if c.service=="thinking-bounded" then require_equal(instruction_target,true,"thinking alias target");require_equal(effective_target,true,"thinking effective target")
  else require_equal(instruction_target,true,"instruction target");require_equal(effective_target,true,"effective target") end
  require_equal(entry_seen,true,"entry seam");require_equal(return_seen,true,"return seam");require_equal(source_copy_seen,true,"source-shaped write seam");if seed_copy_at_helper_return==nil then error("missing helper-return seed-copy state") end
  records[#records+1]={id=c.id,randomSeedAfter=memory.read_u16_be(config.ram.randomSeedAddress,"M68K BUS"),seedCopyAtHelperReturn=seed_copy_at_helper_return,seedCopyAfterSourceCopy=memory.read_u16_be(config.ram.randomSeedCopyAddress,"M68K BUS"),resultLowByte=emu.getregister("M68K D7")&0xFF,generatorCallCount=#generator_outputs,generatorOutputs=generator_outputs,generatorStates=generator_states,returnPath=return_path,instructionTargetObserved=instruction_target,effectiveTargetObserved=effective_target,sourceCopyWriteSeen=source_copy_seen}
  active=false;case_index=case_index+1
  if case_index>#config.cases then
    cleanup_session();if #event_ids~=0 then error("residual registered callback") end
    status("milestone:callbacks-cleared:0")
    local f=assert(io.open(config.outputPath,"w"));f:write("{\"system\":"..json_string(emu.getsystemid())..",\"core\":"..json_string(config.core)..",\"id\":"..json_string(config.id)..",\"caseOrder\":[")
    for i,cse in ipairs(config.cases) do if i>1 then f:write(",") end;f:write(json_string(cse.id)) end
    f:write("],\"records\":[")
    for i,r in ipairs(records) do
      if i>1 then f:write(",") end
      f:write("{\"id\":"..json_string(r.id)..",\"randomSeedAfter\":"..r.randomSeedAfter..",\"seedCopyAtHelperReturn\":"..r.seedCopyAtHelperReturn..",\"seedCopyAfterSourceCopy\":"..r.seedCopyAfterSourceCopy..",\"resultLowByte\":"..r.resultLowByte..",\"generatorCallCount\":"..r.generatorCallCount..",\"generatorOutputs\":"..json_numbers(r.generatorOutputs)..",\"generatorStates\":"..json_numbers(r.generatorStates)..",\"returnPath\":"..json_string(r.returnPath)..",\"instructionTargetObserved\":"..bool(r.instructionTargetObserved)..",\"effectiveTargetObserved\":"..bool(r.effectiveTargetObserved)..",\"sourceCopyWriteSeen\":"..bool(r.sourceCopyWriteSeen).."}")
    end
    f:write("]}");f:close();status("milestone:observer-finished");client.exitCode(0)
  end
end

register_exec(i.battleTestEntryPc,"host-battle-test",host_battle_test)
register_exec(i.numberPromptEntryPc,"host-number-prompt",host_number_prompt)
register_exec(i.flagPromptEntryPc,"host-flag-prompt",host_flag_prompt)
register_exec(i.turnOrderEntryPc,"host-turn-order",redirect_host_return)
register_exec(i.caseEntryPc,"case-entry",begin_case)
register_exec(f.baseEntryAddress,"base-entry",function() if active then helper_entry("base",false) end end)
register_exec(f.baseReturnAddress,"base-return",function() if active then base_return() end end)
register_exec(f.unsignedBoundedEntryAddress,"unsigned-entry",function() if active then helper_entry("unsigned-bounded",false) end end)
register_exec(f.unsignedGeneratorEntryAddress,"unsigned-generator-entry",function() if active then generator_entry("unsigned-bounded") end end)
register_exec(f.unsignedGeneratorReturnAddress,"unsigned-generator-return",function() if active then generator_return("unsigned-bounded") end end)
register_exec(f.unsignedNormalReturnAddress,"unsigned-normal-return",function() if active then helper_return("unsigned-bounded","normal") end end)
register_exec(f.unsignedEarlyReturnAddress,"unsigned-early-return",function() if active then helper_return("unsigned-bounded","early") end end)
register_exec(f.thinkingAliasEntryAddress,"thinking-alias",function() if active then helper_entry("thinking-bounded",true) end end)
register_exec(f.thinkingBoundedEntryAddress,"thinking-entry",function() if active then helper_entry("thinking-bounded",false) end end)
register_exec(f.thinkingGeneratorEntryAddress,"thinking-generator-entry",function() if active then generator_entry("thinking-bounded") end end)
register_exec(f.thinkingGeneratorReturnAddress,"thinking-generator-return",function() if active then generator_return("thinking-bounded") end end)
register_exec(f.thinkingNormalReturnAddress,"thinking-normal-return",function() if active then helper_return("thinking-bounded","normal") end end)
register_exec(f.thinkingEarlyReturnAddress,"thinking-early-return",function() if active then helper_return("thinking-bounded","early") end end)
register_exec(i.sourceCopyWritePc,"source-shaped-copy-write",function() if active then copy_write() end end)
register_exec(i.resultPc,"case-result",function() if active then finish_case() end end)
while true do
  frame_count=frame_count+1
  local button=nil
  if stage=="cheat" then
    local pointer=memory.read_u32_be(0xFFB1A0,"M68K BUS")
    if pointer>=0x28FF0 and pointer<0x29000 then button=names[cheat[pointer-0x28FF0+1]]
    elseif memory.read_u8(0xFFB0A9,"M68K BUS")==255 then button="Up" end
  elseif #queue>0 then button=table.remove(queue,1)
  elseif stage=="ui" and memory.read_u8(0xFFF712,"M68K BUS")==1 then button="C" end
  set_button(button)
  joypad.set({Start=stage=="ui" and memory.read_u8(0xFFF712,"M68K BUS")==1},2)
  emu.frameadvance()
  if frame_count%600==0 then status("frame="..frame_count..",stage="..stage..",cases="..(case_index-1)) end
end
