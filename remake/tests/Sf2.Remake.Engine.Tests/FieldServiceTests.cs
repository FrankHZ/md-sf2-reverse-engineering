using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed partial class FieldServiceTests
{
    [Theory]
    [InlineData(2499u, true, ChurchResultKind.InsufficientGold)]
    [InlineData(2500u, false, ChurchResultKind.Declined)]
    [InlineData(2500u, true, ChurchResultKind.Paid)]
    public void CureQuotesBothCursedCopiesAndOnlyUnwearsTheSelectedMember(uint gold, bool yes, ChurchResultKind expected)
    {
        var session = Session(gold: gold, hp: 100, status: 4, curseItems: [408, 0, 24, 127]);
        var owner = Member(session, First);
        Choose(session, ChurchAction.Cure);
        Assert.Equal(new ChurchOffer(First, ChurchEffect.Curse, 2500, 100, 4, 0, 408), Wait(session).Offer);
        Answer(session, yes);
        Assert.Equal(expected, Wait(session).Result!.Kind);
        var cured = Member(session, First);
        if (expected == ChurchResultKind.Paid)
        {
            Assert.Equal(0u, session.Current.Exploration!.Party.Gold);
            Assert.Equal(0, cured.Status);
            Assert.Equal(new ushort[] { 280, 0, 24, 127 }, cured.SourceLoadout!.Items);
            Assert.Equal(20, session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == First).Attack!.Value);
        }
        else
        {
            Assert.Same(owner, cured);
            Assert.Equal(gold, session.Current.Exploration!.Party.Gold);
        }
    }

    private static ChurchWait Wait(GameSession session) => Assert.IsType<ChurchWait>(session.Current.Story.Wait);
    private static SessionResult Send(GameSession session, SessionCommand command) =>
        session.Submit(new(session.Current.SessionId, session.Current.Revision, null, command));
    private static SessionResult Accept(GameSession session, SessionCommand command)
    { var result = Send(session, command); Assert.Null(result.Failure); return result; }
    private static void Choose(GameSession session, ChurchAction action) => Accept(session, new ChooseChurch(Wait(session).Token, action));
    private static SessionResult Answer(GameSession session, bool yes = true) => Accept(session, new AnswerChurch(Wait(session).Token, Wait(session).Offer!, yes));
    private static void Continue(GameSession session) => Accept(session, new ContinueChurch(Wait(session).Token));
    private static BattleActorStartInput Member(GameSession session, ActorRef actor) => session.Current.Exploration!.Party.Actors.Single(row => row.Actor == actor);

    [Theory]
    [InlineData(19, false, 5, false)]
    [InlineData(20, true, 0, false)]
    [InlineData(31, false, 3, false)]
    [InlineData(20, true, 7, true)]
    public void OrdinaryPromotionCarriesEarnedStateThroughConsentAndTheNextLevel(byte level, bool armed, ushort hp, bool power)
    {
        var session = Session(hp: hp, currentLevel: level, promotion: true, armed: armed, power: power);
        var before = session.Current.Exploration!.Party;
        Choose(session, ChurchAction.Promote);
        if (level < 20)
        {
            Assert.Equal(ChurchResultKind.NoApplicableMember, Wait(session).Result!.Kind);
            Assert.Same(before, session.Current.Exploration.Party);
            return;
        }
        var row = Wait(session).Candidates!.Single(c => c.Member == First);
        Accept(session, new SelectPromotion(Wait(session).Token, row));
        Accept(session, new AnswerPromotion(Wait(session).Token, row, false));
        Assert.Same(before, session.Current.Exploration.Party);
        Accept(session, new SelectPromotion(Wait(session).Token, row));
        Accept(session, new AnswerPromotion(Wait(session).Token, row, true));
        var yes = new AnswerPromotion(Wait(session).Token, row, true);
        Assert.Single(Accept(session, yes).Observations, row => row.Kind == "church-promoted");
        var after = session.Current.Exploration.Party;
        var member = Member(session, First);
        var prior = before.Actors.Single(a => a.Actor == First);
        Assert.Equal(prior with { Exp = 0, Progress = prior.Progress! with { ClassId = 12, Level = 1, BaseMove = null } }, member);
        Assert.Equal(before.MainSeed, after.MainSeed); Assert.Equal(before.ThinkingSeed, after.ThinkingSeed);
        Assert.Equal(before.Gold, after.Gold); Assert.Same(before.ItemCustody, after.ItemCustody);
        Assert.DoesNotContain(First, after.ActiveAllies!);
        Assert.NotNull(Send(session, yes).Failure); Assert.Same(after, session.Current.Exploration.Party);
        var projected = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == First);
        Assert.Equal((armed ? 25 : 20) + (power ? 5 : 0), projected.Attack!.Value);
        Assert.Equal(BattleClassRule.Hero, projected.Deployment!.Definition.ClassRule);
        Assert.Equal(6, projected.Move!.Value);
        var actor = new BattleActorState(projected.Deployment, hp, member.Mp, 99, null, 0, 0, progress: member.Progress);
        uint seed = before.MainSeed;
        var grown = Sf2.Remake.Domain.Gameplay.Sf2.BattleGrowthRules.Award(actor, 1, ref seed, []);
        Assert.Equal((byte)12, grown.Progress!.ClassId); Assert.Equal(2, grown.Level);
        Assert.Equal(hp, grown.Hp); Assert.Equal(member.Mp, grown.Mp);
        if (power)
        {
            Assert.Equal((ushort)147, grown.SourceLoadout!.Items[1]);
            Assert.Equal(grown.BaseAttack + 10, grown.Attack);
        }
        Assert.Single(grown.Spells, spell => spell.Value == "bolt");
        Assert.Equal(new byte[] { 10, 13, 63, 63 }, grown.SourceLoadout!.Spells);
        Continue(session);
        Accept(session, new SelectPromotion(Wait(session).Token, Wait(session).Candidates!.Single(c => c.Member == First)));
        Assert.Equal(ChurchResultKind.NoPromotion, Wait(session).Result!.Kind);
    }

    [Theory]
    [InlineData(BattleClassRule.UnpromotedKnight)]
    [InlineData(BattleClassRule.UnpromotedWarrior)]
    [InlineData(BattleClassRule.UnpromotedArcher)]
    public void APrivateSpecialDestinationRequiresMountedBattleAppearance(BattleClassRule sourceClass)
    {
        var session = Session(hp: 0, currentLevel: 27, promotion: true, armed: true,
            sourceRule: sourceClass, bindSpecial: true, missingScenes: true);
        var before = session.Current.Exploration!.Party;
        Choose(session, ChurchAction.Promote);
        var offer = Wait(session).Candidates!.Single(c => c.Member == First);
        Assert.NotNull(offer.SpecialItem);
        Assert.Null(offer.SpecialDestination);
        Accept(session, new SelectPromotion(Wait(session).Token, offer));
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));
        Assert.Equal(ChurchResultKind.SpecialUnavailable, Wait(session).Result!.Kind);
        Assert.Same(before, session.Current.Exploration.Party);
    }

    [Theory]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    public void SpecialPromotionConsumesOnlyTheAcceptedMaterial(bool baron, bool selfHolder, bool useSpecial)
    {
        byte regular = baron ? (byte)15 : (byte)13;
        byte special = baron ? (byte)16 : (byte)14;
        ushort material = baron ? (ushort)119 : (ushort)118;
        ushort duplicate = (ushort)(material | BattleInventory.Broken);
        ushort weapon = baron ? (ushort)161 : (ushort)178;
        var session = Session(hp: 0, currentLevel: 27, promotion: true, armed: true,
            sourceRule: baron ? BattleClassRule.UnpromotedWarrior : BattleClassRule.UnpromotedKnight,
            bindSpecial: true, selfHolder: selfHolder);
        var before = session.Current.Exploration!.Party;
        var other = Member(session, Second);
        Choose(session, ChurchAction.Promote);
        var offer = Wait(session).Candidates!.Single(c => c.Member == First);
        Assert.Equal(new HeldPromotionItem(selfHolder ? First : Second, 2, duplicate), offer.SpecialItem);
        Assert.Equal(special, offer.SpecialDestination);
        Accept(session, new SelectPromotion(Wait(session).Token, offer));
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));
        var waiting = session.Current;
        // Keep the original wait and command, but change only the high byte in the live pack.
        var holder = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == offer.SpecialItem!.Holder);
        var altered = FieldInventory.WithLoadout(holder, BattleInventory.SetWord(FieldInventory.Loadout(holder), 2, material));
        var stale = new SessionSnapshot(waiting.SessionId, waiting.Revision, waiting.ObservationSequence,
            new ActiveExploration(waiting.Exploration!.WithParty(FieldInventory.Replace(before, [altered]))),
            waiting.Story, waiting.StopReason);
        var rejected = FieldService.Submit(session.Definition, stale, new AnswerPromotion(Wait(session).Token, offer, true), session.Rules);
        Assert.Equal("stale-promotion", rejected.Failure!.Code);
        Assert.Same(stale, rejected.Snapshot);
        var answer = new AnswerPromotion(Wait(session).Token, offer, useSpecial);
        Accept(session, answer);
        if (!useSpecial)
        {
            Assert.Same(before, session.Current.Exploration.Party);
            Assert.Equal(ChurchPhase.Regular, Wait(session).Phase);
            Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));
        }
        byte classId = useSpecial ? special : regular;
        Assert.Equal(classId, Wait(session).Result!.PromotedClass);
        var member = Member(session, First);
        Assert.Equal(classId, member.Progress!.ClassId);
        Assert.Equal(1, member.Progress.Level);
        Assert.Equal((byte)0, member.Exp);
        Assert.Equal(0, member.Hp);
        Assert.Equal(before.Gold, session.Current.Exploration.Party.Gold);
        Assert.Equal(before.ActiveAllies, session.Current.Exploration.Party.ActiveAllies);
        Assert.Equal(selfHolder ? new ushort[] { material, 0, weapon, 127 } : [weapon, 127, 127, 127], member.Progress.SourceLoadout!.Items);
        if (selfHolder || !useSpecial) Assert.Same(other, Member(session, Second));
        else
        {
            var refreshed = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == Second);
            var pack = refreshed.Carried.SourceLoadout!;
            Assert.Equal(new ushort[] { material, 0, 189, 127 }, pack.Items);
            Assert.Equal(new BattleCurrentStats(12, 42, 8), other.Progress!.CurrentStats);
            Assert.Equal(other with { SourceLoadout = pack, Progress = other.Progress with
                { SourceLoadout = pack, CurrentStats = null } }, refreshed.Carried);
            Assert.Equal((25, 4, 30, 6), ((int)refreshed.Attack!, (int)refreshed.Defense!, (int)refreshed.Agility!, (int)refreshed.Move!));
        }
        var projected = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == First);
        var resolved = projected.Deployment!;
        Assert.Equal(baron ? BattleMover.Regular : useSpecial ? BattleMover.Flying : BattleMover.Centaur, resolved.Definition.Mover);
        if (baron)
        {
            Assert.Equal(useSpecial ? 6 : 5, projected.Move!.Value);
            Assert.Equal(useSpecial, BattleEquipment.IsEligible(resolved.Definition, 58));
            Assert.Equal(useSpecial ? PhysicalCriticalRule.Orc : PhysicalCriticalRule.BrassGunner, resolved.Definition.Physical!.Critical);
            var tables = Enum.GetValues<PhysicalPriorityTable>();
            Assert.Equal(useSpecial ? new[] { 27, 10, 27, 7 } : [28, 11, 28, 6],
                tables.Select(table => PhysicalTargetRules.ClassRank(classId, table)));
        }
        else if (useSpecial)
        {
            PhysicalTargetPriority[] tied = [new(1, 15, classId), new(1, 15, 13)];
            foreach (var (table, rank, target) in new[] {
                (PhysicalPriorityTable.Regular, 11, 0), (PhysicalPriorityTable.Flying, 23, 1),
                (PhysicalPriorityTable.Archer, 3, 0), (PhysicalPriorityTable.Mage, 30, 1) })
            {
                Assert.Equal(rank, PhysicalTargetRules.ClassRank(classId, table));
                Assert.Equal(target, PhysicalTargetRules.Select(tied, table)!.Value.Index);
            }
        }
        uint seed = before.MainSeed;
        var grown = BattleGrowthRules.Award(new BattleActorState(resolved, 1, 0, 99, null, 0, 0,
            progress: member.Progress), 1, ref seed, []);
        Assert.Equal(2, grown.Level);
        Assert.Equal(classId, grown.Progress!.ClassId);
        var published = session.Current;
        Assert.NotNull(Send(session, answer).Failure);
        Assert.Same(published, session.Current);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(false, true)]
    public void HeldSpecialItemRequiresItsOwnAnswerBeforeOrdinaryPromotion(bool acceptSpecial, bool acceptRegular)
    {
        var session = Session(hp: 0, currentLevel: 20, promotion: true, armed: true, power: true,
            sourceRule: BattleClassRule.UnpromotedKnight);
        var before = session.Current.Exploration!.Party;
        var beforeFlags = session.Current.Story.Flags.ToArray();

        Choose(session, ChurchAction.Promote);
        var candidate = Wait(session).Candidates!.Single(offer => offer.Member == First);
        Assert.Equal(new HeldPromotionItem(Second, 0, 118), candidate.SpecialItem);
        Accept(session, new SelectPromotion(Wait(session).Token, candidate));
        Accept(session, new AnswerPromotion(Wait(session).Token, candidate, true));
        Assert.Equal(ChurchPhase.Special, Wait(session).Phase);

        var special = new AnswerPromotion(Wait(session).Token, candidate, acceptSpecial);
        Assert.DoesNotContain(Accept(session, special).Observations, row => row.Kind == "church-promoted");
        Assert.Same(before, session.Current.Exploration.Party);
        Assert.NotNull(Send(session, special).Failure);
        Assert.Same(before, session.Current.Exploration.Party);

        if (acceptSpecial)
        {
            Assert.Equal(ChurchResultKind.SpecialUnavailable, Wait(session).Result!.Kind);
            return;
        }

        Assert.Equal(ChurchPhase.Regular, Wait(session).Phase);
        var regular = Accept(session, new AnswerPromotion(Wait(session).Token, candidate, acceptRegular));
        Assert.Equal(acceptRegular ? 1 : 0, regular.Observations.Count(row => row.Kind == "church-promoted"));
        if (!acceptRegular)
        {
            Assert.Equal(ChurchPhase.Members, Wait(session).Phase);
            Assert.Same(before, session.Current.Exploration.Party);
            return;
        }

        Assert.Equal(ChurchResultKind.Promoted, Wait(session).Result!.Kind);
        var after = session.Current.Exploration.Party;
        var prior = before.Actors.Single(actor => actor.Actor == First);
        Assert.Equal(prior with { Exp = 0, Progress = prior.Progress! with { ClassId = 13, Level = 1, BaseMove = null } },
            Member(session, First));
        Assert.Same(before.Actors.Single(actor => actor.Actor == Second), Member(session, Second));
        Assert.Equal(beforeFlags, session.Current.Story.Flags.ToArray());
        Assert.Equal(before.Gold, after.Gold);
        Assert.Equal(before.MainSeed, after.MainSeed);
        Assert.Equal(before.ThinkingSeed, after.ThinkingSeed);
        Assert.DoesNotContain(First, after.ActiveAllies!);
        var projected = session.QueryPartyDetails()!.Members.Single(member => member.Carried.Actor == First);
        Assert.Equal(BattleClassRule.Paladin, projected.Deployment!.Definition.ClassRule);
        Assert.Equal(BattleMover.Centaur, projected.Deployment.Definition.Mover);
        Assert.Equal(7, projected.Move!.Value);
        Assert.Equal(30, projected.Attack!.Value);
    }

    [Theory]
    [InlineData(24, new byte[] { 75, 76, 63, 63 })]
    [InlineData(29, new byte[] { 139, 71, 6, 72 })]
    public void WizardPromotionRetainsTheMembersBookAndRodAfterDecliningASeparateHoldersBook(byte level, byte[] book)
    {
        var session = Session(hp: 27, currentLevel: level, promotion: true, armed: true,
            sourceRule: BattleClassRule.UnpromotedMage);
        var prior = Member(session, First);
        var holder = Member(session, Second);
        var mage = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == First).Deployment!.Definition;
        Assert.False(BattleEquipment.IsEligible(mage, 24));
        Assert.Equal(10, mage.CastPower(10));

        Choose(session, ChurchAction.Promote);
        var offer = Wait(session).Candidates!.Single(c => c.Member == First);
        Assert.Equal(new HeldPromotionItem(Second, 0, 121), offer.SpecialItem);
        Accept(session, new SelectPromotion(Wait(session).Token, offer));
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));
        Assert.Equal(ChurchPhase.Special, Wait(session).Phase);
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, false));
        Assert.Equal(ChurchPhase.Regular, Wait(session).Phase);
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));

        var member = Member(session, First);
        Assert.Same(holder, Member(session, Second));
        Assert.Equal(prior with { Exp = 0, Progress = prior.Progress! with { ClassId = 17, Level = 1, BaseMove = null } }, member);
        var deployment = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == First).Deployment!;
        var wizard = deployment.Definition;
        Assert.Equal(BattleClassRule.Wizard, wizard.ClassRule);
        Assert.Equal(BattleMover.Mage, wizard.Mover);
        Assert.Equal(5, wizard.Move);
        Assert.Equal(12, wizard.CastPower(10));
        Assert.True(BattleEquipment.IsEligible(wizard, 24));
        var actor = new BattleActorState(deployment, member.Hp, member.Mp, 99, null, 0, 0, progress: member.Progress);
        Assert.Equal((byte)85, BattleEquipment.EquippedWeaponItem(actor));
        uint seed = session.Current.Exploration!.Party.MainSeed;
        var grown = Sf2.Remake.Domain.Gameplay.Sf2.BattleGrowthRules.Award(actor, 1, ref seed, []);
        Assert.Equal(2, grown.Level);
        Assert.Equal(book, grown.SourceLoadout!.Spells);
        Assert.Equal(actor.Spells, grown.Spells);
    }

    [Theory]
    [InlineData(BattleClassRule.UnpromotedWarrior, 21, 15, BattleClassRule.Gladiator, BattleMover.Regular, 5, 33, 3, 15, true)]
    [InlineData(BattleClassRule.UnpromotedWarrior, 24, 15, BattleClassRule.Gladiator, BattleMover.Regular, 5, 36, 3, 15, true)]
    [InlineData(BattleClassRule.UnpromotedBirdman, 23, 23, BattleClassRule.BirdBattler, BattleMover.Flying, 6, 58, 2, 0, false)]
    public void RegularMeleePromotionKeepsWeaponAndEarnedStats(BattleClassRule source, byte level,
        byte classId, BattleClassRule destination, BattleMover mover, byte move, byte weapon,
        int forestCost, int forestProtection, bool warrior)
    {
        var session = Session(hp: warrior ? (ushort)27 : (ushort)0, currentLevel: level, promotion: true,
            armed: true, power: warrior, sourceRule: source, warriorAxe: weapon);
        var prior = Member(session, First);
        var other = Member(session, Second);
        Choose(session, ChurchAction.Promote);
        var offer = Wait(session).Candidates!.Single(candidate => candidate.Member == First);
        Assert.Equal(warrior ? new HeldPromotionItem(Second, 0, 119) : null, offer.SpecialItem);
        Accept(session, new SelectPromotion(Wait(session).Token, offer));
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));
        if (warrior)
        {
            Assert.Equal(ChurchPhase.Special, Wait(session).Phase);
            Accept(session, new AnswerPromotion(Wait(session).Token, offer, false));
        }
        Assert.Equal(ChurchPhase.Regular, Wait(session).Phase);
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));

        var member = Member(session, First);
        Assert.Same(other, Member(session, Second));
        Assert.Equal(prior with { Exp = 0, Progress = prior.Progress! with { ClassId = classId, Level = 1, BaseMove = null } }, member);
        var projection = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == First);
        var deployment = projection.Deployment!;
        var physical = deployment.Definition;
        Assert.Equal(destination, physical.ClassRule);
        Assert.Equal(move, physical.Move);
        Assert.Equal(mover, physical.Mover);
        var forest = new BattleTerrain(TerrainSurface.Rough, TerrainProtection.Light);
        Assert.Equal(forestCost, BattleTerrainRules.MovementCost(forest, mover));
        Assert.Equal(forestProtection, BattleTerrainRules.LandEffect(forest, mover));
        int weaponBonus = weapon switch { 36 => 17, 58 => 8, _ => 5 };
        Assert.Equal(20 + weaponBonus + (warrior ? 5 : 0), projection.Attack!.Value);
        Assert.Empty(physical.Spells);
        var actor = new BattleActorState(deployment, member.Hp, member.Mp, 99, null, 0, 0, progress: member.Progress);
        Assert.Equal(weapon, BattleEquipment.EquippedWeaponItem(actor)!.Value);
        uint seed = session.Current.Exploration!.Party.MainSeed;
        var grown = Sf2.Remake.Domain.Gameplay.Sf2.BattleGrowthRules.Award(actor, 1, ref seed, []);
        Assert.Equal(2, grown.Level);
        Assert.Equal(classId, grown.Progress!.ClassId);
        Assert.Equal(member.Progress!.SourceLoadout!.Spells, grown.SourceLoadout!.Spells);
        var unarmed = BattleEquipment.Equip(physical, grown.SourceLoadout, BattleEquipment.UnarmedChoice);
        Assert.Equal(weapon, unarmed.Items[0]);
        Assert.Equal(warrior ? 5 : 0, BattleEquipment.Contribution(physical, unarmed));
    }

    [Theory]
    [InlineData(BattleClassRule.UnpromotedArcher, 22, 21, BattleClassRule.Sniper, BattleMover.Archer, 3, true)]
    [InlineData(BattleClassRule.UnpromotedArcher, 26, 21, BattleClassRule.Sniper, BattleMover.Archer, 3, true)]
    [InlineData(BattleClassRule.UnpromotedRanger, 21, 25, BattleClassRule.BowKnight, BattleMover.CentaurArcher, 5, false)]
    [InlineData(BattleClassRule.UnpromotedArcher, 22, 22, BattleClassRule.BrassGunner, BattleMover.BrassGunner, 3, true)]
    public void RangedPromotionPreservesTheArrowAndChangesPromotedOnlyShotEligibility(BattleClassRule source,
        byte level, byte classId, BattleClassRule destination, BattleMover mover, int roughCost, bool archer)
    {
        bool gunner = classId == 22;
        var session = Session(hp: 27, currentLevel: level, promotion: true, armed: true, sourceRule: source, bindSpecial: gunner);
        var prior = Member(session, First);
        var holder = Member(session, Second);
        var original = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == First).Deployment!;
        Assert.False(BattleEquipment.IsEligible(original.Definition, 50));

        Choose(session, ChurchAction.Promote);
        var offer = Wait(session).Candidates!.Single(c => c.Member == First);
        Assert.Equal(archer ? new HeldPromotionItem(Second, gunner ? 2 : 0, gunner ? (ushort)(120 | BattleInventory.Broken) : (ushort)120) : null, offer.SpecialItem);
        Accept(session, new SelectPromotion(Wait(session).Token, offer));
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));
        if (archer)
        {
            Assert.Equal(ChurchPhase.Special, Wait(session).Phase);
            Accept(session, new AnswerPromotion(Wait(session).Token, offer, gunner));
        }
        if (!gunner)
        {
            Assert.Equal(ChurchPhase.Regular, Wait(session).Phase);
            Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));
        }

        var member = Member(session, First);
        if (gunner) Assert.Equal(new ushort[] { 120, 0, 189, 127 }, Member(session, Second).SourceLoadout!.Items);
        else Assert.Same(holder, Member(session, Second));
        Assert.Equal(prior with { Exp = 0, Progress = prior.Progress! with { ClassId = classId, Level = 1, BaseMove = null } }, member);
        var deployment = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == First).Deployment!;
        var definition = deployment.Definition;
        Assert.Equal(destination, definition.ClassRule);
        Assert.Equal(gunner ? 5 : 6, definition.Move);
        Assert.Equal(mover, definition.Mover);
        var rough = new BattleTerrain(TerrainSurface.Rough, TerrainProtection.Light);
        Assert.Equal(roughCost, BattleTerrainRules.MovementCost(rough, mover));
        Assert.Equal(15, BattleTerrainRules.LandEffect(rough, mover));
        Assert.True(BattleEquipment.IsEligible(definition, 50));
        Assert.Empty(definition.Spells);

        var actor = new BattleActorState(deployment, member.Hp, member.Mp, 99, null, 0, 0, progress: member.Progress);
        Assert.Equal((byte)47, BattleEquipment.EquippedWeaponItem(actor));
        AssertRange(actor, 2, 2);
        var switched = BattleEquipment.Equip(definition, actor.SourceLoadout!, 2);
        Assert.Equal(new ushort[] { 47, 127, 178, 127 }, switched.Items);
        AssertRange(actor.With(sourceLoadout: switched), 2, 3);
        var unarmed = BattleEquipment.Equip(definition, switched, BattleEquipment.UnarmedChoice);
        Assert.Equal(new ushort[] { 47, 127, 50, 127 }, unarmed.Items);
        AssertRange(actor.With(sourceLoadout: unarmed), 1, 1);
        uint seed = session.Current.Exploration!.Party.MainSeed;
        var grown = Sf2.Remake.Domain.Gameplay.Sf2.BattleGrowthRules.Award(actor, 1, ref seed, []);
        Assert.Equal(2, grown.Level);
        Assert.Equal(classId, grown.Progress!.ClassId);
        Assert.Empty(grown.Spells);

        static void AssertRange(BattleActorState candidate, byte minimum, byte maximum)
        {
            var range = BattleEquipment.Physical(candidate)!;
            Assert.Equal(minimum, range.MinimumRange);
            Assert.Equal(maximum, range.MaximumRange);
            Assert.Equal(minimum == 1, BattleRange.Contains(new(5, 5), new(5, 6), range.MinimumRange, range.MaximumRange));
            Assert.Equal(maximum == 3, BattleRange.Contains(new(5, 5), new(6, 7), range.MinimumRange, range.MaximumRange));
        }
    }

    [Theory]
    [InlineData(40, 86, new byte[] { 192, 130, 206, 68 })]
    [InlineData(28, 89, new byte[] { 128, 142, 3, 63 })]
    public void VicarKeepsThePriestsCurrentBookAndLearnsFromTheMemberSchedule(byte level, byte rod, byte[] book)
    {
        var session = Session(hp: 27, currentLevel: level, promotion: true, armed: true,
            sourceRule: BattleClassRule.UnpromotedPriest);
        var prior = Member(session, First);
        Choose(session, ChurchAction.Promote);
        var offer = Wait(session).Candidates!.Single(candidate => candidate.Member == First);
        Accept(session, new SelectPromotion(Wait(session).Token, offer));
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));
        Assert.Equal(ChurchPhase.Regular, Wait(session).Phase);
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));

        var member = Member(session, First);
        Assert.Equal(prior with { Exp = 0, Progress = prior.Progress! with { ClassId = 19, Level = 1, BaseMove = null } }, member);
        Assert.Equal(book, member.Progress!.SourceLoadout!.Spells);
        var deployment = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == First).Deployment!;
        Assert.Equal((BattleClassRule.Vicar, BattleMover.Healer, (byte)5),
            (deployment.Definition.ClassRule, deployment.Definition.Mover, deployment.Definition.Move));
        var actor = new BattleActorState(deployment, member.Hp, member.Mp, 99, null, 0, 0, progress: member.Progress);
        Assert.Equal(rod, BattleEquipment.EquippedWeaponItem(actor));
        uint seed = session.Current.Exploration!.Party.MainSeed;
        var grown = Sf2.Remake.Domain.Gameplay.Sf2.BattleGrowthRules.Award(actor, 1, ref seed, []);
        Assert.Equal(2, grown.Level);
        Assert.Equal(book, grown.SourceLoadout!.Spells);

        if (level == 28)
        {
            var beforeBoost = actor.With(progress: member.Progress with { Level = 9 });
            var withBoost = Sf2.Remake.Domain.Gameplay.Sf2.BattleGrowthRules.Award(beforeBoost, 1, ref seed, []);
            Assert.Equal(10, withBoost.Level);
            Assert.Equal(new byte[] { 128, 142, 67, 63 }, withBoost.SourceLoadout!.Spells);
            var beforeAura = withBoost.With(exp: 99, progress: withBoost.Progress! with { Level = 12 });
            var withAura = Sf2.Remake.Domain.Gameplay.Sf2.BattleGrowthRules.Award(beforeAura, 1, ref seed, []);
            Assert.Equal(13, withAura.Level);
            Assert.Equal(new byte[] { 128, 142, 67, 1 }, withAura.SourceLoadout!.Spells);
            Assert.Contains(new SpellRef("aura", 1), withAura.Spells);
        }
    }

    [Theory]
    [InlineData(19, true)]
    [InlineData(20, true)]
    [InlineData(33, false)]
    public void NinjaRiteClearsOnlyTheWornKnifeAndLearnsItsOwnSpellRanks(byte level, bool worn)
    {
        var session = Session(hp: 27, currentLevel: level, promotion: true, armed: worn, power: true,
            sourceRule: BattleClassRule.UnpromotedThief);
        var prior = Member(session, First);
        Choose(session, ChurchAction.Promote);
        if (level < 20)
        {
            Assert.Equal(ChurchResultKind.NoApplicableMember, Wait(session).Result!.Kind);
            Assert.Equal(prior, Member(session, First));
            return;
        }
        var offer = Assert.Single(Wait(session).Candidates!, row => row.Member == First);
        Accept(session, new SelectPromotion(Wait(session).Token, offer));
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));
        Assert.Equal(ChurchPhase.Regular, Wait(session).Phase);
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));

        var member = Member(session, First);
        var loadout = member.Progress!.SourceLoadout!;
        Assert.Equal(new ushort[] { 103, 147, 58, 127 }, loadout.Items);
        Assert.Equal(new byte[] { 63, 63, 63, 63 }, loadout.Spells);
        Assert.Equal(prior with { Exp = 0, SourceLoadout = loadout, Progress = prior.Progress! with
            { ClassId = 27, Level = 1, BaseMove = null, SourceLoadout = loadout } }, member);
        var deployment = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == First).Deployment!;
        var actor = new BattleActorState(deployment, member.Hp, member.Mp, 99, new(5, 5), 0, 0, progress: member.Progress);
        Assert.Equal((BattleClassRule.Ninja, BattleMover.Stealth, (byte)7),
            (actor.Definition.ClassRule, actor.Definition.Mover, actor.Move));
        Assert.Equal(0, actor.MaxMp);
        Assert.Null(BattleEquipment.EquippedWeaponItem(actor));
        Assert.Equal(5, BattleEquipment.Contribution(actor.Definition, loadout));
        Assert.Throws<BattleRuleException>(() => BattleEquipment.Equip(actor, 0));
        var armed = BattleEquipment.Equip(actor, 2);
        Assert.Equal((byte)58, BattleEquipment.EquippedWeaponItem(armed));
        Assert.Equal((ushort)147, armed.SourceLoadout!.Items[1]);
        Assert.Equal(actor.BaseAttack + 5 + 8, armed.Attack);
        var prowess = BattleEquipment.Physical(actor)!.Critical;
        Assert.Equal(((ushort)8, 1, (ushort)16, (ushort)8),
            (prowess.ChanceDenominator, prowess.DamageBonusShift, prowess.DoubleChanceDenominator, prowess.CounterChanceDenominator));

        uint seed = 0;
        byte[][] expected = [[27, 63, 63, 63], [91, 63, 63, 63], [91, 28, 63, 63],
            [91, 92, 63, 63], [155, 92, 63, 63], [155, 156, 63, 63]];
        byte[] thresholds = [4, 9, 14, 19, 25, 32];
        for (int index = 0; index < thresholds.Length; index++)
        {
            var before = armed.With(exp: 99, progress: armed.Progress! with { Level = (byte)(thresholds[index] - 1) });
            armed = BattleGrowthRules.Award(before, 1, ref seed, []);
            Assert.Equal(thresholds[index], armed.Level);
            Assert.Equal(expected[index], armed.SourceLoadout!.Spells);
            Assert.Equal(new ushort[] { 103, 147, 186, 127 }, armed.SourceLoadout.Items);
        }
        Assert.Equal(6, armed.Spells.Count);
    }

    [Fact]
    public void MonsterRiteKeepsEmptyBookAndUpdatesHoveringResistanceAndCounter()
    {
        var session = Session(hp: 9, currentLevel: 20, promotion: true, power: true,
            sourceRule: BattleClassRule.UnpromotedTortoise);
        var prior = Member(session, First);
        Choose(session, ChurchAction.Promote);
        var offer = Assert.Single(Wait(session).Candidates!, row => row.Member == First);
        Accept(session, new SelectPromotion(Wait(session).Token, offer));
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));
        var after = Member(session, First);
        Assert.Equal(prior with { Exp = 0, Progress = prior.Progress! with
            { ClassId = 28, Level = 1, BaseMove = null } }, after);
        var deployment = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == First).Deployment!;
        var actor = new BattleActorState(deployment, after.Hp, after.Mp, after.Exp, new(5, 5), 0, 0, progress: after.Progress);
        Assert.Equal((BattleClassRule.Monster, BattleMover.Hovering, (byte)7),
            (actor.Definition.ClassRule, actor.Definition.Mover, actor.Move));
        Assert.Equal((ushort)0xAA00, actor.Definition.Resistance);
        Assert.Equal((ushort)16, BattleEquipment.Physical(actor)!.Critical.CounterChanceDenominator);
        Assert.Null(BattleEquipment.EquippedWeaponItem(actor));
        Assert.Equal(5, BattleEquipment.Contribution(actor.Definition, actor.SourceLoadout));
        Assert.Equal(new byte[] { 63, 63, 63, 63 }, actor.SourceLoadout!.Spells);
        Assert.Empty(actor.Spells);
        Assert.Equal(0, actor.MaxMp);
        var water = new BattleTerrain(TerrainSurface.Water, TerrainProtection.None);
        Assert.Equal((sbyte)2, BattleTerrainRules.MovementCost(water, actor.Definition.Mover));
        Assert.Equal((sbyte)-1, BattleTerrainRules.MovementCost(water, BattleMover.Stealth));
    }

    [Theory]
    [InlineData(BattleClassRule.UnpromotedWolfman, BattleClassRule.WolfBaron, 29, 24, 5, 163u)]
    [InlineData(BattleClassRule.UnpromotedPhoenix, BattleClassRule.Phoenix, 31, 26, 7, 189u)]
    public void InnatePromotionChangesCounterAndMovementWhileRetainingUnarmedLoadout(
        BattleClassRule source, BattleClassRule promotedClass, byte level, byte classId, byte move, uint counterSeed)
    {
        bool phoenix = source == BattleClassRule.UnpromotedPhoenix;
        var session = Session(hp: 27, currentLevel: level, promotion: true, sourceRule: source, power: !phoenix);
        var prior = Member(session, First);
        var initial = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == First).Deployment!;
        Choose(session, ChurchAction.Promote);
        var offer = Wait(session).Candidates!.Single(candidate => candidate.Member == First);
        Accept(session, new SelectPromotion(Wait(session).Token, offer));
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));
        Assert.Equal(ChurchPhase.Regular, Wait(session).Phase);
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));

        var member = Member(session, First);
        Assert.Equal(prior with { Exp = 0, Progress = prior.Progress! with { ClassId = classId, Level = 1, BaseMove = null } }, member);
        var promoted = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == First).Deployment!;
        Assert.Equal(promotedClass, promoted.Definition.ClassRule);
        Assert.Equal(phoenix ? BattleMover.Flying : BattleMover.Stealth, promoted.Definition.Mover);
        Assert.Equal(move, promoted.Definition.Move);
        var before = new BattleActorState(initial, prior.Hp, prior.Mp, prior.Exp, null, 0, 0, progress: prior.Progress);
        var after = new BattleActorState(promoted, member.Hp, member.Mp, 99, null, 0, 0, progress: member.Progress);
        Assert.Empty(after.Spells);
        Assert.Null(BattleEquipment.EquippedWeaponItem(after));
        var unarmed = BattleEquipment.Physical(after)!;
        Assert.Equal(((byte)1, (byte)1), (unarmed.MinimumRange, unarmed.MaximumRange));
        Assert.Equal((ushort)32, unarmed.Critical.DoubleChanceDenominator);
        Assert.False(Counters(before, counterSeed));
        Assert.True(Counters(after, counterSeed));

        var grass = new BattleTerrain(TerrainSurface.Brush, TerrainProtection.Heavy);
        var desert = new BattleTerrain(TerrainSurface.Desert, TerrainProtection.Heavy);
        Assert.Equal((sbyte)2, BattleTerrainRules.MovementCost(grass, after.Definition.Mover));
        Assert.Equal(phoenix ? 0 : 30, BattleTerrainRules.LandEffect(grass, after.Definition.Mover));
        if (phoenix)
        {
            Assert.Equal(30, BattleTerrainRules.LandEffect(grass, before.Definition.Mover));
            Assert.Equal(0, BattleTerrainRules.LandEffect(desert, before.Definition.Mover));
            Assert.Equal(0, BattleTerrainRules.LandEffect(desert, after.Definition.Mover));
            var terrain = Enumerable.Repeat(new BattleTerrain(TerrainSurface.Barrier, TerrainProtection.None), 48 * 48).ToArray();
            for (int x = 5; x <= 12; x++) terrain[5 * 48 + x] = new(TerrainSurface.Open, TerrainProtection.None);
            terrain[4 * 48 + 5] = new(TerrainSurface.HighSky, TerrainProtection.None);
            MapPosition origin = new(5, 5), sky = new(5, 4), distant = new(12, 5);
            var hovering = BattleTerrainRules.ReachableCells(terrain, origin, before.Definition.Mover, before.Definition.Move, []);
            var flying = BattleTerrainRules.ReachableCells(terrain, origin, after.Definition.Mover, after.Definition.Move, []);
            Assert.DoesNotContain(sky, hovering); Assert.Contains(sky, flying);
            Assert.DoesNotContain(distant, hovering); Assert.Contains(distant, flying);
            var blaze = new AttackSpellDefinition(new("blaze", 2), 6, 10, 1, 2, 1, AttackSpellElement.Fire);
            Assert.Equal(10, Sf2AttackSpellAction.PotentialDamage(before, before, blaze));
            Assert.Equal(5, Sf2AttackSpellAction.PotentialDamage(before, after, blaze));
            var sleep = new SupportSpellDefinition(new("sleep", 1), 4, 1, 2, 1);
            Assert.NotNull(sleep.Resolve(before, _ => 6).ReactionWord);
            Assert.Null(sleep.Resolve(after, _ => 6).ReactionWord);
            Assert.NotNull(sleep.Resolve(after, _ => 7).ReactionWord);
        }
        else
        {
            Assert.Equal((ushort)147, after.SourceLoadout!.Items[1]);
            Assert.Equal(5, BattleEquipment.Contribution(after.Definition, after.SourceLoadout));
        }
        uint seed = session.Current.Exploration!.Party.MainSeed;
        var grown = BattleGrowthRules.Award(after, 1, ref seed, []);
        Assert.Equal(2, grown.Level);
        Assert.Equal(classId, grown.Progress!.ClassId);

        static bool Counters(BattleActorState defender, uint word)
        {
            var impact = PhysicalStrikeRules.Resolve(30, 20, 60, 256, (word << 16) | 0x1234, 32, 16, 2);
            var doubleDraw = BattleRandom.NextMain(impact.Seed, 32);
            return BattleRandom.NextMain(doubleDraw.After, defender.Definition.Physical!.Critical.CounterChanceDenominator).Value == 0;
        }
    }

    [Theory]
    [InlineData(2, false, 250, 20, 200, 0x30)]
    [InlineData(7, true, 90, 270, 90, 0x38)]
    public void RaiseUsesCurrentLevelPromotionCapAndExactFundsWithoutResettingTheParty(
        byte level, bool promoted, ushort maxHp, uint cost, ushort raisedHp, ushort status)
    {
        var session = Session(gold: cost, level: level, promoted: promoted, maxHp: maxHp, status: status);
        var before = session.Current.Exploration!.Party;
        Choose(session, ChurchAction.Raise);
        Assert.Equal(cost, Wait(session).Offer!.Cost); Assert.Same(before, session.Current.Exploration!.Party);
        var result = Answer(session);
        Assert.Contains(result.Observations, row => row.Kind == "church-paid");
        Assert.Equal(raisedHp, Member(session, First).Hp); Assert.Equal(0u, session.Current.Exploration!.Party.Gold);
        Assert.Equal(before.Actors.Single(row => row.Actor == First) with { Hp = raisedHp }, Member(session, First));
        Assert.Same(before.Actors.Single(row => row.Actor == Second), Member(session, Second));
        Assert.Equal(before.MainSeed, session.Current.Exploration.Party.MainSeed);
        Assert.Equal(before.ThinkingSeed, session.Current.Exploration.Party.ThinkingSeed);
        Assert.Same(before.ItemCustody, session.Current.Exploration.Party.ItemCustody);
        Continue(session); Accept(session, new LeaveChurch(Wait(session).Token));
        Assert.True(session.Current.CanWaitAtInput); Assert.Contains(999, session.Current.Story.Flags);
        Assert.Empty(session.Current.Story.Callers);
    }

    [Fact]
    public void CureScansPoisonThenStunAndRechecksTheChangedStatusForLaterOffers()
    {
        var session = Session(gold: 60, hp: 0, status: 3, secondStatus: 3);
        var before = session.Current.Exploration!.Party;
        Choose(session, ChurchAction.Cure);
        foreach (var (actor, effect, gold) in new[] { (First, ChurchEffect.Poison, 50u), (Second, ChurchEffect.Poison, 40u),
            (First, ChurchEffect.Stun, 20u), (Second, ChurchEffect.Stun, 0u) })
        {
            Assert.Equal(actor, Wait(session).Offer!.Member); Assert.Equal(effect, Wait(session).Offer!.Effect);
            Answer(session); Assert.Equal(gold, session.Current.Exploration!.Party.Gold); Continue(session);
        }
        Assert.Null(Wait(session).Action); Assert.Equal(0, Member(session, First).Status);
        Assert.Equal(0, Member(session, First).Hp); Assert.Equal(0, Member(session, Second).Status);
        Assert.Equal(before.Actors.Select(row => row.Exp), session.Current.Exploration!.Party.Actors.Select(row => row.Exp));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void DeclineAndInsufficientRaisePreserveEveryResourceAndContinueToTheNextDeadMember(bool insufficient)
    {
        var session = Session(gold: 19, secondHp: 0); var before = session.Current.Exploration!.Party;
        Choose(session, ChurchAction.Raise); var result = Answer(session, insufficient);
        Assert.Contains(result.Observations, row => row.Kind == (insufficient ? "church-insufficient-gold" : "church-declined"));
        Assert.Same(before, session.Current.Exploration!.Party); Continue(session);
        Assert.Equal(Second, Wait(session).Offer!.Member);
    }

    [Fact]
    public void InsufficientCureSkipsRemainingPoisonMembersThenStillOffersStun()
    {
        var session = Session(gold: 9, hp: 100, status: 3, secondStatus: 2);
        Choose(session, ChurchAction.Cure); Answer(session); Continue(session);
        Assert.Equal(ChurchEffect.Stun, Wait(session).Offer!.Effect);
        Assert.Equal(3, Member(session, First).Status); Assert.Equal(2, Member(session, Second).Status);
    }

    [Theory]
    [InlineData(ChurchAction.Raise)] [InlineData(ChurchAction.Cure)]
    public void NoApplicableTargetAndRetryKeepControlsAndParty(ChurchAction action)
    {
        var session = Session(hp: 100); var before = session.Current.Exploration!.Party;
        Choose(session, action); Assert.Null(Wait(session).Offer); Assert.Equal(ChurchResultKind.NoApplicableMember, Wait(session).Result!.Kind);
        Assert.Equal(ChurchPhase.Result, Wait(session).Phase);
        Continue(session); Choose(session, action); Continue(session);
        Accept(session, new LeaveChurch(Wait(session).Token, true));
        Assert.Same(before, session.Current.Exploration!.Party); Assert.True(session.Current.CanWaitAtInput);
    }

    [Fact]
    public void InvalidStaleAndDuplicateConfirmationCannotPayTwice()
    {
        var session = Session(); Choose(session, ChurchAction.Raise); var before = session.Current; var wait = Wait(session);
        foreach (var command in new ChurchCommand[] { new AnswerChurch(wait.Token, wait.Offer! with { Cost = 0 }, true),
            new AnswerChurch(new(wait.Token.Value - 1), wait.Offer!, true), new ChooseChurch(wait.Token, ChurchAction.Cure),
            new LeaveChurch(wait.Token) })
        { Assert.NotNull(Send(session, command).Failure); Assert.Same(before, session.Current); }
        var envelope = new CommandEnvelope(before.SessionId, before.Revision, null, new AnswerChurch(wait.Token, wait.Offer!, true));
        Assert.Null(session.Submit(envelope).Failure); var paid = session.Current;
        Assert.Equal("stale-input", session.Submit(envelope).Failure!.Code);
        Assert.NotNull(Send(session, envelope.Command).Failure); Assert.Same(paid, session.Current);
    }

    [Fact]
    public void ResultPhaseRejectsFreshTokenAnswersUntilContinueWithoutAnotherPayment()
    {
        var session = Session(status: 7); Choose(session, ChurchAction.Cure);
        var offered = Wait(session).Offer!;
        Assert.Equal(ChurchPhase.Offer, Wait(session).Phase);
        Assert.NotNull(Send(session, new ContinueChurch(Wait(session).Token)).Failure);
        Answer(session); var paid = session.Current; var result = Wait(session);
        Assert.Equal(ChurchPhase.Result, result.Phase);
        Assert.Equal(new ChurchResult(ChurchResultKind.Paid, offered), result.Result);
        Assert.False(result.CurseUnavailable); // Stun is still queued; curse notice is independent at completion.
        Assert.NotNull(Send(session, new AnswerChurch(result.Token, result.Offer!, true)).Failure);
        Assert.Same(paid, session.Current);
        Continue(session); Assert.Equal(ChurchPhase.Offer, Wait(session).Phase);
        Assert.Equal(ChurchEffect.Stun, Wait(session).Offer!.Effect);
        Answer(session, false);
        Assert.Equal(ChurchResultKind.Declined, Wait(session).Result!.Kind);
        Assert.True(Wait(session).CurseUnavailable);
        Continue(session); Assert.Equal(ChurchPhase.Menu, Wait(session).Phase);
        Assert.Null(Wait(session).Result); Assert.False(Wait(session).CurseUnavailable);
        Assert.Equal(90u, session.Current.Exploration!.Party.Gold);
        Assert.Equal(5, Member(session, First).Status);
    }

    [Fact]
    public void MissingGoldRejectsAtomicallyAndAllowsDeclineAndReturn()
    {
        var session = Session(gold: null); Choose(session, ChurchAction.Raise); var before = session.Current; var wait = Wait(session);
        Assert.Equal("service-gold-unavailable", Send(session, new AnswerChurch(wait.Token, wait.Offer!, true)).Failure!.Code);
        Assert.Same(before, session.Current); Answer(session, false); Continue(session);
        Accept(session, new LeaveChurch(Wait(session).Token)); Assert.True(session.Current.CanWaitAtInput);
    }

    [Fact]
    public void ContentOperandsAreConsumedAndUnsupportedCurseIsExplicit()
    {
        var session = Session(gold: 7, hp: 100, status: 6, terms: new(4, 50, 80, 7, 11));
        Choose(session, ChurchAction.Cure); Assert.Equal(7u, Wait(session).Offer!.Cost); Answer(session);
        Assert.Equal(4, Member(session, First).Status); Assert.True(Wait(session).CurseUnavailable);
        Assert.Equal(new ChurchResult(ChurchResultKind.Paid, new(First, ChurchEffect.Poison, 7, 100, 6)), Wait(session).Result);
        Continue(session); Choose(session, ChurchAction.Cure); Assert.Null(Wait(session).Offer);
    }

    [Fact]
    public void CurrentProgressSuppliesRaiseCostAndMaximumInsteadOfTheDeploymentBaseline()
    {
        var session = Session(gold: 40, level: 1, maxHp: 250, currentLevel: 4);
        Choose(session, ChurchAction.Raise); Assert.Equal(40u, Wait(session).Offer!.Cost);
        Answer(session); Assert.Equal(76, Member(session, First).Hp); Assert.Equal(4, Member(session, First).Progress!.Level);
    }

    [Fact]
    public void OrdinaryCommandsCannotAdvanceAServiceOrConsumeItsOffer()
    {
        var session = Session(); Choose(session, ChurchAction.Raise); var before = session.Current;
        foreach (var command in new SessionCommand[] { new Move(ExplorationDirection.North), new AdvanceSimulation(),
            new Confirm(), new Acknowledge(Wait(session).Token), new Search() })
        { Assert.NotNull(Send(session, command).Failure); Assert.Same(before, session.Current); }
        Answer(session); Assert.Equal(80u, session.Current.Exploration!.Party.Gold);
    }

    [Fact]
    public void ARealCallerFailureRetainsItsCommittedProgressAndAttributedError()
    {
        var session = Session(failReturn: true); var before = session.Current;
        var result = Send(session, new LeaveChurch(Wait(session).Token));
        Assert.NotNull(result.Failure); Assert.Equal("program-opcode", result.Failure!.Code);
        Assert.NotSame(before, session.Current); Assert.Contains(999, session.Current.Story.Flags);
        Assert.Null(session.Current.Story.Wait); Assert.Equal(SessionStopReason.Unsupported, session.Current.StopReason);
    }
}
