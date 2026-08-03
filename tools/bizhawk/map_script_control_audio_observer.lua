local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local stage,prompt_count,case_index="cheat",0,1
local queue,records,event_ids,registered_addresses={},{},{},{}
local replay_state,pending_save,pending_replay,pending_finish=nil,false,false,false
local active,observer_failed,session_cleaned=false,false,false
local current_phase,current_role="registration","registration"
local handler_entries,script_word_reads={},{ }
local skip_gate_set,wait_skip_target_reached,sleep_call_observed=false,false,false
local sleep_d0_word,dispatch_call_observed,play_sound_trap_d0_word=nil,false,nil
local wait_for_vint_calls=0
local csc06_returned,subroutine_target_returned=false,false
local subroutine_entry_sp,subroutine_cursor_after_read_offset=nil,nil
local subroutine_stack_delta_at_call,subroutine_target_stack_delta=nil,nil
local subroutine_resume_stack_delta,jump_cursor_redirect_offset=nil,nil
local end_reached,execute_map_script_returned=false,false
local play_sound_returned,subroutine_handler_returned=false,false
local json_null={}
local names={[1]="Up",[2]="Down",[4]="Left",[8]="Right",[16]="B",[32]="C"}
local cheat={1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4}

local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local buttons={};if name and name~="" then buttons[name]=true end;joypad.set(buttons,1) end
local function word(value) return value&0xFFFF end
local function current_case() return config.cases[case_index] end
local function script_base() return config.instrumentation.ramInputAddress+4 end
local function nullable(value) if value==nil then return json_null end return value end
local function bool(value) if value then return "true" end return "false" end

