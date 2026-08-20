# 战斗 AI 决定合同

- **已确认原版行为：** 下文描述的完整 26 文件 AI 清单、动作过滤器、静态优先级/治疗/支援/动作/移动/控制模型、指令集路由、临时地形清理与受限 14 用例最终攻击动作/目标运行时矩阵。
- **未知原版行为：** 排队的调用方可视过滤/治疗/支援/移动/控制用例、已接受运行时矩阵之外的带符号/溢出边界、自然地图路径选择、完整多回合行为、玩家可见公平性或意图，以及呈现/时序。
- 重制状态：实现无关 Phase 3 合同；尚未选择 AI 架构、难度重设计、可解释性策略或刻意兼容偏差。
- 证据日期：2026-08-08
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`battle-ai-decision.md`](../../contracts/battle-ai-decision.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同定义从 AI 指令集分发经动作类别、目标与移动输出的原版静态决定边界。它拥有：

1. 法术/物品资格扫描及其停止/继续不对称性；
2. 潜在伤害、目标优先级、治疗与支援打分输入；
3. 最终攻击动作类别与目标选择；
4. Move、Move Order、临时地形与移动字符串交接；
5. 顶层指令集、激活/待机/swarm 与特殊攻击者路线；
6. 最终攻击动作/目标选择的显式受限运行时观察。

它不拥有真实战斗/法术结果、战场传播内部、战斗动作构建、玩家战术、遭遇平衡或呈现。那些仍属于[交战解决](../../contracts/combat-resolution.md)、[法术解决](../../contracts/spell-resolution.md)、[战场导航](../../contracts/battlefield-navigation.md)、[战斗动作构建](../../contracts/battle-action-construction.md)与[战斗控制/生命周期](../../contracts/battle-control-lifecycle.md)。

可执行所有者是：

- `sf2-battle-ai-static-v1`，位于 `tests/fixtures/h2/battle-ai-static-v1.json`；
- `sf2-battle-ai-priority-static-v1`，位于
  `tests/fixtures/h2/battle-ai-priority-static-v1.json`；
- `sf2-battle-ai-healing-static-v1`，位于
  `tests/fixtures/h2/battle-ai-healing-static-v1.json`；
- `sf2-battle-ai-support-static-v1`，位于
  `tests/fixtures/h2/battle-ai-support-static-v1.json`；
- `sf2-battle-ai-action-choice-static-v1`，位于
  `tests/fixtures/h2/battle-ai-action-choice-static-v1.json`；
- `sf2-battle-ai-movement-static-v1`，位于
  `tests/fixtures/h2/battle-ai-movement-static-v1.json`；
- `sf2-battle-ai-remaining-static-v1`，位于
  `tests/fixtures/h2/battle-ai-remaining-static-v1.json`；
- `sf2-battle-ai-action-choice-runtime-v1`，位于
  `tests/fixtures/h3/battle-ai-action-choice-v1.json`。

研究所有者是[Battle AI 静态清单与决定合同](../../../research/battle-ai.md)。

## 规范决定状态

适配器必须为以下内容保留独立所有权：

| 状态 | 合同角色 |
| --- | --- |
| 指令集与命令游标 | 有序命令尝试；在首个成功处停止 |
| 动作候选 | 物理、法术与物品可行性加所选法术/物品条目 |
| 目标候选 | 每族目标字节、优先级字节与移动字节 |
| 思考 RNG | 低字节 `RANDOM_SEED_COPY` 流，与基础 RNG 分开 |
| 移动输出 | 移动字符串加动作字段，包括 Stay-with-movement 用例 |
| 临时战场状态 | 为路径选择安装并在每次控制退出时清除的阻挡位 |
| AI 记忆 | 最后目标、移动计数与待机相对位置状态 |

AI 的潜在伤害分数不是真实伤害，成功的移动结果也不意味着动作字段非 Stay。把这些值折叠进一个 `decision` 对象而不保留类型化中间状态，会丢失已确认的原版区分。

## 动作获取器与过滤器合同

全部五个动作获取器至多扫描四个槽。没有法术返回 `SPELL_NOTHING = 0x3F`；没有物品返回 `ITEM_NOTHING = 0x7F`。

### 攻击法术

**已确认静态：** 己方施法者与混乱敌人在要求攻击法术类型 0 之前只接受 BLAZE、FREEZE、BOLT、BLAST、KATON 与 RAIJIN。己方路径被迫经过该过滤器，因为其局部混乱标志被无条件设置。未混乱敌人绕过法术名允许列表但仍要求攻击类型。拒绝推进到下一槽；接受调用 `GetHighestUsableSpellLevel`。

强制的己方过滤器及其自然调用方可视后果在排队的己方/未混乱敌人/混乱敌人运行时矩阵存在之前保持 **未知**。

### 攻击物品

**已确认静态：** 攻击物品必须通过战斗可用性。已装备条目绕过 AI 使用位；未装备条目要求它。己方/混乱路径使用较小的 BLAZE/FREEZE/BOLT/BLAST 允许列表并要求攻击类型。

拒绝策略不对称：

- 真正不可用的物品推进到下一槽；
- 缺失 AI 使用位、不允许的使用法术或非攻击使用法术会用 `ITEM_NOTHING` 中止整个搜索；
- 治疗物品拒绝则继续扫描；
- Healing Rain 绕过 AI 使用位要求；其他治疗物品要求它。

这是静态控制流。物品栏顺序后果与调用方重试行为保持 **未知**。

## 优先级是打分模型，不是解决

物理潜在伤害是 `max(current ATT - current DEF, 1)`，然后按地形设置乘并向下取整：`256/256`、`230/256` 或 `205/256`。在 terrain 之前应用最小值意味着一分估计可以变成零。法术打分从定义威力开始，只应用抗性设置：minor 减四分之一、major 减半、weakness 加四分之一。区域法术优先级按每个受影响目标求和。

这些是目标打分估计。它们不得替换真实战斗/法术公式。

`pt_TargetPriorityScripts` 包含 16 个指针，按难度乘四加激活列索引。己方强制第 2 列。敌人法术打分掩蔽两个激活位；普通攻击提取低半字节，即使表只有四列。四个脚本形状按可执行 fixture 的记录组合受限致命性、先前目标、HP 阈值、移动、职业与 RNG 项。

职业调整只适用于未混乱己方。先前目标字节等于 Sarah（己方 1）会强制法师调整表。自然调用方状态是否如源码名所示到达该条件保持 **未知**。

## 治疗与支援决定

### 治疗

**已确认静态：** 混乱施法者退出。Healing Rain 在法术之前被测试，只在第一个敌人战斗员 HP 低于或等于一半时被准入；其动作目标是施法者，因为物品法术基于区域。否则只接受 HEAL 或 AURA，最低 MP 门为 3 与 7。失败回退到普通治疗物品，当法术与物品都到达动作加载时物品胜出。

存活的同侧目标要求 `3 * currentHP <= 2 * maxHP`；三分之二的相等也合格。单独命名的半 HP 辅助也包含相等。

治疗等级辅助返回：

| 缺失 HP | 静态结果 |
| ---: | --- |
| 0-2 | 不施放 |
| 3-14 | 等级 1 |
| 15-28 | 已知则等级 3，否则等级 1 |
| 29+ | 已知则等级 4；否则已知则等级 3，再否则等级 1 |

其 MP 回退移五位而非六位，并在不掩蔽等级位的情况下加打包法术条目。它从不返回等级 2；调用方只能通过其独立覆盖重新引入等级 2。这些缺陷是 **已确认静态**；MP/阈值调用方可视结果与字节分数溢出保持排队运行时问题。

### 支援

**已确认静态：** 支援仅限敌人，混乱敌人 Stay。只考虑第一个支援法术；如果它不是 MUDDLE 2 或 DISPEL 1，命令不扫描后续槽。

- MUDDLE 2 按受影响目标数给区域中心打分，并移除低于三的中心。
- DISPEL 为每个带攻击或治疗法术的受影响目标加一，并移除低于二的中心。
- 相等字节优先级选择较晚候选。
- 如果所选中心没有合法攻击位置，命令 Stay 而不是尝试下一个排名的中心。

ATTACK 与 BOOST 2 分发分支在该准入门之后存在，但通过普通命令入口不可达。其休眠缺陷保持源码事实，不是必需可达行为。

## 最终攻击动作与目标选择

**已确认静态与受限运行时：** 最终攻击命令记录物理、法术与物品列表是否非空。

- 无可行类别返回 Stay；
- 仅物理则攻击；
- 唯一法术或物品被选择，尽管普通路径在注意到物理不可用之前仍消耗 `RNG(6)`；
- 物理加法术给出法术掷骰 2 与 4、物理掷骰 0、1、3 与 5；
- 物理加物品给出物品掷骰 3 与 5、物理其余四个；
- AQUA 加物理绕过该掷骰并总是施放 AQUA；
- 当法术与物品都可行时，物理即使可行也被忽略。

法术对物品选择使用思考 RNG，把低种子副本字节更新为 `(seed * 541 + 12345) & 0xFF` 直到低于二；0 选择法术，1 选择物品。

优先级从初始最大值零按带符号字节比较，因此 128-255 可作为负数被忽略。返回优先级封顶于 15，而每个与原始最大值持平的候选按反向输入顺序保留。普通 0-127 移动平局打破选择最大存储移动值；相等值选择较晚收集的目标。

14 用例 H3 测试夹具用受控目标列表与种子回放一次自然 Battle 01 进入。它确认全部七种非空可行性形状、两个 RNG 族/结果、AQUA 绕过、普通优先级、最大移动选择与等移动较晚目标选择。它**不**闭合优先级 127/128/255、高于 127 的移动、会心职业组或其他调用方状态。

## 移动与 Move-Order 合同

`aiCommand_Move` 用预算 128 构建移动。混乱单位直接选择其阵营首个索引而不检查 HP 或放置；普通路径收集存活的已放置对手，并在其成本循环前没有空列表守卫。成本按无符号字节升序排序，选择首条目。

Kraken Leg/Arm/Head 使用其自己的 16 字节成本表。在用预算 4 的初步移动字符串之后，命令按半径 0 然后 1 搜索攻击位置。失败把动作改为 Stay，而函数仍返回成功。

`aiCommand_MoveOrder` 在移动前尝试 Attack。零 MOV、缺失顺序、死亡跟随目标或失败地形检查产生 Stay。仅移动成功也编码为 Stay 加非空移动字符串。其构建器使用移动数组预算 128、初步预算 `MOV * 2` 与攻击位置半径 0 到 3。

这些是静态输入/输出规则。空列表行为、混乱无效目标、Kraken 回退、己方模式栈状态与自然路径选择保持 **未知**。

## 临时地形与象限辅助

**已确认静态：** move-order 象限位 0 表示目标在左、位 1 表示在下，与上游注释相反。工作边界扩展四个格子并夹断到 48×48 地形域。

临时阻挡辅助使用地形位 6 与 7，同时让永久的 `0xFF` 单元格保持原样。Block-and-carve 首先阻挡非永久地形，然后清除 Manhattan 环 0-2，或在拴到最后目标时清除环 0-4。解析的环表包含 1、4、8、12 与 16 个条目。每个 AI 控制退出清除临时阻挡标志。

地图边缘效果与调用方可视路线选择没有已接受运行时所有者，保持 **未知**。现代寻路器不得假设临时地形是普通永久碰撞。

## 指令集、激活与特殊攻击者

AI 控制的己方角色使用指令集 6。敌人在 16 个指令集之间选择并在首个成功命令处停止。指令集 10/11 共享 Stay；13/14 共享会心/队长集。18 条目寻路模式表选择普通、阻挡不可移动或 block-and-carve 模式。

激活前控制器清除新触发区域。无触发区域从非激活开始；非激活敌人运行待机并被强制 Stay。死亡的主跟随顺序被其次级顺序替换。

Swarm 指令集 15 只在战斗 16、20 或 22 中满 HP 时等待，并使用战斗特定阈值表。`CountDefeatedEnemies` 在遍历敌人时错误地使用己方子段长度；调用方可视后果未在运行时闭合。

Prism Flower 与 Zeon Guard 绕过指令集：有朝向目标时选择棱镜激光动作 6 与首个目标；否则 Stay。Burst Rock 只在至少一个目标且思考 `RNG(6) == 4` 时爆炸；否则执行 Move 1 然后强制动作 Stay 同时保留移动。

所有这些是 **已确认静态** 路线，不是对战术目的、公平性、选择频率或动画的陈述。

## 分发器、待机与未使用辅助

分发器处理命令值 0-7、10-14 与 16-19。保留的 8、9、15 与未知值在不选择动作的情况下返回。待机首先使用思考 `RNG(8)`：2、4 与 6 Stay；其余五个掷骰进入资格。其打包记忆保留移动计数与先前相对位置索引。

move-order 待机分支把返回的 X 复制进起始 X 与起始 Y 两者，其初始边界检查接受坐标 48。下游拒绝与调用方可视效果保持 **未知**。静态 16 用例资格模型产生 11 个 Stay、4 个普通移动与 1 个 move-order 配置；这不是运行时频率分布。

五个显式未使用辅助在 AI 子树中有零直接调用。其比较与四槽查找合同保持源码事实，不是强制可达重制行为。

## 保真与现代化边界

原版保真 AI 适配器必须保留：

- 指令集顺序、首成功分发与临时地形清理；
- 获取器扫描限制、允许列表、拒绝不对称性与打包条目行为；
- 打分单位、字节带符号性、平局收集、RNG 流所有权与受限 H3 结果；
- 治疗/支援门与无回退分支；
- Move/Move Order 输出区分、预算、半径顺序与临时阻挡模式；
- swarm/特殊/待机源码形状门与显式受限缺陷。

未来重制可以用规划器替换指令集、改进寻路、修复缺陷、暴露 AI 推理、重平衡难度、使用更安全的数字类型或重新设计特殊攻击者。这些是产品决定，不是原版事实。fixture 可见变更需要显式决定与 H4 expected-deviation 记录。

## H4 验收面

未来 H4 适配器应比较：

1. 源码形状法术/物品清单的获取器结果与槽/中止行为；
2. 不替换真实伤害的优先级/治疗/支援中间分数；
3. 全部 14 个动作选择运行时用例，包括 RNG 消耗与目标平局顺序；
4. Move/Move Order 动作加移动字符串输出与临时地形清理；
5. 指令集首成功轨迹、激活、swarm、特殊攻击者与待机输出；
6. 静态缺陷或不安全/未定义调用方状态的已声明偏差。

H4 必须把自然游玩频率、公平性、战术意图、渲染行为与分项分组运行时队列保持在已接受适配器之外，直到单独证据或设计。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 清单与法术/物品过滤器 | **已确认静态** | `sf2-battle-ai-static-v1`（[`battle-ai-static-v1.json`](../../../../tests/fixtures/h2/battle-ai-static-v1.json)） | 调用方重试/顺序结果与自然可达性 |
| 优先级与潜在伤害打分 | **已确认静态** | `sf2-battle-ai-priority-static-v1`（[`battle-ai-priority-static-v1.json`](../../../../tests/fixtures/h2/battle-ai-priority-static-v1.json)） | 带符号/溢出边界、脚本可达性、意图 |
| 治疗与支援 | **已确认静态** | `sf2-battle-ai-healing-static-v1`（[`battle-ai-healing-static-v1.json`](../../../../tests/fixtures/h2/battle-ai-healing-static-v1.json)）与 `sf2-battle-ai-support-static-v1`（[`battle-ai-support-static-v1.json`](../../../../tests/fixtures/h2/battle-ai-support-static-v1.json)） | 阈值/MP/溢出与无回退运行时效果 |
| 最终攻击动作/目标模型 | **已确认静态** | `sf2-battle-ai-action-choice-static-v1`（[`battle-ai-action-choice-static-v1.json`](../../../../tests/fixtures/h2/battle-ai-action-choice-static-v1.json)） | 会心组与带符号移动/优先级边界 |
| 最终攻击动作/目标运行时接缝 | **已确认运行时，受限** | `sf2-battle-ai-action-choice-runtime-v1`（[`battle-ai-action-choice-v1.json`](../../../../tests/fixtures/h3/battle-ai-action-choice-v1.json)） | 其他调用方、地图、指令集与排队边界用例 |
| Move 与 Move Order | **已确认静态** | `sf2-battle-ai-movement-static-v1`（[`battle-ai-movement-static-v1.json`](../../../../tests/fixtures/h2/battle-ai-movement-static-v1.json)） | 自然地图路线、空/无效目标、己方模式栈状态 |
| 分发器、地形辅助、指令集、swarm、特殊、待机、未使用 | **已确认静态** | `sf2-battle-ai-remaining-static-v1`（[`battle-ai-remaining-static-v1.json`](../../../../tests/fixtures/h2/battle-ai-remaining-static-v1.json)） | 调用方可视缺陷、频率、呈现、完整多回合行为 |
| 战术、公平性、平衡、呈现 | **未知 / 产品决定** | 无聚合可执行所有者 | 需要用户研究、设计决定与独立验收 |

## 复现

```powershell
uv run sf2 h2 battle-ai
uv run sf2 h3 battle-ai-action --timeout-seconds 150
uv run sf2 design-contracts test
uv run sf2 research-index test
```
