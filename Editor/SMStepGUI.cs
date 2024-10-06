using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Sub_Missions.Steps;
using TerraTechETCUtil;

namespace Sub_Missions.Editor
{
    public class SMStepGUI
    {
        private readonly string name;
        private readonly Dictionary<ESMSFields, SMSFieldGUI> elements;
        private static SMSFieldGUI SuccessProgressID_Cache = SMStepEditorGUI.GetField(null,
            ESMSFields.SuccessProgressID, "Progress ID on Success");
        internal SMStepGUI(SMissionStep step, Dictionary<ESMSFields, SMSFieldGUI> elements)
        {
            name = step.SMission.StepType.ToString();
            this.elements = elements;
        }
        private string setCache = "";
        internal void DoGUI(SubMissionStep context)
        {
            GUILayout.Label(name);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Progress ID: ");
            if (elements.ContainsKey(ESMSFields.SpawnOnly))
                GUILayout.Label("Start Only");
            else if (context.ProgressID == SubMission.alwaysRunValue)
                GUILayout.Label("Always");
            else
                GUILayout.Label(context.ProgressID.ToString());
            GUILayout.FlexibleSpace();
            if (SMAutoFill.OneWayButton("Do Always", SubMission.alwaysRunValue, ref context.ProgressID))
                setCache = context.ProgressID.ToString();
            if (!int.TryParse(setCache, out int val) || val == context.ProgressID)
                setCache = context.ProgressID.ToString();
            string set = GUILayout.TextField(setCache, 32, AltUI.TextfieldBlackAdjusted, GUILayout.Width(160));
            if (long.TryParse(set, out long val2))
            {
                GUILayout.Label("<color=green>O</color>", GUILayout.Width(25));
                if (set != setCache)
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                    context.ProgressID = SMAutoFill.ClampInt(val2);
                }
            }
            else
            {
                GUILayout.Label("<color=red>X</color>", GUILayout.Width(25));
            }

            setCache = set;
            GUILayout.EndHorizontal();
            foreach (var item in elements)
            {
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                item.Value.DoDisplay(context);
                GUILayout.EndHorizontal();
            }
            if (!elements.ContainsKey(ESMSFields.SuccessProgressID) &&
                context.VaribleType == EVaribleType.DoSuccessID)
            {
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                SuccessProgressID_Cache.DoDisplay(context);
                GUILayout.EndHorizontal();
            }
        }

        internal void Update(SubMissionStep context)
        {
            foreach (var item in elements)
            {
                item.Value.UpdateScene(context);
            }
        }

        internal void RefreshFields(SubMissionStep step)
        {
            setCache = step.ProgressID.ToString();
            foreach (var item in elements)
            {
                item.Value.RefreshGUI(step);
            }
        }
    }
}
