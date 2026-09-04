using Godot;
using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter;

internal sealed record Map3InputActions(
    Action<ExplorationDirection> Move,
    Action SelectContext,
    Action RequestEvent,
    Action AcknowledgeEvent,
    Action RequestLocalTransition,
    Action AcknowledgeLocalTransition,
    Action<SemanticFacing> Turn,
    Action RequestEntityInteraction,
    Action AcknowledgeEntityInteraction,
    Action AdvanceDialogue,
    Action RequestFieldSearch,
    Action AcknowledgeFieldSearch,
    Action RequestItemAcquisition,
    Action AcknowledgeItemAcquisition,
    Action RequestOutboundTransition,
    Action AcknowledgeOutboundTransition,
    Action RequestPublicSyntheticBattle,
    Action AcknowledgePublicSyntheticBattle,
    Action<TacticalDirection> MovePublicSyntheticBattleCursor,
    Action ConfirmPublicSyntheticBattleSelection,
    Action CancelPublicSyntheticBattleSelection,
    Action AcknowledgePublicSyntheticBattleCompletion);

internal sealed record Map3InputBinding(
    string ActionName,
    Key PhysicalKey,
    Action<Map3InputActions> Dispatch,
    ExplorationDirection? PrivateMovementDirection = null);

internal sealed class Map3InputAdapter
{
    private static readonly IReadOnlyList<Map3InputBinding> BindingList =
        Array.AsReadOnly(
            new Map3InputBinding[]
            {
                new(
                    "move_north",
                    Key.W,
                    static actions => actions.Move(ExplorationDirection.North),
                    ExplorationDirection.North),
                new(
                    "move_east",
                    Key.D,
                    static actions => actions.Move(ExplorationDirection.East),
                    ExplorationDirection.East),
                new(
                    "move_south",
                    Key.S,
                    static actions => actions.Move(ExplorationDirection.South),
                    ExplorationDirection.South),
                new(
                    "move_west",
                    Key.A,
                    static actions => actions.Move(ExplorationDirection.West),
                    ExplorationDirection.West),
                new(
                    "select_context",
                    Key.Enter,
                    static actions => actions.SelectContext()),
                new(
                    "request_event",
                    Key.Z,
                    static actions => actions.RequestEvent()),
                new(
                    "acknowledge_event",
                    Key.X,
                    static actions => actions.AcknowledgeEvent()),
                new(
                    "request_transition",
                    Key.C,
                    static actions => actions.RequestLocalTransition()),
                new(
                    "acknowledge_transition",
                    Key.V,
                    static actions => actions.AcknowledgeLocalTransition()),
                new(
                    "turn_north",
                    Key.Up,
                    static actions => actions.Turn(SemanticFacing.North)),
                new(
                    "turn_east",
                    Key.Right,
                    static actions => actions.Turn(SemanticFacing.East)),
                new(
                    "turn_south",
                    Key.Down,
                    static actions => actions.Turn(SemanticFacing.South)),
                new(
                    "turn_west",
                    Key.Left,
                    static actions => actions.Turn(SemanticFacing.West)),
                new(
                    "request_entity_interaction",
                    Key.F,
                    static actions => actions.RequestEntityInteraction()),
                new(
                    "acknowledge_entity_interaction",
                    Key.G,
                    static actions => actions.AcknowledgeEntityInteraction()),
                new(
                    "advance_dialogue",
                    Key.H,
                    static actions => actions.AdvanceDialogue()),
                new(
                    "request_field_search",
                    Key.Q,
                    static actions => actions.RequestFieldSearch()),
                new(
                    "acknowledge_field_search",
                    Key.E,
                    static actions => actions.AcknowledgeFieldSearch()),
                new(
                    "request_item_acquisition",
                    Key.R,
                    static actions => actions.RequestItemAcquisition()),
                new(
                    "acknowledge_item_acquisition",
                    Key.T,
                    static actions => actions.AcknowledgeItemAcquisition()),
                new(
                    "request_outbound_transition",
                    Key.Y,
                    static actions => actions.RequestOutboundTransition()),
                new(
                    "acknowledge_outbound_transition",
                    Key.U,
                    static actions => actions.AcknowledgeOutboundTransition()),
                new(
                    "public_synthetic_battle_request",
                    Key.B,
                    static actions => actions.RequestPublicSyntheticBattle()),
                new(
                    "public_synthetic_battle_acknowledge_entry",
                    Key.N,
                    static actions => actions.AcknowledgePublicSyntheticBattle()),
                new(
                    "public_synthetic_battle_cursor_north",
                    Key.I,
                    static actions => actions.MovePublicSyntheticBattleCursor(
                        TacticalDirection.North)),
                new(
                    "public_synthetic_battle_cursor_east",
                    Key.L,
                    static actions => actions.MovePublicSyntheticBattleCursor(
                        TacticalDirection.East)),
                new(
                    "public_synthetic_battle_cursor_south",
                    Key.K,
                    static actions => actions.MovePublicSyntheticBattleCursor(
                        TacticalDirection.South)),
                new(
                    "public_synthetic_battle_cursor_west",
                    Key.J,
                    static actions => actions.MovePublicSyntheticBattleCursor(
                        TacticalDirection.West)),
                new(
                    "public_synthetic_battle_confirm",
                    Key.Space,
                    static actions => actions.ConfirmPublicSyntheticBattleSelection()),
                new(
                    "public_synthetic_battle_cancel",
                    Key.Backspace,
                    static actions => actions.CancelPublicSyntheticBattleSelection()),
                new(
                    "public_synthetic_battle_acknowledge_completion",
                    Key.M,
                    static actions => actions.AcknowledgePublicSyntheticBattleCompletion()),
            });

