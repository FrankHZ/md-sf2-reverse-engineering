"""Independent source turn generation with full buffer and signed stable passes."""

from sf2tool.h3.rng import _rng_step


def _source_turn_order(candidates, before):
    """Accepted turnorderfunctions.asm model, using the independent H3 word RNG.

    Source range is at most15 here, so its raw upper product also equals the
    semantic doubled-range/halved result. Reproduce the full buffer and62 passes,
    including signed sentinels, rather than sorting a filtered living prefix.
    """
    word = before >> 16
    draws, entries = [], []
    ordered = sorted(candidates, key=lambda c: c["ProcessingOrder"])
    for candidate in ordered:
        if not candidate["Placed"] or candidate["Hp"] == 0:
            continue
        for turn in range(2 if candidate["ExtraRoundAction"] else 1):
            basis = candidate["Agility"] if turn == 0 else candidate["Agility"] * 5 // 6
            values = []
            for index, range_ in enumerate([basis >> 3, basis >> 3] + ([3] if turn == 0 else [])):
                old = word
                word, value = _rng_step(word, range_)
                values.append(value)
                draws.append(
                    dict(
                        Actor=candidate["Actor"],
                        Turn=turn,
                        Index=index,
                        Range=range_,
                        Before=old,
                        After=word,
                        Value=value,
                    )
                )
            score = basis + values[0] - values[1] + (values[2] - 1 if turn == 0 else 0)
            entries.append(dict(Actor=candidate["Actor"], Score=score & 255))
    unsorted = entries + [dict(Actor=None, Score=255) for _ in range(64 - len(entries))]
    slots = list(unsorted)
    for _ in range(62):
        for index in range(63):
            left, right = slots[index]["Score"], slots[index + 1]["Score"]
            if (right if right < 128 else right - 256) > (left if left < 128 else left - 256):
                slots[index], slots[index + 1] = slots[index + 1], slots[index]
    return dict(
        Candidates=ordered,
        Draws=draws,
        Unsorted=unsorted,
        Sorted=slots,
        After=(word << 16) | (before & 65535),
    )
