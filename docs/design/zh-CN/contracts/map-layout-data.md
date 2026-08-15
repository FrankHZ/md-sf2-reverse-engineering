# 地图布局数据合同

- **已确认原版结构：** 完整 77 所有者 block/layout 载荷语料、其 79 个地图引用、两个共享所有者别名、解码 block 与 layout 维度、聚合解码器命令族计数，以及下文描述的 source/ROM parity 边界。
- **推断原版行为：** 此处不提升任何内容。
- **未知原版行为：** 编解码器意图、畸形流恢复、动态或修改载荷准入、碰撞与可通过性含义、事件驱动工作布局变更、重载与存档持久性、地图过渡行为、渲染 VDP parity、呈现时序与玩家面向地图含义。
- 重制状态：实现无关 Phase 3 私有导入合同；尚未选择渲染器、碰撞模型、导航表示、资源格式、替换地图集或分发许可。
- 证据日期：2026-08-09
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`map-layout-data.md`](../../contracts/map-layout-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同定义原版 blockset 与地图布局语料的静态数据与私有导入边界：

1. 77 个 block 载荷所有者与 77 个配对 layout 载荷所有者；
2. 79 个到那些所有者的有序地图引用；
3. 精确两个共享所有者关系；
4. 解码 block 与 layout 形状、语料总计、索引边界与聚合解码器族计数器；
5. 每个拥有压缩载荷的 source/ROM parity。

可执行所有者是 `sf2-map-layout-decode-v1`，位于 [`tests/fixtures/h2/map-layout-decode-v1.json`](../../../../tests/fixtures/h2/map-layout-decode-v1.json)。研究所有者是[Common Map Engine](../../../research/common-maps.md)与[Map Content Tables 与 Binary Payload Parity](../../../research/map-content.md)。

fixture 还绑定 `LoadMapLayoutData` 与 `LoadMapBlocks` 的身份。两个身份都是该数据边界的一部分，但 `LoadMapBlocks` 不创建第二条 research-index 关联。唯一关联记录保持 `maps.map-layout`。

既有[map-exploration 合同](../../contracts/map-exploration.md) 是更高层消费者。它拥有已接受地图构建、配置、事件、工作布局、实体、移动与运行时交接规则。本合同不复制那些规则、不改变该合同的 fixture 注册，也不代其添加关联。

## 合同前证据审计

专用所有者于证据日期从当前 `main` 复现：

```text
sf2-map-layout-decode-v1
SHA256 EF5E7C493BC50AFDC64197D51350B2E69D5C9E33870FF56565694F79F39E3691
PayloadPairs 77 / DecodedBlocks 19771 / DecodedLayoutWords 315392 / PASS
```

审计检查了 fixture、验证器、所属研究文章、忽略私有输出与当前 research-index 绑定。`sf2-map-layout-decode-v1` 恰好绑定一条索引记录：

- `maps.map-layout`。

该记录唯一且当前未关联。它是本合同的精确未来关联集。不需要聚合 fixture 或第二条记录。

审计还保留四个分母边界：

- 77 是唯一 block/layout 载荷所有者数，不是地图引用数；
- 79 是地图引用数，不是独立存储载荷对数；
- 每个所有者贡献一个压缩 block 载荷与一个压缩 layout 载荷；
- 地图 24 与 46 是对既有所有者对的引用，不是额外载荷所有者。

受追踪证据包含聚合元数据与 parity 结果。原版压缩载荷、解码 block 字、解码 layout 字、逐载荷 hash 与渲染捕获保持私有/生成。

## 所有者与引用图

**已确认静态：** 原版语料有服务 79 个地图引用的 77 个独立存储载荷对。精确共享所有者关系：

| 引用地图 | Block 所有者 | Layout 所有者 |
| ---: | ---: | ---: |
| 24 | 23 | 23 |
| 46 | 7 | 7 |

每个其他已接受地图引用解析到其自身所有者对。导入器必须保留引用图，而非把它扩展为 79 个匿名副本或缩减为 77 个资源的无序集。

Block 与 layout 所有权保留为两个显式引用，即使两个别名当前在每行指向同一所有者地图。数据模型不得仅因原版两个别名一起移动就推断 block 与 layout 所有权在修改导入中永不分叉。

别名图是存储身份事实。它不建立运行时缓存身份、共享可变工作内存、事件持久性或过渡行为。那些保持独立所有者或 **未知**。

## 解码 Blockset 语料

**已确认静态：** 77 个 block 载荷解码到 19,771 个有序 block。每个 block 包含 3×3 网格的九个 16 位瓦片字，跨完整语料产生 177,939 个 block 字。单个载荷包含 22 到 666 个 block。

公开 fixture 保留这八个聚合 block 解码器族计数：

| Fixture 字段 | 聚合计数 |
| --- | ---: |
| `absoluteNewFlags` | 9,165 |
| `repeat` | 53,813 |
| `adjacent` | 23,792 |
| `relativeSameFlags` | 19,211 |
| `rightHistory` | 25,076 |
| `absoluteSameFlags` | 19,545 |
| `bottomHistory` | 11,765 |
| `relativeNewFlags` | 13,493 |

这些是完整语料聚合解码器事实。它们不是逐地图直方图、玩法类别、编解码器设计意图、性能要求或每个畸形流都有定义结果的证明。重制只有在私有导入从已接受原版输入复现相同有序 block 字时才可以使用不同内部解码器。

Block 顺序与每个 16 位字是私有导入身份的一部分。导入器不得去重相等 block、重排 block、丢弃未知字位或用无法精确往返的具名字段替换原始字。

## 解码 Layout 语料

**已确认静态：** 每个拥有 layout 精确解码到排列为 64 行乘 64 列的 4,096 个有序 16 位字。因此 77 个 layout 包含 315,392 个字或 630,784 解码字节。

公开 fixture 保留这六个聚合 layout 解码器族计数：

| Fixture 字段 | 聚合计数 |
| --- | ---: |
| `nextBlock` | 19,540 |
| `upperHistory` | 7,938 |
| `copyLeft` | 102,770 |
| `leftHistory` | 28,303 |
| `copyUpper` | 145,135 |
| `literal` | 11,706 |

六个计数总和等于完整 315,392 字解码 layout 语料。该算术只闭合聚合输出分类。它不发布哪个命令族产生任何特定地图位置，也不给字指定碰撞、层、地形或呈现含义。

每个解码 layout block 索引都在其配对 blockset 内。最大已接受索引为 665，对最大已接受 blockset 大小 666。这是语料级有效性边界，不是对修改或畸形输入的通用解码器策略。

私有导入必须保留每个原始字的 64×64 位置与其配对 block 所有者身份。它必须拒绝或显式报告改变字顺序、改变所有者引用或产生越界 block 索引的原版保真导入。

## 压缩载荷与 Parity 边界

**已确认静态：** 全部 77 个压缩 block 载荷与全部 77 个压缩 layout 载荷匹配其已接受 ROM 范围。因此公开 parity 计数器：

| 载荷类 | 所有者计数 | source/ROM parity 计数 |
| --- | ---: | ---: |
| 压缩 block 载荷 | 77 | 77 |
| 压缩 layout 载荷 | 77 | 77 |

受追踪 fixture 不再分发压缩字节、解码字或逐载荷 hash。私有导入器可以在用户拥有的本地输入边界下保留那些值以证明精确往返，但公开报告必须只暴露聚合计数、维度、范围、别名元数据、parity 结果、fixture 溯源与非内容诊断。

Parity 证明所选源载荷匹配固定原版 ROM。它不证明渲染等价、硬件时序、视觉正确性、事件行为或任何瓦片或布局位的预期含义。

## 实现无关导入模型

最小完整逻辑导入保持拥有存储与引用分离：

```text
MapLayoutCorpus {
  blockOwners[77]: BlockPayloadOwner
  layoutOwners[77]: LayoutPayloadOwner
  mapReferences[79]: MapLayoutReference
  publicSummary: MapLayoutPublicSummary
}

BlockPayloadOwner {
  ownerMapId
  privateCompressedBytes
  privateCompressedHash
  privateSourceRange
  blocks[]: Block3x3
}

LayoutPayloadOwner {
  ownerMapId
  privateCompressedBytes
  privateCompressedHash
  privateSourceRange
  width = 64
  height = 64
  privateWords[4096]
}

Block3x3 {
  privateWords[9]
}

MapLayoutReference {
  mapId
  blockOwnerRef
  layoutOwnerRef
}

MapLayoutPublicSummary {
  payloadPairCount = 77
  mapReferenceCount = 79
  aliasReferences[2]
  decodedBlockCount = 19771
  decodedBlockWordCount = 177939
  decodedLayoutWordCount = 315392
  decodedLayoutByteCount = 630784
  blockCountRange = 22..666
  maximumLayoutBlockIndex = 665
  blockCommandFamilyTotals[8]
  layoutCommandFamilyTotals[6]
  blockPayloadParityCount = 77
  layoutPayloadParityCount = 77
  fixtureProvenance
}
```

模型是逻辑的，不是引擎 API。`private*` 字段保持用户导入与验证过程本地。可分发实现可以用项目拥有地图替换它们，但必须保留引用/所有者区分，并把刻意内容替换记录为意图内容替换，而非把替换数据呈现为提取原版内容。

模型刻意有独立所有者集合与引用记录。把解码数据直接存储在 79 个地图行内会擦除两个原版别名。只存储两个别名会擦除其他 77 个引用身份。两层都需要。

## 跨系统分离

本合同不拥有：

- 附加到地图定义的调色板或五个瓦片集槽；
- 区域、标志、踩踏、屋顶、传送、物品、动画或配置记录；
- 工作布局副本、宝箱状态、重置/重载行为或存档持久性；
- 碰撞、可通过性、寻路、实体放置、战斗边界或地形语义；
- map-script 选择、故事可达性、过渡控制或当前地图状态；
- 渲染器构建、VDP 上传、滚动、层、动画节奏或最终像素；
- 畸形流恢复、mod 准入、编辑器 UX、可访问性或平衡。

[map-exploration 合同](../../contracts/map-exploration.md)与[map-design 综合](../synthesis/map-design-principles.md) 可以消费该静态语料，同时保留其自身证据与 **未知** 边界。它们不得用聚合命令计数替代运行时或呈现证据。

## 判断边界

### 已确认

- `LoadMapLayoutData` 与 `LoadMapBlocks` 的精确函数身份与地址；
- 服务 79 个引用的 77 个唯一载荷所有者；
- 别名 24 到 23 与 46 到 7 的两个所有者引用；
- 完整 block/layout 解码维度、总计、计数范围与最大索引；
- 精确八与六聚合解码器族计数器；
- 完整 77 加 77 source/ROM parity。

### 推断

- 本合同不提升任何内容。

### 未知

- 原版编解码器为何选择其命令族或历史规则；
- 截断、畸形、注入或修改压缩流的行为；
- 跨别名地图引用的运行时缓存与可变共享；
- 碰撞、可通过性、事件、持久性、过渡与故事含义；
- VDP 可见渲染、动画、时序与玩家面向呈现。

## H4 验收合同

重制面向 H4 适配器只在能以下情况时通过本合同：

1. 精确识别可执行 fixture 为 `sf2-map-layout-decode-v1` 并保留其溯源；
2. 保留 77 个 block 所有者、77 个 layout 所有者与 79 个独立地图引用，而不压平或发明所有者；
3. 保留地图 24 到所有者 23 与地图 46 到所有者 7 的两个引用；
4. 从私有已接受输入复现 19,771 个有序 3×3 block、177,939 个有序 block 字与 22 到 666 的逐所有者计数范围；
5. 复现 77 个有序 64×64 layout、315,392 个 layout 字、630,784 解码字节与最大 block 索引 665，每个引用在范围内；
6. 复现精确八 block 与六 layout 聚合解码器族计数器，而不把它们呈现为逐地图统计或玩法语义；
7. 本地验证全部 77 block 与 77 layout 压缩载荷 parity 关系；
8. 通过合成或私有输入测试检测别名压平、引用重编号、字重排、丢失原始位与越界索引；
9. 把压缩字节、解码字、逐载荷 hash 与渲染输出保持公开 fixture 与报告之外；
10. 把碰撞、事件、持久性、过渡、渲染与时序报告为独立合同，而非此处隐含成功条件。

H4 实现可以急切、惰性或导入构建期间解码。那些选择只在保留完整私有身份图与公开不披露边界时合规。

## 证据矩阵

| 合同面 | 证据标签 | 精确所有者 | 保留边界 |
| --- | --- | --- | --- |
| 函数身份 | **已确认静态** | `sf2-map-layout-decode-v1`；[fixture](../../../../tests/fixtures/h2/map-layout-decode-v1.json) | 仅 `LoadMapLayoutData` 与 `LoadMapBlocks` 身份/地址；无第二条记录关联 |
| 所有者/引用图 | **已确认静态** | 同一 fixture；[common-map 研究](../../../research/common-maps.md) | 77 所有者、79 引用与两个精确别名；无可变运行时共享声称 |
| 解码 blockset | **已确认静态** | 同一 fixture；[map-content 研究](../../../research/map-content.md) | 19,771 个 3×3 block、177,939 字、22..666 范围；原始字保持私有 |
| 解码 layout | **已确认静态** | 同一 fixture；[map-content 研究](../../../research/map-content.md) | 77 个 64×64 layout、315,392 字、630,784 字节、最大索引 665；无碰撞或层含义 |
| 解码器族总计 | **已确认静态** | 同一 fixture | 精确聚合 8 加 6 计数器；无逐地图分类、编解码器意图或畸形输入行为 |
| 压缩 parity | **已确认静态** | 同一 fixture | 77 block 加 77 layout source/ROM 匹配；无公开载荷或 hash |
| 地图构建与变更 | 独立所有者证据 | [map-exploration 合同](../../contracts/map-exploration.md) | 运行时加载阶段、事件、工作布局状态、重置与持久性此处不复制 |
| 渲染与呈现 | **未知** | 无消费运行时/呈现 fixture | VDP parity、滚动、动画、可见时序与最终像素保持开放 |

## 开放问题

1. 未来分组呈现轨道能否在不发布原版图形或布局内容的情况下比较私有解码地图与完整 VDP 可见结果？
2. 别名引用是否共享任何运行时缓存或可变状态，还是只共享不可变源存储？
3. 哪些畸形流用例需要显式重制拒绝策略而非未指定解码器行为？
4. 哪些原始 layout 字位最终可以获得稳定实现无关名称而不丢失往返保真？
