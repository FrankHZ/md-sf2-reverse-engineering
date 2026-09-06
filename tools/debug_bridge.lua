-- Bounded, single-controller experiment for the pinned Windows BizHawk release.
-- Commands are tab-separated data; no received text is evaluated as Lua.
local json = dofile(assert(os.getenv('SF2_BRIDGE_JSON')))
local status_path = assert(os.getenv('SF2_BRIDGE_STATUS'))
local callback_id, callback_error, hit = nil, nil, nil
local callback_calls = 0

local function status(state, detail)
  json.write(status_path, {state=state, detail=detail, callbackActive=callback_id ~= nil,
    callbackCalls=callback_calls, callbackError=callback_error})
end

local function clear_callback()
  if callback_id then
    assert(event.unregisterbyid(callback_id), 'callback unregister failed')
    callback_id = nil
  end
end

local function snapshot()
  return {frame=emu.framecount(), paused=client.ispaused(), registers=emu.getregisters(),
    callbackActive=callback_id ~= nil, callbackCalls=callback_calls}
end

local function send(value)
  local payload = json.encode(value)
  local expected = #tostring(#payload) + 1 + #payload
  assert(comm.socketServerSend(payload) == expected, 'short socket send')
end

local function integer(value, minimum, maximum)
  assert(type(value) == 'string' and value:match('^%d+$'), 'expected decimal integer')
  local n = tonumber(value)
  assert(n and n >= minimum and n <= maximum and n % 1 == 0, 'integer out of range')
  return n
end

local function fields(text)
  assert(#text > 0 and #text <= 512, 'invalid command size')
  local result = {}
  for part in (text .. '\t'):gmatch('(.-)\t') do result[#result + 1] = part end
  return result
end

-- The public Lua libraries expose system ID but no loaded core name. Read the
-- current process's MainForm.Emulator and its CoreAttribute, never a config echo.
local function core_identity()
  luanet.load_assembly('System.Windows.Forms')
  local application = luanet.import_type('System.Windows.Forms.Application')
  local forms = application.OpenForms:GetEnumerator()
  while forms:MoveNext() do
    local form = forms.Current
    if form:GetType().FullName == 'BizHawk.Client.EmuHawk.MainForm' then
      local core_type = form.Emulator:GetType()
      local attributes = core_type:GetCustomAttributes(false)
      local enumerator = attributes:GetEnumerator()
      while enumerator:MoveNext() do
        local attribute = enumerator.Current
        if attribute:GetType().Name == 'CoreAttribute'
          or attribute:GetType().Name == 'PortedCoreAttribute' then
          return {name=attribute.CoreName, type=core_type.FullName}
        end
      end
    end
  end
  error('loaded core identity unavailable')
end

local function neutral_frame()
  local neutral = {}
  for button, _ in pairs(joypad.get()) do neutral[button] = false end
  joypad.set(neutral)
  client.unpause()
  emu.frameadvance()
  client.pause()
  assert(not callback_error, callback_error)
end

local function main()
  client.pause()
  status('starting')
  comm.socketServerSetTimeout(5000)
  local domains = {}
  for _, name in pairs(memory.getmemorydomainlist()) do
    domains[name] = memory.getmemorydomainsize(name)
  end
  local core = core_identity()
  assert(core.type == 'BizHawk.Emulation.Cores.Consoles.Sega.gpgx.GPGX',
    'unsupported core: ' .. core.type)
  send({protocol=1, token=os.getenv('SF2_BRIDGE_TOKEN'), core=core,
    version=client.getversion(), system=emu.getsystemid(), domains=domains,
    state=snapshot()})
  status('ready')
  local previous_id = 0
  while true do
    local wire = comm.socketServerResponse()
    assert(wire and #wire > 0, 'socket timeout or incomplete message')
    local command = fields(wire)
    local id = integer(command[1], previous_id + 1, 1000000)
    previous_id = id
    local op = command[2]
    status('command', op)
    local function arity(count) assert(#command == count, 'wrong argument count') end
    local ok, result = pcall(function()
      if op == 'ping' or op == 'state' then
        arity(2)
        return snapshot()
      elseif op == 'read' then
        arity(5)
        local domain = command[3]
        -- Explicit RAM allowlist: no bus, cartridge ROM, or device registers.
        assert(domain == '68K RAM' and domains[domain], 'unsupported RAM domain')
        local address = integer(command[4], 0, domains[domain] - 1)
        local count = integer(command[5], 1, 64)
        assert(address + count <= domains[domain], 'RAM range exceeds domain')
        local bytes = {}
        for offset = 0, count - 1 do
          bytes[#bytes + 1] = memory.read_u8(address + offset, domain)
        end
        return {domain=domain, address=address, bytes=bytes, frame=emu.framecount()}
      elseif op == 'advance' then
        arity(3)
        local count = integer(command[3], 1, 120)
        local before = emu.framecount()
        for _ = 1, count do neutral_frame() end
        client.pause()
        assert(emu.framecount() - before == count, 'frame advance drift')
        return snapshot()
      elseif op == 'watch' then
        arity(4)
        assert(command[3] == 'M68K BUS', 'unsupported callback scope')
        assert(not callback_id, 'callback already active')
        local address = integer(command[4], 0, 0xFFFFFF)
        assert(address % 2 == 0, 'execution address must be aligned')
        hit, callback_error, callback_calls = nil, nil, 0
        callback_id = event.on_bus_exec(function(actual, _, flags)
          local success, failure = pcall(function()
            callback_calls = callback_calls + 1
            if not hit then
              assert(actual == address, 'execution callback address mismatch')
              hit = {address=actual, flags=flags, frame=emu.framecount(),
                pc=emu.getregister('M68K PC')}
            end
          end)
          if not success then callback_error = tostring(failure) end
        end, address, 'sf2-debug-bridge-exec', 'M68K BUS')
        assert(callback_id and callback_id ~= '00000000-0000-0000-0000-000000000000',
          'execution callbacks unavailable')
        return snapshot()
      elseif op == 'run' then
        arity(3)
        local count = integer(command[3], 1, 120)
        assert(callback_id and not hit, 'run requires a fresh watch')
        local before = emu.framecount()
        for _ = 1, count do
          neutral_frame()
          if hit then break end
        end
        client.pause()
        clear_callback()
        return {event=hit or json.null, advanced=emu.framecount()-before,
          state=snapshot(), stopBoundary='frame-end'}
      elseif op == 'clear' then
        arity(2)
        clear_callback()
        return snapshot()
      elseif op == 'quit' then
        arity(2)
        clear_callback()
        return snapshot()
      end
      error('unknown command')
    end)
    -- Callback exceptions are fatal even though NLua normally logs/swallow them.
    assert(not callback_error, callback_error)
    send({id=id, ok=ok, result=ok and result or json.null,
      error=not ok and tostring(result) or json.null})
    if op == 'quit' and ok then
      status('closed', 'quit')
      return
    end
    status('ready')
    emu.yield()
  end
end

local ok, failure = pcall(main)
local cleaned, cleanup_error = pcall(clear_callback)
if not ok or not cleaned then
  status('failed', tostring(failure or cleanup_error))
  client.exitCode(1)
else
  client.exitCode(0)
end
