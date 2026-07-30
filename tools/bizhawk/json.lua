local json = {}
json.null = {}

local function string_value(value)
  return '"' .. value:gsub('[%z\1-\31\\"]', function(character)
    local escaped = {
      ['\\'] = '\\\\', ['"'] = '\\"', ['\b'] = '\\b', ['\f'] = '\\f',
      ['\n'] = '\\n', ['\r'] = '\\r', ['\t'] = '\\t',
    }
    return escaped[character] or string.format('\\u%04x', character:byte())
  end) .. '"'
end

local function array_length(value)
  local count = 0
  for key, _ in pairs(value) do
    if type(key) ~= 'number' or key < 1 or key % 1 ~= 0 then return nil end
    count = count + 1
  end
  for index = 1, count do
    if value[index] == nil then return nil end
  end
  return count
end

local function encode(value, active)
  if value == json.null then return 'null' end
  local kind = type(value)
  if kind == 'nil' then return 'null' end
  if kind == 'boolean' then return value and 'true' or 'false' end
  if kind == 'number' then
    assert(value == value and value ~= math.huge and value ~= -math.huge, 'non-finite JSON number')
    return tostring(value)
  end
  if kind == 'string' then return string_value(value) end
  assert(kind == 'table', 'unsupported JSON value type: ' .. kind)
  assert(not active[value], 'cyclic JSON table')
  active[value] = true
  local length = array_length(value)
  local parts = {}
  if length then
    for index = 1, length do parts[index] = encode(value[index], active) end
    active[value] = nil
    return '[' .. table.concat(parts, ',') .. ']'
  end
  local keys = {}
  for key, _ in pairs(value) do
    assert(type(key) == 'string', 'JSON object key must be a string')
    keys[#keys + 1] = key
  end
  table.sort(keys)
  for index, key in ipairs(keys) do
    parts[index] = string_value(key) .. ':' .. encode(value[key], active)
  end
  active[value] = nil
  return '{' .. table.concat(parts, ',') .. '}'
end

function json.encode(value)
  return encode(value, {})
end

function json.write(path, value)
  local file = assert(io.open(path, 'w'))
  file:write(json.encode(value) .. '\n')
  file:close()
end

return json
