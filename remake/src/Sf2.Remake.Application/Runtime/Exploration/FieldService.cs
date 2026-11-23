using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Application.Runtime.Exploration;

public enum ChurchAction { Raise, Cure, Promote }
public enum ChurchEffect { Raise, Poison, Stun, Curse }
public enum ChurchPhase { Menu, Offer, Result, Members, Consent, Regular, AnotherService, Special }
public enum ChurchResultKind { NoApplicableMember, Declined, InsufficientGold, Paid, LowLevel, NoPromotion, Unavailable, SpecialUnavailable, Promoted }
public sealed record ChurchResult(ChurchResultKind Kind, ChurchOffer? Treatment = null);
public sealed record HeldPromotionItem(ActorRef Holder, int Slot, ushort Word);
public sealed record PromotionOffer(ActorRef Member, byte? Source, byte? Destination, byte Level, HeldPromotionItem? SpecialItem);
public sealed record ChurchOffer(ActorRef Member, ChurchEffect Effect, uint Cost, ushort Hp, ushort Status,
    int? Slot = null, ushort? Word = null);
public sealed record ChurchWait(WaitToken Token, ChurchDefinition Definition, ChurchAction? Action = null,
    IReadOnlyList<ChurchOffer>? Offers = null, int Index = 0, ChurchResult? Result = null, bool CurseUnavailable = false) : ProgramWait(Token)
{
    public IReadOnlyList<PromotionOffer>? Candidates { get; init; }
    public PromotionOffer? Candidate { get; init; }
    public bool Consented { get; init; }
    public bool DeclinedSpecial { get; init; }
    public bool LeavingPromotion { get; init; }
    public ChurchPhase Phase => LeavingPromotion ? ChurchPhase.AnotherService : (Result, Action, Candidate, Consented) switch
    {
        (not null, _, _, _) => ChurchPhase.Result,
        (_, null, _, _) => ChurchPhase.Menu,
        (_, ChurchAction.Promote, null, _) => ChurchPhase.Members,
        (_, ChurchAction.Promote, _, false) => ChurchPhase.Consent,
        (_, ChurchAction.Promote, { SpecialItem: not null }, true) when !DeclinedSpecial => ChurchPhase.Special,
        (_, ChurchAction.Promote, _, true) => ChurchPhase.Regular,
        _ => ChurchPhase.Offer,
    };
    public ChurchOffer? Offer => Offers is not null && Index < Offers.Count ? Offers[Index] : null;
}
public abstract record ChurchCommand(WaitToken Wait) : SessionCommand;
public sealed record ChooseChurch(WaitToken Wait, ChurchAction Action) : ChurchCommand(Wait);
public sealed record AnswerChurch(WaitToken Wait, ChurchOffer Offer, bool Accept) : ChurchCommand(Wait);
public sealed record SelectPromotion(WaitToken Wait, PromotionOffer Candidate) : ChurchCommand(Wait);
public sealed record AnswerPromotion(WaitToken Wait, PromotionOffer Candidate, bool Accept) : ChurchCommand(Wait);
public sealed record CancelPromotion(WaitToken Wait) : ChurchCommand(Wait);
public sealed record ContinueChurch(WaitToken Wait) : ChurchCommand(Wait);
public sealed record LeaveChurch(WaitToken Wait, bool Save = false) : ChurchCommand(Wait);

