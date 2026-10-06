using static H4Comparison.ReportIntegrityOperands;
namespace H4Comparison;

internal static class RequiredObligations
{
    internal sealed record Family(string? Parent, List<string> Children);
    public static async ValueTask<List<Family>> Build(object? variant, object? reference, ReportIntegrityOperands o)
    {
        var allies = new List<object?>();
        var rows = await o.Field(await o.Field(await o.Field(reference, "admission"), "accounting"), "allies");
        await foreach (var ally in o.FirstThree(rows)) allies.Add(await o.Field(ally, "id"));
        var families = new List<Family>();
        void Set(string? parent, List<string> children)
        {
            var index = families.FindIndex(f => f.Parent == parent);
            if (index < 0) families.Add(new Family(parent, children));
            else families[index] = new Family(parent, children);
        }
        var children0 = new List<string>();
        children0.Add("map");
        children0.Add("x");
        children0.Add("y");
        children0.Add("facing");
        children0.Add("gold");
        children0.Add("mainSeed");
        foreach (var ally in allies) foreach (var field in new[] { "Hp", "Mp", "Status" }) children0.Add($"ally-{Text(ally)}.{field}");
        children0.Add("admission roster");
        children0.Add("one continuous session");
        children0.Add("monotonic observed sequence");
        children0.Add("full result stream");
        children0.Add("ordinary route and probe assertions");
        children0.Add("mandatory reached checkpoints");
        children0.Add("natural battle first control");
        children0.Add("adaptive actions and consumed outcome");
        children0.Add("whole after/return order");
        children0.Add("victory flags/program/return");
        children0.Add("accepted original Down extension selection");
        children0.Add("returned actual field input");
        foreach (var label in new[] { "first-return", "before-down", "after-down" }) children0.Add(label + " two settled host updates");
        children0.Add("ordinary Down displacement");
        children0.Add("clean actual process");
        children0.Add("actual physical/consumer input records");
        children0.Add("configured device and release observations");
        children0.Add("complete named continuous settings matrix");
        children0.Add("modern finite-music deviation declaration");
        children0.Add("observed input dispatch intervals");
        children0.Add("host delivery adds no gameplay or RNG service");
        if (await o.Same(variant, "C")) children0.Add("actual reveal-only Confirm");
        if (await o.Same(variant, "D")) children0.Add("natural reveal before Confirm");
        Set(null, children0);
        var children1 = new List<string>();
        children1.Add("after-program join/flag/return causal order");
        children1.Add("taken route/setup/caller branch operands and occurrence");
        children1.Add("awaited entity motion/gesture/fade before caller return");
        children1.Add("dialogue speaker/control-token occurrence and choice effect");
        children1.Add("route roster/flag writes at their source branch");
        children1.Add("warp destination/setup initialization before field release");
        children1.Add("before/after operation effects and shared-tail return pairing");
        Set("complete mandatory operation-to-consumption mapping", children1);
        var children2 = new List<string>();
        children2.Add("repeated logical sequence identifies the same observation");
        children2.Add("recorded main draw operands are available");
        children2.Add("turn candidate score draws and tie/order result");
        children2.Add("physical range/dodge/critical/spread/double/counter effects");
        children2.Add("HEAL recovery/cost/fairy opportunity and seed effects");
        children2.Add("EXP/gold/growth/spell learning and after-turn/outcome effects");
        children2.Add("AI thinking draw/choice/memory and movement decision");
        children2.Add("field text/portrait/NPC service draw-to-effect gates");
        Set("matched-state rule/RNG/draw-to-effect comparisons", children2);
        var children3 = new List<string>();
        children3.Add("reached map3/19/20/21/40/57 atlas and layer identities");
        children3.Add("reached entity sprites/portraits/gesture resource identities");
        children3.Add("displayed text tokens/font/glyph private binding");
        children3.Add("scene actor/weapon/healing/death resources");
        children3.Add("scene background/ground actual resource identity");
        children3.Add("reached audio command/timer/PCM provenance and playback lifecycle");
        Set("complete reached 7C resource/provenance inventory", children3);
        var children4 = new List<string>();
        children4.Add("W1 displayed token occurrence/accepting read/service gates");
        children4.Add("W2 accepting read/validation indicator and token return");
        children4.Add("plain JOIN input after matching finite completion");
        children4.Add("entity motion/gesture/fade consumer start/completion before resume");
        children4.Add("battle scene command/resources/wait/effect/end consumer edges");
        children4.Add("audio replacement/fade/stop/resume dependent consumer edges");
        Set("required unshimmed ack and scene consumer binding", children4);
        var children5 = new List<string>();
        children5.Add("admission joined");
        children5.Add("admission active");
        children5.Add("admission logical consumer readiness");
        children5.Add("admission seed-copy byte");
        children5.Add("opening mouth/view controls before first source write");
        children5.Add("admission occupied physical slots");
        children5.Add("effective admission class/level/maxima/stats/spells definition identity");
        children5.Add("walking motion gate/velocity/travel/flags correspondence");
        foreach (var slot in new[] { 5, 6, 8 }) foreach (var field in new[] { "wait timer", "cursor/moving source binding" }) children5.Add($"admission walking slot {slot} {field}");
        foreach (var ally in allies) children5.Add($"ally-{Text(ally)} candidate class/stats/spell words");
        Set("complete relevant admission phase/field mapping", children5);
        foreach (var ally in allies)
        {
            var parent = $"ally-{Text(ally)}.items";
            Set(parent, new[] { "effective four-slot words", "candidate definition slots", "admission loadout identity" }.Select(field => parent + "." + field).ToList());
        }
        var admission = await o.Field(reference, "admission");
        var endpoint = await o.Field(reference, "endpoint");
        foreach (var (parent, prefix, state) in new[]
        {
            ("complete relevant admission phase/field mapping", "admission", admission),
            ("complete mandatory operation-to-consumption mapping", "returned story", endpoint)
        })
        {
            var flags = await o.Field(await o.Field(state, "state"), "flags");
            await foreach (var flag in o.Rows(flags)) families.First(f => f.Parent == parent).Children.Add($"{prefix} flag {Text(flag)}");
        }
        var entities = await o.Field(await o.Field(reference, "inherited"), "entities");
        await foreach (var entity in o.Rows(entities))
        {
            if (await o.Same(await o.Field(entity, "actionScript"), System.Numerics.BigInteger.Zero)
                && await o.Same(await o.Field(entity, "x"), await o.Field(entity, "y"))
                && await o.Same(await o.Field(entity, "y"), new System.Numerics.BigInteger(0x7000))) continue;
            families.First(f => f.Parent == "complete relevant admission phase/field mapping").Children.Add(
                $"admission slot {Text(await o.Field(entity, "physical"))} position/destination/facing/layer");
        }
        return families;
    }
}
