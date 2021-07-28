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
        public string Faction = "GSO";
        public int GradeRequired = 0;
        public string Description = "ThisIsNotSetCorrectly";
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

        public int UpdatePassovers = 1; // DON'T TOUCH THIS unless you know EXACTLY what you are doing!

        [JsonIgnore]
        internal bool AwaitingAction = false;
        [JsonIgnore]
        internal int CurrentProgressID = 0;

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
            if (!FixatePositionInWorld)
                SetPositionFromPlayer();
            if (ClearSceneryOnSpawn)
                TryClearAreaForMission();
            bool isMissionImpossible = true;
            foreach (SubMissionStep step in EventList)
            {
                if (step.StepType == SMissionType.StepActWin)
                    isMissionImpossible = false;
            }
            if (isMissionImpossible)
                Debug.Log("SubMissions: " + Name + " is impossible to complete as there are no existing Win Steps in the Event List!!!");
            foreach (SubMissionStep step in EventList)
            {
                step.Mission = this;
                step.TrySetup();
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
        }
        public void Cleanup()
        {   //
            //Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.MissionFailed);
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
                    catch { };
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
            Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.MissionComplete);
            Rewards.Reward(Tree);
            CurrentProgressID = -98;
            TriggerUpdate();
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
        {   // updated every second
            for (int round = 0; round < UpdatePassovers; round++)
            {
                foreach (SubMissionStep step in EventList)
                {
                    if (IsAdjacentTo(step.ProgressID))
                        step.Trigger();
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
                    catch { };
                }
            }
            catch { };
        }


        // Utilities
        public bool IsAdjacentTo(int input)
        {
            return input <= CurrentProgressID + 1 && input >= CurrentProgressID - 1;
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
