using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.Steps
{
    public class StepFolder : SMissionStep
    {
        public override bool ForceUsesVarBool() => false;
        public override bool ForceUsesVarInt() => false;
        public override string GetTooltip() =>
            "Keep your Steps neat, tidy, and collapsable with a Folder";
        public override string GetDocumentation()
        {
            return
                "{  // " + GetTooltip() +
                  "\n  \"StepType\": \"Folder\",  // Creates a bracket folder space  to hold your SubMissionSteps " +
                  "\n  \"InputString\": \"FolderName\",  // Just a label space for you to use to keep track incase something fails" +
                  "\n  // Conditions TO CHECK before executing the contents of the folder" +
                  "\n  \"VaribleType\": \"None\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,        // An Index to check (-1 means always true)" +
                  "\n  \"InputStringAux\": \"Default\",  // The special trigger case this folder should take when active" +
                  "\n  // The Folder" +
                  "\n  \"FolderEventList\": {},        // Insert your events here" +
                "\n},";
        }
        public override void InitGUI()
        {
            AddField(ESMSFields.VaribleType, "Condition Mode");
            AddField(ESMSFields.VaribleCheckNum, "Conditional Constant");
            AddField(ESMSFields.SetMissionVarIndex1, "Condition");
            AddField(ESMSFields.InputStringAux_Tracked_Tech, "Tracked Tech");
            AddField(ESMSFields.InputString, "Folder Name");
            AddOptions(ESMSFields.InputStringAux, "Trigger Case", new string[]
                {
                    "Team",
                    "Tech",
                },
                new Dictionary<int, KeyValuePair<string, ESMSFields>>()
                {
                    {1, new KeyValuePair<string, ESMSFields>("Tech Name", ESMSFields.InputStringAux_Tech) },
                }
            );
            //AddField(ESMSFields.FolderEventList, "Contents");
        }
        public override void OnInit() {
            SMission.ProgressID = SubMission.alwaysRunValue;
            if (SMission.FolderEventList == null)
                SMission.FolderEventList = new List<SubMissionStep>();
        }

        public override void OnDeInit()
        {
        }
        public override void FirstSetup()
        {
            //SMission.ProgressID = SubMission.alwaysRunValue;
            if (SMission.FolderEventList == null)
                SMission.FolderEventList = new List<SubMissionStep>();
        }
        public override void Trigger()
        {
            int position = 0;
            foreach (SubMissionStep step in SMission.FolderEventList)
            {
                if (Mission.CanRunStep(step.ProgressID))
                {
                    try
                    {   // can potentially fire too early before mission is set
                        step.Trigger();
                    }
                    catch
                    {
                        Debug_SMissions.Log(KickStart.ModID + ": Error on attempting step lerp (In Folder named " + (SMission.InputString != null ? SMission.InputString  : "unnamed") + ") " + position + " in relation to " + Mission.CurrentProgressID + " of mission " + Mission.Name + " in tree " + Mission.Tree.TreeName);
                        try
                        {
                            Debug_SMissions.Log(KickStart.ModID + ": Type of " + step.StepType.ToString() + " ProgressID " + step.ProgressID + " | Is connected to a mission: " + (step.Mission != null).ToString());
                        }
                        catch
                        {
                            Debug_SMissions.Log(KickStart.ModID + ": Confirmed null");
                        }
                    }
                }
                position++;
            }
        }
    }
}
