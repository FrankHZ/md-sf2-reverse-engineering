using Godot;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Audio;

internal static class SessionAudioBuses
{
    internal const string Music = "Music";
    internal const string SystemEffects = "SystemEffects";
    internal const string BattleEffects = "BattleEffects";

    internal static void Apply(InputSettings settings)
    {
        SetVolume(Music, settings.MusicVolume);
        SetVolume(SystemEffects, settings.SystemEffectsVolume);
        SetVolume(BattleEffects, settings.BattleEffectsVolume);
    }

    private static void SetVolume(string bus, int percent)
    {
        int index = AudioServer.GetBusIndex(bus);
        if (percent > 0) AudioServer.SetBusVolumeDb(index, Mathf.LinearToDb(percent / 100f));
        AudioServer.SetBusMute(index, percent == 0);
    }
}
