local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local stage, prompt_count = "cheat", 0
local queue = {}
local action_started, target_supplied, playback = false, false, false
local construction, award, exp_reaction = nil, nil, nil
local ally_reactions, reaction_order = {}, {}
local active_ally, enemy_reaction = nil, nil
local names = { [1]="Up", [2]="Down", [4]="Left", [8]="Right", [16]="B", [32]="C" }
local cheat = { 1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4 }

local function status(value) local f=assert(io.open(config.statusPath,"w"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local b={};if name and name~="" then b[name]=true end;joypad.set(b,1) end
local function entry(c) local s=c;if c>=128 then s=c-96 end;return config.ram.combatantDataAddress+s*config.ram.combatantEntrySize end
local function word(name) return emu.getregister(name)&0xFFFF end
local function signed(v) if v>=0x8000 then return v-0x10000 end;return v end

local function write_result_and_exit()
    local actor,target=entry(config.case.actor),entry(config.case.target)
    local out=assert(io.open(config.outputPath,"w"))
    out:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","id":"%s","battle":%d,'..
        '"action":{"type":%d,"spell":%d,"target":%d},'..
        '"construction":{"randomRoll":%d,"unclampedTransfer":%d,"targetMp":%d,'..
        '"transfer":%d,"accumulatedExp":%d,"actorMp":%d,"actorStatus":%d,'..
        '"award":{"seed":%d,"halved":%d,"firstRoll":%d,"secondRoll":%d,"commandExp":%d}},'..
        '"replay":{"reactionOrder":[',
        emu.getsystemid(),config.case.id,memory.read_u8(config.harness.ram.currentBattleAddress,"M68K BUS"),
        memory.read_u16_be(config.ram.currentBattleActionAddress,"M68K BUS"),
        memory.read_u16_be(config.ram.currentBattleActionAddress+2,"M68K BUS"),config.case.target,
        construction.randomRoll,construction.unclampedTransfer,construction.targetMp,
        construction.transfer,construction.accumulatedExp,construction.actorMp,construction.actorStatus,
        award.seed,award.halved,award.firstRoll,award.secondRoll,award.commandExp))
    for i,item in ipairs(reaction_order) do if i>1 then out:write(",") end;out:write('"'..item..'"') end
    out:write('],"allyReactions":[')
    for i,r in ipairs(ally_reactions) do
        if i>1 then out:write(",") end
        out:write(string.format('{"mpChange":%d,"mpBefore":%d,"mpAfter":%d}',r.mpChange,r.mpBefore,r.mpAfter))
    end
    out:write(string.format(
        '],"enemyReaction":{"mpChange":%d,"mpBefore":%d,"mpAfter":%d},'..
        '"expReaction":{"commandExp":%d,"expBefore":%d,"expAfter":%d},'..
        '"finalActorMp":%d,"finalActorExp":%d,"finalActorStatus":%d,"finalTargetMp":%d}}\n',
        enemy_reaction.mpChange,enemy_reaction.mpBefore,enemy_reaction.mpAfter,
        exp_reaction.commandExp,exp_reaction.expBefore,exp_reaction.expAfter,
        memory.read_u8(actor+17,"M68K BUS"),memory.read_u8(actor+48,"M68K BUS"),
        memory.read_u16_be(actor+44,"M68K BUS"),memory.read_u8(target+17,"M68K BUS")))
    out:close();client.exitCode(0)
end

