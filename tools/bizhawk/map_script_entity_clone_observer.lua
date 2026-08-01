local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local json=assert(loadfile(config.jsonModulePath))()
local stage,prompt_count,case_index="cheat",0,1
local queue,records={},{},{}
local replay_state,pending_save,pending_replay,pending_finish=nil,false,false,false
local active=false
local handler_entry_pc,handler_rts_pc,handler_entry_a6_offset=nil,nil,nil
local operand_reads,lookup_calls={},{}
local source_entnum_read,destination_entnum_write=nil,nil
local destination_adjacent_before={}
local names={[1]="Up",[2]="Down",[4]="Left",[8]="Right",[16]="B",[32]="C"}
local cheat={1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4}

local function status(value)local file=assert(io.open(config.statusPath,"a"));file:write(value.."\n");file:close()end
local function enqueue(name,count)for _=1,count do queue[#queue+1]=name end end
local function pulse(name)enqueue("",30);enqueue(name,4);enqueue("",8)end
local function set_button(name)local buttons={};if name and name~="" then buttons[name]=true end;joypad.set(buttons,1)end
local function current()return config.cases[case_index]end
local function pc()return emu.getregister("M68K PC")&0xFFFFFF end
local function a6_offset()return (emu.getregister("M68K A6")&0xFFFFFF)-config.instrumentation.ramInputAddress end
local function record_index_from_a5()
  local offset=(emu.getregister("M68K A5")&0xFFFFFF)-config.ram.entityDataAddress
  local stride=config.constants.entityRecordByteCount
  if offset<0 or offset%stride~=0 then error("map-script entity clone A5 record-address boundary drift")end
  return offset//stride
end
local function record_address(index)return config.ram.entityDataAddress+index*config.constants.entityRecordByteCount end
local function lookup_offset_from_d0()
  local value=(emu.getregister("M68K D0")&0xFFFF)&config.constants.lookupMask
  local sign_bit=1<<(config.constants.lookupIndexTransferByteCount*8-1)
  if (value&sign_bit)==0 then return value end
  return value-config.constants.lookupIndexDifference
end
local function lookup_offset_from_word(value)
  local masked=value&config.constants.lookupMask
  local sign_bit=1<<(config.constants.lookupIndexTransferByteCount*8-1)
  if (masked&sign_bit)==0 then return masked end
  return masked-config.constants.lookupIndexDifference
end
local function reset_observation()
  handler_entry_pc=nil;handler_rts_pc=nil;handler_entry_a6_offset=nil;operand_reads={};lookup_calls={}
  source_entnum_read=nil;destination_entnum_write=nil;destination_adjacent_before={}
end
local function setup(case)
  local input=config.instrumentation.ramInputAddress
  local script=input+config.instrumentation.scriptInputRamOffset
  local controls=case.harnessControls
  local words=case.sourceInput.sourceWords
  local source_offset=lookup_offset_from_word(words[1])
  local destination_offset=lookup_offset_from_word(words[2])
  memory.write_u8(config.ram.entityIndexListAddress+source_offset,controls.sourceRecordIndexSeed,"M68K BUS")
  memory.write_u8(config.ram.entityIndexListAddress+destination_offset,controls.destinationRecordIndexSeed,"M68K BUS")
  local field=config.constants.entnumByteOffset
  memory.write_u8(record_address(controls.sourceRecordIndexSeed)+field,controls.sourceEntnumByteSeed,"M68K BUS")
  memory.write_u8(record_address(controls.destinationRecordIndexSeed)+field,controls.destinationEntnumByteSeed,"M68K BUS")
  for _,row in ipairs(controls.destinationAdjacentByteSeeds)do
    memory.write_u8(record_address(controls.destinationRecordIndexSeed)+row.byteOffset,row.byteValue,"M68K BUS")
  end
  memory.write_u32_be(input,config["function"].handlerEntryAddress,"M68K BUS")
  for ordinal,word in ipairs(words)do memory.write_u16_be(script+(ordinal-1)*2,word,"M68K BUS")end
end
local function begin_case()
  if active then return end
  local case=current();if not case then error("map-script entity clone unexpected trampoline entry")end
  setup(case);reset_observation();active=true;status("milestone:case:"..case.id)
end
local function observe_handler_entry()
  if not active then return end
  local actual=pc()
  if actual~=config["function"].handlerEntryAddress or handler_entry_pc~=nil then error("map-script entity clone handler-entry PC identity drift")end
  handler_entry_pc=actual;handler_entry_a6_offset=a6_offset();status("milestone:handler-entry:"..current().id)
end
local function observe_operand_read(ordinal)
  if not active then return end
  local actual=pc()
  local expected=ordinal==1 and config["function"].sourceOperandReadAddress or config["function"].destinationOperandReadAddress
  if actual~=expected or #operand_reads+1~=ordinal then error("map-script entity clone operand-read PC/order drift")end
  local offset=a6_offset()
  local word=memory.read_u16_be(config.instrumentation.ramInputAddress+offset,"M68K BUS")
  operand_reads[#operand_reads+1]={ordinal=ordinal,instructionPc=actual,a6RamOffsetBefore=offset,wordObserved=word}
end
local function observe_lookup_call(ordinal)
  if not active then return end
  local actual=pc()
  local expected=ordinal==1 and config["function"].sourceLookupCallSiteAddress or config["function"].destinationLookupCallSiteAddress
  if actual~=expected or #lookup_calls+1~=ordinal then error("map-script entity clone lookup call PC/order drift")end
  lookup_calls[#lookup_calls+1]={ordinal=ordinal,callSitePc=actual,targetEntryPc=nil,returnPc=nil,lookupIndexByteOffsetObserved=lookup_offset_from_d0()}
  status("milestone:lookup-call:"..ordinal..":"..current().id)
end
local function observe_lookup_entry()
  if not active then return end
  local actual=pc()
  local row=lookup_calls[#lookup_calls]
  if actual~=config["function"].lookupEntryAddress or row==nil or row.targetEntryPc~=nil then error("map-script entity clone lookup target-entry PC identity drift")end
  row.targetEntryPc=actual;status("milestone:lookup-entry:"..row.ordinal..":"..current().id)
end
local function observe_source_field_read()
  if not active then return end
  local actual=pc()
  local first=lookup_calls[1]
  if actual~=config["function"].sourceFieldReadAddress or first==nil or first.targetEntryPc==nil or first.returnPc~=nil then error("map-script entity clone source-field read return boundary drift")end
  first.returnPc=actual
  local record_index=record_index_from_a5()
  local value=memory.read_u8(record_address(record_index)+config.constants.entnumByteOffset,"M68K BUS")
  source_entnum_read={instructionPc=actual,recordIndexObserved=record_index,byteOffset=config.constants.entnumByteOffset,byteValueObserved=value}
end
local function observe_destination_field_write()
  if not active then return end
  local actual=pc()
  local second=lookup_calls[2]
  if actual~=config["function"].destinationFieldWriteAddress or second==nil or second.targetEntryPc==nil or second.returnPc~=nil then error("map-script entity clone destination-field write return boundary drift")end
  second.returnPc=actual
  local record_index=record_index_from_a5()
  local address=record_address(record_index)
  destination_entnum_write={instructionPc=actual,recordIndexObserved=record_index,byteOffset=config.constants.entnumByteOffset,byteValueBeforeObserved=memory.read_u8(address+config.constants.entnumByteOffset,"M68K BUS"),byteValueAfterObserved=nil}
  for _,row in ipairs(current().harnessControls.destinationAdjacentByteSeeds)do
    destination_adjacent_before[#destination_adjacent_before+1]={byteOffset=row.byteOffset,byteValueBeforeObserved=memory.read_u8(address+row.byteOffset,"M68K BUS")}
  end
end
local function observe_handler_rts()
  if not active then return end
  local actual=pc()
  if actual~=config["function"].handlerRtsAddress or handler_rts_pc~=nil then error("map-script entity clone handler RTS PC identity drift")end
  handler_rts_pc=actual;status("milestone:handler-rts:"..current().id)
end
local function finish_case()
  if not active then return end
  local case=current();local controls=case.harnessControls
  if handler_entry_pc==nil or handler_rts_pc==nil or handler_entry_a6_offset==nil or #operand_reads~=2 or #lookup_calls~=2 or source_entnum_read==nil or destination_entnum_write==nil then error("map-script entity clone bounded observation is incomplete")end
  for ordinal,row in ipairs(lookup_calls)do
    if row.targetEntryPc==nil or row.returnPc==nil then error("map-script entity clone lookup event is incomplete: "..ordinal)end
  end
  if source_entnum_read.recordIndexObserved~=controls.sourceRecordIndexSeed or destination_entnum_write.recordIndexObserved~=controls.destinationRecordIndexSeed then error("map-script entity clone controlled lookup record identity drift")end
  local destination_address=record_address(destination_entnum_write.recordIndexObserved)
  destination_entnum_write.byteValueAfterObserved=memory.read_u8(destination_address+config.constants.entnumByteOffset,"M68K BUS")
  if destination_entnum_write.byteValueAfterObserved~=source_entnum_read.byteValueObserved then error("map-script entity clone source-named byte result drift")end
  local adjacent={}
  for _,before in ipairs(destination_adjacent_before)do
    local after=memory.read_u8(destination_address+before.byteOffset,"M68K BUS")
    if after~=before.byteValueBeforeObserved then error("map-script entity clone adjacent-byte mutation drift")end
    adjacent[#adjacent+1]={byteOffset=before.byteOffset,byteValueBeforeObserved=before.byteValueBeforeObserved,byteValueAfterObserved=after}
  end
  local after=a6_offset()
  records[#records+1]={id=case.id,sourceOrderKey=case.sourceInput.sourceOrderKey,handlerEntryPc=handler_entry_pc,handlerRtsPc=handler_rts_pc,scriptCursorRamOffsetBefore=handler_entry_a6_offset,scriptCursorRamOffsetAfter=after,cursorAdvanceByteCountObserved=after-handler_entry_a6_offset,operandReads=operand_reads,lookupCallSequence=lookup_calls,sourceEntnumRead=source_entnum_read,destinationEntnumWrite=destination_entnum_write,destinationAdjacentBytes=adjacent,handlerReturned=true}
  active=false;case_index=case_index+1;if case_index>#config.cases then pending_finish=true else pending_replay=true end
end
local function finish(code)
  if replay_state then memorysavestate.removestate(replay_state)end
  if code~=0 then client.exitCode(code);return end
  local order={};for _,row in ipairs(records)do order[#order+1]=row.id end
  json.write(config.outputPath,{system=emu.getsystemid(),core="Genesis Plus GX",id=config.fixtureId,mapTest=config.mapTestIndex,recordOrder=order,records=records})
  client.exitCode(0)
end

event.on_bus_exec(function()prompt_count=prompt_count+1;status("milestone:number-prompt-entry:"..prompt_count);if prompt_count==1 then stage="map";pending_save=true;pulse("C")end end,config.harness["function"].numberPromptAddress,"entity-clone-number","M68K BUS")
event.on_bus_exec(function()status("milestone:flag-prompt-entry");pulse("B")end,config.harness["function"].flagPromptAddress,"entity-clone-flag","M68K BUS")
event.on_bus_exec(begin_case,config["function"].runMapSetupInitFunctionAddress,"entity-clone-trampoline","M68K BUS")
event.on_bus_exec(observe_handler_entry,config["function"].handlerEntryAddress,"entity-clone-handler-entry","M68K BUS")
event.on_bus_exec(function()observe_operand_read(1)end,config["function"].sourceOperandReadAddress,"entity-clone-operand-1","M68K BUS")
event.on_bus_exec(function()observe_operand_read(2)end,config["function"].destinationOperandReadAddress,"entity-clone-operand-2","M68K BUS")
event.on_bus_exec(function()observe_lookup_call(1)end,config["function"].sourceLookupCallSiteAddress,"entity-clone-call-1","M68K BUS")
event.on_bus_exec(function()observe_lookup_call(2)end,config["function"].destinationLookupCallSiteAddress,"entity-clone-call-2","M68K BUS")
event.on_bus_exec(observe_lookup_entry,config["function"].lookupEntryAddress,"entity-clone-lookup-entry","M68K BUS")
event.on_bus_exec(observe_source_field_read,config["function"].sourceFieldReadAddress,"entity-clone-source-field-read","M68K BUS")
event.on_bus_exec(observe_destination_field_write,config["function"].destinationFieldWriteAddress,"entity-clone-destination-field-write","M68K BUS")
event.on_bus_exec(observe_handler_rts,config["function"].handlerRtsAddress,"entity-clone-handler-rts","M68K BUS")
event.on_bus_exec(finish_case,config.instrumentation.postHandlerAddress,"entity-clone-post-handler","M68K BUS")

local frames=0
while true do
  frames=frames+1
  if pending_finish then finish(0)elseif pending_save then pending_save=false;replay_state=memorysavestate.savecorestate();status("milestone:saved-map-prompt")elseif pending_replay then pending_replay=false;memorysavestate.loadcorestate(replay_state);queue={};pulse("C");status("milestone:replay-map-prompt")end
  if frames>=config.maxFrames then status("timeout:frame-budget-exhausted:case="..case_index..":stage="..stage);finish(1)end
  local button=nil
  if stage=="cheat" then local pointer=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS");if pointer>=0x28FF0 and pointer<0x29000 then button=names[cheat[pointer-0x28FF0+1]]elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Down"end elseif #queue>0 then button=table.remove(queue,1)end
  set_button(button);joypad.set({},2);emu.frameadvance()
  if frames%600==0 then status(string.format("frame=%d,stage=%s,case=%d",frames,stage,case_index))end
end
