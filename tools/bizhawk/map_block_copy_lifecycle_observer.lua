local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local json=assert(loadfile(config.jsonModulePath))()
local ids,registered={},{}
local stage,prompt_count,case_index="cheat",0,1
local queue,records={},{}
local state,pending_save,pending_replay,pending_finish=nil,false,false,false
local restoration_scope=nil
local active,failed,current_role,current_phase,current_pc=false,false,"registration","registration",nil
local update_seen,convert_seen,helper_call,helper_entry,helper_return,tail_seen=false,false,false,false,false,false
local helper_call_address,helper_entry_address,helper_return_address,tail_address=nil,nil,nil,nil
local names={[1]="Up",[2]="Down",[4]="Left",[8]="Right",[16]="B",[32]="C"}
local cheat={1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4}

local function status(v)local f=assert(io.open(config.statusPath,"a"));f:write(v.."\n");f:close()end
local function current()return config.cases[case_index]end
local function word(v)return v&0xFFFF end
local function layout_address(x,y)return config.ram.layoutBase+(y*config.constants.rowWords+x)*config.constants.wordBytes end
local function entity_address()return config.ram.entityData end
local function enqueue(name,count)for _=1,count do queue[#queue+1]=name end end
local function pulse(name)enqueue("",30);enqueue(name,4);enqueue("",8)end
local function buttons(name)local b={};if name and name~="" then b[name]=true end;joypad.set(b,1)end
local function clear_events()for i=#ids,1,-1 do event.unregisterbyid(ids[i]);ids[i]=nil end end
local function pending()return {active=active,caseId=current() and current().id or json.null,role=current_role,phase=current_phase,helperCallObserved=helper_call,helperEntryObserved=helper_entry,helperReturnObserved=helper_return,dispatcherTailObserved=tail_seen}end
local function dispatcher_case(case)return case and case.kind:sub(1,11)=="dispatcher-" end
local function helper_expectation(case)
  if not case then return nil end
  if case.kind=="dispatcher-show" then return {callSiteAddress=config["function"].performCallPc,targetAddress=config["function"].performPc,returnAddress=config["function"].performReturnPc} end
  if case.kind=="dispatcher-hide" then return {callSiteAddress=config["function"].hideCallPc,targetAddress=config["function"].hidePc,returnAddress=config["function"].hideReturnPc} end
  if case.kind:sub(1,8)=="perform-" then return {callSiteAddress=case.generatedCallSiteAddress,targetAddress=config["function"].performPc,returnAddress=case.generatedReturnAddress} end
  if case.kind:sub(1,5)=="csub-" then return {callSiteAddress=case.generatedCallSiteAddress,targetAddress=config["function"].hidePc,returnAddress=case.generatedReturnAddress} end
  return nil
end
local function callback_expectation()
  local case=current();local h=helper_expectation(case)
  if current_role=="helper-call" then return h and h.callSiteAddress or nil end
  if current_role=="helper-entry" then return h and h.targetAddress or nil end
  if current_role=="helper-return" then return h and h.returnAddress or nil end
  if current_role=="dispatcher-tail" then return dispatcher_case(case) and config["function"].dispatcherTailPc or nil end
  return nil
end
local function failure(message)
  if failed then return end;failed=true
  local h=helper_expectation(current());local event=callback_expectation();local actual=current_pc or emu.getregister("M68K PC")
  local restored,count,mismatch=restore_scope();clear_events();local callbacks=#ids;os.remove(config.outputPath)
  if state then memorysavestate.removestate(state);state=nil end
  local p={owner=config.observerFailureContract.owner,caseId=current() and current().id or json.null,phase=current_phase,role=current_role,actualPc=actual,actualEventAddress=actual,actualCallSiteAddress=current_role=="helper-call" and actual or json.null,actualTargetAddress=current_role=="helper-entry" and actual or json.null,actualReturnAddress=current_role=="helper-return" and actual or json.null,expectedEventAddress=event or json.null,expectedCallSiteAddress=h and h.callSiteAddress or json.null,expectedTargetAddress=h and h.targetAddress or json.null,expectedReturnAddress=h and h.returnAddress or json.null,pendingCallback=pending(),callbacksRemaining=callbacks,error=tostring(message),restoration={attempted=true,verified=restored,cellCount=count,mismatch=mismatch or json.null},cleanup={outputRemoved=true,callbacksCleared=callbacks==0}}
  status(config.observerFailureContract.statusPrefix..json.encode(p));print(config.observerFailureContract.statusPrefix..json.encode(p));client.exitCode(config.observerFailureContract.exitCode)
end
local function register(address,phase,callback)
  if registered[address] then error("map-block-copy lifecycle duplicate physical-PC callback: "..address) end
  registered[address]=true
  ids[#ids+1]=event.on_bus_exec(function()
    if failed then return end
    current_phase=phase;current_role="unresolved:"..phase;current_pc=address
    local ok,message=pcall(callback);if not ok then failure(message) end
  end,address,"map-block-copy-lifecycle-"..phase,"M68K BUS")
end
local function write_word(address,value)memory.write_u16_be(address,value,"M68K BUS")end
local function write_long(address,value)memory.write_u32_be(address,value,"M68K BUS")end
local function read_word(address)return memory.read_u16_be(address,"M68K BUS")end
local function read_value(address,width)
  if width==1 then return memory.read_u8(address,"M68K BUS") end
  if width==2 then return read_word(address) end
  if width==4 then return memory.read_u32_be(address,"M68K BUS") end
  error("map-block-copy lifecycle restoration width="..tostring(width))
end
local function write_value(address,width,value)
  if width==1 then memory.write_u8(address,value,"M68K BUS");return end
  if width==2 then write_word(address,value);return end
  if width==4 then write_long(address,value);return end
  error("map-block-copy lifecycle restoration width="..tostring(width))
end
local function snapshot_scope(case)
  restoration_scope={}
  for _,cell in ipairs(case.restorationPlan) do restoration_scope[#restoration_scope+1]={address=cell.address,width=cell.width,value=read_value(cell.address,cell.width)} end
end
local function restore_scope()
  if restoration_scope==nil then return true,0,nil end
  for _,cell in ipairs(restoration_scope) do write_value(cell.address,cell.width,cell.value) end
  for _,cell in ipairs(restoration_scope) do
    local actual=read_value(cell.address,cell.width)
    if actual~=cell.value then return false,#restoration_scope,{address=cell.address,width=cell.width,expected=cell.value,actual=actual} end
  end
  local count=#restoration_scope;restoration_scope=nil;return true,count,nil
end
local function probe(case)
  local a=config.instrumentation.generatedProbeAddress
  if case.kind=="dispatcher-fading-skip" or case.kind=="dispatcher-neutral-flag" or case.kind=="dispatcher-show" or case.kind=="dispatcher-hide" then write_word(a,0x4E75);return end
  if case.kind=="csub-inactive-skip" or case.kind=="csub-active" then
    write_word(a,0x4EB8);write_word(a+2,config["function"].hidePc);write_word(a+4,0x4E75);return
  end
  local x,y=0,0
  if case.kind=="perform-matched-positive" then x,y=24,26 elseif case.kind=="perform-matched-negative" then x,y=4,8 end
  write_word(a,0x303C);write_word(a+2,x*config.constants.mapTileSize);write_word(a+4,0x323C);write_word(a+6,y*config.constants.mapTileSize);write_word(a+8,0x4EB8);write_word(a+10,config["function"].performPc);write_word(a+12,0x4E75)
end
local function seed_saved_rectangle(case)
  if case.kind~="csub-active" and case.kind~="dispatcher-hide" then return end
  local roof=config.sourceFacts.selectedRoofRecords.positive;local d=roof.destination;local size=roof.dimensions;local base=config.ram.savedRectangleMetadata
  write_word(base,d.x);write_word(base+2,d.y);write_word(base+4,size.width-1);write_word(base+6,size.height-1)
  local p=config.ram.savedRectangleBuffer
  local n=0
  for row=0,size.height-1 do for column=0,size.width-1 do write_word(p+n*2,0x5100+row*16+column);n=n+1 end end
  write_word(p+n*2,case.sentinelSeed)
end
local function setup(case)
  local e=entity_address();local pos=case.entityCoordinate or {x=0,y=0};write_word(e+config.constants.entityXOffset,pos.x*config.constants.mapTileSize);write_word(e+config.constants.entityYOffset,pos.y*config.constants.mapTileSize);write_word(e+config.constants.entityXDestinationOffset,pos.x*config.constants.mapTileSize);write_word(e+config.constants.entityYDestinationOffset,pos.y*config.constants.mapTileSize)
  write_long(e+config.constants.entityActscriptOffset,case.kind:sub(1,11)=="dispatcher-" and config.instrumentation.actionScriptAddress or 0)
  write_word(config.instrumentation.actionScriptAddress,config.sourceFacts.macroOpcode)
  memory.write_u8(config.ram.currentMap,case.mapIndex,"M68K BUS");memory.write_u8(config.ram.currentBattle,config.constants.notCurrentlyInBattle,"M68K BUS");memory.write_u8(config.ram.fadingSetting,case.fadingSeed,"M68K BUS");write_word(config.ram.busyWord,case.busySeed);write_word(config.ram.savedRectangleBuffer,case.sentinelSeed);memory.write_u8(config.ram.updateToggle,0,"M68K BUS")
  for _,seed in ipairs(case.layoutSeeds) do write_word(seed.address,seed.value) end
  if case.blockWord then write_word(layout_address(pos.x,pos.y),case.blockWord) end
  seed_saved_rectangle(case);probe(case)
end
local function begin()
  current_role="vint-entry";local case=current();if active or not case then return end
  snapshot_scope(case);setup(case);active=true;update_seen=false;convert_seen=false;helper_call=false;helper_entry=false;helper_return=false;tail_seen=false;helper_call_address=nil;helper_entry_address=nil;helper_return_address=nil;tail_address=nil;status("milestone:case:"..case.id)
end
local function check_helper(role,address)
  current_role=role;if not active then return end
  local want=helper_expectation(current())
  if want==nil then error("unexpected helper callback actual="..address) end
  if role=="helper-call" then if address~=want.callSiteAddress then error("helper call expected="..want.callSiteAddress..", actual="..address) end;helper_call=true;helper_call_address=address end
  if role=="helper-entry" then if address~=want.targetAddress then error("helper target expected="..want.targetAddress..", actual="..address) end;helper_entry=true;helper_entry_address=address end
  if role=="helper-return" then if address~=want.returnAddress then error("helper return expected="..want.returnAddress..", actual="..address) end;helper_return=true;helper_return_address=address end
end
local function complete()
  local case=current();local helper=helper_expectation(case)
  if dispatcher_case(case) and not update_seen then error("UpdateEntityData callback state drift") end
  if helper~=nil and (not helper_call or not helper_entry or not helper_return) then error("helper callback state drift") end
  if dispatcher_case(case) and not tail_seen then error("dispatcher tail callback state drift") end
  local reads={};for _,address in ipairs(case.layoutReadbackAddresses) do reads[#reads+1]={address=address,value=read_word(address)} end
  local observed_helper=json.null;if helper_call then observed_helper={callSiteAddress=helper_call_address,targetAddress=helper_entry_address,returnAddress=helper_return_address} end
  local record={id=case.id,kind=case.kind,updateEntityDataEntryObserved=update_seen,helperEvent=observed_helper,dispatcherTailAddressObserved=tail_seen and tail_address or json.null,updateToggleByteAfter=memory.read_u8(config.ram.updateToggle,"M68K BUS"),busyWordAfter=read_word(config.ram.busyWord),layoutReadbacks=reads,savedBufferSentinelAfter={address=case.sentinelAddress,value=read_word(case.sentinelAddress)}}
  local restored,count,mismatch=restore_scope();if not restored then error("restoration readback drift address="..mismatch.address..", expected="..mismatch.expected..", actual="..mismatch.actual) end
  records[#records+1]=record;status("milestone:restored:"..case.id);active=false;case_index=case_index+1;if case_index>#config.cases then pending_finish=true else pending_replay=true end
end
local function finish()
  local restored=restore_scope();if not restored then error("residual restoration scope") end
  clear_events();if #ids~=0 then error("map-block-copy lifecycle residual callback registration") end
  if state then memorysavestate.removestate(state);state=nil end
  status("milestone:callbacks-cleared:0")
  json.write(config.outputPath,{system=emu.getsystemid(),core="Genesis Plus GX",id=config.fixtureId,mapTest=config.mapTestIndex,recordOrder=(function()local out={};for _,r in ipairs(records)do out[#out+1]=r.id end;return out end)(),records=records})
  status("milestone:observer-finished");client.exitCode(0)
end
local function run()
  register(config.harness["function"].numberPromptAddress,"number-prompt",function()current_role="number-prompt";prompt_count=prompt_count+1;if prompt_count==1 then stage="map";pending_save=true;pulse("C")end end)
  register(config.harness["function"].flagPromptAddress,"flag-prompt",function()current_role="flag-prompt";pulse("B")end)
  register(config["function"].vintUpdateEntitiesPc,"vint-entry",begin)
  register(config["function"].updateEntityDataAddress,"update-entity-data",function()current_role="update-entity-data";if active and dispatcher_case(current()) then update_seen=true;status("milestone:update-entity-data:"..current().id)end end)
  register(config["function"].dispatcherPc,"dispatcher-entry",function()current_role="dispatcher-entry";if active and current().kind:sub(1,11)=="dispatcher-" then status("milestone:dispatcher:"..current().id) end end)
  register(config["function"].convertCallPc,"convert-call",function()current_role="convert-call";if active and current().kind:sub(1,11)=="dispatcher-" then convert_seen=true end end)
  register(config["function"].convertReturnPc,"convert-return",function()current_role="convert-return";if active and current().kind:sub(1,11)=="dispatcher-" and not convert_seen then error("coordinate return without call") end end)
  register(config["function"].performCallPc,"perform-call",function()check_helper("helper-call",config["function"].performCallPc)end)
  register(config["function"].hideCallPc,"hide-call",function()check_helper("helper-call",config["function"].hideCallPc)end)
  register(config.instrumentation.generatedProbeAddress+8,"probe-perform-call",function()check_helper("helper-call",config.instrumentation.generatedProbeAddress+8)end)
  register(config.instrumentation.generatedProbeAddress,"probe-hide-call",function()if active and (current().kind=="csub-inactive-skip" or current().kind=="csub-active") then check_helper("helper-call",config.instrumentation.generatedProbeAddress) end end)
  register(config["function"].performPc,"perform-entry",function()check_helper("helper-entry",config["function"].performPc)end)
  register(config["function"].hidePc,"hide-entry",function()check_helper("helper-entry",config["function"].hidePc)end)
  register(config["function"].performReturnPc,"perform-return",function()check_helper("helper-return",config["function"].performReturnPc)end)
  register(config.instrumentation.generatedProbeAddress+12,"probe-perform-return",function()check_helper("helper-return",config.instrumentation.generatedProbeAddress+12)end)
  register(config.instrumentation.generatedProbeAddress+4,"probe-hide-return",function()if active and (current().kind=="csub-inactive-skip" or current().kind=="csub-active") then check_helper("helper-return",config.instrumentation.generatedProbeAddress+4) end end)
  register(config["function"].dispatcherTailPc,"dispatcher-tail",function()current_role="dispatcher-tail";if active and dispatcher_case(current()) then if current().kind=="dispatcher-hide" then check_helper("helper-return",config["function"].hideReturnPc) end;tail_seen=true;tail_address=config["function"].dispatcherTailPc end end)
  register(config["function"].hideReturnPc+20,"dispatcher-clear-pointer",function()current_role="dispatcher-clear-pointer";if active and current().kind:sub(1,11)=="dispatcher-" then write_long(entity_address()+config.constants.entityActscriptOffset,0);complete() end end)
  register(config.instrumentation.probeReturnAddress,"direct-return",function()current_role="direct-return";if active and current().kind:sub(1,11)~="dispatcher-" then complete() end end)
  status("milestone:observer-ready")
  local frames=0
  while true do
    frames=frames+1
    if pending_finish then finish();return elseif pending_save then pending_save=false;state=memorysavestate.savecorestate();status("milestone:saved-map-prompt") elseif pending_replay then pending_replay=false;memorysavestate.loadcorestate(state);queue={};pulse("C");status("milestone:replay-map-prompt") end
    if frames>=config.maxFrames then error("frame budget exhausted case="..case_index) end
    local button=nil;if stage=="cheat" then local p=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS");if p>=0x28FF0 and p<0x29000 then button=names[cheat[p-0x28FF0+1]] elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Down" end elseif #queue>0 then button=table.remove(queue,1)end
    buttons(button);joypad.set({},2);emu.frameadvance()
  end
end
local ok,message=pcall(run);if not ok then failure(message) end
