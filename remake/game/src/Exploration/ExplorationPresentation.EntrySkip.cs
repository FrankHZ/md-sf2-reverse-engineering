using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;

namespace Sf2.Remake.GodotAdapter.Exploration;

internal sealed partial class ExplorationPresentation
{
    private void SkipDelivery(SessionSnapshot current, PresentationWait wait, Vector2 target)
    {
        // Keep resource/binding admission and final projection, omit only the timeline.
        if (current.Exploration is null && wait.Cue.Kind is PresentationCueKind.CameraWait or PresentationCueKind.Gesture or PresentationCueKind.EntityEffect)
            throw new InvalidOperationException("presentation-map-unavailable");
        switch (wait.Cue.Kind)
        {
            case PresentationCueKind.Gesture:
                if (wait.Cue.Resource is not ("nod" or "shiver" or "shake-head") || wait.Cue.Entity is null)
                    throw new InvalidOperationException("gesture-binding");
                _ = Sprite(current.Exploration!.Entities[wait.Cue.Entity.Value], wait.Cue.Resource == "nod");
                break;
            case PresentationCueKind.EntityEffect:
                if (wait.Cue.Resource is not ("mosaic-in" or "mosaic-out" or "slide-in" or "flash-white") || wait.Cue.Entity is null)
                    throw new InvalidOperationException("effect-binding");
                _ = Sprite(current.Exploration!.Entities[wait.Cue.Entity.Value], false);
                break;
            case PresentationCueKind.CameraWait: _camera = target; break;
            case PresentationCueKind.FlashWhite:
                if (wait.Cue.Resource != "white-four-pulses" || wait.Cue.DurationTicks is not (> 0 and <= 240))
                    throw new InvalidOperationException("flash-pulse-binding");
                if (_cue == wait.Token) _white.Color = _flashRestore;
                _flashPulse = -1;
                break;
            case PresentationCueKind.FadeIn:
            case PresentationCueKind.FadeOut:
                if (wait.Cue.Resource is not ("black" or "white")) throw new InvalidOperationException("fade-binding");
                bool fadeOut = wait.Cue.Kind == PresentationCueKind.FadeOut;
                _white.Color = new Color(1, 1, 1, wait.Cue.Resource == "white" && fadeOut && !_reducedFlash ? 1 : 0);
                _owner.Modulate = wait.Cue.Resource == "black" && fadeOut ? Colors.Black : Colors.White;
                break;
            case PresentationCueKind.Sound:
                _audio.Play(wait.Cue.Resource ?? throw new InvalidOperationException("sound-binding"));
                if (current.Story.Music is { EndStep: not null } music && music.Cue == wait.Cue.Resource)
                    _audio.BindMusic(music.Generation, music.Cue);
                break;
            case PresentationCueKind.PreviousMusic:
                _audio.PlayPrevious();
                if (current.Story.TextSettings is not null && current.Story.Music is { } previous && _audio.MusicCue != previous.Cue)
                    throw new InvalidOperationException("music-previous-binding");
                break;
            case PresentationCueKind.SoundWait: _audio.SkipEntryFiniteMusic(); break;
            case PresentationCueKind.SoundFade: _audio.SkipEntryFade(); break;
            default: throw new InvalidOperationException("presentation-binding-" + wait.Cue.Kind);
        }
        _cue = wait.Token; _cueAge = 0; _cueResource = null;
        _gesture = null; _mosaic = null; _storyEffect = null; _shivering = _nodding = false;
        ActiveCue = null; CompletedCueToken = wait.Token.Value; CompletedCueKind = "Skipped:" + wait.Cue.Kind;
    }
}
