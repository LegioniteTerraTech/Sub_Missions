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

        public SMissionType StepType = SMissionType.ActSpeak;           // The type this is

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
                case SMissionType.SetupMM:
                    stepGenerated = new StepSetupResources();
                    break;
                case SMissionType.SetupTech:
                    stepGenerated = new StepSetupTech();
                    break;
                case SMissionType.SetupWaypoint:
                    stepGenerated = new StepSetupWaypoint();
                    break;
                case SMissionType.ActSpeak:
                    stepGenerated = new StepActSpeak();
                    break;
                case SMissionType.ActBoost:
                    stepGenerated = new StepActBoost();
                    break;
                case SMissionType.ActRemove:
                    stepGenerated = new StepActRemove();
                    break;
                case SMissionType.ActTimer:
                    stepGenerated = new StepActTimer();
                    break;
                case SMissionType.ActShifter:
                    stepGenerated = new StepActShifter();
                    break;
                case SMissionType.ActOptions:
                    stepGenerated = new StepActOptions();
                    break;
                case SMissionType.ActForward:
                    stepGenerated = new StepActForward();
                    break;
                case SMissionType.ActMessagePurge:
                    stepGenerated = new StepActMessagePurge();
                    break;
                case SMissionType.ActWin:
                    stepGenerated = new StepActWin();
                    break;
                case SMissionType.ActFail:
                    stepGenerated = new StepActFail();
                    break;
                case SMissionType.CheckResources:
                    stepGenerated = new StepCheckResources();
                    break;
                case SMissionType.CheckHealth:
                    stepGenerated = new StepCheckHealth();
                    break;
                case SMissionType.CheckDestroy:
                    stepGenerated = new StepCheckDestroy();
                    break;
                case SMissionType.ChangeAI:
                    stepGenerated = new StepChangeAI();
                    break;
                case SMissionType.TransformTech:
                    stepGenerated = new StepTransformTech();
                    break;
                case SMissionType.CheckPlayerDist:
                default:
                    stepGenerated = new StepCheckPlayerDist();
                    break;
            }
        }
    }
    public enum SMissionType
    {   // ACTS
        ActSpeak,
        ActWin,
        ActFail,
        ActForward,
        ActShifter,
        ActRemove,
        ActBoost,
        ActTimer,
        ActOptions,
        ActMessagePurge,

        // CHECKS
        CheckHealth,
        CheckDestroy,
        CheckPlayerDist,
        CheckResources,

        // CHANGERS
        ChangeAI,

        // SPAWNS
        SetupTech,
        SetupMM, // ModularMonuments
        SetupWaypoint,

        // TRANSFORMERS/TRANSMUTATORS
        TransformTech,
    }
}
