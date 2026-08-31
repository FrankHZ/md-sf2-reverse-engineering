using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
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
            () => probe.Record(ref probe.AcknowledgeOutboundTransition));

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
            default:
                throw new Xunit.Sdk.XunitException($"Unexpected binding {actionName}");
        }
    }

    private sealed class ActionProbe
    {
        internal int TotalCalls { get; private set; }

        internal List<ExplorationDirection> Moves { get; } = [];

        internal List<SemanticFacing> Turns { get; } = [];

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
    }
}
