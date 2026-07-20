local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local stage, prompt_count = "cheat", 0
local case_index, tick = 1, 0
local queue, records = {}, {}
local replay_state, pending_save, pending_replay, pending_finish = nil, false, false, false
local ready, active = false, false
local names = { [1]="Up", [2]="Down", [4]="Left", [8]="Right", [16]="B", [32]="C" }
local cheat = { 1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4 }

local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local b={};if name and name~="" then b[name]=true end;joypad.set(b,1) end
local function signed16(value) if value>=0x8000 then return value-0x10000 end;return value end
local function write_word(address,value) memory.write_u16_be(address,value&0xFFFF,"M68K BUS") end

local function write_entity(address, entity, script_address)
    for offset=0,config.ram.entitySize-1 do memory.write_u8(address+offset,0,"M68K BUS") end
    write_word(address,entity.x);write_word(address+2,entity.y)
    write_word(address+4,entity.xVelocity);write_word(address+6,entity.yVelocity)
    write_word(address+8,entity.xTravel);write_word(address+10,entity.yTravel)
    write_word(address+12,entity.xDest);write_word(address+14,entity.yDest)
    memory.write_u8(address+16,entity.facing,"M68K BUS")
    memory.write_u8(address+17,entity.layer,"M68K BUS")
    memory.write_u8(address+18,entity.entityNumber,"M68K BUS")
    memory.write_u8(address+19,entity.mapSprite,"M68K BUS")
    memory.write_u32_be(address+20,script_address,"M68K BUS")
    memory.write_u8(address+24,entity.xAccel,"M68K BUS")
    memory.write_u8(address+25,entity.yAccel,"M68K BUS")
    memory.write_u8(address+26,entity.xSpeed,"M68K BUS")
    memory.write_u8(address+27,entity.ySpeed,"M68K BUS")
    memory.write_u8(address+28,entity.flagsA,"M68K BUS")
    memory.write_u8(address+29,entity.flagsB,"M68K BUS")
    memory.write_u8(address+30,entity.animCounter,"M68K BUS")
    memory.write_u8(address+31,entity.waitTimer,"M68K BUS")
end

local function write_script(script)
    local address=config.ram.scriptAddress
    for offset=0,15 do memory.write_u8(address+offset,0,"M68K BUS") end
    if script.kind=="wait" then
        write_word(address,0);write_word(address+2,script.timer);write_word(address+4,0x24)
    elseif script.kind=="relative" then
        write_word(address,4);write_word(address+2,script.x);write_word(address+4,script.y);write_word(address+6,0x24)
    elseif script.kind=="absolute" then
        write_word(address,5);write_word(address+2,script.x);write_word(address+4,script.y);write_word(address+6,0x24)
    elseif script.kind~="none" then error("unsupported script kind: "..script.kind) end
end

local function setup_case(case)
    local base=config.ram.entityDataAddress
    for index=1,config.ram.entityCount-1 do
        local address=base+index*config.ram.entitySize
        for offset=0,config.ram.entitySize-1 do memory.write_u8(address+offset,0,"M68K BUS") end
        write_word(address,0x7000)
    end
    write_script(case.script)
    local script_address=0
    if case.script.kind~="none" then script_address=config.ram.scriptAddress end
    write_entity(base,case.entity,script_address)
    if case.blocker then
        local blocker=case.blocker;local address=base+config.ram.entitySize
        write_word(address,blocker.x);write_word(address+2,blocker.y)
        write_word(address+12,blocker.xDest);write_word(address+14,blocker.yDest)
    end
    memory.write_u8(config.ram.spritesToLoadAddress,0,"M68K BUS")
end

local function snapshot()
    local base=config.ram.entityDataAddress
    local pointer=memory.read_u32_be(base+20,"M68K BUS")
    local script_offset=false;if pointer~=0 then script_offset=pointer-config.ram.scriptAddress end
    return {
        tick,
        signed16(memory.read_u16_be(base,"M68K BUS")),signed16(memory.read_u16_be(base+2,"M68K BUS")),
        signed16(memory.read_u16_be(base+4,"M68K BUS")),signed16(memory.read_u16_be(base+6,"M68K BUS")),
        memory.read_u16_be(base+8,"M68K BUS"),memory.read_u16_be(base+10,"M68K BUS"),
        signed16(memory.read_u16_be(base+12,"M68K BUS")),signed16(memory.read_u16_be(base+14,"M68K BUS")),
        memory.read_u8(base+16,"M68K BUS"),memory.read_u8(base+17,"M68K BUS"),memory.read_u8(base+29,"M68K BUS"),
        memory.read_u8(base+30,"M68K BUS"),memory.read_u8(base+31,"M68K BUS"),script_offset
    }
