using Newtonsoft.Json;
using Sub_Missions.Steps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using TerraTechETCUtil;

namespace Sub_Missions
{
    public class SimpleSMissions
    {
        public static string DLLDirectory => SMissionJSONLoader.DLLDirectory;
        public static string BaseDirectory => SMissionJSONLoader.BaseDirectory;
        public static string MissionsDirectory => SMissionJSONLoader.MissionsDirectory;
        public static string MissionSavesDirectory => SMissionJSONLoader.MissionSavesDirectory;
        public static string MissionCorpsDirectory => SMissionJSONLoader.MissionCorpsDirectory;

        private static JsonSerializerSettings JSONSaver = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };
        private static JsonSerializerSettings JSONSaverMission = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Converters = new List<JsonConverter> { new MissionTypeEnumConverter() },
        };
        private static JsonSerializerSettings JSONSafe = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 12,
        };


        public static SubMission MakePrefabNPC_Mission()
        {
            SubMission mission1 = new SubMission();
            mission1.Name = "NPC Mission";
            mission1.Description = "A complex showcase mission with an NPC involved";
            mission1.GradeRequired = 1;
            mission1.Faction = "GSO";
            mission1.SetStartingPosition = Vector3.forward * 250;
            mission1.Type = SubMissionType.Critical;
            mission1.CannotCancel = true;
            mission1.SpawnPosition = SubMissionPosition.OffsetFromPlayerTechFacing;
            mission1.ClearTechsOnClear = false; // off for now to show the StepActRemove Step
            mission1.VarTrueFalseActive = new List<bool>
            {
                false,  // Range check
                false,  // Choice 1 "No"
                false,  // Choice 2 "Yes"
                false,  // When Player drives away while making the choice
            };
            mission1.VarIntsActive = new List<int>
            {
                0,  // timer value
            };
            mission1.CheckList = new List<MissionChecklist>
            {
                new MissionChecklist
                {
                    ListArticle = "Meet Garrett",
                    ValueType = VarType.Unset,
                    BoolToEnable = 3,
                },
                new MissionChecklist
                {
                    ListArticle = "Speak to Garrett",
                    ValueType = VarType.Unset,
                },
            };
            mission1.EventList = new List<SubMissionStep>
            {
                // Phase 1 - Getting the player to the character
                new SubMissionStep
                {
                    StepType = SMStepType.SetupTech,
                    ProgressID = 0,
                    InitPosition = new Vector3(2,0,6), // needs specific location
                    InputNum = -2, // team
                    VaribleType = EVaribleType.None,
                    InputString = "Garrett Gruntle.RAWTECH",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.SetupWaypoint,
                    ProgressID = SubMission.alwaysRunValue,// update For all time. Always.
                    InitPosition = new Vector3(2,0,6),
                    VaribleType = EVaribleType.None,
                    InputString = "Garrett Gruntle.RAWTECH", // give it a TrackedTech Tech name to assign it to that tech
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 0,
                    SuccessProgressID = 2,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Hello?   <b>Is this thing working!?</b> \n            <b>screEEEEeeeech!</b>\n Henlo there, and thank you for signing up into our Sub-Missions programme!",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 2,
                    VaribleType = EVaribleType.None,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "I've marked where I am on your minimap, come to me so that I can \n       <b>screEEEEeeeech!</b>\n -darn the reception-   finish the briefing!",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActAnchor,
                    ProgressID = 1,
                    InputString = "Garrett Gruntle",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.CheckPlayerDist,
                    ProgressID = 1, // note that this still works even when the CurrentProgressID is 0 or 2 due to the adjecency rule
                    InitPosition = new Vector3(2,0,6), // needs specific location
                    InputNum = 64, // distance before it gets true
                    VaribleType = EVaribleType.True,
                    SetMissionVarIndex1 = 0, //enable
                    InputString = "",
                },  // this basically relays to the below
                new SubMissionStep
                {
                    StepType = SMStepType.ActForward,
                    ProgressID = 1,
                    SuccessProgressID = 10,// Go to Phase 2
                    VaribleType = EVaribleType.True,
                    SetMissionVarIndex1 = 0,
                    InputString = "",
                },


                // Phase 2 - Conversation
                new SubMissionStep
                {
                    StepType = SMStepType.ActMessagePurge,
                    ProgressID = 10,
                    VaribleType = EVaribleType.None,
                    InputNum = 1,// only fire once
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActAnchor,
                    ProgressID = 10,
                    InputString = "Garrett Gruntle",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 10,
                    SuccessProgressID = 15,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Name's <b>Garrett Gruntle</b>.  \n[Otherwise known as GG]",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 15,
                    SuccessProgressID = 20,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Hey there prospector, \nso uhh we're testing the new <b>Sub-Missions</b> protocol. \n   Say, it's a fine day today?",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 20,
                    SuccessProgressID = 25,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "I'll be assigning a series of fairly easy missions for you to try.",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 25,
                    SuccessProgressID = 30,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Say, have you met Agent Pow?    \n      \n   Was she nice?",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActOptions,
                    ProgressID = 30,
                    VaribleType = EVaribleType.True,
                    InputString = "Met Agent Pow?", // title
                    InputStringAux = "Yes", // button label
                    SetMissionVarIndex1 = 0, // value to check before throwing window (disabled on VaribleType: None)
                    SetMissionVarIndex2 = 1, // "No" pressed - Go to Phase 3
                    SetMissionVarIndex3 = 2, // "Other Option [Yes]" pressed - Go to Phase 3.5
                },
                new SubMissionStep
                {
                    StepType = SMStepType.CheckPlayerDist,
                    ProgressID = 30,
                    InitPosition = new Vector3(2,0,6), // needs specific location
                    InputNum = -64, // distance before it gets true - Negative makes this trigger when player outside
                    VaribleType = EVaribleType.True,
                    SetMissionVarIndex1 = 3, //ooop
                    InputString = "",
                },  // When this is triggered - Go to Phase 3.9


                // Phase 3 - Option "No"
                new SubMissionStep
                {
                    StepType = SMStepType.ActForward,
                    ProgressID = 30,
                    SuccessProgressID = 35,
                    VaribleType = EVaribleType.True,
                    SetMissionVarIndex1 = 1, // value to check before moving forward
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 35,
                    SuccessProgressID = 50, // Go to Phase 4
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Figures...    \n   [I wonder what she thinks of Crafty Mike and his shenanigans...] \n      [Wasn't Mike supposedly \"devilishly crafty?\"]",
                },

                // Phase 3.5 - Option "Yes"
                new SubMissionStep
                {
                    StepType = SMStepType.ActForward,
                    ProgressID = 30,
                    SuccessProgressID = 40,
                    VaribleType = EVaribleType.True,
                    SetMissionVarIndex1 = 2, // value to check before moving forward
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 40,
                    SuccessProgressID = 50,// Go to Phase 4
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Oh!  Maybe we CAN be friends after all!    \n      [Skreee you HAVE MAIL]\n   ...",
                },

                // Phase 3.9 - Option "i don't care"
                new SubMissionStep
                {
                    StepType = SMStepType.ActForward,
                    ProgressID = 30,
                    SuccessProgressID = 80,
                    VaribleType = EVaribleType.True,
                    SetMissionVarIndex1 = 3, // value to check before moving forward
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActMessagePurge,
                    ProgressID = 80,
                    VaribleType = EVaribleType.None,
                    InputNum = 1,// only fire once
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 80,
                    SuccessProgressID = 50,// Go to Phase 4
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Uhhh...    [You coming back?]   Figures...  \n          \n  I'll just send the missions over then...",
                },


                // Phase 4 - Letting the NPC leave the scene
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 50,
                    SuccessProgressID = 55,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "[Skreee]... \n\n  Looks like my time has vanished.  \n      Time to go then.",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 55,
                    VaribleType = EVaribleType.None,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Farewell comrade, may we mee-~  \n       <b>screEEEEeeeech!</b>",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActBoost,
                    ProgressID = 55,
                    VaribleType = EVaribleType.None,
                    InputString = "Garrett Gruntle.RAWTECH"
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActTimer,
                    ProgressID = 55,
                    VaribleType = EVaribleType.None,// For All Time. Always.
                    SetMissionVarIndex1 = 0,   // check this int (disabled)
                    SetMissionVarIndex2 = 0,   // tick this int up
                    InputNum = 1,          // every second we increase by this value
                },


                // Phase 5 - Cleanup
                //   There's a toggle that allows the mission to clean up Techs automatically
                //     but in the case you want to keep some techs, this is the example.
                new SubMissionStep
                {
                    StepType = SMStepType.ActForward,
                    ProgressID = 55,
                    SuccessProgressID = 60, //
                    VaribleType = EVaribleType.IntGreaterThan,
                    SetMissionVarIndex1 = 0,    // Check this int - A timer in Phase 4 is pushing this along
                    VaribleCheckNum = 7,    // Above this?  Then we go to Phase 5
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActRemove,
                    ProgressID = 60,
                    VaribleType = EVaribleType.None,
                    InputString = "Garrett Gruntle.RAWTECH"
                },  // The above fires first due to the way the updating works - top to bottom
                new SubMissionStep
                {
                    StepType = SMStepType.ActWin,
                    ProgressID = 60,
                    VaribleType = EVaribleType.None,
                },
            };
            mission1.Rewards = new SubMissionReward
            {
                EXPGain = 250,
                AddProgressX = 2,
            };
            return mission1;
        }
        public static SubMission MakePrefabCombat_Mission()
        {
            SubMission mission2 = new SubMission();
            mission2.Name = "Combat Mission";
            mission2.AltNames = new List<string>
            {
                "Shoot The Target",
                "Weapons Testing",
                "Show Me What You Got",
            };
            mission2.Description = "A nice and simple combat target mission template";
            mission2.AltDescs = new List<string>
            {
                "Let's see how well you can aim pal! \n Shoot the bullseye!",
                "We've set up a test dummy on the field, take it down!",
                "Show me those guns aren't just for show!",
            };
            mission2.GradeRequired = 0;
            mission2.MinProgressX = 1;
            mission2.Faction = "GSO";
            mission2.ClearTechsOnClear = true;
            mission2.ClearModularMonumentsOnClear = false;
            mission2.CheckList = new List<MissionChecklist>
            {
                new MissionChecklist
                {
                    ListArticle = "Destroy TestTarget",
                    ValueType = VarType.Unset,
                    GlobalIndex = 0,
                }
            };
            mission2.VarTrueFalse = new List<bool>
            {
                true,
                true,
            };
            mission2.EventList = new List<SubMissionStep>
            {
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 0,
                    VaribleType = EVaribleType.True, // WARNING - Does only bools!
                    SetMissionVarIndex1 = 0,    // Gets true, shows message
                    SetMissionVarIndex2 = 1,    // sends true on close
                    InputString = "Some Random Feller From GSO",
                    InputStringAux = "Hey it's working now! \n\n    Prospector, could you shoot that target over there?",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.SetupMM,
                    ProgressID = 0,
                    InitPosition = Vector3.up * 2f,//Vector3.forward * 6,
                    Forwards = Vector3.one * 2f,     // Changes the scale
                    InputString = "ModularBrickCube_(636)",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.SetupMM,
                    ProgressID = 0,
                    InitPosition = -Vector3.forward * 9,//Vector3.forward * 6,
                    Forwards = Vector3.one * 1.9f,     // Changes the scale
                    EulerAngles = Quaternion.LookRotation(new Vector3(0, 0.5f, 1).normalized).eulerAngles, // Changes the rotation
                    InputString = "ModularBrickCube_(636)",
                },
                /*
                new SubMissionStep
                {
                    StepType = SMStepType.ActShifter,
                    ProgressID = 0,
                    VaribleType = EVaribleType.True,
                    SetMissionVarIndex1 = 0,            // activates on true
                    RevProgressIDOffset = false,
                },*/
                new SubMissionStep
                {
                    StepType = SMStepType.SetupTech,
                    ProgressID = 0,
                    InputNum = 8,
                    VaribleCheckNum = 21, // Spread for random mass-spawning
                    VaribleType = EVaribleType.None,
                    InputString = "TestTarget.RAWTECH",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.SetupWaypoint,
                    ProgressID = SubMission.alwaysRunValue,// update For all time. Always.
                    InitPosition = new Vector3(2,0,6),
                    VaribleType = EVaribleType.None,
                    InputString = "TestTarget.RAWTECH"
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActAnchor,
                    ProgressID = 0,
                    InputString = "TestTarget.RAWTECH",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.CheckDestroy,
                    ProgressID = 0,
                    SuccessProgressID = 30,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "TestTarget.RAWTECH",
                    InputStringAux = "Tracked Tech",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActWin,
                    ProgressID = 30,
                    VaribleType = EVaribleType.None,
                },
            };
            mission2.Rewards = new SubMissionReward
            {
                MoneyGain = 950,
                EXPGain = 100,
                BlocksToSpawn = new List<string> { BlockTypes.GSOCockpit_111.ToString() }
            };
            return mission2;
        }
        public static SubMission MakePrefabHarvest_Mission()
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
        public static SubMission MakePrefabRewards_Mission()
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
        public static SubMission MakePrefabRewards_Mission_OBSOLETE()
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
        public static SMWorldObjectJSON MakePrefabWorldObject()
        {
            SMWorldObjectJSON SMWO = new SMWorldObjectJSON();
            SMWO.Name = "ModularBrickCube_(636)";
            SMWO.GameMaterialName = "AncientRuins";
            SMWO.VisualMeshName = "ModularBrickCube_6x3x6";
            SMWO.ColliderMeshName = "ModularBrickCube_6x3x6";
            SMWO.WorldObjectJSON = new Dictionary<string, object>();
            GameObject GOInst = UnityEngine.Object.Instantiate(new GameObject("Temp"), null);
            string lightName = typeof(Light).FullName;
            Light light = GOInst.AddComponent<Light>();
            light.intensity = 1337;
            Dictionary<string, object> pair = new Dictionary<string, object> {
                { "GameObject|nextLevel2", null },
                { lightName, MakeCompat(light) },
            };
            SMWO.WorldObjectJSON.Add("GameObject|nextLevel", pair);
            UnityEngine.Object.Destroy(GOInst);
            return SMWO;
        }
        public static void MakePrefabMissionTreeToFile(string TreeName)
        {

            Debug_SMissions.Log(KickStart.ModID + ": Setting up template reference...");

            SubMission mission1 = MakePrefabNPC_Mission();
            SubMission mission2 = MakePrefabCombat_Mission();
            SubMission mission3 = MakePrefabHarvest_Mission();
            SubMission mission4 = MakePrefabRewards_Mission();

            SMWorldObjectJSON SMWO = MakePrefabWorldObject();

            SubMissionTree tree = new SubMissionTree();
            tree.TreeName = TreeName;
            tree.Faction = "GSO";
            tree.ModID = KickStart.ModID;
            tree.MissionNames.Add("NPC Mission");
            tree.MissionNames.Add("Harvest Mission");
            tree.MissionNames.Add("GSO Blocks Kit");
            tree.RepeatMissionNames.Add("Combat Mission");
            tree.WorldObjectFileNames.Add("ModularBrickCube_(636)");

            string RawTreeJSON = JsonConvert.SerializeObject(tree, Formatting.Indented, JSONSaver);
            ValidateDirectory(MissionsDirectory);
            ValidateDirectory(Path.Combine(MissionsDirectory, TreeName));
            ValidateDirectory(Path.Combine(MissionsDirectory, TreeName, "Raw Techs"));
            ValidateDirectory(Path.Combine(MissionsDirectory, TreeName, "Missions"));
            ValidateDirectory(Path.Combine(MissionsDirectory, TreeName, "Pieces"));
            string one = JsonConvert.SerializeObject(mission1, Formatting.Indented, JSONSaverMission);
            string two = JsonConvert.SerializeObject(mission2, Formatting.Indented, JSONSaverMission);
            string three = JsonConvert.SerializeObject(mission3, Formatting.Indented, JSONSaverMission);
            string four = LargeStringData.GarrettGruntle;
            string five = LargeStringData.TestTarget;
            string six = JsonConvert.SerializeObject(mission4, Formatting.Indented, JSONSaverMission);
            string seven = JsonConvert.SerializeObject(SMWO, Formatting.Indented, JSONSafe);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, TreeName, "Missions", mission1.Name), one);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, TreeName, "Missions", mission2.Name), two);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, TreeName, "Missions", mission3.Name), three);
            TryWriteToFile(Path.Combine(MissionsDirectory, TreeName, "Raw Techs", "Garrett Gruntle.RAWTECH"), four);
            TryWriteToFile(Path.Combine(MissionsDirectory, TreeName, "Raw Techs", "TestTarget.RAWTECH"), five);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, TreeName, "Missions", mission4.Name), six);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, TreeName, "Pieces", "ModularBrickCube_(636)"), seven);
            TryWriteToTextFile(Path.Combine(MissionsDirectory, "SubMissionHelp"), SubMission.GetDocumentation());
            TryWriteToTextFile(Path.Combine(MissionsDirectory, "SubMissionsSteps"), SubMissionStep.GetALLStepDocumentations());
            TryCopyFile(Path.Combine(DLLDirectory, "Garrett Gruntle.png"), Path.Combine(MissionsDirectory, TreeName, "Garrett Gruntle.png"));
            TryCopyFile(Path.Combine(DLLDirectory, "ModularBrickCube_6x3x6.obj"), Path.Combine(MissionsDirectory, TreeName, "ModularBrickCube_6x3x6.obj"));
            try
            {
                File.WriteAllText(Path.Combine(MissionsDirectory, TreeName, "MissionTree.json "), RawTreeJSON);
                Debug_SMissions.Log(KickStart.ModID + ": Saved MissionTree.json  for " + TreeName + " successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Export Mission Prefabs", KickStart.ModID + ": Could not edit MissionTree.json  for " + TreeName +
                    "\n - TerraTech + SubMissions was not permitted to access the MissionTree.json  destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Export Mission Prefabs", KickStart.ModID + ": Could not edit MissionTree.json  for " + TreeName +
                    "\n - File MissionTree.json  is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Export Mission Prefabs", KickStart.ModID + ": Could not edit MissionTree.json  for " + TreeName +
                    "\n - File MissionTree.json  is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Export Mission Prefabs", KickStart.ModID + ": Could not edit MissionTree.json  for " + TreeName +
                    "\n - File MissionTree.json  is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
            Debug_SMissions.Log(KickStart.ModID + ": Setup template reference successfully.");
        }


        private static Dictionary<string, object> MakeCompat<T>(T convert)
        {
            IEnumerable<PropertyInfo> PI = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Debug_SMissions.Log(KickStart.ModID + ": MakeCompat - Compiling " + typeof(T) + " which has " + PI.Count() + " properties");
            Dictionary<string, object> converted = new Dictionary<string, object>();
            foreach (PropertyInfo PIC in PI)
            {
                //if (FI.IsPublic)
                converted.Add(PIC.Name, PIC.GetValue(convert));
            }
            return converted;
        }


        public static bool GetName(string FolderDirectory, out string output, bool doJSON = false)
        {
            StringBuilder final = new StringBuilder();
            foreach (char ch in FolderDirectory)
            {
                if (ch == Path.DirectorySeparatorChar || ch == Path.AltDirectorySeparatorChar)
                {
                    final.Clear();
                }
                else
                    final.Append(ch);
            }
            if (doJSON)
            {
                if (!final.ToString().Contains(".json"))
                {
                    output = "error";
                    return false;
                }
                final.Remove(final.Length - 5, 5);// remove ".json"
            }
            output = final.ToString();
            //Debug_SMissions.Log(KickStart.ModID + ": Cleaning Name " + output);
            return true;
        }
        public static bool DirectoryExists(string DirectoryIn)
        {
            return Directory.Exists(DirectoryIn);
        }
        public static void ValidateDirectory(string DirectoryIn)
        {
            if (!GetName(DirectoryIn, out string name))
                return;// error
            if (!Directory.Exists(DirectoryIn))
            {
                Debug_SMissions.Log(KickStart.ModID + ": Generating " + name + " folder.");
                try
                {
                    Directory.CreateDirectory(DirectoryIn);
                    Debug_SMissions.Log(KickStart.ModID + ": Made new " + name + " folder successfully.");
                }
                catch (UnauthorizedAccessException e)
                {
                    SMUtil.Assert(false, "IO Sanity Check", KickStart.ModID + ": Could not create new " + name + " folder" +
                        "\n - TerraTech + SubMissions was not permitted to access Folder \"" + name + "\"", e);
                }
                catch (PathTooLongException e)
                {
                    SMUtil.Assert(false, "IO Sanity Check", KickStart.ModID + ": Could not create new " + name + " folder" +
                        "\n - Folder \"" + name + "\" is located in a directory that makes it too deep and long" +
                        " for the OS to navigate correctly", e);
                }
                catch (DirectoryNotFoundException e)
                {
                    SMUtil.Assert(false, "IO Sanity Check", KickStart.ModID + ": Could not create new " + name + " folder" +
                        "\n - Path to place Folder \"" + name + "\" is incomplete (there are missing folders in" +
                        " the target folder hierachy)", e);
                }
                catch (IOException e)
                {
                    SMUtil.Assert(false, "IO Sanity Check", KickStart.ModID + ": Could not create new " + name + " folder" +
                        "\n - Folder \"" + name + "\" is not accessable because IOException(?) was thrown!", e);
                }
                catch (Exception e)
                {
                    throw new MandatoryException("Encountered exception not properly handled", e);
                }
            }
        }
        public static void TryWriteToFile(string FileDirectory, string ToOverwrite)
        {
            if (!GetName(FileDirectory, out string name))
                return;// error
            try
            {
                File.WriteAllText(FileDirectory, ToOverwrite);
                Debug_SMissions.Log(KickStart.ModID + ": Saved " + name + " successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not edit " + name +
                    "\n - TerraTech + SubMissions was not permitted to access the " + name + " destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not edit " + name +
                    "\n - File " + name + " is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not edit " + name +
                    "\n - File " + name + " is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not edit " + name +
                    "\n - File " + name + " is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }
        public static void TryWriteToJSONFile(string FileDirectory, string ToOverwrite)
        {
            if (!GetName(FileDirectory, out string name))
                return;// error
            try
            {
                File.WriteAllText(FileDirectory + ".json", ToOverwrite);
                Debug_SMissions.Log(KickStart.ModID + ": Saved " + name + ".json successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not edit " + name + ".json" +
                    "\n - TerraTech + SubMissions was not permitted to access the " + name + ".json destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not edit " + name + ".json" +
                    "\n - File " + name + ".json is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not edit " + name + ".json" +
                    "\n - File " + name + ".json is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not edit " + name + ".json" +
                    "\n - File " + name + ".json is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }
        public static void TryWriteToTextFile(string FileDirectory, string ToOverwrite)
        {
            if (!GetName(FileDirectory, out string name))
                return;// error
            try
            {
                File.WriteAllText(FileDirectory + ".txt", ToOverwrite);
                Debug_SMissions.Log(KickStart.ModID + ": Saved " + name + ".txt successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not write to " + name + ".txt" +
                    "\n - TerraTech + SubMissions was not permitted to access the target file", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not write to " + name + ".txt" +
                    "\n - File " + name + ".txt is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not write to " + name + ".txt" +
                    "\n -  File " + name + ".txt is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not write to " + name + ".txt" +
                    "\n -  File " + name + ".txt is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }
        private static void TryCopyFile(string FileDirectory, string FileDirectoryEnd)
        {
            if (!GetName(FileDirectory, out string name))
                return;// error
            try
            {
                File.Copy(FileDirectory, FileDirectoryEnd);
                Debug_SMissions.Log(KickStart.ModID + ": Copied " + name + " successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not copy " + name +
                    "\n   TerraTech + SubMissions was not permitted to access the target file", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not copy " + name +
                    "\n - Target file is too deep and long for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not copy " + name +
                    "\n - Target file is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Saving", KickStart.ModID + ": Could not copy " + name +
                    "\n - Target file is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }
    }
}
