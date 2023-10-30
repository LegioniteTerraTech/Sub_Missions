using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.ManWindows
{
    public class GUIMissionInfo : IGUIFormat
    {
        public GUIPopupDisplay Display { get; set; }
        public static SubMission currentMission { get { return ManSubMissions.Selected; } }
        public string CurrentMissionName = "Loading...";

        private int maxObjectivesToDisplay = 3;


        public void Setup(GUIPopupDisplay display)
        {
            Display = display;
            ManSubMissions.SideUI = Display;
            Display.alpha = 0.795f;
        }
        public void OnOpen()
        {
        }

        public void RunGUI(int ID)
        {
            BuildDescGUI();

            GUI.DragWindow();
            WindowManager.KeepWithinScreenBoundsNonStrict(Display);
        }

        public void DelayedUpdate()
        {
        }
        public void FastUpdate()
        {
        }
        public void BuildDescGUI()
        {
            try
            {
                if (currentMission != null)
                {
                    if (currentMission.Name != Display.context)
                        Display.context = currentMission.Name;

                    try
                    {
                        int currentDisplayedNum = 0;
                        int displayNum = currentMission.CheckList.Count;
                        for (int step = 0; step < displayNum && currentDisplayedNum < maxObjectivesToDisplay; step++)
                        {
                            if (currentMission.CheckList.ElementAt(step).GetStatusGUI())
                                currentDisplayedNum++;
                        }
                    }
                    catch (MandatoryException e)
                    {
                        throw e;
                    }
                    catch (Exception e)
                    {
                        throw new MandatoryException(e);
                    };
                    return;
                }
            }
            catch (MandatoryException e)
            {
                throw e;
            }
            if (CurrentMissionName != "<b>Not Selected</b>")
                Display.context = "<b>Not Selected</b>";
            GUILayout.Label("New Sub Missions\nAvailable", WindowManager.styleLabelLargerFont);
        }
        public void OnRemoval() { }
    }
}
