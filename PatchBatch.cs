using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using TMPro;
using System.Reflection.Emit;
#if !STEAM
using Nuterra.BlockInjector;
#endif


namespace Sub_Missions
{
    class PatchBatch { }

    internal static class Patches
    {
        [HarmonyPatch(typeof(ModeAttract))]
        [HarmonyPatch("SetupTechs")]// Setup main menu techs
        internal static class Subscribe
        {
            private static void Postfix()
            {
                KickStart.DelayedInit();
            }
        }


        /*
        [HarmonyPatch(typeof(ManEncounter))]
        [HarmonyPatch("SetupTechs")]// Get Mission info
        internal static class Subscribe
        {
            private static void Postfix()
            {
                ManSubMissions.Subscribe();
            }
        }*/

        [HarmonyPatch(typeof(Encounter))]
        [HarmonyPatch("Save")]// Get Mission info
        internal static class BlockSavingForDummies
        {
            private static bool Prefix(Encounter __instance)
            {
                if (__instance.GetComponent<DenySave>())
                {
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Encounter))]
        [HarmonyPatch("Load")]// Get Mission info
        internal static class BlockLoadingForDummies
        {
            private static bool Prefix(Encounter __instance)
            {
                if (__instance.GetComponent<DenySave>())
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(EncounterDetails))]
        [HarmonyPatch("GetString")]// Get Mission info
        internal static class EnableCustomText
        {
            private static bool Prefix(EncounterDetails __instance, ref int stringBankIdx, ref string stringID, ref string __result)
            {
                if (stringBankIdx == EncounterShoehorn.CustomDisplayID)
                {
                    __result = stringID;
                    return false;
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(ManProgression))]
        [HarmonyPatch("PopulateListOfRandomEncountersAvailable")]// Get Mission info
        internal static class ShoehornInExternalMissions
        {
            private static void Postfix(ManProgression __instance, ref Dictionary<FactionSubTypes, List<EncounterToSpawn>> randomEncounters)
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


        [HarmonyPatch(typeof(ManEncounter))]
        [HarmonyPatch("StartEncounter")]// Get Mission info
        internal static class StartMissionVanillaInterface
        {
            private static void Prefix(ManEncounter __instance, ref EncounterToSpawn spawnParams)
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
        }

