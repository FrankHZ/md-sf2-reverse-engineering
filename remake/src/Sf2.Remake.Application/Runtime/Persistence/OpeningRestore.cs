using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Persistence;

internal static class OpeningRestore
{
    internal static SessionResult Build(ScenarioDefinition definition, OpeningSaveData data, SessionRules rules)
    {
        Require(data.Compatibility == OpeningSave.Compatibility(definition, rules), "save-incompatible");
        definition.Persistence!.ValidateRegisters(data);
        Require(data.Allies is { Count: > 0 and <= 30 } && data.Flags is { Count: <= 65536 } &&
            data.ActiveAllies is { Count: > 0 and <= 12 } && data.OnceFlags is { Count: <= 30 } && data.Deals is { Count: <= 127 }, "save-shape");
        Require(data.Flags.All(flag => flag is >= 0 and <= 65535) && data.Flags.Distinct().Count() == data.Flags.Count &&
            data.ActiveAllies.Distinct().Count() == data.ActiveAllies.Count && data.Allies.Select(a => a.Actor).Distinct().Count() == data.Allies.Count &&
            data.OnceFlags.All(flag => flag <= 29) && data.OnceFlags.Distinct().Count() == data.OnceFlags.Count &&
            data.Deals.All(deal => deal.Item < 127 && deal.Count <= 15) && data.Deals.Select(d => d.Item).Distinct().Count() == data.Deals.Count, "save-duplicates-or-range");
        Require(data.Gold <= 999999 && data.TextCursor is >= 0 and <= 65535 && data.FadePeriod > 0 && data.Difficulty <= 3 &&
            data.TextSettings is { MessageSpeed: <= 3, ViewSpeed: 0 } &&
            data.X is >= 0 and < 64 && data.Y is >= 0 and < 64 && data.Facing <= 3, "save-range");
        Require(definition.Encounters.ContainsKey(data.Encounter), "save-encounter");
        if (definition.PrivateDefinitions is { } source)
            Require(data.Allies.SelectMany(actor => actor.Items).All(word => source.Items.ContainsKey((byte)(word & 127))) &&
                data.Deals.All(deal => source.Items.ContainsKey(deal.Item)), "save-item-identity");
        var deployments = definition.Encounters.Values.SelectMany(encounter => encounter.Deployments)
            .Where(row => row.Faction == BattleFaction.Ally).GroupBy(row => row.Actor).ToDictionary(group => group.Key, group => group.First());
        Require(deployments.Keys.Select(actor => actor.Value).ToHashSet().SetEquals(data.Allies.Select(actor => actor.Actor)), "save-roster");
        var actors = data.Allies.Select(saved => Ally(saved, deployments[new(saved.Actor)].Definition)).ToArray();
        var layout = definition.Exploration!.PartyFlags ?? throw new BattleRuleException("save-membership-unavailable", "save.party");
        var lists = MapPartyMembership.Rebuild(data.Flags, layout);
        var members = deployments.Values.Where(row => row.Initialization?.AllyPartyMember is not null)
            .ToDictionary(row => row.Initialization!.AllyPartyMember!.Value, row => row.Actor.Value);
        Require(lists.Active.All(members.ContainsKey) && lists.Joined.All(members.ContainsKey) &&
            lists.Active.Select(member => members[member]).SequenceEqual(data.ActiveAllies), "save-membership");
        Require(data.Allies.Where(actor => data.ActiveAllies.Contains(actor.Actor)).All(actor => !actor.Absent), "save-active-absence");
        Require(data.Allies.Single(actor => actor.Actor == members[0]) is { Hp: > 0, Absent: false, Initialized: true }, "save-leader");
        var party = new BattleStartInput(data.Encounter, actors, data.MainSeed, data.ThinkingSeed, data.Gold,
            new("opening-new-game", "source-initialization", "", data.Difficulty, false, true, true,
                data.AllyAutoBattle, data.OpponentControl, null) { Origin = BattleStartOrigin.NewGame },
            data.ActiveAllies.Select(id => new ActorRef(id)), new(data.OnceFlags, data.Deals.ToDictionary(deal => deal.Item, deal => deal.Count)));
        var anchor = new FieldReturnAnchor(new(data.Map), new(data.X, data.Y), data.Facing);
        var location = definition.Persistence!.Select(definition, anchor.Map, data.Flags, anchor);
        Require(location.Recipe == data.Recipe && location.Anchor == anchor, "save-recipe");
        var world = definition.Persistence.Rebuild(definition, location, party, data.Flags, out var flags);
        var visuals = definition.Exploration.Visuals;
        Require(data.Music is { Previous.Count: <= 9 } && visuals is not null &&
            data.Music.Previous.Prepend(data.Music.Cue).All(cue => cue is not null && visuals.Audio.TryGetValue(cue, out var audio) && audio.Command < 65), "save-music-resource");
        Require(world.Definition.BasePalette is { Valid: true }, "save-palette-resource");
        int area = world.Definition.Traversal.SelectActiveArea(anchor.Position)!.OneBasedRecordOrdinal - 1;
        var music = new MusicProgress(1, data.Music.Cue, Array.AsReadOnly(data.Music.Previous.ToArray()), anchor.Map, area,
            visuals!.Audio[data.Music.Cue].ModernEndStep);
        var palette = world.Definition.BasePalette!;
        var story = new StoryState(flags, textCursor: data.TextCursor, partyLists: MapPartyMembership.Rebuild(flags, layout),
            display: new(data.FadePeriod, palette, palette, FullFadeVisibility.BaseRestored), textSettings: data.TextSettings,
            stepCounter: data.StepCounter, egressMap: new(data.EgressMap), music: music, saveAnchor: anchor);
        story = ExplorationTextRunner.Initialize(world, story);
        _ = rules.Bind(definition.Encounters.Values, actors);
        var snapshot = new SessionSnapshot(Guid.NewGuid(), 0, 0, new ActiveExploration(world), story, SessionStopReason.PlayerInput);
        Require(snapshot.CanWaitAtInput, "save-restore-boundary");
        return new(snapshot, Array.Empty<SessionObservation>(), SessionStopReason.PlayerInput);
    }

