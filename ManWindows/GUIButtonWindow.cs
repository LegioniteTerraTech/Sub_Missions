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

        public void OnOpen()
        {
        }

        public void RunGUI(int ID)
        {

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
