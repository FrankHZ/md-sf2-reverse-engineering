using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class FieldServiceTests
{
    private static readonly ActorRef First = FieldInventoryTests.Giver, Second = FieldInventoryTests.Recipient;
    private static readonly ChurchDefinition Terms = new(10, 200, 200, 10, 20);
    private static GameSession Session(uint? gold = 100, ushort hp = 0, ushort status = 0,
        ushort secondHp = 100, ushort secondStatus = 0, byte level = 2, bool promoted = false, ushort maxHp = 250,
        ChurchDefinition? terms = null, byte? currentLevel = null, bool failReturn = false, bool promotion = false, bool armed = false, bool power = false,
        BattleClassRule sourceRule = BattleClassRule.UnpromotedSwordsman, byte warriorAxe = 33)
    {
        var original = FieldInventoryTests.Session().Current.Exploration!.Party;
        var source = FieldInventoryTests.Session().Definition.Encounters.Values.Single();
        (byte baseClass, byte destination, string baseCode, string promotedCode, string title, byte move, string mover,
            byte weapon, ushort? promotionItem) profile = sourceRule switch
        {
            BattleClassRule.UnpromotedKnight => (1, 13, "KNTE", "PLDN", "Paladin", 7, "CENTAUR", 50, 118),
            BattleClassRule.UnpromotedWarrior => (2, 15, "WARR", "GLDT", "Gladiator", 5, "REGULAR", warriorAxe, 119),
            BattleClassRule.UnpromotedMage => (3, 17, "MAGE", "WIZ", "Wizard", 5, "MAGE", 85, 121),
            BattleClassRule.UnpromotedPriest => (4, 19, "PRST", "VICR", "Vicar", 5, "HEALER",
                (byte)(currentLevel >= 40 ? 86 : 89), null),
            BattleClassRule.UnpromotedArcher => (5, 21, "ACHR", "SNIP", "Sniper", 6, "ARCHER", 47, 120),
            BattleClassRule.UnpromotedBirdman => (6, 23, "BDMN", "BDBT", "BirdBattler", 6, "FLYING", 58, null),
            BattleClassRule.UnpromotedRanger => (8, 25, "RNGR", "BWNT", "BowKnight", 6, "CENTAUR_ARCHER", 47, null),
            _ => (0, 12, "SDMN", "HERO", "Hero", 6, "REGULAR", 50, null),
        };
        var (baseClass, destination, baseCode, promotedCode, title, move, mover, weapon, promotionItem) = profile;
        byte[] book = sourceRule switch
        {
            BattleClassRule.UnpromotedMage => currentLevel >= 29 ? [139, 71, 6, 72] : [75, 76, 63, 63],
            BattleClassRule.UnpromotedPriest => currentLevel >= 40 ? [192, 130, 206, 68] : [128, 142, 3, 63],
            BattleClassRule.UnpromotedSwordsman => currentLevel >= 22 ? [10, 13, 63, 63] : [10, 63, 63, 63],
            _ => [63, 63, 63, 63],
        };
        string SpellName(int id) => id switch
        {
            0 => "heal", 1 => "aura", 2 => "detox", 3 => "boost", 4 => "slow", 6 => "dispel",
            7 => "muddle", 8 => "desoul", 10 => "egress", 11 => "blaze", 12 => "freeze", 14 => "blast", _ => "bolt",
        };
        var spells = book.Where(spell => spell != 63)
            .SelectMany(spell => Enumerable.Range(1, 1 + (spell >> 6))
                .Select(rank => new SpellRef(SpellName(spell & 63), (byte)rank))).ToArray();
        bool ranged = sourceRule is BattleClassRule.UnpromotedArcher or BattleClassRule.UnpromotedRanger;
        byte retainedRange = (byte)(ranged ? 2 : 1);
        byte attackBonus = weapon switch { 36 or 47 => 17, 58 => 8, _ => 5 };
        var loadout = new BattleSourceLoadout([(ushort)(weapon | (armed ? 128 : 0)),
            power ? (ushort)147 : sourceRule == BattleClassRule.UnpromotedMage ? (ushort)24 : (ushort)127,
            ranged ? (ushort)50 : (ushort)127, 127], book);
        var initialLoadout = weapon == 50 ? null : new BattleSourceLoadout([weapon, 127, 127, 127], book);
        var deployments = source.Deployments.Select(row => row.Actor != First ? row : row with
        {
            Definition = new(row.Actor, promotion ? sourceRule : row.Definition.ClassRule, level, maxHp, row.Definition.MaxMp,
                row.Definition.Attack, row.Definition.Defense, row.Definition.Agility, false, row.Definition.Move,
                row.Definition.Spells, row.Definition.Physical! with { Promoted = promoted },
                sourceLoadout: initialLoadout ?? row.Definition.SourceLoadout),
        });
        if (promotion)
        {
            var growth = new BattleGrowthDefinition(baseClass, 0, Enumerable.Repeat(new StatGrowth(20, 20, []), 5).ToArray(),
                [new(1, 10), new(22, 13)], new Dictionary<byte, SpellRef> { [10] = new("egress", 1), [13] = new("bolt", 1) });
            List<BattleWeaponDefinition> weapons =
                [new(weapon, "Weapon", (1u << baseClass) | (1u << destination), attackBonus, retainedRange, retainedRange)];
            if (ranged) weapons.Add(new(50, "Great Shot", (1u << 21) | (1u << 22) | (1u << 25), 29, 2, 3));
            var equipment = new BattleEquipmentDefinition(20, weapons,
                rings: [new(19, "Power Ring", uint.MaxValue, RingStat.Attack, 5),
                    new(24, "Black Ring", 1u << 17, RingStat.Attack, 0)]);
            if (sourceRule != BattleClassRule.UnpromotedSwordsman)
            {
                var fixture = source.WithDeployments(deployments);
                var classDefinitions = new PrivateBattleDefinitions(null!, [],
                    [new(destination, promotedCode, title, move, "NONE", mover, "CRITICAL125_1IN16|DOUBLE_1IN32|COUNTER_1IN16")],
                    [new(weapon, "WEAPON", "Weapon", "Weapon", baseCode + "|" + promotedCode, [baseCode, promotedCode], retainedRange, retainedRange, 0,
                        "WEAPON", "NOTHING", "NOTHING", 0, [new("INCREASE_ATT", attackBonus)])], [], [], []);
                var stats = Enumerable.Repeat(new { start = 20, projected = 20, curve = System.Array.Empty<int>() }, 5);
                var schedule = book.Where(packed => packed != 63)
                    .Select(packed => new { packed, level = 22, spell = SpellName(packed & 63) });
                if (sourceRule == BattleClassRule.UnpromotedPriest && currentLevel < 40)
                    schedule = schedule.Append(new { packed = (byte)67, level = 30, spell = "boost" })
                        .Append(new { packed = (byte)1, level = 33, spell = "aura" });
                var rows = JsonSerializer.SerializeToElement(new[] { baseClass, destination }.Select(classId => new
                {
                    actor = First.Value, classId, stats, spells = schedule,
                }));
                deployments = BattleGrowthReader.Bind(rows, new ScenarioDefinition("promotion-test", [fixture], classDefinitions))
                    .Single().Deployments.Select(d => d.Actor != First ? d : d with
                    {
                        Definition = d.Definition.WithEquipment(equipment),
                        RegularPromotion = d.RegularPromotion!.WithEquipment(equipment),
                    });
            }
            else
            {
                deployments = deployments.Select(d => d.Actor != First ? d : d with
                {
                    Definition = d.Definition.WithGrowth(growth).WithEquipment(equipment),
                    RegularPromotion = new(d.Actor, BattleClassRule.Hero, 1, 42, 14, 28, 25, 24, false, 6, [],
                        d.Definition.Physical! with { Promoted = true, Critical = PhysicalCriticalRule.Hero },
                        sourceLoadout: loadout, growth: growth with { ClassId = destination, RetainedSpells = growth.Spells }, equipment: equipment),
                });
            }
        }
        var battle = new BattleDefinition(source.Encounter, source.Map, source.Width, source.Height,
            source.Terrain, deployments, source.Spells.Values);
        EntityRef priest = new("priest");
        var map = new ExplorationMapDefinition(new("village"), new(new ushort[WorkingMapLayout.WordCount]),
            new([new(0, 0, 63, 63)]), [new(priest, new(2, 1), 3, 32)],
            [new(ExplorationEventKind.Interact, null, null, priest, new("caller", 0))]);
        var world = new ExplorationDefinition([map],
            [new("caller", [new CallProgram(new("service", 0)), new WriteFlag(999, true),
                failReturn ? new UnsupportedInstruction("unadmitted", "authored-caller") : new EndProgram()]),
             new("service", [new OpenChurch(terms ?? Terms), new ReturnProgram()])], []);
        var definition = new ScenarioDefinition("service-behavior", [battle], exploration: world);
        var party = new BattleStartInput(battle.Encounter, original.Actors.Select(actor => actor.Actor == First
            ? actor with { Hp = hp, Status = status, Progress = currentLevel is { } attained
                ? new(attained, 76, 10, 20, 4, 60, [], actor.SourceLoadout) : null }
            : actor.Actor == Second ? actor with { Hp = secondHp, Status = secondStatus } : actor),
            original.MainSeed, original.ThinkingSeed, gold, original.NewBattle, original.ActiveAllies, original.ItemCustody);
        if (promotion)
            party = new(party.Encounter, party.Actors.Select(a => a.Actor == First ? a with { SourceLoadout = loadout,
                Progress = a.Progress! with { ClassId = baseClass, SourceLoadout = loadout, Spells = spells, BaseMove = 8 } }
                : a.Actor == Second && promotionItem is { } item ? a with
                    { SourceLoadout = new([item, 127, 127, 127], [63, 63, 63, 63]) } : a),
                party.MainSeed, party.ThinkingSeed, party.Gold, party.NewBattle, [Second], party.ItemCustody);
        var session = Assert.IsType<SessionStarted>(GameSession.Start(definition,
            new ExplorationStartInput(map.Map, new("player"), new(2, 2), 1, 32, [], party))).Session;
        Accept(session, new Interact(priest));
        Assert.IsType<ChurchWait>(session.Current.Story.Wait);
        return session;
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
    public void RangedPromotionPreservesTheArrowAndChangesPromotedOnlyShotEligibility(BattleClassRule source,
        byte level, byte classId, BattleClassRule destination, BattleMover mover, int roughCost, bool archer)
    {
        var session = Session(hp: 27, currentLevel: level, promotion: true, armed: true, sourceRule: source);
        var prior = Member(session, First);
        var holder = Member(session, Second);
        var original = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == First).Deployment!;
        Assert.False(BattleEquipment.IsEligible(original.Definition, 50));

        Choose(session, ChurchAction.Promote);
        var offer = Wait(session).Candidates!.Single(c => c.Member == First);
        Assert.Equal(archer ? new HeldPromotionItem(Second, 0, 120) : null, offer.SpecialItem);
        Accept(session, new SelectPromotion(Wait(session).Token, offer));
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));
        if (archer)
        {
            Assert.Equal(ChurchPhase.Special, Wait(session).Phase);
            Accept(session, new AnswerPromotion(Wait(session).Token, offer, false));
        }
        Assert.Equal(ChurchPhase.Regular, Wait(session).Phase);
        Accept(session, new AnswerPromotion(Wait(session).Token, offer, true));

        var member = Member(session, First);
        Assert.Same(holder, Member(session, Second));
        Assert.Equal(prior with { Exp = 0, Progress = prior.Progress! with { ClassId = classId, Level = 1, BaseMove = null } }, member);
        var deployment = session.QueryPartyDetails()!.Members.Single(m => m.Carried.Actor == First).Deployment!;
        var definition = deployment.Definition;
        Assert.Equal(destination, definition.ClassRule);
        Assert.Equal(6, definition.Move);
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
    [InlineData(2, false, 250, 20, 200)]
    [InlineData(7, true, 90, 270, 90)]
    public void RaiseUsesCurrentLevelPromotionCapAndExactFundsWithoutResettingTheParty(byte level, bool promoted, ushort maxHp, uint cost, ushort raisedHp)
    {
        var session = Session(gold: cost, level: level, promoted: promoted, maxHp: maxHp);
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
