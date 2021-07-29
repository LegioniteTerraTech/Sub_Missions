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
                                SMission.AssignedWaypoint = ManSpawn.inst.HostSpawnWaypoint(SMUtil.GetTrackedTech(ref Mission, SMission.InputString).boundsCentreWorldNoCheck, Quaternion.identity);
                                SMission.SavedInt = SMission.AssignedWaypoint.visible.ID;
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
                            SMission.AssignedWaypoint.visible.transform.position = SMUtil.GetTrackedTech(ref Mission, SMission.InputString).boundsCentreWorldNoCheck;
                        }
                        catch (Exception e)
                        {
                            SMUtil.Assert(false, "SubMissions: StepSetupWaypoint (Tech) - Failed: Could not upkeep waypoint!");
                            Debug.Log("SubMissions: Error - " + e);
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
                            SMission.AssignedWaypoint = vis.Waypoint;
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
