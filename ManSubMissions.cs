using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;
using Sub_Missions.ManWindows;
using Sub_Missions.Steps;
using Newtonsoft.Json;
#if !STEAM
using Nuterra.BlockInjector;
#endif

namespace Sub_Missions
{
    /*
     * A different kind of mission system from vanilla, the SubMissions handles the following:
     *    Instead of setpieces, this uses the ModularMonuments system, which is an older concept with
     *      newer execution.  The way it's setup allows it to be totally modular and even randomly
     *      generate the mission on player will, but the catch is that it cannot modify terrain.
     *  Post: https://forum.terratechgame.com/index.php?threads/modular-monument-terrain-structures-and-ideas-for-them.13688/
     *  
     *    Techs will be spawned and the world will try to save the mission instance based on order of 
     *      queued and completed events.  Dialgue exact position is not saved but "chunks" if progression will be
     *      
     *  
     *  ---------------------------------------------------------------------------------------------------
     *     CustomSubMission
     *       EventList - handles the Steps in order from top to bottom, and repeats if nesseary
     *       
     *       There are variables you can add and reference around the case of the entire mission;
     *       VarTrueFalse, VarInts, VarFloats can be called and pooled later on
     *       Proper syntax for this would be:
     *       "VarTrueFalse" :{
     *          false, // Is Target destroyed?
     *          false, // PlayerIsAlive
     *          false, // PlayerIsAlive
     *          true,  // PlayerIsAlive
     *       }
     *       
     *      Variables with "Global" attached to the beginning:
     *       To re-reference these (Entire-SubMission level Varibles), in the step's trigger, make sure to 
     *       reference the Zero-Based-Index in the respective mission slot to reference the variable.
     *       Zero-Based-Index [0,1,2]  (normal is [1,2,3])
     *       
     *       Each Step has a Progress ID, which tells the SubMission where to iterate to.
     *         When a branch ID is set, the values adjacent of it will still be triggered
     *         this it to allow some keep features to still work like slightly changing the ProgressID to deal with
     *         players leaving the mission area
     *         If the CurrentProgressID is '1', the mission will run the Steps in [0,1,2]
     *         
     *         Steps with a capital "S" at the end can offset Step. It is suggested that you only use one step with
     *         per ProgressID.
     *         
     *         On success, the CurrentProgressID will be set to -98 and do one last loop.
     *         On fail, the CurrentProgressID will be set to -100 and do one last loop.
     *         
     *         If a Step's ProgressID is set to SubMission.alwaysRunValue, it will update all the time regardless of the CurrentProgressID.
     *     
     */
    /// <summary>
    /// The saving is handled within SaveManSubMissions.
    /// <para></para>
    /// Most of the methods in the classes in this mod are internal or private because invoking them out of order 
    /// may break this mod entirely.
    /// </summary>
    public class ManSubMissions : MonoBehaviour, IWorldTreadmill
    {   // Handle non-Payload missions here
        internal static ManSubMissions inst;

        internal static bool Active = false;

        internal static bool Subscribed = false;
        internal static bool SelectedIsAnon = false;
        internal static bool IgnoreSaveThisSession = false;

        internal static List<SubMissionTree> SubMissionTrees = new List<SubMissionTree>();


        internal static List<SubMission> activeSubMissions = new List<SubMission>();
        internal static List<SubMission> ActiveSubMissions
        {
            get
            {
                activeSubMissions.Clear();
                foreach (SubMissionTree tree in SubMissionTrees)
                    activeSubMissions.AddRange(tree.ActiveMissions);
                return activeSubMissions;
            }
        }

        // Ignore the Message here as making it auto will break JSON saving
        private static List<SubMissionStandby> anonSubMissions = new List<SubMissionStandby>();
        internal static List<SubMissionStandby> AnonSubMissions => anonSubMissions;

        internal static SubMission Selected;
        internal static SubMissionStandby SelectedAnon;

        internal static GUIPopupDisplay Button;
        internal static GUISMissionsList Board;
        internal static GUIPopupDisplay SideUI;

        private static float timer = 0;
        private static float timerSecondsDelay = 1;

        public static Dictionary<int, GameObject> WorldObjects => ManModularMonuments.WorldObjects;



        public const float MinLoadedSpawnDist = 250;
        public const float MaxLoadedSpawnDist = 400;
        public const float LoadCheckDist = 600;
        public const float MaxUnloadedSpawnDist = 1250;


