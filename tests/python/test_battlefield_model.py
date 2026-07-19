from sf2tool.h2.battlefield import build_weighted_movement_model

GRID_SIZE = 48 * 48


def test_weighted_movement_uses_lifo_for_equal_budget_bucket() -> None:
    start = 24 * 48 + 24
    result = build_weighted_movement_model(
        [0] * GRID_SIZE,
        [2] * 32,
        start_offset=start,
        budget=4,
    )

    assert result["reachableCount"] == 13
    assert result["maximumCost"] == 4
    assert result["expansionOrder"] == [start, start + 48, start - 48, start - 1, start + 1]


def test_weighted_movement_preserves_flat_horizontal_wrap() -> None:
    start = 1 * 48 + 47
    result = build_weighted_movement_model(
        [0] * GRID_SIZE,
        [1] * 32,
        start_offset=start,
        budget=1,
    )

    assert result["reachableCount"] == 5
    assert result["reachableCosts"][str(2 * 48)] == 1


def test_weighted_movement_wraps_bucket_index_for_budget_128() -> None:
    terrain = [0x80] * GRID_SIZE
    for offset in range(41):
        terrain[offset] = 0

    result = build_weighted_movement_model(
        terrain,
        [1] * 32,
        start_offset=0,
        budget=128,
    )

    assert result["reachableCount"] == 41
    assert result["reachableCosts"]["40"] == 40
    assert result["expansionOrder"] == list(range(41))


def test_weighted_movement_rejects_occupied_and_unaffordable_neighbors() -> None:
    terrain = [0x80] * GRID_SIZE
    start = 100
    terrain[start] = 0
    terrain[start + 1] = 0x80
    terrain[start - 1] = 1
    terrain[start - 48] = 2
    terrain[start + 48] = 3
    move_costs = [1, -1, 5, 4] + [1] * 28

    result = build_weighted_movement_model(
        terrain,
        move_costs,
        start_offset=start,
        budget=4,
    )

    assert result["reachableCosts"] == {str(start): 0, str(start + 48): 4}
    assert result["expansionOrder"] == [start]
