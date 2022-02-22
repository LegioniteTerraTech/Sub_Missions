using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;
using Sub_Missions.Steps;
using Sub_Missions.ManWindows;
using System.IO;
using Snapshots;

namespace Sub_Missions
{
    public static class SMUtil
    {
        public static Tank PlayerTank
        { 
            get => Singleton.playerTank;
        }

        public static bool errorQueued = false;
        public static string errorList = "";
        public static int countError = 0;
        public static bool repeatingErrored = false;

        // DEBUG
        public static void PushErrors()
        {
            if (KickStart.Debugger)
            {
                if (errorQueued)
                {
                    WindowManager.AddPopupMessage("SubMissions Error", errorList);
                    WindowManager.ShowPopup(new Vector2(0.5f, 0.5f));
                    errorList = "";
                    errorQueued = false;
                    countError = 0;
                    repeatingErrored = false;
                }
                else
                {
                    WindowManager.AddPopupMessage("SubMissions Error", "No errors reported.");
                    WindowManager.ShowPopup(new Vector2(0.5f, 0.5f));
                }
            }
        }
        public static void Assert(bool repeatingError, string input)
        {
            if (KickStart.Debugger && !repeatingErrored)
            {
                try
                {
                    if (input.NullOrEmpty())
                    {
                        input = "Sub_Missions.SMUtil.Assert has no information set for an edge case, please notify Legionite (or some SubMissions maintainer) to add an assert. " +
                                "\n Thanks!" +
                                "\n Unresolvable error! - No automatic debugging context was thrown in for this edge case!" +
                                "\n At " + StackTraceUtility.ExtractStackTrace();
                        repeatingError = true;
                    }
                    else
                    {
                        if (input.Contains("SubMissions: "))
                        {
                            countError++;
                            input.Replace("SubMissions: ", "<b>Error " + countError + "</b>: ");
                        }
                    }
                    if (repeatingError)
                    {
                        errorList += "<b>-MAJOR ERROR-<b> ";
                        repeatingErrored = true;
                    }
                    errorList += input + "\n";
                    Debug.Log(input);
                }
                catch (Exception e)
                {
                    errorList = "Error collector failed, please contact Legionite (or some SubMissions maintainer).";
                    Debug.Log(errorList + e);
                }
                errorQueued = true;
            }
        }


        // MAINs
        public static Vector3 VectorOnTerrain(Vector3 input, float heightOffset = 0)
        {
            if (Singleton.Manager<ManWorld>.inst.GetTerrainHeight(input, out float height))
            {
                input.y = height + heightOffset;
            }
            else
            {
                input.y = heightOffset;
            }

            return input;
        }
        public static Vector3 SetPosTerrain(ref Vector3 input, Vector3 missionOriginScene, int terrainHanding)
        {
            switch (terrainHanding)
            {
                case 1:
                    input += missionOriginScene;
                    if (Singleton.Manager<ManWorld>.inst.GetTerrainHeight(input, out float height))
                    {
                        if (height > input.y)
                            input.y = height;
                    }
                    break;
                case 2:
                    input += missionOriginScene.SetY(0);
                    input = VectorOnTerrain(input, input.y);
                    break;
                case 3:
                    input += missionOriginScene.SetY(0);
                    input = VectorOnTerrain(input, 0);
                    break;
                default:
                    input += missionOriginScene;
                    break;
            }

            return input;
        }


        // PLAYER
        public static float GetPlayerDist(Vector3 scenePos)
        {
            try
            {
                return (scenePos - PlayerTank.rootBlockTrans.position).magnitude;
            }
            catch { return 900000; }// probably was destroyed
        }
        public static bool IsPlayerInRangeOfPos(Vector3 Pos, float distance)
        {
            try
            {
                return (Pos - PlayerTank.rootBlockTrans.position).magnitude <= distance;
            }
            catch { return false; }// probably was destroyed
        }
        public static bool IsPlayerDestroyed()
        {
            try
            {
                return PlayerTank == null;
            }
            catch { return true; }// probably was destroyed
        }
        public static bool GetPlayerIsInCombat()
        {
            try
            {
                return PlayerTank.Vision.GetFirstVisibleTechIsEnemy(PlayerTank.Team);
            }
            catch { return false; }// probably was destroyed
        }