        // Setup
        internal static void Initiate()
        {
            if (!inst)
            {
                inst = Instantiate(new GameObject("ManSubMissions")).AddComponent<ManSubMissions>();
                Debug.Log("SubMissions: ManSubMissions initated");
            }
            Active = true;
        }
        internal static void Subscribe()
        {
            if (!Subscribed)
            {
                KickStart.FullyLoadedGame = true;
                Singleton.Manager<ManGameMode>.inst.ModeStartEvent.Subscribe(ModeLoad);
                Singleton.Manager<ManGameMode>.inst.ModeFinishedEvent.Subscribe(ModeFinished);
                Singleton.Manager<ManTechs>.inst.TankDestroyedEvent.Subscribe(BroadcastTechDeath);
                Singleton.Manager<ManWorld>.inst.TileManager.TilePopulatedEvent.Subscribe(OnTileLoaded);
                Singleton.Manager<ManWorld>.inst.TileManager.TileDepopulatedEvent.Subscribe(OnTileUnloaded);
                Singleton.Manager<ManWorldTreadmill>.inst.AddListener(inst);
                Debug.Log("SubMissions: Core module hooks launched");
                WindowManager.LateInitiate();

                WindowManager.AddPopupButton("", "<b>SMissions</b>", false, "Master", windowOverride: WindowManager.MicroWindow);

                if (KickStart.Debugger)
                    WindowManager.ShowPopup(new Vector2(0.8f, 1));

                Button = WindowManager.GetCurrentPopup();

                WindowManager.AddPopupMissionsList();

                WindowManager.AddPopupMessageSide();

                if (KickStart.Debugger)
                    WindowManager.ShowPopup(new Vector2(1, 0.1f));

                SideUI = WindowManager.GetCurrentPopup();

                Debug.Log("SubMissions: ManSubMissions subscribed");
                ManSMCCorps.Subscribe();
                Subscribed = true;
            }
        }
        internal static void DeInit()
        {
            if (Subscribed)
            {
                PurgeAllTrees();
                ManSMCCorps.DeInit();
                WindowManager.DeInit();
                Singleton.Manager<ManWorldTreadmill>.inst.RemoveListener(inst);
                Singleton.Manager<ManGameMode>.inst.ModeStartEvent.Unsubscribe(ModeLoad);
                Singleton.Manager<ManGameMode>.inst.ModeFinishedEvent.Unsubscribe(ModeFinished);
                Singleton.Manager<ManTechs>.inst.TankDestroyedEvent.Unsubscribe(BroadcastTechDeath);
                Singleton.Manager<ManWorld>.inst.TileManager.TilePopulatedEvent.Unsubscribe(OnTileLoaded);
                Singleton.Manager<ManWorld>.inst.TileManager.TileDepopulatedEvent.Unsubscribe(OnTileUnloaded);
                KickStart.FullyLoadedGame = false;
                Debug.Log("SubMissions: Core module hooks removed");

                Debug.Log("SubMissions: ManSubMissions De-Init");
                Subscribed = false;
            }
            Active = false;
        }

        private static int CustCorpStartID = 16;
       
        internal void HarvestAllTrees()
        {
            if (!Active)
                return;
            Debug.Log("SubMissions: HARVESTING ALL TREES!!!");
            SubMissionTrees.Clear();
            List<SubMissionTree> trees = SMissionJSONLoader.LoadAllTrees();
            foreach (SubMissionTree tree in trees)
            {
                if (tree.CompileMissionTree(out SubMissionTree treeOut))
                {
                    SubMissionTrees.Add(treeOut);
                    Debug.Log("SubMissions: Missions count " + tree.Missions.Count + " | " + tree.RepeatMissions.Count);
                }
                else
                    SMUtil.Assert(false, "SubMissions: Failed to compress tree " + tree.TreeName + " properly, unable to push to ManSubMissions...");
            }
            SMissionJSONLoader.BuildAllWorldObjects(trees);
            GetAllPossibleMissions();
        }
        internal void GetAllPossibleMissions()
        {
            Debug.Log("SubMissions: Fetching available missions...");
            anonSubMissions.Clear();
            foreach (SubMissionTree tree in SubMissionTrees)
            {
                anonSubMissions.AddRange(tree.GetReachableMissions());
            }
            UpdateImmediateMissions();
            //SaveSubMissionsToSave();
        }
        internal void UpdateImmediateMissions()
        {
            if (KickStart.OverrideRestrictions)
                return;
            Debug.Log("SubMissions: Fetching available ImmediateMissions...");
            foreach (SubMissionTree tree in SubMissionTrees)
            {
                List<SubMissionStandby> nowMissions = tree.GetImmediateMissions();

                foreach (SubMissionStandby nM in nowMissions)
                {
                    Debug.Log("SubMissions: Forcing mission " + nM.AltName + " to active");
                    tree.AcceptTreeMission(nM);
                }
            }
        }

