using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sub_Missions.Steps;

namespace Sub_Missions.ManWindows
{
    public class GUIScrollMessage : IGUIFormat
    {
        public GUIPopupDisplay Display { get; set; }
        public Texture2D Image;
        public StringBuilder MessageBuilder = new StringBuilder();
        public string Message = "ERROR 404 NOT FOUND (Init Failiure)";
        public string MessageOut = "";

        public bool DenySkip = false;
        private float scrollDelay = 0.075f;
        private int StepFadeoutDelay = 150;
        private float tracker = 0;
        private int step = -5;
        private bool bleep = false;
        private bool bleepPrev = false;
        private SMissionStep assignedStep;

        private static int StepRate = 1;


        public void Setup(GUIPopupDisplay display, string message, float scrollLerpTime, SMissionStep assignedStep)
        {
            Display = display;
            Message = message;
            scrollDelay = scrollLerpTime;
            this.assignedStep = assignedStep;
        }

        public void RunGUI(int ID)
        {
            if (Image)
            {
                GUI.DrawTexture(new Rect(15, 15, Display.Window.height - 30, Display.Window.height - 30), Image);
                GUI.Label(new Rect(20 + Display.Window.height, 30, Display.Window.width - Display.Window.height - 60, Display.Window.height - 60), MessageOut, WindowManager.styleDescLargeFont);
            }
            else
                GUI.Label(new Rect(40, 30, Display.Window.width - 80, Display.Window.height - 60), MessageOut, WindowManager.styleDescLargeFont);
            if (!DenySkip)
            {
                if (GUI.Button(new Rect(Display.Window.width - 50, Display.Window.height - 38, 40, 28), "<b>OK</b>", WindowManager.styleHugeFont))
                {
                    Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Close);
                    OnRemoval();
                    WindowManager.HidePopup(Display);
                    WindowManager.RemovePopup(Display);
                }
            }
            GUI.DragWindow();
            WindowManager.KeepWithinScreenBoundsNonStrict(Display);
        }

        private static FieldInfo clunk = typeof(ManOnScreenMessages).GetField("m_TextBlipSfxEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FMODEvent soundSteal = (FMODEvent)clunk.GetValue(Singleton.Manager<ManOnScreenMessages>.inst);

        private static FieldInfo clank = typeof(ManOnScreenMessages).GetField("m_NewLineSfxEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FMODEvent soundSteal2 = (FMODEvent)clank.GetValue(Singleton.Manager<ManOnScreenMessages>.inst);

        private bool BOLD = false;
        private bool stopped = false;
        private FMODEventInstance fInst;
        public void MessageStepBuilder()
        {
            if (!Display.isOpen || Singleton.Manager<ManPauseGame>.inst.IsPaused)
            {
                return;
            }
            if (step < 0)
            {
                step++;
                return;
            }
            else if (step == Message.Length)
            {
            }
            else if (step < Message.Length)
            {
                // the bleep was here but then it had oversaturation issues
            }
            else
            {   // End of message
                if (step > Message.Length + StepFadeoutDelay)
                {
                    OnRemoval();
                    WindowManager.HidePopup(Display);
                    WindowManager.RemovePopup(Display);
                    return;
                }

                step++;
                return;
            }
            //Debug.Log("SubMissions: stepchek");
            for (int stepSped = 0; stepSped < StepRate; stepSped++)
            {
                try
                {
                    if (!BOLD)
                    {
                        if (Message.Substring(step, 3).Equals("<b>"))
                        {
                            MessageBuilder.Append("<b>");
                            BOLD = true;
                            step += 3;
                        }
                    }
                    else
                    {
                        if (Message.Substring(step, 4).Equals("</b>"))
                        {
                            BOLD = false;
                            step += 4;
                            MessageBuilder.Append("</b>");
                        }
                    }
                }
                catch { }
                if (step + stepSped < Message.Length)
                    MessageBuilder.Append(Message.ElementAt(step + stepSped));
            }
            try
            {
                bleep = Message.ElementAt(step + StepRate) != '\n';
            }
            catch
            {
                bleep = false;
            }
            //Debug.Log("SubMissions: stepchek2");
            MessageOut = MessageBuilder.ToString() + (BOLD ? "</b>" : "");
            step += StepRate;
        }
        public void BleepCommand()
        {
            if (!Display.isOpen || Singleton.Manager<ManPauseGame>.inst.IsPaused)
            {
                try
                {
                    if (fInst.IsInited)
                    {
                        fInst.StopAndRelease();
                    }
                }
                catch { }
                return;
            }
            if (!bleep && bleepPrev)
            {
                try
                {
                    if (fInst.IsInited)
                        fInst.StopAndRelease();
                    soundSteal2.PlayOneShot();
                }
                catch { }
            }
            else if (step >= Message.Length - 1)
            {
                if (!stopped)
                {
                    try
                    {
                        if (fInst.IsInited)
                            fInst.StopAndRelease();
                        soundSteal2.PlayOneShot();
                    }
                    catch { }
                    stopped = true;
                }
            }
            else
            {
                try
                {
                    if (!fInst.IsInited)
                    {
                        fInst = soundSteal.PlayEvent();
                    }
                }
                catch { }
            }
            bleepPrev = bleep;
            //Debug.Log("Is playing noise " + fInst.CheckPlaybackState(FMOD.Studio.PLAYBACK_STATE.PLAYING));
        }

        public void FastUpdate()
        {
            StepRate = Input.GetKey(KeyCode.Space) ? 2 : 1;
            BleepCommand();
            if (tracker > scrollDelay)
            {
                MessageStepBuilder();
                tracker = 0;
            }
            tracker += Time.deltaTime;
        }
        public void DelayedUpdate()
        {
        }
        public void OnRemoval()
        {
            try
            {
                Debug.Log("SubMissions: GUIScrollMessage - OnRemoval");
                if (assignedStep != null)
                {
                    if (assignedStep is StepActSpeak SAS)
                        SAS.WindowHasClosed();
                }
                fInst.StopAndRelease();
            }
            catch { }
        }
    }
}
