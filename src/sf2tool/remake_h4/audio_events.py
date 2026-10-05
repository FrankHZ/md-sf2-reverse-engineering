"""Join dependent logical events to the selected audio session."""


def select_events(actual, session, check):
    events = {}
    for row in actual.get("warpRecords", []):
        state = row.get("state", {})
        check(
            "dependent result belongs to audio session",
            row["result"].get("sessionId") == session
            and (not state or state.get("sessionId") == session),
        )
        for event in row["result"].get("observations", []):
            seq = event["Sequence"]
            if seq in events:
                check("same logical event identity", events[seq] == event, sequence=seq)
            events[seq] = event
    return events
