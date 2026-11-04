using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Battles;
using Sf2.Remake.Content.Scenarios;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;
using Sf2.Remake.TestSupport;
using Xunit;
using System.Text.Json.Nodes;
using static Sf2.Remake.Engine.Tests.EngineTestContent;

namespace Sf2.Remake.Engine.Tests;

public sealed class GalamShopContentTests
{
    [PrivateInputFact("SF2_PRIVATE_BATTLE_SCENE_CONTENT")]
    public void RequiredPrivateDetoxUsesOrdinaryCastContentWithoutHealingFairyAndKeepsHealGuard()
    {
        string Input(string key)=>PrivateInputFactAttribute.RequireInput(key);
        var document=JsonNode.Parse(File.ReadAllText(Input("SF2_PRIVATE_CONTROLLED_START")))!;
        document["mainSeed"]=4660u;document["gold"]=0;
        foreach(var row in document["allies"]!.AsArray())
        {
            row!["exp"]=0;row["kills"]=0;row["defeats"]=0;
            if(row["id"]!.GetValue<int>()==1)
            { row["level"]=4;row["agility"]=100;row["maxMp"]=20;row["mp"]=20;row["spells"]=new JsonArray(0,2,63,63); }
            else if(row["id"]!.GetValue<int>()==0)row["status"]=2;
        }
        string file=Path.GetTempFileName();
        try
        {
            File.WriteAllText(file,document.ToJsonString(),new System.Text.UTF8Encoding(false));
            var read=PrivateBattleScenarioTests.Selected(file).Read();Assert.True(read is ScenarioReadAccepted,read.ToString());
            var admitted=(ScenarioReadAccepted)read;
            var scenes=BattleSceneContentReader.Read(Input("SF2_PRIVATE_BATTLE_SCENE_CONTENT"),admitted.Definition.PrivateDefinitions!) with {Healing=null};
            var definition=new ScenarioDefinition(admitted.Definition.Package,admitted.Definition.Encounters.Values,
                admitted.Definition.PrivateDefinitions,battleScenes:scenes);
            var session=Assert.IsType<SessionStarted>(GameSession.Start(definition,admitted.Start)).Session;
            var priest=new ActorRef("ally-1");var target=new ActorRef("ally-0");
            Assert.Equal(priest,session.Current.Selection!.Actor);
            var before=session.Current;
            var choices=session.QueryBattleChoices();
            Assert.True(choices.Spells.Single(row=>row.Spell==new SpellRef("detox",1)).Targets.Single(row=>row.Actor==target).Enabled);
            Assert.Equal("healing-scene-content",choices.Spells.Single(row=>row.Spell==new SpellRef("heal",1)).Reason!.Code);
            Assert.Same(before,session.Current);
            Accept(session,new Confirm());Accept(session,new SelectSpell(new("detox",1)));
            var selecting=session.Current;
            var missing=BattleCommandDispatcher.Submit(selecting,new SelectTarget(target),requireSceneContent:true,rules:session.Rules);
            Assert.Equal("cure-scene-content",missing.Failure!.Code);Assert.Same(selecting,missing.Snapshot);
            Accept(session,new SelectTarget(target));
            var result=Accept(session,new Confirm());Assert.Null(session.Current.BattleScene!.Healing);
            FinishBattleScenes(session,result);
            Assert.Equal((byte)17,session.Current.Battle.GetActor(priest).Mp);
            Assert.Equal((ushort)0,session.Current.Battle.GetActor(target).Status);
            Assert.True(session.Current.HasBattleControl);
        }
        finally {File.Delete(file);}
    }

