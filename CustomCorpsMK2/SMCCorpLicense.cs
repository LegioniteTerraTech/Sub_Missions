using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Nuterra.BlockInjector;
using Newtonsoft.Json;

namespace Sub_Missions
{
    // The extended version of a Custom Corp License to fill in further details like grade blocks and whatnot.
    [Serializable]
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
        public string Faction = "NULL";     // The SHORTENED name
        public FactionSubTypes EngineReferenceFaction = FactionSubTypes.GSO;    // The Faction to reference for the engine. Due to FMOD this cannot be customized.
        public FactionSubTypes CombatMusicFaction = FactionSubTypes.GSO;    // The Faction to reference for the combat music. Due to FMOD this cannot be customized.
        public FactionSubTypes SkinReferenceFaction = FactionSubTypes.NULL; // The Faction to reference for skins. Leave "NULL" to use only your own.
        public FactionSubTypes CrateReferenceFaction = FactionSubTypes.NULL;// The Faction to reference for Delivery crates. You can override the models with your own.
        public int ID = -3;                 // MUST be set to a unique value above 50000
        public float minEmissive = 0.25f;

        public int[] GradesXP;

        public BlockTypes FirstCabUnlocked = BlockTypes.GSOCockpit_111;
        public List<SMCCorpBlockRange> GradeUnlockBlocks = new List<SMCCorpBlockRange>();
        public List<Texture> TexturesCache = new List<Texture>();



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


        [JsonIgnore]
        public Dictionary<int, Sprite> CSIRenderCache = new Dictionary<int, Sprite>();

        [JsonIgnore]
        public static bool needsToRenderSkins = true;
        [JsonIgnore]
        public static List<CorporationSkinInfo> CSIBacklogRender = new List<CorporationSkinInfo>();
        [JsonIgnore]
        public static int countWorked = 0;

