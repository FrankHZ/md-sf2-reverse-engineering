# 对话命令静态合同

- **已确认的原版行为：** 下面六个源码命令布局、处理器分发、游标/名称索引状态写入、源码标注的修饰位、关闭/清除调用顺序，以及有界的实体到地图精灵对话属性接缝。
- **推断的原版行为：** 此处没有提升任何推断。
- **未知的原版行为：** 正常剧情可达性、渲染文本/立绘/台词/控制器时序，以及未垫片化的服务完成/重复/持久化。
- 证据日期：2026-07-31
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`
- 可追溯性：`tests/fixtures/h2/map-script-engine-static-v1.json` 中的 `sf2-map-script-engine-static-v1`；以及
  `tests/fixtures/h3/map-script-dialogue-v1.json` 中的 `sf2-map-script-dialogue-runtime-v1`；`src/sf2tool/h2/map_script_engine.py`；
  `src/sf2tool/h3/map_script_dialogue.py`；以及 `docs/research/common-scripting.md`。

> 本文件是 [`dialogue-system.md`](../dialogue-system.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 已确认的静态合同

地图脚本对话命令族有六种主要宏形式：`nextSingleText`、`nextSingleTextVar`、`nextText`、`nextTextVar`、`textCursor` 与 `hideText`。它们源码定义的操作码为 `$00` 到 `$04` 与 `$09`；对应的物理命令宽度分别是 4、6、4、8、4 与 2 字节。测试夹具同时保留发出的操作数域与这些物理宽度，而不是把处理器读取当作存储字节的替代品。

全部 2,883 次调用都被保留为对现有地图脚本程序命令的有序引用，含零计数总覆盖全部 304 个程序。源码语料包含 2,058 条 `nextSingleText`、零条 `nextSingleTextVar`、577 条 `nextText`、零条 `nextTextVar`、234 条 `textCursor` 与 14 条 `hideText` 命令。`textCursor` 源码操作数范围从 240 到 4,233；独立以源码/ROM 检查的文本行域从 0 到 4,266 连续。这是 ID 域验证，不是对已解码对话内容或显示顺序的主张。

`csc00_displaySingleTextbox` 与 `csc02_displayTextbox` 在显示路径之前测试过场文本跳过标志。四个显示处理器把打包的修饰/实体字与 `-1` 比较；它们都在直接实体对话属性使用方之前调用立绘辅助函数，调用 `DisplayText`，然后递增 `CUTSCENE_DIALOG_INDEX`。两个 `nextSingle*` 处理器随后调用立绘关闭路径、清除文本，并以源码值 10 调用 `Sleep`，而两个延续处理器不包含该关闭/睡眠序列。两个 `*Var` 处理器都包含对源码命名的对话名称索引状态的两次字读取。零使用 `nextSingleTextVar` 宏的四个操作数字节与其处理器的两次字读取仍是独立的静态事实；本合同不为这个未使用的形式发明运行时解释。

静态调用方审计保留全部六个对话处理器与 `csc1D_showPortrait`，包括两行零/零调用方。它即使在指令目标与有效目标相等时也分别保留直接指令与解析后有效目标身份，并根据其解析后的源码路径把辅助函数分类为内部、实体使用方分类为外部，而不是指派行为角色。

`textCursor` 把其一个字写入 `CUTSCENE_DIALOG_INDEX`。`hideText` 在其文本清除宏之前调用立绘关闭目标。`csc1D_showPortrait` 读取同一个打包字，并依次测试字位 15 然后位 14。这些处理器使用位置推导出高字节 `handlerTestedModifierByteMask` `$C0`；打包字 `$FFFF` 哨兵值之外的已观察修饰字节对照该使用位置推导掩码检查。宏注释把修饰字节 `$80` 标注为 `display on right`、`$40` 标注为 `mirrored`；这些原始标签作为标签保留，而不被改写成渲染器合同。`$FF` `undisplayed` 标签与处理器已确认的全字 `-1` 比较保持分离。

直接实体接缝是 ROM 地址 284,216 处的 `GetEntityPortaitAndSpeechSfx`。其命名段用解析出的 `COMBATANT_MASK_ALL`（255）掩码 `d0`，然后取得实体地址并加载其地图精灵字节。地图脚本合同仅通过兄弟合同的 ID、固定提交、ROM 哈希、源码路径与地址接入现有 119 行、478 字节的精灵对话表；它不复制已解码文本，也不把兄弟黄金测试夹具作为证据。

## 已确认的运行时边界

单次启动的 `sf2-map-script-dialogue-runtime-v1` 测试夹具保留 21 个处理器局部用例：全部六对入口/返回 PC、A6/栈边界、跳过准入、按源码划分的打包输入、两个受控零源码 `*Var` 布局、游标边界、关闭路径、有序直接调用身份与含零计数目标数、直接状态写入，以及会话控制的调用寄存器字。H3 观察器首先捕获入口/调用/目标/返回 PC，并且只通过唯一的受保护源码/H1 地址映射解析调用标签。重制适配器必须把这些事实保留为命令/服务接缝。显式的 D0/D1/D2 trampoline（蹦床）输入与 RTS 服务垫片是测试台控制，而非原版调用方/服务行为。

剩余的原版问题恰好是 `docs/research/common-scripting.md` 中的三组 `map-script-dialogue/*` 队列；重制版绝不得从这个处理器局部矩阵推断视觉立绘位置、音频播放、文本完成、控制器时序、剧情可达性或持久化。

## 重制边界

重制版可以把命令解码、对话行选择、名称索引替换、立绘查找与呈现调度建模为独立服务。保真工作可以保留已确认的命令与状态更新顺序。渲染、音频调度、输入等待、无障碍行为，以及赋给原始标签的任何语义含义，在分组运行时矩阵提供观察之前，仍是显式的现代选择。
