using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using Nuterra.NativeOptions;
using Sub_Missions.ManWindows;
using TerraTechETCUtil;

#if !STEAM
using ModHelper.Config;
#else
using ModHelper;
#endif

namespace Sub_Missions
{
    /// <summary>
    /// TRYING to port to Steam - Heavily reliant on external file positioning
    /// </summary>
    public class KickStart
    {
        public const string ModName = "Sub_Missions";
        
        public static bool Debugger = false;
        public static bool ExportPrefabExample = false;
        public static bool OverrideRestrictions
        {
            get
            {
                bool yes = Debugger;
                try
                {
                    ManGameMode.GameType current = ManGameMode.inst.GetCurrentGameType();
                    if (current == ManGameMode.GameType.MainGame || current == ManGameMode.GameType.CoOpCampaign)
                        yes = false;
                }
                catch { yes = false; }
                return yes;
            }
        }

        public static Harmony harmonyInstance;
        private static bool patched = false;

        public static void Main()
        {
            Debug_SMissions.Log("Sub_Missions: MAIN (TTMM Version) startup");
            if (!VALIDATE_MODS())
                return;
            if (isSteamManaged)
            {
                MainOfficialInit();
                return;
            }
            SMissionJSONLoader.SetupWorkingDirectories();
            harmonyInstance = new Harmony("legioniteterratech.sub_missions");
            try
            {
                if (!patched)
                {
                    try
                    {
                        harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
                    }
                    catch (Exception e)
                    {
                        Debug_SMissions.Log("SubMissions: Error on mass patch");
                        Debug_SMissions.Log(e);
                    }
                    List<MethodBase> MP = harmonyInstance.GetPatchedMethods().ToList();
                    foreach (MethodBase MB in MP)
                    {
                        if (MB.Name == "PatchCCModding")
                        {
                            if (isBlockInjectorPresent)
                            {
                                Debug_SMissions.Log("SubMissions: Patching " + MB.Name);
                                //harmonyInstance.Patch(Patches.);
                            }
                            else
                            {
                                Debug_SMissions.Log("SubMissions: UnPatching " + MB.Name);
                                harmonyInstance.Unpatch(MB, HarmonyPatchType.All);
                            }
                        }
                        else
                        {
                            Debug_SMissions.Log("SubMissions: Patching " + MB.Name);
                            //harmonyInstance.Patch(MB);
                        }
                    }
                    patched = true;
                }
            }
            catch (Exception e)
            {
                Debug_SMissions.Log("SubMissions: Error on patch");
                Debug_SMissions.Log(e);
            };
            WindowManager.Initiate();
            ManSubMissions.Initiate();
            ButtonAct.Initiate();

            if (!isTACAIPresent)
            {
                TACAIRequiredWarning();
            }

            ManSubMissions.inst.HarvestAllTrees();
        }
        public static void DelayedInit()
        {
            ManSubMissions.Subscribe();
            BlockIndexer.ConstructBlockLookupListDelayed();
        }

        public static void MainOfficialInit()
        {
            Debug_SMissions.Log("SubMissions: MAIN (Steam Workshop Version) startup");
            if (!VALIDATE_MODS())
                return;
            SMissionJSONLoader.SetupWorkingDirectories();
            harmonyInstance = new Harmony("legioniteterratech.sub_missions");
            try
            {
                if (!patched)
                {
                    try
                    {
                        harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
                    }
                    catch (Exception e)
                    {
                        Debug_SMissions.Log("SubMissions: Error on mass patch");
                        Debug_SMissions.Log(e);
                    }
                    List<MethodBase> MP = harmonyInstance.GetPatchedMethods().ToList();
                    foreach (MethodBase MB in MP)
                    {
                        if (MB.Name == "PatchCCModding")
                        {
                            if (isBlockInjectorPresent)
                            {
                                Debug_SMissions.Log("SubMissions: Patching " + MB.Name);
                                //harmonyInstance.Patch(Patches.);
                            }
                            else
                            {
                                Debug_SMissions.Log("SubMissions: UnPatching " + MB.Name);
                                harmonyInstance.Unpatch(MB, HarmonyPatchType.All);
                            }
                        }
                        else
                        {
                            Debug_SMissions.Log("SubMissions: Patching " + MB.Name);
                            //harmonyInstance.Patch(MB);
                        }
                    }
                    patched = true;
                }
            }
            catch (Exception e)
            {
                Debug_SMissions.Log("SubMissions: Error on patch");
                Debug_SMissions.Log(e);
            }
            WindowManager.Initiate();
            ButtonAct.Initiate();
            ManSubMissions.Initiate();
            ManSubMissions.Subscribe();
            BlockIndexer.ConstructBlockLookupListDelayed();

            if (!isTACAIPresent)
            {
                TACAIRequiredWarning();
            }
            try
            {
                KickStartOptions.PushExtModOptionsHandling();
            }
            catch
            { 
            }

            ManSubMissions.inst.HarvestAllTrees();
        }

        public static void MainOfficialDeInit()
        {
            Debug_SMissions.Log("SubMissions: MAIN (Steam Workshop Version) shutdown");
            ManSubMissions.DeInit();
            ButtonAct.DeInit();
            WindowManager.DeInit();
            BlockIndexer.ResetBlockLookupList();

            if (patched)
            {
                try
                {
                    harmonyInstance.UnpatchAll("legioniteterratech.sub_missions");
                }
                catch (Exception e)
                {
                    Debug_SMissions.Log("SubMissions: Error on mass un-patch");
                    Debug_SMissions.Log(e);
                }
                patched = false;
            }
        }


