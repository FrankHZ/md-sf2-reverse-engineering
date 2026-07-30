local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local json=assert(loadfile(config.jsonModulePath))()
local stage,prompt_count,case_index="cheat",0,1
local queue,records={},{}
local replay_state,pending_save,pending_replay,pending_finish=nil,false,false,false
local active=false
local active_handler_address=nil
local chronology={}
local names={[1]="Up",[2]="Down",[4]="Left",[8]="Right",[16]="B",[32]="C"}
local cheat={1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4}
local function status(v)local f=assert(io.open(config.statusPath,"a"));f:write(v.."\n");f:close()end
local function enqueue(n,c)for _=1,c do queue[#queue+1]=n end end
local function pulse(n)enqueue("",30);enqueue(n,4);enqueue("",8)end
local function set_button(n)local b={};if n and n~="" then b[n]=true end;joypad.set(b,1)end
local function current()return config.cases[case_index]end
local function setup(case,input)
  assert(input.id==case.id,"story-state case input identity drift")
  for i=0,config.gameFlags.byteSpan-1 do memory.write_u8(config.gameFlags.baseAddress+i,0,"M68K BUS") end
  local f=case.expected.flagStorage
  local seed=0;if f.initialFlagSet then seed=f.flagBitMask end
  memory.write_u8(f.flagStorageAddress,seed,"M68K BUS")
  memory.write_u32_be(config.instrumentation.trampoline.ramInputAddress,input.handlerAddress,"M68K BUS")
  local stream=config.instrumentation.trampoline.ramInputAddress+4
  for offset,value in ipairs(input.streamBytes) do memory.write_u8(stream+offset-1,value,"M68K BUS") end
  memory.write_u16_be(config.instrumentation.yesNoPromptStub.resultRamAddress,input.promptResultWord,"M68K BUS")
  local handler_address=memory.read_u32_be(config.instrumentation.trampoline.ramInputAddress,"M68K BUS")
  assert(handler_address==input.handlerAddress,"story-state trampoline handler input drift")
  return handler_address
end
local function begin()
  if active then return end
  local case=current();if not case then error("story-state extra trampoline entry")end
  local input=config.caseInputs[case_index];if not input then error("story-state case input is missing")end
  active_handler_address=setup(case,input);active=true;chronology={};status("milestone:case:"..case.id)
end
local function word(name)return emu.getregister("M68K "..name)&0xFFFF end
local function trace(kind,address,instruction,effective)
  if not active then return end
  chronology[#chronology+1]={kind=kind,h1Address=address,instructionTarget=instruction or json.null,effectiveTarget=effective or json.null,d0Word=word("D0"),d1Word=word("D1"),a6=emu.getregister("M68K A6")&0xFFFFFF}
end
local function finish(code)
  if replay_state then memorysavestate.removestate(replay_state)end
  if code~=0 then client.exitCode(code);return end
  json.write(config.outputPath,{system=emu.getsystemid(),core="Genesis Plus GX",id=config.fixtureId,mapTest=config.mapTest,records=records})
  client.exitCode(0)
end
event.on_bus_exec(function()prompt_count=prompt_count+1;if prompt_count==1 then stage="map";pending_save=true;pulse("C")end end,config.harness["function"].numberPromptAddress,"story-number","M68K BUS")
event.on_bus_exec(function()pulse("B")end,config.harness["function"].flagPromptAddress,"story-flag","M68K BUS")
event.on_bus_exec(begin,config.runtimeContract.entryAddress,"story-entry","M68K BUS")
event.on_bus_exec(function()
  if not active then return end
  local c=current();local byte=memory.read_u8(c.expected.flagStorage.flagStorageAddress,"M68K BUS");records[#records+1]={id=c.id,handlerAddress=active_handler_address,a6Output=emu.getregister("M68K A6")&0xFFFFFF,flagByteAfter=byte,finalFlagSet=(byte&c.expected.flagStorage.flagBitMask)~=0,chronology=chronology}
  active=false;case_index=case_index+1;if case_index>#config.cases then pending_finish=true else pending_replay=true end
end,config.returnProgramCounter,"story-return","M68K BUS")
for _,handler in ipairs(config.runtimeContract.handlerRecords) do
  for _,site in ipairs(handler.cursorUseSites) do
    event.on_bus_exec(function()trace("use",site.h1Address,nil,nil)end,site.h1Address,"story-use-"..site.h1Address,"M68K BUS")
  end
  for _,call in ipairs(handler.directCalls) do
    event.on_bus_exec(function()trace("call",call.h1Address,call.instructionTarget,call.effectiveTarget)end,call.h1Address,"story-call-"..call.h1Address,"M68K BUS")
  end
end
local frames=0
while true do
  frames=frames+1
  if pending_finish then finish(0)elseif pending_save then pending_save=false;replay_state=memorysavestate.savecorestate()elseif pending_replay then pending_replay=false;memorysavestate.loadcorestate(replay_state);queue={};pulse("C")end
  if frames>=config.maxFrames then status("timeout");finish(1)end
  local b=nil;if stage=="cheat" then local p=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS");if p>=0x28FF0 and p<0x29000 then b=names[cheat[p-0x28FF0+1]] elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then b="Down"end elseif #queue>0 then b=table.remove(queue,1)end
  set_button(b);joypad.set({},2);emu.frameadvance()
end
