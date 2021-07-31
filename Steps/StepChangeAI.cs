using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;

namespace Sub_Missions.Steps
{
    public class StepChangeAI : SMissionStep
    {
        public override void TrySetup()
        {   // Spawn target to kill
        }
        public override void Trigger()
        {   
            if (SMUtil.BoolOut(ref SMission) && SMission.InputStringAux == "Infinite")
            {  
                try
                {
                    SMUtil.SpawnTechAddTracked(ref Mission, SMission.Position, (int)SMission.InputNum, SMission.Forwards, SMission.InputString);
                }
                catch (Exception e)
                {
                    try
                    {
                        SMUtil.Assert(false, "SubMissions: StepSetupTech (Infinite) - Failed: " + SMission.InputString + " finding failed.");
                        SMUtil.Assert(false, "SubMissions: StepSetupTech (Infinite) - Team " + SMission.InputNum);
                        SMUtil.Assert(true, "SubMissions: StepSetupTech (Infinite) - Mission " + Mission.Name);
                    }
                    catch
                    {
                        SMUtil.Assert(true, "SubMissions: StepSetupTech (Infinite) - Failed: COULD NOT FETCH INFORMATION!!!");
                    }
                    //Debug.Log("SubMissions: Stack trace - " + StackTraceUtility.ExtractStackTrace());
                    Debug.Log("SubMissions: Error - " + e);
                }
            }
        }
    }
}
