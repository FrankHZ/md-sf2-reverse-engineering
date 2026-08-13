local config=assert(dofile(assert(os.getenv("SF2_H3_CONFIG"),"SF2_H3_CONFIG is not set")))
local json=assert(loadfile(config.jsonModulePath))()
local stage,prompt_count,case_index="cheat",0,1
local queue,records,callbacks,event_ids={}, {}, {}, {}
local replay_state,active,pending_save,pending_replay,pending_finish=nil,false,false,false,false
local active_handler_address,chronology=nil,{}
local observer_failed,session_cleaned,probe_milestone=false,false,false
local current_phase,current_role,current_pc,current_expectation="registration","registration",nil,nil
local transition,pending_trampoline_return,trampoline_stack_pointer=nil,nil,nil
local frames=0
local snapshots={scratch=nil,ram=nil,sram=nil}
local case_mutation_state={logicalRam=false,sram=false,scratch=false}
local session_touched={logicalRam=false,sram=false,scratch=false}
local restoration={logicalRam=true,sram=true,generatedScratch=true,pointerScratch=true,retainedV1Stream=true,promptResultWord=true,callStack=true}
local names={[1]="Up",[2]="Down",[4]="Left",[8]="Right",[16]="B",[32]="C"}
local cheat={1,1,2,1,16,32,8,4,1,1,2,1,16,32,8,4}

