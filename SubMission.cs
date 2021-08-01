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
        [JsonIgnore]
        internal SubMissionTree Tree;

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
        public bool ClearTechsOnClear = true;
        public bool ClearSceneryOnSpawn = true;
        public bool FixatePositionInWorld = false;
        public Vector3 Position = Vector3.zero;


        public List<MissionChecklist> CheckList; // MUST be set externally via JSON or built C# code!

        public List<SubMissionStep> EventList; // MUST be set externally via JSON or built C# code!

        public List<bool> VarTrueFalse = new List<bool>();          // MUST be set externally via JSON or built C# code!
        public List<int> VarInts = new List<int>();                 // MUST be set externally via JSON or built C# code!
        //public List<float> VarFloats = new List<float>();           // MUST be set externally via JSON or built C# code!
        //public List<Vector3> VarPositions = new List<Vector3>();    // MUST be set externally via JSON or built C# code!

        public List<TrackedTech> TrackedTechs;  // MUST be set externally via JSON or built C# code!


        public SubMissionReward Rewards; // MUST be set externally via JSON or built C# code!

        public int UpdateSpeedMultiplier = 1; // DON'T TOUCH THIS unless you know EXACTLY what you are doing!
        //  If this value is too high, there WILL be framerate and performance issues!

        [JsonIgnore]
        private float updateLerp = 0;
        [JsonIgnore]
        internal bool AwaitingAction = false;
        [JsonIgnore]
        internal int CurrentProgressID = 0;
        [JsonIgnore]
        private bool IsCleaningUp = false;


        public void GetAndSetDisplayName()
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
        public float GetMinimumLoadRange()
        {   // 
            float minRange = 0;
            foreach (SubMissionStep step in EventList)
            {
                float mag = step.Position.magnitude;
                if (mag > minRange)
                    minRange = mag;
            }
            return minRange;
        }
        public void Startup()
        {   // 
            if (UpdateSpeedMultiplier < 1)
            {
                SMUtil.Assert(false, "SubMissions: " + Name + " UpdateSpeedMultiplier cannot be lower than 1");
                UpdateSpeedMultiplier = 1;
            }

            IsCleaningUp = false;
            if (!FixatePositionInWorld)
                SetPositionFromPlayer();
            if (ClearSceneryOnSpawn)
                TryClearAreaForMission();
            bool isMissionImpossible = true;
            foreach (SubMissionStep step in EventList)
            {
                step.Mission = this;
                step.TrySetup();
                if (step.StepType == SMissionType.ActWin)
                    isMissionImpossible = false;
            }
            if (isMissionImpossible)
                SMUtil.Assert(false, "SubMissions: " + Name + " is impossible to complete as there are no existing Win Steps in the Event List!!!");
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
        }
        public void ReSync()
        {   // 
            IsCleaningUp = false; 
            foreach (SubMissionStep step in EventList)
            {
                step.Mission = this;
                step.LoadSetup();
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
            //Debug.Log("SubMissions: ReSynced");
            //Debug.Log("SubMissions: Tree " + Tree.TreeName);
        }
        public void Cleanup()
        {   //
            IsCleaningUp = true;
            try  // We don't want to crash when the mission maker is still testing
            {       // Will inevitably crash with no checklist items assigned
                CheckList.Clear();
            }
            catch { };
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
            }
            catch { };
            if (EventList != null)
            {
                foreach (SubMissionStep step in EventList)
                {
                    try  // We don't want to crash when the mission maker is still testing
                    {
                        if (step.AssignedWindow != null)
                        {
                            WindowManager.HidePopup(step.AssignedWindow);
                            WindowManager.RemovePopup(step.AssignedWindow);
                        }
                    }
                    catch { }
                    try  // We don't want to crash when the mission maker is still testing
                    {
                        if (step.AssignedWaypoint != null)
                        {
                            step.AssignedWaypoint.visible.RemoveFromGame();
                        }
                    }
                    catch { }
                    try  // We don't want to crash when the mission maker is still testing
                    {
                        if (step.AssignedTracked != null)
                        {
                            ManOverlay.inst.RemoveWaypointOverlay(step.AssignedTracked);
                        }
                    }
                    catch { }
                }
            }
        }
        public void Conclude()
        {   // Like Cleanup but leaves some optional aftermath
            IsCleaningUp = true;
            try  // We don't want to crash when the mission maker is still testing
            {       // Will inevitably crash with no checklist items assigned
                CheckList.Clear();
            }
            catch { };
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
                }
                catch { };
            }
            if (EventList != null)
            {
                foreach (SubMissionStep step in EventList)
                {
                    try  // We don't want to crash when the mission maker is still testing
                    {
                        if (step.AssignedWindow != null)
                        {
                            WindowManager.HidePopup(step.AssignedWindow);
                            WindowManager.RemovePopup(step.AssignedWindow);
                        }
                    }
                    catch { }
                    try  // We don't want to crash when the mission maker is still testing
                    {
                        if (step.AssignedWaypoint != null)
                        {
                            step.AssignedWaypoint.visible.RemoveFromGame();
                        }
                    }
                    catch { }
                    try  // We don't want to crash when the mission maker is still testing
                    {
                        if (step.AssignedTracked != null)
                        {
                            ManOverlay.inst.RemoveWaypointOverlay(step.AssignedTracked);
                        }
                    }
                    catch { }
                }
            }
        }

        public void PurgeAllActiveMessages()
        {   //
            foreach (SubMissionStep step in EventList)
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
        public void Finish()
        {   //
            if (IsCleaningUp)
                return;
            Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.MissionComplete);
            Rewards.Reward(Tree, this);
            CurrentProgressID = -98;
            TriggerUpdate();
            Conclude();
            Tree.FinishedMission(this);
        }
        public void Fail()
        {   //
            Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.MissionFailed);
            CurrentProgressID = -100;
            TriggerUpdate();
            Cleanup();
            ManSubMissions.ActiveSubMissions.Remove(this);
        }


        // UPDATE
        public void TriggerUpdate()
        {   // updated at least every second
            updateLerp += Time.deltaTime;
            if (updateLerp > 1 / UpdateSpeedMultiplier)
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


        // Utilities
        public bool CanRunStep(int input)
        {
            return (input <= CurrentProgressID + 1 && input >= CurrentProgressID - 1) || input == -999;
        }
        public bool GetTechPosHeading(string TechName, out Vector3 pos, out Vector3 direction, out int team)
        {   //
            foreach (SubMissionStep step in EventList)
            {
                if (step.hasTech)
                {
                    if (step.InputString == TechName)
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
            foreach (SubMissionStep step in EventList)
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
        public void SetPositionFromPlayer()
        {
            Vector3 randAngle = new Vector3(UnityEngine.Random.Range(-1000f, 1000f), 0, UnityEngine.Random.Range(-1000f, 1000f)).normalized;
            float loadRange = GetMinimumLoadRange();
            float randDistance = UnityEngine.Random.Range(loadRange + ManSubMissions.MinSpawnDist, ManSubMissions.MaxSpawnDist - loadRange);
            Position = Singleton.playerPos + (randAngle * randDistance);
            if (Singleton.Manager<ManWorld>.inst.GetTerrainHeight(Position, out float height))
                Position.y = height;
            return;
        }
        public void TryClearAreaForMission()
        {   //N/A
            int removeCount = 0;
            foreach (Visible vis in Singleton.Manager<ManVisible>.inst.VisiblesTouchingRadius(Position, GetMinimumLoadRange(), new Bitfield<ObjectTypes>(new ObjectTypes[1] { ObjectTypes.Scenery })))
            {   
                if (vis.resdisp.IsNotNull())
                {
                    vis.resdisp.RemoveFromWorld(false);
                    removeCount++;
                }
            }
            Debug.Log("SubMissions: removed " + removeCount + " scenery items around new mission setup");
        }
    }
}
