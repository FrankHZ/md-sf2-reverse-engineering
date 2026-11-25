using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Content.Scenarios;

public sealed record BattleSceneWeaponFrame(int Frame, int Layer, int X, int Y);
public sealed record BattleSceneAnimationFrame(int Frame, int Ticks, int X, int Y, BattleSceneWeaponFrame? Weapon);
public sealed record BattleSceneAnimation(int Index, int Trigger, int Spell, bool Terminate,
    BattleSceneWeaponFrame? IdleWeapon, IReadOnlyList<BattleSceneAnimationFrame> Frames);
public sealed record BattleSceneActorVisual(int Sprite, int Palette, int? Item,
    IReadOnlyList<string> Frames, IReadOnlyList<string> WeaponFrames,
    IReadOnlyDictionary<string, BattleSceneAnimation> Sequences, int IdleTicks = 0, byte? SourceClass = null);
public sealed record HealingSceneVisual(IReadOnlyList<string> Bodies, IReadOnlyList<string> Wings, IReadOnlyList<string> Dust,
    HealingSceneVisual? Enemy = null);
public sealed record NeutralBattleSprite(int Sprite, int X, int Y, int Facing, int CompletedFlag, bool WhirlsInWater = false, bool LyingLeft = false);
public sealed record FieldDeathVisual(IReadOnlyDictionary<int, int> AllySprites,
    IReadOnlyDictionary<ActorRef, int> EnemySprites, IReadOnlyList<string> ExitFrames, IReadOnlyList<NeutralBattleSprite>? Neutrals = null);
public sealed record BattleSceneTerrainVisual(string Background, string Ground);
public sealed record EnemyAppearance(string Name, BattleSceneActorVisual Body, int MapSprite);
public sealed record BattleSceneDefinition(string Encounter, string Background, string Ground,
    IReadOnlyDictionary<string, ExplorationRaster> Rasters,
    IReadOnlyDictionary<BattleClassRule, BattleSceneActorVisual> Allies,
    IReadOnlyDictionary<ActorRef, BattleSceneActorVisual> Enemies,
    IReadOnlyDictionary<int, string> Texts, IReadOnlyList<string> MemberNames, HealingSceneVisual? Healing = null, FieldDeathVisual? FieldDeath = null,
    IReadOnlyList<BattleSceneTerrainVisual>? TerrainScenes = null,
    IReadOnlyDictionary<ActorRef, string>? EnemyNames = null,
    IReadOnlyDictionary<(BattleClassRule Class, int Item), BattleSceneActorVisual>? Equipment = null,
    IReadOnlyDictionary<ActorRef, byte>? EnemyNameIds = null, IReadOnlyDictionary<byte, EnemyAppearance>? Species = null,
    IReadOnlyDictionary<(ActorRef Actor, BattleClassRule Class, int Item), BattleSceneActorVisual>? AllyAppearances = null,
    IReadOnlyDictionary<string, ExplorationRaster>? HdRasters = null)
{
    public bool TryAllyVisual(BattleActorState actor, out BattleSceneActorVisual visual)
    {
        byte? weapon = BattleEquipment.EquippedWeaponItem(actor);
        if (AllyAppearances is not null && AllyAppearances.Keys.Any(key => key.Actor == actor.Actor && key.Class == actor.Definition.ClassRule))
            return AllyAppearances.TryGetValue((actor.Actor, actor.Definition.ClassRule, weapon is { } item ? item : -1), out visual!);
        if (Equipment is not null && weapon is { } equipped)
            return Equipment.TryGetValue((actor.Definition.ClassRule, equipped), out visual!);
        return Allies.TryGetValue(actor.Definition.ClassRule, out visual!);
    }
}
