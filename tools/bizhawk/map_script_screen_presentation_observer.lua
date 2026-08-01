local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local stage,prompt_count,case_index="cheat",0,1
local queue,records={},{}
local replay_state,pending_save,pending_replay,pending_finish=nil,false,false,false
local active,entered,returned=false,false,false
local start_sp,pending=nil,nil
local calls,quake_writes,fade_reads,fade_writes={},nil,nil,nil
local flash_shift,flash_dispatches,entry_pc,return_pc=nil,nil,nil,nil
local json_null={}
local names={[1]="Up",[2]="Down",[4]="Left",[8]="Right",[16]="B",[32]="C"}
local cheat={1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4}

local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local b={};if name and name~="" then b[name]=true end;joypad.set(b,1) end
local function current_case() return config.cases[case_index] end
local function derived() return config.derived[case_index] end
local function word(value) return value&0xFFFF end
local function boolean(value) if value then return "true" end return "false" end
local function quote(value) return string.format("%q",value) end
local function is_array(value) local n=0;for k,_ in pairs(value) do if type(k)~="number" then return false end;n=n+1 end;for i=1,n do if value[i]==nil then return false end end;return true end
local function json(value)
  local kind=type(value);if value==nil or value==json_null then return "null" end;if kind=="boolean" then return boolean(value) end;if kind=="number" then return tostring(value) end;if kind=="string" then return quote(value) end;if kind~="table" then error("screen-presentation JSON type drift: "..kind) end
  local parts={};if is_array(value) then for _,item in ipairs(value) do parts[#parts+1]=json(item) end;return "["..table.concat(parts,",").."]" end
  for key,item in pairs(value) do parts[#parts+1]=quote(key)..":"..json(item) end;return "{"..table.concat(parts,",").."}" end
local function nullable(value) if value==nil then return json_null end return value end

local function reset()
  calls={};quake_writes=nil;fade_reads=nil;fade_writes=nil;flash_shift=nil;flash_dispatches=nil;pending=nil;entered=false;returned=false;start_sp=nil;entry_pc=nil;return_pc=nil
end
local function setup_case()
  local d=derived();if d==nil then error("screen-presentation unexpected trampoline entry") end
  memory.write_u32_be(config.instrumentation.ramInputAddress,d.handlerAddress,"M68K BUS")
  if d.handlerInputWord~=nil then memory.write_u16_be(config.instrumentation.ramInputAddress+config.instrumentation.scriptInputRamOffset,d.handlerInputWord,"M68K BUS") end
  memory.write_u8(config.constants.FADING_COUNTER_MAX,config.instrumentation.fadingCounterSeed,"M68K BUS")
  reset();active=true;status("milestone:case:"..d.id)
end
local function at_handler(address)
  if not active or derived().handlerAddress~=address or emu.getregister("M68K PC")~=address then return end
  entered=true;entry_pc=emu.getregister("M68K PC");start_sp=emu.getregister("M68K SP");status("milestone:handler-entry:"..derived().id)
end
local function at_call(address)
  if not active then return end
  if pending~=nil or emu.getregister("M68K PC")~=address then error("screen-presentation call-site capture drift") end
  pending={callSiteAddressObserved=emu.getregister("M68K PC"),targetAddressObserved=nil,returnAddressObserved=nil,effectiveTargetObserved=nil}
  pending.registerWordsObserved={word(emu.getregister("M68K D0")),word(emu.getregister("M68K D1")),word(emu.getregister("M68K D2"))}
end
local function at_target(address)
  if active and pending~=nil then
    if emu.getregister("M68K PC")~=address then error("screen-presentation service target PC drift") end
    pending.targetAddressObserved=emu.getregister("M68K PC");pending.effectiveTargetObserved=config.targetIdentityByAddress[tostring(address)]
    if pending.effectiveTargetObserved==nil then error("screen-presentation service identity resolution drift") end
    if derived().macro=="flashScreenWhite" and pending.effectiveTargetObserved=="LaunchFading" then flash_dispatches=(flash_dispatches or 0)+1 end
  end
end
local function at_return(address)
  if not active or pending==nil then return end
  if pending.targetAddressObserved==nil or emu.getregister("M68K PC")~=address then error("screen-presentation service return PC drift") end
  pending.returnAddressObserved=emu.getregister("M68K PC");calls[#calls+1]=pending;pending=nil
end
local function at_handler_return(address)
  if active and emu.getregister("M68K PC")==address then returned=true;return_pc=emu.getregister("M68K PC") end
end
local function write_quake()
  if active and derived().macro=="setQuake" then if quake_writes==nil then quake_writes={} end;quake_writes[#quake_writes+1]=memory.read_u16_be(config.constants.QUAKE_AMPLITUDE,"M68K BUS") end
end
local function read_slow()
  if active and (derived().macro=="slowFadeInB" or derived().macro=="slowFadeOutB") then if fade_reads==nil then fade_reads={} end;fade_reads[#fade_reads+1]=memory.read_u8(config.constants.FADING_COUNTER_MAX,"M68K BUS") end
end
local function write_slow()
  if active and (derived().macro=="slowFadeInB" or derived().macro=="slowFadeOutB") then if fade_writes==nil then fade_writes={} end;fade_writes[#fade_writes+1]=memory.read_u8(config.constants.FADING_COUNTER_MAX,"M68K BUS") end
end
local function read_flash_shift()
  if active and derived().macro=="flashScreenWhite" then flash_shift=word(emu.getregister("M68K D7")) end
end
local function append_record()
  local d=derived();if not entered or pending~=nil then error("screen-presentation handler completion drift") end
  for _,call in ipairs(calls) do if call.targetAddressObserved==nil or call.returnAddressObserved==nil or call.effectiveTargetObserved==nil then error("screen-presentation incomplete direct-call observation") end end
  local offset=emu.getregister("M68K A6")-config.instrumentation.ramInputAddress
  local observed={id=current_case().id,handlerEntryPcObserved=nullable(entry_pc),handlerReturnPcObserved=nullable(return_pc),handlerReturned=returned,scriptCursorRamOffsetAfterObserved=offset,stackPointerDeltaBytesObserved=emu.getregister("M68K SP")-start_sp,directCallsObserved={},directCallRegisterWordsObserved={},effectiveTargetCountsObserved={Sleep=0,FadeInFromBlack=0,FadeOutToBlack=0,LaunchFading=0,DuplicatePalettes=0}}
  for _,call in ipairs(calls) do
    observed.directCallsObserved[#observed.directCallsObserved+1]={effectiveTargetObserved=call.effectiveTargetObserved,callSiteAddressObserved=call.callSiteAddressObserved,targetAddressObserved=call.targetAddressObserved,returnAddressObserved=call.returnAddressObserved}
    observed.effectiveTargetCountsObserved[call.effectiveTargetObserved]=observed.effectiveTargetCountsObserved[call.effectiveTargetObserved]+1
    if #d.directCallRegisterWords>0 then observed.directCallRegisterWordsObserved[#observed.directCallRegisterWordsObserved+1]=call.registerWordsObserved end
  end
  observed.quakeAmplitudeWordWritesObserved=nullable(quake_writes);observed.fadingCounterByteReadsObserved=nullable(fade_reads);observed.fadingCounterByteWritesObserved=nullable(fade_writes);observed.flashDurationWordAfterShiftObserved=nullable(flash_shift);observed.flashLoopIterationCountObserved=nullable(flash_dispatches)
  records[#records+1]=observed
end
local function finish(code)
  if replay_state then memorysavestate.removestate(replay_state) end;if code~=0 then client.exitCode(code);return end
  local result={system=emu.getsystemid(),core="Genesis Plus GX",id=config.fixtureId,mapTest=config.mapTestIndex,recordOrder={},records=records};for _,case in ipairs(config.cases) do result.recordOrder[#result.recordOrder+1]=case.id end
  local f=assert(io.open(config.outputPath,"w"));f:write(json(result).."\n");f:close();client.exitCode(0)
end

event.on_bus_exec(function() prompt_count=prompt_count+1;if prompt_count==1 then stage="map";pending_save=true;pulse("C") end end,config.harness["function"].numberPromptAddress,"screen-number","M68K BUS")
event.on_bus_exec(function() pulse("B") end,config.harness["function"].flagPromptAddress,"screen-flag","M68K BUS")
event.on_bus_exec(setup_case,config["function"].entryAddress,"screen-entry","M68K BUS")
for _,d in ipairs(config.derived) do local address=d.handlerAddress;event.on_bus_exec(function() at_handler(address) end,address,"screen-handler-"..address,"M68K BUS") end
local seen_call,seen_return,seen_target,seen_handler={}, {}, {}, {}
for _,d in ipairs(config.derived) do
  for _,call in ipairs(d.directCallPlan) do
    if not seen_call[call.callSiteAddress] then local address=call.callSiteAddress;seen_call[address]=true;event.on_bus_exec(function() at_call(address) end,address,"screen-call-"..address,"M68K BUS") end
    if not seen_return[call.returnAddress] then local address=call.returnAddress;seen_return[address]=true;event.on_bus_exec(function() at_return(address) end,address,"screen-return-"..address,"M68K BUS") end
    if not seen_target[call.targetAddress] then local address=call.targetAddress;seen_target[address]=true;event.on_bus_exec(function() at_target(address) end,address,"screen-target-"..address,"M68K BUS") end
  end
end
for _,d in ipairs(config.derived) do local symbol=({setQuake="csc33_setQuakeAmount",fadeInB="csc39_fadeInFromBlack",fadeOutB="csc3A_fadeOutToBlack",slowFadeInB="csc3B_slowFadeInFromBlack",slowFadeOutB="csc3C_slowFadeOutToBlack",tintMap="csc3D_tintMap",flickerOnce="csc3E_FlickerOnce",mapFadeOutToWhite="csc3F_fadeMapOutToWhite",mapFadeInFromWhite="csc40_fadeMapInFromWhite",flashScreenWhite="csc41_flashScreenWhite",fadeInFromBlackHalf="csc4A_fadeInFromBlackHalf",fadeOutToBlackHalf="csc4B_fadeOutToBlackHalf"})[d.macro];local address=config["function"][symbol.."ReturnAddress"];if not seen_handler[address] then seen_handler[address]=true;event.on_bus_exec(function() if active and derived().handlerAddress==d.handlerAddress then at_handler_return(address) end end,address,"screen-handler-return-"..address,"M68K BUS") end end
event.on_bus_exec(write_quake,config["function"].quakeDirectWriteResumeAddress,"screen-quake-direct","M68K BUS")
event.on_bus_exec(write_quake,config["function"].quakeLoopWriteResumeAddress,"screen-quake-loop","M68K BUS")
event.on_bus_exec(read_slow,config["function"].slowFadeInReadResumeAddress,"screen-slow-in-read","M68K BUS")
event.on_bus_exec(write_slow,config["function"].slowFadeInSetResumeAddress,"screen-slow-in-set","M68K BUS")
event.on_bus_exec(write_slow,config["function"].slowFadeInRestoreResumeAddress,"screen-slow-in-restore","M68K BUS")
event.on_bus_exec(read_slow,config["function"].slowFadeOutReadResumeAddress,"screen-slow-out-read","M68K BUS")
event.on_bus_exec(write_slow,config["function"].slowFadeOutSetResumeAddress,"screen-slow-out-set","M68K BUS")
event.on_bus_exec(write_slow,config["function"].slowFadeOutRestoreResumeAddress,"screen-slow-out-restore","M68K BUS")
event.on_bus_exec(read_flash_shift,config["function"].flashOperandReadResumeAddress,"screen-flash-shift","M68K BUS")
event.on_bus_exec(function() if active then append_record();active=false;case_index=case_index+1;if case_index>#config.cases then pending_finish=true else pending_replay=true end end end,config.instrumentation.postHandlerAddress,"screen-post","M68K BUS")

status("milestone:observer-ready")
local frames=0
while true do
  frames=frames+1;if pending_finish then finish(0) elseif pending_save then pending_save=false;replay_state=memorysavestate.savecorestate() elseif pending_replay then pending_replay=false;memorysavestate.loadcorestate(replay_state);queue={};pulse("C") end
  if frames>=config.maxFrames then status("timeout:frame-budget-exhausted:case="..case_index);finish(1) end
  local button=nil;if stage=="cheat" then local pointer=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS");if pointer>=0x28FF0 and pointer<0x29000 then button=names[cheat[pointer-0x28FF0+1]] elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Down" end elseif #queue>0 then button=table.remove(queue,1) end
  set_button(button);joypad.set({},2);emu.frameadvance()
end
