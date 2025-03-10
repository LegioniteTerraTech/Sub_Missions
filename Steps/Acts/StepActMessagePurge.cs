﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sub_Missions.ManWindows;

namespace Sub_Missions.Steps
{
    public class StepActMessagePurge : SMissionStep
    {
        public override bool ForceUsesVarBool() => false;
        public override bool ForceUsesVarInt() => false;
        public override string GetTooltip() =>
            "Clears ALL NPC dialect from the screen.";

        public override string GetDocumentation()
        {
            return
                "{  // " + GetTooltip() +
                  "\n  \"StepType\": \"ActMessagePurge\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"None\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that determines if it should execute." +
                  "\n  // Input Parameters" +
                  "\n  \"InputNum\": 0.0,             // Set to 1 to only purge once.\n   // Any other value will repeatedly purge messages if the Conditions are true." +
                "\n},";
        }
        public override void InitGUI()
        {
            AddField(ESMSFields.VaribleType, "Condition Mode");
            AddField(ESMSFields.VaribleCheckNum, "Conditional Constant");
            AddField(ESMSFields.SetMissionVarIndex1, "Active Condition");
            AddOptions(ESMSFields.InputNum, "Mode", new string[]
                {
                    "Always",
                    "Once",
                }
            );
        }
        public override void OnInit() { }

        public override void OnDeInit()
        {
        }
        public override void FirstSetup()
        {
            SMission.InputNum = 0;
        }
        public override void Trigger()
        {   // purge the text box things
            if (SMUtil.BoolOut(ref SMission))
            {   // PURGE
                if (SMission.InputNum == 1)
                {   // one means it triggers only once
                    if (SMission.SavedInt != -2)
                    {
                        Mission.PurgeAllActiveMessages();
                        SMission.SavedInt = -2;
                    }
                    return;
                }
                Mission.PurgeAllActiveMessages();
            }
        }
    }
}
