using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Audio;
using Sf2.Remake.GodotAdapter.Exploration;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.Maps;

namespace Sf2.Remake.GodotAdapter.Probes;

// One bounded actual-adapter observation over public authored silence. Completion
// commands come from ExplorationPresentation and native AudioStreamPlayer.Finished.
public partial class SceneMusicContextObservation : Control
{
    private readonly List<object> _samples = [];

    public override async void _Ready()
    {
        try
        {
            await Observe(false);
            await Observe(true);
            var report = new { passed = true, driver = AudioServer.GetDriverName(), samples = _samples };
            System.IO.File.WriteAllText(OS.GetEnvironment("SF2_SCENE_MUSIC_OUTPUT"), JsonSerializer.Serialize(report));
            GD.Print("SCENE-MUSIC ", JsonSerializer.Serialize(report));
            GetTree().Quit();
        }
        catch (Exception error)
        {
            GD.Print("SCENE-MUSIC-SAMPLES ", JsonSerializer.Serialize(_samples));
            GD.PushError(error.ToString());
            GetTree().Quit(1);
        }
    }

    private async Task Observe(bool offArea)
    {
        var path = ProjectSettings.GlobalizePath("res://../content/authored/scene-music-context.json");
        var document = JsonNode.Parse(System.IO.File.ReadAllText(path))!;
        if (offArea) document["world"]!["maps"]![1]!["areas"]![0]!["minY"] = 4;
        var outcome = GameSession.Start(AuthoredScenarioPackageReader.FromDocumentBytes(Encoding.UTF8.GetBytes(document.ToJsonString())));
        Check(outcome is SessionStarted, outcome.ToString()!);
        var started = (SessionStarted)outcome;
        var session = started.Session;
        using var audio = new SessionAudio(this, session.Definition.Exploration!);
        using var presentation = new ExplorationPresentation(this, session.Definition.Exploration!,
            new InputSettings { FormatVersion = 1 }, () => throw new InvalidOperationException("unexpected-battle"), audio, new());
        audio.Observe(started.Result);
        Check(audio.MusicCue == "area-a", "initial field cue");
        ulong deadline = Time.GetTicksMsec() + 10000;
        bool sceneObserved = false, finiteObserved = false, nativeCompletion = false;
        while (Time.GetTicksMsec() < deadline)
        {
            var current = session.Current;
            if (current.Exploration!.Map.Value == "yard-map" && !sceneObserved)
            {
                var context = ExplorationViewContext.Resolve(current.Exploration, current.Story.LogicalView);
                Check(context.Error is null && context.Area == 1, "scene context B");
                Check((current.Exploration.Definition.Traversal.SelectActiveArea(current.Exploration.PlayerEntity.Position) is null) == offArea,
                    "retained player variation");
                Check(audio.MusicCue == "area-b" && audio.MusicPlaying, "actual scene B map playback");
                Sample("scene-selected");
                sceneObserved = true;
            }
            foreach (var completion in presentation.Update(GetProcessDeltaTime(), current))
            {
                if (completion is CompleteMusic)
                {
                    Check(audio.MusicFinished && audio.Completions == 1, "actual finite Finished callback");
                    nativeCompletion = true;
                    Sample("native-finite-completion");
                }
                Submit(completion);
            }
            Check(presentation.Error is null && audio.Error is null, presentation.Error ?? audio.Error ?? "adapter");
            current = session.Current;
            if (current.Story.Music is { Cue: "finite" } music && !finiteObserved)
            {
                Check(music.Area == 1 && music.Previous.SequenceEqual(new[] { "area-b" }), "logical B history");
                Check(audio.MusicCue == music.Cue && audio.MusicGeneration == music.Generation && audio.MusicPlaying,
                    "actual finite generation binding");
                Sample("finite-started");
                finiteObserved = true;
            }
            if (current.Story.Music is { Cue: "area-b" } && current.Story.Wait is TickWait)
            {
                Check(sceneObserved && finiteObserved && nativeCompletion, "all native stages reached");
                Check(audio.MusicCue == "area-b" && audio.MusicPlaying && audio.Completions == 1, "actual previous restart");
                Sample("previous-restarted");
                return;
            }
            if (current.Story.Wait is TickWait || current.Story.Wait is MusicWait { LogicalDone: false })
                Submit(new AdvanceSimulation(current.Story.Wait.Token));
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
        throw new InvalidOperationException("scene-music-observation-timeout");

        void Submit(SessionCommand command)
        {
            var current = session.Current;
            var result = session.Submit(new(current.SessionId, current.Revision, current.Selection?.Actor, command));
            Check(result.Failure is null, result.Failure?.ToString() ?? "session");
            audio.Observe(result);
        }

        void Sample(string stage)
        {
            var current = session.Current;
            _samples.Add(new { offArea, stage, current.Revision,
                context = ExplorationViewContext.Resolve(current.Exploration!, current.Story.LogicalView),
                player = current.Exploration!.PlayerEntity.Position, scroll = current.Story.LogicalView!.Area.AutoscrollAX,
                music = current.Story.Music, wait = current.Story.Wait?.GetType().Name,
                audio = audio.ObservePlayback() });
        }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
