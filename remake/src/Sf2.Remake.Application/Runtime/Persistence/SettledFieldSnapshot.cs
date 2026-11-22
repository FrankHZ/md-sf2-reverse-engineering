using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Persistence;

internal sealed record SettledFieldAvailability(Guid SessionId, long Revision, string? Error)
{
    internal bool Allowed => Error is null;
}

internal sealed record SettledFieldCapture(SettledFieldSnapshot? Data, string? Error);

// A value candidate for the current visit. Legacy recipe selection and entry programs
// never participate in admission or reconstruction.
internal sealed record SettledFieldSnapshot(SaveCompatibility Compatibility, FieldVisitSnapshot Visit,
    FieldPartySnapshot Party, SettledFieldRegisters Registers)
{
    internal static SettledFieldAvailability Query(ScenarioDefinition definition, SessionSnapshot current, SessionRules rules)
    {
        var result = Capture(definition, current, rules, current.SessionId, current.Revision);
        return new(current.SessionId, current.Revision, result.Error);
    }

    internal static SettledFieldCapture Capture(ScenarioDefinition definition, SessionSnapshot current, SessionRules rules,
        Guid session, long revision)
    {
        if (session != current.SessionId || revision != current.Revision) return new(null, "save-stale-revision");
        try
        {
            var value = Read(definition, current, rules);
            var rebuilt = value.Build(definition, rules).Snapshot.Exploration!;
            // A saved source must bind the installed program itself, including for a dormant
            // slot. Equal action cursors alone cannot detect a mislabeled installation.
            Require(current.Exploration!.AllEntities.Zip(rebuilt.AllEntities).All(pair =>
                pair.First.Actions is { } live ? pair.Second.Actions is { } restored &&
                    live.Actions.SequenceEqual(restored.Actions) : pair.Second.Actions is null), "action-binding");
            return new(value, null);
        }
        catch (BattleRuleException error) { return new(null, error.Code); }
    }

    internal SessionResult Build(ScenarioDefinition definition, SessionRules rules)
    {
        Require(Compatibility == OpeningSave.FieldCompatibility(definition, rules), "incompatible");
        var (party, _) = Party.Rebuild(definition, Registers.Flags, Registers.Names);
        var world = Visit.Rebuild(definition.Exploration!, party);
        var story = Registers.Rebuild(definition, world, Visit.Berth);
        _ = rules.Bind(definition.Encounters.Values, party.Actors);
        // Reserve the fresh music generation, if any, before a later command issues tokens.
        var snapshot = new SessionSnapshot(Guid.NewGuid(), 0, story.Music?.Generation ?? 0,
            new ActiveExploration(world), story, SessionStopReason.PlayerInput);
        var rebuilt = Read(definition, snapshot, rules);
        Require(Visit.SameValues(rebuilt.Visit) && Party.SameValues(rebuilt.Party) &&
            Registers.SameValues(rebuilt.Registers), "reconstruction-mismatch");
        return new(snapshot, [], SessionStopReason.PlayerInput);
    }

    private static SettledFieldSnapshot Read(ScenarioDefinition definition, SessionSnapshot current, SessionRules rules)
    {
        var story = current.Story;
        Require(HasStableControl(current), "busy");
        var visit = FieldVisitSnapshot.Capture(current);
        var world = current.Exploration!;
        var party = world.Party;
        // BeforeBattle re-establishes SkipIntro and BeforeBattleRouted. The completed
        // outcome supplies fresh carry. An active death-credit ledger still has a reader.
        Require(party.NewBattle is { Origin: BattleStartOrigin.NewGame, AlreadyRefreshedAllies: true,
            RosterOnly: true, BridgeBoundary: "" } && party.DeathCredit is null &&
            party.Actors.All(actor => actor.PositionOverride is null && !actor.RetainDeadPosition), "party-control");
        return new(OpeningSave.FieldCompatibility(definition, rules), visit, FieldPartySnapshot.Capture(definition, party),
            SettledFieldRegisters.Capture(definition, current));
    }

    internal static bool HasStableControl(SessionSnapshot current)
    {
        var story = current.Story;
        // These predicates cover script/caller stacks, service and input waits, and
        // transfer/admission/outcome ownership. OutcomeReturn can remain after completion.
        return current.CanWaitAtInput && current.HasOrdinaryFieldTime && story.DefeatedScene is null &&
            story.PortraitWindow is ClosedPortraitWindow && !story.Typewriting &&
            story.LogicalText is { Open: false, Moving: false } &&
            story.LogicalView is { Scrolling: false } &&
            story.Display is { Visibility: FullFadeVisibility.BaseRestored } display &&
            display.Base == display.Current && display.Colors?.IsSettled(story.SimulationTick) != false;
    }

    internal static void Require(bool condition, string operand)
    {
        if (!condition) throw new BattleRuleException("field-snapshot-" + operand, "fieldSnapshot." + operand);
    }
}

