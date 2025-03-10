﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Sub_Missions.Steps
{
    public class StepAirstrike : SMissionStep
    {
        private const float deliveryTimeSec = 100;

        public override bool ForceUsesVarBool() => false;
        public override bool ForceUsesVarInt() => false;
        public override string GetTooltip() =>
            "Creates a base bomb that explodes on impact as long as it's linked condition is true";
        public override string GetDocumentation()
        {
            return
                "{  // " + GetTooltip() +
                  "\n  \"StepType\": \"ActAirstrike\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  \"Position\": {  // The position where this is handled relative to the Mission origin." +
                  "\n    \"x\": 0.0," +
                  "\n    \"y\": 2.0," +
                  "\n    \"z\": 0.0" +
                  "\n  }," +
                  "\n  \"Forwards\": {  // The forwards facing of the explosion to spawn relative to north." +
                  "\n    \"x\": 0.0," +
                  "\n    \"y\": 0.0," +
                  "\n    \"z\": 1.0" +
                  "\n  }," +
                  "\n  \"TerrainHandling\": 2,  // " + TerrainHandlingDesc +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"True\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The varible index that determines if it should fire the bombs." +
                  "\n  // Input Parameters" +
                  "\n  \"InputNum\": 0,             // The radius inaccuraccy of the airstrike" +
                  "\n  \"InputString\": \"Position\",   // The way we aim.  Position is the set postion of this Step in relation the the mission" +
                  "\n  //\"Instant\", // Explode instantly at position without airdrop" +
                  "\n  //  TECH POSITIONING" +
                  "\n  //\"AimInstant\", // Explode instantly at the target, or player if InputStringAux is empty" +
                  "\n  //\"AimPredict\", // Aim at the target (or player if InputStringAux is empty), with prediction" +
                  "\n  //\"AimDirect\",  // Aim at the target, or player if InputStringAux is empty" +
                  "\n  \"InputStringAux\": null,   // Change the aiming target to a valid TrackedTech. Leave empty to target the player" +
                "\n},";
        }

        public override void InitGUI()
        {
            AddField(ESMSFields.Position, "Position");
            AddField(ESMSFields.Forwards, "Forwards");
            AddField(ESMSFields.TerrainHandling, "Placement");
            AddField(ESMSFields.VaribleType, "Condition Mode");
            AddField(ESMSFields.VaribleCheckNum, "Conditional Constant");
            AddField(ESMSFields.SetMissionVarIndex1, "Active Condition");
            AddField(ESMSFields.InputNum, "Spread Intensity");
            AddOptions(ESMSFields.InputString, "Aiming", new string[] {
                "Position",
                "Instant",
                "AimInstant",
                "AimPredict",
                "AimDirect",
                "AimGuided",
            });
            AddOptions(ESMSFields.InputStringAux, "Target: ", new string[]
                {
                    "Player",
                    "Tracked",
                },
                new Dictionary<int, KeyValuePair<string, ESMSFields>>()
                {
                    {1, new KeyValuePair<string, ESMSFields>("Tracked Tech", ESMSFields.InputString_Tracked_Tech) },
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
            SMission.hasTech = true;
            if (!ManNetwork.IsNetworked || ManNetwork.IsHost)
            {
                if (SMUtil.BoolOut(ref SMission))
                {   // we spawn infinite bombs every second while this is active
                    if (SMission.InputString == "Instant")
                    {
                        FireInstant(SMission.Position);
                    }
                    else if (!SMission.InputStringAux.NullOrEmpty())
                    {
                        if (SMUtil.GetTrackedTech(ref SMission, SMission.InputStringAux, out Tank tech))
                        {
                            if (tech == null)
                            {
                                Fire(SMission.Position);
                            }
                            else if (SMission.InputString == "AimInstant")
                            {
                                Fire(tech.boundsCentreWorld);
                            }
                            else if (SMission.InputString == "AimPredict")
                            {
                                if (tech.rbody)
                                {
                                    Vector3 aiming = tech.boundsCentreWorld + (tech.rbody.velocity * Time.fixedDeltaTime * deliveryTimeSec);
                                    if (ManWorld.inst.CheckIsTileAtPositionLoaded(aiming))
                                        Fire(aiming);
                                    else
                                    {   // overpredicted (out of bounds)
                                        Fire(tech.boundsCentreWorld);
                                    }
                                }
                                else
                                    Fire(tech.boundsCentreWorld);
                            }
                            else
                            {
                                Fire(tech.boundsCentreWorld);
                            }
                        }
                        else
                        {
                            SMUtil.Error(true, SMission.LogName, 
                                KickStart.ModID + ": ActAirstrike - Failed: InputStringAux does not reference a valid TrackedTech within the mission.  Mission " + Mission.Name);
                        }
                    }
                    else if (SMission.InputString == "AimInstant")
                    {
                        if (Singleton.playerTank?.rbody)
                        {
                            Vector3 aiming = Singleton.playerTank.boundsCentreWorld + (Singleton.playerTank.rbody.velocity * Time.fixedDeltaTime * deliveryTimeSec);
                            if (ManWorld.inst.CheckIsTileAtPositionLoaded(aiming))
                                Fire(aiming);
                            else
                            {   // overpredicted (out of bounds)
                                Fire(Singleton.playerPos);
                            }
                        }
                        else
                            Fire(Singleton.playerPos);
                    }
                    else if (SMission.InputString == "AimPredict")
                    {
                        if (Singleton.playerTank?.rbody)
                        {
                            Vector3 aiming = Singleton.playerTank.boundsCentreWorld + (Singleton.playerTank.rbody.velocity * Time.fixedDeltaTime * deliveryTimeSec);
                            if (ManWorld.inst.CheckIsTileAtPositionLoaded(aiming))
                                Fire(aiming);
                            else
                            {   // overpredicted (out of bounds)
                                Fire(Singleton.playerPos);
                            }
                        }
                        else
                            Fire(Singleton.playerPos);
                    }
                    else if (SMission.InputString == "AimDirect")
                    {
                        Fire(Singleton.playerPos);
                    }
                    else if (SMission.InputString == "AimGuided")
                    {
                        Fire(Singleton.playerPos, Singleton.playerTank?.visible);
                    }
                    else
                    {
                        Fire(SMission.Position);
                    }
                }
            }
        }
        public void FireInstant(Vector3 scenePos)
        {
            try
            {
                Vector3 variation = SMission.InputNum * UnityEngine.Random.insideUnitCircle.ToVector3XZ();
                if (SMission.Forwards == Vector3.zero)
                    SMExplosion.SpawnNew(scenePos + variation, Vector3.forward);
                else
                    SMExplosion.SpawnNew(scenePos + variation, SMission.Forwards);
            }
            catch (Exception e)
            {
                try
                {
                    SMUtil.Assert(false, SMission.LogName, KickStart.ModID + ": StepAirstrike(Instant) - Failed on Mission " + Mission.Name, e);
                }
                catch (Exception e2)
                {
                    SMUtil.Assert(true, SMission.LogName, KickStart.ModID + ": StepAirstrike(Instant) - Failed: COULD NOT FETCH INFORMATION!!!", e2);
                }
                //Debug_SMissions.Log(KickStart.ModID + ": Stack trace - " + StackTraceUtility.ExtractStackTrace());
                Debug_SMissions.Log(KickStart.ModID + ": Error - " + e);
            }
        }
        public void Fire(Vector3 scenePos, Visible target = null)
        {
            try
            {
                Vector3 variation = SMission.InputNum * UnityEngine.Random.insideUnitCircle.ToVector3XZ();
                if (SMission.Forwards == Vector3.zero)
                    SMExplosion.OrbitalLaunch(scenePos + variation, Vector3.forward, target);
                else
                    SMExplosion.OrbitalLaunch(scenePos + variation, SMission.Forwards, target);
                //ManSFX.inst.PlayMiscSFX(ManSFX.MiscSfxType.IntroWindAndAlarm);
            }
            catch (Exception e)
            {
                try
                {
                    SMUtil.Assert(false, SMission.LogName, KickStart.ModID + ": StepAirstrike - Failed on Mission " + Mission.Name, e);
                }
                catch (Exception e2)
                {
                    SMUtil.Assert(true, SMission.LogName, KickStart.ModID + ": StepAirstrike - Failed: COULD NOT FETCH INFORMATION!!!", e2);
                }
                //Debug_SMissions.Log(KickStart.ModID + ": Stack trace - " + StackTraceUtility.ExtractStackTrace());
                Debug_SMissions.Log(KickStart.ModID + ": Error - " + e);
            }
        }
    }
}
