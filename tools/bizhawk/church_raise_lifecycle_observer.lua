-- Grouped Church Raise probe.  Registration never writes generated RAM: CheckSram
-- snapshots that bounded range before atomically emitting the harness program.
local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local s,h=config.static,config.static.harness
local callbacks,event_ids,records={}, {}, {}
local failed,booted,index,frames,case_frames=false,false,0,0,0
local snapshots,generated_snapshots,bootstrap_frame=nil,nil,nil
local transaction_pending,current,prompt_index=nil,nil,0
local frame_detail=""
local final_ready=false
local restoration_scope_armed=false
local first_restoration_mismatch=nil
local phase,role="registration","registration"
local current_expectation={call=nil,target=nil,returnPc=nil,kind="registration"}
local function status(x)local f=assert(io.open(config.statusPath,"a"));f:write(x.."\n");f:close()end
local function u8(a)return memory.read_u8(a,"M68K BUS")end
local function u16(a)return memory.read_u16_be(a,"M68K BUS")end
local function u32(a)return memory.read_u32_be(a,"M68K BUS")end
local function w8(a,x)memory.write_u8(a,x,"M68K BUS")end
local function w16(a,x)memory.write_u16_be(a,x,"M68K BUS")end
local function w32(a,x)memory.write_u32_be(a,x,"M68K BUS")end
local function js(x)return string.format("%q",x)end
local function nullable(x)return x==nil and "null" or tostring(x)end
local function bool(x)return x and "true" or "false"end
local function pc()return emu.getregister("M68K PC")&0xFFFFFF end
local function active_case()return config.cases[index]end
local function epc(i)return h.harnessBase+(i-1)*h.harnessStride end
local function remove_callbacks()for i=#event_ids,1,-1 do event.unregisterbyid(event_ids[i]);event_ids[i]=nil end end
local function event_address(name)
 for _,event in ipairs(s.helperChronology) do if event.role==name then return event.pc end end
 return nil
