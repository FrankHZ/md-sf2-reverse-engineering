using System.Text.Json;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Content.Scenarios;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class GameRoot
{
    // Controlled observation startup, before any gameplay session exists. Uses the ordinary
    // private admission and mount; no active snapshot setters, overlay injection or source edits.
    public string StartPreVoyageLocalizationObservation(string inputPath)
    {
        if (_session is not null) return "observation-session-already-started";
        string repository = Path.GetFullPath(Path.Combine(Godot.ProjectSettings.GlobalizePath("res://"), "..", ".."));
        string local = Path.Combine(repository, "local") + Path.DirectorySeparatorChar;
        if (!Path.IsPathFullyQualified(inputPath) || !Path.GetFullPath(inputPath).StartsWith(local, StringComparison.OrdinalIgnoreCase) ||
            new FileInfo(inputPath).Length > 65536) return "observation-input";
        using var document = JsonDocument.Parse(File.ReadAllBytes(inputPath));
        var start = document.RootElement.GetProperty("start");
        var read = new OpeningNewGameReader(Selected("SF2_PRIVATE_EXPLORATION_CONTENT"),
            Selected("SF2_PRIVATE_BATTLE01_DATA"), Selected("SF2_PRIVATE_BATTLE01_SCENE"), Selected("SF2_PRIVATE_BATTLE01_TERRAIN"),
            Selected("SF2_PRIVATE_STATIC_DATA"), Selected("SF2_PRIVATE_ENEMY_DATA"), Selected("SF2_PRIVATE_ENEMY_GOLD"),
            305441741, 305397760, System.Environment.GetEnvironmentVariable("SF2_PRIVATE_BATTLE_SCENE_CONTENT")).Read();
        if (read is not ExplorationReadAccepted admitted) return ((ScenarioReadRejected)read).Failure.Code;
        var old = admitted.Start;
        if (start.TryGetProperty("battle", out var battleEntry))
        {
            var seed = admitted.Definition.EncounterStarts[battleEntry.GetString()!];
            var inputs = seed.Actors.Select(actor => old.Party.Actors.FirstOrDefault(ally => ally.Actor == actor.Actor) ?? actor).ToArray();
            var policy = new NewBattleStartPolicy("localization-observation", "controlled-consumer", "Not natural reach.",
                0, true, true, true, false, false, null);
            var initialized = GameSession.Start(admitted.Definition, new BattleStartInput(seed.Encounter, inputs,
                old.Party.MainSeed, old.Party.ThinkingSeed, 60, policy, itemCustody: old.Party.ItemCustody));
            if (initialized is SessionStartFailed failure) return failure.Failure.Code;
            var baseline = ((SessionStarted)initialized).Session.Current.Battle;
            inputs = baseline.Actors.Select(actor => new BattleActorStartInput(actor.Actor, actor.Hp, actor.Mp,
                actor.Exp ?? 0, actor.Kills ?? 0, actor.Defeats ?? 0, actor.Status, null, actor.Progress, actor.SourceLoadout,
                actor.ActivationWord, actor.Initialized)).ToArray();
            if (start.TryGetProperty("actors", out var overrides))
                foreach (var row in overrides.EnumerateArray())
                {
                    int index = Array.FindIndex(inputs, actor => actor.Actor.Value == row.GetProperty("actor").GetString());
                    var actor = inputs[index];
                    inputs[index] = actor with { Hp = row.TryGetProperty("hp", out var hp) ? hp.GetUInt16() : actor.Hp,
                        PositionOverride = row.TryGetProperty("position", out var point)
                            ? new(point.GetProperty("x").GetInt32(), point.GetProperty("y").GetInt32()) : actor.PositionOverride };
                }
            var battle = new BattleStartInput(seed.Encounter, inputs, baseline.MainSeed, baseline.ThinkingSeed,
                60, itemCustody: baseline.ItemCustody);
            BeginSource(new LocalizationObservationSource(new ScenarioReadAccepted(admitted.Definition, battle)));
            return _session is null ? "observation-mount-failed" : "started";
        }
        int[] flags = start.GetProperty("flags").EnumerateArray().Select(flag => flag.GetInt32()).ToArray();
        var active = flags.Where(flag => flag is >= 32 and < 62).Order().Select(flag => new ActorRef("ally-" + (flag - 32))).ToArray();
        var party = new BattleStartInput(old.Party.Encounter, old.Party.Actors, old.Party.MainSeed, old.Party.ThinkingSeed,
            60, old.Party.NewBattle, active, old.Party.ItemCustody);
        var map = new MapId(start.GetProperty("map").GetString()!);
        var palette = admitted.Definition.Exploration!.Maps[map].BasePalette!;
        var position = start.GetProperty("position");
        var controlled = new ExplorationStartInput(map, new(start.GetProperty("player").GetString()!),
            new(position.GetProperty("x").GetInt32(), position.GetProperty("y").GetInt32()),
            start.GetProperty("facing").GetByte(), start.GetProperty("speed").GetUInt16(), flags, party,
            start.TryGetProperty("program", out var program) ? new(program.GetProperty("program").GetString()!, 0) : null,
            display: new(3, palette, palette, FullFadeVisibility.BaseRestored), textSettings: old.TextSettings,
            stepCounter: 0, egressMap: old.Map);
        BeginSource(new LocalizationObservationSource(new ExplorationReadAccepted(admitted.Definition, controlled)));
        return _session is null ? "observation-mount-failed" : "started";
    }
    private sealed class LocalizationObservationSource(ScenarioReadResult read) : IScenarioSource
    {
        public ScenarioReadResult Read() => read;
    }
}
