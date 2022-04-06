using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using UnityEngine;
using Newtonsoft.Json;
using Sub_Missions.ManWindows;

namespace Sub_Missions.Steps
{
    /// <summary>
    /// Handles the steps
    /// </summary>
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

        [DefaultValue(SMStepType.NULL)]
        public SMStepType StepType = SMStepType.NULL;           // The type this is

        [DefaultValue(SubMission.alwaysRunValue)]
        public int ProgressID = SubMission.alwaysRunValue;       // Progress ID this runs on. Set to SubMission.alwaysRunValue to always run.
        public int SuccessProgressID = 0;   // transfer to this when successful

        public Vector3 Position = Vector3.zero;     // Offset from Mission Origin
        public Vector3 EulerAngles = Vector3.zero;  // Used for MMs
        public Vector3 Forwards = Vector3.zero;  // Used for matters that self-right
        [DefaultValue(2)]
        public int TerrainHandling = 2;
        // 0 = Align with Mission Origin
        // 1 = Snap to Terrain if Position Lower
        // 2 = Align with terrain + offset by position
        // 3 = Snap to Terrain


        // triggered offset when the requirement is not satisfied
        public bool RevProgressIDOffset = false; // instead of stepping one forwards, we step one back

        [DefaultValue(EVaribleType.None)]
        public EVaribleType VaribleType = EVaribleType.None;           // Selects what the output should be
        public float VaribleCheckNum = 0;
        public float InputNum = 0;
        public string InputString = null;
        public string InputStringAux = null;
        public List<SubMissionStep> FolderEventList; // Only ever used by Folders
        [DefaultValue(-1)]
        public int SetMissionVarIndex1 = -1;
        [DefaultValue(-1)]
        public int SetMissionVarIndex2 = -1;
        [DefaultValue(-1)]
        public int SetMissionVarIndex3 = -1;

        [JsonIgnore] // the saving
        public int SavedInt = 0;

