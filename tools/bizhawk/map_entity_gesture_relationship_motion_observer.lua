local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local stage,prompt_count,case_index="cheat",0,1
local queue,records={},{}
local replay_state,pending_save,pending_replay,pending_finish=nil,false,false,false
local active,handler_entered=false,false
local callback_targets={}
local cursor_adjust_seen,add_follower_words,first_wait_state,wait_exit_forced=false,nil,nil,false
local source_local=nil
local json_null={}
local names={[1]="Up",[2]="Down",[4]="Left",[8]="Right",[16]="B",[32]="C"}
local cheat={1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4}
local state_fields={"xWord","yWord","xDest","yDest","xTravel","yTravel","xVelocity","yVelocity","facingByte","layerByte","animCounterByte","flagsBByte"}
local layout_names={xWord="xWord",yWord="yWord",xDest="xDest",yDest="yDest",xTravel="xTravel",yTravel="yTravel",xVelocity="xVelocity",yVelocity="yVelocity",facingByte="facing",layerByte="layer",animCounterByte="animCounter",flagsBByte="flagsB"}
local case_keys={"id","macro","scriptWords","entityIndexMappings","entityStateSeeds","currentHpWordSeed","spriteSizeWordSeed","expected","runtimeGolden"}
local seed_keys={"entityIndex","xWord","yWord","xDest","yDest","xTravel","yTravel","xVelocity","yVelocity","facingByte","layerByte","animCounterByte","flagsBByte"}
local mapping_keys={"character","entityIndex"}
local plan_keys={"instructionTarget","callSiteAddress","targetAddress"}
local probe_keys={"sourceHelperInvoked","firstScriptWordByteOffset","firstScriptWordByteLane","characterByte","storageAddress","storageTransferByteCount"}
local source_local_keys={"faceFacingByteAtUpdateCall","flyLayerByteAfterWrite","moveFirstWaitState","nodFinalAnimCounterByteAfterWrite","shiverTemporarySpriteSizeWordAfterWrite","shiverTemporaryAnimCounterByteAfterWrite","shiverRestoredSpriteSizeWordAfterWrite","shiverRestoredAnimCounterByteAfterWrite","shiverFlagsSetWriteCount","shiverFlagsClearWriteCount","shiverFlagsBitSetAfterWrite","shiverFlagsBitClearAfterWrite"}
local derived_keys={"id","handlerAddress","scriptCursorRamOffsetAfter","directCallbackPlan","effectiveCallbackPlan","sourceLocal"}
local output_record_keys={"id","handlerAddress","scriptCursorRamOffsetAfter","directCallbackPlan","effectiveCallbackPlan","currentHpSeedProbe","sourceLocal","callbackTargetOrderObserved","handlerReturned","entityStateAfter","spriteSizeWordAfter","aliveStatusCursorAdjustmentInstructionObserved","addFollowerRegisterWordsObserved","moveWaitExitForcedObserved"}

local function assert_closed_keys(value,expected,name)
  if type(value)~="table" then error("entity gesture "..name.." must be a table") end
  local declared={}
  for _,key in ipairs(expected) do
    declared[key]=true
    if value[key]==nil then error("entity gesture "..name.." missing key: "..key) end
  end
  for key,_ in pairs(value) do if not declared[key] then error("entity gesture "..name.." forbidden key: "..tostring(key)) end end
end

local function assert_closed_keys_optional(value,required,optional,name)
  if type(value)~="table" then error("entity gesture "..name.." must be a table") end
  local declared={}
  for _,key in ipairs(required) do declared[key]=true;if value[key]==nil then error("entity gesture "..name.." missing key: "..key) end end
  for _,key in ipairs(optional) do declared[key]=true end
  for key,_ in pairs(value) do if not declared[key] then error("entity gesture "..name.." forbidden key: "..tostring(key)) end end
end

local function assert_plan(plan,name)
  for index,row in ipairs(plan) do assert_closed_keys(row,plan_keys,name..":"..index) end
