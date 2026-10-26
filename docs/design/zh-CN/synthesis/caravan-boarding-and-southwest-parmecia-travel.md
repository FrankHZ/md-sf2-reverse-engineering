# Caravan 登乘与 Southwest Parmecia 旅行

获取 Caravan 会准备一个新的大地图随从；进入它和控制它是独立操作。步行可请求登乘顺序，使 Bowie 与选定随从缩小，随后将控制器改为模式 1。该控制器可以移动、请求传送或请求登陆。登陆使步行行动者变大，并在返回模式 0 之前重新安装随从动作。后续步行区域（事件）可请求 Battle19；Caravan 控制器不生产该区域事件。

**已确认：**已接受的[研究所有者](../../../research/caravan-boarding-and-southwest-parmecia-travel.md)确立这些选定的静态生产方、完整登乘／登陆程序及紧邻使用方。**推断：**它们共同说明进入 Caravan、乘坐旅行及离开。**未知：**自然行经路线、实际传入状态、表现／服务完成、能响应的控制及存档／恢复持久性。源码顺序不确立组合运行时成功。

此前的 F65/F84 变更及有条件神殿返回见 [Battle18 结果与获取](battle18-outcomes-and-caravan-acquisition.md)，独立警告／准入见 [Taros 进入](southwest-parmecia-through-taros-battle18-entry.md)。[Rick 招募（英文）](../../synthesis/hassan-rick-recruitment.md)说明可选的当前跟随，不是必经绕路。[木筏指南（英文）](../../synthesis/polca-raft-boarding-through-south-parmecia-entry.md)保留其不同模式与保存位置生命周期。本指南停止于登陆后有条件的步行控制，或 MainLoop 的 Battle19 交接，早于开场、放置、回合（单次行动）或结果。C 键菜单准入止于菜单调用；不包含事务、存储、村庄内容或后续旅行。独立接受的结果／到达边界见 [Battle19 结果与独立 Elven Village 进入（英文）](../../synthesis/battle19-outcomes-through-elven-village-entry.md)；本指南仍停止于准入。Fnnn 表示标志 nnn。瓦片坐标与等待为源码操作数，不是观察到的位置或时长。

## 离开神殿，先于重新生成大地图实体

**已确认：**选定的 Map12 南出口记录匹配任意 X、Y 31，请求无滚动 Map68 `(55,44)`、Down。获取场景设置 F65 并清除 F84，却没有写入 PLAYER_TYPE 或请求登乘。该记录与场景都不确立走到出口的路径或可到达的登乘瓦片。

`WarpIfSetAtPoint` 按顺序扫描记录，使用除以 384 的坐标及作为通配符的负坐标字节。匹配时写入事件 1、其滚动／地图／坐标／朝向参数及 WARP_SFX。`ProcessMapEvent` 在分发前清除待处理状态。无滚动使用方提交／清除声音、使物理实体 0 空闲、展开探索调用栈、采样当前位置，再替换为非负请求坐标及请求朝向。它返回 `(68,55,44,Down,d4=0)`，不清除运输模式。

MainLoop 在全新探索**之前**调用 SwitchMap 与 CheckBattle。完整切换地图表无 Map68 替代；Map68 唯一战斗坐标记录行为 19，区域 `(5,19)`、大小 48x32，通配触发字节 `(255,255)`。F419 必须设置，但两个触发字节都不要求位于该区域内。F519 已设置时在选择后清除 F419，本次调用仍准入 Battle19。F65、F518、F725、Sword 持有及运输模式不增加准入门槛。若 F419 独立传入，则停止于此 BattleLoop 交接，不声称先发生了全新 Caravan 实体生成。

