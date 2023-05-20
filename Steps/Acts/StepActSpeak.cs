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
        public override string GetDocumentation()
        {
            return
                "{  // Displays a mission NPC-like chatbox with text scrolling." +
                  "\n  \"StepType\": \"ActSpeak\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  \"SuccessProgressID\": 0,      // The ProgressID the mission will be pushed to if \"VaribleType\" is set to \"DoProgressID\"" +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"True\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that is changed on chat ending." +
                  "\n  // Input Parameters" +
                  "\n  \"InputNum\": 0.0,             // How fast the speaker should speak.  Add the minus sign to change it to a dual display." +
                  "\n  \"InputString\": \"TechName\",   // The name of the Speaker to display.  If there's a respectively named image, this will show that respective image." +
                  "\n  \"InputStringAux\": \"Hello World\",   // What the speaker will say." +
                "\n},";
        }

        public override void OnInit() { }

        public override void OnDeInit()
        {
        }
        public override void FirstSetup()
        {
            WindowManager.AddPopupMessageScroll(SMission.InputString, SMission.InputStringAux, Mathf.Max(Mathf.Abs(SMission.InputNum), 0.02f), Mathf.Sign(SMission.InputNum) == -1, this, windowOverride: WindowManager.SmallWideWindow);
            SMission.AssignedWindow = WindowManager.GetCurrentPopup();
            TryFetchImage();
            if (Mathf.Approximately(SMission.Position.z - Mission.ScenePosition.z, 0))
                WindowManager.ChangePopupPositioning(new Vector2(0.5f, 1), SMission.AssignedWindow);
            else
                WindowManager.ChangePopupPositioning(new Vector2(Mathf.Clamp(SMission.Position.x - Mission.ScenePosition.x, -1, 1), Mathf.Clamp(SMission.Position.y - Mission.ScenePosition.y, -1, 1)), SMission.AssignedWindow);
        }
        public override void Trigger()
        {   // run the text box things
            if (winClosed)
            {
                Debug_SMissions.Log("SubMissions: StepActSpeak - Concluding Global2");
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
                TryFetchImage();
            }
            catch (Exception e)
            {
                Debug_SMissions.Log("error " + e);
                SMUtil.ConcludeGlobal2(ref SMission);
            }
        }
        bool winClosed = false;
        internal void WindowHasClosed()
        {
            Debug_SMissions.Log("SubMissions: StepActSpeak - WindowHasClosed");
            winClosed = true;
        }
        private void TryFetchImage()
        {
            try
            {
                GUIScrollMessage scroll = (GUIScrollMessage)SMission.AssignedWindow.GUIFormat;
                if (!scroll.Image)
                {
                    if (SMission.Mission.Tree.MissionTextures.TryGetValue((SMission.InputString + ".png").GetHashCode(), out Texture val))
                    {
                        scroll.Image = (Texture2D)val;
                    }
                }
            }
            catch { }
        }
    }
}
