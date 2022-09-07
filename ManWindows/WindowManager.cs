using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sub_Missions.Steps;
using TerraTechETCUtil;

namespace Sub_Missions.ManWindows
{
    public class WindowManager : MonoBehaviour
    {   // Global popup manager
        public static WindowManager inst;

        internal const int IDOffset = 135000;
        private const int MaxPopups = 48;
        private const int MaxPopupsActive = 32;

        public static Rect DefaultWindow = new Rect(0, 0, 300, 300);   // the "window"
        public static Rect LargeWindow = new Rect(0, 0, 1000, 600);
        public static Rect WideWindow = new Rect(0, 0, 700, 180);
        public static Rect SmallWideWindow = new Rect(0, 0, 600, 140);
        public static Rect SmallHalfWideWindow = new Rect(0, 0, 350, 140);
        public static Rect SideWindow = new Rect(0, 0, 200, 125);
        public static Rect TinyWindow = new Rect(0, 0, 160, 75);
        public static Rect MicroWindow = new Rect(0, 0, 110, 40);
        public static Rect TinyWideWindow = new Rect(0, 0, 260, 100);
        public static Rect SmallWindow = new Rect(0, 0, 160, 120);

        //public static GUIStyle styleSmallFont;
        public static GUIStyle styleDescFont;
        public static GUIStyle styleDescLargeFont;
        public static GUIStyle styleDescLargeFontScroll;
        public static GUIStyle styleLargeFont;
        public static GUIStyle styleHugeFont;
        public static GUIStyle styleGinormusFont;


        public static List<GUIPopupDisplay> AllPopups = new List<GUIPopupDisplay>();
        public static Vector3 PlayerLoc = Vector3.zero;
        public static bool isCurrentlyOpen = false;
        public static bool IndexesChanged = false;

        public static int numActivePopups;
        public static bool SetupAltWins = false;


        private static GUIPopupDisplay currentGUIWindow;
        private static int updateClock = 0;
        private static int updateClockDelay = 50;




        public static void Initiate()
        {
            if (!inst)
            {
                inst = Instantiate(new GameObject()).AddComponent<WindowManager>();
            }
            Debug_SMissions.Log("SubMissions: WindowManager initated");
        }
        public static void LateInitiate()
        {
            ManPauseGame.inst.PauseEvent.Subscribe(SetVisibilityOfAllPopups);
        }
        public static void DeInit()
        {
            if (!inst)
                return;
            RemoveALLPopups();
            ManPauseGame.inst.PauseEvent.Unsubscribe(SetVisibilityOfAllPopups);
            Debug_SMissions.Log("SubMissions: WindowManager De-Init");
        }

        public static void SetCurrentPopup(GUIPopupDisplay disp)
        {
            if (AllPopups.Contains(disp))
            {
                currentGUIWindow = disp;
            }
            else
            {
                Debug_SMissions.Log("SubMissions: GUIPopupDisplay \"" + disp.context + "\" is not in the AllPopups list!");
            }
        }
        public static GUIPopupDisplay GetCurrentPopup()
        {
            return currentGUIWindow;
        }
        public static void KeepWithinScreenBounds(GUIPopupDisplay disp)
        {
            disp.Window.x = Mathf.Clamp(disp.Window.x, 0, Display.main.renderingWidth - disp.Window.width);
            disp.Window.y = Mathf.Clamp(disp.Window.y, 0, Display.main.renderingHeight - disp.Window.height);
        }
        public static void KeepWithinScreenBoundsNonStrict(GUIPopupDisplay disp)
        {
            disp.Window.x = Mathf.Clamp(disp.Window.x, 10 - disp.Window.width, Display.main.renderingWidth - 10);
            disp.Window.y = Mathf.Clamp(disp.Window.y, 10 - disp.Window.height, Display.main.renderingHeight - 10);
        }
        /// <summary>
        /// screenPos x and y must be within 0-1 float range!
        /// </summary>
        /// <param name="screenPos"></param>
        /// <param name="disp"></param>
        public static void ChangePopupPositioning(Vector2 screenPos, GUIPopupDisplay disp)
        {
            disp.Window.x = (Display.main.renderingWidth - disp.Window.width) * screenPos.x;
            disp.Window.y = (Display.main.renderingHeight - disp.Window.height) * screenPos.y;
        }
        /// <summary>
        /// screenPos x and y must be within 0-1 float range!
        /// </summary>
        /// <param name="screenPos"></param>
        /// <param name="disp"></param>
        public static bool PopupPositioningApprox(Vector2 screenPos, GUIPopupDisplay disp, float squareDelta = 0.1f)
        {
            float valX = disp.Window.x / (Display.main.renderingWidth - disp.Window.width);
            float valY = disp.Window.y / (Display.main.renderingHeight - disp.Window.height);
            return valX > -squareDelta && valX < squareDelta && valY > -squareDelta && valY < squareDelta;
        }
        public static bool DoesPopupExist(string title, GUISetTypes type, out GUIPopupDisplay exists)
        {
            exists = AllPopups.Find(delegate (GUIPopupDisplay cand)
            {
                return cand.type == type && cand.context.CompareTo(title) == 0;
            }
            );
            return exists;
        }
        public static GUIPopupDisplay GetPopup(string title, GUISetTypes type)
        {
            return AllPopups.Find(delegate (GUIPopupDisplay cand)
            {
                return cand.type == type && cand.context.CompareTo(title) == 0;
            }
            );
        }
        public static List<GUIPopupDisplay> GetAllPopups(GUISetTypes type)
        {
            return AllPopups.FindAll(delegate (GUIPopupDisplay cand)
            {
                return cand.type == type;
            }
            );
        }
        public static List<GUIPopupDisplay> GetAllActivePopups(GUISetTypes type, bool active = true)
        {
            return AllPopups.FindAll(delegate (GUIPopupDisplay cand)
            {
                return cand.isOpen == active && cand.type == type;
            }
            );
        }


