-- One grouped direct-ROM probe.  The helper cohort calls PickMithrilWeapon;
-- the transaction cohort jumps to the original post-confirmation PlaceOrder
-- block and rewrites only ClearFlag's verified return-to-text stack word.
local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local f,t,ram,c=config["function"],config.transaction,config.ram,config.constants
local probe_base,helper_base,helper_stride,transaction_base,transaction_stride=0xFF6800,0xFF6820,24,0xFF6900,32
local frame_base,stack_top=0xFF6A00,0xFFFF00
local callbacks,event_ids,helper_records,transaction_records={}, {}, {}, {}
local observer_failed,session_cleaned,bootstrapped=false,false,false
local mode,helper_index,transaction_index="none",0,0
local helper_active,first_case_milestone,transaction_milestone=false,false,false
local original_gold,original_seed,original_orders,original_flag,original_records=nil,nil,nil,nil,nil
local pending_rng,row_index,selected_item,function_return_seen,order_write_seen=nil,nil,nil,false,false
local tx={active=false,decreaseGoldReturnSeen=false,pendingOrdersIncrementSeen=false,dropItemReturnSeen=false,pickReturnSeen=false,clearFlagReturnSeen=false,prePresentationReturnAddress=nil,record=nil,rngCalls=nil,rowIndex=nil,selectedItem=nil,orderWriteSeen=false,chronology=nil}
local current_phase,current_role,current_pc,current_expectation="registration","registration",nil,nil
local write_probe

local function status(value) local file=assert(io.open(config.statusPath,"a"));file:write(value.."\n");file:close() end
local function bool(value) return value and "true" or "false" end
local function nullable(value) return value==nil and "null" or tostring(value) end
local function json_string(value) return string.format("%q",value) end
local function word(value) return value&0xFFFF end
local function current_helper() return config.cases[helper_index] end
local function current_transaction() return config.transactionCases[transaction_index] end
local function helper_pc(index) return helper_base+(index-1)*helper_stride end
local function transaction_pc(index) return transaction_base+(index-1)*transaction_stride end
local function frame_address() return frame_base-t.frameOffsetsBytes.clientClass end
local function read_u8(address) return memory.read_u8(address,"M68K BUS") end
local function write_u8(address,value) memory.write_u8(address,value,"M68K BUS") end
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
local function pending_callback_state()
  local case_for_state=0
  if mode=="helper" or current_role=="case-entry" then case_for_state=helper_index elseif mode=="transaction" or current_role=="transaction-case-entry" then case_for_state=transaction_index end
  return "{\"active\":"..bool(helper_active or tx.active)..",\"caseIndex\":"..case_for_state..",\"functionReturnSeen\":"..bool(function_return_seen)..",\"orderWriteSeen\":"..bool(order_write_seen or tx.orderWriteSeen)..",\"pendingRngCall\":"..pending_rng_json()..",\"rolesAtPc\":"..roles_json(current_pc)..",\"transaction\":"..transaction_state_json().."}"
end
local function restore_all()
  if original_gold==nil then return true end
  memory.write_u32_be(ram.currentGoldAddress,original_gold,"M68K BUS")
  memory.write_u16_be(ram.randomSeedAddress,original_seed,"M68K BUS")
  write_orders(original_orders);write_u8(ram.flag80OwningByteAddress,original_flag)
  for member,bytes in pairs(original_records) do write_record(member,bytes) end
  if memory.read_u32_be(ram.currentGoldAddress,"M68K BUS")~=original_gold then return false end
  if memory.read_u16_be(ram.randomSeedAddress,"M68K BUS")~=original_seed then return false end
  if not equal_arrays(read_orders(),original_orders) then return false end
  if read_u8(ram.flag80OwningByteAddress)~=original_flag then return false end
  for member,bytes in pairs(original_records) do if not equal_arrays(read_record(member),bytes) then return false end end
  return true
