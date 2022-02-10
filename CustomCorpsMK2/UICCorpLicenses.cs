using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Nuterra.BlockInjector;
using Sub_Missions.ManWindows;
using UnityEngine.UI;

namespace Sub_Missions
{
    public class UICCorpLicenses : MonoBehaviour
    {
        public static Dictionary<int, UICorpLicense> CustomCorpLicenses = new Dictionary<int, UICorpLicense>();

        public static GameObject ModdedCorpsSectionTEMP;

        private static FieldInfo fabs = typeof(UILicenses).GetField("m_LicensePrefab", BindingFlags.NonPublic | BindingFlags.Instance);

        // Faction License Symbols, EXP, and Inventory
        /// <summary>
        /// Adds the License Tab at the upper-left corner of the HUD
        /// </summary>
        /// <param name="CL"></param>
        internal static void MakeFactionLicenseUnofficialUI(SMCCorpLicense CL)
        {   //
            if (CustomCorpLicenses.TryGetValue(CL.ID, out _))
            {
            }
            else
            {
                FactionSubTypes FST = (FactionSubTypes)CL.ID;
                if (!ManSubMissions.Subscribed)
                    return;
                FactionLicense FL = ManLicenses.inst.GetLicense(FST);
                if (FL == null)
                    return;
                UILicenses UIL = (UILicenses)ManHUD.inst.GetHudElement(ManHUD.HUDElementType.FactionLicences);
                bool wasInit = true;
                GameObject GO;
                try
                {
                    GO = (GameObject)fabs.GetValue(UIL);
                }
                catch
                {
                    wasInit = false;
                    ManHUD.inst.SetCurrentHUD(ManHUD.HUDType.MainGame);
                    ManHUD.inst.InitialiseHudElement(ManHUD.HUDElementType.FactionLicences);
                    UIL = (UILicenses)ManHUD.inst.GetHudElement(ManHUD.HUDElementType.FactionLicences);
                    GO = (GameObject)fabs.GetValue(UIL);
                }

                UICorpLicense newLicense = GO.GetComponent<UICorpLicense>().Spawn(UIL.transform);
                newLicense.transform.localPosition = newLicense.transform.localPosition.SetZ(0f);
                newLicense.transform.localScale = Vector3.one;
                newLicense.Init(FL);
                if (ManSaveGame.inst.CurrentState != null)
                {
                    newLicense.Show(ManLicenses.inst.IsLicenseDiscovered(FST));
                }
                else
                    newLicense.Show(false);
                if (!wasInit)
                {
                    ManHUD.inst.HideHudElement(ManHUD.HUDElementType.FactionLicences);
                    ManHUD.inst.DeInitialiseHudElement(ManHUD.HUDElementType.FactionLicences);
                    ManHUD.inst.SetCurrentHUD(ManHUD.HUDType.None);
                }
                CustomCorpLicenses.Add(CL.ID, newLicense);
            }

        }
        internal static void ShowFactionLicenseUnofficialUI(int corp)
        {   //
            if (CustomCorpLicenses.TryGetValue(corp, out UICorpLicense UICL))
            {
                Debug.Log("SubMissions: ShowFactionLicenseUnofficialUI - showing ID " + corp);
                UICL.Show(true);
            }
            else
            {
            }
        }
        internal static void InitALLFactionLicenseUnofficialUI()
        {   //
            ManSMCCorps.InitALLFactionLicenseUnofficialUI();
        }
        internal static void DeInitALLFactionLicenseUnofficialUI()
        {   //
            foreach (KeyValuePair<int, UICorpLicense> pair in CustomCorpLicenses)
            {
                pair.Value.transform.SetParent(null);
                pair.Value.Recycle();
            }
            CustomCorpLicenses.Clear();
        }

        


