namespace GodotProbe.Domain;

/// <summary>
/// Deterministic turn-based grid simulation with no Godot dependencies.
/// Seeded output is reproducible only within the controlled probe runtime.
/// System.Random is not a cross-runtime gameplay or remake determinism contract.
/// </summary>
public sealed class SimState
{
    public int Seed { get; }
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Score { get; private set; }

    private readonly System.Random _rng;

    public SimState(int seed)
    {
        Seed = seed;
        _rng = new System.Random(seed);
    }

    /// <summary>Advance one turn: move to a new cell and accumulate a score.</summary>
    public void Step()
    {
        X = _rng.Next(0, 8);
        Y = _rng.Next(0, 8);
        Score += X + Y;
    }
}
