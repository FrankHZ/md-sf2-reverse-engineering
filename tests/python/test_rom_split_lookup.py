import re

import pytest

from sf2tool.rom_split_lookup import (
    FAMILIES,
    LEDGER,
    MAPS,
    Family,
    Span,
    index_members,
    parse_splits,
    reconcile,
)
from sf2tool.zh_translation import _heading_anchors


def test_split_comments_and_same_range_names():
    text = """
        #dir data/icons
        /* a disabled block
        #split 0x0,0x2,data/disabled.bin
        */
        /*#split 0x0,0x2,data/other.bin*/
        #splitExtra 0x0,0x2,data/near.bin
        #split 0x1,0x3,data/icons/item.bin
        #split 0x1,0x3,data/icons/other.bin
    """
    span = Span(1, 3)
    paths = ["data/icons/item.bin", "data/icons/other.bin"]
    splits = parse_splits(text)
    assert splits == dict.fromkeys(paths, span)
    family = Family("icons", ("data/icons/*.bin",), "Corpus", "topic", "same-range names")
    by_span, by_path = index_members(splits, [family])
    assert by_span == {span: paths}
    assert by_path == dict.fromkeys(paths, family)
    assert [span.contains(i) for i in range(4)] == [False, True, True, False]


@pytest.mark.parametrize(
    "line",
    [
        "#split 0x2,0x2,data/empty.bin",
        "#split 0x3,0x2,data/reversed.bin",
        "#split 0x0,,data/bad.bin",
        "#split 0x0,0x2,data/file.bin\n#split 0x2,0x4,data/file.bin",
    ],
)
def test_malformed_active_split(line):
    with pytest.raises(ValueError):
        parse_splits(line)


def test_partial_overlap_is_not_a_same_range_alias():
    with pytest.raises(ValueError, match="overlap"):
        reconcile({Span(0, 8): ("s01", 2, 5)}, {Span(1, 4): ["a"], Span(3, 6): ["b"]})


def test_ledger_routes_reach_the_documented_leaf():
    for document in (LEDGER, MAPS, FAMILIES):
        body = document.read_text(encoding="utf-8")
        for link in re.findall(r"\[[^\]]+\]\(([^)]+)\)", body):
            if link.startswith("https://"):
                continue
            relative, _, anchor = link.partition("#")
            target = document.parent / relative if relative else document
            assert target.is_file(), link
            if anchor:
                content = target.read_text(encoding="utf-8")
                assert f'<a id="{anchor}">' in content or anchor in _heading_anchors(content), link
