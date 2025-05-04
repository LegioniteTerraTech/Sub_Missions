using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;

namespace Sub_Missions.Steps
{
    public class StepSetupTech : SMissionStep
    {
        public override bool ForceUsesVarBool() => false;
        public override bool ForceUsesVarInt() => false;
        public override string GetTooltip() =>
            "Creates a specified Tech to use during the mission";
        public override string GetDocumentation()
        {
            return
                "{  // " + GetTooltip() +
                  "\n  \"StepType\": \"SetupTech\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  \"Position\": {  // The position where this is handled relative to the Mission origin." +
                  "\n    \"x\": 0.0," +
                  "\n    \"y\": 2.0," +
                  "\n    \"z\": 0.0" +
                  "\n  }," +
                  "\n  \"Forwards\": {  // The forwards facing of the Tech to spawn relative to north." +
                  "\n    \"x\": 0.0," +
                  "\n    \"y\": 0.0," +
                  "\n    \"z\": 1.0" +
                  "\n  }," +
                  "\n  \"TerrainHandling\": 2,  // " + TerrainHandlingDesc +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VariableType\": \"True\",       // See the top of this file." +
                  "\n  \"VariableCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The varible index that determines if it should spawn more techs." +
                  "\n  // Input Parameters" +
                  "\n  \"InputNum\": 0,             // The Team to set the new Tech(s) to" +
                  "\n  \"InputString\": \"TechName\",   // The name of the TrackedTech to spawn." +
                  "\n  \"InputStringAux\": null, // Leave empty to fire on first Mission Spawning" +
                  "\n  //\"OnStartup\", // Spawn once IMMEDEATELY on mission generation" +
                  "\n  //\"OnTriggerOnce\", // Spawn once when this is triggered and it's SetMissionVarIndex1 is true" +
                  "\n  //\"Infinite\",  // Allow infinite spawning while conditions are true and within the progress ID range" +
                "\n},";
        }
        public override void InitGUI()
        {
            AddField(ESMSFields.Position, "Position");
            AddField(ESMSFields.Forwards, "Forwards");
            AddField(ESMSFields.EulerAngles, "Euler Angles");
            AddField(ESMSFields.TerrainHandling, "Placement");
            AddField(ESMSFields.VaribleType, "Condition Mode");
            AddField(ESMSFields.VaribleCheckNum, "Conditional Constant");
            AddField(ESMSFields.SetMissionVarIndex1, "Success Output");
            AddField(ESMSFields.InputNum, "Team");
            AddField(ESMSFields.InputString_Tech, "Assigned Tech");
            AddOptions(ESMSFields.InputStringAux, "Spawn This Tech ", new string[]
                {
                    "OnStartup",
                    "Trigger Once",
                    "Infinite",
                }
            );
        }
        public override void OnInit() { 
            
        }
        public override void OnDeInit() { }

