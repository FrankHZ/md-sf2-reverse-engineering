namespace Sf2.Remake.Domain.Battles;

internal enum BattleControlAdmission
{ Player, Dead, Unplaced, MuddledAi, MissingActivationWord, AiControlled, AllyAutoBattle, OpponentAi, Sleeping, Stunned }

internal static class BattleControlRules
{
    internal static BattleControlAdmission Classify(bool ally, ushort hp, int x, int y,
        ushort status, ushort? activationWord, bool allyAutoBattle, bool opponentControl)
    {
        if (hp == 0) return BattleControlAdmission.Dead;
        if (x is < 0 or >= 48 || y is < 0 or >= 48) return BattleControlAdmission.Unplaced;
        if ((status & 0x30) != 0) return BattleControlAdmission.MuddledAi;
        if (activationWord is null) return BattleControlAdmission.MissingActivationWord;
        if ((activationWord.Value & 4) != 0) return BattleControlAdmission.AiControlled;
        if (ally && allyAutoBattle) return BattleControlAdmission.AllyAutoBattle;
        if (!ally && !opponentControl) return BattleControlAdmission.OpponentAi;
        if ((status & 0xC0) != 0) return BattleControlAdmission.Sleeping;
        if ((status & 1) != 0) return BattleControlAdmission.Stunned;
        return BattleControlAdmission.Player;
    }

    internal static EngineBattleState EnterPlayer(EngineBattleState current, ActorRef actorRef)
    {
        if (current.StartPolicy is not { } policy) return current;
        var actor = current.GetActor(actorRef);
        ushort? word = actor.ActivationWord;
        if (actor.IsAlly && word is null) word = policy.MissingCandidateAllyWord;
        var decision = Classify(actor.IsAlly, actor.Hp, actor.Position?.X ?? 255, actor.Position?.Y ?? 255,
            actor.Status, word, policy.AllyAutoBattle, policy.OpponentControl);
        if (decision != BattleControlAdmission.Player)
            throw new BattleRuleException("player-control-" + decision.ToString().ToLowerInvariant(), "actor.control", true);
        // Admission precedes the candidate-only controlled supplement. Other allies remain unspecified.
        _ = BattleMovement.Preview(current, actorRef, actor.Position!);
        return actor.ActivationWord == word ? current : current.With(actors: current.Actors.Select(
            item => item.Actor == actorRef ? item.With(activationWord: word) : item));
    }
}
