﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;
using Sub_Missions.Steps;
using Sub_Missions.ManWindows;
using Sub_Missions.Editor;
using Newtonsoft.Json;
using TerraTechETCUtil;

namespace Sub_Missions
{
    [Serializable]
    public class SubMission
    {   //  Build the mission!
        //    Core
        public const int alwaysRunValue = int.MinValue;
        public const int missionCancelledValue = -98;
        public const int missionFailedValue = -100;
        public const float MaxActiveDistValue = 450;

        [JsonIgnore]
        internal SubMissionTree Tree;
        [JsonIgnore]
        public SubMissionType Type = SubMissionType.Basic;

        [Doc("The DEFAULT name for the mission.  Completely ignored if AltNames is set")]
        public string Name = "Unset";
        [JsonIgnore]
        public string SelectedAltName;
        [Doc("Alternate names for the mission (advised for repeating missions).  This directly corresponds to AltNames.")]
        public List<string> AltNames;
        [Doc("The faction that offers this mission offered with.")]
        public string Faction = "GSO";
        [Doc("The minimum grade required for this mission to be offered")]
        public int GradeRequired = 0;
        /// <summary> Missions that are COMPLETE </summary>
        [Doc("Missions that are COMPLETE")]
        public List<string> MissionsRequired;
        [Doc("The DEFAULT description of the mission.  Completely ignored if AltDescs is set")]
        public string Description = "ThisIsNotSetCorrectly";
        [Doc("Alternate descriptions for the mission (advised for repeating missions).  This directly corresponds to AltNames.")]
        public List<string> AltDescs;
        [Doc("Primary alternate progress type, ranging from [0 - 255]")]
        public byte MinProgressX = 0;
        [Doc("Secondary alternate progress type, ranging from [0 - 255]")]
        public byte MinProgressY = 0;
        [Doc("Name of the TerrainMod to use")]
        public string TerrainMod;
        [Doc("If it does not work in multiplayer.  " +
            "Note this shall be true regardless for some unsupported functions")]
        public bool SinglePlayerOnly = false;
        [Doc("If you can do this mission anywhere in the world")]
        public bool IgnorePlayerProximity = false;
        [Doc("Clears all the respective mission Techs from the world")]
        public bool ClearTechsOnClear = true;
        [Doc("Clears all the respective mission World Objects from the world")]
        public bool ClearModularMonumentsOnClear = true;
        [Doc("Clears all the respective mission Scenery from the world")]
        public bool ClearSceneryOnSpawn = true;
        [Doc("If the mission should not be cancellable")]
        public bool CannotCancel = false;
        [Doc("The Biomes this mission can spawn in. Note that this should support custom biomes as well")]
        public List<string> NamesBiomesAllowed = null;
        [Doc("The position where the mission should be spawned in relation to the player." +
            "\n  ClassicSelect - Spawns like a traditional mission" +
            "\n  FixedCoordinate - Spawns at a fixed coordinate relative to world origin.  " +
            "Handy for missions that must have positional awareness!" +
            "\n  FarFromPlayer - Spawn far away from the player, out of render distance" +
            "\n  CloseToPlayer - Spawn close to the player, barely within interaction distance" +
            "\n  OffsetFromPlayer - Spawn with specific offset from the player's camera position")]
        public SubMissionPosition SpawnPosition = SubMissionPosition.FarFromPlayer;

        internal FactionSubTypes FactionType => SubMissionTree.GetTreeCorp(Faction);

        // GLOBAL
        public WorldPosition WorldPos
        {
            get => worldPos;
            set { SetPosition_Internal(value); }
        }
        private WorldPosition worldPos = new WorldPosition(Vector2.zero, Vector3.zero);
        /// <summary>
        /// The ScenePosition of the mission (If needed)
        /// </summary>
        public Vector3 ScenePosition => worldPos.ScenePosition;
        /// <summary>
        /// The ScenePosition of the mission (If needed)
        /// </summary>
        public Vector3 SetStartingPosition { set { WorldPos = WorldPosition.FromScenePosition(value); } }
        [JsonIgnore]
        public Vector3 OffsetFromTile => worldPos.TileRelativePos;
        [JsonIgnore]
        public IntVector2 TilePos => worldPos.TileCoord;

        public bool CanCancel => (MissionDist >= ManSubMissions.MaxLoadedSpawnDist && Type != SubMissionType.Immedeate) || KickStart.OverrideRestrictions;


        public List<MissionChecklist> CheckList; // MUST be set externally via JSON or built C# code!

        public List<SubMissionStep> EventList; // MUST be set externally via JSON or built C# code!

        /// <summary>Used to SAVE AND LOAD the mission to the drive</summary>
        public List<bool> VarTrueFalse = new List<bool>();          // MUST be set externally via JSON or built C# code!
        public List<int> VarInts = new List<int>();                 // MUST be set externally via JSON or built C# code!

        [JsonIgnore]
        /// <summary>Used during ACTIVE mission</summary>
        public List<bool> VarTrueFalseActive = new List<bool>();
        [JsonIgnore]
        /// <summary>Used during ACTIVE mission</summary>
        public List<int> VarIntsActive = new List<int>();

        //public List<float> VarFloats = new List<float>();           // MUST be set externally via JSON or built C# code!
        //public List<Vector3> VarPositions = new List<Vector3>();    // MUST be set externally via JSON or built C# code!


        [JsonIgnore]
        public List<TrackedTech> TrackedTechs = new List<TrackedTech>();
        [JsonIgnore]
        public List<TrackedBlock> TrackedBlocks = new List<TrackedBlock>();
        [JsonIgnore]
        internal List<SMWorldObject> TrackedMonuments = new List<SMWorldObject>();

        public SubMissionReward Rewards = new SubMissionReward(); // MUST be set externally via JSON or built C# code!

        public float UpdateSpeedMultiplier = 1; // DON'T TOUCH THIS unless you know EXACTLY what you are doing!
        //  If this value is too high, there WILL be framerate and performance issues!

        [JsonIgnore]
        public float MissionDist = 1; // Keeps track of how far the mission is

