# L3 · 战场寻路与目标选择

**已确认（静态）：**原版普通玩家移动受移动类型、逐格地形成本、移动预算和占格约束。对方占格会阻断可达格扩展。可到达的友方占格会列入移动范围，可以经过。确认停步时会拒绝其他战斗单位所占的格子。已占格能否作为行动目标另行判断。预算充足也不能经过不可通行地形，受限移动另有停步规则。范围格的实际显示样式与时序仍为**未知**。参阅[移动范围与停步（英文源）](../../../contracts/battle/battlefield-navigation.md#player-movement-display-and-stopping)及[移动与加权传播（英文源）](../../../contracts/battle/battlefield-navigation.md#weighted-propagation)。

可以从合法落点选择行动。可达目的地、可供选择的中心格和实际效果目标分别判定。检查行动的最小与最大距离以及作用范围；在范围内仍需满足阵营、状态、占格等规则。取消的交接见[战术战斗循环](../../synthesis/themes/tactical-battle-loop.md#玩家选择取消与提交)。参阅[范围与目标（英文源）](../../../contracts/battle/battlefield-navigation.md#range-and-target-construction)。

**已确认（静态）：**原版 AI 待机移动沿用当前地形来扩展可达格，传播时不另加敌方占格阻挡，选停步格时才检查是否有人占格。重制在受控案例中沿用待机选路时的范围，走过一个已占格并停在空格。原版中该路线的实际运行、自然游玩何时会遇到，以及画面时序仍为**未知**。参阅[待机移动与占格（英文源）](../../../contracts/battle/battlefield-navigation.md#standby-movement-occupancy)及[重制移动规则与验收（英文源）](../../../../../remake/docs/domain/gameplay-rules.md#movement-terrain-and-occupancy-review)。

## 移动预算与射程例外

**已确认：**原版传播预算为当前 MOV 的两倍，以移动类型的逐格代价从起点累计。恰好耗尽预算的格仍可到达，但不再向外扩展。邻格按右、左、上、下探测，一旦准入就不会执行一般的代价重算。自动选攻击落点已在范围内可留在原地，否则优先最低移动代价，同值选扫描顺序先者（从上到下、从左到右）。

射程以曼哈顿距离计算。无武器默认距离 1，Brass Gunner 与 Kraken Arm 为 1–2，Kraken Head 为 1–3；已装备武器和法术各有自身范围。攻击与支援按相对阵营选取存活且已放置的非中立对象。被挡格可以不显示为可移动位置，却依行动条件成为合法目标。
