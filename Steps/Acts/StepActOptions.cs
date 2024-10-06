using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sub_Missions.ManWindows;

namespace Sub_Missions.Steps
{
    public class StepActOptions : SMissionStep
    {
        public override string GetDocumentation()
        {
            return
                "{  // Gives the player two options:  \"InputStringAux\" or \"No\"" +
                  "\n  \"StepType\": \"ActOptions\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"True\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that determines if it should be shown." +
                  "\n  // Input Parameters" +
                  "\n  \"InputNum\": 0,             // The option window mode - 0 for title, 1 for option text" +
                  "\n  \"InputString\": \"TechName\",   // The name of Option Window." +
                  "\n  \"InputStringAux\": null,      // The name of the Option that isn't \"No\"." +
                  "\n  // Input Parameters" +
                  "\n  \"SetMissionVarIndex2\": -1,       // The index Varible to be set if \"No\" option is selected." +
                  "\n  \"SetMissionVarIndex3\": -1,       // The index Varible to be set if \"InputStringAux\" option is selected." +
                "\n},";
        }
        public override void InitGUI()
        {
            AddField(ESMSFields.VaribleType, "Condition Mode");
            AddField(ESMSFields.VaribleCheckNum, "Conditional Constant");
            AddField(ESMSFields.SetMissionVarIndex1, "Show Condition");
            AddField(ESMSFields.SetMissionVarIndex2, "\"No\" Output");
            AddField(ESMSFields.SetMissionVarIndex3, "Other Output");
            /*
            AddOptions(ESMSFields.InputNum, "Mode", new string[]
                {
                    "Title, Other",
                    "2 Options",
                },
                new Dictionary<int, KeyValuePair<string, ESMSFields>>()
                {
                    {0, new KeyValuePair<string, ESMSFields>("Title", ESMSFields.InputString) },
                    {1, new KeyValuePair<string, ESMSFields>("Option", ESMSFields.InputString) },
                }
            );*/
            AddField(ESMSFields.InputString, "Window Title");
            AddField(ESMSFields.InputStringAux, "Other Option");
        }


        public override void OnInit() { }

        public override void OnDeInit()
        {
        }
        public override void FirstSetup()
        {
            WindowManager.AddPopupButtonDual(SMission.InputString, SMission.InputStringAux, true, this, windowOverride: WindowManager.WideWindow);
            SMission.AssignedWindow = WindowManager.GetCurrentPopup();
            WindowManager.ChangePopupPositioning(new Vector2(0.5f, 0.5f), SMission.AssignedWindow);
        }

        public override void Trigger()
        {   // run the text box things
            if (SMission.VaribleType == EVaribleType.DoSuccessID)
            {
                SMUtil.Error(true, SMission.LogName, 
                    KickStart.ModID + ": ActOptions does not support the VaribleType of DoSuccessID.  Mission " + Mission.Name + ", Step " + Mission.EventList.IndexOf(SMission));
            }
            if (SMission.SavedInt == 9999)
            {
                SMUtil.ConcludeGlobal3(ref SMission);
                return;
            }
            try
            {
                if (SMUtil.BoolOut(ref SMission))
                {   // Start speaking
                    WindowManager.ShowPopup(SMission.AssignedWindow);
                }
                else
                {   // Stop the speaking when the bool is false
                    WindowManager.HidePopup(SMission.AssignedWindow);
                }
            }
            catch
            {
                SMUtil.ConcludeGlobal2(ref SMission);
            }
        }

        public void OptionSelect()
        {
            SMission.SavedInt = 9999;
        }
    }
}
