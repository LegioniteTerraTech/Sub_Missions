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
    {   // Builds a randomly-Generated tileable ModularMonument out of given models and meshes
        public override void TrySetup()
        {   // Spawn a ModularMonument
            //SMUtil.SpawnTechTracked(ref Mission, SMission.Position, (int)SMission.InputNum, SMission.Forwards, SMission.InputString);
        }
        public override void Trigger()
        {   
        }
    }
}
