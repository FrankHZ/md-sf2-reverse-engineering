using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Battles;

namespace Sf2.Remake.GodotAdapter;

// Ordinary game composition only. Reference play has its own project and entry scene.
public sealed partial class GameRoot : Node
{
    public override void _Ready()
    {
        var view = new BattleSessionView { Name = "BattleSessionView" };
        AddChild(view);
        var options = new Dictionary<string, string>(StringComparer.Ordinal);
        var arguments = OS.GetCmdlineUserArgs();
        for (int index = 0; index < arguments.Length; index++)
        {
            string option = arguments[index];
            if (option is not ("--authored-package" or "--private-battle-start"))
            { Fail("unknown-startup-option", "Unknown game option. Use --authored-package or --private-battle-start."); return; }
            if (options.ContainsKey(option))
            { Fail("duplicate-startup-option", "Each game option may appear only once."); return; }
            if (options.Count != 0)
            { Fail("conflicting-startup-options", "Select one battle entry."); return; }
            if (++index >= arguments.Length || string.IsNullOrWhiteSpace(arguments[index]) ||
                arguments[index].StartsWith("--", StringComparison.Ordinal))
            { Fail("missing-startup-path", "The selected game option requires a path."); return; }
            options.Add(option, arguments[index]);
        }
        if (options.TryGetValue("--private-battle-start", out string? controlledStart))
            view.Begin(new PrivateBattleScenarioReader(
                System.Environment.GetEnvironmentVariable("SF2_PRIVATE_BATTLE01_DATA") ?? "",
                System.Environment.GetEnvironmentVariable("SF2_PRIVATE_BATTLE01_SCENE") ?? "",
                System.Environment.GetEnvironmentVariable("SF2_PRIVATE_BATTLE01_TERRAIN") ?? "",
                System.Environment.GetEnvironmentVariable("SF2_PRIVATE_STATIC_DATA") ?? "",
                System.Environment.GetEnvironmentVariable("SF2_PRIVATE_ENEMY_DATA") ?? "",
                System.Environment.GetEnvironmentVariable("SF2_PRIVATE_ENEMY_GOLD") ?? "", controlledStart));
        else
        {
            string package = options.GetValueOrDefault("--authored-package") ??
                System.IO.Path.Combine(ProjectSettings.GlobalizePath("res://"), "..", "content", "authored", "practice-yard.json");
            view.Begin(new AuthoredScenarioPackageReader(package));
        }

        void Fail(string code, string message) =>
            view.FailStartup(new(SessionFailureKind.ContentError, code, "startup", message));
    }
}
