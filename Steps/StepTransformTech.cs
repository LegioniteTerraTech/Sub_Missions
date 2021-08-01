using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.AI;
using TAC_AI.AI.Enemy;
using TAC_AI.Templates;

namespace Sub_Missions.Steps
{
    public class StepTransformTech : SMissionStep
    {
        public override void TrySetup()
        {  
        }
        public override void Trigger()
        {   
            if (SMUtil.BoolOut(ref SMission))
            {
                try
                {
                    TrackedTech tTech = SMUtil.GetTrackedTechBase(ref Mission, SMission.InputString);

                    if (SMission.InputStringAux == "Team")
                    {
                        if (SMission.SavedInt < 1)
                        {
                            tTech.Tech.SetTeam((int)SMission.InputNum);
                            Debug.Log("SubMissions: Changed team of tech " + tTech.Tech.name + " to " + tTech.Tech.Team);
                        }
                    }
                    else if (SMission.InputStringAux != "" || SMission.InputStringAux != null)
                    {   // allow the AI to change it's form on demand
                        if (SMission.SavedInt < SMission.InputNum)
                        {
                            Tank techCur = tTech.Tech;
                            Debug.Log("SubMissions: More than meets the eye");
                            tTech.Tech = RawTechLoader.TechTransformer(techCur, SMission.InputStringAux);
                        }
                    }
                    else
                    {
                        SMUtil.Assert(true, "SubMissions: StepTransformTech - Failed: InputStringAux does not contain a valid RAWTechJSON Blueprint.  Mission " + Mission.Name);
                    }
                }
                catch (Exception e)
                {   // Cannot work without TACtical_AI
                    if (KickStart.isTACAIPresent)
                    {
                        SMUtil.Assert(true, "SubMissions: StepTransformTech  - Failed: COULD NOT FETCH TECH INFORMATION!!!");
                        Debug.Log("SubMissions: Stack trace - " + StackTraceUtility.ExtractStackTrace());
                        Debug.Log("SubMissions: Error - " + e);
                    }
                    Debug.Log("SubMissions: StepTransformTech  - Failed: TACticial_AIs is not installed ~ Unable to execute");
                }
                SMission.SavedInt++;
            }
        }
    }
}
