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
    public class GUIMessageError : IGUIFormat
    {
        public GUIPopupDisplay Display { get; set; }
        private Vector2 scroll = Vector2.zero;
        private SMUtil.ErrorQueue errors;


        internal void Setup(GUIPopupDisplay display, SMUtil.ErrorQueue elements)
        {
            Display = display;
            errors = elements;
        }
        public void OnOpen()
        {
        }


        public void RunGUI(int ID)
        {
            if (errors.Count > 0)
            {
                scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                errors.DispenseGUI();
                GUILayout.EndScrollView();
            }
            else
                GUILayout.Label("No Errors", WindowManager.styleButtonGinormusFont);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<b>CONTINUE</b>", AltUI.ButtonBlueLarge, GUILayout.Width(200), GUILayout.Height(46)))
            {
                Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                WindowManager.HidePopup(Display);
                WindowManager.RemovePopup(Display);
            }
            if (GUILayout.Button("<b>CLEAR</b>", AltUI.ButtonGreyLarge, GUILayout.Width(200), GUILayout.Height(46)))
            {
                Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                SMUtil.ClearErrors();
            }
            if (KickStart.Debugger)
            {
                if (GUILayout.Button("<b>PAUSE</b>", AltUI.ButtonOrangeLargeActive, GUILayout.Width(200), GUILayout.Height(46)))
                {
                    Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.AIIdle);
                    KickStart.Debugger = false;
                }
            }
            else
            {
                if (GUILayout.Button("<b>RESUME</b>", AltUI.ButtonOrangeLarge, GUILayout.Width(200), GUILayout.Height(46)))
                {
                    Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.AIFollow);
                    KickStart.Debugger = true;
                }
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow();
            WindowManager.KeepWithinScreenBoundsNonStrict(Display);
        }

        public void DelayedUpdate()
        {
        }
        public void FastUpdate()
        {
            this.UpdateTransparency(0.4f);
        }
        public void OnRemoval() { }
    }
}
