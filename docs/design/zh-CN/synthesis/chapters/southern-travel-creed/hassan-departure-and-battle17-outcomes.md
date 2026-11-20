# Hassan 出发、Battle17 进入与条件返回

## 离开城镇与随机重遇

**静态已确认：**实际从城镇出口请求大地图后，地图的有序准入可能先选已解锁的 Battle09 或 Battle17；没有匹配才回探索。选定步行区域可请求 Battle17；未完成时直接请求，完成后冷却非零就不掷。归零时，0–7 得 0 才请求，否则改设 2–5 的冷却。木筏不通过同一个步行区域回调提出遭遇。

Battle17 已完成时，重新开战的敌阵可能随 Bowie 的有效等级和随机抽取而变化。
[原版重复战敌军选择研究（英文）](../../../../../research/chapters/southern-travel-creed/battle17-initialization-and-first-control.md#f517-and-actual-repeat-definition-selection)给出静态条件。
[重制版北行重遇记录（英文）](../../../../../../remake/docs/application/hassan-taros-road.md#upgraded-interception-on-the-northern-return)记录重制版普通操作下的重遇、存档和返回。

## 结果、重试与已保存的筏

Bowie 倒下先判败北。取胜恢复符合条件队员，按出战首位战斗结束的位置、方向回大地图；完成并清除本战解锁，不因空场景省略结算。再次获胜同样用当前回点，不另给固定剧情或物品。败北恢复 Bowie、扣半金币并回本战的 Hassan 存档点；合法 EGRESS／Angel Wing 消耗资源后同点返退，保留金币。未完成时仍可再试，退败不标记胜利。若此前已取得木筏，失败或逃离重置地图外的泊位；回到城里并不自动登筏。原版自然离城、重遇和返回时的实际状态、可操作性及存读档结果仍为**未知**。

[回到剧情推进](../../themes/story-progression.md#分段阅读故事内容) · [本章条件与证据（英文源）](../../../../synthesis/chapters/southern-travel-creed/hassan-departure-and-battle17-outcomes.md)

相关章节：[Battle16 结果、下筏与条件 Hassan 入场](battle16-outcomes-through-hassan-entry.md) · [Hassan 祖先对话与 Rohde 的 Caravan 选择](hassan-ancestry-and-rohde-caravan-choice.md) · [Southwest Parmecia、Taros 的警告与有条件的 Battle18 进入](southwest-parmecia-through-taros-battle18-entry.md)
