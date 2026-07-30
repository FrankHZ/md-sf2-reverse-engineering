local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local stage, prompt_count, case_index = "cheat", 0, 1
local queue, records = {}, {}
local replay_state, pending_save, pending_replay, pending_finish = nil, false, false, false
local active, handler_entered = false, false
local lookup_seen, set_camera_call_seen, service_seen = false, false, false
local set_view_call_seen, set_view_seen, wait_call_seen, wait_seen = false, false, false, false
local callback_order, lookup_index, set_view_d0, set_view_d1 = {}, nil, nil, nil
local names = { [1]="Up", [2]="Down", [4]="Left", [8]="Right", [16]="B", [32]="C" }
local cheat = { 1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4 }

local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local b={};if name and name~="" then b[name]=true end;joypad.set(b,1) end
local function boolean(value) if value then return "true" end;return "false" end
local function nullable_number(value) if value==nil then return "null" end;return tostring(value) end
local function write_strings(f, values)
    f:write("[")
    for i,value in ipairs(values) do if i>1 then f:write(",") end;f:write(string.format('"%s"',value)) end
    f:write("]")
end

local function current_case() return config.cases[case_index] end
local function current_derived() return config.derived[case_index] end
local function callback(name) callback_order[#callback_order+1]=name;status("milestone:"..name..":"..current_case().id) end

local function setup_case(case, derived)
    local input=config.instrumentation.ramInputAddress
    if case.kind=="target" then
        memory.write_u8(config.ram.viewTargetEntityAddress,case.viewTargetEntityByteSeed,"M68K BUS")
        if derived.entityIndexListLookupIndex~=nil then
            memory.write_u8(config.ram.entityIndexListAddress+derived.entityIndexListLookupIndex,case.entityIndexListByteSeed,"M68K BUS")
        end
        memory.write_u32_be(input,derived.handlerAddress,"M68K BUS")
        memory.write_u16_be(input+4,case.operandWord,"M68K BUS")
    elseif case.kind=="destination" then
        memory.write_u8(config.ram.viewTargetEntityAddress,case.viewTargetEntityByteSeed,"M68K BUS")
        memory.write_u32_be(input,derived.handlerAddress,"M68K BUS")
        memory.write_u16_be(input+4,case.inputWords[1],"M68K BUS")
        memory.write_u16_be(input+6,case.inputWords[2],"M68K BUS")
    elseif case.kind=="speed" then
        memory.write_u16_be(config.ram.viewScrollingSpeedAddress,case.viewScrollingSpeedWordSeed,"M68K BUS")
        memory.write_u32_be(input,derived.handlerAddress,"M68K BUS")
        memory.write_u16_be(input+4,case.operandWord,"M68K BUS")
    else error("map camera control unknown case kind") end
end

local function begin_case()
    if active then return end
    local case=current_case()
    if case==nil then error("map camera control unexpected wrapper entry") end
    setup_case(case,current_derived())
    active=true;handler_entered=false;lookup_seen=false;set_camera_call_seen=false;service_seen=false
    set_view_call_seen=false;set_view_seen=false;wait_call_seen=false;wait_seen=false
    callback_order={};lookup_index=nil;set_view_d0=nil;set_view_d1=nil
    status("milestone:case:"..case.id)
end

local function observe_handler()
    if not active then return end
    if emu.getregister("M68K PC")~=current_derived().handlerAddress then error("map camera control handler identity drift") end
    handler_entered=true;status("milestone:handler-entry:"..current_case().id)
end

local function observe_lookup()
    if not active then return end
    local derived=current_derived()
    if current_case().kind~="target" or derived.targetEntityLookupAddress==nil or lookup_seen then error("map camera control target lookup drift") end
    lookup_seen=true;lookup_index=emu.getregister("M68K D0")&0xFF;callback("entityIndexListLookup")
end

local function observe_set_camera_call()
    if not active then return end
    if current_case().kind~="destination" or set_camera_call_seen then error("map camera control destination call drift") end
    set_camera_call_seen=true;callback("setCameraDestinationCall")
end
local function observe_service()
    if not active then return end
    if current_case().kind~="destination" or service_seen then error("map camera control destination service drift") end
    service_seen=true;callback("setCameraDestinationService")
end
local function observe_set_view_call()
    if not active then return end
    if current_case().kind~="destination" or set_view_call_seen then error("map camera control SetViewDestination call drift") end
    set_view_call_seen=true;set_view_d0=emu.getregister("M68K D0")&0xFFFF;set_view_d1=emu.getregister("M68K D1")&0xFFFF;callback("setViewDestinationCall")
end
local function observe_set_view()
    if not active then return end
    if current_case().kind~="destination" or set_view_seen then error("map camera control SetViewDestination entry drift") end
    set_view_seen=true
    if set_view_d0~=(emu.getregister("M68K D0")&0xFFFF) or set_view_d1~=(emu.getregister("M68K D1")&0xFFFF) then error("map camera control SetViewDestination register drift") end
    callback("setViewDestination")
end
local function observe_wait_call()
    if not active then return end
    if current_case().kind~="destination" or wait_call_seen then error("map camera control wait call drift") end
    wait_call_seen=true;callback("waitForViewScrollEndCall")
end
local function observe_wait()
    if not active then return end
    if current_case().kind~="destination" or wait_seen then error("map camera control wait entry drift") end
    wait_seen=true;callback("waitForViewScrollEnd")
end

local function append_record()
    local case,derived=current_case(),current_derived()
    if not handler_entered then error("map camera control handler did not execute") end
    local record={}
    for name,value in pairs(derived) do record[name]=value end
    record.handlerReturned=true;record.callbackOrder=callback_order
    if case.kind=="target" then
        if lookup_seen~=(derived.entityIndexListLookupIndex~=nil) then error("map camera control target branch callback drift") end
        if lookup_seen and lookup_index~=derived.entityIndexListLookupIndex then error("map camera control target lookup index drift") end
        record.entityIndexListLookupExecuted=lookup_seen
        if memory.read_u8(config.ram.viewTargetEntityAddress,"M68K BUS")~=derived.viewTargetEntityByteAfter then error("map camera control target state drift") end
    elseif case.kind=="destination" then
        if not (set_camera_call_seen and service_seen and set_view_call_seen and set_view_seen and wait_call_seen and wait_seen) then error("map camera control destination callback did not execute") end
        if set_view_d0~=derived.setViewDestinationD0Word or set_view_d1~=derived.setViewDestinationD1Word then error("map camera control destination scaling drift") end
        if memory.read_u8(config.ram.viewTargetEntityAddress,"M68K BUS")~=derived.viewTargetEntityByteAfter then error("map camera control destination target reset drift") end
        record.setCameraDestinationCallObserved=set_camera_call_seen;record.setViewDestinationCallObserved=set_view_call_seen
        record.waitForViewScrollEndCallObserved=wait_call_seen;record.waitCompletedBeforeHandlerReturn=wait_seen
    elseif case.kind=="speed" then
        if memory.read_u16_be(config.ram.viewScrollingSpeedAddress,"M68K BUS")~=derived.viewScrollingSpeedWordAfter then error("map camera control speed state drift") end
    end
    records[#records+1]=record
end

local function write_record(f,r)
    if r.kind=="target" then
        f:write(string.format('{"id":"%s","kind":"target","targetMode":"%s","handlerAddress":%d,"operandWord":%d,"targetEntityLookupAddress":%s,"entityIndexListLookupIndex":%s,"viewTargetEntityByteAfter":%d,"handlerReturned":true,"entityIndexListLookupExecuted":%s,"callbackOrder":',r.id,r.targetMode,r.handlerAddress,r.operandWord,nullable_number(r.targetEntityLookupAddress),nullable_number(r.entityIndexListLookupIndex),r.viewTargetEntityByteAfter,boolean(r.entityIndexListLookupExecuted)))
    elseif r.kind=="destination" then
        f:write(string.format('{"id":"%s","kind":"destination","handlerAddress":%d,"setCameraDestinationCallSiteAddress":%d,"waitForViewScrollEndCallSiteAddress":%d,"setCameraDestinationServiceAddress":%d,"setViewDestinationAddress":%d,"setViewDestinationCallSiteAddress":%d,"inputWords":[%d,%d],"setViewDestinationD0Word":%d,"setViewDestinationD1Word":%d,"viewTargetEntityByteAfter":%d,"handlerReturned":true,"setCameraDestinationCallObserved":true,"setViewDestinationCallObserved":true,"waitForViewScrollEndCallObserved":true,"waitCompletedBeforeHandlerReturn":true,"callbackOrder":',r.id,r.handlerAddress,r.setCameraDestinationCallSiteAddress,r.waitForViewScrollEndCallSiteAddress,r.setCameraDestinationServiceAddress,r.setViewDestinationAddress,r.setViewDestinationCallSiteAddress,r.inputWords[1],r.inputWords[2],r.setViewDestinationD0Word,r.setViewDestinationD1Word,r.viewTargetEntityByteAfter))
    else
        f:write(string.format('{"id":"%s","kind":"speed","handlerAddress":%d,"operandWord":%d,"viewScrollingSpeedWordAfter":%d,"handlerReturned":true,"callbackOrder":',r.id,r.handlerAddress,r.operandWord,r.viewScrollingSpeedWordAfter))
    end
    write_strings(f,r.callbackOrder);f:write("}")
end

local function finish(exit_code)
    if replay_state then memorysavestate.removestate(replay_state) end
    if exit_code~=0 then client.exitCode(exit_code);return end
    local f=assert(io.open(config.outputPath,"w"))
    f:write(string.format('{"system":"%s","core":"Genesis Plus GX","id":"%s","mapTest":%d,"recordOrder":',emu.getsystemid(),config.fixtureId,config.mapTestIndex))
    write_strings(f,(function() local values={};for _,case in ipairs(config.cases) do values[#values+1]=case.id end;return values end)())
    f:write(',"records":[');for i,r in ipairs(records) do if i>1 then f:write(",") end;write_record(f,r) end;f:write("]}\n");f:close();client.exitCode(0)
end

event.on_bus_exec(function() prompt_count=prompt_count+1;status("milestone:number-prompt-entry:"..prompt_count);if prompt_count==1 then stage="map";pending_save=true;pulse("C") end end,config.harness["function"].numberPromptAddress,"sf2-map-camera-control-number","M68K BUS")
event.on_bus_exec(function() status("milestone:flag-prompt-entry");pulse("B") end,config.harness["function"].flagPromptAddress,"sf2-map-camera-control-flag","M68K BUS")
event.on_bus_exec(begin_case,config["function"].entryAddress,"sf2-map-camera-control-entry","M68K BUS")
event.on_bus_exec(observe_handler,config["function"].setCameraEntityHandlerAddress,"sf2-map-camera-control-target","M68K BUS")
event.on_bus_exec(observe_handler,config["function"].setCameraDestinationHandlerAddress,"sf2-map-camera-control-destination","M68K BUS")
event.on_bus_exec(observe_handler,config["function"].cameraSpeedHandlerAddress,"sf2-map-camera-control-speed","M68K BUS")
event.on_bus_exec(observe_lookup,config["function"].targetEntityLookupAddress,"sf2-map-camera-control-lookup","M68K BUS")
event.on_bus_exec(observe_set_camera_call,config["function"].setCameraDestinationCallSiteAddress,"sf2-map-camera-control-set-camera-call","M68K BUS")
event.on_bus_exec(observe_service,config["function"].setCameraDestinationServiceAddress,"sf2-map-camera-control-service","M68K BUS")
event.on_bus_exec(observe_set_view_call,config["function"].setViewDestinationCallSiteAddress,"sf2-map-camera-control-set-view-call","M68K BUS")
event.on_bus_exec(observe_set_view,config["function"].setViewDestinationAddress,"sf2-map-camera-control-set-view","M68K BUS")
event.on_bus_exec(observe_wait_call,config["function"].waitForViewScrollEndCallSiteAddress,"sf2-map-camera-control-wait-call","M68K BUS")
event.on_bus_exec(observe_wait,config["function"].waitForViewScrollEndAddress,"sf2-map-camera-control-wait","M68K BUS")
event.on_bus_exec(function() if not active then return end;append_record();active=false;case_index=case_index+1;if case_index>#config.cases then pending_finish=true else pending_replay=true end end,config.instrumentation.postHandlerAddress,"sf2-map-camera-control-return","M68K BUS")

local frames=0
while true do
    frames=frames+1
    if pending_finish then finish(0)
    elseif pending_save then pending_save=false;replay_state=memorysavestate.savecorestate();status("milestone:saved-map-prompt")
    elseif pending_replay then pending_replay=false;memorysavestate.loadcorestate(replay_state);queue={};pulse("C");status("milestone:replay-map-prompt") end
    if frames>=config.maxFrames then status("timeout:frame-budget-exhausted:case="..case_index..":stage="..stage);finish(1) end
    local button=nil
    if stage=="cheat" then
        local pointer=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS")
        if pointer>=0x28FF0 and pointer<0x29000 then button=names[cheat[pointer-0x28FF0+1]] elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Down" end
    elseif #queue>0 then button=table.remove(queue,1) end
    set_button(button);joypad.set({},2);emu.frameadvance()
    if frames%600==0 then status(string.format("frame=%d,stage=%s,case=%d",frames,stage,case_index)) end
end
