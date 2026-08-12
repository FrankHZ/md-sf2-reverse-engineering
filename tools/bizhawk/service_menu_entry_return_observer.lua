-- One grouped source-shaped probe for the four built service-entry seams.
-- The static denominator is 69 direct alias transfers: 62 returning calls and
-- seven tail transfers.  This observer runs one representative for each
-- positive caller-family × service × transfer-kind cell, plus the distinct
-- BattleTest Church MOVEM returning-caller frame.  It never executes a menu
-- UI, text, window, input, transaction, or persistence body.
local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local s,h,source=config.static,config.static.harness,config.sourceContext
local probe_base,frame_base,stack_top=0xFF6700,h.callerFrameAddress,config.static.stackTop
local callbacks,event_ids,records={}, {}, {}
local observer_failed,bootstrapped,session_cleaned=false,false,false
local case_index,mode,frames,case_frames=0,"none",0,0
local original_portrait,original_frame=nil,nil
local current_phase,current_role,current_pc,current_expectation="registration","registration",nil,nil
local current_stack_expectation={expectedA7=nil,actualA7=nil,expectedTopLongword=nil,actualTopLongword=nil}
local write_probe,expect

local function status(value) local file=assert(io.open(config.statusPath,"a"));file:write(value.."\n");file:close() end
local function bool(value) return value and "true" or "false" end
local function nullable(value) return value==nil and "null" or tostring(value) end
local function json_string(value) return string.format("%q",value) end
local function read_u8(address) return memory.read_u8(address,"M68K BUS") end
local function write_u8(address,value) memory.write_u8(address,value,"M68K BUS") end
local function read_u16(address) return memory.read_u16_be(address,"M68K BUS") end
local function write_u16(address,value) memory.write_u16_be(address,value,"M68K BUS") end
local function read_u32(address) return memory.read_u32_be(address,"M68K BUS") end
local function write_u32(address,value) memory.write_u32_be(address,value,"M68K BUS") end
local function read_bytes(address,count)
  local values={};for offset=0,count-1 do values[#values+1]=read_u8(address+offset) end;return values
end
local function write_bytes(address,values)
  for offset,value in ipairs(values) do write_u8(address+offset-1,value) end
end
local function equal_arrays(left,right)
  if #left~=#right then return false end
  for index,value in ipairs(left) do if right[index]~=value then return false end end
  return true
end
local function hex_bytes(value)
  expect(type(value)=="string" and #value%2==0 and value:match("^[0-9A-F]+$")~=nil,"hexadecimal contract drift")
  local values={};for index=1,#value,2 do values[#values+1]=tonumber(value:sub(index,index+1),16) end;return values
end
local function current_case() return config.cases[case_index] end
local function case_pc(index) return h.baseAddress+(index-1)*h.strideBytes end
local function result_pc(index) return case_pc(index)+h.resultOffsetBytes end
local function roles_json(address)
  local roles={};for _,entry in ipairs(callbacks[address] or {}) do roles[#roles+1]=json_string(entry.role) end;return "["..table.concat(roles,",").."]"
end
local function chronology_json(events)
  local rows={};for _,item in ipairs(events) do rows[#rows+1]="{\"role\":"..json_string(item.role)..",\"pc\":"..item.pc.."}" end;return "["..table.concat(rows,",").."]"
end
local function expected_case_chronology(case)
  if case==nil then return {} end
  local inner_role=case.service=="blacksmith" and "generated-blacksmith-return-stub" or "generated-service-cancel-stub"
  local inner_address=nil;for _,stub in ipairs(s.generatedStubs) do if stub.role==inner_role then inner_address=stub.address end end
  expect(inner_address~=nil,"generated inner-stub chronology contract drift")
  return {
    {role="case-entry",pc=case_pc(case_index)},
    {role=case.transferKind=="tail-transfer" and "tail-transfer-site" or "caller-call-site",pc=case.callSiteAddress},
    {role="service-entry",pc=s.aliases[case.service].effectiveTargetAddress},
    {role=inner_role,pc=inner_address},
    {role=case.outerReturnRole,pc=s.outerReturnTrampoline.address},
    {role="caller-result",pc=result_pc(case_index)}
  }
end
local function register_json()
  local d0=emu.getregister("M68K D0")&0xFFFF;local d1=emu.getregister("M68K D1")&0xFFFF;local d2=emu.getregister("M68K D2")&0xFFFF
  local a6=emu.getregister("M68K A6")&0xFFFFFF;local a7=emu.getregister("M68K A7")&0xFFFFFF
  return "{\"d0\":"..d0..",\"d1\":"..d1..",\"d2\":"..d2..",\"a6\":"..a6..",\"a7\":"..a7.."}"
end
local function unregister_events() for index=#event_ids,1,-1 do event.unregisterbyid(event_ids[index]);event_ids[index]=nil end end
local function cleanup_session() if session_cleaned then return end;session_cleaned=true;unregister_events() end
local function restore_all()
  if original_portrait==nil then return true end
  write_u16(s.currentPortraitAddress,original_portrait);write_u16(frame_base,original_frame)
  return read_u16(s.currentPortraitAddress)==original_portrait and read_u16(frame_base)==original_frame
end
local function pending_state_json()
  local current=current_case()
  local observed=current and current._chronology or {};local expected=expected_case_chronology(current)
  return "{\"active\":"..bool(mode=="case")..",\"caseIndex\":"..case_index..",\"expectedCaseId\":"..(current and json_string(current.caseId) or "null")..",\"rolesAtPc\":"..roles_json(current_pc)..",\"observedChronology\":"..chronology_json(observed)..",\"expectedChronology\":"..chronology_json(expected)..",\"observedChronologyCount\":"..#observed..",\"expectedChronologyCount\":"..#expected.."}"
end
local function set_expectation(phase,role,event_pc,call_pc,target_pc,return_pc)
  current_phase,current_role,current_expectation=phase,role,{eventPc=event_pc,callPc=call_pc,targetPc=target_pc,returnPc=return_pc}
  current_stack_expectation={expectedA7=nil,actualA7=nil,expectedTopLongword=nil,actualTopLongword=nil}
end
local function set_stack_expectation(expected_a7,expected_top,stack)
  current_stack_expectation={expectedA7=expected_a7,actualA7=emu.getregister("M68K A7")&0xFFFFFF,expectedTopLongword=expected_top,actualTopLongword=stack and read_u32(stack) or nil}
end
local function stack_json()
  local value=current_stack_expectation
  return "{\"expectedA7\":"..nullable(value.expectedA7)..",\"actualA7\":"..nullable(value.actualA7)..",\"expectedTopLongword\":"..nullable(value.expectedTopLongword)..",\"actualTopLongword\":"..nullable(value.actualTopLongword).."}"
end
local function fail_callback(message)
  if observer_failed then return end
  observer_failed=true
  local restored,restore_message=pcall(restore_all)
  local expected=current_expectation or {};local actual=current_role=="registration" and nil or (emu.getregister("M68K PC")&0xFFFFFF)
  local detail=tostring(message);if not restored then detail=detail.."; restoration error: "..tostring(restore_message) elseif restore_message~=true then detail=detail.."; restoration readback drift" end
  os.remove(config.outputPath);cleanup_session()
  local handle=io.open(config.outputPath,"r");local output_removed=handle==nil;if handle then handle:close() end
  local payload="{\"owner\":"..json_string(config.observerFailureContract.owner)..",\"caseId\":"..(current_case() and json_string(current_case().caseId) or "null")..",\"phase\":"..json_string(current_phase)..",\"role\":"..json_string(current_role)..",\"actualPc\":"..nullable(actual)..",\"expectedCallPc\":"..nullable(expected.callPc)..",\"expectedTargetPc\":"..nullable(expected.targetPc)..",\"expectedReturnPc\":"..nullable(expected.returnPc)..",\"stackReadback\":"..stack_json()..",\"pendingCallback\":"..pending_state_json()..",\"restoration\":{\"currentPortraitRestored\":"..bool(restored and restore_message==true)..",\"callerFrameRestored\":"..bool(restored and restore_message==true)..",\"callbacksCleared\":"..bool(#event_ids==0)..",\"outputRemoved\":"..bool(output_removed).."},\"error\":"..json_string(detail).."}"
  local diagnostic=config.observerFailureContract.statusPrefix..payload;status(diagnostic);print(diagnostic);client.exitCode(config.observerFailureContract.exitCode)
end
expect=function(condition,message) if not condition then error(message) end end

local function append(role,pc)
  local case=assert(current_case(),"callback without active case")
  case._chronology=case._chronology or {};case._chronology[#case._chronology+1]={role=role,pc=pc}
end
local function validate_session_patches()
  local ranges={};expect(#s.sessionPatches==17,"session-ROM patch count drift")
  for _,patch in ipairs(s.sessionPatches) do
    local original,patched=hex_bytes(patch.originalHex),hex_bytes(patch.patchedHex)
    expect(#original==patch.widthBytes and #patched==patch.widthBytes,"session-ROM patch width drift: "..patch.role)
    for _,prior in ipairs(ranges) do expect(patch.address+patch.widthBytes<=prior.address or prior.address+prior.width<=patch.address,"session-ROM patch overlap: "..patch.role) end
    ranges[#ranges+1]={address=patch.address,width=patch.widthBytes}
    expect(equal_arrays(read_bytes(patch.address,patch.widthBytes),patched),"session-ROM patch readback drift: "..patch.role)
  end
end
local function generated_stub(role)
  for _,stub in ipairs(s.generatedStubs) do if stub.role==role then return stub end end
  error("generated stub role is absent: "..role)
end
local function write_generated_stubs()
  expect(#s.generatedStubs==2,"generated-stub count drift")
  local service=generated_stub("generated-service-cancel-stub")
  local blacksmith=generated_stub("generated-blacksmith-return-stub")
  expect(service.address==0xFF6D00 and service.widthBytes==4 and service.instructionHex=="70FF4E75" and service.resultD0Word==65535 and service.purpose=="controlled-diamond-cancel-return","generated service-cancel stub contract drift")
  expect(blacksmith.address==0xFF6D10 and blacksmith.widthBytes==2 and blacksmith.instructionHex=="4E75" and blacksmith.resultD0Word==nil and blacksmith.purpose=="controlled-process-blacksmith-orders-return","generated blacksmith-return stub contract drift")
  for _,stub in ipairs(s.generatedStubs) do
    local bytes=hex_bytes(stub.instructionHex);expect(#bytes==stub.widthBytes,"generated stub width drift: "..stub.role)
    write_bytes(stub.address,bytes);expect(equal_arrays(read_bytes(stub.address,stub.widthBytes),bytes),"generated stub readback drift: "..stub.role)
  end
end
local function write_result_stub(index)
  local target=result_pc(index);local bytes={0x4E,0xF9,target>>24&0xFF,target>>16&0xFF,target>>8&0xFF,target&0xFF}
  write_bytes(h.resultStubAddress,bytes);expect(equal_arrays(read_bytes(h.resultStubAddress,6),bytes),"generated result stub readback drift")
end
local function outer_return_trampoline()
  local trampoline=s.outerReturnTrampoline
  expect(trampoline.address==0xFF6D30 and trampoline.widthBytes==6 and trampoline.instructionPrefixHex=="4EF9" and trampoline.purpose=="generated-source-return-transfer","generated outer-return trampoline contract drift")
  return trampoline
end
local function write_outer_return_trampoline(case,stack)
  local trampoline=outer_return_trampoline();local target=case.outerReturnTargetAddress
  local bytes={0x4E,0xF9,target>>24&0xFF,target>>16&0xFF,target>>8&0xFF,target&0xFF}
  expect(case.outerReturnRole==(case.transferKind=="tail-transfer" and "outer-rts-harness-return" or "outer-caller-return"),"outer-return role contract drift")
  write_bytes(trampoline.address,bytes);expect(equal_arrays(read_bytes(trampoline.address,trampoline.widthBytes),bytes),"generated outer-return trampoline readback drift")
  write_u32(stack,trampoline.address);expect(read_u32(stack)==trampoline.address,"outer-return stack replacement readback drift")
  case._outerReturn={address=trampoline.address,targetAddress=target,widthBytes=trampoline.widthBytes,hex="4EF9"..string.format("%08X",target),serviceEntryStackAddress=stack,sourceReturnAddress=target,postServiceRtsStackAddress=case.postServiceRtsStackAddress}
end
local function context_word(case)
  local words={church=0,shop=1,blacksmith=2};local value=words[case.service]
  expect(value~=nil,"context menu controlled-selection service drift");return value
end
local function start_case(index)
  expect(mode=="none" and index==case_index,"case-entry ordering drift")
  local case=assert(current_case(),"case table exhausted");local entry=case_pc(index)
  set_expectation("case-entry","case-entry",entry,nil,case.callerEntryAddress,nil);expect((emu.getregister("M68K PC")&0xFFFFFF)==entry,"generated case-entry PC drift")
  if index==1 then status("milestone:service-menu-cases-entered") end
  write_u16(s.currentPortraitAddress,0xFFFF);write_u16(frame_base,case.family=="context-menu" and context_word(case) or 0)
  write_u32(stack_top,h.resultStubAddress);expect(read_u32(stack_top)==h.resultStubAddress,"caller return-stack setup drift")
  write_result_stub(index);case._chronology={};case_frames=0;mode="case";append("case-entry",entry)
end
local function caller_call(case)
  if mode~="case" then return end
  set_expectation("caller-call","caller-call-site",case.callSiteAddress,case.callSiteAddress,s.aliases[case.service].effectiveTargetAddress,case.returnAddress)
  expect((emu.getregister("M68K PC")&0xFFFFFF)==case.callSiteAddress,"source caller call-site PC drift");append("caller-call-site",case.callSiteAddress)
end
local function tail_transfer(case)
  if mode~="case" then return end
  set_expectation("tail-transfer","tail-transfer-site",case.callSiteAddress,case.callSiteAddress,s.aliases[case.service].effectiveTargetAddress,nil)
  expect(case.transferKind=="tail-transfer" and case.returnAddress==nil and (emu.getregister("M68K PC")&0xFFFFFF)==case.callSiteAddress,"source tail-transfer PC or continuation drift");append("tail-transfer-site",case.callSiteAddress)
end
local function service_entry(address)
  if mode~="case" then return end
  local case=assert(current_case(),"service entry without case");local expected=s.aliases[case.service].effectiveTargetAddress
  local stack=emu.getregister("M68K A7")&0xFFFFFF
  set_expectation("service-entry","service-entry",address,case.callSiteAddress,expected,case.outerReturnTargetAddress)
  set_stack_expectation(case.serviceEntryStackAddress,case.outerReturnTargetAddress,stack)
  expect(address==expected and source[case.service.."EntryAddress"]==expected and (emu.getregister("M68K PC")&0xFFFFFF)==address and stack==case.serviceEntryStackAddress and read_u32(stack)==case.outerReturnTargetAddress,"alias-resolved service-entry or source-return stack drift")
  write_outer_return_trampoline(case,stack);append("service-entry",address)
end
local function generated_service_stub(address,role)
  if mode~="case" then return end
  local case=assert(current_case(),"generated service stub without case")
  local expected_role=case.service=="blacksmith" and "generated-blacksmith-return-stub" or "generated-service-cancel-stub"
  local expected_purpose=case.service=="blacksmith" and "controlled-process-blacksmith-orders-return" or "controlled-diamond-cancel-return"
  local stub=generated_stub(expected_role)
  set_expectation("generated-service-stub",role,address,case.callSiteAddress,address,s.entrySeams[case.service].controlledReturnAddress)
  expect(role==expected_role and stub.purpose==expected_purpose and address==stub.address and (emu.getregister("M68K PC")&0xFFFFFF)==address and equal_arrays(read_bytes(stub.address,stub.widthBytes),hex_bytes(stub.instructionHex)),"generated service stub identity, purpose, or readback drift");append(role,address)
end
local function outer_return_trampoline_callback(address,role)
  if mode~="case" then return end
  local case=assert(current_case(),"outer-return trampoline without case")
  if role~=case.outerReturnRole then return end
  local trampoline=outer_return_trampoline();local readback=assert(case._outerReturn,"outer-return stack state is absent")
  set_expectation("outer-return-trampoline",role,address,case.callSiteAddress,case.outerReturnTargetAddress,case.outerReturnTargetAddress)
  set_stack_expectation(case.postServiceRtsStackAddress,nil,emu.getregister("M68K A7")&0xFFFFFF)
  expect(address==trampoline.address and (emu.getregister("M68K PC")&0xFFFFFF)==address and (emu.getregister("M68K A7")&0xFFFFFF)==case.postServiceRtsStackAddress and readback.serviceEntryStackAddress==case.serviceEntryStackAddress and readback.sourceReturnAddress==case.outerReturnTargetAddress and readback.postServiceRtsStackAddress==case.postServiceRtsStackAddress and equal_arrays(read_bytes(trampoline.address,trampoline.widthBytes),hex_bytes(readback.hex)),"generated outer-return trampoline stack, target, or readback drift");append(role,address)
end
local function finish_case(index)
  if mode~="case" then return end
  local case=assert(current_case(),"case result without case");local pc=result_pc(index)
  set_expectation("caller-result","caller-result",pc,case.callSiteAddress,s.aliases[case.service].effectiveTargetAddress,case.outerReturnTargetAddress)
  expect(index==case_index and (emu.getregister("M68K PC")&0xFFFFFF)==pc,"caller result PC drift")
  append("caller-result",pc);expect(#case._chronology==6,"callback chronology length drift")
  table.remove(case._chronology,1)
  local expected_d0=case.family=="context-menu" and context_word(case) or s.registerSentinels.d0
  local expected_a6=case.family=="context-menu" and frame_base+2 or frame_base
  local expected_a7=(case.family=="context-menu" or case.returnKind=="rts" or case.transferKind=="tail-transfer") and stack_top+4 or stack_top
  expect((emu.getregister("M68K D0")&0xFFFF)==expected_d0,"caller D0 restoration drift")
  expect((emu.getregister("M68K D1")&0xFFFF)==s.registerSentinels.d1,"caller D1 restoration drift")
  expect((emu.getregister("M68K D2")&0xFFFF)==s.registerSentinels.d2,"caller D2 restoration drift")
  expect((emu.getregister("M68K A6")&0xFFFFFF)==expected_a6,"caller A6 restoration drift")
  expect((emu.getregister("M68K A7")&0xFFFFFF)==expected_a7,"caller A7 restoration drift")
  records[#records+1]={case=case,registers=register_json()};mode="none";case_index=case_index+1
  if case_index>#config.cases then
    expect(restore_all(),"exact observer-state restoration drift");cleanup_session();expect(#event_ids==0,"residual registered callback")
    local output=assert(io.open(config.outputPath,"w"));local rows={};for _,record in ipairs(records) do
      local case_item=record.case;rows[#rows+1]="{\"id\":"..json_string(case_item.caseId)..",\"family\":"..json_string(case_item.family)..",\"service\":"..json_string(case_item.service)..",\"transferKind\":"..json_string(case_item.transferKind)..",\"callSiteAddress\":"..case_item.callSiteAddress..",\"entryAddress\":"..s.aliases[case_item.service].effectiveTargetAddress..",\"returnAddress\":"..nullable(case_item.returnAddress)..",\"resultAddress\":"..(h.baseAddress+(#rows)*h.strideBytes+h.resultOffsetBytes)..",\"returnKind\":"..json_string(case_item.returnKind)..",\"registersAfter\":"..record.registers..",\"serviceBodyBypassed\":true,\"callbackChronology\":"..chronology_json(case_item._chronology).."}"
    end
    local patches={};for _,patch in ipairs(s.sessionPatches) do patches[#patches+1]="{\"role\":"..json_string(patch.role)..",\"address\":"..patch.address..",\"hex\":"..json_string(patch.patchedHex).."}" end
    local stubs={};for _,stub in ipairs(s.generatedStubs) do stubs[#stubs+1]="{\"role\":"..json_string(stub.role)..",\"address\":"..stub.address..",\"widthBytes\":"..stub.widthBytes..",\"hex\":"..json_string(stub.instructionHex).."}" end
    local trampolines={};for _,record in ipairs(records) do local item=record.case;local readback=assert(item._outerReturn,"outer-return trampoline observation is absent");trampolines[#trampolines+1]="{\"id\":"..json_string(item.caseId)..",\"role\":"..json_string(item.outerReturnRole)..",\"address\":"..readback.address..",\"targetAddress\":"..readback.targetAddress..",\"widthBytes\":"..readback.widthBytes..",\"hex\":"..json_string(readback.hex)..",\"serviceEntryStackAddress\":"..readback.serviceEntryStackAddress..",\"sourceReturnAddress\":"..readback.sourceReturnAddress..",\"postServiceRtsStackAddress\":"..readback.postServiceRtsStackAddress.."}" end
    local order={};for _,id in ipairs(config.caseOrder) do order[#order+1]=json_string(id) end
    output:write("{\"system\":"..json_string(emu.getsystemid())..",\"core\":"..json_string(config.core)..",\"id\":"..json_string(config.id)..",\"caseOrder\":["..table.concat(order,",").."],\"records\":["..table.concat(rows,",").."],\"sessionPatchReadback\":["..table.concat(patches,",").."],\"generatedStubReadback\":["..table.concat(stubs,",").."],\"outerReturnTrampolineReadback\":["..table.concat(trampolines,",").."],\"restoration\":{\"currentPortraitRestored\":true,\"callerFrameRestored\":true,\"callbacksCleared\":true},\"callbacksCleared\":0}")
    output:close();status("milestone:callbacks-cleared:0");status("milestone:observer-finished");client.exitCode(0)
  end
end
local function bootstrap_check_sram()
  if bootstrapped or mode~="none" then return end
  set_expectation("bootstrap-return-redirect","bootstrap-return-redirect",h.checkSramAddress,nil,h.checkSramAddress,probe_base)
  local stack=emu.getregister("M68K A7")&0xFFFFFF;expect(stack>=0xFF0000 and stack<=0xFFFFFF,"CheckSram return stack outside work RAM")
  original_portrait=read_u16(s.currentPortraitAddress);original_frame=read_u16(frame_base);write_u32(stack,probe_base);expect(read_u32(stack)==probe_base,"CheckSram return redirect write drift")
  write_probe();case_index=1;bootstrapped=true;status("milestone:direct-function-probe")
end
local function dispatch(address,entry)
  if entry.role=="bootstrap-check-sram" then bootstrap_check_sram()
  elseif entry.role=="case-entry" then start_case(entry.index)
  elseif entry.role=="caller-call-site" then caller_call(entry.case)
  elseif entry.role=="tail-transfer-site" then tail_transfer(entry.case)
  elseif entry.role=="service-entry" then service_entry(address)
  elseif entry.role=="generated-service-cancel-stub" or entry.role=="generated-blacksmith-return-stub" then generated_service_stub(address,entry.role)
  elseif entry.role=="outer-caller-return" or entry.role=="outer-rts-harness-return" then outer_return_trampoline_callback(address,entry.role)
  elseif entry.role=="caller-result" then finish_case(entry.index)
  else error("unknown deterministic dispatch role: "..entry.role) end
end
local function register_exec(address,role,index,case)
  if not callbacks[address] then
    callbacks[address]={};event_ids[#event_ids+1]=event.on_bus_exec(function()
      if observer_failed then return end
      local ok,message=pcall(function() current_pc=address;for _,entry in ipairs(callbacks[address]) do dispatch(address,entry) end end)
      if not ok then fail_callback(message) end
    end,address,"service-menu-entry-return-"..address,"M68K BUS")
  end
  for _,entry in ipairs(callbacks[address]) do if entry.role==role and entry.index==index then return end end
  callbacks[address][#callbacks[address]+1]={role=role,index=index,case=case}
end
write_probe=function()
  validate_session_patches()
  write_generated_stubs()
  expect(h.contextMenuHandlerAddress==0x474B6 and h.checkSramAddress==0x6EA6 and h.caseFrameBudget==180,"source/H1 bootstrap, context handler, or case-frame budget identity drift")
  for _,service in ipairs({"shop","church","caravan","blacksmith"}) do expect(source[service.."EntryAddress"]==s.aliases[service].effectiveTargetAddress,"fixture source-context service-entry drift: "..service) end
  write_u16(probe_base,0x46FC);write_u16(probe_base+2,0x2700)
  write_u16(probe_base+4,0x2C7C);write_u32(probe_base+6,frame_base)
  write_u16(probe_base+10,0x2E7C);write_u32(probe_base+12,stack_top)
  write_u16(probe_base+16,0x7011);write_u16(probe_base+18,0x7222);write_u16(probe_base+20,0x7433)
  write_u16(probe_base+22,0x4EF9);write_u32(probe_base+24,case_pc(1))
  for index,case in ipairs(config.cases) do
    local entry=case_pc(index);local next_pc=index==#config.cases and entry+h.strideBytes or case_pc(index+1)
    write_u16(entry,0x2C7C);write_u32(entry+2,frame_base);write_u16(entry+6,0x2E7C);write_u32(entry+8,stack_top)
    write_u16(entry+12,0x7011);write_u16(entry+14,0x7222);write_u16(entry+16,0x7433);write_u16(entry+18,0x4EF9);write_u32(entry+20,case.callerEntryAddress)
    write_u16(entry+24,0x4E71);write_u16(entry+26,0x4EF9);write_u32(entry+28,next_pc)
    if case.family=="context-menu" then expect(case.callerEntryAddress==h.contextMenuHandlerAddress,"context caller entry identity drift") end
    register_exec(entry,"case-entry",index,case);register_exec(entry+h.resultOffsetBytes,"caller-result",index,case)
    register_exec(case.callSiteAddress,case.transferKind=="tail-transfer" and "tail-transfer-site" or "caller-call-site",index,case)
  end
  register_exec(generated_stub("generated-service-cancel-stub").address,"generated-service-cancel-stub",0,nil)
  register_exec(generated_stub("generated-blacksmith-return-stub").address,"generated-blacksmith-return-stub",0,nil)
  register_exec(outer_return_trampoline().address,"outer-caller-return",0,nil)
  register_exec(outer_return_trampoline().address,"outer-rts-harness-return",0,nil)
  for _,service in ipairs({"shop","church","caravan","blacksmith"}) do register_exec(s.aliases[service].effectiveTargetAddress,"service-entry",0,nil) end
end
status("milestone:observer-loaded")
local ok,message=pcall(function() register_exec(h.checkSramAddress,"bootstrap-check-sram",0,nil);status("milestone:direct-function-probe-armed") end)
if not ok then fail_callback(message) end
while true do
  frames=frames+1;joypad.set({Start=true},1);joypad.set({},2);emu.frameadvance()
  if mode=="case" then
    case_frames=case_frames+1
    if case_frames>h.caseFrameBudget then set_expectation("watchdog","case-watchdog-timeout",emu.getregister("M68K PC")&0xFFFFFF,nil,nil,nil);fail_callback("service-menu case watchdog exhausted") end
  end
  if frames%600==0 then status("frame="..frames..",pc="..string.format("%X",emu.getregister("M68K PC"))) end
end
