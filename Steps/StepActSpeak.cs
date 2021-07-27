using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sub_Missions.ManWindows;

namespace Sub_Missions.Steps
{
    public class StepActSpeak : SMissionStep
    {
        public override void TrySetup()
        {
            WindowManager.AddPopupMessageScroll(SMission.InputString, SMission.InputStringAux, windowOverride: WindowManager.WideWindow);
            SMission.AssignedWindow = WindowManager.GetCurrentPopup();
            WindowManager.ChangePopupPositioning(new Vector2(0.5f, 1), SMission.AssignedWindow);
        }
        public override void Trigger()
        {   // run the text box things
            if (SMission.AssignedWindow == null)
            {
                SMUtil.ConcludeGlobal2(ref SMission);
                return;
            }
            try
            {
                if (SMUtil.BoolOut(ref SMission))
                {   // start speaking
                    WindowManager.ShowPopup(SMission.AssignedWindow);
                }
                else
                {   // Stop the speaking when the bool is false
                    WindowManager.HidePopup(SMission.AssignedWindow);
                }
            }
            catch
            {
                SMUtil.ConcludeGlobal2(ref SMission);
            }
        }
    }
}
