# L2 · Grans 开篇与离岛章节总览

- **章节范围**：普通 New Game 玩家开局（格兰希尔 `Map3` 家里）至码头船长确认登船出航（`Map44` 码头 → 排队进入 `Map73`）。片头神庙演出（`--opening-act 1–4`）仅作背景；海上航行、登陆与 `Battle08` 见后续阅读[航行登陆至损坏船只控制](voyage-through-damaged-ship-control.md)与[损坏船只木板选择至 Battle08](../eastern-parmecia/damaged-ship-plank-through-battle08.md)。
- **本章目标与世界变化**：从格兰希尔的日常与首次古代之塔战斗（`Battle01`）→ 国王委任与西进调查耶尔、哈维尔小屋（`Battle02`–`03`）→ 加拉姆伏击、入狱、越狱与邪恶宝石（`Battle04`–`06`）→ 重返遭袭的格兰希尔、古代之塔营救（`Battle07`）、地震毁城与护送王室登船撤离。
- **证据与边界说明**：
  - **原版边界**：仅 `Map3` 至 `Battle01` 具有受限原版运行实测（[原版观察前沿（英文源）](../../../../synthesis/milestones/map3-battle01-readiness.md#accepted-original-frontier)）；`Battle02`–`07`、地震与出航在锁定源码（`SF2DISASM` `c834c652b6862bc5679fd7f69a38a7093206efc6`）与美版 ROM 中为**已确认静态**，原版自然完整步行、硬件时序与断电存档持久性仍为**未知**。
  - **重制边界**：重制版已有从 New Game 到损坏船只 `Map14` 首次控制的普通游玩与 v6 稳定存档观察（[渡海前战役（英文源）](../../../../../../remake/docs/application/pre-voyage-campaign.md)）。命名受控 Debug 起点（如 `slade-join`、`castle-quake`）仅验证对应过场交接，**不等于**自然完整通关，也不携带历史金币或经验。
  - **Flag 不唯一决定整个会话**：`Fnnn` 决定场景配置与门槛，但当前在场角色、随行关系、局部地形改写与背包状态还取决于地图进入方式与存档快照。
  - **发布边界**：本总览先在主仓完成规范化与验收，**尚未同步至线上 Wiki**。

## 按剧情时序排列的事件总表（玩家视角）

*注意：**可选不自动等于错过即永久失去**。下表中的可选地点与拾取只要未到达明确写出的状态切换或最终登船（`G20`），均可折返重访。*

### 第一阶段 · 格兰希尔开局、城堡觐见与 Battle01

| 编号 | 地点与玩家目标 | 触发 / 玩家选择 | 发生的变化与下一步去向 | 必经 / 可选 |
| --- | --- | --- | --- | --- |
| **G1** | **格兰希尔家与学校**（`Map3`）· 起床后前往阿斯特拉尔的课堂 | 普通 New Game 从家门出发，步行进入学校。 | 离家（`F601`）与课堂对话（`F602`）推进城镇状态；教堂与学校搜索（`F128`）此时即可使用。 | 必经 |
| **G2** | **格兰希尔信使**（`Map3`）· 回应城堡传唤 | 信使出现时选择 **Yes** 或 **No**。 | 选 **Yes** 会让**莎拉（Sarah）**与 **Chester** 正式入队并临时随行（`F66`），同时打开城堡大门（`F600`、`F603`）。选 **No** 不入队也不开门，再次对话仍可接受。 | 必经（拒绝可重试） |
| **G3** | **格兰希尔城堡**（`Map19`–`21`）· 见国王、阿斯特拉尔与塔楼卫兵 | 进城门（`F604`）、进国王寝室（`F605`）、回应阿斯特拉尔（`F607`/`F608`），再找西塔楼卫兵（`Map21`）。 | 拒绝阿斯特拉尔会让他留在原地等待重试；同意后他移动离场，塔楼卫兵开启通往古代之塔的路线并解锁 `Battle01`（`F401`）。 | 必经（拒绝可重试） |
| **G4** | **古代之塔**（`Map40`/`57`）· 迎战 `Battle01` | 携带 `F401` 解锁进入塔内前厅。 | **胜利**写入 `F501`、清除 `F401` 并返回 `Map57`，开启城堡驱魔阶段；**败北 / 撤离（EGRESS）**保留 `F401` 可再战。 | 必经 |
| **G5** | **驱魔、国王委任与贾哈入队**（`Map20`/`19`/`3`）· 回城领命并西出 | 回寝室触发驱魔（`F609`）、王座厅接下委任（`F610`，可重复询问），随后从格兰希尔西门离开。 | `F609` 开放城镇商店并移除码头路障（可提前访问 `Map44`）。西出时**贾哈（Jaha）**正式入队（清除莎拉/Chester 的 `F66` 随行）并解锁 `Battle02`（`F402`）。 | 必经 |

### 第二阶段 · 西部大地图、耶尔、哈维尔与 Battle02–04

| 编号 | 地点与玩家目标 | 触发 / 玩家选择 | 发生的变化与下一步去向 | 必经 / 可选 |
| --- | --- | --- | --- | --- |
| **G6** | **西路与南侧支线**（`Map66`/`18`/`41`/`42`）· 打通 `Battle02` 与可选探索 | 向西进入 `Map66` 触发 `Battle02`；可顺路探访未放下的加拉姆吊桥南岸（`Map18`）或**南侧 Devil's Head 与神殿下层**（`Map41`/`42`）。 | `Battle02` 胜利（`F502`）打通通往耶尔的道路（胜利若落在封闭格可用场地 EGRESS 脱困）。南侧神殿下层宝箱无剧情门槛，可直接拿到 `Power Water`（`F133`）。 | `Battle02` 必经；其余可选 |
| **G7** | **耶尔村**（`Map5`）· 打听哈维尔去向并由卡辛带路 | 进村与**卡辛（Kazin）**交谈（教堂与 `Healing Seed` `F134` 可选）。 | 写入**临时随行** `F72`（卡辛仅在地图跟随，**尚未正式入队参战**）。跳过耶尔**不会**阻断 `Battle03` 或哈维尔小屋。 | 可选（非 `Battle03`/小屋前置） |
| **G8** | **林间道**（`Map66`）· 迎战 `Battle03` | 经过耶尔北侧大地图区域（只要 `F503` 未完成，默认区域会独立解锁 `F403`，无论是否去过耶尔）。 | **胜利**（`F503`）打通前往哈维尔小屋的路线；**败北 / 撤离**退回当前存档点重试。 | 必经 |
| **G9** | **哈维尔小屋**（`Map43`）· 得知加拉姆异变并招募卡辛 | 进入 `Map43`（即使跳过耶尔或未设 `F72`，默认入场也会触发剧情）；可顺手查日记与宝箱（`F135`/`F136`）。 | **卡辛正式入队**（`F4`/`F36`），清除临时随行 `F72`，写入安静小屋 `F612` 与耶尔换人 `F650`，并解锁 `Battle04`（`F404`）。重访小屋跳过首场演出。 | 必经 |
| **G10** | **加拉姆伏击战**（`Map66`）· 结算 `Battle04` | 携带 `F404` 离开哈维尔一带。 | **特殊结果规则**：**首胜**触发雷蒙（Lemon）俘获剧情送入加拉姆地牢（`Map17`）；**普通败北**金币减半并恢复鲍伊 HP 后**同样**清除 `F404`、写入 `F504` 并进入地牢。只有**付费撤离 / 天使之翼**才保留 `F404` 供重战。 | 必经（胜负双线皆入狱） |

### 第三阶段 · 加拉姆越狱、斯莱德入队与 Battle05–06

| 编号 | 地点与玩家目标 | 触发 / 玩家选择 | 发生的变化与下一步去向 | 必经 / 可选 |
| --- | --- | --- | --- | --- |
| **G11** | **加拉姆地牢与密道**（`Map17`）· 随斯莱德越狱 | 苏醒（`F660`）、牢房对话开门（`F661`）、楼梯机关（`F662`，重踩仅位移不重复制捷径），进入密道。 | 进图必写 `F611`（之后放下加拉姆吊桥 `Map18`）。密道设置**斯莱德（Slade）临时随行**（`F73`，**尚未正式入队**）。地牢内可顺手拾取 `Chirrup Sandals`（`F205`）与秘银（`F206`）。 | 必经（拾取可选） |
| **G12** | **加拉姆城镇集会与西侧卫兵**（`Map16`）· 穿过城镇迎战守卫 | 触发广场出征演出（`F663`，开放商店服务），随后踩中西侧卫兵触发区。 | 首次发现解锁 `Battle05`（`F405`）并请求切换（`F664` 使后续新入场隐藏该卫兵）。商店、教堂与 `F207`/`F208` 宝箱可用；宝库楼梯始终被固定卫兵挡住（`F137`/`F138` 不可早拿）。 | 必经 |
| **G13** | **加拉姆城堡战**（`Battle05`）· 击败骑士并招募斯莱德 | 赢得 `Battle05`（鲍伊存活且击败槽位 128 骑士会触发特殊清场交接）。 | **首胜**写入首颗宝石显示标记（`F384`，**不进背包**）、**斯莱德正式入队**（`F5`/`F37`，清除 `F73`）、写入 `F505` 并解锁 `Battle06`（`F406`）后返回 `Map16`。**败北 / 撤离**保留 `F405` 且斯莱德未入队。战后王座与东塔房间可自由探访。 | 必经 |
| **G14** | **加拉姆吊桥与南归路**（`Map18`/`66`）· 迎战 `Battle06` 返回格兰希尔 | 经已放下的吊桥（`F611`，桥上卫兵/商人/路牌可选）南下 `Map66` 触发 `Battle06`（`F406`）。 | **入场与胜利分离**：`Battle06` 的**战前脚本**写入 `F407`（解锁 `Battle07`），而**胜利**才写入 `F506`（切换遭袭格兰希尔）并返回 `Map66`；败北/撤离保留 `F406`。 | 必经 |

### 第四阶段 · 遭袭格兰希尔、Battle07、地震与码头出航

| 编号 | 地点与玩家目标 | 触发 / 玩家选择 | 发生的变化与下一步去向 | 必经 / 可选 |
| --- | --- | --- | --- | --- |
| **G15** | **遭袭格兰希尔与可选 Kiwi**（`Map3` `F506`）· 巡查城镇并决定是否收留 Kiwi | 进城查看破损兽笼，与受惊的 **Kiwi** 对话（同意并命名 vs 拒绝/绕过）。 | **同意并确认命名**让 Kiwi 正式入队（`F6`/`F38`）并当前随行；**拒绝或跳过**保持 `F6` 为空，不影响进城推主线，且在出航（`G20`）前随时可回来招募。 | 可选（`G20` 前可补招） |
| **G16** | **遭袭城堡**（`Map19` `F506`）· 目送阿斯特拉尔/雷蒙赴塔并护送国王与大臣 | 沿城堡北行经过两处相互独立的触发区（`F620`、`F621`）。 | `F620` 演出阿斯特拉尔与雷蒙前往古代之塔（雷蒙不入队）；`F621` 让**国王与大臣作为临时随行**（`F83`，**不入出战名册**）。 | 必经 |
| **G17** | **古代之塔与 Battle07**（`Map40`/`57`/`60`/`58`）· 下塔击败黑烟 | 塔外遇负伤雷蒙（纯剧情 NPC，可选秘银 `F132`），沿 `Map57`/`60` 下至 `Map58` 触发 `Battle07`（`F407`）。 | 战前清除 `F83`。**首胜**写入第二颗宝石标记（`F385`，面板显示需与 `F384` 同时存在）、安装**三人王室随行链**（`F74`：阿斯特拉尔 → 鲍伊、国王 → 阿斯特拉尔、大臣 → 国王）、写入地震替换标记 `F640` 与 `F507` 并回到 `Map58`。败北/撤离可重试。 | 必经 |
| **G18** | **城堡预警与地震城镇**（`Map19` `F507` → `Map4`）· 出城堡见证两名卫兵坠入地裂 | 沿塔楼返回 `Map19`（两段独立预警 `F622`/`F623`），再从城堡南门出城。 | `F640` 将 `Map3` 替换为**地震城镇 `Map4`**。**首次进入**演出地震、地裂地形改写与**两名卫兵坠落离屏**，随后写入 `F624`（城堡与塔楼本身无持久地裂，见 `#2288`）。**重访**按 `F624` 重建地裂并停放卫兵，不重演开场；街道秘银 `F217` 仍可拾取。 | 必经 |
| **G19** | **撤离码头与船长选择**（`Map44` `F507`）· 决定是否立刻起航 | 从 `Map4` 东出抵达 `Map44`，在 `(10,14)` 与**船长（实体 138）**对话，选 **No/取消** 或 **Yes**。 | 固定避难人群占满三格登船口（`F507` 下无法再下船舱拿 `F140`，但甲板 `F139` 仍可拿）。西门可回 `Map4`（`F640` 有效）。选 **No / 取消**平顺归还控制，**不写 `F89`**，保留 `F74`/`F640` 供稍后再来。 | 必经抉择（No 可重试） |
| **G20** | **确认登船与离岛**（`Map44` → `Map66` → `Map73`）· 告别毁灭的格兰希尔并驶向 Parmecia | 对船长 138 确认 **Yes**。 | 演出登船、**清除 `F74`**（结束王室地图随行）、在 `Map66` 重建船只演出毁灭过程、排队请求进入 **`Map73`**（`(26,13)` 朝右）并**清除 `F640`**。本章探索至此收束并交接给航行段落。 | 必经章末交接 |

## 技术追踪与命名场景对照（按事件编号反查）

*本表只保留可搜索的关键 F 编号、既有规则/研究链接与已登记的重制场景起点，不抄写第二份完整 flag 表，也不为每行捏造存档。*

| 事件 | 关键 Flag（供反查） | 权威设计 / Flag / 地点来源 | 已登记重制起点与观察边界 |
| --- | --- | --- | --- |
| **G1–G4** | `F0`/`F32`、`F78–80`、`F600–605`、`F607–608`、`F66`、`F1–2`/`F33–34`、`F401`/`F451`/`F501`、访问 `F256–263` | [开篇至 Battle01](opening-through-battle01.md) · [Grans Flag 索引（英文源）](../../../../../../remake/docs/content/campaign-flags-grans.md#opening-granseal-and-the-tower) · [地点索引（英文源）](../../../../../../remake/docs/content/campaign-locations.md#grans-and-the-voyage) | **New Game** + 连续 **Battle01** 与独立快照 `battle01`（`battle01-movement/slot1.json`）；**原版受限实测**也覆盖此段（[开局验收（英文源）](../../../../../../remake/docs/evidence/opening-loop-acceptance.md#reproduce-the-composed-route)、[展示起点（英文源）](../../../../../../remake/docs/application/pre-voyage-campaign.md#showcase-starts-and-continuity)）。 |
| **G5–G8** | `F609–610`、`F3`/`F35`、清 `F66`、`F402`/`F502`、`F72`、`F403`/`F503`、`F133` | [Battle01→02](battle01-return-through-battle02.md) · [Battle02→03](battle02-outcome-through-battle03.md) · [Grans Flag（英文源）](../../../../../../remake/docs/content/campaign-flags-grans.md#yeel-hawel-and-galam) · [南侧神殿（英文源）](../../../../../../remake/docs/application/pre-voyage-optional-locations.md#southern-shrine-and-excluded-connections) | 重制普通连续流程覆盖至 Battle03 返回与南侧神殿（[至 Battle03（英文源）](../../../../../../remake/docs/application/pre-voyage-campaign.md#ordinary-new-game-through-the-battle03-return)）；**无独立场景 JSON**（仅链 owner）。 |
| **G9–G10** | `F4`/`F36`、清 `F72`、`F612`、`F650`、`F404`/`F504` | [Battle03→04](battle03-outcome-through-battle04.md) · [Battle04→05](battle04-outcomes-through-battle05.md) · [Grans Flag（英文源）](../../../../../../remake/docs/content/campaign-flags-grans.md#yeel-hawel-and-galam) | 重制普通流程覆盖哈维尔招募、安静重访、耶尔 `F650` 与 Battle04 胜/败/撤离（[哈维尔（英文源）](../../../../../../remake/docs/application/pre-voyage-campaign.md#ordinary-hawel-house-and-kazin-recruitment)、[Battle04（英文源）](../../../../../../remake/docs/application/pre-voyage-campaign.md#battle04-and-the-galam-prison-handoff)）；**无独立场景 JSON**。 |
| **G11–G14** | `F660–664`、`F611`、`F73`、`F405`/`F505`、`F384`、`F5`/`F37`、清 `F73`、`F406`/`F456`/`F506`、`F407` | [Battle04→05](battle04-outcomes-through-battle05.md) · [Battle05→06](battle05-outcomes-through-battle06.md) · [Grans Flag（英文源）](../../../../../../remake/docs/content/campaign-flags-grans.md#yeel-hawel-and-galam) · [加拉姆探索（英文源）](../../../../../../remake/docs/application/pre-voyage-optional-locations.md#galam-optional-exploration) | 普通游玩覆盖地牢至 Battle06（[加拉姆路线（英文源）](../../../../../../remake/docs/application/pre-voyage-campaign.md#galam-escape-through-battle06-first-control)）。命名受控起点：**`slade-join`**（[`slade-join.json`](../../../../../../remake/tools/showcase_starts/slade-join.json)）用 Debug Auto Victory（无战斗奖励，仅验证 Battle05 结算→宝石→斯莱德入队，**非自然战斗**）；战后存档记录 **`map16`**（`galam-learned-magic/slot2.json`）。 |
| **G15–G17** | `F506`、`F6`/`F38`、`F620–621`、`F83`、`F407`/`F457`/`F507`、`F385`、`F74`、`F640` | [Battle06→07](battle06-outcomes-through-battle07.md) · [Battle07 后交接](battle07-outcomes-and-aftermath.md) · [Grans Flag（英文源）](../../../../../../remake/docs/content/campaign-flags-grans.md#occupied-granseal-and-the-voyage) | 普通游玩覆盖 Kiwi 拒绝/绕过/补招、王室随行与 Battle07 返回（[Battle06→塔（英文源）](../../../../../../remake/docs/application/pre-voyage-campaign.md#battle06-result-through-usable-tower-exploration)）；规划 Debug 起点：**Tower return**（[Debug 起点表（英文源）](../../../../../../remake/docs/application/pre-voyage-campaign.md#proposed-non-battle-debug-starts)）。 |
| **G18–G20** | `F622–624`、`F640`、`F74`、`F507`、清 `F74`/`F640`、后续 `F700` | [塔后→地震](post-tower-return-through-earthquake-entry.md) · [地震→码头](earthquake-town-through-docks-departure.md) · [Grans Flag（英文源）](../../../../../../remake/docs/content/campaign-flags-grans.md#occupied-granseal-and-the-voyage) · [调试端口（英文源）](../../../../../../remake/docs/verification/debug-port.md#preparing-a-progression-start) | 普通承接存档覆盖预警、首次/重访地震、船长 No/取消/Yes 至 Map14 交接（[塔→船（英文源）](../../../../../../remake/docs/application/pre-voyage-campaign.md#tower-return-through-first-damaged-ship-control)）。命名受控起点：**`castle-quake`**（[`castle-quake.json`](../../../../../../remake/tools/showcase_starts/castle-quake.json)，仅验证两卫兵坠落与 Map4 交接）；独立承接记录：**Boarding**（`post-battle07-return/slot1.json`）。*特定导出包内 `Map44` 自然王室立绘仍为**未知**（`#2330`）。* |

## 本章人物速查（正式入队 vs 临时随行 vs 剧情 NPC）

*全战役招募总表与后期载具链归 `#2366` 后续切片；重制版转职与战斗实测见[角色索引（英文源）](../../../../../../remake/docs/content/campaign-characters.md#grans-and-the-voyage)。*

| 角色 | 本章定位 | 临时随行（非出战状态） | 正式入队（军团成员） | 可选与边界说明 |
| --- | --- | --- | --- | --- |
| **鲍伊（Bowie，0）** | 初始队长 | — | New Game（`F0`/`F32`） | 始终出战；HP 归零直接判败北。 |
| **莎拉（Sarah，1）、Chester（2）** | 开局同伴 | `F66`（`G2` 同意起，至 `G5` 贾哈入队清除） | `G2` 信使选 Yes（`F1–2`/`F33–34`） | 拒绝 `G2` 不入队，可重谈接受。 |
| **贾哈（Jaha，3）** | 格兰希尔招募 | 入队时局部跟随 Chester | `G5` 城镇西门触发（`F3`/`F35`） | 委任后（`F609`）必经西出触发。 |
| **卡辛（Kazin，4）** | 耶尔向导 → 队员 | 耶尔写入 **`F72`**（`G7`，可选，此时**不能参战**） | **`G9`** 哈维尔小屋（`F4`/`F36`，清除 `F72`） | 跳过耶尔（`G7`）仍可在 `G9` 正常入队。 |
| **斯莱德（Slade，5）** | 加拉姆带路 → 队员 | 地牢密道写入 **`F73`**（`G11`，**尚未入队**） | **`G13`** `Battle05` 首胜（`F5`/`F37`，清除 `F73`） | 战败或撤离 `Battle05` 会保持未入队。 |
| **Kiwi（6）** | 可选招募队员 | 命名后当前随行（`G15`） | **`G15`** 遭袭格兰希尔（`F6`/`F38`）确认命名后 | **可选**：拒绝或跳过不阻断进城堡；出航（`G20`）前可折返补招。 |
| **阿斯特拉尔（Astral）** | 导师 / 王室护送 | **`F74`** 王室随行链（`G17` 首胜 → `G20` 清除） | 永不进入出战名册 | 开局城堡邀请（`G3`）可拒绝重试；后期 Parmecia 参谋随行另用 `F70`。 |
| **国王与大臣** | 王室随行 | **`F83`**（`G16` → `Battle07` 战前清除），再转 **`F74`**（`G17` → `G20` 清除） | 永不进入出战名册 | 场景随行独立于 12 人出战上限。 |
| **雷蒙（Lemon）** | 剧情 NPC | 本章无随行 | 本章不入队（后期 28 号归 C 片） | 负责 `Battle04` 俘获（`G10`）、城堡赴塔（`G16`）与塔外负伤演出（`G17`），均不构成入队。 |

## 本章关键物品与重要拾取速查

*Grans 阶段**没有必经主线钥匙物品门槛**（不同于后续 Parmecia 的 Wooden Panel、Achilles Sword、Cannon 与 Dynamite）。两颗宝石仅写剧情标记；实体道具均经普通场地搜索获取（[物品索引（英文源）](../../../../../../remake/docs/content/campaign-items.md)、[可选探索（英文源）](../../../../../../remake/docs/application/pre-voyage-optional-locations.md)）。*

| 物品 / 状态 | 分类 | 位置与记录标记 | 时机与可获取边界 |
| --- | --- | --- | --- |
| **邪恶宝石 / 光明宝石**（`F384` / `F385`） | **剧情显示标记（不进背包）** | `Battle05` 首胜（`F384`）与 `Battle07` 首胜（`F385`） | 驱动成员界面鲍伊的宝石图标（`F385` 显示需同时持有 `F384`）；不占 4 格物品栏。 |
| **Power Water** | 可选永久成长道具 | 南侧神殿下层宝箱（`Map42`，`F133`） | `G5` 后可经 `Map66` `(33,43)` → `Map41` 门 → `Map42` 楼梯无门槛进入；确认出航（`G20`）离开 Grans 后关闭。（中部的 Devil's Head 入口要到 `Battle08` 后才开启。） |
| **Quick Chicken** | 可选永久成长道具 | 格兰希尔码头甲板木桶（`Map44` `(27,6)`，`F139`） | `F609` 开放码头后（`G5`）即可拾取，`F507` 撤离码头（`G19`）在 `G20` 前仍可拿；损坏船 `Map14` 上无有效标记。 |
| **Chirrup Sandals** | 可选戒指槽趣味道具 | 加拉姆地牢（`Map17` `(56,4)`，`F205`） | 越狱（`G11`）或 `G20` 前重访加拉姆可取；装备在戒指槽后改变脚步音效。 |
| **秘银（Mithril，前期 4 块）** | 可选后期锻造材料 | 塔外（`Map40` `F132`）、地牢（`Map17` `F206`）、大地图（`Map66` `F213`）、地震城镇（`Map4` `F217`） | 本章无装备/使用效果也无铁匠（留待后期矮人村锻造）；`F217` 在地震地裂改写（`F624`）后仍保留可拾取路径。 |
| **常规补给与受限/后期条目** | 一般搜刮 vs 后期锁闭 | 格兰希尔（`F128`）、城堡（`F129–131`）、耶尔（`F134`）、哈维尔（`F135–136`）、加拉姆（`F207–208`）、船舱（`F140`） | 船舱内 `Medical Herb`（`F140`）在 `F609`/`F506` 码头可下舱拾取，但到 `F507`（`G19`）会被避难人群挡住登船口。`F220` 为被挡住的空箱；加拉姆宝库 `F137–138` 始终被卫兵拦截；耶尔地下 `F196–197`（`F940`）与加拉姆北门 `F200`（`F999`）属于后期返回 Grans 阶段。 |

## 后续阅读与剩余范围

- **紧接后续（不在本 A 片展开）**：[航行登陆至损坏船只控制](voyage-through-damaged-ship-control.md) → [损坏船只木板选择至 Battle08](../eastern-parmecia/damaged-ship-plank-through-battle08.md)。
- **全章总入口与 11 篇段级详解**：[剧情推进](../../themes/story-progression.md#grans-开篇与离开)（按时序收拢本章 11 篇详解，并保留其余五大章待 `#2366` B–C 片分章推进）。
- **英文规范源与技术索引**：[Grans Chapter Overview（英文规范源）](../../../../synthesis/chapters/grans-opening/chapter-overview.md) · [Grans Flag 索引（英文源）](../../../../../../remake/docs/content/campaign-flags-grans.md) · [角色索引（英文源）](../../../../../../remake/docs/content/campaign-characters.md) · [物品索引（英文源）](../../../../../../remake/docs/content/campaign-items.md) · [地点索引（英文源）](../../../../../../remake/docs/content/campaign-locations.md) · [调试端口（英文源）](../../../../../../remake/docs/verification/debug-port.md#preparing-a-progression-start)。