没有该准入时，按传入地图运行的 ExplorationLoop 清除待处理状态，从有符号字 STEP_COUNTER 减去 20000 并将负结果钳位，治疗存活／不死己方角色，淡出／加载，写入 CURRENT_MAP 68/CURRENT_BATTLE 255，重建实体，清除临时标志 256–383 并设置 F80。Map68 有一个配置、空固定实体列表及只返回的初始化。条件音乐／淡入淡出后，待处理值为零允许 WaitForEvent 跟随物理实体 0 并安装传入 PLAYER_TYPE 控制器。安装仍以此前服务完成且无新事件为条件。

## 实体生成、逻辑名称与物理槽位

**已确认：**全新初始化清除实体／查找表，从 physical 1 开始分配按标志选定的随从，处理固定记录，另行声明 main 0，并终止 EXPLORATION_ENTITIES。Main 0 不追加到该全新列表。由于 Map68 是大地图，F65 已设置时选择大地图表：其首行为 Caravan mapsprite 62、logical FOLLOWER_A 30，分配到 physical 1；随后为普通随从。F65 未设置时改选普通随从。分配不检查 HP。

F65 与运输模式有不同使用方。F65 已设置／mode 0 会创建独立 Caravan 随从，同时保留步行。F65 未设置不会修复独立传入的 mode 1。全新 mode 1 不重放登乘；初始化的特殊运输处理针对 mode 2／木筏。首次获取清除了 Rohde 的 F84，重复胜利则保留其传入值。Peter F67、Oddler F68 及其他所有随从标志仍为独立输入。

逻辑查找与原始物理操作数必须分开。后续 F74/F77/F83 记录可覆盖 FOLLOWER_A 查找，而 Caravan 仍为 physical 1。仅有 F65/F67/F68 时，全新列表为 `[1,2,3,-1]`：Caravan、Peter、FOLLOWER_B。FOLLOWERS_LIST 从 EXPLORATION_ENTITIES 之后一个字节开始，因此其后缀为 `[2,3,-1]`；仅有 F65 时它立即为空。这是跳过首字节，不是搜索逻辑 Caravan。F65 未设置或标志改变、缺失映射、过多随从及被 Rick 扩展的当前列表，不必与该全新示例行为相同。全新表无 Rick 记录。选定操作均不修复容量或行动者身份。

## 实际运行的数字控制器与事件

**已确认：**已接受的枚举／宏／分发器／动作关联在固定源码基线上解析出这些名称：

| 数字值 | 上游名称 | 实际使用方 |
| --- | --- | --- |
| PLAYER_TYPE 0 | PLAYERTYPE_BOWIE | eas_ControlledCharacter、ac_controlCharacter、opcode 2：步行。 |
| PLAYER_TYPE 1 | PLAYERTYPE_CARAVAN | eas_Raft、ac_controlRaft、opcode 7：此处说明的模式。 |
| PLAYER_TYPE 2 | PLAYERTYPE_RAFT | eas_Caravan、ac_controlCaravan、opcode 8：实际木筏。除 0/1 外的其他值也选择此分支。 |
| Event 2 | MAP_EVENT_GETINTOCARAVAN | ProcessMapEventType2_GetIntoCaravan → j_MapEventType2 → MapEventType2。 |
| Event 4 | MAP_EVENT_GETOUTOFRAFT | ProcessMapEventType4_GetOutOfCaravan → j_MapEventType4 → MapEventType4：实际 Caravan 登陆。 |
| Event 5 | MAP_EVENT_GETOUTOFCARAVAN | ProcessMapEventType5_GetOutOfRaft → MapEventType5：实际木筏离乘。 |

Event 1 为传送，event 3 为实际木筏登乘，event 6 为区域事件分发。GETOUT 与载具动作标签和数字路由不一致；应同时保留名称与实际执行体。`$100FF` 即源码 MAP_EVENT_RELOADMAP 常量，是打包长字写入，不是另一个分发器用例。

### 步行请求登乘；C 请求菜单

