using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;

namespace Sub_Missions.Steps
{
    public class StepCheckMoney : SMissionStep
    {   // keeps track of a single tech's block count and outputs to the assigned Global1
        public override bool ForceUsesVarBool() => SMission.InputString.Length != 4;
        public override bool ForceUsesVarInt() => SMission.InputString.Length == 4;
        public override string GetTooltip() =>
            "Updates a VarInt based on the player's available money";
        public override string GetDocumentation()
        {
            return
                "{  // " + GetTooltip() +
                  "\n  \"StepType\": \"CheckMoney\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  \"SuccessProgressID\": 0,      // The ProgressID the mission will be pushed to if " +
                  "\n  // Input Parameters" +
                  "\n  \"SetMissionVarIndex1\": -1,        // the index to apply the output from this" +
                  "\n  \"InputNum\": 300,        // Cost" +
                  "\n  \"InputString\": \"\",        // " +
                  "\n     \"Collect\" to charge the player on success." +
                  "\n     \"Send\" to send the . " +
                  "\n      Leave empty to only check" +
                "\n},";
        }

        public override void InitGUI()
        {
            AddField(ESMSFields.InputNum, "Cost");
            AddField(ESMSFields.SetMissionVarIndex1, "Output Condition");
            AddOptions(ESMSFields.InputString, "Mode", new string[] 
                { 
                    "Check",
                    "Send",
                    "Collect",
                }
             );
        }
        public override void OnInit() { }

        public override void FirstSetup()
        {
            SMission.InputNum = Mathf.RoundToInt(SMission.InputNum);

            if (SMission.InputString.Length == 4)
                SMission.VaribleType = EVaribleType.Int;
            else
                SMission.VaribleType = EVaribleType.True;
        }
        public override void Trigger()
        {
            try
            {
                if (SMission.InputString.Length == 4)
                {
                    Mission.VarIntsActive[SMission.SetMissionVarIndex1] = ManPlayer.inst.GetCurrentMoney();
                }
                else
                {
                    if (ManPlayer.inst.CanAfford((int)SMission.InputNum))
                    {
                        Mission.VarTrueFalseActive[SMission.SetMissionVarIndex1] = true;
                        if (SMission.InputString.Length == 7)
                            ManPlayer.inst.PayMoney((int)SMission.InputNum);
                    }
                    else
                    {
                        Mission.VarTrueFalseActive[SMission.SetMissionVarIndex1] = false;
                    }
                }
            }
            catch (IndexOutOfRangeException e)
            {
                SMUtil.Assert(true, SMission.LogName, KickStart.ModID + ": Error in output [SetMissionVarIndex1] in mission " + Mission.Name +
                    " | Step type " + SMission.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) " +
                    "\n and make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry " +
                    "on the list, 1 counts as the second entry, and so on.", e);
            }
            catch (NullReferenceException e)
            {
                SMUtil.Assert(true, SMission.LogName, KickStart.ModID + ": Error in output [SetMissionVarIndex1] in mission " + Mission.Name +
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
