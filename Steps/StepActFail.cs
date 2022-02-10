using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.Steps
{
    public class StepActFail : SMissionStep
    {
        public override void TrySetup()
        {
        }
        public override void Trigger()
        {
            switch (SMission.VaribleType)
            {
                case EVaribleType.True: // when set global is true
                    if (Mission.VarTrueFalse[SMission.SetGlobalIndex1])
                    {
                        Mission.Fail();
                    }
                    break;
                case EVaribleType.False: // when set global is false
                    if (!Mission.VarTrueFalse[SMission.SetGlobalIndex1])
                    {
                        Mission.Fail();
                    }
                    break;
                case 0:
                default: // immedate proceed
                    Mission.Fail();
                    break;
            }
        }
    }
}