**已确认：**步行 opcode 2 对物理实体 0 使用 CURRENT_PLAYER_INPUT，layer 1 边界缩小一个瓦片；其他槽位使用 PLAYER_1_INPUT 和未缩小边界。它按 Up/Down/Left/Right 顺序检查，允许后续符合条件的输入替换轴／朝向选择；水平斜坡分支可调整 Y。没有符合条件的方向时，脚本推进而不移动。FLAGS_A bit 5 已设置时，阻挡扫描 32 个槽位中 bit 7 已设置的行动者，同时使用当前位置与目标位置；两个轴距离都必须小于 `$100`。阻挡会跳过布局／移动处理，但仍可到达精灵／朝向更新。

在登乘分支，工作布局位经掩码 `$3C00` 后等于 `$3800`、CURRENT_BATTLE 255 且 F65 已设置时，**只写入待处理事件字 2**。该分支先于原始 `$C000` 阻挡，并跳过目标、速度、行进与 STEP_COUNTER 工作。朝向只能经精灵／自动朝向／容量条件更新。它不检查 Caravan 坐标、模式、F518/F725、HP、成员资格或逻辑映射。阻挡行动者可阻止到达此分支；Caravan 随从动作执行时会清除其阻挡位。后续 F64/`$3C00` 木筏分支独立。

普通 C 路径则先等待空闲／运动／视野条件，保留调试替代路径，并在缩小／菜单调用顺序之前检查 **physical 1** 的 Caravan 精灵及 physical 0/1 目标坐标相等；该调用顺序止于 `j_CaravanMenu`。这与事件 2 是不同准入；调用不是菜单选择、存储／出战队伍操作或事务完成的证据。

## 登乘：先缩小，最后改变模式

**已确认：**MapEventType2 使物理实体 0 空闲，执行完整 `cs_45284`，将 `byte_45316` 应用于 FOLLOWERS_LIST，经 WaitForFollowersStopped 等待，只有随后才写入 PLAYER_TYPE 1。它没有木筏式对齐门槛，也不重新检查 F65／战斗状态。它不创建行动者、不让任何人加入／获得治疗，也不保存载具位置。MakeEntityIdle 安装空闲指针，不清除运动。

| cs_45284 记录 | 有序动作与完成谓词 |
| --- | --- |
| 0 | 等待 FOLLOWER_A 的 eas_452A4：从原始 physical 0 当前位置，按 physical 0 朝向请求一个瓦片，等待自身目标，然后空闲。 |
| 1 | 等待 Bowie 的 eas_452B2：以距离 0 请求原始 physical 1 当前位置，随后进入 eas_452BA：等待 6、resize ON、传送 SFX、尺寸 22/20/18/16/14/12 及刷新，连续尺寸间 wait 1，等待自身目标，克隆 physical 1，空闲。 |
| 2 | 将 logical FOLLOWER_A 设置到 `(64,64)`、Down：当前／目标变为 `$6000,$6000`；更新朝向／精灵，查找／列表保留。 |
| 3 | 等待 Bowie 的 eas_45308：Caravan mapsprite 62、resize OFF、刷新、空闲。 |
| 4 | End：普通脚本返回，包括条件视野滚动等待与摄像机速度清除。 |

等待动作命令解析逻辑行动者并轮询其空闲指针，不验证 HP／存在性。FOLLOWER_A 30 不在放置时按存活状态跳过的己方 0–29 范围。清空的缺失查找可别名到 main 0；无效 255 不是安全的缺席哨兵。没有任何检查验证映射的 FOLLOWER_A 是 physical 1 或具有 Caravan 精灵。

相对移动动作读取其原始物理目标的**当前位置／朝向**，不是逻辑查找或目标位置，随后按既有速度设置目标／行进／有符号速度。它绕过步行碰撞／布局检查。Clone 复制物理位置、目标、行进和朝向并刷新；不复制速度、精灵、成员资格或控制器。FOLLOWER_A 冲突时，首次移动及移出地图可影响另一行动者，而 Bowie 仍克隆 physical 1。这种静态替代状态既不证明表现成功，也不证明自然观察到的缺陷。

