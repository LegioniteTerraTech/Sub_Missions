using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;

namespace Sub_Missions
{
    /// <summary>
    /// Flag this as a "fake" encounter since dummy encounter saving is done on this mod's end
    /// </summary>
    internal class DenySave : MonoBehaviour
    {
    }

    /// <summary>
    /// Holding off on trying to integrate this mod (yes that's what this intends to do)
    /// with the vanilla missions system since it's a nightmare to navigate
    /// </summary>
    internal class EncounterShoehorn : MonoBehaviour
    {
        private static readonly FieldInfo
            obsticles = FIFetch(typeof(EncounterDetails), "m_Objectives"),
            Title = FIFetch(typeof(EncounterDetails), "m_TitleStringID"),
            Desc = FIFetch(typeof(EncounterDetails), "m_FullDescriptionStringID"),
            Timed = FIFetch(typeof(EncounterDetails), "m_IsTimed"),

            AwardXP = FIFetch(typeof(EncounterDetails), "m_AwardXP"),
            XPAmou = FIFetch(typeof(EncounterDetails), "m_XPAmount"),
            XPTarg = FIFetch(typeof(EncounterDetails), "m_XPCorp"),

            AwardBB = FIFetch(typeof(EncounterDetails), "m_AwardBB"),
            BBAmou = FIFetch(typeof(EncounterDetails), "m_BBAmount"),

            AwardBloc = FIFetch(typeof(EncounterDetails), "m_AwardBlocks"),
            BlocSet = FIFetch(typeof(EncounterDetails), "m_BlocksToAward"),
            BlocPool = FIFetch(typeof(EncounterDetails), "m_RewardFromCorpPool"),
            BlocPoolAmou = FIFetch(typeof(EncounterDetails), "m_AmountToAwardFromPool"),
            BlocPoolLice = FIFetch(typeof(EncounterDetails), "m_RewardPoolCorp"),

            AwardLice = FIFetch(typeof(EncounterDetails), "m_AwardLicense"),
            liceType = FIFetch(typeof(EncounterDetails), "m_CorpLicenseToAward"),
            track = FIFetch(typeof(EncounterDetails), "m_AnalyticsMissionType"),

            popDis = FIFetch(typeof(Encounter), "m_DisablesPopulation"),
            questLog = FIFetch(typeof(Encounter), "m_QuestLog"),
            RadU = FIFetch(typeof(Encounter), "m_EncounterRadius");

        internal static Dictionary<string, GameObject> FakeEncounters = new Dictionary<string, GameObject>();
        private static Encounter GetFakeEncounterInternal(string name, out EncounterDetails EDl, bool New = false)
        {
            string searchTerm = "temp - " + name;
            if (FakeEncounters.TryGetValue(searchTerm, out GameObject val))
            {
                if (New)
                {
                    FakeEncounters.Remove(searchTerm);
                }
                else
                {
                    EDl = val.GetComponent<EncounterDetails>();
                    return val.GetComponent<Encounter>();
                }
            }
            GameObject temp = new GameObject("temp - " + name);
            temp.AddComponent<DenySave>();
            EDl = temp.AddComponent<EncounterDetails>();
            Encounter dummyEmpty = temp.AddComponent<Encounter>();
            return dummyEmpty;
        }
        internal static void DestroyAllFakeEncounters()
        {
            foreach (var item in FakeEncounters)
            {
                Destroy(item.Value, 0.01f);
            }
            FakeEncounters.Clear();
        }

