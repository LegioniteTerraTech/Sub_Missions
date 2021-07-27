using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sub_Missions.ManWindows;

namespace Sub_Missions.Steps
{
    public class StepActMessagePurge : SMissionStep
    {
        public override void TrySetup()
        {
        }
        public override void Trigger()
        {   // purge the text box things
            if (SMUtil.BoolOut(ref SMission))
            {   // PURGE
                if (SMission.InputNum == 1)
                {   // one means it triggers only once
                    if (SMission.SavedInt != -2)
                    {
                        Mission.PurgeAllActiveMessages();
                        SMission.SavedInt = -2;
                    }
                    return;
                }
                Mission.PurgeAllActiveMessages();
            }
        }
    }
}
