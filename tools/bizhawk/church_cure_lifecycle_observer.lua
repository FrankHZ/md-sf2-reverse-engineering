-- One-launch Church Cure lifecycle observer.  Generated RAM is snapshotted only
-- after CheckSram; every physical PC has one dispatcher and callback faults are
-- terminal structured failures.
local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local s,h=config.static,config.static.harness
local callbacks,event_ids,records={}, {}, {}
local failed,booted,index,frames,case_frames,transition_frames=false,false,0,0,0,0
local snapshots,generated_snapshots,bootstrap_frame=nil,nil,nil
local current,prompt_index,pending=nil,0,nil
local phase,role="registration","registration"
local expectation={event=nil,call=nil,target=nil,returnPc=nil,kind="registration"}
local restoration_scope_armed=false
local terminal_finalize_executed=false
local first_restoration_mismatch=nil
local function status(x)local f=assert(io.open(config.statusPath,"a"));f:write(x.."\n");f:close()end
local function u8(a)return memory.read_u8(a,"M68K BUS")end
local function u16(a)return memory.read_u16_be(a,"M68K BUS")end
local function u32(a)return memory.read_u32_be(a,"M68K BUS")end
local function w8(a,v)memory.write_u8(a,v,"M68K BUS")end
local function w16(a,v)memory.write_u16_be(a,v,"M68K BUS")end
local function w32(a,v)memory.write_u32_be(a,v,"M68K BUS")end
local function pc()return emu.getregister("M68K PC")&0xFFFFFF end
local function js(v)return string.format("%q",v)end
local function bool(v)return v and "true" or "false"end
local function nullable(v)return v==nil and "null" or tostring(v)end
local function active_case()return config.cases[index]end
local function epc(i)return h.harnessBase+(i-1)*h.harnessStride end
local function remove_callbacks()for i=#event_ids,1,-1 do event.unregisterbyid(event_ids[i]);event_ids[i]=nil end end
local function expect(ok,msg)if not ok then error(msg)end end
local function family_mask(f)return f=="poison" and s.constants.poisonMask or f=="stun" and s.constants.stunMask or s.constants.curseMask end
local function family_cost(f)return f=="poison" and s.constants.poisonCost or f=="stun" and s.constants.stunCost or s.constants.darkSwordCureCost end
local function family_roles(f)
 if f=="curse" then return {"j-decrease-gold-entry","decrease-gold-entry","decrease-gold-return","j-unequip-all-items-if-not-cursed-entry","unequip-all-items-if-not-cursed-entry","update-combatant-stats-tail-entry","update-combatant-stats-tail-return"}end
 return {"j-decrease-gold-entry","decrease-gold-entry","decrease-gold-return","j-set-status-effects-entry","set-status-effects-entry","set-status-effects-return"}
