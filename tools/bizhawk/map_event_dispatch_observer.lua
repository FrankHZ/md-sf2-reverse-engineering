local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local stage, prompt_count, case_index = "cheat", 0, 1
local queue, records = {}, {}
local replay_state, pending_save, pending_replay, pending_finish = nil, false, false, false
local active = false
local names = { [1]="Up", [2]="Down", [4]="Left", [8]="Right", [16]="B", [32]="C" }
local cheat = { 1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4 }

local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local b={};if name and name~="" then b[name]=true end;joypad.set(b,1) end
local function nullable(value) if value==nil then return "null" end;return tostring(value) end

local function setup_case(case)
    memory.write_u8(config.ram.currentMapAddress,case.map,"M68K BUS")
    for offset=0,127 do memory.write_u8(config.ram.gameFlagsAddress+offset,0,"M68K BUS") end
    for _,flag in ipairs(case.setFlags) do
        local address=config.ram.gameFlagsAddress+math.floor(flag/8)
        local mask=0x80 >> (flag%8)
        memory.write_u8(address,memory.read_u8(address,"M68K BUS")|mask,"M68K BUS")
    end
    local query=case.query
    local input=config.instrumentation.ramInputAddress
    memory.write_u32_be(input,config["function"].entries[case.category],"M68K BUS")
    memory.write_u16_be(input+4,query.entity or 0,"M68K BUS")
    memory.write_u16_be(input+6,query.x or 0,"M68K BUS")
    memory.write_u16_be(input+8,query.y or 0,"M68K BUS")
    memory.write_u16_be(input+10,query.facing or 0,"M68K BUS")
    memory.write_u16_be(input+12,query.item or 0,"M68K BUS")
    memory.write_u16_be(input+14,0,"M68K BUS")
end

local function finish()
    if replay_state then memorysavestate.removestate(replay_state) end
    local f=assert(io.open(config.outputPath,"w"))
    f:write(string.format('{"system":"%s","core":"Genesis Plus GX","id":"%s","mapTest":%d,"records":[',emu.getsystemid(),config.fixtureId,config.mapTestIndex))
    for i,r in ipairs(records) do
        if i>1 then f:write(",") end
        f:write(string.format('{"id":"%s","category":"%s","selectedRecordOffset":%d,"selectedTargetAddress":%d,"eventFlags":%s,"maskedItem":%s}',r.id,r.category,r.selectedRecordOffset,r.selectedTargetAddress,nullable(r.eventFlags),nullable(r.maskedItem)))
    end
    f:write("]}\n");f:close();client.exitCode(0)
end

local function observe_selection(index)
    if not active then return end
    local case=config.cases[case_index]
    if index~=case_index then error("map event target dispatch drift") end
    local category=case.category
    local event_flags=nil
    local masked_item=nil
    if category=="entityEvents" then event_flags=emu.getregister("M68K D6")&0xFF end
    if category=="itemEvents" then masked_item=emu.getregister("M68K D4")&0xFFFF end
    records[#records+1]={
        id=case.id,
        category=category,
        selectedRecordOffset=emu.getregister("M68K D7")&0xFFFF,
        selectedTargetAddress=emu.getregister("M68K A0")&0xFFFFFF,
        eventFlags=event_flags,
        maskedItem=masked_item
    }
    active=false;case_index=case_index+1
    if case_index>#config.cases then pending_finish=true else pending_replay=true end
end

event.on_bus_exec(function()
    prompt_count=prompt_count+1;status("milestone:number-prompt-entry:"..prompt_count)
    if prompt_count==1 then stage="map";pending_save=true;pulse("C") end
end,config.harness["function"].numberPromptAddress,"sf2-map-event-number","M68K BUS")
event.on_bus_exec(function() status("milestone:flag-prompt-entry");pulse("B") end,config.harness["function"].flagPromptAddress,"sf2-map-event-flag","M68K BUS")

event.on_bus_exec(function()
    if active then return end
    local case=config.cases[case_index]
    setup_case(case);active=true;status("milestone:case:"..case.id)
end,config["function"].setupAddress,"sf2-map-event-setup","M68K BUS")

local function target_observer(index)
    return function() observe_selection(index) end
end
for index,case in ipairs(config.cases) do
    event.on_bus_exec(target_observer(index),case.expected.selectedTargetAddress,"sf2-map-event-target-"..index,"M68K BUS")
end

local frames=0
while true do
    frames=frames+1
    if pending_finish then finish()
    elseif pending_save then
        pending_save=false;replay_state=memorysavestate.savecorestate();status("milestone:saved-map-prompt")
    elseif pending_replay then
        pending_replay=false;memorysavestate.loadcorestate(replay_state);queue={};pulse("C");status("milestone:replay-map-prompt")
    end
    local button=nil
    if stage=="cheat" then
        local pointer=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS")
        if pointer>=0x28FF0 and pointer<0x29000 then button=names[cheat[pointer-0x28FF0+1]]
        elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Down" end
    elseif #queue>0 then button=table.remove(queue,1)
    end
    set_button(button);joypad.set({},2)
    emu.frameadvance()
    if frames%600==0 then status(string.format("frame=%d,stage=%s,case=%d",frames,stage,case_index)) end
end