    private static BattleActorStartInput Ally(SavedAlly saved, BattleActorDefinition definition)
    {
        var growth = definition.Growth;
        Require(growth is not null && saved.Class == growth.ClassId && saved.Level is >= 1 and <= 99 &&
            saved.MaxHp is > 0 and <= 200 && saved.MaxMp <= 200 && saved.BaseAttack <= 200 && saved.Defense <= 200 && saved.Agility <= 100 &&
            saved.Hp <= saved.MaxHp && saved.Mp <= saved.MaxMp && saved.Exp <= 200 && saved.Kills <= 9999 && saved.Defeats <= 9999 &&
            saved.Initialized && saved.Items is { Count: 4 } && saved.Spells is { Count: 4 }, "save-ally-range");
        Require(saved.Items.All(word => (word & 127) == 127 || definition.SourceLoadout is not null) &&
            saved.Spells.Where(value => value != 63).Select(value => value & 63).Distinct().Count() == saved.Spells.Count(value => value != 63), "save-loadout");
        var spells = new List<SpellRef>();
        foreach (byte packed in saved.Spells.Where(value => value != 63))
        {
            Require(growth!.Spells.Any(spell => spell.PackedSpell == packed && spell.Level <= saved.Level), "save-learned-spell");
            for (int rank = 0; rank <= packed >> 6; rank++)
            {
                Require(growth.SpellDefinitions.TryGetValue((byte)((packed & 63) | rank << 6), out var spell), "save-spell-resource");
                spells.Add(spell);
            }
        }
        var loadout = new BattleSourceLoadout(saved.Items, saved.Spells);
        // Equipment's numerical accessor validates its supported equipped state where definitions admit it.
        if (definition.Equipment is not null) BattleEquipment.ValidateLoadout(definition, loadout);
        Require(saved.Items.All(word => word <= 511), "save-item-word");
        var progress = new BattleActorProgress(saved.Level, saved.MaxHp, saved.MaxMp, saved.BaseAttack, saved.Defense,
            saved.Agility, Array.AsReadOnly(spells.ToArray()), loadout);
        return new(new(saved.Actor), saved.Hp, saved.Mp, saved.Exp, saved.Kills, saved.Defeats, saved.Status, null,
            progress, loadout, saved.Activation, saved.Initialized, saved.Absent);
    }
    private static void Require(bool valid, string code)
    { if (!valid) throw new BattleRuleException(code, "save"); }
}
