using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Unity;
using UnityEngine;
using TerraTechETCUtil;
using Sub_Missions.Steps;
using Sub_Missions.ModularMonuments;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Sub_Missions
{
    public class MissionTypeEnumConverter : StringEnumConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is SubMissionStep)
            {
                writer.WriteValue(Enum.GetName(typeof(SubMissionStep), (SubMissionStep)value));
                return;
            }

            base.WriteJson(writer, value, serializer);
        }
    }

    /// <summary>
    /// Handles all JSON Mission loading for JSON Mission modders. 
    /// Does not handle saving save games.
    /// Cannot decode existing missions - uScript leaves behind an apocalyptic aftermath of hair-pulling 
    ///   methods and fields that are nearly impossible to retrace.
    ///    I believe the unreadable format is intentional, but it goes strongly 
    ///    against TerraTech's normal coding accessability.
    /// </summary>
    public class SMissionJSONLoader : MonoBehaviour
    {
        public static string DLLDirectory;
        public static string BaseDirectory;
        public static string MissionsDirectory;
        public static string MissionSavesDirectory;
        public static string MissionCorpsDirectory;

        private static JsonSerializerSettings JSONSaver = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };
        private static JsonSerializerSettings JSONSaverMission = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Converters = new List<JsonConverter>{ new MissionTypeEnumConverter() },
        };
        private static JsonSerializerSettings JSONSafe = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 12,
        };

        public static void SetupWorkingDirectories()
        {
            DirectoryInfo di = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
            di = di.Parent; // off of this DLL
            DLLDirectory = di.ToString();
            DirectoryInfo game = new DirectoryInfo(Application.dataPath);
            game = game.Parent; // out of the game folder
            BaseDirectory = game.ToString();
            MissionsDirectory = Path.Combine(game.ToString(), "Custom SMissions");
            MissionSavesDirectory = Path.Combine(game.ToString(), "SMissions Saves");
            MissionCorpsDirectory = Path.Combine(game.ToString(), "SMissions Corps");
            ValidateDirectory(MissionsDirectory);
            ValidateDirectory(MissionSavesDirectory);
            ValidateDirectory(MissionCorpsDirectory);

            if (!ManSMCCorps.hasScanned)
            {
                ManSMCCorps.LoadAllCorps();
            }
#if DEBUG
            Debug_SMissions.Log("SubMissions: DLL folder is at: " + DLLDirectory);
            Debug_SMissions.Log("SubMissions: Custom SMissions is at: " + MissionsDirectory);
            //SMCCorpLicense.SaveTemplateToDisk();
#endif
        }
        public static void SetupWorkingDirectoriesLegacy()
        {
            DirectoryInfo di = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
            di = di.Parent; // off of this DLL
            DLLDirectory = di.ToString();
            DirectoryInfo game = new DirectoryInfo(Application.dataPath);
            game = game.Parent; // out of the game folder
            BaseDirectory = game.ToString();
            MissionsDirectory = Path.Combine(game.ToString(), "Custom SMissions");
            MissionSavesDirectory = Path.Combine(game.ToString(), "SMissions Saves");
            MissionCorpsDirectory = Path.Combine(game.ToString(), "SMissions Corps");
            ValidateDirectory(MissionsDirectory);
            ValidateDirectory(MissionSavesDirectory);
            ValidateDirectory(MissionCorpsDirectory);

            if (!ManSMCCorps.hasScanned)
            {
                ManSMCCorps.LoadAllCorps();
            }
#if DEBUG
            Debug_SMissions.Log("SubMissions: DLL folder is at: " + DLLDirectory);
            Debug_SMissions.Log("SubMissions: Custom SMissions is at: " + MissionsDirectory);
            //SMCCorpLicense.SaveTemplateToDisk();
#endif
        }



        // First Startup
        public static SubMission MakePrefabMission1()
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
            mission1.VarTrueFalse = new List<bool>
            {
                false,  // Range check
                false,  // Choice 1 "No"
                false,  // Choice 2 "Yes"
                false,  // When Player drives away while making the choice
            };
            mission1.VarInts = new List<int>
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
                    InputString = "Garrett Gruntle",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.SetupWaypoint,
                    ProgressID = SubMission.alwaysRunValue,// update For all time. Always.
                    InitPosition = new Vector3(2,0,6),
                    VaribleType = EVaribleType.None,
                    InputString = "Garrett Gruntle", // give it a TrackedTech Tech name to assign it to that tech
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
                    InputStringAux = "I've marked where I am on your minimap, come to me so that I can \n       <b>screEEEEeeeech!</b>\n -darn them cheap buggers-   IMEAN finish the briefing!",
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
                    SuccessProgressID = 15,// Go to Phase 2
                    VaribleType = EVaribleType.True,
                    SetMissionVarIndex1 = 0,
                    InputString = "",
                },


                // Phase 2 - Conversation
                new SubMissionStep
                {
                    StepType = SMStepType.ActMessagePurge,
                    ProgressID = 15,
                    VaribleType = EVaribleType.None,
                    InputNum = 1,// only fire once
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 15,
                    SuccessProgressID = 20,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Hey there prospector, so uhh we're testing the new sub-missions protocol. \n\n Say, it's a fine day today?",
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
                    InputStringAux = "Figures...    \n      \n   [I wonder what she thinks of Crafty Mike...] \n      [Wasn't Mike supposedly \"devilishly handsome?\"]",
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
                    InputStringAux = "Oh!  Maybe we CAN be friends after all!    \n      \n   ...",
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
                    InputStringAux = "Uhhh...    \n[You coming back?]\n   Figures...  \n          \n       I'll just send the missions over then...",
                },


                // Phase 4 - Letting the NPC leave the scene
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 50,
                    SuccessProgressID = 55,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Brrrrt... \n\n  Looks like my time has vanished.  \n      Time to go then.",
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
                    InputString = "Garrett Gruntle"
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
                    InputString = "Garrett Gruntle"
                },  // The above fires first due to the way the updating works - top to bottom
                new SubMissionStep
                {
                    StepType = SMStepType.ActWin,
                    ProgressID = 60,
                    VaribleType = EVaribleType.None,
                },
            };
            mission1.TrackedTechs = new List<TrackedTech>
            {
                new TrackedTech("Garrett Gruntle")
            };
            mission1.Rewards = new SubMissionReward
            {
                EXPGain = 250,
                AddProgressX = 2,
            };
            return mission1;
        }
        public static SubMission MakePrefabMission2()
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
                    EulerAngles = Quaternion.LookRotation(new Vector3(0,0.5f,1).normalized).eulerAngles, // Changes the rotation
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
                    InputString = "TestTarget",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.SetupWaypoint,
                    ProgressID = SubMission.alwaysRunValue,// update For all time. Always.
                    InitPosition = new Vector3(2,0,6),
                    VaribleType = EVaribleType.None,
                    InputString = "TestTarget"
                },
                new SubMissionStep
                {
                    StepType = SMStepType.CheckDestroy,
                    ProgressID = 0,
                    SuccessProgressID = 30,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "TestTarget",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActWin,
                    ProgressID = 30,
                    VaribleType = EVaribleType.None,
                },
            };
            mission2.TrackedTechs = new List<TrackedTech>
            {
                new TrackedTech("TestTarget")
            };
            mission2.Rewards = new SubMissionReward
            {
                MoneyGain = 950,
                EXPGain = 100,
                BlocksToSpawn = new List<string> { BlockTypes.GSOCockpit_111.ToString() }
            };
            return mission2;
        }
        public static SubMission MakePrefabMission3()
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
        public static SubMission MakePrefabMission4()
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
            SMWO.VisualMeshName = "ModularBrickCube_6x3x6.obj";
            SMWO.WorldObjectJSON = new Dictionary<string, object>();
            GameObject GOInst = Instantiate(new GameObject("Temp"), null);
            string lightName = typeof(Light).FullName;
            Light light = GOInst.AddComponent<Light>();
            light.intensity = 1337;
            Dictionary<string, object> pair = new Dictionary<string, object> {
                { "GameObject|nextLevel2", null },
                { lightName, MakeCompat(light) },
            };
            SMWO.WorldObjectJSON.Add("GameObject|nextLevel", pair);
            Destroy(GOInst);
            return SMWO;
        }
        public static void MakePrefabMissionTreeToFile(string TreeName)
        {
            Debug_SMissions.Log("SubMissions: Setting up template reference...");

            SubMission mission1 = MakePrefabMission1();
            SubMission mission2 = MakePrefabMission2();
            SubMission mission3 = MakePrefabMission3();
            SubMission mission4 = MakePrefabMission4();

            SMWorldObjectJSON SMWO = MakePrefabWorldObject();

            SubMissionTree tree = new SubMissionTree();
            tree.TreeName = "Template";
            tree.Faction = "GSO";
            tree.ModID = "Mod Missions";
            tree.MissionNames.Add("NPC Mission");
            tree.MissionNames.Add("Harvest Mission");
            tree.MissionNames.Add("Water Blocks Aid");
            tree.RepeatMissionNames.Add("Combat Mission");
            tree.WorldObjectFileNames.Add("ModularBrickCube_(636)");

            string RawTreeJSON = JsonConvert.SerializeObject(tree, Formatting.Indented, JSONSaver);
            ValidateDirectory(MissionsDirectory);
            ValidateDirectory(Path.Combine(MissionsDirectory, TreeName));
            ValidateDirectory(Path.Combine(MissionsDirectory, TreeName, "Raw Techs"));
            ValidateDirectory(Path.Combine(MissionsDirectory, TreeName,"Missions"));
            ValidateDirectory(Path.Combine(MissionsDirectory, TreeName, "Pieces"));
            string one = JsonConvert.SerializeObject(mission1, Formatting.Indented, JSONSaverMission);
            string two = JsonConvert.SerializeObject(mission2, Formatting.Indented, JSONSaverMission);
            string three = JsonConvert.SerializeObject(mission3, Formatting.Indented, JSONSaverMission);
            string four = "{\n\"Name\": \"Garrett Gruntle\",\n\"IsAnchored\": true,\n\"Blueprint\": \"{\\\"t\\\":\\\"GSOAnchorRotating_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSO_Chassis_Cab_314\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"r\\\":18}|{\\\"t\\\":\\\"GSOCockpit_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOGyroAllAxisActive_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":3.0,\\\"z\\\":0.0},\\\"r\\\":21}|{\\\"t\\\":\\\"GSOAIGuardController_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":4.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOMortarFixed_211\\\",\\\"p\\\":{\\\"x\\\":-1.0,\\\"y\\\":3.0,\\\"z\\\":1.0},\\\"r\\\":19}|{\\\"t\\\":\\\"GSO_Character_A_111\\\",\\\"p\\\":{\\\"x\\\":-2.0,\\\"y\\\":3.0,\\\"z\\\":0.0},\\\"r\\\":3}|{\\\"t\\\":\\\"GSO_Character_A_111\\\",\\\"p\\\":{\\\"x\\\":2.0,\\\"y\\\":3.0,\\\"z\\\":0.0},\\\"r\\\":1}|{\\\"t\\\":\\\"GSOBlockHalf_111\\\",\\\"p\\\":{\\\"x\\\":2.0,\\\"y\\\":2.0,\\\"z\\\":0.0},\\\"r\\\":22}|{\\\"t\\\":\\\"GSOBlockHalf_111\\\",\\\"p\\\":{\\\"x\\\":-2.0,\\\"y\\\":2.0,\\\"z\\\":0.0},\\\"r\\\":22}|{\\\"t\\\":\\\"GSOPlough_211\\\",\\\"p\\\":{\\\"x\\\":3.0,\\\"y\\\":2.0,\\\"z\\\":1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOPlough_211\\\",\\\"p\\\":{\\\"x\\\":-2.0,\\\"y\\\":2.0,\\\"z\\\":1.0},\\\"r\\\":12}|{\\\"t\\\":\\\"GSOBattery_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":3.0,\\\"z\\\":-1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOTractorMini_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0,\\\"z\\\":1.0},\\\"r\\\":16}|{\\\"t\\\":\\\"GSOMortarFixed_211\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":2.0,\\\"z\\\":1.0},\\\"r\\\":17}|{\\\"t\\\":\\\"GSOBlockLongHalf_211\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":2.0,\\\"z\\\":-1.0},\\\"r\\\":21}|{\\\"t\\\":\\\"GSOBlockLongHalf_211\\\",\\\"p\\\":{\\\"x\\\":-1.0,\\\"y\\\":3.0,\\\"z\\\":-1.0},\\\"r\\\":23}|{\\\"t\\\":\\\"VENBracketStraight_211\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":5.0,\\\"z\\\":-1.0},\\\"r\\\":8}|{\\\"t\\\":\\\"GSORadar_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":6.0,\\\"z\\\":-1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOLightSpot_111\\\",\\\"p\\\":{\\\"x\\\":-1.0,\\\"y\\\":4.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOLightFixed_111\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":4.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOBooster_112\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"r\\\":18}|{\\\"t\\\":\\\"GSOBooster_112\\\",\\\"p\\\":{\\\"x\\\":-1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"r\\\":18}|{\\\"t\\\":\\\"GSOBooster_112\\\",\\\"p\\\":{\\\"x\\\":-1.0,\\\"y\\\":1.0,\\\"z\\\":-1.0},\\\"r\\\":20}|{\\\"t\\\":\\\"GSOBooster_112\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":-1.0},\\\"r\\\":20}|{\\\"t\\\":\\\"GSOWheelHub_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOWheelHub_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":-1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOArmourPlateCab_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":2.0},\\\"r\\\":4}|{\\\"t\\\":\\\"GSOArmourPlateCab_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":-2.0},\\\"r\\\":6}|{\\\"t\\\":\\\"GSOBlockHalf_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOArmourPlateSmall_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":3.0,\\\"z\\\":1.0},\\\"r\\\":11}|{\\\"t\\\":\\\"GSOFuelTank_121\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0,\\\"z\\\":-1.0},\\\"r\\\":6}\",\n\"InfBlocks\": false,\n\"Faction\": 1,\n\"NonAggressive\": false,\n\"Cost\": 42828\n}";
            string five = "{\n\"Name\": \"TestTarget\",\n\"IsAnchored\": true,\n\"Blueprint\": \"{\\\"t\\\":\\\"GSOAnchorRotating_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOBlock_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOCockpit_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOPlough_311\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":0.0},\\\"r\\\":17}|{\\\"t\\\":\\\"GSOPlough_311\\\",\\\"p\\\":{\\\"x\\\":-1.0,\\\"y\\\":1.0,\\\"z\\\":0.0},\\\"r\\\":19}|{\\\"t\\\":\\\"GSOArmourPlateSmall_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOArmourPlateSmall_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0,\\\"z\\\":-1.0},\\\"r\\\":2}|{\\\"t\\\":\\\"GSOArmourPlateSmall_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":3.0,\\\"z\\\":0.0},\\\"r\\\":11}\",\n\"InfBlocks\": false,\n\"Faction\": 1,\n\"NonAggressive\": false\n}";
            string six = JsonConvert.SerializeObject(mission4, Formatting.Indented, JSONSaverMission);
            string seven = JsonConvert.SerializeObject(SMWO, Formatting.Indented, JSONSafe);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, TreeName, "Missions", "NPC Mission"), one);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, TreeName, "Missions", "Combat Mission"), two);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, TreeName, "Missions", "Harvest Mission"), three);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, TreeName, "Raw Techs", "Garrett Gruntle"), four);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, TreeName, "Raw Techs", "TestTarget"), five);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, TreeName, "Missions", "Water Blocks Kit"), six);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, TreeName, "Pieces", "ModularBrickCube_(636)"), seven);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, "SubMissionHelp"), SubMission.GetDocumentation());
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, "SubMissionsSteps"), SubMissionStep.GetALLStepDocumentations());
            TryCopyFile(Path.Combine(DLLDirectory, "Garrett Gruntle.png"), Path.Combine(MissionsDirectory, TreeName, "Garrett Gruntle.png"));
            TryCopyFile(Path.Combine(DLLDirectory, "ModularBrickCube_6x3x6.obj"), Path.Combine(MissionsDirectory, TreeName, "ModularBrickCube_6x3x6.obj"));
            try
            {
                File.WriteAllText(Path.Combine(MissionsDirectory, TreeName, "MissionTree.json"), RawTreeJSON);
                Debug_SMissions.Log("SubMissions: Saved MissionTree.json for " + TreeName + " successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Export Mission Prefabs", "SubMissions: Could not edit MissionTree.json for " + TreeName +
                    "\n - TerraTech + SubMissions was not permitted to access the MissionTree.json destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Export Mission Prefabs", "SubMissions: Could not edit MissionTree.json for " + TreeName +
                    "\n - File MissionTree.json is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Export Mission Prefabs", "SubMissions: Could not edit MissionTree.json for " + TreeName +
                    "\n - File MissionTree.json is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Export Mission Prefabs", "SubMissions: Could not edit MissionTree.json for " + TreeName +
                    "\n - File MissionTree.json is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
            Debug_SMissions.Log("SubMissions: Setup template reference successfully.");
        }


        // Majors
        public static List<SubMissionTree> LoadAllTrees()
        {
            List<SubMissionTree> temps = new List<SubMissionTree>();
            Debug_SMissions.Log("SubMissions: Searching Official Mods Folder...");
            List<string> directories = GetTreeDirectoriesOfficial();
            Debug_SMissions.Log("SubMissions: Found " + directories.Count + " trees...");
            foreach (string directed in directories)
            {
                if (GetName(directed, out string name, true))
                {
                    try
                    {
                        TreeLoader(new SubMissionHierachyDirectory(name, directed), out SubMissionTree Tree);
                        Debug_SMissions.Log("SubMissions: Added Tree " + name);
                        temps.Add(Tree);
                    }
                    catch (MandatoryException e)
                    {
                        throw e;
                    }
                    catch (Exception e)
                    {
                        SMUtil.Assert(false, "Mission Tree ~ " + name, "Could not load mission tree " + name, e);
                    }
                }

            }
            // LOAD OFFICIAL
            foreach (var item in ResourcesHelper.IterateAllMods())
            {
                foreach (var item2 in ResourcesHelper.IterateObjectsFromModContainer<TextAsset>(item.Value))
                {
                    if (item2 != null && item2.name.Contains("_Hierarchy"))
                    {
                        try
                        {
                            var bundleDir = SubMissionHierachyAssetBundle.MakeInstance(item.Value, item2.text);
                            TreeLoader(bundleDir, out SubMissionTree Tree);
                            Debug_SMissions.Log("SubMissions: Added Bundled Tree " + bundleDir.TreeName);
                            temps.Add(Tree);
                        }
                        catch (MandatoryException e)
                        {
                            throw e;
                        }
                        catch (Exception e)
                        {
                            SMUtil.Assert(false, "Mission Tree ~ UNKNOWN", "Could not load Bundled mission tree for " + item.Key, e);
                        }
                    }
                }
            }
            //
            ValidateDirectory(MissionsDirectory);
            Debug_SMissions.Log("SubMissions: Searching Custom SMissions Folder...");
            List<string> namesUnofficial = GetCleanedNamesInDirectory();
            Debug_SMissions.Log("SubMissions: Found " + namesUnofficial.Count + " trees...");
            foreach (string name in namesUnofficial)
            {
                try
                {
                    TreeLoader(new SubMissionHierachyDirectory(name, Path.Combine(MissionsDirectory, name)), out SubMissionTree Tree);
                    Debug_SMissions.Log("SubMissions: Added Tree " + name);
                    temps.Add(Tree);
                }
                catch (MandatoryException e)
                {
                    throw e;
                }
                catch (Exception e)
                {
                    SMUtil.Assert(false, "Mission Tree ~ " + name, "Could not load mission tree " + name, e);
                }
            }
            return temps;
        }

        public static List<SubMission> LoadAllMissions(SubMissionTree tree)
        {
            ValidateDirectory(MissionsDirectory);
            List<string> names = GetCleanedNamesInDirectory(Path.Combine(tree.TreeName, "Missions"), true);
            List<SubMission> temps = new List<SubMission>();
            foreach (string name in names)
            {
                var mission = MissionLoader(tree, name);
                if (mission == null)
                {
                    SMUtil.Error(false, "Mission(Load) ~ " + name, "<b> CRITICAL ERROR IN HANDLING MISSION " + 
                        name + " - UNABLE TO IMPORT ANY INFORMATION! </b>");
                    continue;
                }
                temps.Add(mission);
            }
            return temps;
        }



        // Utilities
        public static bool TryGetCorpInfoData(string factionShort,  out string results)
        {
            string location = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.Parent.ToString();
            // Goes to the cluster directory where all the mods are
            string fileName = factionShort + "_MissionCorp.json";

            int attempts = 0;
            foreach (string directoryLoc in Directory.GetDirectories(location))
            {
                while (true)
                {
                    try
                    {
                        string GO;
                        GO = directoryLoc + "\\" + fileName;
                        if (File.Exists(GO))
                        {
                            attempts++;
                            results = File.ReadAllText(GO);
                            return true;
                        }
                        else
                            break;
                    }
                    catch (Exception e)
                    {
                        Debug_SMissions.Log("SubMissions: TryGetCorpInfoDirectory - Error on MissionCorp search " + factionShort + " | " + e);
                        break;
                    }
                }
                attempts = 0;
            }

            string dataName = factionShort + "_MissionCorp";
            foreach (var contained in ResourcesHelper.IterateAllMods())
            {
                if (contained.Value == null)
                    continue;
                foreach (var item in ResourcesHelper.IterateObjectsFromModContainer<TextAsset>(contained.Value))
                {
                    if (item.name.Contains(dataName))
                    {
                        results = item.text;
                        return true;
                    }
                }
            }
            results = null;
            return false;
        }
        public static List<string> GetTreeDirectoriesOfficial()
        {
            List<string> Cleaned = new List<string>();
            string location = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.Parent.ToString();
            // Goes to the cluster directory where all the mods are

            foreach (int cCorp in ManMods.inst.GetCustomCorpIDs())
            {
                int attempts = 0;
                foreach (string directoryLoc in Directory.GetDirectories(location))
                {
                    while (true)
                    {
                        try
                        {
                            string GO;
                            string fileName = ManMods.inst.FindCorpShortName((FactionSubTypes)cCorp) + "_MissionTree_" + attempts + ".json";
                            GO = directoryLoc + "\\" + fileName;
                            if (File.Exists(GO))
                            {
                                Debug_SMissions.Log("SubMissions: GetTreeNamesOfficial - " + GO);
                                attempts++;
                                if (GetName(fileName, out string cleanName, true))
                                {
                                    Cleaned.Add(cleanName);
                                }
                            }
                            else
                                break;
                        }
                        catch (Exception e)
                        {
                            Debug_SMissions.Log("LocalModCorpAudio: RegisterCorpMusics - Error on Music search " + cCorp + " | " + e);
                            break;
                        }
                    }
                    attempts = 0;
                }
            }
            return Cleaned;
        }
        public static List<string> GetCleanedNamesInDirectory(string directoryFromMissionsDirectory = "", bool doJSON = false)
        {
            string search;
            if (directoryFromMissionsDirectory == "")
                search = MissionsDirectory;
            else
                search = Path.Combine(MissionsDirectory, directoryFromMissionsDirectory);
            IEnumerable<string> toClean;
            if (doJSON)
                toClean = Directory.GetFiles(search);
            else
                toClean = Directory.GetDirectories(search);
            //Debug_SMissions.Log("SubMissions: Cleaning " + toClean.Count);
            List<string> Cleaned = new List<string>();
            foreach (string cleaning in toClean)
            {
                if (GetName(cleaning, out string cleanName, doJSON))
                {
                    Cleaned.Add(cleanName);
                }
            }
            return Cleaned;
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
            //Debug_SMissions.Log("SubMissions: Cleaning Name " + output);
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
                Debug_SMissions.Log("SubMissions: Generating " + name + " folder.");
                try
                {
                    Directory.CreateDirectory(DirectoryIn);
                    Debug_SMissions.Log("SubMissions: Made new " + name + " folder successfully.");
                }
                catch (UnauthorizedAccessException e)
                {
                    SMUtil.Assert(false, "IO Sanity Check", "SubMissions: Could not create new " + name + " folder" +
                        "\n - TerraTech + SubMissions was not permitted to access Folder \"" + name + "\"", e);
                }
                catch (PathTooLongException e)
                {
                    SMUtil.Assert(false, "IO Sanity Check", "SubMissions: Could not create new " + name + " folder" +
                        "\n - Folder \"" + name + "\" is located in a directory that makes it too deep and long" +
                        " for the OS to navigate correctly", e);
                }
                catch (DirectoryNotFoundException e)
                {
                    SMUtil.Assert(false, "IO Sanity Check", "SubMissions: Could not create new " + name + " folder" +
                        "\n - Path to place Folder \"" + name + "\" is incomplete (there are missing folders in" +
                        " the target folder hierachy)", e);
                }
                catch (IOException e)
                {
                    SMUtil.Assert(false, "IO Sanity Check", "SubMissions: Could not create new " + name + " folder" +
                        "\n - Folder \"" + name + "\" is not accessable because IOException(?) was thrown!", e);
                }
                catch (Exception e)
                {
                    throw new MandatoryException("Encountered exception not properly handled", e);
                }
            }
        }
        public static void TryWriteToJSONFile(string FileDirectory, string ToOverwrite)
        {
            if (!GetName(FileDirectory, out string name))
                return;// error
            try
            {
                File.WriteAllText(FileDirectory + ".json", ToOverwrite);
                Debug_SMissions.Log("SubMissions: Saved " + name + ".json successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Saving", "SubMissions: Could not edit " + name + ".json" +
                    "\n - TerraTech + SubMissions was not permitted to access the " + name + ".json destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Saving", "SubMissions: Could not edit " + name + ".json" +
                    "\n - File " + name + ".json is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Saving", "SubMissions: Could not edit " + name + ".json" +
                    "\n - File " + name + ".json is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Saving", "SubMissions: Could not edit " + name + ".json" +
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
                Debug_SMissions.Log("SubMissions: Saved " + name + ".txt successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Saving", "SubMissions: Could not write to " + name + ".txt" +
                    "\n - TerraTech + SubMissions was not permitted to access the target file", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Saving", "SubMissions: Could not write to " + name + ".txt" +
                    "\n - File " + name + ".txt is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Saving", "SubMissions: Could not write to " + name + ".txt" +
                    "\n -  File " + name + ".txt is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Saving", "SubMissions: Could not write to " + name + ".txt" +
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
                Debug_SMissions.Log("SubMissions: Copied " + name + " successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Saving", "SubMissions: Could not copy " + name +
                    "\n   TerraTech + SubMissions was not permitted to access the target file", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Saving", "SubMissions: Could not copy " + name +
                    "\n - Target file is too deep and long for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Saving", "SubMissions: Could not copy " + name +
                    "\n - Target file is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Saving", "SubMissions: Could not copy " + name +
                    "\n - Target file is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }


        // -------------------------------- JSON Handlers --------------------------------
        public static void TreeLoader(SubMissionHierachy SMH, out SubMissionTree Tree)
        {
            try
            {
                string output = SMH.LoadMissionTreeTrunkFromFile();
                Tree = JsonConvert.DeserializeObject<SubMissionTree>(output, new MissionTypeEnumConverter());
                Tree.TreeHierachy = SMH;
                SMH.LoadMissionTreeDataFromFile(ref Tree.MissionTextures,
                    ref Tree.MissionMeshes, ref Tree.TreeTechs);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new DirectoryNotFoundException("SubMissions: Check your Tree file names, " +
                    "cases where you referenced the names and make sure they match!!!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }
        public static SubMission MissionLoader(SubMissionTree tree, string MissionName)
        {
            try
            {
                string output = tree.TreeHierachy.LoadMissionTreeMissionFromFile(MissionName);
                SubMission mission = JsonConvert.DeserializeObject<SubMission>(output, JSONSaverMission);
                mission.Tree = tree;
                return mission;
            }
            catch (DirectoryNotFoundException e)
            {
                SMUtil.Assert(false, "Mission (Loading) ~ " + MissionName, "SubMissions: Check your Mission file names, cases where you referenced " +
                    "the names and make sure they match!!!  Tree: " +
                    tree.TreeName + ", Mission: " + MissionName, e);
                return null;
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }
        // Tech loading is handled elsewhere - either PopulationInjector or TACtical_AIs.

        /// <summary>
        /// Returns true if it should be added to the list
        /// </summary>
        /// <param name="SMT"></param>
        /// <param name="name"></param>
        /// <param name="hash"></param>
        /// <param name="GO"></param>
        /// <returns></returns>

        private static Dictionary<string, object> MakeCompat<T>(T convert)
        {
            IEnumerable<PropertyInfo> PI = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Debug_SMissions.Log("SubMissions: MakeCompat - Compiling " + typeof(T) + " which has " + PI.Count() + " properties");
            Dictionary<string, object> converted = new Dictionary<string, object>();
            foreach (PropertyInfo PIC in PI)
            {
                //if (FI.IsPublic)
                converted.Add(PIC.Name, PIC.GetValue(convert));
            }
            return converted;
        }

        /*
        public static void TreeLoaderLEGACY(string TreeName, string TreeDirectory, out SubMissionTree Tree)
        {
            try
            {
                string output = LoadMissionTreeTrunkFromFile(TreeName, TreeDirectory);
                Tree = JsonConvert.DeserializeObject<SubMissionTree>(output, new MissionTypeEnumConverter());
                //Tree.TreeHierachy = TreeDirectory;
                LoadMissionTreeDataFromFile(TreeName, TreeDirectory, ref Tree.MissionTextures,
                    ref Tree.MissionMeshes, ref Tree.TreeTechs);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new DirectoryNotFoundException("SubMissions: Check your Tree file names, " +
                    "cases where you referenced the names and make sure they match!!!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }


        // Loaders
        private static string LoadMissionTreeTrunkFromFile(string TreeName, string TreeDirectory)
        {
            ValidateDirectory(TreeDirectory);
            try
            {
                string output = File.ReadAllText(Path.Combine(TreeDirectory, "MissionTree.json"));

                Debug_SMissions.Log("SubMissions: Loaded MissionTree.json trunk for " + TreeName + " successfully.");
                return output;
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, "SubMissions: Could not edit MissionTree.json for " + TreeName + "." +
                    "\n - TerraTech + SubMissions was not permitted to access the MissionTree.json destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, "SubMissions: Could not edit MissionTree.json for " + TreeName + "." +
                    "\n - File MissionTree.json is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, "SubMissions: Could not edit MissionTree.json for " + TreeName + "." +
                    "\n - File MissionTree.json is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, "SubMissions: Could not edit MissionTree.json for " + TreeName + "." +
                    "\n - File MissionTree.json is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
            return null;
        }
        private static void LoadMissionTreeDataFromFile(string TreeName, string TreeDirectory,
            ref Dictionary<int, Texture> album, ref Dictionary<int, Mesh> models,
            ref Dictionary<string, SpawnableTech> techs)
        {
            ValidateDirectory(TreeDirectory);
            try
            {
                LoadTreePNGs(TreeName, TreeDirectory, ref album);
                LoadTreeMeshes(TreeName, TreeDirectory, ref models);
                LoadTreeTechs(TreeName, TreeDirectory, ref techs);

                Debug_SMissions.Log("SubMissions: Loaded MissionTree.json for " + TreeName + " successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, "SubMissions: Could not edit MissionTree.json for " + TreeName + "." +
                    "\n - TerraTech + SubMissions was not permitted to access the MissionTree.json destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, "SubMissions: Could not edit MissionTree.json for " + TreeName + "." +
                    "\n - File MissionTree.json is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, "SubMissions: Could not edit MissionTree.json for " + TreeName + "." +
                    "\n - File MissionTree.json is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Mission Tree (Loading) ~ " + TreeName, "SubMissions: Could not edit MissionTree.json for " + TreeName + "." +
                    "\n - File MissionTree.json is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }
       

        // ETC
        private static void LoadTreePNGs(string TreeName, string TreeDirectory, 
            ref Dictionary<int, Texture> dictionary)
        {
            dictionary.Clear();
            try
            {
                string[] outputs = Directory.GetFiles(TreeDirectory);
                bool foundAny = false;
                foreach (string str in outputs)
                {
                    if (GetName(str, out string name) && str.EndsWith(".png"))
                    {
                        try
                        {
                            dictionary.Add(name.GetHashCode(), FileUtils.LoadTexture(str));
                            foundAny = true;
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            SMUtil.Assert(false, "Mission Tree Texture (Loading) ~ " + name, "SubMissions: Could not load " + name + ".png for " + TreeName + "." +
                                "\n - TerraTech + SubMissions was not permitted to access the MissionTree.json destination", e);
                        }
                        catch (PathTooLongException e)
                        {
                            SMUtil.Assert(false, "Mission Tree Texture (Loading) ~ " + name, "SubMissions: Could not load " + name + ".png for " + TreeName + "." +
                                "\n - File MissionTree.json is located in a directory that makes it too deep and long" +
                                " for the OS to navigate correctly", e);
                        }
                        catch (DirectoryNotFoundException e)
                        {
                            SMUtil.Assert(false, "Mission Tree Texture (Loading) ~ " + name, "SubMissions: Could not load " + name + ".png for " + TreeName + "." +
                                "\n - Path to place Folder \"" + name + "\" is incomplete (there are missing folders in" +
                                " the target folder hierachy)", e);
                        }
                        catch (FileNotFoundException e)
                        {
                            SMUtil.Assert(false, "Mission Tree Texture (Loading) ~ " + name, "SubMissions: Could not load " + name + ".png for " + TreeName + "." +
                                "\n - File MissionTree.json is not at destination", e);
                        }
                        catch (IOException e)
                        {
                            SMUtil.Assert(false, "Mission Tree Textures (Loading) ~ " + TreeName, "SubMissions: Could not load " + name + ".png for " + TreeName + "." +
                                "\n - File MissionTree.json is not accessable because IOException(?) was thrown!", e);
                        }
                        catch (Exception e)
                        {
                            throw new MandatoryException("Encountered exception not properly handled", e);
                        }
                    }
                }
                if (foundAny)
                {
                    Debug_SMissions.Log("SubMissions: Loaded " + dictionary.Count + " PNG files for " + TreeName + " successfully.");
                }
                else
                    Debug_SMissions.Log("SubMissions: " + TreeName + " does not have any PNG files to load.");
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "Mission Tree Textures (Loading) ~ " + TreeName, "SubMissions: CASCADE FAILIURE ~ Could not load PNG files for " + TreeName + 
                    ".  \n   This could be due to a bug with this mod or file permissions.", e);
            }
        }
        private static void LoadTreeMeshes(string TreeName, string TreeDirectory, 
            ref Dictionary<int, Mesh> dictionary)
        {
            dictionary.Clear();
            try
            {
                string[] outputs = Directory.GetFiles(TreeDirectory);
                bool foundAny = false;
                foreach (string str in outputs)
                {
                    if (GetName(str, out string name) && str.EndsWith(".obj"))
                    {
                        dictionary.Add(name.GetHashCode(), LoadMesh(str));
                        foundAny = true;
                    }
                }
                if (foundAny)
                {
                    Debug_SMissions.Log("SubMissions: Loaded " + dictionary.Count + " .obj files for " + TreeName + " successfully.");
                }
                else
                    Debug_SMissions.Log("SubMissions: " + TreeName + " does not have any .obj files to load.");
            }
            catch (NotImplementedException e)
            {
                SMUtil.Assert(false, "Mission Tree Meshes (Loading) ~ " + TreeName, "SubMissions: Could not load .obj files for " + TreeName +
                    ".  \n   You need the mod \"LegacyBlockLoader\" to import non-AssetBundle models.", e);
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "Mission Tree Meshes (Loading) ~ " + TreeName, "SubMissions: Could not load .obj files for " + TreeName + 
                    ".  \n   This could be due to a bug with this mod or file permissions.", e);
            }
        }
        private static void LoadTreeTechs(string TreeName, string TreeDirectory, 
            ref Dictionary<string, SpawnableTech> dictionary)
        {
            dictionary.Clear();
            try
            {
                string[] outputs = Directory.GetFiles(TreeDirectory + "Techs");
                bool foundAny = false;
                foreach (string str in outputs)
                {
                    if (GetName(str, out string name) && str.EndsWith(".png"))
                    {
                        if (dictionary.ContainsKey(name))
                        {
                            SMUtil.Error(false, "Mission Tree SnapTechs (Loading) ~ " + TreeName + ", tech " + name,
                                "Tech of name " + name + " already is assigned to the tree.  Cannot add " +
                                "multiple Techs of same name!");
                        }
                        else
                            dictionary.Add(name, new SpawnableTechSnapshot(name));
                        foundAny = true;
                    }
                }
                outputs = Directory.GetFiles(TreeDirectory + "Raw Techs");
                foreach (string str in outputs)
                {
                    if (GetName(str, out string name) && (str.EndsWith(".json") || str.EndsWith(".RAWTECH")))
                    {
                        if (dictionary.ContainsKey(name))
                        {
                            SMUtil.Error(false, "Mission Tree RawTechs (Loading) ~ " + TreeName + ", tech " + name,
                                "Tech of name " + name + " already is assigned to the tree.  Cannot add " +
                                "multiple Techs of same name!");
                        }
                        else
                            dictionary.Add(name, new SpawnableTechRAW(name));
                        foundAny = true;
                    }
                }
                if (foundAny)
                {
                    Debug_SMissions.Log("SubMissions: Loaded " + dictionary.Count + " Techs for " + TreeName + " successfully.");
                }
                else
                    Debug_SMissions.Log("SubMissions: " + TreeName + " does not have any Techs to load.");
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "Mission Tree Techs (Loading) ~ " + TreeName, "SubMissions: Could not load Techs for " + TreeName +
                    ".  \n   This could be due to a bug with this mod or file permissions.", e);
            }
        }
        */

        internal static Mesh LoadMesh(string MeshDirectory)
        {
#if !STEAM
            Mesh mesh = ObjImporter.ImportFileFromPath(TreeDirectory);
            if (!mesh)
            {
                throw new NullReferenceException("The object could not be imported at all: ");
            }
            return mesh;
#else
            try
            {
                Mesh mesh = LoadMesh_Encapsulated(MeshDirectory);
                if (!mesh)
                {
                    throw new NullReferenceException("The object could not be imported at all");
                }
                return mesh;
            }
            catch
            {
                throw new NotImplementedException("SMissionJSONLoader.LoadMesh requires the mod " +
                    "\"LegacyBlockLoader\" to load models.");
            }
            //throw new NotImplementedException("SMissionJSONLoader.LoadMesh is not currently supported in Official.");

#endif
        }
        private static Mesh LoadMesh_Encapsulated(string MeshDirectory)
        {
            return LegacyBlockLoader.FastObjImporter.Instance.ImportFileFromPath(MeshDirectory);
        }



        // Savers
        public static void SaveTree(SubMissionTree tree)
        {
            string RawTreeJSON = JsonConvert.SerializeObject(tree, Formatting.Indented, JSONSaver);
            try
            {
                if (!DirectoryExists(Path.Combine(MissionsDirectory, tree.TreeName)))
                    Directory.CreateDirectory(Path.Combine(MissionsDirectory, tree.TreeName));
                File.WriteAllText(Path.Combine(MissionsDirectory, tree.TreeName, "MissionTree.json"), RawTreeJSON);

                string assetBundleable = JsonConvert.SerializeObject(tree.TreeHierachy.CreateAssetBundleable(), Formatting.Indented, JSONSaver);
                File.WriteAllText(Path.Combine(MissionsDirectory, tree.TreeName, tree.TreeName+"_Hierarchy.json"), assetBundleable);
                Debug_SMissions.Log("SubMissions: Saved MissionTree.json for " + tree.TreeName + " successfully.");
            }
            catch (UnauthorizedAccessException e)
            {
                SMUtil.Assert(false, "Export Mission Prefabs", "SubMissions: Could not edit MissionTree.json for " + tree.TreeName +
                    "\n - TerraTech + SubMissions was not permitted to access the MissionTree.json destination", e);
            }
            catch (PathTooLongException e)
            {
                SMUtil.Assert(false, "Export Mission Prefabs", "SubMissions: Could not edit MissionTree.json for " + tree.TreeName +
                    "\n - File MissionTree.json is located in a directory that makes it too deep and long" +
                    " for the OS to navigate correctly", e);
            }
            catch (FileNotFoundException e)
            {
                SMUtil.Assert(false, "Export Mission Prefabs", "SubMissions: Could not edit MissionTree.json for " + tree.TreeName +
                    "\n - File MissionTree.json is not at destination", e);
                //Debug_SMissions.Log(e);
            }
            catch (IOException e)
            {
                SMUtil.Assert(false, "Export Mission Prefabs", "SubMissions: Could not edit MissionTree.json for " + tree.TreeName +
                    "\n - File MissionTree.json is not accessable because IOException(?) was thrown!", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException("Encountered exception not properly handled", e);
            }
        }
        public static void SaveNewMission(SubMissionTree tree, SubMission mission)
        {
            if (!DirectoryExists(Path.Combine(MissionsDirectory, tree.TreeName, "Missions")))
                Directory.CreateDirectory(Path.Combine(MissionsDirectory, tree.TreeName, "Missions"));
            var missionJSON = JsonConvert.SerializeObject(mission, Formatting.Indented, JSONSaverMission);
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, tree.TreeName, "Missions", mission.Name), missionJSON);
            tree.ActiveMissions.Add(mission);
            ManSubMissions.Selected = mission;
        }
        public static void SaveNewTechSnapshot(SubMissionTree tree, string name, Texture2D Tech)
        {
            if (!DirectoryExists(Path.Combine(MissionsDirectory, tree.TreeName, "Techs")))
                Directory.CreateDirectory(Path.Combine(MissionsDirectory, tree.TreeName, "Techs"));
            FileUtils.SaveTexture(Tech, Path.Combine(MissionsDirectory, tree.TreeName, "Techs", name + ".png"));
            tree.TreeTechs.Add(name, new SpawnableTechSnapshot(name));
        }
        public static void SaveNewTechRaw(SubMissionTree tree, string name, string blueprint)
        {
            if (!DirectoryExists(Path.Combine(MissionsDirectory, tree.TreeName, "Raw Techs")))
                Directory.CreateDirectory(Path.Combine(MissionsDirectory, tree.TreeName, "Raw Techs"));
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, tree.TreeName, "Raw Techs", name), blueprint);
            tree.TreeTechs.Add(name, new SpawnableTechRAW(name));
        }
        internal static void SaveNewSMWorldObject(SubMissionTree tree, SMWorldObjectJSON SMWO)
        {
            if (!DirectoryExists(Path.Combine(MissionsDirectory, tree.TreeName, "Pieces")))
                Directory.CreateDirectory(Path.Combine(MissionsDirectory, tree.TreeName, "Pieces"));
            string SMWorldObjectJSON = JsonConvert.SerializeObject(SMWO, Formatting.Indented, JSONSafe);
            SMWO.Tree = tree.TreeName;
            TryWriteToJSONFile(Path.Combine(MissionsDirectory, tree.TreeName, "Pieces", SMWO.Name), SMWorldObjectJSON);
            var prefab = MM_JSONLoader.BuildNewWorldObjectPrefabJSON(tree, SMWO);
            tree.WorldObjects.Add(SMWO.Name, prefab);
        }
    }
}
