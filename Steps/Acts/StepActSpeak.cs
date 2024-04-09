using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sub_Missions.ManWindows;
using TAC_AI.AI.Enemy;
using static Circuits;

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
        public override void InitGUI()
        {
            AddField(ESMSFields.VaribleType, "Condition Mode");
            AddField(ESMSFields.VaribleCheckNum, "Conditional Constant");
            AddField(ESMSFields.SetMissionVarIndex2, "Output Finished");
            AddField(ESMSFields.InputNum, "Speak Speed ([-] - dual display)");
            AddField(ESMSFields.InputString, "Speaker Name");
            AddField(ESMSFields.InputStringAux_large, "Speak");
        }

        public override void OnInit()
        {
            if (!SMission.InputStringAux.NullOrEmpty())
            {
                ExpectedMessageDisplayTime = ((float)SMission.InputStringAux.Length / 5) + 5;
                Debug_SMissions.Log(KickStart.ModID + ": StepActSpeak - ExpectedMessageDisplayTime = " +
                    ExpectedMessageDisplayTime.ToString() + "\n" + SMission.InputStringAux);
            }
            MessageDisplayTime = 0;
            float val = SMission.Position.z - Mission.ScenePosition.z;
            if (val < 1)
            {   // Use Vanilla UI
                Debug_SMissions.Log(KickStart.ModID + ": StepActSpeak - use vanilla UI");
                bool OtherSide = SMission.InputNum < 0;
                oneString[0] = SMission.InputStringAux;
                winClosed = false;
                bool ScrollEnder = SMission.VaribleType == EVaribleType.None ||
                    SMission.VaribleType == EVaribleType.DoSuccessID;
                messageInst = new ManOnScreenMessages.OnScreenMessage(oneString, 
                    ManOnScreenMessages.MessagePriority.Medium, !ScrollEnder,
                    SMission.InputString, SMission.Mission.Tree.GetSpeaker(SMission.InputString, OtherSide),
                     OtherSide ? ManOnScreenMessages.Side.Right : ManOnScreenMessages.Side.Left);
            }
        }

        public override void OnDeInit()
        {
            winClosed = true;
            if (messageInst != null)
                ManOnScreenMessages.inst.RemoveMessage(messageInst, true);
            MessageUp = false;
            messageInst = null;
        }
        public void FinishedAndHide()
        {
            Debug_SMissions.Log(KickStart.ModID + ": StepActSpeak - FinishedAndHide()");
            ManOnScreenMessages.inst.RemoveMessage(messageInst);
            winClosed = true;
            MessageUp = false;
        }

        private string[] oneString = new string[1];
        private ManOnScreenMessages.OnScreenMessage messageInst;
        private bool MessageUp = false;
        private float ExpectedMessageDisplayTime = 5;
        private float MessageDisplayTime = 0;
        public override void FirstSetup()
        {
            MessageUp = false;
            /*
            float val = SMission.Position.z - Mission.ScenePosition.z;
            if (val >= 1)
            {   // Use External UI
                Debug_SMissions.Log(KickStart.ModID + ": StepActSpeak - use external UI");
                WindowManager.AddPopupMessageScroll(SMission.InputString, SMission.InputStringAux, Mathf.Max(Mathf.Abs(SMission.InputNum), 0.02f), Mathf.Sign(SMission.InputNum) == -1, this, windowOverride: WindowManager.SmallWideWindow);
                SMission.AssignedWindow = WindowManager.GetCurrentPopup();
                SMission.Mission.Tree.GetSpeakerTex(SMission.InputString);
                //TryFetchImage();
                if (Mathf.Approximately(SMission.Position.z - Mission.ScenePosition.z, 0))
                    WindowManager.ChangePopupPositioning(new Vector2(0.5f, 1), SMission.AssignedWindow);
                else
                    WindowManager.ChangePopupPositioning(new Vector2(Mathf.Clamp(SMission.Position.x - Mission.ScenePosition.x, -1, 1), Mathf.Clamp(SMission.Position.y - Mission.ScenePosition.y, -1, 1)), SMission.AssignedWindow);
            }*/
        }
        public override void Trigger()
        {   // run the text box things
            if (MessageUp && SMission.AssignedWindow == null && MessageDisplayTime > ExpectedMessageDisplayTime)
            {
                FinishedAndHide();
            }
            if (winClosed)
            {
                //Debug_SMissions.Log(KickStart.ModID + ": StepActSpeak - Concluding Global2");
                SMUtil.ConcludeGlobal2(ref SMission);
                return;
            }
            try
            {
                if (SMUtil.BoolOut(ref SMission))
                {   // start speaking
                    MessageDisplayTime += SMission.Mission.DeltaTime * 4;
                    //Debug_SMissions.Log(KickStart.ModID + ": StepActSpeak - MessageDisplayTime = " + MessageDisplayTime.ToString());
                    if (SMission.AssignedWindow == null)
                        if (!MessageUp)
                        {
                            ManOnScreenMessages.inst.AddMessage(messageInst);
                            MessageUp = true;
                            winClosed = false;
                        }
                    //else
                    //    WindowManager.ShowPopup(SMission.AssignedWindow);
                }
                else
                {   // Stop the speaking when the bool is false
                    if (SMission.AssignedWindow == null)
                        if (MessageUp)
                        {
                            ManOnScreenMessages.inst.RemoveMessage(messageInst,true);
                            MessageDisplayTime = 0;
                            MessageUp = false;
                        }
                    //else
                    //    WindowManager.HidePopup(SMission.AssignedWindow);
                }
                TryFetchImage();
            }
            catch (Exception e)
            {
                
                if (SMission == null)
                    Debug_SMissions.Log("error SMission null " + e);
                else if (SMission.AssignedWindow == null)
                    Debug_SMissions.Log("error SMission.AssignedWindow null " + e);
                else
                    Debug_SMissions.Log("error " + e);
                
                SMUtil.ConcludeGlobal2(ref SMission);
            }
        }
        bool winClosed = false;
        internal void WindowHasClosed()
        {
            Debug_SMissions.Log(KickStart.ModID + ": StepActSpeak - WindowHasClosed");
            winClosed = true;
        }
        private Texture2D TryFetchImage()
        {
            try
            {
                GUIScrollMessage scroll = (GUIScrollMessage)SMission.AssignedWindow.GUIFormat;
                if (!scroll.Image)
                {
                    if (SMission.Mission.Tree.MissionTextures.TryGetValue(SMission.InputString.Replace(".png", "") + ".png", out Texture val))
                    {
                        scroll.Image = (Texture2D)val;
                        return (Texture2D)val;
                    }
                }
            }
            catch { }
            return null;
        }
    }
}
