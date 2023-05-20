using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sub_Missions.Steps;

namespace Sub_Missions.ManWindows
{
    public class GUIDualButton : IGUIFormat
    {
        public GUIPopupDisplay Display { get; set; }
        public string buttonMessage;
        public StepActOptions options;
        public bool DestroyOnPress = false;
        public string InvokeAction;
        public bool ConnectedToMission = false;


        public void Setup(GUIPopupDisplay display, string buttonLabel, bool removeOnPress, object ActionName)
        {
            Display = display;
            buttonMessage = buttonLabel;
            DestroyOnPress = removeOnPress;

            try
            {
                options = (StepActOptions)ActionName;
                ConnectedToMission = true;
                Debug_SMissions.Log("SubMissions: Hooked up a GUIDualButton to StepActOptions of mission " + options.Mission.Name);
            }
            catch
            {
                InvokeAction = (string)ActionName;
            }
        }
        public void OnOpen()
        {
        }


        public void RunGUI(int ID)
        {
            if (GUI.Button(new Rect(15, 30, (Display.Window.width / 2) - 30, Display.Window.height - 45), "<b>" + buttonMessage + "</b>", ConnectedToMission ? WindowManager.styleGinormusFont : WindowManager.styleHugeFont))
            {
                Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Open);
                if (ConnectedToMission)
                { 
                    options.OptionSelect();
                }
                else
                    ButtonAct.Invoke(InvokeAction);// has frame delay

                WindowManager.HidePopup(Display);
                if (DestroyOnPress)
                {
                    WindowManager.RemovePopup(Display);
                }
            }
            if (GUI.Button(new Rect((Display.Window.width / 2) + 15, 30, (Display.Window.width / 2) - 30, Display.Window.height - 45), "<b>No</b>", ConnectedToMission ? WindowManager.styleGinormusFont : WindowManager.styleHugeFont))
            {
                Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Close);

                WindowManager.HidePopup(Display);
                if (DestroyOnPress)
                {
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
