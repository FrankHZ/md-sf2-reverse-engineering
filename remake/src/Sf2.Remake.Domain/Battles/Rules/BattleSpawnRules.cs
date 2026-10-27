namespace Sf2.Remake.Domain.Battles;

internal sealed record BattleSpawnBatch(EngineBattleState Battle, IReadOnlyList<ActorRef> Appearances,
    IReadOnlyList<BattleEffect> Effects);

internal static class BattleSpawnRules
{
    internal static void Validate(BattleDefinition definition, BattleStartInput start)
    {
        var init = definition.Initialization!;
        if (!definition.Deployments.Any(d => d.Initialization?.SpawnMode is 2 or 3)) return;
        Require(init.EnemyCount is >= 1 and <= 32, "spawn-enemy-bound");
        Require(init.SourceCombatants.Count == definition.Deployments.Count &&
            init.SourceCombatants.Values.Distinct().Count() == definition.Deployments.Count &&
            init.SourceCombatants.All(row => definition.Deployments.Any(d => d.Actor == row.Value &&
                (d.Faction == BattleFaction.Ally ? row.Key is >= 0 and < 30 : row.Key is >= 128 and < 160))), "spawn-source-slots");
        foreach (var d in definition.Deployments.Where(d => d.Faction == BattleFaction.Enemy))
        {
            if (d.Initialization!.SpawnMode == 0) continue;
            Require(d.Definition.Growth is null && d.Definition.SourceLoadout is { } loadout &&
                loadout.Items.Count == 4 && loadout.Items.All(w => w is 0 or 127) &&
                loadout.Spells.Count == 4 && loadout.Spells.All(w => w == 63), "spawn-reset-loadout");
            _ = BattleInitializationRules.EnemyAttack(d.Definition.Attack, start.NewBattle?.Difficulty ?? 0);

        }
    }

    internal static BattleSpawnBatch Populate(EngineBattleState before)
    {
        var init = before.Definition.Initialization!;
        var battle = before;
        List<ActorRef> appearances = []; List<BattleEffect> effects = [];
        if (!before.Definition.Deployments.Any(d => d.Initialization?.SpawnMode is 2 or 3))
            return new(before, appearances.AsReadOnly(), effects.AsReadOnly());
        foreach (var row in init.SourceCombatants.Where(r => r.Key >= 128).OrderBy(r => r.Key))
        {
            var enemy = battle.GetActor(row.Value);
            var source = enemy.Deployment.Initialization!;
            int mode = (enemy.ActivationWord!.Value >> 8) & 15;
            if (mode == 0 || enemy.Hp != 0) continue;
            var flags = battle.Regions!.Flags;
            if (!(source.PrimaryRegion != 15 && flags[source.PrimaryRegion]) &&
                !(source.SecondaryRegion != 15 && flags[source.SecondaryRegion])) continue;
            ushort word = BattleActivationRules.ActivateAssignedRegions(enemy.ActivationWord.Value,
                source.PrimaryRegion, source.SecondaryRegion, flags);
            enemy = enemy.With(activationWord: word);
            // UpdateEnemyActivationIfDead precedes HIDDEN's maximum-HP check.
            if (mode == 2 && enemy.MaxHp != 0) { Replace(enemy); continue; }
            bool occupied = battle.Actors.Any(a => a.Position == enemy.Deployment.Position);
            bool admitted = row.Key < 128 + init.EnemyCount && !occupied;
            if (admitted)
            {
                enemy = new(enemy.Deployment, enemy.Definition.MaxHp, enemy.Definition.MaxMp, 0,
                    enemy.Deployment.Position, enemy.Kills, enemy.Defeats, lastTarget: enemy.LastTarget, aiMemory: enemy.AiMemory,
                    attack: BattleInitializationRules.EnemyAttack(enemy.Definition.Attack, before.StartPolicy?.Difficulty ?? 0),
                    activationWord: word, sourceLoadout: enemy.Definition.SourceLoadout);
                appearances.Add(enemy.Actor);
            }
            else enemy = enemy.With(hp: 0, initialized: false, clearPosition: true);
            Replace(enemy);
            effects.Add(new(admitted ? "spawn-reset" : "spawn-refused", enemy.Actor, After: row.Key));
        }
        return new(battle, appearances.AsReadOnly(), effects.AsReadOnly());

        void Replace(BattleActorState actor) => battle = battle.With(actors: battle.Actors.Select(a => a.Actor == actor.Actor ? actor : a));
    }

    private static void Require(bool valid, string code)
    { if (!valid) throw new BattleRuleException(code, "encounter.initialization", true); }
}
