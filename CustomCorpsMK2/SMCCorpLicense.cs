using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json;
using FMOD;
using FMODUnity;
using TerraTechETCUtil;

#if !STEAM
using Nuterra.BlockInjector;
#endif

namespace Sub_Missions
{
    // The extended version of a Custom Corp License to fill in further details like grade blocks and whatnot.
    /// <summary>
    /// Only handles Unofficial for now...
    /// </summary>
    public class SMCCorpLicense
    {
        [JsonIgnore]
        static FieldInfo sess = typeof(ManMods).GetField("m_CurrentSession", BindingFlags.NonPublic | BindingFlags.Instance);
        [JsonIgnore]
        static FieldInfo skinS = typeof(ManCustomSkins).GetField("m_SkinUIInfos", BindingFlags.NonPublic | BindingFlags.Instance);
        [JsonIgnore]
        static FieldInfo crateS = typeof(ManSpawn).GetField("m_CorpCratePrefabsDict", BindingFlags.NonPublic | BindingFlags.Instance);

        //
        public string FullName = "NuLl";    // The FULL Name
        public string Faction = null;     // The SHORTENED name
        public string Lore = "A corporation so mysterious, we have no intel on it!";
        public Dictionary<string, string> MoreLore = new Dictionary<string, string>();
        public bool BlocksUseFullName = false;  // Should we check for the fullname instead?
        //public FactionSubTypes EngineReferenceFaction = FactionSubTypes.GSO;    // The Faction to reference for the engine. Due to FMOD this cannot be customized.
        //public FactionSubTypes CombatMusicFaction = FactionSubTypes.GSO;    // The Faction to reference for the combat music. Due to FMOD this cannot be customized.
        public FactionSubTypes SkinReferenceFaction = FactionSubTypes.NULL; // The Faction to reference for skins. Leave "NULL" to use only your own.
        public FactionSubTypes CrateReferenceFaction = FactionSubTypes.NULL;// The Faction to reference for Delivery crates. You can override the models with your own.
        public int ID = -3;                 // MUST be set to a unique value above 50000
        public float minEmissive = 0.25f;

        public int[] GradesXP;

        public string FirstCabUnlocked = "GSOCockpit_111";
        public List<SMCCorpBlockRange> GradeUnlockBlocks = new List<SMCCorpBlockRange>();
        [JsonIgnore]
        public List<Texture> TexturesCache = new List<Texture>();

        // Used for combat
        public List<string> BattleMusic = new List<string>();

        // Used for Inventory
        public string StandardCorpIcon = null;
        public string SelectedCorpIcon = null;
        public string HighResolutionCorpIcon = null;

        public string ExampleSkinTech = null;


        [JsonIgnore]
        public Sprite SmallCorpIcon = null;         // set by string ref
        [JsonIgnore]
        public Sprite SmallSelectedCorpIcon = null; // set by string ref
        [JsonIgnore]
        public Sprite HighResCorpIcon = null;       // set by string ref
        [JsonIgnore]
        public TankPreset SkinRefTech = null;         // set by string ref
        [JsonIgnore]
        private BlockUnlockTable.CorpBlockData corpBlockData = null;         // set by string ref
        [JsonIgnore]
        public List<int> registeredSkins = new List<int>();
        [JsonIgnore]
        public List<int> importedSkins = new List<int>();
        [JsonIgnore]
        public bool HasCratePrefab = false;
        [JsonIgnore]
        public Crate CratePrefab;

        public string GetLogName(string context = null)
        {
            if (context == null)
                return "Corp [" + (OfficialCorp ? "Official" : "Unofficial") + "] ~ " + Faction;
            return "Corp [" + (OfficialCorp ? "Official" : "Unofficial") + "] (" + context + ") ~ " + Faction;
        }

        private static string openedLore = null;
        public void GetAdditionalLore()
        {
            foreach (var case1 in MoreLore)
            {
                if (GUILayout.Button(case1.Value, AltUI.ButtonBlueLarge))
                {
                    if (openedLore == case1.Key)
                        openedLore = null;
                    else
                        openedLore = case1.Key;
                }
                if (openedLore == case1.Key)
                {
                    GUILayout.Label(case1.Value, AltUI.TextfieldBlackHuge);
                }
            }
        }

        [JsonIgnore]
        public Dictionary<int, Sprite> CSIRenderCache = new Dictionary<int, Sprite>();

        [JsonIgnore]
        public static bool needsToRenderSkins = true;
        [JsonIgnore]
        public static List<CorporationSkinInfo> CSIBacklogRender = new List<CorporationSkinInfo>();
        [JsonIgnore]
        public static int countWorked = 0;

        internal bool OfficialCorp = false;

        private static FieldInfo TPTechData = typeof(TankPreset)
                   .GetField("m_TechData", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        public static void TryReRenderCSIBacklog()
        {   //
            try
            {
                if (ManWorld.inst.CheckIsTileAtPositionLoaded(Vector3.zero))
                {
                    if (CSIBacklogRender.Count <= countWorked)
                    {
                        needsToRenderSkins = false;
                        CSIBacklogRender.Clear();
                        countWorked = 0;
                        return;
                    }
                    CorporationSkinInfo CSI = CSIBacklogRender[countWorked];
                    if (ManSMCCorps.TryGetSMCCorpLicense((int)CSI.m_Corporation, out SMCCorpLicense CL))
                    {
                        if (!CL.ExampleSkinTech.NullOrEmpty())
                        {
                            if (CL.GetDirectory(out string directory))
                            {
                                string directed = Path.Combine(directory, CL.ExampleSkinTech);
                                if (File.Exists(directed))
                                {
                                    Texture2D T2D = FileUtils.LoadTexture(directed);
                                    if (T2D && ManScreenshot.TryDecodeSnapshotRender(T2D, out TechData.SerializedSnapshotData TSS))
                                    {
                                        TankPreset TP = TankPreset.CreateInstance();
                                        TPTechData.SetValue(TP, TSS.CreateTechData()); // Creates the base instance
                                        CL.SkinRefTech = TP;
                                    }
                                    else
                                        SMUtil.Error(false, CL.GetLogName("Skins"), KickStart.ModID + ": TryReRenderCSIBacklog(SMCCorpLicense) " +
                                            "- Could not load ExampleSkinTech for " + CL.Faction + 
                                            ". \n Tech is corrupted or needed blocks are missing");
                                }
                                else
                                    SMUtil.Error(false, CL.GetLogName("Skins"), KickStart.ModID + ": TryReRenderCSIBacklog(SMCCorpLicense) " +
                                        "- Could not load ExampleSkinTech for " + CL.Faction + 
                                        ". \n Check your naming and files");
                            }
                        }
                        if (CL.SkinRefTech == null)
                            CL.MakeFallbackTechIfNeeded();
                        if (CL.SkinRefTech == null)
                        {
                            Debug_SMissions.Log(KickStart.ModID + ": SkinRefTech IS NULL");
                            countWorked++;
                            return;
                        }
                        TechData TD = CL.SkinRefTech.GetTechDataFormatted();
                        if (TD == null)
                        {
                            Debug_SMissions.Log(KickStart.ModID + ": SkinRefTech's TechData IS NULL");
                            countWorked++;
                            return;
                        }
                        if (CL.SkinRefTech)
                        {
                            if (TD.m_BlockSpecs != null)
                            {
                                for (int step = 0; step < TD.m_BlockSpecs.Count; step++)
                                {
                                    try
                                    {
                                        TankPreset.BlockSpec value = TD.m_BlockSpecs[step];
                                        /*
                                        if ((int)Singleton.Manager<ManSpawn>.inst.GetCorporation(value.GetBlockType()) != CL.ID)
                                        {
                                            Debug_SMissions.Log(KickStart.ModID + ": TryReRenderCSIBacklog(SMCCorpLicense) - Example Tech must use only the same blocks from the respective corp!");
                                            continue;
                                        }*/
                                        value.m_SkinID = (byte)CSI.m_SkinUniqueID;
                                        TD.m_BlockSpecs[step] = value;
                                    }
                                    catch
                                    {
                                        Debug_SMissions.Log(KickStart.ModID + ": TryReRenderCSIBacklog(SMCCorpLicense) - BlockSpec IS NULL");
                                    }
                                }
                            }
                            else
                            {
                                Debug_SMissions.Log(KickStart.ModID + ": TryReRenderCSIBacklog(SMCCorpLicense) - BlockSpec IS NULL BY DEFAULT");
                            }
                            Singleton.Manager<ManScreenshot>.inst.RenderTechImage(TD, new IntVector2(128, 128), false, delegate (TechData techData, Texture2D tex)
                            {
                                Rect rect = new Rect(Vector2.zero, new Vector2((float)tex.width, (float)tex.height));
                                Sprite output = Sprite.Create(tex, rect, Vector2.zero);
                                if (!CL.CSIRenderCache.TryGetValue(CSI.m_SkinUniqueID, out _))
                                    CL.CSIRenderCache.Add(CSI.m_SkinUniqueID, output);
                                CSI.m_SkinUIInfo.m_PreviewImage = output;
                            });
                        }
                    }
                    countWorked++;
                    if (CSIBacklogRender.Count - 1 < countWorked)
                    {
                        Debug_SMissions.Log(KickStart.ModID + ": MirrorSkin(SMCCorpLicense) - Rendered all");
                        needsToRenderSkins = false;
                        CSIBacklogRender.Clear();
                        countWorked = 0;
                    }
                }
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "All Corps (Skins)", KickStart.ModID + ": MirrorSkin(SMCCorpLicense) - CRITICAL ERROR " +
                    "- CORRUPTED ATTEMPT", e);
                needsToRenderSkins = false;
                CSIBacklogRender.Clear();
                countWorked = 0;
            }
        }