后缀动作先将各列出随从移向原始 physical 0，距离 0，再从移向 physical 1 的指令之后进入 eas_452BA。它最终仍克隆 **physical 1**。在全新且一致的用例中，该槽位刚被移出地图；随从接近主实体，然后复制该 Caravan 状态，同时保留各自精灵身份。当前物理列表不按 HP 筛选。

ApplyActscriptToFollowers 对后缀每个字节安装一个指针，直到 255。WaitForFollowersStopped 使用相同后缀：空时立即返回；否则等待一个 VInt，找到**最后列出的行动者**，并在 VInt 后仅轮询其空闲指针。它不汇合所有行动者。上表的三个映射行动者等待与这个末位行动者等待相互独立。SPRITE_SIZE 是共享字状态，resize 则为每个行动者的位；这些请求不能证明可见动画同时发生。零速度、缺失服务、格式错误列表或部分前缀都可能阻止最终模式写入。

只有这些返回之后，mode 1 才成为状态。返回 ExplorationLoop 且待处理值为零时，WaitForEvent 可在物理实体 0 安装其控制器；中间出现的事件优先。没有地图重新加载或原子回滚。F65、随从标志、成员资格及已保存 RAFT_MAP/X/Y 均不变。

## Mode 1 旅行与登陆决定

**已确认：**mode 1 安装 eas_Raft：FLAGS_A bits 7/6/5、速度 32、加速因子 0/0 及加速／减速操作数 -2/ON，随后进入地图图块复制检查／opcode 7／目标等待循环。上游“Uncollidable”宏名不禁用 bit 5 阻挡分支。图块复制处理在未淡入淡出时识别当前布局 `$0800/$0C00` 标记；它不提供重建地形。

Opcode 7 以未缩小的 layer 1 边界采样 PLAYER_1_INPUT，按 Up/Down/Left/Right 顺序检查。符合条件的轴可组合；后续检查选择朝向。无符合条件方向时，opcode 7 让出执行而不移动。其 32 槽位阻挡扫描只使用 bit 7 行动者的**当前位置**，两个距离都须小于 `$100`；不同于步行，它不检查目标位置。阻挡可到达精灵更新而不产生新移动。未受阻时，控制器按顺序评估以下布局决定：

| 检查 | 后果 |
| --- | --- |
| 掩码后 `$0400` | 调用 OpenDoor，然后重新读取布局字并应用掩码。 |
| 掩码后 `$1000` | 调用 WarpIfSetAtPoint，无论是否匹配记录，随后进入移动尾部。 |
| 掩码后 `$3800` | 进入移动尾部。 |
| 否则，原始字按无符号比较小于 `$C000` | 写入事件 4，跳过移动尾部。 |
| 否则，原始字至少为 `$C000` | 跳过移动，不请求登陆。 |

OpenDoor 在战斗中直接返回。否则，匹配的踩踏事件记录复制／清除工作布局字、提交开门 SFX 并标记视野平面。无匹配时，该标记不会打开任何东西。控制器的重读保留这一区别。匹配的传送有独立事件使用方；Map68 城镇记录 `(57,24)`/`(48,30)` 及北／西／东边界是替代选择，不是行程。

移动尾部为非零轴设置速度与 travel 384，将瓦片偏移加到目标，并请求条件精灵／朝向更新。登陆跳过该尾部，保留目标位置，但可能转向尝试迈步的方向。Opcode 7 没有直接的 F65／解锁／当前战斗门槛、步行斜坡调整、zone6 生产方、MOVE_SFX 或 STEP_COUNTER 递减。Opcode 8 则识别 `$3C00` 并请求 5，用于实际木筏离乘。这些决定既不确立可达的渡河，也不确立自然登陆格。

## 登陆：当前列表变大与按标志重装

