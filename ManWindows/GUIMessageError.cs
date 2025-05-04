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
    public class GUIMessageError : GUIMiniMenu<GUIMessageError>
    {
        private Vector2 scroll = Vector2.zero;
        private SMUtil.ErrorQueue errors;


        public override void Setup(GUIDisplayStats stats)
        {
            GUIDisplayStatsLegacy stats2 = (GUIDisplayStatsLegacy)stats;
            errors = (SMUtil.ErrorQueue)stats2.val1;
        }
        public override void OnOpen()
        {
        }


        public override void RunGUI(int ID)
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
            if (GUILayout.Button("<b>EXIT</b>", AltUI.ButtonBlueLarge, GUILayout.Width(200), GUILayout.Height(46)))
            {
                Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Open);
                WindowManager.HidePopup(Display);
                WindowManager.RemovePopup(Display);
            }
            if (GUILayout.Button("<b>CLEAR</b>", AltUI.ButtonGreyLarge, GUILayout.Width(200), GUILayout.Height(46)))
            {
                Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Open);
                SMUtil.ClearErrors();
            }
            if (KickStart.Debugger)
            {
                if (GUILayout.Button("<b>Hold Logging</b>", AltUI.ButtonOrangeLargeActive, GUILayout.Width(200), GUILayout.Height(46)))
                {
                    Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Back);
                    KickStart.Debugger = false;
                }
            }
            else
            {
                if (GUILayout.Button("<b>Resume Logging</b>", AltUI.ButtonOrangeLarge, GUILayout.Width(200), GUILayout.Height(46)))
                {
                    Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                    KickStart.Debugger = true;
                }
            }
            if (ManSubMissions.inst.enabled)
            {
                if (GUILayout.Button("<b>STOP UPDATES</b>", AltUI.ButtonOrangeLargeActive, GUILayout.Width(200), GUILayout.Height(46)))
                {
                    Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.AIIdle);
                    ManSubMissions.inst.enabled = false;
                }
            }
            else
            {
                if (GUILayout.Button("<b>RUN UPDATES</b>", AltUI.ButtonOrangeLarge, GUILayout.Width(200), GUILayout.Height(46)))
                {
                    Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.AIGuard);
                    ManSubMissions.inst.enabled = true;
                }
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow();
            WindowManager.KeepWithinScreenBoundsNonStrict(Display);
        }

        public override void DelayedUpdate()
        {
        }
        public override void FastUpdate()
        {
            this.UpdateTransparency(0.4f);
        }
        public override void OnRemoval() { }
    }
}
