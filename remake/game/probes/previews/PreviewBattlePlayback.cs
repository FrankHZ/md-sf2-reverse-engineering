using Godot;
using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.GodotAdapter.UI;

namespace Sf2.Remake.GodotAdapter.Previews;

// A staged review timeline on a 256×224 logical canvas. The admitted poses own their frame durations.
internal sealed partial class PreviewBattlePlayback(PreviewStage assets) : Control
{
    private const float Zoom = 4;
    private const double Windup = 0.75;
    private BattleSceneActorVisual? _caster, _bowie, _opponent;
    private BattleSceneAnimationFrame[] _poses = [];
    private double _time, _poseEnd;
    private bool _heal, _self, _english, _steady;
    private int _startHp, _startMp, _cost;
    private double Impact => _poseEnd + (_heal ? 1.5 : 0);
    private double End => Impact + (_heal && !_self ? 1.8 : 1.1);
    internal bool Finished => _time >= End;
    internal int FinalHp => _heal ? Math.Min(24, _startHp + 8) : Math.Max(0, _startHp - 4);
    internal int FinalMp => _startMp - _cost;
    internal int ShownHp => _time < Impact ? _startHp : FinalHp;
    internal int ShownMp => _time < Windup ? _startMp : FinalMp;
    private readonly MiniWindow _ally = new() { Name = "AllyWindow" }, _enemy = new() { Name = "EnemyWindow" };
    private readonly Node2D _canvas = new() { Name = "Art", Scale = Vector2.One * Zoom };
    private readonly Dictionary<string, Sprite2D> _layers = [];
    private bool HasArt => assets.SceneArt is not null && _caster is not null && _bowie is not null && _opponent is not null;

    public override void _Ready()
    {
        Name = "BattlePlayback";
        Position = new(448, 80); Size = new(1024, 896);
        ClipContents = true; MouseFilter = MouseFilterEnum.Ignore; TextureFilter = TextureFilterEnum.Nearest;
        Theme = StoryTheme.Create();
        AddChild(_canvas); AddChild(_ally); AddChild(_enemy);
        _ally.ZIndex = _enemy.ZIndex = 8;
        Hide();
    }

    internal void Begin(bool heal, bool self, int hp, int mp, int cost)
    {
        _heal = heal; _self = self; _startHp = hp; _startMp = mp; _cost = cost; _time = 0;
        var art = assets.SceneArt;
        _caster = art?.Allies.GetValueOrDefault(BattleClassRule.UnpromotedPriest);
        _bowie = art?.Allies.GetValueOrDefault(BattleClassRule.UnpromotedSwordsman);
        _opponent = art?.Enemies.Values.FirstOrDefault();
        _poses = _caster?.Sequences[heal ? "cast" : "attack"].Frames.Skip(1).ToArray() ?? [];
        _poseEnd = Windup + (_poses.Length == 0 ? 0.65 : _poses.Sum(f => f.Ticks) / 60.0);
    }

    internal void Advance(double delta, bool english, bool steady, int sarahHp, int bowieMp)
    {
        _english = english; _steady = steady;
        _time = Math.Min(End, _time + delta);
        bool onBowie = OnBowie;
        int mp = ShownMp, hp = ShownHp;
        _ally.Display(onBowie ? Word("鲍伊", "Bowie") : Word("莎拉", "Sarah"),
            onBowie || (_heal && _self) ? hp : sarahHp, 24, onBowie ? bowieMp : mp, onBowie ? 0 : 12,
            left: onBowie, lower: false);
        _enemy.Visible = !_heal;
        if (!_heal) _enemy.Display(Word("基兹莫", "Gizmo"), hp, 24, 0, 0, left: true, lower: true);
        if (HasArt) PresentArt();
        QueueRedraw();
    }

