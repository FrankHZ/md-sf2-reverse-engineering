using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleMessagePolicyTests
{
    [Theory]
    [InlineData(0, "Damage", false, true)]
    [InlineData(14, "Damage", true, false)]
    [InlineData(62, "Dodge", false, false)]
    public void CriticalAndMissMessagesRemainRequired(int seed, string kind, bool critical, bool optional)
    {
        var session = Start("stone-court", d =>
        {
            d["start"]!["mainSeed"] = ((uint)seed << 16) | 0x1234u;
            d["actors"]![0]!["attack"] = 5;
            d["start"]!["actors"]![2]!["hp"] = 500;
            d["actors"]![2]!["maxHp"] = 500;
        });
        Accept(session, new Confirm());
        Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        Accept(session, new SelectTarget(new("raider")));
        Accept(session, new Confirm());
        for (int n = 0; session.Current.BattleScene!.Phase != BattleScenePhase.ResultMessage; n++)
        {
            Assert.True(n < 30);
            var step = session.Current.BattleScene!;
            Accept(session, step.RequiresAcknowledgement ? new Acknowledge(step.Token)
                : new CompletePresentation(step.Token, step.CompletionKind));
        }
        var message = session.Current.BattleScene!;
        Assert.Equal(kind, message.ReactionKind);
        Assert.Equal(critical, message.Critical);
        Assert.Equal(optional, message.CanSuppressHitCoinMessage);
        Assert.True(message.RequiresAcknowledgement);
    }

    [Theory]
    [InlineData(0x12341234u)]
    [InlineData(0x00371234u)]
    [InlineData(0x00011234u)]
    public void OptionalDeliveryUsesTheSameTokenAndKeepsDamageRewardAndTurn(uint seed)
    {
        var session = Start("stone-court", d => d["start"]!["mainSeed"] = seed);
        Accept(session, new Confirm());
        Accept(session, new ChooseAction(SessionAction.PhysicalAttack));
        Accept(session, new SelectTarget(new("raider")));
        Accept(session, new Confirm());
        List<BattleScenePhase> retained = [], optional = [];
        int count = 0;
        while (session.Current.BattleScene is { } scene)
        {
            Assert.True(++count < 150);
            var before = session.Current;
            if (scene.CanSuppressHitCoinMessage)
            {
                Assert.True(scene.RequiresAcknowledgement);
                if (scene.Phase == BattleScenePhase.ResultMessage)
                {
                    Assert.False(scene.Critical);
                    Assert.Equal("Damage", scene.ReactionKind);
                    Assert.True(before.Battle.GetActor(new("raider")).Hp < 15);
                }
                optional.Add(scene.Phase);
                // Hiding a message creates no new bypass command. Wrong tokens and
                // presentation completion still refuse without mutating the state.
                Assert.NotNull(Send(session, new Acknowledge(new(scene.Token.Value + 1))).Failure);
                Assert.NotNull(Send(session, new CompletePresentation(scene.Token, scene.CompletionKind)).Failure);
                Assert.Same(before, session.Current);
            }
            else if (scene.RequiresAcknowledgement) retained.Add(scene.Phase);
            SessionCommand command = scene.RequiresAcknowledgement ? new Acknowledge(scene.Token)
                : new CompletePresentation(scene.Token, scene.CompletionKind);
            Accept(session, command);
            Assert.NotNull(Send(session, command).Failure);
        }
        Assert.Contains(BattleScenePhase.ActionMessage, retained);
        Assert.Contains(BattleScenePhase.DeathMessage, retained);
        Assert.Contains(BattleScenePhase.RewardMessage, retained);
        Assert.Contains(BattleScenePhase.GoldMessage, optional);
        Assert.Equal(0, session.Current.Battle.GetActor(new("raider")).Hp);
        Assert.True(session.Current.Battle.GetActor(new("swordsman")).Exp > 0);
        Assert.True(session.Current.Battle.Gold > 100);
        Assert.NotNull(session.Current.Selection);
    }
}
