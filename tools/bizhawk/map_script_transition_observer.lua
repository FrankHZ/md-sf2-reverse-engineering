local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local stage,prompt_count,case_index="cheat",0,1
local queue,records,event_ids,registered_addresses={},{},{},{}
local replay_state,pending_save,pending_replay,pending_finish=nil,false,false,false
local active,observer_failed,session_cleaned=false,false,false
local current_phase,current_role,current_pc="registration","registration",nil
local handler_entries,script_word_reads,service_calls,service_returns={},{},{},{}
local pending_service,dispatch_target,cursor_after_handler,handler_returned,end_reached=nil,nil,nil,false,false
local fallthrough_csc48,fade_setting_at_first_wait=false,nil
local json_null={}
local names={[1]="Up",[2]="Down",[4]="Left",[8]="Right",[16]="B",[32]="C"}
local cheat={1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4}

local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local buttons={};if name and name~="" then buttons[name]=true end;joypad.set(buttons,1) end
local function current_case() return config.cases[case_index] end
local function script_base() return config.instrumentation.ramInputAddress+4 end
local function nullable(value) if value==nil then return json_null end return value end
local function word(value) return value&0xFFFF end
local function bool(value) if value then return "true" end return "false" end
local function copy(value) if type(value)~="table" then return value end local result={};for key,item in pairs(value) do result[key]=copy(item) end;return result end
local function json_string(value) return string.format("%q",value) end
local function is_array(value) local count=0;for key,_ in pairs(value) do if type(key)~="number" then return false end;count=count+1 end;for i=1,count do if value[i]==nil then return false end end;return true end
local function json(value)
  local kind=type(value)
  if value==nil or value==json_null then return "null" end
  if kind=="boolean" then return bool(value) end
  if kind=="number" then return tostring(value) end
  if kind=="string" then return json_string(value) end
  if kind~="table" then error("map-script transition JSON type drift: "..kind) end
  local parts={}
  if is_array(value) then
    for _,item in ipairs(value) do parts[#parts+1]=json(item) end
    return "["..table.concat(parts,",").."]"
  end
  for key,item in pairs(value) do parts[#parts+1]=json_string(key)..":"..json(item) end
  return "{"..table.concat(parts,",").."}"
end

local function unregister_events() for index=#event_ids,1,-1 do event.unregisterbyid(event_ids[index]);event_ids[index]=nil end end
local function cleanup_session()
  if session_cleaned then return end
  session_cleaned=true;unregister_events()
  if replay_state then memorysavestate.removestate(replay_state);replay_state=nil end
end
local function pending_callback_state()
  return {active=active,phase=current_phase,role=current_role,handlerEntriesObserved=copy(handler_entries),scriptWordReadCount=#script_word_reads,dispatchTargetAddress=nullable(dispatch_target),pendingService=nullable(pending_service)}
end
local function expectation()
  local by_pc=config.failureExpectations[tostring(current_pc)]
  if type(by_pc)~="table" or type(by_pc.roles)~="table" then return {} end
  local expected=by_pc.roles[current_role]
  if type(expected)~="table" then return {} end
  return expected
end
local function fail_callback(message)
  if observer_failed then return end
  observer_failed=true
  local expected=expectation()
  local case=current_case()
  local payload={owner=config.observerFailureContract.owner,caseId=nullable(case and case.id or nil),phase=current_phase,actualPc=nullable(emu.getregister("M68K PC")),expectedCallSiteAddress=nullable(expected.callSiteAddress),expectedTargetAddress=nullable(expected.targetAddress),expectedReturnAddress=nullable(expected.returnAddress),pendingCallback=pending_callback_state(),error=tostring(message)}
  local diagnostic=config.observerFailureContract.statusPrefix..json(payload)
  status(diagnostic);print(diagnostic)
  if config.observerFailureContract.removeOutputBeforeExit then os.remove(config.outputPath) end
  cleanup_session();client.exitCode(config.observerFailureContract.exitCode)
end
local function set_role(role) current_role=role end
local function register_exec(address,phase,callback)
  if registered_addresses[address] then error("map-script transition duplicate physical-PC callback: "..address) end
  registered_addresses[address]=phase
  event_ids[#event_ids+1]=event.on_bus_exec(function()
    if observer_failed then return end
    current_phase=phase;current_role="unresolved:"..phase;current_pc=address
    local ok,message=pcall(callback)
    if not ok then fail_callback(message) end
  end,address,"map-script-transition-"..phase,"M68K BUS")
end
local function require_equal(actual,expected,message) if actual~=expected then error(message..": expected="..tostring(expected)..", actual="..tostring(actual)) end end
local function require_list(actual,expected,message)
  if #actual~=#expected then error(message.." count drift") end
  for index,value in ipairs(expected) do if actual[index]~=value then error(message.." order drift at "..index) end end
end
local function require_word_reads(actual,expected)
  if #actual~=#expected then error("map-script transition word-read count drift") end
  for index,value in ipairs(expected) do if actual[index].word~=value.word or actual[index].cursorAfterReadOffset~=value.cursorAfterReadOffset then error("map-script transition word-read identity/order drift at "..index) end end
end

local function reset_observations()
  handler_entries={};script_word_reads={};service_calls={};service_returns={};pending_service=nil;dispatch_target=nil;cursor_after_handler=nil;handler_returned=false;end_reached=false;fallthrough_csc48=false;fade_setting_at_first_wait=nil
end
local function begin_case()
  if active then error("map-script transition nested wrapper entry") end
  local case=current_case();if case==nil then error("map-script transition unexpected wrapper entry") end
  memory.write_u8(config.ram.currentMapAddress,case.initialCurrentMap,"M68K BUS")
  memory.write_u8(config.ram.viewTargetEntityAddress,case.viewTargetSeed,"M68K BUS")
  memory.write_u8(config.ram.fadingSettingAddress,0,"M68K BUS")
  memory.write_u16_be(config.ram.mapEventTypeAddress,0,"M68K BUS")
  for offset=0,4 do memory.write_u8(config.ram.mapEventParam1Address+offset,0,"M68K BUS") end
  for offset=0,31 do memory.write_u8(script_base()+offset,0,"M68K BUS") end
  for index,value in ipairs(case.scriptBytes) do memory.write_u8(script_base()+index-1,value,"M68K BUS") end
  reset_observations();active=true;status("milestone:case:"..case.id)
end
local function at_execute_entry()
  set_role("execute-entry");if not active then return end
  handler_entries[#handler_entries+1]="ExecuteMapScript";status("milestone:execute-entry:"..current_case().id)
end
local function at_script_word_read()
  set_role("script-word-read");if not active then return end
  script_word_reads[#script_word_reads+1]={word=word(emu.getregister("M68K D0")),cursorAfterReadOffset=emu.getregister("M68K A6")-script_base()}
end
local function at_dispatch()
  local case=current_case();if not active then set_role("dispatch:inactive");return end
  set_role("dispatch:"..case.id)
  local field=({warp="warpHandlerAddress",resetMap="resetHandlerAddress",loadMapFadeIn="fadeHandlerAddress",reloadMap="reloadHandlerAddress",mapLoad="mapLoadHandlerAddress"})[case.macro]
  dispatch_target=config["function"][field]
end
local function at_handler(macro,label)
  local case=current_case();if not active then set_role("handler:inactive");return end
  if case.macro~=macro then error("map-script transition unexpected handler: "..label) end
  set_role("handler:"..case.id);handler_entries[#handler_entries+1]=label;status("milestone:handler:"..case.id..":"..label)
end
local function at_csc48()
  local case=current_case();if not active then set_role("csc48:inactive");return end
  if case.macro=="loadMapFadeIn" then
    set_role("fallthrough:"..case.id);fallthrough_csc48=true;handler_entries[#handler_entries+1]="csc48_loadMap";status("milestone:fallthrough-csc48:"..case.id)
  elseif case.macro=="mapLoad" then at_handler("mapLoad","csc48_loadMap")
  else error("map-script transition unexpected csc48 entry") end
end
local function at_service_seam(address)
  local case=current_case();if not active then set_role("service:inactive");return end
  if pending_service and pending_service.returnAddress==address then
    set_role(pending_service.role..":return");service_returns[#service_returns+1]=pending_service.target;pending_service=nil
  end
  local next_index=#service_calls+1
  local site=case.serviceSites[next_index]
  if site and site.address==address then
    local base="service:"..case.id..":"..next_index..":"..site.target
    set_role(base..":call")
    pending_service={callSiteAddress=site.address,target=site.target,targetAddress=config.service[site.target],returnAddress=site.returnAddress,role=base}
    service_calls[#service_calls+1]=site.target
  end
end
local function at_service_entry(address)
  local case=current_case();if not active then set_role("service-entry:inactive");return end
  if pending_service and pending_service.targetAddress==address then
    set_role(pending_service.role..":entry")
    if pending_service.target=="WaitForVInt" and case.releaseFadeAtFirstWait and fade_setting_at_first_wait==nil then
      fade_setting_at_first_wait=memory.read_u8(config.ram.fadingSettingAddress,"M68K BUS")
      if fade_setting_at_first_wait~=0 then
        memory.write_u8(config.ram.fadingSettingAddress,0,"M68K BUS")
        status("milestone:fade-wait-released:"..case.id)
      else
        status("milestone:fade-wait-already-clear:"..case.id)
      end
    end
    return
  end
  if pending_service and pending_service.target=="LoadMap" then
    for _,site in ipairs(config.resetTail.nestedServiceSites) do
      if address==config.service[site.target] then
        set_role(pending_service.role..":nested:"..site.target..":entry")
        status("milestone:loadmap-nested-"..site.target.."-entry:"..case.id)
        return
      end
    end
  end
  if case.macro=="resetMap" and pending_service and pending_service.target=="ResetCurrentMap" then
    if address==config.service.LoadMap then
      set_role("reset-tail:LoadMap:entry:"..case.id);status("milestone:reset-tail-loadmap-entry:"..case.id);return
    end
    for _,site in ipairs(config.resetTail.nestedServiceSites) do
      if address==config.service[site.target] then
        set_role("reset-tail:"..site.target..":entry:"..case.id);status("milestone:reset-tail-"..site.target.."-entry:"..case.id);return
      end
    end
  end
  error("map-script transition unexpected service entry: "..address)
end
local function at_dispatch_return()
  if not active then set_role("dispatcher-return:inactive");return end
  set_role("dispatcher-return:"..current_case().id)
  if pending_service~=nil then error("map-script transition service return missing before dispatcher return") end
  handler_returned=true
  cursor_after_handler=emu.getregister("M68K A6")-script_base()
  require_equal(cursor_after_handler,current_case().expected.cursorAfterHandlerOffset,"map-script transition handler cursor")
end
local function at_end() set_role("script-end");if active then end_reached=true end end
local function append_record()
  local case=current_case();local expected=case.expected
  require_list(handler_entries,expected.handlerEntries,"map-script transition handlers")
  require_word_reads(script_word_reads,expected.scriptWordReads)
  require_equal(handler_returned,expected.handlerReturned,"map-script transition handler return")
  require_equal(fallthrough_csc48,expected.fallthroughCsc48Observed,"map-script transition csc48 fallthrough")
  require_equal(end_reached,true,"map-script transition end boundary")
  require_list(service_calls,expected.serviceCallOrder,"map-script transition service calls")
  require_list(service_returns,expected.serviceReturnOrder,"map-script transition service returns")
  local event_type,event_clear,event_payload=nil,nil,nil
  if case.macro=="warp" then
    event_type=memory.read_u16_be(config.ram.mapEventTypeAddress,"M68K BUS")
    event_clear=memory.read_u8(config.ram.mapEventParam1Address,"M68K BUS")
    event_payload={}
    for offset=1,4 do event_payload[#event_payload+1]=memory.read_u8(config.ram.mapEventParam1Address+offset,"M68K BUS") end
  end
  local record={id=case.id,macro=case.macro,handlerEntries=copy(handler_entries),scriptWordReads=copy(script_word_reads),cursorAfterHandlerOffset=cursor_after_handler,handlerReturned=handler_returned,fallthroughCsc48Observed=fallthrough_csc48,serviceCallOrder=copy(service_calls),currentMapAfter=memory.read_u8(config.ram.currentMapAddress,"M68K BUS"),viewTargetEntityAfter=memory.read_u8(config.ram.viewTargetEntityAddress,"M68K BUS"),viewPlaneAPixelX=case.macro=="warp" and json_null or memory.read_u16_be(config.ram.viewPlaneAPixelXAddress,"M68K BUS"),viewPlaneAPixelY=case.macro=="warp" and json_null or memory.read_u16_be(config.ram.viewPlaneAPixelYAddress,"M68K BUS"),mapEventTypeWordAfter=nullable(event_type),mapEventClearByteAfter=nullable(event_clear),mapEventPayloadBytesAfter=nullable(event_payload),fadeSettingAtFirstWait=nullable(fade_setting_at_first_wait),serviceReturnOrder=copy(service_returns)}
  for key,value in pairs(expected) do
    if type(value)~="table" and record[key]~=value then
      error("map-script transition runtime value drift: "..key..": expected="..tostring(value)..", actual="..tostring(record[key]))
    end
  end
  records[#records+1]=record
end
local function at_post_handler()
  set_role("trampoline-complete");if not active then return end
  append_record();status("milestone:trampoline-complete:"..current_case().id);active=false;case_index=case_index+1
  if case_index>#config.cases then pending_finish=true else pending_replay=true end
end
local function write_output()
  local f=assert(io.open(config.outputPath,"w"));f:write('{"system":"'..emu.getsystemid()..'","core":"Genesis Plus GX","id":"'..config.fixtureId..'","mapTest":'..config.mapTestIndex..',"recordOrder":[')
  for index,row in ipairs(records) do if index>1 then f:write(",") end;f:write(json_string(row.id)) end
  f:write('],"records":[');for index,row in ipairs(records) do if index>1 then f:write(",") end;f:write(json(row)) end;f:write("]}\n");f:close()
end
local function finish(exit_code)
  cleanup_session();if exit_code~=0 then client.exitCode(exit_code);return end
  if #event_ids~=0 then error("map-script transition residual callback registration") end
  status("milestone:callbacks-cleared:0");write_output();status("milestone:observer-finished");client.exitCode(0)
end
local function run()
  register_exec(config.harness["function"].numberPromptAddress,"number-prompt",function() set_role("number-prompt");prompt_count=prompt_count+1;if prompt_count==1 then stage="map";pending_save=true;pulse("C") end end)
  register_exec(config.harness["function"].flagPromptAddress,"flag-prompt",function() set_role("flag-prompt");pulse("B") end)
  register_exec(config["function"].entryAddress,"wrapper-entry",begin_case)
  register_exec(config.instrumentation.stubAddress,"trampoline-entry",function() set_role("trampoline-entry") end)
  register_exec(config["function"].executeMapScriptAddress,"execute-entry",at_execute_entry)
  register_exec(config["function"].scriptWordReadAfterAddress,"script-word-read",at_script_word_read)
  register_exec(config["function"].opcodeDispatchCallAddress,"opcode-dispatch",at_dispatch)
  register_exec(config["function"].opcodeDispatchReturnAddress,"dispatcher-return",at_dispatch_return)
  register_exec(config["function"].endAddress,"script-end",at_end)
  register_exec(config["function"].warpHandlerAddress,"warp-handler",function() at_handler("warp","csc07_warp") end)
  register_exec(config["function"].resetHandlerAddress,"reset-handler",function() at_handler("resetMap","csc36_resetMap") end)
  register_exec(config["function"].fadeHandlerAddress,"fade-handler",function() at_handler("loadMapFadeIn","csc37_loadMapAndFadeIn") end)
  register_exec(config["function"].reloadHandlerAddress,"reload-handler",function() at_handler("reloadMap","csc46_reloadMap") end)
  register_exec(config["function"].mapLoadHandlerAddress,"csc48-handler",at_csc48)
  local seams,targets={},{}
  for _,case in ipairs(config.cases) do for _,site in ipairs(case.serviceSites) do seams[site.address]=true;seams[site.returnAddress]=true;targets[config.service[site.target]]=true end end
  for address,_ in pairs(seams) do register_exec(address,"service-seam",function() at_service_seam(address) end) end
  for address,_ in pairs(targets) do register_exec(address,"service-entry",function() at_service_entry(address) end) end
  register_exec(config.instrumentation.trampolinePostHandlerAddress,"trampoline-complete",at_post_handler)
  status("milestone:observer-ready")
  local frames=0
  while true do
    frames=frames+1
    if pending_finish then finish(0);return end
    if pending_save then pending_save=false;replay_state=memorysavestate.savecorestate();status("milestone:saved-map-prompt")
    elseif pending_replay then pending_replay=false;memorysavestate.loadcorestate(replay_state);queue={};pulse("C");status("milestone:replay-map-prompt") end
    if frames>=config.maxFrames then status("timeout:frame-budget-exhausted:case="..case_index..":stage="..stage);finish(1);return end
    local button=nil
    if stage=="cheat" then local pointer=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS");if pointer>=0x28FF0 and pointer<0x29000 then button=names[cheat[pointer-0x28FF0+1]] elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Down" end elseif #queue>0 then button=table.remove(queue,1) end
    set_button(button);joypad.set({},2);emu.frameadvance()
    if frames%600==0 then status("frame="..frames..",stage="..stage..",case="..case_index) end
  end
end
local ok,message=pcall(run)
if not ok then fail_callback(message) end
