# 立绘窗口与状态合同

- **已确认原版结构：** 六个受限立绘/菜单服务身份；下文描述的源静态选择、加载器消费、窗口、回调、更新与名称窗口顺序；以及由独立图形数据合同拥有的规范立绘数据交接。
- **推断原版行为：** 此处不提升任何内容。上游标签与注释不建立玩家面向意图、死亡语义或可见动画行为。
- **未知原版行为：** 调用方准入与自然可达性、无效立绘索引效果、畸形头行为、VInt/RNG/DMA 节奏、输入重复含义、可见眨眼或嘴部时序、窗口运动时长、调色板/VRAM 完成、最终组合，以及跨对话、菜单、战斗与故事场景的精确呈现。
- 重制状态：实现无关 Phase 3 状态交接与立绘数据消费者合同；未选择渲染器、窗口工具包、立绘动画策略、可访问性行为、本地化流程或许可立绘包。
- 证据日期：2026-08-08
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`portrait-window-state.md`](../../contracts/portrait-window-state.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签按 R1 使用固定中文译法；源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同定义从原版立绘选择器经规范立绘数据消费到受限立绘/名称窗口状态的静态接缝。它拥有：

1. 证据审计中列出的六条研究记录身份与 H1 绑定条目地址；
2. 原始 `GetCombatantPortrait` 符号分支与受限 `GetAllyPortrait` 职业重映射顺序；
3. `LoadPortrait` 对规范立绘记录的消费，包括计数眼/嘴复制、调色板复制顺序、Stack 与转移交接与精确源操作数；
4. 源静态开/关立绘窗口状态与回调注册/移除顺序；
5. 源静态眼/嘴更新门、计数器、RNG 调用操作数/后加、原版/alternate 瓦片选择与镜像状态交接；
6. 源静态开/关名称窗口顺序及其原始当前 HP 零/非零字体分支；
7. 基于受限函数/状态身份、调用顺序与合成轨迹而非原版立绘字节或重复目录表示的公开 H4 面。

它不拥有实体地图精灵属性对立绘的选择、对话命令、服务菜单状态机、原始窗口布局、窗口引擎行为、全局 VInt/DMA 语义、渲染帧、文本内容、音频、持久性、本地化、可访问性或可分发资源。

所选可执行所有者是：

- `sf2-common-menus-static-v1`，位于
  [`tests/fixtures/h2/common-menus-static-v1.json`](../../../../tests/fixtures/h2/common-menus-static-v1.json)，由 [`src/sf2tool/h2/menus.py`](../../../../src/sf2tool/h2/menus.py) 实现；
- `sf2-portrait-graphics-decode-v1`，位于
  [`tests/fixtures/h2/portrait-graphics-decode-v1.json`](../../../../tests/fixtures/h2/portrait-graphics-decode-v1.json)，由 [`src/sf2tool/h2/portraits.py`](../../../../src/sf2tool/h2/portraits.py) 实现。

所属研究文章是[Common Menu Engines 与 Services](../../../research/common-menus.md)与[Technical Graphics 与 Decompression Services](../../../research/technical-graphics.md)。

立绘 fixture 此处保持受限 `LoadPortrait` 消费者见证。静态表身份、指针顺序、载荷所有者别名、头/调色板/流分区、字节/解码计数、一致性与私有导入保真由[Portrait Graphics Data](../../contracts/portrait-graphics-data.md)拥有。本合同从该所有者消费规范记录，不独立拥有或重新验证目录。

## 合同前证据审计

证据日期新鲜复现通过：

```text
Inventory sf2-common-menus-static-v1
SHA256 9D9D1E3B7F847193307DA6E3C0114D33597EE4E7667E99EDFD1C7EF362426DB6
Files 42
LayoutIncludedFiles 41
IndexedRecords 42
Status PASS

Contract sf2-portrait-graphics-decode-v1
SHA256 D691E2058673D2837A391AE771E5DEBE6F3F2F896222F819F3F414F33D4EEEB6
PortraitPointers 56
UniquePayloads 52
DecodedBytes 106496
Status PASS
```

审计发现恰好六条当前未关联研究记录：

| 记录 ID | 原版符号 | ROM 地址 | 所选可执行所有者 |
| --- | --- | ---: | --- |
| `menus.ally-portrait` | `GetAllyPortrait` | 87,862 | `sf2-common-menus-static-v1` |
| `menus.combatant-portrait` | `GetCombatantPortrait` | 75,322 | `sf2-common-menus-static-v1` |
| `menus.name-under-portrait` | `OpenNameUnderPortraitWindow` | 92,590 | `sf2-common-menus-static-v1` |
| `menus.portrait-functions` | `ClosePortraitEyes` | 87,286 | `sf2-common-menus-static-v1` |
| `menus.load-portrait` | `LoadPortrait` | 87,594 | `sf2-portrait-graphics-decode-v1` |
| `menus.portrait-window` | `OpenPortraitWindow` | 72,518 | `sf2-common-menus-static-v1` |

每个候选至少有一个所选可执行所有者。注册推迟到初步语义接受。

common-menus fixture 是聚合清单。本合同只消费五个所选函数身份/地址与其显式立绘时序队列边界。它不消费服务状态机、提示返回规则、野外物品分发、商店/教堂/车队/铁匠事实、图标/UI 图形、小地图/成员/结尾清单或未构建 alternate。

下文的详细辅助序列是固定 commit 处的 **已确认静态源审查**。它们不被呈现为受限运行时观察。

## 立绘选择器边界

### 战斗员选择器

**已确认静态源审查：** `GetCombatantPortrait` 测试传入 `d0` 低字节并按其符号分支：

1. 负字节调用实体立绘/语音 SFX 查找，然后把返回 `d1` 复制进 `d0`；
2. 非负字节调用 `GetAllyPortrait`；
3. 两条路线经同一出口返回。

这是原始符号分支合同。它不证明哪些战斗员种类或调用方状态供给每条路线。敌人/实体查找由[Sprite-Dialogue Property Data](../../contracts/sprite-dialogue-property-data.md)独立拥有；本合同不消费其 fixture，也不复制其表/查找 H4 面。

### 己方/职业重映射

**已确认静态源审查：** `GetAllyPortrait` 保留 `d1`、把传入 `d0.b` 与符号 `COMBATANT_ALLIES_NUMBER` 比较，并在无符号字节高于该源常量时取 `bhi` 直接返回。否则它调用 `GetClass` 并按顺序检查这五个职业身份：

| 职业身份 | 替换立绘身份 |
| --- | --- |
| `CLASS_HERO` | `PORTRAIT_BOWIE_PROMO` |
| `CLASS_PHNX` | `PORTRAIT_PETER_PROMO` |
| `CLASS_WFBR` | `PORTRAIT_GERHALT_PROMO` |
| `CLASS_NINJ` | `PORTRAIT_SLADE_PROMO` |
| `CLASS_MNST` | `PORTRAIT_KIWI_PROMO` |

都不匹配时，输入值保持在 `d0`。合同保留原始字节比较、分支条件、符号身份与顺序。它不从上游注释或名称推断更宽数字立绘或己方域。

## 规范立绘数据消费

完整表、载荷所有者图、计数条目序列、调色板、压缩流、解码身份、大小、一致性与公开/私有投影由[Portrait Graphics Data](../../contracts/portrait-graphics-data.md)定义。本合同为有效已接受选择器从该所有者接收一个规范逻辑立绘记录。它只依赖下文源时间线需要的消费者面向形状：

```text
CanonicalPortraitRecord {
  logicalPortraitId
  eyeEntries[]
  mouthEntries[]
  paletteIdentity
  stackStreamIdentity
  decodedTileIdentity
}
```

该交接不使原版指针图、源地址、载荷字节、hash、别名或聚合语料计数成为本合同的一部分。数据所有者测试私有导入与往返保真；本合同测试所选规范记录如何被消费。

原版加载器在索引指针表前不执行显式选择器范围检查。规范数据合同闭合其已接受有效身份；无效、注入、修改或损坏索引的行为保持 **未知**，而非归一化为新回退。

## `LoadPortrait` 源顺序

**已确认静态源审查与语料边界：** 对有效已接受槽，`LoadPortrait` 执行该顺序：

1. 保留 `d0..a3`；
2. 解析所选指针表条目；
3. 读取眼条目计数、存储它，并在非零时复制那么多四字节条目；
4. 读取嘴条目计数、存储它，并在非零时复制那么多四字节条目；
5. 把恰好八个调色板 longword 并行复制到当前、基础与备份调色板状态；
6. 把 Stack 目标设置为立绘加载空间并调用 `LoadStackCompressedData`，其已接受载荷输出为 2,048 字节；
7. 用源字节值 6 递增 `INPUT_REPEAT_DELAYER`；
8. 把解码后 `a1` 值移进 `a0`、把立即 VRAM 目标 `$F800` 加载进 `a1`、把 `$0400` 字加载进 `d0`、把源值 2 加载进 `d1`；
9. 调用 `ApplyVIntVramDma`；
10. 调用 `ApplyVIntCramDma`；
11. 恢复 `d0..a3` 并返回。

第 6 到 10 步是精确源调用/操作数顺序。立即 `$F800`/`$0400`/`2` 操作数属于 VRAM 调用接缝。所选立绘 fixture 不定义立绘特定 CRAM 转移大小或证明 CRAM 完成；`ApplyVIntCramDma` 使用本合同之外拥有的全局调色板/队列状态。两个调用都不建立 VInt 节奏、队列处理顺序、墙钟时长、可见完成或硬件时序。

递增 6 是原始源变更。其上游注释不证明玩家面向按住输入行为，本合同不指定时序解读。

## 立绘窗口状态

### 打开

**已确认静态源审查：** `OpenPortraitWindow` 在立绘窗口索引非零时立即返回。否则执行该受限顺序：

1. 递增全局窗口在场字节并保留寄存器/输入选择器；
2. 存储镜像与右侧输入字节；
3. 初始化立绘 VDP 瓦片字、眨眼计数器为 20、次级立绘计数器为 6；
4. 创建立绘窗口并存储其一基窗口索引；
5. 从镜像切换选择普通或镜像立绘布局身份并复制 160 字节；
6. 恢复选择器、调用 `GetAllyPortrait`，然后调用 `LoadPortrait`；
7. 用存储侧切换设置窗口目标、用源速度值 4 移动窗口并调用 `WaitForWindowMovementEnd`；
8. 通过 VInt 函数 trap 注册 `VInt_PerformPortraitBlinking`；
9. 把眨眼控制字节设置为 `-1`、恢复寄存器并返回。

普通/镜像 8×10 布局数据与精确原始 160 字节内容保持[UI Layout Data](../../contracts/ui-layout-data.md)拥有。本合同只保留源布局身份选择与复制大小；它不消费 UI-layout fixture，也不添加布局关联。

### 关闭

**已确认静态源审查：** `ClosePortraitWindow` 在其索引为零时立即返回。否则它在计算屏外目标前移除 `VInt_PerformPortraitBlinking`、用源速度 4 移动、等待移动结束、删除窗口、清除存储索引、恢复寄存器、递减窗口在场字节并返回。

打开/关闭顺序在源中建立状态平衡与回调生命周期。它不证明可见运动时长、回调节奏、DMA 完成、呈现正确性或全局状态外部不一致时的行为。

## 眼与嘴更新状态

### 立即关闭/更新辅助

**已确认静态源审查：** `ClosePortraitEyes` 清除眨眼控制字节、调用 `WaitForVInt`、保留 `d0`，并用原始位 0 选择原版/alternate 眼瓦片、原始位 1 选择原版/alternate 嘴瓦片。每个选择用对应计数/数据状态调用 `UpdatePortrait`。此处的位含义是调用形状事实，不是渲染语义标签。

### 注册 VInt 辅助

**已确认静态源审查：** `VInt_PerformPortraitBlinking` 在立绘窗口索引为零或眨眼控制字节为零时返回。其已接受源操作包括：

- 递减眨眼计数器；在计数器 3 选择 alternate 眼瓦片、在计数器 0 选择原版眼瓦片；
- 眨眼计数器为 0 时，调用 `GenerateRandomNumber` 并使用 `d6=120`、给返回 `d7` 加 30，并把结果存储为下一计数器；
- 用打字机字节与既有计数器状态门控嘴计数器处理；
- 计数器到 5 时选择 alternate 嘴瓦片、到零或源重置路径时选择原版嘴瓦片；
- 调用 `GenerateRandomNumber` 并使用 `d6=5`、把 `$000A` 加到返回 `d7` 并存储结果。

RNG 操作数与后加是源静态调用事实。它们不是观察概率分布、VInt 频率、墙钟延迟或可见动画时序。

### 瓦片更新辅助

**已确认静态源审查：** `UpdatePortrait` 在瓦片条目计数为零时返回。否则读取四字节条目记录、选择原版或 alternate 瓦片坐标、对窗口瓦片写入应用镜像切换，并以窗口目标交接结束。精确 VDP 瓦片外观、调色板结果、更新节奏与最终帧保持 **未知**。

## 立绘下名称窗口状态

**已确认静态源审查：** `OpenNameUnderPortraitWindow` 在其存储索引非零时返回。否则创建并写入名称窗口、读取当前 HP 与战斗员名称、用源名称长度水平放置，然后应用该原始分支：

- 当前 HP 字为零：调用橙色字体写入器；
- 当前 HP 字非零：调用常规字体写入器。

然后它用源速度 4 移动窗口并等待移动结束。上游注释把零分支标记为死亡角色呈现，但本合同只保留原始零/非零谓词与所选写入器身份。

`CloseNameUnderPortraitWindow` 在其索引为零时返回。否则把窗口移出屏幕、等待、删除它、清除索引、恢复寄存器并返回。可见字体颜色、名称内容、运动时长、状态语义与调用方准入保持独立或 **未知**。

## 实现无关消费者与状态模型

以下为逻辑兼容模型，不是引擎类层级：

```text
PortraitDataHandle {
  logicalPortraitId
  canonicalRecordRef: portrait-graphics-data
}

PortraitSelectorResult {
  rawInputByte
  route: GET_ALLY_PORTRAIT | ENTITY_PROPERTY_LOOKUP
  portraitIdentity
}

PortraitWindowState {
  windowIndex
  windowPresentState
  mirroredToggle
  rightSideToggle
  blinkControl
  blinkCounter
  secondaryAnimationCounter
  callbackRegistered
}

NameWindowState {
  windowIndex
  selectedFontWriter: ORANGE | REGULAR
}
```

公开形式保留受限函数与选择器身份、消费者调用/操作数顺序、窗口状态字段与合成状态转换轨迹。静态目录元数据与私有载荷材料只由 `portrait-graphics-data` 投影。

## 跨系统分离

本合同接收原始立绘/战斗员选择器并把状态交给窗口、调色板、VRAM 与呈现服务。它不决定：

- 静态立绘目录身份、指针顺序、别名、载荷分区、解码保真或公开/私有数据投影；
- 实体地图精灵赋值或地图精灵到立绘/SFX 属性查找；
- 对话命令准入、文本选择、故事顺序或立绘侧修饰符含义；
- 服务菜单调用方、车队消息、战斗演出选择或成员画面组合；
- UI-layout 数据所有权、窗口分配器/移动实现或 VInt 回调调度器；
- 全局 CRAM/VRAM DMA 队列语义、完成、节奏、硬件时序或帧组合；
- 立绘美术许可、本地化、可访问性、眨眼安全、语音策略或替换内容；
- 无效选择器恢复、畸形私有数据诊断、持久性或存档/读档行为。

那些边界保持独立所有者、**未知**、私有或刻意产品设计。

## 保真、现代化与版权边界

静态立绘数据兼容被委托给[Portrait Graphics Data](../../contracts/portrait-graphics-data.md)。本合同要求保留选择器分支/顺序、规范记录消费、开/关状态顺序、精确 `LoadPortrait` 复制/尾操作数与调用、静态动画操作数/后加与名称窗口零/非零分支。

重制可以在导入期间转码私有立绘流、使用现代纹理图集、用可访问性安全策略替换随机眨眼、选择不同窗口运动或使用编写的区域特定名称呈现。那些决定必须报告为刻意现代化，而非关于原版可见行为的证据。

原版立绘头、调色板、压缩流、解码瓦片、截图、捕获与其他游戏资源是私有/生成版权输入。不要提交或再分发它们。公开构建需要新编写或适当许可内容。

## H4 验收面

重制侧立绘消费者/状态适配器只在自动化测试证明以下内容时声称本合同：

1. 一个已接受逻辑立绘身份经 `portrait-graphics-data` 解析到规范记录，而本合同不重构或重新验证原版目录；
2. 战斗员选择器保留原始字节符号分支与返回交接，己方辅助保留其精确无符号比较/`bhi` 门加五个符号职业重映射检查顺序；
3. `LoadPortrait` 消费规范记录，同时保留计数眼/嘴复制、八个 longword 调色板复制、Stack 调用、重复延迟 `+6`、立即 VRAM 操作数配置 `$F800`/`$0400`/`2`、VRAM 调用，然后 CRAM 调用，而不声称立绘特定 CRAM 边界、完成或节奏；
4. 立绘窗口开/关保留索引守卫、切换/计数器状态、普通/镜像布局身份选择、选择器/加载顺序、移动/等待接缝、回调添加/移除生命周期、索引清除与窗口在场平衡；
5. 源码形状眼/嘴测试保留门、计数器比较、RNG 操作数 `120` 与 `5`、后加 `30` 与 `$000A`、原版/alternate 选择与镜像状态交接，而不把它们当作观察分布或可见时序；
6. 名称窗口保留其索引守卫、创建、初始 `WriteWindowTiles`、当前 HP 读取、战斗员名称读取、所选字体写入器顺序、原始 HP 零→橙与 非零→常规 写入器选择、移动/等待/删除顺序与索引清除；
7. 无效索引、畸形数据、调用方、VInt/RNG/DMA 节奏、窗口运动、可见帧、调色板结果、本地化、可访问性与许可内容保持独立验收面。

H4 不要求原版汇编指令、Mega Drive VDP 模拟器、公开构建中的原版载荷或帧周期一致性，除非后续显式硬件保真决定添加它们。

## 证据矩阵

| 合同区域 | 证据标签 | 所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 六个函数身份与 H1 绑定地址 | **已确认静态清单** | `sf2-common-menus-static-v1` 与 `sf2-portrait-graphics-decode-v1`（上文链接 fixture） | 所选记录之外的聚合服务/菜单事实被排除 |
| 规范目录身份、别名、载荷分区、解码保真与私有投影 | **独立所有者已确认静态** | [Portrait Graphics Data](../../contracts/portrait-graphics-data.md) | 本合同消费记录但不独立拥有或重新验证目录 |
| 选择器、开/关、LoadPortrait、更新、回调与名称窗口指令/调用顺序 | **已确认静态源审查** | commit `c834c652b6862bc5679fd7f69a38a7093206efc6` 的固定源路径 | 自然调用方、外部不一致状态与运行时结果保持 **未知** |
| 立绘属性查找与对话命令/调用方接缝 | **独立所有者** | [Sprite-Dialogue Property Data](../../contracts/sprite-dialogue-property-data.md)与[Dialogue System](../../contracts/dialogue-system.md) | 端到端对话呈现保持未闭合 |
| 普通/镜像立绘布局载荷 | **独立所有者** | [UI Layout Data](../../contracts/ui-layout-data.md) | 本合同保留选择身份/复制大小但不消费布局 fixture |
| VInt 回调执行、RNG 分布、DMA 队列处理/完成、可见时序与最终帧 | **未知 / 独立所有者** | 未来受限运行时/呈现证据 | 静态调用顺序不是运行时节奏 |
| 渲染器、可访问性、本地化、替换立绘与许可内容 | **刻意设计** | 未来产品/内容决定 | 需要独立溯源与验收 |

## 复现

```powershell
uv run sf2 h2 common-menus
uv run sf2 h2 portraits
uv run sf2 design-contracts test
uv run sf2 verify
```

生成详细输出保留在忽略的 `local/derived/common-menus-static.json` 与 `local/derived/portrait-graphics-decode.json` 下。
