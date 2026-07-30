local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local stage,prompt_count,case_index="cheat",0,1
local queue,records={},{},{}
local replay_state,pending_save,pending_replay,pending_finish=nil,false,false,false
local active,handler_entered=false,false
local callback_targets={}
local get_entity_seen,hide_seen,adjust_seen,cursor_adjust_seen,anim_counter_write_seen=false,false,false,false,false
local wait_compare_count,wait_back_edge_count,wait_for_vint_seen=0,0,false
local ally_callback_seen,update_seen,priority_nonzero_seen,priority_zero_seen=false,false,false,false
local load_seen,helper_seen,dma_seen,size_bit_seen=false,false,false,false
local temporary_size_word=nil
local names={[1]="Up",[2]="Down",[4]="Left",[8]="Right",[16]="B",[32]="C"}
local cheat={1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4}

local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local buttons={};if name and name~="" then buttons[name]=true end;joypad.set(buttons,1) end
local function current_case() return config.cases[case_index] end
local function current_derived() return config.derived[case_index] end
local function target(name) callback_targets[#callback_targets+1]=name;status("milestone:"..name..":"..current_case().id) end
local function boolean(value) if value then return "true" end;return "false" end
local function entity_address(case) return config.ram.entityDataAddress+case.entityIndexByteSeed*config.constants.entityRecordByteCount end
local function write_width(address,width,value)
  if width==1 then memory.write_u8(address,value,"M68K BUS")
  elseif width==2 then memory.write_u16_be(address,value,"M68K BUS")
  elseif width==4 then memory.write_u32_be(address,value,"M68K BUS")
  else error("entity lifecycle unsupported transfer width: "..width) end
end
local function read_width(address,width)
  if width==1 then return memory.read_u8(address,"M68K BUS")
  elseif width==2 then return memory.read_u16_be(address,"M68K BUS")
  elseif width==4 then return memory.read_u32_be(address,"M68K BUS")
  else error("entity lifecycle unsupported transfer width: "..width) end
end
local function write_field(case,name,value)
  local field=config.constants[name]
  write_width(entity_address(case)+field.byteOffset,field.transferByteCount,value)
end
local function read_field(case,name)
  local field=config.constants[name]
  return read_width(entity_address(case)+field.byteOffset,field.transferByteCount)
end
local function json_string(value) return string.format("%q",value) end
local function is_array(value)
  local count=0
  for key,_ in pairs(value) do if type(key)~="number" then return false end;count=count+1 end
  for index=1,count do if value[index]==nil then return false end end
  return true
end
local function json(value)
  local kind=type(value)
  if value==nil then return "null" end
  if kind=="boolean" then return boolean(value) end
  if kind=="number" then return tostring(value) end
  if kind=="string" then return json_string(value) end
  if kind~="table" then error("entity lifecycle JSON value type drift: "..kind) end
  local parts={}
  if is_array(value) then
    for index,item in ipairs(value) do parts[#parts+1]=json(item) end
    return "["..table.concat(parts,",").."]"
  end
  for key,item in pairs(value) do parts[#parts+1]=json_string(key)..":"..json(item) end
  return "{"..table.concat(parts,",").."}"
end
local function copy(value)
  if type(value)~="table" then return value end
  local result={};for key,item in pairs(value) do result[key]=copy(item) end;return result
end

local function setup_case(case,derived)
  local input=config.instrumentation.ramInputAddress
  local script=input+config.instrumentation.scriptInputRamOffset
  memory.write_u8(config.ram.entityIndexListAddress+case.scriptWords[1],case.entityIndexByteSeed,"M68K BUS")
  local hp=config.ram.combatantDataAddress+(case.scriptWords[1]&config.constants.combatantMaskAll)*config.constants.combatantEntryByteCount+config.constants.currentHpByteOffset
  write_width(hp,config.constants.currentHpStorageTransferByteCount,case.currentHpWordSeed)
  write_field(case,"animCounter",case.entityStateSeed.animCounterByte)
  write_field(case,"actscriptPointer",case.entityStateSeed.actscriptPointerLong)
  write_field(case,"mapsprite",case.entityStateSeed.mapspriteByte)
  write_field(case,"flagsB",case.entityStateSeed.flagsBByte)
  if case.priorityByteSeed~=nil then memory.write_u8(config.ram.priorityByteBaseAddress+case.entityIndexByteSeed,case.priorityByteSeed,"M68K BUS") end
  if case.spriteSizeWordSeed~=nil then memory.write_u16_be(config.ram.spriteSizeWordAddress,case.spriteSizeWordSeed,"M68K BUS") end
  memory.write_u32_be(input,derived.handlerAddress,"M68K BUS")
  for index,value in ipairs(case.scriptWords) do memory.write_u16_be(script+(index-1)*2,value,"M68K BUS") end
end
local function begin_case()
  if active then return end
  local case=current_case();if case==nil then error("entity lifecycle unexpected trampoline entry") end
  setup_case(case,current_derived());active=true;handler_entered=false;callback_targets={};get_entity_seen=false;hide_seen=false;adjust_seen=false;cursor_adjust_seen=false;anim_counter_write_seen=false;wait_compare_count=0;wait_back_edge_count=0;wait_for_vint_seen=false;ally_callback_seen=false;update_seen=false;priority_nonzero_seen=false;priority_zero_seen=false;load_seen=false;helper_seen=false;dma_seen=false;size_bit_seen=false;temporary_size_word=nil
  status("milestone:case:"..case.id)
end
local function handler(macro)
  if not active then return end
  if current_case().macro~=macro or emu.getregister("M68K PC")~=current_derived().handlerAddress then error("entity lifecycle handler identity drift") end
  handler_entered=true;status("milestone:handler-entry:"..current_case().id)
end
local function observe_get()
  if not active or get_entity_seen then error("entity lifecycle get-entity callback drift") end
  get_entity_seen=true;target("GetEntityAddressFromCharacter")
end
local function observe_adjust()
  if not active or adjust_seen then error("entity lifecycle adjust callback drift") end
  adjust_seen=true;target("AdjustScriptPointerByCharacterAliveStatus")
end
local function observe_wait_compare()
  if not active or current_case().macro~="waitIdle" then return end
  wait_compare_count=wait_compare_count+1
  local injection=current_derived().waitLoopExitInjection
  if wait_compare_count==injection.atCompareEntryCount then
    write_field(current_case(),injection.field,injection.value)
  elseif wait_compare_count>injection.atCompareEntryCount then error("entity lifecycle idle wait exceeded controlled compare boundary") end
end
local function observe_wait_back_edge()
  if active and current_case().macro=="waitIdle" then wait_back_edge_count=wait_back_edge_count+1 end
end
local function observe_vint()
  if not active then return end
  wait_for_vint_seen=true;target("WaitForVInt")
end
local function observe_update()
  if not active then return end
  update_seen=true;target("UpdateEntitySprite_0")
  if current_case().macro=="setSize" then
    temporary_size_word=memory.read_u16_be(config.ram.spriteSizeWordAddress,"M68K BUS")
    size_bit_seen=(read_field(current_case(),"flagsB")&config.constants.sizeBitMutation.immediateValue)~=0
  end
end
local function append_record()
  local case,derived=current_case(),current_derived()
  if not handler_entered then error("entity lifecycle handler did not execute") end
  local record=copy(derived)
  record.handlerReturned=true;record.callbackTargetOrderObserved=callback_targets
  if #callback_targets~=#derived.effectiveCallbackPlan then error("entity lifecycle effective callback count drift") end
  for index,target_record in ipairs(derived.effectiveCallbackPlan) do if callback_targets[index]~=target_record.instructionTarget then error("entity lifecycle effective callback order drift") end end
  if case.macro=="hide" then
    if not hide_seen then error("entity lifecycle hide callback missing") end;record.hideCallbackObserved=true
  elseif case.macro=="startEntity" or case.macro=="stopEntity" then
    if not adjust_seen then error("entity lifecycle adjust callback missing") end
    if case.currentHpWordSeed~=0 and not get_entity_seen then error("entity lifecycle live get callback missing") end
    record.aliveStatusCursorAdjustmentObserved=cursor_adjust_seen
    if read_field(case,"animCounter")~=derived.animCounterByteAfter then error("entity lifecycle animation counter state drift") end
    record.animCounterWriteObserved=anim_counter_write_seen
  elseif case.macro=="waitIdle" then
    if wait_compare_count~=2 or wait_back_edge_count~=2 then error("entity lifecycle controlled idle loop drift") end
    record.waitCompareEntryCountObserved=wait_compare_count;record.waitBackEdgeInstructionEntryCountObserved=wait_back_edge_count;record.harnessForcedWaitExitObserved=true;record.actscriptPointerLongAfter=read_field(case,"actscriptPointer")
  elseif case.macro=="setSprite" then
    record.allyCallbackObserved=ally_callback_seen;record.waitForVIntCallObserved=wait_for_vint_seen;record.updateEntitySpriteCallObserved=update_seen;record.mapspriteByteAfter=read_field(case,"mapsprite")
  elseif case.macro=="setPriority" then
    record.priorityNonzeroBranchObserved=priority_nonzero_seen;record.priorityByteWriteObserved=(priority_nonzero_seen or priority_zero_seen)
    if memory.read_u8(config.ram.priorityByteBaseAddress+case.entityIndexByteSeed,"M68K BUS")~=derived.priorityByteAfter then error("entity lifecycle priority byte result drift") end
  elseif case.macro=="removeShadow" then
    record.loadMapspriteCallObserved=load_seen;record.helperCallObserved=helper_seen;record.dmaMapspriteCallObserved=dma_seen;record.waitForVIntCallObserved=wait_for_vint_seen
  elseif case.macro=="setSize" then
    if temporary_size_word~=derived.spriteSizeWordInput or memory.read_u16_be(config.ram.spriteSizeWordAddress,"M68K BUS")~=derived.spriteSizeWordAfter then error("entity lifecycle temporary sprite-size restoration drift") end
    record.temporarySpriteSizeWordObserved=temporary_size_word;record.updateEntitySpriteCallObserved=update_seen;record.waitForVIntCallObserved=wait_for_vint_seen;record.sizeBitMutationObserved=size_bit_seen
  end
  local offset=emu.getregister("M68K A6")-config.instrumentation.ramInputAddress
  if offset~=derived.scriptCursorRamOffsetAfter then error("entity lifecycle script cursor drift: "..offset) end
  records[#records+1]=record
end
local function finish(code)
  if replay_state then memorysavestate.removestate(replay_state) end
  if code~=0 then client.exitCode(code);return end
  local result={system=emu.getsystemid(),core="Genesis Plus GX",id=config.fixtureId,mapTest=config.mapTestIndex,recordOrder={},records=records}
  for _,case in ipairs(config.cases) do result.recordOrder[#result.recordOrder+1]=case.id end
  local file=assert(io.open(config.outputPath,"w"));file:write(json(result).."\n");file:close();client.exitCode(0)
end

event.on_bus_exec(function() prompt_count=prompt_count+1;status("milestone:number-prompt-entry:"..prompt_count);if prompt_count==1 then stage="map";pending_save=true;pulse("C") end end,config.harness["function"].numberPromptAddress,"entity-life-number","M68K BUS")
event.on_bus_exec(function() status("milestone:flag-prompt-entry");pulse("B") end,config.harness["function"].flagPromptAddress,"entity-life-flag","M68K BUS")
event.on_bus_exec(begin_case,config["function"].runMapSetupInitFunctionAddress,"entity-life-entry","M68K BUS")
event.on_bus_exec(function() handler("hide") end,config["function"].csc2E_hideEntityAddress,"entity-life-hide","M68K BUS")
event.on_bus_exec(function() handler("startEntity") end,config["function"].csc1B_startEntityAnimAddress,"entity-life-start","M68K BUS")
event.on_bus_exec(function() handler("stopEntity") end,config["function"].csc1C_stopEntityAnimAddress,"entity-life-stop","M68K BUS")
event.on_bus_exec(function() handler("waitIdle") end,config["function"].csc16_waitUntilEntityIdleAddress,"entity-life-wait","M68K BUS")
event.on_bus_exec(function() handler("setSprite") end,config["function"].csc1A_setEntitySpriteAddress,"entity-life-sprite","M68K BUS")
event.on_bus_exec(function() handler("setPriority") end,config["function"].csc53_setPriorityAddress,"entity-life-priority","M68K BUS")
event.on_bus_exec(function() handler("removeShadow") end,config["function"].csc30_removeEntityShadowAddress,"entity-life-shadow","M68K BUS")
event.on_bus_exec(function() handler("setSize") end,config["function"].csc50_setEntitySizeAddress,"entity-life-size","M68K BUS")
event.on_bus_exec(observe_get,config["function"].hideGetEntityCallSiteAddress,"entity-life-hide-get","M68K BUS")
event.on_bus_exec(observe_get,config["function"].startGetEntityCallSiteAddress,"entity-life-start-get","M68K BUS")
event.on_bus_exec(observe_get,config["function"].stopGetEntityCallSiteAddress,"entity-life-stop-get","M68K BUS")
event.on_bus_exec(observe_get,config["function"].waitIdleGetEntityCallSiteAddress,"entity-life-wait-get","M68K BUS")
event.on_bus_exec(observe_get,config["function"].setSpriteGetEntityCallSiteAddress,"entity-life-sprite-get","M68K BUS")
event.on_bus_exec(observe_get,config["function"].setPriorityGetEntityCallSiteAddress,"entity-life-priority-get","M68K BUS")
event.on_bus_exec(observe_get,config["function"].removeShadowGetEntityCallSiteAddress,"entity-life-shadow-get","M68K BUS")
event.on_bus_exec(observe_get,config["function"].setSizeGetEntityCallSiteAddress,"entity-life-size-get","M68K BUS")
event.on_bus_exec(observe_adjust,config["function"].startAdjustCallSiteAddress,"entity-life-start-adjust","M68K BUS")
event.on_bus_exec(observe_adjust,config["function"].stopAdjustCallSiteAddress,"entity-life-stop-adjust","M68K BUS")
event.on_bus_exec(function() if active then cursor_adjust_seen=true end end,config["function"].aliveStatusCursorAdjustmentAddress,"entity-life-cursor-adjust","M68K BUS")
event.on_bus_exec(function() if active then anim_counter_write_seen=true end end,config["function"].startAnimCounterWriteAddress,"entity-life-start-anim","M68K BUS")
event.on_bus_exec(function() if active then anim_counter_write_seen=true end end,config["function"].stopAnimCounterWriteAddress,"entity-life-stop-anim","M68K BUS")
event.on_bus_exec(observe_wait_compare,config["function"].waitIdleCompareAddress,"entity-life-wait-compare","M68K BUS")
event.on_bus_exec(observe_wait_back_edge,config["function"].waitIdleBackEdgeAddress,"entity-life-wait-back","M68K BUS")
event.on_bus_exec(function() if active then ally_callback_seen=true;target("GetAllyMapsprite") end end,config["function"].setSpriteAllyCallbackCallSiteAddress,"entity-life-sprite-ally","M68K BUS")
event.on_bus_exec(observe_vint,config["function"].setSpriteWaitForVIntCallSiteAddress,"entity-life-sprite-vint","M68K BUS")
event.on_bus_exec(observe_update,config["function"].setSpriteUpdateCallSiteAddress,"entity-life-sprite-update","M68K BUS")
event.on_bus_exec(function() if active then priority_nonzero_seen=true end end,config["function"].setPriorityNonzeroWriteAddress,"entity-life-priority-nonzero","M68K BUS")
event.on_bus_exec(function() if active then priority_zero_seen=true end end,config["function"].setPriorityZeroClearAddress,"entity-life-priority-zero","M68K BUS")
event.on_bus_exec(function() if active then target("HideEntity");hide_seen=true end end,config["function"].hideCallbackCallSiteAddress,"entity-life-hide-call","M68K BUS")
event.on_bus_exec(function() if active then target("LoadMapsprite");load_seen=true end end,config["function"].removeShadowLoadMapspriteCallSiteAddress,"entity-life-shadow-load","M68K BUS")
event.on_bus_exec(function() if active then target("sub_45A8C");helper_seen=true end end,config["function"].removeShadowHelperCallSiteAddress,"entity-life-shadow-helper","M68K BUS")
event.on_bus_exec(function() if active then target("DmaMapsprite");dma_seen=true end end,config["function"].removeShadowDmaCallSiteAddress,"entity-life-shadow-dma","M68K BUS")
event.on_bus_exec(observe_vint,config["function"].removeShadowWaitForVIntCallSiteAddress,"entity-life-shadow-vint","M68K BUS")
event.on_bus_exec(observe_update,config["function"].setSizeUpdateCallSiteAddress,"entity-life-size-update","M68K BUS")
event.on_bus_exec(observe_vint,config["function"].setSizeWaitForVIntCallSiteAddress,"entity-life-size-vint","M68K BUS")
event.on_bus_exec(function() if not active then return end;append_record();active=false;case_index=case_index+1;if case_index>#config.cases then pending_finish=true else pending_replay=true end end,config.instrumentation.postHandlerAddress,"entity-life-return","M68K BUS")

local frames=0
while true do
  frames=frames+1
  if pending_finish then finish(0) elseif pending_save then pending_save=false;replay_state=memorysavestate.savecorestate();status("milestone:saved-map-prompt") elseif pending_replay then pending_replay=false;memorysavestate.loadcorestate(replay_state);queue={};pulse("C");status("milestone:replay-map-prompt") end
  if frames>=config.maxFrames then status("timeout:frame-budget-exhausted:case="..case_index);finish(1) end
  local button=nil
  if stage=="cheat" then local pointer=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS");if pointer>=0x28FF0 and pointer<0x29000 then button=names[cheat[pointer-0x28FF0+1]] elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Down" end elseif #queue>0 then button=table.remove(queue,1) end
  set_button(button);joypad.set({},2);emu.frameadvance()
end
