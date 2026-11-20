# L0 · 重制愿景

带领 Shining Force 穿行相互连接的世界，用一路获得的资源准备队伍，在战斗中作出战术选择，再回到因战斗结果而改变的世界。有哪些伙伴加入、队伍带了什么、恢复控制后还有哪些选择，共同决定每场遭遇的意义。参阅[玩法总览](synthesis/gameplay-overview.md)。

## 保真与现代化

以 USA 零售版为参照，保留可辨认的推进、遭遇规则、成本与后果。既定保真要求包括普通游玩可走通、必要的剧情与战斗效果，以及回到可操作的探索。呈现以保留玩法含义为准。参阅[首个里程碑的产品决定（英文源）](../../decisions/0010-map3-battle01-product-acceptance.md#current-acceptance-amendment)。

- **看得懂的控制。** 熟悉的图标配持续显示的本地化标签和清晰焦点，必要时显示成本及拒绝原因。首个里程碑以默认键盘 A 布局为必需，C 布局作为补充。游戏支持范围内提供英文和简体中文。参阅[界面设计（英文源）](../../../remake/docs/godot/ui-design-language.md)及[游戏本地化 #1468](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1468)。
- **阅读和节奏由玩家掌握。** 瞬显或一次展开不会为跳过的文字补播语音，已经在播放的语音尾部仍可完成。展开保留原有确认步骤和玩法时间。跳过战前演出仍执行必须完成的逻辑并交回操作。战斗速度仅改变适用消息的呈现。参阅[快显语音（英文源）](../../../remake/docs/godot/audio.md#accepted-fast-text-speech-policy)、[跳过战前演出（英文源）](../../../remake/docs/application/session-and-programs.md#battle-entry-skip-agreement)和[战斗速度（英文源）](../../../remake/docs/godot/presentation.md#battle-speed-and-selective-messages-1650)。
- **可选的引导和加速。** 渡海前可关闭的引导显示当前目标，已知时还给出目的地。按住 Shift 可使适用的探索和战斗暂时以 1.5 倍速度进行，松开即恢复。必须完成的效果和玩家选择各自保留时序。参阅[目标引导（英文源）](../../../remake/docs/godot/objective-guidance.md)及[按住加速（英文源）](../../../remake/docs/godot/modernization-design.md#held-15-acceleration-1619)。
- **时间和等待有明确后果。** 有限音乐的逻辑结束与实际播完分别处理。宿主延迟不会带来额外玩法机会；这可能影响随机序列。玩家等待在已有适用条件的交互和阶段中开放。参阅[音乐等待](contracts/interface/music-wait-service.md)及[玩法等待依据（英文源）](../contracts/milestones/map3-battle01-continuous-scenario.md#evidenced-gameplay-waits)。
- **通过明确的存档边界继续。** 符合条件时可在场地或战斗中存档。继续游戏须恢复可操作状态，失败时提供明确的恢复路径。参阅[场地存档资格（英文源）](../../../remake/docs/application/opening-session-lifecycle.md#eligibility-and-restoration-recipes)和[战斗继续（英文源）](../../../remake/docs/application/battle-save-and-continue.md)。

## 待定的产品选择

远程城镇服务和只读战局总览仍是提议。是否扩大存档许可为**未知**。扩展渡海登船前的引导范围、采用更广泛的美术模式、确定发布范围或主动调整平衡，均需各自决定。参阅[城镇服务与战局总览提议（英文源）](../../../remake/docs/godot/modernization-design.md#local-services-and-battle-overview-1543)。

[设计地图](design-map.md)按玩法、系统和具体行为组织阅读入口。
