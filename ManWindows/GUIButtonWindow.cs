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
                WindowManager.styleLargeFont = new GUIStyle(GUI.skin.label);
                WindowManager.styleLargeFont.fontSize = 16;
                WindowManager.styleHugeFont = new GUIStyle(GUI.skin.button);
                WindowManager.styleHugeFont.fontSize = 20;
                WindowManager.styleGinormusFont = new GUIStyle(GUI.skin.button);
                WindowManager.styleGinormusFont.fontSize = 38;
                WindowManager.SetupAltWins = true;
            }

            if (GUI.Button(new Rect(15, 15, Display.Window.width - 30, Display.Window.height - 30), buttonMessage, WindowManager.styleHugeFont))
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
