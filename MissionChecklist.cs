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
        internal CustomSubMission mission;

        public string ListArticle = "unset";
        public VarType ValueType = VarType.Bool;
        public int BoolToEnable = -1; // pop up when the global bool for this is true - -1 to enable at all times
        public int GlobalIndex = 0;
        public int GlobalIndex2 = 0;

        [JsonIgnore]
        public StringBuilder builder = new StringBuilder();

        [JsonIgnore]
        private int tickdown = 2;
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
                        if (mission.VarTrueFalse[GlobalIndex])
                            triggerCountdownRemoval = true;
                        break;
                    case VarType.IntOverInt:
                        if (mission.VarInts[GlobalIndex] > mission.VarInts[GlobalIndex2])
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
                    tickdown--;
                }
            }
            catch 
            { 
                Debug.Log("SubMissions: MissionChecklist - Error detected at " + ListArticle + ", mission " + mission.Name + ". Check your syntax!");

                switch (ValueType)
                {
                    case VarType.Bool:
                        Debug.Log("SubMissions: Make sure the GlobalIndex is set properly and it's index exists in VarTrueFalse!");
                        break;
                    case VarType.IntOverInt:
                        Debug.Log("SubMissions: Make sure both GlobalIndexes are set properly and exist in VarInts!");
                        break;
                    case VarType.Unset:
                        Debug.Log("SubMissions: Internal issue!  Contact Legionite!");
                        break;
                }
            }
            return triggerCountdownRemoval;
        }
        public bool GetStatus(out string output)
        {
            if (BoolToEnable > 0)
            {
                if (!mission.VarTrueFalse[BoolToEnable])
                {
                    output = "";
                    return false;
                }
            }
            builder.Clear();
            builder.Append(ListArticle + "| ");
            switch (ValueType)
            {
                case VarType.Bool:
                    if (mission.VarTrueFalse[GlobalIndex])
                        builder.Append("<b>✓</b>");
                    else
                        builder.Append("<b>!</b>");
                    break;
                case VarType.IntOverInt:
                    if (mission.VarInts[GlobalIndex] > mission.VarInts[GlobalIndex2])
                        builder.Append("<b>✓</b>");
                    else
                        builder.Append("<b>"+ mission.VarInts[GlobalIndex] + "/" + mission.VarInts[GlobalIndex2] + "</b>");
                    break;
                default:
                    builder.Append("<b>!</b>");
                    break;

            }
            output = builder.ToString();
            return true;
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
