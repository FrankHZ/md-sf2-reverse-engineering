-- One generated direct-function probe for PickMithrilWeapon.  Its cases are
-- harness definitions; all observed result facts come from original registers
-- and work RAM after the untouched helper returns.
local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local f,ram,c=config["function"],config.ram,config.constants
local probe_base,case_base,probe_stride,frame_base,stack_top=0xFF6800,0xFF6820,24,0xFF6A00,0xFFFF00
local callbacks,event_ids,records={}, {}, {}
local active,case_index,observer_failed,session_cleaned,bootstrapped=false,0,false,false,false
local first_case_milestone=false
local original_seed,original_orders=nil,nil
local pending_rng,row_index,selected_item,function_return_seen,order_write_seen=nil,nil,nil,false,false
local current_phase,current_role,current_pc,current_expectation="registration","registration",nil,nil
local write_probe

local function status(value) local file=assert(io.open(config.statusPath,"a"));file:write(value.."\n");file:close() end
local function bool(value) return value and "true" or "false" end
local function nullable(value) return value==nil and "null" or tostring(value) end
local function json_string(value) return string.format("%q",value) end
local function word(value) return value&0xFFFF end
local function current_case() return config.cases[case_index] end
local function pc_for(index) return case_base+(index-1)*probe_stride end
local function read_orders()
  local orders={};for index=0,c.orderSlotCount-1 do orders[#orders+1]=memory.read_u16_be(ram.ordersAddress+index*c.orderSlotSize,"M68K BUS") end
  return orders
end
local function write_orders(orders)
  for index,value in ipairs(orders) do memory.write_u16_be(ram.ordersAddress+(index-1)*c.orderSlotSize,value,"M68K BUS") end
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
local function pending_callback_state()
  local case_for_state=(active or current_role=="case-entry") and case_index or 0
  return "{\"active\":"..bool(active)..",\"caseIndex\":"..case_for_state..",\"functionReturnSeen\":"..bool(function_return_seen)..",\"orderWriteSeen\":"..bool(order_write_seen)..",\"pendingRngCall\":"..pending_rng_json()..",\"rolesAtPc\":"..roles_json(current_pc).."}"
end
local function restore_seed_and_orders()
  if original_seed==nil or original_orders==nil then return true end
  memory.write_u16_be(ram.randomSeedAddress,original_seed,"M68K BUS");write_orders(original_orders)
  if memory.read_u16_be(ram.randomSeedAddress,"M68K BUS")~=original_seed then return false end
  local restored=read_orders();for index,value in ipairs(original_orders) do if restored[index]~=value then return false end end
  return true
end
local function fail_callback(message)
  if observer_failed then return end
  observer_failed=true
  local case=(active or current_role=="case-entry") and current_case() or nil
  local expected=current_expectation or {};local actual=current_role=="registration" and nil or emu.getregister("M68K PC")
  local payload="{\"owner\":"..json_string(config.observerFailureContract.owner)..",\"caseId\":"..(case and json_string(case.id) or "null")..",\"phase\":"..json_string(current_phase)..",\"role\":"..json_string(current_role)..",\"actualPc\":"..nullable(actual)..",\"expectedEventPc\":"..nullable(expected.eventPc)..",\"expectedCallPc\":"..nullable(expected.callPc)..",\"expectedTargetPc\":"..nullable(expected.targetPc)..",\"expectedReturnPc\":"..nullable(expected.returnPc)..",\"pendingCallback\":"..pending_callback_state()..",\"error\":"..json_string(tostring(message)).."}"
  local diagnostic=config.observerFailureContract.statusPrefix..payload
  status(diagnostic);print(diagnostic);os.remove(config.outputPath);restore_seed_and_orders();cleanup_session();client.exitCode(config.observerFailureContract.exitCode)
end
local function expect(condition,message) if not condition then error(message) end end
local function set_expectation(phase,role,event_pc,call_pc,target_pc,return_pc)
  current_phase,current_role,current_expectation=phase,role,{eventPc=event_pc,callPc=call_pc,targetPc=target_pc,returnPc=return_pc}
end
local function case_expectation(index,event_pc)
  local entry=pc_for(index);return entry,event_pc or entry,entry+10,f.entryAddress,entry+16
end
local function record_rng_call()
  local call=pending_rng
  local case=current_case();case._rngCalls=case._rngCalls or {}
  case._rngCalls[#case._rngCalls+1]={role=call.role,callPc=call.callPc,targetPc=call.targetPc,returnPc=call.returnPc,rangeWord=call.rangeWord,result=word(emu.getregister("M68K D7")),randomSeedAfter=memory.read_u16_be(ram.randomSeedAddress,"M68K BUS")}
end
local function start_case(index)
  expect(not active and index==case_index,"case-entry dispatch drift")
  local case=assert(current_case(),"case table exhausted")
  local entry,event_pc,call_pc,target_pc,return_pc=case_expectation(index)
  set_expectation("case-entry","case-entry",event_pc,call_pc,target_pc,return_pc)
  expect(emu.getregister("M68K PC")==entry,"case-entry PC drift")
  if original_seed==nil then original_seed=memory.read_u16_be(ram.randomSeedAddress,"M68K BUS");original_orders=read_orders() end
  memory.write_u16_be(frame_base,case.clientClass,"M68K BUS")
  memory.write_u16_be(ram.randomSeedAddress,case.randomSeedBefore,"M68K BUS");write_orders(case.ordersBefore)
  case._rngCalls={};row_index=nil;selected_item=nil;function_return_seen=false;order_write_seen=false;pending_rng=nil;active=true
  if not first_case_milestone then status("milestone:first-case-entered");first_case_milestone=true end
end
local function helper_entry()
  local entry,event_pc,call_pc,target_pc,return_pc=case_expectation(case_index,f.entryAddress)
  set_expectation("function-entry","function-entry",event_pc,call_pc,target_pc,return_pc)
  expect(active,"helper entry while no case active");expect(emu.getregister("M68K PC")==f.entryAddress,"helper entry PC drift")
end
local function rng_call(role,call_pc,return_pc)
  expect(active and pending_rng==nil,"overlapping RNG call state")
  set_expectation(role,role,call_pc,call_pc,f.rngEntryAddress,return_pc)
  expect(emu.getregister("M68K PC")==call_pc,"RNG call PC drift")
  local range_word=word(emu.getregister("M68K D6"));expect(range_word>0,"RNG range must be nonzero")
  if role=="fallback-row-roll" then expect(range_word==2,"fallback RNG range drift") end
  pending_rng={role=role,callPc=call_pc,targetPc=f.rngEntryAddress,returnPc=return_pc,rangeWord=range_word}
end
local function rng_entry()
  expect(active and pending_rng~=nil,"RNG entry without helper call")
  set_expectation("rng-entry","rng-entry",f.rngEntryAddress,pending_rng.callPc,pending_rng.targetPc,pending_rng.returnPc)
  expect(emu.getregister("M68K PC")==f.rngEntryAddress,"RNG entry PC drift")
end
local function rng_return()
  expect(active and pending_rng~=nil,"RNG return without pending helper call")
  set_expectation("rng-return","rng-return",f.rngReturnRtsAddress,pending_rng.callPc,pending_rng.targetPc,pending_rng.returnPc)
  expect(emu.getregister("M68K PC")==f.rngReturnRtsAddress,"RNG RTS PC drift")
  record_rng_call();pending_rng=nil
end
local function row_resolved()
  expect(active,"row resolution outside helper")
  set_expectation("row-resolved","row-resolved",f.rowResolvedAddress,nil,nil,nil)
  row_index=word(emu.getregister("M68K D0"));expect(row_index<c.weaponRowCount,"row index outside source table")
end
local function item_selected()
  expect(active and row_index~=nil and pending_rng==nil,"item selection state drift")
  set_expectation("item-selected","item-selected",f.loadIndexAddress,nil,nil,nil)
  selected_item=word(emu.getregister("M68K D1"));expect(#current_case()._rngCalls>0,"item selection without RNG call")
end
local function order_write()
  expect(active and selected_item~=nil,"order write before selected item")
  set_expectation("order-write","order-write",f.orderWriteAddress,nil,nil,nil);order_write_seen=true
end
local function function_rts()
  expect(active and pending_rng==nil,"helper RTS with pending RNG call")
  set_expectation("function-rts","function-rts",f.returnRtsAddress,pc_for(case_index)+10,f.entryAddress,pc_for(case_index)+16)
  expect(emu.getregister("M68K PC")==f.returnRtsAddress,"helper RTS guard PC drift")
  function_return_seen=true
end
local function array_json(values)
  local parts={};for _,value in ipairs(values) do parts[#parts+1]=tostring(value) end;return "["..table.concat(parts,",").."]"
end
local function rng_calls_json(calls)
  local parts={};for _,call in ipairs(calls) do parts[#parts+1]="{\"role\":"..json_string(call.role)..",\"callPc\":"..call.callPc..",\"targetPc\":"..call.targetPc..",\"returnPc\":"..call.returnPc..",\"rangeWord\":"..call.rangeWord..",\"result\":"..call.result..",\"randomSeedAfter\":"..call.randomSeedAfter.."}" end;return "["..table.concat(parts,",").."]"
end
local function record_json(record)
  return "{\"id\":"..json_string(record.id)..",\"classGroupIndex\":"..record.classGroupIndex..",\"weaponRowIndex\":"..record.weaponRowIndex..",\"choiceIndex\":"..record.choiceIndex..",\"itemIndex\":"..record.itemIndex..",\"orderWriteIndex\":"..nullable(record.orderWriteIndex)..",\"ordersAfter\":"..array_json(record.ordersAfter)..",\"randomSeedAfter\":"..record.randomSeedAfter..",\"rngCalls\":"..rng_calls_json(record.rngCalls)..",\"functionReturnSeen\":true,\"preservedD0\":"..record.preservedD0..",\"preservedD7\":"..record.preservedD7.."}"
end
local function write_output()
  local values={};for _,record in ipairs(records) do values[#values+1]=record_json(record) end
  local order={};for _,id in ipairs(config.caseOrder) do order[#order+1]=json_string(id) end
  local output=assert(io.open(config.outputPath,"w"));output:write("{\"system\":"..json_string(emu.getsystemid())..",\"core\":"..json_string(config.core)..",\"id\":"..json_string(config.id)..",\"caseOrder\":["..table.concat(order,",").."],\"records\":["..table.concat(values,",").."],\"callbacksCleared\":0,\"seedAndOrdersRestored\":true}");output:close()
end
local function finish_case(index)
  expect(active and index==case_index,"case-result dispatch drift")
  local case=current_case();local entry,event_pc,call_pc,target_pc,return_pc=case_expectation(index,pc_for(index)+16)
  set_expectation("case-result","case-result",event_pc,call_pc,target_pc,return_pc)
  expect(emu.getregister("M68K PC")==pc_for(index)+16,"helper return target drift")
  expect(function_return_seen and pending_rng==nil and row_index~=nil and selected_item~=nil,"incomplete helper callback sequence")
  local after=read_orders();local differences={}
  for slot,before in ipairs(case.ordersBefore) do if after[slot]~=before then differences[#differences+1]=slot-1 end end
  expect(#differences<=1,"helper wrote more than one order slot")
  expect(order_write_seen==(#differences==1),"order-write callback/RAM result drift")
  if #differences==1 then expect(after[differences[1]+1]==selected_item,"helper order write item drift") end
  local special=case.clientClass==c.brnClass or case.clientClass==c.rdbnClass
  local group_index=special and c.classGroupsCounter+1 or row_index
  local weapon_calls=0;for _,call in ipairs(case._rngCalls) do if call.role=="weapon-row-roll" then weapon_calls=weapon_calls+1 end end
  records[#records+1]={id=case.id,classGroupIndex=group_index,weaponRowIndex=row_index,choiceIndex=weapon_calls-1,itemIndex=selected_item,orderWriteIndex=#differences==1 and differences[1] or nil,ordersAfter=after,randomSeedAfter=memory.read_u16_be(ram.randomSeedAddress,"M68K BUS"),rngCalls=case._rngCalls,preservedD0=word(emu.getregister("M68K D0")),preservedD7=word(emu.getregister("M68K D7"))}
  active=false;case_index=case_index+1
  if case_index>#config.cases then
    local restored=restore_seed_and_orders();expect(restored,"RANDOM_SEED/order restore drift");status("milestone:seed-and-orders-restored")
    cleanup_session();expect(#event_ids==0,"residual registered callback");write_output();status("milestone:callbacks-cleared:0");status("milestone:observer-finished");client.exitCode(0)
  end
end
local function bootstrap_check_sram()
  if bootstrapped or active then return end
  set_expectation("bootstrap-return-redirect","bootstrap-return-redirect",f.checkSramAddress,nil,f.checkSramAddress,probe_base)
  local stack=emu.getregister("M68K A7")&0xFFFFFF;expect(stack>=0xFF0000 and stack<=0xFFFFFF,"CheckSram return stack outside work RAM")
  memory.write_u32_be(stack,probe_base,"M68K BUS");expect(memory.read_u32_be(stack,"M68K BUS")==probe_base,"CheckSram return redirect write drift")
  write_probe();case_index=1;bootstrapped=true;status("milestone:direct-function-probe")
end
local function dispatch(address,entry)
  if entry.role=="bootstrap-check-sram" then bootstrap_check_sram()
  elseif entry.role=="case-entry" then start_case(entry.index)
  elseif entry.role=="function-entry" then helper_entry()
  elseif entry.role=="fallback-row-roll" then rng_call(entry.role,f.fallbackRngCallAddress,f.fallbackRngReturnAddress)
  elseif entry.role=="weapon-row-roll" then rng_call(entry.role,f.weaponRngCallAddress,f.weaponRngReturnAddress)
  elseif entry.role=="rng-entry" then rng_entry()
  elseif entry.role=="rng-return" then rng_return()
  elseif entry.role=="row-resolved" then row_resolved()
  elseif entry.role=="item-selected" then item_selected()
  elseif entry.role=="order-write" then order_write()
  elseif entry.role=="function-rts" then function_rts()
  elseif entry.role=="case-result" then finish_case(entry.index)
  else error("unknown deterministic dispatch role: "..entry.role) end
end
local function register_exec(address,role,index)
  if not callbacks[address] then
    callbacks[address]={}
    event_ids[#event_ids+1]=event.on_bus_exec(function()
      if observer_failed then return end
      local ok,message=pcall(function() current_pc=address;for _,entry in ipairs(callbacks[address]) do dispatch(address,entry) end end)
      if not ok then fail_callback(message) end
    end,address,"blacksmith-mithril-"..address,"M68K BUS")
  end
  for _,entry in ipairs(callbacks[address]) do if entry.role==role and entry.index==index then return end end
  callbacks[address][#callbacks[address]+1]={role=role,index=index}
end
write_probe=function()
  memory.write_u16_be(probe_base,0x46FC,"M68K BUS");memory.write_u16_be(probe_base+2,0x2700,"M68K BUS")
  memory.write_u16_be(probe_base+4,0x2C7C,"M68K BUS");memory.write_u32_be(probe_base+6,frame_base-c.clientClassOffset,"M68K BUS")
  memory.write_u16_be(probe_base+10,0x2E7C,"M68K BUS");memory.write_u32_be(probe_base+12,stack_top,"M68K BUS")
  memory.write_u16_be(probe_base+16,0x4EF9,"M68K BUS");memory.write_u32_be(probe_base+18,case_base,"M68K BUS")
  for index,case in ipairs(config.cases) do
    local entry=pc_for(index);memory.write_u16_be(entry,0x4E71,"M68K BUS");memory.write_u16_be(entry+2,0x303C,"M68K BUS");memory.write_u16_be(entry+4,case.registerSentinels.d0,"M68K BUS");memory.write_u16_be(entry+6,0x3E3C,"M68K BUS");memory.write_u16_be(entry+8,case.registerSentinels.d7,"M68K BUS");memory.write_u16_be(entry+10,0x4EB9,"M68K BUS");memory.write_u32_be(entry+12,f.entryAddress,"M68K BUS");memory.write_u16_be(entry+16,0x4E71,"M68K BUS");memory.write_u16_be(entry+18,0x4EF9,"M68K BUS");memory.write_u32_be(entry+20,index==#config.cases and pc_for(index)+probe_stride or pc_for(index+1),"M68K BUS")
    register_exec(entry,"case-entry",index);register_exec(entry+16,"case-result",index)
  end
  register_exec(f.entryAddress,"function-entry",0);register_exec(f.fallbackRngCallAddress,"fallback-row-roll",0);register_exec(f.weaponRngCallAddress,"weapon-row-roll",0);register_exec(f.rngEntryAddress,"rng-entry",0);register_exec(f.rngReturnRtsAddress,"rng-return",0);register_exec(f.rowResolvedAddress,"row-resolved",0);register_exec(f.loadIndexAddress,"item-selected",0);register_exec(f.orderWriteAddress,"order-write",0);register_exec(f.returnRtsAddress,"function-rts",0)
end
status("milestone:observer-loaded")
local ok,message=pcall(function() register_exec(f.checkSramAddress,"bootstrap-check-sram",0);status("milestone:direct-function-probe-armed") end)
if not ok then fail_callback(message) end
local frames=0
while true do frames=frames+1;joypad.set({Start=true},1);joypad.set({},2);emu.frameadvance();if frames%600==0 then status("frame="..frames..",pc="..string.format("%X",emu.getregister("M68K PC"))) end end
