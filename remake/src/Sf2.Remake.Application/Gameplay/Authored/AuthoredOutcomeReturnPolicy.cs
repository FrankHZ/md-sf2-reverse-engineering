using Sf2.Remake.Application.Gameplay.Sf2;

namespace Sf2.Remake.Application.Gameplay.Authored;

internal sealed class AuthoredOutcomeReturnPolicy : Sf2OutcomeReturnPolicy
{
    public override string Identity => "authored-fixed-defeat-loss";
    protected override uint DefeatGold(uint current) => current > 10 ? current - 10 : 0;
}
