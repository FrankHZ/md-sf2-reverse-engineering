using Godot;
using GodotProbe.Domain;

namespace GodotProbe;

/// <summary>
/// Thin Godot adapter: owns the domain state, drives it from the game loop,
/// mirrors state to scene nodes, and quits after a bounded number of frames.
/// </summary>
public partial class Player : Node2D
{
    private SimState _sim = null!;
    private int _frame;

    public override void _Ready()
    {
        _sim = new SimState(seed: 42);
        GD.Print($"PROBE_READY seed={_sim.Seed}");
    }

    public override void _Process(double delta)
    {
        _frame++;
        _sim.Step();
        Position = new Vector2(10 + _sim.X * 8, 20 + _sim.Y * 8);

        var hud = GetNode<Label>("Hud");
        hud.Text = $"t={_frame} x={_sim.X} y={_sim.Y} score={_sim.Score}";

        if (_frame == 60)
        {
            GD.Print($"PROBE_DONE frames={_frame} x={_sim.X} y={_sim.Y} score={_sim.Score}");
            GetTree().Quit();
        }
    }
}
