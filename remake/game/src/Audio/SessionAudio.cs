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
    internal SessionAudio(Node owner, ExplorationDefinition definition)
    {
        _assets = definition.Visuals?.Audio ?? new Dictionary<string, ExplorationAudio>();
        _maps = definition.Visuals?.Maps ?? new Dictionary<MapId, ExplorationMapVisual>();
        _music = new AudioStreamPlayer { Name = "SessionMusic" };
        _sound = new AudioStreamPlayer { Name = "SessionSound" };
        owner.AddChild(_music); owner.AddChild(_sound);
        _music.Finished += () => { MusicFinished = true; Completions++; };
        _sound.Finished += () => Completions++;
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

    internal void Observe(SessionResult result)
    {
        if (Error is not null || result.Failure is not null || _assets.Count == 0) return;
        try
        {
            var current = result.Snapshot;
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
        string? cue = _assets.FirstOrDefault(pair => pair.Value.Command == command).Key;
        Play(cue ?? throw new InvalidOperationException("audio-command-unavailable"));
    }

    internal void Play(string resource, bool remember = true)
    {
        if (!_assets.TryGetValue(resource, out var audio)) throw new InvalidOperationException("audio-content-unavailable");
        bool music = audio.Command < 65;
        // The source suppresses identical music requests even when the finite track already ended.
        if (music && remember && MusicCue == resource) return;
        var player = music ? _music : _sound;
        if (player.Playing) { player.Stop(); Stops++; }
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
            MusicCue = resource; MusicFinished = false;
            if (remember)
            {
                _history.Insert(0, resource);
                if (_history.Count > 10) _history.RemoveAt(10);
            }
        }
        player.VolumeDb = 0;
        player.Play();
        if (!player.Playing) throw new InvalidOperationException("audio-stream-did-not-start");
        Starts++;
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
        if (progress >= 1 && _music.Playing) { _music.Stop(); Stops++; }
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
