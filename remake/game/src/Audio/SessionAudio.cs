using Godot;
using Sf2.Remake.GodotAdapter.Observation;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter.Audio;

// One owner for the session, independent of exploration/battle view lifetimes.
internal sealed partial class SessionAudio : IDisposable
{
    private readonly IReadOnlyDictionary<string, ExplorationAudio> _assets;
    private readonly IReadOnlyDictionary<MapId, ExplorationMapVisual> _maps;
    private readonly ExplorationDefinition _definition;
    private (MapId Map, int Area, bool Battle)? _context;
    private readonly AudioStreamPlayer _music;
    private readonly Node _owner;
    private readonly ObservationCapture? _capture;
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
    private bool _fading;
    private double _fadeProgress;
    internal long? MusicGeneration { get; private set; }
    internal SessionAudio(Node owner, ExplorationDefinition definition)
    {
        _definition = definition;
        _assets = definition.Visuals?.Audio ?? new Dictionary<string, ExplorationAudio>();
        _maps = definition.Visuals?.Maps ?? new Dictionary<MapId, ExplorationMapVisual>();
        _music = new AudioStreamPlayer { Name = "SessionMusic" };
        _owner = owner;
        _capture = ObservationCapture.Find(owner);
        owner.AddChild(_music);
        _musicFinished = () => { if (_music.Playing || _entrySuppressed || _entryMusicConsumed) return; MusicFinished = true; Completions++; Record("finished", _music, MusicCue!); };
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

    internal object ObserveState() => new
    {
        error = Error, sequence = _sequence, revision = _revision, waitToken = _waitToken,
        musicCue = MusicCue, musicPlaying = _music.Playing, musicFinished = MusicFinished, musicGeneration = MusicGeneration,
        musicDeliverySkipped = _entryMusicConsumed, musicPosition = MusicPosition, timerB = TimerB, soundCue = _soundCue,
        soundPlaying = _sounds.Any(voice => voice.Player.Playing),
    };

    internal object ObservePlayback() => new
    {
        error = Error, sequence = _sequence, revision = _revision, waitToken = _waitToken,
        musicCue = MusicCue, musicPlaying = _music.Playing, musicFinished = MusicFinished, musicGeneration = MusicGeneration,
        musicDeliverySkipped = _entryMusicConsumed, musicPosition = MusicPosition, timerB = TimerB,
        musicVolumeDb = _music.VolumeDb, fading = _fading, fadeProgress = _fadeProgress,
        soundCue = _soundCue, soundPlaying = _sounds.Any(voice => voice.Player.Playing),
        soundPosition = _sounds.LastOrDefault()?.Player.GetPlaybackPosition() ?? 0,
        sounds = _sounds.Select(voice => new
        {
            cue = voice.Cue, command = _assets[voice.Cue].Command, slots = voice.Slots.ToString(), startSequence = voice.StartSequence,
            playing = voice.Player.Playing, position = voice.Player.GetPlaybackPosition(), volumeDb = voice.Player.VolumeDb,
        }).ToArray(),
        receipts = _receipts.ToArray(),
    };

    private void Record(string operation, AudioStreamPlayer player, string? cue, int? command = null)
    {
        var asset = cue is null ? null : _assets[cue];
        _receipts.Add(new(++_sequence, operation, cue, command ?? asset!.Command, asset?.TimerB, asset?.PcmSha256,
            asset?.SampleRate, asset?.Channels, asset?.SampleFrames, asset?.LoopBegin, asset?.LoopEnd,
            player.Playing, player.GetPlaybackPosition(), Time.GetTicksUsec(), _revision, _waitToken,
            _sounds.FirstOrDefault(voice => voice.Player == player)?.RequestedTimerB));
        _capture?.AudioReceipt(_receipts[^1]);
        // Existing inspectors poll live state. Sequence makes a missed bounded window explicit.
        if (_receipts.Count > 64) _receipts.RemoveAt(0);
    }

    private void Stop(AudioStreamPlayer player, string? cue)
    {
        if (!player.Playing) return;
        player.Stop(); Stops++;
        if (cue is not null) Record("stopped", player, cue);
    }

    internal void Restore(SessionSnapshot snapshot)
    {
        try
        {
            var world = snapshot.Exploration!;
            var music = snapshot.Story.Music ?? throw new InvalidOperationException("restore-music-required");
            // Prepare the new playback and its logical history before replacing the old host session.
            Play(music.Cue, remember: false);
            _history.Clear(); _history.Add(music.Cue); _history.AddRange(music.Previous);
            _context = (world.Map, music.Area, false);
            BindMusic(music.Generation, music.Cue);
        }
        catch (Exception error) when (error is InvalidOperationException or ArgumentException)
        { Error = "restore-audio-unavailable"; }
    }

    internal Sf2.Remake.Application.Runtime.Persistence.ObserveFieldMusic? ObserveFieldHistory()
    {
        if (Error is not null || _context is not { Battle: false } context || MusicCue is null ||
            _history.Count == 0 || _history[0] != MusicCue) return null;
        return new(context.Map, context.Area, new(MusicCue, Array.AsReadOnly(_history.Skip(1).Take(9).ToArray())));
    }

    internal void Observe(SessionResult result, bool entrySkipDelivery = false)
    {
        _capture?.BindSession(result.Snapshot.SessionId);
        _revision = result.Snapshot.Revision;
        _waitToken = result.Snapshot.BattleScene?.Token.Value ?? result.Snapshot.Story.Wait?.Token.Value;
        if (Error is not null || result.Failure is not null || _assets.Count == 0) return;
        try
        {
            var current = result.Snapshot;
            if (entrySkipDelivery || current.Story.EntrySkip is not null) BeginEntrySuppression();
            foreach (var observation in result.Observations.Where(row => row.Sequence > _observedSequence))
            {
                if (_entrySuppressed) continue;
                if (observation.Kind == "door-opened") Play(92);
                else if (observation.Kind == "warp-started") Play(89);
            }
            _observedSequence = current.ObservationSequence;
            if (current.Exploration is { } world)
            {
                int area = world.Definition.Traversal.SelectActiveArea(world.PlayerEntity.Position)!.OneBasedRecordOrdinal - 1;
                var context = (world.Map, area, current.Story.SelectsBattleMusic);
                if (_context == context) return;
                if (!_maps.TryGetValue(world.Map, out var visual) || area >= visual.Music.Count)
                    throw new InvalidOperationException("map-music-unavailable");
                var selection = visual.Music[area];
                Play(context.Item3 ? selection.Battle : selection.Field);
                _context = context;
            }
            else if (current.Active is ActiveBattle battle)
            {
                var bindings = _definition.Maps.Values.Where(map =>
                    map.Battles.Any(route => route.Encounter == battle.Battle.Definition.Encounter)).ToArray();
                if (bindings.Length != 1 || !_maps.TryGetValue(bindings[0].Map, out var visual))
                    throw new InvalidOperationException("battle-music-map-binding");
                var map = bindings[0].Map;
                // A before-script may retain another field map. Its audio context is not
                // the next encounter's map/area. A new multi-area binding needs an explicit
                // area owner; the admitted second encounter has one source area.
                int area = _context is { } previous && previous.Map == map ? previous.Area :
                    visual.Music.Count == 1 ? 0 : throw new InvalidOperationException("battle-music-area-binding");
                var context = (map, area, true);
                if (_context == context) return;
                Play(visual.Music[area].Battle);
                _context = context;
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
        if (_entrySuppressed) { SuppressEntryCue(resource, remember); return; }
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
        if (music)
        {
            if (_fading) StopSharedEffects();
            _fading = false; _fadeProgress = 0;
            Stop(player, MusicCue);
        }
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
            MusicGeneration = null;
            MusicCue = resource; MusicFinished = false; _entryMusicConsumed = false; TimerB = audio.TimerB;
            if (remember)
            {
                _history.Insert(0, resource);
                if (_history.Count > 10) _history.RemoveAt(10);
            }
        }
        else _soundCue = resource;
        player.VolumeDb = _fading && voice?.Slots == Slots.PsgNoise ? (float)(-60 * _fadeProgress) : 0;
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

    internal void BindMusic(long generation, string cue)
    {
        if (MusicCue != cue || MusicGeneration is { } old && old != generation)
            throw new InvalidOperationException("music-generation-mismatch");
        MusicGeneration = generation;
    }

    internal bool FiniteMusicFinished(long generation, string cue)
    {
        if (MusicGeneration != generation || MusicCue != cue)
            throw new InvalidOperationException("music-generation-replaced");
        return FiniteMusicFinished();
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
        if (!_fading) return;
        _fadeProgress = Math.Max(_fadeProgress, Math.Clamp(progress, 0, 1));
        float volume = (float)(-60 * _fadeProgress);
        _music.VolumeDb = Math.Min(_music.VolumeDb, volume);
        foreach (var voice in _sounds.Where(voice => voice.Slots == Slots.PsgNoise && voice.Player.Playing))
            voice.Player.VolumeDb = Math.Min(voice.Player.VolumeDb, volume);
        if (_fadeProgress < 1) return;
        Stop(_music, MusicCue);
        StopSharedEffects();
        _fading = false;
    }

    private void StopSharedEffects()
    {
        // The admitted shared PSG effects end with StopMusic; extra type-2 voices survive.
        // Already-ended players retain their queued natural Finished callback.
        foreach (var voice in _sounds.Where(voice => voice.Player.Playing &&
            (voice.Slots & (Slots.PsgTone3 | Slots.PsgNoise)) != 0).ToArray())
        {
            Stop(voice.Player, voice.Cue);
            Release(voice);
        }
    }

    internal void BeginBattleScene(int music)
    {
        _battlefieldMusic = MusicCue;
        _sceneMusic = music;
        BeginSceneEnd();
    }
    internal void BeginSceneEnd()
    {
        if (_disposed) throw new InvalidOperationException("audio-owner-disposed");
        if (!_fading) _fadeProgress = 0;
        _fading = true;
        Record("fade-command", _music, _music.Playing ? MusicCue : null, 253);
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
        _fading = false;
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
        65 or 109 or 111 => Slots.Ym5,
        66 or 67 or 70 or 72 or 73 or 74 or 79 or 102 => Slots.PsgTone3,
        77 => Slots.Ym4,
        81 or 83 or 116 => Slots.Ym6,
        89 or 110 => Slots.PsgNoise,
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

internal sealed record AudioPlaybackReceipt(long Sequence, string Operation, string? Cue, int Command,
    int? TimerB, string? PcmSha256, int? SampleRate, int? Channels, int? SampleFrames, int? LoopBegin,
    int? LoopEnd, bool Playing, double PlaybackPosition, ulong Microseconds, long Revision, long? WaitToken, int? RequestedTimerB);
