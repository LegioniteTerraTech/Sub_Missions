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
using TerraTechETCUtil;
using TAC_AI.AI.Enemy;

namespace Sub_Missions
{
    public static class SMUtil
    {
        internal class ErrorQueue : Queue<ErrorElement>
        {
            private const int MaxLimit = 64;

            private static StringBuilder SB = new StringBuilder();
            public static ErrorQueue operator +(ErrorQueue a, string b)
            {
                a.Enqueue(new ErrorElementString(b));
                return a;
            }
            public static ErrorQueue operator +(ErrorQueue a, ErrorElement b)
            {
                a.Enqueue(b);
                return a;
            }
            public static ErrorQueue operator -(ErrorQueue a, string b)
            {
                a.Clear();
                a.Enqueue(new ErrorElementString(b));
                return a;
            }

            private new void Enqueue(ErrorElement ele)
            {
                base.Enqueue(ele);
                if (Count > MaxLimit)
                    Dequeue();
            }

            public void DispenseGUI()
            {
                foreach (var item in this)
                {
                    item.GUICall();
                }
            }

            public override string ToString()
            {
                SB.Clear();
                foreach (var item in this)
                {
                    SB.AppendLine(item.ToString());
                }
                return SB.ToString();
            }
        }

        public static Tank PlayerTank => Singleton.playerTank;

        private static bool spamLog = true;

        public static bool errorQueued = false;
        public static bool collectedErrors = false;
        public static bool collectedLogs = false;
        public static bool collectedInfos = false;
        private static ErrorQueue errorList = new ErrorQueue();
        internal static ErrorQueue Errors => errorList;
        public static int countError = 0;
        private static bool repeatingInfo = false;
        private static bool repeatingLog = false;
        private static bool repeatingErrored = false;
        private static bool repeatingAssert = false;

