# New Granseal 发明家与 Dynamite 赠礼

> 本文是[规范英文原文：New Granseal Inventor and Dynamite Gift](../../../../synthesis/chapters/northern-travel-mitula/new-granseal-inventor-and-dynamite-gift.md)的中文镜像；英文始终为审阅基线，翻译遵循[术语表](../../../../glossary.md) R1–R7。已译指南与合同使用中文导航；未译相邻指南标明英文，研究与源码仍由英文所有者提供。译文不产生新观察或扩大原版/重制边界。文末审阅、环境与验证记录保留原英文综合切片的时间范围。

在 New Granseal，发明家有可清除 North Cave 岩石的爆炸物，却警告使用者也会被炸飞。后来他要求 Astral 许可。返回觐见标志设置后，已加入军团的可搜索物品栏中实际存在 Cannon 会改变交谈：发明家认出安全投送方法，随从 B 点头，发明家提供 Dynamite。最早有空间的已加入成员可收取。物品栏全满则推迟赠礼，保留重试机会。

**推断：**故事表现 Astral 许可将爆炸物与 Cannon 配合使用。**已确认：**许可标志、实际物品栏、映射行动者手势、物品插入、收取宣布与调用方完成各自独立。此交互既不消费 Cannon，也不打开洞穴。**未知：**自然可达、传入状态、表现/服务完成、可用控制与存档持久性。