**已确认：**事件 4 在 MapEventType4 使物理实体 0 空闲并执行 `cs_45348` 之前被清除。随后将 `byte_45368` 应用于 FOLLOWERS_LIST，经同一个末位行动者辅助等待，运行 InitializeFollowerActscripts，**只有随后**才写入 PLAYER_TYPE 0。它既不保存木筏坐标，也不再次检查对齐、登陆地形、F65 或 HP。

| cs_45348 记录 | 有序工作 |
| --- | --- |
| 0 | 等待 FOLLOWER_A 的 eas_45360：克隆原始 physical 0 的位置／目标／行进／朝向，刷新，空闲。 |
| 1 | setSprite Bowie,Bowie：解析映射的 Bowie，取得其当前己方角色 mapsprite，写入，等待一个 VInt 并刷新。 |
| 2 | 等待 Bowie 的 eas_4536C：沿自身当前朝向请求一个瓦片，resize ON、传送 SFX、尺寸 12/14/16/18/20/22，每次刷新／wait 1，resize OFF、size 16／刷新、等待自身目标、空闲。 |
| 3 | End 与普通脚本返回。 |

精灵命令没有按存活状态跳过的守卫。第二个 Bowie 是己方角色身份，不是字面 sprite0：职业影响 mapsprite，战斗外 HP 0 选择蓝色火焰。它既不治疗，也不证明成员资格。变大相对于当前位置／朝向，不是总向 Down、经过碰撞检查的移动或隐式克隆。后缀动作先克隆原始 physical 1，再进入该变大程序体。

映射一致时，record 0 已将独立 Caravan 放在主实体的登陆原点／朝向。Bowie 和后缀行动者从那里向前移动一个瓦片；Caravan 留在原处，直到后续跟随动作移动它。FOLLOWER_A 冲突不会重定向后缀的 physical 1 克隆。当前列表选择谁变大；没有条目或查找被移除或重建。

InitializeFollowerActscripts 使用**不同的权威依据**：选择普通或 F65／大地图指针表，扫描当前标志记录，并从 1 开始写入连续物理槽位。它不遍历 FOLLOWERS_LIST、不解析逻辑查找、不声明／移动行动者，也不等待新安装动作。改变的标志或仅存在于当前列表的 Rick 可与刚变大的列表不一致。这是动作重装，不是列表重建或身份规范化。

Mode 0 加上返回且待处理值为零，允许物理实体 0 普通步行。此前精灵／位置可能已改变，而停滞前缀仍使 mode 1 保持不变。登陆本身不产生区域请求、STEP_COUNTER 递减或战斗选择；下一次符合条件的步行尝试独立。

## 步行独立的 Battle19 请求

**已确认：**步行 opcode 2 后面的掩码 `$1400` 分支写入事件 6，并将区域字 `(XDEST + selected X offset)/384` 写入 parameter 1、`(YDEST + selected Y offset)/384` 写入 parameter 3。这先于原始布局阻挡。获准的移动尾部提交 MOVE_SFX，并只在有符号 STEP_COUNTER 为正时递减；受阻尾部可留下区域请求，而不移动／递减。此前的登乘分支绕过两者。Opcode 7 没有等效生产方，但仍可能存在独立传入或被替换的待处理状态。

事件 6 使用方安装物理实体 0 的 eas_Init，并将两个字传给 RunMapSetupZoneEvent。Map68 唯一区域记录是默认标记 `$FD` → Map68_DefaultZoneEvent：**一旦进入**便绕过坐标匹配，而非每帧或每个瓦片都运行。其完整三操作回调加载 battle 19、调用 CheckRandomBattle 并返回。包装器关闭立绘／文本、等待一个 VInt，并等待物理实体 0 当前位置等于目标。Init 重置运动配置与精灵设置，随后空闲；它不清零位置／目标或既有行进。

