using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Persistence;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class FieldEgressTests
{
    private static readonly ActorRef Caster = FieldInventoryTests.Giver;
    private static readonly SpellRef Egress = new("egress", 1);

    [Theory]
    [InlineData(12, false, "refuge", 4)]
    [InlineData(3, true, "refuge", 0)]
    [InlineData(0, true, "other", 0)]
    public void EgressPaysOnceThenFlashesAndReturnsFromAccessibleOrEnclosedGround(byte mp, bool enclosed, string live, byte paidMp)
    {
        var session = Start(mp, enclosed, live);
        var before = session.Current;
        var party = before.Exploration!.Party;
        Assert.Equal(FieldRecoveryEffect.Egress, Assert.Single(session.QueryFieldActions(Caster).Spells).Effect);
        Assert.Same(before, session.Current); // Query/cancel without confirmation is free.
        var paid = Accept(session, new FieldCast(Caster, Egress));
        Assert.Single(paid.Observations, row => row.Kind == "field-egress-paid");
        Assert.Equal(paidMp, Member(session).Mp);
        Assert.Equal(before.Exploration.PlayerEntity.Position, session.Current.Exploration!.PlayerEntity.Position);
        var sound = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        Assert.Equal(PresentationCueKind.Sound, sound.Cue.Kind);
        Assert.Equal("stale-input", session.Submit(new(before.SessionId, before.Revision, null, new FieldCast(Caster, Egress))).Failure!.Code);
        Assert.NotNull(Send(session, new FieldCast(Caster, Egress)).Failure);
        Assert.Equal(paidMp, Member(session).Mp);
        Accept(session, new CompletePresentation(sound.Token, sound.Cue.Kind));
        var flash = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        Assert.Equal(PresentationCueKind.FlashWhite, flash.Cue.Kind);
        Assert.Same(session.Current, ProgramRunner.Run(session.Definition, session.Current, []).Snapshot);
        Assert.NotNull(Send(session, new Move(ExplorationDirection.East)).Failure);
        Accept(session, new CompletePresentation(flash.Token, flash.Cue.Kind));
        Assert.Equal((byte)12, Member(session).Mp); // Ordinary entry recovers living members after payment/flash.
        Assert.IsType<FullFadeWait>(session.Current.Story.Wait);
        Settle(session);
        Assert.Equal(live, session.Current.Exploration!.Map.Value);
        Assert.Equal(live == "refuge" ? new MapPosition(8, 8) : new(12, 10), session.Current.Exploration.PlayerEntity.Position);
        Assert.Equal(live == "refuge" ? 1 : 2, session.Current.Exploration.PlayerEntity.Motion.Facing);
        Assert.Equal(party.Gold, session.Current.Exploration.Party.Gold);
        Assert.Equal(party.MainSeed, session.Current.Exploration.Party.MainSeed);
        Assert.Equal(party.ThinkingSeed, session.Current.Exploration.Party.ThinkingSeed);
        Assert.Equal(before.Story.Flags, session.Current.Story.Flags);
        foreach (var actor in party.Actors)
        {
            var after = session.Current.Exploration.Party.Actors.Single(row => row.Actor == actor.Actor);
            Assert.Equal((actor.Exp, actor.Kills, actor.Defeats, actor.SourceLoadout, actor.Progress),
                (after.Exp, after.Kills, after.Defeats, after.SourceLoadout, after.Progress));
            if (actor.Hp == 0) Assert.Equal(0, after.Hp);
        }
        Accept(session, new Move(ExplorationDirection.East));
        Settle(session);
        Assert.Equal(live == "refuge" ? new MapPosition(9, 8) : new(13, 10), session.Current.Exploration.PlayerEntity.Position);
    }

    [Theory]
    [InlineData("unknown", false, true)]
    [InlineData("refuge", true, true)]
    [InlineData("refuge", false, false)]
    public void UnsupportedDestinationOrContextRejectsBeforePayment(string live, bool blockedDestination, bool firstBattle)
    {
        var session = Start(3, true, live, blockedDestination, firstBattle);
        var before = session.Current;
        Assert.NotNull(Send(session, new FieldCast(Caster, Egress)).Failure);
        Assert.Same(before, session.Current);
        Assert.Equal((byte)3, Member(session).Mp);
    }

    [Fact]
    public void NonOverworldEgressIsNoEffectAndExplicitTargetIsRejected()
    {
        var indoors = Start(3, false, "refuge", startMap: "refuge");
        var before = indoors.Current;
        Assert.Equal(FieldRecoveryEffect.NoEffect, Assert.Single(indoors.QueryFieldActions(Caster).Spells).Effect);
        var result = Accept(indoors, new FieldCast(Caster, Egress));
        Assert.Contains(result.Observations, row => row.Kind == "field-no-effect");
        Assert.Same(before.Exploration!.Party, indoors.Current.Exploration!.Party);
        var field = Start(8, true, "refuge");
        before = field.Current;
        Assert.Equal("field-egress-target", Send(field, new FieldCast(Caster, Egress, Caster)).Failure!.Code);
        Assert.Same(before, field.Current);
    }

    [Theory]
    [InlineData(0x10, true)]
    [InlineData(0x38, true)]
    [InlineData(0x08, false)]
    [InlineData(0x4008, false)]
    public void RestoredFallenCasterKeepsTheWordThroughRaiseAndEgressChecksItBeforePayment(ushort status, bool supported)
    {
        var refuge = new FieldReturnAnchor(new("map-3"), new(32, 13), 1);
        var (basis, data) = OpeningRestoreTests.Fixture(joined: true, church: refuge);
        var source = basis.Exploration!;
        var ridge = source.Maps[new("map-67")];
        var priest = new EntityRef("entity-128");
        var map = new ExplorationMapDefinition(ridge.Map, ridge.Layout, ridge.Traversal,
            [new(priest, new(8, 7), 3, 32, Sprite: 135)],
            [new(ExplorationEventKind.Interact, null, null, priest, new("raise", 0))],
            population: ridge.Population, basePalette: ridge.BasePalette, viewAreas: ridge.ViewAreas);
        var audio = source.Visuals!.Audio.ToDictionary();
        audio.Add("cast", new(77, 0, 44100, 1, [], "", null, null));
        var world = new ExplorationDefinition(source.Maps.Values.Select(row => row.Map == map.Map ? map : row),
            source.Programs.Values.Append(new("raise", [new OpenChurch(new(10, 200, 200, 10, 20)), new EndProgram()])),
            provenance: source.Provenance, partyFlags: source.PartyFlags, visuals: source.Visuals with { Audio = audio });
        var (bound, egress) = PrivateFieldEgress.Bind(world);
        var battle = basis.Encounters.Values.Single();
        var caster = new ActorRef("ally-1");
        var member = battle.Deployments.Single(row => row.Actor == caster);
        var growth = member.Definition.Growth! with
        {
            Spells = [new(1, 10)], SpellDefinitions = new Dictionary<byte, SpellRef> { [10] = Egress },
        };
        var encounter = battle.WithDeployments(battle.Deployments.Select(row => row.Actor == caster
            ? row with { Definition = member.Definition.WithGrowth(growth,
                new(PhysicalCriticalRule.OneIn32WithHalfBonus, false, false, 0)) } : row));
        var definition = new ScenarioDefinition(basis.Package, [encounter], exploration: bound)
        {
            Persistence = basis.Persistence,
            FieldPreparation = new([], spells: [new(Egress, "Egress", 8, FieldRecoveryEffect.Egress)], egress: egress),
        };
        var rules = RuleCompositions.Sf2();
        data = data with
        {
            Recipe = "devils-tail", Map = map.Map.Value, X = 8, Y = 8, Facing = 1,
            Flags = data.Flags.Concat([399, 521]).Order().ToArray(),
            Allies = data.Allies.Select(row => row.Actor == caster.Value
                ? row with { Hp = 0, Mp = 8, Status = status, Spells = [10, 63, 63, 63] } : row).ToArray(),
        };
        var opened = GameSession.Restore(definition, data, rules);
        Assert.True(opened is SessionStarted, (opened as SessionStartFailed)?.Failure.ToString());
        var original = ((SessionStarted)opened).Session;
        var captured = original.CaptureSave(original.Current.SessionId, original.Current.Revision);
        Assert.Null(captured.Error);
        var record = OpeningSaveJson.ReadRecord(
            OpeningSaveJson.WriteRecord(new OpeningSaveRecord(1, 1, 1, DateTimeOffset.UnixEpoch, captured.Data!)), 1);
        var session = Assert.IsType<SessionStarted>(SaveRecordRestore.Restore(definition, record, rules)).Session;
        BattleActorStartInput Raised() => session.Current.Exploration!.Party.Actors.Single(row => row.Actor == caster);
        Assert.Equal((ushort)0, Raised().Hp);
        Assert.Equal(status, Raised().Status);

        Accept(session, new Interact(priest));
        var choice = Assert.IsType<ChurchWait>(session.Current.Story.Wait);
        Accept(session, new ChooseChurch(choice.Token, ChurchAction.Raise));
        var offer = Assert.IsType<ChurchWait>(session.Current.Story.Wait);
        Assert.NotNull(offer.Offer);
        Assert.Equal(caster, offer.Offer.Member);
        Accept(session, new AnswerChurch(offer.Token, offer.Offer, true));
        var result = Assert.IsType<ChurchWait>(session.Current.Story.Wait);
        Accept(session, new ContinueChurch(result.Token));
        Accept(session, new LeaveChurch(Assert.IsType<ChurchWait>(session.Current.Story.Wait).Token));
        Assert.True(Raised().Hp > 0);
        Assert.Equal(status, Raised().Status);

        if (!supported)
        {
            var before = session.Current;
            var refused = Send(session, new FieldCast(caster, Egress));
            Assert.Equal("return-status-refresh", refused.Failure?.Code);
            Assert.Same(before, session.Current);
            Assert.Empty(refused.Observations);
            Assert.Equal(8, Raised().Mp);
            return;
        }

        var paid = Accept(session, new FieldCast(caster, Egress));
        Assert.Single(paid.Observations, row => row.Kind == "field-egress-paid");
        Assert.Equal(0, Raised().Mp);
        var sound = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        Accept(session, new CompletePresentation(sound.Token, sound.Cue.Kind));
        var flash = Assert.IsType<PresentationWait>(session.Current.Story.Wait);
        Assert.Equal(PresentationCueKind.FlashWhite, flash.Cue.Kind);
        Assert.Equal(status, Raised().Status);
        Assert.Equal(0, Raised().Mp);
        Accept(session, new CompletePresentation(flash.Token, flash.Cue.Kind));
        Assert.Equal(0, Raised().Status);
        Assert.Equal(8, Raised().Mp);
        Settle(session);
        Assert.Equal(refuge.Map, session.Current.Exploration!.Map);
    }

    private static BattleActorStartInput Member(GameSession session) => session.Current.Exploration!.Party.Actors.Single(a => a.Actor == Caster);

    internal static void Settle(GameSession session)
    {
        for (int i = 0; !session.Current.CanWaitAtInput && i < 1000; i++)
        {
            SessionCommand command = session.Current.Story.Wait switch
            {
                FullFadeWait { LogicalDone: true } fade => new CompletePresentation(fade.Token, fade.Kind),
                PresentationWait wait => new CompletePresentation(wait.Token, wait.Cue.Kind),
                var wait => new AdvanceSimulation(wait?.Token),
            };
            Accept(session, command);
        }
        Assert.True(session.Current.CanWaitAtInput);
    }

    private static GameSession Start(byte mp, bool enclosed, string live, bool blockedDestination = false,
        bool firstBattle = true, string startMap = "ridge")
    {
        var loadout = new BattleSourceLoadout([0, 127, 127, 127], []);
        var original = FieldInventoryTests.Session(carried: actor => actor.Actor == Caster
            ? actor with { Mp = mp, Hp = 50, SourceLoadout = loadout, Progress = new(2, 200, 12, 20, 4, 60, [Egress], loadout) }
            : actor with { Hp = 0, Status = 0 });
        var palette = new PalettePair(0xE84, 0x24);
        var area = new OriginalMapTraversal([new(0, 0, 63, 63)]);
        var open = new WorkingMapLayout(new ushort[WorkingMapLayout.WordCount]);
        var words = new ushort[WorkingMapLayout.WordCount];
        if (enclosed)
            foreach (var (x, y, word) in new[] { (32, 32, 0xC16B), (31, 32, 0xC16A), (33, 32, 0xC16C), (32, 31, 0xC164), (32, 33, 0xC177) })
                words[y * 64 + x] = (ushort)word;
        var destinationWords = new ushort[WorkingMapLayout.WordCount];
        if (blockedDestination) destinationWords[8 * 64 + 8] = 0xC000;
        ExplorationBattleRoute Route(string destination, MapPosition position, byte facing) => new(
            original.Current.Exploration!.Party.Encounter, 402, 502, null, null, null,
            Outcome: new(new("empty", 0), 0, new("empty", 0), new("empty", 0), new("empty", 0),
                new("ridge"), 3, new(destination), position, facing));
        var world = new ExplorationDefinition([
            new(new("ridge"), new(words), area, [], [], basePalette: palette,
                battles: [Route("refuge", new(8, 8), 1), Route("other", new(12, 10), 2)]),
            new(new("refuge"), new(destinationWords), area, [], [], basePalette: palette),
            new(new("other"), open, area, [], [], basePalette: palette)],
            [new("empty", [new EndProgram()]), new("escape", [
                new PresentCue(PresentationCueKind.Sound, "cast"),
                new PresentCue(PresentationCueKind.FlashWhite, "white-four-pulses", DurationTicks: 32),
                new ResetPartyBattleStats(LivingOnly: true), new EndProgram()])]);
        var definition = new ScenarioDefinition("field-egress", original.Definition.Encounters.Values, exploration: world)
        {
            FieldPreparation = new([new(0, "Herb", false, false)], spells: [new(Egress, "Egress", 8, FieldRecoveryEffect.Egress)],
                egress: new([new("ridge")], new("escape", 0)))
        };
        // A blocked return uses the already tested combat-placement seam, never a changed map.
        var flags = firstBattle ? new[] { 399, 502 } : [502];
        var safe = Assert.IsType<SessionStarted>(GameSession.Start(definition, new ExplorationStartInput(new(startMap),
            new("player"), new(6, 6), 3, 32, flags, original.Current.Exploration!.Party,
            display: new(1, palette, palette, FullFadeVisibility.BaseRestored), egressMap: new(live)))).Session;
        if (enclosed)
        {
            var field = MapTransfer.Build(world.Maps[new(startMap)], new("player"), new(32, 32), 3, 32,
                safe.Current.Exploration!.Party, flags, sourcePlacement: true);
            // External controlled boundary, not a public teleport or ordinary-start capability.
            typeof(GameSession).GetField("_current", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(safe, new SessionSnapshot(safe.Current.SessionId, safe.Current.Revision, safe.Current.ObservationSequence,
                    new ActiveExploration(field), safe.Current.Story, SessionStopReason.PlayerInput));
        }
        return safe;
    }
}
