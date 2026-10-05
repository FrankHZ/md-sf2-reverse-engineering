"""Read one bounded field case through the caller-owned document reader."""

import json
from pathlib import Path

from sf2tool.paths import repo_path
from sf2tool.remake_h4_reference import require


def case_input(case, read_document):
    """Read one bounded original native case; no normalized copy of its event stream."""
    if "records" in case:
        return case

    def selected(name, limit):
        p = Path(case[name])
        p = p.resolve() if p.is_absolute() else repo_path(p).resolve()
        require(p.is_relative_to(repo_path("local").resolve()), "field case outside local/")
        require(p.stat().st_size <= limit, "field case input cap")
        return p

    profile = read_document(selected("profilePath", 768 * 1024))
    profile = dict(
        start=profile["start"],
        battle=dict(start=dict(mainSeed=profile["battle"]["start"]["mainSeed"])),
        world=dict(
            maps=profile["world"]["maps"],
            programs=profile["world"]["programs"],
            portraits=[
                {k: v for k, v in p.items() if k != "raster"}
                for p in profile["world"]["presentation"]["portraits"]
            ],
        ),
    )
    records = [
        json.loads(line, parse_float=lambda s: int(float(s)) if float(s).is_integer() else float(s))
        for line in selected("recordsPath", 2 * 1024 * 1024)
        .read_text(encoding="utf-8")
        .splitlines()
    ]
    return dict(
        case,
        profile=profile,
        records=records,
        process=read_document(selected("processPath", 128 * 1024)),
        launch=read_document(selected("launchPath", 128 * 1024)),
        testedView=selected("testedViewPath", 128 * 1024).read_text(encoding="utf-8"),
        errors=selected("errorsPath", 512 * 1024).read_text(encoding="utf-8").strip(),
    )
