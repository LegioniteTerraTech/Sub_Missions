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
        public static char up = '\\';

        public static readonly PhysicMaterial PM = new PhysicMaterial("MMPM") {
            dynamicFriction = 0.75f,
            bounciness = 0.1f,
            bounceCombine = PhysicMaterialCombine.Maximum,
            frictionCombine = PhysicMaterialCombine.Maximum,
            staticFriction = 0.75f,
        };
        public static readonly PhysicMaterial PMR = new PhysicMaterial("RUBR")
        {
            dynamicFriction = 0.75f,
            bounciness = 0.9f,
            bounceCombine = PhysicMaterialCombine.Maximum,
            frictionCombine = PhysicMaterialCombine.Maximum,
            staticFriction = 0.75f,
        };
        public static readonly PhysicMaterial PMI = new PhysicMaterial("ICE")
        {
            dynamicFriction = 0.1f,
            bounciness = 0.0f,
            bounceCombine = PhysicMaterialCombine.Average,
            frictionCombine = PhysicMaterialCombine.Average,
            staticFriction = 0.1f,
        };
        public static readonly PhysicMaterial PMN = new PhysicMaterial("NOFRICT")
        {
            dynamicFriction = 0.0f,
            bounciness = 0.0f,
            bounceCombine = PhysicMaterialCombine.Average,
            frictionCombine = PhysicMaterialCombine.Minimum,
            staticFriction = 0.0f,
        };

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
            if (Application.platform == RuntimePlatform.OSXPlayer)
                up = '/'; //Support OSX
            DirectoryInfo di = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
            di = di.Parent; // off of this DLL
            DLLDirectory = di.ToString();
            di = di.Parent; // out of the DLL folder
            di = di.Parent; // out of QMods
            BaseDirectory = di.ToString();
            MissionsDirectory = di.ToString() + up + "Custom SMissions";
            MissionSavesDirectory = di.ToString() + up + "SMissions Saves";
            MissionCorpsDirectory = di.ToString() + up + "SMissions Corps";
            ValidateDirectory(MissionsDirectory);
            ValidateDirectory(MissionSavesDirectory);
            ValidateDirectory(MissionCorpsDirectory);

            if (!ManSMCCorps.hasScanned)
            {
                ManSMCCorps.LoadAllCorps();
            }
#if DEBUG
            Debug.Log("SubMissions: DLL folder is at: " + DLLDirectory);
            Debug.Log("SubMissions: Custom SMissions is at: " + MissionsDirectory);
            //SMCCorpLicense.SaveTemplateToDisk();
