using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sub_Missions.ManWindows;
using TAC_AI.AI;
using TAC_AI;

namespace Sub_Missions.Steps
{
    public class StepActDrive : SMissionStep
    {
        public override bool ForceUsesVarBool() => false;
        public override bool ForceUsesVarInt() => false;

        public override string GetTooltip() =>  // (REQUIRES TACTICAL AI TO FUNCTION)
            "Tells a TrackedTech to DRIVE to a target";

        public override string GetDocumentation()
        {
            return
                "{  // " + GetTooltip() +
                  "\n  \"StepType\": \"StepActDrive\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"None\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that determines if it should execute." +
                  "\n  // Input Parameters" +
                  "\n  \"InputString\": \"TechName\",   // The name of the TrackedTech to command." +
                  "\n  \"InputStringAux\": \"TechName\",// The name of the TrackedTech for the Waypoint to follow.  " +
                  "\n    Leave empty to leave stationary" +
                "\n},";
        }
        public override void InitGUI()
        {
            AddField(ESMSFields.Position, "Position");
            AddField(ESMSFields.TerrainHandling, "Placement");
            AddField(ESMSFields.VaribleType, "Condition Mode");
            AddField(ESMSFields.VaribleCheckNum, "Conditional Constant");
            AddGlobal(ESMSFields.SetMissionVarIndex1, "Active Condition", EVaribleType.Int);
            AddField(ESMSFields.InputString_Tracked_Tech, "Tracked Tech");
            AddOptions(ESMSFields.InputStringAux, "Target: ", new string[]
                {
                    "Position",
                    "Tech",
                },
                new Dictionary<int, KeyValuePair<string, ESMSFields>>()
                {
                    {1, new KeyValuePair<string, ESMSFields>("Tech Name", ESMSFields.InputStringAux_Tech) },
                }
            );
        }

        public override void OnInit()
        {
            SetDest = SMission.Position;
        }

        public override void OnDeInit()
        {
        }
        public override void FirstSetup()
        {
        }
        public Tank SetTech = null;
        public Vector3 SetDest = Vector3.zero;
        public void TechControlUpdate(TankControl.ControlState stateU)
        {
            if (SetTech)
            {
                SetTech.CommandMove(SetDest);
            }
        }
        public void TechDetachControlUpdate(Tank tank)
        {
            tank.GetComponent<TankAIHelper>().RTSControlled = false;
            tank.control.driveControlEvent.Unsubscribe(TechControlUpdate);
            tank.TankRecycledEvent.Unsubscribe(TechDetachControlUpdate);
            SetTech = null;
        }
        public override void Trigger()
        {   // 
            if (ManNetwork.IsHost)
            {
                if (SMUtil.BoolOut(ref SMission))
                {
                    if (SetTech == null && SMUtil.GetTrackedTech(ref SMission, SMission.InputString, out Tank target))
                    {
                        try
                        {
                            SetTech = target;
                            target.GetComponent<TankAIHelper>().RTSControlled = true;
                            target.control.driveControlEvent.Subscribe(TechControlUpdate);
                            target.TankRecycledEvent.Subscribe(TechDetachControlUpdate);
                        }
                        catch
                        {
                            SMUtil.Log(true, KickStart.ModID + ": Could not control Tech as this action requires TACtical AIs to execute correctly!");
                        }
                    }
                    if (!SMission.InputStringAux.NullOrEmpty() && SMission.InputStringAux != "Position")
                    {   // Keep updating the tech target location to follow the target
                        try
                        {
                            Tank tech = SMUtil.GetTrackedTech(ref Mission, SMission.InputStringAux);
                            SetDest = tech.boundsCentreWorldNoCheck + (Vector3.up * tech.blockBounds.extents.y);
                        }
                        catch //(Exception e)
                        {   // the tech isn't ready yet
                            //SMUtil.Assert(false, KickStart.ModID + ": StepSetupWaypoint (Tech) - Failed: Could not upkeep waypoint!");
                            //Debug_SMissions.Log(KickStart.ModID + ": Error - " + e);
                        }
                    }
                }
                else
                {
                    if (SetTech != null && SMUtil.GetTrackedTech(ref SMission, SMission.InputString, out Tank target))
                    {
                        TechDetachControlUpdate(SetTech);
                    }
                }
            }
        }
    }
}
