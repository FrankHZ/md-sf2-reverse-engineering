using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Engine.Tests;

public sealed class RibbleItemEventTests
{
    private static readonly ActorRef Leader = FieldInventoryTests.Giver, Carrier = FieldInventoryTests.Recipient;
    private const byte Panel = 112, Sword = 61;

    private static GameSession Start(int x = 15, int y = 19, byte facing = 1, int[]? flags = null, bool hasSword = true, bool full = false)
    {
        var baseSession = FieldInventoryTests.Session(custody: new([], new Dictionary<byte, byte>()));
        var battle = baseSession.Definition.Encounters.Values.Single();
        var events = new[] { 15, 16 }.Select(tile => new ExplorationEvent(ExplorationEventKind.UseItem, tile, 19,
            null, new("open", 0), Item: Panel, ItemFacing: 1, RequiredFlag: 731, RequiredFlagValue: false));
        var rohde = new EntityRef("scholar");
        var proposal = new ProgramLocation("proposal", 0);
        var tree = new StoryProgram("open", [new WriteFlag(731, true), new RemovePartyItem(Panel), new EndProgram()]);
        var talk = new StoryProgram("talk", [
            new BranchFlag(727, true, new("repeat", 0)), new BranchHasItem(Sword, proposal),
            new WriteFlag(798, true), new EndProgram()]);
        var repeat = new StoryProgram("repeat", [
            new BranchFlag(725, true, new("repeat", 3)), new FaceInteraction(rohde),
            new JumpProgram(proposal), new EndProgram()]);
        var choice = new StoryProgram("proposal", [
            new ChooseYesNo(89), new BranchFlag(89, false, new("proposal", 6)),
            new WriteFlag(725, true), new WriteFlag(84, true), new WriteFlag(418, true),
            new FollowEntity(rohde, new("hero"), -24, 0), new WriteFlag(727, true), new EndProgram()]);
        var layout = new ushort[WorkingMapLayout.WordCount];
        layout[6 * 64 + 35] = 0x1800;
        var chest = new FieldSearchDefinition([new(new(35, 6), 147, Sword, null)], [], [], 0xD801, 0xD802);
        var map = new ExplorationMapDefinition(new("town"), new(layout),
            new([new(0, 0, 63, 63)]), [new(rohde, new(16, 59), 3, 32)],
            events.Append(new(ExplorationEventKind.Interact, null, null, rohde, new("talk", 0), EntityFlags: 0))
                .Append(new(ExplorationEventKind.UseItem, 16, 58, null, proposal, Item: Sword, RequiredFlag: 727, RequiredFlagValue: false)), search: chest);
        var world = new ExplorationDefinition([map], [tree, talk, repeat, choice]);
        var definition = new ScenarioDefinition("quest", [battle], exploration: world)
        {
            FieldPreparation = new([.. baseSession.Definition.FieldPreparation!.Items.Values, new(Panel, "Panel", false, true)],
                new Dictionary<byte, FieldRecoveryEffect> { [Panel] = FieldRecoveryEffect.MapEvent, [Sword] = FieldRecoveryEffect.MapEvent }),
        };
        var before = baseSession.Current.Exploration!.Party;
        var actors = before.Actors.Select(row => row with {
            SourceLoadout = new(full ? [0, 0, 0, 0] : row.Actor == Leader ? [Panel, 0, Panel, 127] : [Panel, hasSword ? Sword : (ushort)127, 127, 127], []),
        });
        return Assert.IsType<SessionStarted>(GameSession.Start(definition, new ExplorationStartInput(map.Map, new("hero"),
            new(x, y), facing, 32, flags ?? [], FieldInventory.Replace(before, actors)))).Session;
    }

    private static SessionResult Use(GameSession session, int slot, ushort word) =>
        session.Submit(new(session.Current.SessionId, session.Current.Revision, null, new FieldUseItem(Carrier, slot, word)));

    [Theory]
    [InlineData(15, 19, 1, true)]
    [InlineData(16, 19, 1, true)]
    [InlineData(15, 20, 1, false)]
    [InlineData(16, 19, 3, false)]
    public void PanelUsesCurrentPlayerTileAndFacing(int x, int y, byte facing, bool opens)
    {
        var session = Start(x, y, facing);
        var before = session.Current;
        Assert.Equal(FieldRecoveryEffect.MapEvent, session.QueryFieldActions(Carrier).Items[0].Use);
        Assert.Same(before, session.Current);
        Assert.Contains(Use(session, 1, Sword).Observations, row => row.Kind == "field-no-effect");
        Assert.Equal("field-stale-item", Use(session, 1, Panel).Failure!.Code);
        Assert.DoesNotContain(731, session.Current.Story.Flags);
        var result = Use(session, 0, Panel);
        Assert.Null(result.Failure);
        Assert.Equal(opens, session.Current.Story.Flags.Contains(731));
        Assert.Contains(result.Observations, row => row.Kind == (opens ? "field-item-event" : "field-no-effect"));
        if (opens)
        {
            var arrival = Assert.IsType<ZoneArrivalWait>(session.Current.Story.Wait);
            Assert.Null(session.Submit(new(session.Current.SessionId, session.Current.Revision, null, new AdvanceSimulation(arrival.Token))).Failure);
            Assert.Null(session.Current.Story.EventCaller);
        }
        int CountPanels() => session.QueryPartyDetails()!.Members.Sum(m => m.Inventory.Count(slot => slot.ItemId == Panel));
        var held = session.Current.Exploration!.Party;
        Assert.Equal(opens ? 2 : 3, CountPanels());
        Assert.Contains(Use(session, 0, Panel).Observations, row => row.Kind == "field-no-effect");
        Assert.Same(held, session.Current.Exploration.Party);
    }

