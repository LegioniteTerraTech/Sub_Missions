using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TerraTechETCUtil;
using Sub_Missions.ManWindows;
using Sub_Missions.Steps;
using Newtonsoft.Json.Linq;

namespace Sub_Missions.Editor
{
    public static class SMAutoFill
    {
        public static byte ClampByte(long input)
        {
            if (input > byte.MaxValue)
                return byte.MaxValue;
            else if (input < byte.MinValue)
                return byte.MinValue;
            return (byte)input;
        }
        public static sbyte ClampSbyte(long input)
        {
            if (input > sbyte.MaxValue)
                return sbyte.MaxValue;
            else if (input < sbyte.MinValue)
                return sbyte.MinValue;
            return (sbyte)input;
        }
        public static int ClampInt(long input)
        {
            if (input > int.MaxValue)
                return int.MaxValue;
            else if (input < int.MinValue)
                return int.MinValue;
            return (int)input;
        }


        public static void Toggle(string name, ref bool boolSet)
        {
            if (boolSet)
            {
                if (GUILayout.Button(name, AltUI.ButtonOrangeLargeActive, GUILayout.Height(48)))
                    boolSet = false;
            }
            else if (GUILayout.Button(name, AltUI.ButtonOrangeLarge, GUILayout.Height(48)))
            {
                boolSet = true;
            }
        }
        public static void OneWayButtonLarge(string name, ref bool boolSet)
        {
            if (boolSet)
                GUILayout.Button(name, AltUI.ButtonOrangeLargeActive, GUILayout.Height(48));
            else if (GUILayout.Button(name, AltUI.ButtonOrangeLarge, GUILayout.Height(48)))
                boolSet = true;
        }
        public static void OneWayButtonLargeInv(string name, ref bool boolSet)
        {
            if (!boolSet)
                GUILayout.Button(name, AltUI.ButtonOrangeLargeActive, GUILayout.Height(48));
            else if (GUILayout.Button(name, AltUI.ButtonOrangeLarge, GUILayout.Height(48)))
                boolSet = false;
        }
        public static void OneWayButton(string name, ref bool boolSet)
        {
            if (boolSet)
                GUILayout.Button(name, AltUI.ButtonBlueActive, GUILayout.Height(48));
            else if (GUILayout.Button(name, AltUI.ButtonBlue, GUILayout.Height(48)))
                boolSet = true;
        }
        public static bool OneWayButton(string name, int valueToSet, ref int boolSet)
        {
            if (valueToSet == boolSet)
                GUILayout.Button(name, AltUI.ButtonBlueActive, GUILayout.Height(48));
            else if (GUILayout.Button(name, AltUI.ButtonBlue, GUILayout.Height(48)))
            {
                boolSet = valueToSet;
                return true;
            }
            return false;
        }
        public static void OneWayButton<T>(string name, T valueToSet, ref T boolSet) where T : Enum
        {
            if (boolSet.CompareTo(valueToSet) == 0)
                GUILayout.Button(name, AltUI.ButtonBlueActive, GUILayout.Height(48));
            else if (GUILayout.Button(name, AltUI.ButtonBlue, GUILayout.Height(48)))
                boolSet = valueToSet;
        }

