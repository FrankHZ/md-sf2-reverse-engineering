local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local json=assert(loadfile(config.jsonModulePath))()
local stage,prompts,index,frames="cheat",0,1,0
local queue,records,callbacks,event_ids={}, {}, {}, {}
local replay,active,pending_save,pending_replay,pending_finish=nil,false,false,false,false
local current_phase,current_role,current_expectation="registration","registration",nil
local failed,cleaned,probe_milestone=false,false,false
local case_before,case_before_list=nil,nil
local snapshots={};local touched={};local names={[1]="Up",[2]="Down",[4]="Left",[8]="Right",[16]="B",[32]="C"}
local cheat={1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4}
local function q(v)return string.format("%q",v)end
local function bool(v)return v and "true" or "false"end
local function nullable(v)return v==nil and "null" or tostring(v)end
local function status(v)local f=assert(io.open(config.statusPath,"a"));f:write(v.."\n");f:close()end
local function enqueue(v,n)for _=1,n do queue[#queue+1]=v end end
local function pulse(v)enqueue("",30);enqueue(v,4);enqueue("",8)end
local function button(v)local b={};if v and v~="" then b[v]=true end;joypad.set(b,1)end
local function pc()return emu.getregister("M68K PC")&0xFFFFFF end
local function a7()return emu.getregister("M68K A7")&0xFFFFFF end
local function word(n)return emu.getregister("M68K "..n)&0xFFFF end
local function current()return config.cases[index]end
local function input()return config.caseInputs[index]end
local function ram8(a)return memory.read_u8(a,"M68K BUS")end
local function put8(a,v)memory.write_u8(a,v&0xFF,"M68K BUS")end
local function ram16(a)return memory.read_u16_be(a,"M68K BUS")end
local function ram32(a)return memory.read_u32_be(a,"M68K BUS")end
local function put16(a,v)memory.write_u16_be(a,v&0xFFFF,"M68K BUS")end
local function put32(a,v)memory.write_u32_be(a,v&0xFFFFFFFF,"M68K BUS")end
local function sram_offset(a)return a-0x200000 end
local function sram8(a)return memory.read_u8(sram_offset(a),"SRAM")end
local function sramput(a,v)memory.write_u8(sram_offset(a),v&0xFF,"SRAM")end
local function copy(a,n,space,stride)
  local b={};stride=stride or 1
  for i=0,n-1 do b[#b+1]=space=="SRAM" and sram8(a+i*stride) or ram8(a+i)end
  return {address=a,bytes=b,space=space,stride=stride}
end
local function restore(s,name)
  if not s then return true,nil end
  for i,v in ipairs(s.bytes) do local a=s.address+(i-1)*s.stride;if s.space=="SRAM" then sramput(a,v)else put8(a,v)end end
  for i,v in ipairs(s.bytes) do local a=s.address+(i-1)*s.stride;local actual=s.space=="SRAM" and sram8(a)or ram8(a);if actual~=v then return false,{domain=name,address=a,expected=v,actual=actual}end end
  return true,nil
end
local function snapshot_once(name,a,n,space,stride)if not snapshots[name] then snapshots[name]=copy(a,n,space,stride)end end
local function snapshot_addresses(name,addresses)
  if snapshots[name] then return end
  local rows={};for _,address in ipairs(addresses)do rows[#rows+1]={address=address,byte=ram8(address)}end;snapshots[name]=rows
end
local function restore_addresses(rows,name)
  if not rows then return true,nil end
  for _,row in ipairs(rows)do put8(row.address,row.byte)end
  for _,row in ipairs(rows)do local actual=ram8(row.address);if actual~=row.byte then return false,{domain=name,address=row.address,expected=row.byte,actual=actual}end end
  return true,nil
end
local function roles(a)local t={};for _,e in ipairs(callbacks[a]or{})do t[#t+1]=q(e.role)end;return "["..table.concat(t,",").."]"end
local function pending()
  local e=current_expectation or {};return "{\"active\":"..bool(active)..",\"caseIndex\":"..index..",\"expectedEventPc\":"..nullable(e.eventPc)..",\"expectedCallPc\":"..nullable(e.callPc)..",\"expectedTargetPc\":"..nullable(e.targetPc)..",\"expectedReturnPc\":"..nullable(e.returnPc)..",\"rolesAtPc\":"..roles(pc()).."}"
end
local function unregister()for i=#event_ids,1,-1 do event.unregisterbyid(event_ids[i]);event_ids[i]=nil end end
local function cleanup()if cleaned then return end;cleaned=true;unregister();if replay then memorysavestate.removestate(replay);replay=nil end end
local function restore_scopes()
  local order={"roster","hp","combatantX","list","listLength","program","stream","pointer","sram","checksum","saveFlags"}
  for _,name in ipairs(order)do
    local ok,mismatch
    if name=="combatantX" then ok,mismatch=restore_addresses(snapshots[name],name)else ok,mismatch=restore(snapshots[name],name)end
    if not ok then return false,mismatch end
  end
  return true,nil
end
local function mismatch(v)if not v then return "null"end;return "{\"domain\":"..q(v.domain)..",\"address\":"..v.address..",\"expected\":"..v.expected..",\"actual\":"..v.actual.."}"end
local function fail(message,forced)
  if failed then return end;failed=true;os.remove(config.outputPath)
  if replay then memorysavestate.loadcorestate(replay)end
  local ok,bad=restore_scopes();bad=forced or bad;local c=current();cleanup()
  local body="{\"owner\":"..q(config.observerFailureContract.owner)..",\"caseId\":"..(c and q(c.id)or"null")..",\"phase\":"..q(current_phase)..",\"role\":"..q(current_role)..",\"actualPc\":"..pc()..",\"expectedEventPc\":"..nullable((current_expectation or {}).eventPc)..",\"expectedCallPc\":"..nullable((current_expectation or {}).callPc)..",\"expectedTargetPc\":"..nullable((current_expectation or {}).targetPc)..",\"expectedReturnPc\":"..nullable((current_expectation or {}).returnPc)..",\"pendingCallback\":"..pending()..",\"callbacksRemaining\":0,\"mutationState\":{\"rosterMutated\":"..bool(touched.roster)..",\"currentHpMutated\":"..bool(touched.hp)..",\"combatantXMutated\":"..bool(touched.combatantX)..",\"defeatedListMutated\":"..bool(touched.list)..",\"sramMutated\":"..bool(touched.sram)..",\"generatedProgramMutated\":"..bool(touched.program).."},\"outputRemoved\":true,\"sessionStateRestored\":"..bool(ok)..",\"restorationMismatch\":"..mismatch(bad)..",\"error\":"..q(tostring(message)).."}"
  local line=config.observerFailureContract.statusPrefix..body;status(line);print(line);client.exitCode(config.observerFailureContract.exitCode)
end
local function add(a,role,f)callbacks[a]=callbacks[a]or{};callbacks[a][#callbacks[a]+1]={role=role,handler=f}end
local function dispatch(a)
  for _,e in ipairs(callbacks[a]or{})do current_role=e.role;local ok,msg=pcall(e.handler);if not ok then fail(msg);return end end
end
local function install()for a,_ in pairs(callbacks)do event_ids[#event_ids+1]=event.on_bus_exec(function()dispatch(a)end,a,"roster-death-dispatch-"..a,"M68K BUS")end end
local function list()local s=config.runtimeContract.storage.defeatedList;local n=ram16(s.lengthAddress);local out={};for i=0,n-1 do out[#out+1]=ram8(s.baseAddress+i)end;return out,n end
local function list_equal(a,b)if #a~=#b then return false end;for i,v in ipairs(a)do if v~=b[i]then return false end end;return true end
local function state_snapshot()
  local i=input();local s=i.state;local r={};if s.joinedFlag then r.joined=ram8(s.joinedFlag.address)end
  if s.hp then r.hp=ram16(s.hp.address)end
  if s.defeatedList then local l,n=list();r.length=n;return r,l end
  return r,nil
end
local function bytes16(bytes,o,v)bytes[o+1]=(v>>8)&0xFF;bytes[o+2]=v&0xFF end
local function bytes32(bytes,o,v)bytes16(bytes,o,(v>>16)&0xFFFF);bytes16(bytes,o+2,v&0xFFFF)end
local function write_bytes(a,b)for i,v in ipairs(b)do put8(a+i-1,v)end end
local function arm_program()
  local g=config.instrumentation.generatedProgram;local b={};for i=1,g.byteCount do b[i]=0x4A end
  bytes16(b,0,0x2C7C);bytes32(b,2,g.streamAddress);bytes16(b,6,0x4EB9);bytes32(b,8,input().handlerAddress)
  if current().id=="csc08-join-absent" then
    bytes16(b,12,0x7000);bytes16(b,14,0x4EB9);bytes32(b,16,config.runtimeContract.services.SaveGame)
    bytes16(b,20,0x7000);bytes16(b,22,0x4EB9);bytes32(b,24,config.runtimeContract.services.LoadGame)
    bytes16(b,28,0x7205);bytes16(b,30,0x4EB9);bytes32(b,32,config.runtimeContract.services.CheckFlag);bytes16(b,36,0x4E75)
  else bytes16(b,12,0x4E75)end
  snapshot_once("program",g.address,g.byteCount,"RAM");snapshot_once("stream",g.streamAddress,16,"RAM");snapshot_once("pointer",config.instrumentation.trampoline.ramInputAddress,4,"RAM")
  write_bytes(g.address,b);for o,v in ipairs(input().streamBytes)do put8(g.streamAddress+o-1,v)end;put32(config.instrumentation.trampoline.ramInputAddress,g.address);touched.program=true
end
local function setup()
  local i=input();local s=i.state;local store=config.runtimeContract.storage;assert(current().id==i.id,"roster/death case input identity drift")
  if s.joinedFlag then snapshot_once("roster",s.joinedFlag.address,1,"RAM");put8(s.joinedFlag.address,s.joinedFlag.initialSet and s.joinedFlag.mask or 0);touched.roster=true end
  if s.hp then snapshot_once("hp",s.hp.address,2,"RAM");put16(s.hp.address,s.hp.value);touched.hp=true end
  if s.combatantX then snapshot_addresses("combatantX",s.combatantXAddresses);for o,v in ipairs(s.combatantX)do put8(s.combatantXAddresses[o],v)end;touched.combatantX=true end
  if s.defeatedList then local d=store.defeatedList;snapshot_once("list",d.baseAddress,s.listTouchedByteCount,"RAM");snapshot_once("listLength",d.lengthAddress,2,"RAM");for o,v in ipairs(s.defeatedList)do put8(d.baseAddress+o-1,v)end;put16(d.lengthAddress,#s.defeatedList);touched.list=true end
  if s.persistence then local p=store.saveLoad;local slot=p.slots[s.persistence.selector+1];snapshot_once("sram",slot.dataAddress,p.logicalRam.logicalByteCount,"SRAM",p.physicalByteStride);snapshot_once("checksum",slot.checksumAddress,1,"SRAM");snapshot_once("saveFlags",p.saveFlagsAddress,1,"SRAM");touched.sram=true end
  case_before,case_before_list=state_snapshot();arm_program();current_phase,current_role,current_expectation="handler-entry","handler-entry",{eventPc=i.handlerAddress,callPc=config.instrumentation.generatedProgram.address+6,targetPc=i.handlerAddress,returnPc=config.instrumentation.generatedProgram.address+12};active=true
end
local function handler_return()
  if not active then return end;current_phase,current_role="handler-return","handler-return";local after,after_list=state_snapshot();after.a6=emu.getregister("M68K A6")&0xFFFFFF
  local r={id=current().id,handlerAddress=current().handlerAddress,scope=current().scope,before=case_before,after=after,milestones={"handler-entry","handler-return"}}
  if case_before_list then r.listBefore=case_before_list;r.listAfter=after_list end;records[index]=r
end
local function poison_saved()
  local s=input().state.joinedFlag;local r=records[index];assert(active and s and current().id=="csc08-join-absent","unexpected SaveGame return")
  local p=input().state.persistence;local saved=ram8(s.address);local selected=sram8(p.selectedPhysicalAddress);local checksum=sram8(p.checksumAddress);local flags=sram8(p.saveFlagsAddress);assert((saved&s.mask)~=0,"join mutation did not set membership flag");assert(selected==saved,"SaveGame selected physical byte drift");assert((flags&p.occupiedFlagMask)~=0,"SaveGame occupied flag drift");put8(s.address,0);r.persistence={mode="roster-membership",saved=saved,poisoned=0,restored=0,checkFlagZero=true,selectedPhysicalAddress=p.selectedPhysicalAddress,selectedSramByte=selected,checksumAddress=p.checksumAddress,checksumByte=checksum,saveFlagsAddress=p.saveFlagsAddress,saveFlags=flags};r.milestones[#r.milestones+1]="save-return-poison"
end
local function load_return()
  local s=input().state.joinedFlag;local r=records[index];local restored=ram8(s.address);assert((restored&s.mask)~=0,"LoadGame did not restore membership flag");r.persistence.restored=restored;r.milestones[#r.milestones+1]="load-return"
end
local function finish_case()
  if not active then return end;local r=records[index];if current().id=="csc08-join-absent" then r.persistence.checkFlagZero=(word("SR")&4)~=0;assert(not r.persistence.checkFlagZero,"original CheckFlag did not observe restored membership")end
  r.milestones[#r.milestones+1]="program-return";active=false;index=index+1;if index>#config.cases then pending_finish=true else pending_replay=true end
end
local function finish()
  if replay then memorysavestate.loadcorestate(replay)end;local ok,bad=restore_scopes();if not ok then current_phase,current_role="cleanup","cleanup";fail("roster/death scoped restoration drift",bad);return end
  cleanup();status("milestone:force-state-roster-death-probe");status("milestone:callbacks-cleared:0");status("milestone:observer-finished")
  local order={};for _,c in ipairs(config.cases)do order[#order+1]=c.id end
  json.write(config.outputPath,{system=emu.getsystemid(),core="Genesis Plus GX",id=config.fixtureId,mapTest=config.mapTest,recordOrder=order,records=records});client.exitCode(0)
end
add(config.harness["function"].numberPromptAddress,"number-prompt",function()prompts=prompts+1;if prompts==1 then stage="map";pending_save=true;pulse("C")end end)
add(config.harness["function"].flagPromptAddress,"flag-prompt",function()pulse("B")end)
add(config.runtimeContract.entryAddress,"map-entry",function()if active then return end;setup()end)
for _,h in ipairs(config.runtimeContract.handlers)do
  add(h.handlerAddress,"handler-entry",function()if active then current_phase,current_role="handler-entry","handler-entry"end end)
  add(h.returnAddress,"handler-return",handler_return)
end
local g=config.instrumentation.generatedProgram
add(g.address+12,"program-return",function()if active and current().id~="csc08-join-absent"then finish_case()end end)
add(config.runtimeContract.services.SaveGame,"save-entry",function()if active and ram32(a7())==g.address+20 then current_phase,current_role="save-entry","save-entry"end end)
add(g.address+20,"save-return-poison",poison_saved)
add(config.runtimeContract.services.LoadGame,"load-entry",function()if active and ram32(a7())==g.address+28 then current_phase,current_role="load-entry","load-entry"end end)
add(g.address+28,"load-return",load_return)
add(config.runtimeContract.services.CheckFlag,"check-flag-entry",function()if active and ram32(a7())==g.address+36 then current_phase,current_role="check-flag-entry","check-flag-entry"end end)
add(g.address+36,"check-flag-return",finish_case)
install()
while true do
  frames=frames+1;if pending_finish then finish();return elseif pending_save then pending_save=false;replay=memorysavestate.savecorestate() elseif pending_replay then pending_replay=false;memorysavestate.loadcorestate(replay);queue={};pulse("C")end
  if frames>=config.maxFrames then fail("roster/death observer frame limit")end
  local b=nil;if stage=="cheat"then local p=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS");if p>=0x28FF0 and p<0x29000 then b=names[cheat[p-0x28FF0+1]]elseif ram8(config.harness.ram.debugModeAddress)==255 then b="Down"end elseif active then b="C" elseif #queue>0 then b=table.remove(queue,1)end
  button(b);joypad.set({},2);emu.frameadvance()
end
