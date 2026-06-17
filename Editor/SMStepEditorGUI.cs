using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sub_Missions.Steps;

namespace Sub_Missions.Editor
{
    public static class SMStepEditorGUI
    {
        private static Dictionary<SMStepType, SMStepGUI> GUILoaded = new Dictionary<SMStepType, SMStepGUI>();
        private static Dictionary<ESMSFields, SMSFieldGUI> currentList = null;
        private static Dictionary<ESMSFields, SMSFieldGUI> heldTemp = null;
        internal static void UpLevel()
        {
            if (heldTemp != null)
                throw new InvalidOperationException("Cannot nest SMStepGUIs more than once");
            heldTemp = currentList;
            currentList = new Dictionary<ESMSFields, SMSFieldGUI>();
        }
        internal static Dictionary<ESMSFields, SMSFieldGUI> DownLevel()
        {
            if (heldTemp == null)
                throw new InvalidOperationException("heldTemp was null, UpLevel() has to be called once before DownLevel()");
            currentList = heldTemp;
            var temp = heldTemp;
            heldTemp = null;
            return temp;
        }

        private static SMissionStep curStep = null;
        private static SMStepGUI GetGUI(SMissionStep step)
        {
            if (GUILoaded.TryGetValue(step.SMission.StepType, out SMStepGUI GUIOut))
            {
                if (curStep != step)
                {
                    GUIOut.RefreshFields(step.SMission);
                    curStep = step;
                }
                return GUIOut;
            }
            else
            {
                if (currentList != null)
                    throw new InvalidOperationException("SubMissionStep.ShowGUI cannot be called recursively.");
                currentList = new Dictionary<ESMSFields, SMSFieldGUI>();
                step.InitGUI();
                var newGUI = new SMStepGUI(step, currentList);
                GUILoaded.Add(step.SMission.StepType, newGUI);
                currentList = null;
                curStep = step;
                return newGUI;
            }
        }
        internal static void ShowGUI(this SubMissionStep step)
        {
            if (step.stepGenerated == null)
                throw new NullReferenceException("SubMissionStep.ShowGUI was called when stepGenerated was null!");
            try
            {
                GetGUI(step.stepGenerated).DoGUI(step);
            }
            catch (Exception e)
            {
                throw new Exception("ShowGUI(OnGUI) of step " + step.StepType + " hit exception", e);
            }
        }
        internal static void Update(this SubMissionStep step)
        {
            if (step.stepGenerated == null)
                throw new NullReferenceException("SubMissionStep.ShowGUI was called when stepGenerated was null!");
            try
            {
                GetGUI(step.stepGenerated).Update(step);
            }
            catch (Exception e)
            {
                throw new Exception("ShowGUI(Update) of step " + step.StepType + " hit exception", e);
            }
        }
        internal static void AddField(SMissionStep step, ESMSFields type, string name)
        {
            if (currentList == null)
                throw new InvalidOperationException("SMissionStep.AddField was called outside of an SMissionStep.InitGUI " +
                    "operation. This is unsupported!");
            currentList.Add(type, GetField(step, type, name));
        }
        /// <summary>
        /// DO NOT RELY ON CALLING THIS AS THIS CREATES NEW ITEMS - CLEAN OUT
        /// </summary>
        /// <param name="step"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static SMSFieldGUI GetField(SMissionStep step, ESMSFields type, string name)
        {
            switch (type)
            {
                case ESMSFields.Position:
                    return new SMSFieldPositionGUI(name, type);
                case ESMSFields.EulerAngles:
                    return new SMSFieldEulerGUI(name, type);
                case ESMSFields.Forwards:
                    return new SMSFieldFacingOptionsGUI(name, type);
                case ESMSFields.TerrainHandling:
                    return new SMSFieldTerrainOptionsGUI(name, type);
                case ESMSFields.RevProgressIDOffset:
                    return new SMSFieldBoolGUI(name, type);
                case ESMSFields.VaribleCheckNum:
                    return new SMSFieldFloatGUI(name, type);
                case ESMSFields.VaribleType:
                    return new SMSFieldVarConditionsGUI(name, type);
                case ESMSFields.InputNum:
                    return new SMSFieldFloatGUI(name, type);
                case ESMSFields.InputNum_int:
                    return new SMSFieldFloatIntGUI(name, ESMSFields.InputNum);
                case ESMSFields.InputNum_radius:
                    return new SMSFieldRadiusGUI(name, ESMSFields.InputNum);
                case ESMSFields.InputString:
                case ESMSFields.InputStringAux:
                    return new SMSFieldStringGUI(name, type);
                case ESMSFields.FolderEventList:
                    return new SMSFieldNullGUI(name, type);
                case ESMSFields.SetMissionVarIndex1:
                case ESMSFields.SetMissionVarIndex2:
                case ESMSFields.SetMissionVarIndex3:
                    return new SMSFieldVarInGUI(name, type);
                case ESMSFields.SuccessProgressID:
                    return new SMSFieldIntGUI(name, type);
                // Specials
                case ESMSFields.VaribleType_Action:
                    return new SMSFieldVarActionsGUI(name, ESMSFields.VaribleType);
                case ESMSFields.InputString_large:
                    return new SMSFieldStringLargeGUI(name, ESMSFields.InputString);
                case ESMSFields.InputStringAux_large:
                    return new SMSFieldStringLargeGUI(name, ESMSFields.InputStringAux);
                case ESMSFields.InputString_float:
                    return new SMSFieldStringFloatGUI(name, ESMSFields.InputString);
                case ESMSFields.InputStringAux_float:
                    return new SMSFieldStringFloatGUI(name, ESMSFields.InputStringAux);
                case ESMSFields.InputString_Tech:
                    return new SMSFieldTechSelectGUI(name, ESMSFields.InputString);
                case ESMSFields.InputStringAux_Tech:
                    return new SMSFieldTechSelectGUI(name, ESMSFields.InputStringAux);
                case ESMSFields.InputString_Tracked_Tech:
                    return new SMSFieldTechSelectGUI(name, ESMSFields.InputString);
                case ESMSFields.InputStringAux_Tracked_Tech:
                    return new SMSFieldTechSelectGUI(name, ESMSFields.InputStringAux);
                case ESMSFields.InputString_Corp:
                    return new SMSFieldCorpSelectGUI(name, ESMSFields.InputString);
                case ESMSFields.InputString_MM:
                    return new SMSFieldMonumentSelectGUI(name, ESMSFields.InputString);
                case ESMSFields.SpawnOnly:
                    return new SMSFieldNullGUI(name, type);
                default:
                    throw new InvalidOperationException("SMissionStep.AddField was called with ESMSFields of type [" +
                        type.ToString() + "]. This is unsupported!");
            }
        }

