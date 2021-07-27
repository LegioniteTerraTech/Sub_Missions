using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.Steps
{
    public class StepActForward : SMissionStep
    {
        public override void TrySetup()
        {
        }
        public override void Trigger()
        {
            if (SMUtil.BoolOut(ref SMission))
            {
                SMUtil.ProceedID(ref SMission);
            }
        }
    }
}