本文接续[返回 New Granseal 与 Astral 跟随](new-granseal-return-and-astral-following.md)，依据 main `36b0026bca3f7aaa5dee2b1d366312534000c522` 上[已接受的 #969 研究](../../../../../research/chapters/northern-travel-mitula/new-granseal-inventor-and-dynamite-gift.md)及其前序成果。指南止于发明家局部交互返回及必要的重新入场后果。Janet 招募与 North Cave 爆破有独立边界。此 Layer B 解释不选择重制规则或新观察工作。

## 从城堡到发明家

**已确认：**[选定返回与人口所有者](../../../../../research/chapters/northern-travel-mitula/new-granseal-inventor-and-dynamite-gift.md#castle-exit-current-town-and-fresh-population)指出 Map7 任意 X/Y23 的南出口行：无滚动到城镇 Map6 `(11,1)` Down。通配不证明每个 X 都可通行。移动必须产生布局传送请求，请求还须在待处理槽保留到分发。坐标低字节替换保留已接受高字节前提。Town6 没有标志切换地图或战斗坐标行。这是条件连接，不证明自然步行到发明家的路线。

普通重新探索准入清除待处理状态，应用带 HP/状态限制的已接受存活/不死治疗，重建行动者与精灵，清除临时标志256–383，设置 F80 并运行 init。剧情标志、军团成员资格与玩家模式仍是传入状态。当前城镇中的回调不重复该准入，也不重建随从。

城镇配置依次选择 default、F701、F702，最后适用选择器胜出。F802/F804/F805/F70 不选择居民表。三份列表的发明家都是 dwarf 引用141：default/F701 将他放在 `(14,27)` Right；F702 放在 `(24,7)` Down，均使用较慢 Init。早期[城镇人口指南](../eastern-parmecia/new-granseal-town-through-peter-introduction.md)拥有其余阵容。三份 entity-event row13 均调用 Map6_EntityEvent13；default 的行 byte0 与后期行 byte1 控制包装器朝向策略，不指定 Right 或 Up 为最终朝向。

共享城镇 init 优先 F802，将 priest132 放到 `(11,18)` Down 后返回。F802 未设/F706 已设时，改将 worker138 放到 `(63,63)` Down、guard139 放到 `(15,23)` Down。两者均未设则返回。均不移动发明家、不授物，也不准入顾问。

城堡觐见当前 Astral 成为随从后仍是 resident139；F70 不把他重命名为 B31。重新城镇重建在居民前消费 F70/B/Astral 声明。更早 F74/B/Astral 或 F68/B/Oddler 分配可共存，后来的 F70 声明覆盖 B 查找，而不去重物理随从。其他启用声明影响分配与顺序。因此 F70 未设不证明 B 缺失；F805 不修复 F70。Town139 是 guard，区别于城堡 Astral139 与 B31。

## 早期警告与许可

**已确认：**[完整回调](../../../../../research/chapters/northern-travel-mitula/new-granseal-inventor-and-dynamite-gift.md#complete-callback-and-nod-scene)先检查 F802。未设时，F702 选择早期爆炸物警告（1063–1065）；F702 未设则给发明家的沉思行1055。警告点名洞穴岩石，随后因使用者危险而收回提议。即使传入 F804/F805 也不能覆盖这个早期分支。

F802 已设时，在 F805 前检查 F804。F804 已设显示1122，请求移除洞穴岩石，不搜索物品栏，也不提供替代品。F804 未设/F805 未设时，1114–1115 说 Astral 禁止使用，需要许可。该分支没有 Cannon 搜索或 B 查找。

| 选定状态 | 局部交谈或尝试 |
| --- | --- |
| F802 未设，F702 未设/已设 | 沉思/早期警告；无赠礼。 |
| F802 已设，F804 已设 | 请求移除洞穴障碍，与 F805 或当前物品无关。 |
| F802 已设，F804 未设，F805 未设 | 要求许可；无物品栏测试。 |
| F802/F805 已设，F804 未设，Cannon 搜索失败 | 发明家询问 Astral；B 上下文说危险，需要安全引爆。 |
| F802/F805 已设，F804 未设，Cannon 搜索匹配 | Cannon 适用、B 点头、提议，以及依赖容量的收取尝试。 |

许可是脚本对话，不是 yes/no 提示。局部准入不要求 F761、F70、实际 Astral、存活角色或出战队伍。F805 是谓词，不验证当前预期顾问存在。调用方从不招募、改变 F805/F806、请求旅行/战斗、消费 Cannon 或检查 Dynamite 唯一性。

## 怎样才算持有 Cannon

**已确认：**[物品栏搜索与收件人分析](../../../../../research/chapters/northern-travel-mitula/new-granseal-inventor-and-dynamite-gift.md#inventory-predicate-and-admitted-recipient)区分已加入军团成员资格与出战成员资格。UpdateForce 升序扫描成员 IDs0–29，把所有已加入成员放入 TARGETS_LIST，包括后备与死亡成员。Active 标志另建出战/后备列表。两个选定物品服务均不筛选 HP、职业、当前地图可见性或跟随；B31 在已加入成员扫描之外。

GetItemInventoryLocation 重建该列表，再升序检查成员。它计数全部四个字槽的占用项，却只搜索 `count-1` 降至0 的槽。每项索引先以 `$7F` 掩码，再与未掩码请求字比较，此处为 Cannon `$72`。因此装备/损坏项位不阻碍这个精确请求。首个匹配成员胜出，该成员已检查匹配槽中最高者胜出。穷尽返回完整字 `-1`；调用方以该哨兵值比较 `d0.w`，忽略返回的持有者/槽。字节测试或布尔值不是这个合同。

紧密排列物品栏让所计数前缀覆盖占用槽；有空隙时未必：`[other, NOTHING, NOTHING, Cannon]` 的 count2 使搜索检查1,0，遗漏3。仅未加入成员持有的 Cannon 也被排除。搜索不紧缩或消费物品栏。F761 的早期 Cannon 赠礼历史不能替代当前搜索，F761 未设也不阻挡可搜索 Cannon。两个选定循环都假定已加入列表非空；先执行主体的 DBF 形式没有空列表守卫。此前提畸形时不保证安全未找到/全满行为。

## 适用、点头与提议

**已确认：**匹配 Cannon 后，1119 说它适用。回调关闭对话，运行三命令 cs_54CC6，随后1120 说已获许可并提供爆炸物。短场景让 B31 朝 Up、让 B31 点头并结束。它自身没有对话、许可菜单、标志或物品栏操作。

B 是查找选择器，不是物理槽位，也不保证是 Astral。解析掩码为一个字节，将映射物理索引乘32。普通查找缺失可别名到 physical0；隐藏 `-1` 映射没有安全缺失守卫。变化或重复映射改变实际行动者。字节朝向命令的死亡己方角色守卫只适用于30 以下，因此31 即使映射死亡 Bowie 也绕过。字操作数点头没有存活状态守卫。它请求动画计数器变化、Sleep10/20/10、变换/DMA 与精灵更新，再将计数器归0；不验证 Astral 精灵、不恢复旧计数器，也不等待所有运动。可见点头完成仍未知。

Cannon 搜索失败时，1116 询问 Astral，随后回调借 B 的立绘/台词上下文显示关于危险与安全引爆的1117–1118。[上下文所有者](../../../../../research/chapters/northern-travel-mitula/new-granseal-inventor-and-dynamite-gift.md#text-borrowed-context-and-receiving-acknowledgement)区分预期 Astral209（portrait31/bleep6）与实际映射精灵。B 缺失/变化影响台词、立绘与布置。回调在借用前复制当前 speech，却不在返回前恢复该副本。仅关闭对话不等于恢复 speech 或关闭立绘。

成功点头场景不解析 speech/portrait。因此1120 与收取辅助程序继承发明家包装器上下文：普通 dwarf226 无立绘、bleep6。点头和收取宣布都不切换到旁白，也不使 B 成为收件人。

## 收取空间与确认

**已确认：**调用方通过 ReceiveMandatoryItem 请求 Dynamite `$74`，选项为 `d1=0`。辅助程序重建已加入军团，选择全部四槽计数小于4 的最早成员。该成员可死亡或在后备，不必是 Bowie、Cannon 持有者、出战首领或首个随从。空隙仍提供收取容量。

AddItem 从0 向上搜索全部四槽，找首个掩码索引为 NOTHING 的项。它以 `$807F` 掩码存物；本请求插入普通 Dynamite。其自身结果为插入0、无槽1。ReceiveMandatoryItem 在宣布赠礼或随后返回0 前**不检查**该结果。有效不变物品栏下，前面的计数提供空槽；不能借此前提合并两个结果合同。两个例程都不拒绝已持有 Dynamite 或重复 Cannon。

辅助程序在 AddItem 后写收件人与物品名字索引，请求 item music27 并显示213，宣布 NAME 收取 ITEM。NAME 使用选定战斗员名字，不是 LEADER；ITEM 使用 Dynamite。这是继承的发明家台词/立绘上下文，不是显式旁白选择。宣布包含 NAME/换行/ITEM 替换，自身没有 W1/W2。

[已接受 Cannon 赠礼](../southern-travel-creed/creed-departure-dwarf-recovery-and-cannon-gift.md#接受或推迟-cannon)只检查 Bowie 四槽，为固定 combatant0 插入，使用1728/Sleep100 后设置 F761。本赠礼搜索已加入军团收取空间，使用213 与两个输入调用，辅助程序返回后调用方才设置 F804。两者均不检查 AddItem 结果，也无原子存档边界。

213 后，辅助程序先调用 j_FadeOut_WaitForP1Input，再直接调用 WaitForPlayerInput。前者保留 `d0.w`，通过 PlayMusicAfterCurrentOne 请求先前音乐，等待输入后恢复。虽名含 FadeOut，它不执行调色板淡出。声音服务提交命令并以 Sleep3 轮询；命令边界完成不建立可听短曲完成。这是两个顺序的持有输入服务：每个都接受当前按住的方向/A/B/C/Start，二者之间不要求松开或新按下。一次持有可满足两者。之后辅助程序才关闭对话、清除 `d0.w`、恢复保存寄存器并返回；`d1` 恢复为调用方的0，不保留为物品或收件人结果。

所有已加入成员物品栏全满时，option0 的辅助程序立即返回1。不运行 AddItem、物品短曲、213 或强制丢弃。此调用方不准入另一非零选项丢弃路径。调用方测试 bit0：奇数结果显示1729，保留 F804 未设；普通0 设置 F804。其谓词会接受2，但这个 option0 调用通常不能选择那条其他路径。因此满手允许后续合格重试。

## 对话与局部返回

**已确认：**回调行使用显式 ID `txt`，不是过场文本游标递增。已接受文本关联保留全部13 个回调字面量加213，包括警告1063–1065、许可1114–1118、适用1119、提议1120、满手1729 与完成分支1122。换行与 W1/W2 仍是文本控制；W1 在 VInt 等待后接受方向/A/B/C，而两个直接收取服务也接受 Start。W2 带有继续/确认音效行为。均不创建许可选择。

直接 `txt`/`clsTxt` 不使用过场跳过锁存。点头脚本在入场清除锁存，但没有可跳过文本或 csWait。其结束在对话打开时有条件地等待视野滚动，清除滚动速度并返回。跳过场景不能成为省略直接赠礼文本或输入服务的依据。

[实体包装器与有序返回](../../../../../research/chapters/northern-travel-mitula/new-granseal-inventor-and-dynamite-gift.md#ordered-return-retries-and-changed-state)重新选择当前配置/事件表，解析交互行动者，保存其映射物理身份、朝向与选项。行 bit0 可选等待一个 VInt，让该行动者朝传入相对方向的反向；default0 禁用，后期1 启用。它在回调期间清除 VInt_UpdateEntities。这些发明家行的 bit1 未设，因此不请求恢复保存朝向。回调返回时关闭立绘、关闭对话、重新启用实体更新、恢复寄存器并返回。不恢复借用 speech/name RAM、不等待全部行动者，也不证明下一次可用控制读取。实体清理区别于前序区域（事件）包装器等待。

## 变化状态、重复与中断前缀

**已确认：**物品写入先于宣布、音乐/输入服务、helper0、调用方 bit0/F804 与包装器返回。仅提议文本不建立插入、完成标志或持久性。

| 变化状态或停止点 | 后果 |
| --- | --- |
| F804 已设，即使 F805 未设或 Dynamite 丢失 | 完成分支胜出；不修复缺物或重新获取。仍需 F802 已设才能到达。 |
| F761 已设但 Cannon 缺失/遗漏，或 F761 未设而 Cannon 可搜索 | 实际搜索独立于历史选择拒绝/赠礼尝试。 |
| F70 未设/F805 已设，或 B 缺失/别名 | 守卫仍通过；实际查找控制点头/台词。不自动安全拒绝缺失顾问。 |
| Bowie 满，后续已加入后备/死亡成员有空间 | 最早有空间的已加入成员收取；全满则推迟而不设 F804。 |
| 已有 Dynamite，F804 未设 | Cannon 与容量准入时可再插入。清除 F804 强制重复没有唯一性保护。 |
| AddItem 后、helper0/F804 前中断 | 物品可存在而 F804 未设、确认未完成；后续重试可再插入。 |
| helper0 后、F804 前中断 | 辅助程序已返回，尚无调用方标志。 |
| F804 后、包装器返回前中断 | 赠礼分支可被抑制，而清理与可用控制仍未完成。没有回滚修复前缀。 |
| F804 后重新进入城镇 | F701/F702 仍选择人口；F802 选择 init，F70 独立贡献 B。F804 只改变此回调分支。 |
| 当前城镇中配置/随从标志变化 | 表选择可改变，实际行动者仍保留旧映射，直到另行重建。 |

接续 [Janet 招募与离开](new-granseal-janet-recruitment-and-departure.md)，了解独立相遇、军团成员资格、临时跟随与局部返回；Dynamite 赠礼不是前提。

Janet 区域（事件）准入是独立 F805 已设/F806 未设边界；F804 不是前提。本指南不解释其招募场景、不行走下一旅程，也不消费 Dynamite/爆破 North Cave。自然可达、传入模式/标志/军团/排列/行动者、实体/DMA/文本/立绘/音乐/输入服务完成、可见许可/收取、可用控制与存档/读档持久性仍为**未知**，不产生自动运行时队列。

## 证据审查与保留结果

**已确认审查边界：**本综合以选定公开证据检查已接受 #969 所有者与前序人口、行动者、物品、输入、音乐所有者。`sf2-map-event-item-transactions-static-v1`（[fixture](../../../../../../tests/fixtures/h2/map-event-item-transactions-static-v1.json)、[schema](../../../../../../schemas/h2/map-event-item-transactions-static-fixture.schema.json)、[消费者](../../../../../../src/sf2tool/h2/map_event_item_transactions.py)）保留49 操作物理调用方、服务别名 `$81D0`→`$9146` 与 `$4F48A`、Cannon/Dynamite 请求寄存器及哨兵值/bit0 分支边。其40 记录 default 上下文共享物理尾部，却从立即返回处进入；上下文重复调用记录不是两次执行。Schema 断言记录形状；消费者检查操作数身份与有序关联。均不断言被调用方完整物品栏算法或自然交互可达。

选定 `sf2-map-events-static-v1`、`sf2-map-init-static-v1`、`sf2-map-script-engine-static-v1` 的 fixture/schema/消费者检查验证城镇回调路由、配置/init 优先级与朝向/点头操作数/处理器边界。被调用方算法与字面量/控制 token 关联仍是 #969 已接受固定源码发现。选定公开表面未发现矛盾。所属新源码命令仍为 `uv run sf2 h2 map-init`、`uv run sf2 h2 map-events`、`uv run sf2 h2 map-script-engine`、`uv run sf2 h2 map-event-item-transactions`。原综合切片工作树缺少固定上游/保留数据对：不声称采集、重新生成或新比较。

[#969 交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/969#issuecomment-6179831287)与 [PR971 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/971#issuecomment-6179847971)保留 head c4c138e4/tree02f3543e/baseaf7a1e65、CI38647099720（scope16s、research32s）、作者0.081s 与独立0.086s 的不变18,285,979 字节数据对比较、完整49/40 操作关联与14 文本关联。[#970 更新交接](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/970#issuecomment-6179905336)与 [PR972 验收](https://github.com/FrankHZ/md-sf2-reverse-engineering/pull/972#issuecomment-6179913615)保留原始5625ee54/CI38647220538（scope19s）、最终5849263a/treec56b4c2c/base9c65746c/CI38647686363（scope11s）、529 链接与独立审查。其基线更新/独立范围澄清不是研究修正。

前序的[保留审查](new-granseal-return-and-astral-following.md#证据审查与保留结果)保留 #965 的 Battle19/32 字节步长修正、不同数据对回执、原始/最终 CI、链接的失败/诊断/未知项、#954 回调修正及 #962 Fairy 修正。所有已完成结果继续保留；不重置失败或称为中断。任何证据矛盾返回研究所有者，不扩展 Layer B 所有权。

原切片 Issue 交接记录直接完整正文/差异、链接/锚点、导航、UTF-8、精确所有权/私有边界及 `git diff --check`、配置后的干净已提交 planner 与实际精确 head CI。文档政策要求这些直接检查，不要求全套套件或翻译运行。
