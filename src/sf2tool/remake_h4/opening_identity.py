"""Join original candidate, host, observer completion and named restoration."""

from sf2tool.remake_h4_reference import ROM, UPSTREAM


def compare_identity(context, checks):
    check, match, absent = checks.check, checks.match, checks.absent
    check(
        "selected controlled original identities",
        match(
            dict(
                SourceCommit=UPSTREAM,
                RomSha256=ROM,
                ConfigurationSha256="C72A2547D51BBE97CBEB6D7DB51E5FCCDDD5507917735CB11554F8461D69DAF7",
                ObserverSha256="738E66DD261CAD1D2077E64EAC85E33A0CADC66C8712ED32F619A2818E747530",
                RunnerSha256="CC1823FDF94A29A3C415F4535C68799DF5D900C75DC6951C5260E0134289535E",
                InputSha256="991A51636102F00D837F605561538A39B60E7621370D523CFC412D52ECDDEB5E",
            ),
            context.get("candidate", absent),
        ),
    )
    check(
        "completed original boundary",
        match(
            dict(
                status="OPENING-DIAGNOSTIC-COMPLETE-UNREVIEWED",
                stopReason="opening-script-return",
                canonicalRomUnchanged=True,
                sessionRomDeleted=True,
                process=dict(returncode=0, timed_out=False, process_terminated=True, error=None),
            ),
            context.get("host", absent),
        ),
    )
    check(
        "original restored selected scope",
        match(
            dict(
                scopeArmed=True,
                gameFlags=True,
                combatantAllyRecords=True,
                mapAndBattleState=True,
                playerEntity=True,
                forceAndParty=True,
                followerState=True,
                touchedEntities=True,
                bootstrapFrame=True,
                gold=True,
                generatedRam=True,
                callbacksCleared=True,
                sessionStateRestored=True,
                sessionCartPatches=True,
                dialogueAndInput=True,
                cameraState=True,
            ),
            (context.get("observed") or {}).get("restoration", absent),
        ),
    )
    # These are the existing fixed-input producer fields. outputRemoved is not
    # a restoration success flag: False intentionally retains the evidence.
    candidate = context.get("candidate") or {}
    host = context.get("host") or {}
    observed = context.get("observed") or {}
    diagnostic = observed.get("diagnostic") or {}
    check(
        "candidate fixed-input opening profile",
        match(
            dict(
                InputFrames=655,
                InputClock="first-r1-wait-next-frame",
                Diagnostic=dict(
                    kind="admission-opening-scalar-diagnostic",
                    program=0x5145C,
                    sourceCommit=UPSTREAM,
                ),
            ),
            candidate,
        ),
    )
    check(
        "host executed reviewed candidate",
        match(context.get("candidate", absent), host.get("reviewedMaterial", absent)),
    )
    check(
        "host runtime matches candidate",
        match(
            {k: candidate.get(k, absent) for k in ("ExecutableSha256", "LuaLibrarySha256")},
            host.get("runtimeIdentities", absent),
        ),
    )
    check(
        "observer completed opening",
        match(
            dict(
                kind="bounded-original-observation",
                stopReason="opening-script-return",
                inputIdentity="991A51636102F00D837F605561538A39B60E7621370D523CFC412D52ECDDEB5E",
                diagnostic=dict(
                    kind="admission-opening-scalar-diagnostic", r1=True, programReturned=True
                ),
            ),
            observed,
        ),
    )
    check(
        "observer input belongs to candidate",
        match(candidate.get("InputSha256", absent), observed.get("inputIdentity", absent)),
    )
    check(
        "observer completion joins host",
        match(host.get("stopReason", absent), observed.get("stopReason", absent)),
    )
    return candidate, observed, diagnostic
