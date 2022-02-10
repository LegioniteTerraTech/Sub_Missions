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
        public override string GetDocumentation()
        {
            return
                "{  // Keeps Track of Resource/Chunk Mining/Creation" +
                  "\n  \"StepType\": \"CheckResources\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  \"SuccessProgressID\": 0,      // The ProgressID the mission will be pushed to if \"VaribleType\" is set to \"DoSuccessID\"" +
                  "\n  \"TerrainHandling\": 2,      // " + TerrainHandlingDesc + 
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"True\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that keeps track of how many resources were harvested." +
                  "\n  \"SetMissionVarIndex2\": -1,       // The index that is set to how many resources to harvest." +
                  "\n  \"SetMissionVarIndex3\": -1,       // The index that is altered on completion." +
                  "\n  // Input Parameters" +
                  "\n  \"InputNum\": 0,             // The ChunkType to use (in number format)" +
                "\n},";
        }
        public override void OnInit() { }

        public override void FirstSetup()
        {
            try
            {
                if (SMission.SavedInt == 0)
                    SMission.SavedInt = Singleton.Manager<ManStats>.inst.GetNumResourcesHarvested((ChunkTypes)SMission.InputNum);
                Debug.Log("SubMissions: StepCheckResources - ChunkType assigned is " + ((ChunkTypes)SMission.InputNum).ToString());
                Debug.Log("SubMissions: StepCheckResources - current count is " + Singleton.Manager<ManStats>.inst.GetNumResourcesHarvested((ChunkTypes)SMission.InputNum) + ".");
            }
            catch
            {   // not assigned correctly
                SMUtil.Assert(true, "SubMissions: Error in input (ChunkTypes) InputNum in mission " + SMission.Mission.Name + " | Step type " + SMission.StepType.ToString());
                SMission.SavedInt = 0;
            }
        }
        public override void Trigger()
        {   // check player mined chunks
            try
            {
                Mission.VarInts[SMission.SetMissionVarIndex1] = Singleton.Manager<ManStats>.inst.GetNumResourcesHarvested((ChunkTypes)SMission.InputNum) - SMission.SavedInt;
                if (Mission.VarInts[SMission.SetMissionVarIndex2] <= Mission.VarInts[SMission.SetMissionVarIndex1])
                    SMUtil.ConcludeGlobal3(ref SMission);
            }
            catch
            {   // it's being triggered wrong - testing
                SMission.SavedInt++;
                try
                {
                    Mission.VarInts[SMission.SetMissionVarIndex1] = SMission.SavedInt;
                    if (Mission.VarInts[SMission.SetMissionVarIndex2] <= Mission.VarInts[SMission.SetMissionVarIndex1])
                        SMUtil.ConcludeGlobal3(ref SMission);
                }
                catch
                {
                    SMUtil.Assert(true, "SubMissions: Error in output [SetMissionVarIndex1] or [SetMissionVarIndex2] in mission " + SMission.Mission.Name + " | Step type " + SMission.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) \nand make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry on the list, 1 counts as the second entry, and so on.");
                }
            }
        }
        public override void OnDeInit()
        {
        }
    }
}
