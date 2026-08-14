-- Grouped Church Save lifecycle observer.  One dispatch table owns every
-- callback PC; protected callbacks report a structured failure and nonzero exit.
local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local s,h=config.static,config.static.harness
local callbacks,event_ids,records={}, {}, {}
local booted,failed,index,frames,case_frames=false,false,0,0,0
local snapshot,generated,bootstrap_frame,current=nil,nil,nil,nil
local phase,role="registration","registration"
local mode="registration"
local expectation={event=nil,call=nil,target=nil,returnPc=nil,kind="registration"}
local restoration_scope_armed=false
local first_mismatch=nil
local function status(x)local f=assert(io.open(config.statusPath,"a"));f:write(x.."\n");f:close()end
local function u8(a)return memory.read_u8(a,"M68K BUS")end
local function u16(a)return memory.read_u16_be(a,"M68K BUS")end
local function u32(a)return memory.read_u32_be(a,"M68K BUS")end
local function w8(a,x)memory.write_u8(a,x,"M68K BUS")end
local function w16(a,x)memory.write_u16_be(a,x,"M68K BUS")end
local function w32(a,x)memory.write_u32_be(a,x,"M68K BUS")end
local function pc()return emu.getregister("M68K PC")&0xFFFFFF end
local function js(x)return string.format("%q",x)end
local function nullable(x)return x==nil and "null" or tostring(x)end
local function bool(x)return x and "true" or "false"end
local function active_case()return config.cases[index]end
local function epc(i)return h.harnessBase+(i-1)*h.harnessStride end
local function remove_callbacks()for i=#event_ids,1,-1 do event.unregisterbyid(event_ids[i]);event_ids[i]=nil end end
local function remember(domain,address,expected,actual)
 if first_mismatch==nil and expected~=actual then first_mismatch={domain=domain,address=address,expected=expected,actual=actual}end
