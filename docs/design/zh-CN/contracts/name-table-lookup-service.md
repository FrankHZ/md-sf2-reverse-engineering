# 名称表查找服务合同

- 状态：**草稿证据绑定合同**
- 原版保真：**已确认静态**（针对下文描述的受限条目身份、长度前缀遍历、输出形状与直接调用方清单）
- 现代化：**允许** 在私有导入等价适配器背后使用引擎原生索引资源
- 未知：畸形或越界索引、运行时准入、所述字保留之外的调用方可视寄存器/CCR 依赖、编码、本地化、渲染、时序与玩家可见含义

> 本文件是 [`name-table-lookup-service.md`](../../contracts/name-table-lookup-service.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 目的

本合同定义已接受 `findname.asm` 证据支持的最小实现无关查找服务。它保留原版长度前缀行遍历与 `GetClassName` 前端，而不拥有任何职业、物品、法术、己方或敌人名称内容。

源服务返回指向调用方所选私有表的指针与所选行的长度。重制可以改为返回引擎原生资源引用与长度，前提是私有兼容适配器能复现已接受索引结果。原版表字节、地址、指针与字符串不是可分发合同载荷。

## 判断边界

**已确认静态：** 已接受 common-stats fixture 把 `GetClassName` 绑定到 ROM 地址 `0x8970`（`35184`）并识别 `findname.asm` 为其源。H1 列表把六字节前端放在 `0x8970..0x8976`，紧接 `FindName` 在 `0x8976`；注释源区间在 `0x898E` 结束。`GetClassName` 把 `p_table_ClassNames` 加载进 `a0` 并 fall-through 进 `FindName`。

在调用方拥有的有效表与 `0..rowCount-1` 中已准入索引内，`FindName` 把每行视为一个无符号长度字节后接那么多载荷字节。它恰好跳过请求数量的前一行、在所选行长字节后立即返回 `a0`，并清除然后把该字节加载进 `d7.w`。源恰好保存与恢复 `d0.w`，经一个平衡双字节栈槽。

已接受完整 common-stats 源清单恰好有三个直接 `FindName` 调用站点：`combatantstats_1.asm`、`itemstats.asm` 与 `spellstats.asm` 各一个。`GetClassName` fallthrough 是不同前端，不计为第四个直接调用。

**推断：** 无。“class”“item”“spell”与“name”等源身份描述原版存储/调用方词汇。它们不建立玩家面向含义、本地化策略或必需公开 API 名。

**未知或排除：** 负、越界、回绕或其他畸形索引；截断行；缺失长度字节；无效指针；跨地址空间行为；调用方验证；运行时可达性与结果使用；调用方可视 `d1`、CCR 或所述 `d0.w` 保存/恢复之外的寄存器行为；文本编码；字形选择；本地化；渲染；窗口布局；时序；持久性；以及替换内容策略。

## 证据合同

本合同只消费以下来自[`sf2-common-stats-static-v1`](../../../../tests/fixtures/h2/common-stats-static-v1.json)的公开面：

- `function.classNameAddress`；
- `expected.representativeSymbols["findname.asm"]`；
- `upstreamCommit` 与 `romSha256` 溯源。

受限时间线直接在固定[`findname.asm`](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/stats/findname.asm)中审查。所属[common-stats 研究](../../../research/common-stats.md)、可执行[`stats.py`](../../../../src/sf2tool/h2/stats.py)与提取[`manifest`](../../../../manifests/extractions/common-stats-static.json) 保留完整源清单、已接受输出摘要与溯源。生成清单建立三个直接调用方出现而不发布任何名称表载荷。

本合同**不**消费任何 `expected.statsFacts` 子树。特别是，它不因共享聚合 fixture 而消费战斗员获取器/变更/夹断/距离行为、队伍、标志、车队、Deals、法术、新游戏、战斗员类型或清单事实。

已接受证据在已接受 ROM 身份下解析代表 H1 条目与固定源时间线。它不建立完整函数体的逐字节 H1/ROM 指令一致性。未来私有指令字节比较会是更强证据，不是当前 H4 要求。

### 精确 research-index 分母

已接受 fixture 直接链接十二条研究记录。本合同恰好改变一条未来语义关联：

| 记录 | 本合同后的设计所有权 |
| --- | --- |
| `stats.names` | 本合同；注册前当前未关联 |
| `stats.caravan` | 不变：`caravan-and-deals-state` |
| `stats.deals` | 不变：`caravan-and-deals-state` |
| `stats.combatant-setters` | 不变：`combatant-state-access` |
| `stats.combatant-type` | 不变：`combatant-state-access` |
| `stats.flags` | 不变：`global-flag-state` |
| `stats.new-game` | 不变：`new-game-state-initialization` |
| `stats.party` | 不变：`party-membership-state` |
| `stats.spell-stats` | 不变：`spellbook-state` |
| `stats.item-inventory` | 保持未关联且在本合同之外 |
| `stats.item-stats` | 保持未关联且在本合同之外 |
| `stats.unused-null` | 保持未关联且在本合同之外 |

共享 common-stats fixture 不把任何兄弟事实、证据子树或设计关联转移给本服务。

## 原版静态服务

### 前端与条目身份

源区间包含两个不同身份：

| 身份 | H1 地址 | 已接受角色 |
| --- | ---: | --- |
| `GetClassName` | `0x8970`（`35184`） | 加载私有职业名表指针并 fall through |
| `FindName` | `0x8976`（`35190`） | 遍历调用方所选长度前缀表 |

`GetClassName` 在其指针加载与 `FindName` 之间没有独立返回。该 fallthrough 是源控制流事实。它不使本服务成为职业表、职业身份域或职业显示内容的所有者。

### 已准入查找域

公共结果合同刻意限定到：

- 有效调用方拥有的有序表；
- 行私下编码为一个长度字节加恰好那么多载荷字节；
- `0..rowCount-1` 中的非负索引；
- 覆盖每个跳过行与所选行的非回绕可读存储。

源不包含局部行计数参数或边界检查。该域之外的输入不被归一化为现代 API 保证。重制可以拒绝它们，但该拒绝是文档化现代安全策略，而非已确认原版行为。

### 遍历时间线

对已准入索引 `n`，源码形状时间线是：

1. 在一个双字节栈槽保存 `d0.w`；
2. 从 `d1.w` 减一；
3. 结果负时跳过行扫描循环，选择行零；
4. 否则清除 `d0.w`、把当前行长字节读进 `d0.b`、把 `a0` 推进过长字节、把零扩展长度加到 `a0`，并用 `dbf` 重复直到恰好跳过 `n` 个前一行；
5. 清除 `d7.w`、把所选行长字节读进 `d7.b` 并把 `a0` 推进一次；
6. 恢复 `d0.w` 并返回。

由此抽象关系是：

| 已准入输入 | 跳过行 | 逻辑输出 |
| --- | --- | --- |
| 索引 `0` | `0` | 首行载荷引用与首行长度 |
| 索引 `1` | `1` | 第二行载荷引用与第二行长度 |
| 索引 `n` | `n` | 第 `n` 行载荷引用与第 `n` 行长度 |

源在使用或返回前零扩展每个长度字节。这是存储与查找事实，不是字符编码、字形计数、显示宽度或本地化长度的断言。

### 寄存器与栈边界

源显式保存与恢复低字 `d0.w`；它不在栈上保存完整 longword 副本。因此本合同精确陈述源操作，不把它提升为全寄存器 ABI 声称。栈递减与匹配字恢复在已准入返回路径平衡。

`a0` 与 `d7.w` 是输出。`d1.w` 是遍历状态而非保留输入。CCR 状态与任何调用方对未列寄存器部分的依赖保持合同之外。

## 直接调用方分离

完整已接受 common-stats 源清单包含这三个直接 `FindName` 站点：

| 调用方源 | 调用方拥有的准备 | 独立设计所有者 |
| --- | --- | --- |
| `combatantstats_1.asm` | 敌人选择与敌人名表选择 | [`combatant-state-access`](../../contracts/combatant-state-access.md)与[`enemy-definition-data`](../../contracts/enemy-definition-data.md) |
| `itemstats.asm` | 物品条目掩蔽与物品名表选择 | [`item-definition-data`](../../contracts/item-definition-data.md)；更广物品服务行为保持独立 |
| `spellstats.asm` | 法术条目掩蔽与法术名表选择 | [`spellbook-state`](../../contracts/spellbook-state.md)与[`spell-definition-data`](../../contracts/spell-definition-data.md) |

职业表前端消费[`ally-definition-data`](../../contracts/ally-definition-data.md)拥有的表。本合同不复制任何调用方的掩蔽、选择器、表基数、内容保真、运行时准入或显示行为。

## 实现无关模型

合规私有导入可以使用该逻辑形状：

```text
NameTableLookupEvidence
  identity
    getClassNameSymbol
    getClassNameH1Address = 0x8970
    findNameSymbol
    findNameH1Address = 0x8976
    sourcePath
    pinnedUpstreamCommit
    acceptedRomSha256Provenance

  privateTable
    orderedRows[]
      privatePayloadBytes
      byteLength

  admittedLookup
    validIndexRange = 0..rowCount-1
    resultPayloadRef
    resultByteLength

  sourceBoundary
    classFrontendFallsThrough
    d0WordSaveRestore
    balancedWordStackSlot
    directCallerOccurrences[3]
```

这是证据/导入模型，不是必需引擎类布局。完整表内容、原版字符串、原始源体、精确指针值、完整地址、指令字节与私有 ROM 摘录保持私有验证输入。受限符号、两个索引条目地址、源路径、行格式规则、直接调用方摘要、hash 与溯源是公开元数据。

在 fixture 已接受 ROM 溯源下验证固定源时间线与 H1 身份后，合规重制可以使用数组、本地化资源 ID、不可变字符串表或类型化查找服务。它不需要复现 Mega Drive 地址、大端指针存储、长度前缀运行时内存、`dbf`、寄存器分配或原版栈操作。

## 公开与私有投影

公开合同可以保留：

- fixture ID、源路径、源符号与两个受限 H1 条目地址；
- 长度字节加载荷行规则与已准入有效索引域；
- 抽象索引到载荷引用与字节长度关系；
- 三个调用方源身份及其分离边界；
- 上游修订、已接受 ROM 身份/hash、输出摘要与复现命令；
- 不含原版名称的项目编写合成查找用例。

公开形式不得暴露原版职业、物品、法术、己方或敌人名称；完整名称表字节；私有行 hash；原版指针值；原始源或 ROM 体；模拟器内存；或本地化/捕获文本。原版兼容内容可以私有导入与验证，而可分发重制使用替换或单独清除文本资源。

## H4 验收面

未来适配器满足本合同时：

1. 已接受 fixture 身份、源路径、上游 commit、ROM SHA 溯源、`GetClassName` 地址与源局部 `FindName` 身份保持可追溯；
2. 私有导入器在发布原版载荷的前提下保留有序长度前缀行边界；
3. 有效索引 `n` 恰好选择第 `n` 行、返回其载荷引用而非长度字节，并返回该行零扩展字节长度；
4. 项目编写合成用例覆盖索引零、索引一与较晚有效索引，包括不同长度行；
5. 源兼容诊断保留 `GetClassName` 指针加载/fallthrough 身份与精确 `d0.w` 保存/恢复加平衡字栈边界，而不声称全寄存器或 CCR 保留；
6. 三个直接调用方出现保持可追溯，而其掩码、选择器、表内容与运行时含义保持其独立所有者；
7. 引擎原生实现可以返回类型化资源引用，且不需要复现原版循环、指针、地址空间、字节顺序、寄存器或内存布局；
8. 畸形/越界行为、编码、本地化、渲染、时序与呈现被拒绝为合同之外或由独立证据/设计策略覆盖；
9. 公开报告保持仅元数据并尊重私有/版权边界。

H4 不得要求公开 fixture 中有原版字符串或表。它用合成行测试已接受关系，并在原版兼容输入可用时用私有一致性适配器。

## 跨系统分离

- [`ally-definition-data`](../../contracts/ally-definition-data.md)、[`item-definition-data`](../../contracts/item-definition-data.md)、[`spell-definition-data`](../../contracts/spell-definition-data.md)与[`enemy-definition-data`](../../contracts/enemy-definition-data.md) 保留规范表身份、顺序、基数与私有内容。
- [`combatant-state-access`](../../contracts/combatant-state-access.md) 保留己方/敌人名选择及其受限 `GetCombatantName` 行为。
- [`spellbook-state`](../../contracts/spellbook-state.md) 保留法术条目掩蔽与法术查找调用方时间线。物品条目掩蔽与物品栏/装备行为保持本合同之外。
- [`text-and-font-system`](../../contracts/text-and-font-system.md)与[`dialogue-system`](../../contracts/dialogue-system.md) 保留编码、字形、文本资源、渲染、对话替换、窗口、时序与玩家可见呈现。
- 聚合 common-stats fixture 中的队伍、标志、车队、Deals、战斗员变更、新游戏、清单与 unused-null 兄弟保持其当前所有者或未关联状态。

## 证据矩阵

| 合同区域 | 证据标签 | 所有者 | 剩余边界 |
| --- | --- | --- | --- |
| `GetClassName` 身份/地址与 `findname.asm` 代表源 | **已确认静态** | `sf2-common-stats-static-v1` | 完整函数体字节一致性 |
| `GetClassName` 指针加载/fallthrough 与 `FindName` 遍历/输出时间线 | **已确认静态 source/H1** | 固定 `findname.asm`、H1 列表与 common-stats 所有者 | 畸形索引、运行时调用方依赖 |
| 恰好三个直接 `FindName` 调用方出现 | **已确认静态清单** | common-stats 生成清单与已接受摘要 | 间接/运行时可达性 |
| 职业/物品/法术/敌人表内容与调用方特定选择 | **独立所有者** | 上述定义/状态合同 | 替换/本地化策略 |
| 编码、字形、窗口、对话、可见长度、呈现 | **未知 / 独立所有者** | 文本/字体、对话与呈现合同 | 运行时与玩家可见结果 |

## 复现

```powershell
uv run sf2 h2 common-stats
uv run sf2 design-contracts test
uv run sf2 verify
```

生成清单保留在忽略的 `local/derived/` 下。没有原版名称表、字符串载荷、源体转储、ROM 摘录、模拟器状态或其他私有/生成工件属于公开合同。
