﻿using System;
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
        public override string GetDocumentation()
        {
            return
                "{  // Pushes the Mission ProgressID forward on success" +
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

        public override void OnInit() { }

        public override void FirstSetup()
        {
        }
        public override void Trigger()
        {
            try
            {
                if (SMUtil.BoolOut(ref SMission, SMission.SetMissionVarIndex1) && SMUtil.BoolOut(ref SMission, SMission.SetMissionVarIndex2) && SMUtil.BoolOut(ref SMission, SMission.SetMissionVarIndex3))
                {
                    SMUtil.ProceedID(ref SMission);
                }
            }
            catch
            {
                SMUtil.Assert(true, "SubMissions: Error in output [SetMissionVarIndex1-3] in mission " + SMission.Mission.Name + " | Step type " + SMission.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) \nand make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry on the list, 1 counts as the second entry, and so on.");
            }
        }
        public override void OnDeInit()
        {
        }
    }
}