end
local function fail_callback(message)
  if observer_failed then return end
  observer_failed=true
  local case=nil
  if mode=="helper" or current_role=="case-entry" then case=current_helper() elseif mode=="transaction" or current_role=="transaction-case-entry" then case=current_transaction() end
  local restored,restore_message=pcall(restore_all)
  local expected=current_expectation or {};local actual=current_role=="registration" and nil or emu.getregister("M68K PC")
  local detail=tostring(message);if not restored then detail=detail.."; restoration error: "..tostring(restore_message) elseif restore_message~=true then detail=detail.."; restoration readback drift" end
  local payload="{\"owner\":"..json_string(config.observerFailureContract.owner)..",\"caseId\":"..(case and json_string(case.id) or "null")..",\"phase\":"..json_string(current_phase)..",\"role\":"..json_string(current_role)..",\"actualPc\":"..nullable(actual)..",\"expectedEventPc\":"..nullable(expected.eventPc)..",\"expectedCallPc\":"..nullable(expected.callPc)..",\"expectedTargetPc\":"..nullable(expected.targetPc)..",\"expectedReturnPc\":"..nullable(expected.returnPc)..",\"pendingCallback\":"..pending_callback_state()..",\"error\":"..json_string(detail).."}"
  os.remove(config.outputPath);cleanup_session();local diagnostic=config.observerFailureContract.statusPrefix..payload;status(diagnostic);print(diagnostic);client.exitCode(config.observerFailureContract.exitCode)
end
local function expect(condition,message) if not condition then error(message) end end
local function set_expectation(phase,role,event_pc,call_pc,target_pc,return_pc)
  current_phase,current_role,current_expectation=phase,role,{eventPc=event_pc,callPc=call_pc,targetPc=target_pc,returnPc=return_pc}
