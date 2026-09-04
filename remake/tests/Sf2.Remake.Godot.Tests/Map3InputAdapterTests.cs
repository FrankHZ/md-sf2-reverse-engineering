using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter;
using Xunit;

namespace Sf2.Remake.Godot.Tests;

public sealed class Map3InputAdapterTests
{
    [Fact]
    public void BindingsPreserveExactActionAndKeyOrder()
    {
        (string ActionName, Key PhysicalKey)[] expected =
        [
            ("move_north", Key.W),
            ("move_east", Key.D),
            ("move_south", Key.S),
            ("move_west", Key.A),
            ("select_context", Key.Enter),
            ("request_event", Key.Z),
            ("acknowledge_event", Key.X),
            ("request_transition", Key.C),
            ("acknowledge_transition", Key.V),
            ("turn_north", Key.Up),
            ("turn_east", Key.Right),
            ("turn_south", Key.Down),
            ("turn_west", Key.Left),
            ("request_entity_interaction", Key.F),
            ("acknowledge_entity_interaction", Key.G),
            ("advance_dialogue", Key.H),
            ("request_field_search", Key.Q),
            ("acknowledge_field_search", Key.E),
            ("request_item_acquisition", Key.R),
            ("acknowledge_item_acquisition", Key.T),
            ("request_outbound_transition", Key.Y),
            ("acknowledge_outbound_transition", Key.U),
            ("public_synthetic_battle_request", Key.B),
            ("public_synthetic_battle_acknowledge_entry", Key.N),
            ("public_synthetic_battle_cursor_north", Key.I),
            ("public_synthetic_battle_cursor_east", Key.L),
            ("public_synthetic_battle_cursor_south", Key.K),
            ("public_synthetic_battle_cursor_west", Key.J),
            ("public_synthetic_battle_confirm", Key.Space),
            ("public_synthetic_battle_cancel", Key.Backspace),
            ("public_synthetic_battle_acknowledge_completion", Key.M),
        ];

        Assert.Equal(
            expected,
            Map3InputAdapter.Bindings.Select(
                binding => (binding.ActionName, binding.PhysicalKey)));
        Assert.Equal(
            expected.Length,
            expected.Select(binding => binding.ActionName).Distinct().Count());
        Assert.Equal(
            expected.Length,
            expected.Select(binding => binding.PhysicalKey).Distinct().Count());
        Assert.Equal(
            new[]
            {
                ExplorationDirection.North,
                ExplorationDirection.East,
                ExplorationDirection.South,
                ExplorationDirection.West,
            },
            Map3InputAdapter.Bindings
                .Select(binding => binding.PrivateMovementDirection)
                .OfType<ExplorationDirection>());
        Assert.All(
            Map3InputAdapter.Bindings.Skip(4),
            binding => Assert.Null(binding.PrivateMovementDirection));
    }

    [Fact]
    public void EveryBindingDispatchesItsExactSemanticAction()
    {
        foreach (Map3InputBinding binding in Map3InputAdapter.Bindings)
        {
            ActionProbe probe = new();
            Map3InputAdapter adapter = new(
                CreateRecordingActions(probe),
                action => action == binding.ActionName);

            adapter.PollPublicSynthetic();

            Assert.Equal(1, probe.TotalCalls);
            AssertExpectedCallback(binding.ActionName, probe);
        }
    }

    [Fact]
    public void NoPressedActionProducesNoSemanticCall()
    {
        ActionProbe probe = new();
        Map3InputAdapter adapter = new(CreateRecordingActions(probe), _ => false);

        adapter.PollPublicSynthetic();

        Assert.Equal(0, probe.TotalCalls);
    }

    [Fact]
    public void SimultaneousActionsDispatchOnlyTheFirstBinding()
    {
        ActionProbe probe = new();
        List<string> inputProbes = [];
        Map3InputAdapter adapter = new(
            CreateRecordingActions(probe),
            action =>
            {
                inputProbes.Add(action);
                return true;
            });

        adapter.PollPublicSynthetic();

        Assert.Equal(1, probe.TotalCalls);
        Assert.Equal([ExplorationDirection.North], probe.Moves);
        Assert.Empty(probe.Turns);
        Assert.Equal(["move_north"], inputProbes);
    }

