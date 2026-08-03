local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local stage,prompt_count,case_index="cheat",0,1
local queue,records={},{}
local replay_state,pending_save,pending_replay,pending_finish=nil,false,false,false
local event_ids={}
local observer_failed,current_dispatch_phase=false,"registration"
local session_cleaned=false
local active,handler_entered,handler_returned=false,false,false
local handler_entry_pc=nil
local callback_dispatches,pending_callback={},nil
local operand_reads,loop_count,initial_stack_pointer={},0,nil
local first_compare,second_compare,post_loop_compare=nil,nil,nil
local chunk_d1,chunk_shift_count,chunk_add_count=nil,0,0
local anim_initial,anim_final,flags_set,flags_clear=nil,nil,nil,nil
local json_null={}
local names={[1]="Up",[2]="Down",[4]="Left",[8]="Right",[16]="B",[32]="C"}
local cheat={1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4}

local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local buttons={};if name and name~="" then buttons[name]=true end;joypad.set(buttons,1) end
local function current_case() return config.cases[case_index] end
local function current_derived() return config.derived[case_index] end
local function current_output_derived() return config.outputDerived[case_index] end
local function word(value) return value&0xFFFF end
local function boolean(value) if value then return "true" end return "false" end
local function json_string(value) return string.format("%q",value) end
local function is_array(value) local count=0;for key,_ in pairs(value) do if type(key)~="number" then return false end;count=count+1 end;for index=1,count do if value[index]==nil then return false end end;return true end
local function json(value)
  local kind=type(value);if value==nil or value==json_null then return "null" end;if kind=="boolean" then return boolean(value) end;if kind=="number" then return tostring(value) end;if kind=="string" then return json_string(value) end;if kind~="table" then error("entity-presentation FX JSON type drift: "..kind) end
  local parts={};if is_array(value) then for _,item in ipairs(value) do parts[#parts+1]=json(item) end;return "["..table.concat(parts,",").."]" end
  for key,item in pairs(value) do parts[#parts+1]=json_string(key)..":"..json(item) end;return "{"..table.concat(parts,",").."}"
end
local function copy(value) if type(value)~="table" then return value end;local result={};for key,item in pairs(value) do result[key]=copy(item) end;return result end
local function nullable(value) if value==nil then return json_null end return value end
local function restore_source_nulls(source_input)
  for _,operand in ipairs(source_input.operandValues) do
    if operand.resolution=="symbol" and operand.resolvedValue==nil then operand.resolvedValue=json_null end
  end
end

local function unregister_events()
  for index=#event_ids,1,-1 do event.unregisterbyid(event_ids[index]);event_ids[index]=nil end
end
local function cleanup_session()
  if session_cleaned then return end
  session_cleaned=true;unregister_events()
  if replay_state then memorysavestate.removestate(replay_state);replay_state=nil end
end
local function expected_callback_for_failure()
  if pending_callback~=nil then return pending_callback end
  local derived=config.derived[case_index]
  if active and derived~=nil and derived.directCallbackPlan~=nil then return derived.directCallbackPlan[#callback_dispatches+1] end
  return nil
end
local function fail_callback(phase,address,message)
  if observer_failed then return end
  observer_failed=true
  local expected=expected_callback_for_failure()
  local payload={owner=config.observerFailureContract.owner,caseId=nullable((config.cases[case_index] or {}).id),phase=phase,actualPc=nullable(emu.getregister("M68K PC")),expectedCallSiteAddress=nullable(expected and (expected.callSiteAddressExpected or expected.callSiteAddress)),expectedTargetAddress=nullable(expected and (expected.targetAddressExpected or expected.targetAddress)),expectedReturnAddress=nullable(expected and (expected.returnAddressExpected or expected.returnAddress)),pendingCallback=nullable(copy(pending_callback)),error=tostring(message)}
  local diagnostic=config.observerFailureContract.statusPrefix..json(payload)
  status(diagnostic);print(diagnostic)
  if config.observerFailureContract.removeOutputBeforeExit then os.remove(config.outputPath) end
  cleanup_session();client.exitCode(config.observerFailureContract.exitCode)
end
local function register_exec(callback,address,name)
  event_ids[#event_ids+1]=event.on_bus_exec(function()
    if observer_failed then return end
    local ok,message=pcall(callback)
    if not ok then fail_callback(current_dispatch_phase,address,message) end
  end,address,name,"M68K BUS")
end

local function entity_address() return config.ram.entityDataAddress end
local function read_entity(offset) return memory.read_u8(entity_address()+offset,"M68K BUS") end
local function reset_observations()
  callback_dispatches={};pending_callback=nil;operand_reads={};loop_count=0;initial_stack_pointer=nil
  first_compare=nil;second_compare=nil;post_loop_compare=nil;chunk_d1=nil;chunk_shift_count=0;chunk_add_count=0
  anim_initial=nil;anim_final=nil;flags_set=nil;flags_clear=nil;handler_entered=false;handler_returned=false;handler_entry_pc=nil
end
local function setup_case()
  local case,derived=current_case(),current_derived();if case==nil or derived==nil then error("entity-presentation FX unexpected trampoline entry") end
  local input=config.instrumentation.ramInputAddress+config.instrumentation.scriptInputRamOffset
  for index,value in ipairs(case.handlerInputWords) do memory.write_u16_be(input+(index-1)*2,value,"M68K BUS") end
  memory.write_u8(entity_address()+config.constants.animCounterByteOffset,127,"M68K BUS")
  memory.write_u8(entity_address()+config.constants.flagsBByteOffset,161,"M68K BUS")
  memory.write_u32_be(config.instrumentation.ramInputAddress,derived.handlerAddress,"M68K BUS")
  reset_observations();active=true;status("milestone:case:"..case.id)
end
local function observe_handler(macro,address)
  if not active then return end
  if current_case().macro~=macro or current_derived().handlerAddress~=address or emu.getregister("M68K PC")~=address then error("entity-presentation FX handler PC identity drift") end
  handler_entry_pc=emu.getregister("M68K PC");handler_entered=true;initial_stack_pointer=emu.getregister("M68K SP");status("milestone:handler-entry:"..current_case().id)
end
local function callback_for_site(address)
  local expected=current_derived().directCallbackPlan
  local index=#callback_dispatches+1
  local callback=expected[index]
  if callback==nil or callback.callSiteAddress~=address then error("entity-presentation FX callback call-site chronology drift") end
  return callback
end
local function observe_callback_site(address)
  if not active then return end
  local pc=emu.getregister("M68K PC");if pc~=address or pending_callback~=nil then error("entity-presentation FX callback call-site PC drift") end
  local callback=callback_for_site(address)
  pending_callback={instructionTarget=callback.instructionTarget,effectiveTarget=callback.effectiveTarget,callSiteAddressExpected=callback.callSiteAddress,callSiteAddressObserved=pc,targetRole=callback.targetRole,targetAddressExpected=callback.targetAddress,returnAddressExpected=callback.returnAddress}
end
local function observe_callback_target(address)
  if not active then return end
  local pc=emu.getregister("M68K PC")
  if pending_callback==nil or pc~=address or pending_callback.targetAddressExpected~=address then error("entity-presentation FX shim target PC drift") end
  pending_callback.targetAddressObserved=pc
end
local function observe_callback_return(address)
  if not active then return end
  if pending_callback==nil then return end
  local pc=emu.getregister("M68K PC")
  if pc~=address or pending_callback.returnAddressExpected~=address or pending_callback.targetAddressObserved==nil then error("entity-presentation FX callback return chronology drift") end
  pending_callback.returnAddressExpected=nil;pending_callback.targetAddressExpected=nil;pending_callback.returnAddressObserved=pc
  callback_dispatches[#callback_dispatches+1]=pending_callback;pending_callback=nil
end
local function observe_operand(address)
  if active and emu.getregister("M68K PC")==address then operand_reads[#operand_reads+1]=word(emu.getregister("M68K D0")) end
end
local function observe_flash_duration(address)
  if active and emu.getregister("M68K PC")==address then operand_reads[#operand_reads+1]=word(emu.getregister("M68K D7")) end
end
local function observe_loop(macro,address)
  if active and current_case().macro==macro and emu.getregister("M68K PC")==address then loop_count=loop_count+1 end
end
local function observe_return(address)
  if not active then return end
  local pc=emu.getregister("M68K PC")
  if pc~=address or address~=current_derived().handlerReturnAddress then return end
  handler_returned=true
end
local function callback_matches_expected(observed,expected)
  return observed.instructionTarget==expected.instructionTarget and observed.effectiveTarget==expected.effectiveTarget and observed.callSiteAddressObserved==expected.callSiteAddress and observed.targetRole==expected.targetRole and observed.targetAddressObserved==expected.targetAddress and observed.returnAddressObserved==expected.returnAddress
end
local function compact_callback_segments()
  local output=current_output_derived();local event_index=1;local result={}
  for _,segment in ipairs(output.callbackPlanSegments) do
    local callbacks=segment.callbacks
    if #callbacks==0 then error("entity-presentation FX callback segment pattern drift") end
    local sites={}
    for callback_index,expected in ipairs(callbacks) do
      local observed=callback_dispatches[event_index+callback_index-1]
      if observed==nil or not callback_matches_expected(observed,expected) then error("entity-presentation FX callback segment identity/order drift") end
      sites[#sites+1]={instructionTarget=observed.instructionTarget,effectiveTarget=observed.effectiveTarget,callSiteAddressObserved=observed.callSiteAddressObserved,targetRole=observed.targetRole,targetAddressObserved=observed.targetAddressObserved,returnAddressObserved=observed.returnAddressObserved}
    end
    local repeat_count=0
    while event_index+(repeat_count+1)*#callbacks<=#callback_dispatches+1 do
      local matched=true
      for callback_index,expected in ipairs(callbacks) do
        local observed=callback_dispatches[event_index+repeat_count*#callbacks+callback_index-1]
        if observed==nil or not callback_matches_expected(observed,expected) then matched=false;break end
      end
      if not matched then break end
      repeat_count=repeat_count+1
    end
    if repeat_count==0 then error("entity-presentation FX callback segment repeat boundary drift") end
    result[#result+1]={repeatCountObserved=repeat_count,callbackSitesObserved=sites}
    event_index=event_index+repeat_count*#callbacks
  end
  if event_index~=#callback_dispatches+1 then error("entity-presentation FX callback segment event-count drift") end
  return result
end
local function append_record()
  local case,derived=current_case(),current_derived()
  if not handler_entered or not handler_returned or pending_callback~=nil then error("entity-presentation FX handler did not return cleanly") end
  if #callback_dispatches~=#derived.directCallbackPlan then error("entity-presentation FX callback count drift") end
  for index,expected in ipairs(derived.directCallbackPlan) do
    local observed=callback_dispatches[index]
    if observed.instructionTarget~=expected.instructionTarget or observed.effectiveTarget~=expected.effectiveTarget or observed.callSiteAddressObserved~=expected.callSiteAddress or observed.targetAddressObserved~=expected.targetAddress or observed.returnAddressObserved~=expected.returnAddress then error("entity-presentation FX callback identity/order drift") end
  end
  if loop_count~=derived.loopIterationCount then error("entity-presentation FX loop iteration drift") end
  local offset=emu.getregister("M68K A6")-config.instrumentation.ramInputAddress
  if offset~=derived.scriptCursorRamOffsetAfter then error("entity-presentation FX A6 cursor drift") end
  local counts={};for _,identity in ipairs(config.targetIdentities) do counts[identity]=0 end;for _,item in ipairs(callback_dispatches) do counts[item.effectiveTarget]=counts[item.effectiveTarget]+1 end
  for identity,value in pairs(derived.callbackSiteCounts) do if counts[identity]~=value then error("entity-presentation FX zero-inclusive callback total drift: "..identity) end end
  local record=copy(current_output_derived())
  restore_source_nulls(record.sourceInput)
  record.specialTransitionD1Word=nullable(derived.specialTransitionD1Word)
  record.entityFieldPlan=nullable(derived.entityFieldPlan)
  record.handlerEntryPcObserved=handler_entry_pc;record.handlerReturned=true
  record.callbackPlanSegmentsObserved=compact_callback_segments();record.handlerInputWordsAtOperandReads=operand_reads
  record.loopIterationCountObserved=loop_count;record.stackPointerDeltaBytesObserved=emu.getregister("M68K SP")-initial_stack_pointer
  record.scriptCursorRamOffsetAfterObserved=offset
  record.selectorFirstCompareWordObserved=nullable(first_compare);record.selectorSecondCompareWordObserved=nullable(second_compare);record.postLoopSelectorWordObserved=nullable(post_loop_compare)
  record.specialTransitionD1WordAtBitTestObserved=nullable(chunk_d1);record.specialTransitionShiftCountObserved=chunk_shift_count;record.specialTransitionAddCountObserved=chunk_add_count
  record.animCounterByteAfterInitialWriteObserved=nullable(anim_initial);record.animCounterByteAfterFinalWriteObserved=nullable(anim_final)
  record.flagsBByteAfterSetWriteObserved=nullable(flags_set);record.flagsBByteAfterClearWriteObserved=nullable(flags_clear)
  records[#records+1]=record
end
local function finish(code)
  if observer_failed then return end
  cleanup_session()
  if code~=0 then client.exitCode(code);return end
  status("milestone:callbacks-cleared:"..#event_ids)
  local result={system=emu.getsystemid(),core="Genesis Plus GX",id=config.fixtureId,mapTest=config.mapTestIndex,recordOrder={},records=records}
  for _,case in ipairs(config.cases) do result.recordOrder[#result.recordOrder+1]=case.id end
  local file=assert(io.open(config.outputPath,"w"));file:write(json(result).."\n");file:close();status("milestone:observer-finished");client.exitCode(0)
end

local function observe_phase(phase,address)
  if phase=="callback-return" then observe_callback_return(address)
  elseif phase=="number-prompt" then prompt_count=prompt_count+1;status("milestone:number-prompt-entry:"..prompt_count);if prompt_count==1 then stage="map";pending_save=true;pulse("C") end
  elseif phase=="flag-prompt" then status("milestone:flag-prompt-entry");pulse("B")
  elseif phase=="setup-case" then setup_case()
  elseif phase=="handler-animEntityFX" then observe_handler("animEntityFX",address)
  elseif phase=="handler-headshake" then observe_handler("headshake",address)
  elseif phase=="handler-entityFlashWhite" then observe_handler("entityFlashWhite",address)
  elseif phase=="operand-csc22-first" or phase=="operand-csc22-second" or phase=="operand-csc27-first" or phase=="operand-csc18-first" then observe_operand(address)
  elseif phase=="flash-duration-csc18-second" then observe_flash_duration(address)
  elseif phase=="selector-first-compare" then if active then first_compare=word(emu.getregister("M68K D0")) end
  elseif phase=="selector-second-compare" then if active then second_compare=word(emu.getregister("M68K D0")) end
  elseif phase=="selector-post-loop-compare" then if active then post_loop_compare=word(emu.getregister("M68K D2")) end
  elseif phase=="special-transition-d1-bit-test" then if active then chunk_d1=word(emu.getregister("M68K D1")) end
  elseif phase=="special-transition-shift" then if active then chunk_shift_count=chunk_shift_count+1 end
  elseif phase=="special-transition-add" then if active then chunk_add_count=chunk_add_count+1 end
  elseif phase=="loop-anim-regular" then observe_loop("animEntityFX",address)
  elseif phase=="loop-anim-chunk" then observe_loop("animEntityFX",address)
  elseif phase=="loop-headshake" then observe_loop("headshake",address)
  elseif phase=="loop-flash" then observe_loop("entityFlashWhite",address)
  elseif phase=="field-headshake-anim-initial" then if active then anim_initial=read_entity(config.constants.animCounterByteOffset) end
  elseif phase=="field-headshake-anim-final" then if active then anim_final=read_entity(config.constants.animCounterByteOffset) end
  elseif phase=="field-flash-flags-set" then if active then flags_set=read_entity(config.constants.flagsBByteOffset) end
  elseif phase=="field-flash-flags-clear" then if active then flags_clear=read_entity(config.constants.flagsBByteOffset) end
  elseif phase=="handler-return" then observe_return(address)
  elseif phase=="callback-site" then observe_callback_site(address)
  elseif phase=="callback-target" then observe_callback_target(address)
  elseif phase=="post-handler" then if not active then return end;append_record();active=false;case_index=case_index+1;if case_index>#config.cases then pending_finish=true else pending_replay=true end
  else error("entity-presentation FX observer dispatch phase drift: "..phase) end
end
local function dispatch_observation(address,phases)
  for _,phase in ipairs(phases) do
    if observer_failed then return end
    current_dispatch_phase=phase;observe_phase(phase,address)
  end
end
for _,plan in ipairs(config.observerDispatchPlan) do
  local address,phases=plan.address,plan.phases
  register_exec(function() dispatch_observation(address,phases) end,address,"entity-fx-dispatch-"..address)
end
event_ids[#event_ids+1]=event.onexit(cleanup_session,"entity-fx-session-cleanup")

status("milestone:observer-ready")
local frames=0
while not observer_failed do
  frames=frames+1
  if pending_finish then finish(0) elseif pending_save then pending_save=false;replay_state=memorysavestate.savecorestate();status("milestone:saved-map-prompt") elseif pending_replay then pending_replay=false;memorysavestate.loadcorestate(replay_state);queue={};pulse("C");status("milestone:replay-map-prompt") end
  if frames>=config.maxFrames then status("timeout:frame-budget-exhausted:case="..case_index);finish(1) end
  local button=nil;if stage=="cheat" then local pointer=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS");if pointer>=0x28FF0 and pointer<0x29000 then button=names[cheat[pointer-0x28FF0+1]] elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Down" end elseif #queue>0 then button=table.remove(queue,1) end
  set_button(button);joypad.set({},2);emu.frameadvance()
end
client.exitCode(config.observerFailureContract.exitCode)