end

local function write_string_array(file, values)
    file:write("[")
    for index,value in ipairs(values) do if index>1 then file:write(",") end;file:write(string.format('"%s"',value)) end
    file:write("]")
end

local function write_state(file, state)
    file:write("[")
    for index=1,15 do
        if index>1 then file:write(",") end
        if state[index]==false then file:write("null") else file:write(tostring(state[index])) end
    end
    file:write("]")
end

local function finish()
    if replay_state then memorysavestate.removestate(replay_state) end
    local f=assert(io.open(config.outputPath,"w"))
    f:write(string.format('{"system":"%s","core":"Genesis Plus GX","id":"%s","mapTest":%d,"stateFields":',emu.getsystemid(),config.fixtureId,config.mapTestIndex))
    write_string_array(f,config.stateFields)
    f:write(',"records":[')
    for index,record in ipairs(records) do
        if index>1 then f:write(",") end
        f:write(string.format('{"id":"%s","states":[',record.id))
        for state_index,state in ipairs(record.states) do if state_index>1 then f:write(",") end;write_state(f,state) end
        f:write("]}")
    end
    f:write("]}\n");f:close();client.exitCode(0)
end

event.on_bus_exec(function()
    prompt_count=prompt_count+1
    if prompt_count==1 then stage="map";pulse("C");status("milestone:map-prompt") end
end,config.harness["function"].numberPromptAddress,"sf2-entity-movement-number","M68K BUS")
event.on_bus_exec(function() pulse("B");status("milestone:flag-prompt") end,config.harness["function"].flagPromptAddress,"sf2-entity-movement-flag","M68K BUS")
event.on_bus_exec(function()
    if not replay_state and not pending_save then pending_save=true;status("milestone:wait-for-event") end
end,config["function"].waitForEventAddress,"sf2-entity-movement-ready","M68K BUS")

event.on_bus_exec(function()
    if not ready or active then return end
    if (emu.getregister("M68K A0")&0xFFFFFF)~=config.ram.entityDataAddress then return end
    local case=config.cases[case_index]
    setup_case(case);tick=0;active=true
    records[case_index]={id=case.id,states={}}
    status("milestone:case:"..case.id)
end,config["function"].updateAddress,"sf2-entity-movement-update","M68K BUS")

event.on_bus_exec(function()
    if not active then return end
    local case=config.cases[case_index]
    if case.arrivalTileWord==nil then return end
    local offset=emu.getregister("M68K D2")&0xFFFF
    write_word(config.ram.mapBlockDataAddress+offset,case.arrivalTileWord)
end,config["function"].convertReturnAddress,"sf2-entity-movement-arrival","M68K BUS")

event.on_bus_exec(function()
    if not active or (emu.getregister("M68K A0")&0xFFFFFF)~=config.ram.entityDataAddress then return end
    tick=tick+1
    local record=records[case_index];record.states[#record.states+1]=snapshot()
    if tick>=config.cases[case_index].ticks then
        active=false;case_index=case_index+1
        if case_index>#config.cases then pending_finish=true else pending_replay=true end
    end
end,config["function"].nextEntityAddress,"sf2-entity-movement-next","M68K BUS")

local frames=0
while true do
    frames=frames+1
    if pending_finish then finish()
    elseif pending_save then
        pending_save=false;replay_state=memorysavestate.savecorestate();ready=true
        status("milestone:saved-exploration")
    elseif pending_replay then
        pending_replay=false;memorysavestate.loadcorestate(replay_state);queue={};tick=0
        status("milestone:replay-exploration")
    end
    local button=nil
    if stage=="cheat" then
        local pointer=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS")
        if pointer>=0x28FF0 and pointer<0x29000 then button=names[cheat[pointer-0x28FF0+1]]
        elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Down" end
    elseif #queue>0 then button=table.remove(queue,1) end
    set_button(button);joypad.set({},2);emu.frameadvance()
    if frames%600==0 then status(string.format("frame=%d,stage=%s,case=%d,tick=%d",frames,stage,case_index,tick)) end
end
