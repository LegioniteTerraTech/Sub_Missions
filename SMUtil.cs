﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;
using Sub_Missions.Steps;

namespace Sub_Missions
{
    public static class SMUtil
    {
        public static Tank PlayerTank
        { 
            get => Singleton.playerTank;
        }

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
            switch (Step.VaribleType)
            {
                case EVaribleType.Int: //
                    Step.Mission.VarInts[Step.SetGlobalIndex1] = (int)Step.InputNum;
                    break;
                case EVaribleType.False: //
                    Step.Mission.VarTrueFalse[Step.SetGlobalIndex1] = false;
                    break;
                case EVaribleType.True: //
                    Step.Mission.VarTrueFalse[Step.SetGlobalIndex1] = true;
                    break;
                case EVaribleType.DoSuccessID: // 
                    ProceedID(ref Step);
                    break;
                case EVaribleType.None: // 
                default:
                    break;
            }
        }
        public static void ConcludeGlobal2(ref SubMissionStep Step)
        {
            switch (Step.VaribleType)
            {
                case EVaribleType.Int: //
                    Step.Mission.VarInts[Step.SetGlobalIndex2] = (int)Step.InputNum;
                    break;
                case EVaribleType.False: //
                    Step.Mission.VarTrueFalse[Step.SetGlobalIndex2] = false;
                    break;
                case EVaribleType.True: //
                    Step.Mission.VarTrueFalse[Step.SetGlobalIndex2] = true;
                    break;
                case EVaribleType.DoSuccessID: // 
                    ProceedID(ref Step);
                    break;
                case EVaribleType.None: // 
                default:
                    break;
            }
        }
        public static void ConcludeGlobal3(ref SubMissionStep Step)
        {
            switch (Step.VaribleType)
            {
                case EVaribleType.Int: //
                    Step.Mission.VarInts[Step.SetGlobalIndex3] = (int)Step.InputNum;
                    break;
                case EVaribleType.False: //
                    Step.Mission.VarTrueFalse[Step.SetGlobalIndex3] = false;
                    break;
                case EVaribleType.True: //
                    Step.Mission.VarTrueFalse[Step.SetGlobalIndex3] = true;
                    break;
                case EVaribleType.DoSuccessID: // 
                    ProceedID(ref Step);
                    break;
                case EVaribleType.None: // 
                default:
                    break;
            }
        }

        public static bool BoolOut(ref SubMissionStep Step)
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


        public static Tank SpawnTechAuto(ref CustomSubMission mission, Vector3 pos, int Team, Vector3 facingDirect, string TechName)
        {   // Load from folder
            return RawTechLoader.SpawnTechExternal(pos, Team, facingDirect, RawTechExporter.LoadTechFromRawJSON(TechName, "Custom SMissions\\" + mission.Name + "\\Raw Techs"));
        }
        public static string SpawnTechTracked(ref CustomSubMission mission, Vector3 pos, int Team, Vector3 facingDirect, string TechName)
        {   // We pull these from MissionTechs.JSON

            //TrackedTech tech = new TrackedTech();
            Tank tech = RawTechLoader.SpawnTechExternal(pos, Team, facingDirect, RawTechExporter.LoadTechFromRawJSON(TechName, "Custom SMissions\\" + mission.Tree.TreeName + "\\Raw Techs"));

            SetTrackedTech(ref mission, tech);
            return tech.name;
        }
        public static string InjectTechTracked(ref CustomSubMission mission, TechData techdata)
        {
            return "error - requires PopulationInjector";
        }
        public static Tank GetTrackedTech(ref SubMissionStep mission, string TechName)
        {
            return mission.Mission.TrackedTechs.Find(delegate (TrackedTech cand) { return cand.TechName == TechName; }).Tech;
        }
        public static Tank GetTrackedTech(ref CustomSubMission mission, string TechName)
        {
            return mission.TrackedTechs.Find(delegate (TrackedTech cand) { return cand.TechName == TechName; }).Tech;
        }
        public static void SetTrackedTech(ref CustomSubMission mission, Tank tech)
        {
            mission.TrackedTechs.Find(delegate (TrackedTech cand) { return cand.TechName == tech.name; }).Tech = tech;
        }
    }
}