        [JsonIgnore]
        internal float updateLerp = 0;
        [JsonIgnore]
        internal bool AwaitingAction = false;
        [JsonIgnore]
        internal int CurrentProgressID = 0;
        [JsonIgnore]
        private bool IsCleaningUp = false;
        [JsonIgnore]
        public SubMissionLoadState ActiveState = SubMissionLoadState.NotAvail;
        /// <summary>
        /// SubMission is FULLY loaded
        /// </summary>
        [JsonIgnore]
        public bool IsActive => ActiveState == SubMissionLoadState.Loaded;
        [JsonIgnore]
        private bool Corrupted = false;
        [JsonIgnore]
        internal Encounter FakeEncounter = null;


        public bool ShowOnGUI()
        {
            return SMMissionGUI.ShowGUI(this);
        }
        public static string GetDocumentation()
        {
            StringBuilder SB = new StringBuilder();
            new AutoDocumentator(typeof(SubMission), "\n*       Every mission starts on the CurrentProgressID of 0." +
                 "\n*" +
                 "\n*       EventList - handles the Steps in order from top to bottom, and repeats if nesseary" +
                 "\n*" +
                 "\n*       There are variables you can add and reference around the case of the entire mission;" +
                 "\n*       VarTrueFalse, VarInts, VarFloats can be called and pooled later on" +
                 "\n*       Proper syntax for this would be:" +
                 "\n*       \"VarTrueFalse\" :{" +
                 "\n*          false, // Is Target destroyed?" +
                 "\n*          false, // PlayerIsAlive" +
                 "\n*          false, // next bool" +
                 "\n*          true,  // whatever" +
                 "\n*       }" +
                 "\n*" +
                 "\n*      Variables with \"Global\" attached to the beginning:" +
                 "\n*       To re-reference these (Entire-SubMission level Varibles), in the step's trigger, make sure to " +
                 "\n*       reference the Zero-Based-Index in the respective mission slot to reference the variable." +
                 "\n*       Zero-Based-Index [0,1,2]  (normal is [1,2,3])" +
                 "\n*" +
                 "\n*       Each Step has a Progress ID, which tells the SubMission where to iterate to." +
                 "\n*         When a branch ID is set, the values adjacent of it will still be triggered" +
                 "\n*         this it to allow some keep features to still work like slightly changing the ProgressID to deal with" +
                 "\n*         players leaving the mission area" +
                 "\n*         If the CurrentProgressID is '1', the mission will run the Steps in [0,1,2]" +
                 "\n*" +
                 "\n*         Steps with a capital \"S\" at the end can offset Step. It is suggested that you only use one step with" +
                 "\n*         per ProgressID." +
                 "\n*" +
                 "\n*         On success, the CurrentProgressID will be set to -98 and do one last loop." +
                 "\n*         On fail, the CurrentProgressID will be set to -100 and do one last loop." +
                 "\n*" +
                 "\n*         If a Step's ProgressID is set to:" +
                 "\n*         - alwaysRunValue (" + alwaysRunValue + ") - Update all the time regardless of the CurrentProgressID." +
                 "\n*         - missionCancelledValue (" + missionCancelledValue + ") - Updated once on Mission Cancelled" +
                 "\n*         - missionFailedValue (" + missionFailedValue + ") - Updated once on Mission Fail.").StringBuild(null, null, SB, SlashState.Slash);
            return SB.ToString();
            /*
            return
                 "*   SubMission Missions Syntax" +
                 "\n*       Every mission starts on the CurrentProgressID of 0." +
                 "\n*" +
                 "\n*       EventList - handles the Steps in order from top to bottom, and repeats if nesseary" +
                 "\n*" +
                 "\n*       There are variables you can add and reference around the case of the entire mission;" +
                 "\n*       VarTrueFalse, VarInts, VarFloats can be called and pooled later on" +
                 "\n*       Proper syntax for this would be:" +
                 "\n*       \"VarTrueFalse\" :{" +
                 "\n*          false, // Is Target destroyed?" +
                 "\n*          false, // PlayerIsAlive" +
                 "\n*          false, // next bool" +
                 "\n*          true,  // whatever" +
                 "\n*       }" +
                 "\n*" +
                 "\n*      Variables with \"Global\" attached to the beginning:" +
                 "\n*       To re-reference these (Entire-SubMission level Varibles), in the step's trigger, make sure to " +
                 "\n*       reference the Zero-Based-Index in the respective mission slot to reference the variable." +
                 "\n*       Zero-Based-Index [0,1,2]  (normal is [1,2,3])" +
                 "\n*" +
                 "\n*       Each Step has a Progress ID, which tells the SubMission where to iterate to." +
                 "\n*         When a branch ID is set, the values adjacent of it will still be triggered" +
                 "\n*         this it to allow some keep features to still work like slightly changing the ProgressID to deal with" +
                 "\n*         players leaving the mission area" +
                 "\n*         If the CurrentProgressID is '1', the mission will run the Steps in [0,1,2]" +
                 "\n*" +
                 "\n*         Steps with a capital \"S\" at the end can offset Step. It is suggested that you only use one step with" +
                 "\n*         per ProgressID." +
                 "\n*" +
                 "\n*         On success, the CurrentProgressID will be set to -98 and do one last loop." +
                 "\n*         On fail, the CurrentProgressID will be set to -100 and do one last loop." +
                 "\n*" +
                 "\n*         If a Step's ProgressID is set to:" +
                 "\n*         - alwaysRunValue (" + alwaysRunValue + ") - Update all the time regardless of the CurrentProgressID." +
                 "\n*         - missionCancelledValue (" + missionCancelledValue + ") - Updated once on Mission Cancelled" +
                 "\n*         - missionFailedValue (" + missionFailedValue + ") - Updated once on Mission Fail." +
                 "\n*" +
                 "\n{  // Holds ONE whole mission" +
                  "\n  \"Name\": \"ExampleMission\",     // Must be the same as the file name (leave out the file extension)" +
                  "\n  \"AltNames\": {  // alternate names for the mission (advised for repeating missions)" +
                  "\n     \"AltName1\"," +
                  "\n     \"AltName2\"," +
                  "\n  },             // " +
                  "\n  \"Description\": \"This mission is an example\",      // The default description for the mission" +
                  "\n  \"Position\": {  // The position where this is handled relative to the WORLD's origin." +
                  "\n    \"x\": 0.0," +
                  "\n    \"y\": 0.0," +
                  "\n    \"z\": 0.0" +
                  "\n  }, //  Note that missions on spawn will always point north." +
                  "\n  \"Faction\": \"GSO\",      // The corporation to affiliate the mission with" +
                  "\n  \"GradeRequired\": 0,  // The minimum grade required to have this mission appear" +
                  "\n  \"SpawnPosition\": \"FarFromPlayer\",   // The position where the mission should be spawned in relation to the player." +
                  "\n  // Conditions TO CHECK before offering mission" +
                  "\n  \"MinProgressX\": 0,   // Your SubMissionTree.json 's ProgressXName required for this mission." +
                  "\n  //  If this value is negative, then it checks based on At Most." +
                  "\n  \"MinProgressX\": 0,   // Your SubMissionTree.json 's ProgressXName required for this mission." +
                  "\n  //   If this value is negative, then it checks based on At Most." +
                  "\n  // Input Parameters" +
                  "\n  \"SinglePlayerOnly\": true,       // Set if it should only be offered in single-player (MP is pending)." +
                  "\n  // This will not allow illegal blocks to spawn." +
                  "\n  \"IgnorePlayerProximity\": false,   // Use this for SubMissions that don't have to have a proximity" +
                  "\n  // Post-SubMission cleanup" +
                  "\n  //  - Everything should clear anyways if the mission is cancelled or failed" +
                  "\n  \"ClearTechsOnClear\": true,      // Clear all the mission techs on successful mission finish." +
                  "\n  \"ClearModularMonumentsOnClear\": true,  // Gets rid of all ModularMonuments in this mission." +
                  "\n  \"ClearSceneryOnSpawn\": true,  // Clears the mission area." +
                  "\n  \"NamesBiomesAllowed\": {  // Biomes that this mission shall be restricted to.  Leave empty for all." +
                  "\n     \"Grasslands\"," +
                  "\n  },             // " +
                 "\n}," +
                 "\n //Note:  Repeating missions do not reserve their position - do not use for missions with persistant elements!";
            */
        }