end

local function same_value(left,right)
  if type(left)~=type(right) then return false end
  if type(left)~="table" then return left==right end
  for key,value in pairs(left) do if not same_value(value,right[key]) then return false end end
  for key,_ in pairs(right) do if left[key]==nil then return false end end
  return true
end

assert_closed_keys(config,{"fixtureId","mapTestIndex","function","ram","constants","instrumentation","maxFrames","harness","cases","derived","callbackHooks","outputPath","statusPath"},"config")
assert_closed_keys(config["function"],{"csc2A_entityShiverAddress","csc26_entityNodHeadAddress","csc2C_followEntityAddress","csc52_faceEntityAddress","csc28_moveEntityNextToPlayerAddress","csc2F_flyAddress","csc31_moveEntityAboveEntityAddress","runMapSetupInitFunctionAddress","aliveStatusCursorAdjustmentAddress","shiverTemporarySizeWriteAddress","shiverTemporarySizeAfterWriteAddress","shiverRestoredSizeAfterWriteAddress","shiverFlagsSetAddress","shiverFlagsSetAfterWriteAddress","shiverFlagsClearAddress","shiverFlagsClearAfterWriteAddress","shiverTemporaryAnimCounterAfterWriteAddress","shiverRestoredAnimCounterAfterWriteAddress","nodInitialSleepCallSiteAddress","nodFinalAnimCounterAfterWriteAddress","faceUpdateCallSiteAddress","moveNextFirstWaitCallSiteAddress","flyZeroLayerAfterWriteAddress","flyNonzeroLayerAfterWriteAddress"},"function")
assert_closed_keys(config.ram,{"combatantDataAddress","entityDataAddress","entityIndexListAddress","spriteSizeWordAddress"},"ram")
assert_closed_keys(config.instrumentation,{"callSiteAddress","callSiteOriginalHex","callSitePatchedHex","stubAddress","stubOriginalHex","stubHex","postHandlerAddress","ramInputAddress","scriptInputRamOffset"},"instrumentation")
for index,case in ipairs(config.cases) do
  assert_closed_keys(case,case_keys,"case:"..index)
  for mapping_index,row in ipairs(case.entityIndexMappings) do assert_closed_keys(row,mapping_keys,"mapping:"..index..":"..mapping_index) end
  for seed_index,row in ipairs(case.entityStateSeeds) do assert_closed_keys(row,seed_keys,"seed:"..index..":"..seed_index) end
end
for index,derived in ipairs(config.derived) do
  assert_closed_keys_optional(derived,derived_keys,{"currentHpSeedProbe"},"derived:"..index)
  assert_plan(derived.directCallbackPlan,"direct plan:"..index)
  assert_plan(derived.effectiveCallbackPlan,"effective plan:"..index)
  if derived.currentHpSeedProbe~=nil then assert_closed_keys(derived.currentHpSeedProbe,probe_keys,"current-HP probe:"..index) end
end
for index,hook in ipairs(config.callbackHooks) do assert_closed_keys(hook,{"callSiteAddress","instructionTarget"},"callback hook:"..index) end