        public static bool AddPopupButton(string title, string buttonLabel, bool removeOnPress, string libFunctionName, object windowOverride = null)
        {
            return AddPopupStackable(GUISetTypes.Button, title, buttonLabel, removeOnPress, libFunctionName, windowOverride: windowOverride);
        }
        public static bool AddPopupButtonDual(string title, string buttonLabel, bool removeOnPress, string libFunctionName, object windowOverride = null)
        {
            return AddPopupStackable(GUISetTypes.ButtonDual, title, buttonLabel, removeOnPress, libFunctionName, windowOverride: windowOverride);
        }
        public static bool AddPopupButtonDual(string title, string buttonLabel, bool removeOnPress, StepActOptions options, object windowOverride = null)
        {
            return AddPopupStackable(GUISetTypes.ButtonDual, title, buttonLabel, removeOnPress, options, windowOverride: windowOverride);
        }

        public static bool AddPopupMissionsList()
        {
            return AddPopupSingle(GUISetTypes.List, "<b>-- Sub Missions DEBUG --</b>");
        }
        public static bool AddPopupMessageScroll(string title, string message, float scrollSpeed = 0.02f, bool Dual = false, SMissionStep missionStep = null, object windowOverride = null)
        {
            return AddPopupStackable(GUISetTypes.MessageScroll, title, message, scrollSpeed, Dual, missionStep, windowOverride: windowOverride);
        }
        public static bool AddPopupMessageSide()
        {
            return AddPopupSingle(GUISetTypes.MessageSide, "");
        }
        public static bool AddPopupMessage(string title, string message)
        {
            return AddPopupSingle(GUISetTypes.Message, title, message);
        }

        private static bool AddPopupStackable(GUISetTypes type, string context, object val1 = null, object val2 = null, object val3 = null, object val4 = null, object windowOverride = null)
        {
            if (MaxPopups <= AllPopups.Count)
            {
                Debug_SMissions.Log("SubMissions: Too many popups!!!  Aborting AddPopup!");
                return false;
            }
            var gameObj = new GameObject(context);
            currentGUIWindow = gameObj.AddComponent<GUIPopupDisplay>();
            currentGUIWindow.obj = gameObj;
            currentGUIWindow.context = context;
            currentGUIWindow.Window = new Rect(DefaultWindow);
            currentGUIWindow.SetupGUI(type, val1, val2, val3, val4, windowOverride);
            gameObj.SetActive(false);
            currentGUIWindow.isOpen = false;

            AllPopups.Add(currentGUIWindow);
            UpdateIndexes();
            return true;
        }

