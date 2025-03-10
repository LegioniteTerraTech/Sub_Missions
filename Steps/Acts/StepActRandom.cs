﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.Steps
{
    public class StepActRandom : SMissionStep
    {
        public override bool ForceUsesVarBool() => false;
        public override bool ForceUsesVarInt() => false;
        public override string GetTooltip() =>
            "Sets a specified VarInt to a random value";

        public override string GetDocumentation()
        {
            return
                "{  // " + GetTooltip() +
                  "\n  \"StepType\": \"ActRandom\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"True\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to if it's set to True or False." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that determines if it should execute." +
                  "\n  // Input Parameters" +
                  "\n  \"InputNum\": 0,    // Any other value than 0 will update with the appopreate ProgressID.  Leave at 0 to fire only once." +
                  "\n  \"InputString\": \"0\",   // The lower range" +
                  "\n  \"InputStringAux\": \"100\",      // The upper range" +
                "\n},";
        }
        public override void InitGUI()
        {
            AddField(ESMSFields.VaribleType, "Condition Mode");
            AddField(ESMSFields.VaribleCheckNum, "Conditional Constant");
            AddField(ESMSFields.SetMissionVarIndex1, "Active Condition");
            AddOptions(ESMSFields.InputNum, "Updating", new string[] {
                "Once",
                "While Active",
            });
            AddField(ESMSFields.InputString_float, "Min");
            AddField(ESMSFields.InputStringAux_float, "Max");
        }

        public override void OnInit() { }

        public override void OnDeInit()
        {
        }
        public override void FirstSetup()
        {
            if (SMission.InputNum == 0)
                Mission.VarIntsActive[SMission.SetMissionVarIndex1] = UnityEngine.Random.Range(int.Parse(SMission.InputString), int.Parse(SMission.InputStringAux));
        }
        public override void Trigger()
        {
            if (SMission.InputNum == 0)
                return;// disabled, only fires once
            switch (SMission.VaribleType)
            {
                case EVaribleType.True: // Random bool
                    Mission.VarTrueFalseActive[SMission.SetMissionVarIndex1] = UnityEngine.Random.Range(int.Parse(SMission.InputString), int.Parse(SMission.InputStringAux)) > SMission.VaribleCheckNum;
                    break;
                case EVaribleType.False:
                    Mission.VarTrueFalseActive[SMission.SetMissionVarIndex1] = UnityEngine.Random.Range(int.Parse(SMission.InputString), int.Parse(SMission.InputStringAux)) < SMission.VaribleCheckNum;
                    break;
                case EVaribleType.Int:
                    Mission.VarIntsActive[SMission.SetMissionVarIndex1] = UnityEngine.Random.Range(int.Parse(SMission.InputString), int.Parse(SMission.InputStringAux));
                    //Mission.VarInts[SMission.SetMissionVarIndex1] = UnityEngine.Random.Range((int)Mathf.Min(SMission.InputNum, 0), (int)Mathf.Max(0, SMission.InputNum));
                    break;
                default:
                    SMUtil.Error(true, SMission.LogName, 
                        KickStart.ModID + ": ActRandom's VaribleType must be set to either True, False, or Int.  Mission " + 
                        Mission.Name + ", Step " + Mission.EventList.IndexOf(SMission));
                    break;
            }
        }
    }
}
