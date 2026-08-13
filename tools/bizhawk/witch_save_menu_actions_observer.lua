-- One grouped, controlled-seam observation of Witch Load/Copy/Delete action admission.
-- Original action and SRAM-service entries execute; text, menu/prompt, and loop handoffs are
-- bounded session-only seams.  Callback failures are always status-bearing and nonzero.
local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local s=config.static
local f,c,r,storage,h=s["function"],s.calls,s.ram,s.storage,s.harness
local callbacks,event_ids,event_ids_by_address,records,observed_action_entries={}, {}, {}, {}, {}
local active,bootstrapped,observer_failed,session_cleaned=false,false,false,false
local action_cases_entered,bootstrap_to_first_case_frames=false,0
local case_index,current_phase,current_role,current_pc,current_expectation=1,"registration","registration",nil,nil
local saved={generated=nil,slot1=nil,slot2=nil,saveFlags=nil,slot1Checksum=nil,slot2Checksum=nil,currentSaveSlot=nil,gameFlag88=nil,a6=nil,a7=nil,cart=nil}
local restoration={currentSaveSlotRestored=false,gameFlag88Restored=false,saveFlagsRestored=false,slotDataRestored=false,generatedBytesRestored=false,stackRestored=false,frameRestored=false,cartPatchesRestored=false}

local function frame_budget(value,name)
  assert(type(value)=="number" and value%1==0 and value>0 and value<=3600,"invalid "..name.." frame budget")
  return value
end
local bootstrap_to_first_case_budget=frame_budget(h.bootstrapToFirstCaseFrameBudget,"bootstrap-to-first-case")
local case_frame_budget=frame_budget(h.caseFrameBudget,"case")

local function status(value) local file=assert(io.open(config.statusPath,"a"));file:write(value.."\n");file:close() end
local function bool(value) return value and "true" or "false" end
local function nullable(value) return value==nil and "null" or tostring(value) end
local function quote(value) return string.format("%q",value) end
local function read8(address,domain) return memory.read_u8(address,domain or "M68K BUS") end
local function write8(address,value,domain) memory.write_u8(address,value&0xFF,domain or "M68K BUS") end
local function read16(address,domain) return memory.read_u16_be(address,domain or "M68K BUS") end
local function write16(address,value,domain) memory.write_u16_be(address,value&0xFFFF,domain or "M68K BUS") end
local function read32(address,domain) return memory.read_u32_be(address,domain or "M68K BUS") end
local function write32(address,value,domain) memory.write_u32_be(address,value&0xFFFFFFFF,domain or "M68K BUS") end
local function pc() return emu.getregister("M68K PC")&0xFFFFFF end
local function case_at() return config.cases[case_index] end
local function case_pc(index) return h.baseAddress+(index-1)*h.caseStride end
local function result_pc(index) return case_pc(index)+h.caseResultOffset end
local function selected_slot(selector) return selector==0 and "slot1" or "slot2" end
local function action_address(action) return f[action.."ActionAddress"] end
local function service_entry(action) if action=="load" then return f.loadGameAddress elseif action=="copy" then return f.copySaveAddress else return f.clearSaveSlotFlagAddress end end
local function service_name(action) if action=="load" then return "LoadGame" elseif action=="copy" then return "CopySave" else return "ClearSaveSlotFlag" end end
local function sram_offset(address)
  local offset=address-storage.physicalWindowBaseAddress
  assert(offset>=0 and offset<memory.getmemorydomainsize("SRAM"),"SRAM address outside emulator domain")
  return offset