        internal static Encounter GetFakeEncounter(SubMission mission, out EncounterDetails EDl, bool New = false)
        {
            Encounter dummyEmpty = GetFakeEncounterInternal(mission.Name, out EDl, New);

            int errorl = 0;
            try
            {
                errorl++;
                int tyopes = typeof(EncounterDetails).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).Length;
                Debug.Log("There are " + tyopes + " nestedTypes in EncounterDetails");

                errorl++;
                Type objective = typeof(EncounterDetails).GetNestedType("Objective", BindingFlags.NonPublic | BindingFlags.Instance);

                errorl++;
                Array array = Array.CreateInstance(objective, 0);

                errorl++;
                obsticles.SetValue(EDl, array);

                Title.SetValue(EDl, mission.Name);
                Desc.SetValue(EDl, mission.Description);
                Timed.SetValue(EDl, false);

                FactionSubTypes FST = SubMissionTree.GetTreeCorp(mission.Rewards.CorpToGiveEXP);
                if (mission.Rewards.EXPGain > 0)
                {
                    if (Singleton.Manager<ManLicenses>.inst.IsLicenseDiscovered(FST))
                    {
                        AwardXP.SetValue(EDl, true);
                        AwardLice.SetValue(EDl, false);
                        XPAmou.SetValue(EDl, mission.Rewards.EXPGain);
                        XPTarg.SetValue(EDl, FST);
                    }
                    else
                    {
                        AwardXP.SetValue(EDl, false);
                        AwardLice.SetValue(EDl, true);
                        liceType.SetValue(EDl, FST);
                    }
                }
                else
                { 
                    AwardXP.SetValue(EDl, false);
                    AwardLice.SetValue(EDl, false);
                }

                if (mission.Rewards.MoneyGain > 0)
                {
                    AwardBB.SetValue(EDl, true);
                    BBAmou.SetValue(EDl, mission.Rewards.MoneyGain);
                }
                else
                    AwardBB.SetValue(EDl, false);

                bool randBlocks = mission.Rewards.RandomBlocksToSpawn > 0;
                if (randBlocks || mission.Rewards.BlocksToSpawn.Count > 0)
                {
                    AwardBloc.SetValue(EDl, true);
                    BlocSet.SetValue(EDl, mission.Rewards.BlocksToSpawn.ToArray());
                    BlocPool.SetValue(EDl, randBlocks);
                    BlocPoolAmou.SetValue(EDl, mission.Rewards.RandomBlocksToSpawn);
                    BlocPoolLice.SetValue(EDl, FST);
                }
                else
                    AwardBloc.SetValue(EDl, false);

                track.SetValue(EDl, EncounterDetails.AnalyticsMissionType.DoNotTrack);
            }
            catch
            {
                Debug.LogError("Error on " + errorl);
            }

            popDis.SetValue(dummyEmpty, false);
            RadU.SetValue(dummyEmpty, mission.GetMinimumLoadRange());

            return dummyEmpty;
        }
        internal static Encounter GetFakeEncounter(SubMissionStandby mission, out EncounterDetails EDl, bool New = false)
        {
            Encounter dummyEmpty = GetFakeEncounterInternal(mission.Name, out EDl, New);

            int error = 0;
            try
            {
                Type objective = typeof(EncounterDetails).GetNestedType("Objective", BindingFlags.NonPublic | BindingFlags.Instance);

                error++;
                FieldInfo objvDesc = objective.GetField("m_DescriptionStringID", BindingFlags.NonPublic | BindingFlags.Instance);
                error++;
                FieldInfo objvShow = objective.GetField("m_ShowByDefault", BindingFlags.NonPublic | BindingFlags.Instance);
                error++;
                FieldInfo objvCount = objective.GetField("m_TargetCount", BindingFlags.NonPublic | BindingFlags.Instance);

                error++;
                Array array = Array.CreateInstance(objective, 0);

                error++;// 5
                obsticles.SetValue(EDl, array);

                error++;
                Title.SetValue(EDl, mission.Name);
                Desc.SetValue(EDl, mission.Desc);
                Timed.SetValue(EDl, false);

                error++;
                if (mission.Rewards != null)
                {
                    FactionSubTypes FST = SubMissionTree.GetTreeCorp(mission.Rewards.CorpToGiveEXP);
                    if (mission.Rewards.EXPGain > 0)
                    {
                        if (Singleton.Manager<ManLicenses>.inst.IsLicenseDiscovered(FST))
                        {
                            AwardXP.SetValue(EDl, true);
                            AwardLice.SetValue(EDl, false);
                            XPAmou.SetValue(EDl, mission.Rewards.EXPGain);
                            XPTarg.SetValue(EDl, FST);
                        }
                        else
                        {
                            AwardXP.SetValue(EDl, false);
                            AwardLice.SetValue(EDl, true);
                            liceType.SetValue(EDl, FST);
                        }
                    }
                    else
                    {
                        AwardXP.SetValue(EDl, false);
                        AwardLice.SetValue(EDl, false);
                    }

                    error++;
                    if (mission.Rewards.MoneyGain > 0)
                    {
                        AwardBB.SetValue(EDl, true);
                        BBAmou.SetValue(EDl, mission.Rewards.MoneyGain);
                    }
                    else
                        AwardBB.SetValue(EDl, false);

                    error++;
                    bool randBlocks = mission.Rewards.RandomBlocksToSpawn > 0;
                    if (randBlocks || mission.Rewards.BlocksToSpawn.Count > 0)
                    {
                        AwardBloc.SetValue(EDl, true);
                        BlocSet.SetValue(EDl, mission.Rewards.BlocksToSpawn.ToArray());
                        BlocPool.SetValue(EDl, randBlocks);
                        BlocPoolAmou.SetValue(EDl, mission.Rewards.RandomBlocksToSpawn);
                        BlocPoolLice.SetValue(EDl, FST);
                    }
                    else
                        AwardBloc.SetValue(EDl, false);
                }

                error++;// 10
                track.SetValue(EDl, EncounterDetails.AnalyticsMissionType.DoNotTrack);

                int checkLength = mission.Checklist.Count;
                error++;// 11
                array = Array.CreateInstance(objective, checkLength);
                for (int step = 0; step < checkLength; step++)
                {
                    try
                    {
                        MissionChecklist ele = mission.Checklist[step];
                        object objectiveCase = Activator.CreateInstance(objective);
                        objvDesc.SetValue(objectiveCase, ele.ListArticle);
                        objvShow.SetValue(objectiveCase, ele.BoolToEnable != 0);
                        if (ele.ValueType == VarType.IntOverInt)
                            objvCount.SetValue(objectiveCase, ele.GlobalIndex);
                        else
                            objvCount.SetValue(objectiveCase, 0);
                        array.SetValue(objective, step);
                    }
                    catch
                    {
                        Debug.Assert(true, "FAILED TO REBUILD " + mission.Name + " MISSION CHECKLIST ON STEP " + step);
                    }
                }
                error++;
                obsticles.SetValue(EDl, array);
                popDis.SetValue(dummyEmpty, false);

                //EncounterIdentifier EI = new EncounterIdentifier(SubMissionTree.GetTreeCorp(mission.Faction), mission.GradeRequired, mission.Name, mission.Name);
                //QuestLogData QLD = new QuestLogData(dummyEmpty);
                //questLog.SetValue(dummyEmpty);

                RadU.SetValue(dummyEmpty, mission.LoadRadius);
            }
            catch {
                Debug.LogError("EROOR IN ACTIONS - level " + error);
            }
            return dummyEmpty;
        }

