using Sf2.Remake.Application.Content.Scenarios;
using Sf2.Remake.Application.Runtime.Exploration;
using Sf2.Remake.Application.Runtime.Persistence;
using Sf2.Remake.Domain.Battles;
using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Application.Gameplay.Sf2;

// Finite opening savepoint recipes. No ordinary on-load program is executed here.
internal sealed class Sf2OpeningRestore(int fieldRecipeRevision = 1) : IOpeningRestorePolicy
{
    private const string InitialRecipe = "new-game";
    private const string YeelRecipe = "yeel-field";
    private const string GransealRecipe = "granseal-field";
    private const string GransealSetup = "ms-map3-flag506";
    private const string GalamRecipe = "galam-field";
    private const string GalamDefault = "ms-map16";
    private const string GalamAfterRally = "ms-map16-flag663";
    private const string HawelRecipe = "hawel-field";
    private const string HawelDefault = "ms-map43";
    private const string HawelRevisit = "ms-map43-flag612";
    private static readonly MapId HawelHouse = new("map-43");
    private static readonly FieldReturnAnchor GransealChurch = new(new("map-3"), new(32, 13), 1);

    private static ExplorationSetupDefinition? SettledGransealSetup(ExplorationMapDefinition map, IReadOnlyList<int> flags) =>
        !flags.Contains(640) && map.SelectedSetup(flags) is { Value: GransealSetup } selected &&
        map.Setups.TryGetValue(selected, out var setup) ? setup : null;

    private static readonly FieldReturnAnchor GalamChurch = new(new("map-16"), new(27, 7), 1);

    private static bool AdmitsGalamSetup(ExplorationMapDefinition map, IReadOnlyList<int> flags, string setup)
    {
        if (flags.Contains(530) || flags.Contains(999) || map.Setup?.DefaultSetup.Value != GalamDefault)
            return false;
        return map.SelectedSetup(flags)?.Value switch
        {
            GalamDefault => setup == GalamDefault && map.Population is not null,
            GalamAfterRally when map.Setups.ContainsKey(new(GalamAfterRally)) =>
                setup == GalamAfterRally || setup == GalamDefault && map.Population is not null,
            _ => false,
        };
    }

    private static string LoadedGalamSetup(ExplorationMapDefinition source, ExplorationMapDefinition loaded,
        IReadOnlyList<int> flags)
    {
        // Setting F663 ends the rally without loading the alternate setup. Bind the body
        // actually used for Talk, Search and movement, including after a prior Continue.
        return LoadedSettledSetup(source, loaded, AdmitsGalamSetup(source, flags, GalamDefault) ? GalamDefault : null,
            AdmitsGalamSetup(source, flags, GalamAfterRally) ? GalamAfterRally : null);
    }

    // F612 selects the next visit. The completed first visit still uses its loaded default body.
    private static bool HasSettledHawelVisit(ExplorationMapDefinition map, IReadOnlyList<int> flags) =>
        flags.Contains(4) && flags.Contains(612) && flags.Contains(650) && !flags.Contains(72) &&
        map.Setup?.DefaultSetup.Value == HawelDefault && map.Population is not null &&
        map.SelectedSetup(flags)?.Value == HawelRevisit && map.Setups.ContainsKey(new(HawelRevisit));

    private static string LoadedSettledSetup(ExplorationMapDefinition source, ExplorationMapDefinition loaded,
        string? defaultSetup, string? alternateSetup) =>
        defaultSetup is not null && Sf2FieldSceneRestore.MatchesLoadedSetup(source, loaded, defaultSetup) ? defaultSetup :
        alternateSetup is not null && Sf2FieldSceneRestore.MatchesLoadedSetup(source, loaded, alternateSetup) ? alternateSetup :
        throw new BattleRuleException("save-field-scene-context", "save.fieldScene");

    private static MapSetupId? YeelSetup(ExplorationMapDefinition map, IReadOnlyList<int> flags)
    {
        var selected = map.SelectedSetup(flags);
        if (flags.Contains(650))
            return selected is { Value: "ms-map5-flag650" } && map.Setups.ContainsKey(selected) ? selected : null;
        return selected == map.Setup?.DefaultSetup ? selected : null;
    }

    private static (string Recipe, string Destination)? PacalonLocation(string map) => map switch
    {
        "map-2" => ("pacalon-town", "Pacalon"),
        "map-36" => ("pacalon-castle", "Pacalon Castle"),
        "map-71" => ("pacalon-road", "Pacalon, southern road"),
        _ => null
    };

