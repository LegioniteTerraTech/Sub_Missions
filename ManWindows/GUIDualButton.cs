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
    public class GUIDualButton : GUIMiniMenu<GUIDualButton>
    {
        public string buttonMessage;
        public StepActOptions options;
        public bool DestroyOnPress = false;
        public string InvokeAction;
        public bool ConnectedToMission = false;


        public override void Setup(GUIDisplayStats stats)
        {
            GUIDisplayStatsLegacy stats2 = (GUIDisplayStatsLegacy)stats;
            buttonMessage = (string)stats2.val1;
            DestroyOnPress = (bool)stats2.val2;

            try
            {
                options = (StepActOptions)stats2.val3;
                ConnectedToMission = true;
                Debug_SMissions.Log(KickStart.ModID + ": Hooked up a GUIDualButton to StepActOptions of mission " + options.Mission.Name);
            }
            catch
            {
                InvokeAction = (string)stats2.val3;
            }
        }
        public override void OnOpen()
        {
        }


        public override void RunGUI(int ID)
        {
            if (GUI.Button(new Rect(15, 30, (Display.Window.width / 2) - 30, Display.Window.height - 45), "<b>" + buttonMessage + "</b>", ConnectedToMission ? WindowManager.styleButtonGinormusFont : WindowManager.styleButtonHugeFont))
            {
                Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Open);
                if (ConnectedToMission)
                    options.OptionSelect();
                else
                    ButtonAct.Invoke(InvokeAction);// has frame delay

                WindowManager.HidePopup(Display);
                if (DestroyOnPress)
                {
                    WindowManager.RemovePopup(Display);
                }
            }
            if (GUI.Button(new Rect((Display.Window.width / 2) + 15, 30, (Display.Window.width / 2) - 30, Display.Window.height - 45), "<b>No</b>", ConnectedToMission ? WindowManager.styleButtonGinormusFont : WindowManager.styleButtonHugeFont))
            {
                Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Close);
                if (ConnectedToMission)
                    options.NoSelect();

                WindowManager.HidePopup(Display);
                if (DestroyOnPress)
                {
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
