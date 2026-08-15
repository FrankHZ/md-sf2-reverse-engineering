# 战斗员状态访问合同

- 状态：**已确认静态战斗员选择器、获取器、变更包装器、夹断辅助、距离与未使用类型编码面；九个受限夹断操作的已确认运行时行为**
- 证据日期：2026-08-08
- 范围：对 56 字节战斗员条目域的源码形状访问，不指定更高层战斗、名册、持久性、呈现或平衡含义

> 本文件是 [`combatant-state-access.md`](../../contracts/combatant-state-access.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同定义原版低级战斗员状态访问边界。它不定义战斗员何时存在、加入队伍、进入战斗、行动、死亡、持久或呈现给玩家。

- **已确认**：选择器到条目源路线与派生 56 字节步长；31 个获取器条目；53 个变更包装器；七个夹断辅助及其静态调用方清单；九个观察夹断操作；双选择器静态距离函数；以及单独受限、源标记未使用的战斗员类型编码。
- **推断**：仅无效选择器路线的调用方可视意图。源写错误码、通过 trap 参数禁用 VInt 并循环，但没有已接受运行时所有者闭合调用方或玩家观察什么。
- **未知**：九操作矩阵之外的调用方相关获取器与变更结果；`DecreaseAndClampWord`、两个 Long 辅助、decrease-current-ATT、间接辅助可达性、选择器-160 运行时使用、距离边界行为、名册与战斗生命周期、存档持久性、UI 与呈现，以及平衡意图。

[party-roster 合同](../../contracts/party-roster-state.md) 拥有成员与活跃队伍命令。[battle-control 合同](../../contracts/battle-control-lifecycle.md) 拥有战斗准入、回合、清理与结果。[Level-up](../../contracts/level-up.md)、[交战解决](../../contracts/combat-resolution.md)与[战场导航](../../contracts/battlefield-navigation.md) 拥有更高层消费者。那些合同可以变更或读取该状态，但其生命周期语义不折叠进该低级 ABI。

## 证据所有者

`sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)）是专用 H2 所有者。其验证器是[`stats.py`](../../../../src/sf2tool/h2/stats.py)，其带来源解释是[Common Stats 与 Inventory Services](../../../research/common-stats.md)。所有者保留此处使用受限面的完整指令与局部标签语料、source/H1 地址、字段宽度与偏移使用站点、辅助 ABI、调用方身份与精确顺序。

`sf2-stat-clamp-boundaries-v1`（[`stat-clamp-boundaries-v1.json`](../../../../tests/fixtures/h3/stat-clamp-boundaries-v1.json)）是 Slade/THIF 升级用例（含九个受控包装器操作）的受限 H3 所有者。它不确认每个包装器或辅助。其与升级合同的既有关联保持有效；本合同只消费同一 fixture 观察的低级夹断结果。

common-stats 聚合中的兄弟标志、队伍、车队、Deals、物品清单、名称/物品/法术服务、新游戏与 unused-null 记录刻意排除在本合同 research-index 边界之外。

## 选择器与条目地址拓扑

**已确认静态：** `GetCombatantEntryAddress` 消费选择器低字节并派生 56 字节条目地址。源按以下方式路由选择器：

| 源谓词 | 路线 | 地址域后果 |
| --- | --- | --- |
| 选择器低于 `128`、至多 `31` | 己方 | 掩蔽选择器乘以 56 |
| 选择器低于 `128`、高于 `31` | 错误 | 写源错误状态、调用 VInt trap 边界，然后自循环 |
| 选择器至少 `128`、在源 `bhi` 比较下至多 `160` | 敌人路线 | 减 `96`、掩蔽，然后乘以 56 |
| 选择器高于 `160` | 错误 | 同一静态错误路线 |

对 `COMBATANT_ENEMIES_SPACE_END=160` 的比较使用 `bhi`，因此已接受源码形状谓词准入选择器 `160`。那会产生调整槽索引 `64`；本合同不把它重新解读为有效普通敌人槽。选择器 `160` 的自然可达性、分配状态安全与运行时行为保持 **未知**。实现必须为保真诊断保留源谓词，同时暴露独立验证应用级选择器。

地址计算掩蔽到一字节、带中间副本两次左移三、从第二个减去第一个移位值，然后把结果加到 `COMBATANT_DATA`。这证明 56 字节步长。它不证明每个条目中每个字节都有一个通用语义视图。

## 获取器暴露记录视图

**已确认静态：** 获取器面包含从 `GetCombatantName` 到 `GetDefeats` 的 31 个有序条目，位于 586 字节区间 `0x82D0..0x851A`。普通字节读取在加载前清除 16 位结果；字读取保留 16 位字段。X 与 Y 获取器符号扩展其存储字节。

下表是获取器暴露投影，不是未列字节未使用的声称：

| 偏移 | 宽度 | 获取器面向视图 |
| ---: | ---: | ---: |
| `0..9` | 受限字节序列 | `GetCombatantName` 检查的己方名称存储；敌人名走独立敌人索引/表路径 |
| `10`、`11` | 各字节 | 职业、等级 |
| `12`、`14` | 各字 | 最大 HP、当前 HP |
| `16`、`17` | 各字节 | 最大 MP、当前 MP |
| `18..25` | 各字节 | 基础/当前 ATT、DEF、AGI 与 MOV 对 |
| `26`、`28` | 各字 | 基础与当前抗性 |
| `30`、`31` | 各字节 | 基础与当前 prowess |
| `44` | 字 | 状态效果 |
| `46`、`47` | 各符号扩展字节 | X 与 Y |
| `48` | 字节 | 当前 EXP |
| `49` | 字节 | 高半字节移动类型与低半字节 AI 指令集视图 |
| `50` | 字 | move-order 高/低字节拆分；己方视图还在此暴露击杀 |
| `52` | 字 | 激活位域 |
| `54` | 按获取器字节或字 | 触发区域半字节拆分；己方视图还把败北暴露为字 |
| `55` | 字节 | 敌人选择器的敌人身份 |

偏移 50 与 54 别名是类型与获取器相关。重制不得把 56 字节状态替换为一个扁平语义结构，使移动顺序与击杀、或触发区域与败北在无显式覆盖规则的情况下同时独立可写。

`GetCombatantName` 也非均匀：己方使用条目局部字节（九字符边界），敌人使用存储敌人索引与独立敌人名表。`GetEnemy` 在非敌人路线返回 `-1`，在敌人路线读取偏移 55。这些是访问事实，不是显示本地化、身份生命周期或普通调用方输入的声称。

## 变更包装器面

**已确认静态：** 变更面包含从 `LoadAllyName` 到 `DecreaseCurrentMov` 的 53 个有序条目，位于 1,046 字节区间 `0x855A..0x8970`：

| 包装器类 | 计数 | 合同含义 |
| --- | ---: | --- |
| 己方名称加载 | 1 | 不同受限复制形式 |
| 直接设置 | 27 | 源特定字节/字或打包字段写入 |
| 增加 | 16 | 包装器向更低辅助提供字段偏移、宽度与夹断参数 |
| 减少 | 9 | 包装器向更低辅助提供字段偏移、宽度与夹断参数 |

fixture 保留每个包装器的选择器/值或增量宽度、字段使用站点、更低辅助 ABI、寄存器角色、保留/终止顺序与直接/有效调用方身份。直接设置器、打包 move-order 与触发区域合并、守卫击杀/败北与当前 HP/MP 最大值读取保持不同操作。它们不得在其宽度、读-改-写顺序与守卫行为被保留前归一化为无约束通用设置器。

静态包装器结构本身不确认每个选择器、调用方或值的成功变更。特别是，不能从包装器清单派生“所有战斗员值夹断到 0..200”等全局规则。

## 夹断辅助算法

**已确认静态：** 七个辅助按该精确顺序占据 268 字节区间 `0x9312..0x941E`：

1. `IncreaseAndClampByte`；
2. `IncreaseAndClamp7Bits`；
3. `DecreaseAndClampByte`；
4. `IncreaseAndClampWord`；
5. `DecreaseAndClampWord`；
6. `IncreaseAndClampLong`；
7. `DecreaseAndClampLong`。

每个辅助接收选择器、字段偏移、增量/结果寄存器与调用方提供的最小与最大值。字节、字与 Long 形式保留其自身操作数宽度与分支顺序；它们不可互换为主机语言算术。Increase-byte 在最大与最小收敛前检查进位。Increase-word 与 Increase-Long 在范围比较前使用其源负结果分支。Decrease 形式复制增量、读取存储字段、减，然后应用其源特定下溢/最小/最大序列。

`IncreaseAndClamp7Bits` 是独立算法。它保留存储 `0x80` 位、用 `0x7F` 掩蔽工作值、加并夹断低部分、把保留位 OR 回、写字节，并把返回字归一化到 `0..255`。通用无符号字节夹断会丢失该状态拆分。

完整静态调用方清单在变更包装器文件中找到 25 个直接站点：

| 辅助 | 直接包装器站点 |
| --- | ---: |
| `IncreaseAndClampByte` | 10 |
| `IncreaseAndClamp7Bits` | 2 |
| `DecreaseAndClampByte` | 8 |
| `IncreaseAndClampWord` | 4 |
| `DecreaseAndClampWord` | 1 |
| `IncreaseAndClampLong` | 0 |
| `DecreaseAndClampLong` | 0 |

零直接站点不证明通过别名、数据驱动分发或未索引代码的运行时不可达性。因此两个 Long 辅助保持保留静态形式，运行时使用 **未知**。

## 受限运行时夹断矩阵

**已确认运行时：** 已接受单用例 H3 fixture 恰好观察九个操作：

| 操作族 | 用例 | 已接受边界示例 |
| --- | ---: | --- |
| 增加字节 | 4 | ATT/DEF/MOV 上限 200；`250 + 50` 的字节进位也产生 200 |
| 增加字 | 1 | 最大 HP `65535 + 2` 在源分支序列完成前回绕到 `1` |
| 增加七位 | 1 | 敏捷 `227` 加 `2` 保留 `0x80` 并产生 `228`，低七位上限 100 |
| 减少字节 | 3 | DEF `3-5`、MOV `1-2` 与 AGI `5-10` 各产生 0 |

该矩阵不观察 `DecreaseAndClampWord`、任一 Long 辅助、decrease-current-ATT、全部 53 个包装器、无效选择器或间接可达性。那些保持显式 H3 扩展门。

## 距离辅助边界

**已确认静态：** `GetDistanceBetweenCombatants` 是 100 字节区间 `0x941E..0x9482`。它接收两个 16 位选择器值、保留 `d0-d1/d3-d5` 并在 `d2` 返回 16 位结果。它按顺序获取行动者 X/Y 与目标 X/Y。每个获取器后它把低字节与 `-1` 比较；任何匹配取源 `d2=-1` 路径。否则它减每个轴、在源进位清除/无借位分支上条件取负，然后加两个字中间值。

静态清单找到两个直接调用方，无经跳转接口别名的调用。当前没有 H3 fixture 观察该函数。静态确认 `d2=-1` 路径的自然/运行时可达性、字回绕边界、坐标解读与调用方可视使用保持 **未知**；源码形状操作不得被提升为通用几何规则。

## 未使用战斗员类型编码

**已确认静态：** 源标记未使用的 `GetCombatantType` 服务对敌人选择器返回敌人索引。对己方它设置位 15 并组合职业类型、己方计数缩放与己方索引。原始编码是单独保留兼容面。

上游 unused 标签与缺乏运行时所有者意味着正常可达性与玩法含义 **未知**。重制可以从普通域 API 省略该值，但原版保真适配器必须保留可复现编码器而非重新分配其位。

## 实现无关状态模型

```text
CombatantSelector
  rawValue
  sourceRoute: ally | enemy | error
  sourceAdjustedIndex
  applicationValidated: boolean

CombatantEntry
  rawBytes[56]
  fieldViews[]
  allyOverlay
  enemyOverlay

FieldView
  name
  offset
  widthBits
  signednessOrPacking
  applicableOverlay

GetterDefinition
  sourceIdentity
  selectorRule
  fieldViewOrCustomPath
  returnShape

MutationDefinition
  sourceIdentity
  selectorRule
  fieldView
  directOrReadModifyWriteOrder
  lowerHelperRef
  guardAndReturnShape

ClampDefinition
  sourceIdentity
  operandWidth
  orderedArithmeticAndBranches
  preservedBits
  observedCases[]

DistanceDefinition
  selectorOrder
  coordinateReadOrder
  invalidComparison
  orderedAxisOperations
  returnShape
```

这是逻辑 parity 模型，不是必需引擎内存布局。原始 56 字节条目与类型化视图共存，使别名偏移、打包字段、带符号坐标读取与源特定变更顺序保持可诊断。应用代码可以暴露更安全类型化状态，但兼容层不得擦除源准入与应用验证之间的区分。

## 原版保真与现代化

原版保真模式保留选择器路由、56 字节步长、获取器/变更身份、字段宽度与覆盖、夹断分支顺序、九个观察结果、距离操作顺序与未使用类型编码。它报告未观察路线而非静默指定行为。

现代引擎可以使用实体句柄、独立己方/敌人组件、显式选项/错误值、饱和数字类型、更安全坐标 API 与事件源变更。只有当适配器能复现已接受原版面向观察且刻意偏差被分别记录时，那些才是合法实现选择。

运行时条目引用的玩家名称与其他原版内容保持私有输入。公开 fixture 与本合同保留结构元数据、标识符、计数与受限观察，而非可分发原版内容。

## H4 验收门

未来重制战斗员状态适配器只在以下情况通过本合同：

1. 己方、敌人、缺口/错误与选择器-160 源谓词保持可复现，与更安全应用验证分开；
2. 派生 56 字节步长与所有获取器暴露偏移、宽度、带符号读取、打包视图与己方/敌人覆盖在不压平冲突的前提下往返；
3. 全部 31 个获取器与 53 个变更包装器身份保持可追溯到其源顺序、自定义路径或字段视图、辅助边界、守卫与返回形状；
4. 全部七个夹断辅助保留精确操作数宽度与有序分支/写入行为，包括七位 `0x80` 保留规则与零直接站点 Long 辅助；
5. 九个已接受 H3 操作复现其精确前/量/上限/后结果，而不泛化到未观察辅助或包装器；
6. 双选择器距离辅助保留读取顺序、字节大小 `-1` 测试、字算术与返回形状，而未观察运行时边保持报告为未知；
7. 未使用战斗员类型编码器保持可复现，而不被当作必需玩家面向域身份；
8. 战斗/名册生命周期、持久性、UI、呈现、平衡与现代化由其自身所有者测试或记录为刻意偏差。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 选择器路线、56 字节步长、31 获取器、53 包装器 | **已确认静态** | `sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)） | 调用方可视无效选择器行为与完整运行时结果 |
| 七个夹断辅助与 25 个直接包装器站点 | **已确认静态** | `sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)） | 间接可达性与零站点辅助使用 |
| 九个受限夹断操作 | **已确认运行时** | `sf2-stat-clamp-boundaries-v1`（[`stat-clamp-boundaries-v1.json`](../../../../tests/fixtures/h3/stat-clamp-boundaries-v1.json)） | Decrease-word、Long 辅助、decrease-current-ATT、其他包装器/选择器 |
| 双选择器距离操作与两个直接调用方 | **已确认静态** | `sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)） | 运行时边、坐标含义、调用方可视使用 |
| 源标记未使用战斗员类型编码 | **已确认静态** | `sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)） | 自然可达性与玩法含义 |
| 成员、战斗生命周期、持久性、UI、呈现、平衡 | **独立所有者 / 未知** | 相邻合同与未来 H3/综合工作 | 不得从访问 API 推断更高层含义 |

## 复现

```powershell
uv run sf2 h2 common-stats
uv run sf2 h3 stat-clamps
uv run sf2 design-contracts test
uv run sf2 verify
```
