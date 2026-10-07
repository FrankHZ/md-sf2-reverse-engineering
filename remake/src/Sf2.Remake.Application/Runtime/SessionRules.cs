using Sf2.Remake.Domain.Battles;

namespace Sf2.Remake.Application.Runtime;

// Composition is selected before start and retained for the life of the session.
public sealed class SessionRules
{
    internal SessionRules(string identity, IHealingRule healing)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identity);
        ArgumentNullException.ThrowIfNull(healing);
        Identity = identity; Healing = healing;
    }
    public string Identity { get; }
    internal IHealingRule Healing { get; }
}
