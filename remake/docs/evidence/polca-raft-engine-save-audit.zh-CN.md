# Battle08–13、波尔卡木筏与存档兼容审计

审计对象为 [#1734](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1734) 合入后的 `52f6a2c3`，实现增量可用 `git diff 8ca3a914..52f6a2c3` 复核。**优先处理旧战斗档的升级交接**。同一份普通 Battle01 存档能在七人配置恢复，在十一人配置被拒。六战至首格水行的既有普通输入验收仍成立。本次读了留存的主场景投影，并在独立进程运行该版本的 Content、Application 和 Domain。未启动 Godot 或重打六战。

## 按影响排序

| 优先级、归属 | 触发、影响与复核 | 后续验收 |
| --- | --- | --- |
| **P1 · 旧战斗档不能 Continue**。Content/Persistence 与 Application/BattleSaveStory。**Confirmed** | #1688 的真实中文 Battle01，v2、第一回合、三名部署盟友及四名保留盟友。同一字节流经 `OpeningSaveJson.ReadRecord` 均成功。用 #1735 七人 world 恢复为 `SessionStarted`，换 #1738 十一人 world 得 `save-battle-retained-roster`。战斗 recipe 仍为 1，gameplay 仍为 4。`BattleSaveStory.Build` 要求部署盟友与保留盟友的并集等于**当前全包**盟友，场景档的补员逻辑没有进入战斗恢复。校验早已有之，#1734 扩编使已写出的普通战斗档失去升级路径。原槽字节仍在。 | 在下一次扩大名册前明确旧战斗档支持范围。若承接旧档，用该样本验证恢复同一 actor、queue/cursor、HP/装备、双 RNG、保留队伍及下一合法输入；只补可证明尚未加入的记录。若保留版本限制，Continue 应识别并说明所需的兼容配置，保全原槽。现有设计明确允许因内容变化拒绝，本项优先级来自玩家无法继续游戏。[战斗档合同](../application/battle-save-and-continue.md) · [恢复检查](../../src/Sf2.Remake.Application/Runtime/Persistence/BattleSaveStory.cs) |
| **P2 · 泊位错误仍显示可继续**。SF2 场景档策略。**Confirmed** | 取 `polca-raft`，保持玩家 Map72 (43,47)、F64、F10。只把 `data.raft.map` 改为 `map-13`，或只把 `data.raft.y` 改为 47。两份有效校验和的档都能恢复、再捕获并再次恢复。前者 Map72 没有槽31，后者槽31与玩家重叠。三份档向南调用 `MapEventDispatcher.Move` 都产生事件3，`RaftRuntime.Aligned` 只对原档为真。删掉 raft 或将 y 改为64则得到 `save-raft-berth`。普通 UI 写出的原档正确。 | `OpeningRestore.Build` 在通用恢复器中直接判断章节名 `polca-raft`，只要求泊位存在、图已知、坐标在 0–63。把这个存点所承诺的**可登乘状态**交给 `Sf2OpeningRestore` 验证。正常档应能登筏，两个反例应在选槽时明确拒绝；其他合法停泊点由各自合同决定。[通用恢复](../../src/Sf2.Remake.Application/Runtime/Persistence/OpeningRestore.cs) · [存点策略](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OpeningRestore.cs) |
| **P2 · 升级次序改变新成员属性**。SF2 场景档迁移。**Confirmed** | 同一 Map58 档（2,806 金，主 RNG 1507850002），不走一步。比较直接 1→4 与 1→2→3→4，每段 Restore、Capture、编解码后交给下一 world。最终只有 Allies 不同。直接/逐版所得 Gerhalt 基础攻击 35/36、防御 22/21；Luke HP 上限 28/29、防御 18/19、敏捷 22/23。`RestoreRoster` 每次从存档主 RNG 的副本开始，对本次新增记录顺次调用 `OpeningPartyInitialization.Initialize`。这保护了游戏 RNG，却让补员成长依赖升级次数。 | 若要求相同历史档产生相同新增队员，让初始化对升级路径稳定。用同一源档比较两条路径的全部新成员记录、原成员、双 RNG 和旗标；不得重算玩家已有属性。实际运行的中间配置见下表。[迁移](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OpeningRestore.cs) · [成长初始化](../../src/Sf2.Remake.Domain/Gameplay/Sf2/OpeningPartyInitialization.cs) |
| **P2 · Luke 入队后的敌方 AI 缺职业行**。SF2 Domain。**Confirmed**（受控输入） | `Sf2ClassRules.SourceClass(UnpromotedBirdman)` 为 6；`PhysicalTargetRules.ClassRank` 没有 6。对两名同为优先级 15 的候选分别给职业 0、6，调用 `PhysicalTargetRules.Select([new(1,15,0),new(1,15,6)], table)`，四种 table 均抛 `ai-target-class`。追击的 `AiMovementRules.PursueOrdered` 也消费此表。让 Luke 提前参战的 Battle13 受控构造在首个输入前同样失败。正常路线在战后才招募 Luke；六战普通验收未出现此错误。 | 在开放 Luke 的下一次战斗前，按固定源 `aipriority.asm` 四张职业顺序表绑定 BDMN，验证物理候选同分及追击类比较，保留其他职业顺序。职业判断属于 SF2 策略，不应增加 Battle13 特判。[职业映射](../../src/Sf2.Remake.Domain/Gameplay/Sf2/Sf2ClassRules.cs) · [目标选择](../../src/Sf2.Remake.Domain/Gameplay/Sf2/PhysicalTargetRules.cs) · [四张源表](https://github.com/ShiningForceCentral/SF2DISASM/blob/c834c652b6862bc5679fd7f69a38a7093206efc6/disasm/data/battles/global/aipriority.asm) |
| **P2 · 带筏战斗档会丢泊位**。Application / v2 战斗故事记录。**Confirmed**（受控组合） | 在下述 Battle13 首回合快照上仅给 story 加 F64 与 Map72 (43,48) 泊位，再走 v2 捕获、编解码、恢复。每步成功，恢复后 F64 仍在、Story.Raft 却为 null。`SavedBattleStory` 没有泊位，重建比较也只覆盖已记录字段。首格水行之前的自然六战尚未获筏。 | 在准入获得筏后的战斗存档前，保存并恢复泊位，验证返场后能找到同一实体；实现前，资格查询应拒绝会丢泊位的状态。保留无泊位的旧 v2 样本作对照。[战斗数据](../../src/Sf2.Remake.Application/Runtime/Persistence/BattleSaveData.cs) · [故事捕获](../../src/Sf2.Remake.Application/Runtime/Persistence/BattleSaveStory.cs) |

## 六战如何接入

**Confirmed**。生成器 `compile_routes` 从地图坐标表取触发轴、战斗原点，从战前、开战、战后和加入表取程序及成员；选择表限定本包的地图、遭遇和 egress。私有 reader 验证每战的布阵、地形、场景身份，`PrivateWorldEncounterReader` 合并遭遇并核对重复盟友的初始定义。`BattleEntry.First/Continue` 用实际位置、解锁/完成/介绍旗标分派程序，从现场名册取成员、装备和控制权，未部署的记录随 `FieldPlayerBinding` 保留。`BattleOutcomeRules.Check` 先判盟军败，再数在场存活敌人；有敌方首领的遭遇由内容绑定清场行为。`BattleOutcome` 将资源交给 `Sf2OutcomeReturnPolicy.Start/Finish` 与路由的战后程序。Godot 消费场景阶段、地图、队伍和旅行模式；抽查的代码没有用第几回合、哪个角色先动、实际胜利坐标来判胜。[选择表](../../content/private/pre-voyage-opening.json) · [生产](../../../src/sf2tool/remake_exploration_battles.py) · [读入](../../src/Sf2.Remake.Content/Scenarios/PrivateWorldEncounterReader.cs) · [准入](../../src/Sf2.Remake.Application/Runtime/Exploration/BattleEntry.cs) · [返场策略](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs)

下列普通输入结果来自各阶段的已交付证据。本次独立执行的存档比较列在下一节。

| 战斗 | 准入、胜利、败退及队伍变化 |
| --- | --- |
| 08 · Map72 | 船上 No 重问，Yes 经 caller 写 F703、设船的 egress、F408 入战。敌方槽128先倒仍需消灭残敌。败北或付费 Egress 回 Map14 (45,7)，F458 使重入跳过介绍。胜利清 F408、置 F508，经 Map73 建城末句复苏队伍，F705 后入城；七人含 Kiwi，无 Kiwi 全程未实走。[船与建城](../application/battle08-new-granseal.md) |
| 09 · Map69 | 一年后、Peter 城堡会见及出镇才到战场。胜利在 Map69 (1,1)，Jaha 中毒，3,693 金。败北按携带的 egress 回新格兰希尔。Peter 随行并由 AI 行动。[新城与 Ribble](../application/new-granseal-east-shrine.md) |
| 10 · Map70 | Ribble 市长允许离镇，May 可拒招；已有不带 May 入战与带 May 胜利两条证据。胜利队伍保留 Power Ring、死者与 5,033 金，败北回新格兰希尔。实际落点 (17,19) 能步行到洞窟，和东神殿入口分岸。神殿访问的单独样本改变了返场坐标，不能用 F77 作第十一战硬前置。[两条路线](../application/new-granseal-east-shrine.md) |
| 11 · Map48 | 洞口 (15,26) 触发。败退回携带的新格兰希尔，2,976 金；胜利回 (15,11)，6,233 金、五人死亡。波尔卡教堂按实际成员收费复活。[洞窟及教堂](../application/dark-caves-bedoe.md) |
| 12 · Map52 | 离开波尔卡上山；救仙女是独立分支。胜后脚本转到 (23,8) 朝左，F714、F712，Chester 和 Slade 仍死亡。独立败北回新格兰希尔，F512/F712/F714 均清。Peter 的 activation bit 2 到 Volcanon 的零 AI 操作数才清，国王同意后置 F799。F711 也可由救仙女写入。[山与 Bedoe](../application/dark-caves-bedoe.md) |
| 13 · Map13 | 从东口沿路至 x=24，对峙招 Gerhalt、F413 开战。胜利回 (16,12) 朝下，F513/F75，9,753 金，尚无筏。败回 Bedoe (27,35) 朝上。原版 `GetEgressPositionForBattle` 明确覆盖本战的 egress；生成器以 `fixedEgress` 表达，返场策略消费该位。败退能否再次自然回镇另无证据。[波尔卡](../application/polca-first-raft.md) · [原版分支](../../../docs/research/chapters/eastern-parmecia/battle13-outcomes-through-polca-raft-grant.md) |

**木筏与队伍。** 镇长前三次对话写临时 F260–262，第四次从 `cs_58856` 返回才写 F716/F64。生成器 `raft_placement` 从三条源写操作取地图、X、Y，reader 产生 `ParkRaft`，`ProgramRunner` 保存泊位，`MapTransfer` 在相应地图声明物理槽31。Oddler 两条答复通向共同离村结尾，F75 清，Luke 在 Map72 init 入队，随后 F416。原版 `JoinForce` 分别更新缓存与 F42；留存的离村及登筏前投影中 Luke 在 reserve，当前 Continue 重建后在 active。这个可见差异符合调用顺序。HP、装备及加入旗标可独立核对。[授筏](../../../docs/research/chapters/eastern-parmecia/battle13-outcomes-through-polca-raft-grant.md) · [加入与缓存](../../../docs/research/chapters/eastern-parmecia/polca-departure-through-luke-recruitment.md) · [操作数](../../../src/sf2tool/remake_exploration_scene.py)

**Confirmed**。步行时 `MapEventDispatcher.Move` 先检查阻挡，再以 F64 和候选格工作布局水面掩码产生事件3。`RaftRuntime.Aligned` 比对朝向和相邻槽31目的地；通过后登乘程序收拢随从、换 sprite 61、写 mode 2。按当前区域及实际水格移动。对照 #1738 的 `win04/{berth,boarded,first-step}.json` 的 `state`，玩家 (43,47)→(43,48)→(43,49)，mode 0→2→2，泊位仍在 (43,48)，三处均为 9,753 金、步数寄存器 26807、可输入。另有 (44,47) 未对齐的正常输入。槽31、模式2和水面掩码来自原版车辆机制，坐标来自赠筏内容。下船事件5仍明确返回 `raft-landing`，骑乘存档受 `OpeningSave.RequireStable` 拒绝。跨图水行、首次自然到达神殿对岸、实机精确时序和主观手感仍为 **Unknown**。[控制器源证据](../../../docs/research/chapters/southern-travel-creed/polca-raft-boarding-through-south-parmecia-entry.md) · [物理对齐](../../src/Sf2.Remake.Application/Runtime/Exploration/RaftRuntime.cs) · [车辆移动](../../src/Sf2.Remake.Application/Runtime/Exploration/CaravanRuntime.cs) · [投影](../../game/src/Exploration/ExplorationSessionView.cs)

**与此前审计对账。** #1728 提出的长等待与 Map72/08 准入已分别由 #1733 和 #1735 接通。F4 的返场资源保护、F7 的随从实体顺序和 F8 的区域音乐上下文仍有实际消费者。[#1728 审计](pre-voyage-readiness.zh-CN.md) · [原 F4/F7/F8](engine-script-boundary-review.md)

Battle10 女巫的 Power Ring（源 ID 19）在通用 `BattleInitializationRules.Validate` 与 `BattleEquipment.OrdinaryAiLoadout` 各加了同一个裸物品字许可；它让敌人携带可掉落的戒指，未授予通用戒指效果。**P3 · Confirmed**，新增同类合法携带物仍得改两处通用白名单。下次扩大携带物准入时由内容绑定已支持的惰性物品，在两种遭遇验证可携带、掉落且不获得未实现装备效果。[初始化](../../src/Sf2.Remake.Domain/Battles/Rules/BattleInitializationRules.cs) · [装备校验](../../src/Sf2.Remake.Domain/Battles/Rules/BattleEquipment.cs)

还有一处旧耦合：通用 `OutcomeReturnRules.Party` 与 SF2 的 `Heal` 都知道永生成员 7/28；本轮扩展了允许清除的睡眠/沉默位，成员号判断在增量前已存在。**P3 · Inferred**，若替换治疗策略，另一个合法角色的临时状态恢复可能被通用校验拒绝。需要第二种恢复语义时，让策略给出可恢复对象，通用层继续校验名册、资源和未授权字段；以不同成员身份的相同恢复动作验收。#1734 普通路线没有这一故障。[通用校验](../../src/Sf2.Remake.Application/Runtime/Exploration/IOutcomeReturnPolicy.cs)

## 存读档实测及最小复核

普通 Save 先冻结输入、提交音乐观察、查询资格，以会话 ID/修订号捕获；Continue 先用独立配置装入世界，由槽 reader 校验信封、SHA-256、版本/种类，再按 `SaveRecordRestore` 分派，成功才挂载会话。`LocalSaveStore` 写临时文件、回读并替换，语义不兼容的主档受保护。本次在内存中调用同一编解码和恢复链，直接检查 `SlotRead.CanRecover`。磁盘替换、备份修复按钮、窗口退出均未重演。[宿主](../../game/src/Entry/OpeningPersistenceHost.cs) · [槽](../../game/src/Persistence/LocalSaveStore.cs) · [编解码](../../src/Sf2.Remake.Content/Persistence/OpeningSaveJson.cs)

**场景档。** gameplay 4，磁盘 v1。字段 recipe revision 依次对应 7/9/10/11 条盟友记录；`OpeningLoadoutBindings.Bind` 按本包最大成员号选策略，`RestoreRoster` 核对连续前缀、其他兼容标识、缺员的旗标/名字，再初始化缺少的未加入者。下表每份均实际恢复到输入点、捕获、用原槽号编码/解码、再次恢复成功。比较所有原成员的完整记录，以及 `OpeningSaveData` 其余字段（含名字、装备物品字、死亡、activation、两路 RNG、金币、交易、旗标、步数、音乐、egress 和泊位）。

| 输入与副本定位 | 当前恢复结果 |
| --- | --- |
| r1 · #1735 使用的 Map58 `battle07-return`，槽1 | 7→11，2,806 金；原成员及其他进度相同。删除第七条记录或给缺少的 Luke 加 F10，各得 `save-roster`。 |
| r2 · #1736 的 `after-year`（槽2）、`after09`、`before-may`、`after10`、`shrine-complete`（槽1） | 9→11；分别为城堡、Map69、Ribble、Map70、神殿。原成员逐项相同。神殿重建后按地图 entryFlags 清临时 F256/F257；F77/F729 和资源保留。 |
| r3 · #1737 的 `after11`、`after12`、`before13`，槽1 | 10→11；洞窟、火山、东波尔卡分别保留 6,233 / 7,413 / 7,413 金。新增 Luke 未加入。 |
| r4 · #1738 的 `after13`、`before-raft`，槽1 | 11→11，两个样本再捕获的全部进度字段相等。前者无泊位，后者槽31在 Map72 (43,48)，两名死者和 Luke 的佩剑保留。 |

**重建边界。** 临时位按重入语义清除。用 `after13` 加 F260–262 后恢复再捕获，三个临时位均清；在三次接触后继续可能需要重新交谈，此处仅执行了恢复，未从该变体走对话。用 `after10` 改玩家位置 (63,63)/(0,0) 得 `map-entry-position`；(43,41) 可以恢复。存点验证可站立的战后锚点，不能证明从战斗胜利处走到了另一岸。[场景重建](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OpeningRestore.cs) · [生成的入图位](../../../src/sf2tool/remake_exploration_maps.py)

**战斗档。** 磁盘 v2，`battle-player-input`，battle recipe 1。除了表首真实旧档，分别在当前六条路由构造可保存的首回合移动选择，使用 `after-year` 恢复所得队员资源。按顺序启用成员前缀 7/8/9/9/9/10，供给当前战斗音乐和返场上下文。六次均完成 `BattleSave.Capture → WriteRecord → ReadRecord → Restore → CaptureBattleSave`，固定相同槽号、sequence 和时间后重编码字节相同，覆盖队列、游标、双 RNG、当场及保留成员；没有操作下一回合。这是受控战斗档检查，未取得这六战的历史普通战斗槽。首次构造漏带毒伤归属，09–13 均在捕获前报 `poison-death-context`；按 `BattleEntry.Continue` 补入 `BattleDeathCredit(null)` 后，提前启用全部成员的 13 又报上述 `ai-target-class`。最终六条使用该章已可加入的成员范围。场景对白、运动中、骑乘中及任意战斗帧均不在这两份存档合同内。[战斗捕获](../../src/Sf2.Remake.Application/Runtime/Persistence/BattleSave.cs) · [恢复与重建比较](../../src/Sf2.Remake.Application/Runtime/Persistence/BattleRestore.cs)

**独立复核步骤。** 在隔离进程用基线的程序集，按章准备的 world、六个 Battle01/静态输入及对应 battle scene 构造 `OpeningRestoreReader`，用 `Read()` 的 `ExplorationReadAccepted.Definition`。样本名均指已交付准备目录中的只读副本，私有字节不随报告发布。场景及旧战斗档的核心调用可直接放入引用 Content 的临时 C# 控制台：

```csharp
var record = OpeningSaveJson.ReadRecord(File.ReadAllBytes(sample), slot);
var result = SaveRecordRestore.Restore(definition, record, RuleCompositions.ForGame());
if (result is SessionStartFailed failed) { Console.WriteLine(failed.Failure.Code); return; }
var session = ((SessionStarted)result).Session;
var here = session.Current;
if (record is OpeningSaveRecord field) {
    var captured = session.CaptureSave(here.SessionId, here.Revision);
    var bytes = OpeningSaveJson.WriteRecord(field with { Data = captured.Data! });
    var next = OpeningSaveJson.ReadRecord(bytes, slot);
    Console.WriteLine(SaveRecordRestore.Restore(definition, next, RuleCompositions.ForGame()).GetType().Name);
}
```

- **历史与逐版对照**：对同一 #1688 战斗槽先选 #1735 的 `release04/world.json`、再选 #1738 的 `release/world.json` 与 `scene-raft.json`。Map58 逐版实验依次用 #1736 `release/world-next.json`、#1737 和 #1738 `release/world.json`；前两版同目录 `scene.json`。每步把再捕获的数据传给下一步。
- **受控六战**：沿 [BattleSaveProfileTests](../../tests/Sf2.Remake.Engine.Tests/BattleSaveProfileTests.cs) 的 `BattleAdvancer.Start` 至 `WithStory` 构造方式，选择 encounter 8–13、相应 route ordinal 0、地图 72/69/70/48/52/13；以上述场景档供给 carried，按实际入口补毒伤上下文。用本节的 v2 调用链替代旧测试的七战计数断言。无需 Godot。
- **拒绝分支**：以 `before-raft` 为底本，解信封 Base64，改变一个字段，以 UTF-8 JSON 重算 SHA-256，再封装。仅校验和错误 → `save-checksum`；formatVersion=99 → `save-version`；formatVersion=2、未知 kind → `save-kind`。三者在 codec 拒绝。field recipeRevision=5 → `save-incompatible`；从 activeAllies 移除首人 → `save-membership`；无泊位/越界/错图/陆地的差别见 P2。在有有效备份的 `SlotRead` 上实际查询：校验和错误允许恢复，未知版本/种类、`save-battle-retained-roster` 和 `save-raft-berth` 均不允许覆盖。
