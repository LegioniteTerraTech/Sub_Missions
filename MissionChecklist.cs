using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

namespace Sub_Missions
{
    public class MissionChecklist
    {
        [JsonIgnore]
        internal SubMission mission { get; set; } = null;

        public string ListArticle = "unset";
        public VarType ValueType = VarType.Bool;
        public int BoolToEnable = -1; // pop up when the global bool for this is true - -1 to enable at all times
        public int GlobalIndex = 0;
        public int GlobalIndex2 = 0;

        [JsonIgnore]
        private float tickdown = 2;
        [JsonIgnore]
        private bool setToRemove = false;

        public bool TestForCompleted()
        {
            bool triggerCountdownRemoval = false;
            try
            {
                switch (ValueType)
                {
                    case VarType.Bool:
                        if (mission.VarTrueFalseActive[GlobalIndex])
                            triggerCountdownRemoval = true;
                        break;
                    case VarType.IntOverInt:
                        if (mission.VarIntsActive[GlobalIndex] >= mission.VarIntsActive[GlobalIndex2])
                            triggerCountdownRemoval = true;
                        break;
                }
                if (triggerCountdownRemoval)
                {
                    if (!setToRemove)
                    {
                        Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
                        setToRemove = true;
                    }
                    if (tickdown <= 0)
                    {
                        mission.CheckList.Remove(this);
                    }
                    tickdown -= Time.deltaTime;
                }
            }
            catch (IndexOutOfRangeException e)
            {
                SMUtil.Error(false, "Mission (Checklist) ~" + mission.Name, KickStart.ModID + ": MissionChecklist - Error detected at " + ListArticle + ", mission " + mission.Name + ". Check your syntax!");

                switch (ValueType)
                {
                    case VarType.Bool:
                        SMUtil.Error(true, "Mission (Checklist) ~" + mission.Name, KickStart.ModID + ": Make sure the GlobalIndex is set properly and it's index exists in VarTrueFalse!");
                        break;
                    case VarType.IntOverInt:
                        SMUtil.Error(true, "Mission (Checklist) ~" + mission.Name, KickStart.ModID + ": Make sure both GlobalIndexes are set properly and exist in VarInts!");
                        break;
                    default:
                        throw new MandatoryException(e);
                }
            }
            catch (Exception e)
            {
                throw new MandatoryException(e);
                //SMUtil.Assert(true, KickStart.ModID + ": Internal issue!  Contact Legionite!", e);
            }
            return triggerCountdownRemoval;
        }
        public bool GetStatusGUI()
        {
            if (mission == null)
            {
                //throw new NullReferenceException("mission is null somehow - this should not be possible");
                //GUILayout.Label("<b>!! MISSION NULL !!</b>");
                GUILayout.Label("<b>Too Far From \nMission</b>");
                GUILayout.FlexibleSpace();
                return false;
            }
            else
            {
                if (BoolToEnable != -1 && BoolToEnable < mission.VarTrueFalseActive.Count)
                {
                    if (!mission.VarTrueFalseActive[BoolToEnable])
                    {
                        return false;
                    }
                }
            }
            GUILayout.BeginHorizontal(GUILayout.Height(38));
            GUILayout.Label(ListArticle == null ? "" : ListArticle);
            GUILayout.FlexibleSpace();
            GUILayout.Label("|");
            ShowEntryData();
            GUILayout.EndHorizontal();
            return true;
        }

        public void ShowEntryData()
        {
            switch (ValueType)
            {
                case VarType.Bool:
                    if (GlobalIndex == -1 || GlobalIndex >= mission.VarTrueFalseActive.Count)
                    {
                        GUILayout.Label("<b>ER</b>");
                    }
                    else
                    {
                        if (mission.VarTrueFalseActive[GlobalIndex])
                            GUILayout.Label("<b>✓</b>");
                        else
                            GUILayout.Label("<b>!</b>");
                    }
                    break;
                case VarType.IntOverInt:
                    if (GlobalIndex == -1 || GlobalIndex >= mission.VarIntsActive.Count ||
                        GlobalIndex2 == -1 || GlobalIndex2 >= mission.VarIntsActive.Count)
                    {
                        GUILayout.Label("<b>ER</b>");
                    }
                    else
                    {
                        if (mission.VarIntsActive[GlobalIndex] >= mission.VarIntsActive[GlobalIndex2])
                            GUILayout.Label("<b>✓</b>");
                        else
                        {
                            GUILayout.Label(mission.VarIntsActive[GlobalIndex].ToString());
                            GUILayout.Label(" / ");
                            GUILayout.Label(mission.VarIntsActive[GlobalIndex2].ToString());
                        }
                    }
                    break;
                default:
                    GUILayout.Label("<b>O</b>");
                    break;

            }
        }


        public bool GetNumber(out int output)
        {
            switch (ValueType)
            {
                case VarType.Bool:
                    output = 0;
                    return false;
                case VarType.IntOverInt:
                    if (GlobalIndex < 0 || GlobalIndex >= mission.VarIntsActive.Count)
                        throw new Exception("Checklist entry set to IntOverInt but GlobalIndex is set to [" +
                            GlobalIndex + "] which is outside the range of present Ints and not the -1" +
                            " always active value: [0 -> " + (mission.VarIntsActive.Count - 1).ToString() + "]");
                    output = mission.VarIntsActive[GlobalIndex];
                    return true;
                default:
                    output = 0;
                    return false;
            }
        }
    }
    public enum VarType
    {
        Unset,
        Bool,           // Checkmark for task true
        IntOverInt,     // Checkmark for task Above GlobalIndex2's value
        //Float,        // 
        //FloatOverFloat,      // 
        //Position,       // Vector3
    }
}