        // DEBUG
        public static void ClearErrors()
        {
            errorList.Clear();
            errorQueued = false;
            countError = 0;
            repeatingInfo = false;
            repeatingLog = false;
            repeatingErrored = false;
            repeatingAssert = false;

            collectedErrors = false;
            collectedInfos = false;
            collectedLogs = false;
        }
        public static void PushErrors()
        {
            if (KickStart.Debugger)
            {
                if (errorQueued)
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.MissionFailed);
                    WindowManager.AddPopupMessageError("SubMissions Error", Errors);
                    WindowManager.ShowPopup(new Vector2(0.5f, 0.5f));
                    repeatingInfo = false;
                    repeatingLog = false;
                    repeatingErrored = false;
                    repeatingAssert = false;
                }
                else
                {
                    WindowManager.AddPopupMessage("SubMissions Error", "No errors reported.");
                    WindowManager.ShowPopup(new Vector2(0.5f, 0.5f));
                }
            }
        }
        public static void Info(bool logSpammer, string name, string input)
        {
            if (KickStart.Debugger && !repeatingInfo)
            {
                try
                {
                    collectedInfos = true;
                    if (input.NullOrEmpty())
                    {
                        input = "Sub_Missions.SMUtil.Info has no information set for this edge case error, please notify Legionite (or some SubMissions maintainer) to add an assert. " +
                                "\n Thanks!" +
                                "\n Unresolvable error! - No automatic debugging context was thrown in for this edge case!" +
                                "\n At " + StackTraceUtility.ExtractStackTrace();
                        logSpammer = true;
                    }
                    else
                    {
                        if (input.Contains(KickStart.ModID + ": "))
                        {
                            countError++;
                            input.Replace(KickStart.ModID + ": ", "<b>Error " + countError + "</b>: ");
                        }
                    }
                    if (logSpammer)
                    {
                        errorList += new ErrorElementInfo(name, "<b>------ REPEATING ------<b>\n" + input);
                        repeatingInfo = true;
                    }
                    else
                        errorList += new ErrorElementInfo(name, input);
                    if (spamLog)
                        Debug_SMissions.Log(input);
                }
                catch (Exception e)
                {
                    errorList -= "SMUtil.Info - Error collector failed, please contact Legionite (or some SubMissions maintainer).";
                    Debug_SMissions.Log(errorList.ToString() + e);
                }
                errorQueued = true;
            }
        }
        public static void Log(bool logSpammer, string input)
        {
            if (KickStart.Debugger && !repeatingLog)
            {
                try
                {
                    collectedLogs = true;
                    if (input.NullOrEmpty())
                    {
                        input = "Sub_Missions.SMUtil.Log has no information set for this edge case error, please notify Legionite (or some SubMissions maintainer) to add an assert. " +
                                "\n Thanks!" +
                                "\n Unresolvable error! - No automatic debugging context was thrown in for this edge case!" +
                                "\n At " + StackTraceUtility.ExtractStackTrace();
                        logSpammer = true;
                    }
                    if (logSpammer)
                    {
                        errorList += "<b>------ REPEATING LOG ------<b>\n" + input;
                        repeatingLog = true;
                    }
                    else
                        errorList += input;
                    if (spamLog) 
                        Debug_SMissions.Log(input);
                }
                catch (Exception e)
                {
                    errorList -= "SMUtil.Log - Error collector failed, please contact Legionite (or some SubMissions maintainer).";
                    Debug_SMissions.Log(errorList.ToString() + e);
                }
                errorQueued = true;
            }
        }
        public static void Error(bool logSpammer, string name, string input)
        {
            if (KickStart.Debugger && !repeatingErrored)
            {
                try
                {
                    collectedErrors = true;
                    if (input.NullOrEmpty())
                    {
                        input = "Sub_Missions.SMUtil.Error has no information set for this edge case error, please notify Legionite (or some SubMissions maintainer) to add an assert. " +
                                "\n Thanks!" +
                                "\n Unresolvable error! - No automatic debugging context was thrown in for this edge case!" +
                                "\n At " + StackTraceUtility.ExtractStackTrace();
                        logSpammer = true;
                    }
                    else
                    {
                        if (input.Contains(KickStart.ModID + ": "))
                        {
                            countError++;
                            input.Replace(KickStart.ModID + ": ", "<b>Error " + countError + "</b>: ");
                        }
                    }
                    if (logSpammer)
                    {
                        errorList += new ErrorElementError(name, "<b>------ REPEATING ERROR ------<b>\n" + input);
                        repeatingErrored = true;
                    }
                    else
                        errorList += new ErrorElementError(name, input);
                    if (spamLog) 
                        Debug_SMissions.Log(input);
                }
                catch (Exception e)
                {
                    errorList -= "SMUtil.Error - Error collector failed, please contact Legionite (or some SubMissions maintainer).";
                    Debug_SMissions.Log(errorList.ToString() + e);
                }
                errorQueued = true;
            }
        }
        
        private static StringBuilder SB = new StringBuilder();
        public static void Assert(bool logSpammer, string name, string input, Exception ex)
        {
            if (KickStart.Debugger && !repeatingAssert)
            {
                try
                {
                    if (input.NullOrEmpty())
                    {
                        input = "Sub_Missions.SMUtil.Assert has no information set for an edge case, please notify Legionite (or some SubMissions maintainer) to add an assert. " +
                                "\n Thanks!" +
                                "\n Unresolvable error! - No automatic debugging context was thrown in for this edge case!" +
                                "\n At " + StackTraceUtility.ExtractStackTrace();
                        logSpammer = true;
                    }
                    else
                    {
                        if (input.Contains(KickStart.ModID + ": "))
                        {
                            countError++;
                            input.Replace(KickStart.ModID + ": ", "<b>Error " + countError + "</b>: ");
                        }
                    }
                    if (logSpammer)
                    {
                        SB.AppendLine("<b>------ CASCADE FAILIURE ------<b> ");
                        repeatingAssert = true;
                    }
                    SB.AppendLine(input);
                    if (spamLog)
                    {
                        Debug_SMissions.Log(input);
                        Debug_SMissions.Log(ex);
                    }
                    errorList += new ErrorElementAssert(name, SB.ToString(), ex);
                    SB.Clear();
                }
                catch (Exception e)
                {
                    errorList -= "SMUtil.Assert - Error collector failed, please contact Legionite (or some SubMissions maintainer).";
                    Debug_SMissions.Log(errorList.ToString() + e);
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
        public static Vector3 RealignWithTerrain(ref Vector3 input, Vector3 missionOriginScene, int terrainHanding)
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
                    input += missionOriginScene;
                    input = VectorOnTerrain(input, input.y);
                    break;
                case 3:
                    input += missionOriginScene;
                    input = VectorOnTerrain(input, 0);
                    break;
                default:
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
            Debug_SMissions.Log(KickStart.ModID + ": ProceedID - Mission " + Step.Mission.Name + " has moved on to ID " + Step.Mission.CurrentProgressID);
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
            catch (IndexOutOfRangeException e)
            {
                Assert(true, Step.LogName, KickStart.ModID + ": Error in output [SetMissionVarIndex1] in mission " + Step.Mission.Name +
                    " | Step type " + Step.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) " +
                    "\n and make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry " +
                    "on the list, 1 counts as the second entry, and so on.", e);
            }
            catch (NullReferenceException e)
            {
                Assert(true, Step.LogName, KickStart.ModID + ": Error in output [SetMissionVarIndex1] in mission " + Step.Mission.Name +
                    " | Step type " + Step.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) " +
                    "\n and make sure your referencing an entry you have declared in VarInts or varTrueFalse, depending" +
                    " on the step's set VaribleType.", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException(e);
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
            catch (IndexOutOfRangeException e)
            {
                Assert(true, Step.LogName, KickStart.ModID + ": Error in output [SetMissionVarIndex2] in mission " + Step.Mission.Name +
                    " | Step type " + Step.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) " +
                    "\n and make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry " +
                    "on the list, 1 counts as the second entry, and so on.", e);
            }
            catch (NullReferenceException e)
            {
                Assert(true, Step.LogName, KickStart.ModID + ": Error in output [SetMissionVarIndex2] in mission " + Step.Mission.Name +
                    " | Step type " + Step.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) " +
                    "\n and make sure your referencing an entry you have declared in VarInts or varTrueFalse, depending" +
                    " on the step's set VaribleType.", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException(e);
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
            catch (IndexOutOfRangeException e)
            {
                Assert(true, Step.LogName, KickStart.ModID + ": Error in output [SetMissionVarIndex3] in mission " + Step.Mission.Name +
                    " | Step type " + Step.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) " +
                    "\n and make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry " +
                    "on the list, 1 counts as the second entry, and so on.", e);
            }
            catch (NullReferenceException e)
            {
                Assert(true, Step.LogName, KickStart.ModID + ": Error in output [SetMissionVarIndex3] in mission " + Step.Mission.Name +
                    " | Step type " + Step.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) " +
                    "\n and make sure your referencing an entry you have declared in VarInts or varTrueFalse, depending" +
                    " on the step's set VaribleType.", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException(e);
            }
        }
        public static void HandleVariables(ref SubMissionStep Step, int setVal)
        {
            switch (Step.VaribleType)
            {
                case EVaribleType.Int: //
                    if (setVal < 0)
                        return; // that means it's not being used
                    Step.Mission.VarIntsActive[setVal] = (int)Step.VaribleCheckNum;
                    break;
                case EVaribleType.False: //
                    if (setVal < 0)
                        return; // that means it's not being used
                    Step.Mission.VarTrueFalseActive[setVal] = false;
                    break;
                case EVaribleType.True: //
                    if (setVal < 0)
                        return; // that means it's not being used
                    Step.Mission.VarTrueFalseActive[setVal] = true;
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
                        return Step.Mission.VarIntsActive[GlobalIndex] == (int)Step.VaribleCheckNum;
                    case EVaribleType.IntGreaterThan: //
                        return Step.Mission.VarIntsActive[GlobalIndex] > (int)Step.VaribleCheckNum;
                    case EVaribleType.IntLessThan: //
                        return Step.Mission.VarIntsActive[GlobalIndex] < (int)Step.VaribleCheckNum;
                    case EVaribleType.False: //
                        return Step.Mission.VarTrueFalseActive[GlobalIndex] == false;
                    case EVaribleType.True: //
                        return Step.Mission.VarTrueFalseActive[GlobalIndex] == true;
                    case EVaribleType.None: // 
                    default:
                        return true;
                }
            }
            catch (IndexOutOfRangeException e)
            {
                Assert(true, Step.LogName, KickStart.ModID + ": Error in output [SetMissionVarIndex1] or [SetMissionVarIndex2] or " +
                    "[SetMissionVarIndex3] in mission " + Step.Mission.Name +
                    " | Step type " + Step.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) " +
                    "\n and make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry " +
                    "on the list, 1 counts as the second entry, and so on.", e);
            }
            catch (NullReferenceException e)
            {
                Assert(true, Step.LogName, KickStart.ModID + ": Error in output [SetMissionVarIndex1] or [SetMissionVarIndex2] or " +
                    "[SetMissionVarIndex3] in mission " + Step.Mission.Name +
                    " | Step type " + Step.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) " +
                    "\n and make sure your referencing an entry you have declared in VarInts or varTrueFalse, depending" +
                    " on the step's set VaribleType.", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException(e);
            }
            return false;
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
        /// <param name="FileTechName"></param>
        /// <returns></returns>
        public static Tank SpawnTechAuto(ref SubMission mission, Vector3 pos, int Team, 
            Vector3 facingDirect, string FileTechName)
        {
            // Load from folder
            if (mission.Tree.TreeTechs.TryGetValue(FileTechName, out SpawnableTech val))
            {   // Supports normal snapshots
                Tank tech = val.Spawn(mission, pos, facingDirect, Team);
                if (Team == ManSpawn.NeutralTeam)
                    tech.SetInvulnerable(true, true);
                return tech;
            }

            return null;
        }


        public static string SpawnTechTracked(ref SubMission mission, Vector3 pos, int Team, 
            Vector3 facingDirect, string FileTechName, bool instant = false)
        {   // We pull these from MissionTechs.json
            Tank tech;
            if (instant)
            {
                if (mission.Tree.TreeTechs.TryGetValue(FileTechName, out SpawnableTech val))
                {   // Supports normal snapshots
                    tech = val.Spawn(mission, pos, facingDirect, Team);
                    SetTrackedTech(ref mission, tech);
                    if (Team == ManSpawn.NeutralTeam)
                        tech.SetInvulnerable(true, true);
                    return FileTechName;
                }
            }
            else
            {
                TrackedTech techCase = GetTrackedTechBase(ref mission, FileTechName);
                techCase.delayedSpawn = ManSpawn.inst.SpawnDeliveryBombNew(pos, DeliveryBombSpawner.ImpactMarkerType.Tech);
                techCase.delayedSpawn.BombDeliveredEvent.Subscribe(techCase.SpawnTech);
                techCase.DeliQueued = true;
            }
            return FileTechName;
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
        public static void SpawnTechAddTracked(ref SubMission mission, Vector3 pos, int Team, 
            Vector3 facingDirect, string FileTechName, bool instant = false)
        {   // We pull these from MissionTechs.json
            if (FileTechName.NullOrEmpty())
                throw new NullReferenceException("The tech name is not valid.  It must be specified");
            if (!mission.Tree.TreeTechs.TryGetValue(FileTechName, out var ST))
                throw new NullReferenceException("The tech " + FileTechName + " is not in the Mission Tree of the " +
                    "hosting mission");
            TrackedTech tech = new TrackedTech(ST.name, FileTechName, true);
            tech.mission = mission;
            if (instant)
            {
                tech.TechAuto = ST.Spawn(mission, pos, facingDirect, Team);
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
        public static bool DoesTrackedTechExist(ref SubMissionStep mission, string FileTechName)
        {
            if (FileTechName == "Player Tech")
                return Singleton.playerTank;
            return mission.Mission.TrackedTechs.Exists(delegate (TrackedTech cand) { return cand.FileTechName == FileTechName; });
        }
        public static bool GetTrackedTech(ref SubMissionStep mission, string FileTechName, out Tank tech)
        {
            if (FileTechName == "Player Tech")
            {
                tech = Singleton.playerTank;
                return Singleton.playerTank;
            }
            tech = mission.Mission.TrackedTechs.Find(delegate (TrackedTech cand) { return cand.FileTechName == FileTechName; }).TechAuto;
            if (tech == null)
                return false;
            return true;
        }
        public static TrackedTech GetTrackedTechBase(ref SubMissionStep mission, string FileTechName)
        {
            if (FileTechName == "Player Tech")
            {
                Error(true, "Mission(TrackedTech) ~ " + mission.Mission.Name + ", Step " + mission.StepType + " ~ " + 
                    mission.ProgressID,  KickStart.ModID + ": GetTrackedTechBase - Called for \"Player Tech\", but the player tech is not trackable!");
            }
            return mission.Mission.TrackedTechs.Find(delegate (TrackedTech cand) { return cand.FileTechName == FileTechName; });
        }
        public static bool GetTrackedTechBase(ref SubMissionStep mission, string FileTechName, out TrackedTech tech)
        {
            tech = GetTrackedTechBase(ref mission, FileTechName);
            return tech != null;
        }
        public static TrackedTech GetTrackedTechBase(ref SubMission mission, string FileTechName)
        {
            return mission.TrackedTechs.Find(delegate (TrackedTech cand) { return cand.FileTechName == FileTechName; });
        }
        public static Tank GetTrackedTech(ref SubMission mission, string FileTechName)
        {
            return mission.TrackedTechs.Find(delegate (TrackedTech cand) { return cand.FileTechName == FileTechName; }).TechAuto;
        }
        public static void SetTrackedTech(ref SubMission mission, Tank tech)
        {
            mission.TrackedTechs.Find(delegate (TrackedTech cand) { return cand.TechName == tech.name; }).TechAuto = tech;
        }
        public static void EnforceTrackedTech(ref SubMission mission, Tank tech, string FileTechName)
        {
            TrackedTech TT = mission.TrackedTechs.Find(delegate (TrackedTech cand) { return cand.TechName == tech.name; });
            if (TT == null)
            {
                mission.TrackedTechs.Add(new TrackedTech(tech.name, FileTechName, tech.IsPopulation));
            }
        }


        // Visuals
        public static void ShowModelsApprox(GameObject GO, Vector3 InWorldSpace, Quaternion rot, Vector3 scale)
        {
            ShowModels_Internal(GO, Matrix4x4.TRS(InWorldSpace, rot, scale));
        }
        private static void ShowModels_Internal(GameObject GO, Matrix4x4 matrix)
        {
            MeshFilter MF = GO.GetComponent<MeshFilter>();
            MeshRenderer MR = GO.GetComponent<MeshRenderer>();
            if (MF && MR)
            {
                Mesh mesh = MF.sharedMesh;
                Material mat = MR.sharedMaterial;
                if (mesh && mat)
                {
                    Graphics.DrawMesh(mesh, matrix, mat, 0);
                }
            }
            var trans = GO.transform;
            for (int i = 0; i < trans.childCount; i++)
            {
                var child = trans.GetChild(i);
                ShowModels_Internal(child.gameObject, matrix * Matrix4x4.TRS(child.localPosition,
                    child.localRotation, child.localScale));
            }
        }


        // ETC
        private static TechData cachedTechInfo = null;
        public static TechData DisplaySelectTechInfo => cachedTechInfo;
        private static Texture2D cachedTechPicture = Texture2D.whiteTexture;
        public static Texture2D DisplaySelectTechImage => cachedTechPicture;
        private static string cachedTechName = "";
        public static string DisplaySelectTechName => cachedTechName;
        private static UIScreenTechLoader loader;
        public static string GUIDisplaySelectTech()
        {
            if (cachedTechPicture != null)
                GUILayout.Label(cachedTechPicture);
            GUILayout.Label(cachedTechName == null ? "<color=red>Not Set</color>" : cachedTechName);
            if (GUILayout.Button("Select"))
            {
                if (loader == null)
                {
                    loader = (UIScreenTechLoader)ManUI.inst.GetScreen(ManUI.ScreenType.TechLoaderScreen);
                    if (loader.SelectorCallback != null)
                        throw new Exception("SMSFieldTechSelectGUI called while UIScreenTechLoader was already busy in an operation");

                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Button);
                    loader.SelectorCallback = OnTechSet;
                    loader.Show(true);
                }
            }
            return cachedTechName;
        }
        private static void OnTechSet(Snapshot set)
        {
            if (loader.SelectorCallback != OnTechSet)
                throw new Exception("UIScreenTechLoader was altered while SMUtil.GUIDisplaySelectTech() was busy using it");
            cachedTechPicture = set.image;
            cachedTechName = set.techData.Name;
            cachedTechInfo = set.techData.GetShallowClonedCopy();
            loader.SelectorCallback = null;
            loader.Hide();
            loader = null;
            ManSFX.inst.PlayUISFX(ManSFX.UISfxType.LevelUp);
        }
    }
}
