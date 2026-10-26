# Battle07 结果与探索交接

> 本文是[规范英文原文](../../synthesis/battle07-outcomes-and-aftermath.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

Battle07 首次胜利后，恢复探索之前有一段很长的塔楼场景。**推断的叙事连接：**营救 Elis 失败，Bowie 获得 Galam 的宝石，众人在塔楼摇晃中撤退。**已确认的创作内容与静态状态：**after 程序安排了这一序列，写入宝石/随从/地震标志，并准备返回 Map58。这些事实不独立确认角色的解释，也不证明观察到自然游玩流程。

本文承接 [Battle06 结果至 Battle07 准入](battle06-outcomes-through-battle07.md)。已接受的[研究所有者](../../../research/battle07-outcomes-and-aftermath-handoff.md)提供选定源码组合与溯源。本指南止于首个条件探索控制边界及 Map58 紧接的出站记录；城堡/城镇撤离、出航及 Battle08 不在范围内。它不增加重制能力或产品/UI 决策。[路线图](../documentation-roadmap.md#当前基线与近期综合)拥有 [#719](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/719) 的 Standard 严谨度协议。

`Fnnn` 表示标志 nnn。下方原版源码引用使用
[`ShiningForceCentral/SF2DISASM` 提交 `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm) 的 `disasm/`。
研究与测试夹具绑定[根基线](../../../../README.md#pinned-original-baseline)记录的 USA 零售 ROM 身份。命令索引是从零开始的地图脚本记录，不是帧数或源码行数。

## 1. 结果决定是否运行长场景

**已确认静态规则：**Battle07 使用普通的阵营计数结果。已放置且存活的战斗员计入；Bowie 的 HP 为零时强制己方计数为零。先按己方为零选择败北，再按敌方为零选择胜利。仅击败敌方槽位 128 不构成 Battle07 特殊胜利：败敌路由为空，首领存在表行的值为零，位置/HP 覆盖表没有 Battle07 条目。作为场景行动者的 Galam 与 Dark Smokes 不改变这些规则。[控制器合同](../contracts/battle-control-lifecycle.md#动作后与回合后顺序)及[选定结果审阅](../../../research/battle07-outcomes-and-aftermath-handoff.md#outcome-branches-and-caller-order)拥有检查和选定表行。

| 结果 / 传入状态 | 有序后果 | 返回边界 |
| --- | --- | --- |
| 胜利，F507 未设 | 治疗符合条件的己方角色；以当前地图/位置/朝向初始化返回字段；执行 `abcs_battle07` 和共享加入尾部；清除 F407，设置 F507。 | 场景将初始返回字段替换为 Map58 `(13,35)`、Down，并在调用方完成前写 F385/F74/F640。 |
| 胜利，F507 已设 | 治疗，保留初始返回字段，跳过整个 after 程序，执行相同加入尾部；清除 F407，设置 F507。 | 普通 Map58 返回使用首名出战成员的最终战斗位置和实体朝向，原点为 `(0,0)`。本分支不产生新的宝石/随从/地震写入或固定 `(13,35)` 返回。 |
| 败北 | 更新战斗解锁标志；固定败北文本 363；恢复 Bowie 的最大 HP；通过无符号向下取整除法将金币减半；取得撤离位置。 | 返回结果 `d4=-1`；无 after 程序/加入或新的 F507/F385/F74/F640 写入。F507 未设则保留 F407；F507 已设则清除它。不适用 Battle04 特殊败北路径。 |
| Egress / Angel Wing | Egress 消耗法术 MP 并使用固定文本 274；Angel Wing 移除选定物品槽并使用文本 275。共享尾部呈现战斗退出、更新解锁标志并取得撤离位置。 | 返回结果 `d4=0`；F407 的条件处理与败北相同，没有败北的金币减半或 after/加入调用。 |

[after 包装器合同](../contracts/battle-cutscene-routing.md#战后路由与加入尾)保留即使跳过场景也执行的共享尾部。所消费的过场加入表中选定行 7 向 `JoinForce` 提供零，即 Bowie；没有提供新的剧情招募对象。不要以名称相近的旧 global 表替代，也不要从该辅助调用推断可见招募事件。

**已确认的输入相关逃脱示例：**Battle07 没有特殊撤离目的地。沿用前篇未修改的 `EGRESS_MAP=16` 且 F399 已设时，存档点选择返回 Map16 `(27,7)`、Up。神殿使用或其他调用方可以改变该输入。未列出的地图返回 `(1,1)`、Up；BattleLoop 设置 F399，因此这里不走辅助函数的游戏开始回退。F64 控制的木筏重置是独立辅助副作用。所有返回目的地都经过 `SwitchMap`；这个示例不证明自然逃脱/重入路线。

胜利在场景前治疗存活己方角色及 Peter/Lemon 例外。每种结果返回时，探索又调用治疗：恢复 HP/MP，跳过其他死亡己方角色，仅保留已存在的眩晕/中毒/诅咒位，再重建属性值。Egress 消耗 MP 与之后治疗是独立阶段。场景中的坠落、摇晃和消失不增加交战 HP/死亡事务。

### 跳过 intro 会保留不同的随从输入

**已确认：**F457 独立于 F507 控制 before/start。执行 `bbcs_07` 时，在布置对峙前清除 F83；F457 已设的重入会跳过该清除，F88 挂起入口则跳过两个 intro 程序。这两种跳过都不阻止 F507 未设时的 after 场景。after 程序不清除 F83。因此，[前篇入场边界](battle06-outcomes-through-battle07.md#6-battle07-before-清除王室跟随并布置对峙)仍是本次返回的条件输入，而非保证已归一化的状态。

## 2. 塔楼场景从对峙推进至撤退

**已确认程序形状：**`data/battles/entries/battle07/cs_afterbattle.asm` 中的 `abcs_battle07` (`0x49F7E`) 将命令 0–396 作为一个线性程序运行。它没有显式标志分支、子程序调用或中途场地控制返回；各处理器仍有存活检查和服务依赖。它的 `loadMapFadeIn` 设置 Map58/摄像机 `(8,16)`；随后的 `loadMapEntities ce_4A912` 重建场景实体群。摄像机坐标不同于 Bowie 的场景起始位置 `(13,23)` 及最终返回 `(13,35)`。

**已确认创作对话：**从 2385 开始的单个游标提供普通未跳过的 ID 2385–2460。下列摘要消费已接受的文本/行动者审阅，不复制逐字对话。调试/P2-Start 跳过可以抑制普通文本和游标递增；该 after 程序内没有固定 ID 的 `txt` 命令。[对话合同](../contracts/dialogue-system.md)拥有命令/输入边界；[场景审阅](../../../research/battle07-outcomes-and-aftermath-handoff.md#long-scene-actors-phases-and-text)拥有阶段、说话者与文本溯源。

| 场景阶段 | 创作内容与有意义的边界 |
| --- | --- |
| 人质对峙——命令 0–89，ID 2385–2401 | 国王和大臣上前；Galam 威胁 Elis，效果行动者演出攻击，王室人物随之后退。局部图块复制和行动者移动布置对峙；威胁和安慰不独立证明交战伤害或存活。 |
| 宝石与门口展示——90–147，ID 2402–2414 | Astral 询问 Galam 有关塔楼之事。叙述描写 Galam 展示宝石；白色闪光、震动、摄像机移动及工作地图复制，在效果行动者进入时演出门口变化。本阶段不授予物品栏物品，也不证明持久的门口碰撞状态。 |
| 光芒升起与营救尝试——148–223，ID 2415–2425 | Galam 与 Elis 随效果升起。Astral 要 Bowie 施展身手；Astral 与大臣协助布置将 Bowie 向上抛起，摄像机跟随他。脚本位移独立于战斗移动合法性；等待与非等待行动者动作仍需区分。 |
| 抓取、闪光与坠落——224–305，ID 2426–2439 | 叙述描写 Bowie 抓住宝石、宝石融入其颈部及两颗宝石合并。文本前后伴有闪光、颤动、姿势和坠落动作。命令 305 只在最后的合并叙述后设置 F385；没有伴随普通物品槽或能力/属性值授予。 |
| 消失与后续——306–337，ID 2440–2447 | 效果行动者以马赛克效果消失，Galam 与 Elis 收到消失脚本，并请求悲伤音乐。Bowie 被恢复/重新启动，众人讨论宝石。营救似乎失败是推断的叙事解读；行动者消失不构成死亡或加入军团。 |
| 再次地震与撤退——338–396，ID 2448–2460 | 震动/首领音乐调用伴随离开的决定。Astral 与大臣演出向南抬走国王；摄像机跟随国王。Astral 最后的警告之后才有返回请求、跟随、持久标志写入及脚本结束。抬走撤退期间没有探索控制交接。 |

场景实体 128/129 起初是地图外效果，之后成为门口效果精灵，并在该实体群内复用。Galam 130、Astral 131、Elis 132、国王 133、大臣 134 标识场景角色；它们不标识存活战斗员或下一批探索实体。Galam/Elis 的 `eas_TwirlShrinkDisappear`、Bowie 恢复的精灵及 Astral 重启的动作脚本都是行动者操作，不是出战队伍成员身份或交战 HP 规则。

**未知：**自然完成的等待、同时移动、滚动/淡入淡出/音频交付及场景总时长。命令范围不是经过帧数。仅最后的震动操作数不证明加载后可见震动。指定地图加载会重建工作布局，因此场景图块复制不是已保存布局的证据；[地图加载合同](../contracts/map-exploration.md#加载与选择顺序)保留重建/保留的区别。

## 3. 场景写入三个由不同使用方消费的标志

**已确认生产方/使用方关系：**叙述、当前行动者和持久标志有各自效果。`abcs_battle07` 不提供新招募、金币事务、普通物品槽事务、`setStoryFlag` 或 Battle08 解锁。

| 生产方 | 后果与限制 |
| --- | --- |
| F385，命令 305 | `BuildMemberStatusWindow` / `@WriteJewelString` 先针对 Bowie 测试 F384。通过该门后，F385 增加第二个宝石图标和复数后缀。F385 已设但 F384 未设时，该分支不显示任何宝石。本场景不设置 F384 或授予物品栏物品。 |
| F74，命令 394 | 随从表准入三行：Astral `FOLLOWER_B`、国王 `FOLLOWER_A`、大臣 `FOLLOWER_D`。只写 Astral + King 的源码注释不完整。探索重建时，这些行与其他已启用行一起被选择。 |
| F640，命令 395 | `SwitchMap` 消费标志切换表：之后的 Map3 请求变成地震城镇 Map4。该标志不替换 Map58。其后续清除/生命周期不在本段范围内。 |

[持久效果审阅](../../../research/battle07-outcomes-and-aftermath-handoff.md#persistent-effects-and-actual-return-consumer)列出菜单、随从和地图切换源码。通用清单没有以可执行验证闭合“场景写入至菜单”或随从重建组合。已接受的源码审阅支持这些条件静态关系；自然显示、随从行为与持久性仍为未知。

## 4. 返回请求先于脚本和调用方完成

**已确认选定顺序：**传送命令准备请求后返回解释器，不终止场景。通向控制的实际路径是：

1. 命令 390 `csc07_warp` 写入事件类型 1、参数 1 为零及返回参数 `(58,13,35,DOWN)`。
2. 命令 391–393 通过 `csc2C_followEntity` / `AddFollower`、关系选择器 2，请求当前场景 Astral → Bowie、国王 → Astral、大臣 → 国王跟随。命令 394/395 设置 F74/F640；命令 396 结束脚本。之后解释器执行条件滚动等待/清理，包装器到达 `JoinForce(0)`。
3. `BattleLoop_Victory` 清除 F407、设置 F507、读取返回参数并返回 `d4=1`。F407 是准入状态；F507 控制完成/after 场景。若解释止于传送，就会遗漏跟随、两项持久写入和调用方完成。
4. `alt_MainLoopEntry` 应用 `SwitchMap`，再直接进入探索；它不在本次结果返回中再次调用 `CheckBattle`。该使用方绕过普通探索的 `ProcessMapEventType1_Warp` 路由。
5. `ExplorationLoop` 立即清除 `MAP_EVENT_TYPE`、治疗、淡入淡出、赋予当前地图和 `NOT_CURRENTLY_IN_BATTLE`、加载瓦片集并初始化实体。Map58 没有配置行：`GetCurrentMapSetup` 返回 `ms_Void`，实体列表访问器解析到其终止符。`InitializeMapEntities` 清除旧场景实体群，选择标志启用的随从，并在指定 `(13,35)`、Down 声明主实体。
6. 探索清除临时标志 256–383，设置 F80，加载地图并安装基本 VInt 函数。`RunMapSetupInitFunction` 对 `ms_Void` 跳过调用；Map58 没有额外 init 场景。F385/F74/F640/F507 不在临时清除范围内，因此保留。
7. 条件音乐/淡入淡出调用之后，无待处理事件的 `WaitForEvent` 跟随实体 0 并安装 `SetControlledEntityActScript(0)`。BattleLoop 的 `PLAYER_TYPE=0` 选择 `eas_ControlledCharacter`。这是首个静态恢复控制边界，前提是先前服务完成。

[请求使用方](../../../research/map-event-request-consumption.md#confirmed-consumer-topology)拥有局部重置/轮询/清除接缝；研究所有者提供选定战斗返回组合。[配置数据合同](../contracts/map-setup-data.md#合同边界)保留实体列表身份，[地图合同](../contracts/map-exploration.md)拥有选择器及转换边界。清单和受控转换观察均不证明这整个自然返回。

当前场景跟随不等于下一地图保留实体 ID。清除实体会移除它们的位置、行进、脚本和映射，随后重建按标志选择随从行。这独立于交战 HP 和[出战队伍/脚本合同](../contracts/party-roster-state.md#处理器边界)。F74 准入三行，不保证随从总数。跳过 intro 而保留的 F83 可以在 F74 之外启用额外国王/大臣行；初始化器不去重。其他随从标志和冲突调用方配置仍是显式输入，不用假设修补。

### F999 改变有效目的地

| 返回输入 | 条件目的地/控制边界 |
| --- | --- |
| 首次胜利，F999 未设 | Map58 `(13,35)`、Down；F407 未设，F507/F385/F74/F640 已设。只有执行 intro 或调用方已建立时，F83 才为未设。上述控制安装前先经过空配置/随从重建。 |
| 重复胜利，F999 未设 | Map58，使用可变的初始返回位置/朝向；相同空配置控制路径，但由于跳过 after 场景，宝石/地震/随从标志仍作为传入状态保留。 |
| 请求 Map58，F999 已设 | `SwitchMap` 以相同坐标/朝向替换为 Map59。F999 已设的默认 Map59 init 可在 F443 已设时再请求传送；F543 配置变体进入不同场景。这些后期章节所有者不在范围内，因此不能对该状态普遍声称立即恢复控制。 |

## 5. 下一边界是独立的场地请求

**已确认紧接的出站记录：**Map58 的标志/踩踏表只有终止符。其位于 `0xB8A26` 的 `6-warp-events.asm` 记录为 `(255,37)` → Map60 `(8,52)`、Right、无滚动；255 是 X 通配符。这个较后的场地请求独立于战斗返回请求。下一所有者是 Map60 的地图内容/传送面，见[已接受出站边界](../../../research/battle07-outcomes-and-aftermath-handoff.md#persistent-effects-and-actual-return-consumer)。

**未知：**受控 Bowie 能否在实际布局、占位和随从条件下从 `(13,35)` 走到该标记。抵达 Map60、城堡/城镇撤离及 Battle08 需要各自已接受证据，才能继续综合。独立后续研究不是本指南消费的依赖。

接着读[塔后返回至地震城镇入场](post-tower-return-through-earthquake-entry.md)，了解已接受的条件塔楼/城堡请求、地震城镇选择及首次/重复 init，直至初始化后的控制/事件边界。

## 证据审阅与剩余限制

综合前审阅检查了已接受的 #716 研究、前篇指南与研究、相关合同、选定公开测试夹具载荷以及下列验证器/schema 边界。没有进行新的提取、保留输出比较、H2 重跑或运行时观察。研究所有者保留直接固定源码/文本审阅及复现回执；本指南消费该已接受溯源，不扩展其观察前沿。

| 关系 | 已审阅可执行面 / 复现路线 | 处置 |
| --- | --- | --- |
| 阵营检查、治疗和结果写入 | [循环测试夹具](../../../../tests/fixtures/h2/battle-loop-static-v1.json)，ID `sf2-battle-loop-static-v1`，`lifecycleFacts.remainingCombatants/betweenBattleHealing`；[控制测试夹具](../../../../tests/fixtures/h2/battle-control-static-v1.json)，ID `sf2-battle-control-static-v1`，`controlFacts.mainLoop/outcomes`；[循环](../../../../src/sf2tool/h2/battle_loop.py)及[控制](../../../../src/sf2tool/h2/battle_control.py)验证器。`uv run sf2 h2 battle-loop`、`battle-control`。 | 通用检查/治疗/结果事实一致；选定 Battle07 路由、初始返回字段及资源/撤离组合来自研究。不声称自然交战。 |
| after 分发/跳过、败敌门和共享加入 | [过场测试夹具](../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)，ID `sf2-battle-cutscenes-static-v1`，`cutsceneFacts.intro/afterBattle/enemyDefeated/leaderDeathPositions`；[验证器](../../../../src/sf2tool/h2/battle_cutscenes.py)、[夹具 schema](../../../../schemas/h2-battle-cutscenes-static-fixture.schema.json)。`uv run sf2 h2 battle-cutscenes`。 | 保留共享尾部/守卫顺序。选定行 7、零加入/首领行及缺少位置覆盖，仍是具名源码审阅事实，不是通用守卫建立的数量。 |
| 场景标志、传送与行动者/文本边界 | [脚本测试夹具](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，ID `sf2-map-script-engine-static-v1`，`storyStateCommandFacts`、`transitionCommandFacts`、`dialogueCommandFacts`、`entityPopulationCommandFacts`、`entityGestureRelationshipMotionCommandFacts`；[验证器](../../../../src/sf2tool/h2/map_script_engine.py)。`uv run sf2 h2 map-script-engine`。 | `abcs_battle07:305/394/395` 标志位置、`:1/390` 转换、`:2` 实体群及 `:391–393` 跟随顺序一致。完整阶段/文本与菜单/随从使用方使用已接受研究；清单不证明自然服务或端到端效果。 |
| 重置、替换、空配置和 init | [请求测试夹具](../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)，ID `sf2-map-event-request-consumption-static-v1`；[地图测试夹具](../../../../tests/fixtures/h2/common-maps-static-v1.json)，ID `sf2-common-maps-static-v1`，`mapFacts.mapSwitch/egress`；[配置测试夹具](../../../../tests/fixtures/h2/map-setup-static-v1.json)，ID `sf2-map-setup-static-v1`，`sourceFacts.selector/dispatch`；[init 测试夹具](../../../../tests/fixtures/h2/map-init-static-v1.json)，ID `sf2-map-init-static-v1`，`primarySourceBodies`；[请求](../../../../src/sf2tool/h2/map_event_request_consumption.py)、[地图](../../../../src/sf2tool/h2/maps.py)、[配置](../../../../src/sf2tool/h2/map_setup.py)及 [init](../../../../src/sf2tool/h2/map_init.py) 验证器。`uv run sf2 h2 map-event-request-consumption`、`common-maps`、`map-setup`、`map-init`。 | 局部接缝与更严格的条件组合一致。配置数据合同不消费选择器/分发行为；这些字段仍属地图探索。没有验证轨道闭合整个“场景 → 调用方 → 空配置 → 控制”链。 |

控制器概览较宽泛的 before/start 分组不能替代前篇精确的选定顺序。随从注释遗漏大臣；应使用三行表数据，并显式保留 F83/冲突标志。历史 H3 问题队列及扩展现有所有者的建议是未知处置，不是授权采集队列。这些更严格边界与已接受所有者一致；本篇不引入 Layer A 修正或新测试夹具。

**推断：**从对峙经失败营救到撤退的预期叙事连接。**未知：**自然胜利/败北/逃脱及传入存档点/标志/HP/出战队伍/行动者状态；等待、滚动、音频、呈现的完成和时长；冲突随从与重入；合法行走、占位/碰撞、后续路线、持久性和可取胜性。[缺口所有者](../../../research/battle07-outcomes-and-aftermath-handoff.md#coverage-gaps-and-reproduction)保留可区分条件的对照，如 F384 未设/F385 已设、F507 跳过、F83+F74 以及 F999 替换。

具名命令是复现路线，不是新的 PASS 声明。在启动命令的 PowerShell 进程中加载忽略的 `local/private-inputs.ps1`。研究所有者的[复现章节](../../../research/battle07-outcomes-and-aftermath-handoff.md#coverage-gaps-and-reproduction)链接保留载荷配方及精确选定源码/程序/文本锚。英文指南的文档验收使用直接正文/分支/链接/私有边界/范围检查、`git diff --check`、干净提交后的验证计划和实际 CI，不运行 normal/full/engine/H3/H4/Godot 或翻译套件。