    [PrivateInputFact("SF2_PRIVATE_EXPLORATION_CONTENT", "SF2_PRIVATE_STATIC_DATA")]
    public void ActualOpeningPrstGrowthRetainsSarahLevelFourDetoxOne()
    {
        string Input(string key)=>PrivateInputFactAttribute.RequireInput(key);
        var reader=new OpeningNewGameReader(Input("SF2_PRIVATE_EXPLORATION_CONTENT"),Input("SF2_PRIVATE_BATTLE01_DATA"),
            Input("SF2_PRIVATE_BATTLE01_SCENE"),Input("SF2_PRIVATE_BATTLE01_TERRAIN"),Input("SF2_PRIVATE_STATIC_DATA"),
            Input("SF2_PRIVATE_ENEMY_DATA"),Input("SF2_PRIVATE_ENEMY_GOLD"),0,uint.MaxValue);
        var read=reader.Read();Assert.True(read is ExplorationReadAccepted,read.ToString());
        var definition=((ExplorationReadAccepted)read).Definition;
        var priest=definition.Encounters.Values.SelectMany(b=>b.Deployments).First(d=>d.Initialization?.AllyPartyMember==1);
        Assert.Contains(priest.Definition.Growth!.Spells,row=>row.Level==4 && row.PackedSpell==2);
        Assert.Equal(new SpellRef("detox",1),priest.Definition.Growth.SpellDefinitions[2]);
    }

    [PrivateInputFact("SF2_GALAM_WORLD")]
    public void PinnedDefaultDoesNotInventShopsAndF663CallersUseRealStockPricesAndSupplies()
    {
        string root = Path.GetDirectoryName(PrivateInputFactAttribute.RequireInput("SF2_GALAM_WORLD"))!;
        foreach (var (name, entity, stock) in new[] { ("item", 144, new byte[]{0,1,3,4}), ("weapon",135,new byte[]{57,72,73,33,85,86,103}), ("default",135,Array.Empty<byte>()) })
        {
            var source = new PrivateExplorationReader(PrivateInputFactAttribute.RequireInput("SF2_GALAM_WORLD"),
                Path.Combine(root, "start-"+name+".json"), PrivateBattleScenarioTests.Selected(Path.Combine(root,"party.json")));
            var read=source.Read();Assert.True(read is ExplorationReadAccepted,read.ToString());
            var content=(ExplorationReadAccepted)read;
            var start=GameSession.Start(content.Definition,content.Start);Assert.True(start is SessionStarted,start.ToString());
            var session=((SessionStarted)start).Session;
            if(!session.Current.CanWaitAtInput) PrivateExplorationTests.RunUntilStop(session);
            Assert.True(session.Current.CanWaitAtInput,session.Current.StopReason.ToString());
            var map=session.Current.Exploration!.Definition;
            if(name=="default")
            {
                Assert.DoesNotContain(map.Events,e=>e.Program?.Program is "map16-297-entityevent7" or "map16-297-entityevent16");continue;
            }
            Assert.Contains(map.Events,e=>e.Entity==new EntityRef("entity-"+entity) && e.Program?.Program=="map16-297-entityevent"+(name=="item"?16:7));
            Accept(session,new Interact(new("entity-"+entity)));
            for(int tick=0;tick<60 && session.Current.Story.Wait is not ShopWait;tick++)
                Accept(session,new AdvanceSimulation(session.Current.Story.Wait?.Token));
            var wait=Assert.IsType<ShopWait>(session.Current.Story.Wait);Assert.Equal(stock,wait.Definition.Stock);
            var raw=content.Definition.PrivateDefinitions!;
            Assert.All(stock,id=>Assert.Equal(raw.Items[id].Price,wait.Definition.Items[id].Price));
            var buyer=session.Current.Exploration.Party.Actors.First().Actor;
            ShopServiceTests.Buy(session,stock[0],buyer);var gold=session.Current.Exploration.Party.Gold;
            ShopServiceTests.Pay(session);Assert.Equal(ShopResult.Bought,ShopServiceTests.Wait(session).Result);
            Assert.Equal(gold-wait.Definition.Items[stock[0]].Price,session.Current.Exploration.Party.Gold);
            ShopServiceTests.Continue(session);Accept(session,new BackShop(ShopServiceTests.Wait(session).Token));
            if(!session.Current.CanWaitAtInput) PrivateExplorationTests.RunUntilStop(session);Assert.True(session.Current.CanWaitAtInput);
            Assert.False(session.QuerySave().Allowed); // This slice grants no Map16 save recipe.
            var battle=content.Definition.Encounters.Values.First();
            Assert.Equal((ushort)20,battle.HealingItems[1].Power);Assert.True(battle.HealingItems[3].CuresPoison);
            Assert.True(battle.Spells[new("detox",1)].CuresPoison);
        }
    }
}
