using Godot;

namespace Sf2.Remake.GodotAdapter.Entry;

// The local candidate carries the same reader inputs as the development host.
internal static class PackagedStartup
{
    internal static string StorageBoundary => OS.HasFeature("local_candidate")
        ? System.IO.Path.GetDirectoryName(OS.GetExecutablePath())!
        : System.IO.Path.GetFullPath(System.IO.Path.Combine(ProjectSettings.GlobalizePath("res://"), "..", ".."));

    internal static void Configure()
    {
        if (!OS.HasFeature("local_candidate")) return;
        string root = System.IO.Path.GetDirectoryName(OS.GetExecutablePath())!;
        void Input(string name, string relative) => System.Environment.SetEnvironmentVariable(name,
            System.IO.Path.Combine(root, "content", relative));
        Input("SF2_PRIVATE_EXPLORATION_CONTENT", "world.json");
        Input("SF2_PRIVATE_BATTLE_SCENE_CONTENT", "main-scenes.json");
        Input("SF2_PRIVATE_ENTRY_CONTENT", "entry/entry.json");
        Input("SF2_ENTRY_DISPLAY_CATALOG", "entry/entry.zh-CN.json");
        Input("SF2_DISPLAY_CATALOG", "world.zh-CN.json");
        Input("SF2_PRIVATE_OPENING_CONTENT", "opening/opening.json");
        foreach (string name in new[] { "SF2_PRIVATE_BATTLE01_DATA", "SF2_PRIVATE_BATTLE01_SCENE",
            "SF2_PRIVATE_BATTLE01_TERRAIN", "SF2_PRIVATE_STATIC_DATA", "SF2_PRIVATE_ENEMY_DATA", "SF2_PRIVATE_ENEMY_GOLD" })
            Input(name, "opening-inputs/" + name + ".json");
        // Explicit isolated operator namespaces remain supported by the existing store.
        if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("SF2_SAVE_ROOT")))
            System.Environment.SetEnvironmentVariable("SF2_SAVE_ROOT", System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Sf2Remake", "saves"));
        if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("SF2_SAVE_NAMESPACE")))
            System.Environment.SetEnvironmentVariable("SF2_SAVE_NAMESPACE", "local-candidate");
    }
}
