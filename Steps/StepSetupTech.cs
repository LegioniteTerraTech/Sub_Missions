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
            catch (Exception e)
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
                Debug.Log("SubMissions: Error - " + e);
            }
        }
        public override void Trigger()
        {   
            if (SMUtil.BoolOut(ref SMission) && SMission.InputStringAux == "Infinite")
            {   // we spawn infinite techs every second while this is active
                try
                {
                    SMUtil.SpawnTechAddTracked(ref Mission, SMission.Position, (int)SMission.InputNum, SMission.Forwards, SMission.InputString);
                }
                catch (Exception e)
                {
                    try
                    {
                        Debug.Log("SubMissions: StepSetupTech (Infinite) - Failed: " + SMission.InputString + " finding failed.");
                        Debug.Log("SubMissions: StepSetupTech (Infinite) - Team " + SMission.InputNum);
                        Debug.Log("SubMissions: StepSetupTech (Infinite) - Mission " + Mission.Name);
                    }
                    catch
                    {
                        Debug.Log("SubMissions: StepSetupTech (Infinite) - Failed: COULD NOT FETCH INFORMATION!!!");
                    }
                    //Debug.Log("SubMissions: Stack trace - " + StackTraceUtility.ExtractStackTrace());
                    Debug.Log("SubMissions: Error - " + e);
                }
            }
        }
    }
}