    [Theory]
    [InlineData("move_north", ExplorationDirection.North)]
    [InlineData("move_east", ExplorationDirection.East)]
    [InlineData("move_south", ExplorationDirection.South)]
    [InlineData("move_west", ExplorationDirection.West)]
    public void PrivateMovementPollingReturnsTheExactSemanticDirectionWithoutDispatch(
        string pressedAction,
        ExplorationDirection expectedDirection)
    {
        ActionProbe probe = new();
        Map3InputAdapter adapter = new(
            CreateRecordingActions(probe),
            action => action == pressedAction);

        ExplorationDirection? direction = adapter.PollPrivateOriginalMapMovement();

        Assert.Equal(expectedDirection, direction);
        Assert.Equal(0, probe.TotalCalls);
    }

    [Fact]
    public void PrivateMovementPollingPreservesNorthEastSouthWestPriority()
    {
        ActionProbe probe = new();
        List<string> inputProbes = [];
        Map3InputAdapter adapter = new(
            CreateRecordingActions(probe),
            action =>
            {
                inputProbes.Add(action);
                return true;
            });

        ExplorationDirection? direction = adapter.PollPrivateOriginalMapMovement();

        Assert.Equal(ExplorationDirection.North, direction);
        Assert.Equal(["move_north"], inputProbes);
        Assert.Equal(0, probe.TotalCalls);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("select_context")]
    [InlineData("request_event")]
    public void PrivateMovementPollingReturnsNullForNoneOrPublicOnlyInput(
        string? pressedAction)
    {
        ActionProbe probe = new();
        Map3InputAdapter adapter = new(
            CreateRecordingActions(probe),
            action => action == pressedAction);

        ExplorationDirection? direction = adapter.PollPrivateOriginalMapMovement();

        Assert.Null(direction);
        Assert.Equal(0, probe.TotalCalls);
    }

    [Fact]
    public void PrivatePollingDispatchesOnlyTheSemanticEntityInteractionAction()
    {
        ActionProbe probe = new();
        Map3InputAdapter adapter = new(
            CreateRecordingActions(probe),
            action => action == "request_entity_interaction");

        ExplorationDirection? direction = adapter.PollPrivateOriginalMapMovement();

        Assert.Null(direction);
        Assert.Equal(1, probe.RequestEntityInteraction);
        Assert.Equal(1, probe.TotalCalls);
    }

    private static Map3InputActions CreateRecordingActions(ActionProbe probe) =>
        new(
            direction => probe.RecordMove(direction),
            () => probe.Record(ref probe.SelectContext),
            () => probe.Record(ref probe.RequestEvent),
            () => probe.Record(ref probe.AcknowledgeEvent),
            () => probe.Record(ref probe.RequestLocalTransition),
            () => probe.Record(ref probe.AcknowledgeLocalTransition),
            facing => probe.RecordTurn(facing),
            () => probe.Record(ref probe.RequestEntityInteraction),
            () => probe.Record(ref probe.AcknowledgeEntityInteraction),
            () => probe.Record(ref probe.AdvanceDialogue),
            () => probe.Record(ref probe.RequestFieldSearch),
            () => probe.Record(ref probe.AcknowledgeFieldSearch),
            () => probe.Record(ref probe.RequestItemAcquisition),
            () => probe.Record(ref probe.AcknowledgeItemAcquisition),
            () => probe.Record(ref probe.RequestOutboundTransition),
            () => probe.Record(ref probe.AcknowledgeOutboundTransition),
            () => probe.Record(ref probe.RequestPublicSyntheticBattle),
            () => probe.Record(ref probe.AcknowledgePublicSyntheticBattle),
            direction => probe.RecordBattleCursor(direction),
            () => probe.Record(ref probe.ConfirmPublicSyntheticBattle),
            () => probe.Record(ref probe.CancelPublicSyntheticBattle),
            () => probe.Record(ref probe.AcknowledgePublicSyntheticBattleCompletion));

