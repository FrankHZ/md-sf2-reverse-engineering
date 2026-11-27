using Godot;
using Sf2.Remake.Application.Runtime.Battles;

namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed partial class BattleSceneView
{
    private void ProjectHealing(BattleSceneState state)
    {
        foreach (var sprite in _fairyBodies.Concat(_fairyWings).Concat(_fairyDust)) sprite.Hide();
        if (state.Healing is not { } healing) return;
        bool ally = _session.Current.Battle.GetActor(state.Actor).IsAlly;
        var recipient = ally ? _ally : _enemy;
        var origin = ally ? new Vector2(136, 64) : new Vector2(16, 48);
        if (healing.CastRequested && !_castSoundPlayed) { _audio?.PlayBattleEffect(77); _castSoundPlayed = true; }
        if (state.Phase == BattleScenePhase.ActionAnimation && healing.CastFrame is > 0 and var frame && frame <= _frames.Length)
        {
            _frameIndex = frame - 1;
            var entry = _frames[_frameIndex];
            if (entry.Frame != 15) SetFrame(ally, entry.Frame);
            recipient.Position = origin + new Vector2(entry.X, entry.Y);
            if (ally) Weapon(entry.Weapon, new(entry.X, entry.Y));
        }
        recipient.Modulate = !_settings.ReducedFlash && healing.Caller == "cast-flash-on" ? new Color(0.7f, 1, 0.7f) : Colors.White;
        if (state.Phase is BattleScenePhase.TargetExit or BattleScenePhase.TargetEnter or BattleScenePhase.ActorExit or BattleScenePhase.ActorEnter)
        {
            float from = state.Phase == BattleScenePhase.TargetEnter ? -376 : state.Phase == BattleScenePhase.ActorEnter ? 8 : 136;
            float to = state.Phase == BattleScenePhase.TargetExit ? 272 : state.Phase == BattleScenePhase.ActorExit ? -112 : 136;
            int opportunity = healing.Caller?.EndsWith(":wait", StringComparison.Ordinal) == true ? 0 : healing.Opportunity;
            float x = from + MathF.Sign(to - from) * MathF.Min(opportunity * 16, MathF.Abs(to - from));
            recipient.Position = ally ? new(x, 64) : new(origin.X - (x - 136), origin.Y);
            if (ally) { _ground.Position = new(x, 140); Weapon(_allyVisual?.Sequences["idle"].IdleWeapon, new(x - 136, 0)); }
        }
        if (Content?.Healing is not { } visual || healing.Fairy is not { Control: not 0 } fairy) return;
        visual = !ally && visual.Enemy is { } enemyVisual ? enemyVisual : visual;
        bool groupHasHd = BattleSceneRaster.HasHdGroup(Content, visual.Bodies.Concat(visual.Wings).Concat(visual.Dust));
        void Put(Sprite2D sprite, string resource, ushort x, ushort y, bool mirror = false)
        {
            Bind(sprite, resource, groupHasHd); sprite.FlipH = mirror;
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

    private static int? DetoxSelector(BattleSceneState? state) =>
        state is { IsSilencedCast: false, RecoveryEffect: { CuresPoison: true, Spell: { Value: "detox", Level: >= 1 and <= 4 } spell } }
            ? 6 + 32 * (spell.Level - 1) : null;

}
