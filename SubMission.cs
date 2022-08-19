using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;
using Sub_Missions.Steps;
using Sub_Missions.ManWindows;
using Newtonsoft.Json;

namespace Sub_Missions
{
    [Serializable]
    public class SubMission
    {   //  Build the mission!
        //    Core
        public const int alwaysRunValue = int.MinValue;

        [JsonIgnore]
        internal SubMissionTree Tree;
        [JsonIgnore]
        public SubMissionType Type = SubMissionType.Basic;

        public string Name = "Unset";
        [JsonIgnore]
        public string SelectedAltName;
        public List<string> AltNames;
        public string Faction = "GSO";
        public int GradeRequired = 0;
        public string Description = "ThisIsNotSetCorrectly";
        public List<string> AltDescs;
        public byte MinProgressX = 0;
        public byte MinProgressY = 0;
        public bool SinglePlayerOnly = false;
        public bool IgnorePlayerProximity = false;
        public bool ClearTechsOnClear = true;
        public bool ClearModularMonumentsOnClear = true;
        public bool ClearSceneryOnSpawn = true;
        public bool CannotCancel = false;
        public SubMissionPosition SpawnPosition = SubMissionPosition.FarFromPlayer;

        internal FactionSubTypes FactionType => SubMissionTree.GetTreeCorp(Faction);

        // GLOBAL
        public WorldPosition WorldPos => new WorldPosition(TilePos, OffsetFromTile);
        /// <summary>
        /// The ScenePosition of the mission (If needed)
        /// </summary>
        public Vector3 Position = Vector3.zero;
        [JsonIgnore]
        public Vector3 OffsetFromTile = Vector3.zero;
        [JsonIgnore]
        public IntVector2 TilePos = Vector2.zero;


        public List<MissionChecklist> CheckList; // MUST be set externally via JSON or built C# code!

        public List<SubMissionStep> EventList; // MUST be set externally via JSON or built C# code!

        public List<bool> VarTrueFalse = new List<bool>();          // MUST be set externally via JSON or built C# code!
        public List<int> VarInts = new List<int>();                 // MUST be set externally via JSON or built C# code!
        //public List<float> VarFloats = new List<float>();           // MUST be set externally via JSON or built C# code!
        //public List<Vector3> VarPositions = new List<Vector3>();    // MUST be set externally via JSON or built C# code!

        public List<TrackedTech> TrackedTechs;  // MUST be set externally via JSON or built C# code!
        public List<TrackedBlock> TrackedBlocks;  // MUST be set externally via JSON or built C# code!
        [JsonIgnore]
        internal List<SMWorldObject> TrackedMonuments = new List<SMWorldObject>();

        public SubMissionReward Rewards; // MUST be set externally via JSON or built C# code!

        public float UpdateSpeedMultiplier = 1; // DON'T TOUCH THIS unless you know EXACTLY what you are doing!
        //  If this value is too high, there WILL be framerate and performance issues!

        [JsonIgnore]
        public float MissionDist = 1; // Keeps track of how far the mission is

        [JsonIgnore]
        private float updateLerp = 0;
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


        public Vector3 GetWorldPosition()
        {
            return OffsetFromTile + ManWorld.inst.TileManager.CalcTileCentre(TilePos);
        }
        public static string GetDocumentation()
        {
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
                 "\n*         If a Step's ProgressID is set to alwaysRunValue (" + alwaysRunValue + "), it will update all the time regardless of the CurrentProgressID." +
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
                  "\n  \"MinProgressX\": 0,   // Your SubMissionTree.json's ProgressXName required for this mission." +
                  "\n  //  If this value is negative, then it checks based on At Most." +
                  "\n  \"MinProgressX\": 0,   // Your SubMissionTree.json's ProgressXName required for this mission." +
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
                 "\n}," +
                 "\n //Note:  Repeating missions do not reserve their position - do not use for missions with persistant elements!";
    }

