using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sub_Missions.ManWindows;

namespace Sub_Missions
{
    internal class ButtonAct : MonoBehaviour
    {
        public static ButtonAct inst;
        internal static void Initiate()
        {
            inst = Instantiate(new GameObject("ButtonMan")).AddComponent<ButtonAct>();
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

        public void AcceptSMission()
        {
            if (ManSubMissions.SelectedAnon == null)
            {
                Debug.Log("SubMissions: GUIMissionsList - tried to fetch NULL ANON MISSION");
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
                Debug.Log("SubMissions: GUIMissionsList - tried to fetch NULL ACTIVE MISSION");
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
                Debug.Log("SubMissions: GUIMissionsList - tried to fetch NULL ACTIVE MISSION");
                return;
            }
            else
            {
                ManSubMissions.inst.CancelMission();
            }
        }
    }
}
