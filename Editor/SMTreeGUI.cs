using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TerraTechETCUtil;
using Sub_Missions.ManWindows;
using Sub_Missions.ModularMonuments;

namespace Sub_Missions.Editor
{
    internal class SMTreeGUI
    {
        //1000 is width of editor window
        internal const int LeftBarSize = 300;
        internal const int RightDisplaySize = WindowManager.LargeWindowWidth - LeftBarSize;

        internal static bool ShowGUITopBar(GUISMissionEditor editorIn)
        {
            bool quit = false;
            editor = editorIn;
            TreeChange(editorIn.TreeSelected);

            GUILayout.BeginHorizontal(GUILayout.Height(64));
            GUILayout.Label("Selected Tree ID: ");
            GUILayout.Label(tree.TreeName == null ? "Loading..." : tree.TreeName);
            GUILayout.FlexibleSpace();
            if (editor.Paused)
            {
                if (GUILayout.Button("Save", AltUI.ButtonBlueLarge, GUILayout.Width(120)))
                {
                    SMissionJSONLoader.SaveTree(tree);
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.InfoOpen);
                }
            }
            else if (GUILayout.Button("Save", AltUI.ButtonGrey, GUILayout.Width(120)))
            {
                editor.Paused = true;
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.PopUpClose);
            }

