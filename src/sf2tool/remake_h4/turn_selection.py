"""Index supplied turn consumer channels and independent required source indices."""


def select_channels(actual, context, checks):
    check, absent = checks.check, checks.absent
    indices = context.get("indices") or {}
    channels = {}
    selected = actual.get("evidenceScope") == "selected-turn-consumers"
    for channel in ("samples", "warpRecords", "inputRecords"):
        indexed = {}
        source_order = []
        for ordinal, row in enumerate(actual.get(channel) or []):
            index = row.get("_index", absent) if selected else row.get("_index", ordinal)
            if index is absent:
                check("supplied source index absent", None, channel=channel)
                continue
            if type(index) is not int or index < 0:
                check("supplied source index type", False, channel=channel)
                continue
            check(
                "unique supplied source index", index not in indexed, channel=channel, index=index
            )
            indexed[index] = row
            source_order.append(index)
        check(
            "supplied channel source order", source_order == sorted(source_order), channel=channel
        )
        channels[channel] = indexed
        required = indices.get(channel)
        check(
            "independent channel inventory",
            None if required is None else len(required) == len(set(required)),
            channel=channel,
        )
        for index in required or []:
            check(
                "supplied selected record",
                True if index in indexed else None,
                channel=channel,
                index=index,
            )
    return indices, channels
