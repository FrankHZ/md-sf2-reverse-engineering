# 战斗演出命令与呈现数据合同

- **已确认原版行为：** 21 命令场景解释器、场景初始化与选择器顺序、完整 32 槽法术配置/更新分发、208 个行动者动画序列，以及下文描述的背景、行动者精灵、武器、地面、法术、召唤、状态与过渡选择/加载器/呈现接缝。
- **未知原版行为：** 精确命令与帧时序、VInt/VDP 效果、调色板过渡外观、可见层组合与放置、512 字节召唤转移尾、每个选择器/索引组合的自然可达性，以及渲染帧一致性。
- 重制状态：实现无关 Phase 3 合同；尚未选择渲染器、动画图、资源格式、帧率策略或刻意兼容偏差。
- 证据日期：2026-08-08
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`battle-scene-presentation.md`](../../contracts/battle-scene-presentation.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同拥有有序战斗演出命令流与用于呈现该流的数据、选择器、加载器与分发表之间的静态边界。它定义：

1. 命令缓冲解释器与命令身份；
2. 场景初始化、行动者/武器/背景选择与层加载顺序；
3. 己方/敌人动画选择与序列数据；
4. 背景、行动者精灵、武器与地面的选择/加载器/呈现合同；
5. 法术动画配置/更新分发与战斗效果消费者/呈现接缝；
6. 未来兼容适配器必须暴露而不规定渲染器的状态。

它不拥有动作选择、目标选择、动作构建、伤害或法术算术、持久命令回放、战斗结果、输入/UI 策略或渲染时序。那些仍归[Battle AI decision](../../contracts/battle-ai-decision.md)、[battle action construction](../../contracts/battle-action-construction.md)、[交战解决](../../contracts/combat-resolution.md)、[法术解决](../../contracts/spell-resolution.md)与[battle control/lifecycle](../../contracts/battle-control-lifecycle.md)。

可执行所有者是：

- `sf2-battle-scene-engine-static-v1`，位于
  `tests/fixtures/h2/battle-scene-engine-static-v1.json`；
- `sf2-battle-scene-animations-static-v1`，位于
  `tests/fixtures/h2/battle-scene-animations-static-v1.json`；
- `sf2-battle-background-decode-v1`，位于
  `tests/fixtures/h2/battle-background-decode-v1.json`，此处保留为受限加载器/呈现见证，而[Battle Background Graphics Data](../../contracts/battle-background-graphics-data.md)拥有静态导入语料；
- `sf2-battle-sprite-decode-v1`，位于
  `tests/fixtures/h2/battle-sprite-decode-v1.json`，此处保留为受限行动者精灵加载器/呈现见证，而[Battle Sprite Graphics Data](../../contracts/battle-sprite-graphics-data.md)拥有静态导入语料；
- `sf2-battle-sprite-animation-static-v1`，位于
  `tests/fixtures/h2/battle-sprite-animation-static-v1.json`；
- `sf2-battle-weapon-ground-decode-v1`，位于
  `tests/fixtures/h2/battle-weapon-ground-decode-v1.json`，此处保留为受限武器/地面加载器与呈现见证，而[Battle Weapon 与 Ground Graphics Data](../../contracts/battle-weapon-ground-graphics-data.md)拥有静态导入语料；
- `sf2-battle-effect-graphics-decode-v1`，位于
  `tests/fixtures/h2/battle-effect-graphics-decode-v1.json`，此处保留为受限法术/召唤/状态/过渡消费者与呈现见证，而[Battle Effect Graphics Data](../../contracts/battle-effect-graphics-data.md)拥有静态导入语料。

研究所有者是[Battle Scene Engine](../../../research/battle-scene-engine.md)，压缩与加载器上下文在[Technical Graphics 与 Decompression Services](../../../research/technical-graphics.md)。

## 合同前证据审计

本切片在综合前检查了所属研究文章、全部七个 fixture 载荷与精确 ID、其 H2 验证器、research-index 绑定与聚焦复现命令。所有者对表大小、流计数、解码/转移单位、选择器顺序与静态呈现边界一致。

以下范围修正约束本合同：

- `sf2-battle-scene-replay-v1` 观察两条战斗演出命令后的持久 HP 与 EXP 变更。它是[交战解决](../../contracts/combat-resolution.md)的证据，不是渲染演出行为的证据，因此此处不注册。
- `TintScreen`、`VInt_UpdateBattlesceneGraphics` 与动画配置/更新标签等源码名标识静态分发与调用结构。它们不确认可见颜色、时序、组合或帧输出。
- [Battle Background Graphics Data](../../contracts/battle-background-graphics-data.md) 拥有 30 槽/27 容器静态导入语料、别名、前缀/调色板/流分区、压缩与解码身份、大小与私有往返证据。本合同只在背景选择、加载器、暂存、调色板操作与呈现接缝消费那些规范记录。
- [Battle Sprite Graphics Data](../../contracts/battle-sprite-graphics-data.md) 拥有独立的 32 槽己方与 54 槽敌人表、86 个源载荷身份、头/调色板/帧流分区、压缩与解码身份、聚合大小与私有往返证据。本合同只在行动者选择、属性/帧加载器、调色板操作、Stack 交接、DMA 请求与呈现接缝消费那些规范记录。
- [Battle Weapon 与 Ground Graphics Data](../../contracts/battle-weapon-ground-graphics-data.md) 拥有 23 槽武器表、42 个武器调色板条目、30 槽/27 头地面表与别名、33 个流身份、聚合大小与私有往返证据。本合同只在选择器、调色板、加载器、相对查找、服务交接、转移请求与呈现接缝消费那些规范记录。
- [Battle Effect Graphics Data](../../contracts/battle-effect-graphics-data.md) 拥有 23 个法术、30 个召唤、一个状态与两个过渡流身份、其私有容器/调色板/偏移图、压缩与解码身份、聚合大小与私有往返证据。本合同只在法术/召唤加载器、状态初始化、过渡选择、Stack 交接、转移请求与呈现接缝消费那些规范记录。

不存在已接受运行时呈现矩阵。因此本合同不作新的 H3 声称，也不把研究运行时队列转成隐含行为。

## 规范场景状态

原版保真适配器必须保留以下可区分域：

| 状态域 | 合同角色 |
| --- | --- |
| 演出命令流 | 战斗动作构建的有序命令字与参数 |
| 场景解释器状态 | 命令游标、死亡战斗员列表、行动者/目标切换、等待与返回状态 |
| 选择状态 | 行动者阵营、精灵/调色板 ID、武器、地面、背景、动画索引、镜像与变体位 |
| 解码资源状态 | 从其数据合同消费的规范背景、行动者精灵、武器、地面、法术、召唤、状态与过渡记录 |
| 序列状态 | 行动者帧条目、偏移、武器字段、法术触发与保持/默认标志 |
| 呈现驱动状态 | 调色板、布局、可选层、VInt 注册、DMA 请求、淡入与阶段/切换状态 |
| 持久战斗状态 | 其他合同拥有的 HP、MP、状态、EXP、金币、物品、死亡与战斗结果 |

命令流不是渲染帧，解码字节不是可见像素的证明。重制可以使用不同内部结构，但 H4 适配器仍必须在其所属边界暴露已确认身份、顺序、选择器、大小与别名。对背景、行动者精灵、武器、地面、法术、召唤、状态图形与过渡，本合同测试规范数据合同记录的使用，而非独立重新验证它们。

## 演出命令解释器

**已确认静态：** `ExecuteBattlesceneScript` 从 `0xFF0000` 的缓冲读取字命令、清除死亡战斗员列表、用 `0xFF` 播种其首条目、在字 `0xFFFF` 停止、通过 21 条目相对跳转表分发并返回零。

命令身份覆盖：

- 敌人/己方动作动画与精灵移动；
- 敌人/己方空闲与行动者切换；
- 敌人/己方反应；
- 行动者空闲/结束、结束与睡眠；
- EXP 奖励；
- 消息显示、无等待消息显示、文本框关闭与玩家输入等待；
- 一个 null 命令。

命令编号与处理器顺序是兼容性事实。`sleep`、`wait` 与 `displayMessageWithNoWait` 等标签不建立经过时间、已接受输入节奏、音频行为或玩家可见完成。那些效果在受限运行时所有者存在之前保持 **未知**。

场景解释器消费[battle action construction](../../contracts/battle-action-construction.md)产生的流。持久反应与奖励效果由战斗/法术测试夹具单独证明；本合同不从静态处理器名推断每个变更。

## 初始化与选择器顺序

**已确认静态顺序：** 场景初始化清除其场景数据块、解析敌人与己方图形、解析武器与背景选择器、清除既有 VInt、加载调色板/布局状态、条件加载敌人、己方、地面与武器层、添加 `VInt_UpdateBattlesceneGraphics` 与 `VInt_UpdateWindows`、加载状态动画瓦片、应用状态动画状态，然后到达淡入接缝。

选择器边界同样具体：

- 武器图形仅限己方；无效武器产生精灵/调色板 `(-1, -1)`；
- 背景选择优先 Zeon，然后战斗特定覆盖，然后地形；
- 初始化优先敌人行动者，然后己方行动者为背景上下文；
- 无当前背景行动者时，选择器使用已保存行动者，然后战斗员 0；
- 己方选择默认普通攻击、使用独立闪避块，并把 KNTE、PLDN 与 PGNT 的长矛/标枪攻击重映射到直接特殊条目。

该顺序不是引擎模块处方，也不证明玩家在调用之间看到什么。精确 VInt、DMA、调色板、淡入与可选层效果保持 **未知**。

## 行动者精灵容器与动画序列

### 精灵容器与帧加载

规范己方/敌人表、载荷所有者、头/调色板/帧流分区、压缩与解码身份、字节计数与私有往返证据由[Battle Sprite Graphics Data](../../contracts/battle-sprite-graphics-data.md)拥有。本合同不独立拥有或重新验证该静态资源目录。

属性加载存储动画速度字与后续状态图标 X/Y 字节、相对头字 2 解析调色板、清除目标颜色 0 并复制其余 15 个调色板字。帧加载解析从头字节 6 开始的自相对字，并在 DMA 前 Stack 解码所选流。固定 DMA 长度是己方帧的 `0x900` 字与敌人帧的 `0xC00` 字。解码帧身份及其已接受 4,608 字节己方 / 6,144 字节敌人大小来自规范数据合同；本合同保留加载器如何消费那些记录并请求固定转移。

这些是加载器与转移边界。源码具名头字段不建立屏幕坐标、调色板外观、精灵层放置或时序。

### 动画序列表

序列语料包含 208 个条目：87 个己方与 121 个敌人动画。跨两侧有 421 个帧条目与 334 个已播放攻击帧条目。

- 己方头与条目各为八字节；条目零也充当可选空闲帧二，因此攻击播放跳过它并消费 147 个后续条目；
- 敌人头与条目各为四字节；全部 187 个条目都是攻击帧；
- 43 个条目使用帧值 15 保留上一战斗精灵帧；
- 七个头请求默认法术动画；
- 发布语料中每个终止法术标志都是零。

普通攻击使用战斗员基础动画索引。闪避为己方加 40 或为敌人加 60。己方 80 或以上、敌人 118 或以上的索引是直接特殊条目。己方长矛重映射对 KNTE、PLDN 与 PGNT 使用直接索引 80 到 82。

可达基础索引组合、帧时长、帧 15 外观、武器翻转/层/偏移解读与法术触发时序保持 **未知**。

## 背景加载器与武器/地面容器

### 背景加载器与呈现接缝

规范指针、容器、别名、前缀/调色板/流、压缩与解码记录由[Battle Background Graphics Data](../../contracts/battle-background-graphics-data.md)拥有。本合同不独立拥有或重新验证该静态资源目录。

`LoadBattlesceneBackground` 消费一个所选规范记录，并把其两个有序流输入提交给独立拥有的 Stack 服务。源码形状的第二暂存目标在第一之后 `0x1800` 字节处。加载器清除目标调色板字 0 并复制其余 15 个源调色板字。本合同保留该选择/加载器接缝、暂存关系与调色板操作时间线，但不保留目录本身、转移完成或可见排列。

### 武器与地面

规范武器指针/流记录、连续武器调色板条目、地面指针/头/别名/流记录、压缩与解码身份、字节计数与私有往返证据由[Battle Weapon 与 Ground Graphics Data](../../contracts/battle-weapon-ground-graphics-data.md)拥有。本合同不独立拥有或重新验证那些静态语料。

`LoadWeaponPalette` 消费一个所选规范四字节调色板条目并写入源码具名的最终两个己方调色板颜色。`LoadWeaponsprite` 通过独立拥有的 Stack 服务消费一个所选规范武器流；已接受解码记录为 8,192 字节，源码格式消费者描述四个 64 瓦片视图而不建立其可见排列或角度选择规则。

`LoadBattlesceneGroundToVram` 消费一个所选规范地面头。它把头三个调色板字应用于源码具名基础颜色索引 3、4 与 8，解析自相对瓦片集字，并把所选压缩流交给独立拥有的压缩 DMA 服务，带固定 `0x300` 字请求。

这些是选择器、加载器、调色板操作、相对查找、服务交接与转移请求边界。它们不建立武器角度选择、地面/背景组合、转移完成、瓦片图放置、调色板外观或优先级，或可见层顺序。

## 法术动画分发

配置与更新各暴露 32 个分发槽。配置侧在 29 个文件中有 32 个唯一目标；Buff 与 Debuff 配置文件共享。更新侧在 26 个子文件中有 28 个唯一目标加两个根拥有目标：

- Absorb 在槽 10 与 24 复用；
- Buff 在槽 8 与 25 复用；
- Debuff 在槽 9、27 与 28 复用；
- `spellanimationUpdate_Nothing` 与 `spellanimationUpdate_Absorb` 是根拥有的。

每个子动画文件至少被一个配置/更新槽到达。配置保留镜像位并把解码变体存储为一基；禁用配置与索引 `-1` 不分发即返回。更新在分发前要求其切换与阶段状态。

槽身份、复用与门控是 **已确认静态**。逐帧状态变更、配置/更新节奏、同时效果、中断行为、调色板过渡与可见动画结果保持 **未知**。

## 战斗效果图形消费者接缝

规范法术、召唤、状态动画与过渡记录由[Battle Effect Graphics Data](../../contracts/battle-effect-graphics-data.md)拥有。本合同不独立拥有或重新验证其私有容器、调色板、偏移、流、压缩、解码或一致性身份。

保留的源码形状消费者面由以下内容组成：

- `LoadInvocationSpriteFrameToVram` 与召唤根；
- 带法术根与独立拥有 Stack 服务的 `LoadSpellTileset` 与 `LoadSpellTilesetForInvocation`；
- `InitializeBattlescene` 对状态动画根的消费，带固定 `0x270` 字请求；以及
- `bsc06_switchEnemies` 从过渡根的选择。

两个召唤消费者路径都为规范 4,096 字节解码流请求 4,608 字节。因此每请求 512 字节尾（跨 30 个流请求共 15,360 字节）是显式消费者边界：其内容、来源、初始化、稳定性、转移完成与可见性为 **未知**，不得从相邻数据或内存发明。

这些是加载器、指针选择、服务交接与转移请求边界。它们不证明调色板、层排序、帧时序、过渡组合或渲染输出。

## 保真与现代化边界

原版保真适配器必须保留：

- 命令字身份、21 条目分发顺序、终止符、死亡列表初始化与返回状态；
- 已接受静态接缝处的场景初始化与选择器时间线；
- 从[Battle Sprite Graphics Data](../../contracts/battle-sprite-graphics-data.md)消费而不独立重新验证其目录的规范行动者精灵记录，加独立序列表、法术分发表与呈现驱动状态；
- 从[Battle Background Graphics Data](../../contracts/battle-background-graphics-data.md)消费而不独立重新验证其目录的规范背景记录，加已接受背景选择、暂存、调色板操作与呈现接缝；
- 从[Battle Weapon 与 Ground Graphics Data](../../contracts/battle-weapon-ground-graphics-data.md)消费而不独立重新验证其目录的规范武器与地面记录，加已接受选择、调色板操作、相对查找、服务交接、转移请求与呈现接缝；
- 已接受行动者精灵属性/帧查找、调色板操作、Stack 交接与固定 DMA 接缝，加从[Battle Effect Graphics Data](../../contracts/battle-effect-graphics-data.md)消费而不独立重新验证该目录的规范法术、召唤、状态与过渡记录，以及已接受加载器、指针选择、Stack 交接、转移请求、未知召唤尾与呈现接缝；
- 解码载荷、请求转移、命令/回放状态与可见呈现之间的区分；
- 每个分项 **未知** 边界，而非用标签或现代约定填充它。

未来重制可以使用解码现代资源、不同渲染器、类型化演出命令、现代动画图、异步资源加载、更高帧率、可跳过效果、可访问性选项或重新设计的过渡。这些是产品决定。刻意偏差需要显式决定与 H4 expected-deviation 测试夹具；它不是新发现的原版规则。

## H4 验收面

未来 H4 适配器应消费本合同具名的七个测试夹具并比较：

1. 命令缓冲初始化、有序命令分发、终止与返回状态；
2. 选择器输入/结果与场景初始化事件顺序；
3. [Battle Sprite Graphics Data](../../contracts/battle-sprite-graphics-data.md)通过所选己方/敌人属性与帧加载器提供的规范行动者精灵记录，包括动画速度/状态偏移消费、调色板选择与字 0 清除/复制 15 时间线、相对帧查找、Stack 交接与固定 `0x900`/`0xC00` DMA 请求；
4. 全部 208 个序列身份、帧记录、保持/默认标志与选择器索引规则；
5. [Battle Background Graphics Data](../../contracts/battle-background-graphics-data.md)通过所选加载器接缝提供的规范背景记录、相隔 `0x1800` 的两个有序暂存目标，以及调色板字 0 清除/复制 15 时间线；加[Battle Weapon 与 Ground Graphics Data](../../contracts/battle-weapon-ground-graphics-data.md)通过武器调色板、武器流与地面头加载器接缝提供的规范武器/地面记录，包括最终两色写入、受限四视图消费者形状、三个地面调色板字写入、自相对查找、服务交接与固定地面 `0x300` 字请求；
6. 全部 32 个配置/更新槽身份、复用、禁用/减一门、镜像/变体值与更新切换/阶段准入；
7. [Battle Effect Graphics Data](../../contracts/battle-effect-graphics-data.md)通过已接受召唤与法术加载器、状态初始化、过渡选择、Stack 交接、固定召唤 `0x900` 字与状态 `0x270` 字请求，以及已声明未知召唤尾边界提供的规范法术、召唤、状态与过渡记录。

当重制没有等价内存布局时，H4 必须比较规范记录而非原版 RAM/ROM 地址。渲染像素、帧节奏、VDP/VInt 时间线、调色板外观、音频/输入时序、自然可达性与持久战斗结果需要独立已接受所有者。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 命令解释器、初始化、选择器、法术配置/更新表 | **已确认静态** | `sf2-battle-scene-engine-static-v1`（[`battle-scene-engine-static-v1.json`](../../../../tests/fixtures/h2/battle-scene-engine-static-v1.json)） | 运行时命令节奏、VInt/VDP/淡入效果、渲染结果 |
| 完整配置/更新源配对 | **已确认静态** | `sf2-battle-scene-animations-static-v1`（[`battle-scene-animations-static-v1.json`](../../../../tests/fixtures/h2/battle-scene-animations-static-v1.json)） | 逐帧状态与可见动画 |
| 背景选择/加载器接缝、暂存关系、调色板操作 | **已确认静态** | `sf2-battle-background-decode-v1`（[`battle-background-decode-v1.json`](../../../../tests/fixtures/h2/battle-background-decode-v1.json)）中的受限函数见证；规范资源记录由[Battle Background Graphics Data](../../contracts/battle-background-graphics-data.md)拥有 | 转移完成、瓦片排列、层组合、可见调色板 |
| 己方/敌人属性/帧加载器接缝、调色板操作、Stack 交接、DMA 请求 | **已确认静态** | `sf2-battle-sprite-decode-v1`（[`battle-sprite-decode-v1.json`](../../../../tests/fixtures/h2/battle-sprite-decode-v1.json)）中的受限函数见证；规范资源记录由[Battle Sprite Graphics Data](../../contracts/battle-sprite-graphics-data.md)拥有 | 转移完成、放置、时序、渲染帧 |
| 208 个行动者动画序列与选择器规则 | **已确认静态** | `sf2-battle-sprite-animation-static-v1`（[`battle-sprite-animation-static-v1.json`](../../../../tests/fixtures/h2/battle-sprite-animation-static-v1.json)） | 可达基础索引组合、时序、武器字段解读 |
| 武器/地面选择与加载器接缝、调色板操作、相对查找、服务/转移交接 | **已确认静态** | `sf2-battle-weapon-ground-decode-v1`（[`battle-weapon-ground-decode-v1.json`](../../../../tests/fixtures/h2/battle-weapon-ground-decode-v1.json)）中的受限函数见证；规范资源记录由[Battle Weapon 与 Ground Graphics Data](../../contracts/battle-weapon-ground-graphics-data.md)拥有 | 转移完成、角度选择、放置、组合 |
| 法术/召唤/状态/过渡加载器与消费者接缝、Stack 交接、固定转移请求 | **已确认静态** | `sf2-battle-effect-graphics-decode-v1`（[`battle-effect-graphics-decode-v1.json`](../../../../tests/fixtures/h2/battle-effect-graphics-decode-v1.json)）中的受限函数见证；规范资源记录由[Battle Effect Graphics Data](../../contracts/battle-effect-graphics-data.md)拥有 | 召唤尾内容/稳定性/可见性、转移完成、调色板、层/时序/过渡组合 |
| 持久 HP/EXP 演出命令回放 | **独立已确认运行时子集** | [交战解决合同](../../contracts/combat-resolution.md) | 不是渲染呈现的证据 |
| 渲染像素、帧时序、VInt/VDP 效果、呈现意图 | **未知** | 无已接受可执行所有者 | 需要分组运行时呈现矩阵或未来产品决定 |

## 复现

```powershell
uv run sf2 h2 battle-scene-engine
uv run sf2 h2 battle-scene-animations
uv run sf2 h2 battle-backgrounds
uv run sf2 h2 battle-sprites
uv run sf2 h2 battle-sprite-animations
uv run sf2 h2 battle-weapon-ground
uv run sf2 h2 battle-effect-graphics
uv run sf2 design-contracts test
uv run sf2 research-index test
```