        /// <summary>
        /// DO NOT CALL!!! 
        /// <para><b>USED ONLY FOR NEWTONSOFT JSON LOADING!</b></para>
        /// </summary>
        public SMCCorpLicense() { }

        /// <summary>
        /// For Official Mods init. 
        /// <para><b>MUST BE RELOADED AFTER EACH NEW SAVE-GAME SESSION INIT</b></para>
        /// </summary>
        /// <param name="FactionName"></param>
        public SMCCorpLicense(FactionSubTypes ID, ModdedCorpDefinition MCD)
        {
            FullName = ManMods.inst.FindCorpName(ID);
            Faction = ManMods.inst.FindCorpShortName(ID);
            this.ID = (int)ID;
            OfficialCorp = true;
            GradesXP = new int[5] { 100, 250, 1000, 2000, 4000 };
            SMUtil.Info(false, "Corp [Official] (Startup) ~ " + Faction, KickStart.ModID + ": SMCCorpLicense(OfficialMods) - Init for " + MCD.m_DisplayName);
            TryInitFactionEXPSys();
        }
        /// <summary>
        /// For standard Init, use ManSMCCorps to create new
        /// </summary>
        /// <param name="FactionName"></param>
        public SMCCorpLicense(string FactionName, int ID, int[] Grades, bool overrideStartup = false)
        {
            FullName = FactionName;
            Faction = FactionName;
            this.ID = ID;
            GradesXP = Grades;
            Debug_SMissions.Log(KickStart.ModID + ": SMCCorpLicense(Scratch) - Init");
            if (!overrideStartup)
                TryInitFactionEXPSys();
        }

#if !STEAM
        /// <summary>
        /// For initing with BlockInjector, use ManSMCCorps to create new
        /// </summary>
        /// <param name="CC"></param>
        public SMCCorpLicense(CustomCorporation CC)
        {
            FullName = CC.Name;
            Faction = CC.Name;
            ID = CC.CorpID;
            GradesXP = CC.XPLevels;
            SmallCorpIcon = CC.CorpIcon;
            SmallSelectedCorpIcon = CC.SelectedCorpIcon;
            HighResCorpIcon = CC.ModernCorpIcon;
            Debug_SMissions.Log(KickStart.ModID + ": SMCCorpLicense(CustomCorp) - Init");
            TryInitFactionEXPSys();
        }
#endif

        public string GetCorpNameForBlocks()
        {   //
            if (BlocksUseFullName)
                return FullName;
            return Faction;
        }
        public int GetNameHashForBlocks()
        {   //
            return GetCorpNameForBlocks().GetHashCode();
        }

