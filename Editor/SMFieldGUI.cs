using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
//using Sub_Missions.Steps;
using Sub_Missions.ManWindows;
using TerraTechETCUtil;

namespace Sub_Missions.Editor
{
    public static class SMFieldGUIExt
    {
        internal static List<SMFieldGUI> SetupFields()
        {
            List<SMFieldGUI> batcher = new List<SMFieldGUI>()
            {
                SetupField(ESMFields.AltNames, "Alternate Names"),
                SetupField(ESMFields.Faction, "Faction"),
                SetupField(ESMFields.GradeRequired, "Minimum Grade"),
                SetupField(ESMFields.Description, "Default Description"),
                SetupField(ESMFields.AltDescs, "Alternate Descriptions"),
                SetupField(ESMFields.MinProgressX, "DynamicX"),
                SetupField(ESMFields.MinProgressY, "DynamicY"),
                SetupField(ESMFields.SinglePlayerOnly, "Only Single-Player"),
                SetupField(ESMFields.SpawnPosition, "Spawn Position"),
                SetupField(ESMFields.IgnorePlayerProximity, "Active Anywhere"),
                SetupField(ESMFields.ClearSceneryOnSpawn, "Remove Scenery on Start"),
                SetupField(ESMFields.ClearTechsOnClear, "Remove Techs on End"),
                SetupField(ESMFields.ClearModularMonumentsOnClear, "Remove Monuments on End"),
                SetupField(ESMFields.CannotCancel, "Disable \"Cancel Mission\""),
                SetupField(ESMFields.UpdateSpeedMultiplier, "Update Speed Multiplier"),
            };
            return batcher;
        }
        /// <summary>
        /// DO NOT RELY ON CALLING THIS AS THIS CREATES NEW ITEMS - CLEAN OUT
        /// </summary>
        /// <param name="step"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static SMFieldGUI SetupField(ESMFields type, string name)
        {
            switch (type)
            {
                case ESMFields.AltNames:
                    return new SMFieldStringMultiGUI(name, type);
                case ESMFields.Faction:
                    return new SMFieldOptionsStringGUI(name, type, ManSMCCorps.AllCorpNames);
                case ESMFields.GradeRequired:
                    return new SMFieldIntGUI(name, type);
                case ESMFields.Description:
                    return new SMFieldStringLargeGUI(name, type);
                case ESMFields.AltDescs:
                    return new SMFieldStringLargeMultiGUI(name, type);
                case ESMFields.MinProgressX:
                    return new SMFieldByteXGUI(name, type);
                case ESMFields.MinProgressY:
                    return new SMFieldByteYGUI(name, type);
                case ESMFields.SinglePlayerOnly:
                    return new SMFieldBoolGUI(name, type);
                case ESMFields.SpawnPosition:
                    return new SMFieldOptionsStringGUI(name, type, 
                        Enum.GetNames(typeof(SubMissionPosition)).ToList());
                case ESMFields.IgnorePlayerProximity:
                    return new SMFieldBoolGUI(name, type);
                case ESMFields.ClearTechsOnClear:
                    return new SMFieldBoolGUI(name, type);
                case ESMFields.ClearModularMonumentsOnClear:
                    return new SMFieldBoolGUI(name, type);
                case ESMFields.ClearSceneryOnSpawn:
                    return new SMFieldBoolGUI(name, type);
                case ESMFields.CannotCancel:
                    return new SMFieldBoolGUI(name, type);
                case ESMFields.UpdateSpeedMultiplier:
                    return new SMFieldFloatGUI(name, type);
            }
            throw new InvalidOperationException("SMFieldGUI.SetupField was called with ESMSFields of type [" +
                type.ToString() + "]. This is unsupported!");
        }
    }
    public interface SMFieldGUI
    {
        string name { get; }
        ESMFields type { get; }
        SubMission context { get; }

        void Display(SubMission runData);
        void DoDisplay(SubMission runData);
    }