        public void OnMoveWorldOrigin(IntVector3 moveDist)
        {
            foreach (SubMission step in activeSubMissions)
            {
                step.OnMoveWorldOrigin(moveDist);
            }
        }
        internal static void OnTileLoaded(WorldTile WT)
        {
            ManModularMonuments.LoadAllAtTile(WT.Coord);
            foreach (SubMission step in activeSubMissions)
            {
                if (step.ActiveState == SubMissionLoadState.NeedsFirstInit)
                {
                    step.CheckIfCanSpawn();
                }
                else if (WT.Coord == step.TilePos)
                    if (!step.IsActive || step.IgnorePlayerProximity)
                        step.CheckForReSync();
            }
        }
        internal static void OnTileUnloaded(WorldTile WT)
        {
            foreach (SubMission step in activeSubMissions)
            {
                if (WT.Coord == step.TilePos)
                    if (step.IsActive && !step.IgnorePlayerProximity)
                        step.PauseAndUnload();
            }
            ManModularMonuments.UnloadAllAtTile(WT.Coord);
        }

        public static SubMissionTree GetTree(string treeName)
        {
            SubMissionTree tree = SubMissionTrees.Find(delegate (SubMissionTree cand) { return cand.TreeName == treeName; });
            if (tree == default(SubMissionTree))
                SMUtil.Assert(false, "SubMissions: Failed to fetch tree - Was it removed!?");
            return tree;
        }

        // Missions
        internal void AcceptMission()
        {   // We bite the bullet and insure the file has been marked tampered with - because it was
            Singleton.Manager<ManSaveGame>.inst.CurrentState.m_FileHasBeenTamperedWith = true;
            SelectedAnon.Tree.AcceptTreeMission(SelectedAnon);
            SelectedIsAnon = false;
        }
        internal void CancelMission()
        {
            Selected.Tree.CancelTreeMission(Selected);
        }
        internal static void ReSyncSubMissions()
        {
            foreach (SubMission sub in ActiveSubMissions)
            {
                sub.CheckForReSync();
            }
        }

        //Does not account for missions larger than two tiles - but if you have a mission bigger than 2 tiles,
        // you probably already have a bunch of more problems...
        internal static bool IsTooCloseToOtherMission(IntVector2 tileWorld)
        {  
            List<SubMissionStandby> SMS = GetAllFinishedMissions();

            IntVector2 tWIU = tileWorld + (IntVector2.one * 2);
            foreach (SubMission sub in ActiveSubMissions)
            {
                IntVector2 tWI = sub.TilePos;
                if (tWI.x <= tWIU.x && tWI.x >= -tWIU.x && tWI.y <= tWIU.y && tWI.y >= -tWIU.y)
                {
                    return true;
                }
            }
            foreach (SubMissionStandby sub in SMS)
            {
                IntVector2 tWI = sub.TilePosWorld;
                if (tWI.x <= tWIU.x && tWI.x >= -tWIU.x && tWI.y <= tWIU.y && tWI.y >= -tWIU.y)
                {
                    return true;
                }
            }
            return false;
        }
        public static List<SubMissionStandby> GetAllFinishedMissions()
        {
            List<SubMissionStandby> SMS = new List<SubMissionStandby>();
            foreach (SubMissionTree sub in SubMissionTrees)
            {
                SMS.AddRange(sub.CompletedMissions);
            }
            return SMS;
        }


