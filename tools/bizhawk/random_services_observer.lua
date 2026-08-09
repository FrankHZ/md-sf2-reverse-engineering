local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local bootstrap=assert(dofile(config.bootstrapLibraryPath))
local f,i=config["function"],config.instrumentation
local case_index,active,observer_failed,session_cleaned=1,false,false,false
local records,event_ids,registered_addresses={},{},{}
local current_phase,current_role,current_pc="registration","registration",nil
local current_expectation,seed_copy_at_helper_return,seed_copy_after_source_copy=nil,nil,nil
local entry_seen,return_seen,instruction_target,effective_target,source_copy_seen=false,false,false,false,false
local caller_preamble_seen,caller_range_seen,caller_rng_call_seen,caller_call_seen,caller_store_seen=false,false,false,false,false
local caller_restore_seen,caller_wait_call_seen,caller_wait_target_seen,caller_wait_rts_seen=false,false,false,false
local caller_continuation_pending,caller_continuation_seen,caller_helper_return_redirect_seen=false,false,false
local caller_saved_d6,caller_saved_d7=nil,nil
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
  return "{\"active\":"..bool(active)..",\"caseIndex\":"..case_index..",\"generatorCallCount\":"..#generator_outputs..",\"entrySeen\":"..bool(entry_seen)..",\"returnSeen\":"..bool(return_seen)..",\"instructionTargetObserved\":"..bool(instruction_target)..",\"effectiveTargetObserved\":"..bool(effective_target)..",\"sourceCopyWriteSeen\":"..bool(source_copy_seen)..",\"callerPreambleSeen\":"..bool(caller_preamble_seen)..",\"callerRangeSeen\":"..bool(caller_range_seen)..",\"callerRngCallSeen\":"..bool(caller_rng_call_seen)..",\"callerCallSeen\":"..bool(caller_call_seen)..",\"callerStoreSeen\":"..bool(caller_store_seen)..",\"callerRestoreSeen\":"..bool(caller_restore_seen)..",\"callerWaitCallSeen\":"..bool(caller_wait_call_seen)..",\"callerWaitTargetSeen\":"..bool(caller_wait_target_seen)..",\"callerWaitRtsSeen\":"..bool(caller_wait_rts_seen)..",\"callerContinuationPending\":"..bool(caller_continuation_pending)..",\"callerContinuationSeen\":"..bool(caller_continuation_seen)..",\"callerHelperReturnRedirectSeen\":"..bool(caller_helper_return_redirect_seen).."}"
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
local function helper_return_pc()
  local expected=current_expectation
  if not expected then error("missing current callback expectation") end
  return expected.expectedReturnPc
end
local function fail_callback(message)
  if observer_failed then return end
  observer_failed=true
  local c=callback_case();local e=current_expectation or {}
  local payload="{\"owner\":"..json_string(config.observerFailureContract.owner)..",\"caseId\":"..(c and json_string(c.id) or "null")..",\"phase\":"..json_string(current_phase)..",\"role\":"..json_string(current_role)..",\"actualPc\":"..nullable(emu.getregister("M68K PC"))..",\"expectedEventPc\":"..nullable(e.expectedEventPc)..",\"expectedCallPc\":"..nullable(e.expectedCallPc)..",\"expectedTargetPc\":"..nullable(e.expectedTargetPc)..",\"expectedReturnPc\":"..nullable(e.expectedReturnPc)..",\"pendingCallback\":"..pending_callback_state()..",\"error\":"..json_string(tostring(message)).."}"
  local diagnostic=config.observerFailureContract.statusPrefix..payload
  status(diagnostic);print(diagnostic)
  if config.observerFailureContract.removeOutputBeforeExit then os.remove(config.outputPath) end
  cleanup_session();client.exitCode(config.observerFailureContract.exitCode)
end
local function set_role(role) current_role=role end
local function require_equal(actual,expected,label) if actual~=expected then error(label..": expected="..tostring(expected)..", actual="..tostring(actual)) end end
local function require_expectation(call_pc,target_pc,return_pc,label)
  require_equal(current_expectation.expectedCallPc,call_pc,label.." call PC")
  require_equal(current_expectation.expectedTargetPc,target_pc,label.." target PC")
  require_equal(current_expectation.expectedReturnPc,return_pc,label.." return PC")
