using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using TAC_AI.Templates;
using Sub_Missions.ManWindows;
using Sub_Missions.Steps;
using Ionic.Zlib;
using Newtonsoft.Json;

namespace Sub_Missions
{
    public class SaveManSubMissions
    {
#if DEBUG
        private static bool UseCompressor = true;
#else
        private static bool UseCompressor = true;
#endif

        private static JsonSerializerSettings JSONSaver = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            MaxDepth = 10,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        public static void LoadDataAutomatic()
        {
            try
            {
                string saveName = Singleton.Manager<ManSaveGame>.inst.GetCurrentSaveName(false);
                LoadData(saveName);
                //Debug_SMissions.Log("SubMissions: SaveManSubMissions - LoadDataAutomatic: Loaded save " + saveName + " successfully");
            }
            catch
            {
                SMUtil.Assert(false, "SubMissions: SaveManSubMissions - LoadDataAutomatic: FAILIURE IN MAJOR OPERATION!");
            }
        }
        public static void SaveDataAutomatic()
        {
            try
            {
                string saveName = Singleton.Manager<ManSaveGame>.inst.GetCurrentSaveName(false);
                SaveData(saveName);
                //Debug_SMissions.Log("SubMissions: SaveManSubMissions - SaveDataAutomatic: Saved save " + saveName + " successfully");
            }
            catch
            {
                SMUtil.Assert(false, "SubMissions: SaveManSubMissions - SaveDataAutomatic: FAILIURE IN MAJOR OPERATION!");
            }
        }


        public static void LoadData(string saveName)
        {
            string destination = SMissionJSONLoader.MissionSavesDirectory + SMissionJSONLoader.up + saveName;
            SMissionJSONLoader.ValidateDirectory(SMissionJSONLoader.MissionSavesDirectory);
            try
            {
                try
                {
                    if (UseCompressor)
                    {
                        if (File.Exists(destination + ".SMSAV"))
                        {
                            using (FileStream FS = File.Open(destination + ".SMSAV", FileMode.Open, FileAccess.Read))
                            {
                                using (GZipStream GZS = new GZipStream(FS, CompressionMode.Decompress))
                                {
                                    using (StreamReader SR = new StreamReader(GZS))
                                    {
                                        DeserializeToManager(SR.ReadToEnd());
                                    }
                                }
                            }
                            ManSubMissions.ReSyncSubMissions();
                            Debug_SMissions.Log("SubMissions: Loaded MissionSave.SMSAV for " + saveName + " successfully.");
                        }
                        else if (File.Exists(destination + ".json"))
                        {
                            string output = "";
                            output = File.ReadAllText(destination + ".json");

                            DeserializeToManager(output);
                            ManSubMissions.ReSyncSubMissions();
                            Debug_SMissions.Log("SubMissions: Loaded MissionSave.json for " + saveName + " successfully.");
                        }
                    }
                    else
                    {
                        if (File.Exists(destination + ".json"))
                        {
                            string output = "";
                            output = File.ReadAllText(destination + ".json");

                            DeserializeToManager(output);
                            ManSubMissions.ReSyncSubMissions();
                            Debug_SMissions.Log("SubMissions: Loaded MissionSave.json for " + saveName + " successfully.");
                        }
                        else if (File.Exists(destination + ".SMSAV"))
                        {
                            using (FileStream FS = File.Open(destination + ".SMSAV", FileMode.Open, FileAccess.Read))
                            {
                                using (GZipStream GZS = new GZipStream(FS, CompressionMode.Decompress))
                                {
                                    using (StreamReader SR = new StreamReader(GZS))
                                    {
                                        DeserializeToManager(SR.ReadToEnd());
                                    }
                                }
                            }
                            ManSubMissions.ReSyncSubMissions();
                            Debug_SMissions.Log("SubMissions: Loaded MissionSave.SMSAV for " + saveName + " successfully.");
                        }
                    }
                }
                catch (Exception e)
                {
                    SMUtil.Assert(false, "SubMissions: Could not load contents of MissionSave.json/.SMSAV for " + saveName + "!");
                    Debug_SMissions.Log(e);
                    return;
                }
                return;
            }
            catch
            {
                try
                {
                    File.WriteAllText(destination + ".json", SerializeFromManager(true));
                    Debug_SMissions.Log("SubMissions: Created new MissionSave.json for " + saveName + " successfully.");
                    return;
                }
                catch
                {
                    Debug_SMissions.Log("SubMissions: Could not read MissionSave.json for " + saveName + ".  \n   This could be due to a bug with this mod or file permissions.");
                    return;
                }
            }
        }
        public static void SaveData(string saveName)
        {
            Debug_SMissions.Log("SubMissions: Setting up template reference...");
            string destination = SMissionJSONLoader.MissionSavesDirectory + SMissionJSONLoader.up + saveName;
            SMissionJSONLoader.ValidateDirectory(SMissionJSONLoader.MissionSavesDirectory);
            try
            {
                if (UseCompressor)
                {
                    using (FileStream FS = File.Create(destination + ".SMSAV"))
                    {
                        using (GZipStream GZS = new GZipStream(FS, CompressionMode.Compress))
                        {
                            using (StreamWriter SW = new StreamWriter(GZS))
                            {
                                SW.WriteLine(SerializeFromManager());
                                SW.Flush();
                            }
                        }
                    }
                    CleanUpCache();
                    Debug_SMissions.Log("SubMissions: Saved MissionSave.SMSAV for " + saveName + " successfully.");
                }
                else
                {
                    File.WriteAllText(destination + ".json", SerializeFromManager());
                    CleanUpCache();
                    Debug_SMissions.Log("SubMissions: Saved MissionSave.json for " + saveName + " successfully.");
                }
            }
            catch
            {
                SMUtil.Assert(false, "SubMissions: Could not save MissionSave.json/.SMSAV for " + saveName + ".  \n   This could be due to a bug with this mod or file permissions.");
                return;
            }
        }


