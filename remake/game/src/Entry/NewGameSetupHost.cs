using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.GodotAdapter.Audio;
using Sf2.Remake.GodotAdapter.Entry;
using Sf2.Remake.GodotAdapter.Persistence;

namespace Sf2.Remake.GodotAdapter;

public sealed partial class GameRoot
{
    private NameEntryView? _nameEntry;
    private string _nameDraft = "";
    private int _newSlot = 1;
    private PreparedNewGame? _preparedNewGame;
    private SessionAudio? _preparedAudio;
    private uint? _newMainSeed, _newThinkingSeed;
    private bool _committing;

    private void BeginNewGame()
    {
        if (_input is null || _slots is not null || _nameEntry is not null) return;
        _nameDraft = ""; _newSlot = 1; _newMainSeed = _newThinkingSeed = null;
        OpenSavePanel(overEntry: true); ShowNewSlots();
    }

    private void ShowNewSlots()
    {
        ClearPreparedNewGame(); CloseName();
        _slots!.Show(); _slots.Cancel = CancelNewGame;
        try
        {
            var read = RestoreSource().Read();
            if (read is not ExplorationReadAccepted admitted)
            {
                if (_session is null) GameText.ClearWorld();
                NewGameError(read is ScenarioReadRejected rejected ? rejected.Failure.Code : "new-game-content-required"); return;
            }
            GameText.Bind(admitted.Definition, System.Environment.GetEnvironmentVariable("SF2_DISPLAY_CATALOG"));
            var store = Store(admitted.Definition);
            _slots.ShowChoices("new-game-slots", GameText.Get("new.slots"),
                (Label(1, store.Read(1), admitted.Definition), () => ChooseNewSlot(1)), (Label(2, store.Read(2), admitted.Definition), () => ChooseNewSlot(2)), (GameText.Get("entry.cancel"), CancelNewGame));
        }
        catch (ArgumentException error) { NewGameError(error.Message); }
        DisarmInput();
    }

    private void ChooseNewSlot(int slot) { _newSlot = slot; ShowName(); }
    private void ShowName()
    {
        ClearPreparedNewGame(); _slots!.Hide(); CloseName();
        _nameEntry = new NameEntryView { Name = "NameEntryView", Draft = _nameDraft,
            Done = name => { _nameDraft = name; CloseName(); ConfirmNewGame(); },
            Back = name => { _nameDraft = name; ShowNewSlots(); } };
        _hostUi.AddChild(_nameEntry); ApplyHostPause(); DisarmInput();
    }
    private void CloseName()
    {
        if (_nameEntry is not { } view) return;
        _nameEntry = null; _hostUi.RemoveChild(view); view.QueueFree();
    }

    private void ConfirmNewGame()
    {
        _slots!.Show(); _slots.Cancel = ShowName;
        _slots.ShowChoices("confirm-new-game", GameText.Get("new.confirm", _nameDraft.Length == 0 ? GameText.Get("name.default-value", DisplayNames.DefaultMember(0)) : _nameDraft, _newSlot),
            (GameText.Get("entry.back"), ShowName), (GameText.Get("new.start", _newSlot), CommitNewGame));
        DisarmInput();
    }

    private void CommitNewGame()
    {
        if (_committing || !EnsureWindowSupport()) return;
        _committing = true;
        try
        {
            if (_preparedNewGame is null)
            {
                if (_newMainSeed is null)
                {
                    Span<byte> seeds = stackalloc byte[8]; System.Security.Cryptography.RandomNumberGenerator.Fill(seeds);
                    _newMainSeed = BitConverter.ToUInt32(seeds[..4]); _newThinkingSeed = BitConverter.ToUInt32(seeds[4..]);
                }
                _preparedNewGame = GameSession.PrepareNewGame(new OpeningNewGameReader(Selected("SF2_PRIVATE_EXPLORATION_CONTENT"),
                    Selected("SF2_PRIVATE_BATTLE01_DATA"), Selected("SF2_PRIVATE_BATTLE01_SCENE"), Selected("SF2_PRIVATE_BATTLE01_TERRAIN"),
                    Selected("SF2_PRIVATE_STATIC_DATA"), Selected("SF2_PRIVATE_ENEMY_DATA"), Selected("SF2_PRIVATE_ENEMY_GOLD"),
                    _newMainSeed.Value, _newThinkingSeed!.Value, System.Environment.GetEnvironmentVariable("SF2_PRIVATE_BATTLE_SCENE_CONTENT")),
                    new(_nameDraft), RuleCompositions.ForGame());
                if (_preparedNewGame.Failure is { } failure)
                { _preparedNewGame = null; NewGameError(failure.Code); return; }
                _preparedAudio = new SessionAudio(this, _preparedNewGame.Started!.Session.Definition.Exploration!);
                if (_preparedAudio.Error is { } audioError) { ClearPreparedNewGame(); NewGameError(audioError); return; }
            }
            var prepared = _preparedNewGame;
            var store = Store(prepared.Started!.Session.Definition);
            var written = store.Write(_newSlot, prepared.Save!);
            if (!written.Success)
            {
                if (written.Error == "save-publication-uncertain")
                {
                    ClearPreparedNewGame();
                    _slots!.Cancel = CancelNewGame;
                    _slots.ShowChoices("new-game-publication-uncertain", "The write may already have completed. Your current session is still paused.\n" +
                        "Progress files have been retained. Inspect Continue before deciding whether to start another journey.",
                        (GameText.Get("entry.inspect"), () => { CancelNewGame(); ShowContinue(); }), (GameText.Get("entry.cancel"), CancelNewGame));
                }
                else NewGameError(written.Error!);
                return;
            }
            var audio = _preparedAudio; _preparedAudio = null; _preparedNewGame = null;
            MountSession(prepared.Started, audio);
            _savedSession = _session!.Current.SessionId; _savedRevision = _session.Current.Revision;
        }
        catch (ArgumentException error) { NewGameError(error.Message); }
        finally { _committing = false; DisarmInput(); }
    }

    private void NewGameError(string error)
    {
        _slots!.Show(); _slots.Cancel = CancelNewGame;
        _slots.ShowChoices("new-game-error", GameText.Get("new.error", error),
            (GameText.Get("entry.back"), ShowNewSlots), (GameText.Get("entry.retry"), _newMainSeed is null ? ShowNewSlots : CommitNewGame), (GameText.Get("entry.cancel"), CancelNewGame));
        DisarmInput();
    }
    private void ClearPreparedNewGame() { _preparedAudio?.Dispose(); _preparedAudio = null; _preparedNewGame = null; }
    private void CancelNewGame()
    {
        ClearPreparedNewGame(); CloseName(); CloseSavePanel(); _entry?.ReturnFromSetup(); DisarmInput();
    }
}
