using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sub_Missions.ManWindows;
using TAC_AI.AI;

namespace Sub_Missions.Steps
{
    public class StepActBoost : SMissionStep
    {
        public override bool ForceUsesVarBool() => false;
        public override bool ForceUsesVarInt() => false;
        public override string GetTooltip() =>
            "Tells a TrackedTech to fire it's own boosters";

        public override string GetDocumentation()
        {
            return
                "{  // " + GetTooltip() +
                  "\n  \"StepType\": \"ActBoost\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"None\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that determines if it should execute." +
                  "\n  // Input Parameters" +
                  "\n  \"InputString\": \"TechName\",   // The name of the TrackedTech to command." +
                "\n},";
        }
        public override void InitGUI()
        {
            AddField(ESMSFields.VaribleType, "Condition Mode");
            AddField(ESMSFields.VaribleCheckNum, "Conditional Constant");
            AddGlobal(ESMSFields.SetMissionVarIndex1, "Active Condition", EVaribleType.Int);
            AddField(ESMSFields.InputString_Tracked_Tech, "Tracked Tech");
        }

        public override void OnInit() { }

        public override void OnDeInit()
        {
        }
        public override void FirstSetup()
        {
        }
        public Tank SetTech = null;
        public void TechControlUpdate(TankControl.ControlState stateU)
        {
            if (SetTech)
            {
                var state = SetTech.control.CurState;
                state.m_BoostJets = true;
                SetTech.control.CurState = state;
            }
        }
        public void TechDetachControlUpdate(Tank tank)
        {
            tank.GetComponent<TankAIHelper>().AIControlOverride = TankAIHelper.OverrideAndFlyAway;
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
                            target.GetComponent<TankAIHelper>().AIControlOverride = TankAIHelper.OverrideAndFlyAway;
                            target.control.driveControlEvent.Subscribe(TechControlUpdate);
                            target.TankRecycledEvent.Subscribe(TechDetachControlUpdate);
                        }
                        catch
                        {
                            SMUtil.Log(true, KickStart.ModID + ": Could not fly away Tech as this action requires TACtical AIs to execute correctly!");
                        }
                    }
                }
                else
                {
                    if (SetTech != null && SMUtil.GetTrackedTech(ref SMission, SMission.InputString, out Tank target))
                        TechDetachControlUpdate(SetTech);
                }
            }
        }
    }
}
