from sf2tool.h2.map_import import _decode_source_table


def test_source_table_decoders_preserve_raw_values() -> None:
    flag = _decode_source_table("flagEvents", bytes.fromhex("0348002602030E0AFFFF"), 1, False)
    assert flag == [
        {
            "flag": 840,
            "source": {"x": 0, "y": 38},
            "size": {"width": 2, "height": 3},
            "destination": {"x": 14, "y": 10},
        }
    ]

    warp = _decode_source_table("warpEvents", bytes.fromhex("0E0B12010A1F0000FFFF"), 1, False)
    assert warp[0]["scrollMode"] == 0x12
    assert warp[0]["retainsCoordinates"] is True
    assert warp[0]["scrollDirection"] == 2
    assert warp[0]["reserved"] == 0


def test_item_table_terminator_excludes_trailing_rts() -> None:
    item = _decode_source_table("otherItems", bytes.fromhex("181A8601FFFF4E75"), 1, True)
    assert item == [{"x": 24, "y": 26, "flag": 134, "item": 1}]


def test_animation_header_names_cached_tile_count_not_speed() -> None:
    animation = _decode_source_table(
        "animations",
        bytes.fromhex("002E00200000001001700014FFFF"),
        1,
        False,
    )

    assert animation == {
        "tileset": 46,
        "cachedTileCount": 32,
        "entries": [
            {
                "replacementStartTile": 0,
                "tileCount": 16,
                "targetStartTile": 0x170,
                "counter": 20,
            }
        ],
    }