| 传入的回调状态 | CheckRandomBattle 后果 |
| --- | --- |
| F519 未设置 | 立即请求，与计数器无关；不使用 RNG。 |
| F519 已设置，计数器非零 | 无请求／RNG；辅助保持计数器不变，包括负值。 |
| F519 已设置，计数器为 0，首次 RNG range 8 返回 0 | 请求；无第二次 RNG。 |
| F519 已设置，计数器为 0，首次 RNG 非零 | 无请求；第二次 RNG range 4 加 2，成为计数器 2–5。 |

GenerateRandomNumber 以乘 13／加 7 更新 16-bit 种子，再缩放到范围。这是算术，不是实测遭遇概率，也不假定种子独立／均匀。请求路径设置 F419，在 MAP_EVENT_TYPE 写入长字 `$000100FF`，写入计数器 30000，等待视野滚动、提交 SFX_BOOST 并运行闪光脚本。它不设置 F519，也不调用 CheckBattle/BattleLoop。闪光 opcode `$41` operand 30 右移 3 位，其 DBF 循环调用 LaunchFading 四次，然后调用 DuplicatePalettes；淡入淡出等待使用 VInt。声音／闪光请求仍依赖服务。

### 打包重新加载通过残留参数改变坐标

**已确认：**MAP_EVENT_TYPE 是 `$FFA84A` 处的字；参数字节 1–5 随后位于 `$FFA84C..$FFA850`。长字写入事件 1、parameter 1=0／无滚动及 parameter 2=255/MAP_CURRENT。它**不**初始化 parameters 3/4/5 或 WARP_SFX。一个可替换待处理槽位不是队列或原子准入。

在未修改的选定步行区域链中，若为普通非负字节坐标，区域 Y 字保留在 parameters 3/4：byte 3=0，byte 4=Y。因此无滚动使用方替换为 **X 0 与区域 Y**，使用残留 parameter 5 朝向，并提交／清除残留 WARP_SFX。这不是保留位置的重新加载。其他传入字节或中间写入会改变元组；负坐标字节则保留采样的当前轴。旧区域 X 字被完全覆盖。

回调／包装器返回后，待处理状态优先于步行控制安装。事件 1 使用方清除它、使 main 0 空闲、展开探索调用栈，并带这些坐标／朝向返回 map 255。SwitchMap 无 255 记录；CheckBattle 通过 CURRENT_MAP 68 解析它。F419 仍设置时，row19 通配选择 Battle19 并写入区域 `(5,19)`、大小 48x32，与残留 X/Y 无关。F519 已设置时清除 F419，但不取消此选定调用；未设置时使 F419 保持设置。不增加运输／F65／HP／Sword 谓词。停止于 MainLoop 选定的 BattleLoop 交接，早于 Battle19 内容／结果。

被替换请求或被清除的 F419 使准入有条件。无战斗的 map 255 返回则使用既有实体探索：更新主实体属性／木筏声明、加载／初始化，不重新配置实体，也不清除临时标志。它不是全新 Map68 实体生成。独立传入 F419 也可在传入 mode 1 的后续无滚动返回中准入 Battle19，包括最初的神殿出口；没有区域生产方不等于禁止所有战斗进入。

## 证据追踪与审查边界

**已确认的溯源：**已接受 #852/PR854 绑定 SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6` 及 USA 零售 ROM SHA-256 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。[研究复现所有者（英文）](../../../research/caravan-boarding-and-southwest-parmecia-travel.md#reproduction-and-remaining-limits)保留完整 vehiclefunctions／事件 2／事件 4／动作尾部、opcodes 2/7、查找／列表／随从及无滚动／区域／随机战斗／闪光／MainLoop／CheckBattle 关联。本综合文档使用该已接受审查，不重新调查源码，也不从精简测试夹具总数推断完整动作。

