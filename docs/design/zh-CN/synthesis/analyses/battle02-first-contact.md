# Battle02：在有人支援的格点迎击 Rat

[英文规范原文](../../../synthesis/analyses/battle02-first-contact.md) ·
[七场战斗覆盖审计（英文）](../../../synthesis/analyses/pre-voyage-battle-model-coverage.md)

**推断的设计解：**南侧 Huge Rat 到达 `(10,13)` 时，Jaha 可以从 `(12,13)`
走到相邻的**平原** `(11,13)` 并攻击。Sarah 随后走进他让出的 `(12,13)`，
就能从一格外治疗。让 Jaha 绕到**道路** `(10,12)` 也能打同一目标，却会失去
防御地形效果。两次 Rat 普通命中后，平原上的 Jaha 还剩 **1/9 HP**，道路上的
则倒下。当我方一次攻击落空、治疗又排在第二次敌方命中之后时，这个区别就有用。
两种站位都不能强迫 Rat 攻击 Jaha。

## 范围与参考队伍

**Standard 严谨度：**[Issue #1725](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1725)
将已接受的原版资料连接成一个有条件的首次接敌选择和一组位置对照。英中文验收
涵盖格点、移动费用、射程、整数伤害、治疗和行动窗口。这里只做纸面计算，不增加
采集工具、不改平衡，也不声称已验证自然游玩。它延续
[Battle02 校准（英文）](../../../synthesis/analyses/pre-voyage-battle-model-coverage.md#one-paper-calibration-for-battle02)。

使用 [`battle01-to-battle02-party.json`](../../../../../remake/reference/inputs/battle01-to-battle02-party.json)
中的 `private-battle02-controlled-party`：普通难度/difficulty 0、首次新进入、
四人均存活、出战且无异常状态。数值已刷新并计入装备，不再叠加武器 ATT。
这是一组具名、受控的 Level 1 配置，**不是**玩家自然打完 Battle01 后的典型队伍。
新进入会补满存活队员的 HP/MP，但不会把成长和物品重置成这几行。

| 战斗员 | 起点 `(x,y)` | HP / MP | ATT / DEF / AGI | MOV 与移动类型 | 相关行动 |
| --- | --- | --- | --- | --- | --- |
| Bowie | `(13,12)` | 12 / 8 | 9 / 4 / 4 | 6，REGULAR | 本对照留在原地 |
| Sarah | `(12,12)` | 11 / 10 | 9 / 5 / 5 | 5，HEALER | HEAL1：射程 0–1，半径 0，3 MP，威力 15 |
| Chester | `(13,13)` | 11 / 0 | 8 / 5 / 7 | 7，CENTAUR | 本对照留在原地 |
| Jaha | `(12,13)` | 9 / 0 | 14 / 7 / 4 | 5，REGULAR | 已装备 Short Axe：射程 1–1 |
| Huge Rat，槽位 131 | `(5,14)` | 10 / 0 | 12 / 8 / 7 | 6，STEALTH | 徒手射程 1–1；ATTACKER1/6，主要命令 NONE |

**已确认输入：**[初始化证据所有者（英文）](../../../../research/chapters/grans-opening/battle02-initialization-and-first-control.md)
给出布阵、普通难度 Rat 刷新及 AI 路由。选中的是 **131**，不是 `(3,5)` 上使用
LEADER 命令集的 Rat128。此战没有激活多边形；区域操作数 `15/15` 走无区域路径。
命令集 6 依次尝试 ATTACK1、HEAL1、SUPPORT、MOVE1、STAY。没有物品和法术的这只
Rat 在无法接敌时可以向对手移动。源码没有承诺它必选这条路线或持续攻击 Jaha。
原版基线与溯源保留在该证据所有者：SF2DISASM
`c834c652b6862bc5679fd7f69a38a7093206efc6`、固定 USA ROM。

## 合法接近路线与支援

全部坐标都是从零开始的**战斗局部坐标**，在解码后的 48×48 数组中使用
`offset = y*48+x`。Battle02 区域为 22×18；读取地形或测量射程时，不加 Map66
原点 `(16,19)`。核对地图坐标时，局部 `(11,13)` 对应 `(27,32)`。
此处地形 ID 1/2/3 分别是平原/道路/草地。下列费用为**内部半 MOV 单位**，
预算为 `2*MOV`；只累计进入的格点，不算起点。REGULAR 与 HEALER 对三类地形的
费用为 2/2/3，STEALTH 为 2/2/2。平原选择地形效果设置 1（`230/256`），
道路为 0（`256/256`），草地为 2（`205/256`）。界面上的 15%/30% 不是伤害乘数。
参见[战场导航合同](../../contracts/battle/battlefield-navigation.md)。

| 分支 / 移动者 | 离开起点后的完整路径 | 格数 / 费用 / 预算 | 落点与行动 |
| --- | --- | --- | --- |
| Rat 接近 | `(6,14)→(7,14)→(8,14)→(9,14)→(10,14)→(10,13)` | 6 / 12 / 12 | 平原；与未移动的 Jaha `(12,13)` 距离 2，不能攻击 |
| A：Jaha | `(11,13)` | 1 / 2 / 10 | 平原；与 Rat 距离 1，可以攻击 |
| A：Sarah，在 Jaha 离开后 | `(12,13)` | 1 / 2 / 10 | 平原；与 Jaha 距离 1，可以使用 HEAL1 |
| B：Jaha，从同一起点选择另一分支 | `(11,13)→(11,12)→(10,12)` | 3 / 6 / 10 | 道路；与同一 Rat 距离 1，可以攻击 |
| B：Sarah，在 Jaha 通过后 | `(11,12)` | 1 / 2 / 10 | 道路；与 Jaha 距离 1，可以使用 HEAL1 |

Rat 进入的第一格 `(6,14)` 是草地，其余五格是平原。两条 Jaha 路线与两个 Sarah
落点除此之外均为表中的平原/道路。所列格点没有水域、阻挡或留守的 Bowie/Chester。
Jaha 会让出 Sarah 的 A 落点；在 B 分支则经过并离开她的落点。两分支互为替代，
不是让 Jaha 连走两次。Sarah 留在 `(12,12)` 时，到任一 Jaha 落点的距离都为
**2**：HEAL1 够不到斜对角。她可以在同一次行动中移动并施法；只移动不会治疗。

**推断的有条件接敌：**Rat 路线可在一次行动内完成，两种应对也各自符合随后我方
行动的移动预算。Rat 从 `(5,14)` 出发，第一次行动无法攻击任何未移动队员：最近的
Jaha 也有曼哈顿距离 8，发动射程 1 攻击前至少要走 7 格，超过 MOV 6。
它可以先于 Jaha 推进。表格证明路线和应对合法，**不证明** MOVE1 的目标排序及
回溯自然选择 `(10,13)`。使用此应对前要检查 Rat 的实际格点；换了格点就要重算射程。

## 伤害与一组有区分力的对照

采用[物理伤害流程](../../contracts/battle/combat-resolution.md#2-用整数运算计算基础物理伤害)：
`max(ATT−DEF,1)`，随后对目标地形效果取整，再进行两次向下波动抽取。
这些非暴击数值均有 `floor(damage/8)+1 = 1`，两次抽取都返回零。
假设是普通、已命中的单次攻击，没有反击、暴击、异常状态或外部伤害，属性不变。
**已确认规则；以下结果为有条件的推断：**

| 互攻 | 整数阶段 | HP 结果 |
| --- | --- | --- |
| Jaha → 平原 Rat，两分支相同 | `14−8=6; floor(6*230/256)=5; 5−0−0=5` | Rat `10→5→0`：Jaha 两次命中可击倒 |
| Rat → A 的平原 Jaha | `12−7=5; floor(5*230/256)=4; 4−0−0=4` | Jaha `9→5→1→0`：第三次命中才倒下 |
| Rat → B 的道路 Jaha | `12−7=5; floor(5*256/256)=5; 5−0−0=5` | Jaha `9→4→0`：第二次命中倒下 |

如果 Jaha 两次攻击均成功，Rat 开场移动后只来得及攻击一次，接着就被 Jaha
击倒。A 的 Jaha 剩 5 HP，B 剩 4 HP；只考虑这只 Rat，两者都不用立即治疗。
有意义的对照是 **Jaha 一次攻击落空**：闪避会跳过伤害，所以他第二次尝试后
Rat 仍可能剩 5 HP。若它赶在治疗前再次攻击，A 能活到接受 HEAL1，B 则不能。
这是应变情况，不保证攻击必定落空，也不推算概率；不额外假定特殊攻击。

## Sarah 什么时候来得及？

**已确认规则：**[回合生成（英文）](../../../../research/chapters/grans-opening/battle02-initialization-and-first-control.md#first-round-and-conditional-control-endpoint)
使用 `AGI + RNG(AGI>>3) − RNG(AGI>>3) + RNG(3)−1`，按分数降序排序；同分保持
我方先于敌方的插入次序。这里三人的 AGI 均低于 8，前两次抽取返回 0，但仍推进 RNG。
Rat 分数为 6–8，Sarah 为 4–6，Jaha 为 3–5。因此新生成的同一回合中，Rat 一定
先于 Jaha。Sarah 只有在 6/6 同分时先于 Rat；她与 Jaha 的先后则取决于分数。
三者均没有第二次行动位。跨过回合边界并不等于获得治疗。

| 有条件的窗口 | 可能发生的顺序 | 后果 |
| --- | --- | --- |
| 开场：Rat 8，Jaha 5，Sarah 4 | Rat 到 `(10,13)`；Jaha 移动并攻击；Sarah 移动进入支援位置 | 与站位表相容的合法分数顺序；满 HP 时 Sarah 不必施法 |
| 下一回合：Sarah 6，Rat 6，Jaha 3 | Sarah 先于 Rat 第一次命中行动，随后 Rat 命中、Jaha 攻击 | 受伤前治疗不能储存额外 HP；若 Jaha 两次攻击都命中，Rat 此时倒下 |
| 再下一回合：Rat 8，Sarah 4，Jaha 3 | 若 Jaha 一次攻击落空，存活的 Rat 可在 Sarah 前再次命中 | A 的 Jaha 剩 1 HP；Sarah 花 3 MP 补回 8 HP，MP `10→7`，随后 Jaha 可补刀。B 的 Jaha 在施法前倒下 |

这些是分数范围内明确声明的**顺序假设**，不是复现过的种子/队列，也不是保证能
照着执行的三回合脚本。HP 对照还要求 Rat 选择 Jaha、两次来袭均命中，且没有其他
伤害或打断。若 Sarah 在两次命中之间行动，则可补回第一次损失（A：4 HP；B：5 HP），
让任一分支的 Jaha 都能承受下一击。未转职 Sarah 的 HEAL1 恢复
`min(15, missing HP)`，不会在 9 HP 上限之外再加 15。10 MP 可用三次并剩 1 MP，
每次都占用 Sarah 的行动。参见[治疗合同](../../contracts/battle/spell-resolution.md#已确认的-heal-1-子集)。

## 选择、代价与其余威胁

**推断：**遇到此接敌状态时优先 A。走一格平原，与绕三格道路的输出和攻击机会相同，
却多保留一次普通来袭命中的余量。先让 Sarah 进入射程；Jaha 无法安全补刀时，
再于受伤后使用 HEAL1。一次施法消耗 3 MP 和一次支援行动；由 Jaha 击倒剩 5 HP
的 Rat 则直接消除其后续攻击。这里不假定另一位 Level 1 队员能补刀：他们的
ATT 8/9 远低于 Jaha 的 14。

这里不是安全角落，也不是强制敌人攻击前排的阵形。从 `(10,13)`，Rat 也能接近
Sarah。例如 A 之后，它可经过 `(10,12),(11,12),(12,12)`，花费 6，攻击
`(12,13)` 的她；B 之后则可走到已空出的 `(11,13)`，花费 2，攻击 `(11,12)` 的她。
Sarah 的 DEF 为 5，普通命中在 A 的平原造成 6 伤害，在 B 的道路造成 7 伤害；
两次均会击倒她的 11 HP。Jaha 不能封住所有接近路线，也不能强制选靶。

其余五敌也不能忽略。从宣告起点到 A 的 Jaha，曼哈顿距离分别为：
Rat128 `(3,5)` **16**；Ooze129 `(5,6)` **13**；Ooze130 `(4,10)` **10**；
Ooze132 `(7,16)` **7**；Ooze133 `(13,6)` **9**。从这些起点出发，没有一只
能在一次移动后攻击 A（MOV 6/4，射程 1）。它们移动后，这个界限就失效。
尤其 Ooze132 可以一次走 `(8,16),(9,16),(10,16),(11,16)`，花费 8；下次再走
`(11,15),(11,14)`，花费 6，即可攻击 `(11,13)`，造成
`floor((11−7)*230/256)=3` 普通伤害。它的 FOLLOW_ENEMY3 命令跟随这只 Rat；
此路线是可能的威胁，不是已断言的 AI 选择。三次敌方命中（两次 Rat 加一次 Ooze）
会超过 A 的 9 HP。依赖单独两次命中的余量前，要检查其他敌人的实际位置和可用行动。

**未知：**自然首次入场的队伍/成长、传入种子、选中的 AI 路线与目标、其他敌人
实际到达的位置、闪避/追加攻击结果，以及能形成此接敌状态的连续原版游玩。
具名受控队伍和源码局部路线合法性都不能证明这些观察。空缺不妨碍这个有条件的
选择，但不能把它称为必定发生的开局或完整 Battle02 解法。

## 复现并审查操作数

**已确认的审查范围：**初始化研究、参考队伍、导航、物理和治疗合同在此处使用的
有限量值上一致。测试夹具 `sf2-battlefield-static-v1`、`sf2-battle-terrain-decode-v1`
与 `sf2-heal1-self-recovery-v1` 覆盖共用规则/解码或受控治疗，不是组合后的自然
Battle02 遭遇。可查阅
[`battlefield-static-v1.json`](../../../../../tests/fixtures/h2/battlefield-static-v1.json)、
[`battle-terrain-decode-v1.json`](../../../../../tests/fixtures/h2/battle-terrain-decode-v1.json)
与 [`spell-healing-v1.json`](../../../../../tests/fixtures/h3/spell-healing-v1.json)。

在固定源码中，检查 `data/battles/spritesets/spriteset02.asm`、
`data/battles/global/landeffectsettingsandmovecosts.asm`、
`data/stats/allies/classes/classdefs.asm`、`data/stats/items/itemdefs.asm` 的 Short Axe、
`data/stats/spells/spelldefs.asm` 的 HEAL，以及适用的 `code/gameflow/battle/`
下 `battlefieldengine.asm:PopulateMoveCostsTable/GetLandEffectSetting` 和
`battleloop/turnorderfunctions.asm`。它们选择移动类型、表内半字节、装备射程、
治疗射程与分数算术；原版初始化证据所有者提供其消费者。

以下小型只读检查使用现有解码器和 ROM 指针表，读取 2 MiB、解码一个 2,304 字节
格网，仅打印选定格点。不需要模拟器、提取发布、新解析器或生成受跟踪文件。
从检出根目录执行，并选择已登记输入：

```powershell
. ./local/private-inputs.ps1
uv run sf2 rom verify
@'
import json, os
from pathlib import Path
from sf2tool.compression import decode_stack_compressed
f = json.loads(Path('tests/fixtures/h2/battle-terrain-decode-v1.json').read_text())
r = (Path(os.environ['SF2_SHARED_INPUT_ROOT']) / 'roms/sf2-us.bin').read_bytes()
p = f['table']['terrainTableAddress'] + 2*4
a = int.from_bytes(r[p:p+4], 'big')
b = int.from_bytes(r[p+4:p+8], 'big')
g = decode_stack_compressed(r[a:b], expected_output_bytes=2304).output
paths = {
    'rat': [(6,14),(7,14),(8,14),(9,14),(10,14),(10,13)],
    'jahaA': [(11,13)], 'sarahA': [(12,13)],
    'jahaB': [(11,13),(11,12),(10,12)], 'sarahB': [(11,12)],
    'ooze1': [(8,16),(9,16),(10,16),(11,16)],
    'ooze2': [(11,15),(11,14)],
}
for name, path in paths.items():
    ts = [g[y*48+x] for x,y in path]
    costs = {1:2, 2:2, 3:2 if name == 'rat' else 3}
    print(name, ts, 'cost', sum(costs[t] for t in ts))
print('terrain ROM span', hex(a), hex(b), 'bytes', b-a)
'@ | uv run python -X utf8 -
```

预期：Rat 类型 `[3,1,1,1,1,1]`，费用 12；Jaha A `[1]`，费用 2；Sarah A `[1]`，
费用 2；Jaha B `[1,2,2]`，费用 6；Sarah B `[2]`，费用 2；Ooze 路线分别为
`[1,1,1,1]`，费用 8，和 `[3,3]`，费用 6。ROM 范围为
`0x1ad460..0x1ad5ac`（332 压缩字节）。使用前应按宣告分支重新检查每个落点的
占用和曼哈顿行动射程。