        // ETC
        public static bool IsTechInRangeOfPos(Tank tech, Vector3 Pos, float distance)
        {
            try
            {
                return (Pos - tech.rootBlockTrans.position).magnitude <= distance;
            }
            catch { return false; }// probably was destroyed
        }
        public static void ShiftCurrentID(ref int currentID, int StepID, bool invertShift)
        {
            if (invertShift)
                currentID = StepID - 1;
            else
                currentID = StepID + 1;
        }
        public static void ShiftCurrentID(ref SubMissionStep Step)
        {
            if (Step.RevProgressIDOffset)
                Step.Mission.CurrentProgressID = Step.ProgressID - 1;
            else
                Step.Mission.CurrentProgressID = Step.ProgressID + 1;
        }
        public static void ReturnCurrentID(ref SubMissionStep Step)
        {
            Step.Mission.CurrentProgressID = Step.ProgressID;
        }
        public static void ProceedID(ref SubMissionStep Step)
        {
            Step.Mission.CurrentProgressID = Step.SuccessProgressID;
            Debug.Log("SubMissions: ProceedID - Mission " + Step.Mission.Name + " has moved on to ID " + Step.Mission.CurrentProgressID);
            if (ManNetwork.IsNetworked && ManNetwork.IsHost)
            {
                //NetworkHandler
            }
        }
        public static void ConcludeGlobal1(ref SubMissionStep Step)
        {
            if (ManNetwork.IsNetworked && !ManNetwork.IsHost)
                return; // only host does variable updates
            try
            {
                int setVal = Step.SetMissionVarIndex1;
                HandleVariables(ref Step, setVal);
            }
            catch
            {
                Assert(true, "SubMissions: Error in output [SetMissionVarIndex1] in mission " + Step.Mission.Name + " | Step type " + Step.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) \nand make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry on the list, 1 counts as the second entry, and so on.");
            }
        }
        public static void ConcludeGlobal2(ref SubMissionStep Step)
        {
            if (ManNetwork.IsNetworked && !ManNetwork.IsHost)
                return; // only host does variable updates
            try
            {
                int setVal = Step.SetMissionVarIndex2;
                HandleVariables(ref Step, setVal);
            }
            catch
            {
                Assert(true, "SubMissions: Error in output [SetMissionVarIndex2] in mission " + Step.Mission.Name + " | Step type " + Step.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) \nand make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry on the list, 1 counts as the second entry, and so on.");
            }
        }
        public static void ConcludeGlobal3(ref SubMissionStep Step)
        {
            if (ManNetwork.IsNetworked && !ManNetwork.IsHost)
                return; // only host does variable updates
            try
            {
                int setVal = Step.SetMissionVarIndex3;
                HandleVariables(ref Step, setVal);
            }
            catch
            {
                Assert(true, "SubMissions: Error in output [SetMissionVarIndex3] in mission " + Step.Mission.Name + " | Step type " + Step.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) \nand make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry on the list, 1 counts as the second entry, and so on.");
            }
        }
        public static void HandleVariables(ref SubMissionStep Step, int setVal)
        {
            switch (Step.VaribleType)
            {
                case EVaribleType.Int: //
                    if (setVal < 0)
                        return; // that means it's not being used
                    Step.Mission.VarInts[setVal] = (int)Step.VaribleCheckNum;
                    break;
                case EVaribleType.False: //
                    if (setVal < 0)
                        return; // that means it's not being used
                    Step.Mission.VarTrueFalse[setVal] = false;
                    break;
                case EVaribleType.True: //
                    if (setVal < 0)
                        return; // that means it's not being used
                    Step.Mission.VarTrueFalse[setVal] = true;
                    break;
                case EVaribleType.DoSuccessID: // 
                    ProceedID(ref Step);
                    break;
                case EVaribleType.None: // 
                default:
                    break;
            }
        }

        /// <summary>
        /// Only uses the first GlobalIndex
        /// </summary>
        /// <param name="Step"></param>
        /// <returns></returns>
        public static bool BoolOut(ref SubMissionStep Step)
        {
            return BoolOut(ref Step, Step.SetMissionVarIndex1);
        }
        public static bool BoolOut(ref SubMissionStep Step, int GlobalIndex)
        {
            try
            {
                if (GlobalIndex < 0)
                    return true; // that means it's not being used
                switch (Step.VaribleType)
                {
                    case EVaribleType.Int: //
                        return Step.Mission.VarInts[GlobalIndex] == (int)Step.VaribleCheckNum;
                    case EVaribleType.IntGreaterThan: //
                        return Step.Mission.VarInts[GlobalIndex] > (int)Step.VaribleCheckNum;
                    case EVaribleType.IntLessThan: //
                        return Step.Mission.VarInts[GlobalIndex] < (int)Step.VaribleCheckNum;
                    case EVaribleType.False: //
                        return Step.Mission.VarTrueFalse[GlobalIndex] == false;
                    case EVaribleType.True: //
                        return Step.Mission.VarTrueFalse[GlobalIndex] == true;
                    case EVaribleType.None: // 
                    default:
                        return true;
                }
            }
            catch
            {
                Assert(true, "SubMissions: Error in output in mission " + Step.Mission.Name + " | Step type " + Step.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) \nand make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry on the list, 1 counts as the second entry, and so on.");
                return false;
            }
        }

