using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TerraTechETCUtil;
using Sub_Missions.Editor;

namespace Sub_Missions.ManWindows
{
    public class GUISMissionEditor : GUIMiniMenu<GUISMissionEditor>
    {
        public Texture CachedPic;
        public bool LockEnding = false;
        public bool SlowMode = false;
        public bool Paused = false;
        private SubMissionTree Tree = null;
        public SubMissionTree TreeSelected => Tree;

        public override void Setup(GUIDisplayStats stats)
        {
            GUIDisplayStatsLegacy stats2 = (GUIDisplayStatsLegacy)stats;
            ManSubMissions.Editor = this;
        }
        public override void OnOpen()
        {
        }

        public void ExitTree()
        {
            ManSubMissions.Selected = null;
            Tree = null;
        }
        public bool DisplayHierachy()
        {
            bool interacted = false;
            GUILayout.BeginHorizontal(GUILayout.Height(32));
            try
            {
                if (Tree != null)
                {
                    if (GUILayout.Button("Path: ", AltUI.ButtonBlue))
                    {
                        Tree = null;
                        ManSubMissions.Selected = null;
                        interacted = true;
                    }
                    else
                    {
                        GUILayout.Label(">", AltUI.TextfieldBordered);
                        if (ManSubMissions.Selected != null)
                        {
                            if (GUILayout.Button(Tree.TreeHierachy.AddressName, WindowManager.styleBorderedFont))
                            {
                                ManSubMissions.Selected = null;
                                interacted = true;
                            }
                            else
                            {
                                GUILayout.Label(">", AltUI.TextfieldBordered);
                                GUILayout.Button(ManSubMissions.Selected.Name, AltUI.ButtonGrey);
                            }
                        }
                        else
                            GUILayout.Button(Tree.TreeHierachy.AddressName, AltUI.ButtonGrey);
                    }
                }
                else
                    GUILayout.Button("Path: ", AltUI.ButtonGrey);

            }
            catch (ExitGUIException e) { throw e; }
            catch (Exception e)
            {
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
                throw new MandatoryException("Error in GUISMissionEditor.DisplayHierachy()", e);
            }
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
            return interacted;
        }

        public override void RunGUI(int ID)
        {
            try
            {
                if (ManSubMissions.Selected != null)
                    GUIMission();
                else if (Tree != null)
                    GUITree();
                else
                    GUIMain();
            }
            catch (ExitGUIException e) { throw e; }
            catch (Exception e)
            {
                GUI.DragWindow();
                WindowManager.KeepWithinScreenBoundsNonStrict(Display);
                throw new MandatoryException("Critical Error in GUISMissionEditor.RunGUI()", e);
            }
            GUI.DragWindow();
            WindowManager.KeepWithinScreenBoundsNonStrict(Display);
        }

        public void GUIMission()
        {
            if (ManSubMissions.Selected.ActiveState <= SubMissionLoadState.NeedsFirstInit)
                ManSubMissions.Selected.ForceInitiateImmedeate();
            Tree = ManSubMissions.Selected.Tree;

            if (SMMissionGUI.ShowGUITopBar(this) && SMMissionGUI.ShowGUI(ManSubMissions.Selected))
                LockEnding = true;
        }


        public void GUITree()
        {
            try
            {
                SMTreeGUI.ShowGUITopBar(this);
            }
            catch (ExitGUIException e) { throw e; }
            catch (Exception e)
            {
                throw new MandatoryException("Error in SMTreeGUI.ShowGUITopBar()", e);
            }

            try
            {
                SMTreeGUI.ShowGUI();
            }
            catch (ExitGUIException e) { throw e; }
            catch (Exception e)
            {
                Tree = null;
                throw new MandatoryException("Error in SMTreeGUI.ShowGUI()", e);
            }
        }

