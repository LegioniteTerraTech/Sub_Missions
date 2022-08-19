using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.ManWindows
{
    public class GUIMessage : IGUIFormat
    {
        public GUIPopupDisplay Display { get; set; }
        public string Message;


        public void Setup(GUIPopupDisplay display, string message)
        {
            Display = display;
            Message = message;
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
                WindowManager.styleDescFont.fontSize = GUI.skin.label.fontSize + 2;
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

            GUI.Label(new Rect(20, 40, Display.Window.width - 40, Display.Window.height - 80), Message, WindowManager.styleLargeFont);
            if (GUI.Button(new Rect((Display.Window.width / 2) - 70, Display.Window.height - 60, 140, 40), "<b>CONTINUE</b>", WindowManager.styleHugeFont))
            {
                Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                WindowManager.HidePopup(Display);
                WindowManager.RemovePopup(Display);
            }
            GUI.DragWindow();
            WindowManager.KeepWithinScreenBoundsNonStrict(Display);
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