end
local function roles_json(address,fallback)
 local names={};for _,entry in ipairs(callbacks[address] or {})do names[#names+1]=js(entry.role)end
 if #names==0 then names[1]=js(fallback)end;return "["..table.concat(names,",").."]"
end
local function mismatch_json()
 local x=first_mismatch;if x==nil then return "null" end
 return '{"domain":'..js(x.domain)..',"address":'..nullable(x.address)..',"expected":'..x.expected..',"actual":'..x.actual..'}'
end
local function slot_start(c)return c.selector==0 and s.saveGame.slot1Data or s.saveGame.slot2Data end
local function restore_case()
 if snapshot==nil then return {currentMap=true,egressMap=true,currentSaveSlot=true,flag399=true,slotSram=true,checksum=true,saveFlags=true,dialoguePortraitScratch=true}end
 local r=s.ram;local c=active_case();local start=slot_start(c)
 w8(r.currentMap,snapshot.currentMap);w8(r.egressMap,snapshot.egressMap);w16(r.currentSaveSlot,snapshot.currentSaveSlot);w8(r.flag399Byte,snapshot.flag399)
 for i,v in ipairs(snapshot.slot)do w8(start+(i-1)*2,v)end
 w8(c.selector==0 and s.saveGame.slot1Checksum or s.saveGame.slot2Checksum,snapshot.checksum);w8(s.saveGame.saveFlags,snapshot.saveFlags)
 w16(r.currentPortrait,snapshot.currentPortrait);w16(r.dialogueName,snapshot.dialogueName);w32(r.dialogueNumber,snapshot.dialogueNumber)
 local out={currentMap=true,egressMap=true,currentSaveSlot=true,flag399=true,slotSram=true,checksum=true,saveFlags=true,dialoguePortraitScratch=true}
 local actual=u8(r.currentMap);out.currentMap=actual==snapshot.currentMap;remember("currentMap",r.currentMap,snapshot.currentMap,actual)
 actual=u8(r.egressMap);out.egressMap=actual==snapshot.egressMap;remember("egressMap",r.egressMap,snapshot.egressMap,actual)
 actual=u16(r.currentSaveSlot);out.currentSaveSlot=actual==snapshot.currentSaveSlot;remember("currentSaveSlot",r.currentSaveSlot,snapshot.currentSaveSlot,actual)
 actual=u8(r.flag399Byte);out.flag399=actual==snapshot.flag399;remember("flag399",r.flag399Byte,snapshot.flag399,actual)
 for i,v in ipairs(snapshot.slot)do actual=u8(start+(i-1)*2);if actual~=v then out.slotSram=false end;remember("slotSramByte",start+(i-1)*2,v,actual)end
 local checksum_address=c.selector==0 and s.saveGame.slot1Checksum or s.saveGame.slot2Checksum;actual=u8(checksum_address);out.checksum=actual==snapshot.checksum;remember("checksum",checksum_address,snapshot.checksum,actual)
 actual=u8(s.saveGame.saveFlags);out.saveFlags=actual==snapshot.saveFlags;remember("saveFlags",s.saveGame.saveFlags,snapshot.saveFlags,actual)
 actual=u16(r.currentPortrait);if actual~=snapshot.currentPortrait then out.dialoguePortraitScratch=false end;remember("currentPortrait",r.currentPortrait,snapshot.currentPortrait,actual)
 actual=u16(r.dialogueName);if actual~=snapshot.dialogueName then out.dialoguePortraitScratch=false end;remember("dialogueName",r.dialogueName,snapshot.dialogueName,actual)
 actual=u32(r.dialogueNumber);if actual~=snapshot.dialogueNumber then out.dialoguePortraitScratch=false end;remember("dialogueNumber",r.dialogueNumber,snapshot.dialogueNumber,actual)
 return out
end
local function restore_generated()
 if generated==nil then return true end
 for _,span in ipairs(generated)do for i,v in ipairs(span.bytes)do w8(span.address+i-1,v)end end
 local ok=true;for _,span in ipairs(generated)do for i,v in ipairs(span.bytes)do local actual=u8(span.address+i-1);if actual~=v then ok=false end;remember("generatedRamByte",span.address+i-1,v,actual)end end
 return ok
end
local function restore_frame()
 if bootstrap_frame==nil then return true end
 local a6=emu.getregister("M68K A6")&0xFFFFFF;local a7=emu.getregister("M68K A7")&0xFFFFFF
 remember("a6",nil,bootstrap_frame.a6,a6);remember("a7",nil,bootstrap_frame.returnA7,a7)
 return a6==bootstrap_frame.a6 and a7==bootstrap_frame.returnA7
end
local function restoration_json(state,generated_ok,frame_ok,cleared,removed)
 return '{"scopeArmed":'..bool(restoration_scope_armed)..',"currentMap":'..bool(state.currentMap)..',"egressMap":'..bool(state.egressMap)..',"currentSaveSlot":'..bool(state.currentSaveSlot)..',"flag399":'..bool(state.flag399)..',"slotSram":'..bool(state.slotSram)..',"checksum":'..bool(state.checksum)..',"saveFlags":'..bool(state.saveFlags)..',"dialoguePortraitScratch":'..bool(state.dialoguePortraitScratch)..',"generatedRam":'..bool(generated_ok)..',"bootstrapFrame":'..bool(frame_ok)..',"sessionCartPatches":false,"callbacksCleared":'..bool(cleared)..',"outputRemoved":'..bool(removed)..'}'
end
local function failure(message)
 if failed then return end;failed=true;first_mismatch=nil
 local state,generated_ok,frame_ok=restore_case(),restore_generated(),restore_frame()
 os.remove(config.outputPath);remove_callbacks();local f=io.open(config.outputPath,"r");local removed=f==nil;if f then f:close()end
 local c=active_case() or config.cases[1]
 local pending='{"active":true,"kind":'..js(expectation.kind)..',"mode":'..js(mode)..',"caseIndex":'..math.max(index,1)..',"expectedCaseId":'..js(c.caseId)..',"expectedEventPc":'..nullable(expectation.event)..',"expectedCallPc":'..nullable(expectation.call)..',"expectedTargetPc":'..nullable(expectation.target)..',"expectedReturnPc":'..nullable(expectation.returnPc)..',"rolesAtPc":'..roles_json(pc(),role)..',"observedRoles":[]}'
 local payload='{"owner":"church-save-lifecycle","caseId":'..js(c.caseId)..',"phase":'..js(phase)..',"role":'..js(role)..',"actualPc":'..pc()..',"expectedEventPc":'..nullable(expectation.event)..',"expectedCallPc":'..nullable(expectation.call)..',"expectedTargetPc":'..nullable(expectation.target)..',"expectedReturnPc":'..nullable(expectation.returnPc)..',"pendingCallback":'..pending..',"restoration":'..restoration_json(state,generated_ok,frame_ok,#event_ids==0,removed)..',"restorationMismatch":'..mismatch_json()..',"error":'..js(tostring(message))..'}'
 status(config.observerFailureContract.statusPrefix..payload);client.exitCode(config.observerFailureContract.exitCode)
end
local function expect(ok,message)if not ok then error(message)end end
local function require_mode(expected)
 expect(current~=nil and mode==expected,"Church Save unexpected "..role.." callback: mode="..mode..", expected="..expected)
end
local function begin_case(i)
 expect(mode=="case-entry" and i==index+1,"Church Save case-entry mode/order drift")
 index=i;current=active_case();case_frames=0;first_mismatch=nil
 local r=s.ram;local start=slot_start(current);snapshot={currentMap=u8(r.currentMap),egressMap=u8(r.egressMap),currentSaveSlot=u16(r.currentSaveSlot),flag399=u8(r.flag399Byte),slot={},checksum=u8(current.selector==0 and s.saveGame.slot1Checksum or s.saveGame.slot2Checksum),saveFlags=u8(s.saveGame.saveFlags),currentPortrait=u16(r.currentPortrait),dialogueName=u16(r.dialogueName),dialogueNumber=u32(r.dialogueNumber)}
 for i=0,s.saveGame.actualStoredBytes-1 do snapshot.slot[i+1]=u8(start+i*2)end
 local mask=s.mutations.flag399.mask;local flag=snapshot.flag399
 if current.flag399InitiallySet then flag=flag|mask else flag=flag&(~mask&0xFF)end
 w8(r.currentMap,current.currentMap);w8(r.egressMap,255-current.currentMap);w16(r.currentSaveSlot,current.selector);w8(r.flag399Byte,flag)
 w16(h.actionStub,0x7003);w16(h.actionStub+2,0x4E75)
 records[i]={roles={},terminal=nil,egressBefore=255-current.currentMap,flagBefore=current.flag399InitiallySet,saveGame=false}
 mode="church-entry"
 status("milestone:case-entry:"..current.caseId)
end
local function prompt_answer(number)
 local answer=current.promptResults[number];expect(answer~=nil,"unexpected Church Save prompt")
 w16(h.promptStub,answer==0 and 0x7000 or 0x70FF);w16(h.promptStub+2,0x4E75)
 return answer
end
local function roles_json_record(r)
 local out={};for _,name in ipairs(r.roles)do out[#out+1]=js(name)end;return "["..table.concat(out,",").."]"
end
local function observed()
 records[index].roles[#records[index].roles+1]=role
 status("milestone:"..role..":"..current.caseId)
end
local function finish_case(i)
 local r=records[i];local c=config.cases[i]
 expect(r.roles[1]=="church-entry" and r.roles[2]=="start-save","ChurchMenu/@StartSave callback observation drift")
 if c.terminal=="exit-save" then expect(not r.saveGame and r.terminal=="exit-save","negative save terminal drift") else expect(r.saveGame,"positive case missed original SaveGame")end
 if c.terminal=="witch-suspend-entry" then expect(r.terminal=="witch-suspend-entry","suspend boundary drift") else expect(r.terminal==c.terminal,"Church terminal drift")end
 local egress=u8(s.ram.egressMap);local flag=(u8(s.ram.flag399Byte)&s.mutations.flag399.mask)~=0
 if not r.saveGame then expect(egress==r.egressBefore and flag==r.flagBefore,"negative callback/state mutation") else expect(egress==c.currentMap and flag,"save mutation order/result drift: egress="..egress..", expected="..c.currentMap..", flag="..tostring(flag))end
 records[i].json='{"caseId":'..js(c.caseId)..',"churchEntryPc":'..s.addresses.churchMenu..',"startSavePc":'..s.addresses.startSave..',"saveGame":'..bool(r.saveGame)..',"terminal":'..js(r.terminal)..',"egressMapBefore":'..r.egressBefore..',"egressMapAfter":'..egress..',"flag399Before":'..bool(r.flagBefore)..',"flag399After":'..bool(flag)..',"chronology":'..roles_json_record(r)..'}'
 local restored=restore_case();expect(restored.currentMap and restored.egressMap and restored.currentSaveSlot and restored.flag399 and restored.slotSram and restored.checksum and restored.saveFlags and restored.dialoguePortraitScratch,"scoped case restoration drift")
 if i<#config.cases then mode="case-entry";w16(h.resultStub,0x4EF9);w32(h.resultStub+2,epc(i+1))else mode="terminal-finalize";write_terminal()end
end
local function finalize_success()
 expect(index==#config.cases,"terminal finalize before last case");expect(restore_frame(),"bootstrap-frame restoration drift");expect(restore_generated(),"generated span restoration drift")
 remove_callbacks();local out='{"system":"sf2-church-save-lifecycle-runtime-v1","caseOrder":[';for i,id in ipairs(config.caseOrder)do out=out..(i>1 and ',' or '')..js(id)end;out=out..'],"records":[';for i,r in ipairs(records)do out=out..(i>1 and ',' or '')..r.json end;out=out..'],"callbacksCleared":true,"restoration":{"currentMap":true,"egressMap":true,"currentSaveSlot":true,"flag399":true,"slotSram":true,"checksum":true,"saveFlags":true,"dialoguePortraitScratch":true,"generatedRam":true,"bootstrapFrame":true,"sessionCartPatches":true}}';local f=assert(io.open(config.outputPath,"w"));f:write(out);f:close();status("milestone:callbacks-cleared:0");status("milestone:observer-finished");client.exitCode(0)
end
function write_terminal()
 w16(h.terminalStub,0x2C7C);w32(h.terminalStub+2,bootstrap_frame.a6);w16(h.terminalStub+6,0x2E7C);w32(h.terminalStub+8,bootstrap_frame.returnA7);w16(h.terminalStub+12,0x4EF9);w32(h.terminalStub+14,h.terminalStub+12)
end
function write_harness()
 for i,_ in ipairs(config.cases)do local a=epc(i);w16(a,0x2C7C);w32(a+2,h.harnessBase+0x180);w16(a+6,0x2E7C);w32(a+8,h.stackTop);w16(a+12,0x4EB9);w32(a+14,s.addresses.churchMenu);w16(a+20,0x4E71);w16(a+22,0x4E71);w16(a+24,0x4EF9);w32(a+26,h.resultStub)end
 w16(h.resultStub,0x4EF9);w32(h.resultStub+2,h.resultStub)
end
local function set_expectation(address,name)
 expectation={event=address,call=nil,target=nil,returnPc=nil,kind="event"}
 if name=="first-prompt-call" or name=="first-prompt-return" then local x=s.prompts.first;expectation={event=address,call=x.call,target=x.target,returnPc=x["return"],kind="prompt"}
 elseif name=="save-game-call" or name=="save-game-entry" or name=="save-game-rts" or name=="save-game-return" then local x=s.saveGame;expectation={event=address,call=x.call,target=x.target,returnPc=x["return"],kind="save"}
 elseif name=="post-save-prompt-call" or name=="post-save-prompt-return" then local x=s.prompts.postSave;expectation={event=address,call=x.call,target=x.target,returnPc=x["return"],kind="prompt"}
 elseif name=="fade-call" or name=="fade-entry" or name=="fade-return" then local x=s.suspendBoundary;expectation={event=address,call=x.fadeCall,target=x.fadeTarget,returnPc=x.fadeReturn,kind="fade"}
 elseif name=="witch-tail-jump" or name=="witch-suspend-entry" then local x=s.suspendBoundary;expectation={event=address,call=x.witchTailJump,target=x.witchTarget,returnPc=nil,kind="witch"}end
end
local function dispatch(address,entry)
 phase,role=entry.role,entry.role;set_expectation(address,role)
 if role~="bootstrap-check-sram" and not booted then return end
 if role=="bootstrap-check-sram" then
  local stack=emu.getregister("M68K A7")&0xFFFFFF;bootstrap_frame={a6=emu.getregister("M68K A6")&0xFFFFFF,returnA7=(stack+4)&0xFFFFFF};generated={}
  for _,span in ipairs({{address=h.harnessBase,width=h.generatedHarnessBytes},{address=h.actionStub,width=h.generatedActionBytes},{address=h.promptStub,width=h.generatedPromptBytes},{address=h.resultStub,width=h.generatedResultBytes},{address=h.terminalStub,width=h.generatedTerminalBytes}})do local bytes={};for i=0,span.width-1 do bytes[i+1]=u8(span.address+i)end;generated[#generated+1]={address=span.address,bytes=bytes}end
  restoration_scope_armed=true;write_harness();w32(stack,h.harnessBase);booted=true;mode="case-entry";status("milestone:direct-function-probe")
 elseif role=="case-entry" then begin_case(entry.index)
 elseif role=="church-entry" then require_mode("church-entry");observed();mode="action-return"
 elseif role=="action-return" then require_mode("action-return");expect((emu.getregister("M68K D0")&0xFFFF)==3,"save action result drift");mode="start-save"
 elseif role=="start-save" then require_mode("start-save");observed();mode="first-prompt-call"
 elseif role=="first-prompt-call" then require_mode("first-prompt-call");observed();prompt_answer(1);mode="first-prompt-return"
 elseif role=="first-prompt-return" then require_mode("first-prompt-return");observed();mode=current.promptResults[1]==0 and "do-save-game" or "exit-save"
 elseif role=="do-save-game" then require_mode("do-save-game");observed();mode="save-game-call"
 elseif role=="save-game-call" then require_mode("save-game-call");observed();expect((emu.getregister("M68K D0")&0xFFFF)==current.selector,"SaveGame selector drift");mode="save-game-entry"
 elseif role=="save-game-entry" then require_mode("save-game-entry");observed();records[index].saveGame=true;mode="save-game-rts"
 elseif role=="save-game-rts" then require_mode("save-game-rts");observed();mode="save-game-return"
 elseif role=="save-game-return" then require_mode("save-game-return");observed();mode="post-save-prompt-call"
 elseif role=="post-save-prompt-call" then require_mode("post-save-prompt-call");observed();prompt_answer(2);mode="post-save-prompt-return"
 elseif role=="post-save-prompt-return" then require_mode("post-save-prompt-return");observed();mode=current.promptResults[2]==0 and "exit-menu" or "fade-call"
 elseif role=="exit-save" then require_mode("exit-save");observed();records[index].terminal="exit-save";mode="case-result"
 elseif role=="exit-menu" then require_mode("exit-menu");observed();records[index].terminal="exit-menu";mode="case-result"
 elseif role=="fade-call" then require_mode("fade-call");observed();mode="fade-entry"
 elseif role=="fade-entry" then require_mode("fade-entry");observed();mode="fade-return"
 elseif role=="fade-return" then require_mode("fade-return");observed();mode="witch-tail-jump"
 elseif role=="witch-tail-jump" then require_mode("witch-tail-jump");observed();mode="witch-suspend-entry"
 elseif role=="witch-suspend-entry" then require_mode("witch-suspend-entry");observed();records[index].terminal="witch-suspend-entry";finish_case(index)
 elseif role=="case-result" then require_mode("case-result");finish_case(index)
 elseif role=="terminal-finalize" then require_mode("terminal-finalize");finalize_success()
 else error("unknown deterministic dispatch role: "..role)end
end
local function register(address,name,i)
 if callbacks[address]==nil then callbacks[address]={};event_ids[#event_ids+1]=event.on_bus_exec(function()if failed then return end;local ok,message=pcall(function()for _,entry in ipairs(callbacks[address])do dispatch(address,entry)end end);if not ok then failure(message)end end,address,"church-save-"..address,"M68K BUS")end
 callbacks[address][#callbacks[address]+1]={role=name,index=i}
end
local function register_callbacks()
 for i,_ in ipairs(config.cases)do register(epc(i),"case-entry",i)end
 register(h.resultStub,"case-result",0)
 register(h.terminalStub+12,"terminal-finalize",0);register(h.checkSram,"bootstrap-check-sram",0)
 register(s.addresses.churchMenu,"church-entry",0);register(s.addresses.startSave,"start-save",0);register(s.addresses.actionReturn,"action-return",0)
 register(s.prompts.first.call,"first-prompt-call",0);register(s.prompts.first["return"],"first-prompt-return",0);register(s.addresses.doSaveGame,"do-save-game",0)
 register(s.saveGame.call,"save-game-call",0);register(s.saveGame.target,"save-game-entry",0);register(s.saveGame.rts,"save-game-rts",0);register(s.saveGame["return"],"save-game-return",0)
 register(s.prompts.postSave.call,"post-save-prompt-call",0);register(s.prompts.postSave["return"],"post-save-prompt-return",0);register(s.addresses.exitSave,"exit-save",0);register(s.addresses.exitMenu,"exit-menu",0)
 register(s.suspendBoundary.fadeCall,"fade-call",0);register(s.suspendBoundary.fadeTarget,"fade-entry",0);register(s.suspendBoundary.fadeReturn,"fade-return",0);register(s.suspendBoundary.fadeReturn,"witch-tail-jump",0);register(s.suspendBoundary.witchTarget,"witch-suspend-entry",0)
end
local ok,message=pcall(function()register_callbacks();status("milestone:observer-loaded");status("milestone:direct-function-probe-armed")end);if not ok then failure(message)end
while true do
 frames=frames+1;if booted and index>0 then case_frames=case_frames+1 end;joypad.set({Start=true},1);joypad.set({},2)
 if not booted and frames>h.bootstrapFrameBudget then phase="bootstrap-watchdog";role="bootstrap-watchdog";expectation={event=nil,call=nil,target=nil,returnPc=nil,kind="watchdog"};failure("bootstrap watchdog exhausted")end
 if booted and index>0 and case_frames>h.caseFrameBudget then phase="case-watchdog";role="case-watchdog";expectation={event=nil,call=nil,target=nil,returnPc=nil,kind="watchdog"};failure("case watchdog exhausted for "..current.caseId)end
 emu.frameadvance()
end
