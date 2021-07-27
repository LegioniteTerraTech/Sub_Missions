using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.Steps
{
    public class StepCheckResources : SMissionStep
    {
        public override void TrySetup()
        {
            try
            {
                if (SMission.SavedInt == 0)
                    SMission.SavedInt = Singleton.Manager<ManStats>.inst.GetNumResourcesHarvested((ChunkTypes)Mission.VarInts[SMission.SetGlobalIndex1]);
                Debug.Log("SubMissions: StepCheckResources - ChunkType assigned is " + ((ChunkTypes)Mission.VarInts[SMission.SetGlobalIndex1]).ToString());
                Debug.Log("SubMissions: StepCheckResources - current count is " + Singleton.Manager<ManStats>.inst.GetNumResourcesHarvested((ChunkTypes)Mission.VarInts[SMission.SetGlobalIndex1]) + ".");
            }
            catch
            {   // it's being triggered not in campaign - testing
                Debug.Log("SubMissions: Called StepCheckResources without ManStats present!");
                SMission.SavedInt = 0;
            }
        }
        public override void Trigger()
        {   // check player mined chunks
            try
            {
                Mission.VarInts[SMission.SetGlobalIndex2] = Singleton.Manager<ManStats>.inst.GetNumResourcesHarvested((ChunkTypes)Mission.VarInts[SMission.SetGlobalIndex1]) - SMission.SavedInt;
                if (SMission.InputNum <= Mission.VarInts[SMission.SetGlobalIndex2])
                    SMUtil.ConcludeGlobal3(ref SMission);
            }
            catch
            {   // it's being triggered wrong - testing
                SMission.SavedInt++;
                Mission.VarInts[SMission.SetGlobalIndex2] = SMission.SavedInt;
                if (SMission.InputNum <= Mission.VarInts[SMission.SetGlobalIndex2])
                    SMUtil.ConcludeGlobal3(ref SMission);
            }
        }
    }
}
