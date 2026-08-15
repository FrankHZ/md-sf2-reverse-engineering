-- Boots to the title screen, enters SF2 debug mode with the cheat code, presses Up to
-- reach the battle-test scene exactly like the accepted battle01-intro-skip observers
-- (0x769C), waits for it, injects a music command, and lets the CLI A/V dump window
-- (--dump-type=wave) capture the original audio as a WAV. Shutdown is owned by
-- --dump-close; this script never calls client.exitCode.
-- Reads config from SF2_H3_CONFIG (see sf2tool.midi_extract.run_wav_dump).

local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))
local domain = "Z80 RAM"
local names = { [1] = "Up", [2] = "Down", [4] = "Left", [8] = "Right", [16] = "B", [32] = "C" }
local cheat = { 1, 1, 2, 1, 16, 32, 8, 4, 1, 1, 2, 1, 16, 32, 8, 4 }

local function status(value)
    local file = assert(io.open(config.statusPath, "a"))
    file:write(value .. "\n")
    file:close()
end

local test_entered = false

event.on_bus_exec(function()
    if not test_entered then
        test_entered = true
        status("milestone:test-scene")
    end
end, 0x769C, "test-scene", "M68K BUS")

status("milestone:lua-start")
local debug_entered = false
local pointer_file = nil
if config.recordPointers then
    pointer_file = assert(io.open(config.pointerPath, "w"))
    pointer_file:write("[")
end
for frame = 1, config.bootFrames do
    local button = nil
    local p = memory.read_u32_be(0xFFB1A0, "M68K BUS")
    if p >= 0x28FF0 and p < 0x29000 then
        button = names[cheat[p - 0x28FF0 + 1]]
    elseif memory.read_u8(0xFFB0A9, "M68K BUS") == 255 then
        if not debug_entered then
            debug_entered = true
            status("milestone:debug-mode:" .. frame)
        end
        button = "Up"
    end
    if button then joypad.set({ [button] = true }, 1) end
    emu.frameadvance()
    joypad.set({}, 1)
end
if not debug_entered then status("milestone:debug-timeout") end
if not test_entered then status("milestone:test-timeout") end
for _ = 1, config.testWaitFrames do emu.frameadvance() end
status("milestone:command-injected:" .. config.command)
memory.write_u8(config.ram.newOperationAddress, config.command, domain)
local record_frame = 0
for _ = 1, config.recordFrames do
    record_frame = record_frame + 1
    if pointer_file then
        local ch0 = memory.read_u8(0x1380, domain)
            + memory.read_u8(0x1381, domain) * 0x100
        pointer_file:write(tostring(ch0))
        if record_frame < config.recordFrames then pointer_file:write(",") end
    end
    emu.frameadvance()
    if config.clearCommandMailbox then
        -- block intro/menu SFX commands from the 68K; the music already playing is unaffected
        memory.write_u8(config.ram.newOperationAddress, 0, domain)
    end
end
if pointer_file then
    pointer_file:write("]")
    pointer_file:close()
end
status("milestone:lua-done")
