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
     *         If a Step's ProgressID is set to -999, it will update all the time regardless of the CurrentProgressID.
     *     
     */
    public class ManSubMissions : MonoBehaviour
    {   // Handle non-Payload missions here
        public static ManSubMissions inst;

        public static bool Subscribed = false;
        public static bool SelectedIsAnon = false;
        public static bool IgnoreSaveThisSession = false;

        public static List<SubMissionTree> SubMissionTrees = new List<SubMissionTree>();


        private static List<SubMission> activeSubMissions = new List<SubMission>();
        public static List<SubMission> ActiveSubMissions
        {
            get
            {
                activeSubMissions.Clear();
                foreach (SubMissionTree tree in SubMissionTrees)
                    activeSubMissions.AddRange(tree.ActiveMissions);
                return activeSubMissions;
            }
        }


        private static List<SubMissionStandby> anonSubMissions = new List<SubMissionStandby>();
        public static List<SubMissionStandby> AnonSubMissions => anonSubMissions;

        public static SubMission Selected;
        public static SubMissionStandby SelectedAnon;

        public static GUIPopupDisplay Button;
        public static GUISMissionsList Board;
        public static GUIPopupDisplay SideUI;

        public static float timer = 0;
        public static float timerSecondsDelay = 1;


        public const float MinSpawnDist = 250;
        public const float MaxSpawnDist = 400;


        // Setup
        public static void Initiate()
        {
            inst = Instantiate(new GameObject("ManSubMissions")).AddComponent<ManSubMissions>();
            Debug.Log("SubMissions: ManSubMissions initated");
        }
        public static void Subscribe()
        {
            if (!Subscribed)
            {;
                Singleton.Manager<ManGameMode>.inst.ModeStartEvent.Subscribe(LoadSubMissionsFromSave);
                Singleton.Manager<ManGameMode>.inst.ModeFinishedEvent.Subscribe(SaveSubMissionsToSave);

                WindowManager.AddPopupButton("", "<b>SMissions</b>", false, "Master", windowOverride: WindowManager.TinyWindow);

                if (KickStart.Debugger)
                    WindowManager.ShowPopup(new Vector2(0.8f, 1));

                Button = WindowManager.GetCurrentPopup();

                WindowManager.AddPopupMissionsList();

                WindowManager.AddPopupMessageSide();

                if (KickStart.Debugger)
                    WindowManager.ShowPopup(new Vector2(1, 0.1f));

                SideUI = WindowManager.GetCurrentPopup();

                Debug.Log("SubMissions: ManSubMissions subscribed");
                Subscribed = true;
            }
        }
        public void HarvestAllTrees()
        {
            Debug.Log("SubMissions: HARVESTING ALL TREES!!!");
            SubMissionTrees.Clear();
            List<SubMissionTree> trees = SMissionJSONLoader.LoadAllTrees();
            foreach (SubMissionTree tree in trees)
            {
                SubMissionTrees.Add(tree.CompileMissionTree());
                Debug.Log("SubMissions: Missions count " + tree.Missions.Count + " | " + tree.RepeatMissions.Count);
            }
            GetAllPossibleMissions();
        }
        public void GetAllPossibleMissions()
        {
            Debug.Log("SubMissions: Fetching available missions...");
            anonSubMissions.Clear();
            foreach (SubMissionTree tree in SubMissionTrees)
            {
                anonSubMissions.AddRange(tree.GetReachableMissions());
            }
            SaveSubMissionsToSave();
        }

        public static SubMissionTree GetTree(string treeName)
        {
            SubMissionTree tree = SubMissionTrees.Find(delegate (SubMissionTree cand) { return cand.TreeName == treeName; });
            if (tree == default(SubMissionTree))
                SMUtil.Assert(false, "SubMissions: Failed to fetch tree - Was it removed!?");
            return tree;
        }

        // Missions
        public void AcceptMission()
        {   // We bite the bullet and insure the file has been marked tampered wth - because it was
            Singleton.Manager<ManSaveGame>.inst.CurrentState.m_FileHasBeenTamperedWith = true;
            SelectedAnon.Tree.AcceptTreeMission(SelectedAnon);
        }
        public void CancelMission()
        {
            Selected.Tree.CancelTreeMission(Selected);
        }
        public static void ReSyncSubMissions()
        {
            foreach (SubMission sub in ActiveSubMissions)
            {
                sub.ReSync();
            }
        }


        // 
        public static void ToggleList()
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
        public void CheckKeyCombos()
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.M))
            {
                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    HarvestAllTrees();
                    LoadSubMissionsFromSave();
                }
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    SMUtil.PushErrors();
                }
            }
        }

        // UPDATE
        public void Update()
        {
            UpdateAllSubMissions();
            timer += Time.deltaTime;
            if (timer >= timerSecondsDelay)
            {
                timer = 0;

                if (!ActiveSubMissions.Contains(Selected))
                {
                    Selected = null;
                    return;
                }
                if (!AnonSubMissions.Contains(SelectedAnon))
                {
                    SelectedAnon = null;
                    return;
                }
            }
            CheckKeyCombos();
        }
        public void UpdateAllSubMissions()
        {
            foreach (SubMission sub in ActiveSubMissions)
            {
                sub.TriggerUpdate();
            }
        }

        public static void PurgeAllTrees()
        {
            inst.GetAllPossibleMissions();
            foreach (SubMissionTree tree in SubMissionTrees)
            {
                tree.ResetALLTreeMissions();
            }
            inst.GetAllPossibleMissions();
        }


        //Saving
        public static List<SubMission> GetAllActiveSubMissions()
        {
            return activeSubMissions;
        }
        public static void SaveSubMissionsToSave()
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
        public static void SaveSubMissionsToSave(Mode mode)
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
        }
        public static void LoadSubMissionsFromSave()
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
        public static void LoadSubMissionsFromSave(Mode mode)
        {
            if (mode is ModeMain)
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
    }
}
