# 返回 New Granseal 与 Astral 跟随

> 本文是[规范英文原文：New Granseal Return and Astral Following](../../../../synthesis/chapters/northern-travel-mitula/new-granseal-return-and-astral-following.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。文末审阅、环境与验证记录保留原英文综合切片的时间范围。

回到 New Granseal Castle，minister 欢迎队伍，Astral 询问新伙伴。Peter 说 Elis 还活着。Bowie 和 Peter 随后讲述经历，king 因而要求打败 Zeon、救出 Elis。Astral 提议去 Tristan 求知，并请求以顾问身份同行。场景宣布这个身份，再改变当前跟随关系，记录供后续城堡访问使用的标志。

**推断：**这是一次回顾式觐见，让 Astral 作为顾问加入旅程。**已确认：**汇报、宣布、当前行动者关系、调用方标志与后续随从重建是不同操作。场景不救出 Elis、不招募战斗伙伴，也不向北旅行。**未知：**自然抵达、实际传入状态、表现完成、可用控制与存档持久性。

本文接续 [Creed 离开、矮人恢复与 Cannon 赠礼](../southern-travel-creed/creed-departure-dwarf-recovery-and-cannon-gift.md)，依据 main `af7a1e656eddc4a0252b9f5cee4bbe31288369a3` 上[已接受的 #965 研究](../../../../../research/chapters/northern-travel-mitula/new-granseal-return-and-astral-following.md)及其前序成果。指南止于觐见有条件的调用方返回，以及相关当前/重新入场后果。Janet、发明家赠礼与 North Cave 爆破是后续边界。Inventor/Dynamite 属于独立研究成果，不是本指南的证据依赖。此 Layer B 解释不选择重制规则或新观察工作。

## 从矿洞返回

**已确认：**选定连接为矿洞内部 Map30 → Southwest Parmecia Map68 → Map73 → 城镇 Map6 → 城堡 Map7。[路线所有者](../../../../../research/chapters/northern-travel-mitula/new-granseal-return-and-astral-following.md#selected-return-route-and-admission)保留精确表行、请求消费者与坐标前提。这些是条件替代路径，不是强制行程或已观察的步行。持有 Cannon 和 F761 都不是这些表行或觐见的前提。

Map30 在任意 X/Y21 的出口行请求无滚动进入 Map68 `(57,25)` Down。该无滚动返回可能遇到 Map68 的 Battle19 准入：F419 选择它，已设置的 F519 仅在选择后清除 F419，该次调用仍返回 Battle19。返回矩形是战斗几何，不是范围包含测试。继续旅行假定没有拦截（通常 F419 未设）；获准战斗是停止边界。

Map68 任意 X/Y0 的北边界请求 Up scroll17 进入 Map73，保留 X，将 Y 放在62。它留在探索内部：保留行动者与跟随关系，采样当前朝向而不应用表行的 Up 朝向，跳过重新治疗、战斗选择、居民构建、临时重置与 Map73 init。Map68 的随机 Battle19 区域（事件）是独立移动边界；Map73 默认区域（事件）只返回。Caravan mode1 没有步行的区域（事件）产生器；本路线不登乘或登陆交通工具，也不规范化携带的玩家/木筏状态。

Map73 的 `(29,52)` 行请求无滚动进入城镇6 `(20,30)` Up；城镇6 北边界请求同样无滚动进入城堡7 `(11,22)` Up。Map73/6/7 没有战斗坐标行，Maps68/73/6/7 均不被全局标志切换。其他出口或待处理事件可打断拟定路线。普通移动仍需适当工作布局属性、碰撞准入，以及能保留到分发时的请求。传入坐标高字节也影响无滚动字节替换。

获准重新入场的地图重建行动者，应用已接受的存活/不死治疗，清除临时标志256–383 与待处理状态，设置 F80 并运行选定 init。CURRENT255 复用与边界滚动保留不同人口。Map68 居民为空，init 仅返回。重新进入73 的居民也为空，但 F700 未设时可运行此前航行，或在 F700/F508 已设、F705 未设时运行建设；选定滚动绕过这些分支。普通后期历史路线携带 F700/F508/F705/F707/F701/F702 和 Peter 的 F67。这些是历史前提，不是额外传送或觐见测试。

## 返回时谁在场

**已确认：**[人口与 init 选择](../../../../../research/chapters/northern-travel-mitula/new-granseal-return-and-astral-following.md#town-and-castle-population-and-init)独立于返回觐见，依赖更早历史。城镇 Map6 依次选择 default、F701、F702，最后适用选择器胜出。F802/F805 不选择居民列表。早期 default 声明 Peter 与19 名 NPC；后两个列表各声明14 名 NPC，没有固定 Peter。它们的位置/行动不同，完整表由研究所有者保留。标志选择的随从在居民之前构建，所以仅选择后期城镇配置不建立实际 Peter 在场。

城镇共享 init 优先检查 F802：将 priest132 放到 `(11,18)` Down，不检查 F706 便返回。F802 未设/F706 已设时，改将 worker138 放到 `(63,63)` Down、guard139 放到 `(15,23)` Down；两者均未设则返回。这些是当前查找重定位，不是隐藏或持久人口变化，均不运行觐见。

城堡 Map7 依次选择 default、F701、F702→default、F805。因此 F702 把 F701 选择恢复为 default，F805 则胜过两者。Default 有12 名 NPC；F805 有相同前11 名，省略 Astral139。F701 在12 名居民前有五个固定己方角色声明，包括 Peter。如果 Peter 已映射为随从，则跳过其固定声明。Default 反而需要传入 Peter 映射，通常是 F67。所有指针表共用普通城堡 init 与区域（事件）表；没有独立 F805 init。

预期觐见行动者为 minister128、King Granseal129、Astral139、Peter7 与 Bowie0。Default 的 minister/king/Astral 声明分别为 `(12,5)` Down、`(11,4)` Down、`(10,5)` Down，采用较慢 Init。F701 则使用普通 Init，Astral 在 `(10,6)` Right。完整 guard/soldier/maid 与己方角色声明留在研究所有者；变化人口不能默认为同等布置。

共享城堡 init 先检查 F707。已设置便立即返回，保留传入 Egress 目的地和 F411。未设置则指定 Egress map6，运行[早期一年后场景与显式 default 居民重载](../../../../../research/chapters/eastern-parmecia/new-granseal-castle-year-later-control.md#reload-caller-flags-and-first-control-read)，再在场景返回后设置 F707 和 F411。因此即使 F805 选择的入场，在 F707 未设时也可被 default 居民替换。F411 自身不是即时战斗请求。普通返回布置假定 F707 已设；缺失历史仍是另一用例。

引用是查找选择器，不是物理槽位。解析器掩码为一个字节，读取映射，将物理索引乘32。缺失普通映射可保持0，别名到主行动者；隐藏映射可为 -1，解析器没有安全守卫。重复映射可覆盖彼此的行动、台词上下文或跟随。Resident139 与随从 B31 即使都用 Astral 精灵，仍相互独立。任何声明都不证明军团成员资格、HP、Init 完成或行动者可正常服务。

## 觐见准入

**已确认：**城堡五条显式区域（事件）行位于 Y8、X9–13，目标为 Map7_ZoneEvent0。[区域（事件）所有者](../../../../../research/chapters/northern-travel-mitula/new-granseal-return-and-astral-following.md#complete-castle-zone-admission)保留精确共享 default 返回。坐标本身不调用它：普通移动必须产生布局区域（事件）请求，由消费者初始化 physical0，再分发选定回调。

| 回调状态 | 条件结果 |
| --- | --- |
| F802 已设，F805 未设 | 返回场景 → 调用方设置 F805 → 调用方设置 F70 → 返回。 |
| F802 已设，F805 已设 | 返回，不运行场景，也不修复缺失 F70。 |
| F802 未设，F701 已设，F702 未设 | 早期 Peter 觐见 → 设置 F702 → 设置 F67 → 返回；这里不测试 F805。 |
| F802 未设，F701 未设或 F702 已设 | 返回，两种觐见都不运行。 |

F707、F70、Cannon 与 F761 不是局部准入测试。F802 优先于旧701/702 分支；F802 未设时，传入 F805 不抑制旧分支。根据变化标志选择回调，也不重建当前阵容。以下故事描述预期映射且服务能够返回时获准的返回场景。

## 欢迎与 Peter 汇报

**已确认：**[完整场景](../../../../../research/chapters/northern-travel-mitula/new-granseal-return-and-astral-following.md#complete-return-scene-and-action-premises)先初始化 Peter 并等待。Minister 欢迎 LEADER；给 Bowie 目的地 `(11,7)`，不等待坐标；给 Peter `(10,8)` 并等待。摄像机请求 `(6,2)`，Bowie/Peter 朝 Up。Bowie 目的地编码字32768 解析为行动者0 加不等待位，不是额外行动者。两个目的地均不测试 HP。

Astral 评论交换伙伴，minister 欢迎新朋友并赞扬队伍，Astral 汇报 Bedoe 消息：Zeon 复活、Volcanon 愤怒。Minister 问起 Elis，Peter 说她活着。King、Astral 与 minister 接受速度48/48 自定义行动，在一次场景暂停与战场死亡音效请求期间暂停动画。他们分两组跳跃，每组只等待 minister，再恢复动画。暂停动画不等于暂停移动。

King 接受速度56/56，沿等待的 Down2,Left1,Down1 路径移动。Peter 跳跃；king 问 Elis 是否活着，Peter 犹豫回答，Astral 要求确认，Peter 确认。King/Astral/minister 随后接受速度32/32；king 沿等待的 Up1,Right1,Up2 路径返回，朝 Down。正常完成时这些 king 路径相互抵消，但变化运动/碰撞或映射下不能赋予通用终点。

King 的问题和 Peter 的回答是对话，不创建 yes/no 菜单，也不变更营救状态。LEADER 是 UpdateForce 后第一个出战军团成员的名字，可不同于固定行动者 Bowie0；Peter 的插入名字同样不证明他在场或存活。

## 回顾旅程

**已确认：**场景在回顾布置前调用调色板重映射辅助程序。Peter 接受速度48/48，Up1 并等待；随着讲述推进，角色点头、跳跃与转向。后续一次 Peter Up1 不等待，接着的跳跃可替换其行动指针。简单累加 Peter 的所有移动操作数不能证明最终位置。

旁白说 LEADER 和 Peter 讲述经历。Peter 移动 Left1,Up1 并点头；Astral 转 Right，minister 转 Left，king 跳跃并点头。Bowie 移动 Left1,Up1，在 Astral 转 Down 时点头，再 Down1,Right1 返回，朝 Up。叙述回顾古代遗迹、Creed，以及 Zeon 经 Evil Spilit（源码拼法）传来的消息，然后标记讲述完成。这是汇报先前事件，不重放那些场景，也不证明自然经过其间流程。

Peter 的 Down1,Left1,Down1 行动安装后不立即等待；minister 朝 Down，然后场景显式等待 Peter 的 Idle 指针并让他朝 Up。随后调用调色板恢复，再暂停。Minister 惊叹 LEADER、Peter 及其经历；Astral 沉思，minister 向他说话。

[调色板所有者](../../../../../research/chapters/northern-travel-mitula/new-granseal-return-and-astral-following.md#palette-remap-and-restore)在这两个调用处复用已接受的咨询辅助程序。第一个通过已接受颜色表把64 个 CURRENT 颜色重映射到 BASE；它不是实测灰度或棕褐色外观的证据。第二个将选定 COPY 快照复制到 BASE，**然后才用**当时的当前调色板**替换 COPY**。它不重载不可变房间调色板。两者均清除自动反转字节，请求共享32 次更新插值与 CRAM DMA；均不等待全部32 次更新。暂停/文本不证明可见完成，中断或其他调色板服务可影响结果。

## 选择下一目标与顾问

**已确认：**Astral 问下一步做什么。King 要求打败 Zeon、救 Elis。Minister 接受速度40/40，向 Left 走一步，朝 Up 并反对。暂停后 Astral 转 Up，同意 king。Minister 惊讶地跳跃、转 Left；king 感谢 Astral 并问第一步。Minister 向 Right 移动，再朝 Left。

Astral 提议前往 North Parmecia，向 Tristan 的说书人求知。Minister 发抖、惊呼。Astral 转 Right；旁白描述他眨眼微笑，然后他朝 Up，请求允许陪伴 LEADER。眨眼是叙述，不是单独眨眼行动。King 同意，请他救 Elis。Minister 警告危险；Astral 转 Right、点头答应。他随后朝 Down，对 LEADER 说将作为顾问同行。

这是关于下一目标的交谈，不会自动把队伍移过去。宣布请求加入音乐，叙述顾问加入军团。场景没有 joinForce、joinForceAI、joinBattleParty、命名或出战容量操作。因此不能把地图跟随描述为战斗军团招募。

## 宣布、输入与当前跟随

**已确认：**[文本与即时服务](../../../../../research/chapters/northern-travel-mitula/new-granseal-return-and-astral-following.md#text-identity-and-service-sequence)保留41 个有序位置1249–1289，本指南不逐字转录。普通 minister 没有立绘，bleep5；king 使用 portrait32/bleep6，Astral 使用 portrait31/bleep6，Peter 的 portrait/bleep 依赖精灵。变化精灵、别名或已打开立绘窗口会改变上下文。Narrator 操作数选择 speech0，绕过立绘解析，不是行动者255。King 的立绘修饰符改变选项，不改变行动者129。

单段文本等待视野滚动、显示、推进游标，再关闭立绘/文本并睡眠10。连续文本保留窗口。N 表示换行；W1/W2 等待输入，W2 带有继续/确认音效行为。最终建议既有内部 W2，又有末尾 W1；更早连续 Zeon 汇报的末尾 W1 仍是等待。没有 yes/no 决策。脚本入场清除文本跳过后，Debug/P2 Start 可锁存跳过，略过显示/游标推进及编码脚本暂停/等待，而其他命令继续。锁存后，后续显示文本未必维持名义位置到 ID 映射。

顾问宣布本身没有 W1/W2。随后直接辅助程序通过声音服务，在当前音乐之后请求先前音乐，再等待玩家输入。尽管名字含 FadeOut，它不执行调色板淡出。它接受当前按住的任意方向/A/B/C/Start，否则等待重试；不要求新按下边沿，也不特指 A/C。声音轮询是即时命令服务边界，不证明短曲已可听地结束。即使叙述被跳过，仍到达这个直接辅助/输入调用。

只有它返回后，场景才关闭文本，让 Astral 建议关于岩石、北方通道与爆炸物的事。这不使用 Cannon、不赠物，也不爆破洞穴。Bowie 点头，随后场景请求 Peter 以 direction2 跟随 Bowie。该字的**高字节**是 HP 守卫：Peter 的字7 检查 Bowie 的 HP，不是 Peter 的。Bowie HP0 跳过该关系；Peter HP0 独立跳过 Peter 朝向命令。目的地、Init/行动、手势与 Astral 添加没有对应 HP 守卫。

获准 Peter 关系安装偏移 `(-24,0)` 的随从行动，并在探索列表尚无其物理行动者时追加。已在列表的 Peter 不移到末尾。addNewFollower139 随后解析当前 Astral，选择列表的**实际最后物理条目**，空列表则选0，使用相同偏移。刚追加的 Peter 可以是目标；若已有 Peter 后面还有随从，则目标未必是他。操作不强制 Peter 目标、不验证列表容量，也不改变 joined/active/AI 状态或 F67。行动安装后才去重。强制重复而 Astral 已是最后条目时，可安装自跟随。

Init/自定义/内联行动替换行动指针，不独立修复映射、位置或列表顺序。自定义速度改变打包 X/Y 速度字节，保留继承碰撞/加速度设置，不授予穿墙。等待某行动者 Idle、坐标、摄像机滚动或点头/摇头/发抖服务都不是全员屏障。其他行动者可在文本与手势期间继续。这也适用于前述未等待布置。

## 当前返回与后续重建

**已确认：**[跟随与调用方返回](../../../../../research/chapters/northern-travel-mitula/new-granseal-return-and-astral-following.md#following-caller-return-and-changed-state)让当前 Astral 仍是 resident139。场景不把他重命名为 B31，也不重载阵容。它在脚本内设置 F70 后结束；脚本返回后，区域（事件）调用方才设置 F805 并再次设置 F70。脚本结束有条件地等待视野滚动并返回；区域（事件）包装器关闭窗口，等待一个 VInt 及 physical0 坐标相等。没有一个等待全部随从、确认可用控制或执行原子存档事务。

后续普通重建时，F70 在居民前选择 B31/Astral 随从声明。其物理分配与首领依赖此前启用的随从声明。多个 B 声明不去重：后来的 F70 可覆盖 lookup31，而更早 F74/Astral 或 F68/Oddler 分配仍留在列表。城堡 Map7 使用普通随从表，没有 Caravan 大地图前缀。F805 则独立选择不含139 的居民。

| 变化状态 | 后果 |
| --- | --- |
| 重新入场，F805/F707 已设、F70 未设 | 没有 resident139 或 F70 派生 B；F802 已设的区域（事件）跳过，不修复 F70。其他随从独立。 |
| 重新入场，F70 已设、F805 未设 | B/Astral 可与 resident139 共存；符合资格的觐见可再追加一个 Astral。 |
| F707 未设，即使有 F805 | 早期 init 重载 default 居民；保留其不同历史/人口。 |
| 仅 F701，F802 已设 | 即使旧觐见未完成，返回场景仍胜出；选定位置/行动不同。 |
| 行动者缺失/隐藏/重复、HP 或随从顺序变化 | 实际解析器/守卫/列表效果取代名义布置；不自动安全重建。 |
| 在场景 F70 前/场景 F70 后/调用方 F805 后中断 | 尚无新 F70 / F70 可先于 F805 / F805 可先于再次 F70 与包装器完成。没有回滚修复前缀。 |
| 直接调用脚本或强制重复 | 脚本内没有 F802/F805 测试或 F805 写入；文本/行动/跟随可重复。普通 F802+F805 区域（事件）再入跳过。 |

Janet 城镇区域（事件）有自己的 F805 已设/F806 未设准入。发明家后续回调优先 F802，在 F805 前测试 F804，并在相关分支搜索实际 Cannon 物品栏。这些是独立后续边界，不是觐见前提。其场景、赠礼和 North Cave 爆破仍在本指南之外。觐见不写 F761/F804/F806、不转移物品，也不请求传送或战斗。

**未知：**自然路线/区域（事件）可达；传入标志、布局、坐标、模式、HP、职业、人口、查找/列表顺序与可正常服务行动者；移动/碰撞/文本/立绘/输入/视野/调色板/DMA/音频完成；可见回顾/顾问表现、可用控制与存档/读档持久性。这些限制不创建自动捕获或采集队列。

接续 [New Granseal 发明家与 Dynamite 赠礼](new-granseal-inventor-and-dynamite-gift.md)，了解独立许可、实际 Cannon 搜索与收取尝试，直至发明家局部返回。

## 证据审查与保留结果

对抗审查检查已接受 Layer A 正文及选定公开测试夹具/schema/消费者表面，未发现阻断矛盾。公开处理器事实与源码关联不包含、也未新近复现完整私有场景语料。指南全篇采用已接受的更严格静态边界。

| 已审查表面 | 处置 |
| --- | --- |
| [#965 研究](../../../../../research/chapters/northern-travel-mitula/new-granseal-return-and-astral-following.md)及已接受路线、城镇、一年后、调色板与矮人前序成果 | 完整返回故事、选定人口/历史、Battle19 拦截、行动者身份、服务/顺序与变化状态限制一致。 |
| `sf2-map-events-static-v1`：[routing](../../../../../../tests/fixtures/h2/map-events/routing-setup.json)、[zones](../../../../../../tests/fixtures/h2/map-events/zone-programs.json)、[zone schema](../../../../../../schemas/h2/map-events-zone-programs.schema.json) | 五条显式行加 default、16+1 个回调操作/共享 rts，以及 F802/F805 对701/702 的极性一致。发明家范围止于已识别准入边界。 |
| `sf2-map-init-static-v1`：[fixture](../../../../../../tests/fixtures/h2/map-init-static-v1.json) | 选定 Map6/7/68/73 主体保留8/7/1/13 个有序操作；城镇 F802 优先与共享 F707 城堡 init 不意味着滚动重新 init 或独立 F805 主体。 |
| `sf2-map-script-engine-static-v1`：[fixture](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)、[schema](../../../../../../schemas/h2/map-script-engine-fixture.schema.json) | 选定布置/行动/生命周期/关系/名册事实保留操作数宽度、守卫、调用与源码身份。合法处理器夹具不证明自然场景完成。 |
| [事件加载器](../../../../../../src/sf2tool/h2/map_events_fixture.py)、[事件验证器](../../../../../../src/sf2tool/h2/map_events.py)、[脚本验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)、[名册合同](../../../contracts/party/party-roster-state.md#处理器边界) | Schema/分片身份/顺序、完整源码输出相等与溯源是独立检查；随从分配不是军团成员资格或场景专属存档验收。 |

[复现所有者](../../../../../research/chapters/northern-travel-mitula/new-granseal-return-and-astral-following.md#reproduction-and-review-boundary)固定 SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` 与 USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。所属命令仍为 `uv run sf2 h2 map-init`、`uv run sf2 h2 map-events`、`uv run sf2 h2 map-script-engine`。原综合切片工作树缺少固定上游/保留数据对：不采集、不重新生成，也不声称新比较。

[#965 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/965#issuecomment-6179605996)与 [PR968 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/968#issuecomment-6179629067)保留最终4f09a34a/tree fe46c1e9/base 7c8a9760 及原始97181f64/CI38645456942；最终 CI38645631608 通过 scope15s/research46s。不变的18,285,979 字节数据对的0.087/0.081/0.080s 回执仍相互区别；main-gate 保留其自身0.080s 完整关联比较。这些关联包含134 条记录、九个自定义及11 个移动主体、41 个文本位置和774 字节区间。保留 Map68 Battle19 与32 字节实体步长修正及检查诊断。

[#966 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/966#issuecomment-6179544821)与 [PR967 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/967#issuecomment-6179565201)保留最终 bebcdeda/CI38645261417、原始 CI38645151326 和闪光/淡入淡出顺序澄清。前序指南的[保留结果](../southern-travel-creed/creed-departure-dwarf-recovery-and-cannon-gift.md#证据审阅与保留结果)保留 #961 原始/rebase/验收回执、编码大小修正，链接的 #957/#953/#949/#945/#941/#937 失败/结果/未知项、[#954 回调修正](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/954#issuecomment-6178761171)及 [#962 Fairy 修正](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/962#issuecomment-6179318374)。Fairy 在宅邸的 `(17,10)` Up/eas_InitSlow 声明仍独立于矿洞重建。没有已完成失败被重启或称为中断。矛盾返回研究所有者，不在 Layer B 编辑研究。

直接完整正文/差异、链接/锚点/导航、范围/私有边界与 `git diff --check`、配置后的干净已提交 planner 及实际精确 head CI 属于原切片。正文变化不要求全套运行时或翻译套件。Issue 交接记录候选身份与检查，供独立验收。