end
local function snapshot_exact_boundary()
  original_gold=memory.read_u32_be(ram.currentGoldAddress,"M68K BUS");original_seed=memory.read_u16_be(ram.randomSeedAddress,"M68K BUS");original_orders=read_orders();original_flag=read_u8(ram.flag80OwningByteAddress);original_records={}
  for _,case in ipairs(config.transactionCases) do
    expect(original_records[case.clientMember]==nil,"transaction cases must select distinct combatant records")
    original_records[case.clientMember]=read_record(case.clientMember)
  end
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
local function write_output()
  local helpers,transactions,helper_order,transaction_order={}, {}, {}, {}
  for _,record in ipairs(helper_records) do helpers[#helpers+1]=helper_record_json(record) end
  for _,record in ipairs(transaction_records) do transactions[#transactions+1]=transaction_record_json(record) end
  for _,id in ipairs(config.caseOrder) do helper_order[#helper_order+1]=json_string(id) end
  for _,id in ipairs(config.transactionCaseOrder) do transaction_order[#transaction_order+1]=json_string(id) end
  local output=assert(io.open(config.outputPath,"w"));output:write("{\"system\":"..json_string(emu.getsystemid())..",\"core\":"..json_string(config.core)..",\"id\":"..json_string(config.id)..",\"caseOrder\":["..table.concat(helper_order,",").."],\"records\":["..table.concat(helpers,",").."],\"transactionCaseOrder\":["..table.concat(transaction_order,",").."],\"transactionRecords\":["..table.concat(transactions,",").."],\"callbacksCleared\":0,\"restoration\":{\"currentGoldLongRestored\":true,\"randomSeedWordRestored\":true,\"orderWordsRestored\":true,\"flag80OwningByteRestored\":true,\"clientCombatantRecordsRestored\":true}}");output:close()
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
    expect(restore_all(),"exact transaction restoration readback drift");status("milestone:transaction-state-restored");cleanup_session();expect(#event_ids==0,"residual registered callback");write_output();status("milestone:callbacks-cleared:0");status("milestone:observer-finished");client.exitCode(0)
  end
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
    local entry=transaction_pc(index);memory.write_u16_be(entry,0x4E71,"M68K BUS");memory.write_u16_be(entry+2,0x2C7C,"M68K BUS");memory.write_u32_be(entry+4,frame_address(),"M68K BUS");memory.write_u16_be(entry+8,0x2E7C,"M68K BUS");memory.write_u32_be(entry+10,stack_top,"M68K BUS");memory.write_u16_be(entry+14,0x4EF9,"M68K BUS");memory.write_u32_be(entry+16,t.placeEntryAddress,"M68K BUS");memory.write_u16_be(entry+20,0x4E71,"M68K BUS");memory.write_u16_be(entry+22,0x4EF9,"M68K BUS");memory.write_u32_be(entry+24,index==#config.transactionCases and entry+transaction_stride or transaction_pc(index+1),"M68K BUS");register_exec(entry,"transaction-case-entry",index);register_exec(entry+20,"transaction-case-result",index)
  end
  register_exec(f.entryAddress,"function-entry",0);register_exec(f.entryAddress,"pick-mithril-effective-target",0);register_exec(f.fallbackRngCallAddress,"fallback-row-roll",0);register_exec(f.fallbackRngCallAddress,"transaction-fallback-row-roll",0);register_exec(f.weaponRngCallAddress,"weapon-row-roll",0);register_exec(f.weaponRngCallAddress,"transaction-weapon-row-roll",0);register_exec(f.rngEntryAddress,"rng-entry",0);register_exec(f.rngEntryAddress,"transaction-rng-entry",0);register_exec(f.rngReturnRtsAddress,"rng-return",0);register_exec(f.rngReturnRtsAddress,"transaction-rng-return",0);register_exec(f.rowResolvedAddress,"row-resolved",0);register_exec(f.rowResolvedAddress,"transaction-row-resolved",0);register_exec(f.loadIndexAddress,"item-selected",0);register_exec(f.loadIndexAddress,"transaction-item-selected",0);register_exec(f.orderWriteAddress,"order-write",0);register_exec(f.orderWriteAddress,"transaction-order-write",0);register_exec(f.returnRtsAddress,"function-rts",0);register_exec(f.returnRtsAddress,"pick-mithril-effective-return",0)
  register_exec(t.placeEntryAddress,"place-entry",0);register_exec(t.decreaseGoldCallAddress,"decrease-gold-call",0);register_exec(t.decreaseGoldInstructionTargetAddress,"decrease-gold-instruction-target",0);register_exec(t.decreaseGoldEffectiveTargetAddress,"decrease-gold-effective-target",0);register_exec(t.decreaseGoldEffectiveReturnAddress,"decrease-gold-effective-return",0);register_exec(t.pendingOrdersIncrementedObserveAddress,"pending-orders-incremented",0);register_exec(t.dropItemCallAddress,"drop-item-call",0);register_exec(t.dropItemInstructionTargetAddress,"drop-item-instruction-target",0);register_exec(t.dropItemEffectiveTargetAddress,"drop-item-effective-target",0);register_exec(t.dropItemTailUpdateTargetAddress,"drop-item-tail-update-target",0);register_exec(t.dropItemEffectiveReturnAddress,"drop-item-effective-return",0);register_exec(t.pickMithrilCallAddress,"pick-mithril-call",0);register_exec(t.clearFlagCallAddress,"clear-flag-call",0);register_exec(t.clearFlagInstructionTargetAddress,"clear-flag-instruction-target",0);register_exec(t.clearFlagEffectiveTargetAddress,"clear-flag-effective-target",0);register_exec(t.clearFlagEffectiveReturnAddress,"clear-flag-pre-presentation-return",0)
end
status("milestone:observer-loaded")
local ok,message=pcall(function() register_exec(f.checkSramAddress,"bootstrap-check-sram",0);status("milestone:direct-function-probe-armed") end)
if not ok then fail_callback(message) end
local frames=0
while true do frames=frames+1;joypad.set({Start=true},1);joypad.set({},2);emu.frameadvance();if frames%600==0 then status("frame="..frames..",pc="..string.format("%X",emu.getregister("M68K PC"))) end end