    private static void AssertExpectedCallback(string actionName, ActionProbe probe)
    {
        switch (actionName)
        {
            case "move_north":
                Assert.Equal([ExplorationDirection.North], probe.Moves);
                break;
            case "move_east":
                Assert.Equal([ExplorationDirection.East], probe.Moves);
                break;
            case "move_south":
                Assert.Equal([ExplorationDirection.South], probe.Moves);
                break;
            case "move_west":
                Assert.Equal([ExplorationDirection.West], probe.Moves);
                break;
            case "select_context":
                Assert.Equal(1, probe.SelectContext);
                break;
            case "request_event":
                Assert.Equal(1, probe.RequestEvent);
                break;
            case "acknowledge_event":
                Assert.Equal(1, probe.AcknowledgeEvent);
                break;
            case "request_transition":
                Assert.Equal(1, probe.RequestLocalTransition);
                break;
            case "acknowledge_transition":
                Assert.Equal(1, probe.AcknowledgeLocalTransition);
                break;
            case "turn_north":
                Assert.Equal([SemanticFacing.North], probe.Turns);
                break;
            case "turn_east":
                Assert.Equal([SemanticFacing.East], probe.Turns);
                break;
            case "turn_south":
                Assert.Equal([SemanticFacing.South], probe.Turns);
                break;
            case "turn_west":
                Assert.Equal([SemanticFacing.West], probe.Turns);
                break;
            case "request_entity_interaction":
                Assert.Equal(1, probe.RequestEntityInteraction);
                break;
            case "acknowledge_entity_interaction":
                Assert.Equal(1, probe.AcknowledgeEntityInteraction);
                break;
            case "advance_dialogue":
                Assert.Equal(1, probe.AdvanceDialogue);
                break;
            case "request_field_search":
                Assert.Equal(1, probe.RequestFieldSearch);
                break;
            case "acknowledge_field_search":
                Assert.Equal(1, probe.AcknowledgeFieldSearch);
                break;
            case "request_item_acquisition":
                Assert.Equal(1, probe.RequestItemAcquisition);
                break;
            case "acknowledge_item_acquisition":
                Assert.Equal(1, probe.AcknowledgeItemAcquisition);
                break;
            case "request_outbound_transition":
                Assert.Equal(1, probe.RequestOutboundTransition);
                break;
            case "acknowledge_outbound_transition":
                Assert.Equal(1, probe.AcknowledgeOutboundTransition);
                break;
            case "public_synthetic_battle_request":
                Assert.Equal(1, probe.RequestPublicSyntheticBattle);
                break;
            case "public_synthetic_battle_acknowledge_entry":
                Assert.Equal(1, probe.AcknowledgePublicSyntheticBattle);
                break;
            case "public_synthetic_battle_cursor_north":
                Assert.Equal([TacticalDirection.North], probe.BattleCursorMoves);
                break;
            case "public_synthetic_battle_cursor_east":
                Assert.Equal([TacticalDirection.East], probe.BattleCursorMoves);
                break;
            case "public_synthetic_battle_cursor_south":
                Assert.Equal([TacticalDirection.South], probe.BattleCursorMoves);
                break;
            case "public_synthetic_battle_cursor_west":
                Assert.Equal([TacticalDirection.West], probe.BattleCursorMoves);
                break;
            case "public_synthetic_battle_confirm":
                Assert.Equal(1, probe.ConfirmPublicSyntheticBattle);
                break;
            case "public_synthetic_battle_cancel":
                Assert.Equal(1, probe.CancelPublicSyntheticBattle);
                break;
            case "public_synthetic_battle_acknowledge_completion":
                Assert.Equal(1, probe.AcknowledgePublicSyntheticBattleCompletion);
                break;
            default:
                throw new Xunit.Sdk.XunitException($"Unexpected binding {actionName}");
        }
    }

    private sealed class ActionProbe
    {
        internal int TotalCalls { get; private set; }

        internal List<ExplorationDirection> Moves { get; } = [];

        internal List<SemanticFacing> Turns { get; } = [];

        internal List<TacticalDirection> BattleCursorMoves { get; } = [];

        internal int SelectContext;
        internal int RequestEvent;
        internal int AcknowledgeEvent;
        internal int RequestLocalTransition;
        internal int AcknowledgeLocalTransition;
        internal int RequestEntityInteraction;
        internal int AcknowledgeEntityInteraction;
        internal int AdvanceDialogue;
        internal int RequestFieldSearch;
        internal int AcknowledgeFieldSearch;
        internal int RequestItemAcquisition;
        internal int AcknowledgeItemAcquisition;
        internal int RequestOutboundTransition;
        internal int AcknowledgeOutboundTransition;
        internal int RequestPublicSyntheticBattle;
        internal int AcknowledgePublicSyntheticBattle;
        internal int ConfirmPublicSyntheticBattle;
        internal int CancelPublicSyntheticBattle;
        internal int AcknowledgePublicSyntheticBattleCompletion;

        internal void Record(ref int counter)
        {
            counter++;
            TotalCalls++;
        }

        internal void RecordMove(ExplorationDirection direction)
        {
            Moves.Add(direction);
            TotalCalls++;
        }

        internal void RecordTurn(SemanticFacing facing)
        {
            Turns.Add(facing);
            TotalCalls++;
        }

        internal void RecordBattleCursor(TacticalDirection direction)
        {
            BattleCursorMoves.Add(direction);
            TotalCalls++;
        }
    }
}
