using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter.Audio;

// One owner for the session, independent of exploration/battle view lifetimes.
internal sealed class SessionAudio : IDisposable
{
    private readonly IReadOnlyDictionary<string, ExplorationAudio> _assets;
    private readonly IReadOnlyDictionary<MapId, ExplorationMapVisual> _maps;
    private (MapId Map, int Area, bool Battle)? _context;
    private readonly AudioStreamPlayer _music;
    private readonly Node _owner;
    private readonly List<SoundVoice> _sounds = [];
    private readonly Action _musicFinished;
    private bool _disposed;
    private readonly List<string> _history = [];
    private readonly List<AudioPlaybackReceipt> _receipts = [];
    private string? _soundCue;
    private long _sequence;
    private long _revision;
    private long _observedSequence;
    private long? _waitToken;
    private string? _battlefieldMusic;
    private int _sceneMusic;
    internal SessionAudio(Node owner, ExplorationDefinition definition)
    {
        _assets = definition.Visuals?.Audio ?? new Dictionary<string, ExplorationAudio>();
        _maps = definition.Visuals?.Maps ?? new Dictionary<MapId, ExplorationMapVisual>();
        _music = new AudioStreamPlayer { Name = "SessionMusic" };
        _owner = owner;
        owner.AddChild(_music);
        _musicFinished = () => { MusicFinished = true; Completions++; Record("finished", _music, MusicCue!); };
        _music.Finished += _musicFinished;
        if (definition.Provenance is not null && _assets.Count == 0) Error = "private-audio-required";
    }

    internal string? Error { get; private set; }
    internal int Starts { get; private set; }
    internal int Stops { get; private set; }
    internal int Completions { get; private set; }
    internal string? MusicCue { get; private set; }
    internal bool MusicFinished { get; private set; }
    internal bool MusicPlaying => _music.Playing;
    internal double MusicPosition => _music.GetPlaybackPosition();
    internal int? TimerB { get; private set; }

    internal object ObservePlayback() => new
    {
        error = Error, sequence = _sequence, revision = _revision, waitToken = _waitToken,
        musicCue = MusicCue, musicPlaying = _music.Playing, musicFinished = MusicFinished,
        musicPosition = MusicPosition, timerB = TimerB,
        soundCue = _soundCue, soundPlaying = _sounds.Any(voice => voice.Player.Playing),
        soundPosition = _sounds.LastOrDefault()?.Player.GetPlaybackPosition() ?? 0,
        sounds = _sounds.Select(voice => new
        {
            cue = voice.Cue, command = _assets[voice.Cue].Command, slots = voice.Slots.ToString(), startSequence = voice.StartSequence,
            playing = voice.Player.Playing, position = voice.Player.GetPlaybackPosition(),
        }).ToArray(),
        receipts = _receipts.ToArray(),
    };

    private void Record(string operation, AudioStreamPlayer player, string cue, int? command = null)
    {
        var asset = _assets[cue];
        _receipts.Add(new(++_sequence, operation, cue, command ?? asset.Command, asset.TimerB, asset.PcmSha256,
            asset.SampleRate, asset.Channels, asset.SampleFrames, asset.LoopBegin, asset.LoopEnd,
            player.Playing, player.GetPlaybackPosition(), Time.GetTicksUsec(), _revision, _waitToken,
            _sounds.FirstOrDefault(voice => voice.Player == player)?.RequestedTimerB));
        // Existing inspectors poll live state. Sequence makes a missed bounded window explicit.
        if (_receipts.Count > 64) _receipts.RemoveAt(0);
    }

    private void Stop(AudioStreamPlayer player, string? cue)
    {
        if (!player.Playing) return;
        player.Stop(); Stops++;
        if (cue is not null) Record("stopped", player, cue);
    }

