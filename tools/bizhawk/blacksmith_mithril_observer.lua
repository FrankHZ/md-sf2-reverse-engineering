-- One grouped direct-ROM probe.  The helper cohort calls PickMithrilWeapon;
-- the transaction cohort jumps to original post-confirmation PlaceOrder; the
-- fulfillment cohort jumps to original @AddItem; and the final cohort enters
-- BlacksmithAction_FulfillOrder at its selection-loop label.  Its harness-only
-- service stubs return through original source branches and stop before any
-- excluded presentation body.
local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local f,t,u,p,ram,c=config["function"],config.transaction,config.fulfillment,config.precommit,config.ram,config.constants
local probe_base,helper_base,helper_stride,transaction_base,transaction_stride,fulfillment_base,fulfillment_stride,precommit_base,precommit_stride=0xFF6800,0xFF6820,24,0xFF6900,32,0xFF6960,32,0xFF6B00,32
local frame_base,stack_top=0xFF6A00,0xFFFF00
local callbacks,event_ids,helper_records,transaction_records,fulfillment_records,precommit_records={}, {}, {}, {}, {}, {}
local observer_failed,session_cleaned,bootstrapped=false,false,false
local mode,helper_index,transaction_index,fulfillment_index,precommit_index="none",0,0,0,0
local helper_active,first_case_milestone,transaction_milestone,fulfillment_milestone,precommit_milestone=false,false,false,false,false
local original_gold,original_seed,original_orders,original_flag,original_records,original_dialogue_name,original_selected_item,original_submenu_action=nil,nil,nil,nil,nil,nil,nil,nil
local precommit_state={serviceStub=0xFF6D00,terminalStub=0xFF6D20,serviceReadbacks={},terminalReadbacks={},generatedServiceStubWrites=false,generatedResultStubWrites=false}
local pending_rng,row_index,selected_item,function_return_seen,order_write_seen=nil,nil,nil,false,false
local tx={active=false,decreaseGoldReturnSeen=false,pendingOrdersIncrementSeen=false,dropItemReturnSeen=false,pickReturnSeen=false,clearFlagReturnSeen=false,prePresentationReturnAddress=nil,record=nil,rngCalls=nil,rowIndex=nil,selectedItem=nil,orderWriteSeen=false,chronology=nil}
local fx={active=false,addItemReturnSeen=false,orderReadSeen=false,orderClearedSeen=false,fulfilledOrdersIncrementSeen=false,equippabilityCarrySet=nil,originalReturnAddress=nil,record=nil,chronology=nil}
local pcx={active=false,attemptIndex=0,memberListCallCount=0,heldItemsCallCount=0,equipmentTypeCallCount=0,equippabilityCallCount=0,pendingService=nil,selectedMember=nil,terminal="none",expectedTerminal="none",frameCount=0,frameBudget=0,record=nil,chronology=nil,transition={active=false,frameCount=0,frameBudget=config.precommitTransitionFrameBudget},cleanup={active=false,case=nil,addItemReturnSeen=false,orderReadSeen=false,orderClearedSeen=false,fulfilledOrdersIncrementSeen=false,equippabilityCarrySet=nil}}
local current_phase,current_role,current_pc,current_expectation="registration","registration",nil,nil
local write_probe,expect

local function status(value) local file=assert(io.open(config.statusPath,"a"));file:write(value.."\n");file:close() end
local function bool(value) return value and "true" or "false" end
local function nullable(value) return value==nil and "null" or tostring(value) end
local function json_string(value) return string.format("%q",value) end
local function word(value) return value&0xFFFF end
local function current_helper() return config.cases[helper_index] end
local function current_transaction() return config.transactionCases[transaction_index] end
local function current_fulfillment() return config.fulfillmentCases[fulfillment_index] end
local function current_precommit() return config.precommitCases[precommit_index] end
function pcx.cleanup_case()
  local case=assert(current_precommit(),"precommit cleanup case table exhausted")
  for _,candidate in ipairs(config.fulfillmentCases) do
    if candidate.id==case.cleanupFulfillmentCaseId then return candidate end
  end
  error("precommit cleanup fulfillment identity drift")
