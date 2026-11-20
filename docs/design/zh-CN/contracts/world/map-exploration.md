# L3 · 地图探索

**已确认**的规则以下述原版条件为限。参阅[地图探索合同（英文源）](../../../contracts/world/map-exploration.md)及[地图设计原则](../../synthesis/themes/map-design-principles.md)。

## 区域、事件与物品

- **选中本地行为。** 地图进入时根据已有剧情状态选择地图变体，多个候选同时满足时取最后一项。检查当前地点的区域、交互和物品时，有序候选取第一个符合条件的项目。已有待处理地图事件与 A/C 操作同次出现时，先处理地图事件。
- **实际交互需满足位置。** C 可依当前距离、朝向和相对位置交谈或检查，也可尝试登乘在同一地点的 Caravan。没有合适的交互对象可回到场地菜单。A 优先打开场地菜单。
- **搜索及转交。** 宝箱、其他物品和普通地点描述分别选择。找到的奖励先尝试交付领队；没有位置时再由其他可接收成员承接。无人可收时，恢复拾取前的可领取状态。成功领取才算取得该物品。某些剧情道具还要在指定地点、以要求的朝向另行使用，参见 [Wooden Panel 与 Achilles Sword](../../synthesis/chapters/southern-travel-creed/ribble-tree-opening-and-achilles-sword.md)。

参阅[区域与物品判定（英文源）](../../../contracts/world/map-exploration.md#areas-events-and-items)及[事件先于输入的依据（英文源）](../../../../research/system/gameflow-core.md)。

## 移动、朝向与继续

一步移动依次考虑 Caravan、木筏、门、传送、区域，然后判断通行。门改变地格后，以已改变的地格继续检查传送和区域。准备布局时按顺序应用有效剧情变更；初到屋顶选第一个包含当前位置的区域，踩踏和传送选第一条符合位置的记录。通常要以可行的一格移动抵达触发位置。转身、开始移动、到达目标格、触发交互是不同状态。阻挡或要求未满足时可以转向而不完成位移。当前场景内转移可以沿用已改变的布局与居民；重建地图时按新的剧情状态选择布局与在场者。滚动跨界与需要重新选择地图、初始化或检查战斗的入口产生不同后果。

提示中的确认、拒绝、再次询问由各场景决定。展开文字与确认继续分别接收输入；选择完成后才能沿对应分支执行。传送、入场、结束场景和恢复可用输入是不同阶段。原版未观察到的时间和自然访问仍为**未知**。参阅[移动与时序（英文源）](../../../contracts/world/map-exploration.md#entity-movement-and-action-timing)、[选择的条件与后果（英文源）](../../../contracts/world/map-exploration.md#source-bound-choice-consumer)及[对话设计](../interface/dialogue-system.md)。

## 返回与持久性

战斗结束后，由该结果选择当前地点、预定目的地或撤退地点。返回探索后，等待先完成适用的场景、界面和本地事件，才可继续接受玩家操作。当前布局或随从状态是否随返回保留，要看是复用当前场景还是重新入场。保存和从记录继续另有[存档规则](../system/save-system.md)。参阅[战斗结果与地图返回（英文源）](../../../contracts/world/map-exploration.md#bound-outcome-world-and-return)。
