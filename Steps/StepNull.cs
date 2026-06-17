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
        public override bool ForceUsesVarBool() => false;
        public override bool ForceUsesVarInt() => false;
        public override string GetTooltip() =>
            "THIS SHOULD NEVER BE IN YOUR MISSION UNDER ANY CIRCUMSTANCES!!!";
        public override string GetDocumentation()
        {
            return
                "{  // " + GetTooltip() +
                  "\n  \"StepType\": null,  // all it needs is a poorly spelt StepType to ruin everything. \n    // WATCH YOUR TYPING AND SYNTAX!!" +
                "\n},";
        }
        public override void InitGUI() { }
        public override void OnInit() { }

        public override void OnDeInit()
        {
        }
        public override void FirstSetup()
        {
            SMUtil.Error(false, SMission.LogName, 
                KickStart.ModID + ": Mission " + Mission.Name + 
                " has a SubMissionStep error at ProgressID " + SMission.ProgressID + 
                ", Type !NULL!, and will not be able to execute. \nStepType is empty or " +
                "contains an invalid/mispelled value!");
        }
        public override void Trigger()
        {
            SMUtil.Error(true, SMission.LogName, 
                KickStart.ModID + ": NULL SubMissionStep.StepType in " + Mission.Name + 
                " |  There should NEVER be a null StepType in a mission.  \n  Watch your typing and syntax!");
        }
    }
}