end
local function helper_pc(index) return helper_base+(index-1)*helper_stride end
local function transaction_pc(index) return transaction_base+(index-1)*transaction_stride end
local function fulfillment_pc(index) return fulfillment_base+(index-1)*fulfillment_stride end
local function precommit_pc(index) return precommit_base+(index-1)*precommit_stride end
local function frame_address() return frame_base-t.frameOffsetsBytes.clientClass end
local function fulfillment_frame_address() return frame_base-u.frameOffsetsBytes.clientClass end
local function read_u8(address) return memory.read_u8(address,"M68K BUS") end
local function write_u8(address,value) memory.write_u8(address,value,"M68K BUS") end
local function read_bytes(address,count)
  local bytes={};for offset=0,count-1 do bytes[#bytes+1]=read_u8(address+offset) end;return bytes
end
local function write_bytes(address,bytes)
  for offset,value in ipairs(bytes) do write_u8(address+offset-1,value) end
end
local function read_orders()
  local orders={};for index=0,c.orderSlotCount-1 do orders[#orders+1]=memory.read_u16_be(ram.ordersAddress+index*c.orderSlotSize,"M68K BUS") end
  return orders
end
local function write_orders(orders)
  for index,value in ipairs(orders) do memory.write_u16_be(ram.ordersAddress+(index-1)*c.orderSlotSize,value,"M68K BUS") end
end
local function read_item_words(member)
  local values,base={},ram.combatantDataAddress+member*c.combatantEntrySizeBytes+c.combatantItemsOffsetBytes
  for index=0,c.combatantItemSlotCount-1 do values[#values+1]=memory.read_u16_be(base+index*2,"M68K BUS") end
  return values
end
local function write_item_words(member,values)
  local base=ram.combatantDataAddress+member*c.combatantEntrySizeBytes+c.combatantItemsOffsetBytes
  for index,value in ipairs(values) do memory.write_u16_be(base+(index-1)*2,value,"M68K BUS") end
end
local function combatant_base(member) return ram.combatantDataAddress+member*c.combatantEntrySizeBytes end
local function read_class(member) return read_u8(combatant_base(member)+c.combatantClassOffsetBytes) end
local function write_class(member,value) write_u8(combatant_base(member)+c.combatantClassOffsetBytes,value) end
local function read_record(member)
  local bytes,base={},ram.combatantDataAddress+member*c.combatantEntrySizeBytes
  for index=0,c.combatantEntrySizeBytes-1 do bytes[#bytes+1]=read_u8(base+index) end
  return bytes
end
local function write_record(member,bytes)
  local base=ram.combatantDataAddress+member*c.combatantEntrySizeBytes
  for index,value in ipairs(bytes) do write_u8(base+index-1,value) end
end
local function equal_arrays(left,right)
  if #left~=#right then return false end
  for index,value in ipairs(left) do if right[index]~=value then return false end end
  return true
end
local function unregister_events() for index=#event_ids,1,-1 do event.unregisterbyid(event_ids[index]);event_ids[index]=nil end end
local function cleanup_session() if session_cleaned then return end;session_cleaned=true;unregister_events() end
local function roles_json(address)
  if address==nil then return "[]" end
  local roles={};for _,entry in ipairs(callbacks[address] or {}) do roles[#roles+1]=json_string(entry.role) end
  return "["..table.concat(roles,",").."]"
end
local function pending_rng_json()
  if not pending_rng then return "null" end
  return "{\"role\":"..json_string(pending_rng.role)..",\"callPc\":"..pending_rng.callPc..",\"targetPc\":"..pending_rng.targetPc..",\"returnPc\":"..pending_rng.returnPc..",\"rangeWord\":"..pending_rng.rangeWord.."}"
end
local function transaction_state_json()
  return "{\"active\":"..bool(tx.active)..",\"mode\":"..json_string(mode)..",\"decreaseGoldReturnSeen\":"..bool(tx.decreaseGoldReturnSeen)..",\"pendingOrdersIncrementSeen\":"..bool(tx.pendingOrdersIncrementSeen)..",\"dropItemReturnSeen\":"..bool(tx.dropItemReturnSeen)..",\"pickReturnSeen\":"..bool(tx.pickReturnSeen)..",\"clearFlagReturnSeen\":"..bool(tx.clearFlagReturnSeen)..",\"prePresentationReturnAddress\":"..nullable(tx.prePresentationReturnAddress).."}"
end
local function fulfillment_state_json()
  return "{\"active\":"..bool(fx.active)..",\"mode\":"..json_string(mode)..",\"addItemReturnSeen\":"..bool(fx.addItemReturnSeen)..",\"orderReadSeen\":"..bool(fx.orderReadSeen)..",\"orderClearedSeen\":"..bool(fx.orderClearedSeen)..",\"fulfilledOrdersIncrementSeen\":"..bool(fx.fulfilledOrdersIncrementSeen)..",\"equippabilityCarrySet\":"..(fx.equippabilityCarrySet==nil and "null" or bool(fx.equippabilityCarrySet))..",\"originalReturnAddress\":"..nullable(fx.originalReturnAddress).."}"
end
local function precommit_service_json()
  if pcx.pendingService==nil then return "null" end
  local service=pcx.pendingService;return "{\"callPc\":"..service.callPc..",\"returnPc\":"..service.returnPc..",\"role\":"..json_string(service.role)..",\"targetPc\":"..service.targetPc.."}"
end
local function precommit_state_json()
  return "{\"active\":"..bool(pcx.active)..",\"attemptIndex\":"..pcx.attemptIndex..",\"equipmentTypeCallCount\":"..pcx.equipmentTypeCallCount..",\"equippabilityCallCount\":"..pcx.equippabilityCallCount..",\"expectedTerminal\":"..json_string(pcx.expectedTerminal)..",\"frameBudget\":"..pcx.frameBudget..",\"frameCount\":"..pcx.frameCount..",\"heldItemsCallCount\":"..pcx.heldItemsCallCount..",\"memberListCallCount\":"..pcx.memberListCallCount..",\"mode\":"..json_string(mode)..",\"pendingService\":"..precommit_service_json()..",\"selectedMember\":"..nullable(pcx.selectedMember)..",\"terminal\":"..json_string(pcx.terminal).."}"
end
local function pending_callback_state()
  local case_for_state=0
  if mode=="helper" or current_role=="case-entry" then case_for_state=helper_index elseif mode=="transaction" or current_role=="transaction-case-entry" then case_for_state=transaction_index elseif mode=="fulfillment" or current_role=="fulfillment-case-entry" then case_for_state=fulfillment_index elseif mode=="precommit" or mode=="precommit-cleanup" or current_role=="precommit-case-entry" or pcx.transition.active then case_for_state=precommit_index end
  return "{\"active\":"..bool(helper_active or tx.active or fx.active or pcx.active or pcx.cleanup.active or pcx.transition.active)..",\"caseIndex\":"..case_for_state..",\"functionReturnSeen\":"..bool(function_return_seen)..",\"orderWriteSeen\":"..bool(order_write_seen or tx.orderWriteSeen)..",\"pendingRngCall\":"..pending_rng_json()..",\"rolesAtPc\":"..roles_json(current_pc)..",\"transaction\":"..transaction_state_json()..",\"fulfillment\":"..fulfillment_state_json()..",\"precommit\":"..precommit_state_json().."}"
end
local function hex_bytes(value)
  expect(type(value)=="string" and #value%2==0 and value:match("^[0-9A-F]+$")~=nil,"precommit shim hexadecimal contract drift")
  local bytes={};for index=1,#value,2 do bytes[#bytes+1]=tonumber(value:sub(index,index+1),16) end;return bytes
end
local function precommit_shim(role,spec)
  for _,shim in ipairs(p.serviceShims) do
    if shim.role==role then
      expect(shim.callAddress==spec.callAddress and shim.instructionTargetAddress==spec.instructionTargetAddress and shim.effectiveTargetAddress==spec.effectiveTargetAddress and shim.returnAddress==spec.returnAddress,"precommit service shim source identity drift: "..role)
      expect(shim.generatedStubTarget==precommit_state.serviceStub and shim.patchedHex=="4EB900FF6D00","precommit service shim generated target/opcode drift: "..role)
      return shim
    end
  end
  error("precommit service shim missing: "..role)
end
local function precommit_terminal_shim(role,address)
  for _,shim in ipairs(p.terminalShims) do
    if shim.role==role then
      expect(shim.type=="terminal-jmp" and shim.boundaryAddress==address,"precommit terminal shim source identity drift: "..role)
      expect(shim.generatedStubTarget==precommit_state.terminalStub and shim.patchedHex=="4EF900FF6D20","precommit terminal shim generated target/opcode drift: "..role)
      return shim
    end
  end
  error("precommit terminal shim missing: "..role)
end
local function validate_precommit_service_call(shim)
  local original=hex_bytes(shim.originalHex);local patched=hex_bytes(shim.patchedHex);expect(#original==6 and #patched==6 and original[1]==0x4E and original[2]==0xB9 and patched[1]==0x4E and patched[2]==0xB9,"precommit service shim JSR opcode drift")
  expect(equal_arrays(read_bytes(shim.callAddress,#patched),patched),"precommit instrumented service call readback drift: "..shim.role);precommit_state.serviceReadbacks[shim.role]=true
end
local function validate_precommit_terminal_boundary(shim)
  local original=hex_bytes(shim.originalHex);local patched=hex_bytes(shim.patchedHex);expect(#original==6 and #patched==6 and patched[1]==0x4E and patched[2]==0xF9,"precommit terminal shim JMP opcode drift")
  expect(equal_arrays(read_bytes(shim.boundaryAddress,#patched),patched),"precommit instrumented terminal boundary readback drift: "..shim.role);precommit_state.terminalReadbacks[shim.role]=true
end
local function write_precommit_service_stub(role,attempt)
  local bytes
  if role=="member-list" then bytes={0x30,0x3C,word(attempt.selectedMemberResult)>>8,word(attempt.selectedMemberResult)&0xFF,0x4E,0x75}
  elseif role=="held-items" then bytes={0x34,0x3C,word(attempt.heldItemsCountResult)>>8,word(attempt.heldItemsCountResult)&0xFF,0x4E,0x75}
  elseif role=="equipment-type" then bytes={0x34,0x3C,word(attempt.equipmentTypeResult)>>8,word(attempt.equipmentTypeResult)&0xFF,0x4E,0x75}
  elseif role=="equippability" then bytes={0x44,0xFC,attempt.equippableCarrySetResult and 0 or 0,attempt.equippableCarrySetResult and 1 or 0,0x4E,0x75}
  else error("precommit generated service stub role drift: "..role) end
  write_bytes(precommit_state.serviceStub,bytes);expect(equal_arrays(read_bytes(precommit_state.serviceStub,#bytes),bytes),"precommit generated service stub write drift: "..role);precommit_state.generatedServiceStubWrites=true
end
local function write_precommit_result_stub(target)
  local bytes={0x4E,0xF9,target>>24&0xFF,target>>16&0xFF,target>>8&0xFF,target&0xFF}
  write_bytes(precommit_state.terminalStub,bytes);expect(equal_arrays(read_bytes(precommit_state.terminalStub,#bytes),bytes),"precommit generated result stub write drift");precommit_state.generatedResultStubWrites=true
end
local function restore_all()
  if original_gold==nil then return true end
  memory.write_u32_be(ram.currentGoldAddress,original_gold,"M68K BUS")
  memory.write_u16_be(ram.randomSeedAddress,original_seed,"M68K BUS")
  write_orders(original_orders);write_u8(ram.flag80OwningByteAddress,original_flag)
  for member,bytes in pairs(original_records) do write_record(member,bytes) end
  memory.write_u16_be(ram.dialogueNameIndex1Address,original_dialogue_name,"M68K BUS");memory.write_u16_be(ram.selectedItemIndexAddress,original_selected_item,"M68K BUS");write_u8(ram.currentItemSubmenuActionAddress,original_submenu_action)
  if memory.read_u32_be(ram.currentGoldAddress,"M68K BUS")~=original_gold then return false end
  if memory.read_u16_be(ram.randomSeedAddress,"M68K BUS")~=original_seed then return false end
  if not equal_arrays(read_orders(),original_orders) then return false end
  if read_u8(ram.flag80OwningByteAddress)~=original_flag then return false end
  for member,bytes in pairs(original_records) do if not equal_arrays(read_record(member),bytes) then return false end end
  if memory.read_u16_be(ram.dialogueNameIndex1Address,"M68K BUS")~=original_dialogue_name then return false end
  if memory.read_u16_be(ram.selectedItemIndexAddress,"M68K BUS")~=original_selected_item then return false end
  if read_u8(ram.currentItemSubmenuActionAddress)~=original_submenu_action then return false end
  return true
end
local function fail_callback(message)
  if observer_failed then return end
  observer_failed=true
  local case=nil
  if mode=="helper" or current_role=="case-entry" then case=current_helper() elseif mode=="transaction" or current_role=="transaction-case-entry" then case=current_transaction() elseif mode=="fulfillment" or current_role=="fulfillment-case-entry" then case=current_fulfillment() elseif mode=="precommit" or mode=="precommit-cleanup" or current_role=="precommit-case-entry" or pcx.transition.active then case=current_precommit() end
  local restored,restore_message=pcall(restore_all)
  local expected=current_expectation or {};local actual=current_role=="registration" and nil or emu.getregister("M68K PC")
  local detail=tostring(message);if not restored then detail=detail.."; restoration error: "..tostring(restore_message) elseif restore_message~=true then detail=detail.."; restoration readback drift" end
  os.remove(config.outputPath);cleanup_session()
  local output_handle=io.open(config.outputPath,"r");local output_removed=output_handle==nil;if output_handle then output_handle:close() end
  local session_state_restored=original_gold~=nil and restored and restore_message==true
  local callbacks_remaining=#event_ids
  local stack=pcx.cleanupStackDiagnostic or {}
  local payload="{\"owner\":"..json_string(config.observerFailureContract.owner)..",\"caseId\":"..(case and json_string(case.id) or "null")..",\"phase\":"..json_string(current_phase)..",\"role\":"..json_string(current_role)..",\"actualPc\":"..nullable(actual)..",\"expectedEventPc\":"..nullable(expected.eventPc)..",\"expectedCallPc\":"..nullable(expected.callPc)..",\"expectedTargetPc\":"..nullable(expected.targetPc)..",\"expectedReturnPc\":"..nullable(expected.returnPc)..",\"expectedStackTop\":"..nullable(stack.expectedTop)..",\"actualStackTop\":"..nullable(stack.actualTop)..",\"expectedStackReturn\":"..nullable(stack.expectedReturn)..",\"actualStackReturn\":"..nullable(stack.actualReturn)..",\"callbacksRemaining\":"..callbacks_remaining..",\"sessionStateRestored\":"..bool(session_state_restored)..",\"outputRemoved\":"..bool(output_removed)..",\"pendingCallback\":"..pending_callback_state()..",\"error\":"..json_string(detail).."}"
  local diagnostic=config.observerFailureContract.statusPrefix..payload;status(diagnostic);print(diagnostic);client.exitCode(config.observerFailureContract.exitCode)
end
expect=function(condition,message) if not condition then error(message) end end
local function set_expectation(phase,role,event_pc,call_pc,target_pc,return_pc)
  current_phase,current_role,current_expectation=phase,role,{eventPc=event_pc,callPc=call_pc,targetPc=target_pc,returnPc=return_pc}
end
local function snapshot_exact_boundary()
  original_gold=memory.read_u32_be(ram.currentGoldAddress,"M68K BUS");original_seed=memory.read_u16_be(ram.randomSeedAddress,"M68K BUS");original_orders=read_orders();original_flag=read_u8(ram.flag80OwningByteAddress);original_dialogue_name=memory.read_u16_be(ram.dialogueNameIndex1Address,"M68K BUS");original_selected_item=memory.read_u16_be(ram.selectedItemIndexAddress,"M68K BUS");original_submenu_action=read_u8(ram.currentItemSubmenuActionAddress);original_records={}
  for _,cohort in ipairs({config.transactionCases,config.fulfillmentCases}) do
    for _,case in ipairs(cohort) do
      if original_records[case.clientMember]==nil then original_records[case.clientMember]=read_record(case.clientMember) end
    end
  end
  local record_count=0;for _ in pairs(original_records) do record_count=record_count+1 end
  expect(record_count==3,"blacksmith cohorts must snapshot exactly three combatant records")
end
local function record_rng_call(destination)
  local call=pending_rng;destination[#destination+1]={role=call.role,callPc=call.callPc,targetPc=call.targetPc,returnPc=call.returnPc,rangeWord=call.rangeWord,result=word(emu.getregister("M68K D7")),randomSeedAfter=memory.read_u16_be(ram.randomSeedAddress,"M68K BUS")}
end
local function helper_case_expectation(index,event_pc)
  local entry=helper_pc(index);return entry,event_pc or entry,entry+10,f.entryAddress,entry+16
end
local function start_helper_case(index)
  expect(mode=="none" and index==helper_index,"helper case-entry dispatch drift")
  local case=assert(current_helper(),"helper case table exhausted");local entry,event_pc,call_pc,target_pc,return_pc=helper_case_expectation(index)
  set_expectation("case-entry","case-entry",event_pc,call_pc,target_pc,return_pc);expect(emu.getregister("M68K PC")==entry,"helper case-entry PC drift")
  memory.write_u16_be(frame_base,case.clientClass,"M68K BUS");memory.write_u16_be(ram.randomSeedAddress,case.randomSeedBefore,"M68K BUS");write_orders(case.ordersBefore)
  case._rngCalls={};row_index=nil;selected_item=nil;function_return_seen=false;order_write_seen=false;pending_rng=nil;helper_active=true;mode="helper"
  if not first_case_milestone then status("milestone:first-case-entered");first_case_milestone=true end
end
local function helper_entry()
  if mode~="helper" then return end
  local entry,event_pc,call_pc,target_pc,return_pc=helper_case_expectation(helper_index,f.entryAddress)
  set_expectation("function-entry","function-entry",event_pc,call_pc,target_pc,return_pc);expect(helper_active,"helper entry while inactive");expect(emu.getregister("M68K PC")==f.entryAddress,"helper entry PC drift")
end
local function helper_rng_call(role,call_pc,return_pc)
  if mode~="helper" then return end
  expect(helper_active and pending_rng==nil,"overlapping helper RNG call state");set_expectation(role,role,call_pc,call_pc,f.rngEntryAddress,return_pc);expect(emu.getregister("M68K PC")==call_pc,"helper RNG call PC drift")
  local range_word=word(emu.getregister("M68K D6"));expect(range_word>0,"helper RNG range must be nonzero");if role=="fallback-row-roll" then expect(range_word==2,"helper fallback RNG range drift") end
  pending_rng={role=role,callPc=call_pc,targetPc=f.rngEntryAddress,returnPc=return_pc,rangeWord=range_word}
end
local function helper_rng_entry()
  if mode~="helper" then return end
  expect(helper_active and pending_rng~=nil,"helper RNG entry without call");set_expectation("rng-entry","rng-entry",f.rngEntryAddress,pending_rng.callPc,pending_rng.targetPc,pending_rng.returnPc);expect(emu.getregister("M68K PC")==f.rngEntryAddress,"helper RNG entry PC drift")
end
local function helper_rng_return()
  if mode~="helper" then return end
  expect(helper_active and pending_rng~=nil,"helper RNG return without call");set_expectation("rng-return","rng-return",f.rngReturnRtsAddress,pending_rng.callPc,pending_rng.targetPc,pending_rng.returnPc);expect(emu.getregister("M68K PC")==f.rngReturnRtsAddress,"helper RNG RTS PC drift");record_rng_call(current_helper()._rngCalls);pending_rng=nil
end
local function helper_row_resolved()
  if mode~="helper" then return end
  expect(helper_active,"helper row resolution outside case");set_expectation("row-resolved","row-resolved",f.rowResolvedAddress,nil,nil,nil);row_index=word(emu.getregister("M68K D0"));expect(row_index<c.weaponRowCount,"helper row index outside table")
end
local function helper_item_selected()
  if mode~="helper" then return end
  expect(helper_active and row_index~=nil and pending_rng==nil,"helper item selection state drift");set_expectation("item-selected","item-selected",f.loadIndexAddress,nil,nil,nil);selected_item=word(emu.getregister("M68K D1"));expect(#current_helper()._rngCalls>0,"helper item selection without RNG")
end
local function helper_order_write()
  if mode~="helper" then return end
  expect(helper_active and selected_item~=nil,"helper order write before selected item");set_expectation("order-write","order-write",f.orderWriteAddress,nil,nil,nil);order_write_seen=true
end
local function helper_function_rts()
  if mode~="helper" then return end
  expect(helper_active and pending_rng==nil,"helper RTS with pending RNG");set_expectation("function-rts","function-rts",f.returnRtsAddress,helper_pc(helper_index)+10,f.entryAddress,helper_pc(helper_index)+16);expect(emu.getregister("M68K PC")==f.returnRtsAddress,"helper RTS guard PC drift");function_return_seen=true
end
local function array_json(values) local parts={};for _,value in ipairs(values) do parts[#parts+1]=tostring(value) end;return "["..table.concat(parts,",").."]" end
local function rng_calls_json(calls)
  local parts={};for _,call in ipairs(calls) do parts[#parts+1]="{\"role\":"..json_string(call.role)..",\"callPc\":"..call.callPc..",\"targetPc\":"..call.targetPc..",\"returnPc\":"..call.returnPc..",\"rangeWord\":"..call.rangeWord..",\"result\":"..call.result..",\"randomSeedAfter\":"..call.randomSeedAfter.."}" end;return "["..table.concat(parts,",").."]"
end
local function helper_record_json(record)
  return "{\"id\":"..json_string(record.id)..",\"classGroupIndex\":"..record.classGroupIndex..",\"weaponRowIndex\":"..record.weaponRowIndex..",\"choiceIndex\":"..record.choiceIndex..",\"itemIndex\":"..record.itemIndex..",\"orderWriteIndex\":"..nullable(record.orderWriteIndex)..",\"ordersAfter\":"..array_json(record.ordersAfter)..",\"randomSeedAfter\":"..record.randomSeedAfter..",\"rngCalls\":"..rng_calls_json(record.rngCalls)..",\"functionReturnSeen\":true,\"preservedD0\":"..record.preservedD0..",\"preservedD7\":"..record.preservedD7.."}"
end
local function chronology_json(events)
  local parts={};for _,event_item in ipairs(events) do parts[#parts+1]="{\"role\":"..json_string(event_item.role)..",\"pc\":"..event_item.pc.."}" end;return "["..table.concat(parts,",").."]"
end
local function transaction_record_json(record)
  return "{\"id\":"..json_string(record.id)..",\"clientMember\":"..record.clientMember..",\"itemSlot\":"..record.itemSlot..",\"goldBefore\":"..record.goldBefore..",\"goldAfter\":"..record.goldAfter..",\"pendingOrdersBefore\":"..record.pendingOrdersBefore..",\"pendingOrdersAfter\":"..record.pendingOrdersAfter..",\"clientItemWordsBefore\":"..array_json(record.clientItemWordsBefore)..",\"clientItemWordsAfter\":"..array_json(record.clientItemWordsAfter)..",\"ordersBefore\":"..array_json(record.ordersBefore)..",\"ordersAfter\":"..array_json(record.ordersAfter)..",\"flag80OwningByteBefore\":"..record.flag80OwningByteBefore..",\"flag80OwningByteAfter\":"..record.flag80OwningByteAfter..",\"randomSeedBefore\":"..record.randomSeedBefore..",\"randomSeedAfter\":"..record.randomSeedAfter..",\"classGroupIndex\":"..record.classGroupIndex..",\"weaponRowIndex\":"..record.weaponRowIndex..",\"choiceIndex\":"..record.choiceIndex..",\"itemIndex\":"..record.itemIndex..",\"orderWriteIndex\":"..record.orderWriteIndex..",\"rngCalls\":"..rng_calls_json(record.rngCalls)..",\"callbackChronology\":"..chronology_json(record.callbackChronology)..",\"safeExitOriginalReturnPc\":"..record.safeExitOriginalReturnPc..",\"safeExitSeen\":true}"
end
local function fulfillment_record_json(record)
  return "{\"id\":"..json_string(record.id)..",\"clientMember\":"..record.clientMember..",\"recipientClass\":"..record.recipientClass..",\"itemIndex\":"..record.itemIndex..",\"clientItemWordsBefore\":"..array_json(record.clientItemWordsBefore)..",\"clientItemWordsAfter\":"..array_json(record.clientItemWordsAfter)..",\"itemWriteIndex\":"..record.itemWriteIndex..",\"addItemResultCode\":"..record.addItemResultCode..",\"ordersBefore\":"..array_json(record.ordersBefore)..",\"ordersAfter\":"..array_json(record.ordersAfter)..",\"ordersCounter\":"..record.ordersCounter..",\"selectedOrderIndex\":"..record.selectedOrderIndex..",\"sourceOrderWordRead\":"..record.sourceOrderWordRead..",\"fulfilledOrdersBefore\":"..record.fulfilledOrdersBefore..",\"fulfilledOrdersAfter\":"..record.fulfilledOrdersAfter..",\"equippableCarrySet\":"..bool(record.equippableCarrySet)..",\"callbackChronology\":"..chronology_json(record.callbackChronology)..",\"safeExitOriginalReturnPc\":"..record.safeExitOriginalReturnPc..",\"safeExitSeen\":true}"
end
local function precommit_record_json(record)
  return "{\"id\":"..json_string(record.id)..",\"itemIndex\":"..record.itemIndex..",\"attemptCount\":"..record.attemptCount..",\"selectedMember\":"..nullable(record.selectedMember)..",\"ordersBefore\":"..array_json(record.ordersBefore)..",\"ordersAfter\":"..array_json(record.ordersAfter)..",\"fulfilledOrdersBefore\":"..record.fulfilledOrdersBefore..",\"fulfilledOrdersAfter\":"..record.fulfilledOrdersAfter..",\"terminal\":"..json_string(record.terminal)..",\"terminalPc\":"..record.terminalPc..",\"addItemMutationObserved\":false,\"orderMutationObserved\":false,\"fulfilledOrdersMutationObserved\":false,\"callbackChronology\":"..chronology_json(record.callbackChronology).."}"
end
local function precommit_readback_roles_json(rows,readbacks)
  local roles={};for _,row in ipairs(rows) do expect(readbacks[row.role],"precommit instrumentation readback missing: "..row.role);roles[#roles+1]=json_string(row.role) end
  return "["..table.concat(roles,",").."]"
end
local function write_output()
  local helpers,transactions,fulfillments,precommits,helper_order,transaction_order,fulfillment_order,precommit_order={}, {}, {}, {}, {}, {}, {}, {}
  for _,record in ipairs(helper_records) do helpers[#helpers+1]=helper_record_json(record) end
  for _,record in ipairs(transaction_records) do transactions[#transactions+1]=transaction_record_json(record) end
  for _,record in ipairs(fulfillment_records) do fulfillments[#fulfillments+1]=fulfillment_record_json(record) end
  for _,record in ipairs(precommit_records) do precommits[#precommits+1]=precommit_record_json(record) end
  for _,id in ipairs(config.caseOrder) do helper_order[#helper_order+1]=json_string(id) end
  for _,id in ipairs(config.transactionCaseOrder) do transaction_order[#transaction_order+1]=json_string(id) end
  for _,id in ipairs(config.fulfillmentCaseOrder) do fulfillment_order[#fulfillment_order+1]=json_string(id) end
  for _,id in ipairs(config.precommitCaseOrder) do precommit_order[#precommit_order+1]=json_string(id) end
  expect(precommit_state.generatedServiceStubWrites and precommit_state.generatedResultStubWrites,"precommit generated stub readback state drift")
  local service_roles=precommit_readback_roles_json(p.serviceShims,precommit_state.serviceReadbacks);local terminal_roles=precommit_readback_roles_json(p.terminalShims,precommit_state.terminalReadbacks)
  local output=assert(io.open(config.outputPath,"w"));output:write("{\"system\":"..json_string(emu.getsystemid())..",\"core\":"..json_string(config.core)..",\"id\":"..json_string(config.id)..",\"caseOrder\":["..table.concat(helper_order,",").."],\"records\":["..table.concat(helpers,",").."],\"transactionCaseOrder\":["..table.concat(transaction_order,",").."],\"transactionRecords\":["..table.concat(transactions,",").."],\"fulfillmentCaseOrder\":["..table.concat(fulfillment_order,",").."],\"fulfillmentRecords\":["..table.concat(fulfillments,",").."],\"precommitCaseOrder\":["..table.concat(precommit_order,",").."],\"precommitRecords\":["..table.concat(precommits,",").."],\"callbacksCleared\":0,\"precommitInstrumentation\":{\"serviceCallSitesReadback\":"..service_roles..",\"terminalBoundarySitesReadback\":"..terminal_roles..",\"generatedServiceStubWritesReadback\":true,\"generatedResultStubWritesReadback\":true},\"precommitRestoration\":{\"dialogueNameIndex1WordRestored\":true,\"selectedItemIndexWordRestored\":true,\"currentItemSubmenuActionByteRestored\":true},\"restoration\":{\"currentGoldLongRestored\":true,\"randomSeedWordRestored\":true,\"orderWordsRestored\":true,\"flag80OwningByteRestored\":true,\"clientCombatantRecordsRestored\":true}}");output:close()
end
local function finish_helper_case(index)
  if mode~="helper" then return end
  expect(index==helper_index,"helper result dispatch drift");local case=current_helper();local entry,event_pc,call_pc,target_pc,return_pc=helper_case_expectation(index,helper_pc(index)+16)
  set_expectation("case-result","case-result",event_pc,call_pc,target_pc,return_pc);expect(emu.getregister("M68K PC")==helper_pc(index)+16,"helper return target drift");expect(function_return_seen and pending_rng==nil and row_index~=nil and selected_item~=nil,"incomplete helper callback sequence")
  local after=read_orders();local differences={};for slot,before in ipairs(case.ordersBefore) do if after[slot]~=before then differences[#differences+1]=slot-1 end end
  expect(#differences<=1,"helper wrote more than one order slot");expect(order_write_seen==(#differences==1),"helper order-write callback/RAM drift");if #differences==1 then expect(after[differences[1]+1]==selected_item,"helper order write item drift") end
  local special=case.clientClass==c.brnClass or case.clientClass==c.rdbnClass;local group_index=special and c.classGroupsCounter+1 or row_index;local weapon_calls=0;for _,call in ipairs(case._rngCalls) do if call.role=="weapon-row-roll" then weapon_calls=weapon_calls+1 end end
  helper_records[#helper_records+1]={id=case.id,classGroupIndex=group_index,weaponRowIndex=row_index,choiceIndex=weapon_calls-1,itemIndex=selected_item,orderWriteIndex=#differences==1 and differences[1] or nil,ordersAfter=after,randomSeedAfter=memory.read_u16_be(ram.randomSeedAddress,"M68K BUS"),rngCalls=case._rngCalls,preservedD0=word(emu.getregister("M68K D0")),preservedD7=word(emu.getregister("M68K D7"))}
  helper_active=false;mode="none";helper_index=helper_index+1
  if helper_index>#config.cases then transaction_index=1;status("milestone:transaction-cases-entered");transaction_milestone=true end
end
local function tx_event(role,pc,call_pc,target_pc,return_pc)
  set_expectation("transaction",role,pc,call_pc,target_pc,return_pc);expect(mode=="transaction" and tx.active,"transaction callback outside active case");expect(emu.getregister("M68K PC")==pc,"transaction callback PC drift: "..role);tx.chronology[#tx.chronology+1]={role=role,pc=pc}
end
local function start_transaction_case(index)
  expect(mode=="none" and index==transaction_index,"transaction case-entry dispatch drift");local case=assert(current_transaction(),"transaction case table exhausted");local entry=transaction_pc(index)
  set_expectation("transaction-case-entry","transaction-case-entry",entry,entry+14,t.placeEntryAddress,nil);expect(emu.getregister("M68K PC")==entry,"transaction case-entry PC drift")
  write_record(case.clientMember,original_records[case.clientMember]);memory.write_u32_be(ram.currentGoldAddress,case.goldBefore,"M68K BUS");memory.write_u16_be(ram.randomSeedAddress,case.randomSeedBefore,"M68K BUS");write_orders(case.ordersBefore);write_u8(ram.flag80OwningByteAddress,case.flag80OwningByteBefore);write_item_words(case.clientMember,case.clientItemWordsBefore)
  local frame=frame_address();memory.write_u16_be(frame+t.frameOffsetsBytes.clientClass,case.clientClass,"M68K BUS");memory.write_u16_be(frame+t.frameOffsetsBytes.clientMember,case.clientMember,"M68K BUS");memory.write_u16_be(frame+t.frameOffsetsBytes.itemSlot,case.itemSlot,"M68K BUS");memory.write_u16_be(frame+t.frameOffsetsBytes.pendingOrdersNumber,case.pendingOrdersBefore,"M68K BUS")
  expect(memory.read_u32_be(ram.currentGoldAddress,"M68K BUS")==case.goldBefore,"transaction gold setup drift");expect(memory.read_u16_be(ram.randomSeedAddress,"M68K BUS")==case.randomSeedBefore,"transaction seed setup drift");expect(equal_arrays(read_orders(),case.ordersBefore),"transaction orders setup drift");expect(read_u8(ram.flag80OwningByteAddress)==case.flag80OwningByteBefore,"transaction flag setup drift");expect(equal_arrays(read_item_words(case.clientMember),case.clientItemWordsBefore),"transaction items setup drift")
  tx={active=true,decreaseGoldReturnSeen=false,pendingOrdersIncrementSeen=false,dropItemReturnSeen=false,pickReturnSeen=false,clearFlagReturnSeen=false,prePresentationReturnAddress=nil,record=nil,rngCalls={},rowIndex=nil,selectedItem=nil,orderWriteSeen=false,chronology={}};pending_rng=nil;mode="transaction"
end
local function tx_place_entry()
  if mode~="transaction" then return end
  tx_event("place-entry",t.placeEntryAddress,nil,nil,nil);local case=current_transaction();local frame=frame_address();expect(emu.getregister("M68K A6")&0xFFFFFF==frame,"transaction frame A6 drift")
  tx.record={id=case.id,clientMember=case.clientMember,itemSlot=case.itemSlot,goldBefore=memory.read_u32_be(ram.currentGoldAddress,"M68K BUS"),pendingOrdersBefore=memory.read_u16_be(frame+t.frameOffsetsBytes.pendingOrdersNumber,"M68K BUS"),clientItemWordsBefore=read_item_words(case.clientMember),ordersBefore=read_orders(),flag80OwningByteBefore=read_u8(ram.flag80OwningByteAddress),randomSeedBefore=memory.read_u16_be(ram.randomSeedAddress,"M68K BUS")}
  expect(tx.record.goldBefore==case.goldBefore and tx.record.pendingOrdersBefore==case.pendingOrdersBefore and equal_arrays(tx.record.clientItemWordsBefore,case.clientItemWordsBefore) and equal_arrays(tx.record.ordersBefore,case.ordersBefore) and tx.record.flag80OwningByteBefore==case.flag80OwningByteBefore and tx.record.randomSeedBefore==case.randomSeedBefore,"transaction pre-state readback drift")
end
local function tx_decrease_call() if mode=="transaction" then tx_event("decrease-gold-call",t.decreaseGoldCallAddress,t.decreaseGoldCallAddress,t.decreaseGoldInstructionTargetAddress,t.pendingOrdersIncrementAddress) end end
local function tx_decrease_instruction() if mode=="transaction" then tx_event("decrease-gold-instruction-target",t.decreaseGoldInstructionTargetAddress,t.decreaseGoldCallAddress,t.decreaseGoldInstructionTargetAddress,t.pendingOrdersIncrementAddress) end end
local function tx_decrease_target() if mode=="transaction" then tx_event("decrease-gold-effective-target",t.decreaseGoldEffectiveTargetAddress,t.decreaseGoldCallAddress,t.decreaseGoldEffectiveTargetAddress,t.decreaseGoldEffectiveReturnAddress) end end
local function tx_decrease_return()
  if mode~="transaction" then return end
  tx_event("decrease-gold-effective-return",t.decreaseGoldEffectiveReturnAddress,t.decreaseGoldCallAddress,t.decreaseGoldEffectiveTargetAddress,t.pendingOrdersIncrementAddress);expect(memory.read_u32_be(ram.currentGoldAddress,"M68K BUS")==tx.record.goldBefore-c.orderCost,"DecreaseGold state drift");tx.decreaseGoldReturnSeen=true
end
local function tx_pending_incremented()
  if mode~="transaction" then return end
  tx_event("pending-orders-incremented",t.pendingOrdersIncrementedObserveAddress,nil,nil,nil);expect(tx.decreaseGoldReturnSeen,"pending increment before DecreaseGold return");expect(memory.read_u16_be(frame_address()+t.frameOffsetsBytes.pendingOrdersNumber,"M68K BUS")==tx.record.pendingOrdersBefore+1,"pending order increment state drift");tx.pendingOrdersIncrementSeen=true
end
local function tx_drop_call() if mode=="transaction" then tx_event("drop-item-call",t.dropItemCallAddress,t.dropItemCallAddress,t.dropItemInstructionTargetAddress,t.pickMithrilCallAddress) end end
local function tx_drop_instruction() if mode=="transaction" then tx_event("drop-item-instruction-target",t.dropItemInstructionTargetAddress,t.dropItemCallAddress,t.dropItemInstructionTargetAddress,t.pickMithrilCallAddress) end end
local function tx_drop_target() if mode=="transaction" then tx_event("drop-item-effective-target",t.dropItemEffectiveTargetAddress,t.dropItemCallAddress,t.dropItemEffectiveTargetAddress,t.dropItemTailUpdateTargetAddress) end end
local function tx_drop_update_target() if mode=="transaction" then tx_event("drop-item-tail-update-target",t.dropItemTailUpdateTargetAddress,t.dropItemCallAddress,t.dropItemTailUpdateTargetAddress,t.dropItemEffectiveReturnAddress) end end
local function tx_drop_return()
  if mode~="transaction" then return end
  tx_event("drop-item-effective-return",t.dropItemEffectiveReturnAddress,t.dropItemCallAddress,t.dropItemTailUpdateTargetAddress,t.pickMithrilCallAddress);expect(tx.pendingOrdersIncrementSeen,"DropItemBySlot before pending increment");local before=tx.record.clientItemWordsBefore;local expected={};for index,value in ipairs(before) do if index-1~=tx.record.itemSlot then expected[#expected+1]=value end end;expected[#expected+1]=c.itemNothingIndex;expect(equal_arrays(read_item_words(tx.record.clientMember),expected),"DropItemBySlot state drift");tx.dropItemReturnSeen=true
end
local function tx_pick_call() if mode=="transaction" then tx_event("pick-mithril-call",t.pickMithrilCallAddress,t.pickMithrilCallAddress,f.entryAddress,t.pickMithrilReturnAddress);expect(tx.dropItemReturnSeen,"PickMithrilWeapon before DropItemBySlot return") end end
local function tx_pick_target() if mode=="transaction" then tx_event("pick-mithril-effective-target",f.entryAddress,t.pickMithrilCallAddress,f.entryAddress,f.returnRtsAddress) end end
local function tx_rng_call(role,call_pc,return_pc)
  if mode~="transaction" then return end
  expect(pending_rng==nil,"overlapping transaction RNG call state");set_expectation("transaction",role,call_pc,call_pc,f.rngEntryAddress,return_pc);expect(emu.getregister("M68K PC")==call_pc,"transaction RNG call PC drift");local range_word=word(emu.getregister("M68K D6"));expect(range_word>0,"transaction RNG range must be nonzero");if role=="fallback-row-roll" then expect(range_word==2,"transaction fallback range drift") end;pending_rng={role=role,callPc=call_pc,targetPc=f.rngEntryAddress,returnPc=return_pc,rangeWord=range_word}
end
local function tx_rng_entry() if mode=="transaction" then expect(pending_rng~=nil,"transaction RNG entry without call");set_expectation("transaction","transaction-rng-entry",f.rngEntryAddress,pending_rng.callPc,pending_rng.targetPc,pending_rng.returnPc);expect(emu.getregister("M68K PC")==f.rngEntryAddress,"transaction RNG entry PC drift") end end
local function tx_rng_return() if mode=="transaction" then expect(pending_rng~=nil,"transaction RNG return without call");set_expectation("transaction","transaction-rng-return",f.rngReturnRtsAddress,pending_rng.callPc,pending_rng.targetPc,pending_rng.returnPc);expect(emu.getregister("M68K PC")==f.rngReturnRtsAddress,"transaction RNG RTS PC drift");record_rng_call(tx.rngCalls);pending_rng=nil end end
local function tx_row_resolved() if mode=="transaction" then set_expectation("transaction","transaction-row-resolved",f.rowResolvedAddress,nil,nil,nil);tx.rowIndex=word(emu.getregister("M68K D0"));expect(tx.rowIndex<c.weaponRowCount,"transaction row index outside table") end end
local function tx_item_selected() if mode=="transaction" then expect(tx.rowIndex~=nil and pending_rng==nil,"transaction item selection state drift");set_expectation("transaction","transaction-item-selected",f.loadIndexAddress,nil,nil,nil);tx.selectedItem=word(emu.getregister("M68K D1"));expect(#tx.rngCalls>0,"transaction item selection without RNG") end end
local function tx_order_write() if mode=="transaction" then expect(tx.selectedItem~=nil,"transaction order write before item");set_expectation("transaction","transaction-order-write",f.orderWriteAddress,nil,nil,nil);tx.orderWriteSeen=true end end
local function tx_pick_return()
  if mode~="transaction" then return end
  tx_event("pick-mithril-effective-return",f.returnRtsAddress,t.pickMithrilCallAddress,f.entryAddress,t.pickMithrilReturnAddress);expect(pending_rng==nil and tx.rowIndex~=nil and tx.selectedItem~=nil,"transaction picker sequence incomplete");expect(tx.orderWriteSeen,"transaction picker omitted first-empty order write");tx.pickReturnSeen=true
end
local function tx_clear_call() if mode=="transaction" then tx_event("clear-flag-call",t.clearFlagCallAddress,t.clearFlagCallAddress,t.clearFlagInstructionTargetAddress,t.prePresentationReturnAddress);expect(tx.pickReturnSeen,"ClearFlag before picker return") end end
local function tx_clear_instruction() if mode=="transaction" then tx_event("clear-flag-instruction-target",t.clearFlagInstructionTargetAddress,t.clearFlagCallAddress,t.clearFlagInstructionTargetAddress,t.prePresentationReturnAddress) end end
local function tx_clear_target() if mode=="transaction" then tx_event("clear-flag-effective-target",t.clearFlagEffectiveTargetAddress,t.clearFlagCallAddress,t.clearFlagEffectiveTargetAddress,t.clearFlagEffectiveReturnAddress) end end
local function tx_clear_return()
  if mode~="transaction" then return end
  tx_event("clear-flag-pre-presentation-return",t.clearFlagEffectiveReturnAddress,t.clearFlagCallAddress,t.clearFlagEffectiveTargetAddress,t.prePresentationReturnAddress);local stack=emu.getregister("M68K A7")&0xFFFFFF;expect(stack==stack_top-4,"ClearFlag RTS stack pointer drift");local original_return=memory.read_u32_be(stack,"M68K BUS");expect(original_return==t.prePresentationReturnAddress,"ClearFlag safe-return target drift");expect((read_u8(ram.flag80OwningByteAddress)&c.flag80BitMask)==0,"ClearFlag state drift");tx.prePresentationReturnAddress=original_return;memory.write_u32_be(stack,transaction_pc(transaction_index)+20,"M68K BUS");expect(memory.read_u32_be(stack,"M68K BUS")==transaction_pc(transaction_index)+20,"ClearFlag safe-return rewrite drift");tx.clearFlagReturnSeen=true
end
local function finish_transaction_case_complete(index)
  if mode~="transaction" then return end
  expect(index==transaction_index,"transaction result dispatch drift");local case=current_transaction();local result_pc=transaction_pc(index)+20;set_expectation("transaction","transaction-case-result",result_pc,t.clearFlagEffectiveReturnAddress,t.prePresentationReturnAddress,result_pc);expect(emu.getregister("M68K PC")==result_pc,"transaction result PC drift");expect(tx.clearFlagReturnSeen and #tx.chronology==18,"transaction chronology incomplete")
  local after_items,after_orders=read_item_words(case.clientMember),read_orders();local changes={};for position,before in ipairs(tx.record.ordersBefore) do if after_orders[position]~=before then changes[#changes+1]=position-1 end end
  expect(#changes==1,"transaction picker must write exactly one order slot");expect(tx.orderWriteSeen,"transaction order-write callback missing");expect(after_orders[changes[1]+1]==tx.selectedItem,"transaction order item drift")
  local special=case.clientClass==c.brnClass or case.clientClass==c.rdbnClass;local group_index=special and c.classGroupsCounter+1 or tx.rowIndex;local weapon_calls=0;for _,call in ipairs(tx.rngCalls) do if call.role=="weapon-row-roll" then weapon_calls=weapon_calls+1 end end
  transaction_records[#transaction_records+1]={id=case.id,clientMember=case.clientMember,itemSlot=case.itemSlot,goldBefore=tx.record.goldBefore,goldAfter=memory.read_u32_be(ram.currentGoldAddress,"M68K BUS"),pendingOrdersBefore=tx.record.pendingOrdersBefore,pendingOrdersAfter=memory.read_u16_be(frame_address()+t.frameOffsetsBytes.pendingOrdersNumber,"M68K BUS"),clientItemWordsBefore=tx.record.clientItemWordsBefore,clientItemWordsAfter=after_items,ordersBefore=tx.record.ordersBefore,ordersAfter=after_orders,flag80OwningByteBefore=tx.record.flag80OwningByteBefore,flag80OwningByteAfter=read_u8(ram.flag80OwningByteAddress),randomSeedBefore=tx.record.randomSeedBefore,randomSeedAfter=memory.read_u16_be(ram.randomSeedAddress,"M68K BUS"),classGroupIndex=group_index,weaponRowIndex=tx.rowIndex,choiceIndex=weapon_calls-1,itemIndex=tx.selectedItem,orderWriteIndex=changes[1],rngCalls=tx.rngCalls,callbackChronology=tx.chronology,safeExitOriginalReturnPc=tx.prePresentationReturnAddress}
  tx.active=false;mode="none";transaction_index=transaction_index+1
  if transaction_index>#config.transactionCases then
    fulfillment_index=1;status("milestone:fulfillment-cases-entered");fulfillment_milestone=true
  end
end
local function fx_event(role,pc,call_pc,target_pc,return_pc)
  set_expectation("fulfillment",role,pc,call_pc,target_pc,return_pc);expect(mode=="fulfillment" and fx.active,"fulfillment callback outside active case");expect(emu.getregister("M68K PC")==pc,"fulfillment callback PC drift: "..role);fx.chronology[#fx.chronology+1]={role=role,pc=pc}
end
local function start_fulfillment_case(index)
  expect(mode=="none" and index==fulfillment_index,"fulfillment case-entry dispatch drift");local case=assert(current_fulfillment(),"fulfillment case table exhausted");local entry=fulfillment_pc(index)
  set_expectation("fulfillment-case-entry","fulfillment-case-entry",entry,entry+14,u.addItemEntryAddress,nil);expect(emu.getregister("M68K PC")==entry,"fulfillment case-entry PC drift")
  write_record(case.clientMember,original_records[case.clientMember]);write_class(case.clientMember,case.recipientClass);write_item_words(case.clientMember,case.clientItemWordsBefore);write_orders(case.ordersBefore)
  local frame=fulfillment_frame_address();memory.write_u16_be(frame+u.frameOffsetsBytes.clientClass,case.recipientClass,"M68K BUS");memory.write_u16_be(frame+u.frameOffsetsBytes.clientMember,case.clientMember,"M68K BUS");memory.write_u16_be(frame+u.frameOffsetsBytes.itemIndex,case.itemIndex,"M68K BUS");memory.write_u16_be(frame+u.frameOffsetsBytes.ordersCounter,case.ordersCounter,"M68K BUS");memory.write_u16_be(frame+u.frameOffsetsBytes.fulfilledOrdersNumber,case.fulfilledOrdersBefore,"M68K BUS")
  expect(read_class(case.clientMember)==case.recipientClass,"fulfillment class setup drift");expect(equal_arrays(read_item_words(case.clientMember),case.clientItemWordsBefore),"fulfillment items setup drift");expect(equal_arrays(read_orders(),case.ordersBefore),"fulfillment orders setup drift");expect(case.ordersCounter>=u.ordersCounterMinimum and case.ordersCounter<=u.ordersCounterMaximum,"fulfillment ordersCounter setup domain drift")
  fx={active=true,addItemReturnSeen=false,orderReadSeen=false,orderClearedSeen=false,fulfilledOrdersIncrementSeen=false,equippabilityCarrySet=nil,originalReturnAddress=nil,record=nil,chronology={}};mode="fulfillment"
end
local function fx_add_call() if mode=="fulfillment" then fx_event("fulfillment-add-item-call",u.addItemCallAddress,u.addItemCallAddress,u.addItemInstructionTargetAddress,u.addItemReturnAddress) end end
local function fx_add_instruction() if mode=="fulfillment" then fx_event("fulfillment-add-item-instruction-target",u.addItemInstructionTargetAddress,u.addItemCallAddress,u.addItemInstructionTargetAddress,u.addItemEffectiveTargetAddress) end end
local function fx_add_target() if mode=="fulfillment" then fx_event("fulfillment-add-item-effective-target",u.addItemEffectiveTargetAddress,u.addItemCallAddress,u.addItemEffectiveTargetAddress,u.addItemEffectiveReturnAddress) end end
local function fx_add_return()
  if mode~="fulfillment" then return end
  fx_event("fulfillment-add-item-effective-return",u.addItemEffectiveReturnAddress,u.addItemCallAddress,u.addItemEffectiveTargetAddress,u.addItemReturnAddress);local case=current_fulfillment();local after=read_item_words(case.clientMember);local empty=nil;for index,value in ipairs(case.clientItemWordsBefore) do if (value&c.itemIndexMask)==c.itemNothingIndex then empty=index-1;break end end;expect(empty~=nil,"fulfillment AddItem full-inventory precondition");expect(word(emu.getregister("M68K D2"))==0,"fulfillment AddItem result code drift");expect(after[empty+1]==(case.itemIndex&c.itemIndexAndBrokenMask),"fulfillment AddItem first-empty write drift");fx.record={id=case.id,clientMember=case.clientMember,recipientClass=case.recipientClass,itemIndex=case.itemIndex,clientItemWordsBefore=case.clientItemWordsBefore,clientItemWordsAfter=after,itemWriteIndex=empty,addItemResultCode=0,ordersBefore=read_orders(),ordersCounter=case.ordersCounter,fulfilledOrdersBefore=case.fulfilledOrdersBefore};fx.addItemReturnSeen=true
end
local function fx_order_read()
  if mode~="fulfillment" then return end
  fx_event("fulfillment-order-read",u.orderReadObserveAddress,nil,nil,nil);local case=current_fulfillment();local selected=c.orderSlotCount-case.ordersCounter;local address=ram.ordersAddress+selected*c.orderSlotSize;local value=memory.read_u16_be(address,"M68K BUS");expect(value==case.itemIndex and word(emu.getregister("M68K D2"))==case.itemIndex,"fulfillment source order read drift");fx.record.selectedOrderIndex=selected;fx.record.sourceOrderWordRead=value;fx.orderReadSeen=true
end
local function fx_order_cleared()
  if mode~="fulfillment" then return end
  fx_event("fulfillment-order-cleared",u.orderClearedObserveAddress,nil,nil,nil);local case=current_fulfillment();local selected=c.orderSlotCount-case.ordersCounter;expect(memory.read_u16_be(ram.ordersAddress+selected*c.orderSlotSize,"M68K BUS")==0,"fulfillment source order clear drift");fx.orderClearedSeen=true
end
local function fx_orders_incremented()
  if mode~="fulfillment" then return end
  fx_event("fulfillment-orders-incremented",u.fulfilledOrdersIncrementedObserveAddress,nil,nil,nil);expect(fx.orderClearedSeen,"fulfillment counter increment before order clear");expect(memory.read_u16_be(fulfillment_frame_address()+u.frameOffsetsBytes.fulfilledOrdersNumber,"M68K BUS")==fx.record.fulfilledOrdersBefore+1,"fulfillment counter increment state drift");fx.fulfilledOrdersIncrementSeen=true
end
local function fx_equippability_call() if mode=="fulfillment" then fx_event("fulfillment-equippability-call",u.equippabilityCallAddress,u.equippabilityCallAddress,u.equippabilityInstructionTargetAddress,u.postEquippabilityReturnAddress);expect(fx.fulfilledOrdersIncrementSeen,"fulfillment equippability before counter increment") end end
local function fx_equippability_instruction() if mode=="fulfillment" then fx_event("fulfillment-equippability-instruction-target",u.equippabilityInstructionTargetAddress,u.equippabilityCallAddress,u.equippabilityInstructionTargetAddress,u.equippabilityEffectiveTargetAddress) end end
local function fx_equippability_target() if mode=="fulfillment" then fx_event("fulfillment-equippability-effective-target",u.equippabilityEffectiveTargetAddress,u.equippabilityCallAddress,u.equippabilityEffectiveTargetAddress,u.equippabilityEffectiveReturnAddress) end end
local function fx_equippability_return()
  if mode~="fulfillment" then return end
  fx_event("fulfillment-equippability-effective-return",u.equippabilityEffectiveReturnAddress,u.equippabilityCallAddress,u.equippabilityEffectiveTargetAddress,u.postEquippabilityReturnAddress);local case=current_fulfillment();local stack=emu.getregister("M68K A7")&0xFFFFFF;expect(stack==stack_top-4,"fulfillment equippability RTS stack pointer drift");local original_return=memory.read_u32_be(stack,"M68K BUS");expect(original_return==u.postEquippabilityReturnAddress,"fulfillment safe-return target drift");local carry=(emu.getregister("M68K SR")&1)~=0;expect(carry==case.equippableCarrySet,"fulfillment equippability carry drift");fx.equippabilityCarrySet=carry;fx.originalReturnAddress=original_return;memory.write_u32_be(stack,fulfillment_pc(fulfillment_index)+20,"M68K BUS");expect(memory.read_u32_be(stack,"M68K BUS")==fulfillment_pc(fulfillment_index)+20,"fulfillment safe-return rewrite drift")
end
local function finish_fulfillment_case(index)
  if mode~="fulfillment" then return end
  expect(index==fulfillment_index,"fulfillment result dispatch drift");local case=current_fulfillment();local result_pc=fulfillment_pc(index)+20;set_expectation("fulfillment","fulfillment-case-result",result_pc,u.equippabilityEffectiveReturnAddress,u.postEquippabilityReturnAddress,result_pc);expect(emu.getregister("M68K PC")==result_pc,"fulfillment result PC drift");expect(fx.addItemReturnSeen and fx.orderReadSeen and fx.orderClearedSeen and fx.fulfilledOrdersIncrementSeen and fx.equippabilityCarrySet~=nil and #fx.chronology==11,"fulfillment chronology incomplete");local orders_after=read_orders();expect(fx.record.ordersBefore[fx.record.selectedOrderIndex+1]==case.itemIndex and orders_after[fx.record.selectedOrderIndex+1]==0,"fulfillment selected order read/zero mismatch");fx.record.ordersAfter=orders_after;fx.record.fulfilledOrdersAfter=memory.read_u16_be(fulfillment_frame_address()+u.frameOffsetsBytes.fulfilledOrdersNumber,"M68K BUS");fx.record.equippableCarrySet=fx.equippabilityCarrySet;fx.record.callbackChronology=fx.chronology;fx.record.safeExitOriginalReturnPc=fx.originalReturnAddress;fulfillment_records[#fulfillment_records+1]=fx.record;fx.active=false;mode="none";fulfillment_index=fulfillment_index+1
  if fulfillment_index>#config.fulfillmentCases then precommit_index=1;pcx.transition={active=true,frameCount=0,frameBudget=config.precommitTransitionFrameBudget};precommit_milestone=true;status("milestone:precommit-cases-entered") end
end
local function pcx_event(role,address,call_pc,target_pc,return_pc)
  set_expectation("precommit",role,address,call_pc,target_pc,return_pc);expect(mode=="precommit" and pcx.active,"precommit callback outside active case");expect(emu.getregister("M68K PC")==address,"precommit callback PC drift: "..role);pcx.chronology[#pcx.chronology+1]={role=role,pc=address}
end
local function pcx_attempt()
  local case=assert(current_precommit(),"precommit case table exhausted");return assert(case.attempts[pcx.attemptIndex],"precommit selection attempt exhausted")
end
local pcx_terminal
local function pcx_selection_loop()
  if mode~="precommit" then return end
  pcx_event("precommit-selection-loop-entry",p.runtimeStartAddress,nil,nil,nil)
end
local function pcx_service_call(role,spec,event_role)
  if mode~="precommit" then return end
  local shim=precommit_shim(role,spec);pcx_event(event_role,spec.callAddress,spec.callAddress,shim.generatedStubTarget,spec.returnAddress);expect(pcx.pendingService==nil,"overlapping precommit service call")
  local attempt=pcx_attempt();if role~="member-list" then expect(role~="held-items" or attempt.heldItemsCountResult~=nil,"precommit held-items fixture value missing");expect(role~="equipment-type" or attempt.equipmentTypeResult~=nil,"precommit equipment-type fixture value missing");expect(role~="equippability" or attempt.equippableCarrySetResult~=nil,"precommit equippability fixture value missing") end
  validate_precommit_service_call(shim);write_precommit_service_stub(role,attempt);pcx.pendingService={role=role,callPc=spec.callAddress,targetPc=shim.generatedStubTarget,returnPc=spec.returnAddress}
  if role=="member-list" then pcx.memberListCallCount=pcx.memberListCallCount+1 elseif role=="held-items" then pcx.heldItemsCallCount=pcx.heldItemsCallCount+1 elseif role=="equipment-type" then pcx.equipmentTypeCallCount=pcx.equipmentTypeCallCount+1 elseif role=="equippability" then pcx.equippabilityCallCount=pcx.equippabilityCallCount+1 else error("precommit service shim role drift: "..role) end
end
function pcx.generated_service_stub()
  if mode~="precommit" then return end
  local pending=assert(pcx.pendingService,"precommit generated service stub without source call");pcx_event("precommit-generated-service-stub",precommit_state.serviceStub,pending.callPc,pending.targetPc,pending.returnPc)
end
local function pcx_controlled_return(role,spec,return_role)
  if mode~="precommit" then return end
  local pending=assert(pcx.pendingService,"precommit generated return without source call");expect(pending.role==role and pending.callPc==spec.callAddress and pending.targetPc==precommit_state.serviceStub and pending.returnPc==spec.returnAddress,"precommit generated return ABI drift: "..role)
  pcx_event(return_role,spec.returnAddress,spec.callAddress,pending.targetPc,spec.returnAddress);pcx.pendingService=nil
end
local function pcx_member_call() pcx_service_call("member-list",p.memberList,"precommit-member-list-controlled-service-call-shim") end
local function pcx_member_return() pcx_controlled_return("member-list",p.memberList,"precommit-member-list-original-return") end
local function pcx_member_compare()
  if mode~="precommit" then return end
  pcx_event("precommit-member-cancel-compare",p.memberCancelCompareAddress,p.memberList.callAddress,p.memberList.instructionTargetAddress,p.memberList.returnAddress);local attempt=pcx_attempt();expect(word(emu.getregister("M68K D0"))==word(attempt.selectedMemberResult),"precommit member-list controlled return drift");if attempt.selectedMemberResult~=-1 then pcx.selectedMember=attempt.selectedMemberResult end
end
local function pcx_member_branch()
  if mode~="precommit" then return end
  pcx_event("precommit-member-cancel-branch",p.memberCancelBranchAddress,nil,nil,nil)
end
local function pcx_held_call() pcx_service_call("held-items",p.heldItems,"precommit-held-items-controlled-service-call-shim") end
local function pcx_held_return() pcx_controlled_return("held-items",p.heldItems,"precommit-held-items-original-return") end
local function pcx_capacity_compare()
  if mode~="precommit" then return end
  pcx_event("precommit-capacity-compare",p.capacityCompareAddress,p.heldItems.callAddress,p.heldItems.instructionTargetAddress,p.heldItems.returnAddress);local attempt=pcx_attempt();expect(attempt.heldItemsCountResult~=nil and word(emu.getregister("M68K D2"))==attempt.heldItemsCountResult,"precommit held-items controlled return drift")
end
local function pcx_capacity_branch()
  if mode~="precommit" then return end
  pcx_event("precommit-capacity-branch",p.capacityBranchAddress,nil,nil,nil)
end
local function pcx_equipment_type_call() pcx_service_call("equipment-type",p.equipmentType,"precommit-equipment-type-controlled-service-call-shim") end
local function pcx_equipment_type_return() pcx_controlled_return("equipment-type",p.equipmentType,"precommit-equipment-type-original-return") end
local function pcx_equipment_type_compare()
  if mode~="precommit" then return end
  pcx_event("precommit-equipment-type-compare",p.equipmentTypeCompareAddress,p.equipmentType.callAddress,p.equipmentType.instructionTargetAddress,p.equipmentType.returnAddress);local attempt=pcx_attempt();expect(attempt.equipmentTypeResult~=nil and word(emu.getregister("M68K D2"))==attempt.equipmentTypeResult,"precommit equipment-type controlled return drift")
end
local function pcx_tool_branch()
  if mode~="precommit" then return end
  pcx_event("precommit-tool-admission-branch",p.toolAdmissionBranchAddress,nil,nil,nil)
end
local function pcx_equippability_call() pcx_service_call("equippability",p.equippability,"precommit-equippability-controlled-service-call-shim") end
local function pcx_equippability_return() pcx_controlled_return("equippability",p.equippability,"precommit-equippability-original-return") end
local function pcx_equippability_branch()
  if mode~="precommit" then return end
  pcx_event("precommit-equippability-branch",p.equippabilityBranchAddress,p.equippability.callAddress,p.equippability.instructionTargetAddress,p.equippability.returnAddress);local attempt=pcx_attempt();expect(attempt.equippableCarrySetResult~=nil and ((emu.getregister("M68K SR")&1)~=0)==attempt.equippableCarrySetResult,"precommit equippability carry drift")
end
pcx_terminal=function(role,terminal,address)
  if mode~="precommit" then return end
  local shim=precommit_terminal_shim(role,address);validate_precommit_terminal_boundary(shim);local case=current_precommit();expect(pcx.pendingService==nil,"precommit terminal with pending service");expect(terminal==pcx.expectedTerminal,"precommit terminal outcome drift");local orders=read_orders();local fulfilled=memory.read_u16_be(fulfillment_frame_address()+p.frameOffsetsBytes.fulfilledOrdersNumber,"M68K BUS");expect(equal_arrays(orders,case.ordersBefore),"precommit terminal order mutation before boundary");expect(fulfilled==case.fulfilledOrdersBefore,"precommit terminal fulfilled-order mutation before boundary");pcx.terminal=terminal;pcx.record={id=case.id,itemIndex=case.itemIndex,attemptCount=pcx.attemptIndex,selectedMember=pcx.selectedMember,ordersBefore=case.ordersBefore,ordersAfter=orders,fulfilledOrdersBefore=case.fulfilledOrdersBefore,fulfilledOrdersAfter=fulfilled,terminal=terminal,terminalPc=address,callbackChronology=pcx.chronology};write_precommit_result_stub(precommit_pc(precommit_index)+20)
end
function pcx.terminal_boundary(role,terminal,address)
  if mode~="precommit" then return end
  pcx_event(pcx.terminal_event_role(role),address,nil,precommit_state.terminalStub,nil);pcx_terminal(role,terminal,address)
end
function pcx.terminal_event_role(role)
  if role=="recipient-cancel-terminal-boundary-shim" then return "precommit-recipient-cancel-terminal-boundary-shim" end
  if role=="full-inventory-terminal-boundary-shim" then return "precommit-full-inventory-terminal-boundary-shim" end
  if role=="non-equippable-terminal-boundary-shim" then return "precommit-non-equippable-terminal-boundary-shim" end
  error("precommit terminal event role drift: "..role)
end
function pcx.cleanup_event(role,address,call_pc,target_pc,return_pc)
  if mode~="precommit-cleanup" then return end
  set_expectation("precommit-cleanup",role,address,call_pc,target_pc,return_pc);expect(pcx.cleanup.active,"precommit cleanup callback outside active cleanup");expect(emu.getregister("M68K PC")==address,"precommit cleanup callback PC drift: "..role)
end
function pcx.add_item_boundary()
  if mode~="precommit" then return end
  pcx_event("precommit-add-item-boundary",p.addItemEntryAddress,nil,nil,nil);local case=current_precommit();local cleanup=pcx.cleanup_case();expect(pcx.pendingService==nil and pcx.expectedTerminal=="add-item","precommit AddItem boundary state drift");expect(cleanup.itemIndex==case.itemIndex,"precommit cleanup item identity drift");local selected=c.orderSlotCount-cleanup.ordersCounter;expect(case.ordersBefore[selected+1]==case.itemIndex,"precommit cleanup source order-word drift");local orders=read_orders();local fulfilled=memory.read_u16_be(fulfillment_frame_address()+p.frameOffsetsBytes.fulfilledOrdersNumber,"M68K BUS");expect(equal_arrays(orders,case.ordersBefore) and fulfilled==case.fulfilledOrdersBefore,"precommit AddItem boundary mutation drift");pcx.terminal="add-item";pcx.record={id=case.id,itemIndex=case.itemIndex,attemptCount=pcx.attemptIndex,selectedMember=pcx.selectedMember,ordersBefore=case.ordersBefore,ordersAfter=orders,fulfilledOrdersBefore=case.fulfilledOrdersBefore,fulfilledOrdersAfter=fulfilled,terminal="add-item",terminalPc=p.addItemEntryAddress,callbackChronology=pcx.chronology}
  write_record(cleanup.clientMember,original_records[cleanup.clientMember]);write_class(cleanup.clientMember,cleanup.recipientClass);write_item_words(cleanup.clientMember,cleanup.clientItemWordsBefore);write_orders(cleanup.ordersBefore);local frame=fulfillment_frame_address();memory.write_u16_be(frame+u.frameOffsetsBytes.clientClass,cleanup.recipientClass,"M68K BUS");memory.write_u16_be(frame+u.frameOffsetsBytes.clientMember,cleanup.clientMember,"M68K BUS");memory.write_u16_be(frame+u.frameOffsetsBytes.itemIndex,cleanup.itemIndex,"M68K BUS");memory.write_u16_be(frame+u.frameOffsetsBytes.ordersCounter,cleanup.ordersCounter,"M68K BUS");memory.write_u16_be(frame+u.frameOffsetsBytes.fulfilledOrdersNumber,cleanup.fulfilledOrdersBefore,"M68K BUS");expect(read_class(cleanup.clientMember)==cleanup.recipientClass and equal_arrays(read_item_words(cleanup.clientMember),cleanup.clientItemWordsBefore) and equal_arrays(read_orders(),cleanup.ordersBefore),"precommit cleanup source state setup drift")
  pcx.cleanup={active=true,case=cleanup,addItemReturnSeen=false,orderReadSeen=false,orderClearedSeen=false,fulfilledOrdersIncrementSeen=false,equippabilityCarrySet=nil};mode="precommit-cleanup"
end
function pcx.cleanup_add_call() if mode=="precommit-cleanup" then pcx.cleanup_event("precommit-cleanup-add-item-call",u.addItemCallAddress,u.addItemCallAddress,u.addItemInstructionTargetAddress,u.addItemReturnAddress) end end
function pcx.cleanup_add_instruction() if mode=="precommit-cleanup" then pcx.cleanup_event("precommit-cleanup-add-item-instruction-target",u.addItemInstructionTargetAddress,u.addItemCallAddress,u.addItemInstructionTargetAddress,u.addItemEffectiveTargetAddress) end end
function pcx.cleanup_add_target() if mode=="precommit-cleanup" then pcx.cleanup_event("precommit-cleanup-add-item-effective-target",u.addItemEffectiveTargetAddress,u.addItemCallAddress,u.addItemEffectiveTargetAddress,u.addItemEffectiveReturnAddress) end end
function pcx.cleanup_add_return()
  if mode~="precommit-cleanup" then return end
  pcx.cleanup_event("precommit-cleanup-add-item-effective-return",u.addItemEffectiveReturnAddress,u.addItemCallAddress,u.addItemEffectiveTargetAddress,u.addItemReturnAddress);local cleanup=pcx.cleanup.case;local after=read_item_words(cleanup.clientMember);local empty=nil;for index,value in ipairs(cleanup.clientItemWordsBefore) do if (value&c.itemIndexMask)==c.itemNothingIndex then empty=index-1;break end end;expect(empty~=nil and word(emu.getregister("M68K D2"))==0 and after[empty+1]==(cleanup.itemIndex&c.itemIndexAndBrokenMask),"precommit cleanup AddItem source result drift");pcx.cleanup.addItemReturnSeen=true
end
function pcx.cleanup_order_read()
  if mode~="precommit-cleanup" then return end
  pcx.cleanup_event("precommit-cleanup-order-read",u.orderReadObserveAddress,nil,nil,nil);local cleanup=pcx.cleanup.case;local selected=c.orderSlotCount-cleanup.ordersCounter;expect(memory.read_u16_be(ram.ordersAddress+selected*c.orderSlotSize,"M68K BUS")==cleanup.itemIndex and word(emu.getregister("M68K D2"))==cleanup.itemIndex,"precommit cleanup source order read drift");pcx.cleanup.orderReadSeen=true
end
function pcx.cleanup_order_cleared()
  if mode~="precommit-cleanup" then return end
  pcx.cleanup_event("precommit-cleanup-order-cleared",u.orderClearedObserveAddress,nil,nil,nil);local cleanup=pcx.cleanup.case;local selected=c.orderSlotCount-cleanup.ordersCounter;expect(memory.read_u16_be(ram.ordersAddress+selected*c.orderSlotSize,"M68K BUS")==0,"precommit cleanup source order clear drift");pcx.cleanup.orderClearedSeen=true
end
function pcx.cleanup_orders_incremented()
  if mode~="precommit-cleanup" then return end
  pcx.cleanup_event("precommit-cleanup-orders-incremented",u.fulfilledOrdersIncrementedObserveAddress,nil,nil,nil);local cleanup=pcx.cleanup.case;expect(pcx.cleanup.orderClearedSeen and memory.read_u16_be(fulfillment_frame_address()+u.frameOffsetsBytes.fulfilledOrdersNumber,"M68K BUS")==cleanup.fulfilledOrdersBefore+1,"precommit cleanup source fulfilled-order increment drift");pcx.cleanup.fulfilledOrdersIncrementSeen=true
end
function pcx.cleanup_equippability_call() if mode=="precommit-cleanup" then local q=p.cleanupEquippability;pcx.cleanup_event("precommit-cleanup-equippability-call",q.callAddress,q.callAddress,q.instructionTargetAddress,q.returnAddress);expect(pcx.cleanup.fulfilledOrdersIncrementSeen,"precommit cleanup equippability before order increment") end end
function pcx.cleanup_equippability_instruction() if mode=="precommit-cleanup" then local q=p.cleanupEquippability;pcx.cleanup_event("precommit-cleanup-equippability-instruction-target",q.instructionTargetAddress,q.callAddress,q.instructionTargetAddress,q.effectiveTargetAddress) end end
function pcx.cleanup_equippability_target() if mode=="precommit-cleanup" then local q=p.cleanupEquippability;pcx.cleanup_event("precommit-cleanup-equippability-effective-target",q.effectiveTargetAddress,q.callAddress,q.effectiveTargetAddress,q.effectiveReturnAddress) end end
function pcx.cleanup_equippability_return()
  if mode~="precommit-cleanup" then return end
  local q=p.cleanupEquippability;pcx.cleanup_event("precommit-cleanup-equippability-effective-return",q.effectiveReturnAddress,q.callAddress,q.effectiveTargetAddress,q.returnAddress);local cleanup=pcx.cleanup.case;local stack=emu.getregister("M68K A7")&0xFFFFFF;local expected_top=stack_top-config.precommitCleanupStackDepthBytes;local actual_return=memory.read_u32_be(stack,"M68K BUS");pcx.cleanupStackDiagnostic={expectedTop=expected_top,actualTop=stack,expectedReturn=q.returnAddress,actualReturn=actual_return};expect(stack==expected_top and actual_return==q.returnAddress,"precommit cleanup safe-return stack relation drift");local carry=(emu.getregister("M68K SR")&1)~=0;expect(carry==cleanup.equippableCarrySet,"precommit cleanup equippability carry drift");local target=precommit_pc(precommit_index)+20;memory.write_u32_be(stack,target,"M68K BUS");expect(memory.read_u32_be(stack,"M68K BUS")==target,"precommit cleanup safe-return rewrite drift");pcx.cleanup.equippabilityCarrySet=carry;pcx.cleanup.active=false;mode="precommit"
end
function pcx.generated_result_stub()
  if mode~="precommit" then return end
  local target=precommit_pc(precommit_index)+20;pcx_event("precommit-generated-result-stub",precommit_state.terminalStub,nil,target,nil);expect(memory.read_u16_be(precommit_state.terminalStub,"M68K BUS")==0x4EF9 and memory.read_u32_be(precommit_state.terminalStub+2,"M68K BUS")==target,"precommit generated result stub readback drift")
end
local function start_precommit_case(index)
  expect(mode=="none" and index==precommit_index,"precommit case-entry dispatch drift");if index==1 then expect(pcx.transition.active,"precommit first-case transition was not armed");pcx.transition.active=false else expect(not pcx.transition.active,"precommit transition leaked beyond first case") end;local case=assert(current_precommit(),"precommit case table exhausted");local entry=precommit_pc(index);set_expectation("precommit-case-entry","precommit-case-entry",entry,entry+14,p.runtimeStartAddress,nil);expect(emu.getregister("M68K PC")==entry,"precommit case-entry PC drift");write_orders(case.ordersBefore);local frame=fulfillment_frame_address();memory.write_u16_be(frame+p.frameOffsetsBytes.itemIndex,case.itemIndex,"M68K BUS");memory.write_u16_be(frame+p.frameOffsetsBytes.fulfilledOrdersNumber,case.fulfilledOrdersBefore,"M68K BUS");expect(equal_arrays(read_orders(),case.ordersBefore),"precommit orders setup drift");expect(memory.read_u16_be(frame+p.frameOffsetsBytes.fulfilledOrdersNumber,"M68K BUS")==case.fulfilledOrdersBefore,"precommit fulfilled-orders setup drift");expect(config.precommitCleanupStackDepthBytes==8,"precommit cleanup stack-depth contract drift");pcx.cleanupStackDiagnostic=nil;pcx.active=true;pcx.attemptIndex=1;pcx.memberListCallCount=0;pcx.heldItemsCallCount=0;pcx.equipmentTypeCallCount=0;pcx.equippabilityCallCount=0;pcx.pendingService=nil;pcx.selectedMember=nil;pcx.terminal="none";pcx.expectedTerminal=case.attempts[1].selectedMemberResult==-1 and "recipient-cancel-pre-presentation" or case.attempts[1].heldItemsCountResult>=c.combatantItemSlotCount and "full-inventory-pre-presentation" or case.attempts[1].equipmentTypeResult==c.equipmentTypeTool and "add-item" or case.attempts[1].equippableCarrySetResult and "add-item" or "non-equippable-pre-presentation";pcx.frameCount=0;pcx.frameBudget=config.precommitCaseFrameBudget;pcx.record=nil;pcx.chronology={};pcx.cleanup={active=false,case=nil,addItemReturnSeen=false,orderReadSeen=false,orderClearedSeen=false,fulfilledOrdersIncrementSeen=false,equippabilityCarrySet=nil};mode="precommit"
end
local function finish_precommit_case(index)
  if mode~="precommit" then return end
  expect(index==precommit_index,"precommit result dispatch drift");local result_pc=precommit_pc(index)+20;set_expectation("precommit","precommit-case-result",result_pc,nil,pcx.record and pcx.record.terminalPc or nil,result_pc);expect(emu.getregister("M68K PC")==result_pc,"precommit result PC drift");expect(pcx.record~=nil and pcx.terminal~="none","precommit result without terminal");precommit_records[#precommit_records+1]=pcx.record;pcx.active=false;mode="none";precommit_index=precommit_index+1
  if precommit_index>#config.precommitCases then expect(restore_all(),"exact blacksmith restoration readback drift");status("milestone:transaction-state-restored");cleanup_session();expect(#event_ids==0,"residual registered callback");write_output();status("milestone:callbacks-cleared:0");status("milestone:observer-finished");client.exitCode(0) end
end
local function bootstrap_check_sram()
  if bootstrapped or mode~="none" then return end
  set_expectation("bootstrap-return-redirect","bootstrap-return-redirect",f.checkSramAddress,nil,f.checkSramAddress,probe_base);local stack=emu.getregister("M68K A7")&0xFFFFFF;expect(stack>=0xFF0000 and stack<=0xFFFFFF,"CheckSram return stack outside work RAM")
  snapshot_exact_boundary();memory.write_u32_be(stack,probe_base,"M68K BUS");expect(memory.read_u32_be(stack,"M68K BUS")==probe_base,"CheckSram return redirect write drift");write_probe();helper_index=1;bootstrapped=true;status("milestone:direct-function-probe")
end
local function dispatch(address,entry)
  if entry.role=="bootstrap-check-sram" then bootstrap_check_sram()
  elseif entry.role=="case-entry" then start_helper_case(entry.index)
  elseif entry.role=="function-entry" then helper_entry()
  elseif entry.role=="fallback-row-roll" then helper_rng_call(entry.role,f.fallbackRngCallAddress,f.fallbackRngReturnAddress)
  elseif entry.role=="weapon-row-roll" then helper_rng_call(entry.role,f.weaponRngCallAddress,f.weaponRngReturnAddress)
  elseif entry.role=="rng-entry" then helper_rng_entry()
  elseif entry.role=="rng-return" then helper_rng_return()
  elseif entry.role=="row-resolved" then helper_row_resolved()
  elseif entry.role=="item-selected" then helper_item_selected()
  elseif entry.role=="order-write" then helper_order_write()
  elseif entry.role=="function-rts" then helper_function_rts()
  elseif entry.role=="case-result" then finish_helper_case(entry.index)
  elseif entry.role=="transaction-case-entry" then start_transaction_case(entry.index)
  elseif entry.role=="place-entry" then tx_place_entry()
  elseif entry.role=="decrease-gold-call" then tx_decrease_call()
  elseif entry.role=="decrease-gold-instruction-target" then tx_decrease_instruction()
  elseif entry.role=="decrease-gold-effective-target" then tx_decrease_target()
  elseif entry.role=="decrease-gold-effective-return" then tx_decrease_return()
  elseif entry.role=="pending-orders-incremented" then tx_pending_incremented()
  elseif entry.role=="drop-item-call" then tx_drop_call()
  elseif entry.role=="drop-item-instruction-target" then tx_drop_instruction()
  elseif entry.role=="drop-item-effective-target" then tx_drop_target()
  elseif entry.role=="drop-item-tail-update-target" then tx_drop_update_target()
  elseif entry.role=="drop-item-effective-return" then tx_drop_return()
  elseif entry.role=="pick-mithril-call" then tx_pick_call()
  elseif entry.role=="pick-mithril-effective-target" then tx_pick_target()
  elseif entry.role=="transaction-fallback-row-roll" then tx_rng_call("fallback-row-roll",f.fallbackRngCallAddress,f.fallbackRngReturnAddress)
  elseif entry.role=="transaction-weapon-row-roll" then tx_rng_call("weapon-row-roll",f.weaponRngCallAddress,f.weaponRngReturnAddress)
  elseif entry.role=="transaction-rng-entry" then tx_rng_entry()
  elseif entry.role=="transaction-rng-return" then tx_rng_return()
  elseif entry.role=="transaction-row-resolved" then tx_row_resolved()
  elseif entry.role=="transaction-item-selected" then tx_item_selected()
  elseif entry.role=="transaction-order-write" then tx_order_write()
  elseif entry.role=="pick-mithril-effective-return" then tx_pick_return()
  elseif entry.role=="clear-flag-call" then tx_clear_call()
  elseif entry.role=="clear-flag-instruction-target" then tx_clear_instruction()
  elseif entry.role=="clear-flag-effective-target" then tx_clear_target()
  elseif entry.role=="clear-flag-pre-presentation-return" then tx_clear_return()
  elseif entry.role=="transaction-case-result" then finish_transaction_case_complete(entry.index)
  elseif entry.role=="fulfillment-case-entry" then start_fulfillment_case(entry.index)
  elseif entry.role=="fulfillment-add-item-call" then fx_add_call()
  elseif entry.role=="fulfillment-add-item-instruction-target" then fx_add_instruction()
  elseif entry.role=="fulfillment-add-item-effective-target" then fx_add_target()
  elseif entry.role=="fulfillment-add-item-effective-return" then fx_add_return()
  elseif entry.role=="fulfillment-order-read" then fx_order_read()
  elseif entry.role=="fulfillment-order-cleared" then fx_order_cleared()
  elseif entry.role=="fulfillment-orders-incremented" then fx_orders_incremented()
  elseif entry.role=="fulfillment-equippability-call" then fx_equippability_call()
  elseif entry.role=="fulfillment-equippability-instruction-target" then fx_equippability_instruction()
  elseif entry.role=="fulfillment-equippability-effective-target" then fx_equippability_target()
  elseif entry.role=="fulfillment-equippability-effective-return" then fx_equippability_return()
  elseif entry.role=="fulfillment-case-result" then finish_fulfillment_case(entry.index)
  elseif entry.role=="precommit-case-entry" then start_precommit_case(entry.index)
  elseif entry.role=="precommit-selection-loop-entry" then pcx_selection_loop()
  elseif entry.role=="precommit-member-list-controlled-service-call-shim" then pcx_member_call()
  elseif entry.role=="precommit-generated-service-stub" then pcx.generated_service_stub()
  elseif entry.role=="precommit-member-list-original-return" then pcx_member_return()
  elseif entry.role=="precommit-member-cancel-compare" then pcx_member_compare()
  elseif entry.role=="precommit-member-cancel-branch" then pcx_member_branch()
  elseif entry.role=="precommit-recipient-cancel-terminal-boundary-shim" then pcx.terminal_boundary("recipient-cancel-terminal-boundary-shim","recipient-cancel-pre-presentation",address)
  elseif entry.role=="precommit-held-items-controlled-service-call-shim" then pcx_held_call()
  elseif entry.role=="precommit-held-items-original-return" then pcx_held_return()
  elseif entry.role=="precommit-capacity-compare" then pcx_capacity_compare()
  elseif entry.role=="precommit-capacity-branch" then pcx_capacity_branch()
  elseif entry.role=="precommit-full-inventory-terminal-boundary-shim" then pcx.terminal_boundary("full-inventory-terminal-boundary-shim","full-inventory-pre-presentation",address)
  elseif entry.role=="precommit-equipment-type-controlled-service-call-shim" then pcx_equipment_type_call()
  elseif entry.role=="precommit-equipment-type-original-return" then pcx_equipment_type_return()
  elseif entry.role=="precommit-equipment-type-compare" then pcx_equipment_type_compare()
  elseif entry.role=="precommit-tool-admission-branch" then pcx_tool_branch()
  elseif entry.role=="precommit-equippability-controlled-service-call-shim" then pcx_equippability_call()
  elseif entry.role=="precommit-equippability-original-return" then pcx_equippability_return()
  elseif entry.role=="precommit-equippability-branch" then pcx_equippability_branch()
  elseif entry.role=="precommit-add-item-boundary" then pcx.add_item_boundary()
  elseif entry.role=="precommit-cleanup-add-item-call" then pcx.cleanup_add_call()
  elseif entry.role=="precommit-cleanup-add-item-instruction-target" then pcx.cleanup_add_instruction()
  elseif entry.role=="precommit-cleanup-add-item-effective-target" then pcx.cleanup_add_target()
  elseif entry.role=="precommit-cleanup-add-item-effective-return" then pcx.cleanup_add_return()
  elseif entry.role=="precommit-cleanup-order-read" then pcx.cleanup_order_read()
  elseif entry.role=="precommit-cleanup-order-cleared" then pcx.cleanup_order_cleared()
  elseif entry.role=="precommit-cleanup-orders-incremented" then pcx.cleanup_orders_incremented()
  elseif entry.role=="precommit-cleanup-equippability-call" then pcx.cleanup_equippability_call()
  elseif entry.role=="precommit-cleanup-equippability-instruction-target" then pcx.cleanup_equippability_instruction()
  elseif entry.role=="precommit-cleanup-equippability-effective-target" then pcx.cleanup_equippability_target()
  elseif entry.role=="precommit-cleanup-equippability-effective-return" then pcx.cleanup_equippability_return()
  elseif entry.role=="precommit-non-equippable-terminal-boundary-shim" then pcx.terminal_boundary("non-equippable-terminal-boundary-shim","non-equippable-pre-presentation",address)
  elseif entry.role=="precommit-generated-result-stub" then pcx.generated_result_stub()
  elseif entry.role=="precommit-case-result" then finish_precommit_case(entry.index)
  else error("unknown deterministic dispatch role: "..entry.role) end
end
local function register_exec(address,role,index)
  if not callbacks[address] then
    callbacks[address]={};event_ids[#event_ids+1]=event.on_bus_exec(function()
      if observer_failed then return end
      local ok,message=pcall(function() current_pc=address;for _,entry in ipairs(callbacks[address]) do dispatch(address,entry) end end)
      if not ok then fail_callback(message) end
    end,address,"blacksmith-mithril-"..address,"M68K BUS")
  end
  for _,entry in ipairs(callbacks[address]) do if entry.role==role and entry.index==index then return end end
  callbacks[address][#callbacks[address]+1]={role=role,index=index}
end
write_probe=function()
  memory.write_u16_be(probe_base,0x46FC,"M68K BUS");memory.write_u16_be(probe_base+2,0x2700,"M68K BUS");memory.write_u16_be(probe_base+4,0x2C7C,"M68K BUS");memory.write_u32_be(probe_base+6,frame_base-c.clientClassOffset,"M68K BUS");memory.write_u16_be(probe_base+10,0x2E7C,"M68K BUS");memory.write_u32_be(probe_base+12,stack_top,"M68K BUS");memory.write_u16_be(probe_base+16,0x4EF9,"M68K BUS");memory.write_u32_be(probe_base+18,helper_base,"M68K BUS")
  for index,case in ipairs(config.cases) do
    local entry=helper_pc(index);memory.write_u16_be(entry,0x4E71,"M68K BUS");memory.write_u16_be(entry+2,0x303C,"M68K BUS");memory.write_u16_be(entry+4,case.registerSentinels.d0,"M68K BUS");memory.write_u16_be(entry+6,0x3E3C,"M68K BUS");memory.write_u16_be(entry+8,case.registerSentinels.d7,"M68K BUS");memory.write_u16_be(entry+10,0x4EB9,"M68K BUS");memory.write_u32_be(entry+12,f.entryAddress,"M68K BUS");memory.write_u16_be(entry+16,0x4E71,"M68K BUS");memory.write_u16_be(entry+18,0x4EF9,"M68K BUS");memory.write_u32_be(entry+20,index==#config.cases and transaction_base or helper_pc(index+1),"M68K BUS");register_exec(entry,"case-entry",index);register_exec(entry+16,"case-result",index)
  end
  for index,_ in ipairs(config.transactionCases) do
    local entry=transaction_pc(index);memory.write_u16_be(entry,0x4E71,"M68K BUS");memory.write_u16_be(entry+2,0x2C7C,"M68K BUS");memory.write_u32_be(entry+4,frame_address(),"M68K BUS");memory.write_u16_be(entry+8,0x2E7C,"M68K BUS");memory.write_u32_be(entry+10,stack_top,"M68K BUS");memory.write_u16_be(entry+14,0x4EF9,"M68K BUS");memory.write_u32_be(entry+16,t.placeEntryAddress,"M68K BUS");memory.write_u16_be(entry+20,0x4E71,"M68K BUS");memory.write_u16_be(entry+22,0x4EF9,"M68K BUS");memory.write_u32_be(entry+24,index==#config.transactionCases and fulfillment_base or transaction_pc(index+1),"M68K BUS");register_exec(entry,"transaction-case-entry",index);register_exec(entry+20,"transaction-case-result",index)
  end
  for index,_ in ipairs(config.fulfillmentCases) do
    local entry=fulfillment_pc(index);memory.write_u16_be(entry,0x4E71,"M68K BUS");memory.write_u16_be(entry+2,0x2C7C,"M68K BUS");memory.write_u32_be(entry+4,fulfillment_frame_address(),"M68K BUS");memory.write_u16_be(entry+8,0x2E7C,"M68K BUS");memory.write_u32_be(entry+10,stack_top,"M68K BUS");memory.write_u16_be(entry+14,0x4EF9,"M68K BUS");memory.write_u32_be(entry+16,u.addItemEntryAddress,"M68K BUS");memory.write_u16_be(entry+20,0x4E71,"M68K BUS");memory.write_u16_be(entry+22,0x4EF9,"M68K BUS");memory.write_u32_be(entry+24,index==#config.fulfillmentCases and precommit_base or fulfillment_pc(index+1),"M68K BUS");register_exec(entry,"fulfillment-case-entry",index);register_exec(entry+20,"fulfillment-case-result",index)
  end
  for index,_ in ipairs(config.precommitCases) do
    local entry=precommit_pc(index);memory.write_u16_be(entry,0x4E71,"M68K BUS");memory.write_u16_be(entry+2,0x2C7C,"M68K BUS");memory.write_u32_be(entry+4,fulfillment_frame_address(),"M68K BUS");memory.write_u16_be(entry+8,0x2E7C,"M68K BUS");memory.write_u32_be(entry+10,stack_top,"M68K BUS");memory.write_u16_be(entry+14,0x4EB9,"M68K BUS");memory.write_u32_be(entry+16,p.runtimeStartAddress,"M68K BUS");memory.write_u16_be(entry+20,0x4E71,"M68K BUS");memory.write_u16_be(entry+22,0x4EF9,"M68K BUS");memory.write_u32_be(entry+24,index==#config.precommitCases and entry+precommit_stride or precommit_pc(index+1),"M68K BUS");register_exec(entry,"precommit-case-entry",index);register_exec(entry+20,"precommit-case-result",index)
  end
  register_exec(f.entryAddress,"function-entry",0);register_exec(f.entryAddress,"pick-mithril-effective-target",0);register_exec(f.fallbackRngCallAddress,"fallback-row-roll",0);register_exec(f.fallbackRngCallAddress,"transaction-fallback-row-roll",0);register_exec(f.weaponRngCallAddress,"weapon-row-roll",0);register_exec(f.weaponRngCallAddress,"transaction-weapon-row-roll",0);register_exec(f.rngEntryAddress,"rng-entry",0);register_exec(f.rngEntryAddress,"transaction-rng-entry",0);register_exec(f.rngReturnRtsAddress,"rng-return",0);register_exec(f.rngReturnRtsAddress,"transaction-rng-return",0);register_exec(f.rowResolvedAddress,"row-resolved",0);register_exec(f.rowResolvedAddress,"transaction-row-resolved",0);register_exec(f.loadIndexAddress,"item-selected",0);register_exec(f.loadIndexAddress,"transaction-item-selected",0);register_exec(f.orderWriteAddress,"order-write",0);register_exec(f.orderWriteAddress,"transaction-order-write",0);register_exec(f.returnRtsAddress,"function-rts",0);register_exec(f.returnRtsAddress,"pick-mithril-effective-return",0)
  register_exec(t.placeEntryAddress,"place-entry",0);register_exec(t.decreaseGoldCallAddress,"decrease-gold-call",0);register_exec(t.decreaseGoldInstructionTargetAddress,"decrease-gold-instruction-target",0);register_exec(t.decreaseGoldEffectiveTargetAddress,"decrease-gold-effective-target",0);register_exec(t.decreaseGoldEffectiveReturnAddress,"decrease-gold-effective-return",0);register_exec(t.pendingOrdersIncrementedObserveAddress,"pending-orders-incremented",0);register_exec(t.dropItemCallAddress,"drop-item-call",0);register_exec(t.dropItemInstructionTargetAddress,"drop-item-instruction-target",0);register_exec(t.dropItemEffectiveTargetAddress,"drop-item-effective-target",0);register_exec(t.dropItemTailUpdateTargetAddress,"drop-item-tail-update-target",0);register_exec(t.dropItemEffectiveReturnAddress,"drop-item-effective-return",0);register_exec(t.pickMithrilCallAddress,"pick-mithril-call",0);register_exec(t.clearFlagCallAddress,"clear-flag-call",0);register_exec(t.clearFlagInstructionTargetAddress,"clear-flag-instruction-target",0);register_exec(t.clearFlagEffectiveTargetAddress,"clear-flag-effective-target",0);register_exec(t.clearFlagEffectiveReturnAddress,"clear-flag-pre-presentation-return",0)
  register_exec(u.addItemCallAddress,"fulfillment-add-item-call",0);register_exec(u.addItemInstructionTargetAddress,"fulfillment-add-item-instruction-target",0);register_exec(u.addItemEffectiveTargetAddress,"fulfillment-add-item-effective-target",0);register_exec(u.addItemEffectiveReturnAddress,"fulfillment-add-item-effective-return",0);register_exec(u.orderReadObserveAddress,"fulfillment-order-read",0);register_exec(u.orderClearedObserveAddress,"fulfillment-order-cleared",0);register_exec(u.fulfilledOrdersIncrementedObserveAddress,"fulfillment-orders-incremented",0);register_exec(u.equippabilityCallAddress,"fulfillment-equippability-call",0);register_exec(u.equippabilityInstructionTargetAddress,"fulfillment-equippability-instruction-target",0);register_exec(u.equippabilityEffectiveTargetAddress,"fulfillment-equippability-effective-target",0);register_exec(u.equippabilityEffectiveReturnAddress,"fulfillment-equippability-effective-return",0)
  register_exec(p.runtimeStartAddress,"precommit-selection-loop-entry",0);register_exec(p.memberList.callAddress,"precommit-member-list-controlled-service-call-shim",0);register_exec(precommit_state.serviceStub,"precommit-generated-service-stub",0);register_exec(p.memberList.returnAddress,"precommit-member-list-original-return",0);register_exec(p.memberCancelCompareAddress,"precommit-member-cancel-compare",0);register_exec(p.memberCancelBranchAddress,"precommit-member-cancel-branch",0);register_exec(p.heldItems.callAddress,"precommit-held-items-controlled-service-call-shim",0);register_exec(p.heldItems.returnAddress,"precommit-held-items-original-return",0);register_exec(p.capacityCompareAddress,"precommit-capacity-compare",0);register_exec(p.capacityBranchAddress,"precommit-capacity-branch",0);register_exec(p.equipmentType.callAddress,"precommit-equipment-type-controlled-service-call-shim",0);register_exec(p.equipmentType.returnAddress,"precommit-equipment-type-original-return",0);register_exec(p.equipmentTypeCompareAddress,"precommit-equipment-type-compare",0);register_exec(p.toolAdmissionBranchAddress,"precommit-tool-admission-branch",0);register_exec(p.equippability.callAddress,"precommit-equippability-controlled-service-call-shim",0);register_exec(p.equippability.returnAddress,"precommit-equippability-original-return",0);register_exec(p.equippabilityBranchAddress,"precommit-equippability-branch",0);register_exec(p.addItemEntryAddress,"precommit-add-item-boundary",0);for _,shim in ipairs(p.terminalShims) do if shim.role=="recipient-cancel-terminal-boundary-shim" then register_exec(shim.boundaryAddress,"precommit-recipient-cancel-terminal-boundary-shim",0) elseif shim.role=="full-inventory-terminal-boundary-shim" then register_exec(shim.boundaryAddress,"precommit-full-inventory-terminal-boundary-shim",0) elseif shim.role=="non-equippable-terminal-boundary-shim" then register_exec(shim.boundaryAddress,"precommit-non-equippable-terminal-boundary-shim",0) else error("precommit terminal registration role drift") end end;register_exec(precommit_state.terminalStub,"precommit-generated-result-stub",0)
  register_exec(u.addItemCallAddress,"precommit-cleanup-add-item-call",0);register_exec(u.addItemInstructionTargetAddress,"precommit-cleanup-add-item-instruction-target",0);register_exec(u.addItemEffectiveTargetAddress,"precommit-cleanup-add-item-effective-target",0);register_exec(u.addItemEffectiveReturnAddress,"precommit-cleanup-add-item-effective-return",0);register_exec(u.orderReadObserveAddress,"precommit-cleanup-order-read",0);register_exec(u.orderClearedObserveAddress,"precommit-cleanup-order-cleared",0);register_exec(u.fulfilledOrdersIncrementedObserveAddress,"precommit-cleanup-orders-incremented",0);register_exec(u.equippabilityCallAddress,"precommit-cleanup-equippability-call",0);register_exec(u.equippabilityInstructionTargetAddress,"precommit-cleanup-equippability-instruction-target",0);register_exec(u.equippabilityEffectiveTargetAddress,"precommit-cleanup-equippability-effective-target",0);register_exec(u.equippabilityEffectiveReturnAddress,"precommit-cleanup-equippability-effective-return",0)
end
status("milestone:observer-loaded")
local ok,message=pcall(function() register_exec(f.checkSramAddress,"bootstrap-check-sram",0);status("milestone:direct-function-probe-armed") end)
if not ok then fail_callback(message) end
local frames=0
while true do
  frames=frames+1;joypad.set({Start=true},1);joypad.set({},2);emu.frameadvance()
  if pcx.active then
    pcx.frameCount=pcx.frameCount+1
    if pcx.frameCount>pcx.frameBudget then
      set_expectation("precommit-watchdog","precommit-watchdog-timeout",emu.getregister("M68K PC"),pcx.pendingService and pcx.pendingService.callPc or nil,pcx.pendingService and pcx.pendingService.targetPc or nil,pcx.pendingService and pcx.pendingService.returnPc or nil)
      fail_callback("precommit case frame budget exhausted before terminal")
    end
  end
  if pcx.transition.active then
    pcx.transition.frameCount=pcx.transition.frameCount+1
    if pcx.transition.frameCount>pcx.transition.frameBudget then
      set_expectation("precommit-transition","precommit-transition-timeout",precommit_pc(1),nil,precommit_pc(1),nil)
      fail_callback("precommit transition frame budget exhausted before first generated case entry")
    end
  end
  if frames%600==0 then status("frame="..frames..",pc="..string.format("%X",emu.getregister("M68K PC"))) end
end
