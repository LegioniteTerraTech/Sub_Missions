using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Sub_Missions.ManWindows;

namespace Sub_Missions.Steps
{
    [Serializable]
    public class SubMissionStep
    {
        // auto-set
        [JsonIgnore]
        public SubMission Mission;
        [JsonIgnore]
        public SMissionStep stepGenerated;
        [JsonIgnore]
        public bool hasTech = false;
        [JsonIgnore]
        public bool hasBlock = false;
        [JsonIgnore]
        public GUIPopupDisplay AssignedWindow;
        [JsonIgnore]
        public Waypoint AssignedWaypoint;
        [JsonIgnore]
        public TrackedVisible AssignedTracked;

        public SMissionType StepType = SMissionType.StepActSpeak;           // The type this is

        public int ProgressID = 0;          // progress ID this runs on
        public int SuccessProgressID = 0;   // transfer to this when successful

        public Vector3 Position = Vector3.zero;
        public Vector3 Forwards = Vector3.forward;
        public bool Grounded = true;

        // triggered offset when the requirement is not satisfied
        public bool RevProgressIDOffset = false; // instead of stepping one forwards, we step one back

        public EVaribleType VaribleType = EVaribleType.True;           // Selects what the output should be
        public float InputNum = 0;
        public string InputString = null;
        public string InputStringAux = null;
        public int SetGlobalIndex1 = 0;
        public int SetGlobalIndex2 = 0;
        public int SetGlobalIndex3 = 0;


        public int SavedInt = 0;


        public void TrySetup()
        {
            try
            {
                Position += Mission.Position;
                SMUtil.SetPosTerrain(ref Position, Grounded);
                TrySetupOnType();
                stepGenerated.Mission = Mission;
                stepGenerated.SMission = this;
                stepGenerated.TrySetup();
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "SubMissions: Mission " + Mission.Name + " Has a TrySetup error at ProgressID " + ProgressID + ", Type " + StepType.ToString() + ", and will not be able to execute. \nCheck your referenced Techs as there may be errors or inconsistancies in there.");
                Debug.Log(e);
            }
        }
        public void LoadSetup()
        {
            try
            {
                TrySetupOnType();
                stepGenerated.Mission = Mission;
                stepGenerated.SMission = this;
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "SubMissions: Mission " + Mission.Name + " Has a LoadSetup error at ProgressID " + ProgressID + ", Type " + StepType.ToString() + ", and will not be able to execute. \nCheck your referenced Techs as there may be errors or inconsistancies in there.");
                Debug.Log(e);
            }
        }
        public void Trigger() 
        {
            try
            {
                stepGenerated.Trigger();
            }
            catch (Exception e)
            {
                SMUtil.Assert(true, "SubMissions: Mission " + Mission.Name + " Has invalid syntax at ProgressID " + ProgressID + ", Type " + StepType.ToString() + ", and will not be able to execute."); 
                Debug.Log(e);
            }
        }
        public void TrySetupOnType()
        {
            switch (StepType)
            {
                case SMissionType.StepSetupMM:
                    stepGenerated = new StepSetupMM();
                    break;
                case SMissionType.StepSetupTech:
                    stepGenerated = new StepSetupTech();
                    break;
                case SMissionType.StepSetupWaypoint:
                    stepGenerated = new StepSetupWaypoint();
                    break;
                case SMissionType.StepActSpeak:
                    stepGenerated = new StepActSpeak();
                    break;
                case SMissionType.StepActBoost:
                    stepGenerated = new StepActBoost();
                    break;
                case SMissionType.StepActRemove:
                    stepGenerated = new StepActRemove();
                    break;
                case SMissionType.StepActTimer:
                    stepGenerated = new StepActTimer();
                    break;
                case SMissionType.StepActShifter:
                    stepGenerated = new StepActShifter();
                    break;
                case SMissionType.StepActOptions:
                    stepGenerated = new StepActOptions();
                    break;
                case SMissionType.StepActForward:
                    stepGenerated = new StepActForward();
                    break;
                case SMissionType.StepActMessagePurge:
                    stepGenerated = new StepActMessagePurge();
                    break;
                case SMissionType.StepActWin:
                    stepGenerated = new StepActWin();
                    break;
                case SMissionType.StepActFail:
                    stepGenerated = new StepActFail();
                    break;
                case SMissionType.StepCheckResources:
                    stepGenerated = new StepCheckResources();
                    break;
                case SMissionType.StepCheckDestroy:
                    stepGenerated = new StepCheckDestroy();
                    break;
                case SMissionType.StepCheckPlayerDist:
                default:
                    stepGenerated = new StepCheckPlayerDist();
                    break;
            }
        }
    }
    public enum SMissionType
    {   // ACTS
        StepActSpeak,
        StepActWin,
        StepActFail,
        StepActForward,
        StepActShifter,
        StepActRemove,
        StepActBoost,
        StepActTimer,
        StepActOptions,
        StepActMessagePurge,

        // CHECKS
        StepCheckDestroy,
        StepCheckPlayerDist,
        StepCheckResources,

        // SPAWNS
        StepSetupTech,
        StepSetupMM, // ModularMonuments
        StepSetupWaypoint,
    }
}
