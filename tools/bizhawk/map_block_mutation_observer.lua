local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local json=assert(loadfile(config.jsonModulePath))()
local stage,prompt_count,case_index="cheat",0,1
local queue,records={},{}
local replay_state,pending_save,pending_replay,pending_finish=nil,false,false,false
local active,handler_address,copy_call_site,copy_instruction_site=false,nil,nil,nil
local direct_input_words,copy_offsets,update_observations=nil,{},{}
local names={[1]="Up",[2]="Down",[4]="Left",[8]="Right",[16]="B",[32]="C"}
local cheat={1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4}

local function status(value)local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close()end
local function enqueue(name,count)for _=1,count do queue[#queue+1]=name end end
local function pulse(name)enqueue("",30);enqueue(name,4);enqueue("",8)end
local function set_button(name)local buttons={};if name and name~="" then buttons[name]=true end;joypad.set(buttons,1)end
local function current()return config.caseInputs[case_index]end
local function layout_address(coordinate)return config.ram.layoutBaseAddress+(coordinate.y*config.constants.layoutWordColumnCount+coordinate.x)*config.constants.wordCopyByteStride end
local function array_equal(left,right)
  if #left~=#right then return false end
  for index,value in ipairs(left)do if value~=right[index]then return false end end
  return true
end
local function setup(case)
  local input=config.instrumentation.ramInputAddress
  for _,record in ipairs(case.initialWords)do memory.write_u16_be(layout_address(record.coordinate),record.value,"M68K BUS")end
  memory.write_u8(config.ram.updateToggleBitfieldAddress,case.updateToggleByteSeed,"M68K BUS")
  memory.write_u32_be(input,case.handlerAddress,"M68K BUS")
  for index,word in ipairs(case.inputWords)do memory.write_u16_be(input+4+2*(index-1),word,"M68K BUS")end
end
local function begin()
  if active then return end
  local case=current();if not case then error("map block mutation unexpected wrapper entry")end
  setup(case);active=true;handler_address=nil;copy_call_site=nil;copy_instruction_site=nil;direct_input_words=nil;copy_offsets={};update_observations={};status("milestone:case:"..case.id)
end
local function observe_handler(address)
  if not active then return end
  if handler_address then error("map block mutation duplicate handler entry")end
  handler_address=address;status("milestone:handler-entry:"..current().id)
end
local function observe_direct_call(address)
  if not active then return end
  if copy_call_site then error("map block mutation duplicate direct CopyMapBlocks call")end
  copy_call_site=address
  direct_input_words={emu.getregister("M68K D0")&0xFFFF,emu.getregister("M68K D1")&0xFFFF,emu.getregister("M68K D2")&0xFFFF}
  status("milestone:copy-call:"..current().id)
end
local function observe_copy_instruction(address)
  if not active then return end
  copy_instruction_site=address
  copy_offsets[#copy_offsets+1]={sourceByteOffset=emu.getregister("M68K D0")&0xFFFF,destinationByteOffset=emu.getregister("M68K D2")&0xFFFF}
end
local function observe_update_bit(use_site)
  if not active then return end
  update_observations[#update_observations+1]={bitIndex=use_site.bitIndex,instructionAddressObserved=use_site.instructionAddress,updateToggleByteBefore=memory.read_u8(config.ram.updateToggleBitfieldAddress,"M68K BUS"),firstDestinationWordBefore=memory.read_u16_be(layout_address(current().destinationCoordinate),"M68K BUS")}
end
local function readback(case)
  local result={}
  for _,coordinate in ipairs(case.readbackCoordinates)do result[#result+1]={coordinate=coordinate,value=memory.read_u16_be(layout_address(coordinate),"M68K BUS")}end
  return result
end
local function finish(code)
  if replay_state then memorysavestate.removestate(replay_state)end
  if code~=0 then client.exitCode(code);return end
  local order={};for _,record in ipairs(records)do order[#order+1]=record.id end
  json.write(config.outputPath,{system=emu.getsystemid(),core="Genesis Plus GX",id=config.fixtureId,mapTest=config.mapTestIndex,recordOrder=order,records=records})
  client.exitCode(0)
end

event.on_bus_exec(function()prompt_count=prompt_count+1;status("milestone:number-prompt-entry:"..prompt_count);if prompt_count==1 then stage="map";pending_save=true;pulse("C")end end,config.harness["function"].numberPromptAddress,"sf2-map-block-mutation-number","M68K BUS")
event.on_bus_exec(function()status("milestone:flag-prompt-entry");pulse("B")end,config.harness["function"].flagPromptAddress,"sf2-map-block-mutation-flag","M68K BUS")
event.on_bus_exec(begin,config["function"].runMapSetupInitFunctionAddress,"sf2-map-block-mutation-entry","M68K BUS")
event.on_bus_exec(function()observe_handler(config["function"].setBlocksHandlerAddress)end,config["function"].setBlocksHandlerAddress,"sf2-map-block-mutation-set-blocks","M68K BUS")
event.on_bus_exec(function()observe_handler(config["function"].setBlocksVarHandlerAddress)end,config["function"].setBlocksVarHandlerAddress,"sf2-map-block-mutation-set-blocks-var","M68K BUS")
event.on_bus_exec(function()observe_direct_call(config.caseInputs[1].copyMapBlocksCallSiteAddress)end,config.caseInputs[1].copyMapBlocksCallSiteAddress,"sf2-map-block-mutation-copy-call-one","M68K BUS")
event.on_bus_exec(function()observe_direct_call(config.caseInputs[2].copyMapBlocksCallSiteAddress)end,config.caseInputs[2].copyMapBlocksCallSiteAddress,"sf2-map-block-mutation-copy-call-two","M68K BUS")
event.on_bus_exec(function()observe_copy_instruction(config["function"].copyInstructionAddress)end,config["function"].copyInstructionAddress,"sf2-map-block-mutation-copy-instruction","M68K BUS")
for _,use_site in ipairs(config.updateBitUseSites)do
  local registered_use_site=use_site
  event.on_bus_exec(function()observe_update_bit(registered_use_site)end,registered_use_site.instructionAddress,"sf2-map-block-mutation-update-"..registered_use_site.instructionAddress,"M68K BUS")
end
event.on_bus_exec(function()
  if not active then return end
  local case=current()
  if handler_address~=case.handlerAddress then error("map block mutation handler identity drift")end
  if copy_call_site~=case.copyMapBlocksCallSiteAddress then error("map block mutation direct-call identity drift")end
  if not array_equal(direct_input_words,case.inputWords)then error("map block mutation direct-call input arity drift")end
  if copy_instruction_site~=config["function"].copyInstructionAddress then error("map block mutation copy instruction identity drift")end
  if #copy_offsets~=case.copyInstructionExecutionCount then error("map block mutation copy count drift")end
  if #update_observations~=#case.updateBitUseSites then error("map block mutation update-bit count drift")end
  records[#records+1]={id=case.id,macro=case.macro,handlerAddressObserved=handler_address,copyMapBlocksCallSiteAddressObserved=copy_call_site,copyInstructionAddressObserved=copy_instruction_site,handlerReturned=true,directCallInputWordsObserved=direct_input_words,copyInstructionByteOffsetsObserved=copy_offsets,postCopyUpdateBitObservations=update_observations,updateToggleByteAfter=memory.read_u8(config.ram.updateToggleBitfieldAddress,"M68K BUS"),readbackWordRecords=readback(case)}
  active=false;case_index=case_index+1;if case_index>#config.caseInputs then pending_finish=true else pending_replay=true end
end,config.instrumentation.postHandlerAddress,"sf2-map-block-mutation-return","M68K BUS")

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