    internal void Observe(SessionResult result)
    {
        _revision = result.Snapshot.Revision;
        _waitToken = result.Snapshot.BattleScene?.Token.Value ?? result.Snapshot.Story.Wait?.Token.Value;
        if (Error is not null || result.Failure is not null || _assets.Count == 0) return;
        try
        {
            var current = result.Snapshot;
            foreach (var observation in result.Observations.Where(row => row.Sequence > _observedSequence))
            {
                if (observation.Kind == "door-opened") Play(92);
                else if (observation.Kind == "warp-started") Play(89);
            }
            _observedSequence = current.ObservationSequence;
            if (current.Exploration is { } world)
            {
                int area = world.Definition.Traversal.SelectActiveArea(world.PlayerEntity.Position)!.OneBasedRecordOrdinal - 1;
                var context = (world.Map, area, current.Story.EnteringBattle is not null);
                if (_context == context) return;
                if (!_maps.TryGetValue(world.Map, out var visual) || area >= visual.Music.Count)
                    throw new InvalidOperationException("map-music-unavailable");
                var selection = visual.Music[area];
                Play(context.Item3 ? selection.Battle : selection.Field);
                _context = context;
            }
            else if (_context is { Battle: false } field)
            {
                Play(_maps[field.Map].Music[field.Area].Battle);
                _context = (field.Map, field.Area, true);
            }
        }
        catch (InvalidOperationException error) { Error = error.Message; }
    }

    private string Select(int command)
    {
        var candidates = _assets.Where(pair => pair.Value.Command == command).ToArray();
        if (command < 65)
            return candidates.FirstOrDefault().Key ?? throw new InvalidOperationException("audio-command-unavailable");
        var exact = candidates.Where(pair => pair.Value.TimerB == TimerB).ToArray();
        if (exact.Length == 1) return exact[0].Key;
        if (exact.Length > 1) throw new InvalidOperationException("audio-command-ambiguous");
        // Modern finite PCM reuse: never relabel the recording's hardware timer,
        // infer a pitch, choose by enumeration order, or borrow another command.
        var finite = candidates.Where(pair => pair.Value.LoopBegin is null).ToArray();
        return finite.Length switch
        {
            1 => finite[0].Key,
            0 => throw new InvalidOperationException("audio-command-unavailable"),
            _ => throw new InvalidOperationException("audio-command-ambiguous"),
        };
    }

    internal void Play(int command)
    {
        if (command != 0) Play(Select(command));
    }

    internal void PlayEffect(int command)
    {
        if (Error is not null || _assets.Count == 0 || command == 0) return;
        try { Play(command); }
        catch (InvalidOperationException error) { Error = error.Message; }
    }

    internal void Play(string resource, bool remember = true)
    {
        if (_disposed) throw new InvalidOperationException("audio-owner-disposed");
        if (!_assets.TryGetValue(resource, out var audio)) throw new InvalidOperationException("audio-content-unavailable");
        bool music = audio.Command < 65;
        // The source suppresses identical music requests even when the finite track already ended.
        if (music && remember && MusicCue == resource) return;
        if (!music && audio.TimerB != TimerB && Select(audio.Command) != resource)
            throw new InvalidOperationException("sfx-timer-context-unavailable");
        SoundVoice? voice = null;
        if (!music)
        {
            var slots = SoundSlots(audio.Command);
            // Whole-clip approximation: replace covered groups, but let a partially
            // overlapping clip finish alongside the new cue. Cosmetic tails never block input.
            foreach (var active in _sounds.Where(active => active.Player.Playing &&
                (active.Slots & slots) == active.Slots).ToArray())
            {
                // Leave already-ended voices alive for their actual queued Finished callback.
                Stop(active.Player, active.Cue);
                Release(active);
            }
            var effect = new AudioStreamPlayer { Name = "SessionSound" };
            _owner.AddChild(effect);
            voice = new SoundVoice(effect, resource, slots, TimerB);
            var startedVoice = voice;
            voice.Finished = () =>
            {
                Completions++; Record("finished", effect, resource);
                Release(startedVoice);
            };
            effect.Finished += voice.Finished;
            _sounds.Add(voice);
        }
        var player = voice?.Player ?? _music;
        if (music) Stop(player, MusicCue);
        var previous = player.Stream; player.Stream = null; previous?.Dispose();
        player.Stream = new AudioStreamWav
        {
            Format = AudioStreamWav.FormatEnum.Format16Bits, MixRate = audio.SampleRate,
            Stereo = audio.Channels == 2, Data = audio.CopyPcm(),
            LoopMode = audio.LoopBegin is null ? AudioStreamWav.LoopModeEnum.Disabled : AudioStreamWav.LoopModeEnum.Forward,
            LoopBegin = audio.LoopBegin ?? 0, LoopEnd = audio.LoopEnd ?? 0,
        };
        if (music)
        {
            MusicCue = resource; MusicFinished = false; TimerB = audio.TimerB;
            if (remember)
            {
                _history.Insert(0, resource);
                if (_history.Count > 10) _history.RemoveAt(10);
            }
        }
        else _soundCue = resource;
        player.VolumeDb = 0;
        player.Play();
        if (!player.Playing) throw new InvalidOperationException("audio-stream-did-not-start");
        Starts++;
        Record("started", player, resource);
        if (voice is not null) voice.StartSequence = _sequence;
    }