end
local function dispatched_phase(phase)
  if phase=="case-entry" and active and caller_continuation_pending then return "caller-continuation" end
  return phase
end
local function register_exec(address,phase,callback,should_dispatch)
  if registered_addresses[address] then error("random-services duplicate physical-PC callback: "..address) end
  registered_addresses[address]=phase
  event_ids[#event_ids+1]=event.on_bus_exec(function()
    if observer_failed then return end
    local ok,message=pcall(function()
      if should_dispatch and not should_dispatch() then return end
      local phase_now=dispatched_phase(phase)
      current_phase=phase_now;current_role="unresolved:"..phase_now;current_pc=address
      if not config.callbackExpectations.static[phase_now] and not active and phase_now~="case-entry" then
        current_role="inactive:"..phase_now
        return
      end
      current_expectation=expectation_for(phase_now)
      if not current_expectation then error("missing callback expectation for "..phase_now) end
      current_role=current_expectation.role
      require_equal(current_expectation.phase,phase_now,"callback expectation phase")
      require_equal(current_expectation.expectedEventPc,address,"callback expectation event PC")
      require_equal(current_expectation.allowed,true,"callback phase allowed")
      callback()
    end)
    if not ok then fail_callback(message) end
  end,address,"random-services-"..phase,"M68K BUS")
end
local function reset_case()
  entry_seen,return_seen,instruction_target,effective_target,source_copy_seen=false,false,false,false,false
  caller_preamble_seen,caller_range_seen,caller_rng_call_seen,caller_call_seen,caller_store_seen=false,false,false,false,false
  caller_restore_seen,caller_wait_call_seen,caller_wait_target_seen,caller_wait_rts_seen=false,false,false,false
  caller_continuation_pending,caller_continuation_seen,caller_helper_return_redirect_seen=false,false,false
  caller_saved_d6,caller_saved_d7=nil,nil
  generator_outputs,generator_states,return_path={}, {}, nil;seed_copy_at_helper_return=nil;seed_copy_after_source_copy=nil
end
local function caller_continuation()
  local c=current_case();if not active or not c or not c.callerExecutionObserved then error("unexpected caller continuation") end
  set_role("caller-continuation")
  require_equal(caller_continuation_pending,true,"caller continuation without rewritten WaitForVInt return")
  require_equal(caller_wait_rts_seen,true,"caller continuation before WaitForVInt RTS")
  require_equal(memory.read_u16_be(i.scratchRamBase,"M68K BUS"),c.rangeWord,"caller continuation range input")
  require_equal(memory.read_u32_be(i.scratchRamBase+2,"M68K BUS"),f.thinkingAliasEntryAddress,"caller continuation thinking target")
  caller_continuation_pending=false;caller_continuation_seen=true
end
local function begin_case()
  set_role("case-entry")
  if not host_redirected then error("probe entered before post-start host redirect") end
  if active then
    if caller_continuation_pending then return caller_continuation() end
    error("nested case entry")
  end
  local c=current_case();if not c then error("case table exhausted") end
  reset_case();local scratch=config.instrumentation.scratchRamBase
  memory.write_u16_be(config.ram.randomSeedAddress,c.randomSeedBefore,"M68K BUS")
  memory.write_u16_be(config.ram.randomSeedCopyAddress,c.seedCopyBefore,"M68K BUS")
  require_equal(memory.read_u16_be(config.ram.randomSeedAddress,"M68K BUS"),c.randomSeedBefore,"case random seed isolation")
  require_equal(memory.read_u16_be(config.ram.randomSeedCopyAddress,"M68K BUS"),c.seedCopyBefore,"case seed-copy isolation")
  if c.callerExecutionObserved then
    local prefix=c.context=="text-symbol-wait-caller-seam" and "textWait" or "diamond"
    memory.write_u16_be(scratch,config.sourceContexts[prefix.."RangeWord"],"M68K BUS")
    memory.write_u32_be(scratch+2,config.sourceContexts[prefix.."PreamblePc"],"M68K BUS")
    require_equal(memory.read_u16_be(scratch,"M68K BUS"),config.sourceContexts[prefix.."RangeWord"],"caller preamble range setup")
    require_equal(memory.read_u32_be(scratch+2,"M68K BUS"),config.sourceContexts[prefix.."PreamblePc"],"caller preamble target setup")
  else
    memory.write_u16_be(scratch,c.rangeWord,"M68K BUS")
    local target=helper_instruction_target(c)
    require_expectation(i.helperCallPc,target,helper_return_pc(),"case entry")
    memory.write_u32_be(scratch+2,target,"M68K BUS")
  end
  active=true;status("milestone:probe-entered")
