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
        public override string GetDocumentation()
        {
            return
                "{  // Tells a TrackedTech to fire it's boosters (REQUIRES TACTICAL AI TO FUNCTION)" +
                  "\n  \"StepType\": \"ActBoost\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"None\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that determines if it should execute." +
                  "\n  // Input Parameters" +
                  "\n  \"InputString\": \"TechName\",   // The name of the TrackedTech to command." +
                "\n},";
        }

        public override void OnInit() { }

        public override void OnDeInit()
        {
        }
        public override void FirstSetup()
        {
        }
        public override void Trigger()
        {   // 
            if (SMUtil.BoolOut(ref SMission))
            {
                if (SMUtil.GetTrackedTech(ref SMission, SMission.InputString, out Tank target))
                {
                    try
                    {
                        target.GetComponent<AIECore.TankAIHelper>().OverrideAllControls = true;
                        target.control.BoostControlJets = true;
                    }
                    catch
                    {
                        Debug_SMissions.Log("SubMissions: Could not fly away Tech as this action requires TACtical AIs to execute correctly!");
                    }
                }
            }
        }
    }
}