        public static bool FirstTime = true;
        public static bool FullyLoadedGame = false;


        internal static bool isTACAIPresent = false;
        internal static bool isWaterModPresent = false;
        internal static bool isWeaponAimModPresent = false;
        internal static bool isBlockInjectorPresent = false;
        internal static bool isPopInjectorPresent = false;
        internal static bool isRandomAdditionsPresent = false;
        internal static bool isCustomCorpsFixPresent = false;


        internal static bool isSteamManaged = false;
        public static bool VALIDATE_MODS()
        {
            if (!LookForMod("NLogManager"))
            {
                isSteamManaged = false;
#if STEAM
                Debug_SMissions.FatalError("This mod NEEDS 0ModManager to function!  Please subscribe to it on the Steam Workshop and follow the instructions carefully.");
                return false;
#endif
            }
            else
                isSteamManaged = true;
            if (!LookForMod("0Harmony"))
            {
                Debug_SMissions.FatalError("This mod NEEDS Harmony to function!  Please subscribe to it on the Steam Workshop");
                return false;
            }

            if (LookForMod("TAC_AI"))
            {
                isTACAIPresent = true;
            }
            else
            {
#if STEAM
                Debug_SMissions.FatalError("This mod NEEDS Advanced AI to function!  Please subscribe to it on the Steam Workshop");
#else
                Debug_SMissions.FatalError("This mod NEEDS TACtical AI to function!  Please install/enable it in TTMM.");
#endif
                return false;
            }
            isWaterModPresent = LookForMod("WaterMod");
            isWeaponAimModPresent = LookForMod("WeaponAimMod");
            isBlockInjectorPresent = LookForMod("BlockInjector");
            isPopInjectorPresent = LookForMod("PopulationInjector");
            isRandomAdditionsPresent = LookForMod("RandomAdditions");
            isCustomCorpsFixPresent = LookForMod("TerraTechCustomCorpFix");
            return true;
        }
        public static void TACAIRequiredWarning()
        {
            SMUtil.Assert(false, "SubMissions: This mod has a very heavy dependancy on TACtical AIs, if that mod is not installed,\n  then the default tech AI won't be able to perform all the duties intended by the player!");
        }
        public static bool LookForMod(string name)
        {
            if (name == "RandomAdditions")
            {
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.FullName.StartsWith(name))
                    {
                        if (assembly.GetType("KickStart") != null)
                            return true;
                    }
                }
            }
            else
            {
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.FullName.StartsWith(name))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static Type LookForType(string name)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var typeGet = assembly.GetType(name);
                if (typeGet != null)
                {
                    return typeGet;
                }
            }
            return null;
        }
    }

    public class KickStartOptions
    {
        public static ModConfig Saver;
        public static OptionToggle editor;
        public static OptionToggle exportTemplate;

        private static bool launched = false;

        internal static void PushExtModOptionsHandling()
        {
            if (launched)
                return;
            launched = true;

            Saver = new ModConfig();
            Saver.BindConfig<KickStart>(null, "Debugger");
            Saver.BindConfig<KickStart>(null, "ExportPrefabExample");
            Saver.BindConfig<KickStart>(null, "FirstTime");

            if (KickStart.FirstTime)
            {
                KickStart.FirstTime = false;
            }

            var SubMissions = KickStart.ModName;
            editor = new OptionToggle("<b>ALLOW DEBUG</b> - WILL CONSUME PROCESSING POWER", SubMissions, KickStart.Debugger);
            editor.onValueSaved.AddListener(() =>
            {
                KickStart.Debugger = editor.SavedValue;
                try
                {
                    if (KickStart.Debugger)
                    {
                        WindowManager.ShowPopup(ManSubMissions.Button);
                        WindowManager.ShowPopup(ManSubMissions.SideUI);
                    }
                    else if (!ManGameMode.inst.IsCurrent<ModeMain>())
                    {
                        WindowManager.HidePopup(ManSubMissions.Button);
                        WindowManager.HidePopup(ManSubMissions.SideUI);
                    }
                }
                catch { }
            });
            exportTemplate = new OptionToggle("Export Mission Examples", SubMissions, KickStart.ExportPrefabExample);
            exportTemplate.onValueSaved.AddListener(() =>
            {
                KickStart.ExportPrefabExample = exportTemplate.SavedValue;
                SMissionJSONLoader.MakePrefabMissionTreeToFile("Template");
            });
            //if (ExportPrefabExample)
            //    SMissionJSONLoader.MakePrefabMissionTreeToFile("Template");
            NativeOptionsMod.onOptionsSaved.AddListener(() => { Save(); });

        }
        public static void Save()
        {
            Saver.WriteConfigJsonFile();
        }
    }

#if STEAM
    public class KickStartSubMissions : ModBase
    {
        internal static KickStartSubMissions oInst = null;

        bool isInit = false;
        public override bool HasEarlyInit()
        {
            return false;
        }

        public override void EarlyInit()
        {
        }
        public override void Init()
        {
            Debug_SMissions.Log("SubMissions: CALLED INIT");
            if (isInit)
                return;
            if (oInst == null)
                oInst = this;

            KickStart.MainOfficialInit();
            isInit = true;
        }
        public override void DeInit()
        {
            Debug_SMissions.Log("SubMissions: CALLED DE-INIT");
            if (!isInit)
                return;
            KickStart.MainOfficialDeInit();
            isInit = false;
        }
    }
#endif
}
