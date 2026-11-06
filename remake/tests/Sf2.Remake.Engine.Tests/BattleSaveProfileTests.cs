using System.Diagnostics;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.TestSupport;
using Xunit;
using Xunit.Abstractions;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleSaveProfileTests(ITestOutputHelper output)
{
    [PrivateInputFact("SF2_PRIVATE_EXPLORATION_CONTENT")]
    public void AdmittedConnectedEncountersPreserveActualActorDataAndNextLegalTurn()
    {
        string Input(string key) => PrivateInputFactAttribute.RequireInput(key);
        var read = new OpeningNewGameReader(Input("SF2_PRIVATE_EXPLORATION_CONTENT"), Input("SF2_PRIVATE_BATTLE01_DATA"),
            Input("SF2_PRIVATE_BATTLE01_SCENE"), Input("SF2_PRIVATE_BATTLE01_TERRAIN"), Input("SF2_PRIVATE_STATIC_DATA"),
            Input("SF2_PRIVATE_ENEMY_DATA"), Input("SF2_PRIVATE_ENEMY_GOLD"), 0x1234ABCD, 0x80ABCDEF).Read();
        Assert.True(read is ExplorationReadAccepted, (read as ScenarioReadRejected)?.Failure.ToString());
        var admitted = (ExplorationReadAccepted)read;
        var definition = admitted.Definition; var world = definition.Exploration!;
        var carried = admitted.Start.Party;
        var rules = RuleCompositions.Sf2();
        var routes = world.Maps.Values.SelectMany(map => map.Battles).ToArray();
        Assert.Equal(7, routes.Select(r => r.Encounter).Distinct().Count());
        foreach (var route in routes)
        {
            var encounter = definition.Encounters[route.Encounter];
            var defaults = definition.EncounterStarts[route.Encounter];
            var actors = encounter.Deployments.Select(d => d.Faction == BattleFaction.Ally
                ? carried.Actors.Single(a => a.Actor == d.Actor) : defaults.Actors.Single(a => a.Actor == d.Actor)).ToArray();
            var active = encounter.Deployments.Where(d => d.Faction == BattleFaction.Ally).ToArray();
            var flags = active.SelectMany(d => new[] { d.Initialization!.AllyPartyMember!.Value, 32 + d.Initialization.AllyPartyMember.Value })
                .Concat(new[] { 399, route.UnlockedFlag!.Value, route.IntroFlag!.Value }).Distinct().ToArray();
            var start = new BattleStartInput(encounter.Encounter, actors, carried.MainSeed, carried.ThinkingSeed, carried.Gold,
                carried.NewBattle! with { BeforeBattleRouted = true }, active.Select(d => d.Actor), carried.ItemCustody, flags);
            var result = BattleAdvancer.Start(encounter, start, rules);
            Assert.Null(result.Failure);
            var current = result.Snapshot;
            for (int guard = 0; current.StopReason != SessionStopReason.PlayerInput; guard++)
            {
                Assert.True(guard < 2000);
                result = current.BattleMovement is { } movement ? BattleMovementContinuation.Submit(current, new CompletePresentation(movement.Token, movement.CompletionKind), rules) :
                    current.BattleScene is { } scene ? BattleSceneContinuation.Submit(current, scene.RequiresAcknowledgement ? new Acknowledge(scene.Token) : new CompletePresentation(scene.Token, scene.CompletionKind), rules) :
                    BattleAdvancer.Advance(current, [], rules);
                Assert.Null(result.Failure); current = result.Snapshot;
            }
            var map = world.Maps[route.Outcome!.BattleMap];
            var origin = route.Outcome.BattleOrigin ?? new(0, 0);
            var position = current.Battle.GetActor(current.Selection!.Actor).Position!;
            var field = MapTransfer.Build(map, admitted.Start.Player, new(position.X + origin.X, position.Y + origin.Y), 3, 32, carried, flags);
            var view = ExplorationViewRunner.Initialize(field);
            int area = ExplorationViewContext.Require(field, view).Area;
            int cueId = world.Visuals!.Maps[map.Map].Music[area].Battle;
            string cue = world.Visuals.Audio.Single(p => p.Value.Command == cueId).Key;
            var story = new StoryState(flags, enteringBattle: route, partyLists: MapPartyMembership.Rebuild(flags, world.PartyFlags!),
                portraitWindow: new ClosedPortraitWindow(), logicalText: new(), logicalView: view, textSettings: new(2, 0, 0),
                display: new(3, map.BasePalette!, map.BasePalette!, FullFadeVisibility.BaseRestored),
                stepCounter: 13, egressMap: route.Outcome.EgressMap, music: new(1, cue, [], map.Map, area, null, Battle: true),
                fieldPlayer: new(admitted.Start.Player, 32, carried.Actors.Where(a => !encounter.Deployments.Any(d => d.Actor == a.Actor)).ToArray()));
            current = current.WithStory(story);
            var watch = Stopwatch.StartNew();
            var saved = BattleSaveFixture.Capture(definition, current);
            var restored = GameSession.RestoreBattle(definition, saved, rules);
            watch.Stop();
            Assert.True(restored is SessionStarted, (restored as SessionStartFailed)?.Failure.ToString());
            var resumed = ((SessionStarted)restored).Session.Current;
            BattleSaveFixture.EqualBattle(current.Battle, resumed.Battle);
            var control = BattleSaveFixture.Stay(current); var next = BattleSaveFixture.Stay(resumed);
            BattleSaveFixture.EqualBattle(control.Battle, next.Battle);
            output.WriteLine($"{encounter.Encounter}: actors={current.Battle.Actors.Count}, retained={story.FieldPlayer!.RetainedAllies!.Count}, capture+restore-ms={watch.Elapsed.TotalMilliseconds:F2}, next actor={next.Selection?.Actor.Value}");
        }
    }
}
