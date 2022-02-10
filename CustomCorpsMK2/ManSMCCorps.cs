using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using UnityEngine;
using Nuterra.BlockInjector;
using Newtonsoft.Json;

namespace Sub_Missions
{
    public class ManSMCCorps
    {
        internal const int UCorpRange = 50000;
        private static int RCC = UCorpRange;

        internal const int AdvisedCorpIDStartRef = 128;

        private static Dictionary<int, SMCCorpLicense> corpsStored = new Dictionary<int, SMCCorpLicense>();
        private static Dictionary<int, SMCCorpLicense> corpsStoredOfficial = new Dictionary<int, SMCCorpLicense>();
        internal static RewardSpawner crateThing = new RewardSpawner();

        internal static bool hasScanned = false;

        public static List<SMCCorpLicense> GetAllSMCCorps()
        {   //
            try
            {
                return corpsStored.Values.ToList();
            }
            catch
            {
                Debug.Log("SubMissions: GetAllSMCCorps - Tried to fetch value but corpsStored is null!?  ");
            }
            return new List<SMCCorpLicense>();
        }
        public static int GetSMCCorpsCount()
        {   //
            try
            {
                return corpsStored.Count();
            }
            catch
            {
                Debug.Log("SubMissions: Tried to fetch value but corpsStored is null!?  ");
            }
            return 0;
        }
        public static bool GetSMCID(string Faction, out FactionSubTypes FST)
        {   //
            FST = FactionSubTypes.NULL;
            try
            {
                FST = (FactionSubTypes)GetSMCCorp(Faction).ID;
                return true;
            }
            catch
            {
                Debug.Log("SubMissions: Tried to fetch value but corpsStored is null!?  ");
            }
            return false;
        }
        public static SMCCorpLicense GetSMCCorp(string corpName)
        {   //
            try
            {
                int hash = corpName.GetHashCode();
                return corpsStored.Values.ToList().Find(delegate (SMCCorpLicense cand) { return cand.Faction.GetHashCode() == hash; });
            }
            catch
            {
                Debug.Log("SubMissions: Tried to fetch value but corpsStored is null!?  ");
            }
            return null;
        }
        public static bool GetSMCCorp(string corpName, out SMCCorpLicense CL)
        {   //
            CL = GetSMCCorp(corpName);
            if (CL != null)
                return true;
            return false;
        }