        public override void FirstSetup()
        {   // Spawn Tech
            SMission.hasTech = true;
            SMission.SavedInt = 0;
            if (ManNetwork.IsHost)
            {
                if (SMission.InputStringAux.NullOrEmpty() || SMission.InputStringAux == "OnStartup")
                {   // Spawn once on MISSION FIRST SPAWN
                    try
                    {
                        if (SMission.Mission.Tree.TreeTechs.TryGetValue(SMission.InputString, out var ST))
                            SMUtil.SpawnTechAddTracked(ref Mission, SMission.Position, (int)SMission.InputNum, 
                                SMission.Forwards, SMission.InputString);
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            string msg = "";
                            foreach (var item in Mission.Tree.TreeTechs)
                            {
                                msg += "\n- " +item.Key;
                            }
                            SMUtil.Error(false, SMission.LogName, 
                                KickStart.ModID + ": StepSetupTech - Failed: " + SMission.InputString + " finding failed.\n" +
                                "existing techs are:" + msg);
                            SMUtil.Error(false, SMission.LogName, 
                                KickStart.ModID + ": StepSetupTech - Team " + SMission.InputNum);
                            SMUtil.Error(false, SMission.LogName, 
                                KickStart.ModID + ": StepSetupTech - Mission " + Mission.Name);
                        }
                        catch (Exception e2)
                        {
                            SMUtil.Assert(false, SMission.LogName, KickStart.ModID + ": StepSetupTech - Failed: COULD NOT FETCH INFORMATION!!!", e2);
                        }
                        SMUtil.Assert(false, SMission.LogName, KickStart.ModID + ": Error", e);
                    }
                }
            }
        }
        public override void Trigger()
        {
            SMission.hasTech = true;
            if (ManNetwork.IsHost)
            {
                if (SMUtil.BoolOut(ref SMission))
                {
                    if (SMission.InputStringAux == "OnTriggerOnce")
                    {   // we spawn only ONE tech!
                        if (SMission.SavedInt == 0)
                        {
                            try
                            {
                                SMUtil.SpawnTechAddTracked(ref Mission, SMission.Position + 
                                    (SMission.VaribleCheckNum * UnityEngine.Random.insideUnitCircle.ToVector3XZ()), 
                                    (int)SMission.InputNum, SMission.Forwards, SMission.InputString);
                            }
                            catch (Exception e)
                            {
                                try
                                {
                                    SMUtil.Error(false, SMission.LogName, 
                                        KickStart.ModID + ": StepSetupTech (OnTriggerOnce) - Failed: " + SMission.InputString + " finding failed.");
                                    SMUtil.Error(false, SMission.LogName, 
                                        KickStart.ModID + ": StepSetupTech (OnTriggerOnce) - Team " + SMission.InputNum);
                                    SMUtil.Error(true, SMission.LogName, 
                                        KickStart.ModID + ": StepSetupTech (OnTriggerOnce) - Mission " + Mission.Name);
                                }
                                catch (Exception e2)
                                {
                                    SMUtil.Assert(true, SMission.LogName, KickStart.ModID + ": StepSetupTech (OnTriggerOnce) - Failed: COULD NOT " +
                                        "FETCH INFORMATION!!!", e2);
                                }
                                //Debug_SMissions.Log(KickStart.ModID + ": Stack trace - " + StackTraceUtility.ExtractStackTrace());
                                Debug_SMissions.Log(KickStart.ModID + ": Error - " + e);
                            }
                            SMission.SavedInt = 1;
                        }
                    }
                    else if (SMission.InputStringAux == "Infinite")
                    {   // we spawn a tech every MissionUpdate while this is active
                        try
                        {
                            SMUtil.SpawnTechAddTracked(ref Mission, SMission.Position + (SMission.VaribleCheckNum * UnityEngine.Random.insideUnitCircle.ToVector3XZ()), (int)SMission.InputNum, SMission.Forwards, SMission.InputString);
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                SMUtil.Error(false, SMission.LogName, 
                                    KickStart.ModID + ": StepSetupTech (Infinite) - Failed: " + SMission.InputString + " finding failed.");
                                SMUtil.Error(false, SMission.LogName, 
                                    KickStart.ModID + ": StepSetupTech (Infinite) - Team " + SMission.InputNum);
                                SMUtil.Error(true, SMission.LogName, 
                                    KickStart.ModID + ": StepSetupTech (Infinite) - Mission " + Mission.Name);
                            }
                            catch (Exception e2)
                            {
                                SMUtil.Assert(true, SMission.LogName, KickStart.ModID + ": StepSetupTech (Infinite) - Failed: COULD NOT " +
                                    "FETCH INFORMATION!!!", e2);
                            }
                            //Debug_SMissions.Log(KickStart.ModID + ": Stack trace - " + StackTraceUtility.ExtractStackTrace());
                            Debug_SMissions.Log(KickStart.ModID + ": Error - " + e);
                        }
                    }
                }
            }
        }
    }
}
