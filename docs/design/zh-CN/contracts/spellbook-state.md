# 法术书状态合同

- 状态：**已确认静态法术书查找与学习边界**
- 证据日期：2026-08-08
- 范围：存储习得法术条目、所选槽与习得计数访问、定义未命中回退与 `LearnSpell` 结果/变更顺序的实现无关重构，不导入运行时获取、持久性、UI、战斗解决、呈现或平衡含义

> 本文件是 [`spellbook-state.md`](../../contracts/spellbook-state.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同从 `spellstats.asm` 中的源码形状辅助开始。它定义一个受限可变法术书服务与一个定义查找交接。它不定义角色何时获得法术、玩家如何看到它或法术在战斗中如何表现。

- **已确认 fixture 拥有事实**：定义查找未命中回退到首定义条目；`LearnSpell` 静态成功返回 `0`、同等级或更高级别已知返回 `1`、无剩余空间返回 `2`；更高传入等级替换已知条目。
- **已确认直接源码审查**：`GetSpellDefinitionAddress` 把原始传入 `d1.b` 与定义条目身份字节比较，且自身不应用 `SPELLENTRY_MASK_INDEX`；`GetSpellAndNumberOfSpells` 返回调用方所选槽的条目，同时独立扫描每个源槽计算习得计数；`LearnSpell` 在任何空槽搜索前完成已知基础扫描。
- **未知**：数字掩码、移位、槽计数、Nothing 与定义计数常量；受限源码形式之外的畸形或重复存储条目行为；无效战斗员、槽或定义输入；运行时调用方可达性与变更结果；事务/回滚行为；存档/读档持久性；获取策略；UI、名称/本地化、MP 使用、目标、范围、效果、battle-scene 呈现、音频与平衡意图。

[spell-definition-data 合同](../../contracts/spell-definition-data.md) 拥有不可变法术身份与记录打包。[Level-up](../../contracts/level-up.md) 拥有其已接受调用方路线，[spell resolution](../../contracts/spell-resolution.md) 拥有战斗执行，[combatant state access](../../contracts/combatant-state-access.md) 拥有通用战斗员条目访问。[new-game initialization 合同](../../contracts/new-game-state-initialization.md) 只拥有其已接受符号空槽初始化事实。本合同不借用那些相邻合同的 research-index 关联。

## 证据所有者与源审计

`sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)）是本合同消费的唯一可执行所有者。其验证器是[`stats.py`](../../../../src/sf2tool/h2/stats.py)，其带来源解释是[Common Stats 与 Inventory Services](../../../research/common-stats.md)。

fixture 在代表 `GetSpellName` 十进制地址 `37318` 绑定 `spellstats.asm` 服务面，并拥有恰好五个 `expected.statsFacts.spells` 事实。那五个事实不构成运行时观察，也不建立每个辅助的完整 API。

对固定上游 commit `c834c652b6862bc5679fd7f69a38a7093206efc6` 的独立只读审计审查了 `code/common/stats/spellstats.asm`，从 `GetSpellName` 到 `GetSpellCost`。该审计提供下文受限辅助时间线。它不把源注释、符号常量或相邻资源查找提升为玩家面向语义。

活跃 common-maps Issue #80、排队 tech-graphics Issue #81 与所有聚合 fixture 被刻意排除。法术定义记录、战斗员设置器、新游戏、升级、战斗动作、菜单、存档、文本与 gameflow 记录在本合同 research-index 关联边界之外。唯一未来关联是 `stats.spell-stats`。

## 存储条目边界

源码形状法术书存储有序原始条目。辅助可以通过符号掩码/移位操作从条目派生基础法术身份与等级码，但本合同不导入数字位位置或数字 Nothing 编码。

掩蔽是辅助特定的，而非通用前置条件：

- `GetSpellName` 在其名称查找前应用符号基础索引掩码；
- `GetSpellAndNumberOfSpells` 在决定每个扫描槽是否为 Nothing 时应用掩码；
- `LearnSpell` 应用基础索引掩码与等级移位以比较存储与传入条目；
- `GetSpellDefinitionAddress` 在比较前**不**应用基础索引掩码。

重制不得通过一个共享掩蔽步骤归一化每个传入法术值。特别是，即使另一辅助接受相同打包条目类型，定义查找也保留原始传入 `d1.b` 比较边界。

源码包含符号槽计数器与符号条目常量。其数字值在本合同之外。逻辑模型必须保留每个导入槽，而不硬编码从另一文档推断的基数。

## 定义查找边界

**已确认静态：** `GetSpellDefinitionAddress` 把原始传入 `d1.b` 与每个源定义条目的身份字节比较。如果没有比较在源受限扫描中匹配，它返回首定义条目。

例程本身不掩蔽等级位或以其他方式归一化 `d1.b`。把掩蔽名称或法术书计数行为导入该查找会改变已接受源边界。

首条目回退是地址选择规则，不是条目零语义安全、等价于 Nothing、对玩家可见或适合畸形重制数据的证明。定义表基数、重复身份、损坏表与调用方可视错误策略保持 **未知** 或显式现代化决定。

## 所选槽与习得计数

`GetSpellAndNumberOfSpells` 有两个独立输出：

1. 它读取并返回调用方所选槽的原始条目；
2. 它独立扫描每个源槽、把每个扫描条目掩蔽到其基础身份，并计数掩蔽身份不是符号 Nothing 值的条目。

所选返回条目不一定是首个习得法术。本合同刻意不重复上游注释的“first spell entry”措辞，因为指令路径索引调用方提供的槽。

习得计数不重排、压缩或变更存储条目。数字槽基数、无效所选槽行为、重复基础身份与“known”的运行时含义在已接受静态合同之外。

## `LearnSpell` 时间线

源审查的变更顺序是：

1. 保留传入原始条目，同时派生其符号基础身份与等级码；
2. 在搜索空槽前扫描既有槽寻找该基础身份；
3. 找到匹配基础身份时，把存储等级与传入等级比较；
4. 已存储同等级或更高级别时，不执行空槽搜索即返回结果 `1`；
5. 传入等级更高时，替换该已知条目并返回结果 `0`；
6. 只在完整已知基础扫描未找到匹配时，扫描符号 Nothing 条目；
7. 把传入原始条目写入源所选空槽并返回 `0`，或空槽扫描未找到空间时返回 `2`。

早期结果-`1` 路径是权威的：可用空槽不允许已已知基础身份的重复更低、相等或非升级副本。空槽搜索只在未找到已知基础身份后可达。

直接源审查观察到已知基础扫描从源端向起始、后续空槽扫描从起始向端。保真适配器保留该顺序，但本合同不把畸形重复基础行为提升为受支持玩法规则。运行时部分写入、中断、并发变更与调用方可视事务保持 **未知**。

数字结果 `0`、`1` 与 `2` 是 fixture 拥有静态返回身份。它们不证明任何调用方如何呈现成功/失败、是否显示消息，或包围状态是否持久。

## 相邻辅助分离

`GetSpellName` 与 `GetSpellCost` 保持审计文件中的源操作身份，但不扩展本可变状态合同：

- 名称辅助的掩蔽查找不建立玩家面向文本、本地化或资源许可；
- 消耗辅助通过定义查找委托并读取源消耗字段，但 MP 可负担性、扣除时序、敌人规则、UI 显示与平衡归其他所有者。

同样，`LearnSpell` 不定义法术为何被授予。升级、脚本奖励、调试路由或其他调用方必须保留其自身可达性与顺序证据。

## 跨系统分离

法术书存储不是完整法术系统：

- 不可变名称/元素/定义表仍归法术定义数据；
- 战斗员条目布局与通用字段访问仍归 combatant-state access；
- new-game 拥有其符号空初始化，但不拥有后续学习结果；
- level-up 拥有已接受增益/prowess/法术调用方时间线，而非通用存储服务；
- 目标几何、MP 事务、状态/效果分发、战斗解决、battle-scene 呈现与 AI 法术选择需要其自身证据；
- 存档/读档、UI、本地化、音频、可访问性与平衡保持刻意设计或独立运行时边界。

## 实现无关状态模型

```text
StoredSpellEntry
  rawEntry
  derivedBaseIdentity: apply symbolic base mask only in helpers that do so
  derivedLevelCode: apply symbolic level shift only in helpers that do so

SpellbookState
  orderedSlots: source-bounded symbolic cardinality

lookupDefinition(rawQueryByte)
  compare rawQueryByte directly with definition identity bytes
  masking: none
  onMiss: firstDefinitionEntry

getSelectedEntryAndLearnedCount(selectedSlot)
  selectedEntry: orderedSlots[selectedSlot]
  learnedCount:
    count each slot whose symbolically masked base identity is not Nothing

learnSpell(incomingRawEntry)
  scan known base identities before empty slots
  if sameOrHigherKnown:
    return 1
  if knownLevelIsLowerThanIncoming:
    replace known entry with incomingRawEntry
    return 0
  if emptySlotExists:
    write incomingRawEntry to source-selected empty slot
    return 0
  return 2
```

这是逻辑 parity 模型，不是必需引擎内存布局。重制可以使用类型化条目、验证集合、结果枚举或事务。其保真适配器必须保留原始对掩蔽辅助边界、所选槽/计数分离、已知先于空时间线、替换规则与三个已接受结果身份。

## 原版保真与现代化

原版保真模式保留五个 fixture 拥有事实、直接审查的辅助顺序与代表服务身份/地址。它报告运行时、畸形输入、持久性与呈现问题，而非把静态返回码当作完整玩家体验。

重制可以拒绝畸形条目、强制唯一基础身份、暴露类型化失败原因、调整法术书大小或使用不同定义未命中策略。那些是显式产品决定。原版保真适配器必须仍复现已接受源边界或记录偏差。

公开 parity fixture 需要结构元数据、符号身份与合成条目值；它们不需要版权法术名、描述、图形、音频或对话。

## H4 验收门

未来重制法术书适配器只在以下情况通过本合同：

1. 原始存储条目保持无损，而基础身份与等级码只在已接受源路径应用对应符号操作的辅助中派生；
2. 定义查找比较原始 `d1.b` 并在未命中时回退到首定义条目，而不从另一辅助导入 `SPELLENTRY_MASK_INDEX`；
3. 所选槽返回与全槽习得计数保持独立输出，且所选条目不被误标为首个习得法术；
4. `LearnSpell` 在空槽搜索前完成已知基础扫描、对同等级或更高级别已知提前返回 `1`、以结果 `0` 替换更低已知等级、只在无基础匹配时以结果 `0` 写入空槽，并在无剩余空间时返回 `2`；
5. 数字常量/基数、畸形输入、运行时可达性、持久性、获取策略、UI、本地化、MP/目标/效果解决、呈现、音频与平衡保持分别测试或显式 **未知**；
6. 相邻定义、战斗员、新游戏、升级、解决、存档与呈现合同保持独立可测试，而非被折叠进本状态层；
7. 公开 fixture 使用结构元数据与合成值，而非版权内容。

## 证据矩阵

| 合同区域 | 证据标签 | 所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 定义未命中回退；结果 `0`/`1`/`2`；更高级别替换 | **已确认静态** | `sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)） | 运行时调用方、呈现、持久性 |
| 无辅助导入掩蔽的原始定义比较 | **已确认静态源审查** | 固定 `spellstats.asm:GetSpellDefinitionAddress` | 无效/损坏表行为与调用方策略 |
| 调用方所选条目加独立全槽计数 | **已确认静态源审查** | 固定 `spellstats.asm:GetSpellAndNumberOfSpells` | 数字槽计数与无效选择 |
| 已知先于空学习时间线 | **已确认静态源审查** | 固定 `spellstats.asm:LearnSpell` | 运行时变更/事务边界与畸形重复 |
| 获取、存档/读档、UI、本地化、解决、呈现、音频、平衡 | **独立所有者 / 未知** | 相邻合同与未来运行时/综合工作 | 不得推断完整法术体验 |

## 复现

```powershell
uv run sf2 h2 common-stats
uv run sf2 design-contracts test
uv run sf2 verify
```
