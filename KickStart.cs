using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using ModHelper.Config;
using Nuterra.NativeOptions;
using Sub_Missions.ManWindows;

namespace Sub_Missions
{
    public class KickStart
    {
        const string ModName = "Sub_Missions";


        public static bool OpenEditor = false;
        public static bool OverrideRestrictions = true;

        public static bool Debugger = false;
        public static ModConfig Saver;

        public static void Main()
        {
            Harmony harmonyInstance = new Harmony("legioniteterratech.sub_missions");
            try
            {
                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e)
            {
                Debug.Log("SubMissions: Error on patch");
                Debug.Log(e);
            }

            CheckActiveMods();
            if (!isTACAIPresent)
            {
                TACAIRequiredWarning();
            }

            SMissionJSONLoader.SetupWorkingDirectories();
            WindowManager.Initiate();
            ManSubMissions.Initiate();
            ButtonAct.Initiate();

            Saver = new ModConfig();
            Saver.BindConfig<KickStart>(null, "Debugger");
            Saver.BindConfig<KickStart>(null, "FirstTime");

            SMissionJSONLoader.MakePrefabMissionTreeToFile("Template");
            if (FirstTime)
            {
                FirstTime = false;
            }

            var SubMissions = ModName;
            editor = new OptionToggle("<b>ALLOW DEBUG</b> - WILL CONSUME PROCESSING POWER", SubMissions, Debugger);
            editor.onValueSaved.AddListener(() => { Debugger = editor.SavedValue; Save(); });

            ManSubMissions.inst.HarvestAllTrees();
        }

        public static OptionToggle editor;
        public static OptionToggle NPCActOut;


        public static bool FirstTime = true;

        internal static bool isTACAIPresent = false;
        internal static bool isWaterModPresent = false;
        internal static bool isTougherEnemiesPresent = false;
        internal static bool isWeaponAimModPresent = false;
        internal static bool isBlockInjectorPresent = false;
        internal static bool isPopInjectorPresent = false;
        internal static bool isRandomAdditionsPresent = false;

        public static void Save()
        {
            Saver.WriteConfigJsonFile();
        }

        public static void CheckActiveMods()
        {
            if (LookForMod("TAC_AI"))
            {
                isTACAIPresent = true;
            }
            if (LookForMod("WaterMod"))
            {
                isWaterModPresent = true;
            }
            if (LookForMod("WeaponAimMod"))
            {
                isWeaponAimModPresent = true;
            }
            if (LookForMod("TougherEnemies"))
            {
                isTougherEnemiesPresent = true;
            }
            if (LookForMod("BlockInjector"))
            {
                isBlockInjectorPresent = true;
            }
            if (LookForMod("PopulationInjector"))
            {
                isPopInjectorPresent = true;
            }
            if (LookForMod("RandomAdditions"))
            {
                isRandomAdditionsPresent = true;
            }
        }
        public static void TACAIRequiredWarning()
        {
            Debug.Log("SubMissions: This mod has a very heavy dependancy on TACtical AIs, if that mod is not installed,\n  then the default tech AI won't be able to perform all the duties intended by the player!");
        }
        public static bool LookForMod(string name)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith(name))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