        private static List<BlockTypes> boundle = new List<BlockTypes>();
        private static List<BlockTypes> boundle2 = new List<BlockTypes>();
        public BlockTypes[] GetRandomBlocks(int grade, int amount)
        {   //
            try
            {
                if (GradeUnlockBlocks.Count == 0)
                    return new BlockTypes[1] { BlockIndexer.StringToBlockType(FirstCabUnlocked) };
                boundle2.Clear();
                int maxGradeVal = grade > GradeUnlockBlocks.Count - 1 ? GradeUnlockBlocks.Count - 1 : grade;
                for (int step = 0; maxGradeVal > step; step++)
                    boundle.AddRange(GradeUnlockBlocks[step].BlocksAvail);
                for (int step2 = 0; amount > step2; step2++)
                    boundle2.Add(boundle.GetRandomEntry());
                boundle.Clear();
                return boundle2.ToArray();
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, GetLogName("Reward"), KickStart.ModID + ": GetRandomBlocks(SMCCorpLicense) - BLOCK could not be obtained", e);
            }
            return new BlockTypes[1] { BlockTypes.GSOCockpit_111 };
        }
        public BlockTypes GetRandomBlock(int grade)
        {   //
            try
            {
                if (GradeUnlockBlocks.Count == 0)
                    return BlockIndexer.StringToBlockType(FirstCabUnlocked);
                if (grade > GradeUnlockBlocks.Count - 1)
                {
                    return GradeUnlockBlocks.Last().GetRandomBlock();
                }
                return GradeUnlockBlocks[grade].GetRandomBlock();
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, GetLogName("Reward"), KickStart.ModID + ": GetRandomBlock(SMCCorpLicense) - BLOCK could not be obtained", e);
            }
            return BlockTypes.GSOAIController_111;
        }
        public BlockTypes[] GetGradeUnlockBlocks(int grade)
        {
            try
            {
                if (GradeUnlockBlocks.Count != 0)
                {
                    if (grade > GradeUnlockBlocks.Count - 1)
                    {
                        return GradeUnlockBlocks.Last().GetGradeUpBlocks();
                    }
                    return GradeUnlockBlocks[grade].GetGradeUpBlocks();
                }
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, GetLogName("Reward"), KickStart.ModID + ": GetGradeUnlockBlocks(SMCCorpLicense) - BLOCK could not be obtained", e);
            }
            var fCab = BlockIndexer.StringToBlockType(FirstCabUnlocked);
            return new BlockTypes[3] { fCab, fCab, fCab };
        }

        [JsonIgnore]
        internal List<Sound> combatMusicLoaded = new List<Sound>();
      

        // Audio Test
        internal List<Sound> GetCombatMusics()
        {
            return combatMusicLoaded;
        }
        /// <summary>
        /// Credit to Exund for looking to FMOD!
        /// </summary>
        internal void RegisterCorpMusics()
        {   //
            if (BattleMusic == null)
            {
                Debug_SMissions.Log(KickStart.ModID + ": RegisterCorpMusics - No custom corp music for " + Faction + ".");
                return;
            }
            ManSMCCorps.sys = RuntimeManager.LowlevelSystem;
            foreach (string fileName in BattleMusic)
            {
                try
                {
                    string GO;
                    if (GetDirectory(out string directory))
                    {
                        GO = Path.Combine(Path.GetFullPath(directory), fileName);
                        Debug_SMissions.Log(KickStart.ModID + ": RegisterCorpMusics - " + GO);
                        ManSMCCorps.sys.createSound(GO, FMOD.MODE.CREATESAMPLE | FMOD.MODE.ACCURATETIME, out Sound newSound);
                        combatMusicLoaded.Add(newSound);
                        Debug_SMissions.Log(KickStart.ModID + ": RegisterCorpMusics - The corp music for " + Faction + " named " + fileName + " loaded correctly");
                    }
                }
                catch (Exception e)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": RegisterCorpMusics - Error " + e);
                }
            }
            if (combatMusicLoaded.Count == 0)
            {
                Debug_SMissions.Log(KickStart.ModID + ": RegisterCorpMusics - No custom corp music built for " + Faction + ".");
            }
        }
        internal void TryInitFactionEXPSys()
        {   //

            //if ((int)CombatMusicFaction > Enum.GetNames(typeof(FactionSubTypes)).Length - 1 || (int)CombatMusicFaction < -1)
            //    CombatMusicFaction = FactionSubTypes.GSO;
            int errorLevel = 0;
            try
            {
                FactionSubTypes corpID = (FactionSubTypes)ID;
                errorLevel++;
                if (ManSMCCorps.IsUnofficialSMCCorpLicense(corpID)) // Unoffical mods
                {   // We enable support for EVERYTHING in the tree for the corp!
                    errorLevel++;
                    BuildFactionLicenseUnofficial();
                    errorLevel += 12;
                }
                else if (ManMods.inst.IsModdedCorp(corpID)) // Official mods
                {   // Official Support is mostly a dev matter
                    BuildFactionLicenseOfficial();
                }
                errorLevel++;
                // It's vanilla or null - take no action.
                RefreshCorpUISP();
            }
            catch (Exception e)
            {
                Debug_SMissions.Log(KickStart.ModID + ": TryInitFactionEXPSys - Error level " + errorLevel + " - " + e);
            }
        }
        /// <summary>
        /// Builds all of the visual elements of the Custom Corp
        /// </summary>
        internal void RefreshCorpUISP()
        {   //
            try
            {
                FactionSubTypes corpID = (FactionSubTypes)ID;
                if (ManSMCCorps.IsUnofficialSMCCorpLicense(corpID)) // Unofficial mods
                {   // We enable support for EVERYTHING in the tree for the corp!
                    RefreshCorpUISPUnofficial(corpID);
                }
                else
                {   // Official Mods
                    if (ManMods.inst.GetCorpDefinition(corpID) != null)
                    {
                        if (GradeUnlockBlocks.Count == 0)
                        {
                            OfficialTryBuildBlockTypes(corpID);
                            //SaveToDisk();
                        }
                        if (SkinRefTech == null)
                        {
                            if (!ManSpawn.inst)
                            {
                                SMUtil.Log(false, KickStart.ModID + ": ManSpawn is still loading");
                                return;
                            }
                            else
                                MakeFallbackTechIfNeeded();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SMUtil.Info(false, "Backend", KickStart.ModID + ": GAME is still loading " + e);
            }
        }

        internal void RefreshCorpUISPUnofficial(FactionSubTypes corpID)
        {
#if !STEAM
            if (GradeUnlockBlocks.Count == 0)
            {
                if (BlockLoader.CustomBlocks.Count > 0)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": REBUILDING GradeUnlockBlocks for " + Faction);
                    UnofficialTryBuildBlockTypes();
                    SaveToDisk();
                }
                else
                    Debug_SMissions.Log(KickStart.ModID + ": Waiting for Nuterra.BlockInjector to load...");
            }
            if (SkinRefTech == null)
            {
                if (!ManSpawn.inst)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": ManSpawn is still loading");
                    return;
                }
                else
                {
                    MakeFallbackTechIfNeeded();
                }
            }
            if (!ManPurchases.inst)
            {
                Debug_SMissions.Log(KickStart.ModID + ": ManPurchases is still loading");
                return;
            }
            else
            {
                if (!ManPurchases.inst.AvailableCorporations.Contains(corpID))
                {
                    if (SkinReferenceFaction == FactionSubTypes.NULL && BlockLoader.CustomBlocks.Count() == 0)
                    {
                        Debug_SMissions.Log(KickStart.ModID + ": Waiting for blocks to compile...");
                        return;
                    }
                    Debug_SMissions.Log(KickStart.ModID + ": Making Corp Identifier for " + Faction + ", ID " + (int)corpID);
                    //UICCorpLicenses.MakeFactionLicenseUnofficialUI(this);
                    ManPurchases.inst.AddCustomCorp((int)corpID);

                    ManCustomSkins.inst.AddCorp((int)corpID);

                    BuildSkins();
                }
            }
