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
        public override string GetDocumentation()
        {
            return
                "{  // Updates a VarInt based on the player's available money" +
                  "\n  \"StepType\": \"CheckHealth\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  \"SuccessProgressID\": 0,      // The ProgressID the mission will be pushed to if " +
                  "\n  // Input Parameters" +
                  "\n  \"SetMissionVarIndex1\": -1,        // the index to apply the output from this" +
                  "\n  \"SetMissionVarIndex1\": -1,        // the index to apply the output from this" +
                "\n},";
        }

        public override void OnInit() { }

        public override void FirstSetup()
        {
            SMission.InputNum = Mathf.RoundToInt(SMission.InputNum);
        }
        public override void Trigger()
        {
            try
            {
                if (ManPlayer.inst.CanAfford((int)SMission.InputNum))
                {
                    Mission.VarTrueFalse[SMission.SetMissionVarIndex1] = true;
                }
                else
                {
                    Mission.VarTrueFalse[SMission.SetMissionVarIndex1] = false;
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
