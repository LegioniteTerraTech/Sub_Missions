﻿using System;
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
     *         CurrentProgressID is '1', will run the missions in [0,1,2]
     *         
     *         Steps with a capital "S" at the end can offset step. It is suggested that you only use one step with
     *         per ProgressID.
     *         
     *         On success, the CurrentProgressID will be set to -98 and do one last loop.
     *         On fail, the CurrentProgressID will be set to -100 and do one last loop.
     *     
     */
    public class ManSubMissions : MonoBehaviour
    {   // Handle non-Payload missions here
        public static ManSubMissions inst;
        public static bool Subscribed = false;
        public static bool SelectedIsAnon = false;

        public static List<CustomSubMissionTree> SubMissionTrees = new List<CustomSubMissionTree>();


        private static List<CustomSubMission> activeSubMissions = new List<CustomSubMission>();
        public static List<CustomSubMission> ActiveSubMissions 
        {
            get {
                activeSubMissions.Clear();
                foreach (CustomSubMissionTree tree in SubMissionTrees)
                        activeSubMissions.AddRange(tree.ActiveMissions);
                return activeSubMissions;
            }
        }


        [JsonIgnore]
        private static List<CustomSubMissionStandby> anonSubMissions = new List<CustomSubMissionStandby>();
        [JsonIgnore]
        public static List<CustomSubMissionStandby> AnonSubMissions => anonSubMissions;

        public static CustomSubMission Selected;
        public static CustomSubMissionStandby SelectedAnon;
        public static GUISMissionsList Board;

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
            {
                WindowManager.AddPopupButton("", "<b>SMissions</b>", false, "Master", windowOverride: WindowManager.TinyWindow);

                WindowManager.ShowPopup(new Vector2(0.8f, 1));

                WindowManager.AddPopupMissionsList();

                WindowManager.AddPopupMessageSide();

                WindowManager.ShowPopup(new Vector2(1, 0.1f));

                Subscribed = true;
            }
        }
        public void HarvestAllTrees()
        {
            Debug.Log("SubMissions: HARVESTING ALL TREES!!!");
            SubMissionTrees.Clear();
            List<CustomSubMissionTree> trees = SMissionJSONLoader.LoadAllTrees();
            foreach (CustomSubMissionTree tree in trees)
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
            foreach (CustomSubMissionTree tree in SubMissionTrees)
            {
                anonSubMissions.AddRange(tree.GetReachableMissions());
            }
        }


        // Missions
        public void AcceptMission()
        {
            SelectedAnon.Tree.AcceptTreeMission(SelectedAnon);
        }
        public void CancelMission()
        {
            Selected.Tree.CancelTreeMission(Selected);
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
        public void CheckKeyCombo()
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftAlt))
            {
                if (Input.GetKeyDown(KeyCode.M))
                {
                    HarvestAllTrees();
                }
            }
        }

        // UPDATE
        public void Update()
        {
            timer += Time.deltaTime;
            if (timer >= timerSecondsDelay)
            {
                timer = 0; 
                UpdateAllSubMissions();
            }
            CheckKeyCombo();
        }
        public void UpdateAllSubMissions()
        {
            foreach (CustomSubMission sub in ActiveSubMissions)
            {
                sub.TriggerUpdate();
            }
        }
    }
}