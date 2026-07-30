local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local stage, prompt_count, case_index = "cheat", 0, 1
local queue, records = {}, {}
local replay_state, pending_save, pending_replay, pending_finish = nil, false, false, false
local active, handler_entered, direct_call_seen = false, false, false
local scan_seen, terminator_seen, selected_seen = false, false, false
local direct_d0_word, direct_d1_word = nil, nil
local names = { [1]="Up", [2]="Down", [4]="Left", [8]="Right", [16]="B", [32]="C" }
local cheat = { 1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4 }

local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local b={};if name and name~="" then b[name]=true end;joypad.set(b,1) end
local function boolean(value) if value then return "true" end;return "false" end
local function nullable_number(value) if value==nil then return "null" end;return tostring(value) end

local function handler_address(case)
    if case.eventKind=="roof" then return config["function"].roofHandlerAddress end
    return config["function"].stepHandlerAddress
end

local function direct_call_site(case)
    if case.eventKind=="roof" then return config["function"].roofDirectCalleeCallSiteAddress end
    return config["function"].stepDirectCalleeCallSiteAddress
end

local function setup_case(case, derived)
    local state=case.initialState
    memory.write_u8(config.ram.currentMapAddress,state.currentMap,"M68K BUS")
    memory.write_u8(config.ram.currentBattleAddress,state.currentBattleByte,"M68K BUS")
    memory.write_u16_be(config.ram.busyWordAddress,state.busyWord,"M68K BUS")
    memory.write_u8(config.ram.mapAreaLayerTypeAddress,state.mapAreaLayerTypeByte,"M68K BUS")
    memory.write_u8(config.ram.updateToggleBitfieldAddress,state.updateToggleBitfieldSeed,"M68K BUS")
    if derived.layoutSourceWordAddress~=nil then
        memory.write_u16_be(derived.layoutSourceWordAddress,case.markerSeeds.layoutSourceWordSeed,"M68K BUS")
    end
    memory.write_u16_be(derived.layoutDestinationWordAddress,case.markerSeeds.layoutDestinationWordSeed,"M68K BUS")
    local input=config.instrumentation.ramInputAddress
    memory.write_u32_be(input,handler_address(case),"M68K BUS")
    memory.write_u16_be(input+4,case.triggerTile.x,"M68K BUS")
    memory.write_u16_be(input+6,case.triggerTile.y,"M68K BUS")
end

local function begin_case()
    if active then return end
    local case=config.cases[case_index]
    local derived=config.derived[case_index]
    setup_case(case,derived)
    active=true;handler_entered=false;direct_call_seen=false
    scan_seen=false;terminator_seen=false;selected_seen=false
    direct_d0_word=nil;direct_d1_word=nil
    status("milestone:case:"..case.id)
end

local function observe_handler(event_kind,address)
    return function()
        if not active then return end
        local case=config.cases[case_index]
        if case.eventKind~=event_kind or handler_address(case)~=address then
            error("map interaction trigger handler identity drift")
        end
        handler_entered=true
        status("milestone:handler-entry:"..case.id)
    end
end

local function observe_direct_call(event_kind,address)
    return function()
        if not active then return end
        local case=config.cases[case_index]
        if case.eventKind~=event_kind or direct_call_site(case)~=address or direct_call_seen then
            error("map interaction trigger direct call-site drift")
        end
        direct_call_seen=true
        direct_d0_word=emu.getregister("M68K D0")&0xFFFF
        direct_d1_word=emu.getregister("M68K D1")&0xFFFF
        status("milestone:direct-call:"..case.id)
    end
end

local function observe_scan(event_kind)
    return function()
        if not active then return end
        local case=config.cases[case_index]
        local derived=config.derived[case_index]
        if case.eventKind~=event_kind then return end
        scan_seen=true
        if emu.getregister("M68K A2")==derived.terminatorAddress then terminator_seen=true end
    end
end

