using Godot;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Battles;

// Explicitly authored EGRESS approximation: text, a soft blue pulse/tone, then closure.
// No original audio or hardware timing claim. Only actual delivery completes a token.
internal sealed partial class BattleRetreatView : Control
{
    private ColorRect _veil = null!;
    private Label _text = null!;
    private AudioStreamPlayer _cue = null!;
    private GameSession _session = null!;
    private InputSettings _settings = null!;
    private BattleRetreatState? _state;
    private double _elapsed, _revealed;
    private bool _delivered, _suspended;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        _veil = new ColorRect { Name = "RetreatEffect", MouseFilter = MouseFilterEnum.Ignore };
        AddChild(_veil); _veil.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _text = new Label { Name = "RetreatText", HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center, MouseFilter = MouseFilterEnum.Ignore };
        AddChild(_text); _text.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _cue = new AudioStreamPlayer { Name = "AuthoredRetreatCue", VolumeDb = -18 };
        AddChild(_cue);
        const int rate = 22050, samples = 7718;
        var bytes = new byte[samples * 2];
        for (int i = 0; i < samples; i++)
        {
            double t = (double)i / rate, envelope = Math.Sin(Math.PI * i / samples);
            short value = (short)(12000 * envelope * Math.Sin(Math.Tau * (440 * t + 220 * t * t)));
            bytes[i * 2] = (byte)value; bytes[i * 2 + 1] = (byte)(value >> 8);
        }
        _cue.Stream = new AudioStreamWav { Format = AudioStreamWav.FormatEnum.Format16Bits, MixRate = rate, Data = bytes };
        Hide();
    }

    public override void _EnterTree()
    { GetWindow().FocusExited += Suspend; GetWindow().FocusEntered += Resume; }
    public override void _ExitTree()
    { GetWindow().FocusExited -= Suspend; GetWindow().FocusEntered -= Resume; }
    private void Suspend() { _suspended = true; if (_cue is not null) _cue.StreamPaused = true; }
    private void Resume() { _suspended = false; if (_cue is not null) _cue.StreamPaused = false; }
    internal void Configure(GameSession session, InputSettings settings) { _session = session; _settings = settings; }

    internal void Present()
    {
        var state = _session.Current.BattleRetreat;
        if (state is null) { Hide(); _state = null; _cue.Stop(); return; }
        Show();
        if (_state?.Token != state.Token)
        {
            _elapsed = _revealed = 0; _delivered = false;
            _text.Text = state.Text;
            _text.VisibleCharacters = _settings.TextMode == "instant" ? -1 : 0;
            if (state.Phase == BattleRetreatPhase.Effect) { _cue.Play(); _cue.StreamPaused = _suspended; }
        }
        _state = state;
        _text.Visible = state.Phase == BattleRetreatPhase.Text;
        _veil.Color = state.Phase == BattleRetreatPhase.Text ? new(0, 0, 0, 0.65f) : Colors.Transparent;
    }

    internal SessionCommand? Consume(double delta)
    {
        if (_state is not { } state || _suspended || _delivered || !IsVisibleInTree() || !CanProcess()) return null;
        _elapsed += delta;
        if (state.Phase == BattleRetreatPhase.Text)
        {
            if (state.Revealed) return null;
            if (_text.VisibleCharacters != -1)
            {
                _revealed += delta * _settings.CharactersPerSecond;
                _text.VisibleCharacters = Math.Min(_text.Text.Length, (int)_revealed);
                if (_text.VisibleCharacters < _text.Text.Length) return null;
            }
            _delivered = true; return LastDelivery = new CompleteTextReveal(state.Token);
        }
        if (state.Phase == BattleRetreatPhase.Effect)
        {
            float pulse = _settings.ReducedFlash ? 0.15f : (float)(Math.Sin(Math.PI * Math.Min(1, _elapsed / 0.45)) * 0.3);
            _veil.Color = new(0.3f, 0.55f, 0.8f, pulse);
            if (_elapsed < 0.45 || _cue.Playing) return null;
        }
        else if (state.Phase == BattleRetreatPhase.Close)
        {
            _veil.Color = new(0, 0, 0, (float)Math.Min(1, _elapsed / 0.2));
            if (_elapsed < 0.2) return null;
        }
        else return null;
        _delivered = true; return LastDelivery = new CompletePresentation(state.Token, state.CompletionKind);
    }

    internal SessionCommand? Acknowledge(GameAction action)
    {
        if (_suspended || !CanProcess() || _state is not { Phase: BattleRetreatPhase.Text } state || action != GameAction.Confirm) return null;
        if (!state.Revealed) { _text.VisibleCharacters = -1; return null; }
        return new Acknowledge(state.Token);
    }

    internal SessionCommand? LastDelivery { get; private set; }
    internal object Observe() => new { active = _state is not null, phase = _state?.Phase.ToString(),
        token = _state?.Token.Value, paidMp = _state?.PaidMp, revealed = _state?.Revealed,
        text = _text.Text, visibleCharacters = _text.VisibleCharacters, textVisible = _text.Visible,
        elapsed = _elapsed, delivered = _delivered, suspended = _suspended,
        effectAlpha = _veil.Color.A, cuePlaying = _cue.Playing, cue = "authored-egress-tone-v1" };
}
