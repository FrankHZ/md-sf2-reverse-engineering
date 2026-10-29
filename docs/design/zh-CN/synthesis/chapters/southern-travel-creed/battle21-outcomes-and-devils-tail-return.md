# Battle21 结果与 Devil's Tail 返回

> 本文是[规范英文原文](../../../../synthesis/chapters/southern-travel-creed/battle21-outcomes-and-devils-tail-return.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。

Battle21 可在敌方首领死亡、Bowie 存活后结束，无需军团逐个击杀全部十二名初始敌人。
首次符合条件的胜利，其战后场景从交战转到 Peter 对移动中同伴的疑问，以及他们要见 Mr. Creed 的意图。
场景完成后请求返回 Devil's Tail `(26,23)`、Down。进入宅邸仍是独立步骤；重复胜利跳过对话，使用当前军团成员所播种的返回值。

**已确认：**已接受静态证据确立下述结果检查、完整选定场景及条件返回使用方。
**推断：**同伴回应意在表现 Oddler 的对话部分。**未知：**自然结果、实际传入状态、服务完成、渲染场景、可用控制及存档持久性。
有关视力的对话不确立生理事实或后续设定，请求返回也不是已游玩行程。

本指南接续 [Battle21 开场与初始名册](battle21-opening-and-initial-roster.md)，
使用[已接受结果研究](../../../../../research/chapters/southern-travel-creed/battle21-outcomes-and-devils-tail-return.md)。
止于条件 Map67 探索/控制或待处理返回，或传入败北/逃脱目的地的继续。
宅邸初始化/室内、Battle22、后续招募/设定、伤害/AI/掉落/地形重建及重制实现不在范围内。
Fnnn 表示标志 nnn；命令索引从零开始。坐标、速度及等待是源码操作数。

## 首领清理可结束遭遇

**已确认：**Battle21 的首领存在表槽位为 -1。单次行动后，defeated hook 首先要求 Bowie HP 非零、enemy 128 HP 为零。
F521 未设置时调用 Battle21 空 defeated 程序 `ms_Empty`；F521 已设置时绕过程序。两条获准路径都到达首领尾部。
尾部将 128–159 中每个非零 HP 敌人追加到死亡工作列表，维护长度及终止符；既不测试放置 X，也不将其 HP 清零。

后续 killed-combatant 使用方执行死亡记账/表现工作，将这些战斗员 X/Y 移至 -1、清除状态、更新属性值并将映射行动者移出地图。
计数随后因放置 X 为负而排除他们，即使 HP 仍非零。这不同于写入 HP 零的 debug kill-all 路径。
因此在一致状态下，Bowie 存活时击杀 leader 128，可不逐个击败其他敌人而达到胜利；也不要求 Bowie 亲自给予首领致命一击。
此处不选择 Battle21 分阶段位置覆盖。

双方计数均检查放置 X 和正 HP：30 个己方槽位、32 个敌人槽位。Bowie HP 为零强制己方结果为零。
每次结果检查均先测试零己方，再测试零敌人，因此**双方为零判为败北**。Bowie 死亡还会阻止首领清理 hook。

完整行动后顺序为 defeated hook → killed pass → 有序计数。双方仍非零时运行单次行动后效果，再执行 killed pass 和相同有序计数。
此时没有第二次 defeated hook。首领在单次行动后效果中 HP 降至零，本身不会在第二次检查调用首领尾部。
后续执行及传入状态会影响结果，先前 hook/死亡服务必须返回才能到达计数。

完整区域脚本表只有 battles 32/40 的条目；Battle21 不运行区域过场便返回。
这不意味着其 AI 区域未激活，也不重建其交战行为。

## 首次与重复胜利返回不同

**已确认：**胜利路径首先遍历全部 30 槽位，将存活角色及不死 Peter 7/Lemon 28 治疗至最大 HP/MP，
仅保留已有 STUN/POISON/CURSE 位并更新属性值。不使任何人加入，也不复活其他死亡角色。随后在检查 F521 前播种共享返回参数：

- 地图为 CURRENT_MAP；X/Y 是首个出战成员的战斗员坐标加 BATTLE_AREA_X/Y，使用字节算术。
- 朝向来自该成员映射的战斗实体；parameter1 清零。
- UpdateForce 在读取首个出战字节前，按己方 ID 升序重建已加入/出战列表。这既不是查找首个存活者，也不是无条件选择 Bowie；没有空列表修复。

一致 Battle21 入场中，地图为 67，源码区域原点 `(21,0)`、大小 32x42。
播种值将该原点加到当前战斗员坐标，不重用 Fairy Cave 抵达或初始放置。改变出战队伍、位置、朝向查找或已保存几何均可改变它。

| 结果路径 | 有序后果 |
| --- | --- |
| 首次胜利，F521 未设置 | 治疗并播种；运行全部 abcs_battle21，包括传送覆盖；共享 JoinForce(0)；清除 F421；设置 F521；读取当前返回字节；返回 d4=1。 |
| 重复胜利，F521 已设置 | 治疗并播种；跳过整个战后场景及其角色群/文本/移动/跟随/传送；共享 JoinForce(0) 及相同完成写入/读取/结果。除非其他写入方替换，播种值保留。 |

F421 是准入/解锁状态，F521 是完成及战后场景门槛，F471 是独立开场/start 标志。
F471 既不控制此战后场景，也不在胜利时改变；F471 未设置/F521 未设置仍选择它。F521 已设置时无论 F471 如何都跳过。
这两个标志都不选择替代 Map67 配置。

共享战后尾部 slot 21 为 0：JoinForce 接收 Bowie，不是新具名招募者。
它设置已加入 F0、重建军团列表，仅在结果出战人数小于 12 时激活 F32。满队且 F32 未设置会让 Bowie 留在后备；已设置的 F32 不因容量而清除。
不追加重复成员、不挤出已有成员，JoinBattleParty 也不立即第二次重建列表。
该调用无加入文本/音乐、治疗或职业/装备/位置变化，不能修复此前由哪名成员提供播种值。
战后程序不招募 Peter 或 B，军团成员资格也不选择探索随从。

## 场景使用战斗裁剪，返回使用完整地图

**已确认：**战后包装器保持 CURRENT_BATTLE=21。其 loadMapFadeIn 用摄像机 `(4,18)` 重载 Map67，
该战斗值仍令加载器将传入矩形复制到工作原点 `(0,0)`。战斗视野使用终点为 `(31,41)*384` 的 layer-1 边界，而不是普通完整地图边界。
加载相同地图编号不代表坐标域或实体群相同。

| 场景操作数 | 裁剪工作位置 | 在一致区域 `(21,0)` 下对应源码地图位置 |
| --- | --- | --- |
| 主 Bowie | `(10,22)`，Left | `(31,22)` |
| 固定 Peter | `(11,23)`，Left | `(32,23)` |
| B setPos | `(11,21)`，Left | `(32,21)` |
| A setPos | `(11,22)`，Left | `(32,22)` |

setPos 直接写入 tile×384 当前/目的地坐标和朝向，不加减战斗原点。
摄像机 `(4,18)` 被压缩/乘以 3，再以图块单位×128 使用；它是视野操作数，不是玩家抵达。
内联移动相对于映射行动者当前坐标。上述源码对应关系解释存储，不证明地形/通行或渲染视野。

最终传送将字面 Map67 `(26,23)`、Down 写入共享请求槽位。它不移动场景行动者、不加 21，也不清除 CURRENT_BATTLE。
只有完整场景和包装器返回后，胜利才返回当前参数。后续全新 ExplorationLoop 在加载 Map67 前设置 CURRENT_BATTLE=255，
所以普通抵达是完整地图 `(26,23)`，**不是**裁剪 26 或源码 X47。场景和传送不重新计算 BATTLE_AREA 字段；探索加载器此时忽略它们。

## 角色群取决于随从映射

**已确认：**abcs_battle21 的 `ce_4B948` 包含上述主头部、一个 eas_Init 固定 Peter 行及终止符，没有固定 A、B 或敌人行。
加载器清除旧实体及查找字节，声明按标志选择的随从，处理固定 Peter，再以 eas_Idle 在 physical 0 声明 Bowie 并加载精灵。
它不运行普通 Map67 配置/init。

A 和 B 指 logical FOLLOWER_A 30 和 FOLLOWER_B 31。在大地图 Map67，F65 选择 Caravan/A，F67 选择 Peter，F68 选择 Oddler/B。
仅这些选择器已设置时，他们占据 physical 1/2/3，Peter 非零查找跳过固定行。没有 F67 时，固定行仍在选定随从后声明 Peter。
重复检查测试查找，不测试 HP、已加入状态或出战队伍。F66 添加 Sarah/Chester；其他选择器可添加行动者并替换查找。
特别是 F70 较后的 Astral/B 行可覆盖 Oddler 的 B 查找；较后 A 行可覆盖 Caravan。
F421/F471/F521 和出战队伍成员资格均不确立这些随从标志。

已清除的缺失查找为 0。GetEntityAddressFromCharacter 使用掩码后的逻辑 ID 查找（enemy IDs 减 96），无缺失行动者守卫；
因此缺失 A/B 别名到 physical 0。A 的 setPos 或 B 的动作/文本可影响 Bowie。映射 255 不是经过验证的缺失哨兵值。
固定 Peter 行为 logical 7 提供场景回退，但 A/B 没有回退。

真实己方 IDs 0–29 在 setPos/setFacing 有 HP 守卫；零 HP Bowie/Peter 跳过这些写入。
A/B 绕过该守卫，动作/文本/nod 也不继承它。CURRENT_BATTLE=21 时，即使 HP 零仍按职业选择己方精灵；声明不等于复活。
一致普通胜利具有存活 Bowie 和先前已治疗 Peter，但直接/传入改变状态不会被场景规范化。
逻辑说话者或随从名称均不保证显示行动者的身份。

## Peter 的疑问与完整战后场景

**已确认：**全部 33 个顶层命令、自定义速度程序体、两个内联移动及实体表属于已接受研究。完整程序如下：

| 命令 | 场景推进 |
| --- | --- |
| 0–2 | 选择文本游标 2583；以摄像机 `(4,18)` 加载 Map67；重建 ce_4B948。 |
| 3–9 | 安装/等待 Bowie Init，再 Peter Init；设置 Peter `(11,23)` Left；安装/等待 B Init；设置 B `(11,21)` Left、A `(11,22)` Left；fadeInB。A 不获 Init/等待。 |
| 10–13 | 安装 B 自定义程序体：ac_setSpeed 28,28；ac_jump eas_Idle；ac_end。等待空闲；csWait 5；启动 eas_2xRightLeft，不等待空闲；csWait 120。 |
| 14–16 | B 内联 moveLeft 3、moveDown 1、moveLeft 1、endActions，带空闲等待；csWait 40；B 朝 Right。 |
| 17–22 | 六次 nextSingleText 在 B/Peter 间交替，文本 2583–2588。B 上操作数字节为 `$0`；Peter 为 `$C0`。 |
| 23–27 | Peter 内联 moveLeft 1/endActions，带空闲等待；Peter 朝 Up；以 `$C0` 说单文本 2589；Bowie 朝 Down；nod Bowie。 |
| 28–32 | 安装跟随 A→Bowie relative 2、Peter→A relative 1、B→A relative 3；传送 Map67 `(26,23)` Down；csc_end。 |

映射一致、放置获准、移动完成且无中途位移时，B 请求 `(11,21)`→`(8,21)`→`(8,22)`→`(7,22)`，Peter 请求 `(11,23)`→`(10,23)`。
setPos 改变当前/目的地坐标时，A 保留传入动作；没有命令冻结或等待 A。跟随可再次移动角色群。这些是条件目的地。

七个字面键 `0A17`–`0A1D` 包含完整交流：

| 十进制 / 十六进制 | 说话者查找 | 字面文本与控制符 |
| --- | --- | --- |
| 2583 /0A17 | B | `Hurry!  Let's go into{N}Creed's mansion.{W1}` |
| 2584 /0A18 | Peter | `Oddler, can you see?{W1}` |
| 2585 /0A19 | B | `No, why?{W1}` |
| 2586 /0A1A | Peter | `You're moving around as{N}if you can see.{W1}` |
| 2587 /0A1B | B | `Am I?  Maybe my other{N}senses have sharpened{N}since I lost my sight.{W1}` |
| 2588 /0A1C | Peter | `Oh, I see.{W1}` |
| 2589 /0A1D | Peter | `{LEADER}, let's go see{N}Mr. Creed!{W1}` |

**推断：**Peter 的称呼表明预期对话对象为 Oddler。**已确认：**命令指向当前 B 查找；标志改变后它未必解析到 Oddler。
B 的视力陈述仍是对话，没有独立生理结论、选择分支或显式招募。Bowie 点头是动画命令，不是同意或成员资格变更。
`{LEADER}` 重建军团列表，使用首个出战成员名称，而非固定 Bowie 字面值。`{N}` 为换行；`{W1}` 是输入等待服务，不证明已确认输入。

### 动作、立绘与等待仍须完成

**已确认：**eas_Init 设置零加速度、速度 32/32、关闭加速/减速、可阻挡/可碰撞设置、开启自动朝向、orientation Up、无浸没、尺寸 24、精灵更新及跳至空闲。
orientation 与 facing 相区分。B 自定义程序体随后写入速度 28/28；其内联移动前没有中间 Init。
eas_2xRightLeft 刷新 Right/Left/Right/Left，每次等待 30，随后进入空闲；既不移动行动者，也不还原速度。

csWait 5/120/40 提交 Sleep 计数。特别是 120 并非内联动作替换脚本前，四次转向等待的独立完成屏障。
动作助手安装指针，并在请求时轮询精确 eas_Idle，无 HP 守卫。
内联移动清除动作标志 5/6，编译带目的地等待的相对 tile×384 目的地，并附加跳至空闲。
标志 5 未设置绕过目的地冲突扫描。这些移动既不分发普通玩家区域/传送事件，也不证明自然穿行。
点头停止动画、sleep 10、变换/DMA 精灵、sleep 20、刷新、sleep 10，再还原动画。

每个 nextSingleText 从映射行动者当前精灵解析立绘/台词，等待滚动、显示/递增游标文本，再关闭立绘/文本并 sleep 10。
Peter 的 `$C0` 设置压缩位 15/14，被用作右侧及镜像立绘切换；B 的 `$0` 均不设置。
已有窗口和缺失立绘分支仍适用。仅说话者/修饰符不证明渲染立绘。
ExecuteMapScript 入场清除调试文本跳过；debug 加 P2 Start 可抑制普通文本显示/游标递增和 csWait，而自定义/内联动作及传送仍保留。

csc_end 时，非零对话窗口索引触发滚动等待，随后清除摄像机速度并还原寄存器。这不是全行动者屏障。
淡入淡出/加载/显示/VInt、移动、滚动、立绘/文本及输入服务必须返回才能执行后续命令。
停滞不会回滚先前 RAM 写入，也不确立场景成功完成。

### 返回时替换临时跟随

**已确认：**三个 followEntity 调用安装以下有序逻辑关系：

| 随从 → 目标 | 相对操作数 / 表偏移 |
| --- | --- |
| A 30 → Bowie 0 | 2 / `(-24,0)` |
| Peter 7 → A 30 | 1 / `(0,-24)` |
| B 31 → A 30 | 3 / `(0,24)` |

使用方把每个随从字的首字节交给存活守卫；30/7/31 的该字节均为零。
因此这些命令测试 **Bowie HP**，不是所命名随从的 HP。Bowie HP 零跳过对应调用。
否则 AddFollower 安装 42 字节 eas_Follower1 副本，带 physical 目标和偏移操作数，仅在 physical 随从尚不存在时追加至 EXPLORATION_ENTITIES。
固定回退 Peter 可在此添加。缺失 A 可令 Bowie 跟随自己，别名查找可替换先前动作。
没有别名拒绝、成员资格变化或持久选择器写入。

模板设置速度 32/32、零加速度，关闭加速/减速/可阻挡，开启不可碰撞位，再循环跟随/目的地等待。
执行时会替换 B 速度 28；安装不证明过渡前已执行。跟随使用方使用目标相对距离/朝向/目的地/布局：
间隔最多 `$1E0` 时保留目的地；否则尝试旋转偏移、零横向回退，若受阻则使用目标目的地。
相对偏移不是绝对落点，所有这些调用均不等待稳定。

## 待处理传送不是已完成胜利

**已确认：**csc07 向唯一共享槽位写入 MAP_EVENT_TYPE=1、parameter1=0 及字面 map/X/Y/facing 字节。
它覆盖先前请求，没有队列；不加载/淡入淡出地图、不清除 CURRENT_BATTLE、不写 F521，也不自行结束脚本。
先前胜利播种将 parameter1 清零，但未清除 MAP_EVENT_TYPE，因此重复胜利未必产生 type1 事件。

完整场景返回，或 F521 绕过之后，共享 JoinForce(0) 尾部返回。
之后 BattleLoop 才清除 F421、设置 F521，并把 parameters2–5 读入 d4=1 战斗结果。
它读取当前字节，不验证类型，也没有不可变传送副本。该读取前的替换可改变返回元组。
传送前停滞保留播种值或其他请求；传送后停滞可留下待处理事件，而无完成标志或返回结果。任一部分状态均不保证返回。

## 全新 Devil's Tail 实体群与条件控制

**已确认：**MainLoop 替代返回先调用 SwitchMap，再调用 ExplorationLoop，不调用 CheckBattle。
Map67 无按标志切换行，因此普通返回坐标/朝向保留。即使 CURRENT_MAP 已为 67，仍选择全新探索并替换战后场景实体群。

探索清除 MAP_EVENT_TYPE，从 STEP_COUNTER 减去 20000 并对负结果钳位，治疗符合条件己方角色并开始淡入淡出/视野变化。
传入 Map67 时赋值 CURRENT_MAP=67 和 CURRENT_BATTLE=255，加载瓦片集/等待淡入淡出、清除 VInts、在返回位置/朝向选择/初始化配置实体并加载精灵。
它清除临时标志 256–383、设置 F80、初始化精灵帧状态、选择视野目标 0、加载完整地图/视野、安装基础 VInts 并运行 init。
WaitForEvent 前的音乐/淡入淡出取决于调色板比较。pending 零选择 physical 0 进行视野跟随并安装当前模式控制动作；非零 pending 优先。

Map67 完整配置无标志替代。`ms_map67_Entities` 仅含终止符，`ms_map67_InitFunction` 仅含 rts。
F471/F521 既不选择另一角色群，也不选择另一场景；init 不修复缺失随从，不创建自动宅邸请求。
全新实体初始化丢弃场景动作/查找，在返回坐标周围重建选定随从，再以返回朝向在 physical 0 声明 Bowie。
仅 F65/F67/F68 时，Caravan/A、Peter、Oddler/B 分别获得新的 eas_Follower1、eas_OverworldFollower2 和 eas_OverworldFollower3 模板。
它们不是场景中的三个跟随副本。其他标志改变分配/模板顺序。

普通实体群没有固定 Peter 回退。F67 缺失会移除场景回退，即使 Peter 已加入/出战；说出 Oddler 名称不设置 F68。
职业/HP 仍影响己方精灵，CURRENT_BATTLE=255 此时允许零 HP 蓝焰分支。
已加入/出战和随从选择器、F421/F471/F521 在临时重置之外，但 RAM 保留不等于存档持久性。

BattleLoop 设置 PLAYER_TYPE=0，一致场景/返回不改变它；普通控制使用 eas_ControlledCharacter。
胜利/传送不改变 F64 或 RAFT_MAP/X/Y。F64 已设置且已保存 map67 时，独立木筏尾部可在以 `$7F` 掩码的已保存坐标声明 physical 31；
它不登乘、不加战斗原点，也不修复位置。独立传入 mode2 则使用既有 raft-main/standing-follower 尾部。
控制器选择器使用 mode0 步行、mode1 eas_Raft（实际 Caravan）、其他模式 eas_Caravan（实际木筏）；不验证任意模式。

| 传入变化或未完成工作 | 返回边界 |
| --- | --- |
| F521 已设置 | 跳过全部场景/传送；无替换时使用出战成员播种值。共享加入/完成仍运行。 |
| 改变成员资格、位置、朝向查找或已保存几何 | 播种值改变；完整完成的首次场景覆盖坐标/朝向，重复则不覆盖。无空/陈旧列表修复。 |
| 缺失/替换 A/B/Peter 选择器 | 场景中采用别名或不同身份；全新返回移除临时跟随和场景 Peter 回退。 |
| 死亡/错误映射行动者或服务停滞 | HP/裁剪/动作/等待规则仍生效；先前写入未必到达完成或控制。 |
| 战斗结果前请求被替换 | 最新参数字节决定目的地，不是不可变预期行程。 |
| 探索入场重置后出现新事件 | 待处理优先于安装控制器；rts init 本身不生成事件。 |

传入 map255 结果走探索既有实体群分支，不执行全新战斗赋值/实体配置/临时重置；它更新主属性/木筏，并经该分支加载/init。
这不是普通 Map67 胜利返回。其他替换目的地需要自身使用方，本指南不完成它们。

下一个独立边界是 Map67 门口 `(26,22)`、warpNoScroll，请求 Creed Mansion Map33 `(15,30)`、Up。
战后场景完整地图抵达为 `(26,23)`、Down。相邻本身不分发该门口，不证明通行/输入，也不进入宅邸。止于其配置/init 前。

## 败北、逃脱与挂起采取独立继续路径

**已确认：**普通败北更新解锁、提交力竭音乐/文本、将 Bowie HP 恢复至最大、以无符号数将金币除以二、选择 Egress 目的地并返回 d4=-1。
不选择 Battle04 特殊可败情况。获准 Egress/Angel Wing 则扣法术 MP/移除选定物品槽、提交文本/闪烁/关闭、更新解锁、退出单次行动栈帧、
选择 Egress 目的地并返回 d4=0。行动类型/掩码 ID 准入既不证明菜单可用，也不证明 MP/物品持有。
逃脱没有胜利场景/加入/完成，也没有败北金币惩罚。

两条路径的 UpdateBattleUnlockedFlag 在 F521 未设置时保留 F421，已设置时清除 F421。
两者都不设置 F521 或改变 F471。Battle21 无特殊 Egress 目的地：由传入 EGRESS_MAP 和 F399 已设置的存档点选择决定。
缺失行保留该地图，使用默认 `(1,1)`、Up。直接以 F399 未设置调用助手则使用初始 Map3 `(56,3)`、Down；
这不是设置 F399 的普通战斗入场。

F64 未设置则木筏坐标不变。F64 已设置选择完整重置表 Hassan9→Map69 `(28,55)`、Polca13/Bedoe10→Map72 `(43,48)`，否则 Map69 `(42,15)`；这些写入不登乘。
替代 MainLoop 返回跳过战斗准入，SwitchMap 可在 F640 下映射 3→4、F530 下 71→78、F999 下 58→59，保留坐标/朝向。
这里不选择特定最后存档点或 Devil's Tail 返回。止于传入目的地继续。

CURRENT_BATTLE=21 也接纳战场挂起提示。负结果返回菜单；接受则保存秒数、设置 F88、为当前槽位调用 SaveGame，再淡出/跳至 WitchSuspend。
debug 加 P1 Start 则在存档调用后清除 F88 并返回菜单。没有胜利结果或 F521 写入。
后续 F88 已设置入场安装 mode0/F399、还原秒数、清除 F88/AI 内存，LoadBattle 直接进入单次行动，
绕过全新地图/战斗赋值、before/start 场景、治疗和名册初始化。
传入已保存地图/战斗/区域/出战队伍须一致；这既不证明存储完成，也不证明后续加载成功。

## 证据审阅与剩余问题

**已确认溯源：**本指南使用 main `f43add920e4c905195f35cb6009d870433153f19` 上已接受研究，
源码为 [SF2DISASM master `c834c652b6862bc5679fd7f69a38a7093206efc6`](https://github.com/ShiningForceCentral/SF2DISASM/tree/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm)，
USA 零售 ROM SHA-256 为 `9ADF662D09881F58EC37D174AB01E87A7FCFB24700B5F84B26C0CD4F351509E9`。
[研究结果所有者](../../../../../research/chapters/southern-travel-creed/battle21-outcomes-and-devils-tail-return.md#actual-outcome-routing)
及[复现边界](../../../../../research/chapters/southern-travel-creed/battle21-outcomes-and-devils-tail-return.md#defeatescape-continuation-and-verification-limits)
负责精确源码路径、槽位/地址、完整选定命令/程序体/实体/文本及使用方。这里不声称重新调查原始源码、重新生成载荷/比较或采集运行时。

| 已审阅证据所有者 | 本指南保留的支持与限制 |
| --- | --- |
| 已接受结果研究及开场指南；前篇 Fairy Cave 返回 | 精确领队/空/战后/加入/区域选择、两次计数顺序、当前战斗裁剪生命周期、全部 33 场景命令/自定义/两个内联体/ce_4B948/七条字面文本、完整地图返回及传入失败继续。局部内容由已接受选定源码关联确立。 |
| `sf2-battle-cutscenes-static-v1`，[fixture](../../../../../../tests/fixtures/h2/battle-cutscenes-static-v1.json)，expected.cutsceneFacts；[路由合同](../../../contracts/battle/battle-cutscene-routing.md#战后路由与加入尾) | 共享 HP/完成标志门槛、工作列表追加和加入尾部。合同对精确路由目标/空执行及加入效果的未知项仍是汇总限制；已接受选定研究提供更窄 Battle21 和 JoinForce(0) 关联。 |
| `sf2-battle-loop-static-v1`，[fixture](../../../../../../tests/fixtures/h2/battle-loop-static-v1.json)，expected.lifecycleFacts；[生命周期合同](../../../contracts/battle/battle-control-lifecycle.md#动作后与回合后顺序) | 槽位数、放置/HP 计数、killed 变更及治疗支持共享机制。已接受精确研究提供双方为零优先级及播种/结果使用方；Battle01 观察不证明 Battle21 运行时结果。 |
| `sf2-common-stats-static-v1`，[fixture](../../../../../../tests/fixtures/h2/common-stats-static-v1.json)，expected.statsFacts | 已加入/出战标志范围和自动激活上下文；已接受选定加入/列表使用方确立精确容量/重建顺序。这些标志不选择场景随从。 |
| `sf2-map-init-static-v1`，[fixture](../../../../../../tests/fixtures/h2/map-init-static-v1.json)，expected dispatcher/entity/init facts | 共享配置/随从上下文。已接受研究提供完整选定 Map67 终止符/rts、全新实体群、模式/木筏/待处理/控制器和门口关联；仅公开汇总不选择自然行程。 |
| `sf2-map-script-engine-static-v1`，[fixture](../../../../../../tests/fixtures/h2/map-script-engine-static-v1.json)，expected.programSummary 和命令族事实 | 共享解释器/语料上下文。研究保留的 abcs_battle21 选项有 33 个命令；完整自定义/内联体/实体表/字面文本及直接使用方仍是各自负责的关联。 |

既有 fixture schema/溯源检查使用 [cutscene schema](../../../../../../schemas/h2-battle-cutscenes-static-fixture.schema.json)、
[loop schema](../../../../../../schemas/h2-battle-loop-static-fixture.schema.json)、[stats schema](../../../../../../schemas/h2/common-stats-fixture.schema.json)、
[map-init schema](../../../../../../schemas/h2-map-init-static-fixture.schema.json) 和 [script schema](../../../../../../schemas/h2/map-script-engine-fixture.schema.json)，
通过既有 [JSON 验证器](../../../../../../src/sf2tool/jsonio.py)。已审阅所属验证器为
[battle_cutscenes.py](../../../../../../src/sf2tool/h2/battle_cutscenes.py)、[battle_loop.py](../../../../../../src/sf2tool/h2/battle_loop.py)、
[stats.py](../../../../../../src/sf2tool/h2/stats.py)、[map_init.py](../../../../../../src/sf2tool/h2/map_init.py) 和 [map_script_engine.py](../../../../../../src/sf2tool/h2/map_script_engine.py)。
其 schema/源码/溯源/语义比较保留自身边界；形状验证不复现每项正文声明。
已接受研究保留未改变的 18,285,979 字节 map-init/script 对比较及选定直接源码审阅，本指南不重跑它们。

保留 [#888 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/888#issuecomment-6173000296)、
[变基交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/888#issuecomment-6173114839)
和 [PR890 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/890#issuecomment-6173127050)，
包括原始/最终身份/CI、0.102 秒保留比较、已解决检查失败和前序收据。
保留 [#887 原始交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/887#issuecomment-6172913854)、
[修正交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/887#issuecomment-6173044344)
和 [PR889 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/889#issuecomment-6173054505)：
其已接受生命周期摘要顺序为 before → reset/heal/ally then enemy initialization/AI clear/load → start。
所报陈旧摘要 P2 已解决；原始仅范围 CI 与后续 research-public CI 保留不同结果。
交接定位检查；原版事实由跟踪研究及已接受 Git 对象负责。本指南直接检查、已提交 planner 及最终 CI 属于 #891。

**未知：**自然胜利/败北/逃脱/挂起及路线可达；传入出战队伍/HP/职业/标志、行动者查找/几何/模式/木筏/存档点；
动作/文本/输入/淡入淡出/滚动/窗口服务完成；实际场景/立绘/声音、可用控制及存储持久性。
改变状态示例约束静态声明；它们不是已执行场景、自动运行时工作或后续设定证据。
本指南既不完成战役，也不完成设计 epic，止于宅邸初始化前。

后续见 [Creed Mansion 入场与首次 Goliath 阻拦](creed-mansion-entry-and-first-goliath-scene.md)，
了解独立门口准入和区域场景；本指南保留宅邸初始化前停止点。
