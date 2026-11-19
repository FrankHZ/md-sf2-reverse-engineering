# Ketto and the eastern North Cave road

**Confirmed** in the remake. Both English and Simplified Chinese parties continued their North Cave saves at Map72 (25,29), walked onto (31,26) and appeared at Map25 (16,30), facing Up. The town has one default setup and eight resident callbacks. In both parties Bowie occupied slot 0, logical followers 7 and 31 occupied slots 1 and 2, and logical residents 128–135 occupied slots 3–10. These slot numbers describe the two observed parties. [North Cave saves](battle25-north-cave.md) · [Original admission](../../../docs/research/chapters/northern-travel-mitula/battle25-outcomes-through-ketto-entry.md)

## Residents and the counters

**Confirmed** through ordinary movement and activation in both languages. The following numbers identify source messages and permit checking the private projections without redistributing dialogue. [Original callbacks, relocation, service and search tables](../../../docs/research/chapters/northern-travel-mitula/ketto-interactions-and-merchant-relocation.md)

| Logical resident | Observed callback |
| --- | --- |
| 128 | 1956 |
| 129 | 1957–1959 |
| 130 | 1960–1962 |
| 131 | 1963–1964 |
| 132 | 1965 |
| 133, merchant | With F800 clear, 1966–1969 used speeds 0, 1, 2 and 3, with no choice wait. The caller's speed 2 returned. F800 was already set when Bowie was still moving to (12,2). The merchant then walked at speed 64. Control returned with him at (25,18) and the woman at (25,17), both facing Right. His next activation opened weapon shop 9. |
| 134, priest | 1971 on the first activation set visit flag F256 and opened Church. Cancelling the Save panel retained Hassan (Map9). A second activation reached Church again. Confirming Save recorded Ketto and F399. |
| 135, woman | Before relocation, English chose Yes at 1972 and received 1973–1975. Chinese chose No and received 1976–1977. Both set F257 and received 1978 on repeat. After relocation she opened item shop 27 with F257 already set, and again after a fresh visit with F257 clear. |

**Confirmed** on leaving by the southern row and re-entering. F800 persisted, F256 and F257 cleared, both counters retained their Right-facing placements, and the newly initialized merchant had speed 32. The settled scene saved before leaving preserved speed 64 on Continue.

| Counter | Stock in order with gold price |
| --- | --- |
| Weapon shop 9 | Broad Sword 1,600 · Buster Sword 2,600 · Heavy Lance 1,600 · Javelin 3,400 · Large Axe 2,250 · Robin Arrow 1,480 · Assault Shell 2,500 · Guardian Staff 2,380 · Indra Staff 3,200 · Power Glove 1,800 |
| Item shop 27 | Medical Herb 10 · Healing Seed 200 · Healing Drop 300 · Antidote 20 · Fairy Powder 100 · Angel Wing 40 |

**Confirmed** from the two localized shop pages and the resulting packs. Both parties bought a Medical Herb for Bowie. English gold went from 7,897 to 7,887 and Chinese gold from 12,005 to 11,995. In a separate ordinary trip Rohde bought a Robin Arrow for 1,480, left through North Cave and encountered Battle24. On his first turn he equipped item 48 (pack word 176) and attacked a Skeleton for 12 damage and 4 EXP. The same battle save also supported a paid EGRESS to Ketto (12,11), facing Up. The map's church anchor is available to the shared return lookup independently of any Ketto battle row.

**Confirmed** in the shared implementation. Defeat resolves the live destination through the same lookup on routes that use the saved town. Field Angel Wing also uses that lookup after its source-context checks, including walking and clear F64/F640. Both northern parties carry F64. **Unknown** from this visit are a Ketto-bound defeat, a field Angel Wing return, individual promoted-class equips, Indra Staff use and a critical hit from the Robin Arrow. The town introduces no new buy/sell/repair, promotion, full-pack or defeat rules. [Field item use](../../src/Sf2.Remake.Application/Runtime/Exploration/FieldEgress.cs) · [Battle return](../../src/Sf2.Remake.Application/Gameplay/Sf2/Sf2OutcomeReturnPolicy.cs)

## Search and departure

**Confirmed** from actual Search results in both languages. The first two description rows returned introduction 427 and IDs 4132–4133. The other nine returned introduction 425 and IDs 4134–4141 and 4143. Map25 has no chest rows. Bright Honey at (4,22) set F170 and went to ally 2 in both parties. Healing Water at (29,18) set F171 and went to ally 2 in English and ally 3 in Chinese. Repeating either search produced the empty response (405, 410) and no recipient. [Field Search rules](campaign-field-search.md)

**Confirmed** with the producing checkout's local directory unavailable. Exiting on row 31 yielded Map72 (31,27), facing Down, with F426 clear. Fresh title Continue and immediate Save preserved the complete decoded gameplay data in both languages for the settled town and the overworld departure. The town saves advanced from sequence 4 to 5, and departure from 5 to 6. Ordinary input then moved Bowie to (28,17) in town and (30,27) outside. The original north-mouth and upgraded Battle17 slots also retained their complete gameplay data in both languages on immediate re-save. The [Battle26 package](battle26-higins.md) continues both departure slots through Higins’s rescue. Human audiovisual acceptance and the native ObjectDB shutdown observation remain open under [#1716](https://github.com/FrankHZ/md-sf2-reverse-engineering/issues/1716).

## Reproduction

The private handoff is `issue1922/ketto`. From that directory use the commands below with fresh sample names. Repeat in `zh-CN` to use the independent Chinese party. Omitting the sample from the Continue command starts the interactive player. The final world composes the pinned original with the [campaign selection](../../content/private/pre-voyage-opening.json), #1918 north-mouth slots and #1920 upgraded Battle17 encounter inputs. The private handoff records build inputs, controls and comparison evidence.

```powershell
./Rebuild.ps1 -Repo <accepted-checkout> -Output <new-private-directory>
./Play.ps1 -Language en -Mode route -Sample visit
./Play.ps1 -Language en -Mode residents -Sample talk -Seed ./receipts/en-visit/relocated-slot.json
./Play.ps1 -Language en -Mode descriptions -Sample read -Seed ./receipts/en-visit/relocated-slot.json
./Play.ps1 -Language en -Mode continue -Sample town-load -Seed ./slots/settled-en.json
./Play.ps1 -Language en -Mode continue -Sample departure-load -Seed ./slots/departure-en.json
./Play.ps1 -Language en -Mode shot -Sample robin -Seed ./slots/robin-battle24-en.json
./Play.ps1 -Language en -Mode egress -Sample return -Seed ./slots/robin-battle24-en.json
```

Each title run writes a separate slot namespace. Decode its envelope's base64 `payload` as UTF-8 JSON and compare the entire `data` object with the seed. The outer sequence advances by one. The native receipts contain positions after movement, physical actors, localized service pages, search recipients, and merchant motion waits.
