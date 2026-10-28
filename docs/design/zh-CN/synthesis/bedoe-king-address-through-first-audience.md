# Bedoe 国王讲话与首次觐见

> 本文是[规范英文原文](../../synthesis/bedoe-king-address-through-first-audience.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

国王讲话编排关于 devils 的报告、下令警戒并遣散七名卫兵。首次觐见让 Peter 和 Bowie 与 Luke、国王及一名 birdwoman 交谈：介绍后是 Zeon 警告和拜见 Volcanon 的邀请。这是两个独立准入的场景。其对话提供从 [Bedoe 初始卫兵遭遇](battle12-outcomes-through-bedoe-entry.md)接续的候选内容连接，但不强制将该遭遇或国王讲话作为觐见前提。

**已确认静态内容与控制流：**已接受的[研究所有者](../../../research/bedoe-king-address-through-first-audience.md)支持下文请求、行动者和调用方边界。**推断：**这些场景构成英文内容指南的候选接续。**未知：**自然楼层访问及顺序、传入状态、服务完成、可见表现和可用输入。本指南不增加重制能力或产品决定。全部交谈概述都是有归属的台词，不是独立确立的 Grans、devils 或 Volcanon 历史。

止于首次觐见调用方及条件控制恢复。独立的 [Bedoe 至 Volcanon 觐见指南](bedoe-to-volcanon-audience.md)继续解释选定神殿请求、交谈、Peter 成员资格/AI/跟随变化与条件 Map11 控制。该连接保留自身准入及自然路线限制。`cs_5766C` 与之后国王选择、Skreech、后续招募、服务/楼层清单、Battle13 及未合并后继发现仍不在本文范围内。Fnnn 表示标志 nnn；坐标为地图瓦片，另注单位者除外。

## 两次独立听取国王讲话的机会

**已确认选择：**Map10 默认配置提供下列有序 zone 行：

| 请求谓词 | 调用方与准入 |
| --- | --- |
| X29，任意 Y | `Map10_ZoneEvent0`：F720 未设则运行讲话 `cs_573EC` 后设置 F720；已设则返回。 |
| X33，任意 Y | `Map10_ZoneEvent1`：F721 未设则运行觐见 `cs_574A6` 后设置 F721；已设则返回。 |
| X31，Y53 | 同一觐见调用方与 F721 门禁。 |
| 不匹配 | 默认调用方返回，不运行任一场景。 |

表使用字节坐标、独立 255 通配符及首匹配行。被自身标志抑制的调用方不继续落入另一行。两调用方都不测试 F719、另一调用方的位、成员资格、HP 或行动者存在。两位均未设时允许任一选定调用方；一位已设仅抑制自己的场景；两位均已设则抑制两者。两个觐见行仍共享一个脚本调用位置。这确立传入状态中的独立准入，不是演示先到觐见的路线。

F722 选择另一配置，替换 zone 表，包括上述三行。其后期 zone 不在此分析。Zone 分发再次解析当前配置，不从对话、记忆中的场景顺序或仅 F719 推导登场实体或 zone 表。

坐标谓词仅是准入的一部分。普通移动的 zone 产生方要求选定布局属性与 `$3C00` 掩码结果等于 `$1400`。它写入待处理事件 type 6，以及由目的地加尝试移动偏移再除以 384 得出的字 X/Y 参数。`WaitForEvent` 消费待处理请求；`ProcessMapEvent` 在分发前清除事件 type。Zone 使用方为 physical 0 安装 `eas_Init` 并提供坐标字；表比较其低字节。没有选定楼层/路径证据证明自然到达这些属性、楼梯连接这些机会，或步行强制场景顺序。

## 场景使用哪些行动者

**已确认行动者身份：**初始化清除映射、安装按标志选择的随从，再读取选定固定/行走声明。己方角色声明保留角色查找，不使从 128 开始的普通 NPC 流递增。已有己方映射跳过其固定声明。角色查找、声明位置和物理实体槽位保持区别。

默认 Bedoe 声明 Luke 为己方角色 10，位于 `(38,51)`、Up；F75 随从映射可取代该放置。King 147 从 `(36,49)`、Down 开始，birdwoman 148 从 `(37,50)`、Down 开始。此处未确立该 birdwoman 的个人姓名。卫兵 150–156 是七名 birdmen；前序卫兵场景显式将 150/151 返回 `(34,51)` / `(35,51)`、Up。Peter 7 没有固定行：F67 提供随从声明。Bowie/main 则单独声明为 physical 0。

F722 替代实体群保留 Luke、king 147 和 birdwoman 148，但改变其他身份与放置：查找 152 是 birdwoman，153 是 chick，154–156 没有固定行。其普通 zone 表不选择这两个场景。不能安全地将传入的不匹配实体群/调用方描述成默认七卫兵阵容。

Peter 的 F67 声明独立于已加入/出战 F7/F39、字 HP 和军团 AI。Luke 固定行或 F75 映射同样不证明 F10/F42 成员资格或招募。两场景都不加入军团/出战队伍、不改变 AI 或修复两个随从标志。探索治疗，包括 Peter 的特殊资格，不证明映射，也不因 zone 场景单独再次执行。

两个程序都不重载地图或重建实体。它们消费当前映射、位置与行动。查找解析将角色掩码为字节，经 `$60` 索引差转换 NPC 128+，读取物理索引并缩放为实体地址。未初始化的零映射解析为 physical 0，不会安全跳过缺席 Peter。隐藏使映射失效为 `$FF` 时，则不检查有效性就缩放物理索引 255。行动、点头与隐藏请求没有通用存在守卫。因此，传入缺失或别名映射可改变受影响实体。

## 报告、国王命令与卫兵散去

**已确认有序内容：**讲话 `cs_573EC` 请求十条文本 1641–1650（`0669..0672`），king 147 六条、birdwoman 148 四条。下列概述覆盖全部请求；完整命令/文本台账仍由研究所有。

1. **报告（1641–1644）。**摄像机被送往 `(31,47)` 并等待滚动。Birdwoman 描述强大的 greater devil（1641），将扫除 devils 归功于 Volcanon（1642）。国王询问 greater devil（1643）。Birdwoman 移动 Left1 并面向 Up 后，报告他逃走（1644）。
2. **追击与危险（1645–1649）。**国王询问追击（1645），以及在 greater devil 恢复前将其抓获（1646）。Birdwoman 询问他是否来自 Grans（1647）。国王点头，表达对该推测的看法（1648），并警告再次袭击（1649）。这些仍是交谈中的推测、报告和意图。
3. **警戒与散去（1650）。**请求 sleep 10 帧后，birdwoman 面向 Down，国王下令警戒。请求 sleep 40 帧后是卫兵相对路径。150 与 152–156 行动启动但不等待；151 的路径启动并等待。脚本随后依次隐藏 150–156，等待 148 移动 Right1/faceDown1，然后结束。

内部没有选择、传送、加入、跟随命令或故事标志写入。两次 sleep 合计请求 50 帧；文本、摄像机和行动服务使其不能视为场景时长。遣散请求不证明完成巡逻或其目的地。

**已确认隐藏边界：**`HideEntity` 将当前位置与目的地写为内部 `$7000,$7000`，将 64 字节 `ENTITY_INDEX_LIST` 中每个匹配物理索引的映射条目替换为 `$FF`，并前移后续条目，从 `EXPLORATION_ENTITIES` 移除该物理索引。行动指针与行进字段保留。X 至少为 `$7000` 时，普通更新循环跳过实体/行动分发；保留的不等待行动不会因此完成，也不会在那里继续正常推进。不清除出战队伍标志。卫兵映射正常且互异时，国王、birdwoman、Luke 与 Peter 保持映射；错误别名可扩大影响。

这不同于前序 F720 重入处理。新初始化重建实体群与映射；默认 init 随后在 F720 已设时调用 `MoveEntityOutOfMap(150..156)`。该辅助程序将位置/目的地改为 `$7000,$7000`，保留映射及探索列表条目。它既不复用旧 `$FF` 映射，也不执行讲话中的隐藏操作。F719 仍在该 F720 处理之前独立控制初始卫兵场景。

## Luke 问候与访客介绍

**已确认有序内容：**觐见 `cs_574A6` 请求 29 条文本 1651–1679（`0673..068F`）。Luke 五次、Peter 十二次、king 147 八次、birdwoman 148 四次。Bowie 没有文本请求；其点头请求是编排，不是口头回应。

1. **问候 Peter 与国王（1651–1654）。**摄像机移至 `(34,47)` 并等待。Luke 移动 Down1/faceLeft1，执行两次带等待跳跃并叫 Peter（1651）。脚本请求 Peter 位于 `(33,52)`、Right，Bowie 位于 `(33,53)`、Right，然后等待摄像机到达 `(31,47)`。Peter 移动 Right3，问候 Luke（1652），面向 Up 并问候国王（1653）。国王收到带等待内联速度 48/48 行动，移动 Down1 后回应 Peter 的问候（1654）。
2. **介绍 Bowie（1655–1657）。**Peter 移动 Up1；请求 sleep 和点头穿插于 Left 再 Up 的转向，之后介绍（1655）。他移动 Down2/Left2，随后 sleep，Bowie 点头。Peter 启动 Right3/faceUp1 而不等待，Bowie 执行相同路径并被等待，之后点头。Peter 介绍 Bowie 来自 Grans（1656）。Luke 移动 Down1/faceLeft1；Peter/Bowie 面向 Right，Luke 认出 Bowie 是 Astral 的学生（1657）。
3. **Luke 的相识与信使身份（1658–1661）。**Peter 执行两次带等待跳跃，询问 Luke 如何知道（1658）。Luke 解释自己熟悉 Bowie（1659），Peter 质疑该解释（1660），Luke 点头并解释自己曾是 Volcanon 的信使（1661）。这些请求传达说话者对关系的解释，不独立证明 Astral、Bowie 或 Luke 旅行的历史。

## Zeon、Grans 与 Volcanon 邀请

**已确认同一觐见的后续：**

4. **国王警告（1662–1666）。**国王移动 Down1；Luke、Peter 和 Bowie 面向 Up。国王报告 Zeon 被释放（1662）并归咎于 Grans（1663）。带等待速度 16/16 行动后，他移动 Up2/faceDown1，警告访客（1664）。Peter 移动 Up1/Left1/faceUp1，询问 Zeon（1665）；国王称其为 devils 之王（1666）。
5. **去拜见 Volcanon（1667–1672）。**Birdwoman 148 移动 Down1，敦促拜见 Volcanon（1667），并说 Peter 或许能让他平静（1668）。Peter 犹豫（1669）。Luke 移动 Up1/Left1；Peter 面向 Right，Luke 催促立即拜见（1670），Peter 同意（1671）。Peter 转向 Down，让 Bowie 跟随自己（1672）。这是口头请求；最后跟随命令使 Peter 跟随 Bowie。
6. **允许 Bowie 同行（1673–1679）。**脚本等待 king 147 的 `eas_Init`，再等待其 Down1 移动。他询问 Bowie 是否同行（1673）。Peter/Luke 面向 Up，Peter 回答（1674），国王开始反对（1675）。Birdwoman 移动 Left1/faceUp1，主张让 Bowie 觐见（1676–1677）。国王点头同意（1678）。Birdwoman、Luke 和 Peter 面向 Down；Peter 结束交谈（1679），脚本尝试 selector 2 的 Peter→Bowie 跟随，然后结束。

邀请不请求 Map11、不显示 Volcanon、不设置 F722 或招募 Luke/Peter。没有选择/菜单、传送、实体/地图重载、加入或内部故事标志写入。四次 sleep 合计请求 120 帧，不是觐见实际时长。关于 Zeon 释放、责任、Volcanon 脾气及未来拜见的表述仍是有归属的台词。

## 相对位置取决于先前场景与传入状态

**已确认共享行动者：**觐见从不命令已隐藏卫兵 150–156，因此没有要求先讲话的隐藏卫兵依赖。两个程序都对 147/148 使用相对移动。若从正常固定位置成功运动，讲话结束时 birdwoman 会在 `(37,50)`、Down，国王仍在 `(36,49)`。相同假设下，觐见结束时国王在 `(36,50)`，birdwoman 在 `(36,51)`、Down。

若先运行觐见，之后讲话的 Left1/Right1 使用改变后的 birdwoman 位置，不会将她初始化回 `(37,50)`。两调用方都不修复先前位置或要求另一场景将其规范化。已有 Luke 随从放置、因 HP 跳过的命令、缺失映射、未完成行动或传入实体群，同样使绝对编排无法无条件描述。独立标志允许这种调用顺序，却不证明自然访问。

## 移动、HP、跟随与服务边界

**已确认单位与准入：**`setPos` 与 `setFacing` 读取实际字节行动者，测试该己方角色字 HP，仅在为零时跳过；HP256 非零。因此 Peter/Bowie 编排和 Luke 朝向有各自 HP 谓词。NPC 147/148 绕过己方 HP 检查。跳过位置/朝向命令不抑制文本或行动/跳跃/点头请求。F67 缺失时，获准 Peter 放置可经零映射影响 physical 0。

位置写入设置当前/目的地坐标与朝向。一地图瓦片为 384 内部 1/16 像素单位，即 24 像素。相对移动将有符号瓦片差值乘以 384 后加到当前位置，受目的地/阻挡服务约束。朝向行动计数是更新/等待操作数，不是距离。打包速度 48/48、16/16 与 `eas_Init` 32/32 有别；`eas_Init` 也恢复碰撞、阻挡、自动朝向/定向和精灵属性。

行动安装没有通用 HP 或映射守卫。带等待行动轮询选定行动者指针是否为 `eas_Idle`；不等待行动安装后即返回。等待卫兵 151 不等于等待 150/152–156；等待 Bowie 不等于等待 Peter。后续安装可替换未完成行动。跳跃暂时禁用自动朝向，运动/等待后恢复；点头运行动画/精灵/DMA/sleep 服务而不检查 HP。隐藏是排除更新，不是行动完成屏障；场景结束不增加全体行动者等待。

摄像机目的地命令清除跟踪并等待滚动。文本请求使用立绘/台词服务、等待视野滚动、显示并推进游标。单文本请求还关闭窗口并 sleep 10 帧。调试跳过使请求不能等同可见输出或时长。若对话窗口仍在，终止符有条件等待滚动并清除滚动速度；它自身不恢复控制。已有受控处理器观察不认证这些组合场景或自然表现。

最后跟随命令存储字 `$0007,$0000,$0002`。存活辅助程序检查第一个字的**高字节**，所以准入测试 **Bowie 0 的字 HP**，不是 Peter 7 的。Bowie 为零则跳过六个操作数字节。Bowie 存活且映射有效时，selector 2 安装 Peter 跟随 Bowie，独立于 Peter HP。其 `(-24,0)` 偏移单位为像素：随从使用方左移 4 位成为内部单位，再应用朝向/距离/布局规则。这不是即时固定世界位置，也不是让 Bowie 跟随 Peter。

`AddFollower` 先复制/修补并替换行动，再检测重复列表；Peter 缺席可使两操作数都解析为 main。没有跟随完成等待、F67 写入、加入、出战队伍准入或军团 AI 变化。觐见内部之后不重建。这安装当前实体关系；后续入场从当时选定标志/列表重建，不一定保留该关系或行动。

## 从调用方返回条件控制

**已确认调用方边界：**F720 或 F721 仅在各自脚本返回后写入。文本、摄像机或行动服务停滞时，不确立写入。选定调用方返回后，包括已设位跳过，zone 包装器关闭立绘/文本窗口、等待一个 VInt，并调用 `WaitForEntityToStopMoving(0)`。它等待 physical 0 当前 X/Y 等于目的地，既不测试 `eas_Idle`，也不测试全部行动者。

共享探索循环返回 `WaitForEvent`。仅待处理为零才将摄像机重新指向 0 并调用 `SetControlledEntityActScript(0)`；另一待处理请求优先。普通 `PLAYER_TYPE=0` 选择 `eas_ControlledCharacter`，其他类型选择各自行动。因此调用方/包装器服务完成及待处理为零确立有条件恢复控制，不确立输入响应、Peter 跟随稳定或立即前往神殿。这是首次觐见后本指南的停止边界。

## 证据审阅与剩余限制

本指南消费 [PR #786](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/786) 已接受研究及 [PR #785](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/785) 前序英文指南。研究保留 USA 零售版身份、固定 SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`、115 条记录、全部 39 个文本键与内联行动。其[复现边界](../../../research/bedoe-king-address-through-first-audience.md#caller-return-reproduction-and-stopping-boundary)拥有直接源码及保留比较。本正文切片审阅已接受研究和限定 tracked 测试夹具选择，没有重复源码调查、生成/复制可选载荷或采集运行时证据。

| 审阅关系 | 可执行所有者与合同 | 对抗性审阅边界 |
| --- | --- | --- |
| 独立调用方 | [sf2-map-events-static-v1, zone-programs](../../../../tests/fixtures/h2/map-events/zone-programs.json)、[地图探索](../contracts/map-exploration.md)、[请求消费](../../../research/map-event-request-consumption.md)。 | 选定程序保留 `chkFlg`→分支→脚本→`setFlg`→返回、独立 720/721 及一/两行引用。研究拥有精确 zone 行和产生方/使用方组合；坐标匹配本身不证明步行。 |
| 配置、init 与登场实体 | [sf2-map-setup-static-v1](../../../../tests/fixtures/h2/map-setup-static-v1.json)、[sf2-map-init-static-v1](../../../../tests/fixtures/h2/map-init-static-v1.json) bodies 14/15、[sf2-map-entities-static-v1](../../../../tests/fixtures/h2/map-entities-static-v1.json)、[出战队伍/名册](../contracts/party-roster-state.md)。 | 汇总清单不是每行/列表的独立解码。已接受研究拥有 F722 替换、行动者关联与新映射；body 14 区分 F719 场景与 F720 移出地图处理。 |
| 对话与编排 | [sf2-map-script-engine-static-v1](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)、[对话](../contracts/dialogue-system.md)。 | 公开命令族事实不含完整文本转录，也不确立场景可达。研究拥有讲话 1641–1650/觐见 1651–1679 及其 115 条记录；请求、内联行动与服务完成保持区别。 |
| 隐藏、HP 与跟随 | 同一静态夹具的放置/行动/生命周期/关系族与 [sf2-map-entity-gesture-relationship-motion-runtime-v1](../../../../tests/fixtures/h3/map-entity-gesture-relationship-motion-v1.json)、[地图探索](../contracts/map-exploration.md)。 | 宏操作数宽度、查找/HP 辅助调用与受控跟随边界仍窄于自然场景。研究提供隐藏与重入、更新排除、高字节准入、像素偏移及基于坐标的 main 停止组合。 |

[地图 init 验证器](../../../../src/sf2tool/h2/map_init.py)与[测试夹具 schema](../../../../schemas/h2-map-init-static-fixture.schema.json)保留完整分界体及溯源。[地图脚本验证器](../../../../src/sf2tool/h2/map_script_engine.py)与 [schema 组合](../../../../src/sf2tool/h2/map_script_schema.py)保留命令族事实和独立使用方。无需 Layer A 修正。既有窄复现路线包括 `uv run sf2 h2 map-init`、`map-script-engine`、`map-setup`、`map-entities` 和 `map-events`；它们是路线，不是本次文档切片的新执行声明。

**未知：**无碰撞楼层访问与自然顺序；实际标志、字 HP、映射、出战队伍/随从/AI 状态及继承位置/行动；不中断服务、隐藏/不等待行动生命周期、待处理完成与输入响应；可见摄像机/运动/立绘/台词及其时序；后续随从重建和接续 Volcanon。这些问题保留在证据所有者处，不授权采集。指南既不完成战役，也不完成设计 epic。
