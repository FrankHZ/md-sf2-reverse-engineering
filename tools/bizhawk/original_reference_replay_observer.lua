-- Passive transport observer for sf2-original-reference-replay-capability-v1.
-- It observes the controller value consumed by the core; it never supplies input.

local status_path = os.getenv("SF2_ORIGINAL_REFERENCE_STATUS")
local expected_rows = tonumber(os.getenv("SF2_ORIGINAL_REFERENCE_EXPECTED_ROWS"))
local callbacks, input_poll_trace = {}, {}
local initial_frame, first_poll_frame, terminal_frame = nil, nil, nil
local finished, finalizing, readonly, power_on = false, false, nil, nil
local header_platform, header_core, terminal_mode, client_version = nil, nil, nil, nil
local JSON_STRING_MAX_BYTES = 1024

local function json_number_or_null(value)
    if value == nil then return "null" end
    return tostring(value)
end

local function json_boolean_or_null(value)
    if value == nil then return "null" end
    return value and "true" or "false"
end

local function utf8_safe_prefix(text, max_bytes)
    if #text <= max_bytes then return text end
    local cut = max_bytes
    local byte = string.byte(text, cut)
    while cut > 0 and byte >= 128 and byte <= 191 do
        cut = cut - 1
        byte = string.byte(text, cut)
    end
    if cut == 0 then return "" end

    local width = 1
    if byte >= 194 and byte <= 223 then
        width = 2
    elseif byte >= 224 and byte <= 239 then
        width = 3
    elseif byte >= 240 and byte <= 244 then
        width = 4
    end
    if cut + width - 1 > max_bytes then cut = cut - 1 end
    return string.sub(text, 1, cut)
end

