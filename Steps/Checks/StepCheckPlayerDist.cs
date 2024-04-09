using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.Steps
{
    public class StepCheckPlayerDist : SMissionStep
    {
        public override string GetDocumentation()
        {
            return
                "{  // Tracks the distance of a Tech from a specified point in wold space" +
                  "\n  \"StepType\": \"CheckPlayerDist\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  \"SuccessProgressID\": 0,      // The ProgressID the mission will be pushed to if \"VaribleType\" is set to \"DoSuccessID\"" +
                  "\n  \"Position\": {  // The position where this is handled relative to the Mission origin." +
                  "\n    \"x\": 0.0," +
                  "\n    \"y\": 0.0," +
                  "\n    \"z\": 0.0" +
                  "\n  }," +
                  "\n  \"TerrainHandling\": 2,      // The way this should handle terrain: " + TerrainHandlingDesc +
                  "\n  // Input Parameters" +
                  "\n  \"InputNum\": 0.0,             // The distance which the player has to be within to trigger.  Use a negative number to invert the check." +
                  "\n  \"InputString\": \"TechName\",   // The name of the TrackedTech to keep watch over.  Leave empty to watch over the player." +
                  "\n  // Conditions TO SET on success" +
                  "\n  \"VaribleType\": \"True\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // Set the index to this value on completion if \"VaribleType\" is \"Int\"." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that is to be affected by this triggering" +
                "\n},";
        }
        public override void InitGUI()
        {
            AddField(ESMSFields.Position, "Position");
            AddField(ESMSFields.TerrainHandling, "Placement");
            AddField(ESMSFields.VaribleType_Action, "Condition Mode");
            AddField(ESMSFields.VaribleCheckNum, "Conditional Constant");
            AddField(ESMSFields.SetMissionVarIndex1, "Success Output");
            AddField(ESMSFields.InputNum, "Radius ([-] for invert)");
            AddOptions(ESMSFields.InputString, "Tracking: ", new string[]
                {
                    "Player",
                    "Tracked Tech",
                },
                new Dictionary<int, KeyValuePair<string, ESMSFields>>()
                {
                    {1, new KeyValuePair<string, ESMSFields>("Tech Name", ESMSFields.InputString_Tracked_Tech) },
                }
            );
        }
        public override void OnInit() { }

        public override void FirstSetup()
        {
        }
        public override void Trigger()
        {   // check player dist
            if (SMission.InputString.NullOrEmpty() || SMission.InputString == "Player")
            {
                if (SMission.InputNum > 0)
                {
                    if (SMUtil.IsPlayerInRangeOfPos(SMission.Position, SMission.InputNum))
                    {
                        Debug_SMissions.Log(KickStart.ModID + ": ProceedID - Mission " + Mission.Name + "'s CheckPlayerDist(Position - Within) has triggered");
                        SMUtil.ConcludeGlobal1(ref SMission);
                    }
                }
                else
                {   //invert detection trigger
                    if (!SMUtil.IsPlayerInRangeOfPos(SMission.Position, -SMission.InputNum))
                    {
                        Debug_SMissions.Log(KickStart.ModID + ": ProceedID - Mission " + Mission.Name + "'s CheckPlayerDist(Position - Outside) has triggered");
                        SMUtil.ConcludeGlobal1(ref SMission);
                    }
                }
            }
            else
            {
                if (SMission.InputNum > 0)
                {
                    if (SMUtil.GetTrackedTech(ref SMission, SMission.InputString, out Tank target))
                        if (SMUtil.IsPlayerInRangeOfPos(target.boundsCentreWorld, SMission.InputNum))
                        {
                            Debug_SMissions.Log(KickStart.ModID + ": ProceedID - Mission " + Mission.Name + "'s CheckPlayerDist(Enemy - Within) has triggered");
                            SMUtil.ConcludeGlobal1(ref SMission);
                        }
                }
                else
                {   //invert detection trigger
                    if (SMUtil.GetTrackedTech(ref SMission, SMission.InputString, out Tank target))
                        if (!SMUtil.IsPlayerInRangeOfPos(target.boundsCentreWorld, -SMission.InputNum))
                        {
                            Debug_SMissions.Log(KickStart.ModID + ": ProceedID - Mission " + Mission.Name + "'s CheckPlayerDist(Enemy - Outside) has triggered");
                            SMUtil.ConcludeGlobal1(ref SMission);
                        }
                }
            }
        }
        public override void OnDeInit()
        {
        }
    }
}