    private bool OnBowie => _heal && !_self && _time >= _poseEnd + 0.35 && _time < Impact + 1.45;
    private string Word(string zh, string en) => _english ? en : zh;
    internal string Message
    {
        get
        {
            if (_time < Impact)
                return _heal ? Word($"莎拉施放回复，消耗 {_cost} MP。", $"Sarah casts HEAL for {_cost} MP.")
                    : Word("莎拉挥动木杖！", "Sarah strikes with her wooden rod!");
            if (!_heal) return Word($"基兹莫受到 {_startHp - FinalHp} 点伤害。", $"Gizmo takes {_startHp - FinalHp} damage.");
            string name = _self ? Word("莎拉", "Sarah") : Word("鲍伊", "Bowie");
            return Word($"{name}恢复了 {FinalHp - _startHp} HP。", $"{name} recovers {FinalHp - _startHp} HP.");
        }
    }

    private void Raster(string layer, string id, Vector2 at, int z, bool flipH = false, bool flipV = false)
    {
        if (!_layers.TryGetValue(layer, out var sprite))
        {
            _layers[layer] = sprite = new Sprite2D { Name = layer, Centered = false, Scale = Vector2.One / 2 };
            _canvas.AddChild(sprite);
        }
        sprite.Texture = assets.BattleTexture(id);
        sprite.Position = at.Round(); sprite.ZIndex = z; sprite.FlipH = flipH; sprite.FlipV = flipV;
        sprite.Show();
    }

    private BattleSceneAnimationFrame? Pose()
    {
        if (_time < Windup || _poses.Length == 0 || _time > _poseEnd + 0.14) return null;
        double remaining = (_time - Windup) * 60;
        foreach (var pose in _poses)
        {
            if (remaining < pose.Ticks) return pose;
            remaining -= pose.Ticks;
        }
        return _poses[^1];
    }

    private float AllyX()
    {
        if (_time < Windup) return 136 + 22 * (1 - (float)(_time / Windup));
        if (!_heal || _self) return 136;
        // Leave right, enter left as the recipient, then return to the caster on the right.
        if (_time < _poseEnd) return 136;
        if (_time < _poseEnd + 0.35) return Mathf.Lerp(136, 272, (float)((_time - _poseEnd) / 0.35));
        if (_time < _poseEnd + 0.75) return Mathf.Lerp(-100, 24, (float)((_time - _poseEnd - 0.35) / 0.4));
        if (_time < Impact + 1.1) return 24;
        if (_time < Impact + 1.45) return Mathf.Lerp(24, -100, (float)((_time - Impact - 1.1) / 0.35));
        return Mathf.Lerp(272, 136, Mathf.Clamp((float)((_time - Impact - 1.45) / 0.35), 0, 1));
    }

    private void DrawAlly(BattleSceneActorVisual actor, Vector2 at, bool mirror)
    {
        var pose = mirror ? null : Pose();
        int frame = pose?.Frame ?? 0;
        if (frame == 15) frame = 0;
        at += new Vector2(pose?.X ?? 0, pose?.Y ?? 0);
        var weapon = pose is null ? actor.Sequences["idle"].IdleWeapon : pose.Weapon;
        void DrawWeapon()
        {
            if (weapon is null || actor.WeaponFrames.Count == 0) return;
            string id = actor.WeaponFrames[weapon.Frame & 7];
            float dx = mirror ? (assets.BattleTexture(actor.Frames[frame]).GetWidth() -
                assets.BattleTexture(id).GetWidth()) / 2f - weapon.X : weapon.X;
            Raster("Weapon", id, at + new Vector2(dx, weapon.Y), weapon.Layer == 2 ? 4 : 2,
                ((weapon.Frame & 16) != 0) ^ mirror, (weapon.Frame & 32) != 0);
        }
        Raster("Ally", actor.Frames[frame], at, 3, mirror);
        DrawWeapon();
    }

    private float Recoil => !_steady && !_heal && _time >= Impact && _time < Impact + 0.3 ?
        MathF.Round(3 * MathF.Sin((float)(_time - Impact) * 35)) : 0;
    private bool FairyVisible => _heal && _time >= _poseEnd + 0.75 && _time < Impact + 1.05;
    private float FairyTime => (float)(_time - _poseEnd - 0.75);
    private Vector2 FairyPosition => new(AllyX() + 40 + 21 * MathF.Cos(FairyTime * 4), 60 + 8 * MathF.Sin(FairyTime * 5));

