using Sf2.Remake.Domain.Maps;
using Sf2.Remake.Domain.Gameplay.Sf2;

namespace Sf2.Remake.Domain.Battles;

internal enum BattleReactionKind { Damage, Dodge, Ineffective, Recovery, Resource, Status, CurseRecoil, InstantDeath }
internal sealed record BattleReaction(ActorRef Actor, ActorRef Target, string Action,
    BattleReactionKind Kind, ushort HpBefore, ushort HpAfter, byte MpBefore, byte MpAfter,
    bool Critical = false, int Amount = 0, ushort? StatusWrite = null, ushort? InflictedSleep = null,
    BattleItemReceipt? Receipt = null, ushort? InflictedSilence = null, ushort? InflictedPoison = null, ushort? InflictedStun = null,
    ushort ClearedStatus = 0);
internal sealed record BattleActionReward(ActorRef Actor, int Amount);
internal sealed record PhysicalInaction(ActorRef Target, bool DueToCurse);
internal sealed record BattleAutomaticAction(EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects,
    MapPosition Destination, IReadOnlyList<MapPosition> Path, ActorRef? Target = null, bool QueueOnly = false, BattleActionRef? ChosenAction = null)
{
    internal WeightedMovementGrid? MovementGrid { get; init; }
    internal OffensiveTargetSide Side { get; init; }
    internal bool Guarded { get; init; }

    internal void Validate(EngineBattleState before, ActorRef actorRef)
    {
        static void Require(bool valid)
        { if (!valid) throw new InvalidOperationException("Invalid automatic decision."); }
        var actor = before.GetActor(actorRef);
        bool temporary = BattleControlRules.UsesTemporaryAllyAi(actor);
        Require(Side == (Target is null ? OffensiveTargetSide.Opponents : BattleOffense.AutomaticSide(actor)));
        bool protectedCenter = Side == OffensiveTargetSide.OwnSide && Target is { } center &&
            BattleOffense.IsProtected(before, actor, center);
        bool selfGuard = Side == OffensiveTargetSide.OwnSide && !protectedCenter && Target == actorRef;
        if (Side == OffensiveTargetSide.OwnSide)
            Require(Target is { } ownTarget && BattleOffense.OwnTarget(before, actor, ownTarget));
        if (Guarded)
            Require(Side == OffensiveTargetSide.OwnSide && !QueueOnly && Path.Count == 1 && Destination == actor.Position);
        bool itemAttack = false, healingItem = false;
        if (temporary && !QueueOnly && Target is not null &&
            ChosenAction is { Kind: BattleActionKind.Item, Spell: null, ItemSlot: { } itemSlot })
        {
            itemAttack = itemSlot == EnemySpellDecision.AllyAttackItemSlot(before.Definition, actor);
            healingItem = Target == actorRef && itemSlot == EnemySpellDecision.AllyRainSlot(before.Definition, actor);
        }
        Require(itemAttack || healingItem || ChosenAction is null || ChosenAction == new BattleActionRef(BattleActionKind.PrismLaser) ||
            ChosenAction is { } sword && TarosEffectiveness.IsSwordUse(actor, sword) && Target is not null && !QueueOnly ||
            ChosenAction is { Kind: BattleActionKind.AttackSpell or BattleActionKind.Healing or BattleActionKind.SupportSpell, Spell: not null, ItemSlot: null } &&
                Target is not null && !QueueOnly);
        if (ChosenAction?.Kind == BattleActionKind.PrismLaser)
        {
            PrismLaser.RequireActor(before, actorRef);
            Require(Target is not null && Path.Count == 1 && Destination == actor.Position && !QueueOnly &&
                ReferenceEquals(Battle, before) && Effects.Count == 0 &&
                PrismLaser.Targets(before, actorRef, alliesOnly: true).FirstOrDefault()?.Actor == Target);
        }
        Require(ReferenceEquals(Battle.Definition, before.Definition) &&
            Battle.Round == before.Round && Battle.Cursor == before.Cursor && Battle.Gold == before.Gold &&
            ReferenceEquals(Battle.ItemCustody, before.ItemCustody) && Equals(Battle.DeathCredit, before.DeathCredit) &&
            ReferenceEquals(Battle.StartPolicy, before.StartPolicy) && ReferenceEquals(Battle.Visuals, before.Visuals) && Battle.Queue.SequenceEqual(before.Queue) &&
            Battle.Actors.Count == before.Actors.Count &&
            (actor.Control == BattleControl.Automatic || temporary) &&
            actor.Hp > 0 && actor.Position is not null);
        Require(before.Regions is null ? Battle.Regions is null : Battle.Regions is { } regions &&
            regions.Flags.SequenceEqual(before.Regions.Flags) && (regions.Tested == before.Regions.Tested || regions.Tested == 0));
        for (int index = 0; index < before.Actors.Count; index++)
        {
            var old = before.Actors[index]; var candidate = Battle.Actors[index];
            Require(ReferenceEquals(candidate.Deployment, old.Deployment) && candidate.Hp == old.Hp && candidate.Initialized == old.Initialized &&
                candidate.Mp == old.Mp && candidate.Exp == old.Exp && candidate.Position == old.Position && candidate.Facing == old.Facing &&
                candidate.Kills == old.Kills && candidate.Defeats == old.Defeats && candidate.Attack == old.Attack &&
                candidate.Status == old.Status && candidate.LastRefreshBits == old.LastRefreshBits && candidate.UsesInitialStats == old.UsesInitialStats && Equals(candidate.Progress, old.Progress) &&
                ReferenceEquals(candidate.SourceLoadout, old.SourceLoadout));
            if (old.Actor != actorRef)
                Require(candidate.LastTarget == old.LastTarget && candidate.AiMemory == old.AiMemory &&
                    candidate.ActivationWord == old.ActivationWord && candidate.PrimaryOrder == old.PrimaryOrder &&
                    candidate.SecondaryOrder == old.SecondaryOrder);
            else if (candidate.LastTarget != old.LastTarget)
                Require(!Guarded && Target is not null && candidate.LastTarget == (ChosenAction?.ItemSlot is not null ? actorRef : Target));
            if (old.Actor == actorRef && (candidate.PrimaryOrder != old.PrimaryOrder || candidate.SecondaryOrder != old.SecondaryOrder))
            {
                Require(old.PrimaryOrder is not null and not 255 && (old.PrimaryOrder.Value & 64) == 0 &&
                    before.Definition.Initialization is { } initialization &&
                    initialization.SourceCombatants.TryGetValue(old.PrimaryOrder.Value, out var followed) &&
                    before.GetActor(followed).Hp == 0 && candidate.PrimaryOrder == old.SecondaryOrder && candidate.SecondaryOrder == 255);
            }
        }
        Require(Path.Count > 0 && Path[0] == actor.Position && Path[^1] == Destination &&
            before.Definition.Contains(Destination));
        int cost = 0;
        for (int index = 1; index < Path.Count; index++)
        {
            var position = Path[index];
            Require(before.Definition.Contains(position) && BattleRange.Contains(Path[index - 1], position, 1, 1));
            int step = BattleTerrainRules.MovementCost(before.Definition.Terrain[position.Y * 48 + position.X], actor.Definition.Mover);
            Require(step > 0); cost += step;
            // Physical routes block living opponents; no-target source movement owns its occupancy policy.
            if (Target is not null)
                Require(!before.Actors.Any(unit => unit.Hp > 0 && unit.Faction != actor.Faction &&
                    unit.Position == position));
        }
        if (cost > actor.Move * 2)
        {
            // The source descending-cost walker can retain an earlier direction bit and
            // travel farther than the destination's propagated cost. Admit that exact path.
            // Reuse only the decision's obstruction footprint. Terrain costs, origin
            // and MOV always come from the authoritative battle.
            var grid = MovementGrid is null ? BattleMovement.Grid(before, actorRef) :
                BattleTerrainRules.MovementGrid(before.Definition.Terrain, actor.Position!, actor.Definition.Mover,
                    actor.Move, Enumerable.Range(0, MovementGrid.MovableGrid.Count)
                        .Where(index => MovementGrid.CostAtOffset(index) is null)
                        .Select(index => new MapPosition(index % 48, index / 48)));
            Require(grid.CostAt(Destination) is { } destinationCost && destinationCost <= actor.Move * 2 &&
                Path.SequenceEqual(BattleMovement.Route(actor.Position!, BattleMovement.MoveString(grid, actor.Position!,
                    Destination, before.Definition.Width, before.Definition.Height))));
        }
        Require(!before.Actors.Any(unit => unit.Actor != actorRef && unit.Hp > 0 && unit.Position == Destination));
        if (Target is { } target)
            Require(before.Actors.Any(unit => unit.Actor == target && unit.Hp > 0 && unit.Position is not null &&
                BattleOffense.Matches(actor, unit,
                    ChosenAction?.Kind == BattleActionKind.Healing || healingItem ? OffensiveTargetSide.OwnSide : Side)));
        uint seed = before.ThinkingSeed;
        uint main = before.MainSeed;
        bool hasPhysical = false, hasItem = false, hasSpell = false, mainCategory = false;
        ushort? thinkingCategory = null;
        BattleEffect? selfRoll = null;
        foreach (var effect in Effects)
        {
            Require(!string.IsNullOrWhiteSpace(effect.Kind) && effect.Actor == actorRef &&
                (effect.Target is null || before.Actors.Any(unit => unit.Actor == effect.Target)));
            if (effect.Kind is "ai-candidate" or "ai-item-candidate" or "ai-spell-candidate")
            {
                Require(selfRoll is null && !mainCategory && thinkingCategory is null);
                hasPhysical |= effect.Kind == "ai-candidate";
                hasItem |= effect.Kind == "ai-item-candidate";
                hasSpell |= effect.Kind == "ai-spell-candidate";
            }
            if (effect.Kind == "ai-attack-unselected")
            {
                // An ordered command can finish an ATTACK without a target and advance to
                // another one. Preserve the seeds and close only this attempt's category.
                Require(!actor.IsAlly && MuddleRules.Confused(actor.Status) && selfRoll is null &&
                    (hasPhysical || hasItem || hasSpell) && (hasItem && hasSpell) == thinkingCategory.HasValue &&
                    effect == new BattleEffect("ai-attack-unselected", actorRef));
                hasPhysical = hasItem = hasSpell = mainCategory = false;
                thinkingCategory = null;
                continue;
            }
            if (effect.Kind == "rng-muddle-self-guard")
            {
                Require(selfGuard && selfRoll is null && effect == BattleOffense.GuardRoll(actorRef, main));
                selfRoll = effect;
                main = checked((uint)effect.After!.Value);
                continue;
            }
            if (effect.Kind == "rng-ai-attack-category")
            {
                var category = BattleRandom.NextMain(main, 6);
                Require(selfRoll is null && hasItem != hasSpell && !mainCategory && thinkingCategory is null &&
                    effect.RandomRange == category.Range && effect.Target is null &&
                    effect.Before == main && effect.After == category.After && effect.RandomValue == category.Value);
                mainCategory = true;
                main = category.After; continue;
            }
            if (effect.RandomRange is null)
            { Require(effect.RandomValue is null && !effect.Kind.StartsWith("rng-", StringComparison.Ordinal)); continue; }
            if (effect.Kind == "thinking-ai-attack-category")
            {
                Require(selfRoll is null && temporary && hasItem && hasSpell && thinkingCategory is null &&
                    !mainCategory && effect.RandomRange == 2 && effect.Target is null && effect.RandomValue.HasValue);
                thinkingCategory = effect.RandomValue;
            }
            else Require(selfRoll is null && !mainCategory && thinkingCategory is null &&
                effect.Kind == "thinking-rng" && effect.RandomRange <= byte.MaxValue);
            var draw = BattleRandom.NextThinkingWord((ushort)(seed >> 16), (byte)effect.RandomRange!.Value);
            uint after = ((uint)draw.After << 16) | (seed & 65535);
            Require(effect.Before == seed && effect.After == after && effect.RandomValue == draw.Value);
            seed = after;
        }
        Require(selfGuard == (selfRoll is not null) && Guarded == (protectedCenter || selfRoll?.RandomValue == 1));
        Require((hasItem && hasSpell) == thinkingCategory.HasValue &&
            (thinkingCategory is null || Target is null ||
                (thinkingCategory == 1 ? itemAttack : ChosenAction is { Kind: BattleActionKind.AttackSpell })) &&
            Battle.ThinkingSeed == seed && Battle.MainSeed == main);
        if (QueueOnly) Require(ReferenceEquals(Battle, before) && Path.Count == 1 && Target is null);
    }
}

