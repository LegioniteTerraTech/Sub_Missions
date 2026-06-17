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
using TerraTechETCUtil;
#if !STEAM
using Nuterra.BlockInjector;
#endif


namespace Sub_Missions
{
    class PatchBatch { }

    internal static class Patches
    {

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
                    //Debug_SMissions.Log(KickStart.ModID + ": ManSMCCorps not dangerous");
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
                //Debug_SMissions.Log(KickStart.ModID + ": PatchCCModdingAfter - CALLED FOR " + block.Name);
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
                    Debug_SMissions.Log(KickStart.ModID + ": PatchCCModding - Error on block " + block.Name + " " + e);
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
                //Debug_SMissions.Log(KickStart.ModID + ": PatchCCModding - TRANSPILING");
                var codes = new List<CodeInstruction>(instructions);
                var codesOut = new List<CodeInstruction>();
                /*
                int line = 0;
                foreach (CodeInstruction CI in codes)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": PatchCCModding - TRANSPILER: code " + CI.opcode + " | line - " + line);
                    line++;
                }*/
                codesOut.AddRange(codes.Take(skipUntil).ToList());
                List<CodeInstruction> spacer = new List<CodeInstruction>();
                for (int step = 0; step < skipBit; step++)
                    spacer.Add(new CodeInstruction(opcode: OpCodes.Nop));
                codesOut.AddRange(spacer);
                codesOut.AddRange(codes.Skip(skipUntil + skipBit).ToList());
                Debug_SMissions.Log(KickStart.ModID + ": PatchCCModding - ------------ TRANSPILED ------------");
                //codesOut.AddRange(codes);
                /*
                Debug_SMissions.Log("\n");
                Debug_SMissions.Log("\n");
                Debug_SMissions.Log(KickStart.ModID + ": PatchCCModding - Checked lines of code, " + codes.Count + " confirmed in, final is " + codesOut.Count);
                line = 0;
                foreach (CodeInstruction CI in codesOut)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": PatchCCModding - TRANSPILER: code " + CI.opcode + " | line - " + line);
                    if (line == skipUntil)
                        Debug_SMissions.Log(KickStart.ModID + ": PatchCCModding - ------------ SNIPPED HERE!!!! ------------");
                    line++;
                }*/
                return codesOut;
            }
            
            private static void Prefix(ref CustomBlock block)
            {
                //Debug_SMissions.Log(KickStart.ModID + ": PatchCCModding - CALLED FOR " + block.Name);
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
                            Debug_SMissions.Log(KickStart.ModID + ": PatchCCModding - Reassigned Corp of " + block.Name + " to " + CL.ID);
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
                    Debug_SMissions.Log(KickStart.ModID + ": PatchCCModding - Error on block " + block.Name + " " + e);
                }
            }
            private static void Postfix(ref CustomBlock block)
            {
                //Debug_SMissions.Log(KickStart.ModID + ": PatchCCModding - CALLED FOR " + block.Name);
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
                    Debug_SMissions.Log(KickStart.ModID + ": PatchCCModding(Postfix) - Error on block " + block.Name + " " + e);
                }
            }
        }
#endif


    }
}