end
local function helper_entry(service,alias)
  local c=current_case();if not active or not c then error("helper entry without active case") end
  require_equal(c.service,service,"helper service")
  if alias then
    require_expectation(current_expectation.expectedCallPc,helper_instruction_target(c),helper_return_pc(),"alias entry")
    instruction_target=true
  else
    require_expectation(current_expectation.expectedCallPc,helper_effective_target(c),helper_return_pc(),"effective entry")
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
  require_expectation(current_expectation.expectedCallPc,helper_effective_target(c),helper_return_pc(),"helper return")
  if c.callerExecutionObserved then
    require_equal(caller_continuation_seen,true,"caller helper return before continuation")
    local stack=emu.getregister("M68K A7")&0xFFFFFF
    if stack<0xFF0000 or stack>0xFFFFFF then error("caller helper return stack outside work RAM: "..stack) end
    require_equal(memory.read_u32_be(stack,"M68K BUS"),i.sourceCopyWritePc,"caller helper original probe return")
    seed_copy_at_helper_return=memory.read_u16_be(config.ram.randomSeedCopyAddress,"M68K BUS")
    memory.write_u32_be(stack,i.resultPc,"M68K BUS")
    require_equal(memory.read_u32_be(stack,"M68K BUS"),i.resultPc,"caller helper result return redirect")
    caller_helper_return_redirect_seen=true
  end
end
local function base_return()
  local c=current_case();if not active or c.service~="base" then error("base return mismatch") end
  set_role("base-return");instruction_target=true;effective_target=true;entry_seen=true;return_seen=true;return_path="base"
  require_expectation(current_expectation.expectedCallPc,helper_effective_target(c),helper_return_pc(),"base return")
end
local function copy_write()
  set_role("source-shaped-copy-write")
  if not active then error("source-shaped copy write without active case") end
  local c=current_case();if c.callerExecutionObserved then error("caller reached controlled probe copy") end
  require_expectation(current_expectation.expectedCallPc,helper_effective_target(c),helper_return_pc(),"source-shaped copy")
  seed_copy_at_helper_return=memory.read_u16_be(config.ram.randomSeedCopyAddress,"M68K BUS")
  source_copy_seen=true
end
local function caller_prefix(c)
  if c.context=="text-symbol-wait-caller-seam" then return "textWait" end
  if c.context=="diamond-menu-caller-seam" then return "diamond" end
  error("caller seam has no source context")
end
local function caller_preamble()
  local c=current_case();if not active or not c or not c.callerExecutionObserved then error("unexpected caller preamble") end
  set_role("caller-preamble");local prefix=caller_prefix(c)
  require_expectation(i.helperCallPc,config.sourceContexts[prefix.."PreamblePc"],i.sourceCopyWritePc,"caller preamble")
  local stack=emu.getregister("M68K A7")&0xFFFFFF
  if stack<0xFF0000 or stack>0xFFFFFF then error("caller preamble stack outside work RAM: "..stack) end
  require_equal(memory.read_u32_be(stack,"M68K BUS"),i.sourceCopyWritePc,"caller preamble probe return")
  caller_saved_d6=emu.getregister("M68K D6")&0xFFFF;caller_saved_d7=emu.getregister("M68K D7")&0xFFFF
  caller_preamble_seen=true
