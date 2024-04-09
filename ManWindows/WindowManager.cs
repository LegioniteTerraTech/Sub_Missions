using Sub_Missions.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using TerraTechETCUtil;
using UnityEngine;
using static Sub_Missions.ManWindows.WindowManager;

namespace Sub_Missions.ManWindows
{
    public class GUIDisplayStatsLegacy : GUIDisplayStats
    {
        public object val1 = null;
        public object val2 = null;
        public object val3 = null;
        public object val4 = null;
    }
    public class WindowManager : ManModGUI
    {
        private static GUIDisplayStatsLegacy DSL = new GUIDisplayStatsLegacy();
        public static bool AddPopupButton(string title, string buttonLabel, bool removeOnPress, string libFunctionName, object windowOverride = null)
        {
            if (windowOverride != null)
                DSL.windowSize = (Rect)windowOverride;
            else
                DSL.windowSize = WindowManager.TinyWindow;
            DSL.val1 = buttonLabel; DSL.val2 = removeOnPress; DSL.val3 = libFunctionName; 
            return AddPopupStackable<GUIButtonWindow>(title, DSL);
        }
        public static bool AddPopupButtonDual(string title, string buttonLabel, bool removeOnPress, string libFunctionName, object windowOverride = null)
        {
            if (windowOverride != null)
                DSL.windowSize = (Rect)windowOverride;
            else
                DSL.windowSize = WindowManager.TinyWideWindow;
            DSL.val1 = buttonLabel; DSL.val2 = removeOnPress; DSL.val3 = libFunctionName;
            return AddPopupStackable<GUIDualButton>(title, DSL);
        }
        public static bool AddPopupButtonDual(string title, string buttonLabel, bool removeOnPress, StepActOptions options, object windowOverride = null)
        {
            if (windowOverride != null)
                DSL.windowSize = (Rect)windowOverride;
            else
                DSL.windowSize = WindowManager.TinyWideWindow;
            DSL.val1 = buttonLabel; DSL.val2 = removeOnPress; DSL.val3 = options;
            return AddPopupStackable<GUIDualButton>(title, DSL);
        }


        public static bool AddPopupMissionsDEVControl()
        {
            DSL.windowSize = WindowManager.TinyWideWindow;
            return AddPopupSingle<GUIDevControl>(string.Empty, DSL);
        }
        public static bool AddPopupMissionsList()
        {
            DSL.windowSize = WindowManager.LargeWindow;
            return AddPopupSingle<GUISMissionsList>("<b>-- Sub Missions DEBUG --</b>", DSL);
        }
        public static bool AddPopupMissionEditor()
        {
            DSL.windowSize = WindowManager.LargeWindow;
            return AddPopupSingle<GUISMissionEditor>("<b>-- Sub Mission Editor --</b>", DSL);
        }
        public static bool AddPopupMessageScroll(string title, string message, float scrollSpeed = 0.02f, bool Dual = false, SMissionStep missionStep = null, object windowOverride = null)
        {
            if (windowOverride != null)
                DSL.windowSize = (Rect)windowOverride;
            else
                DSL.windowSize = WindowManager.WideWindow;
            DSL.val1 = message; DSL.val2 = scrollSpeed; DSL.val3 = Dual; DSL.val4 = missionStep;
            return AddPopupStackable<GUIScrollMessage>(title, DSL);
        }
        public static bool AddPopupMessageSide()
        {
            DSL.windowSize = WindowManager.SideWindow;
            DSL.val1 = string.Empty;
            return AddPopupSingle<GUIMissionInfo>(string.Empty, DSL);
        }
        public static bool AddPopupMessage(string title, string message)
        {
            DSL.windowSize = WindowManager.LargeWindow;
            DSL.val1 = message;
            return AddPopupSingle<GUIMessage>(title, DSL);
        }
        internal static bool AddPopupMessageError(string title, SMUtil.ErrorQueue errors)
        {
            DSL.windowSize = WindowManager.LargeWindow;
            DSL.val1 = errors;
            return AddPopupSingle<GUIMessageError>(title, DSL);
        }

    }
}
