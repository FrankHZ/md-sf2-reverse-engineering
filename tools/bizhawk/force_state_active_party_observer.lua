local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local json=assert(loadfile(config.jsonModulePath))()
local stage,prompts,index="cheat",0,1
local queue,records={},{}
local replay,pending_save,pending_replay,pending_finish=nil,false,false,false
local active,chronology=false,{}
local completed
local reset_trace={}
local names={[1]="Up",[2]="Down",[4]="Left",[8]="Right",[16]="B",[32]="C"}
local cheat={1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4}
local function enqueue(value,count) for _=1,count do queue[#queue+1]=value end end
local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
local function pulse(value) enqueue("",30);enqueue(value,4);enqueue("",8) end
local function button(value) local b={};if value and value~="" then b[value]=true end;joypad.set(b,1) end
local function current() return config.cases[index] end
local function flag(bit)
  local address=config.ram.gameFlags+math.floor(bit/config.ram.bitsPerByte)
  return (memory.read_u8(address,"M68K BUS") & (config.ram.firstFlagMask >> (bit%config.ram.bitsPerByte)))~=0
end
local function snapshot(case,input)
  local state=input.state
  local members={};for _,member in ipairs(state.probeMembers or {}) do members[#members+1]={member=member,joined=flag(member),active=flag(config.ram.forceFlagActiveStart+member)} end
  local follower={};for offset=0,#(state.followers or {}) do follower[#follower+1]=memory.read_u8(config.ram.explorationEntities+offset,"M68K BUS") end
  local character=state.probeCharacter or 0
  local combatant=config.ram.combatantData+character*config.ram.combatantEntryBytes
  local result={};if state.probeMembers then result.members=members end
  if state.probeMembers then result.battleParty={count=memory.read_u16_be(config.ram.battlePartyMembersNumber,"M68K BUS"),members={memory.read_u8(config.ram.battlePartyMembers,"M68K BUS"),memory.read_u8(config.ram.battlePartyMembers+1,"M68K BUS")},dialogueNameIndex=memory.read_u16_be(config.ram.dialogueNameIndex,"M68K BUS")} end
  if state.activation~=nil then result.activationBitfield=memory.read_u16_be(combatant+config.ram.activationOffset,"M68K BUS") end
  if state.reset then result.representative={maxHp=memory.read_u16_be(combatant+config.ram.maxHpOffset,"M68K BUS"),currentHp=memory.read_u16_be(combatant+config.ram.currentHpOffset,"M68K BUS"),maxMp=memory.read_u8(combatant+config.ram.maxMpOffset,"M68K BUS"),currentMp=memory.read_u8(combatant+config.ram.currentMpOffset,"M68K BUS"),status=memory.read_u16_be(combatant+config.ram.statusOffset,"M68K BUS")} end
  if state.followers then
    local entities={};for _,entity in ipairs(state.probeEntities) do local base=config.ram.entityData+entity*config.ram.entityEntryBytes;local pointer=memory.read_u32_be(base+config.ram.entityActscriptOffset,"M68K BUS");entities[#entities+1]={entity=entity,actscriptPointer=pointer,parameters={memory.read_u16_be(pointer+config.ram.followerParameterOffsets[1],"M68K BUS"),memory.read_u16_be(pointer+config.ram.followerParameterOffsets[2],"M68K BUS"),memory.read_u16_be(pointer+config.ram.followerParameterOffsets[3],"M68K BUS")}} end
    result.followerBytesThroughPostTerminator=follower;result.walkingParametersPointer=memory.read_u32_be(config.ram.entityWalkingParameters,"M68K BUS");result.entities=entities
  end
  return result
end
local before=nil
local function setup()
  local case,input=current(),config.caseInputs[index]
  assert(case and input and case.id==input.id,"active-party case input drift")
  for offset=0,config.ram.forceFlagClearByteSpan-1 do memory.write_u8(config.ram.gameFlags+offset,0,"M68K BUS") end
  local function set_flag(bit)
    local address=config.ram.gameFlags+math.floor(bit/config.ram.bitsPerByte)
    memory.write_u8(address,memory.read_u8(address,"M68K BUS") | (config.ram.firstFlagMask >> (bit%config.ram.bitsPerByte)),"M68K BUS")
  end
  for _,member in ipairs(input.state.joined or {}) do set_flag(config.ram.forceFlagJoinedStart+member) end
  for _,member in ipairs(input.state.active or {}) do set_flag(config.ram.forceFlagActiveStart+member) end
  for member,hp in ipairs(input.state.hp or {}) do
    memory.write_u16_be(config.ram.combatantData+(member-1)*config.ram.combatantEntryBytes+config.ram.currentHpOffset,hp,"M68K BUS")
  end
  if input.state.activation~=nil then memory.write_u16_be(config.ram.combatantData+input.state.probeCharacter*config.ram.combatantEntryBytes+config.ram.activationOffset,input.state.activation,"M68K BUS") end
  if input.state.reset then
    for member=0,config.runtimeContract.resetServices.allyCounter do
      local base=config.ram.combatantData+member*config.ram.combatantEntryBytes
      memory.write_u16_be(base+config.ram.maxHpOffset,20+member,"M68K BUS");memory.write_u16_be(base+config.ram.currentHpOffset,1,"M68K BUS")
      memory.write_u8(base+config.ram.maxMpOffset,10,"M68K BUS");memory.write_u8(base+config.ram.currentMpOffset,1,"M68K BUS");memory.write_u16_be(base+config.ram.statusOffset,15,"M68K BUS")
    end
  end
  if input.state.followers then
    for offset,value in ipairs(input.state.followers) do memory.write_u8(config.ram.explorationEntities+offset-1,value & 0xFF,"M68K BUS") end
    for character,entity in ipairs(input.state.entityIndexAssignments or {}) do memory.write_u8(config.ram.entityIndexList+character-1,entity,"M68K BUS") end
  end
  memory.write_u32_be(config.instrumentation.trampoline.ramInputAddress,input.handlerAddress,"M68K BUS")
  local base=config.instrumentation.trampoline.ramInputAddress+4
  for offset,value in ipairs(input.streamBytes) do memory.write_u8(base+offset-1,value,"M68K BUS") end
  chronology={};reset_trace={};before=snapshot(case,input);active=true
end
local function finish(code)
  if replay then memorysavestate.removestate(replay) end
  if code~=0 then client.exitCode(code);return end
  local order={};for _,case in ipairs(config.cases) do order[#order+1]=case.id end
  json.write(config.outputPath,{system=emu.getsystemid(),core="Genesis Plus GX",id=config.fixtureId,mapTest=config.mapTest,recordOrder=order,records=records})
  client.exitCode(0)
end
event.on_bus_exec(function()
  prompts=prompts+1;status("number:"..prompts);if prompts==1 then stage="map";pending_save=true;pulse("C") end
end,config.harness["function"].numberPromptAddress,"force-number","M68K BUS")
event.on_bus_exec(function() status("flag");pulse("B") end,config.harness["function"].flagPromptAddress,"force-flag","M68K BUS")
event.on_bus_exec(function()
  status("entry:"..index)
  if active then completed();return end
  setup()
end,config.runtimeContract.entryAddress,"force-entry","M68K BUS")
for _,handler in ipairs(config.runtimeContract.handlers) do
  for _,call in ipairs(handler.calls) do
    event.on_bus_exec(function()
      if active then chronology[#chronology+1]={kind="call",instructionTarget=call.instructionTarget,effectiveTarget=call.effectiveTarget,h1Address=call.h1Address} end
    end,call.h1Address,"force-call-"..call.h1Address,"M68K BUS")
  end
end
completed=function()
  if not active then return end
  local case,input=current(),config.caseInputs[index];local record={id=case.id,handlerAddress=case.handlerAddress,handlerReturned=true,before=before,after=snapshot(case,input),chronology=chronology};if case.id=="reset-mixed-allies" then record.resetServiceTrace=table.concat(reset_trace,",") end;records[#records+1]=record
  active=false;index=index+1;if index>#config.cases then pending_finish=true else pending_replay=true end
end
for _,handler in ipairs(config.runtimeContract.handlers) do
  event.on_bus_exec(completed,handler.returnAddress,"force-return-"..handler.returnAddress,"M68K BUS")
end
for _,call in ipairs(config.runtimeContract.resetServices.calls) do
  event.on_bus_exec(function() if active and current().id=="reset-mixed-allies" then reset_trace[#reset_trace+1]=call.code..":"..(emu.getregister("M68K D0")&0xFFFF) end end,call.h1Address,"force-reset-"..call.code,"M68K BUS")
end
local frames=0
while true do
  frames=frames+1
  if pending_finish then finish(0) elseif pending_save then pending_save=false;replay=memorysavestate.savecorestate() elseif pending_replay then pending_replay=false;memorysavestate.loadcorestate(replay);queue={};pulse("C") end
  if frames>=config.maxFrames then status("frame-limit:"..index);finish(1) end
  local pressed=nil
  if stage=="cheat" then
    local pointer=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS")
    if pointer>=0x28FF0 and pointer<0x29000 then pressed=names[cheat[pointer-0x28FF0+1]] elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then pressed="Down" end
  elseif #queue>0 then pressed=table.remove(queue,1) end
  button(pressed);joypad.set({},2);emu.frameadvance()
end
