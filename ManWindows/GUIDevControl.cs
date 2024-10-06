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
    public class GUIDevControl : GUIMiniMenu<GUIDevControl>
    {
        public string buttonMessage;

        public override void Setup(GUIDisplayStats stats)
        {
            GUIDisplayStatsLegacy stats2 = (GUIDisplayStatsLegacy)stats;
            buttonMessage = (string)stats2.val1;
        }
        public override void OnOpen()
        {
            Display.Window.width = 340;
        }


        public override void RunGUI(int ID)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<b>Missions</b>", AltUI.ButtonBlueLarge))
            {
                Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Open);
                ButtonAct.Invoke("Master");// has frame delay
            }
            if (GUILayout.Button("<b>Editor</b>", AltUI.ButtonOrangeLarge))
            {
                Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Open);
                ButtonAct.Invoke("Editor");// has frame delay
            }
            GUILayout.EndHorizontal();
            GUI.DragWindow();
            WindowManager.KeepWithinScreenBounds(Display);
        }

        public override void DelayedUpdate()
        {
            Display.Window.width = 340;
        }
        public override void FastUpdate()
        {
            this.UpdateTransparency(0.2f);
        }
        public override void OnRemoval() { }
    }
}
