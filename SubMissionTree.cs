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
    // Nuts are stored in the tree
    //   In other words - all the important information is stored in these trees, ManSubMissions just acts as a forging method
    [Serializable]
    public class SubMissionTree
    {   //  Build the mission tree!
        public string TreeName = "unset";
        public string Faction = "GSO";

        // Cache
        [JsonIgnore]
        internal List<SubMissionStandby> Missions = new List<SubMissionStandby>();          //MUST BE SET VIA JSON
        [JsonIgnore]
        internal List<SubMissionStandby> RepeatMissions = new List<SubMissionStandby>();    //MUST BE SET VIA JSON

        // JSON string linking
        public List<string> MissionNames = new List<string>();          //MUST BE SET VIA JSON
        public List<string> RepeatMissionNames = new List<string>();    //MUST BE SET VIA JSON

        public string ProgressXName = "Prestiege";
        public string ProgressYName = "Status";

        // Campaign Progression
        [JsonIgnore]
        internal List<SubMission> ActiveMissions = new List<SubMission>();// DO NOT SET!!! - saved in campaign
        [JsonIgnore]
        internal sbyte ProgressX = 0; // DO NOT SET!!! - saved in campaign
        [JsonIgnore]
        internal sbyte ProgressY = 0; // DO NOT SET!!! - saved in campaign
        [JsonIgnore]
        internal List<SubMissionStandby> CompletedMissions = new List<SubMissionStandby>();// DO NOT SET!!! - saved in campaign


        // Initialization
        public SubMissionTree CompileMissionTree()
        {   // Reduce memory loads
            SubMissionTree tree = SMissionJSONLoader.TreeLoader(TreeName);
            List<SubMission> MissionsLoaded = SMissionJSONLoader.LoadAllMissions(TreeName, tree);
            List<SubMissionStandby> compiled = CompileToStandby(MissionsLoaded);

            // Now we sort them based on input strings
            foreach (SubMissionStandby sort in compiled)
            {
                sort.Tree = tree;
                bool repeat = tree.RepeatMissionNames.Contains(sort.Name);
                bool main = tree.MissionNames.Contains(sort.Name);
                if (repeat && main)
                {
                    SMUtil.Assert(false, "SubMissions: Tree " + TreeName + " contains mission " + sort.Name + " that's specified in both MissionNames or RepeatMissionNames.");
                    SMUtil.Assert(false, "  Make sure to assign it to MissionNames or RepeatMissionNames.");
                    SMUtil.Assert(false, "  Defaulting " + sort.Name + " to MissionNames.");
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
                    SMUtil.Assert(false, "SubMissions: Tree " + TreeName + " contains unspecified mission " + sort.Name + ".");
                    SMUtil.Assert(false, "  Make sure to assign it to MissionNames or RepeatMissionNames.");
                    SMUtil.Assert(false, "  Defaulting " + sort.Name + " to MissionNames.");
                    tree.Missions.Add(sort);
                }

            }
            Debug.Log("SubMissions: Compiled tree for " + TreeName + ".");
            return tree;
        }


        // Actions
        public void AcceptTreeMission(SubMissionStandby Anon)
        {   //
            Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.AcceptMission);
            SubMission newMission = DeployMission(TreeName, Anon);
            newMission.Startup();
            ActiveMissions.Add(newMission);
            ManSubMissions.inst.GetAllPossibleMissions();
            ManSubMissions.Selected = newMission;
        }
        public void CancelTreeMission(SubMission Active)
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
        public List<SubMissionStandby> GetReachableMissions()
        {   //
            //Debug.Log("SubMissions: " + TreeName + " is fetching missions");
            List<SubMissionStandby> initMissions = new List<SubMissionStandby>();
            Debug.Log("SubMissions: Missions count " + Missions.Count + " | " + RepeatMissions.Count);
            foreach (SubMissionStandby mission in Missions)
            {
                //Debug.Log("SubMissions: Trying to validate mission " + mission.Name);
                if (ActiveMissions.Exists(delegate (SubMission cand) { return cand.Name == mission.Name; }))
                {   // It's already in the list
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
                {
                    Debug.Log("SubMissions: " + mission.Name + " - not enough " + mission.Tree.ProgressXName + ".");
                    continue;
                }
                if (mission.MinProgressY > ProgressY)
                {
                    Debug.Log("SubMissions: " + mission.Name + " - not enough " + mission.Tree.ProgressYName + ".");
                    continue;
                }
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
            foreach (SubMissionStandby mission in RepeatMissions)
            {
                //Debug.Log("SubMissions: Trying to validate mission " + mission.Name);
                if (ActiveMissions.Exists(delegate (SubMission cand) { return cand.Name == mission.Name; }))
                {   // It's already in the list
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
                {
                    Debug.Log("SubMissions: " + mission.Name + " - not enough " + mission.Tree.ProgressXName + ".");
                    continue;
                }
                if (mission.MinProgressY > ProgressY)
                {
                    Debug.Log("SubMissions: " + mission.Name + " - not enough " + mission.Tree.ProgressYName + ".");
                    continue;
                }
                try
                {
                    FactionLicense licence = Singleton.Manager<ManLicenses>.inst.GetLicense((FactionSubTypes)Enum.Parse(typeof(FactionSubTypes), mission.Faction));
                    if (licence.IsDiscovered && licence.CurrentLevel >= mission.GradeRequired)
                    {
                        mission.GetAndSetDisplayName();
                        Debug.Log("SubMissions: Presenting mission " + mission.AltName);
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
        public List<SubMission> DeployMissions(string treeName, List<SubMissionStandby> toDeploy)
        {   // Because each mission takes up an unholy amount of memory, we want to 
            //   only load the entire thing when nesseary
            List<SubMission> missionsLoaded = new List<SubMission>();
            foreach (SubMissionStandby mission in toDeploy)
            {
                missionsLoaded.Add(DeployMission(treeName, mission));
            }
            return missionsLoaded;
        }
        public SubMission DeployMission(string treeName, SubMissionStandby toDeploy)
        {   // Because each mission takes up an unholy amount of memory, we want to 
            //   only load the entire thing when nesseary
            SubMission mission = SMissionJSONLoader.MissionLoader(treeName, toDeploy.Name, this);
            mission.SelectedAltName = toDeploy.AltName;
            return mission;
        }
        public static List<SubMissionStandby> CompileToStandby(List<SubMission> MissionsLoaded)
        {   // Reduce memory loads
            List<SubMissionStandby> missions = new List<SubMissionStandby>();
            foreach (SubMission mission in MissionsLoaded)
            {
                missions.Add(CompileToStandby(mission));
            }
            return missions;
        }
        public static SubMissionStandby CompileToStandby(SubMission mission)
        {   // Reduce memory loads
            SubMissionStandby missionCompiled = new SubMissionStandby();
            missionCompiled.Tree = mission.Tree;
            missionCompiled.Name = mission.Name;
            missionCompiled.AltName = mission.SelectedAltName;
            missionCompiled.AltNames = mission.AltNames;
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
        public void FinishedMission(SubMission finished)
        {
            ActiveMissions.Remove(finished);
            if (RepeatMissions.Find(delegate (SubMissionStandby cand) { return cand.Name == finished.Name; }).Name == finished.Name)
            {   // Do nothing special - repeat missions are to be repeated
                return;
            }
            else if (Missions.Find(delegate (SubMissionStandby cand) { return cand.Name == finished.Name; }).Name == finished.Name)
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
            int CountStep = ActiveMissions.Count();
            for (int step = 0; step < CountStep; step++)
            {
                try
                {
                    CancelTreeMission(ActiveMissions.First());
                }
                catch { }
            }
            CompletedMissions = new List<SubMissionStandby>();
            ProgressX = 0;
            ProgressY = 0;
        }

    }
    public class SubMissionStandby 
    {
        [JsonIgnore]
        public SubMissionTree tree;
        [JsonIgnore]
        public SubMissionTree Tree
        {
            get 
            {
                if (tree == null)
                    tree = GetTree();
                return tree; 
            }
            set
            {
                treeName = value.TreeName;
                tree = value;
            }
        }
        public string treeName;

        public string Name = "Unset";
        public string AltName = "Unset";
        public List<string> AltNames;
        public string Desc = "Nothing";
        public string Faction = "";
        public int GradeRequired = 0;
        public byte MinProgressX = 0;
        public byte MinProgressY = 0;

        public float LoadRadius = 0;

        public SubMissionReward Rewards; //

        public void GetAndSetDisplayName()
        {   // 
            if (!AltName.NullOrEmpty())
                return;
            if (AltNames == null)
                AltName = Name;
            else if (AltNames.Count < 1)
                AltName = Name;
            else
            {
                string check = AltNames.GetRandomEntry();
                if (check.NullOrEmpty())
                    AltName = Name;
                AltName = check;
            }
        }
        public SubMissionTree GetTree()
        {   // 
            return ManSubMissions.GetTree(treeName);
        }
    }
}
