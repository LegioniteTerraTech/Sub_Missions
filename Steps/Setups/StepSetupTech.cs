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
        public override string GetDocumentation()
        {
            return
                "{  // Creates a Tech to use during the mission" +
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
                  "\n  \"VaribleType\": \"True\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The varible index that determines if it should spawn more techs." +
                  "\n  // Input Parameters" +
                  "\n  \"InputNum\": 0,             // The Team to set the new Tech(s) to" +
                  "\n  \"InputString\": \"TechName\",   // The name of the TrackedTech to spawn." +
                  "\n  \"InputStringAux\": null, // Leave empty to fire on first Mission Spawning" +
                  "\n  //\"OnTriggerOnce\", // Spawn once when this is triggered and it's SetMissionVarIndex1 is true" +
                  "\n  //\"Infinite\",  // Allow infinite spawning while conditions are true and within the progress ID range" +
                "\n},";
        }
        public override void OnInit() { }
        public override void OnDeInit() { }

        public override void FirstSetup()
        {   // Spawn target to kill
            SMission.hasTech = true;
            if (ManNetwork.IsHost)
            {
                if (SMission.InputStringAux.NullOrEmpty())
                {
                    try
                    {
                        SMUtil.SpawnTechTracked(ref Mission, SMission.Position, (int)SMission.InputNum, SMission.Forwards, SMission.InputString);
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            SMUtil.Assert(false, "SubMissions: StepSetupTech - Failed: " + SMission.InputString + " finding failed.");
                            SMUtil.Assert(false, "SubMissions: StepSetupTech - Team " + SMission.InputNum);
                            SMUtil.Assert(false, "SubMissions: StepSetupTech - Mission " + Mission.Name);
                        }
                        catch
                        {
                            SMUtil.Assert(false, "SubMissions: StepSetupTech - Failed: COULD NOT FETCH INFORMATION!!!");
                        }
                        Debug.Log("SubMissions: Error - " + e);
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
                                SMUtil.SpawnTechAddTracked(ref Mission, SMission.Position + (SMission.VaribleCheckNum * UnityEngine.Random.insideUnitCircle.ToVector3XZ()), (int)SMission.InputNum, SMission.Forwards, SMission.InputString);
                            }
                            catch (Exception e)
                            {
                                try
                                {
                                    SMUtil.Assert(false, "SubMissions: StepSetupTech (OnTriggerOnce) - Failed: " + SMission.InputString + " finding failed.");
                                    SMUtil.Assert(false, "SubMissions: StepSetupTech (OnTriggerOnce) - Team " + SMission.InputNum);
                                    SMUtil.Assert(true, "SubMissions: StepSetupTech (OnTriggerOnce) - Mission " + Mission.Name);
                                }
                                catch
                                {
                                    SMUtil.Assert(true, "SubMissions: StepSetupTech (OnTriggerOnce) - Failed: COULD NOT FETCH INFORMATION!!!");
                                }
                                //Debug.Log("SubMissions: Stack trace - " + StackTraceUtility.ExtractStackTrace());
                                Debug.Log("SubMissions: Error - " + e);
                            }
                            SMission.SavedInt = 1;
                        }
                    }
                    else if (SMission.InputStringAux == "Infinite")
                    {   // we spawn infinite techs every second while this is active
                        try
                        {
                            SMUtil.SpawnTechAddTracked(ref Mission, SMission.Position + (SMission.VaribleCheckNum * UnityEngine.Random.insideUnitCircle.ToVector3XZ()), (int)SMission.InputNum, SMission.Forwards, SMission.InputString);
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                SMUtil.Assert(false, "SubMissions: StepSetupTech (Infinite) - Failed: " + SMission.InputString + " finding failed.");
                                SMUtil.Assert(false, "SubMissions: StepSetupTech (Infinite) - Team " + SMission.InputNum);
                                SMUtil.Assert(true, "SubMissions: StepSetupTech (Infinite) - Mission " + Mission.Name);
                            }
                            catch
                            {
                                SMUtil.Assert(true, "SubMissions: StepSetupTech (Infinite) - Failed: COULD NOT FETCH INFORMATION!!!");
                            }
                            //Debug.Log("SubMissions: Stack trace - " + StackTraceUtility.ExtractStackTrace());
                            Debug.Log("SubMissions: Error - " + e);
                        }
                    }
                }
            }
        }
    }
}
