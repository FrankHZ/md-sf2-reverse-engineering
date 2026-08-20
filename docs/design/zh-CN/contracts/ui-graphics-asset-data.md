# UI 图形资源数据合同

- **已确认原版结构：** 完整共享 base/菜单/提示图形语料、其异构九条目菜单表、完整连续组装图标块、仅源载荷例外、物理存储角色，以及下文描述的受限复制/高亮操作。
- **推断原版行为：** 此处不提升任何内容。
- **未知原版行为：** 运行时菜单准入与选择、动态或非标准图标可达性、无效索引、调色板选择、VInt/DMA 节奏、帧时序、渲染组合、调用方可视结果、输入、音频、本地化、可访问性与玩家面向菜单含义。
- 重制状态：实现无关 Phase 3 私有导入合同；未选择渲染器、资源格式、分辨率、动画系统、widget 工具包、替换美术或分发许可。
- 证据日期：2026-08-09
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`ui-graphics-asset-data.md`](../../contracts/ui-graphics-asset-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同定义两个专用 UI 图形轨道的静态身份与私有导入边界：

1. 九个共享 base、菱形菜单、yes/no 与主菜单资源、其指针身份与有序异构菜单表；
2. 167 个源可用图标载荷身份、精确 163 载荷 vanilla 组装、物理存储角色、高亮掩码与受限复制/高亮变换。

它还定义基于符号、地址、计数、路线元数据、存储角色、hash 与合成示例而非版权图形载荷的公开 H4 面。

它不拥有布局网格、窗口分配、菜单状态机、服务准入、控制器输入、运行时图标选择、调色板上传、转移调度、可见动画、本地化或最终呈现。相邻[UI layout data 合同](../../contracts/ui-layout-data.md) 拥有已接受布局/指针/边框/直接布局资源语料，而[window-system 合同](../../contracts/window-system.md) 拥有共享运行时窗口状态。[graphics-service 合同](../../contracts/graphics-service-state.md) 只拥有其受限解压/显示服务面。那些合同都不证明渲染菜单帧。

可执行所有者是：

- `sf2-ui-graphics-decode-v1`，位于
  [`tests/fixtures/h2/ui-graphics-decode-v1.json`](../../../../tests/fixtures/h2/ui-graphics-decode-v1.json)；
- `sf2-icon-graphics-static-v1`，位于
  [`tests/fixtures/h2/icon-graphics-static-v1.json`](../../../../tests/fixtures/h2/icon-graphics-static-v1.json)。

研究所有者是[Common Menu Engines 与 Services](../../../research/common-menus.md)与[Technical Graphics 与 Decompression Services](../../../research/technical-graphics.md)的图形段。

## 合同前证据审计

两个专用所有者于证据日期从当前 `main` 复现：

```text
sf2-ui-graphics-decode-v1
SHA256 58C9089F1DD43BE8F0EA049F8CEE4102019231336A3A6B5F7729F1D3B70B52FC
Resources 9 / DecodedBytes 23168 / MenuEntries 9 / PASS

sf2-icon-graphics-static-v1
SHA256 CF3C4E928698EEC45A1F1F21D12EBE63BEC418CF0F6F40E7820AE0B8FFE5AE1F
AvailablePayloads 167 / AssembledIcons 163 / UnassembledPayloads 4 / PASS
```

审计检查了 fixture、验证器、研究文章、生成私有输出与当前 research-index 绑定。两个 fixture 当前绑定八条索引记录。六条未关联，构成本合同精确未来关联集：

- `menus.diamond`；
- `menus.tile-pointers`；
- `tech.interfaces.ptr-s16`；
- `tech.services.resource-icon`；
- `gameflow.start.base-tiles`；
- `auxiliary.data.itemicon000`。

其他两条 fixture 链接记录已属于已接受合同，必须保持语义不变：

- `tech.graphics.stack-decompression` 仍与[graphics-service-state](../../contracts/graphics-service-state.md)关联；
- `tech.services.resource-graphics` 仍与[text-and-font-system](../../contracts/text-and-font-system.md)关联。

候选记录还携带 common menus、technical interfaces、technical services、gameflow 或 auxiliary data 的聚合证据。本合同刻意不消费 `sf2-common-menus-static-v1`、`sf2-tech-interfaces-static-v1`、`sf2-tech-services-static-v1`、`sf2-gameflow-core-static-v1` 或 `sf2-auxiliary-data-static-v1`。专用 fixture 拥有此处提升的每个声称。

审计保留以下限制：

- 九个资源与九个指针身份不是九个菜单表条目的同一域。表包含三个打包主菜单行与六个压缩菜单指针行；它不路由 base 或 yes/no 资源；
- 主菜单图标索引 6 在完整已接受菜单表中无引用。这是静态缺席，不是通用运行时不可达性的证明；
- 源树包含 167 个可用载荷，而 vanilla 组装包含 163。源可用性与组装物理存储必须保持分开；
- 存储槽 129 保留源身份 `OtherIcon002`；只有其已接受枚举名为 null。它不得被归一化为匿名或未命名资源；
- 存储槽 146 到 148 携带 `OtherIcon003` 到 `OtherIcon005`，并与法术索引 16 到 18 的算术法术槽位置重合。对应三个法术载荷是仅源例外，不是那些槽中第二组组装字节；
- 受追踪 fixture 包含元数据与 hash。压缩流、解码瓦片、图标字节、高亮掩码字节、截图与渲染捕获保持私有/生成。

## 共享 UI 资源语料

**已确认静态：** 语料包含九个资源：

| 资源类 | 计数 | 存储形式 | 已确认静态边界 |
| --- | ---: | --- | --- |
| base 瓦片 | 1 | Stack 压缩 | source/ROM 资源与指针一致性 |
| 菱形菜单瓦片 | 6 | Stack 压缩 | 每流两个四 288 字节图标转移帧 |
| yes/no 瓦片 | 1 | Stack 压缩 | 两个双图标帧；1,152 解码字节 |
| 主菜单瓦片 | 1 | 非压缩 | 七个有序 576 字节 / 18 瓦片图标记录 |

八个压缩流占用 7,848 字节并解码到 23,168 字节。非压缩 `tiles_MainMenu` 载荷占用 4,032 字节。九个资源身份、九个指针身份与独立九条目菜单表匹配已接受 source/H1/ROM 边界。其组合已接受 source/ROM 一致性核算是 11,952 字节：

```text
compressed resources  7848
main-menu payload      4032
nine pointer words       36
nine menu-table rows     36
total                 11952
```

这些项是核算分区，不是运行时转移序列。base 资源可以被多个源路径消费，但本合同不把启动、结尾字幕、调色板或时序语义指定给那些使用。

私有导入器必须保留每个资源符号、源与指针地址、压缩或非压缩存储种类、source/ROM 范围一致性、适用解码长度、指针身份与整资源元数据 hash。它必须把主菜单载荷保留为七个有序记录，而非一个匿名 4,032 字节 blob。

Stack fixture 还闭合聚合流结构：315 个命令组、3,111 个字面字、产生 8,473 个复制字的 1,869 个复制命令、32 到 46 尾位、最大复制偏移 1,904 字与最大复制长度 33 字。这些验证私有导入；它们不使解码器微实现或畸形流恢复成为本合同的一部分。

## 异构菜单表合同

**已确认静态：** `pt_tiles_Menu` 有两种格式的九个有序 longword 条目。前三个设置位 31 并各打包四个主菜单图标索引：

| 表行 | 有序打包图标索引 |
| ---: | --- |
| 0 | `[5, 1, 2, 4]` |
| 1 | `[0, 1, 2, 3]` |
| 2 | `[0, 1, 2, 4]` |

行 3 到 8 反而包含有序指针身份 `p_tiles_ItemMenu`、`p_tiles_BattlefieldMenu`、`p_tiles_ChurchMenu`、`p_tiles_ShopMenu`、`p_tiles_CaravanMenu` 与 `p_tiles_DepotMenu`。

导入器必须保留全部九个路线位置、每行格式标签、每个打包行内四个有序索引与每个压缩行内精确指针身份。它不得把表归一化为引用资源集，也不得把打包值重新解读为指针。

三个打包行引用主菜单图标索引 0 到 5。索引 6 无表引用。这只证明已接受表关系。直接调用、修改表、调试状态、畸形输入与玩家可见选择保持 **未知**。

## 主菜单记录边界

**已确认静态：** `tiles_MainMenu` 包含七个连续 576 字节记录，各含十八个 32 字节瓦片。fixture 保留资源符号、`p_tiles_MainMenu`、源路径、定义与指针所有者路径、source/指针地址、整资源与指针 hash，以及逐记录地址与 hash。

私有导入器必须独立保留索引 0 到 6，即使索引 6 无已接受表引用。它不得作为优化移除、合并或重编号索引 6。公开 fixture 可以保留记录地址、大小、hash 与引用计数，但不得保留原版瓦片字节。

该记录形状不建立动画顺序、调色板、图标含义、菜单准入或可见时序。那些保持调用方或未来呈现证据。

## 连续图标存储

**已确认静态：** 源树暴露 167 个固定 192 字节载荷，共 32,064 字节。vanilla 组装恰好包含 163 个载荷，共 31,296 连续字节：

| source/组装类 | 计数 | vanilla 组装边界 |
| --- | ---: | --- |
| 物品载荷 | 127 | 组装 |
| 法术载荷 | 30 | 组装 |
| 其他载荷 | 6 | 组装 |
| 显式仅源载荷 | 4 | 未组装 |

每个组装载荷包含六个瓦片并匹配已接受 ROM 范围。物理地址公式是 `p_Icons + storageIndex * 192`；无逐图标指针表。规范导入器必须保留全部 167 个精确源路径、192 字节文件大小与 vanilla 组装成员。对 163 个组装行它还须保留已接受源符号、物理存储索引、ROM 地址、载荷 hash 与私有字节。四个仅源例外不携带源符号、存储索引、ROM 地址、ROM 一致性或载荷 hash 的合同声称。

四个仅源例外：

- `data/graphics/icons/item/icon127.bin`；
- `data/graphics/icons/spell/icon016.bin`；
- `data/graphics/icons/spell/icon017.bin`；
- `data/graphics/icons/spell/icon018.bin`。

它们不获得借用 vanilla 地址或 ROM-一致性声称。其从组装语料的排除不证明它们死、通用不可达或与每个 alternate 构建无关。

## 物理存储角色与碰撞

**已确认静态：** 六个组装 `OtherIcon` 资源保留这些物理角色：

| 存储索引 | 源符号 | 已接受枚举身份 | 法术索引碰撞 |
| --- | --- | --- | ---: |
| 127 | `OtherIcon000` | `ICON_NOTHING` | — |
| 128 | `OtherIcon001` | `ICON_UNARMED` | — |
| 129 | `OtherIcon002` | 无已接受枚举名 | — |
| 146 | `OtherIcon003` | `ICON_JEWEL_OF_LIGHT` | 16 |
| 147 | `OtherIcon004` | `ICON_JEWEL_OF_EVIL` | 17 |
| 148 | `OtherIcon005` | `ICON_CRACKS_OVERLAY` | 18 |

槽 129 不是未命名数据：其源符号是 `OtherIcon002`。只有已接受 fixture 中的 `enumName` 为 null。同样，三个碰撞行保留组装 other-icon 身份与碰撞算术法术索引。仅源法术载荷 16 到 18 不创建共驻第二载荷。

重制可以在内部使用类型化资源引用，但其原版格式适配器必须保留每个物理槽、源符号、可选枚举身份与可选法术索引碰撞。它不得把槽 146 到 148 去重为法术资源，也不得为 `OtherIcon002` 编造枚举名。

## 复制与高亮操作

**已确认静态：** 专用所有者只闭合这些消费者局部数据变换：

- 直接图标复制产生 192 字节（六个瓦片）；
- 四个角清理字操作在字节偏移 0 与 156 应用 `0xF000`，在字节偏移 34 与 190 应用 `0x000F`；
- 高亮路径产生两个 192 字节帧，共 384 字节；
- 其受追踪操作身份是 source-bitwise-AND-mask；
- 192 字节高亮掩码与已接受函数/表身份匹配 ROM。

这些事实为私有导入载荷定义可复现字节变换。它们不定义哪个调用方选择图标、无效索引是否被准入、调色板选择、DMA 顺序、帧交替、高亮时序或渲染角像素。

## 实现无关导入模型

以下为逻辑数据合同，不是引擎类处方：

```text
UIGraphicsAssetCorpus {
  sharedResourceIdentities[9] {
    resourceId
    sourceSymbol
    storageKind: stack-compressed | uncompressed-records
    sourceAddress
    pointerIdentity
    pointerAddress
    payloadRef
    sourceRomParity
  }

  stackCompressedPayloads[8] {
    resourceRef
    compressedByteCount
    decodedByteCount
    privateCompressedBytes[]
    privateDecodedBytes[]
    payloadHash
  }

  uncompressedMainMenuPayload {
    resourceRef
    storedByteCount: 4032
    recordCount: 7
    recordByteCount: 576
    privateStoredBytes[4032]
    payloadHash
  }

  menuRoutes[9] {
    routeIndex
    routeKind: packed-main-menu-indexes | compressed-pointer
    orderedPackedIndexes[4]
    pointerIdentity
  }

  mainMenuRecords[7] {
    recordIndex
    address
    byteCount: 576
    tileCount: 18
    payloadHash
    privateTileBytes[]
    tableReferenceCount
  }

  iconSources[167] {
    sourcePath
    byteCount: 192
    assembledInVanilla
    optionalAssembledSlotRef
  }

  assembledIconSlots[163] {
    storageIndex
    sourcePath
    sourceSymbol
    optionalEnumName
    optionalSpellIndexCollision
    romAddress
    payloadHash
    privatePayloadBytes[192]
  }

  iconTransform {
    directCopyByteCount: 192
    cornerCleanOperations[4]
    highlightFrameCount: 2
    highlightOutputByteCount: 384
    highlightOperation: source-bitwise-and-mask
    privateHighlightMaskBytes[192]
    highlightMaskHash
  }
}
```

对存储槽 129，`sourceSymbol` 是 `OtherIcon002` 且 `optionalEnumName` 缺席。模型绝不用枚举名缺席作为源身份缺席。

公开形式省略每个 `private*` 字段与原版载荷。它保留每个证据受限符号、路径、地址、存储成员、维度、计数、路线种类/顺序、打包索引行、物理角色、碰撞、操作与 hash，使验证用户提供私有语料无需把版权图形变成仓库依赖。它不为四个仅源例外中的任何一个发明符号、存储索引、地址、ROM 一致性结果或 hash。

## 跨系统分离

把以下内容保持本合同之外：

- Stack 解压器实现、无效流行为与全局显示初始化；
- UI 布局、边框、指针到布局路线与窗口分配/移动/组合；
- 菱形菜单、yes/no、商店、教堂、车队、仓库、战场或主菜单输入/状态流；
- 调用方准入、已接受存储角色事实之外的枚举到图标选择、无效索引、动态/调试可达性与返回行为；
- 调色板选择、VInt、DMA、裁剪、层组合、帧节奏、音频与像素；
- 本地化、可访问性、替换美术、分辨率策略与许可分发。

聚合研究所有者可以描述相邻调用方与服务。本合同不消费其聚合 fixture，也不把那些控制流事实变成资源导入要求。

## 保真、现代化与版权边界

原版数据兼容要求在导入私有原版语料时保留资源/指针身份、压缩与非压缩存储形状、解码长度、菜单表格式与路线、全部七个主菜单记录、全部 167 个图标载荷身份的 source/组装成员、精确 163 槽 vanilla 存储、物理角色/碰撞与复制/高亮变换元数据。

重制可以刻意选择新美术、调色板、分辨率、布局、动画、输入、响应式组合、可访问性与本地化。那些选择必须与原版数据一致性分开追踪。私有导入适配器在能复现已接受元数据与 hash 并报告刻意偏差时，可以把已接受资源转码为现代格式。

原版压缩流、解码瓦片、主菜单/图标/高亮字节、截图与渲染捕获是私有/生成版权输入。不要提交或再分发它们。公开构建需要新编写或适当许可替换资源。

## H4 验收面

重制侧私有导入器或兼容适配器只在自动化测试证明以下内容时声称本合同：

1. 全部九个共享资源与指针身份、地址、存储形式与 source/H1/ROM 一致性匹配专用所有者；恰好八个 Stack 资源保留其压缩与解码计数加已接受聚合流计数器，而非压缩主菜单资源保留其 4,032 字节 / 七记录形状；
2. 九个菜单表路线保留精确顺序、三个打包对六个指针行种类、全部打包索引位置与全部六个指针身份；
3. 全部七个主菜单记录保留索引、地址、576 字节/18 瓦片形状、私有字节顺序与 hash，包括表未引用索引 6；
4. 全部 167 个源载荷身份保留精确源路径、192 字节大小与 vanilla 组装成员，恰好 163 组装与恰好四个显式列出仅源例外路径；那四个例外不需要符号、存储索引、地址、ROM 一致性或 hash；
5. 全部 163 个物理槽保留其源路径与符号、192 字节/六瓦片形状、存储索引、地址、载荷 hash、私有字节顺序与精确 127 物品/30 法术/6 其他分区；
6. 槽 127、128、129 与 146 到 148 保留其源符号、可选枚举名与可选法术索引碰撞，包括槽 129 的 `OtherIcon002`（无已接受枚举名）；
7. 直接复制、四个角清理操作、两帧/384 字节高亮输出、高亮掩码身份/hash 与 source-bitwise-AND-mask 操作匹配已接受所有者；
8. 公开 fixture 与报告只暴露元数据、hash 与合成示例，绝不暴露原版压缩流、解码瓦片、图标字节、高亮掩码、截图或渲染捕获；
9. 调用方行为、无效输入、渲染、本地化、可访问性与刻意呈现变更与静态原版数据一致性分别测试与报告。

H4 不要求现代渲染器在运行时使用 Genesis 瓦片格式或原版物理槽布局。它要求使用私有原版兼容语料时保留溯源的导入与显式偏差报告。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 九个共享资源/指针、八个 Stack 流、一个主菜单载荷与一致性核算 | **已确认静态** | `sf2-ui-graphics-decode-v1`（[`ui-graphics-decode-v1.json`](../../../../tests/fixtures/h2/ui-graphics-decode-v1.json)） | 解码器实现、转移执行、调色板、时序与像素 |
| 有序三打包/六指针菜单表与七个主菜单记录 | **已确认静态** | `sf2-ui-graphics-decode-v1`（[`ui-graphics-decode-v1.json`](../../../../tests/fixtures/h2/ui-graphics-decode-v1.json)） | 索引 6 动态可达性、调用方选择、动画与玩家面向含义 |
| 167 个源载荷、精确 163 载荷 vanilla 组装与四个仅源例外 | **已确认静态** | `sf2-icon-graphics-static-v1`（[`icon-graphics-static-v1.json`](../../../../tests/fixtures/h2/icon-graphics-static-v1.json)） | alternate 构建使用与非标准可达性保持 **未知** |
| 物理图标角色/碰撞与受限复制/角/高亮操作 | **已确认静态** | `sf2-icon-graphics-static-v1`（[`icon-graphics-static-v1.json`](../../../../tests/fixtures/h2/icon-graphics-static-v1.json)） | 调用方准入、无效索引、DMA/帧节奏、调色板与渲染输出 |
| 布局网格、运行时窗口、菜单、服务与输入 | **独立所有者** | [UI layout](../../contracts/ui-layout-data.md)、[window system](../../contracts/window-system.md)与已接受菜单/服务/输入合同 | 端到端可见 UI 保持未闭合 |
| 渲染器架构、可访问性、本地化、替换美术与可分发内容 | **刻意设计** | 未来产品/内容决定 | 需要溯源、许可与独立验收 |

## 复现

```powershell
uv run sf2 h2 ui-graphics
uv run sf2 h2 icon-graphics
uv run sf2 design-contracts test
uv run sf2 verify
```

生成详细输出保留在忽略的 `local/derived/ui-graphics-decode.json` 与 `local/derived/icon-graphics-static.json` 下。