event.on_bus_exec(function() stage="ui" end,config.harness["function"].battleTestAddress,"sf2-mp-battle","M68K BUS")
event.on_bus_exec(function() prompt_count=prompt_count+1;if prompt_count==1 then pulse("Right");pulse("C") elseif prompt_count==2 then pulse("C") end end,config.harness["function"].numberPromptAddress,"sf2-mp-number","M68K BUS")
event.on_bus_exec(function() pulse("B") end,config.harness["function"].flagPromptAddress,"sf2-mp-flag","M68K BUS")
event.on_bus_exec(function()
    stage="battle";local a,t=entry(config.case.actor),entry(config.case.target)
    memory.write_u8(a+10,config.case.actorClass,"M68K BUS");memory.write_u8(a+11,1,"M68K BUS")
    memory.write_u16_be(a+12,100,"M68K BUS");memory.write_u16_be(a+14,100,"M68K BUS")
    memory.write_u8(a+16,config.case.actorMaxMp,"M68K BUS");memory.write_u8(a+17,config.case.actorInitialMp,"M68K BUS")
    memory.write_u8(a+23,99,"M68K BUS");memory.write_u8(a+31,0,"M68K BUS")
    for o=32,38,2 do memory.write_u16_be(a+o,0x007F,"M68K BUS") end
    memory.write_u16_be(a+44,config.case.actorInitialStatus,"M68K BUS");memory.write_u8(a+48,config.case.actorInitialExp,"M68K BUS")
    memory.write_u8(a+49,0x80,"M68K BUS");memory.write_u16_be(a+52,4,"M68K BUS")
    memory.write_u8(t+11,1,"M68K BUS");memory.write_u16_be(t+12,100,"M68K BUS");memory.write_u16_be(t+14,100,"M68K BUS")
    memory.write_u8(t+16,config.case.targetMaxMp,"M68K BUS");memory.write_u8(t+17,config.case.targetInitialMp,"M68K BUS")
    memory.write_u16_be(t+26,0,"M68K BUS");memory.write_u16_be(t+28,0,"M68K BUS");memory.write_u16_be(t+44,0,"M68K BUS")
    memory.write_u8(t+46,8,"M68K BUS");memory.write_u8(t+47,17,"M68K BUS");memory.write_u8(t+49,0x60,"M68K BUS");memory.write_u8(t+55,0,"M68K BUS")
    memory.write_u8(config.harness.ram.terrainDataAddress+17*48+8,3,"M68K BUS")
end,config.harness["function"].turnOrderEntryAddress,"sf2-mp-turn","M68K BUS")
event.on_bus_exec(function()
    if stage~="battle" or action_started or (emu.getregister("M68K D0")&0xFF)~=config.case.actor then return end
    action_started=true
    memory.write_u16_be(config.ram.currentBattleActionAddress,config.case.actionType,"M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress+2,config.case.actionSpell,"M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress+4,config.case.target,"M68K BUS")
end,config["function"].writeBattleSceneScriptAddress,"sf2-mp-action","M68K BUS")
event.on_bus_exec(function()
    if not action_started or target_supplied then return end;target_supplied=true
    memory.write_u16_be(config.ram.targetsListLengthAddress,1,"M68K BUS");memory.write_u8(config.ram.targetsListAddress,config.case.target,"M68K BUS")
end,config["function"].initializePropertiesAddress,"sf2-mp-target","M68K BUS")

event.on_bus_exec(function()
    if not action_started then return end
    memory.write_u16_be(config.ram.seedAddress,config.case.seed,"M68K BUS")
    construction={targetMp=memory.read_u8(entry(config.case.target)+17,"M68K BUS")}
end,config["function"].absorbEffectEntryAddress,"sf2-mp-effect","M68K BUS")
event.on_bus_exec(function() if construction then construction.randomRoll=word("M68K D0");construction.unclampedTransfer=construction.randomRoll+3 end end,config["function"].randomRollAddress,"sf2-mp-roll","M68K BUS")
event.on_bus_exec(function() if construction then construction.transfer=word("M68K D0") end end,config["function"].clampedTransferAddress,"sf2-mp-clamp","M68K BUS")
event.on_bus_exec(function() if construction then construction.accumulatedExp=memory.read_u16_be(config.ram.battleSceneExpAddress,"M68K BUS") end end,config["function"].statusExpAppliedAddress,"sf2-mp-exp","M68K BUS")

