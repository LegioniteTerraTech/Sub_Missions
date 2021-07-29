using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace Sub_Missions
{
    class PatchBatch { }

    internal static class Patches
    {
        [HarmonyPatch(typeof(ModeAttract))]
        [HarmonyPatch("SetupTechs")]// Setup main menu techs
        private static class Subscribe
        {
            private static void Postfix()
            {
                ManSubMissions.Subscribe();
                Debug.Log("SubMissions: Core module hooks launched");
            }
        }

        [HarmonyPatch(typeof(ManSaveGame))]
        [HarmonyPatch("Save")]// SAAAAAVVE
        private static class SaveTheMissions
        {
            private static void Prefix()
            {
                Debug.Log("SubMissions: ManSubMissions Saving!");
                SaveManSubMissions.SaveDataAutomatic();
            }
        }
    }
}
