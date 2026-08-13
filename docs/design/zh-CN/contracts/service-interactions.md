# 服务交互

- **已确认的原版行为：** 下文描述的四个服务界面的静态行动排序、取消边界与直接资源辅助边界，以及有界 Church Raise H3 准入/提交边界和 Church Cure 事务边界。
- **未知的原版行为：** 调用方准入/返回效果、跨地图/存档重载的持久化、输入/音频/窗口/立绘时序，以及最终呈现合成。
- 重制状态：实现无关且证据约束的合同；Church Raise 与 Cure 有有界运行时边界，更广泛的服务生命周期仍不完整。
- 证据日期：2026-08-13
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`
- 可追溯性：`tests/fixtures/h2/common-menus-static-v1.json` 中的 `sf2-common-menus-static-v1`；
  `tests/fixtures/h3/church-raise-lifecycle-v1.json` 中的 `sf2-church-raise-lifecycle-runtime-v1`；
  `tests/fixtures/h3/church-cure-lifecycle-v1.json` 中的 `sf2-church-cure-lifecycle-runtime-v1`；
  `src/sf2tool/h2/menus.py`、`src/sf2tool/h3/church_raise_lifecycle.py`、`src/sf2tool/h3/church_cure_lifecycle.py`；以及
  `docs/research/common-menus.md`。

> 本文件是 [`service-interactions.md`](../../contracts/service-interactions.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 已确认的交互合同

服务层暴露四个静态行动界面。面向重制版的实现可以在不复制原版呈现资源或时序的情况下建模它们的选择顺序与直接资源效果。

| 界面 | 有序行动 | 已确认的静态边界 |
| --- | --- | --- |
| Shop | buy、sell、repair、deals | Buy/deals 在授予物品前移除金币；特卖购买还移除其特卖条目。Sell 授予金币、移除成员持有的物品，并把珍稀物品路由到特卖。Repair 移除金币并修理所选物品槽位。 |
| Church | raise、cure、promote、save | H2 源码边界保留 Raise 的变更调用顺序 `j_DecreaseGold` → `j_IncreaseCurrentHp`（在 `move.w #CHAR_STATCAP_HP,d1` 之后）→ `UpdateAllyMapsprite`；仅靠源码并未确立最终当前 HP 值。有界 H3 Raise 边界现已观察到接受的案例钳制到 `min(hpMax, 200)` 并完成 mapsprite 辅助。超出该边界的调用方可见行为仍是 **未知**。Cure 在付款后替换状态位；promote 在职业变更/转职前由数据/成员门控；save 到达存档操作。 |
| Caravan | join、depot、item、purge | 顶部、depot 与 item 选择器是按源码排序的 word 相对表。Deposit 先调用存储添加再丢弃成员槽位；derive 与 give 保留不同的正常/交换调用序列；珍稀掉落分支可以调用特卖辅助。 |
| Blacksmith | 履行就绪订单，然后放置待定订单 | 无菱形菜单。静态源码保留就绪/待定计数、履行存储清除/添加/装备顺序、放置门禁与付款/掉落/拾取/标志顺序，以及有界的武器行选择器。 |

Shop、church、caravan、depot 与 item 界面通过共同的菱形/选择边界取消；共享的选择画面具有源码形状的进入、导航、选择、资源加载与清理记录；它的 B→C→A 测试/结果顺序仍是源码级事实，而非生命周期保证。Shop 与 caravan 在非退出行动后循环回它们的行动菜单。Blacksmith 序列是访问驱动而非菱形菜单循环。

### Shop 源码边界

Shop 设计合同只消费 `sf2-common-menus-static-v1` 中 **已确认** 的静态源码边界：Buy 与 Deals 调用减少金币再加物品；Deals 随后调用从特卖移除物品；Sell 调用增加金币然后丢弃物品并有条件地到达珍稀物品 Deals 辅助；Repair 调用减少金币然后按槽位修理物品。它的源码解析器还固定选择器比较、价格算术输入、容量/类型/损坏物品守卫、列表路由与结构化源码指令顺序。它把跳转接口调用方身份与有效的 Shop/选择目标分开保留。它不把观察到的辅助名、分支标签或静态输入位变成对玩家可见时序、调用方返回状态、持久化或呈现的声称。

### Church 源码边界

H2 Church 设计合同消费 **已确认** 的静态路由边界：`routeDerived.raise.mutationCalls` 保留来源派生的变更调用顺序 `j_DecreaseGold` → `j_IncreaseCurrentHp`（在 `move.w #CHAR_STATCAP_HP,d1` 之后）→ `UpdateAllyMapsprite`。这个过滤后的变更辅助顺序不断言没有其他调用介入；仅 H2 并未确立最终当前 HP 值。有界且 **已确认** 的 `sf2-church-raise-lifecycle-runtime-v1` H3 边界则观察到每个被接受且负担得起的 Raise 将当前 HP 钳制到 `min(hpMax, 200)`，并完成原始 `UpdateAllyMapsprite` 辅助。调用方可见的延续、呈现和持久化仍是 **未知**。Cure 的源码/H1/ROM 有界运行时边界确认 poison → stun → curse 的状态/装备提交、允许相等的可负担比较以及 Dark Sword `17000 >> 2 = 4250` 费用；更广泛的 Cure 准入、UI、持久化与呈现仍是 **未知**。Promote 保留其等级/数据门与职业再转职的调用顺序；Save 到达其具名存档调用并记录其独立的挂起分支。该合同不把选择器值、辅助名、状态掩码或跳转接口调用方当作对服务准入、持久化、提示时序或呈现渲染的运行时承诺。

