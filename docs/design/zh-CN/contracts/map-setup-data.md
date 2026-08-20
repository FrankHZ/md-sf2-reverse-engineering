# 地图配置数据合同

- **已确认原版结构：** 一个有序地图配置路由表、已接受地图 ID 域 `0..78` 上的 64 个地图行与 66 个标志行、130 个有序配置引用、126 个指针表身份、四个默认指针别名、126 个六槽配置定义，以及下文描述的受限实体列表访问器时间线。
- **推断原版行为：** 此处不提升任何内容。下文的类型化路线/定义模型是实现无关导入模型，不是原版引擎暴露那些类型的证据。
- **未知原版行为：** 个别路线的自然或故事驱动可达性、标志生命周期与持久性含义、调用方可视实体人口与重载行为、事件或初始化效果、过渡状态、呈现时序、畸形输入恢复与替换或修改策略。
- 重制状态：实现无关 Phase 3 私有导入合同；未选择场景图、故事状态模型、事件运行时、持久性方案、编辑器格式、替换内容策略或引擎。
- 证据日期：2026-08-14
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`map-setup-data.md`](../../contracts/map-setup-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签按 R1 使用固定中文译法；源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同定义原版地图配置路由语料的静态存储、引用、溯源与私有导入边界：

1. 有序 `MapSetups` 路由表序列化；
2. 64 个地图行、66 个有序标志行及其 130 个指针引用；
3. 126 个唯一标识六指针配置定义与四个路线别名；
4. 每个配置定义中的六个有序目标槽身份；
5. 受限 `GetMapSetupEntityList` 身份与源时间线；
6. 不再分发完整路线或指针图的公开聚合投影。

此处消费的唯一可执行所有者是 fixture id `sf2-map-setup-static-v1`，位于 [`tests/fixtures/h2/map-setup-static-v1.json`](../../../../tests/fixtures/h2/map-setup-static-v1.json)，由 [`src/sf2tool/h2/map_setup.py`](../../../../src/sf2tool/h2/map_setup.py) 实现。其所属研究文章是[Map Data Inventory 与 Binary 一致性](../../../research/map-data-inventory.md)。

消费 fixture 面刻意限定到：

- `function.GetMapSetupEntityList` 与 `table.MapSetups` 身份/地址溯源；
- `expected.summary`；
- `expected.sourceFacts.pointerLayout`；
- 只有 `expected.sourceFacts.selector.mapTableEndWord` 与 `expected.sourceFacts.selector.mapRowEndWord` 作为序列化终止符身份；
- `expected.aliasFlagRoutes`；
- 专用验证器生成的路由序列化、源顺序、表所有权、source/H1/ROM 一致性与受限溯源事实；
- 紧凑 `expected.selectionCases` 只作为公开跨所有者见证元数据。

本合同**不**消费其余选择器行为字段 `defaultPointerLoadedBeforeFlags`、`allFlagRowsAreScanned`、`setFlagOverwritesCandidate`、`winner` 或 `missingMapResult`。那些字段保持已接受 map-exploration 选择器所有者。它也不消费或重述 `expected.sourceFacts.dispatch`：初始化、区域、物品、实体与区域描述分发器规则保持其已接受运行时与专用分发器所有者。选择器 H3 fixture 与实体人口 H3 fixture 同样排除。

## 合同前证据审计

专用所有者从已接受基线复现：

```text
sf2-map-setup-static-v1
SHA256 DFD9BE7323894A2738FE5D00C4197BD0A36CA825CDB787A2391B31EFF76DF438
MapRows 64 / FlagRows 66 / PointerTables 126 / PointerSlots 756 / PASS
```

fixture 直接绑定恰好三条 research-index 记录：

| 记录 | 身份 | 当前设计所有者 | 此处边界 |
| --- | --- | --- | --- |
| `map.data.mapsetups` | `MapSetups` | 无 | 未来关联 |
| `map.setup.entity-list` | `GetMapSetupEntityList` | 无 | 未来关联 |
| `map.setup.selector` | `GetCurrentMapSetup` | `map-exploration` | 不变独立所有者 |

因此精确未来关联集只有 `map.data.mapsetups` 与 `map.setup.entity-list`。`map.setup.selector` 只与[Map 与 Exploration](../../contracts/map-exploration.md)关联。

`map.setup.entity-list` 记录还携带 `sf2-entity-population-reload-runtime-v1` 的已接受 H3 证据。该 H3 证据此处显式排除。共享索引记录不授权本合同声称人口、重载、调用方、运行时或呈现语义。

没有 `map.data.*` 兄弟、事件分发器、选择器、实体列表载荷、初始化程序、地图资源、故事、持久性或呈现记录获得本合同。宽 `sf2-map-data-static-v1` 与连接 `sf2-canonical-map-import-v1` fixture 不是本文档的可执行所有者。

## 有序路线序列化

**已确认静态：** `MapSetups` 在 ROM `0x4F6E2`（`325346`）开始。其源码形状序列化语法：

```text
MapSetupRoutingTable := MapRow* 0xFFFF
MapRow              := mapWord defaultPointer FlagRow* 0xFFFD
FlagRow             := flagWord setupPointer
```

字与指针是大端源字段。每个地图行贡献八个结构字节：两字节地图字、四字节默认指针与两字节行终止符。每个标志行贡献两字节标志字与四字节指针。因此完整字节计数闭合为：

```text
64 * 8 + 66 * 6 + 2 = 910 bytes
```

最后两字节是表终止符。它们不是第 65 个地图行或另一标志行。

已接受路线面包含：

| 面 | 已确认计数 |
| --- | ---: |
| 地图行 | 64 |
| 标志行 | 66 |
| 有序路线引用 | 130 |
| 路由字节 | 910 |
| 完整路由表 source/ROM 一致性 | 1 |

`missingMapCount` 值 15 限定到验证器已接受地图 ID 域 `0..78`（含 79 个 ID）。它意味着该精确域中十五个 ID 没有路由行。它不是通用地图命名空间、修改输入声称或任何路径在运行时自然不可达的证明。

私有导入器必须保留地图行顺序、每个地图行内标志行顺序、默认与标志指针身份与两个终止符身份。它不得把源归一化为无序字典、因指针重复丢弃路线或从标志数字推断故事优先级。

本合同保留有序路线数据，而非选择器所有权。已接受 `GetCurrentMapSetup` source/H3 选择行为（包括十个受追踪见证用例的含义）保持[Map 与 Exploration](../../contracts/map-exploration.md)。见证用例可以作为已受追踪公开元数据暴露，但它们不是 H4 数据保真要求，也不在此创建第二条选择器声称。

## 别名与定义拓扑

**已确认静态：** 130 个有序路线引用解析到 126 个唯一标识指针表定义。四个标志行指回其地图已具名的默认定义：

| 地图 | 标志 | 引用定义 |
| ---: | ---: | --- |
| 7 | 702 | `ms_map7` |
| 33 | 783 | `ms_map33` |
| 33 | 22 | `ms_map33` |
| 40 | 507 | `ms_map40` |

算术闭合为 `130 - 126 = 4`。导入器必须把那些保留为指向既有定义身份的四个不同有序路线行。它不得把它们删除为冗余，也不得把它们物化为四个发明定义。

计数 126 意味着已接受源与地址图中的唯一指针表身份/定义。它不断言全部 24 字节表内容两两不同。内容相等（若存在）不会擦除源符号、地址、路径或引用身份。

每个定义是 24 字节：六个有序四字节指针。因此完整指针表存储边界闭合为：

```text
126 * 6 = 756 pointer slots
126 * 24 = 3,024 pointer bytes
```

全部 126 个定义范围匹配其已接受 ROM 字节。一致性计数是逐定义 source/ROM 关系，不是发布指针、源路径或目标图的许可。

## 六槽定义形状

**已确认静态：** 每个配置定义有相同有序指针布局：

| 槽 | 字节偏移 | 源身份 |
| ---: | ---: | ---: |
| 0 | 0 | `entities` |
| 1 | 4 | `entityEvents` |
| 2 | 8 | `zoneEvents` |
| 3 | 12 | `areaDescriptions` |
| 4 | 16 | `itemEvents` |
| 5 | 20 | `initFunction` |

这些名称与偏移是静态源身份。它们定义引用位置，而非其目标的运行时含义、副作用、持久性、呈现或可达性。

私有导入器必须保留全部 126 个有序定义身份与全部 756 个目标引用，包括每个定义的源符号、源路径、ROM 地址与目标符号/地址。目标保持对独立拥有资源的引用；它们不被复制进每个定义。

完整定义/引用图是私有的。公开证据可以保留六个槽名与偏移、计数、字节总计、一致性总计、fixture 溯源与四条已受追踪别名行，但不得发布 126 个符号/地址/源路径或 756 个目标值。

## 实体列表访问接缝

**已确认静态身份：** `GetMapSetupEntityList` 在 ROM `0x47790`（`292752`）开始。fixture 绑定该地址。固定源直接审查只保留该精确时间线：

1. 分支到子程序 `GetCurrentMapSetup`；
2. 把所选 `a0` 处的字与 `-1` 比较；
3. 相等时保持 `a0` 不变并返回；
4. 否则把偏移 0 的 longword 加载进 `a0`；
5. 返回。

这建立所选定义、void 标记与槽 0 之间的受限关系。它不导入 `GetCurrentMapSetup` 选择行为、不把 `a0` 描述为现代 API、不证明任何调用方准入，也不定义后续代码用返回实体列表指针做什么。

底层实体列表语料保持[Map Entity Data](../../contracts/map-entity-data.md)。运行时人口与重载保持[Map 与 Exploration](../../contracts/map-exploration.md)及其已接受 H3 所有者。本合同不消费那些 fixture，也不把其结果变成 H4 要求。

## 实现无关导入模型

最小完整逻辑导入把路线行、指针表定义、目标引用与公开元数据分开：

```text
MapSetupCorpus {
  privateRoutes[64]: PrivateMapSetupRoute
  privateDefinitions[126]: PrivateMapSetupDefinition
  publicSummary: MapSetupPublicSummary
}

PrivateMapSetupRoute {
  sourceOrder
  mapId
  defaultDefinitionRef
  flagRows[]: PrivateMapSetupFlagRoute
}

PrivateMapSetupFlagRoute {
  sourceOrder
  flagId
  definitionRef
}

PrivateMapSetupDefinition {
  definitionId
  privateSourceSymbol
  privateSourcePath
  privateRomAddress
  privateTargets[6] {
    slotIdentity
    byteOffset
    privateTargetSymbol
    privateTargetAddress
  }
}

MapSetupPublicSummary {
  fixtureId = "sf2-map-setup-static-v1"
  routingTableAddress = 325346
  entityListAccessorAddress = 292752
  acceptedMapIdDomain = 0..78
  mapRowCount = 64
  flagRowCount = 66
  missingMapCount = 15
  routeReferenceCount = 130
  pointerTableIdentityCount = 126
  aliasFlagRouteCount = 4
  pointerSlotsPerDefinition = 6
  pointerSlotCount = 756
  routingByteCount = 910
  pointerTableByteCount = 3024
  routingParityCount = 1
  pointerTableParityCount = 126
  pointerLayout[6]
  aliasMetadata[4]
  compactCrossOwnerWitnessCases[10]
  fixtureProvenance
}
```

该模型是 **推断** 的安全现代表示。它不是原版程序把路线与定义当作类型化对象、向脚本暴露它们或给其源名指定玩家面向含义的证据。

公开投影必须省略完整 130 引用路线图、全部 126 个定义符号、地址与源路径、全部 756 个目标值、原始路由字/字节与底层资源内容。十个紧凑见证用例保持跨所有者元数据，不得用于声称本合同下的选择器保真。

## 跨系统分离

本合同不拥有：

- `GetCurrentMapSetup` 运行时选择、标志求值、H3 用例或调用方可视返回行为，仍归[Map 与 Exploration](../../contracts/map-exploration.md)；
- 实体列表记录、地图精灵身份、初始放置或呈现，仍归[Map Entity Data](../../contracts/map-entity-data.md)及其所有者；
- 区域、物品、实体或初始化分发、事件效果、工作布局变更、地图生命周期、重载或过渡行为；
- 区域描述匹配、文本/函数载荷或显示路由，仍归[Map Area-Description Routing](../../contracts/map-area-description-routing.md)；
- standalone map-script 程序与解释器行为，仍归[Standalone Map-Script Program Data](../../contracts/standalone-map-script-program-data.md)、对话所有者与 map-exploration；
- `SwitchMap`、`CheckBattle` 或存档点辅助，仍归[Map Entry Routing State](../../contracts/map-entry-routing-state.md)；
- block/layout、调色板、瓦片集与地图精灵载荷，仍归[Map Layout Data](../../contracts/map-layout-data.md)、[Map Palette Data](../../contracts/map-palette-data.md)、[Map Tileset Data](../../contracts/map-tileset-data.md)与[Map-Sprite Graphics Data](../../contracts/map-sprite-graphics-data.md)；
- 故事推进、标志持久性、存档/读档行为、UI、对话呈现、帧时序或玩家面向含义；
- `sf2-map-data-static-v1`、`sf2-canonical-map-import-v1` 与上文排除的每个聚合或运行时 fixture。

[Story Progression](../synthesis/story-progression.md)与[Map Design Principles](../synthesis/map-design-principles.md) 综合可以引用这些静态事实，同时保留其证据边界。两者都不得从路线顺序、标志数字或非 null 目标推断故事时间线或自然可达性。

## 判断边界

### 已确认

- 精确 `MapSetups` 与 `GetMapSetupEntityList` 身份/地址；
- 有序路线序列化、`0xFFFD` 行与 `0xFFFF` 表终止符与精确 910 字节总计；
- 产生 130 个有序引用的 64 个地图行与 66 个标志行；
- 只在已接受 `0..78` 地图 ID 域内无路由行的十五个地图 ID；
- 126 个唯一标识六指针定义、四个精确别名行、756 个槽与 3,024 指针字节；
- 六个有序槽身份与偏移；
- 一个路由表与 126 个指针表 source/H1/ROM 一致性关系；
- 受限实体列表访问器时间线与公开/私有证据投影。

### 推断

- 上文显示的类型化私有导入模型，无原版引擎抽象或玩家面向含义声称。

### 未知

- 个别路线与定义的自然、调试、畸形或故事驱动可达性；
- 标志生命周期、持久性、优先级含义或叙事解读；
- 调用方可视实体列表使用、人口/重载顺序与运行时变更；
- 事件与初始化效果、地图过渡、呈现、文本、立绘与帧时序；
- 畸形、截断、重复、注入、修改或替换输入行为；
- 编辑器 UX、验证策略、引擎表示与刻意内容替换规则。

## H4 验收合同

重制面向 H4 私有导入器只在能以下情况时通过本合同：

1. 识别 `sf2-map-setup-static-v1`、固定基线、`MapSetups` 与 `GetMapSetupEntityList` 溯源；
2. 用已接受序列化、终止符与 910 字节私有源边界保留恰好 64 个有序地图行与 66 个有序标志行；
3. 保留恰好 130 个有序定义引用，而不把路线行替换为无序图或丢弃重复目标；
4. 保留 126 个不同源定义身份、每定义六个有序目标槽、756 个总槽与 3,024 指针字节，而不声称两两内容唯一；
5. 把四个精确默认定义别名保留为不同路线行，并拒绝意外别名压平或发明定义；
6. 复现六个槽身份/偏移，并让每个私有目标引用保持其原定义与槽；
7. 私有验证一个路由表与 126 个定义 source/H1/ROM 一致性关系；
8. 保留受限实体列表访问器时间线，而不声称选择器、调用方、人口或重载保真；
9. 把完整路线、符号、地址、源路径、目标值、原始字/字节与底层资源内容保持公开 fixture 与报告之外；
10. 通过独立所有者报告选择器行为、事件/初始化执行、运行时生命周期、持久性、故事、呈现、畸形输入与替换策略，或作为 **未知**。

Source/H1/ROM 字节、符号、指针值与 ROM 地址只是私有导入与往返验证输入。该验证后，合规重制运行时可以使用引擎原生引用，不需要复现 Mega Drive 地址空间、大端指针存储或原版内存表布局。该实现自由不削弱上文私有保真要求，也不允许丢失验证它们所需证据。

十个紧凑选择用例不是本数据合同的 H4 要求。选择器保真通过既有 map-exploration 所有者接受。H4 可以为导入器测试使用合成路线图，但公开测试不得复现私有原版图。

## 证据矩阵

| 合同面 | 证据标签 | 精确所有者 | 保留边界 |
| --- | --- | --- | --- |
| 路由表身份与序列化 | **已确认静态** | `sf2-map-setup-static-v1`；[fixture](../../../../tests/fixtures/h2/map-setup-static-v1.json) | 64/66 行、130 个有序引用、终止符、910 字节；完整路线图私有 |
| 定义拓扑 | **已确认静态** | 同一 fixture；[map-data 研究](../../../research/map-data-inventory.md) | 126 个身份、各 6 槽、756 个槽、3,024 字节、四个别名；无字节内容唯一声称 |
| 实体列表访问接缝 | **已确认静态身份/源时间线** | 同一 fixture 加固定 `mapsetupsfunctions_1.asm` | 仅受限调用/测试/加载/返回顺序；H3 人口/重载证据排除 |
| 紧凑选择用例 | 跨所有者公开见证元数据 | 同一受追踪 fixture；[Map 与 Exploration](../../contracts/map-exploration.md)拥有选择器行为 | 不是 H4 要求，也不是第二条选择器声称 |
| 分发行为 | 排除 fixture 面 | `expected.sourceFacts.dispatch` 与专用分发器所有者 | 此处不消费 init/zone/item/entity/area-description 行为 |
| 完整路线/目标内容 | 私有原版输入 | 忽略 `local/derived/map-setup-static.json` | 符号、地址、路径、指针与原始字节不公开 |
| 故事、持久性、生命周期与呈现 | 独立所有者 / **未知** | 已接受相邻合同或未来受限证据 | 静态引用拓扑不证明运行时或玩家面向行为 |

## 开放问题

1. 重制导入器应对重复地图行、畸形终止符、未解析目标或刻意修改配置定义使用什么显式验证策略？
2. 哪些路线与配置身份在普通游玩、调试路径或原版异常状态中自然可达，而不把静态在场与故事时间线混淆？
3. 未来运行时证据能否在不暴露私有路线与目标图的情况下闭合实体列表访问与配置目标生命周期？
4. 哪些源槽身份可以成为稳定编辑器面向概念，而不导入原版分发器副作用或呈现假设？

## 复现

```powershell
uv run sf2 h2 map-setup
uv run sf2 design-contracts test
uv run sf2 research-index test
```

生成完整输出保留在忽略的 `local/derived/map-setup-static.json` 下。公开验收使用受限元数据与溯源，而非完整原版路线、指针、符号、地址、源路径或目标内容。