end
local function read_sram(address) return read8(sram_offset(address),"SRAM") end
local function write_sram(address,value) write8(sram_offset(address),value,"SRAM") end
local function roles_json(address)
  local roles={};for _,entry in ipairs(callbacks[address] or {}) do roles[#roles+1]=quote(entry.role) end
  return "["..table.concat(roles,",").."]"
end
local function events_json(events)
  local values={};for _,event in ipairs(events or {}) do values[#values+1]='{"role":'..quote(event.role)..',"pc":'..event.pc..'}' end
  return "["..table.concat(values,",").."]"
end
local function restore_json()
  return '{"currentSaveSlotRestored":'..bool(restoration.currentSaveSlotRestored)..',"gameFlag88Restored":'..bool(restoration.gameFlag88Restored)..',"saveFlagsRestored":'..bool(restoration.saveFlagsRestored)..',"slotDataRestored":'..bool(restoration.slotDataRestored)..',"generatedBytesRestored":'..bool(restoration.generatedBytesRestored)..',"stackRestored":'..bool(restoration.stackRestored)..',"frameRestored":'..bool(restoration.frameRestored)..',"cartPatchesRestored":'..bool(restoration.cartPatchesRestored)..'}'
end
local function pending_json()
  local case=case_at()
  local pending=active and active.pendingKind or "none"
  return '{"active":'..bool(active~=nil)..',"caseIndex":'..(active and case_index or 0)..',"caseId":'..(case and quote(case.id) or "null")..',"expectedEventPc":'..nullable(current_expectation and current_expectation.eventPc)..',"expectedCallPc":'..nullable(current_expectation and current_expectation.callPc)..',"expectedTargetPc":'..nullable(current_expectation and current_expectation.targetPc)..',"expectedReturnPc":'..nullable(current_expectation and current_expectation.returnPc)..',"pendingKind":'..quote(pending)..',"serviceEntrySeen":'..bool(active and active.serviceEntrySeen or false)..',"serviceReturnSeen":'..bool(active and active.serviceReturnSeen or false)..',"rolesAtPc":'..roles_json(current_pc)..'}'
end
local function unregister_events()
  for i=#event_ids,1,-1 do event.unregisterbyid(event_ids[i]);event_ids[i]=nil end
  event_ids_by_address={}
end
local function cleanup_events() if session_cleaned then return end;session_cleaned=true;unregister_events() end
local function unregister_exec(address)
  local event_id=assert(event_ids_by_address[address],"missing registered callback at "..address)
  event.unregisterbyid(event_id);event_ids_by_address[address]=nil;callbacks[address]=nil
  for index=#event_ids,1,-1 do if event_ids[index]==event_id then table.remove(event_ids,index);break end end
end
local function read_bytes(address,count,domain)
  local values={};for offset=0,count-1 do values[#values+1]=read8(address+offset,domain) end;return values
end
local function write_bytes(address,values,domain) for index,value in ipairs(values) do write8(address+index-1,value,domain) end end
local function arrays_equal(left,right)
  if left==nil or right==nil or #left~=#right then return false end
  for index,value in ipairs(left) do if right[index]~=value then return false end end
  return true
end
local function set_expectation(phase,role,event_pc,call_pc,target_pc,return_pc)
  current_phase,current_role,current_expectation=phase,role,{eventPc=event_pc,callPc=call_pc,targetPc=target_pc,returnPc=return_pc}
end
local function capture_state()
  saved.generated=read_bytes(h.generatedBegin,h.generatedEnd-h.generatedBegin,"M68K BUS")
  saved.slot1={};saved.slot2={}
  for index=0,storage.logicalPayloadByteCount-1 do
    saved.slot1[index+1]=read_sram(storage.slot1DataAddress+index*storage.physicalAddressStep)
    saved.slot2[index+1]=read_sram(storage.slot2DataAddress+index*storage.physicalAddressStep)
  end
  saved.saveFlags=read_sram(storage.saveFlagsAddress)
  saved.slot1Checksum=read_sram(storage.slot1ChecksumAddress)
  saved.slot2Checksum=read_sram(storage.slot2ChecksumAddress)
  saved.currentSaveSlot=read16(r.currentSaveSlotAddress)
  saved.gameFlag88=read8(r.gameFlagsAddress+r.flag88ByteOffset)
  saved.a6=emu.getregister("M68K A6")&0xFFFFFF;saved.a7=emu.getregister("M68K A7")&0xFFFFFF
  saved.cart={};for index,patch in ipairs(s.sessionPatches) do saved.cart[index]=patch.originalHex end
end
local function hex_bytes(value)
  assert(type(value)=="string" and #value%2==0 and value:match("^[0-9A-F]+$")~=nil,"patch hex drift")
  local result={};for index=1,#value,2 do result[#result+1]=tonumber(value:sub(index,index+1),16) end;return result
end
local function apply_patches()
  for _,patch in ipairs(s.sessionPatches) do
    local original=hex_bytes(patch.originalHex);local patched=hex_bytes(patch.patchedHex)
    assert(arrays_equal(read_bytes(patch.address,patch.widthBytes,"MD CART"),original),"session patch original readback drift: "..patch.role)
    write_bytes(patch.address,patched,"MD CART")
    assert(arrays_equal(read_bytes(patch.address,patch.widthBytes,"MD CART"),patched),"session patch write readback drift: "..patch.role)
  end
end
local function restore_state()
  if saved.generated==nil then return end
  write16(r.currentSaveSlotAddress,saved.currentSaveSlot);restoration.currentSaveSlotRestored=read16(r.currentSaveSlotAddress)==saved.currentSaveSlot
  write8(r.gameFlagsAddress+r.flag88ByteOffset,saved.gameFlag88);restoration.gameFlag88Restored=read8(r.gameFlagsAddress+r.flag88ByteOffset)==saved.gameFlag88
  write_sram(storage.saveFlagsAddress,saved.saveFlags);restoration.saveFlagsRestored=read_sram(storage.saveFlagsAddress)==saved.saveFlags
  write_sram(storage.slot1ChecksumAddress,saved.slot1Checksum)
  write_sram(storage.slot2ChecksumAddress,saved.slot2Checksum)
  restoration.slotDataRestored=read_sram(storage.slot1ChecksumAddress)==saved.slot1Checksum and read_sram(storage.slot2ChecksumAddress)==saved.slot2Checksum
  for index=0,storage.logicalPayloadByteCount-1 do
    write_sram(storage.slot1DataAddress+index*storage.physicalAddressStep,saved.slot1[index+1])
    write_sram(storage.slot2DataAddress+index*storage.physicalAddressStep,saved.slot2[index+1])
  end
  for index=0,storage.logicalPayloadByteCount-1 do
    if read_sram(storage.slot1DataAddress+index*storage.physicalAddressStep)~=saved.slot1[index+1] or read_sram(storage.slot2DataAddress+index*storage.physicalAddressStep)~=saved.slot2[index+1] then restoration.slotDataRestored=false;break end
  end
  write_bytes(h.generatedBegin,saved.generated,"M68K BUS");restoration.generatedBytesRestored=arrays_equal(read_bytes(h.generatedBegin,#saved.generated,"M68K BUS"),saved.generated)
  emu.setregister("M68K A7",saved.a7);emu.setregister("M68K A6",saved.a6)
  restoration.stackRestored=(emu.getregister("M68K A7")&0xFFFFFF)==saved.a7
  restoration.frameRestored=(emu.getregister("M68K A6")&0xFFFFFF)==saved.a6
  for index,patch in ipairs(s.sessionPatches) do write_bytes(patch.address,hex_bytes(saved.cart[index]),"MD CART") end
  restoration.cartPatchesRestored=true
  for index,patch in ipairs(s.sessionPatches) do if not arrays_equal(read_bytes(patch.address,patch.widthBytes,"MD CART"),hex_bytes(saved.cart[index])) then restoration.cartPatchesRestored=false;break end end
end
local function fail_callback(message)
  if observer_failed then return end
  observer_failed=true
  local ok,restore_message=pcall(restore_state);if not ok then status("milestone:restore-error:"..tostring(restore_message)) end
  os.remove(config.outputPath);cleanup_events()
  local handle=io.open(config.outputPath,"r");local output_removed=handle==nil;if handle then handle:close() end
  local expected=current_expectation or {}
  local payload='{"owner":'..quote(config.observerFailureContract.owner)..',"caseId":'..(case_at() and quote(case_at().id) or "null")..',"phase":'..quote(current_phase)..',"role":'..quote(current_role)..',"actualPc":'..nullable(current_role=="registration" and nil or pc())..',"expectedEventPc":'..nullable(expected.eventPc)..',"expectedCallPc":'..nullable(expected.callPc)..',"expectedTargetPc":'..nullable(expected.targetPc)..',"expectedReturnPc":'..nullable(expected.returnPc)..',"pendingCallback":'..pending_json()..',"callbacksRemaining":0,"restoration":'..restore_json()..',"cleanup":{"outputRemoved":'..bool(output_removed)..',"callbacksCleared":'..bool(#event_ids==0)..'},"error":'..quote(tostring(message))..'}'
  local diagnostic=config.observerFailureContract.statusPrefix..payload;status(diagnostic);print(diagnostic);client.exitCode(config.observerFailureContract.exitCode)
end
local function expect(value,message) if not value then error(message) end end
local function append(role,address) active.events[#active.events+1]={role=role,pc=address} end
local function write_controlled_stub(result)
  write16(h.controlledStubAddress,0x303C);write16(h.controlledStubAddress+2,result&0xFFFF);write16(h.controlledStubAddress+4,0x4E75)
end
local function write_terminal(result_address)
  write16(h.terminalStubAddress,0x2E79);write32(h.terminalStubAddress+2,h.savedA7Address);write16(h.terminalStubAddress+6,0x4EF9);write32(h.terminalStubAddress+8,result_address)
end
local function set_flag88(value)
  local address=r.gameFlagsAddress+r.flag88ByteOffset;local current=read8(address);local mask=r.flag88Mask
  write8(address,value and (current|mask) or (current&((~mask)&0xFF)))
end
local function blank_slots()
  for offset=0,storage.logicalPayloadByteCount-1 do write_sram(storage.slot1DataAddress+offset*storage.physicalAddressStep,0);write_sram(storage.slot2DataAddress+offset*storage.physicalAddressStep,0) end
end
local function menu_values(case)
  local availability=(case.saveFlags&3)<<1
  return (availability&2)~=0 and 1 or 2,availability
end
local function admitted(case)
  return (case.action=="load" and case.menuResult~=-1) or (case.action=="copy" and case.promptResult==0) or (case.action=="delete" and case.menuResult~=-1 and case.promptResult==0)
end
local function prepare_case(case)
  blank_slots();write_sram(storage.saveFlagsAddress,case.saveFlags);write16(r.currentSaveSlotAddress,0xFFFF);set_flag88(case.flag88Set)
  local result=case.action=="copy" and case.promptResult or case.menuResult;write_controlled_stub(result);write_terminal(result_pc(case_index))
  active={case=case,events={},pendingKind="none",serviceEntrySeen=false,serviceReturnSeen=false,menu=nil,prompt=nil,currentSaveSlot=nil,service=nil,handoff=nil,frameBudget=case_frame_budget,frameCount=0}
end
local function write_program()
  write32(h.savedA7Address,saved.a7)
  for index,case in ipairs(config.cases) do
    local entry=case_pc(index);write16(entry,0x2E7C);write32(entry+2,h.stackTop);write16(entry+6,0x4EB9);write32(entry+8,action_address(case.action));write16(entry+12,0x4E71);write16(entry+14,0x4EF9);write32(entry+16,index==#config.cases and entry+14 or case_pc(index+1))
    register_exec(entry,"case-entry",index);register_exec(entry+12,"case-result",index)
  end
  register_exec(h.controlledStubAddress,"controlled-seam",0);register_exec(h.terminalStubAddress,"terminal",0)
end
function start_case(index)
  expect(not active and index==case_index,"case-entry dispatch drift")
  local case=assert(case_at(),"case table exhausted");set_expectation("case-entry","case-entry",case_pc(index),nil,action_address(case.action),nil);expect(pc()==current_expectation.eventPc,"case-entry PC drift")
  if not action_cases_entered then status("milestone:action-cases-entered");action_cases_entered=true end
  prepare_case(case);append("case-entry",pc())
end
local function action_entry(address)
  local case=assert(active and active.case,"action entry without active case");set_expectation("action-entry","action-entry",address,nil,address,nil);expect(address==action_address(case.action) and pc()==address,"action-entry target drift");observed_action_entries[case.action]=address;append("action-entry",address)
end
local function menu_call(address)
  local case=assert(active and active.case,"menu call without active case");local call=c.menu[case.action];local selector,availability=menu_values(case)
  set_expectation("menu-call","menu-call",address,address,f.menuInstructionTargetAddress,call.returnAddress);expect(address==call.callSiteAddress and pc()==address,"menu call PC drift");expect((emu.getregister("M68K D0")&0xFFFF)==selector and (emu.getregister("M68K D1")&0xFFFF)==2 and (emu.getregister("M68K D2")&0xFFFF)==availability,"menu ABI input drift")
  active.menu={callSiteAddress=address,returnAddress=call.returnAddress,page=2,initialSelector=selector,availability=availability,controlledReturn=case.menuResult};active.pendingKind="menu";append("menu-call",address)
end
local function prompt_call(address)
  local case=assert(active and active.case,"prompt call without active case");local call=c.prompt[case.action]
  set_expectation("prompt-call","prompt-call",address,address,f.promptInstructionTargetAddress,call.returnAddress);expect(address==call.callSiteAddress and pc()==address,"prompt call PC drift")
  if case.action=="delete" then active.currentSaveSlot=read16(r.currentSaveSlotAddress);expect(active.currentSaveSlot==case.menuResult-1,"delete current-slot write drift") end
  write_controlled_stub(case.promptResult)
  active.prompt={callSiteAddress=address,returnAddress=call.returnAddress,controlledReturn=case.promptResult};active.pendingKind="prompt";append("prompt-call",address)
end
local function controlled_seam(address)
  local case=assert(active and active.case,"controlled seam without active case");expect(address==h.controlledStubAddress and pc()==address,"controlled seam PC drift");expect(active.pendingKind=="menu" or active.pendingKind=="prompt","controlled seam kind drift")
  local result=active.pendingKind=="menu" and case.menuResult or case.promptResult;set_expectation(active.pendingKind.."-seam","controlled-seam",address,current_expectation and current_expectation.callPc,address,current_expectation and current_expectation.returnPc);expect(read16(address)==0x303C and read16(address+2)==(result&0xFFFF) and read16(address+4)==0x4E75,"controlled seam generated instruction drift");append("controlled-seam",address)
end
local function menu_return(address)
  local case=assert(active and active.case,"menu return without active case");local call=c.menu[case.action];set_expectation("menu-return","menu-return",address,call.callSiteAddress,f.menuInstructionTargetAddress,address);expect(active.pendingKind=="menu" and address==call.returnAddress and pc()==address and signed_word(emu.getregister("M68K D0"))==case.menuResult,"menu result return drift");active.pendingKind="none";append("menu-return",address)
end
local function prompt_return(address)
  local case=assert(active and active.case,"prompt return without active case");local call=c.prompt[case.action];set_expectation("prompt-return","prompt-return",address,call.callSiteAddress,f.promptInstructionTargetAddress,address);expect(active.pendingKind=="prompt" and address==call.returnAddress and pc()==address and signed_word(emu.getregister("M68K D0"))==case.promptResult,"prompt result return drift");active.pendingKind="none";append("prompt-return",address)
end
function signed_word(value) value=value&0xFFFF;return value>=0x8000 and value-0x10000 or value end
local function service_call(address)
  local case=assert(active and active.case,"service call without active case");local call=c.service[case.action];local selector=case.action=="copy" and ((case.saveFlags&3)-1) or read16(r.currentSaveSlotAddress)
  set_expectation("service-call","service-call",address,address,service_entry(case.action),call.returnAddress);expect(admitted(case) and address==call.callSiteAddress and pc()==address and selector>=0 and selector<=1,"service admission/call drift");if case.action=="load" then active.currentSaveSlot=selector end
  active.service={name=service_name(case.action),callSiteAddress=address,returnAddress=call.returnAddress,entryAddress=service_entry(case.action),selector=selector};active.pendingKind="service";append("service-call",address)
end
local function service_entry_callback(address)
  if not active then return end
  local case=assert(active.case,"service entry without active case");local call=c.service[case.action];local target=service_entry(case.action)
  set_expectation("service-entry","service-entry",target,call.callSiteAddress,target,call.returnAddress)
  expect(active.pendingKind=="service" and active.service~=nil and address==target and pc()==address and active.service.callSiteAddress==call.callSiteAddress and active.service.entryAddress==target and active.service.returnAddress==call.returnAddress,"unexpected original service entry")
  if case.action=="copy" then
    expect(f.copySaveNestedLoadCallAddress~=nil and f.copySaveNestedLoadCallAddress>0,"CopySave nested LoadGame source guard drift")
    unregister_exec(f.loadGameAddress);active.copyNestedLoadEntrySuppressed=true
  end
  active.serviceEntrySeen=true;append("service-entry",address)
end
local function service_return(address)
  local case=assert(active and active.case,"service return without active case");local call=c.service[case.action];set_expectation("service-return","service-return",address,call.callSiteAddress,service_entry(case.action),address);expect(active.pendingKind=="service" and active.serviceEntrySeen and address==call.returnAddress and pc()==address,"service return drift");active.serviceReturnSeen=true;active.pendingKind="none";append("service-return",address)
  if case.action=="copy" then
    expect(active.copyNestedLoadEntrySuppressed and callbacks[f.loadGameAddress]==nil,"CopySave nested LoadGame callback state drift")
    register_exec(f.loadGameAddress,"service-entry",0);active.copyNestedLoadEntrySuppressed=false
  end
end
local function load_handoff(address)
  local case=assert(active and active.case,"load handoff without active case");local kind=case.flag88Set and "battle" or "savepoint";local call=c.handoff[kind]
  set_expectation("load-handoff","load-handoff",address,address,kind=="battle" and f.battleInstructionTargetAddress or f.savepointInstructionTargetAddress,call.returnAddress);expect(case.action=="load" and active.serviceReturnSeen and address==call.callSiteAddress and pc()==address,"load handoff drift")
  active.handoff={kind=kind,callSiteAddress=address,returnAddress=call.returnAddress,instructionTargetAddress=kind=="battle" and f.battleInstructionTargetAddress or f.savepointInstructionTargetAddress,effectiveTargetAddress=kind=="battle" and f.battleEffectiveTargetAddress or f.savepointEffectiveTargetAddress};active.pendingKind="handoff";append("load-handoff",address)
end
local function load_flag_control(address)
  local case=assert(active and active.case,"load flag control without active case")
  set_expectation("load-flag-control","load-flag-control",address,nil,address,nil)
  expect(case.action=="load" and active.serviceReturnSeen and address==f.loadFlagTrapAddress and pc()==address,"load flag control drift")
  set_flag88(case.flag88Set);append("load-flag-control",address)
end
local function menu_loop_terminal(address)
  local case=assert(active and active.case,"menu loop terminal without active case");set_expectation("menu-loop-terminal","menu-loop-terminal",address,nil,nil,nil);expect(address==f.menuLoopAddress and pc()==address,"menu-loop terminal PC drift")
  local is_load_confirm=case.action=="load" and case.menuResult~=-1;expect(not is_load_confirm,"load confirm reached menu loop instead of safe handoff");if admitted(case) then expect(active.serviceEntrySeen and active.serviceReturnSeen,"confirmed case missing service entry/return") else expect(not active.serviceEntrySeen and not active.serviceReturnSeen,"cancel case reached original service") end
  active.pendingKind="terminal";append("menu-loop-terminal",address)
end
local function terminal(address)
  local case=assert(active and active.case,"terminal without active case");set_expectation("terminal","terminal",address,nil,address,result_pc(case_index));expect(address==h.terminalStubAddress and pc()==address,"terminal PC drift")
  if case.action=="load" and case.menuResult~=-1 then expect(active.pendingKind=="handoff","load terminal lacks handoff") else expect(active.pendingKind=="terminal","menu terminal state drift") end
  active.pendingKind="terminal";append("terminal",address)
end
local function record_text(record)
  local function menu_text(value) return value and '{"callSiteAddress":'..value.callSiteAddress..',"returnAddress":'..value.returnAddress..',"page":'..value.page..',"initialSelector":'..value.initialSelector..',"availability":'..value.availability..',"controlledReturn":'..value.controlledReturn..'}' or "null" end
  local function prompt_text(value) return value and '{"callSiteAddress":'..value.callSiteAddress..',"returnAddress":'..value.returnAddress..',"controlledReturn":'..value.controlledReturn..'}' or "null" end
  local function service_text(value) return value and '{"name":'..quote(value.name)..',"callSiteAddress":'..value.callSiteAddress..',"returnAddress":'..value.returnAddress..',"entryAddress":'..value.entryAddress..',"selector":'..value.selector..'}' or "null" end
  local function handoff_text(value) return value and '{"kind":'..quote(value.kind)..',"callSiteAddress":'..value.callSiteAddress..',"returnAddress":'..value.returnAddress..',"instructionTargetAddress":'..value.instructionTargetAddress..',"effectiveTargetAddress":'..value.effectiveTargetAddress..'}' or "null" end
  return '{"id":'..quote(record.case.id)..',"action":'..quote(record.case.action)..',"actionEntryAddress":'..action_address(record.case.action)..',"menu":'..menu_text(record.menu)..',"prompt":'..prompt_text(record.prompt)..',"currentSaveSlot":'..nullable(record.currentSaveSlot)..',"service":'..service_text(record.service)..',"handoff":'..handoff_text(record.handoff)..',"callbackChronology":'..events_json(record.events)..'}'
end
local function finish_case(index)
  local case=assert(active and active.case,"case result without active case");set_expectation("case-result","case-result",result_pc(index),nil,nil,nil);expect(index==case_index and pc()==result_pc(index),"case result PC drift");expect((emu.getregister("M68K A7")&0xFFFFFF)==saved.a7 and (emu.getregister("M68K A6")&0xFFFFFF)==saved.a6,"generated stack/frame restoration drift")
  records[#records+1]=active;active=nil;case_index=case_index+1
  emu.setregister("M68K A7",saved.a7);emu.setregister("M68K A6",saved.a6)
  if case_index<=#config.cases then return end
  restore_state();cleanup_events();expect(#event_ids==0,"residual registered callback");for _,value in pairs(restoration) do expect(value,"scoped restoration drift") end
  local output=assert(io.open(config.outputPath,"w"));local rows={};for _,record in ipairs(records) do rows[#rows+1]=record_text(record) end;local order={};for _,id in ipairs(config.caseOrder) do order[#order+1]=quote(id) end
  output:write('{"system":"'..emu.getsystemid()..'","core":'..quote(config.core)..',"id":'..quote(config.id)..',"caseOrder":['..table.concat(order,",")..'],"observedActionEntries":{"load":'..assert(observed_action_entries.load,"missing Load action callback")..',"copy":'..assert(observed_action_entries.copy,"missing Copy action callback")..',"delete":'..assert(observed_action_entries.delete,"missing Delete action callback")..'},"records":['..table.concat(rows,",")..'],"restoration":'..restore_json()..',"callbacksCleared":0}');output:close();status("milestone:callbacks-cleared:0");status("milestone:observer-finished");client.exitCode(0)
end
local function bootstrap_check(address)
  if bootstrapped then return end
  set_expectation("bootstrap-check-sram","bootstrap-return-redirect",address,nil,address,h.baseAddress);expect(address==f.checkSramAddress and pc()==address,"CheckSram bootstrap PC drift");capture_state();write32(emu.getregister("M68K A7")&0xFFFFFF,h.baseAddress);apply_patches();write_program();bootstrapped=true;status("milestone:action-probe-armed")
end
local function dispatch(address,entry)
  if entry.role=="bootstrap-check-sram" then bootstrap_check(address)
  elseif entry.role=="case-entry" then start_case(entry.index)
  elseif entry.role=="case-result" then
    if active and active.pendingKind=="terminal" then finish_case(entry.index) else error("witch action returned instead of bounded terminal") end
  elseif entry.role=="action-entry" then action_entry(address)
  elseif entry.role=="menu-call" then menu_call(address)
  elseif entry.role=="prompt-call" then prompt_call(address)
  elseif entry.role=="controlled-seam" then controlled_seam(address)
  elseif entry.role=="menu-return" then menu_return(address)
  elseif entry.role=="prompt-return" then prompt_return(address)
  elseif entry.role=="service-call" then service_call(address)
  elseif entry.role=="service-entry" then service_entry_callback(address)
  elseif entry.role=="service-return" then service_return(address)
  elseif entry.role=="load-flag-control" then load_flag_control(address)
  elseif entry.role=="load-handoff" then load_handoff(address)
  elseif entry.role=="menu-loop-terminal" then menu_loop_terminal(address)
  elseif entry.role=="terminal" then terminal(address)
  else error("unknown deterministic dispatch role: "..entry.role) end
end
function register_exec(address,role,index)
  if not callbacks[address] then
    callbacks[address]={};local event_id=event.on_bus_exec(function()
      if observer_failed then return end
      local ok,message=pcall(function() current_pc=address;for _,entry in ipairs(callbacks[address]) do dispatch(address,entry) end end)
      if not ok then fail_callback(message) end
    end,address,"witch-save-menu-actions-"..address,"M68K BUS");event_ids[#event_ids+1]=event_id;event_ids_by_address[address]=event_id
  end
  for _,entry in ipairs(callbacks[address]) do if entry.role==role and entry.index==index then return end end
  callbacks[address][#callbacks[address]+1]={role=role,index=index}
end
status("milestone:observer-loaded")
local ok,message=pcall(function()
  register_exec(f.checkSramAddress,"bootstrap-check-sram",0)
  for action,address in pairs({load=f.loadActionAddress,copy=f.copyActionAddress,delete=f.deleteActionAddress}) do register_exec(address,"action-entry",0) end
  for action,call in pairs(c.menu) do register_exec(call.callSiteAddress,"menu-call",0);register_exec(call.returnAddress,"menu-return",0) end
  for action,call in pairs(c.prompt) do register_exec(call.callSiteAddress,"prompt-call",0);register_exec(call.returnAddress,"prompt-return",0) end
  for action,call in pairs(c.service) do register_exec(call.callSiteAddress,"service-call",0);register_exec(call.returnAddress,"service-return",0) end
  for _,address in ipairs({f.loadGameAddress,f.copySaveAddress,f.clearSaveSlotFlagAddress}) do register_exec(address,"service-entry",0) end
  register_exec(f.loadFlagTrapAddress,"load-flag-control",0)
  for _,call in pairs(c.handoff) do register_exec(call.callSiteAddress,"load-handoff",0) end
  register_exec(f.menuLoopAddress,"menu-loop-terminal",0)
end)
if not ok then fail_callback(message) end
local function enforce_watchdogs()
  if observer_failed then return end
  current_pc=pc()
  if not action_cases_entered then
    bootstrap_to_first_case_frames=bootstrap_to_first_case_frames+1
    if bootstrap_to_first_case_frames>bootstrap_to_first_case_budget then
      local target=bootstrapped and case_pc(1) or f.checkSramAddress
      set_expectation("bootstrap-to-first-case-watchdog","bootstrap-to-first-case-watchdog-timeout",case_pc(1),nil,target,nil)
      fail_callback("bootstrap-to-first-case frame budget exhausted before first generated case entry")
    end
  elseif active then
    active.frameCount=active.frameCount+1
    if active.frameCount>active.frameBudget then
      set_expectation("case-watchdog","case-watchdog-timeout",result_pc(case_index),nil,h.terminalStubAddress,result_pc(case_index))
      fail_callback("action case frame budget exhausted before terminal")
    end
  end
end
local frames=0
while true do
  frames=frames+1;joypad.set({Start=true},1);joypad.set({},2);emu.frameadvance();enforce_watchdogs()
  if frames%600==0 then status("frame="..frames..",pc="..string.format("%X",pc())) end
end
