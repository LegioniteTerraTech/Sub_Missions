using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;

namespace Sub_Missions.Steps
{
    public class StepSetupResources : SMissionStep
    {
        public override string GetDocumentation()
        {
            return
                "{  // Spawns a Resource node / Scenery Object" +
                  "\n  \"StepType\": \"SetupResources\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  \"Position\": {  // The position where this is handled relative to the Mission origin." +
                  "\n    \"x\": 0.0," +
                  "\n    \"y\": 2.0," +
                  "\n    \"z\": 0.0" +
                  "\n  }," +
                  "\n  \"TerrainHandling\": 2,      // " + TerrainHandlingDesc +
                  "\n  // Input Parameters" +
                  "\n  \"InputString\": \"TechName\",   // The name of the Resource Node to spawn." +
                  "\n  \"InputNum\": 0,               // How many of the nodes to spawn" +
                "\n},";
        }

        public override void OnInit() { }
        public override void OnDeInit() { }

        public override void FirstSetup()
        {   // Spawn a single resource
            if (Enum.TryParse(SMission.InputString, out SceneryTypes result) && SMission.InputNum >= 1)
            {
                ItemTypeInfo info = new ItemTypeInfo(ObjectTypes.Scenery, (int)result);
                SMission.AssignedTracked = ManSpawn.inst.SpawnDispenser(SMission.Position, Quaternion.identity, info, (int)SMission.InputNum);
            }
            else
            {
                SMUtil.Assert(false, "SubMissions: StepSetupResources - Failed: Input SceneryType not valid or InputNum below 1.  Mission " + Mission.Name);
            }
        }
        public override void Trigger()
        {
        }


    }
}
