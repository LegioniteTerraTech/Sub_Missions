using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sub_Missions.ManWindows;

namespace Sub_Missions.Steps
{
    public class StepActOptions : SMissionStep
    {
        public override void TrySetup()
        {
            WindowManager.AddPopupButtonDual(SMission.InputString, SMission.InputStringAux, true, this, windowOverride: WindowManager.WideWindow);
            SMission.AssignedWindow = WindowManager.GetCurrentPopup();
            WindowManager.ChangePopupPositioning(new Vector2(0.5f, 0.5f), SMission.AssignedWindow);
        }

        public override void Trigger()
        {   // run the text box things
            if (SMission.SavedInt == 9999)
            {
                SMUtil.ConcludeGlobal3(ref SMission);
                return;
            }
            try
            {
                if (SMUtil.BoolOut(ref SMission))
                {   // Start speaking
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

        public void OptionSelect()
        {
            SMission.SavedInt = 9999;
        }
    }
}
