using Sf2.Remake.Application.Content;
using Sf2.Remake.Application.Sessions;
using Sf2.Remake.Domain.Maps;
using Xunit;

namespace Sf2.Remake.Application.Tests;

public sealed class PrivateOriginalBattle01ExplorationEntryTests
{
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
