using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;

namespace Sub_Missions.Steps
{
    public class StepSetupTech : SMissionStep
    {
        public override void TrySetup()
        {   // Spawn target to kill
            try
            {
                SMUtil.SpawnTechTracked(ref Mission, SMission.Position, (int)SMission.InputNum, SMission.Forwards, SMission.InputString);
            }
            catch
            {
                try
                {
                    Debug.Log("SubMissions: StepSetupTech - Failed: " + SMission.InputString + " finding failed.");
                    Debug.Log("SubMissions: StepSetupTech - Team " + SMission.InputNum);
                    Debug.Log("SubMissions: StepSetupTech - Mission " + Mission.Name);
                }
                catch
                {
                    Debug.Log("SubMissions: StepSetupTech - Failed: COULD NOT FETCH INFORMATION!!!");
                }
                Debug.Log("SubMissions: Stack trace - " + StackTraceUtility.ExtractStackTrace());
            }
        }
        public override void Trigger()
        {   
        }
    }
}