        internal static void AddGlobal(SMissionStep step, ESMSFields type, string name, EVaribleType varType)
        {
            if (currentList == null)
                throw new InvalidOperationException("SMissionStep.AddField was called outside of an SMissionStep.InitGUI " +
                    "operation. This is unsupported!");
            switch (type)
            {
                case ESMSFields.SetMissionVarIndex1:
                case ESMSFields.SetMissionVarIndex2:
                case ESMSFields.SetMissionVarIndex3:
                    currentList.Add(type, new SMSFieldVarInFixedGUI(name, type, varType));
                    break;
                default:
                    throw new InvalidOperationException("SMissionStep.AddGlobalFixed was called with ESMSFields of type [" +
                        type.ToString() + "]. This is unsupported!");
            }
        }
        internal static void AddOptions(SMissionStep step, ESMSFields type, string name, string[] options, Dictionary<int, KeyValuePair<string, ESMSFields>> ext = null)
        {
            if (currentList == null)
                throw new InvalidOperationException("SMissionStep.AddField was called outside of an SMissionStep.InitGUI " +
                    "operation. This is unsupported!");
            switch (type)
            {
                case ESMSFields.InputString:
                case ESMSFields.InputStringAux:
                    currentList.Add(type, new SMSFieldOptionsStringGUI(name, type, step, options, ext));
                    break;
                case ESMSFields.InputNum:
                case ESMSFields.InputNum_int:
                    currentList.Add(type, new SMSFieldOptionsFloatGUI(name, ESMSFields.InputNum, step, options, ext));
                    break;
                case ESMSFields.FolderEventList:
                default:
                    throw new InvalidOperationException("SMissionStep.AddField was called with ESMSFields of type [" +
                        type.ToString() + "]. This is unsupported!");
            }
        }
    }
}
