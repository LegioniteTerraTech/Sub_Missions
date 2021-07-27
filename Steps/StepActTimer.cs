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
    }
}