local function observe_selected_record(event_kind)
    return function()
        if not active then return end
        local case=config.cases[case_index]
        local derived=config.derived[case_index]
        if case.eventKind~=event_kind then return end
        local expected=derived.selectedTableAddress+case.table.recordIndex*derived.recordStrideByteCount
        if emu.getregister("M68K A2")~=expected then
            error("map interaction trigger selected record pointer drift")
        end
        selected_seen=true
        status("milestone:selected-record:"..case.id)
    end
end

local function observed_boundary(case)
    if selected_seen then return "selected-record" end
    if terminator_seen then return "terminator" end
    if not scan_seen and (case.matchBoundary=="busy-gate" or case.matchBoundary=="battle-gate") then
        return case.matchBoundary
    end
    error("map interaction trigger match boundary was not observed")
end

local function append_record()
    local case=config.cases[case_index]
    local derived=config.derived[case_index]
    if not handler_entered or not direct_call_seen then
        error("map interaction trigger handler/callee callback did not execute")
    end
    if direct_d0_word~=derived.calleeD0WordAtDirectCall or direct_d1_word~=derived.calleeD1WordAtDirectCall then
        error("map interaction trigger direct-call word input drift")
    end
    local destination=memory.read_u16_be(derived.layoutDestinationWordAddress,"M68K BUS")
    local source_matches=nil
    if case.markerSeeds.layoutSourceWordSeed~=nil then
        source_matches=destination==case.markerSeeds.layoutSourceWordSeed
    end
    local toggles=memory.read_u8(config.ram.updateToggleBitfieldAddress,"M68K BUS")
    records[#records+1]={
        id=derived.id,
        handlerAddress=derived.handlerAddress,
        directCalleeCallSiteAddress=derived.directCalleeCallSiteAddress,
        calleeName=derived.calleeName,
        calleeD0WordAtDirectCall=direct_d0_word,
        calleeD1WordAtDirectCall=direct_d1_word,
        currentMapAfter=memory.read_u8(config.ram.currentMapAddress,"M68K BUS"),
        hashedTriggerTile=derived.hashedTriggerTile,
        layoutSourceWordAddress=derived.layoutSourceWordAddress,
        layoutDestinationWordAddress=derived.layoutDestinationWordAddress,
        selectedTableAddress=derived.selectedTableAddress,
        recordStrideByteCount=derived.recordStrideByteCount,
        terminatorAddress=derived.terminatorAddress,
        handlerReturned=true,
        matchBoundaryObserved=observed_boundary(case),
        terminatorBoundaryObserved=terminator_seen,
        layoutDestinationMarkerChanged=destination~=case.markerSeeds.layoutDestinationWordSeed,
        layoutDestinationMarkerMatchesSourceMarker=source_matches,
        updateToggleBit0Set=(toggles&1)~=0,
        updateToggleBit1Set=(toggles&2)~=0,
        busyWordAfter=memory.read_u16_be(config.ram.busyWordAddress,"M68K BUS"),
        currentBattleByteAfter=memory.read_u8(config.ram.currentBattleAddress,"M68K BUS")
    }
end

local function write_record(f,r)
    f:write(string.format('{"id":"%s","handlerAddress":%d,"directCalleeCallSiteAddress":%d,"calleeName":"%s","calleeD0WordAtDirectCall":%d,"calleeD1WordAtDirectCall":%d,"currentMapAfter":%d,"hashedTriggerTile":{"x":%d,"y":%d},"layoutSourceWordAddress":%s,"layoutDestinationWordAddress":%d,"selectedTableAddress":%d,"recordStrideByteCount":%d,"terminatorAddress":%d,"handlerReturned":true,"matchBoundaryObserved":"%s","terminatorBoundaryObserved":%s,"layoutDestinationMarkerChanged":%s,"layoutDestinationMarkerMatchesSourceMarker":%s,"updateToggleBit0Set":%s,"updateToggleBit1Set":%s,"busyWordAfter":%d,"currentBattleByteAfter":%d}',r.id,r.handlerAddress,r.directCalleeCallSiteAddress,r.calleeName,r.calleeD0WordAtDirectCall,r.calleeD1WordAtDirectCall,r.currentMapAfter,r.hashedTriggerTile.x,r.hashedTriggerTile.y,nullable_number(r.layoutSourceWordAddress),r.layoutDestinationWordAddress,r.selectedTableAddress,r.recordStrideByteCount,r.terminatorAddress,r.matchBoundaryObserved,boolean(r.terminatorBoundaryObserved),boolean(r.layoutDestinationMarkerChanged),nullable_number(r.layoutDestinationMarkerMatchesSourceMarker),boolean(r.updateToggleBit0Set),boolean(r.updateToggleBit1Set),r.busyWordAfter,r.currentBattleByteAfter))
