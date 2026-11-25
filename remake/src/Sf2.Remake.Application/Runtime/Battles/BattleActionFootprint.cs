using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Gameplay.Sf2;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Runtime.Battles;

public sealed record BattleFootprintUnit(ActorRef Actor, MapPosition Position);

// Reach is the display band. Centers come from command admission, independently of the display
// mask. Area is nominal geometry. Recipients are eligible before numerical effects and RNG.
public sealed record BattleActionFootprint(IReadOnlyList<MapPosition> Reach, IReadOnlyList<BattleFootprintUnit> Centers,
    IReadOnlyList<MapPosition> Area, IReadOnlyList<BattleFootprintUnit> Recipients)
{
    internal static BattleActionFootprint? Query(SessionSnapshot current, SessionRules rules,
        BattleSceneDefinition? content, bool requireContent, ActorRef? candidate)
    {
        if (!current.HasBattleControl || current.Selection is not
            { Stage: BattleSelectionStage.TargetChoice or BattleSelectionStage.CommitReady, ActionReference: { } action } selection)
            return null;

        var battle = selection.AtConfirmedStop(current.Battle);
        var origin = selection.Preview.Destination;
        var actor = selection.Actor;
        BattleFootprintUnit Unit(BattleActorState row) => new(row.Actor, row.Actor == actor ? origin : row.Position!);

        try
        {
            BattleMovement.RequireStop(battle, actor, origin);
            var admission = BattleChoices.RequireAction(rules, battle, actor, action);
            if (admission is not { MinimumRange: { } min, MaximumRange: { } max }) return null;
            var targets = BattleChoices.QueryTargets(rules, battle, actor, origin, action, content, requireContent);
            var centers = targets.Where(target => target.Enabled).Select(target => Unit(battle.GetActor(target.Actor))).ToArray();
            if (centers.Length == 0) return null;

            var map = battle.Definition;
            var reach = Cells(map, origin, min, max, hideBarriers: true);
            if (candidate is not { } id || !centers.Any(target => target.Actor == id))
                return new(reach, Array.AsReadOnly(centers), [], []);

            var center = battle.GetActor(id);
            var selectedRule = rules.Action(action);
            var footprint = BattleActionRules.Invoke(selectedRule, "action-area-query",
                () => BattleActionArea.Read(battle, actor, origin, action, admission, center));
            var area = footprint.Radius is { } radius ? Cells(map, Unit(center).Position, 0, radius)
                : Array.AsReadOnly(footprint.NominalRoster.Select(row => Unit(row).Position).ToArray());
            return new(reach, Array.AsReadOnly(centers), area,
                Array.AsReadOnly(footprint.Recipients.Select(Unit).ToArray()));
        }
        catch (BattleRuleException) { return null; }
        catch (BattleActionRuleFault) { return null; }
    }

    private static IReadOnlyList<MapPosition> Cells(BattleDefinition map, MapPosition origin,
        int min, int max, bool hideBarriers = false)
    {
        List<MapPosition> result = [];
        for (int y = Math.Max(0, origin.Y - max); y <= Math.Min(map.Height - 1, origin.Y + max); y++)
        for (int x = Math.Max(0, origin.X - max); x <= Math.Min(map.Width - 1, origin.X + max); x++)
        {
            var position = new MapPosition(x, y);
            if (BattleRange.Contains(origin, position, min, max) &&
                (!hideBarriers || map.Terrain[y * 48 + x].Surface != TerrainSurface.Barrier))
                result.Add(position);
        }
        return result.AsReadOnly();
    }
}