        private static string SerializeFromManager(bool defaultState = false)
        {
            return JsonConvert.SerializeObject(SaveToFileFormatting(defaultState), KickStart.Debugger ? Formatting.Indented : Formatting.None, JSONSaver);
        }
        private static void DeserializeToManager(string ManSubMissionSaveIn)
        {
            LoadFromFileFormatting(JsonConvert.DeserializeObject<ManSubMissionSave>(ManSubMissionSaveIn, JSONSaver));
        }
        private static void CleanUpCache()
        {
            stepsSaved = new List<SubMissionStepSave>();
            actMissions = new List<SubMissionSave>();
            treesSaved = new List<ManSubMissionTreeSave>();
        }


        // Formatting
        private static List<SubMissionSave> actMissions = new List<SubMissionSave>();
        private static List<ManSubMissionTreeSave> treesSaved = new List<ManSubMissionTreeSave>();
        private static ManSubMissionSave SaveToFileFormatting(bool defaultState)
        {
            if (defaultState)
            {
                Debug_SMissions.Log("SubMissions: Resetting ManSubMissions for new save instance...");
            }

            treesSaved = new List<ManSubMissionTreeSave>();
            foreach (SubMissionTree missionTree in ManSubMissions.SubMissionTrees)
            {
                if (defaultState)
                {
                    missionTree.ResetALLTreeMissions();
                }
                ManSubMissionTreeSave treeSaved = new ManSubMissionTreeSave();
                treeSaved.TreeName = missionTree.TreeName;
                treeSaved.Faction = missionTree.Faction;

                actMissions = new List<SubMissionSave>();
                foreach (SubMission missionCase in missionTree.ActiveMissions)
                {
                    actMissions.Add(SaveMissionState(missionCase));
                }
                treeSaved.ActiveMissions = actMissions;

                treeSaved.CompletedMissions = missionTree.CompletedMissions;

                treeSaved.ProgressX = missionTree.ProgressX;
                treeSaved.ProgressY = missionTree.ProgressY;

                treesSaved.Add(treeSaved);
            }
            ManSubMissionSave save = new ManSubMissionSave
            {
                SubMissionTrees = treesSaved,

                ModularMonuments = ManModularMonuments.SaveAll(),

                SelectedIsAnon = ManSubMissions.SelectedIsAnon,

                Selected = ManSubMissions.ActiveSubMissions.IndexOf(ManSubMissions.Selected),
                SelectedAnon = ManSubMissions.AnonSubMissions.IndexOf(ManSubMissions.SelectedAnon),
                SavedModLicences = ManSubMissions.SavedModLicences,
            };
            return save;
        }
        private static void LoadFromFileFormatting(ManSubMissionSave save)
        {
            if (save == null)
            {
                Debug_SMissions.Log("SubMissions: SaveManSubMissions - Save is corrupted!");
                return;
            }
            if (save.ModularMonuments != null)
                ManModularMonuments.LoadAll(save.ModularMonuments);
            if (save.SubMissionTrees.Count > ManSubMissions.SubMissionTrees.Count)
            {
                Debug_SMissions.Log("SubMissions: SaveManSubMissions - Tree counts are wrong! There are missing trees!");
            }

            bool hasMissingItems = false;
            StringBuilder missingTrees = new StringBuilder();
            StringBuilder missingMissions = new StringBuilder();
            missingTrees.Append("Missing Trees: ");
            missingMissions.Append("Missing Missions: ");
            foreach (ManSubMissionTreeSave treeSaved in save.SubMissionTrees)
            {
                if (ManSubMissions.SubMissionTrees.Exists(delegate (SubMissionTree cand) { return cand.TreeName == treeSaved.TreeName; }))
                {
                    SubMissionTree missionTree = ManSubMissions.SubMissionTrees.Find(delegate (SubMissionTree cand) { return cand.TreeName == treeSaved.TreeName; });

                    missionTree.TreeName = treeSaved.TreeName;
                    missionTree.Faction = treeSaved.Faction;

                    missionTree.ActiveMissions.Clear();
                    List<SubMission> reloadMissions = new List<SubMission>();
                    foreach (SubMissionSave mission in treeSaved.ActiveMissions)
                    {
                        //var Loaded = missionTree.ActiveMissions.Find(delegate (SubMission cand) { return cand.Name == mission.Name && cand.Faction == mission.Faction; });
                        //if (Loaded != null)
                        //{
                        //    LoadMissionState(ref Loaded, mission);
                        //}
                        //else
                        //{
                            SubMission add = SMissionJSONLoader.MissionLoader(missionTree, mission.Name);
                            if (add == null)
                            {
                                missingMissions.Append(treeSaved.TreeName + ":" + mission.Name);
                                hasMissingItems = true;
                                continue;// load failure
                            }
                            LoadMissionState(ref add, mission);
                            reloadMissions.Add(add);
                        //}
                    }
                    missionTree.ActiveMissions = reloadMissions;
                    missionTree.CompletedMissions = treeSaved.CompletedMissions;

                    missionTree.ProgressX = treeSaved.ProgressX;
                    missionTree.ProgressY = treeSaved.ProgressY;
                }
                else
                {
                    Debug_SMissions.Log("SubMissions: SaveManSubMissions - Missing tree " + treeSaved.TreeName + " from save!  If this tree is not returned then all data will be lost!");
                    missingTrees.Append(treeSaved.TreeName + " - ");
                    hasMissingItems = true;
                }

            }
            if (hasMissingItems)
            {
                ManSubMissions.IgnoreSaveThisSession = true;
                WindowManager.AddPopupMessage("<b>Missing Sub Missions!</b>", missingTrees.ToString() + "\n" + missingMissions.ToString() + "\n Please add back in these Custom SMissions to prevent loss of Sub Missions save data!");
                WindowManager.ShowPopup(new Vector2(0.5f, 0.5f));
                WindowManager.AddPopupButtonDual("<b>Save Anyways?</b>", "Yes", true, "SaveThisGameAnyways");
                WindowManager.ShowPopup(new Vector2(0.5f, 1));
            }

            if (save.SavedModLicences == null)
                ManSubMissions.SavedModLicences = new List<KeyValuePair<string, KeyValuePair<int, int>>>();
            else
                ManSubMissions.SavedModLicences = save.SavedModLicences;
            ManSubMissions.SelectedIsAnon = save.SelectedIsAnon;
            SubMission chek1 = ManSubMissions.ActiveSubMissions.ElementAtOrDefault(save.Selected);
            if (chek1 != default(SubMission))
                ManSubMissions.Selected = chek1;
            else
                ManSubMissions.Selected = null;
            SubMissionStandby chek2 = ManSubMissions.AnonSubMissions.ElementAtOrDefault(save.SelectedAnon);
            if (chek2 != default(SubMissionStandby))
                ManSubMissions.SelectedAnon = chek2;
            else
                ManSubMissions.SelectedAnon = null;
        }

