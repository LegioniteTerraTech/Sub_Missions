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
            SMission.hasTech = true;
            try
            {
                SMUtil.SpawnTechTracked(ref Mission, SMission.Position, (int)SMission.InputNum, SMission.Forwards, SMission.InputString);
            }
            catch (Exception e)
            {
                try
                {
                    SMUtil.Assert(false, "SubMissions: StepSetupTech - Failed: " + SMission.InputString + " finding failed.");
                    SMUtil.Assert(false, "SubMissions: StepSetupTech - Team " + SMission.InputNum);
                    SMUtil.Assert(false, "SubMissions: StepSetupTech - Mission " + Mission.Name);
                }
                catch
                {
                    SMUtil.Assert(false, "SubMissions: StepSetupTech - Failed: COULD NOT FETCH INFORMATION!!!");
                }
                Debug.Log("SubMissions: Error - " + e);
            }
        }
        public override void Trigger()
        {
            SMission.hasTech = true;
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