        internal void OnMoveWorldOrigin(IntVector3 moveDist)
        {
            //Position += moveDist;
            foreach (SubMissionStep step in GetAllEvents())
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
            foreach (SubMissionStep step in GetAllEvents())
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
        private float minRange = 0;
        /// <summary>
        /// MUST BE ACTIVATED ON A NON-ACTIVE 
        /// </summary>
        /// <returns></returns>
        public float GetMinimumLoadRange()
        {   // 
            if (minRange == 0)
            {
                if (IsActive)
                {
                    SMUtil.Assert(false, "SubMissions: " + Name + " - GetMinimumLoadRange was called after init but was not properly initialised first!");
                }
                foreach (SubMissionStep step in GetAllEvents())
                {
                    float mag = step.Position.magnitude;
                    if (mag > minRange)
                        minRange = mag;
                }
            }
            return minRange;
        }

        internal void Startup()
        {   // 
            if (UpdateSpeedMultiplier < 0.5f)
            {
                SMUtil.Assert(false, "SubMissions: " + Name + " UpdateSpeedMultiplier cannot be lower than 0.5");
                UpdateSpeedMultiplier = 0.5f;
            }
            Debug.Log("SubMissions: Startup for mission " + Name);

            IsCleaningUp = false;
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

            bool isMissionImpossible = true;
            foreach (SubMissionStep step in GetAllEvents())
            {
                if (step.StepType == SMStepType.ActWin)
                    isMissionImpossible = false;
            }
            if (isMissionImpossible)
                SMUtil.Assert(false, "SubMissions: " + Name + " is impossible to complete as there are no existing Win Steps in the Event List!!!");
            
            TilePos = ManWorld.inst.TileManager.SceneToTileCoord(Position);
            OffsetFromTile = Position - ManWorld.inst.TileManager.CalcTileCentreScene(TilePos);
            ActiveState = SubMissionLoadState.NeedsFirstInit;
            UpdateDistance();
            if (IgnorePlayerProximity)
            {
                FirstLoad();
            }
            else
                CheckIfCanSpawn();
        }


        internal void UpdateDistance()
        {   // 
            if (ActiveState == SubMissionLoadState.NeedsFirstInit || ActiveState == SubMissionLoadState.PositionSetReady)
            {
                RunWaypoint(!IgnorePlayerProximity && ManSubMissions.Selected == this);
            }   
            else
                RunWaypoint(false);
            MissionDist = SMUtil.GetPlayerDist(GetWorldPosition() - ManWorld.inst.GetSceneToGameWorld());
        }
        public void CheckIfCanSpawn()
        {   // 
            if (IsActive || MissionDist > ManSubMissions.LoadCheckDist)
                return;
            if (ManWorld.inst.CheckAllTilesAtPositionHaveReachedLoadStep(Position, GetMinimumLoadRange()))
            {
                FirstLoad();
            }
        }
        private void FirstLoad()
        {   // 
            if (Corrupted)
                return; // cannot run!!!
            try
            {
                if (ClearSceneryOnSpawn)
                    TryClearAreaForMission();
                foreach (SubMissionStep step in GetAllEvents())
                {
                    step.Mission = this;
                    step.FirstSetup();
                }
                try
                {
                    foreach (TrackedBlock listEntry in TrackedBlocks)
                    {
                        listEntry.mission = this;
                    }
                }
                catch
                {
                    SMUtil.Assert(false, "SubMissions: Mission " + Name + " has encountered some errors while loading the TrackedBlocks.  Check your syntax.");
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
                    SMUtil.Assert(false, "SubMissions: Mission " + Name + " has encountered some errors while loading the TrackedTechs.  Check your syntax.");
                }
                try
                {
                    foreach (MissionChecklist listEntry in CheckList)
                    {
                        listEntry.mission = this;
                    }
                }
                catch
                {
                    SMUtil.Assert(false, "SubMissions: Mission " + Name + " has encountered some errors while loading the CheckList.  Check your syntax.");
                }

                Debug.Log("SubMissions: Mission " + Name + " has loaded into the world!");
                ActiveState = SubMissionLoadState.Loaded;
            }
            catch (Exception e)
            {
                SMUtil.Assert(true, "SubMissions: Mission " + Name + " has encountered a serious error on spawning and will not be able to load! " + e);
                Corrupted = true; 
            }
        }

        private void ReSync()
        {   // 
            Debug.Log("SubMissions: Resync for SubMission " + Name);
            IsCleaningUp = false;
            Position = WorldPosition.FromGameWorldPosition(GetWorldPosition()).ScenePosition;
            foreach (SubMissionStep step in GetAllEvents())
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
            //Debug.Log("SubMissions: ReSynced");
            //Debug.Log("SubMissions: Tree " + Tree.TreeName);
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
                foreach (SubMissionStep step in GetAllEvents())
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
                        catch { };
                    }
                    TrackedTechs.Clear();
                }
                catch { };
            }
            if (EventList != null)
            {
                foreach (SubMissionStep step in GetAllEvents())
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

        public List<SubMissionStep> GetAllEvents()
        {   //
            List<SubMissionStep> allEvents = new List<SubMissionStep>();
            if (EventList != null)
                allEvents.AddRange(GetEventsRecursive(EventList));
            return allEvents;
        }
        private List<SubMissionStep> GetEventsRecursive(List<SubMissionStep> toSearch)
        {   //
            List<SubMissionStep> allEvents = new List<SubMissionStep>();
            foreach (SubMissionStep step in toSearch)
            {
                allEvents.Add(step);
                if (step.StepType == SMStepType.Folder && step.FolderEventList != null && step.FolderEventList.Count > 0)
                    allEvents.AddRange(GetEventsRecursive(step.FolderEventList));
            }
            return allEvents;
        }
        internal void PurgeAllActiveMessages()
        {   //
            foreach (SubMissionStep step in GetAllEvents())
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
        internal void Finish()
        {   //
            if (IsCleaningUp)
                return;
            Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.MissionComplete);
            Rewards.Reward(Tree, this);
            CurrentProgressID = -98;
            EncounterShoehorn.FinishSubMission(this, ManEncounter.FinishState.Completed);
            TriggerUpdate();
            Conclude();
            Tree.FinishedMission(this);
        }
        internal void Fail()
        {   //
            Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.MissionFailed);
            CurrentProgressID = -100;
            EncounterShoehorn.FinishSubMission(this, ManEncounter.FinishState.Failed);
            TriggerUpdate();
            Cleanup();
            ManSubMissions.ActiveSubMissions.Remove(this);
        }


