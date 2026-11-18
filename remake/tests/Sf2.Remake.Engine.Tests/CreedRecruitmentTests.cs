using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Gameplay;
using Sf2.Remake.Application.Gameplay.Sf2;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class CreedRecruitmentTests
{
    private static readonly int[] Members = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 13, 14, 15, 16, 17];

    [Theory]
    [InlineData(14, false, 12)] [InlineData(15, false, 12)]
    [InlineData(16, false, 12)] [InlineData(17, false, 12)]
    [InlineData(14, true, 12)] [InlineData(15, true, 12)]
    [InlineData(16, true, 12)] [InlineData(17, true, 12)]
    [InlineData(14, true, 10)]
    public void ChoiceSeparatesJoinedDeployedAndCurrentFollowing(int member, bool yes, int active)
    {
        var session = Start(active);
        var before = session.Current.Exploration!.Party;
        Talk(session, "entity-" + member);
        Assert.IsType<ChoiceWait>(session.Current.Story.Wait);
        Answer(session, yes);
        Assert.Equal(yes, session.Current.Story.Flags.Contains(member));
        Assert.Equal(yes, session.Current.Story.Flags.Contains(787));
        Assert.Equal(yes && active < 12, session.Current.Story.Flags.Contains(32 + member));
        var world = session.Current.Exploration!;
        Assert.Equal(yes, world.Entities[new("entity-" + member)].Follower is not null);
        Assert.Equal(before.Actors, world.Party.Actors);
        Assert.Same(before.ItemCustody, world.Party.ItemCustody);
        Assert.Equal(before.MainSeed, world.Party.MainSeed);
        Assert.Equal(before.ThinkingSeed, world.Party.ThinkingSeed);
        if (!yes) return;
        int other = member == 14 ? 15 : 14;
        Talk(session, "entity-" + other);
        Assert.Null(session.Current.Story.Wait);
        Assert.DoesNotContain(other, session.Current.Story.Flags);
        Assert.Equal(1, new[] { 14, 15, 16, 17 }.Count(session.Current.Story.Flags.Contains));
    }

    [Fact]
    public void LaterOfferBypassesTheInitialChoiceLockWithoutChangingEarlierMembership()
    {
        var session = Start();
        Talk(session, "entity-14"); Answer(session, true);
        // Separately authored later story condition; no natural F22 chronology claim.
        Talk(session, "entity-128");
        Talk(session, "entity-15");
        Assert.IsType<ChoiceWait>(session.Current.Story.Wait);
        Answer(session, true);
        Assert.Contains(14, session.Current.Story.Flags);
        Assert.Contains(15, session.Current.Story.Flags);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void ColdRestorePreservesCurrentFairyAndRecruitWhileFreshEntrySelectsTheNewFairy(bool recruited)
    {
        var session = Start();
        Talk(session, "entity-130");
        if (recruited) { Talk(session, "entity-14"); Answer(session, true); }
        var original = session.Current.Exploration!;
        var data = session.CaptureSave(session.Current.SessionId, session.Current.Revision);
        Assert.Null(data.Error);
        var restored = Assert.IsType<SessionStarted>(GameSession.Restore(session.Definition, data.Data!, session.Rules)).Session;
        var world = restored.Current.Exploration!;
        Assert.Equal(original.CurrentEntities, world.CurrentEntities);
        Assert.Equal(original.AllEntities.Select(actor => (actor.Entity, actor.Position, actor.Follower)),
            world.AllEntities.Select(actor => (actor.Entity, actor.Position, actor.Follower)));
        Assert.Equal(219, world.Entities[new("entity-130")].Sprite);
        Assert.DoesNotContain(world.AllEntities, actor => actor.Entity.Value == "entity-158");
        Assert.Equal(original.Layout.Words, world.Layout.Words);
        var recaptured = restored.CaptureSave(restored.Current.SessionId, restored.Current.Revision);
        Assert.Null(recaptured.Error);
        Assert.Equal(data.Data!.FieldScene!.Actors, recaptured.Data!.FieldScene!.Actors);
        // The zone has no recruit prerequisite. Its caller writes follow the scene.
        Talk(restored, "entity-129");
        Assert.Contains(786, restored.Current.Story.Flags);
        Assert.Contains(69, restored.Current.Story.Flags);
        Assert.DoesNotContain(68, restored.Current.Story.Flags);
        Assert.Equal(recruited, restored.Current.Story.Flags.Contains(787));
        var after = restored.CaptureSave(restored.Current.SessionId, restored.Current.Revision);
        Assert.Null(after.Error);
        var stillHere = Assert.IsType<SessionStarted>(GameSession.Restore(restored.Definition, after.Data!, restored.Rules)).Session;
        Assert.Equal(219, stillHere.Current.Exploration!.Entities[new("entity-130")].Sprite);
        var fresh = MapTransfer.Build(session.Definition.Exploration!.Maps[new("map-33")], new("entity-0"),
            new(15, 30), 1, 32, world.Party, restored.Current.Story.Flags);
        Assert.Equal(213, fresh.Entities[new("entity-130")].Sprite);
        Assert.Equal(219, fresh.Entities[new("entity-158")].Sprite);
        Assert.Null(fresh.Entities[new("entity-130")].Follower);
    }

    private static SessionResult Accept(GameSession s, SessionCommand command)
    {
        var result = s.Submit(new(s.Current.SessionId, s.Current.Revision, null, command));
        Assert.Null(result.Failure); return result;
    }
    private static void Talk(GameSession s, string id)
    {
        var world = s.Current.Exploration!;
        var target = world.Entities[new(id)].Position;
        var goal = new MapPosition(target.X, target.Y + 1);
        var start = world.PlayerEntity.Position;
        var queue = new Queue<(MapPosition At, List<ExplorationDirection> Path)>();
        var visited = new HashSet<MapPosition> { start };
        queue.Enqueue((start, []));
        List<ExplorationDirection>? route = null;
        while (queue.TryDequeue(out var next))
        {
            if (next.At == goal) { route = next.Path; break; }
            foreach (var (dx, dy, direction) in new[] { (0, -1, ExplorationDirection.North), (0, 1, ExplorationDirection.South),
                (-1, 0, ExplorationDirection.West), (1, 0, ExplorationDirection.East) })
            {
                int x = next.At.X + dx, y = next.At.Y + dy;
                if (x is < 0 or > 40 || y is < 0 or > 40) continue;
                var at = new MapPosition(x, y);
                if (visited.Contains(at) ||
                    world.AllEntities.Any(e => e.Visible && e.Entity != world.Player && e.Follower is null && e.Position == at)) continue;
                visited.Add(at); queue.Enqueue((at, [.. next.Path, direction]));
            }
        }
        Assert.NotNull(route);
        foreach (var direction in route)
        { Accept(s, new Move(direction)); Accept(s, new AdvanceSimulation(s.Current.Story.Wait?.Token, 32)); }
        Accept(s, new Move(ExplorationDirection.North));
        Accept(s, new AdvanceSimulation(s.Current.Story.Wait?.Token, 32));
        Accept(s, new Interact(new(id)));
    }

    private static void Answer(GameSession s, bool yes)
    {
        var wait = Assert.IsType<ChoiceWait>(s.Current.Story.Wait);
        Accept(s, new ChoiceHeldInput(wait.Token, false));
        Accept(s, new AdvanceSimulation(wait.Token, 5));
        Accept(s, new ChooseDialogue(wait.Token, yes));
        Accept(s, new AdvanceSimulation(wait.Token, 5));
        Accept(s, new AdvanceSimulation(wait.Token, 10));
    }
    private static GameSession Start(int active = 12)
    {
        var (basis, saved) = OpeningRestoreTests.Fixture();
        var source = basis.Exploration!; var oldMap = source.Maps[new("map-33")];
        var prototype = basis.Encounters["opening"].Deployments[0];
        var rows = Members.Select(id => prototype with
        {
            Definition = new(new("ally-" + id), prototype.Definition.ClassRule, 1, 20, 5, 8, 4, 6, false, 6, [],
                growth: prototype.Definition.Growth, sourceLoadout: prototype.Definition.SourceLoadout),
            ProcessingOrder = id, Initialization = prototype.Initialization! with { AllyPartyMember = id },
        }).ToArray();
        var battle = basis.Encounters["opening"].WithDeployments(rows);
        var population = new ExplorationPopulation(30, 128, 0,
            [new(67, 7, 7), new(68, 31, 213), new(69, 158, 219)]);
        ExplorationEntityDefinition Npc(int id, int sprite) => new(new("entity-" + id), new(id == 128 ? 10 : id == 129 ? 20 : 17, id == 130 ? 10 : 5), 3, 32, Sprite: sprite);
        var residents = new[] { Npc(128, 202), Npc(129, 212), Npc(130, 219),
            Npc(14, 14), Npc(16, 16), Npc(17, 17), Npc(15, 15) };
        var events = new[] { 14, 15, 16, 17, 128, 129, 130 }.Select(id => new ExplorationEvent(
            ExplorationEventKind.Interact, null, null, new("entity-" + id),
            new(id == 128 ? "later" : id == 129 ? "oddler" : id == 130 ? "fairy" : "offer-" + id, 0))).ToArray();
        var map = new ExplorationMapDefinition(oldMap.Map, new(new ushort[4096]), oldMap.Traversal, residents, events,
            setup: new(new("ms-map33"), [new(new("784"), new("ms-map33-flag784")), new(new("786"), new("ms-map33-flag786"))]),
            population: population, basePalette: oldMap.BasePalette, viewAreas: oldMap.ViewAreas,
            setups: [new(new("ms-map33-flag784"), residents, events, null, null, population),
                new(new("ms-map33-flag786"), [Npc(128, 202), Npc(129, 212), Npc(130, 213)], events, null, null, population)]);
        var programs = new List<StoryProgram>();
        foreach (int id in new[] { 14, 15, 16, 17 })
            programs.Add(new("offer-" + id, [new BranchFlag(22, true, new("offer-" + id, 2)),
                new BranchFlag(787, true, new("refuse", 0)), new ChooseYesNo(89, new("refuse", 0)),
                new JoinPartyMember(id), new WriteFlag(787, true), new AddExplorationFollower(new("entity-" + id)), new EndProgram()]));
        programs.Add(new("refuse", [new EndProgram()]));
        programs.Add(new("later", [new WriteFlag(22, true), new EndProgram()]));
        programs.Add(new("fairy", [new CopyMapLayout(new(17, 13, 9, 5, 1, 1)), new FollowEntity(new("entity-130"), new("entity-31"), -24, 0),
            new WriteFlag(762, true), new WriteFlag(785, true), new EndProgram()]));
        programs.Add(new("oddler", [new FollowEntity(new("entity-7"), new("entity-0"), -24, 0),
            new FollowEntity(new("entity-130"), new("entity-7"), -24, 0), new WriteFlag(786, true),
            new WriteFlag(69, true), new WriteFlag(68, false), new EndProgram()]));
        var visuals = source.Visuals! with { Sprites = Enumerable.Range(0, 220).ToDictionary(i => i,
            i => new ExplorationSpriteVisual(i, [], null, 0)) };
        var world = new ExplorationDefinition([map], programs, provenance: source.Provenance,
            partyFlags: source.PartyFlags, visuals: visuals);
        var definition = new ScenarioDefinition(basis.Package, [battle], exploration: world) { Persistence = new Sf2OpeningRestore() };
        var data = saved with { Recipe = "creed-mansion", Map = "map-33", X = 9, Y = 5, EgressMap = "map-9",
            Allies = Members.Select(id => saved.Allies[0] with { Actor = "ally-" + id }).ToArray(),
            ActiveAllies = Members.Take(active).Select(id => "ally-" + id).ToArray(),
            Flags = Members.Take(12).Concat(Members.Take(active).Select(id => id + 32)).Concat([67, 68, 774, 783, 784]).ToArray() };
        return Assert.IsType<SessionStarted>(GameSession.Restore(definition, data, RuleCompositions.Sf2())).Session;
    }
}
