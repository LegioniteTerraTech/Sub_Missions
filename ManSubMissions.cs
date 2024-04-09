using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;
using Sub_Missions.ManWindows;
using Sub_Missions.Steps;
using Sub_Missions.ModularMonuments;
using Newtonsoft.Json;
using System.Reflection;
using static HarmonyLib.Code;
using System.IO;
using TerraTechETCUtil;
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
    {   // Handle non-UScript missions here
        internal static ManSubMissions inst;

        internal static bool Active = false;

        internal static bool Subscribed = false;
        internal static bool SelectedIsAnon = false;
        internal static bool IgnoreSaveThisSession = false;

        public static bool BlockMissionEnd => Editor.LockEnding;
        public static bool SlowMo => Editor.SlowMode;
        public static bool Stopped => Editor.Paused;


        internal static List<KeyValuePair<string, KeyValuePair<int, int>>> SavedModLicences = new List<KeyValuePair<string, KeyValuePair<int, int>>>();

        internal static List<SubMissionTree> SubMissionTrees = new List<SubMissionTree>();


        internal static List<SubMission> activeSubMissionsCached = new List<SubMission>();
        internal static List<SubMission> GetActiveSubMissions
        {
            get
            {
                activeSubMissionsCached.Clear();
                foreach (SubMissionTree tree in SubMissionTrees)
                    if (tree.ActiveMissions != null)
                        activeSubMissionsCached.AddRange(tree.ActiveMissions);
                return activeSubMissionsCached;
            }
        }

        // Ignore the Message here as making it auto will break JSON saving
        private static List<SubMissionStandby> anonSubMissions = new List<SubMissionStandby>();
        internal static List<SubMissionStandby> AnonSubMissions => anonSubMissions;

        internal static SubMission Selected;
        internal static SubMissionStandby SelectedAnon;

        internal static GUIPopupDisplay Button;
        internal static GUISMissionsList Board;
        internal static GUISMissionEditor Editor;
        internal static GUIPopupDisplay SideUI;

        private static float timer = 0;
        private static float timerSecondsDelay = 1;


        public static Event<SubMission> MissionStartedEvent = new Event<SubMission>();
        public static Event<SubMission, ManEncounter.FinishState> MissionFinishedEvent = new Event<SubMission, ManEncounter.FinishState>();

        public static Dictionary<string, KeyValuePair<Texture2D, ManOnScreenMessages.Speaker>> Speakers = 
            new Dictionary<string, KeyValuePair<Texture2D, ManOnScreenMessages.Speaker>>();

        public const float MinLoadedSpawnDist = 250;
        public const float MaxLoadedSpawnDist = 400;
        public const float LoadCheckDist = 600;
        public const float MaxUnloadedSpawnDist = 1250;


        private static FieldInfo speakersMain = typeof(ManOnScreenMessages).GetField("m_SpeakerData",
            BindingFlags.NonPublic | BindingFlags.Instance);
        private static Localisation.GlyphInfo[] emptyGlyphs = new Localisation.GlyphInfo[0];

        internal static ManOnScreenMessages.Speaker LeftCustomSpeaker = (ManOnScreenMessages.Speaker)
            Enum.GetValues(typeof(ManOnScreenMessages.Speaker)).Length;
        internal static ManOnScreenMessages.Speaker RightCustomSpeaker = (ManOnScreenMessages.Speaker)
            (Enum.GetValues(typeof(ManOnScreenMessages.Speaker)).Length + 1);
        private static ManOnScreenMessages.SpeakerData[] AllSpeakers = null;

        internal static void ExtendSpeakers()
        {
            if (AllSpeakers == null)
            {
                try
                {
                    AllSpeakers = (ManOnScreenMessages.SpeakerData[])speakersMain.GetValue(ManOnScreenMessages.inst);

                    int defaultIndex = (int)ManOnScreenMessages.Speaker.GSOGeneric;
                    for (int step = defaultIndex; step < AllSpeakers.Length; step++)
                    {
                        string name = ((ManOnScreenMessages.Speaker)step).ToString();
                        Speakers.Add(name, new KeyValuePair<Texture2D, ManOnScreenMessages.Speaker>(
                                AllSpeakers[step].m_SpeakerImage.texture, (ManOnScreenMessages.Speaker)step));
                        Debug_SMissions.Log(KickStart.ModID + ": Fetched speaker " + name);
                    }

                    Array.Resize(ref AllSpeakers, (int)RightCustomSpeaker + 1);
                    AllSpeakers[(int)LeftCustomSpeaker] = new ManOnScreenMessages.SpeakerData
                    {
                        m_BehindSpeakerImage = AllSpeakers[defaultIndex].m_BehindSpeakerImage,
                        m_InFrontOfSpeakerImage = AllSpeakers[defaultIndex].m_InFrontOfSpeakerImage,
                        m_SpeakerTitle = new LocalisedString
                        {
                            m_Bank = "NULL",
                            m_Id = "MOD",
                            m_GUIExpanded = true,
                            m_InlineGlyphs = emptyGlyphs,
                        },
                        m_SpeakerImage = AllSpeakers[defaultIndex].m_SpeakerImage,
                    };
                    AllSpeakers[(int)RightCustomSpeaker] = new ManOnScreenMessages.SpeakerData
                    {
                        m_BehindSpeakerImage = AllSpeakers[defaultIndex].m_BehindSpeakerImage,
                        m_InFrontOfSpeakerImage = AllSpeakers[defaultIndex].m_InFrontOfSpeakerImage,
                        m_SpeakerTitle = new LocalisedString
                        {
                            m_Bank = "NULL",
                            m_Id = "MOD",
                            m_GUIExpanded = true,
                            m_InlineGlyphs = emptyGlyphs,
                        },
                        m_SpeakerImage = AllSpeakers[defaultIndex].m_SpeakerImage,
                    };

                    speakersMain.SetValue(ManOnScreenMessages.inst, AllSpeakers);
                }
                catch (Exception e)
                {
                    Debug_SMissions.FatalError(e.ToString());
                }
            }
        }

        public static ManOnScreenMessages.Speaker GenerateSpeaker(string name, Texture2D image, bool rightSide)
        {
            int selector;
            if (rightSide)
                selector = (int)RightCustomSpeaker;
            else
                selector = (int)LeftCustomSpeaker;

            var speak = AllSpeakers[selector];

            speak.m_SpeakerTitle.m_Bank = name;
            speak.m_SpeakerImage = Sprite.Create(image, 
                new Rect(0, 0, image.width, image.height), Vector2.zero);

            AllSpeakers[selector] = speak;

            return (ManOnScreenMessages.Speaker)selector;
        }

        static void AddHooksToActiveGameInterop()
        {
            if (ActiveGameInterop.OnRecieve.ContainsKey("OpenMissionEditor"))
                return;
            ActiveGameInterop.OnRecieve.Add("OpenMissionEditor", (string x) =>
            {
                /*
                ActiveGameInterop.TryTransmit("RetreiveTechPop", Path.Combine(RawTechsDirectory,
                    "Bundled", "RawTechs.RTList"));
                */
            });
        }

        // Setup
        internal static void Initiate()
        {
            if (!inst)
            {
                inst = Instantiate(new GameObject("ManSubMissions")).AddComponent<ManSubMissions>();
                Debug_SMissions.Log(KickStart.ModID + ": ManSubMissions initated");
                TerraTechETCUtil.ResourcesHelper.PostBlocksLoadEvent.Subscribe(ExtendSpeakers);
                TerraTechETCUtil.WikiPageCorp.GetCorpDescription = ManSMCCorps.GetCorpLores;
                TerraTechETCUtil.WikiPageCorp.OnWikiPageMade.Subscribe(ManSMCCorps.GetCorpLoresExtended);
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
                Debug_SMissions.Log(KickStart.ModID + ": Core module hooks launched");
                //WindowManager.LateInitiate();

                //WindowManager.AddPopupButton("", "<b>SMissions</b>", false, "Master", windowOverride: WindowManager.MicroWindow);
                WindowManager.AddPopupMissionsDEVControl();

                if (KickStart.Debugger)
                    WindowManager.ShowPopup(new Vector2(0.8f, 1));

                //TerraTechETCUtil.DebugExtUtilities.AllowEnableDebugGUIMenu_KeypadEnter = KickStart.Debugger;

                Button = WindowManager.GetCurrentPopup();

                WindowManager.AddPopupMissionsList();
                WindowManager.AddPopupMissionEditor();

                WindowManager.AddPopupMessageSide();

                if (KickStart.Debugger)
                    WindowManager.ShowPopup(new Vector2(1, 0.1f));

                SideUI = WindowManager.GetCurrentPopup();

                Debug_SMissions.Log(KickStart.ModID + ": ManSubMissions subscribed");
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
                //WindowManager.DeInit();
                Singleton.Manager<ManWorldTreadmill>.inst.RemoveListener(inst);
                Singleton.Manager<ManGameMode>.inst.ModeStartEvent.Unsubscribe(ModeLoad);
                Singleton.Manager<ManGameMode>.inst.ModeFinishedEvent.Unsubscribe(ModeFinished);
                Singleton.Manager<ManTechs>.inst.TankDestroyedEvent.Unsubscribe(BroadcastTechDeath);
                Singleton.Manager<ManWorld>.inst.TileManager.TilePopulatedEvent.Unsubscribe(OnTileLoaded);
                Singleton.Manager<ManWorld>.inst.TileManager.TileDepopulatedEvent.Unsubscribe(OnTileUnloaded);
                KickStart.FullyLoadedGame = false;
                Debug_SMissions.Log(KickStart.ModID + ": Core module hooks removed");

                Debug_SMissions.Log(KickStart.ModID + ": ManSubMissions De-Init");
                Subscribed = false;
            }
            Active = false;
        }

       
        internal void ReloadAllMissionTrees()
        {
            if (!Active)
                return;
            Debug_SMissions.Log(KickStart.ModID + ": HARVESTING ALL TREES!!!");
            SubMissionTrees.Clear();
            Selected = null;
            SelectedAnon = null;
            SelectedIsAnon = true;
            List<SubMissionTree> trees = SMissionJSONLoader.LoadAllTrees();
            foreach (SubMissionTree tree in trees)
            {
                try
                {
                    tree.CompileMissionTree(out SubMissionTree treeOut);
                    SubMissionTrees.Add(treeOut);
                    Debug_SMissions.Log(KickStart.ModID + ": Missions count " + tree.Missions.Count + " | " + tree.RepeatMissions.Count);
                }
                catch (Exception e)
                {
                    SMUtil.Assert(false, "Mission Tree (Startup) ~ " + tree.TreeName, KickStart.ModID + ": Failed to compress tree " + tree.TreeName + " properly, unable to push to ManSubMissions...", e);
                }
            }
            MM_JSONLoader.BuildAllWorldObjects(trees);
            GetAllPossibleMissions();
        }
        internal void ReloadMissionTree(SubMissionTree treeC)
        {
            if (!Active)
                return;
            SubMissionTrees.Remove(treeC);
            List<SubMissionTree> trees = SMissionJSONLoader.LoadAllTrees();
            foreach (SubMissionTree tree in trees)
            {
                try
                {
                    tree.CompileMissionTree(out SubMissionTree treeOut);
                    SubMissionTrees.Add(treeOut);
                    Debug_SMissions.Log(KickStart.ModID + ": Missions count " + tree.Missions.Count + " | " + tree.RepeatMissions.Count);
                }
                catch (Exception e)
                {
                    SMUtil.Assert(false, "Mission Tree (Startup) ~ " + tree.TreeName, KickStart.ModID + ": Failed to compress tree " + tree.TreeName + " properly, unable to push to ManSubMissions...", e);
                }
            }
            MM_JSONLoader.BuildAllWorldObjects(trees);
            GetAllPossibleMissions();
        }
        internal void GetAllPossibleMissions()
        {
            Debug_SMissions.Assert(true, KickStart.ModID + ": Fetching available missions...");
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
            Debug_SMissions.Log(KickStart.ModID + ": Fetching available ImmediateMissions...");
            foreach (SubMissionTree tree in SubMissionTrees)
            {
                foreach (SubMissionStandby nM in tree.GetImmediateMissions())
                {
                    Debug_SMissions.Log(KickStart.ModID + ": Forcing mission " + nM.AltName + " to active");
                    tree.AcceptTreeMission(nM, false);
                }
            }
        }

        public void OnMoveWorldOrigin(IntVector3 moveDist)
        {
            foreach (SubMission step in GetActiveSubMissions)
            {
                step.OnMoveWorldOrigin(moveDist);
            }
        }
        internal static void OnTileLoaded(WorldTile WT)
        {
            ManModularMonuments.LoadAllAtTile(WT.Coord);
            foreach (SubMission step in GetActiveSubMissions)
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
            foreach (SubMission step in GetActiveSubMissions)
            {
                if (WT.Coord == step.TilePos)
                    if (step.IsActive && !step.IgnorePlayerProximity)
                        step.PauseAndUnload();
            }
            ManModularMonuments.UnloadAllAtTile(WT.Coord);
        }

        public static SubMissionTree GetTree(string treeName)
        {
            if (treeName == null)
            {
                SMUtil.Error(false, "Mission Tree ~ NULL", KickStart.ModID + ": GetTree given null treeName");
                return null;
            }
            SubMissionTree tree = SubMissionTrees.Find(delegate (SubMissionTree cand) { return cand.TreeName == treeName; });
            if (tree == default(SubMissionTree))
                SMUtil.Error(false, "Mission Tree ~ " + treeName, KickStart.ModID + ": Failed to fetch tree - Was it removed!?");
            return tree;
        }

        // Missions
        internal void AcceptMission(Encounter Enc = null)
        {   // We bite the bullet and insure the file has been marked tampered with - because it was
            Singleton.Manager<ManSaveGame>.inst.CurrentState.m_FileHasBeenTamperedWith = true;
            SelectedAnon.Tree.AcceptTreeMission(SelectedAnon, Enc);
            SelectedIsAnon = false;
        }
        internal void CancelMission()
        {
            Selected.Tree.CancelTreeMission(Selected);
        }
        internal static void ReSyncSubMissions()
        {
            foreach (SubMission sub in GetActiveSubMissions)
            {
                sub.CheckForReSync();
            }
        }

        //Does not account for missions larger than two tiles - but if you have a mission bigger than 2 tiles,
        // you probably already have a bunch of more problems...
        internal static bool IsTooCloseToOtherMission(IntVector2 tileWorld)
        {  
            IntVector2 tWIU = tileWorld + (IntVector2.one * 2);
            foreach (SubMission sub in GetActiveSubMissions)
            {
                IntVector2 tWI = sub.TilePos;
                if (tWI.x <= tWIU.x && tWI.x >= -tWIU.x && tWI.y <= tWIU.y && tWI.y >= -tWIU.y)
                {
                    return true;
                }
            }
            foreach (SubMissionStandby sub in GetAllFinishedMissions())
            {
                IntVector2 tWI = sub.TilePosWorld;
                if (tWI.x <= tWIU.x && tWI.x >= -tWIU.x && tWI.y <= tWIU.y && tWI.y >= -tWIU.y)
                {
                    return true;
                }
            }
            return false;
        }
        internal static bool IsSubMissionActive(string name)
        {
            foreach (SubMission sub in GetActiveSubMissions)
            {
                if (sub.Name.CompareTo(name) == 0)
                    return true;
            }
            return false;
        }
        public static List<Encounter> GetAllFakeEncounters()
        {
            List<Encounter> Encs = new List<Encounter>();
            foreach (SubMission sub in GetActiveSubMissions)
            {
                if (sub.FakeEncounter != null)
                    Encs.Add(sub.FakeEncounter);
            }
            return Encs;
        }
        private static List<SubMissionStandby> SMSCache = new List<SubMissionStandby>();
        public static List<SubMissionStandby> GetAllFinishedMissions()
        {
            SMSCache.Clear();
            foreach (SubMissionTree sub in SubMissionTrees)
            {
                SMSCache.AddRange(sub.CompletedMissions);
            }
            return SMSCache;
        }


        public static void RecycleAllDataForMissionsAndRefresh()
        {
            SaveManSubMissions.PurgeALL_SAVEDATA();
            while (GetActiveSubMissions.Any())
            {
                activeSubMissionsCached.First().Finish(true);
            }
            Selected = null;
            SelectedAnon = null;
            SelectedIsAnon = true;
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
        internal static void ToggleEditor()
        {
            if (Editor.Display.isOpen)
            {
                WindowManager.HidePopup(Editor.Display);
            }
            else
            {
                WindowManager.ShowPopup(new Vector2(0.5f, 0.5f), Editor.Display);
            }
        }
        private void CheckKeyCombos()
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.M))
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    Debug_SMissions.Log(KickStart.ModID + ": Key combination pressed!!  Reload Missions");
                    //ManSMCCorps.ReloadAllCorps(); // Disabled as it's broken... for now.
                    ReloadAllMissionTrees();
                    LoadSubMissionSaveData();
                    ManSMCCorps.ReloadUnofficialSkins();
                }
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    Debug_SMissions.Log(KickStart.ModID + ": Key combination pressed!!  All Errors Logged");
                    SMUtil.PushErrors();
                }
            }
        }

        // UPDATE
        private void Update()
        {
            try
            {
                if (SMCCorpLicense.needsToRenderSkins)
                {
                    SMCCorpLicense.TryReRenderCSIBacklog();
                }

                if (!SlowMo && !Stopped)
                    UpdateAllSubMissions();
                timer += Time.deltaTime;
                if (timer >= timerSecondsDelay)
                {
                    if (SlowMo && !Stopped)
                        UpdateAllSubMissionsStep(1);
                    timer = 0;

                    foreach (SubMission step in GetActiveSubMissions)
                        step.UpdateDistance();

                    if (!GetActiveSubMissions.Contains(Selected))
                        Selected = null;
                    if (!AnonSubMissions.Contains(SelectedAnon))
                        SelectedAnon = null;
                }
                CheckKeyCombos();
            }
            catch (MandatoryException e)
            {
                SMUtil.Error(true, "ManSubMissions - Entire System(EPIC FAILIURE)", "CATASTROPHIC FAILIURE: " + e);
                throw new Exception(KickStart.ModID + ": Critical Error within ManSubMissions.Update() hierachy!", e);
            }
            catch (Exception e)
            {
                Debug_SMissions.Log(KickStart.ModID + ": Minor Error within ManSubMissions.Update() hierachy! \n This is the mod developer's issue, please report it! " + e);
            }
        }
        private void UpdateAllSubMissions()
        {
            try
            {
                for (int i = 0; i < GetActiveSubMissions.Count; i++)
                {
                    SubMission sub = GetActiveSubMissions[i];
                    try
                    {
                        sub.TriggerUpdate(Time.deltaTime, int.MaxValue);
                    }
                    catch (MandatoryException e)
                    {
                        throw new MandatoryException(KickStart.ModID + ": Critical Error within ManSubMissions.UpdateAllSubMissions() hierachy for mission sub!", e);
                    }
                    catch (WarningException e)
                    {
                        Debug_SMissions.Log(KickStart.ModID + ": Error for SubMission of name " + (sub.Name.NullOrEmpty() ? "NULL NAME?!?" : sub.Name) +
                        ", of Tree " + (sub.Tree != null ? (sub.Tree.TreeName.NullOrEmpty() ? "NULL NAME?!?" : sub.Name) : "ENTIRE TREE NULL?!?") +
                        ".  " + e.StackTrace);
                        SMUtil.PushErrors();
                        enabled = false;

                    } //Probably just a mission cleaning up...
                }
            }
            catch (MandatoryException e)
            {
                enabled = false;
                GC.Collect();
                SMUtil.PushErrors();
                throw new MandatoryException("SubMissions Cascade failiure within ManSubMissions.UpdateAllSubMissions() hierachy! " +
                    "\n Some articles may have been corrupted!  \n  Shutting down ManSubMissions to prevent memory leak!", e);
            }
            catch (Exception e)
            {
                enabled = false;
                GC.Collect();
                SMUtil.PushErrors();
                throw new MandatoryException(KickStart.ModID + ": Catastrophic Failiure within ManSubMissions.UpdateAllSubMissions() hierachy! " +
                    "\n This is the mod developer's issue, please report it! \n  Shutting down ManSubMissions to prevent memory leak!", e);
            } //Probably just a mission cleaning up...
        }
        private void UpdateAllSubMissionsStep(int speed)
        {
            try
            {
                foreach (SubMission sub in GetActiveSubMissions)
                {
                    sub.TriggerUpdate(1, speed);
                }
            }
            catch { } //Probably just a mission cleaning up...
        }
        private void UpdateAllSubMissionsSlow(float speed)
        {
            try
            {
                foreach (SubMission sub in GetActiveSubMissions)
                {
                    sub.TriggerUpdate(1, speed);
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
                EncounterShoehorn.RecycleAllFakeEncounters();
                inst.GetAllPossibleMissions();
            }
        }
        internal static void BroadcastTechDeath(Tank techIn, ManDamage.DamageInfo oof)
        {
            //Debug_SMissions.Log(KickStart.ModID + ": Tech " + techIn.name + " of ID " + techIn.visible.ID + " was destroyed");
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

        internal static void UpdateSaveStateForCustomCorps()
        {
            if (inst)
                inst.UpdateSaveStateForCustomCorpsInternal();
        }
        private void UpdateSaveStateForCustomCorpsInternal()
        {
            SavedModLicences.Clear();
            foreach (var item in ManSMCCorps.GetAllSMCCorpFactionTypes())
            {
                if (ManLicenses.inst.IsLicenseDiscovered(item))
                {
                    FactionLicense FL = ManLicenses.inst.GetLicense(item);
                    if (FL != null && ManSMCCorps.TryGetSMCCorpLicense((int)item, out SMCCorpLicense CL))
                    {
                        KeyValuePair<int, int> gradeEXP = new KeyValuePair<int, int>(FL.CurrentLevel, FL.CurrentAbsoluteXP);
                        SavedModLicences.Add(new KeyValuePair<string, KeyValuePair<int, int>>(CL.Faction, gradeEXP));
                    }
                }
            }
        }

        //Saving
        private static void ClearAllActiveSubMissionsForUnload()
        {
            foreach (SubMission SM in GetActiveSubMissions)
            {
                SM.Cleanup(true);
            }
            foreach (SubMissionTree SMT in SubMissionTrees)
            {
                SMT.ActiveMissions.Clear();
            }
        }
        public static List<SubMission> GetAllActiveSubMissions()
        {
            return GetActiveSubMissions;
        }
        internal static void UpdateButtonState()
        {
            if (SubMissionsWiki.inst.ShowButtons)
            {
                Debug_SMissions.Assert(Button == null, "UI Mission menu Button is null");
                WindowManager.ShowPopup(new Vector2(0.8f, 1), Button);
                Debug_SMissions.Assert(SideUI == null, "UI Mission side panel is null");
                WindowManager.ShowPopup(new Vector2(1, 0.1f), SideUI);
            }
            else
            {
                WindowManager.HidePopup(Button);
                WindowManager.HidePopup(SideUI);
            }
        }
        private static void ModeLoad(Mode mode)
        {
            if ((mode is ModeMain && KickStart.Debugger) || mode is ModeMisc)
            {
                IgnoreSaveThisSession = false;
                Debug_SMissions.Log(KickStart.ModID + ": ManSubMissions Loading from save!");
                Debug_SMissions.Assert(inst == null, "ManSubMissions IS NULL");
                ManSMCCorps.ReloadAllOfficialIfApplcable();
                PurgeAllTrees();
                SaveManSubMissions.LoadDataAutomaticLegacy();
                ForceCreateNewLicences();
                LoadModdedLicencesInst();
                inst.GetAllPossibleMissions();
                if (SubMissionsWiki.inst.ShowButtons)
                {
                    Debug_SMissions.Assert(Button == null, "UI Mission menu Button is null");
                    WindowManager.ShowPopup(new Vector2(0.8f, 1), Button);
                    Debug_SMissions.Assert(SideUI == null, "UI Mission side panel is null");
                    WindowManager.ShowPopup(new Vector2(1, 0.1f), SideUI);
                }
            }
            else
            {
                if (!(KickStart.Debugger && SubMissionsWiki.inst.ShowButtons))
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
                    Debug_SMissions.Log(KickStart.ModID + ": ManSubMissions Saving!");
                    SaveManSubMissions.SaveDataAutomaticLegacy();
                }

            }
            UICCorpLicenses.DeInitALLFactionLicenseOfficialUI();
            try
            {
                ClearAllActiveSubMissionsForUnload();
                ManModularMonuments.PurgeAllActive();
                SavedModLicences.Clear();
            }
            catch { }
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
                        Debug_SMissions.Log(KickStart.ModID + ": ManSubMissions Saving!");
                        SaveManSubMissions.SaveDataAutomaticLegacy();
                    }
                }
            }
            catch { }
        }
        private static void LoadSubMissionSaveData()
        {
            if (Singleton.Manager<ManGameMode>.inst.IsCurrent<ModeMain>() && !IgnoreSaveThisSession)
            {
                IgnoreSaveThisSession = false;
                Debug_SMissions.Log(KickStart.ModID + ": ManSubMissions Loading from save!");
                PurgeAllTrees();
                SaveManSubMissions.LoadDataAutomaticLegacy();
                inst.GetAllPossibleMissions();
                if (KickStart.Debugger)
                {
                    WindowManager.ShowPopup(new Vector2(0.8f, 1), Button);
                    WindowManager.ShowPopup(new Vector2(1, 0.1f), SideUI);
                }
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
        private static void LoadModdedLicencesInst()
        {
            Debug_SMissions.Log(KickStart.ModID + ": LoadModdedLicencesInst - Adding modded corp save states...");
            try
            {
                Dictionary<FactionSubTypes, FactionLicense> licences = 
                    (Dictionary<FactionSubTypes, FactionLicense>)ProgressionPatches.m_FactionLicenses.GetValue(ManLicenses.inst);
                foreach (var item in SavedModLicences)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": LoadModdedLicencesInst - Loading for " + item);
                    SMCCorpLicense CL = ManSMCCorps.GetSMCCorp(item.Key);
                    if (SubMissionTree.GetTreeCorp(item.Key, out FactionSubTypes FST) && CL != null)
                    {
                        UICCorpLicenses.MakeFactionLicenseOfficialUI(CL);
                        if (licences.TryGetValue(FST, out FactionLicense FL))
                        {
                            FactionLicense.Progress pog = (FactionLicense.Progress)ProgressionPatches.progg.GetValue(FL);
                            pog.m_Discovered = true;
                            pog.m_CurrentLevel = item.Value.Key;
                            pog.m_CurrentXP = item.Value.Value;
                            ManLicenses.inst.AddXP(FST, 0, true);
                            UICCorpLicenses.ShowFactionLicenseOfficialUI((int)FST);
                            Debug_SMissions.Log(KickStart.ModID + ": LoadModdedLicencesInst - Loaded for " + item);
                        }
                        else
                        {
                            Debug_SMissions.FatalError(KickStart.ModID + ": Fired load attempt too early with unofficial mods installed");
                        }
                    }
                }
            }
            catch
            {
                Debug_SMissions.Log(KickStart.ModID + ": LoadModdedLicencesInst - Entry corrupted");
            }
        }
        internal static void ForceCreateNewLicences()
        {
            try
            {
                Dictionary<FactionSubTypes, FactionLicense> licences = 
                    (Dictionary<FactionSubTypes, FactionLicense>)ProgressionPatches.m_FactionLicenses.GetValue(ManLicenses.inst);
                foreach (var item in ManSMCCorps.GetAllSMCCorps())
                {
                    if (item != null && SubMissionTree.GetTreeCorp(item.Faction, out FactionSubTypes FST))
                    {
                        if (!licences.TryGetValue(FST, out _))
                        {
                            licences.Add(FST, new FactionLicense(FST, item.BuildThresholds(), item.BuildProgress()));
                            Debug_SMissions.Log(KickStart.ModID + ": Added Licence " + item.Faction);
                            ProgressionPatches.LogLicenceReady(FST);
                        }
                    }
                }
            }
            catch
            {
                Debug_SMissions.Log(KickStart.ModID + ": LoadModdedLicencesInst - Entry corrupted");
            }
        }

        private static void PushFakeEncounters()
        {
            foreach (var item in GetActiveSubMissions)
            {
                EncounterShoehorn.SetFakeEncounter(item);
            }
        }

        // testing

    }
}
