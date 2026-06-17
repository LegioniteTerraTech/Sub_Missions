using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TerraTechETCUtil;

namespace Sub_Missions.ManWindows
{
    public class GUIButtonWindow : GUIMiniMenu<GUIButtonWindow>
    {
        public string buttonMessage;
        public bool DestroyOnPress = false;
        public string InvokeAction;


        public override void Setup(GUIDisplayStats stats)
        {
            GUIDisplayStatsLegacy stats2 = (GUIDisplayStatsLegacy)stats;
            buttonMessage = (string)stats2.val1;
            DestroyOnPress = (bool)stats2.val2;
            InvokeAction = (string)stats2.val3;
        }

        public override void OnOpen()
        {
        }

        public override void RunGUI(int ID)
        {

            if (GUI.Button(new Rect(0, 10, Display.Window.width, Display.Window.height - 10), buttonMessage, WindowManager.styleButtonHugeFont))
            {
                Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Open);
                ButtonAct.Invoke(InvokeAction);// has frame delay

                if (DestroyOnPress)
                {
                    WindowManager.HidePopup(Display);
                    WindowManager.RemovePopup(Display);
                }
            }
            GUI.DragWindow();
            WindowManager.KeepWithinScreenBounds(Display);
        }

        public override void DelayedUpdate()
        {
        }
        public override void FastUpdate()
        {
        }
        public override void OnRemoval() { }
    }
}
