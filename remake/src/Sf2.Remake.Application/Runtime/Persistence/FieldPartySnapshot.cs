using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Persistence;

// The same carried values serve recipe saves and exact settled visits.
internal sealed record FieldPartySnapshot(string Encounter, IReadOnlyList<SavedAlly> Allies,
    IReadOnlyList<string> ActiveAllies, uint MainSeed, uint ThinkingSeed, uint Gold,
    IReadOnlyList<byte> OnceFlags, IReadOnlyList<SavedDeal> Deals, IReadOnlyList<byte> Depot,
    byte Difficulty, bool AllyAutoBattle, bool OpponentControl, ushort? MissingCandidateAllyWord = null)
{
    internal static FieldPartySnapshot Capture(ScenarioDefinition definition, BattleStartInput party)
    {
        var actors = party.Actors.Select(actor =>
        {
            var progress = actor.Progress;
            var loadout = actor.SourceLoadout ?? progress?.SourceLoadout;
            var source = definition.Encounters.Values.SelectMany(e => e.Deployments).FirstOrDefault(row => row.Actor == actor.Actor)?.Definition;
            if (progress is null || loadout is null || source?.Growth is null || actor.Exp is null ||
                actor.Kills is null || actor.Defeats is null || actor.ActivationWord is null || actor.Initialized is null)
                throw new BattleRuleException("save-party-unknown", "save.allies");
            return new SavedAlly(actor.Actor.Value, progress.ClassId ?? source.Growth.ClassId, actor.Hp, actor.Mp, actor.Exp.Value,
                actor.Kills.Value, actor.Defeats.Value, actor.Status, actor.ActivationWord.Value, actor.Initialized.Value, actor.Absent,
                progress.Level, progress.MaxHp, progress.MaxMp, progress.BaseAttack, progress.Defense, progress.Agility,
                Array.AsReadOnly(loadout.Items.ToArray()), Array.AsReadOnly(loadout.Spells.ToArray()))
            { BaseMove = progress.BaseMove, CurrentStats = progress.CurrentStats };
        }).ToArray();
        if (party.Gold is null || party.ItemCustody is null || party.ActiveAllies is null || party.NewBattle is null)
            throw new BattleRuleException("save-progress-unknown", "save.party");
        return new(party.Encounter, Array.AsReadOnly(actors),
            Array.AsReadOnly(party.ActiveAllies.Select(actor => actor.Value).ToArray()), party.MainSeed, party.ThinkingSeed,
            party.Gold.Value, Array.AsReadOnly(party.ItemCustody.OnceFlags.ToArray()),
            Array.AsReadOnly(party.ItemCustody.Deals.Select(pair => new SavedDeal(pair.Key, pair.Value)).ToArray()),
            Array.AsReadOnly(party.ItemCustody.Depot.ToArray()), party.NewBattle.Difficulty,
            party.NewBattle.AllyAutoBattle, party.NewBattle.OpponentControl, party.NewBattle.MissingCandidateAllyWord);
    }

    internal (BattleStartInput Party, MapPartyLists Lists) Rebuild(ScenarioDefinition definition,
        IReadOnlyList<int> flags, IReadOnlyDictionary<string, string> names,
        OpeningSaveData? legacy = null, SaveCompatibility? expected = null)
    {
        Require(Depot is { Count: <= 64 } && Depot.All(item => item < 127), "save-depot");
        Require(Allies is { Count: > 0 and <= 30 } && flags is { Count: <= 65536 } &&
            ActiveAllies is { Count: > 0 and <= 12 } && OnceFlags is { Count: <= 30 } && Deals is { Count: <= 127 }, "save-shape");
        Require(flags.All(flag => flag is >= 0 and <= 65535) && flags.Distinct().Count() == flags.Count &&
            ActiveAllies.Distinct().Count() == ActiveAllies.Count && Allies.Select(a => a.Actor).Distinct().Count() == Allies.Count &&
            OnceFlags.All(flag => flag <= 29) && OnceFlags.Distinct().Count() == OnceFlags.Count &&
            Deals.All(deal => deal.Item < 127 && deal.Count <= 15) && Deals.Select(d => d.Item).Distinct().Count() == Deals.Count, "save-duplicates-or-range");
        Require(Gold <= 999999 && Difficulty <= 3, "save-range");
        Require(definition.Encounters.ContainsKey(Encounter), "save-encounter");
        if (definition.PrivateDefinitions is { } source)
            Require(Allies.SelectMany(actor => actor.Items).All(word => source.Items.ContainsKey((byte)(word & 127))) &&
                Deals.All(deal => source.Items.ContainsKey(deal.Item)) && Depot.All(source.Items.ContainsKey), "save-item-identity");
        var deployments = definition.Encounters.Values.SelectMany(encounter => encounter.Deployments)
            .Where(row => row.Faction == BattleFaction.Ally).GroupBy(row => row.Actor).ToDictionary(group => group.Key, group => group.First());
        Require(names is { Count: <= 30 } && names.All(pair => deployments.ContainsKey(new(pair.Key)) &&
            pair.Value is { Length: > 0 } && NewGameName.Error(pair.Value) is null), "save-name");
        var savedActors = Allies.Select(actor => new ActorRef(actor.Actor)).ToHashSet();
        Require(savedActors.IsSubsetOf(deployments.Keys), "save-roster");
        var actors = Allies.Select(saved => Ally(saved, deployments[new(saved.Actor)].ResolveClass(saved.Class).Definition)).ToList();
        if (legacy is null)
            Require(savedActors.SetEquals(deployments.Keys), "save-roster");
        else
            actors.AddRange(definition.Persistence!.RestoreRoster(definition, legacy, expected!, deployments, savedActors));
        var layout = definition.Exploration!.PartyFlags ?? throw new BattleRuleException("save-membership-unavailable", "save.party");
        var lists = MapPartyMembership.Rebuild(flags, layout);
        var members = deployments.Values.Where(row => row.Initialization?.AllyPartyMember is not null)
            .ToDictionary(row => row.Initialization!.AllyPartyMember!.Value, row => row.Actor.Value);
        Require(lists.Active.All(members.ContainsKey) && lists.Joined.All(members.ContainsKey) &&
            lists.Active.Select(member => members[member]).SequenceEqual(ActiveAllies), "save-membership");
        Require(Allies.Where(actor => ActiveAllies.Contains(actor.Actor)).All(actor => !actor.Absent), "save-active-absence");
        Require(members.TryGetValue(0, out var leader) &&
            Allies.SingleOrDefault(actor => actor.Actor == leader) is { Hp: > 0, Absent: false, Initialized: true }, "save-leader");
        var party = new BattleStartInput(Encounter, actors, MainSeed, ThinkingSeed, Gold,
            new("opening-new-game", "source-initialization", "", Difficulty, false, true, true,
                AllyAutoBattle, OpponentControl, MissingCandidateAllyWord) { Origin = BattleStartOrigin.NewGame },
            ActiveAllies.Select(id => new ActorRef(id)), new(OnceFlags, Deals.ToDictionary(deal => deal.Item, deal => deal.Count), Depot));
        return (party, lists);
    }

    internal bool SameValues(FieldPartySnapshot other) =>
        this == (other with { Allies = Allies, ActiveAllies = ActiveAllies, OnceFlags = OnceFlags, Deals = Deals, Depot = Depot }) &&
        ActiveAllies.SequenceEqual(other.ActiveAllies) && OnceFlags.SequenceEqual(other.OnceFlags) &&
        Deals.SequenceEqual(other.Deals) && Depot.SequenceEqual(other.Depot) && Allies.Count == other.Allies.Count &&
        Allies.Zip(other.Allies).All(pair => pair.First == (pair.Second with { Items = pair.First.Items, Spells = pair.First.Spells }) &&
            pair.First.Items.SequenceEqual(pair.Second.Items) && pair.First.Spells.SequenceEqual(pair.Second.Spells));

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
            Require(growth!.CanKnow(packed, saved.Level), "save-learned-spell");
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
            saved.Agility, Array.AsReadOnly(spells.ToArray()), loadout, saved.Class)
        { BaseMove = saved.BaseMove, CurrentStats = saved.CurrentStats };
        Require(progress.ValidGrowthItemStats, "save-ally-range");
        return new(new(saved.Actor), saved.Hp, saved.Mp, saved.Exp, saved.Kills, saved.Defeats, saved.Status, null,
            progress, loadout, saved.Activation, saved.Initialized, saved.Absent);
    }
    private static void Require(bool valid, string code)
    { if (!valid) throw new BattleRuleException(code, "save"); }
}
