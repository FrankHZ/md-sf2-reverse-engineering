using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01ExplorationEntryTests
{
    [Theory]
    [InlineData(32, 13, ExplorationDirection.North, 32, 13, "wall")]
    [InlineData(32, 13, ExplorationDirection.East, 33, 13, "move")]
    [InlineData(32, 13, ExplorationDirection.West, 31, 13, "move")]
    [InlineData(32, 13, ExplorationDirection.South, 32, 14, "move")]
    [InlineData(31, 12, ExplorationDirection.East, 31, 12, "wall")]
    [InlineData(33, 12, ExplorationDirection.West, 33, 12, "wall")]
    [InlineData(31, 13, ExplorationDirection.East, 31, 13, "return.followers")]
    [InlineData(32, 14, ExplorationDirection.North, 32, 14, "return.followers")]
    [InlineData(32, 14, ExplorationDirection.South, 32, 15, "move")]
    [InlineData(32, 15, ExplorationDirection.South, 32, 16, "move")]
    [InlineData(31, 14, ExplorationDirection.South, 31, 14, "return.region")]
    [InlineData(32, 15, ExplorationDirection.West, 32, 15, "return.region")]
    [InlineData(32, 16, ExplorationDirection.East, 32, 16, "return.region")]
    [InlineData(32, 16, ExplorationDirection.South, 32, 16, "return.region")]
    [InlineData(31, 13, ExplorationDirection.West, 31, 13, "return.region")]
    [InlineData(33, 13, ExplorationDirection.East, 33, 13, "return.region")]
    [InlineData(31, 12, ExplorationDirection.North, 31, 12, "return.region")]
    public void ReturnPocketDistinguishesRealTerrainFromUnsupportedEventsAndFollowerCollision(
        int x, int y, ExplorationDirection direction, int targetX, int targetY, string outcome)
    {
        var words = new ushort[64 * 64];
        words[12 * 64 + 32] = 0xE8DC;
        words[15 * 64 + 32] = 0xC48F;
        words[62] = 0x080E;
        words[16 * 64 + 32] = 0x0C57;
        var layout = new WorkingMapLayout(words);
        var traversal = new OriginalMapTraversal([new(0, 0, 50, 31)]);
        var (result, unsupported, after) = GameSession.EvaluateReturnMovement(traversal, layout, new(x, y), direction,
            ChurchDoor(), 255);
        if (outcome.StartsWith("return.", StringComparison.Ordinal))
        {
            Assert.Null(result); Assert.Equal(outcome, unsupported!.Field);
            if (outcome == "return.followers") Assert.Contains("Unknown", unsupported.Message);
        }
        else
        {
            Assert.Null(unsupported); Assert.NotNull(result);
            Assert.Equal(new MapPosition(targetX, targetY), result.Position);
            Assert.Equal(new MapPosition(x, y), result.Source); Assert.Equal(direction, result.Direction);
            Assert.Equal(outcome == "move" ? OriginalMapTraversalOutcome.Moved : OriginalMapTraversalOutcome.BlockedByCollision,
                result.Outcome);
            if (outcome == "wall") Assert.Equal((ushort)0xE8DC, result.DestinationWord);
        }
        Assert.Equal(words, layout.Words);
        if (x == 32 && y == 14 && direction == ExplorationDirection.South)
        {
            Assert.Equal((ushort)0x080E, result!.DestinationWord);
            Assert.Equal(1, layout.Words.Zip(after.Words).Count(pair => pair.First != pair.Second));
        }
        else Assert.Same(layout, after);
    }

    private static OriginalMapStepCopyDefinition ChurchDoor() => new(
        new(ContentProfile.PrivateLocal, new("map3"), "Map03s4_StepEvents", 4),
        new(32, 15), new(62, 0, 32, 15, 1, 1));

    [Theory]
    [InlineData("battle")] [InlineData("binding")] [InlineData("ordinal")]
    [InlineData("warp")] [InlineData("zone")] [InlineData("blocked")] [InlineData("wrong-show")]
    public void DoorCopyRejectsForeignModeBindingAndPostCopyWordsWithoutChangingInput(string mutation)
    {
        var words = new ushort[4096]; words[32 + 15 * 64] = 0xC48F;
        words[62] = mutation switch { "warp" => 0x100E, "zone" => 0x140E,
            "blocked" => 0xC80E, "wrong-show" => 0x080F, _ => 0x080E };
        var layout = new WorkingMapLayout(words); var door = ChurchDoor();
        if (mutation == "ordinal") door = new(new(ContentProfile.PrivateLocal, new("map3"),
            "Map03s4_StepEvents", 1), door.Trigger, door.Copy);
        Assert.Equal("return.door", Assert.Throws<ArgumentException>(() => GameSession.EvaluateReturnMovement(
            new([new(0, 0, 50, 31)]), layout, new(32, 14), ExplorationDirection.South,
            mutation == "binding" ? null : door, mutation == "battle" ? (byte)0 : (byte)255)).ParamName);
        Assert.Equal(words, layout.Words);
    }

    [Fact]
    public void OpenDoorAndOutsideMarkersTraverseWithoutAnotherCopy()
    {
        var words = new ushort[4096]; words[32 + 15 * 64] = 0x080E; words[32 + 16 * 64] = 0x0C57;
        var layout = new WorkingMapLayout(words); var traversal = new OriginalMapTraversal([new(0, 0, 50, 31)]);
        var (result, unsupported, after) = GameSession.EvaluateReturnMovement(traversal, layout,
            new(32, 16), ExplorationDirection.North, ChurchDoor(), 255);
        Assert.Null(unsupported); Assert.Equal(new MapPosition(32, 15), result!.Position); Assert.Same(layout, after);
        var (refused, boundary, unchanged) = GameSession.EvaluateReturnMovement(traversal, layout,
            new(32, 16), ExplorationDirection.South, null, 0);
        Assert.Null(refused); Assert.Equal("return.region", boundary!.Field); Assert.Same(layout, unchanged);
    }

    [Fact]
    public void ReturnInputsCannotBypassPendingBattleAdmission()
    {
        var session = PrivateOriginalBattle01StartupTests.PendingSession();
        var pending = session.PrivateOriginalBattle01Admission;
        Assert.IsType<PrivateOriginalMapReturnMovementRejected>(session.BeginPrivateOriginalMapReturnMovement(null, new(ExplorationDirection.North)));
        Assert.IsType<PrivateOriginalMapReturnMovementRejected>(session.AdvancePrivateOriginalMapReturnMovement(null));
        Assert.IsType<PrivateOriginalMapReturnMovementRejected>(session.RefusePrivateOriginalMapReturnInteraction(null));
        Assert.Same(pending, session.PrivateOriginalBattle01Admission); Assert.Null(session.PrivateOriginalBattle01);
    }

    [Theory]
    [InlineData("accepted")] [InlineData("duplicate")] [InlineData("foreign-source")]
    [InlineData("moved")] [InlineData("blocking-npc")] [InlineData("extra-follower")]
    [InlineData("missing-blue-flame")] [InlineData("wrong-target")]
    public void DeclarationFreezeAdmitsOnlyTheSpecificPlayerAndTwoFollowerOverlap(string mutation)
    {
        // Authored population isolates structural validation; real pinned admission is tested in Content.
        var population=new OriginalMapEntityPopulation(new("map3"),new("ms_map3"),
            Enumerable.Range(1,19).Select(i=>new OriginalMapEntityDefinition(new("authored-entry",i),
                (byte)(i+1),5,1,(byte)(i<3?i:100),new byte[]{0,4,96,206})));
        var destination=new MapPosition(32,13);
        var rows=population.Records;
        var entities=new List<PrivateOriginalMapReturnEntity> {
            new(0,0,false,null,"eas_Idle",destination,destination,destination,1,0,false),
            new(1,1,true,rows[0],"eas_Follower1",destination,destination,destination,1,1,false),
            new(2,2,true,rows[1],"eas_Follower2",destination,destination,destination,1,190,true) };
        foreach(var row in rows.Skip(2)) {
            int ordinal=row.Identity.OneBasedRecordOrdinal; int id=128+ordinal-3; bool hidden=id==142;
            entities.Add(new(id,ordinal,false,row,"authored-entry["+ordinal+"].tail",row.Position,
                hidden?null:row.Position,hidden?null:row.Position,row.OpaqueFacing,row.MapSprite,false,hidden,hidden));
        }
        if(mutation=="duplicate") entities[1]=entities[2];
        if(mutation=="foreign-source") entities[1]=entities[1] with {SourceRecord=rows[2]};
        if(mutation=="moved") entities[1]=entities[1] with {Position=new(31,13),DeclarationPosition=new(31,13)};
        if(mutation=="blocking-npc") entities[3]=entities[3] with {Position=destination};
        if(mutation=="extra-follower") entities[3]=entities[3] with {IsFollower=true};
        if(mutation=="missing-blue-flame") entities[2]=entities[2] with {MapSprite=2};
        if(mutation=="wrong-target") entities[2]=entities[2] with {TargetPosition=new(32,14)};
        var before=entities.ToArray();
        if(mutation=="accepted") {
            PrivateOriginalMapReturnArrivalSnapshot.ValidateEntities(entities,population,destination);
            Assert.All(entities.Take(3),e=>{Assert.Equal(destination,e.Position);Assert.Equal(destination,e.TargetPosition);
                Assert.Equal(0,e.VelocityXUnits);Assert.Equal(0,e.TravelYUnits);});
        }
        else Assert.Throws<ArgumentException>(()=>PrivateOriginalMapReturnArrivalSnapshot.ValidateEntities(entities,population,destination));
        Assert.Equal(before,entities); Assert.Equal(19,population.Records.Count);
    }

    private sealed class Source : IOriginalBattle01StartupSource
    {
        public int Calls { get; private set; }
        public OriginalBattle01StartupImportResult Admit()
        { Calls++; return new OriginalBattle01StartupImported(PrivateOriginalBattle01StartupTests.Definition()); }
    }

    [Theory]
    [InlineData("id")] [InlineData("step")] [InlineData("missing-flag")] [InlineData("flag220")]
    [InlineData("follower67")] [InlineData("missing-peter")] [InlineData("duplicate-lemon")]
    public void InvalidArrivalInputsRejectBeforeAnySourceCall(string mutation)
    {
        var selected = OriginalBattle01ControlledArrivalInputs.GransealFirstAttemptComparison;
        var flags = selected.Flags.ToDictionary(p => p.Key, p => p.Value);
        var slots = selected.DormantSlots.ToList();
        if (mutation == "missing-flag") flags.Remove(543);
        if (mutation == "flag220") flags[220] = true;
        if (mutation == "follower67") flags[67] = true;
        if (mutation == "missing-peter") slots.RemoveAll(slot => slot.Id == 7);
        if (mutation == "duplicate-lemon") slots[^1] = slots[^2];
        var invalid = new OriginalBattle01ControlledArrivalInputs(mutation == "id" ? "foreign" : selected.Id,
            mutation == "step" ? (ushort)1 : (ushort)0, flags, slots);
        var session = PrivateOriginalBattle01StartupTests.PendingSession(); var source = new Source();
        var pending = session.PrivateOriginalBattle01Admission;
        Assert.IsType<PrivateOriginalBattle01StartupRejected>(session.PreparePrivateOriginalBattle01Startup(pending,
            source, OriginalBattle01ControlledPartyPreset.LeaderDefeatComparison,
            OriginalBattle01ControlledReturnInputs.GransealFirstAttemptComparison, invalid));
        Assert.Equal(0, source.Calls); Assert.Same(pending, session.PrivateOriginalBattle01Admission);
        Assert.Null(session.PrivateOriginalBattle01);
    }

    [Fact]
    public void ArrivalCannotBeSelectedWithoutTheEarlyReturnComparisonOrOnAnUnadmittedImport()
    {
        var session = PrivateOriginalBattle01StartupTests.PendingSession(); var source = new Source();
        var arrival = OriginalBattle01ControlledArrivalInputs.GransealFirstAttemptComparison;
        Assert.Equal("arrival.binding", Assert.IsType<PrivateOriginalBattle01StartupRejected>(
            session.PreparePrivateOriginalBattle01Startup(session.PrivateOriginalBattle01Admission, source,
                OriginalBattle01ControlledPartyPreset.LeaderDefeatComparison, arrivalInputs: arrival)).Diagnostic.Field);
        Assert.Equal("arrival.load", Assert.IsType<PrivateOriginalBattle01StartupRejected>(
            session.PreparePrivateOriginalBattle01Startup(session.PrivateOriginalBattle01Admission, source,
                OriginalBattle01ControlledPartyPreset.LeaderDefeatComparison,
                OriginalBattle01ControlledReturnInputs.GransealFirstAttemptComparison, arrival)).Diagnostic.Field);
        Assert.Equal(0, source.Calls);
    }
}
