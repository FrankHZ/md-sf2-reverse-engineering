-- Controlled private export. Only command boundaries or full-state recurrence are recorded.
local cfg = assert(dofile(assert(os.getenv("SF2_AUDIO_CONFIG"))))
local trace = assert(io.open(cfg.trace, "w"))
local function row(tag, value)
    trace:write(tag, ",", emu.framecount(), ",", tostring(value), "\n")
    trace:flush()
end
local function mailbox(command)
    memory.write_u8(0x1fff, command, "Z80 RAM")
end
local function advance(n)
    for _ = 1, n do emu.frameadvance() end
end
local function active(channel)
    return memory.read_u8(0x1383 + 32 * channel, "Z80 RAM") == 0
end
local ok, err = pcall(function()
    advance(180)
    memory.write_u8(0xb0a3, 1, "68K RAM")
    for i = 0, 7 do memory.write_u8(0xded8 + i, 0, "68K RAM") end
    mailbox(cfg.context)
    advance(2)
    mailbox(254)
    advance(60)
    for i = 0, 12 do assert(not active(i), "context did not stop") end
    row("dispatch", cfg.command)
    mailbox(cfg.command)
    local anchor, previous
    local window = 0
    local seen = {}
    for frame = 1, cfg.frames do
        emu.frameadvance()
        local live = false
        for i = 0, 12 do live = live or active(i) end
        if frame == 1 then
            assert(live, "no active channel")
            assert(memory.read_u8(0x1fff, "Z80 RAM") == 0, "mailbox not consumed")
            if cfg.music then
                for i = 0, 9 do
                    assert(memory.read_u16_le(0x1380 + i*32, "Z80 RAM") == cfg.pointers[i+1], "music header mismatch")
                    if not anchor and active(i) then anchor = i end
                end
            end
        end
        if not live then
            row("all-inactive", frame)
            advance(60)
            row("tail", frame + 60)
            return
        end
        if cfg.music and anchor then
            local pointer = memory.read_u16_le(0x1380 + 32*anchor, "Z80 RAM")
            if previous and pointer < previous then
                window = cfg.command == 15 and 96 or 1
            end
            if window > 0 then
                window = window - 1
                local bytes = {}
                for i = 0,319 do bytes[#bytes+1] = string.char(memory.read_u8(0x1380+i, "Z80 RAM")) end
                local state = table.concat(bytes)
                if seen[state] and emu.framecount() - seen[state] > 60 then
                    row("matched", seen[state])
                    return
                end
                seen[state] = seen[state] or emu.framecount()
            end
            previous = pointer
        end
    end
    error("no termination or whole-channel recurrence in capture window")
end)
row(ok and "PASS" or "FAIL", ok and "controlled export" or err)
trace:close()
while true do emu.frameadvance() end