    [Fact]
    public void RefusalAllowsLaterConsentWithoutTheSwordAndNeverEnlistsTheFollower()
    {
        var session = Start(16, 58, 3);
        var scholar = new EntityRef("scholar");
        SessionResult Submit(SessionCommand command) =>
            session.Submit(new(session.Current.SessionId, session.Current.Revision, null, command));
        Assert.Null(Submit(new Interact(scholar)).Failure);
        Assert.Null(Submit(new ChooseDialogue(session.Current.Story.Wait!.Token, false)).Failure);
        Assert.Contains(727, session.Current.Story.Flags);
        Assert.DoesNotContain(84, session.Current.Story.Flags);
        Assert.Null(session.Current.Exploration!.Entities[scholar].Follower);
        Assert.Contains(Use(session, 1, Sword).Observations, row => row.Kind == "field-no-effect");
        Assert.Null(Submit(new FieldDrop(Carrier, 1, Sword)).Failure);
        Assert.Null(Submit(new Interact(scholar)).Failure);
        Assert.Equal(1, session.Current.Exploration.Entities[scholar].Motion.Facing);
        Assert.Null(Submit(new ChooseDialogue(session.Current.Story.Wait!.Token, true)).Failure);
        Assert.All(new[] { 725, 84, 418 }, flag => Assert.Contains(flag, session.Current.Story.Flags));
        Assert.Equal(session.Current.Exploration.PlayerEntity.Slot, session.Current.Exploration.Entities[scholar].Follower!.LeaderSlot);
        Assert.Equal(new[] { Leader, Carrier }, session.QueryPartyDetails()!.Members.Select(m => m.Carried.Actor));
    }

    [Fact]
    public void FirstTalkWithoutSwordDismissesAndSwordUseCanStartTheChoiceWithoutFacingTheScholar()
    {
        var noSword = Start(16, 58, 3, hasSword: false);
        Assert.Null(noSword.Submit(new(noSword.Current.SessionId, noSword.Current.Revision, null, new Interact(new("scholar")))).Failure);
        Assert.Contains(798, noSword.Current.Story.Flags);
        Assert.DoesNotContain(727, noSword.Current.Story.Flags);
        var show = Start(16, 58, 0);
        Assert.Null(Use(show, 1, Sword).Failure);
        Assert.IsType<ChoiceWait>(show.Current.Story.Wait);
        Assert.Contains((ushort)Sword, show.Current.Exploration!.Party.Actors.Single(a => a.Actor == Carrier).SourceLoadout!.Items);
    }

    [Fact]
    public void SwordChestFullPartyCanDropAnItemAndRetryWithoutChangingThePanelChest()
    {
        var session = Start(35, 7, 1, flags: [152, 731], full: true);
        SessionResult Submit(SessionCommand command) =>
            session.Submit(new(session.Current.SessionId, session.Current.Revision, null, command));
        FieldSearchWait SearchChest()
        {
            Assert.Null(Submit(new Search()).Failure);
            return Assert.IsType<FieldSearchWait>(session.Current.Story.Wait);
        }
        var full = SearchChest();
        Assert.Equal(FieldSearchKind.Full, full.Result.Kind);
        Assert.DoesNotContain(147, session.Current.Story.Flags);
        Assert.Null(Submit(new Acknowledge(full.Token)).Failure);
        Assert.Null(Submit(new FieldDrop(Carrier, 0, 0)).Failure);
        var receipt = SearchChest();
        Assert.Equal(FieldSearchKind.Item, receipt.Result.Kind);
        Assert.Equal(Carrier, receipt.Result.Recipient);
        Assert.Equal((ushort)Sword, session.Current.Exploration!.Party.Actors.Single(r => r.Actor == Carrier).SourceLoadout!.Items[3]);
        Assert.All(new[] { 147, 152, 731 }, f => Assert.Contains(f, session.Current.Story.Flags));
        Assert.Null(Submit(new Acknowledge(receipt.Token)).Failure);
        Assert.Equal(FieldSearchKind.Empty, SearchChest().Result.Kind);
    }

    [Fact]
    public void RemovalUsesLastMatchingSlotOfFirstJoinedCarrierRegardlessOfSelection()
    {
        var session = Start();
        var after = Assert.IsType<ActiveExploration>(FieldItemEvents.Remove(session.Definition, session.Current, Panel)).World.Party;
        Assert.Equal(new ushort[] { Panel, 0, 127, 127 }, after.Actors.Single(row => row.Actor == Leader).SourceLoadout!.Items);
        Assert.Equal(new ushort[] { Panel, Sword, 127, 127 }, after.Actors.Single(row => row.Actor == Carrier).SourceLoadout!.Items);
        Assert.Equal(Carrier, FieldItemEvents.Find(session.Definition, session.Current, Sword)!.Value.Member.Carried.Actor);
        Assert.Same(session.Current.Active, FieldItemEvents.Remove(session.Definition, session.Current, 99));
    }
}
