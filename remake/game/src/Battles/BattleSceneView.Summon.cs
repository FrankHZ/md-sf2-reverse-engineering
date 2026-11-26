using Godot;
using Sf2.Remake.Application.Runtime.Battles;

namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed partial class BattleSceneView
{
    private AtlasEffectView _atlas = null!;
    private NeptunEffectView _neptun = null!;
    private ApolloEffectView _apollo = null!;
    private void CreateSummons()
    {
        _atlas = new AtlasEffectView { Name = "AtlasEffect", ZIndex = 4 }; _canvas.AddChild(_atlas);
        _neptun = new NeptunEffectView { Name = "NeptunEffect", ZIndex = 4 }; _canvas.AddChild(_neptun);
        _dao = new DaoEffectView { Name = "DaoEffect", ZIndex = 4 }; _canvas.AddChild(_dao);
        _apollo = new ApolloEffectView { Name = "ApolloEffect", ZIndex = 4 }; _canvas.AddChild(_apollo);
    }
    private void ResetSummons() { _atlas.Reset(); _dao.Reset(); _apollo.Reset(); _neptun.Reset(); _enemy.Material = null; }
    private void PresentSummons(BattleSceneState state)
    {
        _atlas.Present(state); _dao.Present(state); _apollo.Present(state); _neptun.Present(state); _enemy.Material = null;
    }
    private bool PreparedSummon(BattleSceneState state) => PreparedAtlas(state) || PreparedDao(state) || PreparedApollo(state) || PreparedNeptun(state);
    private bool PreparedAtlas(BattleSceneState state)
    {
        if (state.AttackAnimationSelector is not (20 or 52) || state.IsSilencedCast || Content is not { } content)
            return false;
        int available = AtlasEffectView.Resources.Count(content.Rasters.ContainsKey);
        if (available != 0 && available != AtlasEffectView.Resources.Length)
            throw new InvalidOperationException("atlas-original-group-incomplete");
        return available != 0;
    }
    private bool PreparedNeptun(BattleSceneState state)
    {
        if (state.AttackAnimationSelector is not (19 or 51) || state.IsSilencedCast || Content is not { } content)
            return false;
        int available = NeptunEffectView.Resources.Count(content.Rasters.ContainsKey);
        if (available != 0 && available != NeptunEffectView.Resources.Length)
            throw new InvalidOperationException("neptun-original-group-incomplete");
        return available != 0;
    }
    private bool PreparedApollo(BattleSceneState state)
    {
        if (state.AttackAnimationSelector is not (18 or 50) || state.IsSilencedCast || Content is not { } content)
            return false;
        int available = ApolloEffectView.Resources.Count(content.Rasters.ContainsKey);
        if (available != 0 && available != ApolloEffectView.Resources.Length)
            throw new InvalidOperationException("apollo-original-group-incomplete");
        return available != 0;
    }
    private void BeginSummon(BattleSceneState state, double trigger)
    {
        BeginDao(state, trigger);
        if (PreparedAtlas(state))
            _atlas.Begin(Frames(AtlasEffectView.Resources), trigger,
                (state.AttackAnimationSelector!.Value >> 5) + 1, _settings.ReducedFlash);
        if (PreparedNeptun(state))
            _neptun.Begin(Frames(NeptunEffectView.Resources), trigger,
                (state.AttackAnimationSelector!.Value >> 5) + 1, _settings.ReducedFlash);
        if (PreparedApollo(state))
            _apollo.Begin(Frames(ApolloEffectView.Resources), trigger,
                (state.AttackAnimationSelector!.Value >> 5) + 1, _settings.ReducedFlash);
    }
    private void AdvanceSummons(double delta)
    {
        if (_atlas.Active) _atlas.Advance(delta, cue => _audio?.PlayEffect(cue));
        if (_dao.Active) _dao.Advance(delta, cue => _audio?.PlayEffect(cue));
        if (_apollo.Active) _apollo.Advance(delta, cue => _audio?.PlayEffect(cue));
        if (_neptun.Active) _neptun.Advance(delta, cue => _audio?.PlayEffect(cue));
        ProjectSummons();
    }
    private double SummonDuration(double duration) => _dao.Active ? Math.Max(duration, _dao.CastDuration)
        : _apollo.Active ? Math.Max(duration, _apollo.CastDuration)
        : _neptun.Active ? Math.Max(duration, _neptun.CastDuration)
        : _atlas.Active ? Math.Max(duration, _atlas.CastDuration) : duration;
    private void ProjectSummons()
    {
        ProjectDao();
        if (_atlas.Active)
        {
            if (_atlas.ReplacesCaster) { _ally.Hide(); _weapon.Hide(); _ground.Hide(); }
            _background.Position = new Vector2(0, 56) + _atlas.Shake;
            _backgroundWrap.Position = new Vector2(256, 56) + _atlas.Shake;
            _sceneBlack.Color = !_settings.ReducedFlash && _atlas.Flash ? new Color(1, 12f/14, 12f/14) : Colors.Black;
        }
        if (_neptun.Active)
        {
            if (_neptun.ReplacesCaster) { _ally.Hide(); _weapon.Hide(); _ground.Hide(); }
            _background.Position = new Vector2(0, 56) + _neptun.Shake;
            _backgroundWrap.Position = new Vector2(256, 56) + _neptun.Shake;
            _sceneBlack.Color = !_settings.ReducedFlash && _neptun.Flash ? new Color(1, 12f/14, 12f/14) : Colors.Black;
        }
        if (!_apollo.Active) return;
        if (_apollo.ReplacesCaster) { _ally.Hide(); _weapon.Hide(); _ground.Hide(); }
        _background.Position = new Vector2(0, 56) + _apollo.Shake;
        _backgroundWrap.Position = new Vector2(256, 56) + _apollo.Shake;
        _sceneBlack.Color = _settings.ReducedFlash ? Colors.Black : _apollo.Flash ? new Color(1, 12f/14, 12f/14)
            : _apollo.FlameFlash ? new Color(1, 0, 0) : Colors.Black;
    }
    private DaoEffectView _dao = null!;
    private readonly ShaderMaterial _daoDistortion = new()
    {
        Shader = new Shader { Code = """
            shader_type canvas_item;
            uniform float tick;
            uniform vec2 source_size;
            void fragment() {
                // Source modification3 alternates its accumulating displacement
                // every20 updates. Project it on the existing target texture.
                float cycle = mod(tick, 40.0);
                float amount = cycle < 20.0 ? cycle : 40.0 - cycle;
                vec2 p = UV * source_size;
                p.x += (p.y - source_size.y * 0.5) * amount / 128.0;
                p.y += (p.x - source_size.x * 0.5) * amount / 256.0;
                vec2 uv = p / source_size;
                COLOR = uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0
                    ? vec4(0.0) : texture(TEXTURE, uv);
            }
            """ }
    };
    private bool PreparedDao(BattleSceneState state)
    {
        if (state.AttackAnimationSelector is not (17 or 49) || state.IsSilencedCast || Content is not { } content)
            return false;
        int available = DaoEffectView.Resources.Count(content.Rasters.ContainsKey);
        if (available != 0 && available != DaoEffectView.Resources.Length)
            throw new InvalidOperationException("dao-original-group-incomplete");
        return available != 0;
    }
    private void BeginDao(BattleSceneState state, double trigger)
    {
        if (PreparedDao(state))
            _dao.Begin(Frames(DaoEffectView.Resources), trigger,
                (state.AttackAnimationSelector!.Value >> 5) + 1, _settings.ReducedFlash);
    }
    private void ProjectDao()
    {
        if (!_dao.Active) return;
        // LoadInvocationSpell replaces the ally body/weapon and ground with
        // invocation0. Restore is owned by the next ordinary scene projection.
        if (_dao.ReplacesCaster) { _ally.Hide(); _weapon.Hide(); _ground.Hide(); }
        var shift = _dao.Shake;
        _background.Position = new Vector2(0, 56) + shift;
        _backgroundWrap.Position = new Vector2(256, 56) + shift;
        _enemy.Material = _dao.DistortionTick >= 0 ? _daoDistortion : null;
        if (_enemy.Material is not null)
        {
            _daoDistortion.SetShaderParameter("tick", _dao.DistortionTick);
            _daoDistortion.SetShaderParameter("source_size", _enemy.GetRect().Size * _enemy.Scale);
        }
        _sceneBlack.Color = _settings.ReducedFlash ? Colors.Black : _dao.Flash ? new Color(1, 12f/14, 12f/14)
            : _dao.RockFlash ? new Color(8f/14, 4f/14, 2f/14) : Colors.Black;
        if (!_dao.ReplacesCaster)
            _ally.Modulate = Colors.White;
    }
}
