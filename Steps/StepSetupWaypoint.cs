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
        public override void TrySetup()
        {  
        }
        public override void Trigger()
        {
            if (SMUtil.BoolOut(ref SMission))
            {   // Spawn waypoint
                if (SMission.AssignedWaypoint == null)
                {
                    try
                    {
                        if (SMission.InputString != "")
                        {
                            try
                            {
                                //UIBouncingArrow.BouncingArrowContext arrow = default;
                                //arrow.targetTransform = SMUtil.GetTrackedTech(ref Mission, SMission.InputString).CentralBlock.trans;
                                SMission.AssignedWaypoint = ManSpawn.inst.HostSpawnWaypoint(SMission.Position, Quaternion.identity);
                                SMission.SavedInt = SMission.AssignedWaypoint.visible.ID;
                                SMission.AssignedTracked = new TrackedVisible(SMission.SavedInt, SMission.AssignedWaypoint.visible, ObjectTypes.Waypoint, RadarTypes.AreaQuest);
                                ManOverlay.inst.AddWaypointOverlay(SMission.AssignedTracked);
                                Debug.Log("SubMissions: StepSetupWaypoint (Tech) - Attached Waypoint to Tech " + SMission.InputString);
                            }
                            catch (Exception e)
                            {
                                SMUtil.Assert(false, "SubMissions: StepSetupWaypoint (Tech) - Failed: Could not setup waypoint!");
                                Debug.Log("SubMissions: Error - " + e);
                            }
                        }
                        else
                        {
                            SMission.AssignedWaypoint = ManSpawn.inst.HostSpawnWaypoint(SMission.Position, Quaternion.identity);
                            SMission.SavedInt = SMission.AssignedWaypoint.visible.ID;
                            SMission.AssignedTracked = new TrackedVisible(SMission.SavedInt, SMission.AssignedWaypoint.visible, ObjectTypes.Waypoint, RadarTypes.AreaQuest);
                            ManOverlay.inst.AddWaypointOverlay(SMission.AssignedTracked);
                            Debug.Log("SubMissions: StepSetupWaypoint - Placed Waypoint at " + SMission.Position);
                        }
                    }
                    catch (Exception e)
                    {
                        SMUtil.Assert(false, "SubMissions: StepSetupWaypoint - Failed: Could not setup waypoint!");
                        Debug.Log("SubMissions: Error - " + e);
                    }
                }
                else
                {
                    if (SMission.InputString != "")
                    {   // Keep updating the waypoint to follow the target
                        try
                        {
                            Tank tech = SMUtil.GetTrackedTech(ref Mission, SMission.InputString);
                            SMission.AssignedWaypoint.visible.transform.position = tech.boundsCentreWorldNoCheck + (Vector3.up * tech.blockBounds.extents.y);
                        }
                        catch //(Exception e)
                        {   // the tech isn't ready yet
                            //SMUtil.Assert(false, "SubMissions: StepSetupWaypoint (Tech) - Failed: Could not upkeep waypoint!");
                            //Debug.Log("SubMissions: Error - " + e);
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
                    try
                    {
                        ManOverlay.inst.RemoveWaypointOverlay(SMission.AssignedTracked);
                        SMission.AssignedWaypoint.visible.RemoveFromGame();
                    }
                    catch (Exception e)
                    {
                        SMUtil.Assert(false, "SubMissions: StepSetupWaypoint - Failed: Could not despawn waypoint!");
                        Debug.Log("SubMissions: Error - " + e);
                    }
                }
            }
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
