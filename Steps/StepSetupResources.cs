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
        public override void TrySetup()
        {   // Spawn a single ModularMonument
            if (Enum.TryParse(SMission.InputString, out SceneryTypes result) && SMission.InputNum >= 1)
            {
                ItemTypeInfo info = new ItemTypeInfo(ObjectTypes.Scenery, (int)result);
                SMission.AssignedTracked = ManSpawn.inst.SpawnDispenser(SMission.Position, Quaternion.identity, info, (int)SMission.InputNum);
                //ManWorld.inst.LandmarkSpawner.SpawnLandmarks(ManWorld.inst.TileManager.LookupTile(SMission.Position))
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
