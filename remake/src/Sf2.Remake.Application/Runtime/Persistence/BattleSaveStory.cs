using System.Collections.ObjectModel;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using static Sf2.Remake.Domain.Battles.BattleContinuationRestore;

namespace Sf2.Remake.Application.Runtime.Persistence;

internal static class BattleSaveStory
{
    internal static bool Same(SavedBattleStory left, SavedBattleStory right) =>
        left with { Flags = right.Flags, Names = right.Names, Music = right.Music, Player = right.Player } == right &&
        left.Flags.SequenceEqual(right.Flags) && left.Names.OrderBy(p => p.Key).SequenceEqual(right.Names.OrderBy(p => p.Key)) &&
        left.Music with { Previous = right.Music.Previous } == right.Music && left.Music.Previous.SequenceEqual(right.Music.Previous) &&
        left.Player with { RetainedAllies = right.Player.RetainedAllies } == right.Player &&
        left.Player.RetainedAllies.Count == right.Player.RetainedAllies.Count &&
        left.Player.RetainedAllies.Zip(right.Player.RetainedAllies).All(p => BattleContinuationValues.SameCarried(p.First, p.Second));

    internal static SavedBattleRoute Locate(ScenarioDefinition definition, ExplorationBattleRoute route)
    {
        var matches = definition.Exploration!.Maps.Values.SelectMany(map => map.Battles.Select((value, index) =>
            (map.Map, value, index))).Where(row => ReferenceEquals(row.value, route)).ToArray();
        Require(matches.Length == 1, "route-identity");
        return new(matches[0].Map.Value, matches[0].index);
    }

    internal static ExplorationBattleRoute Route(ScenarioDefinition definition, BattleSaveData data)
    {
        Require(definition.Exploration!.Maps.TryGetValue(new(data.Route.Map), out var map) &&
            data.Route.Ordinal >= 0 && data.Route.Ordinal < map.Battles.Count, "route");
        var route = map!.Battles[data.Route.Ordinal];
        Require(route.Encounter == data.Battle.Encounter && route.Outcome is not null, "route-encounter");
        var outcome = route.Outcome!;
        Require(definition.Exploration.Maps.ContainsKey(outcome.BattleMap) &&
            definition.Exploration.Maps.ContainsKey(outcome.EgressMap), "return-map");
        foreach (var location in new[] { outcome.AfterProgram, outcome.DefeatedProgram, outcome.DefeatProgram, outcome.ReturnProgram })
            Require(definition.Programs.TryGetValue(location.Program, out var program) &&
                location.Instruction >= 0 && location.Instruction < program.Instructions.Count, "return-program");
        return route;
    }

    internal static SavedBattleStory Capture(StoryState story)
    {
        Require(story.StepCounter is not null && story.EgressMap is not null && story.TextSettings is not null, "story-unknown");
        Require(story.Music is { HistoryBound: true, Battle: true, EndStep: null, PreviousEligible: false, ActualDone: false, DeliverySkipped: false }, "music-unknown");
        var music = story.Music!;
        var player = story.FieldPlayer!;
        return new(Array.AsReadOnly(story.Flags.ToArray()), new ReadOnlyDictionary<string, string>(story.Names.ToDictionary()),
            story.StepCounter!.Value, story.EgressMap!.Value, story.TextCursor, story.TextSettings! with { },
            story.Display! with { }, story.LogicalText! with { }, story.LogicalView! with { }, story.Speaker,
            story.EntityServices, story.WindowFixPending, story.SaveAnchor is null ? null : story.SaveAnchor with { },
            new(music.Map.Value, music.Area, music.Cue, Array.AsReadOnly(music.Previous.ToArray())),
            new(player.Player.Value, player.Speed, Array.AsReadOnly((player.RetainedAllies ?? []).Select(BattleContinuationValues.Carried).ToArray())));
    }

