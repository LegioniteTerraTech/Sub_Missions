using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HarmonyLib;

namespace Sub_Missions
{
    internal static class UIPatches
    {
        //skins
        internal static class ManTechMaterialSwapPatches
        {
            internal static Type target = typeof(ManTechMaterialSwap);
            // AddSkinsCorrect - shoehorn in unofficial corps
            private static bool GetMinEmissiveForCorporation_Prefix(ManTechMaterialSwap __instance, ref FactionSubTypes corp, ref float __result)
            {
                int corpIndex = (int)corp;
                if (ManSMCCorps.IsUnofficialSMCCorpLicense(corpIndex))
                {
                    if (ManSMCCorps.TryGetSMCCorpLicense(corpIndex, out SMCCorpLicense CL))
                    {
                        __result = CL.minEmissive;
                        return false;
                    }
                }
                return true;
            }
        }
        internal static class UISkinsPaletteHUDPatches
        {
            internal static Type target = typeof(UISkinsPaletteHUD);

            private static readonly FieldInfo
                imagePre = typeof(UISkinsPaletteHUD).GetField("m_PreviewImage", BindingFlags.NonPublic | BindingFlags.Instance),
                imageIco = typeof(UISkinsPaletteHUD).GetField("m_PreviewImageCorpIcon", BindingFlags.NonPublic | BindingFlags.Instance),
                imageCoT = typeof(UISkinsPaletteHUD).GetField("m_PreviewCorpText", BindingFlags.NonPublic | BindingFlags.Instance),
                imageSkT = typeof(UISkinsPaletteHUD).GetField("m_PreviewSkinText", BindingFlags.NonPublic | BindingFlags.Instance),
                trans = typeof(UISkinsPaletteHUD).GetField("m_CurrentSkinButtons", BindingFlags.NonPublic | BindingFlags.Instance);
            //ProperlySelectSkins
            private static bool SetSelectedSkin_Prefix(UISkinsPaletteHUD __instance, ref CorporationSkinUIInfo info, ref FactionSubTypes corp)
            {
                List<Transform> transs = (List<Transform>)trans.GetValue(__instance);
                if (ManSMCCorps.IsUnofficialSMCCorpLicense(corp))
                {
                    if (transs != null && transs.Count > 0)
                        transs.Last().gameObject.SetActive(false);

                    //there's no corp button, so we do everything BUT that
                    Image IG = (Image)imagePre.GetValue(__instance);
                    IG.sprite = info.m_PreviewImage;
                    Image IG2 = (Image)imageIco.GetValue(__instance);
                    IG2.sprite = Singleton.Manager<ManUI>.inst.GetSelectedCorpIcon(corp);
                    TMP_Text txt = (TMP_Text)imageCoT.GetValue(__instance);
                    txt.text = StringLookup.GetCorporationName(corp);
                    TMP_Text txt2 = (TMP_Text)imageSkT.GetValue(__instance);
                    txt2.text = info.m_FallbackString;
                    return false;
                }
                else
                {
                    if (transs != null && transs.Count > 0)
                        transs.Last().gameObject.SetActive(true);
                }
                return true;
            }


            private static void OnSpawn_Prefix(UISkinsPaletteHUD __instance)
            {
                UICCorpLicenses.ForceAddModdedCorpsSection(__instance);
            }
            private static void OnSpawn_Postfix(UISkinsPaletteHUD __instance)
            {
                UICCorpLicenses.ForceAddModdedCorpsSectionPost(__instance);
            }
        }


        internal static class UILicensesPatches
        {
            internal static Type target = typeof(UILicenses);
            //InitCorrect
            private static void Init_Prefix(UILicenses __instance, ref object context)
            {
                try
                {
                    Dictionary<FactionSubTypes, FactionLicense> dictionary = context as Dictionary<FactionSubTypes, FactionLicense>;

                    Dictionary<FactionSubTypes, FactionLicense> NewDictionary = new Dictionary<FactionSubTypes, FactionLicense>();
                    foreach (KeyValuePair<FactionSubTypes, FactionLicense> pair in dictionary)
                    {
                        if ((int)pair.Key < Enum.GetNames(typeof(FactionSubTypes)).Length)
                            NewDictionary.Add(pair.Key, pair.Value);

                    }
                    context = NewDictionary;
                }
                catch { }
            }
            private static void Init_Postfix()
            {
                try
                {
                    UICCorpLicenses.InitALLFactionLicenseUnofficialUI();
                }
                catch { }
            }
            //DeInitCorrect
            private static void DeInit_Postfix()
            {
                try
                {
                    UICCorpLicenses.InitALLFactionLicenseUnofficialUI();
                }
                catch { }
            }
            //ToggleTheRightInstance
            private static bool ShowCorpLicense_Prefix(UILicenses __instance, ref FactionSubTypes corp)
            {
                if (ManSMCCorps.IsUnofficialSMCCorpLicense(corp))
                {
                    UICCorpLicenses.ShowFactionLicenseUnofficialUI((int)corp);
                    return false;
                }
                else if (ManSMCCorps.IsOfficialSMCCorpLicense((int)corp))
                {
                    UICCorpLicenses.ShowFactionLicenseOfficialUI((int)corp);
                    return false;
                }
                return true;
            }
        }

    }
}
