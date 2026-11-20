# 简体中文设计文档

从[设计地图](design/zh-CN/design-map.md)按层级阅读。英文是规范源，中文提供 L0–L3 玩家规则和设计解读。原版研究、源码与 ROM 机制、配置记录、实现和验收在英文所有者处查阅。参阅[英文文档索引](README.md)和[设计与支撑范围（英文源）](design/documentation-roadmap.md#design-and-support-scope)。

## L0 愿景

[重制愿景](design/zh-CN/game-vision.md)说明核心体验、保真边界、已接受的现代化及待定的产品选择。

## L1 总体玩法

[玩法总览](design/zh-CN/synthesis/gameplay-overview.md)串起探索、准备、战斗、奖励与返回。

## L2 系统设计

| 我想了解 | 中文入口 |
| --- | --- |
| 移动、目标、法术、回合与战斗结果 | [战术战斗循环](design/zh-CN/synthesis/themes/tactical-battle-loop.md) |
| 加入、出战、职业、装备、学习及服务 | [成长与经济](design/zh-CN/synthesis/themes/progression-and-economy.md) |
| 交互、地图重入、车辆和返回 | [地图设计原则](design/zh-CN/synthesis/themes/map-design-principles.md) |
| 章节、遭遇与可选条件 | [剧情推进](design/zh-CN/synthesis/themes/story-progression.md#分段阅读故事内容) |
| 如何读懂焦点、成本与拒绝原因 | [重制愿景](design/zh-CN/game-vision.md#保真与现代化)及[共享界面规范（英文源）](../remake/docs/godot/ui-design-language.md) |

## L3 机制与行为

- **探索和选择。** [地图探索](design/zh-CN/contracts/world/map-exploration.md)、[输入](design/zh-CN/contracts/interface/input-system.md)、[对话](design/zh-CN/contracts/interface/dialogue-system.md)。
- **战斗和效果。** [战斗生命周期](design/zh-CN/contracts/battle/battle-control-lifecycle.md)、[战场寻路](design/zh-CN/contracts/battle/battlefield-navigation.md)、[AI 选择](design/zh-CN/contracts/battle/battle-ai-decision.md)、[行动结算](design/zh-CN/contracts/battle/battle-action-construction.md)、[物理交战](design/zh-CN/contracts/battle/combat-resolution.md)、[法术结算](design/zh-CN/contracts/battle/spell-resolution.md)和[效果呈现](design/zh-CN/contracts/battle/battle-scene-presentation.md)。
- **军团与经济。** [名册](design/zh-CN/contracts/party/party-roster-state.md)、[升级](design/zh-CN/contracts/party/level-up.md)、[法术书](design/zh-CN/contracts/party/spellbook-state.md)、[转职](design/zh-CN/design-map.md#装备与转职)和 [Caravan 与特卖](design/zh-CN/contracts/party/caravan-and-deals-state.md)。
- **服务与继续。** [服务](design/zh-CN/contracts/party/service-interactions.md)、[存档和继续](design/zh-CN/contracts/system/save-system.md)和[有限音乐等待](design/zh-CN/contracts/interface/music-wait-service.md)。

[Battle01、Taros 与棋盘](design/zh-CN/synthesis/analyses/battle-design-analysis-pilot.md)讨论条件成立时如何准备和应对。[Battle02 首次接敌](design/zh-CN/synthesis/analyses/battle02-first-contact.md)对照 Jaha 的地形选择及 Sarah 的治疗窗口。

## 证据和同步

**已确认**以所引静态规则或观察范围为限。**推断**用于读出规则间联系，未确立的问题为**未知**。重制决定以其设计所有者为准。

[英文规范源与验证入口](README.md) · [术语表](design/glossary.md) · [翻译审阅与漂移记录](../manifests/zh-translation-index.json) · [维护协议（英文源）](design/documentation-roadmap.md#bounded-translation-repair-agreement)