    public int FieldRecipeRevision { get; } = fieldRecipeRevision;

    public IReadOnlyList<BattleActorStartInput> RestoreRoster(ScenarioDefinition definition, OpeningSaveData data,
        SaveCompatibility expected, IReadOnlyDictionary<ActorRef, BattleDeploymentDefinition> deployments,
        IReadOnlySet<ActorRef> saved)
    {
        if (data.Compatibility == expected)
        {
            if (!saved.SetEquals(deployments.Keys)) throw new BattleRuleException("save-roster", "save.allies");
            return [];
        }
        int prior = data.Compatibility.RecipeRevision;
        if (prior >= expected.RecipeRevision || data.Compatibility != (expected with { RecipeRevision = prior }))
            throw new BattleRuleException("save-incompatible", "save.compatibility");
        return Sf2HistoricalRoster.Missing(definition, prior, expected, deployments, saved, data.Flags,
            data.ActiveAllies.Concat(data.Names.Keys), data.MainSeed, "save-roster");
    }

    public SavedBattleStory UpgradeBattleRoster(ScenarioDefinition definition, BattleSaveData data,
        EngineBattleState battle, SaveCompatibility expected) =>
        Sf2HistoricalRoster.UpgradeBattle(definition, data, battle, expected);

    public SaveLocation InitialLocation(ScenarioDefinition definition, ExplorationStartInput start)
    {
        if (start.Party.NewBattle is not { Origin: BattleStartOrigin.NewGame, Difficulty: 0, AllyAutoBattle: false, OpponentControl: false } ||
            start.Map.Value != "map-3" || start.Player.Value != "entity-0" || start.Position != new MapPosition(56, 3) ||
            start.Facing != 3 || start.Speed != 32 || start.EntryProgram is not null || start.EntityPhases.Count != 0 ||
            !start.Flags.Order().SequenceEqual(new[] { 0, 32, 80 }))
            throw new BattleRuleException("new-game-initial-boundary", "newGame");
        return new(InitialRecipe, new(start.Map, start.Position, start.Facing), "Granseal, beginning of the opening");
    }

    public ExplorationStartInput? InitialStart(ScenarioDefinition definition, OpeningSaveData data, BattleStartInput party)
    {
        if (data.Recipe != InitialRecipe) return null;
        if (data.Map != "map-3" || data.X != 56 || data.Y != 3 || data.Facing != 3 || data.Layer != 0 ||
            !data.Flags.Order().SequenceEqual(new[] { 0, 32, 80 }) || data.Gold != 0 || data.OnceFlags.Count != 0 || data.Deals.Count != 0 || data.Depot.Count != 0 ||
            data.StepCounter != 0 || data.EgressMap is not ("map-0" or "map-3") || data.TextCursor != 0 || data.FadePeriod != 3 ||
            data.TextSettings != new ExplorationTextSettings(2, 0, 0) || data.Difficulty != 0 || data.AllyAutoBattle || data.OpponentControl ||
            !data.ActiveAllies.SequenceEqual(new[] { "ally-0" }) ||
            data.Allies.Any(ally => ally.Hp != ally.MaxHp || ally.Mp != ally.MaxMp || ally.Exp != 0 || ally.Kills != 0 ||
                ally.Defeats != 0 || ally.Status != 0 || ally.Absent) || data.Music.Previous.Count != 0)
            throw new BattleRuleException("save-initial-boundary", "save");
        var map = definition.Exploration!.Maps[new("map-3")];
        int area = map.Traversal.SelectActiveArea(new(56, 3))!.OneBasedRecordOrdinal - 1;
        if (definition.Exploration.Visuals!.Audio[data.Music.Cue].Command != definition.Exploration.Visuals.Maps[map.Map].Music[area].Field)
            throw new BattleRuleException("save-initial-music", "save");
        var palette = map.BasePalette!;
        return new(new("map-3"), new("entity-0"), new(56, 3), 3, 32, data.Flags, party,
            display: new(3, palette, palette, FullFadeVisibility.BaseRestored), textSettings: data.TextSettings,
            stepCounter: 0, egressMap: new(data.EgressMap)) { Names = data.Names };
    }

