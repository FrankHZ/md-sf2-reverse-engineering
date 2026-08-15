# 精灵对话属性数据合同

- **已确认原版结构：** 完整 119 记录地图精灵对话属性表、独立双字节终止符、H1 绑定地址、source/ROM parity 与下文描述的受限查找消费者。
- **推断原版行为：** 此处不提升任何内容。
- **未知原版行为：** 自然回退可达性、调用方准入、立绘抑制与渲染、语音 SFX 播放与时序、文本/窗口/输入同步、提供给该查找的特定实体地图精灵字节的运行时与调用方特定溯源，以及玩家面向含义。
- 重制状态：实现无关 Phase 3 数据/查找合同；尚未选择对话呈现、立绘渲染器、语音或哔声策略、本地化流程、可访问性行为或许可内容包。
- 证据日期：2026-08-08
- 源码基线：`ShiningForceCentral/SF2DISASM`
  `c834c652b6862bc5679fd7f69a38a7093206efc6`

> 本文件是 [`sprite-dialogue-property-data.md`](../../contracts/sprite-dialogue-property-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 合同边界

本合同定义实体地图精灵字节到一立绘字节与一语音 SFX 字节的原版映射的静态身份与私有导入边界。它拥有：

1. 有序 119 记录表、其四字节记录布局、地址范围与 hash；
2. 那些记录后的独立 `0xFFFF` 字终止符；
3. 地图精灵键唯一性、立绘值与语音 SFX 聚合事实与保留字节事实；
4. 完整源静态查找顺序、匹配结果、未命中终止与回退值；
5. 基于结构、身份、计数、直方图、地址与 hash 而非原版行载荷的公开 H4 面。

它不拥有对话命令解码、文本选择、窗口组合、立绘像素、语音播放、控制器等待、调用方可达性、地图精灵赋值或最终呈现。相邻[dialogue-command 合同](../../contracts/dialogue-system.md) 拥有命令与调用方局部接缝。[audio-system 合同](../../contracts/audio-system.md) 拥有其受限音乐/SFX 命令域与驱动数据；两个相邻合同都不把属性表语音 SFX 身份变成观察音频行为。

唯一可执行所有者是 `sf2-sprite-dialogue-static-v1`，位于 [`tests/fixtures/h2/sprite-dialogue-static-v1.json`](../../../../tests/fixtures/h2/sprite-dialogue-static-v1.json)，由 [`src/sf2tool/h2/sprite_dialogue.py`](../../../../src/sf2tool/h2/sprite_dialogue.py) 实现。研究所有者是[Auxiliary Data Inventory](../../../research/auxiliary-data-inventory.md)与[Common Scripting](../../../research/common-scripting.md)的精灵对话段。

## 合同前证据审计

证据日期新鲜复现通过：

```text
Contract sf2-sprite-dialogue-static-v1
SHA256 B1D5979F71C298F2805D88D223CC89581D198396812943EA4FF1E93A7BA2B185
Rows 119
PortraitBearingRows 80
Status PASS
```

审计检查了专用 fixture、验证器、源所有者文章、私有生成输出、H1 绑定地址、source/ROM 范围与固定消费者源。它还发现两条当前未关联研究记录的精确一对一来来关联边界：

- 绑定到 `table.GetEntityPortaitAndSpeechSfx` 的 `scripting.entity.getentityportaitandspeechsfx`；
- 绑定到 `table.table_MapspriteDialogueProperties` 的 `auxiliary.data.table-mapspritedialogueproperties`。

源拼写 `GetEntityPortaitAndSpeechSfx` 只作为原版标识符保留；文章使用“portrait”。聚合 common-scripting 与 auxiliary-data fixture 不是本合同的证据依赖。注册推迟到初步语义接受。

审计保留以下限制：

- 119 条四字节记录占用 476 字节。后续 `0xFFFF` 字是独立双字节终止符，不是第 120 条记录；它们一起构成 478 字节检查范围。
- “51 个不同立绘字节值”是基数事实。它不定义连续 `0..50` 范围或闭合立绘域。
- 十个源码具名语音 SFX 身份及其计数不建立可听波形、音高、说话者、情绪、场景使用、时序或玩家面向含义；
- 本已接受表中所有地图精灵键唯一，但消费者首匹配扫描仍是原版算法的一部分；
- 受追踪 fixture 不包含原始行数组。完整行保持私有/生成输入。

活跃 Issue #81 拥有独立 technical-graphics 修正，不是本文档的证据或合并依赖。

## 物理表合同

**已确认静态：** `table_MapspriteDialogueProperties` 在 H1 绑定 ROM 地址 284,282 开始，其已接受范围在 284,760 排他结束。完整 478 字节 source/ROM 范围 SHA-256 为 `DA351FAB189D0AE07D1300152A02B5D214DE179CE89C2E802B27EA7D617CAACC`。

119 条记录中的每条都有该精确字节顺序：

```text
offset +0: map-sprite key
offset +1: portrait byte
offset +2: speech-SFX byte
offset +3: reserved byte
```

全部 119 个保留字节为零。消费者不加载偏移 `+3`；零填充存储与消费者忽略是独立已确认事实。476 记录字节后，一个大端 `0xFFFF` 字终止扫描域。

私有导入器必须保留记录顺序、每个原始字节、独立终止符、源符号、地址范围与全范围 hash。公开 fixture 或报告必须只保留结构元数据、hash、聚合计数、标识符或合成示例。

## 键与值域

**已确认静态：** 119 个地图精灵键字节全部不同。已接受语料中没有重复键值。这证明每个在场键选择一行，但不证明每个可能字节值在场或被自然分配给实体。

立绘列包含 51 个不同字节值。三十九行包含源身份 `PORTRAIT_NONE`；按已接受所有者分类其他 80 行带立绘。这些计数既不建立连续立绘 ID 范围，也不保证每个原始、修改、调试或损坏值都有可渲染资源。

语音 SFX 列包含十个源身份：

| 源身份 | 已确认行数 |
| --- | ---: |
| `DIALOG_BLEEP_1` | 7 |
| `DIALOG_BLEEP_2` | 12 |
| `DIALOG_BLEEP_3` | 9 |
| `DIALOG_BLEEP_4` | 8 |
| `DIALOG_BLEEP_5` | 33 |
| `DIALOG_BLEEP_6` | 33 |
| `DIALOG_BLEEP_7` | 9 |
| `DIALOG_BLEEP_8` | 5 |
| `TAROS_DIALOG_BLEEP` | 1 |
| `DEMON_BREATH` | 2 |

计数总计 119。这些名称保持带溯源的枚举身份，而非呈现或音频语义声称。

## 完整消费者查找顺序

**已确认静态：** 原版标识符 `GetEntityPortaitAndSpeechSfx` 在 ROM 地址 284,216 开始。其完整查找与结果顺序是：

1. 在栈上保留 `d0`、`a0` 与 `a5`；
2. 应用 `andi.w #COMBATANT_MASK_ALL,d0`；
3. 执行 `clr.w d1`，然后 `clr.w d2`；
4. 调用 `GetEntityAddressFromCharacter` 并把实体地图精灵字节读进 `d0`；
5. 加载属性表地址并把该字节与当前记录键比较；
6. 匹配时把立绘字节移进 `d1`、符号扩展到字、把语音 SFX 字节移进已清除 `d2`，并走完成路径；
7. 未命中时恰好推进四字节、把下一字与 `0xFFFF` 比较，并重复键比较或写两个回退字；
8. 恢复 `d0`、`a0` 与 `a5`，然后返回。

命中路径立绘结果带符号，因为字节移动后接 `ext.w d1`；`PORTRAIT_NONE` 字节值 255 因此变成字值 `-1`。命中路径语音 SFX 结果是字中的无符号字节值，因为 `clr.w d2` 先于字节写入 `d2`。导入器或兼容适配器在声称已接受无符号结果时不得省略该清除。

表耗尽时源把 `PORTRAIT_DEFAULT`（`-1`）写入 `d1`、`SFX_DIALOG_BLEEP_6`（`74`）写入 `d2`。这是静态回退路径。哪些原版实体/调用方状态自然到达它保持 **未知**。

已接受语料唯一键使“首匹配”确定性。兼容实现仍应为私有原版格式导入保留有序首匹配行为，而非静默定义新重复键策略。

## 实现无关导入模型

以下为逻辑合同，不是引擎类处方：

```text
SpriteDialoguePropertyTable {
  tableId
  sourceSymbol: table_MapspriteDialogueProperties
  romStart: 284282
  romEndExclusive: 284760
  rangeHash

  records[119] {               // private import only
    rowIndex
    mapSpriteKeyByte
    portraitByte
    speechSfxByte
    reservedByte
  }

  terminatorWord: 0xFFFF       // independent of records[]
}

SpriteDialogueLookupInput {
  characterIndexWord
  resolvedEntityMapSpriteByte
}

SpriteDialogueLookupResult {
  portraitWord                 // sign-extended on hit; -1 on fallback
  speechSfxWord                // zero-extended on hit; 74 on fallback
  matchedRowIndex?
  usedFallback
}
```

公开形式省略 `records[]`。它保留表身份、范围、hash、记录/终止符形状、基数、语音 SFX 直方图、消费者地址与查找规则。现代运行时可以把验证私有导入转码为键控资源，前提是原版行顺序与首匹配行为对保真测试保持可复现。

## 跨系统分离

进入本合同的交接是实体地图精灵字节。实体如何接收该字节由实体、map-script、己方/敌人定义或其他状态系统拥有。本合同既不验证传入赋值，也不断言全部 119 个键在正常游玩中出现。

出去的交接是立绘字与语音 SFX 字。对话系统可以使用那些身份，但本合同不定义：

- 对话命令准入、文本行选择、名称替换或故事顺序；
- 立绘资源查找、抑制、放置、镜像、动画、调色板或渲染；
- 语音 SFX 命令提交、音频驱动行为、波形、时长或同步；
- 窗口分配、文本绘制、控制器等待、跳过行为或可见时序；
- 本地化、可访问性、替代呈现、配音或替换资源；
- 对重复键表、截断记录或缺失终止符的导入器准入、诊断或恢复，以及注入域外状态的行为。

重复键表在已接受有效语料之外。如果源码形状私有表带重复键被扫描而非拒绝，已确认有序算法选择首匹配行；该确定性扫描结果不是畸形输入接受保证。

那些边界保持独立所有者、**未知** 或刻意产品设计。

## 保真、现代化与版权边界

原版格式兼容要求在导入私有原版语料时保留表身份、H1 绑定地址、地址范围、行顺序、每行全部四字节、独立终止符、source/ROM hash、键/值身份、完整消费者顺序、带符号立绘结果、零扩展语音 SFX 结果与回退字。

重制可以选择具名对话画像、编写说话者元数据、语音播放、合成哔声、可缩放立绘、字幕可访问性或区域特定呈现。那些选择必须保持显式现代内容/架构决定，而非关于原版表的证据。

原版 119 行载荷、立绘美术、声音数据、对话文本、截图与捕获输出是私有/生成版权输入。不要提交或再分发它们。公开构建需要新编写或适当许可内容。

## H4 验收面

重制侧导入器或兼容适配器只在自动化测试证明以下内容时声称本合同：

1. 恰好 119 条有序四字节记录占用 476 字节，后接一个独立双字节 `0xFFFF` 终止符，产生已接受 478 字节范围与 hash；
2. 全部 119 个地图精灵键保持不同、全部 119 个保留字节保持零，记录顺序与完整私有字节在发布行载荷的前提下往返；
3. 立绘元数据保留恰好 51 个不同字节值、39 个 `PORTRAIT_NONE` 行与 80 个带立绘行，而不发明连续或闭合立绘域；
4. 十个语音 SFX 源身份及其精确 119 行直方图保持稳定，而不指定未验证可听或玩家面向含义；
5. 消费者保留掩码、`clr.w d1`、`clr.w d2`、实体解析、地图精灵读取、有序比较、匹配加载、立绘符号扩展、未命中步长、终止符检查、回退写入、寄存器恢复与返回顺序；
6. 命中结果保留带符号立绘行为与零扩展语音 SFX 行为，而未命中结果保留立绘 `-1` 与语音 SFX 值 `74`；
7. 公开 fixture 与报告只包含元数据、身份、计数、直方图、范围、hash 与合成示例，绝不包含原版行字节或音视频内容；
8. 调用方可达性、回退使用、立绘/音频渲染、时序、本地化、可访问性与刻意呈现变更分别测试与报告。

H4 不要求原版线性表作为重制运行时存储。它要求确定性保留溯源导入与已接受有序查找行为的显式兼容路径。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 119 条四字节记录、独立终止符、478 字节范围/hash、唯一键、立绘/SFX/保留聚合 | **已确认静态** | `sf2-sprite-dialogue-static-v1`（[`sprite-dialogue-static-v1.json`](../../../../tests/fixtures/h2/sprite-dialogue-static-v1.json)） | 原始行保持私有；自然键可达性保持未闭合 |
| 完整查找顺序、带符号立绘命中结果、零扩展 SFX 命中结果、终止与回退 | **已确认静态** | `sf2-sprite-dialogue-static-v1`（[`sprite-dialogue-static-v1.json`](../../../../tests/fixtures/h2/sprite-dialogue-static-v1.json)） | 自然回退可达性与调用方可视使用保持 **未知** |
| 命令/调用方局部对话接缝 | **独立所有者** | [dialogue-command 合同](../../contracts/dialogue-system.md) | 端到端文本/立绘/音频呈现保持未闭合 |
| 语音 SFX 驱动/域数据 | **独立所有者** | [audio-system 合同](../../contracts/audio-system.md) | 播放准入、波形、时序与同步保持未闭合 |
| 本地化、可访问性、语音策略、替换资源、可分发内容 | **刻意设计** | 未来产品/内容决定 | 需要溯源、许可与独立验收 |

## 复现

```powershell
uv run sf2 h2 sprite-dialogue
uv run sf2 design-contracts test
uv run sf2 verify
```

生成详细输出保留在忽略的 `local/derived/sprite-dialogue-static.json` 下。
