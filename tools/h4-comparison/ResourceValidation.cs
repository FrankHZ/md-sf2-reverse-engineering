using System.Numerics;
using static H4Comparison.Operands;
using static H4Comparison.ResourceOperands;

namespace H4Comparison;

internal sealed class ResourceValidation(CountedChecks checks, ResourceContext context, int digitLimit)
{
    private static string Family(object? row) => Equal(Get(row, "kind"), "map") ? "map" : "entity";

    public bool Available(object? row, bool requirement, BigInteger weight, object? locator)
    {
        var family = Family(row);
        var admitted = false;
        checks.Evaluated(family, "resource occurrence", weight, locator, () =>
        {
            var identity = At(row, "identity");
            foreach (var key in new[] { "sessionId", "visit", "map", "phase", "observationSequence" }) At(identity, key);
            At(row, "kind");
            At(row, requirement ? "expected" : "used");
            admitted = true;
        });
        return admitted;
    }

    public void Identity(object? row, BigInteger weight, object? locator)
    {
        var family = Family(row);
        checks.Evaluated(family, "resource identity", weight, locator, () =>
        {
            var identity = At(row, "identity");
            At(row, "kind");
            checks.Check(family, "same-session resource delivery identity", context.SessionCount == 0 ? null
                : context.SessionCount == 1 && context.HasSession(At(identity, "sessionId")), weight, locator);
            var visit = context.Latest(At(identity, "observationSequence"));
            checks.Check(family, "actual use belongs to its latest logical map visit",
                Equal(At(identity, "visit"), visit.Sequence) && Equal(At(identity, "map"), visit.Map), weight, locator);
        });
    }

    private static readonly Dictionary<Key, object?> passes = new()
    {
        [new Key("background")] = BigInteger.Zero, [new Key("foreground")] = BigInteger.One,
        [new Key("backgroundHigh")] = new BigInteger(2), [new Key("foregroundHigh")] = new BigInteger(3)
    };

    public void Texture(object? row, BigInteger weight, object? locator, object? membership)
    {
        var family = Family(row);
        checks.Evaluated(family, "actual texture use", weight, locator, () =>
        {
            if (row is Dictionary<string, object?> map && map.TryGetValue("_keyError", out var error))
            {
                At(row, "kind");
                checks.Check(family, Equal(error, "KeyError") ? "actual texture use operand absent"
                    : "actual texture use malformed " + (string)error!, Equal(error, "KeyError") ? null : false, weight, locator);
                return;
            }
            At(row, "identity");
            if (Equal(At(row, "kind"), "map"))
            {
                var high = Get(row, "highPriority");
                var word = At(At(row, "used"), "word");
                var name = Get(row, "layer"); var drawPass = Get(row, "pass");
                var valid = !Equal(name, "occlusion") ? Equal(Lookup(passes, name).Value, drawPass)
                    : Less(new BigInteger(5), drawPass) || Equal(drawPass, new BigInteger(5));
                checks.Check("map", "actual source tile priority and named layer pass", valid
                    && Equal(word, Integer(word, digitLimit))
                    && (high is null || Equal((Integer(word, digitLimit) & 0x8000) != 0, high)), weight, locator);
            }
            // Python's existing phase-key sets remain the storage authority. Any
            // lookup exception is deferred to this exact predicate boundary.
            if (Get(membership, "error") is { } kind)
                throw new OperandError((string)kind, (string?)Get(membership, "message"));
            checks.Check(family, "actual use phase has independent logical requirements",
                Truth(At(membership, "group")) ? At(membership, "phase") : null, weight, locator);
        });
    }
}
