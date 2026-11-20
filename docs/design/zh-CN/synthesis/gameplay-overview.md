# L1 · 玩法总览与系统边界

探索让玩家移动、面对目标、交谈和搜索。地图与剧情状态为同一地点选择不同内容，互动可能交还控制，也可能转移地点、交付物品、招募伙伴或发起战斗。出战成员在格子上移动，然后选择行动。结算改变角色属性或资源，最终接入奖励、恢复和世界返回。参阅[重制愿景](../game-vision.md)及[设计地图](../design-map.md)。

## 沿玩法循环选择阅读深度

**推断的设计联系：**探索、遭遇、战术行动、奖励或恢复、世界返回彼此提供条件。探索找到的物品可以影响之后的遭遇，战斗行动消耗资源，结果可以改变之后出现的人物和选择。局部交谈可以直接返回，败北与撤退各有落点，获得物品也需要另行决定何时使用。

| 想了解的选择 | 继续阅读 |
| --- | --- |
| 如何移动、面对目标、交谈、搜索、登乘和退出 | [地图设计原则](themes/map-design-principles.md) |
| 谁会出现，哪些事件改变接下来的访问 | [剧情推进](themes/story-progression.md#分段阅读故事内容) |
| 谁能参战，成长和装备如何改变可用行动 | [成长与经济](themes/progression-and-economy.md) |
| 行动何时可选、可取消、提交与结算 | [战术战斗循环](themes/tactical-battle-loop.md) |
| 哪些状态随战后返回、存档和继续而保留 | [存档规则](../contracts/system/save-system.md) |

## 探索和剧情

**已确认：**附近实体、目标格、朝向和当前剧情状态共同限制交互。条件重叠时，事件选择首条匹配项，地图变体则采用最后一个条件满足的候选。待处理地图事件优先于同次采样的 A/C 操作。A 打开场地菜单；C 可与实体交互、检查区域、登乘同地的 Caravan，或回到场地菜单。搜索得到的物品先尝试给领队，再分给其他队员；全队无空位时，该物品恢复为可尝试领取。参阅[地图探索](../contracts/world/map-exploration.md)及[游戏流程依据（英文源）](../../../research/system/gameflow-core.md)。

交谈触发的局部状态会影响再访：可能更换居民、打开一条路、提出加入或安排战斗。当前跟随、正式加入、出战名额与 AI 控制都需单独判断。参阅[对话设计](../contracts/interface/dialogue-system.md)及[队伍与名册](../contracts/party/party-roster-state.md)。

## 准备队伍、行动和结算

角色的当前 HP/MP 与最大值分别变化。升级可增加最大值、使角色学会或升级某项法术，并刷新装备与状态贡献；提高最大值本身不回复已损失的 HP/MP。持有装备、可以装备、实际穿戴也是三个判断。商店、教堂、Caravan 和战斗奖励都可改变后来选择。参阅[升级](../contracts/party/level-up.md)和[服务交互](../contracts/party/service-interactions.md)。

**已确认：**地形、占格、移动预算和行动范围限制战术选择。选择目标、提交行动和应用效果分开；因此取消可退回行动前状态，资源在有效提交后才消耗。物理攻击受属性、地形、状态和随机判定影响；法术另有抗性、目标和数值顺序。回合结束按行动者处理状态与恢复，检查胜败后交给下一个行动者或进入结果。参阅[战场寻路](../contracts/battle/battlefield-navigation.md)、[物理交战](../contracts/battle/combat-resolution.md)及[法术结算](../contracts/battle/spell-resolution.md)。

## 返回世界与继续冒险

获胜可以解锁后续剧情并重建居民，败北与主动撤退按各自规则返回。战后原地探索与从存档继续分开：战后的返回地点和可操作状态由当前遭遇决定，存档则记录可用于恢复的场景及返回地点。首次进入战斗和继续挂起的战斗也走不同的准备路径。参阅[战斗控制与生命周期](../contracts/battle/battle-control-lifecycle.md)及[存档与继续](../contracts/system/save-system.md)。

## 一个贯穿多个系统的例子

[Taros 与 Caravan](themes/story-progression.md#完整示例taros-与-caravan)串起取得 Achilles Sword、回应 Rohde 的提议、选择遭遇、让 Bowie 实际装备剑、战后获得 Caravan，再自行登乘扩大旅行范围。拒绝、没有取得或装备剑、未获胜或未登乘，各自会在不同阶段改变结果。条件成立后的规则不相当于实际走通整条原版路线；更广的调用与持续性仍有**未知**。参阅[原版覆盖与观察边界（英文源）](../../../research/story-content-coverage.md#four-different-frontiers)及[重制的当前支持（英文源）](../../../../remake/docs/capability-status.md#campaign-feature-trace)。
