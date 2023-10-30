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
    public class GUIDevControl : IGUIFormat
    {
        public GUIPopupDisplay Display { get; set; }
        public string buttonMessage;

        public void Setup(GUIPopupDisplay display)
        {
            Display = display;
        }
        public void OnOpen()
        {
            Display.Window.width = 340;
        }


        public void RunGUI(int ID)
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

        public void DelayedUpdate()
        {
            Display.Window.width = 340;
        }
        public void FastUpdate()
        {
            this.UpdateTransparency(0.2f);
        }
        public void OnRemoval() { }
    }
}