#endif
        }

        internal void BuildUnofficialSkins()
        {   //
            try
            {
                if (OfficialCorp)
                    return; // Already done.
                FactionSubTypes corpID = (FactionSubTypes)ID;
                if ((int)SkinReferenceFaction >= Enum.GetValues(typeof(FactionSubTypes)).Length)
                    SkinReferenceFaction = FactionSubTypes.GSO;
                if (SkinReferenceFaction != FactionSubTypes.NULL)
                {
                    registeredSkins.Clear();
                    int count = ManCustomSkins.inst.GetNumSkinsInCorp(SkinReferenceFaction);
                    for (int step = 0; step < count; step++)
                    {
                        if ((int)SkinReferenceFaction >= Enum.GetValues(typeof(FactionSubTypes)).Length || !ManCustomSkins.inst.CanUseSkin(SkinReferenceFaction, step))
                        {
                            SMUtil.Log(false, KickStart.ModID + ": Could not make a Corp Skin for ID " + (int)corpID + " skinIndex " + step +
                                " Required DLC not present");
                            continue;
                        }
                        Debug_SMissions.Log(KickStart.ModID + ": Making a Corp Skin for ID " + (int)corpID + " skinIndex " + step);
                        CorporationSkinInfo CSI = ScriptableObject.CreateInstance<CorporationSkinInfo>();
                        if (MirrorSkin(ref CSI, SkinReferenceFaction, step, out int ID))
                        {
                            ManCustomSkins.inst.AddSkinToCorp(CSI, true);// we keep this true so that it can self-purge to prevent nasty corruptions with Official Modding
                            registeredSkins.Add(ID);
                        }
                    }
                    PushExternalCorpSkins();
                }
                else
                {
                    PushExternalCorpSkins();
                    int skinCount = ManCustomSkins.inst.GetNumSkinsInCorp(corpID);
                    if (skinCount == 0)
                    {
                        SMUtil.Error(false, "Corp [Unofficial] (Skins) ~ " + Faction, "Corp " + Faction + " HAS NO VALID TEXTURES!!!");
                        /*
                        CorporationSkinInfo CSI = ScriptableObject.CreateInstance<CorporationSkinInfo>();
                        if (SkinFromExisting(ref CSI, SkinReferenceFaction, 0, out int ID))
                        {
                            ManCustomSkins.inst.AddSkinToCorp(CSI, true);// we keep this true so that it can self-purge to prevent nasty corruptions with Official Modding
                            registeredSkins.Add(ID);
                        }
                        CorporationSkinInfo CSI2 = ScriptableObject.CreateInstance<CorporationSkinInfo>();
                        if (SkinFromExisting(ref CSI2, SkinReferenceFaction, 1, out int ID2))
                        {
                            ManCustomSkins.inst.AddSkinToCorp(CSI2, true);// we keep this true so that it can self-purge to prevent nasty corruptions with Official Modding
                            registeredSkins.Add(ID2);
                        }
                        */
                    }
                }
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, GetLogName("Skins"), KickStart.ModID + ": BuildTextures - Error on compiling skins for " + Faction, e);
            }
        }
        internal bool MirrorSkin(ref CorporationSkinInfo toChange, FactionSubTypes corpID, int skinIndex, out int skinID)
        {   //
            int error = 0;
            skinID = 0;
            try
            {
                error++;
                CorporationSkinUIInfo UII = new CorporationSkinUIInfo();
                CorporationSkinUIInfo UIIref;
                error++;
                //Dictionary<int, List<CorporationSkinUIInfo>> dict = (Dictionary<int, List<CorporationSkinUIInfo>>)skinS.GetValue();
                error++;
                skinID = ManCustomSkins.inst.SkinIndexToID((byte)skinIndex, corpID);
                if (Singleton.Manager<ManDLC>.inst.IsSkinLocked(skinID, corpID))
                {
                    byte skinIDDefault = ManCustomSkins.inst.SkinIndexToID(0, corpID);
                    byte skinIndexDefault = ManCustomSkins.inst.SkinIDToIndex(skinIDDefault, corpID);
                    toChange.m_Corporation = (FactionSubTypes)ID;
                    toChange.m_SkinUniqueID = skinID;

                    toChange.m_SkinMeshes = new SkinMeshes();
                    error++;
                    toChange.m_SkinTextureInfo = ManCustomSkins.inst.GetSkinTexture(corpID, skinIndexDefault);
                    error++;

                    UIIref = ManCustomSkins.inst.GetSkinUIInfo(corpID, skinIndexDefault);
                    error++;
                    if (CSIRenderCache.TryGetValue(skinID, out Sprite val))
                    {
                        UII.m_PreviewImage = val;
                    }
                    else
                    {
                        Debug_SMissions.Log(KickStart.ModID + ": MirrorSkin(SMCCorpLicense[SkinLocked]) - RenderTechImage needs time to boot");
                        CSIBacklogRender.Add(toChange);
                        UII.m_PreviewImage = UIIref.m_PreviewImage;
                        needsToRenderSkins = true;
                    }
                    error++;
                    UII.m_SkinButtonImage = UIIref.m_SkinButtonImage;
                    UII.m_SkinMiniPaletteImage = UIIref.m_SkinMiniPaletteImage;
                    UII.m_SkinLocked = false;
                    UII.m_IsModded = true;
                    UII.m_LocalisedString = null;
                    toChange.m_SkinUIInfo = UII;
                    SMUtil.Log(false, KickStart.ModID + ": Made skin for " + ID + " skinIndex: " + skinIndex + " ID: " + skinID);
                    return true;
                }
                else
                {
                    toChange.m_Corporation = (FactionSubTypes)ID;
                    toChange.m_SkinUniqueID = skinID;

                    toChange.m_SkinMeshes = new SkinMeshes();
                    error++;
                    toChange.m_SkinTextureInfo = ManCustomSkins.inst.GetSkinTexture(corpID, skinIndex);
                    //Debug_SMissions.Log(KickStart.ModID + ": MirrorSkin(SMCCorpLicense) - Skin Name: " + ManCustomSkins.inst.GetSkinNameForSnapshot(corpID, (uint)skinIndex));
                    error++;

                    UIIref = ManCustomSkins.inst.GetSkinUIInfo(corpID, skinIndex);
                    error++;
                    if (CSIRenderCache.TryGetValue(skinID, out Sprite val))
                    {
                        UII.m_PreviewImage = val;
                    }
                    else
                    {
                        Debug_SMissions.Log(KickStart.ModID + ": MirrorSkin(SMCCorpLicense) - RenderTechImage needs time to boot");
                        CSIBacklogRender.Add(toChange);
                        UII.m_PreviewImage = UIIref.m_PreviewImage;
                        needsToRenderSkins = true;
                    }
                    error++;
                    UII.m_SkinButtonImage = UIIref.m_SkinButtonImage;
                    UII.m_SkinMiniPaletteImage = UIIref.m_SkinMiniPaletteImage;
                    UII.m_SkinLocked = false;
                    UII.m_IsModded = false;
                    UII.m_LocalisedString = null;
                    SMUtil.Log(false, KickStart.ModID + ": Made skin for " + ID + " skinIndex: " + skinIndex + " ID: " + skinID);
                    toChange.m_SkinUIInfo = UII;
                }
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, GetLogName("Skins"), KickStart.ModID + ": MirrorSkin(SMCCorpLicense) - error on execution!  error no " + error, e);
                return false;
            }
            return true;
        }
        internal bool SkinFromExisting(ref CorporationSkinInfo toChange, FactionSubTypes corpID, int skinIndex, out int skinID)
        {   //
            int error = 0;
            skinID = 0;
            try
            {
                error++;
                CorporationSkinUIInfo UII = new CorporationSkinUIInfo();
                error++;
                skinID = skinIndex;//ManCustomSkins.inst.SkinIndexToID((byte)skinIndex, corpID);
                toChange.m_Corporation = (FactionSubTypes)ID;
                toChange.m_SkinUniqueID = skinID;

                toChange.m_SkinMeshes = new SkinMeshes();
                error++;
                Texture2D Albedo = null;
                Texture2D Metal = null;
                Texture2D Emissive = null;
                if (BlockIndexer.StringToBlockType(FirstCabUnlocked, out BlockTypes BT))
                {
                    var CB = ManSpawn.inst.GetBlockPrefab(BT);
                    if (CB)
                    {
                        MeshRenderer MR = CB.GetComponentInChildren<MeshRenderer>();
                        if (MR)
                        {
                            Albedo = (Texture2D)MR.sharedMaterial.GetTexture("_MainTex");
                            Metal = (Texture2D)MR.sharedMaterial.GetTexture("_MetallicGlossMap");
                            Emissive = (Texture2D)MR.sharedMaterial.GetTexture("_EmissionMap");
                        }
                    }
                    else
                        SMUtil.Error(false, "Corp [Unofficial] (Skins) ~ " + Faction, "Corp " + Faction + " does not have an existing FirstCabUnlocked!");
                }
                else
                    SMUtil.Error(false, "Corp [Unofficial] (Skins) ~ " + Faction, "Corp " + Faction + " does not have an existing FirstCabUnlocked!");
                SkinTextures ST = new SkinTextures();
                ST.m_Albedo = Albedo;
                ST.m_Emissive = Emissive;
                ST.m_Metal = Metal;
                toChange.m_SkinTextureInfo = ST;
                error++;

                Sprite placeholder = Sprite.Create(Albedo, new Rect(0, 0, Albedo.width, Albedo.height), Vector2.zero);
                error++;
                if (CSIRenderCache.TryGetValue(skinID, out Sprite val))
                {
                    UII.m_PreviewImage = val;
                }
                else
                {
                    Debug_SMissions.Log(KickStart.ModID + ": SkinFromExisting(SMCCorpLicense) - RenderTechImage needs time to boot");
                    CSIBacklogRender.Add(toChange);
                    UII.m_PreviewImage = placeholder;
                    needsToRenderSkins = true;
                }
                error++;
                UII.m_SkinButtonImage = placeholder;
                UII.m_SkinMiniPaletteImage = placeholder;
                UII.m_SkinLocked = false;
                UII.m_IsModded = false;
                UII.m_LocalisedString = null;
                SMUtil.Log(false, KickStart.ModID + ": Made skin for " + Faction + " skinIndex: " + skinIndex + " ID: " + skinID);
                toChange.m_SkinUIInfo = UII;
            }
            catch (Exception e)
            {
                Debug_SMissions.Log(KickStart.ModID + ": SkinFromExisting(SMCCorpLicense) - error on execution!  error no " + error + " | " + e);
                return false;
            }
            return true;
        }


        internal Sprite GetPreviewImage(int skinIndex)
        {   //
            try
            {
                Sprite output = null;
                if (SkinRefTech)
                {
                    TechData TD = SkinRefTech.GetTechDataFormatted();
                    if (TD.m_BlockSpecs != null)
                    {
                        for (int step = 0; step < TD.m_BlockSpecs.Count; step++)
                        {
                            try
                            {
                                TankPreset.BlockSpec value = TD.m_BlockSpecs[step];
                                /*
                                if ((int)Singleton.Manager<ManSpawn>.inst.GetCorporation(value.GetBlockType()) != CL.ID)
                                {
                                    Debug_SMissions.Log(KickStart.ModID + ": MirrorSkin(SMCCorpLicense) - Example Tech must use only the same blocks from the respective corp!");
                                    continue;
                                }*/
                                value.m_SkinID = Singleton.Manager<ManCustomSkins>.inst.SkinIndexToID((byte)skinIndex, (FactionSubTypes)ID);
                                TD.m_BlockSpecs[step] = value;
                            }
                            catch
                            {
                                Debug_SMissions.Log(KickStart.ModID + ": GetPreviewImage(SMCCorpLicense) - BlockSpec IS NULL");
                            }
                        }
                    }
                    else
                    {
                        Debug_SMissions.Log(KickStart.ModID + ": GetPreviewImage(SMCCorpLicense) - BlockSpec IS NULL BY DEFAULT");
                    }
                    Singleton.Manager<ManScreenshot>.inst.RenderTechImage(TD, new IntVector2(128, 128), false, delegate (TechData techData, Texture2D tex)
                    {
                        Rect rect = new Rect(Vector2.zero, new Vector2((float)tex.width, (float)tex.height));
                        output = Sprite.Create(tex, rect, Vector2.zero);
                    });
                }
                return output;
            }
            catch (Exception e)
            {
                Debug_SMissions.Log(KickStart.ModID + ": GetPreviewImage(SMCCorpLicense) - error " + e);
                return null;
            }
        }

        internal void BuildFactionLicenseOfficial()
        {
            FactionSubTypes Lookup = (FactionSubTypes)ID;
            if (!ManLicenses.inst || ManLicenses.inst.m_ThresholdData == null)
            {
                Debug_SMissions.Log(KickStart.ModID + ": Corporation Licence for " + Faction + ", ID: " + ID + " cannot be built as ManLicenses is " + (!ManLicenses.inst ? "still loading" : "still building"));
            }
            else if (!ManLicenses.inst.m_ThresholdData.Exists(delegate (ManLicenses.ThresholdsTableEntry cand) { return cand.faction == Lookup; }))
            {
                ManLicenses.ThresholdsTableEntry TTE = new ManLicenses.ThresholdsTableEntry
                {
                    faction = Lookup,
                    thresholds = BuildThresholds(),
                };
                ManLicenses.inst.m_ThresholdData.Add(TTE);
                BuildInfoList();

                Debug_SMissions.Log(KickStart.ModID + ": Init Corporation Licence for " + Faction + ", ID: " + ID + ".\n     This cannot be changed after first load as it has a massive gameplay impact.");
            }
            else
                Debug_SMissions.Log(KickStart.ModID + ": Corporation Licence for " + Faction + ", ID: " + ID + " is already built");
        }
        internal void BuildFactionLicenseUnofficial()
        {   //
#if !STEAM
            FactionSubTypes Lookup = (FactionSubTypes)ID;
            if (!ManLicenses.inst || ManLicenses.inst.m_ThresholdData == null)
            {
                Debug_SMissions.Log(KickStart.ModID + ": Corporation Licence for " + Faction + ", ID: " + ID + " cannot be built as ManLicenses is " + (!ManLicenses.inst ? "still loading" : "still building"));
            }
            else if (!ManLicenses.inst.m_ThresholdData.Exists(delegate (ManLicenses.ThresholdsTableEntry cand) { return cand.faction == Lookup; }))
            {
                ManLicenses.ThresholdsTableEntry TTE = new ManLicenses.ThresholdsTableEntry();
                TTE.faction = Lookup;
                TTE.thresholds = BuildThresholds();
                ManLicenses.inst.m_ThresholdData.Insert(0, TTE);
                BuildInfoList();

                Debug_SMissions.Log(KickStart.ModID + ": Init Corporation Licence for " + Faction + ", ID: " + ID + ".\n     This cannot be changed after first load as it has a massive gameplay impact.");
            }
            else
                Debug_SMissions.Log(KickStart.ModID + ": Corporation Licence for " + Faction + ", ID: " + ID + " is already built");
#endif
        }
        internal FactionLicense.Thresholds BuildThresholds()
        {   //
            FactionLicense.Thresholds TH = new FactionLicense.Thresholds
            {
                m_MaxSupportedLevel = GradesXP.Length,
                m_XPLevels = GradesXP
            };
            return TH;
        }
        internal FactionLicense.Progress BuildProgress()
        {   //
            FactionLicense.Progress prog = new FactionLicense.Progress
            {
                m_CurrentLevel = 0,
                m_CurrentXP = 0,
                m_Discovered = false,
            };
            return prog;
        }

        // Blocks
        /// <summary>
        /// SETS THE TEXTURES
        /// </summary>
        internal void PushBlockTypesToAssignedCorp()
        {   // Pushes the textures
            FactionSubTypes FST = (FactionSubTypes)ID;
            Texture texToLookFor = null;
            if (SkinReferenceFaction == FactionSubTypes.NULL)
            {
                if (BlockIndexer.StringToBlockType(FirstCabUnlocked, out BlockTypes BT))
                {
                    var CB = ManSpawn.inst.GetBlockPrefab(BT);
                    if (CB)
                    {
                        MeshRenderer MR = CB.GetComponentInChildren<MeshRenderer>();
                        if (MR)
                        {
                            texToLookFor = MR.sharedMaterial.GetTexture("_MainTex");
                        }
                    }
                    else
                        SMUtil.Error(false, GetLogName("Setup - Blocks"), "Corp " + Faction + " does not have an existing FirstCabUnlocked!");
                }
                else
                    SMUtil.Error(false, GetLogName("Setup - Blocks"), "Corp " + Faction + " does not have an existing FirstCabUnlocked!");
            }
            if (ManTechMaterialSwap.inst.m_FinalCorpMaterials.TryGetValue(ID, out Material val))
            {
                foreach (SMCCorpBlockRange CBR in GradeUnlockBlocks)
                {
                    List<BlockTypes> blocs = CBR.BlocksAvail;
                    foreach (BlockTypes bloc in blocs)
                    {
                        TankBlock TB = ManSpawn.inst.GetBlockPrefab(bloc);

                        foreach (Renderer rend in TB.GetComponentsInChildren<Renderer>())
                        {
                            if (SkinReferenceFaction == FactionSubTypes.NULL)
                            {
                                try
                                {
                                    if (rend.sharedMaterial)
                                        if (texToLookFor == rend.sharedMaterial.GetTexture("_MainTex"))
                                            rend.sharedMaterial = val;
                                }
                                catch { };
                            }
                            else if (ManTechMaterialSwap.inst.m_FinalCorpMaterials.ContainsValue(rend.sharedMaterial))
                                rend.sharedMaterial = val;
                        }
                        //TB.GetComponent<MaterialSwapper>().SetupMaterial(null, FST);
                    }
                }
                Debug_SMissions.Log(KickStart.ModID + ": Pushed all blocks textures to corp " + ID);
            }
            else
                Debug_SMissions.Log(KickStart.ModID + ": ManTechMaterialSwap - Failed. Could not push all blocks textures to corp " + ID);
            foreach (KeyValuePair<int, Material> CBR in ManTechMaterialSwap.inst.m_FinalCorpMaterials)
            {
                Debug_SMissions.Log(KickStart.ModID + ": ManTechMaterialSwap - " + CBR.Key + " | " + CBR.Value );
            }
        }
        private void UnofficialTryBuildBlockTypes()
        {   //
#if !STEAM
            if (KickStart.isBlockInjectorPresent)
            {
                BlockTypes BT;
                int smallestCabVolume = 262145;
                foreach (KeyValuePair<int, CustomBlock> pair in BlockLoader.CustomBlocks)
                {
                    CustomBlock CB = pair.Value;
                    if (CB.Name.StartsWith(GetCorpNameForBlocks()))
                    {
                        int CBGrade = CB.Grade;
                        if (CB.Grade < 0)
                            CBGrade = 0;
                        if (CB.Grade > 9)
                        {
                            CBGrade = 9;
                        }
                        for (int step = 0; step <= CBGrade; step++)
                        {
                            if (GradeUnlockBlocks.Count <= step)
                            {
                                GradeUnlockBlocks.Add(new SMCCorpBlockRange());
                            }
                        }
                        BT = (BlockTypes)CB.BlockID;
                        if (!GradeUnlockBlocks[CBGrade].BlocksOutOfRange.Contains(BT))
                            GradeUnlockBlocks[CBGrade].BlocksOutOfRange.Add(BT);
                        GameObject GO = CB.Prefab;
                        if (GO)
                        {
                            int cabVolume = GO.GetComponent<TankBlock>().filledCells.Count();
                            var techControl = GO.GetComponent<ModuleTechController>();
                            if (techControl && smallestCabVolume > cabVolume)
                            {
                                if (techControl.HandlesPlayerInput)
                                {
                                    FirstCabUnlocked = CB.Prefab.name;
                                    smallestCabVolume = cabVolume;
                                }
                            }
                        }
                    }
                }

                // Tidy up!
                for (int step = 0; step < GradeUnlockBlocks.Count; step++)
                {
                    GradeUnlockBlocks[step].BlocksOutOfRange = GradeUnlockBlocks[step].BlocksOutOfRange.OrderBy(x => x).ToList();
                }
            }
#endif
        }
        private void OfficialTryBuildBlockTypes(FactionSubTypes FST)
        {   //
            ModdedCorpDefinition MCD = ManMods.inst.FindCorp(ID);
            if (MCD)
            {
                
                if (ProgressionPatches.OfficialBlocksPool.TryGetValue(FST, out List<SMCCorpBlockRange> range))
                {
                    GradeUnlockBlocks = range;
                    Debug_SMissions.Log(KickStart.ModID + ": SMCCorpLicense(OfficialMods) - GradeUnlockBlocks for " + MCD.m_DisplayName + " has " + range.Count + " grades");
                }
                else
                    Debug_SMissions.Log(KickStart.ModID + ": SMCCorpLicense(OfficialMods) - GradeUnlockBlocks not present for Corp " + MCD.m_DisplayName);

                ModSessionInfo MSI = (ModSessionInfo)sess.GetValue(ManMods.inst);
                if (GradeUnlockBlocks.Count == 0)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": SMCCorpLicense - Corp exists but it's respective mod HAS NO BLOCKS!");
                    return;
                }
                int smallestCabVolume = 262145;
                foreach (var item in GradeUnlockBlocks)
                {
                    foreach (BlockTypes BT in item.BlocksAvail)
                    {
                        TankBlock TB = ManSpawn.inst.GetBlockPrefab(BT);
                        if (TB)
                        {
                            int cabVolume = TB.filledCells.Length;
                            var techControl = TB.GetComponent<ModuleTechController>();
                            if (techControl && smallestCabVolume > cabVolume)
                            {
                                if (techControl.HandlesPlayerInput)
                                {
                                    FirstCabUnlocked = ManSpawn.inst.GetBlockPrefab(BT).name;
                                    smallestCabVolume = cabVolume;
                                }
                            }
                        }
                    }
                }
                if (FirstCabUnlocked == "GSOCockpit_111")
                {
                    Debug_SMissions.Log(KickStart.ModID + ": SMCCorpLicense - Corp has no valid cab block.  Defaulting to the first block declared...");
                    FirstCabUnlocked = MSI.BlockIDs.First().Value;
                }

                Sprite shared = Sprite.Create(MCD.m_Icon, new Rect(0, 0, 64, 64), Vector2.zero);
                SmallCorpIcon = shared;
                SmallSelectedCorpIcon = shared;
                HighResCorpIcon = shared;
            }
        }

        private void BuildInfoList()
        {   //
            if (!GetDirectory(out string directory))
                return;
            try
            {
                if (SMissionJSONLoader.DirectoryExists(Path.Combine(directory, "AutoBlockList.txt")))
                    return;
            }
            catch (ArgumentException e)
            {
                throw new Exception("Invalid path: " + directory, e);
            }
            Debug_SMissions.Log("Building AutoBlockList for faction " + Faction);
            StringBuilder SB = new StringBuilder();
            int tier = 0;
            int categoriesCount = Enum.GetValues(typeof(BlockCategories)).Length;
            foreach (SMCCorpBlockRange CBR in GradeUnlockBlocks)
            {
                SB.Append("Grade " + tier + " Blocks: (Count = " + CBR.BlocksAvail.Count + ")\n");
                for (int step = 0; step < categoriesCount; step++)
                {
                    BlockCategories BC = (BlockCategories)step;
                    if (step == 0)
                        SB.Append("  Category - Unlisted:\n");
                    else
                        SB.Append("  Category - " + BC + ":\n");
                    foreach (BlockTypes BT in CBR.BlocksAvail)
                    {
                        if (ManSpawn.inst.GetCategory(BT) == BC)
                        {
                            TankBlock TB = ManSpawn.inst.GetBlockPrefab(BT);
#if !STEAM
                            if (BlockLoader.CustomBlocks.TryGetValue((int)BT, out CustomBlock CB))
                                SB.Append("    " + (int)BT + ", BlockName: " + TB.name + " | Ingame name: " + CB.Name + "\n");
                            else
#endif
                                SB.Append("    " + (int)BT + ", BlockName: " + TB.name + " | Ingame name: same as BlockName?!\n");
                        }
                    }
                }
                tier++;
            }
            SaveToDiskAutoBlockList(SB.ToString());
        }


        public BlockUnlockTable.CorpBlockData GetCorpBlockData(out int numberEntries)
        {   //
            numberEntries = 0;
            if (corpBlockData == null)
            {
                if (OfficialCorp)
                {
                    corpBlockData = Singleton.Manager<ManLicenses>.inst.GetBlockUnlockTable().GetCorpBlockData((int)SubMissionTree.GetTreeCorp(Faction));
                    Debug_SMissions.Log(KickStart.ModID + ": GetCorpBlockData(SMCCorpLicense) - corpBlockData was fetched for " + Faction + ".");
                }
                else
                {
                    List<BlockUnlockTable.GradeData> GDl = new List<BlockUnlockTable.GradeData>();
                    foreach (SMCCorpBlockRange CBR in GradeUnlockBlocks)
                    {
                        List<BlockUnlockTable.UnlockData> UDl = new List<BlockUnlockTable.UnlockData>();
                        BlockUnlockTable.UnlockData UD;
                        foreach (BlockTypes BT in CBR.BlocksAvail)
                        {
                            int hash = ItemTypeInfo.GetHashCode(ObjectTypes.Block, (int)BT);
                            bool shouldNotShow = (BlockCategories)ManSpawn.inst.VisibleTypeInfo.GetDescriptorFlags<BlockCategories>(hash) == BlockCategories.Standard;
                            UD = new BlockUnlockTable.UnlockData
                            {
                                m_BlockType = BT,
                                m_BasicBlock = true,
                                m_DontRewardOnLevelUp = false,
                                m_HideOnLevelUpScreen = shouldNotShow,
                            };
                            numberEntries++;
                            UDl.Add(UD);
                        }
                        BlockUnlockTable.GradeData GD = new BlockUnlockTable.GradeData
                        {
                            m_BlockList = UDl.ToArray(),
                            m_AdditionalUnlocks = new BlockTypes[0] { },
                        };
                        GDl.Add(GD);
                    }

                    BlockUnlockTable.CorpBlockData CBD = new BlockUnlockTable.CorpBlockData { m_GradeList = GDl.ToArray(), };
                    corpBlockData = CBD;
                    Debug_SMissions.Log(KickStart.ModID + ": GetCorpBlockData(SMCCorpLicense) - corpBlockData was built for " + Faction + ".");
                }
            }
            return corpBlockData;
        }

        // Skins
        private void PushExternalCorpSkins()
        {
            try
            {
                ManSMCCorps.SkinLoader(this);
                if (importedSkins.Count == 0)
                {
                    Debug_SMissions.Log(KickStart.ModID + ": There were no Skin.json skins for Custom Corp " + Faction);
                    return;
                }
                Debug_SMissions.Log(KickStart.ModID + ": Imported " + importedSkins.Count + " Skin.json skins for Custom Corp " + Faction);
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, GetLogName("Skins"), KickStart.ModID + ": Check your Skins file names!  Some may be invalid!", e);
            }
        }
        private static FieldInfo FIFetch(Type type, string name)
        {
            return type.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        }
        private static MethodInfo MIFetch(Type type, string name)
        {
            return type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private static readonly FieldInfo
            cratePool = FIFetch(typeof(ManSpawn), "m_CorpCrateRuntimePrefabsDict"),
            crateStuff = FIFetch(typeof(ManSpawn), "m_StrippedCrateTypes");
        private static readonly MethodInfo
            prefabss = MIFetch(typeof(ManSpawn), "SetupNetworkedPrefab");

        internal void ForceCrateIn(FactionSubTypes FST, Crate great)
        {
            Type pair = typeof(ManSpawn).GetNestedType("PrefabPair", BindingFlags.NonPublic | BindingFlags.Instance);
            Type dictT = typeof(Dictionary<,>).MakeGenericType(new Type[2] { typeof(FactionSubTypes), pair });
            MethodInfo adder = dictT.GetMethod("Add");
            MethodInfo check = dictT.GetMethod("TryGetValue");
            object dict = cratePool.GetValue(ManSpawn.inst);
            object stuff = crateStuff.GetValue(ManSpawn.inst);
            if ((bool)check.Invoke(dict, new object[2] { FST, null }))
            {
                object newCrateInst = prefabss.Invoke(ManSpawn.inst, new object[2] { great, stuff });
                adder.Invoke(dict, new object[2] { FST, newCrateInst });
            }
            //object newCrateInst = Activator.CreateInstance(pair)
        }
        // Crates
        /// <summary>
        /// Must be executed before the crates in ManSpawn are built
        /// </summary>
        internal void TryBuildCrate()
        {   //
            try
            {
                bool validFactionRef = CrateReferenceFaction > FactionSubTypes.NULL && (int)CrateReferenceFaction < Enum.GetNames(typeof(FactionSubTypes)).Length;
                if (GetDirectory(out string directory))
                {
                    if (validFactionRef)
                    {
                        try
                        {
                            Dictionary<FactionSubTypes, Crate> refs = (Dictionary<FactionSubTypes, Crate>)crateS.GetValue(ManSpawn.inst);
                            if (refs.TryGetValue(CrateReferenceFaction, out Crate refCrate))
                            {
                                GameObject GO = UnityEngine.Object.Instantiate(refCrate.gameObject, null);
                                GO.SetActive(false);
                                Crate newCrate = GO.GetComponent<Crate>();
                                string crateName = "m_" + CrateReferenceFaction.ToString() + "_";
                                Mesh mesh;
                                Transform trans;
                                mesh = SMissionJSONLoader.LoadMeshFromFile(Path.Combine(directory, "Crate_Base.obj"));
                                if (mesh != null)
                                {
                                    trans = GO.transform.Find(crateName + "Crate_Base");
                                    trans.GetComponent<MeshFilter>().sharedMesh = mesh;
                                }
                                else
                                {
                                    Debug_SMissions.Log("Using defaults for " + Faction + " crate which is " + CrateReferenceFaction);
                                    HasCratePrefab = false;
                                    return;
                                }
                                BuildCratePart(crateName, GO, "Crate_A");
                                BuildCratePart(crateName, GO, "Crate_B");

                                FactionSubTypes FST = SubMissionTree.GetTreeCorp(Faction);
                                GO.SetActive(false);
                                ForceCrateIn(FST, newCrate);
                            }
                            crateS.SetValue(ManSpawn.inst, refs);
                            HasCratePrefab = true;
                            return;
                        }
                        catch (Exception e)
                        {
                            Debug_SMissions.Log("Failiure on crate addition for corp " + Faction + " | The crate has missing models: Make sure you have a: ");
                            Debug_SMissions.Log("\"Crate_Base.obj\", \"Crate_A.obj\", \"Crate_B.obj\", \"Crate_A_Lock.obj\", \"Crate_B_Lock.obj\", \"Crate_B_LightRed.obj\", \"Crate_B_LightGreen.obj\"");
                            Debug_SMissions.Log(e);
                        }
                        HasCratePrefab = false;
                        return;
                    }
                }
                if (validFactionRef)
                {
                    Debug_SMissions.Log("Using defaults for " + Faction + " crate which is " + CrateReferenceFaction);
                }
                else
                {
                    Debug_SMissions.Log("Just using defaults for " + Faction + " crate which is GSO");
                    CrateReferenceFaction = FactionSubTypes.GSO;
                }
                HasCratePrefab = false;
                return;
            }
            catch (Exception e)
            {
                Debug_SMissions.Log("Failiure on crate addition for corp " + Faction + " | " + e);
                HasCratePrefab = false;
                return;
            }
        }

        internal void BuildCratePart(string crateName, GameObject GO, string partName)
        {   //
            try
            {
                if (GetDirectory(out string directory))
                {
                    Mesh mesh = SMissionJSONLoader.LoadMeshFromFile(Path.Combine(directory, partName + ".obj"));
                    if (mesh != null)
                    {
                        Transform trans = GO.transform.Find(crateName + partName);
                        trans.GetComponent<MeshFilter>().sharedMesh = mesh;
                    }
                }
            }
            catch (Exception e)
            {
                Debug_SMissions.Log("Failiure on crate addition for corp " + Faction + " | " + e);
                return;
            }
        }

        // Fallback
        private void MakeFallbackTechIfNeeded()
        {   //
            if (!KickStart.FullyLoadedGame)
                return;
            BlockTypes BTcab = BlockTypes.GSOCockpit_111;
            if (FirstCabUnlocked.NullOrEmpty())
            {
                Debug_SMissions.Log(KickStart.ModID + ": THERE IS NO ASSIGNED CAB WITHIN THIS CORP!!!");
                FirstCabUnlocked = ManSpawn.inst.GetBlockPrefab(BlockTypes.GSOCockpit_111).name;
            }
            else
                BlockIndexer.GetBlockIDLogFree(FirstCabUnlocked, out BTcab);
            if (BTcab == BlockTypes.GSOCockpit_111)
            {
                Debug_SMissions.Log(KickStart.ModID + ": Cannot make fallback tech.  No block specified for cab.");
                return;
            }
            Debug_SMissions.Log(KickStart.ModID + ": Making fallback Tech...");
            string blockName = ManSpawn.inst.GetBlockPrefab(BTcab).name;
            TechData TD = new TechData();
            TD.Name = "FALLBACK";
            TD.m_BlockSpecs = new List<TankPreset.BlockSpec>();
            TD.m_TechSaveState = new Dictionary<int, TechComponent.SerialData>();
            TD.m_CreationData = new TechData.CreationData();
            TD.m_SkinMapping = new Dictionary<uint, string>();

            BlockTypes BT = BTcab;
            TD.m_BlockSpecs.Add(
                    new TankPreset.BlockSpec
                    {
                        m_BlockType = BT,
                        m_SkinID = 0,
                        m_VisibleID = 0,
                        block = blockName,
                        position = IntVector3.zero,
                        orthoRotation = new OrthoRotation(Quaternion.LookRotation(Vector3.forward)),
                        saveState = new Dictionary<int, Module.SerialData>(),
                        textSerialData = new List<string>(),
                    }
                );
            SkinRefTech = TankPreset.CreateInstance();
            techData.SetValue(SkinRefTech, TD);
            Debug_SMissions.Log(KickStart.ModID + ": Made Skinref Tech (FALLBACK) for ID " + ID);
        }


        // FileSys
        public static FieldInfo techData = typeof(TankPreset)
                   .GetField("m_TechData", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        private string directoryCache = null;
        internal bool GetDirectory(out string directory)
        {
            if (OfficialCorp)
            {
                if (directoryCache == null)
                {
                    if (SMissionJSONLoader.TryGetDirectoryOfCorp(Faction, out string directoryEnd))
                    {
                        directoryCache = directoryEnd;
                    }
                    else
                    {
                        directory = null;
                        return false;
                    }
                }
                directory = directoryCache;
                return true;
            }
            directory = Path.Combine(SMissionJSONLoader.MissionCorpsDirectory, Faction);
            return true;
        }
        public void TryFindTextures()
        {   //
            try
            {
                if (GetDirectory(out string directory))
                {
                    if (SmallCorpIcon == null && !StandardCorpIcon.NullOrEmpty())
                    {
                        Texture tex = FileUtils.LoadTexture(Path.Combine(directory,StandardCorpIcon));
                        SmallCorpIcon = Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                    }
                    if (SmallSelectedCorpIcon == null && !SelectedCorpIcon.NullOrEmpty())
                    {
                        Texture tex = FileUtils.LoadTexture(Path.Combine(directory, SelectedCorpIcon));
                        SmallSelectedCorpIcon = Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                    }
                    if (HighResCorpIcon == null && !HighResolutionCorpIcon.NullOrEmpty())
                    {
                        Texture tex = FileUtils.LoadTexture(Path.Combine(directory, HighResolutionCorpIcon));
                        HighResCorpIcon = Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                    }
                    if (SkinRefTech == null && !ExampleSkinTech.NullOrEmpty())
                    {
                        Texture tex = FileUtils.LoadTexture(Path.Combine(directory, ExampleSkinTech));
                        if (ManScreenshot.TryDecodeSnapshotRender((Texture2D)tex, out TechData.SerializedSnapshotData data))
                        {
                            TankPreset TP = TankPreset.CreateInstance();
                            techData.SetValue(TP, data.CreateTechData());
                            SkinRefTech = TP;
                        }
                    }
                }
            }
            catch (Exception e)
            { Debug_SMissions.Log(e); }
        }


        private SMCCorpLicenseJSON ConvertToJSON()
        {
            return new SMCCorpLicenseJSON(this);
        }
        private void SaveToDiskAutoBlockList(string toSave)
        {
            try
            {
                SMissionJSONLoader.ValidateDirectory(SMissionJSONLoader.MissionCorpsDirectory);
                if (GetDirectory(out string directory))
                {
                    SMissionJSONLoader.ValidateDirectory(directory);
                    SMissionJSONLoader.TryWriteToTextFile(Path.Combine(directory, "AutoBlockList"), toSave);
                }
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, GetLogName("Export"), KickStart.ModID + ": Could not edit BlockList.txt.  " +
                    "\n   This could be due to a bug with this mod or file permissions.", e);
            }
        }
        private void SaveToDisk()
        {
            try
            {
                SMissionJSONLoader.ValidateDirectory(SMissionJSONLoader.MissionCorpsDirectory);
                if (GetDirectory(out string directory))
                {
                    SMissionJSONLoader.ValidateDirectory(directory);
                    string toSave = JsonConvert.SerializeObject(ConvertToJSON(), Formatting.Indented);
                    SMissionJSONLoader.TryWriteToJSONFile(Path.Combine(directory, "MissionCorp"), toSave);
                }
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, GetLogName("Export"), KickStart.ModID + ": Could not edit MissionCorp.json.  " +
                    "\n   This could be due to a bug with this mod or file permissions.", e);
            }
        }
        public static void SaveTemplateToDisk()
        {
            try
            {
                SMCCorpLicense CL = new SMCCorpLicense("ExampleCorp", 1000000, new int[3] { 100, 450, 9001 }, true);
                CL.SaveToDisk();
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "Corps (Export) ~ ExampleCorp", KickStart.ModID + ": Could not edit MissionCorp.json.  " +
                    "\n   This could be due to a bug with this mod or file permissions.", e);
            }
        }


    }
    
    [Serializable]
    public class SMCCorpLicenseJSON
    {
        //
        public string FullName = "NuLl";    // The FULL Name
        public string Faction = null;     // The SHORTENED name
        public bool BlocksUseFullName = false;  // Should we check for the fullname instead?
        public string Lore = "A corporation so mysterious, we have no intel on it!";
        public Dictionary<string, string> MoreLore = new Dictionary<string, string>();
        //public FactionSubTypes EngineReferenceFaction = FactionSubTypes.GSO;    // The Faction to reference for the engine. Due to FMOD this cannot be customized.
        //public FactionSubTypes CombatMusicFaction = FactionSubTypes.GSO;    // The Faction to reference for the combat music. Due to FMOD this cannot be customized.
        public FactionSubTypes SkinReferenceFaction = FactionSubTypes.NULL; // The Faction to reference for skins. Leave "NULL" to use only your own.
        public FactionSubTypes CrateReferenceFaction = FactionSubTypes.NULL;// The Faction to reference for Delivery crates. You can override the models with your own.
        public int ID = -3;                 // MUST be set to a unique value above 50000
        public float minEmissive = 0.25f;

        public int[] XPLevels;
        public int[] GradesXP;

        public string FirstCabUnlocked = "GSOCockpit_111";
        public List<SMCCorpBlockRange> GradeUnlockBlocks = new List<SMCCorpBlockRange>();

        // Used for combat
        public List<string> BattleMusic = new List<string>();

        // Used for Inventory
        public string StandardCorpIcon = null;
        public string SelectedCorpIcon = null;
        public string HighResolutionCorpIcon = null;

        public string ExampleSkinTech = null;

        public SMCCorpLicenseJSON()
        {
        }

        public SMCCorpLicenseJSON(SMCCorpLicense toAddTo)
        {
            //EngineReferenceFaction = toAddTo.EngineReferenceFaction;
            ExampleSkinTech = toAddTo.ExampleSkinTech;
            minEmissive = toAddTo.minEmissive;
            SelectedCorpIcon = toAddTo.SelectedCorpIcon;
            SkinReferenceFaction = toAddTo.SkinReferenceFaction;
            StandardCorpIcon = toAddTo.StandardCorpIcon;
            HighResolutionCorpIcon = toAddTo.HighResolutionCorpIcon;
            BattleMusic = toAddTo.BattleMusic;
            BlocksUseFullName = toAddTo.BlocksUseFullName;
            GradeUnlockBlocks = toAddTo.GradeUnlockBlocks;
            //CombatMusicFaction = toAddTo.CombatMusicFaction;
            CrateReferenceFaction = toAddTo.CrateReferenceFaction;
            FirstCabUnlocked = toAddTo.FirstCabUnlocked;
            Faction = toAddTo.Faction;
            FullName = toAddTo.FullName;
            GradesXP = toAddTo.GradesXP;
            ID = toAddTo.ID;
        }
        public SMCCorpLicense Apply(SMCCorpLicense toAddTo)
        {
            //toAddTo.EngineReferenceFaction = EngineReferenceFaction;
            toAddTo.ExampleSkinTech = ExampleSkinTech;
            toAddTo.minEmissive = minEmissive;
            toAddTo.SelectedCorpIcon = SelectedCorpIcon;
            toAddTo.SkinReferenceFaction = SkinReferenceFaction;
            toAddTo.StandardCorpIcon = StandardCorpIcon;
            toAddTo.HighResolutionCorpIcon = HighResolutionCorpIcon;
            toAddTo.BattleMusic = BattleMusic;
            toAddTo.BlocksUseFullName = BlocksUseFullName;
            toAddTo.GradeUnlockBlocks = GradeUnlockBlocks;
            //toAddTo.CombatMusicFaction = CombatMusicFaction;
            toAddTo.CrateReferenceFaction = CrateReferenceFaction;
            toAddTo.FirstCabUnlocked = FirstCabUnlocked;
            toAddTo.Faction = Faction;
            toAddTo.FullName = FullName;
            if (XPLevels != null)
                toAddTo.GradesXP = XPLevels;
            else
                toAddTo.GradesXP = GradesXP;
            if (MoreLore != null)
                toAddTo.MoreLore = MoreLore;
            if (Lore != null)
                toAddTo.Lore = Lore;
            toAddTo.ID = ID;
            return toAddTo;
        }
        public SMCCorpLicense ConvertToActive()
        {
            return Apply(new SMCCorpLicense());
        }
    }
}