    public StoryState? RecordAtChurch(ScenarioDefinition definition, ExplorationState field, StoryState story)
    {
        // The house and the old castle rooms have no Church caller.
        if (field.Map == HawelHouse || Sf2CastleRestore.Contains(field.Map)) return null;
        var map = definition.Exploration!.Maps[field.Map];
        var point = map.ChurchSavepoint;
        if (field.Map == GransealChurch.Map && map.Setup?.FlagAlternatives.Any(row => row.Setup.Value == GransealSetup) == true)
        {
            // The source refuge also serves earlier battle returns. Only this visit changes Record.
            if (SettledGransealSetup(map, story.Flags) is null) return story;
            if (point != GransealChurch) return null;
        }
        if (field.Map == GalamChurch.Map && point != GalamChurch) return null;
        if (point is null) return story;
        return story.Copy(story.Cursor, story.Wait, flags: story.Flags.Append(399).Distinct().Order().ToArray(),
            egressMap: point.Map);
    }

    public void ValidateRegisters(OpeningSaveData data, ExplorationDefinition? world = null)
    {
        if ((data.Recipe == GransealRecipe || data.Map is "map-16" or "map-43" or "map-5" or "map-23" or "map-25" or "map-2" or "map-36") && data.FieldScene is null)
            throw new BattleRuleException("save-field-scene-required", "save.fieldScene");
        if (data.Recipe == Sf2CastleRestore.Recipe && data.FieldScene is null)
            throw new BattleRuleException("save-field-scene-required", "save.fieldScene");
        if ((data.Recipe is "elven-village" or "ketto" || PacalonLocation(data.Map)?.Recipe == data.Recipe) && data.EgressMap is "map-6" or "map-8" or "map-9" or "map-10" or "map-13") return;
        if (data.Recipe == "new-granseal-return" && data.Map is "map-6" or "map-7" && data.FieldScene is null)
            throw new BattleRuleException("save-field-scene-required", "save.fieldScene");
        if (data.Recipe == "new-granseal-return" && data.EgressMap is "map-6" or "map-8" or "map-9" or "map-10" or "map-13") return;
        if ((data.Map == "map-30" || data.Map == "map-33" && data.Flags.Any(flag => flag is 785 or 786 or 787)) && data.FieldScene is null)
            throw new BattleRuleException("save-field-scene-required", "save.fieldScene");
        if (world?.Maps.TryGetValue(new(data.EgressMap), out var town) == true &&
            town.ChurchSavepoint is not null && data.Flags.Contains(399)) return;
        if (data.Recipe == "evil-spirit-shrine" && data.EgressMap == "map-9") return;
        if (data.Recipe is "floor-world" or "willard-burrow" && data.EgressMap == "map-26") return;
        if (data.Recipe == "desktop-kingdom" && data.EgressMap == "map-22") return;
        // Preserve legacy saved zero without migration; the Witch New Game handoff writes Map3.
        if (data.Recipe is "new-granseal-castle" or "ribble-welcome" or "east-shrine" or "parmecia-battle-return" &&
            data.EgressMap is "map-6" or "map-8") return;
        if (data.Recipe is "dark-caves" or "mount-volcano" or "polca-return" or "polca-victory" or "polca-raft" or "hassan" &&
            data.EgressMap is "map-6" or "map-8" or "map-10" or "map-13") return;
        if (data.Recipe is "ancient-hall" or "taros-shrine" or "north-parmecia" or "southwest-parmecia" or "harpy-pond" or "fairy-cave" or "devils-tail" or "creed-mansion" or "dwarven-mine" or "desktop-kingdom" && data.EgressMap is "map-6" or "map-8" or "map-9" or "map-10" or "map-13") return;
        if (data.Recipe == "east-shrine" && data.Flags.Contains(514) && data.EgressMap is "map-9" or "map-10" or "map-13") return;
        if (data.Recipe == "hassan" && data.EgressMap == "map-9") return;
        if (data.Recipe == "parmecia-battle-return" && data.Map == "map-70" && data.Flags.Contains(514) &&
            data.EgressMap is "map-9" or "map-10" or "map-13") return;
        if (data.EgressMap is not ("map-0" or "map-3") &&
            !(data.Recipe is "battle07-return" or YeelRecipe or GransealRecipe or GalamRecipe or HawelRecipe or Sf2CastleRestore.Recipe &&
                data.EgressMap is "map-5" or "map-43" or "map-16"))
            throw new BattleRuleException("save-egress-map", "save.egressMap");
    }