    internal bool FiniteMusicFinished()
    {
        if (MusicCue is null || _assets[MusicCue].LoopBegin is not null)
            throw new InvalidOperationException("finite-music-required");
        return MusicFinished;
    }

    internal void PlayPrevious()
    {
        if (_history.Count < 2) throw new InvalidOperationException("previous-music-unavailable");
        _history.RemoveAt(0);
        // Original $FB submits the prior command; no saved sample offset is restored.
        Play(_history[0], remember: false);
    }

    internal void FadeOut(double progress)
    {
        _music.VolumeDb = (float)(-60 * Math.Clamp(progress, 0, 1));
        if (progress >= 1) Stop(_music, MusicCue);
    }

    internal void BeginBattleScene(int music)
    {
        _battlefieldMusic = MusicCue;
        _sceneMusic = music;
        BeginSceneEnd();
    }
    internal void BeginSceneEnd()
    {
        if (MusicCue is { } cue) Record("fade-command", _music, cue, 253);
    }
    internal void StartBattleSceneMusic() { if (_assets.Count > 0) Play(_sceneMusic); }
    internal void RestoreBattlefieldMusic()
    {
        if (_battlefieldMusic is { } cue) Play(cue);
        _battlefieldMusic = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _music.Finished -= _musicFinished;
        foreach (var voice in _sounds.ToArray()) Release(voice);
        ReleasePlayer(_music);
    }

    private void Release(SoundVoice voice)
    {
        voice.Player.Finished -= voice.Finished;
        _sounds.Remove(voice);
        ReleasePlayer(voice.Player);
    }

    private static void ReleasePlayer(AudioStreamPlayer player)
    {
        player.Stop();
        var stream = player.Stream; player.Stream = null; stream?.Dispose();
        player.QueueFree();
    }

    [Flags]
    private enum Slots { Ym4 = 1, Ym5 = 2, Ym6 = 4, PsgTone3 = 8, PsgNoise = 16 }

    // Pinned SF2DISASM c834c652, sounddriver.asm Load_SFX and the H2 sound inventory's
    // activeSlots for the admitted commands. These are cheap whole-clip replacement groups,
    // not hardware output masks or a claim of acoustic independence.
    private static Slots SoundSlots(int command) => command switch
    {
        65 => Slots.Ym5,
        66 or 67 or 70 or 72 or 73 or 74 or 79 or 102 => Slots.PsgTone3,
        77 => Slots.Ym4,
        81 or 83 or 116 => Slots.Ym6,
        89 => Slots.PsgNoise,
        92 or 113 => Slots.Ym4 | Slots.Ym5,
        _ => throw new InvalidOperationException("sfx-slots-unavailable"),
    };

    private sealed class SoundVoice(AudioStreamPlayer player, string cue, Slots slots, int? requestedTimerB)
    {
        internal AudioStreamPlayer Player { get; } = player;
        internal string Cue { get; } = cue;
        internal int? RequestedTimerB { get; } = requestedTimerB;
        internal Slots Slots { get; } = slots;
        internal long StartSequence { get; set; }
        internal Action Finished { get; set; } = null!;
    }
}

internal sealed record AudioPlaybackReceipt(long Sequence, string Operation, string Cue, int Command,
    int TimerB, string PcmSha256, int SampleRate, int Channels, int SampleFrames, int? LoopBegin,
    int? LoopEnd, bool Playing, double PlaybackPosition, ulong Microseconds, long Revision, long? WaitToken, int? RequestedTimerB);
