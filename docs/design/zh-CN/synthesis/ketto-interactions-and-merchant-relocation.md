# Ketto：对话、商店与商人搬移

> 本文是[规范英文原文：Ketto: Conversations, Shops and the Merchant's Move](../../synthesis/ketto-interactions-and-merchant-relocation.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../glossary.md) R1–R7。已译相邻指南与合同使用中文导航，未译指南标明英文回退；研究、源码与溯源链接保留英文所有者。历史环境、审阅和验证记录属于原英文切片。

**已确认：** Ketto 居民描述恶魔强加的隔离，而独立回调提供真正 Yes/No 对话、布置商人搬移、调用商店/教堂，并处理检查与两件可收取物品。话语、标志、移动与服务结果各有后果。**推断：** 这些是已接受 [Ketto 入场](battle25-outcomes-through-ketto-entry.md)后的局部活动，不是必需序列。**未知：** 自然访问、传入状态、已完成服务、表现/时序、可用控制及存读档持久性。

此英文指南消费完整已接受 [Ketto 交互研究](../../../research/ketto-interactions-and-merchant-relocation.md)，其固定源码、ROM 身份、符号、地址、字面量和复现配方拥有原版证据。它为 [#1002](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1002)遵循 [Standard 范围协议（英文）](../../documentation-roadmap.md#current-baseline-and-near-term-synthesis)。不增加原版发现或重制行为，止于局部返回和条件 Map72 准入，早于 Battle26 场景、名册或交战。

Fnnn 表示标志 nnn；对话数字标识选定文本，不是序列门槛。

## 与居民交互

**已确认：** F800 不选择不同人口。新 Ketto 在实际普通随从后声明逻辑行动者 128–135；有 n 名随从时，通常占物理 n+1 至 n+8。Bowie/主角为物理 0；商人 133 与女子 135 通常用物理 n+6/n+8 和查找偏移 37/39。[入场指南](battle25-outcomes-through-ketto-entry.md#独立入口构建-ketto)拥有原点、步行边界、随从别名与有限存储前提。

普通玩家行动先令物理 0 Idle，并等待目标/视野滚动。A 打开场地菜单；C 在调试和重叠 Caravan 替代分支之后到达激活。视野目标 0 时，激活使用当前位置和朝向，可穿过柜台布局类型 `$2800` 查找，再选择 Manhattan 距离小于 384 的首个符合物理行动者。它跳过被观察行动者及当前随从。负视野目标返回；其他摄像机状态不自动等于普通交互。布局读取不是图层相等测试。逆向查找从行动者列表起点搜索，在 32 个己方项后切到逻辑 128，无匹配后备逻辑 0。因此更早别名可改变所选事件。

实体包装器在 EVENT_RELATIVE_POSITION 存玩家朝向，取首个匹配逻辑行或默认行。立绘/台词上下文来自实际映射精灵，不是预期 NPC 的叙事假定；它预填 CURRENT_PORTRAIT/CURRENT_SPEECH_SFX，并有条件打开立绘。行 128–134 的 `UP1` 表示一个 VInt 后转向 `(playerFacing+2)&3`，不恢复保存朝向。135 的 `DOWN3` 还在返回后恢复行动者保存朝向。这些是转向/恢复动作位，不是接近门槛。默认 0 两者均不做。包装器在回调前抑制 VInt_UpdateEntities，返回时应用选定朝向恢复、关闭立绘/文本并激活实体更新。

| 行动者 / 回调 | 对话与局部后果 |
| --- | --- |
|128 年轻男子 / Event0|1956 说明 Ketto 位于 North Parmecia 南缘；返回。|
|129 老人 / Event1|1957 问男孩从何而来；1958 对 South Parmecia 表示惊讶；1959 说南方隧道堵塞；返回。|
|130 男子 / Event2|1960 质疑从 South Parmecia 来；1961 说 Pacalon 反抗恶魔后一直被隔离；1962 劝离开；返回。|
|131 年轻男子 / Event3|1963 抱怨异常炎热；1964 问 Mitula 是否如传闻般病重；返回。|
|132 男子 / Event4|1965 说恶魔禁止居民外出；返回。|
|133 商人 / Event5|800 未设：1966–1969、恢复速度、设置 800 并运行搬移程序。800 已设：选商店 9 并调用 ShopMenu；若服务返回则返回。|
|134 祭司 / Event6|256 未设：1971 警告，再设 256。未设/已设两路径均调用 ChurchMenu；若服务返回则返回。|
|135 女子 / Event7|800 已设：商店 27。否则，257 已设：1978 逐客。否则 1972 与实际 YesNoPrompt，再 1973–1975 或 1976–1977，设置 257；返回。|

默认实体回调是该包装器内的 RTS。独立 `nullsub_5D2E8` 不是第九名居民。没有局部回调或商人程序设置 426、招募/激活成员、收费或授予物品。共享服务有自己的选定变更路径。

这些居民字面量没有名称/物品替换。连续 `txt` 调用创建/复用文本窗口并重置替换指针；不会隐式关闭、sleep 或推进过场游标。W1/W2 文本控制是确认，各有不同游标、RNG、VInt/输入与清理工作；两者都不创建选择菜单。[对话合同](../contracts/dialogue-system.md)拥有这些消费者。显式关闭文本与包装器清理仍独立。叙事禁令不创建旅行、祈祷或销售门槛。

## 女子的选择与祭司的警告

**已确认：** 800 与 257 未设时，女子说“嘿，听我说。”（1972），然后实际调用 YesNoPrompt。初选 Yes。入场时按住的输入必须先释放。Left 选 Yes/0；Right 选 No/$FF；选择动作循环。循环按 Left、Right、B、C、A 顺序测试；B 提交 No，C/A 确认当前选择。窗口清理后，辅助返回符号扩展的 Yes=0 或 No/B=-1。回调关闭文本并按该字分支。

Yes 请求 1973–1975：Pacalon 曾是好顾客，买很多东西，但恶魔现在阻止武器销售；她说他们讨厌。No 或 B 请求 1976–1977：她道歉，不想打扰玩家。两路径都只在最终文本返回后设临时 257。在该写入前中断可使提示仍可再次出现。持有 257 则给 1978：“又是你？再见。”先检查 F800：一旦已设，物品商店覆盖提示和重复逐客。

祭司 1971 说他不能打开教堂并请谅解。临时 256 仅抑制首次警告：设置发生在文本后，两路径都无条件调用 ChurchMenu。文本 1970“我是祭司。”存在，但这些回调均不调用它。警告和女子所述销售禁令都不禁用所调用服务。

## 商人搬移与商店分支

**已确认：** 800 未设时，商人咒骂恶魔（1966）、认出顾客（1967）、秘密推销优质武器（1968），再问玩家是否想要，却立即假定同意并邀请到店（1969）。此交流无 YesNoPrompt 或结果分支。确认 1969 不等于同意、付款或物品授予。

Event5 保存 MESSAGE_SPEED 字节，在 1966/1967/1968/1969 前分别写 0/1/2/3，恢复原字节，再在启动 `cs_5D3B8` **之前**设置持久 800。打字消费者由这些值导出普通字形延迟操作数 4/2/1/0；台词及 W1/W2 工作仍独立，因此不是耗时承诺。文本中断可能留下临时速度；之后中断可能留下 800 已设及仅部分行动者变化。没有回滚。800 已设的商店分支不重复该场景。

完整程序按以下顺序：

| 阶段 | 命令与后果 |
| --- | --- |
|商人速度|`customActscriptWait 133` 安装成对 X/Y 速度 64 与跳到 eas_Idle；等待该 Idle 指针再继续。|
|Bowie 目标|`setDest ALLY_BOWIE,12,2` 使用行动者字 0，等待两轴位置均等于目标。|
|Bowie 朝向|仅在该等待之后，`setFacing ALLY_BOWIE,DOWN` 请求 Down。|
|五段商人移动|五条独立 `entityActionsWait 133` 流分别移动 Right5、Down1、Right2、Up1、Right2；每条先返回 Idle 再开始下一条。|
|Bowie 手势|`headshake ALLY_BOWIE` 执行摇头工作。|
|最终布置|`setPos 133,25,18,RIGHT`，再 `setPos 135,25,17,RIGHT`，然后脚本 end。|

已接受修正至关重要：**行动者 0 的 setDest 会等待**。消费者保留完整行动者字、写目标/行程/±32 速度，并测试位 15。只有高位已设才跳过 WaitForEntityToStopMoving。若操作数改为 `$8000`，仍解析逻辑 0 但跳过等待。实际场景使用 0。随后朝向命令有己方 HP 守卫，可跳过死亡 Bowie；setDest 与 headshake 无此守卫。这是条件命令行为，不证明行动者稳定到位或完整场景返回。

每段商人移动生成 14 行动字节：有符号相对瓦片运动、目标等待和跳到 Idle。五条流共 70 生成字节。安装清 FLAGS_A 位 5/6；位 5 原会控制目标阻挡。运动向行动者实际当前位置加缩放增量，并使用存储速度。净相对位移为 (+9,0)：原位置未变的 (11,3) 条件终点为 (20,3)，早于最终商店布置。变化原点改变终点。这些命令不证明寻路或通行。速度 64 安装本身等待 Idle 指针，不证明特定显示动画或耗时。

Headshake 运行七次迭代，每次六次显式 VInt 等待，另有精灵变换/DMA 请求。42 次命名等待不证明总时长或 DMA 完成。最终 setPos 写位置、目标为瓦片×384、朝 Right，再请求精灵更新。不步行、不替换行动脚本，也不清速度/行程。商人/女子 NPC 选择器绕过己方 HP 守卫。缺失零查找别名到物理 0；隐藏 `$FF` 不是安全空操作。当前映射必须确实指向预期行动者。中断可只留下首个布置。脚本 end 可在开放文本时等待滚动并清滚动速度。文本跳过不跳过这些运动命令；整个过程都以服务推进为前提。

## 服务与重新入场保留内容

**已确认：** 800 设置后，商人选 CURRENT_SHOP_INDEX 9，女子选 27。这些是从零开始、带长度前缀的物品栏索引：9 是第十物理行（注释“Weapon shop 10”），不是注释“Weapon shop 9”；27 是物理行 27（注释“Item shop 13”，别名 SHOP_ITEM_MOUN）。没有独立局部类型字节选择另一服务。

| 索引 / 类别 | 有序库存，省略 ITEM_ 前缀 |
| --- | --- |
|9 / 武器|BROAD_SWORD, BUSTER_SWORD, HEAVY_LANCE, JAVELIN, LARGE_AXE, ROBIN_ARROW, ASSAULT_SHELL, GUARDIAN_STAFF, INDRA_STAFF, POWER_GLOVE|
|27 / 物品|MEDICAL_HERB, HEALING_SEED, HEALING_DROP, ANTIDOTE, FAIRY_POWDER, ANGEL_WING|

ShopMenu 有条件打开当前立绘、请求 158、关闭文本/立绘并进入通用选择器。库存与调用不建立成功购买、可负担、免费物品或已完成事务。[服务合同](../contracts/service-interactions.md#shop-源码边界)拥有购买/特卖先付款后加入、移除特卖、出售/丢弃/珍稀特卖、修理、取消与循环，以及物品栏/装备/容量前提。

ChurchMenu 有条件打开当前立绘、请求 110、关闭文本/立绘并进入复活/治疗/转职/存档。仅进入既不改变 EGRESS_MAP，也不治疗或保存所有人。选定 Save 分支中，首提示零将 CURRENT_MAP 复制到 EGRESS_MAP（此处 25）、设 399，再以 CURRENT_SAVE_SLOT 调 SaveGame。拒绝该提示不提交路径；在 Save 前取消保留旧选择器。之后继续/挂起提示独立：零退出，非零到淡出和 WitchSuspend，而非普通回调返回。

已接受[教堂边界](../contracts/service-interactions.md#church-源码边界)与[存档合同](../contracts/save-system.md)保留共享服务观察；它们不是新 Ketto 运行时证据。Ketto 存档点数据为 Map25 (12,11),Up。399 已设时 GetSavepointForMap 使用此表路径；399 未设用初始 Granseal 后备，之后木筏重置处理由入场所有者保留。该行是可能返回数据，不是入场传送、已完成存读档或持久性保证。

| 入场/状态 | 局部后果 |
| --- | --- |
|新显式 25，800 未设|重建 NPC 原点/行动，清临时 256–383；init 不搬移。|
|新显式 25，800 已设|先重建，再只运行 cs_5D3AA 两次 setPos。无速度 64 安装、路线、文本或摇头；新 eas_Init 提供速度 32 策略。|
|当前 25 上 CURRENT255|保留行动者/行动/临时标志并运行 init。800 已设向当前映射重应用布置；800 未设不恢复原点。|
|滚动|跳过重置/init；不推断新行动者或临时标志重置。|
|移动后手工清 800|可从变化位置重放相对运动；无自动恢复或修复。|

持久 800 不同于临时 256/257：新入场重置警告与女子重复状态，但 800 仍用商店 27 覆盖提示。另一地图上的 CURRENT 不是 Ketto。传入模式、映射与中断前缀保留自身效果。

## 书籍、标牌与两件搜索物品

**已确认：** 普通 CheckArea 在视野目标 0 时检查前方瓦片，清对话名称索引 1 和台词 SFX 为 0，并在局部描述处理器之前优先符合的宝箱/花瓶/木桶/书架/区域物品布局类型。普通无实体 C 提供 d6=1。坐标本身不建立布局资格、可达或实际检查。

全部 11 条局部行均为 condition 0/kind 0，无一要求 d6=0。首个获准压缩 X/Y 匹配显示调查 423+offset，再显示描述 4132+offset，然后关闭立绘/文本并返回 -1；无匹配返回 0 给调用者后备。调查通过 NAME 使用成员 0 实际名称，不硬编码“Bowie”。偏移 4/文本 427 说该名称调查书架；偏移 2/文本 425 说该名称调查标牌。其 W2/CLEAR 继续不同于描述末尾 W1。

| X,Y / 调查 | 描述偏移 / ID | 英文内容的中文释义 |
| --- | --- | --- |
|2,10 / 书架|0 /4132|一本书：TRAVELS。|
|3,10 / 书架|1 /4133|一本书：VILLAGES AND TOWNS。|
|2,16 / 标牌|2 /4134|禁止外出，否则受罚；署名 Devils。|
|6,26 / 标牌|3 /4135|“怀疑惩罚？”；署名 Devils。|
|15,26 / 标牌|4 /4136|教堂关闭。|
|15,15 / 标牌|5 /4137|今日禁止祈祷；署名 Devils。|
|14,18 / 标牌|6 /4138|Pacalon 永远不会来救你们。|
|16,19 / 标牌|7 /4139|禁止工作。|
|22,29 / 标牌|8 /4140|Mitula 不理你们。|
|27,24 / 标牌|9 /4141|禁止销售武器或物品；署名 Devils。|
|16,28 / 标牌|11 /4143|Ketto 村。|

无局部行选偏移 10/文本 4142。标牌构成压迫氛围，不施加可执行服务禁令。[描述合同](../contracts/map-area-description-routing.md)保留共享首匹配和调用者边界；此处未提取地图。

宝箱表为空。其他物品表包含 (4,22) 的 BRIGHT_HONEY，收取标志 170，以及 (29,18) 的 HEALING_WATER，标志 171。这些是收取标志，不同于 256/257。共享非宝箱搜索要求符合的掩码布局类型（`$2C00/$3000/$3400/$1C00`）和实际坐标匹配，使用低字节，仅在 CURRENT_BATTLE=255 之外应用战斗裁切。无匹配返回 -1。空宝箱数据不禁用共享搜索。

匹配行中，持有收取标志给 ITEM_NOTHING，并再次设置标志；未设则返回物品并在**物品栏成功之前**设置标志。文本 413 使用名称索引 1=0，索引 2 为选定物品。Bowie 少于四物品时接收 AddItem。否则 UpdateForce 和有序其他已加入军团列表选择首个有空间成员，不论 HP 或出战资格。该列表从 TARGETS_LIST 后一个字节开始，假定普通已加入 Bowie 占首项。Bowie 未加入时，仍先尝试槽 0，但后备省略实际首个已加入成员；不修复该传入状态。

成功保留标志、请求 415 或 416（第三名称索引标识接收者）、物品音乐及独立音乐/普通输入辅助。这些消息不添加虚构 W1/W2 token。获取不是消耗品使用。全部已满则请求 417，然后 CloseChest 与 RefillNonChestItem 在保存坐标重新查找行、清其标志以供重试。已收取 ITEM_NOTHING 走空物体文本，不重复授予。设标志后中断可能无物品且无补回；AddItem 后中断可能有物品但表现未完成。满军团恢复不是事务保证。

默认 zone 与 item-event 回调是 RTS，但包装器仍清理并等待 VInt/物理 0 目标。Item-event 配置还清台词、掩码所用物品、将传入 Y 存 EVENT_RELATIVE_POSITION 并设 d6=0。物品使用分发不同于其他物品收取。这些空操作不禁用共享处理器，也不解锁 426。

## 离开 Ketto 是独立请求

**已确认：** 回调/服务实际返回且清理完成后，调用者可到 WaitForEvent。待处理 0 选择摄像机物理 0 并请求控制：模式 0 为角色，模式 1 为木筏，其他非零模式为 Caravan。待处理非零则返回该事件。这些是条件请求，不是可玩控制或输入/文本/菜单/行动服务完成的观察。

唯一出口行匹配通配 X255/Y31，请求无滚动 Map72 (31,27),Down，无 800/256/257/426、物品或队伍条件。普通角色控制仍需符合布局类型 `$1000` 和传送生产者；Y31 本身不立即离开。生产者写可变 type 1/参数和声音。消费清类型、提交/清声音、令 0 Idle、退出探索、采样坐标、替换非负坐标低字节并应用朝向。名义 (31,27) 假定普通采样高字节。替换或中断可改变请求。

MainLoop 随后运行 SwitchMap/CheckBattle。Map72 候选 8/24/26 有序，均为通配触发，各有独立解锁 408/424/426。首个已解锁胜出；匹配持有完成 508/524/526 在**已选择同一战斗后**清解锁。裁切是输出，不是包含测试。Ketto 无局部回调写 426；购物与 800 不选择 Battle26。无准入时，Map72 配置/init 仍适用，包括 F65 Caravan 资格与独立 716 已设/Luke10 未设场景。

之后 X32–34 zone 可调用辅助 26，检查 526 而非 525。526 未设立即请求。526 已设/STEP_COUNTER 非零拒绝；计数器零抽范围 8：零请求，否则范围 4+2 提供计数器 2–5 并拒绝。请求先于后续服务设置 426 与 CURRENT255；下一扫描仍可先选 8/24。其他洞穴/默认 zone 保留[已接受行为](battle25-outcomes-through-ketto-entry.md#独立洞穴出口与-map72-准入)。此边界不建立必需、可通行或无阻路线，止于 Battle26 所选场景、编队、交战或战后之前。

[Battle26 准入与 Geshp 对峙](battle26-admission-and-geshp-confrontation.md)继续解释独立获准的新 before-battle 场景：实际场地演员、两名 Pacalon 士兵、Geshp 附身威胁与 Granseal 打断。终点仍早于新治疗/编队/加载/start；该连接不建立自然旅行、交战死亡或 Higins 招募。

## 证据审查与保留结果

完整已接受研究及变化状态矩阵已审阅，包括实际激活/别名/接近位、全部 800/256/257 路径、Yes/No/B 与中断提交、行动者 0 对比 `$8000`、死亡/缺失/隐藏行动者、相对原点、重载模式、库存索引、服务取消、Save/挂起、坐标/布局优先级、收取/补回与有序出口准入。选定公开表面已直接检查：

| 表面 | 可复现所有者与限制 |
| --- | --- |
|实体/默认回调与包装器|组合 map-events 夹具，包括 entity-programs 和 routing/default 相邻文件；选定公开操作不替代私有行动者/脚本行。|
|交互/对话状态|`sf2-map-event-interaction-state-static-v1` 与 `sf2-map-event-dialogue-state-static-v1`；实际生产/消费角色和文本状态，不是自然激活或已交付文本。|
|选择/服务|`sf2-common-menus-static-v1`、menuFacts.yesNoPrompt 与 serviceStateMachines；已确认返回值和选定变更，不是已完成 Ketto 事务。|
|运动/init|`sf2-map-script-engine-static-v1` 布置/行动消费者与 `sf2-map-init-static-v1`；实际高位等待和初始化顺序，不是动画完成。|
|检查|`sf2-map-descriptions-static-v1`；首匹配/运算/布局边界，不是全部局部字面量或可达瓦片。|
|离开/存档点|`sf2-map-event-request-consumption-static-v1` 与 `sf2-common-maps-static-v1`；可变请求/准入/返回规则，不是自然旅程。|

维护的[地图事件加载器](../../../../src/sf2tool/h2/map_events_fixture.py)、[菜单验证器](../../../../src/sf2tool/h2/menus.py)、[脚本验证器](../../../../src/sf2tool/h2/map_script_engine.py)、[描述验证器](../../../../src/sf2tool/h2/map_descriptions.py)与[请求消费者](../../../../src/sf2tool/h2/map_event_request_consumption.py)保留这些 schema 和聚焦边界。未发现范围内矛盾；源码矛盾返回研究。完整局部表、文本字面量与私有程序行仍由已接受研究拥有，汇总 schema 验证不重新证明它们。

所有命令仍是 `uv run sf2 h2` 配合 `map-events`、`map-event-interaction-state`、`map-event-dialogue-state`、`common-menus`、`map-script-engine`、`map-init`、`map-descriptions`、`map-event-request-consumption` 与 `common-maps`；研究所有者链接保留数据对配方。接受原设计环境缺少源码/数据对。公开检查与正文审阅**不是新比较**。无需采集、重生成、运行时、环境变化或新维护机制。

[#997 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/997#issuecomment-6181515501)与 [PR1000 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/1000#issuecomment-6181536109)保留 `cb3d3f0b`/tree `42adda37`/base `10e1ec16`、CI 38659895203（scope 15s/research 47s）、0.081s 未变数据对比较、等待 Bowie 修正、从零开始的库存 9、opcode 大小写断言失败/修正及诊断。此处保留已接受行动者 0 等待。

[#998 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/998#issuecomment-6181422397)与 [PR999 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/999#issuecomment-6181437287)保留 `d086715e`/tree `2dac7cbc`、CI 38659282728（scope 12s）、十一 schema 加 map-events/531 链接、失败的可读性链接检查/修正与不可用源码/数据对。其[保留审查](battle25-outcomes-through-ketto-entry.md#证据审查与保留结果)保留前序修正、失败与未知项。已完成结果不重置。

#1002 Issue/PR 交接记录完整正文/diff/导航/链接/锚点、UTF-8/私有边界/精确五路径检查、git diff --check、已配置干净提交规划及实际精确 head CI。文档范围不启动一揽子 normal/full/Python/.NET/Godot/H3/H4/翻译套件。传入状态、自然可达、服务推进、渲染时序/控制与存档持久性仍为**未知**，不产生自动采集队列。独立验收与集成仍由 main-gate 负责。