internal static class FieldService
{
    internal static SessionResult Submit(ScenarioDefinition definition, SessionSnapshot current, ChurchCommand command, SessionRules rules)
    {
        if (current.Exploration is not { } field || current.Story.Wait is not ChurchWait wait || wait.Token != command.Wait)
            return BattleCommandDispatcher.Reject(current, "stale-or-wrong-service", "wait");
        List<SessionObservation> observations = [];
        try
        {
            var next = wait with { Token = new(current.ObservationSequence + 1) };
            var party = field.Party;
            string kind;
            switch (command)
            {
                case ChooseChurch { Action: ChurchAction.Promote } when wait.Phase == ChurchPhase.Menu:
                    var candidates = Candidates(definition, current);
                    next = next with { Action = ChurchAction.Promote, Candidates = candidates,
                        Result = candidates.Any(c => Sf2ClassRules.RegularBase(c.Source) && c.Level >= 20)
                            ? null : new(ChurchResultKind.NoApplicableMember) };
                    kind = "church-promotion-members"; break;
                case SelectPromotion select when wait.Phase == ChurchPhase.Members && wait.Candidates!.Contains(select.Candidate):
                    var choice = select.Candidate;
                    next = next with { Candidate = choice, Consented = false, DeclinedSpecial = false,
                        Result = !Sf2ClassRules.RegularBase(choice.Source) ? new(ChurchResultKind.NoPromotion) :
                            choice.Level < 20 ? new(ChurchResultKind.LowLevel) : null };
                    kind = "church-promotion-selection"; break;
                case AnswerPromotion answer when wait.Phase is ChurchPhase.Consent or ChurchPhase.Special or ChurchPhase.Regular &&
                        wait.Candidate == answer.Candidate:
                    var candidate = answer.Candidate;
                    if (!Candidates(definition, current).Contains(candidate))
                        throw new BattleRuleException("stale-promotion", "service.offer");
                    if (wait.Phase == ChurchPhase.Special)
                    {
                        next = answer.Accept
                            ? next with { Result = new(ChurchResultKind.SpecialUnavailable) }
                            : next with { DeclinedSpecial = true,
                                Result = candidate.Destination is null ? new(ChurchResultKind.Unavailable) : null };
                    }
                    else if (!answer.Accept) next = next with { Candidate = null, Consented = false, DeclinedSpecial = false };
                    else if (wait.Phase == ChurchPhase.Consent)
                    {
                        next = next with { Consented = true,
                            Result = candidate.SpecialItem is null && candidate.Destination is null ? new(ChurchResultKind.Unavailable) : null };
                    }
                    else
                    {
                        (field, party) = Promote(definition, current, field, candidate);
                        next = next with { Result = new(ChurchResultKind.Promoted) };
                    }
                    kind = next.Result?.Kind == ChurchResultKind.Promoted ? "church-promoted" : "church-promotion-answer";
                    break;
                case ContinueChurch when wait.Phase == ChurchPhase.Result && wait.Action == ChurchAction.Promote &&
                        wait.Result!.Kind != ChurchResultKind.NoApplicableMember:
                    next = next with { Candidates = Candidates(definition, current), Result = null, Candidate = null, Consented = false, DeclinedSpecial = false };
                    kind = "church-promotion-members"; break;
                case CancelPromotion when wait.Phase == ChurchPhase.Members:
                case ContinueChurch when wait.Action == ChurchAction.Promote && wait.Result?.Kind == ChurchResultKind.NoApplicableMember:
                    next = new(next.Token, wait.Definition) { LeavingPromotion = true };
                    kind = "church-promotion-cancelled"; break;
                case ChooseChurch choose when wait.Phase == ChurchPhase.Menu && Enum.IsDefined(choose.Action):
                    var offers = Offers(definition, current, wait.Definition, choose.Action);
                    next = next with { Action = choose.Action, Offers = offers, Index = 0,
                        Result = offers.Count == 0 ? new(ChurchResultKind.NoApplicableMember) : null,
                        CurseUnavailable = offers.Count == 0 && CurseUnavailable(definition, current, choose.Action) };
                    kind = "church-action"; break;
                case AnswerChurch answer when wait.Phase == ChurchPhase.Offer && wait.Offer == answer.Offer:
                    var offer = wait.Offer!;
                    // Recheck the complete immutable offer against live membership/vitals/cost.
                    if (!Offers(definition, current, wait.Definition, wait.Action!.Value).Contains(offer))
                        throw new BattleRuleException("stale-service-offer", "service.offer");
                    int index = wait.Index + 1;
                    ChurchResult result;
                    if (!answer.Accept) { kind = "church-declined"; result = new(ChurchResultKind.Declined, offer); }
                    else
                    {
                        uint gold = party.Gold ?? throw new BattleRuleException("service-gold-unavailable", "party.gold", true);
                        if (gold < offer.Cost)
                        {
                            kind = "church-insufficient-gold"; result = new(ChurchResultKind.InsufficientGold, offer);
                            // Source Cure aborts the remaining members of this status family.
                            if (wait.Action == ChurchAction.Cure)
                                while (index < wait.Offers!.Count && wait.Offers[index].Effect == offer.Effect) index++;
                        }
                        else
                        {
                            var member = PartyDetailsQuery.Query(definition, current)!.Members.Single(row => row.Carried.Actor == offer.Member);
                            var changed = offer.Effect switch
                            {
                                ChurchEffect.Raise => member.Carried with { Hp = Math.Min(member.MaxHp!.Value, wait.Definition.RaisedHpCap) },
                                ChurchEffect.Curse => FieldInventory.WithLoadout(member,
                                    BattleEquipment.RemoveCurse(member.Deployment!.Definition, FieldInventory.Loadout(member)), refresh: true),
                                _ => member.Carried with { Status = (ushort)(member.Carried.Status & ~TreatmentMask(offer.Effect)) },
                            };
                            party = FieldInventory.Replace(party, [changed], gold: gold - offer.Cost);
                            next = next with { Offers = Array.AsReadOnly(wait.Offers!.Select(row => row.Member == changed.Actor
                                ? row with { Hp = changed.Hp, Status = changed.Status } : row).ToArray()) };
                            kind = "church-paid";
                            result = new(ChurchResultKind.Paid, offer);
                        }
                    }
                    next = next with { Index = index, Result = result }; break;
                case ContinueChurch when wait.Phase is ChurchPhase.Result or ChurchPhase.AnotherService:
                    next = wait.Offer is null ? new(next.Token, wait.Definition) : next with { Result = null, CurseUnavailable = false };
                    kind = "church-continued"; break;
                case LeaveChurch when wait.Phase == ChurchPhase.Menu:
                case LeaveChurch { Save: false } when wait.Phase == ChurchPhase.AnotherService:
                    current = ProgramRunner.Commit(current, current.Active, current.Story.Copy(current.Story.Cursor), observations,
                        command is LeaveChurch { Save: true } ? "church-save-requested" : "church-closed");
                    return ProgramRunner.Run(definition, current, observations, rules);
                default: return BattleCommandDispatcher.Reject(current, "invalid-service-selection", "service");
            }
            if (next.Action is { } action && next.Offer is null)
                next = next with { CurseUnavailable = CurseUnavailable(definition, current, action) };
            current = ProgramRunner.Commit(current, new ActiveExploration(field.WithParty(party)),
                current.Story.Copy(current.Story.Cursor, next), observations, kind);
            return ProgramRunner.Result(current, observations);
        }
        catch (BattleRuleException error)
        {
            return new(current, [], error.Unsupported ? SessionStopReason.Unsupported : SessionStopReason.Rejected,
                new(error.Unsupported ? SessionFailureKind.UnsupportedCapability : SessionFailureKind.IllegalCommand,
                    error.Code, error.Field, error.Code.Replace('-', ' ')));
        }
    }

