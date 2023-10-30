using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sub_Missions.ManWindows;
using UnityEngine;

namespace Sub_Missions
{
    /// <summary>
    /// Fallback for when custom skins wasn't implemented.  Now Obsolete as it JUST came out
    /// </summary>
    public class UICustomCorpSkinsTEMP : MonoBehaviour
    {
        private static FieldInfo
            MCSect2 = typeof(UISkinsPaletteHUD).GetField("m_SkinsController", BindingFlags.NonPublic | BindingFlags.Instance);

        public UISkinsPaletteHUD HUDinst { 
            get 
            {
                if (HUDinstInternal == null)
                    HUDinstInternal = (UISkinsPaletteHUD)ManHUD.inst.GetHudElement(ManHUD.HUDElementType.SkinsPalette);
                return HUDinstInternal;
            } 
        }
        public UISkinsPaletteHUD HUDinstInternal;
        public int ID = WindowManager.IDOffset - 1;
        public string context = "Custom Corps Skins";
        public bool isOpen = false;
        public Rect Window = new Rect(WindowManager.WideWindow);   // the "window"

        private Vector2 scrolll = new Vector2(0, 0);
        public float scrolllSize = 50;
        private IntVector2 picScale = new Vector2(64, 64);
        private FactionSubTypes selected = FactionSubTypes.GSO;

        private const int buttonSideSpacing = 40;

        private void OnGUI()
        {
            //Debug_SMissions.Log("SubMissions: UICustomCorpSkinsTEMP - ACTIVE: " + (bool)HUDinst + ((bool)HUDinst ? " - Expanded: " + HUDinst.IsShowing : "not expanded"));

            if (HUDinst && HUDinst.IsShowing && !ManPauseGame.inst.IsPaused)
                Window = GUI.Window(ID, Window, RunGUI, isOpen ? context : "");
        }

        public void RunGUI(int ID)
        {
            //Debug_SMissions.Log("SubMissions: UICustomCorpSkinsTEMP - GUIACTIVE");
            try
            {
                if (!isOpen)
                {
                    Window.width = 65;
                    Window.height = 45;
                    Window.x = Display.main.renderingWidth - Window.width;
                    Window.y = Display.main.renderingHeight - Window.height;

                    if (GUI.Button(new Rect(0, 0, 65, 45), "Show", WindowManager.styleButtonHugeFont))
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
                        isOpen = true;
                    }
                }
                else
                {
                    Window.width = 380;
                    Window.height = 380;
                    Window.x = Display.main.renderingWidth - Window.width;
                    Window.y = Display.main.renderingHeight - Window.height;
                    int lerpMax = (int)((Window.width - buttonSideSpacing - 100) / picScale.x);

                    if (GUI.Button(new Rect(Window.width - 40, 0, 40, Window.height), "<b>H\ni\nd\ne</b>", WindowManager.styleButtonHugeFont))
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
                        isOpen = false;
                    }
                    int posLerp = 0;
                    scrolll = GUI.BeginScrollView(new Rect(20, 20, Window.width - buttonSideSpacing - 40, Window.height - 40), scrolll, new Rect(0, 0, Window.width - buttonSideSpacing - 40, scrolllSize));
                    IEnumerable<SMCCorpLicense> licenses = ManSMCCorps.GetAllSMCCorps();
                    int posXLerp = 10;
                    int posYLerp = 10;
                    if (licenses.Count() > 0)
                    {
                        foreach (SMCCorpLicense CL in licenses)
                        {
                            try
                            {
                                CL.RefreshCorpUISP();
                                FactionSubTypes FST = (FactionSubTypes)CL.ID;
                                if (CL.registeredSkins.Count == 0)
                                    continue;
                                posXLerp = ((int)(posLerp % lerpMax) * picScale.x);
                                posYLerp = ((int)(posLerp / lerpMax) * picScale.y) + 10;
                                Sprite disp;
                                if (selected == FST)
                                {
                                    if (CL.SmallSelectedCorpIcon != null)
                                        disp = ManUI.inst.GetSelectedCorpIcon(FST);
                                    else
                                        disp = ManUI.inst.GetSelectedCorpIcon(FactionSubTypes.GSO);
                                }
                                else
                                {
                                    if (CL.SmallCorpIcon != null)
                                        disp = ManUI.inst.GetCorpIcon(FST);
                                    else
                                        disp = ManUI.inst.GetCorpIcon(FactionSubTypes.GSO);
                                }
                                if (GUI.Button(new Rect(posXLerp, posYLerp, picScale.x, picScale.y), disp.texture))
                                {
                                    selected = FST;
                                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
                                    UISkinsPaletteController PC = (UISkinsPaletteController)MCSect2.GetValue(HUDinst);
                                    if (PC)
                                    {
                                        PC.SetCurrentSelectedCorp(FST);
                                        Debug_SMissions.Log("SubMissions: UICustomCorpSkinsTEMP - SetCurrentSelectedCorp " + (int)PC.GetCurrentSelectedCorp() + "... CustomSkins - " + (int)Singleton.Manager<ManCustomSkins>.inst.GetCurrentSelectedCorp());
                                    }
                                }
                                posLerp++;
                            }
                            catch (Exception e)
                            {
                                Debug_SMissions.Log("SubMissions: UICustomCorpSkinsTEMP - SetCurrentSelectedCorp ERROR - " + e);
                            }
                        }
                    }
                    GUI.EndScrollView();
                    scrolllSize = posYLerp;
                }
            }
            catch (Exception e)
            {
                Debug_SMissions.Log("SubMissions: UICustomCorpSkinsTEMP(FULL) - SetCurrentSelectedCorp ERROR - " + e);
            }
        }
    }
}
