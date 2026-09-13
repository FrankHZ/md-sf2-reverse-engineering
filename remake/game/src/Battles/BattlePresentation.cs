using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed record BattleActorMarker(ActorRef Actor, MapPosition Position, bool Ally, bool Selected, string Text);
internal sealed record BattlePresentation(string Title, string Status, string Roster,
    IReadOnlyList<BattleActorMarker> Markers, IReadOnlyList<MapPosition> Preview);

internal static class BattleSnapshotProjection
{
    internal static BattlePresentation Project(SessionResult result)
    {
        var snapshot = result.Snapshot;
        var battle = snapshot.Battle;
        var selection = snapshot.Selection;
        string status = result.Failure is { } failure
            ? $"{failure.Kind}: {failure.Code} ({failure.Field})"
            : selection is null ? result.StopReason.ToString()
            : $"{selection.Actor.Value} · {selection.Stage}" +
                (selection.Target is { } target ? $" · target {target.Value}" : "");
        var markers = battle.Actors.Where(a => a.Hp > 0).Select(a => new BattleActorMarker(
            a.Actor, a.Position, a.Definition.IsAlly, a.Actor == selection?.Actor,
            $"{a.Actor.Value}\n{a.Hp}/{a.Definition.MaxHp}")).ToArray();
        string roster = string.Join("\n", battle.Actors.Select(a =>
            $"{a.Actor.Value}: HP {a.Hp}/{a.Definition.MaxHp}  MP {a.Mp}/{a.Definition.MaxMp}  EXP {a.Exp}"));
        return new($"AUTHORED BATTLE · {battle.Definition.Map.Value} · round {battle.Round}", status,
            roster, Array.AsReadOnly(markers), selection?.Preview.Path ?? []);
    }
}
