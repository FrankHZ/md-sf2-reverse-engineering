# Battle02：在可获支援的地格迎战 Huge Rat

**推断的战术选择：**在南侧 Huge Rat 到达 `(10,13)` 时，Jaha 可从 `(12,13)` 到平原 `(11,13)` 攻击。Sarah 可后来到他让出的格子，从一格外治疗。改走道路 `(10,12)` 也能攻击此目标，但少了防御地形效果。选择价值取决于敌人实际目标、先后顺序、是否命中及支援。参阅[英文设计与操作数](../../../synthesis/analyses/battle02-first-contact.md)。

## 条件、合法路线与伤害

假设普通难度、首次全新入场，四名己方是指定的无异常 Level 1 小队。已经计入装备的 Jaha 是 9 HP、ATT 14、DEF 7，装备 Short Axe，近战射程一格。Huge Rat 是 HP 10、ATT 12、DEF 8。Sarah 有 HEAL 1（相邻或自身，3 MP，威力 15）。这些条件用于比较，同一自然行程的实际队伍和资源可能不同。

| Jaha 的选择 | Jaha 普通命中 | Rat 普通命中 | 两次普通来袭后 |
| --- | --- | --- | --- |
| 平原 `(11,13)` | 5 | 每次 4 | 剩 1 HP，可获治疗 |
| 道路 `(10,12)` | 5 | 每次 5 | 倒下 |

两处格子在该假设下均有可行的近战接近；Sarah 的治疗也需选择可达格，并在适用的治疗窗口行动。Jaha 第一次普通命中留下 5 HP，其他行动可以结束交战或改变敌人威胁。若 Jaha 的一次出手未命中，Rat 可能赶在治疗前打第二次。Sarah 可在两击之间恢复第一击的损失；若她在第二击后才行动，平原的 Jaha 尚可吃到治疗，道路上的已倒下。HEAL 1 最多恢复缺失 HP，耗费 3 MP 和一个行动。Rat 也可从合法路径攻击 Sarah；自然移动与目标仍为**未知**。参阅[战场寻路](../../contracts/battle/battlefield-navigation.md)、[物理交战](../../contracts/battle/combat-resolution.md)与[接敌的假设和其余威胁（英文源）](../../../synthesis/analyses/battle02-first-contact.md#choice-cost-and-remaining-threats)。
