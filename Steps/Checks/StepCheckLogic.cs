using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;

namespace Sub_Missions.Steps
{
    public class StepCheckLogic : SMissionStep
    {   // keeps track of a single tech's block count and outputs to the assigned Global1
        public override bool ForceUsesVarBool() => false;
        public override bool ForceUsesVarInt() => false;
        public override string GetTooltip() =>
            "Sets the Mission ProgressID on success";
        public override string GetDocumentation()
        {
            return
                "{  // " + GetTooltip() +
                  "\n  \"StepType\": \"CheckLogic\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  \"SuccessProgressID\": 0,      // The ProgressID the mission will be pushed to if " +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"True\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,        // An Index to check (-1 means always true)" +
                  "\n  \"SetMissionVarIndex2\": -1,        // An Index to check (-1 means always true)" +
                  "\n  \"SetMissionVarIndex3\": -1,        // An Index to check (-1 means always true)" +
                "\n},";
        }
        public override void InitGUI()
        {
            AddField(ESMSFields.VaribleType, "Condition Mode");
            AddField(ESMSFields.VaribleCheckNum, "Conditional Constant");
            AddField(ESMSFields.SetMissionVarIndex1, "Condition 1");
            AddField(ESMSFields.SetMissionVarIndex2, "Condition 2");
            AddField(ESMSFields.SetMissionVarIndex3, "Condition 3");
        }

        public override void OnInit() { }

        public override void FirstSetup()
        {
        }
        public override void Trigger()
        {
            try
            {
                if (SMUtil.BoolOut(ref SMission, SMission.SetMissionVarIndex1) && 
                    SMUtil.BoolOut(ref SMission, SMission.SetMissionVarIndex2) && 
                    SMUtil.BoolOut(ref SMission, SMission.SetMissionVarIndex3))
                {
                    SMUtil.ProceedID(ref SMission);
                }
            }
            catch (IndexOutOfRangeException e)
            {
                SMUtil.Assert(true, SMission.LogName, KickStart.ModID + ": Error in output [SetMissionVarIndex1] or [SetMissionVarIndex2] or " +
                    "[SetMissionVarIndex3] in mission " + Mission.Name +
                    " | Step type " + SMission.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) " +
                    "\n and make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry " +
                    "on the list, 1 counts as the second entry, and so on.", e);
            }
            catch (NullReferenceException e)
            {
                SMUtil.Assert(true, SMission.LogName, KickStart.ModID + ": Error in output [SetMissionVarIndex1] or [SetMissionVarIndex2] or " +
                    "[SetMissionVarIndex3] in mission " + Mission.Name +
                    " | Step type " + SMission.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) " +
                    "\n and make sure your referencing an entry you have declared in VarInts or varTrueFalse, depending" +
                    " on the step's set VaribleType.", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException(e);
            }
        }
        public override void OnDeInit()
        {
        }
    }
}