end

local function finish(exit_code)
    if replay_state then memorysavestate.removestate(replay_state) end
    if exit_code~=0 then client.exitCode(exit_code);return end
    local f=assert(io.open(config.outputPath,"w"))
    f:write(string.format('{"system":"%s","core":"Genesis Plus GX","id":"%s","mapTest":%d,"recordOrder":[',emu.getsystemid(),config.fixtureId,config.mapTestIndex))
    for i,r in ipairs(records) do if i>1 then f:write(",") end;f:write(string.format('"%s"',r.id)) end
    f:write('],"records":[')
    for i,r in ipairs(records) do if i>1 then f:write(",") end;write_record(f,r) end
    f:write("]}\n");f:close();client.exitCode(0)
end

event.on_bus_exec(function()
    prompt_count=prompt_count+1;status("milestone:number-prompt-entry:"..prompt_count)
    if prompt_count==1 then stage="map";pending_save=true;pulse("C") end
end,config.harness["function"].numberPromptAddress,"sf2-map-interaction-trigger-number","M68K BUS")
event.on_bus_exec(function() status("milestone:flag-prompt-entry");pulse("B") end,config.harness["function"].flagPromptAddress,"sf2-map-interaction-trigger-flag","M68K BUS")
event.on_bus_exec(begin_case,config["function"].entryAddress,"sf2-map-interaction-trigger-entry","M68K BUS")
event.on_bus_exec(observe_handler("roof",config["function"].roofHandlerAddress),config["function"].roofHandlerAddress,"sf2-map-interaction-trigger-roof-handler","M68K BUS")
event.on_bus_exec(observe_handler("step",config["function"].stepHandlerAddress),config["function"].stepHandlerAddress,"sf2-map-interaction-trigger-step-handler","M68K BUS")
event.on_bus_exec(observe_direct_call("roof",config["function"].roofDirectCalleeCallSiteAddress),config["function"].roofDirectCalleeCallSiteAddress,"sf2-map-interaction-trigger-roof-call","M68K BUS")
event.on_bus_exec(observe_direct_call("step",config["function"].stepDirectCalleeCallSiteAddress),config["function"].stepDirectCalleeCallSiteAddress,"sf2-map-interaction-trigger-step-call","M68K BUS")
event.on_bus_exec(observe_scan("roof"),config["function"].roofTableScanAddress,"sf2-map-interaction-trigger-roof-scan","M68K BUS")
event.on_bus_exec(observe_scan("step"),config["function"].stepTableScanAddress,"sf2-map-interaction-trigger-step-scan","M68K BUS")
event.on_bus_exec(observe_selected_record("roof"),config["function"].roofSelectedRecordAddress,"sf2-map-interaction-trigger-roof-selected","M68K BUS")
event.on_bus_exec(observe_selected_record("step"),config["function"].stepSelectedRecordAddress,"sf2-map-interaction-trigger-step-selected","M68K BUS")
event.on_bus_exec(function()
    if not active then return end
    append_record();active=false;case_index=case_index+1
    if case_index>#config.cases then pending_finish=true else pending_replay=true end
end,config.instrumentation.postHandlerAddress,"sf2-map-interaction-trigger-return","M68K BUS")

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
        if pointer>=0x28FF0 and pointer<0x29000 then button=names[cheat[pointer-0x28FF0+1]]
        elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Down" end
    elseif #queue>0 then button=table.remove(queue,1) end
    set_button(button);joypad.set({},2);emu.frameadvance()
    if frames%600==0 then status(string.format("frame=%d,stage=%s,case=%d",frames,stage,case_index)) end
end
