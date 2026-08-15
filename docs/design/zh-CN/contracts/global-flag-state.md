# 全局标志状态合同

- 状态：**已确认静态标志寻址形状与受限标志 trap 清单**
- 证据日期：2026-08-08
- 范围：原版全局标志状态的实现无关存储与操作身份，不指定战役、持久性、调用方、呈现或平衡含义

> 本文件是 [`global-flag-state.md`](../../contracts/global-flag-state.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同定义最低已接受全局标志边界。它不定义单个标志的含义、故事或战斗路径何时读取它、它是否在存档/读档循环中存活，或玩家在其值变化时看到什么。

- **已确认**：标志索引经过已接受掩蔽步骤；每字节共享八个标志；位选择从位 7 开始；Check、Set 与 Clear 共享同一 `GetFlag` 解析路径；中断所有者报告四个标志 trap 包装器、在地址 `5888` 识别 `Trap4_CheckFlag`，并把 trap 1 到 4 分组为 Check、Set 与 Clear 操作的包装器。
- **推断**：无。高层意图刻意不从存储与包装器形状推断。
- **未知**：已接受掩蔽事实之外的精确可用标志域大小；单个标志的名称与含义；自然与运行时调用方可达性；存档/读档持久性；跨系统顺序；调用方可视结果或条件码使用；内联 trap 操作数解码；返回地址移动；UI 与呈现；调试路由；以及平衡或战役意图。

[存档系统合同](../../contracts/save-system.md) 拥有已接受存档结构与动作，而非全局标志持久性，除非证据所有者闭合该连接。[故事推进综合](../../synthesis/story-progression.md) 可以解释已接受消费者，但不得把该低级存储合同变成故事状态分类法。

## 证据所有者

`sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)）是标志寻址事实的专用 H2 所有者。其验证器是[`stats.py`](../../../../src/sf2tool/h2/stats.py)，其带来源解释是[Common Stats 与 Inventory Services](../../../research/common-stats.md)。本合同只消费所有者的 `flags` 事实：掩蔽索引、每字节八位、位 7 优先选择与共享 `GetFlag` 解析路径。

`sf2-tech-interrupts-static-v1`（[`tech-interrupts-static-v1.json`](../../../../tests/fixtures/h2/tech-interrupts-static-v1.json)）是受限 trap 清单的专用 H2 所有者。其验证器是[`interrupts.py`](../../../../src/sf2tool/h2/interrupts.py)，其带来源解释是[Technical Interrupts](../../../research/technical-interrupts.md)。本合同只消费 `flagTrapCount=4`、代表 `Trap4_CheckFlag` 身份/地址与所有者文章中 Check/Set/Clear 包装器分组。

common-scripting 与 battle-functions 聚合被刻意排除。其排队所有者修正确认既不是本合同的证据依赖也不是合并依赖。Map-script、战斗、菜单与调试消费者也保持在 research-index 关联边界之外。

## 标志引用解析

**已确认静态：** 已接受存储解析器在选择存储前对传入标志索引应用掩码。然后它寻址每字节八标志的域，并从位 7 开始位选择。Check、Set 与 Clear 都共享该解析器。

可执行所有者不暴露数字掩码值或完整标志存储基数。因此本合同要求归一化步骤与字节/位拓扑，而不发明最大标志 ID、字节数组长度或有效语义名集。

导入或兼容适配器必须保留调用方提供的索引与归一化存储引用。把它们折叠成一个未检查的应用布尔值会隐藏已接受掩码造成的别名，并使源面向诊断不可能。

| 解析器属性 | 已接受合同 | 刻意边界 |
| --- | --- | --- |
| 传入身份 | 保留原始标志索引 | 调用方溯源与语义名 **未知** |
| 归一化 | 寻址前应用已接受标志索引掩码 | 数字掩码与可用域大小 **未知** |
| 字节选择 | 八个标志位置共享一个字节 | 总字节数 **未知** |
| 位选择 | 在所选字节内从位 7 开始 | 不暗示更高层优先级或含义 |
| 操作复用 | Check、Set 与 Clear 共享 `GetFlag` 解析 | 调用方可视结果与条件语义 **未知** |

## 操作身份边界

**已确认静态：** Check、Set 与 Clear 是汇聚到同一引用解析路径的不同操作身份。保真面向表示必须分别保留请求操作种类与解析字节/位引用。

这不是运行时事务合同。已接受所有者不闭合交错、并发、中断、回滚、持久性或通知行为。它们也不证明调用方如何消费 Check 结果。现代实现可以提供类型化查询与变更 API，但不得静默地用单个切换替换三个原版面向身份，或给包装器层指定新调用方可视语义。

## 标志 Trap 清单边界

**已确认静态：** 中断所有者报告 `flagTrapCount=4`。它在十进制地址 `5888` 识别 `Trap4_CheckFlag`，并描述 trap 1 到 4 为标志 Check、Set 与 Clear 操作周围的包装器。这些只是清单与分组事实。

本合同不解码内联操作数、不规定任何 trap 如何改变已保存返回地址、不把每个 trap 号映射到一个精确操作、不定义返回值或条件码，也不声称运行时可达性。那些细节在专用静态或运行时所有者接受它们之前保持 **未知**。代表 Trap 4 身份不得泛化为完整四条目 ABI 表。

## 实现无关状态模型

```text
FlagStore
  storageBytes[]

FlagReference
  rawIndex
  normalizedIndex
  byteIndex
  bitMask

FlagOperation
  kind: check | set | clear
  reference: FlagReference

FlagTrapInventory
  wrapperCount: 4
  representativeIdentity: Trap4_CheckFlag
  representativeAddress: 5888
  groupedOperationKinds: check | set | clear
```

这是逻辑 parity 模型，不是必需引擎内存布局。`storageBytes` 刻意没有合同级长度，`normalizedIndex` 的推导刻意没有发明的数字掩码。独立原始与归一化索引保留已接受寻址边界，而不指定有效性或叙事含义。

trap 清单是元数据，不是可执行 ABI 规范。重制可以在没有机器 trap 的情况下实现普通玩法消费者，同时在原版保真适配器或诊断层保留该清单。

## 原版保真与现代化

原版保真模式保留掩蔽索引步骤、每字节八标志拓扑、位 7 优先选择、共享解析器、三个操作身份与受限 trap 清单。未知域与调用方行为保持可见，而非用猜的名字或生命周期填充。

现代引擎可以暴露具名标志、类型化战役事实、不可变查询、显式变更命令、事件日志与独立持久性策略。只有当它们到原始与归一化原版面向引用的映射显式且刻意偏差被分别记录时，那些才是合法设计选择。

原版标志名、对话与其他版权内容对本公开合同不必要。公开 fixture 只保留结构元数据、身份、计数与地址。

## H4 验收门

未来重制全局标志适配器只在以下情况通过本合同：

1. 调用方提供的标志索引与其归一化存储引用保持分别可观察；
2. 已接受先掩码后寻址步骤被保留，而不替换发明的域大小；
3. 八个标志位置共享每个字节，位选择从位 7 开始；
4. Check、Set 与 Clear 保持使用一个共享解析路径的不同操作身份；
5. trap 清单保留计数四、代表 `Trap4_CheckFlag` 身份/地址与受限 Check/Set/Clear 分组，而不发明完整 trap ABI；
6. 内联操作数解码、返回地址移动、调用方可视结果/条件行为、运行时可达性、持久性、呈现与战役含义保持分别测试或显式 **未知**；
7. 公开 parity 工件包含结构元数据而非原版版权内容。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 掩蔽标志索引、每字节八位存储、位 7 优先选择 | **已确认静态** | `sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)） | 数字掩码、总容量、语义标志域 |
| 共享 Check/Set/Clear 引用解析 | **已确认静态** | `sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)） | 运行时结果、顺序、调用方可视 Check 语义 |
| 四个标志 trap 与 `5888` 的代表 `Trap4_CheckFlag` | **已确认静态清单** | `sf2-tech-interrupts-static-v1`（[`tech-interrupts-static-v1.json`](../../../../tests/fixtures/h2/tech-interrupts-static-v1.json)） | 完整逐 trap 映射、内联 ABI、运行时可达性 |
| trap 1-4 围绕 Check/Set/Clear 分组 | **已确认所有者文章** | [Technical Interrupts](../../../research/technical-interrupts.md) 加 `sf2-tech-interrupts-static-v1` | 操作数解码、返回移动、结果与条件 |
| 战役消费者、持久性、UI、调试路由、平衡 | **独立所有者 / 未知** | 相邻合同与未来运行时/综合工作 | 不得从存储或包装器形状推断高层含义 |

## 复现

```powershell
uv run sf2 h2 common-stats
uv run sf2 h2 tech-interrupts
uv run sf2 design-contracts test
uv run sf2 verify
```
