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
using System.IO;


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
        internal const string ModID = "Mod Missions";
        public const string ModName = "Mod Missions";
        
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

        public static Harmony harmonyInstance = new Harmony("legioniteterratech.sub_missions");
        private static bool patched = false;


        public static void Main()
        {
            Debug_SMissions.Log(KickStart.ModID + ": MAIN (TTMM Version) startup");
            if (!VALIDATE_MODS())
                return;
            if (isSteamManaged)
            {
                MainOfficialInit();
                return;
            }
            SMissionJSONLoader.SetupWorkingDirectories();
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
                        Debug_SMissions.Log(KickStart.ModID + ": Error on mass patch");
                        Debug_SMissions.Log(e);
                    }
                    foreach (MethodBase MB in harmonyInstance.GetPatchedMethods())
                    {
                        if (MB.Name == "PatchCCModding")
                        {
                            if (isBlockInjectorPresent)
                            {
                                Debug_SMissions.Log(KickStart.ModID + ": Patching " + MB.Name);
                                //harmonyInstance.Patch(Patches.);
                            }
                            else
                            {
                                Debug_SMissions.Log(KickStart.ModID + ": UnPatching " + MB.Name);
                                harmonyInstance.Unpatch(MB, HarmonyPatchType.All);
                            }
                        }
                        else
                        {
                            Debug_SMissions.Log(KickStart.ModID + ": Patching " + MB.Name);
                            //harmonyInstance.Patch(MB);
                        }
                    }
                    patched = true;
                }
            }
            catch (Exception e)
            {
                Debug_SMissions.Log(KickStart.ModID + ": Error on patch");
                Debug_SMissions.Log(e);
            };
            ManSubMissions.Initiate();
            ButtonAct.Initiate();

            if (!isTACAIPresent)
            {
                TACAIRequiredWarning();
            }
            else

            ManSubMissions.inst.ReloadAllMissionTrees();
        }
        public static void IncreaseAIHeightRange()
        {
            TAC_AI.KickStart.TerrainHeight = TerrainOperations.TileHeightRescaled;
        }
        public static void DelayedInit()
        {
            ManSubMissions.Subscribe();
            BlockIndexer.ConstructBlockLookupListDelayed();
        }

        public static void MainOfficialInit()
        {
            Debug_SMissions.Log(KickStart.ModID + ": MAIN (Steam Workshop Version) startup");
            if (!VALIDATE_MODS())
                return;
            DebugExtUtilities.AllowEnableDebugGUIMenu_KeypadEnter = true;
            SMissionJSONLoader.SetupWorkingDirectories();

            LegModExt.InsurePatches();
            CursorChanger.AddNewCursors();

            WorldTerraformer.Init();

            SubMissionsWiki.InitWiki();

            try
            {
                if (!patched)
                {
                    try
                    {
                        if (!MassPatcher.MassPatchAllWithin(harmonyInstance, typeof(GlobalPatches), "SubMissions"))
                            Debug_SMissions.FatalError("Error on patching GlobalPatches");
                        if (!MassPatcher.MassPatchAllWithin(harmonyInstance, typeof(ProgressionPatches), "SubMissions"))
                            Debug_SMissions.FatalError("Error on patching ProgressionPatches");
                        if (!MassPatcher.MassPatchAllWithin(harmonyInstance, typeof(UIPatches), "SubMissions"))
                            Debug_SMissions.FatalError("Error on patching UIPatches");
                        if (!MassPatcher.MassPatchAllWithin(harmonyInstance, typeof(TerrainPatches), "SubMissions"))
                            Debug_SMissions.FatalError("Error on patching TerrainPatches");

                        harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
                        Debug_SMissions.Log(KickStart.ModID + ": Patched");
                    }
                    catch (Exception e)
                    {
                        Debug_SMissions.Log(KickStart.ModID + ": Error on mass patch");
                        Debug_SMissions.Log(e);
                        Debug_SMissions.FatalError("Error on patching base game");
                    }
                    foreach (MethodBase MB in harmonyInstance.GetPatchedMethods())
                    {
                        if (MB.Name == "PatchCCModding")
                        {
                            if (isBlockInjectorPresent)
                            {
                                Debug_SMissions.Log(KickStart.ModID + ": Patching " + MB.Name);
                                //harmonyInstance.Patch(Patches.);
                            }
                            else
                            {
                                Debug_SMissions.Log(KickStart.ModID + ": UnPatching " + MB.Name);
                                harmonyInstance.Unpatch(MB, HarmonyPatchType.All);
                            }
                        }
                        else
                        {
                            Debug_SMissions.Log(KickStart.ModID + ": Patching " + MB.Name);
                            //harmonyInstance.Patch(MB);
                        }
                    }
                    patched = true;
                }
            }
            catch (Exception e)
            {
                Debug_SMissions.Log(KickStart.ModID + ": Error on patch");
                Debug_SMissions.Log(e);
            }
            try
            {
                SafeSaves.ManSafeSaves.RegisterSaveSystem(Assembly.GetExecutingAssembly(), OnSaveManagers, OnLoadManagers);
            }
            catch (Exception e)
            {
                Debug_SMissions.Log(KickStart.ModID + ": Error on RegisterSaveSystem");
                Debug_SMissions.Log(e);
            }
            ButtonAct.Initiate();
            ManSubMissions.Initiate();
            ManSubMissions.Subscribe();
            BlockIndexer.ConstructBlockLookupListDelayed();
            ManModGUI.RequestInit(KickStartSubMissions.mInst);

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

            ManSubMissions.inst.ReloadAllMissionTrees();
        }

        public static void MainOfficialDeInit()
        {
            Debug_SMissions.Log(KickStart.ModID + ": MAIN (Steam Workshop Version) shutdown");
            ManModGUI.DeInit(KickStartSubMissions.mInst);
            ManSubMissions.DeInit();
            ButtonAct.DeInit();
            BlockIndexer.ResetBlockLookupList();

            try
            {
                SafeSaves.ManSafeSaves.UnregisterSaveSystem(Assembly.GetExecutingAssembly(), OnSaveManagers, OnLoadManagers);
            }
            catch (Exception e)
            {
                Debug_SMissions.Log(KickStart.ModID + ": Error on UnregisterSaveSystem");
                Debug_SMissions.Log(e);
            }
            if (patched)
            {
                try
                {
                    MassPatcher.MassUnPatchAllWithin(harmonyInstance, typeof(TerrainPatches), "SubMissions");
                    MassPatcher.MassUnPatchAllWithin(harmonyInstance, typeof(UIPatches), "SubMissions");
                    MassPatcher.MassUnPatchAllWithin(harmonyInstance, typeof(ProgressionPatches), "SubMissions");
                    MassPatcher.MassUnPatchAllWithin(harmonyInstance, typeof(GlobalPatches), "SubMissions");
                    harmonyInstance.UnpatchAll("legioniteterratech.sub_missions");
                }
                catch (Exception e)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": Error on mass un-patch");
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
            SMUtil.Log(false, KickStart.ModID + ": This mod has a very heavy dependancy on TACtical AIs, if that mod is not installed,\n  then the default tech AI won't be able to perform all the duties intended by the player!");
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


        public static void OnSaveManagers(bool Doing)
        {
            if (Doing)
            {
                WorldTerraformer.PrepareForSaving();
                SaveManSubMissions.PrepareForSaving();
            }
            else
            {
                SaveManSubMissions.FinishedSaving();
                WorldTerraformer.FinishedSaving();
            }
        }
        public static void OnLoadManagers(bool Doing)
        {
            if (!Doing)
            {
                WorldTerraformer.FinishedLoading();
                SaveManSubMissions.FinishedLoading();
            }
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
                SimpleSMissions.MakePrefabMissionTreeToFile("Template");
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
        internal static ModDataHandle oInst;
        internal static KickStartSubMissions mInst;

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
            Debug_SMissions.Log(KickStart.ModID + ": CALLED INIT");
            if (isInit)
                return;
            try
            {
                mInst = this;
                oInst = new ModDataHandle(KickStart.ModID);
                oInst.DebugLogModContents();
                TerraTechETCUtil.ModStatusChecker.EncapsulateSafeInit(KickStart.ModID,
                    KickStart.MainOfficialInit, KickStart.MainOfficialDeInit);
            }
            catch { }
            isInit = true;
        }
        public override void DeInit()
        {
            Debug_SMissions.Log(KickStart.ModID + ": CALLED DE-INIT");
            if (!isInit)
                return;
            KickStart.MainOfficialDeInit();
            isInit = false;
        }
    }
#endif
}
