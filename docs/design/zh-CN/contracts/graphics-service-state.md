# 图形服务与状态合同

- **已确认原版结构：** 下文描述的受限解压条目身份与寄存器 ABI、显示初始化顺序、精灵链接初始化形状、调色板过渡状态、固定 flash 脚本字、特殊精灵加载/更新路由与名义未使用辅助清单。
- **推断原版行为：** 此处不提升任何内容。把这些源服务分组成现代图形边界是实现无关设计综合，不是原版引擎架构的证据。
- **未知原版行为：** 渲染帧一致性、可见 flash 时长、VInt 或 CRAM-DMA 节奏、硬件时序、最终调色板外观、特殊精灵帧呈现、刻意畸形/调试/原始 RAM 输入的调用方可视行为，以及名义未使用辅助的运行时含义。
- 重制状态：实现无关 Phase 3 服务/状态合同；未选择渲染器 API、图形引擎、资源格式、硬件模拟目标、时序模型、呈现策略或许可内容包。
- 证据日期：2026-08-08
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`graphics-service-state.md`](../../contracts/graphics-service-state.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同为源静态身份、输入/输出接缝、顺序或状态转换已接受的原版图形服务定义窄兼容边界。它拥有：

1. Basic 与 Stack 解压条目身份及其共享 `a0` 源、`a1` 目标与 `d0` 输出字节计数 ABI；
2. 已接受显示初始化顺序，而不声称可见帧或硬件时序一致性；
3. 精灵表链接初始化器与独立清单的战斗精灵链接辅助的存在；
4. 调色板过渡计时器/步/权重状态及其受限源可见队列交接；
5. 精确特殊精灵加载/更新路由拆分与加载对刷新转移接缝，消费[special-sprite-graphics-data](../../contracts/special-sprite-graphics-data.md)的规范指针/资源身份；
6. 固定三字 flash 脚本；
7. 名义未使用显示/图形辅助的身份与源使用清单。

它不拥有地图摄像机目标计算、源静态 VInt 摄像机更新算法、视差/自动滚动输入、压缩资源载荷、解码美术、地图/UI/战斗呈现、VDP 模拟、DMA 调度节奏、帧时序、本地化、可访问性或可分发游戏资源。

所选可执行所有者是：

- `sf2-tech-graphics-static-v1`，位于
  [`tests/fixtures/h2/tech-graphics-static-v1.json`](../../../../tests/fixtures/h2/tech-graphics-static-v1.json)，由
  [`src/sf2tool/h2/graphics.py`](../../../../src/sf2tool/h2/graphics.py) 实现；
- `sf2-special-sprite-decode-v1`，位于
  [`tests/fixtures/h2/special-sprite-decode-v1.json`](../../../../tests/fixtures/h2/special-sprite-decode-v1.json)，由
  [`src/sf2tool/h2/special_sprites.py`](../../../../src/sf2tool/h2/special_sprites.py) 实现。

所属研究文章是[Technical Graphics 与 Decompression Services](../../../research/technical-graphics.md)。相邻呈现与资源导入合同保留其自身证据与验收面。

## 合同前证据审计

证据日期新鲜复现通过：

```text
Inventory sf2-tech-graphics-static-v1
SHA256 E915FC30C3E25983BE47A859D3DF2A169E9F53036727FF4F5C3FC5B889209EDC
Files 11
GlobalLabels 209
DirectCallSites 34
IndexedRecords 14
Status PASS

Contract sf2-special-sprite-decode-v1
SHA256 E3DF0CEDBA48E8A5BB30D639868B9CB90C6C4FFA660D8ADA56DAB9969CEEFCA7
Pointers 10
Streams 6
DecodedBytes 16704
FullyRoutedIds 9
Status PASS
```

审计识别恰好十三条当前未关联 `tech.graphics.*` 记录：

- `tech.graphics.animate-special-sprite`；
- `tech.graphics.battle-sprite-links`；
- `tech.graphics.decompression`；
- `tech.graphics.display`；
- `tech.graphics.display-init`；
- `tech.graphics.flash-white`；
- `tech.graphics.palette-transition`；
- `tech.graphics.special-sprite-anims`；
- `tech.graphics.special-sprites`；
- `tech.graphics.sprite-core`；
- `tech.graphics.stack-decompression`；
- `tech.graphics.unused-display`；
- `tech.graphics.unused-helpers`。

每个候选至少有一个两个所选执行所有者。注册推迟到初步语义接受。

审计刻意只消费聚合 tech-graphics fixture 的所选字段：

- `graphicsFacts.viewDestination` 被整体排除。额外索引记录 `map.camera-control.set-view-destination`、区域视差/自动滚动输入、摄像机命令/H3 行为与目标轴写入保持已接受 map-camera H3 轨道与[map-exploration 合同](../../contracts/map-exploration.md)拥有。`VInt_UpdateViewData` 内的独立源静态目标跟随与滚动速度派生属于[map-camera-update-control-flow](../../contracts/map-camera-update-control-flow.md)。
- 从 `graphicsFacts.inventoryBoundary`，本合同消费 `unusedDisplayAndGraphicsHelpersInventoried` 加视觉/VDP 时序与特殊精灵帧呈现的显式队列。它不消费战斗、地图、UI、立绘、特殊画面或其他资源语料完成布尔值，也不把 fixture 的历史剩余语料队列变成重制要求。
- `stackHistoryBytes`、初始历史字、位流语法、复制循环细节与编解码器实现是独立所有者静态证据。它们此处不是 H4 保真要求。
- 特殊精灵服务与路由声称使用专用特殊精灵 fixture，而非聚合清单完成标志。静态指针/资源目录与私有导入保真被委托给[special-sprite-graphics-data](../../contracts/special-sprite-graphics-data.md)。

## 解压服务 ABI

**已确认静态：** 原版暴露 Basic 与 Stack 解压条目身份。两者使用相同受限调用接缝：

| 寄存器 | 合同角色 |
| --- | --- |
| `a0` | 入口源地址 |
| `a1` | 入口目标地址 |
| `d0` | 返回输出字节计数 |

`LoadBasicCompressedData` 在 ROM 地址 6,788 H1 绑定。专用特殊精灵所有者把其已接受语料使用的 Stack 条目绑定在 ROM 地址 7,752。条目身份与 ABI 是兼容面；重制不需要复现原版历史存储、位流解析器、复制循环、寄存器分配或指令顺序。

私有导入器可以使用项目拥有解码器验证原版载荷。公开合同与 fixture 保留元数据、hash、大小与编解码器统计，而非压缩或解码版权美术。已接受语料之外的畸形流准入、恢复、部分写入与诊断保持产品/导入器策略，而非重构运行时行为。

## 显示初始化顺序

**已确认静态：** `InitializeDisplay` 在 ROM 地址 12,322 H1 绑定并执行这些已接受顺序约束：

1. 停用上下文 VInt 函数；
2. 等待 VInt；
3. 禁用显示与中断；
4. 清除精灵表；
5. 配置 H32/V32 非隔行平面与滚动表；
6. 立即加载黑屏；
7. 加载精灵掩码与基础 UI 调色板。

这是源可见初始化计划，不是渲染帧时间线。精确 VInt 边界、中断延迟、VDP 寄存器节奏、DMA 完成、首可见帧与硬件周期行为保持 **未知** 或独立所有者。

独立 `tech.graphics.display` 记录保留 ROM 地址 12,526 的 `sub_30EE` 服务身份及其清单边界。它不导入同一源文件中的排除 view-destination 事实。上文七个编号组是解释性组织，不是独立已接受源基数。

## 精灵链接初始化

**已确认静态：** `InitializeSprites` 在 ROM 地址 6,000 H1 绑定。已接受源形状使用 `dbf` 计数器、写顺序精灵链接并终止最终链接。兼容初始化器必须保留结果有序链接链不变量；它不需要复现相同循环指令或寄存器分配。

战斗精灵链接辅助（由 ROM 地址 6,466 的 H1 绑定 `sub_1942` 条目代表）被完整清单为源身份，但该 fixture 不指定完整调用方可视状态合同。其精确准入、实体到精灵选择、帧组合、可见性与时序保持相邻战斗/呈现所有者。

## 调色板过渡状态

**已确认静态：** `UpdateBasePalettesAndBackupCurrent` 在 ROM 地址 6,600 H1 绑定。已接受状态合同有：

- 初始计时器值 32；
- 由当前计时器除以 4 派生的混合步选择器；
- 总和为 8 的两个混合权重；
- 每次调色板更新调用一个源可见 CRAM-DMA 队列交接；
- 可以把备份调色板提升进新基础调色板的已接受完成分支。

值 `32`、`4` 与 `8` 是状态/权重事实，不是墙钟时长。源可见队列交接不建立 VInt 节奏、CRAM 转移节奏、硬件时序、丢弃或合并更新、渲染颜色或玩家感知过渡。

## 特殊精灵路由与转移接缝

**已确认静态：** `LoadSpecialSprite` 在 ROM 地址 154,660 开始，`AnimateSpecialSprite` 在 154,806 开始。指针表在 154,620 开始。专用 fixture 包含解析到五个初始载荷的十个指针槽，加一个仅动画流，但十个指针不意味着十个完整路由地图精灵 ID。

十槽指针与六资源目录由[special-sprite-graphics-data](../../contracts/special-sprite-graphics-data.md)规范化。本服务合同消费那些记录进行路由与转移测试；它不独立拥有或重新验证资源目录、别名、调色板、压缩/解码载荷、大小或私有导入图。

已接受路由拆分精确：

| 地图精灵 ID | 指针状态 | 加载/更新分发状态 | 合同分类 |
| --- | --- | --- | --- |
| `247..255` | 有支撑 | 有支撑 | 9 个完整路由 ID |
| `246` | 由 Kraken 指针支撑 | 无加载或更新槽 | 1 个仅指针 ID |
| `240..245` | 无支撑 | 无支撑 | 6 个无支撑特殊 ID |

对九个完整路由 ID，特殊索引 2 是探索路线，其他已接受槽使用战斗处理。调色板 4 在分发前加载。初始战斗/探索加载使用立即 DMA 接缝，而动画刷新使用排队 DMA 接缝。这些是操作与路由事实，不是转移时序或渲染帧证据。

单独索引特殊精灵动画表身份 `table_2784C` 在 ROM 地址 161,868 H1 绑定。本合同保留该身份及其与特殊精灵服务清单的关系；它不单独从表名或地址声称解码帧顺序、可见动画时序或最终输出。

已接受[Common Scripting](../../../research/common-scripting.md) 所有者已静态排除完整原版构建地图精灵赋值域中的 ID `237..250`。本合同不消费该所有者 fixture 或复制其 H4 面，但把结果保留为 **独立所有者已确认静态边界**。因此它不把所有保留/特殊可达性重新标记为未知。刻意畸形脚本、仅调试赋值、原始 RAM 注入、调用方可视失败行为与强制值的最终呈现保持 **未知** 或刻意测试输入。

## 固定 Flash 脚本

**已确认静态：** `ExecuteFlashScreenScript` 在 ROM 地址 294,634 H1 绑定，其固定字序列：

```text
0x0041, 0x001E, 0xFFFF
```

字与顺序是此处的全部合同。其可见时长、更新节奏、调色板结果、中断同步与硬件时序保持 **未知**。重制可以实现可访问性或光敏安全呈现策略，但那是刻意现代化，而非关于原版显示结果的证据。

## 名义未使用辅助清单

**已确认静态清单：** 已接受源边界包含 ROM 地址 12,478 的代表 `tech.graphics.unused-display` 条目与 6,338 的 `tech.graphics.unused-helpers` 条目。其源文件与身份为溯源与导入兼容保留。

“Unused”是上游/源标签与源使用清单结果。它不是原始地址调用、计算分发、调试行为、修改 ROM 或注入状态下代码死亡的证明。自然运行时可达性与调用方可视效果保持 **未知**。

## 实现无关服务模型

以下为逻辑兼容模型，不是引擎类层级：

```text
DecompressionPort {
  algorithmIdentity: BASIC | STACK
  sourceAddress
  destinationAddress
  outputByteCount
}

DisplayInitializationPlan {
  orderedActions
}

SpriteLinkInitialization {
  orderedSequentialLinks
  finalLinkTerminated
}

PaletteTransitionState {
  timerInitial: 32
  blendStepDivisor: 4
  blendWeightTotal: 8
  updateQueueHandoff
  promoteBackupAtCompletion
}

SpecialSpriteRoute {
  mapSpriteId
  pointerState: BACKED | UNBACKED
  loadDispatchState: BATTLE | EXPLORATION | ABSENT
  updateDispatchState: BATTLE | EXPLORATION | ABSENT
}

FlashScript {
  orderedWords[3]: [0x0041, 0x001E, 0xFFFF]
}

GraphicsServiceInventoryEntry {
  researchRecordId
  sourceSymbol
  sourcePath
  romAddress
  acceptedRole
}
```

模型刻意没有 view-destination、摄像机更新或视差对象，也没有资源载荷集合。摄像机命令/数据与目标服务行为保持[`map-exploration`](../../contracts/map-exploration.md)；受限 VInt 更新算法保持[`map-camera-update-control-flow`](../../contracts/map-camera-update-control-flow.md)。规范特殊精灵资源记录从[special-sprite-graphics-data](../../contracts/special-sprite-graphics-data.md)消费。原版压缩字节、解码美术、调色板、瓦片图、渲染帧与截图保持私有/生成或单独许可数据。

## 跨系统分离

本合同可以把验证解码字节计数与服务事件交给资源、显示、地图、战斗、UI 或呈现系统。它不决定：

- 应选择哪个地图摄像机目标、视差因子或自动滚动状态；
- 加载哪个版权资源载荷或如何把它变换为可分发内容；
- 精灵身份、战斗动画选择、地图实体赋值或调用方准入；
- VDP 寄存器模拟、DMA 调度、帧边界、调色板外观或最终组合；
- 对话、菜单、故事排序、持久性、本地化、可访问性或平衡；
- 畸形输入恢复、调试注入策略或已接受原版语料之外的兼容行为。

那些面保持独立所有者、**未知** 或刻意产品设计。

## 保真、现代化与版权边界

兼容要求稳定服务身份、已接受寄存器角色、初始化顺序、精灵链接不变量、调色板过渡状态、规范数据合同记录上的精确特殊精灵路由分类、转移接缝身份、flash 字与清单辅助条目的溯源。

重制可以用验证导入时间转码器替换原版解码器、使用现代 GPU、批量转移、改变内部调色板表示或实现可访问性安全呈现。此类选择必须在声称兼容处保留已接受外部事实，并分别报告刻意呈现差异。

原版压缩流、解码图形、调色板、瓦片图、截图、视频捕获与其他游戏资源是私有/生成版权输入。不要提交或再分发它们。公开 fixture 与构建必须使用元数据、hash、合成数据、新编写内容或适当许可资源。

## H4 验收面

重制侧图形适配器只在自动化测试证明以下内容时声称本合同：

1. Basic 与 Stack 条目身份保持可区分，两者保留 `a0` 源、`a1` 目标与 `d0` 输出字节计数接缝，而不要求原版编解码器微实现；
2. 已接受显示初始化动作保留其文档顺序，而七个解释组不被当作独立源计数，渲染时序与硬件节奏分别测试；
3. 精灵初始化产生顺序链接与终止最终链接，而不声称未接受战斗精灵辅助语义；
4. 调色板过渡状态保留计时器 32、除数 4、权重总 8、更新队列交接身份与受限完成提升分支，而不把它们当作墙钟时序；
5. 特殊精灵路由消费 `special-sprite-graphics-data` 的规范记录并保留恰好九个完整路由 ID `247..255`、仅指针 ID `246` 与无支撑 ID `240..245`，探索槽 2 与战斗路线不同，而不独立重新验证静态资源目录；
6. 初始特殊精灵加载与动画刷新保留立即对排队转移接缝的不同，而 DMA/VInt 节奏与可见呈现保持一致性之外；
7. flash 脚本字保持恰好 `0x0041, 0x001E, 0xFFFF`，不推断可见时长；
8. 两条名义未使用辅助记录保留身份与溯源，而不断言死代码或运行时可达性；
9. 摄像机命令/view-destination 事实、源静态 VInt 更新算法、资源语料完成事实、编解码器历史/位流细节、版权载荷与独立所有者构建赋值可达性结果不被静默吸收进本合同保真声称；
10. 公开 fixture 与报告包含元数据、身份、计数、范围与 hash，而非原版压缩字节、解码美术、调色板、瓦片图或捕获帧。

H4 不要求 Mega Drive VDP 模拟器、指令相同解压器、运行时原版资源格式或帧周期一致性，除非后续显式硬件保真决定添加那些要求。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 解压条目身份与 `a0`/`a1`/`d0` ABI | **已确认静态** | `sf2-tech-graphics-static-v1`（[`tech-graphics-static-v1.json`](../../../../tests/fixtures/h2/tech-graphics-static-v1.json)）；Stack 条目也由 `sf2-special-sprite-decode-v1`（[`special-sprite-decode-v1.json`](../../../../tests/fixtures/h2/special-sprite-decode-v1.json)）绑定 | 编解码器历史、语法、复制循环、畸形输入行为与微实现是 H4 之外 |
| 显示初始化、精灵链接形状、调色板计时器/权重/队列接缝、flash 字 | **已确认静态** | `sf2-tech-graphics-static-v1`（[`tech-graphics-static-v1.json`](../../../../tests/fixtures/h2/tech-graphics-static-v1.json)） | 可见帧、VInt/CRAM-DMA 节奏、硬件时序与呈现保持 **未知** |
| 规范数据记录上的精确 `9 + 1 + 6` 特殊 ID 路由拆分 | **已确认静态** | `sf2-special-sprite-decode-v1`（[`special-sprite-decode-v1.json`](../../../../tests/fixtures/h2/special-sprite-decode-v1.json)）；目录由[special-sprite-graphics-data](../../contracts/special-sprite-graphics-data.md)拥有 | 本合同保留路线/加载器/转移接缝但不独立拥有十指针/六资源目录；强制无效/调试行为与渲染帧保持 **未知** |
| 完整原版构建赋值排除 ID `237..250` | **独立所有者已确认静态** | [Common Scripting](../../../research/common-scripting.md)，本合同之外 | 本合同不复制其 fixture 或 H4 面；强制畸形/调试/原始 RAM 行为保持 **未知** |
| 摄像机命令、区域输入与目标轴行为 | **独立所有者** | [map-exploration 合同](../../contracts/map-exploration.md)及其 H3 摄像机所有者 | `graphicsFacts.viewDestination` 此处显式不消费 |
| 源静态 VInt 目标跟随与滚动速度派生 | **独立所有者** | [map-camera-update-control-flow](../../contracts/map-camera-update-control-flow.md) | 本合同不消费 common-map 摄像机事实或更新算法 |
| 名义未使用显示/图形辅助身份 | **已确认静态清单** | `sf2-tech-graphics-static-v1`（[`tech-graphics-static-v1.json`](../../../../tests/fixtures/h2/tech-graphics-static-v1.json)） | 死代码状态、运行时可达性与调用方效果保持 **未知** |
| 渲染器架构、可访问性策略、替换资源、本地化与许可内容 | **刻意设计** | 未来产品/内容决定 | 需要独立溯源与验收 |

## 复现

```powershell
uv run sf2 h2 tech-graphics
uv run sf2 h2 special-sprites
uv run sf2 design-contracts test
uv run sf2 verify
```

生成详细输出保留在忽略的 `local/derived/tech-graphics-static.json` 与 `local/derived/special-sprite-decode.json` 下。
