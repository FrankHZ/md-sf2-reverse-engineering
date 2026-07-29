local config = assert(dofile(assert(os.getenv("SF2_H3_CONFIG"), "SF2_H3_CONFIG is not set")))

-- Probe seam: CheckSram's original return enters the real witchMenuAction_New.
-- The menu alias returns configured synthetic choices; NameAlly and DisplayText
-- return immediately. The harness pulses C only after the original New action
-- begins to release its text-macro waits. CheatModeConfiguration remains original
-- and sees a cleared PLAYER_1_INPUT byte, so its early no-Start return executes.
local phase, frame_count, running_action_input_frame = "await-check-sram", 0, 0
local case_index, active, records = 1, nil, {}
local replay_state, pending_replay, pending_save, pending_case_start = nil, false, false, false
local patch_readbacks = {}
local checkpoint_address = config.ram.workRamScratchAddress
local menu_thunk_address = checkpoint_address + 0x20
local main_loop_thunk_address = checkpoint_address + 0x40

local function status(value)
    local file = assert(io.open(config.statusPath, "a"))
    file:write(value .. "\n")
    file:close()
end

local function json_escape(value)
    return tostring(value):gsub("[\\\"%z\1-\31]", function(character)
        local byte = string.byte(character)
        if character == "\\" then return "\\\\" end
        if character == "\"" then return "\\\"" end
        if character == "\b" then return "\\b" end
        if character == "\f" then return "\\f" end
        if character == "\n" then return "\\n" end
        if character == "\r" then return "\\r" end
        if character == "\t" then return "\\t" end
        return string.format("\\u%04x", byte)
    end)
end

local function register(name)
    return emu.getregister("M68K " .. name) & 0xFFFFFFFF
end

local function write_work_ram_jump(target, address)
    memory.write_u16_be(address, 0x4EF9, "M68K BUS")
    memory.write_u32_be(address + 2, target, "M68K BUS")
end

local function write_work_ram_rts(address)
    memory.write_u16_be(address, 0x4E75, "M68K BUS")
end

local function patch_cart_word(address, value, label)
    -- GPGX ignores M68K-BUS writes to ROM. Record both domains before applying
    -- the session-only MD-CART patch that instruction fetches will observe.
    memory.write_u16_be(address, value, "M68K BUS")
    local bus_after_bus_write = memory.read_u16_be(address, "M68K BUS")
    local cart_after_bus_write = memory.read_u16_be(address, "MD CART")
    memory.write_u16_be(address, value, "MD CART")
    local bus_after_cart_write = memory.read_u16_be(address, "M68K BUS")
    local cart_after_cart_write = memory.read_u16_be(address, "MD CART")
    status(string.format(
        "patch-readback:%s:bus-after-bus=%04X,cart-after-bus=%04X,"
            .. "bus-after-cart=%04X,cart-after-cart=%04X",
        label, bus_after_bus_write, cart_after_bus_write, bus_after_cart_write,
        cart_after_cart_write
    ))
    patch_readbacks[#patch_readbacks + 1] = {
        label = label,
        busAfterBusWrite = bus_after_bus_write,
        cartAfterBusWrite = cart_after_bus_write,
        busAfterCartWrite = bus_after_cart_write,
        cartAfterCartWrite = cart_after_cart_write,
    }
    assert(cart_after_cart_write == value,
        "MD CART session patch did not persist for " .. label)
end

local function patch_cart_long(address, value, label)
    patch_cart_word(address, (value >> 16) & 0xFFFF, label .. "-high")
    patch_cart_word(address + 2, value & 0xFFFF, label .. "-low")
end

local function write_rom_jump(target, address, label)
    patch_cart_word(address, 0x4EF9, label .. "-opcode")
    patch_cart_long(address + 2, target, label .. "-target")
end

local function write_rom_rts(address, label)
    patch_cart_word(address, 0x4E75, label)
end

