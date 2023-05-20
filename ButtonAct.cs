using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Sub_Missions.ManWindows;

namespace Sub_Missions
{
    internal class ButtonAct : MonoBehaviour
    {
        public static ButtonAct inst;
        internal static void Initiate()
        {
            if (inst)
                return;
            inst = Instantiate(new GameObject("ButtonMan")).AddComponent<ButtonAct>();
        }
        internal static void DeInit()
        {
            if (!inst)
                return;
        }

        private static BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        public static void Invoke(string InvokeAction)
        {
            if (InvokeAction.Contains("|"))
            {
                Type getType = KickStart.LookForType(InvokeAction.Substring(0, InvokeAction.IndexOf("|")));
                if (getType != null)
                {
                    MethodInfo MI = getType.GetMethod(InvokeAction, flags);
                    if (MI != null)
                        MI.Invoke(null, new object[0]);
                    else
                        SMUtil.Assert(false, "Button(External) - " + InvokeAction + " Method field does not exist");
                }
                else
                    SMUtil.Assert(false, "Button(External) - " + InvokeAction + " Type field does not exist");
            }
            else
            {
                MethodInfo MI = typeof(ButtonAct).GetMethod(InvokeAction, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (MI != null)
                    MI.Invoke(inst, new object[0]);
                else
                    SMUtil.Assert(false, "Button(ButtonAct) - " + InvokeAction + " Method field does not exist");
                //inst.Invoke(InvokeAction, 0);
            }
        }

        public void Nothing()
        {
        }
        public void Master()
        {
            ManSubMissions.inst.GetAllPossibleMissions();
            ManSubMissions.ToggleList();
        }
        public void UpdateMissions()
        {
            ManSubMissions.inst.GetAllPossibleMissions();
        }
        public void UpdateAllWindowsNow()
        {
            WindowManager.UpdateAllPopups();
        }
        public void SaveThisGameAnyways()
        {
            ManSubMissions.IgnoreSaveThisSession = false;
        }

        public void AcceptSMission()
        {
            if (ManSubMissions.SelectedAnon == null)
            {
                Debug_SMissions.Log("SubMissions: GUIMissionsList - tried to fetch NULL ANON MISSION");
                return;
            }
            else
            {
                ManSubMissions.inst.AcceptMission();
            }
        }
        public void RequestCancelSMission()
        {
            if (ManSubMissions.Selected == null)
            {
                Debug_SMissions.Log("SubMissions: GUIMissionsList - tried to fetch NULL ACTIVE MISSION");
                return;
            }
            else
            {
                WindowManager.AddPopupButtonDual("<b>Drop Mission?</b>", "<b>Ok</b>", true, "CancelSMission", WindowManager.TinyWideWindow);
                WindowManager.ShowPopup(new Vector2(0.5f, 0.5f));
            }
        }
        public void CancelSMission()
        {
            if (ManSubMissions.Selected == null)
            {
                Debug_SMissions.Log("SubMissions: GUIMissionsList - tried to fetch NULL ACTIVE MISSION");
                return;
            }
            else
            {
                ManSubMissions.inst.CancelMission();
                Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.MissionFailed);
            }
        }
    }
}
