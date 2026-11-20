# 重制设计地图

| 层级 | 阅读入口 |
| --- | --- |
| L0 · 愿景 | [重制愿景](game-vision.md)说明保留的体验与已接受的产品选择。 |
| L1 · 总体玩法 | [玩法总览](synthesis/gameplay-overview.md)串起探索、队伍准备、战术、结果和返回。 |
| L2 · 系统设计 | [战术战斗](synthesis/themes/tactical-battle-loop.md)、[成长与经济](synthesis/themes/progression-and-economy.md)、[剧情推进](synthesis/themes/story-progression.md)、[探索与地图](synthesis/themes/map-design-principles.md)及[共享界面设计（英文源）](../../../remake/docs/godot/ui-design-language.md)。 |
| L3 · 机制与行为 | 从下列案例或[中文机制入口](../../README.zh-CN.md#l3-机制与行为)查阅合法选择、变化顺序、失败和返回。 |

**已确认**的原版规则适用于证据所有者明确的静态或观察范围。**推断**用于解释规则之间如何衔接。尚未证实的行为标为**未知**。重制决定以相应设计协议为准。参阅[设计与支撑范围（英文源）](../documentation-roadmap.md#design-and-support-scope)和[英文设计地图](../design-map.md)。

## 装备与转职

[成长与经济](synthesis/themes/progression-and-economy.md#定义当前职业与转职)将职业成长与装备贡献分开。祭司从已加入成员中选择当前职业可转职、等级至少为 20 的成员。后备与倒下的成员也可入选。普通转职免费。特殊转职从已加入成员携带栏中寻找所需物品，最终同意才从选定持有者扣除。拒绝特殊选项会另行询问普通转职；SORC 的法术书覆盖确认被拒绝时，保留物品和法术书，返回选择。

成功后成为新职业、等级 1、EXP 0，保留已成长的属性、当前 HP 和 MP。先更新职业基础，再应用状态和已装备贡献。SORC 以 DAO 1 替换法术书。MMNK 和 NINJ 卸下武器，物品留在携带栏，戒指装备独立。重制在场地、战斗、返回与存档间保留当前职业。完整原版祭司交易和 SRAM 往返仍为**未知**。

参阅[服务交互](contracts/party/service-interactions.md)、[成长与属性刷新](contracts/party/level-up.md)、[转职选择与持有（英文源）](../../../remake/docs/application/church-promotion-design.md#selection-and-custody)、[成功写入及例外（英文源）](../../../remake/docs/application/church-promotion-design.md#what-changes-on-success)及[物品配置（英文源）](../contracts/party/item-definition-data.md#item-identity-and-fixed-record)。

## 状态法术

[战术战斗循环](synthesis/themes/tactical-battle-loop.md#魔法治疗与辅助)将已学等级、可选中心格、行动提交和逐目标效果连起来。取消确认无需支付 MP。SLOW 与 BOOST 首次生效、重施失败、属性刷新和持续时间递减分别处理；重施可能改变计数而当下沿用旧的有效属性。重制在战斗 Continue 中保留这项差别，直至该行动者真正刷新。原版 SLOW 2 的自然完整重施过程，以及带未刷新效果的读档，仍为**未知**。

参阅[法术结算](contracts/battle/spell-resolution.md#已确认的-boost-1-首次应用与重施怪癖)、[重制支援法术决定（英文源）](../../../remake/docs/domain/support-spell-status-design.md#implementation-decisions)及[法术配置（英文源）](../contracts/party/spell-definition-data.md#fixed-eight-byte-record)。

## 探索服务与存读档

[地图设计原则](synthesis/themes/map-design-principles.md#返回与持久性)区分退出服务、战后返回和继续冒险。Elven Village 在 Fairy 回来后换一批居民，仍可找到共用的祭司，上下楼沿用同一区域的当前状态。保存时，选择 EGRESS 的返回地点和捕获可继续的场景是两个步骤。重制先关闭服务，再从稳定的探索状态检查保存资格；取消保留原返回地点，确认才记住新地点和可恢复的场景。

参阅[Elven Village 的交互与楼梯](synthesis/chapters/southern-travel-creed/elven-village-conversations-and-local-exploration.md)、[存档规则](contracts/system/save-system.md)、[原版人口与楼梯条件（英文源）](../../research/chapters/southern-travel-creed/elven-village-conversations-and-local-exploration.md#internal-stairs-and-conditional-map23-control)及[重制资格与恢复（英文源）](../../../remake/docs/application/opening-session-lifecycle.md#eligibility-and-restoration-recipes)。