        public static void AutoFixedOptions(string name, ref int settable, ref Vector2 scroller, ref bool opened,
            string[] options, int[] lookup = null, Dictionary<int, int> lookupInv = null)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(32));
            GUILayout.Label(name);
            GUILayout.FlexibleSpace();
            if (FixedOptions(settable, ref scroller, ref opened, options, out var newSettable, lookup, lookupInv))
                settable = newSettable;
            GUILayout.EndHorizontal();
        }
        public static int AutoFixedOptions(string name, int settable, ref Vector2 scroller, ref bool opened,
            string[] options, int[] lookup = null, Dictionary<int, int> lookupInv = null)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(32));
            GUILayout.Label(name);
            GUILayout.FlexibleSpace();
            if (FixedOptions(settable, ref scroller, ref opened, options, out var newSettable, lookup, lookupInv))
                settable = newSettable;
            GUILayout.EndHorizontal();
            return settable;
        }
        public static void AutoFixedOptions(string name, ref string settable, ref Vector2 scroller, ref bool opened,
            string[] options)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(32));
            GUILayout.Label(name);
            GUILayout.FlexibleSpace();
            if (FixedOptions(settable, ref scroller, ref opened, options, out var newSettable))
                settable = newSettable;
            GUILayout.EndHorizontal();
        }
        public static void AutoFixedOptions(string name, ref string settable, ref Vector2 scroller, ref bool opened,
            List<string> options)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(32));
            GUILayout.Label(name);
            GUILayout.FlexibleSpace();
            if (FixedOptions(settable, ref scroller, ref opened, options, out var newSettable))
                settable = newSettable;
            GUILayout.EndHorizontal();
        }
        public static bool FixedOptions(int settable, ref Vector2 scroller, ref bool opened,
            string[] options, out int newSettable, int[] lookup = null, Dictionary<int, int> lookupInv = null)
        {
            bool setted = false;
            GUILayout.BeginVertical();
            int num = settable;
            try
            {
                if (lookup != null)
                    num = lookupInv[num];
            }
            catch (Exception)
            {
                num = lookupInv.FirstOrDefault().Value;
            }
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
                        if (lookup != null)
                            settable = lookup[step];
                        else
                            settable = step;
                        opened = false;
                        setted = true;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
            newSettable = settable;
            return setted;
        }
        public static bool FixedOptions(string settable, ref Vector2 scroller, ref bool opened,
            string[] options, out string newSettable)
        {
            bool setted = false;
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
                        setted = true;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
            newSettable = settable;
            return setted;
        }
        public static bool FixedOptions(string settable, ref Vector2 scroller, ref bool opened,
            List<string> options, out string newSettable)
        {
            bool setted = false;
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
                        setted = true;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
            newSettable = settable;
            return setted;
        }


        public static bool AutoTextField(string name, ref string field, float height = 32)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(height));
            GUILayout.Label(name);
            GUILayout.FlexibleSpace();
            var fieldE = GUILayout.TextField(field, GUILayout.Width(300));
            GUILayout.EndHorizontal();
            if (fieldE != field)
            {
                field = fieldE;
                return true;
            }
            else
                return false;
        }
        public static void AutoTextField(string name, ref string setCache, ref int settable, float height = 32)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(height));
            GUILayout.Label(name);
            GUILayout.FlexibleSpace();
            if (!int.TryParse(setCache, out int val) || val != settable)
                setCache = settable.ToString();
            string set = GUILayout.TextField(setCache, 32, AltUI.TextfieldBlackAdjusted, GUILayout.Width(180));
            if (long.TryParse(set, out long val2))
            {
                GUILayout.Label("<color=green>O</color>", AltUI.TextfieldBordered, GUILayout.Width(height), GUILayout.Height(height));
                if (set != setCache)
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                    settable = ClampInt(val2);
                }
            }
            else
            {
                GUILayout.Label("<color=red>X</color>", AltUI.TextfieldBordered, GUILayout.Width(height), GUILayout.Height(height));
            }
            GUILayout.EndHorizontal();
            setCache = set;
        }
        public static void AutoTextField(string name, ref string setCache, ref sbyte settable, float height = 32)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(height));
            GUILayout.Label(name);
            GUILayout.FlexibleSpace();
            if (!int.TryParse(setCache, out int val) || val != settable)
                setCache = settable.ToString();
            string set = GUILayout.TextField(setCache, 32, AltUI.TextfieldBlackAdjusted, GUILayout.Width(180));
            if (long.TryParse(set, out long val2))
            {
                GUILayout.Label("<color=green>O</color>", AltUI.TextfieldBordered, GUILayout.Width(height), GUILayout.Height(height));
                if (set != setCache)
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                    settable = ClampSbyte(val2);
                }
            }
            else
            {
                GUILayout.Label("<color=red>X</color>", AltUI.TextfieldBordered, GUILayout.Width(height), GUILayout.Height(height));
            }
            GUILayout.EndHorizontal();
            setCache = set;
        }

        public static void AutoTextFields(string name, ref bool opened, List<string> settable, float height = 32)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(height));
            GUILayout.Label(name);
            GUILayout.FlexibleSpace();
            if (opened)
            {
                if (GUILayout.Button("Retract", AltUI.ButtonBlueActive, GUILayout.Width(80)))
                    opened = false;
            }
            else if (GUILayout.Button("Expand", AltUI.ButtonBlue, GUILayout.Width(80)))
                opened = true;
            GUILayout.Label(settable.Count.ToString(), AltUI.TextfieldBordered, GUILayout.Width(32));
            GUILayout.EndHorizontal();
            if (opened)
            {
                for (int step = 0; step < settable.Count; step++)
                {
                    GUILayout.Label(step.ToString(), AltUI.TextfieldBorderedBlue, GUILayout.Width(32));
                    GUILayout.FlexibleSpace();
                    string fielder = settable[step];
                    if (AutoTextField(settable[step], ref fielder, height))
                        settable[step] = fielder;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal(GUILayout.Height(height));
                if (GUILayout.Button("Add", AltUI.ButtonGreen))
                    settable.Add("Unset");
                else if (GUILayout.Button("Remove", AltUI.ButtonRed))
                    settable.RemoveAt(settable.Count - 1);
            }
        }

        public static void AutoBoolField(string name, bool invertOutput, SubMission Mission, ref string setCache, 
            ref int settable, float height = 32)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(height));
            GUILayout.Label(name);
            GUILayout.FlexibleSpace();
            if (settable < 0)
                GUILayout.Label("Disabled");
            else if (settable < Mission.VarTrueFalseActive.Count)
                GUILayout.Label(invertOutput ? (!Mission.VarTrueFalseActive[settable]).ToString() :
                    Mission.VarTrueFalseActive[settable].ToString());
            else
                GUILayout.Label("N/A");

            if (int.TryParse(setCache, out int val) && val != settable)
                setCache = settable.ToString();
            string set = GUILayout.TextField(setCache, 32, AltUI.TextfieldBlackAdjusted, GUILayout.Width(180));
            if (long.TryParse(set, out long val2))
            {
                if (val < 0)
                {
                    GUILayout.Button("~", AltUI.ButtonGrey, GUILayout.Width(height), GUILayout.Height(height));
                    if (set != setCache)
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                        settable = ClampInt(val2);
                    }
                }
                else if (settable >= Mission.VarTrueFalseActive.Count)
                {
                    GUILayout.Button("–", AltUI.TextfieldBordered, GUILayout.Width(height), GUILayout.Height(height));
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.RadarOn);
                    settable = Mission.VarTrueFalseActive.Count;
                    setCache = Mission.VarTrueFalseActive.Count.ToString();
                    Mission.VarTrueFalseActive.Add(false);
                }
                else
                {
                    if (GUILayout.Button("O", AltUI.ButtonGreen, GUILayout.Width(height), GUILayout.Height(height)))
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
                }
            }
            else
            {
                if (GUILayout.Button("X", AltUI.ButtonRed, GUILayout.Width(height), GUILayout.Height(height)))
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
            }
            GUILayout.EndHorizontal();
        }
        public static void AutoIntField(string name, SubMission Mission, ref string setCache,
            ref int settable, float height = 32)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(height));
            GUILayout.Label(name);
            GUILayout.FlexibleSpace();
            if (settable < 0)
                GUILayout.Label("Disabled");
            else if (settable < Mission.VarIntsActive.Count)
                GUILayout.Label(Mission.VarIntsActive[settable].ToString());
            else
                GUILayout.Label("N/A");

            if (int.TryParse(setCache, out int val) && val != settable)
                setCache = settable.ToString();
            string set = GUILayout.TextField(setCache, 32, AltUI.TextfieldBlackAdjusted, GUILayout.Width(180));
            if (long.TryParse(set, out long val2))
            {
                if (val < 0)
                {
                    GUILayout.Button("~", AltUI.ButtonGrey, GUILayout.Width(height), GUILayout.Height(height));
                    if (set != setCache)
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                        settable = ClampInt(val2);
                    }
                }
                else if (settable >= Mission.VarIntsActive.Count)
                {
                    GUILayout.Button("–", AltUI.TextfieldBordered, GUILayout.Width(height), GUILayout.Height(height));
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.RadarOn);
                    settable = Mission.VarIntsActive.Count;
                    setCache = Mission.VarIntsActive.Count.ToString();
                    Mission.VarIntsActive.Add(0);
                }
                else
                {
                    if (GUILayout.Button("O", AltUI.ButtonGreen, GUILayout.Width(height), GUILayout.Height(height)))
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
                }
            }
            else
            {
                if (GUILayout.Button("X", AltUI.ButtonRed, GUILayout.Width(height), GUILayout.Height(height)))
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
            }
            GUILayout.EndHorizontal();
        }
    }
    /// <summary>
    /// Creates an auto-setter for general use! Yay
    /// </summary>
    /// <typeparam name="C">The class type to get/set</typeparam>
    /// <typeparam name="F">Field type to get/set</typeparam>
    /// <typeparam name="T">Enum that lists the classes to edit</typeparam>
    public abstract class SMAutoFill<C, F, T> where C : class where T : Enum
    {
        public string name => _name;
        private readonly string _name;
        public C context {
            get => _context;
            set => _context = value;
        }
        protected C _context;

        public T type => _type;
        private readonly T _type;

        protected F data;
        protected F settable
        {
            get => data;
            set => FuncSave(_context, value);
        }

        protected SMAutoFill(string name, T type)
        {
            _name = name;
            _type = type;
        }
        /// <summary>
        /// DO NOT CALL EXTERNALLY!
        /// <list type="bullet">  Place the UnityEngine.GUILayout setup in here for GUI displaying. </list>
        /// </summary>
        /// <param name="runData">The class instance to manage</param>
        public abstract void Display(C runData);
        /*{
            GUILayout.Label("SMAutoFill<" + typeof(C).ToString() + "," + typeof(F).ToString() + "," + 
                typeof(T).ToString() + ">.Display(" + typeof(C).ToString() + " runData) was incorrectly set up!" +
                " \nDisplay(" + typeof(C).ToString() + " runData) must be overrriden with the display GUILayout functions!");
        }*/
        public virtual void RefreshGUI(SubMissionStep runData) { }
        /// <summary>
        /// Call this to show the function and it's respective fields
        /// </summary>
        /// <param name="runData">The class instance to manage</param>
        public virtual void DoDisplay(C runData)
        {
            GUILayout.Label(name);
            GUILayout.FlexibleSpace();
            context = runData;
            data = (F)FuncLookup(context);
            Display(runData);
        }
        public virtual void UpdateScene(C runData) { }

        /// <summary>
        /// "Get" operation for field <typeparamref name="F"/>.  
        /// <list type="bullet">  This should contain a switch with the enums that point to instance fields. </list>
        /// </summary>
        /// <param name="runData">The class instance to manage</param>
        /// <returns>The <typeparamref name="F"/> field extracted from the <typeparamref name="C"/> instance</returns>
        protected abstract object FuncLookup(C runData);
        /// <summary>
        /// "Set" operation for field <typeparamref name="F"/>.  
        /// <list type="bullet">  This should contain a switch with the enums that point to instance fields. </list>
        /// </summary>
        /// <param name="runData">The class instance to manage</param>
        /// <param name="input">The <typeparamref name="F"/> field instance goes in here</param>
        protected abstract void FuncSave(C runData, object input);

        // Helper GUI Functions
        protected Func<long, int> ClampInt => SMAutoFill.ClampInt;
        protected Func<long, byte> ClampByte => SMAutoFill.ClampByte;
        protected bool DisplayBoolean(bool settable)
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label(settable ? "True" : "False");
            bool set = GUILayout.Toggle(settable, string.Empty);
            if (set != settable)
            {
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
                return true;
            }
            return false;
        }

        // Simple
        protected bool DisplayByte(byte settable, ref string setCache, out byte newSettable)
        {
            bool setted = false;
            if (!byte.TryParse(setCache, out byte val) || val != settable)
                setCache = settable.ToString();
            string set = GUILayout.TextField(setCache, 32, AltUI.TextfieldBlackAdjusted, GUILayout.Width(180));
            if (long.TryParse(set, out long val2))
            {
                GUILayout.Label("<color=green>O</color>", GUILayout.Width(25));
                if (set != setCache)
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                    settable = ClampByte(val2);
                    setted = true;
                }
            }
            else
            {
                GUILayout.Label("<color=red>X</color>", GUILayout.Width(25));
            }
            setCache = set;
            newSettable = settable;
            return setted;
        }
        protected bool DisplayInt(int settable, ref string setCache, out int newSettable)
        {
            bool setted = false;
            if (!int.TryParse(setCache, out int val) || val != settable)
                setCache = settable.ToString();
            string set = GUILayout.TextField(setCache, 32, AltUI.TextfieldBlackAdjusted, GUILayout.Width(180));
            if (long.TryParse(set, out long val2))
            {
                GUILayout.Button("O", AltUI.ButtonGreen, GUILayout.Width(32), GUILayout.Height(32));
                if (set != setCache)
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                    settable = ClampInt(val2);
                    setted = true;
                }
            }
            else
            {
                GUILayout.Button("X", AltUI.ButtonRed, GUILayout.Width(32), GUILayout.Height(32));
            }
            setCache = set;
            newSettable = settable;
            return setted;
        }
        protected bool DisplayFloatRounded(float settable, ref string setCache, out float newSettable)
        {
            bool setted = false;
            if (!float.TryParse(setCache, out float val) || val.Approximately(settable))
                setCache = settable.ToString("F");
            string set = GUILayout.TextField(setCache, 32, AltUI.TextfieldBlackAdjusted, GUILayout.Width(180));
            if (long.TryParse(set, out long refined))
            {
                GUILayout.Button("O", AltUI.ButtonGreen, GUILayout.Width(32), GUILayout.Height(32));
                if (set != setCache)
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                    settable = ClampInt(refined);
                    setted = true;
                }
            }
            else
            {
                GUILayout.Button("X", AltUI.ButtonRed, GUILayout.Width(32), GUILayout.Height(32));
            }
            setCache = set;
            newSettable = settable;
            return setted;
        }
        protected bool DisplayFloat(float settable, ref string setCache, out float newSettable)
        {
            bool setted = false;
            if (!float.TryParse(setCache, out float val) || val.Approximately(settable))
                setCache = settable.ToString("F");
            string set = GUILayout.TextField(setCache, 32, AltUI.TextfieldBlackAdjusted, GUILayout.Width(180));
            if (float.TryParse(set, out val))
            {
                GUILayout.Button("O", AltUI.ButtonGreen, GUILayout.Width(32), GUILayout.Height(32));
                if (set != setCache)
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                    settable = val;
                    setted = true;
                }
            }
            else
            {
                GUILayout.Button("X", AltUI.ButtonRed, GUILayout.Width(32), GUILayout.Height(32));
            }
            setCache = set;
            newSettable = settable;
            return setted;
        }
        protected bool DisplayString(string settable, out string newSettable, float width = 210)
        {
            newSettable = GUILayout.TextField(settable == null ? "" : settable, 32, GUILayout.Width(width));
            if (newSettable != settable)
            {
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                return true;
            }
            return false;
        }
        protected bool DisplayStringArea(string settable, out string newSettable, float maxWidth = 750)
        {
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            newSettable = GUILayout.TextArea(settable == null ? "" : settable, 5000, GUILayout.MaxWidth(maxWidth));
            if (newSettable != settable)
            {
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                return true;
            }
            return false;
        }
        protected bool DisplayStringFloat(string settable, out string newSettable)
        {
            newSettable = GUILayout.TextField(settable == null ? "" : settable, 32, GUILayout.Width(180));
            if (float.TryParse(newSettable, out _))
                GUILayout.Button("O", AltUI.ButtonGreen, GUILayout.Width(32), GUILayout.Height(32));
            else
                GUILayout.Button("X", AltUI.ButtonRed, GUILayout.Width(32), GUILayout.Height(32));
            if (newSettable != settable)
            {
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                return true;
            }
            return false;
        }
        protected bool DisplayVec3(Vector3 settable, ref string setCache1,
            ref string setCache2, ref string setCache3, out Vector3 newSettable)
        {
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            bool setted = false;
            if (!float.TryParse(setCache1, out float val1) || !val1.Approximately(settable.x))
                setCache1 = settable.x.ToString("F");
            string set1 = GUILayout.TextField(setCache1, 32, AltUI.TextfieldBlackAdjusted, GUILayout.Width(120));
            if (float.TryParse(set1, out val1))
            {
                GUILayout.Label("<color=green>X</color>", GUILayout.Width(25));
                if (set1 != setCache1)
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                    settable = settable.SetX(val1);
                    setted = true;
                }
            }
            else
                GUILayout.Label("<color=red>X</color>", GUILayout.Width(25));

            if (!float.TryParse(setCache2, out float val2) || !val2.Approximately(settable.y))
                setCache2 = settable.y.ToString("F");
            string set2 = GUILayout.TextField(setCache2, 32, AltUI.TextfieldBlackAdjusted, GUILayout.Width(120));
            if (float.TryParse(set2, out val2))
            {
                GUILayout.Label("<color=green>Y</color>", GUILayout.Width(25));
                if (set2 != setCache2)
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                    settable = settable.SetY(val2);
                    setted = true;
                }
            }
            else
                GUILayout.Label("<color=red>Y</color>", GUILayout.Width(25));

            if (!float.TryParse(setCache3, out float val3) || !val3.Approximately(settable.z))
                setCache3 = settable.z.ToString("F");
            string set3 = GUILayout.TextField(setCache3, 32, AltUI.TextfieldBlackAdjusted, GUILayout.Width(120));
            if (float.TryParse(set3, out val3))
            {
                GUILayout.Label("<color=green>Z</color>", GUILayout.Width(25));
                if (set3 != setCache3)
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                    settable = settable.SetZ(val3);
                    setted = true;
                }
            }
            else
                GUILayout.Label("<color=red>Z</color>", GUILayout.Width(25));

            setCache1 = set1;
            setCache2 = set2;
            setCache3 = set3;
            newSettable = settable;
            return setted;
        }

        // Advanced
        public static bool DisplayFixedOptions(int settable, ref Vector2 scroller, ref bool opened,
            string[] options, int[] lookup, Dictionary<int, int> lookupInv, out int newSettable)
        {
            return SMAutoFill.FixedOptions(settable, ref scroller, ref opened, 
                options, out newSettable, lookup, lookupInv);
        }
        protected bool DisplayFixedOptions(float settable, ref Vector2 scroller, ref bool opened, 
            string[] options, int[] lookup, Dictionary<int, int> lookupInv, out float newSettable)
        {
            bool setted = false;
            GUILayout.BeginVertical();
            int num = Mathf.RoundToInt(settable);
            if (lookup != null)
                num = lookupInv[num];
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
                        if (lookup != null)
                            settable = lookup[step];
                        else
                            settable = step;
                        setted = true;
                        opened = false;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
            newSettable = settable;
            return setted;
        }

    }
}
