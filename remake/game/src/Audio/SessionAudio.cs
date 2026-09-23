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
    private readonly AudioStreamPlayer _sound;
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
        _sound = new AudioStreamPlayer { Name = "SessionSound" };
        owner.AddChild(_music); owner.AddChild(_sound);
        _music.Finished += () => { MusicFinished = true; Completions++; Record("finished", _music, MusicCue!); };
        _sound.Finished += () => { Completions++; Record("finished", _sound, _soundCue!); };
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
        soundCue = _soundCue, soundPlaying = _sound.Playing, soundPosition = _sound.GetPlaybackPosition(),
        receipts = _receipts.ToArray(),
    };

    private void Record(string operation, AudioStreamPlayer player, string cue, int? command = null)
    {
        var asset = _assets[cue];
        _receipts.Add(new(++_sequence, operation, cue, command ?? asset.Command, asset.TimerB, asset.PcmSha256,
            asset.SampleRate, asset.Channels, asset.SampleFrames, asset.LoopBegin, asset.LoopEnd,
            player.Playing, player.GetPlaybackPosition(), Time.GetTicksUsec(), _revision, _waitToken));
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

    internal void Play(int command)
    {
        if (command == 0) return;
        string? cue = _assets.FirstOrDefault(pair => pair.Value.Command == command &&
            (command < 65 || pair.Value.TimerB == TimerB)).Key;
        Play(cue ?? throw new InvalidOperationException("audio-command-unavailable"));
    }

    internal void PlayEffect(int command)
    {
        if (Error is not null || _assets.Count == 0 || command == 0) return;
        try { Play(command); }
        catch (InvalidOperationException error) { Error = error.Message; }
    }

    internal void Play(string resource, bool remember = true)
    {
        if (!_assets.TryGetValue(resource, out var audio)) throw new InvalidOperationException("audio-content-unavailable");
        bool music = audio.Command < 65;
        // The source suppresses identical music requests even when the finite track already ended.
        if (music && remember && MusicCue == resource) return;
        var player = music ? _music : _sound;
        if (!music && audio.TimerB != TimerB) throw new InvalidOperationException("sfx-timer-context-unavailable");
        Stop(player, music ? MusicCue : _soundCue);
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
        foreach (var player in new[] { _music, _sound })
        {
            player.Stop();
            var stream = player.Stream; player.Stream = null; stream?.Dispose();
        }
    }
}

internal sealed record AudioPlaybackReceipt(long Sequence, string Operation, string Cue, int Command,
    int TimerB, string PcmSha256, int SampleRate, int Channels, int SampleFrames, int? LoopBegin,
    int? LoopEnd, bool Playing, double PlaybackPosition, ulong Microseconds, long Revision, long? WaitToken);