    private static (ExplorationState, BattleStartInput) Promote(ScenarioDefinition definition, SessionSnapshot current,
        ExplorationState field, PromotionOffer candidate)
    {
        var member = PartyDetailsQuery.Query(definition, current)!.Members.Single(m => m.Carried.Actor == candidate.Member);
        var destination = member.Deployment!.ResolveClass(candidate.Destination).Definition;
        var loadout = PromotionLoadout(member, destination);
        if (!PoisonTurnRules.SupportedPhysicalStatus(member.Carried.Status) || member.Carried.Progress is not { } progress)
            throw new BattleRuleException("promotion-refresh", "party.progress", true);
        BattleEquipment.ValidateLoadout(destination, loadout);
        var changed = member.Carried with { Exp = 0,
            SourceLoadout = member.Carried.SourceLoadout is null ? null : loadout, Progress = progress with
            { ClassId = candidate.Destination, Level = 1, SourceLoadout = loadout, BaseMove = null, CurrentStats = null } };
        var party = FieldInventory.Replace(field.Party, [changed]);
        if (field.Population is { } population && member.Deployment.Initialization?.AllyPartyMember is { } memberIndex)
        {
            var sprite = population.AllySprites?.SingleOrDefault(s => s.Character == memberIndex);
            if (sprite?.PromotionClass != candidate.Destination)
                throw new BattleRuleException("promotion-appearance", "population.allySprites", true);
            var entity = field.AllEntities.FirstOrDefault(e => e.Entity.Value == $"entity-{memberIndex}");
            if (entity is not null && changed.Hp > 0)
                field = field.WithEntity(entity with { Sprite = sprite!.ForClass(candidate.Destination),
                    SpriteRequest = checked(entity.SpriteRequest + 1) });
        }
        return (field, party);
    }

    private static BattleSourceLoadout PromotionLoadout(PartyMemberDetails member, BattleActorDefinition destination)
    {
        var loadout = FieldInventory.Loadout(member);
        if (destination.ClassRule != BattleClassRule.Ninja) return loadout;
        // The source rite removes the worn weapon independently of the ring and its slot.
        int slot = BattleEquipment.EquippedSlot(member.Deployment!.Definition, loadout);
        return slot == BattleEquipment.UnarmedChoice ? loadout :
            BattleInventory.SetWord(loadout, slot, (ushort)(loadout.Items[slot] & ~0x80));
    }