local function json_string_or_null(value)
    if value == nil then return "null" end
    local text = tostring(value)
    if #text > JSON_STRING_MAX_BYTES then
        text = utf8_safe_prefix(text, JSON_STRING_MAX_BYTES) .. "...[truncated]"
    end
    local escaped = { "\"" }
    for index = 1, #text do
        local byte = string.byte(text, index)
        if byte == 34 then
            escaped[#escaped + 1] = "\\\""
        elseif byte == 92 then
            escaped[#escaped + 1] = "\\\\"
        elseif byte == 8 then
            escaped[#escaped + 1] = "\\b"
        elseif byte == 9 then
            escaped[#escaped + 1] = "\\t"
        elseif byte == 10 then
            escaped[#escaped + 1] = "\\n"
        elseif byte == 12 then
            escaped[#escaped + 1] = "\\f"
        elseif byte == 13 then
            escaped[#escaped + 1] = "\\r"
        elseif byte < 32 then
            escaped[#escaped + 1] = string.format("\\u%04X", byte)
        else
            escaped[#escaped + 1] = string.char(byte)
        end
    end
    escaped[#escaped + 1] = "\""
    return table.concat(escaped)
end

local function text_or_unknown(value)
    if value == nil then return "<unknown>" end
    return tostring(value)
end

local function write_status(status, callbacks_remaining, movie_position)
    local ok, written = pcall(function()
        if status_path == nil then return false end
        local handle = io.open(status_path, "w")
        if handle == nil then return false end
        local rows = {}
        for index, value in ipairs(input_poll_trace) do
            rows[index] = string.format(
                "{\"semanticIndex\":%d,\"bk2Row\":%d,\"emuFrame\":%d,\"input\":%s}",
                value.semanticIndex, value.bk2Row, value.emuFrame, json_string_or_null(value.input)
            )
        end
        local payload = string.format(
            "{\"status\":%s,\"callbacksRemaining\":%d,\"moviePosition\":%d,\"inputPollTrace\":[%s],\"initialFrame\":%s,\"firstPollFrame\":%s,\"terminalFrame\":%s,\"movieMode\":%s,\"readOnly\":%s,\"powerOn\":%s,\"headerPlatform\":%s,\"headerCore\":%s,\"clientVersion\":%s,\"statusWriteOk\":true}",
            json_string_or_null(status), callbacks_remaining, movie_position, table.concat(rows, ","),
            json_number_or_null(initial_frame), json_number_or_null(first_poll_frame),
            json_number_or_null(terminal_frame), json_string_or_null(terminal_mode),
            json_boolean_or_null(readonly), json_boolean_or_null(power_on),
            json_string_or_null(header_platform), json_string_or_null(header_core),
            json_string_or_null(client_version)
        )
        local wrote = handle:write(payload)
        local closed = handle:close()
        return wrote ~= nil and closed ~= nil
    end)
    return ok and written == true
end

local function clear_callbacks()
    local remaining, unresolved = 0, {}
    for _, callback_id in ipairs(callbacks) do
        local ok, removed = pcall(function() return event.unregisterbyid(callback_id) end)
        if not ok or removed ~= true then
            remaining = remaining + 1
            unresolved[#unresolved + 1] = callback_id
        end
    end
    callbacks = unresolved
    return remaining
end

local function exit_with(code)
    pcall(function() client.exitCode(code) end)
end

local function fail(code, expected, actual)
    if finished or finalizing then return end
    finalizing = true
    local frame_ok, frame = pcall(function() return emu.framecount() end)
    if frame_ok then terminal_frame = frame end
    local mode_ok, mode = pcall(function() return movie.mode() end)
    if mode_ok then terminal_mode = mode end
    local cleanup_ok, remaining = pcall(clear_callbacks)
    if not cleanup_ok then remaining = #callbacks end
    local detail = "FAIL:" .. text_or_unknown(code) .. ":expected=" .. text_or_unknown(expected) .. ":actual=" .. text_or_unknown(actual)
    if remaining ~= 0 then detail = detail .. ":unregister=" .. tostring(remaining) end
    write_status(detail, remaining, #input_poll_trace)
    exit_with(1)
end

local function pressed(input, button)
    return input[button] == true or input["P1 " .. button] == true
end

local function canonical_input(input)
    local order, letters = { "Up", "Down", "Left", "Right", "A", "B", "C", "Start" }, { "U", "D", "L", "R", "A", "B", "C", "S" }
    local value = "|.|"
    for index, button in ipairs(order) do value = value .. (pressed(input, button) and letters[index] or ".") end
    return value .. "|"
end

if status_path == nil or expected_rows ~= 32 then
    fail("configuration", "expectedRows=32", tostring(expected_rows))
    return
end

local ok, message = pcall(function()
    readonly = movie.getreadonly()
    power_on = not movie.startsfromsavestate() and not movie.startsfromsaveram()
    client_version, initial_frame = client.getversion(), emu.framecount()
    if not movie.isloaded() then fail("movie-loaded", "true", "false"); return end
    if not readonly then fail("movie-readonly", "true", "false"); return end
    if not power_on then fail("power-on", "true", "false"); return end
    if movie.length() ~= expected_rows + 1 then fail("movie-length", "33", tostring(movie.length())); return end
    if emu.getsystemid() ~= "GEN" then fail("system", "GEN", tostring(emu.getsystemid())); return end
    if initial_frame ~= 1 then fail("warm-up-frame", "1", tostring(initial_frame)); return end
    local header = movie.getheader()
    header_platform, header_core = header["Platform"], header["Core"]
    if header_platform ~= "GEN" or header_core ~= "Genesis Plus GX" then fail("header", "GEN/Genesis Plus GX", tostring(header_platform) .. "/" .. tostring(header_core)); return end
    callbacks[1] = event.oninputpoll(function()
        if finalizing then return end
        local callback_ok, callback_error = pcall(function()
            local semantic_index, frame = #input_poll_trace, emu.framecount()
            local bk2_row = semantic_index + 1
            if semantic_index >= expected_rows then fail("input-poll-overrun", "32", tostring(semantic_index + 1)); return end
            if frame ~= semantic_index + 1 then fail("semantic-frame", tostring(semantic_index + 1), tostring(frame)); return end
            if semantic_index == 0 then
                first_poll_frame = frame
                if first_poll_frame ~= initial_frame then fail("semantic-attachment", tostring(initial_frame), tostring(first_poll_frame)); return end
            end
            local expected = movie.getinputasmnemonic(bk2_row)
            movie.getinput(bk2_row)
            local actual = canonical_input(joypad.get(1))
            if actual ~= expected then fail("consumed-input", expected, actual); return end
            input_poll_trace[semantic_index + 1] = { semanticIndex = semantic_index, bk2Row = bk2_row, emuFrame = frame, input = actual }
        end)
        if not callback_ok then fail("input-poll-callback", "callback-success", tostring(callback_error)) end
    end, "original-reference-input-poll")
    while not finalizing and #input_poll_trace < expected_rows do emu.frameadvance() end
    if finalizing then return end
    terminal_frame, terminal_mode = emu.framecount(), movie.mode()
    if terminal_frame ~= 33 then fail("terminal-frame", "33", tostring(terminal_frame)); return end
    if terminal_mode ~= "FINISHED" then fail("movie-mode", "FINISHED", tostring(terminal_mode)); return end
    local cleanup_ok, remaining = pcall(clear_callbacks)
    if not cleanup_ok then fail("callback-cleanup", "0", "exception"); return end
    if remaining ~= 0 then fail("callback-cleanup", "0", tostring(remaining)); return end
    if not write_status("PASS", 0, #input_poll_trace) then fail("status-write", "true", "false"); return end
    finished = true
    exit_with(0)
end)

if not ok then fail("observer", "no-lua-error", tostring(message)) end
