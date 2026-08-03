local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local bootstrap = assert(dofile(config.bootstrapLibraryPath))
local stage, prompt_count = "cheat", 0
local queue = {}
local action_started, targets_supplied, playback = false, false, false
local records, award, ally_reaction, exp_reaction = {}, nil, nil, nil
local enemy_reactions, reaction_order = {}, {}
local active, active_enemy, construction_actor_mp = nil, nil, nil
local names = { [1]="Up", [2]="Down", [4]="Left", [8]="Right", [16]="B", [32]="C" }
local cheat = { 1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4 }

local function status(value) local f=assert(io.open(config.statusPath,"w"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local b={};if name and name~="" then b[name]=true end;joypad.set(b,1) end
local function entry(c) local s=c;if c>=128 then s=c-96 end;return config.ram.combatantDataAddress+s*config.ram.combatantEntrySize end
local function word(name) return emu.getregister(name)&0xFFFF end
local function signed(v) if v>=0x8000 then return v-0x10000 end;return v end
local function target_case(combatant)
    for _,target in ipairs(config.case.targets) do if target.combatant==combatant then return target end end
    error("DESOUL target is absent from fixture cases")
end

local function write_result_and_exit()
    local actor=entry(config.case.actor)
    local final_gold=memory.read_u32_be(config.ram.currentGoldAddress,"M68K BUS")
    local out=assert(io.open(config.outputPath,"w"))
    out:write(string.format(
        '{"system":"%s","core":"Genesis Plus GX","id":"%s","battle":%d,'..
        '"action":{"type":%d,"spell":%d,"target":%d},"construction":{"records":[',
        emu.getsystemid(),config.case.id,memory.read_u8(config.harness.ram.currentBattleAddress,"M68K BUS"),
        memory.read_u16_be(config.ram.currentBattleActionAddress,"M68K BUS"),
        memory.read_u16_be(config.ram.currentBattleActionAddress+2,"M68K BUS"),config.case.targets[1].combatant))
    for i,r in ipairs(records) do
        if i>1 then out:write(",") end
        out:write(string.format(
            '{"combatant":%d,"setting":%d,"threshold":%d,"roll":%d,"success":%s,'..
            '"reactionEmitted":%s,"accumulatedExp":%d,"accumulatedGold":%d,'..
            '"targetDiesFlag":%d,"targetHp":%d}',
            r.combatant,r.setting,r.threshold,r.roll,tostring(r.success),tostring(r.reactionEmitted),
            r.accumulatedExp,r.accumulatedGold,r.targetDiesFlag,r.targetHp))
    end
    out:write(string.format(
        '],"accumulatedExp":%d,"accumulatedGold":%d,"actorMp":%d,'..
        '"award":{"seed":%d,"halved":%d,"firstRoll":%d,"secondRoll":%d,'..
        '"commandExp":%d,"commandGold":%d}},"replay":{"reactionOrder":[',
        records[#records].accumulatedExp,records[#records].accumulatedGold,construction_actor_mp,
        award.seed,award.halved,award.firstRoll,award.secondRoll,award.commandExp,award.commandGold))
    for i,item in ipairs(reaction_order) do if i>1 then out:write(",") end;out:write('"'..item..'"') end
    out:write(string.format(
        '],"allyReaction":{"combatant":%d,"mpChange":%d,"mpBefore":%d,"mpAfter":%d},'..
        '"enemyReactions":[',
        ally_reaction.combatant,ally_reaction.mpChange,ally_reaction.mpBefore,ally_reaction.mpAfter))
    for i,r in ipairs(enemy_reactions) do
        if i>1 then out:write(",") end
        out:write(string.format(
            '{"combatant":%d,"hpChange":%d,"hpBefore":%d,"hpAfter":%d}',
            r.combatant,r.hpChange,r.hpBefore,r.hpAfter))
    end
    out:write(string.format(
        '],"expReaction":{"commandExp":%d,"expBefore":%d,"expAfter":%d},'..
        '"goldReaction":{"commandGold":%d,"goldBefore":0,"goldAfter":%d},'..
        '"finalActorMp":%d,"finalActorExp":%d,"finalTargetHp":[',
        exp_reaction.commandExp,exp_reaction.expBefore,exp_reaction.expAfter,
        award.commandGold,final_gold,memory.read_u8(actor+17,"M68K BUS"),memory.read_u8(actor+48,"M68K BUS")))
    for i,target in ipairs(config.case.targets) do
        if i>1 then out:write(",") end
        out:write(tostring(memory.read_u16_be(entry(target.combatant)+14,"M68K BUS")))
    end
    out:write(string.format('],"finalGold":%d}}\n',final_gold));out:close();client.exitCode(0)
end

event.on_bus_exec(function() stage="ui" end,config.harness["function"].battleTestAddress,"sf2-desoul-battle","M68K BUS")
event.on_bus_exec(function() prompt_count=prompt_count+1;bootstrap.battle01_intro_skip(config.bootstrap.profile,prompt_count,pulse) end,config.harness["function"].numberPromptAddress,"sf2-desoul-number","M68K BUS")
event.on_bus_exec(function() pulse("B") end,config.harness["function"].flagPromptAddress,"sf2-desoul-flag","M68K BUS")
event.on_bus_exec(function()
    stage="battle";local actor=entry(config.case.actor)
    memory.write_u8(actor+10,config.case.actorClass,"M68K BUS");memory.write_u8(actor+11,config.case.actorLevel,"M68K BUS")
    memory.write_u16_be(actor+12,100,"M68K BUS");memory.write_u16_be(actor+14,100,"M68K BUS")
    memory.write_u8(actor+16,config.case.initialMp,"M68K BUS");memory.write_u8(actor+17,config.case.initialMp,"M68K BUS")
    memory.write_u8(actor+23,99,"M68K BUS");memory.write_u8(actor+31,0,"M68K BUS")
    for o=32,38,2 do memory.write_u16_be(actor+o,0x007F,"M68K BUS") end
    memory.write_u16_be(actor+44,0,"M68K BUS");memory.write_u8(actor+48,config.case.actorInitialExp,"M68K BUS")
    memory.write_u8(actor+49,0x80,"M68K BUS");memory.write_u16_be(actor+52,4,"M68K BUS")
    for i,target in ipairs(config.case.targets) do
        local t=entry(target.combatant)
        memory.write_u8(t+11,target.level,"M68K BUS");memory.write_u16_be(t+12,target.maxHp,"M68K BUS");memory.write_u16_be(t+14,target.initialHp,"M68K BUS")
        memory.write_u16_be(t+26,target.resistanceWord,"M68K BUS");memory.write_u16_be(t+28,target.resistanceWord,"M68K BUS");memory.write_u16_be(t+44,0,"M68K BUS")
        memory.write_u8(t+46,8+i-1,"M68K BUS");memory.write_u8(t+47,17,"M68K BUS");memory.write_u8(t+49,0x60,"M68K BUS");memory.write_u8(t+55,target.enemyIndex,"M68K BUS")
        memory.write_u8(config.harness.ram.terrainDataAddress+17*48+8+i-1,3,"M68K BUS")
    end
    memory.write_u32_be(config.ram.currentGoldAddress,0,"M68K BUS")
end,config.harness["function"].turnOrderEntryAddress,"sf2-desoul-turn","M68K BUS")
event.on_bus_exec(function()
    if stage~="battle" or action_started or (emu.getregister("M68K D0")&0xFF)~=config.case.actor then return end
    action_started=true
    memory.write_u16_be(config.ram.currentBattleActionAddress,config.case.actionType,"M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress+2,config.case.actionSpell,"M68K BUS")
    memory.write_u16_be(config.ram.currentBattleActionAddress+4,config.case.targets[1].combatant,"M68K BUS")
end,config["function"].writeBattleSceneScriptAddress,"sf2-desoul-action","M68K BUS")
event.on_bus_exec(function()
    if not action_started or targets_supplied then return end;targets_supplied=true
    memory.write_u16_be(config.ram.targetsListLengthAddress,#config.case.targets,"M68K BUS")
    for i,target in ipairs(config.case.targets) do memory.write_u8(config.ram.targetsListAddress+i-1,target.combatant,"M68K BUS") end
end,config["function"].initializePropertiesAddress,"sf2-desoul-targets","M68K BUS")

event.on_bus_exec(function()
    if not action_started then return end
    local a5=emu.getregister("M68K A5")&0xFFFFFF;local combatant=memory.read_u8(a5,"M68K BUS");local target=target_case(combatant)
    memory.write_u16_be(config.ram.seedAddress,config.case.seed,"M68K BUS")
    active={combatant=combatant,setting=target.setting};records[#records+1]=active
end,config["function"].desoulEffectEntryAddress,"sf2-desoul-effect","M68K BUS")
event.on_bus_exec(function() if active then active.threshold=word("M68K D2") end end,config["function"].effectivenessEntryAddress,"sf2-desoul-threshold","M68K BUS")
event.on_bus_exec(function() if active then active.roll=word("M68K D0") end end,config["function"].effectivenessRollAddress,"sf2-desoul-roll","M68K BUS")
event.on_bus_exec(function()
    if active then
        local a2=emu.getregister("M68K A2")&0xFFFFFF
        active.success=false;active.reactionEmitted=false
        active.accumulatedExp=memory.read_u16_be(config.ram.battleSceneExpAddress,"M68K BUS")
        active.accumulatedGold=memory.read_u16_be(config.ram.battleSceneGoldAddress,"M68K BUS")
        active.targetDiesFlag=memory.read_u8(a2-4,"M68K BUS");active.targetHp=memory.read_u16_be(entry(active.combatant)+14,"M68K BUS")
    end
end,config["function"].effectivenessFailureAddress,"sf2-desoul-failure","M68K BUS")
event.on_bus_exec(function() if active then active.success=true end end,config["function"].effectivenessSuccessAddress,"sf2-desoul-success","M68K BUS")
event.on_bus_exec(function()
    if active then
        active.accumulatedExp=memory.read_u16_be(config.ram.battleSceneExpAddress,"M68K BUS")
        active.accumulatedGold=memory.read_u16_be(config.ram.battleSceneGoldAddress,"M68K BUS")
    end
end,config["function"].killExpGoldAppliedAddress,"sf2-desoul-reward","M68K BUS")
event.on_bus_exec(function()
    if active then
        local a2=emu.getregister("M68K A2")&0xFFFFFF
        active.reactionEmitted=true;active.targetDiesFlag=memory.read_u8(a2-4,"M68K BUS")
        active.targetHp=memory.read_u16_be(entry(active.combatant)+14,"M68K BUS")
    end
end,config["function"].targetDiesAppliedAddress,"sf2-desoul-dies","M68K BUS")

event.on_bus_exec(function() if not action_started then return end;award={seed=memory.read_u16_be(config.ram.seedAddress,"M68K BUS"),halved=word("M68K D1")} end,config["function"].expHalvedAddress,"sf2-desoul-half","M68K BUS")
event.on_bus_exec(function() if award then award.firstRoll=word("M68K D0") end end,config["function"].expFirstRollAddress,"sf2-desoul-first","M68K BUS")
event.on_bus_exec(function() if award then award.secondRoll=word("M68K D0") end end,config["function"].expSecondRollAddress,"sf2-desoul-second","M68K BUS")
event.on_bus_exec(function() if award then award.commandExp=word("M68K D1");award.commandGold=memory.read_u16_be(config.ram.battleSceneGoldAddress,"M68K BUS") end end,config["function"].expFinalAddress,"sf2-desoul-final","M68K BUS")
event.on_bus_exec(function() if #records==#config.case.targets then construction_actor_mp=memory.read_u8(entry(config.case.actor)+17,"M68K BUS");playback=true end end,config["function"].battleSceneEndReturnAddress,"sf2-desoul-end","M68K BUS")

event.on_bus_exec(function()
    if not playback then return end;local a6=emu.getregister("M68K A6")&0xFFFFFF
    ally_reaction={combatant=config.case.actor,mpChange=signed(memory.read_u16_be(a6+2,"M68K BUS")),mpBefore=memory.read_u8(entry(config.case.actor)+17,"M68K BUS")}
    reaction_order[#reaction_order+1]="ally:"..tostring(ally_reaction.mpChange)
end,config["function"].allyReactionEntryAddress,"sf2-desoul-ally","M68K BUS")
event.on_bus_exec(function() if ally_reaction then ally_reaction.mpAfter=memory.read_u8(entry(config.case.actor)+17,"M68K BUS") end end,config["function"].allyReactionAppliedAddress,"sf2-desoul-ally-applied","M68K BUS")
event.on_bus_exec(function()
    if not playback then return end;local a6=emu.getregister("M68K A6")&0xFFFFFF;local combatant=memory.read_u16_be(config.ram.battleSceneEnemyAddress,"M68K BUS")
    active_enemy={combatant=combatant,hpChange=signed(memory.read_u16_be(a6,"M68K BUS")),hpBefore=memory.read_u16_be(entry(combatant)+14,"M68K BUS")}
    reaction_order[#reaction_order+1]="enemy:"..tostring(active_enemy.hpChange)
end,config["function"].enemyReactionEntryAddress,"sf2-desoul-enemy","M68K BUS")
event.on_bus_exec(function() if active_enemy then active_enemy.hpAfter=memory.read_u16_be(entry(active_enemy.combatant)+14,"M68K BUS");enemy_reactions[#enemy_reactions+1]=active_enemy;active_enemy=nil end end,config["function"].enemyReactionAppliedAddress,"sf2-desoul-enemy-applied","M68K BUS")
event.on_bus_exec(function() if not playback then return end;local a6=emu.getregister("M68K A6")&0xFFFFFF;exp_reaction={commandExp=memory.read_u16_be(a6,"M68K BUS"),expBefore=memory.read_u8(entry(config.case.actor)+48,"M68K BUS")} end,config["function"].giveExpEntryAddress,"sf2-desoul-exp","M68K BUS")
event.on_bus_exec(function() if exp_reaction then exp_reaction.expAfter=memory.read_u8(entry(config.case.actor)+48,"M68K BUS") end end,config["function"].giveExpAppliedAddress,"sf2-desoul-exp-applied","M68K BUS")
event.on_bus_exec(function() if playback and ally_reaction and ally_reaction.mpAfter and #enemy_reactions==3 and exp_reaction and exp_reaction.expAfter then write_result_and_exit() end end,config["function"].executeScriptEndAddress,"sf2-desoul-script-end","M68K BUS")

local frames=0
while true do
    frames=frames+1;local button=nil
    if stage=="cheat" then local p=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS");if p>=0x28FF0 and p<0x29000 then button=names[cheat[p-0x28FF0+1]] elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Up" end
    elseif #queue>0 then button=table.remove(queue,1) elseif stage=="ui" and memory.read_u8(config.harness.ram.currentBattleAddress,"M68K BUS")==1 then button="C" elseif playback and frames%12<4 then button="C" end
    set_button(button);joypad.set({Start=((stage=="ui" and memory.read_u8(config.harness.ram.currentBattleAddress,"M68K BUS")==1) or playback)},2);emu.frameadvance()
    if frames%600==0 then status(string.format("frame=%d,stage=%s,pc=%X,records=%d,enemies=%d",frames,stage,emu.getregister("M68K PC"),#records,#enemy_reactions)) end
end
