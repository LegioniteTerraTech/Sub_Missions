using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.ManWindows
{
    public class GUIScrollMessage : IGUIFormat
    {
        public GUIPopupDisplay Display { get; set; }
        public StringBuilder MessageBuilder = new StringBuilder();
        public string Message;
        public string MessageOut;

        private float scrollDelay = 0.075f;
        private int StepFadeoutDelay = 250;
        private float tracker = 0;
        private int step = -5;


        public void Setup(GUIPopupDisplay display, string message, float scrollLerpTime)
        {
            Display = display;
            Message = message;
            scrollDelay = scrollLerpTime;
        }

        public void RunGUI(int ID)
        {
            GUI.Label(new Rect(200, 40, Display.Window.width - 220, Display.Window.height - 80), MessageOut, WindowManager.styleLargeFont);
            if (GUI.Button(new Rect(20, Display.Window.height - 60, 60, 40), "<b>OK</b>", WindowManager.styleHugeFont))
            {
                Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                OnRemoval();
                WindowManager.HidePopup(Display);
                WindowManager.RemovePopup(Display);
            }
            GUI.DragWindow();
        }

        private static FieldInfo clunk = typeof(ManOnScreenMessages).GetField("m_TextBlipSfxEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FMODEvent soundSteal = (FMODEvent)clunk.GetValue(Singleton.Manager<ManOnScreenMessages>.inst);

        private static FieldInfo clank = typeof(ManOnScreenMessages).GetField("m_NewLineSfxEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FMODEvent soundSteal2 = (FMODEvent)clank.GetValue(Singleton.Manager<ManOnScreenMessages>.inst);

        private FMODEventInstance fInst;
        public void MessageStepBuilder()
        {
            if (!Display.isOpen || Singleton.Manager<ManPauseGame>.inst.IsPaused)
            {
                return;
            }
            for (int internalStep = 0; internalStep < 1; internalStep++)
            {
                if (step < 0)
                {
                    step++;
                    return;
                }
                else if (step == Message.Length - 1)
                {
                }
                else if (step < Message.Length - 1)
                {
                    // the bleep was here but then it had oversaturation issues
                }
                else
                {
                    if (step > Message.Length - 1 + StepFadeoutDelay)
                    {
                        WindowManager.HidePopup(Display);
                        WindowManager.RemovePopup(Display);
                        return;
                    }

                    step++;
                    return;
                }
                //Debug.Log("SubMissions: stepchek");
                MessageBuilder.Append(Message.ElementAt(step));
                //Debug.Log("SubMissions: stepchek2");
                MessageOut = MessageBuilder.ToString();
                step++;
            }
        }
        public void BleepCommand()
        {
            if (!Display.isOpen || Singleton.Manager<ManPauseGame>.inst.IsPaused)
            {
                try
                {
                    if (fInst.CheckPlaybackState(FMOD.Studio.PLAYBACK_STATE.PLAYING))
                    {
                        fInst.StopAndRelease();
                    }
                }
                catch { }
                return;
            }
            if (step >= Message.Length - 1)
            {
                try
                {
                    fInst.StopAndRelease();
                    soundSteal2.PlayOneShot();
                }
                catch { }
            }
            else if (step < Message.Length - 1)
            {
                try
                {
                    if (!fInst.CheckPlaybackState(FMOD.Studio.PLAYBACK_STATE.PLAYING))
                    {
                        fInst = soundSteal.PlayEvent();
                    }
                }
                catch { }
            }
            //Debug.Log("Is playing noise " + fInst.CheckPlaybackState(FMOD.Studio.PLAYBACK_STATE.PLAYING));
        }

        public void FastUpdate()
        {
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
                fInst.StopAndRelease();
            }
            catch { }
        }
    }
}