    public bool CanDiscardEntityProgram(ExplorationState world, ExplorationEntity entity, IReadOnlyList<int> flags)
    {
        var actions = entity.Actions!.Actions;
        // A story can replace the population before setting the flag that selects it on reentry.
        bool Matches(ExplorationEntityDefinition row) => row.Actions is { } ambient && actions.SequenceEqual(ambient.Actions);
        if (world.Definition.Entities.Any(Matches)) return true;
        var selected = world.Definition.SelectedSetup(flags);
        if (selected is not null && world.Definition.Setups.TryGetValue(selected, out var setup) &&
            setup.Entities.Any(Matches)) return true;
        // The completed Map3 messenger installs wandering with a new origin. Only its
        // already-entered closed random-walk/wait/back-jump cycle can reset. Pending
        // setup and scripted movement cannot be discarded at a savepoint.
        if (world.Map.Value != "map-3") return false;
        for (int index = 0; index + 2 < actions.Count; index++)
            if (actions[index] is RandomWalkEntity && actions[index + 1] is WaitEntityTicks &&
                actions[index + 2] is JumpEntityAction jump && jump.Instruction == index &&
                entity.ActionCursor >= index && entity.ActionCursor <= index + 2)
                return true;
        return false;
    }

    public SaveLocation Select(ScenarioDefinition definition, FieldReturnAnchor player, IReadOnlyList<int> flags, FieldReturnAnchor? anchor)
    {
        var map = player.Map;
        if (!definition.Exploration!.Maps.TryGetValue(map, out var content)) Fail();
        if (map.Value is "map-6" or "map-7" or "map-73" && flags.Contains(802) &&
            content!.Traversal.IsWithinActiveArea(player.Position))
            return new("new-granseal-return", player, "New Granseal, return journey");
        if (map.Value == "map-3")
        {
            if (SettledGransealSetup(content!, flags) is not null && content!.Traversal.IsWithinActiveArea(player.Position))
                return new(GransealRecipe, player, "Granseal");
            var selected = content!.SelectedSetup(flags);
            if (selected != content.Setup?.DefaultSetup &&
                !(selected?.Value == "ms-map3-flag609" && flags.Contains(603)))
                Fail();
            return flags.Contains(603)
                ? new("map3-joined", GransealChurch, "Granseal, outside the church")
                : new("map3-opening", new(map, new(56, 3), 3), "Granseal, home");
        }
        if (map == GalamChurch.Map && content!.Traversal.IsWithinActiveArea(player.Position) &&
            (AdmitsGalamSetup(content, flags, GalamDefault) || AdmitsGalamSetup(content, flags, GalamAfterRally)))
            return new(GalamRecipe, player, "Galam");
        if (map == HawelHouse && HasSettledHawelVisit(content!, flags) && content!.Traversal.IsWithinActiveArea(player.Position))
            return new(HawelRecipe, player, "Hawel's House");
        if (Sf2CastleRestore.Contains(map) && Sf2CastleRestore.SettledSetup(content!, flags) is not null &&
            content!.Traversal.IsWithinActiveArea(player.Position))
            return new(Sf2CastleRestore.Recipe, player, "Granseal Castle");
        if (map.Value == "map-5" && YeelSetup(content!, flags) is not null &&
            content!.Traversal.IsWithinActiveArea(player.Position))
            return new(YeelRecipe, player, "Yeel");
        if (map.Value == "map-57" && flags.Contains(501) && !flags.Contains(506) &&
            anchor is { Map.Value: "map-57", Facing: 3 })
            return new("battle01-return", anchor, "Ancient Tower, completed battle return");
        if (map.Value == "map-58" && flags.Contains(507) && flags.Contains(74) && flags.Contains(640) &&
            !flags.Contains(407) && !flags.Contains(999) && anchor is { Map.Value: "map-58", Facing: 3 })
            return new("battle07-return", anchor, "Ancient Tower, earthquake aftermath");
        if (map.Value == "map-7" && flags.Contains(707) && !flags.Contains(701))
            return new("new-granseal-castle", new(map, new(11, 7), 1), "New Granseal Castle");
        if (map.Value == "map-8" && flags.Contains(708))
            return new("ribble-welcome", new(map, new(14, 13), 1), "Ribble, after the welcome");
        if (map.Value == "map-62" && flags.Contains(510) && flags.Contains(729) && flags.Contains(77))
            return new("east-shrine", new(map, new(6, 18), 1), "East Shrine");
        if (map.Value == "map-12" && flags.Contains(518) && flags.Contains(725) && flags.Contains(65) &&
            content!.Traversal.SelectActiveArea(player.Position) is not null)
            return new("taros-shrine", player, "Taros Shrine, after the Caravan");
        if (map.Value == "map-67" && flags.Contains(521) && content!.Traversal.IsWithinActiveArea(player.Position))
            return new("devils-tail", player, "Devil's Tail");
        // First visit and completed restoration retain their separate source init tails.
        if (map.Value == "map-33" && !flags.Contains(22) &&
            (!flags.Contains(523) || flags.Contains(783)) &&
            content!.Traversal.IsWithinActiveArea(player.Position))
            return new("creed-mansion", player, "Creed's Mansion");
        if (!flags.Contains(530) && PacalonLocation(map.Value) is { } pacalon && content!.Traversal.IsWithinActiveArea(player.Position))
            return new(pacalon.Recipe, player, pacalon.Destination);
        if (map.Value == "map-23" && content!.Traversal.IsWithinActiveArea(player.Position))
            return new("elven-village", player, "Elven Village");
        if (map.Value == "map-25" && content!.Traversal.IsWithinActiveArea(player.Position))
            return new("ketto", player, "Ketto");
        if (map.Value == "map-30" && content!.Traversal.IsWithinActiveArea(player.Position))
            return new("dwarven-mine", player, "Dwarven Mine");
        if (map.Value == "map-34" &&
            content!.Traversal.IsWithinActiveArea(player.Position))
            return new("evil-spirit-shrine", player, "Evil Spirit Shrine");
        // A nonvictory return retains F422; completion requires the separate aftermath to return.
        if (map.Value == "map-22" && flags.Contains(776) && (!flags.Contains(522) || flags.Contains(778)) &&
            content!.Traversal.IsWithinActiveArea(player.Position))
            return new("desktop-kingdom", player, "Desktop Kingdom");
        if (map.Value == "map-26" && flags.Contains(779) && content!.Traversal.IsWithinActiveArea(player.Position))
            return new("floor-world", player, "Floor World");
        if (map.Value == "map-27" && content!.Traversal.IsWithinActiveArea(player.Position))
            return new("willard-burrow", player, "Willard Burrow");
        if (map.Value == "map-32" && content!.Traversal.IsWithinActiveArea(player.Position))
            return new("fairy-cave", player, "Fairy Cave");
        if (map.Value == "map-51" && !flags.Contains(420) && flags.Contains(470) == flags.Contains(520) &&
            content!.Traversal.IsWithinActiveArea(player.Position) && (content.Layout[player.Position.X, player.Position.Y] & 0xFC00) == 0)
            return new("harpy-pond", player, "Harpy Pond");
        if (map.Value == "map-68" && flags.Contains(65) && CaravanRuntime.HasOverworldPopulation(content!.SelectSetup(flags).Population) &&
            content.Traversal.IsWithinActiveArea(player.Position))
            return new("southwest-parmecia", player, "Southwest Parmecia");
        if (anchor?.Map == map && (map.Value == "map-69" && flags.Contains(509) ||
            map.Value == "map-70" && flags.Contains(510)))
            return new("parmecia-battle-return", anchor, "Eastern Parmecia, battle return");
        if (anchor?.Map == map && map.Value == "map-48" && flags.Contains(511))
            return new("dark-caves", anchor, "Dark Caves, battle return");
        if (anchor?.Map == map && map.Value == "map-52" && flags.Contains(512))
            return new("mount-volcano", anchor, "Mount Volcano, battle return");
        if (map.Value == "map-13" && flags.Contains(799) && flags.Contains(710) && !flags.Contains(715))
            return new("polca-return", new(map, new(30, 6), 2), "Polca, eastern approach");
        if (map.Value == "map-13" && flags.Contains(513) && !flags.Contains(716) && anchor?.Map == map)
            return new("polca-victory", anchor, "Polca, battle return");
        if (map.Value == "map-72" && flags.Contains(524) && content!.Traversal.IsWithinActiveArea(player.Position))
            return new("north-parmecia", player, "North Parmecia");
        if (map.Value == "map-72" && flags.Contains(64) && flags.Contains(10) &&
            player.Position == new MapPosition(43, 47))
            return new("polca-raft", player, "Polca, raft landing");
        if (map.Value == "map-50" && flags.Contains(515) && flags.Contains(732) &&
            content!.Traversal.SelectActiveArea(new(27, 11))?.Area.Contains(player.Position) == true)
            return new("ancient-hall", new(map, new(27, 11), 1), "Ancient Hall, shrine entrance");
        if (map.Value == "map-9")
            return new("hassan", new(map, new(8, 9), 1), "Hassan");
        Fail(); return null!;
    }