local function write_menu_thunk(case)
    -- cmpi.w #initialMenuPage,d1 ; bne.s difficulty ; moveq return,d0 ; rts
    memory.write_u16_be(menu_thunk_address, 0x0C41, "M68K BUS")
    memory.write_u16_be(menu_thunk_address + 2, config.newAction.initialMenuPage, "M68K BUS")
    memory.write_u16_be(menu_thunk_address + 4, 0x6604, "M68K BUS")
    memory.write_u16_be(
        menu_thunk_address + 6,
        0x7000 | (case.injectedInitialMenuReturn & 0xFF),
        "M68K BUS"
    )
    write_work_ram_rts(menu_thunk_address + 8)
    memory.write_u16_be(
        menu_thunk_address + 10,
        0x7000 | (case.injectedDifficultyMenuReturn & 0xFF),
        "M68K BUS"
    )
    write_work_ram_rts(menu_thunk_address + 12)
end

local function sram_domain_offset(physical_address)
    local delta = physical_address - config.storage.physicalWindowBaseAddress
    assert(delta >= 0 and delta < memory.getmemorydomainsize("SRAM"),
        "source physical SRAM address is outside the emulator SRAM domain")
    return delta
end

local function read_sram_byte(physical_address)
    return memory.read_u8(sram_domain_offset(physical_address), "SRAM")
end

local function read_slot(selector)
    local data_address, checksum_address
    if selector == 0 then
        data_address = config.storage.slot1DataAddress
        checksum_address = config.storage.slot1ChecksumAddress
    else
        data_address = config.storage.slot2DataAddress
        checksum_address = config.storage.slot2ChecksumAddress
    end
    local checksum, samples = 0, {}
    for offset = 0, config.storage.logicalPayloadByteCountPerSlot - 1 do
        local physical_address = data_address + offset * config.storage.physicalAddressStepPerLogicalByte
        checksum = (checksum + read_sram_byte(physical_address)) & 0xFF
    end
    for _, offset in ipairs(config.sampleOffsets) do
        local physical_address = data_address + offset * config.storage.physicalAddressStepPerLogicalByte
        samples[#samples + 1] = {
            logicalOffset = offset,
            physicalAddress = physical_address,
            storedPhysicalByte = read_sram_byte(physical_address),
        }
    end
    return {
        storedChecksumByte = read_sram_byte(checksum_address),
        computedChecksumByte = checksum,
        storedPayloadSamples = samples,
    }
end

local function flag_is_set(flag)
    local byte_address = config.ram.gameFlagsAddress + math.floor(flag / 8)
    local mask = 0x80 >> (flag % 8)
    return (memory.read_u8(byte_address, "M68K BUS") & mask) ~= 0
end

local function write_patch_readbacks(output)
    output:write('"romPatchReadbacks":[')
    for index, readback in ipairs(patch_readbacks) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"label":"%s","busAfterBusWrite":%d,"cartAfterBusWrite":%d,'
                .. '"busAfterCartWrite":%d,"cartAfterCartWrite":%d}',
            json_escape(readback.label), readback.busAfterBusWrite, readback.cartAfterBusWrite,
            readback.busAfterCartWrite, readback.cartAfterCartWrite
        ))
    end
    output:write("]")
end

local function write_samples(output, samples)
    output:write('"storedPayloadSamples":[')
    for index, sample in ipairs(samples) do
        if index > 1 then output:write(",") end
        output:write(string.format(
            '{"logicalOffset":%d,"physicalAddress":%d,"storedPhysicalByte":%d}',
            sample.logicalOffset, sample.physicalAddress, sample.storedPhysicalByte
        ))
    end
    output:write("]")
end

