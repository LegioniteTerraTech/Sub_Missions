using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using UnityEngine;
using Newtonsoft.Json;
using Sub_Missions.ManWindows;
using TerraTechETCUtil;
using UnityEngine.UI;
using Sub_Missions.Steps.Acts;

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

        /// <summary> Progress ID this runs on. Set to <code>SubMission.alwaysRunValue</code> to always run. </summary>
        [DefaultValue(SubMission.alwaysRunValue)]
        public int ProgressID = SubMission.alwaysRunValue;
        /// <summary> transfer to this ProgressID when successful </summary>
        public int SuccessProgressID = 0;

        /// <summary> Active Mission Processed Position </summary>
        public Vector3 Position = Vector3.zero;     // Processed Position
        /// <summary> The Offset from Mission Origin </summary>
        [JsonIgnore]
        public Vector3 InitPosition = Vector3.zero;
        /// <summary> Used for finely rotating MMs </summary>
        public Vector3 EulerAngles = Vector3.zero;
        /// <summary> Used for matters that self-right </summary>
        public Vector3 Forwards = Vector3.zero;
        /// <summary>
        /// <list type="number">Position relative to Mission Origin
        /// <item>Snap to Terrain if Position Lower</item>
        /// <item>Align with terrain + offset by position</item>
        /// <item>Snap to Terrain</item>
        /// </list>
        /// </summary>
        [DefaultValue(2)]
        public int TerrainHandling = 2;
        // 0 = Position relative to Mission Origin
        // 1 = Snap to Terrain if Position Lower
        // 2 = Align with terrain + offset by position
        // 3 = Snap to Terrain


        // triggered offset when the requirement is not satisfied
        public bool RevProgressIDOffset = false; // instead of stepping one forwards, we step one back

        [DefaultValue(EVaribleType.None)]
        public EVaribleType VaribleType = EVaribleType.None;           // Selects what the output should be
        public EVaribleType VariableType
        {
            get { return VaribleType; }
            set { VaribleType = value; }
        }
        public float VaribleCheckNum = 0;
        public float VariableCheckNum
        {
            get { return VaribleCheckNum; }
            set { VaribleCheckNum = value; }
        }
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

        [JsonIgnore] // When we save the mission we also save this
        public int SavedInt = 0;

        [JsonIgnore]
        public string LogName => (Mission.Name.NullOrEmpty() ? "[NULL MISSION]" : Mission.Name) + " ~ " + StepType + " - " + ProgressID;


        internal SubMissionStep CloneDeep()
        {
            var clone = new SubMissionStep()
            {
                Mission = Mission,
                StepType = StepType,
                ProgressID = ProgressID,
                AssignedWaypoint = AssignedWaypoint,
                AssignedWindow = AssignedWindow,
                RevProgressIDOffset = RevProgressIDOffset,
                AssignedTracked = AssignedTracked,
                EulerAngles = EulerAngles,
                FolderEventList = FolderEventList,
                Forwards = Forwards,
                hasBlock = hasBlock,
                hasTech = hasTech,
                InputStringAux = InputStringAux,
                InitPosition = InitPosition,
                InputNum = InputNum,
                InputString = InputString,
                Position = Position,
                SavedInt = SavedInt,
                SetMissionVarIndex1 = SetMissionVarIndex1,
                SetMissionVarIndex2 = SetMissionVarIndex2,
                SetMissionVarIndex3 = SetMissionVarIndex3,
                stepGenerated = null,
                SuccessProgressID = SuccessProgressID,
                TerrainHandling = TerrainHandling,
                VaribleCheckNum = VaribleCheckNum,
                VaribleType = VaribleType,
            };
            clone.TrySetupOnType();

            return clone;
        }


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
                Position = InitPosition;
                SMUtil.SetPosTerrain(ref Position, Mission.ScenePosition, TerrainHandling);
                TrySetupOnType();
                stepGenerated.Mission = Mission;
                stepGenerated.SMission = this;
                stepGenerated.OnInit();
                stepGenerated.FirstSetup();
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, LogName, KickStart.ModID + ": Mission " + Mission.Name + 
                    " Has a TrySetup error at ProgressID " + ProgressID + ", Type " + StepType.ToString() + 
                    ", and will not be able to execute. \nCheck your referenced Techs as there may be errors " +
                    "or inconsistancies in there.", e);
                //Debug_SMissions.Log(e);
            }
        }
        internal void LoadStep()
        {
            try
            {
                if (Forwards == Vector3.zero)
                    Forwards = Vector3.forward;
                Position = InitPosition;
                SMUtil.RealignWithTerrain(ref Position, Mission.ScenePosition, TerrainHandling);
                TrySetupOnType();
                stepGenerated.Mission = Mission;
                stepGenerated.SMission = this;
                stepGenerated.OnInit();
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, LogName, KickStart.ModID + ": Mission " + Mission.Name + " Has a LoadStep error at ProgressID " +
                    "" + ProgressID + ", Type " + StepType.ToString() + ", and will not be able to execute. " +
                    "\nCheck your referenced Techs as there may be errors or inconsistancies in there.", e);
                Debug_SMissions.Log(e);
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
                SMUtil.Assert(false, LogName, KickStart.ModID + ": Mission " + Mission.Name + " Has a UnloadStep error at ProgressID " + 
                    ProgressID + ", Type " + StepType.ToString() + ", and will not be able to execute. " +
                    "\nCheck your referenced Techs as there may be errors or inconsistancies in there.", e);
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
                SMUtil.Assert(false, LogName, KickStart.ModID + ": Mission " + Mission.Name + " Has a UnloadStep error at ProgressID " + 
                    ProgressID + ", Type " + StepType.ToString() + ", and will not be able to execute. " +
                    "\nCheck your referenced Techs as there may be errors or inconsistancies in there.", e);
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
                SMUtil.Assert(true, LogName, KickStart.ModID + ": Mission " + Mission.Name + " Has invalid syntax at ProgressID " + 
                    ProgressID + ", Type " + StepType.ToString() + ", and will not be able to execute.", e); 
            }
        }
        private static Type GetMissionStepType(SMStepType type)
        {
            switch (type)
            {
                case SMStepType.Folder:
                    return typeof(StepFolder);
                case SMStepType.SetupResources:
                    return typeof(StepSetupResources);
                case SMStepType.SetupMM:
                    return typeof(StepSetupMM);
                case SMStepType.SetupTech:
                    return typeof(StepSetupTech);
                case SMStepType.SetupWaypoint:
                    return typeof(StepSetupWaypoint);
                case SMStepType.ActSpeak:
                    return typeof(StepActSpeak);
                case SMStepType.ActBoost:
                    return typeof(StepActBoost);
                case SMStepType.ActDrive:
                    return typeof(StepActDrive);
                case SMStepType.ActAnchor:
                    return typeof(StepActAnchor);
                case SMStepType.ActRemove:
                    return typeof(StepActRemove);
                case SMStepType.ActTimer:
                    return typeof(StepActTimer);
                case SMStepType.ActShifter:
                    return typeof(StepActShifter);
                case SMStepType.ActOptions:
                    return typeof(StepActOptions);
                case SMStepType.ActForward:
                    return typeof(StepActForward);
                case SMStepType.ActMessagePurge:
                    return typeof(StepActMessagePurge);
                case SMStepType.ActWin:
                    return typeof(StepActWin);
                case SMStepType.ActFail:
                    return typeof(StepActFail);
                case SMStepType.ActRandom:
                    return typeof(StepActRandom);
                case SMStepType.ActAirstrike:
                    return typeof(StepAirstrike);
                case SMStepType.CheckLogic:
                    return typeof(StepCheckLogic);
                case SMStepType.CheckResources:
                    return typeof(StepCheckResources);
                case SMStepType.CheckHealth:
                    return typeof(StepCheckHealth);
                case SMStepType.CheckMoney:
                    return typeof(StepCheckMoney);
                case SMStepType.CheckDestroy:
                    return typeof(StepCheckDestroy);
                case SMStepType.ChangeAI:
                    return typeof(StepChangeAI);
                case SMStepType.ChangeTech:
                    return typeof(StepTransformTech);
                case SMStepType.CheckPlayerDist:
                    return typeof(StepCheckPlayerDist);
                default:
                    return typeof(StepNull);
            }
        }
        public static SMissionStep CreateMissionStep(SMStepType type)
        {
            return (SMissionStep)Activator.CreateInstance(GetMissionStepType(type));
        }
        internal void TrySetupOnType()
        {
            stepGenerated = CreateMissionStep(StepType);
        }

        internal static void ShowStringColorGUI(SMStepType type, string num)
        {
            GUILayout.Space(-20);
            if (type == SMStepType.NULL)
                GUILayout.Label("<color=#DB0C00>" + num + "</color>", AltUI.LabelWhiteTitle, GUILayout.Width(60), GUILayout.Height(40));
            else if (type == SMStepType.Folder)
                GUILayout.Label("<color=#E4E6D1>" + num + "</color>", AltUI.LabelWhiteTitle, GUILayout.Width(60), GUILayout.Height(40));
            else if (type < SMStepType.CheckLogic)
                GUILayout.Label("<color=#E6D900>" + num + "</color>", AltUI.LabelWhiteTitle, GUILayout.Width(60), GUILayout.Height(40));
            else if (type < SMStepType.ChangeAI)
                GUILayout.Label("<color=#09E605>" + num + "</color>", AltUI.LabelWhiteTitle, GUILayout.Width(60), GUILayout.Height(40));
            else if (type < SMStepType.SetupTech)
                GUILayout.Label("<color=#E600DE>" + num + "</color>", AltUI.LabelWhiteTitle, GUILayout.Width(60), GUILayout.Height(40));
            else if (type < SMStepType.OutOfBounds)
                GUILayout.Label("<color=#1FC27B>" + num + "</color>", AltUI.LabelWhiteTitle, GUILayout.Width(60), GUILayout.Height(40));
            else
                GUILayout.Label("<color=#DB0C00>" + num + "</color>", AltUI.LabelWhiteTitle, GUILayout.Width(60), GUILayout.Height(40));
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
        ActDrive,
        ActAnchor,
        ActTimer,
        ActOptions,
        ActMessagePurge,
        ActRandom,
        ActAirstrike,

        // CHECKS
        CheckLogic,
        CheckHealth,
        CheckMoney,
        CheckDestroy,
        CheckPlayerDist,
        CheckResources,

        // CHANGERS
        ChangeAI,
        ChangeTech, // TRANSFORMERS/TRANSMUTATORS

        // SPAWNS
        SetupTech,
        SetupResources,
        SetupMM, // ModularMonuments
        SetupWaypoint,

        // Error
        OutOfBounds,
    }
}