        private static FieldInfo
            MCSect = typeof(UISkinsPaletteHUD).GetField("m_ModdedCorpsSection", BindingFlags.NonPublic | BindingFlags.Instance),
            CKPrefab = typeof(UISkinsPaletteHUD).GetField("m_ModdedCorpButtonPrefab", BindingFlags.NonPublic | BindingFlags.Instance),
            CKAll = typeof(UISkinsPaletteHUD).GetField("m_CurrentCorpButtons", BindingFlags.NonPublic | BindingFlags.Instance),
            CKPopPanel = typeof(UISkinsPaletteHUD).GetField("m_ModdedCorpFilterHolder", BindingFlags.NonPublic | BindingFlags.Instance),
            CKPar = typeof(UISkinsPaletteHUD).GetField("m_CorpButtonParent", BindingFlags.NonPublic | BindingFlags.Instance),
            CKCorpCount = typeof(UISkinsPaletteHUD).GetField("m_NumModdedCorps", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo
            OCBC = typeof(UISkinsPaletteHUD).GetMethod("OnCorpButtonClicked", BindingFlags.NonPublic | BindingFlags.Instance);

        private static bool requiresNew = false;
        private static int corpsLoadedCache = 0;
        private static UISkinsPaletteHUD paletteInst;
        private static Sprite transparent;
        public static void RemoteInvoke(FactionSubTypes FST)
        {
            OCBC.Invoke(paletteInst, new object[1] { FST });
        }
        // must fire BEFORE OnSpawn() of UISkinsPaletteHUD!
        public static void ForceAddModdedCorpsSection(UISkinsPaletteHUD inst)
        {
            corpsLoadedCache = 0;
            paletteInst = inst;
            if (((GameObject)MCSect.GetValue(inst)) == null && (ManSMCCorps.GetSMCCorpsCount() > 0 || ManMods.inst.GetNumCustomCorps() > 0))
            {
                //GameObject corpToggles = (GameObject)MCSectW.GetValue((UICorpToggles)MCSect2.GetValue((UIPaletteBlockSelect)ManHUD.inst.GetHudElement(ManHUD.HUDElementType.BlockPalette)));
                //Debug.Log("SubMissions: ForceAddModdedCorpsSection - PARENT is: " + (corpToggles.transform.parent ? corpToggles.transform.parent.ToString() : "null"));
                if (!ModdedCorpsSectionTEMP)
                {
                    ModdedCorpsSectionTEMP = new GameObject("ModdedCorpsSectionTEMP");
                    var MCS = ModdedCorpsSectionTEMP.AddComponent<UICustomCorpSkinsTEMP>();
                    MCS.HUDinstInternal = inst;
                    ModdedCorpsSectionTEMP.SetActive(true);
                    requiresNew = true;
                    Debug.Log("SubMissions: ForceAddModdedCorpsSection - INITING NEW...");
                }
                else
                {
                    ModdedCorpsSectionTEMP.GetComponent<UICustomCorpSkinsTEMP>().HUDinstInternal = inst;
                }
                //MCSect.SetValue(inst, ModdedCorpsSectionTEMP);
            }
            else
            {
                Debug.Log("SubMissions: ForceAddModdedCorpsSection - No action required!  It has been implemented!");
                List<SMCCorpLicense> CLs = ManSMCCorps.GetAllSMCCorps();
                Dictionary<FactionSubTypes, Transform> dict = (Dictionary<FactionSubTypes, Transform>)CKAll.GetValue(inst);
                GameObject GO = (GameObject)CKPopPanel.GetValue(inst);
                if (transparent == null)
                {
                    transparent = ManUI.inst.GetAICategoryIcon(AICategories.AIIdle);
                    transparent = Sprite.Create(transparent.texture, new Rect(0, 0, 2, 2), Vector2.zero);
                }
                foreach (SMCCorpLicense CL in CLs)
                {
                    FactionSubTypes FST = (FactionSubTypes)CL.ID;
                    UICustomSkinCorpButton pFab = (UICustomSkinCorpButton)CKPrefab.GetValue(inst);
                    Transform trans = pFab.transform.Spawn(GO.transform);
                    trans.localPosition = trans.localPosition.SetZ(0f);
                    trans.localScale = Vector3.one;
                    dict.Add(FST, trans);
                    UICustomSkinCorpButton UICSCB = trans.GetComponent<UICustomSkinCorpButton>();
                    UICSCB.SetupButton(FST, CL.HighResCorpIcon, CL.SmallCorpIcon);
                    UICSCB.CorpButtonClickedEvent.Subscribe(new Action<FactionSubTypes>(RemoteInvoke));
                    Toggle toggol = UICSCB.GetComponent<Toggle>();
                    if (toggol.IsNotNull())
                        toggol.group = (ToggleGroup)CKPar.GetValue(inst);
                    corpsLoadedCache++;
                }
                CKAll.SetValue(inst, dict);
                Debug.Log("SubMissions: ForceAddModdedCorpsSection - Injected " + dict.Count + " corps");
            }
        }
        // must fire AFTER OnSpawn() of UISkinsPaletteHUD!
        public static void ForceAddModdedCorpsSectionPost(UISkinsPaletteHUD inst)
        {
            //inst.ToggleModdedCorpsPanel();
            CKCorpCount.SetValue(inst, (int)CKCorpCount.GetValue(inst) + corpsLoadedCache);
            GameObject GO = (GameObject)MCSect.GetValue(inst);
            if (GO != null)
            {
                GO.SetActive(corpsLoadedCache > 0);
            }
        }
    }
}
