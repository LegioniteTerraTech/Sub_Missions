using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;

namespace Sub_Missions.Steps
{
    public class StepSetupWaypoint : SMissionStep
    {
        public override string GetDocumentation()
        {
            return
                "{  // Sets up a Waypoint for the Mission." +
                  "\n  \"StepType\": \"SetupWaypoint\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"True\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that determines if it should be shown." +
                  "\n  // Input Parameters" +
                  "\n  \"InputString\": \"TechName\",   // The name of the TrackedTech for the Waypoint to follow.  Leave empty to leave stationary" +
                "\n},";
        }
        public override void OnInit() { }

        public override void OnDeInit()
        {
        }
        public override void FirstSetup()
        {  
        }
        public override void Trigger()
        {
            if (ManNetwork.IsHost)
            {
                if (SMUtil.BoolOut(ref SMission))
                {   // Spawn waypoint
                    if (SMission.AssignedWaypoint == null)
                    {
                        SpawnWaypoint();
                    }
                    else
                    {
                        if (!SMission.InputString.NullOrEmpty())
                        {   // Keep updating the waypoint to follow the target
                            try
                            {
                                Tank tech = SMUtil.GetTrackedTech(ref Mission, SMission.InputString);
                                SMission.AssignedWaypoint.visible.transform.position = tech.boundsCentreWorldNoCheck + (Vector3.up * tech.blockBounds.extents.y);
                            }
                            catch //(Exception e)
                            {   // the tech isn't ready yet
                                //SMUtil.Assert(false, "SubMissions: StepSetupWaypoint (Tech) - Failed: Could not upkeep waypoint!");
                                //Debug_SMissions.Log("SubMissions: Error - " + e);
                            }
                        }
                    }
                }
                else
                {   // we eradicate the waypoint
                    if (SMission.AssignedWaypoint == null)
                    {
                        TryFetchWaypoint();
                    }
                    else
                    {
                        DespawnWaypoint();
                    }
                }
            }
        }
        private void TryFetchOrSpawnWaypoint()
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
                if (!SpawnWaypoint())
                    SMUtil.Assert(false, "SubMissions: StepSetupWaypoint (Tech) - Failed: Could not find/spawn waypoint!");
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
            if (!SpawnWaypoint())
                SMUtil.Assert(false, "SubMissions: StepSetupWaypoint - Failed: Could not find/spawn waypoint!");
        }
        private void TryFetchWaypoint()
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
                            return;
                        }
                    }
                }
                return;
            }
            foreach (Visible vis in Singleton.Manager<ManVisible>.inst.VisiblesTouchingRadius(SMission.Position, 8, new Bitfield<ObjectTypes>()))
            {
                if ((bool)vis.Waypoint)
                {
                    if (vis.ID == SMission.SavedInt)
                    {
                        SMission.AssignedWaypoint = vis.Waypoint;
                        return;
                    }
                }
            }
            SMUtil.Assert(false, "SubMissions: StepSetupWaypoint - Failed: Could not find/spawn waypoint!");
        }
        private bool SpawnWaypoint()
        {
            try
            {
                if (SMission.InputString != "")
                {
                    try
                    {
                        //UIBouncingArrow.BouncingArrowContext arrow = default;
                        //arrow.targetTransform = SMUtil.GetTrackedTech(ref Mission, SMission.InputString).CentralBlock.trans;
                        CreateNewWaypoint();
                        Debug_SMissions.Log("SubMissions: StepSetupWaypoint (Tech) - Attached Waypoint to Tech " + SMission.InputString);
                        return true;
                    }
                    catch (Exception e)
                    {
                        SMUtil.Assert(false, "SubMissions: StepSetupWaypoint (Tech) - Failed: Could not setup waypoint!");
                        Debug_SMissions.Log("SubMissions: Error - " + e);
                    }
                }
                else
                {
                    CreateNewWaypoint();
                    Debug_SMissions.Log("SubMissions: StepSetupWaypoint - Placed Waypoint at " + SMission.Position);
                    return true;
                }
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "SubMissions: StepSetupWaypoint - Failed: Could not setup waypoint!");
                Debug_SMissions.Log("SubMissions: Error - " + e);
            }
            return false;
        }
        private void CreateNewWaypoint()
        {
            SMission.AssignedWaypoint = ManSpawn.inst.HostSpawnWaypoint(SMission.Position, Quaternion.identity);
            SMission.SavedInt = SMission.AssignedWaypoint.visible.ID;
            SMission.AssignedTracked = new TrackedVisible(SMission.SavedInt, SMission.AssignedWaypoint.visible, ObjectTypes.Waypoint, RadarTypes.AreaQuest);
            ManOverlay.inst.AddWaypointOverlay(SMission.AssignedTracked);
        }
        private bool DespawnWaypoint()
        {
            try
            {
                if (SMission.AssignedWaypoint)
                {
                    ManOverlay.inst.RemoveWaypointOverlay(SMission.AssignedTracked);
                    SMission.AssignedWaypoint.visible.RemoveFromGame();
                }
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "SubMissions: StepSetupWaypoint - Failed: Could not despawn waypoint!");
                Debug_SMissions.Log("SubMissions: Error - " + e);
            }
            return false;
        }
    }
}
