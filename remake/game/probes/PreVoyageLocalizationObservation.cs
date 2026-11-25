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
    // Read-only, local controlled records: observe the production formatter without mounting or rewriting a save.
    public string ReadSaveSummaryObservationJson(string inputPath, string savedAt = "")
    {
        string local = Path.GetFullPath(Path.Combine(Godot.ProjectSettings.GlobalizePath("res://"), "..", "..", "local")) + Path.DirectorySeparatorChar;
        if (!Path.IsPathFullyQualified(inputPath) || !Path.GetFullPath(inputPath).StartsWith(local, StringComparison.OrdinalIgnoreCase) ||
            new FileInfo(inputPath).Length > Sf2.Remake.Content.Persistence.OpeningSaveJson.MaximumBytes)
            throw new ArgumentException("observation-input");
        Sf2.Remake.Content.Persistence.SaveRecord record;
        try { record = Sf2.Remake.Content.Persistence.OpeningSaveJson.ReadRecord(File.ReadAllBytes(inputPath), null); }
        catch (Sf2.Remake.Content.Persistence.SaveFormatException error) { return JsonSerializer.Serialize(new { error = error.Code }); }
        if (savedAt != "") record = record with { SavedAt = DateTimeOffset.Parse(savedAt, System.Globalization.CultureInfo.InvariantCulture) };
        var definition = _session?.Definition ?? ((ExplorationReadAccepted)RestoreSource().Read()).Definition;
        return JsonSerializer.Serialize(new { kind = record.GetType().Name, level = SavedLevel(record, definition),
            time = SavedTime(record), zone = TimeZoneInfo.Local.Id,
            label = Label(record.Slot, new(new(record, null), new(null, null)), definition) });
    }

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
            // Campaign encounter definitions can contain undeployed reserve actors.
            var activeAllies = start.TryGetProperty("activeAllies", out var lineup)
                ? lineup.EnumerateArray().Select(id => new ActorRef(id.GetString()!)).ToArray() : null;
            var initialized = GameSession.Start(admitted.Definition, new BattleStartInput(seed.Encounter, inputs,
                old.Party.MainSeed, old.Party.ThinkingSeed, 60, policy, activeAllies, itemCustody: old.Party.ItemCustody, deathCredit: seed.DeathCredit));
            if (initialized is SessionStartFailed failure) return failure.Failure.Code;
            var baseline = ((SessionStarted)initialized).Session.Current.Battle;
            inputs = baseline.Actors.Select(actor => new BattleActorStartInput(actor.Actor, actor.Hp, actor.Mp,
                actor.Exp ?? 0, actor.Kills ?? 0, actor.Defeats ?? 0, actor.Status, activeAllies is null ? null : actor.Position, actor.Progress, actor.SourceLoadout,
                actor.ActivationWord, actor.Initialized, Absent: activeAllies is not null && actor.Position is null)).ToArray();
            if (start.TryGetProperty("partyInput", out var battlePartyInput))
            {
                // Read a disposable party fixture through the existing controlled start reader.
                // Import its level and learned book for the named allies before the battle mounts.
                var path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(inputPath)!, battlePartyInput.GetString()!));
                if (!path.StartsWith(local, StringComparison.OrdinalIgnoreCase) || !File.Exists(path))
                    return "observation-party-input";
                var partyReader = new PrivateBattleScenarioReader(Selected("SF2_PRIVATE_BATTLE01_DATA"),
                    Selected("SF2_PRIVATE_BATTLE01_SCENE"), Selected("SF2_PRIVATE_BATTLE01_TERRAIN"),
                    Selected("SF2_PRIVATE_STATIC_DATA"), Selected("SF2_PRIVATE_ENEMY_DATA"), Selected("SF2_PRIVATE_ENEMY_GOLD"), path);
                if (partyReader.Read() is not ScenarioReadAccepted acceptedParty)
                    return "observation-party-rejected";
                foreach (var row in acceptedParty.Definition.Encounters.Values.Single().Deployments.Where(row => row.Faction == BattleFaction.Ally))
                {
                    int index = Array.FindIndex(inputs, actor => actor.Actor == row.Actor);
                    if (index < 0 || inputs[index].Progress is not { } progress ||
                        inputs[index].SourceLoadout is not { } original) return "observation-party-progression";
                    var member = baseline.GetActor(row.Actor).Definition;
                    if (member.Growth is not { } growth || row.Definition.ClassRule != member.ClassRule ||
                        row.Definition.Level < progress.Level || row.Definition.SourceLoadout is not { } loadout ||
                        !loadout.Items.SequenceEqual(original.Items) ||
                        loadout.Spells.Where(packed => packed != 63).Any(packed =>
                            !growth.Spells.Any(spell => spell.PackedSpell == packed && spell.Level <= row.Definition.Level)))
                        return "observation-party-progression";
                    inputs[index] = inputs[index] with { SourceLoadout = loadout,
                        Progress = progress with { Level = row.Definition.Level, Spells = row.Definition.Spells, SourceLoadout = loadout } };
                }
            }
            if (start.TryGetProperty("actors", out var overrides))
                foreach (var row in overrides.EnumerateArray())
                {
                    int index = Array.FindIndex(inputs, actor => actor.Actor.Value == row.GetProperty("actor").GetString());
                    var actor = inputs[index];
                    inputs[index] = actor with { Hp = row.TryGetProperty("hp", out var hp) ? hp.GetUInt16() : actor.Hp,
                        Status = row.TryGetProperty("status", out var status) ? status.GetUInt16() : actor.Status,
                        PositionOverride = row.TryGetProperty("position", out var point)
                            ? new(point.GetProperty("x").GetInt32(), point.GetProperty("y").GetInt32()) : actor.PositionOverride };
                }
            uint mainSeed = start.TryGetProperty("mainSeed", out var selectedSeed) ? selectedSeed.GetUInt32() : baseline.MainSeed;
            var battle = new BattleStartInput(seed.Encounter, inputs, mainSeed, baseline.ThinkingSeed,
                60, itemCustody: baseline.ItemCustody, deathCredit: baseline.DeathCredit);
            BeginSource(new LocalizationObservationSource(new ScenarioReadAccepted(admitted.Definition, battle)));
            return _session is null ? "observation-mount-failed" : "started";
        }
        int[] flags = start.GetProperty("flags").EnumerateArray().Select(flag => flag.GetInt32()).ToArray();
        var active = flags.Where(flag => flag is >= 32 and < 62).Order().Select(flag => new ActorRef("ally-" + (flag - 32))).ToArray();
        var initialParty = old.Party;
        if (start.TryGetProperty("partyInput", out var selectedParty))
        {
            // Explicit initial input only, validated by the existing controlled reader.
            // Full-world admission stays with OpeningNewGameReader above.
            if (selectedParty.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(selectedParty.GetString()))
                return "observation-party-input";
            string inputRoot = Path.Combine(repository, "remake", "reference", "inputs") + Path.DirectorySeparatorChar;
            string path;
            try { path = Path.GetFullPath(Path.Combine(repository, selectedParty.GetString()!)); }
            catch (ArgumentException) { return "observation-party-input"; }
            if (!path.StartsWith(inputRoot, StringComparison.OrdinalIgnoreCase)) return "observation-party-input";
            var file = new FileInfo(path);
            if (!file.Exists || file.Length > 65536) return "observation-party-input";
            if (PrivateBattle(path).Read() is not ScenarioReadAccepted acceptedParty) return "observation-party-rejected";
            initialParty = acceptedParty.Start;
            var initialized = GameSession.Start(acceptedParty.Definition, initialParty, RuleCompositions.ForGame());
            if (initialized is SessionStartFailed partyFailure) return partyFailure.Failure.Code;
            var actors = ((SessionStarted)initialized).Session.Current.Battle.Actors.Select(actor => new BattleActorStartInput(
                actor.Actor, actor.Hp, actor.Mp, actor.Exp ?? 0, actor.Kills ?? 0, actor.Defeats ?? 0,
                actor.Status, null, actor.Progress ?? new BattleActorProgress(actor.Level, actor.MaxHp, actor.MaxMp,
                    actor.BaseAttack, actor.Defense, actor.Agility, actor.Definition.Spells, actor.SourceLoadout),
                actor.SourceLoadout, actor.ActivationWord, actor.Initialized)).ToArray();
            initialParty = new(initialParty.Encounter, actors, initialParty.MainSeed, initialParty.ThinkingSeed,
                initialParty.Gold, initialParty.NewBattle, itemCustody: initialParty.ItemCustody);
        }
        var party = new BattleStartInput(initialParty.Encounter, initialParty.Actors, initialParty.MainSeed, initialParty.ThinkingSeed,
            start.TryGetProperty("partyInput", out _) ? initialParty.Gold : 60, initialParty.NewBattle, active, initialParty.ItemCustody);
        var map = new MapId(start.GetProperty("map").GetString()!);
        var palette = admitted.Definition.Exploration!.Maps[map].BasePalette!;
        var position = start.GetProperty("position");
        var names = new Dictionary<string, string>();
        if (start.TryGetProperty("names", out var declaredNames))
            foreach (var row in declaredNames.EnumerateObject())
            {
                string? value = row.Value.GetString();
                if (string.IsNullOrEmpty(value) || NewGameName.Error(value) is not null || !admitted.Definition.Encounters.Values
                    .SelectMany(e => e.Deployments).Any(d => d.Faction == BattleFaction.Ally && d.Actor.Value == row.Name)) return "observation-name";
                names.Add(row.Name, value);
            }
        var controlled = new ExplorationStartInput(map, new(start.GetProperty("player").GetString()!),
            new(position.GetProperty("x").GetInt32(), position.GetProperty("y").GetInt32()),
            start.GetProperty("facing").GetByte(), start.GetProperty("speed").GetUInt16(), flags, party,
            start.TryGetProperty("program", out var program) ? new(program.GetProperty("program").GetString()!, 0) : null,
            display: new(3, palette, palette, FullFadeVisibility.BaseRestored), textSettings: old.TextSettings,
            stepCounter: 0, egressMap: old.Map) { Names = names };
        BeginSource(new LocalizationObservationSource(new ExplorationReadAccepted(admitted.Definition, controlled)));
        return _session is null ? "observation-mount-failed" : "started";
    }
    private sealed class LocalizationObservationSource(ScenarioReadResult read) : IScenarioSource
    {
        public ScenarioReadResult Read() => read;
    }
}
