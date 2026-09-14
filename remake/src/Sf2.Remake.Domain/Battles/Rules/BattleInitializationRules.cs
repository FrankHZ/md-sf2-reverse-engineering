namespace Sf2.Remake.Domain.Battles;

internal static class BattleInitializationRules
{
    internal static void Validate(BattleDefinition definition, BattleStartInput start)
    {
        var policy = start.NewBattle;
        Require((definition.Initialization is null) == (policy is null), "initialization-policy", "start.newBattle");
        if (policy is null) return;
        Require(!string.IsNullOrWhiteSpace(policy.Declaration) && !string.IsNullOrWhiteSpace(policy.EvidenceOwner) &&
            !string.IsNullOrWhiteSpace(policy.BridgeBoundary), "controlled-provenance", "start.newBattle");
        Require((policy.SkipIntro || policy.BeforeBattleRouted) && policy.AlreadyRefreshedAllies && policy.RosterOnly,
            "new-battle-policy", "start.newBattle", true);
        Require(policy.Difficulty == 0, "difficulty", "start.newBattle.difficulty", true);
        Require(!policy.AllyAutoBattle && !policy.OpponentControl,
            "control-mode", "start.newBattle.control", true);
        BattleActivationRules.Validate(definition);
        foreach (var deployment in definition.Deployments)
        {
            var source = deployment.Initialization;
            Require(source is not null, "missing-deployment-initialization", "placements.initialization");
            Require((deployment.Faction == BattleFaction.Enemy) == (source!.EnemyBaseAiWord is not null),
                "enemy-initialization", "placements.initialization");
            Require(deployment.Definition.SourceLoadout is { } loadout && loadout.Items.Count == 4 && loadout.Spells.Count == 4 &&
                loadout.Items.All(word => word <= 255), "source-loadout", "actors.loadout");
            Require(Enum.IsDefined(deployment.Definition.Mover), "movement-profile", "actors.mover", true);
            if (deployment.Faction == BattleFaction.Enemy)
            {
                _ = EnemyAttack(deployment.Definition.Attack, policy.Difficulty);
                // General equipped/status refresh is not admitted by a loaded baseline.
                Require(deployment.Definition.SourceLoadout!.Items.All(word => word == 127),
                    "enemy-equipment-refresh", "actors.loadout", true);
            }
        }
        foreach (var input in start.Actors)
        {
            Require((input.Status & ~7) == 0, "new-battle-status-refresh", "start.actors.status", true);
            Require(input.PositionOverride is null, "initialization-placement-override", "start.actors.positionOverride", true);
        }
    }

    internal static IEnumerable<BattleDeploymentDefinition> Deployments(BattleDefinition definition, BattleStartInput start)
    {
        var formation = definition.Initialization?.AllyFormation;
        var active = start.ActiveAllies;
        if (active is null)
        {
            if (start.NewBattle is null || formation is null) return definition.Deployments;
            active = definition.Deployments.Where(row => row.Faction == BattleFaction.Ally).Select(row => row.Actor).ToArray();
        }
        Require(formation is not null && active.Count <= formation.Count, "ally-formation", "start.activeAllies", true);
        var inputs = start.Actors.ToDictionary(actor => actor.Actor);
        var positions = active.Where(actor => inputs[actor].Hp > 0 ||
                definition.Deployments.Single(row => row.Actor == actor).Initialization?.AllyPartyMember is 7 or 28)
            .Select((actor, index) => (actor, position: formation![index])).ToDictionary(row => row.actor, row => row.position);
        return definition.Deployments.Select(deployment => positions.TryGetValue(deployment.Actor, out var position)
            ? deployment with { Position = position } : deployment);
    }

    internal static EngineBattleState Initialize(EngineBattleState before, BattleStartInput start)
    {
        var policy = before.StartPolicy!;
        var actors = before.Actors.Select(actor =>
        {
            bool heal = !actor.IsAlly || actor.Hp > 0 || actor.Deployment.Initialization?.AllyPartyMember is 7 or 28;
            bool placed = !actor.IsAlly || (heal && (start.ActiveAllies is null || start.ActiveAllies.Contains(actor.Actor)));
            return new BattleActorState(actor.Deployment,
                heal ? actor.MaxHp : actor.Hp, heal ? actor.MaxMp : actor.Mp, actor.Exp, placed ? actor.Deployment.Position : null,
                actor.Kills, actor.Defeats, attack: actor.IsAlly ? actor.Attack : EnemyAttack(actor.Definition.Attack, policy.Difficulty),
                status: actor.IsAlly && heal ? (ushort)(actor.Status & 7) : actor.Status,
                activationWord: actor.IsAlly ? null : InitialActivationWord(actor.Deployment.Initialization!), progress: actor.Progress);
        });
        return before.With(actors: actors, regions: new(new bool[16], 0));
    }

    // Pinned new-battle difficulty0 branch: source ATT plus its truncated quarter, once.
    internal static byte EnemyAttack(byte sourceAttack, byte difficulty)
    {
        Require(difficulty == 0, "difficulty", "start.newBattle.difficulty", true);
        Require(sourceAttack <= 204, "enemy-attack-domain", "actors.attack", true);
        return (byte)(sourceAttack + sourceAttack / 4);
    }
    internal static ushort InitialActivationWord(BattleDeploymentInitialization source) =>
        (ushort)((source.EnemyBaseAiWord!.Value & 0xF000) | ((source.SpawnMode & 15) << 8) | source.Filler);
    private static void Require(bool condition, string code, string field, bool unsupported = false)
    { if (!condition) throw new BattleRuleException(code, field, unsupported); }
}
