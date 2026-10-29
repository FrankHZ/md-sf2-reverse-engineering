# Oddler 留下与 Fairy 交接

> 本文是[规范英文原文](../../../../synthesis/chapters/southern-travel-creed/oddler-stays-and-fairy-handoff.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。文末审阅、环境与验证记录保留原英文综合切片的时间范围。

Oddler 说他想留在 Creed 身边，却解释不出原因。Creed 建议他休息并道别。场景布置这次告别，隐藏 Oddler 当前行动者，请求 Peter 和 Fairy 随队跟随。仅场景返回后，调用方才改变之后重新入场时用于选择 Oddler 与 Fairy 的标志。

**推断：**这表达 Oddler 留在 Creed 身边、Fairy 随队同行。**已确认：**获准源码序列把当前行动者操作、有序调用方标志及后来人口重建分开。**未知：**自然到达区域（事件）、实际传入状态、呈现/跟随完成、可用控制及存档持久性。

本指南接续 [Creed 返回与招募选择](creed-return-fairy-and-recruitment-choices.md)，消费 main `936748b0715fdc992b05833745b566953dc77de3` 上[已接受的 #957 研究](../../../../../research/chapters/southern-travel-creed/oddler-stays-and-fairy-handoff.md)及已接受前篇。它止于紧接的条件区域（事件）返回及相关重新入场后果。步行离开宅邸、大地图路线和矮人治疗各有独立边界；Creed 建议向北不等于执行该旅程。此 Layer B 指南不选择重制规则或新观察工作。

继续阅读 [Creed 离开、矮人恢复与 Cannon 赠礼](creed-departure-dwarf-recovery-and-cannon-gift.md)，了解选定矿洞路线、Fairy 恢复场景、依赖容量的赠礼及挖掘，直至各自调用方和重新入场边界。

## 何时准入告别

**已确认：**房间区域（事件）表把 `(15,21)` 关联到 Map33_ZoneEvent0。匹配回调先检查 F774。未设置时选择[更早的 Goliath 场景](creed-mansion-entry-and-first-goliath-scene.md)，仅在其返回后设置 F774，然后返回，不继续落入 Oddler 告别。F785/F786 不能覆盖这个优先级。

F774 已设置时，还必须 F785 已设置、F786 未设置才调用 `cs_5B466`。F785 来自更早的 Fairy 交互，即使其中受 Bowie-HP 守卫的跟随被跳过也会写入。告别门槛不检查 F787 或可招募者已加入标志：选择一名可招募者不是局部前提。它也没有 F784、F783、F762、F67/F68、HP、容量或成功跟随前提。[请求与门槛所有者](../../../../../research/chapters/southern-travel-creed/oddler-stays-and-fairy-handoff.md#request-gate-and-actual-actors)保留全部三个表行、六条配置路由及精确分支/PC 关联。

区域（事件）请求仍依赖工作布局类型、移动/碰撞和坐标产生。分发在选择回调前清除待处理事件；包装器重新选择当前配置并比较坐标低字节。传入一个 `(15,21)` 回调不是普通步行到达的证据。F785 未设置或 F786 已设置时，跳过告别及后续三次写入，但被调用回调仍会返回。默认回调仅返回；另一个特定行 `(12,14)` 有独立的 F783 守卫场景。

## 房间内已有的行动者

**已确认：**通常 F784 人口且 F786/F22 未设置时，居民为 Goliath128、Creed129、Fairy130 及四名可招募者。声明将 Goliath 放在 `(17,14)` 朝 Up，Creed 在 `(17,5)` 朝 Down，Fairy130（FAIRY219）在 `(17,10)` 朝 Up，带 `eas_InitSlow`。普通 F784 init 不另布置这三者。更早当前跟随与后续场景命令可改变 Fairy 状态；声明不是告别准入时她的稳定位置。存活、未加入的可招募者可布置在房间四周。Peter 是 ally7，Oddler 是普通 FOLLOWER_B31，与这些居民分开。[前篇人口指南](creed-return-fairy-and-recruitment-choices.md#返回房间)保留选择、init、成员资格和 HP 前提。

仅有 F67/F68 普通随从时，Bowie 为 physical0，Peter/Oddler 为1/2，Goliath/Creed/Fairy 为3/4/5，可招募者为6–9。这是条件分配，不是固定行动者槽。F69 独立选择 FOLLOWER_C158/Fairy、查找偏移62，与 Fairy130 的偏移34 不同。其他随从行可重复 B31，覆盖其查找却留下更早的物理行动者。缺失普通查找可别名到 physical0；隐藏使其失效为 -1。场景不修复缺失、隐藏或别名映射。

实际 HP、职业、模式、列表顺序和地图精灵均有关。Bowie/Peter 的布置和朝向可因 HP0 跳过，而 NPC129/130 和 B31 绕过这个具名己方角色守卫。Init、手势、内联行动及隐藏没有同样的 HP 跳过。因此，变化人口可准入脚本，却不提供普通角色阵容。以下行动者标签描述预期的不同映射，不断言每种传入状态都包含这些人物。

## 房间中的告别

**已确认：**[完整场景所有者](../../../../../research/chapters/southern-travel-creed/oddler-stays-and-fairy-handoff.md#complete-scene-and-embedded-actions)按以下顺序布置、交谈和行动。摄像机目的地是视野请求，不是玩家移动或显示摄像机位置的证明。

场景请求视野 `(12,15)`，暂停，将 Peter 和 Bowie 转 Up，让 Oddler 发抖。Peter 询问 Oddler 的情况；Creed 说他明白了。Oddler 和 Fairy 朝 Up，Creed 被放到 `(17,15)` 朝 Down，并请求第二个视野 `(12,7)`。Peter、Oddler 和 Fairy 各收到等待式 Init 行动。它替换此前行动，包括 Fairy 更早的跟随和较慢 Init 属性；不重置位置，也不移除已有探索列表条目。

随后将 Bowie 放到 `(15,23)`、Peter 到 `(15,22)`、Oddler 到 `(16,22)`、Fairy 到 `(16,23)`，全部朝 Up，受具名己方角色 HP 守卫约束。按 Eric、Randolf、Tyrin、Karna 顺序隐藏，不论选择、HP、已加入/出战状态或此前缩小情况。摄像机以 Creed 为目标，他走 Down2、Left2、Down4，有条件到达 `(15,21)`。

视野 `(10,18)` 之后，Peter 问 Creed 明白了什么。Creed 说 Oddler 会留下，走 Right1 并朝 Down，然后请 Oddler 同意。Peter 朝 Right，Oddler 朝 Left。Peter 问是否属实；Oddler 说他想留下却不知道原因，随后坚持必须留下。这些问题没有 Yes/No 菜单或选择变更。

Creed 走 Left1 并朝 Down；Peter 和 Oddler 朝 Up。Creed 说 Oddler 虚弱，建议留下并道别。Oddler 走 Up1、朝 Left，Creed 朝 Right，暂停后点头。Creed 随后走 Left1 并朝 Down；Oddler 走 Left1 并朝 Down。Oddler 感谢首领一路同行，感谢大家并请他们小心。他点头、暂停、走 Up1，再跨几次暂停转 Down 和 Up。等待式 Up3 后隐藏当前 B31 行动者。

Creed 走 Right1 并朝 Down。他告诉首领以后会明白原因，请他们别担心并指向北方。最终命令先请求 Peter→Bowie，再请求当前 Fairy130→Peter，然后结束脚本。此场景没有重载、当前图块复制、传送、军团加入/离开、治疗、剧情标志命令、显式音乐改变或 Egress/玩家模式写入。先前招募不被撤销，也不选择替代可招募者。

在不同映射且布置/移动完成的前提下，Creed 路径为 `(17,15)→(17,17)→(15,17)→(15,21)→(16,21)→(15,21)→(14,21)→(15,21)`。Oddler 在隐藏前的路径为 `(16,22)→(16,21)→(15,21)→(15,20)→(15,17)`。五个 `faceDown1` 操作数改变朝向、更新精灵并 wait1，不增加 Down 移动。这些坐标推导要求相对行动完成，不证明观察到的无碰撞运动。

[对话与行动消费者所有者](../../../../../research/chapters/southern-travel-creed/oddler-stays-and-fairy-handoff.md#dialogue-action-consumers-and-limits)保留全部十四个文本位置1932–1945、所归属说话者、命令形式和控制。Oddler 四条台词使用编码 `$801F`；实际映射精灵决定立绘/提示音，已打开的立绘不会替换。LEADER 重建军团，读取首名出战成员名字，不一定是 Bowie。这不改变独立 Bowie HP 守卫。虚弱、意愿和 Creed 的解释是台词，不是已实现治疗，也不证明 Oddler 状况的成因。

N 为换行；W1/W2 等待输入，W2 带继续箭头/声音行为。单条文本关闭窗口并 sleeps10；继续文本不如此。解释器入口清除跳过状态后，Debug/P2 Start 可锁存文本跳过，省略显示和游标推进，而其他命令仍执行。发抖、点头、摄像机滚动、精灵更新和源码睡眠是服务请求，不是实测呈现或经过时间的证据。内联行动等待只轮询选定行动者的 Idle 指针，没有超时或全局屏障。异步实体服务和指针替换在全文中仍有关。

## 隐藏与新的当前跟随

**已确认：**[HideEntity 与跟随消费者](../../../../../research/chapters/southern-travel-creed/oddler-stays-and-fairy-handoff.md#five-hides-two-current-follows-and-ordered-return)区分可见布置、查找/列表状态与军团成员资格。每次隐藏把当前/目的地坐标设为 `$7000`，使所有匹配查找失效，若物理行动者在探索列表中则移除。它保留记录、精灵/标志及旧行动指针；普通实体服务在当前 X 至少为 `$7000` 时跳过，使其休眠。

因此，四名可招募者隐藏移除当前跟随/列表参与，不移除招募或出战资格。它们不等待先前缩小。更早缩小尾部停在 `(63,63)`，低于该服务阈值，并保留查找/列表条目。隐藏和缩小后果不同。别名可使一次隐藏令多个逻辑行动者失效，并重定向后续操作；五次隐藏不保证影响五个不同物理行动者，也不证明渲染消失。

最终两个跟随字的高字节均为0：Peter 的 `$0007` 和 Fairy130 的 `$0082`。处理器在解码行动者字前读取该编码高字节，所以两种关系均检查 Bowie HP。Bowie HP0 跳过各关系的六个参数字节。若脚本返回，它不会撤销更早 Init、隐藏或调用方写入。尤其是，跳过最终 Fairy 跟随**不会**保留更早 Fairy→Oddler 行动：Init 已经替换它。存活的 Bowie 和死亡的 Peter 可准入两次跟随，尽管 Peter 自身布置/朝向已跳过。

获准时，各关系解析当前映射，分配/复制 42-byte 随从行动，安装偏移 `(-24,0)`。已列入的行动者仍收到替换行动；只对列表插入去重。这是 Peter→Bowie 和 Fairy130→Peter 请求，不是瞬移或等待到位。它们不创建 Fairy158、不授予军团成员资格，也不设置 F69。缺失/自我/别名目标没有局部验证。即使之后调用方标志改变，当前 Fairy 仍是 resident130。

## 脚本返回与有序标志

**已确认：**脚本结束在对话窗口索引非零时有条件等待视野滚动，清除滚动速度，并经脚本 trap 返回。寄存器恢复不恢复已变更全局状态。它既不轮询全部行动者，也不恢复 Bowie 摄像机目标。仅在该返回后，zone0 才按顺序设置 F786、设置 F69、清除 F68 并返回。

随后区域（事件）包装器关闭立绘/文本，等待一个 VInt，再等待 physical0 当前/目的地 X/Y 相等。这是坐标等待，不是 Idle 指针、随从或全部服务汇合。默认与跳过回调也到达此等待。若没有有效配置，则绕过回调/窗口关闭，但 physical0 等待仍在。回调调用、场景准入与可用控制器返回是不同声明。

该序列非原子。脚本返回前，隐藏/行动/位置可已变化，而调用方标志仍保留旧值。set786 后，69/68 可能仍未变；set69 后，68 可能仍已设置。后续回调若786 已设置，会跳过，而非补完中断前缀。完整返回会再次设置已设置的69 或再次清除已清除的68。直接调用场景省略门槛和调用方写入；直接重复或清除786 可遇到隐藏或变化行动者。不能由此推出安全回放、回滚或存档持久性。

## 后来重新入场选择什么

**已确认：**[重新入场与变化前提证据](../../../../../research/chapters/southern-travel-creed/oddler-stays-and-fairy-handoff.md#current-population-fresh-entry-and-changed-premises)独立于这些当前操作。告别返回不重载房间。Goliath128 仍是当前行动者，Creed129 仍是场景布置后的行动者，Fairy130 仍是当前 Fairy；B31 与可招募者隐藏/休眠。跟随/服务运行后，最后一次显式 Bowie/Peter/Fairy 布置不保证是稳定位置。此时尚未构建居民 Oddler130。

显式重新 LoadMap 重建随从/居民并运行选定 init。CURRENT255 复用保留当前行动者/布局。重新入场清除临时256–383 并设置80，保留这里讨论的非临时标志；这是加载行为，不是已验证存档存储。访问器重新选择当前标志，因此它们之间的变化可产生混合前提。

| 后来传入的状态 | 重新选择及其限制 |
| --- | --- |
| F786 已设置，F22 未设置 | F786 居民为 Goliath128 `(22,4)` Up，带行走原点/范围1；Creed129 `(17,5)` Down Init；Oddler130 `(17,7)` Up Init。没有可招募居民；替代 init 仅返回，尽管其标签含 flag312。 |
| 调用方完整返回，F22 未设置，普通 F67/69 已设置且68 未设置 | 随从是 Peter 和独立构建的 Fairy158。没有额外随从时，它们占 physical1/2，之后居民128/129/130 占3/4/5。隐藏旧 B31 不是新居民 Oddler130；旧 Fairy130 不是新 Fairy158。其他产生 B 的标志仍有关。 |
| F786 已设置，F22 未设置，69 未设置或68 仍已设置 | 居民 Oddler130 仍被选中，而独立随从标志省略 Fairy158 和/或包含 B31/Oddler。可能有两个 Oddler 精灵。区域（事件）跳过不协调这些标志。 |
| F22 已设置，即使786 已设置 | 最后设置标志胜出的顺序为默认、523、783、784、786、22。F22 选择六名基线居民：Goliath、Creed 和四名可招募者，初始停放在63,63；没有 resident130。F69 独立选择 Fairy158。普通 init 将 Goliath 放在 `(14,14)` Up。 |
| F22 与784 均已设置 | 普通 init 的独立784 尾部仍将存活、未加入的 Eric/Randolf/Tyrin/Karna 放在 `(15,14)/(16,15)/(18,15)/(19,14)` Down。已加入者通常保持停放；787 不控制布置。在传入标志下，更早的523-set/783-clear 恢复分支仍可能运行。 |
| 重新加载的 F22 布局 | 其标志复制表把 `(28,0)`、大小25×32 复制到 `(0,0)`。没有按786/69/68 重放告别或更早 Fairy 单格复制。不推断碰撞可达性。 |
| 保留人口，新改变786/22 | 已有行动者不会变身、移动或重建。Fairy130 此时可收到 Oddler 的1948–1949 回调话语；仅标志不替换她、不取消可招募者隐藏，也不创建158。F22 表选择不提供缺失130。 |

场景/调用方不改变 F67,762,774,783–785,787 及可招募者已加入/出战标志。未招募或没有 F787 不拒绝准入，缺少 F784 可提供不同角色阵容。这些状态变化路径约束解释，不证明自然战役时间线。

**未知：**自然区域（事件）可达及精确布局/请求；传入标志、HP、职业、模式、名册/列表、人口与别名；行动、视野、立绘、文本、音频和碰撞完成；可用控制及存档/读档持久性。后续离开、向北旅行和矮人恢复仍在本指南之外。这些问题不创建采集或模拟器队列。

## 证据审阅与保留结果

对抗式 Layer A 审阅检查已接受所有者及选定公开载荷/schema/消费者表面。未发现阻塞矛盾；综合解释采用上述更严格源码与服务边界。

| 审阅表面 | 处置 |
| --- | --- |
| [#957 研究](../../../../../research/chapters/southern-travel-creed/oddler-stays-and-fairy-handoff.md)与 [#953 前篇](../../../../../research/chapters/southern-travel-creed/creed-return-fairy-and-recruitment-choices.md) | 完整告别、行动者/行动、文本、Init 替换、编码高字节守卫、有序返回及重新构建/复用选择仍分开。 |
| `sf2-map-events-static-v1`：[routing-setup](../../../../../../tests/fixtures/h2/map-events/routing-setup.json)、[zone-programs](../../../../../../tests/fixtures/h2/map-events/zone-programs.json) | 选定载荷/schema 检查在三个行、六条路由、全部十四个 zone0 操作、相邻 zone1 和默认上一致；不声称自然可达。 |
| `sf2-map-init-static-v1`：[fixture](../../../../../../tests/fixtures/h2/map-init-static-v1.json) | Schema 及完整选定普通/替代体在独立784 尾部、F22 分支和替代返回上一致。 |
| `sf2-map-script-engine-static-v1`：[fixture](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json) | Schema 与选定 Init/内联行动/hide/follow 守卫一致；处理器事实不提供私有完整场景语料。 |
| [事件加载器](../../../../../../src/sf2tool/h2/map_events_fixture.py)、[事件验证器](../../../../../../src/sf2tool/h2/map_events.py)、[脚本验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)、[脚本 schema](../../../../../../schemas/h2/map-script-engine-fixture.schema.json) | 分片身份/形状/顺序与溯源/预期相等是独立检查；仅形状合法不证明原版行为。 |
| [出战队伍/名册](../../../contracts/party/party-roster-state.md#处理器边界)与[对话](../../../contracts/interface/dialogue-system.md#已确认的静态合同) | 跟随、成员资格、名字/游标和输入服务保留有界范围，不证明自然可达或持久性。 |

[复现所有者](../../../../../research/chapters/southern-travel-creed/oddler-stays-and-fairy-handoff.md#reproduction-and-review-boundary)固定 SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` 及 USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。它拥有完整67 记录、九个内联体及十四个文本关联。原英文综合切片的检出缺少固定 upstream 和保留对：不获取、不重新生成，也不声称新源码/输出比较。所有者命令仍为 `uv run sf2 h2 map-init`、`uv run sf2 h2 map-events` 和 `uv run sf2 h2 map-script-engine`；原英文切片不为文字修改而运行这些命令。

[#957 冻结交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/957#issuecomment-6178978855)及[独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/959#issuecomment-6179038849)保留 head5f45f3f8/treee851dcc4/base17718cd3、未变18,285,979-byte 对0.085s、CI38640917634 和完整关联。Init/F22/编码结束修正，以及已修正源码路径/截断输出诊断，仍作为检查证据保留。

[#958 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/958#issuecomment-6178996339)、[变基回执](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/958#issuecomment-6179069477)和[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/960#issuecomment-6179080841)保留未变五文件指南、518 个链接及 CI38641099043/38641713008。[前篇保留所有者](creed-return-fairy-and-recruitment-choices.md#证据审阅与保留结果)保留 #953 原始/变基后0.081s 比较、#954 回调修正，以及所链接 #949/#945/#941/#937 结果、已完成失败和未知项。没有已完成失败被改称中断或重启。矛盾交回研究所有者，不由 Layer B 修改研究。直接文字、链接/锚、范围/私有边界和 diff 检查，以及已配置的提交后 planner 和实际精确 head CI，属于原英文切片；不引入整套运行时或翻译验证。
