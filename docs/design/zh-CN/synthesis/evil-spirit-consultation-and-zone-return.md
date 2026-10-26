# Evil Spirit 咨询与条件区域（事件）返回

> 本文是[规范英文原文](../../synthesis/evil-spirit-consultation-and-zone-return.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。文末审阅、环境与验证记录保留原英文综合切片的时间范围。

Creed 召唤 Evil Spirit，询问 Zeon。首次接触受阻，Peter 因而提醒他注意首领的宝石。检查后，Creed 要求向灵体展示。灵体报告再次出现的威胁；自称 Zeon 的声音打断，在 Arc Valley 索取 Jewel of Evil，并提出用 Elis 交换。随后布置描绘灵体爆炸。Creed 与 Fairy 讨论失去灵体，并离开房间。

**推断：**这是有序布置的叙事含义。**已确认：**这些是归属于说话者的对话声明与源码命令，不是已实现的宝石移除、交换、Elis 返回或招募。程序不含物品栏事务。**未知：**自然可达、实际传入状态、服务完成、渲染表现、可用控制和持久性。

本指南从[神殿入口与场景准入](evil-spirit-shrine-entry-and-scene-admission.md)继续，消费已接受 main `9de649eb1da588065f513b5d0f32819636bb8933` 上 #949/PR952 的[完整咨询研究](../../../research/evil-spirit-consultation-and-zone-return.md)。它解释完整场景，止于有条件的调用方/包装器返回。后续步行返回、宅邸重新入场、Fairy 事件、招募及 Oddler 离开仍为独立边界。此 Layer B 解释不选择重制行为或新运行时工作。

继续阅读 [Creed 返回、Fairy 跟随与招募选择（英文）](../../synthesis/creed-return-fairy-and-recruitment-choices.md)，了解独立宅邸返回与紧接的房间交互。

## 谁在场，以及什么准入场景

**已确认：**独立产生的 type6 请求匹配 Map34 通配 X 与 Y 低字节13 时，选择 Map34_ZoneEvent0。消费者清待处理类型，并在回调检查 F784 前为 physical0 安装 Init。未设置则准入 cs_5B6C0；已设置则跳过场景与两个调用方写入。F783、F523 和 F802 均非额外谓词，进入神殿本身不触发咨询。[入口所有者](../../../research/evil-spirit-shrine-entry-and-scene-admission.md#actual-zone-request-and-scene-call-boundary)保留布局、候选/目的地与请求产生前提。

新 F784 未设置入场声明21 名居民。其逻辑 ID 经实际查找状态解析；逻辑、物理与存储 ENTNUM 值不同：

| 场景角色 | 基线身份与分配 |
| --- | --- |
| Creed128 与 Fairy129 | CREED212 和 FAIRY219；physical N+1、N+2。 |
| Evil Spirit130 | EVIL_SPIRIT252；特殊 physical47，存储 ENTNUM32。 |
| 对话131 | BEASTMAN2=222；普通 physical N+3。提供 Zeon 声音，不是新创建的 Zeon 行动者。 |
| 未使用132 | BOY200；普通 physical N+4，没有选定场景语音或移动。 |
| 效果133–148 | 十六个独立 EFFECT1=63 居民，physical N+5 至 N+20。 |
| Bowie0、Peter7 与 FOLLOWER_B31 | 独立主角/随从查找；仅在基线随从前提下 B 才是 Oddler。 |

N 计居民之前的普通随从。仅 F67/F68 时 N=2：Peter/B 为 physical1/2，Creed/Fairy3/4，131/132 为 physical5/6，效果7–22。灵体仍为 physical47。不同随从标志可使 B 解析为 Astral；重复 B 分配覆盖该查找。缺席行可经零映射解析为 physical0。隐藏将匹配查找改为-1，无局部修复。新 F784 已设置入场不创建居民；在保留实体群上改变标志不重建它。[已接受行动者前提](../../../research/evil-spirit-consultation-and-zone-return.md#actor-and-admission-premises)负责 HP/职业/模式/木筏及分配限制。

以下名义故事假设不同的基线映射及返回的服务。场景没有命令创建这些演员、改变其居民地图精灵 ID 或操作132。特殊图形选择与对话行动者131 仍独立。

## 召唤与首次受阻接触

**已确认，场景行4–105 / 文本3207–3226：**Creed 呼唤灵体，Fairy 宣告其出现。程序反复将灵体置于(7,6)、Down，再停放(60,60)，各次之间睡眠改变。九组出现/停放后，灵体留在(7,6)。这是源码布置序列，不是观察到的实体化动画。

灵体获得 Transparent，请求 blinking ON 并跳至 Idle；不插值透明度或替换精灵。对 Bowie、Peter、B 分别安装并等待 Init，再将他们置于(7,12)、(6,12)、(8,12)，朝 Up。Creed 靠近，问 Zeon 在做什么。灵体声称可联系他。闪光与地图白色淡出淡入伴随其尝试；出战队伍前进，Fairy 解释通灵并要求安静。

灵体报告屏障遮蔽 Grans。Creed 要它再试，它请求与 Zeon 有关的物体。这些报告不独立证明屏障的世界状态。摄像机依次访问(2,4)、(2,2)、(2,4)；后面的摄像机/服务讨论解释请求确立什么。

## 检查并展示宝石

**已确认，行106–179 / 文本3227–3242：**Peter 提及首领的 Jewel of Evil。Creed 认出封印宝石并索要。Peter 说无法取下；Creed 先说能取下，再要求检查。Creed 短暂 Down/Up 行动、闪光与手势伴随检查。他看到两道光，Peter 说出 Jewel of Light，Creed 表示 Mithril 链阻止取下。他要求将两颗宝石展示给灵体。

Bowie 向灵体前进，Creed 异步让开。无归属文本3240 叙述首领展示宝石；Bowie 也闪光，灵体声称接触改善。交替共享调色板辅助请求伴随另一次集中尝试。

**推断：**这描绘检查与展示。**已确认：**完整程序不执行物品栏转移或宝石移除命令。Mithril 链解释是 Creed 对话。文本 `{LEADER}` 替换使用当前首个战斗队伍成员名字；点头与布置命令则寻址 Bowie。传入成员资格可使两种权威不同。

## 灵体报告什么

**已确认，行180–196 / 文本3243–3257：**灵体说 Arc Valley 已打开、Zeon 已复活，devils 正飞向 North Parmecia。Creed 怀疑涉及 Mitula 的事件。灵体说看不到 Zeon，将他定位于 Arc Valley，并说力量尚未完全恢复。Creed 问其力量；灵体开始解释。

这些是归属于说话者的报告，不是新验证的战役事实。Creed 跨3254–3255 的陈述保持一次续文：3254 仅以 `{N}` 结束；3255 以 `{N}{W2}` 继续。两者之间不要插入确认或关闭。[精确文本/控制所有者](../../../research/evil-spirit-consultation-and-zone-return.md#text-identity-speaker-and-continuation-state)保留全部73 关联，包括此仅 N 段。

## Zeon 打断与索求

**已确认，行197–256 / 文本3258–3270：**Creed 要灵体说完并收到断续回答时，特殊图形辅助程序交替选择 EvilSpiritAlt 与 EvilSpirit。较长屏幕闪光请求先于 Zeon 标签图形选择。两轮各自为 Bowie、Peter、B、Creed 安装 Jump，之后等待 Fairy Jump。每轮只等待 Fairy 行动，不等待其他四人。

经131 寻址的声音自称 Zeon，威胁呼叫者并认出 Jewel of Evil。Creed 说 Zeon 正透过灵体看，称不可能。声音认出首领，要求在 Arc Valley 交出宝石。Peter 反对；声音提出以 Elis 交换，并重复目的地。源码随后交替特殊图形与闪光，设置 quake 振幅5，并请求大门隆隆声。

**推断：**布置描绘 Zeon 接管联系。**已确认：**131 保留声明的 BEASTMAN2 地图精灵与普通分配。特殊精灵辅助程序独立更新共享图形；Zeon 标签不创建行动者、不将对话行动者改为新精灵，也不执行交换或 Elis 返回。这里无同意菜单。

## 十六个效果、五次爆发与灵体隐藏

**已确认，行257–540：**每个效果133–148 各自获得等待的自定义行动，设置速度64/64 后跳 Idle。不重置其 motion、碰撞、透明度或其他继承状态。源码将射线与两组循环行动者分开：

| 组 | 请求轨迹与完成边界 |
| --- | --- |
| 内侧141–144 | 置于(8,6)、(7,5)、(6,6)、(7,7)，Right；进入顺时针循环程序的四个阶段。 |
| 外侧145–148 | 置于(8,6)、(7,7)、(6,6)、(7,5)，Left；分别获 Right1/Down1/Left1/Up1，睡眠2 后 StopMoving；进入逆时针循环程序的四个阶段。 |
| 射线133–140 | 五次爆发各自开始前，全部重置为(7,6)、Right。分别获得 Right4、Up4、Left4、Down4、UpRight4、UpLeft4、DownLeft4、DownRight4。仅等待140。 |

循环程序由相对移动、目的地等待与回分支组成，无 Idle 出口。源码标签不可靠描述首操作数：`eas_ClockwiseMoveUpRight` 从(-1,+1) 开始。内侧移动若完成，经过菱形各阶段；外侧轨迹从实际停止坐标开始，不是假设的整瓦片偏移。[完整轨迹](../../../research/evil-spirit-consultation-and-zone-return.md#sixteen-effect-trajectories-and-hides)保留每个入口、增量和循环尾部。两组均不证明实测半径、同步环或渲染圆形。

每次爆发在射线前请求死亡 SFX。重置位置并替换未等待完成的射线行动可中断其先前移动。爆发3 后隐藏灵体，并再请求隆隆声；之后爆发4、5。爆发5 后，133–140 按序隐藏，再睡眠40，141–148 按序隐藏。这些效果不实现战斗员死亡、EXP 奖励或 `eas_Die` 事务。

Hide 将当前和目的地坐标写为($7000,$7000)，使全部匹配查找失效，并在存在时将物理行动者移出探索列表。保留行动指针、精灵与标志，不等 Idle，也不释放记录。关键是普通实体循环在实体/行动分发前跳过当前 X 至少 $7000 的记录。因此隐藏循环槽在该 X 保持时休眠；保留指针不等于继续轨道移动。后续重新激活和渲染缺席为**未知**。别名可使一次隐藏同时令多个逻辑 ID 失效。

## 后续与侍从离开

**已确认，行541–580 / 文本3271–3279：**quake $4005 请求下降序列4、3、2、1、0，每次写入后内部 Sleep40。最后调色板恢复辅助请求和睡眠60 先于讨论。Creed 说灵体爆炸，归因于 Zeon 力量，并警告完全复活。Fairy 质疑灵体可靠性；Creed 肯定它，疑惑 Zeon 为何要宝石，并说灵体再也无法回答。

Creed 获等待的 Down1、Left1、Down5 路径并隐藏。Fairy 获等待的 Left1、Down1 路径，呼唤他，带等待跳两次，叫他等候，再获 Down5 并隐藏。她最后台词前，Peter 和 B 被朝 Down；Bowie 更早的 Down 朝向有自身 HP 守卫。Creed Init 后移动可经自动朝向改变朝向，因此最后显式朝向不是无条件最终方向。

最后程序请求 Peter→Bowie 方向2，再 B→Peter 方向2。编码随从字为7 和31，但处理器先测试高字节，再解码完整字。两高字节均0，因此两调用均由 Bowie HP 守卫，而非 Peter 或 B。准入时方向2 选择偏移(-24,0)；AddFollower 安装修补的42 字节跟随模板，仅在缺席时将物理随从追加到探索列表。不招募、不设 F67/F68、不瞬移、不修复查找，也不等待跟随稳定。别名可产生自我跟随或替换首个关系。[条件结束所有者](../../../research/evil-spirit-consultation-and-zone-return.md#conditional-end-state-and-zone-caller-return)保留精确守卫和模板重置/跟随边界。

## 阅读文本、移动与表现请求

**已确认：**完整场景有409 条顶层记录、58 个移动主体、16 个自定义主体和73 个精确文本/控制关联。上述六个叙事阶段覆盖场景行4–580，不遗漏无对话爆炸区间。[完整有序所有者](../../../research/evil-spirit-consultation-and-zone-return.md#complete-ordered-scene)保留全部连续记录分区；本指南不重复逐字稿或操作码清单。

名义游标经过3207–3279，结束于3280。`nextSingleText` 显示、递增、关闭立绘/文本并睡眠10；`nextText` 保留续文打开。W1/W2 是输入等待，不是选择，命令形式不决定等待数量。Creed 用修饰符 $80，请求右侧不镜像立绘；其他行动者用0。基线 Creed 选择 CREED/bleep8，Fairy NONE/1，灵体 NONE/8，131 NONE/6。Peter 实际基础/转职后精灵选择立绘及 bleep4/5；改变精灵可不同。叙述3240 用 `$FFFF` 和语音 SFX0。已有打开立绘或 NONE 结果可抑制打开；选择131 不请求 Zeon 立绘。

ExecuteMapScript 入场清文本跳过锁存。调试模式加 P2 Start 可锁存，跳过后续文本显示/游标推进及分发器 csWait 睡眠。其他行动/辅助命令及内部等待保留。因此73 个名义关联不是73 次观察显示，调试跳过会改变并发移动可用时间。

行动安装替换解析行动者指针并初始化等待计时器。等待安装无超时轮询该行动者至 Idle；生成的移动等待比较当前/目的地坐标。不等待其他行动者。Bowie/Peter Up1 与 B 等待的 Up1 并发、Creed 在 Bowie 等待靠近期间让开、分组跳跃和全部五次射线重置都必须保留此区别。StopMoving 经 moveRel(0,0) 从当前坐标重新定目标，不完成被覆盖移动。`stopEntity`/`startEntity` 写动画计数器-1/0，是不同操作。Init 重置其具名移动默认值，不重置坐标或所有标志。己方布置和朝向可跳过 ID 小于30、HP0 者；行动安装及选定闪光/手势不共享此跳过。别名或服务延迟下，替换可改变名义编排。见[行动与完成](../../../research/evil-spirit-consultation-and-zone-return.md#actions-placement-and-completion)。

表现有独立直接效果与完成限制：

| 表面 | 已接受请求/效果及限制 |
| --- | --- |
| 摄像机 | 目的地(2,4)、(2,2)、(2,4)、(2,5)、(2,4)；跟随目标成为-1，每条命令等待滚动。无命令恢复跟随；摄像机等待不证明玩家控制交接。 |
| 特殊图形 | 辅助程序选择 EvilSpirit+$20、EvilSpiritAlt 或 Zeon+$20；解压到加载缓冲，并排队 $480 字（$900 字节）至 VRAM $AF00..$B800。EnableDmaQueueProcessing 设置 DMA_REQUEST，不等待 VInt 消费。最后选择 EvilSpiritAlt；无最终图形恢复。 |
| 共享调色板辅助程序 | P1 用已接受查找/舍入规则将64 个 CURRENT 颜色重映射至 BASE，再将 CURRENT 备份至 COPY。P0 先复制既有 COPY 至 BASE，再用当时 CURRENT 替换 COPY。两者都清自动返回字节并启动32 次更新转换。 |
| 调色板插值 | 计时器派生权重和为8；更新写 CURRENT 并请求 CRAM DMA。中间快照、辅助调用和闪光重置可互相作用。最终 P0 目标为其选定 COPY 快照，不保证原房间调色板；睡眠60 不是完成轮询。 |
| 白色淡入淡出/屏幕闪光 | 接触前：三次 flash4 和白色地图淡出/淡入；打断时：flash40；后续：四次 flash4。csc41 移位计数器对4 请求一次启动，对40 请求六次。LaunchFading 条件等待其设置清除，再等待一个 VInt；操作数不是帧数。 |
| 实体闪光/手势 | Creed100 请求26 次闪光循环；每个 Bowie40 请求11 次。精灵标志/更新等待、nod/headshake/shiver 变换和共享大小恢复有自身效果，不完成全部移动。 |
| 震动/音频 | 振幅5 启动爆炸；完成的 $4005 经五个下降写入留下0。内部睡眠不受调试文本跳过影响。声音请求既不证明播放，也不证明视听完成。 |

[选定辅助程序/服务所有者](../../../research/evil-spirit-consultation-and-zone-return.md#special-graphics-palettes-and-presentation-services)保留精确序列、调色板算术、闪光计数器和消费者。名称、睡眠与服务返回不确立实测时序或渲染结果。

## 条件调用方与包装器返回

**已确认：**csc_end 在对话窗口打开时条件等待视野滚动，清 VIEW_SCROLLING_SPEED、恢复保存寄存器并返回。不轮询每个行动者或调色板/DMA 队列、不恢复摄像机跟随、不重置隐藏槽，也不规范新写待处理事件。寄存器恢复也不回滚全局场景状态。

只有返回的场景到达调用方 `setFlg 784`，再 `setFlg 802`。回调返回后，包装器关闭立绘/文本、等待一个 VInt，再等待 physical0 当前 X/Y 等于目的地，之后恢复寄存器并返回。该尾部同样适用于 F784 已设置或默认回调路径。它是一个物理行动者的坐标等待，不是 Idle、随从或服务屏障。

顺序非原子。场景中断可保留部分位置、行动、图形和隐藏而无任何新调用方标志；两个写入间中断可留下 F784 已设置/F802 未变。两个标志都可能已设置，而 physical0 包装器等待仍未完成。均不证明可用控制、表现完成或存档/读档持久性。

以下**已确认静态判别示例**约束改变的传入状态：

- 新 F784 未设置入场重建基线居民；仍需产生符合条件的请求。新 F784 已设置入场不创建居民；回调跳过场景调用及两个调用方标志写入。
- 在保留基线上设置 F784 改变准入但不移除居民。在空/隐藏实体群上清它准入调用却不重建行动者。重复不是重置；显式新34 与 CURRENT255 复用不同。
- 仅改变 F783/F802 不改变局部门槛。F802 已设置不阻止咨询。其他传入随从/HP/职业/模式/木筏值可独立改变行动者、别名、精灵和获准命令。
- 传入 Y13 请求不确立自然可达。不同 Y 低字节选择默认；实际请求写入者与布局仍为独立前提。
- 不返回的行动/文本/摄像机/淡入淡出等待阻止后续源码操作。调试跳过改变游标/调度，不跳过全部内部等待。
- 场景/包装器期间写入的新请求未在此显式清除；回调标志和 physical0 稳定不证明下一可用事件状态。

**未知：**自然路线与调用方状态、实际实体群/HP/映射、服务完成、精确表现、控制和持久性。这些缺口不自动授权运行时获取。后续宅邸/Fairy/招募及 Oddler 事件需要独立已接受证据和综合范围。

## 证据审阅与保留结果

原英文切片的对抗审阅覆盖完整已接受 #949 研究、入口/行动前篇边界和对话/图形/特殊精灵合同。检查完整叙事顺序、文本3254 续文、实际行动者身份、受守卫命令、未等待/被替换移动、休眠隐藏、辅助快照/DMA 边界及非原子调用方/包装器尾部。选定已接受关联未发现矛盾；广泛运行时/表现问题仍为未知，不是名义场景已完成的证据。

| 直接检查的公共所有者 | 审阅边界 |
| --- | --- |
| sf2-map-events-static-v1:routing-setup，[routing-setup.json](../../../../tests/fixtures/h2/map-events/routing-setup.json) | 精确 ms_map34_ZoneEvents 通配255/Y13 和默认记录、目标与源码行。 |
| sf2-map-events-static-v1:zone-programs，[zone-programs.json](../../../../tests/fixtures/h2/map-events/zone-programs.json) | 全部六条 Map34_ZoneEvent0 操作、地址/操作数/行顺序、清/设极性、cs_5B6C0、先 F784 后 F802 及默认 rts。 |
| sf2-map-script-engine-static-v1，[map-script-engine-static-v1.json](../../../../tests/fixtures/h2/map-script-engine-static-v1.json) | cs_5B6C0 的409 命令汇总；对话、行动桥、布置、生命周期、手势/跟随、摄像机、屏幕和实体 FX 处理器事实/场景引用。不含完整 programCorpus。 |
| [地图脚本 schema](../../../../schemas/h2/map-script-engine-fixture.schema.json)和[验证器](../../../../src/sf2tool/h2/map_script_engine.py) | 组合对话/实体/表现/脚本边界及溯源/语义事实比较；源码引用和汇总事实不替代完整命令主体。 |
| [事件分片消费者](../../../../src/sf2tool/h2/map_events_fixture.py)和[验证器](../../../../src/sf2tool/h2/map_events.py) | 分片/段身份、target-program schema、完整预期输出/溯源相等，而非仅合法形状。 |

原英文切片直接对两个事件分片/预期段和地图脚本 fixture 做 schema 验证。按更严格研究边界检查精确回调关联和选定处理器事实。[对话合同](../contracts/dialogue-system.md)、[图形服务合同](../contracts/graphics-service-state.md)与[特殊精灵数据合同](../contracts/special-sprite-graphics-data.md)保留自身受控/汇总范围；均不独立证明此咨询的自然文本、渲染图形或传输完成。

研究固定 [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm) 及 USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。[复现所有者](../../../research/evil-spirit-consultation-and-zone-return.md#changed-premises-evidence-limits-and-reproduction)具名保留比较配方、按 id=cs_5B6C0 的完整程序选择、源码/文本关联和直接消费者。原英文切片检出缺该 upstream 与保留对；不声称新比较、提取、获取或运行时结果。窄所有者仍为 `uv run sf2 h2 map-init`、`uv run sf2 h2 map-events` 和 `uv run sf2 h2 map-script-engine`，配合既有保留配方。

[#949 冻结交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/949#issuecomment-6178414274)与 [PR952 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/952#issuecomment-6178451561)保留 head3917f0e9/tree46e77101/base4a87e98e、未改变18,285,979 字节对比较0.079s、完整409/58/16/73 源码/主体/文本关联及 CI38636725332 scope14s/research-public36s PASS。其修正休眠槽、文本与编码跟随事实，以及全部检查/解析诊断继续保留。这些是已接受结果，不是新重跑。

链接的 [#945 原始记录](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/945#issuecomment-6178009849)、[变基交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/945#issuecomment-6178108485)与[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/948#issuecomment-6178118055)保留0.084s 对比较、原始/变基 CI 和换行/索引诊断。[#941 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/941#issuecomment-6177767348)与[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/944#issuecomment-6177797802)保留0.080s 结果、修正的1892 控制与高字节/Bowie HP 推理。其 [#937 原始记录](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/937#issuecomment-6177275252)、[审阅](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/939#issuecomment-6177302395)、[修正交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/937#issuecomment-6177356550)及[验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/939#issuecomment-6177369382)保留已完成失败与未知项。不重启已完成失败套件。
