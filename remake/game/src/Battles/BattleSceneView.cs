using System.Text.RegularExpressions;
using Godot;
using Sf2.Remake.GodotAdapter.Localization;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.GodotAdapter.Audio;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Battles;

// One scene consumer. It projects admitted frames and acknowledges the session's
// wait token; it never computes damage, rewards, RNG or the next combatant.
internal sealed partial class BattleSceneView : Control
{
    private static readonly string[] BreathResources = ["kiwi-breath/tile-1", "kiwi-breath/tile-9"];
    private PrismLaserView _laser = null!;
    private AuraRecoveryView _aura = null!;
    private AttackSpellView _attackSpell = null!;
    private PhoenixWaveView _phoenixWave = null!;
    private DetoxEffectView _detox = null!;
    private DebuffEffectView _debuff = null!;
    private DesoulEffectView _desoul = null!;
    private bool PreparedDesoul(BattleSceneState state) => state.AttackAnimationSelector is 3 or 35 &&
        !state.IsSilencedCast && Content is { } content && DesoulEffectView.Resources.All(content.Rasters.ContainsKey);
    private readonly BuffEffectView _buff = new();
    private readonly AbsorbEffectView _absorb = new();
    private bool PreparedAbsorb(BattleSceneState state) => Content is not null && AbsorbEffectView.Selector(state) is not null &&
        _audio?.CanSelectSceneEffects(_session.Current.Battle.GetActor(state.Actor).IsAlly ? 2 : 5, 89) == true;
    private bool _suspended;
    private Node2D _canvas = null!;
    internal Control PresentationRoot { get; set; } = null!;
    private readonly Control _overlay = new() { Name = "SceneUi", MouseFilter = MouseFilterEnum.Ignore };
    private void SyncOverlay() => _overlay.Visible = IsVisibleInTree();
    private ColorRect _sceneBlack = null!;
    private Sprite2D _background = null!, _backgroundWrap = null!, _ground = null!, _ally = null!, _enemy = null!, _weapon = null!;
    private Label _message = null!;
    private string _messageFallback = "";
    private readonly StoryDialoguePanel _messagePanel = new() { Name = "MessagePanel" };
    private GameInput _input = null!;
    private readonly BattleStatusCard _actorCard = new() { Name = "ActorCard" }, _targetCard = new() { Name = "TargetCard" };
    private GameSession _session = null!;
    private InputSettings _settings = null!;
    private SessionAudio? _audio;
    private BattleSceneState? _state;
    private BattleSceneActorVisual? _allyVisual, _enemyVisual;
    private BattleSceneAnimationFrame[] _frames = [];
    private double _elapsed, _revealed;
    private int _frameIndex, _allyFrame, _enemyFrame, _logicalReaction;
    private bool _completed;
    private int? _animationIndex;
    private bool _sceneMusicStarted, _hasWeapon;
    private string? _allyResource, _enemyResource, _weaponResource;
    private readonly List<Sprite2D> _fairyBodies = [], _fairyWings = [], _fairyDust = [];
    private bool _castSoundPlayed;
    private bool _blastSoundPlayed;
    private bool _preparedAttackSpell;
    private double _attackSpellStart;
    internal string? Error { get; private set; }
    private BattleSceneDefinition? Content => _session.ActiveBattleScenes;
    // DEBUFF1 variation 2 still uses the same setup/update and palette. Equipment
    // selects its Use spell effect, but retains the item gesture below.
    private static int? DebuffSelector(BattleSceneState? state) => state is { IsSilencedCast: false, SupportSpell.Spell: var spell }
        ? (spell.Value, spell.Level) switch
        {
            ("slow", 2) => 41,
            ("slow" or "muddle", 1) or ("muddle", 2) or ("sleep" or "dispel", 1) => 9,
            _ => null,
        } : null;
    private bool PreparedDebuff(BattleSceneState state) =>
        DebuffSelector(state) is not null &&
        Content is { } content && DebuffEffectView.Resources.All(content.Rasters.ContainsKey);

    private bool PreparedBuff(BattleSceneState state) => Content is not null && BuffEffectView.Selector(state) is not null &&
        _audio?.CanSelectSceneEffects(_session.Current.Battle.GetActor(state.Actor).IsAlly ? 2 : 5, 77, 97) == true;

    private bool UsesMessagePlayback(BattleSceneState state) =>
        state.SupportSpell is not null && state.EquipmentItem is null && !PreparedDebuff(state) && !PreparedBuff(state) || state.IsSilencedCast ||
        state.AttackSpell?.IsInstantDeath == true && !PreparedDesoul(state) || state.MuddleActionMessage is not null;

