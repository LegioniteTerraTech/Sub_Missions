using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;
using Sub_Missions.Steps;
using Sub_Missions.ManWindows;

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
                    if (input.Contains("SubMissions: "))
                    {
                        countError++;
                        input.Replace("SubMissions: ", "<b>Error " + countError + "</b>: ");
                    }
                    if (repeatingError)
                    {
                        errorList += "<b>-MAJOR ERROR-<b> ";
                        repeatingErrored = true;
                    }
                    errorList += input + "\n";
                }
                catch
                {
                    errorList = "Error collector failed, please contact Legionite";
                }
                errorQueued = true;
            }
            Debug.Log(input);
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
        public static Vector3 SetPosTerrain(ref Vector3 input, bool Grounded = true)
        {
            if (Grounded)
                input = VectorOnTerrain(input, 0);
            else
                input = VectorOnTerrain(input, input.y);

            return input;
        }
        

        // PLAYER
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
        }
        public static void ConcludeGlobal1(ref SubMissionStep Step)
        {
            try
            {
                int setVal = Step.SetGlobalIndex1;
                switch (Step.VaribleType)
                {
                    case EVaribleType.Int: //
                        Step.Mission.VarInts[setVal] = (int)Step.InputNum;
                        break;
                    case EVaribleType.False: //
                        Step.Mission.VarTrueFalse[setVal] = false;
                        break;
                    case EVaribleType.True: //
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
            catch
            {
                Assert(true, "SubMissions: Error in output [SetGlobalIndex1] in mission " + Step.Mission.Name + " | Step type " + Step.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) \nand make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry on the list, 1 counts as the second entry, and so on.");
            }
        }
        public static void ConcludeGlobal2(ref SubMissionStep Step)
        {
            try
            {
                int setVal = Step.SetGlobalIndex2;
                switch (Step.VaribleType)
                {
                    case EVaribleType.Int: //
                        Step.Mission.VarInts[setVal] = (int)Step.InputNum;
                        break;
                    case EVaribleType.False: //
                        Step.Mission.VarTrueFalse[setVal] = false;
                        break;
                    case EVaribleType.True: //
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
            catch
            {
                Assert(true, "SubMissions: Error in output [SetGlobalIndex2] in mission " + Step.Mission.Name + " | Step type " + Step.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) \nand make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry on the list, 1 counts as the second entry, and so on.");
            }
        }
        public static void ConcludeGlobal3(ref SubMissionStep Step)
        {
            try
            {
                int setVal = Step.SetGlobalIndex3;
                switch (Step.VaribleType)
                {
                    case EVaribleType.Int: //
                        Step.Mission.VarInts[setVal] = (int)Step.InputNum;
                        break;
                    case EVaribleType.False: //
                        Step.Mission.VarTrueFalse[setVal] = false;
                        break;
                    case EVaribleType.True: //
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
            catch
            {
                Assert(true, "SubMissions: Error in output [SetGlobalIndex3] in mission " + Step.Mission.Name + " | Step type " + Step.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) \nand make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry on the list, 1 counts as the second entry, and so on.");
            }
        }

        public static bool BoolOut(ref SubMissionStep Step)
        {
            try
            {
                switch (Step.VaribleType)
                {
                    case EVaribleType.Int: //
                        return Step.Mission.VarInts[Step.SetGlobalIndex1] == (int)Step.InputNum;
                    case EVaribleType.IntGreaterThan: //
                        return Step.Mission.VarInts[Step.SetGlobalIndex1] > (int)Step.InputNum;
                    case EVaribleType.IntLessThan: //
                        return Step.Mission.VarInts[Step.SetGlobalIndex1] < (int)Step.InputNum;
                    case EVaribleType.False: //
                        return Step.Mission.VarTrueFalse[Step.SetGlobalIndex1] == false;
                    case EVaribleType.True: //
                        return Step.Mission.VarTrueFalse[Step.SetGlobalIndex1] == true;
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
        /// </summary>
        /// <param name="mission"></param>
        /// <param name="pos"></param>
        /// <param name="Team"></param>
        /// <param name="facingDirect"></param>
        /// <param name="TechName"></param>
        /// <returns></returns>
        public static Tank SpawnTechAuto(ref SubMission mission, Vector3 pos, int Team, Vector3 facingDirect, string TechName)
        {   // Load from folder
            return RawTechLoader.SpawnTechExternal(pos, Team, facingDirect, RawTechExporter.LoadTechFromRawJSON(TechName, "Custom SMissions\\" + mission.Tree.TreeName + "\\Raw Techs"));
        }
        public static string SpawnTechTracked(ref SubMission mission, Vector3 pos, int Team, Vector3 facingDirect, string TechName, bool instant = false)
        {   // We pull these from MissionTechs.JSON
            Tank tech;
            if (instant)
            { 
                tech = RawTechLoader.SpawnTechExternal(pos, Team, facingDirect, RawTechExporter.LoadTechFromRawJSON(TechName, "Custom SMissions\\" + mission.Tree.TreeName + "\\Raw Techs"));
                SetTrackedTech(ref mission, tech);
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
        public static void SpawnTechAddTracked(ref SubMission mission, Vector3 pos, int Team, Vector3 facingDirect, string TechName, bool instant = false)
        {   // We pull these from MissionTechs.JSON
            TrackedTech tech = new TrackedTech();
            if (instant)
                tech.Tech = RawTechLoader.SpawnTechExternal(pos, Team, facingDirect, RawTechExporter.LoadTechFromRawJSON(TechName, "Custom SMissions\\" + mission.Tree.TreeName + "\\Raw Techs"));
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
            return mission.Mission.TrackedTechs.Exists(delegate (TrackedTech cand) { return cand.TechName == TechName; });
        }
        public static bool GetTrackedTech(ref SubMissionStep mission, string TechName, out Tank tech)
        {
            tech = mission.Mission.TrackedTechs.Find(delegate (TrackedTech cand) { return cand.TechName == TechName; }).Tech;
            if (tech == null)
                return false;
            return true;
        }
        public static TrackedTech GetTrackedTechBase(ref SubMissionStep mission, string TechName)
        {
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