event.on_bus_exec(function()
    if not action_started then return end;award={seed=memory.read_u16_be(config.ram.seedAddress,"M68K BUS"),halved=word("M68K D1")}
end,config["function"].expHalvedAddress,"sf2-mp-half","M68K BUS")
event.on_bus_exec(function() if award then award.firstRoll=word("M68K D0") end end,config["function"].expFirstRollAddress,"sf2-mp-first","M68K BUS")
event.on_bus_exec(function() if award then award.secondRoll=word("M68K D0") end end,config["function"].expSecondRollAddress,"sf2-mp-second","M68K BUS")
event.on_bus_exec(function() if award then award.commandExp=word("M68K D1") end end,config["function"].expFinalAddress,"sf2-mp-final","M68K BUS")
event.on_bus_exec(function() if construction then local actor=entry(config.case.actor);construction.actorMp=memory.read_u8(actor+17,"M68K BUS");construction.actorStatus=memory.read_u16_be(actor+44,"M68K BUS");playback=true end end,config["function"].battleSceneEndReturnAddress,"sf2-mp-end","M68K BUS")

event.on_bus_exec(function()
    if not playback then return end;local a6=emu.getregister("M68K A6")&0xFFFFFF
    active_ally={mpChange=signed(memory.read_u16_be(a6+2,"M68K BUS")),mpBefore=memory.read_u8(entry(config.case.actor)+17,"M68K BUS")}
    reaction_order[#reaction_order+1]="ally:"..tostring(active_ally.mpChange)
end,config["function"].allyReactionEntryAddress,"sf2-mp-ally","M68K BUS")
event.on_bus_exec(function() if active_ally then active_ally.mpAfter=memory.read_u8(entry(config.case.actor)+17,"M68K BUS");ally_reactions[#ally_reactions+1]=active_ally;active_ally=nil end end,config["function"].allyReactionAppliedAddress,"sf2-mp-ally-applied","M68K BUS")
event.on_bus_exec(function()
    if not playback then return end;local a6=emu.getregister("M68K A6")&0xFFFFFF
    enemy_reaction={mpChange=signed(memory.read_u16_be(a6+2,"M68K BUS")),mpBefore=memory.read_u8(entry(config.case.target)+17,"M68K BUS")}
    reaction_order[#reaction_order+1]="enemy:"..tostring(enemy_reaction.mpChange)
end,config["function"].enemyReactionEntryAddress,"sf2-mp-enemy","M68K BUS")
event.on_bus_exec(function() if enemy_reaction then enemy_reaction.mpAfter=memory.read_u8(entry(config.case.target)+17,"M68K BUS") end end,config["function"].enemyMpAppliedAddress,"sf2-mp-enemy-applied","M68K BUS")
event.on_bus_exec(function() if not playback then return end;local a6=emu.getregister("M68K A6")&0xFFFFFF;exp_reaction={commandExp=memory.read_u16_be(a6,"M68K BUS"),expBefore=memory.read_u8(entry(config.case.actor)+48,"M68K BUS")} end,config["function"].giveExpEntryAddress,"sf2-mp-give-exp","M68K BUS")
event.on_bus_exec(function() if exp_reaction then exp_reaction.expAfter=memory.read_u8(entry(config.case.actor)+48,"M68K BUS") end end,config["function"].giveExpAppliedAddress,"sf2-mp-give-applied","M68K BUS")
event.on_bus_exec(function() if playback and #ally_reactions==2 and enemy_reaction and enemy_reaction.mpAfter~=nil and exp_reaction and exp_reaction.expAfter then write_result_and_exit() end end,config["function"].executeScriptEndAddress,"sf2-mp-script-end","M68K BUS")

local frames=0
while true do
    frames=frames+1;local button=nil
    if stage=="cheat" then local p=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS");if p>=0x28FF0 and p<0x29000 then button=names[cheat[p-0x28FF0+1]] elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Up" end
    elseif #queue>0 then button=table.remove(queue,1) elseif stage=="ui" and memory.read_u8(config.harness.ram.currentBattleAddress,"M68K BUS")==1 then button="C" elseif playback and frames%12<4 then button="C" end
    set_button(button);joypad.set({Start=((stage=="ui" and memory.read_u8(config.harness.ram.currentBattleAddress,"M68K BUS")==1) or playback)},2);emu.frameadvance()
    if frames%600==0 then status(string.format("frame=%d,stage=%s,pc=%X,allies=%d",frames,stage,emu.getregister("M68K PC"),#ally_reactions)) end
end