        internal void OnMoveWorldOrigin(IntVector3 moveDist)
        {
            //Position += moveDist;
            foreach (SubMissionStep step in GetAllEventsLinear())
            {
                step.UpdatePosition(moveDist);
            }
        }
        internal void CheckForReSync()
        {
            if (!IsActive)
                ReSync();
        }
        internal void PauseAndUnload()
        {
            ActiveState = SubMissionLoadState.PositionSetReady;
            foreach (SMWorldObject SMWO in TrackedMonuments)
            {
                SMWO.Remove(false);
            }
            foreach (SubMissionStep step in GetAllEventsLinear())
            {
                step.UnloadStep();
            }
        }

        internal void GetAndSetDisplayName()
        {   // 
            if (!SelectedAltName.NullOrEmpty())
                return;
            if (AltNames == null)
                SelectedAltName = Name;
            else if (AltNames.Count < 1)
                SelectedAltName = Name;
            else
            {
                string check = AltNames.GetRandomEntry();
                if (check.NullOrEmpty())
                    SelectedAltName = Name;
                SelectedAltName = check;
            }
        }
        
        [JsonIgnore]
        private float minRange = -1;
        public float GetMinimumLoadRange()
        {   // 
            if (minRange == -1)
            {
                foreach (SubMissionStep step in GetAllEventsLinear())
                {
                    float mag = step.InitPosition.magnitude;
                    if (mag > minRange)
                        minRange = mag;
                }
            }
            return minRange;
        }
        public float GetWorldActiveRange()
        {   // 
            return MaxActiveDistValue;
        }

        /// <summary> Called when the mission starts for the first time </summary>
        /// <exception cref="MandatoryException"></exception>
        internal void Startup(bool forceNearPlayer)
        {   // 
            try
            {
                if (UpdateSpeedMultiplier < 0.5f)
                {
                    SMUtil.Error(false, "Mission (Startup) ~ " + Name, 
                        KickStart.ModID + ": " + Name + " UpdateSpeedMultiplier cannot be lower than 0.5");
                    UpdateSpeedMultiplier = 0.5f;
                }
                SMUtil.Log(false, KickStart.ModID + ": Startup for mission " + Name);

                IsCleaningUp = false;
                if (forceNearPlayer)
                    SetPositionATPlayer();
                else
                {
                    switch (SpawnPosition)
                    {
                        case SubMissionPosition.ClassicSelect:
                            SetPositionClassicMission();
                            break;
                        case SubMissionPosition.FarFromPlayer:
                            SetPositionFarFromPlayer();
                            break;
                        case SubMissionPosition.CloseToPlayer:
                            SetPositionCloseToPlayer();
                            break;
                        case SubMissionPosition.OffsetFromPlayer:
                            SetPositionFromPlayer();
                            break;
                        case SubMissionPosition.OffsetFromPlayerTechFacing:
                            SetPositionFromPlayerTankFacing();
                            break;
                        case SubMissionPosition.FixedCoordinate:
                            break;
                    }
                }

                bool isMissionImpossible = true;
                foreach (SubMissionStep step in GetAllEventsLinear())
                {
                    if (step.StepType == SMStepType.ActWin)
                        isMissionImpossible = false;
                }
                if (isMissionImpossible)
                    SMUtil.Error(false, "Mission (Startup) ~ " + Name, 
                        KickStart.ModID + ": " + Name + " is impossible to complete as there are no existing Win Steps in the Event List!!!");

                ActiveState = SubMissionLoadState.NeedsFirstInit;
                UpdateDistance();
                if (IgnorePlayerProximity)
                {
                    FirstLoad();
                }
                else
                    CheckIfCanSpawn();
                ManSubMissions.MissionStartedEvent.Send(this);
            }
            catch (MandatoryException e)
            {
                throw new MandatoryException("Critical Error within SubMission.Startup() of mission " + Name
                    + ", type " + Type + "!", e);
            }
        }
        internal void ForceInitiateImmedeate()
        {
            try
            {
                if (ActiveState != SubMissionLoadState.Loaded)
                {
                    SetPositionCloseToPlayer();
                    FirstLoad();
                }
            }
            catch (MandatoryException e)
            {
                throw new MandatoryException("Critical Error within SubMission.ForceInitiateImmedeate() of mission " + Name
                    + ", type " + Type + "!", e);
            }
        }


