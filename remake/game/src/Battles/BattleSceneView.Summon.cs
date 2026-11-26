using Godot;
using Sf2.Remake.Application.Runtime.Battles;

namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed partial class BattleSceneView
{
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