// Construction and replay are separate source boundaries. Prepared state contains movement,
// construction RNG, gold and inventory; reactions and giveExp have not run yet.
internal sealed record BattleActionResolution(EngineBattleState Prepared, ActorRef Actor,
    MapPosition Destination, IReadOnlyList<BattleReaction> Reactions, BattleActionReward? Reward,
    IReadOnlyList<BattleEffect> ConstructionEffects, IReadOnlyList<BattleEffect> CompletionEffects,
    HealingItemDefinition? Item = null, HealingSpellDefinition? Spell = null, RetreatPayment? Retreat = null, PrismLaserShot? Laser = null, AttackSpellDefinition? AttackSpell = null, SupportSpellDefinition? SupportSpell = null, SilencedCast? Silenced = null, int? AttackItemSlot = null, ushort? CasterCostStatus = null,
    AttackItemDefinition? AttackItem = null, EquipmentItemUse? EquipmentUse = null)
{
    internal PhysicalInaction? Inaction { get; init; }
    internal int? MuddleMessage { get; init; }
    internal OffensiveTargetSide Side { get; init; }

    internal void Validate(EngineBattleState before, ActorRef actor, MapPosition destination,
        BattleActionRef action, ActorRef? target, BattleActionAdmission admission,
        OffensiveTargetSide side = OffensiveTargetSide.Opponents)
    {
        Require(MuddleMessage is null && Side == side);
        Require(Inaction is null || action.Kind == BattleActionKind.Physical && Silenced is null);
        Require(CasterCostStatus == (action.Kind == BattleActionKind.SupportSpell && SupportSpell is not null
            ? before.GetActor(actor).Status : (ushort?)null));
        byte? wornItem = admission.AttackItem is { Breakable: true } gear ? gear.ItemId : admission.SupportItem?.ItemId;
        Require(EquipmentUse?.ItemId == wornItem && (EquipmentUse is null || action.Kind == BattleActionKind.Item));
        Require(AttackItem == admission.AttackItem && (AttackItem is null || action.Kind == BattleActionKind.Item));
        if (Silenced is { } blocked)
        {
            Require(SilenceRules.Present(before.GetActor(actor).Status) && blocked.Action == action &&
                SilenceRules.Cost(action, admission) == blocked.MpCost && Reactions.Count == 0 && Reward is null &&
                Spell is null && AttackSpell is null && SupportSpell is null && Item is null && Retreat is null && Laser is null &&
                ConstructionEffects.Count == 0 && CompletionEffects.Count == 0 && Prepared.MainSeed == before.MainSeed);
            ValidateState(before, actor, destination); return;
        }
        Require((action.Kind == BattleActionKind.SupportSpell || admission.SupportItem is not null) == (SupportSpell is not null));
        Require((action.Kind == BattleActionKind.PrismLaser) == (Laser is not null));
        Require(action.Kind is BattleActionKind.Retreat or BattleActionKind.Item || Retreat is null);
        var conversion = action.Kind == BattleActionKind.Physical && target is { } physicalTarget
            ? Sf2PhysicalAction.MonsterBreathRoll(before, actor, physicalTarget) : null;
        if (conversion is not null) Require(ConstructionEffects.FirstOrDefault() == conversion);
        Require(ConstructionEffects.Count(e => e.Kind == "rng-kiwi-conversion") == (conversion is null ? 0 : 1));
        bool converts = conversion?.RandomValue == 0;
        Require(!converts || Inaction is null);
        bool usesDamageEffect = action.Kind == BattleActionKind.AttackSpell || AttackItem is not null || converts;
        Require(usesDamageEffect || action.Kind == BattleActionKind.Physical || Reactions.All(reaction => reaction.Receipt is null));
        Require(usesDamageEffect == (AttackSpell is not null));
        switch (action.Kind)
        {
            case BattleActionKind.PrismLaser when action == new BattleActionRef(BattleActionKind.PrismLaser):
                ValidateState(before, actor, destination);
                PrismLaser.Validate(this, before, actor, destination, target);
                break;
            case BattleActionKind.Retreat when action.Spell is { } retreatSpell && action.ItemSlot is null && target == actor:
                ValidateState(before, actor, destination);
                Require(admission.Retreat is { } retreat && Retreat == new SpellRetreatPayment(retreat.Spell, retreat.MpCost) && retreat.Spell == retreatSpell &&
                    before.Definition.RetreatSpells.TryGetValue(retreatSpell, out var known) && known == retreat &&
                    before.GetActor(actor).Mp >= retreat.MpCost && Spell is null && Item is null &&
                    Reactions.Count == 0 && Reward is null && ConstructionEffects.Count == 0 &&
                    CompletionEffects.Count == 0 && Prepared.MainSeed == before.MainSeed);
                break;
            case BattleActionKind.Healing when action.Spell is { } spell && target is { } patient &&
                admission.Spell is { } definition && spell == definition.Spell && action.ItemSlot is null:
                ValidateHealing(before, actor, destination, definition, patient);
                break;
            case BattleActionKind.AttackSpell when action.Spell is { } attackKey && target is { } center &&
                admission.AttackSpell is { } attack && attackKey == attack.Spell && action.ItemSlot == AttackItemSlot &&
                (AttackItemSlot is null || TarosEffectiveness.IsSwordUse(before.GetActor(actor), action)):
                ValidateAttackSpell(before, actor, destination, attack, center);
                break;
            case BattleActionKind.SupportSpell when action.Spell is { } supportKey && target is { } supportCenter &&
                admission.SupportSpell is { } support && supportKey == support.Spell && action.ItemSlot is null:
                ValidateSupport(before, actor, destination, support, supportCenter);
                break;
            case BattleActionKind.Physical when action.Spell is null && action.ItemSlot is null && target is { } victim:
                if (converts)
                {
                    var key = Sf2PhysicalAction.MonsterBreathSpell(before.GetActor(actor).Level);
                    Require(before.Definition.AttackSpells.TryGetValue(key, out var breath) &&
                        AttackSpell == breath && AttackItemSlot is null);
                    ValidateAttackSpell(before, actor, destination, breath!, victim, firstDraw: 1);
                }
                else ValidatePhysical(before, actor, destination, victim, conversion is null ? 0 : 1);
                break;
            case BattleActionKind.Item when action.Spell is null && action.ItemSlot is { } attackSlot &&
                target is { } attackTarget && AttackItem is { } attackItem:
                var words = before.GetActor(actor).SourceLoadout!.Items;
                Require(attackSlot == AttackItemSlot && (!attackItem.Breakable || EquipmentUse?.Slot == attackSlot) &&
                    attackSlot >= 0 && attackSlot < words.Count &&
                    (words[attackSlot] & 127) == attackItem.ItemId &&
                    before.Definition.AttackItems.TryGetValue(attackItem.ItemId, out var knownItem) && knownItem == attackItem);
                ValidateAttackSpell(before, actor, destination, attackItem.Effect, attackTarget);
                break;
            case BattleActionKind.Item when action.Spell is null && action.ItemSlot is { } supportSlot &&
                target is { } itemCenter && admission.SupportItem is { } supportItem:
                Require(EquipmentUse?.Slot == supportSlot &&
                    before.Definition.SupportItems.GetValueOrDefault(supportItem.ItemId) == supportItem);
                ValidateSupport(before, actor, destination, supportItem.Effect, itemCenter);
                break;
            case BattleActionKind.Item when action.Spell is null && action.ItemSlot is { } retreatSlot &&
                target == actor && admission.RetreatItem is { } wing:
                ValidateRetreatItem(before, actor, destination, retreatSlot, wing);
                break;
            case BattleActionKind.Item when action.Spell is null && action.ItemSlot is { } slot &&
                target is { } recipient && admission.Item is { } item:
                ValidateItem(before, actor, destination, slot, item, recipient);
                break;
            case BattleActionKind.Stay when action == new BattleActionRef(BattleActionKind.Stay) && target is null:
                ValidateState(before, actor, destination);
                Require(Spell is null && Item is null && Reactions.Count == 0 && Reward is null &&
                    ConstructionEffects.Count == 0 && CompletionEffects.Count == 0 && Prepared.MainSeed == before.MainSeed);
                break;
            default: throw new InvalidOperationException("Invalid action reference or admission.");
        }
    }

    private static void Require(bool valid)
    { if (!valid) throw new InvalidOperationException("Invalid prepared action transition."); }

    private void ValidateRetreatItem(EngineBattleState before, ActorRef actorRef, MapPosition destination,
        int slot, RetreatItemDefinition item)
    {
        var actor = before.GetActor(actorRef);
        var loadout = actor.SourceLoadout!;
        Require(slot >= 0 && slot < loadout.Items.Count && (loadout.Items[slot] & 127) == item.ItemId &&
            before.Definition.RetreatItems.TryGetValue(item.ItemId, out var known) && known == item &&
            Retreat == new ItemRetreatPayment(item.ItemId, item.Name, slot, loadout.Items[slot]) &&
            Spell is null && Item is null && Reactions.Count == 0 && Reward is null &&
            CompletionEffects.Count == 0 && Prepared.MainSeed == before.MainSeed &&
            ConstructionEffects.SequenceEqual([new BattleEffect("item-consumed", actorRef, loadout.Items[slot], slot)]));
        var items = loadout.Items.ToList(); items.RemoveAt(slot); items.Add(127);
        ValidateState(before, actorRef, destination, new(items, loadout.Spells));
    }

    private void ValidateState(EngineBattleState before, ActorRef actor, MapPosition destination,
        BattleSourceLoadout? consumedLoadout = null, bool allowGold = false, EngineBattleState? inventory = null, IReadOnlyDictionary<ActorRef, ushort>? statusWrites = null)
    {
        // A stationary laser has no movement-range/occupancy-grid operation. Its
        // own ray admission also permits departed/off-crop targets.
        if (Laser is null) BattleMovement.RequireStop(before, actor, destination);
        else Require(destination == before.GetActor(actor).Position);
        Require(Actor == actor && Destination == destination &&
            Equals(Prepared.DeathCredit, before.DeathCredit) && ReferenceEquals(Prepared.Definition, before.Definition) && Prepared.ThinkingSeed == before.ThinkingSeed &&
            Prepared.Round == before.Round && Prepared.Cursor == before.Cursor &&
            (allowGold || Prepared.Gold == before.Gold) && ReferenceEquals(Prepared.StartPolicy, before.StartPolicy) &&
            ReferenceEquals(Prepared.Regions, before.Regions) && ReferenceEquals(Prepared.Visuals, before.Visuals) && Prepared.Queue.SequenceEqual(before.Queue) &&
            Prepared.Actors.Count == before.Actors.Count);
        var expectedCustody = (inventory ?? before).ItemCustody;
        Require(ReferenceEquals(Prepared.ItemCustody, expectedCustody) || Prepared.ItemCustody is { } actual &&
            expectedCustody is { } expected && actual.Depot.SequenceEqual(expected.Depot) && actual.OnceFlags.SequenceEqual(expected.OnceFlags) &&
            actual.Deals.OrderBy(pair => pair.Key).SequenceEqual(expected.Deals.OrderBy(pair => pair.Key)));
        for (int index = 0; index < before.Actors.Count; index++)
        {
            var old = before.Actors[index]; var candidate = Prepared.Actors[index];
            var afterDrops = inventory?.GetActor(old.Actor) ?? old;
            if (statusWrites is not null && statusWrites.TryGetValue(old.Actor, out ushort word)) afterDrops = afterDrops.With(status: word);
            if (old.Actor == actor && consumedLoadout is not null)
                afterDrops = afterDrops.With(sourceLoadout: consumedLoadout, refreshStats: true);
            var expectedLoadout = afterDrops.SourceLoadout;
            bool changedLoadout = !ReferenceEquals(expectedLoadout, old.SourceLoadout);
            Require(ReferenceEquals(candidate.Deployment, old.Deployment) && candidate.Hp == old.Hp && candidate.Initialized == old.Initialized &&
                candidate.Mp == old.Mp && candidate.Exp == old.Exp && candidate.Kills == old.Kills && candidate.Defeats == old.Defeats &&
                candidate.LastTarget == old.LastTarget && candidate.Attack == afterDrops.Attack && candidate.Status == afterDrops.Status &&
                candidate.UsesInitialStats == afterDrops.UsesInitialStats &&
                candidate.LastRefreshBits == afterDrops.LastRefreshBits && candidate.Facing == old.Facing &&
                candidate.ActivationWord == old.ActivationWord && candidate.AiMemory == old.AiMemory &&
                Equals(changedLoadout && candidate.Progress is { } progress
                    ? progress with { SourceLoadout = afterDrops.Progress?.SourceLoadout } : candidate.Progress,
                    afterDrops.Progress) &&
                candidate.Position == (old.Actor == actor ? destination : old.Position));
            if (changedLoadout)
                Require(candidate.SourceLoadout is { } loadout && loadout.Items.SequenceEqual(expectedLoadout!.Items) &&
                    loadout.Spells.SequenceEqual(expectedLoadout.Spells) &&
                    (candidate.Progress is null || ReferenceEquals(candidate.Progress.SourceLoadout, loadout)));
            else Require(ReferenceEquals(candidate.SourceLoadout, old.SourceLoadout));
        }
    }

    private void ValidateRandom(EngineBattleState before, Func<BattleEffect, bool> allowed)
    {
        uint seed = before.MainSeed;
        foreach (var effect in ConstructionEffects)
        {
            Require(allowed(effect));
            if (!effect.Kind.StartsWith("rng-", StringComparison.Ordinal)) continue;
            Require(effect.Kind.Length > 4 && effect.RandomRange is not null);
            var draw = BattleRandom.NextMain(seed, effect.RandomRange!.Value);
            Require(effect.Before == seed && effect.After == draw.After && effect.RandomValue == draw.Value);
            seed = draw.After;
        }
        Require(Prepared.MainSeed == seed);
    }

    private void ValidateReward(EngineBattleState before, ActorRef? eligible)
    {
        Require(Reward is null || eligible is { } actor && Reward.Actor == actor && Reward.Amount > 0);
        if (Reward is not { } reward) return;
        var recipient = before.GetActor(reward.Actor);
        if (recipient.Exp is null) throw new BattleRuleException("unspecified-exp", "actor.exp", true);
    }

    private void ValidatePhysical(EngineBattleState before, ActorRef actorRef, MapPosition destination, ActorRef targetRef,
        int cursor)
    {
        var actor = before.GetActor(actorRef); var target = before.GetActor(targetRef);
        bool self = actorRef == targetRef;
        Require(BattleOffense.Matches(actor, target, Side) && actor.Hp > 0 && target.Hp > 0 &&
            target.Position is not null && Spell is null && Item is null && CompletionEffects.Count == 0);
        int openingStart = cursor;
        bool curseStops = CurseRules.Present(actor.Status) && OpeningDraw("rng-curse-inaction", CurseRules.InactionRange) == 0;
        bool stunStops = CurseRules.Present(actor.Status) && (actor.Status & PoisonCureRules.Stun) != 0 &&
            OpeningDraw("rng-stun-inaction", 4) == 0;
        Require(ConstructionEffects.Count(effect => effect.Kind is "rng-curse-inaction" or "rng-stun-inaction") == cursor - openingStart);
        if (curseStops || stunStops)
        {
            Require(Inaction == new PhysicalInaction(targetRef, DueToCurse: curseStops) && Reactions.Count == 0 &&
                Reward is null && ConstructionEffects.Count == cursor);
            ValidateState(before, actorRef, destination);
            ValidateRandom(before, _ => true);
            return;
        }

        Require(Inaction is null && Reactions.Count is > 0 and <= 6);
        ushort actorHp = actor.Hp, targetHp = target.Hp;
        bool ineffective = before.Definition.TarosEffectiveness?.PrepareInitial(actor, target) ?? false;
        bool counter = false;
        int strikes = 0, dropDraws = 0, recoilDraws = 0;
        List<BattleEffect> facts = [];
        var inventory = before;
        for (int index = 0; index < Reactions.Count; index++)
        {
            var reaction = Reactions[index];
            bool reversed = reaction.Action == "physical-counter";
            Require(!counter && (reversed
                ? Side == OffensiveTargetSide.Opponents && strikes > 0 && reaction.Actor == targetRef && reaction.Target == actorRef
                : strikes < (Side == OffensiveTargetSide.OwnSide ? 1 : 2) &&
                    reaction.Actor == actorRef && reaction.Target == targetRef));
            counter = reversed;
            var attacker = reversed ? target : actor; var defender = reversed ? actor : target;
            var profile = BattleEquipment.Physical(attacker) ?? throw new BattleRuleException("physical-definition", "actor.physical", true);
            bool inRange = reversed ? BattleRange.Contains(target.Position!, destination, profile.MinimumRange, profile.MaximumRange) :
                Sf2PhysicalAction.InRange(actor, target, destination, profile);
            ushort hp = reversed ? actorHp : targetHp;
            ushort status = defender.Status;
            Require(actorHp > 0 && targetHp > 0 && inRange &&
                reaction.Action == (reversed ? "physical-counter" : strikes == 0 ? "physical-first" : "physical-second") &&
                reaction.Kind is BattleReactionKind.Damage or BattleReactionKind.Dodge or BattleReactionKind.Ineffective && reaction.HpBefore == hp &&
                reaction.Amount >= 0 && reaction.HpAfter == Math.Max(0, hp - reaction.Amount) &&
                reaction.MpBefore == defender.Mp && reaction.MpAfter == defender.Mp &&
                (reaction.Kind == BattleReactionKind.Damage || reaction.Amount == 0 && !reaction.Critical));
            Require(reaction.Kind != BattleReactionKind.Ineffective || !reversed && ineffective);
            Require(reversed || !ineffective || reaction.Kind != BattleReactionKind.Damage);
            Require(!reversed || (target.Status & 0xC1) == 0 &&
                !(before.Definition.TarosEffectiveness?.IsTaros(actor) ?? false));
            Require(reaction.StatusWrite is null || reaction.Kind == BattleReactionKind.Damage && reaction.StatusWrite == status);
            Require(reaction.InflictedSleep is null || profile.Critical.InflictsSleep && reaction.Kind is BattleReactionKind.Damage or BattleReactionKind.Ineffective &&
                reaction.HpAfter > 0 && !reaction.Critical &&
                (reaction.Kind == BattleReactionKind.Ineffective ? reaction.StatusWrite is null : reaction.StatusWrite == status) &&
                reaction.InflictedSleep == (ushort)(status | 0xC0));
            Require(reaction.InflictedStun is null || profile.Critical.InflictsStun &&
                reaction.Kind is BattleReactionKind.Damage or BattleReactionKind.Ineffective && reaction.HpAfter > 0 &&
                !reaction.Critical && reaction.InflictedStun == (ushort)(status | 1));
            Require(reaction.InflictedPoison is null || before.Definition.Poison && profile.Critical.InflictsPoison &&
                reaction.Kind is BattleReactionKind.Damage or BattleReactionKind.Ineffective && reaction.HpAfter > 0 &&
                !reaction.Critical && reaction.InflictedPoison == (ushort)(status | 2));
            Require(reaction.InflictedSilence is null || profile.Critical.InflictsSilence &&
                reaction.Kind is BattleReactionKind.Damage or BattleReactionKind.Ineffective && reaction.HpAfter > 0 &&
                !reaction.Critical && SilenceRules.HasSpells(defender) && reaction.InflictedSilence == (ushort)(status | SilenceRules.Mask));
            strikes++;
            ushort attackerHp = self ? reaction.HpAfter : reversed ? targetHp : actorHp;
            facts.Add(new(reaction.Action, reaction.Actor, Target: reaction.Target));
            if (reaction.Kind == BattleReactionKind.Dodge)
                facts.Add(new("dodge", reaction.Target, reaction.HpBefore, reaction.HpAfter));
            else if (reaction.Kind == BattleReactionKind.Ineffective)
                facts.Add(new("ineffective", reaction.Target, reaction.HpBefore, reaction.HpAfter));
            else
            {
                if (reaction.Critical) facts.Add(new("critical", reaction.Actor, Target: reaction.Target));
                facts.Add(new("hp", reaction.Target, reaction.HpBefore, reaction.HpAfter));
            }
            int impactStart = cursor;
            while (cursor < ConstructionEffects.Count && ConstructionEffects[cursor] != facts[^1]) cursor++;
            Require(cursor < ConstructionEffects.Count);
            var impact = ConstructionEffects.Skip(impactStart).Take(cursor++ - impactStart).ToArray();
            var recoilRolls = impact.Where(effect => effect.Kind == "rng-curse-recoil").ToArray();
            bool canRecoil = CurseRules.CanRecoil(attacker, reaction.Kind == BattleReactionKind.Dodge, reaction.HpAfter);
            Require(recoilRolls.Length == (canRecoil ? 1 : 0));
            if (canRecoil)
            {
                var roll = recoilRolls[0];
                recoilDraws++;
                Require(roll.Actor == attacker.Actor && roll.Target == defender.Actor &&
                    roll.RandomRange == CurseRules.RecoilRange &&
                    !impact.TakeWhile(effect => effect != roll).Any(IsFollowUpDraw));
                if (roll.RandomValue != 0)
                {
                    Require(++index < Reactions.Count);
                    var recoil = Reactions[index];
                    Require(recoil.Amount > 0 && (reaction.Kind != BattleReactionKind.Damage ||
                        recoil.Amount == CurseRules.RecoilDamage(reaction.Amount)));
                    ushort after = (ushort)Math.Max(0, attackerHp - recoil.Amount);
                    Require(recoil == new BattleReaction(attacker.Actor, attacker.Actor, "curse-recoil",
                        BattleReactionKind.CurseRecoil, attackerHp, after, attacker.Mp, attacker.Mp, Amount: recoil.Amount));
                    Require(after > 0 || !impact.Any(IsFollowUpDraw));
                    attackerHp = after;
                    var recoilFact = new BattleEffect(recoil.Action, recoil.Actor, recoil.HpBefore, recoil.HpAfter, Target: recoil.Target);
                    Require(cursor < ConstructionEffects.Count && ConstructionEffects[cursor++] == recoilFact);
                    facts.Add(recoilFact);
                }
            }
            (actorHp, targetHp) = self ? (attackerHp, attackerHp) :
                reversed ? (reaction.HpAfter, attackerHp) : (attackerHp, reaction.HpAfter);

            // Loot belongs to the opposing strike, after recoil and before the next strike.
            var lastDraw = ConstructionEffects.Take(cursor).LastOrDefault(effect =>
                effect.Kind.StartsWith("rng-", StringComparison.Ordinal));
            uint seed = checked((uint)(lastDraw?.After ?? before.MainSeed));
            var dropped = EnemyItemDropRules.Prepare(inventory.With(mainSeed: seed), reaction.Actor,
                reaction.Target, reaction.HpAfter == 0);
            Require(reaction.Receipt == dropped.Receipt && ConstructionEffects.Skip(cursor).Take(dropped.Effects.Count).SequenceEqual(dropped.Effects));
            cursor += dropped.Effects.Count;
            inventory = dropped.Battle;
            facts.AddRange(dropped.Effects.Where(effect => !effect.Kind.StartsWith("rng-", StringComparison.Ordinal)));
            dropDraws += dropped.Effects.Count(effect => effect.Kind == "rng-item-drop");
        }
        Require(ConstructionEffects.Count(effect => effect.Kind == "rng-item-drop") == dropDraws &&
            ConstructionEffects.Count(effect => effect.Kind == "rng-curse-recoil") == recoilDraws);
        foreach (var reaction in Reactions) BattleOffense.RequireDeathAccounting(before, reaction);
        var goldKill = Reactions.SingleOrDefault(r => BattleOffense.PaysGold(before, r));
        if (goldKill is not null && before.Gold is null)
            throw new BattleRuleException("unspecified-gold", "battle.gold", true);
        Require(goldKill is not null ? Prepared.Gold is not null : Prepared.Gold == before.Gold);
        var recipient = actor.IsAlly ? actor : target.IsAlly && counter ? target : null;
        ValidateReward(before, recipient is not null && BattleOffense.Survives(recipient, Reactions) ? recipient.Actor : null);
        ValidateState(before, actorRef, destination, allowGold: goldKill is not null, inventory: inventory);
        if (goldKill is not null) facts.Add(new("gold", goldKill.Actor, before.Gold, Prepared.Gold));
        Require(ConstructionEffects.Where(effect => !effect.Kind.StartsWith("rng-", StringComparison.Ordinal)).SequenceEqual(facts));
        ValidateRandom(before, effect => !effect.Kind.StartsWith("rng-", StringComparison.Ordinal) ||
            (effect.Actor == actorRef || effect.Actor == targetRef) &&
            (effect.Target is null || (self || effect.Target != effect.Actor) &&
                (effect.Target == actorRef || effect.Target == targetRef)));

        ushort OpeningDraw(string kind, ushort range)
        {
            Require(cursor < ConstructionEffects.Count);
            var draw = ConstructionEffects[cursor++];
            Require(draw.Kind == kind && draw.RandomRange == range && draw.Actor == actorRef && draw.Target == targetRef);
            return draw.RandomValue ?? throw new InvalidOperationException("Missing opening draw value.");
        }

        static bool IsFollowUpDraw(BattleEffect effect) => effect.Kind is "rng-double" or "rng-counter";
    }

    private void ValidateItem(EngineBattleState before, ActorRef actorRef, MapPosition destination,
        int slot, HealingItemDefinition item, ActorRef targetRef)
    {
        var actor = before.GetActor(actorRef);
        var loadout = actor.SourceLoadout ?? throw new BattleRuleException("item-slot", "item.slot");
        Require(slot >= 0 && slot < loadout.Items.Count && (loadout.Items[slot] & 127) != 127 &&
            before.Definition.HealingItems.TryGetValue((byte)(loadout.Items[slot] & 127), out var admitted) && item == admitted &&
            Item == item && Spell is null && Retreat is null && CompletionEffects.Count == 0);
        var items = loadout.Items.ToList(); items.RemoveAt(slot); items.Add(127);
        ValidateState(before, actorRef, destination, new(items, loadout.Spells));
        var targets = HealingTargetRules.ResolveItem(before, actorRef, destination, item, targetRef);
        Require(Reactions.SequenceEqual(targets.Select(target => Sf2ItemAction.Reaction(actorRef, item, target))));
        ValidateReward(before, actorRef);
        ValidateRandom(before, effect => effect.Kind == "item-consumed"
            ? effect == new BattleEffect("item-consumed", actorRef, loadout.Items[slot], slot)
            : effect.Kind.StartsWith("rng-", StringComparison.Ordinal) && effect.Actor == actorRef && effect.Target is null);
        Require(ConstructionEffects.Count(effect => effect.Kind == "item-consumed") == 1);
    }

    // The finite healing authority boundary: construction may move only the selected
    // actor and advance main RNG. HP, MP and progress are later scene publications.
    internal void ValidateHealing(EngineBattleState before, ActorRef actorRef, MapPosition destination,
        HealingSpellDefinition spell, ActorRef targetRef)
    {
        void Require(bool valid) { if (!valid) throw new InvalidOperationException("Invalid prepared HEAL transition."); }
        BattleMovement.RequireStop(before, actorRef, destination);
        var actor = before.GetActor(actorRef);
        var targets = HealingTargetRules.Resolve(before, actorRef, destination, spell, targetRef);
        Require(Actor == actorRef && Destination == destination && Spell == spell && Item is null);
        Require(actor.IsAlly || Reward is null);
        ValidateState(before, actorRef, destination);
        Require(Reactions.Count == targets.Count && Reactions.Count > 0 && CompletionEffects.Count == 0);
        for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i]; var reaction = Reactions[i];
            Require(spell.CuresPoison ? reaction == PoisonCureRules.Reaction(actorRef, target, "heal", PoisonCureRules.SpellMask(spell.Spell.Level)) :
                reaction.Actor == actorRef && reaction.Target == target.Actor && reaction.Action == "heal" &&
                reaction.Kind == BattleReactionKind.Recovery && !reaction.Critical && reaction.HpBefore == target.Hp &&
                reaction.HpAfter >= target.Hp && reaction.HpAfter <= target.MaxHp &&
                reaction.Amount == reaction.HpAfter - target.Hp && reaction.MpBefore == target.Mp && reaction.MpAfter == target.Mp &&
                reaction.StatusWrite is null && reaction.ClearedStatus == 0 && reaction.InflictedSleep is null && reaction.InflictedSilence is null && reaction.InflictedPoison is null && reaction.InflictedStun is null);
        }
        // The source enemy HEAL level lookup can approve a cheaper packed key.
        // Its scripted MP reaction then clamps the actual selected cost at zero.
        Require(Sf2HealingRule.CanPay(actor, spell));
        Require(Reward is null || Reward.Actor == actorRef && Reward.Amount > 0);
        uint seed = before.MainSeed;
        // The rule owns draw count, purpose and range. This boundary checks that
        // construction carries only ordered contribution facts and the actual random chain.
        int contributionIndex = 0, accumulated = 0;
        int contributionCap = spell.CuresPoison ? 49 : 25;
        foreach (var effect in ConstructionEffects)
        {
            if (effect.Kind == "healing-exp-contribution")
            {
                Require(contributionIndex < Reactions.Count && effect.Actor == actorRef &&
                    effect.Target == Reactions[contributionIndex].Target && effect.Before == accumulated &&
                    effect.After >= accumulated && effect.After <= contributionCap && effect.RandomRange is null && effect.RandomValue is null);
                accumulated = checked((int)effect.After!.Value); contributionIndex++; continue;
            }
            Require(effect.Kind.StartsWith("rng-", StringComparison.Ordinal) && effect.Kind.Length > 4 &&
                effect.Actor == actorRef && effect.Target is null && effect.RandomRange is not null);
            var draw = BattleRandom.NextMain(seed, effect.RandomRange!.Value);
            Require(effect.Before == seed && effect.After == draw.After &&
                effect.RandomValue == draw.Value);
            seed = draw.After;
        }
        Require(contributionIndex == 0 || contributionIndex == Reactions.Count);
        Require(Prepared.MainSeed == seed);
    }

    private void ValidateAttackSpell(EngineBattleState before, ActorRef actor, MapPosition destination,
        AttackSpellDefinition spell, ActorRef center, int firstDraw = 0)
    {
        var targets = Sf2AttackSpellAction.ResolveTargets(before, actor, destination, spell, center, Side);
        var caster = before.GetActor(actor);
        bool earnsExp = caster.IsAlly && BattleOffense.Survives(caster, Reactions);
        Require(Spell is null && Item is null && AttackSpell == spell && CompletionEffects.Count == 0 &&
            Reactions.Count == targets.Count && Reactions.Count > 0 &&
            (earnsExp ? Reward is { Actor: var recipient } && recipient == actor : Reward is null));
        var inventory = before;
        int cursor = firstDraw;
        var taros = before.Definition.TarosEffectiveness;
        bool ineffective = !spell.IsInstantDeath && taros?.IsTaros(targets[0]) == true;
        for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i]; var reaction = Reactions[i];
            Require(reaction.Actor == actor && reaction.Target == target.Actor &&
                reaction.Action == (AttackItem is null ? "attack-spell" : "attack-item") &&
                reaction.HpBefore == target.Hp && reaction.MpBefore == target.Mp && reaction.MpAfter == target.Mp &&
                reaction.StatusWrite == target.Status && reaction.InflictedSleep is null && reaction.InflictedSilence is null &&
                reaction.InflictedPoison is null && reaction.InflictedStun is null);
            bool blocked = ineffective && taros!.IsTaros(target);
            int drawCount = spell.IsInstantDeath || blocked ? 1 : 3;
            var draws = ConstructionEffects.Skip(cursor).Take(drawCount).ToArray();
            Require(draws.Length == drawCount && draws.All(e => e.Actor == actor && e.Target == target.Actor));
            if (spell.IsInstantDeath)
            {
                var draw = draws[0];
                Require(draw.Kind == "rng-desoul" && draw.RandomRange == 8 && draw.RandomValue.HasValue);
                bool hit = AttackSpellDefinition.InstantDeathSucceeds(target, draw.RandomValue!.Value);
                Require(reaction.Kind == (hit ? BattleReactionKind.InstantDeath : BattleReactionKind.Ineffective) &&
                    reaction.HpAfter == (hit ? 0 : target.Hp) && reaction.Amount == 0 && !reaction.Critical);
            }
            else
            {
                Require(reaction.Kind == (blocked ? BattleReactionKind.Ineffective : BattleReactionKind.Damage) &&
                    (blocked ? reaction.Amount == 0 && !reaction.Critical : reaction.Amount >= 1) &&
                    reaction.HpAfter == Math.Max(0, target.Hp - reaction.Amount) &&
                    draws[0].Kind == "rng-spell-critical" && draws[0].RandomRange == spell.CriticalRange &&
                    draws.Skip(1).All(e => e.Kind == "rng-spell-variance"));
            }
            BattleOffense.RequireDeathAccounting(before, reaction);
            cursor += drawCount;
            var dropped = EnemyItemDropRules.Prepare(inventory.With(mainSeed: checked((uint)draws[^1].After!.Value)),
                actor, target.Actor, reaction.HpAfter == 0);
            Require(reaction.Receipt == dropped.Receipt && ConstructionEffects.Skip(cursor).Take(dropped.Effects.Count).SequenceEqual(dropped.Effects));
            cursor += dropped.Effects.Count; inventory = dropped.Battle;
        }
        if (EquipmentUse is not null)
        {
            inventory = ValidateWear(inventory, inventory.GetActor(actor), inventory.MainSeed, ref cursor);
        }
        else if (AttackItem is not null)
        {
            int slot = AttackItemSlot!.Value;
            var holder = inventory.GetActor(actor);
            Require(cursor < ConstructionEffects.Count &&
                ConstructionEffects[cursor] == new BattleEffect("item-consumed", actor, holder.SourceLoadout!.Items[slot], slot));
            cursor++;
            var paid = BattleEquipment.RemoveItem(holder, slot);
            inventory = inventory.With(actors: inventory.Actors.Select(unit => unit.Actor == actor ? paid : unit));
        }
        Require(ConstructionEffects.Skip(cursor).All(e => e.Actor == actor &&
            (e.Kind is "rng-exp-plus" or "rng-exp-minus" or "spell-exp-contribution" or "gold")));
        bool paysGold = Reactions.Any(r => BattleOffense.PaysGold(before, r));
        Require(paysGold || Prepared.Gold == before.Gold);
        ValidateState(before, actor, destination, allowGold: paysGold, inventory: inventory);
        ValidateReward(before, earnsExp ? actor : null);
        ValidateRandom(before, _ => true);
    }

    private void ValidateSupport(EngineBattleState before, ActorRef actor, MapPosition destination,
        SupportSpellDefinition spell, ActorRef center)
    {
        var targets = Sf2SupportSpellAction.Targets(before, actor, destination, spell, center);
        Require(SupportSpell == spell && Spell is null && AttackSpell is null && Item is null &&
            CompletionEffects.Count == 0 && Reactions.Count == (spell.AbsorbsMp ? 2 : targets.Count) && Reactions.Count > 0 &&
            (before.GetActor(actor).IsAlly ? Reward is { Actor: var recipient } && recipient == actor : Reward is null));
        int cursor = 0;
        Dictionary<ActorRef, ushort> writes = [];
        if (spell.AbsorbsMp)
        {
            Require(EquipmentUse is not null && targets.Count == 1);
            var target = targets[0];
            int roll = TakeRoll(target.Actor, "rng-mp-absorb", 3);
            Require(Reactions.SequenceEqual(SupportSpellDefinition.MpTransfer(before.GetActor(actor), target, roll)));
        }
        else for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i]; var reaction = Reactions[i];
            var (reactionWord, constructionWord) = spell.Resolve(target, kind => TakeRoll(target.Actor, kind, 8));
            if (constructionWord.HasValue) writes.Add(target.Actor, constructionWord.Value);
            Require(reaction.Actor == actor && reaction.Target == target.Actor && reaction.Action == (EquipmentUse is null ? "support-spell" : "item-use") &&
                reaction.Kind == (reactionWord.HasValue ? BattleReactionKind.Status : BattleReactionKind.Ineffective) &&
                reaction.HpBefore == target.Hp && reaction.HpAfter == target.Hp && reaction.MpBefore == target.Mp &&
                reaction.MpAfter == target.Mp && !reaction.Critical && reaction.Amount == 0 &&
                reaction.StatusWrite == reactionWord &&
                reaction.InflictedSleep is null && reaction.InflictedSilence is null && reaction.InflictedPoison is null && reaction.InflictedStun is null);
        }
        EngineBattleState? inventory = null;
        if (EquipmentUse is not null)
        {
            var owner = before.GetActor(actor);
            if (writes.TryGetValue(actor, out ushort word)) owner = owner.With(status: word);
            uint seed = cursor == 0 ? before.MainSeed : (uint)ConstructionEffects[cursor - 1].After!.Value;
            inventory = ValidateWear(before, owner, seed, ref cursor);
        }
        Require(ConstructionEffects.Skip(cursor).All(e => e.Actor == actor && e.Target is null &&
            e.Kind is "rng-exp-plus" or "rng-exp-minus"));
        ValidateState(before, actor, destination, inventory: inventory, statusWrites: writes);
        ValidateReward(before, actor);
        ValidateRandom(before, _ => true);

        int TakeRoll(ActorRef target, string kind, int range)
        {
            Require(cursor < ConstructionEffects.Count);
            var draw = ConstructionEffects[cursor++];
            Require(draw.Kind == kind && draw.Actor == actor && draw.Target == target &&
                draw.RandomRange == range && draw.RandomValue.HasValue);
            return draw.RandomValue!.Value;
        }
    }

    private EngineBattleState ValidateWear(EngineBattleState inventory, BattleActorState owner, uint seed, ref int cursor)
    {
        var use = EquipmentUse!;
        Require(use.Slot >= 0 && use.Slot < owner.SourceLoadout!.Items.Count && owner.SourceLoadout.Items[use.Slot] == use.Word);
        List<BattleEffect> expected = [];
        var wear = EquipmentItemUse.Apply(use.Slot, owner, seed, expected);
        Require(wear.Use == use && ConstructionEffects.Skip(cursor).Take(expected.Count).SequenceEqual(expected));
        cursor += expected.Count;
        return inventory.With(actors: inventory.Actors.Select(actor => actor.Actor == owner.Actor ? wear.Owner : actor));
    }

    internal EngineBattleState ApplyReaction(EngineBattleState current, BattleReaction reaction)
    {
        if (reaction.Kind is BattleReactionKind.Dodge or BattleReactionKind.Ineffective) return current;
        var target = current.GetActor(reaction.Target);
        var changed = reaction.Kind == BattleReactionKind.Resource ? target.With(mp: reaction.MpAfter) : ApplyTargetReaction(target, reaction);
        return current.With(actors: current.Actors.Select(actor => actor.Actor == reaction.Target ? changed : actor));
    }

    private BattleActorState ApplyTargetReaction(BattleActorState target, BattleReaction reaction)
    {
        var pack = Spell?.CuresPoison == true && CurseRules.Present(reaction.ClearedStatus) && target.SourceLoadout is { } worn
            ? BattleEquipment.RemoveCurse(target.Definition, worn) : null;
        byte? attack = pack is null ? null : BattleEquipment.Attack(target.Definition, target.Progress,
            pack, reaction.StatusWrite ?? target.Status);
        // The caster cost was already applied, including for a self-targeted cure.
        byte mp = Spell is null && AttackSpell is null && SupportSpell is null ? reaction.MpAfter : target.Mp;
        return target.With(hp: reaction.HpAfter, mp: mp, attack: attack,
            status: reaction.StatusWrite, sourceLoadout: pack, refreshStats: true);
    }

    internal EngineBattleState ApplySleep(EngineBattleState current, BattleReaction reaction) =>
        reaction.InflictedSleep is not { } status ? current : current.With(actors: current.Actors.Select(actor =>
            actor.Actor == reaction.Target ? actor.With(status: status, refreshStats: true) : actor));

    internal EngineBattleState ApplyStun(EngineBattleState current, BattleReaction reaction) =>
        reaction.InflictedStun is not { } status ? current : current.With(actors: current.Actors.Select(actor =>
            actor.Actor == reaction.Target ? actor.With(status: status, refreshStats: true) : actor));

    internal EngineBattleState ApplyPoison(EngineBattleState current, BattleReaction reaction) =>
        reaction.InflictedPoison is not { } status ? current : current.With(actors: current.Actors.Select(actor =>
            actor.Actor == reaction.Target ? actor.With(status: status, refreshStats: true) : actor));

    internal EngineBattleState ApplySilence(EngineBattleState current, BattleReaction reaction) =>
        reaction.InflictedSilence is not { } status ? current : current.With(actors: current.Actors.Select(actor =>
            actor.Actor == reaction.Target ? actor.With(status: status, refreshStats: true) : actor));

    internal EngineBattleState ApplySpellCost(EngineBattleState current) =>
        AttackItemSlot.HasValue || EquipmentUse is not null ? current : current.With(actors: current.Actors.Select(actor => actor.Actor == Actor
            ? actor.With(mp: (byte)Math.Max(0, actor.Mp - (Silenced?.MpCost ?? Spell?.MpCost ?? AttackSpell?.MpCost ?? SupportSpell!.MpCost)), status: CasterCostStatus, refreshStats: true) : actor));

    internal (EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects) ApplyReward(EngineBattleState current, IBattleProgressionRule rule)
    {
        var credit = CreditReward(current, rule);
        var growth = ApplyGrowth(credit.Battle, rule);
        return (growth.Battle, Array.AsReadOnly<BattleEffect>([.. credit.Effects, .. growth.Effects]));
    }

    internal (EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects) CreditReward(EngineBattleState current, IBattleProgressionRule rule)
    {
        if (Reward is not { } reward) return (current, Array.Empty<BattleEffect>());
        var result = BattleProgressionRules.Credit(rule, current.GetActor(reward.Actor), reward.Amount);
        return (current.With(actors: current.Actors.Select(row => row.Actor == reward.Actor ? result.Actor : row)), result.Effects);
    }

    internal (EngineBattleState Battle, IReadOnlyList<BattleEffect> Effects) ApplyGrowth(EngineBattleState current, IBattleProgressionRule rule)
    {
        if (Reward is not { } reward) return (current, Array.Empty<BattleEffect>());
        var result = BattleProgressionRules.Grow(rule, current.GetActor(reward.Actor), current.MainSeed, current.Definition.Poison);
        return (current.With(mainSeed: result.Seed, actors: current.Actors.Select(row => row.Actor == reward.Actor ? result.Actor : row)), result.Effects);
    }

    internal ActorRef? FirstAlly => Reactions.Count == 0 ? (Prepared.GetActor(Actor).IsAlly ? Actor : null) : Prepared.GetActor(Reactions[0].Actor).IsAlly
        ? Reactions[0].Actor : Prepared.GetActor(Reactions[0].Target).IsAlly ? Reactions[0].Target : null;
}