        internal void UpdateDistance()
        {   // 
            if (ActiveState == SubMissionLoadState.NeedsFirstInit || ActiveState == SubMissionLoadState.PositionSetReady)
            {
                RunWaypoint(!IgnorePlayerProximity && ManSubMissions.Selected == this);
            }   
            else
                RunWaypoint(false);
            MissionDist = SMUtil.GetPlayerDist(ScenePosition);
        }
        public void CheckIfCanSpawn()
        {   // 
            if (IsActive || MissionDist > ManSubMissions.LoadCheckDist)
                return;
            if (ManWorld.inst.CheckAllTilesAtPositionHaveReachedLoadStep(ScenePosition, GetMinimumLoadRange()))
            {
                FirstLoad();
            }
        }
        /// <summary>
        /// Called for newly started Sub Missions
        /// </summary>
        private void FirstLoad()
        {   // 
            if (Corrupted)
                return; // cannot run!!!
            try
            {
                // Sanity check to INSURE no invalid variable counts
                int minVarInts = VarInts.Count;
                int minVarTF = VarTrueFalse.Count;
                foreach (SubMissionStep step in GetAllEventsLinear())
                {
                    step.Mission = this;
                    step.FirstSetup();
                    if (step.VaribleType == EVaribleType.Int ||
                        step.VaribleType == EVaribleType.IntLessThan ||
                        step.VaribleType == EVaribleType.IntGreaterThan ||
                        step.stepGenerated.ForceUsesVarInt())
                        minVarInts = Mathf.Max(minVarInts, step.SetMissionVarIndex1 + 1,
                            step.SetMissionVarIndex2 + 1, step.SetMissionVarIndex3 + 1);
                    else if (step.VaribleType == EVaribleType.True ||
                        step.VaribleType == EVaribleType.False ||
                        step.stepGenerated.ForceUsesVarBool())
                        minVarTF = Mathf.Max(minVarTF, step.SetMissionVarIndex1 + 1, 
                            step.SetMissionVarIndex2 + 1, step.SetMissionVarIndex3 + 1);
                }
                if (VarInts.Count < minVarInts)
                {
                    SMUtil.Log(false, "Mission " + Name + " had less VarInts than expected [" + VarInts.Count + "] vs [" + minVarInts +
                        "].  Please report this to the mission developer.");
                    while (VarInts.Count <= minVarInts)
                        VarInts.Add(0);
                }
                if (VarTrueFalse.Count < minVarTF)
                {
                    SMUtil.Log(false, "Mission " + Name + " had less VarTrueFalse than expected [" + VarTrueFalse.Count + "] vs [" + minVarTF +
                        "].  Please report this to the mission developer.");
                    while (VarTrueFalse.Count <= minVarTF)
                        VarTrueFalse.Add(false);
                }

                VarTrueFalseActive.Clear();
                foreach (var item in VarTrueFalse)
                    VarTrueFalseActive.Add(item);
                VarIntsActive.Clear();
                foreach (var item in VarInts)
                    VarIntsActive.Add(item);
                if (ClearSceneryOnSpawn)
                    TryClearAreaForMission();
                if (!TerrainMod.NullOrEmpty())
                    SubMissionTree.ApplyTerrain(this, TerrainMod);
                try
                {
                    foreach (TrackedBlock listEntry in TrackedBlocks)
                    {
                        listEntry.mission = this;
                    }
                }
                catch
                {
                    SMUtil.Error(false, "Mission (Startup) ~ " + Name, KickStart.ModID + ": Mission " + Name +
                        " has encountered some errors while loading the TrackedBlocks.  Check your syntax.");
                }
                try
                {
                    foreach (TrackedTech tech in TrackedTechs)
                    {
                        tech.mission = this;
                    }
                }
                catch
                {
                    SMUtil.Error(false, "Mission (Startup) ~ " + Name, KickStart.ModID + ": Mission " + Name + 
                        " has encountered some errors while loading the TrackedTechs.  Check your syntax.");
                }
                try
                {
                    foreach (MissionChecklist listEntry in CheckList)
                    {
                        if (listEntry != null)
                            listEntry.mission = this;
                    }
                }
                catch
                {
                    SMUtil.Error(false, "Mission (Startup) ~ " + Name, KickStart.ModID + ": Mission " + Name + 
                        " has encountered some errors while loading the CheckList.  Check your syntax.");
                }

                Debug_SMissions.Log(KickStart.ModID + ": Mission " + Name + " has loaded into the world!");
                ActiveState = SubMissionLoadState.Loaded;
            }
            catch (Exception e)
            {
                SMUtil.Assert(true, "Mission (Startup) ~ " + Name, KickStart.ModID + ": Mission " + Name + 
                    " has encountered a serious error on spawning and will not be able to load!", e);
                Corrupted = true; 
            }
        }
        /// <summary>
        /// Reloads the mission from the save file
        /// </summary>
        private void ReSync()
        {   // 
            SMUtil.Log(false, KickStart.ModID + ": Resync(Load from save file) for SubMission " + Name);
            IsCleaningUp = false;
            foreach (SubMissionStep step in GetAllEventsLinear())
            {
                step.Mission = this;
                step.LoadStep();
            }
            try
            {
                foreach (TrackedTech tech in TrackedTechs)
                {
                    tech.mission = this;
                }
            }
            catch { }
            try
            {
                foreach (MissionChecklist listEntry in CheckList)
                {
                    listEntry.mission = this;
                }
            }
            catch { }
            ActiveState = SubMissionLoadState.Loaded;
            //Debug_SMissions.Log(KickStart.ModID + ": ReSynced");
            //Debug_SMissions.Log(KickStart.ModID + ": Tree " + Tree.TreeName);
        }
        internal void Cleanup(bool isWorldUnloading = false)
        {   //
            IsCleaningUp = true;
            try  // We don't want to crash when the mission maker is still testing
            {       // Will inevitably crash with no checklist items assigned
                if (CheckList != null)
                    CheckList.Clear();
            }
            catch { };
            ManQuestLog.inst.HideMissionTimerUI(FakeEncounter.EncounterDef);
            RunWaypoint(false);
            if (!isWorldUnloading)
            {
                try  // not all missions involve techs
                {
                    foreach (TrackedTech tech in TrackedTechs)
                    {
                        try  // not all techs will still exist
                        {
                            tech.DestroyTech();
                        }
                        catch { };
                    }
                    TrackedTechs.Clear();
                }
                catch { };
            }
            if (EventList != null)
            {
                foreach (SubMissionStep step in GetAllEventsLinear())
                {
                    try  // We don't want to crash when the mission maker is still testing
                    {
                        step.ClearStep();
                    }
                    catch { }
                }
            }
            foreach (SMWorldObject WO in TrackedMonuments)
            {
                try
                {
                    WO.Remove(false);
                }
                catch { }
            }
            TrackedMonuments.Clear();
            ActiveState = SubMissionLoadState.NotAvail;
        }
        private void Conclude()
        {   // Like Cleanup but leaves some optional aftermath
            IsCleaningUp = true;
            try  // We don't want to crash when the mission maker is still testing
            {       // Will inevitably crash with no checklist items assigned
                CheckList.Clear();
            }
            catch { };
            ManQuestLog.inst.HideMissionTimerUI(FakeEncounter.EncounterDef);
            RunWaypoint(false);
            if (ClearTechsOnClear)
            {
                try  // not all missions involve techs
                {
                    foreach (TrackedTech tech in TrackedTechs)
                    {
                        try  // not all techs will still exist
                        {
                            tech.DestroyTech();
                        }
                        catch (Exception e)
                        {
                            Debug_SMissions.Assert("SubMission.Conclude() errored at TrackedTechs: " + e.StackTrace);
                        }
                    }
                    TrackedTechs.Clear();
                }
                catch { };
            }
            if (EventList != null)
            {
                foreach (SubMissionStep step in GetAllEventsLinear())
                {
                    try  // We don't want to crash when the mission maker is still testing
                    {
                        step.ClearStep();
                    }
                    catch { }
                }
            }
            if (ClearModularMonumentsOnClear)
            {
                foreach (SMWorldObject WO in TrackedMonuments)
                {
                    try
                    {
                        WO.Remove(false);
                    }
                    catch { }
                }
            }
            else
                ManModularMonuments.GraduateToPerm(TrackedMonuments);
            TrackedMonuments.Clear();
            ActiveState = SubMissionLoadState.PositionSetReady;
        }