    private void PresentArt()
    {
        foreach (var sprite in _layers.Values) sprite.Hide();
        var art = assets.SceneArt!;
        float allyX = AllyX();
        Raster("Background", art.Background, new(0, 56), 0);
        if (!_heal) Raster("Enemy", _opponent!.Frames[0], new(16 + Recoil, 48), 1);
        Raster("Ground", art.Ground, new(allyX, 140), 1, OnBowie);
        DrawAlly(OnBowie ? _bowie! : _caster!, new(allyX, 64), OnBowie);
        if (FairyVisible && art.Healing is { } effect)
        {
            var p = FairyPosition;
            int flap = (int)(FairyTime * 7) % 2;
            Raster("Wings", effect.Wings[flap], p, 5);
            Raster("Fairy", effect.Bodies[flap], p, 6);
            for (int i = 0; i < 4; i++)
                Raster($"Dust{i}", effect.Dust[(flap + i) % effect.Dust.Count],
                    p + new Vector2(i * 6, 22 + (FairyTime * 34 + i * 9) % 34), 5);
        }
    }

    public override void _Draw()
    {
        DrawSetTransform(Vector2.Zero, 0, Vector2.One * Zoom);
        DrawRect(new(0, 0, 256, 224), Colors.Black);
        float allyX = AllyX();
        if (!HasArt)
        {
            DrawRect(new(0, 56, 256, 96), new Color("294350"));
            void Figure(float x, Color color)
            {
                DrawCircle(new(x + 24, 98), 11, color);
                DrawRect(new(x + 12, 110, 24, 33), color);
            }
            Figure(allyX, OnBowie ? new Color("a7c4df") : new Color("dfccae"));
            if (!_heal) Figure(16 + Recoil, new Color("b69bbe"));
            if (!OnBowie) DrawLine(new(allyX + 12, 123), new(allyX + (_time >= Windup && _time <= _poseEnd ? -15 : 6), 91),
                new Color("c7a777"), 3);
        }

        if (!HasArt && FairyVisible) DrawCircle(FairyPosition, 5, new Color("e0f2b9"));
        DrawSetTransform(Vector2.Zero, 0, Vector2.One);
    }

    private sealed partial class MiniWindow : Panel
    {
        private readonly Label _name = new(), _hp = new(), _mp = new();
        private readonly BattleGaugeImage _hpBar = new("HP"), _mpBar = new("MP");
        public override void _Ready()
        {
            Size = new(384, 164); MouseFilter = MouseFilterEnum.Ignore;
            void Line(Label label, int y, int width, int font)
            {
                label.Position = new(16, y); label.Size = new(width, 48);
                label.VerticalAlignment = VerticalAlignment.Center;
                label.AddThemeFontSizeOverride("font_size", font); AddChild(label);
            }
            Line(_name, 6, 352, 30); Line(_hp, 55, 170, 26); Line(_mp, 103, 170, 26);
            foreach (var (bar, y) in new[] { (_hpBar, 68), (_mpBar, 116) })
            { bar.Position = new(192, y); bar.Size = new(172, 24); AddChild(bar); }
        }
        internal void Display(string name, int hp, int maxHp, int mp, int maxMp, bool left, bool lower)
        {
            // x = 1 or 31 - window width, y = 1 or 20, in eight-pixel tiles.
            Position = new(left ? 32 : 992 - Size.X, lower ? 640 : 32);
            _name.Text = name; _hp.Text = $"HP  {hp}/{maxHp}";
            _mp.Text = maxMp == 0 ? "MP  —" : $"MP  {mp}/{maxMp}";
            _hpBar.Fraction = (float)hp / maxHp; _mpBar.Fraction = maxMp == 0 ? 0 : (float)mp / maxMp;
            _hpBar.QueueRedraw(); _mpBar.QueueRedraw();
        }
    }
}
