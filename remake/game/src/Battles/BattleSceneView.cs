using System.Text.RegularExpressions;
using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.Audio;
using Sf2.Remake.GodotAdapter.Input;

namespace Sf2.Remake.GodotAdapter.Battles;

// One scene consumer. It projects admitted frames and acknowledges the session's
// wait token; it never computes damage, rewards, RNG or the next combatant.
internal sealed partial class BattleSceneView : Control
{
    private readonly Dictionary<string, ImageTexture> _textures = new(StringComparer.Ordinal);
    private Node2D _canvas = null!;
    private Sprite2D _background = null!, _backgroundWrap = null!, _ground = null!, _ally = null!, _enemy = null!, _weapon = null!;
    private Label _message = null!, _allyStatus = null!, _enemyStatus = null!;
    private GameSession _session = null!;
    private InputSettings _settings = null!;
    private SessionAudio? _audio;
    private BattleSceneState? _state;
    private BattleSceneActorVisual? _allyVisual, _enemyVisual;
    private BattleSceneAnimationFrame[] _frames = [];
    private double _elapsed, _revealed;
    private int _frameIndex, _allyFrame, _enemyFrame, _logicalReaction;
    private bool _completed;
    private bool _sceneMusicStarted, _hasWeapon;
    private string? _allyResource, _enemyResource, _weaponResource;
    internal string? Error { get; private set; }
    private BattleSceneDefinition? Content => _session.Definition.BattleScenes;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        ClipContents = true;
        var black = new ColorRect { Color = Colors.Black, MouseFilter = MouseFilterEnum.Ignore };
        black.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); AddChild(black);
        _canvas = new Node2D { Name = "SceneCanvas", TextureFilter = TextureFilterEnum.Nearest }; AddChild(_canvas);
        _background = Sprite("Background", new(0, 56));
        _backgroundWrap = Sprite("BackgroundWrap", new(256, 56));
        _enemy = Sprite("Enemy", new(16, 48));
        _ground = Sprite("Ground", new(136, 140));
        _ally = Sprite("Ally", new(136, 64)); _ally.ZIndex = 2;
        _weapon = Sprite("Weapon", new(136, 64));
        _allyStatus = Label("AllyStatus", new(136, 8), new(118, 40));
        _enemyStatus = Label("EnemyStatus", new(2, 8), new(126, 40));
        _message = Label("Message", new(8, 174), new(240, 48));
        Hide();
    }

    internal void Configure(GameSession session, InputSettings settings, SessionAudio? audio)
    { _session = session; _settings = settings; _audio = audio; }

    internal void Present()
    {
        var state = _session.Current.BattleScene;
        if (state is null) { Hide(); _state = null; return; }
        Show();
        if (_state?.Token == state.Token) return;
        _state = state; _elapsed = _revealed = 0; _frameIndex = _logicalReaction = -1; _completed = _sceneMusicStarted = false; _frames = [];
        try
        {
            if (_session.Definition.PrivateDefinitions is not null && Content is null)
                throw new InvalidOperationException("battle-scene-content-required");
            var battle = _session.Current.Battle;
            var actor = battle.GetActor(state.Actor); var target = battle.GetActor(state.Target);
            var ally = actor.IsAlly ? actor : target; var enemy = actor.IsAlly ? target : actor;
            if (Content is { } content)
            {
                if (content.Encounter != battle.Definition.Encounter || !content.Allies.TryGetValue(ally.Definition.ClassRule, out _allyVisual) ||
                    !content.Enemies.TryGetValue(enemy.Actor, out _enemyVisual)) throw new InvalidOperationException("battle-scene-actor-unavailable");
                Bind(_background, content.Background); Bind(_backgroundWrap, content.Background); Bind(_ground, content.Ground);
                _allyFrame = _enemyFrame = 0;
                var equipped = ally.SourceLoadout?.Items.FirstOrDefault(item => (item & 128) != 0 && (item & 127) != 127);
                _hasWeapon = equipped is > 0;
                if (_hasWeapon && (equipped!.Value & 127) != _allyVisual.Item) throw new InvalidOperationException("battle-scene-weapon-unavailable");
                SetFrame(true, 0); SetFrame(false, 0);
                Weapon(_allyVisual.Sequences["idle"].IdleWeapon, Vector2.Zero);
                _ground.Visible = ally.Definition.Mover is not (BattleMover.Hovering);
                if (state.Phase == BattleScenePhase.ActionAnimation)
                    _frames = (actor.IsAlly ? _allyVisual : _enemyVisual).Sequences["attack"].Frames.Skip(actor.IsAlly ? 1 : 0).ToArray();
                if (state.Phase == BattleScenePhase.Reaction && state.ReactionKind == "Dodge")
                    _frames = (target.IsAlly ? _allyVisual : _enemyVisual).Sequences["dodge"].Frames.Skip(target.IsAlly ? 1 : 0).ToArray();
            }
            _ally.Position = new(136, 64); _enemy.Position = new(16, 48);
            _background.Position = new(0,56); _backgroundWrap.Position = new(256,56); _ground.Position = new(136,140);
            _ally.Modulate = _enemy.Modulate = Colors.White;
            _allyStatus.Text = $"{ActorName(ally.Actor)}\nHP {ally.Hp}/{ally.MaxHp}   MP {ally.Mp}";
            _enemyStatus.Text = $"{ActorName(enemy.Actor)}\nHP {enemy.Hp}/{enemy.MaxHp}   MP {enemy.Mp}";
            _message.Text = Message(state);
            _message.VisibleCharacters = _settings.TextMode == "instant" ? -1 : 0;
            if (state.Phase == BattleScenePhase.Initialize) _audio?.BeginBattleScene(actor.IsAlly ? 2 : 5);
            if (state.Phase == BattleScenePhase.Reaction && state.ReactionKind == "Damage") _audio?.PlayEffect(target.IsAlly ? 81 : 83);
            if (state.GrowthNotice is { Kind: BattleGrowthNoticeKind.Level }) _audio?.PlayEffect(102);
            if (state.Phase == BattleScenePhase.End) _audio?.BeginSceneEnd();
        }
        catch (InvalidOperationException error) { Error = error.Message; }
    }

    internal SessionCommand? Consume(double delta)
    {
        if (_state is not { } state || Error is not null || _completed) return null;
        var viewport = GetViewportRect().Size;
        float scale = MathF.Min(viewport.X / 256, viewport.Y / 224);
        _canvas.Scale = new(scale, scale); _canvas.Position = (viewport - new Vector2(256, 224) * scale) / 2;
        _elapsed += delta;
        if (state.RequiresAcknowledgement)
        {
            _revealed += delta * _settings.CharactersPerSecond;
            if (_message.VisibleCharacters >= 0) _message.VisibleCharacters = (int)_revealed;
            return null;
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
                bool ally = _session.Current.Battle.GetActor(state.Phase == BattleScenePhase.ActionAnimation ? state.Actor : state.Target).IsAlly;
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
            bool ally = _session.Current.Battle.GetActor(state.Target).IsAlly;
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
        if (action != GameAction.Confirm || _state is not { RequiresAcknowledgement: true } state || Error is not null) return null;
        if (_message.VisibleCharacters >= 0 && _message.VisibleCharacters < _message.GetTotalCharacterCount())
        { _revealed = _message.GetTotalCharacterCount(); _message.VisibleCharacters = (int)_revealed; return null; }
        return new Acknowledge(state.Token);
    }

    private string ActorName(ActorRef actor)
    {
        if (Content is not { } content) return actor.Value;
        var state = _session.Current.Battle.GetActor(actor);
        return state.IsAlly ? content.MemberNames[state.ProcessingOrder] : _session.Definition.PrivateDefinitions!.Enemies["GIZMO"].DisplayName;
    }

    private string Message(BattleSceneState state)
    {
        bool ally = _session.Current.Battle.GetActor(state.Actor).IsAlly;
        int id = state.Phase switch
        {
            BattleScenePhase.ActionMessage => state.ActionKind switch { "physical-second" => 293, "physical-counter" => 292, _ => 273 },
            BattleScenePhase.ResultMessage => state.ReactionKind == "Dodge" ? 286 : state.Critical ? (ally ? 287 : 288) : (ally ? 284 : 285),
            BattleScenePhase.DeathMessage => _session.Current.Battle.GetActor(state.Target).IsAlly ? 291 : 290,
            BattleScenePhase.RewardMessage => 263, BattleScenePhase.GoldMessage => 393, _ => -1,
        };
        if (state.GrowthNotice is { } notice)
            id = notice.Kind switch { BattleGrowthNoticeKind.Level => 244, BattleGrowthNoticeKind.MaxHp => 266,
                BattleGrowthNoticeKind.MaxMp => 267, BattleGrowthNoticeKind.Attack => 268, BattleGrowthNoticeKind.Defense => 269,
                BattleGrowthNoticeKind.Agility => 270, BattleGrowthNoticeKind.Spell => 271, _ => -1 };
        if (id < 0) return "";
        string name = ActorName(state.Phase == BattleScenePhase.ActionMessage ? state.Actor :
            state.Phase is BattleScenePhase.RewardMessage or BattleScenePhase.GrowthMessage ? state.RewardActor!.Value : state.Target);
        int amount = state.GrowthNotice?.Amount ?? (state.Phase == BattleScenePhase.RewardMessage ? state.RewardAmount : state.Phase == BattleScenePhase.GoldMessage ? state.GoldAmount : state.Amount);
        string text = Content?.Texts.GetValueOrDefault(id) ?? (state.Phase == BattleScenePhase.ActionMessage ? "{NAME}'s attack!" :
            state.Phase == BattleScenePhase.RewardMessage ? "{NAME} earned {#} EXP." : state.Phase == BattleScenePhase.GoldMessage ? "Found {#} gold." : "{NAME}: {#}");
        // Text timing remains the existing product setting. No battle speech bleeps:
        // source bsc10/bsc11 clear CURRENT_SPEECH_SFX.
        return Regex.Replace(text.Replace("{NAME}", name).Replace("{#}", amount.ToString()).Replace("{N}", "\n")
            .Replace("{SPELL}", state.GrowthNotice?.Spell?.Value.ToUpperInvariant() ?? ""), @"\{D[0-9]+\}", "");
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
        if (!_textures.TryGetValue(resource, out var texture))
        {
            var raster = Content!.Rasters[resource]; using var image = new Image();
            if (image.LoadPngFromBuffer(raster.CopyBytes()) != Godot.Error.Ok || image.GetWidth() != raster.Width || image.GetHeight() != raster.Height)
                throw new InvalidOperationException("battle-scene-texture-invalid");
            texture = ImageTexture.CreateFromImage(image); _textures.Add(resource, texture);
        }
        node.Texture = texture; node.Scale = new(0.5f,0.5f);
    }
    private Sprite2D Sprite(string name, Vector2 position)
    { var node = new Sprite2D { Name = name, Position = position, Centered = false }; _canvas.AddChild(node); return node; }
    private Label Label(string name, Vector2 position, Vector2 size)
    {
        var label = new Label { Name = name, Position = position, Size = size, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        label.AddThemeFontSizeOverride("font_size", 9); _canvas.AddChild(label); return label;
    }
    internal object Observe() => new
    {
        visible = Visible, error = Error, phase = _state?.Phase.ToString(), waitToken = _state?.Token.Value,
        elapsed = _elapsed, completed = _completed, frameIndex = _frameIndex, reactionState = _logicalReaction,
        allyFrame = _allyFrame, enemyFrame = _enemyFrame, allyResource = _allyResource, enemyResource = _enemyResource,
        weaponResource = _weaponResource, weaponVisible = _weapon.Visible, weaponFlipH = _weapon.FlipH, weaponFlipV = _weapon.FlipV,
        textureCount = _textures.Count, reducedFlash = _settings?.ReducedFlash, message = _message.Text,
        visibleCharacters = _message.VisibleCharacters, allyStatus = _allyStatus.Text, enemyStatus = _enemyStatus.Text,
        allyX = _ally.Position.X, allyY = _ally.Position.Y, enemyX = _enemy.Position.X, enemyY = _enemy.Position.Y,
        backgroundX = _background.Position.X, groundX = _ground.Position.X, weaponX = _weapon.Position.X,
    };
    public override void _ExitTree() { foreach (var texture in _textures.Values) texture.Dispose(); }
}