    public class SMFieldGUI<T> : SMAutoFill<SubMission, T, ESMFields>, SMFieldGUI
    {
        protected SMFieldGUI(string name, ESMFields type) : base(name, type) { }
        public override void Display(SubMission runData)
        {
            throw new NotImplementedException("SMFieldGUI<T> is derived class, not a standalone");
        }

        protected override object FuncLookup(SubMission runData)
        {
            switch (type)
            {
                case ESMFields.AltNames:
                    return runData.AltNames;
                case ESMFields.Faction:
                    return runData.Faction;
                case ESMFields.GradeRequired:
                    return runData.GradeRequired;
                case ESMFields.Description:
                    return runData.Description;
                case ESMFields.AltDescs:
                    return runData.AltDescs;
                case ESMFields.MinProgressX:
                    return runData.MinProgressX;
                case ESMFields.MinProgressY:
                    return runData.MinProgressY;
                case ESMFields.SinglePlayerOnly:
                    return runData.SinglePlayerOnly;
                case ESMFields.SpawnPosition:
                    return runData.SpawnPosition.ToString();
                case ESMFields.IgnorePlayerProximity:
                    return runData.IgnorePlayerProximity;
                case ESMFields.ClearTechsOnClear:
                    return runData.ClearTechsOnClear;
                case ESMFields.ClearModularMonumentsOnClear:
                    return runData.ClearModularMonumentsOnClear;
                case ESMFields.ClearSceneryOnSpawn:
                    return runData.ClearSceneryOnSpawn;
                case ESMFields.CannotCancel:
                    return runData.CannotCancel;
                case ESMFields.UpdateSpeedMultiplier:
                    return runData.UpdateSpeedMultiplier;
            }
            throw new IndexOutOfRangeException("SMFieldGUI.settable_set called on an invalid instance of type " + type.ToString());
        }
        protected override void FuncSave(SubMission runData, object input)
        {
            try
            {
                switch (type)
                {
                    case ESMFields.AltNames:
                        throw new UnauthorizedAccessException("SMFieldGUI.set_settable[" + type.ToString() + 
                            "] does not support replacement of list fields");
                    case ESMFields.Faction:
                        runData.Faction = (string)input;
                        return;
                    case ESMFields.GradeRequired:
                        runData.GradeRequired = (int)input;
                        return;
                    case ESMFields.Description:
                        runData.Description = (string)input;
                        return;
                    case ESMFields.AltDescs:
                        throw new UnauthorizedAccessException("SMFieldGUI.set_settable[" + type.ToString() + 
                            "] does not support replacement of list fields");
                    case ESMFields.MinProgressX:
                        runData.MinProgressX = (byte)input;
                        return;
                    case ESMFields.MinProgressY:
                        runData.MinProgressY = (byte)input;
                        return;
                    case ESMFields.SinglePlayerOnly:
                        runData.SinglePlayerOnly = (bool)input;
                        return;
                    case ESMFields.SpawnPosition:
                        if (Enum.TryParse((string)input, true, out SubMissionPosition resu))
                            runData.SpawnPosition = resu;
                        else
                            runData.SpawnPosition = SubMissionPosition.FarFromPlayer;
                        return;
                    case ESMFields.IgnorePlayerProximity:
                        runData.IgnorePlayerProximity = (bool)input;
                        return;
                    case ESMFields.ClearTechsOnClear:
                        runData.ClearTechsOnClear = (bool)input;
                        return;
                    case ESMFields.ClearModularMonumentsOnClear:
                        runData.ClearModularMonumentsOnClear = (bool)input;
                        return;
                    case ESMFields.ClearSceneryOnSpawn:
                        runData.ClearSceneryOnSpawn = (bool)input;
                        return;
                    case ESMFields.CannotCancel:
                        runData.CannotCancel = (bool)input;
                        return;
                    case ESMFields.UpdateSpeedMultiplier:
                        runData.UpdateSpeedMultiplier = (float)input;
                        return;
                }
                throw new IndexOutOfRangeException("SMFieldGUI.set_settable called on an invalid instance of type " + type.ToString());
            }
            catch (InvalidCastException e)
            {
                throw new InvalidCastException("Invalid cast attempt from " + typeof(T).Name + " to parameter " + type.ToString(), e);
            }
        }

    }


