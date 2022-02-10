using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using TMPro;
using Nuterra.BlockInjector;
using System.Reflection.Emit;

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
                ManSubMissions.Subscribe();
                Debug.Log("SubMissions: Core module hooks launched");
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
                Debug.Log("SubMissions: ManSubMissions Saving!");
                SaveManSubMissions.SaveData(saveName);
            }
        }
        [HarmonyPatch(typeof(TechAudio))]
        [HarmonyPatch("GetCorpParams")]//
        private static class RevRight
        {
            private static bool Prefix(TechAudio __instance, ref TechAudio.UpdateAudioCache cache)
            {
                FactionSubTypes FST = __instance.Tech.GetMainCorp();
                int corpIndex = (int)FST;
                if (corpIndex > ManSMCCorps.UCorpRange)
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
                if (corpIndex > ManSMCCorps.UCorpRange)
                {
                    if (ManSMCCorps.TryGetSMCCorpLicense(corpIndex, out SMCCorpLicense CL))
                    {
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
                return true;
            }
        }

        [HarmonyPatch(typeof(BlockLoader))]
        [HarmonyPatch("FixBlockUnlockTable")]// SAAAAAVVE
        private static class PatchCCModdingAfter
        {
            private static bool Prefix(ref CustomBlock block)
            {
                Debug.Log("SubMissions: PatchCCModdingAfter - CALLED FOR " + block.Name);
                int error = 0;
                try
                {
                    error++;
                    foreach (SMCCorpLicense CL in ManSMCCorps.GetAllSMCCorps())
                    {
                        if (block.Name.StartsWith(CL.Faction))
                        {
                            return false;
                        }
                    }
                    error++;
                    return true;
                }
                catch (Exception e)
                {
                    Debug.Log("SubMissions: PatchCCModding - Error on block " + block.Name + " " + e);
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(BlockLoader))]
        [HarmonyPatch("Register", new Type[] { typeof(CustomBlock) })]// SAAAAAVVE
        private static class PatchCCModding
        {
            const int skipUntil = 102;
            const int skipBit = 6;

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                Debug.Log("SubMissions: PatchCCModding - TRANSPILING");
                var codes = new List<CodeInstruction>(instructions);
                var codesOut = new List<CodeInstruction>();
                /*
                int line = 0;
                foreach (CodeInstruction CI in codes)
                {
                    Debug.Log("SubMissions: PatchCCModding - TRANSPILER: code " + CI.opcode + " | line - " + line);
                    line++;
                }*/
                codesOut.AddRange(codes.Take(skipUntil).ToList());
                List<CodeInstruction> spacer = new List<CodeInstruction>();
                for (int step = 0; step < skipBit; step++)
                    spacer.Add(new CodeInstruction(opcode: OpCodes.Nop));
                codesOut.AddRange(spacer);
                codesOut.AddRange(codes.Skip(skipUntil + skipBit).ToList());
                //codesOut.AddRange(codes);
                /*
                Debug.Log("\n");
                Debug.Log("\n");
                Debug.Log("SubMissions: PatchCCModding - Checked lines of code, " + codes.Count + " confirmed in, final is " + codesOut.Count);
                line = 0;
                foreach (CodeInstruction CI in codesOut)
                {
                    Debug.Log("SubMissions: PatchCCModding - TRANSPILER: code " + CI.opcode + " | line - " + line);
                    if (line == skipUntil)
                        Debug.Log("SubMissions: PatchCCModding - ------------ SNIPPED HERE!!!! ------------");
                    line++;
                }*/
                return codesOut;
            }
            
            private static void Prefix(ref CustomBlock block)
            {
                Debug.Log("SubMissions: PatchCCModding - CALLED FOR " + block.Name);
                int error = 0;
                try
                {
                    error++;
                    FactionSubTypes FST = block.Faction;
                    bool neededReassign = false;
                    error++;
                    foreach (SMCCorpLicense CL in ManSMCCorps.GetAllSMCCorps())
                    {
                        if (block.Name.StartsWith(CL.Faction))
                        {
                            FST = (FactionSubTypes)CL.ID;
                            neededReassign = true;
                            break;
                        }
                    }
                    error++;
                    if (neededReassign)
                        Debug.Log("SubMissions: PatchCCModding - Reassigned Corp of " + block.Name);
                    int hash = ItemTypeInfo.GetHashCode(ObjectTypes.Block, block.RuntimeID);
                    error++;
                    ManSpawn.inst.VisibleTypeInfo.SetDescriptor<FactionSubTypes>(hash, FST);
                }
                catch (Exception e)
                {
                    Debug.Log("SubMissions: PatchCCModding - Error on block " + block.Name + " " + e);
                }
            }
        }




        // Custom Faction Support
        [HarmonyPatch(typeof(ManSpawn))]
        [HarmonyPatch("Start")]//
        private static class AddCrates
        {
            private static void Prefix(ManSpawn __instance)
            {
                Debug.Log("SubMissions: AddCrates - INIT...");
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
                if (corpIndex > ManSMCCorps.UCorpRange)
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
                if ((int)corp > ManSMCCorps.UCorpRange)
                {
                    if (transs != null)
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
                    if (transs != null)
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
        private static class AddUnOfficialCorpsToInventory
        {
            private static bool Prefix(BlockUnlockTable __instance, ref int corpIndex, ref BlockUnlockTable.CorpBlockData __result)
            {
                if (corpIndex > ManSMCCorps.UCorpRange)
                {
                    if (ManSMCCorps.TryGetSMCCorpLicense(corpIndex, out SMCCorpLicense CL))
                    {
                        __result = CL.UnofficialGetCorpBlockData();
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
                if (corpIndex > ManSMCCorps.UCorpRange)
                {
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
        [HarmonyPatch(typeof(ManLicenses))]
        [HarmonyPatch("SetupLicenses")]//
        private static class GetTheLayout
        {
            private static void Prefix(ManLicenses __instance)
            {
                Debug.Log("SubMissions: SetupLicenses - Injecting Unofficial corps beforehand");
                ManSMCCorps.PushUnofficialCorpsToPool();
            }
        }
        [HarmonyPatch(typeof(ManMods))]
        [HarmonyPatch("InjectModdedCorps")]//
        private static class RepoolOfficial
        {
            private static void Postfix(ManMods __instance)
            {
                Debug.Log("SubMissions: SetupLicenses - Injecting Official corps...");
                ManSMCCorps.ReloadAllOfficialIfApplcable();
                ManSMCCorps.ReloadAllUnofficialIfApplcable();
                ManTechMaterialSwap.inst.RebuildCorpArrayTextures();
                Debug.Log("SubMissions: ManTechMaterialSwap - Fetching MainTex");
                foreach (KeyValuePair<int, Material> CBR in ManTechMaterialSwap.inst.m_FinalCorpMaterials)
                {
                    Debug.Log("SubMissions: ManTechMaterialSwap - " + CBR.Key + " | " + CBR.Value);
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
                if ((int)corp > ManSMCCorps.UCorpRange)
                {
                    UICCorpLicenses.ShowFactionLicenseUnofficialUI((int)corp);
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
                if ((int)corporation > ManSMCCorps.UCorpRange)
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
                if ((int)corporation > ManSMCCorps.UCorpRange)
                {
                    if (ManSMCCorps.TryGetSMCCorpLicense((int)corporation, out SMCCorpLicense CL))
                    {
                        if (CL.HasCratePrefab)
                            ManSMCCorps.crateThing.RewardBlocksByCrate(CL.GetGradeUnlockBlocks(grade), Singleton.playerPos, corporation);
                        else
                            ManSMCCorps.crateThing.RewardBlocksByCrate(CL.GetGradeUnlockBlocks(grade), Singleton.playerPos, FactionSubTypes.GSO);
                        return false;
                    }
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
                if ((int)corp > ManSMCCorps.UCorpRange)
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
                if ((int)corp > ManSMCCorps.UCorpRange)
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
                if ((int)corp > ManSMCCorps.UCorpRange)
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