local function status(v)local f=assert(io.open(config.statusPath,"a"));f:write(v.."\n");f:close()end
local function bool(v)return v and "true" or "false"end
local function nullable(v)return v==nil and "null" or tostring(v)end
local function q(v)return string.format("%q",v)end
local function enqueue(n,c)for _=1,c do queue[#queue+1]=n end end
local function pulse(n)enqueue("",30);enqueue(n,4);enqueue("",8)end
local function set_button(n)local b={};if n and n~="" then b[n]=true end;joypad.set(b,1)end
local function current()return config.cases[case_index]end
local function input()return config.caseInputs[case_index]end
local function word(name)return emu.getregister("M68K "..name)&0xFFFF end
local function pc()return emu.getregister("M68K PC")&0xFFFFFF end
local function a7()return emu.getregister("M68K A7")&0xFFFFFF end
local function put16(address,value)memory.write_u16_be(address,value&0xFFFF,"M68K BUS")end
local function put32(address,value)memory.write_u32_be(address,value&0xFFFFFFFF,"M68K BUS")end
local function get32(address)return memory.read_u32_be(address,"M68K BUS")end
local function probe()return config.instrumentation.persistenceProbe end
local function route()return config.wrapperRoute end
local function layout()return config.scratchLayout end
local function transition_matches(stage)
  local r=route();local expected=stage=="outer" and {callPc=r.outerCallSiteAddress,targetPc=r.outerTargetAddress,returnPc=r.outerReturnAddress} or {callPc=r.innerCallSiteAddress,targetPc=r.innerTargetAddress,returnPc=r.innerReturnAddress}
  return transition.stage==stage and transition.callPc==expected.callPc and transition.targetPc==expected.targetPc and transition.returnPc==expected.returnPc
end
local function set_transition(stage,role,event_pc)
  local r=route();local expected=stage=="outer" and {callPc=r.outerCallSiteAddress,targetPc=r.outerTargetAddress,returnPc=r.outerReturnAddress} or {callPc=r.innerCallSiteAddress,targetPc=r.innerTargetAddress,returnPc=r.innerReturnAddress}
  transition.stage=stage;transition.role=role;transition.eventPc=event_pc;transition.callPc=expected.callPc;transition.targetPc=expected.targetPc;transition.returnPc=expected.returnPc
end
local function roles_json(address)
  local rows={};for _,entry in ipairs(callbacks[address] or {}) do rows[#rows+1]=q(entry.role)end
  return "["..table.concat(rows,",").."]"
end
local function pending_callback()
  local case=current();local pending=current_expectation or transition or {}
  return "{\"active\":"..bool(active)..",\"caseIndex\":"..case_index..",\"caseKind\":"..(case and q(input().kind) or "null")..",\"expectedEventPc\":"..nullable(pending.eventPc)..",\"expectedCallPc\":"..nullable(pending.callPc)..",\"expectedReturnPc\":"..nullable(pending.returnPc)..",\"expectedTargetPc\":"..nullable(pending.targetPc)..",\"rolesAtPc\":"..roles_json(current_pc).."}"
end
local function unregister_events()for i=#event_ids,1,-1 do event.unregisterbyid(event_ids[i]);event_ids[i]=nil end end
local function cleanup_session()
  if session_cleaned then return end
  session_cleaned=true;unregister_events()
  if replay_state then memorysavestate.removestate(replay_state);replay_state=nil end
end
local function sram_offset(address)
  local offset=address-config.runtimeContract.persistence.physicalWindowBaseAddress
  assert(offset>=0 and offset<memory.getmemorydomainsize("SRAM"),"story-state SRAM physical address drift")
  return offset
end
local function sram_read(address)return memory.read_u8(sram_offset(address),"SRAM")end
local function sram_write(address,value)memory.write_u8(sram_offset(address),value&0xFF,"SRAM")end
local function snapshot_bytes(address,count,space,stride)
  local bytes={};stride=stride or 1
  for offset=0,count-1 do
    bytes[#bytes+1]=space=="SRAM" and sram_read(address+offset*stride) or memory.read_u8(address+offset,"M68K BUS")
  end
  return {address=address,byteCount=count,space=space,stride=stride,bytes=bytes}
end
local function restore_bytes(snapshot,domain)
  if not snapshot then return true,nil end
  for offset,byte in ipairs(snapshot.bytes) do
    local address=snapshot.address+(offset-1)*snapshot.stride
    if snapshot.space=="SRAM" then sram_write(address,byte) else memory.write_u8(address,byte,"M68K BUS") end
  end
  for offset,byte in ipairs(snapshot.bytes) do
    local address=snapshot.address+(offset-1)*snapshot.stride
    local actual=snapshot.space=="SRAM" and sram_read(address) or memory.read_u8(address,"M68K BUS")
    if actual~=byte then return false,{domain=domain,address=address,expected=byte,actual=actual} end
  end
  return true,nil
end
local function snapshot_sram()
  if snapshots.sram then return end
  local slots,seen={},{}
  local span=config.runtimeContract.persistence.ramLogicalSpan
  for _,case in ipairs(config.cases) do
    local selected=case.expected.selectedSlot
    if selected and not seen[selected.slot] then
      slots[#slots+1]={slot=selected.slot,data=snapshot_bytes(selected.selectedDataAddress,span.logicalByteCount,"SRAM",selected.selectedPhysicalByteStride),checksum=snapshot_bytes(selected.selectedChecksumAddress,1,"SRAM",1)}
      seen[selected.slot]=true
    end
  end
  snapshots.sram={slots=slots,saveFlags=snapshot_bytes(config.runtimeContract.persistence.saveFlagsAddress,1,"SRAM",1)}
end
local function snapshot_scratch()
  if snapshots.scratch then return end
  local rows={}
  for _,range in ipairs(layout().ranges) do rows[#rows+1]={name=range.name,snapshot=snapshot_bytes(range.address,range.byteCount,"RAM",1)} end
  snapshots.scratch={ranges=rows,pointer=snapshot_bytes(layout().pointerScratch.address,layout().pointerScratch.byteCount,"RAM",1),retainedV1Stream=snapshot_bytes(layout().retainedV1Stream.address,layout().retainedV1Stream.byteCount,"RAM",1),prompt=snapshot_bytes(config.instrumentation.yesNoPromptStub.resultRamAddress,2,"RAM",1)}
end
local function snapshot_probe_domains()
  if not snapshots.ram then
    local span=config.runtimeContract.persistence.ramLogicalSpan
    snapshots.ram=snapshot_bytes(span.baseAddress,span.logicalByteCount,"RAM",1)
  end
  snapshot_sram()
end
local function restore_scopes()
  local ok,mismatch=true,nil
  if session_touched.logicalRam then
    restoration.logicalRam,mismatch=restore_bytes(snapshots.ram,"logicalRam");ok=ok and restoration.logicalRam
    if not restoration.logicalRam then return false,mismatch end
  end
  if session_touched.sram then
    for _,slot in ipairs(snapshots.sram.slots) do
      restoration.sram,mismatch=restore_bytes(slot.data,"sram");ok=ok and restoration.sram
      if not restoration.sram then return false,mismatch end
      restoration.sram,mismatch=restore_bytes(slot.checksum,"sram");ok=ok and restoration.sram
      if not restoration.sram then return false,mismatch end
    end
    restoration.sram,mismatch=restore_bytes(snapshots.sram.saveFlags,"sram");ok=ok and restoration.sram
    if not restoration.sram then return false,mismatch end
  end
  if session_touched.scratch then
    for _,row in ipairs(snapshots.scratch.ranges) do
      restoration.generatedScratch,mismatch=restore_bytes(row.snapshot,"generatedScratch");ok=ok and restoration.generatedScratch
      if not restoration.generatedScratch then return false,mismatch end
    end
    restoration.pointerScratch,mismatch=restore_bytes(snapshots.scratch.pointer,"pointerScratch");ok=ok and restoration.pointerScratch
    if not restoration.pointerScratch then return false,mismatch end
    restoration.retainedV1Stream,mismatch=restore_bytes(snapshots.scratch.retainedV1Stream,"retainedV1Stream");ok=ok and restoration.retainedV1Stream
    if not restoration.retainedV1Stream then return false,mismatch end
    restoration.promptResultWord,mismatch=restore_bytes(snapshots.scratch.prompt,"promptResultWord");ok=ok and restoration.promptResultWord
    if not restoration.promptResultWord then return false,mismatch end
  end
  return ok,nil
end
local function restoration_json()
  local span=config.runtimeContract.persistence.ramLogicalSpan
  local ranges={};for _,range in ipairs(layout().ranges) do ranges[#ranges+1]={name=range.name,address=range.address,byteCount=range.byteCount} end
  return {logicalRam={baseAddress=span.baseAddress,logicalByteCount=span.logicalByteCount,restored=restoration.logicalRam},sram={logicalByteCountPerSlot=span.logicalByteCount,slotCount=2,checksumRestored=restoration.sram,saveFlagsRestored=restoration.sram,restored=restoration.sram},generatedScratch={ranges=ranges,restored=restoration.generatedScratch},pointerScratch={address=layout().pointerScratch.address,byteCount=layout().pointerScratch.byteCount,restored=restoration.pointerScratch},retainedV1Stream={address=layout().retainedV1Stream.address,byteCount=layout().retainedV1Stream.byteCount,restored=restoration.retainedV1Stream},promptResultWord={address=config.instrumentation.yesNoPromptStub.resultRamAddress,byteCount=2,restored=restoration.promptResultWord},callStack={observedBalanced=restoration.callStack}}
end
local function mismatch_json(mismatch)
  if not mismatch then return "null" end
  return "{\"domain\":"..q(mismatch.domain)..",\"address\":"..nullable(mismatch.address)..",\"expected\":"..nullable(mismatch.expected)..",\"actual\":"..nullable(mismatch.actual).."}"
end
local function fail(message,forced_mismatch)
  if observer_failed then return end
  observer_failed=true
  local expected=current_expectation or transition or {};local case=current()
  os.remove(config.outputPath)
  if replay_state then memorysavestate.loadcorestate(replay_state) end
  local restored,mismatch=restore_scopes();mismatch=forced_mismatch or mismatch
  cleanup_session()
  local payload="{\"owner\":"..q(config.observerFailureContract.owner)..",\"caseId\":"..(case and q(case.id) or "null")..",\"phase\":"..q(current_phase)..",\"role\":"..q(current_role)..",\"actualPc\":"..nullable(pc())..",\"expectedCallPc\":"..nullable(expected.callPc)..",\"expectedEventPc\":"..nullable(expected.eventPc)..",\"expectedReturnPc\":"..nullable(expected.returnPc)..",\"expectedTargetPc\":"..nullable(expected.targetPc)..",\"pendingCallback\":"..pending_callback()..",\"callbacksRemaining\":0,\"mutationState\":{\"logicalRamMutated\":"..bool(case_mutation_state.logicalRam)..",\"sramMutated\":"..bool(case_mutation_state.sram)..",\"scratchMutated\":"..bool(case_mutation_state.scratch).."},\"outputRemoved\":true,\"sessionStateRestored\":"..bool(restored)..",\"restorationMismatch\":"..mismatch_json(mismatch)..",\"error\":"..q(tostring(message)).."}"
  local diagnostic=config.observerFailureContract.statusPrefix..payload
  status(diagnostic);print(diagnostic);client.exitCode(config.observerFailureContract.exitCode)
end
local function add_callback(address,role,handler)
  callbacks[address]=callbacks[address] or {};callbacks[address][#callbacks[address]+1]={role=role,handler=handler}
end
local function dispatch(address)
  current_pc=address
  for _,entry in ipairs(callbacks[address] or {}) do
    current_role=entry.role
    local ok,error_message=pcall(entry.handler)
    if not ok then fail(error_message);return end
  end
end
local function install_callbacks()
  for address,_ in pairs(callbacks) do event_ids[#event_ids+1]=event.on_bus_exec(function()dispatch(address)end,address,"story-dispatch-"..address,"M68K BUS") end
end
local function trace(kind,address,instruction,effective)
  chronology[#chronology+1]={kind=kind,h1Address=address,instructionTarget=instruction or json.null,effectiveTarget=effective or json.null,d0Word=word("D0"),d1Word=word("D1"),a6=emu.getregister("M68K A6")&0xFFFFFF}
end
local function persistence_trace()
  local case=current();local selected=case.expected.selectedSlot;local pending=current_expectation
  chronology[#chronology+1]={role=current_role,pc=pc(),callPc=pending.callPc,targetPc=pending.targetPc,returnPc=pending.returnPc,ramByte=memory.read_u8(case.expected.mutation.flagStorage.flagStorageAddress,"M68K BUS"),selectedSramByte=sram_read(selected.selectedFlagPhysicalAddress),saveFlags=sram_read(config.runtimeContract.persistence.saveFlagsAddress)}
end
local function clear_selected_slot(selected)
  local span=config.runtimeContract.persistence.ramLogicalSpan
  for offset=0,span.logicalByteCount-1 do sram_write(selected.selectedDataAddress+offset*selected.selectedPhysicalByteStride,0) end
  sram_write(selected.selectedChecksumAddress,0)
end
local function pattern(seed,offset)return (seed+17*offset+29*(offset//8))&0xFF end
local function put_word(bytes,offset,value)bytes[offset+1]=(value>>8)&0xFF;bytes[offset+2]=value&0xFF end
local function put_long(bytes,offset,value)put_word(bytes,offset,(value>>16)&0xFFFF);put_word(bytes,offset+2,value&0xFFFF)end
local function stream_address(case_input)
  local retained=layout().retainedV1Stream.address;local persistence=probe().mutationStreamAddress
  local expected=case_input.kind=="v1" and retained or persistence
  assert(case_input.streamAddress==expected,"story-state case stream-address drift")
  if case_input.kind=="persistence" then assert(case_input.streamAddress~=retained,"story-state persistence stream used retained-v1 address") end
  return expected
end
local function generated_program(case,case_input)
  local p=probe().programAddress;local bytes={};for index=1,42 do bytes[index]=0x4A end
  if case_input.kind=="v1" then
    put_word(bytes,0,0x2C7C);put_long(bytes,2,stream_address(case_input));put_word(bytes,6,0x4EB9);put_long(bytes,8,case_input.handlerAddress);put_word(bytes,12,0x4E75)
  else
    local stream=stream_address(case_input);local final_stream=probe().finalStreamAddress
    put_word(bytes,0,0x2C7C);put_long(bytes,2,stream);put_word(bytes,6,0x4EB9);put_long(bytes,8,case_input.handlerAddress)
    put_word(bytes,12,0x7000|case.expected.selector);put_word(bytes,14,0x4EB9);put_long(bytes,16,config.runtimeContract.persistence.saveGameAddress)
    put_word(bytes,20,0x7000|case.expected.selector);put_word(bytes,22,0x4EB9);put_long(bytes,24,config.runtimeContract.persistence.loadGameAddress)
    put_word(bytes,28,0x2C7C);put_long(bytes,30,final_stream);put_word(bytes,34,0x4EB9);put_long(bytes,36,case_input.finalHandlerAddress);put_word(bytes,40,0x4E75)
  end
  assert(#bytes==42 and p==route().probeEntryAddress,"story-state generated program layout drift")
  return bytes
end
local function write_bytes(address,bytes)for offset,value in ipairs(bytes) do memory.write_u8(address+offset-1,value,"M68K BUS") end end
local function verify_bytes(address,bytes,domain)
  for offset,value in ipairs(bytes) do
    local actual=memory.read_u8(address+offset-1,"M68K BUS")
    if actual~=value then error("story-state generated "..domain.." readback drift at "..string.format("%06X",address+offset-1)) end
  end
end
local function arm_probe(case,case_input)
  snapshot_scratch()
  local program=generated_program(case,case_input);local input_stream_address=stream_address(case_input);local stream={0,0,0,0,0,0};local final=nil
  for offset,value in ipairs(case_input.streamBytes) do stream[offset]=value end
  if case_input.kind=="persistence" then put16(probe().finalStreamAddress,case_input.finalFlagIndexWord);put32(probe().finalStreamAddress+2,probe().programAddress);final={(case_input.finalFlagIndexWord>>8)&0xFF,case_input.finalFlagIndexWord&0xFF,(probe().programAddress>>24)&0xFF,(probe().programAddress>>16)&0xFF,(probe().programAddress>>8)&0xFF,probe().programAddress&0xFF} end
  write_bytes(probe().programAddress,program);write_bytes(input_stream_address,stream);if final then write_bytes(probe().finalStreamAddress,final)end;put32(layout().pointerScratch.address,probe().programAddress)
  case_mutation_state.scratch=true;session_touched.scratch=true
  return {program=program,stream={address=input_stream_address,bytes=stream},final=final}
end
local function seed_probe_case(case,case_input,armed)
  snapshot_probe_domains()
  local span=config.runtimeContract.persistence.ramLogicalSpan
  for offset=0,span.logicalByteCount-1 do memory.write_u8(span.baseAddress+offset,pattern(case.expected.ramPatternSeed or 0,offset),"M68K BUS") end
  local storage=case_input.kind=="v1" and case.expected.flagStorage or case.expected.mutation.flagStorage
  memory.write_u8(storage.flagStorageAddress,storage.initialFlagSet and storage.flagBitMask or 0,"M68K BUS")
  case_mutation_state.logicalRam=true;session_touched.logicalRam=true
  if case_input.kind=="persistence" then clear_selected_slot(case.expected.selectedSlot);sram_write(config.runtimeContract.persistence.saveFlagsAddress,0);case_mutation_state.sram=true;session_touched.sram=true end
  put16(config.instrumentation.yesNoPromptStub.resultRamAddress,case_input.promptResultWord)
  verify_bytes(probe().programAddress,armed.program,"program");verify_bytes(armed.stream.address,armed.stream.bytes,case_input.kind=="v1" and "retained-v1 stream" or "mutation stream");if armed.final then verify_bytes(probe().finalStreamAddress,armed.final,"final stream")end
  assert(get32(layout().pointerScratch.address)==probe().programAddress,"story-state pointer scratch readback drift")
end
local function begin()
  if active then return end
  local case=current();local case_input=input();assert(case and case_input and case.id==case_input.id,"story-state case identity drift")
  case_mutation_state={logicalRam=false,sram=false,scratch=false}
  active=true;chronology={};active_handler_address=case_input.handlerAddress
  local r=route();local armed=arm_probe(case,case_input);transition={active=true,stage="outer",role="wrapper-callsite",eventPc=r.outerCallSiteAddress,callPc=r.outerCallSiteAddress,targetPc=r.outerTargetAddress,returnPc=r.outerReturnAddress,deadline=frames+60,armed=armed}
  current_phase,current_role,current_expectation="wrapper-transition","story-entry",transition
  status("milestone:case:"..case.id)
end
local function transition_event(role,address,next_role)
  assert(active and transition and transition.active,"story-state missing wrapper-to-probe transition")
  assert(transition.role==role and pc()==address,"story-state wrapper-to-probe role/PC drift")
  if role=="wrapper-callsite" then assert(transition_matches("outer"),"story-state outer transition tuple drift");set_transition("outer",next_role,route().trampolineEntryAddress)
  elseif role=="trampoline-entry" then assert(transition_matches("outer"),"story-state outer transition tuple drift");set_transition("outer",next_role,route().innerCallSiteAddress)
  else assert(role=="trampoline-jsr" and transition_matches("outer"),"story-state outer transition tuple drift");set_transition("inner",next_role,route().innerTargetAddress);assert(get32(layout().pointerScratch.address)==route().innerTargetAddress,"story-state trampoline target drift");trampoline_stack_pointer=a7() end
  current_phase,current_role,current_expectation="wrapper-transition",current_role,transition
end
local function actual_probe_entry()
  assert(active and transition and transition.active and transition.role=="probe-entry" and transition_matches("inner"),"story-state probe entry transition drift")
  assert(pc()==route().innerTargetAddress and get32(layout().pointerScratch.address)==route().innerTargetAddress,"story-state actual probe entry drift")
  local armed=transition.armed;transition.active=false;current_expectation=nil
  seed_probe_case(current(),input(),armed)
  if not probe_milestone then probe_milestone=true;status("milestone:story-state-probe") end
end
local function next_case()
  active=false;pending_trampoline_return=nil;case_index=case_index+1;if case_index>#config.cases then pending_finish=true else pending_replay=true end
end
local function v1_program_return()
  if not active or input().kind~="v1" then return end
  local case=current();local byte=memory.read_u8(case.expected.flagStorage.flagStorageAddress,"M68K BUS")
  records[#records+1]={id=case.id,handlerAddress=active_handler_address,a6Output=emu.getregister("M68K A6")&0xFFFFFF,flagByteAfter=byte,finalFlagSet=(byte&case.expected.flagStorage.flagBitMask)~=0,chronology=chronology}
  pending_trampoline_return={kind="v1",stackPointer=trampoline_stack_pointer,deadline=frames+60}
end
local function persistence_handler_entry(address)
  if not active or input().kind~="persistence" then return end
  local probe_address=probe().programAddress;local phase=pc()==input().handlerAddress and "mutation-handler-entry" or "final-handler-entry"
  if phase=="mutation-handler-entry" then current_phase,current_role,current_expectation=phase,phase,{eventPc=address,callPc=probe_address+6,targetPc=address,returnPc=probe_address+12} else current_phase,current_role,current_expectation=phase,phase,{eventPc=address,callPc=probe_address+34,targetPc=address,returnPc=probe_address+40} end
  persistence_trace()
end
local function persistence_call(handler,address,call)
  if not active or input().kind~="persistence" then return end
  local case=current();local mutation=case.expected.mutation;local final=case.expected.finalCheck;local expected_target,role=nil,nil
  if handler==mutation.handler and call.instructionTarget==mutation.expectedInstructionTarget then role="mutation-call";expected_target=mutation.expectedEffectiveTarget elseif handler==final.handler and call.instructionTarget==final.instructionTarget then role="final-check-call";expected_target=final.effectiveTarget end
  if role then current_phase,current_role,current_expectation=role,role,{eventPc=address,callPc=address,targetPc=config.runtimeContract.effectiveServiceAddresses[expected_target],returnPc=call.returnAddress};persistence_trace() end
end
local function persistence_program(address)
  if not active then return end
  if input().kind=="v1" and address==probe().programAddress+12 then v1_program_return();return end
  if input().kind~="persistence" then return end
  local case=current();local expected=case.expected;local selected=expected.selectedSlot;local probe_address=probe().programAddress;local tracked=expected.mutation.flagStorage.flagStorageAddress
  if address==probe_address+12 then
    current_phase,current_role,current_expectation="mutation-return","mutation-return",{eventPc=address,callPc=probe_address+6,targetPc=input().handlerAddress,returnPc=address};assert(memory.read_u8(tracked,"M68K BUS")==expected.stateBytes.mutated,"story-state mutation byte drift");persistence_trace()
  elseif address==config.runtimeContract.persistence.saveGameAddress then
    current_phase,current_role,current_expectation="save-entry","save-entry",{eventPc=address,callPc=probe_address+14,targetPc=address,returnPc=probe_address+20};assert(get32(a7())==current_expectation.returnPc,"story-state SaveGame stack return drift");persistence_trace()
  elseif address==probe_address+20 then
    current_phase,current_role,current_expectation="save-return-poison","save-return-poison",{eventPc=address,callPc=probe_address+14,targetPc=config.runtimeContract.persistence.saveGameAddress,returnPc=address};assert(sram_read(selected.selectedFlagPhysicalAddress)==expected.stateBytes.mutated,"story-state saved physical byte drift");assert((sram_read(config.runtimeContract.persistence.saveFlagsAddress)&(1<<selected.occupiedFlagBit))~=0,"story-state occupied-bit write drift");memory.write_u8(tracked,expected.stateBytes.poisoned,"M68K BUS");assert(memory.read_u8(tracked,"M68K BUS")==expected.stateBytes.poisoned,"story-state inverse poison drift");persistence_trace()
  elseif address==config.runtimeContract.persistence.loadGameAddress then
    current_phase,current_role,current_expectation="load-entry","load-entry",{eventPc=address,callPc=probe_address+22,targetPc=address,returnPc=probe_address+28};assert(get32(a7())==current_expectation.returnPc,"story-state LoadGame stack return drift");persistence_trace()
  elseif address==probe_address+28 then
    current_phase,current_role,current_expectation="load-return","load-return",{eventPc=address,callPc=probe_address+22,targetPc=config.runtimeContract.persistence.loadGameAddress,returnPc=address};assert(memory.read_u8(tracked,"M68K BUS")==expected.stateBytes.restored,"story-state restored RAM byte drift");persistence_trace()
  elseif address==probe_address+40 then
    current_phase,current_role,current_expectation="final-branch-result","final-branch-result",{eventPc=address,callPc=probe_address+34,targetPc=input().finalHandlerAddress,returnPc=address};assert((emu.getregister("M68K A6")&0xFFFFFF)==probe_address,"story-state final branch target drift");persistence_trace();records[#records+1]={id=case.id,mutationHandlerAddress=input().handlerAddress,finalHandlerAddress=input().finalHandlerAddress,selector=expected.selector,ramLogicalSpan=config.runtimeContract.persistence.ramLogicalSpan,trackedByte={ramAddress=tracked,logicalOffset=expected.selectedSlot.gameFlagsLogicalOffset,selectedPhysicalAddress=expected.selectedSlot.selectedFlagPhysicalAddress,before=expected.stateBytes.before,mutated=expected.stateBytes.mutated,poisoned=expected.stateBytes.poisoned,restored=memory.read_u8(tracked,"M68K BUS"),saved=sram_read(expected.selectedSlot.selectedFlagPhysicalAddress)},storedChecksumByte=sram_read(expected.selectedSlot.selectedChecksumAddress),saveFlags=sram_read(config.runtimeContract.persistence.saveFlagsAddress),finalA6Output=emu.getregister("M68K A6")&0xFFFFFF,chronology=chronology};pending_trampoline_return={kind="persistence",stackPointer=trampoline_stack_pointer,deadline=frames+60}
  end
end
local function trampoline_return()
  if not active then return end
  assert(not transition or not transition.active,"story-state trampoline returned before probe entry")
  assert(pending_trampoline_return,"story-state trampoline return before generated completion")
  if a7()~=pending_trampoline_return.stackPointer then fail("story-state trampoline call/return stack imbalance",{domain="callStack",address=a7(),expected=pending_trampoline_return.stackPointer,actual=a7()});return end
  restoration.callStack=true;next_case()
end
local function wrapper_bypass()
  if transition and transition.active then
    current_phase,current_role,current_expectation="wrapper-transition","wrapper-bypass",transition
    fail("story-state wrapper-to-probe transition bypassed before patched callsite")
  end
end
local function finish(code)
  if code~=0 then cleanup_session();client.exitCode(code);return end
  if replay_state then memorysavestate.loadcorestate(replay_state) end
  local restored,mismatch=restore_scopes()
  if not restored then
    current_phase,current_role,current_expectation="cleanup","trampoline-return",nil
    fail("story-state scoped restoration drift",mismatch)
    return
  end
  cleanup_session();status("milestone:callbacks-cleared:0");status("milestone:observer-finished")
  json.write(config.outputPath,{system=emu.getsystemid(),core="Genesis Plus GX",id=config.fixtureId,mapTest=config.mapTest,recordOrder=(function()local t={};for _,case in ipairs(config.cases)do t[#t+1]=case.id end;return t end)(),records=records,callbacksCleared=0,scopedSramRestored=restoration.sram,restoration=restoration_json()})
  client.exitCode(0)
end

add_callback(config.harness["function"].numberPromptAddress,"story-number",function()prompt_count=prompt_count+1;if prompt_count==1 then stage="map";pending_save=true;pulse("C")end end)
add_callback(config.harness["function"].flagPromptAddress,"story-flag",function()pulse("B")end)
add_callback(route().wrapperEntryAddress,"story-entry",begin)
add_callback(route().outerCallSiteAddress,"wrapper-callsite",function()transition_event("wrapper-callsite",route().outerCallSiteAddress,"trampoline-entry")end)
add_callback(route().trampolineEntryAddress,"trampoline-entry",function()transition_event("trampoline-entry",route().trampolineEntryAddress,"trampoline-jsr")end)
add_callback(route().innerCallSiteAddress,"trampoline-jsr",function()transition_event("trampoline-jsr",route().innerCallSiteAddress,"probe-entry")end)
add_callback(route().probeEntryAddress,"probe-entry",actual_probe_entry)
add_callback(route().innerReturnAddress,"trampoline-return",trampoline_return)
add_callback(route().bypassAddress,"wrapper-bypass",wrapper_bypass)
for _,handler in ipairs(config.runtimeContract.handlerRecords) do
  add_callback(handler.h1Address,"mutation-handler-entry",function()persistence_handler_entry(handler.h1Address)end)
  for _,site in ipairs(handler.cursorUseSites) do add_callback(site.h1Address,"story-use-"..site.id,function()if active and input().kind=="v1" then trace("use",site.h1Address,nil,nil)end end)end
  for _,call in ipairs(handler.directCalls) do add_callback(call.h1Address,"mutation-call",function()if active and input().kind=="v1" then trace("call",call.h1Address,call.instructionTarget,call.effectiveTarget) else persistence_call(handler.handler,call.h1Address,call)end end)end
end
for _,address in ipairs({probe().programAddress+12,config.runtimeContract.persistence.saveGameAddress,probe().programAddress+20,config.runtimeContract.persistence.loadGameAddress,probe().programAddress+28,probe().programAddress+40}) do add_callback(address,"mutation-return",function()persistence_program(address)end)end
install_callbacks()
while true do
  frames=frames+1
  if pending_finish then finish(0) elseif pending_save then pending_save=false;replay_state=memorysavestate.savecorestate() elseif pending_replay then pending_replay=false;memorysavestate.loadcorestate(replay_state);queue={};pulse("C") end
  if transition and transition.active and frames>transition.deadline then current_phase,current_role,current_expectation="wrapper-transition","wrapper-transition-watchdog",transition;fail("story-state wrapper-to-probe transition watchdog expired") end
  if pending_trampoline_return and frames>pending_trampoline_return.deadline then current_phase,current_role,current_expectation="trampoline-return","trampoline-return",{eventPc=route().innerReturnAddress,callPc=route().innerCallSiteAddress,targetPc=route().innerTargetAddress,returnPc=route().innerReturnAddress};fail("story-state trampoline return watchdog expired") end
  if frames>=config.maxFrames then fail("story-state observer timeout") end
  local b=nil;if stage=="cheat" then local p=memory.read_u32_be(config.harness.ram.cheatPointerAddress,"M68K BUS");if p>=0x28FF0 and p<0x29000 then b=names[cheat[p-0x28FF0+1]] elseif memory.read_u8(config.harness.ram.debugModeAddress,"M68K BUS")==255 then b="Down"end elseif #queue>0 then b=table.remove(queue,1)end
  set_button(b);joypad.set({},2);emu.frameadvance()
end