    private readonly Map3InputActions _actions;
    private readonly Func<string, bool> _isActionJustPressed;

    internal Map3InputAdapter(
        Map3InputActions actions,
        Func<string, bool> isActionJustPressed)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ArgumentNullException.ThrowIfNull(isActionJustPressed);
        _actions = actions;
        _isActionJustPressed = isActionJustPressed;
    }

    internal static IReadOnlyList<Map3InputBinding> Bindings => BindingList;

    internal static Map3InputAdapter CreateGodot(Map3InputActions actions) =>
        new(actions, static action => Input.IsActionJustPressed(action));

    internal void EnsureActionsRegistered()
    {
        foreach (Map3InputBinding binding in BindingList)
        {
            RegisterAction(binding.ActionName, binding.PhysicalKey);
        }
    }

    internal void PollPublicSynthetic()
    {
        foreach (Map3InputBinding binding in BindingList)
        {
            if (!_isActionJustPressed(binding.ActionName))
            {
                continue;
            }

            binding.Dispatch(_actions);
            return;
        }
    }

    internal ExplorationDirection? PollPrivateOriginalMapMovement()
    {
        foreach (Map3InputBinding binding in BindingList)
        {
            if (binding.PrivateMovementDirection is not ExplorationDirection direction)
            {
                continue;
            }

            if (_isActionJustPressed(binding.ActionName))
            {
                return direction;
            }
        }

        foreach (string actionName in new[]
        {
            "request_entity_interaction",
            "acknowledge_entity_interaction",
        })
        {
            Map3InputBinding interaction = BindingList.Single(
                binding => string.Equals(
                    binding.ActionName,
                    actionName,
                    StringComparison.Ordinal));
            if (_isActionJustPressed(interaction.ActionName))
            {
                interaction.Dispatch(_actions);
                return null;
            }
        }

        return null;
    }

    private static void RegisterAction(string action, Key physicalKey)
    {
        if (!InputMap.HasAction(action))
        {
            InputMap.AddAction(action);
        }

        if (InputMap.ActionGetEvents(action).OfType<InputEventKey>().Any(
            input => input.PhysicalKeycode == physicalKey))
        {
            return;
        }

        InputMap.ActionAddEvent(
            action,
            new InputEventKey
            {
                PhysicalKeycode = physicalKey,
            });
    }
}
