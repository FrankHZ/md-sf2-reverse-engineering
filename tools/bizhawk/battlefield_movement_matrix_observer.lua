local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local bootstrap = assert(dofile(config.bootstrapLibraryPath))
local stage, prompt_count, case_index = "cheat", 0, 1
local queue, records = {}, {}
local replay_state, pending_save, pending_replay, active = nil, false, false, false
local expansion_order, out_of_range_calls = {}, 0
local names = { [1]="Up", [2]="Down", [4]="Left", [8]="Right", [16]="B", [32]="C" }
local cheat = { 1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4 }

local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local b={};if name and name~="" then b[name]=true end;joypad.set(b,1) end
local function word_register(name) return emu.getregister(name) & 0xFFFF end

local function write_int_array(file, values)
    file:write("[")
    for i,value in ipairs(values) do if i>1 then file:write(",") end;file:write(tostring(value)) end
    file:write("]")
end

local function setup_case(case)
    local ram=config.ram
    for offset=0,2303 do
        memory.write_u8(ram.battleTerrainAddress+offset,case.terrainDefault,"M68K BUS")
        memory.write_u8(ram.targetGridAddress+offset,0xFF,"M68K BUS")
    end
    for _,range in ipairs(case.terrainRanges) do
        for offset=range.start,range["end"] do memory.write_u8(ram.battleTerrainAddress+offset,range.value,"M68K BUS") end
    end
    for _,entry in ipairs(case.terrainEntries) do memory.write_u8(ram.battleTerrainAddress+entry.offset,entry.value,"M68K BUS") end
    for index,value in ipairs(case.moveCosts) do memory.write_u8(ram.moveCostsTableAddress+index-1,value&0xFF,"M68K BUS") end
    expansion_order={};out_of_range_calls=0
end

local function prepare_combatant(case)
    local combatant=word_register("M68K D0")&0xFF
    local index=combatant;if combatant>=128 then index=combatant-96 end
    local entry=config.ram.combatantDataAddress+index*56
    memory.write_u8(entry+25,math.floor(case.budget/2),"M68K BUS")
    memory.write_u8(entry+46,case.startOffset%48,"M68K BUS")
    memory.write_u8(entry+47,math.floor(case.startOffset/48),"M68K BUS")
end

local function observe_case(case)
    local ram=config.ram;local reachable_count=0;local maximum_cost=-1
    for offset=0,2303 do
        local high=memory.read_u8(ram.movableGridAddress+offset,"M68K BUS")
        if high<0x80 then
            local cost=high*256+memory.read_u8(ram.totalMoveCostsAddress+offset,"M68K BUS")
            reachable_count=reachable_count+1;if cost>maximum_cost then maximum_cost=cost end
        end
    end
    local probes={}
    for _,offset in ipairs(case.probeOffsets) do
        local high=memory.read_u8(ram.movableGridAddress+offset,"M68K BUS");local cost=-1
        if high<0x80 then cost=high*256+memory.read_u8(ram.totalMoveCostsAddress+offset,"M68K BUS") end
        probes[#probes+1]={offset=offset,cost=cost}
    end
    return {id=case.id,budget=case.budget,startOffset=case.startOffset,reachableCount=reachable_count,
        maximumCost=maximum_cost,expansionOrder=expansion_order,outOfRangeNeighborCalls=out_of_range_calls,probes=probes}
end

local function finish()
    if replay_state then memorysavestate.removestate(replay_state) end
    local f=assert(io.open(config.outputPath,"w"))
    f:write(string.format('{"system":"%s","core":"Genesis Plus GX","id":"%s","battle":%d,"records":[',emu.getsystemid(),config.fixtureId,config.battleId))
    for i,r in ipairs(records) do
        if i>1 then f:write(",") end
        f:write(string.format('{"id":"%s","budget":%d,"startOffset":%d,"reachableCount":%d,"maximumCost":%d,"expansionOrder":',r.id,r.budget,r.startOffset,r.reachableCount,r.maximumCost))
        write_int_array(f,r.expansionOrder)
        f:write(string.format(',"outOfRangeNeighborCalls":%d,"probes":[',r.outOfRangeNeighborCalls))
        for j,p in ipairs(r.probes) do if j>1 then f:write(",") end;f:write(string.format('{"offset":%d,"cost":%d}',p.offset,p.cost)) end
        f:write("]}")
    end
    f:write("]}\n");f:close();client.exitCode(0)
end

event.on_bus_exec(function() stage="ui" end,config.harness["function"].battleTestAddress,"sf2-field-battle","M68K BUS")
event.on_bus_exec(function() prompt_count=prompt_count+1;bootstrap.battle01_intro_skip(config.bootstrap.profile,prompt_count,pulse) end,config.harness["function"].numberPromptAddress,"sf2-field-number","M68K BUS")
event.on_bus_exec(function() pulse("B") end,config.harness["function"].flagPromptAddress,"sf2-field-flag","M68K BUS")
event.on_bus_exec(function() stage="battle";memory.write_u8(config.ram.autoBattleToggleAddress,0xFF,"M68K BUS");pending_save=true end,config.harness["function"].turnOrderEntryAddress,"sf2-field-turn","M68K BUS")

event.on_bus_exec(function()
    if active then return end
    prepare_combatant(config.cases[case_index]);active=true;status("milestone:case:"..config.cases[case_index].id)
end,config["function"].initializeAddress,"sf2-field-initialize","M68K BUS")

event.on_bus_exec(function() if active then setup_case(config.cases[case_index]) end end,config["function"].entryAddress,"sf2-field-entry","M68K BUS")

event.on_bus_exec(function() if active then expansion_order[#expansion_order+1]=word_register("M68K D5") end end,config["function"].expansionAddress,"sf2-field-expand","M68K BUS")
event.on_bus_exec(function() if active and word_register("M68K D5")>=2304 then out_of_range_calls=out_of_range_calls+1 end end,config["function"].neighborEntryAddress,"sf2-field-neighbor","M68K BUS")
event.on_bus_exec(function()
    if not active then return end
    records[#records+1]=observe_case(config.cases[case_index]);active=false;case_index=case_index+1
    if case_index>#config.cases then finish() else pending_replay=true end
end,config["function"].returnAddress,"sf2-field-return","M68K BUS")

local frames=0
while true do
    frames=frames+1
    if pending_save then pending_save=false;replay_state=memorysavestate.savecorestate();status("milestone:saved")
    elseif pending_replay then pending_replay=false;memorysavestate.loadcorestate(replay_state) end
    local button=nil
    if stage=="cheat" then
        local pointer=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS")
        if pointer>=0x28FF0 and pointer<0x29000 then button=names[cheat[pointer-0x28FF0+1]]
        elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Up" end
    elseif #queue>0 then button=table.remove(queue,1)
    elseif stage=="ui" and memory.read_u8(config.harness.ram.currentBattleAddress,"M68K BUS")==1 then button="C" end
    set_button(button);joypad.set({Start=(stage=="ui" and memory.read_u8(config.harness.ram.currentBattleAddress,"M68K BUS")==1)},2)
    emu.frameadvance()
    if frames%600==0 then status(string.format("frame=%d,stage=%s,case=%d",frames,stage,case_index)) end
end
