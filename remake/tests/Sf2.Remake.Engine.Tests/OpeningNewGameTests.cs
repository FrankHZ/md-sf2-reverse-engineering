using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.TestSupport;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class OpeningNewGameTests
{
    private static BattleDeploymentDefinition Ally()
    {
        var curve = Array.AsReadOnly(Enumerable.Range(1, 29).Select(index =>
            new StatGrowthFraction((ushort)Math.Min(256, index * 64), (ushort)(index <= 4 ? 64 : 0))).ToArray());
        var growth = new BattleGrowthDefinition(4, 3,
            [new(10, 31, curve), new(4, 29, curve), new(5, 22, curve), new(3, 30, curve), new(6, 29, curve)],
            [new(1, 0), new(2, 64)], new Dictionary<byte, SpellRef> { [0] = new("heal", 1), [64] = new("heal", 2) });
        var actor = new BattleActorDefinition(new("new-priest"), BattleClassRule.UnpromotedPriest, 1, 10, 4, 8, 3, 6,
            false, 5, [], sourceLoadout: new([128, 127, 127, 127], [63, 63, 63, 63]), growth: growth);
        return new(actor, BattleFaction.Ally, 1, BattleControl.Player, null, new(1, 1));
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(0x1234abcdu)]
    [InlineData(uint.MaxValue)]
    public void LevelOneStartsAreFullExplicitAndDoNotConsumeAnyGrowthDraw(uint initial)
    {
        uint seed = initial;
        var input = OpeningPartyInitialization.Initialize(Ally(), 1, ref seed);
        Assert.Equal(initial, seed);
        Assert.Equal((ushort)10, input.Hp); Assert.Equal((byte)4, input.Mp);
        Assert.Equal((byte)0, input.Exp); Assert.Equal((ushort)0, input.Kills); Assert.Equal((ushort)0, input.Defeats);
        Assert.Equal((ushort)0, input.Status); Assert.Equal((ushort)0, input.ActivationWord);
        Assert.True(input.Initialized);
        Assert.Equal(new ushort[] { 128, 127, 127, 127 }, input.SourceLoadout!.Items);
        Assert.Equal(new byte[] { 0, 63, 63, 63 }, input.SourceLoadout.Spells);
        Assert.Equal(new SpellRef("heal", 1), Assert.Single(input.Progress!.Spells));
    }

    [Theory]
    [InlineData(0u, 0xf60a0000u, 15, 10, 9, 10, 12)]
    [InlineData(0x1234abcdu, 0xc4deabcdu, 16, 10, 9, 10, 12)]
    public void StartingLevelReplaysSourceStatOrderThenHealsAndKeepsEquipmentSeparate(uint initial, uint expectedSeed,
        int hp, int mp, int attack, int defense, int agility)
    {
        uint seed = initial;
        var deployment = Ally();
        var input = OpeningPartyInitialization.Initialize(deployment, 2, ref seed);
        Assert.Equal(expectedSeed, seed);
        var progress = input.Progress!;
        Assert.Equal(new[] { hp, mp, attack, defense, agility },
            new int[] { progress.MaxHp, progress.MaxMp, progress.BaseAttack, progress.Defense, progress.Agility });
        Assert.Equal(progress.MaxHp, input.Hp); Assert.Equal(progress.MaxMp, input.Mp);
        Assert.Equal(attack + 3, BattleEquipment.Attack(deployment.Definition, progress, input.SourceLoadout));
        Assert.Equal(64, input.SourceLoadout!.Spells[0]);
        Assert.Equal(2, progress.Spells.Count);
        Assert.Equal(1, deployment.Definition.Level); Assert.Empty(deployment.Definition.Spells);
        uint again = initial;
        var second = OpeningPartyInitialization.Initialize(deployment, 2, ref again);
        Assert.NotSame(input.Progress, second.Progress); Assert.NotSame(input.SourceLoadout, second.SourceLoadout);
        Assert.Equal(input.Hp, second.Hp); Assert.Equal(input.SourceLoadout.Spells, second.SourceLoadout!.Spells);
        Assert.Equal(expectedSeed, again);
    }

    private static OpeningNewGameReader Selected(uint main, uint thinking) => new(
        PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_EXPLORATION_CONTENT"),
        PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_BATTLE01_DATA"), PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_BATTLE01_SCENE"),
        PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_BATTLE01_TERRAIN"), PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_STATIC_DATA"),
        PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_ENEMY_DATA"), PrivateInputFactAttribute.RequireInput("SF2_PRIVATE_ENEMY_GOLD"), main, thinking);

    [PrivateInputFact("SF2_PRIVATE_EXPLORATION_CONTENT", "SF2_PRIVATE_STATIC_DATA", "SF2_PRIVATE_ENEMY_DATA", "SF2_PRIVATE_ENEMY_GOLD",
        "SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE", "SF2_PRIVATE_BATTLE01_TERRAIN")]
    public void PrivateNewGamePublishesFreshRosterAndIndependentSessionsWithoutCheckpointInput()
    {
        var first = Assert.IsType<SessionStarted>(GameSession.Start(Selected(0, uint.MaxValue))).Session;
        var second = Assert.IsType<SessionStarted>(GameSession.Start(Selected(0, uint.MaxValue))).Session;
        Assert.NotEqual(first.Current.SessionId, second.Current.SessionId);
        Assert.Equal("private-local-new-game", first.Definition.Origin);
        Assert.Equal(new[] { 0, 32, 80 }, first.Current.Story.Flags);
        Assert.Equal(new MapPosition(56, 3), first.Current.Exploration!.PlayerEntity.Position);
        Assert.Equal((short)0, first.Current.Story.StepCounter); Assert.False(first.Current.Story.BattleRequest);
        Assert.Equal(new MapId("map-0"), first.Current.Story.EgressMap);
        var party = first.Current.Exploration.Party;
        Assert.Equal(4, party.Actors.Count); Assert.Equal(0u, party.MainSeed); Assert.Equal(uint.MaxValue, party.ThinkingSeed);
        Assert.Equal(0u, party.Gold); Assert.Equal(new ActorRef("ally-0"), Assert.Single(party.ActiveAllies!));
        Assert.Empty(party.ItemCustody!.Deals); Assert.Empty(party.ItemCustody.OnceFlags);
        Assert.Equal(BattleStartOrigin.NewGame, party.NewBattle!.Origin); Assert.Empty(party.NewBattle.BridgeBoundary);
        Assert.False(party.NewBattle.BeforeBattleRouted); Assert.False(party.NewBattle.SkipIntro);
        Assert.All(party.Actors, actor => { Assert.Equal((byte)0, actor.Exp); Assert.Equal((ushort)0, actor.Status);
            Assert.Equal(actor.Progress!.MaxHp, actor.Hp); Assert.Equal(actor.Progress.MaxMp, actor.Mp); });
        var old = second.Current;
        var moved = first.Submit(new(first.Current.SessionId, first.Current.Revision, null, new Move(ExplorationDirection.South)));
        Assert.Null(moved.Failure); Assert.Same(old, second.Current);
        Assert.Equal(new MapPosition(56, 3), second.Current.Exploration!.PlayerEntity.Position);
    }
}
