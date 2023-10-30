using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TerraTechETCUtil;

namespace Sub_Missions.ManWindows
{
    public static class IGUIFormatExtenions
    {
        public static void UpdateTransparency(this IGUIFormat inst, float lowerLimit)
        {
            if (UIHelpersExt.MouseIsOverSubMenu(inst.Display.Window))
                inst.Display.alpha = Mathf.Min(0.95f, inst.Display.alpha + Time.deltaTime * 1.85f);
            else
                inst.Display.alpha = Mathf.Max(lowerLimit, inst.Display.alpha - Time.deltaTime * 0.475f);
        }
    }

    public interface IGUIFormat
    {
        GUIPopupDisplay Display { get; set; }

        void RunGUI(int ID);

        void DelayedUpdate();
        void FastUpdate();
        void OnRemoval();

        void OnOpen();
    }
}