        private static bool AddPopupSingle(GUISetTypes type, string Header, object val1 = null, object val2 = null, object val3 = null, object val4 = null, object windowOverride = null)
        {
            if (DoesPopupExist(Header, type, out GUIPopupDisplay exists))
            {
                SetCurrentPopup(exists);
                RefreshPopup(exists, val1, val2, val3, val4, windowOverride);
                return true;
            }
            if (MaxPopups <= AllPopups.Count)
            {
                Debug_SMissions.Log("SubMissions: Too many popups!!!  Aborting AddPopup!");
                return false;
            }
            var gameObj = new GameObject(Header);
            currentGUIWindow = gameObj.AddComponent<GUIPopupDisplay>();
            currentGUIWindow.obj = gameObj;
            currentGUIWindow.context = Header;
            currentGUIWindow.Window = new Rect(DefaultWindow);
            currentGUIWindow.SetupGUI(type, val1, val2, val3, val4, windowOverride);
            gameObj.SetActive(false);
            currentGUIWindow.isOpen = false;

            AllPopups.Add(currentGUIWindow);
            UpdateIndexes();
            return true;
        }
        private static void RefreshPopup(GUIPopupDisplay disp, object val1 = null, object val2 = null, object val3 = null, object val4 = null, object windowOverride = null)
        {
            disp.SetupGUI(disp.type, val1, val2, val3, val4, windowOverride);
        }
        public static bool RemovePopup(GUIPopupDisplay disp)
        {
            try
            {
                disp.GUIFormat.OnRemoval();
                Destroy(disp.gameObject);
                if (disp == null)
                {
                    Debug_SMissions.Log("SubMissions: RemoveCurrentPopup - POPUP IS NULL!!");
                    return false;
                }
                AllPopups.Remove(disp);
            }
            catch { }
            try
            {
                UpdateIndexes();
                return true;
            }
            catch { }
            return false;
        }
        public static bool RemoveALLPopups()
        {
            int FireTimes = AllPopups.Count;
            for (int step = 0; step < FireTimes; step++)
            {
                try
                {
                    RemovePopup(AllPopups.First());
                }
                catch { }
            }
            return true;
        }

        public static void UpdateIndexes()
        {
            foreach (GUIPopupDisplay disp in AllPopups)
            {
                disp.ID = AllPopups.IndexOf(disp) + IDOffset;
            }
        }
        public static void UpdateStatuses()
        {
            foreach (GUIPopupDisplay disp in AllPopups)
            {
                disp.GUIFormat.DelayedUpdate();
            }
        }