    public SavedFieldScene? CaptureScene(ScenarioDefinition definition, ExplorationState world, IReadOnlyList<int> flags) =>
        world.Map.Value == "map-72" && flags.Contains(256) && definition.Exploration!.Programs.ContainsKey("cs-4ffda")
            ? Sf2FieldSceneRestore.Capture(definition, RaftRuntime.OnScroll(world, flags, null), definition.Exploration.Maps[world.Map].Setup!.DefaultSetup.Value) :
        world.Map.Value is "map-6" or "map-7" && flags.Contains(802) ? Sf2NewGransealRestore.Capture(definition, world) :
        world.Map == GransealChurch.Map &&
            SettledGransealSetup(definition.Exploration!.Maps[world.Map], flags) is { } granseal &&
            world.Definition.OnLoad == granseal.OnLoad && world.Definition.Entities.SequenceEqual(granseal.Entities) &&
            world.Definition.Events.SequenceEqual(granseal.Events)
                ? Sf2FieldSceneRestore.Capture(definition, world, GransealSetup) :
        world.Map == GalamChurch.Map ? Sf2FieldSceneRestore.Capture(definition, world,
            LoadedGalamSetup(definition.Exploration!.Maps[world.Map], world.Definition, flags)) :
        world.Map == HawelHouse ? Sf2FieldSceneRestore.Capture(definition, world,
            LoadedSettledSetup(definition.Exploration!.Maps[world.Map], world.Definition, HawelDefault, HawelRevisit)) :
        Sf2CastleRestore.Contains(world.Map) ? Sf2CastleRestore.Capture(definition, world, flags) :
        world.Map.Value == "map-5" ? Sf2FieldSceneRestore.Capture(definition, world,
            YeelSetup(definition.Exploration!.Maps[world.Map], flags)!.Value) :
        world.Map.Value == "map-23" ? Sf2FieldSceneRestore.Capture(definition, world, definition.Exploration!.Maps[world.Map].SelectedSetup(flags)!.Value) :
        world.Map.Value is "map-25" or "map-2" or "map-36" ? Sf2FieldSceneRestore.Capture(definition, world, definition.Exploration!.Maps[world.Map].Setup!.DefaultSetup.Value) :
        world.Map.Value == "map-30" ? Sf2MineRestore.Capture(definition, world) : Sf2CreedRestore.Capture(definition, world, flags);