    internal static StoryState Build(ScenarioDefinition definition, BattleSaveData data, EngineBattleState battle)
    {
        var value = data.Story;
        var route = Route(definition, data);
        Require(value.Flags.Count <= 65536 && value.Flags.All(f => f is >= 0 and <= 65535) &&
            value.Flags.Distinct().Count() == value.Flags.Count && !value.Flags.Contains(88), "flags");
        Require(value.TextCursor is >= 0 and <= 65535 && value.TextSettings is { MessageSpeed: <= 3, ViewSpeed: 0 } &&
            value.Display is { Period: > 0, Visibility: FullFadeVisibility.BaseRestored, Base.Valid: true } &&
            value.Display.Current == value.Display.Base && value.LogicalText is { Open: false, Moving: false } && !value.LogicalView.Scrolling, "display");
        Require(!string.IsNullOrWhiteSpace(value.Player.Entity) && value.Player.Speed is > 0 and <= 384 && value.Player.RetainedAllies.Count <= 30, "player");
        var music = value.Music;
        var world = definition.Exploration!;
        Require(music.Map == route.Outcome!.BattleMap.Value && world.Visuals is not null &&
            world.Visuals.Maps.TryGetValue(new(music.Map), out var map) && music.Area >= 0 && music.Area < map.Music.Count &&
            music.Previous.Count <= 9 && music.Previous.Prepend(music.Cue).All(c => world.Visuals.Audio.TryGetValue(c, out var audio) && audio.Command < 65) &&
            world.Visuals.Audio[music.Cue].ModernEndStep is null, "music-resource");
        if (route.Outcome.DefeatedScene && !(route.CompletedFlag is { } completed && value.Flags.Contains(completed)))
        {
            Require(value.Player.Entity == "entity-0" && world.Maps[route.Outcome.BattleMap].SelectSetup(value.Flags).Population is not null,
                "return-population");
            if (battle.Visuals is { } visuals)
                foreach (var actor in battle.Actors) _ = visuals.Resolve(actor.Actor);
            Require(definition.ScenesFor(battle.Definition.Encounter)?.FieldDeath is { } field &&
                battle.Actors.Where(a => !a.IsAlly).All(a => field.EnemySprites.TryGetValue(a.Actor, out int sprite) &&
                    world.Visuals!.Sprites.ContainsKey(sprite)), "return-sprite-resource");
        }
        if (value.SaveAnchor is { } anchor)
            Require(anchor.Facing <= 3 && world.Maps.ContainsKey(anchor.Map), "prior-anchor");
        ExplorationViewRunner.Validate(value.LogicalView.Area);
        var egress = Sf2OutcomeReturnPolicy.FindEgress(definition, new(value.EgressMap), route.Outcome);
        if (egress is null && definition.DefeatFallback is { } fallback && fallback.Register.Value == value.EgressMap)
            egress = Sf2OutcomeReturnPolicy.FindEgress(definition, fallback.Destination, route.Outcome);
        Require(egress is not null, "egress");
        RequireReturnPlayer(world, route.Outcome.BattleMap, value.Player.Entity);
        RequireReturnPlayer(world, egress!.Map, value.Player.Entity);
        if (route.Outcome.CompletedDefeatMap is { } handoff) RequireReturnPlayer(world, handoff, value.Player.Entity);
        var roster = definition.Encounters.Values.SelectMany(e => e.Deployments).Where(d => d.Faction == BattleFaction.Ally)
            .GroupBy(d => d.Actor).ToDictionary(g => g.Key, g => g.First());
        Require(value.Names.Count <= 30 && value.Names.All(p => roster.ContainsKey(new(p.Key)) &&
            p.Value is { Length: > 0 } && NewGameName.Error(p.Value) is null), "names");
        var retained = value.Player.RetainedAllies;
        Require(retained.Select(a => a.Actor).Distinct().Count() == retained.Count && retained.All(a => roster.ContainsKey(a.Actor) &&
            !battle.Actors.Any(live => live.Actor == a.Actor)) &&
            roster.Keys.ToHashSet().SetEquals(battle.Actors.Where(a => a.IsAlly).Select(a => a.Actor).Concat(retained.Select(a => a.Actor))), "retained-roster");
        foreach (var ally in retained)
        {
            var deployment = roster[ally.Actor];
            Require(ally.Initialized is not null && ally.ActivationWord is not null && ally.Exp is not null &&
                ally.Kills is not null && ally.Defeats is not null && ally.PositionOverride is null && !ally.RetainDeadPosition, "retained-unknown");
            ValidateValues(new(ally.Actor.Value, null, deployment.Position, 3, ally.Hp, ally.Mp, ally.Exp, ally.Kills, ally.Defeats,
                ally.Status, ally.Initialized!.Value, BattleEquipment.Attack(deployment.Definition, ally.Progress, ally.SourceLoadout),
                ally.ActivationWord, 0, null, null, null, ally.Progress, ally.SourceLoadout), deployment.Definition);
        }
        if (definition.PrivateDefinitions is { } source)
            Require(battle.Actors.SelectMany(a => a.SourceLoadout?.Items ?? []).Concat(retained.SelectMany(a => a.SourceLoadout?.Items ?? []))
                .All(w => source.Items.ContainsKey((byte)(w & 127))) && battle.ItemCustody!.Deals.Keys.All(source.Items.ContainsKey), "item-identity");
        var layout = world.PartyFlags ?? throw new BattleRuleException("save-battle-membership-unknown", "save.story");
        return new(value.Flags, textCursor: value.TextCursor, enteringBattle: route,
            partyLists: MapPartyMembership.Rebuild(value.Flags, layout), speaker: value.Speaker,
            portraitWindow: new ClosedPortraitWindow(), display: value.Display with { }, textSettings: value.TextSettings with { },
            logicalText: value.LogicalText with { }, logicalView: value.LogicalView with { }, entityServices: value.EntityServices,
            windowFixPending: value.WindowFixPending,
            music: new(1, music.Cue, Array.AsReadOnly(music.Previous.ToArray()), new(music.Map), music.Area, null, Battle: true),
            stepCounter: value.StepCounter, egressMap: new(value.EgressMap), fieldPlayer: new(new(value.Player.Entity), value.Player.Speed,
                Array.AsReadOnly(retained.Select(BattleContinuationValues.Carried).ToArray())), saveAnchor: value.SaveAnchor, names: value.Names);
    }

