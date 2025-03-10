﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;

namespace Sub_Missions.Steps
{
    public class StepSetupMM : SMissionStep
    {   // Builds a tileable ModularMonument out of given models and meshes
        public override bool ForceUsesVarBool() => true;
        public override bool ForceUsesVarInt() => false;
        public override string GetTooltip() =>
            "Builds a tileable ModularMonument out of given models";
        public override string GetDocumentation()
        {
            return
                "{  // " + GetTooltip() + 
                  "\n  \"StepType\": \"SetupMM\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  \"Position\": {  // The position where this is handled relative to the Mission origin." +
                  "\n    \"x\": 0.0," +
                  "\n    \"y\": 2.0," +
                  "\n    \"z\": 0.0" +
                  "\n  }," +
                  "\n  \"EulerAngles\": {  // The rotation in EulerAngles that the ModularMonument should be oriented." +
                  "\n    \"x\": 0.0," +
                  "\n    \"y\": 0.0," +
                  "\n    \"z\": 0.0" +
                  "\n  }," +
                  "\n  \"Forwards\": {  // The scale of the ModularMonument." +
                  "\n    \"x\": 1.0," +
                  "\n    \"y\": 1.0," +
                  "\n    \"z\": 1.0" +
                  "\n  }," +
                  "\n  \"TerrainHandling\": 2,      // " + TerrainHandlingDesc + 
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"True\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that determines if the ModularMonument should be physically present." +
                  "\n  // Input Parameters" +
                  "\n  \"InputString\": \"MM_Name\",   // The name of the ModularMonument to spawn." +
                "\n},";
        }
        public override void InitGUI()
        {
            AddField(ESMSFields.Position, "Position");
            AddField(ESMSFields.Forwards, "Forwards");
            AddField(ESMSFields.EulerAngles, "Euler Angles");
            AddField(ESMSFields.TerrainHandling, "Placement");
            AddField(ESMSFields.VaribleType, "Condition Mode");
            AddField(ESMSFields.VaribleCheckNum, "Conditional Constant");
            AddField(ESMSFields.SetMissionVarIndex1, "Active Condition");
            AddField(ESMSFields.InputString_MM, "Monument");
        }
        public override void OnInit()
        { // Spawn a ModularMonument
            if (ManModularMonuments.SpawnMM(Mission.Tree, SMission.InputString, SMission.Position, SMission.EulerAngles, SMission.Forwards, out GameObject GO))
            {
                Debug_SMissions.Log(KickStart.ModID + ": SpawnMM - " + SMission.InputString + " for mission " + Mission.Name);
                SMission.Mission.TrackedMonuments.Add(GO.GetComponent<SMWorldObject>());
            }
        }

        public override void FirstSetup()
        {  
        }
        public override void Trigger()
        {
        }
        public override void OnDeInit()
        {
        }
    }
}
