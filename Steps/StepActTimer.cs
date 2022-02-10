using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.Steps
{
    public class StepActTimer : SMissionStep
    {
        public override void TrySetup()
        {
        }
        public override void Trigger()
        {
            try
            {
                switch (SMission.VaribleType)
                {
                    case EVaribleType.True: // tick up with bool
                        if (Mission.VarTrueFalse[SMission.SetGlobalIndex1])
                            Mission.VarInts[SMission.SetGlobalIndex2] += (int)SMission.InputNum;
                        break;
                    case EVaribleType.False: // tick up with bool
                        if (Mission.VarTrueFalse[SMission.SetGlobalIndex1])
                            Mission.VarInts[SMission.SetGlobalIndex2] += (int)SMission.InputNum;
                        break;
                    default:    // tick always
                        Mission.VarInts[SMission.SetGlobalIndex2] += (int)SMission.InputNum;
                        break;
                }
            }
            catch
            {
                SMUtil.Assert(true, "SubMissions: Error in output [SetGlobalIndex1] or [SetGlobalIndex2] in mission " + SMission.Mission.Name + " | Step type " + SMission.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) \nand make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry on the list, 1 counts as the second entry, and so on.");
            }
        }
    }
}
