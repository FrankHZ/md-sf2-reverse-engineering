# 法术定义数据合同

- 状态：**已确认静态法术身份、属性行、定义打包与定义 source/ROM parity**
- 证据日期：2026-08-08
- 范围：44 个原版基础法术身份与属性行、89 个固定法术定义、打包身份/等级与动画字段，以及半径 3 存储例外

> 本文件是 [`spell-definition-data.md`](../../contracts/spell-definition-data.md) 的中文镜像。英文原文始终是审阅基线；本镜像为派生文档，遵循 [`glossary.md`](../../glossary.md) 的术语规则（R1–R7）。证据标签、源码标识符、fixture ID 与路径按 R2 原样保留。

## 判断边界

本合同定义不可变法术记录。它不从字段或枚举名推断运行时效果，也不拥有法术范围表或目标几何。

- **已确认**：44 个有序法术名；44 个有序属性值；89 个固定八字节定义记录；定义 source/ROM parity；字段打包；以及存储半径 3 例外。
- **推断**：无。element、properties、animation、radius 与 power 等标签在专用运行时所有者建立其行为之前保持存储词汇。
- **未知**：完整元素与属性语义；目标准入与范围几何；状态与效果分发；MP 检查/支付；调用方可达性；动画与呈现；半径 3 的运行时含义；平衡意图；本地化；以及可分发内容。

[法术解决合同](../../contracts/spell-resolution.md) 拥有已接受伤害、治疗、状态、目标与 EXP 用例。[Battle-scene presentation](../../contracts/battle-scene-presentation.md) 拥有独立动画/图形管线。[升级合同](../../contracts/level-up.md) 拥有习得法术变更，而[ally definition data](../../contracts/ally-definition-data.md) 拥有存储学习列表。那些行为都不在此重新定义。

## 证据所有者

`sf2-core-stats-data-static-v1`（[`core-stats-data-static-v1.json`](../../../../tests/fixtures/h2/core-stats-data-static-v1.json)）是三个关联表的专用 H2 清单所有者。它确认其 H1 地址与 `44 名 / 44 元素 / 89 定义` 基数。其带来源解释是[item、spell 与 enemy data inventory](../../../research/core-stats-data-inventory.md)。

[static core owner](../../../research/static-core-data.md)、[manifest](../../../../manifests/extractions/static-data.json)、[schema](../../../../schemas/static-data.schema.json)、[ROM layout](../../../../manifests/extractions/rom-static-layout.json)与[ROM schema](../../../../schemas/rom-static-data.schema.json) 拥有规范名称提取与独立定义 source/ROM parity。更广轨道比较 281 个固定宽度记录且零字段不匹配；本合同消费其法术名与 89 个法术定义行。

独立的 `spellranges.asm` 指针/环表与战场范围行为刻意不是证据依赖。定义局部 min/max/radius 字节只作为原始字段保留。

## 身份与定义域

源码暴露两个相关域：

| 域 | 已确认形状 | 合同后果 |
| --- | ---: | --- |
| 基础法术身份 | 44 个有序名行与 44 个有序属性行 | ID `0..43` 位置性连接名称与存储属性值。 |
| 法术定义 | 89 个有序八字节行 | 多行可以在不同打包等级引用一个基础身份或代表特殊条目。 |

89 行定义域不得折叠为 44 行。重制可以按基础 ID 与等级为普通变体建键，但必须保留定义顺序、原始打包身份与每个已接受行，使特殊或非均匀条目保持可解释。

名称是显示资源，不是行为规范。元素枚举标签是稳定导入值，不是抗性算术、效果族、目标或动画的证明。

## 固定八字节记录

每个法术定义恰好存储八个字节：

| 字节 | 存储字段 | 打包边界 |
| ---: | --- | --- |
| `0` | 法术身份与等级 | 低六位：基础法术 ID；高二位：等级码 |
| `1` | MP 消耗 | 原始字节 |
| `2` | 动画 | 五个索引位、两个变体位、一个镜像位 |
| `3` | 属性 | 原始位域 |
| `4` | 最大范围 | 原始字节 |
| `5` | 最小范围 | 原始字节 |
| `6` | 半径 | 原始字节 |
| `7` | 威力 | 原始字节 |

存储顺序是最大范围在最小范围之前。导入器不得静默重排原始记录，即使其归一化模型在 `maxRange` 之前暴露 `minRange`。

打包等级值在高二位编码等级 1 到 4。这是身份/存储规则。它不证明等级可用性、获取顺序、MP 可负担性或效果缩放。

## 半径 3 例外

源码语法注释把半径描述为 `0..2`，但定义 58 为 LASER 条目存储半径 3。独立 ROM 解码器与源字节一致，因此 schema 接受 `0..3`。

