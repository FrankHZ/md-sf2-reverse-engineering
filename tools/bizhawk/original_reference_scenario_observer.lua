-- Passive generic observer for original-reference scenario API preflight contracts.
-- It never supplies input or writes emulated state. Source-backed addresses below
-- are declaration-only until a separately admitted private scenario uses them.

local status_path = os.getenv("SF2_ORIGINAL_REFERENCE_SCENARIO_STATUS")
local case_id = os.getenv("SF2_ORIGINAL_REFERENCE_SCENARIO_CASE")
local callbacks, observed_roles = {}, {}
local finalizing, finished, current_role = false, false, nil
local JSON_TEXT_MAX_BYTES = 500
local JSON_TEXT_TRUNCATION = "...[truncated]"

-- Shared-PC groups dispatch their roles in this source order. The addresses are
-- static anchors, not evidence that a route reaches either instruction.
local checkpoint_groups = {
    { address = 0x24106, roles = { "turn-finalization-resume" } },
    { address = 0x23CBA, roles = { "victory-entry", "declared-terminal" } },
}

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

local function json_string(value)
    local text = tostring(value)
    local text_limit = JSON_TEXT_MAX_BYTES - #JSON_TEXT_TRUNCATION
    if #text > JSON_TEXT_MAX_BYTES then
        text = utf8_safe_prefix(text, text_limit) .. JSON_TEXT_TRUNCATION
    end
    local escaped = { "\"" }
    for index = 1, #text do
        local byte = string.byte(text, index)
        if byte == 34 then
            escaped[#escaped + 1] = "\\\""
        elseif byte == 47 or byte == 92 or byte < 32 or byte == 127 then
            escaped[#escaped + 1] = "?"
        else
            escaped[#escaped + 1] = string.char(byte)
        end
    end
    escaped[#escaped + 1] = "\""
    return table.concat(escaped)
end

local function json_optional(value)
    if value == nil then return "null" end
    return json_string(value)
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

local function write_status(status, phase, code, role, callbacks_remaining, cleanup_result, expected, actual, detail)
    local ok, written = pcall(function()
        if status_path == nil then return false end
        local handle = io.open(status_path, "w")
        if handle == nil then return false end
        local roles = {}
        for index, role in ipairs(observed_roles) do roles[index] = json_string(role) end
        local payload = string.format(
            "{\"status\":%s,\"phase\":%s,\"code\":%s,\"caseId\":%s,\"currentRole\":%s,\"callbacksRemaining\":%d,\"cleanupResult\":%s,\"observedRoles\":[%s],\"expected\":%s,\"actual\":%s,\"detail\":%s,\"exitCode\":%d,\"consoleCheckRequired\":true}",
            json_string(status), json_string(phase), json_string(code), json_optional(case_id),
            json_optional(role), callbacks_remaining, json_string(cleanup_result), table.concat(roles, ","),
            json_string(expected), json_string(actual), json_string(detail), status == "PASS" and 0 or 1
        )
        local wrote = handle:write(payload)
        local closed = handle:close()
        return wrote ~= nil and closed ~= nil
    end)
    return ok and written == true
end

local function finalize(status, phase, code, role, expected, actual, detail)
    if finalizing or finished then return end
    finalizing = true
    local cleanup_ok, remaining = pcall(clear_callbacks)
    local cleanup_result = "protected-cleanup-ok"
    if not cleanup_ok then
        remaining = #callbacks
        cleanup_result = "protected-cleanup-failed"
    elseif remaining ~= 0 then
        cleanup_result = "protected-cleanup-failed"
    end
    local terminal_status = status
    if terminal_status == "PASS" and cleanup_result ~= "protected-cleanup-ok" then
        terminal_status = "FAIL"
        phase = "finalizer"
        code = "callback-cleanup"
        actual = "callback-cleanup=" .. tostring(remaining)
        detail = "protected callback cleanup did not reach zero"
    end
    local wrote = write_status(
        terminal_status, phase, code, role, remaining, cleanup_result, expected, actual, detail
    )
    finished = terminal_status == "PASS" and wrote
    pcall(function() client.exitCode(finished and 0 or 1) end)
end

local function fail(phase, code, role, expected, actual, detail)
    finalize("FAIL", phase, code, role, expected, actual, detail)
end

local function dispatch(group)
    if finalizing then return end
    local callback_ok, callback_error = pcall(function()
        for _, role in ipairs(group.roles) do
            current_role = role
            observed_roles[#observed_roles + 1] = role
            if role == "declared-terminal" then
                finalize("PASS", "terminal", "pass", role, "ordered terminal role", role, "terminal role reached")
                return
            end
        end
    end)
    if not callback_ok then
        fail("callback", "callback-exception", current_role, "callback-success", tostring(callback_error), "protected callback failed")
    end
end

if status_path == nil or case_id == nil then
    fail("finalizer", "missing-configuration", nil, "scenario-status-path-and-case", "missing configuration", "status cannot be attributed without configuration")
    return
end

for _, group in ipairs(checkpoint_groups) do
    local ok, callback_id = pcall(function()
        return event.onmemoryexecute(function() dispatch(group) end, group.address, "System Bus")
    end)
    if not ok or callback_id == nil then
        fail("callback", "checkpoint-registration", nil, "checkpoint-registration", tostring(callback_id), "checkpoint callback registration failed")
        return
    end
    callbacks[#callbacks + 1] = callback_id
end

local exit_ok, exit_callback_id = pcall(function()
    return event.onexit(function()
        local callback_ok, callback_error = pcall(function()
            if not finalizing and not finished then
                fail("callback", "exit-before-terminal", current_role, "declared terminal role", "emulator exit before terminal role", "exit callback fired before terminal role")
            end
        end)
        if not callback_ok then
            fail("callback", "exit-callback-exception", current_role, "exit-callback-success", tostring(callback_error), "protected exit callback failed")
        end
    end, "original-reference-scenario-exit")
end)
if not exit_ok or exit_callback_id == nil then
    fail("callback", "exit-callback-registration", nil, "exit-callback-registration", tostring(exit_callback_id), "exit callback registration failed")
    return
end
callbacks[#callbacks + 1] = exit_callback_id
