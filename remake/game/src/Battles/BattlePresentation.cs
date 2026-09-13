using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.GodotAdapter.Battles;

internal sealed record BattleActorMarker(ActorRef Actor, MapPosition Position, bool Ally, bool Selected, string Text);
internal sealed record BattlePresentation(string Title, string Status, string Roster,
    IReadOnlyList<BattleActorMarker> Markers, IReadOnlyList<MapPosition> Preview, IReadOnlyList<MapPosition> Focus);

internal static class BattleSnapshotProjection
{
    internal static BattlePresentation Project(SessionResult result, ActorRef? candidate)
    {
        var snapshot = result.Snapshot;
        var battle = snapshot.Battle;
        var selection = snapshot.Selection;
        string status = result.Failure is { } failure
            ? $"{failure.Kind}: {failure.Code} ({failure.Field})"
            : selection is null ? result.StopReason.ToString()
            : $"{selection.Actor.Value} · {selection.Stage}" +
                (selection.Target is { } target ? $" · target {target.Value}" : "");
        if (result.Failure is not null && candidate is { } attempted)
            status += $" · tried {attempted.Value} · selected target {selection?.Target?.Value ?? "none"}";
        var markers = battle.Actors.Where(a => a.Hp > 0).Select(a => new BattleActorMarker(
            a.Actor, a.Position!, a.Definition.IsAlly, a.Actor == selection?.Actor,
            $"{a.Actor.Value}\n{a.Hp}/{a.Definition.MaxHp}")).ToArray();
        string roster = string.Join("\n", battle.Actors.Select(a =>
            $"{a.Actor.Value}: HP {a.Hp}/{a.Definition.MaxHp}  MP {a.Mp}/{a.Definition.MaxMp}  EXP {a.Exp}  KILLS {a.Kills}  DEFEATS {a.Defeats}"));
        var focus = selection?.Preview.Path.ToList() ?? [];
        foreach (var actor in battle.Actors.Where(a => a.Hp > 0 && (a.Actor == selection?.Actor ||
            a.Actor == selection?.Target || a.Actor == candidate))) focus.Add(actor.Position!);
        return new($"AUTHORED BATTLE · {battle.Definition.Map.Value} · round {battle.Round} · gold {battle.Gold}", status,
            roster, Array.AsReadOnly(markers), selection?.Preview.Path ?? [], focus.AsReadOnly());
    }
}
