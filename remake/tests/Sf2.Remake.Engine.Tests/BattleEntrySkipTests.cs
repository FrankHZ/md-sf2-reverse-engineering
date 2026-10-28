using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleEntrySkipTests
{
    [Theory]
    [InlineData(0, false, false)]
    [InlineData(2, true, false)]
    [InlineData(3, true, true)]
    public void SameRequestStatePreservesLiveWorkAndStopsBeforeCombat(int speed, bool midText, bool enemyFirst)
    {
        var normal = Entry(speed, enemyFirst);
        var skipped = Entry(speed, enemyFirst);
        foreach (var session in new[] { normal, skipped })
        {
            if (midText)
            {
                for (int i = 0; i < 400 && session.Current.Story.Wait is not FieldTextWait; i++) NormalStep(session);
                Assert.IsType<FieldTextWait>(session.Current.Story.Wait);
                Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
            }
            else
            {
                Assert.IsType<EntityWait>(session.Current.Story.Wait);
                Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
            }
        }
        Assert.Equal(Logical(normal.Current), Logical(skipped.Current));
        var input = skipped.Current;
        Accept(skipped, new SkipBattleEntry());
        Assert.Equal(Logical(input), Logical(skipped.Current));
        List<SessionObservation> fast = [];
        for (int i = 0; i < 1500 && BattleEntrySkip.Eligible(normal.Current); i++) NormalStep(normal);
        for (int i = 0; i < 300 && skipped.Current.Story.EntrySkip is not null; i++) fast.AddRange(SkipStep(skipped).Observations);
        Assert.False(BattleEntrySkip.Eligible(normal.Current));
        Assert.Null(skipped.Current.Story.EntrySkip);
        Assert.Equal(Logical(normal.Current), Logical(skipped.Current));
        Assert.Contains(fast, row => row.Kind == "text-delivery-skipped");
        Assert.Contains(fast, row => row.Kind == "nod-delivery-skipped");
        Assert.Contains(fast, row => row.Kind == "battle-entry-skip-stopped" && row.Detail == "entry-finished");
        Assert.Contains(615 + speed, skipped.Current.Story.Flags);
        Assert.Contains(20, skipped.Current.Story.Flags);
        Assert.Equal(1, skipped.Current.Battle.Round);
        if (enemyFirst) Assert.NotNull(skipped.Current.BattleMovement);
        else Assert.NotNull(skipped.Current.Selection);
        var done = skipped.Current;
        Assert.NotNull(Send(skipped, new AdvanceBattleEntrySkip()).Failure);
        Assert.Same(done, skipped.Current);
    }

    [Sf2.Remake.TestSupport.PrivateInputFact("SF2_ENTRY_SKIP_START")]
    public void ConnectedBattle01NormalAndSkipKeepTheSameInitializedPartySeedsAndControl()
    {
        string Required(string name) => Sf2.Remake.TestSupport.PrivateInputFactAttribute.RequireInput(name);
        var source = new Sf2.Remake.Content.Scenarios.PrivateExplorationReader(Required("SF2_PRIVATE_EXPLORATION_CONTENT"),
            Required("SF2_ENTRY_SKIP_START"), PrivateBattleScenarioTests.Selected(
                Path.Combine(AppContext.BaseDirectory, "controlled", "battle01-player-ready.json")));
        var admitted = Assert.IsType<ExplorationReadAccepted>(source.Read());
        GameSession Open() => Assert.IsType<SessionStarted>(GameSession.Start(admitted.Definition, admitted.Start)).Session;
        var normal = Open(); var skipped = Open();
        using var fixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures", "player-ready.json")));
        foreach (var session in new[] { normal, skipped })
            foreach (var step in fixture.RootElement.GetProperty("static").GetProperty("inputPlan").EnumerateArray())
            {
                for (int i = 0; i < 4000 && !session.Current.CanWaitAtInput && !BattleEntrySkip.Eligible(session.Current); i++) NormalStep(session);
                if (BattleEntrySkip.Eligible(session.Current)) break;
                var direction = step.GetProperty("input").GetString() switch
                {
                    "Up" => ExplorationDirection.North, "Right" => ExplorationDirection.East,
                    "Down" => ExplorationDirection.South, _ => ExplorationDirection.West,
                };
                Accept(session, new Move(direction));
            }
        for (int i = 0; i < 4000 && !BattleEntrySkip.Eligible(normal.Current); i++) NormalStep(normal);
        for (int i = 0; i < 4000 && !BattleEntrySkip.Eligible(skipped.Current); i++) NormalStep(skipped);
        Assert.Equal(Logical(normal.Current), Logical(skipped.Current));
        Accept(skipped, new SkipBattleEntry());
        for (int i = 0; i < 10000 && BattleEntrySkip.Eligible(normal.Current); i++) NormalStep(normal);
        for (int i = 0; i < 1000 && skipped.Current.Story.EntrySkip is not null; i++) SkipStep(skipped);
        Assert.True(normal.Current.HasBattleControl);
        Assert.True(skipped.Current.HasBattleControl);
        Assert.All(skipped.Current.Battle.Actors, actor => Assert.True(actor.Initialized));
        Assert.Equal(Logical(normal.Current), Logical(skipped.Current));
    }

    [Theory]
    [InlineData(1, 7)]
    [InlineData(3, 31)]
    public void FiniteMusicAndFullFadeRetainMandatoryServiceAndTruthfulDelivery(int period, int endStep)
    {
        GameSession Open() => Entry(2, edit: doc =>
        {
            doc["start"]!["display"] = JsonNode.Parse($$$"""{"period":{{{period}}},"base":{"color2":546,"color3":1092},"current":{"color2":546,"color3":1092},"visibility":"base-restored"}""");
            byte[] pcm = [0, 0, 0, 0];
            object Audio(string cue, int command, int? end) => new { cue, command, timerB = 192, sampleRate = 8000,
                channels = 1, sampleFrames = 2, pcm16 = Convert.ToBase64String(pcm),
                sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pcm)),
                loopBegin = (int?)null, loopEnd = (int?)null, modernEndStep = end };
            var town = JsonSerializer.SerializeToNode(Audio("town", 8, null))!;
            town.AsObject().Remove("modernEndStep");
            doc["world"]!["presentation"]!["audio"] = new JsonArray(town, JsonSerializer.SerializeToNode(Audio("join", 19, endStep)));
            var instructions = Before(doc);
            instructions.Clear();
            foreach (var instruction in JsonNode.Parse("""
                [{"op":"wait-ticks","ticks":1},
                 {"op":"present","kind":"Sound","resource":"town","entity":null,"position":null},
                 {"op":"present","kind":"Sound","resource":"join","entity":null,"position":null},
                 {"op":"present","kind":"SoundWait","resource":null,"entity":null,"position":null},
                 {"op":"present","kind":"PreviousMusic","resource":null,"entity":null,"position":null},
                 {"op":"present","kind":"FadeOut","resource":"black","entity":null,"position":null},
                 {"op":"present","kind":"FadeIn","resource":"black","entity":null,"position":null},
                 {"op":"set-flag","flag":955,"value":true},{"op":"wait-ticks","ticks":1},{"op":"end"}]
                """)!.AsArray()) instructions.Add(instruction!.DeepClone());
        });
        var normal = Open(); var skipped = Open();
        Accept(skipped, new SkipBattleEntry());
        foreach (var session in new[] { normal, skipped })
        {
            bool fast = session == skipped;
            for (int i = 0; i < 400 && BattleEntrySkip.Eligible(session.Current); i++)
            {
                if (session.Current.Story.Music is { EndStep: not null, ActualDone: false, DeliverySkipped: false } music)
                {
                    var receipt = Accept(session, fast ? new CompleteSkippedEntryMusic(music.Generation, music.Cue) : new CompleteMusic(music.Generation, music.Cue));
                    Assert.Equal(!fast, receipt.Snapshot.Story.Music!.ActualDone);
                    Assert.Equal(fast, receipt.Snapshot.Story.Music.DeliverySkipped);
                    var current = session.Current;
                    Assert.NotNull(Send(session, new CompleteMusic(music.Generation, music.Cue)).Failure);
                    Assert.Same(current, session.Current);
                }
                if (fast) SkipStep(session); else NormalStep(session);
            }
        }
        Assert.Equal(Logical(normal.Current), Logical(skipped.Current));
        Assert.Contains(955, skipped.Current.Story.Flags);
        Assert.Equal("town", skipped.Current.Story.Music!.Cue);
        Assert.Empty(skipped.Current.Story.Music.Previous);
        Assert.Equal(normal.Current.Story.Music!.Step, skipped.Current.Story.Music.Step);
        Assert.Null(skipped.Current.Story.EntrySkip);
        Assert.True(skipped.Current.HasBattleControl);
    }

    [Fact]
    public void RequestsAndLateCompletionsAreScopedAndChoicesRequireAFreshRequest()
    {
        var outside = Start("harbor-arrival");
        var idle = outside.Current;
        Assert.NotNull(Send(outside, new SkipBattleEntry()).Failure);
        Assert.Same(idle, outside.Current);
        var session = Entry(1, edit: doc => Before(doc).Insert(2, JsonNode.Parse("""{"op":"yes-no","flag":902}""")));
        var requested = session.Current;
        Accept(session, new SkipBattleEntry());
        var active = session.Current;
        Assert.NotNull(session.Submit(new(active.SessionId, requested.Revision, null, new SkipBattleEntry())).Failure);
        Assert.NotNull(Send(session, new SkipBattleEntry()).Failure);
        Assert.NotNull(Send(session, new CompleteSkippedEntryPresentation(new(-1), PresentationCueKind.Gesture)).Failure);
        Assert.Same(active, session.Current);
        for (int i = 0; i < 30 && session.Current.Story.Wait is not ChoiceWait; i++) SkipStep(session);
        Assert.Null(session.Current.Story.EntrySkip);
        var choice = Assert.IsType<ChoiceWait>(session.Current.Story.Wait);
        foreach (var phase in Enum.GetValues<ChoicePhase>())
        {
            var current = session.Current.WithStory(session.Current.Story.Copy(session.Current.Story.Cursor,
                choice with { Work = new(Phase: phase) }));
            Assert.False(BattleEntrySkip.Eligible(current));
        }
        Assert.NotNull(Send(session, new SkipBattleEntry()).Failure);
        if (choice.Work is null) Accept(session, new ChooseDialogue(choice.Token, true));
        else
        {
            Accept(session, new ChoiceHeldInput(choice.Token, false));
            for (int i = 0; i < 100 && !session.Current.CanWaitForChoice; i++)
            {
                if (session.Current.Story.Wait is ChoiceWait { Work.Phase: ChoicePhase.Release })
                    Accept(session, new ChoiceHeldInput(choice.Token, false));
                else Accept(session, new AdvanceSimulation(choice.Token));
            }
            Accept(session, new PollChoice(choice.Token, ChoiceInput.ConfirmC));
            for (int i = 0; i < 100 && session.Current.Story.Wait is ChoiceWait; i++) Accept(session, new AdvanceSimulation(choice.Token));
        }
        Assert.Null(session.Current.Story.EntrySkip);
        Accept(session, new SkipBattleEntry());
        Accept(session, new CancelBattleEntrySkip());
        var cancelled = session.Current;
        Assert.Null(cancelled.Story.EntrySkip);
        Assert.NotNull(Send(session, new CompleteSkippedEntryPresentation(choice.Token, PresentationCueKind.Gesture)).Failure);
        Assert.Same(cancelled, session.Current);
        Accept(session, new SkipBattleEntry());
    }

    [Fact]
    public void UnsupportedRetainsCommittedPrefixAndDisarms()
    {
        var session = Entry(0, edit: doc => Before(doc).Insert(2, JsonNode.Parse("""{"op":"native-call","symbol":"unimplemented","source":"authored"}""")));
        Accept(session, new SkipBattleEntry());
        SessionResult? failure = null;
        for (int i = 0; i < 30 && failure?.Failure is null; i++) failure = Send(session, new AdvanceBattleEntrySkip());
        Assert.NotNull(failure!.Failure);
        Assert.Null(session.Current.Story.EntrySkip);
        Assert.Equal(2, session.Current.Story.Cursor!.Value.Instruction);
        Assert.Equal(SessionStopReason.Unsupported, session.Current.StopReason);
        Assert.Contains(15, session.Current.Story.Flags);
        Assert.Null(session.Current.Story.Wait);
    }

    [Fact]
    public void PresentationOnlyLoopConsumesTheSameFiniteRequestBudget()
    {
        var session = Entry(0, edit: doc =>
        {
            Before(doc).Clear();
            Before(doc).Add(JsonNode.Parse("""{"op":"present","kind":"FadeIn","resource":"black","entity":null,"position":null}"""));
            Before(doc).Add(JsonNode.Parse("""{"op":"jump","target":{"program":"before-battle","instruction":0}}"""));
        });
        Accept(session, new SkipBattleEntry());
        for (int i = 0; i < BattleEntrySkip.RequestOperations; i++)
        {
            var wait = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
            Accept(session, new CompleteSkippedEntryPresentation(wait.Token, wait.Cue.Kind));
        }
        Assert.Null(session.Current.Story.EntrySkip);
        Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        Assert.True(BattleEntrySkip.Eligible(session.Current));
        Assert.Equal(SessionStopReason.PresentationWait, session.Current.StopReason);
    }

    [Fact]
    public void RetainedBattleLoadLoopAlsoExhaustsTheSameRequestBudget()
    {
        var session = Entry(0, edit: doc =>
        {
            Before(doc).Clear(); Before(doc).Add(JsonNode.Parse("""{"op":"end"}"""));
            doc["world"]!["maps"]![0]!["battle"]!["load"] = JsonNode.Parse("""{"program":"mount-loop","instruction":0}""");
            doc["world"]!["programs"]!.AsArray().Add(JsonNode.Parse("""
                {"id":"mount-loop","instructions":[
                {"op":"present","kind":"BattleLoad","resource":null,"entity":null,"position":null},
                {"op":"jump","target":{"program":"mount-loop","instruction":0}}]}
                """));
        });
        Accept(session, new SkipBattleEntry());
        for (int i = 0; i < BattleEntrySkip.RequestOperations; i++)
        {
            var wait = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
            if (i == 0)
            {
                var current = session.Current;
                Assert.NotNull(Send(session, new CompletePresentation(new(-1), wait.Cue.Kind)).Failure);
                Assert.Same(current, session.Current);
            }
            Accept(session, new CompletePresentation(wait.Token, wait.Cue.Kind));
        }
        Assert.Null(session.Current.Story.EntrySkip);
        Assert.Equal(PresentationCueKind.BattleLoad, Assert.IsType<PresentationWait>(session.Current.Story.Wait).Cue.Kind);
        Assert.Equal(ProgramContinuation.BattleLoadFinished, session.Current.Story.Continuation);
        Assert.Equal(0, session.Current.Battle.Round);
    }

    [Fact]
    public void BatchYieldIsNotCompletionAndMountsCannotBeSuppressed()
    {
        var session = Entry(0, edit: doc =>
        {
            Before(doc).Clear();
            Before(doc).Add(JsonNode.Parse("""{"op":"wait-ticks","ticks":500}"""));
            Before(doc).Add(JsonNode.Parse("""{"op":"end"}"""));
            var route = doc["world"]!["maps"]![0]!["battle"]!;
            route["load"] = JsonNode.Parse("""{"program":"load-skip-test","instruction":0}""");
            doc["world"]!["programs"]!.AsArray().Add(JsonNode.Parse("""{"id":"load-skip-test","instructions":[{"op":"present","kind":"BattleLoad","resource":null,"entity":null,"position":null},{"op":"end"}]}"""));
        });
        Accept(session, new SkipBattleEntry());
        long tick = session.Current.Story.SimulationTick;
        Accept(session, new AdvanceBattleEntrySkip());
        Assert.Equal(BattleEntrySkip.BatchOperations, session.Current.Story.SimulationTick - tick);
        Assert.Equal(500 - BattleEntrySkip.BatchOperations, Assert.IsType<TickWait>(session.Current.Story.Wait).Remaining);
        for (int i = 0; i < 30 && session.Current.Story.Wait is not PresentationWait; i++) Accept(session, new AdvanceBattleEntrySkip());
        var mount = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        var before = session.Current;
        Assert.Equal(PresentationCueKind.BattleLoad, mount.Cue.Kind);
        Assert.NotNull(Send(session, new CompleteSkippedEntryPresentation(mount.Token, mount.Cue.Kind)).Failure);
        Assert.Same(before, session.Current);
        Accept(session, new CompletePresentation(mount.Token, mount.Cue.Kind));
        Assert.Null(session.Current.Story.EntrySkip);
        Assert.True(session.Current.HasBattleControl);
    }

    private static GameSession Entry(int speed, bool enemyFirst = false, Action<JsonNode>? edit = null) =>
        ExplorationTextWaitTests.StartFieldText("AB{W1}C{W2}", speed: speed, enabled: true, npcRandom: true, configure: doc =>
        {
            doc["start"]!.AsObject().Remove("program");
            doc["start"]!["flags"] = new JsonArray(13);
            doc["battle"]!["start"]!["mainSeed"] = 0x12341234u + (uint)speed;
            doc["battle"]!["start"]!["gold"] = 87 + speed;
            doc["battle"]!["start"]!["actors"]![0]!["hp"] = 71;
            doc["battle"]!["start"]!["actors"]![0]!["exp"] = 19;
            if (enemyFirst)
            {
                doc["battle"]!["encounters"]![0]!["rewards"] = Document("stone-court")["encounters"]![0]!["rewards"]!.DeepClone();
                doc["battle"]!["actors"]![2]!["move"] = 1;
                doc["battle"]!["actors"]![2]!["physical"] = Document("stone-court")["actors"]![2]!["physical"]!.DeepClone();
                doc["battle"]!["encounters"]![0]!["placements"]![2]!["aiStrategy"] = "attack-then-approach";
                foreach (var actor in doc["battle"]!["actors"]!.AsArray())
                    actor!["agility"] = actor["id"]!.GetValue<string>() == "dummy-a" ? 120 : 1;
            }
            var maps = doc["world"]!["maps"]!.AsArray();
            maps[0]!["onLoad"] = null;
            maps[0]!["battle"] = maps[1]!["battle"]!.DeepClone();
            maps[1]!["battle"] = null;
            var before = doc["world"]!["programs"]!.AsArray().Single(row => row!["id"]!.GetValue<string>() == "before-battle")!;
            before["entitiesRunning"] = true;
            before["instructions"] = JsonNode.Parse($$$"""
                [{"op":"set-flag","flag":15,"value":true},
                 {"op":"motion","entity":"ferryman","actions":[{"op":"move","x":1,"y":0}],"wait":true},
                 {"op":"call","target":{"program":"nested-{{{speed}}}","instruction":0}},
                 {"op":"set-flag","flag":{{{615 + speed}}},"value":true},{"op":"end"}]
                """);
            doc["world"]!["programs"]!.AsArray().Add(JsonNode.Parse($$$"""
                {"id":"nested-{{{speed}}}","entitiesRunning":true,"instructions":[
                 {"op":"text-cursor","text":100},{"op":"show-text","mode":"single","speaker":"ferryman","explicitWindows":true},
                 {"op":"close-text"},{"op":"present","kind":"Gesture","resource":"nod","entity":"ferryman","position":null},
                 {"op":"return"}]}
                """));
            edit?.Invoke(doc);
        });

    private static JsonArray Before(JsonNode doc) => doc["world"]!["programs"]!.AsArray()
        .Single(row => row!["id"]!.GetValue<string>() == "before-battle")!["instructions"]!.AsArray();

    private static SessionResult NormalStep(GameSession session)
    {
        var current = session.Current;
        var sprite = current.Exploration?.AllEntities.FirstOrDefault(entity => entity.WaitingForSprite && entity.SpriteReady != entity.SpriteRequest);
        if (sprite is not null) return Accept(session, new EntitySpriteReady(sprite.Slot, sprite.SpriteRequest));
        SessionCommand command = current.Story.Wait switch
        {
            PresentationWait wait => new CompletePresentation(wait.Token, wait.Cue.Kind),
            NodWait { ActualDone: false } wait => new CompletePresentation(wait.Token, PresentationCueKind.Gesture),
            FullFadeWait { ActualDone: false } wait => new CompletePresentation(wait.Token, wait.Kind),
            FieldTextWait { LogicalDone: true, Revealed: false } wait => new CompleteTextReveal(wait.Token),
            FieldTextWait { LogicalDone: true } wait when current.CanWaitForText => new Acknowledge(wait.Token),
            W1TextWait { Revealed: false } wait => new CompleteTextReveal(wait.Token),
            W1TextWait wait => new Acknowledge(wait.Token),
            DialogueWait wait => new Acknowledge(wait.Token),
            _ => new AdvanceSimulation(current.Story.Wait?.Token),
        };
        return Accept(session, command);
    }

    private static SessionResult SkipStep(GameSession session)
    {
        var sprite = session.Current.Exploration?.AllEntities.FirstOrDefault(entity => entity.WaitingForSprite && entity.SpriteReady != entity.SpriteRequest);
        if (sprite is not null) return Accept(session, new EntitySpriteReady(sprite.Slot, sprite.SpriteRequest));
        return Accept(session, session.Current.Story.Wait switch
        {
        PresentationWait { Cue.Kind: PresentationCueKind.BattleLoad } wait => new CompletePresentation(wait.Token, wait.Cue.Kind),
        PresentationWait wait => new CompleteSkippedEntryPresentation(wait.Token, wait.Cue.Kind),
        NodWait { ActualDone: false, DeliverySkipped: false } wait => new CompleteSkippedEntryPresentation(wait.Token, PresentationCueKind.Gesture),
        FullFadeWait { ActualDone: false, DeliverySkipped: false } wait => new CompleteSkippedEntryPresentation(wait.Token, wait.Kind),
            _ => new AdvanceBattleEntrySkip(),
        });
    }

    private static string Logical(SessionSnapshot current) => JsonSerializer.Serialize(new
    {
        current.Mode, current.StopReason, world = current.Exploration is { } world ? new { world.Map, world.Player,
            entities = world.AllEntities, party = world.Party.Actors, world.Party.MainSeed, world.Party.ThinkingSeed,
            world.Party.Gold, world.Party.ItemCustody } : null,
        battle = current.Mode == SessionMode.Battle ? new { current.Battle.MainSeed, current.Battle.ThinkingSeed,
            current.Battle.Round, current.Battle.Cursor, current.Battle.Gold, current.Battle.ItemCustody,
            current.Battle.StartPolicy, current.Battle.Regions,
            order = current.Battle.TurnOrder.Select(row => new { row.Actor, row.AlteredAgility }),
            actors = current.Battle.Actors.Select(actor => new { actor.Actor, actor.Hp, actor.Mp, actor.Exp,
                actor.Kills, actor.Defeats, actor.Status, actor.Position, actor.Facing, actor.Progress,
                actor.Initialized, actor.SourceLoadout, actor.ActivationWord, actor.AiMemory }) } : null,
        selection = current.Selection is { } selection ? new { selection.Actor, selection.Stage } : null,
        movement = current.BattleMovement?.GetType().Name,
        story = new { current.Story.Flags, current.Story.Cursor, current.Story.Callers, current.Story.Continuation,
            wait = current.Story.Wait?.GetType().Name, current.Story.TextWindow, current.Story.PortraitWindow,
            current.Story.SimulationTick, current.Story.PartyLists, current.Story.CameraTarget, current.Story.CameraEntitySlot,
            current.Story.Display, current.Story.RandomSeedCopy, current.Story.LogicalText, current.Story.LogicalView,
            current.Story.EntityServices, current.Story.Typewriting },
    });
}
