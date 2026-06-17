using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HarmonyLib;

namespace Sub_Missions
{
    internal static class ProgressionPatches
    {
        // Custom Faction Support
        private static class ManSpawnPatches
        {
            internal static Type target = typeof(ManSpawn);
            //AddCrates
            private static void Start_Prefix(ManSpawn __instance)
            {
                Debug_SMissions.Log(KickStart.ModID + ": AddCrates - INIT...");
                ManSMCCorps.BuildUnofficialCustomCorpCrates();
            }
        }
        /*
        [HarmonyPatch(typeof(BlockUnlockTable))]
        [HarmonyPatch("GetCorpBlockData")]// shoehorn in unofficial corps
        private static class ShoveCorpsIntoInventoryCorrectly
        {
            private static bool Prefix(BlockUnlockTable __instance, ref int corpIndex, ref BlockUnlockTable.CorpBlockData __result)
            {
                if (KickStart.isCustomCorpsFixPresent)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": CustomCorpsFix is present, holding off GetCorpBlockData.");
                }
                else if (ManSMCCorps.IsUnofficialSMCCorpLicense(corpIndex))
                {
                    if (ManSMCCorps.TryGetSMCCorpLicense(corpIndex, out SMCCorpLicense CL))
                    {
                        __result = CL.GetCorpBlockData(out int numEntries);
                        Debug_SMissions.Log("ShoveCorpsIntoInventoryCorrectly - Called with " + numEntries + " blocks assigned");

                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(BlockUnlockTable))]
        [HarmonyPatch("GetBlockTier")]// shoehorn in unofficial corps
        private static class MakeSureReturnRightGrade
        {
            private static bool Prefix(BlockUnlockTable __instance, ref BlockTypes blockType, ref int __result)
            {
                if (KickStart.isCustomCorpsFixPresent)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": CustomCorpsFix is present, holding offGetBlockTier.");
                }
                else
                {
                    int corpIndex = (int)Singleton.Manager<ManSpawn>.inst.GetCorporation(blockType);
                    if (ManSMCCorps.TryGetSMCCorpLicense(corpIndex, out SMCCorpLicense CL))
                    {
                        foreach (SMCCorpBlockRange CBR in CL.GradeUnlockBlocks)
                        {
                            if (CBR.BlocksAvail.Contains(blockType))
                            {
                                __result = CL.GradeUnlockBlocks.IndexOf(CBR);
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(BlockUnlockTable))]
        [HarmonyPatch("AddModdedBlocks")]// shoehorn in unofficial corps
        private static class MakeSureWeRegisterGradeCorrectly
        {   // Default just stacks them all in Grade 1
            private const int MaxGrade = 4;
            private static bool Prefix(BlockUnlockTable __instance, ref int corpIndex, ref int gradeIndex, ref Dictionary<BlockTypes, ModdedBlockDefinition> blocks)
            {
                if (KickStart.isCustomCorpsFixPresent)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": CustomCorpsFix is present, holding off operations for patching.");
                }
                else if (blocks.Count > 0)
                {
                    BlockUnlockTable.CorpBlockData CBD = __instance.GetCorpBlockData(corpIndex);
                    if (CBD != null)
                    {
                        int gradesAvail = CBD.m_GradeList.Length;
                        gradeIndex = Mathf.Clamp(gradeIndex, 0, MaxGrade);
                        Debug_SMissions.Log("Current supported Grades " + gradesAvail + " Suggested grade count " + (gradeIndex + 1));
                        int futureSize = gradeIndex + 1;
                        if (gradesAvail < futureSize)
                        {
                            Debug_SMissions.Log("There are now " + futureSize + " grade(s) in corp " + corpIndex);
                            Array.Resize(ref CBD.m_GradeList, futureSize);
                            for (int step = 0; step < futureSize; step++)
                            {
                                if (CBD.m_GradeList[step] == null)
                                    CBD.m_GradeList[step] = new BlockUnlockTable.GradeData();
                            }
                        }
                        int prevBlockCount = CBD.m_GradeList[gradeIndex].m_BlockList.Count();
                        int combinedBlockCount = prevBlockCount + blocks.Count;
                        Debug_SMissions.Log("Prev size of blocks in array " + prevBlockCount + " now " + combinedBlockCount);
                        Array.Resize(ref CBD.m_GradeList[gradeIndex].m_BlockList, combinedBlockCount);
                        int position = 0;
                        int displayedCount = 0;
                        foreach (KeyValuePair<BlockTypes, ModdedBlockDefinition> pair in blocks)
                        {
                            BlockCategories BC = pair.Value.m_Category;
                            bool hide = BC == BlockCategories.Standard || BC == BlockCategories.Null;
                            if (!hide)
                            {
                                if (displayedCount == 12)
                                {
                                    hide = true;
                                }
                                else
                                    displayedCount++;
                            }
                            CBD.m_GradeList[gradeIndex].m_BlockList[prevBlockCount + position] = new BlockUnlockTable.UnlockData
                            {
                                m_BlockType = pair.Key,
                                m_BasicBlock = true,
                                m_DontRewardOnLevelUp = !pair.Value.m_UnlockWithLicense,
                                m_HideOnLevelUpScreen = hide
                            };
                            position++;
                        }
                    }
                }
                return false;
            }
        }*/