local function json_string(value) return string.format("%q",value) end
local function is_array(value) local count=0;for key,_ in pairs(value) do if type(key)~="number" then return false end;count=count+1 end;for index=1,count do if value[index]==nil then return false end end;return true end
local function json(value)
  local kind=type(value)
  if value==nil or value==json_null then return "null" end
  if kind=="boolean" then return bool(value) end
  if kind=="number" then return tostring(value) end
  if kind=="string" then return json_string(value) end
  if kind~="table" then error("map-script control/audio JSON type drift: "..kind) end
  local parts={}
  if is_array(value) then for _,item in ipairs(value) do parts[#parts+1]=json(item) end;return "["..table.concat(parts,",").."]" end
  for key,item in pairs(value) do parts[#parts+1]=json_string(key)..":"..json(item) end
  return "{"..table.concat(parts,",").."}"
end
local function copy(value) if type(value)~="table" then return value end;local out={};for key,item in pairs(value) do out[key]=copy(item) end;return out end

local function unregister_events()
  for index=#event_ids,1,-1 do event.unregisterbyid(event_ids[index]);event_ids[index]=nil end
end
local function cleanup_session()
  if session_cleaned then return end
  session_cleaned=true;unregister_events()
  if replay_state then memorysavestate.removestate(replay_state);replay_state=nil end
end
local function pending_callback_state()
  return {phase=current_phase,role=current_role,active=active,handlerEntriesObserved=copy(handler_entries),scriptWordReadCount=#script_word_reads,waitForVIntCallCount=wait_for_vint_calls,subroutineEntryStackPointer=nullable(subroutine_entry_sp)}
end
local function callback_role(phase)
  local roles_by_case=config.failureRoles[phase]
  if roles_by_case==nil then return phase end
  local case=current_case()
  if case==nil or type(roles_by_case[case.id])~="string" then error("map-script control/audio missing deterministic callback role: "..phase) end
  return roles_by_case[case.id]
end
local function failure_expectation(address)
  local by_pc=config.failureExpectations[tostring(address)]
  if type(by_pc)~="table" or type(by_pc.roles)~="table" then return {} end
  local expected=by_pc.roles[current_role]
  if type(expected)~="table" then return {} end
  return expected
end
local function fail_callback(phase,address,message)
  if observer_failed then return end
  observer_failed=true
  local expected=failure_expectation(address)
  local payload={owner=config.observerFailureContract.owner,caseId=nullable((config.cases[case_index] or {}).id),phase=phase,actualPc=nullable(emu.getregister("M68K PC")),expectedCallSiteAddress=nullable(expected.callSiteAddress),expectedTargetAddress=nullable(expected.targetAddress),expectedReturnAddress=nullable(expected.returnAddress),pendingCallback=pending_callback_state(),error=tostring(message)}
  local diagnostic=config.observerFailureContract.statusPrefix..json(payload)
  status(diagnostic);print(diagnostic)
  if config.observerFailureContract.removeOutputBeforeExit then os.remove(config.outputPath) end
  cleanup_session();client.exitCode(config.observerFailureContract.exitCode)
end
local function register_exec(address,phase,callback)
  if registered_addresses[address] then error("map-script control/audio duplicate physical-PC callback: "..address) end
  registered_addresses[address]=phase
  event_ids[#event_ids+1]=event.on_bus_exec(function()
    if observer_failed then return end
    current_phase=phase
    current_role="unresolved:"..phase
    local ok,message=pcall(function() current_role=callback_role(phase);callback() end)
    if not ok then fail_callback(phase,address,message) end
  end,address,"map-script-control-audio-"..phase,"M68K BUS")
end
local function require_equal(actual,expected,message)
  if actual~=expected then error(message..": expected="..tostring(expected)..", actual="..tostring(actual)) end
end
local function require_list(actual,expected,message)
  if #actual~=#expected then error(message.." count drift") end
  for index,value in ipairs(expected) do if actual[index]~=value then error(message.." order drift at "..index) end end
end
local function require_word_reads(actual,expected)
  if #actual~=#expected then error("map-script control/audio word-read count drift") end
  for index,value in ipairs(expected) do
    if actual[index].word~=value.word or actual[index].cursorAfterReadOffset~=value.cursorAfterReadOffset then error("map-script control/audio word-read identity/order drift at "..index) end
  end
end

local function reset_observations()
  handler_entries={};script_word_reads={};skip_gate_set=false;wait_skip_target_reached=false;sleep_call_observed=false;sleep_d0_word=nil;wait_for_vint_calls=0;dispatch_call_observed=false;play_sound_trap_d0_word=nil;csc06_returned=false;subroutine_target_returned=false;subroutine_entry_sp=nil;subroutine_cursor_after_read_offset=nil;subroutine_stack_delta_at_call=nil;subroutine_target_stack_delta=nil;subroutine_resume_stack_delta=nil;jump_cursor_redirect_offset=nil;end_reached=false;execute_map_script_returned=false;play_sound_returned=false;subroutine_handler_returned=false
end
local function begin_case()
  if active then error("map-script control/audio nested entry trampoline") end
  local case=current_case();if case==nil then error("map-script control/audio unexpected entry trampoline") end
  for offset=0,31 do memory.write_u8(script_base()+offset,0,"M68K BUS") end
  for index,value in ipairs(case.scriptBytes) do memory.write_u8(script_base()+index-1,value,"M68K BUS") end
  memory.write_u8(config.ram.player2InputAddress,case.kind=="wait-skip" and config.constants.inputStartMask or 0,"M68K BUS")
  memory.write_u8(config.ram.debugModeToggleAddress,case.kind=="wait-skip" and 255 or 0,"M68K BUS")
  memory.write_u8(config.ram.skipCutsceneTextAddress,0,"M68K BUS")
  reset_observations();active=true;status("milestone:case:"..case.id)
end
local function at_execute_entry()
  if not active then return end
  status("milestone:execute-map-script-entry:"..current_case().id)
  handler_entries[#handler_entries+1]="ExecuteMapScript"
end
local function at_trampoline_entry()
  if not active then return end
  status("milestone:trampoline-entry:"..current_case().id)
end
local function at_script_word_read()
  if not active then return end
  local observed_word=word(emu.getregister("M68K D0"))
  local cursor=emu.getregister("M68K A6")-script_base()
  script_word_reads[#script_word_reads+1]={word=observed_word,cursorAfterReadOffset=cursor}
  status("milestone:loop-predicate:"..current_case().id..":word="..observed_word..":cursor="..cursor)
end
local function at_skip_gate_set()
  if active then skip_gate_set=true;status("milestone:wait-skip-gate:"..current_case().id) end
end
local function at_wait_skip_target()
  if active then wait_skip_target_reached=true;status("milestone:wait-loop-branch:"..current_case().id) end
end
local function at_sleep_call()
  if not active then return end
  sleep_call_observed=true;sleep_d0_word=word(emu.getregister("M68K D0"));status("milestone:sleep-call:"..current_case().id..":d0="..sleep_d0_word)
end
local function at_wait_for_vint()
  if not active then return end
  wait_for_vint_calls=wait_for_vint_calls+1
  status("milestone:wait-for-vint:"..current_case().id..":count="..wait_for_vint_calls)
end
local function at_dispatch_call()
  if active then dispatch_call_observed=true end
end
local function at_dispatch_return()
  if not active then return end
  local case=current_case()
  if case.kind=="no-op" then csc06_returned=true end
  if case.kind=="sound" and not play_sound_returned then error("map-script control/audio csc05 return boundary missing") end
  if case.kind=="subroutine" and not subroutine_handler_returned then error("map-script control/audio csc0A return boundary missing") end
end
local function at_end()
  if active then end_reached=true;status("milestone:csc-end-dispatch:"..current_case().id) end
end
local function at_csc05_entry()
  if not active then return end
  if current_case().kind~="sound" then error("map-script control/audio unexpected csc05 entry") end
  handler_entries[#handler_entries+1]="csc05_playSound";status("milestone:handler-entry:csc05:"..current_case().id)
end
local function at_csc05_trap()
  if not active then return end
  play_sound_trap_d0_word=word(emu.getregister("M68K D0"))
  script_word_reads[#script_word_reads+1]={word=play_sound_trap_d0_word,cursorAfterReadOffset=emu.getregister("M68K A6")-script_base()}
end
local function at_csc05_return()
  if active then play_sound_returned=true end
end
local function at_csc06_entry()
  if not active then return end
  local kind=current_case().kind
  if kind~="no-op" and kind~="subroutine" then error("map-script control/audio unexpected csc06 entry") end
  handler_entries[#handler_entries+1]="csc06_doNothing";status("milestone:handler-entry:csc06:"..current_case().id)
  if kind=="subroutine" then subroutine_target_stack_delta=emu.getregister("M68K SP")-subroutine_entry_sp end
end
local function at_csc0a_entry()
  if not active then return end
  if current_case().kind~="subroutine" then error("map-script control/audio unexpected csc0A entry") end
  handler_entries[#handler_entries+1]="csc0A_executeSubroutine";status("milestone:handler-entry:csc0a:"..current_case().id);subroutine_entry_sp=emu.getregister("M68K SP")
end
local function at_csc0a_cursor()
  if active then subroutine_cursor_after_read_offset=emu.getregister("M68K A6")-script_base() end
end
local function at_csc0a_call()
  if active then subroutine_stack_delta_at_call=emu.getregister("M68K SP")-subroutine_entry_sp end
end
local function at_csc0a_resume()
  if not active then return end
  subroutine_target_returned=true;subroutine_resume_stack_delta=emu.getregister("M68K SP")-subroutine_entry_sp
end
local function at_csc0a_return()
  if active then subroutine_handler_returned=true end
end
local function at_csc0b_entry()
  if not active then return end
  if current_case().kind~="jump" then error("map-script control/audio unexpected csc0B entry") end
  handler_entries[#handler_entries+1]="csc0B_jump";status("milestone:handler-entry:csc0b:"..current_case().id)
end
local function at_csc0b_redirect()
  if active then jump_cursor_redirect_offset=emu.getregister("M68K A6")-script_base() end
end
local function append_record()
  local case=current_case();local expected=case.expected
  if not end_reached then error("map-script control/audio csc_end boundary missing") end
  if expected.skipGateSetObserved~=skip_gate_set or (skip_gate_set and not wait_skip_target_reached) then error("map-script control/audio skip-gate branch drift") end
  require_list(handler_entries,expected.handlerEntries,"map-script control/audio handler entries")
  require_word_reads(script_word_reads,expected.scriptWordReads)
  require_equal(sleep_call_observed,expected.sleepCallObserved,"map-script control/audio sleep-call presence")
  require_equal(wait_for_vint_calls,case.waitForVIntCalls,"map-script control/audio WaitForVInt count")
  require_equal(sleep_d0_word,expected.sleepD0Word,"map-script control/audio sleep D0")
  require_equal(dispatch_call_observed,expected.dispatchCallObserved,"map-script control/audio dispatch call")
  require_equal(play_sound_trap_d0_word,expected.playSoundTrapD0Word,"map-script control/audio sound trap D0")
  require_equal(csc06_returned,expected.csc06Returned,"map-script control/audio csc06 return")
  require_equal(subroutine_cursor_after_read_offset,expected.subroutineCursorAfterReadOffset,"map-script control/audio subroutine cursor")
  require_equal(subroutine_stack_delta_at_call,expected.subroutineStackDeltaAtCall,"map-script control/audio subroutine save stack")
  require_equal(subroutine_target_stack_delta,expected.subroutineTargetStackDelta,"map-script control/audio subroutine target stack")
  require_equal(subroutine_target_returned,expected.subroutineTargetReturned,"map-script control/audio subroutine target return")
  require_equal(subroutine_resume_stack_delta,expected.subroutineResumeStackDelta,"map-script control/audio subroutine restore stack")
  require_equal(jump_cursor_redirect_offset,expected.jumpCursorRedirectOffset,"map-script control/audio jump cursor")
  records[#records+1]={id=case.id,questionId=case.questionId,handlerEntries=copy(handler_entries),executeMapScriptReturned=true,scriptWordReads=copy(script_word_reads),skipGateSetObserved=skip_gate_set,sleepCallObserved=sleep_call_observed,sleepD0Word=nullable(sleep_d0_word),dispatchCallObserved=dispatch_call_observed,playSoundTrapD0Word=nullable(play_sound_trap_d0_word),csc06Returned=csc06_returned,subroutineCursorAfterReadOffset=nullable(subroutine_cursor_after_read_offset),subroutineStackDeltaAtCall=nullable(subroutine_stack_delta_at_call),subroutineTargetStackDelta=nullable(subroutine_target_stack_delta),subroutineTargetReturned=subroutine_target_returned,subroutineResumeStackDelta=nullable(subroutine_resume_stack_delta),jumpCursorRedirectOffset=nullable(jump_cursor_redirect_offset),endReached=end_reached}
end
local function at_post_handler()
  if not active then return end
  execute_map_script_returned=true;append_record();status("milestone:trampoline-complete:"..current_case().id);active=false;case_index=case_index+1
  if case_index>#config.cases then pending_finish=true else pending_replay=true end
end
local function write_output()
  local f=assert(io.open(config.outputPath,"w"))
  f:write('{"system":"'..emu.getsystemid()..'","core":"Genesis Plus GX","id":"'..config.fixtureId..'","mapTest":'..config.mapTestIndex..',"recordOrder":[')
  for index,row in ipairs(records) do if index>1 then f:write(",") end;f:write(json_string(row.id)) end
  f:write('],"records":[')
  for index,row in ipairs(records) do if index>1 then f:write(",") end;f:write(json(row)) end
  f:write("]}\n");f:close()
end
local function finish(exit_code)
  cleanup_session()
  if exit_code~=0 then client.exitCode(exit_code);return end
  if #event_ids~=0 then error("map-script control/audio residual callback registration") end
  status("milestone:callbacks-cleared:0");write_output();status("milestone:observer-finished");client.exitCode(0)
end

local function run()
  register_exec(config.harness["function"].numberPromptAddress,"number-prompt",function()
    prompt_count=prompt_count+1;status("milestone:number-prompt-entry:"..prompt_count)
    if prompt_count==1 then stage="map";pending_save=true;pulse("C") end
  end)
  register_exec(config.harness["function"].flagPromptAddress,"flag-prompt",function() status("milestone:flag-prompt-entry");pulse("B") end)
  register_exec(config["function"].entryAddress,"run-map-setup-entry",begin_case)
  register_exec(config.instrumentation.stubAddress,"trampoline-entry",at_trampoline_entry)
  register_exec(config["function"].executeMapScriptAddress,"execute-map-script-entry",at_execute_entry)
  register_exec(config["function"].scriptWordReadAfterAddress,"script-word-read",at_script_word_read)
  register_exec(config["function"].waitSkipGateSetAddress,"wait-skip-gate",at_skip_gate_set)
  register_exec(config["function"].waitSleepCallAddress,"wait-sleep",at_sleep_call)
  register_exec(config.service.waitForVIntAddress,"wait-for-vint",at_wait_for_vint)
  register_exec(config["function"].waitSkipTargetAddress,"wait-skip-target",at_wait_skip_target)
  register_exec(config["function"].opcodeDispatchCallAddress,"opcode-dispatch",at_dispatch_call)
  register_exec(config["function"].opcodeDispatchReturnAddress,"opcode-dispatch-return",at_dispatch_return)
  register_exec(config["function"].endAddress,"end",at_end)
  register_exec(config["function"].csc05PlaySoundAddress,"csc05-entry",at_csc05_entry)
  register_exec(config["function"].playSoundTrapAddress,"csc05-trap",at_csc05_trap)
  register_exec(config["function"].playSoundReturnAddress,"csc05-return",at_csc05_return)
  register_exec(config["function"].csc06DoNothingAddress,"csc06-entry",at_csc06_entry)
  register_exec(config["function"].csc0AExecuteSubroutineAddress,"csc0a-entry",at_csc0a_entry)
  register_exec(config["function"].subroutineCursorAfterReadAddress,"csc0a-cursor",at_csc0a_cursor)
  register_exec(config["function"].subroutineIndirectCallAddress,"csc0a-call",at_csc0a_call)
  register_exec(config["function"].subroutineResumeAddress,"csc0a-resume",at_csc0a_resume)
  register_exec(config["function"].subroutineReturnAddress,"csc0a-return",at_csc0a_return)
  register_exec(config["function"].csc0BJumpAddress,"csc0b-entry",at_csc0b_entry)
  register_exec(config["function"].jumpCursorRedirectAfterAddress,"csc0b-redirect",at_csc0b_redirect)
  register_exec(config.instrumentation.trampolinePostHandlerAddress,"post-handler",at_post_handler)
  status("milestone:observer-ready")
  local frames=0
  while true do
    frames=frames+1
    if pending_finish then finish(0);return end
    if pending_save then pending_save=false;replay_state=memorysavestate.savecorestate();status("milestone:saved-map-prompt")
    elseif pending_replay then pending_replay=false;memorysavestate.loadcorestate(replay_state);queue={};pulse("C");status("milestone:replay-map-prompt") end
    if frames>=config.maxFrames then status("timeout:frame-budget-exhausted:case="..case_index..":stage="..stage);finish(1);return end
    local button=nil
    if stage=="cheat" then
      local pointer=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS")
      if pointer>=0x28FF0 and pointer<0x29000 then button=names[cheat[pointer-0x28FF0+1]]
      elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Down" end
    elseif #queue>0 then button=table.remove(queue,1) end
    set_button(button);joypad.set({},2);emu.frameadvance()
    if frames%600==0 then status("frame="..frames..",stage="..stage..",case="..case_index) end
  end
end

local ok,message=pcall(run)
if not ok then fail_callback(current_phase,emu.getregister("M68K PC"),message) end
