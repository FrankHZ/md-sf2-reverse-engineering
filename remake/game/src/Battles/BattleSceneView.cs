using System.Text.RegularExpressions;
using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.Audio;
using Sf2.Remake.GodotAdapter.Input;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Battles;

// One scene consumer. It projects admitted frames and acknowledges the session's
// wait token; it never computes damage, rewards, RNG or the next combatant.
internal sealed partial class BattleSceneView : Control
{
    private readonly Dictionary<(string Encounter, string Resource), ImageTexture> _textures = [];
    private PrismLaserView _laser = null!;
    private AuraRecoveryView _aura = null!;
    private bool _suspended;
    private Node2D _canvas = null!;
    private ColorRect _sceneBlack = null!;
    private Sprite2D _background = null!, _backgroundWrap = null!, _ground = null!, _ally = null!, _enemy = null!, _weapon = null!;
    private Label _message = null!;
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
    internal string? Error { get; private set; }
    private BattleSceneDefinition? Content => _session.ActiveBattleScenes;

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
        AddChild(_actorCard); AddChild(_targetCard);
        AddChild(_messagePanel); _message = _messagePanel.Body; _messagePanel.Choices.Hide();
        for (int i = 0; i < 2; i++)
        {
            _fairyBodies.Add(Sprite("FairyBody" + i, Vector2.Zero));
            _fairyWings.Add(Sprite("FairyWings" + i, Vector2.Zero));
        }
        for (int i = 0; i < 23; i++) _fairyDust.Add(Sprite("FairyDust" + i, Vector2.Zero));
        foreach (var sprite in _fairyBodies.Concat(_fairyWings).Concat(_fairyDust)) { sprite.ZIndex = 3; sprite.Hide(); }
        _aura = new AuraRecoveryView { Name = "AuraRecovery", ZIndex = 4 }; _canvas.AddChild(_aura); _aura.Hide();
        _laser = new PrismLaserView { Name = "PrismLaser", ZIndex = 4 }; _canvas.AddChild(_laser); _laser.Hide();
        Hide();
    }

    public override void _EnterTree()
    { GetWindow().FocusExited += Suspend; GetWindow().FocusEntered += Resume; }
    private void Suspend() => _suspended = true;
    private void Resume() => _suspended = false;

    internal void Configure(GameSession session, GameInput input, SessionAudio? audio)
    { _session = session; _input = input; _settings = input.Settings; _audio = audio; }

    internal void Present()
    {
        var state = _session.Current.BattleScene;
        if (state is null) { _messagePanel.Present(false, false, false, default, "", false); Hide(); _laser.Hide(); _aura.Hide(); _state = null; return; }
        if (state.IsFieldDeath) Hide(); else Show();
        PresentStatus(state);
        if (_state?.Token != state.Token) _messagePanel.ResetReading();
        if (_state?.Token == state.Token) { _state = state; PresentMessage(state); ProjectHealing(state); return; }
        _state = state; _elapsed = _revealed = 0; _frameIndex = _logicalReaction = -1; _completed = _sceneMusicStarted = false; _frames = []; _animationIndex = null;
        try
        {
            _laser.Hide(); _aura.Hide();
            if (state.IsAura)
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
            var ally = battle.GetActor(state.DisplayedAlly!.Value);
            var enemy = state.DisplayedEnemy is { } enemyRef ? battle.GetActor(enemyRef) : null;
            _enemy.Visible = enemy is not null;
            _enemyVisual = null; _enemyResource = null; _weaponResource = null;
            if (Content is { } content)
            {
                if (content.Encounter != battle.Definition.Encounter || !content.Allies.TryGetValue(ally.Definition.ClassRule, out _allyVisual) ||
                    (enemy is not null && !content.Enemies.TryGetValue(enemy.Actor, out _enemyVisual))) throw new InvalidOperationException("battle-scene-actor-unavailable");
                string background = content.Background, ground = content.Ground;
                if (content.TerrainScenes is { } terrain)
                {
                    var back = enemy?.Position ?? ally.Position ?? ally.Deployment.Position;
                    var front = ally.Position ?? ally.Deployment.Position;
                    background = terrain[back.Y * 48 + back.X].Background;
                    ground = terrain[front.Y * 48 + front.X].Ground;
                }
                Bind(_background, background); Bind(_backgroundWrap, background); Bind(_ground, ground);
                _allyFrame = _enemyFrame = 0;
                var equipped = ally.SourceLoadout?.Items.FirstOrDefault(item => (item & 128) != 0 && (item & 127) != 127);
                _hasWeapon = equipped is > 0;
                if (_hasWeapon && (equipped!.Value & 127) != _allyVisual.Item) throw new InvalidOperationException("battle-scene-weapon-unavailable");
                SetFrame(true, 0); if (enemy is not null) SetFrame(false, 0);
                Weapon(_allyVisual.Sequences["idle"].IdleWeapon, Vector2.Zero);
                _ground.Visible = ally.Definition.Mover is not (BattleMover.Hovering);
                if (state.Phase == BattleScenePhase.ActionAnimation)
                {
                    var animation = (actor.IsAlly ? _allyVisual : _enemyVisual!).Sequences[state.Spell is not null ? "cast" : state.Item is null ? "attack" : "item"];
                    _animationIndex = animation.Index;
                    _frames = animation.Frames.Skip(actor.IsAlly ? 1 : 0).ToArray();
                }
                if (state.Phase == BattleScenePhase.Reaction && state.ReactionKind == "Dodge")
                    _frames = (target.IsAlly ? _allyVisual : _enemyVisual!).Sequences["dodge"].Frames.Skip(target.IsAlly ? 1 : 0).ToArray();
            }
            _ally.Position = new(136, 64); _enemy.Position = new(16, 48);
            _background.Position = new(0,56); _backgroundWrap.Position = new(256,56); _ground.Position = new(136,140);
            _ally.Modulate = _enemy.Modulate = Colors.White;
            PresentMessage(state);
            _message.VisibleCharacters = _settings.TextMode == "instant" || !state.RequiresAcknowledgement ? -1 : 0;
            if (state.Phase == BattleScenePhase.Initialize) { _castSoundPlayed = false; _audio?.BeginBattleScene(actor.IsAlly ? 2 : 5); }
            if (state.Phase == BattleScenePhase.Reaction && state.ReactionKind == "Damage") _audio?.PlayEffect(target.IsAlly ? 81 : 83);
            if (state.Phase == BattleScenePhase.Reaction && state.ReactionKind == "Recovery") _audio?.PlayEffect(113);
            if (state.GrowthNotice is { Kind: BattleGrowthNoticeKind.Level }) _audio?.PlayEffect(102);
            if (state.Phase == BattleScenePhase.End) _audio?.BeginSceneEnd();
            ProjectHealing(state);
        }
        catch (InvalidOperationException error) { Error = error.Message; }
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
        var viewport = GetViewportRect().Size;
        float scale = MathF.Min(viewport.X / 256, viewport.Y / 224);
        _canvas.Scale = new(scale, scale); _canvas.Position = (viewport - new Vector2(256, 224) * scale) / 2;
        RefreshMessageHint(state);
        _elapsed += delta;
        if (state.IsAfterTurnNotice || state.Phase == BattleScenePhase.SleepMessage)
        {
            // Modern finite delivery of the source's no-input status notices. Actual
            // completion releases the status command; reveal/input speed adds no RNG.
            if (_elapsed < 0.5) return null;
            _completed = true;
            return new CompletePresentation(state.Token, state.CompletionKind);
        }
        if (state.RequiresAcknowledgement)
        {
            _revealed += delta * _settings.CharactersPerSecond;
            if (_message.VisibleCharacters >= 0) _message.VisibleCharacters = (int)_revealed;
            // Ready text is host delivery, not another player acknowledgement.
            // Publish at the input boundary so modern reveal speed cannot insert
            // delivery among mandatory logical operations or advance timed input.
            if (state.IsHealingMessage && state.Healing is { Delivered: false } message &&
                (message.AtTimedInput || message.LogicalComplete) &&
                (_message.VisibleCharacters < 0 || _message.VisibleCharacters >= _message.GetTotalCharacterCount()) && !_messagePanel.MoreBelow)
                return new CompletePresentation(state.Token, state.CompletionKind);
            return state.Healing is { LogicalComplete: false, AtTimedInput: false } ? new AdvanceSimulation(state.Token) : null;
        }
        if (state.IsAura)
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
        else if (state.Phase is BattleScenePhase.TargetExit or BattleScenePhase.TargetEnter or BattleScenePhase.ActorExit or BattleScenePhase.ActorEnter)
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
            double time = _elapsed * 60;
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
        if (_elapsed < duration) return null;
        if (state.Phase == BattleScenePhase.End) _audio?.RestoreBattlefieldMusic();
        _completed = true;
        return new CompletePresentation(state.Token, state.CompletionKind);
    }

    internal SessionCommand? Acknowledge(GameAction action)
    {
        if (_suspended || !IsVisibleInTree() || !CanProcess()) return null;
        if (action == GameAction.Wait && _state is { Healing.AtTimedInput: true } waiting && Error is null)
            return new AdvanceSimulation(waiting.Token);
        if (action != GameAction.Confirm || _state is not { RequiresAcknowledgement: true } state || Error is not null) return null;
        if (_message.VisibleCharacters >= 0 && _message.VisibleCharacters < _message.GetTotalCharacterCount())
        { _revealed = _message.GetTotalCharacterCount(); _message.VisibleCharacters = (int)_revealed; return null; }
        if (_messagePanel.ReadMore()) return null;
        return state.Healing is { LogicalComplete: false, AtTimedInput: false } ? null : new Acknowledge(state.Token);
    }

    private void ProjectHealing(BattleSceneState state)
    {
        foreach (var sprite in _fairyBodies.Concat(_fairyWings).Concat(_fairyDust)) sprite.Hide();
        if (state.Healing is not { } healing) return;
        if (healing.CastRequested && !_castSoundPlayed) { _audio?.PlayEffect(77); _castSoundPlayed = true; }
        if (state.Phase == BattleScenePhase.ActionAnimation && healing.CastFrame is > 0 and var frame && frame <= _frames.Length)
        {
            _frameIndex = frame - 1;
            var entry = _frames[_frameIndex];
            if (entry.Frame != 15) SetFrame(true, entry.Frame);
            _ally.Position = new Vector2(136, 64) + new Vector2(entry.X, entry.Y);
            Weapon(entry.Weapon, new(entry.X, entry.Y));
        }
        _ally.Modulate = !_settings.ReducedFlash && healing.Caller == "cast-flash-on" ? new Color(0.7f, 1, 0.7f) : Colors.White;
        if (state.Phase is BattleScenePhase.TargetExit or BattleScenePhase.TargetEnter or BattleScenePhase.ActorExit or BattleScenePhase.ActorEnter)
        {
            float from = state.Phase == BattleScenePhase.TargetEnter ? -376 : state.Phase == BattleScenePhase.ActorEnter ? 8 : 136;
            float to = state.Phase == BattleScenePhase.TargetExit ? 272 : state.Phase == BattleScenePhase.ActorExit ? -112 : 136;
            int opportunity = healing.Caller?.EndsWith(":wait", StringComparison.Ordinal) == true ? 0 : healing.Opportunity;
            float x = from + MathF.Sign(to - from) * MathF.Min(opportunity * 16, MathF.Abs(to - from));
            _ally.Position = new(x, 64); _ground.Position = new(x, 140);
            Weapon(_allyVisual?.Sequences["idle"].IdleWeapon, new(x - 136, 0));
        }
        if (Content?.Healing is not { } visual || healing.Fairy is not { Control: not 0 } fairy) return;
        void Put(Sprite2D sprite, string resource, ushort x, ushort y, bool mirror = false)
        {
            Bind(sprite, resource); sprite.FlipH = mirror;
            sprite.Position = new Vector2(x - 128, y - 128) - sprite.GetRect().Position * sprite.Scale;
            sprite.Show();
        }
        for (int i = 0; i < fairy.Fairies.Count; i++)
        {
            var instance = fairy.Fairies[i];
            if (!instance.Active) continue;
            Put(_fairyBodies[i], visual.Bodies[instance.BodyFrame], instance.X, instance.Y, instance.Mirrored);
            Put(_fairyWings[i], visual.Wings[instance.WingFrame], instance.X, instance.Y, instance.Mirrored);
        }
        for (int i = 0; i < fairy.Dust.Count; i++)
        {
            var dust = fairy.Dust[i];
            if (dust.Age != 0) Put(_fairyDust[i], visual.Dust[dust.Frame], dust.X, dust.Y);
        }
    }

    private string ActorName(ActorRef actor) => BattleCharacterNames.Resolve(_session.Current.Battle.GetActor(actor), Content);

    private void PresentStatus(BattleSceneState state)
    {
        var battle = _session.Current.Battle;
        var actor = battle.GetActor(state.Actor);
        var target = !state.IsAfterTurnNotice && state.Target is { } id ? battle.GetActor(id) : null;
        _actorCard.Present(actor, ActorName(actor.Actor), "Acting");
        _targetCard.Present(target, target is null ? "" : ActorName(target.Actor), "Target");
        _actorCard.Position = new(32, 32); _actorCard.Size = _actorCard.RequestedSize;
        _targetCard.Position = new(GetViewportRect().Size.X - 32 - _targetCard.RequestedSize.X, 32);
        _targetCard.Size = _targetCard.RequestedSize;
    }

    private string Message(BattleSceneState state)
    {
        if (state.IsAura && state.Phase == BattleScenePhase.ActionMessage)
            return $"{ActorName(state.Actor)} casts {state.Spell!.Spell.Value.ToUpperInvariant()} {state.Spell.Spell.Level}.";
        if (state.IsAura && state.Phase == BattleScenePhase.ResultMessage)
            return $"{ActorName(state.Target!.Value)} recovered {state.Amount} HP.";
        if (state.Healing is { } healing && state.Phase is BattleScenePhase.ActionMessage or BattleScenePhase.SpellCost)
            return BattleCharacterNames.HealingText(healing.ActionText, _session.Current.Battle.GetActor(state.Actor), Content);
        if (state.Healing is { } recovery && state.Phase is BattleScenePhase.ResultMessage or BattleScenePhase.MakeIdle or BattleScenePhase.SpellStop)
            return BattleCharacterNames.HealingText(recovery.RecoveryText, _session.Current.Battle.GetActor(state.Target!.Value), Content);
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
        string name = ActorName(state.Phase is BattleScenePhase.ActionMessage or BattleScenePhase.ItemReceivedMessage ? state.Actor :
            state.Phase is BattleScenePhase.RewardMessage or BattleScenePhase.GrowthMessage ? state.RewardActor!.Value : state.Target!.Value);
        int amount = state.GrowthNotice?.Amount ?? (state.Phase == BattleScenePhase.RewardMessage ? state.RewardAmount : state.Phase == BattleScenePhase.GoldMessage ? state.GoldAmount : state.Amount);
        string text = Content?.Texts.GetValueOrDefault(id) ?? (id == 307 ? "{NAME} takes {#} poison damage." : id == 308 ? "{NAME} is exhausted." : id == 319 ? "{NAME} fires a prism laser!" : id == 396 ? "{NAME} dropped {ITEM}." : id == 395 ? "{NAME} received {ITEM}." : id == 289 ? "The attack is ineffective against {NAME}." : id == 338 ? "{NAME} fell asleep." : id == 306 ? "{NAME} is sleeping." : id == 354 ? "{NAME} has awakened." : state.Phase == BattleScenePhase.ActionMessage ? "{NAME}'s attack!" :
            state.Phase == BattleScenePhase.RewardMessage ? "{NAME} earned {#} EXP." : state.Phase == BattleScenePhase.GoldMessage ? "Found {#} gold." : "{NAME}: {#}");
        // Text timing remains the existing product setting. No battle speech bleeps:
        // source bsc10/bsc11 clear CURRENT_SPEECH_SFX.
        return Regex.Replace(text.Replace("{NAME}", name).Replace("{#}", amount.ToString()).Replace("{N}", "\n")
            .Replace("{ITEM}", state.ItemReceipt?.Item.Name ?? state.Item?.Name ?? "").Replace("{SPELL}", state.GrowthNotice?.Spell?.Value.ToUpperInvariant() ?? "").Replace("{CLEAR}", ""), @"\{D[0-9]+\}", "");
    }

    private void SetFrame(bool ally, int frame)
    {
        var visual = ally ? _allyVisual! : _enemyVisual!;
        var resource = visual.Frames[frame]; Bind(ally ? _ally : _enemy, resource);
        if (ally) { _allyFrame = frame; _allyResource = resource; } else { _enemyFrame = frame; _enemyResource = resource; }
    }
    private void Weapon(BattleSceneWeaponFrame? frame, Vector2 offset)
    {
        _weapon.Visible = _hasWeapon && frame is not null && _allyVisual!.WeaponFrames.Count > 0;
        if (!_weapon.Visible) return;
        _weaponResource = _allyVisual!.WeaponFrames[frame!.Frame & 7]; Bind(_weapon, _weaponResource);
        _weapon.FlipH = (frame.Frame & 16) != 0; _weapon.FlipV = (frame.Frame & 32) != 0;
        _weapon.Position = new Vector2(136 + frame.X,64 + frame.Y) + offset;
        _weapon.ZIndex = frame.Layer == 1 ? 3 : 1;
    }
    private void Bind(Sprite2D node, string resource)
    {
        var key = (Content!.Encounter, resource);
        if (!_textures.TryGetValue(key, out var texture))
        {
            var raster = Content!.Rasters[resource]; using var image = new Image();
            if (image.LoadPngFromBuffer(raster.CopyBytes()) != Godot.Error.Ok || image.GetWidth() != raster.Width || image.GetHeight() != raster.Height)
                throw new InvalidOperationException("battle-scene-texture-invalid");
            texture = ImageTexture.CreateFromImage(image); _textures.Add(key, texture);
        }
        node.Texture = texture; node.Scale = new(0.5f,0.5f);
        node.SetMeta("bound_resource", resource);
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
        string hint = !state.RequiresAcknowledgement ? "Action in progress…" : revealing
            ? $"{_input.Hint(GameAction.Confirm)}: Reveal" : _messagePanel.MoreBelow
            ? $"{_input.Hint(GameAction.Confirm)}: Read more" : state.Healing is { LogicalComplete: false, AtTimedInput: false }
            ? "Action in progress…" : $"{_input.Hint(GameAction.Confirm)}: Continue ▾";
        if (state.Healing is { AtTimedInput: true }) hint += $"    {_input.Hint(GameAction.Wait)}: Wait";
        _messagePanel.SetHint(hint);
    }
    internal object Observe() => new
    {
        aura = _state?.IsAura == true ? _aura.Observe() : null,
        suspended = _suspended,
        laser = _state?.Laser is not null ? _laser.Observe() : null,
        visible = Visible, error = Error, phase = _state?.Phase.ToString(), waitToken = _state?.Token.Value,
        fieldDeath = _state is { IsFieldDeath: true } field ? new { actors = field.DeadActors.Select(a => a.Value),
            step = field.FieldStep, facing = field.FieldFacing, delay = field.FieldDelay } : null,
        elapsed = _elapsed, completed = _completed, animationIndex = _animationIndex, frameIndex = _frameIndex, reactionState = _logicalReaction,
        displayedAlly = _state?.DisplayedAlly?.Value, displayedEnemy = _state?.DisplayedEnemy?.Value,
        enemyVisible = _enemy.Visible, actionKind = _state?.ActionKind, reactionKind = _state?.ReactionKind,
        reactionAmount = _state?.Amount,
        item = _state?.Item?.ItemId, spell = _state?.Spell?.Spell,
        spellAnimationSelector = _state?.IsAura == true ? (int?)null : _state?.Laser is not null ? 149 : _state?.Spell is not null ? 4 : _state?.Item is not null ? 0 : (int?)null,
        healing = _state?.Healing is { } healing ? new { healing.Caller, healing.Remaining, healing.AtTimedInput,
            healing.LogicalComplete, healing.Delivered, healing.CastRequested, healing.CastFrame, healing.Fairy } : null,
        fairySprites = _fairyBodies.Concat(_fairyWings).Concat(_fairyDust).Where(sprite => sprite.Visible)
            .Select(sprite => new { name = sprite.Name.ToString(), x = sprite.Position.X, y = sprite.Position.Y, mirror = sprite.FlipH,
                binding = BoundResource(sprite),
                width = sprite.GetRect().Size.X, height = sprite.GetRect().Size.Y }).ToArray(),
        allyFrame = _allyFrame, enemyFrame = _enemyFrame, allyResource = _allyResource, enemyResource = _enemyResource,
        weaponResource = _weaponResource, weaponVisible = _weapon.Visible, weaponFlipH = _weapon.FlipH, weaponFlipV = _weapon.FlipV,
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
    private static object BoundResource(Sprite2D node) => new
    {
        resource = node.HasMeta("bound_resource") ? node.GetMeta("bound_resource").AsString() : null,
        texturePresent = node.Texture is not null, visible = node.IsVisibleInTree(),
    };
    public override void _ExitTree()
    {
        GetWindow().FocusExited -= Suspend; GetWindow().FocusEntered -= Resume; foreach (var texture in _textures.Values) texture.Dispose(); _textures.Clear(); }
}
