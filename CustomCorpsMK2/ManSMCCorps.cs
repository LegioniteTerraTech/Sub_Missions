using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json;

#if !STEAM
using Nuterra.BlockInjector;
#endif

namespace Sub_Missions
{
    public class ManSMCCorps : MonoBehaviour
    {
        internal const int UCorpRange = 1000000;
        internal static int CustCorpStartID = 16;
        private static int RCC = UCorpRange;

        internal const int AdvisedCorpSkinIDStartRef = 128;

        private static Dictionary<int, SMCCorpLicense> corpsStoredUnofficial = new Dictionary<int, SMCCorpLicense>();
        private static Dictionary<int, SMCCorpLicense> corpsStoredOfficial = new Dictionary<int, SMCCorpLicense>();
        private static Dictionary<int, SMCCorpLicense> corpsStored
        {
            get
            {
                Dictionary<int, SMCCorpLicense> corpsS = new Dictionary<int, SMCCorpLicense>(corpsStoredUnofficial);
                foreach (var item in corpsStoredOfficial)
                {
                    corpsS.Add(item.Key, item.Value);
                }
                return corpsS;
            }
        }
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
                Debug_SMissions.Log("SubMissions: GetAllSMCCorps - Tried to fetch value but corpsStored is null!?  ");
            }
            return new List<SMCCorpLicense>();
        }
        public static List<FactionSubTypes> GetAllSMCCorpFactionTypes()
        {   //
            try
            {
                return corpsStored.Select(x => (FactionSubTypes)x.Key).ToList();
            }
            catch
            {
                Debug_SMissions.Log("SubMissions: GetAllSMCCorps - Tried to fetch value but corpsStored is null!?  ");
            }
            return new List<FactionSubTypes>();
        }
        public static int GetSMCCorpsCount()
        {   //
            try
            {
                return corpsStored.Count();
            }
            catch
            {
                Debug_SMissions.Log("SubMissions: GetSMCCorpsCount - Tried to fetch value but corpsStored is null!?  ");
            }
            return 0;
        }
        public static bool GetSMCIDUnofficial(string Faction, out FactionSubTypes FST)
        {   //
            FST = FactionSubTypes.NULL;
            try
            {
                FST = (FactionSubTypes)GetSMCCorpUnofficial(Faction).ID;
                return true;
            }
            catch
            {
                Debug_SMissions.Log("SubMissions: Tried to fetch value but corpsStored is null!?  ");
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
                Debug_SMissions.Assert("SubMissions: GetSMCCorp - Tried to fetch value but corpsStored is null!?  ");
            }
            return null;
        }
        public static SMCCorpLicense GetSMCCorpUnofficial(string corpName)
        {   //
            try
            {
                int hash = corpName.GetHashCode();
                return corpsStoredUnofficial.Values.ToList().Find(delegate (SMCCorpLicense cand) { return cand.Faction.GetHashCode() == hash; });
            }
            catch
            {
                Debug_SMissions.Assert("SubMissions: GetSMCCorp - Tried to fetch value but corpsStored is null!?  ");
            }
            return null;
        }
        public static SMCCorpLicense GetSMCCorpBlocks(string corpName)
        {   //
            try
            {
                int hash = corpName.GetHashCode();
                return corpsStored.Values.ToList().Find(delegate (SMCCorpLicense cand) { return cand.GetNameHashForBlocks() == hash; });
            }
            catch
            {
                Debug_SMissions.Assert("SubMissions: GetSMCCorpBlocks - Tried to fetch value but corpsStored is null!?  ");
            }
            return null;
        }
        public static bool GetSMCCorpUnofficial(string corpName, out SMCCorpLicense CL)
        {   //
            CL = GetSMCCorpUnofficial(corpName);
            if (CL != null)
                return true;
            return false;
        }


        public static bool IsSMCCorpLicense(int corp)
        {
            return corp >= CustCorpStartID;
        }
        public static bool IsOfficialSMCCorpLicense(int corp)
        {
            return corp >= CustCorpStartID && corp < UCorpRange;
        }
        public static bool IsUnofficialSMCCorpLicense(int corp)
        {
            return corp >= UCorpRange;
        }
        public static bool IsUnofficialSMCCorpLicense(FactionSubTypes corp) 
        {
            return (int)corp >= UCorpRange;
        }
        public static bool TryGetSMCCorpLicense(int corpID, out SMCCorpLicense CL)
        {   //
            try
            {
                if (corpsStoredUnofficial.TryGetValue(corpID, out CL))
                    return true;
                if (corpsStoredOfficial.TryGetValue(corpID, out CL))
                    return true;
            }
            catch (Exception e)
            {
                Debug_SMissions.Log("SubMissions: TryFindSMCCorpLicense - error " + e);
            }
            CL = null;
            return false;
        }
        internal static void PushUnofficialCorpsToPool()
        {   //
            try
            {
                foreach (SMCCorpLicense CL in corpsStoredUnofficial.Values)
                {
                    CL.BuildFactionLicenseUnofficial();
                }
            }
            catch (Exception e)
            {
                Debug_SMissions.Log("SubMissions: COULD NOT PUSH CORPS TO CORP POOL!!! " + e);
            }
        }
        internal static void ReloadAllOfficialIfApplcable()
        {   //
            try
            {
                Debug_SMissions.Log("SubMissions: SetupLicenses - Injecting Official corps...");
                if (!ManMods.inst.HasPendingLoads())
                {
                    corpsStoredOfficial.Clear();
                    List<int> corps = ManMods.inst.GetCustomCorpIDs().ToList();
                    foreach (int corpID in corps)
                    {
                        FactionSubTypes FST = (FactionSubTypes)corpID;
                        string sName = ManMods.inst.FindCorpShortName(FST);
                        if (OfficialCorpLoader(sName, FST, out SMCCorpLicense License))
                        {
                            corpsStoredOfficial.Add(corpID, License);
                            Debug_SMissions.Log("SubMissions: Added Corp " + sName);
                        }
                        else
                        {
                            corpsStoredOfficial.Add(corpID, new SMCCorpLicense(FST, ManMods.inst.GetCorpDefinition(FST)));
                            Debug_SMissions.Log("SubMissions: Added Corp " + sName + " with no MissionCorp.json present.");
                        }
                    }
                }
                else
                {
                    Debug_SMissions.FatalError("ManMods has failed to load somehow, Sub_Missions may encounter some serious problems later on.  YOU HAVE BEEN WARNED");
                }
            }
            catch (Exception e)
            {
                Debug_SMissions.Log("SubMissions: COULD NOT ReloadAllOfficialIfApplcable!!! " + e);
                Debug_SMissions.FatalError("ReloadAllOfficialIfApplcable (Important trigger) failed somewhere, Sub_Missions may encounter some serious problems later on.  YOU HAVE BEEN WARNED");
            }
        }
        internal static void ReloadAllUnofficialIfApplcable()
        {   //
            foreach (KeyValuePair<int, SMCCorpLicense> pair in corpsStoredUnofficial)
            {
                pair.Value.RefreshCorpUISP();
            }

        }
        internal static void InitALLFactionLicenseUnofficialUI()
        {   //
            foreach (KeyValuePair<int, SMCCorpLicense> pair in corpsStoredUnofficial)
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
                    if (!corpsStoredUnofficial.TryGetValue(CL.ID, out _))
                    {
                        corpsStoredUnofficial.Add(CL.ID, CL);
                        CL.TryInitFactionEXPSys();
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug_SMissions.Log("SubMissions: TryMakeNewCorp - Could not make new corp! " + e);
            }
            return false;
        }
        public static FactionSubTypes TryMakeNewCorp(string corpName)
        {   //
            try
            {
                SMCCorpLicense CL = GetSMCCorpUnofficial(corpName);
                if (CL != null)
                    return (FactionSubTypes)CL.ID;

                RCC++;
                SMCCorpLicense CLn = new SMCCorpLicense(corpName, RCC, new int[5] { 100,200,300,400,500});
                corpsStoredUnofficial.Add(RCC, CLn);
            }
            catch (Exception e)
            {
                Debug_SMissions.Log("SubMissions: TryMakeNewCorp - Could not make new corp! " + e);
            }
            return (FactionSubTypes)RCC;
        }
