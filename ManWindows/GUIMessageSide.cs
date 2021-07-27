using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.ManWindows
{
    public class GUIMessageSide : IGUIFormat
    {
        public GUIPopupDisplay Display { get; set; }
        public static CustomSubMission currentMission { get { return ManSubMissions.Selected; } }
        public string CurrentMissionName = "Loading...";
        public string CurrentMissionObjectives = "MissionTasks";

        private int maxObjectivesToDisplay = 3;

        public StringBuilder builder = new StringBuilder();


        public void Setup(GUIPopupDisplay display)
        {
            Display = display;
        }

        public void RunGUI(int ID)
        {
            GUI.Label(new Rect(15, 15, Display.Window.width - 30, Display.Window.height - 30), CurrentMissionName, WindowManager.styleLargeFont);
            GUI.Label(new Rect(15, 40, Display.Window.width - 30, Display.Window.height - 60), CurrentMissionObjectives);
        
            GUI.DragWindow();
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
                CurrentMissionName = "<b>" + currentMission.Name + "</b>";
                builder.Clear();
                try
                {
                    int displayNum = maxObjectivesToDisplay;
                    if (currentMission.CheckList.Count < maxObjectivesToDisplay)
                        displayNum = currentMission.CheckList.Count;
                    for (int step = 0; step < displayNum; step++)
                    {
                        if (currentMission.CheckList.ElementAt(step).GetStatus(out string output))
                            builder.Append(output + "\n");
                        else
                            displayNum++;
                    }
                }
                catch { };
                CurrentMissionObjectives = builder.ToString();
            }
            catch 
            {
                CurrentMissionName = "<b>Not Selected</b>";
                CurrentMissionObjectives = "New Sub Missions\nAvailable";
            }
        }
        public void OnRemoval() { }
    }
}