        internal static void FinishSubMission(SubMission mission, ManEncounter.FinishState finish)
        {
            try
            {
                Encounter dummyEmpty = GetFakeEncounterInternal(mission.Name, out _);
                if (dummyEmpty)
                {
                    ManEncounter.inst.FinishEncounter(dummyEmpty, finish);
                }
            }
            catch { }
        }

        private static readonly FieldInfo 
            live = FIFetch(typeof(EncounterDisplayData), "m_HasLiveData"),
            enc = FIFetch(typeof(EncounterDisplayData), "m_Encounter"),
            ID = FIFetch(typeof(EncounterDisplayData), "m_Identifier"),
            det = FIFetch(typeof(EncounterDisplayData), "m_EncounterReadonlyDetails"),
            nam = FIFetch(typeof(EncounterDisplayData), "m_EncounterStringBankIdx"),
            log = FIFetch(typeof(EncounterDisplayData), "m_ActiveQuestLog"),
            spw = FIFetch(typeof(EncounterDisplayData), "m_EncounterToSpawn"),
            posB = FIFetch(typeof(EncounterDisplayData), "m_EncounterHasPosition"),
            pos = FIFetch(typeof(EncounterDisplayData), "m_EncounterPosition"),
            obj = FIFetch(typeof(EncounterDisplayData), "m_EncounterObjectives"),
            can = FIFetch(typeof(EncounterDisplayData), "m_CanBeCancelled"),

            encIDCat = FIFetch(typeof(EncounterIdentifier), "m_Category"),

            manMod = FIFetch(typeof(ManEncounter), "m_EncounterDataLookup");



        internal static EncounterDisplayData GetEncounterDisplayInfo(SubMission mission)
        {
            int titleStringID = -100001;
            Encounter encounter = GetFakeEncounter(mission, out EncounterDetails EDl);

            EncounterIdentifier EI = new EncounterIdentifier(mission.FactionType, mission.GradeRequired, mission.Name, mission.Name);
            EncounterDisplayData EDD = new EncounterDisplayData();
            EncounterData ED = GetEncounterData(mission, encounter);

            live.SetValue(EDD, false);
            enc.SetValue(EDD, encounter);
            ID.SetValue(EDD, EI);
            det.SetValue(EDD, ED);
            nam.SetValue(EDD, titleStringID);
            log.SetValue(EDD, new QuestLogData(EI, EDl, titleStringID));
            spw.SetValue(EDD, new EncounterToSpawn(ED, EI));
            posB.SetValue(EDD, !mission.IgnorePlayerProximity);
            pos.SetValue(EDD, mission.WorldPos);
            obj.SetValue(EDD, new QuestLogData.EncounterObjective[1] { new QuestLogData.EncounterObjective(EDl, titleStringID, 0) });
            can.SetValue(EDD, !mission.CannotCancel);

            return EDD;
        }