            if (GUILayout.Button("Load", AltUI.ButtonOrangeLarge, GUILayout.Width(120)))
            {
                ManSubMissions.Selected = null;
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.InfoClose);
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("", AltUI.TextfieldBordered, GUILayout.Width(10));
                if (editor.Paused)
                {
                    if (GUILayout.Button("Paused", AltUI.ButtonGrey, GUILayout.Width(75)))
                    {
                        editor.Paused = false;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.PopUpOpen);
                    }
                }
                else if (GUILayout.Button("Play", AltUI.ButtonBlue, GUILayout.Width(75)))
                {
                    editor.Paused = true;
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.PopUpClose);
                }
                if (editor.SlowMode)
                {
                    if (GUILayout.Button("Slow", AltUI.ButtonBlue, GUILayout.Width(60)))
                    {
                        editor.SlowMode = false;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.PopUpOpen);
                    }
                }
                else if (GUILayout.Button("Slow", AltUI.ButtonGrey, GUILayout.Width(60)))
                {
                    editor.SlowMode = true;
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.PopUpClose);
                }

                if (ManSubMissions.Selected != null)
                {
                    if (GUILayout.Button("Reset", AltUI.ButtonRed, GUILayout.Width(60)))
                        ManSubMissions.Selected.Reboot(true);
                    if (GUILayout.Button("Restart", AltUI.ButtonRed, GUILayout.Width(80)))
                        ManSubMissions.Selected.Reboot();
                }
                else
                {
                    GUILayout.Button("Reset", AltUI.ButtonGrey, GUILayout.Width(60));
                    GUILayout.Button("Restart", AltUI.ButtonGrey, GUILayout.Width(80));
                }
                if (editor.LockEnding)
                {
                    if (GUILayout.Button("End", AltUI.ButtonGrey, GUILayout.Width(60)))
                    {
                        editor.LockEnding = false;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                    }
                }
                else if (GUILayout.Button("End", AltUI.ButtonBlue, GUILayout.Width(60)))
                {
                    editor.LockEnding = false;
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Back);
                }
                if (GUILayout.Button("Log", AltUI.ButtonBlue, GUILayout.Width(60)))
                {
                    SMUtil.PushErrors();
                }
                if (GUILayout.Button("Exit", AltUI.ButtonRed, GUILayout.Width(60)))
                {
                    editor.ExitTree();
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.InfoClose);
                }
                GUILayout.EndHorizontal();


                if (!editor.DisplayHierachy())
                {
                    quit = true;
                }
            }
            return quit;
        }


        private static GUISMissionEditor editor = null;
        private static SubMissionTree tree = null;

        internal static bool ShowGUI()
        {
            bool interacted = false;

            if (tree != null)
            {
                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical(GUILayout.Width(LeftBarSize));
                try
                {
                    GUILeftBar();
                }
                catch (ExitGUIException e) { throw e; }
                catch (Exception e)
                {
                    tree = null;
                    //GUIUtility.ExitGUI();
                    throw new MandatoryException("Error in SMTreeGUI.GUILeftBar()", e);
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                try
                {
                    GUIRightDisplay();
                }
                catch (ExitGUIException e) { throw e; }
                catch (Exception e)
                {
                    tree = null;
                    //GUIUtility.ExitGUI();
                    throw new MandatoryException("Error in SMTreeGUI.GUITreeFiles()", e);
                }
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }
            else
                GUILayout.Label("No Mission Tree Selected error");
            return interacted;
        }

        private static void TreeChange(SubMissionTree treeIn)
        {
            if (tree == treeIn)
                return;
            tree = treeIn;
            OnTreeChange();
        }
        private static void OnTreeChange()
        {
        }


        internal enum TreeGUIType
        {
            Main,
            Missions,
            Techs,
            Monuments,
        }

        private static string LBarTitle = "";
        private static TreeGUIType guiType = TreeGUIType.Main;
        private static void GUILeftBar()
        {
            GUILayout.Label(LBarTitle, AltUI.WindowHeaderBlue, GUILayout.Height(32));
            if (guiType != TreeGUIType.Main && GUILayout.Button("Back", AltUI.ButtonRed, GUILayout.Height(32)))
            {
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Back);
                guiType = TreeGUIType.Main;
            }
            else
            {
                switch (guiType)
                {
                    case TreeGUIType.Main:
                        GUIListTree();
                        break;
                    case TreeGUIType.Missions:
                        GUIListMissions();
                        break;
                    case TreeGUIType.Techs:
                        GUIListTechs();
                        break;
                    case TreeGUIType.Monuments:
                        GUIListWObjects();
                        break;
                    default:
                        break;
                }
            }
        }

        private static string RDispTitle = "";
        private static void GUIRightDisplay()
        {
            GUILayout.Label(RDispTitle, AltUI.WindowHeaderBlue, GUILayout.Height(32));
            switch (guiType)
            {
                case TreeGUIType.Main:
                    GUIDisplayTree();
                    break;
                case TreeGUIType.Missions:
                    GUIDisplayMission();
                    break;
                case TreeGUIType.Techs:
                    GUIDisplayTech();
                    break;
                case TreeGUIType.Monuments:
                    GUIDisplayWObject();
                    break;
                default:
                    guiType = TreeGUIType.Main;
                    break;
            }
        }

        // The Tree
        private static bool validNew = false;
        private static bool delta = false;
        private static void GUIListTree()
        {
            LBarTitle = "Tree Contents:";

            if (GUILayout.Button("Missions", AltUI.ButtonRed))
            {
                validNew = false;
                delta = true;
                guiType = TreeGUIType.Missions;
            }
            if (GUILayout.Button("Techs", AltUI.ButtonRed))
            {
                validNew = false;
                delta = true;
                guiType = TreeGUIType.Techs;
            }
            if (GUILayout.Button("Monuments", AltUI.ButtonRed))
            {
                validNew = false;
                delta = true;
                guiType = TreeGUIType.Monuments;
            }
        }
        private static Vector2 scrollDragD = Vector2.zero;
        private static void GUIDisplayTree()
        {
            RDispTitle = tree.TreeName;
            //GUIListMissionsList();
        }

        // Missions
        private static Vector2 scrollDrag2 = Vector2.zero;
        private static void GUIListMissionsList()
        {
            scrollDrag2 = GUILayout.BeginScrollView(scrollDrag2);
            SubMissionType prev = (SubMissionType)(-1);

            foreach (var item in tree.Missions.OrderBy(x => x.Type))
            {
                if (prev != item.Type)
                {
                    GUILayout.Label(item.Type.ToString(), AltUI.ButtonGrey, GUILayout.Height(32));
                    prev = item.Type;
                }
                GUILayout.BeginHorizontal(GUILayout.Height(32));
                var active = ManSubMissions.activeSubMissions.Find(x => x.Name == item.Name);
                if (active != null)
                {
                    if (GUILayout.Button(item.Name, AltUI.ButtonGreen))
                    {
                        ManSubMissions.Selected = active;
                        addNewMission = false;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Open);
                    }
                }
                else
                {
                    if (GUILayout.Button(item.Name, AltUI.ButtonRed))
                    {
                        if (item.tree.AcceptTreeMission(item))
                        {
                            active = ManSubMissions.activeSubMissions.Find(x => x.Name == item.Name);
                            if (active != null)
                            {
                                ManSubMissions.Selected = active;
                                addNewMission = false;
                                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.AcceptMission);
                            }
                            else
                                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.MissionFailed);
                        }
                        else
                            ManSFX.inst.PlayUISFX(ManSFX.UISfxType.AnchorFailed);
                    }
                }
                GUILayout.Label(item.Faction);
                GUILayout.Label("|");
                GUILayout.Label(item.GradeRequired.ToString());
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
        
        private static SubMission selectedMission = null;
        private static bool addNewMission = false;
        private static void GUIListMissions()
        {
            LBarTitle = "Missions:";

            SMAutoFill.OneWayButton("Add New", ref addNewMission);

            GUIListMissionsList();
        }

        private static void GUIDisplayMission()
        {
            if (tree != null)
            {
                try
                {
                    if (addNewMission)
                        GUIDisplayNewMission();
                    else
                        GUIDisplayExistingMission();
                }
                catch (ExitGUIException e) { throw e; }
                catch (Exception e)
                {
                    selectedMission = null;
                    //GUIUtility.ExitGUI();
                    throw new Exception("Error in GUIDisplayMission()", e);
                }
            }
            else
            {
                RDispTitle = "Not Selected";
            }
        }

        private static string nameMission = "";
        private static void GUIDisplayNewMission()
        {
            if (delta || SMAutoFill.AutoTextField("Name", ref nameMission, 64))
            {
                delta = false;
                if (nameMission.NullOrEmpty() || nameMission.Length < 3 || 
                    tree.Missions.Exists(x => x.Name == nameMission))
                    validNew = false;
                else
                    validNew = true;
            }
            if (validNew)
            {
                if (GUILayout.Button("Create", AltUI.ButtonOrangeLarge, GUILayout.Height(128)))
                {
                    validNew = false;
                    SubMission mission = new SubMission()
                    {
                        Name = nameMission,
                        Tree = tree,
                        Description = "TRANSMISSION FAILED",
                        GradeRequired = 1,
                        Faction = "GSO",
                        SetStartingPosition = Vector3.zero,
                        Type = SubMissionType.Basic,
                        CannotCancel = false,
                        SpawnPosition = SubMissionPosition.CloseToPlayer,
                        ClearTechsOnClear = true,
                        VarTrueFalse = new List<bool>(),
                        VarInts = new List<int>(),
                        EventList = new List<Steps.SubMissionStep>(),
                        TrackedTechs = new List<TrackedTech>(),
                        ActiveState = SubMissionLoadState.NotAvail,
                        CheckList = new List<MissionChecklist>(),
                    };
                    SMissionJSONLoader.SaveNewMission(tree, mission);
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                }
            }
            else if (GUILayout.Button("Create", AltUI.ButtonGreyLarge, GUILayout.Height(128))) { }
        }
        private static void GUIDisplayExistingMission()
        {
            if (selectedMission != null)
            {
                if (ManSubMissions.activeSubMissions.Contains(selectedMission))
                {
                    if (GUILayout.Button("Open Editor", AltUI.ButtonBlueLarge, GUILayout.Height(128)))
                    {
                        ManSubMissions.Selected = selectedMission;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                    }
                }
                else if (GUILayout.Button("Inititate", AltUI.ButtonGreyLarge, GUILayout.Height(128)))
                {
                    ManSubMissions.Selected = selectedMission;
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                }
            }
            else
                RDispTitle = "Not Selected";
        }



        // Techs
        private static Vector2 scrollDrag3 = Vector2.zero;
        private static SpawnableTech tech = null;
        private static string techName = "";
        private static Texture2D techImage = null;
        private static bool addNewTech = false;
        private static void GUIListTechs()
        {
            LBarTitle = "Techs:";

            SMAutoFill.OneWayButton("Add New", ref addNewTech);

            scrollDrag3 = GUILayout.BeginScrollView(scrollDrag3);
            SubMissionType prev = (SubMissionType)(-1);

            foreach (var itemKey in tree.TreeTechs.OrderBy(x => x.Key))
            {
                var item = itemKey.Value;
                GUILayout.BeginHorizontal(GUILayout.Height(32));
                if (item.name == techName)
                {
                    GUILayout.Button(item.name, AltUI.ButtonBlueActive);
                }
                else if (GUILayout.Button(item.name, AltUI.ButtonBlue) && item.name != techName)
                {
                    tech = item;
                    techName = item.name;
                    techImage = item.GetTexture(tree);
                    addNewTech = false;
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Open);
                }
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        private static void GUIDisplayTech()
        {
            if (tree != null)
            {
                try
                {
                    if (addNewTech)
                        GUIDisplayNewTech();
                    else
                        GUIDisplayExistingTech();
                }
                catch (ExitGUIException e) { throw e; }
                catch (Exception e)
                {
                    tech = null;
                    throw new Exception("Error in GUIDisplayTech()", e);
                }
            }
        }
        private static string newTech = "";
        private static void GUIDisplayNewTech()
        {
            var newTech2 = SMUtil.GUIDisplaySelectTech();
            if (delta || (newTech != newTech2))
            {
                delta = false;
                newTech = newTech2;
                if (newTech.NullOrEmpty() || tree.TreeTechs.Keys.Any(x => x == newTech))
                    validNew = false;
                else
                    validNew = true;
            }
            GUILayout.Label("Make sure your tech is posted to the Steam Workshop first!");
            if (validNew)
            {
                if (GUILayout.Button("Attach Snap", AltUI.ButtonOrangeLarge, GUILayout.Height(128)))
                {
                    SMissionJSONLoader.SaveNewTechSnapshot(tree, newTech, SMUtil.DisplaySelectTechImage);
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Snapshot);
                    validNew = false;
                }
            }
            else if (GUILayout.Button("Attach Snap", AltUI.ButtonGreyLarge, GUILayout.Height(128))) { }
            if (validNew)
            {
                if (GUILayout.Button("Attach Raw", AltUI.ButtonOrangeLarge, GUILayout.Height(128)))
                {
                    if (ManScreenshot.TryDecodeSnapshotRender(SMUtil.DisplaySelectTechImage, out var TechData))
                    {
                        SMissionJSONLoader.SaveNewTechRaw(tree, newTech, new RawTechTemplate(TechData.CreateTechData()).savedTech);
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Snapshot);
                    }
                    validNew = false;
                }
            }
            else if (GUILayout.Button("Attach Raw", AltUI.ButtonGreyLarge, GUILayout.Height(128))) { }
        }
        
        private static void GUIDisplayExistingTech()
        {
            if (tech != null)
            {
                GUILayout.Label(tech.name, GUILayout.Width(300));
                GUILayout.Box(techImage, AltUI.TextfieldBorderedBlue);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove", AltUI.ButtonRed))
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.SendToInventory);
                }
            }
            else
                RDispTitle = "Not Selected";
        }


        // ModularMonuments
        private static Vector2 scrollDrag4 = Vector2.zero;
        private static SMWorldObject wObject = null;
        private static string wObjectName = "";
        private static bool addWObject = false;
        private static void GUIListWObjects()
        {
            LBarTitle = "Pieces:";

            SMAutoFill.OneWayButton("Add New", ref addWObject);

            scrollDrag4 = GUILayout.BeginScrollView(scrollDrag4);
            SubMissionType prev = (SubMissionType)(-1);

            foreach (var itemKey in tree.WorldObjects.OrderBy(x => x.Key))
            {
                var item = itemKey.Value;
                GUILayout.BeginHorizontal(GUILayout.Height(32));
                if (item.name == techName)
                {
                    GUILayout.Button(item.name, AltUI.ButtonBlueActive);
                }
                else if (GUILayout.Button(item.name, AltUI.ButtonBlue) && item.name != wObjectName)
                {
                    wObject = item;
                    wObjectName = item.name;
                    addWObject = false;
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Open);
                }
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        private static void GUIDisplayWObject()
        {
            if (tree != null)
            {
                try
                {
                    if (addWObject)
                        GUIDisplayNewWObject();
                    else
                        GUIDisplayExistingWObject();
                }
                catch (ExitGUIException e) { throw e; }
                catch (Exception e)
                {
                    tech = null;
                    throw new Exception("Error in GUIDisplayWObject()", e);
                }
            }
            else
            {
                RDispTitle = "Not Selected";
            }
        }
        private static string newWObject = "";
        private static void GUIDisplayNewWObject()
        {
            if (delta || SMAutoFill.AutoTextField("Name", ref newWObject, 64))
            {
                delta = false;
                if (newWObject.NullOrEmpty() || newWObject.Length > 3 ||
                    tree.WorldObjects.Values.Any(x => x.Name == newWObject))
                    validNew = false;
                else
                    validNew = true;
            }
            if (validNew)
            {
                if (GUILayout.Button("Create", AltUI.ButtonOrangeLarge, GUILayout.Height(128)))
                {
                    validNew = false;
                    var SMWO = new SMWorldObjectJSON()
                    {
                        Name = newWObject,
                        GameMaterialName = "AncientRuins",
                        VisualMeshName = "",
                        aboveGround = true,
                        WorldObjectJSON = new Dictionary<string, object>(),
                    };
                    SMissionJSONLoader.SaveNewSMWorldObject(tree, SMWO);
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Craft);
                }
            }
            else if (GUILayout.Button("Create", AltUI.ButtonGreyLarge, GUILayout.Height(128))) { }
        }

        private static void GUIDisplayExistingWObject()
        {
            if (wObject != null)
            {
                GUILayout.Label(wObject.Name);
                //GUILayout.Box(techImage, AltUI.TextfieldBorderedBlue);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove", AltUI.ButtonRed))
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.SendToInventory);
                }
            }
            else
                RDispTitle = "Not Selected";
        }
    }
}
