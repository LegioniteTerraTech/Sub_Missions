using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sub_Missions.Steps;

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
        public static Rect SideWindow = new Rect(0, 0, 200, 125);
        public static Rect TinyWindow = new Rect(0, 0, 160, 75);
        public static Rect TinyWideWindow = new Rect(0, 0, 260, 100);
        public static Rect SmallWindow = new Rect(0, 0, 160, 120);

        //public static GUIStyle styleSmallFont;
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
            inst = Instantiate(new GameObject()).AddComponent<WindowManager>();
            Debug.Log("SubMissions: WindowManager initated");
        }
        public static void SetCurrentPopup(GUIPopupDisplay disp)
        {
            if (AllPopups.Contains(disp))
            {
                currentGUIWindow = disp;
            }
            else
            {
                Debug.Log("SubMissions: GUIPopupDisplay \"" + disp.context + "\" is not in the AllPopups list!");
            }
        }
        public static GUIPopupDisplay GetCurrentPopup()
        {
            return currentGUIWindow;
        }
        public static void ChangePopupPositioning(Vector2 screenPos, GUIPopupDisplay disp)
        {
            if (screenPos.x > 1)
                screenPos.x = 1;
            if (screenPos.y > 1)
                screenPos.y = 1;
            disp.Window.x = (Display.main.renderingWidth - disp.Window.width) * screenPos.x;
            disp.Window.y = (Display.main.renderingHeight - disp.Window.height) * screenPos.y;
        }
        public static bool DoesPopupExist(string title, GUISetTypes type)
        {
            return AllPopups.Exists(delegate (GUIPopupDisplay cand) 
            { 
                return cand.context == title && cand.type == type; 
            }
            );
        }
        public static GUIPopupDisplay GetPopup(string title, GUISetTypes type)
        {
            return AllPopups.Find(delegate (GUIPopupDisplay cand)
            {
                return cand.context == title && cand.type == type;
            }
            );
        }


        public static bool AddPopupButton(string title, string buttonLabel, bool removeOnPress, string libFunctionName, object windowOverride = null)
        {
            if (DoesPopupExist(title, GUISetTypes.Button))
            {
                SetCurrentPopup(GetPopup(title, GUISetTypes.Button));
                RefreshPopup(GetPopup(title, GUISetTypes.Button), buttonLabel, removeOnPress, libFunctionName, windowOverride);
                return true;
            }
            return AddPopup(GUISetTypes.Button, title, buttonLabel, removeOnPress, libFunctionName, windowOverride: windowOverride);
        }
        public static bool AddPopupButtonDual(string title, string buttonLabel, bool removeOnPress, string libFunctionName, object windowOverride = null)
        {
            if (DoesPopupExist(title, GUISetTypes.ButtonDual))
            {
                SetCurrentPopup(GetPopup(title, GUISetTypes.ButtonDual));
                RefreshPopup(GetPopup(title, GUISetTypes.ButtonDual), buttonLabel, removeOnPress, libFunctionName, windowOverride);
                return true;
            }
            return AddPopup(GUISetTypes.ButtonDual, title, buttonLabel, removeOnPress, libFunctionName, windowOverride: windowOverride);
        }
        public static bool AddPopupButtonDual(string title, string buttonLabel, bool removeOnPress, StepActOptions options, object windowOverride = null)
        {
            if (DoesPopupExist(title, GUISetTypes.ButtonDual))
            {
                SetCurrentPopup(GetPopup(title, GUISetTypes.ButtonDual));
                RefreshPopup(GetPopup(title, GUISetTypes.ButtonDual), buttonLabel, removeOnPress, options, windowOverride);
                return true;
            }
            return AddPopup(GUISetTypes.ButtonDual, title, buttonLabel, removeOnPress, options, windowOverride: windowOverride);
        }

        public static bool AddPopupMissionsList()
        {
            return AddPopup(GUISetTypes.List, "<b>-- Sub Missions --</b>");
        }
        public static bool AddPopupMessageScroll(string title, string message, float scrollSpeed = 0.02f, object windowOverride = null)
        {
            if (DoesPopupExist(title, GUISetTypes.MessageScroll))
            {
                SetCurrentPopup(GetPopup(title, GUISetTypes.MessageScroll));
                RefreshPopup(GetPopup(title, GUISetTypes.MessageScroll), message, scrollSpeed, windowOverride: windowOverride);
                return true;
            }
            return AddPopup(GUISetTypes.MessageScroll, title, message, scrollSpeed, windowOverride: windowOverride);
        }
        public static bool AddPopupMessageSide()
        {
            return AddPopup(GUISetTypes.MessageSide, "");
        }
        public static bool AddPopupMessage(string title, string message)
        {
            if (DoesPopupExist(title, GUISetTypes.Message))
            {
                SetCurrentPopup(GetPopup(title, GUISetTypes.Message));
                RefreshPopup(GetPopup(title, GUISetTypes.Message), message);
                return true;
            }
            return AddPopup(GUISetTypes.Message, title, message);
        }

        private static bool AddPopup(GUISetTypes type, string context, object val1 = null, object val2 = null, object val3 = null, object val4 = null, object windowOverride = null)
        {
            if (MaxPopups <= AllPopups.Count)
            {
                Debug.Log("SubMissions: Too many popups!!!  Aborting AddPopup!");
                return false;
            }
            var gameObj = new GameObject(context);
            currentGUIWindow = gameObj.AddComponent<GUIPopupDisplay>();
            currentGUIWindow.obj = gameObj;
            currentGUIWindow.context = context;
            currentGUIWindow.Window = new Rect(DefaultWindow);
            currentGUIWindow.GUIFormat = currentGUIWindow.SetupGUI(type, val1, val2, val3, val4, windowOverride);
            gameObj.SetActive(false);
            currentGUIWindow.isOpen = false;

            AllPopups.Add(currentGUIWindow);
            UpdateIndexes();
            return true;
        }
        private static bool RefreshPopup(GUIPopupDisplay disp, object val1 = null, object val2 = null, object val3 = null, object val4 = null, object windowOverride = null)
        {
            disp.GUIFormat = disp.SetupGUI(disp.type, val1, val2, val3, val4, windowOverride);
            return true;
        }
        public static bool RemoveCurrentPopup()
        {
            if (currentGUIWindow == null)
            {
                Debug.Log("SubMissions: RemoveCurrentPopup - There is no current window active!!");
                return false;
            }
            AllPopups.Remove(currentGUIWindow);
            Destroy(currentGUIWindow);
            UpdateIndexes();

            /*
            Vector3 Mous = Input.mousePosition;
            xMenu = 0;
            yMenu = 0;
            */
            return true;
        }
        public static bool RemovePopup(GUIPopupDisplay disp)
        {
            try
            {
                disp.GUIFormat.OnRemoval();
                Destroy(disp.gameObject);
                if (disp == null)
                {
                    Debug.Log("SubMissions: RemoveCurrentPopup - POPUP IS NULL!!");
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
                Debug.Log("SubMissions: Too many popups active!!!  Aborting ShowPopup!");
                return false;
            }
            if (disp.isOpen)
            {
                //Debug.Log("SubMissions: ShowPopup - Window is already open");
                return false;
            }
            Debug.Log("SubMissions: Popup active");
            disp.obj.SetActive(true);
            disp.isOpen = true;
            Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.InfoOpen);
            numActivePopups++;
            return true;
        }
        public static bool ShowPopup()
        {
            if (MaxPopupsActive <= numActivePopups)
            {
                Debug.Log("SubMissions: Too many popups active!!!  Aborting ShowPopup!");
                return false;
            }
            if (currentGUIWindow.isOpen)
            {
                //Debug.Log("SubMissions: ShowPopup - Window is already open");
                return false;
            }
            numActivePopups++;
            Debug.Log("SubMissions: Popup active");
            currentGUIWindow.obj.SetActive(true);
            currentGUIWindow.isOpen = true;
            Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.InfoOpen);
            return true;
        }

        public static bool HidePopup()
        {
            if (currentGUIWindow == null)
            {
                Debug.Log("SubMissions: HidePopup - There is no current window active!!");
                return false;
            }
            if (!currentGUIWindow.isOpen)
            {
                //Debug.Log("SubMissions: HidePopup - Window is already closed");
                return false;
            }
            numActivePopups--;
            currentGUIWindow.obj.SetActive(false);
            currentGUIWindow.isOpen = false;
            Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.InfoClose);
            return true;
        }
        public static bool HidePopup(GUIPopupDisplay disp)
        {
            if (disp == null)
            {
                Debug.Log("SubMissions: HidePopup - THE WINDOW IS NULL!!");
                return false;
            }
            if (!disp.isOpen)
            {
                //Debug.Log("SubMissions: HidePopup - Window is already closed");
                return false;
            }
            numActivePopups--;
            disp.obj.SetActive(false);
            disp.isOpen = false;
            Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.InfoClose);
            return true;
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
        public Rect Window = new Rect(WindowManager.DefaultWindow);   // the "window"
        public GUISetTypes type = GUISetTypes.Default;
        public IGUIFormat GUIFormat;

        private void OnGUI()
        {
            if (isOpen)
            {
                Window = GUI.Window(ID, Window, GUIFormat.RunGUI, context);
            }
        }

        public IGUIFormat SetupGUI(GUISetTypes type, object val1 = null, object val2 = null, object val3 = null, object val4 = null, object windowOverride = null)
        {
            IGUIFormat gui;
            switch (type)
            {
                case GUISetTypes.Button:
                    GUIButtonWindow guiSet2 = new GUIButtonWindow();
                    guiSet2.Setup(this, (string)val1, (bool)val2, (string)val3);
                    Window = WindowManager.TinyWindow;
                    if (windowOverride != null)
                        Window = (Rect)windowOverride;
                    gui = guiSet2;
                    break;
                case GUISetTypes.ButtonDual:
                    GUIDualButton guiSet3 = new GUIDualButton();
                    guiSet3.Setup(this, (string)val1, (bool)val2, val3);
                    Window = WindowManager.TinyWideWindow;
                    if (windowOverride != null)
                        Window = (Rect)windowOverride;
                    gui = guiSet3;
                    break;
                case GUISetTypes.List:
                    GUISMissionsList guiSet4 = new GUISMissionsList();
                    guiSet4.Setup(this);
                    Window = WindowManager.LargeWindow;
                    gui = guiSet4;
                    break;
                case GUISetTypes.MessageScroll:
                    GUIScrollMessage guiSet5 = new GUIScrollMessage();
                    guiSet5.Setup(this, (string)val1, (float)val2);
                    Window = WindowManager.WideWindow;
                    if (windowOverride != null)
                        Window = (Rect)windowOverride;
                    gui = guiSet5;
                    break;
                case GUISetTypes.MessageSide:
                    GUIMessageSide guiSet6 = new GUIMessageSide();
                    guiSet6.Setup(this);
                    Window = WindowManager.SideWindow;
                    gui = guiSet6;
                    break;
                case GUISetTypes.Message:
                default:
                    GUIMessage guiSet9 = new GUIMessage();
                    guiSet9.Setup(this, (string)val1);
                    Window = WindowManager.LargeWindow;
                    gui = guiSet9;
                    break;
            }
            return gui;
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