end
local function roles_json(address,fallback)local out={};for _,e in ipairs(callbacks[address]or{})do out[#out+1]=js(e.role)end;if #out==0 then out[1]=js(fallback)end;return"["..table.concat(out,",").."]"end
local function mismatch(domain,address,expected,actual)if first_restoration_mismatch==nil and expected~=actual then first_restoration_mismatch={domain=domain,address=address,expected=expected,actual=actual}end end
local function mismatch_json()local m=first_restoration_mismatch;if not m then return"null"end;return'{"domain":'..js(m.domain)..',"address":'..nullable(m.address)..',"expected":'..m.expected..',"actual":'..m.actual..'}'end
local function member_base()return s.ram.combatantData+current.member.memberId*s.ram.combatantRecordSize end
local function snapshot_member()
 local base=member_base();local record={};for o=0,s.ram.combatantRecordSize-1 do record[o+1]=u8(base+o)end
 return{base=base,record=record}
end
local function restore_case()
 if not snapshots then return{gold=false,combatantRecords=false,targetsListLength=false,targetsListBytes=false,dialogueScratch=false,currentPortrait=false}end
 w32(s.ram.currentGold,snapshots.gold);w16(s.ram.targetsListLength,snapshots.targetsLength);w8(s.ram.targetsList,snapshots.target);w16(s.ram.dialogueName,snapshots.name);w32(s.ram.dialogueNumber,snapshots.number);w16(s.ram.currentPortrait,snapshots.portrait)
 for o=0,s.ram.combatantRecordSize-1 do w8(snapshots.member.base+o,snapshots.member.record[o+1])end
 local r={gold=true,combatantRecords=true,targetsListLength=true,targetsListBytes=true,dialogueScratch=true,currentPortrait=true}
 local actual=u32(s.ram.currentGold);r.gold=actual==snapshots.gold;mismatch("gold",s.ram.currentGold,snapshots.gold,actual)
 actual=u16(s.ram.targetsListLength);r.targetsListLength=actual==snapshots.targetsLength;mismatch("targetsListLength",s.ram.targetsListLength,snapshots.targetsLength,actual)
 actual=u8(s.ram.targetsList);r.targetsListBytes=actual==snapshots.target;mismatch("targetsListByte",s.ram.targetsList,snapshots.target,actual)
 actual=u16(s.ram.dialogueName);r.dialogueScratch=actual==snapshots.name;mismatch("dialogueName",s.ram.dialogueName,snapshots.name,actual)
 actual=u32(s.ram.dialogueNumber);r.dialogueScratch=r.dialogueScratch and actual==snapshots.number;mismatch("dialogueNumber",s.ram.dialogueNumber,snapshots.number,actual)
 actual=u16(s.ram.currentPortrait);r.currentPortrait=actual==snapshots.portrait;mismatch("currentPortrait",s.ram.currentPortrait,snapshots.portrait,actual)
 for o=0,s.ram.combatantRecordSize-1 do actual=u8(snapshots.member.base+o);if actual~=snapshots.member.record[o+1]then r.combatantRecords=false end;mismatch("combatantRecordByte",snapshots.member.base+o,snapshots.member.record[o+1],actual)end
 return r
end
local function restore_generated()
 if not generated_snapshots then return false end
 for _,span in ipairs(generated_snapshots)do for o,v in ipairs(span.bytes)do w8(span.address+o-1,v)end end
 local ok=true;for _,span in ipairs(generated_snapshots)do for o,v in ipairs(span.bytes)do local a=u8(span.address+o-1);if a~=v then ok=false end;mismatch("generatedRamByte",span.address+o-1,v,a)end end;return ok
end
local function restore_frame()
 if not bootstrap_frame or not terminal_finalize_executed then mismatch("terminalFinalize",nil,1,0);return false end
 local a6=emu.getregister("M68K A6")&0xFFFFFF;local a7=emu.getregister("M68K A7")&0xFFFFFF;mismatch("a6",nil,bootstrap_frame.a6,a6);mismatch("a7",nil,bootstrap_frame.returnA7,a7);return a6==bootstrap_frame.a6 and a7==bootstrap_frame.returnA7
end
local function restoration_json(r,generated,frame,cleared,removed)
 return'{"scopeArmed":'..bool(restoration_scope_armed)..',"gold":'..bool(r.gold)..',"combatantRecords":'..bool(r.combatantRecords)..',"targetsListLength":'..bool(r.targetsListLength)..',"targetsListBytes":'..bool(r.targetsListBytes)..',"dialogueScratch":'..bool(r.dialogueScratch)..',"currentPortrait":'..bool(r.currentPortrait)..',"generatedRam":'..bool(generated)..',"a6a7Balance":'..bool(frame)..',"sessionCartPatches":false,"callbacksCleared":'..bool(cleared)..',"outputRemoved":'..bool(removed)..'}'
end
local function failure(message)
 if failed then return end;failed=true;first_restoration_mismatch=nil
 local r,g,f
 if not restoration_scope_armed then mismatch("scope",nil,1,0);restore_case();restore_generated();restore_frame();r={gold=false,combatantRecords=false,targetsListLength=false,targetsListBytes=false,dialogueScratch=false,currentPortrait=false};g=false;f=false else r,g,f=restore_case(),restore_generated(),restore_frame()end;os.remove(config.outputPath);remove_callbacks();local test=io.open(config.outputPath,"r");local removed=test==nil;if test then test:close()end
 local c=active_case()or config.cases[1];local pending_json='{"active":'..bool(pending~=nil)..',"kind":'..js(expectation.kind)..',"caseIndex":'..math.max(index,1)..',"expectedCaseId":'..js(c.caseId)..',"memberId":'..c.member.memberId..',"expectedEventPc":'..nullable(expectation.event)..',"expectedCallPc":'..nullable(expectation.call)..',"expectedTargetPc":'..nullable(expectation.target)..',"expectedReturnPc":'..nullable(expectation.returnPc)..',"rolesAtPc":'..roles_json(pc(),role)..',"family":'..(pending and js(pending.family)or"null")..',"observedRoles":'..(records[index]and records[index].rolesJson or"[]")..'}'
 local out='{"owner":"church-cure-lifecycle","caseId":'..js(c.caseId)..',"phase":'..js(phase)..',"role":'..js(role)..',"actualPc":'..pc()..',"expectedEventPc":'..nullable(expectation.event)..',"expectedCallPc":'..nullable(expectation.call)..',"expectedTargetPc":'..nullable(expectation.target)..',"expectedReturnPc":'..nullable(expectation.returnPc)..',"pendingCallback":'..pending_json..',"restoration":'..restoration_json(r,g,f,#event_ids==0,removed)..',"restorationMismatch":'..mismatch_json()..',"error":'..js(tostring(message))..'}'
 status(config.observerFailureContract.statusPrefix..out);client.exitCode(config.observerFailureContract.exitCode)
end
local function add_role(name)for _,seen in ipairs(records[index].roles)do expect(seen~=name,"duplicate ordered milestone: "..name)end;records[index].roles[#records[index].roles+1]=name;local x={};for _,v in ipairs(records[index].roles)do x[#x+1]=js(v)end;records[index].rolesJson="["..table.concat(x,",").."]"end
local function begin_case(i)
 index=i;current=active_case();prompt_index=0;pending=nil;case_frames=0;transition_frames=0;status("milestone:case-entry:"..current.caseId)
 snapshots={gold=u32(s.ram.currentGold),targetsLength=u16(s.ram.targetsListLength),target=u8(s.ram.targetsList),name=u16(s.ram.dialogueName),number=u32(s.ram.dialogueNumber),portrait=u16(s.ram.currentPortrait),member=snapshot_member()}
 restoration_scope_armed=true
 local base=member_base();w32(s.ram.currentGold,current.gold);w16(s.ram.targetsListLength,1);w8(s.ram.targetsList,current.member.memberId);w16(base+s.ram.statusOffset,current.member.statusEffects)
 for i2,item in ipairs(current.member.items)do w16(base+s.ram.itemsOffset+(i2-1)*s.ram.itemSize,item)end
 w16(s.ram.currentPortrait,0xFFFF);w16(h.actionStub,0x7001);w16(h.actionStub+2,0x4E75);w16(h.promptStub,0x4E75)
 records[i]={entry=false,route=false,roles={},rolesJson="[]",chronology={},mutations={}}
end
local function prepare_prompt(family)
 prompt_index=prompt_index+1;local answer=current.promptResults[prompt_index];expect(answer~=nil,"unexpected Cure prompt for "..family);w16(h.promptStub,answer==0 and 0x7000 or 0x70FF);w16(h.promptStub+2,0x4E75);status("milestone:prompt:"..family..":"..current.caseId..":"..answer)
end
local function start_mutation(family)
 expect(pending==nil,"second Cure mutation pending");pending={family=family,stage=1};add_role(family..":do");status("milestone:do-"..family..":"..current.caseId)
end
local function helper(name)
 local p=pending;expect(p~=nil,"unexpected mutation helper while not pending: "..name);local family=p.family;add_role(family..":"..name)
 if family=="curse" and p.stage==7 and (name=="set-status-effects-entry" or name=="set-status-effects-return")then return end
 if name=="j-decrease-gold-entry"then expect(p.stage==1,"DecreaseGold entry order");p.stage=2
 elseif name=="decrease-gold-entry"then expect(p.stage==2,"DecreaseGold target order");p.stage=3
 elseif name=="decrease-gold-return"then expect(p.stage==3,"DecreaseGold return order");p.stage=4
 elseif name=="j-set-status-effects-entry"then expect(family~="curse"and p.stage==4,"SetStatusEffects entry order");p.stage=5
 elseif name=="set-status-effects-entry"then expect(p.stage==5,"SetStatusEffects target order");p.stage=6
 elseif name=="set-status-effects-return"then expect(p.stage==6,"SetStatusEffects return order");p.stage=7
 elseif name=="j-unequip-all-items-if-not-cursed-entry"then expect(family=="curse"and p.stage==4,"Unequip entry order");p.stage=5
 elseif name=="unequip-all-items-if-not-cursed-entry"then expect(p.stage==5,"Unequip target order");p.stage=6
 elseif name=="update-combatant-stats-tail-entry"then expect(p.stage==6,"UpdateStats tail entry order");p.stage=7
 elseif name=="update-combatant-stats-tail-return"then expect(p.stage==7,"UpdateStats tail return order");p.stage=8 end
 if (family~="curse"and p.stage==7)or(family=="curse"and p.stage==8)then
  local base=member_base();local items={};for i2=0,3 do items[#items+1]=u16(base+s.ram.itemsOffset+i2*s.ram.itemSize)end
  records[index].chronology[#records[index].chronology+1]={family=family,roles=family_roles(family)};records[index].mutations[#records[index].mutations+1]={family=family,cost=family_cost(family),statusAfter=u16(base+s.ram.statusOffset),items=items};pending=nil
 end
end
local function array_json(values)local out={};for _,v in ipairs(values)do out[#out+1]=tostring(v)end;return"["..table.concat(out,",").."]"end
local function chronology_json(values)local out={};for _,v in ipairs(values)do local r={};for _,name in ipairs(v.roles)do r[#r+1]=js(name)end;out[#out+1]='{"family":'..js(v.family)..',"roles":['..table.concat(r,",")..']}'end;return"["..table.concat(out,",").."]"end
local function mutations_json(values)local out={};for _,v in ipairs(values)do out[#out+1]='{"family":'..js(v.family)..',"cost":'..v.cost..',"statusAfter":'..v.statusAfter..',"itemSlotsAfter":'..array_json(v.items)..'}'end;return"["..table.concat(out,",").."]"end
local function finish_case(i)
 local r=records[i];expect(r.entry and r.route,"original ChurchMenu entry/route not observed");expect(pending==nil,"mutation pending at case terminal")
 local base=member_base();local after={};for i2=0,3 do after[#after+1]=u16(base+s.ram.itemsOffset+i2*s.ram.itemSize)end
 r.json='{"caseId":'..js(current.caseId)..',"churchEntryPc":'..s.entryAddresses.churchMenu..',"cureRoutePc":'..s.entryAddresses.cureRoute..',"goldBefore":'..current.gold..',"goldAfter":'..u32(s.ram.currentGold)..',"memberId":'..current.member.memberId..',"statusBefore":'..current.member.statusEffects..',"statusAfter":'..u16(base+s.ram.statusOffset)..',"itemSlotsBefore":'..array_json(current.member.items)..',"itemSlotsAfter":'..array_json(after)..',"successChronology":'..chronology_json(r.chronology)..',"mutations":'..mutations_json(r.mutations)..'}'
 local restored=restore_case();expect(restored.gold and restored.combatantRecords and restored.targetsListLength and restored.targetsListBytes and restored.dialogueScratch and restored.currentPortrait,"scoped case restoration drift")
end
local function finalize_success()
 expect(index==#config.cases,"terminal finalizer before last case");terminal_finalize_executed=true;expect(restore_generated(),"generated RAM restoration drift");expect(restore_frame(),"bootstrap A6/A7 restoration drift");remove_callbacks() -- terminal:success is represented by the shared observer-finished tail.
 local out='{"system":"sf2-church-cure-lifecycle-runtime-v1","caseOrder":[';for i,id in ipairs(config.caseOrder)do out=out..(i>1 and","or"")..js(id)end;out=out..'],"records":[';for i,r in ipairs(records)do out=out..(i>1 and","or"")..r.json end;out=out..'],"callbacksCleared":true,"restoration":{"gold":true,"combatantRecords":true,"targetsListLength":true,"targetsListBytes":true,"dialogueScratch":true,"currentPortrait":true,"generatedRam":true,"a6a7Balance":true}}'
 local f=assert(io.open(config.outputPath,"w"));f:write(out);f:close();status("milestone:callbacks-cleared:0");status("milestone:observer-finished");client.exitCode(0)
end
local function set_expectation(address,name)
 expectation={event=address,call=nil,target=nil,returnPc=nil,kind="event"}
 local x
 if name:find("decrease")then x=s.callbackSeams.decreaseGold;expectation={event=address,call=x.call,target=x.target,returnPc=x["return"],kind="helper"}
 elseif name:find("set%-status")then x=s.callbackSeams.setStatusEffects;expectation={event=address,call=x.call,target=x.target,returnPc=x["return"],kind="helper"}
 elseif name:find("unequip")then x=s.callbackSeams.unequip;expectation={event=address,call=x.call,target=x.target,returnPc=x["return"],kind="helper"}
 elseif name:find("update%-combatant")then x=s.callbackSeams.updateStats;expectation={event=address,call=nil,target=x.target,returnPc=x["return"],kind="helper"}
 elseif name=="cure-route"then x=s.callbackSeams.cureRoute;expectation={event=address,call=x.call,target=x.target,returnPc=nil,kind="route"}end
end
local function dispatch(address,event)
 phase=event.role;role=event.role;set_expectation(address,role)
 if not booted and role~="bootstrap-check-sram"then return end
 transition_frames=0
 if role=="bootstrap-check-sram"then
  local stack=emu.getregister("M68K A7")&0xFFFFFF;bootstrap_frame={a6=emu.getregister("M68K A6")&0xFFFFFF,returnA7=(stack+4)&0xFFFFFF};generated_snapshots={};for _,span in ipairs({{address=h.harnessBase,width=h.generatedHarnessBytes},{address=h.actionStub,width=h.generatedStubBytes},{address=h.promptStub,width=h.generatedStubBytes},{address=h.terminalStub,width=h.generatedTerminalBytes}})do local b={};for o=0,span.width-1 do b[o+1]=u8(span.address+o)end;generated_snapshots[#generated_snapshots+1]={address=span.address,bytes=b}end;write_harness();w32(stack,h.harnessBase);booted=true;status("milestone:direct-function-probe")
 elseif role=="case-entry"then begin_case(event.index)
 elseif role=="church-entry"then records[index].entry=true;status("milestone:church-entry")
 elseif role=="cure-route"then expect((emu.getregister("M68K D0")&0xFFFF)==1,"Cure action selection drift");records[index].route=true;status("milestone:cure-route")
 elseif role=="action-stub"then expect(true,"controlled Cure action stub")
 elseif role=="prompt-poison"then prepare_prompt("poison")
 elseif role=="prompt-stun"then prepare_prompt("stun")
 elseif role=="prompt-curse"then prepare_prompt("curse")
 elseif role=="do-poison"then start_mutation("poison")
 elseif role=="do-stun"then start_mutation("stun")
 elseif role=="do-curse"then start_mutation("curse")
 elseif role=="case-result"then finish_case(event.index)
 elseif role=="terminal-finalize"then finalize_success()
 else helper(role) end
end
function register(address,name,i)
 if callbacks[address]==nil then callbacks[address]={};event_ids[#event_ids+1]=event.on_bus_exec(function()local ok,msg=pcall(function()for _,event in ipairs(callbacks[address])do dispatch(address,event)end end);if not ok then failure(msg)end end,address,"church-cure-"..address,"M68K BUS")end
 callbacks[address][#callbacks[address]+1]={role=name,index=i}
end
function write_harness()
 for i,_ in ipairs(config.cases)do local a=epc(i);w16(a,0x2C7C);w32(a+2,h.harnessBase+0x180);w16(a+6,0x2E7C);w32(a+8,h.stackTop);w16(a+12,0x4EB9);w32(a+14,s.entryAddresses.churchMenu);w16(a+18,0x4E71);w16(a+20,0x4E71);w16(a+22,0x4EF9);w32(a+24,i==#config.cases and h.terminalStub or epc(i+1))end
 w16(h.terminalStub,0x2C7C);w32(h.terminalStub+2,bootstrap_frame.a6);w16(h.terminalStub+6,0x2E7C);w32(h.terminalStub+8,bootstrap_frame.returnA7);w16(h.terminalStub+12,0x4EF9);w32(h.terminalStub+14,h.terminalStub+12)
end
local function register_callbacks()
 for i,_ in ipairs(config.cases)do register(epc(i),"case-entry",i);register(epc(i)+h.resultOffset,"case-result",i)end
 register(h.terminalStub+12,"terminal-finalize",0);register(s.entryAddresses.churchMenu,"church-entry",0);register(s.entryAddresses.cureRoute,"cure-route",0);register(h.actionStub,"action-stub",0)
 register(0x20BBE,"prompt-poison",0);register(0x211BA,"prompt-stun",0);register(0x20CC0,"prompt-curse",0);register(s.entryAddresses.poisonDo,"do-poison",0);register(s.entryAddresses.stunDo,"do-stun",0);register(s.entryAddresses.curseDo,"do-curse",0)
 register(s.aliases.j_DecreaseGold.address,"j-decrease-gold-entry",0);register(s.aliases.j_DecreaseGold.effectiveTarget,"decrease-gold-entry",0);register(s.callbackSeams.decreaseGold["return"],"decrease-gold-return",0)
 register(s.aliases.j_SetStatusEffects.address,"j-set-status-effects-entry",0);register(s.aliases.j_SetStatusEffects.effectiveTarget,"set-status-effects-entry",0);register(s.callbackSeams.setStatusEffects["return"],"set-status-effects-return",0)
 register(s.aliases.j_UnequipAllItemsIfNotCursed.address,"j-unequip-all-items-if-not-cursed-entry",0);register(s.aliases.j_UnequipAllItemsIfNotCursed.effectiveTarget,"unequip-all-items-if-not-cursed-entry",0);register(s.callbackSeams.updateStats.target,"update-combatant-stats-tail-entry",0);register(s.callbackSeams.updateStats["return"],"update-combatant-stats-tail-return",0)
end
local ok,msg=pcall(function()register_callbacks();register(h.checkSram,"bootstrap-check-sram",0);status("milestone:observer-loaded");status("milestone:direct-function-probe-armed")end);if not ok then failure(msg)end
while true do frames=frames+1;if booted and index>0 then case_frames=case_frames+1;transition_frames=transition_frames+1 end;joypad.set({Start=true},1);joypad.set({},2);if not booted and frames>h.bootstrapFrameBudget then phase="bootstrap-watchdog";role="bootstrap-watchdog";expectation={event=nil,call=nil,target=nil,returnPc=nil,kind="event"};failure("bootstrap watchdog exhausted")end;if booted and index>0 and transition_frames>h.transitionFrameBudget then phase="transition-watchdog";role="transition-watchdog";expectation={event=nil,call=nil,target=nil,returnPc=nil,kind="event"};failure("transition watchdog exhausted for "..current.caseId)end;if booted and index>0 and case_frames>h.caseFrameBudget then phase="case-watchdog";role="case-watchdog";expectation={event=nil,call=nil,target=nil,returnPc=nil,kind="event"};failure("case watchdog exhausted for "..current.caseId)end;emu.frameadvance()end