local function write_record(output, record)
    output:write(string.format(
        '{"id":"%s","preconditionSaveFlags":%d,',
        json_escape(record.id), record.preconditionSaveFlags
    ))
    output:write(string.format(
        '"initialMenu":{"observedInitialSelector":%d,"observedPage":%d,'
            .. '"observedAvailability":%d,"injectedReturn":%d},',
        record.initialMenu.observedInitialSelector, record.initialMenu.observedPage,
        record.initialMenu.observedAvailability, record.initialMenu.injectedReturn
    ))
    output:write(string.format(
        '"difficultyMenu":{"observedSelector":%d,"observedPage":%d,'
            .. '"observedAvailability":%d,"injectedReturn":%d},',
        record.difficultyMenu.observedSelector, record.difficultyMenu.observedPage,
        record.difficultyMenu.observedAvailability, record.difficultyMenu.injectedReturn
    ))
    output:write(string.format(
        '"seams":{"initialMenuAliasBypassed":true,"difficultyMenuAliasBypassed":true,'
            .. '"nameAllyAliasBypassed":%s,"cheatModeConfigurationExecuted":%s,'
            .. '"displayTextBypassCalls":%d,"newGameAliasExecuted":%s,'
            .. '"newGameEffectiveTargetExecuted":%s},',
        tostring(record.seams.nameAllyAliasBypassed),
        tostring(record.seams.cheatModeConfigurationExecuted),
        record.seams.displayTextBypassCalls,
        tostring(record.seams.newGameAliasExecuted),
        tostring(record.seams.newGameEffectiveTargetExecuted)
    ))
    output:write(string.format(
        '"handoff":{"currentSaveSlot":%d,"currentMap":%d,"egressMap":%d,'
            .. '"d0":%d,"d1":%d,"d2":%d,"d3":%d,"d4":%d},',
        record.handoff.currentSaveSlot, record.handoff.currentMap, record.handoff.egressMap,
        record.handoff.d0, record.handoff.d1, record.handoff.d2, record.handoff.d3,
        record.handoff.d4
    ))
    output:write(string.format(
        '"difficultyFlags":{"flag78Set":%s,"flag79Set":%s},',
        tostring(record.difficultyFlags.flag78Set), tostring(record.difficultyFlags.flag79Set)
    ))
    output:write(string.format(
        '"savedSlot":{"selector":%d,"saveFlagsByte":%d,"storedChecksumByte":%d,'
            .. '"computedChecksumByte":%d,',
        record.savedSlot.selector, record.savedSlot.saveFlagsByte,
        record.savedSlot.storedChecksumByte, record.savedSlot.computedChecksumByte
    ))
    write_samples(output, record.savedSlot.storedPayloadSamples)
    output:write("}}")
end

local function write_observation()
    if replay_state then memorysavestate.removestate(replay_state) end
    local output = assert(io.open(config.outputPath, "w"))
    output:write(string.format(
        '{"system":"%s","core":"%s","id":"%s","harness":{'
            .. '"checkSramReturnTrampoline":true,"maxFrames":%d,"romPatchDomain":"MD CART",'
            .. '"textWaitHarnessControl":"C-pulse-after-new-action-entry",',
        json_escape(emu.getsystemid()), json_escape(config.core), json_escape(config.fixtureId),
        config.harness.maxFrames
    ))
    write_patch_readbacks(output)
    output:write('},"records":[')
    for index, record in ipairs(records) do
        if index > 1 then output:write(",") end
        write_record(output, record)
    end
    output:write("]}\n")
    output:close()
    status("milestone:all-main-loop-handoffs-observed")
    client.exitCode(0)
end

local function install_session_seams()
    write_work_ram_jump(checkpoint_address, checkpoint_address)
    write_rom_jump(
        menu_thunk_address, config["function"].menuInstructionTargetAddress, "menu-alias"
    )
    write_rom_rts(config["function"].nameAllyInstructionTargetAddress, "name-ally-alias")
    write_rom_rts(config["function"].displayTextAddress, "display-text")
    write_rom_jump(main_loop_thunk_address, config["function"].mainLoopAddress, "main-loop")
    phase = "await-checkpoint"
    status("milestone:check-sram-return-trampoline-installed")
end