        private static bool addTree = false;
        private static bool validNew = false;
        private static bool delta = false;
        private static string newTree = "";
        public void GUIMain()
        {
            GUILayout.BeginHorizontal(GUILayout.Height(64));
            GUILayout.Label("Trees Loaded: ");
            GUILayout.Label(ManSubMissions.SubMissionTrees.Count.ToString());
            GUILayout.FlexibleSpace();
            if (ActiveGameInterop.inst && ActiveGameInterop.IsReady)
            {
                if (GUILayout.Button("Unhook from UnityEditor", AltUI.ButtonBlue))
                    ActiveGameInterop.DeInitBothEnds();
            }
            else if (ActiveGameInterop.inst)
            {
                if (GUILayout.Button("Waiting For UnityEditor...", AltUI.ButtonRed))
                    ActiveGameInterop.DeInitJustThisSide();
            }
            else if (GUILayout.Button("Try Hook To UnityEditor", AltUI.ButtonBlue))
            {
                ActiveGameInterop.Init();
                InvokeHelper.InvokeSingleRepeat(ActiveGameInterop.UpdateNow, 1);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Height(64));
            if (GUILayout.Button("New Tree", AltUI.ButtonBlueLarge))
            {
                addTree = true;
            }
            if (GUILayout.Button("Reload Trees", AltUI.ButtonOrangeLarge))
            {
                ManSubMissions.inst.ReloadAllMissionTrees();
            }
            if (GUILayout.Button("Reset ALL", AltUI.ButtonOrangeLarge))
                ManSubMissions.RecycleAllDataForMissionsAndRefresh();

            GUILayout.EndHorizontal();

            try
            {
                if (!DisplayHierachy())
                {
                    if (addTree)
                    {
                        if (delta || SMAutoFill.AutoTextField("Name", ref newTree, 64))
                        {
                            delta = false;
                            if (!newTree.NullOrEmpty() && newTree.Length > 3 &&
                                ManSubMissions.SubMissionTrees.Exists(x => x.TreeName == newTree))
                                validNew = false;
                            else
                                validNew = true;
                        }
                        if (validNew)
                        {
                            if (GUILayout.Button("Create", AltUI.ButtonOrangeLarge, GUILayout.Height(128)))
                            {
                                validNew = false;
                                var SMT = new SubMissionTree()
                                {
                                    TreeName = newTree,
                                };
                                SMissionJSONLoader.SaveTree(SMT);
                            }
                        }
                        else if (GUILayout.Button("Create", AltUI.ButtonGreyLarge, GUILayout.Height(128))) { }
                    }
                    else
                    {
                        foreach (var item in ManSubMissions.SubMissionTrees)
                        {
                            GUILayout.BeginHorizontal(GUILayout.Height(64));
                            if (GUILayout.Button(item.TreeName, AltUI.ButtonBlueLarge))
                            {
                                Tree = item;
                                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.AcceptMission);
                            }
                            GUILayout.Label(item.Faction);
                            GUILayout.Label("|");
                            GUILayout.Label(item.Missions.Count.ToString());
                            GUILayout.FlexibleSpace();

                            GUILayout.EndHorizontal();
                        }
                    }
                }
                GUILayout.FlexibleSpace();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Open Files", AltUI.ButtonBlue))
                    SMissionJSONLoader.OpenInExplorer(SMissionJSONLoader.MissionsDirectory);
                if (ManTerraformTool.tool.UtilityShown)
                {
                    if (GUILayout.Button("Close Terraformer", AltUI.ButtonGreen))
                    {
                        ManTerraformTool.tool.ToggleGUIDisplay();
                    }
                }
                else if (GUILayout.Button("Open Terraformer", AltUI.ButtonBlue))
                {
                    ManTerraformTool.tool.ToggleGUIDisplay();
                }
                GUILayout.EndHorizontal();
            }
            catch (ExitGUIException e) { throw e; }
            catch (Exception e)
            {
                Tree = null;
                throw new Exception("Error in GUIMissionEditor.GUIMain()", e);
            }
        }



        public override void DelayedUpdate()
        {
            BuildStatus();
        }
        public override void FastUpdate()
        {
            this.UpdateTransparency(0.4f);
            SMMissionGUI.GUIWorldDisplayUpdate();
        }

        public void BuildStatus()
        {
        }
        public override void OnRemoval() { }
    }
}
