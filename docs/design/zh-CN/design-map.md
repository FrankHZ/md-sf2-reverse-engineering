# 重制设计地图

从这里按由浅入深的层级查阅重制版设计：先看核心体验与玩法循环，再进具体系统与规则，最后按需追到英文原版证据或实现文档。

| 层级 | 回答什么问题 | 阅读入口 |
| --- | --- | --- |
| L0 · 愿景 | 我们想保留怎样的旅程，接受了哪些现代化改进？ | [重制愿景](game-vision.md) |
| L1 · 总体玩法 | 探索、整备、战斗、奖励和返程怎样串成一次冒险？ | [玩法总览](synthesis/gameplay-overview.md) |
| L2 · 系统设计 | 每个系统给玩家什么选择、代价和后果？ | 见下方[系统地图](#系统地图) |
| L3 · 机制规则 | 在具体局面下，什么操作合法、按什么顺序结算？ | [中文机制入口](../../README.zh-CN.md#l3-机制与行为)与下方典型链路 |

## 系统地图

下表把游戏各大系统映射到当前的策划入口、技术/证据来源，以及尚未补齐的策划文档缺口（暂缺策划入口的领域会直接标明，不把纯英文工程页当成已完成的策划案）。

| 系统领域 | 策划入口（L0–L3） | 技术规则、证据与实现来源（英文源） | 当前缺口与追踪 |
| --- | --- | --- | --- |
| 战术战斗与遭遇 | [战术战斗循环](synthesis/themes/tactical-battle-loop.md) · [Battle01/塔罗斯/棋盘分析](synthesis/analyses/battle-design-analysis-pilot.md) · [Battle02 首次接敌](synthesis/analyses/battle02-first-contact.md) | [战斗生命周期](contracts/battle/battle-control-lifecycle.md)、[寻路](contracts/battle/battlefield-navigation.md)、[物理交战](contracts/battle/combat-resolution.md)、[法术结算](contracts/battle/spell-resolution.md)、[AI 决策](contracts/battle/battle-ai-decision.md)、[特殊机制（英文源）](../../../remake/docs/domain/special-mechanics-design.md) | 核心数值与公式速查待后续整理（`#2259` C 阶段）；后期特殊机制实现归 [`#1370`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1370)。 |
| 成长、职业、装备与经济 | [成长与经济](synthesis/themes/progression-and-economy.md) | [名册](contracts/party/party-roster-state.md)、[升级](contracts/party/level-up.md)、[法术书](contracts/party/spellbook-state.md)、[服务交互](contracts/party/service-interactions.md)、[车队与特卖](contracts/party/caravan-and-deals-state.md)、[转职设计（英文源）](../../../remake/docs/application/church-promotion-design.md)、[装备使用与修理（英文源）](../../../remake/docs/domain/equipment-use-repair-design.md) | 全表装备、转职与消耗品规则已在各自边界内实现；后期自然入队、取得与秘银锻造归战役主线 [`#1818`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1818)。 |
| 探索、地图与城镇服务 | [地图设计原则](synthesis/themes/map-design-principles.md) | [地图探索](contracts/world/map-exploration.md)、[对话](contracts/interface/dialogue-system.md)、[输入](contracts/interface/input-system.md)、[探索与商店界面（英文源）](../../../remake/docs/godot/field-and-shop.md)、[车队服务（英文源）](../../../remake/docs/application/caravan-services-design.md) | 后期地图布置与可选访问随 [`#1818`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1818) 推进。 |
| 剧情结构与章节关卡 | [剧情推进](synthesis/themes/story-progression.md) | [原版剧情覆盖（英文源）](../../research/story-content-coverage.md)、[战役 flags 索引（英文源）](../../../remake/docs/content/campaign-flags.md)、[物品/地点/角色索引（英文源）](../../../remake/docs/content/campaign-items.md)、[剩余战役基线（英文源）](../../../remake/docs/application/remaining-campaign-risk-baseline.md) | 按故事时序贯通的剧情事件与章节总表归 [`#2366`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2366)。 |
| 界面、交互与现代化体验 | [重制愿景·保真与现代化](game-vision.md#保真与现代化)（已接受摘要） | [共享界面规范（英文源）](../../../remake/docs/godot/ui-design-language.md)、[场景评审（英文源）](../../../remake/docs/godot/ui-scene-review.md)、[目标引导（英文源）](../../../remake/docs/godot/objective-guidance.md)、[现代化提案（英文源）](../../../remake/docs/godot/modernization-design.md) | **策划缺口：** 尚无统一的中文 L2 界面交互与原版/重制差异总览（[`#2386`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2386)、`#2259` C 阶段）；UI 升级归 [`#438`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/438)、[`#1616`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1616)、[`#2369`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2369)、[`#1578`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1578)。 |
| 音画表现与双画质模式 | *尚无独立 L2 策划入口* | [呈现规范（英文源）](../../../remake/docs/godot/presentation.md)、[音频（英文源）](../../../remake/docs/godot/audio.md)、[战斗演出（英文源）](../../../remake/docs/godot/battle-scenes.md)、[法术演出（英文源）](../../../remake/docs/godot/spell-presentation.md)、[高清推进（英文源）](../../../remake/docs/content/hd-rollout.md)、[媒体管线（英文源）](../../../remake/docs/content/asset-pipeline-workflow.md) | **制作文档缺口：** 美术/音频规格与 HD 覆盖总览归 [`#2384`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2384)；高清美术归 [`#1699`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1699)。 |
| 存读档与续玩边界 | [存档规则](contracts/system/save-system.md) · [地图返回与持久性](synthesis/themes/map-design-principles.md#返回与持久性) | [场地存档与恢复（英文源）](../../../remake/docs/application/opening-session-lifecycle.md)、[战斗存档与继续（英文源）](../../../remake/docs/application/battle-save-and-continue.md)、[当前可玩能力（英文源）](../../../remake/docs/capability-status.md#campaign-feature-trace) | 后续章节存档配方随 [`#1818`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1818) 扩展；快照最小状态审计归 [`#2284`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2284)。 |
| 现代引擎与 Debug 支持 | *尚无策划可读入口* | [重制架构（英文源）](../../../remake/docs/architecture.md)、[ADR 0019（英文源）](../../decisions/0019-state-and-content-driven-remake-engine.md)、[本地调试端口（英文源）](../../../remake/docs/verification/debug-port.md) | **策划缺口：** 现代引擎协作边界与 Debug 支持入口归 [`#2358`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/2358)。 |

## 典型规则链路示例

修改或核对具体规则时，可按以下三条典型链路从玩家体验追到唯一技术来源。

### 装备与转职

职业决定队员能穿戴什么装备、学什么法术以及移动方式。在教堂转职时，祭司会从所有已加入军团的队员里（包含后备与倒下成员）筛选当前职业可转职、且等级至少为 20 的人。普通转职免费；特殊转职会自动检索全军团携带栏里是否有对应转职道具，直到最终确认才从持有者背包扣除。拒绝特殊转职会退回询问普通转职；转职为召唤师（SORC）时若拒绝覆盖原法术书，会保留道具和原法术并退回选择。

转职成功后，等级重置为 1、EXP 归 0，但保留已成长的属性和当前 HP/MP，并按新职业基础值与已装备效果重新刷新属性；召唤师（SORC）的法术书会替换为达奥（DAO）1，武僧（MMNK）与忍者（NINJ）会卸下原武器（武器仍留在背包里，戒指不受影响）。每名队员可独立装备一把武器和一枚戒指；部分装备可在战斗中发动效果，也可能损坏，损坏后需找商店修理。重制版已实现全部 94 件原版装备定义、30 名角色与 59 个合法职业绑定的规则与消费方；原版完整教堂转职交易与 SRAM 往返持久性仍为**未知**，后期自然取得与秘银锻造仍属战役主线（[`#1818`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1818)）。

参阅：[成长与经济·职业与转职](synthesis/themes/progression-and-economy.md#定义当前职业与转职)、[服务交互](contracts/party/service-interactions.md)、[升级与属性刷新](contracts/party/level-up.md)、[教堂转职设计（英文源）](../../../remake/docs/application/church-promotion-design.md)、[全表装备实现边界（英文源）](../../../remake/docs/capability-status.md#whole-game-equipment-audit-and-implementation-boundary)。

### 状态法术

施放状态与支援法术需要已学对应等级、足够 MP 与合法中心格，确认前取消不耗 MP。迟缓（SLOW）与强化（BOOST）把状态计数变化和属性刷新分开结算：重施失败时计数可能已变化，但当下仍沿用旧的有效属性，直到下次刷新为止；重制版在战斗 Continue 中保留了这项差异。解毒（DETOX）2/3 级可在场地和战斗中按各自规则解除中毒与眩晕，3 级还能解除诅咒并卸下受诅咒装备；催眠（SLEEP）1、混乱（MUDDLE）1/2 与摄魂（DESOUL）1/2 则各自遵循独立的范围、抗性、控制权与即死规则，不共用同一套倒计时。

重制版在各自边界内已支持上述状态法术；原版迟缓（SLOW）2 的自然完整重施寿命，以及带未刷新状态读档的行为仍为**未知**。参阅：[战术战斗·魔法、治疗与辅助](synthesis/themes/tactical-battle-loop.md#魔法治疗与辅助)、[法术结算](contracts/battle/spell-resolution.md)、[支援法术设计（英文源）](../../../remake/docs/domain/support-spell-status-design.md)、[解毒分级规则（英文源）](../../../remake/docs/domain/detox-ranks-design.md)、[敌对状态法术（英文源）](../../../remake/docs/domain/hostile-spells-design.md)。

### 探索服务与存读档

进入城镇时，游戏按当前剧情标志决定在场居民与可用服务。退出服务、战后返回与冷启动读档是三件不同的事：例如 Elven Village 在妖精归来后会换一批居民，但共用祭司和楼内楼梯仍沿用当前状态。重制版在教堂选择记录冒险时，先关闭教堂对话，再从稳定的可步行探索状态检查存档资格：取消会保留原先的撤离（EGRESS）返回城镇，确认才会同时更新返回点并写入可冷启动恢复的存档。

原版各章自然游玩后的完整断电存读档表现仍为**未知**；远程使用已访问城镇的服务目前仍是待定提案（[现代化设计（英文源）](../../../remake/docs/godot/modernization-design.md#recommended-town-access)），偶发退出原生终结器问题归 [`#1716`](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716)。参阅：[地图设计原则·返回与持久性](synthesis/themes/map-design-principles.md#返回与持久性)、[存档规则](contracts/system/save-system.md)、[场地存档资格与恢复（英文源）](../../../remake/docs/application/opening-session-lifecycle.md#eligibility-and-restoration-recipes)。

## 依据与边界

- **证据含义**：**已确认**代表有明确静态或运行观察支撑；**推断**代表基于已确认规则的合理关联；**未知**代表尚未证实的原版行为。详见[三层证据边界（英文源）](../documentation-roadmap.md#three-layer-boundary)与[中文文档覆盖范围（英文源）](../documentation-roadmap.md#design-and-support-scope)。
- **实现边界**：当前实际可玩范围以[重制能力状态（英文源）](../../../remake/docs/capability-status.md#campaign-feature-trace)为准；设计文档完整不代表对应章节已可玩或可公开分发原版素材。
- **英文规范源**：[English Design Map](../design-map.md) · [英文文档索引](../../README.md) · [中文设计总入口](../../README.zh-CN.md)。