| 说明的边界 | 合同与确切可执行所有者 | 测试夹具未确立的内容 |
| --- | --- | --- |
| 全新实体生成／配置／初始化 | [地图探索](../contracts/map-exploration.md)、[配置](../contracts/map-setup-data.md)、[实体](../contracts/map-entity-data.md)；[初始化](../../../../tests/fixtures/h2/map-init-static-v1.json)中的 `sf2-map-init-static-v1`、[配置](../../../../tests/fixtures/h2/map-setup-static-v1.json)中的 `sf2-map-setup-static-v1`、[实体](../../../../tests/fixtures/h2/map-entities-static-v1.json)中的 `sf2-map-entities-static-v1` | 共享实体生成／选择器结构不是自然 Map68 到达，也不是实际传入标志／查找／列表状态。选定表／返回关联使用已接受研究。 |
| 登乘／登陆与动作命令 | [地图探索](../contracts/map-exploration.md)；[脚本](../../../../tests/fixtures/h2/map-script-engine-static-v1.json)中的 `sf2-map-script-engine-static-v1`，尤其 entityActionBridgeCommandFacts/entityCloneCommandFacts 及放置／生命周期事实 | 公开摘要／顺序／哈希／共享处理器不序列化完整 programCorpus。保留的 cs_45284 records0–4 与 cs_45348 records0–3 是九条地图命令；紧邻动作体是独立的已接受关联。未提供组合载具运行时证据。 |
| Map68 默认回调 | [地图探索](../contracts/map-exploration.md)；`sf2-map-events-static-v1`、[索引](../../../../tests/fixtures/h2/map-events-static-v1.json)与[区域分片](../../../../tests/fixtures/h2/map-events/zone-programs.json)，expected.zoneTargetPrograms canonicalSymbol Map68_DefaultZoneEvent | programOrder 125 的恰好三项操作，加上已接受默认记录，不确立步行地形或每帧回调执行。 |
| 首次／重复随机请求 | [地图探索](../contracts/map-exploration.md)；[随机状态](../../../../tests/fixtures/h2/map-event-random-battle-state-static-v1.json)中的 `sf2-map-event-random-battle-state-static-v1`、eventRandomBattleState.callerSites.map68-default-battle19 及 requestWriteSequence | Caller 19 及 34 条源码语句／35 条 H1 记录保留局部结构，不代表概率、闪光完成、残留字节值或下游运行时准入。 |
| 请求替换／消费 | [地图探索](../contracts/map-exploration.md)、[探索流程](../contracts/exploration-control-flow.md)；[写入](../../../../tests/fixtures/h2/map-event-request-state-static-v1.json)中的 `sf2-map-event-request-state-static-v1`、[消费](../../../../tests/fixtures/h2/map-event-request-consumption-static-v1.json)中的 `sf2-map-event-request-consumption-static-v1` | eventRequestState 写入方／到达定义与 eventRequestConsumption 轮询／清除／分发事实不是实际读取时状态或生产方至使用方运行时回执。打包字节使用已接受的直接使用方关联。 |
| MainLoop 准入 | [探索流程](../contracts/exploration-control-flow.md)、[进入路由](../contracts/map-entry-routing-state.md)；[流程](../../../../tests/fixtures/h2/gameflow-core-static-v1.json)中的 `sf2-gameflow-core-static-v1`、[地图](../../../../tests/fixtures/h2/common-maps-static-v1.json)中的 `sf2-common-maps-static-v1` | 仅共享调用／选择／区域写入／完成顺序，既不提供私有 row19，也不代表战斗已开始。选定记录／调用方关联来自已接受研究，止于交接。 |