local function start_case()
    local definition = config.cases[case_index]
    assert(definition ~= nil, "case index exceeds configured witch New lifecycle matrix")
    active = {
        id = definition.id,
        preconditionSaveFlags = definition.preconditionSaveFlags,
        injectedInitialMenuReturn = definition.injectedInitialMenuReturn,
        injectedDifficultyMenuReturn = definition.injectedDifficultyMenuReturn,
        seams = {
            newGameAliasExecuted = false,
            newGameEffectiveTargetExecuted = false,
            nameAllyAliasBypassed = false,
            cheatModeConfigurationExecuted = false,
            displayTextBypassCalls = 0,
        },
    }
    memory.write_u8(
        config.storage.saveFlagsAddress - config.storage.physicalWindowBaseAddress,
        active.preconditionSaveFlags,
        "SRAM"
    )
    memory.write_u8(config.ram.player1InputAddress, 0, "M68K BUS")
    write_menu_thunk(active)
    write_work_ram_jump(config["function"].newActionAddress, checkpoint_address)
    running_action_input_frame = 0
    phase = "await-new-action"
    status("milestone:case-started:" .. active.id)
end

event.on_bus_exec(function()
    if phase ~= "await-check-sram" then return end
    local return_address = register("A7") & 0xFFFFFF
    local original_return = memory.read_u32_be(return_address, "M68K BUS")
    memory.write_u32_be(return_address, checkpoint_address, "M68K BUS")
    install_session_seams()
    status(string.format(
        "check-sram-return:sp=%X,original=%X,replaced=%X,checkpoint=%04X/%04X/%04X",
        return_address, original_return, memory.read_u32_be(return_address, "M68K BUS"),
        memory.read_u16_be(checkpoint_address, "M68K BUS"),
        memory.read_u16_be(checkpoint_address + 2, "M68K BUS"),
        memory.read_u16_be(checkpoint_address + 4, "M68K BUS")
    ))
end, config["function"].checkSramAddress, "sf2-witch-new-check-sram", "M68K BUS")

event.on_bus_exec(function()
    if phase ~= "await-checkpoint" then return end
    status("milestone:checkpoint-entered-before-core-state")
    if replay_state == nil then pending_save = true end
    pending_case_start = true
    phase = "await-safe-checkpoint"
end, checkpoint_address, "sf2-witch-new-checkpoint", "M68K BUS")

event.on_bus_exec(function()
    if phase == "await-new-action" then
        phase = "running-new-action"
        status("milestone:original-witch-new-action-entered:" .. active.id)
    end
end, config["function"].newActionAddress, "sf2-witch-new-action", "M68K BUS")

event.on_bus_exec(function()
    if phase == "running-new-action" then
        local page = register("D1") & 0xFFFF
        if page == config.newAction.initialMenuPage then
            assert(active.initialMenu == nil, "initial menu alias was entered more than once")
            active.initialMenu = {
                observedInitialSelector = register("D0") & 0xFFFF,
                observedPage = page,
                observedAvailability = register("D2") & 0xFFFF,
                injectedReturn = active.injectedInitialMenuReturn,
            }
        elseif page == config.newAction.difficultyMenuPage then
            assert(active.difficultyMenu == nil, "difficulty menu alias was entered more than once")
            active.difficultyMenu = {
                observedSelector = register("D0") & 0xFFFF,
                observedPage = page,
                observedAvailability = register("D2") & 0xFFFF,
                injectedReturn = active.injectedDifficultyMenuReturn,
            }
        else
            error("injected menu alias was reached with an unmodelled page " .. tostring(page))
        end
    end
end, config["function"].menuInstructionTargetAddress, "sf2-witch-new-menu-alias", "M68K BUS")

event.on_bus_exec(function()
    if phase == "running-new-action" then active.seams.newGameAliasExecuted = true end
end, config["function"].newGameInstructionTargetAddress, "sf2-witch-new-newgame-alias", "M68K BUS")

event.on_bus_exec(function()
    if phase == "running-new-action" then active.seams.newGameEffectiveTargetExecuted = true end
end, config["function"].newGameEffectiveTargetAddress, "sf2-witch-new-newgame-effective", "M68K BUS")

event.on_bus_exec(function()
    if phase == "running-new-action" then active.seams.nameAllyAliasBypassed = true end
end, config["function"].nameAllyInstructionTargetAddress, "sf2-witch-new-name-alias", "M68K BUS")

event.on_bus_exec(function()
    if phase == "running-new-action" then active.seams.cheatModeConfigurationExecuted = true end
end, config["function"].cheatModeConfigurationAddress, "sf2-witch-new-configuration", "M68K BUS")

