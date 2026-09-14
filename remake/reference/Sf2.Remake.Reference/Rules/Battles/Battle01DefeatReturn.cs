using Sf2.Remake.Domain.Maps;

namespace Sf2.Remake.Domain.Battles;

// A distinct instance belongs to one preparation and enters the battle only at initialization.
public sealed class Battle01DefeatReturnAdmission
{
    public Battle01DefeatReturnAdmission(byte egressMap, bool flag64, bool flag640)
    {
        if (egressMap != 3 || flag64 || flag640)
            throw new ArgumentException("Only explicit Granseal egress3/F64false/F640false is admitted.", "return.inputs");
    }
    public byte EgressMap => 3;
    public bool Flag64 => false;
    public bool Flag640 => false;
}
