local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local stage, prompt_count, case_index = "cheat", 0, 1
local queue, records = {}, {}
local replay_state, pending_save, pending_replay, pending_finish = nil, false, false, false
local active, handler_entered, continuation_entered = false, false, false
local direct_call_order, load_map_d0_word_at_call, load_map_d1_word_at_call, tileset_d1_word_at_call = {}, nil, nil, nil
local reset_tail_load_map_d0_word_at_transfer, reset_tail_load_map_d1_word_at_transfer = nil, nil
local names = { [1]="Up", [2]="Down", [4]="Left", [8]="Right", [16]="B", [32]="C" }
local cheat = { 1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4 }

local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local b={};if name and name~="" then b[name]=true end;joypad.set(b,1) end
local function nullable(value) if value==nil then return "null" end;return tostring(value) end

local function setup_case(case)
    memory.write_u8(config.ram.currentMapAddress,case.initialCurrentMap,"M68K BUS")
    memory.write_u8(config.ram.viewTargetEntityAddress,case.viewTargetSeed,"M68K BUS")
    memory.write_u8(config.ram.fadingSettingAddress,0,"M68K BUS")
    memory.write_u16_be(config.ram.layoutClearStartMarkerAddress,config.layoutMarkers.layoutClearStartMarkerSeed,"M68K BUS")
    memory.write_u16_be(config.ram.layoutClearEndMarkerAddress,config.layoutMarkers.layoutClearEndMarkerSeed,"M68K BUS")
    local input=config.instrumentation.ramInputAddress
    memory.write_u32_be(input,config["function"].handlerAddresses[case.macro],"M68K BUS")
    for index,word in ipairs(case.operandWords) do
        memory.write_u16_be(input+4+2*(index-1),word,"M68K BUS")
    end
end

local function boolean(value) if value then return "true" end;return "false" end
local function marker_facts(address,seed)
    local value=memory.read_u16_be(address,"M68K BUS")
    return value==0,value~=seed
end

local function finish(exit_code)
    if replay_state then memorysavestate.removestate(replay_state) end
    if exit_code~=0 then client.exitCode(exit_code);return end
    local f=assert(io.open(config.outputPath,"w"))
    f:write(string.format('{"system":"%s","core":"Genesis Plus GX","id":"%s","mapTest":%d,"recordOrder":[',emu.getsystemid(),config.fixtureId,config.mapTestIndex))
    for i,r in ipairs(records) do
        if i>1 then f:write(",") end
        f:write(string.format('"%s"',r.id))
    end
    f:write('],"records":[')
    for i,r in ipairs(records) do
        if i>1 then f:write(",") end
        f:write(string.format('{"id":"%s","handlerAddress":%d,"handlerReturned":true,"directCallSiteOrder":[',r.id,r.handlerAddress))
        for index,target in ipairs(r.directCallSiteOrder) do
            if index>1 then f:write(",") end
            f:write(string.format('"%s"',target))
        end
        f:write(string.format('],"loadMapD0WordAtCall":%s,"loadMapD1WordAtCall":%s,"tilesetD1WordAtCall":%s,"resetTailLoadMapD0WordAtTransfer":%s,"resetTailLoadMapD1WordAtTransfer":%s,"viewTargetEntityAfter":%d,"currentMapAfter":%d,"viewPlaneAPixelX":%d,"viewPlaneAPixelY":%d,"layoutClearStartMarkerCleared":%s,"layoutClearStartMarkerReplaced":%s,"layoutClearEndMarkerCleared":%s,"layoutClearEndMarkerReplaced":%s}',nullable(r.loadMapD0WordAtCall),nullable(r.loadMapD1WordAtCall),nullable(r.tilesetD1WordAtCall),nullable(r.resetTailLoadMapD0WordAtTransfer),nullable(r.resetTailLoadMapD1WordAtTransfer),r.viewTargetEntityAfter,r.currentMapAfter,r.viewPlaneAPixelX,r.viewPlaneAPixelY,boolean(r.layoutClearStartMarkerCleared),boolean(r.layoutClearStartMarkerReplaced),boolean(r.layoutClearEndMarkerCleared),boolean(r.layoutClearEndMarkerReplaced)))
    end
    f:write("]}\n");f:close();client.exitCode(0)
end

local function begin_case()
    if active then return end
    local case=config.cases[case_index]
    setup_case(case)
    active=true;handler_entered=false;continuation_entered=false
    direct_call_order={};load_map_d0_word_at_call=nil;load_map_d1_word_at_call=nil;tileset_d1_word_at_call=nil;reset_tail_load_map_d0_word_at_transfer=nil;reset_tail_load_map_d1_word_at_transfer=nil
    status("milestone:case:"..case.id)
end

local function observe_handler(macro,address)
    if not active then return end
    local case=config.cases[case_index]
    if macro==case.macro then
        handler_entered=true
        status("milestone:handler-entry:"..case.id)
    elseif case.macro=="loadMapFadeIn" and macro=="mapLoad" then
        continuation_entered=true
        status("milestone:fade-continuation:"..case.id)
    else
        error("unexpected map lifecycle handler entry at "..address)
    end
end

