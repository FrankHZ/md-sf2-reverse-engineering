using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;

namespace Sf2.Remake.GodotAdapter.Exploration;

public sealed partial class ExplorationSessionView
{
    // One explicit reading intent, never a queue of confirmations or a script skip.
    private (WaitToken Token, int DisplayStart, ulong Frame)? _readingConfirmation;

    private void ConfirmReading()
    {
        if (_readingConfirmation is not null) return;
        bool revealed = TextRevealed;
        if (!revealed) RevealText();
        if (_dialogueView.Panel.MoreBelow)
        {
            // First expose this page; subsequent fresh edges read the remaining pages.
            if (revealed) _dialogueView.Panel.ReadMore();
            return;
        }
        var wait = _session!.Current.Story.Wait!;
        if (wait is DialogueWait)
        {
            Send(new Acknowledge(wait.Token));
            return;
        }
        _readingConfirmation = (wait.Token, (wait as FieldTextWait)?.DisplayStart ?? 0, Engine.GetProcessFrames());
    }

    private bool ReadingHostReady => !_handedOff && !_battleHandoffPending &&
        CanProcess() && IsVisibleInTree() && GetWindow().HasFocus() &&
        PresentationFailure is null && _dialogueView.Panel.IsVisibleInTree();

    private bool ContinueReadingConfirmation()
    {
        if (_readingConfirmation is not { } reading) return false;
        bool SameSpan() => _session!.Current.Story.Wait?.Token == reading.Token &&
            ((_session.Current.Story.Wait as FieldTextWait)?.DisplayStart ?? 0) == reading.DisplayStart;
        // W1 has no display-start field; its token owns the entire current span.
        bool SameReading() => _session!.Current.Story.Wait is W1TextWait w1 ? w1.Token == reading.Token : SameSpan();
        if (!SameReading()) { _readingConfirmation = null; return false; }
        if (!ReadingHostReady) { _readingConfirmation = null; return true; }
        if (Engine.GetProcessFrames() <= reading.Frame) return true;
        // Reuse real neutral services, preserving RNG/entities/window order. Yield at 600
        // opportunities; explicit {D1} time remains on the ordinary timed path below.
        for (int work = 0; work < 600 && _session!.Current.Story.Wait is FieldTextWait { LogicalDone: false } field; work++)
        {
            if (field.Phase == FieldTextPhase.TextPause) return false;
            Send(new AdvanceSimulation(field.Token));
            if (!ReadingHostReady || _readingConfirmation is null) { _readingConfirmation = null; return true; }
            // CLEAR may replace a projection under the same token. It needs new reading intent.
            if (!SameReading()) { _readingConfirmation = null; return true; }
        }
        if (_session!.Current.Story.Wait is FieldTextWait { LogicalDone: false }) return true;
        if (!TextRevealed || _dialogueView.Panel.MoreBelow) { _readingConfirmation = null; return true; }
        if (_session.Current.Story.Wait is W1TextWait { Revealed: false } or FieldTextWait { Revealed: false })
        {
            Send(new CompleteTextReveal(reading.Token));
            if (!ReadingHostReady || _readingConfirmation is null)
            { _readingConfirmation = null; return true; }
        }
        if (SameReading() && _session.Current.CanWaitForText)
        {
            Send(new Acknowledge(reading.Token));
        }
        _readingConfirmation = null;
        return true;
    }
}
