using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TerraTechETCUtil;
using Sub_Missions.ManWindows;
using Sub_Missions.ModularMonuments;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Newtonsoft.Json;
using TAC_AI.Templates;
using static BlockPlacementCollector.Collection;
using FMOD;
using System.Reflection;
using static CameraManager.Camera;
using Newtonsoft.Json.Linq;

namespace Sub_Missions.Editor
{
    internal class SMTreeGUI
    {
        //1000 is width of editor window
        internal const int LeftBarSize = 300;
        internal const int RightDisplaySize = ManModGUI.LargeWindowWidth - LeftBarSize;

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
                if (GUILayout.Button("Save", AltUI.ButtonGreen, GUILayout.Width(60)))
                {
                    SMissionJSONLoader.SaveTree(tree);
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.InfoOpen);
                }
            }
            else if (GUILayout.Button("Save", AltUI.ButtonGrey, GUILayout.Width(60)))
            {
                editor.Paused = true;
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.PopUpClose);
            }

            if (GUILayout.Button("Load", AltUI.ButtonBlue, GUILayout.Width(60)))
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
                if (GUILayout.Button("Log", SMUtil.collectedErrors ? AltUI.ButtonRed :
                    SMUtil.collectedLogs ? AltUI.ButtonGreen : SMUtil.collectedInfos ? 
                    AltUI.ButtonBlue : AltUI.ButtonGrey, GUILayout.Width(60)))
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
            NULL,
            Main,
            Missions,
            Techs,
            Monuments,
            TechSelect,
            MonumentsSelect,
        }

        private static string LBarTitle = "";
        private static TreeGUIType guiType = TreeGUIType.Main;
        private static TreeGUIType guiTypeCache = TreeGUIType.NULL;
        private static Action actReturn;
        private static bool PlayerOption = false;
        internal static void JumpToTechSelector(bool showPlayerAsOption, Action callbackReturn)
        {
            if (callbackReturn == null)
                throw new NullReferenceException("callbackReturn cannot be null");
            PlayerOption = showPlayerAsOption;
            if (guiTypeCache == TreeGUIType.NULL)
            {
                SelectorIndex = -1;
                actReturn = () =>
                {
                    guiType = guiTypeCache;
                    guiTypeCache = TreeGUIType.NULL;
                    callbackReturn();
                };
                guiTypeCache = guiType;
                guiType = TreeGUIType.TechSelect;
            }
        }
        internal static void JumpToTechSelector(bool showPlayerAsOption) => JumpToTechSelector(showPlayerAsOption, () => { });
        internal static void JumpToMMSelector(Action callbackReturn)
        {
            if (callbackReturn == null)
                throw new NullReferenceException("callbackReturn cannot be null");
            PlayerOption = false;
            if (guiTypeCache == TreeGUIType.NULL)
            {
                SelectorIndex = -1;
                actReturn = () =>
                {
                    guiType = guiTypeCache;
                    guiTypeCache = TreeGUIType.NULL;
                    callbackReturn();
                };
                guiTypeCache = guiType;
                guiType = TreeGUIType.MonumentsSelect;
            }
        }
        internal static void JumpToMMSelector() => JumpToMMSelector(() => { });
        private static void GUILeftBar()
        {
            GUILayout.Label(LBarTitle, AltUI.WindowHeaderBlue, GUILayout.Height(32));
            if (guiType != TreeGUIType.Main && GUILayout.Button("Back", AltUI.ButtonRed, GUILayout.Height(32)))
            {
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Back);
                if (actReturn != null)
                {
                    actReturn.Invoke();
                    actReturn = null;
                }
                else
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
                    case TreeGUIType.TechSelect:
                        GUIListTechSelect();
                        break;
                    case TreeGUIType.MonumentsSelect:
                        GUIListMMSelect();
                        break;
                }
            }
        }

        private static string RDispTitle = "";
        private static Vector2 pagePosT = Vector2.zero;
        private static void GUIRightDisplay()
        {
            GUILayout.Label(RDispTitle, AltUI.WindowHeaderBlue, GUILayout.Height(32));
            switch (guiType)
            {
                case TreeGUIType.Main:
                    pagePosT = GUILayout.BeginScrollView(pagePosT);
                    GUIDisplayTree();
                    GUILayout.EndScrollView();
                    break;
                case TreeGUIType.Missions:
                    pagePosT = GUILayout.BeginScrollView(pagePosT);
                    GUIDisplayMission();
                    GUILayout.EndScrollView();
                    break;
                case TreeGUIType.Techs:
                    pagePosT = GUILayout.BeginScrollView(pagePosT);
                    GUIDisplayTech();
                    GUILayout.EndScrollView();
                    break;
                case TreeGUIType.Monuments:
                    pagePosT = GUILayout.BeginScrollView(pagePosT);
                    GUIDisplayWObject();
                    GUILayout.EndScrollView();
                    break;
                case TreeGUIType.TechSelect:
                    GUIDisplayTechSelect();
                    break;
                case TreeGUIType.MonumentsSelect:
                    GUIDisplayMMSelect();
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
            /*
            if (GUILayout.Button("Techs Included", AltUI.ButtonRed))
            {
                validNew = false;
                delta = true;
                guiType = TreeGUIType.TechSelect;
            }*/
            if (GUILayout.Button("Add Techs", AltUI.ButtonRed))
            {
                validNew = false;
                delta = true;
                PlayerOption = false;
                guiType = TreeGUIType.TechSelect;
            }
            if (GUILayout.Button("Monuments", AltUI.ButtonRed))
            {
                validNew = false;
                delta = true;
                guiType = TreeGUIType.Monuments;
            }
            if (ActiveGameInterop.inst && ActiveGameInterop.IsReady)
            {
                if (GUILayout.Button("Send To Editor", AltUI.ButtonBlue))
                    ActiveGameInterop.TryTransmit("RetreiveMissionTree", tree.TreeHierachy.AddressName);
            }
            else if (GUILayout.Button("Needs UnityEditor", AltUI.ButtonGrey))
            { 
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Files", AltUI.ButtonBlue))
                SMissionJSONLoader.OpenInExplorer(Path.Combine(SMissionJSONLoader.MissionsDirectory, 
                    tree.TreeHierachy.AddressName));
        }
        private static Vector2 scrollDragD = Vector2.zero;
        private static string FactionPrev = string.Empty;
        private static bool ValidCorp = false;
        private static string XPrev = string.Empty;
        private static string YPrev = string.Empty;
        private static bool ValidY = false;
        private static void GUIDisplayTree()
        {
            RDispTitle = tree.TreeName;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Tree Identifier:");
            GUILayout.FlexibleSpace();
            tree.TreeName = GUILayout.TextField(tree.TreeName, AltUI.TextfieldBlackLeft);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Tree Type:");
            GUILayout.FlexibleSpace();
            GUILayout.Label(tree.TreeHierachy.GetType().ToString());
            GUILayout.EndHorizontal();
            GUILayoutHelpers.GUILabelDispFast("Total Number Of Missions:", tree.Missions.Count);
            GUILayoutHelpers.GUILabelDispFast("Number Of Instant Missions:", tree.ImmedeateMissions.Count);
            GUILayoutHelpers.GUILabelDispFast("Number Of Techs:", tree.TreeTechCount);
            GUILayoutHelpers.GUILabelDispFast("Number Of WorldObjects:", tree.WorldObjects.Count);

            GUILayout.Space(16);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Main Corp (Short Name): ");
            GUILayout.Label(tree.Faction);
            GUILayout.FlexibleSpace();
            tree.Faction = GUILayout.TextField(tree.Faction, AltUI.TextfieldBlackLeft);
            if (tree.Faction != FactionPrev)
            {
                ValidCorp = tree.GetTreeCorp() != (FactionSubTypes)(-1);
                FactionPrev = tree.Faction;
            }
            if (ValidCorp)
                GUILayout.Label("<color=green>O</color>", GUILayout.Width(25));
            else
                GUILayout.Label("<color=red>X</color>", GUILayout.Width(25));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Progress X: ");
            GUILayout.Label(tree.ProgressXName);
            GUILayout.FlexibleSpace();
            tree.ProgressXName = GUILayout.TextField(tree.ProgressXName, AltUI.TextfieldBlackLeft);
            if (tree.ProgressXName != XPrev)
            {
                ValidY = tree.ProgressXName != tree.ProgressYName;
                XPrev = tree.ProgressXName;
            }
            if (ValidY)
                GUILayout.Label("<color=green>O</color>", GUILayout.Width(25));
            else
                GUILayout.Label("<color=red>X</color>", GUILayout.Width(25));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Progress Y: ");
            GUILayout.Label(tree.ProgressYName);
            GUILayout.FlexibleSpace();
            tree.ProgressYName = GUILayout.TextField(tree.ProgressYName, AltUI.TextfieldBlackLeft);
            if (tree.ProgressYName != YPrev)
            {
                ValidY = tree.ProgressXName != tree.ProgressYName;
                YPrev = tree.ProgressYName;
            }
            if (ValidY)
                GUILayout.Label("<color=green>O</color>", GUILayout.Width(25));
            else
                GUILayout.Label("<color=red>X</color>", GUILayout.Width(25));
            GUILayout.EndHorizontal();
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
                var active = ManSubMissions.activeSubMissionsCached.Find(x => x.Name == item.Name);
                if (active != null)
                {
                    if (GUILayout.Button(item.Name, AltUI.ButtonGreen))
                    {
                        selectedMission = null;
                        selectedActiveMission = active;
                        addNewMission = false;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Open);
                    }
                }
                else
                {
                    if (GUILayout.Button(item.Name, AltUI.ButtonRed))
                    {
                        selectedMission = item;
                        selectedActiveMission = null;
                        addNewMission = false;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Open);
                    }
                }
                GUILayout.Label(item.Faction);
                GUILayout.Label("|");
                GUILayout.Label(item.GradeRequired.ToString());
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
            }

            foreach (var item in tree.ImmedeateMissions.OrderBy(x => x.Type))
            {
                if (prev != item.Type)
                {
                    GUILayout.Label(item.Type.ToString(), AltUI.ButtonGrey, GUILayout.Height(32));
                    prev = item.Type;
                }
                if (prev != item.Type)
                {
                    GUILayout.Label(item.Type.ToString(), AltUI.ButtonGrey, GUILayout.Height(32));
                    prev = item.Type;
                }
                GUILayout.BeginHorizontal(GUILayout.Height(32));
                var active = ManSubMissions.activeSubMissionsCached.Find(x => x.Name == item.Name);
                if (active != null)
                {
                    if (GUILayout.Button(item.Name, AltUI.ButtonGreen))
                    {
                        selectedMission = null;
                        selectedActiveMission = active;
                        addNewMission = false;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Open);
                    }
                }
                else
                {
                    if (GUILayout.Button(item.Name, AltUI.ButtonRed))
                    {
                        selectedMission = item;
                        selectedActiveMission = null;
                        addNewMission = false;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Open);
                    }
                }
                GUILayout.Label(item.Faction);
                GUILayout.Label("|");
                GUILayout.Label(item.GradeRequired.ToString());
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
            }

            foreach (var item in tree.RepeatMissions.OrderBy(x => x.Type))
            {
                if (prev != item.Type)
                {
                    GUILayout.Label(item.Type.ToString(), AltUI.ButtonGrey, GUILayout.Height(32));
                    prev = item.Type;
                }
                if (prev != item.Type)
                {
                    GUILayout.Label(item.Type.ToString(), AltUI.ButtonGrey, GUILayout.Height(32));
                    prev = item.Type;
                }
                GUILayout.BeginHorizontal(GUILayout.Height(32));
                var active = ManSubMissions.activeSubMissionsCached.Find(x => x.Name == item.Name);
                if (active != null)
                {
                    if (GUILayout.Button(item.Name, AltUI.ButtonGreen))
                    {
                        selectedMission = null;
                        selectedActiveMission = active;
                        addNewMission = false;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Open);
                    }
                }
                else
                {
                    if (GUILayout.Button(item.Name, AltUI.ButtonRed))
                    {
                        selectedMission = item;
                        selectedActiveMission = null;
                        addNewMission = false;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Open);
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

        private static SubMissionStandby selectedMission = null;
        private static SubMission selectedActiveMission = null;
        private static bool addNewMission = false;
        private static void GUIListMissions()
        {
            LBarTitle = "Missions:";

            SMAutoFill.OneWayButtonLarge("Add New", ref addNewMission);

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
                    selectedActiveMission = null;
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
                        VarTrueFalseActive = new List<bool>(),
                        VarIntsActive = new List<int>(),
                        EventList = new List<Steps.SubMissionStep>(),
                        TrackedTechs = new List<TrackedTech>(),
                        ActiveState = SubMissionLoadState.NotAvail,
                        CheckList = new List<MissionChecklist>(),
                    };
                    SMissionJSONLoader.SaveMission(tree, mission);
                    tree.Missions.Add(SubMissionTree.CompileToStandby(mission));
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                }
            }
            else if (GUILayout.Button("Create", AltUI.ButtonGreyLarge, GUILayout.Height(128))) { }
            if (GUILayout.Button("Open in Explorer", AltUI.ButtonBlue))
            {
                SMissionJSONLoader.OpenInExplorer(
                    Path.Combine(SMissionJSONLoader.MissionsDirectory,
                    tree.TreeName, "Missions"));
            }
        }
        private static void GUIDisplayExistingMission()
        {
            if (selectedActiveMission != null && ManSubMissions.activeSubMissionsCached.Contains(selectedActiveMission))
            {
                RDispTitle = selectedActiveMission.Name;
                if (GUILayout.Button("Open Editor", AltUI.ButtonBlueLarge, GUILayout.Height(128)))
                {
                    ManSubMissions.Selected = selectedActiveMission;
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                }
                if (GUILayout.Button("FORCE END (SAVE FIRST)", AltUI.ButtonRed, GUILayout.Height(128)))
                {
                    ManSubMissions.SelectedIsAnon = false;
                    ManSubMissions.Selected = selectedActiveMission;
                    ButtonAct.inst.Invoke("RequestCancelSMission", 0);
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.MissionFailed);
                }
            }
            else if (selectedMission != null)
            {
                RDispTitle = selectedMission.Name;
                if (GUILayout.Button("Edit Mission", AltUI.ButtonBlueLarge, GUILayout.Height(128)))
                {
                    if (selectedMission.tree.AcceptTreeMission(selectedMission, true))
                    {
                        editor.Paused = true;
                        selectedActiveMission = ManSubMissions.activeSubMissionsCached.Find(x => x.Name == selectedMission.Name);
                        if (selectedActiveMission != null)
                        {
                            ManSubMissions.Selected = selectedActiveMission;
                            addNewMission = false;
                            ManSFX.inst.PlayUISFX(ManSFX.UISfxType.AcceptMission);
                        }
                        else
                            ManSFX.inst.PlayUISFX(ManSFX.UISfxType.MissionFailed);
                    }
                    else
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.AnchorFailed);
                    selectedMission = null;
                }
                if (GUILayout.Button("Test Mission", AltUI.ButtonOrangeLarge, GUILayout.Height(128)))
                {
                    if (selectedMission.tree.AcceptTreeMission(selectedMission, false))
                    {
                        editor.Paused = false;
                        selectedActiveMission = ManSubMissions.activeSubMissionsCached.Find(x => x.Name == selectedMission.Name);
                        if (selectedActiveMission != null)
                        {
                            ManSubMissions.Selected = selectedActiveMission;
                            addNewMission = false;
                            ManSFX.inst.PlayUISFX(ManSFX.UISfxType.AcceptMission);
                        }
                        else
                            ManSFX.inst.PlayUISFX(ManSFX.UISfxType.MissionFailed);
                    }
                    else
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.AnchorFailed);
                    selectedMission = null;
                }
            }
            else
                RDispTitle = "Not Selected";
        }



        // Techs
        private static Vector2 scrollDrag3 = Vector2.zero;
        private static SpawnableTech tech = null;
        private static string techName = "";
        private static Texture2D techImage = Texture2D.whiteTexture;
        private static bool addNewTech = false;
        private static void GUIListTechs()
        {
            LBarTitle = "Techs:";

            SMAutoFill.OneWayButtonLarge("Add New", ref addNewTech);

            scrollDrag3 = GUILayout.BeginScrollView(scrollDrag3);
            SubMissionType prev = (SubMissionType)(-1);

            foreach (var itemKey in tree.TreeTechs)
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
                    item.GetTextureAsync(tree, (TechData techData, Texture2D techImage2) =>
                    {
                        techImage = techImage2;
                    });
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
        private static Tank previewTech = null;
        private static string newTech = "";
        private static void GUIDisplayNewTech()
        {
            var newTech2 = SMUtil.GUIDisplaySelectTech();
            if (delta || (newTech != newTech2))
            {
                delta = false;
                newTech = newTech2;
                if (newTech.NullOrEmpty() || tree.TreeTechs.Any(x => x.Key == newTech))
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
                        SMissionJSONLoader.SaveNewTechRaw(tree, newTech, TechData.CreateTechData());
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Snapshot);
                    }
                    validNew = false;
                }
            }
            else if (GUILayout.Button("Attach Raw", AltUI.ButtonGreyLarge, GUILayout.Height(128))) { }
            if (GUILayout.Button("Open Snapshots in Explorer", AltUI.ButtonBlue))
            {
                SMissionJSONLoader.OpenInExplorer(
                    Path.Combine(SMissionJSONLoader.MissionsDirectory,
                    tree.TreeName, "Techs"));
            }
            if (GUILayout.Button("Open RAW Techs in Explorer", AltUI.ButtonBlue))
            {
                SMissionJSONLoader.OpenInExplorer(
                    Path.Combine(SMissionJSONLoader.MissionsDirectory,
                    tree.TreeName, "Raw Techs"));
            }
        }
        
        private static void GUIDisplayExistingTech()
        {
            if (tech != null)
            {
                RDispTitle = tech.name;
                GUILayout.Label("Type: " + tech.GetType(), GUILayout.Width(300));
                GUILayout.Box(techImage, AltUI.TextfieldBorderedBlue, GUILayout.Width(300), GUILayout.Height(220));
                if (previewTech == null)
                {
                    if (GUILayout.Button("Spawn Preview", AltUI.ButtonGreen))
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Select);
                        previewTech = tech.Spawn(tree, Singleton.playerPos + Camera.main.transform.forward.SetY(0) * 32,
                            Vector3.forward, ManPlayer.inst.PlayerTeam);
                    }
                }
                else if (GUILayout.Button("Clear Preview", AltUI.ButtonBlue))
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.SendToInventory);
                    previewTech.visible.RemoveFromGame();
                    previewTech = null;
                }
                if (GUILayout.Button("DELETE", AltUI.ButtonRed))
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.SendToInventory);
                }
            }
            else
                RDispTitle = "Not Selected";
        }

        // TechSelectPopupScreen (Side)
        private const int SelectorHeight = 90;
        private const int techSelRendCount = 8;
        private const string loading = "Loading";
        private static int SelectorIndex = -1;
        private static int SelectorRendIndex = -1;
        private static string[] SelectorRendNames = new string[techSelRendCount];
        private static Texture2D[] SelectorRendTextures = new Texture2D[techSelRendCount];
        private static Vector2 SelectorPos = Vector2.zero;
        public static string lastSelectedName = null;
        internal static void GUITechSelectorSet(string itemName)
        {
            if (itemName != null)
            {
                if (itemName == "Player Tech")
                    SelectorIndex = -2;
                else
                    SelectorIndex = tree.TreeTechs.Keys.ToList().IndexOf(itemName);
            }
            else
                SelectorIndex = -1;
        }
        private static void GUIDisplayTechSelect()
        {
            RDispTitle = "Tech Select";
            int techsPresent = tree.TreeTechs.Count;
            int pageScrollerTopIndex = Mathf.FloorToInt(SelectorPos.y / SelectorHeight);
            if (Event.current.type == EventType.Layout &&
                SelectorRendIndex != pageScrollerTopIndex)
            {
                SelectorRendIndex = pageScrollerTopIndex;
                for (int j = 0; j < techSelRendCount; j++)
                {
                    var indexR = j + SelectorRendIndex;
                    if (indexR >= 0 && indexR < techsPresent)
                    {
                        var inst = tree.TreeTechs.Values.ElementAt(indexR);
                        SelectorRendNames[j] = inst.name;
                        SelectorRendTextures[j] = inst.GetTexture(tree);
                    }
                }
            }
            SelectorPos = GUILayout.BeginScrollView(SelectorPos);
            int prevTechSelect = SelectorIndex;
            if (PlayerOption)
            {
                GUIStyle style;
                if (prevTechSelect == -2)
                    style = AltUI.ButtonOrangeLarge;
                else
                    style = AltUI.ButtonBlueLarge;
                if (GUILayout.Button("Player Tech", style, GUILayout.Height(SelectorHeight)))
                {
                    lastSelectedName = "Player Tech";
                    SelectorIndex = -2;
                }
            }
            for (int i = 0; i < techsPresent; i++)
            {
                int indexR = i - SelectorRendIndex;
                GUILayout.BeginHorizontal(AltUI.BoxBlack, GUILayout.Height(SelectorHeight));
                if (indexR >= 0 && indexR < techSelRendCount)
                {
                    GUIStyle style;
                    if (prevTechSelect == i)
                        style = AltUI.ButtonOrangeLarge;
                    else
                        style = AltUI.ButtonBlueLarge;
                    if (SelectorRendTextures[indexR] && SelectorRendTextures[indexR] != Texture2D.whiteTexture)
                    {
                        if (GUILayout.Button(SelectorRendTextures[indexR], style,
                            GUILayout.Height(SelectorHeight), GUILayout.Width(SelectorHeight * 1.3f)))
                        {
                            lastSelectedName = SelectorRendNames[indexR];
                            SelectorIndex = i;
                        }
                    }
                    else
                    {
                        var inst = tree.TreeTechs.Values.ElementAt(i);
                        SelectorRendTextures[indexR] = inst.GetTexture(tree);
                        if (GUILayout.Button(loading, style,
                        GUILayout.Height(SelectorHeight), GUILayout.Width(SelectorHeight * 1.3f)))
                        {
                            lastSelectedName = SelectorRendNames[indexR];
                            SelectorIndex = i;
                        }
                    }

                    if (GUILayout.Button(SelectorRendNames[indexR], style, GUILayout.Height(SelectorHeight)))
                    {
                        lastSelectedName = SelectorRendNames[indexR];
                        SelectorIndex = i;
                    }
                }
                else
                {
                    if (GUILayout.Button(loading, AltUI.ButtonBlueLarge,
                        GUILayout.Height(SelectorHeight), GUILayout.Width(SelectorHeight * 1.3f)))
                    {
                        lastSelectedName = tree.TreeTechs.Keys.ElementAt(i);
                        SelectorIndex = i;
                    }
                    if (GUILayout.Button(loading, AltUI.ButtonBlueLarge, GUILayout.Height(SelectorHeight)))
                    {
                        lastSelectedName = tree.TreeTechs.Keys.ElementAt(i);
                        SelectorIndex = i;
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            if (SelectorIndex == -2)
            {
                GUILayout.EndVertical();
                GUILayout.BeginVertical(GUILayout.Width(200));
                GUILayout.Label(lastSelectedName.NullOrEmpty() ? "NULL" : lastSelectedName, AltUI.WindowHeaderBlue,
                    GUILayout.Height(32), GUILayout.Width(200));
                if (GUILayout.Button("Close", AltUI.ButtonRed))
                {
                    SelectorIndex = -1;
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Select);
                }
                GUILayout.FlexibleSpace();
                if (actReturn != null && GUILayout.Button("Select", AltUI.ButtonGreen))
                {
                    actReturn();
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Select);
                }
                GUILayout.FlexibleSpace();
            }
            else if (SelectorIndex >= 0 && SelectorIndex < techsPresent)
            {
                GUILayout.EndVertical();
                GUILayout.BeginVertical(GUILayout.Width(200));
                GUILayout.Label(lastSelectedName.NullOrEmpty() ? "NULL" : lastSelectedName, AltUI.WindowHeaderBlue,
                    GUILayout.Height(32), GUILayout.Width(200));
                var tech = tree.TreeTechs.Values.ElementAt(SelectorIndex);
                if (GUILayout.Button("Close", AltUI.ButtonRed))
                {
                    SelectorIndex = -1;
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Select);
                }
                GUILayout.FlexibleSpace();
                if (SelectorIndex >= 0 && SelectorIndex < techsPresent)
                {
                    if (actReturn != null && GUILayout.Button("Select", AltUI.ButtonGreen))
                    {
                        actReturn();
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Select);
                    }
                    if (previewTech == null)
                    {
                        if (GUILayout.Button("Spawn", AltUI.ButtonGreen))
                        {
                            ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Select);
                            previewTech = tech.Spawn(tree, Singleton.playerPos + Camera.main.transform.forward.SetY(0) * 32,
                                Vector3.forward, ManPlayer.inst.PlayerTeam);
                        }
                    }
                    else if (GUILayout.Button("Clear", AltUI.ButtonBlue))
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.SendToInventory);
                        previewTech.visible.RemoveFromGame();
                        previewTech = null;
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("DELETE", AltUI.ButtonRed))
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.SendToInventory);
                        tech.Remove(tree);
                        SelectorRendIndex = -1;
                    }
                }
            }
        }
        internal static void GUIResetSelector()
        {
            SelectorRendIndex = -1;
            SelectorIndex = -1;
        }

        private static UIScreenTechLoader loader;
        private static void GUIListTechSelect()
        {
            LBarTitle = "Techs:";
            // Open
            if (loader == null)
            {
                if (GUILayout.Button("Import", AltUI.ButtonOrangeLarge, GUILayout.Height(48)))
                {
                    loader = (UIScreenTechLoader)ManUI.inst.GetScreen(ManUI.ScreenType.TechLoaderScreen);
                    if (loader.SelectorCallback != null)
                        throw new Exception("GUIListTechSelect ~ ////Open called while UIScreenTechLoader was already busy in an operation");

                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Open);
                    loader.SelectorCallback = OnTechSet;
                    loader.Show(true);
                    WindowManager.HidePopup(ManSubMissions.Editor.Display);
                }
            }
            else if (GUILayout.Button("Import", AltUI.ButtonGreyLarge, GUILayout.Height(48)))
            {
                loader.SelectorCallback = null;
                loader.Hide();
                loader = null;
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Close);
            }
            if (Singleton.playerTank != null)
            {
                if (GUILayout.Button("Add Current", AltUI.ButtonBlueLarge, GUILayout.Height(48)))
                {
                    TechData TD = RawTechBase.CreateNewTechData();
                    TD.SaveTech(Singleton.playerTank);
                    if (SMissionJSONLoader.SaveNewTechRaw(tree, Singleton.playerTank.name, TD))
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.LevelUp);
                    else
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.AnchorFailed);
                    SelectorRendIndex = -1;
                }
            }
            else if (GUILayout.Button("Add Current", AltUI.ButtonGreyLarge, GUILayout.Height(48)))
            {
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.AnchorFailed);
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Folder", AltUI.ButtonBlue))
            {
                SMissionJSONLoader.OpenInExplorer(
                    Path.Combine(SMissionJSONLoader.MissionsDirectory,
                    tree.TreeName, "Raw Techs"));
            }
            if (GUILayout.Button("Reload Techs", AltUI.ButtonBlue))
            {
                tree.TreeHierachy.LoadTreeTechs(ref tree.MissionTextures, ref tree.TreeTechs);
                GUIResetSelector();
                GUITechSelectorSet(lastSelectedName);
            }
        }
        private static void OnTechSet(Snapshot set)
        {
            if (loader.SelectorCallback != OnTechSet)
                throw new Exception("UIScreenTechLoader was altered while SMSFieldTechSelectGUI was busy using it");

            SMissionJSONLoader.SaveNewTechRaw(tree, set.m_Name.Value, set.techData);
            //SMissionJSONLoader.SaveNewTechSnapshot(tree, set.m_Name.Value, set.image);

            loader.SelectorCallback = null;
            loader.Hide();
            loader = null;
            ManSFX.inst.PlayUISFX(ManSFX.UISfxType.LevelUp);
            WindowManager.ShowPopup(ManSubMissions.Editor.Display);
            SelectorRendIndex = -1;
        }



        // ModularMonuments
        private static Vector2 scrollDrag4 = Vector2.zero;
        private static SMWorldObject wObject = null;
        private static GameObject previewWO = null;
        private static string wObjectName = "";
        private static bool addWObject = false;
        private static void GUIListWObjects()
        {
            LBarTitle = "Pieces:";

            SMAutoFill.OneWayButtonLarge("Add New", ref addWObject);

            scrollDrag4 = GUILayout.BeginScrollView(scrollDrag4);
            SubMissionType prev = (SubMissionType)(-1);

            foreach (var itemKey in tree.WorldObjects.OrderBy(x => x.Key))
            {
                var item = itemKey.Value;
                if (item == null)
                {
                    GUILayout.Button("ErrorCode0", AltUI.ButtonGrey);
                    continue;
                }
                string name = item.name.NullOrEmpty() ? "NULL_NAME": item.name;
                GUILayout.BeginHorizontal(GUILayout.Height(32));
                if (name == wObjectName)
                {
                    GUILayout.Button(name, AltUI.ButtonBlueActive);
                }
                else if (GUILayout.Button(name, AltUI.ButtonBlue) && item.name != wObjectName)
                {
                    wObject = item;
                    wObjectName = name;
                    WObjectVis = GetTexture(item);
                    addWObject = false;
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Open);
                }
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            if (GUILayout.Button("Open Folder", AltUI.ButtonBlue))
            {
                SMissionJSONLoader.OpenInExplorer(
                    Path.Combine(SMissionJSONLoader.MissionsDirectory,
                    tree.TreeName, "Pieces"));
            }
            if (GUILayout.Button("Reload Files", AltUI.ButtonBlue))
            {
                tree.TreeHierachy.LoadTreeWorldObjects(ref tree.WorldObjectFileNames);
            }
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
        private static Texture2D WObjectVis = null;
        private static void GUIDisplayNewWObject()
        {
            if (delta || SMAutoFill.AutoTextField("Name", ref newWObject, 64))
            {
                delta = false;
                if (newWObject.NullOrEmpty() || newWObject.Length <= 3 ||
                    tree.WorldObjects.Values.Any(x => x.IsNotNull() && x.Name == newWObject))
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
                        VisualMeshName = "ModularBrickCube_6x3x6.obj",
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
                RDispTitle = wObject.Name;
                if (WObjectVis != null)
                    GUILayout.Box(WObjectVis, AltUI.TextfieldBorderedBlue);
                if (previewWO == null)
                {
                    if (GUILayout.Button("Spawn Preview", AltUI.ButtonGreen))
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Select);
                        WorldPosition WP = WorldPosition.FromScenePosition(Singleton.playerPos + 
                            Camera.main.transform.forward * 80);
                        ManModularMonuments.SpawnMM(new ModularMonumentSave
                        {
                            eulerAngles = Vector3.zero,
                            name = wObject.Name,
                            treeName = tree.TreeName,
                            offsetFromTile = WP.TileRelativePos,
                            tilePos = WP.TileCoord,
                            scale = Vector3.one,
                        }, out previewWO);
                    }
                }
                else if (GUILayout.Button("Clear Preview", AltUI.ButtonBlue))
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.SendToInventory);
                    previewWO.GetComponent<SMWorldObject>().Remove(false);
                }
                if (GUILayout.Button("DELETE", AltUI.ButtonRed))
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.SendToInventory);
                }
            }
            else
                RDispTitle = "Not Selected";
        }


        // WorldObjectSelector
        internal static void GUIMMSelectorSet(string itemName)
        {
            if (itemName != null)
                SelectorIndex = tree.WorldObjects.Keys.ToList().IndexOf(itemName);
            else
                SelectorIndex = -1;
        }
        private static void GUIDisplayMMSelect()
        {
            RDispTitle = "World Object Select";
            int techsPresent = tree.WorldObjects.Count;
            int pageScrollerTopIndex = Mathf.FloorToInt(SelectorPos.y / SelectorHeight);
            if (Event.current.type == EventType.Layout &&
                SelectorRendIndex != pageScrollerTopIndex)
            {
                SelectorRendIndex = pageScrollerTopIndex;
                for (int j = 0; j < techSelRendCount; j++)
                {
                    var indexR = j + SelectorRendIndex;
                    if (indexR >= 0 && indexR < techsPresent)
                    {
                        var inst = tree.WorldObjects.Values.ElementAt(indexR);
                        SelectorRendNames[j] = inst.name;
                        SelectorRendTextures[j] = GetTexture(inst);
                    }
                }
            }
            SelectorPos = GUILayout.BeginScrollView(SelectorPos);
            int prevTechSelect = SelectorIndex;
            for (int i = 0; i < techsPresent; i++)
            {
                int indexR = i - SelectorRendIndex;
                GUILayout.BeginHorizontal(AltUI.BoxBlack, GUILayout.Height(SelectorHeight));
                if (indexR >= 0 && indexR < techSelRendCount)
                {
                    GUIStyle style;
                    if (prevTechSelect == i)
                        style = AltUI.ButtonOrangeLarge;
                    else
                        style = AltUI.ButtonBlueLarge;
                    if (SelectorRendTextures[indexR] && SelectorRendTextures[indexR] != Texture2D.whiteTexture)
                    {
                        if (GUILayout.Button(SelectorRendTextures[indexR], style,
                            GUILayout.Height(SelectorHeight), GUILayout.Width(SelectorHeight * 1.3f)))
                        {
                            lastSelectedName = SelectorRendNames[indexR];
                            SelectorIndex = i;
                        }
                    }
                    else
                    {;
                        if (GUILayout.Button(loading, style,
                        GUILayout.Height(SelectorHeight), GUILayout.Width(SelectorHeight * 1.3f)))
                        {
                            lastSelectedName = SelectorRendNames[indexR];
                            SelectorIndex = i;
                        }
                    }

                    if (GUILayout.Button(SelectorRendNames[indexR], style, GUILayout.Height(SelectorHeight)))
                    {
                        lastSelectedName = SelectorRendNames[indexR];
                        SelectorIndex = i;
                    }
                }
                else
                {
                    if (GUILayout.Button(loading, AltUI.ButtonBlueLarge,
                        GUILayout.Height(SelectorHeight), GUILayout.Width(SelectorHeight * 1.3f)))
                    {
                        lastSelectedName = tree.TreeTechs.Keys.ElementAt(i);
                        SelectorIndex = i;
                    }
                    if (GUILayout.Button(loading, AltUI.ButtonBlueLarge, GUILayout.Height(SelectorHeight)))
                    {
                        lastSelectedName = tree.TreeTechs.Keys.ElementAt(i);
                        SelectorIndex = i;
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            if (SelectorIndex >= 0 && SelectorIndex < techsPresent)
            {
                GUILayout.EndVertical();
                GUILayout.BeginVertical(GUILayout.Width(200));
                GUILayout.Label(lastSelectedName.NullOrEmpty() ? "NULL" : lastSelectedName, AltUI.WindowHeaderBlue,
                    GUILayout.Height(32), GUILayout.Width(200));
                var tech = tree.TreeTechs.Values.ElementAt(SelectorIndex);
                if (GUILayout.Button("Close", AltUI.ButtonRed))
                {
                    SelectorIndex = -1;
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Select);
                }
                GUILayout.FlexibleSpace();
                if (SelectorIndex >= 0 && SelectorIndex < techsPresent)
                {
                    if (actReturn != null && GUILayout.Button("Select", AltUI.ButtonGreen))
                    {
                        actReturn();
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Select);
                    }
                    if (previewTech == null)
                    {
                        if (GUILayout.Button("Spawn", AltUI.ButtonGreen))
                        {
                            ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Select);
                            previewTech = tech.Spawn(tree, Singleton.playerPos + Camera.main.transform.forward.SetY(0) * 32,
                                Vector3.forward, ManPlayer.inst.PlayerTeam);
                        }
                    }
                    else if (GUILayout.Button("Clear", AltUI.ButtonBlue))
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.SendToInventory);
                        previewTech.visible.RemoveFromGame();
                        previewTech = null;
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("DELETE", AltUI.ButtonRed))
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.SendToInventory);
                        tech.Remove(tree);
                        SelectorRendIndex = -1;
                    }
                }
            }
        }
        private static void GUIListMMSelect()
        {
            LBarTitle = "Objects:";
            // Open
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Folder", AltUI.ButtonBlue))
            {
                SMissionJSONLoader.OpenInExplorer(
                    Path.Combine(SMissionJSONLoader.MissionsDirectory,
                    tree.TreeName, "Pieces"));
            }
            if (GUILayout.Button("Reload Objects", AltUI.ButtonBlue))
            {
                tree.TreeHierachy.LoadTreeWorldObjects(ref tree.WorldObjectFileNames);
                MM_JSONLoader.BuildAllWorldObjects(ManSubMissions.SubMissionTrees);
                GUIResetSelector();
                GUIMMSelectorSet(lastSelectedName);
            }
        }

        internal static Texture2D GetTexture(SMWorldObject obj)
        {
            try
            {
                obj.gameObject.SetActive(true);
                return ResourcesHelper.GeneratePreviewForGameObject(obj.gameObject, obj.GetComponent<Collider>().bounds);
            }
            catch
            {
                return null;
            }
            finally
            {
                obj.gameObject.SetActive(false);
            }
        }
    }
}
