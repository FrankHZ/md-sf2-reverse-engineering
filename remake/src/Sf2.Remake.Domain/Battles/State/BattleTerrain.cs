namespace Sf2.Remake.Domain.Battles;

public enum TerrainSurface { Open, Brush, Rough, Deep, Impassable, Barrier }
public enum TerrainProtection { None, Light, Heavy }

// Immutable authored ground properties; mover rules and presentation interpret them independently.
public sealed record BattleTerrain(TerrainSurface Surface, TerrainProtection Protection);
