using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.Steps
{
    public class StepActRandom : SMissionStep
    {
        public override void TrySetup()
        {
            if (SMission.InputNum == 0)
                Mission.VarInts[SMission.SetGlobalIndex1] = UnityEngine.Random.Range(0, 100);
        }
        public override void Trigger()
        {
            if (SMission.InputNum == 0)
                return;// disabled, only fires once
            switch (SMission.VaribleType)
            {
                case EVaribleType.True: // Random bool
                    Mission.VarTrueFalse[SMission.SetGlobalIndex1] = UnityEngine.Random.Range(0, 100) > SMission.InputNum;
                    break;
                case EVaribleType.False:
                    Mission.VarTrueFalse[SMission.SetGlobalIndex1] = UnityEngine.Random.Range(0, 100) < SMission.InputNum;
                    break;
                case EVaribleType.Float:
                    Mission.VarInts[SMission.SetGlobalIndex1] = UnityEngine.Random.Range((int)Mathf.Min(SMission.InputNum, 0), (int)Mathf.Max(0, SMission.InputNum));
                    break;
                default:
                    SMUtil.Assert(true, "SubMissions: ActRandom's VaribleType must be set to either True, False, or Float.  Mission " + Mission.Name + ", Step " + Mission.EventList.IndexOf(SMission));
                    break;
            }
        }
    }
}
