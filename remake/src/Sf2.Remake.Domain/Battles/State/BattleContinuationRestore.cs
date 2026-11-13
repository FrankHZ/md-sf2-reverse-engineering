using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

internal static class BattleContinuationRestore
{
    internal static EngineBattleState Build(BattleDefinition definition, BattleContinuationData data)
    {
        Require(data.Encounter == definition.Encounter && data.Actors.Count == definition.Deployments.Count &&
            data.Actors.Select(a => a.Actor).Distinct().Count() == data.Actors.Count &&
            definition.Deployments.All(d => data.Actors.Any(a => a.Actor == d.Actor.Value)), "actors");
        Require(data.Queue.Count is > 0 and <= 64 && data.Cursor >= 0 && data.Cursor < data.Queue.Count && data.Round > 0 &&
            data.Queue.All(t => t.Actor is null || data.Actors.Any(a => a.Actor == t.Actor)) &&
            data.Queue.Take(data.Cursor + 1).All(t => t.Actor is not null), "queue");
        Require(data.Gold <= 9999999 && data.Options.Difficulty == 0 && !data.Options.AllyAutoBattle && !data.Options.OpponentControl, "options");
        Require((definition.Initialization is null) == (data.Regions is null) &&
            (data.Regions is null || data.Regions.Flags.Count == 16), "regions");
        Require(data.DeathCredit?.Ally is not { } credit || definition.Deployments.Any(d => d.Actor == credit && d.Faction == BattleFaction.Ally), "death-credit");
        if (definition.Poison) Require(data.DeathCredit is not null, "death-credit-unknown");
        var actors = definition.Deployments.Select(deployment => Actor(deployment,
            data.Actors.Single(a => a.Actor == deployment.Actor.Value), definition)).ToArray();
        var occupied = actors.Where(a => a.Hp > 0 && a.Position is not null).Select(a => a.Position).ToArray();
        Require(occupied.Distinct().Count() == occupied.Length, "occupied");
        Require(actors.All(a => a.LastTarget is null || actors.Any(other => other.Actor == a.LastTarget)), "last-target");
        var policy = new NewBattleStartPolicy("battle-continuation", "", "", data.Options.Difficulty,
            false, true, true, data.Options.AllyAutoBattle, data.Options.OpponentControl, data.Options.MissingCandidateAllyWord)
            { Origin = BattleStartOrigin.NewGame, BeforeBattleRouted = true };
        var visuals = Visuals(data.Visuals, actors, definition);
        return new(definition, actors, data.MainSeed, data.ThinkingSeed, data.Round,
            data.Queue.Select(t => new TurnOrderEntry<ActorRef>(t.Actor is null ? null : new ActorRef(t.Actor), t.AlteredAgility)),
            data.Cursor, data.Gold, policy, data.Regions is null ? null : new(data.Regions.Flags, data.Regions.Tested),
            itemCustody: new(data.Custody.OnceFlags, data.Custody.Deals), deathCredit: data.DeathCredit is null ? null : data.DeathCredit with { }, visuals: visuals);
    }

    private static BattleActorState Actor(BattleDeploymentDefinition deployment, SavedBattleActor saved, BattleDefinition battle)
    {
        ValidateValues(saved, deployment.Definition);
        Require(InBoard(saved.DeploymentPosition, battle) && (deployment.Faction == BattleFaction.Enemy
            ? saved.DeploymentPosition == deployment.Position
            : saved.DeploymentPosition == deployment.Position || battle.Initialization?.AllyFormation?.Contains(saved.DeploymentPosition) == true), "deployment");
        Require(saved.Position is null || InBoard(saved.Position, battle), "position");
        Require(saved.Initialized || deployment.Faction == BattleFaction.Enemy && deployment.Initialization?.SpawnMode is 2 or 3 &&
            saved.Hp == 0 && saved.Position is null, "initialization");
        byte ordinaryAttack = BattleEquipment.Attack(deployment.Definition, saved.Progress, saved.Loadout);
        if (deployment.Faction == BattleFaction.Enemy && saved.Initialized)
        {
            byte initializedAttack = BattleInitializationRules.EnemyAttack(deployment.Definition.Attack, 0);
            Require(saved.Attack == (deployment.Definition.Equipment is null ? initializedAttack :
                Math.Min(200, initializedAttack + BattleEquipment.Contribution(deployment.Definition, saved.Loadout))), "attack");
        }
        else Require(saved.Attack == ordinaryAttack, "attack");
        if (deployment.Initialization is not null || deployment.SourceAllyControl)
            Require(saved.ActivationWord is not null, "activation-unknown");
        if (deployment.Faction == BattleFaction.Ally)
            Require(saved.Exp is not null && saved.Kills is not null && saved.Defeats is not null, "accounting-unknown");
        if (deployment.AiStrategy?.Value is "source-orders" or "source-ally-physical" ||
            deployment.Initialization?.PrimaryOrder is not null)
            Require(saved.PrimaryOrder is not null && saved.SecondaryOrder is not null, "orders-unknown");
        if (deployment.AiStrategy?.Value == "source-orders")
        {
            Require(saved.AiMemory == 0 || (saved.AiMemory & 15) is 3 or 4 && saved.AiMemory >> 4 < 4, "ai-memory");
            foreach (byte? order in new[] { saved.PrimaryOrder, saved.SecondaryOrder })
                Require(order == 255 || order is { } value && battle.Initialization is { } init &&
                    ((value & 64) != 0 ? init.ReadMoveOrderPoint(value & 15) is not null : init.SourceCombatants.ContainsKey(value)), "ai-order");
        }
        // Constructor fallback is harmless only if absence was also absent in the immutable definition.
        Require(saved.PrimaryOrder is not null || deployment.Initialization?.PrimaryOrder is null, "orders");
        Require(saved.SecondaryOrder is not null || deployment.Initialization?.SecondaryOrder is null, "orders");
        return new(deployment with { Position = saved.DeploymentPosition }, saved.Hp, saved.Mp, saved.Exp, saved.Position,
            saved.Kills, saved.Defeats, saved.LastTarget is null ? null : new ActorRef(saved.LastTarget), saved.Attack,
            saved.Status, saved.ActivationWord, saved.AiMemory, BattleContinuationValues.Progress(saved.Progress),
            BattleContinuationValues.Loadout(saved.Loadout), retainDeadPosition: true,
            primaryOrder: saved.PrimaryOrder, secondaryOrder: saved.SecondaryOrder, facing: saved.Facing, initialized: saved.Initialized);
    }