    private bool Suppressed(BattleSceneState state) => _settings.SuppressBattleHitCoinMessages && state.CanSuppressHitCoinMessage;
    internal bool WaitingForInput => _state is { RequiresAcknowledgement: true } state && !Suppressed(state) &&
        state.Healing is not { LogicalComplete: false, AtTimedInput: false };

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        ClipContents = true;
        _sceneBlack = new ColorRect { Color = Colors.Black, MouseFilter = MouseFilterEnum.Ignore };
        _sceneBlack.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(_sceneBlack);
        _canvas = new Node2D { Name = "SceneCanvas", TextureFilter = TextureFilterEnum.Nearest }; AddChild(_canvas);
        _background = Sprite("Background", new(0, 56));
        _backgroundWrap = Sprite("BackgroundWrap", new(256, 56));
        _enemy = Sprite("Enemy", new(16, 48));
        _ground = Sprite("Ground", new(136, 140));
        _ally = Sprite("Ally", new(136, 64)); _ally.ZIndex = 2;
        _weapon = Sprite("Weapon", new(136, 64));
        _desoul = new DesoulEffectView { Name = "DesoulEffect", ZIndex = 4 }; _canvas.AddChild(_desoul);
        _desoul.ConfigureBackdrop(_background, _backgroundWrap);
        CreateSummons();
        _debuff = new DebuffEffectView { Name = "DebuffEffect" }; _canvas.AddChild(_debuff);
        _detox = new DetoxEffectView { Name = "DetoxEffect" }; _canvas.AddChild(_detox);
        _canvas.MoveChild(_detox, _enemy.GetIndex());
        PresentationRoot.AddChild(_overlay);
        _overlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _overlay.AddChild(_actorCard); _overlay.AddChild(_targetCard);
        _overlay.AddChild(_messagePanel); _message = _messagePanel.Body; _message.AutoTranslateMode = AutoTranslateModeEnum.Disabled; _messagePanel.Choices.Hide();
        for (int i = 0; i < 3; i++)
        {
            _fairyBodies.Add(Sprite("FairyBody" + i, Vector2.Zero));
            _fairyWings.Add(Sprite("FairyWings" + i, Vector2.Zero));
        }
        for (int i = 0; i < 23; i++) _fairyDust.Add(Sprite("FairyDust" + i, Vector2.Zero));
        foreach (var sprite in _fairyBodies.Concat(_fairyWings).Concat(_fairyDust)) { sprite.ZIndex = 3; sprite.Hide(); }
        _aura = new AuraRecoveryView { Name = "AuraRecovery", ZIndex = 4 }; _canvas.AddChild(_aura); _aura.Hide();
        _attackSpell = new AttackSpellView { Name = "AttackSpell", ZIndex = 4 }; _canvas.AddChild(_attackSpell); _attackSpell.Hide();
        _phoenixWave = new PhoenixWaveView { Name = "PhoenixWave", ZIndex = 4 }; _canvas.AddChild(_phoenixWave);
        _laser = new PrismLaserView { Name = "PrismLaser", ZIndex = 4 }; _canvas.AddChild(_laser); _laser.Hide();
        Hide();
    }

    public override void _EnterTree()
    { GetWindow().FocusExited += Suspend; GetWindow().FocusEntered += Resume;
        VisibilityChanged += SyncOverlay; SyncOverlay(); }
    private void Suspend() => _suspended = true;
    private void Resume() => _suspended = false;

    internal void Configure(GameSession session, GameInput input, SessionAudio? audio)
    { _session = session; _input = input; _settings = input.Settings; _audio = audio; }

    internal void Present()
    {
        var state = _session.Current.BattleScene;
        ArrangeCanvas();
        if (state is null)
        {
            _messagePanel.Present(false, false, false, default, "", false);
            Hide(); _laser.Hide(); _aura.Hide(); ResetRasterEffects();
            if (_detox.Active) _detox.Reset();
            if (_debuff.Active) _debuff.Reset();
            _desoul.Reset(); ResetSummons();
            _buff.Reset(); _absorb.Reset(); _sceneBlack.Color = Colors.Black;
            RetireRasters(); _state = null;
            return;
        }
        if (state.IsFieldDeath) Hide(); else Show();
        PresentStatus(state);
        if (_state?.Token != state.Token) _messagePanel.ResetReading();
        if (_state?.Token == state.Token) { _state = state; PresentMessage(state); ProjectHealing(state); return; }
        _state = state; _elapsed = _revealed = 0; _frameIndex = _logicalReaction = -1; _completed = _sceneMusicStarted = false; _frames = []; _animationIndex = null;
        try
        {
            _laser.Hide(); _aura.Hide(); ResetRasterEffects();
            if (_detox.Active && state.Phase is BattleScenePhase.ActorExit or BattleScenePhase.Reward or BattleScenePhase.End)
                _detox.Reset();
            if (_detox.Active && state.Phase == BattleScenePhase.SpellStop) _detox.Stop();
            _sceneBlack.Color = Colors.Black;
            if (state.Phase != BattleScenePhase.ActionAnimation) _buff.Reset();
            if (state.Phase != BattleScenePhase.ActionAnimation) _absorb.Reset();
            if (_debuff.Active && state.Phase is BattleScenePhase.Reward or BattleScenePhase.End) _debuff.Reset();
            var preparedResources = state.AttackAnimationSelector switch
            {
                1 => AttackSpellView.BlazeResources,
                33 => BlazeMeteorEffect.SmallResources,
                65 => BlazeMeteorEffect.LargeResources,
                97 => BlazeMeteorEffect.DragonResources,
                7 or 39 or 71 or 103 => BoltEffect.Resources,
                2 or 34 or 66 or 98 => FreezeEffect.Resources,
                5 or 37 or 69 or 101 => BlastEffect.ResourcesFor((state.AttackAnimationSelector.Value >> 5) + 1),
                _ => null,
            };
            _preparedAttackSpell = preparedResources is not null &&
                Content is { } preparedContent && preparedResources.All(preparedContent.Rasters.ContainsKey);
            if (UsesMessagePlayback(state))
            {
                foreach (var node in new CanvasItem[] { _background, _backgroundWrap, _ground, _ally, _enemy, _weapon }) node.Hide();
                foreach (var sprite in _fairyBodies.Concat(_fairyWings).Concat(_fairyDust)) sprite.Hide();
                _sceneBlack.Show(); PresentMessage(state);
                _message.VisibleCharacters = _settings.TextMode == "instant" || !state.RequiresAcknowledgement ? -1 : 0;
                return;
            }
            if (state.AttackSpell is not null && state.AttackItem is null && !state.IsMonsterBreath && !state.IsFieldDeath && !_preparedAttackSpell && !PreparedDesoul(state) && !PreparedSummon(state))
            {
                foreach (var node in new CanvasItem[] { _background, _backgroundWrap, _ground, _ally, _enemy, _weapon }) node.Hide();
                foreach (var sprite in _fairyBodies.Concat(_fairyWings).Concat(_fairyDust)) sprite.Hide();
                _sceneBlack.Show(); _attackSpell.Present(state);
                PresentMessage(state);
                _message.VisibleCharacters = _settings.TextMode == "instant" || !state.RequiresAcknowledgement ? -1 : 0;
                return;
            }
            if (state.Spell?.IsAura == true && state.Healing is null)
            {
                if (Content is not null) throw new InvalidOperationException("aura-private-presentation-unsupported");
                foreach (var node in new CanvasItem[] { _background, _backgroundWrap, _ground, _ally, _enemy, _weapon }) node.Hide();
                foreach (var sprite in _fairyBodies.Concat(_fairyWings).Concat(_fairyDust)) sprite.Hide();
                _sceneBlack.Show(); _aura.Present(state, _session.Current.Battle);
                PresentMessage(state);
                _message.VisibleCharacters = _settings.TextMode == "instant" || !state.RequiresAcknowledgement ? -1 : 0;
                if (state.Phase == BattleScenePhase.Reaction) _audio?.PlayEffect(113);
                return;
            }
            if (state.Laser is not null && !state.IsFieldDeath)
            {
                if (Content is not null) throw new InvalidOperationException("prism-private-presentation-unsupported");
                foreach (var node in new CanvasItem[] { _background, _backgroundWrap, _ground, _ally, _enemy, _weapon }) node.Hide();
                foreach (var sprite in _fairyBodies.Concat(_fairyWings).Concat(_fairyDust)) sprite.Hide();
                _sceneBlack.Show(); _laser.Present(state, _session.Current.Battle);
                PresentMessage(state);
                _message.VisibleCharacters = _settings.TextMode == "instant" || !state.RequiresAcknowledgement ? -1 : 0;
                return;
            }
            if (state.IsFieldDeath)
            {
                Hide();
                if (_session.Definition.PrivateDefinitions is not null && Content?.FieldDeath is null)
                    throw new InvalidOperationException("field-death-content-required");
                if (state.Phase == BattleScenePhase.FieldExit && state.FieldStep == 0) _audio?.PlayEffect(116);
                return;
            }
            if (_session.Definition.PrivateDefinitions is not null && Content is null)
                throw new InvalidOperationException("battle-scene-content-required");
            _sceneBlack.Visible = !state.IsAfterTurnNotice;
            if (state.IsAfterTurnNotice)
            {
                foreach (var node in new CanvasItem[] { _background, _backgroundWrap, _ground, _ally, _enemy, _weapon }) node.Hide();
                foreach (var sprite in _fairyBodies.Concat(_fairyWings).Concat(_fairyDust)) sprite.Hide();
                PresentMessage(state); _message.VisibleCharacters = -1;
                return;
            }
            _background.Show(); _backgroundWrap.Show(); _ground.Show(); _ally.Show();
            var battle = _session.Current.Battle;
            var actor = battle.GetActor(state.Actor); var target = battle.GetActor(state.Target!.Value);
            var ally = state.DisplayedAlly is { } allyRef ? battle.GetActor(allyRef) : null;
            var enemy = state.DisplayedEnemy is { } enemyRef ? battle.GetActor(enemyRef) : null;
            _ally.Visible = ally is not null;
            _enemy.Visible = enemy is not null;
            _enemyVisual = null; _enemyResource = null; _weaponResource = null;
            if (Content is { } content)
            {
                _allyVisual = null;
                if (content.Encounter != battle.Definition.Encounter || (ally is not null && !content.TryAllyVisual(ally, out _allyVisual)) ||
                    (enemy is not null && !content.Enemies.TryGetValue(enemy.Actor, out _enemyVisual))) throw new InvalidOperationException("battle-scene-actor-unavailable");
                string background = content.Background, ground = content.Ground;
                if (content.TerrainScenes is { } terrain)
                {
                    var back = enemy?.Position ?? ally!.Position ?? ally.Deployment.Position;
                    var front = ally?.Position ?? back;
                    background = terrain[back.Y * 48 + back.X].Background;
                    ground = terrain[front.Y * 48 + front.X].Ground;
                }
                Bind(_background, background); Bind(_backgroundWrap, background); Bind(_ground, ground);
                _allyFrame = _enemyFrame = 0;
                byte? weapon = ally is null ? null : BattleEquipment.EquippedWeaponItem(ally);
                _hasWeapon = weapon.HasValue;
                if (_hasWeapon && weapon != _allyVisual!.Item) throw new InvalidOperationException("battle-scene-weapon-unavailable");
                if (ally is not null) SetFrame(true, 0); if (enemy is not null) SetFrame(false, 0);
                Weapon(_allyVisual?.Sequences["idle"].IdleWeapon, Vector2.Zero);
                _ground.Visible = ally is not null && ally.Definition.Mover is not (BattleMover.Flying or BattleMover.Hovering);
                if (state.Phase == BattleScenePhase.ActionAnimation)
                {
                    var sequences = (actor.IsAlly ? _allyVisual! : _enemyVisual!).Sequences;
                    string sequence = state.Spell is not null || state.IsMonsterBreath || (_preparedAttackSpell || PreparedDesoul(state) || PreparedSummon(state)) && state.AttackItem is null || (PreparedDebuff(state) || PreparedBuff(state)) && state.EquipmentItem is null ? "cast" : state.Item is not null || state.AttackItem is not null || state.EquipmentItem is not null ? "item" : "attack";
                    if (sequence == "attack" && state.SpecialPhysicalAnimation) sequence = "special";
                    // battlesceneScript_PerformAnimation selects the ordinary sequence for these
                    // casters. Earlier packages expose it as ally idle or enemy attack.
                    if ((_preparedAttackSpell || PreparedDesoul(state) || PreparedSummon(state) || PreparedDebuff(state) || PreparedBuff(state)) && sequence == "cast" && !sequences.ContainsKey(sequence)) sequence = actor.IsAlly ? "idle" : "attack";
                    var animation = sequences[sequence];
                    _animationIndex = animation.Index;
                    int skippedFrames = actor.IsAlly ? 1 : 0;
                    _frames = animation.Frames.Skip(skippedFrames).ToArray();
                    if (_preparedAttackSpell)
                    {
                        _attackSpellStart = _frames.Take(Math.Max(0, animation.Trigger - 1 - skippedFrames)).Sum(frame => frame.Ticks) / 60.0;
                        _attackSpell.PrepareAttackSpell(state, Frames(preparedResources!), target.IsAlly, _settings.ReducedFlash);
                    }
                    BeginSummon(state, _frames.Take(Math.Max(0, animation.Trigger - 1 - skippedFrames)).Sum(frame => frame.Ticks) / 60.0);
                    if (PreparedDesoul(state))
                        _desoul.Begin(Frames(DesoulEffectView.Resources),
                            _frames.Take(Math.Max(0, animation.Trigger - 1 - skippedFrames)).Sum(frame => frame.Ticks) / 60.0,
                            (state.AttackAnimationSelector!.Value >> 5) + 1, target.IsAlly, _settings.ReducedFlash);
                    if (PreparedBuff(state))
                        _buff.Begin(_frames.Take(Math.Max(0, animation.Trigger - 1 - skippedFrames)).Sum(frame => frame.Ticks));
                    if (PreparedAbsorb(state))
                        _absorb.Begin(_frames.Take(Math.Max(0, animation.Trigger - 1 - skippedFrames)).Sum(frame => frame.Ticks));
                    if (PreparedDebuff(state))
                    {
                        int trigger = _frames.Take(Math.Max(0, animation.Trigger - 1 - skippedFrames)).Sum(frame => frame.Ticks);
                        _debuff.Begin(Frames(DebuffEffectView.Resources), trigger, target.IsAlly);
                    }
                    if (DetoxSelector(state) is not null && DetoxEffectView.Resources.All(content.Rasters.ContainsKey))
                    {
                        int trigger = _frames.Take(Math.Max(0, animation.Trigger - 1 - skippedFrames)).Sum(frame => frame.Ticks);
                        _detox.Begin(Frames(DetoxEffectView.Resources), trigger);
                    }
                    if (state.IsMonsterBreath)
                        _attackSpell.Present(state, Frames(BreathResources));
                    if (_frames.Length != 0 && sequence == "attack" && animation.Spell == PhoenixWaveView.Selector &&
                        PhoenixWaveView.Resources.All(Content!.Rasters.ContainsKey))
                        _phoenixWave.Begin(Frames(PhoenixWaveView.Resources), actor.IsAlly);
                }
                if (state.Phase == BattleScenePhase.Reaction && state.ReactionKind == "Dodge")
                    _frames = (target.IsAlly ? _allyVisual! : _enemyVisual!).Sequences["dodge"].Frames.Skip(target.IsAlly ? 1 : 0).ToArray();
            }
            _desoul.Present(state); PresentSummons(state);
            _ally.Position = new(136, 64); _enemy.Position = new(16, 48);
            _background.Position = new(0,56); _backgroundWrap.Position = new(256,56); _ground.Position = new(136,140);
            _ally.Modulate = _enemy.Modulate = Colors.White;
            ProjectSummons();
            PresentMessage(state);
            _message.VisibleCharacters = _settings.TextMode == "instant" || !state.RequiresAcknowledgement ? -1 : 0;
            if (state.Phase == BattleScenePhase.Initialize) { _castSoundPlayed = false; _blastSoundPlayed = false; _audio?.BeginBattleScene(actor.IsAlly ? 2 : 5); }
            if (state.Phase == BattleScenePhase.Reaction && (state.ReactionKind == "Damage" || state.IsCurseRecoil)) _audio?.PlayEffect(target.IsAlly ? 81 : 83);
            if (state.Phase == BattleScenePhase.Reaction &&
                (state.ReactionKind == "Recovery" || _detox.Active && state.ReactionKind == "Status")) _audio?.PlayEffect(113);
            if (state.GrowthNotice is { Kind: BattleGrowthNoticeKind.Level }) _audio?.PlayEffect(102);
            if (state.Phase == BattleScenePhase.End) _audio?.BeginSceneEnd();
            ProjectHealing(state);
        }
        catch (InvalidOperationException error) { Error = error.Message; }
    }

    internal void RefreshGeometry()
    {
        if (_state is not { IsFieldDeath: false } state) return;
        ArrangeCanvas();
        _messagePanel.Arrange();
        ArrangeStatus(state);
    }

    private void ResetRasterEffects()
    {
        _attackSpell.Reset();
        _phoenixWave.Reset();
        _effectBindings.Clear();
    }

    private void ArrangeCanvas()
    {
        var viewport = GetViewportRect().Size;
        float scale = MathF.Min(viewport.X / 256, viewport.Y / 224);
        _canvas.Scale = new(scale, scale); _canvas.Position = (viewport - new Vector2(256, 224) * scale) / 2;
    }

    internal SessionCommand? Read(double delta)
    {
        if (_state is not { RequiresAcknowledgement: true } state || Error is not null || _completed ||
            _suspended || !IsVisibleInTree() || !CanProcess() || Suppressed(state)) return null;
        RefreshMessageHint(state);
        if (_detox.Active) _detox.Advance(delta);
        if (_debuff.Active) _debuff.Advance(delta);
        if (_desoul.Active) _desoul.Advance(delta, cue => _audio?.PlayEffect(cue));
        AdvanceSummons(delta);
        _revealed += delta * _settings.CharactersPerSecond;
        if (_message.VisibleCharacters >= 0) _message.VisibleCharacters = (int)_revealed;
        if (state.IsHealingMessage && state.Healing is { Delivered: false } message &&
            (message.AtTimedInput || message.LogicalComplete) &&
            (_message.VisibleCharacters < 0 || _message.VisibleCharacters >= _message.GetTotalCharacterCount()) && !_messagePanel.MoreBelow)
            return new CompletePresentation(state.Token, state.CompletionKind);
        return null;
    }

    internal SessionCommand? Consume(double delta)
    {
        if (_state is not { } state || Error is not null || _completed || _suspended ||
            (!IsVisibleInTree() && !state.IsFieldDeath) || !CanProcess()) return null;
        if (state.IsFieldDeath)
        {
            _elapsed += delta;
            // Modern delivery speed never adds RNG or selects a new death list.
            double fieldDuration = state.FieldDelay / (_settings.ReducedFlash ? 120.0 : 60.0);
            if (_elapsed < fieldDuration) return null;
            _completed = true;
            return new CompletePresentation(state.Token, state.CompletionKind);
        }
        RefreshMessageHint(state);
        _elapsed += delta;
        if (_desoul.Active && !state.RequiresAcknowledgement) _desoul.Advance(delta, cue => _audio?.PlayEffect(cue));
        bool desoulCast = _desoul.Active && state.Phase == BattleScenePhase.ActionAnimation;
        if (desoulCast)
        {
            var caster = _session.Current.Battle.GetActor(state.Actor).IsAlly ? _ally : _enemy;
            caster.Modulate = !_settings.ReducedFlash && _desoul.Flash ? new Color(1f/7, 1f/7, 1f/7) : Colors.White;
        }
        bool buffCast = _buff.Active && state.Phase == BattleScenePhase.ActionAnimation;
        bool absorbCast = _absorb.Active && state.Phase == BattleScenePhase.ActionAnimation;
        if (absorbCast)
        {
            _absorb.Advance(delta, cue => _audio?.PlayEffect(cue));
            _sceneBlack.Color = !_settings.ReducedFlash && _absorb.Flash ? Colors.Red : Colors.Black;
        }
        if (buffCast)
        {
            _buff.Advance(delta, cue => _audio?.PlayEffect(cue));
            _sceneBlack.Color = !_settings.ReducedFlash && _buff.Flash ? new Color(2f/14, 12f/14, 2f/14) : Colors.Black;
        }
        bool debuffCast = _debuff.Active && state.Phase == BattleScenePhase.ActionAnimation;
        if (_debuff.Active && !state.RequiresAcknowledgement && _debuff.Advance(delta)) _audio?.PlayEffect(77);
        if (debuffCast)
        {
            var caster = _session.Current.Battle.GetActor(state.Actor).IsAlly ? _ally : _enemy;
            caster.Modulate = !_settings.ReducedFlash && _debuff.Flash ? new Color(0.14f, 0.14f, 1) : Colors.White;
        }
        bool detoxCast = _detox.Active && state.Phase == BattleScenePhase.ActionAnimation;
        if (_detox.Active && !state.RequiresAcknowledgement && _detox.Advance(delta)) _audio?.PlayEffect(77);
        if (detoxCast)
        {
            var caster = state.DisplayedAlly is not null ? _ally : _enemy;
            caster.Modulate = !_settings.ReducedFlash && _detox.Flash ? new Color(1, 0.82f, 1) : Colors.White;
        }
        if (Suppressed(state))
        {
            _completed = true;
            return new Acknowledge(state.Token);
        }
        if (state.IsAfterTurnNotice || state.Phase is BattleScenePhase.SleepMessage or BattleScenePhase.SilenceMessage
            or BattleScenePhase.PoisonMessage or BattleScenePhase.StunMessage)
        {
            // Modern finite delivery of the source's no-input status notices. Actual
            // completion releases the status command; reveal/input speed adds no RNG.
            if (_elapsed < 0.5) return null;
            _completed = true;
            return new CompletePresentation(state.Token, state.CompletionKind);
        }
        if (state.RequiresAcknowledgement)
        {
            return state.Healing is { LogicalComplete: false, AtTimedInput: false } ? new AdvanceSimulation(state.Token) : null;
        }
        if (UsesMessagePlayback(state))
        {
            if (_elapsed < 0.25) return null;
            _completed = true; return new CompletePresentation(state.Token, state.CompletionKind);
        }
        if (state.AttackSpell is not null && state.AttackItem is null && !state.IsMonsterBreath && !_preparedAttackSpell && !PreparedDesoul(state) && !PreparedSummon(state))
        {
            if (!_attackSpell.Advance(_elapsed)) return null;
            _completed = true;
            return new CompletePresentation(state.Token, state.CompletionKind);
        }
        if (state.Spell?.IsAura == true && state.Healing is null)
        {
            if (!_aura.Advance(_elapsed)) return null;
            _completed = true;
            return new CompletePresentation(state.Token, state.CompletionKind);
        }
        if (state.Laser is not null)
        {
            if (!_laser.Advance(_elapsed)) return null;
            _completed = true;
            return new CompletePresentation(state.Token, state.CompletionKind);
        }
        if (state.Healing is { } healing && state.Phase is not (BattleScenePhase.Initialize or BattleScenePhase.End))
        {
            if (!healing.LogicalComplete) return new AdvanceSimulation(state.Token);
            _completed = true;
            return new CompletePresentation(state.Token, state.CompletionKind);
        }
        double duration;
        if (state.Phase == BattleScenePhase.Initialize)
        {
            duration = 0.5 + 22.0 / 60;
            if (_elapsed < 0.5) _audio?.FadeOut(_elapsed / 0.5);
            else if (!_sceneMusicStarted)
            { _audio?.FadeOut(1); _audio?.StartBattleSceneMusic(); _sceneMusicStarted = true; }
            int tick = Math.Clamp((int)((_elapsed - 0.5) * 60), 0, 22);
            // InitializeBattlescene's 22 entrance iterations, after its fade/music cue.
            _background.Position = new(-44 + tick*2,56); _backgroundWrap.Position = new(212 + tick*2,56);
            _enemy.Position = new(-6 + tick,48); _ally.Position = new(158-tick,64); _ground.Position = new(158-tick,140);
            if (_allyVisual is not null) Weapon(_allyVisual.Sequences["idle"].IdleWeapon, new(22-tick,0));
        }
        else if (state.Phase == BattleScenePhase.End)
        {
            // Existing modern fade service: a bounded consumer, not original 253 timing.
            duration = 0.5;
            _audio?.FadeOut(_elapsed / duration);
        }
        else if (PreparedDebuff(state) && state.Phase == BattleScenePhase.TargetEnter) duration = 1.0 / 60;
        else if (!state.IsCurseRecoil && state.Phase is (BattleScenePhase.TargetExit or BattleScenePhase.TargetEnter or BattleScenePhase.ActorExit or BattleScenePhase.ActorEnter))
        {
            // bscWait $1E then bsc07/SwitchAllyBattlesprite: source coordinates
            // minus the VDP 128 bias, steps capped at 16. Presentation only;
            // these elapsed intervals do not invent shared-RNG opportunities.
            bool exiting = state.Phase is BattleScenePhase.TargetExit or BattleScenePhase.ActorExit;
            float from = state.Phase == BattleScenePhase.TargetEnter ? -376 : state.Phase == BattleScenePhase.ActorEnter ? 8 : 136;
            float to = state.Phase == BattleScenePhase.TargetExit ? 272 : state.Phase == BattleScenePhase.ActorExit ? -112 : 136;
            double wait = exiting ? 30.0 / 60 : 0;
            int steps = (int)MathF.Ceiling(MathF.Abs(to-from) / 16);
            duration = wait + steps / 60.0;
            float traveled = Math.Clamp((int)((_elapsed-wait)*60), 0, steps) * 16;
            float x = from + MathF.Sign(to-from) * MathF.Min(traveled, MathF.Abs(to-from));
            _ally.Position = new(x,64); _ground.Position = new(x,140);
            Weapon(_allyVisual?.Sequences["idle"].IdleWeapon, new(x-136,0));
        }
        else if (_frames.Length > 0)
        {
            duration = _frames.Sum(frame => frame.Ticks) / 60.0;
            double time = (absorbCast ? _absorb.CastElapsed(_elapsed) : detoxCast ? _detox.CastElapsed(_elapsed) : debuffCast ? _debuff.CastElapsed(_elapsed) : buffCast ? _buff.CastElapsed(_elapsed) : _elapsed) * 60;
            int index = 0;
            while (index < _frames.Length-1 && time >= _frames[index].Ticks) time -= _frames[index++].Ticks;
            if (index != _frameIndex)
            {
                _frameIndex = index;
                var frame = _frames[index];
                bool ally = _session.Current.Battle.GetActor(state.Phase == BattleScenePhase.ActionAnimation ? state.Actor : state.Target!.Value).IsAlly;
                if (frame.Frame != 15) SetFrame(ally, frame.Frame);
                var sprite = ally ? _ally : _enemy;
                sprite.Position = (ally ? new Vector2(136,64) : new Vector2(16,48)) + new Vector2(frame.X,frame.Y);
                if (ally) Weapon(frame.Weapon, new(frame.X,frame.Y));
            }
        }
        else if (state.Phase == BattleScenePhase.Reaction && state.Motion.Count > 0)
        {
            duration = state.Motion.Count / 60.0;
            _logicalReaction = Math.Min((int)(_elapsed * 60), state.Motion.Count - 1);
            bool ally = _session.Current.Battle.GetActor(state.Target!.Value).IsAlly;
            var node = ally ? _ally : _enemy;
            var offset = state.Motion[_logicalReaction];
            var shift = _settings.ReducedFlash ? Vector2.Zero : new Vector2(offset.X, -offset.Y);
            _enemy.Position = new Vector2(16,48) + shift;
            if (ally)
            {
                _ally.Position = new Vector2(136,64) + shift; _ground.Position = new Vector2(136,140) + shift;
                _background.Position = new Vector2(0,56) + shift; _backgroundWrap.Position = new Vector2(256,56) + shift;
                Weapon(_allyVisual?.Sequences["idle"].IdleWeapon, shift);
            }
            node.Modulate = _settings.ReducedFlash || _logicalReaction % 2 == 0 ? Colors.White : new Color(0.2f,0.2f,0.2f,1);
        }
        else duration = 1.0 / 60;
        if (_preparedAttackSpell && state.Phase == BattleScenePhase.ActionAnimation)
        {
            _attackSpell.AdvancePrepared(_elapsed - _attackSpellStart);
            if (_attackSpell.PreparedSound() is { } effectSound) _audio?.PlayEffect(effectSound);
            duration = Math.Max(duration, _attackSpellStart + _attackSpell.PreparedDuration);
            if (!_blastSoundPlayed && state.AttackSpell?.Spell.Value == "blast" && _elapsed >= _attackSpellStart + 0.1)
            {
                _blastSoundPlayed = true;
                _audio?.PlayEffect(114);
            }
            if (!_castSoundPlayed && _elapsed >= _attackSpellStart)
            {
                _castSoundPlayed = true;
                _audio?.PlayEffect(77);
            }
        }
        if (state.IsMonsterBreath && state.Phase == BattleScenePhase.ActionAnimation)
        {
            duration = Math.Max(duration, 0.6);
            _attackSpell.Advance(_elapsed);
        }
        if (_phoenixWave.Active)
        {
            // The source trigger is on the last authored attack frame.
            double start = duration - _frames[^1].Ticks / 60.0;
            _phoenixWave.Advance(_elapsed - start);
            duration = Math.Max(duration, start + PhoenixWaveView.Duration);
        }
        if (!state.RequiresAcknowledgement) AdvanceSummons(delta);
        else ProjectSummons();
        if (state.Phase == BattleScenePhase.ActionAnimation) duration = SummonDuration(duration);
        if (desoulCast) duration = Math.Max(duration, _desoul.CastDuration);
        if (_desoul.Active && state.Phase == BattleScenePhase.Reaction && state.IsInstantDeath)
            duration = Math.Max(duration, DesoulEffectView.StrikeDuration);
        if (buffCast) duration = Math.Max(duration, _buff.Duration - 28.0 / 60);
        if (debuffCast) duration = Math.Max(duration, _debuff.MinimumCastDuration);
        if ((absorbCast ? _absorb.CastElapsed(_elapsed) : detoxCast ? _detox.CastElapsed(_elapsed) : debuffCast ? _debuff.CastElapsed(_elapsed) : buffCast ? _buff.CastElapsed(_elapsed) : _elapsed) < duration ||
            state.Phase == BattleScenePhase.SpellStop && _detox.Active && _detox.Finishing) return null;
        if (state.Phase == BattleScenePhase.End) _audio?.RestoreBattlefieldMusic();
        _completed = true;
        return new CompletePresentation(state.Token, state.CompletionKind);
    }

    internal SessionCommand? Acknowledge(GameAction action)
    {
        if (_suspended || !IsVisibleInTree() || !CanProcess()) return null;
        if (action == GameAction.Wait && _state is { Healing.AtTimedInput: true } waiting && Error is null)
            return new AdvanceSimulation(waiting.Token);
        if (action != GameAction.Confirm || _state is not { RequiresAcknowledgement: true } state || Error is not null || _completed || Suppressed(state)) return null;
        if (_message.VisibleCharacters >= 0 && _message.VisibleCharacters < _message.GetTotalCharacterCount())
        { _revealed = _message.GetTotalCharacterCount(); _message.VisibleCharacters = (int)_revealed; return null; }
        if (_messagePanel.ReadMore()) return null;
        return state.Healing is { LogicalComplete: false, AtTimedInput: false } ? null : new Acknowledge(state.Token);
    }

    private string ActorName(ActorRef actor) => BattleCharacterNames.Resolve(_session.Current.Battle.GetActor(actor), Content, _session.Current.Story);

    private void PresentStatus(BattleSceneState state)
    {
        var battle = _session.Current.Battle;
        var actor = battle.GetActor(state.Actor);
        var target = !state.IsAfterTurnNotice && state.Target is { } id ? battle.GetActor(id) : null;
        if (state.IsCurseRecoil) target = null;
        // Single-side scenes switch the one status card to Application's displayed combatant.
        if (state.Spell is not null || state.Item is not null || state.FriendlySupport ||
            target?.Faction == actor.Faction)
        {
            actor = battle.GetActor(state.DisplayedAlly ?? state.DisplayedEnemy ?? state.Actor);
            target = null;
        }
        _actorCard.Present(actor, ActorName(actor.Actor));
        _targetCard.Present(target, target is null ? "" : ActorName(target.Actor));
        ArrangeStatus(state);
    }

    private void ArrangeStatus(BattleSceneState state)
    {
        Vector2 Place(ActorRef unit, Vector2 size)
        {
            bool recoveryTarget = (state.Spell is not null || state.Item is not null || state.FriendlySupport) && unit != state.Actor;
            bool ally = _session.Current.Battle.GetActor(unit).IsAlly;
            float bottom = _messagePanel.IsVisibleInTree() ? _messagePanel.Position.Y : GetViewportRect().Size.Y - 32;
            if (ally && !recoveryTarget)
                return new(GetViewportRect().Size.X - StoryTheme.GameplayInset - size.X, StoryTheme.BelowMenu);
            return new(StoryTheme.GameplayInset, ally ? StoryTheme.GameplayInset : MathF.Max(32, bottom - size.Y - 16));
        }
        if (_actorCard.Actor is { } actor) _actorCard.Position = Place(actor, _actorCard.RequestedSize);
        if (_targetCard.Actor is { } target) _targetCard.Position = Place(target, _targetCard.RequestedSize);
    }

    private string Message(BattleSceneState state)
    {
        _messageFallback = "";
        if (Suppressed(state)) return "";
        if (state.Phase == BattleScenePhase.EquipmentWearMessage) return SemanticMessage(state)!;
        if (state.Phase == BattleScenePhase.MuddleTurnMessage)
            return LocalizedMessage("battle.scene.muddle-expired", ActorName(state.Actor));
        if (state.Phase == BattleScenePhase.PhysicalInactionMessage && state.MuddleActionMessage is not null)
            return LocalizedMessage("battle.scene.muddle-inaction", ActorName(state.Actor));
        if (state.Phase == BattleScenePhase.PhysicalInactionMessage)
            return LocalizedMessage(state.CurseStoppedAttack ? "battle.scene.curse-inaction" : "battle.scene.stun-inaction",
                ActorName(state.Actor));
        if (state.IsCutoff && state.Phase == BattleScenePhase.ResultMessage)
            return LocalizedMessage("battle.scene.cutoff", ActorName(state.Target!.Value));
        if (state.IsCurseRecoil && state.Phase == BattleScenePhase.ResultMessage)
            return LocalizedMessage("battle.scene.curse-recoil", ActorName(state.Actor), state.Amount);
        if (state.IsCurseRecoil && state.Phase == BattleScenePhase.DeathMessage)
            return LocalizedMessage("battle.scene.ally-death", ActorName(state.Actor));
        if (state.Healing is { } message && (!GameText.Ready || message.Display is null ||
            string.IsNullOrEmpty(message.Display.CasterName) || string.IsNullOrEmpty(message.Display.TargetName) ||
            message.Display.RecoveredHp < 0))
        {
            Error = GameText.Ready ? "display-operands" : "localization-resources";
            return "Message unavailable (" + Error + ").";
        }
        if (state.IsSilencedCast || state.SupportSpell is not null || state.IsSilenceTurn || state.Phase is BattleScenePhase.SlowExpiredMessage or BattleScenePhase.BoostExpiredMessage or BattleScenePhase.AttackExpiredMessage or BattleScenePhase.SilenceMessage or BattleScenePhase.PoisonMessage or BattleScenePhase.StunMessage or BattleScenePhase.StunTurnMessage)
        {
            if (state.EquipmentItem is { } item && state.Phase == BattleScenePhase.ActionMessage)
                return LocalizedMessage("battle.scene.item", ActorName(state.Actor), DisplayNames.Item(item.ItemId, item.Name));
            var spell = state.SilencedSpell ?? state.SupportSpell?.Spell;
            if (spell is { } key && state.Phase is BattleScenePhase.ActionMessage or BattleScenePhase.SpellCost)
                return LocalizedMessage("battle.spell.action", ActorName(state.Actor), DisplayNames.Spell(key), key.Level);
            if (SemanticMessage(state) is { } statusMessage) return statusMessage;
            return "";
        }
        if (state.AttackSpell is { } attack)
        {
            if (state.IsMonsterBreath && state.Phase == BattleScenePhase.ActionMessage)
                return LocalizedMessage("battle.scene.flame-breath", ActorName(state.Actor));
            if (state.UsesAttackItem && state.Phase == BattleScenePhase.ActionMessage)
            {
                var item = state.AttackItem;
                return LocalizedMessage("battle.scene.item", ActorName(state.Actor),
                    DisplayNames.Item(item?.ItemId ?? 110, item?.Name ?? "Taros Sword"));
            }
            if (state.Phase is BattleScenePhase.ActionMessage or BattleScenePhase.SpellCost)
                return LocalizedMessage("battle.spell.action", ActorName(state.Actor), DisplayNames.Spell(attack.Spell), attack.Spell.Level);
            if (SemanticMessage(state) is { } spellMessage) return spellMessage;
        }
        if (state.Spell?.CuresPoison == true)
        {
            if (state.Phase is BattleScenePhase.ActionMessage or BattleScenePhase.SpellCost)
                return LocalizedMessage("battle.heal.action", ActorName(state.Actor), DisplayNames.Spell(state.Spell.Spell), state.Spell.Spell.Level);
            if (SemanticMessage(state) is { } cureMessage) return cureMessage;
        }
        if (state.Item?.CuresPoison == true && SemanticMessage(state) is { } itemCure)
            return itemCure;
        if (state.EquipmentRecoveryText is { } recoveryText)
            return LocalizedMessage(recoveryText == 357 ? "battle.heal.mp-recovery" : "battle.heal.recovery",
                ActorName(state.Actor), state.Amount);
        if (state.Item is { Effect: HealingItemEffect.RestoreMp } && state.Phase == BattleScenePhase.ResultMessage)
            return LocalizedMessage("battle.heal.mp-recovery", ActorName(state.Target!.Value), state.Amount);
        if (state.Spell?.IsAura == true && state.Phase == BattleScenePhase.ActionMessage)
            return GameText.Language == "en" ? $"{ActorName(state.Actor)} casts {state.Spell!.Spell.Value.ToUpperInvariant()} {state.Spell.Spell.Level}." :
                LocalizedMessage("battle.heal.action", ActorName(state.Actor), DisplayNames.Spell(state.Spell!.Spell), state.Spell.Spell.Level);
        if (state.Spell?.IsAura == true && state.Phase == BattleScenePhase.ResultMessage)
            return GameText.Language == "en" ? $"{ActorName(state.Target!.Value)} recovered {state.Amount} HP." :
                LocalizedMessage("battle.heal.recovery", ActorName(state.Target!.Value), state.Amount);
        if (state.Item is { } recoveryItem && state.Phase == BattleScenePhase.ActionMessage)
            return LocalizedMessage("battle.scene.item", ActorName(state.Actor), DisplayNames.Item(recoveryItem.ItemId, recoveryItem.Name));
        if (state.Healing is { } healing && state.Phase is BattleScenePhase.ActionMessage or BattleScenePhase.SpellCost)
        {
            _messageFallback = GameText.Fallback("battle.heal.action", true, exact: true);
            return BattleCharacterNames.HealingText(healing.Display, false, _session);
        }
        if (state.Healing is { } recovery && state.Phase is BattleScenePhase.ResultMessage or BattleScenePhase.MakeIdle or BattleScenePhase.SpellStop)
        {
            _messageFallback = GameText.Fallback("battle.heal.recovery", true, exact: true);
            return BattleCharacterNames.HealingText(recovery.Display, true, _session);
        }
        // Chinese uses authored functional templates from typed state. English keeps
        // the admitted source-ID route below; missing Chinese rows fall back whole.
        if (GameText.Language != "en" && SemanticMessage(state) is { } translated) return translated;
        bool ally = _session.Current.Battle.GetActor(state.Actor).IsAlly;
        int id = state.Phase switch
        {
            BattleScenePhase.SleepMessage => 338,
            BattleScenePhase.SleepTurnMessage => state.SleepTurnText!.Value,
            BattleScenePhase.PoisonDamageMessage => 307, BattleScenePhase.PoisonExhaustedMessage => 308,
            BattleScenePhase.ActionMessage => state.ActionKind switch { "prism-laser" => 319, "item-use" => 275, "physical-second" => 293, "physical-counter" => 292, _ => 273 },
            BattleScenePhase.ResultMessage => state.ReactionKind == "Ineffective" ? 289 : state.ReactionKind == "Recovery" ? 298 : state.ReactionKind == "Dodge" ? 286 : state.Critical ? (ally ? 287 : 288) : (ally ? 284 : 285),
            BattleScenePhase.DeathMessage => _session.Current.Battle.GetActor(state.Target!.Value).IsAlly ? 291 : 290,
            BattleScenePhase.ItemDroppedMessage => 396, BattleScenePhase.ItemReceivedMessage => 395,
            BattleScenePhase.RewardMessage => 263, BattleScenePhase.GoldMessage => 393, _ => -1,
        };
        if (state.GrowthNotice is { } notice)
            id = notice.Kind switch { BattleGrowthNoticeKind.Level => 244, BattleGrowthNoticeKind.MaxHp => 266,
                BattleGrowthNoticeKind.MaxMp => 267, BattleGrowthNoticeKind.Attack => 268, BattleGrowthNoticeKind.Defense => 269,
                BattleGrowthNoticeKind.Agility => 270, BattleGrowthNoticeKind.Spell => 271, _ => -1 };
        if (id < 0) return "";
        if (GameText.Language != "en") _messageFallback = GameText.Unmigrated("battle.text/" + id);
        string name = ActorName(state.Phase is BattleScenePhase.ActionMessage or BattleScenePhase.ItemReceivedMessage ? state.Actor :
            state.Phase is BattleScenePhase.RewardMessage or BattleScenePhase.GrowthMessage ? state.RewardActor!.Value : state.Target!.Value);
        int amount = state.GrowthNotice?.Amount ?? (state.Phase == BattleScenePhase.RewardMessage ? state.RewardAmount : state.Phase == BattleScenePhase.GoldMessage ? state.GoldAmount : state.Amount);
        string text = Content?.Texts.GetValueOrDefault(id) ?? (id == 307 ? "{NAME} takes {#} poison damage." : id == 308 ? "{NAME} is exhausted." : id == 319 ? "{NAME} fires a prism laser!" : id == 396 ? "{NAME} dropped {ITEM}." : id == 395 ? "{NAME} received {ITEM}." : id == 289 ? "The attack is ineffective against {NAME}." : id == 338 ? "{NAME} fell asleep." : id == 306 ? "{NAME} is sleeping." : id == 354 ? "{NAME} has awakened." : state.Phase == BattleScenePhase.ActionMessage ? "{NAME}'s attack!" :
            state.Phase == BattleScenePhase.RewardMessage ? "{NAME} earned {#} EXP." : state.Phase == BattleScenePhase.GoldMessage ? "Found {#} gold." : "{NAME}: {#}");
        // Text timing remains the existing product setting. No battle speech bleeps:
        // source bsc10/bsc11 clear CURRENT_SPEECH_SFX.
        // Replace only source tokens once. Names/item labels are literal values,
        // including braces that happen to resemble another source token.
        return Regex.Replace(text, @"\{(?:NAME|#|N|ITEM|SPELL|CLEAR|D[0-9]+)\}", match => match.Value switch
        {
            "{NAME}" => name, "{#}" => amount.ToString(), "{N}" => "\n",
            "{ITEM}" => state.ItemReceipt?.Item.Name ?? state.Item?.Name ?? "",
            "{SPELL}" => state.GrowthNotice?.Spell?.Value.ToUpperInvariant() ?? "", _ => "",
        });
    }

    private string? SemanticMessage(BattleSceneState state)
    {
        string Name(ActorRef? actor) => ActorName(actor!.Value);
        string Target() => Name(state.Target);
        if (state.GrowthNotice is { } growth)
        {
            string? key = growth.Kind switch
            {
                BattleGrowthNoticeKind.Level => "level", BattleGrowthNoticeKind.MaxHp => "max-hp",
                BattleGrowthNoticeKind.MaxMp => "max-mp", BattleGrowthNoticeKind.Attack => "attack-growth",
                BattleGrowthNoticeKind.Defense => "defense-growth", BattleGrowthNoticeKind.Agility => "agility-growth",
                BattleGrowthNoticeKind.Spell => "spell-growth", _ => null,
            };
            if (key is null) return null;
            // Amount is the learned level for Spell, not a stat gain; retain SpellRef.
            return growth.Spell is { } spell && growth.Kind == BattleGrowthNoticeKind.Spell
                ? LocalizedMessage("battle.scene." + key, Name(state.RewardActor), DisplayNames.Spell(spell), spell.Level)
                : growth.Kind == BattleGrowthNoticeKind.Spell ? null
                : LocalizedMessage("battle.scene." + key, Name(state.RewardActor), growth.Amount);
        }
        return state.Phase switch
        {
            BattleScenePhase.ActionMessage => state.ActionKind switch
            {
                "physical-first" => LocalizedMessage("battle.scene.attack", ActorName(state.Actor)),
                "physical-second" => LocalizedMessage("battle.scene.second", ActorName(state.Actor)),
                "physical-counter" => LocalizedMessage("battle.scene.counter", ActorName(state.Actor)),
                "item-use" => LocalizedMessage("battle.scene.item", ActorName(state.Actor), DisplayNames.Item(state.Item!.ItemId, state.Item.Name)),
                "prism-laser" => LocalizedMessage("battle.scene.prism", ActorName(state.Actor)), _ => null,
            },
            BattleScenePhase.ResultMessage when state.AbsorbsMp =>
                LocalizedMessage("battle.scene.mp-absorbed", ActorName(state.Actor), state.Amount),
            BattleScenePhase.ResultMessage when state.SupportSpell?.Spell.Value == "muddle" && state.ReactionKind == "Status" =>
                LocalizedMessage("battle.scene.muddle", Target()),
            BattleScenePhase.ResultMessage when state.SupportSpell?.Spell.Value == "sleep" && state.ReactionKind == "Status" =>
                LocalizedMessage("battle.scene.sleep", Target()),
            BattleScenePhase.ResultMessage when state.SupportSpell?.ChangesStats == true && state.ReactionKind == "Status" =>
                LocalizedMessage(state.RaisesAttack ? "battle.scene.att-up" : state.FriendlySupport ? "battle.scene.agi-up" : "battle.scene.agi-down", Target(), state.SupportStatAmount),
            BattleScenePhase.SupportDefenseMessage =>
                LocalizedMessage(state.FriendlySupport ? "battle.scene.def-up" : "battle.scene.def-down", Target(), state.SupportStatAmount),
            BattleScenePhase.SlowExpiredMessage => LocalizedMessage("battle.scene.slow-expired", Target()),
            BattleScenePhase.BoostExpiredMessage => LocalizedMessage("battle.scene.boost-expired", Target()),
            BattleScenePhase.AttackExpiredMessage => LocalizedMessage("battle.scene.attack-expired", Target()),
            BattleScenePhase.EquipmentWearMessage =>
                LocalizedMessage(state.Wear == EquipmentWear.Damaged ? "battle.scene.item-damaged" : "battle.scene.item-destroyed",
                    DisplayNames.Item(state.EquipmentItem?.ItemId ?? state.Item?.ItemId ?? state.AttackItem!.ItemId,
                        state.EquipmentItem?.Name ?? state.Item?.Name ?? state.AttackItem!.Name)),
            _ when state.ShowsStunCure => LocalizedMessage("battle.scene.stun-cleared", Target()),
            _ when state.ShowsCurseCure => LocalizedMessage("battle.scene.curse-cleared", Target()),
            BattleScenePhase.ResultMessage => state.ReactionKind switch
            {
                "Status" => LocalizedMessage(state.Spell?.CuresPoison == true || state.Item?.CuresPoison == true ? "battle.scene.poison-cured" : "battle.scene.silence", Target()),
                "Recovery" => LocalizedMessage("battle.heal.recovery", Target(), state.Amount),
                "Damage" when state.IsCutoff => LocalizedMessage("battle.scene.cutoff", Target()),
                "Damage" => LocalizedMessage(state.Critical ? "battle.scene.critical" : "battle.scene.damage", Target(), state.Amount),
                "Dodge" => LocalizedMessage("battle.scene.dodge", Target()),
                "Ineffective" => LocalizedMessage("battle.scene.ineffective", Target()), _ => null,
            },
            BattleScenePhase.DeathMessage when state.IsInstantDeath => LocalizedMessage("battle.scene.instant-death", Target()),
            BattleScenePhase.DeathMessage => LocalizedMessage(_session.Current.Battle.GetActor(state.Target!.Value).IsAlly
                ? "battle.scene.ally-death" : "battle.scene.enemy-death", Target()),
            BattleScenePhase.ItemReceivedMessage => LocalizedMessage("battle.scene.received", ActorName(state.Actor), DisplayNames.Item(state.ItemReceipt!.Item.ItemId, state.ItemReceipt.Item.Name)),
            BattleScenePhase.ItemDroppedMessage => LocalizedMessage("battle.scene.dropped", Target(), DisplayNames.Item(state.ItemReceipt!.Item.ItemId, state.ItemReceipt.Item.Name)),
            BattleScenePhase.RewardMessage => LocalizedMessage("battle.scene.exp", Name(state.RewardActor), state.RewardAmount),
            BattleScenePhase.GoldMessage => LocalizedMessage("battle.scene.gold", state.GoldAmount),
            BattleScenePhase.SilencedCastMessage => LocalizedMessage("battle.scene.silenced-cast", ActorName(state.Actor)),
            BattleScenePhase.StunMessage => LocalizedMessage("battle.scene.stunned", Target()),
            BattleScenePhase.StunTurnMessage => LocalizedMessage(state.StunCleared ? "battle.scene.stun-cleared" : "battle.scene.stunned", Target()),
            BattleScenePhase.PoisonMessage => LocalizedMessage("battle.scene.poison-inflicted", Target()),
            BattleScenePhase.SilenceMessage => LocalizedMessage("battle.scene.silence", Target()),
            BattleScenePhase.SilenceTurnMessage => LocalizedMessage("battle.scene.silence-expired", ActorName(state.Actor)),
            BattleScenePhase.SleepMessage => LocalizedMessage("battle.scene.sleep", Target()),
            // SleepTurnText is the typed existing discriminator: 306 asleep, 354 awake.
            BattleScenePhase.SleepTurnMessage => state.SleepTurnText switch
            { 306 => LocalizedMessage("battle.scene.sleeping", Target()), 354 => LocalizedMessage("battle.scene.awake", Target()), _ => null },
            BattleScenePhase.PoisonDamageMessage => LocalizedMessage("battle.scene.poison", Target(), state.Amount),
            BattleScenePhase.PoisonExhaustedMessage => LocalizedMessage("battle.scene.exhausted", Target()), _ => null,
        };
    }

    private string LocalizedMessage(string key, params object[] args)
    {
        _messageFallback = GameText.Fallback(key, true, exact: true);
        return GameText.Get(key, args);
    }

    private void SetFrame(bool ally, int frame)
    {
        var visual = ally ? _allyVisual! : _enemyVisual!;
        var resource = visual.Frames[frame];
        var body = ally ? _ally : _enemy;
        // The installed allies also include native 96x96 originals. Use that
        // Original footprint to lay out both densities.
        var original = Content!.Rasters[resource];
        float scale = ally && original.Width == 96 && original.Height == 96 ? 1 : 0.5f;
        Bind(body, resource, BattleSceneRaster.HasHdGroup(Content, visual.Frames), scale);
        if (ally) { _allyFrame = frame; _allyResource = resource; } else { _enemyFrame = frame; _enemyResource = resource; }
    }
    private void Weapon(BattleSceneWeaponFrame? frame, Vector2 offset)
    {
        _weapon.Visible = _hasWeapon && frame is not null && _allyVisual!.WeaponFrames.Count > 0;
        if (!_weapon.Visible) return;
        var frames = _allyVisual!.WeaponFrames;
        _weaponResource = frames[frame!.Frame & 7];
        Bind(_weapon, _weaponResource, BattleSceneRaster.HasHdGroup(Content!, frames));
        _weapon.FlipH = (frame.Frame & 16) != 0; _weapon.FlipV = (frame.Frame & 32) != 0;
        _weapon.Position = new Vector2(136 + frame.X,64 + frame.Y) + offset;
        // Source layer 1 follows the ally in the VDP sprite chain (behind);
        // layer 2 precedes it (in front). Keep that per-frame distinction.
        _weapon.ZIndex = frame.Layer == 2 ? 3 : 1;
    }
    private Sprite2D Sprite(string name, Vector2 position)
    { var node = new Sprite2D { Name = name, Position = position, Centered = false }; _canvas.AddChild(node); return node; }
    private void PresentMessage(BattleSceneState state)
    {
        _message.Text = Message(state);
        _messagePanel.Present(!state.IsFieldDeath && _message.Text.Length > 0, false, false, default, "");
        RefreshMessageHint(state);
    }
    private void RefreshMessageHint(BattleSceneState state)
    {
        bool revealing = _message.VisibleCharacters >= 0 && _message.VisibleCharacters < _message.GetTotalCharacterCount();
        string key = !state.RequiresAcknowledgement ? "battle.scene.in-progress" : revealing
            ? "field.reveal" : _messagePanel.MoreBelow ? "field.more"
            : state.Healing is { LogicalComplete: false, AtTimedInput: false }
            ? "battle.scene.in-progress" : "story.continue";
        string hint = key == "battle.scene.in-progress" ? GameText.Get(key) : GameText.Get(key, _input.CompactHint(GameAction.Confirm));
        var fallbacks = new List<string> { _messageFallback, GameText.Fallback(key, true, exact: true) };
        if (state.Healing is { AtTimedInput: true })
        {
            hint += "    " + GameText.Get("battle.scene.wait", _input.CompactHint(GameAction.Wait));
            fallbacks.Add(GameText.Fallback("battle.scene.wait", true, exact: true));
        }
        // Only the current visible body/hint contributes a fallback notice.
        if (_message.Text.Length > 0)
            foreach (string fallback in fallbacks.Where(value => value.Length > 0).Distinct()) hint += "\n" + fallback;
        _messagePanel.SetHint(hint);
        PresentStatus(state);
    }
    internal object Observe() => new
    {
        aura = _state is { Spell.IsAura: true, Healing: null } ? _aura.Observe() : null,
        suspended = _suspended,
        laser = _state?.Laser is not null ? _laser.Observe() : null,
        attackSpell = _state?.AttackSpell is not null && (_state.AttackItem is null || _preparedAttackSpell) ? _attackSpell.Observe() : null,
        monsterBreath = _state?.IsMonsterBreath == true,
        phoenixWave = _phoenixWave.Observe(),
        desoul = _desoul.Observe(), dao = _dao.Observe(), apollo = _apollo.Observe(), neptun = _neptun.Observe(), atlas = _atlas.Observe(), daoDistortion = _enemy.Material == _daoDistortion,
        detox = _detox.Observe(),
        debuff = _debuff.Observe(),
        buff = _buff.Observe(), buffBackdrop = _sceneBlack.Color.ToHtml(),
        absorb = _absorb.Observe(), absorbBackdrop = _sceneBlack.Color.ToHtml(),
        visible = Visible, error = Error, phase = _state?.Phase.ToString(), waitToken = _state?.Token.Value,
        suppressed = _state is { } messageState && Suppressed(messageState), suppressible = _state?.CanSuppressHitCoinMessage,
        requiresAcknowledgement = _state?.RequiresAcknowledgement,
        fieldDeath = _state is { IsFieldDeath: true } field ? new { actors = field.DeadActors.Select(a => a.Value),
            step = field.FieldStep, facing = field.FieldFacing, delay = field.FieldDelay } : null,
        elapsed = _elapsed, completed = _completed, animationIndex = _animationIndex, frameIndex = _frameIndex, reactionState = _logicalReaction,
        displayedAlly = _state?.DisplayedAlly?.Value, displayedEnemy = _state?.DisplayedEnemy?.Value,
        canvas = BattleMapViewport.Rectangle(new Rect2(_canvas.Position, _canvas.Scale * new Vector2(256, 224))),
        allyVisible = _ally.Visible, enemyVisible = _enemy.Visible, backgroundVisible = _background.Visible, groundVisible = _ground.Visible,
        actionKind = _state?.ActionKind, reactionKind = _state?.ReactionKind, supportSpell = _state?.SupportSpell, silencedSpell = _state?.SilencedSpell,
        reactionAmount = _state?.Amount,
        reactionTarget = _state?.Target?.Value,
        item = _state?.EquipmentItem?.ItemId ?? _state?.AttackItem?.ItemId ?? _state?.Item?.ItemId,
        equipmentWear = _state?.Wear.ToString(),
        spell = _state?.RecoveryEffect?.Spell ?? _state?.AttackSpell?.Spell ?? _state?.SupportSpell?.Spell ?? _state?.SilencedSpell,
        spellAnimationSelector = AbsorbEffectView.Selector(_state) ?? BuffEffectView.Selector(_state) ?? DebuffSelector(_state) ?? _state?.AttackAnimationSelector ?? (_state?.AttackItem is not null || _state?.EquipmentItem is not null ? 0 : _state?.AttackSpell is { } attack ? attack.SourceAnimationSelector : _state is { Spell.IsAura: true, Healing: null } ? (int?)null : _state?.Laser is not null ? 149 : DetoxSelector(_state) ?? (_state?.RecoveryEffect is { } recoveryEffect ? 4 + 32 * (recoveryEffect.Spell.Level - 1) : _state?.Item is not null ? 0 : (int?)null)),
        healing = _state?.Healing is { } healing ? new { healing.Caller, healing.Remaining, healing.AtTimedInput,
            healing.LogicalComplete, healing.Delivered, healing.CastRequested, healing.CastFrame, healing.Fairy } : null,
        fairySprites = _fairyBodies.Concat(_fairyWings).Concat(_fairyDust).Where(sprite => sprite.Visible)
            .Select(sprite => new { name = sprite.Name.ToString(), x = sprite.Position.X, y = sprite.Position.Y, mirror = sprite.FlipH,
                binding = BoundResource(sprite),
                width = sprite.GetRect().Size.X, height = sprite.GetRect().Size.Y }).ToArray(),
        allyFrame = _allyFrame, enemyFrame = _enemyFrame, allyResource = _allyResource, enemyResource = _enemyResource,
        ally = BoundResource(_ally), enemy = BoundResource(_enemy), weapon = BoundResource(_weapon),
        weaponResource = _weaponResource, weaponVisible = _weapon.Visible, weaponFlipH = _weapon.FlipH, weaponFlipV = _weapon.FlipV,
        weaponZ = _weapon.ZIndex, allyZ = _ally.ZIndex, weaponY = _weapon.Position.Y,
        weaponRect = new { x = _weapon.GetRect().Position.X, y = _weapon.GetRect().Position.Y,
            width = _weapon.GetRect().Size.X, height = _weapon.GetRect().Size.Y },
        background = BoundResource(_background), backgroundWrap = BoundResource(_backgroundWrap), ground = BoundResource(_ground),
        textureCount = _textures.Count, reducedFlash = _settings?.ReducedFlash, message = _message.Text,
        messagePanel = _messagePanel.Observe(), visibleCharacters = _message.VisibleCharacters, messageFont = ObserveMessageFont(),
        actorCard = _actorCard.Observe(), targetCard = _targetCard.Observe(),
        allyX = _ally.Position.X, allyY = _ally.Position.Y, enemyX = _enemy.Position.X, enemyY = _enemy.Position.Y,
        backgroundX = _background.Position.X, groundX = _ground.Position.X, weaponX = _weapon.Position.X,
    };
    private object ObserveMessageFont()
    {
        var font = _message.LabelSettings?.Font ?? _message.GetThemeFont("font");
        var server = TextServerManager.GetPrimaryInterface();
        // These are the mounted Label's resolved font/fallback caches. Public
        // Label APIs do not expose which face shaped each character.
        return new
        {
            visible = _message.IsVisibleInTree(),
            size = _message.LabelSettings?.FontSize ?? _message.GetThemeFontSize("font_size"),
            resourceClass = font.GetClass().ToString(),
            faces = font.GetRids().Select(rid => new
            {
                family = server.FontGetName(rid), style = server.FontGetStyleName(rid),
                faceIndex = server.FontGetFaceIndex(rid),
                allowSystemFallback = server.FontIsAllowSystemFallback(rid),
            }).ToArray(),
        };
    }
    public override void _ExitTree()
    {
        VisibilityChanged -= SyncOverlay;
        // The overlay shares BattleSessionView lifetime; reparenting a battle exits/re-enters
        // this tree without retiring its presenters or their reading state.
        GetWindow().FocusExited -= Suspend; GetWindow().FocusEntered -= Resume;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete) DisposeTextures();
    }
}