#if !STEAM
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
                Debug_SMissions.Log("SubMissions: TryMakeNewCorpBI (BlockInjector) - Could not make new corp! " + e);
            }
            return (FactionSubTypes)CC.CorpID;
        }
#endif

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
                    Debug_SMissions.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - too early!");
                    return;
                }
                bool criticalerror = false;
                Debug_SMissions.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - BUILDING!!!");
                foreach (KeyValuePair<int, SMCCorpLicense> corpEntry in corpsStoredUnofficial)
                {
                    int skinCount = corpEntry.Value.importedSkins.Count + corpEntry.Value.registeredSkins.Count;
                    Debug_SMissions.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - count " + skinCount);
                    bool worked = false;
                    List<SkinTextures> STs = new List<SkinTextures>();
                    try
                    {
                        STs = ManCustomSkins.inst.GetCorpSkinTextureInfos((FactionSubTypes)corpEntry.Key);
                        worked = true;
                    }
                    catch { }
                    Debug_SMissions.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - 1");
                    if (skinCount == 0 || !worked)
                    {
                        criticalerror = true;
                        SMUtil.Assert(false, "SubMissions: BuildUnofficialCustomCorpArrayTextures - COULD NOT COMPILE FOR " + corpEntry.Value.Faction);
                        continue;
                    }
                    Debug_SMissions.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - 2");
                    ManTechMaterialSwap.MatReplacePairs MPP = new ManTechMaterialSwap.MatReplacePairs();
                    List<ManTechMaterialSwap.MatReplacePairs> stuff = (List<ManTechMaterialSwap.MatReplacePairs>)skinSwapInsts.GetValue(ManTechMaterialSwap.inst);
                    List<Material> mats = new List<Material>();
                    Material newMat = new Material(stuff.First().m_Materials.First());

                    Debug_SMissions.Log("SubMissions: Size Alb: "
                        + newMat.GetTexture("_MainTex").width
                        + " Size Met: "
                        + newMat.GetTexture("_MetallicGlossMap").width
                        + " Size Emissive: " +
                        newMat.GetTexture("_EmissionMap").width);

                    SkinTextures ST = STs.First();
                    //newMat.SetTexturesToMaterial(ST.m_Albedo, ST.m_Metal, ST.m_Emissive, false);
                    newMat.name = corpEntry.Value.Faction + "_Main";
                    mats.Add(newMat);
                    for (int step = 1; step < skinCount; step++)
                    {
                        mats.Add(newMat);
                    }
                    /*
                    Debug_SMissions.Log("SubMissions: new Size Alb: " 
                        + newMat.GetTexture("_MainTex").width
                        + " Size Met: "
                        + newMat.GetTexture("_MetallicGlossMap").width
                        + " Size Emissive: " + 
                        newMat.GetTexture("_EmissionMap").width);
                    */
                    Debug_SMissions.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - skin count " + skinCount);
                    if (skinCount < 2)
                    {
                        criticalerror = true;
                        SMUtil.Assert(false, "Minimum allowed skins for any Unofficial corp is 2. Unofficial faction " + corpEntry.Value.Faction + " does NOT have enough skins for this operation!");
                    }
                    Debug_SMissions.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - 3");
                    MPP.m_Materials = mats.ToArray();
                    List<bool> bools = new List<bool>();
                    for (int step = 0; step < skinCount; step++)
                        bools.Add(true);
                    MPP.m_ScrollMaterial = bools.ToArray();
                    stuff.Add(MPP);

                    ManTechMaterialSwap.MatSwapGroup MSG = new ManTechMaterialSwap.MatSwapGroup();
                    MSG.m_Corp = (FactionSubTypes)corpEntry.Key;
                    List<ManTechMaterialSwap.MatSwapInfo> MSIL = new List<ManTechMaterialSwap.MatSwapInfo>();
                    ManTechMaterialSwap.MatSwapInfo MSI = ManTechMaterialSwap.inst.m_MaterialsToSwap.First().m_Materials.First();
                    //ManTechMaterialSwap.MatSwapInfo MSI2 = new ManTechMaterialSwap.MatSwapInfo { m_Material = newMat, m_Scroll = MSI.m_Scroll, };
                    for (int step = 0; step < skinCount; step++)
                        MSIL.Add(MSI);
                    MSG.m_Materials = MSIL.ToArray();
                    Debug_SMissions.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - 4");

                    skinSwapInsts.SetValue(ManTechMaterialSwap.inst, stuff);
                    ManTechMaterialSwap.inst.m_MaterialsToSwap.Add(MSG);
                    if (corpEntry.Value.HasCratePrefab)
                    {
                        // pending...
                    }

                    Debug_SMissions.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - Compiled for " + corpEntry.Value.Faction);
                    Debug_SMissions.Log("SubMissions: BuildUnofficialCustomCorpArrayTextures - new " + stuff.Count + " | " + ManTechMaterialSwap.inst.m_MaterialsToSwap.Count);
                }
                if (criticalerror)
                {
                    SMUtil.PushErrors();
                }
                builtCustomCorps = true;
                ManTechMaterialSwap.inst.RebuildCorpArrayTextures();
                foreach (KeyValuePair<int, SMCCorpLicense> corpEntry in corpsStoredUnofficial)
                {
                    corpEntry.Value.PushBlockTypesToAssignedCorp();
                }
            }
        }


        // CRATES
        internal static void BuildUnofficialCustomCorpCrates()
        {
            foreach (KeyValuePair<int, SMCCorpLicense> corpEntry in corpsStoredUnofficial)
            {
                corpEntry.Value.TryBuildCrate();
            }
        }

        // FileSystem
        /// <summary>
        /// On first initialization
        /// </summary>
        internal static void LoadAllCorps()
        {
            ReloadAllUnofficialCorps(false); 
        }
        internal static void ReloadAllUnofficialCorps(bool isReloading = false)
        {
            List<string> names = GetNameListUnofficial();
            List<int> hashSetExisting = new List<int>();
            List<int> hashSetAlready = new List<int>();
            foreach (string name in Enum.GetNames(typeof(FactionSubTypes)))
            {
                hashSetExisting.Add(name.GetHashCode());
            }
            foreach (string name in names)
            {
                if (name.Length > 3)
                {
                    SMUtil.Assert(false, name + " corp abbrivation exceeds the supported 3 letters.  \n  Cannot add this corp to the UI.");
                    continue;
                }
                int hash = name.ToUpper().GetHashCode();
                if (hashSetExisting.Contains(hash))
                {
                    SMUtil.Assert(false, name + " is a Vanilla Corp!  \n  SubMissions cannot overwrite or edit existing faction licenses.");
                    continue;
                }
                else if (!isReloading && hashSetAlready.Contains(hash))
                {
                    SMUtil.Assert(false, "Custom Corp " + name + " already exists!  \n  Make sure there's only one corp with the same Faction name! \n   Lowercase letters are treated the same as uppercase.");
                    continue;
                }
                else
                    hashSetAlready.Add(hash);
                if (UnofficialCorpLoader(name, out SMCCorpLicense CL))
                {
                    if (corpsStoredUnofficial.ContainsKey(CL.ID))
                    {
                        corpsStoredUnofficial.Remove(CL.ID);
                        Debug_SMissions.Log("SubMissions: Reloaded Corp " + name);
                    }
                    else
                        Debug_SMissions.Log("SubMissions: Added Corp " + name);
                    corpsStoredUnofficial.Add(CL.ID, CL);
                }
                else
                    SMUtil.Assert(false, "Could not load Corp " + name);

            }
            BuildUnofficialCustomCorpCrates();
            if (isReloading)
            {
                SMUtil.Assert(false, "Note: You will have to reload your save to get all of the changes to apply");
                SMUtil.PushErrors();
            }
            hasScanned = true;
        }
        private static List<string> GetNameListUnofficial(string directoryFromMissionCorpsDirectory = "", bool doJSON = false)
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
            //Debug_SMissions.Log("SubMissions: Cleaning " + toClean.Count);
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
        private static bool UnofficialCorpLoader(string CorpName, out SMCCorpLicense License, bool Init = true)
        {
            try
            {
                string output = LoadMissionCorpFromFile(CorpName);
                License = JsonConvert.DeserializeObject<SMCCorpLicenseJSON>(output).ConvertToActive();
                License.Faction.ToString();

                if (IsUnofficialSMCCorpLicense(License.ID))
                {
                    if (CorpName != License.Faction)
                    {
                        SMUtil.Assert(false, "Custom Corp " + License.FullName + "'s folder name " + CorpName + " does not match it's \"Faction\": variable in it's respective MissionCorp.json" + License.Faction + ".");
                        return false;
                    }
                    if (Init)
                    {
                        try
                        {
                            License.TryFindTextures();
                            License.RegisterCorpMusics();
                            License.TryInitFactionEXPSys();
                        }
                        catch { }
                    }
                    return true;
                }
                else
                {
                    SMUtil.Assert(false, "Custom Corp " + License.FullName + "'s ID is not within the valid Unofficial Custom Corps range (" + UCorpRange + " - " + int.MaxValue + ").");
                    return false;
                }
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "SubMissions: Check your Corp file for errors in syntax, cases where you referenced the names and make sure they match!!! " + e);
                License = null;
                return false;
            }
        }
        private static bool OfficialCorpLoader(string CorpShortName, FactionSubTypes FST, out SMCCorpLicense License, bool Init = true)
        {
            try
            {
                if (SMissionJSONLoader.TryGetCorpInfoDirectory(CorpShortName, out string directEnd))
                {
                    License = JsonConvert.DeserializeObject<SMCCorpLicenseJSON>(directEnd).ConvertToActive();
                    License.Faction.ToString();
                    License.OfficialCorp = true;
                    License.ID = (int)FST;

                    if (IsOfficialSMCCorpLicense(License.ID))
                    {
                        //License.TryFindTextures(); - Official Corps already have declared textures
                        //License.RegisterCorpMusics(); - Official corps already have ModCorpExtAudio
                        License.TryInitFactionEXPSys();
                        return true;
                    }
                    else
                    {
                        SMUtil.Assert(false, "Custom Corp " + License.FullName + "'s ID is not within the valid Official Custom Corps range (" + CustCorpStartID + " - " + (UCorpRange - 1) + ").");
                        return false;
                    }
                }
                License = null;
                return false;
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "SubMissions: Check your Corp file for errors in syntax, cases where you referenced the names and make sure they match!!! " + e);
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
                Debug_SMissions.Log("SubMissions: Loaded MissionCorp.json for " + corpName + " successfully.");
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
                    Debug_SMissions.Log("SubMissions: SkinLoader - Already exported...");
                    return true;
                }
                */
                CL.importedSkins.Clear();
                Debug_SMissions.Log("SubMissions: Searching Custom Corp " + CL.Faction + " Folder for skins...");
                if (CL.GetDirectory(out string directory))
                {
                    string directorySkins = directory + SMissionJSONLoader.up + "Skins";
                    SMissionJSONLoader.ValidateDirectory(directorySkins);
                    List<string> names = GetNameListUnofficial(CL.Faction + SMissionJSONLoader.up + "Skins");
                    Debug_SMissions.Log("SubMissions: Found " + names.Count + " skins...");
                    CL.TexturesCache.Clear();
                    List<CorporationSkinInfo> CSISort = new List<CorporationSkinInfo>();
                    foreach (string name in names)
                    {
                        CorporationSkinInfo CSI = ScriptableObject.CreateInstance<CorporationSkinInfo>();
                        if (LoadSkinForCorp(CL, name, directorySkins + SMissionJSONLoader.up + name, ref CSI))
                        {
                            Debug_SMissions.Log("SubMissions: Added Skin " + name);
                            CL.importedSkins.Add(CSI.m_SkinUniqueID);
                            CSISort.Add(CSI);
                        }
                        else
                            SMUtil.Assert(false, "Could not load Skin " + name);
                    }
                    if (CL.SkinReferenceFaction == FactionSubTypes.NULL && CL.importedSkins.Count() == 1)
                    {
                        string name = names.First();
                        CorporationSkinInfo CSI = ScriptableObject.CreateInstance<CorporationSkinInfo>();
                        if (LoadSkinForCorp(CL, name, directorySkins + SMissionJSONLoader.up + name, ref CSI, true))
                        {
                            Debug_SMissions.Log("SubMissions: Added Skin " + name);
                            CL.importedSkins.Add(CSI.m_SkinUniqueID);
                            CSISort.Add(CSI);
                        }
                        else
                            SMUtil.Assert(false, "Could not load Skin " + name);
                    }
                    CSISort = CSISort.OrderBy(x => x.m_SkinUniqueID).ToList();
                    foreach (var CSIC in CSISort)
                        ManCustomSkins.inst.AddSkinToCorp(CSIC, true);
                    return true;
                }
                return false;
            }
            catch
            {
                SMUtil.Assert(false, "SubMissions: Check your Skins file names!  Some may be invalid!");
                return false;
            }
        }
        private static bool LoadSkinForCorp(SMCCorpLicense CL, string folderName, string destination, ref CorporationSkinInfo CSI, bool stackTemp = false)
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
                        int IDFind = 0;
                        if (CL.SkinReferenceFaction != FactionSubTypes.NULL)
                            IDFind = AdvisedCorpSkinIDStartRef;
                        while (CL.registeredSkins.Contains(IDFind) || CL.importedSkins.Contains(IDFind))//CL.importedSkins.Exists(delegate (CorporationSkinInfo cand) { return cand.m_SkinUniqueID == IDFind; }))
                            IDFind++;
                        SID.UniqueID = IDFind;
                        File.WriteAllText(skinJSON, JsonConvert.SerializeObject(SID, Formatting.Indented));
                        SMUtil.Assert(false, "SubMissions: \nMade a new Skin.json for " + folderName + " with ID " + IDFind + " - use this to set your skin information \n Only change UniqueID if ABSOLUETLY NESSEARY (There can only be one skin to one UniqueID)");
                        SMUtil.PushErrors();
                    }
                    if (stackTemp && CL.importedSkins.Contains(SID.UniqueID))
                    {
                        SID.UniqueID++;
                    }
                    if (SID.UniqueID < 0)
                    {
                        SMUtil.Assert(false, "SubMissions: Skin UniqueID for skin " + folderName + " (is below 1) is out of range!!");
                        return false;
                    }
                    else if (CL.registeredSkins.Contains(SID.UniqueID))
                    {
                        Debug_SMissions.Log("SubMissions: Skin " + folderName + "'s UniqueID was already registered at a referenced skin.  Overwriting the reference!");
                    }
                    if (CL.importedSkins.Contains(SID.UniqueID) )// FindAll(delegate (CorporationSkinInfo cand) { return cand.m_SkinUniqueID == SID.UniqueID; }).Count > 1)
                    {
                        SMUtil.Assert(false, "SubMissions: Skin " + folderName + "'s UniqueID was already registered before by another respective Skin.json corp skin!  Unable to import!");
                        return false;
                    }
                    if (SID.UniqueID < AdvisedCorpSkinIDStartRef && CL.SkinReferenceFaction != FactionSubTypes.NULL)
                    {
                        SMUtil.Assert(false, "SubMissions: Skin UniqueID for skin " + folderName + ".  is below 128, the advised start range for Unofficial Custom Corps with a Skin reference corp. \n  This can cause compatability issues later on with Official and Unofficial mods.");
                    }

                    CorporationSkinUIInfo UII = new CorporationSkinUIInfo();
                    
                    // Setup skins
                    //SkinTextures refs = ManCustomSkins.inst.GetCorpSkinTextureInfos((FactionSubTypes)CL.SkinReferenceFaction).First();
                    SkinTextures ST = new SkinTextures();
                    int SSD = 1024;
                    ST.m_Albedo = TryGetPNG(destination, SID.Albedo);
                    if (ST.m_Albedo.width != SSD || ST.m_Albedo.height != SSD)
                    {
                        TextureScale.Bilinear(ST.m_Albedo, SSD, SSD);
                    }
                    CL.TexturesCache.Add(ST.m_Albedo);
                    ST.m_Metal = TryGetPNG(destination, SID.Metal);
                    if (ST.m_Metal.width != SSD || ST.m_Metal.height != SSD)
                    {
                        TextureScale.Bilinear(ST.m_Metal, SSD, SSD);
                    }
                    CL.TexturesCache.Add(ST.m_Metal);
                    int SSDE = 512;
                    ST.m_Emissive = TryGetPNG(destination, SID.Emissive);
                    if (ST.m_Emissive.width != SSDE || ST.m_Emissive.height != SSDE)
                    {
                        TextureScale.Bilinear(ST.m_Emissive, SSDE, SSDE);
                    }
                    CL.TexturesCache.Add(ST.m_Emissive);

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
                            Debug_SMissions.Log("SubMissions: LoadSkinForCorp(SMCCorpLicense) - RenderTechImage is queued");
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

        internal static void ReloadSkins()
        {
            foreach (var SMCCL in corpsStoredUnofficial)
            {
                if (ManLicenses.inst.GetAllCorpIDs().Contains(SMCCL.Key))
                    SMCCL.Value.BuildSkins();
            }
        }


        // AUDIO
        private const float fadeINRatePreDelay = 0.2f;
        private const float fadeINRate = 12f;
        private const float fadeRate = 0.6f;
        private static float currentVolPercent = 0; // the music from 0 to one
        private static float currentVol = 0;        // the player's settings
        public static bool musicOpening = false;

        public static ManSMCCorps inst;
        internal static FMOD.System sys;
        internal FMOD.ChannelGroup CG;
        internal FMOD.Sound MusicCurrent;
        internal FMOD.Channel ActiveMusic;
        internal int enemyBlockCountCurrent = 0;
        internal int enemyID = int.MinValue;
        public FactionSubTypes faction = FactionSubTypes.NULL;
        private bool isDangerValid = false;
        private bool isPaused = false;
        private bool queuedChange = false;
        private float justAssignedTime = 0;
        public static void Initiate()
        {
            inst = Instantiate(new GameObject("ManSMCCorps")).AddComponent<ManSMCCorps>();
            Debug_SMissions.Log("SubMissions: ManSMCCorps initated");
        }
        public static void Subscribe()
        {
            if (!inst)
            {
                Initiate();
            }
            Singleton.Manager<ManPauseGame>.inst.PauseEvent.Subscribe(inst.OnPause);
        }
        public static void DeInit()
        {
            if (!inst)
                return;
            Singleton.Manager<ManPauseGame>.inst.PauseEvent.Unsubscribe(inst.OnPause);
            Debug_SMissions.Log("SubMissions: ManSMCCorps De-Init");
        }
        public void OnPause(bool paused)
        {
            ManProfile.Profile Prof = ManProfile.inst.GetCurrentUser();
            if (paused)
            {
                try
                {
                    ActiveMusic.isPlaying(out bool currentlyPlaying);
                    if (currentlyPlaying)
                    {
                        ActiveMusic.setPaused(true);
                    }
                    ManMusic.inst.SetMusicMixerVolume(Prof.m_SoundSettings.m_MusicVolume);
                }
                catch { }
            }
            else
            {
                try
                {
                    ActiveMusic.isPlaying(out bool currentlyPlaying);
                    if (currentlyPlaying)
                    {
                        ActiveMusic.setVolume(Prof.m_SoundSettings.m_MusicVolume);
                        ActiveMusic.setPaused(false);
                    }
                    if (isDangerValid)
                        ManMusic.inst.SetMusicMixerVolume(0);
                    else
                        ManMusic.inst.SetMusicMixerVolume(Prof.m_SoundSettings.m_MusicVolume);
                }
                catch { }
            }
            isPaused = paused;
        }


        public static void SetDangerContext(SMCCorpLicense CL, int enemyBlockCount, int enemyVisID)
        {
            inst.SetDangerContextInternal(CL, enemyBlockCount, enemyVisID);
        }
        public void SetDangerContextInternal(SMCCorpLicense CL, int enemyBlockCount, int enemyVisID)
        {
            FactionSubTypes FST = (FactionSubTypes)CL.ID;
            if (queuedChange)
            {
                if (!isDangerValid && faction != FST)
                {
                    MusicCurrent = CL.combatMusicLoaded.GetRandomEntry();
                    isDangerValid = true;
                    faction = FST;
                    enemyBlockCountCurrent = enemyBlockCount;
                    enemyID = enemyVisID;
                    currentVolPercent = 0.01f;
                    ForceReboot();
                    Debug_SMissions.Log("SubMissions: ManSMCCorps Playing danger music (Transition) for " + CL.Faction);
                    ManMusic.inst.SetMusicMixerVolume(0);
                    queuedChange = false;
                }
                else if (musicOpening)
                    queuedChange = false;
            }
            else if ((musicOpening || enemyBlockCount > enemyBlockCountCurrent) && (faction != FST || enemyID != enemyVisID))
            {   // Set the music
                if (isDangerValid)
                {
                    if (!queuedChange)
                    {
                        Debug_SMissions.Log("SubMissions: ManSMCCorps Transitioning danger music...");
                        queuedChange = true;
                    }
                }
                else
                {
                    MusicCurrent = CL.combatMusicLoaded.GetRandomEntry();
                    isDangerValid = true;
                    faction = FST;
                    enemyBlockCountCurrent = enemyBlockCount;
                    enemyID = enemyVisID;
                    currentVolPercent = 0.01f;
                    ForceReboot();
                    Debug_SMissions.Log("SubMissions: ManSMCCorps Playing danger music for " + CL.Faction);
                    ManMusic.inst.SetMusicMixerVolume(0);
                }
            }
            if (faction == FST && !queuedChange)
            {   // Sustain the music
                ManProfile.Profile Prof = ManProfile.inst.GetCurrentUser();
                if (currentVolPercent < 0.03f)
                    currentVolPercent += Time.deltaTime * fadeINRatePreDelay;
                else if (currentVolPercent < 1)
                    currentVolPercent += Time.deltaTime * fadeINRate;
                else
                    currentVolPercent = 1;
                currentVol = Prof.m_SoundSettings.m_MusicVolume;
                justAssignedTime = 3;
            }
        }
        public static void HaltDanger()
        {
            if (!inst)
                return;
            if (inst.isDangerValid && inst.justAssignedTime <= 0)
            {
                inst.isDangerValid = false;
                inst.faction = FactionSubTypes.NULL;
                inst.enemyBlockCountCurrent = 0;
                inst.enemyID = -1;
                inst.ActiveMusic.stop();
                Debug_SMissions.Log("SubMissions: ManSMCCorps Stopping danger music");
                ManProfile.Profile Prof = ManProfile.inst.GetCurrentUser();
                ManMusic.inst.SetMusicMixerVolume(ManPauseGame.inst.IsPaused ? 0 : Prof.m_SoundSettings.m_MusicVolume);
            }
        }

        private void StartSound(FMOD.Sound sound)
        {
            sys.getMasterChannelGroup(out CG);
            if (!isPaused)
            {
                try
                {
                    ManProfile.Profile Prof = ManProfile.inst.GetCurrentUser();
                    sys.playSound(sound, CG, false, out ActiveMusic);
                    ActiveMusic.setVolume(Prof.m_SoundSettings.m_MusicVolume);
                    Debug_SMissions.Log("SubMissions: ManSMCCorps Playing danger music at " + Prof.m_SoundSettings.m_MusicVolume);
                }
                catch { }
            }
        }
        private void ForceReboot()
        {
            sys.getMasterChannelGroup(out CG);
            try
            {
                ActiveMusic.stop();
                StartSound(MusicCurrent);
            }
            catch { }
        }
        private void Update()
        {
            RunAudio();
            if (inst.justAssignedTime < 1f)
            {
                if (currentVolPercent > 0)
                    currentVolPercent -= fadeRate * Time.deltaTime;
            }
            if (justAssignedTime > 0)
                justAssignedTime -= Time.deltaTime;
        }

        private void RunAudio()
        {
            if (isDangerValid)
            {
                ActiveMusic.isPlaying(out bool currentlyPlaying);
                try
                {
                    if (currentlyPlaying)
                    {
                        ActiveMusic.getPosition(out uint pos, FMOD.TIMEUNIT.MS);
                        if (pos == 0)
                        {
                            ForceReboot();
                        }
                        ActiveMusic.setVolume(currentVolPercent * currentVol);
                    }
                    else
                    {
                        ForceReboot();
                    }
                }
                catch { }
                if (currentVolPercent <= 0)
                    HaltDanger();
                return;
            }
            else
            {
                ActiveMusic.stop();
                isPaused = false;
            }
        }
    }
}
