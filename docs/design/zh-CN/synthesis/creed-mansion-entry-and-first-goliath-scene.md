# Creed Mansion 入场与首次 Goliath 阻拦

> 本文是[规范英文原文](../../synthesis/creed-mansion-entry-and-first-goliath-scene.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

**已确认：**Devil's Tail 门口请求全新 Creed Mansion 抵达。首次 Goliath 场景属于独立获准区域事件，不属于抵达初始化。
完整场景表现请求觐见、遭拒、尝试进入、拦截及表面退让；改变两个当前布局图块并安装临时跟随。
**推断：**预期第二名同伴是 Oddler。**未知：**自然到达、实际传入角色群/状态、服务完成、渲染演出、可用控制及持久性。

本指南使用[已接受入场/场景研究](../../../research/creed-mansion-entry-and-first-goliath-scene.md)，
接续 [Battle21 结果与 Devil's Tail 返回](battle21-outcomes-and-devils-tail-return.md)。
Standard 协议见 [Issue #895](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/895)
及[文档路线图](../documentation-roadmap.md#当前基线与近期综合)。
说明首次局部返回/重复，止于独立缩小事件前；不确立完整宅邸行程、后续招募、Desktop Kingdom/Battle22、渲染器/地形审计或重制能力。

## 门口和抵达独立于场景

**已确认：**前一胜利返回为完整 Map67 `(26,23)`、Down。独立 Map67 门口 `(26,22)` 提交 warpNoScroll 至 Map33 `(15,30)`、Up。
相邻不证明步行、通行或激活。传送处理器使用共享请求、使主角色空闲并退出至 MainLoop；no-scroll parameter1 为零。
普通准入调用 SwitchMap，**再调用 CheckBattle**，不同于替代战后返回。完整地图切换表和战斗坐标表都没有 Map33 行。
战斗扫描返回 d7=-1 并保留元组：仅传入解锁标志不会将此次抵达变为 Battle22。

全新探索清除待处理地图事件类型，从 STEP_COUNTER 减去 20000 并对负结果钳位，治疗符合条件己方角色并开始淡入淡出/资源工作。
它写入 CURRENT_MAP=33 和 CURRENT_BATTLE=255，选择配置，在请求抵达处初始化行动者并加载精灵/地图。
清除临时标志 256–383、设置 F80、安装基础 VInts 并调用选定 init。调色板比较控制之后的音乐/淡入淡出调用。
这些是以服务返回为条件的有序源码使用方，不证明抵达已执行。

CURRENT_BATTLE=255 选择**完整地图**布局域。陈旧 Battle21 裁剪偏移不移动宅邸抵达或场景操作数。
显式 Map33 请求也走全新实体生成，而不是 map 255 保留实体群分支。
WaitForEvent 中待处理工作优先；仅 pending-zero 将 physical main 0 安装为摄像机目标和当前模式控制器。
[地图合同](../contracts/map-exploration.md#加载与选择顺序)负责通用加载/布局规则；
[请求使用](../../../research/map-event-request-consumption.md)负责共享槽位生命周期。两者均不保证这条具体自然路线。

## 场景实际接收的配置与行动者

**已确认：**选定首次访问的 F523/F783/F784/F786/F22 均未设置，初始 F774 也未设置。
GetCurrentMapSetup 从默认项开始，对终止符前**每个**已设置行替换一次：最后设置标志胜出。

| 源码顺序 | 选定指针 |
| --- | --- |
| 默认 | ms_map33 |
| F523 | ms_map33_flag523 |
| F783 | ms_map33 |
| F784 | ms_map33_flag784 |
| F786 | ms_map33_flag786 |
| 最后 F22 | ms_map33 |

五者均未设置选择 ms_map33，但指针身份不证明首次访问状态：F783 或 F22 可在较早替代项之后恢复同一指针。
F774/F785 不选择指针。每个访问器重新解析配置，不保证调用间标志冻结。
指针六字段为 Entities、EntityEvents、ZoneEvents、AreaDescriptions、Section5 和 InitFunction。
[配置选择](../../../research/map-event-flag-route-selection.md)负责共享选择器；已接受入场研究提供完整 Map33 关联。

基线实体列表声明六个固定行，均以 eas_Init 停放于 `(63,63)`：

| 行 / 逻辑身份 | 地图精灵来源 | 初始朝向 |
| --- | --- | --- |
| 0 / NPC 128 | WORKER=202；场景将其标识为 Goliath | Up |
| 1 / NPC 129 | CREED=212 | Left |
| 2 / Eric 14 | 己方精灵查找 | Down |
| 3 / Randolf 16 | 己方精灵查找 | Up |
| 4 / Tyrin 17 | 己方精灵查找 | Down |
| 5 / Karna 15 | 己方精灵查找 | Up |

停放声明不证明可见室内居民。初始化清除实体/查找，从 physical 1 分配选定随从，再处理固定行。
表坐标以 `$3F` 掩码并按 MAP_TILE_SIZE=384 缩放。普通 NPC 将 physical 索引追加到偏移 32 起的查找区；
logical 128/129 经该表解析，不是字面 physical 槽位。已有非零己方查找的固定己方行被完整跳过。
这些行无已加入/出战成员资格门槛。没有固定 Peter 行或 newEntity 回退。
Bowie 在 physical 0、`(15,30)`、Up、eas_Idle 被声明。

Map33 **不是大地图**。F65 在此不贡献 Caravan/A 前缀。普通随从标志仍选择其行。
这些行中仅 F67/F68 被选择时，Peter 7 是 physical 1，使用 eas_Follower1；B 31 使用 Oddler 精灵，是 physical 2，使用 eas_Follower2。
NPC 128/129 随后映射 physical 3/4，Eric/Randolf/Tyrin/Karna 映射 5/6/7/8。
随从初始接收传入抵达坐标/朝向。这是条件分配示例，不是保证的角色群。

其他选定随从行改变 physical 分配。F74/F68/F70 可用 Astral/Oddler/Astral 精灵重复声明 B；初始化不去重这些行。
最后一行写入逻辑 B 查找，而较早 physical 行动者保留。缺失 Peter/B 查找为零，可别名到 Bowie 0。
成员资格不提供缺失随从标志。职业和 HP 也影响己方精灵：战斗外零 HP 选择蓝焰，入场治疗不复活所有死亡角色。
固定声明、军团成员资格、选定随从和当前动作关系仍为不同[名册边界](../contracts/party-roster-state.md)。

此入场/init/场景不改变 PLAYER_TYPE；一致前篇提供步行 0。F64 加 RAFT_MAP=33 独立允许在 physical 31 声明已保存木筏，
以 `$7F` 掩码已保存坐标；这不登乘或重定位。传入 mode 2 让主角色使用木筏精灵，随从在木筏声明前站立。
控制器选择为 mode 0 ControlledCharacter、mode 1 经源码名 eas_Raft 使用 Caravan 控制，否则经 eas_Caravan 使用木筏控制；此处不验证任意模式。
对话不选择运输状态。

完整基线 init 有独立有序守卫：

| 守卫 | 直接效果 |
| --- | --- |
| F523 已设置、F783 未设置 | 后续恢复 cs_5B016，再写 F783 和 EGRESS_MAP=Hassan；程序体排除。 |
| F22 已设置 | cs_5ABA8 将 128 置于 `(14,14)`、Up，绕过后续放置替代项。 |
| 否则 F784 已设置 | 跳过中间放置。 |
| 否则 F783 已设置 | cs_5ABCC 将 128 置于 `(17,10)`、Down。 |
| 否则 F774 已设置 | cs_5ABE6 将 128 置于 `(17,12)`、Down。 |
| 尾部 F784 已设置 | 放置每个未加入的 Eric/Randolf/Tyrin/Karna；后续招募上下文排除。 |

五个选定后期剧情标志及 F774 均未设置时，init 不执行这些脚本或写入便返回。
它既不运行 cs_5AC58，也不创建该区域请求。基线 F774 已设置的重新入场只在重建实体群后将 128 置于 `(17,12)`、Down，
既不重放对话，也不重放门复制。

## 获准区域事件启动觐见请求

**已确认：**区域生产方识别布局类型 `$1400`，写 event type 6，并从主角色目的地加移动偏移、除以 384 导出字坐标。
仅表行不证明自然输入/碰撞可达或准入。ProcessMapEvent 在分发前清除待处理类型；type 6 安装主角色 eas_Init，
把坐标字交给当前配置区域包装器。[共享区域调用方](../contracts/dialogue-system.md#源码区域调用方直至返回)负责其服务和返回谓词。

首个匹配 `(15,21)` 行选择 Map33_ZoneEvent0。抵达为 `(15,30)`，不是该区域。
F774 未设置时，Zone0 调用**全部** cs_5AC58。只有返回后 Zone0 才设置 F774 并直接分支返回；首次调用不检查后续 F785 分支。

场景有 129 个命令，索引 0–128，带 17 个完整内联动作体和六个自定义体。
已接受源码所有者标明 scripts.asm lines 4–191、address 0x5AC58，结束于 cs_5AF36/0x5AF36 前的 csc_end。
下文 P 是 logical Peter 7，B 是 FOLLOWER_B 31，128/129 是逻辑 NPC。每个位置、相对移动、速度及等待均为源码操作数，不是实测运动/时间。
下列内容章节覆盖完整程序，包括并发工作。

### 遣退、门操作与接近（命令 0–39）

NPC 129 遣退仆人以便与 仙女 交谈；128 应答。摄像机移至 `(12,11)`，等待 30，stepEvent `(17,11)` 改变当前图块。
NPC 128 被置于 `(17,11)`、Down，带等待移动 Down 3，再等待 5 后启动 eas_2xRightLeft，不等待空闲。
等待 80 后发出哼唱/记起检查收藏的文本；摄像机跟随 128，等待其 Left 5 移动，再朝 Up。
这次等待 80 不会在移动替换前完成整个具名动作。

Peter 和 B 获得带等待的 eas_Init。P/Bowie/B 置于 `(15,21)/(15,22)/(15,23)`、Up。
第二个 stepEvent `(15,20)`、摄像机目的地 `(8,12)` 及等待 30 后，无说话者旁白称 Goliath 开锁。
安装三组接近动作：P Up 4/Left 3/Up 1、Bowie Up 5/Left 3、B Up 6/Left 2。**只等待 B**。
Bowie/B 朝 Up；Peter 请求注意。NPC 128 颤抖、回应、停止动画、等待 30、转 Right、等待 60、转 Down、等待 30、
重新开始动画，再犹豫回应。停止动画不是移动锁。

### 预约、拒绝与同伴计划（命令 40–74）

等待 Peter Up 1 后，他请求见 Creed。Goliath 询问是否指其主人，再问是否预约；Peter 不理解。
等待 Peter Down 1、Bowie 摇头，再等待 Peter Up 1。Peter 承认没有预约；Goliath 称 Creed 忙碌，要求改日再来。
等待 Peter Down 1 后，他报告遭拒。Bowie 朝 Right、B 朝 Left。B 作出反应，使用运行时领队/Peter 名称替换，敦促强行进入。
等待 Peter Right 1 并转 Down 后，他赞同其称为 Oddler 的大胆主意；Bowie/B 朝 Up。

NPC 128 朝 Up，等待 10 并颤抖。等待 Peter Left 1；Bowie/P/B 朝 Up。NPC 128 再颤抖，等待 10 后表示不赞同，等待 20 后 Peter 发出信号。
**推断：**显式 Oddler 称呼加 F68 精灵支持预期 B 归属。它们不证明 F68 已传入，也不排除其他 B 查找。
对话确立角色陈述和拟议行动，不确立可靠设定、成功强行进入或后续行程。

### 尝试进入与拦截（命令 75–112）

摄像机跟随 Bowie。安装 Bowie/P eas_Jump，再等待 B eas_Jump。
依次给 Bowie/P/B 安装三个带等待的自定义体：ac_setSpeed 40,40；ac_jump eas_Idle；ac_end。
给 P、B 和 Bowie 安装 Right 5/Up 3 移动，无个别等待。等待 10 后，NPC 128 获得 WIPE_OUT，被显式置于 `(17,12)`、Down，再获得 WIPE_IN；
waitIdle 仅等待 Bowie。效果对不创建行动者，也不执行传送；源码编排没有测量超自然速度。

先安装 P/B eas_BumpUp，再等待 Bowie eas_BumpUp。随后 Peter 提问、NPC 128 颤抖，并发出斥责/禁止入内/赶走文本。
Bowie/P/B 依次运行带等待 eas_DeactivateAutoFacing。再按同顺序安装三个完整带等待自定义体，设 speed 10,10 并跳至空闲。
先安装 Bowie/P Down 1，再等待 B Down 1。带等待 eas_Init 重置 Bowie/P/B 动作，
随后 wait 25 和 wait 5，Peter 启动不等待的 eas_2xRightLeft，再 wait 120。

全部 17 个内联体以 endActions 结束：早期两个 NPC 移动、三组接近动作、预约交流中四次 P Up/Down、P Right 1/Left 1、
三次 Right 5/Up 3 尝试，以及三次 Down 1 撤退。上述六个自定义体以 ac_end（`$8080`）结束。
40/40 和 10/10 是压缩速度操作数，不是每帧像素。
共享[移动/动作计时](../contracts/map-exploration.md#实体移动与动作时序)不证明每项并发移动均到达目的地。

### 表面退让与临时跟随（命令 113–128）

Peter 提出左侧门可能通向 Creed，转 Down，再建议避开仆人。Bowie 转 Right、B 转 Left，听 B 鼓励。Bowie 点头。
Peter 转 Up，假装让步，说他们稍后再来并离开。B/Bowie 朝 Up，B 同意。这是拟议替代方案和表面退让，不是已完成路线。

csc_end 前最后两条命令依次安装 P→Bowie、B→P，place 2，偏移 `(-24,0)`。
场景没有传送、标志写入、招募、选择或调用下一程序。
关键是 csc2C 为己方存活守卫测试**每个随从字的高字节**：7 和 31 都提供 0，因此均测试 Bowie HP。
获准时，实际查找决定 physical 随从/目标身份。AddFollower 将带目标/偏移的 42 字节 eas_Follower1 模板复制到步行缓冲，
仅在 physical 随从不存在时追加至 EXPLORATION_ENTITIES。不改变已加入/出战或持久随从标志。
执行后的模板重置速度 32/32 和移动标志，再跟随。两个调用都不等待稳定，也不定义绝对瓦片。
别名可自跟随或覆盖另一关系；重新入场丢弃这些动作并从随从标志重建。

## 全部三十条文本及使用方

**已确认：**textCursor 1804 选择连续 gamescript IDs 1804–1833（`070C`–`0729`）。
已接受所有者检查每项字面文本/注释，包括空格/控制符。此表释义内容并保留有序 token；S=nextSingleText，T=nextText。
除 1807 外，说话者均使用 modifier 0。

| ID | 形式 / 说话者 | 内容 | 有序控制符 |
| --- | --- | --- | --- |
| 1804 | S / 129 | 遣退仆人以便与 仙女 交谈 | `{N}{W1}` |
| 1805 | S / 128 | 应答主人 | `{W1}` |
| 1806 | S / 128 | 哼唱，记起检查收藏 | `{N}{N}{W1}` |
| 1807 | S / ENTITY_NONE，`$FF` | 旁白：Goliath 开锁 | `{N}{W1}` |
| 1808 | S / P | 请求注意 | `{W1}` |
| 1809 | S / 128 | 惊呼 | `{W1}` |
| 1810 | S / 128 | 犹豫回应 | `{W1}` |
| 1811 | S / P | 请求觐见 Creed | `{W1}` |
| 1812 | T / 128 | 询问是否指其主人 | `{W2}` |
| 1813 | S / 128 | 询问预约 | `{W1}` |
| 1814 | S / P | 表示不理解 | `{W1}` |
| 1815 | S / P | 承认没有预约 | `{W1}` |
| 1816 | T / 128 | 称 Creed 忙碌 | `{W2}` |
| 1817 | S / 128 | 要求稍后再来 | `{W1}` |
| 1818 | S / P | 报告遭拒 | `{W1}` |
| 1819 | T / B | 对拒绝作出反应 | `{W2}` |
| 1820 | S / B | 敦促领队和 Peter 强行进入 | `{LEADER}{NAME;7}{N}{W1}` |
| 1821 | T / P | 称 Oddler 大胆 | `{W2}` |
| 1822 | S / P | 赞同主意 | `{W1}` |
| 1823 | S / 128 | 表示不赞同 | `{W1}` |
| 1824 | S / P | 发出尝试信号 | `{W1}` |
| 1825 | S / P | 质疑突然拦截 | `{W1}` |
| 1826 | T / 128 | 斥责众人并禁止入内 | `{N}{W2}` |
| 1827 | S / 128 | 再次拒绝并赶走众人 | `{N}{W1}` |
| 1828 | T / P | 提出左门可能通向 Creed | `{LEADER}{N}{N}{W2}` |
| 1829 | S / P | 建议经该门避开仆人 | `{N}{W1}` |
| 1830 | S / B | 敦促领队前进 | `{LEADER}{N}{W1}` |
| 1831 | T / P | 假装让步 | `{W2}` |
| 1832 | S / P | 称将稍后再来并离开 | `{N}{W1}` |
| 1833 | S / B | 同意表面退让 | `{W1}` |

[对话合同](../contracts/dialogue-system.md#已确认的静态合同)负责共享游标、显示、关闭及等待规则。
每次选定显示等待摄像机滚动、使用 DisplayText 并递增游标。S 关闭立绘/文本并 sleep 10；T 保留继续。
`{N}` 为换行；W1/W2 区分文本等待/继续，不是选择。名称控制符解析运行时名称。
`$FF,ENTITY_NONE` 编码 `$FFFF`，绕过说话者/立绘查找并选择 speech SFX 0。
其他说话者经对话属性解析实际映射精灵；modifier 0 不设置右侧或镜像立绘位。已有/缺失立绘状态仍有影响。
这些事实不保证渲染立绘或声音。Debug/P2-Start 可锁存 SKIP_CUTSCENE_TEXT；名义三十条文本/csWait 顺序以未锁存跳过模式为前提。
B 身份仍为传入值。

## 门变化与动作完成有各自边界

**已确认：**csc47 将 step 操作数乘以 384 并调用 OpenDoor；除非 CURRENT_BATTLE=255，否则立即返回。此处选定记录为：

| 命令 | 触发 / 目的地 | 来源 | 大小 |
| --- | --- | --- | --- |
| 5 | `(17,11)` | `(26,0)` | 1x1 |
| 20 | `(15,20)` | `(25,0)` | 1x1 |

非负 source Y 选择 FF0000 中行步长 64 字的当前布局单字复制。
OpenDoor 播放 SFX_DOOR_OPEN，并设置 MAP_AREA_LAYER_TYPE 的平面更新位。
它不写 F774、存档、传送或持久事件标志。Map33 完整标志事件表只有 F22 的大范围替换，没有 F774 重放这些复制。
基线全新重新入场不会仅凭 F774 还原它们。旁白开锁及图块变更都不证明渲染开口或碰撞/通行。

已接受[场景/动作使用方审阅](../../../research/creed-mansion-entry-and-first-goliath-scene.md#block-copies-actions-and-presentation-boundaries)
负责精确宏/处理器。摄像机目的地禁用跟随并等待滚动；摄像机行动者命令解析 physical 目标。
setPos 同时改变位置和目的地；朝向/放置跳过零 HP 己方角色，不修复缺失查找。动作安装器没有该放置 HP 跳过。
其等待变体及 waitIdle 只轮询解析行动者是否 eas_Idle，无超时/恢复。
相对移动编译到缓冲，将有符号差值乘以 384 并检查其他实体目的地冲突；安装不证明抵达。

选定 eas_Jump/eas_BumpUp 禁用自动朝向，移动 `(0,-1)`、等待 5、移动 `(0,1)`、等待 5、移动 `(0,0)`、还原自动朝向并跳至空闲。
eas_Init 重置速度 32/32、加速度因子 0/0、关闭加速/减速、开启可阻挡、关闭 entity/map uncollidable、开启自动朝向、orientation Up、
关闭浸没、尺寸 24 及精灵，再空闲；它不是额外位置设置。
eas_2xRightLeft 改变朝向/精灵 Right/Left/Right/Left，每次等待 30。
Shiver 执行三次 wait 5/wait 5 迭代并还原尺寸/动画/标志状态；nod/headshake 使用同步精灵/DMA/等待序列并还原动画。
WIPE_OUT 2/WIPE_IN 3 选择 `(1,0,1,0)` 和 `(22,0,-1,0)` 行，各有 23 次裁剪/DMA 迭代，每次两次 VInt 等待。
这些都不证明可见完成、实测计时、行动者创建、mosaic 行为或通用渲染器合同。

## 返回、立即重复与重新入场

**已确认：**csc_end 在存在对话窗口时有条件地等待视野滚动，清除 VIEW_SCROLLING_SPEED 并还原调用方寄存器。它不设置 F774。
Zone0 之后才写 F774。区域包装器关闭立绘/文本、等待一次 VInt，再等待 **physical 0 当前 X/Y 等于目的地 X/Y**；
不等待全部随从脚本空闲。WaitForEvent 在普通摄像机/控制器安装前处理新待处理工作。
若后续动作、文本/输入、滚动、DMA/VInt 或移动服务停滞，先前变更不会回滚。
后续包装器等待停滞时，F774 可能已经设置。

| 传入状态 | 局部后果 |
| --- | --- |
| F774 未设置、选定后期标志未设置 | 获准 Zone0 运行完整首次场景，再写 F774；首次分支不检查 F785。 |
| F774 已设置、F785 未设置 | Zone0 不运行场景/移动/文本/标志变化便返回；包装器关闭/等待仍适用。 |
| F774 已设置、F785 已设置、F786 未设置 | 后续 cs_5B466，再设置 F786/F69 并清除 F68；已标识，程序体排除。 |
| F774 已设置、F786 已设置 | 绕过后续离开；配置优先级可能已选择其他实体群/init。 |
| 全新基线重新入场、F774 已设置 | 重建实体群，将 128 置于 `(17,12)` Down；不自动重放对话/门复制。 |
| 缺失/覆盖随从查找、改变 HP/职业 | 别名、改变精灵或跳过放置/朝向/跟随；不保证行动者或完成。 |
| 传入运输状态、待处理事件或服务停滞 | 实际模式/木筏/待处理/服务使用方控制继续；正文不提供可用控制结果。 |

区域 `(12,14)` 独立：F783 未设置调用 cs_5AF36，已设置则返回。其缩小/收集程序体排除。
首次场景结束不落入该程序；同伴计划和跟随关系不保证后续激活。
F774/后期剧情/随从标志在临时 256–383 重置后保留，但 RAM 保留不确立存档/读档持久性。
止于此条件局部返回/重复，在缩小、后续离开/恢复/招募或 Battle22 前。

后续独立阅读 [Creed 魔法门与 Desktop 转移](creed-magic-door-and-desktop-transfer.md)。

## 证据审阅与保留限制

**已确认溯源：**所用研究已在 main `887c6f6772c0c5f785a3a3684a14a7b0cfd19d7e` 接受，固定于
[SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)，
USA ROM SHA-256 为 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。
[研究所有者](../../../research/creed-mansion-entry-and-first-goliath-scene.md#agreement-and-provenance)
负责精确选定路径、地址、命令/程序体/字面文本及使用方关联。本综合审阅已接受证据，不重新调查原始源码或新增游玩流程。

| 已审阅表面 | 支持与限制 |
| --- | --- |
| 完整已接受入场研究及前篇指南 | 门口/准入、完整地图域、最后设置标志配置、全部基线行动者/init/区域关联、129 命令/17 内联/六个自定义体/三十条文本、两次当前复制、动作及返回/重复。选定直接源码关联负责局部完整性。 |
| `sf2-map-init-static-v1`，[fixture](../../../../tests/fixtures/h2/map-init-static-v1.json)，expected dispatcher/entity/init facts | 共享配置/实体群/init 上下文。仅汇总事实不证明全部 Map33 内容或自然抵达。 |
| `sf2-map-setup-static-v1`，[fixture](../../../../tests/fixtures/h2/map-setup-static-v1.json) | 配置选择器/访问器事实；指针身份不是首次访问状态。 |
| `sf2-map-script-engine-static-v1`，[fixture](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，expected programSummary 及选定命令族事实 | cs_5AC58 及解释器使用方的公开有序命令族引用。完整命令体/字面文本仍属已接受保留/直接源码关联，不是公开汇总载荷保证。 |
| `sf2-map-events-static-v1`，[zone-programs 分区](../../../../tests/fixtures/h2/map-events/zone-programs.json) | 选定 Zone0 分支/脚本/标志操作；回调不同于包装器/服务完成。 |
| 上文链接的对话、地图及名册合同 | 共享服务/状态边界。早期 H3 接缝及已接受重制绑定不证明原版宅邸运行时或组合场景成功。 |

既有 schema/溯源验证使用 [map-init schema](../../../../schemas/h2-map-init-static-fixture.schema.json)、
[setup schema](../../../../schemas/h2-map-setup-static-fixture.schema.json) 和 [script schema](../../../../schemas/h2/map-script-engine-fixture.schema.json)，
通过 [jsonio.validate_json](../../../../src/sf2tool/jsonio.py)。区域分区使用既有 [shard schema](../../../../schemas/h2/map-events-fixture-shard.schema.json)
及 [zone section schema](../../../../schemas/h2/map-events-zone-programs.schema.json)；全部 14 个 Zone0 操作保留选定标志/脚本/分支顺序。
已审阅验证器为 [map_init.py](../../../../src/sf2tool/h2/map_init.py)、[map_setup.py](../../../../src/sf2tool/h2/map_setup.py)、
[map_script_engine.py](../../../../src/sf2tool/h2/map_script_engine.py) 和 [map_events.py](../../../../src/sf2tool/h2/map_events.py)。
形状/溯源/语义门禁不复现每项正文声明。
[研究复现路线](../../../research/creed-mansion-entry-and-first-goliath-scene.md#reproduction-and-review-boundary)负责保留相等比较及选定源码检查。
这里不重新生成载荷、比较或观察。

保留 [#892 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/892#issuecomment-6173418824)
及 [PR894 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/894#issuecomment-6173456022)，
包括原始/变基后 head/tree/base 及两份 CI 收据、未改变的 18,285,979 字节对/0.081 秒比较、完整选定关联及检查失败。
保留 [#891 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/891#issuecomment-6173346871)
及 [PR893 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/893#issuecomment-6173367158)，
包括前序失败及已解决生命周期摘要 P2，以及不同 CI 收据。
交接定位检查；跟踪所有者和已接受 Git 对象确立事实。本切片直接检查、已配置提交 planner 及实际最终 CI 属于 Issue #895。

**未知：**自然路线/到达/行程；传入标志/出战队伍/HP/职业/查找、几何/模式/木筏/待处理状态；
服务完成、实际场景/立绘/声音、可用控制及存储持久性。
改变状态行是静态区分条件，不是已执行场景或自动运行时队列。
本指南既不完成战役，也不完成 [epic #514](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/514)。
