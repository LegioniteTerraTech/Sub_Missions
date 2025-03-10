﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using TerraTechETCUtil;
using Sub_Missions.Steps;
using Sub_Missions.ManWindows;

namespace Sub_Missions.Editor
{
    public interface SMSFieldGUI
    {
        string name { get; }
        ESMSFields type { get;}
        SubMissionStep context { get; }

        void Display(SubMissionStep runData);
        void DoDisplay(SubMissionStep runData);
        void RefreshGUI(SubMissionStep runData);
        void UpdateScene(SubMissionStep runData);
    }
    public class SMSFieldGUI<T> : SMAutoFill<SubMissionStep, T, ESMSFields>, SMSFieldGUI
    {
        protected SMSFieldGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            throw new NotImplementedException("SMSFieldGUI<T> is derived class, not a standalone");
        }

        protected bool UpdatePosDispSelect(Vector3 scenePos, Color dispColor, out Vector3 hitPos)
        {
            hitPos = Vector3.zero;
            DebugExtUtilities.DrawDirIndicator(scenePos, scenePos + new Vector3(0, 16, 0), dispColor, Time.deltaTime + 0.01f);
            if (Input.GetMouseButtonUp(0) && !ManSubMissions.Editor.Display.CursorWithinWindow)
            {
                var cam = Singleton.cameraTrans;
                var gbls = Globals.inst;
                if (cam && Physics.Raycast(ManUI.inst.ScreenPointToRay(Input.mousePosition),
                    out RaycastHit hit, 750, gbls.layerTerrain.mask, QueryTriggerInteraction.Ignore))
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                    hitPos = hit.point;
                    return true;
                }
            }
            return false;
        }
        protected void UpdatePosDisp(Vector3 scenePos, Color dispColor)
        {
            DebugExtUtilities.DrawDirIndicator(scenePos, scenePos + new Vector3(0, 16, 0), dispColor, Time.deltaTime + 0.01f);
        }

        protected void UpdateSphereDisp(Vector3 scenePos, Color dispColor, float radius = 1)
        {
            DebugExtUtilities.DrawDirIndicatorSphere(scenePos, radius, dispColor, Time.deltaTime + 0.01f);
        }


        protected override object FuncLookup(SubMissionStep runData)
        {
            switch (type)
            {
                case ESMSFields.Position:
                    return runData.InitPosition;
                case ESMSFields.EulerAngles:
                    return runData.EulerAngles;
                case ESMSFields.Forwards:
                    return runData.Forwards;
                case ESMSFields.TerrainHandling:
                    return runData.TerrainHandling;
                case ESMSFields.RevProgressIDOffset:
                    return runData.RevProgressIDOffset;
                case ESMSFields.VaribleCheckNum:
                    return runData.VaribleCheckNum;
                case ESMSFields.VaribleType:
                    return runData.VaribleType;
                case ESMSFields.InputNum:
                case ESMSFields.InputNum_int:
                case ESMSFields.InputNum_radius:
                    return runData.InputNum;
                case ESMSFields.InputString:
                case ESMSFields.InputString_float:
                case ESMSFields.InputString_large:
                case ESMSFields.InputString_Tech:
                case ESMSFields.InputString_Tracked_Tech:
                    return runData.InputString;
                case ESMSFields.InputStringAux:
                case ESMSFields.InputStringAux_float:
                case ESMSFields.InputStringAux_large:
                case ESMSFields.InputStringAux_Tech:
                case ESMSFields.InputStringAux_Tracked_Tech:
                    return runData.InputStringAux;
                case ESMSFields.FolderEventList:
                    return runData.FolderEventList;
                case ESMSFields.SetMissionVarIndex1:
                    return runData.SetMissionVarIndex1;
                case ESMSFields.SetMissionVarIndex2:
                    return runData.SetMissionVarIndex2;
                case ESMSFields.SetMissionVarIndex3:
                    return runData.SetMissionVarIndex3;
                case ESMSFields.SuccessProgressID:
                    return runData.SuccessProgressID;
                default:
                    throw new IndexOutOfRangeException("SMSFieldGUI.settable_set called on an invalid instance of type " + type.ToString());
            }
        }
        protected override void FuncSave(SubMissionStep runData, object input)
        {
            try
            {
                switch (type)
                {
                    case ESMSFields.Position:
                        runData.InitPosition = (Vector3)input;
                        runData.Position = runData.InitPosition;
                        SMUtil.RealignWithTerrain(ref runData.Position, runData.Mission.ScenePosition,
                            runData.TerrainHandling);
                        break;
                    case ESMSFields.EulerAngles:
                        runData.EulerAngles = (Vector3)input;
                        break;
                    case ESMSFields.Forwards:
                        runData.Forwards = (Vector3)input;
                        break;
                    case ESMSFields.TerrainHandling:
                        runData.TerrainHandling = (int)input;
                        runData.Position = runData.InitPosition;
                        SMUtil.RealignWithTerrain(ref runData.Position, runData.Mission.ScenePosition,
                            runData.TerrainHandling);
                        break;
                    case ESMSFields.RevProgressIDOffset:
                        runData.RevProgressIDOffset = (bool)input;
                        break;
                    case ESMSFields.VaribleCheckNum:
                        runData.VaribleCheckNum = (float)input;
                        break;
                    case ESMSFields.VaribleType:
                        runData.VaribleType = (EVaribleType)input;
                        break;
                    case ESMSFields.InputNum:
                    case ESMSFields.InputNum_int:
                    case ESMSFields.InputNum_radius:
                        runData.InputNum = (float)input;
                        break;
                    case ESMSFields.InputString:
                    case ESMSFields.InputString_float:
                    case ESMSFields.InputString_large:
                    case ESMSFields.InputString_Tech:
                    case ESMSFields.InputString_Tracked_Tech:
                        runData.InputString = (string)input;
                        break;
                    case ESMSFields.InputStringAux:
                    case ESMSFields.InputStringAux_float:
                    case ESMSFields.InputStringAux_large:
                    case ESMSFields.InputStringAux_Tech:
                    case ESMSFields.InputStringAux_Tracked_Tech:
                        runData.InputStringAux = (string)input;
                        break;
                    case ESMSFields.FolderEventList:
                        throw new UnauthorizedAccessException("SMSFieldGUI.set_settable[FolderEventList] does not support replacement of list fields");
                    case ESMSFields.SetMissionVarIndex1:
                        runData.SetMissionVarIndex1 = (int)input;
                        break;
                    case ESMSFields.SetMissionVarIndex2:
                        runData.SetMissionVarIndex2 = (int)input;
                        break;
                    case ESMSFields.SetMissionVarIndex3:
                        runData.SetMissionVarIndex3 = (int)input;
                        break;
                    case ESMSFields.SuccessProgressID:
                        runData.SuccessProgressID = (int)input;
                        break;
                    default:
                        throw new IndexOutOfRangeException("SMSFieldGUI.set_settable called on an invalid instance of type " + type.ToString());
                }
            }
            catch (InvalidCastException e)
            {
                throw new InvalidCastException("Invalid cast attempt from " + typeof(T).Name + " to parameter " + type.ToString(), e);
            }
        }
    }

    // -----------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------


    public class SMSFieldNullGUI : SMSFieldGUI<object>
    {
        internal SMSFieldNullGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
        }
    }
    public class SMSFieldWIPGUI : SMSFieldGUI<object>
    {
        internal SMSFieldWIPGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            GUILayout.Label("This is WIP!");
        }
    }

    public class SMSFieldBoolGUI : SMSFieldGUI<bool>
    {
        internal SMSFieldBoolGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            if (DisplayBoolean(settable))
                settable = !settable;
        }
    }
    public class SMSFieldIntGUI : SMSFieldGUI<int>
    {
        private string setCache = "";
        internal SMSFieldIntGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            if (DisplayInt(settable, ref setCache, out int newSet))
                settable = newSet;
        }
        public override void RefreshGUI(SubMissionStep runData) 
        {
            setCache = settable.ToString();
        }
    }
    public class SMSFieldFloatGUI : SMSFieldGUI<float>
    {
        protected string setCache = "";
        internal SMSFieldFloatGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            if (DisplayFloat(settable, ref setCache, out var newSet))
                settable = newSet;
        }
        public override void RefreshGUI(SubMissionStep runData)
        {
            setCache = settable.ToString();
        }
    }
    public class SMSFieldRadiusGUI : SMSFieldFloatGUI
    {
        private bool isSelecting = false;
        internal SMSFieldRadiusGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            if (DisplayFloat(settable, ref setCache, out var newSet))
            {
                isSelecting = true;
                settable = newSet;
            }
        }
        public void DelayedHide()
        {
            try
            {
                isSelecting = false;
            }
            catch { }
        }
        public override void UpdateScene(SubMissionStep runData)
        {
            Color dispColor = SMMissionGUI.PositioningColor[runData.StepType];
            if (isSelecting)
            {
                UpdateSphereDisp(runData.InitPosition, dispColor, settable);
                InvokeHelper.InvokeSingle(DelayedHide, SMMissionGUI.PositioningHighlightStopDelay);
            }
            else
            {
                dispColor *= new Color(1, 1, 1, 0.3f);
                UpdateSphereDisp(runData.InitPosition, dispColor, settable);
            }
        }

    }
    public class SMSFieldFloatIntGUI : SMSFieldGUI<float>
    {
        private string setCache = "";
        internal SMSFieldFloatIntGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            if (DisplayFloatRounded(settable, ref setCache, out var newSet))
                settable = newSet;
        }
        public override void RefreshGUI(SubMissionStep runData)
        {
            setCache = settable.ToString("F");
        }
    }
    public class SMSFieldVector3GUI : SMSFieldGUI<Vector3>
    {
        protected string setCache1 = "";
        protected string setCache2 = "";
        protected string setCache3 = "";
        internal SMSFieldVector3GUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            if (DisplayVec3(settable, ref setCache1, ref setCache2, ref setCache3, out Vector3 newSet))
                settable = newSet;
        }
        public override void RefreshGUI(SubMissionStep runData)
        {
            setCache1 = settable.x.ToString();
            setCache2 = settable.y.ToString();
            setCache3 = settable.z.ToString();
        }
    }
    public class SMSFieldStringGUI : SMSFieldGUI<string>
    {
        internal SMSFieldStringGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            if (DisplayString(settable, out var newSet))
                settable = newSet;
        }
    }
    public class SMSFieldStringLargeGUI : SMSFieldGUI<string>
    {
        internal SMSFieldStringLargeGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            if (DisplayStringArea(settable, out var newSet))
                settable = newSet;
        }
    }
    public class SMSFieldStringFloatGUI : SMSFieldGUI<string>
    {
        internal SMSFieldStringFloatGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            if (DisplayStringFloat(settable, out var newSet))
                settable = newSet;
        }
    }


    // -----------------------------------------------------------------------------------------

    public abstract class SMSFieldFixedOptionsFloatGUI : SMSFieldGUI<float>
    {
        private bool opened = false;
        private Vector2 scroller = Vector2.zero;
        protected abstract string[] options { get; }
        protected abstract int[] lookup { get; }
        private readonly Dictionary<int, int> lookupInv;
        internal SMSFieldFixedOptionsFloatGUI(string name, ESMSFields type) : base(name, type) 
        {
            if (lookup != null)
            {
                var batcher = new Dictionary<int, int>();
                for (int i = 0; i < lookup.Length; i++)
                {
                    batcher.Add(lookup[i], i);
                }
                lookupInv = batcher;
            }
        }
        public override void Display(SubMissionStep runData)
        {
            if (DisplayFixedOptions(settable, ref scroller, ref opened, options, lookup, lookupInv, out var newSet))
                settable = newSet;
        }
    }
    public abstract class SMSFieldFixedOptionsGUI : SMSFieldGUI<int>
    {
        private bool opened = false;
        private Vector2 scroller = Vector2.zero;
        protected abstract string[] options { get; }
        protected abstract int[] lookup { get; }
        private readonly Dictionary<int, int> lookupInv;
        internal SMSFieldFixedOptionsGUI(string name, ESMSFields type) : base(name, type)
        {
            if (lookup != null)
            {
                var invBatcher = new Dictionary<int, int>();
                for (int i = 0; i < lookup.Length; i++)
                {
                    invBatcher.Add(lookup[i], i);
                }
                lookupInv = invBatcher;
            }
        }
        public override void Display(SubMissionStep runData)
        {
            try
            {
                if (DisplayFixedOptions(settable, ref scroller, ref opened, options, lookup, lookupInv, out var newSet))
                    settable = newSet;
            }
            catch (Exception e)
            {
                throw new Exception("Error in " + GetType().Name, e);
            }
        }
    }
    public class SMSFieldPositionGUI : SMSFieldVector3GUI
    {
        private bool isSelecting = false;
        private bool isShowing = false;
        internal SMSFieldPositionGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (DisplayVec3(settable, ref setCache1, ref setCache2, ref setCache3, out Vector3 newSet))
                settable = newSet;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (Singleton.playerTank && GUILayout.Button("Own Position", AltUI.ButtonBlue))
            {
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                settable = Singleton.playerTank.boundsCentreWorld;
            }
            if (GUILayout.Button(isSelecting ? "Selecting" : "Select", isSelecting ? AltUI.ButtonBlueActive : AltUI.ButtonBlue))
            {
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                isSelecting = !isSelecting;
                isShowing = true;
            }
        }
        public void DelayedHide()
        {
            try
            {
                isShowing = false;
            }
            catch { }
        }
        public override void UpdateScene(SubMissionStep runData)
        {
            Color dispColor = SMMissionGUI.PositioningColor[runData.StepType];
            if (isSelecting)
            {
                if (UpdatePosDispSelect(settable, dispColor, out var hit))
                {
                    var posS = hit - runData.Mission.ScenePosition;
                    if (posS.ApproxZero())
                        settable = Vector3.zero;
                    else
                        settable = posS;
                    isSelecting = false;
                    InvokeHelper.InvokeSingle(DelayedHide, SMMissionGUI.PositioningHighlightStopDelay);
                }
            }
            else
            {
                dispColor *= new Color(1, 1, 1, 0.3f);
                UpdatePosDisp(settable, dispColor);
            }
        }
    }
    public class SMSFieldEulerGUI : SMSFieldVector3GUI
    {
        internal SMSFieldEulerGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            if (DisplayVec3(settable, ref setCache1, ref setCache2, ref setCache3, out Vector3 newSet))
                settable = newSet;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (Singleton.playerTank && GUILayout.Button("Own Rotation", AltUI.ButtonBlue))
            {
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                settable = Singleton.playerTank.rootBlockTrans.rotation.eulerAngles;
            }
        }
    }
    public class SMSFieldFacingOptionsGUI : SMSFieldVector3GUI
    {
        private bool opened = false;
        private Vector2 scroller = Vector2.zero;
        private float angleCache = 0;
        private Vector3 lastVec = Vector3.forward;
        private bool isAngle = true;
        protected Dictionary<Vector3, KeyValuePair<int, string>> options =
            new Dictionary<Vector3, KeyValuePair<int, string>>() {
                { new Vector3(Mathf.Cos(0.5f), 0, Mathf.Cos(0.5f)),     new KeyValuePair<int, string>(0, "Angle") },
                { Vector3.zero,     new KeyValuePair<int, string>(1, "Custom") },
                { Vector3.forward,  new KeyValuePair<int, string>(2, "North") },
                { Vector3.back,     new KeyValuePair<int, string>(3, "South") },
                { Vector3.right,    new KeyValuePair<int, string>(4, "East") },
                { Vector3.left,     new KeyValuePair<int, string>(5, "West") },
            };
        protected List<Vector3> optionsLookup =
            new List<Vector3> {
                new Vector3(Mathf.Cos(0.5f), 0, Mathf.Cos(0.5f)),
                Vector3.zero,
                Vector3.forward,
                Vector3.back,
                Vector3.right,
                Vector3.left,
            };
        internal SMSFieldFacingOptionsGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            if (!lastVec.Approximately(settable, 0.001f))
            { // magnitude is an expensive call
                isAngle = settable.magnitude.Approximately(1);
                lastVec = settable;
            }
            if (options.TryGetValue(settable, out var nameD))
            {
                if (GUILayout.Button(nameD.Value))
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Button);
                    opened = !opened;
                }
            }
            else
            {
                if (GUILayout.Button(isAngle ? "Angle" : "Custom"))
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Button);
                    opened = !opened;
                }
            }
            if (opened)
            {
                GUILayout.BeginVertical();
                scroller = GUILayout.BeginScrollView(scroller, AltUI.TextfieldBlackHuge, GUILayout.Height(420));
                if (Singleton.playerTank && GUILayout.Button("Cab Forwards", WindowManager.styleBorderedFont))
                {
                    settable = Singleton.playerTank.rootBlockTrans.forward;
                    opened = false;
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                }
                foreach (var item in options)
                {
                    if (GUILayout.Button(item.Value.Value, WindowManager.styleBorderedFont))
                    {
                        if (item.Value.Key != 0)
                            settable = optionsLookup[item.Value.Key];
                        opened = false;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
                    }
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (DisplayVec3(settable, ref setCache1, ref setCache2, ref setCache3, out Vector3 newSet))
                    settable = newSet;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (isAngle)
                {
                    float angle = Vector3.SignedAngle(Vector3.forward, settable, Vector3.up);
                    if (!angleCache.Approximately(angle))
                    {
                        angleCache = angle;
                    }
                    string set = GUILayout.TextField(angleCache.ToString("F"), 32, AltUI.TextfieldBlackAdjusted, GUILayout.Width(180));
                    if (float.TryParse(set, out float val))
                    {
                        GUILayout.Label("<color=green>O</color>", GUILayout.Width(25));
                        if (!angleCache.Approximately(angle))
                        {
                            settable = Quaternion.Euler(0, val, 0) * Vector3.forward;
                            ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                            angleCache = val;
                        }
                    }
                    else
                    {
                        GUILayout.Label("<color=red>X</color>", GUILayout.Width(25));
                    }
                }
            }
        }
    }
    public class SMSFieldTerrainOptionsGUI : SMSFieldFixedOptionsGUI
    {
        private static readonly string[] optionsSet = new string[] {
                "Mission Origin",
                "Above Terrain",
                "Aligned Terrain",
                "Flush Terrain",
            };
        protected override string[] options => optionsSet;
        protected override int[] lookup => null;
        internal SMSFieldTerrainOptionsGUI(string name, ESMSFields type) : base(name, type) { }
    }
    /*
     *  "// VaribleType : Condition                   | Action" +
     *  "\n// None : Always True                        | Nothing" +
     *  "\n// True : Var is True                        | Sets Var to True" +
     *  "\n// False : Var is False                      | Sets Var to False" +
     *  "\n// Int : Var is equal to                     | Sets the value to VaribleCheckNum" +
     *  "\n// IntGreaterThan : Var is > VaribleCheckNum | Not an Action" +
     *  "\n// IntLessThan : Var is < VaribleCheckNum    | Not an Action" +
     *  "\n// DoSuccessID : Not a Condition             | Advances the ProgressID to SuccessProgressID";
     */
    public class SMSFieldVarConditionsGUI : SMSFieldFixedOptionsGUI
    {
        private static readonly string[] optionsSet = {
            EVaribleType.None.ToString(),
            EVaribleType.True.ToString(),
            EVaribleType.False.ToString(),
            EVaribleType.Int.ToString(),
            EVaribleType.DoSuccessID.ToString(),
        };
        protected override string[] options => optionsSet;

        private static readonly int[] lookupSet = {
            (int)EVaribleType.None,
            (int)EVaribleType.True,
            (int)EVaribleType.False,
            (int)EVaribleType.Int,
            (int)EVaribleType.DoSuccessID,
        };
        protected override int[] lookup => lookupSet;
        internal SMSFieldVarConditionsGUI(string name, ESMSFields type) : base(name, type) { }
    }

    public class SMSFieldVarActionsGUI : SMSFieldFixedOptionsGUI
    {
        private static readonly string[] optionsSet = {
            EVaribleType.None.ToString(),
            EVaribleType.True.ToString(),
            EVaribleType.False.ToString(),
            EVaribleType.Int.ToString(),
            EVaribleType.IntGreaterThan.ToString(),
            EVaribleType.IntLessThan.ToString(),
        };
        protected override string[] options => optionsSet;

        protected override int[] lookup => null;
        internal SMSFieldVarActionsGUI(string name, ESMSFields type) : base(name, type) { }
    }

    // -----------------------------------------------------------------------------------------

    public class SMSFieldOptionsFloatGUI : SMSFieldGUI<float>
    {
        private bool opened = false;
        private Vector2 scroller = Vector2.zero;
        private string[] options;
        private Dictionary<int, SMSFieldGUI> optionsExtended;
        internal SMSFieldOptionsFloatGUI(string name, ESMSFields type, SMissionStep step, string[] options,
            Dictionary<int, KeyValuePair<string, ESMSFields>> ext = null) : base(name, type)
        {
            this.options = options;

            if (ext != null)
            {
                optionsExtended = new Dictionary<int, SMSFieldGUI>();
                foreach (var item in ext)
                {
                    optionsExtended.Add(item.Key,
                        SMStepEditorGUI.GetField(step, item.Value.Value, "- " + item.Value.Key));
                }
            }
        }
        public override void Display(SubMissionStep runData)
        {
            GUILayout.BeginVertical();
            int num = Mathf.RoundToInt(settable);
            if (GUILayout.Button(options[num]))
            {
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Button);
                opened = !opened;
            }
            if (opened)
            {
                scroller = GUILayout.BeginScrollView(scroller, AltUI.TextfieldBlackHuge, GUILayout.Height(420));
                for (int step = 0; step < options.Length; step++)
                {
                    if (GUILayout.Button(options[step], WindowManager.styleBorderedFont))
                    {
                        settable = step;
                        opened = false;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
                    }
                }
                GUILayout.EndScrollView();
            }
            else if (optionsExtended != null && optionsExtended.TryGetValue(num, out SMSFieldGUI GUIext))
            {
                GUILayout.BeginHorizontal();
                GUIext.DoDisplay(runData);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
    }
    public class SMSFieldOptionsStringGUI : SMSFieldGUI<string>
    {
        private bool opened = false;
        private Vector2 scroller = Vector2.zero;
        private string[] options;
        private Dictionary<int, SMSFieldGUI> optionsExtended;
        private Dictionary<int, int> optionsLookup;
        internal SMSFieldOptionsStringGUI(string name, ESMSFields type, SMissionStep step, string[] options,
            Dictionary<int, KeyValuePair<string, ESMSFields>> ext = null) : base(name, type)
        {
            if (options == null)
                throw new NullReferenceException("SMSFieldOptionsStringGUI expects the options parameter to be a valid array!");
            if (options.Length == 0)
                throw new NullReferenceException("SMSFieldOptionsStringGUI expects the options parameter to have at least one entry!"); 
            
            this.options = options;
            if (ext != null)
            {
                optionsLookup = new Dictionary<int, int>();
                int stepC = 0;
                foreach (var item in options)
                {
                    optionsLookup.Add(item.GetHashCode(), stepC);
                    stepC++;
                }

                optionsExtended = new Dictionary<int, SMSFieldGUI>();
                foreach (var item in ext)
                {
                    optionsExtended.Add(item.Key,
                        SMStepEditorGUI.GetField(step, item.Value.Value, "- " + item.Value.Key));
                }
            }
            else
            {
                optionsLookup = null;
                optionsExtended = null;
            }
        }
        public override void Display(SubMissionStep runData)
        {
            /*
            if (settable == null)
            {
                //throw new MandatoryException("settable is illegally null somehow");
            }*/
            GUILayout.BeginVertical();
            if (GUILayout.Button(settable == null ? "" : settable))
            {
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Button);
                opened = !opened;
            }
            if (opened)
            {
                scroller = GUILayout.BeginScrollView(scroller, AltUI.TextfieldBlackHuge, GUILayout.Height(420));
                for (int step = 0; step < options.Length; step++)
                {
                    if (GUILayout.Button(options[step], WindowManager.styleBorderedFont))
                    {
                        settable = options[step];
                        opened = false;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
                    }
                }
                GUILayout.EndScrollView();
            }
            else if (optionsLookup != null && settable != null && 
                optionsLookup.TryGetValue(settable.GetHashCode(), out int index) &&
                optionsExtended.TryGetValue(index, out SMSFieldGUI GUIext))
            {
                GUILayout.BeginHorizontal();
                GUIext.DoDisplay(runData);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
    }

    // -----------------------------------------------------------------------------------------

    public class SMSFieldVarInGUI : SMSFieldGUI<int>
    {
        protected string setCache = null;
        internal SMSFieldVarInGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            DisplayValues(runData, runData.VaribleType);
        }
        protected void DisplayValues(SubMissionStep runData, EVaribleType VaribleType)
        {
            switch (VaribleType)
            {
                case EVaribleType.None:
                    GUILayout.Label("Disabled");
                    break;
                case EVaribleType.True:
                    DisplayBool(runData, false);
                    break;
                case EVaribleType.False:
                    DisplayBool(runData, true);
                    break;
                case EVaribleType.Int:
                    DisplayInt(runData);
                    break;
                case EVaribleType.IntGreaterThan:
                    if (settable >= 0 && settable < runData.Mission.VarIntsActive.Count)
                    {
                        DisplayInt(runData);
                        if (runData.Mission.VarIntsActive[settable] > runData.VaribleCheckNum)
                            GUILayout.Label("<color=green>></color>", GUILayout.Width(25));
                        else
                            GUILayout.Label("<color=red>></color>", GUILayout.Width(25));
                        GUILayout.Label(runData.VaribleCheckNum.ToString("F"));
                    }
                    else
                        GUILayout.Label("<color=yellow>True - Disabled</color>");
                    break;
                case EVaribleType.IntLessThan:
                    if (settable >= 0 && settable < runData.Mission.VarIntsActive.Count)
                    {
                        DisplayInt(runData);
                        if (runData.Mission.VarIntsActive[settable] < runData.VaribleCheckNum)
                            GUILayout.Label("<color=green><</color>", GUILayout.Width(25));
                        else
                            GUILayout.Label("<color=red><</color>", GUILayout.Width(25));
                        GUILayout.Label(runData.VaribleCheckNum.ToString("F"));
                    }
                    else
                        GUILayout.Label("<color=yellow>True - Disabled</color>");
                    break;
                case EVaribleType.DoSuccessID:
                    GUILayout.Label("Change Progress ID to: ");
                    GUILayout.Label(runData.SuccessProgressID.ToString());
                    if (DisplayInt(runData.SuccessProgressID, ref setCache, out int newSet))
                        runData.SuccessProgressID = newSet;
                    break;
                default:
                    break;
            }
        }
        protected void DisplayBool(SubMissionStep runData, bool invertOutput)
        {
            if (settable > -1 && settable < runData.Mission.VarTrueFalseActive.Count)
                GUILayout.Label(invertOutput ? (!runData.Mission.VarTrueFalseActive[settable]).ToString() :
                    runData.Mission.VarTrueFalseActive[settable].ToString());
            else
                GUILayout.Label("N/A");


            if (setCache == null)
                setCache = settable.ToString();
            if (int.TryParse(setCache, out int val) && val != settable)
                setCache = settable.ToString();
            string set = GUILayout.TextField(setCache, 32, AltUI.TextfieldBlackAdjusted, GUILayout.Width(180));
            if (long.TryParse(set, out long val2))
            {
                if (val < 0)
                {
                    GUILayout.Button("<color=yellow>~</color>");
                    if (Event.current.type == EventType.Repaint &&
                        GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                        AltUI.TooltipWorld("Variable bool disabled", false);
                    if (set != setCache)
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                        settable = ClampInt(val2);
                    }
                }
                else if (settable >= runData.Mission.VarTrueFalseActive.Count)
                {
                    GUILayout.Button("–");
                    if (Event.current.type == EventType.Repaint &&
                        GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                        AltUI.TooltipWorld("Variable bool out of range!", false);
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.RadarOn);
                    settable = runData.Mission.VarTrueFalseActive.Count;
                    setCache = runData.Mission.VarTrueFalseActive.Count.ToString();
                    runData.Mission.VarTrueFalseActive.Add(false);
                    runData.Mission.VarTrueFalse.Add(false);
                }
                else
                {
                    if (GUILayout.Button("<color=green>O</color>"))
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Close);
                        setCache = "-1";
                        settable = -1;
                    }
                    else if (set != setCache)
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                        settable = ClampInt(val2);
                    }
                    if (Event.current.type == EventType.Repaint &&
                        GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                        AltUI.TooltipWorld("Valid variable bool.  Click to unassign", false);
                }
            }
            else
            {
                if (GUILayout.Button("<color=red>X</color>"))
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Close);
                    setCache = "-1";
                    settable = -1;
                }
                else if (set != setCache)
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                    settable = val;
                }
                if (Event.current.type == EventType.Repaint &&
                    GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                    AltUI.TooltipWorld("Invalid variable bool.  Click to unassign", false);
            }
        }
        protected void DisplayInt(SubMissionStep runData)
        {
            if (settable > -1 && settable < runData.Mission.VarIntsActive.Count)
                GUILayout.Label(runData.Mission.VarIntsActive[settable].ToString());
            else
                GUILayout.Label("N/A");

            if (setCache == null)
                setCache = settable.ToString();
            if (int.TryParse(setCache, out int val) && val != settable)
                setCache = settable.ToString();
            string set = GUILayout.TextField(setCache, 32, AltUI.TextfieldBlackAdjusted, GUILayout.Width(180));
            if (long.TryParse(set, out long val2))
            {
                if (val < 0)
                {
                    GUILayout.Button("<color=yellow>~</color>");
                    if (Event.current.type == EventType.Repaint &&
                        GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                        AltUI.TooltipWorld("Variable bool disabled", false);
                    if (set != setCache)
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                        settable = ClampInt(val2);
                    }
                }
                else if (settable >= runData.Mission.VarIntsActive.Count)
                {
                    GUILayout.Button("–");
                    if (Event.current.type == EventType.Repaint &&
                        GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                        AltUI.TooltipWorld("Variable bool out of range!", false);
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.RadarOn);
                    settable = runData.Mission.VarIntsActive.Count;
                    setCache = runData.Mission.VarIntsActive.Count.ToString();
                    runData.Mission.VarIntsActive.Add(0);
                    runData.Mission.VarInts.Add(0);
                }
                else
                {
                    if (GUILayout.Button("<color=green>O</color>"))
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Close);
                        setCache = "-1";
                        settable = -1;
                    }
                    else if (set != setCache)
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                        settable = ClampInt(val2);
                    }
                    if (Event.current.type == EventType.Repaint &&
                        GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                        AltUI.TooltipWorld("Valid variable bool.  Click to unassign", false);
                }
            }
            else
            {
                if (GUILayout.Button("<color=red>X</color>"))
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Close);
                    setCache = "-1";
                    settable = -1;
                }
                else if (set != setCache)
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                    settable = val;
                }
                if (Event.current.type == EventType.Repaint &&
                    GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                    AltUI.TooltipWorld("Invalid variable bool.  Click to unassign", false);
            }
        }
    }
    public class SMSFieldVarInFixedGUI : SMSFieldVarInGUI
    {
        private readonly EVaribleType varType;
        internal SMSFieldVarInFixedGUI(string name, ESMSFields type, EVaribleType varType) : base(name, type)
        {
            this.varType = varType;
        }
        public override void Display(SubMissionStep runData)
        {
            DisplayValues(runData, varType);
        }
    }
    public class SMSFieldVarBoolGUI : SMSFieldVarInGUI
    {
        internal SMSFieldVarBoolGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            DisplayBool(runData, false);
        }
    }
    public class SMSFieldVarIntGUI : SMSFieldVarInGUI
    {
        internal SMSFieldVarIntGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            DisplayInt(runData);
        }
    }

    public class SMSFieldTechSelectGUI : SMSFieldGUI<string>
    {
        private Texture2D displayTechPicture;
        private string displayTechName = null;
        //private UIScreenTechLoader loader;
        private static SubMission LastSelected = null;
        internal SMSFieldTechSelectGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            if (displayTechName == null)
            {
                displayTechName = "<color=red>NULL</color>";
                RefreshGUI(runData);
            }
            GUILayout.Label(displayTechName);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (displayTechPicture != null)
                GUILayout.Label(displayTechPicture, AltUI.TextfieldBordered, GUILayout.Height(180), GUILayout.Width(210));
            else
                GUILayout.Label("Not Selected", AltUI.TextfieldBordered, GUILayout.Height(180), GUILayout.Width(210));
            if (GUILayout.Button("Select"))
            {
                LastSelected = ManSubMissions.Selected;
                SMTreeGUI.GUITechSelectorSet(settable);
                SMTreeGUI.JumpToTechSelector(true, () => {
                    if (SMTreeGUI.lastSelectedName != null)
                    {
                        Debug_SMissions.Log("Set tech to  " + SMTreeGUI.lastSelectedName);
                        settable = SMTreeGUI.lastSelectedName;
                        SMTreeGUI.lastSelectedName = null;
                        ManSubMissions.Selected = LastSelected;
                        RefreshGUI(runData);
                    }
                    else
                        Debug_SMissions.Log("Set tech false, no lastSelectedTechName");
                });
                ManSubMissions.Selected = null;
            }
        }

        public override void UpdateScene(SubMissionStep runData)
        {
            Color dispColor = SMMissionGUI.PositioningColor[runData.StepType];
            if (runData.Mission.GetTechPosHeading(settable, out Vector3 pos, out Vector3 direction, out int team))//SMUtil.GetTrackedTechBase(ref runData, settable, out var tracked))
            {
                DebugExtUtilities.DrawDirIndicatorSphere(pos, 4, dispColor);
                DebugExtUtilities.DrawDirIndicatorRecPriz(pos, Quaternion.LookRotation(direction, Vector3.up), 
                    new Vector3(2, 2, 3), ManPlayer.inst.PlayerTeam == team ? Color.blue :
                    Tank.IsEnemy(ManPlayer.inst.PlayerTeam, team) ? Color.red :
                    Tank.IsFriendly(ManPlayer.inst.PlayerTeam, team) ? Color.green : 
                    Color.yellow);
            }
        }
        public override void RefreshGUI(SubMissionStep runData)
        {
            if (settable != null && runData.Mission.Tree.TreeTechs.TryGetValue(settable, out var techD))
            {
                displayTechName = settable;
                displayTechPicture = techD.GetTexture(runData.Mission.Tree);
            }
            else
            {
                if (!settable.NullOrEmpty())
                    displayTechName = "<color=red>" + settable + "</color>";
                else
                    displayTechName = "<color=red>NULL</color>";
                displayTechPicture = null;
            }
        }
    }
    public class SMSFieldCorpSelectGUI : SMSFieldGUI<string>
    {
        private Texture2D displayCorpPicture;
        private string displayCorpName = null;
        //private UIScreenTechLoader loader;
        private static SubMission LastSelected = null;
        private bool expanded = false;
        internal SMSFieldCorpSelectGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            if (displayCorpName == null)
            {
                displayCorpName = "<color=red>NULL</color>";
                displayCorpName = GetFaction(settable);
            }
            GUILayout.Label(displayCorpName);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (displayCorpPicture != null)
                GUILayout.Label(displayCorpPicture, AltUI.TextfieldBordered, GUILayout.Height(80), GUILayout.Width(80));
            else
                GUILayout.Label("Not Selected", AltUI.TextfieldBordered, GUILayout.Height(80), GUILayout.Width(80));
            if (GUILayout.Button("Select"))
            {
                expanded = !expanded;
            }
            if (expanded)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(85));
                GUILayout.Space(30);
                if (GUILayout.Button("Any Corp"))
                {
                    settable = "Any";
                }
                GUILayout.EndHorizontal();
                for (FactionSubTypes i = FactionSubTypes.GSO; i <= (FactionSubTypes)Enum.GetValues(typeof(FactionSubTypes)).Length; i++)
                {
                    Sprite spr = ManUI.inst.GetCorpIcon(i);
                    if (spr?.texture != null)
                    {
                        GUILayout.BeginHorizontal(GUILayout.Height(85));
                        GUILayout.Space(30);
                        if (GUILayout.Button(spr.texture, GUILayout.Height(80), GUILayout.Width(80)))
                        {
                            settable = GetFactionName(i);
                        }
                        if (GUILayout.Button(i.ToString()))
                        {
                            settable = GetFactionName(i);
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                foreach (int j in ManMods.inst.GetCustomCorpIDs())
                {
                    FactionSubTypes i = (FactionSubTypes)j;
                    Sprite spr = ManUI.inst.GetCorpIcon(i);
                    if (spr?.texture != null)
                    {
                        GUILayout.BeginHorizontal(GUILayout.Height(85));
                        GUILayout.Space(30);
                        if (GUILayout.Button(spr.texture, GUILayout.Height(80), GUILayout.Width(80)))
                        {
                            settable = GetFactionName(i);
                        }
                        if (GUILayout.Button(i.ToString()))
                        {
                            settable = GetFactionName(i);
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }
        private string GetFactionName(FactionSubTypes input)
        {
            if (ManMods.inst.IsModdedCorp(input))
            {
                return ManMods.inst.FindCorpShortName(input);
            }
            else
                return input.ToString();
        }
        private string GetFaction(string input)
        {
            if (Enum.TryParse(input, out FactionSubTypes result))
            {
                return result.ToString();
            }
            else
            {
                ModdedCorpDefinition MCD = ManMods.inst.FindCorp(input);
                if (MCD != null)
                    return MCD.m_ShortName;
            }
            return "Any";
        }
    }
    public class SMSFieldMonumentSelectGUI : SMSFieldGUI<string>
    {
        private Texture2D displayPicture;
        private string displayName = null;
        //private UIScreenTechLoader loader;
        private static SubMission LastSelected = null;
        internal SMSFieldMonumentSelectGUI(string name, ESMSFields type) : base(name, type) { }
        public override void Display(SubMissionStep runData)
        {
            GUILayout.Label(displayName == null ? "<color=red>NULL</color>" : displayName);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (displayPicture != null)
                GUILayout.Label(displayPicture, AltUI.TextfieldBordered, GUILayout.Height(180), GUILayout.Width(210));
            else
                GUILayout.Label("Not Selected", AltUI.TextfieldBordered, GUILayout.Height(180), GUILayout.Width(210));
            if (GUILayout.Button("Select"))
            {
                LastSelected = ManSubMissions.Selected;
                SMTreeGUI.GUIMMSelectorSet(settable);
                SMTreeGUI.JumpToMMSelector(() => {
                    if (SMTreeGUI.lastSelectedName != null)
                    {
                        Debug_SMissions.Log("Set Piece to  " + SMTreeGUI.lastSelectedName);
                        settable = SMTreeGUI.lastSelectedName;
                        SMTreeGUI.lastSelectedName = null;
                        ManSubMissions.Selected = LastSelected;
                        RefreshGUI(runData);
                    }
                    else
                        Debug_SMissions.Log("Set Piece false, no lastSelectedName");
                });
                ManSubMissions.Selected = null;
            }
        }

        public override void UpdateScene(SubMissionStep runData)
        {
            Color dispColor = SMMissionGUI.PositioningColor[runData.StepType];
            if (runData.Mission.GetPiecePosHeading(settable, out Vector3 pos, out Vector3 direction))
            {
                DebugExtUtilities.DrawDirIndicatorSphere(pos, 5, dispColor);
                DebugExtUtilities.DrawDirIndicatorRecPriz(pos, Quaternion.LookRotation(direction, Vector3.up),
                    new Vector3(4, 4, 6), Color.magenta);
            }
        }
        public override void RefreshGUI(SubMissionStep runData)
        {
            if (settable != null && runData.Mission.Tree.WorldObjects.TryGetValue(settable, out var pieceD))
            {
                displayName = settable;
                SMTreeGUI.GetTexture(pieceD, (Texture2D delegater) => { displayPicture = delegater; });
            }
            else
            {
                if (!settable.NullOrEmpty())
                    displayName = "<color=red>" + settable + "</color>";
                else
                    displayName = "<color=red>NULL</color>";
                displayPicture = null;
            }
        }
    }
}
