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
        public bool DestroyOnClose = false;


        public void Setup(GUIPopupDisplay display, string message, bool removeOnClose)
        {
            Display = display;
            Message = message;
            DestroyOnClose = removeOnClose;
        }

        public void RunGUI(int ID)
        {
            if (!WindowManager.SetupAltWins)
            {
                WindowManager.styleLargeFont = new GUIStyle(GUI.skin.label);
                WindowManager.styleLargeFont.fontSize = 16;
                WindowManager.styleHugeFont = new GUIStyle(GUI.skin.button);
                WindowManager.styleHugeFont.fontSize = 20;
                WindowManager.SetupAltWins = true;
            }

            GUI.Label(new Rect(20, 40, Display.Window.width - 40, Display.Window.height - 80), Message, WindowManager.styleLargeFont);
            if (GUI.Button(new Rect((Display.Window.width / 2) - 70, Display.Window.height - 60, 140, 40), "<b>CONTINUE</b>", WindowManager.styleHugeFont))
            {
                Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                WindowManager.SetCurrentPopup(Display);
                WindowManager.HidePopup(Display);
                if (DestroyOnClose)
                    WindowManager.RemovePopup(Display);
            }
            GUI.DragWindow();
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
