using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Exploration;

public sealed partial class ExplorationSessionView
{
    private readonly MenuPointer _choicePointer = new();
    private WaitToken? _pointerChoice;
    private bool ChoiceButtonsHeld => _input.GameplayHeld || Godot.Input.IsMouseButtonPressed(MouseButton.Left) ||
        Godot.Input.IsMouseButtonPressed(MouseButton.Right);

    private void RefreshChoicePointer()
    {
        var token = (_session?.Current.Story.Wait as ChoiceWait)?.Token;
        if (_pointerChoice == token) return;
        _pointerChoice = token;
        _choicePointer.Cancel();
        _dialogueView.Panel.Choices.ResetFocus();
    }

    internal void HandlePointer(InputEvent input)
    {
        if (_session is null || _handedOff || _battleHandoffPending || _skipRequiresRelease ||
            !GetWindow().HasFocus() || !CanProcess() || !IsVisibleInTree() || PresentationFailure is not null ||
            _partyStatus.Open || _session.Current.Story.Wait is not ChoiceWait choice)
        { _choicePointer.Suspend(); return; }
        _movementHold.Clear(_input); CancelFieldWait();
        var row = _dialogueView.Panel.Choices;
        bool ready = !_choiceRequiresRelease && TextRevealed && !_dialogueView.Panel.MoreBelow &&
            row.IsVisibleInTree() && (choice.Work is null || _session.Current.CanWaitForChoice);
        var hit = ready ? row.Hit(((InputEventMouse)input).Position) : (null, default(Rect2));
        if (input is InputEventMouseMotion motion)
        {
            if (_choicePointer.Moved(motion) && hit.Item1 is { } hovered)
            {
                bool yes = hovered == "Yes";
                if (choice.Work is { } work && work.Yes != yes)
                    Send(new PollChoice(choice.Token, yes ? ChoiceInput.Left : ChoiceInput.Right));
                else { row.Focus(yes); Present(); }
            }
            return;
        }
        if (input is not InputEventMouseButton button) return;
        if (ready && button.ButtonIndex == MouseButton.Right) hit = ("No", GetViewportRect());
        // Source polling changes revision every logical service. The live token/phase owns this gesture.
        object boundary = (_session.Current.SessionId, choice.Token, choice.Work?.Phase);
        if (_choicePointer.Read(button, hit.Item1, hit.Item2, boundary) is { } answer)
        {
            _choiceRequiresRelease = true;
            Send(new ChooseDialogue(choice.Token, answer == "Yes"));
        }
    }
}
