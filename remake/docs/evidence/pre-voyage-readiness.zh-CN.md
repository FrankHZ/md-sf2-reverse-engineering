# 渡海后首个可玩切片能否启动？

**可以启动首个纵向切片。建议从损坏船只上的自由行动开始，以玩家登上甲板、能读到并推进第一段交接对白为终点。** 拒绝或取消搬运木板应交还控制，玩家可再次触发选择。做到这一步就能检验剧情传送、演员动作、对白和输入是否接得上。Battle08 首回合需要额外的地图与战斗输入，可单独验收。

审计从 `7c225fda347d74b3ee1e0fc62e699e616f860de7` 出发。交付基线 `28193f7fab5af0620636bf1acdc1922a6f5b9c8f` 只增加 #1726 的三行测试修正。本文的 **Confirmed** 指直接核对的实现、固定原版源码或注明来源的既有运行记录；**Inferred** 指据此判断的影响；**Unknown** 指尚无相应观察。检查使用 Git 对象、公开源码、局部编译函数和托管工作项，未启动游戏或模拟器，未访问玩家工作区、实例或存档。[审计任务 #1728](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1728)

## 会改变排期的事项

| 优先级与时点 | 证据与影响 | 责任与验收 |
| --- | --- | --- |
| 启动前 · 确定交接和停止点 | **Confirmed**：前一里程碑已独立验收到 Map14 (40,8) 朝左，F700 已写，F703/F408/F508 未写，无脚本等待；普通向下到 (40,9)。合法的后段存档在 Map58，本版本没有 Map14 自由存档。**Inferred**：这个起点足以继续开发。 | 主门在新票写明从已有 Map58 兼容存档的独立副本经普通离港接入，控制起步样例只用于定位。沿用跨章节进度的 High 交接约定，局部动作按 Standard。验收比较进入/退出状态、一次拒绝及再次同意、甲板演员与对白输入。无需先增设船上存档。[独立验收 #1563](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1563) |
| 随首片 · **确定的实现阻塞** | 甲板初始化给演员 129 安装异步脚本，两条 `ac_wait` 的原始操作数在导入时变成不支持的指令。通过文件读取不代表能跨过首个演员服务。 | 动作编译和执行的所有者按下节验证源操作数、起始计时器和下一条指令。同意路径必须在实际普通输入下继续，拒绝路径不能被该脚本拖住。 |
| 若首片扩大到 Battle08 · **确定的内容阻塞** | 普通包没有 Map72；战斗内容登记表只有 1–7。仅加一行选择会超出登记数，亦缺 Battle08 的身份校验。 | 划入该终点时，内容 owner 补地图、战斗/地形/场景来源、携带队伍和剧情接线。验收 Map14 写 F408、队伍及资源随请求到 Map72、Battle08 入场且首次合法输入可用；保留可选 Kiwi 的两种队伍状态。若只交付甲板对白，就不需要这批输入。[原版入场条件](../../../docs/design/synthesis/chapters/eastern-parmecia/damaged-ship-plank-through-battle08.md) |
| 随对应切片 · **风险尚未证实** | 发布记录中选用的高清世界约 121 MB（十进制），读取上限 134,217,728 字节，按该报告估算余量约 13 MB。下一段实际新增量 **Unknown**。中文对白选择也止于海上首次交还控制。 | 内容/本地化 owner 在新终点先量实际世界字节、所需文本 ID 和资源依赖，按所选中英文路径读到首段对白；超限才调整方案。已有画风可回落，高清全量重绘不构成前置。[素材清单](../content/asset-inventory.md) · [中文准入范围](../application/pre-voyage-chinese-admission.md) |
| 可延期 · 生命周期与体验 | #1716 有中文 Continue 的终态报告之后一次退出访问冲突；首次未单独保留原生退出码，同一输入复跑 13 项通过且退出 0。原因、正常玩家退出是否受影响均 **Unknown**。#1456 仍需玩家评价具体路线的可读性、手感与设备表现。 | #1716 用独立副本区分退出码、终态报告、普通退出和观察器路径；若新证据显示进行中崩溃、进度丢失或阻断交接，再提高优先级。#1456 接回人工决定。两项均没有证据推翻上述 Map14 起点。[#1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716) · [#1456](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456) |

## 1. 架构和代码质量

**木板搬运的第一处硬边界。** 原版默认区域事件在 (45,5) 调用 `cs_58E5C`，返回后写 F257；该位不阻止再次触发。Yes 请求显式 Map14 (13,11) 朝下，区别于保留现场的楼梯传送和先前 Map73 脚本内加载 Map14。F458/F703 均清时，入场初始化运行 `cs_58FA4`。第一批指令淡入，安装 `customActscript 129`，接着是商人的第一段对白。把文字表抄进新世界、或只验证首次自由移动，都测不到这个交接。[研究与可复核的事件表](../../../docs/research/chapters/eastern-parmecia/damaged-ship-plank-through-battle08.md)

- **Confirmed，原版静态。** `customActscript` 展开为 `csc14 actor,0`，不等待演员空闲。`ac_wait` 写入操作码字 0 和一个操作数字；`eaWait` 则是另一种宏编码。`csc14_setEntityActscriptManual` 先解析角色、用返回的 d0 低字节初始化演员计时器，再安装脚本。`esc00_wait` 读取操作数字，却用 `cmp.b` 和有符号 `ble` 比较低字节。操作数低字节 ≤ 当前计时器（均按有符号字节）即清计时器并执行下一条，否则递增计时器并让出本次服务。[宏定义](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/sf2cutscenemacros.asm) · [安装](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/scripting/map/mapscriptengine_1.asm) · [执行](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/code/common/scripting/entity/entityscriptengine_2.asm)
- **Confirmed，重制实现。** 在实际编译函数上以十进制字面量单独输入 40、255，得到 `wait`；输入 256、600、1200，均得到 `native-call(ac_wait)`。读取器把后者保留为 `UnsupportedEntityAction`，运行到它时报告 `entity-action`。该回退只带 symbol/source，动作数值未进入可执行等待。已接纳的普通 `WaitEntityTicks(byte)` 用无符号小于比较。只取低字节也会把 0xB0 当成 176。生产编译器为该异步安装选 `SlotTimer`、`wait=false`，执行器以实际物理槽初始化计时器；角色号 129 不能直接当作初值。[编译动作](../../../src/sf2tool/remake_exploration_actions.py) · [编译安装](../../../src/sf2tool/remake_exploration_programs.py) · [读取](../../src/Sf2.Remake.Content/Scenarios/ExplorationContentReader.cs) · [安装](../../src/Sf2.Remake.Application/Runtime/Exploration/ProgramRunner.cs) · [执行](../../src/Sf2.Remake.Application/Runtime/Exploration/EntityActionRunner.cs)
- **验收判据。** 以初值 0 作对照，1200 = 0x04B0，低字节为 −80，立即继续；600 = 0x0258，低字节为 88，递增 88 次后才继续。以计时器初值 5 为第二个对照，600 应递增 83 次。实际接入时验证实际槽、动作游标、清零、相邻移动和父脚本是否继续。在真实场景里，两条等待之间还有移动及目的地等待；原版自然时序和各演员何时完成仍为 **Unknown**。不把操作数当作帧数，不靠延长通用计时器或删掉背景演员过关。[Map14 原始调用](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/maps/entries/map14/mapsetups/scripts_2.asm)

| 从玩家操作追到实际所有者 | 核到的边界与变化例 |
| --- | --- |
| 系统存档 → `OpeningPersistenceHost` → `GameSession.QuerySave/CaptureSave` → `OpeningSave` → `Sf2OpeningRestore` | 查询先检查稳定状态、选恢复配方，并试建脱离当前会话的候选；提交时会话或 revision 已变则报 `save-stale-revision`。Map58 配方要求 F507/74/640、排除 F407/999、同图朝向 3 的返回锚。另一近例是 Map3：选中 `ms-map3-flag609` 且 F603 已写可回教堂外 (32,13)，F3 决定恢复时是否移除 Jaha 的场景实体。地点特例留在 SF2 恢复策略。[资格与捕获](../../src/Sf2.Remake.Application/Runtime/Persistence/OpeningSave.cs) · [配方](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OpeningRestore.cs) |
| 写盘 → 标题 Continue → 恢复 | 两槽独立写锁；临时文件落盘、重读校验，再以替换/移动发布。发布后不确定返回 `save-publication-uncertain`。主槽缺失或发生所列结构错误且备份有效才允许显式恢复，语义不兼容不会静默修复。Continue 检查主槽，`SaveRecordRestore.Restore` 成功才挂载；`OpeningRestore.Build` 对比包、玩法 revision、规则与来源兼容信息，普通已落定存点不重播 OnLoad。新进程保存往返另有 #1714 的原生记录。[存储](../../game/src/Persistence/LocalSaveStore.cs) · [入口](../../game/src/Entry/OpeningPersistenceHost.cs) · [重建](../../src/Sf2.Remake.Application/Runtime/Persistence/OpeningRestore.cs) · [近例](../verification/map3-commission-save.md) |
| 内容选择 → 场景脚本 → 战斗 → 返场 | `pre-voyage-opening.json` 的包名仍是 `private-battle01-to-battle02`，revision 4，实际选了 18 图、7 战。`compile_routes/compile_maps` 消费选择；`PrivateWorldEncounterReader` 启动时读相对 world 所在目录的依赖、校验唯一遭遇和共享队员基线。普通模式最多 1 根战斗 + 6 附加，受控模式最多 1 + 1。`BattleEntry.Continue` 按解锁/完成旗标选择，使用随行 HP、装备等覆盖默认队员；`BattleOutcome.Begin` 从本场实际资源、种子、物品和保留队员构造返场，走选定战后程序。追加 08 需要身份、输入及内容接线，不能靠在 Godot 里切号。[选择](../../content/private/pre-voyage-opening.json) · [生产器](../../../src/sf2tool/remake_exploration_content.py) · [输入上限](../../src/Sf2.Remake.Content/Scenarios/PrivateWorldEncounterReader.cs) · [登记](../../src/Sf2.Remake.Content/Scenarios/PrivateBattleEncounterReader.cs) · [入战](../../src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) · [返场](../../src/Sf2.Remake.Application/Runtime/Exploration/BattleOutcome.cs) |
| 私有素材 → Godot 设置 → 玩家看到的图 | 读入 `hdBlocks` 时数量必须匹配原块，每块 96×96。设置先调用 `PrepareGraphics`；`Painted(map)` 取决于所选模式和该图是否有高清块；纹理键含地图/块/模式，绘制仍按 24 像素逻辑格。#1724 在同一暂停进度上观察到 24→96→24、成员图像变清而面板大小不变。这里的特例是私有内容给 Map3 提供图，并未给章节引入另一份游戏状态。[读取](../../src/Sf2.Remake.Content/Scenarios/ExplorationAssetReader.cs) · [设置](../../game/src/Entry/SettingsHost.cs) · [显示](../../game/src/Exploration/ExplorationPresentation.cs) · [纹理](../../game/src/Maps/MapBlockTextures.cs) · [PR #1724](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1724) |

**与旧审计对账。** 对照 #1528/#1564 的现行调用：恢复队伍资源时 `Sf2OutcomeReturnPolicy` 用 `WithResources`；准备治疗/攻击/辅助法术时先验目标再处理沉默（F4/F5）。`ProgramRunner` 对战败演出中的每个实际指令调用 `RequireStagingInstruction`（F6）；`MapTransfer.LoadScene` 带走 `CurrentEntities`（F7）；逻辑音乐和 Godot 音频均用 `ExplorationViewContext` 选区域（F8）。早期 F3 的遭遇重建现用 `WithOutcome`。这些曾经的缺口都有实际消费者。另一方面，Map20/F605 对演员 130 的额外清理只在私有内容绑定器包装源程序，先核源标识和 sprite 209；该现代宽屏规则没有进入通用地图转移。首片没有证据要求先重写脚本机。[旧审计与修复边界](structure-and-quality-review.md) · [法术](../../src/Sf2.Remake.Domain/Battles/Rules/IBattleActionRule.cs) · [恢复](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs) · [场景切换](../../src/Sf2.Remake.Application/Runtime/Exploration/MapTransfer.cs) · [音乐](../../game/src/Audio/SessionAudio.cs) · [城堡绑定](../../src/Sf2.Remake.Content/Scenarios/PrivateCastleSceneContent.cs)

**维护成本。** 从 `7c225fda` 往前 60 个 first-parent 提交，按每次提交出现的文件名计，`GameText.cs` 17 次（371 行），`BattleSessionView.cs` 13 次（612 行）；近例包括诊断开关、目标指引与调试胜利共用词条。新对白应走 world 消息表，校对新增 UI 语义时才协调这些共享文件。按 Git 跟踪文件的物理行数，`ExplorationSessionView.cs` 922，脚本编译 `remake_exploration_programs.py` 939，专门的 `remake_exploration_actions.py` 215；此次等待问题已经有可单独审阅的动作 owner。真正超大的 `h2/map_script_engine.py` 有 13,276 行，混有源码摘要、关系解析、夹具比对；`ExplorationSessionTests.cs` 2,124 行。在样例已能区分两种比较语义的情况下，重跑整个历史轨或预先拆库没有增加这条交接的把握。后续若必须改超大文件，按被触及的职责切分、检查净增长；#638 的 H4 整理仍有自己的范围。[源码审查约定](../../../docs/operations/bounded-inspection-and-review.md#source-size-and-responsibility)

## 2. 文档能带读者走到哪里

| 类型 | 从哪里进、查到什么 | 修正或验收 |
| --- | --- | --- |
| 已有证据与合同 | 英文索引从研究 → 设计 → 重制分别导航；木板有英文原版证据、英中玩法说明。研究区分 F257 的写入与重复触发条件，也注明自然可达性未观察。重制协议和短试玩路线则止于 Map14。 | 新票用研究/设计说明规定分支，用实际重制输入证明新增可玩结果，沿现有 owner 更新能力表；不必另写整章合同。[研究](../../../docs/research/chapters/eastern-parmecia/damaged-ship-plank-through-battle08.md) · [地图入口合同](../../../docs/design/contracts/world/map-entry-routing-state.md) · [中文玩法](../../../docs/design/zh-CN/synthesis/chapters/eastern-parmecia/damaged-ship-plank-through-battle08.md) · [能力](../capability-status.md) |
| 已过期 | 基线的中文工程入口仍说“不包括 Battle03 结果”，而包与 #1563 均已越过该点。本次更新该入口。campaign 矩阵 D 仍写 independent review pending，首节还称 #1629 未解；实际已合 #1674。`ExplorationDispatcher.Start` 取 `start.EntryProgram ?? world.Definition.OnLoad`，后者是已选 setup；显式覆盖仍优先，落定存点重建另行处理。 | 后续由 campaign owner 修正文内现在时及 D 的独立验收链接，保留各次历史观察的限制。以“新读者能否判断从哪张图继续”验收。[工程协议](../application/pre-voyage-campaign.md) · [启动代码](../../src/Sf2.Remake.Application/Runtime/Exploration/ExplorationDispatcher.cs) · [PR #1674](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1674) |
| 玩家/开发入口之间缺一跳 | Remake README 的普通启动链接落在菜单资源的“Final publication”；它选择一对 world/scene，但从基础包重制菜单不会生成 Map3 重绘。现有高清发布器另需 entry 和 material master，输出另一对 world/scene。照前一路径可成功进入游戏却看不到新版美术。短试玩路线已有英文实际操作、兼容塔楼存档条件和试玩终点。 | 从普通启动的资源选择直接指向当前画面包的生成/选择说明；用同一基线回答“怎样普通 Continue、如何辨别 HD 是否真的切换”。这里只发现导航缺口，未证明玩家现场选错包。[运行入口](../../README.md#run-locally) · [菜单发布](../content/presentation-assets.md#final-publication-and-ordinary-selection) · [高清发布](../content/asset-inventory.md) · [短路线](../godot/ui-scene-review.md#short-play-route) |
| 实现缺口、人工体验、合理延期 | 船上自由存点/08 未实现；原版研究不授予重制存点。中文 806 条 world 消息的已审范围排除了后续甲板事件；`DisplayCatalogReader` 实限为 world 1,024 行、每文件 256 KiB、每消息 8,192 个 UTF-16 单元。容量余量不证明新对白已选入或排版舒适。全程手感、设备/DPI、HD 美术认可由玩家决定，后续城镇、美术和整部剧情可延期。 | 新切片按实际消息 ID、源参数及游戏内分页验收新增文本，注明仍回落英文的行；真人反馈回到 #1456/美术 #1699。数值平衡可沿七战覆盖报告继续研究，首段木板选择不依赖完整七战复算。[中文范围](../application/pre-voyage-chinese-admission.md) · [读取器](../../src/Sf2.Remake.Content/Localization/DisplayCatalogReader.cs) · [战斗分析](../../../docs/design/synthesis/analyses/pre-voyage-battle-model-coverage.md) |

## 3. 从根目录找输入、代码与产物

- **原版与重制。** 根 `manifests/`、`schemas/`、`tests/fixtures/`、`docs/research/` 定位来源、数据契约、可发布预期和研究解释；`docs/design/` 放引擎无关合同与章节，`design/zh-CN/` 镜像设计。根 `src/sf2tool/` 是 Python 工具，包括重制内容生产器；`tools/bizhawk/` 是观察脚本，`scripts/` 是冻结的兼容入口。重制的三套 C# 工程在 `remake/src/`，Godot 在 `remake/game/`；实际项目引用方向是 Application → Domain，Content → Application/Domain，Godot → 三者，没有到 reference 或检查工具的引用。[根布局](../../../README.md#repository-layout) · [重制布局](../../README.md#repository-layout)
- **样例与测试。** `remake/content/authored/` 可供 authored 入口读入，`remake/content/private/pre-voyage-opening.json` 只是受版本控制的选择表；`remake/reference/inputs/` 是外部受控比对输入。引擎行为测试在 `remake/tests/Sf2.Remake.Engine.Tests/`，真实适配层探针在 `remake/game/probes/`，私有准入/现场检查工具在 `remake/tools/`。根 `tests/python/` 服务于仓库工具。由此可定位 #1726 中“玩家用七战 world 能 Continue，但受控 reader 拒绝”的两个不同入口。
- **跨 worktree 与资产。** `src/sf2tool/paths.py` 从工具文件定位仓库；附加遭遇相对 world 文件所在目录解析，绝对路径或越过目录被拒。高清发布器在指定输出旁暂存，成功后改名，不替换已有目录。资产库的 `manifests/pre-voyage-asset-inventory.csv` 不在代码库；发布器实际写到新包的 `review/assets.csv`。前者是资产库清单，后者是本次产物，查错根会找不到。生成的 `local/`、Godot 导入、C# bin/obj 在 Git 忽略范围；玩家槽位由 checkout 外的显式根和 namespace 选择。新切片使用独立输出、现有私有只读选择和可丢弃的外部槽位即可。[路径](../../../src/sf2tool/paths.py) · [发布器](../../../src/sf2tool/remake_map3_hd.py) · [素材归属](../content/asset-inventory.md)
- **Confirmed，实际断链。** 从 [剧情证据覆盖](../../../docs/research/story-content-coverage.md) 的 retained failures 导航，会去 `docs/research/original-reference-replay-capability.md`；该位置没有 Git 对象，实际 owner 在 `docs/research/methods/`，原锚点仍存在。根/中英入口能打开不代表下一跳有效。由研究导航 owner 将该一处相对路径改为 `./methods/original-reference-replay-capability.md#current-lineage-hard-stop`，从原页点击抵达标题即验收，无需搬目录。[实际 owner](../../../docs/research/methods/original-reference-replay-capability.md#current-lineage-hard-stop)

## 4. 一个可玩增量怎样从反馈到交付

| 近例 | 实际发生的事 | 可沿用的做法或缺口 |
| --- | --- | --- |
| 玩法里程碑 #1535 → #1540 → PR #1563 | 四段按前段结果依次接入；末段用 Map58 保存/普通 Quit/独立进程 Continue 贯通地震、港口 No/取消/Yes。主门在候选 `1c6087c3` 另验终态输入与投影，合入 `75d68286`。#1535 和 #1540 均 Closed / Project Done。 | 在已发生的状态上验下一段，使用确切停止点和保留的失败记录。里程碑由 epic、分片及合并证据表达，#1540 的 GitHub Milestone 字段为空；单靠该字段无法判断交付。[Epic](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1535) · [末段](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1540) · [独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1563) |
| 中文试玩 #1456 → #1711 → PR #1714 | 玩家在 `34244d1a` 的委任后教堂外无法保存。主门查到是 setup 选择，区别于先前跟随者运动；修正已合入 `d787a2cc`。保存、新进程 Continue、再走到 Jaha 入队均有独立测试副本的记录；#1711 为 Done。同批显示问题 #1708 单独修复。 | 反馈有版本、地点、旗标、期望和复现，再分票。#1456 正文的“Current status”仍称 #1580 未实现，实际 #1592 已合：普通探索时钟 1/60 秒，每回调最多 4 步，菜单等待上限 120 步。用最新评论和修复证据校正文首，同时保留 Review / Verify 等人类结论。[反馈](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1456) · [时钟](../../game/src/Exploration/ExplorationSessionView.Clock.cs) · [#1592](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1592) · [委任后保存](../verification/map3-commission-save.md) |
| 美术/本地化/QA #1717 → PR #1724 → #1726 | Map3 发布同包切换图像、资产清单与中文 Continue；PR 有七个成功检查，私有普通玩家路径另有观察。一次继承七战试玩配置的本地套件却是 2,409 过、27 败。限定复核后 24 项在对应选择下不改即过。另三项的实现已纳入 MITHRIL、普通开局换装；#1729 加入精确 ID 123，把两项 `AllowChanges` 改为 true，这三处断言所在的三个完整方法均通过、无跳过。 | 保留原失败，分别验证受控、真实试玩、公共 CI；美术意义仍由玩家验。#1717 已关闭且合并，读取 Project 时仍是 Backlog，与 Done 规则不符。主门去重后纠正该项，并将现行试玩版本与包选择链接到交付记录。[#1717](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1717) · [#1724](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1724) · [#1726](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1726) · [#1729](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1729) |

**可复现的版本单位。** `global.json` 锁定 .NET SDK 10.0.204（禁用 roll-forward），`toolchain.json` 固定 Godot 4.7.2 Mono，`uv.lock` 管 Python；画材母版有独立 Git 对象，试玩包则须匹配 world/scene、entry、所用语言文本表和兼容存档。即使同为 gameplay revision 4，原版画面包与 Map3 高清包也可能不同。交接可复用已有 Issue/PR 的候选 head、输入身份、包选择、语言、测试起点、实际原生结果及失败记录；在新切片验收时用独立配置据此重现同一终点即可，不必增建发布系统。[锁定环境](../development-and-verification.md#locked-net-workflow) · [发布重建](../content/asset-inventory.md)

## 复核入口

在仓库根目录执行。下列命令只读取 Git 或托管状态；Project 字段会随主门维护变化。

```powershell
git diff 7c225fda 28193f7f -- remake/tests
git show 7c225fda:remake/content/private/pre-voyage-opening.json
git log -60 --first-parent --format= --name-only 7c225fda |
    Where-Object { $_ -match '\.(cs|py|gd)$' } | Group-Object | Sort-Object Count -Descending | Select-Object -First 10 Count,Name
git ls-tree -d --name-only 7c225fda
git ls-files '*original-reference-replay-capability.md'
git grep -n 'original-reference-replay-capability.md' 7c225fda -- docs/research/story-content-coverage.md
git check-ignore -v local/example.json remake/game/.godot/example remake/src/Sf2.Remake.Domain/bin/x
gh issue view 1717 --json state,projectItems,milestone
gh issue view 1456 --json body,comments,projectItems
gh pr view 1563 --json headRefOid,mergeCommit,comments,statusCheckRollup
gh pr view 1729 --json files,mergeCommit,body
```

动作边界可用已配置的 worktree Python 在根目录复核。此小样只调用原编译函数的数值分支，以空符号表处理十进制字面量；它没有构造场景或重放游戏。

```powershell
@'
from sf2tool.remake_exploration_actions import compile_action
from sf2tool.remake_exploration_programs import OriginalPrograms
reader = object.__new__(OriginalPrograms)
reader.equates = {}
for ticks in (40, 255, 256, 600, 1200):
    print(ticks, compile_action(reader, "ac_wait", [str(ticks)], "audit"))
'@ | & ./.venv/Scripts/python.exe -B -X utf8 -
```
