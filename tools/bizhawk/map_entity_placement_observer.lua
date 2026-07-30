local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local stage, prompt_count, case_index = "cheat", 0, 1
local queue, records = {}, {}
local replay_state, pending_save, pending_replay, pending_finish = nil, false, false, false
local active, handler_entered = false, false
local callback_order, cursor_adjusted, wait_seen = {}, false, false
local wait_vint_count, sleep_count, shared_tail_seen = 0, 0, false
local negative_x_seen, negative_y_seen, get_entity_seen, update_seen = false, false, false, false
local flash_initial_get_seen, shared_tail_get_seen, shared_tail_update_seen = false, false, false
local state_at_wait = nil
local names = { [1]="Up", [2]="Down", [4]="Left", [8]="Right", [16]="B", [32]="C" }
local cheat = { 1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4 }

local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local b={};if name and name~="" then b[name]=true end;joypad.set(b,1) end
local function boolean(value) if value then return "true" end;return "false" end
local function nullable(value) if value==nil then return "null" end;return tostring(value) end
local function strings(f, values) f:write("[");for i,value in ipairs(values) do if i>1 then f:write(",") end;f:write(string.format('"%s"',value)) end;f:write("]") end
local function current_case() return config.cases[case_index] end
local function current_derived() return config.derived[case_index] end
local function callback(name) callback_order[#callback_order+1]=name;status("milestone:"..name..":"..current_case().id) end

local function entity_address(case)
    return config.ram.entityDataAddress + case.entityIndexByteSeed * config.constants.entityRecordByteCount
end
local function write_bus_value(address,width,value)
    if width==1 then memory.write_u8(address,value,"M68K BUS")
    elseif width==2 then memory.write_u16_be(address,value,"M68K BUS")
    else error("map entity placement unsupported stored width: "..width) end
end
local function read_bus_value(address,width)
    if width==1 then return memory.read_u8(address,"M68K BUS")
    elseif width==2 then return memory.read_u16_be(address,"M68K BUS")
    else error("map entity placement unsupported stored width: "..width) end
end
local function write_entity_field(address,field,value)
    write_bus_value(address+field.byteOffset,field.transferByteCount,value)
end
local function read_entity_field(address,field)
    return read_bus_value(address+field.byteOffset,field.transferByteCount)
end
local function write_state(case)
    local a,s=entity_address(case),case.entityStateSeed
    local o=config.constants.entityFieldLayouts
    write_entity_field(a,o.xWord,s.xWord);write_entity_field(a,o.yWord,s.yWord)
    write_entity_field(a,o.xVelocityWord,s.xVelocityWord);write_entity_field(a,o.yVelocityWord,s.yVelocityWord)
    write_entity_field(a,o.xTravelWord,s.xTravelWord);write_entity_field(a,o.yTravelWord,s.yTravelWord)
    write_entity_field(a,o.xDestWord,s.xDestWord);write_entity_field(a,o.yDestWord,s.yDestWord)
    write_entity_field(a,o.facingByte,s.facingByte)
end
local function state(case)
    local a,o=entity_address(case),config.constants.entityFieldLayouts
    return {xWord=read_entity_field(a,o.xWord),yWord=read_entity_field(a,o.yWord),xVelocityWord=read_entity_field(a,o.xVelocityWord),yVelocityWord=read_entity_field(a,o.yVelocityWord),xTravelWord=read_entity_field(a,o.xTravelWord),yTravelWord=read_entity_field(a,o.yTravelWord),xDestWord=read_entity_field(a,o.xDestWord),yDestWord=read_entity_field(a,o.yDestWord),facingByte=read_entity_field(a,o.facingByte)}
end
local function state_equal(a,b)
    for _,key in ipairs({"xWord","yWord","xVelocityWord","yVelocityWord","xTravelWord","yTravelWord","xDestWord","yDestWord","facingByte"}) do if a[key]~=b[key] then error("map entity placement state drift: "..key) end end
end
local function write_state_json(f,s)
    f:write(string.format('{"xWord":%d,"yWord":%d,"xDestWord":%d,"yDestWord":%d,"xTravelWord":%d,"yTravelWord":%d,"xVelocityWord":%d,"yVelocityWord":%d,"facingByte":%d}',s.xWord,s.yWord,s.xDestWord,s.yDestWord,s.xTravelWord,s.yTravelWord,s.xVelocityWord,s.yVelocityWord,s.facingByte))
end

local function setup_case(case,derived)
    local input=config.instrumentation.ramInputAddress
    local script_input=input+config.instrumentation.scriptInputRamOffset
    write_state(case)
    if case.kind=="destination" then
        local combatant=case.selectorWord & config.constants.combatantMaskAll
        local cursor=config.constants.destinationInputCursorUseSites
        if #cursor~=3 or cursor[1].destinationOperand~="d0" or cursor[2].destinationOperand~="d1" or cursor[3].destinationOperand~="d2" then error("map entity placement destination input cursor config drift") end
        memory.write_u8(config.ram.entityIndexListAddress+combatant,case.entityIndexByteSeed,"M68K BUS")
        memory.write_u32_be(input,derived.handlerAddress,"M68K BUS")
        write_bus_value(script_input+cursor[1].scriptInputByteOffset,cursor[1].transferredByteCount,case.selectorWord)
        write_bus_value(script_input+cursor[2].scriptInputByteOffset,cursor[2].transferredByteCount,case.xInputWord)
        write_bus_value(script_input+cursor[3].scriptInputByteOffset,cursor[3].transferredByteCount,case.yInputWord)
    else
        memory.write_u8(config.ram.entityIndexListAddress+case.selectorByte,case.entityIndexByteSeed,"M68K BUS")
        local hp=config.ram.combatantDataAddress+case.selectorByte*config.constants.combatantEntryByteCount+config.constants.currentHpByteOffset
        write_bus_value(hp,config.constants.currentHpSeedTransferByteCount,case.currentHpWordSeed)
        memory.write_u32_be(input,derived.handlerAddress,"M68K BUS")
        for i,value in ipairs(case.scriptBytes) do memory.write_u8(script_input+i-1,value,"M68K BUS") end
    end
end

local function begin_case()
    if active then return end
    local case=current_case();if case==nil then error("map entity placement unexpected wrapper entry") end
    setup_case(case,current_derived());active=true;handler_entered=false;callback_order={};cursor_adjusted=false;wait_seen=false
    wait_vint_count=0;sleep_count=0;shared_tail_seen=false;negative_x_seen=false;negative_y_seen=false;get_entity_seen=false;update_seen=false;flash_initial_get_seen=false;shared_tail_get_seen=false;shared_tail_update_seen=false;state_at_wait=nil
    status("milestone:case:"..case.id)
end
local function handler(kind)
    if not active then return end
    if current_case().kind=="flash" and kind=="position" then return end
    if current_case().kind~=kind then error("map entity placement handler identity drift") end
    if emu.getregister("M68K PC")~=current_derived().handlerAddress then error("map entity placement handler address drift") end
    handler_entered=true;status("milestone:handler-entry:"..current_case().id)
end
local function observe_adjust(kind)
    if not active then return end
    if current_case().kind~=kind and not (current_case().kind=="flash" and kind=="position") then error("map entity placement adjustment drift") end
    callback("adjustScriptPointer")
end
local function observe_cursor_adjustment()
    if not active then return end
    if current_case().lifeState~="dead" or cursor_adjusted then error("map entity placement dead cursor adjustment drift") end
    cursor_adjusted=true;callback("aliveStatusCursorAdjustment")
end
local function observe_get(kind)
    if not active then return end
    if current_case().kind=="flash" then
        if kind=="flash" then
            if flash_initial_get_seen then error("map entity placement duplicate flash get-entity") end
            flash_initial_get_seen=true;callback("getEntityAddressForFlash");return
        end
        if kind=="position" then
            if shared_tail_get_seen then error("map entity placement duplicate shared-tail get-entity") end
            shared_tail_get_seen=true;callback("getEntityAddressFromSharedTail");return
        end
    end
    if current_case().kind~=kind or get_entity_seen then error("map entity placement get-entity callback drift") end
    get_entity_seen=true;callback("getEntityAddress")
end
local function observe_update(kind)
    if not active then return end
    if current_case().kind=="flash" and kind=="position" then
        if shared_tail_update_seen then error("map entity placement duplicate shared-tail sprite update") end
        shared_tail_update_seen=true;callback("updateEntitySpriteFromSharedTail");return
    end
    if current_case().kind~=kind or update_seen then error("map entity placement sprite callback drift") end
    update_seen=true;callback("updateEntitySprite")
end
local function observe_vint()
    if not active or current_case().kind~="flash" then return end
    wait_vint_count=wait_vint_count+1;callback("waitForVInt")
end
local function observe_sleep()
    if not active or current_case().kind~="flash" then return end
    sleep_count=sleep_count+1;callback("sleep")
end
local function observe_tail()
    if not active or current_case().kind~="flash" or shared_tail_seen then error("map entity placement shared-tail drift") end
    shared_tail_seen=true;callback("sharedTail")
end
local function observe_negative(axis)
    if not active or current_case().kind~="destination" then return end
    if axis=="x" then if negative_x_seen then error("map entity placement duplicate negative x") end;negative_x_seen=true else if negative_y_seen then error("map entity placement duplicate negative y") end;negative_y_seen=true end
    callback("negative"..string.upper(axis).."Delta")
end
local function observe_wait()
    if not active or current_case().kind~="destination" or wait_seen then error("map entity placement wait callback drift") end
    wait_seen=true;state_at_wait=state(current_case());callback("waitForEntityToStopMoving")
    local d=current_derived();state_equal(state_at_wait,{xWord=current_case().entityStateSeed.xWord,yWord=current_case().entityStateSeed.yWord,xVelocityWord=d.xVelocityWordAfter,yVelocityWord=d.yVelocityWordAfter,xTravelWord=d.xTravelWordAfter,yTravelWord=d.yTravelWordAfter,xDestWord=d.xDestWordAfter,yDestWord=d.yDestWordAfter,facingByte=current_case().entityStateSeed.facingByte})
end

local function append_record()
    local case,derived=current_case(),current_derived()
    if not handler_entered then error("map entity placement handler did not execute") end
    local final=state(case)
    if case.kind=="destination" then
        if derived.waitBypassed then if wait_seen then error("map entity placement wait bypass failed") end;state_equal(final,{xWord=case.entityStateSeed.xWord,yWord=case.entityStateSeed.yWord,xVelocityWord=derived.xVelocityWordAfter,yVelocityWord=derived.yVelocityWordAfter,xTravelWord=derived.xTravelWordAfter,yTravelWord=derived.yTravelWordAfter,xDestWord=derived.xDestWordAfter,yDestWord=derived.yDestWordAfter,facingByte=case.entityStateSeed.facingByte}) elseif not wait_seen then error("map entity placement required wait was skipped") end
    else
        state_equal(final,{xWord=derived.xWordAfter or case.entityStateSeed.xWord,yWord=derived.yWordAfter or case.entityStateSeed.yWord,xVelocityWord=case.entityStateSeed.xVelocityWord,yVelocityWord=case.entityStateSeed.yVelocityWord,xTravelWord=case.entityStateSeed.xTravelWord,yTravelWord=case.entityStateSeed.yTravelWord,xDestWord=derived.xDestWordAfter or case.entityStateSeed.xDestWord,yDestWord=derived.yDestWordAfter or case.entityStateSeed.yDestWord,facingByte=derived.facingByteAfter})
    end
    local offset=emu.getregister("M68K A6")-config.instrumentation.ramInputAddress
    if offset~=derived.scriptCursorRamOffsetAfter then error("map entity placement script cursor result drift: "..offset) end
    records[#records+1]={case=case,derived=derived,handlerReturned=true,cursorAdjusted=cursor_adjusted,getEntityObserved=get_entity_seen,updateObserved=update_seen,flashInitialGetObserved=flash_initial_get_seen,sharedTailGetObserved=shared_tail_get_seen,sharedTailUpdateObserved=shared_tail_update_seen,waitVintCount=wait_vint_count,sleepCount=sleep_count,sharedTailObserved=shared_tail_seen,negativeXObserved=negative_x_seen,negativeYObserved=negative_y_seen,waitObserved=wait_seen,callbackOrder=callback_order}
end

local function write_record(f,r)
    local d=r.derived
    if d.kind=="destination" then
        f:write(string.format('{"id":"%s","kind":"destination","handlerAddress":%d,"selectorWord":%d,"xInputWord":%d,"yInputWord":%d,"scriptCursorRamOffsetAfter":%d,"xDestWordAfter":%d,"yDestWordAfter":%d,"xTravelWordAfter":%d,"yTravelWordAfter":%d,"xVelocityWordAfter":%d,"yVelocityWordAfter":%d,"negativeXInstructionAddress":%s,"negativeYInstructionAddress":%s,"waitBypassed":%s,"waitCallSiteAddress":%s,"entityStateSeed":',d.id,d.handlerAddress,d.selectorWord,d.xInputWord,d.yInputWord,d.scriptCursorRamOffsetAfter,d.xDestWordAfter,d.yDestWordAfter,d.xTravelWordAfter,d.yTravelWordAfter,d.xVelocityWordAfter,d.yVelocityWordAfter,nullable(d.negativeXInstructionAddress),nullable(d.negativeYInstructionAddress),boolean(d.waitBypassed),nullable(d.waitCallSiteAddress)));write_state_json(f,d.entityStateSeed);f:write(string.format(',"handlerReturned":true,"getEntityCallObserved":%s,"negativeXObserved":%s,"negativeYObserved":%s,"waitForEntityToStopMovingObserved":%s,"callbackOrder":',boolean(r.getEntityObserved),boolean(r.negativeXObserved),boolean(r.negativeYObserved),boolean(r.waitObserved)))
    elseif d.kind=="facing" then
        f:write(string.format('{"id":"%s","kind":"facing","lifeState":"%s","handlerAddress":%d,"adjustCallSiteAddress":%d,"cursorAdjustmentByteCount":%d,"scriptCursorRamOffsetAfter":%d,"selectorByte":%d,"facingByteAfter":%d,"updateSpriteCallSiteAddress":%s,"entityStateSeed":',d.id,d.lifeState,d.handlerAddress,d.adjustCallSiteAddress,d.cursorAdjustmentByteCount,d.scriptCursorRamOffsetAfter,d.selectorByte,d.facingByteAfter,nullable(d.updateSpriteCallSiteAddress)));write_state_json(f,d.entityStateSeed);f:write(string.format(',"handlerReturned":true,"aliveStatusCursorAdjustmentExecuted":%s,"getEntityCallObserved":%s,"updateSpriteCallObserved":%s,"callbackOrder":',boolean(r.cursorAdjusted),boolean(r.getEntityObserved),boolean(r.updateObserved)))
    else
        f:write(string.format('{"id":"%s","kind":"%s","lifeState":"%s","handlerAddress":%d,"adjustCallSiteAddress":%d,"cursorAdjustmentByteCount":%d,"scriptCursorRamOffsetAfter":%d,"selectorByte":%d,"xWordAfter":%d,"xDestWordAfter":%d,"yWordAfter":%d,"yDestWordAfter":%d,"facingByteAfter":%d,"updateSpriteCallSiteAddress":%s,"entityStateSeed":',d.id,d.kind,d.lifeState,d.handlerAddress,d.adjustCallSiteAddress,d.cursorAdjustmentByteCount,d.scriptCursorRamOffsetAfter,d.selectorByte,d.xWordAfter,d.xDestWordAfter,d.yWordAfter,d.yDestWordAfter,d.facingByteAfter,nullable(d.updateSpriteCallSiteAddress)));write_state_json(f,d.entityStateSeed);f:write(string.format(',"flashLoopIterationCount":%s,"handlerReturned":true,"aliveStatusCursorAdjustmentExecuted":%s,"getEntityCallObserved":%s,"updateSpriteCallObserved":%s,"flashInitialGetEntityCallObserved":%s,"sharedTailGetEntityCallObserved":%s,"sharedTailUpdateSpriteCallObserved":%s,"flashWaitForVIntCallCount":%d,"flashSleepCallCount":%d,"sharedTailObserved":%s,"callbackOrder":',nullable(d.flashLoopIterationCount),boolean(r.cursorAdjusted),boolean(r.getEntityObserved),boolean(r.updateObserved),boolean(r.flashInitialGetObserved),boolean(r.sharedTailGetObserved),boolean(r.sharedTailUpdateObserved),r.waitVintCount,r.sleepCount,boolean(r.sharedTailObserved)))
    end
    strings(f,r.callbackOrder);f:write("}")
end
local function finish(exit_code)
    if replay_state then memorysavestate.removestate(replay_state) end
    if exit_code~=0 then client.exitCode(exit_code);return end
    local f=assert(io.open(config.outputPath,"w"));f:write(string.format('{"system":"%s","core":"Genesis Plus GX","id":"%s","mapTest":%d,"recordOrder":',emu.getsystemid(),config.fixtureId,config.mapTestIndex));strings(f,(function() local v={};for _,c in ipairs(config.cases) do v[#v+1]=c.id end;return v end)());f:write(',"records":[');for i,r in ipairs(records) do if i>1 then f:write(",") end;write_record(f,r) end;f:write("]}\n");f:close();client.exitCode(0)
end

event.on_bus_exec(function() prompt_count=prompt_count+1;status("milestone:number-prompt-entry:"..prompt_count);if prompt_count==1 then stage="map";pending_save=true;pulse("C") end end,config.harness["function"].numberPromptAddress,"sf2-map-entity-placement-number","M68K BUS")
event.on_bus_exec(function() status("milestone:flag-prompt-entry");pulse("B") end,config.harness["function"].flagPromptAddress,"sf2-map-entity-placement-flag","M68K BUS")
event.on_bus_exec(begin_case,config["function"].entryAddress,"sf2-map-entity-placement-entry","M68K BUS")
event.on_bus_exec(function() handler("position") end,config["function"].setPositionHandlerAddress,"sf2-map-entity-placement-position","M68K BUS")
event.on_bus_exec(function() handler("flash") end,config["function"].setPositionFlashHandlerAddress,"sf2-map-entity-placement-flash","M68K BUS")
event.on_bus_exec(function() handler("facing") end,config["function"].setFacingHandlerAddress,"sf2-map-entity-placement-facing","M68K BUS")
event.on_bus_exec(function() handler("destination") end,config["function"].setDestinationHandlerAddress,"sf2-map-entity-placement-destination","M68K BUS")
event.on_bus_exec(function() observe_adjust("position") end,config["function"].setPositionAdjustCallSiteAddress,"sf2-map-entity-placement-position-adjust","M68K BUS")
event.on_bus_exec(function() observe_adjust("facing") end,config["function"].setFacingAdjustCallSiteAddress,"sf2-map-entity-placement-facing-adjust","M68K BUS")
event.on_bus_exec(observe_cursor_adjustment,config["function"].aliveStatusCursorAdjustmentAddress,"sf2-map-entity-placement-cursor-adjust","M68K BUS")
event.on_bus_exec(function() observe_get("position") end,config["function"].setPositionGetEntityCallSiteAddress,"sf2-map-entity-placement-position-get","M68K BUS")
event.on_bus_exec(function() observe_update("position") end,config["function"].setPositionUpdateSpriteCallSiteAddress,"sf2-map-entity-placement-position-update","M68K BUS")
event.on_bus_exec(function() observe_get("facing") end,config["function"].setFacingGetEntityCallSiteAddress,"sf2-map-entity-placement-facing-get","M68K BUS")
event.on_bus_exec(function() observe_update("facing") end,config["function"].setFacingUpdateSpriteCallSiteAddress,"sf2-map-entity-placement-facing-update","M68K BUS")
event.on_bus_exec(function() observe_get("flash") end,config["function"].setPositionFlashGetEntityCallSiteAddress,"sf2-map-entity-placement-flash-get","M68K BUS")
event.on_bus_exec(observe_vint,config["function"].setPositionFlashWaitForVIntCallOneAddress,"sf2-map-entity-placement-flash-vint-one","M68K BUS")
event.on_bus_exec(observe_vint,config["function"].setPositionFlashWaitForVIntCallTwoAddress,"sf2-map-entity-placement-flash-vint-two","M68K BUS")
event.on_bus_exec(observe_sleep,config["function"].setPositionFlashSleepCallSiteAddress,"sf2-map-entity-placement-flash-sleep","M68K BUS")
event.on_bus_exec(observe_tail,config["function"].setPositionFlashSharedTailBranchAddress,"sf2-map-entity-placement-flash-tail","M68K BUS")
event.on_bus_exec(function() observe_get("destination") end,config["function"].setDestinationGetEntityCallSiteAddress,"sf2-map-entity-placement-destination-get","M68K BUS")
event.on_bus_exec(function() observe_negative("x") end,config["function"].setDestinationNegativeXAddress,"sf2-map-entity-placement-negative-x","M68K BUS")
event.on_bus_exec(function() observe_negative("y") end,config["function"].setDestinationNegativeYAddress,"sf2-map-entity-placement-negative-y","M68K BUS")
event.on_bus_exec(observe_wait,config["function"].setDestinationWaitCallSiteAddress,"sf2-map-entity-placement-wait","M68K BUS")
event.on_bus_exec(function() if not active then return end;append_record();active=false;case_index=case_index+1;if case_index>#config.cases then pending_finish=true else pending_replay=true end end,config.instrumentation.postHandlerAddress,"sf2-map-entity-placement-return","M68K BUS")

local frames=0
while true do
    frames=frames+1
    if pending_finish then finish(0) elseif pending_save then pending_save=false;replay_state=memorysavestate.savecorestate();status("milestone:saved-map-prompt") elseif pending_replay then pending_replay=false;memorysavestate.loadcorestate(replay_state);queue={};pulse("C");status("milestone:replay-map-prompt") end
    if frames>=config.maxFrames then status("timeout:frame-budget-exhausted:case="..case_index..":stage="..stage);finish(1) end
    local button=nil
    if stage=="cheat" then local pointer=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS");if pointer>=0x28FF0 and pointer<0x29000 then button=names[cheat[pointer-0x28FF0+1]] elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Down" end elseif #queue>0 then button=table.remove(queue,1) end
    set_button(button);joypad.set({},2);emu.frameadvance()
    if frames%600==0 then status(string.format("frame=%d,stage=%s,case=%d",frames,stage,case_index)) end
end
