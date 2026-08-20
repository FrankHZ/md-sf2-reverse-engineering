# 新游戏状态初始化合同

- 状态：**已确认静态 NewGame 阶段与受限状态初始化顺序**
- 证据日期：2026-08-08
- 范围：七个已接受 `NewGame` 初始化事实的实现无关重构，不导入冷启动路由、数字内容常量、运行时结果、持久性、UI、呈现或平衡含义

> 本文件是 [`new-game-state-initialization.md`](../../contracts/new-game-state-initialization.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同从源码形状的 `NewGame` 例程开始。它不建立玩家如何到达该例程、平台启动或标题流中其前是什么，或其变更何时变得持久或可见。

- **已确认**：游戏设置在己方条目之前初始化；每个原版己方条目都被初始化；起始金币在 Bowie 加入前分配；己方法术槽接收所有者的 Nothing/空状态；每个己方的职业数据在初始属性之前加载，后者先于派生属性刷新；设置清除标志、Deals 与车队；默认消息速度设置为 `2`。
- **推断**：无。故事意图与调用方可视含义刻意不从静态初始化序列推断。
- **未知**：数字起始金币与空法术常量；己方基数、身份内容与迭代顺序；已接受设置清除目标之间的顺序；七个已接受事实之外的设置字段；自然/运行时调用方可达性；调用方可视返回与部分失败行为；冷启动、系统初始化、标题/开场路由与新游戏 UI/取消；存档/读档持久性；呈现、音频、本地化与平衡意图。

[ally-definition-data 合同](../../contracts/ally-definition-data.md) 拥有己方身份、定义与成长数据，而不提供本例程的运行时含义。[combatant-state-access 合同](../../contracts/combatant-state-access.md) 拥有低级战斗员条目访问面，而非新游戏生命周期。[Party-roster state](../../contracts/party-roster-state.md)、[global flags](../../contracts/global-flag-state.md)与[Caravan/Deals state](../../contracts/caravan-and-deals-state.md) 拥有各自状态边界；本合同只记录已接受初始化交接。

## 证据所有者与源审计

`sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)）是本合同消费的唯一可执行所有者。其验证器是[`stats.py`](../../../../src/sf2tool/h2/stats.py)，其带来源解释是[Common Stats 与 Inventory Services](../../../research/common-stats.md)。

fixture 绑定 `NewGame`，其十进制地址为 `38710`，并拥有恰好七个 `expected.statsFacts.newGame` 事实。对固定源的只读审计确认相同受限形状：设置调用先于己方循环；起始金币分配先于 Bowie 加入；己方初始化器包含已接受空法术赋值与已接受职业/初始/派生属性阶段顺序；设置例程包含三个已接受清除目标加消息速度 `2`。

源码包含额外常量、写入与低级循环细节。它们不由本合同提升，因为可执行所有者不把它们暴露为已接受 `newGame` 事实。特别是，此处不使用源检查来发明己方计数、数字金币值、数字空法术编码、内部清除顺序、完整四阶段顶层顺序或完整设置重置语料。

common-scripting 与 battle-functions 聚合被刻意排除。Gameflow 启动与主循环记录、兄弟队伍/标志/车队/Deals 记录、定义记录、服务/菜单记录与存档记录也在本合同 research-index 关联边界之外。

## 顶层偏序

**已确认静态：** `NewGame` 有两个已接受顺序约束加一个覆盖事实：

1. 游戏设置先于己方条目初始化；
2. 每个原版己方条目都被初始化；
3. 符号起始金币值在 Bowie 加入前分配。

fixture 不在全部己方初始化完成与起始金币分配之间添加顺序边。它也不暴露数字己方计数、循环方向、金币常量、调用方或最终运行时观察。“Join Bowie”保留源操作身份；它不建立队伍容量行为、故事可用性、可见名册构成或玩家选择。

| 阶段 | 已接受状态事实 | 刻意边界 |
| --- | --- | --- |
| 设置 | 在己方初始化之前完成 | 内部清除/写入顺序与未列字段 **未知** |
| 己方条目 | 每个原版己方条目都被初始化 | 计数、内容与迭代顺序 **未知** |
| 起始资源 | 金币在 Bowie 加入前分配 | 数字金币价值与经济意图 **未知** |
| 起始成员 | Bowie 加入跟随金币分配 | 运行时队伍结果、容量与呈现 **未知** |

## 逐己方初始化边界

**已确认静态：** 每个己方初始化包括所有者的 Nothing/空法术槽状态与以下有序属性阶段：

1. 加载己方职业数据；
2. 初始化己方属性；
3. 刷新派生战斗员属性。

已接受顺序防止重制导入器把职业派生字段、初始属性与派生属性刷新压平成无序写入。它不定义完整战斗员记录、法术槽数量或打包、职业/属性公式、成长曲线、装备效果或任何字段的最终运行时值。

[spell-definition-data 合同](../../contracts/spell-definition-data.md) 拥有固定法术身份与定义，而非可变法术槽存储。[Level-up](../../contracts/level-up.md) 拥有其已接受成长与法术学习路线。两个合同都不向本初始化合同提供数字空槽编码。

## 设置初始化边界

**已确认静态：** 设置阶段清除这三个逻辑存储：

- 全局标志；
- Deals 计数；
- 车队存储。

同一已接受设置边界把默认消息速度设置为 `2`。fixture 不建立三个清除目标之间的内部顺序，或那些清除与消息速度写入之间的顺序。它也不暴露源例程触及的每个设置或内存字段。

这些事实定义初始化交接，而非存储的完整语义。标志寻址、车队归一化/压缩与 Deals 打包计数变更仍归其自身合同。消息速度 `2` 是已接受存储默认值，不是可见文本节奏、可访问性策略、输入时序或本地化行为的证据。

## 跨系统分离

NewGame 例程不是完整新游戏体验：

- 冷启动、系统配置、区域检查、基础资源、标题/开场流与顶层循环路由仍归 gameflow 与技术所有者；
- 己方定义与战斗员访问描述数据/状态形状，而非运行时初始化成功；
- 名册成员在加入请求后拥有队伍状态行为；
- 物品、法术、标志、车队与 Deals 所有者保留其独立存储语义；
- 存档证据不证明新初始化状态跨每个进程或断电路径持久；
- UI、确认/取消、音频、呈现、本地化与起始平衡策略不从该静态调用顺序派生。

[故事推进综合](../../synthesis/story-progression.md) 可以把已接受交接放入更大解释，但不得用本合同作为自然运行时可达性或玩家可见时间线的证明。

## 实现无关状态模型

```text
NewGameInitializationPlan
  operations:
    settingsStage
    allyInitializationStage
    startingGoldAssignment
    startingMemberJoin: Bowie
  acceptedOrderEdges:
    settingsStage -> allyInitializationStage
    startingGoldAssignment -> startingMemberJoin
  allyInitializationCoverage: everyOriginalAllyEntry

SettingsStage
  clearGlobalFlags
  clearDeals
  clearCaravan
  defaultMessageSpeed: 2

AllyInitializationStage
  coverage: everyOriginalAllyEntry
  perEntry:
    initializeSpellSlots: Nothing
    orderedStatStages:
      - loadClassData
      - initializeStats
      - refreshDerivedStats
    spellInitRelativeToStatStages: notContracted
```

这是逻辑一致性模型，不是必需引擎内存布局。模型刻意省略己方计数/顺序、原始内容、数字起始金币、数字 Nothing 编码、未接受设置字段、返回值与持久性行为。

现代引擎可以构建不可变默认值、批量实体创建、使用事务或暴露类型化初始化结果。其兼容适配器仍必须复现已接受顶层偏序、逐己方属性阶段顺序、三个清除目标与消息速度默认值。

## 原版保真与现代化

原版保真模式保留七个已接受静态事实与代表 `NewGame` 身份/地址。它报告调用方、运行时与持久性问题，而非把源调用顺序当作完整玩家面向新游戏流程。

重制可以选择不同起始资源、名册构成、默认设置、可访问性默认值、错误处理或存档创建策略。除非原版保真适配器复现已接受边界并记录刻意偏差，否则那些是显式产品决定。

原版名称与其他版权内容对公开一致性 fixture 不必要。公开测试应使用结构元数据、源标识符与合成值。

## H4 验收门

未来重制 NewGame 适配器只在以下情况通过本合同：

1. 设置初始化先于己方条目初始化；
2. 每个导入的原版己方条目都接受初始化遍，而不硬编码未接受合同级计数或内容语料；
3. 每个己方把法术槽初始化为符号 Nothing 状态，并独立保留职业数据加载、初始属性构建与派生属性刷新顺序，而不发明这两个已接受事实之间的合同级相对顺序；
4. 起始金币分配先于 Bowie 加入，而不发明数字金币常量或从己方初始化到金币分配的未接受顺序边；
5. 设置清除标志、Deals 与车队并把消息速度设置为 `2`，而不发明内部清除/写入顺序或完整设置语料；
6. gameflow 可达性、调用方/返回行为、运行时结果、持久性、UI、呈现、本地化与平衡保持分别测试或显式 **未知**；
7. 公开 fixture 包含结构元数据与合成值，而非版权内容。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 设置先于全部己方初始化 | **已确认静态** | `sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)） | 调用方、己方基数/顺序、运行时结果 |
| 起始金币然后 Bowie 加入 | **已确认静态** | `sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)） | 数字金币、队伍结果、故事/玩家含义 |
| 空法术状态；独立职业 → 初始属性 → 派生刷新顺序 | **已确认静态** | `sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)） | 法术/属性阶段相对顺序、数字编码、公式、最终字段值 |
| 标志/Deals/车队清除与消息速度 `2` | **已确认静态** | `sf2-common-stats-static-v1`（[`common-stats-static-v1.json`](../../../../tests/fixtures/h2/common-stats-static-v1.json)） | 内部顺序、未列设置、可见/运行时效果 |
| 启动/标题/UI/存档/呈现/平衡语义 | **独立所有者 / 未知** | 相邻合同与未来运行时/综合工作 | 不得从静态初始化推断完整新游戏体验 |

## 复现

```powershell
uv run sf2 h2 common-stats
uv run sf2 design-contracts test
uv run sf2 verify
```
