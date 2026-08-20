# 出战队伍与名册状态合同

- **已确认的原版结构与有界行为：** 十种源码命名的地图脚本命令形式、它们的物理流布局、命名处理器的分支/变更/调用顺序、源码位置语料、直接/有效调用方身份，以及到 common-stats 与随从所有者的溯源连接。
- **推断的原版行为：** 本合同中没有推断。
- **未知的原版行为：** 正常剧情可达性、名册/列表容量、玩家可见的名册/败北结果，以及物理/跨进程 SRAM 持久性。
- 重制状态：实现无关的 Phase 3 合同；尚未选择任何引擎。

> 本文件是 [`party-roster-state.md`](../../contracts/party-roster-state.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签按 R1 使用固定中文译法；源码标识符、fixture ID 与路径按 R2 原样保留。

## 导入边界

重制导入器必须把源码形式 `join`、`jumpIfDefeatedByLastAttack`、`jumpIfDead`、`allyDefeated`、`updateDefeatedAllies`、`reviveAlly`、`joinBatParty`、`joinForceAI`、`resetForceBattleStats` 与 `addNewFollower` 保留为不同的有序命令。它必须保留每个命令的原始字节宽度、操作数宽度与流偏移、源码程序身份、命令索引与原始操作数文本。它必须保留两个完整的 304 行程序总数语料，包括零行与两个零使用名册/败北形式；零源码使用次数并不确立运行时不可达性。

导入器必须分别保留宏与处理器标签。特别是，`jumpIfDefeatedByLastAttack` 是处理器 `csc0E_jumpIfForceMemberInList` 的源码宏标签；绝不得把它静默归一化为新的语义命令名。`jumpIfDead` 与 `csc0F_jumpIfCharacterDead` 同样是独立的源码身份。任何面向引擎的名称只能作为显式版本化的重制决定添加，而不能替代这些源码标签。

## 处理器边界

导入的表示必须保留以下源码已确认的指令结构：

- `join` 有一个字操作数。其段保留位清除/`bne` 音乐分支、对照解析出的 `COMBATANT_ENEMIES_START` 值的选择器测试、两次命名特殊调用，以及常规 `JoinForce`/`GetClass`/对话索引调用与变更顺序。
- `jumpIfDefeatedByLastAttack` 与 `jumpIfDead` 各有一个字后跟一个长字。它们的有条件 A6 转移与四字节跳过是显式的流结果，而非泛化的布尔结果。
- `allyDefeated` 有一个字，并在递增源码命名的列表长度之前存储其字节。
- `updateDefeatedAllies` 没有操作数。其 `cmpi.w #-1,d1; beq` 跳过意味着源码列表写入位于非相等直落路径上。重制版必须保留该静态分支事实，但绝不得从相邻源码注释推断用户可见的死亡定义。
- `reviveAlly` 有一个字。其相等路径递减源码命名的长度，而非相等路径复制一个字节并同时推进两个指针；本规则不提供任何容量或持久化策略。
- `joinBatParty` 有一个字。它保留在成员测试之前的初始源码 `-1` 写入 `DIALOGUE_NAME_INDEX_1`、`BATTLE_PARTY_MEMBERS_NUMBER` 读取、源码 `subq.w #2,d7` 指令、当前 HP 零分支，以及在 `LeaveBattleParty` 随后 `JoinBattleParty` 之前的后续替换写入。这些状态写入/调用顺序事实不定义容量或激活/死亡生命周期。
- `joinForceAI` 有两个字。其第二个字的 `bne` 极性、`AIBITFIELD_AI_CONTROLLED` 的清除/设置使用、仅在设置路径上的 `JoinForce` 调用，以及共同的 `SetActivationBitfield` 尾部是独立的顺序事实。重制版不得用其宏注释断言的 "on/off"（开启/关闭）解释替换源码宏标签。
- `resetForceBattleStats` 没有操作数，并保留确切的 `ResetAlliesBattleStats` 服务调用。
- `addNewFollower` 有一个字。它保留 `-1` 扫描哨兵值、`d1` 中最后观察到的字节、固定的 `$FFE8`/`0` `d2`/`d3` 源码参数，以及最终的 `AddFollower` 调用顺序；这些寄存器事实没有一个定义随从生命周期或可见效果。

导入的直接调用图必须分别保留指令目标与有效目标。跳转接口别名不会被抹除：`j_JoinForce` 仍是指令目标，而 `JoinForce` 是解析后的目标。到 `sf2-common-stats-static-v1` 的连接是 `code/common/stats/battleparty.asm` 及其 `JoinForce`/`UpdateForce` 标签的源码溯源；它不是把兄弟测试夹具数据复制进本合同的许可。激活出战队伍组还保留 `GetActivationBitfield`/`SetActivationBitfield` 所有者路径、`AddFollower` 所有者路径与 `ResetAlliesBattleStats` 所有者路径，仅作为溯源身份。

## 证据与运行时边界

证据日期：2026-08-12。

可执行证据是 `tests/fixtures/h2/map-script-engine-static-v1.json` 处、字段 `forceStateCommandFacts` 的测试夹具 ID `sf2-map-script-engine-static-v1`；其验证器是 `src/sf2tool/h2/map_script_engine.py`。它固定上游提交、US ROM 哈希、处理器地址、完整 304 程序源码位置/总数语料、段守卫、调用方映射与 common-stats 溯源身份。嵌套的 `forceStateCommandFacts.activePartyCommandFacts` 字段固定另外四种形式、它们的 29 个位置、源码所有者身份与它们自己的 304 行总数语料。

名册/败北矩阵是 `tests/fixtures/h3/force-state-roster-death-v1.json` 处、测试夹具 ID `sf2-force-state-roster-death-runtime-v1`；其验证器是 `src/sf2tool/h3/force_state_roster_death.py`。它确认固定的 14 个处理器案例矩阵：已加入成员的 absent/already-present、败北列表 empty/hit/miss、HP dead/live、败北追加、败北更新的 offscreen/onscreen，以及 revive 的 empty/hit-first/hit-middle/miss。它还确认已加入成员资格和 HP 位于原版 4,016 字节 `COMBATANT_DATA` 的 SaveGame/LoadGame 域内，而 `DEAD_COMBATANTS_LIST` 及其长度位于该域外。只有会变更的 absent-join 案例执行原版 SaveGame、对已加入标志字节的窄范围逆向毒化、原版 LoadGame 和原版 `CheckFlag`，并记录独立推导出的选定物理 SRAM 字节、校验和字节及 `SAVE_FLAGS` 已占用位。HP 案例是已保存域中的分支观察，而非合成的 Save/poison/Load 证据；列表案例是处理器局部的范围恢复事实，并非持久化主张。激活出战队伍矩阵是 `tests/fixtures/h3/force-state-active-party-v1.json` 处、测试夹具 ID `sf2-force-state-active-party-runtime-v1`；其验证器是 `src/sf2tool/h3/force_state_active_party.py`。它确认有界的处理器局部标志/列表时序、激活/加入状态、重置服务顺序与随从分配/列表效果；重制版仍必须通过
`force-state/active-party-ai-follower/normal-story-reachability`、
`force-state/active-party-ai-follower/save-load-capacity-lifecycle` 与
`force-state/active-party-ai-follower/player-visible-presentation`
显式定义正常剧情可达性、容量生命周期，以及玩家可见的呈现。

对于有界的 `updateDefeatedAllies` 探针，源码/H1 循环进行 32 次 `GetCombatantX` 检查。fixture 仅为恢复而快照一个已设定的列表字节以及其 32 个可能写入位置（33 字节）。这不是推导出的列表容量规则。

为了在此有界命令表面内的保真，保留可观察顺序而不是立即修复列表：`UpdateForce` 可能在成员标志已经改变时留下处理器局部的替换前出战队伍快照。保留零选择器不加入与非零 `JoinForce` 行为，在随后的属性更新之前应用重置状态掩码，并保留重复随从分配与动态行走参数写入，即使随从列表本身没有改变。这些不是关于存档/读档容量或玩家可见呈现的规则。
