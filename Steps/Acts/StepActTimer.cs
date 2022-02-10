using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.Steps
{
    public class StepActTimer : SMissionStep
    {
        public override string GetDocumentation()
        {
            return
                "{  // Constantly increments the specified VarInt while true on each SubMission update." +
                  "\n  \"StepType\": \"ActTimer\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"None\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that determines if it should execute." +
                  "\n  // Output Actions" +
                  "\n  \"SetMissionVarIndex2\": -1,       // The VarInt index that is updated while it is true." +
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
            try
            {
                switch (SMission.VaribleType)
                {
                    case EVaribleType.True: // tick up with bool
                        if (Mission.VarTrueFalse[SMission.SetMissionVarIndex1])
                            Mission.VarInts[SMission.SetMissionVarIndex2] += (int)SMission.InputNum;
                        break;
                    case EVaribleType.False: // tick up with bool
                        if (Mission.VarTrueFalse[SMission.SetMissionVarIndex1])
                            Mission.VarInts[SMission.SetMissionVarIndex2] += (int)SMission.InputNum;
                        break;
                    default:    // tick always
                        Mission.VarInts[SMission.SetMissionVarIndex2] += (int)SMission.InputNum;
                        break;
                }
            }
            catch
            {
                SMUtil.Assert(true, "SubMissions: Error in output [SetMissionVarIndex1] or [SetMissionVarIndex2] in mission " + SMission.Mission.Name + " | Step type " + SMission.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) \nand make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry on the list, 1 counts as the second entry, and so on.");
            }
        }
    }
}