    internal static void ValidateValues(SavedBattleActor saved, BattleActorDefinition definition)
    {
        var progress = saved.Progress;
        Require(saved.Facing <= 3 && saved.Hp <= (progress?.MaxHp ?? definition.MaxHp) &&
            saved.Mp <= (progress?.MaxMp ?? definition.MaxMp) && saved.Exp is null or <= 200 &&
            saved.Kills is null or <= 9999 && saved.Defeats is null or <= 9999 && SilenceRules.Supported(saved.Status), "actor-range");
        if (progress is not null)
            Require(definition.Growth is not null && progress.Level is >= 1 and <= 99 && progress.MaxHp is > 0 and <= 200 &&
                progress.MaxMp <= 200 && progress.BaseAttack <= 200 && progress.Defense <= 200 && progress.Agility <= 100, "progress");
        Require(saved.Loadout is not null || definition.SourceLoadout is null && progress?.SourceLoadout is null, "loadout-unknown");
        if (saved.Loadout is { } loadout)
        {
            Require(loadout.Items.Count == 4 && loadout.Spells.Count == 4 && loadout.Items.All(w => w <= 511), "loadout");
            Require(progress?.SourceLoadout is null || progress.SourceLoadout.Items.SequenceEqual(loadout.Items) &&
                progress.SourceLoadout.Spells.SequenceEqual(loadout.Spells), "progress-loadout");
            if (definition.Equipment is not null) BattleEquipment.ValidateLoadout(definition, loadout);
            if (progress is not null)
            {
                var spells = new List<SpellRef>();
                foreach (byte packed in loadout.Spells.Where(p => p != 63))
                {
                    Require(definition.Growth!.Spells.Any(s => s.PackedSpell == packed && s.Level <= progress.Level), "learned-spell");
                    for (int rank = 0; rank <= packed >> 6; rank++)
                    {
                        Require(definition.Growth.SpellDefinitions.ContainsKey((byte)((packed & 63) | rank << 6)), "spell-resource");
                        spells.Add(definition.Growth.SpellDefinitions[(byte)((packed & 63) | rank << 6)]);
                    }
                }
                Require(spells.SequenceEqual(progress.Spells), "progress-spells");
            }
            Require(loadout.Spells.Where(p => p != 63).Select(p => p & 63).Distinct().Count() == loadout.Spells.Count(p => p != 63), "spell-duplicates");
        }
        // Difficulty can raise a fixed enemy ATT above 200; byte is the actual live range.
    }

    private static BattleVisualState? Visuals(SavedBattleVisuals? saved, IReadOnlyList<BattleActorState> actors, BattleDefinition definition)
    {
        if (saved is null) return null;
        Require(saved.Records.Count <= 49 && saved.Records.Select(v => v.Slot).Distinct().Count() == saved.Records.Count &&
            saved.Records.All(v => v.Slot is >= 0 and < 49 && v.Facing <= 3 && InBoard(v.Position, definition) && actors.Any(a => a.Actor == v.Appearance)) &&
            saved.Aliases.All(p => actors.Any(a => a.Actor.Value == p.Key) && saved.Records.Any(v => v.Slot == p.Value)), "visuals");
        return new(saved.Aliases.ToDictionary(p => new ActorRef(p.Key), p => p.Value), saved.Records.Select(v => v with { }));
    }

    private static bool InBoard(MapPosition position, BattleDefinition definition) =>
        position.X >= 0 && position.Y >= 0 && definition.Contains(position);
    internal static void Require(bool valid, string code)
    { if (!valid) throw new BattleRuleException("save-battle-" + code, "save.battle"); }
}
