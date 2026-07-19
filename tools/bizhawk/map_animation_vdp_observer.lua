local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local stage, prompt_count = "cheat", 0
local case_index, phase = 1, "boot"
local queue, records = {}, {}
local replay_state, pending_save, ready = nil, false, false
local sentinel = 0xA5
local names = { [1]="Up", [2]="Down", [4]="Left", [8]="Right", [16]="B", [32]="C" }
local cheat = { 1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4 }

local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local b={};if name and name~="" then b[name]=true end;joypad.set(b,1) end
local function pattern_byte(offset, seed) return (offset*37+seed*53+11)&0xFF end

local function target_is_sentinel(case)
    for offset=0,case.expected.byteCount-1 do
        if memory.read_u8(case.expected.targetByteAddress+offset,"VRAM")~=sentinel then return false end
    end
    return true
end

local function target_matches_source(case)
    for offset=0,case.expected.byteCount-1 do
        local expected=pattern_byte(case.expected.sourceByteOffset+offset,case.patternSeed)
        if memory.read_u8(case.expected.targetByteAddress+offset,"VRAM")~=expected then return false end
    end
    return true
end

local function setup_case(case)
    memory.write_u8(config.ram.currentMapAddress,case.map,"M68K BUS")
    memory.write_u32_be(config.ram.animationDataAddress,case.expected.dataAddressBefore,"M68K BUS")
    memory.write_u16_be(config.ram.animationCounterAddress,1,"M68K BUS")
    memory.write_u8(config.ram.animationMapAddress,case.map,"M68K BUS")
    memory.write_u8(config.ram.dmaQueueSizeAddress,0,"M68K BUS")
    memory.write_u32_be(config.ram.dmaQueuePointerAddress,config.ram.dmaQueueBaseAddress,"M68K BUS")
    for offset=0,config.ram.animationCacheCapacityBytes-1 do
        memory.write_u8(config.ram.animationCacheAddress+offset,pattern_byte(offset,case.patternSeed),"M68K BUS")
    end
    for offset=0,case.expected.byteCount-1 do
        memory.write_u8(case.expected.targetByteAddress+offset,sentinel,"VRAM")
    end
    return target_is_sentinel(case)
end

local function setup_control()
    memory.write_u32_be(config.ram.animationDataAddress,0,"M68K BUS")
    memory.write_u16_be(config.ram.animationCounterAddress,1,"M68K BUS")
    memory.write_u8(config.ram.dmaQueueSizeAddress,0,"M68K BUS")
    memory.write_u32_be(config.ram.dmaQueuePointerAddress,config.ram.dmaQueueBaseAddress,"M68K BUS")
end

local function finish()
    if replay_state then memorysavestate.removestate(replay_state) end
    local f=assert(io.open(config.outputPath,"w"))
    f:write(string.format('{"system":"%s","core":"Genesis Plus GX","id":"%s","mapTest":%d,"records":[',emu.getsystemid(),config.fixtureId,config.mapTestIndex))
    for index,record in ipairs(records) do
        if index>1 then f:write(",") end
        f:write(string.format('{"id":"%s","map":%d,"targetWasSentinelBeforeSubmit":%s,"targetWasSentinelAfterSubmit":%s,"targetMatchedSourceAfterTransfer":%s,"dataAddressAfterSubmit":%d,"dataAddressAfterTransfer":%d,"counterAfterSubmit":%d,"counterAfterTransfer":%d,"queueContributionAfterSubmit":%d,"queueContributionAfterTransfer":%d}',record.id,record.map,tostring(record.targetWasSentinelBeforeSubmit),tostring(record.targetWasSentinelAfterSubmit),tostring(record.targetMatchedSourceAfterTransfer),record.dataAddressAfterSubmit,record.dataAddressAfterTransfer,record.counterAfterSubmit,record.counterAfterTransfer,record.queueContributionAfterSubmit,record.queueContributionAfterTransfer))
    end
    f:write("]}\n");f:close();client.exitCode(0)
end

event.on_bus_exec(function()
    prompt_count=prompt_count+1
    if prompt_count==1 then stage="map";pulse("C");status("milestone:map-prompt") end
end,config.harness["function"].numberPromptAddress,"sf2-map-animation-number","M68K BUS")
event.on_bus_exec(function() pulse("B");status("milestone:flag-prompt") end,config.harness["function"].flagPromptAddress,"sf2-map-animation-flag","M68K BUS")
event.on_bus_exec(function()
    if not replay_state and not pending_save then pending_save=true;status("milestone:wait-for-event") end
end,config["function"].waitForEventAddress,"sf2-map-animation-ready","M68K BUS")
event.on_bus_exec(function() end,config["function"].animationAddress,"sf2-map-animation-vint","M68K BUS")
event.on_bus_exec(function() end,config["function"].processDmaQueueAddress,"sf2-map-animation-dma","M68K BUS")

local frames=0
while true do
    frames=frames+1
    if pending_save then
        pending_save=false;replay_state=memorysavestate.savecorestate();ready=true
        status("milestone:saved-exploration")
    end
    if ready and phase=="boot" then
        if case_index>1 then memorysavestate.loadcorestate(replay_state) end
        local case=config.cases[case_index]
        phase="control-first"
        records[case_index]={id=case.id,map=case.map}
        setup_control()
        status("milestone:case:"..case.id)
    elseif phase=="actual-pending" then
        memorysavestate.loadcorestate(replay_state)
        local case=config.cases[case_index]
        records[case_index].targetWasSentinelBeforeSubmit=setup_case(case)
        phase="submit"
    end

    local button=nil
    if stage=="cheat" then
        local pointer=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS")
        if pointer>=0x28FF0 and pointer<0x29000 then button=names[cheat[pointer-0x28FF0+1]]
        elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Down" end
    elseif #queue>0 then button=table.remove(queue,1) end
    set_button(button);joypad.set({},2)
    emu.frameadvance()

    if phase=="control-first" then
        records[case_index].controlQueueAfterSubmit=memory.read_u8(config.ram.dmaQueueSizeAddress,"M68K BUS")
        phase="control-second"
    elseif phase=="control-second" then
        records[case_index].controlQueueAfterTransfer=memory.read_u8(config.ram.dmaQueueSizeAddress,"M68K BUS")
        phase="actual-pending"
    elseif phase=="submit" then
        local case=config.cases[case_index];local record=records[case_index]
        record.targetWasSentinelAfterSubmit=target_is_sentinel(case)
        record.dataAddressAfterSubmit=memory.read_u32_be(config.ram.animationDataAddress,"M68K BUS")
        record.counterAfterSubmit=memory.read_u16_be(config.ram.animationCounterAddress,"M68K BUS")
        record.queueContributionAfterSubmit=memory.read_u8(config.ram.dmaQueueSizeAddress,"M68K BUS")-record.controlQueueAfterSubmit
        phase="transfer"
    elseif phase=="transfer" then
        local case=config.cases[case_index];local record=records[case_index]
        record.targetMatchedSourceAfterTransfer=target_matches_source(case)
        record.dataAddressAfterTransfer=memory.read_u32_be(config.ram.animationDataAddress,"M68K BUS")
        record.counterAfterTransfer=memory.read_u16_be(config.ram.animationCounterAddress,"M68K BUS")
        record.queueContributionAfterTransfer=memory.read_u8(config.ram.dmaQueueSizeAddress,"M68K BUS")-record.controlQueueAfterTransfer
        case_index=case_index+1
        if case_index>#config.cases then finish() else phase="boot" end
    end
    if frames%600==0 then status(string.format("frame=%d,stage=%s,case=%d,phase=%s",frames,stage,case_index,phase)) end
end