    // ----------------------------------------------------------------------------- 

    public class SMFieldNullGUI : SMFieldGUI<object>
    {
        internal SMFieldNullGUI(string name, ESMFields type) : base(name, type) { }
        public override void Display(SubMission runData) { }
    }
    public class SMFieldWIPGUI : SMFieldGUI<object>
    {
        internal SMFieldWIPGUI(string name, ESMFields type) : base(name, type) { }
        public override void Display(SubMission runData)
        {
            GUILayout.Label("This is WIP!");
        }
    }

    public class SMFieldBoolGUI : SMFieldGUI<bool>
    {
        internal SMFieldBoolGUI(string name, ESMFields type) : base(name, type) { }
        public override void Display(SubMission runData)
        {
            if (DisplayBoolean(settable))
                settable = !settable;
        }
    }
    public class SMFieldByteXGUI : SMFieldGUI<byte>
    {
        private string setCache = "";
        internal SMFieldByteXGUI(string name, ESMFields type) : base(name, type) { }
        public override void Display(SubMission runData)
        {
            if (DisplayByte(settable, ref setCache, out var newSet))
                settable = newSet;
        }
        public override void DoDisplay(SubMission runData)
        {
            GUILayout.Label(runData.Tree.ProgressXName);
            GUILayout.FlexibleSpace();
            context = runData;
            data = (byte)FuncLookup(context);
            Display(runData);
        }
    }
    public class SMFieldByteYGUI : SMFieldGUI<byte>
    {
        private string setCache = "";
        internal SMFieldByteYGUI(string name, ESMFields type) : base(name, type) { }
        public override void Display(SubMission runData)
        {
            if (DisplayByte(settable, ref setCache, out var newSet))
                settable = newSet;
        }
        public override void DoDisplay(SubMission runData)
        {
            GUILayout.Label(runData.Tree.ProgressYName);
            GUILayout.FlexibleSpace();
            context = runData;
            data = (byte)FuncLookup(context);
            Display(runData);
        }
    }
    public class SMFieldIntGUI : SMFieldGUI<int>
    {
        private string setCache = "";
        internal SMFieldIntGUI(string name, ESMFields type) : base(name, type) { }
        public override void Display(SubMission runData)
        {
            if (DisplayInt(settable, ref setCache, out int newSet))
                settable = newSet;
        }
    }
    public class SMFieldFloatGUI : SMFieldGUI<float>
    {
        private string setCache = "";
        internal SMFieldFloatGUI(string name, ESMFields type) : base(name, type) { }
        public override void Display(SubMission runData)
        {
            if (DisplayFloat(settable, ref setCache, out var newSet))
                settable = newSet;
        }
    }
    public class SMFieldFloatIntGUI : SMFieldGUI<float>
    {
        private string setCache = "";
        internal SMFieldFloatIntGUI(string name, ESMFields type) : base(name, type) { }
        public override void Display(SubMission runData)
        {
            if (DisplayFloatRounded(settable, ref setCache, out var newSet))
                settable = newSet;
        }
    }
    public class SMFieldVector3GUI : SMFieldGUI<Vector3>
    {
        protected string setCache1 = "";
        protected string setCache2 = "";
        protected string setCache3 = "";
        internal SMFieldVector3GUI(string name, ESMFields type) : base(name, type) { }
        public override void Display(SubMission runData)
        {
            if (DisplayVec3(settable, ref setCache1, ref setCache2, ref setCache3, out Vector3 newSet))
                settable = newSet;
        }
    }
    public class SMFieldStringGUI : SMFieldGUI<string>
    {
        internal SMFieldStringGUI(string name, ESMFields type) : base(name, type) { }
        public override void Display(SubMission runData)
        {
            if (DisplayString(settable, out var newSet))
                settable = newSet;
        }
    }
    public class SMFieldStringMultiGUI : SMFieldGUI<List<string>>
    {
        private bool opened = false;
        internal SMFieldStringMultiGUI(string name, ESMFields type) : base(name, type) { }
        public override void Display(SubMission runData)
        {
            if (opened)
            {
                if (GUILayout.Button("Retract", AltUI.ButtonBlueActive, GUILayout.Width(80)))
                    opened = false;
            }
            else if (GUILayout.Button("Expand", AltUI.ButtonBlue, GUILayout.Width(80)))
                opened = true;
            GUILayout.Label(settable.Count.ToString(), AltUI.TextfieldBordered, GUILayout.Width(32));
            if (opened)
            {
                for (int step = 0; step < settable.Count; step++)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(GUILayout.Height(32));
                    GUILayout.Label(step.ToString(), AltUI.TextfieldBorderedBlue, GUILayout.Width(32));
                    GUILayout.FlexibleSpace();
                    if (settable[step] == null)
                        settable[step] = "";
                    if (DisplayString(settable[step], out var newSet))
                        settable[step] = newSet;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal(GUILayout.Height(32));
                if (GUILayout.Button("Add", AltUI.ButtonGreen))
                    settable.Add("Unset");
                else if (GUILayout.Button("Remove", AltUI.ButtonRed))
                    settable.RemoveAt(settable.Count - 1);
            }
        }
    }
    public class SMFieldStringLargeGUI : SMFieldGUI<string>
    {
        internal SMFieldStringLargeGUI(string name, ESMFields type) : base(name, type) { }
        public override void Display(SubMission runData)
        {
            if (DisplayStringArea(settable, out var newSet, SMMissionGUI.RightDisplaySize))
                settable = newSet;
        }
    }
    public class SMFieldStringLargeMultiGUI : SMFieldGUI<List<string>>
    {
        private bool opened = false;
        internal SMFieldStringLargeMultiGUI(string name, ESMFields type) : base(name, type) { }
        public override void Display(SubMission runData)
        {
            if (opened)
            {
                if (GUILayout.Button("Retract", AltUI.ButtonBlueActive, GUILayout.Width(80)))
                    opened = false;
            }
            else if (GUILayout.Button("Expand", AltUI.ButtonBlue, GUILayout.Width(80)))
                opened = true;
            GUILayout.Label(settable.Count.ToString(), AltUI.TextfieldBordered, GUILayout.Width(32));
            if (opened)
            {
                for (int step = 0; step < settable.Count; step++)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(GUILayout.Height(32));
                    GUILayout.Label(step.ToString(), AltUI.TextfieldBorderedBlue, GUILayout.Width(32));
                    GUILayout.FlexibleSpace();
                    if (settable[step] == null)
                        settable[step] = "";
                    if (DisplayStringArea(settable[step], out var newSet, SMMissionGUI.RightDisplaySize))
                        settable[step] = newSet;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal(GUILayout.Height(32));
                if (GUILayout.Button("Add", AltUI.ButtonGreen))
                    settable.Add("Unset");
                else if (GUILayout.Button("Remove", AltUI.ButtonRed))
                    settable.RemoveAt(settable.Count - 1);
            }
        }
    }

    // ADVANCED
    public class SMFieldOptionsStringGUI : SMFieldGUI<string>
    {
        private bool opened = false;
        private Vector2 scroller = Vector2.zero;
        private List<string> options;
        internal SMFieldOptionsStringGUI(string name, ESMFields type, List<string> options) : base(name, type)
        {
            this.options = options;
        }
        public override void Display(SubMission runData)
        {
            GUILayout.BeginVertical();
            if (GUILayout.Button(settable == null ? "" : settable))
            {
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Button);
                opened = !opened;
            }
            if (opened)
            {
                scroller = GUILayout.BeginScrollView(scroller, AltUI.TextfieldBlackHuge, GUILayout.Height(420));
                for (int step = 0; step < options.Count; step++)
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
            GUILayout.EndVertical();
        }
    }
}
