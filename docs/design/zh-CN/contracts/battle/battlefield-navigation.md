# L3 · 战场寻路与目标选择

**已确认：**选格受行动者移动类型、逐格地形成本、移动预算和占格共同约束。当前阵营规则认可的阻挡者所占的格子，在可达格扩展时排除。已占格能否作为行动目标另行判断。不可通行地形不能仅凭总距离内就纳入。受限移动需要按其实际限制判断合法终点。参阅[移动与加权传播（英文源）](../../../contracts/battle/battlefield-navigation.md#weighted-propagation)。

可以从合法落点选择行动。可达目的地、可供选择的中心格和实际效果目标分别判定。检查行动的最小与最大距离以及作用范围；在范围内仍需满足阵营、状态、占格等规则。取消的交接见[战术战斗循环](../../synthesis/themes/tactical-battle-loop.md#玩家选择取消与提交)。参阅[范围与目标（英文源）](../../../contracts/battle/battlefield-navigation.md#range-and-target-construction)。