        /// <summary>
        /// FIRES INSTANTLY NO MATTER WHAT
        /// Supports normal snapshots in the folder but performance will suffer on lower-end computers.  
        /// Snapshots are usually cached on first load.
        /// </summary>
        /// <param name="mission"></param>
        /// <param name="pos"></param>
        /// <param name="Team"></param>
        /// <param name="facingDirect"></param>
        /// <param name="TechName"></param>
        /// <returns></returns>
        public static Tank SpawnTechAuto(ref SubMission mission, Vector3 pos, int Team, Vector3 facingDirect, string TechName)
        {
            // Load from folder
            Tank tech;
            string dest = "Custom SMissions" + SMissionJSONLoader.up +  mission.Tree.TreeName + SMissionJSONLoader.up + "Raw Techs";
            //Debug.Log("SubMissions: SpawnTechAuto path is " + SMissionJSONLoader.BaseDirectory + SMissionJSONLoader.up + dest + SMissionJSONLoader.up + TechName);if ("__Airstrike".Equals(TechName))
            if (KickStart.isTACAIPresent && File.Exists(SMissionJSONLoader.BaseDirectory + SMissionJSONLoader.up + dest + SMissionJSONLoader.up + TechName + ".json"))
            {
                tech = RawTechLoader.SpawnTechExternal(pos, Team, facingDirect, RawTechExporter.LoadTechFromRawJSON(TechName, dest));

                if (Team == ManSpawn.NeutralTeam)
                    tech.SetInvulnerable(true, true);
                return tech;
            }
            else if (TechName.Length > 4 && mission.Tree.MissionTextures.TryGetValue((TechName + ".png").GetHashCode(), out Texture value))
            {   // Supports normal snapshots
                if (ManScreenshot.TryDecodeSnapshotRender((Texture2D)value, out TechData.SerializedSnapshotData data))
                {
                    ManSpawn.TankSpawnParams spawn = new ManSpawn.TankSpawnParams
                    {
                        isInvulnerable = Team == ManSpawn.NeutralTeam,
                        teamID = Team,
                        blockIDs = null,
                        isPopulation = Team == -1,
                        techData = data.CreateTechData(),
                        position = pos,
                        rotation = Quaternion.LookRotation(facingDirect),
                    };
                    tech = ManSpawn.inst.SpawnTank(spawn, true);
                    return tech;
                }
            }

            return null;
        }