end
local function caller_range_load()
  local c=current_case();if not active or not c or not c.callerExecutionObserved then error("unexpected caller range load") end
  set_role("caller-range-load");local prefix=caller_prefix(c)
  require_equal(caller_preamble_seen,true,"caller range without preamble")
  require_expectation(i.helperCallPc,config.sourceContexts[prefix.."PreamblePc"],i.sourceCopyWritePc,"caller range load")
  require_equal(caller_saved_d6,config.sourceContexts[prefix.."RangeWord"],"caller preamble D6 before source range")
  caller_range_seen=true
end
local function caller_rng_call()
  local c=current_case();if not active or not c or not c.callerExecutionObserved then error("unexpected caller RNG call") end
  set_role("caller-rng-call");local prefix=caller_prefix(c)
  require_equal(caller_preamble_seen,true,"caller RNG call without preamble");require_equal(caller_range_seen,true,"caller RNG call without range")
  require_expectation(config.sourceContexts[prefix.."CallPc"],f.baseEntryAddress,config.sourceContexts[prefix.."StorePc"],"caller RNG call")
  require_equal(emu.getregister("M68K D6")&0xFFFF,config.sourceContexts[prefix.."RangeWord"],"caller source range load")
  caller_rng_call_seen=true
end
local function caller_store()
  local c=current_case();if not active or not c or not c.callerExecutionObserved then error("unexpected caller store") end
  set_role("caller-seed-copy-store");local prefix=caller_prefix(c)
  require_equal(caller_rng_call_seen,true,"caller store without RNG call");require_equal(caller_call_seen,true,"caller store without caller call")
  require_expectation(config.sourceContexts[prefix.."CallPc"],f.baseEntryAddress,config.sourceContexts[prefix.."StorePc"],"caller source store")
  caller_store_seen=true
end
local function caller_post_store()
  local c=current_case();if not active or not c or not c.callerExecutionObserved then error("unexpected caller post-store") end
  set_role("caller-post-store-restore");local prefix=caller_prefix(c)
  require_equal(caller_call_seen,true,"caller post-store without caller call");require_equal(caller_store_seen,true,"caller post-store without caller store")
  require_expectation(config.sourceContexts[prefix.."CallPc"],f.baseEntryAddress,config.sourceContexts[prefix.."StorePc"],"caller post-store")
  source_copy_seen=true
  seed_copy_after_source_copy=memory.read_u16_be(config.ram.randomSeedCopyAddress,"M68K BUS")
  if seed_copy_after_source_copy==c.seedCopyBefore then error("caller source store did not update seed-copy base byte") end
  caller_restore_seen=true
end
local function caller_wait_call()
  local c=current_case();if not active or not c or not c.callerExecutionObserved then error("unexpected caller WaitForVInt call") end
  set_role("caller-wait-call");local prefix=caller_prefix(c)
  require_equal(caller_restore_seen,true,"caller WaitForVInt call without restore")
  if caller_wait_call_seen then error("duplicate caller WaitForVInt call") end
  require_expectation(config.sourceContexts[prefix.."VIntCallPc"],config.sourceContexts.waitForVIntEntryPc,config.sourceContexts[prefix.."VIntReturnPc"],"caller WaitForVInt call")
  require_equal(emu.getregister("M68K D6")&0xFFFF,caller_saved_d6,"caller D6 restore")
  require_equal(emu.getregister("M68K D7")&0xFFFF,caller_saved_d7,"caller D7 restore")
  caller_wait_call_seen=true
