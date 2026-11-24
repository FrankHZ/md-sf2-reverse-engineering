# L3 · 有限音乐等待

重制已接受的有限音乐规则把音乐实际播放与玩法逻辑的完成分开处理。通常需要逻辑时间达到终点，且当前这次曲目实际播完。音频迟到不再增加玩法时间或随机判定，先播完也不会省略后续逻辑工作。恢复上一首曲目后，场景才能继续。

符合条件的招募场景若在有限曲目等待后立即恢复上一首曲目，玩家可以新按确认快进。游戏先完成剩余的逻辑步骤，再跳过尚未播完的部分并恢复上一首曲目。招募相关的队伍、物品和剧情变化仍按顺序发生一次。遇到下一处普通确认时，要松开再按。循环曲目、其他玩家确认和要求效果实际结束的等待各有条件。参阅[场地剧情等待中的确认规则（英文源）](../../../../../remake/docs/application/session-and-programs.md#confirm-during-field-story-waits)。

这项重制决定可能影响后续随机序列。**已确认（静态）：**原版先等待，再检查是否继续等待。实际听到的结束时点和切换时序仍为**未知**。参阅[重制愿景](../../game-vision.md)、[现代有限音乐决定（英文源）](../../../contracts/interface/music-wait-service.md#accepted-modern-finite-music-policy)及[原版静态边界（英文源）](../../../contracts/interface/music-wait-service.md#original-static-purpose)。
