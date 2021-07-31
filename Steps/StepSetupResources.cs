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
            ItemTypeInfo info = new ItemTypeInfo(ObjectTypes.Scenery, (int)Enum.Parse(typeof(SceneryTypes), SMission.InputString));
            SMission.AssignedTracked = ManSpawn.inst.SpawnDispenser(SMission.Position, Quaternion.identity, info, 1);
            //ManWorld.inst.LandmarkSpawner.SpawnLandmarks(ManWorld.inst.TileManager.LookupTile(SMission.Position))

        }
        public override void Trigger()
        {
        }


        private void TryFetchTerrainObj()
        {
            if (SMission.InputString != "")
            {
                foreach (Visible vis in Singleton.Manager<ManVisible>.inst.VisiblesTouchingRadius(SMUtil.GetTrackedTech(ref Mission, SMission.InputString).boundsCentreWorldNoCheck, 32, new Bitfield<ObjectTypes>()))
                {
                    if ((bool)vis.Waypoint)
                    {
                        if (vis.ID == SMission.SavedInt)
                        {
                            SMission.AssignedWaypoint = vis.Waypoint;
                            SMission.AssignedTracked = ManVisible.inst.GetTrackedVisible(SMission.SavedInt);
                        }
                        return;
                    }
                }
                SMUtil.Assert(false, "SubMissions: StepSetupWaypoint (Tech) - Failed: Could not find waypoint!");
                return;
            }
            foreach (Visible vis in Singleton.Manager<ManVisible>.inst.VisiblesTouchingRadius(SMission.Position, 8, new Bitfield<ObjectTypes>()))
            {
                if ((bool)vis.Waypoint)
                {
                    if (vis.ID == SMission.SavedInt)
                        SMission.AssignedWaypoint = vis.Waypoint;
                    return;
                }
            }
            SMUtil.Assert(false, "SubMissions: StepSetupWaypoint - Failed: Could not find waypoint!");
        }
    }
}
