from __future__ import annotations

from dataclasses import dataclass


class _BitReader:
    def __init__(self, data: bytes) -> None:
        self._data = data
        self.position = 0

    def bit(self) -> int:
        if self.position >= len(self._data) * 8:
            raise ValueError("compressed bitstream ended before its terminator")
        value = (self._data[self.position // 8] >> (7 - self.position % 8)) & 1
        self.position += 1
        return value

    def bits(self, count: int) -> int:
        value = 0
        for _ in range(count):
            value = (value << 1) | self.bit()
        return value


@dataclass(frozen=True)
class StackDecodeResult:
    output: bytes
    input_bits_consumed: int
    command_group_count: int
    literal_word_count: int
    copy_command_count: int
    copied_word_count: int
    maximum_copy_offset_words: int
    maximum_copy_length_words: int
    history_index_counts: tuple[int, ...]


def _stack_command_nibble(reader: _BitReader) -> int:
    if reader.bit() == 0:
        return 0
    second = reader.bit()
    third = reader.bit()
    if (second, third) == (0, 0):
        return 1
    if (second, third) == (0, 1):
        return 2
    if (second, third) == (1, 0):
        return 4
    if reader.bit() == 0:
        return 8
    return reader.bits(4)


def _stack_history_index(reader: _BitReader) -> int:
    if reader.bit() == 0:
        return reader.bit()
    if reader.bit() == 0:
        return 2 + reader.bit()
    if reader.bit() == 0:
        return 4

    pair_level = 0
    while pair_level < 3:
        pair = reader.bits(2)
        if pair != 3:
            return 5 + pair_level * 3 + pair
        pair_level += 1
    return 14 + reader.bit()


def _words_to_bytes(words: list[int]) -> bytes:
    return b"".join(word.to_bytes(2, "big") for word in words)


def decode_stack_compressed(
    data: bytes, *, expected_output_bytes: int | None = None
) -> StackDecodeResult:
    """Decode SF2's bit-oriented move-to-front/section-copy format.

    The routine mirrors ``LoadStackCompressedData``. The source terminates with a section-copy
    command whose eleven-bit backward offset is zero; the decoder never relies on a file boundary.
    """

    if not data:
        raise ValueError("stack-compressed input is empty")
    if expected_output_bytes is not None and expected_output_bytes % 2:
        raise ValueError("stack decompression produces words, so expected bytes must be even")

    reader = _BitReader(data)
    output: list[int] = []
    history = list(range(16))
    history_counts = [0] * 16
    command_groups = 0
    literal_words = 0
    copy_commands = 0
    copied_words = 0
    maximum_copy_offset = 0
    maximum_copy_length = 0

    def check_output_bound() -> None:
        if expected_output_bytes is not None and len(output) * 2 > expected_output_bytes:
            raise ValueError("stack decompression exceeded the expected output size")

    while True:
        command_groups += 1
        command_word = 0
        for _ in range(4):
            command_word = (command_word << 4) | _stack_command_nibble(reader)

        for command_bit in range(15, -1, -1):
            if not (command_word >> command_bit) & 1:
                value = 0
                for _ in range(4):
                    history_index = _stack_history_index(reader)
                    history_counts[history_index] += 1
                    nibble = history.pop(history_index)
                    history.insert(0, nibble)
                    value = (value << 4) | nibble
                output.append(value)
                literal_words += 1
                check_output_bound()
                continue

            offset = reader.bits(11)
            if offset == 0:
                decoded = _words_to_bytes(output)
                if expected_output_bytes is not None and len(decoded) != expected_output_bytes:
                    raise ValueError(
                        "stack decompression output-size drift: "
                        f"expected {expected_output_bytes}, got {len(decoded)}"
                    )
                return StackDecodeResult(
                    output=decoded,
                    input_bits_consumed=reader.position,
                    command_group_count=command_groups,
                    literal_word_count=literal_words,
                    copy_command_count=copy_commands,
                    copied_word_count=copied_words,
                    maximum_copy_offset_words=maximum_copy_offset,
                    maximum_copy_length_words=maximum_copy_length,
                    history_index_counts=tuple(history_counts),
                )
            if offset > len(output):
                raise ValueError(
                    f"stack section-copy offset {offset} exceeds {len(output)} output words"
                )

            copy_length = 2
            while reader.bit() == 0:
                if reader.bit() == 0:
                    copy_length += 2
                else:
                    copy_length += 1
                    break
            for _ in range(copy_length):
                output.append(output[-offset])
                check_output_bound()
            copy_commands += 1
            copied_words += copy_length
            maximum_copy_offset = max(maximum_copy_offset, offset)
            maximum_copy_length = max(maximum_copy_length, copy_length)
