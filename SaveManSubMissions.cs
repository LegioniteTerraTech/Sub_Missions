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
using Newtonsoft.Json;

namespace Sub_Missions
{
    public class SaveManSubMissions
    {
        private static JsonSerializerSettings JSONSaver = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        public static void LoadDataAutomatic()
        {
            try
            {
                string saveName = Singleton.Manager<ManSaveGame>.inst.GetCurrentSaveName(false);
                LoadData(saveName);
                //Debug.Log("SubMissions: SaveManSubMissions - LoadDataAutomatic: Loaded save " + saveName + " successfully");
            }
            catch
            {
                Debug.Log("SubMissions: SaveManSubMissions - LoadDataAutomatic: FAILIURE IN MAJOR OPERATION!");
            }
        }
        public static void SaveDataAutomatic()
        {
            try
            {
                string saveName = Singleton.Manager<ManSaveGame>.inst.GetCurrentSaveName(false);
                SaveData(saveName);
                //Debug.Log("SubMissions: SaveManSubMissions - SaveDataAutomatic: Saved save " + saveName + " successfully");
            }
            catch
            {
                Debug.Log("SubMissions: SaveManSubMissions - SaveDataAutomatic: FAILIURE IN MAJOR OPERATION!");
            }
        }


        private static void LoadData(string saveName)
        {
            string destination = SMissionJSONLoader.MissionSavesDirectory + "\\" + saveName;
            SMissionJSONLoader.ValidateDirectory(SMissionJSONLoader.MissionSavesDirectory);
            try
            {
                string output = File.ReadAllText(destination + ".JSON");
                try
                {
                    DeserializeToManager(output);
                    ManSubMissions.inst.GetAllPossibleMissions();
                }
                catch
                {
                    Debug.Log("SubMissions: Could not load contents of MissionSave.JSON for " + saveName + "!");
                    return;
                }
                Debug.Log("SubMissions: Loaded MissionSave.JSON for " + saveName + " successfully.");
                return;
            }
            catch
            {
                try
                {
                    File.WriteAllText(destination + ".JSON", SerializeFromManager(true));
                    Debug.Log("SubMissions: Created new MissionSave.JSON for " + saveName + " successfully.");
                    return;
                }
                catch
                {
                    Debug.Log("SubMissions: Could not read MissionSave.JSON for " + saveName + ".  \n   This could be due to a bug with this mod or file permissions.");
                    return;
                }
            }
        }
        private static void SaveData(string saveName)
        {
            Debug.Log("SubMissions: Setting up template reference...");
            string destination = SMissionJSONLoader.MissionSavesDirectory + "\\" + saveName;
            SMissionJSONLoader.ValidateDirectory(SMissionJSONLoader.MissionSavesDirectory);
            try
            {
                File.WriteAllText(destination + ".JSON", SerializeFromManager());
                Debug.Log("SubMissions: Saved MissionSave.JSON for " + saveName + " successfully.");
            }
            catch
            {
                Debug.Log("SubMissions: Could not save MissionSave.JSON for " + saveName + ".  \n   This could be due to a bug with this mod or file permissions.");
                return;
            }
        }


        private static string SerializeFromManager(bool defaultState = false)
        {
            return JsonConvert.SerializeObject(SaveToFileFormatting(defaultState), JSONSaver);
        }
        private static void DeserializeToManager(string ManSubMissionSave)
        {
            LoadFromFileFormatting((ManSubMissionSave)JsonConvert.DeserializeObject(ManSubMissionSave, JSONSaver));
        }


        // Formatting
        private static SubMissionSave SaveMissionState(SubMission mission)
        {
            List<SubMissionStepSave> stepsSaved = new List<SubMissionStepSave>();
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
                Faction = mission.Faction,

                TrackedTechs = mission.TrackedTechs,
                VarTrueFalse = mission.VarTrueFalse,
                VarInts = mission.VarInts,
            };
            return save;
        }
        private static void LoadMissionState(SubMission mission, SubMissionSave missionLoad)
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
                    treeSaved.ProgressID = StepFind.ProgressID;
                    treeSaved.SuccessProgressID = StepFind.SuccessProgressID;
                    treeSaved.StepType = StepFind.StepType;
                    treeSaved.VaribleType = StepFind.VaribleType;

