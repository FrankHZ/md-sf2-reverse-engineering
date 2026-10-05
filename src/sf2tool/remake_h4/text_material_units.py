"""Configured modern font predicates and source field-unit expansion."""

import re


def font(value, size):
    if not value or not value.get("faces"):
        return None
    return (
        value.get("size") == size
        and value.get("resourceClass") == "FontFile"
        and all(
            f.get("family") == "Open Sans SemiBold"
            and f.get("style") == "SemiBold"
            and f.get("faceIndex") == 0
            and f.get("allowSystemFallback") is True
            for f in value["faces"]
        )
    )


def unit_reader(names, ascii_map, advances):
    def units(text, leader):
        out = []
        for part in re.split(r"(\{[^}]+\})", text):
            if part == "{N}":
                out.append(dict(Kind=1, Text="\n", Symbol=0, Advance=0))
                continue
            if part in ("{W1}", "{W2}", "{D1}"):
                out.append(
                    dict(
                        Kind={"{W1}": 3, "{W2}": 4, "{D1}": 6}[part],
                        Text="",
                        Symbol=0,
                        Advance=0,
                    )
                )
                continue
            if part.startswith("{NAME;"):
                part = names[int(part[6:-1])]
            elif part == "{LEADER}":
                part = names[int(leader)]
            elif part.startswith("{"):
                raise ValueError("unbound source control")
            for c in part:
                symbol = ascii_map[ord(c)]
                out.append(dict(Kind=0, Text=c, Symbol=symbol, Advance=advances[symbol - 1]))
        return out

    return units