    public ExplorationState Rebuild(ScenarioDefinition definition, SaveLocation location, BattleStartInput party,
        IReadOnlyList<int> flags, RaftBerth? raft, out IReadOnlyList<int> enteredFlags, byte layer = 0,
        SavedFieldScene? scene = null)
    {
        if (location.Recipe == Sf2CastleRestore.Recipe)
        {
            enteredFlags = flags;
            return Sf2CastleRestore.Rebuild(definition, location, party, flags, scene, layer);
        }
        if (location.Recipe is GransealRecipe or GalamRecipe or HawelRecipe)
        {
            var sourceMap = definition.Exploration!.Maps[location.Anchor.Map];
            if (scene is null || !(location.Recipe switch
                {
                    GalamRecipe => AdmitsGalamSetup(sourceMap, flags, scene.Setup),
                    HawelRecipe => HasSettledHawelVisit(sourceMap, flags) && scene.Setup is HawelDefault or HawelRevisit,
                    _ => scene.Setup == SettledGransealSetup(sourceMap, flags)?.Id.Value,
                }))
                throw new BattleRuleException("save-field-scene-context", "save.fieldScene");
            var town = Sf2FieldSceneRestore.Rebuild(definition, location, party, flags, scene, out _, layer);
            // Galam and Hawel retire actors in place and retain their physical records.
            if (location.Recipe is GalamRecipe or HawelRecipe &&
                town.Definition.Entities.Any(row => !town.AllEntities.Any(actor => actor.Entity == row.Entity)))
                throw new BattleRuleException("save-field-scene-actor", "save.fieldScene");
            if (location.Recipe == HawelRecipe && scene.Setup == HawelDefault &&
                (!town.TryResolveEntity(new("entity-4"), out var kazin) || kazin.Entity.Value != "entity-4" ||
                 kazin.Follower?.LeaderSlot != town.PlayerEntity.Slot))
                throw new BattleRuleException("save-field-scene-actor", "save.fieldScene");
            Sf2FieldSceneRestore.RequireUnoccupiedLanding(town);
            enteredFlags = flags;
            return town;
        }
        if (location.Anchor.Map.Value == "map-5")
        {
            var sourceMap = definition.Exploration!.Maps[location.Anchor.Map];
            if (scene is null || location.Recipe != YeelRecipe || scene.Setup != YeelSetup(sourceMap, flags)?.Value)
                throw new BattleRuleException("save-field-scene-context", "save.fieldScene");
            var town = Sf2FieldSceneRestore.Rebuild(definition, location, party, flags, scene, out _, layer);
            enteredFlags = flags;
            return town;
        }
        if (scene is not null && location.Anchor.Map.Value == "map-72")
        {
            var cliff = definition.Exploration!.Maps[location.Anchor.Map];
            if (location.Recipe != "north-parmecia" || scene.Setup != cliff.Setup?.DefaultSetup.Value || !flags.Contains(256))
                throw new BattleRuleException("save-field-scene-context", "save.fieldScene");
            var allocations = flags.Contains(750) && definition.Exploration.Programs.TryGetValue("cs-4ffda", out var blast)
                ? blast.Instructions.OfType<AddMapEntity>().ToArray() : [];
            var loaded = Sf2FieldSceneRestore.Rebuild(definition, location, party, flags, scene, out _, layer, allocations);
            // Continue the saved encounter visit. A fresh map transfer owns the temporary reset.
            enteredFlags = flags;
            return RaftRuntime.Declare(loaded, flags, raft);
        }
        if (scene is not null && location.Anchor.Map.Value is "map-6" or "map-7")
            return Sf2NewGransealRestore.Rebuild(definition, location, party, flags, scene, out enteredFlags, layer);
        if (scene is not null && location.Anchor.Map.Value is "map-23" or "map-25" or "map-2" or "map-36")
        {
            var sourceMap = definition.Exploration!.Maps[location.Anchor.Map];
            var village = sourceMap.Map.Value == "map-23";
            var recipe = village ? "elven-village" : PacalonLocation(sourceMap.Map.Value)?.Recipe ?? "ketto";
            var setup = village ? sourceMap.SelectedSetup(flags) : sourceMap.Setup?.DefaultSetup;
            if (location.Recipe != recipe || scene.Setup != setup?.Value ||
                setup != sourceMap.Setup?.DefaultSetup && !sourceMap.Setups.ContainsKey(new(scene.Setup)))
                throw new BattleRuleException("save-field-scene-context", "save.fieldScene");
            var town = Sf2FieldSceneRestore.Rebuild(definition, location, party, flags, scene, out _, layer);
            enteredFlags = flags;
            // The live merchant finished in idle. Reinstalling eas_Init here would reset his speed.
            if (sourceMap.Map.Value == "map-25" && flags.Contains(800) && town.TryResolveEntity(new("entity-133"), out var merchant))
                town = town.WithEntity(merchant with { Actions = null });
            // The completed castle scene leaves the centaur idle at speed 8. A fresh visit
            // with the same flag still starts his original ambient script at speed 32.
            if (sourceMap.Map.Value == "map-36" && flags.Contains(810) &&
                town.TryResolveEntity(new("entity-131"), out var centaur) && centaur.Motion.XSpeed == 8)
                town = town.WithEntity(centaur with { Actions = null });
            return town;
        }
        if (scene is not null && location.Anchor.Map.Value == "map-30")
            return Sf2MineRestore.Rebuild(definition, location, party, flags, scene, out enteredFlags, layer);
        if (scene is not null)
            return Sf2CreedRestore.Rebuild(definition, location, party, flags, scene, out enteredFlags, layer);
        var anchor = location.Anchor;
        var source = definition.Exploration!.Maps[anchor.Map];
        var selected = source.SelectedSetup(flags);
        var map = source.SelectSetup(flags);
        var entered = flags.ToHashSet();
        foreach (var write in map.EntryFlags)
            if (write.Value) entered.Add(write.Flag); else entered.Remove(write.Flag);
        enteredFlags = Array.AsReadOnly(entered.Order().ToArray());
        if (layer is not (0 or 2)) throw new BattleRuleException("save-player-layer", "save.layer");
        // Secondary landings may be blocked in the primary layout. Build the ordinary population,
        // then validate the saved layer against the rebuilt layout through the movement accessor.
        var world = MapTransfer.Build(map, new("entity-0"), anchor.Position, anchor.Facing, 32, party, flags, enteredFlags,
            sourcePlacement: layer != 0);
        world = world.WithEntities(world.AllEntities.Select(entity => entity.Slot == world.PlayerEntity.Slot || entity.Follower is not null
            ? entity with { Motion = entity.Motion with { Layer = layer } } : entity));
        if (FieldLayout.Word(world, world.PlayerEntity.Motion, anchor.Position.X, anchor.Position.Y) is not < 0xC000)
            throw new BattleRuleException("save-anchor-unusable", "save.anchor");
        if (anchor.Map.Value == "map-3" && selected?.Value == "ms-map3-flag609")
        {
            // ms_map3_flag609 init retires Jaha only after his actual join.
            if (flags.Contains(3)) Hide("entity-3", true);
        }
        else if (anchor.Map.Value == "map-3")
        {
            // ms_map3 init: joined Sarah retires entity142; otherwise F602 moves Sarah.
            if (flags.Contains(1)) Hide("entity-142", true);
            else if (flags.Contains(602) && world.TryResolveEntity(new("entity-1"), out var sarah))
                world = world.WithEntity(sarah with { Motion = sarah.Motion with
                    { X = 41 * 384, Y = 10 * 384, XDestination = 41 * 384, YDestination = 10 * 384,
                        Facing = 1, XSpeed = 32, YSpeed = 32 } });
            if (flags.Contains(603)) Hide("entity-142", false);
        }
        if (anchor.Map.Value == "map-8" && flags.Contains(8)) Hide("entity-8", true);
        if (anchor.Map.Value == "map-9")
        {
            // Both Hassan setups execute the same init. A zero lookup can resolve to physical 0.
            if (!flags.Contains(518)) world = world.MoveOffMap(new("entity-137"));
            if (flags.Contains(65)) world = world.MoveOffMap(new("entity-11"));
            if (flags.Contains(12)) world = world.MoveOffMap(new("entity-12"));
            if (world.PlayerEntity.Motion.X == 0x7000)
                throw new BattleRuleException("save-anchor-unusable", "save.anchor");
        }
        if (location.Recipe == "southwest-parmecia" && (world.Layout[anchor.Position.X, anchor.Position.Y] & 0x3C00) is not (0 or 0x1400))
            throw new BattleRuleException("save-anchor-unusable", "save.anchor");
        if (location.Recipe == "taros-shrine")
            world = world.MoveOffMap(new("entity-128"));
        // Fresh population keeps the ring worker's lookup; the init tail parks
        // him after a completed attempt, unlike the scene's same-visit Hide.
        if (location.Recipe == "floor-world" && flags.Contains(780))
            world = world.MoveOffMap(new("entity-129"));
        if (location.Recipe == "creed-mansion" && flags.Contains(784) && !flags.Contains(786))
        {
            // The post-consultation init places the still-unjoined offer actors only.
            foreach (var (member, x, y) in new[] { (14, 15, 14), (16, 16, 15), (17, 18, 15), (15, 19, 14) })
                if (!flags.Contains(member) && world.TryResolveEntity(new("entity-" + member), out var offer))
                    world = world.WithEntity(offer with { Motion = offer.Motion with
                        { X = (short)(x * 384), Y = (short)(y * 384), XDestination = (short)(x * 384), YDestination = (short)(y * 384), Facing = 3 } });
        }
        else if (location.Recipe == "creed-mansion" && (flags.Contains(783) || flags.Contains(774)))
        {
            // ms_map33_InitFunction / cs_5ABE6: reentry places Goliath only.
            // F774 does not replay cs_5AC58's two live door copies or temporary followers.
            var goliath = world.Entities[new("entity-128")];
            world = world.WithEntity(goliath with { Motion = goliath.Motion with
                { X = 17 * 384, Y = (short)((flags.Contains(783) ? 10 : 12) * 384),
                    XDestination = 17 * 384, YDestination = (short)((flags.Contains(783) ? 10 : 12) * 384), Facing = 3 } });
        }
        // Allocation starts followers at the leader. This is the same legal overlap as a map reload;
        // their ordinary following service establishes spacing on the next deliberate input.
        if (world.AllEntities.Any(entity => entity.Visible && entity.Slot != world.PlayerEntity.Slot &&
                entity.Follower is null && entity.Motion.X / 384 == anchor.Position.X && entity.Motion.Y / 384 == anchor.Position.Y))
            throw new BattleRuleException("save-anchor-occupied", "save.anchor");
        world = RaftRuntime.Declare(world, enteredFlags, raft);
        if (location.Recipe == "polca-raft")
        {
            var approach = MapEventDispatcher.Move(world, ExplorationDirection.South, enteredFlags);
            if (raft?.Map != world.Map || approach.Outcome.VehicleEvent != 3 || !RaftRuntime.Aligned(approach.World))
                throw new BattleRuleException("save-raft-berth", "save.raft");
        }
        return world;

        void Hide(string id, bool aliases)
        {
            if (world.TryResolveEntity(new(id), out var entity) && entity.Entity.Value == id)
                world = world.Hide(entity, aliases);
        }
    }
    private static void Fail() => throw new BattleRuleException("save-location-unavailable", "save.recipe", true);
}