        // mission formatting
        private static List<SubMissionStepSave> stepsSaved;
        private static SubMissionSave SaveMissionState(SubMission mission)
        {
            stepsSaved = new List<SubMissionStepSave>();
            foreach (SubMissionStep missionStep in mission.EventList)
            {
                SubMissionStepSave treeSaved = new SubMissionStepSave();

                treeSaved.ProgressID = missionStep.ProgressID;
                treeSaved.SuccessProgressID = missionStep.SuccessProgressID;
                treeSaved.StepType = missionStep.StepType;
                treeSaved.VaribleType = missionStep.VaribleType;

                treeSaved.SavedInt = missionStep.SavedInt;

                stepsSaved.Add(treeSaved);
            }
            SubMissionSave save = new SubMissionSave
            {
                EventList = stepsSaved,

                Name = mission.Name,
                SelectedAltName = mission.SelectedAltName,
                SelectedAltDesc = mission.Description,
                Faction = mission.Faction,

                TilePos = mission.TilePos,
                OffsetFromTile = mission.OffsetFromTile,
                NeedsFirstInit = mission.ActiveState == SubMissionLoadState.NeedsFirstInit,

                CurrentProgressID = mission.CurrentProgressID,

                TrackedTechs = mission.TrackedTechs,
                VarTrueFalse = mission.VarTrueFalse,
                VarInts = mission.VarInts,
            };
            return save;
        }
        private static void LoadMissionState(ref SubMission mission, SubMissionSave missionLoad)
        {
            foreach (SubMissionStep treeSaved in mission.EventList)
            {
                var StepFind = missionLoad.EventList.Find(delegate (SubMissionStepSave cand)
                {
                    return cand.ProgressID == treeSaved.ProgressID &&
                           cand.StepType == treeSaved.StepType &&
                           cand.SuccessProgressID == treeSaved.SuccessProgressID &&
                           cand.VaribleType == treeSaved.VaribleType;
                });
                if (StepFind != null)
                {
                    /*
                    treeSaved.ProgressID = StepFind.ProgressID;
                    treeSaved.SuccessProgressID = StepFind.SuccessProgressID;
                    treeSaved.StepType = StepFind.StepType;
                    treeSaved.VaribleType = StepFind.VaribleType;
                    */
                    treeSaved.SavedInt = StepFind.SavedInt;
                }
                else
                {
                    Debug_SMissions.Log("SubMissions: SaveManSubMissions - Missing step " + treeSaved.StepType + ", ID " + treeSaved.ProgressID + " from save!  \n  The mission may handle strangely as it was changed!");
                }
            }
            mission.SelectedAltName = missionLoad.SelectedAltName;
            mission.Description = missionLoad.SelectedAltDesc;

            mission.WorldPos = new WorldPosition(missionLoad.TilePos, missionLoad.OffsetFromTile);
            mission.ActiveState = missionLoad.NeedsFirstInit ? SubMissionLoadState.NeedsFirstInit : SubMissionLoadState.PositionSetReady;

            mission.CurrentProgressID = missionLoad.CurrentProgressID;

            mission.TrackedTechs = missionLoad.TrackedTechs;
            mission.VarTrueFalse = missionLoad.VarTrueFalse;
            mission.VarInts = missionLoad.VarInts;
            EncounterShoehorn.SetFakeEncounter(mission);
            return;
        }
    }


