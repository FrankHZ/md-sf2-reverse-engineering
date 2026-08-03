local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))

local f,storage,layout,ram=config.static.functionEntries,config.static.storage,config.static.layout,config.static.ram
local a=config.static.addresses
local probe_base,case_base,probe_stride,stack_top=0xFF6800,0xFF6820,20,0xFFFF00
local callbacks,event_ids,records={}, {}, {}
local active,case_index,observer_failed,session_cleaned,bootstrapped=false,1,false,false,false
local copy_load_seen,copy_save_seen=false,false
local current_phase,current_role,current_pc,current_expectation="registration","registration",nil,nil
local write_probe
local operation_functions={check="CheckSram",save="SaveGame",load="LoadGame",copy="CopySave",clear="ClearSaveSlotFlag"}
local operation_roles={check="CheckSram",save="SaveGame",load="LoadGame",copy="CopySave",clear="ClearSaveSlotFlag"}

local function status(value) local file=assert(io.open(config.statusPath,"a"));file:write(value.."\n");file:close() end
local function bool(value) return value and "true" or "false" end
local function nullable(value) return value==nil and "null" or tostring(value) end
local function json_string(value) return string.format("%q",value) end
local function signed_word(value) value=value&0xFFFF;return value>=0x8000 and value-0x10000 or value end
local function current_case() return config.cases[case_index] end
local function pc_for(index) return case_base+(index-1)*probe_stride end
local function function_for(case) return f[operation_functions[case.operation]] end
local function selected_slot(selector) return selector==0 and "slot1" or "slot2" end
local function other_slot(slot) return slot=="slot1" and "slot2" or "slot1" end
local function pattern(seed,offset) return (seed+17*offset+29*(offset//8))&0xFF end
local function sram_offset(address)
  local offset=address-storage.physicalWindowBaseAddress
  assert(offset>=0 and offset<memory.getmemorydomainsize("SRAM"),"SRAM physical address outside emulator domain")
  return offset
end
local function read_sram(address) return memory.read_u8(sram_offset(address),"SRAM") end
local function write_sram(address,value) memory.write_u8(sram_offset(address),value&0xFF,"SRAM") end
local function logical_address(base,offset) return base+offset*layout.physicalAddressStepPerLogicalByte end
local function write_pattern_to_sram(slot,seed)
  local base=storage.slotDataAddresses[slot]
  for offset=0,layout.logicalBytesPerSlot-1 do write_sram(logical_address(base,offset),pattern(seed,offset)) end
end
local function write_pattern_to_ram(seed)
  for offset=0,layout.logicalBytesPerSlot-1 do memory.write_u8(ram.combatantDataAddress+offset,pattern(seed,offset),"M68K BUS") end
end
local function clear_sram()
  for offset=0,layout.fullClearLogicalByteCount-1 do write_sram(a.SRAM_START+offset*layout.physicalAddressStepPerLogicalByte,0) end
end
local function restore_sram_zero()
  clear_sram()
  for offset=0,layout.fullClearLogicalByteCount-1 do
    if read_sram(a.SRAM_START+offset*layout.physicalAddressStepPerLogicalByte)~=0 then return false end
  end
  return true
end
local function checksum_seed(seed)
  local total=0
  for offset=0,layout.logicalBytesPerSlot-1 do total=(total+pattern(seed,offset))&0xFF end
  return total
end
local function expected_seed(case,slot)
  if case.operation=="check" and case.setup.signature=="mismatch" then return nil end
  if case.operation=="save" and selected_slot(case.selector)==slot then return case.setup.ramSeed end
  if case.operation=="copy" and other_slot(selected_slot(case.selector))==slot then return case.setup[selected_slot(case.selector).."Seed"] end
  return case.setup[slot.."Seed"]
end
local function expected_slot_checksum(case,slot,seed)
  if case.operation=="check" and case.setup.signature=="mismatch" then return 0 end
  local mode=case.setup[slot.."Checksum"]
  if case.operation=="save" and selected_slot(case.selector)==slot then mode="computed" end
  if case.operation=="copy" and other_slot(selected_slot(case.selector))==slot then mode="computed" end
  local checksum=checksum_seed(seed)
  return mode=="computed" and checksum or ((checksum+1)&0xFF)
end
local function span_json(read_byte,expected_byte,size)
  local checksum,mismatches=0,0
  local first,last=read_byte(0),read_byte(size-1)
  local sentinels={}
  for offset=0,size-1 do
    local actual=read_byte(offset);checksum=(checksum+actual)&0xFF
    if actual~=expected_byte(offset) then mismatches=mismatches+1 end
  end
  for _,offset in ipairs(config.pattern.sentinelOffsets) do sentinels[#sentinels+1]={logicalOffset=offset,byte=read_byte(offset)} end
  return {logicalByteCount=size,checksumByte=checksum,mismatchCount=mismatches,boundary={first=first,last=last},sentinels=sentinels}
end
local function slot_fact(case,slot)
  local seed=expected_seed(case,slot)
  local base=storage.slotDataAddresses[slot]
  local expected=function(offset) return seed==nil and 0 or pattern(seed,offset) end
  return {slot=slot,storedChecksumByte=read_sram(storage.slotChecksumAddresses[slot]),span=span_json(function(offset) return read_sram(logical_address(base,offset)) end,expected,layout.logicalBytesPerSlot)}
end
local function combatant_fact(case)
  if case.operation~="save" and case.operation~="load" and case.operation~="copy" then return nil end
  local slot=selected_slot(case.selector)
  local seed=case.operation=="save" and case.setup.ramSeed or case.setup[slot.."Seed"]
  return span_json(function(offset) return memory.read_u8(ram.combatantDataAddress+offset,"M68K BUS") end,function(offset) return pattern(seed,offset) end,layout.logicalBytesPerSlot)
end
local function full_sram_fact(case)
  if case.operation~="check" or case.setup.signature~="mismatch" then return nil end
  local signature=config.static.signatureBytes
  local signature_index=(storage.signatureAddress-a.SRAM_START)//layout.physicalAddressStepPerLogicalByte
  local checksum,mismatches=0,0
  local first,last=read_sram(a.SRAM_START),read_sram(a.SRAM_START+(layout.fullClearLogicalByteCount-1)*layout.physicalAddressStepPerLogicalByte)
  for offset=0,layout.fullClearLogicalByteCount-1 do
    local actual=read_sram(a.SRAM_START+offset*layout.physicalAddressStepPerLogicalByte)
    local expected=offset>=signature_index and offset<signature_index+#signature and signature[offset-signature_index+1] or 0
    checksum=(checksum+actual)&0xFF;if actual~=expected then mismatches=mismatches+1 end
  end
  return {logicalByteCount=layout.fullClearLogicalByteCount,checksumByte=checksum,mismatchCount=mismatches,boundary={first=first,last=last}}
end
local function unregister_events() for index=#event_ids,1,-1 do event.unregisterbyid(event_ids[index]);event_ids[index]=nil end end
local function cleanup_session() if session_cleaned then return end;session_cleaned=true;unregister_events() end
local function roles_json(address)
  if address==nil then return "[]" end
  local roles={};for _,entry in ipairs(callbacks[address] or {}) do roles[#roles+1]=entry.role end
  local parts={};for _,role in ipairs(roles) do parts[#parts+1]=json_string(role) end
  return "["..table.concat(parts,",").."]"
end
local function pending_callback_state()
  return "{\"active\":"..bool(active)..",\"caseIndex\":"..case_index..",\"copyLoadSeen\":"..bool(copy_load_seen)..",\"copySaveSeen\":"..bool(copy_save_seen)..",\"expectedFunctionPc\":"..nullable(active and function_for(current_case()) or nil)..",\"pendingReturnPc\":"..nullable(active and (pc_for(case_index)+12) or nil)..",\"rolesAtPc\":"..roles_json(current_pc).."}"
end
local function fail_callback(message)
  if observer_failed then return end
  observer_failed=true
  local case=current_case();local expected=current_expectation or {}
  local payload="{\"owner\":"..json_string(config.observerFailureContract.owner)..",\"caseId\":"..(case and json_string(case.id) or "null")..",\"phase\":"..json_string(current_phase)..",\"role\":"..json_string(current_role)..",\"actualPc\":"..nullable(emu.getregister("M68K PC"))..",\"expectedEventPc\":"..nullable(expected.eventPc)..",\"expectedCallPc\":"..nullable(expected.callPc)..",\"expectedTargetPc\":"..nullable(expected.targetPc)..",\"expectedReturnPc\":"..nullable(expected.returnPc)..",\"pendingCallback\":"..pending_callback_state()..",\"error\":"..json_string(tostring(message)).."}"
  local diagnostic=config.observerFailureContract.statusPrefix..payload
  status(diagnostic);print(diagnostic);os.remove(config.outputPath);restore_sram_zero();cleanup_session();client.exitCode(config.observerFailureContract.exitCode)
end
local function expectation(phase,case,address)
  local entry=pc_for(case_index)
  if phase=="case-entry" then return {eventPc=entry,callPc=entry+6,targetPc=function_for(case),returnPc=entry+12} end
  if phase=="function-entry" then return {eventPc=function_for(case),callPc=entry+6,targetPc=function_for(case),returnPc=entry+12} end
  return {eventPc=entry+12,callPc=entry+6,targetPc=function_for(case),returnPc=entry+12}
end
local function prepare_case(case)
  clear_sram()
  if case.setup.signature=="mismatch" then
    for offset=0,layout.fullClearLogicalByteCount-1 do write_sram(a.SRAM_START+offset*layout.physicalAddressStepPerLogicalByte,pattern(case.setup.slot1Seed,offset)) end
  end
  write_pattern_to_sram("slot1",case.setup.slot1Seed);write_pattern_to_sram("slot2",case.setup.slot2Seed)
  if case.setup.signature=="valid" then
    for index,byte in ipairs(config.static.signatureBytes) do write_sram(storage.signatureAddress+(index-1)*layout.physicalAddressStepPerLogicalByte,byte) end
  end
  write_sram(storage.slotChecksumAddresses.slot1,expected_slot_checksum(case,"slot1",case.setup.slot1Seed))
  write_sram(storage.slotChecksumAddresses.slot2,expected_slot_checksum(case,"slot2",case.setup.slot2Seed))
  write_sram(storage.saveFlagsAddress,case.setup.flags)
  write_pattern_to_ram(case.setup.ramSeed)
end
local function start_case(index)
  if active or index~=case_index then error("direct case entry dispatch drift") end
  local case=current_case();if not case then error("direct case table exhausted") end
  current_phase,current_role,current_expectation="case-entry","direct-case-entry",expectation("case-entry",case,pc_for(index))
  if emu.getregister("M68K PC")~=current_expectation.eventPc then error("direct case-entry PC drift") end
  prepare_case(case);active=true
  copy_load_seen,copy_save_seen=false,false
end
local function function_entry(address)
  if not active then
    if not bootstrapped and address==f.CheckSram then
      current_phase,current_role,current_expectation="bootstrap-check-sram","bootstrap-return-redirect",{eventPc=address,callPc=nil,targetPc=address,returnPc=probe_base}
      local stack=emu.getregister("M68K A7")&0xFFFFFF
      if stack<0xFF0000 or stack>0xFFFFFF then error("CheckSram return stack outside work RAM") end
      memory.write_u32_be(stack,probe_base,"M68K BUS")
      if memory.read_u32_be(stack,"M68K BUS")~=probe_base then error("CheckSram return thunk write drift") end
      write_probe()
      bootstrapped=true;status("milestone:direct-function-probe")
    end
    return
  end
  local case=current_case()
  if address==function_for(case) then
    current_phase,current_role,current_expectation="function-entry",operation_roles[case.operation],expectation("function-entry",case,address)
  elseif case.operation=="copy" and address==f.LoadGame then
    current_phase,current_role,current_expectation="copy-load-entry","CopySave->LoadGame",{eventPc=address,callPc=config.static.copyFlow.loadCallPc,targetPc=address,returnPc=config.static.copyFlow.loadReturnPc}
    copy_load_seen=true
  elseif case.operation=="copy" and address==f.SaveGame then
    current_phase,current_role,current_expectation="copy-save-entry","CopySave->SaveGame",{eventPc=address,callPc=config.static.copyFlow.saveCallPc,targetPc=address,returnPc=config.static.copyFlow.saveReturnPc}
    copy_save_seen=true
  else
    current_phase,current_role,current_expectation="unexpected-function-entry","unexpected:"..address,expectation("function-entry",case,address)
    error("unexpected SRAM function target")
  end
  if address~=current_expectation.targetPc or emu.getregister("M68K PC")~=address then error("direct function target PC drift") end
end
local function span_json_text(span)
  local sentinels={};for _,item in ipairs(span.sentinels) do sentinels[#sentinels+1]="{\"logicalOffset\":"..item.logicalOffset..",\"byte\":"..item.byte.."}" end
  return "{\"logicalByteCount\":"..span.logicalByteCount..",\"checksumByte\":"..span.checksumByte..",\"mismatchCount\":"..span.mismatchCount..",\"boundary\":{\"first\":"..span.boundary.first..",\"last\":"..span.boundary.last.."},\"sentinels\":["..table.concat(sentinels,",").."]}"
end
local function slot_fact_text(fact) return "{\"slot\":"..json_string(fact.slot)..",\"storedChecksumByte\":"..fact.storedChecksumByte..",\"span\":"..span_json_text(fact.span).."}" end
local function record_text(record)
  local slots={};for _,fact in ipairs(record.slotFacts) do slots[#slots+1]=slot_fact_text(fact) end
  local full="null";if record.fullSramFact then local v=record.fullSramFact;full="{\"logicalByteCount\":"..v.logicalByteCount..",\"checksumByte\":"..v.checksumByte..",\"mismatchCount\":"..v.mismatchCount..",\"boundary\":{\"first\":"..v.boundary.first..",\"last\":"..v.boundary.last.."}}" end
  return "{\"id\":"..json_string(record.id)..",\"resultD0\":"..record.resultD0..",\"resultD1\":"..record.resultD1..",\"saveFlags\":"..record.saveFlags..",\"slotFacts\":["..table.concat(slots,",").."],\"combatantFacts\":"..(record.combatantFacts and span_json_text(record.combatantFacts) or "null")..",\"fullSramFact\":"..full.."}"
end
local function write_output(residue_zero)
  local output=assert(io.open(config.outputPath,"w"));local parts={}
  for _,record in ipairs(records) do parts[#parts+1]=record_text(record) end
  local order={};for _,id in ipairs(config.caseOrder) do order[#order+1]=json_string(id) end
  output:write("{\"system\":"..json_string(emu.getsystemid())..",\"core\":"..json_string(config.core)..",\"id\":"..json_string(config.id)..",\"caseOrder\":["..table.concat(order,",").."],\"records\":["..table.concat(parts,",").."],\"sramResidueZero\":"..bool(residue_zero)..",\"callbacksCleared\":0}")
  output:close()
end
local function finish_case(index)
  if not active or index~=case_index then error("direct case result dispatch drift") end
  local case=current_case();current_phase,current_role,current_expectation="case-result","direct-case-result",expectation("case-result",case,pc_for(index))
  if emu.getregister("M68K PC")~=current_expectation.returnPc then error("direct function return PC drift") end
  if case.operation=="copy" and (not copy_load_seen or not copy_save_seen) then error("CopySave nested callback sequence drift") end
  local observed_slots=case.expected.observedSlots;local slot_facts={}
  for _,slot in ipairs(observed_slots) do slot_facts[#slot_facts+1]=slot_fact(case,slot) end
  records[#records+1]={id=case.id,resultD0=signed_word(emu.getregister("M68K D0")),resultD1=signed_word(emu.getregister("M68K D1")),saveFlags=read_sram(storage.saveFlagsAddress),slotFacts=slot_facts,combatantFacts=combatant_fact(case),fullSramFact=full_sram_fact(case)}
  active=false;case_index=case_index+1
  if case_index>#config.cases then
    local residue_zero=restore_sram_zero();cleanup_session();if #event_ids~=0 then error("residual registered callback") end
    if not residue_zero then error("residual SRAM bytes") end
    write_output(residue_zero);status("milestone:callbacks-cleared:0");status("milestone:observer-finished");client.exitCode(0)
  end
end
local function register_exec(address,role,index)
  if not callbacks[address] then
    callbacks[address]={}
    event_ids[#event_ids+1]=event.on_bus_exec(function()
      if observer_failed then return end
      local ok,message=pcall(function()
        current_pc=address
        for _,entry in ipairs(callbacks[address]) do
          if entry.role=="case-entry" then start_case(entry.index)
          elseif entry.role=="function-entry" then function_entry(address)
          elseif entry.role=="case-result" then finish_case(entry.index)
          else error("unknown deterministic dispatch role: "..entry.role) end
        end
      end)
      if not ok then fail_callback(message) end
    end,address,"sram-lifecycle-"..address,"M68K BUS")
  end
  -- A duplicate physical-PC callback role is intentionally coalesced into this one dispatch list.
  for _,entry in ipairs(callbacks[address]) do if entry.role==role then return end end
  callbacks[address][#callbacks[address]+1]={role=role,index=index}
end
write_probe=function()
  memory.write_u16_be(probe_base,0x46FC,"M68K BUS")
  memory.write_u16_be(probe_base+2,0x2700,"M68K BUS")
  memory.write_u16_be(probe_base+4,0x2E7C,"M68K BUS")
  memory.write_u32_be(probe_base+6,stack_top,"M68K BUS")
  memory.write_u16_be(probe_base+10,0x4EF9,"M68K BUS")
  memory.write_u32_be(probe_base+12,case_base,"M68K BUS")
  for index,case in ipairs(config.cases) do
    local entry=pc_for(index);memory.write_u16_be(entry,0x4E71,"M68K BUS");memory.write_u16_be(entry+2,0x7000|(case.selector&0xFF),"M68K BUS");memory.write_u16_be(entry+4,0x7200,"M68K BUS");memory.write_u16_be(entry+6,0x4EB9,"M68K BUS");memory.write_u32_be(entry+8,function_for(case),"M68K BUS");memory.write_u16_be(entry+12,0x4E71,"M68K BUS")
    memory.write_u16_be(entry+14,0x4EF9,"M68K BUS");memory.write_u32_be(entry+16,index==#config.cases and pc_for(index)+probe_stride or pc_for(index+1),"M68K BUS")
    register_exec(entry,"case-entry",index);register_exec(entry+12,"case-result",index);register_exec(function_for(case),"function-entry",index)
  end
end
status("milestone:observer-loaded")
local ok,message=pcall(function()
  register_exec(f.CheckSram,"function-entry",0);status("milestone:direct-function-probe-armed")
end)
if not ok then fail_callback(message) end
local frames=0
while true do
  frames=frames+1;joypad.set({Start=true},1);joypad.set({},2);emu.frameadvance()
  if frames%600==0 then status("frame="..frames..",pc="..string.format("%X",emu.getregister("M68K PC"))) end
end