internal sealed record SettledFieldRegisters(IReadOnlyList<int> Flags, IReadOnlyDictionary<string, string> Names,
    MapId EgressMap, short StepCounter, long SimulationTick, int TextCursor, ExplorationTextSettings TextSettings,
    bool? EntityServices, bool WindowFixPending, StoryItemReceipt? ItemReceipt, byte? DialogueItem, EntityRef? Speaker,
    MapPartyLists? PartyLists, FieldView View, MapPosition? CameraTarget, int? CameraEntitySlot,
    byte FadePeriod, PalettePair Palette, FieldPaletteColors? Colors, FieldMusic Music)
{
    internal static SettledFieldRegisters Capture(ScenarioDefinition definition, SessionSnapshot current)
    {
        var world = current.Exploration!;
        var story = current.Story;
        SettledFieldSnapshot.Require(story.EgressMap is not null && story.StepCounter is not null &&
            story.TextSettings is not null, "unknown-register");
        int area = ExplorationViewContext.Require(world, story.LogicalView).Area;
        var colors = story.Display!.Colors?.SettledColors;
        var history = OpeningSave.Music(definition, current);
        var live = story.Music;
        if (live is not { HistoryBound: true, Battle: false } || live.Map != world.Map || live.Area != area)
            live = null;
        // A stale context is bound to the current default on the next field input.
        // A visit with no music register keeps the ordinary implicit default.
        var timed = live is { EndStep: not null } ? live : null;
        if (timed is not null)
            SettledFieldSnapshot.Require(definition.Exploration!.Visuals?.Audio.GetValueOrDefault(timed.Cue)?.ModernEndStep ==
                timed.EndStep, "music-resource");
        var music = new FieldMusic(history.Cue, history.Previous, story.Music is not null,
            timed is not null, timed?.Step ?? 0, timed is { ActualDone: true } or { DeliverySkipped: true },
            timed?.PreviousEligible ?? false);
        return new(story.Flags, story.Names, story.EgressMap!, story.StepCounter!.Value, story.SimulationTick,
            story.TextCursor, story.TextSettings!, story.EntityServices, story.WindowFixPending, story.ItemReceipt,
            story.DialogueItem, story.Speaker, story.PartyLists, FieldView.Capture(world, area, story.LogicalView!), story.CameraTarget,
            story.CameraEntitySlot, story.Display.Period, story.Display.Base,
            colors is { } lookup ? new(lookup.Current, lookup.Previous) : null, music);
    }

    internal StoryState Rebuild(ScenarioDefinition definition, ExplorationState world, RaftBerth? berth)
    {
        var content = definition.Exploration!;
        bool refuge = content.Maps.ContainsKey(EgressMap) && Sf2OutcomeReturnPolicy.FindEgress(definition, EgressMap) is not null;
        bool fallback = definition.DefeatFallback is { } rule && EgressMap == rule.Register &&
            Sf2OutcomeReturnPolicy.FindEgress(definition, rule.Destination) is not null;
        SettledFieldSnapshot.Require(refuge || fallback, "egress");
        SettledFieldSnapshot.Require(TextCursor is >= 0 and <= 65535 && TextSettings is { MessageSpeed: <= 3, ViewSpeed: 0 } &&
            SimulationTick is >= 0 and < long.MaxValue && FadePeriod > 0 && Palette is { Valid: true }, "register");
        SettledFieldSnapshot.Require(ItemReceipt is null || ItemReceipt is { Member: >= 0 and < 30, Item: < 127 },
            "receipt");
        var partyLayout = content.PartyFlags!;
        if (PartyLists is { } lists)
        {
            // UpdateForce writes these counted lists at its real call sites. They can lag
            // flags, but any retained values still come from the same roster layout.
            var joined = lists.Joined;
            SettledFieldSnapshot.Require(joined.Count <= partyLayout.MemberCount &&
                joined.All(member => member >= 0 && member < partyLayout.MemberCount) &&
                joined.SequenceEqual(lists.Active.Concat(lists.Reserve).Order()) &&
                joined.Distinct().Count() == joined.Count && lists.Active.Count <= partyLayout.Capacity, "party-lists");
        }
        var view = View.Rebuild(world);
        var music = Music.Rebuild(content, world.Map, View.Area);
        var display = new ExplorationDisplay(FadePeriod, Palette, Palette, FullFadeVisibility.BaseRestored,
            Colors?.Rebuild(SimulationTick));
        return new(Flags, textCursor: TextCursor, simulationTick: SimulationTick, partyLists: PartyLists,
            speaker: Speaker, cameraTarget: CameraTarget, cameraEntitySlot: CameraEntitySlot,
            portraitWindow: new ClosedPortraitWindow(), display: display, textSettings: TextSettings,
            logicalText: new(), logicalView: view, entityServices: EntityServices,
            windowFixPending: WindowFixPending, music: music, stepCounter: StepCounter, egressMap: EgressMap,
            names: Names, raft: berth, itemReceipt: ItemReceipt, dialogueItem: DialogueItem);
    }

    internal bool SameValues(SettledFieldRegisters other) =>
        this == (other with { Flags = Flags, Names = Names, Colors = Colors, Music = Music, PartyLists = PartyLists }) &&
        Flags.SequenceEqual(other.Flags) && FieldVisitSnapshot.SameBindings(Names, other.Names) &&
        (Colors is null ? other.Colors is null : other.Colors is { } colors &&
            Colors.Current.SequenceEqual(colors.Current) && Colors.Previous.SequenceEqual(colors.Previous)) &&
        Music == (other.Music with { Previous = Music.Previous }) && Music.Previous.SequenceEqual(other.Music.Previous) &&
        (PartyLists is null ? other.PartyLists is null : other.PartyLists is { } lists &&
            PartyLists.Joined.SequenceEqual(lists.Joined) && PartyLists.Active.SequenceEqual(lists.Active) &&
            PartyLists.Reserve.SequenceEqual(lists.Reserve));
}

