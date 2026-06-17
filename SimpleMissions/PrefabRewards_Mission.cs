using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sub_Missions.Steps;
using UnityEngine;

namespace Sub_Missions.SimpleMissions
{
    internal static class PrefabRewards_Mission
    {
        public static SubMission MakeMission()
        {
            SubMission mission4 = new SubMission();
            mission4.Name = "GSO Blocks Kit";
            mission4.Description = "We happen to have some spare blocks laying around, " +
                "you can have them I guess...";
            mission4.GradeRequired = 0;
            mission4.MinProgressX = 0;
            mission4.Faction = "GSO";
            mission4.ClearTechsOnClear = true;
            mission4.SetStartingPosition = Vector3.forward * 32;
            mission4.SpawnPosition = SubMissionPosition.OffsetFromPlayer;
            mission4.VarTrueFalse = new List<bool>
            {
                //Simple mission using only ProgressIDs
            };
            mission4.VarInts = new List<int>
            {
                //Simple mission that only rewards once
            };
            mission4.CheckList = new List<MissionChecklist>
            {
                //Simple mission that only rewards once
            };
            mission4.EventList = new List<SubMissionStep>
            {
                // instant win
                new SubMissionStep
                {
                    StepType = SMStepType.ActWin,
                    ProgressID = 0,
                    VaribleType = EVaribleType.None,
                },
            };
            mission4.Rewards = new SubMissionReward
            {
                RandomBlocksToSpawn = 3,
                BlocksToSpawn = new List<string>
                 {
                    BlockTypes.GSOBlock_111.ToString(),
                    BlockTypes.GSOBlock_111.ToString(),
                    BlockTypes.GSOLightFixed_111.ToString(),
                    BlockTypes.GSOLaserFixed_111.ToString(),
                    BlockTypes.GSOWheelHub_111.ToString(),
                    BlockTypes.GSOWheelHub_111.ToString(),
                 }
            };
            return mission4;
        }
        public static SubMission MakeMission_OBSOLETE()
        {
            SubMission mission4 = new SubMission();
            mission4.Name = "Water Blocks Kit";
            mission4.Description = "If you are stuck in water, we can launch some blocks to help out.  But use these wisely, we won't be able to spare another.";
            mission4.GradeRequired = 0;
            mission4.MinProgressX = 0;
            mission4.Faction = "GSO";
            mission4.ClearTechsOnClear = true;
            mission4.SetStartingPosition = Vector3.forward * 32;
            mission4.SpawnPosition = SubMissionPosition.OffsetFromPlayer;
            mission4.VarTrueFalse = new List<bool>
            {
                //Simple mission using only ProgressIDs
            };
            mission4.VarInts = new List<int>
            {
                //Simple mission that only rewards once
            };
            mission4.CheckList = new List<MissionChecklist>
            {
                //Simple mission that only rewards once
            };
            mission4.EventList = new List<SubMissionStep>
            {
                // instant win
                new SubMissionStep
                {
                    StepType = SMStepType.ActWin,
                    ProgressID = 0,
                    VaribleType = EVaribleType.None,
                },
            };
            mission4.Rewards = new SubMissionReward
            {
                BlocksToSpawn = new List<string>
                 {
                     "GSO_Ebb",
                     "GSO_Ebb",
                     "GSO_One_Flote",
                     "GSO_One_Flote",
                     "GSO_One_Flote",
                     "GSO_One_Flote",
                 }
            };
            return mission4;
        }
    }
}