        internal static EncounterToSpawn GetEncounterSpawnDisplayInfo(SubMission mission, Encounter encounter = null)
        {

            EncounterIdentifier EI = new EncounterIdentifier(SubMissionTree.GetTreeCorp(mission.Faction), mission.GradeRequired, mission.Name, mission.Name);
            Dictionary<EncounterIdentifier, EncounterData> knab = (Dictionary<EncounterIdentifier, EncounterData>)manMod.GetValue(ManEncounter.inst);
            if (!knab.TryGetValue(EI, out EncounterData ED))
            {
                if (encounter == null)
                    encounter = GetFakeEncounter(mission, out _);

                ED = GetEncounterData(mission, encounter);

                knab.Add(EI, ED);
            }
            EncounterToSpawn ETS = new EncounterToSpawn(ED, EI);
            ETS.m_EncounterStringBankIdx = -100001;
            ETS.m_UsePosForPlacement = false;
            ETS.m_Rotation = Quaternion.identity;
            return ETS;
        }

        internal static Dictionary<string, EncounterToSpawn> cache = new Dictionary<string, EncounterToSpawn>();
        internal static EncounterToSpawn GetEncounterSpawnDisplayInfo(SubMissionStandby mission, bool New = false, bool ForceTop = false)
        {
            if (New)
                cache.Remove(mission.Name);

            if (!cache.TryGetValue(mission.Name, out EncounterToSpawn ETS))
            {
                EncounterIdentifier EI = new EncounterIdentifier(SubMissionTree.GetTreeCorp(mission.Faction), mission.GradeRequired, ForceTop ? "Core" : "Side", mission.Name);
                Dictionary<EncounterIdentifier, EncounterData> knab = (Dictionary<EncounterIdentifier, EncounterData>)manMod.GetValue(ManEncounter.inst);
                if (New)
                    knab.Remove(EI);
                if (!knab.TryGetValue(EI, out EncounterData ED))
                {
                    Encounter encounter = GetFakeEncounter(mission, out _, New);

                    ED = GetEncounterData(mission, encounter);

                    knab.Add(EI, ED);
                }

                ETS = new EncounterToSpawn();
                ETS.m_EncounterData = ED;
                ETS.m_EncounterDef = EI;
                ETS.m_EncounterStringBankIdx = -100001;
                ETS.m_Position = new WorldPosition(mission.TilePosWorld, Vector3.zero);
                ETS.m_UsePosForPlacement = false;
                ETS.m_Rotation = Quaternion.identity;
                _ = ETS.m_EncounterData.m_EncounterPrefab.EncounterDetails;
                cache.Add(mission.Name, ETS);
            }
            return ETS;
        }

        private static readonly FieldInfo
            LiCorp = FIFetch(typeof(LicenseCondition), "m_Corp"),
            LimT = FIFetch(typeof(LicenseCondition), "m_MinTier"),
            LiMT = FIFetch(typeof(LicenseCondition), "m_MaxTier"),

            XpCorp = FIFetch(typeof(XPPercentageCondition), "m_Corp"),
            XpGde = FIFetch(typeof(XPPercentageCondition), "m_Grade"),
            XpPer = FIFetch(typeof(XPPercentageCondition), "m_PercentageInGrade"),

            lice = FIFetch(typeof(EncounterConditions), "m_LicenseConditions"),
            XpP = FIFetch(typeof(EncounterConditions), "m_XpPercentageCondition"),
            misC = FIFetch(typeof(EncounterConditions), "m_MissionCompletedCondition"),
            CECC = FIFetch(typeof(EncounterConditions), "m_CoreEncounterCompletedCondition"),
            FM = FIFetch(typeof(EncounterConditions), "m_UnfinishedMission");