        // 
        internal static void ToggleList()
        {
            if (Board.Display.isOpen)
            {
                WindowManager.HidePopup(Board.Display);
            }
            else
            {
                WindowManager.ShowPopup(new Vector2(0.5f, 0.5f), Board.Display);
            }
        }
        private void CheckKeyCombos()
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.M))
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    Debug.Log("SubMissions: Key combination pressed!!");
                    //ManSMCCorps.ReloadAllCorps(); // Disabled as it's broken... for now.
                    HarvestAllTrees();
                    LoadSubMissionsFromSave();
                    ManSMCCorps.ReloadSkins();
                }
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    SMUtil.PushErrors();
                }
            }
        }

        // UPDATE
        private void Update()
        {

            if (SMCCorpLicense.needsToRenderSkins)
            {
                SMCCorpLicense.TryReRenderCSIBacklog();
            }

            UpdateAllSubMissions();
            timer += Time.deltaTime;
            if (timer >= timerSecondsDelay)
            {
                timer = 0;

                foreach (SubMission step in activeSubMissions)
                    step.UpdateDistance();

                if (!ActiveSubMissions.Contains(Selected))
                    Selected = null;
                if (!AnonSubMissions.Contains(SelectedAnon))
                    SelectedAnon = null;
            }
            CheckKeyCombos();
        }
        private void UpdateAllSubMissions()
        {
            try
            {
                foreach (SubMission sub in ActiveSubMissions)
                {
                    sub.TriggerUpdate();
                }
            }
            catch { } //Probably just a mission cleaning up...
        }

        private static void PurgeAllTrees()
        {
            if (inst)
            {
                inst.GetAllPossibleMissions();
                foreach (SubMissionTree tree in SubMissionTrees)
                {
                    tree.ResetALLTreeMissions();
                }
                EncounterShoehorn.DestroyAllFakeEncounters();
                inst.GetAllPossibleMissions();
            }
        }
        internal static void BroadcastTechDeath(Tank techIn, ManDamage.DamageInfo oof)
        {
            //Debug.Log("SubMissions: Tech " + techIn.name + " of ID " + techIn.visible.ID + " was destroyed");
            foreach (SubMissionTree tree in SubMissionTrees)
            {
                foreach (SubMission mission in tree.ActiveMissions)
                {
                    try
                    {
                        foreach (TrackedTech tech in mission.TrackedTechs)
                        {
                            try
                            {
                                tech.CheckWasDestroyed(techIn);
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
            }
        }


        //Saving
        private static void ClearAllActiveSubMissionsForUnload()
        {
            foreach (SubMission SM in activeSubMissions)
            {
                SM.Cleanup(true);
            }
            activeSubMissions.Clear();
        }
        public static List<SubMission> GetAllActiveSubMissions()
        {
            return activeSubMissions;
        }
        private static void ModeLoad(Mode mode)
        {
            if ((mode is ModeMain && KickStart.Debugger) || mode is ModeMisc)
            {
                IgnoreSaveThisSession = false;
                Debug.Log("SubMissions: ManSubMissions Loading from save!");
                Debug_SMissions.Assert(inst == null, "ManSubMissions IS NULL");
                PurgeAllTrees();
                SaveManSubMissions.LoadDataAutomatic();
                inst.GetAllPossibleMissions();
                Debug_SMissions.Assert(Button == null, "UI Mission menu Button is null");
                WindowManager.ShowPopup(new Vector2(0.8f, 1), Button);
                Debug_SMissions.Assert(SideUI == null, "UI Mission side panel is null");
                WindowManager.ShowPopup(new Vector2(1, 0.1f), SideUI);
            }
            else
            {
                if (!KickStart.Debugger)
                {
                    WindowManager.HidePopup(Button);
                    WindowManager.HidePopup(SideUI);
                }
            }
        }
        private static void ModeFinished(Mode mode)
        {
            if (mode is ModeMain && !IgnoreSaveThisSession)
            {
                var saver = Singleton.Manager<ManSaveGame>.inst;
                if (saver.IsSaveNameAutoSave(saver.GetCurrentSaveName(false)))
                {
                    Debug.Log("SubMissions: ManSubMissions Saving!");
                    SaveManSubMissions.SaveDataAutomatic();
                }
            }
            ClearAllActiveSubMissionsForUnload();
            ManModularMonuments.PurgeAllActive();
        }
        private static void SaveSubMissionsToSave()
        {
            try
            {
                if (Singleton.Manager<ManGameMode>.inst.IsCurrent<ModeMain>() && !IgnoreSaveThisSession)
                {
                    var saver = Singleton.Manager<ManSaveGame>.inst;
                    if (saver.IsSaveNameAutoSave(saver.GetCurrentSaveName(false)))
                    {
                        Debug.Log("SubMissions: ManSubMissions Saving!");
                        SaveManSubMissions.SaveDataAutomatic();
                    }
                }
            }
            catch { }
        }
        private static void LoadSubMissionsFromSave()
        {
            if (Singleton.Manager<ManGameMode>.inst.IsCurrent<ModeMain>() && !IgnoreSaveThisSession)
            {
                IgnoreSaveThisSession = false;
                Debug.Log("SubMissions: ManSubMissions Loading from save!");
                PurgeAllTrees();
                SaveManSubMissions.LoadDataAutomatic();
                inst.GetAllPossibleMissions();
                WindowManager.ShowPopup(new Vector2(0.8f, 1), Button);
                WindowManager.ShowPopup(new Vector2(1, 0.1f), SideUI);
            }
            else
            {
                if (!KickStart.Debugger)
                {
                    WindowManager.HidePopup(Button);
                    WindowManager.HidePopup(SideUI);
                }
            }
        }


        // testing

    }
}