local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local buttons={};if name and name~="" then buttons[name]=true end;joypad.set(buttons,1) end
local function current_case() return config.cases[case_index] end
local function current_derived() return config.derived[case_index] end
local function boolean(value) if value then return "true" end;return "false" end
local function json_string(value) return string.format("%q",value) end
local function is_array(value) local count=0;for key,_ in pairs(value) do if type(key)~="number" then return false end;count=count+1 end;for index=1,count do if value[index]==nil then return false end end;return true end
local function json(value)
  local kind=type(value);if value==nil or value==json_null then return "null" end;if kind=="boolean" then return boolean(value) end;if kind=="number" then return tostring(value) end;if kind=="string" then return json_string(value) end;if kind~="table" then error("entity gesture JSON value type drift: "..kind) end
  local parts={}
  if is_array(value) then
    for index,item in ipairs(value) do parts[#parts+1]=json(item) end
    return "["..table.concat(parts,",").."]"
  end
  for key,item in pairs(value) do parts[#parts+1]=json_string(key)..":"..json(item) end
  return "{"..table.concat(parts,",").."}"
end
local function copy(value) if type(value)~="table" then return value end;local result={};for key,item in pairs(value) do result[key]=copy(item) end;return result end
local function value_or_json_null(value) if value==nil then return json_null end;return value end
local function source_local_record()
  return {faceFacingByteAtUpdateCall=json_null,flyLayerByteAfterWrite=json_null,moveFirstWaitState=json_null,nodFinalAnimCounterByteAfterWrite=json_null,shiverTemporarySpriteSizeWordAfterWrite=json_null,shiverTemporaryAnimCounterByteAfterWrite=json_null,shiverRestoredSpriteSizeWordAfterWrite=json_null,shiverRestoredAnimCounterByteAfterWrite=json_null,shiverFlagsSetWriteCount=0,shiverFlagsClearWriteCount=0,shiverFlagsBitSetAfterWrite=json_null,shiverFlagsBitClearAfterWrite=json_null}
end
local function write_width(address,width,value) if width==1 then memory.write_u8(address,value,"M68K BUS") elseif width==2 then memory.write_u16_be(address,value,"M68K BUS") else error("entity gesture write width drift: "..width) end end
local function read_width(address,width) if width==1 then return memory.read_u8(address,"M68K BUS") elseif width==2 then return memory.read_u16_be(address,"M68K BUS") else error("entity gesture read width drift: "..width) end end
local function entity_address(index) return config.ram.entityDataAddress+index*config.constants.entityRecordByteCount end
local function field_layout(name) return config.constants.entityFieldLayouts[layout_names[name]] end
local function write_state(seed)
  local base=entity_address(seed.entityIndex)
  for _,name in ipairs(state_fields) do local field=field_layout(name);write_width(base+field.byteOffset,field.transferByteCount,seed[name]) end
end
local function read_state(index)
  local result={entityIndex=index};local base=entity_address(index)
  for _,name in ipairs(state_fields) do local field=field_layout(name);result[name]=read_width(base+field.byteOffset,field.transferByteCount) end
  return result
end
local function write_field(index,name,value)
  local field=field_layout(name);write_width(entity_address(index)+field.byteOffset,field.transferByteCount,value)
end
local function find_primary_index(case)
  local character=case.scriptWords[1]
  for _,row in ipairs(case.entityIndexMappings) do if row.character==character then return row.entityIndex end end
  error("entity gesture primary mapping drift")
end
local function setup_case(case,derived)
  local input=config.instrumentation.ramInputAddress;local script=input+config.instrumentation.scriptInputRamOffset
  for _,row in ipairs(case.entityIndexMappings) do memory.write_u8(config.ram.entityIndexListAddress+row.character,row.entityIndex,"M68K BUS") end
  for index,value in ipairs(case.scriptWords) do memory.write_u16_be(script+(index-1)*2,value,"M68K BUS") end
  local probe=derived.currentHpSeedProbe
  if probe~=nil then
    local hp_character=memory.read_u8(script,"M68K BUS")
    if hp_character~=probe.characterByte then error("entity gesture current-HP byte-probe character drift") end
    local hp=config.ram.combatantDataAddress+(hp_character&config.constants.combatantMaskAll)*config.constants.combatantEntryByteCount+config.constants.currentHpByteOffset
    if hp~=probe.storageAddress then error("entity gesture current-HP probe storage-address drift") end
    write_width(hp,probe.storageTransferByteCount,case.currentHpWordSeed)
  end
  for _,seed in ipairs(case.entityStateSeeds) do write_state(seed) end
  memory.write_u16_be(config.ram.spriteSizeWordAddress,case.spriteSizeWordSeed,"M68K BUS")
  memory.write_u32_be(input,derived.handlerAddress,"M68K BUS")
end
local function begin_case()
  if active then return end
  local case=current_case();if case==nil then error("entity gesture unexpected trampoline entry") end
  setup_case(case,current_derived());active=true;handler_entered=false;callback_targets={};cursor_adjust_seen=false;add_follower_words=nil;first_wait_state=nil;wait_exit_forced=false;source_local=source_local_record()
  status("milestone:case:"..case.id)
end
local function handler(macro)
  if not active then return end
  if current_case().macro~=macro or emu.getregister("M68K PC")~=current_derived().handlerAddress then error("entity gesture handler identity drift") end
  handler_entered=true;status("milestone:handler-entry:"..current_case().id)
end
local function observe_callback(target)
  if not active then return end
  callback_targets[#callback_targets+1]=target
  status("milestone:callback:"..target..":"..current_case().id)
end
local function observe_first_wait()
  if not active or current_case().macro~="moveNextToPlayer" or first_wait_state~=nil then return end
  local index=find_primary_index(current_case());local state=read_state(index)
  first_wait_state={xWord=state.xWord,yWord=state.yWord,xDest=state.xDest,yDest=state.yDest,xTravel=state.xTravel,yTravel=state.yTravel,xVelocity=state.xVelocity,yVelocity=state.yVelocity,facingByte=state.facingByte}
  source_local.moveFirstWaitState=first_wait_state
  write_field(index,"xWord",state.xDest);write_field(index,"yWord",state.yDest);wait_exit_forced=true
end
local function observe_add_follower()
  if not active then return end
  add_follower_words={d0Word=emu.getregister("M68K D0")&0xFFFF,d1Word=emu.getregister("M68K D1")&0xFFFF,d2Word=emu.getregister("M68K D2")&0xFFFF,d3Word=emu.getregister("M68K D3")&0xFFFF}
end
local function append_record()
  local case,derived=current_case(),current_derived();if not handler_entered then error("entity gesture handler did not execute") end
  local record=copy(derived);record.handlerReturned=true;record.callbackTargetOrderObserved=callback_targets
  if #callback_targets~=#derived.effectiveCallbackPlan then error("entity gesture effective callback count drift") end
  for index,target_record in ipairs(derived.effectiveCallbackPlan) do if callback_targets[index]~=target_record.instructionTarget then error("entity gesture effective callback order drift") end end
  record.entityStateAfter={};for _,seed in ipairs(case.entityStateSeeds) do record.entityStateAfter[#record.entityStateAfter+1]=read_state(seed.entityIndex) end
  record.spriteSizeWordAfter=memory.read_u16_be(config.ram.spriteSizeWordAddress,"M68K BUS")
  record.currentHpSeedProbe=value_or_json_null(derived.currentHpSeedProbe)
  record.sourceLocal=source_local
  for _,key in ipairs(source_local_keys) do
    local expected_source_value=derived.sourceLocal[key]
    if expected_source_value~=nil and not same_value(source_local[key],expected_source_value) then error("entity gesture source-local probe drift: "..key) end
  end
  record.aliveStatusCursorAdjustmentInstructionObserved=cursor_adjust_seen;record.addFollowerRegisterWordsObserved=value_or_json_null(add_follower_words);record.moveWaitExitForcedObserved=wait_exit_forced
  assert_closed_keys(record,output_record_keys,"output record:"..case.id)
  assert_plan(record.directCallbackPlan,"output direct plan:"..case.id)
  assert_plan(record.effectiveCallbackPlan,"output effective plan:"..case.id)
  for state_index,row in ipairs(record.entityStateAfter) do assert_closed_keys(row,seed_keys,"output state:"..case.id..":"..state_index) end
  assert_closed_keys(record.sourceLocal,source_local_keys,"source-local output:"..case.id)
  local offset=emu.getregister("M68K A6")-config.instrumentation.ramInputAddress;if offset~=derived.scriptCursorRamOffsetAfter then error("entity gesture script cursor drift: "..offset) end
  records[#records+1]=record
end
local function finish(code)
  if replay_state then memorysavestate.removestate(replay_state) end;if code~=0 then client.exitCode(code);return end
  local result={system=emu.getsystemid(),core="Genesis Plus GX",id=config.fixtureId,mapTest=config.mapTestIndex,recordOrder={},records=records};for _,case in ipairs(config.cases) do result.recordOrder[#result.recordOrder+1]=case.id end
  local file=assert(io.open(config.outputPath,"w"));file:write(json(result).."\n");file:close();client.exitCode(0)
end

event.on_bus_exec(function() prompt_count=prompt_count+1;status("milestone:number-prompt-entry:"..prompt_count);if prompt_count==1 then stage="map";pending_save=true;pulse("C") end end,config.harness["function"].numberPromptAddress,"entity-gesture-number","M68K BUS")
event.on_bus_exec(function() status("milestone:flag-prompt-entry");pulse("B") end,config.harness["function"].flagPromptAddress,"entity-gesture-flag","M68K BUS")
event.on_bus_exec(begin_case,config["function"].runMapSetupInitFunctionAddress,"entity-gesture-entry","M68K BUS")
event.on_bus_exec(function() handler("shiver") end,config["function"].csc2A_entityShiverAddress,"entity-gesture-shiver","M68K BUS")
event.on_bus_exec(function() handler("nod") end,config["function"].csc26_entityNodHeadAddress,"entity-gesture-nod","M68K BUS")
event.on_bus_exec(function() handler("followEntity") end,config["function"].csc2C_followEntityAddress,"entity-gesture-follow","M68K BUS")
event.on_bus_exec(function() handler("faceEntity") end,config["function"].csc52_faceEntityAddress,"entity-gesture-face","M68K BUS")
event.on_bus_exec(function() handler("moveNextToPlayer") end,config["function"].csc28_moveEntityNextToPlayerAddress,"entity-gesture-move","M68K BUS")
event.on_bus_exec(function() handler("fly") end,config["function"].csc2F_flyAddress,"entity-gesture-fly","M68K BUS")
event.on_bus_exec(function() handler("moveEntityAboveAnother") end,config["function"].csc31_moveEntityAboveEntityAddress,"entity-gesture-above","M68K BUS")
for _,hook in ipairs(config.callbackHooks) do local item=hook;event.on_bus_exec(function() observe_callback(item.instructionTarget) end,item.callSiteAddress,"entity-gesture-callback-"..item.callSiteAddress,"M68K BUS") end
event.on_bus_exec(function() if active and current_case().macro=="shiver" then source_local.shiverTemporarySpriteSizeWordAfterWrite=memory.read_u16_be(config.ram.spriteSizeWordAddress,"M68K BUS") end end,config["function"].shiverTemporarySizeAfterWriteAddress,"entity-gesture-shiver-size-temporary","M68K BUS")
event.on_bus_exec(function() if active and current_case().macro=="shiver" then source_local.shiverRestoredSpriteSizeWordAfterWrite=memory.read_u16_be(config.ram.spriteSizeWordAddress,"M68K BUS") end end,config["function"].shiverRestoredSizeAfterWriteAddress,"entity-gesture-shiver-size-restored","M68K BUS")
event.on_bus_exec(function() if active and current_case().macro=="shiver" then source_local.shiverTemporaryAnimCounterByteAfterWrite=read_state(find_primary_index(current_case())).animCounterByte end end,config["function"].shiverTemporaryAnimCounterAfterWriteAddress,"entity-gesture-shiver-anim-temporary","M68K BUS")
event.on_bus_exec(function() if active and current_case().macro=="shiver" then source_local.shiverRestoredAnimCounterByteAfterWrite=read_state(find_primary_index(current_case())).animCounterByte end end,config["function"].shiverRestoredAnimCounterAfterWriteAddress,"entity-gesture-shiver-anim-restored","M68K BUS")
event.on_bus_exec(function() if active and current_case().macro=="shiver" then source_local.shiverFlagsSetWriteCount=source_local.shiverFlagsSetWriteCount+1;source_local.shiverFlagsBitSetAfterWrite=(read_state(find_primary_index(current_case())).flagsBByte&8)~=0 end end,config["function"].shiverFlagsSetAfterWriteAddress,"entity-gesture-shiver-set","M68K BUS")
event.on_bus_exec(function() if active and current_case().macro=="shiver" then source_local.shiverFlagsClearWriteCount=source_local.shiverFlagsClearWriteCount+1;source_local.shiverFlagsBitClearAfterWrite=(read_state(find_primary_index(current_case())).flagsBByte&8)~=0 end end,config["function"].shiverFlagsClearAfterWriteAddress,"entity-gesture-shiver-clear","M68K BUS")
event.on_bus_exec(function() if active and current_case().macro=="nod" then source_local.nodFinalAnimCounterByteAfterWrite=read_state(find_primary_index(current_case())).animCounterByte end end,config["function"].nodFinalAnimCounterAfterWriteAddress,"entity-gesture-nod-final-anim","M68K BUS")
event.on_bus_exec(function() if active and current_case().macro=="faceEntity" then source_local.faceFacingByteAtUpdateCall=read_state(find_primary_index(current_case())).facingByte end end,config["function"].faceUpdateCallSiteAddress,"entity-gesture-face-facing","M68K BUS")
event.on_bus_exec(function() if active and current_case().macro=="fly" then source_local.flyLayerByteAfterWrite=read_state(find_primary_index(current_case())).layerByte end end,config["function"].flyZeroLayerAfterWriteAddress,"entity-gesture-fly-zero","M68K BUS")
event.on_bus_exec(function() if active and current_case().macro=="fly" then source_local.flyLayerByteAfterWrite=read_state(find_primary_index(current_case())).layerByte end end,config["function"].flyNonzeroLayerAfterWriteAddress,"entity-gesture-fly-nonzero","M68K BUS")
event.on_bus_exec(function() if active then cursor_adjust_seen=true end end,config["function"].aliveStatusCursorAdjustmentAddress,"entity-gesture-alive-cursor","M68K BUS")
event.on_bus_exec(observe_first_wait,config["function"].moveNextFirstWaitCallSiteAddress,"entity-gesture-move-wait","M68K BUS")
for _,hook in ipairs(config.callbackHooks) do if hook.instructionTarget=="AddFollower" then local item=hook;event.on_bus_exec(observe_add_follower,item.callSiteAddress,"entity-gesture-add-"..item.callSiteAddress,"M68K BUS") end end
event.on_bus_exec(function() if not active then return end;append_record();active=false;case_index=case_index+1;if case_index>#config.cases then pending_finish=true else pending_replay=true end end,config.instrumentation.postHandlerAddress,"entity-gesture-return","M68K BUS")

local frames=0
while true do
  frames=frames+1
  if pending_finish then finish(0) elseif pending_save then pending_save=false;replay_state=memorysavestate.savecorestate();status("milestone:saved-map-prompt") elseif pending_replay then pending_replay=false;memorysavestate.loadcorestate(replay_state);queue={};pulse("C");status("milestone:replay-map-prompt") end
  if frames>=config.maxFrames then status("timeout:frame-budget-exhausted:case="..case_index);finish(1) end
  local button=nil;if stage=="cheat" then local pointer=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS");if pointer>=0x28FF0 and pointer<0x29000 then button=names[cheat[pointer-0x28FF0+1]] elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Down" end elseif #queue>0 then button=table.remove(queue,1) end
  set_button(button);joypad.set({},2);emu.frameadvance()
end
