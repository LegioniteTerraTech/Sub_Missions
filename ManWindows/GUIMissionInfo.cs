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
        public string CurrentMissionObjectives = "MissionTasks";

        private int maxObjectivesToDisplay = 3;

        public StringBuilder builder = new StringBuilder();


        public void Setup(GUIPopupDisplay display)
        {
            Display = display;
            ManSubMissions.SideUI = this.Display;
        }

        public void RunGUI(int ID)
        {
            GUI.Label(new Rect(10, 25, Display.Window.width - 20, Display.Window.height - 40), CurrentMissionObjectives);
        
            GUI.DragWindow();
            WindowManager.KeepWithinScreenBoundsNonStrict(Display);
        }

        public void DelayedUpdate()
        {
            BuildDesc();
        }
        public void FastUpdate()
        {
        }
        public void BuildDesc()
        {
            try
            {
                if (currentMission != null)
                {
                    CurrentMissionName = "<b>" + currentMission.Name + "</b>";
                    Display.context = CurrentMissionName;
                    try
                    {
                        int currentDisplayedNum = 0;
                        int displayNum = currentMission.CheckList.Count;
                        for (int step = 0; step < displayNum && currentDisplayedNum < maxObjectivesToDisplay; step++)
                        {
                            if (currentMission.CheckList.ElementAt(step).GetStatus(out string output))
                            {
                                builder.Append(output + "\n");
                                currentDisplayedNum++;
                            }
                        }
                    }
                    catch { };
                    CurrentMissionObjectives = builder.ToString();
                    builder.Clear();
                    return;
                }
            }
            catch { }
            CurrentMissionName = "<b>Not Selected</b>";
            Display.context = CurrentMissionName;
            CurrentMissionObjectives = "New Sub Missions\nAvailable";
        }
        public void OnRemoval() { }
    }
}