end
local function caller_wait_target()
  local c=current_case();if not active or not c or not c.callerExecutionObserved then error("unexpected WaitForVInt target") end
  set_role("wait-for-vint-target");local prefix=caller_prefix(c)
  require_equal(caller_wait_call_seen,true,"WaitForVInt target without source call")
  if caller_wait_target_seen then error("duplicate WaitForVInt target") end
  require_expectation(config.sourceContexts[prefix.."VIntCallPc"],config.sourceContexts.waitForVIntEntryPc,config.sourceContexts[prefix.."VIntReturnPc"],"WaitForVInt target")
  local stack=emu.getregister("M68K A7")&0xFFFFFF
  if stack<0xFF0000 or stack>0xFFFFFF then error("WaitForVInt source return stack outside work RAM: "..stack) end
  require_equal(memory.read_u32_be(stack,"M68K BUS"),config.sourceContexts[prefix.."VIntReturnPc"],"WaitForVInt source stack return")
  memory.write_u32_be(stack,i.callerContinuationPc,"M68K BUS")
  require_equal(memory.read_u32_be(stack,"M68K BUS"),i.callerContinuationPc,"WaitForVInt continuation return")
  memory.write_u16_be(i.scratchRamBase,c.rangeWord,"M68K BUS")
  memory.write_u32_be(i.scratchRamBase+2,f.thinkingAliasEntryAddress,"M68K BUS")
  require_equal(memory.read_u16_be(i.scratchRamBase,"M68K BUS"),c.rangeWord,"WaitForVInt continuation range setup")
  require_equal(memory.read_u32_be(i.scratchRamBase+2,"M68K BUS"),f.thinkingAliasEntryAddress,"WaitForVInt continuation target setup")
  caller_wait_target_seen=true;caller_continuation_pending=true
end
local function caller_wait_rts()
  local c=current_case();if not active or not c or not c.callerExecutionObserved then error("unexpected WaitForVInt RTS") end
  set_role("wait-for-vint-rts")
  require_equal(caller_wait_target_seen,true,"WaitForVInt RTS without target")
  if caller_wait_rts_seen then error("duplicate WaitForVInt RTS") end
  require_expectation(current_expectation.expectedCallPc,current_expectation.expectedTargetPc,i.callerContinuationPc,"WaitForVInt RTS")
  local stack=emu.getregister("M68K A7")&0xFFFFFF
  if stack<0xFF0000 or stack>0xFFFFFF then error("WaitForVInt continuation stack outside work RAM: "..stack) end
  require_equal(memory.read_u32_be(stack,"M68K BUS"),i.callerContinuationPc,"WaitForVInt rewritten stack return")
  caller_wait_rts_seen=true
end
local function caller_base_entry()
  local c=current_case();if not active or not c or not c.callerExecutionObserved then error("unexpected caller base entry") end
  set_role("caller-base-effective-target");local prefix=caller_prefix(c)
  require_equal(caller_rng_call_seen,true,"caller base entry without RNG call")
  require_expectation(config.sourceContexts[prefix.."CallPc"],f.baseEntryAddress,config.sourceContexts[prefix.."StorePc"],"caller base entry")
  local stack=emu.getregister("M68K A7")&0xFFFFFF
  if stack<0xFF0000 or stack>0xFFFFFF then error("caller source return stack outside work RAM: "..stack) end
  require_equal(memory.read_u32_be(stack,"M68K BUS"),config.sourceContexts[prefix.."StorePc"],"caller source stack return")
  caller_call_seen=true