internal sealed record FieldView(int Area, int AutoscrollAX, int AutoscrollAY, int AutoscrollBX, int AutoscrollBY,
    LogicalViewAxis AX, LogicalViewAxis AY, LogicalViewAxis BX, LogicalViewAxis BY, int FollowCounter, bool HideWindows)
{
    internal static FieldView Capture(ExplorationState world, int area, LogicalView view)
    {
        SettledFieldSnapshot.Require(view.TargetSlot == world.PlayerEntity.Slot, "view");
        return new(area, view.Area.AutoscrollAX, view.Area.AutoscrollAY, view.Area.AutoscrollBX, view.Area.AutoscrollBY,
            view.AX, view.AY, view.BX, view.BY, view.FollowCounter, view.HideWindows);
    }

    internal LogicalView Rebuild(ExplorationState world)
    {
        SettledFieldSnapshot.Require(Area >= 0 && Area < world.Definition.ViewAreas.Count &&
            FollowCounter is >= 0 and <= 65535, "view");
        var area = world.Definition.ViewAreas[Area] with { AutoscrollAX = AutoscrollAX,
            AutoscrollAY = AutoscrollAY, AutoscrollBX = AutoscrollBX, AutoscrollBY = AutoscrollBY };
        ExplorationViewRunner.Validate(area);
        SettledFieldSnapshot.Require(new[] { AX, AY, BX, BY }
            .All(axis => axis.Speed is >= 0 and <= 128 && (!axis.Active || axis.Speed > 0)), "view-axis");
        return new(area, world.PlayerEntity.Slot, AX, AY, BX, BY, FollowCounter, HideWindows);
    }
}

internal sealed record FieldPaletteColors(IReadOnlyList<ushort> Current, IReadOnlyList<ushort> Previous)
{
    internal PaletteColorTransition Rebuild(long tick)
    {
        SettledFieldSnapshot.Require(Current.Count == 512 && Previous.Count == 512 &&
            Current.All(PaletteColorTransition.Valid) && Previous.All(PaletteColorTransition.Valid), "palette-colors");
        return PaletteColorTransition.RestoreSettled(Current, Previous, tick);
    }
}

internal sealed record FieldMusic(string Cue, IReadOnlyList<string> Previous, bool Explicit, bool Timed,
    int Step, bool Delivered, bool PreviousEligible)
{
    internal MusicProgress? Rebuild(ExplorationDefinition content, MapId map, int area)
    {
        var audio = content.Visuals?.Audio;
        SettledFieldSnapshot.Require(audio is not null && Previous.Count <= 9 &&
            Previous.Prepend(Cue).All(cue => cue is not null && audio.TryGetValue(cue, out var row) && row.Command < 65),
            "music-resource");
        int? end = Timed ? audio![Cue].ModernEndStep : null;
        SettledFieldSnapshot.Require(Timed ? end is > 0 && Step >= 0 && Step <= end &&
            (!PreviousEligible || Step == end) : Step == 0 && !Delivered && !PreviousEligible, "music-phase");
        return Explicit ? new(1, Cue, Array.AsReadOnly(Previous.ToArray()), map, area, end, Step,
            ActualDone: Delivered, PreviousEligible: PreviousEligible) : null;
    }
}
