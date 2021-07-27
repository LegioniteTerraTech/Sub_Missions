using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sub_Missions.ManWindows;
using TAC_AI.AI;

namespace Sub_Missions.Steps
{
    public class StepActBoost : SMissionStep
    {
        public override void TrySetup()
        {
        }
        public override void Trigger()
        {   // 
            if (SMUtil.BoolOut(ref SMission))
            {
                Tank target = SMUtil.GetTrackedTech(ref SMission, SMission.InputString);
                try
                {
                    target.GetComponent<AIECore.TankAIHelper>().OverrideAllControls = true;
                    target.control.BoostControlJets = true;
                }
                catch
                {
                    Debug.Log("SubMissions: Could not fly away Tech as this action requires TACtical AIs to execute correctly!");
                }
            }
        }
    }
}
