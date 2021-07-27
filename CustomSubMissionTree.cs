using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;
using Sub_Missions.Steps;
using Newtonsoft.Json;

namespace Sub_Missions
{
    [Serializable]
    public class CustomSubMissionTree
    {   //  Build the mission tree!
        public string TreeName = "unset";
        public string Faction = "GSO";

        [JsonIgnore]
        internal List<CustomSubMissionStandby> Missions = new List<CustomSubMissionStandby>();          //MUST BE SET VIA JSON

        [JsonIgnore]
        internal List<CustomSubMissionStandby> RepeatMissions = new List<CustomSubMissionStandby>();    //MUST BE SET VIA JSON

        public List<string> MissionNames = new List<string>();          //MUST BE SET VIA JSON
        public List<string> RepeatMissionNames = new List<string>();    //MUST BE SET VIA JSON

        internal List<CustomSubMission> ActiveMissions = new List<CustomSubMission>();// DO NOT SET!!! - saved in campaign

        internal byte ProgressX = 0; // DO NOT SET!!! - saved in campaign
        internal byte ProgressY = 0; // DO NOT SET!!! - saved in campaign


        private List<CustomSubMissionStandby> CompletedMissions = new List<CustomSubMissionStandby>();// DO NOT SET!!! - saved in campaign


        // Initialization
        public CustomSubMissionTree CompileMissionTree()
        {   // Reduce memory loads
            CustomSubMissionTree tree = SMissionJSONLoader.TreeLoader(TreeName);
            List<CustomSubMission> MissionsLoaded = SMissionJSONLoader.LoadAllMissions(TreeName, tree);
            List<CustomSubMissionStandby> compiled = CompileToStandby(MissionsLoaded);

            // Now we sort them based on input strings
            foreach (CustomSubMissionStandby sort in compiled)
            {
                sort.Tree = tree;
                bool repeat = tree.RepeatMissionNames.Contains(sort.Name);
                bool main = tree.MissionNames.Contains(sort.Name);
                if (repeat && main)
                {
                    Debug.Log("SubMissions: Tree " + TreeName + " contains mission " + sort.Name + " that's specified in both MissionNames or RepeatMissionNames.");
                    Debug.Log("SubMissions:   Make sure to assign it to MissionNames or RepeatMissionNames.");
                    Debug.Log("SubMissions:   Defaulting " + sort.Name + " to MissionNames.");
                    tree.Missions.Add(sort);
                }
                else if (repeat)
                {
                    Debug.Log("SubMissions: Mission " + sort.Name + " has been assigned to " + TreeName + " as a repeatable mission.");
                    tree.RepeatMissions.Add(sort);
                }
                else if (main)
                {
                    Debug.Log("SubMissions: Mission " + sort.Name + " has been assigned to " + TreeName + " as a main mission.");
                    tree.Missions.Add(sort);
                }
                else
                {
                    Debug.Log("SubMissions: Tree " + TreeName + " contains unspecified mission " + sort.Name + ".");
                    Debug.Log("SubMissions:   Make sure to assign it to MissionNames or RepeatMissionNames.");
                    Debug.Log("SubMissions:   Defaulting " + sort.Name + " to MissionNames.");
                    tree.Missions.Add(sort);
                }

            }
            Debug.Log("SubMissions: Compiled tree for " + TreeName + ".");
            return tree;
        }


        // Actions
        public void AcceptTreeMission(CustomSubMissionStandby Anon)
        {   //
            Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.AcceptMission);
            CustomSubMission newMission = DeployMission(TreeName, Anon);
            newMission.Startup();
            ActiveMissions.Add(newMission);
            ManSubMissions.inst.GetAllPossibleMissions();
            ManSubMissions.Selected = newMission;
        }
        public void CancelTreeMission(CustomSubMission Active)
        {   //
            if (ManSubMissions.Selected == Active)
            {
                if (ManSubMissions.ActiveSubMissions[0] != null)
                    ManSubMissions.Selected = ManSubMissions.ActiveSubMissions[0];
                else
                    ManSubMissions.Selected = null;
            }
            Active.Cleanup();
            if (!ActiveMissions.Remove(Active))
                Debug.Log("SubMissions: Called wrong tree [" + TreeName + "] for mission " + Active.Name + " on CancelTreeMission!");
            ManSubMissions.inst.GetAllPossibleMissions();
        }
        public List<CustomSubMissionStandby> GetReachableMissions()
        {   //
            //Debug.Log("SubMissions: " + TreeName + " is fetching missions");
            List<CustomSubMissionStandby> initMissions = new List<CustomSubMissionStandby>();
            Debug.Log("SubMissions: Missions count " + Missions.Count + " | " + RepeatMissions.Count);
            foreach (CustomSubMissionStandby mission in Missions)
            {
                //Debug.Log("SubMissions: Trying to validate mission " + mission.Name);
                if (ActiveMissions.Exists(delegate (CustomSubMission cand) { return cand.Name == mission.Name; }))
                {   // It's been finished already, do not get
                    Debug.Log("SubMissions: " + mission.Name + " is already active");
                    continue;
                }
                if (KickStart.OverrideRestrictions)
                {
                    Debug.Log("SubMissions: Presenting mission " + mission.Name);
                    initMissions.Add(mission);
                    continue;
                }
                if (CompletedMissions.Contains(mission))
                {   // It's been finished already, do not get
                    Debug.Log("SubMissions: " + mission.Name + " is already finished");
                    continue;
                }
                if (mission.MinProgressX > ProgressX)
                    continue;
                if (mission.MinProgressY > ProgressY)
                    continue;
                try
                {
                    FactionLicense licence = Singleton.Manager<ManLicenses>.inst.GetLicense((FactionSubTypes)Enum.Parse(typeof(FactionSubTypes), mission.Faction));
                    if (licence.IsDiscovered && licence.CurrentLevel >= mission.GradeRequired)
                    {
                        Debug.Log("SubMissions: Presenting mission " + mission.Name);
                        initMissions.Add(mission);
                    }
                }
                catch
                {
                    Debug.Log("SubMissions: " + mission.Name + " is not available right now");
                    continue;
                }
            }
            foreach (CustomSubMissionStandby mission in RepeatMissions)
            {
                //Debug.Log("SubMissions: Trying to validate mission " + mission.Name);
                if (ActiveMissions.Exists(delegate (CustomSubMission cand) { return cand.Name == mission.Name; }))
                {   // It's been finished already, do not get
                    Debug.Log("SubMissions: " + mission.Name + " is already active");
                    continue;
                }
                if (KickStart.OverrideRestrictions)
                {
                    Debug.Log("SubMissions: Presenting mission " + mission.Name);
                    initMissions.Add(mission);
                    continue;
                }
                if (mission.MinProgressX > ProgressX)
                    continue;
                if (mission.MinProgressY > ProgressY)
                    continue;
                try
                {
                    FactionLicense licence = Singleton.Manager<ManLicenses>.inst.GetLicense((FactionSubTypes)Enum.Parse(typeof(FactionSubTypes), mission.Faction));
                    if (licence.IsDiscovered && licence.CurrentLevel >= mission.GradeRequired)
                    {
                        Debug.Log("SubMissions: Presenting mission " + mission.Name);
                        initMissions.Add(mission);
                    }
                }
                catch
                {
                    Debug.Log("SubMissions: " + mission.Name + " is not available right now");
                    continue;
                }
            }
            return initMissions;
        }