        /// <summary>
        /// Position in percent of the max screen width and length
        /// </summary>
        /// <param name="screenPos"></param>
        /// <returns></returns>
        public static bool ShowPopup(Vector2 screenPos, GUIPopupDisplay disp)
        {
            bool shown = ShowPopup(disp);
            if (screenPos.x > 1)
                screenPos.x = 1;
            if (screenPos.y > 1)
                screenPos.y = 1;
            disp.Window.x = (Display.main.renderingWidth - disp.Window.width) * screenPos.x;
            disp.Window.y = (Display.main.renderingHeight - disp.Window.height) * screenPos.y;

            return shown;
        }
        /// <summary>
        /// Position in percent of the max screen width and length. 
        ///  Controls the currentGUIWindow
        /// </summary>
        public static bool ShowPopup(Vector2 screenPos)
        {
            bool shown = ShowPopup();
            if (screenPos.x > 1)
                screenPos.x = 1;
            if (screenPos.y > 1)
                screenPos.y = 1;
            currentGUIWindow.Window.x = (Display.main.renderingWidth - currentGUIWindow.Window.width) * screenPos.x;
            currentGUIWindow.Window.y = (Display.main.renderingHeight - currentGUIWindow.Window.height) * screenPos.y;

            return shown;
        }
        public static bool ShowPopup(GUIPopupDisplay disp)
        {
            if (MaxPopupsActive <= numActivePopups)
            {
                SMUtil.Assert(false, "Too many popups active!!!  Limit is " + MaxPopupsActive + ".  Aborting ShowPopup!");
                return false;
            }
            if (disp.isOpen)
            {
                //Debug_SMissions.Log("SubMissions: ShowPopup - Window is already open");
                return false;
            }
            Debug_SMissions.Log("SubMissions: Popup " + disp.context + " active");
            disp.GUIFormat.OnOpen();
            disp.obj.SetActive(true);
            disp.isOpen = true;
            Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.InfoOpen);
            numActivePopups++;
            return true;
        }
        /// <summary>
        /// Controls the currentGUIWindow
        /// </summary>
        public static bool ShowPopup()
        {
            if (MaxPopupsActive <= numActivePopups)
            {
                SMUtil.Assert(false, "Too many popups active!!!  Limit is " + MaxPopupsActive + ".  Aborting ShowPopup!");
                return false;
            }
            if (currentGUIWindow.isOpen)
            {
                //Debug_SMissions.Log("SubMissions: ShowPopup - Window is already open");
                return false;
            }
            numActivePopups++;
            Debug_SMissions.Log("SubMissions: Popup " + currentGUIWindow.context + " active");
            currentGUIWindow.GUIFormat.OnOpen();
            currentGUIWindow.obj.SetActive(true);
            currentGUIWindow.isOpen = true;
            Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.InfoOpen);
            return true;
        }

        public static bool HidePopup(GUIPopupDisplay disp)
        {
            if (disp == null)
            {
                Debug_SMissions.Log("SubMissions: HidePopup - THE WINDOW IS NULL!!");
                return false;
            }
            if (!disp.isOpen)
            {
                //Debug_SMissions.Log("SubMissions: HidePopup - Window is already closed");
                return false;
            }
            numActivePopups--;
            disp.obj.SetActive(false);
            disp.isOpen = false;
            Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.InfoClose);
            return true;
        }

        private static bool allPopupsOpen = true;
        private static List<GUIPopupDisplay> PopupsClosed = new List<GUIPopupDisplay>();
        public static void SetVisibilityOfAllPopups(bool Closed)
        {
            bool isOpen = !Closed;
            if (allPopupsOpen != isOpen)
            {
                allPopupsOpen = isOpen;
                if (isOpen)
                {
                    foreach (GUIPopupDisplay disp in PopupsClosed)
                    {
                        try
                        {
                            disp.obj.SetActive(true);
                            disp.isOpen = true;
                        }
                        catch { }
                    }
                    PopupsClosed.Clear();
                }
                else
                {
                    foreach (GUIPopupDisplay disp in AllPopups)
                    {
                        try
                        {
                            if (disp.obj.activeSelf)
                            {
                                disp.obj.SetActive(false);
                                disp.isOpen = false;
                                PopupsClosed.Add(disp);
                            }
                        }
                        catch { }
                    }
                }
            }
        }


        private void Update()
        {
            UpdateAllFast();
            if (updateClock > updateClockDelay)
            {
                UpdateAllPopups();
                updateClock = 0;
            }
            updateClock++;
        }

        public static void UpdateAllPopups()
        {
            int FireTimes = AllPopups.Count;
            for (int step = 0; step < FireTimes; step++)
            {
                try
                {
                    GUIPopupDisplay disp = AllPopups.ElementAt(step);
                    disp.GUIFormat.DelayedUpdate();
                }
                catch { }
            }
        }
        public static void UpdateAllFast()
        {
            int FireTimes = AllPopups.Count;
            for (int step = 0; step < FireTimes; step++)
            {
                try
                {
                    GUIPopupDisplay disp = AllPopups.ElementAt(step);
                    disp.GUIFormat.FastUpdate();
                }
                catch { }
            }
        }
    }

    public class GUIPopupDisplay : MonoBehaviour
    {
        public int ID = WindowManager.IDOffset;
        public GameObject obj;
        public string context = "error";
        public bool isOpen = false;
        public bool isOpaque = false;
        public Rect Window = new Rect(WindowManager.DefaultWindow);   // the "window"
        public GUISetTypes type = GUISetTypes.Default;
        public IGUIFormat GUIFormat;

        private void OnGUI()
        {
            if (isOpen)
            {
                if (isOpaque)
                    AltUI.StartUIOpaque();
                else
                    AltUI.StartUI();
                if (!WindowManager.SetupAltWins)
                {
                    WindowManager.styleDescLargeFont = new GUIStyle(GUI.skin.textField);
                    WindowManager.styleDescLargeFont.fontSize = 16;
                    WindowManager.styleDescLargeFont.alignment = TextAnchor.MiddleLeft;
                    WindowManager.styleDescLargeFont.wordWrap = true;
                    WindowManager.styleDescLargeFontScroll = new GUIStyle(WindowManager.styleDescLargeFont);
                    WindowManager.styleDescLargeFontScroll.alignment = TextAnchor.UpperLeft;
                    WindowManager.styleDescFont = new GUIStyle(GUI.skin.textField);
                    WindowManager.styleDescFont.fontSize = 12;
                    WindowManager.styleDescFont.alignment = TextAnchor.UpperLeft;
                    WindowManager.styleDescFont.wordWrap = true;
                    WindowManager.styleLargeFont = new GUIStyle(GUI.skin.label);
                    WindowManager.styleLargeFont.fontSize = 16;
                    WindowManager.styleHugeFont = new GUIStyle(GUI.skin.button);
                    WindowManager.styleHugeFont.fontSize = 20;
                    WindowManager.styleGinormusFont = new GUIStyle(GUI.skin.button);
                    WindowManager.styleGinormusFont.fontSize = 38;
                    WindowManager.SetupAltWins = true;
                    Debug_SMissions.Log("SubMissions: WindowManager performed first setup");
                }
                Window = GUI.Window(ID, Window, GUIFormat.RunGUI, context);
                AltUI.EndUI();
            }
        }

        public void SetupGUI(GUISetTypes newType, object val1 = null, object val2 = null, object val3 = null, object val4 = null, object windowOverride = null)
        {
            IGUIFormat gui;
            if (GUIFormat != null)
            {
                if (type != newType)
                {
                    Debug_SMissions.Assert("SubMissions: SetupGUI - Illegal type change on inited GUIPopupDisplay!");
                    GUIFormat.OnRemoval();
                    GUIFormat = null;
                }
            }
            type = newType;
            switch (newType)
            {
                case GUISetTypes.Button:
                    GUIButtonWindow guiSet2;
                    if (GUIFormat != null)
                        guiSet2 = (GUIButtonWindow)GUIFormat;
                    else
                        guiSet2 = new GUIButtonWindow();
                    guiSet2.Setup(this, (string)val1, (bool)val2, (string)val3);
                    Window = WindowManager.TinyWindow;
                    if (windowOverride != null)
                        Window = (Rect)windowOverride;
                    gui = guiSet2;
                    break;
                case GUISetTypes.ButtonDual:
                    GUIDualButton guiSet3;
                    if (GUIFormat != null)
                        guiSet3 = (GUIDualButton)GUIFormat;
                    else
                        guiSet3 = new GUIDualButton();
                    guiSet3.Setup(this, (string)val1, (bool)val2, val3);
                    Window = WindowManager.TinyWideWindow;
                    if (windowOverride != null)
                        Window = (Rect)windowOverride;
                    gui = guiSet3;
                    break;
                case GUISetTypes.List:
                    GUISMissionsList guiSet4;
                    if (GUIFormat != null)
                        guiSet4 = (GUISMissionsList)GUIFormat;
                    else
                        guiSet4 = new GUISMissionsList();
                    guiSet4.Setup(this);
                    Window = WindowManager.LargeWindow;
                    gui = guiSet4;
                    break;
                case GUISetTypes.MessageScroll:
                    GUIScrollMessage guiSet5;
                    if (GUIFormat != null)
                        guiSet5 = (GUIScrollMessage)GUIFormat;
                    else
                        guiSet5 = new GUIScrollMessage();
                    isOpaque = true;
                    guiSet5.Setup(this, (string)val1, (float)val2, (bool)val3, (SMissionStep)val4);
                    Window = WindowManager.WideWindow;
                    if (windowOverride != null)
                        Window = (Rect)windowOverride;
                    gui = guiSet5;
                    break;
                case GUISetTypes.MessageSide:
                    GUIMissionInfo guiSet6;
                    if (GUIFormat != null)
                        guiSet6 = (GUIMissionInfo)GUIFormat;
                    else
                        guiSet6 = new GUIMissionInfo();
                    guiSet6.Setup(this);
                    Window = WindowManager.SideWindow;
                    gui = guiSet6;
                    break;
                case GUISetTypes.Message:
                default:
                    GUIMessage guiSet9 = new GUIMessage();
                    if (GUIFormat != null)
                        guiSet9 = (GUIMessage)GUIFormat;
                    else
                        guiSet9 = new GUIMessage();
                    guiSet9.Setup(this, (string)val1);
                    Window = WindowManager.LargeWindow;
                    gui = guiSet9;
                    break;
            }
            GUIFormat = gui;
        }
    }
    public enum GUISetTypes
    {
        Default,    // not set
        Portrait,   // Character display
        Warning,    // Little popup if action taken is bad/dangerous
        Message,    // 
        MessageScroll,// like the normal terratech NPC chat boxes
        MessageSide,
        List,       // 
        ButtonDual,
        Button      // 
    }
}
