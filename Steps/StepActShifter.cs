using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sub_Missions.Steps
{
    public class StepActShifter : SMissionStep
    {   // The Shifter acts as a possible "update" gate for ProgressIDs
        //   Say, the Shifter's Progress ID always remains true but
        //   When the Shifter is Shifting (up one)
        //   The value the opposite direction is no longer updated
        //   The value that the Shifter moved to remains updating
        //   The value two shifts right begins to get updated
        // BUT:
        //   The Shifter is greedy since there can only be one per ID and they cannot be safely adjacent.
        public override void TrySetup()
        {
        }
        public override void Trigger()
        {   // run the text box things
            switch (SMission.VaribleType)
            {
                case EVaribleType.True:
                    if (Mission.VarTrueFalse[SMission.SetGlobalIndex1])
                    {
                        SMUtil.ShiftCurrentID(ref SMission);
                        return;
                    }
                    break;
                case EVaribleType.False:
                    if (!Mission.VarTrueFalse[SMission.SetGlobalIndex1])
                    {
                        SMUtil.ShiftCurrentID(ref SMission);
                        return;
                    }
                    break;
                case EVaribleType.IntGreaterThan:
                    if (Mission.VarInts[SMission.SetGlobalIndex1] > (int)SMission.InputNum)
                    {
                        SMUtil.ShiftCurrentID(ref SMission);
                        return;
                    }
                    break;
                case EVaribleType.IntLessThan:
                    if (Mission.VarInts[SMission.SetGlobalIndex1] < (int)SMission.InputNum)
                    {
                        SMUtil.ShiftCurrentID(ref SMission);
                        return;
                    }
                    break;
                case EVaribleType.None:
                    SMUtil.ShiftCurrentID(ref SMission);
                    return;
            }
            SMUtil.ReturnCurrentID(ref SMission);// Branch off while player doing something else;
        }
    }
}
