using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TerraTechETCUtil;
using HarmonyLib;


namespace Sub_Missions
{
    internal static class TerrainPatches
    {
        internal static class ManWorldPatches
        {
            internal static Type target = typeof(ManWorld);
            private static void Reset_Prefix(ManWorld __instance)
            {
                if (__instance.CurrentBiomeMap != null)
                {
                    Debug_SMissions.Log("Biomes reset");
                    //ManTerraformTool.ready = false;
                }
            }
        }
        /*
        internal static class BiomeMapDatabasePatches
        {
            internal static Type target = typeof(BiomeMap).GetNestedType("BiomeMapDatabase", BindingFlags.NonPublic);
            
            //AddOceanicBiomes
            private static void Init_Prefix(ref BiomeMap map)
            {
                WorldTerraformer.AddOceanicBiomes(map);
            }
        }*/

        /*
        [HarmonyPatch(typeof(MapGenerator.Operation))]
        [HarmonyPatch("Evaluate", new Type[2] { typeof(float), typeof(MapGenerator.Operation.ParamBuffer) })]// Setup new WorldTile
        internal static class ExpandWorld2
        {
            private static void Postfix(ref float __result)
            {
                __result = (__result * TerrainOperations.RescaleFactor) + TerrainOperations.DownOffsetScaled;
            }
        }
        [HarmonyPatch(typeof(MapGenerator.Operation))]
        [HarmonyPatch("Evaluate", new Type[2] { typeof(float), typeof(float) })]// Setup new WorldTile
        internal static class ExpandWorld3
        {
            private static void Postfix(ref float __result)
            {
                __result = (__result * TerrainOperations.RescaleFactorInv) - TerrainOperations.DownOffsetScaled;
            }
        }
        */


            }
        }
