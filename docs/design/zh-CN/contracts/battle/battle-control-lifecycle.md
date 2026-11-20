# L3 · 战斗控制与生命周期

**已确认：**全新入场准备阵容、场景、战场和回合；挂起恢复继续已有行动状态。每个新回合先处理敌方激活、区域与生成，再按当前敏捷等条件建立行动顺序。拥有行动资格、已经在某个阵营、由玩家控制是三个判断。参阅[进入战斗与选择单次行动](../../synthesis/themes/tactical-battle-loop.md#进入战斗与选择单次行动)和[入口与调度（英文源）](../../../contracts/battle/battle-control-lifecycle.md#entry-and-round-scheduling)。

已提交行动产生效果后，先处理死亡和结果，再处理仍可行动的当前行动者的回合后效果，之后再检查死亡和结果。已发生的状态、资源和队列变化决定后继行动。Bowie 倒下、满足当前遭遇胜利目标和合法撤退会引出不同的费用、恢复、场景和返回位置。参阅[败退与返回](../../synthesis/themes/tactical-battle-loop.md#胜利败北与获准撤退留下不同状态)及[结果合同（英文源）](../../../contracts/battle/battle-control-lifecycle.md#outcome-contract)。
