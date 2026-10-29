# 对话命令与已到达文本等待合同

<a id="对话命令静态合同"></a>

- **已确认的原版行为：** 下文六种源码命令布局、处理器分发、游标/名称索引状态写入、源码标注的修饰位、关闭/清除调用顺序，以及有界的实体到地图精灵对话属性接缝；还包括下文另有证据支持的 W1 轮询规则与已到达文本 483 绑定。
- **推断的原版行为：** 下文明示的有界开场前序来源。
- **未知的原版行为：** 具名观察之外的可达性、已到达等待处完整的已启用服务与立绘状态、渲染时序，以及更广泛的完成/重复/持久性。
- 证据日期：2026-07-31（命令矩阵）；2026-09-24（既有已到达等待证据绑定）。
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`
- 可追溯性：`tests/fixtures/h2/map-script-engine-static-v1.json` 中的 `sf2-map-script-engine-static-v1`；以及
  `tests/fixtures/h3/map-script-dialogue-v1.json` 中的 `sf2-map-script-dialogue-runtime-v1`；`src/sf2tool/h2/map_script_engine.py`；
  `src/sf2tool/h3/map_script_dialogue.py`；以及 `docs/research/world/common-scripting.md`。
  已到达等待绑定则使用[开场对话证据](../../../../research/chapters/grans-opening/map3-messenger-acceptance.md#opening-dialogue-return-and-subsequent-actions)中的保留观察与固定源码。

> 本文件是 [`dialogue-system.md`](../../../contracts/interface/dialogue-system.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../../glossary.md) 的术语规则（R1–R7）。证据标签按 R1 使用固定中文译法；源码标识符、fixture ID 与路径按 R2 原样保留。

## 已确认的静态合同

地图脚本对话命令族有六种主要宏形式：`nextSingleText`、`nextSingleTextVar`、`nextText`、`nextTextVar`、`textCursor` 与 `hideText`。它们源码定义的操作码为 `$00` 到 `$04` 与 `$09`；对应的物理命令宽度分别是 4、6、4、8、4 与 2 字节。测试夹具同时保留发出的操作数域与这些物理宽度，而不是把处理器读取当作存储字节的替代品。

全部 2,883 次调用都被保留为对现有地图脚本程序命令的有序引用，含零计数总覆盖全部 304 个程序。源码语料包含 2,058 条 `nextSingleText`、零条 `nextSingleTextVar`、577 条 `nextText`、零条 `nextTextVar`、234 条 `textCursor` 与 14 条 `hideText` 命令。`textCursor` 源码操作数范围从 240 到 4,233；独立以源码/ROM 检查的文本行域从 0 到 4,266 连续。这是 ID 域验证，不是对已解码对话内容或显示顺序的主张。

`csc00_displaySingleTextbox` 与 `csc02_displayTextbox` 在显示路径之前测试过场文本跳过标志。四个显示处理器把打包的修饰/实体字与 `-1` 比较；它们都在直接实体对话属性使用方之前调用立绘辅助函数，调用 `DisplayText`，然后递增 `CUTSCENE_DIALOG_INDEX`。两个 `nextSingle*` 处理器随后调用立绘关闭路径、清除文本，并以源码值 10 调用 `Sleep`，而两个延续处理器不包含该关闭/睡眠序列。两个 `*Var` 处理器都包含对源码命名的对话名称索引状态的两次字读取。零使用 `nextSingleTextVar` 宏的四个操作数字节与其处理器的两次字读取仍是独立的静态事实；本合同不为这个未使用的形式发明运行时解释。

静态调用方审计保留全部六个对话处理器与 `csc1D_showPortrait`，包括两行零/零调用方。它即使在指令目标与有效目标相等时也分别保留直接指令与解析后有效目标身份，并根据其解析后的源码路径把辅助函数分类为内部、实体使用方分类为外部，而不是指派行为角色。

`textCursor` 把其一个字写入 `CUTSCENE_DIALOG_INDEX`。`hideText` 在其文本清除宏之前调用立绘关闭目标。`csc1D_showPortrait` 读取同一个打包字，并依次测试字位 15 然后位 14。这些处理器使用位置推导出高字节 `handlerTestedModifierByteMask` `$C0`；打包字 `$FFFF` 哨兵值之外的已观察修饰字节对照该使用位置推导掩码检查。宏注释把修饰字节 `$80` 标注为 `display on right`、`$40` 标注为 `mirrored`；这些原始标签作为标签保留，而不被改写成渲染器合同。`$FF` `undisplayed` 标签与处理器已确认的全字 `-1` 比较保持分离。

直接实体接缝是 ROM 地址 284,216 处的 `GetEntityPortaitAndSpeechSfx`。其命名段用解析出的 `COMBATANT_MASK_ALL`（255）掩码 `d0`，然后取得实体地址并加载其地图精灵字节。地图脚本合同仅通过兄弟合同的 ID、固定提交、ROM 哈希、源码路径与地址接入现有 119 行、478 字节的精灵对话表；它不复制已解码文本，也不把兄弟黄金测试夹具作为证据。

## 已确认的运行时边界

单次启动的 `sf2-map-script-dialogue-runtime-v1` 测试夹具保留 21 个处理器局部用例：全部六对入口/返回 PC、A6/栈边界、跳过准入、按源码划分的打包输入、两个受控零源码 `*Var` 布局、游标边界、关闭路径、有序直接调用身份与含零计数目标数、直接状态写入，以及会话控制的调用寄存器字。H3 观察器首先捕获入口/调用/目标/返回 PC，并且只通过唯一的受保护源码/H1 地址映射解析调用标签。重制适配器必须把这些事实保留为命令/服务接缝。显式的 D0/D1/D2 trampoline（蹦床）输入与 RTS 服务垫片是测试台控制，而非原版调用方/服务行为。

`docs/research/world/common-scripting.md` 中的三组 `map-script-dialogue/*` 队列保留处理器矩阵的未决问题。重制版不得从该矩阵推断视觉立绘位置、音频播放、文本完成、控制器时序、剧情可达性或持久性。下文独立来源的绑定不扩大服务垫片的观察范围。

## 已到达的 W1 消费者绑定

**已确认（固定源码与保留观察）：** 开场 `cs_5145C` 依次显示文本 510、511、483。其有序源码 token 分别包含 W2、W2、W1。既有观察把 `DisplayText` 目标 483 与 `symbol_wait1`、接受输入的 `loc_65B4` 读取及该文本调用返回配对。后续基于 trap 的显示也以 483 为目标，而过场游标仍为 484；游标不是当前文本身份。[研究所有者](../../../../research/chapters/grans-opening/map3-messenger-acceptance.md#reached-text-wait-source-binding)保留精确边界与复现路线。这证明具名已到达消费者，不证明全部自然文本等待或其间的服务状态。

必须保留完整有序 token 流，以及每个等待在流中的位置。不得推断每个显示行只有一次确认、每个 `ShowText` 只有一次等待，或从 `Single`/`Continued` 模式推断等待。同一文本中的多个等待仍是不同消费者。文本解码与字形数据仍归[文本与字体数据](../../../contracts/interface/text-and-font-system.md)所有；识别控制 token 无需把私有对话复制进公开 fixture。

### 轮询顺序与不同消费者

**已确认（静态源码）：** 在固定修订版中，`disasm/code/common/scripting/text/textfunctions_1.asm:symbol_wait1` 保存并清除逐字显示字节。随后 `loc_659C` 的每次迭代：

1. 针对共享主种子，以范围 256 调用 `GenerateRandomNumber`；
2. 把结果字节写入 `RANDOM_SEED_COPY`；
3. 调用 `WaitForVInt`；
4. 在 `loc_65B4` 读取 `CURRENT_PLAYER_INPUT`，掩码保留方向与 A/B/C 按键；
5. 输入为零则重复，非零则恢复逐字显示并继续 token 解析。

接受输入的迭代也包含抽样、复制与等待。输入接受不能绕过此前导。等待中任何已启用服务均在本次轮询复制之后、输入测试之前发生；其门条件与顺序需要绑定到具体调用方。仅凭回调帧或辅助函数名称，不能建立恰好一次实体/立绘更新的计数。

| 消费者 | 已确认规则 | 观察限制 |
| --- | --- | --- |
| W1 | 抽样/复制、等待、输入测试；接受时恢复逐字显示。 | 已观察文本 483 的具名接受读取；未观察完整实时服务/立绘状态。 |
| W2 | `ParseSpecialTextSymbol:@wait2/loc_6472` 抽样/复制，调用 `sub_64A8`，等待并测试输入；接受时请求 `SFX_VALIDATION`，以零调用 `sub_64A8`，并恢复逐字显示。 | 文本 510/511 的源码 token 与文本返回有依据。此 W1 观测手段未观察其精确 W2 接受读取；W2 循环入口回调不是之后的输入读取。 |
| 普通 JOIN 输入 | `input.asm:WaitForPlayerInput` 在等待前测试输入；接受的输入不增加 W1/W2 前导或接受轮询 tick。 | [自然 JOIN 证据](../../../../research/chapters/grans-opening/map3-messenger-acceptance.md#natural-join-audio-and-input-boundary)绑定音乐处理后的具名辅助函数；音频结束/交错与完整服务门条件仍独立。 |

JOIN 文本 446/447 没有 W1/W2 token。其调用方在 `FadeOut_WaitForP1Input` 的音乐处理后提供独立普通输入辅助函数。不得在该辅助函数前增加文本确认、对其应用 W1 的 RNG 前导，或从进入文本等待推断音频完成。

### 逻辑输入所有权与实现缺口

依据 [ADR 0010 Option A](../../../../decisions/0010-map3-battle01-product-acceptance.md#evidenced-gameplay-waits-accepted-option-a)与[连续 Wait 合同](../../../contracts/milestones/map3-battle01-continuous-scenario.md#evidenced-gameplay-waits)，接受输入的 W1 轮询是消费该次确认所必需的工作。额外非接受轮询，只有在已识别且符合资格、服务门条件/顺序已准入的消费者处，才可成为玩家 Wait。仅揭示输入、呈现交付完成、接受轮询与可选非接受 Wait 是不同事件。交付延迟不提供额外轮询或 tick 欠账，也不能删去必需工作。这些规则不规定墙钟速率或历史轮询配额；它们保留共享 RNG 与全部选定玩法断言。

**已确认（有界重制实现）：** 实体事件服务受抑制的消费者中的有序 W1 区段，以及显式[已绑定探索文本配置](../../../../../remake/docs/application/session-and-programs.md#bound-field-text-work)，现在各自负责接受输入的抽样/复制/服务/输入序列。后者还执行常规字形、窗口、视图辅助与 W2 工作，独立于实际显示交付。通用未绑定 `ShowText` 仍使用 `DialogueWait`；它不建立原版时序。JOIN 发出 `waitForAcknowledgement: false`，之后发出 `wait-text-input`；其先测试输入的辅助函数仍独立。已完成的有限 JOIN 诊断在音频交付期间记录 5,313 个服务结果与 572 次最小 LCG 等效推进；逐实体归因与通用跨时钟调度仍 **未知**；保留[诊断所有者](../../../../../remake/docs/evidence/retained-comparisons.md#retained-first-control-opportunity-alignment)。更早的 5,000 帧超时是已完成的失败，不是需要重启的中断运行。

**已确认（选定的受控开场）：** [有界 R1 回读](../../../../research/chapters/grans-opening/map3-messenger-acceptance.md#controlled-opening-scalar-readback)在输入前、各值首次适用的源码读取处，以及脚本返回清除视图之前，观察到 mouth0/view override0。这为这些受控准入值提供直接证据。
**推断（自然前序来源）：** 前面的 StartWitchScreen/reset 前序来源仍在该观察之外；更早的 speed2/settings 构造与后续保存字节佐证保留其[绑定证据](../../../../research/chapters/grans-opening/map3-messenger-acceptance.md#opening-field-text-settings-and-view-binding)。
**未知：** 该配置之外完整的原版立绘/服务时序、硬件呈现与整条路线 9A/H4 仍开放。仅凭说话者提示、立绘身份或已清除的逐字显示字节，不能准入活动或未知立绘。下文源码绑定的实体事件规则显式准入已注册立绘。重制测试/原生观察证明消费者，不增加原版运行时观察。

后续行为验收必须使用已准入的源码驱动消费者，并检查不同种子与行动者阶段、服务启用/禁用、零/一/多次额外 Wait 后接必需接受轮询、过期 token/修订，以及同一文本中的多个等待 token。在即时/可调揭示与仅揭示 Confirm 下，比较相同语义 Wait/确认流；状态/RNG 必须一致，实际呈现条件仍须成立。W2 与普通输入用例保持独立。此处不豁免任何既有失败、黄金基准或未决首次传送时序字段。

### W2 组合语义验收

当前语义要求可由独立原版处理器规则、适用调用方/服务门条件的证据，以及实际 token/输入/服务/指示器/确认音效/继续执行观察共同建立。历史内部接受读取的直接回调不是该组合证明的先决条件。它仍是独立 **未知**；绝不能把文本 510/511 返回与 `loc_6472` 入口重新标记为后续输入读取。如果缺失的调用方或门条件可能改变逻辑结果，该次出现应保持 **未知**，不得从文本 ID 或通过的测试推断。

**已确认（静态源码）：** 固定 SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6` 的 `textfunctions_1.asm:@wait2/loc_6472/sub_64A8` 建立上文抽样/复制 → 指示器 → 等待 → 掩码输入读取序列。指示器从 20 开始，计数器 >=7 时可见，递减并在 0 时重置；HIDE_WINDOWS 强制其隐藏分支。接受时请求 validation67，并在 token 继续前清除指示器。指示器辅助函数不增加等待或随机抽样。实体包装器的 CLEAR_FLAG 抑制实体更新，而 `code/common/tech/interrupts/trap6_mapscript.asm:Trap6_TriggerAndExecuteMapScript` 在执行嵌套脚本前重新激活它们。返回不会撤销该激活。因此，仅凭 EntityEventContext 不足以决定 NPC 服务是否运行。

**已确认（有界实际消费者）：** 保留的成功默认键盘 A 组（`local/issue534/caller-resource-cohort-01/variant-A-02/actual.json`）包含以下 16 次 W2 接受。程序位置是已准入的内容游标位置；token/序号值标识证据，不是生产准入规则：

| 源码调用方/内容 | 已到达的 W2 文本 ID | 实际实体服务与调用方 |
| --- | --- | --- |
| map03 `scripts_1.asm:cs_5145C` | 510,511 | 已启用的区域调用方；立绘关闭 |
| map03 `s2_entityevents.asm:Map3_EntityEvent0/15` | 512,500 | 实体包装器抑制服务；已注册的打开立绘 |
| map03 `s3_zoneevents.asm:byte_50E96` 与 `scripts_1.asm:cs_5149A` | 514,515,517,518,526 | 已启用的区域调用方；已注册的打开立绘 |
| map20 `s6_initfunction.asm:cs_53996` | 2193 | 经 Trap6 的初始化脚本；已启用，已注册的打开立绘 |
| map19 `s2_entityevents.asm:Map19_EntityEvent12/cs_52F0C` | 575 两次，576 | 实体包装器后经 Trap6 激活；已启用，已注册的打开立绘 |
| battle01 `cs_beforebattle.asm:bbcs_01` | 2292,2299,2303 | 已启用的战前脚本；已注册的打开立绘 |

全部 16 次接受均有符合资格、已交付的 FieldTextWait、实际键盘 Confirm、恰好一次 `rng-text-w2`、一次接受服务机会、`text-w2-input(accept)`、`text-w2-accepted`，以及新 continuation token。14 次刻意中性轮询仍独立。16 条匹配 validation67 回执报告真实播放 `started`，通过会话与**整个 submit 结果**的修订连接。`ExplorationSessionView.Send` 在 Submit 返回后请求声音，因此回执修订可能晚于内部接受观察。要求它与内部事件修订相等会丢弃有效回执。

保留的同次 submit 样本显示 14 次接受的 indicator0/hidden。最后两次战前接受则由其下一文本 2300/2304 的输入样本显示 indicator0/hidden；未记录中间的同次 submit 投影。显式接受事件、生产清除/继续行为，以及随后隐藏状态共同组成语义边界；精确中间宿主绘制/时间仍 **未知**。全部选定就绪状态均为 HideWindows=false/Scrolling=false；变化的视图/指示器状态保留独立引擎行为覆盖。

离线审查仅从既有 A 组选取 W2 token/输入/结果记录与周围相关状态，并复用其已保留音频回执。保留原数组索引与来源元数据。有界选择保留在 `local/issue534/closure-preflight-01/current-A-w2.jsonl`，附选择回执；四个后期上下文样本位于 `current-A-w2-tail.jsonl`；`semantic-closure-01/w2-occurrences.json` 绑定 16 次接受。复现这些选定事实无需新增原版或完整路线运行。

`ExplorationTextWaitTests.PollCopySurvivesNpcThenBlinkAndMouthDraws` 在区域/实体上下文中启用 NPC，同时进行眨眼/嘴部抽样，执行中性与接受轮询。它独立检查轮询副本、最终共享种子、单次服务/抽样、指示器清除与继续执行。既有抑制服务、指示器周期、原始包装器与交付测试覆盖互补用例。使用已配置 SDK 与 Engine.Tests 筛选 `FullyQualifiedName~ExplorationTextWaitTests` 复现。测试证明引擎行为；A 组证明实际消费者。两者均不创建新的原版运行时观察，也不清除无关 H4 行。

## 已绑定的中性文本延迟与旁白

**已确认（静态源码）：** 固定 SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6` 的 `disasm/code/common/scripting/text/textfunctions_1.asm:symbol_delay1/loc_65CC/loc_65D8/loc_65F0` 加载计数器 21，保存并清除 `CURRENTLY_TYPEWRITING`，在中性输入、mouth-control0 下执行 `WaitForVInt` 后接 DBF，然后恢复保存值。这意味着 22 次源码等待服务，不是 22 个独立测量的自然硬件帧。非零 mouth-control 跳过等待；掩码后的持续按键输入可缩短源码循环。

已绑定的重制消费者必须把 D1 保留为类型化 token，不产生字形、游标推进、首字形状态变化、W1/W2 抽样/复制或确认。其中性延迟必须在逐字显示被清除时执行 22 次既有公共服务，并在下一 token 前恢复实际保存的逻辑标志。非零 mouth-control 不贡献延迟服务。token 顺序、重复/末尾/输入前后的位置、符合条件的实体/立绘 RNG，以及单次/批量服务等价，保留各自既有所有者。揭示完成与仅揭示 Confirm 不得代替源码的持续逻辑输入。此现代消费者仍未准入持续输入缩短；D3 与其他未绑定控制仍为 Unsupported。旧服务受抑制的 W1 捷径不能通过忽略服务来消费 D1。

**已确认（静态源码）：** `mapscriptengine_2.asm:csc00_displaySingleTextbox` 在操作数 FFFF 时跳过立绘查找，写入 `CURRENT_SPEECH_SFX=0`，然后执行视图等待/文本及普通关闭/Sleep10 尾部。显式 sourceFF/null/no-event-speaker 旁白可以无行动者地使用已绑定文本；任意缺失/无效行动者仍为错误。跳过查找必须保留已经打开的立绘，不能据此证明立绘不存在。即使保留立绘，实际台词音效也必须静音；显式源码关闭仍负责其生命周期。[已绑定引擎执行](../../../../../remake/docs/application/session-and-programs.md#bound-map-initialization-lifecycle)与[原生验收](../../../../../remake/docs/verification/field-programs.md#bound-map-initialization-observation)验证已准入消费者。原版自然节奏、台词调度与整条路线 H4 仍 **未知**。

## 实体事件立绘服务

**已确认（静态源码）：** 在 SF2DISASM `c834c652b6862bc5679fd7f69a38a7093206efc6` 中，`portraitwindow.asm` 在实体包装器的朝向等待前打开立绘；移动完成后注册，关闭移动前移除注册。已打开立绘予以保留。新计数器为 blink20/mouth6。`portraitfunctions.asm:VInt_PerformPortraitBlinking` 在 blink3/0 时更新眼睛，并在 0 时抽取主 RNG120+30。逐字显示推进嘴部，在 5 时选择替代瓦片，在 0 时用 RNG5+10 重置正常瓦片；不逐字显示时 mouth<=5 立即重置。眨眼先于嘴部。逐字显示与 mouth-control 输入缩短是独立门条件。`textfunctions_1.asm:DisplayText` 只在 `CreateDialogueWindow` 返回后设置逐字显示。新窗口清除/移动等待必须保留传入值，同时眨眼与既有非逐字显示 mouth<=5 重置仍有效。复用窗口立即返回；空文本/仅 W 的文本可在无中间服务时设置再清除逐字显示。[研究绑定](../../../../research/chapters/grans-opening/map3-messenger-acceptance.md#classroom-portrait-entity-event-binding)负责精确符号、源码顺序与已到达示例。

已准入实现必须保留单一实时立绘身份、打包位置与镜像标志、眼/嘴瓦片映射、注册生命周期与共享 RNG 效果。未知立绘状态不能回退为 Closed 或静态脸。W1/W2 在已启用 NPC/立绘服务前复制轮询结果；之后的抽样不得覆写该副本。打开/关闭移动请求既有 menu-switch65 乐段。这不建立新的台词策略。

实时实体启用标志在包装器入口/抑制及 Trap6 激活时变化；它必须跨脚本返回保留。源码地图脚本结束时，若对话存在则等待视图，随后清除覆盖；原生子程序返回仍独立。包装器返回时恢复朝向，移除/关闭立绘，关闭对话，最后以实体启用状态释放控制。若没有脚本激活实体，抑制持续到关闭尾部结束。

**已确认（有界重制）：** [实体事件消费者](../../../../../remake/docs/application/session-and-programs.md#bound-entity-event-portrait)对受支持实时状态/内容使用这些规则，在三种显示设置下以相同语义输入到达 Sarah 首次课堂调用方返回。下文源码区域节单独准入首次介绍。
**未知：** 后续区域立绘、摄像机、JOIN 与战斗消费者服务调度、原版 DMA/呈现时序，以及既有整条路线验收缺口。本次准入不增加原版运行时观察。

## 源码区域调用方直至返回

**已确认（静态源码）：** 固定 `ProcessMapEventType6_ZoneEvent` 安装玩家 `eas_Init` 后，立即调用 `RunMapSetupZoneEvent`。请求在玩家实体服务期间产生；其余实体完成本轮处理后，处理器才运行。实体阻挡先于请求；地图通行性检查后于请求。因此，受地图阻挡的标记仍可调用处理器，被实体阻挡的目标则不能。初始化安装必须保留当前位置、目的地、速度与定时器；其动作在后续服务执行。[空间所有者](../../../../research/chapters/grans-opening/map3-controlled-start-egress-transition.md#source-zone-request-order)记录源码符号与合法楼梯示例。

区域调用方必须与实体交互区分：它不隐含交互朝向/抑制，也不隐含朝向恢复。实际实时实体启用标志必须贯穿原生代码与脚本调用。复用已准入立绘/文本/视图工作，包括传入逐字显示、已注册立绘保留、未知状态拒绝与轮询副本保留。

处理器返回后，先关闭/移除立绘，再关闭对话，然后执行一次无条件 VInt 机会。只有之后才比较玩家物理 X/Y 与其 X/Y 目的地；不相等时重复 VInt/重新检查。脚本空闲与聚合 Busy 不得替代该谓词。空/跳过处理器或已到达的玩家仍须执行无条件机会。只有此调用方尾部完成后，才恢复普通探索控制。

**已确认（有界重制）：** [源码区域消费者](../../../../../remake/docs/application/session-and-programs.md#source-zone-caller)使用单一实时调用方权威，在三种显示设置下，从保留的已绑定起点到达首次 Astral 介绍返回。这包括更早开场区域的同一规则。
**未知：** 后续区域/摄像机/选择/JOIN/战斗消费者调度与原版硬件时序。不隐含任何新的原版运行时观察。

## 重制边界

重制版可以把命令解码、对话行选择、名称索引替换、立绘查找与呈现调度建模为独立服务。准入消费者时，必须保留已确认的命令/状态顺序及上文有界输入消费者规则。渲染与宿主时序选择仍受已接受的 8D/9A 语义约束；它们不授权删去影响玩法的轮询工作，也不授权在调用方/服务绑定仍未知时发明调度器。处理器局部矩阵本身不提供额外的完整消费者或渲染保真保证。