### Caravan 源码边界

Caravan 设计合同只消费 `sf2-common-menus-static-v1` 中 **已确认** 的静态源码边界：顶部 Join/Depot/Item/Purge 表及其两个嵌套表保留它们的源码顺序、word 选择器缩放、取消分支与行动循环分支。Join/Purge 保留它们具名的队伍辅助调用顺序；Depot 保留解析出的 64 物品存储守卫、四槽位接收者守卫、正常与交换转移调用顺序，以及单独守卫的珍稀/不可出售路径。Item Use 与 Give 保留它们的源码调用序列；Equip 仅被建模为具名的选择行动交接，而非已被证实的装备生命周期。物理 ROM 跨度、word 表宽度、物品定义偏移、容量与循环计数器仍是独立的合同字段。别名解析的直接调用方记录指令拼写与有效目标，但不暗示运行时准入、返回状态、持久化、时序或呈现。

### Blacksmith 源码边界

Blacksmith 设计合同只消费 **已确认** 的静态源码边界：一个 24 字节局部帧在处理前清除它的四个具名计数器；字节 force 复制与成对的字面 80 检查/清除位置被分别记录，包括 `TARGETS_LIST_LENGTH` `d7` 计数器来源加载。履行保留接收者取消/容量/装备分支及其加物品、word 顺序存储清除、计数递增、可选装备序列。放置保持源码顺序的材料/客户取消、秘银、转职/职业、确认与金币门禁，然后是它的减少金币、按槽位丢弃、选择器、字面 80 加载、标志清除顺序；最大订单比较是放置后的延续分支。选择器保留源码形状的带前缀职业扫描、初始行与 BRN/RDBN 回退分支、参数到 RNG 范围/结果循环、物品辅助参数分母，以及两字节槽位搜索/写入循环——不是对 RNG 分布、持久化、提示含义、调用方准入或呈现的运行时承诺。

## 面向未来重制的边界

本合同不确立哪些地图/NPC 准入每个服务、取消是否有调用方可见的副作用，或服务何时返回探索。它也不规定订单、特卖、车队存储或剧情标志的存档/重载持久化；输入重复行为；窗口/立绘/音频时序；或最终视觉合成。那些仍是原版运行时问题，而非重制默认值。

未来的 H4 测试应消费 `serviceStateMachines` fixture 对象来获取静态行动排序与直接效果预期，然后仅在分组 H3 服务矩阵解决那些未知项之后添加独立的行为 fixtures。

## Church Raise 运行时边界

已确认的 `sf2-church-raise-lifecycle-runtime-v1` fixture `tests/fixtures/h3/church-raise-lifecycle-v1.json` 只收窄 Raise 的准入和提交：正常 `ChurchMenu` 入口到达 Raise；存活成员被跳过；死亡成员按列表顺序处理；原始、允许相等的可负担比较接受 `等级 × 10` 加上转职后的 `200`。接受且可负担时观察原始 `DecreaseGold`、带 HP 上限 `200` 的 `IncreaseCurrentHp`，再到 `UpdateAllyMapsprite`；拒绝和金币不足路径没有这些辅助入口。这是实现无关的顺序和变更边界。observer 范围内的恢复还覆盖已选择 force-list 的长度和字节、当前 portrait word、生成的 harness/action/prompt span 和一个 18 字节 terminal trampoline。该 trampoline 在回调读回之前执行真实的 `MOVEA.L` 以恢复捕获的 CheckSram frame；失败诊断报告未恢复的 bootstrap frame，而不声称寄存器写入成功。Python 验证已修补 session ROM 被删除。这不是呈现、持久化、经济平衡或全内存恢复规则。

## Church Cure 运行时边界

已确认的 `sf2-church-cure-lifecycle-runtime-v1` fixture `tests/fixtures/h3/church-cure-lifecycle-v1.json` 只收窄原始 Church Cure 事务边界。它保留 poison → stun → curse 的源码顺序、每个成员/物品的 `dbf` 迭代、受控零提示结果到允许相等的 `cmp.l d0,d1; bcc` 可负担路径，以及 poison/stun/curse 的 `10`/`20`/Dark-Sword-`17000 >> 2 = 4250` 费用。完整的十一记录 cohort 包含无状态、拒绝、差一金币、恰好金币和 `4280 → 4270 → 4250 → 0` 的有序成功序列。Poison/stun 提交观察原始 `DecreaseGold` 后接 `SetStatusEffects`；curse 观察 `DecreaseGold`、`UnequipAllItemsIfNotCursed` 及其 `UpdateCombatantStats` tail/RTS 完成。这是实现无关的资源/状态/装备排序边界，不是文字、提示 UI 语义、持久化、经济平衡、服务准入或渲染呈现的规则。observer 恢复范围仅包括触及的金币、完整 combatant record、target-list 长度/字节、dialogue scratch、portrait scratch、generated RAM 和 bootstrap frame。
