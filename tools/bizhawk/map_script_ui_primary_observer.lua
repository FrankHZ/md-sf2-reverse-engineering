local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local stage,prompt_count,case_index="cheat",0,1
local queue,records={},{}
local replay_state,pending_save,pending_replay,pending_finish=nil,false,false,false
local active,handler_entered,handler_return_observed=false,false,false
local callback_dispatches,pending_callback={},nil
local operand_word_at_use,helper_word_at_compare,handler_stack_delta=nil,nil,nil
local operand_use_reached,sentinel_compare_reached=false,false
local menu_a6_at_save_boundary,a6_restored_from_stack_observed=nil,nil
local json_null={}
local names={[1]="Up",[2]="Down",[4]="Left",[8]="Right",[16]="B",[32]="C"}
local cheat={1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4}

local function status(value) local f=assert(io.open(config.statusPath,"a"));f:write(value.."\n");f:close() end
local function enqueue(name,count) for _=1,count do queue[#queue+1]=name end end
local function pulse(name) enqueue("",30);enqueue(name,4);enqueue("",8) end
local function set_button(name) local buttons={};if name and name~="" then buttons[name]=true end;joypad.set(buttons,1) end
local function current_case() return config.cases[case_index] end
local function current_derived() return config.derived[case_index] end
local function boolean(value) if value then return "true" end return "false" end
local function json_string(value) return string.format("%q",value) end
local function is_array(value) local count=0;for key,_ in pairs(value) do if type(key)~="number" then return false end;count=count+1 end;for index=1,count do if value[index]==nil then return false end end;return true end
local function json(value)
  local kind=type(value);if value==nil or value==json_null then return "null" end;if kind=="boolean" then return boolean(value) end;if kind=="number" then return tostring(value) end;if kind=="string" then return json_string(value) end;if kind~="table" then error("map-script UI JSON type drift: "..kind) end
  local parts={};if is_array(value) then for _,item in ipairs(value) do parts[#parts+1]=json(item) end;return "["..table.concat(parts,",").."]" end
  for key,item in pairs(value) do parts[#parts+1]=json_string(key)..":"..json(item) end;return "{"..table.concat(parts,",").."}"
end
local function copy(value) if type(value)~="table" then return value end;local result={};for key,item in pairs(value) do result[key]=copy(item) end;return result end
local function word(value) return value & 0xFFFF end

local function setup_case()
  local case,derived=current_case(),current_derived();if case==nil or derived==nil then error("map-script UI unexpected harness entry") end
  local input=config.instrumentation.ramInputAddress
  local script=input+config.instrumentation.scriptInputRamOffset
  local helper_seed=input+config.instrumentation.serviceInterception.helperD1SeedRamOffset
  memory.write_u32_be(input,derived.handlerAddress,"M68K BUS")
  if case.handlerInputWord~=nil then memory.write_u16_be(script,case.handlerInputWord,"M68K BUS") end
  if case.helperD1Word~=nil then memory.write_u16_be(helper_seed,case.helperD1Word,"M68K BUS") end
  if case.portraitWindowIndexWordSeed~=nil then memory.write_u16_be(config.ram.portraitWindowIndexAddress,case.portraitWindowIndexWordSeed,"M68K BUS") end
  callback_dispatches={};pending_callback=nil;operand_word_at_use=nil;helper_word_at_compare=nil;handler_stack_delta=nil;operand_use_reached=false;sentinel_compare_reached=false;menu_a6_at_save_boundary=nil;a6_restored_from_stack_observed=nil;handler_entered=false;handler_return_observed=false;active=true
  status("milestone:case:"..case.id)
end

local function handler(expected)
  if not active then return end
  if current_derived().handlerAddress~=expected or emu.getregister("M68K PC")~=expected then error("map-script UI handler identity drift") end
  initial_stack_pointer=emu.getregister("M68K SP");handler_entered=true;status("milestone:handler-entry:"..current_case().id)
end

local function find_callback(address)
  local matches={}
  for _,callback in ipairs(current_derived().directCallbackPlan) do if callback.callSiteAddress==address then matches[#matches+1]=callback end end
  if #matches~=1 then error("map-script UI unexpected callback site: "..address) end
  return matches[1]
end

local function observe_callback(address)
  if not active then return end
  local pc=emu.getregister("M68K PC")
  if pc~=address then error("map-script UI callback call-site PC drift") end
  if pending_callback~=nil then error("map-script UI callback returned out of order") end
  local callback=find_callback(address)
  pending_callback={instructionTarget=callback.instructionTarget,effectiveTarget=callback.effectiveTarget,callSiteAddressObserved=pc,expectedReturnAddress=callback.returnAddress}
  status("milestone:callback:"..callback.instructionTarget..":"..current_case().id)
end

local function find_interception_patch(address)
  local matches={}
  for _,patch in ipairs(config.instrumentation.serviceInterception.patches) do if patch.address==address then matches[#matches+1]=patch end end
  if #matches~=1 then error("map-script UI unexpected callback target: "..address) end
  return matches[1]
end

local function observe_callback_target(address)
  if not active then return end
  local pc=emu.getregister("M68K PC")
  if pc~=address then error("map-script UI callback target PC drift") end
  if pending_callback==nil or pending_callback.targetRole~=nil then error("map-script UI callback target chronology drift") end
  local patch=find_interception_patch(address)
  local expected_identity=pending_callback[patch.targetRole=="instruction" and "instructionTarget" or "effectiveTarget"]
  if patch.targetIdentity~=expected_identity then error("map-script UI callback target identity drift") end
  pending_callback.targetRole=patch.targetRole
  pending_callback.targetAddressObserved=pc
end

local function observe_callback_return(address)
  if not active then return end
  local pc=emu.getregister("M68K PC")
  if pc~=address then error("map-script UI callback return PC drift") end
  if pending_callback==nil or pending_callback.expectedReturnAddress~=address or pending_callback.targetRole==nil then error("map-script UI callback return chronology drift") end
  pending_callback.returnAddressObserved=pc
  pending_callback.expectedReturnAddress=nil
  callback_dispatches[#callback_dispatches+1]=pending_callback
  pending_callback=nil
end

local function observe_operand(expected_handler,expected_address)
  if not active or current_derived().handlerAddress~=expected_handler then return end
  if emu.getregister("M68K PC")~=expected_address then error("map-script UI operand-use PC drift") end
  operand_use_reached=true
  operand_word_at_use=word(emu.getregister("M68K D0"))
  if expected_handler==config["function"].csc12_executeContextMenuAddress then menu_a6_at_save_boundary=emu.getregister("M68K A6") end
end

local function observe_sentinel_compare(expected_address)
  if active and current_derived().handlerAddress==config["function"].csc1D_showPortraitAddress then
    if emu.getregister("M68K PC")~=expected_address then error("map-script UI sentinel compare PC drift") end
    sentinel_compare_reached=true;helper_word_at_compare=word(emu.getregister("M68K D1"))
  end
end

local function observe_handler_return(expected_handler,expected_return)
  if not active or current_derived().handlerAddress~=expected_handler then return end
  if emu.getregister("M68K PC")~=expected_return then error("map-script UI handler return identity drift") end
  handler_stack_delta=emu.getregister("M68K SP")-initial_stack_pointer
  if handler_stack_delta~=current_derived().stackPointerDeltaBytesAfter then error("map-script UI source stack transfer drift: "..handler_stack_delta) end
  if expected_handler==config["function"].csc12_executeContextMenuAddress then
    if menu_a6_at_save_boundary==nil then error("map-script UI menu A6 save boundary did not execute") end
    a6_restored_from_stack_observed=emu.getregister("M68K A6")==menu_a6_at_save_boundary
  else
    a6_restored_from_stack_observed=json_null
  end
  handler_return_observed=true
end

local function append_record()
  local case,derived=current_case(),current_derived()
  if not handler_entered or not handler_return_observed or pending_callback~=nil then error("map-script UI handler return did not execute") end
  if #callback_dispatches~=#derived.directCallbackPlan then error("map-script UI callback count drift") end
  for index,callback in ipairs(derived.directCallbackPlan) do
    local observed=callback_dispatches[index]
    if observed.instructionTarget~=callback.instructionTarget or observed.effectiveTarget~=callback.effectiveTarget or observed.callSiteAddressObserved~=callback.callSiteAddress or observed.returnAddressObserved~=callback.returnAddress then error("map-script UI callback identity/order drift") end
  end
  local input_offset=emu.getregister("M68K A6")-config.instrumentation.ramInputAddress
  if input_offset~=derived.scriptCursorRamOffsetAfter then error("map-script UI A6 cursor drift: "..input_offset) end
  if derived.handlerInputWord~=nil and operand_word_at_use~=derived.handlerInputWord then error("map-script UI operand read observation drift") end
  local portrait_window_busy_early_return_observed=derived.handlerAddress==config["function"].csc1D_showPortraitAddress and handler_entered and operand_use_reached and not sentinel_compare_reached and #callback_dispatches==0 and handler_return_observed
  local sentinel_d1_branch_observed=derived.handlerAddress==config["function"].csc1D_showPortraitAddress and handler_entered and operand_use_reached and sentinel_compare_reached and helper_word_at_compare==config.constants.signedWordSentinel and #callback_dispatches==2 and callback_dispatches[1].instructionTarget=="WaitForViewScrollEnd" and callback_dispatches[2].instructionTarget=="GetEntityPortaitAndSpeechSfx" and handler_return_observed
  if case.kind=="show-sentinel" and not sentinel_d1_branch_observed then error("map-script UI sentinel compare drift") end
  if case.kind=="show-busy" and not portrait_window_busy_early_return_observed then error("map-script UI busy early-return drift") end
  local record=copy(derived)
  record.handlerReturned=handler_return_observed
  record.callbackDispatchesObserved=callback_dispatches
  record.handlerInputWord=derived.handlerInputWord or json_null
  record.sourceInput=derived.sourceInput or json_null
  record.handlerInputWordAtFirstOperandUse=operand_word_at_use or json_null
  record.portraitWindowBusyEarlyReturnObserved=portrait_window_busy_early_return_observed
  record.sentinelD1BranchObserved=sentinel_d1_branch_observed
  record.helperD1WordAtComparison=helper_word_at_compare or json_null
  record.stackPointerDeltaBytesObserved=handler_stack_delta
  record.a6AtMenuSaveBoundaryObserved=menu_a6_at_save_boundary or json_null
  record.a6RestoredFromStackObserved=a6_restored_from_stack_observed
  record.scriptCursorRamOffsetAfterObserved=input_offset
  records[#records+1]=record
end

local function finish(code)
  if replay_state then memorysavestate.removestate(replay_state) end
  if code~=0 then client.exitCode(code);return end
  local result={system=emu.getsystemid(),core="Genesis Plus GX",id=config.fixtureId,mapTest=config.mapTestIndex,recordOrder={},records=records}
  for _,case in ipairs(config.cases) do result.recordOrder[#result.recordOrder+1]=case.id end
  local file=assert(io.open(config.outputPath,"w"));file:write(json(result).."\n");file:close();client.exitCode(0)
end

event.on_bus_exec(function() prompt_count=prompt_count+1;status("milestone:number-prompt-entry:"..prompt_count);if prompt_count==1 then stage="map";pending_save=true;pulse("C") end end,config.harness["function"].numberPromptAddress,"ui-number","M68K BUS")
event.on_bus_exec(function() status("milestone:flag-prompt-entry");pulse("B") end,config.harness["function"].flagPromptAddress,"ui-flag","M68K BUS")
event.on_bus_exec(setup_case,config["function"].entryAddress,"ui-entry","M68K BUS")
event.on_bus_exec(function() handler(config["function"].csc1D_showPortraitAddress) end,config["function"].csc1D_showPortraitAddress,"ui-show","M68K BUS")
event.on_bus_exec(function() handler(config["function"].csc1E_hidePortraitAddress) end,config["function"].csc1E_hidePortraitAddress,"ui-hide","M68K BUS")
event.on_bus_exec(function() handler(config["function"].csc12_executeContextMenuAddress) end,config["function"].csc12_executeContextMenuAddress,"ui-menu","M68K BUS")
event.on_bus_exec(function() observe_operand(config["function"].csc1D_showPortraitAddress,config["function"].showPortraitFirstOperandFollowupAddress) end,config["function"].showPortraitFirstOperandFollowupAddress,"ui-show-operand","M68K BUS")
event.on_bus_exec(function() observe_operand(config["function"].csc12_executeContextMenuAddress,config["function"].menuFirstOperandFollowupAddress) end,config["function"].menuFirstOperandFollowupAddress,"ui-menu-operand","M68K BUS")
event.on_bus_exec(function() observe_sentinel_compare(config["function"].showPortraitSentinelCompareAddress) end,config["function"].showPortraitSentinelCompareAddress,"ui-sentinel-compare","M68K BUS")
event.on_bus_exec(function() observe_handler_return(config["function"].csc1D_showPortraitAddress,config["function"].csc1D_showPortraitReturnInstructionAddress) end,config["function"].csc1D_showPortraitReturnInstructionAddress,"ui-show-return","M68K BUS")
event.on_bus_exec(function() observe_handler_return(config["function"].csc1E_hidePortraitAddress,config["function"].csc1E_hidePortraitReturnInstructionAddress) end,config["function"].csc1E_hidePortraitReturnInstructionAddress,"ui-hide-return","M68K BUS")
event.on_bus_exec(function() observe_handler_return(config["function"].csc12_executeContextMenuAddress,config["function"].csc12_executeContextMenuReturnInstructionAddress) end,config["function"].csc12_executeContextMenuReturnInstructionAddress,"ui-menu-return","M68K BUS")
local registered_returns={}
local function register_callback_return(address)
  if registered_returns[address] then return end
  registered_returns[address]=true
  event.on_bus_exec(function() observe_callback_return(address) end,address,"ui-callback-return-"..address,"M68K BUS")
end
for _,derived in ipairs(config.derived) do for _,callback in ipairs(derived.directCallbackPlan) do register_callback_return(callback.returnAddress) end end
local registered_callbacks={}
local function register_callback(address)
  if registered_callbacks[address] then return end
  registered_callbacks[address]=true
  event.on_bus_exec(function() observe_callback(address) end,address,"ui-callback-"..address,"M68K BUS")
end
for _,derived in ipairs(config.derived) do for _,callback in ipairs(derived.directCallbackPlan) do register_callback(callback.callSiteAddress) end end
local registered_callback_targets={}
local function register_callback_target(address)
  if registered_callback_targets[address] then return end
  registered_callback_targets[address]=true
  event.on_bus_exec(function() observe_callback_target(address) end,address,"ui-callback-target-"..address,"M68K BUS")
end
for _,patch in ipairs(config.instrumentation.serviceInterception.patches) do register_callback_target(patch.address) end
event.on_bus_exec(function() if not active then return end;append_record();active=false;case_index=case_index+1;if case_index>#config.cases then pending_finish=true else pending_replay=true end end,config.instrumentation.postHandlerAddress,"ui-return","M68K BUS")

local frames=0
while true do
  frames=frames+1
  if pending_finish then finish(0) elseif pending_save then pending_save=false;replay_state=memorysavestate.savecorestate();status("milestone:saved-map-prompt") elseif pending_replay then pending_replay=false;memorysavestate.loadcorestate(replay_state);queue={};pulse("C");status("milestone:replay-map-prompt") end
  if frames>=config.maxFrames then status("timeout:frame-budget-exhausted:case="..case_index);finish(1) end
  local button=nil
  if stage=="cheat" then local pointer=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS");if pointer>=0x28FF0 and pointer<0x29000 then button=names[cheat[pointer-0x28FF0+1]] elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then button="Down" end elseif #queue>0 then button=table.remove(queue,1) end
  set_button(button);joypad.set({},2);emu.frameadvance()
end
