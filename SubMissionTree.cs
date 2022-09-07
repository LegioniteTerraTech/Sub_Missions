using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;
using Sub_Missions.Steps;
using Newtonsoft.Json;
#if !STEAM
using Nuterra.BlockInjector;
#endif

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
        internal readonly List<SubMissionStandby> Missions = new List<SubMissionStandby>();          //MUST BE SET VIA JSON
        [JsonIgnore]
        internal readonly List<SubMissionStandby> RepeatMissions = new List<SubMissionStandby>();    //MUST BE SET VIA JSON
        [JsonIgnore]
        internal readonly List<SubMissionStandby> ImmedeateMissions = new List<SubMissionStandby>();    //MUST BE SET VIA JSON

        // JSON string linking
        public List<string> WorldObjectFileNames = new List<string>(); //MUST BE SET VIA JSON

        public List<string> MissionNames = new List<string>();          //MUST BE SET VIA JSON
        public List<string> RepeatMissionNames = new List<string>();    //MUST BE SET VIA JSON
        public List<string> ImmedeateMissionNames = new List<string>(); //MUST BE SET VIA JSON 

        public string ProgressXName = "Prestiege";
        public string ProgressYName = "Status";

        public SMCCorpLicense CustomCorpInfo;

        // Campaign Progression
        [JsonIgnore]
        internal List<SubMission> ActiveMissions = new List<SubMission>();// DO NOT SET!!! - saved in campaign
        [JsonIgnore]
        internal sbyte ProgressX = 0; // DO NOT SET!!! - saved in campaign
        [JsonIgnore]
        internal sbyte ProgressY = 0; // DO NOT SET!!! - saved in campaign
        [JsonIgnore]
        internal List<SubMissionStandby> CompletedMissions = new List<SubMissionStandby>();// DO NOT SET!!! - saved in campaign


        // COMPILED ON TREE BUILD
        [JsonIgnore]
        internal Dictionary<int, Texture> MissionTextures = new Dictionary<int, Texture>();// Compiled on tree building.
        [JsonIgnore]
        internal Dictionary<int, Mesh> MissionMeshes = new Dictionary<int, Mesh>();// Compiled on tree building.
        [JsonIgnore]
        internal Dictionary<int, GameObject> MissionPieces = new Dictionary<int, GameObject>();// Compiled on tree building.
        [JsonIgnore]
        internal string TreeDirectory = "error";// Compiled on tree building.

        // Documentation
        public static string GetDocumentation()
        {   
            throw new NotImplementedException();
            //return null;
        }

        // Initialization
        internal bool CompileMissionTree(out SubMissionTree newTree)
        {   // Reduce memory loads
            if (!SMissionJSONLoader.TreeLoader(TreeName, TreeDirectory, out SubMissionTree tree))
            {
                newTree = null;
                return false;
            }
            if (tree.TreeName == null)
                tree.TreeName = "NULL_INVALID";
            SetupTreeCorp();
            List<SubMission> MissionsLoaded = SMissionJSONLoader.LoadAllMissions(tree);
            List<SubMissionStandby> compiled = CompileToStandby(MissionsLoaded);

            // Now we sort them based on input strings
            foreach (SubMissionStandby sort in compiled)
            {
                sort.Tree = tree;
                bool doNow = tree.ImmedeateMissionNames.Contains(sort.Name);
                bool repeat = tree.RepeatMissionNames.Contains(sort.Name);
                if (repeat && doNow)
                {
                    SMUtil.Assert(false, "SubMissions: Tree " + TreeName + " contains mission " + sort.Name + " that's specified in both ImmedeateMissionNames and RepeatMissionNames.");
                    SMUtil.Assert(false, "  Make sure to assign it to ImmedeateMissionNames or RepeatMissionNames.");
                    SMUtil.Assert(false, "  Defaulting " + sort.Name + " to MissionNames.");
                    sort.Type = SubMissionType.Basic;
                    tree.Missions.Add(sort);
                    continue;
                }
                else if (doNow)
                {
                    Debug_SMissions.Log("SubMissions: Mission " + sort.Name + " has been assigned to " + TreeName + " as a Immedeate mission that will be auto-assigned as soon as it's criteria is met.");
                    sort.Type = SubMissionType.Immedeate;
                    tree.RepeatMissions.Add(sort);
                    continue;
                }
                bool main = tree.MissionNames.Contains(sort.Name);
                if (repeat && main)
                {
                    SMUtil.Assert(false, "SubMissions: Tree " + TreeName + " contains mission " + sort.Name + " that's specified in both MissionNames and RepeatMissionNames.");
                    SMUtil.Assert(false, "  Make sure to assign it to MissionNames or RepeatMissionNames.");
                    SMUtil.Assert(false, "  Defaulting " + sort.Name + " to MissionNames.");
                    sort.Type = SubMissionType.Basic;
                    tree.Missions.Add(sort);
                }
                else if (repeat)
                {
                    Debug_SMissions.Log("SubMissions: Mission " + sort.Name + " has been assigned to " + TreeName + " as a repeatable mission.");
                    sort.Type = SubMissionType.Repeating;
                    tree.RepeatMissions.Add(sort);
                }
                else if (main)
                {
                    Debug_SMissions.Log("SubMissions: Mission " + sort.Name + " has been assigned to " + TreeName + " as a main mission.");
                    sort.Type = SubMissionType.Basic;
                    tree.Missions.Add(sort);
                }
                else
                {
                    SMUtil.Assert(false, "SubMissions: Tree " + TreeName + " contains unspecified mission " + sort.Name + ".");
                    SMUtil.Assert(false, "  Make sure to assign it to MissionNames or RepeatMissionNames.");
                    SMUtil.Assert(false, "  Defaulting " + sort.Name + " to MissionNames.");
                    sort.Type = SubMissionType.Basic;
                    tree.Missions.Add(sort);
                }

            }
            Debug_SMissions.Log("SubMissions: Compiled tree for " + TreeName + ".");
            newTree = tree;



            return true;
        }

        // Accessing
        private void SetupTreeCorp()
        {   //
            FactionSubTypes FST = ManMods.inst.GetCorpIndex(Faction);
            if (FST == (FactionSubTypes)(-1))
            {
                if (ManSMCCorps.GetSMCIDUnofficial(Faction, out FactionSubTypes FST1))
                {
                    Debug_SMissions.Log("SubMissions: linked MissionTree with unofficial Custom Corp " + Faction + " of ID " + FST1);
                }
                else if (CustomCorpInfo != null)
                {
                    ManSMCCorps.TryMakeNewCorp(CustomCorpInfo);
                }
#if !STEAM
                else if (KickStart.isBlockInjectorPresent)
                {
                    int hash = Faction.GetHashCode();
                    List<CustomCorporation> CC = BlockLoader.CustomCorps.Values.ToList();
                    CustomCorporation CCS = CC.Find(delegate (CustomCorporation cand) { return cand.Name.GetHashCode() == hash; });
                    if (CCS != null)
                    {
                        ManSMCCorps.TryMakeNewCorpBI(CCS);
                        FST = (FactionSubTypes)CCS.CorpID;
                    }
                }
#endif
            }
            else
            { 
            }
        }
        public static bool GetTreeCorp(string factionName, out FactionSubTypes FST)
        {   //
            FST = (FactionSubTypes)(-1);
            try
            {
                FST = ManMods.inst.GetCorpIndex(factionName);
            }
            catch { }

            if (FST == (FactionSubTypes)(-1))
            {
                if (ManSMCCorps.GetSMCIDUnofficial(factionName, out FactionSubTypes FST1))
                {
                    FST = FST1;
                    return true;
                }
                else if (KickStart.isBlockInjectorPresent)
                {
#if !STEAM
                    int hash = factionName.GetHashCode();
                    List<CustomCorporation> CC = BlockLoader.CustomCorps.Values.ToList();
                    CustomCorporation CCS = CC.Find(delegate (CustomCorporation cand) { return cand.Name.GetHashCode() == hash; });
                    if (CCS != null)
                    {
                        ManSMCCorps.TryMakeNewCorpBI(CCS);
                        FST = (FactionSubTypes)CCS.CorpID;
                        return true;
                    }
#endif
                }
            }
            else
                return true;
            return false;
        }
        public FactionSubTypes GetTreeCorp()
        {   //
            FactionSubTypes FST = (FactionSubTypes)(-1);
            try
            {
                FST = ManMods.inst.GetCorpIndex(Faction);
            }
            catch { }

            if (FST == (FactionSubTypes)(-1))
            {
                if (ManSMCCorps.GetSMCIDUnofficial(Faction, out FactionSubTypes FST1))
                {
                    return FST1;
                }
                else if (KickStart.isBlockInjectorPresent)
                {
#if !STEAM
                    int hash = Faction.GetHashCode();
                    List<CustomCorporation> CC = BlockLoader.CustomCorps.Values.ToList();
                    CustomCorporation CCS = CC.Find(delegate (CustomCorporation cand) { return cand.Name.GetHashCode() == hash; });
                    if (CCS != null)
                    {
                        ManSMCCorps.TryMakeNewCorpBI(CCS);
                        return (FactionSubTypes)CCS.CorpID;
                    }
                    else
                    {
                        ManSMCCorps.TryMakeNewCorp(Faction);
                        return (FactionSubTypes)ManSMCCorps.GetSMCCorp(Faction).ID;
                     }
#endif
                }
            }
            else 
                return FST;
            return FactionSubTypes.GSO;
        }
        public static FactionSubTypes GetTreeCorp(string factionName)
        {   //
            FactionSubTypes FST = (FactionSubTypes)(-1);
            try
            {
                FST = ManMods.inst.GetCorpIndex(factionName);
            }
            catch { }

            if (FST == (FactionSubTypes)(-1))
            {
                if (ManSMCCorps.GetSMCIDUnofficial(factionName, out FactionSubTypes FST1))
                {
                    return FST1;
                }
                else if (KickStart.isBlockInjectorPresent)
                {
#if !STEAM
                    int hash = factionName.GetHashCode();
                    List<CustomCorporation> CC = BlockLoader.CustomCorps.Values.ToList();
                    CustomCorporation CCS = CC.Find(delegate (CustomCorporation cand) { return cand.Name.GetHashCode() == hash; });
                    if (CCS != null)
                    {
                        return (FactionSubTypes)CCS.CorpID;
                    }
                    else
                    {
                        return ManSMCCorps.TryMakeNewCorp(factionName);
                    }
#endif
                }
            }
            else
                return FST;
            return FactionSubTypes.GSO;
        }


        // Actions
        internal bool AcceptTreeMission(SubMissionStandby Anon, Encounter Enc = null)
        {   //
            if (DeployMission(TreeName, Anon, out SubMission Deployed, Enc))
            {
                Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.AcceptMission);
                SubMission newMission = Deployed;
                newMission.Startup();
                ActiveMissions.Add(newMission);
                EncounterShoehorn.SetFakeEncounter(newMission, false);
                ManSubMissions.inst.GetAllPossibleMissions();
                ManSubMissions.Selected = newMission;
                return true;
            }
            else
                SMUtil.Assert(false, "<b> Could not deploy mission! </b>");
            return false;
        }
        internal void CancelTreeMission(SubMission Active)
        {   //
            try
            {
                if (ManSubMissions.Selected == Active)
                {
                    if (ManSubMissions.ActiveSubMissions[0] != null)
                        ManSubMissions.Selected = ManSubMissions.ActiveSubMissions[0];
                    else
                        ManSubMissions.Selected = null;
                }
                EncounterShoehorn.OnFinishSubMission(Active, ManEncounter.FinishState.Cancelled);
                Active.Cleanup();
                if (!ActiveMissions.Remove(Active))
                    Debug_SMissions.Log("SubMissions: Called wrong tree [" + TreeName + "] for mission " + Active.Name + " on CancelTreeMission!");
                ManSubMissions.inst.GetAllPossibleMissions();
            }
            catch
            {
                Debug_SMissions.Log("SubMissions: CancelTreeMission - Could not cancel mission!  Mission " + Active.Name + " of Tree" + TreeName);
            }
        }

        /// <summary>
        /// For external use
        /// </summary>
        public void ExternalCancelTreeMission(SubMission toCancel)
        {   //
            Debug_SMissions.Log("SubMissions: SubMissionTree.ExternalCancelTreeMission - Externally invoked: " + StackTraceUtility.ExtractStackTrace());
            CancelTreeMission(toCancel);
        }
        public void ExternalResetALLTreeMissions()
        {   //
            Debug_SMissions.Log("SubMissions: SubMissionTree.ExternalResetALLTreeMissions - Externally invoked: " + StackTraceUtility.ExtractStackTrace());
            ResetALLTreeMissions();
        }

        public List<SubMissionStandby> GetImmediateMissions()
        {   //
            //Debug_SMissions.Log("SubMissions: " + TreeName + " is fetching missions");
            List<SubMissionStandby> initMissions = new List<SubMissionStandby>();
            Debug_SMissions.Info("SubMissions: Immediate Missions count " + ImmedeateMissions.Count);
            if (!KickStart.OverrideRestrictions)
            {
                foreach (SubMissionStandby mission in ImmedeateMissions)
                {
                    //Debug_SMissions.Log("SubMissions: Trying to validate mission " + mission.Name);
                    int hashName = mission.Name.GetHashCode();
                    if (ActiveMissions.Exists(delegate (SubMission cand) { return cand.Name.GetHashCode() == hashName; }))
                    {   // It's already in the list
                        Debug_SMissions.Info("SubMissions: " + mission.Name + " is already active");
                        continue;
                    }
                    if (CompletedMissions.Exists(delegate (SubMissionStandby cand) { return cand.Name.GetHashCode() == hashName; }))
                    {   // It's been finished already, do not get
                        Debug_SMissions.Info("SubMissions: " + mission.Name + " is already finished");
                        continue;
                    }
                    if (mission.MinProgressX > ProgressX)
                    {
                        Debug_SMissions.Info("SubMissions: " + mission.Name + " - not enough " + mission.Tree.ProgressXName + ".");
                        continue;
                    }
                    if (mission.MinProgressY > ProgressY)
                    {
                        Debug_SMissions.Info("SubMissions: " + mission.Name + " - not enough " + mission.Tree.ProgressYName + ".");
                        continue;
                    }
                    try
                    {
                        FactionLicense licence = Singleton.Manager<ManLicenses>.inst.GetLicense(GetTreeCorp(mission.Faction));
                        if (licence.IsDiscovered && licence.CurrentLevel >= mission.GradeRequired)
                        {
                            mission.GetAndSetDisplayName();
                            Debug_SMissions.Log("SubMissions: Pushing mission " + mission.Name + " now - the player has no option to deny this");
                            initMissions.Add(mission);
                        }
                    }
                    catch
                    {
                        Debug_SMissions.Info("SubMissions: " + mission.Name + " is not available right now");
                        continue;
                    }
                }
            }
            return initMissions;
        }

        public List<SubMissionStandby> GetReachableMissions()
        {   //
            //Debug_SMissions.Log("SubMissions: " + TreeName + " is fetching missions");
            //if (ManSMCCorps.GetSMCCorp(Faction, out SMCCorpLicense CL))
            //   CL.RefreshCorpUISP();
            List<SubMissionStandby> initMissions = new List<SubMissionStandby>();
            //Debug_SMissions.Log("SubMissions: Tree " + TreeName + " Missions count " + Missions.Count + " | " + RepeatMissions.Count);
            foreach (SubMissionStandby mission in Missions)
            {
                //Debug_SMissions.Log("SubMissions: Trying to validate mission " + mission.Name);
                int hashName = mission.Name.GetHashCode();
                if (ActiveMissions.Exists(delegate (SubMission cand) { return cand.Name.GetHashCode() == hashName; }))
                {   // It's already in the list
                    //Debug_SMissions.Log("SubMissions: " + mission.Name + " is already active");
                    continue;
                }
                if (KickStart.OverrideRestrictions)
                {
                    //Debug_SMissions.Log("SubMissions: Presenting mission " + mission.Name);
                    mission.GetAndSetDisplayName();
                    initMissions.Add(mission);
                    continue;
                }
                if (CompletedMissions.Exists(delegate (SubMissionStandby cand) { return cand.Name.GetHashCode() == hashName; }))
                {   // It's been finished already, do not get
                    //Debug_SMissions.Log("SubMissions: " + mission.Name + " is already finished");
                    continue;
                }
                if (mission.MinProgressX > ProgressX)
                {
                    //Debug_SMissions.Log("SubMissions: " + mission.Name + " - not enough " + mission.Tree.ProgressXName + ".");
                    continue;
                }
                if (mission.MinProgressY > ProgressY)
                {
                    //Debug_SMissions.Log("SubMissions: " + mission.Name + " - not enough " + mission.Tree.ProgressYName + ".");
                    continue;
                }
                if (mission.SPOnly && ManNetwork.IsNetworked)
                    continue;
                try
                {
                    switch (mission.placementMethod)
                    {
                        case SubMissionPosition.CloseToPlayer:
                        case SubMissionPosition.OffsetFromPlayer:
                        case SubMissionPosition.OffsetFromPlayerTechFacing:
                        case SubMissionPosition.FixedCoordinate:
                            if (ManSubMissions.IsTooCloseToOtherMission(WorldPosition.FromScenePosition(Singleton.playerPos).TileCoord))
                            {
                                Debug_SMissions.Log("SubMissions: " + mission.Name + " - another mission is too close!");
                                continue;
                            }
                            break;
                    }
                }
                catch { Debug_SMissions.Log("Player does not exist yet"); }
                try
                {
                    FactionLicense licence = Singleton.Manager<ManLicenses>.inst.GetLicense(GetTreeCorp(mission.Faction));
                 
                    if (licence.IsDiscovered && licence.CurrentLevel >= mission.GradeRequired)
                    {
                        mission.GetAndSetDisplayName();
                       // Debug_SMissions.Log("SubMissions: Presenting mission " + mission.Name);
                        initMissions.Add(mission);
                    }
                }
                catch
                {
                    //Debug_SMissions.Log("SubMissions: " + mission.Name + " is not available right now");
                    continue;
                }
            }
            foreach (SubMissionStandby mission in RepeatMissions)
            {
                //Debug_SMissions.Log("SubMissions: Trying to validate mission " + mission.Name);
                int hashName = mission.Name.GetHashCode();
                if (ActiveMissions.Exists(delegate (SubMission cand) { return cand.Name.GetHashCode() == hashName; }))
                {   // It's already in the list
                    //Debug_SMissions.Log("SubMissions: " + mission.Name + " is already active");
                    continue;
                }
                if (KickStart.OverrideRestrictions)
                {
                    //Debug_SMissions.Log("SubMissions: Presenting mission " + mission.Name);
                    mission.GetAndSetDisplayName();
                    initMissions.Add(mission);
                    continue;
                }
                if (mission.MinProgressX > ProgressX)
                {
                    //Debug_SMissions.Log("SubMissions: " + mission.Name + " - not enough " + mission.Tree.ProgressXName + ".");
                    continue;
                }
                if (mission.MinProgressY > ProgressY)
                {
                    //Debug_SMissions.Log("SubMissions: " + mission.Name + " - not enough " + mission.Tree.ProgressYName + ".");
                    continue;
                }
                try
                {
                    FactionLicense licence = Singleton.Manager<ManLicenses>.inst.GetLicense(GetTreeCorp());
                    if (licence.IsDiscovered && licence.CurrentLevel >= mission.GradeRequired)
                    {
                        mission.GetAndSetDisplayName();
                        Debug_SMissions.Log("SubMissions: Presenting mission " + mission.AltName);
                        initMissions.Add(mission);
                    }
                }
                catch
                {
                    //Debug_SMissions.Log("SubMissions: " + mission.Name + " is not available right now");
                    continue;
                }
            }
            if (KickStart.OverrideRestrictions)
            {
                foreach (SubMissionStandby mission in ImmedeateMissions)
                {
                    //Debug_SMissions.Log("SubMissions: Trying to validate mission " + mission.Name);
                    int hashName = mission.Name.GetHashCode();
                    if (ActiveMissions.Exists(delegate (SubMission cand) { return cand.Name.GetHashCode() == hashName; }))
                    {   // It's already in the list
                        //Debug_SMissions.Log("SubMissions: " + mission.Name + " is already active");
                        continue;
                    }
                    Debug_SMissions.Log("SubMissions: Presenting mission " + mission.Name);
                    mission.GetAndSetDisplayName();
                    initMissions.Add(mission);
                }
            }
            return initMissions;
        }


        // COMPILER
        private List<SubMission> DeployMissions(string treeName, List<SubMissionStandby> toDeploy)
        {   // Because each mission takes up an unholy amount of memory, we want to 
            //   only load the entire thing when nesseary
            List<SubMission> missionsLoaded = new List<SubMission>();
            foreach (SubMissionStandby mission in toDeploy)
            {
                if (DeployMission(treeName, mission, out SubMission Deployed))
                    missionsLoaded.Add(Deployed);
            }
            return missionsLoaded;
        }
        private bool DeployMission(string treeName, SubMissionStandby toDeploy, out SubMission Deployed, Encounter Enc = null)
        {   // Because each mission takes up an unholy amount of memory, we want to 
            //   only load the entire thing when nesseary
            Deployed = SMissionJSONLoader.MissionLoader(this, toDeploy.Name);
            if (Deployed == null)
            {
                SMUtil.Assert(false, "<b> CRITICAL ERROR IN HANDLING " + toDeploy.Name + " of tree " + treeName + " - UNABLE TO IMPORT ANY INFORMATION! </b>");
                return false;
            }
            Deployed.SelectedAltName = toDeploy.AltName;
            Deployed.Description = toDeploy.Desc;
            Deployed.Type = toDeploy.Type;
            Deployed.FakeEncounter = Enc;
            return true;
        }
        private static List<SubMissionStandby> CompileToStandby(List<SubMission> MissionsLoaded)
        {   // Reduce memory loads
            List<SubMissionStandby> missions = new List<SubMissionStandby>();
            foreach (SubMission mission in MissionsLoaded)
            {
                missions.Add(CompileToStandby(mission));
            }
            return missions;
        }
        private static SubMissionStandby CompileToStandby(SubMission mission)
        {   // Reduce memory loads
            List<MissionChecklist> augmentedList = new List<MissionChecklist>();
            int lengthList = mission.CheckList.Count;
            for (int step = 0; step < lengthList; step++)
            {
                MissionChecklist listEle = mission.CheckList[step];
                MissionChecklist listEleC = new MissionChecklist
                {
                    BoolToEnable = listEle.BoolToEnable,
                    GlobalIndex = listEle.GlobalIndex,
                    GlobalIndex2 = listEle.GlobalIndex2,
                    ListArticle = listEle.ListArticle,
                    ValueType = listEle.ValueType,
                };

                if (mission.VarInts.Count > listEle.GlobalIndex)
                    listEleC.GlobalIndex = mission.VarInts[listEle.GlobalIndex];
                augmentedList.Add(listEleC);
            }

            SubMissionStandby missionCompiled = new SubMissionStandby
            {
                Tree = mission.Tree,
                Name = mission.Name,
                AltName = mission.SelectedAltName,
                AltNames = mission.AltNames,
                Desc = mission.Description,
                AltDescs = mission.AltDescs,
                GradeRequired = mission.GradeRequired,
                Faction = mission.Faction,
                Type = mission.Type,
                Checklist = augmentedList,
                Rewards = mission.Rewards,
                MinProgressX = mission.MinProgressX,
                MinProgressY = mission.MinProgressY,
                SPOnly = mission.SinglePlayerOnly,
                TilePosWorld = mission.TilePos,
                placementMethod = mission.SpawnPosition,
                CannotCancel = mission.CannotCancel,
            };
            missionCompiled.LoadRadius = mission.GetMinimumLoadRange();
            return missionCompiled;
        }


        // Events
        internal void FinishedMission(SubMission finished)
        {
            Debug_SMissions.Log("SubMissions: Finished mission " + finished.Name + " of Tree " + TreeName + ".");
            int hashName = finished.Name.GetHashCode();
            if (RepeatMissions.Exists(delegate (SubMissionStandby cand) { return cand.Name.GetHashCode() == hashName; }))
            {   // Do nothing special - repeat missions are to be repeated
            }
            else if (Missions.Exists(delegate (SubMissionStandby cand) { return cand.Name.GetHashCode() == hashName; }))
            {
                CompletedMissions.Add(CompileToStandby(finished));
            }
            else
                Debug_SMissions.Log("SubMissions: Tried to finish mission " + finished.Name + " that's not listed in this tree!  Tree " + TreeName);
            if (!ActiveMissions.Remove(finished))
                Debug_SMissions.Log("SubMissions: Tried to finish mission " + finished.Name + " but it doesn't exist in the ActiveMissions list!  Tree " + TreeName);
            try
            {
                ManSubMissions.Selected = ActiveMissions.First();
            }
            catch
            {
                ManSubMissions.Selected = null;
            }
            ManSubMissions.UpdateSaveStateForCustomCorps();
            ManSubMissions.inst.GetAllPossibleMissions();
        }
        internal void ResetALLTreeMissions()
        {
            ManSubMissions.Selected = null;
            ManSubMissions.SelectedAnon = null;
            int CountStep = ActiveMissions.Count();
            for (int step = 0; step < CountStep; step++)
            {
                try
                {
                    UnloadTreeMission(ActiveMissions.First());
                }
                catch { }
            }
            CompletedMissions = new List<SubMissionStandby>();
            ProgressX = 0;
            ProgressY = 0;
            ManSubMissions.inst.GetAllPossibleMissions();
        }
        private void UnloadTreeMission(SubMission Active)
        { 
            try
            {
                Active.Cleanup(true);
                if (!ActiveMissions.Remove(Active))
                    Debug_SMissions.Log("SubMissions: Called wrong tree [" + TreeName + "] for mission " + Active.Name + " on UnloadTreeMission!");
            }
            catch
            {
                Debug_SMissions.Log("SubMissions: UnloadTreeMission - Could not unload mission!  Mission " + Active.Name + " of Tree" + TreeName);
            }
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
        public string AltName;
        public List<string> AltNames;
        public string Desc = "Nothing";
        public List<string> AltDescs;
        public string Faction = "";
        public int GradeRequired = 0;
        public SubMissionType Type;
        public byte MinProgressX = 0;
        public byte MinProgressY = 0;
        public bool SPOnly = false;
        public bool CannotCancel = false;

        // Mostly just to prevent overlapping
        public IntVector2 TilePosWorld = IntVector2.zero;
        public SubMissionPosition placementMethod;

        public float LoadRadius = 0;

        public List<MissionChecklist> Checklist;
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
                try
                {
                    Desc = AltDescs.ElementAt(AltNames.IndexOf(check));
                }
                catch { }// don't change
            }
        }
        public SubMissionTree GetTree()
        {   // 
            return ManSubMissions.GetTree(treeName);
        }
    }
}