local function observe_direct_call(address,target)
    if not active then return end
    local case=config.cases[case_index]
    local expected=config["function"].callSitesByMacro[case.macro]
    local expected_site=expected[#direct_call_order+1]
    if expected_site==nil or expected_site.address~=address or expected_site.target~=target then
        error("map lifecycle direct call-site order drift")
    end
    direct_call_order[#direct_call_order+1]=target
    if target=="LoadMap" then
        load_map_d0_word_at_call=emu.getregister("M68K D0")&0xFFFF
        load_map_d1_word_at_call=emu.getregister("M68K D1")&0xFFFF
    elseif target=="LoadMapTilesets" then
        tileset_d1_word_at_call=emu.getregister("M68K D1")&0xFFFF
    elseif target=="WaitForVInt" and case.clearFadeAtFirstWait and #direct_call_order==2 then
        memory.write_u8(config.ram.fadingSettingAddress,0,"M68K BUS")
        status("milestone:fade-wait-released:"..case.id)
    end
end

local function observe_reset_tail()
    if not active or config.cases[case_index].macro~="resetMap" then return end
    reset_tail_load_map_d0_word_at_transfer=emu.getregister("M68K D0")&0xFFFF
    reset_tail_load_map_d1_word_at_transfer=emu.getregister("M68K D1")&0xFFFF
end

event.on_bus_exec(function()
    prompt_count=prompt_count+1;status("milestone:number-prompt-entry:"..prompt_count)
    if prompt_count==1 then stage="map";pending_save=true;pulse("C") end
end,config.harness["function"].numberPromptAddress,"sf2-map-lifecycle-number","M68K BUS")
event.on_bus_exec(function() status("milestone:flag-prompt-entry");pulse("B") end,config.harness["function"].flagPromptAddress,"sf2-map-lifecycle-flag","M68K BUS")
event.on_bus_exec(begin_case,config["function"].entryAddress,"sf2-map-lifecycle-entry","M68K BUS")

local function handler_observer(macro,address)
    return function() observe_handler(macro,address) end
end
for macro,address in pairs(config["function"].handlerAddresses) do
    event.on_bus_exec(handler_observer(macro,address),address,"sf2-map-lifecycle-handler-"..macro,"M68K BUS")
end
local function direct_call_observer(address,target)
    return function() observe_direct_call(address,target) end
end
local registered_call_sites={}
for _,sites in pairs(config["function"].callSitesByMacro) do
    for _,site in ipairs(sites) do
        if not registered_call_sites[site.address] then
            registered_call_sites[site.address]=true
            event.on_bus_exec(direct_call_observer(site.address,site.target),site.address,"sf2-map-lifecycle-call-"..site.address,"M68K BUS")
        end
    end
end
event.on_bus_exec(observe_reset_tail,config["function"].resetTailAddress,"sf2-map-lifecycle-reset-tail","M68K BUS")

event.on_bus_exec(function()
    if not active then return end
    local case=config.cases[case_index]
    if not handler_entered then error("map lifecycle handler did not enter") end
    if case.macro=="loadMapFadeIn" and not continuation_entered then error("map lifecycle fade did not continue") end
    if #direct_call_order~=#config["function"].callSitesByMacro[case.macro] then error("map lifecycle direct call count drift") end
    local start_cleared,start_replaced=marker_facts(config.ram.layoutClearStartMarkerAddress,config.layoutMarkers.layoutClearStartMarkerSeed)
    local end_cleared,end_replaced=marker_facts(config.ram.layoutClearEndMarkerAddress,config.layoutMarkers.layoutClearEndMarkerSeed)
    records[#records+1]={
        id=case.id,
        handlerAddress=config["function"].handlerAddresses[case.macro],
        directCallSiteOrder=direct_call_order,
        loadMapD0WordAtCall=load_map_d0_word_at_call,
        loadMapD1WordAtCall=load_map_d1_word_at_call,
        tilesetD1WordAtCall=tileset_d1_word_at_call,
        resetTailLoadMapD0WordAtTransfer=reset_tail_load_map_d0_word_at_transfer,
        resetTailLoadMapD1WordAtTransfer=reset_tail_load_map_d1_word_at_transfer,
        viewTargetEntityAfter=memory.read_u8(config.ram.viewTargetEntityAddress,"M68K BUS"),
        currentMapAfter=memory.read_u8(config.ram.currentMapAddress,"M68K BUS"),
        viewPlaneAPixelX=memory.read_u16_be(config.ram.viewPlaneAPixelXAddress,"M68K BUS"),
        viewPlaneAPixelY=memory.read_u16_be(config.ram.viewPlaneAPixelYAddress,"M68K BUS"),
        layoutClearStartMarkerCleared=start_cleared,
        layoutClearStartMarkerReplaced=start_replaced,
        layoutClearEndMarkerCleared=end_cleared,
        layoutClearEndMarkerReplaced=end_replaced
    }
    active=false;case_index=case_index+1
    if case_index>#config.cases then pending_finish=true else pending_replay=true end
end,config.instrumentation.postHandlerAddress,"sf2-map-lifecycle-return","M68K BUS")

local frames=0
while true do
    frames=frames+1
    if pending_finish then finish(0)
    elseif pending_save then
        pending_save=false;replay_state=memorysavestate.savecorestate();status("milestone:saved-map-prompt")
    elseif pending_replay then
        pending_replay=false;memorysavestate.loadcorestate(replay_state);queue={};pulse("C");status("milestone:replay-map-prompt")
    end
    if frames>=config.maxFrames then
        status("timeout:frame-budget-exhausted:case="..case_index..":stage="..stage)
        finish(1)
    end
    local button=nil
    if stage=="cheat" then
        local pointer=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS")
        if pointer>=0x28FF0 and pointer<0x29000 then button=names[cheat[pointer-0x28FF0+1]]
        elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Down" end
    elseif #queue>0 then button=table.remove(queue,1) end
    set_button(button);joypad.set({},2)
    emu.frameadvance()
    if frames%600==0 then status(string.format("frame=%d,stage=%s,case=%d",frames,stage,case_index)) end
end
