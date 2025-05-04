using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sub_Missions.Steps;

namespace Sub_Missions.SimpleMissions
{
    internal static class PrefabHarvest_Mission
    {
        public static SubMission MakeMission()
        {
            SubMission mission3 = new SubMission();
            mission3.Name = "Harvest Mission";
            mission3.Description = "A showcase mission with harvesting involved";
            mission3.GradeRequired = 1;
            mission3.MinProgressX = 1;
            mission3.Faction = "GSO";
            mission3.IgnorePlayerProximity = true;
            mission3.ClearTechsOnClear = true;
            mission3.VarTrueFalse = new List<bool>
            {
                false,
                false,
                //Simple mission using only ProgressIDs
            };
            mission3.VarInts = new List<int>
            {
                // F*bron
                0,  // count of chunks
                30, // aimed count
                // Plumbia
                0,  // count of chunks
                30, // aimed count
            };
            mission3.CheckList = new List<MissionChecklist>
            {
                new MissionChecklist
                {
                    ListArticle = "Mine F*bron",
                    ValueType = VarType.IntOverInt,
                    GlobalIndex = 0,    // numberator
                    GlobalIndex2 = 1,    // denominator
                },
                new MissionChecklist
                {
                    ListArticle = "Mine Plumbia",
                    ValueType = VarType.IntOverInt,
                    GlobalIndex = 2,    // numberator
                    GlobalIndex2 = 3,    // denominator
                }
            };
            mission3.EventList = new List<SubMissionStep>
            {
                // Phase 1 - Getting the player to mine the resources
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 0,
                    VaribleType = EVaribleType.None,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Hey there prospector, so uhh we're testing the new sub-missions protocol. \n Feller, could you chop down some trees?",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.CheckResources,
                    ProgressID = 0,
                    SuccessProgressID = 0, //
                    VaribleType = EVaribleType.True,    //Sets output
                    InputNum = (int)ChunkTypes.Wood, // the chunktype to harvest
                    SetMissionVarIndex1 = 0,    // resource count mined
                    SetMissionVarIndex2 = 1,    // the amount of resources needed
                    SetMissionVarIndex3 = 0,    // output index to VariableType
                },
                new SubMissionStep
                {
                    StepType = SMStepType.CheckResources,
                    ProgressID = 0,
                    SuccessProgressID = 0, //
                    VaribleType = EVaribleType.True, //Sets output
                    InputNum = (int)ChunkTypes.PlumbiteOre, // the chunktype to harvest
                    SetMissionVarIndex1 = 2,    // resource count mined
                    SetMissionVarIndex2 = 3,    // the amount of resources needed
                    SetMissionVarIndex3 = 1,    // output index to VariableType
                },
                new SubMissionStep
                {
                    StepType = SMStepType.CheckLogic,   // Progresses to SuccessProgressID when all GlobalIndexes are satisifed
                    ProgressID = 0,
                    SuccessProgressID = 30, //
                    VaribleType = EVaribleType.True,// What to check ALL specified GlobalIndexes for
                    SetMissionVarIndex1 = 0,    // f*brons mined all
                    SetMissionVarIndex2 = 1,    // plumbia mined all
                    SetMissionVarIndex3 = -1,   // unused bool
                },
                
                // Phase 2 - Letting the NPC leave the scene
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 30,
                    SuccessProgressID = 35, //
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Alright, that should do it.            \n  You can keep the chunks.    \n    No it's fine.",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 35,
                    SuccessProgressID = 45, //
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Farewell comrade, may we mee-~   \n          \n       <b>screEEEEeeeech!</b>",
                },

                // Phase 2 - Cleanup
                new SubMissionStep
                {
                    StepType = SMStepType.ActWin,
                    ProgressID = 45,
                    VaribleType = EVaribleType.None,
                },
            };
            mission3.Rewards = new SubMissionReward
            {
                MoneyGain = 500,
                EXPGain = 100,
            };
            return mission3;
        }
    }
}
