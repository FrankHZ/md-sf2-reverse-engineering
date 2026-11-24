# L0 · 重制愿景

带领 Bowie 和 Shining Force 旅行时，在镇上找到的一件东西，可能到下一场大战才显出用处。在 Ribble，站对位置，面朝那棵树使用 Wooden Panel，通道就会打开。Achilles Sword 还在宝箱里，得有人腾得出位置才能把剑带走。后来见到 Rohde，哪怕剑在后备队员身上，他也能认出来。你可以拒绝他的提议。答应了，他就会同行，队伍也有机会挑战 Taros。等到交手时，剑由谁装备才要紧。Bowie 装备它，就能挥剑攻击 Taros。首次取胜后，队伍得到 Caravan，亲自登乘后又多了一种旅行方式。[Taros 与 Caravan](synthesis/themes/story-progression.md#完整示例taros-与-caravan)

重制希望保留这样的旅程。一个发现让你有理由回到熟悉的地方，一次交谈让你决定要不要和某人同行。整备时也得想清楚谁背这把剑，谁上场，谁在关键时刻用得上它。战斗的回报可以是一段接下来由你亲自走的路。Taros 的例子串起了原版在这些条件下的规则。[玩法总览](synthesis/gameplay-overview.md)从一次出门写到战后，说明沿途的小选择如何影响下一步。

## 保真与现代化

谁能行动、行动要付什么代价、失手会怎样、结束后在哪里继续，都以美国零售版为参照。首个本地重制里程碑要求能一路操纵 Bowie，从探索进入第一场战斗，经历必要的场景和战斗效果，回到场地后还能迈出下一步。这个范围关注玩家经历的过程，无需逐像素、逐帧或逐段声波复现原版硬件输出。[首个里程碑的产品决定（英文源）](../../decisions/0010-map3-battle01-product-acceptance.md#current-acceptance-amendment)

- **选命令时看得清。** 图标旁保留本地化名称，当前选项有清晰标记，需要付费或不能执行时说明原因。已支持的内容提供英文和简体中文。首个里程碑采用默认键盘布局 A 手动游玩，布局 C 作为补充。[界面设计（英文源）](../../../remake/docs/godot/ui-design-language.md) · [键盘布局范围（英文源）](../../decisions/0010-map3-battle01-product-acceptance.md#current-keyboard-scope)
- **想自己找路，也可以关掉提示。** 渡海登船前，引导可显示剧情已经交代的目标，知道目的地时也会指出去处。在允许加速的探索和战斗中，按住加速键（默认 Shift）即可提速，松手恢复正常。默认 1.5 倍，最高 3 倍。[目标引导（英文源）](../../../remake/docs/godot/objective-guidance.md) · [按住加速（英文源）](../../../remake/docs/godot/modernization-design.md#held-15-acceleration-1619)
- **按自己的速度读。** 一次展开整句，还可以停下来读，确认后才继续。允许跳过战前演出时，必要事件仍会发生，接下来由玩家操作。战斗速度可调整部分消息的显示，战斗结果照常结算。[文字与语音（英文源）](../../../remake/docs/godot/audio.md#accepted-fast-text-speech-policy) · [跳过战前演出（英文源）](../../../remake/docs/application/session-and-programs.md#battle-entry-skip-agreement) · [战斗速度（英文源）](../../../remake/docs/godot/presentation.md#battle-speed-and-selective-messages-1650)
- **行动前看清战场。** 轮到己方队员移动时，撤回试走的一步，就可以自由查看地形和已出现的单位。在空地上还能调出战场地图，看清全局。退出查看，仍轮到原来的队员行动。[战场检视与地图（英文源）](../../../remake/docs/godot/presentation.md#battle-inspection-and-on-demand-map-1633)
- **下次回来接着玩。** 在支持的场景里，队伍已经站稳、可以自由步行时存档，会记住这次到访的位置和在场的人。在支持的战斗里，轮到己方队员、还站在起点等候移动时可以保存，下次继续这一回合。赶路时先下车或下筏，遇到过场或战斗行动则等它结束，再尝试存档。[场地存档与恢复（英文源）](../../../remake/docs/application/opening-session-lifecycle.md#unified-settled-field-snapshots) · [战斗存档与继续（英文源）](../../../remake/docs/application/battle-save-and-continue.md#shared-snapshot-boundary)

文字一次展开时，跳过的字不补播语音，已经响起的尾音可以播完。在支持等待的地方，玩家可以主动等待。音乐播完得晚，游戏也不会因此多走几拍，这项时序选择可能使随机结果与原版不同。[音乐与等待](contracts/interface/music-wait-service.md)

## 待定的产品选择

远程使用城镇服务还在提议阶段，是否开放这条捷径仍为**未知**。扩大引导或美术覆盖、确定发布范围或主动调整平衡，也需要各自决定。含原版私有素材的内容仅限本地使用，公开版本需要项目自制或已取得相应授权的素材。[远程城镇服务提议（英文源）](../../../remake/docs/godot/modernization-design.md#local-services-and-battle-overview-1543) · [素材与分发（英文源）](../../../remake/docs/content/profiles-and-trust.md#redistribution)

[设计地图](design-map.md)通往各项玩法与具体规则。[当前重制支持范围（英文源）](../../../remake/docs/capability-status.md)记录哪些部分已经可以游玩。
