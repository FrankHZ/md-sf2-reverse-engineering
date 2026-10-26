# Battle10 结果至可选 East Shrine 访问

> 本文是[规范英文原文](../../synthesis/battle10-outcomes-through-east-shrine-visit.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

Battle10 胜利让出战队伍以战斗导出位置返回大地图。East Shrine 是独立可选访问：居民误以为访客是 Petro，祖父解释一块被盗古石，之后另一段对话提出协助前往 Grans。该邀请通过不同机制改变当前跟随与后续实体群体选择。

**已确认静态内容与控制流：**已接受的[研究所有者](../../../research/battle10-outcomes-through-east-shrine-visit.md)支持下文结果、选定 Map70→62 连接、调用方与状态变化。**推断：**此分支在向东遭遇之后发展 Sky Orb/Grans 故事。**未知：**自然穿行、实际行动者与映射、服务完成和连续可玩性。该访问不是已确立的 Battle11 必经路线，也不是重制就绪主张。

本文承接 [Battle10 准入](ribble-may-through-battle10-admission.md)，止于最终老人调用方之后及首个条件性探索控制恢复。南出口仅为接续边界；[选定返回至 Battle11 准入](east-shrine-return-through-battle11-admission.md)另有指南。后续旅行、洞窟/Battle11、wooden panel、Sky Orb 取得与之后神殿场景在本文之外。Fnnn 表示标志 nnn；地图/战斗 ID 保留源码编号。

## 1. 战斗结果通过不同目标返回

**已确认静态结果：**Battle10 没有敌方首领捷径。胜利需要至少一名计入己方且没有计入敌人；Bowie HP 为零强制败北，同时耗尽时走败北分支。仅击杀敌方槽位 128 既不获胜，也不移除其他敌人。row-10 defeated 与 after-battle 脚本均为 `ms_Empty`；空脚本仍执行其终止符的条件性滚动等待与速度清除。

| 结果 | 返回时的后果 |
| --- | --- |
| 胜利 | 治疗存活/不死己方角色，在 `UpdateForce` 后选择首名出战队伍成员，并由其导出返回坐标/朝向。Map70 战斗区域偏移为 `(0,0)`，因此使用该成员战斗员坐标。清除 F410、设置 F510。 |
| 败北 | 呈现耗尽文本、恢复 Bowie HP、通过无符号移位将金币减半，并返回携带 egress 地图的存档点元组。不新设 F510。 |
| Egress / Angel Wing | 消耗法术 MP 或物品并执行其服务，然后返回 egress 元组，不施加败北惩罚或胜利完成写入。 |

败北/egress 仅在 F510 原已设时清除 F410；首次战斗撤退保留提供的解锁。携带的 egress 地图决定目标，包括存档点查找未匹配地图时的回退。本文不确立最近存档点或神殿目标。胜利治疗不复活死亡且非不死的己方角色。

after-battle 共享尾部仍为 row 10 的零/Bowie 条目调用 `JoinForce`，即使 F510 导致脚本跳过。它通过普通加入/容量使用方设置 F0 并条件性设置 F32；不招募 May 或祖父。位置选择先于该尾部。

即时胜利返回时，main 切换地图并进入探索，没有第二次 `CheckBattle`。Map70 没有切换地图替换，固定实体群体为空，init 为 `rts`。普通提供入场重建实体和标志随从、清除临时标志与待处理，然后加载/初始化地图。服务之后待处理为零才允许控制恢复。携带的 F414 不截获本次即时返回。

之后 Map70 zone 请求保持独立。[准入指南](ribble-may-through-battle10-admission.md#6-zone-请求与战斗选择器决定启动内容)保留特定 Battle14 辅助、默认 Battle10 请求和有序选择器。F510 抑制默认请求，但不清除提供的 F410；F410/F510 均已设时，在后续调用中仍可选择 Battle10。这些规则不确立从胜利位置不受打断地步行。

## 2. 进入 East Shrine 是独立可选连接

**已确认条件连接：**选定 Map70 传送是 `(43,40)`，不滚动，通往 East Shrine（Map62）`(6,18)`、Up。移动必须遇到传送布局属性及匹配行、产生完整请求，并以完整请求到达使用方。坐标本身不证明可导航路径。选定传送与 Map62 准入均不测试 Battle10 胜利、May 招募、Peter 存在或神殿故事标志。可选性来自这些谓词。

Map62 没有战斗坐标行或按标志切换的替换。普通入场重建实体群体、重置临时标志 256–383 并运行 `rts` init。入场自身不启动访问场景。若服务以待处理为零返回，探索可首次在 `(6,18)`、Up 恢复控制；尚未观察到该处响应输入。

F77 未设时，固定实体群体将年轻人声明为事件身份 128，位于 `(6,11)`、Up；祖父为 129，位于 `(5,6)`、Down。这些是编写原点及查找身份，不是观察位置或物理槽位。标志随从先被声明，改变物理 NPC 槽位。F67 即使在 Peter 死亡时也可将其声明为 character 7；仅对话请求不确立说话者存活或存在。

两个 NPC 事件行让激活行动者转向玩家，并在返回时恢复先前朝向。其 `DOWN=3` 控制字节不是靠近方向要求。普通激活要求当前行动者/映射，并排除受控实体和当前随从。

## 3. 问候、解释与邀请具有独立门控

**已确认静态内容：**问候询问 Petro，并将来者误认为他。老人索要 Sky Orb、向访客移动，随后认出误会。首段对话解释家族古石被孙子偷走，以及孙子为操作古代交通工具而前往 Grans。独立 zone 场景提出帮助并邀请出战队伍交谈。下一段老人对话解释一条可通往 Grans 的隧道；Peter 询问如何到达 Grans，老人邀请出战队伍，旁白称领队决定陪同他，老人请求帮助找回 Orb。

这些描述概述已接受的文本请求；叙事节点顺序不使其成为强制互动序列。完整局部谓词如下：

| 触发 | 门控与结果 |
| --- | --- |
| zone `(5,15)` 或 `(6,15)` | F729 未设且 F256 未设：运行问候/身份误认 `cs_5C876`，然后调用方设置 F256。否则返回。 |
| zone `(5,16)` 或 `(6,16)` | F729 未设、F256 已设且 F257 未设：运行帮助/交谈邀请 `cs_5C8FE`，然后调用方设置 F257。否则返回。 |
| 老人 129，F729 未设 / F728 未设 | 运行祖父叙述 `cs_5C8D4`，然后调用方设置 F728。 |
| 老人 129，F729 未设 / F728 已设 | 运行旅行邀请 `cs_5C914`；其跟随尝试及 F77 写入后，调用方设置 F729。 |
| 老人 129，F729 已设 | 请求文本 2512，称此路线无法到达 Grans，然后返回。跟随之后是否能自然激活是独立问题。 |

两段老人对话均不测试 zone 标志、F510、F67 或 F77。若物理激活可行，两次交谈即可在未运行任一 zone 场景时到达最终对话。F256 一旦已设，第二个 zone 可在第一次交谈之前运行，因为它不测试 F728。F256 未设时跨过 Y16 无事发生；之后再跨过则可请求邀请，而不需要先前尝试离开。

这四个程序均没有是/否命令或拒绝分支。它们不写入传送、加入、军团 AI、实体群体重载或地图图块变化。邀请措辞不是玩家选择证据，描述隧道也不是此处出战队伍通过隧道旅行的证据。

问候请求老人总计 Down 6 的移动；首段叙述请求 Up 6。这些是带等待的相对行动，不是绝对位置重置。在问候之前交谈使用行动者实际位置。摄像机和文本服务也会等待；没有显式 sleep 记录不等于没有等待（`nextSingleText` 有共享 sleep 服务）。源码顺序证明请求，不证明无碰撞完成、时序、已渲染文本或响应输入。文本 ID 描述普通未跳过路径。

## 4. 再访保留对话状态但重置局部遭遇

**已确认状态生命周期：**F728、F729 和 F77 是独立持久游戏标志。F256、F257、F258 是普通提供地图入场时清除的入场局部临时标志。F729 未设时，即使 F728 保留，重新入场仍可再次允许两个 zone 场景；下一次老人交谈则使用 F728。F729 已设抑制两个 zone。F77 选择实体群体，而非门控任一对话或 zone。

F77 已设配置仅保留年轻人 128，编写位置 `(7,5)`、Down。交谈期间设置 F77 不重载当前实体群体。反之，提供 F77 已设/F729 未设的入场可在没有 129 映射时选择 zone 脚本；这既不是安全跳过，也未确立为自然状态。不臆造这三个持久标志之间的蕴含关系。

年轻人提供有界背景。F732 优先选择之后对无法进行旅行的哀叹；本指南既不纳入其产生方，也不证明那次旅行。否则，F258 未设会请求他叙述祖父对 Sky Orb 的研究、祖先的飞行和 Nazca Ship 最后已知位置。仅在这些文本之后，F729 已设才导致设置 F258。因此 F729 未设会重复该叙述；F729 已设则允许每次普通入场一次，之后使用 F258 选择的告别和给兄弟的口信。提供 F258 已设时，即使 F729 未设也选择告别。这些说明及后续替代是对话背景，不是取得物品或旅行证据。

## 5. 当前跟随与之后的 F77 随从不同

**已确认当前命令：**最终对话请求 `followEntity 129,FOLLOWER_B,2`，然后设置 F77。`FOLLOWER_B=31` 是查找身份，区别于 Peter 的 7 和 `FOLLOWER_A=30`。三个字操作数是 `$0081,$001F,$0002`。在解析任一实体前，处理器探测首字高字节 0，因此其 HP 守卫在此检查 Bowie。它不验证老人、Peter、目标、映射新鲜度或容量。Bowie HP 为零会跳过跟随操作；若其余执行完成，后续 F77 和调用方 F729 写入仍发生。

守卫允许执行时，目标来自实际 character-31 查找。仅 F67 将 Peter 映射为 7，不提供 31 映射：其已清零字节解析为物理槽位 0/main，这是零映射后果，不是显式 Bowie 回退或 Peter 同义词。F74 可将 Astral 映射为 31，F68 可用 Oddler 覆盖，F70 可再用 Astral 覆盖，顺序按源码如此。其他随从或临时 May 不会以当前列表尾部替换 31。[研究映射表](../../../research/battle10-outcomes-through-east-shrine-visit.md#current-following-versus-the-f77-rebuild)拥有完整提供状态控制。

`AddFollower` 用循环跟随关系替换解析后 129 行动者的行动块，修补物理目标和 `(-24,0)` 偏移，再在尚未列入时追加其物理索引。替换先于重复检查。不同于 May 的 `addNewFollower`，此命令不选择列表尾部。它不等待可见跟随完成，也不保证间距。缺失的 129 同样解析为槽位 0，因此强制缺失映射可能改变 main，甚至令其跟随自身。

在普通实体群体中成功跟随时，老人仍是当前地图身份 129，但作为当前随从被普通互动排除。F729 重复文本不证明可以立即再次与他交谈。脚本 F77 和调用方 F729 是独立于当前行动/列表变更的结果。

之后普通入场清除映射与 walking 分配，并重建随从列表。F77 以 character **30** 声明老人精灵，而固定实体群体不再声明 129。仅 F77 时，老人在物理槽位 1 跟随 main；F67 加 F77 时，Peter 在槽位 1，老人在槽位 2，通过按序数选定的随从行动跟随 Peter。其他标志改变此顺序；不由此推出任意数量的目标/容量保证。之后 F83 king 行可覆盖 character 30 查找。Character 30 没有显式 Map62 事件行，且是随从，而非保留的事件 129。

F77/F728/F729 在游戏标志存档域内；临时随从列表、walking 指针与行动存储在逻辑存档范围之外。这一区别不证明自然存档/重新入场呈现或物理跨进程持久性。这些仍为**未知**。

## 6. 最终调用方交回条件性探索控制

**已确认静态终点：**最终脚本返回后，老人调用方设置 F729。实体事件分发器恢复先前朝向、关闭窗口并重新启用更新。在待处理为零的 `WaitForEvent` 读取时，探索恢复摄像机目标物理 0 和受控实体行动脚本。这是首个条件性控制交接，不是等待异步随从循环。zone 调用方尾部不同，带有 VInt/物理 main 停止等待；两个尾部均不证明每项服务/行动者完成，也不丢弃新产生的待处理请求。

南出口行为 X 通配、Y19，不滚动，通往 Map70 `(43,41)`、Down，没有 F77/F728/F729 门控。它仍要求布局/请求路径。这仅是接续边界：本文不主张后续准入、Grans 旅行、Orb 取得或 Battle11。选定连接请继续阅读独立的[返回与洞窟准入指南](east-shrine-return-through-battle11-admission.md)。

## 证据审阅与剩余限制

本指南使用 PR #766 接受的研究及 PR #765 接受的前序指南。研究将 USA retail 绑定到固定 SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6`，拥有精确源码/文本组合及复现。直接审阅下列受跟踪选择与 schema/使用方边界，未发现必须修正的 Layer A。不需要可选派生载荷；不主张提取、上游重新调查或新运行时观察。

| 审阅关系 | 可执行/合同边界与限制 |
| --- | --- |
| 结果与包装器 | [sf2-battle-global-data-static-v1](../../../../tests/fixtures/h2/battle-global-data-static-v1.json)、[sf2-battle-routing-data-static-v1](../../../../tests/fixtures/h2/battle-routing-data-static-v1.json)、[sf2-battle-cutscenes-static-v1](../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)及[过场合同](../contracts/battle-cutscene-routing.md)。routing 有 `routeTargetsParsed=false`；精确 row-10 目标与返回组合来自已接受研究，而非汇总计数。 |
| 入场与实体群体 | [sf2-map-data-static-v1](../../../../tests/fixtures/h2/map-data-static-v1.json)、[sf2-map-setup-static-v1](../../../../tests/fixtures/h2/map-setup-static-v1.json)、[sf2-map-entities-static-v1](../../../../tests/fixtures/h2/map-entities-static-v1.json)、[sf2-map-init-static-v1](../../../../tests/fixtures/h2/map-init-static-v1.json) 主体 71，以及[地图/探索合同](../contracts/map-exploration.md)。选择器、实体群体声明与 `rts` 不确立自然行动者或可穿行传送。 |
| 对话/zone 谓词 | [sf2-map-events-static-v1](../../../../tests/fixtures/h2/map-events-static-v1.json)、[实体分片](../../../../tests/fixtures/h2/map-events/entity-programs.json) 顺序 610–612 和 [zone 分片](../../../../tests/fixtures/h2/map-events/zone-programs.json) 顺序 112–114。有序操作/原始操作数确认独立门控与调用方写入；四个完整短脚本/文本序列仍归研究所有。 |
| 跟随守卫与生命周期 | [sf2-map-script-engine-static-v1](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，`entityGestureRelationshipMotionCommandFacts`，以及 [sf2-map-entity-gesture-relationship-motion-runtime-v1](../../../../tests/fixtures/h3/map-entity-gesture-relationship-motion-v1.json)，用例 `follow-selector-zero-live`、`follow-selector-seven-live`、`follow-zero-current-hp`；[出战队伍/名册合同](../contracts/party-roster-state.md)区分 May 的另一使用方。处理器守卫和受控高字节 HP 观察，不在运行时闭合本神殿组合。 |

[事件夹具加载器](../../../../src/sf2tool/h2/map_events_fixture.py)与[验证器](../../../../src/sf2tool/h2/map_events.py)保留有序分片/原始操作数；[地图 init schema](../../../../schemas/h2-map-init-static-fixture.schema.json)与[验证器](../../../../src/sf2tool/h2/map_init.py)保留主体。[地图脚本 schema 组合](../../../../src/sf2tool/h2/map_script_schema.py)、[验证器](../../../../src/sf2tool/h2/map_script_engine.py)、[关系夹具 schema](../../../../schemas/h3-map-entity-gesture-relationship-motion-fixture.schema.json)及[观察器](../../../../src/sf2tool/h3/map_entity_gesture_relationship_motion.py)保留处理器与受控用例边界。研究的[验证/复现边界](../../../research/battle10-outcomes-through-east-shrine-visit.md#final-caller-continuation-and-verification-boundary)拥有窄比较；不单为本篇文字选择整套测试。

**未知：**自然路径/碰撞，实际激活行动者/映射/HP/出战队伍/随从，已完成文本/输入/滚动/实体服务，待处理请求存续，可见跟随，任意随从容量，存档/重新入场呈现，以及连续可玩性。这些限制不创建模拟器工作队列。本指南不选择引擎/产品，也未完成战役。
