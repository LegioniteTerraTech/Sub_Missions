using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sub_Missions.Steps
{
    public class StepActShifter : SMissionStep
    {   // The Shifter acts as a possible "update" gate for ProgressIDs
        //   Say, the Shifter's Progress ID always remains true but
        //   When the Shifter is Shifting (up one)
        //   The value the opposite direction is no longer updated
        //   The value that the Shifter moved to remains updating
        //   The value two shifts right begins to get updated
        // BUT:
        //   The Shifter is greedy since there can only be one per ID and they cannot be safely adjacent.

        public override string GetDocumentation()
        {
            return
                "{  // Shifts the ProgressID by +1 when the input variable is true" +
                "\n  // The Shifter acts as a possible \"update\" gate for ProgressIDs " +
                "\n  //   Say, the Shifter's Progress ID always remains true but " +
                "\n  //   When the Shifter is Shifting (up one) " +
                "\n  //   The value the opposite direction is no longer updated " +
                "\n  //   The value that the Shifter moved to remains updating " +
                "\n  //   The value two shifts right begins to get updated " +
                "\n  // BUT: " +
                "\n  //   The Shifter is greedy since there can only be one per ID and they cannot be safely adjacent. " +
                "\n  \"StepType\": \"ActShifter\"," +
                "\n  \"ProgressID\": 0,             // " + StepDesc +
                "\n  \"RevProgressIDOffset\": false,// Should this shift -1 instead?" +
                "\n  // Conditions TO CHECK before executing" +
                "\n  \"VaribleType\": \"True\",       // See the top of this file." +
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
        {   // run the text box things
            switch (SMission.VaribleType)
            {
                case EVaribleType.True:
                    if (Mission.VarTrueFalse[SMission.SetMissionVarIndex1])
                    {
                        SMUtil.ShiftCurrentID(ref SMission);
                        return;
                    }
                    break;
                case EVaribleType.False:
                    if (!Mission.VarTrueFalse[SMission.SetMissionVarIndex1])
                    {
                        SMUtil.ShiftCurrentID(ref SMission);
                        return;
                    }
                    break;
                case EVaribleType.IntGreaterThan:
                    if (Mission.VarInts[SMission.SetMissionVarIndex1] > (int)SMission.InputNum)
                    {
                        SMUtil.ShiftCurrentID(ref SMission);
                        return;
                    }
                    break;
                case EVaribleType.IntLessThan:
                    if (Mission.VarInts[SMission.SetMissionVarIndex1] < (int)SMission.InputNum)
                    {
                        SMUtil.ShiftCurrentID(ref SMission);
                        return;
                    }
                    break;
                case EVaribleType.None:
                    SMUtil.ShiftCurrentID(ref SMission);
                    return;
            }
            SMUtil.ReturnCurrentID(ref SMission);// Branch off while player doing something else;
        }
    }
}
