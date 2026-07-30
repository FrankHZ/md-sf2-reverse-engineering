local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local stage,prompt_count,case_index="cheat",0,1
local queue,records={},{}
local replay_state,pending_save,pending_replay,pending_finish=nil,false,false,false
local active,handler_entered=false,false
local callback_order,wait_compare_count,wait_back_edge_count={},0,0
local get_entity_seen,indexed_seen,indexed_target_seen,terminal_seen,inline_seen=false,false,false,false,false
local action_buffer_words,terminal_idle_payload_long,terminal_snapshot_count={},nil,0
local names={[1]="Up",[2]="Down",[4]="Left",[8]="Right",[16]="B",[32]="C"}
local cheat={1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4}

local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local b={};if name and name~="" then b[name]=true end;joypad.set(b,1) end
local function strings(f,values) f:write("[");for i,value in ipairs(values) do if i>1 then f:write(",") end;f:write(string.format('"%s"',value)) end;f:write("]") end
local function nullable(value) if value==nil then return "null" end;return tostring(value) end
local function boolean(value) if value then return "true" end;return "false" end
local function current_case() return config.cases[case_index] end
local function current_derived() return config.derived[case_index] end
local function callback(name) callback_order[#callback_order+1]=name;status("milestone:"..name..":"..current_case().id) end
local function entity_address(case) return config.ram.entityDataAddress+case.entityIndexByteSeed*config.constants.entityRecordByteCount end
local function field_address(case,name) return entity_address(case)+config.constants.entityStateFields[name].byteOffset end
local function read_field(case,name) local f=config.constants.entityStateFields[name];local a=field_address(case,name);if f.transferByteCount==1 then return memory.read_u8(a,"M68K BUS") end;return memory.read_u32_be(a,"M68K BUS") end
local function write_field(case,name,value) local f=config.constants.entityStateFields[name];local a=field_address(case,name);if f.transferByteCount==1 then memory.write_u8(a,value,"M68K BUS") else memory.write_u32_be(a,value,"M68K BUS") end end
local function pointer_input(case) if case.pointerInputKind=="eas-idle" then return config["function"].easIdleAddress end;if case.pointerInputKind=="session-action-buffer" then return config.instrumentation.sessionActionBufferAddress end;error("map entity action bridge pointer kind drift") end

local function setup_case(case,derived)
  local input=config.instrumentation.ramInputAddress
  local script=input+config.instrumentation.scriptInputRamOffset
  memory.write_u8(config.ram.entityIndexListAddress+case.selectorByte,case.entityIndexByteSeed,"M68K BUS")
  write_field(case,"actscriptPointer",case.entityStateSeed.actscriptPointerLong)
  write_field(case,"actscriptWaitTimer",case.entityStateSeed.actscriptWaitTimerByte)
  write_field(case,"flagsA",case.entityStateSeed.flagsAByte)
  memory.write_u32_be(config.ram.entityActionBufferPointerAddress,case.actionBufferPointerLongSeed,"M68K BUS")
  memory.write_u32_be(input,derived.handlerAddress,"M68K BUS")
  memory.write_u8(script,case.selectorByte,"M68K BUS");memory.write_u8(script+1,case.controlByte,"M68K BUS")
  if case.handler=="csc15_setEntityActscript" then memory.write_u32_be(script+2,pointer_input(case),"M68K BUS")
  elseif case.handler=="csc14_setEntityActscriptManual" then memory.write_u16_be(script+2,case.inlineTerminatorWord,"M68K BUS")
  else
    memory.write_u8(script+2,case.actionCommandByte,"M68K BUS");memory.write_u8(script+3,case.actionPayloadByte,"M68K BUS")
    memory.write_u8(script+4,case.terminalCommandByte,"M68K BUS");memory.write_u8(script+5,case.terminalSkippedByte,"M68K BUS")
  end
end

local function begin_case()
  if active then return end
  local case=current_case();if case==nil then error("map entity action bridge unexpected trampoline entry") end
  setup_case(case,current_derived());active=true;handler_entered=false;callback_order={};wait_compare_count=0;wait_back_edge_count=0;get_entity_seen=false;indexed_seen=false;indexed_target_seen=false;terminal_seen=false;inline_seen=false;action_buffer_words={};terminal_idle_payload_long=nil;terminal_snapshot_count=0
  status("milestone:case:"..case.id)
end
local function handler(name)
  if not active then return end
  if current_case().handler~=name or emu.getregister("M68K PC")~=current_derived().handlerAddress then error("map entity action bridge handler identity drift") end
  handler_entered=true;status("milestone:handler-entry:"..current_case().id)
end
local function observe_get()
  if not active or get_entity_seen then error("map entity action bridge get-entity callback drift") end
  get_entity_seen=true;callback("getEntityAddress")
end
local function observe_indexed()
  if not active or current_case().handler~="csc2D_entityActionSequence" or indexed_seen then error("map entity action bridge indexed call drift") end
  indexed_seen=true;callback("indexedCallback")
end
local function observe_indexed_target()
  if not active or current_case().handler~="csc2D_entityActionSequence" or indexed_target_seen then error("map entity action bridge indexed target drift") end
  if emu.getregister("M68K PC")~=current_derived().indexedActionTargetAddress then error("map entity action bridge indexed effective target drift") end
  indexed_target_seen=true;callback("indexedTargetEntry")
end
local function observe_terminal()
  if not active or current_case().handler~="csc2D_entityActionSequence" or terminal_seen then error("map entity action bridge terminal drift") end
  terminal_seen=true;callback("terminalEntry")
end
local function observe_terminal_payload_after_write()
  if not active or current_case().handler~="csc2D_entityActionSequence" or not terminal_seen then error("map entity action bridge terminal payload hook drift") end
  if terminal_snapshot_count~=0 then error("map entity action bridge duplicate terminal buffer snapshot") end
  local d=current_derived()
  local base=current_case().actionBufferPointerLongSeed
  for i,_ in ipairs(d.actionBufferWords) do action_buffer_words[i]=memory.read_u16_be(base+(i-1)*d.indexedActionBufferWordTransferByteCount,"M68K BUS") end
  local address=current_case().actionBufferPointerLongSeed+d.indexedActionBufferByteCount+config.constants.terminalActionBufferRecordWordTransferByteCount
  terminal_idle_payload_long=memory.read_u32_be(address,"M68K BUS")
  terminal_snapshot_count=terminal_snapshot_count+1
end
local function observe_inline()
  if not active or current_case().handler~="csc14_setEntityActscriptManual" or inline_seen then error("map entity action bridge inline terminator drift") end
  inline_seen=true;callback("inlineTerminator")
end
local function observe_wait_compare()
  if not active or current_derived().waitLoopExitInjection==nil then return end
  wait_compare_count=wait_compare_count+1;callback("waitCompareEntry")
  local injection=current_derived().waitLoopExitInjection
  if wait_compare_count==injection.afterCompareEntryCount then
    write_field(current_case(),injection.field,injection.value);callback("harnessForcedWaitExit")
  elseif wait_compare_count>injection.afterCompareEntryCount then error("map entity action bridge excessive wait compares") end
end
local function observe_wait_back_edge_instruction()
  if not active or current_derived().waitLoopExitInjection==nil then return end
  wait_back_edge_count=wait_back_edge_count+1;callback("waitBackEdgeInstructionEntry")
end

local function write_words(f,r)
  f:write("[")
  for i,value in ipairs(r.actionBufferWords) do if i>1 then f:write(",") end;f:write(value) end
  f:write("]")
end
local function write_injection(f,value)
  if value==nil then f:write("null");return end
  f:write(string.format('{"programCounterAddress":%d,"backEdgeInstructionAddress":%d,"field":"%s","value":%d,"afterCompareEntryCount":%d}',value.programCounterAddress,value.backEdgeInstructionAddress,value.field,value.value,value.afterCompareEntryCount))
end
local function append_record()
  local case,derived=current_case(),current_derived()
  if not handler_entered or not get_entity_seen then error("map entity action bridge handler/callback did not execute") end
  if wait_compare_count~=derived.waitCompareEntryCount then error("map entity action bridge wait compare count drift") end
  if wait_back_edge_count~=derived.waitCompareEntryCount then error("map entity action bridge wait back-edge instruction count drift") end
  if inline_seen~=derived.inlineTerminatorObserved or indexed_seen~=derived.indexedCallbackObserved or terminal_seen~=derived.terminalObserved then error("map entity action bridge callback path drift") end
  if case.handler=="csc2D_entityActionSequence" and not indexed_target_seen then error("map entity action bridge indexed target not entered") end
  if case.handler=="csc2D_entityActionSequence" then
    if terminal_snapshot_count~=1 or terminal_idle_payload_long~=derived.terminalActionBufferIdlePayloadLong then error("map entity action bridge terminal payload snapshot drift") end
    for i,value in ipairs(derived.actionBufferWords) do if action_buffer_words[i]~=value then error("map entity action bridge terminal buffer snapshot drift") end end
  end
  local offset=emu.getregister("M68K A6")-config.instrumentation.ramInputAddress
  if offset~=derived.scriptCursorRamOffsetAfter then error("map entity action bridge cursor drift") end
  if read_field(case,"actscriptWaitTimer")~=derived.actscriptWaitTimerByteAfter or read_field(case,"actscriptPointer")~=derived.actscriptPointerLongAfter or read_field(case,"flagsA")~=derived.flagsAByteAfter then error("map entity action bridge state result drift") end
  if memory.read_u32_be(config.ram.entityActionBufferPointerAddress,"M68K BUS")~=derived.actionBufferPointerLongAfter then error("map entity action bridge buffer pointer drift") end
  records[#records+1]={case=case,derived=derived,indexedTargetObserved=indexed_target_seen,actionBufferWords=action_buffer_words,terminalActionBufferIdlePayloadLong=terminal_idle_payload_long,terminalBufferSnapshotCountObserved=terminal_snapshot_count,waitBackEdgeInstructionEntryCountObserved=wait_back_edge_count,waitCompareEntryCountObserved=wait_compare_count,callbackOrder=callback_order}
end
local function write_record(f,r)
  local d,c=r.derived,r.case
  f:write(string.format('{"id":"%s","macro":"%s","handlerAddress":%d,"selectorByte":%d,"controlByte":%d,"actscriptWaitTimerByteAfter":%d,"actscriptWaitTimerTransferByteCount":%d,"actscriptPointerTransferByteCount":%d,"getEntityCallSiteAddress":%d,"waitCompareEntryCount":%d,"waitLoopExitInjection":',d.id,d.macro,d.handlerAddress,d.selectorByte,d.controlByte,d.actscriptWaitTimerByteAfter,d.actscriptWaitTimerTransferByteCount,d.actscriptPointerTransferByteCount,d.getEntityCallSiteAddress,d.waitCompareEntryCount));write_injection(f,d.waitLoopExitInjection);f:write(string.format(',"scriptCursorRamOffsetAfter":%d,"actscriptPointerLongAfter":%d,"inlineTerminatorObserved":%s,"indexedCallbackObserved":%s,"terminalObserved":%s,"flagsAByteAfter":%d,"actionBufferWords":',d.scriptCursorRamOffsetAfter,d.actscriptPointerLongAfter,boolean(d.inlineTerminatorObserved),boolean(d.indexedCallbackObserved),boolean(d.terminalObserved),d.flagsAByteAfter));write_words(f,r);f:write(string.format(',"actionBufferPointerLongAfter":%d',d.actionBufferPointerLongAfter))
  if c.handler=="csc2D_entityActionSequence" then
    f:write(string.format(',"indexedActionBufferByteCount":%d,"indexedActionBufferWordTransferByteCount":%d,"terminalActionBufferIdlePayloadLong":%d,"terminalActionBufferIdlePayloadTransferByteCount":%d,"indexedActionTarget":"%s","indexedActionTargetAddress":%d,"terminalCommandByte":%d,"terminalSkippedByte":%d',d.indexedActionBufferByteCount,d.indexedActionBufferWordTransferByteCount,r.terminalActionBufferIdlePayloadLong,d.terminalActionBufferIdlePayloadTransferByteCount,d.indexedActionTarget,d.indexedActionTargetAddress,d.terminalCommandByte,d.terminalSkippedByte))
  end
  f:write(string.format(',"handlerReturned":true,"getEntityCallObserved":true,"indexedTargetObserved":%s,"terminalBufferSnapshotCountObserved":%d,"waitBackEdgeInstructionEntryCountObserved":%d,"waitCompareEntryCountObserved":%d,"callbackOrder":',boolean(r.indexedTargetObserved),r.terminalBufferSnapshotCountObserved,r.waitBackEdgeInstructionEntryCountObserved,r.waitCompareEntryCountObserved));strings(f,r.callbackOrder);f:write("}")
end
local function finish(code)
  if replay_state then memorysavestate.removestate(replay_state) end
  if code~=0 then client.exitCode(code);return end
  local f=assert(io.open(config.outputPath,"w"));f:write(string.format('{"system":"%s","core":"Genesis Plus GX","id":"%s","mapTest":%d,"recordOrder":',emu.getsystemid(),config.fixtureId,config.mapTestIndex));local ids={};for _,c in ipairs(config.cases) do ids[#ids+1]=c.id end;strings(f,ids);f:write(',"records":[');for i,r in ipairs(records) do if i>1 then f:write(",") end;write_record(f,r) end;f:write("]}\n");f:close();client.exitCode(0)
end

event.on_bus_exec(function() prompt_count=prompt_count+1;status("milestone:number-prompt-entry:"..prompt_count);if prompt_count==1 then stage="map";pending_save=true;pulse("C") end end,config.harness["function"].numberPromptAddress,"bridge-number","M68K BUS")
event.on_bus_exec(function() status("milestone:flag-prompt-entry");pulse("B") end,config.harness["function"].flagPromptAddress,"bridge-flag","M68K BUS")
event.on_bus_exec(begin_case,config["function"].runMapSetupInitFunctionAddress,"bridge-entry","M68K BUS")
event.on_bus_exec(function() handler("csc15_setEntityActscript") end,config["function"].csc15_setEntityActscriptAddress,"bridge-csc15","M68K BUS")
event.on_bus_exec(function() handler("csc14_setEntityActscriptManual") end,config["function"].csc14_setEntityActscriptManualAddress,"bridge-csc14","M68K BUS")
event.on_bus_exec(function() handler("csc2D_entityActionSequence") end,config["function"].csc2D_entityActionSequenceAddress,"bridge-csc2d","M68K BUS")
event.on_bus_exec(observe_get,config["function"].csc15GetEntityCallSiteAddress,"bridge-csc15-get","M68K BUS")
event.on_bus_exec(observe_get,config["function"].csc14GetEntityCallSiteAddress,"bridge-csc14-get","M68K BUS")
event.on_bus_exec(observe_get,config["function"].csc2DGetEntityCallSiteAddress,"bridge-csc2d-get","M68K BUS")
event.on_bus_exec(observe_indexed,config["function"].csc2DIndexedCallSiteAddress,"bridge-indexed","M68K BUS")
event.on_bus_exec(observe_terminal,config["function"].csc2DTerminalEntryAddress,"bridge-terminal","M68K BUS")
event.on_bus_exec(observe_terminal_payload_after_write,config["function"].csc2DTerminalPayloadAfterWriteAddress,"bridge-terminal-payload","M68K BUS")
event.on_bus_exec(observe_inline,config["function"].csc14InlineTerminatorCompareAddress,"bridge-inline","M68K BUS")
event.on_bus_exec(observe_wait_compare,config["function"].csc15WaitCompareAddress,"bridge-csc15-wait","M68K BUS")
event.on_bus_exec(observe_wait_compare,config["function"].csc14WaitCompareAddress,"bridge-csc14-wait","M68K BUS")
event.on_bus_exec(observe_wait_compare,config["function"].csc2DTerminalWaitCompareAddress,"bridge-csc2d-wait","M68K BUS")
event.on_bus_exec(observe_wait_back_edge_instruction,config["function"].csc15WaitBackEdgeAddress,"bridge-csc15-back","M68K BUS")
event.on_bus_exec(observe_wait_back_edge_instruction,config["function"].csc14WaitBackEdgeAddress,"bridge-csc14-back","M68K BUS")
event.on_bus_exec(observe_wait_back_edge_instruction,config["function"].csc2DTerminalWaitBackEdgeAddress,"bridge-csc2d-back","M68K BUS")
event.on_bus_exec(function() if active and current_case().handler=="csc2D_entityActionSequence" then observe_indexed_target() end end,config.indexedTargetAddress,"bridge-indexed-target","M68K BUS")
event.on_bus_exec(function() if not active then return end;append_record();active=false;case_index=case_index+1;if case_index>#config.cases then pending_finish=true else pending_replay=true end end,config.instrumentation.postHandlerAddress,"bridge-return","M68K BUS")

local frames=0
while true do
  frames=frames+1
  if pending_finish then finish(0) elseif pending_save then pending_save=false;replay_state=memorysavestate.savecorestate();status("milestone:saved-map-prompt") elseif pending_replay then pending_replay=false;memorysavestate.loadcorestate(replay_state);queue={};pulse("C");status("milestone:replay-map-prompt") end
  if frames>=config.maxFrames then status("timeout:frame-budget-exhausted:case="..case_index);finish(1) end
  local button=nil
  if stage=="cheat" then local p=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS");if p>=0x28FF0 and p<0x29000 then button=names[cheat[p-0x28FF0+1]] elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Down" end elseif #queue>0 then button=table.remove(queue,1) end
  set_button(button);joypad.set({},2);emu.frameadvance()
end
