using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using UnityEngine;
using Sub_Missions.Steps;
using Newtonsoft.Json;

namespace Sub_Missions
{
    public class SMissionJSONLoader
    {
        public static string DLLDirectory;
        public static string BaseDirectory;
        public static string MissionsDirectory;

        private static JsonSerializerSettings JSONSaver = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };

        public static void SetupWorkingDirectories()
        {
            DirectoryInfo di = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
            di = di.Parent; // off of this DLL
            DLLDirectory = di.ToString();
            di = di.Parent; // out of the DLL folder
            di = di.Parent; // out of QMods
            BaseDirectory = di.ToString();
            MissionsDirectory = di.ToString() + "\\Custom SMissions";
            Debug.Log("SubMissions: DLL folder is at: " + DLLDirectory);
            Debug.Log("SubMissions: Custom SMissions is at: " + MissionsDirectory);
        }

        // First Startup
        public static void MakePrefabMissionTreeToFile(string TreeName)
        {
            Debug.Log("SubMissions: Setting up template reference...");
            CustomSubMission mission1 = new CustomSubMission();
            mission1.Name = "NPC Mission";
            mission1.Description = "A complex showcase mission with an NPC involved";
            mission1.GradeRequired = 1;
            mission1.Faction = "GSO";
            mission1.ClearTechsOnClear = false; // off for now to show the StepActRemove Step
            mission1.VarTrueFalse = new List<bool>
            {
                false,  // Range check
                false,  // Choice 1 "No"
                false,  // Choice 2 "Yes"
                true,   // False when Player drives away
            };
            mission1.VarInts = new List<int>
            {
                0,  // timer value
            };
            mission1.CheckList = new List<MissionChecklist>
            {
                new MissionChecklist
                {
                    ListArticle = "Meet Garrett ",
                    ValueType = VarType.Unset,
                    BoolToEnable = 3,
                },
            };
            mission1.EventList = new List<SubMissionStep>
            {
                // Phase 1 - Getting the player to the character
                new SubMissionStep
                {
                    StepType = SMissionType.StepSetupTech,
                    ProgressID = 0,
                    Position = new Vector3(2,0,6), // needs specific location
                    InputNum = -2, // team
                    VaribleType = EVaribleType.None,
                    InputString = "GSO Garrett"
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepActSpeak,
                    ProgressID = 0,
                    SuccessProgressID = 2,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "GSO Garrett",
                    InputStringAux = "Hello?   <b>Is this thing working!?</b> \n            <b>screEEEEeeeech!</b>\n Henlo there, and thank you for signing up into our Sub-Missions programme!",
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepActSpeak,
                    ProgressID = 2,
                    VaribleType = EVaribleType.None,
                    InputString = "GSO Garrett",
                    InputStringAux = "I've marked where I am on your minimap, come to me so that I can \n       <b>screEEEEeeeech!</b>\n -darn them cheap buggers-   IMEAN finish the briefing!",
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepCheckPlayerDist,
                    ProgressID = 1, // note that this still works even when the CurrentProgressID is 0 or 2 due to the adjecency rule
                    Position = new Vector3(2,0,6), // needs specific location
                    InputNum = 64, // distance before it gets true
                    VaribleType = EVaribleType.True,
                    SetGlobalIndex1 = 0, //enable
                    InputString = "",
                },  // this basically relays to the below
                new SubMissionStep
                {
                    StepType = SMissionType.StepActForward,
                    ProgressID = 1,
                    SuccessProgressID = 15,// Go to Phase 2
                    VaribleType = EVaribleType.True,
                    InputNum = 64, // distance before it gets true
                    SetGlobalIndex1 = 0,
                    InputString = "",
                },


                // Phase 2 - Conversation
                new SubMissionStep
                {
                    StepType = SMissionType.StepActMessagePurge,
                    ProgressID = 15,
                    VaribleType = EVaribleType.None,
                    InputNum = 1,// only fire once
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepActSpeak,
                    ProgressID = 15,
                    SuccessProgressID = 20,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "GSO Garrett",
                    InputStringAux = "Hey there prospector, so uhh we're testing the new sub-missions protocol. \n\n Say, it's a fine day today?",
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepActSpeak,
                    ProgressID = 20,
                    SuccessProgressID = 25,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "GSO Garrett",
                    InputStringAux = "I'll be assigning a series of fairly easy missions for you to try.",
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepActSpeak,
                    ProgressID = 25,
                    SuccessProgressID = 30,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "GSO Garrett",
                    InputStringAux = "Say, have you met Agent Pow?    \n      \n   Was she nice?",
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepActOptions,
                    ProgressID = 30,
                    VaribleType = EVaribleType.True,
                    InputString = "Met Agent Pow?", // title
                    InputStringAux = "Yes", // button label
                    SetGlobalIndex1 = 0, // value to check before throwing window (disabled on VaribleType: None)
                    SetGlobalIndex2 = 1, // "No" pressed - Go to Phase 3
                    SetGlobalIndex3 = 2, // "Other Option [Yes]" pressed - Go to Phase 3.5
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepCheckPlayerDist,
                    ProgressID = 30,
                    Position = new Vector3(2,0,6), // needs specific location
                    InputNum = 64, // distance before it gets true
                    VaribleType = EVaribleType.True,
                    SetGlobalIndex1 = 3, //ooop
                    InputString = "",
                },  // When this is triggered - Go to Phase 3.9


                // Phase 3 - Option "No"
                new SubMissionStep
                {
                    StepType = SMissionType.StepActForward,
                    ProgressID = 30,
                    SuccessProgressID = 35,
                    VaribleType = EVaribleType.True,
                    SetGlobalIndex1 = 1, // value to check before moving forward
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepActSpeak,
                    ProgressID = 35,
                    SuccessProgressID = 50, // Go to Phase 4
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "GSO Garrett",
                    InputStringAux = "Figures...    \n      \n   [I wonder what she thinks of Crafty Mike...] \n      [Wasn't Mike supposedly \"devilishly handsome?\"]",
                },

                // Phase 3.5 - Option "Yes"
                new SubMissionStep
                {
                    StepType = SMissionType.StepActForward,
                    ProgressID = 30,
                    SuccessProgressID = 40,
                    VaribleType = EVaribleType.True,
                    SetGlobalIndex1 = 2, // value to check before moving forward
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepActSpeak,
                    ProgressID = 40,
                    SuccessProgressID = 50,// Go to Phase 4
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "GSO Garrett",
                    InputStringAux = "Oh!  Maybe we CAN be friends after all!    \n      \n   ...",
                },

                // Phase 3.9 - Option "i don't care"
                new SubMissionStep
                {
                    StepType = SMissionType.StepActForward,
                    ProgressID = 30,
                    SuccessProgressID = 80,
                    VaribleType = EVaribleType.False,
                    SetGlobalIndex1 = 3, // value to check before moving forward
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepActMessagePurge,
                    ProgressID = 80,
                    VaribleType = EVaribleType.None,
                    InputNum = 1,// only fire once
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepActSpeak,
                    ProgressID = 80,
                    SuccessProgressID = 50,// Go to Phase 4
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "GSO Garrett",
                    InputStringAux = "Uhhh...    \n[You coming back?]\n   Figures...  \n          \n       I'll just send the missions over then...",
                },


                // Phase 4 - Letting the NPC leave the scene
                new SubMissionStep
                {
                    StepType = SMissionType.StepActSpeak,
                    ProgressID = 50,
                    SuccessProgressID = 55,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "GSO Garrett",
                    InputStringAux = "Brrrrt... \n\n  Looks like my time has vanished.  \n      Time to go then.",
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepActSpeak,
                    ProgressID = 55,
                    VaribleType = EVaribleType.None,
                    InputString = "GSO Garrett",
                    InputStringAux = "Farewell comrade, may we mee-~  \n       <b>screEEEEeeeech!</b>",
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepActBoost,
                    ProgressID = 55,
                    VaribleType = EVaribleType.None,
                    InputString = "GSO Garrett"
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepActTimer,
                    ProgressID = 55,
                    VaribleType = EVaribleType.None,// For All Time. Always.
                    SetGlobalIndex1 = 0,   // check this int (disabled)
                    SetGlobalIndex2 = 0,   // tick this int up
                    InputNum = 1,          // every second we increase by this value
                },


                // Phase 5 - Cleanup
                //   There's a toggle that allows the mission to clean up Techs automatically
                //     but in the case you want to keep some techs, this is the example.
                new SubMissionStep
                {
                    StepType = SMissionType.StepActForward,
                    ProgressID = 55,
                    SuccessProgressID = 60, //
                    VaribleType = EVaribleType.IntGreaterThan,
                    SetGlobalIndex1 = 0,    // Check this int - A timer in Phase 4 is pushing this along
                    InputNum = 7,          // Above this?  Then we go to Phase 5
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepActRemove,
                    ProgressID = 60,
                    VaribleType = EVaribleType.None,
                    InputString = "GSO Garrett"
                },  // The above fires first due to the way the updating works - top to bottom
                new SubMissionStep
                {
                    StepType = SMissionType.StepActWin,
                    ProgressID = 60,
                    VaribleType = EVaribleType.None,
                },
            };
            mission1.TrackedTechs = new List<TrackedTech>
            {
                new TrackedTech
                {
                     TechName = "GSO Garrett",
                }
            };
            mission1.Rewards = new SubMissionReward
            {
                EXPGain = 250,
                AddProgressX = 2,
            };

            CustomSubMission mission2 = new CustomSubMission();
            mission2.Name = "Combat Mission";
            mission2.Description = "A nice and simple combat target mission template";
            mission2.GradeRequired = 0;
            mission2.MinProgressX = 1;
            mission2.Faction = "GSO";
            mission2.ClearTechsOnClear = true;
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
                    StepType = SMissionType.StepActSpeak,
                    ProgressID = 0,
                    VaribleType = EVaribleType.True, // WARNING - Does only bools!
                    SetGlobalIndex1 = 0,    // Gets true, shows message
                    SetGlobalIndex2 = 1,    // sends true on close
                    InputString = "Some Random Feller From GSO",
                    InputStringAux = "Hey it's working now! \n\n    Feller, could you shoot that target over there?",
                },
                /*
                new SubMissionStep
                {
                    StepType = SMissionType.StepActShifter,
                    ProgressID = 0,
                    VaribleType = EVaribleType.True,
                    SetGlobalIndex1 = 0,            // activates on true
                    RevProgressIDOffset = false,
                },*/
                new SubMissionStep
                {
                    StepType = SMissionType.StepSetupTech,
                    ProgressID = 0,
                    InputNum = 8,
                    VaribleType = EVaribleType.None,
                    InputString = "TestTarget",
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepCheckDestroy,
                    ProgressID = 0,
                    SuccessProgressID = 30,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "TestTarget",
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepActWin,
                    ProgressID = 30,
                    VaribleType = EVaribleType.None,
                },
            };
            mission2.TrackedTechs = new List<TrackedTech>
            {
                new TrackedTech
                {
                     TechName = "TestTarget",
                }
            };
            mission2.Rewards = new SubMissionReward
            {
                MoneyGain = 950,
                EXPGain = 100,
                BlocksToSpawn = new List<BlockTypes> { BlockTypes.GSOAIGuardController_111, BlockTypes.GSOBlock_511 }
            };


            CustomSubMission mission3 = new CustomSubMission();
            mission3.Name = "Harvest Mission";
            mission3.Description = "A showcase mission with harvesting involved";
            mission3.GradeRequired = 1;
            mission3.MinProgressX = 1;
            mission3.Faction = "GSO";
            mission3.ClearTechsOnClear = true;
            mission3.VarTrueFalse = new List<bool>
            {
                //Simple mission using only ProgressIDs
            };
            mission3.VarInts = new List<int>
            {
                (int)ChunkTypes.Wood, // the chunktype to harvest
                0,  // count of chunks
                30, // aimed count
            };
            mission3.CheckList = new List<MissionChecklist>
            {
                new MissionChecklist
                {
                    ListArticle = "Mine F*bron ",
                    ValueType = VarType.Unset,
                    GlobalIndex = 1,    // numberator
                    GlobalIndex2 = 2,    // denominator
                }
            };
            mission3.EventList = new List<SubMissionStep>
            {
                // Phase 1 - Getting the player to mine the resources
                new SubMissionStep
                {
                    StepType = SMissionType.StepActSpeak,
                    ProgressID = 0,
                    VaribleType = EVaribleType.None,
                    InputString = "GSO Garrett",
                    InputStringAux = "Hey there prospector, so uhh we're testing the new sub-missions protocol. \n Feller, could you chop down some trees?",
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepCheckResources,
                    ProgressID = 0,
                    SuccessProgressID = 30, //
                    VaribleType = EVaribleType.DoSuccessID,
                    InputNum = 30,          // mine 30
                    SetGlobalIndex1 = 0,    // resource type
                    SetGlobalIndex2 = 1,    // resource count mined
                    SetGlobalIndex3 = 0,    // output
                },

                // Phase 2 - Letting the NPC leave the scene
                new SubMissionStep
                {
                    StepType = SMissionType.StepActSpeak,
                    ProgressID = 30,
                    SuccessProgressID = 35, //
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "GSO Garrett",
                    InputStringAux = "Alright, that should do it.            \n  You can keep the chunks.    \n    No it's fine.",
                },
                new SubMissionStep
                {
                    StepType = SMissionType.StepActSpeak,
                    ProgressID = 35,
                    SuccessProgressID = 45, //
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "GSO Garrett",
                    InputStringAux = "Farewell comrade, may we mee-~   \n          \n       <b>screEEEEeeeech!</b>",
                },

                // Phase 2 - Cleanup
                new SubMissionStep
                {
                    StepType = SMissionType.StepActWin,
                    ProgressID = 45,
                    VaribleType = EVaribleType.None,
                },
            };
            mission3.Rewards = new SubMissionReward
            {
                MoneyGain = 500,
                EXPGain = 100,
            };

            CustomSubMissionTree tree = new CustomSubMissionTree();
            tree.TreeName = "Template";
            tree.Faction = "GSO";
            tree.MissionNames.Add("NPC Mission");
            tree.MissionNames.Add("Harvest Mission");
            tree.RepeatMissionNames.Add("Combat Mission");

            string RawTreeJSON = JsonConvert.SerializeObject(tree, Formatting.Indented, JSONSaver);
            ValidateDirectory(MissionsDirectory);
            ValidateDirectory(MissionsDirectory + "\\" + TreeName);
            ValidateDirectory(MissionsDirectory + "\\" + TreeName + "\\Raw Techs");
            ValidateDirectory(MissionsDirectory + "\\" + TreeName + "\\Missions");
            string one = JsonConvert.SerializeObject(mission1, Formatting.Indented, JSONSaver);
            string two = JsonConvert.SerializeObject(mission2, Formatting.Indented, JSONSaver);
            string three = JsonConvert.SerializeObject(mission3, Formatting.Indented, JSONSaver);
            string four = "{\n\"Name\": \"GSO Garrett\",\n\"Blueprint\": \"{\\\"t\\\":\\\"GSOAnchorRotating_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSO_Chassis_Cab_314\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"r\\\":18}|{\\\"t\\\":\\\"GSOCockpit_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOGyroAllAxisActive_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":3.0,\\\"z\\\":0.0},\\\"r\\\":21}|{\\\"t\\\":\\\"GSOAIGuardController_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":4.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOMortarFixed_211\\\",\\\"p\\\":{\\\"x\\\":-1.0,\\\"y\\\":3.0,\\\"z\\\":1.0},\\\"r\\\":19}|{\\\"t\\\":\\\"GSO_Character_A_111\\\",\\\"p\\\":{\\\"x\\\":-2.0,\\\"y\\\":3.0,\\\"z\\\":0.0},\\\"r\\\":3}|{\\\"t\\\":\\\"GSO_Character_A_111\\\",\\\"p\\\":{\\\"x\\\":2.0,\\\"y\\\":3.0,\\\"z\\\":0.0},\\\"r\\\":1}|{\\\"t\\\":\\\"GSOBlockHalf_111\\\",\\\"p\\\":{\\\"x\\\":2.0,\\\"y\\\":2.0,\\\"z\\\":0.0},\\\"r\\\":22}|{\\\"t\\\":\\\"GSOBlockHalf_111\\\",\\\"p\\\":{\\\"x\\\":-2.0,\\\"y\\\":2.0,\\\"z\\\":0.0},\\\"r\\\":22}|{\\\"t\\\":\\\"GSOPlough_211\\\",\\\"p\\\":{\\\"x\\\":3.0,\\\"y\\\":2.0,\\\"z\\\":1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOPlough_211\\\",\\\"p\\\":{\\\"x\\\":-2.0,\\\"y\\\":2.0,\\\"z\\\":1.0},\\\"r\\\":12}|{\\\"t\\\":\\\"GSOBattery_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":3.0,\\\"z\\\":-1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOTractorMini_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0,\\\"z\\\":1.0},\\\"r\\\":16}|{\\\"t\\\":\\\"GSOMortarFixed_211\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":2.0,\\\"z\\\":1.0},\\\"r\\\":17}|{\\\"t\\\":\\\"GSOBlockLongHalf_211\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":2.0,\\\"z\\\":-1.0},\\\"r\\\":21}|{\\\"t\\\":\\\"GSOBlockLongHalf_211\\\",\\\"p\\\":{\\\"x\\\":-1.0,\\\"y\\\":3.0,\\\"z\\\":-1.0},\\\"r\\\":23}|{\\\"t\\\":\\\"VENBracketStraight_211\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":5.0,\\\"z\\\":-1.0},\\\"r\\\":8}|{\\\"t\\\":\\\"GSORadar_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":6.0,\\\"z\\\":-1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOLightSpot_111\\\",\\\"p\\\":{\\\"x\\\":-1.0,\\\"y\\\":4.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOLightFixed_111\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":4.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOBooster_112\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"r\\\":18}|{\\\"t\\\":\\\"GSOBooster_112\\\",\\\"p\\\":{\\\"x\\\":-1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"r\\\":18}|{\\\"t\\\":\\\"GSOBooster_112\\\",\\\"p\\\":{\\\"x\\\":-1.0,\\\"y\\\":1.0,\\\"z\\\":-1.0},\\\"r\\\":20}|{\\\"t\\\":\\\"GSOBooster_112\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":-1.0},\\\"r\\\":20}|{\\\"t\\\":\\\"GSOWheelHub_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOWheelHub_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":-1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOArmourPlateCab_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":2.0},\\\"r\\\":4}|{\\\"t\\\":\\\"GSOArmourPlateCab_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":-2.0},\\\"r\\\":6}|{\\\"t\\\":\\\"GSOBlockHalf_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOArmourPlateSmall_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":3.0,\\\"z\\\":1.0},\\\"r\\\":11}|{\\\"t\\\":\\\"GSOFuelTank_121\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0,\\\"z\\\":-1.0},\\\"r\\\":6}\",\n\"InfBlocks\": false,\n\"Faction\": 1,\n\"NonAggressive\": false,\n\"Cost\": 42828\n};";
            string five = "{\n\"Name\": \"TestTarget\",\n\"Blueprint\": \"{\\\"t\\\":\\\"GSOAnchorRotating_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOBlock_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOCockpit_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOPlough_311\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":0.0},\\\"r\\\":17}|{\\\"t\\\":\\\"GSOPlough_311\\\",\\\"p\\\":{\\\"x\\\":-1.0,\\\"y\\\":1.0,\\\"z\\\":0.0},\\\"r\\\":19}|{\\\"t\\\":\\\"GSOArmourPlateSmall_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOArmourPlateSmall_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0,\\\"z\\\":-1.0},\\\"r\\\":2}|{\\\"t\\\":\\\"GSOArmourPlateSmall_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":3.0,\\\"z\\\":0.0},\\\"r\\\":11}\",\n\"InfBlocks\": false,\n\"Faction\": 1,\n\"NonAggressive\": false\n};";
            TryWriteToJSONFile(MissionsDirectory + "\\" + TreeName + "\\Missions\\NPC Mission", one);
            TryWriteToJSONFile(MissionsDirectory + "\\" + TreeName + "\\Missions\\Combat Mission", two);
            TryWriteToJSONFile(MissionsDirectory + "\\" + TreeName + "\\Missions\\Harvest Mission", three);
            TryWriteToJSONFile(MissionsDirectory + "\\" + TreeName + "\\Raw Techs\\GSO Garrett", four);
            TryWriteToJSONFile(MissionsDirectory + "\\" + TreeName + "\\Raw Techs\\TestTarget", five);
            try
            {
                File.WriteAllText(MissionsDirectory + "\\" + TreeName + "\\MissionTree.JSON", RawTreeJSON);
                Debug.Log("SubMissions: Saved MissionTree.JSON for " + TreeName + " successfully.");
            }
            catch
            {
                Debug.Log("SubMissions: Could not edit MissionTree.JSON for " + TreeName + ".  \n   This could be due to a bug with this mod or file permissions.");
                return;
            }
            Debug.Log("SubMissions: Setup template reference successfully.");
        }

        // Majors
        public static List<CustomSubMissionTree> LoadAllTrees()
        {
            Debug.Log("SubMissions: Searching Custom SMissions Folder...");
            List<string> names = GetNameList();
            Debug.Log("SubMissions: Found " + names.Count + " trees...");
            List<CustomSubMissionTree> temps = new List<CustomSubMissionTree>();
            foreach (string name in names)
            {
                Debug.Log("SubMissions: Added Tree " + name);
                temps.Add(TreeLoader(name));
            }
            return temps;
        }
        public static List<CustomSubMission> LoadAllMissions(string TreeName, CustomSubMissionTree tree)
        {
            ValidateDirectory(MissionsDirectory);
            List<string> names = GetNameList(TreeName + "\\Missions", true);
            List<CustomSubMission> temps = new List<CustomSubMission>();
            foreach (string name in names)
            {
                temps.Add(MissionLoader(TreeName, name, tree));
            }
            return temps;
        }


        // Utilities
        public static List<string> GetNameList(string directoryFromMissionsDirectory = "", bool doJSON = false)
        {
            string search;
            if (directoryFromMissionsDirectory == "")
                search = MissionsDirectory;
            else
                search = MissionsDirectory + "\\" + directoryFromMissionsDirectory;
            List<string> toClean;
            if (doJSON)
                toClean = Directory.GetFiles(search).ToList();
            else
                toClean = Directory.GetDirectories(search).ToList();
            //Debug.Log("SubMissions: Cleaning " + toClean.Count);
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
                if (ch == '\\')
                {
                    final.Clear();
                }
                else
                    final.Append(ch);
            }
            if (doJSON)
            {
                if (!final.ToString().Contains(".JSON"))
                {
                    output = "error";
                    return false;
                }
                final.Remove(final.Length - 5, 5);// remove ".JSON"
            }
            output = final.ToString();
            //Debug.Log("SubMissions: Cleaning Name " + output);
            return true;
        }
        public static void ValidateDirectory(string DirectoryIn)
        {
            if (!GetName(DirectoryIn, out string name))
                return;// error
            if (!Directory.Exists(DirectoryIn))
            {
                Debug.Log("SubMissions: Generating " + name + " folder.");
                try
                {
                    Directory.CreateDirectory(DirectoryIn);
                    Debug.Log("SubMissions: Made new " + name + " folder successfully.");
                }
                catch
                {
                    Debug.Log("SubMissions: Could not create new " + name + " folder.  \n   This could be due to a bug with this mod or file permissions.");
                    return;
                }
            }
        }
        public static void TryWriteToJSONFile(string FileDirectory, string ToOverwrite)
        {
            if (!GetName(FileDirectory, out string name))
                return;// error
            try
            {
                File.WriteAllText(FileDirectory + ".JSON", ToOverwrite);
                Debug.Log("SubMissions: Saved " + name + ".JSON successfully.");
            }
            catch
            {
                Debug.Log("SubMissions: Could not edit " + name + ".JSON.  \n   This could be due to a bug with this mod or file permissions.");
                return;
            }
        }


        // JSON Handlers
        public static CustomSubMissionTree TreeLoader(string TreeName)
        {
            string output = LoadMissionTreeFromFile(TreeName);
            return JsonConvert.DeserializeObject<CustomSubMissionTree>(output, JSONSaver);
        }
        public static CustomSubMission MissionLoader(string TreeName, string MissionName, CustomSubMissionTree tree)
        {
            string output = LoadMissionTreeMissionFromFile(TreeName, MissionName);
            CustomSubMission mission = JsonConvert.DeserializeObject<CustomSubMission>(output, JSONSaver);
            mission.Tree = tree;
            return mission;
        }
        // Tech loading is handled elsewhere


        // Loaders
        private static string LoadMissionTreeFromFile(string TreeName)
        {
            string destination = MissionsDirectory + "\\" + TreeName;
            ValidateDirectory(MissionsDirectory);
            ValidateDirectory(destination);
            try
            {
                string output = File.ReadAllText(destination + "\\MissionTree.JSON");
                Debug.Log("SubMissions: Loaded MissionTree.JSON for " + TreeName + " successfully.");
                return output;
            }
            catch
            {
                Debug.Log("SubMissions: Could not read MissionTree.JSON for " + TreeName + ".  \n   This could be due to a bug with this mod or file permissions.");
                return null;
            }
        }
        private static string LoadMissionTreeMissionFromFile(string TreeName, string MissionName)
        {
            string destination = MissionsDirectory + "\\" + TreeName + "\\Missions\\";

            ValidateDirectory(MissionsDirectory);
            ValidateDirectory(MissionsDirectory + "\\" + TreeName);
            ValidateDirectory(destination);
            try
            {
                string output = File.ReadAllText(destination + "\\" + MissionName + ".JSON");
                Debug.Log("SubMissions: Loaded Mission.JSON for " + MissionName + " successfully.");
                return output;
            }
            catch
            {
                Debug.Log("SubMissions: Could not read Mission.JSON for " + MissionName + ".  \n   This could be due to a bug with this mod or file permissions.");
                return null;
            }
        }
    }
}