    private static void RequireReturnPlayer(ExplorationDefinition world, MapId destination, string player)
    {
        Require(world.Maps.TryGetValue(destination, out var map), "return-map");
        if (player == "entity-0") return;
        // Outcome construction first uses BattleMap; the return then uses live egress or
        // the defeat handoff. Programs/entry can change flags before that rebuild, so check
        // the destination's declared setup alternatives, without executing those programs.
        // Plain authored maps construct the supplied player directly; unrelated maps do not
        // constrain this binding. Population allocation owns the entity-0 requirement.
        Require(map!.Population is null && (map.Setup is null || map.Setup.FlagAlternatives
            .Where(v => v.Setup != map.Setup.DefaultSetup).All(v =>
                map.Setups.TryGetValue(v.Setup, out var setup) && setup.Population is null)), "return-player");
    }

    internal static void RequireMembership(ScenarioDefinition definition, StoryState story, EngineBattleState battle)
    {
        var layout = definition.Exploration!.PartyFlags;
        Require(layout is not null && story.PartyLists is not null, "membership-unknown");
        var lists = MapPartyMembership.Rebuild(story.Flags, layout!);
        Require(lists.Joined.SequenceEqual(story.PartyLists!.Joined) && lists.Active.SequenceEqual(story.PartyLists.Active) &&
            lists.Reserve.SequenceEqual(story.PartyLists.Reserve), "membership");
        var members = definition.Encounters.Values.SelectMany(e => e.Deployments).Where(d => d.Faction == BattleFaction.Ally &&
            d.Initialization?.AllyPartyMember is not null).GroupBy(d => d.Initialization!.AllyPartyMember!.Value).ToDictionary(g => g.Key, g => g.First().Actor);
        Require(lists.Joined.All(members.ContainsKey) && lists.Active.All(m => members.ContainsKey(m) &&
            battle.Actors.Any(a => a.Actor == members[m] && a.IsAlly)), "active-membership");
        Require(lists.Active.Count > 0 && lists.Active.Any(m => members[m] == BattleTurnFlow.QueuedActor(battle).Actor), "current-membership");
    }
}
