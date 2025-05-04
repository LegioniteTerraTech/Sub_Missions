using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sub_Missions.ManWindows;

namespace Sub_Missions.Steps
{
    public class StepActRemove : SMissionStep
    {
        public override bool ForceUsesVarBool() => false;
        public override bool ForceUsesVarInt() => false;
        public override string GetTooltip() =>
            "Destroys existing TrackedTechs";
        public override string GetDocumentation()
        {
            return
                "{  // " + GetTooltip() +
                  "\n  \"StepType\": \"ActRemove\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"True\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that determines if it should execute." +
                  "\n  // Input Parameters" +
                  "\n  \"InputNum\": 0.0,             // Greater than 2 to explode, 0 to remove immedeately" +
                  "\n  \"InputString\": \"TechName\",   // The name of the TrackedTech to remove." +
                "\n},";
        }
        public override void InitGUI()
        {
            AddField(ESMSFields.VaribleType, "Condition Mode");
            AddField(ESMSFields.VaribleCheckNum, "Conditional Constant");
            AddField(ESMSFields.SetMissionVarIndex1, "Condition");
            AddField(ESMSFields.InputString_Tracked_Tech, "Tracked Tech");
            AddOptions(ESMSFields.InputNum, "Mode", new string[] { 
                "Remove",
                "Dismantle",
                "Destroy",
            });
        }

        public override void OnInit() { }

        public override void OnDeInit()
        {
        }
        public override void FirstSetup()
        {
        }
        public override void Trigger()
        {   // 
            if (SMUtil.BoolOut(ref SMission))
            {
                if (SMUtil.GetTrackedTech(ref SMission, SMission.InputString, out Tank target))
                {
                    if (SMission.InputNum < 0.5f)
                    { // zero = just despawn
                        target.visible.RemoveFromGame();
                    }
                    else if (SMission.InputNum > 1.5f)
                    { // two = disintegrate to smithereens
                        foreach (TankBlock block in target.blockman.IterateBlocks())
                        {
                            block.visible.SetLockTimout(Visible.LockTimerTypes.Interactible, 0.5f);
                            block.damage.SelfDestruct(0.1f);
                        }

                        target.blockman.Disintegrate(true);
                    }
                    else
                    { // one = detach all
                        target.blockman.Disintegrate(true);
                    }
                }
                // Else we are waiting of that enemy to spawn back into the world
            }
        }
    }
}