        private static List<SubMissionStep> allEventsCached = new List<SubMissionStep>();
        private static bool GetAllEventsBusy = false;
        public List<SubMissionStep> GetAllEventsLinear()
        {   //
            if (GetAllEventsBusy)
                throw new InvalidOperationException("SubMission.GetAllEvents() - cannot nest calls!");
            GetAllEventsBusy = true;
            allEventsCached.Clear();
            if (EventList != null)
                GetEventsRecursive(EventList, ref allEventsCached);
            GetAllEventsBusy = false;
            return allEventsCached;
        }
        private void GetEventsRecursive(List<SubMissionStep> toSearch, ref List<SubMissionStep> allEvents)
        {   //
            for (int i = 0; i < toSearch.Count; i++)
            {
                SubMissionStep step = toSearch[i];
                allEvents.Add(step);
                if (step.StepType == SMStepType.Folder && step.FolderEventList != null && step.FolderEventList.Count > 0)
                {
                    GetEventsRecursive(step.FolderEventList, ref allEvents);
                }
            }
        }

        public bool RemoveEvent(SubMissionStep toRemove)
        {   //
            if (EventList != null)
                return RemoveEventRecursive(EventList, toRemove);
            return false;
        }
        private bool RemoveEventRecursive(List<SubMissionStep> toSearch, SubMissionStep toRemove)
        {   //
            for (int i = 0; i < toSearch.Count; i++)
            {
                SubMissionStep step = toSearch[i];
                if (step == toRemove)
                {
                    toSearch.RemoveAt(i);
                    return true;
                }
                if (step.StepType == SMStepType.Folder && step.FolderEventList != null && step.FolderEventList.Count > 0)
                {
                    if (RemoveEventRecursive(step.FolderEventList, toRemove))
                        return true;
                }
            }
            return false;
        }

        internal void PurgeAllActiveMessages()
        {   //
            foreach (SubMissionStep step in GetAllEventsLinear())
            {
                try  // We don't want to crash when the mission maker is still testing
                {
                    if (step.AssignedWindow != null)
                    {
                        if (step.AssignedWindow.isOpen)
                        {
                            WindowManager.HidePopup(step.AssignedWindow);
                            WindowManager.RemovePopup(step.AssignedWindow);
                        }
                    }
                }
                catch { };
            }
        }

