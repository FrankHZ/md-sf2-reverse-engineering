using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter;
using Xunit;

namespace Sf2.Remake.Godot.Tests;

public sealed class PrivateBattle01PresenterTests
{
    [Theory]
    [InlineData(false, false, false, false, false, false)]
    [InlineData(true, false, false, false, false, false)]
    [InlineData(true, true, false, false, false, false)]
    [InlineData(true, true, true, false, false, false)]
    [InlineData(true, true, true, true, false, false)]
    [InlineData(true, true, false, false, true, false)]
    [InlineData(true, true, false, false, true, true)]
    public void PursuitAndAttackBoundaryProjectionKeepCompletionAndCurrentCandidateDistinct(bool chesterPlayer, bool firstAlly, bool leader, bool returnSelected, bool counter, bool firstKill)
    {
        var admission = returnSelected ? new Battle01DefeatReturnAdmission(3, false, false) : null;
        var current=Battle01EnemyStandby.CompleteFirst(AuthoredSecondCompleted(regionEntry:true,combatProfile:true,chesterPlayer,firstAlly,leader,admission,firstKill),128,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        for(int i=0;i<5;i++) current=Battle01EnemyStandby.CompleteNext(current,current.FirstRound!.CurrentCandidate!.Value.CombatantIndex,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        current=Stay(Battle01NextPlayerControl.Enter(current,0).State!,0,new(8,17));
        current=Battle01FirstRound.EnterNext(current);
        while(current.FirstRound!.CurrentCandidate is { } candidate)
        {
            int actor=candidate.CombatantIndex;
            current=actor==0 ? Stay(Battle01NextPlayerControl.Enter(current,0).State!,0,new(11,15)) : CompleteAuthoredTurn(current);
        }
        current=Battle01FirstRound.EnterNext(current);
        for(int i=0;i<4;i++) current=CompleteAuthoredTurn(current);
        var pursuit=PrivateBattle01Presenter.BuildProjection(current,"Pursuit complete.");
        Assert.True(pursuit.PursuitCompleted); Assert.Equal(131,pursuit.CompletedActorIndex);
        Assert.Equal(128,pursuit.NextCandidateIndex); Assert.Null(pursuit.ActorIndex); Assert.Null(pursuit.Cursor);
        Assert.Empty(pursuit.Path); Assert.False(pursuit.CanConfirm); Assert.Contains("Input closed",pursuit.Controls);
        Assert.Equal(new MapPosition(9,4),pursuit.Units.Single(unit=>unit.Index==131).Position);
        while(current.FirstRound!.RoundNumber<6 || current.FirstRound.CurrentTurnOffset<10)
        {
            if(current.FirstRound.CurrentCandidate is null)
            {
                current=Battle01FirstRound.EnterNext(current);
                if(current.FirstRound!.RoundNumber==4)
                {
                    var generated=PrivateBattle01Presenter.BuildProjection(current,"Round 4 generated.");
                    Assert.False(generated.PursuitCompleted); Assert.Null(generated.CompletedActorIndex); Assert.Equal(1,generated.ActorIndex);
                }
            }
            else current=CompleteAuthoredTurn(current);
        }
        var boundary=Assert.Throws<Battle01AttackSelectionRequiredException>(()=>Battle01EnemyPursuit.CompleteNext(current,132,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats));
        var stopped=PrivateBattle01Presenter.BuildProjection(current,boundary.Message);
        Assert.False(stopped.PursuitCompleted); Assert.Equal(128,stopped.CompletedActorIndex); Assert.Equal(132,stopped.NextCandidateIndex);
        Assert.Null(stopped.Cursor); Assert.Empty(stopped.Path); Assert.False(stopped.CanConfirm);
        Assert.All(stopped.Tiles,tile=> { Assert.False(tile.Reachable); Assert.False(tile.CanStop); });
        Assert.Contains("attack selection",stopped.Status); Assert.Contains("Input closed",stopped.Controls); Assert.DoesNotContain("Space",stopped.Controls);
        current=Battle01EnemyPhysicalAttack.CompleteNext(current,132,Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike);
        var completed=PrivateBattle01Presenter.BuildProjection(current,"Attack completed.");
        Assert.True(completed.PhysicalAttackCompleted); Assert.False(completed.PursuitCompleted);
        Assert.Equal(132,completed.CompletedActorIndex); Assert.Equal(0,completed.NextCandidateIndex);
        Assert.Contains("hit 3",completed.AttackResult); Assert.Contains("HP 12 -> 9",completed.AttackResult);
        current=Battle01NextPlayerControl.Enter(current,0).State!;
        var ready=PrivateBattle01Presenter.BuildProjection(current,"Round 6. Player 0 ready.");
        Assert.Equal(0,ready.ActorIndex); Assert.Null(ready.CompletedActorIndex);
        Assert.Equal(completed.AttackResult,ready.AttackResult); Assert.Equal(9,ready.Units.Single(unit=>unit.Index==0).Hp);
        Assert.Contains("Space",ready.Controls); Assert.True(ready.CanConfirm);
        var cancelled=Battle01PlayerMovement.Cancel(Battle01PlayerMovement.Confirm(current,0),0);
        Assert.Equal(ready.AttackResult,PrivateBattle01Presenter.BuildProjection(cancelled,"Cancelled.").AttackResult);
        var chosen=Battle01PlayerPhysicalAttack.Begin(Battle01PlayerMovement.Confirm(current,0),0);
        var targetView=PrivateBattle01Presenter.BuildProjection(chosen,"Target selected.");
        Assert.Equal(132,targetView.SelectedTargetIndex); Assert.Contains("previous",targetView.Controls);
        Assert.Equal((byte?)0,targetView.Units.Single(unit=>unit.Index==0).Exp);
        current=Battle01PlayerPhysicalAttack.Confirm(chosen,0,Battle01PlayerPhysicalCompletionPolicy.ControlledNonlethalStrikeAndExp);
        var playerResult=PrivateBattle01Presenter.BuildProjection(current,"Player hit.");
        Assert.True(playerResult.PhysicalAttackCompleted); Assert.Contains("HP 5 -> 2",playerResult.AttackResult);
        Assert.Contains("EXP +15: 0 -> 15",playerResult.AttackResult);
        current=CompleteAuthoredTurn(current); current=CompleteAuthoredTurn(current);
        current=Battle01NextPlayerControl.Enter(Battle01FirstRound.EnterNext(current),2).State!;
        var next=PrivateBattle01Presenter.BuildProjection(current,"Round 7. Player 2 ready.");
        Assert.Equal(playerResult.AttackResult,next.AttackResult); Assert.Equal(2,next.ActorIndex);
        Assert.Equal((byte?)15,next.Units.Single(unit=>unit.Index==0).Exp);
        current = Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(current, 2), 2,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        current = CompleteAuthoredTurn(current);
        current = Battle01EnemyPhysicalAttack.CompleteNext(current, 132, Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike);
        current = CompleteAuthoredTurn(current);
        current = Battle01NextPlayerControl.Enter(current, 0).State!;
        current = Battle01PlayerPhysicalAttack.Begin(Battle01PlayerMovement.Confirm(current, 0), 0);
        current = Battle01PlayerPhysicalAttack.Confirm(current, 0, Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndFirstDefeat);
        var defeated = PrivateBattle01Presenter.BuildProjection(current, "First defeat complete.");
        Assert.Equal(8, defeated.Units.Count); Assert.DoesNotContain(defeated.Units, unit => unit.Index == 132);
        Assert.Contains("E4 (132) defeated", defeated.AttackResult); Assert.Contains("HP 2 -> 0", defeated.AttackResult);
        Assert.Contains("EXP +24: 15 -> 39", defeated.AttackResult); Assert.Contains("Gold +60", defeated.AttackResult);
        Assert.Equal(((uint?)60, (ushort?)1), (defeated.Gold, defeated.BowieKills));
        Assert.Equal((6, (byte?)39), ((int)defeated.Units.Single(unit => unit.Index == 0).Hp, defeated.Units.Single(unit => unit.Index == 0).Exp));
        current = Battle01NextPlayerControl.Enter(current, 1).State!;
        var playerOne = PrivateBattle01Presenter.BuildProjection(current, "Player 1 ready.");
        Assert.Equal(1, playerOne.ActorIndex); Assert.True(playerOne.CanConfirm); Assert.Contains("Space", playerOne.Controls);
        var clearTile = Assert.Single(playerOne.Tiles, tile => tile.Position == new MapPosition(11, 14));
        Assert.Equal((byte)1, clearTile.Terrain); Assert.True(clearTile.CanStop);
        current = Battle01PlayerMovement.Cancel(Battle01PlayerMovement.Confirm(
            Battle01PlayerMovement.SelectDestination(current, 1, new(10, 17)), 1), 1);
        var afterCancel = PrivateBattle01Presenter.BuildProjection(current, "Cancelled.");
        Assert.Equal(defeated.AttackResult, afterCancel.AttackResult);
        Assert.Equal(defeated.Units, afterCancel.Units); Assert.Equal((uint?)60, afterCancel.Gold); Assert.Equal((ushort?)1, afterCancel.BowieKills);

        current = Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(current, 1), 1,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        for (int i = 0; i < 3; i++) current = CompleteAuthoredTurn(current);
        current = Battle01NextPlayerControl.Enter(Battle01FirstRound.EnterNext(current), 2).State!;
        current = Stay(current, 2, new(11, 14)); current = CompleteAuthoredTurn(current); // actual133
        for (int i = 0; i < 5; i++) current = CompleteAuthoredTurn(current); // Bowie/player1 origin STAY,129/128/130
        current = Battle01EnemyPhysicalAttack.CompleteNext(current, 131, Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike);
        var chesterHit = PrivateBattle01Presenter.BuildProjection(current, "Chester hit.");
        Assert.Equal("E3 -> A2: hit 2. HP 11 -> 9.", chesterHit.AttackResult);
        Assert.True(chesterHit.PhysicalAttackCompleted); Assert.Equal(131, chesterHit.CompletedActorIndex);
        current = Battle01NextPlayerControl.Enter(Battle01FirstRound.EnterNext(current), 2).State!;
        var chesterReady = PrivateBattle01Presenter.BuildProjection(current, "Round 9. Player 2 ready.");
        Assert.Equal(chesterHit.AttackResult, chesterReady.AttackResult); Assert.Equal(2, chesterReady.ActorIndex);
        Assert.Equal(14, chesterReady.Budget); Assert.True(chesterReady.CanConfirm);
        Assert.Equal(9, chesterReady.Units.Single(unit => unit.Index == 2).Hp);
        Assert.DoesNotContain(chesterReady.Units, unit => unit.Index == 132);
        Assert.Equal(((uint?)60, (ushort?)1), (chesterReady.Gold, chesterReady.BowieKills));
        Assert.Equal((byte?)39, chesterReady.Units.Single(unit => unit.Index == 0).Exp);
        current = Battle01PlayerMovement.Cancel(Battle01PlayerMovement.Confirm(
            Battle01PlayerMovement.SelectDestination(current, 2, new(12, 14)), 2), 2);
        var chesterCancelled = PrivateBattle01Presenter.BuildProjection(current, "Cancelled.");
        Assert.Equal(chesterHit.AttackResult, chesterCancelled.AttackResult); Assert.Equal(chesterReady.Units, chesterCancelled.Units);
        Assert.Equal(chesterPlayer ? (byte?)0 : null, chesterCancelled.Units.Single(u => u.Index == 2).Exp);
        if (!chesterPlayer) return;
        current = Battle01PlayerPhysicalAttack.Begin(Battle01PlayerMovement.Confirm(current, 2), 2);
        var chesterTarget = PrivateBattle01Presenter.BuildProjection(current, "Chester target.");
        Assert.Equal(2, chesterTarget.ActorIndex); Assert.Equal(131, chesterTarget.SelectedTargetIndex);
        current = Battle01PlayerPhysicalAttack.Confirm(current, 2, Battle01PlayerPhysicalCompletionPolicy.ControlledNonlethalStrikeAndExp);
        var chesterAttack = PrivateBattle01Presenter.BuildProjection(current, "Chester attack.");
        Assert.Equal("A2 -> E3: hit 2. HP 5 -> 3. EXP +10: 0 -> 10.", chesterAttack.AttackResult);
        Assert.Equal((byte?)10, chesterAttack.Units.Single(u => u.Index == 2).Exp);
        Assert.Equal(3, chesterAttack.Units.Single(u => u.Index == 131).Hp);
        current = CompleteAuthoredTurn(current); current = CompleteAuthoredTurn(current);
        Assert.Equal(chesterAttack.AttackResult, PrivateBattle01Presenter.BuildProjection(current, "Standby complete.").AttackResult);
        current = Battle01EnemyPhysicalAttack.CompleteNext(current, 131, Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike);
        current = CompleteAuthoredTurn(current);
        current = Battle01NextPlayerControl.Enter(current, 1).State!;
        var sarahReady = PrivateBattle01Presenter.BuildProjection(current, "Sarah ready.");
        Assert.Equal("E3 -> A0: hit 3. HP 6 -> 3.", sarahReady.AttackResult);
        Assert.Equal(1, sarahReady.ActorIndex); Assert.Equal(10, sarahReady.Budget); Assert.True(sarahReady.CanConfirm);
        Assert.Equal((byte?)10, sarahReady.Units.Single(u => u.Index == 2).Exp);
        Assert.Equal(9, sarahReady.Units.Single(u => u.Index == 2).Hp);
        Assert.Equal(((uint?)60, (ushort?)1), (sarahReady.Gold, sarahReady.BowieKills));
        Assert.DoesNotContain(sarahReady.Units, u => u.Index == 132);
        var sarahCancelled = Battle01PlayerMovement.Cancel(Battle01PlayerMovement.Confirm(
            Battle01PlayerMovement.SelectDestination(current, 1, new(10, 17)), 1), 1);
        var final = PrivateBattle01Presenter.BuildProjection(sarahCancelled, "Sarah cancelled.");
        Assert.Equal(sarahReady.Units, final.Units); Assert.Equal(sarahReady.AttackResult, final.AttackResult);
        current = Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(sarahCancelled, 1), 1,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        current = CompleteAuthoredTurn(current); // actual130, then Bowie
        current = Battle01NextPlayerControl.Enter(current, 0).State!;
        current = Battle01PlayerPhysicalAttack.Begin(Battle01PlayerMovement.Confirm(current, 0), 0);
        Assert.Equal(131, PrivateBattle01Presenter.BuildProjection(current, "Second target.").SelectedTargetIndex);
        current = Battle01PlayerPhysicalAttack.Confirm(current, 0, Battle01PlayerPhysicalCompletionPolicy.ControlledStrikeAndSecondDefeat);
        var secondDefeat = PrivateBattle01Presenter.BuildProjection(current, "Second defeat complete.");
        // This authored grid has plains at (10,15); the required Content/native route owns terrain0 damage4.
        Assert.Contains("hit 3", secondDefeat.AttackResult); Assert.Contains("HP 3 -> 0", secondDefeat.AttackResult);
        Assert.Contains("E3 (131) defeated", secondDefeat.AttackResult);
        Assert.Contains("EXP +24: 39 -> 63", secondDefeat.AttackResult); Assert.Contains("Gold +60", secondDefeat.AttackResult);
        Assert.Equal(((uint?)120, (ushort?)2), (secondDefeat.Gold, secondDefeat.BowieKills));
        Assert.Equal(7, secondDefeat.Units.Count); Assert.Equal(4, secondDefeat.Units.Count(u => u.Index >= 128));
        Assert.DoesNotContain(secondDefeat.Units, u => u.Index is 131 or 132);
        Assert.Null(secondDefeat.ActorIndex); Assert.Null(secondDefeat.NextCandidateIndex);
        current = Battle01NextPlayerControl.Enter(Battle01FirstRound.EnterNext(current), 2).State!;
        var roundTen = PrivateBattle01Presenter.BuildProjection(current, "Round 10. Player 2 ready.");
        Assert.Equal(2, roundTen.ActorIndex); Assert.Equal(14, roundTen.Budget); Assert.True(roundTen.CanConfirm);
        Assert.Equal(secondDefeat.AttackResult, roundTen.AttackResult);
        Assert.Equal((byte?)63, roundTen.Units.Single(u => u.Index == 0).Exp);
        Assert.Equal((9, (byte?)10), ((int)roundTen.Units.Single(u => u.Index == 2).Hp, roundTen.Units.Single(u => u.Index == 2).Exp));
        current = Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(current, 2, new(12, 14)), 2);
        var provisional = PrivateBattle01Presenter.BuildProjection(current, "Chester provisional.");
        Assert.Equal(new MapPosition(12, 14), provisional.Units.Single(u => u.Index == 2).Position);
        Assert.Equal(roundTen.AttackResult, provisional.AttackResult);
        current = Battle01PlayerMovement.Cancel(current, 2);
        var roundTenCancelled = PrivateBattle01Presenter.BuildProjection(current, "Chester cancelled.");
        Assert.Equal(roundTen.Units, roundTenCancelled.Units); Assert.Equal(roundTen.AttackResult, roundTenCancelled.AttackResult);
        Assert.Equal((roundTen.Gold, roundTen.BowieKills), (roundTenCancelled.Gold, roundTenCancelled.BowieKills));

        if (counter)
        {
            current=Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(
                Battle01PlayerMovement.SelectDestination(current,2,new(9,9)),2),2,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
            for(int step=0;step<16;step++)
            {
                if(current.FirstRound!.RoundNumber==11 && current.FirstRound.CurrentCandidate?.CombatantIndex==2)break;
                current=current.FirstRound.CurrentCandidate is null ? Battle01FirstRound.EnterNext(current):CompleteAuthoredTurn(current);
            }
            current=Battle01NextPlayerControl.Enter(current,2).State!;
            current=Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(current,2,new(9,4)),2);
            current=Battle01PlayerPhysicalAttack.Confirm(Battle01PlayerPhysicalAttack.Begin(current,2),2,Battle01PlayerPhysicalCompletionPolicy.ControlledNonlethalStrikeAndExp);
            current=Battle01EnemyPhysicalAttack.CompleteNext(current,128,Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike);
            current=Battle01EnemyPhysicalAttack.CompleteNext(current,130,Battle01PhysicalCompletionPolicy.ControlledNonlethalChesterCounterAndExp);
            var view=PrivateBattle01Presenter.BuildProjection(current,"Counter complete.");
            int counterExp = firstKill ? 30 : 25;
            Assert.Equal($"E2 -> A2: hit 2. HP 7 -> 5.\nCounter: A2 -> E2: hit 1. HP 5 -> 4. EXP +5: {counterExp - 5} -> {counterExp}.",view.AttackResult);
            Assert.Equal((130,133),(view.CompletedActorIndex,view.NextCandidateIndex));Assert.Null(view.ActorIndex);
            Assert.Equal((5,(byte?)counterExp),((int)view.Units.Single(u=>u.Index==2).Hp,view.Units.Single(u=>u.Index==2).Exp));
            Assert.Equal(4,view.Units.Single(u=>u.Index==130).Hp);
            Assert.Contains($"A2 (9,4) HP 5 EXP {counterExp}",view.AllyStatus);Assert.Contains("Input closed",view.Controls);
            if (firstKill)
            {
                for (int step = 0; current.FirstRound!.RoundNumber != 12; step++)
                {
                    Assert.InRange(step,0,15);
                    try { current = current.FirstRound.CurrentCandidate is null ? Battle01FirstRound.EnterNext(current) : CompleteAuthoredTurn(current); }
                    catch (Battle01AttackSelectionRequiredException)
                    {
                        current = Battle01EnemyPhysicalAttack.CompleteNext(current,current.FirstRound!.CurrentCandidate!.Value.CombatantIndex,
                            Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike);
                    }
                }
                current = Battle01NextPlayerControl.Enter(current,2).State!;
                current = Battle01PlayerPhysicalAttack.Begin(Battle01PlayerMovement.Confirm(current,2),2);
                for (int i = 0; current.FirstControl!.Movement.Attack!.TargetIndex != 129; i++)
                {
                    Assert.InRange(i,0,3); current = Battle01PlayerPhysicalAttack.Cycle(current,2,1);
                }
                current = Battle01PlayerPhysicalAttack.Confirm(current,2,Battle01PlayerPhysicalCompletionPolicy.ControlledChesterFirstKill);
                var killed = PrivateBattle01Presenter.BuildProjection(current,"Chester first kill complete.");
                Assert.Equal(((uint?)180,(ushort?)2,(ushort?)1),(killed.Gold,killed.BowieKills,killed.ChesterKills));
                Assert.Equal((2,128),(killed.CompletedActorIndex,killed.NextCandidateIndex));
                Assert.DoesNotContain(killed.Units,u => u.Index == 129);
                Assert.Contains("A2 (9,4) HP 1 EXP 54",killed.AllyStatus);
                Assert.Contains("EXP +24: 30 -> 54",killed.AttackResult);
                Assert.Contains("Chester kills 0 -> 1",killed.AttackResult);
                Assert.False(killed.CanConfirm); Assert.Contains("Input closed",killed.Controls);
                Assert.Equal("attack.lethal",Assert.Throws<Battle01PhysicalAttackUnsupportedException>(() =>
                    Battle01EnemyPhysicalAttack.CompleteNext(current,128,Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike)).ParamName);
                current=Battle01EnemyPhysicalAttack.CompleteNext(current,128,Battle01PhysicalCompletionPolicy.ControlledChesterDefeatAfterFirstKill);
                var defeated=PrivateBattle01Presenter.BuildProjection(current,"Enemy128 completed.");
                Assert.Equal((128,0),(defeated.CompletedActorIndex,defeated.NextCandidateIndex));
                Assert.Equal(5,defeated.Units.Count);Assert.DoesNotContain(defeated.Units,u=>u.Index is 2 or 129 or 131 or 132);
                Assert.Contains("A2 defeated HP 0 EXP 54 Defeats 1",defeated.AllyStatus);
                Assert.Contains("E0 -> A2: hit 2. HP 1 -> 0.",defeated.AttackResult);
                Assert.Contains("A2 (2) defeated. Defeats 0 -> 1.",defeated.AttackResult);
                Assert.Equal(((uint?)180,(ushort?)2,(ushort?)1),(defeated.Gold,defeated.BowieKills,defeated.ChesterKills));
                Assert.False(defeated.CanRequestDefeatReturn);Assert.False(defeated.CanEnterExploration);
                current=Battle01NextPlayerControl.Enter(current,current.FirstRound!.CurrentCandidate!.Value.CombatantIndex).State!;
                var bowieReady=PrivateBattle01Presenter.BuildProjection(current,"Actual Bowie ready.");
                Assert.Equal((0,12),(bowieReady.ActorIndex,bowieReady.Budget));Assert.True(bowieReady.CanConfirm);
                Assert.Equal(defeated.AttackResult,bowieReady.AttackResult);
                current=Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(current,0,new(11,14)),0);
                var bowieMoved=PrivateBattle01Presenter.BuildProjection(current,"Bowie provisional.");
                Assert.Equal(new MapPosition(11,14),bowieMoved.Units.Single(u=>u.Index==0).Position);
                Assert.Contains("Backspace",bowieMoved.Controls);
                current=Battle01PlayerMovement.Cancel(current,0);
                var bowieCancelled=PrivateBattle01Presenter.BuildProjection(current,"Bowie cancelled.");
                Assert.Equal(bowieReady.Units,bowieCancelled.Units);Assert.Equal(defeated.AttackResult,bowieCancelled.AttackResult);
                Assert.Equal((0x98321234u,(ushort?)0x0234),(current.RandomSeedImage,current.RandomSeedCopy));
            }
            return;
        }
        if (!firstAlly) return;
        current = Battle01PlayerMovement.SelectDestination(current, 2, new(9, 9));
        current = Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(current, 2), 2,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        for (int step = 0; step < 40; step++)
        {
            var order = current.FirstRound!;
            if (order.RoundNumber == 13 && order.CurrentTurnOffset == 10 && order.CurrentCandidate?.CombatantIndex == 133) break;
            if (order.CurrentCandidate is null) current = Battle01FirstRound.EnterNext(current);
            else
            {
                try { current = CompleteAuthoredTurn(current); }
                catch (Battle01AttackSelectionRequiredException)
                {
                    current = Battle01EnemyPhysicalAttack.CompleteNext(current, order.CurrentCandidate.Value.CombatantIndex,
                        Battle01PhysicalCompletionPolicy.ControlledNonlethalStrike);
                }
            }
        }
        Assert.Equal((13, (byte)10), (current.FirstRound!.RoundNumber, current.FirstRound.CurrentTurnOffset));
        current = Battle01EnemyPhysicalAttack.CompleteNext(current, 133, Battle01PhysicalCompletionPolicy.ControlledFirstAllyDefeat);
        var allyDefeated = PrivateBattle01Presenter.BuildProjection(current, "First ally defeated.");
        Assert.Equal(6, allyDefeated.Units.Count); Assert.DoesNotContain(allyDefeated.Units, u => u.Index is 2 or 131 or 132);
        Assert.Contains("HP 1 -> 0", allyDefeated.AttackResult); Assert.Contains("A2 (2) defeated. Defeats 0 -> 1.", allyDefeated.AttackResult);
        Assert.Contains("A2 defeated HP 0 EXP 10 Defeats 1", allyDefeated.AllyStatus);
        Assert.Null(allyDefeated.ActorIndex); Assert.Equal(133, allyDefeated.CompletedActorIndex); Assert.Equal(130, allyDefeated.NextCandidateIndex);
        current = Battle01EnemyPursuit.CompleteNext(current, 130, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        current = Battle01NextPlayerControl.Enter(Battle01FirstRound.EnterNext(current), 1).State!;
        var sarah = PrivateBattle01Presenter.BuildProjection(current, "Round 14. Player 1 ready.");
        Assert.Equal((1, 10, true), (sarah.ActorIndex, sarah.Budget, sarah.CanConfirm));
        Assert.Equal(allyDefeated.AllyStatus, sarah.AllyStatus); Assert.Equal(allyDefeated.AttackResult, sarah.AttackResult);
        current = Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(current, 1, new(10, 17)), 1);
        var movedSarah = PrivateBattle01Presenter.BuildProjection(current, "Sarah provisional.");
        Assert.Equal(new MapPosition(10, 17), movedSarah.Units.Single(u => u.Index == 1).Position);
        current = Battle01PlayerMovement.Cancel(current, 1);
        var cancelledSarah = PrivateBattle01Presenter.BuildProjection(current, "Sarah cancelled.");
        Assert.Equal(sarah.Units, cancelledSarah.Units); Assert.Equal(sarah.AllyStatus, cancelledSarah.AllyStatus);
        Assert.Equal(sarah.AttackResult, cancelledSarah.AttackResult);
        Assert.Equal(((uint?)120, (ushort?)2), (cancelledSarah.Gold, cancelledSarah.BowieKills));
        if (!leader) return;
        current = Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(current, 1), 1,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        for (int step = 0; step < 24; step++)
        {
            if (current.FirstRound!.RoundNumber == 16 && current.FirstRound.CurrentTurnOffset == 0) break;
            current = current.FirstRound.CurrentCandidate is null ? Battle01FirstRound.EnterNext(current) : CompleteAuthoredTurn(current);
        }
        current = Battle01EnemyPhysicalAttack.CompleteNext(current, 129, Battle01PhysicalCompletionPolicy.ControlledLeaderDefeatPending);
        var terminal = PrivateBattle01Presenter.BuildProjection(current, "Bowie defeated.");
        Assert.Equal(Battle01Phase.DefeatPending, terminal.Phase);
        Assert.Null(terminal.ActorIndex); Assert.Null(terminal.CompletedActorIndex); Assert.Null(terminal.NextCandidateIndex);
        Assert.Null(terminal.Cursor); Assert.Null(terminal.SelectedTargetIndex); Assert.Empty(terminal.Path);
        Assert.False(terminal.CanConfirm); Assert.False(terminal.PursuitCompleted); Assert.True(terminal.PhysicalAttackCompleted);
        Assert.All(terminal.Tiles, tile => { Assert.False(tile.Reachable); Assert.False(tile.CanStop); });
        Assert.Equal(5, terminal.Units.Count); Assert.DoesNotContain(terminal.Units, unit => unit.Index is 0 or 2 or 131 or 132);
        Assert.Contains("HP 3 -> 0. Bowie defeated. Defeats 0 -> 1", terminal.AttackResult);
        Assert.Contains("A0 defeated HP 0 EXP 63 Defeats 1", terminal.AllyStatus);
        Assert.Contains("A2 defeated HP 0 EXP 10 Defeats 1", terminal.AllyStatus);
        Assert.Contains("Space", terminal.Controls); Assert.Contains("Return unavailable", terminal.Controls);
        var recovery = Battle01DefeatRecovery.Complete(current);
        var recovered = PrivateBattle01Presenter.BuildProjection(recovery, "Recovery applied.");
        Assert.Equal(Battle01Phase.DefeatRecoveryPending, recovered.Phase);
        Assert.Equal(terminal.Units, recovered.Units); Assert.Null(recovered.ActorIndex);
        Assert.Null(recovered.NextCandidateIndex); Assert.Null(recovered.Cursor); Assert.False(recovered.CanConfirm);
        Assert.Contains("A0 unplaced HP 12 EXP 63 Defeats 1", recovered.AllyStatus);
        Assert.Contains("A2 defeated HP 0 EXP 10 Defeats 1", recovered.AllyStatus);
        Assert.Equal(terminal.AttackResult, recovered.AttackResult);
        Assert.Equal((uint?)60, recovered.Gold); Assert.Equal((ushort?)2, recovered.BowieKills);
        Assert.Contains("Input closed", recovered.Controls); Assert.DoesNotContain("Space", recovered.Controls);
        Assert.Equal(((uint?)120, (ushort?)2), (terminal.Gold, terminal.BowieKills));
        if (returnSelected)
        {
            Assert.Same(admission, recovery.ReturnAdmission); Assert.Same(admission, current.ReturnAdmission);
            var eligible = PrivateBattle01Presenter.BuildProjection(recovery, "Return unavailable", true);
            Assert.True(eligible.CanRequestDefeatReturn); Assert.Contains("Space: request", eligible.Controls);
            var request = Battle01DefeatReturn.Select(recovery, admission);
            var requested = PrivateBattle01Presenter.BuildProjection(recovery, PrivateBattle01Presenter.ReturnRequestedStatus, false, request);
            Assert.Same(request, requested.DefeatReturn); Assert.False(requested.CanRequestDefeatReturn);
            Assert.Equal(recovered.Units, requested.Units); Assert.Equal(recovered.AttackResult, requested.AttackResult);
            Assert.Equal(recovered.AllyStatus, requested.AllyStatus); Assert.Equal(recovered.Gold, requested.Gold);
            Assert.Contains("Input closed", requested.Controls); Assert.DoesNotContain("Space", requested.Controls);
            Assert.Equal("Return requested: Granseal (32,13), facing up. Exploration unavailable.", requested.Status);
            var entry = PrivateBattle01Presenter.BuildProjection(recovery, requested.Status, false, request, true);
            Assert.True(entry.CanEnterExploration); Assert.Contains("Space: enter Granseal", entry.Controls);
            Assert.Equal(requested.Units, entry.Units); Assert.Equal(requested.AllyStatus, entry.AllyStatus);
            Assert.False(requested.CanEnterExploration);
            // Correct-looking status text alone cannot enable return on the legacy projection.
            Assert.False(PrivateBattle01Presenter.BuildProjection(recovery, "Space requests return").CanRequestDefeatReturn);
        }
    }

    private static Battle01InitializedState CompleteAuthoredTurn(Battle01InitializedState current)
    {
        int actor=current.FirstRound!.CurrentCandidate!.Value.CombatantIndex;
        if(actor>=128)
            return (current.Roster.Single(unit=>unit.Index==actor).AiBitfield!.Value&1)!=0
                ? Battle01EnemyPursuit.CompleteNext(current,actor,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats)
                : Battle01EnemyStandby.CompleteNext(current,actor,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        var ready=Battle01NextPlayerControl.Enter(current,actor).State!;
        return Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(ready,actor),actor,
            Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
    }

    [Fact]
    public void NewRoundProjectionKeepsTheGeneratedCandidateVisibleWithoutReusingOldCompletion()
    {
        var current = Battle01EnemyStandby.CompleteFirst(AuthoredSecondCompleted(),128,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        for (int i=0;i<5;i++) current = Battle01EnemyStandby.CompleteNext(current,
            current.FirstRound!.CurrentCandidate!.Value.CombatantIndex,Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        var firstEnd = Stay(Battle01NextPlayerControl.Enter(current,0).State!,0,new(8,17));
        var generated = Battle01FirstRound.EnterNext(firstEnd);
        var view = PrivateBattle01Presenter.BuildProjection(generated,"Control rejected: terrain; actor 2; current state retained.");
        Assert.Equal(Battle01Phase.RoundGenerated,view.Phase); Assert.Null(view.CompletedActorIndex);
        Assert.Equal(2,view.ActorIndex); Assert.Null(view.Cursor); Assert.Empty(view.Path);
        Assert.Equal(1,generated.TurnCompletion!.RoundNumber);
        Assert.DoesNotContain("Round exhausted",view.Controls);
        Assert.Contains("terrain",view.Status);
        var ready = Battle01NextPlayerControl.Enter(generated,2).State!;
        var readyView = PrivateBattle01Presenter.BuildProjection(ready,"Round 2. Player 2 ready.");
        Assert.Equal(2,readyView.ActorIndex); Assert.Equal(new MapPosition(7,17),readyView.Cursor);
        Assert.Null(readyView.CompletedActorIndex); Assert.Contains("Space",readyView.Controls);
        Assert.Same(firstEnd.TurnCompletion,ready.TurnCompletion);
    }
    [Theory]
    [InlineData(GameFlowStage.Exploration, false, false, false)]
    [InlineData(GameFlowStage.Exploration, false, true, false)]
    [InlineData(GameFlowStage.Exploration, true, false, false)]
    [InlineData(GameFlowStage.Exploration, true, true, true)]
    [InlineData(GameFlowStage.Battle, false, false, true)]
    [InlineData(GameFlowStage.Battle, false, true, true)]
    public void BattleRouteClosesOldInputEvenAfterPendingIsConsumed(GameFlowStage stage,
        bool pending, bool selected, bool expected) =>
        Assert.Equal(expected, PrivateBattle01Ui.OwnsInput(stage, pending, selected));

    [Fact]
    public void ProjectionTracksLiveRelocationCancelAndActualCandidateRatherThanDeploymentOrActorOne()
    {
        var initial = AuthoredBattle();
        var round = Battle01FirstRound.Enter(initial);
        Assert.Equal(2, round.FirstRound!.FirstCandidate!.Value.CombatantIndex);
        var ready = Battle01FirstControl.Enter(round, 2).State!;
        var before = PrivateBattle01Presenter.BuildProjection(ready, "ready");
        Assert.Equal(2, before.ActorIndex);
        Assert.Equal(9, before.Units.Count);
        Assert.Equal(320, before.Tiles.Count);
        Assert.Equal(new MapPosition(2, 1), before.Cursor);
        Assert.True(before.Tiles.Single(tile => tile.Position == new MapPosition(2, 2)).CanStop);
        Assert.False(before.Tiles.Single(tile => tile.Position == new MapPosition(15, 19)).Reachable);
        var selected = Battle01PlayerMovement.SelectDestination(ready, 2, new(2, 2));
        var preview = PrivateBattle01Presenter.BuildProjection(selected, "selected");
        Assert.Equal(new MapPosition(2, 1), preview.Units.Single(unit => unit.Index == 2).Position);
        Assert.Equal(new MapPosition(2, 2), preview.Cursor);
        Assert.Equal(new MapPosition[] { new(2, 1), new(2, 2) }, preview.Path);
        Assert.Equal(2, preview.PathCost);
        Assert.Equal(2, preview.GridCost);
        Assert.Equal(10, preview.Budget);
        Assert.True(preview.CanConfirm);
        var confirmed = Battle01PlayerMovement.Confirm(selected, 2);
        var action = PrivateBattle01Presenter.BuildProjection(confirmed, "confirmed");
        Assert.Equal(Battle01Phase.PlayerActionChoice, action.Phase);
        Assert.Equal(new MapPosition(2, 2), action.Units.Single(unit => unit.Index == 2).Position);
        Assert.Equal(new MapPosition(2, 1), confirmed.Roster[2].Deployment.Position);
        Assert.False(action.CanConfirm);
        Assert.Contains("Space: STAY", action.Controls);
        Assert.Contains("Backspace", action.Controls);
        var cancelled = Battle01PlayerMovement.Cancel(confirmed, 2);
        var restored = PrivateBattle01Presenter.BuildProjection(cancelled, "cancelled");
        Assert.Equal(before.Units, restored.Units);
        Assert.Equal(before.Cursor, restored.Cursor);
        Assert.Equal(0, restored.PathCost);
        Assert.Equal(ready.Occupancy, cancelled.Occupancy);
        Assert.Same(round.FirstRound, cancelled.FirstRound);
        Assert.Equal(round.RandomSeedImage, cancelled.RandomSeedImage);
    }

    [Fact]
    public void EarlierFailurePhasesRemainBattleProjectionsWithoutMovementControls()
    {
        var initial = AuthoredBattle();
        foreach (var battle in new[] { initial, Battle01FirstRound.Enter(initial) })
        {
            var projection = PrivateBattle01Presenter.BuildProjection(battle, "First control unavailable");
            Assert.Equal(battle.Phase, projection.Phase);
            Assert.Null(projection.Cursor);
            Assert.Empty(projection.Path);
            Assert.False(projection.CanConfirm);
            Assert.DoesNotContain("Space", projection.Controls);
            Assert.Equal(9, projection.Units.Count);
            Assert.Contains("Current battle retained", projection.Controls);
        }
        Assert.Contains("DIAGNOSTIC", PrivateBattle01Presenter.Heading);
        Assert.Contains("Original Map 57 graphics unavailable", PrivateBattle01Presenter.Boundary);
    }

    [Fact]
    public void BaseArtAndLiveOverlaysShareTheUnscaledTwentyFourPixelGrid()
    {
        Assert.Equal(24, PrivateBattle01Presenter.TileSize);
        Assert.Equal(PrivateOriginalMapBaseViewProjection.BlockPixelSize, PrivateBattle01Presenter.TileSize);
        var origin = PrivateBattle01Presenter.Cell(new(0, 0));
        Assert.Equal(384, PrivateBattle01Presenter.Cell(new(16, 0)).X - origin.X);
        Assert.Equal(480, PrivateBattle01Presenter.Cell(new(0, 20)).Y - origin.Y);
        Assert.True(origin.Y + 480 <= 540);
        Assert.Contains("DIAGNOSTIC UNITS", PrivateBattle01Presenter.BaseArtHeading);
        var ready = Battle01FirstControl.Enter(Battle01FirstRound.Enter(AuthoredBattle()), 2).State!;
        var selected = PrivateBattle01Presenter.BuildProjection(Battle01PlayerMovement.SelectDestination(ready, 2, new(2, 2)), "");
        Assert.Equal(24, PrivateBattle01Presenter.Cell(selected.Cursor!).Y -
            PrivateBattle01Presenter.Cell(selected.Units.Single(unit => unit.Index == 2).Position).Y);
        Assert.Equal(PrivateBattle01Presenter.Cell(selected.Cursor!), PrivateBattle01Presenter.Cell(selected.Path[^1]));
    }

    [Fact]
    public void CompletedStayShowsHistoricalActorAndUndispatchedCandidateWithoutOldInteraction()
    {
        var ready = Battle01FirstControl.Enter(Battle01FirstRound.Enter(AuthoredBattle()), 2).State!;
        var action = Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(ready, 2, new(2, 2)), 2);
        var completed = Battle01TurnCompletion.CommitStay(action, 2, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        var view = PrivateBattle01Presenter.BuildProjection(completed, "STAY complete");
        Assert.Null(view.ActorIndex); Assert.Equal(2, view.CompletedActorIndex);
        Assert.Equal((int?)action.FirstRound!.Slots[1].CombatantIndex, view.NextCandidateIndex);
        Assert.Equal(new MapPosition(2, 2), view.Units.Single(unit => unit.Index == 2).Position);
        Assert.Null(view.Cursor); Assert.Empty(view.Path); Assert.Null(view.GridCost); Assert.Null(view.PathCost); Assert.Null(view.Budget);
        Assert.All(view.Tiles, tile => { Assert.False(tile.Reachable); Assert.False(tile.CanStop); });
        Assert.False(view.CanConfirm); Assert.DoesNotContain("Space", view.Controls); Assert.Contains("Candidate not started", view.Controls);
    }

    [Fact]
    public void PreviousCompletionDoesNotHideTheNextPlayersIndependentMovementProjection()
    {
        var ready = Battle01FirstControl.Enter(Battle01FirstRound.Enter(AuthoredBattle(nextPlayer: true)), 2).State!;
        var action = Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(ready, 2, new(2, 2)), 2);
        var completed = Battle01TurnCompletion.CommitStay(action, 2, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        Assert.Equal((byte)1, completed.FirstRound!.CurrentCandidate!.Value.CombatantIndex);
        var next = Battle01NextPlayerControl.Enter(completed, 1).State!;
        var view = PrivateBattle01Presenter.BuildProjection(next, "next player ready");
        Assert.Equal(1, view.ActorIndex); Assert.Null(view.CompletedActorIndex); Assert.Null(view.NextCandidateIndex);
        Assert.Equal(Battle01Phase.PlayerMovementSelection, view.Phase);
        Assert.Equal(new MapPosition(1, 1), view.Cursor); Assert.Equal(14, view.Budget);
        Assert.Equal(new MapPosition(2, 2), view.Units.Single(unit => unit.Index == 2).Position);
        Assert.Same(completed.TurnCompletion, next.TurnCompletion);
        Assert.Contains("Space", view.Controls); Assert.Contains(view.Tiles, tile => tile.CanStop);
    }

    private static Battle01InitializedState AuthoredSecondCompleted(bool regionEntry=false, bool combatProfile=false,
        bool chesterPlayer=false, bool firstAlly=false, bool leader=false, Battle01DefeatReturnAdmission? returnAdmission=null, bool firstKill=false)
    {
        MapPosition[] positions = [new(8, 18), new(9, 18), new(7, 18), new(7, 3), new(9, 4), new(6, 4), new(8, 3), new(9, 5), new(6, 5)];
        var rows = Enumerable.Range(0, 9).Select(i => new Battle01Deployment((byte)i, i < 3 ? i : 125 + i,
            (byte)(i < 3 ? i : 39), positions[i], (byte)(i >= 7 ? 7 : i >= 3 ? 6 : 0), 127, 255,
            (byte)(i < 6 ? 2 : i < 8 ? 1 : 0), 255, 15, (byte)(i >= 7 ? 112 : i >= 3 ? 96 : 0), 0));
        IEnumerable<Battle01Region> regions = Enumerable.Range(0, 3).Select(i => new Battle01Region((byte)i, 0,
            regionEntry && i==1 ? [new(10,14),new(12,14),new(12,16),new(10,16)] : [new(0, 0), new(1, 0), new(1, 1), new(0, 1)], 0, 0));
        if (firstAlly) regions = [
            new(0, 0, [new(0, 0), new(0, 19), new(15, 7), new(15, 0)], 0, 0),
            new(1, 0, [new(0, 0), new(0, 7), new(15, 19), new(15, 0)], 0, 0),
            new(2, 0, [new(0, 0), new(0, 12), new(15, 12), new(15, 0)], 0, 0)];
        byte[] agility = [4, 5, 7];
        var party = Enumerable.Range(0, 3).Select(i => new Battle01AllyInput((byte)i, (byte)(i == 0 ? 0 : i == 1 ? 4 : 1),
            chesterPlayer && i==1 ? new(1, 11, 11, 10, 10, 9, 5, 5, 5, 0, [213,0,0,127], [0,63,63,63]) :
            combatProfile && i==2 ? new(1, 11, 11, 0, 0, 8, 5, 7, 7, 0, [184,0,127,127], [63,63,63,63], chesterPlayer ? (byte?)0 : null, firstKill ? (ushort?)0 : null, firstAlly || firstKill ? (ushort?)0 : null) :
            new(1, 12, 12, 8, 8, 9, 4, agility[i], (byte)(i == 0 ? 6 : i == 2 ? 7 : 5), 0,
                combatProfile && i==0 ? [199,0,127,127] : [127,127,127,127],
                combatProfile && i==0 ? [10,63,63,63] : [63,63,63,63], combatProfile && i==0 ? (byte?)0 : null,
                combatProfile && i==0 ? (ushort?)0 : null, leader && i==0 ? (ushort?)0 : null)));
        var enemy = new Battle01EnemyInput(39, 39, 0, new(0, 5, 5, 0, 0, 7, 5, 5, 5, 0,
            [127, 127, 127, 127], [63, 63, 63, 63]), 0x40E3, 0, 6, 0x2000);
        var terrain = Enumerable.Repeat((byte)1, 2304).ToArray(); terrain[4 * 48 + 7] = 255;
        if (firstKill) terrain[4 * 48 + 10] = 0;
        var initial = Battle01Initialization.Initialize(rows, regions, terrain, party, enemy, 0x1234, 0, 0x1234,
            combatProfile ? (uint?)0 : null, returnAdmission);
        var ready = Battle01FirstControl.Enter(Battle01FirstRound.Enter(initial), 1).State!;
        var first = Stay(ready, 1, new(9, 17)); var next = Battle01NextPlayerControl.Enter(first, 2).State!;
        return Stay(next, 2, new(7, 17));
    }

    [Fact]
    public void EnemyStandbyProjectionCanRetainTheLastCompletedEnemyAtAMidRelayBoundary()
    {
        var completed = Battle01EnemyStandby.CompleteFirst(AuthoredSecondCompleted(), 128, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        var view = PrivateBattle01Presenter.BuildProjection(completed, "Enemy standby: E0 moved; STAY complete.");
        Assert.Equal(Battle01Phase.EnemyTurnCompleted, view.Phase); Assert.Null(view.ActorIndex);
        Assert.Equal(128, view.CompletedActorIndex); Assert.Equal(131, view.NextCandidateIndex);
        Assert.Equal(new MapPosition(6, 3), view.Units.Single(unit => unit.Index == 128).Position);
        Assert.Equal(new MapPosition(8, 3), view.Units.Single(unit => unit.Index == 131).Position);
        Assert.Null(view.Cursor); Assert.Empty(view.Path); Assert.False(view.CanConfirm);
        Assert.All(view.Tiles, tile => { Assert.False(tile.Reachable); Assert.False(tile.CanStop); });
        Assert.Contains("Input closed", view.Controls); Assert.Contains("Candidate not started", view.Controls); Assert.Contains("Enemy standby", view.Status);
    }

    [Fact]
    public void BowieProjectionShowsHisIndependentControlThenTheExhaustedSentinelWithoutOldOverlays()
    {
        var current = Battle01EnemyStandby.CompleteFirst(AuthoredSecondCompleted(), 128, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        for (int i = 0; i < 5; i++) current = Battle01EnemyStandby.CompleteNext(current,
            current.FirstRound!.CurrentCandidate!.Value.CombatantIndex, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);
        var ready = Battle01NextPlayerControl.Enter(current, 0).State!;
        var view = PrivateBattle01Presenter.BuildProjection(ready, "Bowie ready");
        Assert.Equal(0, view.ActorIndex); Assert.Null(view.CompletedActorIndex); Assert.Equal(12, view.Budget);
        Assert.Equal(new MapPosition(8,18), view.Cursor); Assert.Contains(view.Tiles, tile => tile.CanStop); Assert.Contains("Space", view.Controls);
        var selected = Battle01PlayerMovement.SelectDestination(ready, 0, new(8,17)); var moved = Battle01PlayerMovement.Confirm(selected, 0);
        var provisional = PrivateBattle01Presenter.BuildProjection(moved, "provisional");
        Assert.Equal(new MapPosition(8,17), provisional.Units.Single(unit => unit.Index == 0).Position);
        Assert.Contains("Space: STAY", provisional.Controls); Assert.Equal(2, provisional.PathCost);
        var cancelled = Battle01PlayerMovement.Cancel(moved, 0);
        Assert.Equal(view.Units, PrivateBattle01Presenter.BuildProjection(cancelled, "cancelled").Units);
        var exhausted = Stay(cancelled, 0, new(8,17)); var end = PrivateBattle01Presenter.BuildProjection(exhausted, "First round exhausted");
        Assert.Null(end.ActorIndex); Assert.Equal(0, end.CompletedActorIndex); Assert.Null(end.NextCandidateIndex);
        Assert.Null(end.Cursor); Assert.Empty(end.Path); Assert.False(end.CanConfirm); Assert.Null(end.Budget);
        Assert.All(end.Tiles, tile => { Assert.False(tile.Reachable); Assert.False(tile.CanStop); });
        Assert.Contains("Round exhausted", end.Controls); Assert.DoesNotContain("Space", end.Controls);
    }

    private static Battle01InitializedState Stay(Battle01InitializedState state, int actor, MapPosition target) =>
        Battle01TurnCompletion.CommitStay(Battle01PlayerMovement.Confirm(Battle01PlayerMovement.SelectDestination(state, actor, target), actor),
            actor, Battle01StayCompletionPolicy.ControlledUnchangedEffectiveStats);

    // Public authored geometry and party, deliberately with actor 2 as the first candidate.
    private static Battle01InitializedState AuthoredBattle(bool nextPlayer = false)
    {
        var rows = Enumerable.Range(0, 9).Select(index => new Battle01Deployment((byte)index,
            index < 3 ? index : 128 + index - 3, (byte)(index < 3 ? index : 39), new(index, 1),
            0, 127, 255, 0, 255, 15, 0, 0));
        var regions = Enumerable.Range(0, 3).Select(index => new Battle01Region((byte)index, 0,
            [new(0, 0), new(1, 0), new(1, 1), new(0, 1)], 0, 0));
        var allies = Enumerable.Range(0, 3).Select(index => new Battle01AllyInput((byte)index, (byte)(nextPlayer && index == 1 ? 1 : 4),
            new(1, 12, 12, 8, 8, 9, 4, (byte)(index == 2 ? 50 : nextPlayer && index == 1 ? 30 : 4),
                (byte)(nextPlayer && index == 1 ? 7 : 5), 0,
                [127, 127, 127, 127], [63, 63, 63, 63])));
        var enemy = new Battle01EnemyInput(39, 39, 0,
            new(0, 5, 5, 0, 0, 7, 5, 5, 5, 0, [127, 127, 127, 127], [63, 63, 63, 63]),
            0x40E3, 0, 6, 0x2000);
        return Battle01Initialization.Initialize(rows, regions, Enumerable.Repeat((byte)1, 2304),
            allies, enemy, 0x1234, 0);
    }
}