                    treeSaved.SavedInt = StepFind.SavedInt;
                }
                else
                {
                    Debug.Log("SubMissions: SaveManSubMissions - Missing step " + treeSaved.StepType + ", ID " + treeSaved.ProgressID + " from save!  \n  The mission may handle strangely!");
                }
            }

            mission.TrackedTechs = missionLoad.TrackedTechs;
            mission.VarTrueFalse = missionLoad.VarTrueFalse;
            mission.VarInts = missionLoad.VarInts;
            return;
        }

        private static ManSubMissionSave SaveToFileFormatting(bool defaultState)
        {
            if (defaultState)
            {
                Debug.Log("SubMissions: Resetting ManSubMissions for new save instance...");
            }
            List<ManSubMissionTreeSave> treesSaved = new List<ManSubMissionTreeSave>();
            foreach (SubMissionTree missionTree in ManSubMissions.SubMissionTrees)
            {
                if (defaultState)
                {
                    missionTree.ResetALLTreeMissions();
                }
                ManSubMissionTreeSave treeSaved = new ManSubMissionTreeSave();
                treeSaved.TreeName = missionTree.TreeName;
                treeSaved.Faction = missionTree.Faction;

                List<SubMissionSave> actMissions = new List<SubMissionSave>();
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
            ManSubMissionSave save = new ManSubMissionSave {
                SubMissionTrees = treesSaved,

                SelectedIsAnon = ManSubMissions.SelectedIsAnon,

                Selected = ManSubMissions.ActiveSubMissions.IndexOf(ManSubMissions.Selected),
                SelectedAnon = ManSubMissions.AnonSubMissions.IndexOf(ManSubMissions.SelectedAnon),
            };
            return save;
        }
        private static void LoadFromFileFormatting(ManSubMissionSave save)
        {
            if (save.SubMissionTrees.Count > ManSubMissions.SubMissionTrees.Count)
            {
                Debug.Log("SubMissions: SaveManSubMissions - Tree counts are wrong! There are missing trees!");
            }

            foreach (SubMissionTree missionTree in ManSubMissions.SubMissionTrees)
            {
                if (save.SubMissionTrees.Exists(delegate (ManSubMissionTreeSave cand) { return cand.TreeName == missionTree.TreeName; }))
                {
                    ManSubMissionTreeSave treeSaved = save.SubMissionTrees.Find(delegate (ManSubMissionTreeSave cand) { return cand.TreeName == missionTree.TreeName; });

                    missionTree.TreeName = treeSaved.TreeName;
                    missionTree.Faction = treeSaved.Faction;

                    foreach (SubMission mission in missionTree.ActiveMissions)
                    {
                        var Loaded = treeSaved.ActiveMissions.Find(delegate (SubMissionSave cand) { return cand.Name == mission.Name && cand.Faction == mission.Faction; });
                        if (Loaded != null)
                        {
                            LoadMissionState(mission, Loaded);
                        }
                    }
                    missionTree.CompletedMissions = treeSaved.CompletedMissions;

                    missionTree.ProgressX = treeSaved.ProgressX;
                    missionTree.ProgressY = treeSaved.ProgressY;
                }
                else
                {
                    Debug.Log("SubMissions: SaveManSubMissions - Missing tree " + missionTree.TreeName + " from save!  If this tree is not returned then all data will be lost!");
                }

            }
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
    }


    // Save compiling - only need the relivant matters
    public class SubMissionStepSave
    {   // Grab some key details 
        public SMissionType StepType;           // The type this is

        public int ProgressID = 0;          // progress ID this runs on
        public int SuccessProgressID = 0;   // transfer to this when successful

        public EVaribleType VaribleType = EVaribleType.True;           // Selects what the output should be

        public int SavedInt = 0;
    }
    public class SubMissionSave
    {   //  Build the mission!
        public string Name = "Unset";
        public string Faction = "GSO";
        public Vector3 Position = Vector3.zero;

        internal int CurrentProgressID = 0;     // EXTREMELY IMPORTANT - determines the state the mission is at!

        public List<SubMissionStepSave> EventList;  

        public List<bool> VarTrueFalse = new List<bool>();   
        public List<int> VarInts = new List<int>();                 

        public List<TrackedTech> TrackedTechs;  
    }
    public class ManSubMissionTreeSave
    {   // Save ManSubMissions in a separate file since ManSaveGame does not have hooks for saving unofficial
        //   modded content
        public string TreeName = "unset";
        public string Faction = "GSO";

        internal List<SubMissionSave> ActiveMissions = new List<SubMissionSave>();// DO NOT SET!!! - saved in campaign

        internal byte ProgressX = 0; // DO NOT SET!!! - saved in campaign
        internal byte ProgressY = 0; // DO NOT SET!!! - saved in campaign

        internal List<SubMissionStandby> CompletedMissions = new List<SubMissionStandby>();

    }
    public class ManSubMissionSave
    {   // Save ManSubMissions in a separate file since ManSaveGame does not have hooks for saving unofficial
        //   modded content
        public bool SelectedIsAnon = false;

        public List<ManSubMissionTreeSave> SubMissionTrees = new List<ManSubMissionTreeSave>();

        public int Selected;
        public int SelectedAnon;
    }
}
