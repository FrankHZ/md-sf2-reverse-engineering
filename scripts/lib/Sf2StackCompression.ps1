Set-StrictMode -Version Latest

function Expand-Sf2StackCompressedData {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] [byte[]] $Data,
        [int] $ExpectedLength = 0
    )

    $state = [pscustomobject] @{ BitPosition = 0 }
    function Read-Bit {
        if ($state.BitPosition -ge $Data.Length * 8) { throw 'Unexpected end of SF2 Stack-compressed bitstream.' }
        $byteIndex = [Math]::Floor($state.BitPosition / 8)
        $shift = 7 - ($state.BitPosition % 8)
        $value = ([int] $Data[$byteIndex] -shr $shift) -band 1
        $state.BitPosition++
        return $value
    }
    function Read-Bits([int] $Count) {
        $value = 0
        for ($index = 0; $index -lt $Count; $index++) { $value = ($value -shl 1) -bor (Read-Bit) }
        return $value
    }
    function Read-CommandNibble {
        if ((Read-Bit) -eq 0) { return 0 }
        if ((Read-Bit) -eq 0) { return $(if ((Read-Bit) -eq 0) { 1 } else { 2 }) }
        if ((Read-Bit) -eq 0) { return 4 }
        if ((Read-Bit) -eq 0) { return 8 }
        return Read-Bits 4
    }
    function Read-HistoryIndex {
        if ((Read-Bit) -eq 0) { return Read-Bit }
        if ((Read-Bit) -eq 0) { return 2 + (Read-Bit) }
        if ((Read-Bit) -eq 0) { return 4 }
        $base = 5
        for ($group = 0; $group -lt 3; $group++) {
            $pair = Read-Bits 2
            if ($pair -lt 3) { return $base + $pair }
            $base += 3
        }
        return 14 + (Read-Bit)
    }

    $history = [Collections.Generic.List[int]]::new()
    0..15 | ForEach-Object { $history.Add($_) }
    $words = [Collections.Generic.List[int]]::new()
    $finished = $false
    while (-not $finished) {
        $command = 0
        for ($nibble = 0; $nibble -lt 4; $nibble++) {
            $command = ($command -shl 4) -bor (Read-CommandNibble)
        }
        for ($commandBit = 15; $commandBit -ge 0; $commandBit--) {
            if ((($command -shr $commandBit) -band 1) -eq 0) {
                $word = 0
                for ($nibble = 0; $nibble -lt 4; $nibble++) {
                    $historyIndex = Read-HistoryIndex
                    $value = $history[$historyIndex]
                    $word = ($word -shl 4) -bor $value
                    $history.RemoveAt($historyIndex)
                    $history.Insert(0, $value)
                }
                $words.Add($word)
                continue
            }

            $offset = Read-Bits 11
            if ($offset -eq 0) { $finished = $true; break }
            if ($offset -gt $words.Count) { throw "Invalid Stack copy offset $offset at output word $($words.Count)." }
            $copyLength = 2
            while ($true) {
                if ((Read-Bit) -eq 1) { break }
                if ((Read-Bit) -eq 1) { $copyLength++; break }
                $copyLength += 2
            }
            for ($copy = 0; $copy -lt $copyLength; $copy++) {
                $words.Add($words[$words.Count - $offset])
            }
        }
    }

    $output = [byte[]]::new($words.Count * 2)
    for ($index = 0; $index -lt $words.Count; $index++) {
        $output[$index * 2] = [byte] (($words[$index] -shr 8) -band 0xFF)
        $output[$index * 2 + 1] = [byte] ($words[$index] -band 0xFF)
    }
    if ($ExpectedLength -gt 0 -and $output.Length -ne $ExpectedLength) {
        throw "Stack decompression produced $($output.Length) bytes, expected $ExpectedLength."
    }
    return $output
}
