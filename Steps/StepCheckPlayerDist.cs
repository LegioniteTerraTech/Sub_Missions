using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.Steps
{
    public class StepCheckPlayerDist : SMissionStep
    {
        public override void TrySetup()
        {
        }
        public override void Trigger()
        {   // check player dist
            if (SMission.InputString == "")
            {
                if (SMission.InputNum > 0)
                {
                    if (SMUtil.IsPlayerInRangeOfPos(SMission.Position, SMission.InputNum))
                        SMUtil.ConcludeGlobal1(ref SMission);
                }
                else
                {   //invert detection trigger
                    if (!SMUtil.IsPlayerInRangeOfPos(SMission.Position, -SMission.InputNum))
                        SMUtil.ConcludeGlobal1(ref SMission);
                }
            }
            else
            {
                if (SMission.InputNum > 0)
                {
                    if (SMUtil.IsPlayerInRangeOfPos(SMUtil.GetTrackedTech(ref SMission, SMission.InputString).boundsCentreWorld, SMission.InputNum))
                        SMUtil.ConcludeGlobal1(ref SMission);
                }
                else
                {   //invert detection trigger
                    if (!SMUtil.IsPlayerInRangeOfPos(SMUtil.GetTrackedTech(ref SMission, SMission.InputString).boundsCentreWorld, -SMission.InputNum))
                        SMUtil.ConcludeGlobal1(ref SMission);
                }
            }
        }
    }
}
