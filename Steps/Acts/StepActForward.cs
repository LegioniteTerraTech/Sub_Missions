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
        public override string GetDocumentation()
        {
            return
                "{  // Progresses the Mission ProgressID when successful." +
                  "\n  \"StepType\": \"ActForward\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  \"SuccessProgressID\": 5,      // The ProgressID the mission will be pushed to if this is successful" +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"None\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that determines if it should execute." +
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
        {
            if (SMUtil.BoolOut(ref SMission))
            {
                SMUtil.ProceedID(ref SMission);
            }
        }
    }
}
