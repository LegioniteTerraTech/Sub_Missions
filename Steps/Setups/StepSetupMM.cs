using System;
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
        public override string GetDocumentation()
        {
            return
                "{  // Builds a tileable ModularMonument out of given models" +
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
                  "\n  \"TerrainHandling\": 2,      // " + TerrainHandlingDesc + 
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"True\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that determines if the ModularMonument should be physically present." +
                  "\n  // Input Parameters" +
                  "\n  \"InputString\": \"TechName\",   // The name of the ModularMonument to spawn." +
                "\n},";
        }
        public override void OnInit()
        { // Spawn a ModularMonument
            if (ManModularMonuments.SpawnMM(SMission.InputString, SMission.Position, SMission.EulerAngles, SMission.Forwards, out GameObject GO))
            {
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
