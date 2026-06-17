using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TerraTechETCUtil;

namespace Sub_Missions.ManWindows
{
    public class GUIMessage : GUIMiniMenu<GUIMessage>
    {
        public string Message;
        private Vector2 scroll = Vector2.zero;


        public override void Setup(GUIDisplayStats stats)
        {
            GUIDisplayStatsLegacy stats2 = (GUIDisplayStatsLegacy)stats;
            Message = (string)stats2.val1;
        }
        public override void OnOpen()
        {
        }


        public override void RunGUI(int ID)
        {
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.Label(Message, WindowManager.styleLabelLargerFont, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();

            if (GUILayout.Button("<b>CONTINUE</b>", WindowManager.styleButtonHugeFont, GUILayout.Width(140) ,GUILayout.Height(40)))
            {
                Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                WindowManager.HidePopup(Display);
                WindowManager.RemovePopup(Display);
            }

            GUI.DragWindow();
            WindowManager.KeepWithinScreenBoundsNonStrict(Display);
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