        [HarmonyPatch(typeof(ManEncounter))]
        [HarmonyPatch("CancelEncounter")]// Get Mission info
        internal static class StopMissionVanillaInterface
        {
            private static bool Prefix(ManEncounter __instance, ref Encounter encounter, ref NetPlayer fromPlayer)
            {
                if (encounter != null)
                {
                    int hashName = encounter.EncounterName.GetHashCode();
                    SubMission SM = ManSubMissions.activeSubMissions.Find(delegate (SubMission cand) { return cand.Name.GetHashCode() == hashName; });
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
        }

        [HarmonyPatch(typeof(ManProgression))]
        [HarmonyPatch("EncounterCancelled")]// Get Mission info
        internal static class DoNotRecycleModdedEncounter
        {
            private static bool Prefix(ref Encounter encounter)
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


        [HarmonyPatch(typeof(ManEncounter))]
        [HarmonyPatch("Save")]
        internal static class StopSavingOfInvalidMissionShoehorns
        {
            private static void Prefix(ManEncounter __instance, ref ManSaveGame.State saveState)
            {
                EncounterShoehorn.SuspendAllFakeEncounters();
            }
            private static void Postfix(ManEncounter __instance, ref ManSaveGame.State saveState)
            {
                EncounterShoehorn.ResumeAllFakeEncounters();
            }
        }


        internal static Dictionary<FactionSubTypes, List<SMCCorpBlockRange>> OfficialBlocksPool = new Dictionary<FactionSubTypes, List<SMCCorpBlockRange>>();

        [HarmonyPatch(typeof(JSONBlockLoader))]
        [HarmonyPatch("Inject")]// Get Block Import info
        internal static class TrackImportedBlocks
        {
            private static void Prefix(ref int blockID, ref ModdedBlockDefinition def)
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
                            for (; RANGE.Count < grade; )
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

        [HarmonyPatch(typeof(ManMods))]
        [HarmonyPatch("PurgeModdedContentFromGame")]// Get Block Import info
        internal static class PurgeImportedBlocks
        {
            private static void Prefix()
            {
                OfficialBlocksPool.Clear();
            }
        }

        [HarmonyPatch(typeof(TileManager))]
        [HarmonyPatch("CreateTile")]// Setup main menu techs
        internal static class ExpandWorld
        {
            private static void Postfix(TileManager __instance, ref WorldTile tile)
            {
                TerrainOperations.AmplifyTerrain(tile.Terrain);
            }
        }
        /*
        [HarmonyPatch(typeof(MapGenerator))]
        [HarmonyPatch("GeneratePoint")]// Setup main menu techs
        internal static class FixWorldHeightExtending
        {
            private static void Postfix(MapGenerator __instance, ref float __result)
            {
                __result /= TerrainOperations.RescaleFactor;
            }
        }*/
        [HarmonyPatch(typeof(ManSaveGame))]
        [HarmonyPatch("Save")]// SAAAAAVVE
        private static class SaveTheMissions
        {
            private static void Prefix(ref ManGameMode.GameType gameType, ref string saveName)
            {
                Debug_SMissions.Log("SubMissions: ManSubMissions Saving!");
                SaveManSubMissions.SaveData(saveName);
            }
        }

#if !STEAM
        [HarmonyPatch(typeof(TechAudio))]
        [HarmonyPatch("GetCorpParams")]//
        private static class RevRight
        {
            private static bool Prefix(TechAudio __instance, ref TechAudio.UpdateAudioCache cache)
            {
                FactionSubTypes FST = __instance.Tech.GetMainCorp();
                int corpIndex = (int)FST;
                if (ManSMCCorps.IsOfficialSMCCorpLicense(corpIndex))
                {
                    if (ManSMCCorps.TryGetSMCCorpLicense(corpIndex, out SMCCorpLicense CL))
                    {
                        cache.corpMain = CL.EngineReferenceFaction;
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(ManMusic))]
        [HarmonyPatch("SetDanger", new Type[] { typeof(ManMusic.DangerContext.Circumstance), typeof(Tank), typeof(Tank),})]//
        private static class VeryyScary
        {
            private static readonly FieldInfo
                dangerFactor = typeof(ManMusic).GetField("m_DangerHistory", BindingFlags.NonPublic | BindingFlags.Instance);
            private static bool Prefix(ManMusic __instance, ref ManMusic.DangerContext.Circumstance circumstance, ref Tank enemyTech, ref Tank friendlyTech)
            {
                int corpIndex = (int)enemyTech.GetMainCorp();
                if (ManSMCCorps.IsSMCCorpLicense(corpIndex))
                {
                    if (ManSMCCorps.TryGetSMCCorpLicense(corpIndex, out SMCCorpLicense CL) && !CL.OfficialCorp)
                    {
                        if (CL.combatMusicLoaded.Count > 0)
                        {
                            ManSMCCorps.SetDangerContext(CL, enemyTech.blockman.blockCount, enemyTech.visible.ID);
                        }
                        else
                        {
                            //ManSMCCorps.HaltDanger();
                        }
                        ManMusic.DangerContext context;
                        if ((int)CL.CombatMusicFaction == -1)
                        {
                            ManMusic.inst.SetDangerMusicOverride(ManMusic.MiscDangerMusicType.Halloween);
                            context = new ManMusic.DangerContext
                            {
                                m_Circumstance = ManMusic.DangerContext.Circumstance.Generic,
                                m_Timeout = Time.time + 2f,// this does not come with very early loop end prevention!!!
                                m_Corporation = FactionSubTypes.NULL,
                                m_BlockCount = enemyTech.blockman.blockCount,
                                m_VisibleID = enemyTech.visible.ID,
                            };
                        }
                        else
                        {
                            ManMusic.inst.SetDangerMusicOverride(ManMusic.MiscDangerMusicType.None);
                            context = new ManMusic.DangerContext
                            {
                                m_Circumstance = circumstance,
                                m_Timeout = Time.time + 2f,
                                m_Corporation = CL.CombatMusicFaction,
                                m_BlockCount = enemyTech.blockman.blockCount,
                                m_VisibleID = enemyTech.visible.ID,
                            };
                        }
                        ManMusic.DangerContextHistory DCH = (ManMusic.DangerContextHistory)dangerFactor.GetValue(__instance);
                        DCH.Record(context);
                        return false;
                    }
                }
                //ManSMCCorps.HaltDanger();
                return true;
            }
        }
       
        
        [HarmonyPatch(typeof(ManMusic))]
        [HarmonyPatch("IsDangerous")]//
        private static class VeryyScary2
        {
            private static void Postfix(ManMusic __instance, ref bool __result)
            {
                /*
                if (!__result)
                {
                    //Debug_SMissions.Log("SubMissions: ManSMCCorps not dangerous");
                    ManSMCCorps.HaltDanger();
                }*/
                ManSMCCorps.musicOpening = !__result;
            }
        }

        [HarmonyPriority(-998)]
        [HarmonyPatch(typeof(BlockLoader))]
        [HarmonyPatch("FixBlockUnlockTable")]// SAAAAAVVE
        private static class PatchCCModdingAfter
        {
            private static bool Prefix(ref CustomBlock block)
            {
                //Debug_SMissions.Log("SubMissions: PatchCCModdingAfter - CALLED FOR " + block.Name);
                int error = 0;
                try
                {
                    error++;
                    foreach (SMCCorpLicense CL in ManSMCCorps.GetAllSMCCorps())
                    {
                        if (block.Name.StartsWith(CL.GetCorpNameForBlocks()))
                        {
                            return false;
                        }
                    }
                    error++;
                    return true;
                }
                catch (Exception e)
                {
                    Debug_SMissions.Log("SubMissions: PatchCCModding - Error on block " + block.Name + " " + e);
                }
                return true;
            }
        }
        [HarmonyPriority(-999)]
        [HarmonyPatch(typeof(BlockLoader))]
        [HarmonyPatch("Register", new Type[] { typeof(CustomBlock) })]// SAAAAAVVE
        private static class PatchCCModding
        {
            const int skipUntil = 102;
            const int skipBit = 6;

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                //Debug_SMissions.Log("SubMissions: PatchCCModding - TRANSPILING");
                var codes = new List<CodeInstruction>(instructions);
                var codesOut = new List<CodeInstruction>();
                /*
                int line = 0;
                foreach (CodeInstruction CI in codes)
                {
                    Debug_SMissions.Log("SubMissions: PatchCCModding - TRANSPILER: code " + CI.opcode + " | line - " + line);
                    line++;
                }*/
                codesOut.AddRange(codes.Take(skipUntil).ToList());
                List<CodeInstruction> spacer = new List<CodeInstruction>();
                for (int step = 0; step < skipBit; step++)
                    spacer.Add(new CodeInstruction(opcode: OpCodes.Nop));
                codesOut.AddRange(spacer);
                codesOut.AddRange(codes.Skip(skipUntil + skipBit).ToList());
                Debug_SMissions.Log("SubMissions: PatchCCModding - ------------ TRANSPILED ------------");
                //codesOut.AddRange(codes);
                /*
                Debug_SMissions.Log("\n");
                Debug_SMissions.Log("\n");
                Debug_SMissions.Log("SubMissions: PatchCCModding - Checked lines of code, " + codes.Count + " confirmed in, final is " + codesOut.Count);
                line = 0;
                foreach (CodeInstruction CI in codesOut)
                {
                    Debug_SMissions.Log("SubMissions: PatchCCModding - TRANSPILER: code " + CI.opcode + " | line - " + line);
                    if (line == skipUntil)
                        Debug_SMissions.Log("SubMissions: PatchCCModding - ------------ SNIPPED HERE!!!! ------------");
                    line++;
                }*/
                return codesOut;
            }
            
            private static void Prefix(ref CustomBlock block)
            {
                //Debug_SMissions.Log("SubMissions: PatchCCModding - CALLED FOR " + block.Name);
                int error = 0;
                try
                {
                    error++;
                    FactionSubTypes FST = block.Faction;
                    error++;
                    foreach (SMCCorpLicense CL in ManSMCCorps.GetAllSMCCorps())
                    {
                        if (block.Name.StartsWith(CL.GetCorpNameForBlocks()))
                        {
                            FST = (FactionSubTypes)CL.ID;
                            Debug_SMissions.Log("SubMissions: PatchCCModding - Reassigned Corp of " + block.Name + " to " + CL.ID);
                            break;
                        }
                    }
                    error++;
                    int hash = ItemTypeInfo.GetHashCode(ObjectTypes.Block, block.RuntimeID);
                    error++;
                    ManSpawn.inst.VisibleTypeInfo.SetDescriptor<FactionSubTypes>(hash, FST);
                }
                catch (Exception e)
                {
                    Debug_SMissions.Log("SubMissions: PatchCCModding - Error on block " + block.Name + " " + e);
                }
            }
            private static void Postfix(ref CustomBlock block)
            {
                //Debug_SMissions.Log("SubMissions: PatchCCModding - CALLED FOR " + block.Name);
                int error = 0;
                try
                {
                    if (block.Prefab)
                    {
                        //block.Prefab.GetComponent<MaterialSwapper>().SetupMaterial()
                    }
                }
                catch (Exception e)
                {
                    Debug_SMissions.Log("SubMissions: PatchCCModding(Postfix) - Error on block " + block.Name + " " + e);
                }
            }
        }
#endif




        // Custom Faction Support
        [HarmonyPatch(typeof(ManSpawn))]
        [HarmonyPatch("Start")]//
        private static class AddCrates
        {
            private static void Prefix(ManSpawn __instance)
            {
                Debug_SMissions.Log("SubMissions: AddCrates - INIT...");
                ManSMCCorps.BuildUnofficialCustomCorpCrates();
            }
        }


        //skins

        [HarmonyPatch(typeof(ManTechMaterialSwap))]
        [HarmonyPatch("GetMinEmissiveForCorporation")]// shoehorn in unofficial corps
        private static class AddSkinsCorrect
        {
            private static bool Prefix(ManTechMaterialSwap __instance, ref FactionSubTypes corp, ref float __result)
            {
                int corpIndex = (int)corp;
                if (ManSMCCorps.IsUnofficialSMCCorpLicense(corpIndex))
                {
                    if (ManSMCCorps.TryGetSMCCorpLicense(corpIndex, out SMCCorpLicense CL))
                    {
                        __result = CL.minEmissive;
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(UISkinsPaletteHUD))]
        [HarmonyPatch("SetSelectedSkin")]// shoehorn in unofficial corps
        private static class ProperlySelectSkins
        {
            private static readonly FieldInfo
                imagePre = typeof(UISkinsPaletteHUD).GetField("m_PreviewImage", BindingFlags.NonPublic | BindingFlags.Instance),
                imageIco = typeof(UISkinsPaletteHUD).GetField("m_PreviewImageCorpIcon", BindingFlags.NonPublic | BindingFlags.Instance),
                imageCoT = typeof(UISkinsPaletteHUD).GetField("m_PreviewCorpText", BindingFlags.NonPublic | BindingFlags.Instance),
                imageSkT = typeof(UISkinsPaletteHUD).GetField("m_PreviewSkinText", BindingFlags.NonPublic | BindingFlags.Instance),
                trans = typeof(UISkinsPaletteHUD).GetField("m_CurrentSkinButtons", BindingFlags.NonPublic | BindingFlags.Instance);
            private static bool Prefix(UISkinsPaletteHUD __instance, ref CorporationSkinUIInfo info, ref FactionSubTypes corp)
            {
                List<Transform> transs = (List<Transform>)trans.GetValue(__instance);
                if (ManSMCCorps.IsUnofficialSMCCorpLicense(corp))
                {
                    if (transs != null && transs.Count > 0)
                        transs.Last().gameObject.SetActive(false);

                    //there's no corp button, so we do everything BUT that
                    Image IG = (Image)imagePre.GetValue(__instance);
                    IG.sprite = info.m_PreviewImage;
                    Image IG2 = (Image)imageIco.GetValue(__instance);
                    IG2.sprite = Singleton.Manager<ManUI>.inst.GetSelectedCorpIcon(corp);
                    TMP_Text txt = (TMP_Text)imageCoT.GetValue(__instance);
                    txt.text = StringLookup.GetCorporationName(corp);
                    TMP_Text txt2 = (TMP_Text)imageSkT.GetValue(__instance);
                    txt2.text = info.m_FallbackString;
                    return false;
                }
                else
                {
                    if (transs != null && transs.Count > 0)
                        transs.Last().gameObject.SetActive(true);
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(UISkinsPaletteHUD))]
        [HarmonyPatch("OnSpawn")]// shoehorn in unofficial corps
        private static class AddSkinsMenu
        {
            private static void Prefix(UISkinsPaletteHUD __instance)
            {
                UICCorpLicenses.ForceAddModdedCorpsSection(__instance);
            }
            private static void Postfix(UISkinsPaletteHUD __instance)
            {
                UICCorpLicenses.ForceAddModdedCorpsSectionPost(__instance);
            }
        }
        [HarmonyPatch(typeof(BlockUnlockTable))]
        [HarmonyPatch("GetCorpBlockData")]// shoehorn in unofficial corps
        private static class ShoveCorpsIntoInventoryCorrectly
        {
            private static bool Prefix(BlockUnlockTable __instance, ref int corpIndex, ref BlockUnlockTable.CorpBlockData __result)
            {
                if (ManSMCCorps.IsUnofficialSMCCorpLicense(corpIndex))
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
                    Debug_SMissions.Log("SubMissions: CustomCorpsFix is present, holding off operations for patching.");
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
        }


        internal static FieldInfo licencesAll = typeof(ManLicenses).GetField("m_FactionLicenses", BindingFlags.NonPublic | BindingFlags.Instance),
            progg = typeof(FactionLicense).GetField("m_Progress", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPatch(typeof(ManLicenses))]
        [HarmonyPatch("SetupLicenses")]//
        private static class GetTheLayout
        {
            private static void Prefix(ManLicenses __instance)
            {
                Debug_SMissions.Log("SubMissions: SetupLicenses - Injecting Unofficial corps beforehand");
                ManSMCCorps.PushUnofficialCorpsToPool();
            }
        }
        [HarmonyPatch(typeof(ManLicenses))]
        [HarmonyPatch("Save")]//
        private static class DontSaveModdedLicenses
        {
            private static void Prefix(ManLicenses __instance, ref ManSaveGame.State saveState)
            {
                Debug_SMissions.Log("SubMissions: DontSaveModdedLicenses - Removing modded corp save states...");
                Dictionary<FactionSubTypes, FactionLicense> licences = (Dictionary<FactionSubTypes, FactionLicense>)licencesAll.GetValue(__instance);
                foreach (var item in ManSMCCorps.GetAllSMCCorpFactionTypes())
                {
                    licences.Remove(item);
                }
            }
        }

        [HarmonyPatch(typeof(ManMods))]
        [HarmonyPatch("InjectModdedCorps")]//
        private static class PoolForUnofficial
        {
            private static void Postfix(ManMods __instance)
            {
                Debug_SMissions.Log("SubMissions: SetupLicenses - Injecting Unofficial corps...");
                ManSMCCorps.ReloadAllUnofficialIfApplcable();
                ManTechMaterialSwap.inst.RebuildCorpArrayTextures();
                Debug_SMissions.Info("SubMissions: ManTechMaterialSwap - Fetching MainTex");
                foreach (KeyValuePair<int, Material> CBR in ManTechMaterialSwap.inst.m_FinalCorpMaterials)
                {
                    Debug_SMissions.Info("SubMissions: ManTechMaterialSwap - " + CBR.Key + " | " + CBR.Value);
                }
                ManSMCCorps.BuildUnofficialCustomCorpArrayTextures();
            }
        }

        [HarmonyPatch(typeof(UILicenses))]
        [HarmonyPatch("Init")]//
        private static class InitCorrect
        {
            private static void Prefix(UILicenses __instance, ref object context)
            {
                try
                {
                    Dictionary<FactionSubTypes, FactionLicense> dictionary = context as Dictionary<FactionSubTypes, FactionLicense>;

                    Dictionary<FactionSubTypes, FactionLicense> NewDictionary = new Dictionary<FactionSubTypes, FactionLicense>();
                    foreach (KeyValuePair<FactionSubTypes, FactionLicense> pair in dictionary)
                    {
                        if ((int)pair.Key < Enum.GetNames(typeof(FactionSubTypes)).Length)
                            NewDictionary.Add(pair.Key, pair.Value);

                    }
                    context = NewDictionary;
                }
                catch { }
            }
            private static void Postfix()
            {
                try
                {
                    UICCorpLicenses.InitALLFactionLicenseUnofficialUI();
                }
                catch { }
            }
        }
        [HarmonyPatch(typeof(UILicenses))]
        [HarmonyPatch("DeInit")]//
        private static class DeInitCorrect
        {
            private static void Postfix(UILicenses __instance)
            {
                try
                {
                    UICCorpLicenses.DeInitALLFactionLicenseUnofficialUI();
                }
                catch { }
            }
        }
        [HarmonyPatch(typeof(UILicenses))]
        [HarmonyPatch("ShowCorpLicense")]//
        private static class ToggleTheRightInstance
        {
            private static bool Prefix(UILicenses __instance, ref FactionSubTypes corp)
            {
                if (ManSMCCorps.IsUnofficialSMCCorpLicense(corp))
                {
                    UICCorpLicenses.ShowFactionLicenseUnofficialUI((int)corp);
                    return false;
                }
                else if (ManSMCCorps.IsOfficialSMCCorpLicense((int)corp))
                {
                    UICCorpLicenses.ShowFactionLicenseOfficialUI((int)corp);
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(StringLookup))]
        [HarmonyPatch("GetCorporationName")]//
        private static class GetCorrectName
        {
            private static bool Prefix(ref FactionSubTypes corporation, ref string __result)
            {
                if (ManSMCCorps.IsUnofficialSMCCorpLicense(corporation))
                {
                    if (ManSMCCorps.TryGetSMCCorpLicense((int)corporation, out SMCCorpLicense CL))
                    {
                        __result = CL.FullName;
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(ManLicenses))]
        [HarmonyPatch("AwardLevelUpBlocks")]//
        private static class AwardCorrect
        {
            private static bool Prefix(ref FactionSubTypes corporation, ref int grade)
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

        [HarmonyPatch(typeof(SpriteFetcher))]
        [HarmonyPatch("GetCorpIcon")]//
        private static class LoadTheRightStuff
        {
            private static bool Prefix(SpriteFetcher __instance, ref FactionSubTypes corp, ref Sprite __result)
            {
                if (ManSMCCorps.IsUnofficialSMCCorpLicense(corp))
                {
                    
                    if (ManSMCCorps.TryGetSMCCorpLicense((int)corp, out SMCCorpLicense CL))
                    {
                        if (CL.SmallCorpIcon)
                        {
                            __result = CL.SmallCorpIcon;
                            return false;
                        }
                        if (CL.SkinReferenceFaction != FactionSubTypes.NULL && (int)CL.SkinReferenceFaction < Enum.GetValues(typeof(FactionSubTypes)).Length)
                        {
                            corp = CL.SkinReferenceFaction;
                            return true;
                        }
                    }
                    
                    corp = FactionSubTypes.GSO;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(SpriteFetcher))]
        [HarmonyPatch("GetSelectedCorpIcon")]//
        private static class LoadTheRightStuff2
        {
            private static bool Prefix(SpriteFetcher __instance, ref FactionSubTypes corp, ref Sprite __result)
            {
                if (ManSMCCorps.IsUnofficialSMCCorpLicense(corp))
                {
                    if (ManSMCCorps.TryGetSMCCorpLicense((int)corp, out SMCCorpLicense CL))
                    {
                        if (CL.SmallSelectedCorpIcon)
                        {
                            __result = CL.SmallSelectedCorpIcon;
                            return false;
                        }
                        if (CL.SkinReferenceFaction != FactionSubTypes.NULL && (int)CL.SkinReferenceFaction < Enum.GetValues(typeof(FactionSubTypes)).Length)
                        {
                            corp = CL.SkinReferenceFaction;
                            return true;
                        }
                    }
                    corp = FactionSubTypes.GSO;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(SpriteFetcher))]
        [HarmonyPatch("GetModernCorpIcon")]//
        private static class LoadTheRightStuff3
        {
            private static bool Prefix(SpriteFetcher __instance, ref FactionSubTypes corp, ref Sprite __result)
            {
                if (ManSMCCorps.IsUnofficialSMCCorpLicense(corp))
                {
                    if (ManSMCCorps.TryGetSMCCorpLicense((int)corp, out SMCCorpLicense CL))
                    {
                        if (CL.HighResCorpIcon)
                        {
                            __result = CL.HighResCorpIcon;
                            return false;
                        }
                        if (CL.SkinReferenceFaction != FactionSubTypes.NULL && (int)CL.SkinReferenceFaction < Enum.GetValues(typeof(FactionSubTypes)).Length)
                        {
                            corp = CL.SkinReferenceFaction;
                            return true;
                        }
                    }
                    corp = FactionSubTypes.GSO;
                }
                return true;
            }
        }
    }
}