        internal static FieldInfo m_FactionLicenses = typeof(ManLicenses).GetField("m_FactionLicenses", BindingFlags.NonPublic | BindingFlags.Instance),
            progg = typeof(FactionLicense).GetField("m_Progress", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static void LogLicenceReady(FactionSubTypes faction)
        {
            Dictionary<FactionSubTypes, FactionLicense> licences =
                (Dictionary<FactionSubTypes, FactionLicense>)m_FactionLicenses.GetValue(ManLicenses.inst);
            if (licences.ContainsKey(faction))
                Debug_SMissions.Log("LogLicenceReady - Corp " +
                    (int)faction + " IS registered!");
            else
                Debug_SMissions.Log("LogLicenceReady - Corp " +
                    (int)faction + " is NOT registered!");
        }

        internal static class ManLicensesPatches
        {
            internal static Type target = typeof(ManLicenses);
            //InsureShoehornedLicenceReady
            internal static void GetLicense_Prefix(ManLicenses __instance, FactionSubTypes faction)
            {
                //LogLicenceReady(faction);
            }

            //GetTheLayout
            internal static void SetupLicenses_Prefix(ManLicenses __instance)
            {
                Debug_SMissions.Log(KickStart.ModID + ": SetupLicenses - Injecting Unofficial corps beforehand");
                ManSMCCorps.PushUnofficialCorpsToPool();
            }
            //DontSaveModdedLicenses
            internal static void Save_Prefix(ManLicenses __instance, ref ManSaveGame.State saveState)
            {
                Debug_SMissions.Log(KickStart.ModID + ": DontSaveModdedLicenses - Preventing the base game from saving it (prevent crash on corp absence)...");
                Dictionary<FactionSubTypes, FactionLicense> licences = (Dictionary<FactionSubTypes, FactionLicense>)m_FactionLicenses.GetValue(__instance);
                foreach (var item in ManSMCCorps.GetAllSMCCorpFactionTypes())
                {
                    licences.Remove(item);
                }
            }
            //AwardCorrect
            internal static bool AwardLevelUpBlocks_Prefix(ref FactionSubTypes corporation, ref int grade)
            {
                if (ManSMCCorps.TryGetSMCCorpLicense((int)corporation, out SMCCorpLicense CL))
                {
                    if (CL.HasCratePrefab)
                        ManSMCCorps.crateThing.RewardBlocksByCrate(CL.GetGradeUnlockBlocks(grade), Singleton.playerPos, corporation);
                    else
                        ManSMCCorps.crateThing.RewardBlocksByCrate(CL.GetGradeUnlockBlocks(grade), Singleton.playerPos, CL.CrateReferenceFaction);
                    return false;
                }
                return true;
            }
        }

        internal static class ManModsPatches
        {
            internal static Type target = typeof(ManMods);
            //PoolForUnofficial
            private static void InjectModdedCorps_Postfix(ManMods __instance)
            {
                Debug_SMissions.Log(KickStart.ModID + ": SetupLicenses - Injecting Unofficial corps...");
                ManSMCCorps.ReloadAllUnofficialIfApplcable();
                ManTechMaterialSwap.inst.RebuildCorpArrayTextures();
                Debug_SMissions.Info(KickStart.ModID + ": ManTechMaterialSwap - Fetching MainTex");
                foreach (KeyValuePair<int, Material> CBR in ManTechMaterialSwap.inst.m_FinalCorpMaterials)
                {
                    Debug_SMissions.Info(KickStart.ModID + ": ManTechMaterialSwap - " + CBR.Key + " | " + CBR.Value);
                }
                ManSMCCorps.BuildUnofficialCustomCorpArrayTextures();
            }
            //PurgeImportedBlocks
            private static void PurgeModdedContentFromGame_Prefix()
            {
                OfficialBlocksPool.Clear();
            }
        }

        internal static class EncounterPatches
        {
            internal static Type target = typeof(Encounter);
            //BlockSavingForDummies - Get Mission info
            private static bool Save_Prefix(Encounter __instance)
            {
                if (__instance.GetComponent<DenySave>())
                {
                    return false;
                }
                return true;
            }
            //BlockLoadingForDummies
            private static bool Load_Prefix(Encounter __instance)
            {
                if (__instance.GetComponent<DenySave>())
                {
                    return false;
                }
                return true;
            }
            
        }
        internal static class EncounterDetailsPatches
        {
            internal static Type target = typeof(EncounterDetails);
            //EnableCustomText
            private static bool GetString_Prefix(EncounterDetails __instance, ref int stringBankIdx, ref string stringID, ref string __result)
            {
                if (stringBankIdx == EncounterShoehorn.CustomDisplayID)
                {
                    __result = stringID;
                    return false;
                }
                return true;
            }
        }
        internal static class ManProgressionPatches
        {
            internal static Type target = typeof(ManProgression);
            //ShoehornInExternalMissions
            private static void PopulateListOfRandomEncountersAvailable_Postfix(ManProgression __instance, ref Dictionary<FactionSubTypes, List<EncounterToSpawn>> randomEncounters)
            {
                ManSubMissions.inst.GetAllPossibleMissions();
                Debug_SMissions.Log("MISSIONS FOR MISSION BOARD INLINING");
                foreach (var item in ManSubMissions.AnonSubMissions)
                {
                    FactionSubTypes FST = SubMissionTree.GetTreeCorp(item.Faction);
                    if (randomEncounters.TryGetValue(FST, out List<EncounterToSpawn> encounters))
                    {
                        if (!encounters.Exists(delegate (EncounterToSpawn cand) { return cand.m_EncounterDef.m_Name == item.Name; }))
                        {
                            EncounterShoehorn.GetFakeEncounter(item, out EncounterDetails EDl, out EncounterIdentifier EI);
                            encounters.Add(EncounterShoehorn.GetEncounterSpawnDisplayInfo(item));
                            Debug_SMissions.Log("MISSION BOARD - new mission " + item.Name);
                        }
                    }
                    else
                    {
                        EncounterShoehorn.GetFakeEncounter(item, out EncounterDetails EDl, out EncounterIdentifier EI);
                        randomEncounters.Add(FST, new List<EncounterToSpawn> { EncounterShoehorn.GetEncounterSpawnDisplayInfo(item) });
                        Debug_SMissions.Log("MISSION BOARD [New corp added - " + FST + "] - new mission " + item.Name);
                    }
                }
            }

            //DoNotRecycleModdedEncounter
            private static bool EncounterCancelled_Prefix(ref Encounter encounter)
            {
                try
                {
                    if (encounter?.EncounterDef == null || encounter.EncounterDef.m_Name == null)
                    {
                        Debug_SMissions.Log("ENCOUNTER CANCELLED - NULL!!!!");
                        return false;
                    }
                    else
                    {
                        Debug_SMissions.Log("ENCOUNTER CANCELLED " + encounter.EncounterDef.m_Name);
                        //return EncounterShoehorn.cache.TryGetValue(encounter.EncounterDef.m_Name, out _);
                        return true;
                    }
                }
                catch { }
                return false;
            }
        }
        /*
        [HarmonyPatch(typeof(ManQuestLog))]
        [HarmonyPatch("AddLog", new Type[] { typeof(Encounter), typeof(NetPlayer), typeof(bool), })]// Get Mission info
        internal static class ShoehornInChecker
        {
            private static void Prefix(ManQuestLog __instance, ref Encounter encounter)
            {
                Debug_SMissions.Log("CHECKING LOG CONSISTANCY");
                Debug_SMissions.Log("CANIDATE " + encounter.EncounterName);
                foreach (var item in ManSubMissions.GetAllFakeEncounters())
                {
                    Debug_SMissions.Log("ACTIVE " + item.EncounterName);
                }
                foreach (var item in ManSubMissions.GetAllFakeEncounters())
                {
                    if (item == encounter)
                    {
                        Debug_SMissions.Log("\nMATCH WITH " + item.EncounterName);
                        return;
                    }
                }
            }
        }*/

        internal static class ManEncounterPatches
        {
            internal static Type target = typeof(ManEncounter);
            //StartMissionVanillaInterface
            private static void StartEncounter_Prefix(ManEncounter __instance, ref EncounterToSpawn spawnParams)
            {
                if (!EncounterShoehorn.IsSetting && spawnParams?.m_EncounterData?.m_Name != null)
                {
                    EncounterShoehorn.OfficialCall = true;
                    int hashName = spawnParams.m_EncounterData.m_Name.GetHashCode();
                    SubMissionStandby SMS = ManSubMissions.AnonSubMissions.Find(delegate (SubMissionStandby cand) { return cand.Name.GetHashCode() == hashName; });
                    if (SMS != null)
                    {
                        ManSubMissions.SelectedAnon = SMS;
                        ManSubMissions.SelectedIsAnon = true;
                        ManSubMissions.inst.AcceptMission(spawnParams.m_EncounterData.m_EncounterPrefab);
                    }
                    EncounterShoehorn.OfficialCall = false;
                }
            }
            //StopMissionVanillaInterface
            private static bool CancelEncounter_Prefix(ManEncounter __instance, ref Encounter encounter, ref NetPlayer fromPlayer)
            {
                if (encounter != null)
                {
                    int hashName = encounter.EncounterName.GetHashCode();
                    SubMission SM = ManSubMissions.activeSubMissionsCached.Find(delegate (SubMission cand) { return cand.Name.GetHashCode() == hashName; });
                    if (SM != null)
                    {
                        ManSubMissions.Selected = SM;
                        ManSubMissions.SelectedIsAnon = false;
                        ManSubMissions.inst.CancelMission();
                        //ManEncounter.inst.FinishEncounter(encounter, ManEncounter.FinishState.Cancelled, fromPlayer);
                        return false;
                    }
                }
                return true;
            }
            //StopSavingOfInvalidMissionShoehorns
            private static void Save_Prefix(ManEncounter __instance, ref ManSaveGame.State saveState)
            {
                EncounterShoehorn.SuspendAllFakeEncounters();
            }
            private static void Save_Postfix(ManEncounter __instance, ref ManSaveGame.State saveState)
            {
                EncounterShoehorn.ResumeAllFakeEncounters();
            }
        }

        internal static Dictionary<FactionSubTypes, List<SMCCorpBlockRange>> OfficialBlocksPool = new Dictionary<FactionSubTypes, List<SMCCorpBlockRange>>();
        internal static class JSONBlockLoaderPatches
        {
            internal static Type target = typeof(JSONBlockLoader);
            //TrackImportedBlocks
            private static void Inject_Prefix(ref int blockID, ref ModdedBlockDefinition def)
            {
                if (def != null)
                {
                    int grade = Mathf.Clamp(def.m_Grade, 1, 5);
                    int gradeRaw = grade - 1;
                    FactionSubTypes FST = ManMods.inst.GetCorpIndex(def.m_Corporation);
                    if (OfficialBlocksPool.TryGetValue(FST, out List<SMCCorpBlockRange> RANGE))
                    {
                        if (grade > RANGE.Count)
                        {
                            for (; RANGE.Count < grade;)
                            {
                                RANGE.Add(new SMCCorpBlockRange());
                            }
                        }
                        SMCCorpBlockRange CBR = RANGE[gradeRaw];
                        CBR.BlocksOutOfRange.Add((BlockTypes)blockID);
                    }
                    else
                    {
                        RANGE = new List<SMCCorpBlockRange>();
                        OfficialBlocksPool.Add(FST, RANGE);
                        if (grade > RANGE.Count)
                        {
                            for (; RANGE.Count < grade;)
                            {
                                RANGE.Add(new SMCCorpBlockRange());
                            }
                        }
                        SMCCorpBlockRange CBR = RANGE[gradeRaw];
                        CBR.BlocksOutOfRange.Add((BlockTypes)blockID);
                    }
                }
            }
        }
        internal static class ManSaveGamePatches
        {
            internal static Type target = typeof(ManSaveGame);
            //SaveTheMissions - SAAAAAVVE
            private static void Save_Prefix(ref ManGameMode.GameType gameType, ref string saveName)
            {
                Debug_SMissions.Log(KickStart.ModID + ": ManSubMissions Saving!");
                SaveManSubMissions.SaveDataLegacy(saveName);
            }
        }
    }
}
