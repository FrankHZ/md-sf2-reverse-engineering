using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.UI;

internal sealed partial class SettingsView
{
    // Child binding editors and Help own their inputs; only category pages offer Reset.
    private bool HasReset => _page is Page.Root or Page.Display or Page.Assist or Page.Controls or Page.Debug or Page.Audio or Page.Battle;
    private string ResetDescription => S(_page switch
    {
        Page.Root => "reset-all-detail", Page.Controls => "reset-controls-detail",
        Page.Battle => "reset-battle-detail", _ => "reset-page-detail",
    });

    private List<Choice> Choices()
    {
        var rows = PageChoices();
        if (HasReset)
            rows.Add(new("reset-page", S(_page == Page.Root ? "reset-all" : "reset-page",
                Input.CompactHint(GameAction.Spell)), ReadOnly, Footer: true));
        return rows;
    }

    private void ResetPage()
    {
        if (ReadOnly || !HasReset || _help is not null) return;
        var defaults = InputSettings.Load(null);
        switch (_page)
        {
            case Page.Root:
                _draft = defaults;
                _reset = true; // Explicit whole-settings recovery, still pending Apply.
                break;
            case Page.Display:
                _draft.Language = defaults.Language;
                _draft.GraphicsMode = defaults.GraphicsMode;
                _draft.TextMode = defaults.TextMode;
                _draft.CharactersPerSecond = defaults.CharactersPerSecond;
                break;
            case Page.Assist:
                _draft.RunInBackground = defaults.RunInBackground;
                _draft.ReducedFlash = defaults.ReducedFlash;
                _draft.ModernGuidance = defaults.ModernGuidance;
                _draft.AccelerationMultiplier = defaults.AccelerationMultiplier;
                break;
            case Page.Controls:
                _draft.ConfirmCancel = defaults.ConfirmCancel;
                _draft.Bindings.Clear();
                break;
            case Page.Audio:
                _draft.MusicVolume = defaults.MusicVolume;
                _draft.SystemEffectsVolume = defaults.SystemEffectsVolume;
                _draft.BattleEffectsVolume = defaults.BattleEffectsVolume;
                break;
            case Page.Debug:
                _draft.ShowDebugDiagnostics = defaults.ShowDebugDiagnostics;
                _draft.DebugAutoVictory = defaults.DebugAutoVictory;
                break;
            case Page.Battle:
                _draft.TextMode = defaults.TextMode;
                _draft.CharactersPerSecond = defaults.CharactersPerSecond;
                _draft.SuppressBattleHitCoinMessages = defaults.SuppressBattleHitCoinMessages;
                break;
        }
        _status = S("reset-staged") + "\n" + ResetDescription;
        ShowPage(Focused?.Id, _scroll.ScrollVertical);
    }
}
