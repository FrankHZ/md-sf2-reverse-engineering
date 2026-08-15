-- Plays one music command while the CLI A/V dump window (--dump-type=wave) captures the
-- original audio as a WAV. Shutdown is owned by --dump-close; this script never calls
-- client.exitCode. Reads config from SF2_H3_CONFIG (see sf2tool.midi_extract.run_wav_dump).

local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))

local function status(value)
    local file = assert(io.open(config.statusPath, "a"))
    file:write(value .. "\n")
    file:close()
end

status("milestone:lua-start")
for _ = 1, config.bootFrames do emu.frameadvance() end
memory.write_u8(config.ram.newOperationAddress, config.command, "Z80 RAM")
status("milestone:command-injected:" .. config.command)
for _ = 1, config.recordFrames do emu.frameadvance() end
status("milestone:lua-done")
