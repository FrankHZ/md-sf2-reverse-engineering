local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local stage, prompt_count, case_index = "cheat", 0, 1
local queue, records = {}, {}
local replay_state, pending_save, pending_replay, active = nil, false, false, false
local names = { [1]="Up", [2]="Down", [4]="Left", [8]="Right", [16]="B", [32]="C" }
local cheat = { 1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4 }

local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local b={};if name and name~="" then b[name]=true end;joypad.set(b,1) end
local function signed16(value) if value >= 0x8000 then return value-0x10000 end;return value end

local function write_list(case, name)
    local list=case[name]; local ram=config.ram
    memory.write_u16_be(ram[name.."CountAddress"],#list.targets,"M68K BUS")
    for i=1,#list.targets do
        memory.write_u8(ram[name.."TargetsAddress"]+i-1,list.targets[i],"M68K BUS")
        memory.write_u8(ram[name.."MovementsAddress"]+i-1,list.movements[i],"M68K BUS")
        memory.write_u8(ram[name.."PrioritiesAddress"]+i-1,list.priorities[i],"M68K BUS")
    end
end

local function finish()
    if replay_state then memorysavestate.removestate(replay_state) end
    local f=assert(io.open(config.outputPath,"w"))
    f:write(string.format('{"system":"%s","core":"Genesis Plus GX","id":"%s","battle":%d,"records":[',emu.getsystemid(),config.fixtureId,config.battleId))
    for i,r in ipairs(records) do
        if i>1 then f:write(",") end
        f:write(string.format('{"id":"%s","seed":%d,"thinkingSeed":%d,"roll":%d,"finalSeed":%d,"finalThinkingSeed":%d,"target":%d,"priority":%d,"action":%d}',r.id,r.seed,r.thinkingSeed,r.roll,r.finalSeed,r.finalThinkingSeed,r.target,r.priority,r.action))
    end
    f:write("]}\n");f:close();client.exitCode(0)
end

event.on_bus_exec(function() stage="ui" end,config.harness["function"].battleTestAddress,"sf2-ai-choice-battle","M68K BUS")
event.on_bus_exec(function() prompt_count=prompt_count+1;if prompt_count==1 then pulse("Right");pulse("C") elseif prompt_count==2 then pulse("C") end end,config.harness["function"].numberPromptAddress,"sf2-ai-choice-number","M68K BUS")
event.on_bus_exec(function() pulse("B") end,config.harness["function"].flagPromptAddress,"sf2-ai-choice-flag","M68K BUS")
event.on_bus_exec(function()
    stage="battle";memory.write_u8(config.ram.autoBattleToggleAddress,0xFF,"M68K BUS");pending_save=true
end,config.harness["function"].turnOrderEntryAddress,"sf2-ai-choice-turn","M68K BUS")

event.on_bus_exec(function()
    if active then return end
    local case=config.cases[case_index]
    write_list(case,"attack");write_list(case,"spell");write_list(case,"item")
    memory.write_u16_be(config.ram.spellEntryAddress,case.spellEntry,"M68K BUS")
    memory.write_u16_be(config.ram.seedAddress,case.seed,"M68K BUS")
    memory.write_u8(config.ram.thinkingSeedCopyAddress,case.thinkingSeed,"M68K BUS")
    active=true;status("milestone:case:"..case.id)
end,config["function"].entryAddress,"sf2-ai-choice-entry","M68K BUS")

event.on_bus_exec(function()
    if not active then return end
    local case=config.cases[case_index]
    local final_seed=memory.read_u16_be(config.ram.seedAddress,"M68K BUS")
    local final_thinking_seed=memory.read_u8(config.ram.thinkingSeedCopyAddress,"M68K BUS")
    local roll=-1
    if #case.spell.targets>0 and #case.item.targets>0 then
        roll=final_thinking_seed
    elseif final_seed~=case.seed then
        roll=math.floor(final_seed*6/65536)
    end
    records[#records+1]={id=case.id,seed=case.seed,thinkingSeed=case.thinkingSeed,roll=roll,finalSeed=final_seed,finalThinkingSeed=final_thinking_seed,
        target=signed16(emu.getregister("M68K D0")&0xFFFF),priority=emu.getregister("M68K D1")&0xFFFF,
        action=emu.getregister("M68K D2")&0xFF}
    active=false;case_index=case_index+1
    if case_index>#config.cases then finish() else pending_replay=true end
end,config["function"].callerReturnAddress,"sf2-ai-choice-return","M68K BUS")

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
