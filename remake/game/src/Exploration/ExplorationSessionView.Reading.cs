using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;

namespace Sf2.Remake.GodotAdapter.Exploration;

public sealed partial class ExplorationSessionView
{
    // One explicit reading intent, never a queue of confirmations or a script skip.
    private (WaitToken Token, int DisplayStart, ulong Frame, bool Direct)? _readingConfirmation;
    private (WaitToken Token, bool Direct)? _joinMusicConfirmation;

    private bool ConfirmJoinMusic(MusicWait wait, bool direct)
    {
        if (!ReadingHostReady(direct) || _session!.Current.NeedsFieldWindowService || _joinMusicConfirmation is not null) return false;
        if (!RevealText()) _joinMusicConfirmation = (wait.Token, direct);
        return true;
    }

    private bool ContinueJoinMusicConfirmation()
    {
        if (_joinMusicConfirmation is not { } pending) return false;
        var (token, direct) = pending;
        if (!ReadingHostReady(direct) || _session!.Current.Story.Wait is not MusicWait wait || wait.Token != token)
        { _joinMusicConfirmation = null; return false; }

        // Preserve the live helper's entity, view and window service, including its final group.
        if (!wait.LogicalDone) Send(new AdvanceSimulation(token, 600));
        if (_joinMusicConfirmation == pending && ReadingHostReady(direct) &&
            _session.Current.Story.Wait is MusicWait { LogicalDone: true } done && done.Token == token)
            Send(new CompleteJoinMusicWait(token));
        if (_session.Current.Story.Wait?.Token != token || !ReadingHostReady(direct)) _joinMusicConfirmation = null;
        return true;
    }

    private void InterruptDelay(FieldTextWait delay)
    {
        // The held or fresh input belongs to this delay, even if another W2 follows in the same span.
        _readingConfirmation = null;
        Send(new InterruptTextDelay(delay.Token));
    }

    private bool ConfirmReading(bool direct)
    {
        if (_readingConfirmation is not null || _session!.Current.NeedsFieldWindowService) return false;
        bool revealed = TextRevealed;
        if (!revealed) RevealText();
        if (_dialogueView.Panel.MoreBelow)
        {
            // First expose this page; subsequent fresh edges read the remaining pages.
            if (revealed) _dialogueView.Panel.ReadMore();
            return true;
        }
        var wait = _session!.Current.Story.Wait!;
        if (wait is DialogueWait)
        {
            return Send(new Acknowledge(wait.Token)).Failure is null;
        }
        _readingConfirmation = (wait.Token, (wait as FieldTextWait)?.DisplayStart ?? 0, Engine.GetProcessFrames(), direct);
        return true;
    }

    private bool ReadingHostReady(bool direct) => !_handedOff && !_battleHandoffPending &&
        CanProcess() && IsVisibleInTree() && (direct || GetWindow().HasFocus()) &&
        PresentationFailure is null && _dialogueView.Panel.IsVisibleInTree();

    private bool ContinueReadingConfirmation()
    {
        if (_readingConfirmation is not { } reading) return false;
        bool SameSpan() => _session!.Current.Story.Wait?.Token == reading.Token &&
            ((_session.Current.Story.Wait as FieldTextWait)?.DisplayStart ?? 0) == reading.DisplayStart;
        // W1 has no display-start field; its token owns the entire current span.
        bool SameReading() => _session!.Current.Story.Wait is W1TextWait w1 ? w1.Token == reading.Token : SameSpan();
        if (!SameReading()) { _readingConfirmation = null; return false; }
        if (!ReadingHostReady(reading.Direct)) { _readingConfirmation = null; return true; }
        if (Engine.GetProcessFrames() <= reading.Frame) return true;
        // Preserve RNG, entity and window service order. D1 belongs to this span's reading
        // intent, while D3 retains its separate interruption and elapsed-time rules.
        for (int work = 0; work < 600 && _session!.Current.Story.Wait is FieldTextWait { LogicalDone: false } field; work++)
        {
            if (field.InterruptibleDelay || field.AutomaticDelay) return false;
            Send(field.Phase == FieldTextPhase.TextPause
                ? new InterruptTextDelay(field.Token) { FromReadingConfirm = true }
                : new AdvanceSimulation(field.Token));
            if (!ReadingHostReady(reading.Direct) || _readingConfirmation is null) { _readingConfirmation = null; return true; }
            // CLEAR may replace a projection under the same token. It needs new reading intent.
            if (!SameReading()) { _readingConfirmation = null; return true; }
        }
        if (_session!.Current.Story.Wait is FieldTextWait { LogicalDone: false }) return true;
        if (!TextRevealed || _dialogueView.Panel.MoreBelow) { _readingConfirmation = null; return true; }
        if (_session.Current.Story.Wait is W1TextWait { Revealed: false } or FieldTextWait { Revealed: false })
        {
            Send(new CompleteTextReveal(reading.Token));
            if (!ReadingHostReady(reading.Direct) || _readingConfirmation is null)
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
