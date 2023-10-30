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
        private Vector2 scroll = Vector2.zero;


        public void Setup(GUIPopupDisplay display, string message)
        {
            Display = display;
            Message = message;
        }
        public void OnOpen()
        {
        }


        public void RunGUI(int ID)
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

        public void DelayedUpdate()
        {
        }
        public void FastUpdate()
        {
        }
        public void OnRemoval() { }
    }
}
