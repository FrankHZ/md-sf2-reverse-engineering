# Creed 返回、Fairy 跟随与招募选择

> 本文是[规范英文原文](../../../../synthesis/chapters/southern-travel-creed/creed-return-fairy-and-recruitment-choices.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。文末审阅、环境与验证记录保留原英文综合切片的时间范围。

咨询之后，神殿有出口通回 Creed Mansion。在通常传入的咨询后房间状态下，Goliath 说主人想提供帮助，请出战队伍挑选一名同伴。Creed 建议向 Tristan 的说书人打听。Fairy 提出同行，Eric、Randolf、Tyrin 和 Karna 也各有招募对话。

**推断：**这些对话构成房间内的帮助与同伴选择。**已确认：**下述路线、对话分支和有序操作是已接受静态证据。它们不证明自然步行返回、已完成跟随、可见缩小、可用输入或已保存的选择。**未知：**实际传入状态及这些运行时结果。

本指南接续[咨询与条件区域（事件）返回](evil-spirit-consultation-and-zone-return.md)，消费已接受 main `17718cd3ece7dc087737f3a6bea4eb463fd4553f` 上[已接受的 #953 研究](../../../../../research/chapters/southern-travel-creed/creed-return-fairy-and-recruitment-choices.md)。它止于紧接的房间交互返回。稍后的 Oddler 离开场景在 [Oddler 留下与 Fairy 交接](oddler-stays-and-fairy-handoff.md)中继续，解释当时的行动者、有序调用方标志及后来重新入场的人口。此 Layer B 解释不选择重制规则或新观察工作。

## 返回房间

**已确认：**Map34 在 `(26,28)` 的出口请求显式、无滚动地进入 Map33 的 `(9,5)`，朝 Right。该表不检查咨询标志。另外两个传送使用 CURRENT255 在神殿内转移；它们保留人口，不重新进入宅邸。[路线所有者](../../../../../research/chapters/southern-travel-creed/evil-spirit-shrine-entry-and-scene-admission.md#return-only-local-surfaces-and-the-three-warps)保留工作布局传送图块、获准移动、请求消费与继承坐标高字节等前提。咨询后的 F784 与返回相容，但不证明玩家走过该出口。

显式宅邸入场重建行动者，加载选定布局和地图精灵。它清除临时标志 256–383 并设置 F80，保留房间的 F22/F67–69/F759/F762/F774/F783–787 及已加入/出战标志。CURRENT255 复用则保留当前行动者和布局。每个配置访问器都会重读当前标志，因此若调用之间标志变化，人口、init 与交互可能采用不同前提。

[人口与 init 所有者](../../../../../research/chapters/southern-travel-creed/creed-return-fairy-and-recruitment-choices.md#return-population-and-init)采用最后设置标志胜出的选择顺序：默认、F523、F783、F784、F786，然后 F22。F783 再次选择基线；F22 甚至覆盖 F786。所有选择共用房间交互表，但居民和 init 并不相同。

| 传入的房间状态 | 重新构建的人口与选定 init |
| --- | --- |
| F784 已设置，F786/F22 未设置 | Goliath128 在 `(17,14)` 朝 Up，Creed129 在 `(17,5)` 朝 Down，Fairy130 在 `(17,10)` 朝 Up，随后是四名初始停放的可招募者。普通 init 有下述布置尾部。 |
| F786 已设置，F22 未设置 | 行走的 worker128 在 `(22,4)` 朝 Up，Creed129 在 `(17,5)` 朝 Down，Oddler130 在 `(17,7)` 朝 Up。没有可招募者行；替代 init 仅返回。其提到312 的源码标签并不选择 F312。 |
| F22 已设置 | 六个基线行，初始均在 `(63,63)`：worker128、Creed129 及四名可招募者，没有 resident130。普通 init 将128 放到 `(14,14)` 朝 Up，并独立检查 F784 以布置可招募者。 |
| F784 未设置，没有更晚覆盖 | 默认/F783 给出基线；仅 F523 时给出恢复场景人口。独立的 F523-set/F783-clear 恢复门槛仍适用。 |

普通 init 先以 F523 已设置/F783 未设置检查恢复场景；仅该场景返回后才设置 F783 和 EGRESS_MAP=Hassan9。F783 已设置时跳过场景及 Egress 写入。Goliath 的各布置分支之后，F784 独立准入四次已加入标志检查。F787 不是 init 门槛。

| 可招募者 | 检查的已加入标志 | 未设置时请求的位置 |
| --- | --- | --- |
| Eric14 | F14 | `(15,14)` Down |
| Randolf16 | F16 | `(16,15)` Down |
| Tyrin17 | F17 | `(18,15)` Down |
| Karna15 | F15 | `(19,14)` Down |

这些布置命令跳过零 HP 的己方角色。固定己方角色行的分配则跳过已有非零查找的行，不检查已加入或出战资格。因此，仅未加入标志不能证明可招募者可见、可达。实际职业/HP 决定己方角色精灵，HP 为 0 时可能是蓝色火焰；探索治疗覆盖存活/不死的己方角色，并非每名死亡的可招募者。

逻辑身份也不同于物理分配。仅有 F67/F68 普通随从时，Peter7 与 B31/Oddler 占 physical 1/2，随后七名居民占 3–9。其他随从选择器会改变这个例子。重复 B31 行可覆盖其查找，不作去重。缺失的普通映射可别名到 physical 0；隐藏后的映射是 -1，局部不作修复。F69 的 Fairy 是 FOLLOWER_C158，查找偏移 62，不同于 resident130。[恢复所有者](../../../../../research/chapters/southern-travel-creed/creed-restoration-and-basement-departure.md#admission-setup-selection-and-population)保留模式、木筏、过量人口及 HP/别名限制。仅 F784 既不证明恢复完成，也不证明咨询完成。

入场不自动等于可用返回。Init 没有区域（事件）式的 physical 0 坐标等待。调色板/音乐和 WaitForEvent 必须返回，且待处理地图事件优先于控制器安装。局部交互既不改变 EGRESS_MAP，也不改变 PLAYER_TYPE。

## Goliath、Creed 与变化后的房间

**已确认：**七个特定回调分别对应 Goliath128、Creed129、resident130 及四名可招募者。默认回调仅返回。[完整决策与说话者所有者](../../../../../research/chapters/southern-travel-creed/creed-return-fairy-and-recruitment-choices.md#complete-interaction-table-and-callbacks)保留精确 ID 与源码分支；本指南转述其内容。

Goliath 先检查 F22。在该分支中，他哭泣，声称 Oddler 恢复记忆后发狂，说 Creed 理解那男孩可怕的面容，并在询问 Creed 是否还活着时报告坠海之事。没有 F22 时，F786 选择有关恢复记忆和严重头痛的问题。否则，F784 选择主人愿意帮忙、请挑一名同伴的话；F783 选择他对 Zeon 苏醒的反应；后备分支则拒绝来访。

Creed 检查 F786，没有单独检查 F22。已设置时，他声称恶魔无法帮忙，人们必须封印 Zeon。未设置时，他怀疑宝石藏有秘密，建议询问 Tristan 的说书人，并指明去 Tristan 的方向。这些是归属于说话者的报告和建议，不是已实现旅行，也不证明后来灾难确实发生。

Resident130 首先检查 F786。已设置时，向首领问候并报告头痛，然后返回。重新构建的 F786 人口使该行动者成为 Oddler；在保留的 Fairy 身上改变标志，也能说出同一分支。重新构建的 F22 基线没有 resident130。因此，仅凭回调身份不足以把实际说话者标为 Fairy。

## Fairy 的决定与当前跟随

**已确认：**在 F786 分支之外，F759 选择 Fairy 前面的台词。未设置时，她宣布要随出战队伍向东走。已设置时，她询问生病的矮人，提出帮助并请大家带她去。两条路径都关闭文本并调用同一场景 `cs_5A828`。没有 Yes/No 菜单或军团加入。[Fairy 操作所有者](../../../../../research/chapters/southern-travel-creed/creed-return-fairy-and-recruitment-choices.md#fairy-live-block-copy-and-current-following)把叙述、布局、HP 与跟随分开。

场景先把当前布局中 `(17,13)` 的字复制到 `(9,5)`，请求视图平面更新。复制的是当前瓦片/碰撞位，不是固定瓦片或传送表变化。更早的变更，包括 F22 的独立图块复制，都可能改变源字。该命令既不证明实际图像，也不证明重绘完成。重新加载 Map33 的标志复制表没有通过 F762/F785 重放这一单格复制。

随后叙述首领决定带上 Fairy，再执行 `followEntity130,FOLLOWER_B,2`。编码随从字为 `$0082`，但 HP 守卫读取其高字节 0：检查的是 Bowie HP，不是 Fairy HP。Bowie HP 为 0 时，在复制和叙述后跳过跟随操作。普通场景返回仍可到达调用方 F762 再 F785。显示的 LEADER 名字可能属于首名出战成员而非 Bowie；这不改变 HP 守卫。

获准时，跟随解析 resident130 和 B31，并安装偏移 `(-24,0)` 的行动。只有该物理随从尚未在探索列表中时才追加，但重复调用仍分配/替换行动。行动使用目标的实际坐标、目的地和朝向；安装不会瞬间将 Fairy 放在 Oddler 身后。缺失的 B 可解析为 physical 0，别名可造成自我跟随，局部操作不创建缺失行动者，也不修复无效查找。没有随从到位等待。

只有场景返回后，回调才先设置 F762、再设置 F785。这些写入非原子，也没有重复守卫。它们不设置 F69、不清除 F68，也不授予军团成员资格。普通激活排除已列入的探索随从；直接回调可重复，而跳过跟随并未建立该排除条件。重新入场丢弃当前行动/列表分配并重新选择居民：F762/F785 不抑制 F784 Fairy 行，也不创建 FOLLOWER_C158。静态标志存续不是已验证存档或永久跟随。本次交互中，治疗提议同样不治疗矮人。

## 四名同伴，各有两种邀请

**已确认：**每名可招募者先检查 F22。已设置时选择稍后的邀请，即使 F787 已设置。否则，F787 未设置时选择初始邀请；F787 已设置时给出该行动者的初始拒绝文本，不提示选择。没有回调要求 F784、已完成恢复/咨询、尚未加入、存活 HP 或可用出战队伍容量。[八种邀请所有者](../../../../../research/chapters/southern-travel-creed/creed-return-fairy-and-recruitment-choices.md#eight-offers-and-ordered-successful-branches)保留各程序、提示/拒绝 ID 与成功目标。

| 同伴 | 初始邀请与 No 回应 | F22 邀请与 No 回应 |
| --- | --- | --- |
| Eric | 寻剑的骑士请求被选中；被拒绝后祝大家幸福。 | 他想要传奇之剑，请求加入；被拒绝后提议独自寻找。 |
| Randolf | 他报上姓名，自称战士力量出众并提出同行；被拒绝后说这里生活舒适。 | 他说已恢复原本大小，考虑同行；被拒绝后决定留下。 |
| Tyrin | 在 Ponpei 受训的法师提出帮助；被拒绝后说他的魔法本可帮忙。 | 他说自己自由了，请求加入；被拒绝后警告对方会后悔。 |
| Karna | 年轻祭司请求随队离开；她会辱骂拒绝她的人。 | 她庆祝自由，请求同行；被拒绝后说这样的选择很无礼。 |

每种邀请先显示两条台词，再给出真正的 Yes/No 选择。默认和 Left 选择 Yes 0；Right 选择 No -1。A/C 接受当前选择；B 选择 No。进入时已按住的输入必须先释放，才进入菜单循环。返回 Yes 设置 F89；非零 No 清除它，覆盖旧 F89。两条路径均有源码 sleep 10。F89 已设置分支到达成功；No 显示拒绝并结束，不加入、不设置 F787、不缩小、不跟随。对话 W1/W2 等待独立于此菜单。

**推断：**初始对话呈现挑选一名帮手，F22 邀请则表达后来可自由招募其他人。**已确认：**局部规则是 F22/F787 分支，不是通用的仅招一人限制。重新构建的 F784 入场即使 F787 已设置也能布置尚未加入者，但其初始回调随后给出拒绝文本。F22 绕过这种抑制。在这些分支下，若已加入行动者被提供/可达，也能再次收到邀请；重新 init 时的停放和普通随从排除可阻止自然交谈。寻剑、大小恢复、力量、自由和目的地均是台词；这些回调不授予剑/装备、职业变化、治疗或 AI。

## Yes 改变什么，按什么顺序

**已确认：**两种形式都先让选定己方角色加入。`JoinForce` 设置其已加入标志，经 UpdateForce 重建已加入/出战/后备列表及计数，不按 HP 过滤，然后仅在重建的出战计数小于 12 时设置其出战标志。为 12 或以上时跳过该出战写入；它不拒绝加入、不替换另一己方角色，也不清除已有出战标志。

| 己方角色 | 设置的已加入标志 | 有条件设置的出战标志 |
| --- | --- | --- |
| Eric14 | F14 | F46 |
| Randolf16 | F16 | F48 |
| Tyrin17 | F17 | F49 |
| Karna15 | F15 | F47 |

UpdateForce 在出战写入之前运行，此加入尾部没有第二次重建。新出战角色可仍留在此前的后备/计数快照中，直到另一消费者重建。已出战角色计数一次，其标志可再次设置。队伍已满或 HP 为 0 不阻止后续确认、F787、初始缩小操作或选定跟随。

加入处理器请求加入音乐，读取己方角色当前名字/职业，并请求通用文本 446。确认环节安排恢复先前音乐，等待非零标准方向/按键输入，不要求新的输入边沿；函数名不证明画面淡入淡出。文本关闭并 sleep 10 后，处理器才返回。然后场景才设置 F787。因此，若确认停滞或被中断，成员资格可已改变而 F787 仍未设置。[加入/容量所有者](../../../../../research/chapters/southern-travel-creed/creed-return-fairy-and-recruitment-choices.md#join-and-capacity-are-separate-from-following)及[出战队伍合同](../../../contracts/party/party-roster-state.md#处理器边界)保留调用、快照和输入边界。

初始 Yes 在 F787 之后，按下表精确顺序为另外三名行动者安装缩小行动。前两次为异步；仅等待第三次。之后才请求选中者跟随。这些具名目标不按已加入/出战资格或 HP 过滤。

| 选中者 | 第一次异步 | 第二次异步 | 第三次，等待 |
| --- | --- | --- | --- |
| Eric | Randolf | Tyrin | Karna |
| Randolf | Eric | Tyrin | Karna |
| Tyrin | Eric | Randolf | Karna |
| Karna | Eric | Randolf | Tyrin |

每次 F22 Yes 则是在加入后设置 F787，请求选中者跟随并结束，没有其他行动者的缩小操作。两种形式都以当前探索随从列表最后一名为目标；列表为空则用 physical 0。它们不明确选择 Fairy、Peter 或 Oddler。因此，先与 Fairy 还是先与可招募者交互，可能得到不同目标。此命令没有 HP 守卫、不创建缺失行动者，也不设置永久随从选择器。若选中者已是最后一名，可跟随自己；去重避免第二个列表条目，但仍分配新行动。跟随始终独立于军团成员资格和出战队伍容量。

## 缩小停放行动者，不移除成员资格

**已确认：**[完整缩小/行动所有者](../../../../../research/chapters/southern-travel-creed/creed-return-fairy-and-recruitment-choices.md#complete-shrink-action-and-completion-boundary)以 wait 5 和启用缩放开始。九次朝向/大小更新反复转 Down/Right/Up/Left，最终朝 Down；大小操作数依次为 22,20,18,16,14,12,10,8,6，每次更新后 wait 5。完整尾部将位置设为 `(63,63)`、禁用缩放、写大小 18 并跳转 Idle。这些是源码操作数和精灵服务请求，不是实测像素大小或帧时长。此体内最终 size 18 写入后没有 updateSprite。

布置写当前及目的地坐标，乘384 后为 `$5E80`，并清除行进状态，但不清除速度。它不使查找失效、不移除探索列表条目、不清除成员资格，也不释放行动者槽。`$5E80` 低于实体循环跳过阈值 `$7000`，允许剩余尾部运行。HideEntity 则停放在 `$7000`，使查找失效并移除列表条目；保留的行动指针处于休眠。因此，缩小和隐藏的状态后果不同。

等待式安装仅轮询自身行动者指针是否精确为 Idle，不汇合三次缩小行动。行动者索引、先前状态、精灵加载压力和更新可用性都会影响推进。目标别名可在同一物理槽上替换更早的安装；隐藏/无效映射不被修复。等待停滞不撤销加入或 F787，一名行动者到达 Idle 也不证明三者全部完成或渲染消失。

## 文本、说话者与交互返回

**已确认：**Goliath 的包装器使他转向玩家，并在返回时恢复保存朝向；其余六个特定行只转向，不恢复。这些标志描述行动行为，不是准入朝向要求。实际激活仍需满足视野/朝向、布局/柜台可达、附近行动者及反向查找条件。列出的随从被排除。直接调用回调与普通交谈是不同前提。

包装器按映射行动者的实际精灵选择立绘/台词声音。基线 worker 和 Fairy 没有立绘，Creed 与 Oddler 有；可招募者的职业/HP/精灵差异可改变立绘和台词声音。招募脚本使用其具名行动者及 modifier0；Fairy 的决定使用 `$FFFF` 叙述，绕过行动者查找并选择 speech SFX0。请求立绘不是渲染证据。[文本/控制所有者](../../../../../research/chapters/southern-travel-creed/creed-return-fairy-and-recruitment-choices.md#exact-text-joins-and-presentation-limits)保留全部 49 个房间文本位置、45 个不同房间 ID 及通用加入 446，此处不重复抄录台词。

显式名字控制读取存储的己方角色名字，不为可招募者改名。LEADER 重建军团，使用首名出战成员的名字。Join446 读取当前 NAME/CLASS，没有 W1/W2 token；其独立确认环节提供输入等待。N 为换行，W1/W2 等待输入，W2 另有继续箭头/确认声行为。即使字面文本以 W1 结束，继续文本与单条文本的关闭/睡眠行为仍不同。

名义 Fairy 游标结束于 1904。初始 No 结束于 1910/1913/1916/1919；F22 No 结束于 1923/1926/1929/1932。Yes 跳过拒绝，游标留在其 ID。解释器入口清除跳过状态后，Debug/P2 Start 可锁存文本跳过；显示和游标推进随后可被省略，而 yesNo、加入及其他命令仍运行。名义文本/游标顺序不证明每行都显示或异步行动者完成。[对话合同](../../../contracts/interface/dialogue-system.md#已确认的静态合同)保留处理器和有界服务范围。

包装器在回调周围禁用实体更新；脚本桥为行动执行重新启用。普通回调返回时，窗口关闭、更新重新启用。此返回没有全部随从到位等待、已验证存档或可用控制器输入观察。

## 中断状态与下一边界

**已确认：**这些操作有序，非原子。Fairy 复制、叙述和跟随可在调用方标志之前仅部分完成；写入之间中断可留下 F762 已设置、F785 未变。加入确认可留下已加入或出战标志改变、F787 未设置。F787 之后，即使后续初始邀请已受抑制，缩小或跟随仍可能未完成。全文始终区分标志变化的保留行动者与重新构建的行动者。

在 `(15,21)`，后续区域（事件）先检查 F774。未设置时选择更早的 Goliath 场景并返回，不继续落入后续分支。F774 已设置、F785 已设置且 F786 未设置时准入 `cs_5B466`。仅该独立场景返回后，调用方才依次设置 F786、设置 F69、清除 F68。它既不检查 F787，也不检查可招募者已加入标志，因此招募不是局部前提。[下一边界所有者](../../../../../research/chapters/southern-travel-creed/creed-return-fairy-and-recruitment-choices.md#changed-premises-return-and-next-boundary)仅确定该准入，不推断尚未接受的 Oddler 场景内部或自然区域（事件）可达。当前 resident130 跟随还不是稍后的 Fairy/Oddler 交接。

**未知：**自然到达的返回/交谈/区域（事件）请求；实际标志、HP、职业、模式、布局、人口、映射与成员资格/列表/计数快照；未经垫片替代的服务/行动完成、呈现/音频、可用控制及存档/读档持久性。这些限制不抹去已确认的有序源码行为，也不创建自动模拟器或采集队列。

## 证据审阅与保留结果

综合前审阅将已接受 Layer A 与选定公开载荷和消费者核对，区分源码结构与运行时可达：

| 审阅表面 | 处置与声明边界 |
| --- | --- |
| [#953 研究](../../../../../research/chapters/southern-travel-creed/creed-return-fairy-and-recruitment-choices.md)、已接受神殿/恢复前篇 | 完整房间路线、七个回调/八种邀请、文本及实际 HP/别名前提；不推导通用招募规则或后续场景。 |
| `sf2-map-events-static-v1`：[routing-setup](../../../../../../tests/fixtures/h2/map-events/routing-setup.json) 与 [entity-programs](../../../../../../tests/fixtures/h2/map-events/entity-programs.json) | 八个房间行/六个实体路由关联及全部 89 个回调操作；F22/F787 优先级、F786/F759 台词及有序762/785 一致。 |
| `sf2-map-init-static-v1`：[map-init fixture](../../../../../../tests/fixtures/h2/map-init-static-v1.json) | 普通 58 操作体与 F786 替代返回；F523/F783 和 F22/F784 分支、已加入检查及精确布置一致。 |
| `sf2-map-script-engine-static-v1`：[map-script fixture](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json) | 选定 join、addNewFollower、yesNo、followEntity、行动桥、hide 与当前图块复制处理器事实；此公开 fixture 不是完整私有 programCorpus。 |
| [事件分片/schema 消费者](../../../../../../src/sf2tool/h2/map_events_fixture.py)、[事件验证器](../../../../../../src/sf2tool/h2/map_events.py)、[脚本验证器](../../../../../../src/sf2tool/h2/map_script_engine.py)、[地图脚本 schema](../../../../../../schemas/h2/map-script-engine-fixture.schema.json) | 选定分片/章节与地图脚本 fixture 经 schema 验证；溯源及完整预期相等强于形状合法。不声称新源码/输出比较。 |
| [出战队伍/名册](../../../contracts/party/party-roster-state.md)与[对话](../../../contracts/interface/dialogue-system.md)合同 | 处理器局部快照、跟随分配及文本服务保留各自受控范围；广泛容量/持久性问题不覆盖已接受局部源码/计数顺序。 |

未发现阻塞矛盾。当前跟随与持久选择器、源码等待与完成呈现、局部军团容量与广泛生命周期未知项，均采用更严格研究边界。矛盾应交回研究所有者，不由 Layer B 修改研究。

研究固定 [SF2DISASM master c834c652b6862bc5679fd7f69a38a7093206efc6](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm) 及 USA ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。[复现所有者](../../../../../research/chapters/southern-travel-creed/creed-return-fairy-and-recruitment-choices.md#reproduction-and-review-boundary)选择完整 17 程序/105 记录体、49 个文本关联、缩小消费者及 init/人口证据。原英文切片的检出缺少该固定 upstream 和保留对；保留已接受证据，不获取、不重新生成，也不声称新比较。窄证据命令仍为 `uv run sf2 h2 map-init`、`uv run sf2 h2 map-events` 和 `uv run sf2 h2 map-script-engine`。

[#953 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/953#issuecomment-6178749558)、[变基回执](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/953#issuecomment-6178813915)及[独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/956#issuecomment-6178823238)保留未变的 18,285,979-byte 对比较 0.081s、原始 CI 38639160994 与变基后 CI 38639720868、完整关联、修正后的 FOLLOWER_C158/缩小事实及检查诊断。原始及变基后结果均保留，包括验收检查器修正后的空白正则；它们都不是中断的套件。

[#954 修正交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/954#issuecomment-6178761171)保留原始与修正 CI，以及调用回调与跳过咨询场景/调用方写入的区别。所链接的 [#949 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/949#issuecomment-6178414274)、[#945 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/945#issuecomment-6178009849)、[#941 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/941#issuecomment-6177767348)和 [#937 修正交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/937#issuecomment-6177356550)保留更早的比较、修正文本/高字节/休眠槽事实、已完成失败及未知项。原英文这些文字不引入已完成失败的重启、整套本地验证、运行时或翻译检查。
