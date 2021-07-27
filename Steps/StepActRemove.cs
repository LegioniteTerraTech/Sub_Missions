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
        public override void TrySetup()
        {
        }
        public override void Trigger()
        {   // 
            if (SMUtil.BoolOut(ref SMission))
            {
                Tank target = SMUtil.GetTrackedTech(ref SMission, SMission.InputString);
                if (SMission.InputNum < 0.5f)
                { // zero = just despawn
                    target.visible.RemoveFromGame();
                }
                else if (SMission.InputNum > 1.5f)
                { // two = disintegrate to smithereens
                    foreach (TankBlock block in target.blockman.IterateBlocks())
                    {
                        block.visible.SetInteractionTimeout(0.5f);
                        block.damage.SelfDestruct(0.1f);
                    }

                    target.blockman.Disintegrate(true);
                }
                else 
                { // one = detach all
                    target.blockman.Disintegrate(true);
                }
            }
        }
    }
}
