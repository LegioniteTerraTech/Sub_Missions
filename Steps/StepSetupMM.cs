using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;

namespace Sub_Missions.Steps
{
    public class StepSetupMM : SMissionStep
    {
        public override void TrySetup()
        {   // Spawn a single ModularMonument
            //SMUtil.SpawnTechTracked(ref Mission, SMission.Position, (int)SMission.InputNum, SMission.Forwards, SMission.InputString);
        }
        public override void Trigger()
        {   
        }
    }
}