    private static IReadOnlyList<PromotionOffer> Candidates(ScenarioDefinition definition, SessionSnapshot current)
    {
        var members = PartyDetailsQuery.Query(definition, current)!.Members
            .OrderBy(m => m.Deployment?.Initialization?.AllyPartyMember ?? int.MaxValue).ToArray();
        return Array.AsReadOnly(members.Select(m =>
        {
            var source = m.Carried.Progress?.ClassId;
            var item = Sf2ClassRules.PromotionItem(source);
            HeldPromotionItem? held = null;
            if (item.HasValue)
                held = members.SelectMany(holder => FieldInventory.Loadout(holder).Items.Select((word, slot) =>
                    new HeldPromotionItem(holder.Carried.Actor, slot, word)).Reverse()).FirstOrDefault(slot => (byte)slot.Word == item);
            var destination = m.Deployment?.RegularPromotion;
            if (destination is not null && definition.PrivateDefinitions is not null)
            {
                var pack = PromotionLoadout(m, destination);
                int slot = BattleEquipment.EquippedSlot(destination, pack);
                int itemId = slot == 4 ? -1 : pack.Items[slot] & 127;
                var key = (m.Carried.Actor, destination.ClassRule, itemId);
                if (definition.EncounterScenes.Count == 0 || definition.EncounterScenes.Values.Any(scene => scene.AllyAppearances?.ContainsKey(key) != true))
                    destination = null;
            }
            return new PromotionOffer(m.Carried.Actor, source, destination?.Growth?.ClassId,
                m.Level ?? 0, held);
        }).ToArray());
    }

    private static bool CurseUnavailable(ScenarioDefinition definition, SessionSnapshot current, ChurchAction action) =>
        action == ChurchAction.Cure && PartyDetailsQuery.Query(definition, current)!.Members.Any(member =>
            CurseRules.Present(member.Carried.Status) && CurseOffer(definition, member) is null);

    private static ushort TreatmentMask(ChurchEffect effect) =>
        effect == ChurchEffect.Poison ? PoisonCureRules.Poison : PoisonCureRules.Stun;

    private static ChurchOffer? CurseOffer(ScenarioDefinition definition, PartyMemberDetails member)
    {
        if (!CurseRules.Present(member.Carried.Status) || definition.PrivateDefinitions is not { } source ||
            member.Deployment is not { } deployment) return null;
        var loadout = FieldInventory.Loadout(member);
        if (BattleEquipment.EquippedRingSlot(deployment.Definition, loadout) is not { } slot ||
            (!BattleEquipment.WornCurse(deployment.Definition, loadout.Items[slot]) ||
             !CaravanDepotQuery.Cursed(definition, loadout.Items[slot]))) return null;

        uint cost = 0;
        foreach (ushort held in loadout.Items)
            if (CaravanDepotQuery.Cursed(definition, held))
                cost += (uint)source.Items[(byte)(held & 127)].Price >> 2;
        return new(member.Carried.Actor, ChurchEffect.Curse, cost, member.Carried.Hp, member.Carried.Status,
            slot, loadout.Items[slot]);
    }

    private static IReadOnlyList<ChurchOffer> Offers(ScenarioDefinition definition, SessionSnapshot current, ChurchDefinition terms, ChurchAction action)
    {
        var members = PartyDetailsQuery.Query(definition, current)!.Members;
        // PartyDetails preserves carried order; private source membership uses its member index order.
        var ordered = members.OrderBy(member => member.Deployment?.Initialization?.AllyPartyMember ?? int.MaxValue).ToArray();
        List<ChurchOffer> offers = [];
        foreach (var effect in action == ChurchAction.Raise ? new[] { ChurchEffect.Raise } :
            new[] { ChurchEffect.Poison, ChurchEffect.Stun, ChurchEffect.Curse })
            foreach (var member in ordered)
            {
                if (member.Carried.Initialized == false || member.Deployment is null)
                    throw new BattleRuleException("service-member-unavailable", "party.member", true);
                if (effect == ChurchEffect.Curse)
                {
                    if (CurseOffer(definition, member) is { } treatment) offers.Add(treatment);
                    continue;
                }
                var actor = member.Carried;
                bool applicable = effect == ChurchEffect.Raise ? actor.Hp == 0 : (actor.Status & TreatmentMask(effect)) != 0;
                if (!applicable) continue;
                uint cost;
                if (effect == ChurchEffect.Raise)
                {
                    if (member.Level is null || member.MaxHp is null or 0 || member.Deployment.Definition.Physical is not { } physical)
                        throw new BattleRuleException("service-raise-definition", "party.member", true);
                    cost = checked((uint)(member.Level.Value * terms.RaisePerLevel + (physical.Promoted ? terms.PromotedExtra : 0)));
                }
                else cost = effect == ChurchEffect.Poison ? terms.PoisonCost : terms.StunCost;
                offers.Add(new(actor.Actor, effect, cost, actor.Hp, actor.Status));
            }
        return offers.AsReadOnly();
    }
}
