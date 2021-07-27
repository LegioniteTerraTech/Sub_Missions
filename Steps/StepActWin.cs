using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.Steps
{
    public class StepActWin : SMissionStep
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
                        Mission.Finish();
                    }
                    break;
                case EVaribleType.False: // when set global is false
                    if (!Mission.VarTrueFalse[SMission.SetGlobalIndex1])
                    {
                        Mission.Finish();
                    }
                    break;
                case EVaribleType.None: 
                default: // immedate proceed
                    Mission.Finish();
                    break;
            }
        }
    }
}
