local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local json=assert(loadfile(config.jsonModulePath))()
local stage,prompt_count,case_index="cheat",0,1
local queue,records={},{}
local replay_state,pending_save,pending_replay,pending_finish=nil,false,false,false
local active,observed_handler,callback_events=false,nil,{}
local names={[1]="Up",[2]="Down",[4]="Left",[8]="Right",[16]="B",[32]="C"}
local cheat={1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4}

local function status(value)local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close()end
local function enqueue(name,count)for _=1,count do queue[#queue+1]=name end end
local function pulse(name)enqueue("",30);enqueue(name,4);enqueue("",8)end
local function set_button(name)local buttons={};if name and name~="" then buttons[name]=true end;joypad.set(buttons,1)end
local function current()return config.cases[case_index]end
local function write_value(address,width,value)
  if width==1 then memory.write_u8(address,value,"M68K BUS")
  elseif width==2 then memory.write_u16_be(address,value,"M68K BUS")
  elseif width==4 then memory.write_u32_be(address,value,"M68K BUS")
  else error("entity population unsupported write width: "..width)end
end
local function read_value(address,width)
  if width==1 then return memory.read_u8(address,"M68K BUS")
  elseif width==2 then return memory.read_u16_be(address,"M68K BUS")
  elseif width==4 then return memory.read_u32_be(address,"M68K BUS")
  else error("entity population unsupported read width: "..width)end
end
local function reset_case_memory(case)
  for offset=0,config.constants.allocationScanItemCount-1 do memory.write_u8(config.ram.entityIndexListAddress+offset,0,"M68K BUS")end
  if case.clearGameFlags then for offset=0,127 do memory.write_u8(config.ram.gameFlagsAddress+offset,0,"M68K BUS")end end
  for _,record in ipairs(case.indexListSeeds)do memory.write_u8(config.ram.entityIndexListAddress+record.offset,record.value,"M68K BUS")end
  for _,record in ipairs(case.entitySlotSeeds)do
    local base=config.ram.entityDataAddress+record.slotIndex*config.constants.entityRecordByteCount
    for _,field in ipairs(record.fields)do write_value(base+field.byteOffset,field.transferByteCount,field.value)end
  end
  memory.write_u8(config.ram.currentMapAddress,case.currentMap,"M68K BUS")
end
local function setup(case)
  reset_case_memory(case)
  local input=config.instrumentation.ramInputAddress
  memory.write_u32_be(input,case.handlerAddress,"M68K BUS")
  for _,write in ipairs(case.scriptInputWrites)do write_value(input+4+write.byteOffset,write.transferredByteCount,write.value)end
end
local function snapshot(callback)
  local pc=emu.getregister("M68K PC")&0xFFFFFF
  if pc~=callback.callSiteAddress then error("entity population callback PC identity drift")end
  return {instructionTarget=callback.instructionTarget,callSiteAddressObserved=pc,d0=emu.getregister("M68K D0")&0xFFFFFFFF,d1=emu.getregister("M68K D1")&0xFFFFFFFF,d2=emu.getregister("M68K D2")&0xFFFFFFFF,d3=emu.getregister("M68K D3")&0xFFFFFFFF,d4=emu.getregister("M68K D4")&0xFFFFFFFF,d5=emu.getregister("M68K D5")&0xFFFFFFFF,a0=emu.getregister("M68K A0")&0xFFFFFF}
end
local function begin()
  if active then return end
  local case=current();if not case then error("entity population unexpected wrapper entry")end
  setup(case);active=true;observed_handler=nil;callback_events={};status("milestone:case:"..case.id)
end
local function observe_handler(handler)
  if not active then return end
  local case=current()
  if handler.macro~=case.macro or observed_handler~=nil then error("entity population handler identity drift")end
  local pc=emu.getregister("M68K PC")&0xFFFFFF
  if pc~=handler.handlerAddress then error("entity population handler PC identity drift")end
  observed_handler=pc;status("milestone:handler-entry:"..case.id)
end
local function observe_callback(handler,callback)
  if not active then return end
  local case=current()
  if handler.macro~=case.macro then error("entity population unexpected callback handler")end
  callback_events[#callback_events+1]=snapshot(callback)
  status("milestone:callback:"..callback.instructionTarget..":"..case.id)
end
local function index_records(case)
  local result={}
  for _,offset in ipairs(case.indexReadOffsets)do result[#result+1]={offset=offset,value=memory.read_u8(config.ram.entityIndexListAddress+offset,"M68K BUS")}end
  return result
end
local function entity_records(case)
  local result={}
  for _,slot in ipairs(case.entitySlotReadIndices)do
    local base=config.ram.entityDataAddress+slot*config.constants.entityRecordByteCount
    local fields={}
    for name,layout in pairs(config.constants.entityFieldLayouts)do fields[name]=read_value(base+layout.byteOffset,layout.transferByteCount)end
    result[#result+1]={slotIndex=slot,fields=fields}
  end
  return result
end
local function non_empty_slot_count()
  local count=0
  for slot=0,config.constants.clearRecordCount-1 do
    local x=memory.read_u16_be(config.ram.entityDataAddress+slot*config.constants.entityRecordByteCount,"M68K BUS")
    if x~=config.constants.emptyCoordinateWord then count=count+1 end
  end
  return count
end
local function finish(code)
  if replay_state then memorysavestate.removestate(replay_state)end
  if code~=0 then client.exitCode(code);return end
  local order={};for _,record in ipairs(records)do order[#order+1]=record.id end
  json.write(config.outputPath,{system=emu.getsystemid(),core="Genesis Plus GX",id=config.fixtureId,mapTest=config.mapTestIndex,recordOrder=order,records=records})
  client.exitCode(0)
end

event.on_bus_exec(function()prompt_count=prompt_count+1;status("milestone:number-prompt-entry:"..prompt_count);if prompt_count==1 then stage="map";pending_save=true;pulse("C")end end,config.harness["function"].numberPromptAddress,"sf2-entity-population-number","M68K BUS")
event.on_bus_exec(function()status("milestone:flag-prompt-entry");pulse("B")end,config.harness["function"].flagPromptAddress,"sf2-entity-population-flag","M68K BUS")
event.on_bus_exec(begin,config["function"].runMapSetupInitFunctionAddress,"sf2-entity-population-entry","M68K BUS")
for _,handler in ipairs(config.handlers)do
  local registered_handler=handler
  event.on_bus_exec(function()observe_handler(registered_handler)end,registered_handler.handlerAddress,"sf2-entity-population-handler-"..registered_handler.macro,"M68K BUS")
  for _,callback in ipairs(registered_handler.callbacks)do
    local registered_callback=callback
    event.on_bus_exec(function()observe_callback(registered_handler,registered_callback)end,registered_callback.callSiteAddress,"sf2-entity-population-callback-"..registered_handler.macro.."-"..registered_callback.callSiteAddress,"M68K BUS")
  end
end
event.on_bus_exec(function()
  if not active then return end
  local case=current()
  if observed_handler~=case.handlerAddress then error("entity population handler did not execute")end
  local expected_callbacks=config.callbackOrdersByMacro[case.macro]
  if #callback_events~=#expected_callbacks then error("entity population callback count drift")end
  for index,target in ipairs(expected_callbacks)do if callback_events[index].instructionTarget~=target then error("entity population callback order drift")end end
  records[#records+1]={id=case.id,macro=case.macro,handlerAddressObserved=observed_handler,handlerReturned=true,scriptCursorRamOffsetAfter=emu.getregister("M68K A6")-config.instrumentation.ramInputAddress,callbackEvents=callback_events,indexReadRecords=index_records(case),entitySlotReadRecords=entity_records(case),nonEmptyClearSpanSlotCount=non_empty_slot_count()}
  active=false;case_index=case_index+1;if case_index>#config.cases then pending_finish=true else pending_replay=true end
end,config.instrumentation.postHandlerAddress,"sf2-entity-population-return","M68K BUS")

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
