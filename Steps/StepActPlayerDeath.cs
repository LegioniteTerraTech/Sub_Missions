using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AnimeAI.SubMissions
{
    public class StepActPlayerDeath : SMissionStep
    {
        public override void TrySetup()
        {
        }
        public override void Trigger()
        {
            if (SMTracker.IsPlayerDestroyed())
                SMission.mission.CurrentProgressID = SMission.SuccessProgressID;
        }
    }
}
