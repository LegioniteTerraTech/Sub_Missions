using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.Steps
{
    public class StepNull : SMissionStep
    {
        public override string GetDocumentation()
        {
            return
                "{  // THIS SHOULD NEVER BE IN YOUR MISSION UNDER ANY CIRCUMSTANCES!!!" +
                  "\n  \"StepType\": null,  // all it needs is a poorly spelt StepType to ruin everything. \n    // WATCH YOUR TYPING AND SYNTAX!!" +
                "\n},";
        }
        public override void OnInit() { }

        public override void OnDeInit()
        {
        }
        public override void FirstSetup()
        {
            SMUtil.Assert(false, "SubMissions: Mission " + Mission.Name + " has a SubMissionStep error at ProgressID " + SMission.ProgressID + ", Type !NULL!, and will not be able to execute. \nStepType is empty or contains an invalid/mispelled value!");
        }
        public override void Trigger()
        {
            SMUtil.Assert(true, "SubMissions: NULL SubMissionStep.StepType in " + Mission.Name + " |  There should never be a null StepType in a mission.  \n  Watch your typing and syntax!");
        }
    }
}