这是 **已确认数据例外**，不是已确认几何规则。无损导入器保留该值；半径 3 的目标形状、调用方可达性与可见行为保持 **未知**。验证必须遵循已接受数据，而不是把 schema 收窄到不准确的源注释。

## 元素与消费者分离

元素表为 44 个基础法术 ID 各包含一个字节。位置连接是 **已确认**。完整元素语义不是：运行时抗性选择、weakness/major/minor 调整、非伤害法术处理与特例分发归法术解决。

同样，定义属性、动画位、MP 消耗、范围字节、半径与威力是导入事实。其消费者可以按动作族不同地解读同一字段。本合同要求无损可用性，而非一个通用玩法公式。

## 实现无关导入模型

```text
SpellIdentity
  spellId
  rawNameExpression
  displayResourceRef
  elementValue

SpellDefinition
  definitionIndex
  rawSpellAndLevelByte
  spellId
  levelCode
  mpCost
  rawAnimationByte
  animationIndex
  animationVariation
  mirrored
  propertyBits
  maxRange, minRange
  radius
  power
```

该记法是逻辑的，不是必需引擎布局。原始字节与归一化字段都保持可用于 parity 诊断。范围表、运行时效果处理器、目标与呈现资源的引用属于独立层。

## 原版保真与现代化

原版保真模式保留全部 44 个身份与属性行、全部 89 个定义、表顺序、打包字节、定义局部范围字段与半径 3。它不为缺少已接受消费者合同的源码标签发明行为。

现代法术分类法、修订 MP 消耗、目标预览、新等级、平衡变更、显式效果类型、可访问呈现与替换名称是刻意设计/内容层。它们必须分别测试，且不得覆盖导入的原版基线。

生成名称与完整数字定义是私有原版内容。公开 fixture 只保留计数、地址、hash 与结构事实。可分发重制需要替换或单独清除的名称与数据。

## H4 验收门

未来重制法术数据导入器只在以下情况通过本合同：

1. 全部 44 个法术身份/名称/属性行保留顺序、数字 ID、原始名称溯源与原始属性值；
2. 全部 89 个八字节定义行保留顺序与每个原始字段；
3. 打包法术 ID/等级与动画索引/变体/镜像值精确往返；
4. 最大/最小范围存储顺序保持可追溯，即使归一化 API 使用另一顺序；
5. 定义 58 保留半径 3，而不发明几何或调用方可达性；
6. 44 行身份域与 89 行定义域保持不同且可连接；
7. 原版兼容私有数据确定性导入，而公开工件只暴露已清除内容或非表达元数据；
8. 解决、目标、效果、MP 事务、呈现、获取、平衡、本地化与现代化由独立所有者测试或报告为刻意偏差。

## 证据矩阵

| 合同区域 | 证据标签 | 可执行所有者 | 剩余边界 |
| --- | --- | --- | --- |
| 更广四文件法术源边界内的三个范围内表，44/44/89 计数 | **已确认静态** | `sf2-core-stats-data-static-v1`（[`core-stats-data-static-v1.json`](../../../../tests/fixtures/h2/core-stats-data-static-v1.json)） | 法术范围表、完整消费者与设计意图 |
| 法术名、89 个固定记录、打包与 source/ROM parity | **已确认静态** | [static core owner](../../../research/static-core-data.md)、[manifest](../../../../manifests/extractions/static-data.json)与[ROM layout](../../../../manifests/extractions/rom-static-layout.json) | 运行时字段语义与版权内容 |
| 44 个位置属性行 | **已确认静态** | `sf2-core-stats-data-static-v1`（[`core-stats-data-static-v1.json`](../../../../tests/fixtures/h2/core-stats-data-static-v1.json)） | 抗性/效果解读 |
| 伤害、治疗、状态、目标准入与 EXP 行为 | **独立所有者** | [法术解决](../../contracts/spell-resolution.md) | 完整动作族与调用方可达性 |
| 范围环/几何与战斗呈现 | **独立所有者** | 未来/相邻战场与[battle-scene presentation](../../contracts/battle-scene-presentation.md)所有者 | 几何、动画时序、渲染输出 |
| MP 曲线、法术平衡、本地化、替换内容 | **未知 / 刻意设计** | 未来综合、模拟与内容所有者 | 不得从存储行推断意图 |

## 复现

```powershell
uv run sf2 h2 core-stats-data
pwsh ./scripts/Test-StaticExtraction.ps1
pwsh ./scripts/Test-RomStaticParity.ps1
uv run sf2 design-contracts test
uv run sf2 verify
```