event.on_bus_exec(function()
    if phase == "running-new-action" then
        active.seams.displayTextBypassCalls = active.seams.displayTextBypassCalls + 1
    end
end, config["function"].displayTextAddress, "sf2-witch-new-display-text", "M68K BUS")

event.on_bus_exec(function()
    if phase == "running-new-action" then active.saveGameSelector = register("D0") & 1 end
end, config["function"].saveGameAddress, "sf2-witch-new-save-game", "M68K BUS")

event.on_bus_exec(function()
    if phase ~= "running-new-action" then return end
    assert(active.initialMenu ~= nil, "MainLoop reached without initial menu alias observation")
    assert(active.difficultyMenu ~= nil, "MainLoop reached without difficulty menu alias observation")
    assert(active.saveGameSelector ~= nil, "MainLoop reached without original SaveGame entry")
    local storage = read_slot(active.saveGameSelector)
    records[#records + 1] = {
        id = active.id,
        preconditionSaveFlags = active.preconditionSaveFlags,
        initialMenu = active.initialMenu,
        difficultyMenu = active.difficultyMenu,
        seams = active.seams,
        handoff = {
            currentSaveSlot = memory.read_u16_be(config.ram.currentSaveSlotAddress, "M68K BUS"),
            currentMap = memory.read_u8(config.ram.currentMapAddress, "M68K BUS"),
            egressMap = memory.read_u8(config.ram.egressMapAddress, "M68K BUS"),
            d0 = register("D0") & 0xFFFF,
            d1 = register("D1") & 0xFFFF,
            d2 = register("D2") & 0xFFFF,
            d3 = register("D3") & 0xFFFF,
            d4 = register("D4") & 0xFFFF,
        },
        difficultyFlags = {
            flag78Set = flag_is_set(config.newAction.sourceFlag78),
            flag79Set = flag_is_set(config.newAction.sourceFlag79),
        },
        savedSlot = {
            selector = active.saveGameSelector,
            saveFlagsByte = read_sram_byte(config.storage.saveFlagsAddress),
            storedChecksumByte = storage.storedChecksumByte,
            computedChecksumByte = storage.computedChecksumByte,
            storedPayloadSamples = storage.storedPayloadSamples,
        },
    }
    status("milestone:main-loop-handoff-observed:" .. active.id)
    if case_index == #config.cases then
        write_observation()
    else
        case_index = case_index + 1
        active = nil
        pending_replay = true
        phase = "await-replay"
    end
end, main_loop_thunk_address, "sf2-witch-new-main-loop-thunk", "M68K BUS")

while true do
    frame_count = frame_count + 1
    if frame_count > config.harness.maxFrames then
        status(string.format(
            "milestone:timeout:frame=%d,max-frames=%d,phase=%s,pc=%X",
            frame_count, config.harness.maxFrames, phase, register("PC")
        ))
        client.exitCode(1)
        return
    end
    if pending_replay then
        pending_replay = false
        memorysavestate.loadcorestate(replay_state)
        phase = "await-checkpoint"
        status("milestone:fresh-core-state-replayed:" .. config.cases[case_index].id)
    elseif pending_save then
        pending_save = false
        replay_state = memorysavestate.savecorestate()
        status("milestone:fresh-core-state-saved")
    elseif pending_case_start then
        pending_case_start = false
        start_case()
    end
    if phase == "await-check-sram" then
        joypad.set({ Start = true }, 1)
    elseif phase == "running-new-action" then
        local cycle_frame = running_action_input_frame % 42
        if cycle_frame >= 30 and cycle_frame < 34 then
            joypad.set({ C = true }, 1)
        else
            joypad.set({}, 1)
        end
        running_action_input_frame = running_action_input_frame + 1
    else
        joypad.set({}, 1)
    end
    joypad.set({}, 2)
    emu.frameadvance()
    if frame_count % 600 == 0 then
        status(string.format("frame=%d,phase=%s,pc=%X", frame_count, phase, register("PC")))
    end
end
