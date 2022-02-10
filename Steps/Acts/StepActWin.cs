using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.Steps
{
    public class StepActWin : SMissionStep
    {
        public override string GetDocumentation()
        {
            return
                "{  // Wins and ends the mission, giving the player any set SubMissionRewards." +
                  "\n  \"StepType\": \"ActWin\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"None\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that determines if it should execute." +
                "\n},";
        }

        public override void OnInit() { }

        public override void FirstSetup()
        {
        }
        public override void Trigger()
        {
            try
            {
                switch (SMission.VaribleType)
                {
                    case EVaribleType.True: // when set global is true
                        if (Mission.VarTrueFalse[SMission.SetMissionVarIndex1])
                        {
                            Mission.Finish();
                        }
                        break;
                    case EVaribleType.False: // when set global is false
                        if (!Mission.VarTrueFalse[SMission.SetMissionVarIndex1])
                        {
                            Mission.Finish();
                        }
                        break;
                    case EVaribleType.None:
                    default: // immedate proceed
                        Mission.Finish();
                        break;
                }
            }
            catch
            {
                SMUtil.Assert(true, "SubMissions: Error in output [SetMissionVarIndex1] in mission " + SMission.Mission.Name + " | Step type " + SMission.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) \nand make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry on the list, 1 counts as the second entry, and so on.");
            }
        }
        public override void OnDeInit()
        {
        }
    }
}
