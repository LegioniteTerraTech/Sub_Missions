using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.Steps
{
    public class StepActFail : SMissionStep
    {
        public override bool ForceUsesVarBool() => false;
        public override bool ForceUsesVarInt() => false;
        public override string GetTooltip() =>
            "Fails and ends the mission, giving the player good grief.";

        public override string GetDocumentation()
        {
            return
                "{  // " + GetTooltip() +
                  "\n  \"StepType\": \"ActFail\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"None\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that determines if it should execute." +
                "\n},";
        }
        public override void InitGUI()
        {
            AddField(ESMSFields.VaribleType, "Condition Mode");
            AddField(ESMSFields.VaribleCheckNum, "Conditional Constant");
            AddField(ESMSFields.SetMissionVarIndex1, "Fail Condition");
        }

        public override void OnInit() { }

        public override void OnDeInit()
        {
        }
        public override void FirstSetup()
        {
            triggered = false;
        }
        /// <summary>
        /// Prevent ENDLESS LOOP from happening!
        /// </summary>
        private bool triggered = false;
        public override void Trigger()
        {
            switch (SMission.VaribleType)
            {
                case EVaribleType.True: // when set global is true
                    if (Mission.VarTrueFalseActive[SMission.SetMissionVarIndex1])
                    {
                        Mission.Fail();
                    }
                    break;
                case EVaribleType.False: // when set global is false
                    if (!Mission.VarTrueFalseActive[SMission.SetMissionVarIndex1])
                    {
                        Mission.Fail();
                    }
                    break;
                case 0:
                default: // immedate proceed
                    Mission.Fail();
                    break;
            }
        }
    }
}
