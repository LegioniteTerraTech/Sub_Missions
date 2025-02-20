using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using TAC_AI.Templates;
using Sub_Missions.Steps;
using Newtonsoft.Json;
using System.Reflection;
using TerraTechETCUtil;
using TAC_AI.AI.Enemy;
using System.Collections;
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
        public string ModID = KickStart.ModID;

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
        internal Dictionary<string, Texture> MissionTextures = new Dictionary<string, Texture>();// Compiled on tree building.
        [JsonIgnore]
        internal Dictionary<string, Mesh> MissionMeshes = new Dictionary<string, Mesh>();// Compiled on tree building.
        [JsonIgnore]
        internal Dictionary<string, Dictionary<IntVector2, TerrainModifier>> TerrainEdits = 
            new Dictionary<string, Dictionary<IntVector2, TerrainModifier>>();// Compiled on tree building.
        [JsonIgnore]
        internal Dictionary<string, SMWorldObject> WorldObjects = new Dictionary<string, SMWorldObject>();// Compiled on tree building.
        [JsonIgnore]
        internal Dictionary<string, SpawnableTech> TreeTechs = new Dictionary<string, SpawnableTech>();// Compiled on tree building.
        [JsonIgnore]
        internal int TreeTechCount => TreeTechs.Count;
        internal void AddTreeTech(string name, string dir)
        {
            if (TreeTechs.ContainsKey(name))
            {
                SMUtil.Error(false, "Mission Tree SnapTechs (Loading) ~ " + TreeName + ", tech " + name,
                    "Tech of name " + name + " already is assigned to the tree.  Cannot add " +
                    "multiple Techs of same name!");
            }
            else
            {
                if (MissionTextures.ContainsKey(name))
                    SMUtil.Error(false, "Mission Tree SnapTechs (Loading) ~ " + TreeName + ", tech " + name,
                        "Tech of name " + name + " tried to be added to the tree but a Texture of the same name" +
                        " is already assigned!  Cannot add multiple Textures of same name!");
                else
                {
                    TreeTechs.Add(name, new SpawnableTechSnapshot(name));
                    MissionTextures.Add(name, FileUtils.LoadTexture(dir));
                }
            }
        }

        [JsonIgnore]
        internal SubMissionHierachy TreeHierachy = null;// Compiled on tree building.

        public bool TryGetMesh(string nameWithExt, out Mesh mesh)
        {
            if (SMissionJSONLoader.TryGetBuiltinMesh(nameWithExt, out mesh))
                return true;
            if (MissionMeshes.TryGetValue(nameWithExt, out mesh))
                return true;
            return false;
        }

        // Documentation
        public static string GetDocumentation()
        {
            return "{" +
  "\"TreeName\": \"Template\", //The name of the mission tree.  Must be unique." +
  "\"Faction\": \"GSO\", //The Faction/Corp the mission tree is affilated with." +
  "\"WorldObjectFileNames\": [//The names of the world objects affilated with this mission tree." +
  "  \"ModularBrickCube_(636)\"" +
  "]," +
  "\"MissionNames\": [//The names of the missions affilated with this mission tree." +
  "  \"NPC Mission\"," +
  "  \"Harvest Mission\"," +
  "  \"Water Blocks Aid\"" +
  "]," +
  "\"RepeatMissionNames\": [//The names of the missions affilated with this mission tree that should REPEAT." +
  "  \"Combat Mission\"" +
  "]," +
  "\"ImmedeateMissionNames\": [],//The names of the missions affilated with this mission tree that should trigger as soon as they can." +
  "\"ProgressXName\": \"Prestiege\"," +
  "\"ProgressYName\": \"Status\"," +
  "\"CustomCorpInfo\": null" +
"}";
        
            throw new NotImplementedException();
            //return null;
        }

        // Initialization
        internal void CompileMissionTree(out SubMissionTree newTree)
        {   // Reduce memory loads
            try
            {
                SMissionJSONLoader.TreeLoader(TreeHierachy, out SubMissionTree tree);
                if (tree.TreeName == null)
                    tree.TreeName = "NULL_INVALID";
                SetupTreeCorp();

                // Now we sort them based on input strings
                foreach (SubMissionStandby sort in SMissionJSONLoader.LoadAllMissionsToStandby(tree))
                {
                    sort.Tree = tree;
                    bool doNow = tree.ImmedeateMissionNames.Contains(sort.Name);
                    bool repeat = tree.RepeatMissionNames.Contains(sort.Name);
                    if (repeat && doNow)
                    {
                        SMUtil.Error(false, "Mission Tree (Startup) ~ " + tree.TreeName, 
                            KickStart.ModID + ": Tree " + TreeName + " contains mission " + sort.Name + 
                            " that's specified in both ImmedeateMissionNames and RepeatMissionNames. \n" +
                            "  Make sure to assign it to ImmedeateMissionNames or RepeatMissionNames. \n" +
                            "  Defaulting " + sort.Name + " to MissionNames.");
                        sort.Type = SubMissionType.Basic;
                        tree.Missions.Add(sort);
                        continue;
                    }
                    else if (doNow)
                    {
                        Debug_SMissions.Log(KickStart.ModID + ": Mission " + sort.Name + " has been assigned to " + TreeName + " as a Immedeate mission that will be auto-assigned as soon as it's criteria is met.");
                        sort.Type = SubMissionType.Immedeate;
                        tree.RepeatMissions.Add(sort);
                        continue;
                    }
                    bool main = tree.MissionNames.Contains(sort.Name);
                    if (repeat && main)
                    {
                        SMUtil.Error(false, "Mission Tree (Startup) ~ " + tree.TreeName, 
                            KickStart.ModID + ": Tree " + TreeName + " contains mission " + sort.Name + 
                            " that's specified in both MissionNames and RepeatMissionNames.\n" +
                            "  Make sure to assign it to MissionNames or RepeatMissionNames.\n" +
                            "  Defaulting " + sort.Name + " to MissionNames.");
                        sort.Type = SubMissionType.Basic;
                        tree.Missions.Add(sort);
                    }
                    else if (repeat)
                    {
                        Debug_SMissions.Log(KickStart.ModID + ": Mission " + sort.Name + " has been assigned to " + 
                            TreeName + " as a repeatable mission.");
                        sort.Type = SubMissionType.Repeating;
                        tree.RepeatMissions.Add(sort);
                    }
                    else if (main)
                    {
                        Debug_SMissions.Log(KickStart.ModID + ": Mission " + sort.Name + " has been assigned to " + 
                            TreeName + " as a main mission.");
                        sort.Type = SubMissionType.Basic;
                        tree.Missions.Add(sort);
                    }
                    else
                    {
                        SMUtil.Error(false, "Mission Tree (Startup) ~ " + tree.TreeName,
                            KickStart.ModID + ": Tree " + TreeName + " contains unspecified mission " + 
                            sort.Name + ".\n Make sure to assign it to MissionNames or RepeatMissionNames." +
                            "\n Defaulting " + sort.Name + " to MissionNames.");
                        sort.Type = SubMissionType.Basic;
                        tree.Missions.Add(sort);
                    }
                }
                
                SMUtil.Log(false, KickStart.ModID + ": Compiled tree for " + TreeName + ".");
                newTree = tree;
            }
            catch (MandatoryException e)
            {
                throw e;
            }
        }

        internal void ReloadMissionFromFile(ref SubMissionStandby mission)
        {
            SubMissionStandby inst = CompileToStandby(SMissionJSONLoader.MissionLoader(
                mission.Tree, mission.Name));
            if (LoadMissionFromFile(mission.tree, inst))
            {
                RepeatMissions.Remove(mission);
                ImmedeateMissions.Remove(mission);
                Missions.Remove(mission);
                mission = inst;
            }
        }
        private bool LoadMissionFromFile(SubMissionTree tree, SubMissionStandby sort)
        {
            sort.Tree = tree;
            bool doNow = tree.ImmedeateMissionNames.Contains(sort.Name);
            bool repeat = tree.RepeatMissionNames.Contains(sort.Name);
            if (repeat && doNow)
            {
                SMUtil.Error(false, "Mission Tree (Startup) ~ " + tree.TreeName,
                    KickStart.ModID + ": Tree " + TreeName + " contains mission " + sort.Name +
                    " that's specified in both ImmedeateMissionNames and RepeatMissionNames. \n" +
                    "  Make sure to assign it to ImmedeateMissionNames or RepeatMissionNames. \n" +
                    "  Defaulting " + sort.Name + " to MissionNames.");
                sort.Type = SubMissionType.Basic;
                tree.Missions.Add(sort);
                return false;
            }
            else if (doNow)
            {
                Debug_SMissions.Log(KickStart.ModID + ": Mission " + sort.Name + " has been assigned to " + TreeName + " as a Immedeate mission that will be auto-assigned as soon as it's criteria is met.");
                sort.Type = SubMissionType.Immedeate;
                tree.RepeatMissions.Add(sort);
                return false;
            }
            bool main = tree.MissionNames.Contains(sort.Name);
            if (repeat && main)
            {
                SMUtil.Error(false, "Mission Tree (Startup) ~ " + tree.TreeName,
                    KickStart.ModID + ": Tree " + TreeName + " contains mission " + sort.Name +
                    " that's specified in both MissionNames and RepeatMissionNames.\n" +
                    "  Make sure to assign it to MissionNames or RepeatMissionNames.\n" +
                    "  Defaulting " + sort.Name + " to MissionNames.");
                sort.Type = SubMissionType.Basic;
                tree.Missions.Add(sort);
            }
            else if (repeat)
            {
                Debug_SMissions.Log(KickStart.ModID + ": Mission " + sort.Name + " has been assigned to " +
                    TreeName + " as a repeatable mission.");
                sort.Type = SubMissionType.Repeating;
                tree.RepeatMissions.Add(sort);
            }
            else if (main)
            {
                Debug_SMissions.Log(KickStart.ModID + ": Mission " + sort.Name + " has been assigned to " +
                    TreeName + " as a main mission.");
                sort.Type = SubMissionType.Basic;
                tree.Missions.Add(sort);
            }
            else
            {
                SMUtil.Error(false, "Mission Tree (Startup) ~ " + tree.TreeName,
                    KickStart.ModID + ": Tree " + TreeName + " contains unspecified mission " +
                    sort.Name + ".\n Make sure to assign it to MissionNames or RepeatMissionNames." +
                    "\n Defaulting " + sort.Name + " to MissionNames.");
                sort.Type = SubMissionType.Basic;
                tree.Missions.Add(sort);
            }
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
                    Debug_SMissions.Log(KickStart.ModID + ": linked MissionTree with unofficial Custom Corp " + Faction + " of ID " + FST1);
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
        public static bool GetTreeCorp(string factionNameShort, out FactionSubTypes FST)
        {   //
            FST = (FactionSubTypes)(-1);
            try
            {
                FST = ManMods.inst.GetCorpIndex(factionNameShort);
            }
            catch { }

            if (FST == (FactionSubTypes)(-1))
            {
                if (ManSMCCorps.GetSMCIDUnofficial(factionNameShort, out FactionSubTypes FST1))
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
            return FST;
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

        // Terrain
        public static void ApplyTerrain(SubMission mission, string terrainName)
        {
            if (mission.Tree.TerrainEdits.TryGetValue(terrainName, out var terrain))
            {
                foreach (var item in terrain)
                    item.Value.FlushApply(1, new WorldPosition(mission.TilePos + item.Key, Vector3.zero).ScenePosition);
            }
            else
                SMUtil.Error(false, "Mission Tree (Startup) ~ " + mission.Tree.TreeName,
                    KickStart.ModID + ": Tree " + mission.Tree.TreeName + " could not find TerrainMod \"" +
                    terrainName + "\" in the tree.  Is it misnamed?");
        }



        // Techs
        public static Tank SpawnMissionTech(ref SubMission mission, Vector3 pos, int Team, Vector3 facingDirect, string TechName, bool instant = false)
        {   // We pull these from MissionTechs.json
            Tank tech = null;
            if (instant)
            {
                if (mission.Tree.TreeTechs.TryGetValue(TechName, out SpawnableTech val))
                {
                    val.Spawn(mission, pos, facingDirect, Team);
                }
            }
            else
            {
                TrackedTech techCase = SMUtil.GetTrackedTechBase(ref mission, TechName);
                techCase.delayedSpawn = ManSpawn.inst.SpawnDeliveryBombNew(pos, DeliveryBombSpawner.ImpactMarkerType.Tech);
                techCase.delayedSpawn.BombDeliveredEvent.Subscribe(techCase.SpawnTech);
                techCase.DeliQueued = true;
            }
            return tech;
        }


        // Actions
        internal bool AcceptTreeMission(SubMissionStandby Anon, bool forceNearPlayer, Encounter Enc = null)
        {   //
            if (DeployMission(TreeName, Anon, out SubMission Deployed, Enc))
            {
                if (GetTreeCorp(Anon.Faction) == (FactionSubTypes)(-1))
                {
                    Debug_SMissions.FatalError(KickStart.ModID + ": " + Anon.Name + " - Mission NEEDED to load " +
                        "but Corp \"" + Anon.Faction + "\" is not loaded!");
                }
                else
                {
                    Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.AcceptMission);
                    SubMission newMission = Deployed;
                    newMission.Startup(forceNearPlayer);
                    ActiveMissions.Add(newMission);
                    EncounterShoehorn.SetFakeEncounter(newMission, false);
                    ManSubMissions.inst.GetAllPossibleMissions();
                    ManSubMissions.Selected = newMission;
                    return true;
                }
            }
            SMUtil.Assert(false, "Mission (Startup) ~ " + Anon.Name, "<b> Could not deploy mission! </b>",
                new MandatoryException("Mission " + TreeName + " failed to deploy"));
            return false;
        }
        internal void CancelTreeMission(SubMission Active)
        {   //
            try
            {
                if (ManSubMissions.Selected == Active)
                {
                    if (ManSubMissions.GetActiveSubMissions[0] != null)
                        ManSubMissions.Selected = ManSubMissions.GetActiveSubMissions[0];
                    else
                        ManSubMissions.Selected = null;
                }
                EncounterShoehorn.OnFinishSubMission(Active, ManEncounter.FinishState.Cancelled);
                Active.Cleanup();
                if (!ActiveMissions.Remove(Active))
                    Debug_SMissions.Log(KickStart.ModID + ": Called wrong tree [" + TreeName + "] for mission " + Active.Name + " on CancelTreeMission!");
                ManSubMissions.inst.GetAllPossibleMissions();
            }
            catch
            {
                Debug_SMissions.Log(KickStart.ModID + ": CancelTreeMission - Could not cancel mission!  Mission " + Active.Name + " of Tree" + TreeName);
            }
        }

        /// <summary>
        /// For external use
        /// </summary>
        public void ExternalCancelTreeMission(SubMission toCancel)
        {   //
            Debug_SMissions.Log(KickStart.ModID + ": SubMissionTree.ExternalCancelTreeMission - Externally invoked: " + StackTraceUtility.ExtractStackTrace());
            CancelTreeMission(toCancel);
        }
        public void ExternalResetALLTreeMissions()
        {   //
            Debug_SMissions.Log(KickStart.ModID + ": SubMissionTree.ExternalResetALLTreeMissions - Externally invoked: " + StackTraceUtility.ExtractStackTrace());
            ResetALLTreeMissions();
        }

        private static List<SubMissionStandby> initMissionsCache = new List<SubMissionStandby>();
        public List<SubMissionStandby> GetImmediateMissions()
        {   //
            initMissionsCache.Clear();
            if (GetTreeCorp() == (FactionSubTypes)(-1))
            {
                SMUtil.Error(false, "Mission Tree (Startup) ~ " + TreeName,
                    KickStart.ModID + ": Tree " + TreeName + " could not find MissionCorp \"" + 
                    Faction + "\" in ManSMCCorps.  Do we have the respective containing mod installed?");
                return initMissionsCache;
            }
            //Debug_SMissions.Log(KickStart.ModID + ": " + TreeName + " is fetching missions");
            Debug_SMissions.Info(KickStart.ModID + ": Immediate Missions count " + ImmedeateMissions.Count);
            if (!KickStart.OverrideRestrictions)
            {
                foreach (SubMissionStandby mission in ImmedeateMissions)
                {
                    //Debug_SMissions.Log(KickStart.ModID + ": Trying to validate mission " + mission.Name);
                    int hashName = mission.Name.GetHashCode();
                    if (ActiveMissions.Exists(delegate (SubMission cand) { return cand.Name.GetHashCode() == hashName; }))
                    {   // It's already in the list
                        Debug_SMissions.Info(KickStart.ModID + ": " + mission.Name + " is already active");
                        continue;
                    }
                    if (CompletedMissions.Exists(delegate (SubMissionStandby cand) { return cand.Name.GetHashCode() == hashName; }))
                    {   // It's been finished already, do not get
                        Debug_SMissions.Info(KickStart.ModID + ": " + mission.Name + " is already finished");
                        continue;
                    }
                    if (GetTreeCorp(mission.Faction) == (FactionSubTypes)(-1))
                    {
                        SMUtil.Error(false, mission.Name, "Corp \"" + mission.Faction + "\" is not loaded.  Mission cannot load.");
                        continue;
                    }
                    if (mission.MinProgressX > ProgressX)
                    {
                        Debug_SMissions.Info(KickStart.ModID + ": " + mission.Name + " - not enough " + mission.Tree.ProgressXName + ".");
                        continue;
                    }
                    if (mission.MinProgressY > ProgressY)
                    {
                        Debug_SMissions.Info(KickStart.ModID + ": " + mission.Name + " - not enough " + mission.Tree.ProgressYName + ".");
                        continue;
                    }
                    try
                    {
                        FactionLicense licence = Singleton.Manager<ManLicenses>.inst.GetLicense(GetTreeCorp(mission.Faction));
                        if (licence.IsDiscovered && licence.CurrentLevel >= mission.GradeRequired)
                        {
                            mission.GetAndSetDisplayName();
                            Debug_SMissions.Log(KickStart.ModID + ": Pushing mission " + mission.Name + " now - the player has no option to deny this");
                            initMissionsCache.Add(mission);
                        }
                    }
                    catch
                    {
                        Debug_SMissions.Info(KickStart.ModID + ": " + mission.Name + " is not available right now");
                        continue;
                    }
                }
            }
            return initMissionsCache;
        }

        public List<SubMissionStandby> GetReachableMissions()
        {   //
            //Debug_SMissions.Log(KickStart.ModID + ": " + TreeName + " is fetching missions");
            //if (ManSMCCorps.GetSMCCorp(Faction, out SMCCorpLicense CL))
            //   CL.RefreshCorpUISP();
            initMissionsCache.Clear();
            if (GetTreeCorp() == (FactionSubTypes)(-1))
            {
                SMUtil.Error(false, "Mission Tree (Startup) ~ " + TreeName,
                    KickStart.ModID + ": Tree " + TreeName + " could not find MissionCorp \"" +
                    Faction + "\" in ManSMCCorps.  Do we have the respective containing mod installed?");
                return initMissionsCache;
            }
            //Debug_SMissions.Log(KickStart.ModID + ": Tree " + TreeName + " Missions count " + Missions.Count + " | " + RepeatMissions.Count);
            foreach (SubMissionStandby mission in Missions)
            {
                //Debug_SMissions.Log(KickStart.ModID + ": Trying to validate mission " + mission.Name);
                int hashName = mission.Name.GetHashCode();
                if (ActiveMissions.Exists(delegate (SubMission cand) { return cand.Name.GetHashCode() == hashName; }))
                {   // It's already in the list
                    //Debug_SMissions.Log(KickStart.ModID + ": " + mission.Name + " is already active");
                    continue;
                }
                if (KickStart.OverrideRestrictions)
                {
                    //Debug_SMissions.Log(KickStart.ModID + ": Presenting mission " + mission.Name);
                    mission.GetAndSetDisplayName();
                    initMissionsCache.Add(mission);
                    continue;
                }
                if (CompletedMissions.Exists(delegate (SubMissionStandby cand) { return cand.Name.GetHashCode() == hashName; }))
                {   // It's been finished already, do not get
                    //Debug_SMissions.Log(KickStart.ModID + ": " + mission.Name + " is already finished");
                    continue;
                }
                if (GetTreeCorp(mission.Faction) == (FactionSubTypes)(-1))
                {
                    SMUtil.Error(false, mission.Name, "Corp \"" + mission.Faction + "\" is not loaded.  Mission cannot load.");
                    continue;
                }
                if (mission.MinProgressX > ProgressX)
                {
                    //Debug_SMissions.Log(KickStart.ModID + ": " + mission.Name + " - not enough " + mission.Tree.ProgressXName + ".");
                    continue;
                }
                if (mission.MinProgressY > ProgressY)
                {
                    //Debug_SMissions.Log(KickStart.ModID + ": " + mission.Name + " - not enough " + mission.Tree.ProgressYName + ".");
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
                                Debug_SMissions.Log(KickStart.ModID + ": " + mission.Name + " - another mission is too close!");
                                continue;
                            }
                            break;
                    }
                }
                catch
                {
                    Debug_SMissions.Log("Player does not exist yet");
                }
                try
                {
                    FactionLicense licence = Singleton.Manager<ManLicenses>.inst.GetLicense(GetTreeCorp(mission.Faction));
                 
                    if (licence.IsDiscovered && licence.CurrentLevel >= mission.GradeRequired)
                    {
                        mission.GetAndSetDisplayName();
                        // Debug_SMissions.Log(KickStart.ModID + ": Presenting mission " + mission.Name);
                        initMissionsCache.Add(mission);
                    }
                }
                catch
                {
                    //Debug_SMissions.Log(KickStart.ModID + ": " + mission.Name + " is not available right now");
                    continue;
                }
            }
            foreach (SubMissionStandby mission in RepeatMissions)
            {
                //Debug_SMissions.Log(KickStart.ModID + ": Trying to validate mission " + mission.Name);
                int hashName = mission.Name.GetHashCode();
                if (ActiveMissions.Exists(delegate (SubMission cand) { return cand.Name.GetHashCode() == hashName; }))
                {   // It's already in the list
                    //Debug_SMissions.Log(KickStart.ModID + ": " + mission.Name + " is already active");
                    continue;
                }
                if (KickStart.OverrideRestrictions)
                {
                    //Debug_SMissions.Log(KickStart.ModID + ": Presenting mission " + mission.Name);
                    mission.GetAndSetDisplayName();
                    initMissionsCache.Add(mission);
                    continue;
                }
                if (GetTreeCorp(mission.Faction) == (FactionSubTypes)(-1))
                {
                    SMUtil.Error(false, mission.Name, "Corp \"" + mission.Faction + "\" is not loaded.  Mission cannot load.");
                    continue;
                }
                if (mission.MinProgressX > ProgressX)
                {
                    //Debug_SMissions.Log(KickStart.ModID + ": " + mission.Name + " - not enough " + mission.Tree.ProgressXName + ".");
                    continue;
                }
                if (mission.MinProgressY > ProgressY)
                {
                    //Debug_SMissions.Log(KickStart.ModID + ": " + mission.Name + " - not enough " + mission.Tree.ProgressYName + ".");
                    continue;
                }
                try
                {
                    FactionLicense licence = Singleton.Manager<ManLicenses>.inst.GetLicense(GetTreeCorp());
                    if (licence.IsDiscovered && licence.CurrentLevel >= mission.GradeRequired)
                    {
                        mission.GetAndSetDisplayName();
                        Debug_SMissions.Log(KickStart.ModID + ": Presenting mission " + mission.AltName);
                        initMissionsCache.Add(mission);
                    }
                }
                catch
                {
                    //Debug_SMissions.Log(KickStart.ModID + ": " + mission.Name + " is not available right now");
                    continue;
                }
            }
            if (KickStart.OverrideRestrictions)
            {
                foreach (SubMissionStandby mission in ImmedeateMissions)
                {
                    //Debug_SMissions.Log(KickStart.ModID + ": Trying to validate mission " + mission.Name);
                    int hashName = mission.Name.GetHashCode();
                    if (ActiveMissions.Exists(delegate (SubMission cand) { return cand.Name.GetHashCode() == hashName; }))
                    {   // It's already in the list
                        //Debug_SMissions.Log(KickStart.ModID + ": " + mission.Name + " is already active");
                        continue;
                    }
                    Debug_SMissions.Log(KickStart.ModID + ": Presenting mission " + mission.Name);
                    mission.GetAndSetDisplayName();
                    initMissionsCache.Add(mission);
                }
            }
            return initMissionsCache;
        }


        internal static ManOnScreenMessages.Speaker LeftCustomSpeaker = (ManOnScreenMessages.Speaker)
            Enum.GetValues(typeof(ManOnScreenMessages.Speaker)).Length;
        internal static ManOnScreenMessages.Speaker RightCustomSpeaker = (ManOnScreenMessages.Speaker)
            (Enum.GetValues(typeof(ManOnScreenMessages.Speaker)).Length + 1);
        public Texture2D GetSpeakerTex(string name)
        {
            if (ManSubMissions.Speakers.TryGetValue(name, out var val))
                return val.Key;
            else
            {
                if (MissionTextures.TryGetValue(name.Replace(".png", "") + ".png", out Texture tex))
                    return (Texture2D)tex;
            }
            return ManUI.inst.m_SpriteFetcher.GetSprite(ObjectTypes.Null, 0).texture;
        }
        public ManOnScreenMessages.Speaker GetSpeaker(string name, bool rightSide)
        {
            if (ManSubMissions.Speakers.TryGetValue(name, out var val))
                return val.Value;
            else
            {
                if (MissionTextures.TryGetValue(name.Replace(".png", "") + ".png", out Texture tex))
                    return ManSubMissions.GenerateSpeaker(name, (Texture2D)tex, rightSide);
            }
            return ManOnScreenMessages.Speaker.GSOGeneric;
        }


        // COMPILER
        private List<SubMission> DeployAllMissions(string treeName, List<SubMissionStandby> toDeploy)
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
                SMUtil.Error(false, "Mission (Startup) ~ " + toDeploy.Name, "<b> CRITICAL ERROR IN HANDLING " + 
                    toDeploy.Name + " of tree " + treeName + " - UNABLE TO IMPORT ANY INFORMATION! </b>");
                return false;
            }
            Deployed.SelectedAltName = toDeploy.AltName;
            Deployed.Description = toDeploy.Desc;
            Deployed.Type = toDeploy.Type;
            Deployed.FakeEncounter = Enc;
            return true;
        }
        internal static List<SubMissionStandby> CompileToStandby(IEnumerable<SubMission> MissionsLoaded)
        {   // Reduce memory loads
            List<SubMissionStandby> missions = new List<SubMissionStandby>();
            foreach (SubMission mission in MissionsLoaded)
            {
                missions.Add(CompileToStandby(mission));
            }
            return missions;
        }
        internal static SubMissionStandby CompileToStandby(SubMission mission)
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

                if (mission.VarIntsActive.Count > listEle.GlobalIndex)
                    listEleC.GlobalIndex = mission.VarIntsActive[listEle.GlobalIndex];
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
            Debug_SMissions.Log(KickStart.ModID + ": Finished mission " + finished.Name + " of Tree " + TreeName + ".");
            int hashName = finished.Name.GetHashCode();
            if (RepeatMissions.Exists(delegate (SubMissionStandby cand) { return cand.Name.GetHashCode() == hashName; }))
            {   // Do nothing special - repeat missions are to be repeated
            }
            else if (Missions.Exists(delegate (SubMissionStandby cand) { return cand.Name.GetHashCode() == hashName; }))
            {
                CompletedMissions.Add(CompileToStandby(finished));
            }
            else
                Debug_SMissions.Log(KickStart.ModID + ": Tried to finish mission " + finished.Name + " that's not listed in this tree!  Tree " + TreeName);
            if (!ActiveMissions.Remove(finished))
                Debug_SMissions.Exception(KickStart.ModID + ": Tried to finish mission " + finished.Name + " but it doesn't exist in the ActiveMissions list!  Tree " + TreeName);

            ManSubMissions.Selected = ActiveMissions.FirstOrDefault();
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
                    UnloadTreeMission(ActiveMissions.FirstOrDefault());
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
                    Debug_SMissions.Log(KickStart.ModID + ": Called wrong tree [" + TreeName + "] for mission " + Active.Name + " on UnloadTreeMission!");
            }
            catch
            {
                Debug_SMissions.Log(KickStart.ModID + ": UnloadTreeMission - Could not unload mission!  Mission " + Active.Name + " of Tree" + TreeName);
            }
        }

    }
}