Schema／使用方配对为[初始化 schema](../../../../schemas/h2-map-init-static-fixture.schema.json)／[map_init.py](../../../../src/sf2tool/h2/map_init.py)、[配置 schema](../../../../schemas/h2-map-setup-static-fixture.schema.json)／[map_setup.py](../../../../src/sf2tool/h2/map_setup.py)、[实体 schema](../../../../schemas/h2-map-entities-static-fixture.schema.json)／[map_entities.py](../../../../src/sf2tool/h2/map_entities.py)、[脚本 schema](../../../../schemas/h2/map-script-engine-fixture.schema.json)及[实体组件](../../../../schemas/h2/map-script-engine-entity.schema.json)／[map_script_engine.py](../../../../src/sf2tool/h2/map_script_engine.py)与 [schema 所有权](../../../../src/sf2tool/h2/map_script_schema.py)、[区域 schema](../../../../schemas/h2/map-events-zone-programs.schema.json)／[map_events.py](../../../../src/sf2tool/h2/map_events.py)与[分片使用方](../../../../src/sf2tool/h2/map_events_fixture.py)、[随机 schema](../../../../schemas/h2/map-event-random-battle-state-static-fixture.schema.json)／[map_event_random_battle_state.py](../../../../src/sf2tool/h2/map_event_random_battle_state.py)、[写入 schema](../../../../schemas/h2/map-event-request-state-static-fixture.schema.json)／[map_event_request_state.py](../../../../src/sf2tool/h2/map_event_request_state.py)、[消费 schema](../../../../schemas/h2/map-event-request-consumption-static-fixture.schema.json)／[map_event_request_consumption.py](../../../../src/sf2tool/h2/map_event_request_consumption.py)、[流程 schema](../../../../schemas/h2-gameflow-core-static-fixture.schema.json)／[gameflow.py](../../../../src/sf2tool/h2/gameflow.py)，以及[地图 schema](../../../../schemas/h2-common-maps-static-fixture.schema.json)／[maps.py](../../../../src/sf2tool/h2/maps.py)。既有复现命令为 `uv run sf2 h2` 加 `map-init`、`map-setup`、`map-entities`、`map-script-engine`、`map-events`、`map-event-random-battle-state`、`map-event-request-state`、`map-event-request-consumption`、`gameflow-core` 和 `common-maps`。受控复现前，在同一进程加载被忽略的工作树配置。此处指明所有者，不是新增纯文字运行。

综合前审查将已接受研究与更严格的合同／测试夹具边界对照，包括摘要与保留语料、源码与 H1 单位、模式／动作名、物理／逻辑选择器、当前列表等待与标志记录重装、部分服务及待处理优先级。F65／模式不匹配、F74/F77/F83 查找覆盖、HP 0／缺失行动者、仅在当前列表的 Rick／空后缀、零速度、改变的标志、残留参数、F419/F519／计数器／种子变体及被替换事件，是用于辨别边界的静态用例，不是已执行的运行时矩阵。不要求修复 Layer A。较早的通用运行时问题队列不会抹去这些已接受的选定静态使用方关联，也不会成为自动获取工作。

保留 [PR854 原始证据](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/854#issuecomment-6161835568)、[变基后最终交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/854#issuecomment-6161892874)与[独立验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/854#issuecomment-6161933230)：保留的 18,285,979-byte 比较、选定 9/3/34 程序／回调／辅助关联、已修正／失败检查、原始／变基后身份及实际 CI。它们保留链接的 PR849/846/842 历史。已接受比较不会仅因文字说明而重放；不弱化测试夹具／schema／黄金夹具，也不将聚合事实提升为组合运行时观察。

**未知：**自然到达／瓦片／门／传送／登陆可达性及实际输入；传入标志／计数器／种子、HP／职业、行动者映射／列表及残留参数；运动／尺寸调整／精灵／声音／淡入淡出／视野服务完成；能响应的步行／载具控制及持久性。它们继续按 [ADR0014](../../../decisions/0014-static-first-runtime-evidence-after-map3-battle01.md) 与 [ADR0016](../../../decisions/0016-remake-start-evidence-deferral.md) 延后处理。本指南不选择重制版规则、新验证机制或自动运行时工作，也不使用未接受的后续发现。