        public static void TryReRenderCSIBacklog()
        {   //
            try
            {
                if (ManWorld.inst.CheckIsTileAtPositionLoaded(Vector3.zero))
                {
                    if (countWorked % 2 > 0)
                    {
                        countWorked++;
                        return;
                    }
                    if (CSIBacklogRender.Count < countWorked / 2)
                    {
                        CSIBacklogRender.Clear();
                        countWorked = 0;
                        return;
                    }
                    CorporationSkinInfo CSI = CSIBacklogRender[countWorked / 2];
                    if (ManSMCCorps.TryGetSMCCorpLicense((int)CSI.m_Corporation, out SMCCorpLicense CL))
                    {
                        if (CL.SkinRefTech == null)
                            CL.MakeFallbackTechIfNeeded();
                        if (CL.SkinRefTech == null)
                        {
                            Debug.Log("SubMissions: SkinRefTech IS NULL");
                            countWorked++;
                            return;
                        }
                        TechData TD = CL.SkinRefTech.GetTechDataFormatted();
                        if (TD == null)
                        {
                            Debug.Log("SubMissions: SkinRefTech's TechData IS NULL");
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
                                            Debug.Log("SubMissions: MirrorSkin(SMCCorpLicense) - Example Tech must use only the same blocks from the respective corp!");
                                            continue;
                                        }*/
                                        value.m_SkinID = (byte)CSI.m_SkinUniqueID;
                                        TD.m_BlockSpecs[step] = value;
                                    }
                                    catch
                                    {
                                        Debug.Log("SubMissions: MirrorSkin(SMCCorpLicense) - BlockSpec IS NULL");
                                    }
                                }
                            }
                            else
                            {
                                Debug.Log("SubMissions: MirrorSkin(SMCCorpLicense) - BlockSpec IS NULL BY DEFAULT");
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
                    if ((CSIBacklogRender.Count * 2) - 2 < countWorked)
                    {
                        Debug.Log("SubMissions: MirrorSkin(SMCCorpLicense) - Rendered all");
                        needsToRenderSkins = false;
                        CSIBacklogRender.Clear();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("SubMissions: MirrorSkin(SMCCorpLicense) - CRITICAL ERROR " + e);
                needsToRenderSkins = false;
                CSIBacklogRender.Clear();
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
        public SMCCorpLicense(FactionSubTypes ID)
        {
            FullName = ManMods.inst.FindCorpName(ID);
            Faction = ManMods.inst.FindCorpShortName(ID);
            this.ID = (int)ID;
            GradesXP = new int[3] { 100, 250, 1000 };
            Debug.Log("SubMissions: SMCCorpLicense(OfficialMods) - Init");
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
            Debug.Log("SubMissions: SMCCorpLicense(Scratch) - Init");
            if (!overrideStartup)
                TryInitFactionEXPSys();
        }

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
            Debug.Log("SubMissions: SMCCorpLicense(CustomCorp) - Init");
            TryInitFactionEXPSys();
        }


        public BlockTypes[] GetRandomBlocks(int grade, int amount)
        {   //
            try
            {
                if (GradeUnlockBlocks.Count == 0)
                    return new BlockTypes[1] { FirstCabUnlocked };
                int maxGradeVal = grade > GradeUnlockBlocks.Count - 1 ? GradeUnlockBlocks.Count - 1 : grade;
                List<BlockTypes> boundle = new List<BlockTypes>();
                for (int step = 0; maxGradeVal > step; step++)
                    boundle.AddRange(GradeUnlockBlocks[step].BlocksAvail);
                List<BlockTypes> boundle2 = new List<BlockTypes>();
                for (int step2 = 0; amount > step2; step2++)
                    boundle2.Add(boundle.GetRandomEntry());
                return boundle2.ToArray();
            }
            catch (Exception e)
            {
                Debug.Log("SubMissions: GetRandomBlocks(SMCCorpLicense) - BLOCK could not be obtained - " + e);
            }
            return new BlockTypes[1] { BlockTypes.GSOCockpit_111 };
        }
        public BlockTypes GetRandomBlock(int grade)
        {   //
            try
            {
                if (GradeUnlockBlocks.Count == 0)
                    return FirstCabUnlocked;
                if (grade > GradeUnlockBlocks.Count - 1)
                {
                    return GradeUnlockBlocks.Last().GetRandomBlock();
                }
                return GradeUnlockBlocks[grade].GetRandomBlock();
            }
            catch (Exception e)
            {
                Debug.Log("SubMissions: GetRandomBlock(SMCCorpLicense) - BLOCK could not be obtained - " + e);
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
                Debug.Log("SubMissions: GetGradeUnlockBlocks(SMCCorpLicense) - BLOCK could not be obtained - " + e);
            }
            return new BlockTypes[3] { FirstCabUnlocked, FirstCabUnlocked, FirstCabUnlocked };
        }

        [JsonIgnore]
        AudioClip AC;
        public void RegisterCorpMusics()
        {   //
            try
            {
#pragma warning disable CS0618 // Type or member is obsolete
                WWW inst = WWW.LoadFromCacheOrDownload("file://" + GetDirectory() + SMissionJSONLoader.up + "CombatMusic.wav", 1);
                AC = inst.GetAudioClip(false, false, audioType: AudioType.WAV);
                if (!AC.isReadyToPlay)
                    Debug.Log("SubMissions: RegisterCorpMusics - The corp music did not load");
#pragma warning restore CS0618 // Type or member is obsolete

            }
            catch (Exception e)
            {
                Debug.Log("SubMissions: RegisterCorpMusics - Error " + e);
            }
        }
        public void TryInitFactionEXPSys()
        {   //
            if ((int)CombatMusicFaction > Enum.GetNames(typeof(FactionSubTypes)).Length - 1 || (int)CombatMusicFaction < -1)
                CombatMusicFaction = FactionSubTypes.GSO;
            int errorLevel = 0;
            try
            {
                FactionSubTypes corpID = (FactionSubTypes)ID;
                errorLevel++;
                if ((int)corpID > ManSMCCorps.UCorpRange) // Unoffical mods
                {   // We enable support for EVERYTHING in the tree for the corp!
                    errorLevel++;
                    BuildFactionLicenseUnofficial();
                    errorLevel += 12;
                }
                else if (ManMods.inst.IsModdedCorp(corpID)) // Official mods
                {   // Official Support is mostly a dev matter

                }
                errorLevel++;
                // It's vanilla or null - take no action.
                RefreshCorpUISP();
            }
            catch (Exception e)
            {
                Debug.Log("SubMissions: TryInitFactionEXPSys - Error level " + errorLevel + " - " + e);
            }
        }
        public void RefreshCorpUISP()
        {   //
            try
            {
                FactionSubTypes corpID = (FactionSubTypes)ID;
                if ((int)corpID > ManSMCCorps.UCorpRange) // Unoffical mods
                {   // We enable support for EVERYTHING in the tree for the corp!

                    if (GradeUnlockBlocks.Count == 0)
                    {
                        Debug.Log("SubMissions: REBUILDING GradeUnlockBlocks for " + Faction);
                        UnofficialTryBuildBlockTypes();
                        SaveToDisk();
                    }
                    if (SkinRefTech == null)
                    {
                        if (!ManSpawn.inst)
                        {
                            Debug.Log("SubMissions: ManSpawn is still loading");
                            return;
                        }
                        else
                        {
                            MakeFallbackTechIfNeeded();
                        }
                    }
                    if (!ManPurchases.inst)
                    {
                        Debug.Log("SubMissions: ManPurchases is still loading");
                        return;
                    }
                    else if (!ManPurchases.inst.AvailableCorporations.Contains(corpID))
                    {
                        Debug.Log("SubMissions: Making Corp Identifier for " + Faction + ", ID " + (int)corpID);
                        //UICCorpLicenses.MakeFactionLicenseUnofficialUI(this);
                        ManPurchases.inst.AddCustomCorp((int)corpID);

                        ManCustomSkins.inst.AddCorp((int)corpID);

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
                                    Debug.Log("SubMissions: Could not make a Corp Skin for ID " + (int)corpID + " skinIndex " + step);
                                    continue;
                                }
                                Debug.Log("SubMissions: Making a Corp Skin for ID " + (int)corpID + " skinIndex " + step);
                                CorporationSkinInfo CSI = ScriptableObject.CreateInstance<CorporationSkinInfo>();
                                if (MirrorSkin(ref CSI, SkinReferenceFaction, step, out int ID))
                                {
                                    ManCustomSkins.inst.AddSkinToCorp(CSI, true);// we keep this true so that it can self-purge to prevent nasty corruptions with Official Modding
                                    registeredSkins.Add(ID);
                                }
                            }
                        }
                        else
                        {
                        }
                        PushExternalCorpSkins();
                        //if (GradeUnlockBlocks.Count > 0 && ManCustomSkins.inst.GetNumSkinsInCorp((FactionSubTypes)ID) > 0)
                        //    PushBlockTypesToAssignedCorp();
                    }
                }
                else
                {   // Official Mods
                    if (ManMods.inst.GetCorpDefinition(corpID) != null)
                    {
                        if (GradeUnlockBlocks.Count == 0)
                        {
                            OfficialTryBuildBlockTypes();
                            //SaveToDisk();
                        }
                        if (SkinRefTech == null)
                        {
                            if (!ManSpawn.inst)
                            {
                                Debug.Log("SubMissions: ManSpawn is still loading");
                                return;
                            }
                            else
                                MakeFallbackTechIfNeeded();
                        }
                    }
                }
            }
            catch (Exception e) { Debug.Log("SubMissions: GAME is still loading " + e); }
        }
        public bool MirrorSkin(ref CorporationSkinInfo toChange, FactionSubTypes corpID, int skinIndex, out int skinID)
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
                        Debug.Log("SubMissions: MirrorSkin(SMCCorpLicense[SkinLocked]) - RenderTechImage needs time to boot");
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
                    Debug.Log("SubMissions: Made skin for " + ID + " skinIndex: " + skinIndex + " ID: " + skinID);
                    return true;
                }
                else
                {
                    toChange.m_Corporation = (FactionSubTypes)ID;
                    toChange.m_SkinUniqueID = skinID;

                    toChange.m_SkinMeshes = new SkinMeshes();
                    error++;
                    toChange.m_SkinTextureInfo = ManCustomSkins.inst.GetSkinTexture(corpID, skinIndex);
                    Debug.Log("SubMissions: MirrorSkin(SMCCorpLicense) - Skin Name: " + ManCustomSkins.inst.GetSkinNameForSnapshot(corpID, (uint)skinIndex));
                    error++;

                    UIIref = ManCustomSkins.inst.GetSkinUIInfo(corpID, skinIndex);
                    error++;
                    if (CSIRenderCache.TryGetValue(skinID, out Sprite val))
                    {
                        UII.m_PreviewImage = val;
                    }
                    else
                    {
                        Debug.Log("SubMissions: MirrorSkin(SMCCorpLicense) - RenderTechImage needs time to boot");
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
                    Debug.Log("SubMissions: Made skin for " + ID + " skinIndex: " + skinIndex + " ID: " + skinID);
                    toChange.m_SkinUIInfo = UII;
                }
            }
            catch (Exception e)
            {
                Debug.Log("SubMissions: MirrorSkin(SMCCorpLicense) - error on execution!  error no " + error + " | " + e);
                return false;
            }
            return true;
        }

        public Sprite GetPreviewImage(int skinIndex)
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
                                    Debug.Log("SubMissions: MirrorSkin(SMCCorpLicense) - Example Tech must use only the same blocks from the respective corp!");
                                    continue;
                                }*/
                                value.m_SkinID = Singleton.Manager<ManCustomSkins>.inst.SkinIndexToID((byte)skinIndex, (FactionSubTypes)ID);
                                TD.m_BlockSpecs[step] = value;
                            }
                            catch
                            {
                                Debug.Log("SubMissions: GetPreviewImage(SMCCorpLicense) - BlockSpec IS NULL");
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("SubMissions: GetPreviewImage(SMCCorpLicense) - BlockSpec IS NULL BY DEFAULT");
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
                Debug.Log("SubMissions: GetPreviewImage(SMCCorpLicense) - error " + e);
                return null;
            }
        }

        internal void BuildFactionLicenseUnofficial()
        {   //
            FactionSubTypes Lookup = (FactionSubTypes)ID;
            if (!ManLicenses.inst || ManLicenses.inst.m_ThresholdData == null)
            {
                Debug.Log("SubMissions: Corporation Licence for " + Faction + ", ID: " + ID + " cannot be built as ManLicenses is " + (!ManLicenses.inst ? "still loading" : "still building"));
            }
            else if (!ManLicenses.inst.m_ThresholdData.Exists(delegate (ManLicenses.ThresholdsTableEntry cand) { return cand.faction == Lookup; }))
            {
                ManLicenses.ThresholdsTableEntry TTE = new ManLicenses.ThresholdsTableEntry();
                TTE.faction = Lookup;
                TTE.thresholds = BuildThresholds();
                ManLicenses.inst.m_ThresholdData.Insert(0, TTE);

                Debug.Log("SubMissions: Init Corporation Licence for " + Faction + ", ID: " + ID + ".\n     This cannot be changed after first load as it has a massive gameplay impact.");
            }
            else
                Debug.Log("SubMissions: Corporation Licence for " + Faction + ", ID: " + ID + " is already built");
        }
        private FactionLicense.Thresholds BuildThresholds()
        {   //
            FactionLicense.Thresholds TH = new FactionLicense.Thresholds();
            TH.m_MaxSupportedLevel = GradesXP.Length;
            TH.m_XPLevels = GradesXP;
            return TH;
        }

        internal void PushBlockTypesToAssignedCorp()
        {   //

            FactionSubTypes FST = (FactionSubTypes)ID;
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
                            if (ManTechMaterialSwap.inst.m_FinalCorpMaterials.ContainsValue(rend.sharedMaterial))
                                rend.sharedMaterial = val;
                        }
                        //TB.GetComponent<MaterialSwapper>().SetupMaterial(null, FST);
                    }
                }
                //Debug.Log("SubMissions: Pushed all blocks textures to corp " + ID);
            }
            else
                Debug.Log("SubMissions: ManTechMaterialSwap - Failed. Could not push all blocks textures to corp " + ID);
            foreach (KeyValuePair<int, Material> CBR in ManTechMaterialSwap.inst.m_FinalCorpMaterials)
            {
                Debug.Log("SubMissions: ManTechMaterialSwap - " + CBR.Key + " | " + CBR.Value );
            }
        }
        private void UnofficialTryBuildBlockTypes()
        {   //
            if (KickStart.isBlockInjectorPresent)
            {
                BlockTypes BT;
                int smallestCabVolume = 262145;
                foreach (KeyValuePair<int, CustomBlock> pair in BlockLoader.CustomBlocks)
                {
                    CustomBlock CB = pair.Value;
                    if (CB.Name.StartsWith(Faction))
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
                                    FirstCabUnlocked = BT;
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
        }
        private void OfficialTryBuildBlockTypes()
        {   //
            ModdedCorpDefinition MCD = ManMods.inst.FindCorp(ID);
            if (MCD)
            {
                ModSessionInfo MSI = (ModSessionInfo)sess.GetValue(ManMods.inst);
                if (MSI == new ModSessionInfo())
                {
                    Debug.Log("SubMissions: SMCCorpLicense - THE MOD SESSION DOES NOT EXIST!");
                    return;
                }
                if (MSI.BlockIDs.Count == 0)
                {
                    Debug.Log("SubMissions: SMCCorpLicense - Corp exists but it's respective mod HAS NO BLOCKS!");
                    return;
                }
                int smallestCabVolume = 262145;
                foreach (int BTI in MSI.BlockIDs.Keys)
                {
                    BlockTypes BT = (BlockTypes)BTI;
                    TankBlock TB = ManSpawn.inst.GetBlockPrefab(BT);
                    if (TB)
                    {
                        int cabVolume = TB.filledCells.Length;
                        var techControl = TB.GetComponent<ModuleTechController>();
                        if (techControl && smallestCabVolume > cabVolume)
                        {
                            if (techControl.HandlesPlayerInput)
                            {
                                FirstCabUnlocked = BT;
                                smallestCabVolume = cabVolume;
                            }
                        }
                        if (TB.name.StartsWith(Faction))
                        {
                            int CBGrade = TB.m_Tier;
                            for (int step = 0; step < CBGrade; step++)
                            {
                                if (GradeUnlockBlocks.Count < step)
                                {
                                    GradeUnlockBlocks.Add(new SMCCorpBlockRange());
                                }
                            }
                            BT = (BlockTypes)BTI;
                            if (!GradeUnlockBlocks[CBGrade].BlocksOutOfRange.Contains(BT))
                                GradeUnlockBlocks[CBGrade].BlocksOutOfRange.Add(BT);
                        }
                    }
                }
                if (FirstCabUnlocked == BlockTypes.GSOCockpit_111)
                {
                    Debug.Log("SubMissions: SMCCorpLicense - Corp has no vaild cab block.  Defaulting to the first block declared...");
                    FirstCabUnlocked = (BlockTypes)MSI.BlockIDs.First().Key;
                }

                Sprite shared = Sprite.Create(MCD.m_Icon, new Rect(0, 0, 64, 64), Vector2.zero);
                SmallCorpIcon = shared;
                SmallSelectedCorpIcon = shared;
                HighResCorpIcon = shared;
            }
        }

        public BlockUnlockTable.CorpBlockData UnofficialGetCorpBlockData()
        {   //
            if (corpBlockData == null)
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
                        UDl.Add(UD);
                    }
                    BlockUnlockTable.GradeData GD = new BlockUnlockTable.GradeData {
                        m_BlockList = UDl.ToArray(),
                        m_AdditionalUnlocks = new BlockTypes[0] { },
                    };
                    GDl.Add(GD);
                }

                BlockUnlockTable.CorpBlockData CBD = new BlockUnlockTable.CorpBlockData { m_GradeList = GDl.ToArray(), };
                corpBlockData = CBD;
                Debug.Log("SubMissions: UnofficialGetCorpBlockData(SMCCorpLicense) - corpBlockData was built.");
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
                    Debug.Log("SubMissions: There were no Skin.json skins for Custom Corp " + Faction);
                    return;
                }
                Debug.Log("SubMissions: Imported " + importedSkins.Count + " Skin.json skins for Custom Corp " + Faction);
            }
            catch
            {
                SMUtil.Assert(false, "SubMissions: Check your Skins file names!  Some may be invalid!");
            }
        }

        // Crates
        public void TryBuildCrate()
        {   //
            try
            {
                if (CrateReferenceFaction != FactionSubTypes.NULL)
                {
                    Dictionary<FactionSubTypes, Crate> refs = (Dictionary<FactionSubTypes, Crate>)crateS.GetValue(ManSpawn.inst);
                    if (refs.TryGetValue(CrateReferenceFaction, out Crate refCrate))
                    {
                        Mesh mesh = SMissionJSONLoader.LoadMesh(GetDirectory() + SMissionJSONLoader.up + "Crate_Base.obj");


                        HasCratePrefab = true;
                    }

                }
            }
            catch (Exception e)
            { 
                Debug.Log("Failiure on crate addition for corp " + Faction + " | " + e);
                return;
            }
            Debug.Log("Failiure on crate addition for corp " + Faction + " | The crate has missing models: Make sure you have a: ");
            Debug.Log("\"Crate_Base.obj\", \"Crate_A.obj\", \"Crate_B.obj\", \"Crate_A_Lock.obj\", \"Crate_B_Lock.obj\", \"Crate_B_LightRed.obj\", \"Crate_B_LightGreen.obj\"");
        }

        // Fallback
        private void MakeFallbackTechIfNeeded()
        {   //
            if (!KickStart.FullyLoadedGame)
                return;
            if (ManSpawn.inst.GetBlockPrefab(FirstCabUnlocked) == null)
            {
                Debug.Log("SubMissions: THERE IS NO ASSIGNED CAB WITHIN THIS CORP!!!");
                FirstCabUnlocked = BlockTypes.GSOCockpit_111;
            }
            if (FirstCabUnlocked == BlockTypes.GSOCockpit_111)
            {
                Debug.Log("SubMissions: Cannot make fallback tech.  No block specified for cab.");
                return;
            }
            Debug.Log("SubMissions: Making fallback Tech...");
            string blockName = ManSpawn.inst.GetBlockPrefab(FirstCabUnlocked).name;
            TechData TD = new TechData();
            TD.Name = "FALLBACK";
            TD.m_BlockSpecs = new List<TankPreset.BlockSpec>();
            TD.m_TechSaveState = new Dictionary<int, TechComponent.SerialData>();
            TD.m_CreationData = new TechData.CreationData();
            TD.m_SkinMapping = new Dictionary<uint, string>();

            BlockTypes BT = FirstCabUnlocked;
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
            Debug.Log("SubMissions: Made Skinref Tech (FALLBACK) for ID " + ID);
        }


        // FileSys
        public static FieldInfo techData = typeof(TankPreset)
                   .GetField("m_TechData", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        internal string GetDirectory()
        {
            return SMissionJSONLoader.MissionCorpsDirectory + SMissionJSONLoader.up + Faction;
        }
        public void TryFindTextures()
        {   //
            try
            {
                if (SmallCorpIcon == null && !StandardCorpIcon.NullOrEmpty())
                {
                    Texture tex = FileUtils.LoadTexture(GetDirectory() + SMissionJSONLoader.up + StandardCorpIcon);
                    SmallCorpIcon = Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                }
                if (SmallSelectedCorpIcon == null && !SelectedCorpIcon.NullOrEmpty())
                {
                    Texture tex = FileUtils.LoadTexture(GetDirectory() + SMissionJSONLoader.up + SelectedCorpIcon);
                    SmallSelectedCorpIcon = Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                }
                if (HighResCorpIcon == null && !HighResolutionCorpIcon.NullOrEmpty())
                {
                    Texture tex = FileUtils.LoadTexture(GetDirectory() + SMissionJSONLoader.up + HighResolutionCorpIcon);
                    HighResCorpIcon = Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                }
                if (SkinRefTech == null && !ExampleSkinTech.NullOrEmpty())
                {
                    Texture tex = FileUtils.LoadTexture(GetDirectory() + SMissionJSONLoader.up + ExampleSkinTech);
                    if (ManScreenshot.TryDecodeSnapshotRender((Texture2D)tex, out TechData.SerializedSnapshotData data))
                    {
                        TankPreset TP = TankPreset.CreateInstance();
                        techData.SetValue(TP, data.CreateTechData());
                        SkinRefTech = TP;
                    }
                }
            }
            catch (Exception e)
            { Debug.Log(e); }
        }
        

        private void SaveToDisk()
        {
            try
            {
                SMissionJSONLoader.ValidateDirectory(SMissionJSONLoader.MissionCorpsDirectory);
                SMissionJSONLoader.ValidateDirectory(GetDirectory());
                string toSave = JsonConvert.SerializeObject(this, Formatting.Indented);
                SMissionJSONLoader.TryWriteToJSONFile(GetDirectory() + SMissionJSONLoader.up + "MissionCorp", toSave);
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "SubMissions: Could not edit MissionCorp.json.  \n   This could be due to a bug with this mod or file permissions. - " + e);
                return;
            }
        }
        public static void SaveTemplateToDisk()
        {
            try
            {
                SMCCorpLicense CL = new SMCCorpLicense("ExampleCorp", 10001, new int[3] { 100, 450, 9001 }, true);
                CL.SaveToDisk();
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "SubMissions: Could not edit MissionCorp.json.  \n   This could be due to a bug with this mod or file permissions. - " + e);
                return;
            }
        }
    }
}
