using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;

namespace Sf2.Remake.GodotAdapter.Audio;

internal sealed partial class SessionAudio
{
    private bool _entrySuppressed;
    private bool _entrySilenced;
    private bool _entryMusicConsumed;

    internal bool FiniteMusicReady() => FiniteMusicFinished() || _entryMusicConsumed;

    private void BeginEntrySuppression()
    {
        if (_entrySuppressed) return;
        _entrySuppressed = true;
        _entrySilenced = _fading || !_music.Playing;
        Stop(_music, MusicCue);
        _fading = false; _fadeProgress = 0;
        // Existing short effect/speech voices retain their own natural tails.
    }

    private void SuppressEntryCue(string resource, bool remember)
    {
        if (!_assets.TryGetValue(resource, out var audio)) throw new InvalidOperationException("audio-content-unavailable");
        if (audio.Command >= 65)
        {
            if (audio.TimerB != TimerB && Select(audio.Command) != resource)
                throw new InvalidOperationException("sfx-timer-context-unavailable");
            Record("battle-entry-skip", _music, resource);
            return;
        }
        if (remember && MusicCue == resource) return;
        MusicCue = resource; MusicGeneration = null; MusicFinished = false;
        TimerB = audio.TimerB; _entrySilenced = false; _entryMusicConsumed = false;
        if (remember)
        {
            _history.Insert(0, resource);
            if (_history.Count > 10) _history.RemoveAt(10);
        }
        Record("battle-entry-skip", _music, resource);
    }

    internal void SkipEntryFiniteMusic()
    {
        _ = FiniteMusicFinished(); // Retain finite-resource admission, not its elapsed playback.
        _entrySilenced = true; _entryMusicConsumed = true;
        Record("battle-entry-skip-consumed", _music, MusicCue);
    }

    internal void SkipEntryFade()
    {
        BeginEntrySuppression();
        _entrySilenced = true;
        Record("battle-entry-skip", _music, MusicCue, 253);
    }

    internal void ReconcileEntry(SessionSnapshot current)
    {
        if (!_entrySuppressed || current.Story.EntrySkip is not null || Error is not null) return;
        try
        {
            // Rebuild from the authoritative known stack. Unbound authored audio retained every
            // suppressed request above, so its ordinary adapter stack is also current.
            if (current.Story.Music is { HistoryBound: true } music)
            {
                MusicCue = music.Cue; MusicGeneration = music.Generation;
                _history.Clear(); _history.Add(music.Cue); _history.AddRange(music.Previous);
                if (music is { EndStep: not null, DeliverySkipped: true })
                { _entrySilenced = true; _entryMusicConsumed = true; }
            }
            _entrySuppressed = false;
            if (!_entrySilenced && MusicCue is { } cue)
            {
                long? generation = MusicGeneration;
                Play(cue, remember: false);
                MusicGeneration = generation;
                Record("battle-entry-reconciled", _music, cue);
            }
        }
        catch (InvalidOperationException error) { Error = error.Message; }
    }
}
