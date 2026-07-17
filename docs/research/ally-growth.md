# Ally Growth and Spell Learning Contract

- Status: **Confirmed** for source structure, curve arithmetic, counts, references, and deterministic extraction
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
two trailing start-definition records, whose runtime reachability remains unknown.

## Semantics Boundary

**Confirmed** here means the stored curves, projections, list inheritance, and references reproduce
the pinned source contract. The random variance, expected-minimum rounding, and promotion-level path
have now been traced statically in
[`runtime-rng-and-battle-math.md`](./runtime-rng-and-battle-math.md), but controlled level-up outcomes,
caps, and the documented TORT edge case still require H3 scenarios.

The extractor keeps generated names and numeric content under ignored `local/derived/`. Only schemas,
counts, hashes, structural rules, and research conclusions are tracked.

## Next Evidence

Create a small emulator-backed fixture that records deterministic gains for a controlled
ally/class/level and RNG state, including the expected-minimum bonus and TORT promotion boundary.
That fixture should become the implementation-neutral input for a later remake growth module.
