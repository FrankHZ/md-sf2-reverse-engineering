using System.Text.Json;
using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Audio;

namespace Sf2.Remake.GodotAdapter.Probes;

// Controlled playback over admitted existing PCM, not original natural reach or acoustic parity.
public partial class FiniteAudioDefaultObservation : Node
{
    private readonly List<object> _samples = [];

    public override async void _Ready()
    {
        try
        {
            string Selected(string name) => OS.GetEnvironment(name);
            var read = new OpeningNewGameReader(Selected("SF2_PRIVATE_EXPLORATION_CONTENT"),
                Selected("SF2_PRIVATE_BATTLE01_DATA"), Selected("SF2_PRIVATE_BATTLE01_SCENE"),
                Selected("SF2_PRIVATE_BATTLE01_TERRAIN"), Selected("SF2_PRIVATE_STATIC_DATA"),
                Selected("SF2_PRIVATE_ENEMY_DATA"), Selected("SF2_PRIVATE_ENEMY_GOLD"), 305441741, 305397760,
                Selected("SF2_PRIVATE_BATTLE_SCENE_CONTENT")).Read();
            Check(read is ExplorationReadAccepted, read.ToString()!);
            var assets = ((ExplorationReadAccepted)read).Definition.Exploration!.Visuals!.Audio;
            using var audio = new SessionAudio(this, assets);
            string loopCue = Selected("SF2_AUDIO_LOOP_CUE");
            if (!string.IsNullOrEmpty(loopCue))
            {
                await ObserveLoop(audio, assets[loopCue], loopCue);
                return;
            }
            audio.Play("MUSIC_MAIN_THEME"); // Real C2 music, unchanged recording.
            await Effect("death-command", () => audio.PlayEffect(116), "SFX_BATTLEFIELD_DEATH_TIMER_BD", 194);
            await Effect("death-named", () => audio.Play("SFX_BATTLEFIELD_DEATH_TIMER_C1"), "SFX_BATTLEFIELD_DEATH_TIMER_BD", 194);
            await Effect("validation-named", () => audio.Play("SFX_VALIDATION_TIMER_D2"), "SFX_VALIDATION_TIMER_CB", 194);
            audio.Play("MUSIC_ATTACK"); // Different real context, also unmatched.
            await Effect("validation-command", () => audio.PlayEffect(67), "SFX_VALIDATION_TIMER_CB", 198);
            audio.PlayPrevious();
            Check(audio.MusicCue == "MUSIC_MAIN_THEME" && audio.TimerB == 194, "music history unchanged");
            await Effect("post-completion-input-effect", () => audio.PlayEffect(65), "SFX_MENU_SWITCH_TIMER_C2", 194);
            await Effect("named-exact-before-default", () => audio.Play("SFX_MENU_SWITCH_TIMER_CB"), "SFX_MENU_SWITCH_TIMER_C2", 194);
            audio.CutMusic();
            var report = new { passed = true, boundary = "controlled actual adapter playback; unchanged private PCM", samples = _samples };
            System.IO.File.WriteAllText(OS.GetEnvironment("SF2_AUDIO_SELECTION_OUTPUT"), JsonSerializer.Serialize(report));
            GD.Print("FINITE-AUDIO-DEFAULT PASS");
            GetTree().Quit();

            async Task Effect(string label, Action play, string expected, int requested)
            {
                int before = audio.Completions;
                play();
                Check(audio.Error is null, audio.Error ?? "audio error");
                var start = JsonSerializer.SerializeToElement(audio.ObservePlayback());
                var receipt = start.GetProperty("receipts").EnumerateArray().Last();
                Check(receipt.GetProperty("Operation").GetString() == "started" && receipt.GetProperty("Cue").GetString() == expected,
                    label + " selected cue");
                var selected = assets[expected];
                Check(receipt.GetProperty("TimerB").GetInt32() == selected.TimerB &&
                    receipt.GetProperty("RequestedTimerB").GetInt32() == requested &&
                    receipt.GetProperty("PcmSha256").GetString() == selected.PcmSha256 &&
                    receipt.GetProperty("SampleRate").GetInt32() == selected.SampleRate, label + " identity/timers");
                ulong deadline = Time.GetTicksMsec() + 5000;
                while (audio.Completions == before && Time.GetTicksMsec() < deadline)
                    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                Check(audio.Completions == before + 1, label + " exactly one Finished");
                var end = JsonSerializer.SerializeToElement(audio.ObservePlayback());
                var finished = end.GetProperty("receipts").EnumerateArray().Last();
                Check(finished.GetProperty("Operation").GetString() == "finished" &&
                    finished.GetProperty("Cue").GetString() == expected, label + " native receipt");
                _samples.Add(new { label, start, end });
            }
        }
        catch (Exception error)
        {
            System.IO.File.WriteAllText(OS.GetEnvironment("SF2_AUDIO_SELECTION_OUTPUT"),
                JsonSerializer.Serialize(new { passed = false, error = error.ToString(), samples = _samples }));
            GD.PushError(error.ToString());
            GetTree().Quit(1);
        }
    }

    private async Task ObserveLoop(SessionAudio audio, ExplorationAudio asset, string cue)
    {
        Check(asset.LoopBegin is not null && asset.LoopEnd is not null, "selected loop boundaries");
        audio.Play(cue);
        var player = GetChildren().OfType<AudioStreamPlayer>().Single(p => p.Name == "SessionMusic");
        int starts = audio.Starts;
        await ToSignal(GetTree().CreateTimer(2), SceneTreeTimer.SignalName.Timeout);
        double before = player.GetPlaybackPosition();
        for (int i = 0; i < 30; i++)
        {
            audio.Play(cue);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
        Check(audio.Starts == starts && player.GetPlaybackPosition() >= before, "same cue keeps playback");
        _samples.Add(new { label = "same-cue", audio = audio.ObservePlayback() });
        double end = asset.LoopEnd!.Value / (double)asset.SampleRate;
        double begin = asset.LoopBegin!.Value / (double)asset.SampleRate;
        player.Seek((float)(end - 0.3));
        await ToSignal(GetTree().CreateTimer(0.8), SceneTreeTimer.SignalName.Timeout);
        double wrapped = player.GetPlaybackPosition();
        Check(player.Playing && audio.Completions == 0 && wrapped >= begin && wrapped < begin + 2,
            "actual forward loop wraps and stays playing");
        _samples.Add(new { label = "loop-wrapped", wrapped, begin, end, audio = audio.ObservePlayback() });
        audio.Play("MUSIC_TOWN");
        Check(audio.MusicCue == "MUSIC_TOWN", "scene music switched");
        audio.PlayPrevious();
        Check(audio.MusicCue == cue && player.GetPlaybackPosition() < 1, "previous music restarts");
        Check(audio.Starts == starts + 2 && audio.Error is null, "one switch and one restore");
        _samples.Add(new { label = "previous-restored", audio = audio.ObservePlayback() });
        audio.CutMusic();
        System.IO.File.WriteAllText(OS.GetEnvironment("SF2_AUDIO_SELECTION_OUTPUT"),
            JsonSerializer.Serialize(new { passed = true, driver = AudioServer.GetDriverName(), samples = _samples }));
        GD.Print("LOOP-AUDIO PASS");
        GetTree().Quit();
    }

    private static void Check(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
}
