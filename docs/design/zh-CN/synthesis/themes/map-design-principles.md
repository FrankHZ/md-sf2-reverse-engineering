# L2 · 地图设计原则

探索的关键是玩家当前在哪、面向何处、正在跟谁旅行，以及事件得知的剧情状态。一块地图可以随先前选择改变在场者、通行与服务。参阅[玩法总览](../gameplay-overview.md)和[地图设计的依据（英文源）](../../../synthesis/themes/map-design-principles.md)。

## 行动与交互

移动、转向、交谈、搜索、物品使用和行经区域有不同入口。先判断占格或地形、到达与朝向，再处理适用交互。待处理地图事件可先于同次按键。搜索时由实际持有容量决定交付；满员时按该物品规则恢复可领取状态。参阅[地图探索](../../contracts/world/map-exploration.md)。

## 对话与继续执行

对话文字的展开、确认、选择与交还操作分别处理。拒绝可结束交流、退回选择或引出后续劝说。只有实际选择及相应事件完成才改变后续条件。参阅[对话规则](../../contracts/interface/dialogue-system.md)和 [Polca 的两次 Oddler 请求](../chapters/eastern-parmecia/polca-oddler-choice-and-following.md)。

## 地点、布局与事件

- **位置与版本。** 到了同一地点，当前剧情可以决定不同居民、出口或局部遭遇。已选地图变体与随后选中的事件是两次判断。
- **顺序。** 多项同时适用时，地图变体取最后一个满足的候选，交互取第一条匹配记录。改变一处门或障碍可以改变下一步是否合法。
- **局部变化。** 暂时使用的地形和居民可随当前场景延续；从别处重新进入可重建它们。楼梯、世界滚动、地图重载和进入战斗不共享一套保留规则。

参阅[区域、事件与物品](../../contracts/world/map-exploration.md#区域事件与物品)及[选定区域与布局规则（英文源）](../../../contracts/world/map-exploration.md#areas-events-and-items)。

## 交通工具：获取、登乘、行驶与登陆

赠予木筏或取得 Caravan 只建立后续旅行的条件。实际登乘还要到达适用位置并满足朝向或接触条件，完成后控制方可用于相应地形。离开交通工具另行保存位置、恢复步行和跟随。水上或 Caravan 的路线与只能步行触发的遭遇不可互换。参阅[Polca 木筏](../chapters/southern-travel-creed/polca-raft-boarding-through-south-parmecia-entry.md)及 [Caravan 行驶](../chapters/southern-travel-creed/caravan-boarding-and-southwest-parmecia-travel.md)。

## 连接实例：从 Polca 返回到南方行程

这条**推断**的路线连接各段已确认的规则，连贯自然旅行仍为**未知**。

1. Battle13 首胜发起特定 Polca 返回，并安排 Luke 跟随；再胜会跳过首胜场景，败北或撤退走各自的 Bedoe 返回。参阅[战斗结果](../chapters/eastern-parmecia/battle13-outcomes-through-polca-raft-grant.md#首次与重复胜利)。
2. 在适用的 Polca 人口中，前三次市长接触推进临时对话，第四次才给议价。重访会清除未做完的临时对话；已记过授予则只给重复台词，不补发木筏。参阅[请求木筏](../chapters/eastern-parmecia/battle13-outcomes-through-polca-raft-grant.md#市长的四次接触)。
3. 南出口不检查是否已招募 Luke 或接受 Oddler。普通入场按条件重建木筏与随从。登乘还要求获准标记、无战斗且面前一格匹配实际木筏；有木筏资格但位置不对，仍能留下步行状态。参阅[离开与招募](../chapters/eastern-parmecia/polca-departure-through-luke-recruitment.md)。
4. 两条南出口都去 South Parmecia。一条重新入场并可先接纳已解锁战斗；另一条滚动保留在场者和木筏模式，本次不会接战。参阅[两种进入南方的方法](../chapters/southern-travel-creed/polca-raft-boarding-through-south-parmecia-entry.md#同一个目的地区域两种南出口)。

## 返回与持久性

战斗结果给出当前或预定的回归点；能否继续操作还取决于当地场景或事件。重访可能保留剧情完成和持有，同时重建邻近人物。传统存档服务再选用专门的恢复地点与场景，临时停留可以走另一条路径。参阅[战斗结果](../../contracts/battle/battle-control-lifecycle.md)和[存档规则](../../contracts/system/save-system.md)。