#endif
        }

        // First Startup
        public static void MakePrefabMissionTreeToFile(string TreeName)
        {
            Debug.Log("SubMissions: Setting up template reference...");

            SubMission mission1 = MakePrefabMission1();
            SubMission mission2 = MakePrefabMission2();
            SubMission mission3 = MakePrefabMission3();
            SubMission mission4 = MakePrefabMission4();

            SMWorldObjectJSON SMWO = MakePrefabWorldObject();

            SubMissionTree tree = new SubMissionTree();
            tree.TreeName = "Template";
            tree.Faction = "GSO";
            tree.MissionNames.Add("NPC Mission");
            tree.MissionNames.Add("Harvest Mission");
            tree.MissionNames.Add("Water Blocks Aid");
            tree.RepeatMissionNames.Add("Combat Mission");
            tree.WorldObjectFileNames.Add("ModularBrickCube_(636)");

            string RawTreeJSON = JsonConvert.SerializeObject(tree, Formatting.Indented, JSONSaver);
            ValidateDirectory(MissionsDirectory);
            ValidateDirectory(MissionsDirectory + up + TreeName);
            ValidateDirectory(MissionsDirectory + up + TreeName + up + "Raw Techs");
            ValidateDirectory(MissionsDirectory + up + TreeName + up + "Missions");
            ValidateDirectory(MissionsDirectory + up + TreeName + up + "Pieces");
            string one = JsonConvert.SerializeObject(mission1, Formatting.Indented, JSONSaverMission);
            string two = JsonConvert.SerializeObject(mission2, Formatting.Indented, JSONSaverMission);
            string three = JsonConvert.SerializeObject(mission3, Formatting.Indented, JSONSaverMission);
            string four = "{\n\"Name\": \"Garrett Gruntle\",\n\"IsAnchored\": true,\n\"Blueprint\": \"{\\\"t\\\":\\\"GSOAnchorRotating_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSO_Chassis_Cab_314\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"r\\\":18}|{\\\"t\\\":\\\"GSOCockpit_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOGyroAllAxisActive_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":3.0,\\\"z\\\":0.0},\\\"r\\\":21}|{\\\"t\\\":\\\"GSOAIGuardController_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":4.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOMortarFixed_211\\\",\\\"p\\\":{\\\"x\\\":-1.0,\\\"y\\\":3.0,\\\"z\\\":1.0},\\\"r\\\":19}|{\\\"t\\\":\\\"GSO_Character_A_111\\\",\\\"p\\\":{\\\"x\\\":-2.0,\\\"y\\\":3.0,\\\"z\\\":0.0},\\\"r\\\":3}|{\\\"t\\\":\\\"GSO_Character_A_111\\\",\\\"p\\\":{\\\"x\\\":2.0,\\\"y\\\":3.0,\\\"z\\\":0.0},\\\"r\\\":1}|{\\\"t\\\":\\\"GSOBlockHalf_111\\\",\\\"p\\\":{\\\"x\\\":2.0,\\\"y\\\":2.0,\\\"z\\\":0.0},\\\"r\\\":22}|{\\\"t\\\":\\\"GSOBlockHalf_111\\\",\\\"p\\\":{\\\"x\\\":-2.0,\\\"y\\\":2.0,\\\"z\\\":0.0},\\\"r\\\":22}|{\\\"t\\\":\\\"GSOPlough_211\\\",\\\"p\\\":{\\\"x\\\":3.0,\\\"y\\\":2.0,\\\"z\\\":1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOPlough_211\\\",\\\"p\\\":{\\\"x\\\":-2.0,\\\"y\\\":2.0,\\\"z\\\":1.0},\\\"r\\\":12}|{\\\"t\\\":\\\"GSOBattery_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":3.0,\\\"z\\\":-1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOTractorMini_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0,\\\"z\\\":1.0},\\\"r\\\":16}|{\\\"t\\\":\\\"GSOMortarFixed_211\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":2.0,\\\"z\\\":1.0},\\\"r\\\":17}|{\\\"t\\\":\\\"GSOBlockLongHalf_211\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":2.0,\\\"z\\\":-1.0},\\\"r\\\":21}|{\\\"t\\\":\\\"GSOBlockLongHalf_211\\\",\\\"p\\\":{\\\"x\\\":-1.0,\\\"y\\\":3.0,\\\"z\\\":-1.0},\\\"r\\\":23}|{\\\"t\\\":\\\"VENBracketStraight_211\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":5.0,\\\"z\\\":-1.0},\\\"r\\\":8}|{\\\"t\\\":\\\"GSORadar_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":6.0,\\\"z\\\":-1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOLightSpot_111\\\",\\\"p\\\":{\\\"x\\\":-1.0,\\\"y\\\":4.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOLightFixed_111\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":4.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOBooster_112\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"r\\\":18}|{\\\"t\\\":\\\"GSOBooster_112\\\",\\\"p\\\":{\\\"x\\\":-1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"r\\\":18}|{\\\"t\\\":\\\"GSOBooster_112\\\",\\\"p\\\":{\\\"x\\\":-1.0,\\\"y\\\":1.0,\\\"z\\\":-1.0},\\\"r\\\":20}|{\\\"t\\\":\\\"GSOBooster_112\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":-1.0},\\\"r\\\":20}|{\\\"t\\\":\\\"GSOWheelHub_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOWheelHub_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":-1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOArmourPlateCab_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":2.0},\\\"r\\\":4}|{\\\"t\\\":\\\"GSOArmourPlateCab_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":-2.0},\\\"r\\\":6}|{\\\"t\\\":\\\"GSOBlockHalf_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOArmourPlateSmall_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":3.0,\\\"z\\\":1.0},\\\"r\\\":11}|{\\\"t\\\":\\\"GSOFuelTank_121\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0,\\\"z\\\":-1.0},\\\"r\\\":6}\",\n\"InfBlocks\": false,\n\"Faction\": 1,\n\"NonAggressive\": false,\n\"Cost\": 42828\n}";
            string five = "{\n\"Name\": \"TestTarget\",\n\"IsAnchored\": true,\n\"Blueprint\": \"{\\\"t\\\":\\\"GSOAnchorRotating_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOBlock_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOCockpit_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0,\\\"z\\\":0.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOPlough_311\\\",\\\"p\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":0.0},\\\"r\\\":17}|{\\\"t\\\":\\\"GSOPlough_311\\\",\\\"p\\\":{\\\"x\\\":-1.0,\\\"y\\\":1.0,\\\"z\\\":0.0},\\\"r\\\":19}|{\\\"t\\\":\\\"GSOArmourPlateSmall_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"r\\\":0}|{\\\"t\\\":\\\"GSOArmourPlateSmall_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0,\\\"z\\\":-1.0},\\\"r\\\":2}|{\\\"t\\\":\\\"GSOArmourPlateSmall_111\\\",\\\"p\\\":{\\\"x\\\":0.0,\\\"y\\\":3.0,\\\"z\\\":0.0},\\\"r\\\":11}\",\n\"InfBlocks\": false,\n\"Faction\": 1,\n\"NonAggressive\": false\n}";
            string six = JsonConvert.SerializeObject(mission4, Formatting.Indented, JSONSaverMission); 
            string seven = JsonConvert.SerializeObject(SMWO, Formatting.Indented, JSONSafe);
            TryWriteToJSONFile(MissionsDirectory + up + TreeName + up + "Missions" + up +  "NPC Mission", one);
            TryWriteToJSONFile(MissionsDirectory + up + TreeName + up + "Missions" + up +  "Combat Mission", two);
            TryWriteToJSONFile(MissionsDirectory + up + TreeName + up + "Missions" + up +  "Harvest Mission", three);
            TryWriteToJSONFile(MissionsDirectory + up + TreeName + up + "Raw Techs" + up +  "Garrett Gruntle", four);
            TryWriteToJSONFile(MissionsDirectory + up + TreeName + up + "Raw Techs" + up +  "TestTarget", five);
            TryWriteToJSONFile(MissionsDirectory + up + TreeName + up + "Missions" + up +  "Water Blocks Kit", six);
            TryWriteToJSONFile(MissionsDirectory + up + TreeName + up + "Pieces" + up + "ModularBrickCube_(636)", seven);
            TryWriteToJSONFile(MissionsDirectory + up + "SubMissionHelp", SubMission.GetDocumentation());
            TryWriteToJSONFile(MissionsDirectory + up + "SubMissionsSteps", SubMissionStep.GetALLStepDocumentations());
            TryCopyFile(DLLDirectory + up + "Garrett Gruntle.png", MissionsDirectory + up + TreeName + up + "Garrett Gruntle.png");
            TryCopyFile(DLLDirectory + up + "ModularBrickCube_6x3x6.obj", MissionsDirectory + up + TreeName + up + "ModularBrickCube_6x3x6.obj");
            try
            {
                File.WriteAllText(MissionsDirectory + up + TreeName + up + "MissionTree.json", RawTreeJSON);
                Debug.Log("SubMissions: Saved MissionTree.json for " + TreeName + " successfully.");
            }
            catch
            {
                Debug.Log("SubMissions: Could not edit MissionTree.json for " + TreeName + ".  \n   This could be due to a bug with this mod or file permissions.");
                return;
            }
            Debug.Log("SubMissions: Setup template reference successfully.");
        }
        public static SubMission MakePrefabMission1()
        {
            SubMission mission1 = new SubMission();
            mission1.Name = "NPC Mission";
            mission1.Description = "A complex showcase mission with an NPC involved";
            mission1.GradeRequired = 1;
            mission1.Faction = "GSO";
            mission1.Position = Vector3.forward * 40;
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
            };
            mission1.EventList = new List<SubMissionStep>
            {
                // Phase 1 - Getting the player to the character
                new SubMissionStep
                {
                    StepType = SMStepType.SetupTech,
                    ProgressID = 0,
                    Position = new Vector3(2,0,6), // needs specific location
                    InputNum = -2, // team
                    VaribleType = EVaribleType.None,
                    InputString = "Garrett Gruntle",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.SetupWaypoint,
                    ProgressID = SubMission.alwaysRunValue,// update For all time. Always.
                    Position = new Vector3(2,0,6),
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
                    Position = new Vector3(2,0,6), // needs specific location
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
                    Position = new Vector3(2,0,6), // needs specific location
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
                    Position = Vector3.up * 2f,//Vector3.forward * 6,
                    Forwards = Vector3.one * 2f,     // Changes the scale
                    InputString = "ModularBrickCube_(636)",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.SetupMM,
                    ProgressID = 0,
                    Position = -Vector3.forward * 9,//Vector3.forward * 6,
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
                    Position = new Vector3(2,0,6),
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
                BlocksToSpawn = new List<BlockTypes> { BlockTypes.GSOCockpit_111 }
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
            mission4.Position = Vector3.forward * 32;
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
                BlocksToSpawn = new List<BlockTypes>
                 {
                     (BlockTypes)584812,
                     (BlockTypes)584812,
                     (BlockTypes)584812,
                     (BlockTypes)584812,
                     (BlockTypes)584811,
                     (BlockTypes)584811
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


        // Majors
        public static List<SubMissionTree> LoadAllTrees()
        {
            Debug.Log("SubMissions: Searching Custom SMissions Folder...");
            List<string> names = GetNameList();
            Debug.Log("SubMissions: Found " + names.Count + " trees...");
            List<SubMissionTree> temps = new List<SubMissionTree>();
            foreach (string name in names)
            {
                if (TreeLoader(name, out SubMissionTree Tree))
                {
                    Debug.Log("SubMissions: Added Tree " + name);
                    temps.Add(Tree);
                }
                else
                    SMUtil.Assert(false, "Could not load mission tree " + name);

            }
            return temps;
        }
        public static List<SubMission> LoadAllMissions(string TreeName, SubMissionTree tree)
        {
            ValidateDirectory(MissionsDirectory);
            List<string> names = GetNameList(TreeName + up + "Missions", true);
            List<SubMission> temps = new List<SubMission>();
            foreach (string name in names)
            {
                var mission = MissionLoader(TreeName, name, tree);
                if (mission == null)
                {
                    SMUtil.Assert(false, "<b> CRITICAL ERROR IN HANDLING MISSION " + name + " - UNABLE TO IMPORT ANY INFORMATION! </b>");
                    continue;
                }
                temps.Add(mission);
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
                search = MissionsDirectory + up + directoryFromMissionsDirectory;
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
                if (ch == up)
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
            //Debug.Log("SubMissions: Cleaning Name " + output);
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
                Debug.Log("SubMissions: Generating " + name + " folder.");
                try
                {
                    Directory.CreateDirectory(DirectoryIn);
                    Debug.Log("SubMissions: Made new " + name + " folder successfully.");
                }
                catch
                {
                    SMUtil.Assert(false, "SubMissions: Could not create new " + name + " folder.  \n   This could be due to a bug with this mod or file permissions.");
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
                File.WriteAllText(FileDirectory + ".json", ToOverwrite);
                Debug.Log("SubMissions: Saved " + name + ".json successfully.");
            }
            catch
            {
                SMUtil.Assert(false, "SubMissions: Could not edit " + name + ".json.  \n   This could be due to a bug with this mod or file permissions.");
                return;
            }
        }
        public static void TryWriteToTextFile(string FileDirectory, string ToOverwrite)
        {
            if (!GetName(FileDirectory, out string name))
                return;// error
            try
            {
                File.WriteAllText(FileDirectory + ".txt", ToOverwrite);
                Debug.Log("SubMissions: Saved " + name + ".txt successfully.");
            }
            catch
            {
                SMUtil.Assert(false, "SubMissions: Could not edit " + name + ".json.  \n   This could be due to a bug with this mod or file permissions.");
                return;
            }
        }
        private static void TryCopyFile(string FileDirectory, string FileDirectoryEnd)
        {
            if (!GetName(FileDirectory, out string name))
                return;// error
            try
            {
                File.Copy(FileDirectory, FileDirectoryEnd);
                Debug.Log("SubMissions: Copied " + name + " successfully.");
            }
            catch //(Exception e)
            {
                SMUtil.Assert(false, "SubMissions: Could not copy " + name + ".  \n   The file is already there or it could be due to a bug with this mod or file permissions.");
                //Debug.Log(e);
                return;
            }
        }


        // JSON Handlers
        public static bool TreeLoader(string TreeName, out SubMissionTree Tree)
        {
            try
            {
                string output = LoadMissionTreeFromFile(TreeName, out Dictionary<int, Texture> album, out Dictionary<int, Mesh> models);
                Tree = JsonConvert.DeserializeObject<SubMissionTree>(output, new MissionTypeEnumConverter());
                Tree.MissionTextures = album;
                Tree.MissionMeshes = models;
                return true;
            }
            catch
            {
                SMUtil.Assert(false, "SubMissions: Check your Tree file names, cases where you referenced the names and make sure they match!!!");
                Tree = null;
                return false;
            }
        }
        public static SubMission MissionLoader(string TreeName, string MissionName, SubMissionTree tree)
        {
            try
            {
                string output = LoadMissionTreeMissionFromFile(TreeName, MissionName);
                SubMission mission = JsonConvert.DeserializeObject<SubMission>(output, JSONSaverMission);
                mission.Tree = tree;
                return mission;
            }
            catch
            {
                SMUtil.Assert(false, "SubMissions: Check your Mission file names, cases where you referenced the names and make sure they match!!!  Tree: " + TreeName + ", Mission: " + MissionName);
                return null;
            }
        }
        // Tech loading is handled elsewhere - either PopulationInjector or TACtical_AIs.


        // WorldObjects
        public static void BuildAllWorldObjects(List<SubMissionTree> SMTs)
        {
            try
            {
                ManModularMonuments.WorldObjects.Clear();
                Dictionary<int, GameObject> iGO = ManModularMonuments.WorldObjects;
                bool foundAny = false;
                foreach (SubMissionTree tree in SMTs)
                {
                    List<string> outputs = tree.WorldObjectFileNames;
                    foreach (string str in outputs)
                    {
                        if (BuildWorldObject(tree, str, out int hash, out GameObject GO))
                        {
                            iGO.Add(hash, GO);
                            foundAny = true;
                        }
                    }
                }
                if (foundAny)
                {
                    Debug.Log("SubMissions: BuildAllWorldObjects - Loaded " + iGO.Count + " WorldObj.json files from all trees successfully.");
                }
                else
                    Debug.Log("SubMissions: BuildAllWorldObjects - There were no WorldObj.json files to load.");
                ManModularMonuments.WorldObjects = iGO;
            }
            catch
            {
                SMUtil.Assert(false, "SubMissions: BuildAllWorldObjects - CRITICAL ERROR NOTIFY LEGIONITE");
            }
        }
        /// <summary>
        /// Returns true if it should be added to the list
        /// </summary>
        /// <param name="SMT"></param>
        /// <param name="name"></param>
        /// <param name="hash"></param>
        /// <param name="GO"></param>
        /// <returns></returns>
        private static bool BuildWorldObject(SubMissionTree SMT, string name, out int hash, out GameObject GO)
        {
            try
            {
                hash = name.GetHashCode();
                if (ManModularMonuments.WorldObjects.TryGetValue(hash, out GameObject GO2))
                {
                    GO = GO2;
                    return false;
                }
                else
                    GO = BuildNewWorldObject(SMT, name);
                return true;
            }
            catch
            {
                hash = 0;
                GO = null;
                SMUtil.Assert(false, "SubMissions: BuildWorldObject - Could not build for " + name + " of tree " + SMT.TreeName + "!");
            }
            return false;
        }

        public static GameObject BuildNewWorldObject(SubMissionTree SMT, string ObjectName)
        {
            try
            {
                string output = LoadMissionTreeWorldObjectFromFile(SMT.TreeName, ObjectName);
                GameObject gameObj = Instantiate(new GameObject("Unset"), null);
                SMWorldObject worldObj = gameObj.AddComponent<SMWorldObject>();
                worldObj.SetFromJSON(JsonConvert.DeserializeObject<SMWorldObjectJSON>(output));
                gameObj.name = worldObj.Name;
                var MRender = gameObj.AddComponent<MeshRenderer>();
                Material Mat = null;
                if (worldObj.GameMaterialName != null)
                {
                    Material[] mats = Resources.FindObjectsOfTypeAll<Material>();
                    mats = mats.Where(cases => cases.name == worldObj.GameMaterialName).ToArray();
                    if (mats != null && mats.Count() > 0)
                        Mat = mats.First();
                    if (!Mat)
                    {
                        SMUtil.Assert(false, "SubMissions: Check your WorldObject.json " + worldObj.Name + "'s GameMaterialName is not a valid material!  Tree: " + SMT.TreeName);
                        Mat = (Material)Resources.Load("GSO_Scenery");
                    }
                }
                else 
                    Mat = (Material)Resources.Load("GSO_Scenery");
                if (worldObj.TextureName != null && SMT.MissionTextures.TryGetValue(worldObj.TextureName.GetHashCode(), out Texture tex))
                {
                    Material newMat = new Material(Mat);
                    newMat.mainTexture = tex;
                    MRender.sharedMaterial = newMat;
                }
                else
                {
                    MRender.sharedMaterial = Mat;
                }
                var MFilter = gameObj.AddComponent<MeshFilter>();
                if (worldObj.VisualMeshName != null && SMT.MissionMeshes.TryGetValue(worldObj.VisualMeshName.GetHashCode(), out Mesh obj))
                    MFilter.sharedMesh = obj;
                else
                {
                    SMUtil.Assert(false, "SubMissions: Check your WorldObject.json " + worldObj.Name + " for a mesh!  It MUST have a valid VisualMeshName (with .obj included) provided in it's base SubMission folder!  Tree: " + SMT.TreeName);
                    worldObj.Remove(true);
                    return null;
                }
                if (worldObj.ColliderMeshName != null && SMT.MissionMeshes.TryGetValue(worldObj.ColliderMeshName.GetHashCode(), out Mesh obj2))
                {
                    var meshCol = gameObj.AddComponent<MeshCollider>();
                    meshCol.sharedMesh = obj2;
                }
                else
                {
                    var boxCol = gameObj.AddComponent<BoxCollider>();
                    boxCol.size = MFilter.sharedMesh.bounds.size;
                    boxCol.center = MFilter.sharedMesh.bounds.center;
                    //SMUtil.Assert(false, "SubMissions: Check your WorldObject.json " + worldObj.Name + " for a mesh!  It MUST have a valid ColliderMeshName (with .obj included) provided in it's base SubMission folder!  Tree: " + SMT.TreeName);
                    //return null;
                }
                Damageable dmg = gameObj.AddComponent<Damageable>();
                dmg.SetInvulnerable(true, true);
                if (worldObj.WorldObjectJSON != null)
                {
                    RecursiveGameObjectBuilder(SMT, gameObj, gameObj, worldObj.WorldObjectJSON);
                }
                int layerSet = Globals.inst.layerTerrain;
                if (worldObj.aboveGround) 
                    layerSet = Globals.inst.layerLandmark;  // no longer accepts anchors but does not mess with anchors.
                foreach (Collider Col in gameObj.GetComponentsInChildren<Collider>())
                {
                    Col.gameObject.layer = layerSet;
                    switch (worldObj.TerrainType)
                    {
                        case SMWOTerrain.Rubber:
                            Col.sharedMaterial = PMR;
                            break;
                        case SMWOTerrain.Ice:
                            Col.sharedMaterial = PMI;
                            break;
                        case SMWOTerrain.Frictionless:
                            Col.sharedMaterial = PMN;
                            break;
                        default:
                            Col.sharedMaterial = PM;
                            break;
                    }
                }
                gameObj.transform.position = Vector3.down * 50;
                gameObj.SetActive(false);
                return gameObj;
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "SubMissions: BuildNewWorldObject - Check your WorldObject.json for errors!  Tree: " + SMT.TreeName + "\n" + e);
                return null;
            }
        }

        private static int depth = 0;
        private const int MaxDepth = 10;
        private static void RecursiveGameObjectBuilder(SubMissionTree SMT, GameObject baseCase, GameObject parent, Dictionary<string, object> WorldObject)
        {
            depth++;
            try
            {
                foreach (KeyValuePair<string, object> entry in WorldObject)
                {
                    try
                    {
                        if (entry.Key.Where(delegate (char cCase) { return cCase != ' '; }).ToString().StartsWith("GameObject|"))
                        {
                            if (depth > MaxDepth)
                            {
                                SMUtil.Assert(false, "SubMissions: Error in " + baseCase.name + " WorldObjectJSON Tree: " + SMT.TreeName + "\n You have exceeded the maximum safe GameObject depth of " + MaxDepth + "!");
                                continue;
                            }
                            GameObject nextLevel = Instantiate(new GameObject(entry.Key.Skip(11).ToString()), parent.transform);

                            RecursiveGameObjectBuilder(SMT, baseCase, nextLevel, (Dictionary<string, object>)entry.Value);
                        }
                        else
                        {
                            try
                            {
                                Type type = Type.GetType(entry.Key);
                                if (type == null)
                                    continue;
                                if (!type.IsClass)
                                    continue;
                                var comp = parent.GetComponent(type.GetType());
                                if (!comp)
                                    comp = parent.AddComponent(type.GetType());

                                foreach (KeyValuePair<string, object> pair in (Dictionary<string, object>)entry.Value)
                                {
                                    PropertyInfo PI = type.GetType().GetProperty(pair.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                    if (PI != null)
                                    {
                                        PI.SetValue(comp, pair.Value);
                                    }
                                }
                            }
                            catch
                            {   // report missing component 
                                SMUtil.Assert(false, "SubMissions: Error in " + baseCase.name + " WorldObjectJSON Tree: " + SMT.TreeName + "\n No such variable exists: " + (entry.Key.NullOrEmpty() ? entry.Key : "ENTRY IS NULL OR EMPTY"));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        SMUtil.Assert(false, "SubMissions: Error in " + baseCase.name + " WorldObjectJSON Tree: " + SMT.TreeName + "\n" + e);
                    }
                }
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "SubMissions: Error in " + baseCase.name + " WorldObjectJSON Tree: " + SMT.TreeName + "\n GameObject case: " + e);
            }
            depth--;
        }

        private static Dictionary<string, object> MakeCompat<T>(T convert)
        {
            List<PropertyInfo> PI = convert.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
            Debug.Log("SubMissions: MakeCompat - Compiling " + convert.GetType() + " which has " + PI.Count() + " properties");
            Dictionary<string, object> converted = new Dictionary<string, object>();
            foreach (PropertyInfo PIC in PI)
            {
                //if (FI.IsPublic)
                    converted.Add(PIC.Name, PIC.GetValue(convert));
            }
            return converted;
        }



        // Loaders
        private static string LoadMissionTreeFromFile(string TreeName, out Dictionary<int, Texture> album, out Dictionary<int, Mesh> models)
        {
            album = null;
            models = null;
            string destination = MissionsDirectory + up + TreeName;
            ValidateDirectory(MissionsDirectory);
            ValidateDirectory(destination);
            try
            {
                string output = File.ReadAllText(destination + up + "MissionTree.json");
                album = LoadTreePNGs(TreeName, destination);
                models = LoadTreeMeshes(TreeName, destination);
                Debug.Log("SubMissions: Loaded MissionTree.json for " + TreeName + " successfully.");
                return output;
            }
            catch
            {
                SMUtil.Assert(false, "SubMissions: Could not read MissionTree.json for " + TreeName + ".  \n   This could be due to a bug with this mod or file permissions.");
                return null;
            }
        }
        private static string LoadMissionTreeMissionFromFile(string TreeName, string MissionName)
        {
            string destination = MissionsDirectory + up + TreeName + up + "Missions";

            ValidateDirectory(MissionsDirectory);
            ValidateDirectory(MissionsDirectory + up + TreeName);
            ValidateDirectory(destination);
            try
            {
                string output = File.ReadAllText(destination + up + MissionName + ".json");
                Debug.Log("SubMissions: Loaded Mission.json for " + MissionName + " successfully.");
                return output;
            }
            catch
            {
                SMUtil.Assert(false, "SubMissions: Could not read Mission.json for " + MissionName + ".  \n   This could be due to a bug with this mod or file permissions.");
                return null;
            }
        }
        private static string LoadMissionTreeWorldObjectFromFile(string TreeName, string ObjectName)
        {
            string destination = MissionsDirectory + up + TreeName + up + "Pieces";

            ValidateDirectory(MissionsDirectory);
            ValidateDirectory(MissionsDirectory + up + TreeName);
            ValidateDirectory(destination);
            try
            {
                string output = File.ReadAllText(destination + up + ObjectName + ".json");
                Debug.Log("SubMissions: Loaded WorldObject.json for " + ObjectName + " successfully.");
                return output;
            }
            catch
            {
                SMUtil.Assert(false, "SubMissions: Could not read WorldObject.json for " + ObjectName + ".  \n   This could be due to a bug with this mod or file permissions.");
                return null;
            }
        }



        // ETC
        private static Dictionary<int, Texture> LoadTreePNGs(string TreeName, string destination)
        {
            Dictionary<int, Texture> dictionary = new Dictionary<int, Texture>();
            try
            {
                string[] outputs = Directory.GetFiles(destination);
                bool foundAny = false;
                foreach (string str in outputs)
                {
                    if (GetName(str, out string name) && str.EndsWith(".png"))
                    {
                        dictionary.Add(name.GetHashCode(), FileUtils.LoadTexture(str));
                        foundAny = true;
                    }
                }
                if (foundAny)
                {
                    Debug.Log("SubMissions: Loaded " + dictionary.Count + " PNG files for " + TreeName + " successfully.");
                }
                else
                    Debug.Log("SubMissions: " + TreeName + " does not have any PNG files to load.");
                return dictionary;
            }
            catch
            {
                SMUtil.Assert(false, "SubMissions: Could not load PNG files for " + TreeName + ".  \n   This could be due to a bug with this mod or file permissions.");
                return null;
            }
        }
        private static Dictionary<int, Mesh> LoadTreeMeshes(string TreeName, string destination)
        {
            Dictionary<int, Mesh> dictionary = new Dictionary<int, Mesh>();
            try
            {
                string[] outputs = Directory.GetFiles(destination);
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
                    Debug.Log("SubMissions: Loaded " + dictionary.Count + " .obj files for " + TreeName + " successfully.");
                }
                else
                    Debug.Log("SubMissions: " + TreeName + " does not have any .obj files to load.");
                return dictionary;
            }
            catch
            {
                SMUtil.Assert(false, "SubMissions: Could not load .obj files for " + TreeName + ".  \n   This could be due to a bug with this mod or file permissions.");
                return null;
            }
        }
        internal static Mesh LoadMesh(string destination)
        {
            Mesh mesh = ObjImporter.ImportFileFromPath(destination);
            if (!mesh)
            {
                throw new NullReferenceException("The object could not be imported at all: ");
            }
            return mesh;
        }
    }
}
