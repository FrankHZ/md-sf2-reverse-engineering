using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

// The pending battle/death work is suspended while the ordinary field interpreter
// stages its actors. CompletedWorld retains that scene's projection through field
// death and the following after-program; combat resources remain in the battle.
internal sealed record DefeatedProgramState(EngineBattleState Battle, ActorRef Actor,
    MapPosition? Destination, BattleDeathBatch Deaths, ExplorationState? CompletedWorld = null);

internal static class BattleDefeatedProgram
{
    internal static SessionResult Begin(ScenarioDefinition definition, SessionResult result, SessionRules rules)
    {
        var current = result.Snapshot;
        var observations = result.Observations.ToList();
        try
        {
            var battle = current.Battle;
            var route = current.Story.EnteringBattle!.Outcome!;
            var binding = current.Story.FieldPlayer!;
            var party = new BattleStartInput(battle.Definition.Encounter,
                battle.Actors.Where(a => a.IsAlly).Select(a => new BattleActorStartInput(a.Actor, a.Hp, a.Mp,
                    a.Exp, a.Kills, a.Defeats, a.Status, null, a.Progress, a.SourceLoadout, a.ActivationWord, a.Initialized))
                    .Concat(binding.RetainedAllies ?? []), battle.MainSeed, battle.ThinkingSeed, battle.Gold,
                battle.StartPolicy, itemCustody: battle.ItemCustody);
            var map = definition.Exploration!.Maps[route.BattleMap];
            var origin = route.BattleOrigin ?? new MapPosition(0, 0);
            MapPosition MapPoint(MapPosition p) => new(p.X + origin.X, p.Y + origin.Y);
            var leader = battle.GetActor(battle.Definition.Outcome!.Leader);
            var leaderPosition = MapPoint(leader.Position ?? leader.Deployment.Position);
            var shell = MapTransfer.Build(map, binding.Player, leaderPosition, 3, binding.Speed, party,
                current.Story.Flags, combatPlacement: true);
            var field = definition.ScenesFor(battle.Definition.Encounter)?.FieldDeath;
            var entities = new List<ExplorationEntity>();
            foreach (var actor in battle.Actors.Where(a => a.Position is not null))
            {
                int sourceSlot = actor.IsAlly ? actor.Deployment.Initialization?.AllyPartyMember ?? actor.ProcessingOrder : actor.ProcessingOrder;
                int sprite;
                if (actor.IsAlly) sprite = SceneEntities.ResolveAllySprite(shell.Population!, current.Story.Flags, sourceSlot, sourceSlot);
                else if (field is null || !field.EnemySprites.TryGetValue(actor.Actor, out sprite))
                    throw new BattleRuleException("defeated-field-sprite", "battleScenes.fieldDeath", true);
                entities.Add(Entity(sourceSlot, MapPoint(actor.Position!), sprite, entities.Count));
            }
            // An unjoined living ally can be staged by the source position table.
            // Resolve membership identities from content, never from party-list indices.
            foreach (var row in route.DeathPositions ?? new Dictionary<int, MapPosition>())
            {
                if (row.Key >= 30 || entities.Any(e => e.Entity == Reference(row.Key))) continue;
                var deployment = definition.Encounters.Values.SelectMany(e => e.Deployments)
                    .FirstOrDefault(d => d.Faction == BattleFaction.Ally && d.Initialization?.AllyPartyMember == row.Key);
                if (deployment is null || !party.Actors.Any(a => a.Actor == deployment.Actor && a.Hp > 0)) continue;
                int sprite = SceneEntities.ResolveAllySprite(shell.Population!, current.Story.Flags, row.Key, row.Key);
                entities.Add(Entity(row.Key, MapPoint(row.Value), sprite, entities.Count));
            }
            var player = entities.Single(e => e.Entity == Reference(leader.Deployment.Initialization?.AllyPartyMember ?? leader.ProcessingOrder));
            var aliases = Enumerable.Range(0, 64).ToDictionary(i => Reference(i < 32 ? i : i + 96), _ => player.Slot);
            foreach (var entity in entities) aliases[entity.Entity] = entity.Slot;
            aliases[binding.Player] = player.Slot;
            var world = new ExplorationState(map, shell.Layout, binding.Player, entities, party, aliases: aliases, population: shell.Population);
            var story = current.Story.Copy(route.DefeatedProgram, callers: [], continuation: ProgramContinuation.DefeatedProgramFinished,
                outcomeReturn: new(route.BattleMap, leaderPosition, 3), clearCameraEntity: true, clearCameraTarget: true,
                textWindow: new ClosedTextWindow());
            story = ExplorationTextRunner.Initialize(world, story);
            current = ProgramRunner.Commit(current, new ActiveExploration(world), story, observations, "enemy-defeated-program-started");
            return ProgramRunner.Run(definition, current, observations, rules);
        }
        catch (BattleRuleException error) { return ProgramRunner.Failure(current, observations, error); }
    }

    internal static SessionResult Resume(ScenarioDefinition definition, SessionSnapshot current,
        List<SessionObservation> observations, SessionRules rules)
    {
        var pending = current.Story.DefeatedScene!;
        var world = current.Exploration!;
        var battle = pending.Battle.With(mainSeed: world.Party.MainSeed, thinkingSeed: world.Party.ThinkingSeed);
        current = ProgramRunner.Commit(current, new ActiveBattle(battle, null),
            current.Story.Copy(null, continuation: ProgramContinuation.FieldInput,
                defeatedScene: pending with { Battle = battle, CompletedWorld = world }), observations, "enemy-defeated-program-finished");
        var committed = BattleActionCommitter.Publish(current, battle, pending.Actor, pending.Destination, [], observations,
            rules, defeatedHookHandled: true, sceneEnded: true, deaths: pending.Deaths);
        var result = BattleAdvancer.Advance(committed, observations, rules);
        return result.Failure is null ? BattleOutcome.Begin(definition, result, rules) : result;
    }

    internal static ExplorationState AfterDeath(ExplorationState world, EngineBattleState battle, BattleStartInput party)
    {
        var removed = battle.Actors.Where(a => !a.IsAlly && a.Position is null).Select(a => Reference(a.ProcessingOrder)).ToHashSet();
        return world.WithParty(party).WithEntities(world.AllEntities.Select(e => removed.Contains(e.Entity)
            ? e with { Visible = false } : e));
    }

    private static EntityRef Reference(int slot) => new("entity-" + slot.ToString(System.Globalization.CultureInfo.InvariantCulture));
    private static ExplorationEntity Entity(int sourceSlot, MapPosition point, int sprite, int physicalSlot) =>
        new(Reference(sourceSlot), EntityMotionState.At(point, 3, 32) with { FlagsA = 0xE0 }, true, Slot: physicalSlot, Sprite: sprite);
}
