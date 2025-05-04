using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TerraTechETCUtil;
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
        public const int CustomDisplayID = -100000;
        private static readonly FieldInfo
            obsticles = FIFetch(typeof(EncounterDetails), "m_Objectives"),
            Title = FIFetch(typeof(EncounterDetails), "m_TitleStringID"),
            Desc = FIFetch(typeof(EncounterDetails), "m_FullDescriptionStringID"),
            Timed = FIFetch(typeof(EncounterDetails), "m_IsTimed"),

            XPTarg = FIFetch(typeof(EncounterDetails), "m_XPCorp"),

            AwardXP = FIFetch(typeof(EncounterDetails), "m_AwardXP"),
            XPAmou = FIFetch(typeof(EncounterDetails), "m_XPAmount"),

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
            saveD = FIFetch(typeof(Encounter), "m_SaveData"),
            RadU = FIFetch(typeof(Encounter), "m_EncounterRadius"),
            aEnc = FIFetch(typeof(ManEncounter), "m_ActiveEncounters");

        internal static Dictionary<int, GameObject> FakeEncounters = new Dictionary<int, GameObject>();
        
        /// <summary>
        /// Returns true if it made a new
        /// </summary>
        private static bool GetFakeEncounterInternal(string name, FactionSubTypes faction, int grade, bool important, out Encounter encounter, out EncounterDetails EDl, out EncounterIdentifier EI)
        {
            string searchTerm = "temp - " + name;
            int hash = searchTerm.GetHashCode();
            if (FakeEncounters.TryGetValue(hash, out GameObject val))
            {
                Encounter Enc = val.GetComponent<Encounter>();
                Debug_SMissions.Log(KickStart.ModID + ": GetFakeEncounterInternal - Loaded for " + name);
                EDl = val.GetComponent<EncounterDetails>();
                EI = ((Encounter.SaveData)saveD.GetValue(Enc)).m_EncounterDef;
                encounter = Enc;
                return false;
            }
            Debug_SMissions.Log(KickStart.ModID + ": GetFakeEncounterInternal - New for " + name);
            GameObject temp = new GameObject(searchTerm);
            temp.AddComponent<DenySave>();
            EDl = temp.AddComponent<EncounterDetails>();


            Encounter dummyEmpty = temp.AddComponent<Encounter>();
            Encounter.SaveData SD = (Encounter.SaveData)saveD.GetValue(dummyEmpty);
            EI = new EncounterIdentifier(faction, grade, important ? "Core" : "Side", name);
            SD.m_EncounterDef = EI;
            SD.m_EncounterStringBankIdx = CustomDisplayID;
            FakeEncounters.Add(hash, temp);
            encounter = dummyEmpty;
            return true;
        }

        public static bool OfficialCall = false;
        public static bool IsSetting = false;
        /// <summary>
        /// Sets the FakeEncounter in the target SubMission
        /// </summary>
        /// <param name="faker"></param>
        /// <param name="init"></param>
        /// <returns></returns>
        internal static void SetFakeEncounter(SubMission faker, bool init = true)
        {
            if (!OfficialCall)
            {
                Encounter Enc2 = GetFakeEncounter(faker, out _, out EncounterIdentifier EI);
                faker.FakeEncounter = Enc2;
                IsSetting = true;
                ManEncounter.inst.SpawnAndStartListedEncounter(GetEncounterSpawnDisplayInfo(faker, EI), null);
                IsSetting = false;
            }
        }

        private static List<Encounter> regiCounter
        {
            get
            {
                try
                {
                    return (List<Encounter>)aEnc.GetValue(ManEncounter.inst);
                }
                catch
                {
                    Debug_SMissions.Assert("Could not fetch ActiveEncounters!");
                }
                return _regiCounter;
            }
        }
        private static List<Encounter> _regiCounter = new List<Encounter>();
        internal static void SuspendAllFakeEncounters()
        {
            foreach (var item in FakeEncounters)
            {
                Encounter Enc = item.Value.GetComponent<Encounter>();
                if (regiCounter.Contains(Enc))
                {
                    regiCounter.Remove(Enc);
                }
                else
                    Debug_SMissions.Assert(KickStart.ModID + ": Encounter " + Enc.EncounterName + " was not within ManEncounter!");
            }
            FakeEncounters.Clear();
        }
        internal static void ResumeAllFakeEncounters()
        {
            foreach (var item in FakeEncounters)
            {
                Encounter Enc = item.Value.GetComponent<Encounter>();
                if (!regiCounter.Contains(Enc))
                {
                    regiCounter.Add(Enc);
                }
                else
                    Debug_SMissions.Assert(KickStart.ModID + ": Encounter " + Enc.EncounterName + " was already within ManEncounter!");
            }
            FakeEncounters.Clear();
        }

        /// <summary>
        /// Uses Hash Searching - if issues arise here than it's likely this!
        /// </summary>
        /// <param name="name"></param>
        /// <param name="Enc"></param>
        private static void RecycleFakeEncounter(string name, Encounter Enc)
        {
            int hash = ("temp - " + name).GetHashCode();
            if (FakeEncounters.Remove(hash))
            {
                int hash2 = name.GetHashCode();
                int index = regiCounter.FindIndex(delegate (Encounter cand) { return cand.EncounterName.GetHashCode() == hash2; });
                if (index != -1)
                {
                    regiCounter.RemoveAt(index);
                }
                else
                {
                    Debug_SMissions.Log(KickStart.ModID + ": Encounters within ManEncounter:");
                    foreach (var item in regiCounter)
                    {
                        if (item != null && !item.EncounterName.NullOrEmpty())
                            Debug_SMissions.Log("- " + item.EncounterName);
                    }
                    Debug_SMissions.Assert(KickStart.ModID + ": Encounter " + name + " was removed but was not within ManEncounter!");
                }
                if (!cache.Remove(name))
                    Debug_SMissions.Assert(KickStart.ModID + ": Encounter " + name + " was removed but cache did not have the entry!");
                Enc.Recycle();
                Debug_SMissions.Log(KickStart.ModID + ": Encounter Destroyed.");
            }
        }
        internal static void RecycleAllFakeEncounters()
        {
            foreach (var item in FakeEncounters)
            {
                Encounter Enc = item.Value.GetComponent<Encounter>();
                if (regiCounter.Contains(Enc))
                {
                    regiCounter.Remove(Enc);
                }
                else
                    Debug_SMissions.Assert(KickStart.ModID + ": Encounter " + Enc.EncounterName + " was removed but was not within ManEncounter!");
                Enc.Recycle();
            }
            FakeEncounters.Clear();
        }


        private static Type objective = typeof(EncounterDetails).GetNestedType("Objective", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo objvDesc = objective.GetField("m_DescriptionStringID", BindingFlags.Public | BindingFlags.Instance);
        private static FieldInfo objvShow = objective.GetField("m_ShowByDefault", BindingFlags.Public | BindingFlags.Instance);
        private static FieldInfo objvCount = objective.GetField("m_TargetCount", BindingFlags.Public | BindingFlags.Instance);
        internal static Encounter GetFakeEncounter(SubMission mission, out EncounterDetails EDl, out EncounterIdentifier EI)
        {
            if (!GetFakeEncounterInternal(mission.Name, mission.FactionType, mission.GradeRequired, 
                mission.Type == SubMissionType.Critical, out Encounter dummyEmpty, out EDl, out EI))
                return dummyEmpty;

            int errorl = 0;
            try
            {
                errorl = 1;
                Array array = Array.CreateInstance(objective, 0);

                errorl = 2;
                obsticles.SetValue(EDl, array);

                errorl = 3;
                Title.SetValue(EDl, mission.Name);
                Desc.SetValue(EDl, mission.Description);
                Timed.SetValue(EDl, false);
                errorl = 4;

                FactionSubTypes FST = SubMissionTree.GetTreeCorp(mission.Rewards.CorpToGiveEXP);
                errorl = 5;
                if (mission.Rewards.EXPGain > 0)
                {
                    if (Singleton.Manager<ManLicenses>.inst.IsLicenseDiscovered(FST))
                    {
                        AwardXP.SetValue(EDl, true);
                        AwardLice.SetValue(EDl, false);
                        XPTarg.SetValue(EDl, FST);
                        XPAmou.SetValue(EDl, mission.Rewards.EXPGain);
                    }
                    else
                    {
                        AwardXP.SetValue(EDl, false);
                        AwardLice.SetValue(EDl, true);
                        XPTarg.SetValue(EDl, FST);
                        liceType.SetValue(EDl, FST);
                    }
                }
                else
                {
                    AwardXP.SetValue(EDl, false);
                    AwardLice.SetValue(EDl, false);
                    XPTarg.SetValue(EDl, FST);
                }
                errorl = 6;

                if (mission.Rewards.MoneyGain > 0)
                {
                    AwardBB.SetValue(EDl, true);
                    BBAmou.SetValue(EDl, mission.Rewards.MoneyGain);
                }
                else
                    AwardBB.SetValue(EDl, false);
                errorl = 7;

                bool randBlocks = mission.Rewards.RandomBlocksToSpawn > 0;
                if (randBlocks || mission.Rewards.BlocksToSpawn.Count > 0)
                {
                    AwardBloc.SetValue(EDl, true);
                    BlockTypes[] blocks = new BlockTypes[mission.Rewards.BlocksToSpawn.Count];
                    for (int i = 0; blocks.Length > i; i++)
                    {
                        blocks[i] = BlockIndexer.GetBlockIDLogFree(mission.Rewards.BlocksToSpawn[i]);
                    }
                    BlocSet.SetValue(EDl, blocks);
                    BlocPool.SetValue(EDl, randBlocks);
                    BlocPoolAmou.SetValue(EDl, mission.Rewards.RandomBlocksToSpawn);
                    BlocPoolLice.SetValue(EDl, FST);
                }
                else
                    AwardBloc.SetValue(EDl, false);
                errorl = 8;

                track.SetValue(EDl, EncounterDetails.AnalyticsMissionType.DoNotTrack);
                popDis.SetValue(dummyEmpty, false);
                RadU.SetValue(dummyEmpty, mission.GetMinimumLoadRange());
                errorl = 9;

                // Setup the UI stuff
                int checkLength = mission.CheckList.Count;
                array = Array.CreateInstance(objective, checkLength);
                errorl = 10;

                for (int step = 0; step < checkLength; step++)
                {
                    try
                    {
                        MissionChecklist ele = mission.CheckList[step];
                        object objectiveCase = Activator.CreateInstance(objective);
                        objvDesc.SetValue(objectiveCase, ele.ListArticle);
                        objvShow.SetValue(objectiveCase, (ele.BoolToEnable != 0));
                        if (ele.ValueType == VarType.IntOverInt)
                            objvCount.SetValue(objectiveCase, ele.GlobalIndex);
                        else
                            objvCount.SetValue(objectiveCase, 0);
                        array.SetValue(objectiveCase, step);
                    }
                    catch (Exception e)
                    {
                        Debug_SMissions.Assert(true, "FAILED TO REBUILD " + mission.Name + " MISSION CHECKLIST ON STEP " + step + " | " + e);
                    }
                }
                errorl = 11;
                obsticles.SetValue(EDl, array);
                questLog.SetValue(dummyEmpty, new QuestLogData(EI, EDl, CustomDisplayID));
            }
            catch (Exception e)
            {
                Debug_SMissions.LogError("Error on " + errorl + "\n" + e);
                throw new MandatoryException("EncounterShoehorn.GetFakeEncounter Error on " + errorl, e);
            }

            return dummyEmpty;
        }
        internal static Encounter GetFakeEncounter(SubMissionStandby mission, out EncounterDetails EDl, out EncounterIdentifier EI)
        {
            if (!GetFakeEncounterInternal(mission.Name, SubMissionTree.GetTreeCorp(mission.Faction), mission.GradeRequired, 
                mission.Type == SubMissionType.Critical, out Encounter dummyEmpty, out EDl, out EI))
                return dummyEmpty;

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
                            XPTarg.SetValue(EDl, FST);
                            XPAmou.SetValue(EDl, mission.Rewards.EXPGain);
                        }
                        else
                        {
                            AwardXP.SetValue(EDl, false);
                            AwardLice.SetValue(EDl, true);
                            XPTarg.SetValue(EDl, FST);
                            liceType.SetValue(EDl, FST);
                        }
                    }
                    else
                    {
                        AwardXP.SetValue(EDl, false);
                        AwardLice.SetValue(EDl, false);
                        XPTarg.SetValue(EDl, FST);
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
                // Setup the UI stuff
                QuestLogData QLD = new QuestLogData(dummyEmpty);
                QuestLogData.EncounterObjective[] objectives = QLD.InternalObjectives;
                int checkCount = mission.Checklist.Count;
                Array.Resize(ref objectives, checkCount);

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
                        objectives[step] = new QuestLogData.EncounterObjective(EDl, CustomDisplayID, step);
                    }
                    catch
                    {
                        Debug_SMissions.Assert(true, "FAILED TO REBUILD " + mission.Name + " MISSION CHECKLIST ON STEP " + step);
                    }
                }
                error++;
                obsticles.SetValue(EDl, array);
                questLog.SetValue(dummyEmpty, QLD);

                track.SetValue(EDl, EncounterDetails.AnalyticsMissionType.DoNotTrack);
                popDis.SetValue(dummyEmpty, false);
                RadU.SetValue(dummyEmpty, mission.LoadRadius);
            }
            catch {
                Debug_SMissions.LogError("EROOR IN ACTIONS - level " + error);
            }
            return dummyEmpty;
        }

        internal static void OnFinishSubMission(SubMission mission, ManEncounter.FinishState finish)
        {
            Encounter dummyEmpty = ManEncounter.inst.GetActiveEncounter(mission.FakeEncounter.EncounterDef);
            if (dummyEmpty)
            {
                Debug_SMissions.Log(KickStart.ModID + ": EncounterShoehorn - Finished SubMission " + mission.Name);
                ForceFinish(dummyEmpty);
                if (finish == ManEncounter.FinishState.Cancelled)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": FinishSubMission Cancelled " + mission.Name);
                    ManQuestLog.inst.RemoveLog(dummyEmpty, ManEncounter.FinishState.Cancelled, null);
                }
                else if (finish == ManEncounter.FinishState.Completed)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": FinishSubMission Completed " + mission.Name);
                    ManQuestLog.inst.RemoveLog(dummyEmpty, ManEncounter.FinishState.Completed, null);
                }
                else if (finish == ManEncounter.FinishState.Failed)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": FinishSubMission Failed " + mission.Name);
                    ManQuestLog.inst.RemoveLog(dummyEmpty, ManEncounter.FinishState.Failed, null);
                }
                else
                {
                    Debug_SMissions.Assert(KickStart.ModID + ": FinishSubMission recieved invalid request! " + finish);
                }
                //ManEncounter.inst.FinishEncounter(dummyEmpty, finish);
                RecycleFakeEncounter(mission.Name, dummyEmpty);
            }
            else
                Debug_SMissions.Log(KickStart.ModID + ": OnFinishSubMission " + mission.Name + " HAS NO ASSIGNED FAKE ENCOUNTER");
        }
        internal static void ForceFinish(Encounter mission)
        {
            Type loggerr = typeof(ManQuestLog).GetNestedType("EncounterLogEntry", BindingFlags.NonPublic | BindingFlags.Instance);
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
            Encounter encounter = GetFakeEncounter(mission, out EncounterDetails EDl, out EncounterIdentifier EI);

            EncounterDisplayData EDD = new EncounterDisplayData();
            EncounterData ED = GetEncounterData(mission, encounter);

            live.SetValue(EDD, false);
            enc.SetValue(EDD, encounter);
            ID.SetValue(EDD, EI);
            det.SetValue(EDD, ED);
            nam.SetValue(EDD, CustomDisplayID);
            log.SetValue(EDD, encounter.QuestLog);
            spw.SetValue(EDD, new EncounterToSpawn(ED, EI));
            posB.SetValue(EDD, !mission.IgnorePlayerProximity);
            pos.SetValue(EDD, mission.WorldPos);
            obj.SetValue(EDD, encounter.QuestLog.InternalObjectives);
            can.SetValue(EDD, !mission.CannotCancel);

            return EDD;
        }

        internal static Dictionary<string, EncounterToSpawn> cache = new Dictionary<string, EncounterToSpawn>();
        internal static EncounterToSpawn GetEncounterSpawnDisplayInfo(SubMission mission, EncounterIdentifier EI)
        {
            Dictionary<EncounterIdentifier, EncounterData> knab = (Dictionary<EncounterIdentifier, EncounterData>)manMod.GetValue(ManEncounter.inst);
            if (!knab.TryGetValue(EI, out EncounterData ED))
            {
                Encounter encounter = GetFakeEncounter(mission, out _, out _);

                ED = GetEncounterData(mission, encounter);

                knab.Add(EI, ED);
            }
            EncounterToSpawn ETS = new EncounterToSpawn(ED, EI);
            ETS.m_EncounterStringBankIdx = CustomDisplayID;
            ETS.m_Position = mission.WorldPos;
            ETS.m_Rotation = Quaternion.identity;
            ETS.m_UsePosForPlacement = false;
            _ = ETS.m_EncounterData.m_EncounterPrefab.EncounterDetails;
            Debug_SMissions.Log(KickStart.ModID + ": GetEncounterSpawnDisplayInfo(SubMission) - New EncounterToSpawn for " + mission.Name + ".");
            return ETS;
        }

        internal static EncounterToSpawn GetEncounterSpawnDisplayInfo(SubMissionStandby mission)
        {
            if (!cache.TryGetValue(mission.Name, out EncounterToSpawn ETS))
            {
                Encounter encounter = GetFakeEncounter(mission, out _, out EncounterIdentifier EI);
                Dictionary<EncounterIdentifier, EncounterData> knab = (Dictionary<EncounterIdentifier, EncounterData>)manMod.GetValue(ManEncounter.inst);
                if (!knab.TryGetValue(EI, out _))
                {
                    EncounterData ED = GetEncounterData(mission, encounter);

                    knab.Add(EI, ED);
                }

                ETS = new EncounterToSpawn(EI);
                ETS.m_EncounterStringBankIdx = CustomDisplayID;
                ETS.m_Position = new WorldPosition(mission.TilePosWorld, Vector3.zero);
                ETS.m_Rotation = Quaternion.identity;
                ETS.m_UsePosForPlacement = false;
                _ = ETS.m_EncounterData.m_EncounterPrefab.EncounterDetails;
                cache.Add(mission.Name, ETS);
                Debug_SMissions.Log(KickStart.ModID + ": GetEncounterSpawnDisplayInfo(SubMissionStandby) - New EncounterToSpawn for " + mission.Name + ".");
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
            XpPer.SetValue(LC, 0);
            */

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
                m_SpawnConditions = EC,
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
                m_SpawnConditions = EC,
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
