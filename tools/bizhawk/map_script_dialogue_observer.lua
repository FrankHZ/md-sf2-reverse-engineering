local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local stage,prompt_count,case_index="cheat",0,1
local queue,records={},{}
local replay_state,pending_save,pending_replay,pending_finish=nil,false,false,false
local active,entered,returned=false,false,false
local start_sp,pending_call=nil,nil
local calls,state_writes,skip_branch_taken,handler_entry_pc,handler_return_pc,pending_state_write={}, {}, nil, nil, nil, nil
local json_null={}
local names={[1]="Up",[2]="Down",[4]="Left",[8]="Right",[16]="B",[32]="C"}
local cheat={1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4}

local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local b={};if name and name~="" then b[name]=true end;joypad.set(b,1) end
local function current_case() return config.cases[case_index] end
local function word(value) return value&0xFFFF end
local function bool(value) if value then return "true" end return "false" end
local function quote(value) return string.format("%q",value) end
local function is_array(value) local n=0;for key,_ in pairs(value) do if type(key)~="number" then return false end;n=n+1 end;for i=1,n do if value[i]==nil then return false end end;return true end
local function json(value)
  local kind=type(value);if value==nil or value==json_null then return "null" end;if kind=="boolean" then return bool(value) end;if kind=="number" then return tostring(value) end;if kind=="string" then return quote(value) end;if kind~="table" then error("dialogue JSON type drift: "..kind) end
  local parts={};if is_array(value) then for _,item in ipairs(value) do parts[#parts+1]=json(item) end;return "["..table.concat(parts,",").."]" end
  for key,item in pairs(value) do parts[#parts+1]=quote(key)..":"..json(item) end;return "{"..table.concat(parts,",").."}"
end

local function reset()
  entered=false;returned=false;start_sp=nil;pending_call=nil;calls={};state_writes={};skip_branch_taken=nil;handler_entry_pc=nil;handler_return_pc=nil;pending_state_write=nil
end

local function setup_case()
  local case=current_case();if case==nil then error("dialogue unexpected map-setup injection") end
  local input=config.instrumentation.ramInputAddress
  memory.write_u32_be(input,case.handlerAddress,"M68K BUS")
  for offset=0,4,2 do memory.write_u16_be(input+config.instrumentation.scriptInputRamOffset+offset,0,"M68K BUS") end
  for index,value in ipairs(case.inputWords) do memory.write_u16_be(input+config.instrumentation.scriptInputRamOffset+(index-1)*2,value,"M68K BUS") end
  memory.write_u8(config.ram.SKIP_CUTSCENE_TEXT,case.skipFlagByteSeed,"M68K BUS")
  memory.write_u16_be(input+config.instrumentation.speechSfxSeedRamOffset,config.instrumentation.speechSfxWordSeed,"M68K BUS")
  memory.write_u16_be(config.ram.CUTSCENE_DIALOG_INDEX,config.instrumentation.stateSeeds.dialogueIndexWord,"M68K BUS")
  memory.write_u16_be(config.ram.CURRENT_SPEECH_SFX,config.instrumentation.stateSeeds.currentSpeechSfxWord,"M68K BUS")
  memory.write_u16_be(config.ram.DIALOGUE_NAME_INDEX_1,config.instrumentation.stateSeeds.nameIndex1Word,"M68K BUS")
  memory.write_u16_be(config.ram.DIALOGUE_NAME_INDEX_2,config.instrumentation.stateSeeds.nameIndex2Word,"M68K BUS")
  reset();active=true;status("milestone:case:"..case.id)
end

local function at_handler(address)
  local pc=emu.getregister("M68K PC")
  if not active or current_case().handlerAddress~=address or pc~=address then return end
  if config.observedIdentity.handlerEntryByAddress[tostring(pc)]==nil then error("dialogue handler-entry identity drift") end
  entered=true;handler_entry_pc=pc;start_sp=emu.getregister("M68K SP");status("milestone:handler-entry:"..current_case().id)
end

local function at_call(address)
  if not active then return end
  local pc=emu.getregister("M68K PC")
  if pc~=address or pending_call~=nil then error("dialogue call chronology drift") end
  local call=config.observedIdentity.callSiteByAddress[tostring(pc)]
  if call==nil or call.handlerEntryAddress~=handler_entry_pc then error("dialogue observed call-site identity drift") end
  pending_call={
    callIdentity=call,callSiteAddressObserved=pc,targetEntryAddressObserved=nil,returnAddressObserved=nil,
    instructionTargetObserved=nil,effectiveTargetObserved=nil,
    registerWordsObserved={word(emu.getregister("M68K D0")),word(emu.getregister("M68K D1")),word(emu.getregister("M68K D2"))}
  }
end

local function at_target(address)
  if not active or pending_call==nil then return end
  local pc=emu.getregister("M68K PC")
  if pc~=address then error("dialogue target-entry PC drift") end
  local target=config.observedIdentity.targetEntryByAddress[tostring(pc)]
  if target==nil or pending_call.callIdentity.targetEntryAddress~=pc then error("dialogue observed target-entry identity drift") end
  if target.instructionTarget~=pending_call.callIdentity.instructionTarget or target.effectiveTarget~=pending_call.callIdentity.effectiveTarget then error("dialogue instruction/effective target identity drift") end
  pending_call.instructionTargetObserved=target.instructionTarget;pending_call.effectiveTargetObserved=target.effectiveTarget;pending_call.targetEntryAddressObserved=pc
end

local function at_return(address)
  if not active or pending_call==nil then return end
  local pc=emu.getregister("M68K PC")
  if pending_call.targetEntryAddressObserved==nil or config.observedIdentity.returnAddressByAddress[tostring(pc)]==nil or pending_call.callIdentity.returnAddress~=pc or pc~=address then error("dialogue callback return chronology drift") end
  pending_call.returnAddressObserved=pc;pending_call.callIdentity=nil
  calls[#calls+1]=pending_call;pending_call=nil
end

local function at_call_or_return(address)
  if not active then return end
  local pc=emu.getregister("M68K PC")
  if pending_call~=nil and pending_call.callIdentity.returnAddress==pc then at_return(address) end
  if config.observedIdentity.callSiteByAddress[tostring(pc)]~=nil then at_call(address) end
end

local function at_state_write(address)
  if not active or emu.getregister("M68K PC")~=address then return end
  -- A following instruction can share the preceding instruction's resume PC.
  -- Complete the old write before arming the new one at that one execution
  -- event; two independently registered callbacks have no stable ordering.
  if pending_state_write~=nil and pending_state_write.resumeAddress==address then
    state_writes[#state_writes+1]={kindObserved=pending_state_write.kind,resumeAddressObserved=address,wordValueObserved=memory.read_u16_be(pending_state_write.address,"M68K BUS")}
    pending_state_write=nil
  end
  local info=config.stateWrites.byInstruction[tostring(address)]
  if info~=nil then
    if pending_state_write~=nil then error("dialogue state-write source chronology drift") end
    pending_state_write=info
  end
end

local function at_skip_target(address)
  if not active or emu.getregister("M68K PC")~=address then return end
  if current_case().handlerAddress==config["function"].csc00_displaySingleTextboxAddress or current_case().handlerAddress==config["function"].csc02_displayTextboxAddress then skip_branch_taken=true end
end

local function at_handler_return(address)
  if not active or current_case().handlerAddress==nil or emu.getregister("M68K PC")~=address then return end
  returned=true;handler_return_pc=emu.getregister("M68K PC")
end

local function append_record()
  local case=current_case();if not entered or not returned or pending_call~=nil or pending_state_write~=nil then error("dialogue handler completion drift") end
  for _,call in ipairs(calls) do if call.targetEntryAddressObserved==nil or call.returnAddressObserved==nil then error("dialogue incomplete call observation") end end
  local counts={};for _,target in ipairs(config.observedIdentity.effectiveTargets) do counts[target]=0 end
  local observed_calls,registers={},{}
  for _,call in ipairs(calls) do
    observed_calls[#observed_calls+1]={instructionTargetObserved=call.instructionTargetObserved,effectiveTargetObserved=call.effectiveTargetObserved,callSiteAddressObserved=call.callSiteAddressObserved,targetEntryAddressObserved=call.targetEntryAddressObserved,returnAddressObserved=call.returnAddressObserved}
    registers[#registers+1]=call.registerWordsObserved;counts[call.effectiveTargetObserved]=counts[call.effectiveTargetObserved]+1
  end
  local skip=json_null
  if case.handlerAddress==config["function"].csc00_displaySingleTextboxAddress or case.handlerAddress==config["function"].csc02_displayTextboxAddress then skip=(skip_branch_taken==true) end
  local record={
    id=case.id,handlerEntryPcObserved=handler_entry_pc,handlerReturnPcObserved=handler_return_pc,handlerReturned=returned,
    scriptCursorRamOffsetAfterObserved=emu.getregister("M68K A6")-config.instrumentation.ramInputAddress,
    stackPointerDeltaBytesObserved=emu.getregister("M68K SP")-start_sp,skipFlagBranchTakenObserved=skip,
    directCallsObserved=observed_calls,effectiveTargetCountsObserved=counts,directCallRegisterWordsObserved=registers,stateWritesObserved=state_writes
  }
  if record.handlerReturnPcObserved==nil then error("dialogue handler return PC identity drift") end
  records[#records+1]=record
end

local function finish(code)
  if replay_state then memorysavestate.removestate(replay_state) end;if code~=0 then client.exitCode(code);return end
  local result={system=emu.getsystemid(),core="Genesis Plus GX",id=config.fixtureId,mapTest=config.mapTestIndex,recordOrder={},records=records};for _,case in ipairs(config.cases) do result.recordOrder[#result.recordOrder+1]=case.id end
  local f=assert(io.open(config.outputPath,"w"));f:write(json(result).."\n");f:close();client.exitCode(0)
end

event.on_bus_exec(function() prompt_count=prompt_count+1;if prompt_count==1 then stage="map";pending_save=true;pulse("C") end end,config.harness["function"].numberPromptAddress,"dialogue-number","M68K BUS")
event.on_bus_exec(function() pulse("B") end,config.harness["function"].flagPromptAddress,"dialogue-flag","M68K BUS")
event.on_bus_exec(setup_case,config["function"].entryAddress,"dialogue-entry","M68K BUS")
for address,_ in pairs(config.observedIdentity.handlerEntryByAddress) do local a=tonumber(address);event.on_bus_exec(function() at_handler(a) end,a,"dialogue-handler-"..a,"M68K BUS") end
local seen_call_callbacks={}
for address,_ in pairs(config.observedIdentity.callSiteByAddress) do seen_call_callbacks[tonumber(address)]=true end
for address,_ in pairs(config.observedIdentity.returnAddressByAddress) do seen_call_callbacks[tonumber(address)]=true end
for address,_ in pairs(seen_call_callbacks) do local a=address;event.on_bus_exec(function() at_call_or_return(a) end,a,"dialogue-call-or-return-"..a,"M68K BUS") end
for address,_ in pairs(config.observedIdentity.targetEntryByAddress) do local a=tonumber(address);event.on_bus_exec(function() at_target(a) end,a,"dialogue-target-"..a,"M68K BUS") end
local seen_state_writes={}
for address,_ in pairs(config.stateWrites.byInstruction) do seen_state_writes[tonumber(address)]=true end
for address,_ in pairs(config.stateWrites.byResume) do seen_state_writes[tonumber(address)]=true end
for address,_ in pairs(seen_state_writes) do local a=address;event.on_bus_exec(function() at_state_write(a) end,a,"dialogue-state-write-"..a,"M68K BUS") end
for _,field in ipairs({"csc00_displaySingleTextboxSkipBranchTargetAddress","csc02_displayTextboxSkipBranchTargetAddress"}) do local a=config["function"][field];event.on_bus_exec(function() at_skip_target(a) end,a,"dialogue-skip-"..a,"M68K BUS") end
for _,case in ipairs(config.cases) do
  for key,address in pairs(config["function"]) do if key:sub(-13)=="ReturnAddress" and address==case.handlerAddress then error("dialogue return address aliases handler entry") end end
end
for _,field in ipairs({"csc00_displaySingleTextboxReturnAddress","csc01_displaySingleTextboxWithVarsReturnAddress","csc02_displayTextboxReturnAddress","csc03_displayTextboxWithVarsReturnAddress","csc04_setTextIndexReturnAddress","csc09_hideDialogueAndPortraitWindowsReturnAddress"}) do local a=config["function"][field];event.on_bus_exec(function() at_handler_return(a) end,a,"dialogue-return-"..a,"M68K BUS") end
event.on_bus_exec(function() if active then append_record();active=false;case_index=case_index+1;if case_index>#config.cases then pending_finish=true else pending_replay=true end end end,config.instrumentation.postHandlerAddress,"dialogue-post","M68K BUS")

status("milestone:observer-ready")
local frames=0
while true do
  frames=frames+1;if pending_finish then finish(0) elseif pending_save then pending_save=false;replay_state=memorysavestate.savecorestate() elseif pending_replay then pending_replay=false;memorysavestate.loadcorestate(replay_state);queue={};pulse("C") end
  if frames>=config.maxFrames then status("timeout:frame-budget-exhausted:case="..case_index);finish(1) end
  local button=nil;if stage=="cheat" then local pointer=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS");if pointer>=0x28FF0 and pointer<0x29000 then button=names[cheat[pointer-0x28FF0+1]] elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Down" end elseif #queue>0 then button=table.remove(queue,1) end
  set_button(button);joypad.set({},2);emu.frameadvance()
end
