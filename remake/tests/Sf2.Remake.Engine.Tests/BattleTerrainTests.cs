using System.Text.Json.Nodes;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class BattleTerrainTests
{
    [Theory]
    [InlineData("open", 2)]
    [InlineData("brush", 3)]
    [InlineData("deep", 4)]
    [InlineData("impassable", -1)]
    public void ReferencedSurfaceChangesActualPreviewAndCommitWithoutChangingProtection(string surface, int cost)
    {
        var session = Start(change: d => d["terrains"]![0]!["legend"]!["b"]!["surface"] = surface);
        var before = session.Current;
        var move = Send(session, new Move(ExplorationDirection.East));
        if (cost < 0)
        {
            Assert.Equal("movement-range", move.Failure!.Code);
            Assert.Same(before, session.Current);
            return;
        }
        Assert.Null(move.Failure);
        Assert.Equal(cost, session.Current.Selection!.Preview.Cost);
        Assert.Equal(new MapPosition(4, 3), session.Current.Selection.Preview.Destination);
        Assert.Same(before.Battle, session.Current.Battle);
        Assert.Equal(TerrainProtection.Heavy, before.Battle.Definition.Terrain[3 * 48 + 4].Protection);
        Accept(session, new Cancel());
        Assert.Equal(new MapPosition(3, 3), session.Current.Selection!.Preview.Destination);
        Accept(session, new Move(ExplorationDirection.East));
        Accept(session, new Confirm()); Accept(session, new ChooseAction(SessionAction.Stay));
        var committed = Accept(session, new Confirm());
        Assert.Equal(new MapPosition(4, 3), committed.Snapshot.Battle.GetActor(new("medic-a")).Position);
        Assert.Single(committed.Observations, o => o.Kind == "movement");
        Assert.Same(before.Battle.Definition.Terrain, committed.Snapshot.Battle.Definition.Terrain);
    }

    [Theory]
    [InlineData("none", "swordsman", 16, 2, 500)]
    [InlineData("heavy", "lookout", 1, 24, 478)]
    public void TargetProtectionChangesActualAiLethalityChoiceAndPhysicalSettlement(
        string protection, string target, int primaryPriority, int primaryHp, int secondaryHp)
    {
        var session = Start("stone-court", d =>
        {
            // Accepted seed55 counter values, with thinking draws2/0 from high byte1.
            d["start"]!["mainSeed"] = 0x00371234u;
            d["start"]!["thinkingSeed"] = 0x01EF0042u;
            foreach (int index in new[] { 0, 1, 2 })
            {
                d["start"]!["actors"]![index]!["hp"] = 500;
                d["actors"]![index]!["maxHp"] = 500; d["actors"]![index]!["defense"] = 4;
                d["actors"]![index]!["attack"] = index == 2 ? 30 : 18;
                d["actors"]![index]!["physical"]!["critical"] = index == 2
                    ? new JsonObject { ["chance"] = "one-in-16", ["damageBonus"] = "quarter" }
                    : new JsonObject { ["chance"] = "one-in-32", ["damageBonus"] = "half" };
            }
            d["start"]!["actors"]![0]!["hp"] = 24;
            d["actors"]![2]!["move"] = 3;
            var placements = d["encounters"]![0]!["placements"]!;
            placements[1]!["x"] = 3; placements[1]!["y"] = 1;
            placements[2]!["aiStrategy"] = "attack-then-approach";
            d["terrains"]![0]!["legend"]!["t"] = new JsonObject { ["surface"] = "open", ["protection"] = protection };
            d["terrains"]![0]!["rows"]![3] = "#pptpppp#";
        });
        var result = FinishBattleScenes(session, Stay(session));
        // ATT30-DEF4 gives26 without protection, floor(26*205/256)=20 with heavy.
        // Primary HP24 is therefore lethal only without protection; the other candidate costs4/priority11.
        var candidate = Assert.Single(result.Observations, o => o.Kind == "ai-candidate" && o.Target == new ActorRef("swordsman"));
        Assert.Equal(primaryPriority, candidate.After);
        Assert.Equal(11, Assert.Single(result.Observations, o => o.Kind == "ai-candidate" && o.Target == new ActorRef("lookout")).After);
        Assert.Equal(new ushort?[] { 2, 0 }, result.Observations.Where(o => o.Kind == "thinking-rng").Select(o => o.RandomValue));
        Assert.Equal(new ActorRef(target), Assert.Single(result.Observations, o => o.Kind == "ai-target").Target);
        Assert.Equal(primaryHp, result.Snapshot.Battle.GetActor(new("swordsman")).Hp);
        Assert.Equal(secondaryHp, result.Snapshot.Battle.GetActor(new("lookout")).Hp);
        Assert.Equal(493, result.Snapshot.Battle.GetActor(new("raider")).Hp);
        Assert.Equal(0x557E1234u, ConstructionSeed(result));
        // Range3 rejection retains accepted byte0, as in EnemyActionTests continued-history case.
        Assert.Equal(0x00EF0042u, result.Snapshot.Battle.ThinkingSeed);
        Assert.Equal(new ActorRef(target), result.Snapshot.Battle.GetActor(new("raider")).LastTarget);
        Assert.Equal(new ActorRef("lookout"), result.Snapshot.Selection!.Actor);
    }

    [Theory]
    [InlineData("missing-legend", "missing-field", SessionFailureKind.ContentError)]
    [InlineData("missing-definition", "missing-terrain-definition", SessionFailureKind.ContentError)]
    [InlineData("surface", "terrain-surface", SessionFailureKind.UnsupportedCapability)]
    [InlineData("protection", "terrain-protection", SessionFailureKind.UnsupportedCapability)]
    [InlineData("number", "string-required", SessionFailureKind.ContentError)]
    [InlineData("mover-cost", "unknown-or-duplicate-field", SessionFailureKind.ContentError)]
    [InlineData("symbol", "terrain-symbol", SessionFailureKind.ContentError)]
    [InlineData("blocked-start", "blocked-placement", SessionFailureKind.ContentError)]
    public void RealAdmissionRejectsUnresolvedOrUnsupportedTerrain(string shape, string code, SessionFailureKind kind)
    {
        var document = Document(); var terrain = document["terrains"]![0]!;
        switch (shape)
        {
            case "missing-legend": terrain.AsObject().Remove("legend"); break;
            case "missing-definition": terrain["legend"]!.AsObject().Remove("g"); break;
            case "surface": terrain["legend"]!["g"]!["surface"] = "flight-only"; break;
            case "protection": terrain["legend"]!["g"]!["protection"] = "invulnerable"; break;
            case "number": terrain["legend"]!["g"]!["protection"] = 1; break;
            case "mover-cost": terrain["legend"]!["g"]!["moveCost"] = 2; break;
            case "symbol": terrain["legend"]!["ground"] = terrain["legend"]!["g"]!.DeepClone(); break;
            case "blocked-start": terrain["legend"]!["g"]!["surface"] = "impassable"; break;
        }
        var failed = Assert.IsType<SessionStartFailed>(GameSession.Start(Reader(document)));
        Assert.Equal((kind, code), (failed.Failure.Kind, failed.Failure.Code));
    }
}