        public static bool TryGetSMCCorpLicense(int corpID, out SMCCorpLicense CL)
        {   //
            try
            {
                if (corpsStored.TryGetValue(corpID, out CL))
                    return true;
                if (corpsStoredOfficial.TryGetValue(corpID, out CL))
                    return true;
            }
            catch (Exception e)
            {
                Debug.Log("SubMissions: TryFindSMCCorpLicense - error " + e);
            }
            CL = null;
            return false;
        }
        public static void PushUnofficialCorpsToPool()
        {   //
            try
            {
                foreach (SMCCorpLicense CL in corpsStored.Values)
                {
                    CL.BuildFactionLicenseUnofficial();
                }
            }
            catch (Exception e)
            {
                Debug.Log("SubMissions: COULD NOT PUSH CORPS TO CORP POOL!!! " + e);
            }
        }
        public static void ReloadAllOfficialIfApplcable()
        {   //
            try
            {
                if (!ManMods.inst.HasPendingLoads())
                {
                    corpsStoredOfficial.Clear();
                    List<int> corps = ManMods.inst.GetCustomCorpIDs().ToList();
                    foreach (int corpID in corps)
                    {
                        corpsStoredOfficial.Add(corpID, new SMCCorpLicense((FactionSubTypes)corpID));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("SubMissions: COULD NOT ReloadAllOfficialIfApplcable!!! " + e);
            }
        }
        internal static void ReloadAllUnofficialIfApplcable()
        {   //
            foreach (KeyValuePair<int, SMCCorpLicense> pair in corpsStored)
            {
                pair.Value.RefreshCorpUISP();
            }

        }
        internal static void InitALLFactionLicenseUnofficialUI()
        {   //
            foreach (KeyValuePair<int, SMCCorpLicense> pair in corpsStored)
            {
                UICCorpLicenses.MakeFactionLicenseUnofficialUI(pair.Value);
            }
        }

        internal static bool TryMakeNewCorp(SMCCorpLicense CL)
        {   //
            try
            {
                if (CL != null)
                {
                    if (!corpsStored.TryGetValue(CL.ID, out _))
                    {
                        corpsStored.Add(CL.ID, CL);
                        CL.TryInitFactionEXPSys();
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.Log("SubMissions: TryMakeNewCorp - Could not make new corp! " + e);
            }
            return false;
        }
        public static FactionSubTypes TryMakeNewCorp(string corpName)
        {   //
            try
            {
                SMCCorpLicense CL = GetSMCCorp(corpName);
                if (CL != null)
                    return (FactionSubTypes)CL.ID;

                RCC++;
                SMCCorpLicense CLn = new SMCCorpLicense(corpName, RCC, new int[5] { 100,200,300,400,500});
                corpsStored.Add(RCC, CLn);
            }
            catch (Exception e)
            {
                Debug.Log("SubMissions: TryMakeNewCorp - Could not make new corp! " + e);
            }
            return (FactionSubTypes)RCC;
        }
        public static FactionSubTypes TryMakeNewCorpBI(CustomCorporation CC)
        {   //
            try
            {
                SMCCorpLicense CL = GetSMCCorp(CC.Name);
                if (CL != null)
                    return (FactionSubTypes)CL.ID;
                SMCCorpLicense CLn = new SMCCorpLicense(CC);

                corpsStored.Add(CC.CorpID, CLn);
            }
            catch (Exception e)
            {
                Debug.Log("SubMissions: TryMakeNewCorpBI (BlockInjector) - Could not make new corp! " + e);
            }
            return (FactionSubTypes)CC.CorpID;
        }

        // SKINS
        private static FieldInfo
            skinSwapInsts = typeof(ManTechMaterialSwap).GetField("m_MaterialPairsInstances", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static bool builtCustomCorps = false;
        internal static void BuildUnofficialCustomCorpArrayTextures()
        {
            if (!builtCustomCorps)
            {   // VERY EXPENSIVE but we only need to do this once!
                if (((List<ManTechMaterialSwap.MatReplacePairs>)skinSwapInsts.GetValue(ManTechMaterialSwap.inst)).Count == 0)
                {
                    Debug.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - too early!");
                    return;
                }
                Debug.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - BUILDING!!!");
                foreach (KeyValuePair<int, SMCCorpLicense> corpEntry in corpsStored)
                {
                    int skinCount = corpEntry.Value.importedSkins.Count + corpEntry.Value.registeredSkins.Count;
                    Debug.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - count " + skinCount);
                    bool worked = false;
                    try
                    {
                        ManCustomSkins.inst.GetCorpSkinTextureInfos((FactionSubTypes)corpEntry.Key);
                        worked = true;
                    }
                    catch { }
                    Debug.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - 1");
                    if (skinCount == 0 || !worked)
                    {
                        Debug.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - COULD NOT COMPILE FOR " + corpEntry.Value.Faction);
                        continue;
                    }
                    Debug.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - 2");
                    ManTechMaterialSwap.MatReplacePairs MPP = new ManTechMaterialSwap.MatReplacePairs();
                    List<ManTechMaterialSwap.MatReplacePairs> stuff = (List<ManTechMaterialSwap.MatReplacePairs>)skinSwapInsts.GetValue(ManTechMaterialSwap.inst);
                    List<Material> mats = new List<Material>();
                    Material newMat = new Material(stuff.First().m_Materials.First());
                    newMat.name = corpEntry.Value.Faction + "_Main";
                    mats.Add(newMat);
                    for (int step = 1; step < skinCount; step++)
                    {
                        mats.Add(newMat);
                    }
                    Debug.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - 3");
                    MPP.m_Materials = mats.ToArray();
                    List<bool> bools = new List<bool>();
                    for (int step = 0; step < skinCount; step++)
                        bools.Add(true);
                    MPP.m_ScrollMaterial = bools.ToArray();
                    stuff.Add(MPP);

                    ManTechMaterialSwap.MatSwapGroup MSG = new ManTechMaterialSwap.MatSwapGroup();
                    MSG.m_Corp = (FactionSubTypes)corpEntry.Key;
                    List<ManTechMaterialSwap.MatSwapInfo> MSIL = new List<ManTechMaterialSwap.MatSwapInfo>();
                    for (int step = 0; step < skinCount; step++)
                        MSIL.Add(ManTechMaterialSwap.inst.m_MaterialsToSwap.First().m_Materials.First());
                    MSG.m_Materials = MSIL.ToArray();
                    Debug.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - 4");

                    skinSwapInsts.SetValue(ManTechMaterialSwap.inst, stuff);
                    ManTechMaterialSwap.inst.m_MaterialsToSwap.Add(MSG);
                    Debug.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - Compiled for " + corpEntry.Value.Faction);
                    Debug.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - new " + stuff.Count + " | " + ManTechMaterialSwap.inst.m_MaterialsToSwap.Count);
                }
                builtCustomCorps = true;
                ManTechMaterialSwap.inst.RebuildCorpArrayTextures();
                foreach (KeyValuePair<int, SMCCorpLicense> corpEntry in corpsStored)
                {
                    corpEntry.Value.PushBlockTypesToAssignedCorp();
                }
            }
        }


        // FileSystem
        /// <summary>
        /// On first initialization
        /// </summary>
        internal static void LoadAllCorps()
        {
            ReloadAllCorps(false); 
        }
        internal static void ReloadAllCorps(bool isReloading = false)
        {
            List<string> names = GetNameList();
            List<int> hashSetExisting = new List<int>();
            List<int> hashSetAlready = new List<int>();
            foreach (string name in Enum.GetNames(typeof(FactionSubTypes)))
            {
                hashSetExisting.Add(name.GetHashCode());
            }
            foreach (string name in names)
            {
                int hash = name.ToUpper().GetHashCode();
                if (hashSetExisting.Contains(hash))
                {
                    SMUtil.Assert(false, name + " is a Vanilla Corp!  \n  SubMissions cannot overwrite or edit existing faction licenses.");
                    continue;
                }
                else if (hashSetAlready.Contains(hash))
                {
                    SMUtil.Assert(false, "Custom Corp " + name + " already exists!  \n  Make sure there's only one corp with the same Faction name! \n   Lowercase letters are treated the same as uppercase.");
                    continue;
                }
                else
                    hashSetAlready.Add(hash);
                if (CorpLoader(name, out SMCCorpLicense CL))
                {
                    if (corpsStored.ContainsKey(CL.ID))
                    {
                        corpsStored.Remove(CL.ID);
                        Debug.Log("SubMissions: Reloaded Corp " + name);
                    }
                    else
                        Debug.Log("SubMissions: Added Corp " + name);
                    corpsStored.Add(CL.ID, CL);
                }
                else
                    SMUtil.Assert(false, "Could not load Corp " + name);

            }
            if (isReloading)
            {
                SMUtil.Assert(false, "Note: You will have to reload your save to get all of the changes to apply");
                SMUtil.PushErrors();
            }
            hasScanned = true;
        }
        private static List<string> GetNameList(string directoryFromMissionCorpsDirectory = "", bool doJSON = false)
        {
            string search;
            if (directoryFromMissionCorpsDirectory == "")
                search = SMissionJSONLoader.MissionCorpsDirectory;
            else
                search = SMissionJSONLoader.MissionCorpsDirectory + SMissionJSONLoader.up + directoryFromMissionCorpsDirectory;
            List<string> toClean;
            if (doJSON)
                toClean = Directory.GetFiles(search).ToList();
            else
                toClean = Directory.GetDirectories(search).ToList();
            //Debug.Log("SubMissions: Cleaning " + toClean.Count);
            List<string> Cleaned = new List<string>();
            foreach (string cleaning in toClean)
            {
                if (SMissionJSONLoader.GetName(cleaning, out string cleanName, doJSON))
                {
                    Cleaned.Add(cleanName);
                }
            }
            return Cleaned;
        }
        private static bool CorpLoader(string CorpName, out SMCCorpLicense License, bool Init = true)
        {
            try
            {
                string output = LoadMissionCorpFromFile(CorpName);
                License = JsonConvert.DeserializeObject<SMCCorpLicense>(output);
                if (Init)
                {
                    try
                    {
                        License.TryFindTextures();
                        License.TryInitFactionEXPSys();
                    }
                    catch { }
                }
                return true;
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "SubMissions: Check your Corp file names, cases where you referenced the names and make sure they match!!! " + e);
                License = null;
                return false;
            }
        }
        private static string LoadMissionCorpFromFile(string corpName)
        {
            string destination = SMissionJSONLoader.MissionCorpsDirectory + SMissionJSONLoader.up + corpName;
            SMissionJSONLoader.ValidateDirectory(SMissionJSONLoader.MissionCorpsDirectory);
            SMissionJSONLoader.ValidateDirectory(destination);
            try
            {
                string output = File.ReadAllText(destination + SMissionJSONLoader.up + "MissionCorp.json");
                Debug.Log("SubMissions: Loaded MissionCorp.json for " + corpName + " successfully.");
                return output;
            }
            catch
            {
                SMUtil.Assert(false, "SubMissions: Could not read MissionCorp.json for " + corpName + ".  \n   This could be due to a bug with this mod or file permissions.");
                return null;
            }
        }

        // For Custom Corp Skins
        internal static bool SkinLoader(SMCCorpLicense CL)
        {
            try
            {
                /*
                if (CL.importedSkins != null)
                {
                    Debug.Log("SubMissions: SkinLoader - Already exported...");
                    return true;
                }
                */
                CL.importedSkins.Clear();
                Debug.Log("SubMissions: Searching Custom Corp " + CL.Faction + " Folder for skins...");
                string directorySkins = CL.GetDirectory() + SMissionJSONLoader.up + "Skins";
                SMissionJSONLoader.ValidateDirectory(directorySkins);
                List<string> names = GetNameList(CL.Faction + SMissionJSONLoader.up + "Skins");
                Debug.Log("SubMissions: Found " + names.Count + " skins...");
                CL.TexturesCache.Clear();
                foreach (string name in names)
                {
                    CorporationSkinInfo CSI = ScriptableObject.CreateInstance<CorporationSkinInfo>();
                    if (LoadSkinForCorp(CL, name, directorySkins + SMissionJSONLoader.up + name, ref CSI))
                    {
                        Debug.Log("SubMissions: Added Skin " + name);
                        CL.importedSkins.Add(CSI.m_SkinUniqueID);
                        ManCustomSkins.inst.AddSkinToCorp(CSI, true);
                    }
                    else
                        SMUtil.Assert(false, "Could not load Skin " + name);
                }
                return true;

            }
            catch
            {
                SMUtil.Assert(false, "SubMissions: Check your Skins file names!  Some may be invalid!");
                return false;
            }
        }
        private static bool LoadSkinForCorp(SMCCorpLicense CL, string folderName, string destination, ref CorporationSkinInfo CSI)
        {
            try
            {
                if (Directory.Exists(destination))
                {
                    SMCSkinID SID; 
                    string skinJSON = destination + SMissionJSONLoader.up + "Skin.json";
                    if (File.Exists(skinJSON))
                    {
                        SID = JsonConvert.DeserializeObject<SMCSkinID>(File.ReadAllText(skinJSON));
                    }
                    else
                    {
                        SID = new SMCSkinID();
                        int IDFind = 1; // 0 is taken by the default block textures
                        if (CL.SkinReferenceFaction != FactionSubTypes.NULL)
                            IDFind = AdvisedCorpIDStartRef;
                        while (CL.registeredSkins.Contains(IDFind) || CL.importedSkins.Contains(IDFind))//CL.importedSkins.Exists(delegate (CorporationSkinInfo cand) { return cand.m_SkinUniqueID == IDFind; }))
                            IDFind++;
                        SID.UniqueID = IDFind;
                        File.WriteAllText(skinJSON, JsonConvert.SerializeObject(SID, Formatting.Indented));
                        SMUtil.Assert(false, "SubMissions: \nMade a new Skin.json for " + folderName + " with ID " + IDFind + " - use this to set your skin information \n Only change UniqueID if ABSOLUETLY NESSEARY (There can only be one skin to one UniqueID)");
                        SMUtil.PushErrors();
                    }
                    if (SID.UniqueID < 1)
                    {
                        SMUtil.Assert(false, "SubMissions: Skin UniqueID for skin " + folderName + ".  is below 1 - 0 is already auto-generated with existing blocks and below is not allowed!!");
                        return false;
                    }
                    else if (CL.registeredSkins.Contains(SID.UniqueID))
                    {
                        Debug.Log("SubMissions: Skin " + folderName + "'s UniqueID was already registered at a referenced skin.  Overwriting the reference!");
                    }
                    if (CL.importedSkins.Contains(SID.UniqueID) )// FindAll(delegate (CorporationSkinInfo cand) { return cand.m_SkinUniqueID == SID.UniqueID; }).Count > 1)
                    {
                        SMUtil.Assert(false, "SubMissions: Skin " + folderName + "'s UniqueID was already registered before by another respective Skin.json corp skin!  Unable to import!");
                        return false;
                    }
                    if (SID.UniqueID < AdvisedCorpIDStartRef && CL.SkinReferenceFaction != FactionSubTypes.NULL)
                    {
                        SMUtil.Assert(false, "SubMissions: Skin UniqueID for skin " + folderName + ".  is below 128, the advised start range for Unofficial Custom Corps with a Skin reference corp. \n  This can cause compatability issues later on with Official and Unofficial mods.");
                    }

                    CorporationSkinUIInfo UII = new CorporationSkinUIInfo();
                    
                    // Setup skins
                    SkinTextures refs = ManCustomSkins.inst.GetCorpSkinTextureInfos((FactionSubTypes)CL.ID).First();
                    SkinTextures ST = new SkinTextures();
                    ST.m_Albedo = TryGetPNG(destination, SID.Albedo);
                    //ST.m_Albedo.Resize(refs.m_Albedo.width, refs.m_Albedo.height);
                    CL.TexturesCache.Add(ST.m_Albedo);
                    ST.m_Emissive = TryGetPNG(destination, SID.Emissive);
                    //ST.m_Emissive.Resize(refs.m_Emissive.width, refs.m_Emissive.height);
                    CL.TexturesCache.Add(ST.m_Emissive);
                    ST.m_Metal = TryGetPNG(destination, SID.Metal);
                    //ST.m_Metal.Resize(refs.m_Metal.width, refs.m_Metal.height);
                    CL.TexturesCache.Add(ST.m_Metal);
                    CSI.m_SkinTextureInfo = ST;
                    
                    // Setup UI
                    UII.m_AlwaysEmissive = SID.AlwaysEmissive;
                    UII.m_FallbackString = folderName;
                    Texture2D T2D;
                    if (!File.Exists(destination + SMissionJSONLoader.up + SID.Preview))
                    {
                        if (CL.CSIRenderCache.TryGetValue(SID.UniqueID, out Sprite val))
                        {
                            FileUtils.SaveTexture(val.texture, destination + SMissionJSONLoader.up + SID.Preview);
                            UII.m_PreviewImage = val;
                        }
                        else
                        {
                            Debug.Log("SubMissions: LoadSkinForCorp(SMCCorpLicense) - RenderTechImage is queued");
                            SMCCorpLicense.CSIBacklogRender.Add(CSI);
                            T2D = TryGetPNG(destination, SID.Preview);
                            UII.m_PreviewImage = Sprite.Create(T2D, new Rect(0, 0, T2D.width, T2D.height), Vector2.zero);
                            SMCCorpLicense.needsToRenderSkins = true;
                        }
                    }
                    else
                    {
                        T2D = TryGetPNG(destination, SID.Preview);
                        UII.m_PreviewImage = Sprite.Create(T2D, new Rect(0, 0, T2D.width, T2D.height), Vector2.zero);
                    }
                    T2D = TryGetPNG(destination, SID.Button);
                    Sprite buttonImage = Sprite.Create(T2D, new Rect(0, 0, T2D.width, T2D.height), Vector2.zero);
                    UII.m_SkinButtonImage = buttonImage;
                    UII.m_SkinMiniPaletteImage = buttonImage;
                    UII.m_SkinLocked = false;
                    UII.m_IsModded = false;// we know it's modded but this is to get it without the wrench
                    UII.m_LocalisedString = null;
                    CSI.m_SkinUIInfo = UII;

                    //Setup other
                    CSI.m_SkinMeshes = new SkinMeshes();
                    CSI.m_Corporation = (FactionSubTypes)CL.ID;
                    CSI.m_SkinUniqueID = SID.UniqueID;

                    if (ST.m_Emissive == null || ST.m_Metal == null || ST.m_Albedo == null)
                    {
                        SMUtil.Assert(false, "SubMissions: Elements of the skin ARE NULL");
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "SubMissions: Could not read Skin.  \n   This could be due to a bug with this mod or file permissions. " + e);
                return false;
            }
        }
        private static Texture2D TryGetPNG(string destination, string FileName)
        {
            if (File.Exists(destination + SMissionJSONLoader.up + FileName))
            {
                return (Texture2D)FileUtils.LoadTexture(destination + SMissionJSONLoader.up + FileName);
            }
            SMUtil.Assert(false, "SubMissions: Could not read Skin texture " + FileName + ".  \n   This could be due to a bug with this mod or file permissions.");
            return ManUI.inst.GetModernCorpIcon(FactionSubTypes.GSO).texture;
        }
    }
}