        // COMPILER
        public List<CustomSubMission> DeployMissions(string treeName, List<CustomSubMissionStandby> toDeploy)
        {   // Because each mission takes up an unholy amount of memory, we want to 
            //   only load the entire thing when nesseary
            List<CustomSubMission> missionsLoaded = new List<CustomSubMission>();
            foreach (CustomSubMissionStandby mission in toDeploy)
            {
                missionsLoaded.Add(DeployMission(treeName, mission));
            }
            return missionsLoaded;
        }
        public CustomSubMission DeployMission(string treeName, CustomSubMissionStandby toDeploy)
        {   // Because each mission takes up an unholy amount of memory, we want to 
            //   only load the entire thing when nesseary
            return SMissionJSONLoader.MissionLoader(treeName, toDeploy.Name, this);
        }
        public static List<CustomSubMissionStandby> CompileToStandby(List<CustomSubMission> MissionsLoaded)
        {   // Reduce memory loads
            List<CustomSubMissionStandby> missions = new List<CustomSubMissionStandby>();
            foreach (CustomSubMission mission in MissionsLoaded)
            {
                missions.Add(CompileToStandby(mission));
            }
            return missions;
        }
        public static CustomSubMissionStandby CompileToStandby(CustomSubMission mission)
        {   // Reduce memory loads
            CustomSubMissionStandby missionCompiled = new CustomSubMissionStandby();
            missionCompiled.Tree = mission.Tree;
            missionCompiled.Name = mission.Name;
            missionCompiled.Desc = mission.Description;
            missionCompiled.GradeRequired = mission.GradeRequired;
            missionCompiled.Faction = mission.Faction;
            missionCompiled.Rewards = mission.Rewards;
            missionCompiled.Tree = mission.Tree;
            missionCompiled.MinProgressX = mission.MinProgressX;
            missionCompiled.MinProgressY = mission.MinProgressY;
            missionCompiled.LoadRadius = mission.GetMinimumLoadRange();
            return missionCompiled;
        }


        // Events
        public void FinishedMission(CustomSubMission finished)
        {
            ActiveMissions.Remove(finished);
            if (RepeatMissions.Find(delegate (CustomSubMissionStandby cand) { return cand.Name == finished.Name; }).Name == finished.Name)
            {   // Do nothing special - repeat missions are to be repeated
                return;
            }
            else if (Missions.Find(delegate (CustomSubMissionStandby cand) { return cand.Name == finished.Name; }).Name == finished.Name)
            {
                CompletedMissions.Add(CompileToStandby(finished));
            }
            else
                Debug.Log("SubMissions: Tried to finish mission " + finished.Name + " that's not listed in this tree!  Tree " + TreeName);
            if (ManSubMissions.Selected == finished)
            {
                if (ManSubMissions.ActiveSubMissions[0] != null)
                    ManSubMissions.Selected = ManSubMissions.ActiveSubMissions[0];
                else
                    ManSubMissions.Selected = null;
            }
            ManSubMissions.inst.GetAllPossibleMissions();
        }
        public void ResetALLTreeMissions()
        {
            foreach (CustomSubMission mission in ActiveMissions)
            {
                CancelTreeMission(mission);
            }
            ManSubMissions.inst.GetAllPossibleMissions();
        }
    }
    public class CustomSubMissionStandby 
    {
        public CustomSubMissionTree Tree;
        public string Name = "Unset";
        public string Desc = "Nothing";
        public string Faction = "";
        public int GradeRequired = 0;
        public byte MinProgressX = 0;
        public byte MinProgressY = 0;

        public float LoadRadius = 0;

        public SubMissionReward Rewards; //
    }
}
