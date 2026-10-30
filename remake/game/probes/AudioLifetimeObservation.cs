using System.Reflection;
using System.Text.Json;
using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.GodotAdapter.Audio;

namespace Sf2.Remake.GodotAdapter.Probes;

// Bounded adapter observation. Synthetic silence isolates ownership from private content.
// Reflection constructs immutable observation data, never changes a running session.
public partial class AudioLifetimeObservation : Node
{
    private SessionAudio? _audio;
    private readonly List<ulong> _released = [];
    private readonly List<object> _samples = [];

    public override async void _Ready()
    {
        try
        {
            _audio = new SessionAudio(this, Definition());
            for (int cycle = 0; cycle < 4; cycle++)
            {
                int before = _audio.Completions;
                _audio.Play("loop");
                Remember(Music());
                _audio.Play("finite");
                Remember(Music());
                await Until(() => _audio.FiniteMusicFinished(), "finite-music-finished");
                Check(_audio.Completions == before + 1, "finite music callback exactly once");
                _audio.Play("loop");
                await Released("music-replaced", cycle);

                _audio.Play("effect");
                Remember(Sounds().Single());
                await Until(() => _audio.Completions == before + 2, "effect-finished");
                await Released("effect-finished", cycle);

                _audio.Play("effect");
                Remember(Sounds().Single());
                _audio.Play("effect"); // Same-slot interruption is not Finished.
                _audio.Play("other");
                Check(Sounds().Count() == 2, "two disjoint active effects");
                foreach (var sound in Sounds()) Remember(sound);
                await Until(() => _audio.Completions == before + 4, "disjoint natural tails");
                await Released("interrupted-and-disjoint-finished", cycle);
            }
            int completions = _audio.Completions;
            _audio.Play("effect");
            _audio.Play("other");
            foreach (var player in GetChildren().OfType<AudioStreamPlayer>()) Remember(player);
            _audio.Dispose();
            await Released("active-dispose", 4);
            Check(_audio.Completions == completions, "disposal does not invent completion");
            Check(!GetChildren().OfType<AudioStreamPlayer>().Any(), "disposed nodes released");
            Finish();
        }
        catch (Exception error)
        {
            foreach (ulong id in _released.Where(IsAlive))
            {
                GD.Print("RETAINED ", id);
            }
            GD.Print("SESSION-SAMPLES ", JsonSerializer.Serialize(_samples));
            GD.PushError(error.ToString()); GetTree().Quit(1);
        }
    }

    private AudioStreamPlayer Music() => GetChildren().OfType<AudioStreamPlayer>().Single(p => p.Name == "SessionMusic");
    private IEnumerable<AudioStreamPlayer> Sounds() => GetChildren().OfType<AudioStreamPlayer>()
        .Where(p => p != Music() && p.Playing && !p.IsQueuedForDeletion());
    private void Remember(AudioStreamPlayer player)
    {
        // Store IDs only. Holding a managed Resource or playback wrapper would change its lifetime.
        _released.Add(GetTree().Call("stream_id", player).AsUInt64());
        _released.Add(GetTree().Call("playback_id", player).AsUInt64());
    }

    private bool IsAlive(ulong id) => GetTree().Call("alive", unchecked((long)id)).AsBool();

    private async Task Released(string label, int cycle)
    {
        await Until(() => _released.All(id => !IsAlive(id)), label);
        _samples.Add(new { label, cycle, releasedObjects = _released.Count });
        _released.Clear();
    }

    private async Task Until(Func<bool> predicate, string label)
    {
        ulong deadline = Time.GetTicksMsec() + 2000;
        while (!predicate() && Time.GetTicksMsec() < deadline)
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Check(predicate(), label);
        // Observe node QueueFree as well as resource release before selecting children again.
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    private static void Check(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException(label);
    }

    private void Finish()
    {
        var result = new { passed = true, driver = AudioServer.GetDriverName(), samples = _samples };
        System.IO.File.WriteAllText(OS.GetEnvironment("SF2_AUDIO_OUTPUT"), JsonSerializer.Serialize(result));
        GD.Print("SESSION-LIFETIME ", JsonSerializer.Serialize(result));
        GetTree().Quit();
    }

    public override void _ExitTree() => _audio?.Dispose();

    private static ExplorationDefinition Definition()
    {
        var audio = new Dictionary<string, ExplorationAudio>
        {
            ["loop"] = Clip(1, true), ["finite"] = Clip(2, false),
            ["effect"] = Clip(67, false), ["other"] = Clip(83, false),
        };
        var visuals = new ExplorationVisuals(new Dictionary<MapId, ExplorationMapVisual>(),
            new Dictionary<int, ExplorationSpriteVisual>(), new Dictionary<int, ExplorationPortraitVisual>(), audio);
        return (ExplorationDefinition)Activator.CreateInstance(typeof(ExplorationDefinition),
            BindingFlags.Instance | BindingFlags.NonPublic, null,
            [Array.Empty<ExplorationMapDefinition>(), Array.Empty<StoryProgram>(), null, null, null, visuals, null, null, null], null)!;
    }

    private static ExplorationAudio Clip(int command, bool loop) =>
        (ExplorationAudio)Activator.CreateInstance(typeof(ExplorationAudio), BindingFlags.Instance | BindingFlags.NonPublic,
            null, [command, 204, 8000, 1, new byte[1600], "authored-silence", loop ? 0 : (int?)null,
                loop ? 800 : (int?)null, null], null)!;
}