        // UPDATE
        internal void TriggerUpdate()
        {   // updated at least every half second

            if (IsActive || IgnorePlayerProximity)
            {
                updateLerp += Time.deltaTime;
                if (updateLerp * UpdateSpeedMultiplier > 1)
                {
                    updateLerp = 0;
                    int position = 0;
                    foreach (SubMissionStep step in EventList)
                    {
                        if (CanRunStep(step.ProgressID))
                        {
                            try
                            {   // can potentially fire too early before mission is set
                                step.Trigger();
                            }
                            catch
                            {
                                Debug.Log("SubMissions: Error on attempting step lerp " + position + " in relation to " + CurrentProgressID + " of mission " + Name + " in tree " + Tree.TreeName);
                                try
                                {
                                    Debug.Log("SubMissions: Type of " + step.StepType.ToString() + " ProgressID " + step.ProgressID + " | Is connected to a mission: " + (step.Mission != null).ToString());
                                }
                                catch
                                {
                                    Debug.Log("SubMissions: Confirmed null");
                                }
                            }
                        }
                        position++;
                    }
                }
                try
                {   // can potentially fire too early before mission is set
                    foreach (MissionChecklist task in CheckList)
                    {
                        try
                        {
                            task.TestForCompleted();
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }

        internal Encounter GetEncounterInfoL()
        {
            return null;
        }
        /*
        internal Encounter GetEncounterInfo()
        {
            GameObject tempOb = new GameObject("Temp - " + Name);
            EncounterDetails encD = tempOb.AddComponent<EncounterDetails>();
            encD.
            encD.AwardBB = Rewards ? Rewards.MoneyGain : 0;
            Encounter enc = tempOb.AddComponent<Encounter>();
        }*/

        internal EncounterDisplayData GetEncounterDisplayInfo()
        {
            EncounterDisplayData EDD = EncounterShoehorn.GetEncounterDisplayInfo(this);
            return EDD;
        }


        // Utilities
        internal bool CanRunStep(int input)
        {
            return (input == alwaysRunValue || input <= CurrentProgressID + 1 && input >= CurrentProgressID - 1);
        }
        public bool GetTechPosHeading(string TechName, out Vector3 pos, out Vector3 direction, out int team)
        {   //
            if (!IsActive)
                Debug.Log("SubMissions: GetTechPosHeading - Called when MISSION IS INACTIVE");
            int hash = TechName.GetHashCode();
            foreach (SubMissionStep step in GetAllEvents())
            {
                if (step.hasTech)
                {
                    if (step.InputString.GetHashCode() == hash)
                    {
                        pos = step.Position;
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
            foreach (SubMissionStep step in GetAllEvents())
            {
                if (step.hasBlock)
                {
                    if (step.InputString == BlockName)
                    {
                        pos = step.Position;
                        return true;
                    }
                }
            }
            pos = Vector3.zero;
            return false;
        }

        private const int maxAttempts = 32;
        private static Bitfield<ObjectTypes> searchTypes = new Bitfield<ObjectTypes>(new ObjectTypes[2] { ObjectTypes.Vehicle, ObjectTypes.Crate });
        private void SetPositionClassicMission()
        {
            Debug.Log("SubMissions: SetPositionClassicMission");
            List<WorldTile> IV = new List<WorldTile>();
            Vector2 playerTile = ManWorld.inst.TileManager.SceneToTileCoord(Singleton.playerPos);
            foreach (WorldTile IVc in ManWorld.inst.TileManager.IterateTiles(WorldTile.State.Created))
            {
                if (IVc.Terrain && !IVc.IsPopulated)
                    IV.Add(IVc);
            }
            IV = IV.OrderBy(x => ((Vector2)x.Coord - playerTile).sqrMagnitude).ToList();
            int missionReqRad = (int)GetMinimumLoadRange();
            foreach (WorldTile IVc in IV)
            {
                if (IVc.LargestFreeSpaceOnTile > missionReqRad && !ManSubMissions.IsTooCloseToOtherMission(IVc.Coord))
                {
                    Position = IVc.CalcSceneCentre();
                    Debug.Log("Decided on Scene Position " + Position);
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
            Debug.Log("SubMissions: SetPositionFarFromPlayer");
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
                Position = Singleton.playerPos + (randAngle * randDistance);
                if (Singleton.Manager<ManWorld>.inst.GetTerrainHeight(Position, out float height))
                    Position.y = height;
            }
            while (maxAttempts > attemptCount && ManVisible.inst.VisiblesTouchingRadius(Position, loadRange, searchTypes).Count > 0 &&
                !ManSubMissions.IsTooCloseToOtherMission(ManWorld.inst.TileManager.SceneToTileCoord(Position)));
            if (maxAttempts == attemptCount)
                Debug.Log("SubMissions: SetPositionFarFromPlayer - FAILED TO FIND A REASONABLE POSITION");
            return;
        }
        private void SetPositionCloseToPlayer()
        {
            Debug.Log("SubMissions: SetPositionCloseToPlayer");
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
                Position = Singleton.playerPos + (randAngle * randDistance);
                if (Singleton.Manager<ManWorld>.inst.GetTerrainHeight(Position, out float height))
                    Position.y = height;
            }
            while (maxAttempts > attemptCount && ManVisible.inst.VisiblesTouchingRadius(Position, loadRange, searchTypes).Count > 0);
            return;
        }
        private void SetPositionFromPlayer()
        {
            Debug.Log("SubMissions: SetPositionFromPlayer");
            float loadRange = GetMinimumLoadRange();
            if (Position.magnitude > loadRange)
            {
                Position = (loadRange * Position.normalized) + Singleton.playerPos;
            }
            else 
                Position = Singleton.playerPos + Position;
            if (Singleton.Manager<ManWorld>.inst.GetTerrainHeight(Position, out float height))
                Position.y = height;
            return;
        }
        private void SetPositionFromPlayerTankFacing()
        {
            Debug.Log("SubMissions: SetPositionFromPlayerTankFacing");
            float loadRange = GetMinimumLoadRange();
            if (Position.magnitude > loadRange)
            {
                Position = (loadRange * Position.normalized) + Singleton.playerPos;
            }
            else
            {
                if (Singleton.playerTank)
                {
                    Position = Singleton.playerTank.rootBlockTrans.TransformPoint(Position);
                }
                else
                    Position = Singleton.playerPos + Position;
            }
            if (Singleton.Manager<ManWorld>.inst.GetTerrainHeight(Position, out float height))
                Position.y = height;
            return;
        }
        private void TryClearAreaForMission()
        {   //N/A
            int removeCount = 0;
            foreach (Visible vis in Singleton.Manager<ManVisible>.inst.VisiblesTouchingRadius(Position, GetMinimumLoadRange(), new Bitfield<ObjectTypes>(new ObjectTypes[1] { ObjectTypes.Scenery })))
            {   
                if (vis.resdisp.IsNotNull())
                {
                    vis.resdisp.RemoveFromWorld(false, true, true, true);
                    removeCount++;
                }
            }
            Debug.Log("SubMissions: removed " + removeCount + " scenery items around new mission setup");
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
                    UnloadedPosWayVis.SetPos(Position);
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
            UnloadedPosWay = ManSpawn.inst.HostSpawnWaypoint(GetWorldPosition() - ManWorld.inst.GetSceneToGameWorld(), Quaternion.identity);
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
                SMUtil.Assert(true, "SubMissions: SubMission - Failed: Could not despawn waypoint!");
                Debug.Log("SubMissions: Error - " + e);
            }
            return false;
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
    }
}
