using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.AI;
using TAC_AI.AI.Enemy;
using TAC_AI.Templates;

namespace Sub_Missions.Steps
{
    public class StepTransformTech : SMissionStep
    {
        public override string GetDocumentation()
        {
            /*
            return
                "{  // Changes/Edits the entire form of a TrackedTech" +
                  "\n  \"StepType\": \"TransformTech\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  \"SuccessProgressID\": 0,      // The ProgressID the mission will be pushed to if this is successful" +
                  "\n  \"Position\": {  // The position where this is handled relative to the Mission origin." +
                  "\n    \"x\": 0.0," +
                  "\n    \"y\": 0.0," +
                  "\n    \"z\": 0.0" +
                  "\n  }," +
                  "\n  \"EulerAngles\": {  // The rotation in EulerAngles that the step subject should be oriented." +
                  "\n    \"x\": 0.0," +
                  "\n    \"y\": 0.0," +
                  "\n    \"z\": 0.0" +
                  "\n  }," +
                  "\n  \"Forwards\": {  // The forwards facing of the Tech to spawn relative to north." +
                  "\n    \"x\": 0.0," +
                  "\n    \"y\": 0.0," +
                  "\n    \"z\": 1.0" +
                  "\n  }," +
                  "\n  \"TerrainHandling\": 2,      // The way this should handle terrain: " + TerrainHandlingDesc +
                  "\n  \"RevProgressIDOffset\": false," +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"True\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that determines if it should execute." +
                  "\n  // Input Parameters" +
                  "\n  \"InputNum\": 0.0,             // The Team to set the Tech to if \"InputStringAux\" is set to team" +
                  "\n  \"InputString\": \"TechName\",   // The name of the TrackedTech to change." +
                  "\n  \"InputStringAux\": null,      // What to change on the Tracked Tech, or the Tech to swap to." +
                "\n},";
            */
            return
                "{  // Changes/Edits the entire form of a TrackedTech" +
                  "\n  \"StepType\": \"TransformTech\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"True\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that determines if it should execute." +
                  "\n  // Input Parameters" +
                  "\n  \"InputNum\": 0.0,             // The Team to set the Tech to if \"InputStringAux\" is set to team" +
                  "\n  \"InputString\": \"TechName\",   // The name of the TrackedTech to change." +
                  "\n  \"InputStringAux\": null,      // What to change on the Tracked Tech, or the Tech to swap to." +
                "\n},";
        }
        public override void InitGUI()
        {
            AddField(ESMSFields.VaribleType, "Condition Mode");
            AddField(ESMSFields.VaribleCheckNum, "Conditional Constant");
            AddField(ESMSFields.SetMissionVarIndex1, "Active Condition");
            AddField(ESMSFields.InputString_Tracked_Tech, "Tracked Tech");
            AddField(ESMSFields.InputNum_int, "Team"); 
            AddOptions(ESMSFields.InputStringAux, "Change: ", new string[] 
                { 
                    "Team",
                    "Tech",
                },
                new Dictionary<int, KeyValuePair<string, ESMSFields>>()
                {
                    {1, new KeyValuePair<string, ESMSFields>("Tech Name", ESMSFields.InputStringAux_Tech) },
                }
            );
        }

        public override void OnInit() { }

        public override void OnDeInit() { }

        public override void FirstSetup()
        {
        }
        public override void Trigger()
        {
            if (ManNetwork.IsHost)
            {
                if (SMUtil.BoolOut(ref SMission))
                {
                    try
                    {
                        TrackedTech tTech = SMUtil.GetTrackedTechBase(ref Mission, SMission.InputString);

                        if (SMission.InputStringAux == "Team")
                        {
                            if (SMission.SavedInt < 1)
                            {
                                tTech.TechAuto.SetTeam((int)SMission.InputNum);
                                Debug_SMissions.Log("SubMissions: Changed team of tech " + tTech.TechAuto.name + " to " + tTech.TechAuto.Team);
                            }
                        }
                        else if (SMission.InputStringAux != null && SMission.InputStringAux != "")
                        {   // allow the AI to change it's form on demand
                            if (SMission.SavedInt < SMission.InputNum)
                            {
                                Tank techCur = tTech.TechAuto;
                                Debug_SMissions.Log("SubMissions: More than meets the eye");
                                tTech.TechAuto = RawTechLoader.TechTransformer(techCur, SMission.InputStringAux);
                            }
                        }
                        else
                        {
                            SMUtil.Error(true, SMission.LogName,
                                "SubMissions: TransformTech - Failed: InputStringAux does not contain a valid RAWTechJSON Blueprint.  Mission " + Mission.Name);
                        }
                    }
                    catch (Exception e)
                    {   // Cannot work without TACtical_AI
                        if (KickStart.isTACAIPresent)
                        {
                            SMUtil.Assert(true, SMission.LogName, "SubMissions: TransformTech  - Failed: COULD NOT FETCH TECH INFORMATION!!!", e);
                            Debug_SMissions.Log("SubMissions: Stack trace - " + StackTraceUtility.ExtractStackTrace());
                            Debug_SMissions.Log("SubMissions: Error - " + e);
                        }
                        else
                            Debug_SMissions.Log("SubMissions: TransformTech  - Failed: TACticial_AIs is not installed ~ Unable to execute");
                    }
                    //SMission.SavedInt++;
                }
            }
        }
    }
}