end
local function caller_base_return()
  local c=current_case();if not active or not c or not c.callerExecutionObserved then error("unexpected caller base return") end
  set_role("caller-base-return");local prefix=caller_prefix(c)
  require_equal(caller_call_seen,true,"caller base return without source call")
  require_expectation(config.sourceContexts[prefix.."CallPc"],f.baseEntryAddress,config.sourceContexts[prefix.."StorePc"],"caller base return")
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
  require_expectation(current_expectation.expectedCallPc,helper_effective_target(c),helper_return_pc(),"case result")
  if c.service=="thinking-bounded" then require_equal(instruction_target,true,"thinking alias target");require_equal(effective_target,true,"thinking effective target")
  else require_equal(instruction_target,true,"instruction target");require_equal(effective_target,true,"effective target") end
  require_equal(entry_seen,true,"entry seam");require_equal(return_seen,true,"return seam");require_equal(source_copy_seen,true,"source copy write seam");if seed_copy_at_helper_return==nil then error("missing helper-return seed-copy state") end
  if c.callerExecutionObserved then
    require_equal(caller_preamble_seen,true,"caller source preamble seam");require_equal(caller_range_seen,true,"caller source range seam");require_equal(caller_rng_call_seen,true,"caller source RNG call seam");require_equal(caller_call_seen,true,"caller source base-entry stack seam");require_equal(caller_store_seen,true,"caller source store seam");require_equal(caller_restore_seen,true,"caller source restore seam");require_equal(caller_wait_call_seen,true,"caller source WaitForVInt call seam");require_equal(caller_wait_target_seen,true,"caller WaitForVInt target seam");require_equal(caller_wait_rts_seen,true,"caller WaitForVInt RTS seam");require_equal(caller_continuation_seen,true,"caller continuation seam");require_equal(caller_helper_return_redirect_seen,true,"caller helper return redirect seam")
  else
    require_equal(caller_preamble_seen,false,"unexpected caller preamble");require_equal(caller_range_seen,false,"unexpected caller range");require_equal(caller_rng_call_seen,false,"unexpected caller RNG call");require_equal(caller_call_seen,false,"unexpected caller source call");require_equal(caller_store_seen,false,"unexpected caller source store");require_equal(caller_restore_seen,false,"unexpected caller restore");require_equal(caller_wait_call_seen,false,"unexpected caller WaitForVInt call");require_equal(caller_wait_target_seen,false,"unexpected caller WaitForVInt target");require_equal(caller_wait_rts_seen,false,"unexpected caller WaitForVInt RTS");require_equal(caller_continuation_seen,false,"unexpected caller continuation");require_equal(caller_helper_return_redirect_seen,false,"unexpected caller helper return redirect")
  end
  local source_copy_after=seed_copy_after_source_copy or memory.read_u16_be(config.ram.randomSeedCopyAddress,"M68K BUS")
  records[#records+1]={id=c.id,randomSeedAfter=memory.read_u16_be(config.ram.randomSeedAddress,"M68K BUS"),seedCopyAtHelperReturn=seed_copy_at_helper_return,seedCopyAfterSourceCopy=source_copy_after,resultLowByte=emu.getregister("M68K D7")&0xFF,generatorCallCount=#generator_outputs,generatorOutputs=generator_outputs,generatorStates=generator_states,returnPath=return_path,instructionTargetObserved=instruction_target,effectiveTargetObserved=effective_target,sourceCopyWriteSeen=source_copy_seen,callerExecutionObserved=c.callerExecutionObserved,callerPreambleSeen=caller_preamble_seen,callerRangeSeen=caller_range_seen,callerRngCallSeen=caller_rng_call_seen,callerCallSeen=caller_call_seen,callerStoreSeen=caller_store_seen,callerRestoreSeen=caller_restore_seen,callerWaitCallSeen=caller_wait_call_seen,callerWaitTargetSeen=caller_wait_target_seen,callerWaitRtsSeen=caller_wait_rts_seen,callerContinuationSeen=caller_continuation_seen,callerHelperReturnRedirectSeen=caller_helper_return_redirect_seen}
  active=false;case_index=case_index+1
  if case_index>#config.cases then
    cleanup_session();if #event_ids~=0 then error("residual registered callback") end
    status("milestone:callbacks-cleared:0")
    local f=assert(io.open(config.outputPath,"w"));f:write("{\"system\":"..json_string(emu.getsystemid())..",\"core\":"..json_string(config.core)..",\"id\":"..json_string(config.id)..",\"caseOrder\":[")
    for i,cse in ipairs(config.cases) do if i>1 then f:write(",") end;f:write(json_string(cse.id)) end
    f:write("],\"records\":[")
    for i,r in ipairs(records) do
      if i>1 then f:write(",") end
      f:write("{\"id\":"..json_string(r.id)..",\"randomSeedAfter\":"..r.randomSeedAfter..",\"seedCopyAtHelperReturn\":"..r.seedCopyAtHelperReturn..",\"seedCopyAfterSourceCopy\":"..(r.seedCopyAfterSourceCopy and tostring(r.seedCopyAfterSourceCopy) or "null")..",\"resultLowByte\":"..r.resultLowByte..",\"generatorCallCount\":"..r.generatorCallCount..",\"generatorOutputs\":"..json_numbers(r.generatorOutputs)..",\"generatorStates\":"..json_numbers(r.generatorStates)..",\"returnPath\":"..json_string(r.returnPath)..",\"instructionTargetObserved\":"..bool(r.instructionTargetObserved)..",\"effectiveTargetObserved\":"..bool(r.effectiveTargetObserved)..",\"sourceCopyWriteSeen\":"..bool(r.sourceCopyWriteSeen)..",\"callerExecutionObserved\":"..bool(r.callerExecutionObserved)..",\"callerPreambleSeen\":"..bool(r.callerPreambleSeen)..",\"callerRangeSeen\":"..bool(r.callerRangeSeen)..",\"callerRngCallSeen\":"..bool(r.callerRngCallSeen)..",\"callerCallSeen\":"..bool(r.callerCallSeen)..",\"callerStoreSeen\":"..bool(r.callerStoreSeen)..",\"callerRestoreSeen\":"..bool(r.callerRestoreSeen)..",\"callerWaitCallSeen\":"..bool(r.callerWaitCallSeen)..",\"callerWaitTargetSeen\":"..bool(r.callerWaitTargetSeen)..",\"callerWaitRtsSeen\":"..bool(r.callerWaitRtsSeen)..",\"callerContinuationSeen\":"..bool(r.callerContinuationSeen)..",\"callerHelperReturnRedirectSeen\":"..bool(r.callerHelperReturnRedirectSeen).."}")
    end
    f:write("]}");f:close();status("milestone:observer-finished");client.exitCode(0)
  end
end

local function register_callbacks()
  register_exec(i.battleTestEntryPc,"host-battle-test",host_battle_test)
  register_exec(i.numberPromptEntryPc,"host-number-prompt",host_number_prompt)
  register_exec(i.flagPromptEntryPc,"host-flag-prompt",host_flag_prompt)
  register_exec(i.turnOrderEntryPc,"host-turn-order",redirect_host_return)
  register_exec(i.caseEntryPc,"case-entry",begin_case)
  register_exec(f.baseEntryAddress,"base-entry",function() if active then if current_case().callerExecutionObserved then caller_base_entry() else helper_entry("base",false) end end end)
  register_exec(f.baseReturnAddress,"base-return",function() if active then if current_case().callerExecutionObserved then caller_base_return() else base_return() end end end)
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
  register_exec(config.sourceContexts.textWaitPreamblePc,"caller-preamble",function() if active then caller_preamble() end end)
  register_exec(config.sourceContexts.textWaitRangePc,"caller-range-load",function() if active then caller_range_load() end end)
  register_exec(config.sourceContexts.textWaitCallPc,"caller-rng-call",function() if active then caller_rng_call() end end)
  register_exec(config.sourceContexts.textWaitStorePc,"caller-store",function() if active then caller_store() end end)
  register_exec(config.sourceContexts.textWaitPostStorePc,"caller-post-store",function() if active then caller_post_store() end end)
  register_exec(config.sourceContexts.textWaitVIntCallPc,"caller-wait-call",function() if active then caller_wait_call() end end)
  register_exec(config.sourceContexts.diamondPreamblePc,"caller-preamble",function() if active then caller_preamble() end end)
  register_exec(config.sourceContexts.diamondRangePc,"caller-range-load",function() if active then caller_range_load() end end)
  register_exec(config.sourceContexts.diamondCallPc,"caller-rng-call",function() if active then caller_rng_call() end end)
  register_exec(config.sourceContexts.diamondStorePc,"caller-store",function() if active then caller_store() end end)
  register_exec(config.sourceContexts.diamondPostStorePc,"caller-post-store",function() if active then caller_post_store() end end)
  register_exec(config.sourceContexts.diamondVIntCallPc,"caller-wait-call",function() if active then caller_wait_call() end end)
  register_exec(config.sourceContexts.waitForVIntEntryPc,"wait-for-vint-target",caller_wait_target,function() return active and current_case() and current_case().callerExecutionObserved end)
  register_exec(config.sourceContexts.waitForVIntRtsPc,"wait-for-vint-rts",caller_wait_rts,function() return active and current_case() and current_case().callerExecutionObserved end)
  register_exec(i.resultPc,"case-result",function() if active then finish_case() end end)
end
local registration_ok,registration_error=pcall(register_callbacks)
if not registration_ok then fail_callback(registration_error) end
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
