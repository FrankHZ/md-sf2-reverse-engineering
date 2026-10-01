using System.Text.Json;
using System.Text.Json.Nodes;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class ExplorationTextWaitTests
{
    [Theory]
    [InlineData(false, 0x12341234u)]
    [InlineData(true, 0xC632A55Au)]
    public void NodServicesLiveEntitiesThenWindowAndPortraitWithoutOverwritingPollCopy(bool enabled, uint seed)
    {
        var session = StartFieldText("A{W1}", npcRandom: true, configure: doc => doc["battle"]!["start"]!["mainSeed"] = seed);
        var entry = session.Current;
        var story = entry.Story.Copy(entry.Story.Cursor, new NodWait(new(999), new("ferryman")),
            entityServices: enabled, eventCaller: new ZoneEventContext(), randomSeedCopy: 0xA9,
            logicalText: new(Open: true, AnimationLength: 5),
            portraitWindow: new OpenPortraitWindow(7, 0, new(Blink: 1, Mouth: 0, Registered: true, Movement: 4, Moving: false)));
        var current = entry.WithStory(story);
        var first = ExplorationDispatcher.Submit(session.Definition, current, new AdvanceSimulation(new(999)));
        Assert.Null(first.Failure);
        var entitySeed = enabled ? EntityActionRunner.Tick(entry.Exploration!, storyFlags: story.Flags).World.Party.MainSeed : seed;
        var blink = Sf2.Remake.Domain.Battles.BattleRandom.NextMain(entitySeed, 120);
        var mouth = Sf2.Remake.Domain.Battles.BattleRandom.NextMain(blink.After, 5);
        Assert.Equal(new[] { "nod-service", "rng-portrait-blink", "rng-portrait-mouth", "nod-progress" }, first.Observations.Select(row => row.Kind));
        Assert.Equal(entitySeed, first.Observations[1].Before);
        Assert.Equal(mouth.After, first.Snapshot.Exploration!.Party.MainSeed);
        Assert.Equal(1, first.Snapshot.Story.LogicalText!.AnimationCounter);
        var portrait = Assert.IsType<OpenPortraitWindow>(first.Snapshot.Story.PortraitWindow).Work!;
        Assert.Equal(blink.Value + 30, portrait.Blink);
        Assert.Equal(mouth.Value + 10, portrait.Mouth);
        Assert.Equal((byte)0xA9, first.Snapshot.Story.RandomSeedCopy);
        var rest = ExplorationDispatcher.Submit(session.Definition, first.Snapshot, new AdvanceSimulation(new(999), 600));
        Assert.Null(rest.Failure);
        Assert.Equal(40, rest.Snapshot.Story.SimulationTick - entry.Story.SimulationTick);
        Assert.True(Assert.IsType<NodWait>(rest.Snapshot.Story.Wait).LogicalDone);
        Assert.Equal(5, rest.Snapshot.Story.LogicalText!.AnimationCounter);
        Assert.False(rest.Snapshot.Story.LogicalText.Moving);
        Assert.Equal((byte)0xA9, rest.Snapshot.Story.RandomSeedCopy);
        if (!enabled) Assert.Equal(entry.Exploration!.AllEntities, rest.Snapshot.Exploration!.AllEntities);
        else Assert.NotEqual(entry.Exploration!.Party.MainSeed, rest.Snapshot.Exploration!.Party.MainSeed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HeldCameraUsesCommonLiveEntityWindowPortraitAndRandomService(bool enabled)
    {
        const uint seed = 0xC632A55A;
        var session = StartFieldText("A{W1}", npcRandom: true, configure: doc => doc["battle"]!["start"]!["mainSeed"] = seed);
        var entry = session.Current;
        var view = ExplorationViewRunner.SetDestination(entry.Story.LogicalView!, new(5, 6));
        var story = entry.Story.Copy(entry.Story.Cursor, new ViewWait(new(999), Recheck: false),
            logicalView: view, entityServices: enabled, eventCaller: new ZoneEventContext(), randomSeedCopy: 0xA9,
            logicalText: new(Open: true, AnimationLength: 5),
            portraitWindow: new OpenPortraitWindow(7, 0, new(Blink: 1, Mouth: 0, Registered: true, Movement: 4, Moving: false)));
        var result = ExplorationDispatcher.Submit(session.Definition, entry.WithStory(story), new AdvanceSimulation(new(999)));
        Assert.Null(result.Failure);
        var entitySeed = enabled ? EntityActionRunner.Tick(entry.Exploration!, storyFlags: story.Flags).World.Party.MainSeed : seed;
        var blink = Sf2.Remake.Domain.Battles.BattleRandom.NextMain(entitySeed, 120);
        var mouth = Sf2.Remake.Domain.Battles.BattleRandom.NextMain(blink.After, 5);
        Assert.Equal(new[] { "text-mandatory-service", "rng-portrait-blink", "rng-portrait-mouth", "text-work-advanced" },
            result.Observations.Select(row => row.Kind));
        Assert.Equal(entitySeed, result.Observations[1].Before);
        Assert.Equal(mouth.After, result.Snapshot.Exploration!.Party.MainSeed);
        Assert.Equal((byte)0xA9, result.Snapshot.Story.RandomSeedCopy);
        Assert.Equal(1, result.Snapshot.Story.LogicalText!.AnimationCounter);
        Assert.True(result.Snapshot.Story.LogicalView!.HideWindows);
        Assert.Null(result.Snapshot.Story.LogicalView.TargetSlot);
        Assert.Equal(enabled, result.Snapshot.Story.EntityServices);
        var portrait = Assert.IsType<OpenPortraitWindow>(result.Snapshot.Story.PortraitWindow).Work!;
        Assert.Equal(blink.Value + 30, portrait.Blink);
        Assert.Equal(mouth.Value + 10, portrait.Mouth);
        if (!enabled) Assert.Equal(entry.Exploration!.AllEntities, result.Snapshot.Exploration.AllEntities);
    }

    [Fact]
    public void CameraHelperRestartsWhenRetargetedDuringSettledRecheck()
    {
        var session = StartFieldText("A{W1}", viewWait: true);
        var world = session.Current.Exploration!;
        var view = ExplorationViewRunner.SetDestination(session.Current.Story.LogicalView!, new(0, 0));
        var story = session.Current.Story.Copy(session.Current.Story.Cursor, new ViewWait(new(999), Recheck: true), logicalView: view);
        story = story.Copy(story.Cursor, story.Wait, logicalView: ExplorationViewRunner.SetDestination(view, new(1, 0)));
        story = ExplorationTextRunner.AfterService(ExplorationTextRunner.AfterEntities(world, story));
        Assert.False(Assert.IsType<ViewWait>(story.Wait).Recheck);
        Assert.False(Assert.IsType<ViewWait>(story.Wait).FinalService);
        int services = 1;
        while (story.Wait is ViewWait && services < 30)
        {
            story = ExplorationTextRunner.AfterService(ExplorationTextRunner.AfterEntities(world, story));
            services++;
        }
        Assert.Null(story.Wait);
        Assert.Equal(18, services);
        Assert.Equal(384, story.LogicalView!.BX.Position);
        Assert.Null(story.LogicalView.TargetSlot);
    }

    [Theory]
    [InlineData(0x12341234u, 0, 1, 0xECAB1234u, 236)]
    [InlineData(0xC632A55Au, 1, 16, 0xD764A55Au, 215)]
    [InlineData(0xFFFFBEEFu, 4, 7, 0xA3AEBEEFu, 163)]
    public void EveryPollIncludingAcceptanceDrawsCopiesThenWaitsWithoutEntityServices(uint seed, int polls, int phase, uint expected, int copy)
    {
        var session = StartText("Hello{W1}", seed, phase);
        var entry = session.Current;
        var wait = Assert.IsType<W1TextWait>(entry.Story.Wait);
        Assert.Null(entry.Story.RandomSeedCopy);
        Assert.False(entry.CanWaitForText);
        Assert.Equal("text-input-unavailable", Send(session, new Acknowledge(wait.Token)).Failure!.Code);
        Assert.Equal("text-input-unavailable", Send(session, new WaitForText(wait.Token)).Failure!.Code);
        Assert.Equal("explicit-text-wait-required", Send(session, new AdvanceSimulation(wait.Token)).Failure!.Code);
        Assert.Same(entry, session.Current);
        Accept(session, new CompleteTextReveal(wait.Token));
        Assert.Equal(entry.Story.SimulationTick, session.Current.Story.SimulationTick);
        Assert.Equal(seed, session.Current.Exploration!.Party.MainSeed);
        Assert.True(session.Current.CanWaitForText);
        for (int index = 0; index <= polls; index++)
        {
            var before = session.Current;
            var result = Accept(session, index == polls ? new Acknowledge(wait.Token) : new WaitForText(wait.Token));
            Assert.Equal(new[] { "rng-text-w1", "text-seed-copy", "text-w1-wait", "text-w1-input" },
                result.Observations.Take(4).Select(row => row.Kind));
            var draw = result.Observations[0];
            Assert.Equal((ushort)256, draw.RandomRange);
            Assert.Equal(before.Exploration!.Party.MainSeed, draw.Before);
            Assert.Equal(session.Current.Exploration!.Party.MainSeed, draw.After);
            Assert.Equal(draw.RandomValue, result.Observations[1].After);
            Assert.Equal(index == polls ? "accept" : "none", result.Observations[3].Detail);
            Assert.Equal(before.Story.SimulationTick + 1, session.Current.Story.SimulationTick);
            Assert.Equal(entry.Exploration!.AllEntities, session.Current.Exploration.AllEntities);
            Assert.Equal(entry.Exploration.Party.ThinkingSeed, session.Current.Exploration.Party.ThinkingSeed);
        }
        Assert.Equal(expected, session.Current.Exploration!.Party.MainSeed);
        Assert.Equal((byte)copy, session.Current.Story.RandomSeedCopy);
        Assert.True(session.Current.CanWaitAtInput);
        Assert.Null(session.Current.Story.EntityEvent);
        Assert.IsType<ClosedTextWindow>(session.Current.Story.TextWindow);
        Assert.IsType<ClosedPortraitWindow>(session.Current.Story.PortraitWindow);
        // Ordinary service resumes only after returning control, preserving the copy byte.
        Accept(session, new WaitAtInput());
        Assert.Equal((byte)copy, session.Current.Story.RandomSeedCopy);
        Assert.NotEqual(entry.Exploration!.Entities[new("ferryman")].Motion, session.Current.Exploration.Entities[new("ferryman")].Motion);
    }

    [Fact]
    public void OrderedSpansNamesAndRepeatedWaitsRequireSeparateTokensAndDeliverTheTail()
    {
        var session = StartText("Hi{N}{NAME;1}{W1} then {LEADER}{W1} tail");
        var entry = session.Current;
        var first = Assert.IsType<W1TextWait>(entry.Story.Wait);
        Assert.Equal(3, first.EndToken);
        var tokens = session.Definition.Exploration!.TextTokens[100];
        Assert.Equal(new[] { "Hi", "{N}", "{NAME;1}", "{W1}", " then ", "{LEADER}", "{W1}", " tail" }, tokens.Select(part => part.Value));
        Assert.Equal("B{W2}", session.Definition.Exploration.MemberNames[1]); // Substitution is literal data.
        Accept(session, new CompleteTextReveal(first.Token));
        var staleRevision = session.Current.Revision;
        Accept(session, new WaitForText(first.Token));
        Assert.Equal("stale-input", session.Submit(new(entry.SessionId, staleRevision, null, new Acknowledge(first.Token))).Failure!.Code);
        Accept(session, new Acknowledge(first.Token));
        var second = Assert.IsType<W1TextWait>(session.Current.Story.Wait);
        Assert.NotEqual(first.Token, second.Token);
        Assert.Equal(6, second.EndToken);
        Assert.False(second.Revealed);
        Assert.Same(entry.Story.TextWindow, session.Current.Story.TextWindow);
        var before = session.Current;
        Assert.Equal("stale-or-wrong-wait", Send(session, new Acknowledge(first.Token)).Failure!.Code);
        Assert.Equal("stale-or-wrong-text-delivery", Send(session, new CompleteTextReveal(first.Token)).Failure!.Code);
        Assert.Same(before, session.Current);
        Accept(session, new CompleteTextReveal(second.Token));
        Accept(session, new Acknowledge(second.Token));
        var tail = Assert.IsType<W1TextWait>(session.Current.Story.Wait);
        Assert.False(tail.AtInput);
        Assert.Equal(8, tail.EndToken);
        Assert.False(session.Current.CanWaitForText);
        var afterPolls = session.Current;
        Assert.Equal("text-input-unavailable", Send(session, new Acknowledge(tail.Token)).Failure!.Code);
        var delivered = Accept(session, new CompleteTextReveal(tail.Token));
        Assert.True(session.Current.CanWaitAtInput);
        Assert.Equal(afterPolls.Story.SimulationTick, session.Current.Story.SimulationTick);
        Assert.Equal(afterPolls.Exploration!.Party.MainSeed, session.Current.Exploration!.Party.MainSeed);
        Assert.DoesNotContain(delivered.Observations, row => row.Kind == "rng-text-w1");
        Assert.Equal(101, session.Current.Story.TextCursor);
    }

    [Fact]
    public void DeliveryDelayAndRevealOnlyAttemptsDoNotChangeEquivalentPollOutcomes()
    {
        var instant = StartText("A{W1}{W1}end");
        var delayed = StartText("A{W1}{W1}end");
        foreach (var session in new[] { instant, delayed })
        {
            while (session.Current.Story.Wait is W1TextWait wait)
            {
                var before = session.Current;
                if (session == delayed)
                    for (int index = 0; index < 12; index++)
                    {
                        Send(session, new AdvanceSimulation(wait.Token));
                        Send(session, new WaitForText(wait.Token));
                        Assert.Same(before, session.Current);
                    }
                Accept(session, new CompleteTextReveal(wait.Token));
                if (!wait.AtInput) break;
                Accept(session, new WaitForText(wait.Token));
                Accept(session, new Acknowledge(wait.Token));
            }
        }
        Assert.Equal(instant.Current.Story.SimulationTick, delayed.Current.Story.SimulationTick);
        Assert.Equal(instant.Current.Exploration!.Party.MainSeed, delayed.Current.Exploration!.Party.MainSeed);
        Assert.Equal(instant.Current.Story.RandomSeedCopy, delayed.Current.Story.RandomSeedCopy);
        Assert.Equal(JsonSerializer.Serialize(instant.Current.Exploration.AllEntities), JsonSerializer.Serialize(delayed.Current.Exploration.AllEntities));
        Assert.True(delayed.Current.CanWaitAtInput);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("open")]
    [InlineData("missing")]
    [InlineData("portrait-present")]
    [InlineData("entities-enabled")]
    [InlineData("not-event")]
    [InlineData("legacy-windows")]
    [InlineData("w2")]
    [InlineData("unknown-token")]
    [InlineData("missing-name")]
    public void UnboundConsumerRemainsLegacyAndCannotPollAsW1(string excluded)
    {
        var text = excluded switch { "w2" => "A{W1}B{W2}", "unknown-token" => "A{W1}{S1}", "missing-name" => "{NAME;99}{W1}", _ => "A{W1}" };
        var session = StartText(text, exclusion: excluded);
        Assert.IsType<DialogueWait>(session.Current.Story.Wait);
        Assert.False(session.Current.CanWaitForText);
        var before = session.Current;
        Send(session, new WaitForText(before.Story.Wait!.Token));
        Assert.Same(before, session.Current);
        Assert.Null(session.Current.Story.RandomSeedCopy);
        Accept(session, new Acknowledge(before.Story.Wait.Token));
        Assert.Equal(before.Exploration!.Party.MainSeed, session.Current.Exploration!.Party.MainSeed);
        Assert.Equal(before.Story.SimulationTick, session.Current.Story.SimulationTick);
    }

    [Fact]
    public void OrdinaryFacingServiceRunsBeforeSuppressedW1AndRestoresFacingOnReturn()
    {
        var session = StartText("A{W1}", face: true);
        Assert.IsType<EntityEventFacingWait>(session.Current.Story.Wait);
        var entry = session.Current;
        var npc = entry.Exploration!.Entities[new("ferryman")];
        Accept(session, new AdvanceSimulation(entry.Story.Wait!.Token));
        var wait = Assert.IsType<W1TextWait>(session.Current.Story.Wait);
        Assert.Equal(entry.Story.SimulationTick + 1, session.Current.Story.SimulationTick);
        Assert.NotEqual(npc.Motion, session.Current.Exploration!.Entities[new("ferryman")].Motion);
        var faced = session.Current.Exploration.Entities[new("ferryman")];
        Accept(session, new CompleteTextReveal(wait.Token));
        Accept(session, new WaitForText(wait.Token));
        Assert.Equal(faced, session.Current.Exploration.Entities[new("ferryman")]);
        Accept(session, new Acknowledge(wait.Token));
        Assert.Equal(npc.Motion.Facing, session.Current.Exploration.Entities[new("ferryman")].Motion.Facing);
        Assert.True(session.Current.CanWaitAtInput);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 16)]
    [InlineData(2, 14)]
    [InlineData(3, 12)]
    public void FreshWindowGlyphWorkAndCloseDoNotDependOnDelivery(int speed, int mandatory)
    {
        foreach (bool early in new[] { false, true })
        {
            var session = StartFieldText("AB{W2}", speed: speed);
            var wait = Assert.IsType<FieldTextWait>(session.Current.Story.Wait);
            if (early) Accept(session, new CompleteTextReveal(wait.Token));
            DrainTextWork(session);
            Assert.Equal(mandatory, session.Current.Story.SimulationTick);
            Assert.Equal(14, session.Current.Story.LogicalText!.X);
            Assert.Equal(0, session.Current.Story.LogicalText.Y);
            Assert.Equal(early, session.Current.CanWaitForText);
            if (!early) Accept(session, new CompleteTextReveal(wait.Token));
            var poll = Accept(session, new Acknowledge(wait.Token));
            Assert.Contains(poll.Observations, row => row.Kind == "text-w2-accepted");
            Assert.IsType<TextCloseWait>(session.Current.Story.Wait);
            Assert.False(session.Current.Story.LogicalText.IndicatorVisible);
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token, 8));
            Assert.IsType<TextCloseWait>(session.Current.Story.Wait);
            Assert.True(session.Current.Story.LogicalText.Moving);
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
            Assert.True(session.Current.CanWaitAtInput);
            Assert.Equal(mandatory + 10, session.Current.Story.SimulationTick);
            Assert.False(session.Current.Story.LogicalText.Open);
        }
    }

    [Fact]
    public void NamesWidthsWrapScrollAndReusedWindowConsumeActualWork()
    {
        var session = StartFieldText("{NAME;1}{NAME;1}{NAME;1}{W2}{W1}tail", name: new string('W', 14), width: 16, second: "Z{W1}");
        DrainTextWork(session);
        var first = Assert.IsType<FieldTextWait>(session.Current.Story.Wait);
        Assert.Equal(new string('W', 42), first.Projection);
        // Four lines, with one two-row scroll. Width rather than character count controls wrap.
        Assert.Equal(10 + 42 * 2 + 3, session.Current.Story.SimulationTick);
        Assert.Equal(50, session.Current.Story.LogicalText!.X);
        Assert.Equal(32, session.Current.Story.LogicalText.Y);
        Assert.Equal(2, session.Current.Story.LogicalText.Row);
        Accept(session, new CompleteTextReveal(first.Token));
        Accept(session, new Acknowledge(first.Token));
        var consecutive = Assert.IsType<FieldTextWait>(session.Current.Story.Wait);
        Assert.True(consecutive.LogicalDone);
        Assert.False(consecutive.Wait2);
        Assert.NotEqual(first.Token, consecutive.Token);
        Assert.Equal("stale-or-wrong-text-delivery", Send(session, new CompleteTextReveal(first.Token)).Failure!.Code);
        Assert.Equal("stale-or-wrong-wait", Send(session, new Acknowledge(first.Token)).Failure!.Code);
        Accept(session, new CompleteTextReveal(consecutive.Token));
        var revision = session.Current.Revision;
        Accept(session, new WaitForText(consecutive.Token));
        Assert.Equal("stale-input", session.Submit(new(session.Current.SessionId, revision, null, new Acknowledge(consecutive.Token))).Failure!.Code);
        Accept(session, new Acknowledge(consecutive.Token));
        var tail = Assert.IsType<FieldTextWait>(session.Current.Story.Wait);
        Assert.False(tail.LogicalDone);
        DrainTextWork(session);
        long beforeReuse = session.Current.Story.SimulationTick;
        Accept(session, new CompleteTextReveal(tail.Token));
        var reused = Assert.IsType<FieldTextWait>(session.Current.Story.Wait);
        Assert.Equal(101, reused.Text);
        Assert.Equal(FieldTextPhase.Scroll, reused.Phase); // A new display starts a new line.
        DrainTextWork(session);
        Assert.Equal(beforeReuse + 3 + 2, session.Current.Story.SimulationTick);
        Assert.Equal(18, session.Current.Story.LogicalText.X);
        Assert.Equal(32, session.Current.Story.LogicalText.Y);
        Assert.Equal(4, session.Current.Story.LogicalText.Row);
        Assert.Equal(102, session.Current.Story.TextCursor);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 3)]
    [InlineData(true, 1)]
    [InlineData(true, 4)]
    public void PollCopiesBeforeEnabledNpcDrawAndKeepsSuppressedServices(bool enabled, int optional)
    {
        var session = StartFieldText("{W2}", enabled: enabled, npcRandom: true);
        DrainTextWork(session);
        var wait = Assert.IsType<FieldTextWait>(session.Current.Story.Wait);
        Accept(session, new CompleteTextReveal(wait.Token));
        var entry = session.Current;
        for (int i = 0; i <= optional; i++)
        {
            var before = session.Current;
            var result = Accept(session, i == optional ? new Acknowledge(wait.Token) : new WaitForText(wait.Token));
            Assert.Equal(new[] { "rng-text-w2", "text-seed-copy", "text-w2-wait", "text-w2-input" }, result.Observations.Take(4).Select(row => row.Kind));
            var draw = result.Observations[0];
            Assert.Equal(before.Exploration!.Party.MainSeed, draw.Before);
            Assert.Equal((byte)draw.RandomValue!.Value, session.Current.Story.RandomSeedCopy);
            Assert.Equal(before.Story.SimulationTick + 1, session.Current.Story.SimulationTick);
            if (enabled) Assert.NotEqual(draw.After, session.Current.Exploration!.Party.MainSeed);
            else Assert.Equal(draw.After, session.Current.Exploration!.Party.MainSeed);
        }
        if (!enabled) Assert.Equal(entry.Exploration!.AllEntities, session.Current.Exploration!.AllEntities);
    }

    [Theory]
    [InlineData(0, 4)]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    [InlineData(3, 0)]
    public void NeutralGlyphWorkIgnoresRevealAndSourceShorteningRequiresItsOwnInput(int speed, int delay)
    {
        Assert.Equal(delay, ExplorationTextRunner.TypewriteDelay(new((byte)speed, 0, 0), 0));
        Assert.Equal(0, ExplorationTextRunner.TypewriteDelay(new((byte)speed, 0, 0), 1));
        Assert.Equal(delay, ExplorationTextRunner.TypewriteDelay(new((byte)speed, 255, 0), 1));
        var session = StartFieldText("A{W1}", speed: speed);
        var entry = session.Current;
        Accept(session, new CompleteTextReveal(entry.Story.Wait!.Token));
        Assert.Equal(entry.Story.SimulationTick, session.Current.Story.SimulationTick);
        Assert.Equal(entry.Exploration!.Party, session.Current.Exploration!.Party);
        Assert.Equal("text-input-unavailable", Send(session, new Acknowledge(entry.Story.Wait.Token)).Failure!.Code);
        DrainTextWork(session);
        Assert.Equal(11 + delay, session.Current.Story.SimulationTick);
    }

    [Fact]
    public void ViewHelperRechecksWhenItsFirstServiceStartsScrolling()
    {
        var session = StartFieldText("A{W1}", viewWait: true);
        var world = session.Current.Exploration!;
        var view = session.Current.Story.LogicalView!;
        var player = world.PlayerEntity;
        world = world.WithEntity(player with { Motion = player.Motion with { X = 2305 } });
        var story = session.Current.Story;
        Assert.IsType<ViewWait>(story.Wait);
        story = ExplorationTextRunner.AfterService(ExplorationTextRunner.AfterEntities(world, story));
        Assert.IsType<ViewWait>(story.Wait);
        Assert.True(story.LogicalView!.Scrolling);
        Assert.False(((ViewWait)story.Wait!).FinalService);
        int services = 1;
        while (story.Wait is ViewWait && services < 30)
        {
            story = ExplorationTextRunner.AfterService(ExplorationTextRunner.AfterEntities(world, story));
            services++;
        }
        Assert.Null(story.Wait);
        Assert.Equal(18, services); // 16 scroll passes, then settled recheck and final wait.
        Assert.Equal(384, story.LogicalView!.BX.Position);
        Assert.Null(story.LogicalView.BX.Destination);
        Assert.Equal(view.AY.Position, story.LogicalView.AY.Position);
    }

    [Fact]
    public void ViewDeadbandsClampsCounterAndIndependentAxesAreStateDriven()
    {
        var session = StartFieldText("A{W1}");
        var world = session.Current.Exploration!;
        var view = session.Current.Story.LogicalView!;
        var player = world.PlayerEntity;
        LogicalView TickAt(int x, int y, LogicalView current) => ExplorationViewRunner.Tick(
            world.WithEntity(player with { Motion = player.Motion with { X = (short)x, Y = (short)y } }), current, new(2, 0, 0));
        Assert.False(TickAt(2304, 1536, view).Scrolling);
        Assert.False(TickAt(0, 0, view).Scrolling);
        var moving = TickAt(2305, 2305, view with { FollowCounter = 6 });
        Assert.Equal(32, moving.BX.Position);
        Assert.Equal(7, moving.FollowCounter);
        Assert.Equal(24, TickAt(2305, 1536, view with { FollowCounter = 32767 }).BX.Speed);
        Assert.Equal(384, moving.AX.Destination);
        Assert.Equal(384 + 32 * 384, moving.AY.Destination);
        var finishing = moving with { AX = new(380, 384, 32), BX = new(360, 384, 24), AY = new(32 * 384 + 300, 32 * 384 + 384, 24), BY = new(300, 384, 24) };
        var next = TickAt(0, 0, finishing);
        Assert.False(next.AX.Active);
        Assert.False(next.BX.Active);
        Assert.True(next.AY.Active);
        Assert.Equal(324, next.BY.Position);
        Assert.True(next.HideWindows);
        var upper = view with { AX = new(20 * 384), BX = new(20 * 384), AY = new(21 * 384 + 32 * 384), BY = new(21 * 384) };
        Assert.False(TickAt(30 * 384, 30 * 384, upper).Scrolling);
        Assert.Equal("field-view-profile", Assert.Throws<Sf2.Remake.Domain.Battles.BattleRuleException>(() => ExplorationViewRunner.Tick(world, view with { Area = view.Area with { ParallaxAX = 192 } }, new(2, 0, 0))).Message);
        Assert.Throws<Sf2.Remake.Domain.Battles.BattleRuleException>(() => ExplorationViewRunner.Tick(world, view with { TargetSlot = 63 }, new(2, 0, 0)));
        Assert.Throws<Sf2.Remake.Domain.Battles.BattleRuleException>(() => ExplorationViewRunner.Tick(world, view, new(2, 0, 1)));
    }

    [Fact]
    public void W2IndicatorBlinksWhileW1AndUnknownPortraitDoNotGainItsBehavior()
    {
        var session = StartFieldText("{W2}{W1}");
        var token = session.Current.Story.Wait!.Token;
        DrainTextWork(session);
        Accept(session, new CompleteTextReveal(token));
        for (int i = 0; i < 20; i++)
        {
            Accept(session, new WaitForText(token));
            Assert.Equal(i < 14, session.Current.Story.LogicalText!.IndicatorVisible);
        }
        Assert.Equal(20, session.Current.Story.LogicalText!.Indicator);
        Accept(session, new Acknowledge(token));
        var next = Assert.IsType<FieldTextWait>(session.Current.Story.Wait);
        Assert.False(next.Wait2);
        Accept(session, new CompleteTextReveal(next.Token));
        var result = Accept(session, new Acknowledge(next.Token));
        Assert.DoesNotContain(result.Observations, row => row.Kind == "text-w2-accepted");
        var current = session.Current;
        Assert.Throws<Sf2.Remake.Domain.Battles.BattleRuleException>(() => ExplorationTextRunner.ValidateContext(
            current.WithStory(current.Story.Copy(current.Story.Cursor, current.Story.Wait, portraitWindow: new UnknownPortraitWindow()))));
        Assert.Throws<Sf2.Remake.Domain.Battles.BattleRuleException>(() => ExplorationTextRunner.ValidateContext(
            current.WithStory(current.Story.Copy(current.Story.Cursor, current.Story.Wait, continuation: ProgramContinuation.BeforeBattleFinished))));
    }

    [Fact]
    public void EntityWrapperRestoresFacingThenClosesWithSuppressedServicesExactlyOnce()
    {
        var session = StartFieldText("{W1}", interaction: true);
        DrainTextWork(session);
        var token = session.Current.Story.Wait!.Token;
        Accept(session, new CompleteTextReveal(token));
        var result = Accept(session, new Acknowledge(token));
        var closing = session.Current;
        Assert.True(Assert.IsType<TextCloseWait>(closing.Story.Wait).CallerReturn);
        Assert.NotNull(closing.Story.EntityEvent);
        Assert.True(closing.Story.LogicalText!.Open);
        Assert.IsType<OpenTextWindow>(closing.Story.TextWindow);
        Assert.False(closing.CanWaitAtInput);
        Assert.Single(result.Observations, row => row.Kind == "interaction-closing");
        var all = new List<SessionObservation>();
        for (int i = 0; i < 9; i++) all.AddRange(Accept(session, new AdvanceSimulation(closing.Story.Wait!.Token)).Observations);
        Assert.Equal(closing.Exploration!.AllEntities, session.Current.Exploration!.AllEntities);
        Assert.Equal(closing.Exploration.Party.MainSeed, session.Current.Exploration.Party.MainSeed);
        Assert.Equal(closing.Story.SimulationTick + 9, session.Current.Story.SimulationTick);
        Assert.Null(session.Current.Story.EntityEvent);
        Assert.Null(session.Current.Story.Cursor);
        Assert.False(session.Current.Story.LogicalText!.Open);
        Assert.IsType<ClosedTextWindow>(session.Current.Story.TextWindow);
        Assert.True(session.Current.CanWaitAtInput);
        Assert.Single(all, row => row.Kind == "interaction-finished");
    }

    [Fact]
    public void LeaderNameComesFromCurrentActiveMembershipRatherThanMemberZero()
    {
        var session = StartFieldText("{LEADER}{W1}", alternateLeader: true);
        DrainTextWork(session);
        Assert.Equal("Name", Assert.IsType<FieldTextWait>(session.Current.Story.Wait).Projection);
        Assert.Equal(18, session.Current.Story.SimulationTick);
        Assert.Equal(new[] { 1 }, session.Current.Story.PartyLists!.Active);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PortraitEventCarriesTextTailAndScriptActivationThroughRealReturn(bool script)
    {
        var session = StartFieldText("A{W2}", interaction: true, startEvent: false, configure: document =>
        {
            AddVisuals(document, 7);
            var world = document["world"]!;
            world["texts"]![1]!["text"] = "B";
            world["texts"]!.AsArray().Add(JsonNode.Parse("""{"id":900,"text":"C{W1}"}"""));
            var instructions = world["programs"]![0]!["instructions"]!.AsArray();
            instructions.Insert(2, instructions[1]!.DeepClone());
            instructions.Insert(3, JsonNode.Parse("""{"op":"text-cursor","text":900}"""));
            instructions.Insert(4, instructions[1]!.DeepClone());
            if (script)
            {
                instructions.Insert(5, JsonNode.Parse("""{"op":"call","activateEntities":true,"target":{"program":"walk-return","instruction":0}}"""));
                world["programs"]!.AsArray().Add(JsonNode.Parse("""{"id":"walk-return","instructions":[{"op":"wait-ticks","ticks":1},{"op":"end-map-script"}]}"""));
            }
        });
        var entry = Accept(session, new Interact(new("ferryman")));
        Assert.Single(entry.Observations, row => row.Kind == "portrait-window-moving");
        Assert.True(session.Current.Story.EntityServices);
        Assert.Equal(new PortraitWork(), Assert.IsType<OpenPortraitWindow>(session.Current.Story.PortraitWindow).Work);
        for (int i = 0; i < 5; i++) Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.IsType<EntityEventFacingWait>(session.Current.Story.Wait);
        var opened = Assert.IsType<OpenPortraitWindow>(session.Current.Story.PortraitWindow);
        Assert.True(opened.Work!.Registered);
        Assert.Equal(20, opened.Work.Blink);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.False(session.Current.Story.EntityServices);
        Assert.Equal(19, Assert.IsType<OpenPortraitWindow>(session.Current.Story.PortraitWindow).Work!.Blink);
        int acknowledgements = 0;
        for (int display = 0; display < 3; display++)
        {
            var text = Assert.IsType<FieldTextWait>(session.Current.Story.Wait);
            DrainTextWork(session);
            Assert.False(session.Current.Story.Typewriting);
            Accept(session, new CompleteTextReveal(text.Token));
            if (display == 1)
            {
                Assert.Equal(900, Assert.IsType<FieldTextWait>(session.Current.Story.Wait).Text);
                continue; // The no-W trailing display finished without an invented Ack.
            }
            Assert.True(session.Current.CanWaitForText);
            Accept(session, new WaitForText(text.Token));
            Accept(session, new Acknowledge(text.Token));
            acknowledgements++;
        }
        Assert.Equal(2, acknowledgements);
        if (script)
        {
            Assert.True(session.Current.Story.EntityServices);
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
            Assert.True(Assert.IsType<ViewWait>(session.Current.Story.Wait).ScriptReturn);
            while (session.Current.Story.Wait is ViewWait)
                Accept(session, new AdvanceSimulation(session.Current.Story.Wait.Token));
        }
        Assert.True(Assert.IsType<PortraitMovementWait>(session.Current.Story.Wait).Closing);
        var closing = Assert.IsType<OpenPortraitWindow>(session.Current.Story.PortraitWindow).Work!;
        Assert.False(closing.Registered);
        Assert.Equal(script, session.Current.Story.EntityServices);
        for (int i = 0; i < 5; i++) Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.IsType<ClosedPortraitWindow>(session.Current.Story.PortraitWindow);
        Assert.True(Assert.IsType<TextCloseWait>(session.Current.Story.Wait).CallerReturn);
        Assert.Equal(script, session.Current.Story.EntityServices);
        for (int i = 0; i < 9; i++) Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.True(session.Current.CanWaitAtInput);
        Assert.True(session.Current.Story.EntityServices);
        Assert.Null(session.Current.Story.EntityEvent);
        Assert.IsType<ClosedTextWindow>(session.Current.Story.TextWindow);
    }

    [Theory]
    [InlineData(false, 1, 0, 0x04B61234u, 140, 10, false, false)]
    [InlineData(true, 1, 1, 0x04B61234u, 140, 10, false, false)]
    [InlineData(false, 4, 6, 0x12341234u, 3, 6, true, false)]
    [InlineData(true, 20, 6, 0x12341234u, 19, 5, false, true)]
    [InlineData(false, 20, 5, 0xECAB1234u, 19, 14, false, false)]
    public void PortraitCountersUseSourceTypewritingAndOrderedIndependentRng(bool typing, short blink, short mouth,
        uint seed, short expectedBlink, short expectedMouth, bool eyes, bool lips)
    {
        var session = StartFieldText("A{W1}", configure: doc => doc["battle"]!["start"]!["mainSeed"] = 0x12341234u);
        var current = session.Current;
        var state = current.Story.Copy(current.Story.Cursor, current.Story.Wait, typewriting: typing,
            randomSeedCopy: 123, portraitWindow: new OpenPortraitWindow(7, 0,
                new(Blink: blink, Mouth: mouth, Registered: true, Movement: 4, Moving: false)));
        var observations = new List<SessionObservation>();
        var next = ExplorationPortraitRunner.Service(current.WithStory(state), observations);
        var work = Assert.IsType<OpenPortraitWindow>(next.Story.PortraitWindow).Work!;
        Assert.Equal((expectedBlink, expectedMouth, eyes, lips), (work.Blink, work.Mouth, work.EyesClosed, work.MouthOpen));
        Assert.Equal(seed, next.Exploration!.Party.MainSeed);
        Assert.Equal((byte)123, next.Story.RandomSeedCopy);
        if (blink == 1) Assert.Equal(new ushort?[] { 120, 5 }, observations.Select(row => row.RandomRange));
    }

    [Fact]
    public void ExistingPortraitIsRetainedAndUnknownCannotBeSilentlyOpened()
    {
        var session = StartFieldText("A{W1}", interaction: true);
        var current = session.Current;
        var portrait = new OpenPortraitWindow(7, 192, new(Blink: 7, Mouth: 11, Registered: true, Movement: 4, Moving: false));
        current = current.WithStory(current.Story.Copy(current.Story.Cursor, portraitWindow: portrait));
        var kept = ExplorationPortraitRunner.Open(session.Definition, current, new("ferryman"), 0, [], false);
        Assert.Same(portrait, kept.Story.PortraitWindow);
        Assert.Throws<Sf2.Remake.Domain.Battles.BattleRuleException>(() => ExplorationPortraitRunner.Open(session.Definition,
            current.WithStory(current.Story.Copy(current.Story.Cursor, portraitWindow: new UnknownPortraitWindow())), new("ferryman"), 0, [], false));
    }

    [Theory]
    [InlineData(0, 7)]
    [InlineData(3, 12)]
    public void PortraitAdmissionUsesLiveSpeakerAndRepeatedFlagBranch(int flags, int portraitId)
    {
        var session = StartFieldText("{W1}", interaction: true, startEvent: false, configure: document =>
        {
            AddVisuals(document, portraitId);
            var world = document["world"]!;
            world["presentation"]!["sprites"]![0]!["sprite"] = 40 + portraitId;
            world["maps"]![0]!["entities"]![0]!["sprite"] = 40 + portraitId;
            world["presentation"]!["portraits"]![0]!["portrait"] = portraitId;
            world["maps"]![0]!["events"]![0]!["entityFlags"] = flags;
            world["texts"]![1]!["text"] = "{W2}";
            world["programs"]![0]!["instructions"] = JsonNode.Parse("""
                [{"op":"branch-flag","flag":777,"whenSet":true,"target":{"program":"invitation","instruction":5}},
                 {"op":"text-cursor","text":100},{"op":"show-text","mode":"single","speaker":"ferryman","explicitWindows":true},
                 {"op":"set-flag","flag":777,"value":true},{"op":"end"},
                 {"op":"text-cursor","text":101},{"op":"show-text","mode":"single","speaker":"ferryman","explicitWindows":true},{"op":"end"}]
                """);
        });
        for (int visit = 0; visit < 2; visit++)
        {
            byte originalFacing = session.Current.Exploration!.Entities[new("ferryman")].Motion.Facing;
            Accept(session, new Interact(new("ferryman")));
            Assert.Equal(portraitId, Assert.IsType<OpenPortraitWindow>(session.Current.Story.PortraitWindow).Portrait);
            for (int step = 0; step < 5; step++)
            {
                Assert.False(Assert.IsType<OpenPortraitWindow>(session.Current.Story.PortraitWindow).Work!.Registered);
                Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
            }
            if ((flags & 1) != 0) Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
            Assert.Equal(100 + visit, Assert.IsType<FieldTextWait>(session.Current.Story.Wait).Text);
            DrainTextWork(session);
            var token = session.Current.Story.Wait!.Token;
            Accept(session, new CompleteTextReveal(token));
            Accept(session, new Acknowledge(token));
            Assert.Equal(originalFacing, session.Current.Exploration!.Entities[new("ferryman")].Motion.Facing);
            var removed = Assert.IsType<OpenPortraitWindow>(session.Current.Story.PortraitWindow).Work!;
            for (int step = 0; step < 5; step++)
            {
                Assert.False(session.Current.CanWaitAtInput);
                var work = Assert.IsType<OpenPortraitWindow>(session.Current.Story.PortraitWindow).Work!;
                Assert.False(work.Registered);
                Assert.Equal((removed.Blink, removed.Mouth), (work.Blink, work.Mouth));
                Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
            }
            while (session.Current.Story.Wait is TextCloseWait)
                Accept(session, new AdvanceSimulation(session.Current.Story.Wait.Token));
            Assert.True(session.Current.CanWaitAtInput);
            Assert.Contains(777, session.Current.Story.Flags);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(64)]
    [InlineData(128)]
    [InlineData(192)]
    public void FreshPortraitRetainsRequestedPlacementAndMirrorFlags(byte flags)
    {
        var session = StartFieldText("{W1}", interaction: true, startEvent: false,
            configure: document => AddVisuals(document, 7));
        var current = session.Current.WithStory(session.Current.Story.Copy(null, entityEvent: new(new("ferryman"), 1, 0)));
        var opened = ExplorationPortraitRunner.Open(session.Definition, current, new("ferryman"), flags, [], false);
        var portrait = Assert.IsType<OpenPortraitWindow>(opened.Story.PortraitWindow);
        Assert.Equal(flags, portrait.Flags);
        Assert.Equal(new PortraitWork(), portrait.Work);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PollCopySurvivesNpcThenBlinkAndMouthDraws(bool zone)
    {
        var session = StartFieldText("{W2}", enabled: true, npcRandom: true);
        DrainTextWork(session);
        var token = session.Current.Story.Wait!.Token;
        Accept(session, new CompleteTextReveal(token));
        var entry = session.Current;
        var world = entry.Exploration!;
        var party = world.Party;
        world = world.WithParty(new(party.Encounter, party.Actors, 0x12341234u, party.ThinkingSeed, party.Gold, party.NewBattle));
        var story = entry.Story.Copy(entry.Story.Cursor, entry.Story.Wait, entityServices: true,
            eventCaller: zone ? new ZoneEventContext() : new EntityEventContext(new("ferryman"), 1, 0),
            portraitWindow: new OpenPortraitWindow(7, 0, new(Blink: 1, Mouth: 0, Registered: true, Movement: 4, Moving: false)));
        var snapshot = new SessionSnapshot(entry.SessionId, entry.Revision, entry.ObservationSequence,
            new ActiveExploration(world), story, entry.StopReason);
        var result = ExplorationDispatcher.Submit(session.Definition, snapshot, new WaitForText(token));
        Assert.Null(result.Failure);
        Assert.Equal(new[] { "rng-text-w2", "text-seed-copy", "text-w2-wait", "rng-portrait-blink", "rng-portrait-mouth", "text-w2-input" },
            result.Observations.Select(row => row.Kind));
        // Independent LCG sequence: one poll, four radius-zero rejected NPC candidates, blink, mouth.
        Assert.Equal(0xECAB1234L, result.Observations[0].After);
        Assert.Equal((byte)236, result.Snapshot.Story.RandomSeedCopy);
        Assert.Equal(0x72EF1234L, result.Observations[3].Before);
        Assert.Equal(0xE0291234u, result.Snapshot.Exploration!.Party.MainSeed);
    }

    [Fact]
    public void SourceScriptEndWithoutDialogueClearsViewOverrideWithoutWaiting()
    {
        var session = StartFieldText("{W1}", configure: document =>
        {
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse("""[{"op":"end-map-script"}]""");
        });
        var current = session.Current;
        var result = ProgramRunner.Run(session.Definition, current.WithStory(current.Story.Copy(new("invitation", 0),
            textSettings: current.Story.TextSettings! with { ViewSpeed = 1 })), []);
        Assert.Null(result.Failure);
        Assert.True(result.Snapshot.CanWaitAtInput);
        Assert.Equal(0, result.Snapshot.Story.TextSettings!.ViewSpeed);
        Assert.Equal(0, result.Snapshot.Story.SimulationTick);
    }

    [Fact]
    public void FreshDialogueEnablesTypewritingAfterFinalCreationServiceBeforeFirstGlyphService()
    {
        var session = StartFieldText("A{W1}B{W1}", second: "C{W1}", interaction: true, startEvent: false,
            configure: document => AddVisuals(document, 7));
        Accept(session, new Interact(new("ferryman")));
        for (int i = 0; i < 6; i++) Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.Equal(FieldTextPhase.ClearFirst, Assert.IsType<FieldTextWait>(session.Current.Story.Wait).Phase);
        uint seed = session.Current.Exploration!.Party.MainSeed;
        for (int i = 0; i < 10; i++)
        {
            Assert.False(session.Current.Story.Typewriting);
            var result = Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
            var work = Assert.IsType<OpenPortraitWindow>(session.Current.Story.PortraitWindow).Work!;
            Assert.Equal(18 - i, work.Blink);
            Assert.Equal(6, work.Mouth);
            Assert.False(work.MouthOpen);
            Assert.Equal(seed, session.Current.Exploration!.Party.MainSeed);
            Assert.DoesNotContain(result.Observations, row => row.Kind.StartsWith("rng-portrait-"));
        }
        Assert.True(session.Current.Story.Typewriting);
        Assert.Equal(FieldTextPhase.GlyphCursor, Assert.IsType<FieldTextWait>(session.Current.Story.Wait).Phase);
        Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        var firstGlyph = Assert.IsType<OpenPortraitWindow>(session.Current.Story.PortraitWindow).Work!;
        Assert.Equal(5, firstGlyph.Mouth);
        Assert.True(firstGlyph.MouthOpen);
        DrainTextWork(session);
        Assert.False(session.Current.Story.Typewriting);
        var token = session.Current.Story.Wait!.Token;
        Accept(session, new CompleteTextReveal(token));
        Accept(session, new Acknowledge(token));
        Assert.True(session.Current.Story.Typewriting); // W continuation restores typing for B.
        Assert.Equal(FieldTextPhase.GlyphCursor, Assert.IsType<FieldTextWait>(session.Current.Story.Wait).Phase);
        DrainTextWork(session);
        token = session.Current.Story.Wait!.Token;
        Accept(session, new CompleteTextReveal(token));
        long beforeReuse = session.Current.Story.SimulationTick;
        Accept(session, new Acknowledge(token));
        Assert.Equal(beforeReuse + 1, session.Current.Story.SimulationTick); // Accepting poll only.
        Assert.True(session.Current.Story.Typewriting);
        var reused = Assert.IsType<FieldTextWait>(session.Current.Story.Wait);
        Assert.Equal(101, reused.Text);
        Assert.Equal(FieldTextPhase.GlyphCursor, reused.Phase); // Already-open window returns immediately.
    }

    [Theory]
    [InlineData("")]
    [InlineData("{W1}")]
    [InlineData("{W2}")]
    public void EmptyOrWaitOnlyDialogueDoesNotExposeTypingToPortraitService(string text)
    {
        var session = StartFieldText(text.Length == 0 ? "{W1}" : text, interaction: true, startEvent: false,
            configure: document => AddVisuals(document, 7));
        Accept(session, new Interact(new("ferryman")));
        for (int i = 0; i < 6; i++) Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        var current = session.Current;
        // The authored reader requires a nonempty text string; exercise an empty engine token stream directly.
        if (text.Length == 0)
        {
            var wait = Assert.IsType<FieldTextWait>(current.Story.Wait);
            current = current.WithStory(current.Story.Copy(current.Story.Cursor,
                wait with { Units = [], End = 0, Projection = "" }));
        }
        for (int i = 0; i < 10; i++)
        {
            Assert.False(current.Story.Typewriting);
            var result = ExplorationDispatcher.Submit(session.Definition, current, new AdvanceSimulation(current.Story.Wait!.Token));
            Assert.Null(result.Failure);
            current = result.Snapshot;
        }
        Assert.False(current.Story.Typewriting);
        Assert.Equal(text.Length == 0 ? FieldTextPhase.End : FieldTextPhase.Input,
            Assert.IsType<FieldTextWait>(current.Story.Wait).Phase);
        var work = Assert.IsType<OpenPortraitWindow>(current.Story.PortraitWindow).Work!;
        Assert.Equal(9, work.Blink);
        Assert.Equal(6, work.Mouth);
        Assert.False(work.MouthOpen);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void DialogueCreationPreservesIncomingTypingAndKeepsPortraitService(bool incoming, bool zone)
    {
        var session = StartFieldText("A{W1}");
        var entry = session.Current;
        var current = entry.WithStory(entry.Story.Copy(entry.Story.Cursor, textCursor: 100, typewriting: incoming,
            entityServices: zone, eventCaller: zone ? new ZoneEventContext() : new EntityEventContext(new("ferryman"), 1, 0),
            portraitWindow: new OpenPortraitWindow(7, 0,
                new(Blink: 4, Mouth: 5, MouthOpen: true, Registered: true, Movement: 4, Moving: false))));
        var story = ExplorationTextRunner.Begin(session.Definition.Exploration!, current,
            new(TextDisplayMode.Single, new("ferryman")), new(999));
        Assert.Equal(incoming, story.Typewriting);
        var result = ExplorationDispatcher.Submit(session.Definition, current.WithStory(story), new AdvanceSimulation(story.Wait!.Token));
        Assert.Null(result.Failure);
        Assert.Equal(incoming, result.Snapshot.Story.Typewriting);
        var work = Assert.IsType<OpenPortraitWindow>(result.Snapshot.Story.PortraitWindow).Work!;
        Assert.Equal(3, work.Blink);
        Assert.True(work.EyesClosed);
        Assert.Equal(incoming, work.MouthOpen);
        if (incoming) Assert.Equal(4, work.Mouth);
        else
        {
            Assert.InRange(work.Mouth, (short)10, (short)14);
            Assert.Single(result.Observations, row => row.Kind == "rng-portrait-mouth");
        }
    }

    [Fact]
    public void ZoneInitPreservesPendingMotionAndTimerAndRetainsOrRejectsIncomingPortrait()
    {
        var session = StartFieldText("A{W1}");
        var entry = session.Current;
        var world = entry.Exploration!;
        var player = world.PlayerEntity;
        var motion = player.Motion with { XDestination = (short)(player.Motion.X + 384), XTravel = 384,
            XVelocity = 8, XSpeed = 8, WaitTimer = 19 };
        world = world.WithEntity(player with { Motion = motion });
        var retained = new OpenPortraitWindow(7, 192, new(Blink: 3, Mouth: 12, Registered: true, Movement: 4, Moving: false));
        var before = new SessionSnapshot(entry.SessionId, entry.Revision, entry.ObservationSequence, new ActiveExploration(world),
            entry.Story.Copy(null, portraitWindow: retained), entry.StopReason);
        var init = new EntityActionProgram([new SetEntitySpeed(32, 32), new IdleEntityAction()]);
        var next = MapEventDispatcher.EnterZone(before, new(ExplorationEventKind.SourceZone, null, null, null,
            new("invitation", 0), SourceInit: init), false, []);
        Assert.Equal(motion, next.Exploration!.PlayerEntity.Motion);
        Assert.Same(init, next.Exploration.PlayerEntity.Actions);
        Assert.False(next.Story.EntityServices);
        var kept = ExplorationPortraitRunner.Open(session.Definition, next, new("ferryman"), 0, [], false);
        Assert.Same(retained, kept.Story.PortraitWindow);
        Assert.Throws<Sf2.Remake.Domain.Battles.BattleRuleException>(() => ExplorationPortraitRunner.Open(session.Definition,
            next.WithStory(next.Story.Copy(next.Story.Cursor, portraitWindow: new UnknownPortraitWindow())), new("ferryman"), 0, [], false));
        Assert.Throws<Sf2.Remake.Domain.Battles.BattleRuleException>(() => ExplorationPortraitRunner.Close(
            next.WithStory(next.Story.Copy(next.Story.Cursor, portraitWindow: new UnknownPortraitWindow())), [], true));
    }

    [Theory]
    [InlineData(7, false, 0x12341234u)]
    [InlineData(9, false, 0xFEDC4321u)]
    [InlineData(null, false, 0xC632A55Au)]
    [InlineData(7, true, 0xFFFFBEEFu)]
    public void ZoneDialogueUsesLivePortraitServicesAndCompleteCallerTail(int? portrait, bool skip, uint seed)
    {
        var session = StartFieldText("Z{W1}", interaction: true, startEvent: false, configure: document =>
        {
            document["battle"]!["start"]!["mainSeed"] = seed;
            var world = document["world"]!;
            AddVisuals(document, portrait);
            if (portrait is not null) world["presentation"]!["portraits"]![0]!["portrait"] = portrait;
            var map = world["maps"]![0]!;
            map["layout"]![2]![1] = 0x1400;
            map["events"] = JsonNode.Parse("""[{"kind":"source-zone","x":1,"y":2,"marker":5120,"requiredFlag":null,"requiredValue":true,"program":{"program":"invitation","instruction":0},"actions":[{"op":"idle"}]}]""");
            var program = world["programs"]![0]!;
            program["instructions"] = JsonNode.Parse("""[{"op":"branch-flag","flag":73,"whenSet":true,"target":{"program":"zone-end","instruction":0}},{"op":"open-portrait","entity":"ferryman","flags":0},{"op":"text-cursor","text":100},{"op":"show-text","mode":"single","speaker":"ferryman","explicitWindows":true},{"op":"end"}]""");
            world["programs"]!.AsArray().Add(JsonNode.Parse("""{"id":"zone-end","entitiesRunning":false,"instructions":[{"op":"end"}]}"""));
            if (skip) document["start"]!["flags"] = new JsonArray(73);
        });
        var facing = session.Current.Exploration!.Entities[new("ferryman")].Motion.Facing;
        Accept(session, new Move(Sf2.Remake.Domain.Maps.ExplorationDirection.South));
        var entry = Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.IsType<ZoneEventContext>(session.Current.Story.EventCaller);
        Assert.True(session.Current.Story.EntityServices);
        Assert.Null(session.Current.Story.EntityEvent);
        Assert.True(session.Current.Exploration!.PlayerEntity.Motion.IsMoving);
        if (!skip)
        {
            while (session.Current.Story.Wait is PortraitMovementWait)
                Accept(session, new AdvanceSimulation(session.Current.Story.Wait.Token));
            DrainTextWork(session);
            if (portrait is not null) Assert.Equal(portrait, Assert.IsType<OpenPortraitWindow>(session.Current.Story.PortraitWindow).Portrait);
            Accept(session, new CompleteTextReveal(session.Current.Story.Wait!.Token));
            var before = session.Current;
            var poll = Accept(session, new WaitForText(before.Story.Wait!.Token));
            Assert.Equal(before.Story.SimulationTick + 1, poll.Snapshot.Story.SimulationTick);
            Assert.NotNull(poll.Snapshot.Story.RandomSeedCopy);
            Accept(session, new Acknowledge(session.Current.Story.Wait!.Token));
        }
        var kinds = new List<string>();
        for (int i = 0; session.Current.Story.Wait is not null && i < 100; i++)
        {
            var result = Accept(session, new AdvanceSimulation(session.Current.Story.Wait.Token));
            kinds.AddRange(result.Observations.Select(x => x.Kind));
        }
        Assert.Null(session.Current.Story.EventCaller);
        Assert.True(session.Current.CanWaitAtInput);
        Assert.IsType<ClosedTextWindow>(session.Current.Story.TextWindow);
        Assert.IsType<ClosedPortraitWindow>(session.Current.Story.PortraitWindow);
        Assert.Equal(facing, session.Current.Exploration!.Entities[new("ferryman")].Motion.Facing);
        Assert.True(session.Current.Story.EntityServices);
        Assert.Contains("zone-return-service", kinds);
        Assert.Contains("zone-finished", kinds);
        if (portrait is not null && !skip)
            Assert.True(kinds.IndexOf("portrait-closed") < kinds.IndexOf("zone-arrival-wait"));
    }

    [Theory]
    [InlineData(false, ChoiceInput.ConfirmC, true, 89)]
    [InlineData(true, ChoiceInput.ConfirmA, true, 731)]
    [InlineData(false, ChoiceInput.Cancel, false, 19)]
    [InlineData(true, ChoiceInput.Cancel | ChoiceInput.ConfirmA, false, 65000)]
    public void BoundChoiceDeletesThenWritesGenericFlagThenReturnsAfterTenServices(bool held, ChoiceInput input, bool yes, int flag)
    {
        var session = StartChoice(flag);
        var entry = session.Current;
        var token = entry.Story.Wait!.Token;
        Assert.NotNull(Send(session, new ChooseDialogue(token, true)).Failure);
        Accept(session, new ChoiceHeldInput(token, held));
        Accept(session, new AdvanceSimulation(token, 600));
        Assert.Equal(5, session.Current.Story.SimulationTick - entry.Story.SimulationTick);
        if (held)
        {
            Assert.Equal(ChoicePhase.Release, Assert.IsType<ChoiceWait>(session.Current.Story.Wait).Work!.Phase);
            Assert.NotNull(Send(session, new PollChoice(token, input)).Failure);
            Assert.NotNull(Send(session, new AdvanceSimulation(token)).Failure);
            Accept(session, new ChoiceHeldInput(token, true));
            Accept(session, new ChoiceHeldInput(token, false));
        }
        Assert.True(session.Current.CanWaitForChoice);
        Assert.NotNull(Send(session, new PollChoice(new(token.Value + 1))).Failure);
        Assert.NotNull(Send(session, new WaitAtInput()).Failure);
        var before = session.Current;
        var accepted = Accept(session, new PollChoice(token, input));
        Assert.Equal(before.Story.SimulationTick, session.Current.Story.SimulationTick);
        Assert.DoesNotContain(accepted.Observations, row => row.Kind is "choice-poll-service" or "choice-result-flag");
        Accept(session, new AdvanceSimulation(token, 4));
        Assert.DoesNotContain(flag, session.Current.Story.Flags);
        Assert.True(Assert.IsType<ChoiceWait>(session.Current.Story.Wait).Work!.Visible);
        var deleted = Accept(session, new AdvanceSimulation(token));
        Assert.Equal(new[] { "choice-mandatory-service", "choice-deleted", "choice-result-flag" }, deleted.Observations.Select(row => row.Kind));
        Assert.Equal(yes, session.Current.Story.Flags.Contains(flag));
        Assert.False(Assert.IsType<ChoiceWait>(session.Current.Story.Wait).Work!.Visible);
        Accept(session, new AdvanceSimulation(token, 9));
        Assert.Equal(token, session.Current.Story.Wait!.Token);
        var returned = Accept(session, new AdvanceSimulation(token, 600));
        Assert.Contains(returned.Observations, row => row.Kind == "choice-returned");
        Assert.Equal(20, session.Current.Story.SimulationTick - entry.Story.SimulationTick);
        Assert.NotEqual(token, session.Current.Story.Wait!.Token);
        Assert.Equal(ChoicePhase.Entry, Assert.IsType<ChoiceWait>(session.Current.Story.Wait).Work!.Phase);
        Assert.NotNull(Send(session, new PollChoice(token, ChoiceInput.Cancel)).Failure);
    }

    [Theory]
    [InlineData(ChoiceInput.Right | ChoiceInput.Cancel, false)]
    [InlineData(ChoiceInput.Left | ChoiceInput.Right | ChoiceInput.ConfirmC, true)]
    [InlineData(ChoiceInput.Up | ChoiceInput.Down, true)]
    public void ChoicePollUsesConditionedPrecedenceReselectionAndOneTail(ChoiceInput mask, bool selected)
    {
        var session = StartChoice(421);
        var token = session.Current.Story.Wait!.Token;
        Accept(session, new ChoiceHeldInput(token, false));
        Accept(session, new AdvanceSimulation(token, 5));
        long tick = session.Current.Story.SimulationTick;
        var poll = Accept(session, new PollChoice(token, mask));
        var work = Assert.IsType<ChoiceWait>(session.Current.Story.Wait).Work!;
        Assert.Equal(selected, work.Yes);
        Assert.Equal(ChoicePhase.Input, work.Phase);
        Assert.Equal(tick + 1, session.Current.Story.SimulationTick);
        Assert.Single(poll.Observations, row => row.Kind == "choice-poll-service");
        if ((mask & (ChoiceInput.Left | ChoiceInput.Right)) != 0)
        {
            Assert.Equal(18, work.Counter);
            Assert.Single(poll.Observations, row => row.Kind == "choice-sound" && row.Detail == "66");
            var again = Accept(session, new PollChoice(token, selected ? ChoiceInput.Left : ChoiceInput.Right));
            Assert.Single(again.Observations, row => row.Kind == "choice-sound" && row.Detail == "66");
        }
        else Assert.Equal(14, work.Counter);
        var accepted = Accept(session, new PollChoice(token, ChoiceInput.ConfirmA));
        Assert.Equal(selected, Assert.IsType<ChoiceWait>(session.Current.Story.Wait).Work!.Yes);
        Assert.DoesNotContain(accepted.Observations, row => row.Kind == "choice-poll-service");
    }

    [Theory]
    [InlineData(false, 0xC632A55Au)]
    [InlineData(true, 0x1234BEEFu)]
    public void ChoiceServicesPreserveLiveEntityPortraitViewTypewritingAndCopy(bool enabled, uint seed)
    {
        var session = StartChoice(99, enabled, seed);
        var entry = session.Current;
        var token = entry.Story.Wait!.Token;
        var story = entry.Story.Copy(entry.Story.Cursor, entry.Story.Wait, entityServices: enabled,
            eventCaller: new ZoneEventContext(), randomSeedCopy: 0xA9, typewriting: true,
            logicalView: ExplorationViewRunner.SetDestination(entry.Story.LogicalView!, new(5, 6)),
            logicalText: new(Open: true, AnimationLength: 9),
            portraitWindow: new OpenPortraitWindow(7, 0, new(Blink: 1, Mouth: 1, Registered: true, Movement: 0, Moving: true)));
        var opened = ExplorationDispatcher.Submit(session.Definition, entry.WithStory(story), new ChoiceHeldInput(token, false));
        Assert.Null(opened.Failure);
        Assert.Equal(2, opened.Observations.Count(row => row.Kind == "choice-sound" && row.Detail == "65"));
        // A longer represented dialogue movement remains part of the global predicate.
        var longer = opened.Snapshot.Story.Copy(opened.Snapshot.Story.Cursor, opened.Snapshot.Story.Wait,
            logicalText: new(Open: true, AnimationLength: 8));
        var first = ExplorationDispatcher.Submit(session.Definition, opened.Snapshot.WithStory(longer), new AdvanceSimulation(token));
        Assert.Null(first.Failure);
        var entitySeed = enabled ? EntityActionRunner.Tick(entry.Exploration!, storyFlags: story.Flags).World.Party.MainSeed : seed;
        var blink = Sf2.Remake.Domain.Battles.BattleRandom.NextMain(entitySeed, 120);
        var mouth = Sf2.Remake.Domain.Battles.BattleRandom.NextMain(blink.After, 5);
        Assert.Equal(mouth.After, first.Snapshot.Exploration!.Party.MainSeed);
        Assert.Equal((byte)0xA9, first.Snapshot.Story.RandomSeedCopy);
        Assert.True(first.Snapshot.Story.Typewriting);
        Assert.True(first.Snapshot.Story.LogicalView!.HideWindows);
        Assert.Null(first.Snapshot.Story.LogicalView.TargetSlot);
        var fifth = ExplorationDispatcher.Submit(session.Definition, first.Snapshot, new AdvanceSimulation(token, 4));
        Assert.Equal(ChoicePhase.Opening, Assert.IsType<ChoiceWait>(fifth.Snapshot.Story.Wait).Work!.Phase);
        var finish = ExplorationDispatcher.Submit(session.Definition, fifth.Snapshot, new AdvanceSimulation(token, 600));
        Assert.Equal(ChoicePhase.Input, Assert.IsType<ChoiceWait>(finish.Snapshot.Story.Wait).Work!.Phase);
        Assert.Equal(9, finish.Snapshot.Story.SimulationTick - entry.Story.SimulationTick);
        Assert.Equal((byte)0xA9, finish.Snapshot.Story.RandomSeedCopy);
        Assert.Equal(enabled, finish.Snapshot.Story.EntityServices);
    }

    [Fact]
    public void ChoiceAnimationFollowsLogicalPollsAndCancelClearsAnExistingFlag()
    {
        var session = StartChoice(307);
        var token = session.Current.Story.Wait!.Token;
        Accept(session, new ChoiceHeldInput(token, false));
        Accept(session, new AdvanceSimulation(token, 5));
        var seed = session.Current.Exploration!.Party.MainSeed;
        foreach (int counter in Enumerable.Range(1, 15).Reverse())
        {
            Accept(session, new PollChoice(token));
            var work = Assert.IsType<ChoiceWait>(session.Current.Story.Wait).Work!;
            Assert.Equal(counter >= 10, work.YesAlternate);
            Assert.False(work.NoAlternate);
            Assert.Equal(counter == 1 ? 20 : counter - 1, work.Counter);
        }
        Assert.Equal(seed, session.Current.Exploration!.Party.MainSeed);
        Accept(session, new PollChoice(token, ChoiceInput.Right));
        var selected = Assert.IsType<ChoiceWait>(session.Current.Story.Wait).Work!;
        Assert.False(selected.YesAlternate);
        Assert.True(selected.NoAlternate);
        var entry = session.Current;
        var flagged = entry.WithStory(entry.Story.Copy(entry.Story.Cursor, entry.Story.Wait, flags: [307]));
        var closing = ExplorationDispatcher.Submit(session.Definition, flagged, new PollChoice(token, ChoiceInput.Cancel));
        Assert.Contains(307, closing.Snapshot.Story.Flags);
        var deleting = ExplorationDispatcher.Submit(session.Definition, closing.Snapshot, new AdvanceSimulation(token, 5));
        Assert.DoesNotContain(307, deleting.Snapshot.Story.Flags);
        Assert.Equal(ChoicePhase.ReturnDelay, Assert.IsType<ChoiceWait>(deleting.Snapshot.Story.Wait).Work!.Phase);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ChoiceWindowWaitIncludesPostScrollFixAfterItsFourthMovementPass(bool closing, bool portrait)
    {
        var session = StartChoice(307);
        var current = session.Current;
        var token = current.Story.Wait!.Token;
        var story = current.Story.Copy(current.Story.Cursor, current.Story.Wait,
            logicalText: new(Open: true), eventCaller: new ZoneEventContext(),
            portraitWindow: portrait ? new OpenPortraitWindow(7, 0,
                new(Blink: 100, Mouth: 100, Registered: true, Movement: 4, Moving: false)) : new ClosedPortraitWindow());
        current = ExplorationDispatcher.Submit(session.Definition, current.WithStory(story), new ChoiceHeldInput(token, false)).Snapshot;
        if (closing)
        {
            current = ExplorationDispatcher.Submit(session.Definition, current, new AdvanceSimulation(token, 5)).Snapshot;
            current = ExplorationDispatcher.Submit(session.Definition, current, new PollChoice(token, ChoiceInput.Cancel)).Snapshot;
        }
        var view = current.Story.LogicalView!;
        // A valid held tile destination with three 24-unit scroll passes left.
        // Hide is true on passes1..3, then false on4, when own movement reaches its destination.
        view = view with { TargetSlot = null, FollowCounter = 0,
            AX = view.AX with { Position = view.AX.Position + 312, Destination = view.AX.Position + 384 },
            BX = view.BX with { Position = view.BX.Position + 312, Destination = view.BX.Position + 384 } };
        current = current.WithStory(current.Story.Copy(current.Story.Cursor, current.Story.Wait, logicalView: view));
        long entryTick = current.Story.SimulationTick;
        var fifth = ExplorationDispatcher.Submit(session.Definition, current, new AdvanceSimulation(token, 5));
        Assert.Null(fifth.Failure);
        Assert.Equal(closing ? ChoicePhase.Closing : ChoicePhase.Opening,
            Assert.IsType<ChoiceWait>(fifth.Snapshot.Story.Wait).Work!.Phase);
        Assert.Equal(entryTick + 5, fifth.Snapshot.Story.SimulationTick);
        var sixth = ExplorationDispatcher.Submit(session.Definition, fifth.Snapshot, new AdvanceSimulation(token));
        Assert.Null(sixth.Failure);
        Assert.Equal(closing ? ChoicePhase.ReturnDelay : ChoicePhase.Input,
            Assert.IsType<ChoiceWait>(sixth.Snapshot.Story.Wait).Work!.Phase);
        Assert.Equal(entryTick + 6, sixth.Snapshot.Story.SimulationTick);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PostScrollFixUsesGeometryAndRetainsPrepassBusyBeforePortraitRng(bool displaced)
    {
        var session = StartChoice(307);
        var entry = session.Current;
        var wait = (ChoiceWait)entry.Story.Wait!;
        var view = entry.Story.LogicalView! with { TargetSlot = null };
        var story = entry.Story.Copy(entry.Story.Cursor, wait with { Work = new(Phase: ChoicePhase.Input,
                OriginX: displaced ? 32 : 12, DestinationX: 12, Movement: 2, Length: 9) },
            windowFixPending: true, logicalView: view, typewriting: true, entityServices: false,
            logicalText: new(Open: true, AnimationLength: 9, AnimationCounter: 2, OriginY: displaced ? 29 : 19),
            portraitWindow: new OpenPortraitWindow(7, 0, new(Blink: 1, Mouth: 1, Registered: true,
                Movement: 2, Length: 9, OriginY: displaced ? -10 : 1)));
        var observations = new List<SessionObservation>();
        var current = ExplorationDispatcher.Service(session.Definition, entry.WithStory(story), observations, "window-service");
        var work = Assert.IsType<ChoiceWait>(current.Story.Wait).Work!;
        var portrait = Assert.IsType<OpenPortraitWindow>(current.Story.PortraitWindow).Work!;
        Assert.False(current.Story.WindowFixPending);
        Assert.True(work.Moving);
        Assert.True(portrait.Moving);
        Assert.True(current.Story.LogicalText!.Moving);
        Assert.Equal(displaced ? 3 : 0, work.Movement);
        Assert.Equal(displaced ? 3 : 0, portrait.Movement);
        Assert.Equal(displaced ? 3 : 0, current.Story.LogicalText.AnimationCounter);
        Assert.Equal(displaced ? 9 : 1, work.Length);
        Assert.Equal(displaced ? 9 : 1, portrait.Length);
        Assert.Equal(displaced ? 9 : 1, current.Story.LogicalText.AnimationLength);
        var blink = Sf2.Remake.Domain.Battles.BattleRandom.NextMain(entry.Exploration!.Party.MainSeed, 120);
        var mouth = Sf2.Remake.Domain.Battles.BattleRandom.NextMain(blink.After, 5);
        Assert.Equal(new[] { "window-service", "rng-portrait-blink", "rng-portrait-mouth" }, observations.Select(row => row.Kind));
        Assert.Equal(mouth.After, current.Exploration!.Party.MainSeed);
        Assert.Equal(blink.Value + 30, portrait.Blink);
        Assert.Equal(mouth.Value + 10, portrait.Mouth);
        if (displaced) return;
        current = ExplorationDispatcher.Service(session.Definition, current, observations, "window-service");
        Assert.True(ExplorationTextRunner.WindowsMoving(current.Story));
        Assert.Equal(1, Assert.IsType<OpenPortraitWindow>(current.Story.PortraitWindow).Work!.Movement);
        current = ExplorationDispatcher.Service(session.Definition, current, observations, "window-service");
        Assert.False(ExplorationTextRunner.WindowsMoving(current.Story));
    }

    [Fact]
    public void PendingWindowFixSurvivesEmptyPassAndMapInitializationAndRepeatedScrolls()
    {
        var session = StartChoice(307);
        var current = session.Current;
        var token = current.Story.Wait!.Token;
        var story = current.Story.Copy(current.Story.Cursor, new TickWait(token, 100), windowFixPending: true);
        story = ExplorationTextRunner.Initialize(current.Exploration!, story);
        Assert.True(story.WindowFixPending);
        var observations = new List<SessionObservation>();
        current = ExplorationDispatcher.Service(session.Definition, current.WithStory(story), observations, "window-service");
        Assert.True(current.Story.WindowFixPending);
        story = current.Story.Copy(current.Story.Cursor, current.Story.Wait,
            logicalText: new(Open: true), logicalView: current.Story.LogicalView! with { TargetSlot = null });
        current = ExplorationDispatcher.Service(session.Definition, current.WithStory(story), observations, "window-service");
        Assert.False(current.Story.WindowFixPending);
        Assert.False(current.Story.LogicalText!.Moving); // postpass does not invent a busy bit
        Assert.Equal(0, current.Story.LogicalText.AnimationCounter);
        Assert.Equal(1, current.Story.LogicalText.AnimationLength);
        for (int cycle = 0; cycle < 2; cycle++)
        {
            var view = current.Story.LogicalView!;
            view = view with { AX = view.AX with { Destination = view.AX.Position + 24 } };
            current = current.WithStory(current.Story.Copy(current.Story.Cursor, current.Story.Wait, logicalView: view));
            current = ExplorationDispatcher.Service(session.Definition, current, observations, "window-service");
            Assert.True(current.Story.WindowFixPending);
            Assert.True(current.Story.LogicalView!.HideWindows);
            current = ExplorationDispatcher.Service(session.Definition, current, observations, "window-service");
            Assert.False(current.Story.WindowFixPending);
            Assert.False(current.Story.LogicalView!.HideWindows);
            Assert.Equal(0, current.Story.LogicalText!.AnimationCounter);
            Assert.Equal(19, current.Story.LogicalText.WindowY);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TextAndPortraitCloseWaitForAllRepresentedWindows(bool portraitClose)
    {
        var session = StartChoice(307);
        var current = session.Current;
        var token = current.Story.Wait!.Token;
        var story = current.Story.Copy(current.Story.Cursor,
            portraitClose ? new PortraitMovementWait(token, true, false) : new TextCloseWait(token), eventCaller: new ZoneEventContext(),
            logicalView: current.Story.LogicalView! with { TargetSlot = null },
            logicalText: portraitClose ? new(Open: true, AnimationLength: 12) : ExplorationTextRunner.Close(new(Open: true)),
            portraitWindow: new OpenPortraitWindow(7, 0, portraitClose
                ? new(Registered: false, Closing: true, OriginY: 1, DestinationY: -10)
                : new(Registered: true, Blink: 100, Mouth: 100, OriginY: 1, Length: 12)));
        current = current.WithStory(story);
        var busy = ExplorationDispatcher.Submit(session.Definition, current, new AdvanceSimulation(token, 12));
        Assert.Null(busy.Failure);
        Assert.Equal(token, busy.Snapshot.Story.Wait!.Token);
        var done = ExplorationDispatcher.Submit(session.Definition, busy.Snapshot, new AdvanceSimulation(token));
        Assert.Null(done.Failure);
        Assert.NotEqual(token, done.Snapshot.Story.Wait!.Token);
        Assert.Equal(current.Story.SimulationTick + 13, done.Snapshot.Story.SimulationTick);
        if (portraitClose) Assert.IsType<ClosedPortraitWindow>(done.Snapshot.Story.PortraitWindow);
        else Assert.False(done.Snapshot.Story.LogicalText!.Open);
    }

    [Theory]
    [InlineData("A{NAME;1}B", 0, 6, "N", false, false)]
    [InlineData("A{N}{NAME;1}{N}BC{N}D", 3, 16, "Long name", true, false)]
    [InlineData("{NAME;1} AB", 1, 8, "Other", true, true)]
    public void RawFieldTextCompletesMandatoryWorkAndRevealWithoutImplicitAcknowledgement(
        string text, int speed, int width, string name, bool enabled, bool speaker)
    {
        var early = Raw();
        var late = Raw();
        var token = early.Current.Story.Wait!.Token;
        Accept(early, new CompleteTextReveal(token));
        Assert.IsType<FieldTextWait>(early.Current.Story.Wait);
        Accept(early, new AdvanceSimulation(token, 600));
        Assert.IsType<ChoiceWait>(early.Current.Story.Wait);
        DrainTextWork(late);
        var completed = late.Current;
        var end = Assert.IsType<FieldTextWait>(completed.Story.Wait);
        Assert.Equal(FieldTextPhase.End, end.Phase);
        Assert.False(completed.CanWaitForText);
        Assert.False(completed.Story.Typewriting);
        Assert.Equal(text.Replace("{NAME;1}", name).Replace("{N}", "\n"), end.Projection);
        foreach (var command in new SessionCommand[] { new Acknowledge(end.Token), new WaitForText(end.Token), new AdvanceSimulation(end.Token) })
        {
            Assert.NotNull(Send(late, command).Failure);
            Assert.Same(completed, late.Current);
        }
        var returned = Accept(late, new CompleteTextReveal(end.Token));
        Assert.IsType<ChoiceWait>(late.Current.Story.Wait);
        Assert.Equal(completed.Story.SimulationTick, late.Current.Story.SimulationTick);
        Assert.Equal(completed.Exploration!.Party.MainSeed, late.Current.Exploration!.Party.MainSeed);
        Assert.Null(late.Current.Story.RandomSeedCopy);
        Assert.True(late.Current.Story.LogicalText!.Open);
        Assert.Equal(speaker ? new EntityRef("ferryman") : (EntityRef?)null,
            Assert.IsType<OpenTextWindow>(late.Current.Story.TextWindow).Speaker);
        Assert.DoesNotContain(returned.Observations, row => row.Kind.StartsWith("rng-"));
        Assert.Equal(early.Current.Story.SimulationTick, late.Current.Story.SimulationTick);
        Assert.Equal(early.Current.Exploration!.Party.MainSeed, late.Current.Exploration.Party.MainSeed);
        Assert.Equal(JsonSerializer.Serialize(early.Current.Exploration.AllEntities), JsonSerializer.Serialize(late.Current.Exploration.AllEntities));
        Assert.Equal(early.Current.Story.LogicalText, late.Current.Story.LogicalText);

        GameSession Raw() => StartFieldText(text, speed: speed, width: width, name: name, enabled: enabled, npcRandom: true,
            configure: document => RawInstructions(document, speaker));
    }

    [Fact]
    public void RawWrapperRetainsItsActualW1AndW2Consumers()
    {
        var session = StartFieldText("A{W1}B{W2}C", configure: document => RawInstructions(document, false));
        foreach (bool wait2 in new[] { false, true })
        {
            DrainTextWork(session);
            var wait = Assert.IsType<FieldTextWait>(session.Current.Story.Wait);
            Assert.Equal(FieldTextPhase.Input, wait.Phase);
            Assert.Equal(wait2, wait.Wait2);
            Assert.NotNull(Send(session, new Acknowledge(wait.Token)).Failure);
            Accept(session, new CompleteTextReveal(wait.Token));
            Assert.True(session.Current.CanWaitForText);
            long before = session.Current.Story.SimulationTick;
            var polled = Accept(session, new WaitForText(wait.Token));
            Assert.Equal(before + 1, session.Current.Story.SimulationTick);
            Assert.Single(polled.Observations, row => row.Kind == (wait2 ? "rng-text-w2" : "rng-text-w1"));
            var accepted = Accept(session, new Acknowledge(wait.Token));
            Assert.Single(accepted.Observations, row => row.Kind == (wait2 ? "rng-text-w2" : "rng-text-w1"));
            Assert.DoesNotContain(accepted.Observations, row => row.Kind == (wait2 ? "rng-text-w1" : "rng-text-w2"));
        }
        DrainTextWork(session);
        var tail = Assert.IsType<FieldTextWait>(session.Current.Story.Wait);
        Assert.Equal(FieldTextPhase.End, tail.Phase);
        var beforeReturn = session.Current;
        Accept(session, new CompleteTextReveal(tail.Token));
        Assert.IsType<ChoiceWait>(session.Current.Story.Wait);
        Assert.Equal(beforeReturn.Story.SimulationTick, session.Current.Story.SimulationTick);
        Assert.Equal(beforeReturn.Story.RandomSeedCopy, session.Current.Story.RandomSeedCopy);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void RawSpeakerAbsenceDoesNotHideAnInvalidSpeaker(bool eventSpeaker, bool explicitInvalid)
    {
        var session = StartFieldText("A");
        var show = new ShowText(TextDisplayMode.Single, explicitInvalid ? new("absent") : null,
            UseEventSpeaker: eventSpeaker, WaitForAcknowledgement: !eventSpeaker && !explicitInvalid, ExplicitWindows: true);
        var error = Assert.Throws<Sf2.Remake.Domain.Battles.BattleRuleException>(() =>
            ExplorationTextRunner.Begin(session.Definition.Exploration!, session.Current, show, new(998)));
        Assert.Equal("field-text-speaker", error.Code);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SpeakerlessRawTextRetainsRegisteredPortraitAndLiveServices(bool enabled)
    {
        var session = StartFieldText("ABC", enabled: enabled, npcRandom: true, configure: document => RawInstructions(document, false));
        var current = session.Current;
        var story = current.Story.Copy(current.Story.Cursor, current.Story.Wait, eventCaller: new ZoneEventContext(),
            entityServices: enabled, windowFixPending: true,
            logicalView: current.Story.LogicalView! with { TargetSlot = null }, randomSeedCopy: 0xA9,
            portraitWindow: new OpenPortraitWindow(7, 0, new(Blink: 1, Mouth: 0, Registered: true, Movement: 4, Moving: false)));
        var result = ExplorationDispatcher.Submit(session.Definition, current.WithStory(story), new AdvanceSimulation(story.Wait!.Token));
        Assert.Null(result.Failure);
        var portrait = Assert.IsType<OpenPortraitWindow>(result.Snapshot.Story.PortraitWindow).Work!;
        Assert.True(portrait.Registered);
        Assert.Equal(0, portrait.Movement);
        Assert.Equal(1, portrait.Length);
        Assert.Equal(1, portrait.Y);
        Assert.False(result.Snapshot.Story.WindowFixPending);
        Assert.Equal((byte)0xA9, result.Snapshot.Story.RandomSeedCopy);
        uint entitySeed = enabled ? EntityActionRunner.Tick(current.Exploration!, storyFlags: story.Flags).World.Party.MainSeed : current.Exploration!.Party.MainSeed;
        var blink = Sf2.Remake.Domain.Battles.BattleRandom.NextMain(entitySeed, 120);
        var mouth = Sf2.Remake.Domain.Battles.BattleRandom.NextMain(blink.After, 5);
        Assert.Equal(mouth.After, result.Snapshot.Exploration!.Party.MainSeed);
        Assert.Equal(new[] { "text-mandatory-service", "rng-portrait-blink", "rng-portrait-mouth", "text-work-advanced" }, result.Observations.Select(row => row.Kind));
        Assert.Null(Assert.IsType<OpenTextWindow>(result.Snapshot.Story.TextWindow).Speaker);
    }

    [Fact]
    public void BoundMusicWaitStopsWithTheCompletedRawDisplayPreserved()
    {
        var session = StartFieldText("AB", enabled: true, npcRandom: true, configure: document =>
        {
            RawInstructions(document, false);
            document["world"]!["programs"]![0]!["instructions"]![2] = JsonNode.Parse("""
                {"op":"present","kind":"SoundWait","resource":null,"entity":null,"position":null}
                """);
        });
        DrainTextWork(session);
        var done = session.Current;
        var result = Send(session, new CompleteTextReveal(done.Story.Wait!.Token));
        Assert.Equal("field-music-progress-unbound", result.Failure!.Code);
        Assert.Equal(SessionFailureKind.UnsupportedCapability, result.Failure.Kind);
        Assert.Equal(SessionStopReason.Unsupported, result.StopReason);
        Assert.Null(result.Snapshot.Story.Wait);
        Assert.Equal(2, result.Snapshot.Story.Cursor!.Value.Instruction);
        Assert.Equal(done.Story.TextWindow, result.Snapshot.Story.TextWindow);
        Assert.Equal(done.Story.LogicalText, result.Snapshot.Story.LogicalText);
        Assert.Equal(done.Story.TextCursor, result.Snapshot.Story.TextCursor);
        Assert.Equal(done.Story.SimulationTick, result.Snapshot.Story.SimulationTick);
        Assert.Equal(done.Exploration!.Party.MainSeed, result.Snapshot.Exploration!.Party.MainSeed);
        Assert.Equal(done.Story.RandomSeedCopy, result.Snapshot.Story.RandomSeedCopy);
        Assert.DoesNotContain(result.Observations, row => row.Detail == nameof(PresentationCueKind.PreviousMusic));
        var after = ExplorationDispatcher.Submit(session.Definition, result.Snapshot, new AdvanceSimulation());
        Assert.Equal("session-stopped", after.Failure!.Code);
        Assert.Equal(result.Snapshot.Story.SimulationTick, after.Snapshot.Story.SimulationTick);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(255)]
    public void MapInitializationCameraRetainsItsContinuationUntilTheNestedScriptReturns(int layer)
    {
        var session = StartFieldText("A{W1}", configure: document =>
        {
            var area = document["world"]!["maps"]![0]!["areas"]![0]!["view"]!;
            area["layer"] = layer;
            if (layer == 255) area["foregroundY"] = 0;
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse("""
                [{"op":"transfer","map":"quay","position":{"x":1,"y":1},"facing":0,"loadMode":"rebuild"},{"op":"end"}]
                """);
            document["world"]!["maps"]![0]!["onLoad"] = JsonNode.Parse("""{"program":"initialize","instruction":0}""");
            var programs = document["world"]!["programs"]!.AsArray();
            programs.Add(JsonNode.Parse("""
                {"id":"initialize","instructions":[{"op":"call","target":{"program":"scene","instruction":0},"activateEntities":true},
                {"op":"set-flag","flag":605,"value":true},{"op":"end"}]}
                """));
            programs.Add(JsonNode.Parse("""
                {"id":"scene","instructions":[{"op":"camera-target","position":{"x":3,"y":3}},
                {"op":"wait-view"},{"op":"end-map-script"}]}
                """));
        });
        Assert.Equal(SessionStopReason.SimulationWait, session.Current.StopReason);
        Assert.IsType<ViewWait>(session.Current.Story.Wait);
        Assert.Equal(ProgramContinuation.MapLoaded, session.Current.Story.Continuation);
        Assert.Null(session.Current.Story.EventCaller);
        Assert.Equal(new[] { "invitation", "initialize" }, session.Current.Story.Callers.Select(x => x.Program));
        Assert.DoesNotContain(605, session.Current.Story.Flags);
        for (int i = 0; session.Current.Story.Wait is ViewWait && i < 100; i++)
        {
            Assert.Equal(ProgramContinuation.MapLoaded, session.Current.Story.Continuation);
            Assert.Null(session.Current.Story.EventCaller);
            if (layer == 255)
            {
                Assert.Equal(session.Current.Story.LogicalView!.AX, session.Current.Story.LogicalView.BX);
                Assert.Equal(session.Current.Story.LogicalView.AY, session.Current.Story.LogicalView.BY);
            }
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait.Token));
        }
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Equal(ProgramContinuation.FieldInput, session.Current.Story.Continuation);
        Assert.Contains(605, session.Current.Story.Flags);
        Assert.Empty(session.Current.Story.Callers);
        Assert.Null(session.Current.Story.Cursor);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 2)]
    public void MapInitializationRegistersPortraitAndServicesTextAcrossNestedReturns(bool closeInChild, int speed)
    {
        var session = StartFieldText("AB{W1}", speed: speed, configure: document =>
        {
            document["world"]!["presentation"]!["sprites"]![0]!["portrait"] = 7;
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse("""
                [{"op":"transfer","map":"quay","position":{"x":1,"y":1},"facing":0,"loadMode":"rebuild"},{"op":"end"}]
                """);
            document["world"]!["maps"]![0]!["onLoad"] = JsonNode.Parse("""{"program":"initialize","instruction":0}""");
            var programs = document["world"]!["programs"]!.AsArray();
            programs.Add(JsonNode.Parse("""
                {"id":"initialize","instructions":[{"op":"call","target":{"program":"scene","instruction":0},"activateEntities":true},
                {"op":"close-portrait"},{"op":"close-text"},{"op":"set-flag","flag":605,"value":true},{"op":"end"}]}
                """));
            programs.Add(JsonNode.Parse($$"""
                {"id":"scene","instructions":[{"op":"open-portrait","entity":"ferryman","flags":192},
                {"op":"wait-view"},{"op":"text-cursor","text":100},
                {"op":"show-text","mode":"single","speaker":"ferryman","explicitWindows":true},
                {{(closeInChild ? "{\"op\":\"close-portrait\"},{\"op\":\"close-text\"}," : "")}}
                {"op":"end-map-script"}]}
                """));
        });
        Assert.IsType<PortraitMovementWait>(session.Current.Story.Wait);
        bool registered = false, input = false, closing = false, nestedReturn = false;
        for (int i = 0; session.Current.StopReason != SessionStopReason.PlayerInput && i < 300; i++)
        {
            var story = session.Current.Story;
            Assert.Equal(ProgramContinuation.MapLoaded, story.Continuation);
            Assert.Null(story.EventCaller);
            Assert.DoesNotContain(605, story.Flags);
            Assert.NotEmpty(story.Callers);
            registered |= story.PortraitWindow is OpenPortraitWindow { Work.Registered: true };
            closing |= story.Wait is PortraitMovementWait { Closing: true };
            nestedReturn |= story.Wait is ViewWait { ScriptReturn: true };
            if (story.Wait is FieldTextWait { LogicalDone: true } text)
            {
                Assert.True(Assert.IsType<OpenPortraitWindow>(story.PortraitWindow).Work!.Registered);
                Assert.False(session.Current.CanWaitForText);
                Assert.False(session.Current.CanWaitAtInput);
                Accept(session, new CompleteTextReveal(text.Token));
                Assert.True(session.Current.CanWaitForText);
                foreach (var invalid in new[] { ProgramContinuation.BeforeBattleFinished, ProgramContinuation.VictoryProgramFinished,
                    ProgramContinuation.DefeatProgramFinished, ProgramContinuation.OutcomeMapLoaded })
                {
                    var excluded = session.Current.WithStory(session.Current.Story.Copy(session.Current.Story.Cursor,
                        session.Current.Story.Wait, continuation: invalid));
                    Assert.False(excluded.CanWaitForText);
                    Assert.Equal("text-input-unavailable", ExplorationDispatcher.Submit(session.Definition, excluded,
                        new WaitForText(text.Token)).Failure!.Code);
                }
                Accept(session, new WaitForText(text.Token));
                Accept(session, new Acknowledge(text.Token));
                input = true;
            }
            else Accept(session, new AdvanceSimulation(story.Wait!.Token));
        }
        Assert.True(registered && input && closing);
        Assert.Equal(!closeInChild, nestedReturn);
        Assert.Equal(SessionStopReason.PlayerInput, session.Current.StopReason);
        Assert.Equal(ProgramContinuation.FieldInput, session.Current.Story.Continuation);
        Assert.Empty(session.Current.Story.Callers);
        Assert.Contains(605, session.Current.Story.Flags);
        Assert.IsType<ClosedPortraitWindow>(session.Current.Story.PortraitWindow);
        Assert.IsType<ClosedTextWindow>(session.Current.Story.TextWindow);
        Assert.False(session.Current.Story.LogicalText!.Open);
        Assert.Null(session.Current.Story.Cursor);
        Assert.Null(session.Current.Story.Wait);
    }

    [Fact]
    public void ForegroundMapFollowSelectsPlaneAAndPreservesCoincidentAxesDuringServices()
    {
        var session = StartFieldText("A{W1}", configure: document =>
        {
            var area = document["world"]!["maps"]![0]!["areas"]![0]!["view"]!;
            area["layer"] = 255;
            area["foregroundY"] = 0;
        });
        var world = session.Current.Exploration!;
        var view = session.Current.Story.LogicalView!;
        world = world.WithEntity(world.PlayerEntity with { Motion = world.PlayerEntity.Motion with { X = 2305, Y = 2305 } });
        var fromA = ExplorationViewRunner.Tick(world, view with { AX = new(384), AY = new(384) }, new(2, 0, 0));
        Assert.Equal(0, fromA.FollowCounter); // A is inside its deadband; B would request a destination.
        Assert.Null(fromA.BX.Destination);
        for (int i = 0; i < 25; i++)
        {
            view = ExplorationViewRunner.Tick(world, view, new(2, 0, 0));
            Assert.Equal(view.AX, view.BX);
            Assert.Equal(view.AY, view.BY);
        }
        Assert.Equal(384, view.AX.Position);
        Assert.Equal(384, view.AY.Position);
        Assert.False(view.Scrolling);
        var context = session.Current.Story;
        Assert.Throws<Sf2.Remake.Domain.Battles.BattleRuleException>(() => ExplorationViewRunner.Tick(world,
            view with { Area = view.Area with { ForegroundX = 1 } }, context.TextSettings!));
        Assert.Throws<Sf2.Remake.Domain.Battles.BattleRuleException>(() => ExplorationViewRunner.Tick(world,
            view with { Area = view.Area with { Layer = 1 } }, context.TextSettings!));
    }

    [Theory]
    [InlineData(255, true)]
    [InlineData(0, false)]
    public void ExplicitNarrationUsesSourceLookupSkipWithoutAdmittingAnInvalidSpeaker(int flags, bool accepted)
    {
        var session = StartFieldText("{LEADER}{W1}", configure: document =>
        {
            var instruction = document["world"]!["programs"]![0]!["instructions"]![1]!;
            instruction["speaker"] = null;
            instruction["speakerFlags"] = flags;
            document["world"]!["programs"]![0]!["instructions"]!.AsArray().Insert(1,
                JsonNode.Parse("""{"op":"wait-ticks","ticks":1}"""));
        });
        var result = Send(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        if (!accepted)
        {
            Assert.Equal("field-text-speaker", result.Failure!.Code);
            Assert.Equal(SessionStopReason.Unsupported, session.Current.StopReason);
            return;
        }
        Assert.Null(result.Failure);
        Assert.Equal(SessionStopReason.SimulationWait, session.Current.StopReason);
        DrainTextWork(session);
        var token = session.Current.Story.Wait!.Token;
        Assert.IsType<ClosedPortraitWindow>(session.Current.Story.PortraitWindow);
        Assert.Null(Assert.IsType<OpenTextWindow>(session.Current.Story.TextWindow).Speaker);
        Assert.Equal("Leader", Assert.IsType<FieldTextWait>(session.Current.Story.Wait).Projection);
        Accept(session, new CompleteTextReveal(token));
        Accept(session, new WaitForText(token));
        Accept(session, new Acknowledge(token));
        for (int i = 0; session.Current.Story.Wait is TextCloseWait && i < 20; i++)
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait.Token));
        Assert.True(session.Current.CanWaitAtInput);
    }

    [Theory]
    [InlineData(0, 22)]
    [InlineData(1, 0)]
    public void SourceDelayContributesNeutralServicesWithoutGlyphOrPollWork(int mouthControl, int pauseServices)
    {
        var session = StartFieldText("{D1}A{W1}", configure: document =>
            document["start"]!["textSettings"]!["mouthControl"] = mouthControl);
        Assert.Equal(SessionStopReason.SimulationWait, session.Current.StopReason);
        int pauses = 0, services = 0;
        while (session.Current.Story.Wait is FieldTextWait { LogicalDone: false } && services < 100)
        {
            var story = session.Current.Story;
            bool pause = story.Wait is FieldTextWait { Phase: FieldTextPhase.TextPause };
            if (pause)
            {
                Assert.False(story.Typewriting);
                Assert.True(((FieldTextWait)story.Wait!).SavedTypewriting);
                Assert.Equal((byte)2, story.LogicalText!.X);
                Assert.True(((FieldTextWait)story.Wait!).FirstGlyph);
                pauses++;
            }
            var result = Accept(session, new AdvanceSimulation(story.Wait!.Token));
            Assert.DoesNotContain(result.Observations, row => row.Kind.StartsWith("rng-text", StringComparison.Ordinal));
            Assert.Equal(story.RandomSeedCopy, session.Current.Story.RandomSeedCopy);
            services++;
        }
        Assert.Equal(pauseServices, pauses);
        Assert.Equal(12 + pauseServices, services); // Ten window passes, one cursor and speed2 delay1.
        Assert.Equal("A", Assert.IsType<FieldTextWait>(session.Current.Story.Wait).Projection);
        Assert.False(session.Current.Story.Typewriting);
    }

    [Fact]
    public void ConsecutiveAndPostInputDelaysMatchBatchedServicesWithLivePortraitRng()
    {
        (long Tick, uint Seed, byte? Copy, string[] Draws) Run(int batch)
        {
            var session = StartFieldText("{D1}{D1}{W2}A{D1}{W1}{D1}", interaction: true, configure: document =>
                document["world"]!["presentation"]!["sprites"]![0]!["portrait"] = 7);
            var draws = new List<string>();
            for (int i = 0; !session.Current.CanWaitAtInput && i < 1000; i++)
            {
                var wait = session.Current.Story.Wait!;
                SessionResult result;
                if (wait is FieldTextWait { LogicalDone: true } text)
                {
                    Accept(session, new CompleteTextReveal(text.Token));
                    if (text.Phase == FieldTextPhase.End) continue;
                    result = Accept(session, new WaitForText(text.Token));
                    draws.AddRange(result.Observations.Where(row => row.RandomRange is not null)
                        .Select(row => $"{row.Kind}:{row.Before}:{row.After}:{row.RandomRange}:{row.RandomValue}"));
                    result = Accept(session, new Acknowledge(text.Token));
                }
                else result = Accept(session, new AdvanceSimulation(wait.Token, batch));
                draws.AddRange(result.Observations.Where(row => row.RandomRange is not null)
                    .Select(row => $"{row.Kind}:{row.Before}:{row.After}:{row.RandomRange}:{row.RandomValue}"));
            }
            Assert.True(session.Current.CanWaitAtInput);
            Assert.IsType<ClosedPortraitWindow>(session.Current.Story.PortraitWindow);
            Assert.IsType<ClosedTextWindow>(session.Current.Story.TextWindow);
            Assert.NotEmpty(draws);
            return (session.Current.Story.SimulationTick, session.Current.Exploration!.Party.MainSeed,
                session.Current.Story.RandomSeedCopy, draws.ToArray());
        }
        var single = Run(1);
        var batched = Run(600);
        Assert.Equal(single.Tick, batched.Tick);
        Assert.Equal(single.Seed, batched.Seed);
        Assert.Equal(single.Copy, batched.Copy);
        Assert.Equal(single.Draws, batched.Draws);
    }

    [Fact]
    public void DelayRestoresTheActualSavedTypewritingValueBeforeTheNextGlyph()
    {
        var session = StartFieldText("{D1}A{W1}");
        for (int i = 0; session.Current.Story.Wait is not FieldTextWait { Phase: FieldTextPhase.TextPause } && i < 20; i++)
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        var story = session.Current.Story;
        var wait = Assert.IsType<FieldTextWait>(story.Wait);
        var paused = ExplorationTextRunner.Normalize(story.Copy(story.Cursor,
            wait with { Index = 0, Phase = FieldTextPhase.Tokens }, typewriting: false));
        var actual = Assert.IsType<FieldTextWait>(paused.Wait);
        Assert.False(actual.SavedTypewriting);
        Assert.True(actual.FirstGlyph);
        var restored = ExplorationTextRunner.AfterService(paused.Copy(paused.Cursor, actual with { Remaining = 1 }));
        Assert.False(restored.Typewriting);
        Assert.Equal(FieldTextPhase.GlyphCursor, Assert.IsType<FieldTextWait>(restored.Wait).Phase);
    }

    [Fact]
    public void SourceNarrationRetainsAnAlreadyRegisteredPortraitUntilExplicitClose()
    {
        var session = StartFieldText("{LEADER}{W1}", interaction: true, configure: document =>
        {
            document["world"]!["presentation"]!["sprites"]![0]!["portrait"] = 7;
            var text = document["world"]!["programs"]![0]!["instructions"]![1]!;
            text["speaker"] = null;
            text["speakerFlags"] = 255;
            document["world"]!["programs"]![0]!["instructions"]!.AsArray().Insert(2,
                JsonNode.Parse("""{"op":"close-portrait"}"""));
        });
        for (int i = 0; session.Current.Story.Wait is not FieldTextWait && i < 20; i++)
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        DrainTextWork(session);
        Assert.Null(Assert.IsType<OpenTextWindow>(session.Current.Story.TextWindow).Speaker);
        Assert.True(Assert.IsType<OpenPortraitWindow>(session.Current.Story.PortraitWindow).Work!.Registered);
        var token = session.Current.Story.Wait!.Token;
        Accept(session, new CompleteTextReveal(token));
        Accept(session, new Acknowledge(token));
        Assert.IsType<PortraitMovementWait>(session.Current.Story.Wait);
        Assert.False(Assert.IsType<OpenPortraitWindow>(session.Current.Story.PortraitWindow).Work!.Registered);
        for (int i = 0; !session.Current.CanWaitAtInput && i < 40; i++)
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        Assert.True(session.Current.CanWaitAtInput);
        Assert.IsType<ClosedPortraitWindow>(session.Current.Story.PortraitWindow);
    }

    private static void RawInstructions(JsonNode document, bool speaker)
    {
        var instructions = document["world"]!["programs"]![0]!["instructions"]!;
        instructions[1]!["speaker"] = speaker ? "ferryman" : null;
        instructions[1]!["waitForAcknowledgement"] = false;
        instructions[2] = JsonNode.Parse("""{"op":"yes-no","flag":307}""");
    }

    private static GameSession StartChoice(int flag, bool enabled = false, uint seed = 0x12341234) =>
        StartFieldText("{W1}", enabled: enabled, npcRandom: true, configure: document =>
        {
            document["battle"]!["start"]!["mainSeed"] = seed;
            document["world"]!["programs"]![0]!["instructions"] = JsonNode.Parse($$"""
                [{"op":"yes-no","flag":{{flag}}},{"op":"yes-no","flag":{{flag + 1}}},{"op":"end"}]
                """);
        });

    internal static void DrainTextWork(GameSession session)
    {
        for (int i = 0; session.Current.Story.Wait is FieldTextWait { LogicalDone: false } && i < 1000; i++)
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait.Token));
        Assert.True(Assert.IsType<FieldTextWait>(session.Current.Story.Wait).LogicalDone);
    }

    internal static GameSession StartFieldText(string text, int speed = 2, bool enabled = false,
        bool npcRandom = false, string name = "Name", int width = 6, string? second = null, bool viewWait = false, bool interaction = false, bool alternateLeader = false,
        Action<JsonNode>? configure = null, bool startEvent = true)
    {
        var session = Start("harbor-arrival", document =>
        {
            var world = document["world"]!;
            world["texts"]![0]!["text"] = text;
            world["texts"]![1]!["text"] = second ?? "B{W1}";
            world["memberNames"] = new JsonArray("Leader", name);
            if (alternateLeader)
            {
                world["partyFlags"] = JsonNode.Parse("""{"memberCount":2,"joinedStart":10,"activeStart":20,"capacity":1}""");
                document["start"]!["flags"] = new JsonArray(11, 21);
            }
            world["textFont"] = JsonSerializer.SerializeToNode(new { asciiToSymbol = Enumerable.Repeat(1, 256), advances = Enumerable.Repeat(width, 80) });
            foreach (var map in world["maps"]!.AsArray())
            {
                map!["layout"] = JsonSerializer.SerializeToNode(Enumerable.Range(0, 31).Select(_ => new int[31]));
                map["areas"] = JsonNode.Parse("""[{"minX":0,"minY":0,"maxX":30,"maxY":30,"view":{"foregroundX":0,"foregroundY":32,"backgroundX":0,"backgroundY":0,"parallaxAX":256,"parallaxAY":256,"parallaxBX":256,"parallaxBY":256,"autoscrollAX":0,"autoscrollAY":0,"autoscrollBX":0,"autoscrollBY":0,"layer":0}}]""");
            }
            var program = world["programs"]![0]!;
            program["entitiesRunning"] = enabled;
            var instructions = JsonNode.Parse("""[{"op":"text-cursor","text":100},{"op":"show-text","mode":"single","speaker":"ferryman","explicitWindows":true},{"op":"close-text"},{"op":"end"}]""")!.AsArray();
            if (second is not null) instructions.Insert(2, instructions[1]!.DeepClone());
            if (viewWait) instructions.Insert(1, JsonNode.Parse("""{"op":"wait-view"}"""));
            if (interaction) instructions.RemoveAt(instructions.Count - 2);
            program["instructions"] = instructions;
            var npc = world["maps"]![0]!["entities"]![0]!;
            npc["sprite"] = 30;
            if (npcRandom) npc["actions"] = JsonNode.Parse("""[{"op":"random-walk","x":2,"y":1,"radius":0},{"op":"jump","instruction":0}]""");
            document["start"]!["textSettings"] = JsonSerializer.SerializeToNode(new { messageSpeed = speed, mouthControl = 0, viewSpeed = 0 });
            document["start"]!["program"] = JsonNode.Parse("""{"program":"invitation","instruction":0}""");
            if (interaction)
            {
                document["start"]!.AsObject().Remove("program");
                world["maps"]![0]!["events"]![0]!["entityFlags"] = 3;
            }
            AddVisuals(document, null);
            configure?.Invoke(document);
        });
        if (interaction && startEvent)
        {
            Accept(session, new Interact(new("ferryman")));
            Accept(session, new AdvanceSimulation(session.Current.Story.Wait!.Token));
        }
        return session;
    }

    private static GameSession StartText(string text, uint seed = 0x12341234, int phase = 1, string? exclusion = null, bool face = false)
    {
        var session = Start("harbor-arrival", document =>
        {
            document["battle"]!["start"]!["mainSeed"] = seed;
            var world = document["world"]!;
            world["texts"]![0]!["text"] = text;
            world["memberNames"] = new JsonArray("A", "B{W2}");
            var callback = world["programs"]![0]!;
            callback["entitiesRunning"] = exclusion == "entities-enabled";
            callback["instructions"] = JsonNode.Parse("""
                [{"op":"text-cursor","text":100},
                 {"op":"show-text","mode":"single","speaker":null,"useEventSpeaker":true,"explicitWindows":true},
                 {"op":"end"}]
                """);
            if (exclusion == "legacy-windows") callback["instructions"]![1]!["explicitWindows"] = false;
            var npc = world["maps"]![0]!["entities"]![0]!;
            npc["sprite"] = 30;
            npc["facing"] = 1;
            npc["actions"] = JsonNode.Parse($$"""[{"op":"wait","ticks":{{phase}}},{"op":"random-walk","x":2,"y":1,"radius":1}]""");
            if (exclusion != "not-event") world["maps"]![0]!["events"]![0]!["entityFlags"] = face ? 3 : 0;
            if (exclusion != "unknown")
            {
                world["programs"]!.AsArray().Add(JsonNode.Parse(exclusion == "open" ? """
                    {"id":"setup","instructions":[{"op":"close-portrait"},{"op":"open-portrait","entity":"ferryman","flags":0},{"op":"end"}]}
                    """ : """{"id":"setup","instructions":[{"op":"close-portrait"},{"op":"end"}]}"""));
                document["start"]!["program"] = JsonNode.Parse("""{"program":"setup","instruction":0}""");
            }
            if (exclusion != "missing") AddVisuals(document, exclusion is "open" or "portrait-present" ? 7 : null);
        });
        Accept(session, new Interact(new("ferryman")));
        return session;
    }

    private static void AddVisuals(JsonNode document, int? portrait)
    {
        object Raster(int width, int height)
        {
            byte[] bytes = new byte[width * height * 4];
            return new { width, height, format = "rgba8", data = Convert.ToBase64String(bytes),
                sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)) };
        }
        document["world"]!["presentation"] = JsonSerializer.SerializeToNode(new
        {
            maps = document["world"]!["maps"]!.AsArray().Select(map => new
            { map = map!["id"]!.GetValue<string>(), atlas = Raster(128, 320), scale = 1,
                blocks = Enumerable.Range(0, 1024).Select(_ => new int[9]).ToArray() }),
            sprites = new[] { new { sprite = 30, directions = Enumerable.Range(0, 3).Select(_ => Raster(48, 24)).ToArray(), portrait, speech = 0 } },
            portraits = new[] { new { portrait = 7, raster = Raster(64, 64), eyes = new[] { new[] { 1, 1, 6, 7 } }, mouth = new[] { new[] { 1, 3, 7, 7 } } } },
        });
    }
}
