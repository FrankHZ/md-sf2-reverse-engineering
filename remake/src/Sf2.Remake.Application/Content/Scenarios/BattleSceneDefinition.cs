using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Content.Scenarios;

public sealed record BattleSceneWeaponFrame(int Frame, int Layer, int X, int Y);
public sealed record BattleSceneAnimationFrame(int Frame, int Ticks, int X, int Y, BattleSceneWeaponFrame? Weapon);
public sealed record BattleSceneAnimation(int Index, int Trigger, int Spell, bool Terminate,
    BattleSceneWeaponFrame? IdleWeapon, IReadOnlyList<BattleSceneAnimationFrame> Frames);
public sealed record BattleSceneActorVisual(int Sprite, int Palette, int? Item,
    IReadOnlyList<string> Frames, IReadOnlyList<string> WeaponFrames,
    IReadOnlyDictionary<string, BattleSceneAnimation> Sequences, int IdleTicks = 0);
public sealed record HealingSceneVisual(IReadOnlyList<string> Bodies, IReadOnlyList<string> Wings, IReadOnlyList<string> Dust);
public sealed record BattleSceneDefinition(string Encounter, string Background, string Ground,
    IReadOnlyDictionary<string, ExplorationRaster> Rasters,
    IReadOnlyDictionary<BattleClassRule, BattleSceneActorVisual> Allies,
    IReadOnlyDictionary<ActorRef, BattleSceneActorVisual> Enemies,
    IReadOnlyDictionary<int, string> Texts, IReadOnlyList<string> MemberNames, HealingSceneVisual? Healing = null);
