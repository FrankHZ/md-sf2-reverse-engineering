# Ally Growth and Spell Learning Contract

- Status: **Confirmed** static contract with stat-gain and complete level-up H3 coverage
- Evidence date: 2026-07-17
- Scope: stat-growth curves, per-class ally projections, and learned-spell lists

## Result

The pinned SF2DISASM source defines five stored growth curves. Each has 29 entries for levels 2–30;
each entry contains a cumulative fraction and this-level fraction on a 256-point scale. H2 verifies
that every cumulative value equals the prior cumulative value plus the current gain, and that every
curve reaches 256 at level 30.

```powershell
pwsh ./scripts/Test-GrowthExtraction.ps1
```

The canonical ignored output contains 30 allies, 59 class records, and 122 explicit learned-spell
entries. Two exports produce SHA-256
`AAD613BD38B68CE7B983A54A7825FDA5280A363F0F152FB19A392CBD276A7059`. The tracked owner is
[`manifests/extractions/growth-data.json`](../../manifests/extractions/growth-data.json), and the
contract is [`schemas/growth-data.schema.json`](../../schemas/growth-data.schema.json).

## Storage Model

Each class record begins with a class ID and five three-byte stat records, in HP, MP, attack,
defense, and agility order. A stat record stores curve ID, starting value, and projected level-30
value. Curve ID 0 means `NONE`; the five stored curves use IDs 1–5.

The following bytes form either an explicit learned-spell list or the control byte `$FE`, which
means reuse the first class record's list. An explicit list is a sequence of learn-level/spell bytes
terminated by `$FF`; the spell byte uses the same six-bit ID and two-bit spell-level packing as the
spell definition table.

The pointer table at `0x1EE270..0x1EE2F0` has 32 entries. Entries 0–29 point to their corresponding
ally stats, while entries 30 and 31 both point to `AllyStats29` (Claude). This is distinct from the
two trailing start-definition records, whose runtime reachability remains unknown. `LevelUp` starts
at the selected pointer but does not enforce an ally-local end address while seeking a class ID: a
missing local class can therefore borrow a later ally's matching block before a negative sentinel
terminates the scan.

## Semantics Boundary

**Confirmed** here means the stored curves, projections, list inheritance, and references reproduce
the pinned source contract. Random variance, expected-minimum rounding, the curve-None path, and one
minimum-stat pity increment are now covered by controlled H3 observations in
[`runtime-rng-and-battle-math.md`](./runtime-rng-and-battle-math.md). The complete caller fixture
additionally confirms first-level stat application, the seven-byte level-up result payload, and both
TORT effective-level defect sites. A second complete-caller fixture confirms level-30
post-projection growth, base/promoted cap exits and their level 39→40 / 98→99 predecessors, a promoted
`level + 20` effective threshold, `$FE` spell-list inheritance, and successful BLAZE 1→3 replacement.
It also confirms that Peter/WIZ scans forward into Tyrin's WIZ block at `0x1EE653`, while Claude/SDMN
reaches the final sentinel and takes the genuine missing-class exit. A mutation-free startup observer
confirms Karna's HEAL 3 initialization special case changes PRST base prowess from `0x03` to `0x13`
before the ordinary level-up replay learns the spell. Fifteen controlled runs at the same branch
complete the four-double-setting by four-counter-setting matrix. The combined high nibble increments
except at 7, which remains capped at 7, and 15, which wraps to 0 on the byte write. Thus
`0x33→0x43` crosses into the counter field, `0x73→0x73` hits the guard, and `0xF3→0x03` clears both
fields; the critical low nibble remains `0x3` throughout.

A complete-combatant fixture follows Slade/THIF from level 39 to 40 through the call to
`UpdateCombatantStats`. Current HP/MP remain unchanged when maxima grow. Current ATT/DEF/AGI/MOV,
resistance, and prowess are rebuilt from base/class values, then equipped effects are reapplied; the
Short Knife adds 5 ATT to the refreshed base 47, producing current ATT 52. The fixture independently
parses the class bases and item effect from the pinned source before observing the original ROM.
A second run combines full ATTACK/BOOST/SLOW counters and STUN with a Thieve's Dagger. The original
applies status deltas from refreshed base stats, applies STUN's AGI/MOV penalties, then applies the
dagger's ATT +17 and AGI +5 effects, yielding current ATT/DEF/AGI/MOV `81/39/40/6`.
Three further runs confirm partial ATTACK/BOOST/SLOW counter magnitudes and separate floor rounding;
Black Ring + Short Knife ATT stacking followed by CURSE insertion; and Ninja Katana's
`INCREASE_DOUBLE` bug changing NINJ prowess `0x94→0x24`, which raises double chance but clears
counter 1/8 to 1/32.

The stat-clamp fixture reuses one natural Slade/THIF 39→40 call while replacing only the destination
byte at eight wrapper entries. Five increases saturate base/current ATT, base DEF, flagged base AGI,
and current MOV at their source-defined caps. Three decreases clamp current DEF/AGI/MOV to zero
instead of wrapping. The source oracle independently derives the natural growth amounts, equipped
item effects, field caps, and helper arithmetic before the ROM observation.

The extractor keeps generated names and numeric content under ignored `local/derived/`. Only schemas,
counts, hashes, structural rules, and research conclusions are tracked.

## Next Evidence

Extend the committed emulator-backed fixtures with current-ATT decrease and word/long clamp edges,
enemy curse handling, and the remaining prowess-effect functions. The existing stat-gain,
complete-caller, boundary, refresh, initialization-prowess, and stat-clamp fixtures are the first
implementation-neutral inputs for a later remake growth module; see
[`../design/level-up.md`](../design/level-up.md).
