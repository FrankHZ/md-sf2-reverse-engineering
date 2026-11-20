# L3 · 法术结算

已学等级、当前 MP、沉默、可选择位置和实际受影响目标各自判断。取消有效提交前的选择不消耗 MP。**已确认**的原版计算和效果见下；[法术结算合同（英文源）](../../../contracts/battle/spell-resolution.md)保留完整条件和证据。

## 伤害、治疗与经验

- 伤害从法术威力开始。原版已确认的转职加成为 `floor(威力 × 5 / 4)`。DAO、APOLLO、NEPTUN、ATLAS 的计算分支再除以目标数，向下取整。令调整后威力为 P，四分之一为 `floor(P / 4)`。中性伤害为 P，轻微抗性减四分之一，重度抗性取 `floor(P / 2)`，弱点加四分之一。
- BLAZE 会心在 0–31 掷出 0 时再加相同四分之一。随后以当时伤害计算范围 `floor(伤害 / 8) + 1`，从 0 至范围减 1 掷两次并减去，最低伤害 1。每个目标以 `floor(击杀档 × 最终伤害 / 目标最大 HP)` 加入 EXP。击杀另加完整档位，每次累积上限 49，档位与最终奖励见[物理交战](combat-resolution.md#经验金币与掉落)。
- HEAL 1 的基础威力 15、消耗 3 MP。调整后的威力以目标缺失 HP 为上限；HEAL 4 直接补满缺失 HP。只有己方 PRST/VICR/MMNK 取得治疗 EXP；最大 HP 非零的目标贡献 `max(floor(25 × 恢复 HP / 目标最大 HP), 10)`，全行动累计上限 25。同侧奖励不走 Battle01 对敌减半。
- AURA 1、2 分别取中心周围曼哈顿半径 1、2 内有效队友。AURA 4 选全部已放置且存活的己方成员，逐个补满。治疗按战斗员顺序处理，各目标分别计算恢复并累计经验。

参阅[伤害顺序（英文源）](../../../contracts/battle/spell-resolution.md#confirmed-damage-pipeline)及[治疗目标（英文源）](../../../contracts/battle/spell-resolution.md#confirmed-aura-target-and-accumulation-subset)。

## 状态、吸取与失败

- DETOX 1 解毒，2 兼解眩晕，3 和 4 另可解诅咒。解诅咒时先卸下被诅咒的装备，再刷新属性。有效目标只给一次 5 EXP，保留无关状态。没有可解除状态则无反应和经验。
- SLEEP 1、MUDDLE 2、DESOUL 1 对目标使用阈值 `5 + 状态抗性等级`。掷 0–7 达到阈值才生效，抗性等级 3 免疫。MUDDLE 1 无视该抗性而用阈值 5，可刷新一级混乱，无法覆盖二级混乱。DESOUL 成功走死亡和击杀奖励，按目标保留击杀顺序。
- ATTACK 1 首次应用的 ATT 加成为 `floor(基础 ATT × 3 / 8)`。已有该效果的目标重施失败时，持续计数仍可先改为 3，属性保留原计算直到有效刷新。
- DISPEL 1 对有法术的目标使用同样的状态抗性阈值。成功可刷新已有沉默并产生经验。原版施法者若已沉默，带沉默限制的法术仍先扣 MP，随后无目标效果或 EXP。SPOIT 不受该限制，消耗 0，逐目标吸取 `min(对方当前 MP, 3 + 掷出 0–2)`，补给不超过施法者 MP 上限；即使吸取 0，已确认路径仍给 5 EXP。
- 正式回放先应用施法消耗，再按顺序处理逐目标反应，最后发放经验。准备阶段可临时试算，临时状态与正式反应分开。

## 已确认的 BOOST 1 首次应用与重施怪癖

首次 BOOST 1 作用于合格己方目标，在反应刷新时 DEF、AGI 各增加 `floor(对应基础属性 × 3 / 8)`，持续计数为 3。重施已有 BOOST 的目标会先补足计数，再判失败；失败不产生反应、额外经验或立即刷新。SLOW 1 对有效对方目标在刷新时扣减相同比例；抗性等级 0 阈值为 0，其余为 6/7/8。SLOW 和 BOOST 的有效属性可在持续计数变化后暂时沿用旧值。

存活行动者完成其队列项后，处理 STUN、SLEEP、MUDDLE、SILENCE，再扣 SLOW、处理 ATTACK、扣 BOOST，之后处理装备恢复和中毒，再做一次最终刷新。待机也计入；死亡跳过。每个非零计数减 1，降至 0 时才报告失效。重制 Continue 保留尚未刷新的有效属性，直到真实刷新。参阅[状态法术的重制决定](../../design-map.md#状态法术)、[BOOST 证据（英文源）](../../../contracts/battle/spell-resolution.md#confirmed-boost-1-fresh-application-and-recast-quirk)及[SLOW 证据（英文源）](../../../contracts/battle/spell-resolution.md#confirmed-slow-1-status-resistance-matrix)。

上述已确认分支来自有界规则与原版观察，外部条件下的自然多目标排程及若干高级法术完整流程仍为**未知**。