    // Save compiling - only need the relivant matters
    public class SubMissionStepSave
    {   // Grab some key details 
        public SMStepType StepType = SMStepType.ActSpeak;           // The type this is

        public int ProgressID = 0;          // progress ID this runs on
        public int SuccessProgressID = 0;   // transfer to this when successful

        public int SavedInt = 0;
        public EVaribleType VaribleType = EVaribleType.True;           // Selects what the output should be
    }
    public class SubMissionSave
    {   //  Build the mission!
        public string Name = "Unset";
        public string SelectedAltName;
        public string SelectedAltDesc;
        public string Faction = "GSO";

        public IntVector2 TilePos = Vector2.zero;
        public Vector3 OffsetFromTile = Vector3.zero;
        public bool NeedsFirstInit = false;

        public int CurrentProgressID = 0;     // EXTREMELY IMPORTANT - determines the state the mission is at!

        public List<bool> VarTrueFalse = new List<bool>();   
        public List<int> VarInts = new List<int>();                 

        public List<TrackedTech> TrackedTechs;

        public List<SubMissionStepSave> EventList;
    }
    public class ManSubMissionTreeSave
    {   // Save ManSubMissions in a separate file since ManSaveGame does not have hooks for saving unofficial
        //   modded content
        public string TreeName = "unset";
        public string Faction = "GSO";

        public sbyte ProgressX = 0; // DO NOT SET!!! - saved in campaign
        public sbyte ProgressY = 0; // DO NOT SET!!! - saved in campaign

        public List<SubMissionStandby> CompletedMissions = new List<SubMissionStandby>();

        public List<SubMissionSave> ActiveMissions = new List<SubMissionSave>();// DO NOT SET!!! - saved in campaign
    }
    public class ManSubMissionSave
    {   // Save ManSubMissions in a separate file since ManSaveGame does not have hooks for saving unofficial
        //   modded content
        public bool SelectedIsAnon = false;
        public int Selected;
        public int SelectedAnon;

        /// <summary>
        /// Licence Short Name, Grade, EXP
        /// </summary>
        public List<KeyValuePair<string, KeyValuePair<int, int>>> SavedModLicences = new List<KeyValuePair<string, KeyValuePair<int, int>>>();

        public List<ManSubMissionTreeSave> SubMissionTrees = new List<ManSubMissionTreeSave>();

        public List<ModularMonumentSave> ModularMonuments = new List<ModularMonumentSave>();
    }
}
