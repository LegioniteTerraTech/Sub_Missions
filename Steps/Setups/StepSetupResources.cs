using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;
using TerraTechETCUtil;

namespace Sub_Missions.Steps
{
    public class StepSetupResources : SMissionStep
    {
        public override bool ForceUsesVarBool() => false;
        public override bool ForceUsesVarInt() => false;
        public override string GetTooltip() =>
            "Spawns a Resource Node or Scenery Object";

        public override string GetDocumentation()
        {
            return
                "{  // " + GetTooltip() +
                  "\n  \"StepType\": \"SetupResources\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  \"Position\": {  // The position where this is handled relative to the Mission origin." +
                  "\n    \"x\": 0.0," +
                  "\n    \"y\": 2.0," +
                  "\n    \"z\": 0.0" +
                  "\n  }," +
                  "\n  \"TerrainHandling\": 2,      // " + TerrainHandlingDesc +
                  "\n  // Input Parameters" +
                  "\n  \"InputString\": \"Resource_Name\",   // The name of the Resource Node to spawn." +
                  "\n  \"InputNum\": 0,               // How many of the nodes to spawn" +
                "\n},";
        }
        public override void InitGUI()
        {
            AddField(ESMSFields.SpawnOnly, "On Mission Startup");
            AddField(ESMSFields.Position, "Position");
            AddField(ESMSFields.TerrainHandling, "Placement");
            //AddField(ESMSFields.InputNum_int, "Num to Spawn");
            AddOptions(ESMSFields.InputString, "Resource Type", Enum.GetNames(typeof(SceneryTypes)));
        }

        public override void OnInit() { }
        public override void OnDeInit() { }

        public override void FirstSetup()
        {   // Spawn a single resource
            if (Enum.TryParse(SMission.InputString, out SceneryTypes result) && SMission.InputNum >= 1)
            {
                ResourceDispenser RD = SpawnHelper.SpawnResourceNode(SMission.Position, Quaternion.identity, 
                    result, ManWorld.inst.GetBiomeWeightsAtScenePosition(SMission.Position).Biome(0).name);
                SMission.AssignedTracked = new TrackedVisible(RD.visible.ID, RD.visible, ObjectTypes.Scenery, RadarTypes.Hidden);
            }
            else
            {
                SMUtil.Error(false, SMission.LogName, 
                    KickStart.ModID + ": StepSetupResources - Failed: Input SceneryType not valid or " +
                    "InputNum below 1.  Mission " + Mission.Name);
            }
        }
        public override void Trigger()
        {
        }


    }
}
