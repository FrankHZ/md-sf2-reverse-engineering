using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.TestSupport;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class NewGameSetupTests
{
    [Fact]
    public void BattleAndHealingMessagesUseStableActorNamesWithoutChangingDefinitions()
    {
        var session = EngineTestContent.Start();
        var battle = session.Current.Battle;
        var id = new ActorRef("medic-a");
        var story = new StoryState(names: new Dictionary<string, string> { [id.Value] = "Nova" });
        var action = new Sf2HealingRule().Prepare(battle, id, battle.GetActor(id).Position!, new("mend", 1), id, new Sf2BattleProgressionRule());
        var cursor = HealingSceneCursor.Create(action, null, story);
        Assert.StartsWith("Nova cast", cursor.ActionText);
        Assert.StartsWith("Nova recovered", cursor.RecoveryText);
        Assert.Equal("Nova", CharacterNames.Battle(story, battle.GetActor(id), null));
        Assert.Equal("Medic A", CharacterNames.Battle(session.Current.Story, battle.GetActor(id), null));
        Assert.Equal("medic-a", battle.GetActor(id).Definition.Actor.Value);
    }

    [Theory]
    [InlineData("")] [InlineData("A")] [InlineData("Abc123!")] [InlineData(" ,.-!?#")] [InlineData("O'Neal ")]
    public void EditorAcceptsSourceAlphabetAndPreservesSpaces(string name) => Assert.Null(NewGameName.Error(name));

    [Theory]
    [InlineData("12345678", "name-too-long")] [InlineData("é", "name-character")]
    [InlineData("中", "name-character")] [InlineData("A\nB", "name-character")] [InlineData("{NAME}", "name-character")]
    public void UnsupportedNamesRejectBeforeContentRead(string name, string error)
    {
        Assert.Equal(error, NewGameName.Error(name));
        Assert.Equal(error, GameSession.PrepareNewGame(new UnreadableSource(), new(name), RuleCompositions.Sf2()).Failure!.Code);
    }

    [Fact]
    public void NamesAreDetachedSharedStateAndSurviveOrdinarySaveWithoutRenamingContent()
    {
        var (definition, data) = Fixture();
        var names = new Dictionary<string, string> { ["ally-0"] = "Nova 7" };
        data = data with { Names = names };
        var session = Restored(definition, data);
        names["ally-0"] = "Changed";
        Assert.Equal("Nova 7", CharacterNames.Member(definition, session.Current.Story, 0));
        Assert.Equal("Sarah", CharacterNames.Member(definition, session.Current.Story, 1));
        Assert.Equal("Bowie", definition.Exploration!.MemberNames[0]);
        Assert.Equal("Nova 7", session.Current.Story.Copy(null).Names["ally-0"]);
        var capture = session.CaptureSave(session.Current.SessionId, session.Current.Revision);
        Assert.Null(capture.Error);
        Assert.NotSame(session.Current.Story.Names, capture.Data!.Names);
        var record = OpeningSaveJson.Read(OpeningSaveJson.Write(new(1, 1, 1, DateTimeOffset.UtcNow, capture.Data)), 1);
        var continued = Restored(definition, record.Data);
        Assert.Equal("Nova 7", CharacterNames.Member(definition, continued.Current.Story, 0));
        Assert.Equal("Nova 7", CharacterNames.Member(definition, record.Data.Names, 0));
        Assert.Equal(session.Current.Exploration!.Party.MainSeed, continued.Current.Exploration!.Party.MainSeed);
        Assert.Equal(session.Current.Exploration.Party.Actors[0].Hp, continued.Current.Exploration.Party.Actors[0].Hp);
        Assert.Equal("ally-0", continued.QueryPartyDetails()!.Members[0].Carried.Actor.Value);
    }

    [Fact]
    public void ExistingVersionOneWithoutNamesLoadsDefaultButPresentInvalidNamesReject()
    {
        var (definition, data) = Fixture();
        byte[] bytes = OpeningSaveJson.Write(new(1, 2, 7, DateTimeOffset.UtcNow, data));
        var old = ChangePayload(bytes, root => root["data"]!.AsObject().Remove("names"));
        var record = OpeningSaveJson.Read(old, 2);
        Assert.Empty(record.Data.Names);
        Assert.Equal("Bowie", CharacterNames.Member(definition, Restored(definition, record.Data).Current.Story, 0));
        foreach (var name in new[] { "", "12345678", "中", "B\t" })
            Assert.Equal("save-name", Assert.IsType<SessionStartFailed>(GameSession.Restore(definition,
                data with { Names = new Dictionary<string, string> { ["ally-0"] = name } }, RuleCompositions.Sf2())).Failure.Code);
        Assert.Equal("save-name", Assert.IsType<SessionStartFailed>(GameSession.Restore(definition,
            data with { Names = new Dictionary<string, string> { ["missing"] = "Nova" } }, RuleCompositions.Sf2())).Failure.Code);
        foreach (var malformed in new JsonNode?[] { null, JsonValue.Create("Nova"), new JsonArray("Nova") })
        {
            byte[] changed = ChangePayload(bytes, root => root["data"]!["names"] = malformed);
            Assert.Throws<SaveFormatException>(() => OpeningSaveJson.Read(changed, 2));
        }
    }

    [Fact]
    public void InitialRecipeRunsRealOnloadWhileOrdinaryBusySaveStillRejects()
    {
        var (definition, data) = Fixture(initial: true);
        var first = Restored(definition, data);
        var second = Restored(definition, data);
        Assert.IsType<TickWait>(first.Current.Story.Wait);
        Assert.Equal("save-busy", first.QuerySave().Error);
        Assert.DoesNotContain(999, first.Current.Story.Flags);
        Assert.Equal("Nova", CharacterNames.Member(definition, first.Current.Story, 0));
        Assert.Equal(data.MainSeed, first.Current.Exploration!.Party.MainSeed);
        Assert.Equal(data.ThinkingSeed, first.Current.Exploration.Party.ThinkingSeed);
        Assert.NotEqual(first.Current.SessionId, second.Current.SessionId);
        for (int i = 0; i < 30 && !first.Current.CanWaitAtInput; i++)
            EngineTestContent.Accept(first, new AdvanceSimulation(first.Current.Story.Wait?.Token));
        Assert.Contains(999, first.Current.Story.Flags);
        Assert.DoesNotContain(1, first.Current.Story.Flags);
        Assert.Single(first.Current.Exploration.Party.ActiveAllies!);
        Assert.Equal("Nova", first.Current.Story.Names["ally-0"]);
        Assert.Equal(0u, first.Current.Exploration.Party.Gold);
        Assert.IsType<TickWait>(second.Current.Story.Wait);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)]
    public void InitialRecipeRejectsProgressOrUnsupportedOptions(int kind)
    {
        var (definition, data) = Fixture(initial: true);
        data = kind switch
        {
            0 => data with { Flags = [0, 32, 80, 603] },
            1 => data with { Gold = 1 },
            2 => data with { Difficulty = 1 },
            3 => data with { TextCursor = 1 },
            _ => data with { X = 55 },
        };
        Assert.IsType<SessionStartFailed>(GameSession.Restore(definition, data, RuleCompositions.Sf2()));
    }

    [PrivateInputFact("SF2_PRIVATE_EXPLORATION_CONTENT", "SF2_PRIVATE_STATIC_DATA", "SF2_PRIVATE_ENEMY_DATA", "SF2_PRIVATE_ENEMY_GOLD",
        "SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE", "SF2_PRIVATE_BATTLE01_TERRAIN")]
    public void NamedProductCandidateUsesPostGrowthSeedsAndFreshContinueReplaysOnlyOpeningSetup()
    {
        string Input(string key) => PrivateInputFactAttribute.RequireInput(key);
        var prepared = GameSession.PrepareNewGame(new OpeningNewGameReader(Input("SF2_PRIVATE_EXPLORATION_CONTENT"),
            Input("SF2_PRIVATE_BATTLE01_DATA"), Input("SF2_PRIVATE_BATTLE01_SCENE"), Input("SF2_PRIVATE_BATTLE01_TERRAIN"),
            Input("SF2_PRIVATE_STATIC_DATA"), Input("SF2_PRIVATE_ENEMY_DATA"), Input("SF2_PRIVATE_ENEMY_GOLD"), 0, uint.MaxValue),
            new("Nova7"), RuleCompositions.ForGame());
        Assert.Null(prepared.Failure);
        var initial = prepared.Save!;
        Assert.Equal("new-game", initial.Recipe);
        Assert.Equal(new[] { 0, 32, 80 }, initial.Flags);
        Assert.Equal("Nova7", initial.Names["ally-0"]);
        var candidate = prepared.Started!.Session;
        var continued = Restored(candidate.Definition, initial);
        Assert.Equal(candidate.Current.Story.Cursor, continued.Current.Story.Cursor);
        Assert.Equal(candidate.Current.Story.Flags, continued.Current.Story.Flags);
        Assert.Equal(candidate.Current.Exploration!.Party.MainSeed, continued.Current.Exploration!.Party.MainSeed);
        Assert.Equal(candidate.Current.Exploration.Party.ThinkingSeed, continued.Current.Exploration.Party.ThinkingSeed);
        foreach (var actor in candidate.Current.Exploration.Party.Actors)
        {
            var restored = continued.Current.Exploration.Party.Actors.Single(row => row.Actor == actor.Actor);
            Assert.Equal(actor.Hp, restored.Hp); Assert.Equal(actor.Mp, restored.Mp);
            Assert.Equal(actor.Progress! with { Spells = restored.Progress!.Spells, SourceLoadout = restored.SourceLoadout }, restored.Progress);
            Assert.Equal(actor.Progress.Spells, restored.Progress.Spells);
            Assert.Equal(actor.SourceLoadout!.Items, restored.SourceLoadout!.Items);
            Assert.Equal(actor.SourceLoadout.Spells, restored.SourceLoadout.Spells);
        }
        Assert.DoesNotContain(603, continued.Current.Story.Flags);
        Assert.Equal("Nova7", CharacterNames.Member(candidate.Definition, continued.Current.Story, 0));
    }

    private static GameSession Restored(ScenarioDefinition definition, OpeningSaveData data) =>
        Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;

    private static (ScenarioDefinition, OpeningSaveData) Fixture(bool initial = false)
    {
        var (old, data) = OpeningRestoreTests.Fixture();
        var world = old.Exploration!;
        var definition = new ScenarioDefinition(old.Package, old.Encounters.Values, exploration:
            new(world.Maps.Values, [new("dangerous-onload", [new WaitProgramTicks(4), new WriteFlag(999, true), new EndProgram()])],
                provenance: world.Provenance, partyFlags: world.PartyFlags, visuals: world.Visuals, memberNames: ["Bowie", "Sarah"]))
            { Persistence = old.Persistence };
        if (initial) data = data with { Recipe = "new-game", Flags = [0, 32, 80], Gold = 0, OnceFlags = [], Deals = [],
            StepCounter = 0, EgressMap = "map-0", TextCursor = 0,
            Allies = data.Allies.Select(ally => ally with { Hp = ally.MaxHp, Mp = ally.MaxMp, Exp = 0, Kills = 0, Defeats = 0, Status = 0 }).ToArray(),
            Names = new Dictionary<string, string> { ["ally-0"] = "Nova" } };
        return (definition, data);
    }

    private static byte[] ChangePayload(byte[] bytes, Action<JsonObject> change)
    {
        var envelope = JsonNode.Parse(bytes)!.AsObject();
        var root = JsonNode.Parse(Convert.FromBase64String(envelope["payload"]!.GetValue<string>()))!.AsObject();
        change(root);
        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(root);
        envelope["payload"] = Convert.ToBase64String(payload);
        envelope["sha256"] = Convert.ToHexString(SHA256.HashData(payload));
        return JsonSerializer.SerializeToUtf8Bytes(envelope);
    }
    private sealed class UnreadableSource : IScenarioSource
    { public ScenarioReadResult Read() => throw new InvalidOperationException("Invalid name must reject before content read."); }
}