        internal static EncounterData GetEncounterData(SubMission mission, Encounter enc)
        {
            LicenseCondition LC = new LicenseCondition();
            LiCorp.SetValue(LC, mission.FactionType);
            LimT.SetValue(LC, mission.GradeRequired);
            LiMT.SetValue(LC, -1);

            ExternalCondition ExC = new ExternalCondition
            {
                tree = mission.Tree,
                hashName = mission.Name.GetHashCode(),
            };
            /*
            XPPercentageCondition XpPC = new XPPercentageCondition();
            XpCorp.SetValue(LC, mission.FactionType);
            XpGde.SetValue(LC, mission.GradeRequired);
            XpPer.SetValue(LC, 0);*/

            EncounterConditions EC = new EncounterConditions();
            lice.SetValue(EC, new LicenseCondition[2] { LC, ExC });
            XpP.SetValue(EC, new XPPercentageCondition[0] { });
            misC.SetValue(EC, new MissionCompletedCondition[0] { });
            CECC.SetValue(EC, new CoreEncounterCompletedCondition[0] { });

            EncounterData ED = new EncounterData
            {
                m_EncounterPrefab = enc,
                m_AddLog = false,
                m_AllowsPointOfInterest = false,
                m_BaseTechIsRadarPosition = false,
                m_BlockFutureEncountersInThisRadius = !mission.ClearModularMonumentsOnClear,
                m_CameraSpawnCondition = ManSpawn.CameraSpawnConditions.OffCamera,
                m_CanAcceptFromQuestGiver = true,
                m_CanBeCancelled = !mission.CannotCancel,
                m_CanSpawnOffTile = false,
                m_ForceSpawnIfNew = true,
                m_HasNoPosition = mission.IgnorePlayerProximity,
                m_IgnoreSceneryWhenSpawning = true,
                m_Name = mission.Name,
                m_LocationConditions = new LocationConditions(),
                m_RecycleAllManagedObjectsOnCancel = true,
                m_SetActiveInLog = false,
                m_ShowAreaOnMiniMap = false,
                m_SkippedByTutorialSkip = false,
                m_SpawnConditions = new EncounterConditions(),
                m_SpawnWithoutUserAccept = false,
            };
            return ED;
        }

        internal static EncounterData GetEncounterData(SubMissionStandby mission, Encounter enc)
        {
            LicenseCondition LC = new LicenseCondition();
            LiCorp.SetValue(LC, SubMissionTree.GetTreeCorp(mission.Faction));
            LimT.SetValue(LC, mission.GradeRequired);
            LiMT.SetValue(LC, -1);

            ExternalCondition ExC = new ExternalCondition
            {
                tree = mission.Tree,
                hashName = mission.Name.GetHashCode(),
            };
            /*
            XPPercentageCondition XpPC = new XPPercentageCondition();
            XpCorp.SetValue(LC, mission.FactionType);
            XpGde.SetValue(LC, mission.GradeRequired);
            XpPer.SetValue(LC, 0);*/

            EncounterConditions EC = new EncounterConditions();
            lice.SetValue(EC, new LicenseCondition[2] { LC, ExC });
            XpP.SetValue(EC, new XPPercentageCondition[0] { });
            misC.SetValue(EC, new MissionCompletedCondition[0] { });
            CECC.SetValue(EC, new CoreEncounterCompletedCondition[0] { });

            EncounterData ED = new EncounterData
            {
                m_EncounterPrefab = enc,
                m_AddLog = false,
                m_AllowsPointOfInterest = false,
                m_BaseTechIsRadarPosition = false,
                m_BlockFutureEncountersInThisRadius = false,
                m_CameraSpawnCondition = ManSpawn.CameraSpawnConditions.OffCamera,
                m_CanAcceptFromQuestGiver = true,
                m_CanBeCancelled = !mission.CannotCancel,
                m_CanSpawnOffTile = false,
                m_ForceSpawnIfNew = true,
                m_HasNoPosition = true,
                m_IgnoreSceneryWhenSpawning = true,
                m_Name = mission.Name,
                m_LocationConditions = new LocationConditions(),
                m_RecycleAllManagedObjectsOnCancel = true,
                m_SetActiveInLog = false,
                m_ShowAreaOnMiniMap = false,
                m_SkippedByTutorialSkip = false,
                m_SpawnConditions = new EncounterConditions(),
                m_SpawnWithoutUserAccept = false,
            };
            return ED;
        }




        private static FieldInfo FIFetch(Type type, string name)
        {
            return type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private static FieldInfo ForceIntoLoc(string name)
        {
            return null;
        }
    }

    public class ExternalCondition : LicenseCondition
    {
        public SubMissionTree tree;
        public int hashName; 
        public override bool Passes()
        {
            return tree.GetReachableMissions().Exists(delegate (SubMissionStandby cand) { return cand.Name.GetHashCode() == hashName; });
        }
    }
}
