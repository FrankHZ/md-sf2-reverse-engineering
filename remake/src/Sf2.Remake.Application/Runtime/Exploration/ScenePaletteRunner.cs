using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime.Exploration;

public sealed record ScenePaletteState(ScenePaletteDefinition Resource, IReadOnlyList<ushort> Terrain, int ActorOffset)
{
    public static ushort Offset(ushort word, int offset) => (ushort)(
        Math.Clamp((word & 14) + offset * 2, 0, 14) |
        Math.Clamp(((word >> 4) & 14) + offset * 2, 0, 14) << 4 |
        Math.Clamp(((word >> 8) & 14) + offset * 2, 0, 14) << 8);
    internal ScenePaletteState Fade(int banks, int offset) => this with
    {
        Terrain = Array.AsReadOnly(Resource.Base.Select(color => Offset(color, offset)).ToArray()),
        ActorOffset = banks == 15 ? offset : ActorOffset,
    };
}

public sealed record ScenePaletteWait(WaitToken Token, ScenePaletteOperation Operation,
    ScenePaletteState Previous, int Elapsed = 0, bool ActualDone = false) : ProgramWait(Token)
{
    public int Duration => Operation switch
    {
        FadeScenePalette fade => (fade.Offsets.Count + 1) * fade.Period + 1,
        PulseScenePalette pulse => pulse.Ticks * 2,
        _ => throw new InvalidOperationException("scene-palette-operation"),
    };
    public bool LogicalDone => Elapsed >= Duration;
}
public sealed record SceneEndWait(WaitToken Token) : ProgramWait(Token);

internal static class ScenePaletteRunner
{
    internal static StoryState Begin(ScenarioDefinition definition, SessionSnapshot current,
        ScenePaletteOperation operation, WaitToken token)
    {
        ExplorationTextRunner.ValidateContext(current);
        var display = current.Story.Display ?? throw new BattleRuleException("scene-palette-display", "story.display", true);
        if (definition.Exploration!.Visuals?.Maps.GetValueOrDefault(current.Exploration!.Map)?.ScenePalette is not { } resource)
            throw new BattleRuleException("scene-palette-resource", "map.scenePalette", true);
        var previous = display.Scene ?? new ScenePaletteState(resource,
            Array.AsReadOnly(resource.Base.Select(color => display.Visibility == FullFadeVisibility.Black ? (ushort)0 : color).ToArray()),
            display.Visibility == FullFadeVisibility.Black ? -7 : 0);
        if (!ReferenceEquals(previous.Resource, resource))
            throw new BattleRuleException("scene-palette-map", "map.scenePalette", true);
        Validate(operation);
        var scene = operation is PulseScenePalette pulse ? previous with { Terrain = pulse.Colors } : previous;
        return current.Story.Copy(current.Story.Cursor, new ScenePaletteWait(token, operation, previous),
            display: display with { Scene = scene });
    }

    private static void Validate(ScenePaletteOperation operation)
    {
        bool valid = operation switch
        {
            FadeScenePalette fade => fade.Banks is 1 or 15 && fade.Period is > 0 and <= 60 &&
                fade.Offsets.Count is > 0 and <= 8 && fade.Offsets.All(offset => offset is >= -7 and <= 7),
            PulseScenePalette pulse => pulse.Bank == 0 && pulse.Ticks is > 0 and <= 60 &&
                pulse.Colors.Count == 16 && pulse.Colors.All(PaletteColorTransition.Valid),
            _ => false,
        };
        if (!valid) throw new BattleRuleException("scene-palette-operands", "program.palette", true);
    }

    internal static SessionSnapshot Tick(ScenarioDefinition definition, SessionSnapshot current, List<SessionObservation> observations)
    {
        var wait = (ScenePaletteWait)current.Story.Wait!;
        int elapsed = wait.Elapsed + 1;
        var display = current.Story.Display!;
        var scene = display.Scene!;
        if (wait.Operation is FadeScenePalette fade && elapsed % fade.Period == 0 && elapsed / fade.Period <= fade.Offsets.Count)
            scene = scene.Fade(fade.Banks, fade.Offsets[elapsed / fade.Period - 1]);
        if (wait.Operation is PulseScenePalette pulse && elapsed == pulse.Ticks) scene = wait.Previous;
        current = current.WithStory(current.Story.Copy(current.Story.Cursor, wait with { Elapsed = elapsed },
            display: display with { Scene = scene }));
        current = ExplorationDispatcher.Service(definition, current, observations, "scene-palette-service");
        return Finish(current, observations);
    }

    internal static SessionSnapshot Finish(SessionSnapshot current, List<SessionObservation> observations)
    {
        if (current.Story.Wait is not ScenePaletteWait { LogicalDone: true, ActualDone: true }) return current;
        return ProgramRunner.Commit(current, current.Active,
            current.Story.Copy(ProgramRunner.Next(current.Story.Cursor!.Value)), observations, "scene-palette-completed");
    }

    internal static StoryState End(SessionSnapshot current, WaitToken token)
    {
        if (current.Story.TextWindow is not ClosedTextWindow || current.Story.PortraitWindow is not ClosedPortraitWindow)
            throw new BattleRuleException("scene-end-windows", "story.windows", true);
        var display = current.Story.Display ?? throw new BattleRuleException("scene-end-display", "story.display", true);
        return current.Story.Copy(current.Story.Cursor, new SceneEndWait(token), display: display with { Scene = null });
    }
}