        /// <summary>
        /// Upkeeps this Step's position in Scene when the Worldtreadmill does it's treadmill
        /// </summary>
        /// <param name="offset">Treadmill move</param>
        internal void UpdatePosition(IntVector3 offset)
        {
            Position += offset;
        }
        internal void FirstSetup()
        {
            try
            {
                if (Forwards == Vector3.zero)
                    Forwards = Vector3.forward;
                SMUtil.SetPosTerrain(ref Position, Mission.Position, TerrainHandling);
                TrySetupOnType();
                stepGenerated.Mission = Mission;
                stepGenerated.SMission = this;
                stepGenerated.OnInit();
                stepGenerated.FirstSetup();
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "SubMissions: Mission " + Mission.Name + " Has a TrySetup error at ProgressID " + ProgressID + ", Type " + StepType.ToString() + ", and will not be able to execute. \nCheck your referenced Techs as there may be errors or inconsistancies in there.");
                Debug.Log(e);
            }
        }
        internal void LoadStep()
        {
            try
            {
                if (Forwards == Vector3.zero)
                    Forwards = Vector3.forward;
                SMUtil.RealignWithTerrain(ref Position, TerrainHandling);
                TrySetupOnType();
                stepGenerated.Mission = Mission;
                stepGenerated.SMission = this;
                stepGenerated.OnInit();
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "SubMissions: Mission " + Mission.Name + " Has a LoadStep error at ProgressID " + ProgressID + ", Type " + StepType.ToString() + ", and will not be able to execute. \nCheck your referenced Techs as there may be errors or inconsistancies in there.");
                Debug.Log(e);
            }
        }
        internal void UnloadStep()
        {
            try
            {
                stepGenerated.OnDeInit();
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "SubMissions: Mission " + Mission.Name + " Has a UnloadStep error at ProgressID " + ProgressID + ", Type " + StepType.ToString() + ", and will not be able to execute. \nCheck your referenced Techs as there may be errors or inconsistancies in there.");
                Debug.Log(e);
            }
        }
        internal void ClearStep()
        {
            try
            {
                try  // We don't want to crash when the mission maker is still testing
                {
                    if (AssignedWindow != null)
                    {
                        WindowManager.HidePopup(AssignedWindow);
                        WindowManager.RemovePopup(AssignedWindow);
                    }
                }
                catch { }
                try  // We don't want to crash when the mission maker is still testing
                {
                    if (AssignedWaypoint != null)
                    {
                        AssignedWaypoint.visible.RemoveFromGame();
                    }
                }
                catch { }
                try  // We don't want to crash when the mission maker is still testing
                {
                    if (AssignedTracked != null)
                    {
                        ManOverlay.inst.RemoveWaypointOverlay(AssignedTracked);
                    }
                }
                catch { }
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "SubMissions: Mission " + Mission.Name + " Has a UnloadStep error at ProgressID " + ProgressID + ", Type " + StepType.ToString() + ", and will not be able to execute. \nCheck your referenced Techs as there may be errors or inconsistancies in there.");
                Debug.Log(e);
            }
        }
        internal void Trigger() 
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
        internal void TrySetupOnType()
        {
            switch (StepType)
            {
                case SMStepType.Folder:
                    stepGenerated = new StepFolder();
                    break;
                case SMStepType.SetupResources:
                    stepGenerated = new StepSetupResources();
                    break;
                case SMStepType.SetupMM:
                    stepGenerated = new StepSetupMM();
                    break;
                case SMStepType.SetupTech:
                    stepGenerated = new StepSetupTech();
                    break;
                case SMStepType.SetupWaypoint:
                    stepGenerated = new StepSetupWaypoint();
                    break;
                case SMStepType.ActSpeak:
                    stepGenerated = new StepActSpeak();
                    break;
                case SMStepType.ActBoost:
                    stepGenerated = new StepActBoost();
                    break;
                case SMStepType.ActRemove:
                    stepGenerated = new StepActRemove();
                    break;
                case SMStepType.ActTimer:
                    stepGenerated = new StepActTimer();
                    break;
                case SMStepType.ActShifter:
                    stepGenerated = new StepActShifter();
                    break;
                case SMStepType.ActOptions:
                    stepGenerated = new StepActOptions();
                    break;
                case SMStepType.ActForward:
                    stepGenerated = new StepActForward();
                    break;
                case SMStepType.ActMessagePurge:
                    stepGenerated = new StepActMessagePurge();
                    break;
                case SMStepType.ActWin:
                    stepGenerated = new StepActWin();
                    break;
                case SMStepType.ActFail:
                    stepGenerated = new StepActFail();
                    break;
                case SMStepType.ActRandom:
                    stepGenerated = new StepActRandom();
                    break;
                case SMStepType.ActAirstrike:
                    stepGenerated = new StepAirstrike();
                    break;
                case SMStepType.CheckLogic:
                    stepGenerated = new StepCheckLogic();
                    break;
                case SMStepType.CheckResources:
                    stepGenerated = new StepCheckResources();
                    break;
                case SMStepType.CheckHealth:
                    stepGenerated = new StepCheckHealth();
                    break;
                case SMStepType.CheckDestroy:
                    stepGenerated = new StepCheckDestroy();
                    break;
                case SMStepType.ChangeAI:
                    stepGenerated = new StepChangeAI();
                    break;
                case SMStepType.TransformTech:
                    stepGenerated = new StepTransformTech();
                    break;
                case SMStepType.CheckPlayerDist:
                    stepGenerated = new StepCheckPlayerDist();
                    break;
                default:
                    stepGenerated = new StepNull();
                    break;
            }
        }


        internal static string GetALLStepDocumentations()
        {
            StringBuilder SB = new StringBuilder();
            int entries = Enum.GetValues(typeof(SMStepType)).Length;
            SubMissionStep temp = new SubMissionStep();
            SB.Append("-------------------- MISSION STEPS --------------------\n");
            SB.Append("// Note: VaribleTypes are applied to all SetMissionVarIndexes specified unless otherwise noted!\n");
            string Batch =
        "// VaribleType : Condition                   | Action" +
        "\n// None : Always True                        | Nothing" +
        "\n// True : Var is True                        | Sets Var to True" +
        "\n// False : Var is False                      | Sets Var to False" +
        "\n// Int : Var is equal to                     | Sets the value to VaribleCheckNum" +
        "\n// IntGreaterThan : Var is > VaribleCheckNum | Not an Action" +
        "\n// IntLessThan : Var is < VaribleCheckNum    | Not an Action" +
        //"\n// SetPosition : Always True                 | Not an Action" +
        "\n// DoSuccessID : Not a Condition             | Advances the ProgressID to SuccessProgressID";
            SB.Append(Batch);
            SB.Append("// Note: VaribleTypes are applied to all SetMissionVarIndexes specified unless otherwise noted!\n");
            SB.Append("\n--------------------------------------------------------\n"); 
                temp.StepType = (SMStepType)(entries + 10);
            temp.TrySetupOnType();
            SB.Append(temp.stepGenerated.GetDocumentation());
            SB.Append("\n--------------------------------------------------------\n");
            for (int step = 0; step < entries; step++)
            {
                temp.StepType = (SMStepType)step;
                temp.TrySetupOnType();
                SB.Append(temp.stepGenerated.GetDocumentation());
                SB.Append("\n--------------------------------------------------------\n");
            }
            return SB.ToString();
        }
    }
    public enum SMStepType
    {   // error
        NULL,
        // Utilities
        Folder,

        // ACTS
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
        ActRandom,
        ActAirstrike,

        // CHECKS
        CheckLogic,
        CheckHealth,
        CheckDestroy,
        CheckPlayerDist,
        CheckResources,

        // CHANGERS
        ChangeAI,

        // SPAWNS
        SetupTech,
        SetupResources,
        SetupMM, // ModularMonuments
        SetupWaypoint,

        // TRANSFORMERS/TRANSMUTATORS
        TransformTech,
    }
}