        // Endings
        internal void Finish(bool forced = false)
        {   //
            if (ActiveState != SubMissionLoadState.Loaded)
                return;
            ActiveState = SubMissionLoadState.Concluded;
            if (forced)
            {
                Debug_SMissions.Assert(true, "SubMission.Finish(true) was force-ended and may encounter problems!");
            }
            else if (IsCleaningUp || ManSubMissions.BlockMissionEnd)
                return;
            ManSubMissions.MissionFinishedEvent.Send(this, ManEncounter.FinishState.Completed);
            Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.MissionComplete);
            Rewards.Reward(Tree, this);
            CurrentProgressID = missionCancelledValue;
            TriggerUpdate(Time.deltaTime);
            EncounterShoehorn.OnFinishSubMission(this, ManEncounter.FinishState.Completed);
            Conclude();
            Tree.FinishedMission(this);
        }
        internal void Fail()
        {   //
            if (ActiveState != SubMissionLoadState.Loaded)
                return;
            ActiveState = SubMissionLoadState.Concluded;
            ManSubMissions.MissionFinishedEvent.Send(this, ManEncounter.FinishState.Failed);
            Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.MissionFailed);
            CurrentProgressID = missionFailedValue;
            EncounterShoehorn.OnFinishSubMission(this, ManEncounter.FinishState.Failed);
            TriggerUpdate(Time.deltaTime);
            Cleanup();
            ManSubMissions.GetActiveSubMissions.Remove(this);
        }

        internal void Reboot(bool ToBeginning = false)
        {   //
            Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Craft);
            CurrentProgressID = 0;
            if (ToBeginning)
            {
                Cleanup();
                FirstLoad();
            }
            else
            {
                Cleanup(true);
                ReSync();
            }
        }


        // UPDATE
        [JsonIgnore]
        internal float DeltaTime = 0;
        private int updateStep = 0;
        public int UpdateStep => updateStep;
        internal void TriggerUpdate(float deltaTime, int Steps = int.MaxValue)
        {
            DeltaTime = deltaTime;
            int stepper = 0;
            if (IsActive || IgnorePlayerProximity)
            {
                updateLerp = 0;
                int position = 0;
                Debug_SMissions.Info(KickStart.ModID + ": LERP " + CurrentProgressID);
                for (int total = 0; stepper < Steps && total < EventList.Count; total++)
                {
                    if (updateStep >= EventList.Count)
                        updateStep = 0;
                    SubMissionStep step = EventList[updateStep];

                    if (CanRunStep(step.ProgressID))
                    {
                        stepper++;
                        try
                        {   // can potentially fire too early before mission is set
                            step.Trigger();
                        }
                        catch (MandatoryException e)
                        {
                            throw new MandatoryException("MandatoryException on step " + updateStep + " of StepType " + step.StepType.ToString(), e);
                        }
                        catch
                        {
                            Debug_SMissions.Log(KickStart.ModID + ": Error on attempting step lerp " + position + " in relation to " + CurrentProgressID + " of mission " + Name + " in tree " + Tree.TreeName);
                            try
                            {
                                Debug_SMissions.Log(KickStart.ModID + ": Type of " + step.StepType.ToString() + " ProgressID " + step.ProgressID + " | Is connected to a mission: " + (step.Mission != null).ToString());
                            }
                            catch (Exception e)
                            {
                                throw new MandatoryException(KickStart.ModID + ": Confirmed null step, step.StepType, " +
                                    "step.ProgressID, or step.Mission.  Step " + updateStep + " was improperly set up!", e);
                            }
                        }
                    }
                    updateStep++;
                    position++;
                }
                UpdateFakeEncounter();
            }
        }
        internal void TriggerUpdate(float deltaTime, float Speed)
        {   // updated at least every half second

            float segmentPercent = Mathf.Min(EventList.Count, 1 / Mathf.Max(1, EventList.Count));

            updateLerp += Time.deltaTime;

            float Sped = updateLerp * UpdateSpeedMultiplier * Speed;
            if (Sped > segmentPercent)
            {
                Sped /= segmentPercent;
                TriggerUpdate(deltaTime, Sped);
            }
        }

        private void UpdateFakeEncounterQuestLog()
        {
            QuestLogData QLD = FakeEncounter.QuestLog;
            if (CheckList != null)
            {
                if (QLD == null)
                    throw new Exception("We screwed up real bad and lost the mission checklist somewhere.  pls help?");
                if (VarTrueFalseActive == null)
                    throw new Exception("VarTrueFalse is missing...      Wait how?");
                for (int step = 0; step < CheckList.Count; step++)
                {
                    int boolIndex = CheckList[step].BoolToEnable;
                    if (boolIndex == -1)
                        QLD.SetObjectiveVisible(step, true);
                    else
                    {
                        if (boolIndex < 0 || boolIndex >= VarTrueFalseActive.Count)
                            throw new Exception("Checklist contains invalid entry at index [" + step +
                                "], which has BoolToEnable set to [" + boolIndex +
                                "] but such value is outside the range of present Bools and not the -1" +
                                " always active value: [0 -> " + (VarTrueFalseActive.Count - 1).ToString() + "]");
                        bool trueFalse = VarTrueFalseActive[boolIndex];
                        QLD.SetObjectiveVisible(step, trueFalse);
                    }

                    try
                    {
                        if (CheckList[step].GetNumber(out int number))
                            QLD.SetObjectiveCount(step, number);
                    }
                    catch (Exception e)
                    {
                        throw new WarningException("Checklist GetNumber fail at index [" + step + "]", e);
                    }
                    try
                    {
                        if (CheckList[step].TestForCompleted() != QLD.GetObjectiveCompleted(step))
                            QLD.SetObjectiveCompleted(step, true);
                    }
                    catch (Exception e)
                    {
                        throw new WarningException("Checklist TestForCompleted partial fail", e);
                    }
                }
            }
        }
        private void UpdateFakeEncounter()
        {
            try
            {   // can potentially fire too early before mission is set
                if (FakeEncounter)
                {
                    UpdateFakeEncounterQuestLog();

                    /*
                    EncounterUpdateMessage EUM = new EncounterUpdateMessage();
                    FakeEncounter.QuestLog.FillMessage(EUM);
                    FakeEncounter.
                    int count = CheckList.Count;
                    EncounterUpdateMessage.ObjectiveData[] newBatch = new EncounterUpdateMessage.ObjectiveData[count];
                    for (int step = 0; step < count; step++)
                    {
                        try
                        {
                            EncounterUpdateMessage.ObjectiveData OD = EUM.m_ObjectiveData[step];
                            OD.m_ShowCount = CheckList[step].GetNumber(out int numberOb);
                            if (OD.m_ShowCount)
                                OD.m_Count = numberOb;
                            OD.m_Visible = EUM.m_ActiveObjectiveIdx == step;
                            OD.m_Completed = CheckList[step].TestForCompleted();
                            newBatch[step] = OD;
                        }
                        catch { }
                    }
                    EUM.m_ObjectiveData = newBatch;
                    FakeEncounter.QuestLog.UpdateFromMessage(EUM);
                    */
                }
                else
                {
                    foreach (MissionChecklist task in CheckList)
                    {
                        task.TestForCompleted();
                    }
                }
            }
            catch (Exception e){
                throw new Exception("Failiure when handling UpdateFakeEncounter()", e);
            }
        }

        internal EncounterDisplayData GetEncounterDisplayInfo()
        {
            EncounterDisplayData EDD = EncounterShoehorn.GetEncounterDisplayInfo(this);
            return EDD;
        }


        // Utilities
        internal bool CanRunStep(int input)
        {
            return input == alwaysRunValue || (input <= CurrentProgressID + 1 &&
                input >= CurrentProgressID - 1);
        }
        public bool GetTechPosHeading(string TechName, out Vector3 pos, out Vector3 direction, out int team)
        {   //
            if (!IsActive)
                Debug_SMissions.Log(KickStart.ModID + ": GetTechPosHeading - Called when MISSION IS INACTIVE");
            int hash = TechName.GetHashCode();
            foreach (SubMissionStep step in GetAllEventsLinear())
            {
                if (step.hasTech)
                {
                    if (step.InputString.GetHashCode() == hash)
                    {
                        pos = step.InitPosition;
                        direction = step.Forwards;
                        team = (int)step.InputNum;
                        return true;
                    }
                }
            }
            pos = Vector3.zero;
            direction = Vector3.forward;
            team = -1;
            return false;
        }
        public bool GetBlockPos(string BlockName, out Vector3 pos)
        {   //
            foreach (SubMissionStep step in GetAllEventsLinear())
            {
                if (step.hasBlock)
                {
                    if (step.InputString == BlockName)
                    {
                        pos = step.InitPosition;
                        return true;
                    }
                }
            }
            pos = Vector3.zero;
            return false;
        }

        public bool GetPiecePosHeading(string PieceName, out Vector3 pos, out Vector3 direction)
        {   //
            if (!IsActive)
                Debug_SMissions.Log(KickStart.ModID + ": GetTechPosHeading - Called when MISSION IS INACTIVE");
            int hash = PieceName.GetHashCode();
            foreach (SubMissionStep step in GetAllEventsLinear())
            {
                if (step.stepGenerated is StepSetupMM mm)
                {
                    if (step.InputString.GetHashCode() == hash)
                    {
                        pos = step.InitPosition;
                        direction = step.Forwards;
                        return true;
                    }
                }
            }
            pos = Vector3.zero;
            direction = Vector3.forward;
            return false;
        }

        private const int maxAttempts = 32;
        private static Bitfield<ObjectTypes> searchTypes = new Bitfield<ObjectTypes>(new ObjectTypes[2] { ObjectTypes.Vehicle, ObjectTypes.Crate });
        private void SetPosition_Internal(WorldPosition Wp)
        {
            worldPos = Wp;
        }
        private void SetPositionClassicMission()
        {
            Debug_SMissions.Log(KickStart.ModID + ": SetPositionClassicMission");
            List<WorldTile> IV = new List<WorldTile>();
            Vector2 playerTile = ManWorld.inst.TileManager.SceneToTileCoord(Singleton.playerPos);
            foreach (WorldTile IVc in ManWorld.inst.TileManager.IterateTiles(WorldTile.State.Created))
            {
                if (IVc.Terrain && !IVc.IsPopulated)
                    IV.Add(IVc);
            }
            int missionReqRad = (int)GetMinimumLoadRange();
            foreach (WorldTile IVc in IV.OrderBy(x => ((Vector2)x.Coord - playerTile).sqrMagnitude))
            {
                if (IVc.LargestFreeSpaceOnTile > missionReqRad && !ManSubMissions.IsTooCloseToOtherMission(IVc.Coord))
                {
                    WorldPos = WorldPosition.FromScenePosition(IVc.CalcSceneCentre());
                    Debug_SMissions.Log("Decided on Scene Position " + ScenePosition);
                    break;
                }
            }

            /*
            // Will try configuring later
            string NameCase = "MOD_SubMission_" + Name;
            FreeSpaceFinder FSF = new FreeSpaceFinder();
            ManFreeSpace.FreeSpaceParams FSP = new ManFreeSpace.FreeSpaceParams
            {
                m_AllowSpawnInSceneryBlocker = ClearSceneryOnSpawn,
                m_AllowUnloadedTiles = true,
                m_AvoidLandmarks = true,
                m_CenterPos = Position,
                m_CenterPosWorld = WorldPosition.FromScenePosition(Position),
                m_CameraSpawnConditions = ManSpawn.CameraSpawnConditions.OffCamera,
                m_CheckSafeArea = true,
                m_CircleRadius = GetMinimumLoadRange(),
                m_DebugName = NameCase,
                m_ObjectsToAvoid = new Bitfield<ObjectTypes>(new ObjectTypes[2] { ObjectTypes.Vehicle, ObjectTypes.Crate }),
                m_CircleIndex = 0,
                m_RejectFunc = null,//new ManFreeSpace.FreeSpaceParams.RejectFunction(),
                m_SearchRadiusMultiplier = 1,
                m_SilentFailIfNoSpaceFound = true,
                m_RejectFuncContext = null,
            };
            //FSP.CustomValidator = new ManFreeSpace.FreeSpaceParams.CustomValidatorFunc(FSP, Position);
            FSF.Setup(FSP, NameCase, true);
            */
        }
        private void SetPositionFarFromPlayer()
        {
            Debug_SMissions.Log(KickStart.ModID + ": SetPositionFarFromPlayer");
            Vector3 randAngle;
            float loadRange;
            float randDistance;
            int attemptCount = 0;
            do
            {
                attemptCount++;
                randAngle = new Vector3(UnityEngine.Random.Range(-1000f, 1000f), 0, UnityEngine.Random.Range(-1000f, 1000f)).normalized;
                loadRange = GetMinimumLoadRange();
                randDistance = UnityEngine.Random.Range(ManSubMissions.MaxLoadedSpawnDist + loadRange, ManSubMissions.MaxUnloadedSpawnDist - loadRange);
                Vector3 pos = Singleton.playerPos + (randAngle * randDistance);
                if (Singleton.Manager<ManWorld>.inst.GetTerrainHeight(ScenePosition, out float height))
                    pos.y = height;
                WorldPos = WorldPosition.FromScenePosition(pos);
            }
            while (maxAttempts > attemptCount && ManVisible.inst.VisiblesTouchingRadius(ScenePosition, loadRange, searchTypes).Count > 0 &&
                !ManSubMissions.IsTooCloseToOtherMission(ManWorld.inst.TileManager.SceneToTileCoord(ScenePosition)));
            if (maxAttempts == attemptCount)
                Debug_SMissions.Log(KickStart.ModID + ": SetPositionFarFromPlayer - FAILED TO FIND A REASONABLE POSITION");
            return;
        }
        private void SetPositionCloseToPlayer()
        {
            Debug_SMissions.Log(KickStart.ModID + ": SetPositionCloseToPlayer");
            Vector3 randAngle;
            float loadRange;
            float randDistance;
            int attemptCount = 0;
            do
            {
                attemptCount++;
                randAngle = new Vector3(UnityEngine.Random.Range(-1000f, 1000f), 0, UnityEngine.Random.Range(-1000f, 1000f)).normalized;
                loadRange = GetMinimumLoadRange();
                randDistance = UnityEngine.Random.Range(loadRange + ManSubMissions.MinLoadedSpawnDist, ManSubMissions.MaxLoadedSpawnDist - loadRange);
                Vector3 pos = Singleton.playerPos + (randAngle * randDistance);
                if (Singleton.Manager<ManWorld>.inst.GetTerrainHeight(ScenePosition, out float height))
                    pos.y = height;
                WorldPos = WorldPosition.FromScenePosition(pos);
            }
            while (maxAttempts > attemptCount && ManVisible.inst.VisiblesTouchingRadius(ScenePosition, loadRange, searchTypes).Count > 0);
            return;
        }
        private void SetPositionATPlayer()
        {
            Debug_SMissions.Log(KickStart.ModID + ": SetPositionATPlayer");
            Vector3 pos = Singleton.playerPos;
            if (Singleton.Manager<ManWorld>.inst.GetTerrainHeight(ScenePosition, out float height))
                pos.y = height;
            WorldPos = WorldPosition.FromScenePosition(pos);
            return;
        }
        private void SetPositionFromPlayer()
        {
            Debug_SMissions.Log(KickStart.ModID + ": SetPositionFromPlayer");
            float loadRange = GetWorldActiveRange();
            Vector3 pos = WorldPos.ScenePosition;
            if (pos.magnitude > loadRange)
            {
                pos = (loadRange * ScenePosition.normalized) + Singleton.playerPos;
            }
            else
                pos = Singleton.playerPos + ScenePosition;
            if (Singleton.Manager<ManWorld>.inst.GetTerrainHeight(ScenePosition, out float height))
                pos.y = height;
            WorldPos = WorldPosition.FromScenePosition(pos);
            return;
        }
        private void SetPositionFromPlayerTankFacing()
        {
            Debug_SMissions.Log(KickStart.ModID + ": SetPositionFromPlayerTankFacing");
            float loadRange = GetWorldActiveRange();
            Vector3 pos = WorldPos.ScenePosition;
            if (pos.magnitude > loadRange)
            {
                pos = (loadRange * ScenePosition.normalized) + Singleton.playerPos;
            }
            else
            {
                if (Singleton.playerTank)
                {
                    pos = Singleton.playerTank.rootBlockTrans.TransformPoint(ScenePosition);
                }
                else
                    pos = Singleton.playerPos + ScenePosition;
            }
            if (Singleton.Manager<ManWorld>.inst.GetTerrainHeight(ScenePosition, out float height))
                pos.y = height;
            WorldPos = WorldPosition.FromScenePosition(pos);
            return;
        }
        private void TryClearAreaForMission()
        {   //N/A
            int removeCount = 0;
            foreach (Visible vis in Singleton.Manager<ManVisible>.inst.VisiblesTouchingRadius(ScenePosition, GetMinimumLoadRange(), new Bitfield<ObjectTypes>(new ObjectTypes[1] { ObjectTypes.Scenery })))
            {   
                if (vis.resdisp.IsNotNull())
                {
                    vis.resdisp.RemoveFromWorld(false, true, true, true);
                    removeCount++;
                }
            }
            Debug_SMissions.Log(KickStart.ModID + ": removed " + removeCount + " scenery items around new mission setup");
        }

        // Mission pointer when unloaded
        [JsonIgnore]
        private Waypoint UnloadedPosWay;
        [JsonIgnore]
        private TrackedVisible UnloadedPosWayVis;
        internal void RunWaypoint(bool show)
        {   //N/A
             if (show && !IgnorePlayerProximity)
            {
                if (UnloadedPosWay == null)
                {
                    CreateNewWaypoint();
                }
                else
                {
                    UnloadedPosWayVis.SetPos(ScenePosition);
                }

            }
            else
            {
                if (UnloadedPosWay)
                {
                    RemoveWaypoint();
                }
            }
        }
        private void CreateNewWaypoint()
        {
            UnloadedPosWay = ManSpawn.inst.HostSpawnWaypoint(ScenePosition - ManWorld.inst.GetSceneToGameWorld(), Quaternion.identity);
            UnloadedPosWayVis = new TrackedVisible(UnloadedPosWay.visible.ID, UnloadedPosWay.visible, ObjectTypes.Waypoint, RadarTypes.AreaQuest);
            ManOverlay.inst.AddWaypointOverlay(UnloadedPosWayVis);
        }
        private bool RemoveWaypoint()
        {
            try
            {
                ManOverlay.inst.RemoveWaypointOverlay(UnloadedPosWayVis);
                Singleton.Manager<ManVisible>.inst.StopTrackingVisible(UnloadedPosWayVis.ID);
                UnloadedPosWayVis.StopTracking();
                if (UnloadedPosWay)
                    UnloadedPosWay.visible.RemoveFromGame();
                UnloadedPosWay = null;
                UnloadedPosWayVis = null;
            }
            catch (Exception e)
            {
                SMUtil.Assert(true, "Mission (Waypoint) ~ " + Name, KickStart.ModID + ": SubMission - Failed: Could not despawn waypoint!", e);
                Debug_SMissions.Log(KickStart.ModID + ": Error - " + e);
            }
            return false;
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

    public enum SubMissionPosition
    {
        FarFromPlayer,
        CloseToPlayer,
        FixedCoordinate,
        OffsetFromPlayer,
        OffsetFromPlayerTechFacing,
        ClassicSelect,
    }
    public enum SubMissionType
    {
        Basic,
        Repeating,
        Immedeate,
        Critical,
    }
    public enum SubMissionLoadState
    {
        NotAvail,           // Mission
        NeedsFirstInit,
        PositionSetReady,
        Loaded,
        Concluded,
    }
}