        public static string SpawnTechTracked(ref SubMission mission, Vector3 pos, int Team, Vector3 facingDirect, string TechName, bool instant = false)
        {   // We pull these from MissionTechs.json
            Tank tech;
            if (instant)
            {
                string dest = "Custom SMissions" + SMissionJSONLoader.up + mission.Tree.TreeName + SMissionJSONLoader.up + "Raw Techs";
                //Debug.Log("SubMissions: SpawnTechAuto path is " + SMissionJSONLoader.BaseDirectory + SMissionJSONLoader.up + dest + SMissionJSONLoader.up + TechName);
                if (KickStart.isTACAIPresent && File.Exists(SMissionJSONLoader.BaseDirectory + SMissionJSONLoader.up + dest + SMissionJSONLoader.up + TechName + ".json"))
                {
                    tech = RawTechLoader.SpawnTechExternal(pos, Team, facingDirect, RawTechExporter.LoadTechFromRawJSON(TechName, dest));
                    SetTrackedTech(ref mission, tech);
                    if (Team == ManSpawn.NeutralTeam)
                        tech.SetInvulnerable(true, true);
                }
                else if (TechName.Length > 4 && mission.Tree.MissionTextures.TryGetValue((TechName + ".png").GetHashCode(), out Texture value))
                {   // Supports normal snapshots
                    if (ManScreenshot.TryDecodeSnapshotRender((Texture2D)value, out TechData.SerializedSnapshotData data))
                    {
                        ManSpawn.TankSpawnParams spawn = new ManSpawn.TankSpawnParams
                        {
                            isInvulnerable = Team == 0,
                            teamID = Team,
                            blockIDs = null,
                            isPopulation = Team == -1,
                            techData = data.CreateTechData(),
                            position = pos,
                            rotation = Quaternion.LookRotation(facingDirect),
                        };
                        tech = ManSpawn.inst.SpawnTank(spawn, true);
                        SetTrackedTech(ref mission, tech);
                        if (Team == ManSpawn.NeutralTeam)
                            tech.SetInvulnerable(true, true);
                        return TechName;
                    }
                }
            }
            else
            {
                TrackedTech techCase = GetTrackedTechBase(ref mission, TechName);
                techCase.delayedSpawn = ManSpawn.inst.SpawnDeliveryBombNew(pos, DeliveryBombSpawner.ImpactMarkerType.Tech);
                techCase.delayedSpawn.BombDeliveredEvent.Subscribe(techCase.SpawnTech);
                techCase.DeliQueued = true;
            }
            return TechName;
        }
        /// <summary>
        /// Makes a FRESH NEW TrackedTech entry for the mission
        /// </summary>
        /// <param name="mission"></param>
        /// <param name="pos"></param>
        /// <param name="Team"></param>
        /// <param name="facingDirect"></param>
        /// <param name="TechName"></param>
        /// <param name="instant"></param>
        public static void SpawnTechAddTracked(ref SubMission mission, Vector3 pos, int Team, Vector3 facingDirect, string TechName, bool instant = false)
        {   // We pull these from MissionTechs.json
            TrackedTech tech = new TrackedTech(TechName, true);
            tech.mission = mission;
            if (instant)
            {
                string dest = "Custom SMissions" + SMissionJSONLoader.up + mission.Tree.TreeName + SMissionJSONLoader.up + "Raw Techs";
                //Debug.Log("SubMissions: SpawnTechAuto path is " + SMissionJSONLoader.BaseDirectory + SMissionJSONLoader.up + dest + SMissionJSONLoader.up + TechName);
                if (KickStart.isTACAIPresent && File.Exists(SMissionJSONLoader.BaseDirectory + SMissionJSONLoader.up + dest + SMissionJSONLoader.up + TechName + ".json"))
                {
                    tech.Tech = RawTechLoader.SpawnTechExternal(pos, Team, facingDirect, RawTechExporter.LoadTechFromRawJSON(TechName, dest));
                }
                else if (TechName.Length > 4 && mission.Tree.MissionTextures.TryGetValue((TechName + ".png").GetHashCode(), out Texture value))
                {   // Supports normal snapshots
                    if (ManScreenshot.TryDecodeSnapshotRender((Texture2D)value, out TechData.SerializedSnapshotData data))
                    {
                        ManSpawn.TankSpawnParams spawn = new ManSpawn.TankSpawnParams
                        {
                            isInvulnerable = Team == ManSpawn.NeutralTeam,
                            teamID = Team,
                            blockIDs = null,
                            isPopulation = Team == -1,
                            techData = data.CreateTechData(),
                            position = pos,
                            rotation = Quaternion.LookRotation(facingDirect),
                        };
                        tech.Tech = ManSpawn.inst.SpawnTank(spawn, true);
                    }
                }
            }
            else
            {
                tech.delayedSpawn = ManSpawn.inst.SpawnDeliveryBombNew(pos, DeliveryBombSpawner.ImpactMarkerType.Tech);
                tech.delayedSpawn.BombDeliveredEvent.Subscribe(tech.SpawnTech);
                tech.DeliQueued = true;
            }
            mission.TrackedTechs.Add(tech);
            return;
        }
        public static string InjectTechTracked(ref SubMission mission, TechData techdata)
        {
            return "error - requires PopulationInjector";
        }
        public static bool DoesTrackedTechExist(ref SubMissionStep mission, string TechName)
        {
            if (TechName == "Player Tech")
                return Singleton.playerTank;
            return mission.Mission.TrackedTechs.Exists(delegate (TrackedTech cand) { return cand.TechName == TechName; });
        }
        public static bool GetTrackedTech(ref SubMissionStep mission, string TechName, out Tank tech)
        {
            if (TechName == "Player Tech")
            {
                tech = Singleton.playerTank;
                return Singleton.playerTank;
            }
            tech = mission.Mission.TrackedTechs.Find(delegate (TrackedTech cand) { return cand.TechName == TechName; }).Tech;
            if (tech == null)
                return false;
            return true;
        }
        public static TrackedTech GetTrackedTechBase(ref SubMissionStep mission, string TechName)
        {
            if (TechName == "Player Tech")
            {
                Assert(true, "SubMissions: GetTrackedTechBase - Called for \"Player Tech\", but the player tech is not trackable!");
            }
            return mission.Mission.TrackedTechs.Find(delegate (TrackedTech cand) { return cand.TechName == TechName; });
        }
        public static TrackedTech GetTrackedTechBase(ref SubMission mission, string TechName)
        {
            return mission.TrackedTechs.Find(delegate (TrackedTech cand) { return cand.TechName == TechName; });
        }
        public static Tank GetTrackedTech(ref SubMission mission, string TechName)
        {
            return mission.TrackedTechs.Find(delegate (TrackedTech cand) { return cand.TechName == TechName; }).Tech;
        }
        public static void SetTrackedTech(ref SubMission mission, Tank tech)
        {
            mission.TrackedTechs.Find(delegate (TrackedTech cand) { return cand.TechName == tech.name; }).Tech = tech;
        }


    }
}
