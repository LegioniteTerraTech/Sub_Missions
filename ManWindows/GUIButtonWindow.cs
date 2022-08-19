using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.ManWindows
{
    public class GUIButtonWindow : IGUIFormat
    {
        public GUIPopupDisplay Display { get; set; }
        public string buttonMessage;
        public bool DestroyOnPress = false;
        public string InvokeAction;


        public void Setup(GUIPopupDisplay display, string buttonLabel, bool removeOnPress, string ActionName)
        {
            Display = display;
            buttonMessage = buttonLabel;
            DestroyOnPress = removeOnPress;
            InvokeAction = ActionName;
        }

        public void RunGUI(int ID)
        {
            if (!WindowManager.SetupAltWins)
            {
                WindowManager.styleDescLargeFont = new GUIStyle(GUI.skin.textField);
                WindowManager.styleDescLargeFont.fontSize = 16;
                WindowManager.styleDescLargeFont.alignment = TextAnchor.MiddleLeft;
                WindowManager.styleDescLargeFont.wordWrap = true;
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

            if (GUI.Button(new Rect(0, 10, Display.Window.width, Display.Window.height - 10), buttonMessage, WindowManager.styleHugeFont))
            {
                Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Open);
                ButtonAct.inst.Invoke(InvokeAction, 0);// has frame delay

                if (DestroyOnPress)
                {
                    WindowManager.HidePopup(Display);
                    WindowManager.RemovePopup(Display);
                }
            }
            GUI.DragWindow();
            WindowManager.KeepWithinScreenBounds(Display);
        }

        public void DelayedUpdate()
        {
        }
        public void FastUpdate()
        {
        }
        public void OnRemoval() { }
    }
}