end
local function events_json(names)
 local out={};for _,name in ipairs(names) do out[#out+1]='{"role":'..js(name)..',"pc":'..event_address(name)..'}' end
 return "["..table.concat(out,",").."]"
end
local function roles_json(address,fallback)
 local out={};for _,event in ipairs(callbacks[address] or {})do out[#out+1]=js(event.role)end;if #out==0 then out[#out+1]=js(fallback)end
 return "["..table.concat(out,",").."]"
end
local function capture_restoration_mismatch(domain,address,expected,actual)
 if first_restoration_mismatch==nil and expected~=actual then first_restoration_mismatch={domain=domain,address=address,expected=expected,actual=actual}end
end
local function restoration_mismatch_json()
 local m=first_restoration_mismatch;if not m then return "null" end
 return '{"domain":'..js(m.domain)..',"address":'..nullable(m.address)..',"expected":'..m.expected..',"actual":'..m.actual..'}'
end
local function restoration_json(state,generated_ok,frame_ok,cleared,removed)
 return '{"scopeArmed":'..bool(restoration_scope_armed)..',"gold":'..bool(state.gold)..',"combatantRecords":'..bool(state.combatantRecords)..',"mapspriteBytes":'..bool(state.mapspriteBytes)..',"dialogueScratch":'..bool(state.dialogueScratch)..',"targetsListLength":'..bool(state.targetsListLength)..',"targetsListBytes":'..bool(state.targetsListBytes)..',"currentPortrait":'..bool(state.currentPortrait)..',"generatedRam":'..bool(generated_ok)..',"a6a7Balance":'..bool(frame_ok)..',"sessionCartPatches":false,"callbacksCleared":'..bool(cleared)..',"outputRemoved":'..bool(removed)..'}'
end
local function restore_case()
 if not snapshots then return {gold=true,combatantRecords=true,mapspriteBytes=true,dialogueScratch=true,targetsListLength=true,targetsListBytes=true,currentPortrait=true}end
 w32(s.ram.currentGold,snapshots.gold);w16(s.ram.dialogueName,snapshots.name);w32(s.ram.dialogueNumber,snapshots.number)
 w16(s.ram.targetsListLength,snapshots.targetsLength);for o,v in ipairs(snapshots.targets)do w8(s.ram.targetsList+o-1,v)end;w16(s.ram.currentPortrait,snapshots.portrait)
 for _,v in ipairs(snapshots.members)do for o=0,s.ram.combatantRecordSize-1 do w8(v.base+o,v.record[o+1])end;w8(v.spriteAddress,v.sprite)end
 local result={gold=true,dialogueScratch=true,targetsListLength=true,currentPortrait=true,combatantRecords=true,mapspriteBytes=true,targetsListBytes=true}
 local actual=u32(s.ram.currentGold);result.gold=actual==snapshots.gold;capture_restoration_mismatch("gold",s.ram.currentGold,snapshots.gold,actual)
 actual=u16(s.ram.dialogueName);if actual~=snapshots.name then result.dialogueScratch=false end;capture_restoration_mismatch("dialogueName",s.ram.dialogueName,snapshots.name,actual)
 actual=u32(s.ram.dialogueNumber);if actual~=snapshots.number then result.dialogueScratch=false end;capture_restoration_mismatch("dialogueNumber",s.ram.dialogueNumber,snapshots.number,actual)
 actual=u16(s.ram.targetsListLength);result.targetsListLength=actual==snapshots.targetsLength;capture_restoration_mismatch("targetsListLength",s.ram.targetsListLength,snapshots.targetsLength,actual)
 for o,v in ipairs(snapshots.targets)do actual=u8(s.ram.targetsList+o-1);if actual~=v then result.targetsListBytes=false end;capture_restoration_mismatch("targetsListByte",s.ram.targetsList+o-1,v,actual)end
 actual=u16(s.ram.currentPortrait);result.currentPortrait=actual==snapshots.portrait;capture_restoration_mismatch("currentPortrait",s.ram.currentPortrait,snapshots.portrait,actual)
 for _,v in ipairs(snapshots.members)do for o=0,s.ram.combatantRecordSize-1 do actual=u8(v.base+o);if actual~=v.record[o+1] then result.combatantRecords=false end;capture_restoration_mismatch("combatantRecordByte",v.base+o,v.record[o+1],actual)end;actual=u8(v.spriteAddress);if actual~=v.sprite then result.mapspriteBytes=false end;capture_restoration_mismatch("mapspriteByte",v.spriteAddress,v.sprite,actual)end
 return result
end
local function restore_generated()
 if not generated_snapshots then return true end
 for _,span in ipairs(generated_snapshots)do for o,v in ipairs(span.bytes)do w8(span.address+o-1,v)end end
 local ok=true;for _,span in ipairs(generated_snapshots)do for o,v in ipairs(span.bytes)do local actual=u8(span.address+o-1);if actual~=v then ok=false end;capture_restoration_mismatch("generatedRamByte",span.address+o-1,v,actual)end end
 return ok
end
local function restore_frame()
 local saved=bootstrap_frame;if not saved then return true end
 local actualA7=emu.getregister("M68K A7")&0xFFFFFF;local actualA6=emu.getregister("M68K A6")&0xFFFFFF
 frame_detail=string.format(" expectedA6=%X actualA6=%X expectedA7=%X actualA7=%X",saved.a6,actualA6,saved.returnA7,actualA7)
 capture_restoration_mismatch("a6",nil,saved.a6,actualA6);capture_restoration_mismatch("a7",nil,saved.returnA7,actualA7)
 return actualA7==saved.returnA7 and actualA6==saved.a6
end
local function failure(message)
 if failed then return end;failed=true
 first_restoration_mismatch=nil
 local state,generated_ok,frame_ok=restore_case(),restore_generated(),restore_frame()
 os.remove(config.outputPath);remove_callbacks();local f=io.open(config.outputPath,"r");local removed=f==nil;if f then f:close()end
 local c=active_case() or config.cases[1];local r=records[index] or {helperRoles={}}
 local pending='{"active":true,"kind":'..js(current_expectation.kind)..',"caseIndex":'..math.max(index,1)..',"expectedCaseId":'..js(c.caseId)..',"rolesAtPc":'..roles_json(pc(),role)..',"observedChronology":'..events_json(r.helperRoles)..',"expectedChronology":'..events_json(s.helperChronology and (function()local x={};for n,e in ipairs(s.helperChronology)do x[n]=e.role end;return x end)() or {})..',"observedChronologyCount":'..#r.helperRoles..',"expectedChronologyCount":'..#s.helperChronology..'}'
 local payload='{"owner":"church-raise-lifecycle","caseId":'..js(c.caseId)..',"phase":'..js(phase)..',"role":'..js(role)..',"actualPc":'..pc()..',"expectedCallPc":'..nullable(current_expectation.call)..',"expectedTargetPc":'..nullable(current_expectation.target)..',"expectedReturnPc":'..nullable(current_expectation.returnPc)..',"pendingCallback":'..pending..',"restoration":'..restoration_json(state,generated_ok,frame_ok,#event_ids==0,removed)..',"restorationMismatch":'..restoration_mismatch_json()..',"error":'..js(tostring(message))..'}'
 status(config.observerFailureContract.statusPrefix..payload);client.exitCode(config.observerFailureContract.exitCode)
end
local function expect(ok,msg)if not ok then error(msg)end end
local function record_member(member)
 local base=s.ram.combatantData+member.memberId*s.ram.combatantRecordSize;local sprite=s.ram.entityData+s.ram.mapspriteOffset+member.memberId*32
 local record={};for o=0,s.ram.combatantRecordSize-1 do record[o+1]=u8(base+o)end
 return {base=base,record=record,spriteAddress=sprite,sprite=u8(sprite)}
end
local function begin_case(i)
 index=i;current=active_case();prompt_index=0;transaction_pending=nil;case_frames=0
 status("milestone:case-entry:"..current.caseId);snapshots={gold=u32(s.ram.currentGold),name=u16(s.ram.dialogueName),number=u32(s.ram.dialogueNumber),targetsLength=u16(s.ram.targetsListLength),portrait=u16(s.ram.currentPortrait),targets={},members={}}
 for o=0,h.targetsSnapshotBytes-1 do snapshots.targets[o+1]=u8(s.ram.targetsList+o)end
 for _,m in ipairs(current.members)do snapshots.members[#snapshots.members+1]=record_member(m)end
 w32(s.ram.currentGold,current.gold);w16(s.ram.targetsListLength,#current.members)
 for n,m in ipairs(current.members)do local base=s.ram.combatantData+m.memberId*s.ram.combatantRecordSize;w8(s.ram.targetsList+n-1,m.memberId);w8(base+s.ram.classOffset,m.classId);w8(base+s.ram.levelOffset,m.level);w16(base+s.ram.hpMaxOffset,m.hpMax);w16(base+s.ram.hpCurrentOffset,m.hpCurrent);w8(s.ram.entityData+s.ram.mapspriteOffset+m.memberId*32,m.mapsprite)end
 w16(s.ram.currentPortrait,0xFFFF);w16(h.actionStub,0x7000);w16(h.actionStub+2,0x4E75);w16(h.promptStub,0x4E75)
 records[i]={entry=false,route=false,doRaiseSeen=false,commitComplete=false,raised={},helperRoles={}}
end
local function prepare_prompt()
 prompt_index=prompt_index+1;local answer=current.promptResults[prompt_index];expect(answer~=nil,"unexpected Raise prompt");status("milestone:prompt:"..current.caseId..":"..answer);w16(h.promptStub,answer==0 and 0x7000 or 0x70FF);w16(h.promptStub+2,0x4E75)
end
local function helper(name)
 local r=records[index];r.helperRoles[#r.helperRoles+1]=name
 if name=="j-decrease-gold-entry" then expect(not transaction_pending,"DecreaseGold entry while already pending");expect(r.doRaiseSeen,"DecreaseGold entry without original DoRaise admission");expect(not r.commitComplete,"unexpected second Raise helper admission");transaction_pending={stage=1}
 elseif name=="decrease-gold-entry" then expect(transaction_pending and transaction_pending.stage==1,"DecreaseGold target chronology");transaction_pending.stage=2
 elseif name=="decrease-gold-return" then expect(transaction_pending and transaction_pending.stage==2,"DecreaseGold return chronology");transaction_pending.stage=3
 elseif name=="j-increase-current-hp-entry" then expect(transaction_pending and transaction_pending.stage==3,"IncreaseCurrentHp entry while not pending");transaction_pending.member=emu.getregister("M68K D0")&0xFFFF;expect((emu.getregister("M68K D1")&0xFFFF)==s.ram.hpCap,"HP cap operand drift");transaction_pending.stage=4
 elseif name=="increase-current-hp-entry" then expect(transaction_pending and transaction_pending.stage==4,"IncreaseCurrentHp target chronology");transaction_pending.stage=5
 elseif name=="increase-current-hp-return" then expect(transaction_pending and transaction_pending.stage==5,"IncreaseCurrentHp return chronology");transaction_pending.stage=6
 elseif name=="mapsprite-entry" then expect(transaction_pending and transaction_pending.stage==6,"mapsprite entry while not pending");transaction_pending.stage=7
 elseif name=="mapsprite-return" then expect(transaction_pending and transaction_pending.stage==7,"mapsprite return chronology");r.raised[transaction_pending.member]=true;r.commitComplete=true;transaction_pending=nil end
end
local success_roles='["j-decrease-gold-entry","decrease-gold-entry","decrease-gold-return","j-increase-current-hp-entry","increase-current-hp-entry","increase-current-hp-return","mapsprite-entry","mapsprite-return"]'
local function json_member(m,raised)
 local base=s.ram.combatantData+m.memberId*s.ram.combatantRecordSize;local sprite=s.ram.entityData+s.ram.mapspriteOffset+m.memberId*32
 return '{"memberId":'..m.memberId..',"classId":'..m.classId..',"level":'..m.level..',"promoted":'..bool(m.promoted)..',"hpMax":'..m.hpMax..',"hpCurrent":'..u16(base+s.ram.hpCurrentOffset)..',"mapsprite":'..u8(sprite)..',"raised":'..bool(raised)..'}'
end
local function finalize_success()
 expect(final_ready,"terminal finalizer reached before final case")
 local saved=bootstrap_frame;expect(saved,"missing bootstrap A6/A7 frame")
 local actualA7=emu.getregister("M68K A7")&0xFFFFFF;local actualA6=emu.getregister("M68K A6")&0xFFFFFF
 frame_detail=string.format(" expectedA6=%X actualA6=%X expectedA7=%X actualA7=%X",saved.a6,actualA6,saved.returnA7,actualA7)
 expect(actualA7==saved.returnA7 and actualA6==saved.a6,"A6/A7 restoration drift"..frame_detail)
 expect(restore_generated(),"generated RAM restoration drift")
 remove_callbacks();local out='{"system":"sf2-church-raise-lifecycle-runtime-v1","caseOrder":[';for n,id in ipairs(config.caseOrder)do out=out..(n>1 and ',' or '')..js(id)end;out=out..'],"records":[';for n,x in ipairs(records)do out=out..(n>1 and ',' or '')..x.json end;out=out..'],"callbacksCleared":true,"restoration":{"gold":true,"combatantRecords":true,"mapspriteBytes":true,"dialogueScratch":true,"targetsListLength":true,"targetsListBytes":true,"currentPortrait":true,"generatedRam":true,"a6a7Balance":true}}';local f=assert(io.open(config.outputPath,"w"));f:write(out);f:close();status("milestone:callbacks-cleared:0");status("milestone:observer-finished");client.exitCode(0)
end
local function finish_case(i)
 local c=config.cases[i];local r=records[i];expect(r.entry and r.route,"original ChurchMenu entry/route not observed");expect(transaction_pending==nil,"transaction pending at terminal")
 local expected_roles="j-decrease-gold-entry,decrease-gold-entry,decrease-gold-return,j-increase-current-hp-entry,increase-current-hp-entry,increase-current-hp-return,mapsprite-entry,mapsprite-return"
 if r.commitComplete then expect(table.concat(r.helperRoles,",")==expected_roles,"original helper chronology drift")else expect(#r.helperRoles==0,"negative case entered original mutation helper")end
 expect((emu.getregister("M68K A6")&0xFFFFFF)==h.harnessBase+0x180,"ChurchMenu A6 frame balance drift");expect((emu.getregister("M68K A7")&0xFFFFFF)==h.stackTop,"ChurchMenu A7 stack balance drift")
 local dead,gold_after,members,chron,mut=0,u32(s.ram.currentGold),{},{},{}
 for _,m in ipairs(c.members)do if m.hpCurrent==0 then dead=dead+1 end;local raised=r.raised[m.memberId] or false;members[#members+1]=json_member(m,raised);if raised then local cost=m.level*s.cost.perLevel+(m.promoted and s.cost.promotedExtra or 0);chron[#chron+1]='{"memberId":'..m.memberId..',"roles":'..success_roles..'}';mut[#mut+1]='{"memberId":'..m.memberId..',"cost":'..cost..',"hpAfter":'..u16(s.ram.combatantData+m.memberId*s.ram.combatantRecordSize+s.ram.hpCurrentOffset)..'}'end end
 records[i].json='{"caseId":'..js(c.caseId)..',"churchEntryPc":'..s.entryAddresses.churchMenu..',"raiseRoutePc":'..s.entryAddresses.raiseRoute..',"deadMemberCount":'..dead..',"goldBefore":'..c.gold..',"goldAfter":'..gold_after..',"members":['..table.concat(members,",")..'],"successChronology":['..table.concat(chron,",")..'],"mutations":['..table.concat(mut,",")..']}'
 local restored=restore_case();expect(restored.gold and restored.combatantRecords and restored.mapspriteBytes and restored.dialogueScratch and restored.targetsListLength and restored.targetsListBytes and restored.currentPortrait,"scoped case state restoration drift")
 if i==#config.cases then final_ready=true end
end
local function set_expectation(address,name)
 current_expectation={call=nil,target=nil,returnPc=nil,kind="event"}
 if name=="raise-route" then local x=s.callbackSeams.raiseRoute;current_expectation={call=x.call,target=x.target,returnPc=x["return"],kind="route"}
 elseif name:find("decrease%-gold")then local x=s.callbackSeams.decreaseGold;current_expectation={call=x.call,target=x.target,returnPc=x["return"],kind="helper"}
 elseif name:find("increase%-current%-hp")then local x=s.callbackSeams.increaseCurrentHp;current_expectation={call=x.call,target=x.target,returnPc=x["return"],kind="helper"}
 elseif name:find("mapsprite")then local x=s.callbackSeams.mapsprite;current_expectation={call=x.call,target=x.target,returnPc=x["return"],kind="helper"}
 end
end
local function dispatch(address,event)
 phase=event.role;role=event.role;set_expectation(address,role)
 if role=="bootstrap-check-sram" then local stack=emu.getregister("M68K A7")&0xFFFFFF;bootstrap_frame={a6=emu.getregister("M68K A6")&0xFFFFFF,a7=stack,returnA7=(stack+4)&0xFFFFFF};generated_snapshots={};for _,span in ipairs({{address=h.harnessBase,width=h.generatedHarnessBytes},{address=h.actionStub,width=h.generatedStubBytes},{address=h.promptStub,width=h.generatedStubBytes},{address=h.terminalStub,width=h.generatedTerminalBytes}})do local bytes={};for o=0,span.width-1 do bytes[o+1]=u8(span.address+o)end;generated_snapshots[#generated_snapshots+1]={address=span.address,bytes=bytes}end;restoration_scope_armed=true;write_harness();w32(stack,h.harnessBase);booted=true;status("milestone:direct-function-probe")
 elseif role=="case-entry" then begin_case(event.index)
 elseif role=="church-entry" then records[index].entry=true;status("milestone:church-entry")
 elseif role=="raise-route" then records[index].route=true;status("milestone:raise-route")
 elseif role=="action-stub" then expect((emu.getregister("M68K D0")&0xFFFF)==0,"Raise action stub result drift")
 elseif role=="prompt-call" then prepare_prompt()
 elseif role=="prompt-stub" then expect(true,"controlled prompt stub")
 elseif role=="prompt-compare" then status("milestone:prompt-compare:"..current.caseId..":"..(emu.getregister("M68K D0")&0xFFFF))
 elseif role=="promotion-check" then status("milestone:promotion:"..current.caseId..":"..(emu.getregister("M68K D1")&0xFFFF))
 elseif role=="affordability-check" then status("milestone:cost:"..current.caseId..":"..(emu.getregister("M68K D0")&0xFFFFFFFF)..":"..(emu.getregister("M68K D1")&0xFFFFFFFF))
 elseif role=="do-raise" then records[index].doRaiseSeen=true;status("milestone:do-raise:"..current.caseId)
 elseif role=="case-result" then finish_case(event.index)
 elseif role=="terminal-finalize" then finalize_success()
 elseif role:find("gold")or role:find("hp")or role:find("mapsprite")then helper(role)
 else error("unknown deterministic dispatch role: "..role)end
end
local function register(address,name,i)
 if not callbacks[address]then callbacks[address]={};event_ids[#event_ids+1]=event.on_bus_exec(function()if failed then return end;local ok,msg=pcall(function()for _,event in ipairs(callbacks[address])do dispatch(address,event)end end);if not ok then failure(msg)end end,address,"church-raise-"..address,"M68K BUS")end
 callbacks[address][#callbacks[address]+1]={role=name,index=i}
end
function write_harness()
 for i,_ in ipairs(config.cases)do local a=epc(i);w16(a,0x2C7C);w32(a+2,h.harnessBase+0x180);w16(a+6,0x2E7C);w32(a+8,h.stackTop);w16(a+12,0x4EB9);w32(a+14,s.entryAddresses.churchMenu);w16(a+18,0x4E71);w16(a+20,0x4E71);w16(a+22,0x4EF9);w32(a+24,i==#config.cases and h.terminalStub or epc(i+1))end;w16(h.terminalStub,0x2C7C);w32(h.terminalStub+2,bootstrap_frame.a6);w16(h.terminalStub+6,0x2E7C);w32(h.terminalStub+8,bootstrap_frame.returnA7);w16(h.terminalStub+12,0x4EF9);w32(h.terminalStub+14,h.terminalStub+12)
end
local function register_callbacks()
 for i,_ in ipairs(config.cases)do register(epc(i),"case-entry",i);register(epc(i)+h.resultOffset,"case-result",i)end
 register(h.terminalStub+12,"terminal-finalize",0)
 register(s.entryAddresses.churchMenu,"church-entry",0);register(s.entryAddresses.raiseRoute,"raise-route",0);register(h.actionStub,"action-stub",0);register(0x20AD8,"prompt-call",0);register(h.promptStub,"prompt-stub",0);register(0x20AE4,"prompt-compare",0);register(0x20AB6,"promotion-check",0);register(0x20B02,"affordability-check",0);register(0x20B0C,"do-raise",0)
 register(s.aliases.jDecreaseGold.address,"j-decrease-gold-entry",0);register(s.aliases.jDecreaseGold.effectiveTarget,"decrease-gold-entry",0);register(s.aliases.jDecreaseGold["return"],"decrease-gold-return",0);register(s.aliases.jIncreaseCurrentHp.address,"j-increase-current-hp-entry",0);register(s.aliases.jIncreaseCurrentHp.effectiveTarget,"increase-current-hp-entry",0);register(s.aliases.jIncreaseCurrentHp["return"],"increase-current-hp-return",0);register(s.entryAddresses.updateAllyMapsprite,"mapsprite-entry",0);register(s.callbackSeams.mapsprite["return"],"mapsprite-return",0)
end
local ok,msg=pcall(function()register_callbacks();register(h.checkSram,"bootstrap-check-sram",0);status("milestone:observer-loaded");status("milestone:direct-function-probe-armed")end);if not ok then failure(msg)end
while true do frames=frames+1;if booted and index>0 then case_frames=case_frames+1 end;joypad.set({Start=true},1);joypad.set({},2);if not booted and frames>h.bootstrapFrameBudget then phase="bootstrap-watchdog";role="bootstrap-watchdog";current_expectation={call=nil,target=nil,returnPc=nil,kind="event"};failure("bootstrap watchdog exhausted")end;if booted and index>0 and case_frames>h.caseFrameBudget then phase="case-watchdog";role="case-watchdog";current_expectation={call=nil,target=nil,returnPc=nil,kind="event"};failure("case watchdog exhausted for "..current.caseId)end;emu.frameadvance()end
