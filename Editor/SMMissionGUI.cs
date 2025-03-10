﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TerraTechETCUtil;
using Sub_Missions.Steps;
using Sub_Missions.ManWindows;
using TerraTechETCUtil;

namespace Sub_Missions.Editor
{
    public static class SMMissionGUI
    {
        //1000 is width of editor window
        internal const int LeftBarSize = 200;
        internal const int RightDisplaySize = ManModGUI.LargeWindowWidth - LeftBarSize;

        // UI Display Information
        internal static float PositioningHighlightStopDelay = 2;
        internal static Dictionary<SMStepType, Color> PositioningColor = new Dictionary<SMStepType, Color>()
        {
            { SMStepType.ActAirstrike, new Color(1, 0.5f, 0.5f) },
            { SMStepType.ActDrive, new Color(1, 0.5f, 0) },
            { SMStepType.CheckPlayerDist, Color.grey },
            { SMStepType.SetupMM, Color.black },
            { SMStepType.SetupResources, Color.green },
            { SMStepType.SetupTech, Color.cyan },
            { SMStepType.SetupWaypoint, Color.yellow },
        };



        internal static bool ShowGUITopBar(GUISMissionEditor editor)
        {
            GUILayout.BeginHorizontal(GUILayout.Height(32));
            GUILayout.Label("Selected Tree ID: ");
            GUILayout.Label(ManSubMissions.Selected.Name);
            GUILayout.FlexibleSpace();
            if (editor.Paused)
            {
                if (GUILayout.Button("Save", AltUI.ButtonGreen, GUILayout.Width(60)))
                {
                    SMissionJSONLoader.SaveMission(ManSubMissions.Selected.Tree, ManSubMissions.Selected);
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
            }
            else
            {
                GUILayout.Label("|", GUILayout.Width(10));
                if (editor.Paused)
                {
                    if (GUILayout.Button("Paused", AltUI.ButtonGrey, GUILayout.Width(75)))
                    {
                        editor.Paused = false;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.PopUpOpen);
                    }
                }
                else
                {
                    if (GUILayout.Button("Play", AltUI.ButtonBlue, GUILayout.Width(75)))
                    {
                        editor.Paused = true;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.PopUpClose);
                    }
                }
                if (editor.SlowMode)
                {
                    if (GUILayout.Button("Slow", AltUI.ButtonBlue, GUILayout.Width(60)))
                    {
                        editor.SlowMode = false;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.PopUpOpen);
                    }
                }
                else
                {
                    if (GUILayout.Button("Slow", AltUI.ButtonGrey, GUILayout.Width(60)))
                    {
                        editor.SlowMode = true;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.PopUpClose);
                    }
                }
                if (GUILayout.Button("Reset", AltUI.ButtonRed, GUILayout.Width(60)))
                    ManSubMissions.Selected.Reboot(true);
                if (GUILayout.Button("Restart", AltUI.ButtonRed, GUILayout.Width(80)))
                    ManSubMissions.Selected.Reboot();
                if (editor.LockEnding)
                {
                    if (GUILayout.Button("End", AltUI.ButtonGrey, GUILayout.Width(60)))
                    {
                        editor.LockEnding = false;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                    }
                }
                else
                {
                    if (GUILayout.Button("End", AltUI.ButtonBlue, GUILayout.Width(60)))
                    {
                        editor.LockEnding = false;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Back);
                    }
                }
                if (GUILayout.Button("Log", SMUtil.collectedErrors ? AltUI.ButtonRed :
                    SMUtil.collectedLogs ? AltUI.ButtonGreen : SMUtil.collectedInfos ?
                    AltUI.ButtonBlue : AltUI.ButtonGrey, GUILayout.Width(60)))
                {
                    SMUtil.PushErrors();
                }
                if (GUILayout.Button("Exit", AltUI.ButtonRed, GUILayout.Width(60)))
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Close);
                    ManSubMissions.Selected = null;
                }

                GUILayout.EndHorizontal();
                return !editor.DisplayHierachy();
            }
            GUILayout.EndHorizontal();
            return false;
        }

        private static SubMission mission = null;
        private static SubMissionStep selectedStep = null;
        private static bool ShowAllStepsInWorld = false;

        internal static bool ShowGUI(SubMission mission)
        {
            MissionChange(mission);
            bool interacted = false;

            if (mission != null)
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
                    throw new Exception("Error in GUILeftBar()", e);
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                try
                {
                    interacted = GUIRightInfo();
                }
                catch (ExitGUIException e) { throw e; }
                catch (Exception e)
                {
                    throw new Exception("Error in GUIRightInfo()", e);
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            else
                GUILayout.Label("No Mission Selected error");
            return interacted;
        }

        private static Dictionary<int, Dictionary<int, SubMissionStep>> StepsByTier = new Dictionary<int, Dictionary<int, SubMissionStep>>();
        private static void MissionChange(SubMission missionIn)
        {
            if (mission == missionIn)
                return;
            mission = missionIn;
            OnMissionChange();
        }
        private static void OnMissionChange()
        {
            if (StepsByTier == null)
                throw new Exception("GUIReParse - somehow StepsByTier is null");
            StepsByTier.Clear();
            if (mission != null)
            {
                int step = 0;
                foreach (var item in mission.GetAllEventsLinear())
                {
                    if (StepsByTier.TryGetValue(item.ProgressID, out var val))
                    {
                        val.Add(step, item);
                    }
                    else
                    {
                        val = new Dictionary<int, SubMissionStep>
                        {
                            { step, item }
                        };
                        StepsByTier.Add(item.ProgressID, val);
                    }
                    step++;
                }
            }
        }



        private static string LBarTitle = "";
        private static MissionGUIType mgType = MissionGUIType.Main;
        private static Vector2 scrollDrag = Vector2.zero;
        private static void GUILeftBar()
        {
            if (LBarTitle == null)
                LBarTitle = "";
            GUILayout.Label(LBarTitle, AltUI.WindowHeaderBlue, GUILayout.Height(32));
            if (mgType != MissionGUIType.Main && 
                GUILayout.Button("Back", AltUI.ButtonRed, GUILayout.Height(32)))
                mgType = MissionGUIType.Main;
            else
            {
                switch (mgType)
                {
                    case MissionGUIType.Main:
                        GUIMainBar();
                        break;
                    case MissionGUIType.Steps:
                        GUIDrag();
                        break;
                    case MissionGUIType.Checklist:
                        GUIChecklistBar();
                        break;
                    case MissionGUIType.Rewards:
                        GUIRewardsBar();
                        break;
                    case MissionGUIType.Variables:
                    case MissionGUIType.TrackedBlocks:
                    case MissionGUIType.TrackedTechs:
                    case MissionGUIType.TrackedMonuments:
                        GUIInformationBar();
                        break;
                    default:
                        mgType = MissionGUIType.Main;
                        throw new IndexOutOfRangeException("MissionGUIType [" + mgType.ToString() +
                            "] does not exist in GUILeftBar().  Was it recently added?");
                }
            }
        }
        private static bool GUIRightInfo()
        {
            bool interacted = false;
            switch (mgType)
            {
                case MissionGUIType.Main:
                    GUIMissionInfo();
                    break;
                case MissionGUIType.Steps:
                    try
                    {
                        interacted = GUITimeTable();
                    }
                    catch (ExitGUIException e) { throw e; }
                    catch (Exception e)
                    {
                        mission = null;
                        //GUIUtility.ExitGUI();
                        throw new Exception("Error in GUITimeTable()", e);
                    }
                    if (selectedStep != null)
                    {
                        GUILayout.EndVertical();
                        GUILayout.BeginVertical(GUILayout.Width(500));
                        try
                        {
                            GUIStepDetails();

                            if (HeldElement != null && HeldSomething)
                            {
                                HeldSomething = false;
                            }
                        }
                        catch (ExitGUIException e) { throw e; }
                        catch (Exception e)
                        {
                            selectedStep = null;
                            //GUIUtility.ExitGUI();
                            throw new Exception("Error in GUIStepDetails()", e);
                        }
                    }
                    break;
                case MissionGUIType.Checklist:
                    GUIChecklistInfo();
                    break;
                case MissionGUIType.Rewards:
                    GUIRewardsInfo();
                    break;
                case MissionGUIType.Variables:
                    GUIVariablesInfo();
                    break;
                case MissionGUIType.TrackedBlocks:
                    GUITrackedBlocksInfo();
                    break;
                case MissionGUIType.TrackedTechs:
                    GUITrackedTechsInfo();
                    break;
                case MissionGUIType.TrackedMonuments:
                    GUITrackedMonumentsInfo();
                    break;
                default:
                    mgType = MissionGUIType.Main;
                    throw new IndexOutOfRangeException("MissionGUIType [" + mgType.ToString() +
                        "] does not exist in GUILeftBar().  Was it recently added?");
            }
            return interacted;
        }



        // -------------------------------------------------------------------------- 
        internal enum MissionGUIType
        {
            Main,
            Steps,
            Checklist,
            Rewards,
            Variables,
            TrackedBlocks,
            TrackedTechs,
            TrackedMonuments,
        }
        private static void GUIMainBar()
        {
            LBarTitle = "Details";
            if (GUILayout.Button("Steps", AltUI.ButtonGreen, GUILayout.Height(32)))
                mgType = MissionGUIType.Steps;
            else if (GUILayout.Button("Checklist", AltUI.ButtonGreen, GUILayout.Height(32)))
                mgType = MissionGUIType.Checklist;
            else if (GUILayout.Button("Rewards", AltUI.ButtonGreen, GUILayout.Height(32)))
                mgType = MissionGUIType.Rewards;

            GUILayout.Box("Information", AltUI.TextfieldBorderedBlue, GUILayout.Height(32));

            if (GUILayout.Button("Variables", AltUI.ButtonBlue, GUILayout.Height(32)))
                mgType = MissionGUIType.Variables;
            else if (GUILayout.Button("Tracked Blocks", AltUI.ButtonBlue, GUILayout.Height(32)))
                mgType = MissionGUIType.TrackedBlocks;
            else if (GUILayout.Button("Tracked Techs", AltUI.ButtonBlue, GUILayout.Height(32)))
                mgType = MissionGUIType.TrackedTechs;
            else if (GUILayout.Button("Tracked Monuments", AltUI.ButtonBlue, GUILayout.Height(32)))
                mgType = MissionGUIType.TrackedMonuments;
        }
        private static List<SMFieldGUI> GUIMissionInfoDisplayer = null;
        private static Vector2 GUIMissionInfoScroll = Vector2.zero;
        private static void GUIMissionInfo()
        {
            GUILayout.Box(mission.Name + " Info", AltUI.WindowHeaderBlue, GUILayout.Height(32));
            if (mission != null)
            {
                GUIMissionInfoScroll = GUILayout.BeginScrollView(GUIMissionInfoScroll);
                GUILayout.BeginHorizontal(GUILayout.Height(64));
                GUILayout.Label("Default Name/ID", ManModGUI.styleLabelLargerFont);
                GUILayout.FlexibleSpace();
                GUILayout.Label(mission.Name, ManModGUI.styleBorderedFont);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(GUILayout.Height(32));
                GUILayout.Label("Active State");
                GUILayout.FlexibleSpace();
                GUILayout.Label(mission.ActiveState.ToString(), AltUI.TextfieldBordered);
                GUILayout.EndHorizontal();

                if (mission.AltNames == null)
                    mission.AltNames = new List<string>();
                if (mission.AltDescs == null)
                    mission.AltDescs = new List<string>();

                if (GUIMissionInfoDisplayer == null)
                    GUIMissionInfoDisplayer = SMFieldGUIExt.SetupFields();

                foreach (var item in GUIMissionInfoDisplayer)
                {
                    GUILayout.BeginHorizontal(GUILayout.Height(32));
                    item.DoDisplay(mission);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
            else
                GUILayout.Label("Mission is somehow NULL", ManModGUI.styleLabelLargerFont);
        }


        // -------------------------------------------------------------------------- 

        private static string HeldElement = null;
        private static string LastHeldElement = null;
        private static SubMissionStep HeldElementInst = null;
        private static bool HeldSomething = false;
        private static Rect HeldElementDrag = new Rect(0, 0, 120, 40);

        public static void UpdateDraggables()
        {
            if (HeldElement != null)
            {
                HeldElementDrag = GUI.ModalWindow(3245352, HeldElementDrag, Draggable, "");
            }
        }
        private static void Draggable(int id)
        {
            //GUIUtility.hotControl.
        }

        private static SMStepType lastHoveredTooltip = SMStepType.NULL;
        private static SMissionStep lastHoveredTooltipInst = new StepNull();
        private static void GUIDrag()
        {
            LBarTitle = "Add New";

            if (HeldElementInst != null)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Place down\nheld element", ManModGUI.styleLargeFont);
                GUILayout.Label(HeldElementInst.StepType.ToString(), ManModGUI.styleLargeFont);
                GUILayout.Label("first!", ManModGUI.styleLargeFont);
                GUILayout.FlexibleSpace();
            }
            else
            {
                scrollDrag = GUILayout.BeginScrollView(scrollDrag);

                HeldElement = null;

                for (int i = 0; i < Enum.GetValues(typeof(SMStepType)).Length; i++)
                {
                    SMStepType type = (SMStepType)i;
                    string item = type.ToString();
                    if (item.CompareTo(LastHeldElement) == 0)
                    {
                        if (GUILayout.RepeatButton(item, AltUI.ButtonBlueActive))
                        {
                            HeldSomething = true;
                            HeldElement = item;
                            LastHeldElement = item;
                        }
                    }
                    else if (GUILayout.RepeatButton(item, AltUI.ButtonBlue))
                    {
                        HeldSomething = true;
                        HeldElement = item;
                        LastHeldElement = item;
                    }
                    if (Event.current.type == EventType.Repaint)
                    {
                        var lastWindow = GUILayoutUtility.GetLastRect();
                        if (lastWindow.Contains(Event.current.mousePosition))
                        {
                            if (lastHoveredTooltip != type)
                            {
                                lastHoveredTooltip = type;
                                lastHoveredTooltipInst = SubMissionStep.CreateMissionStep(type);
                            }
                            AltUI.TooltipWorld(lastHoveredTooltipInst.GetTooltip(), false);
                        }
                    }
                }

                GUILayout.EndScrollView();
            }
        }


        internal enum ETimeTableType
        {
            Single,
            Rows,
        }

        private static ETimeTableType tableType = ETimeTableType.Single;
        private static Vector2 scrollTable = Vector2.zero;
        private static string emptyString = "";
        private static string[] emptyStringFillers = new string[0];
        private static int curStep => ManSubMissions.SlowMo ? (mission != null ? mission.UpdateStep : 0) : -1;
        private static bool GUITimeTable()
        {
            GUILayout.BeginHorizontal(AltUI.WindowHeaderBlue, GUILayout.Height(32));
            GUILayout.Label("Step:", AltUI.LabelBlackTitle, GUILayout.Height(32));
            GUILayout.FlexibleSpace();
            GUILayout.Label(mission.CurrentProgressID.ToString(), AltUI.LabelBlackTitle, GUILayout.Height(32));
            GUILayout.EndHorizontal();

            /// ONLY SINGLES WORK!!!
            /*
            tableType = (ETimeTableType)GUILayout.Toolbar((int)tableType,
                Enum.GetNames(typeof(ETimeTableType)), GUILayout.Height(32));
            */
            if (GUILayout.Button(ShowAllStepsInWorld ? "Hide From World" : " Show In World", ShowAllStepsInWorld ?
                AltUI.ButtonBlueActive : AltUI.ButtonBlue))
                ShowAllStepsInWorld = !ShowAllStepsInWorld;
            switch (tableType)
            {
                case ETimeTableType.Single:
                    return GUITimeTableSingle();
                case ETimeTableType.Rows:
                    return GUITimeTableRows();
                default:
                    throw new IndexOutOfRangeException("GUITimeTable() - ETimeTableType is not a valid type");
            }
        }
        private static bool GUITimeTableSingle()
        {
            bool interacted = false;
            scrollTable = GUILayout.BeginScrollView(scrollTable);

            int InsertIndex = 0;
            GUITimeTableSingleSteps(mission.EventList, ref interacted, ref InsertIndex);
            GUITimeTableSingleAddNew(mission.EventList, ref interacted, ref InsertIndex);

            GUILayout.EndScrollView();
            return interacted;
        }
        private static void GUITimeTableSingleAddNew(List<SubMissionStep> listEvents, 
            ref bool interacted, ref int y)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+ ADD +", AltUI.ButtonGrey))
            {
                if (HeldElementInst != null)
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
                    listEvents.Insert(y, HeldElementInst);
                    OnMissionChange();
                    HeldElementInst = null;
                    selectedStep = null;
                    LastHeldElement = null;
                }
                else if (LastHeldElement != null)
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
                    SubMissionStep SMS = new SubMissionStep
                    {
                        StepType = (SMStepType)Enum.Parse(typeof(SMStepType), LastHeldElement),
                        Mission = mission,
                        ProgressID = 0,
                        InitPosition = new Vector3(0, 0, 0),
                        VaribleType = EVaribleType.None,
                    };
                    listEvents.Insert(y, SMS);
                    SMS.FirstSetup();
                    OnMissionChange();
                    selectedStep = null;
                    LastHeldElement = null;
                    interacted = true;
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        private static SubMissionStep lastHoveredActive = null;
        private static void GUITimeTableSingleSteps(List<SubMissionStep> iterate, 
            ref bool interacted, ref int y)
        {
            lastHoveredActive = null;
            for (int step = 0; step < iterate.Count; step++)
            {
                var item = iterate[step];
                GUILayout.BeginVertical(AltUI.TextfieldBlackHuge, GUILayout.MinHeight(55));
                GUIStyle style;
                if (mission.CanRunStep(item.ProgressID))
                {
                    if (item == selectedStep)
                        style = AltUI.ButtonBlueActive;
                    else
                        style = AltUI.ButtonBlue;
                }
                else
                {
                    if (item == selectedStep)
                        style = AltUI.ButtonRedActive;
                    else
                        style = AltUI.ButtonRed;
                }

                GUILayout.BeginHorizontal();

                if (item.ProgressID == SubMission.alwaysRunValue)
                    SubMissionStep.ShowStringColorGUI(item.StepType, "Any");
                else
                    SubMissionStep.ShowStringColorGUI(item.StepType, item.ProgressID.ToString());

                if (GUILayout.Button(item.StepType.ToString(), style, GUILayout.Height(40)))
                {
                    if (HeldElementInst != null)
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
                        iterate.Insert(y, HeldElementInst);
                        OnMissionChange();
                        HeldElementInst = null;
                        selectedStep = null;
                        LastHeldElement = null;
                    }
                    else if (LastHeldElement != null)
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
                        SubMissionStep SMS = new SubMissionStep
                        {
                            StepType = (SMStepType)Enum.Parse(typeof(SMStepType), LastHeldElement),
                            Mission = mission,
                            ProgressID = 0,
                            InitPosition = new Vector3(0, 0, 0),
                            VaribleType = EVaribleType.None,
                        };
                        iterate.Insert(y, SMS);
                        SMS.FirstSetup();
                        OnMissionChange();
                        selectedStep = null;
                        LastHeldElement = null;
                    }
                    else
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Select);
                        if (selectedStep != item)
                            selectedStep = item;
                        else
                            selectedStep = null;
                    }
                    interacted = true;
                }
                if (Event.current.type == EventType.Repaint)
                {
                    var lastWindow = GUILayoutUtility.GetLastRect();
                    if (lastWindow.Contains(Event.current.mousePosition))
                    {   // Preview mission step in the world as we hover over it!
                        lastHoveredActive = selectedStep;
                    }
                }

                GUILayout.EndHorizontal();


                if (Event.current.type == EventType.Repaint)
                {
                    var hoverRect = GUILayoutUtility.GetLastRect();

                    if (HeldElementInst != null || LastHeldElement != null)
                    {
                        if (hoverRect.Contains(Event.current.mousePosition))
                        {
                            GUI.Box(new Rect(hoverRect.x, hoverRect.y - 10, hoverRect.width, 15),
                                string.Empty, AltUI.TextfieldBordered);
                        }
                    }
                }

                if (item.StepType == SMStepType.Folder)
                {
                    if (item.FolderEventList == null)
                        item.FolderEventList = new List<SubMissionStep>();
                    int InsertIndex = 0;
                    GUITimeTableSingleSteps(item.FolderEventList, ref interacted, ref InsertIndex);
                    GUITimeTableSingleAddNew(item.FolderEventList, ref interacted, ref InsertIndex);
                }
                GUILayout.EndVertical();

                y++;
            }
            GUILayout.FlexibleSpace();
        }
        private static bool GUITimeTableRows()
        {
            bool interacted = false;
            lastHoveredActive = null;
            GUILayout.BeginHorizontal();
            scrollTable = GUILayout.BeginScrollView(scrollTable);

            int count = mission.EventList.Count + 1;
            int[] intArray = StepsByTier.Keys.ToArray();
            int lower = Mathf.Min(intArray) - 1;
            int higher = Mathf.Max(intArray) + 1;

            if (count != emptyStringFillers.Length)
            {
                Array.Resize(ref emptyStringFillers, count);
                for (int i = 0; i < count; i++)
                {
                    emptyStringFillers[i] = emptyString;
                }
            }

            for (int x = lower; x < higher; x++)
            {
                if (StepsByTier.TryGetValue(x, out var col))
                {
                    GUILayout.BeginVertical();
                    for (int y = 0; y < count; y++)
                    {
                        if (col.TryGetValue(y, out SubMissionStep val))
                        {
                            if (GUILayout.Button(val.StepType.ToString(), y == curStep ? AltUI.ButtonGreenActive : AltUI.ButtonGreen))
                            {
                                if (LastHeldElement != null)
                                {
                                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
                                    SubMissionStep SMS = new SubMissionStep
                                    {
                                        StepType = (SMStepType)Enum.Parse(typeof(SMStepType), LastHeldElement),
                                        Mission = mission,
                                        ProgressID = y,
                                        InitPosition = new Vector3(0, 0, 0),
                                        VaribleType = EVaribleType.None,
                                    };
                                    mission.EventList.Insert(y, SMS);
                                    SMS.FirstSetup();
                                    OnMissionChange();
                                    selectedStep = null;
                                    LastHeldElement = null;
                                }
                                else
                                {
                                    if (selectedStep != val)
                                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Select);
                                    selectedStep = val;
                                }
                                interacted = true;
                            }
                            if (Event.current.type == EventType.Repaint)
                            {
                                var lastWindow = GUILayoutUtility.GetLastRect();
                                if (lastWindow.Contains(Event.current.mousePosition))
                                {   // Preview mission step in the world as we hover over it!
                                    lastHoveredActive = selectedStep;
                                }
                            }
                        }
                        else
                        {
                            if (GUILayout.Button(emptyString, AltUI.ButtonGrey))
                            {
                                if (LastHeldElement != null)
                                {
                                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
                                    SubMissionStep SMS = new SubMissionStep
                                    {
                                        StepType = (SMStepType)Enum.Parse(typeof(SMStepType), LastHeldElement),
                                        Mission = mission,
                                        ProgressID = y,
                                        InitPosition = new Vector3(0, 0, 0),
                                        VaribleType = EVaribleType.None,
                                    };
                                    mission.EventList.Insert(y, SMS);
                                    SMS.FirstSetup();
                                    OnMissionChange();
                                    selectedStep = null;
                                    LastHeldElement = null;
                                }
                                else
                                {
                                    if (selectedStep != val)
                                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Select);
                                    selectedStep = val;
                                }
                                interacted = true;
                            }
                        }
                    }
                    GUILayout.EndVertical();
                }
                else if (x == SubMission.alwaysRunValue || StepsByTier.ContainsKey(x + 1))
                {
                    GUILayout.BeginVertical();
                    for (int y = 0; y < count; y++)
                    {
                        if (GUILayout.Button(emptyString, AltUI.ButtonGrey))
                        {
                            if (LastHeldElement != null)
                            {
                                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
                                SubMissionStep SMS = new SubMissionStep
                                {
                                    StepType = (SMStepType)Enum.Parse(typeof(SMStepType), LastHeldElement),
                                    Mission = mission,
                                    ProgressID = y,
                                    InitPosition = new Vector3(0, 0, 0),
                                    VaribleType = EVaribleType.None,
                                };
                                mission.EventList.Insert(y, SMS);
                                SMS.FirstSetup();
                                OnMissionChange();
                                selectedStep = null;
                                LastHeldElement = null;
                                interacted = true;
                            }
                        }
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndScrollView();

            GUILayout.EndHorizontal();
            return interacted;
        }

        private static Vector2 scrollDetails = Vector2.zero;
        private static void GUIStepDetails()
        {
            GUILayout.Label("Details", AltUI.WindowHeaderBlue, GUILayout.Height(32));
            scrollDetails = GUILayout.BeginScrollView(scrollDetails);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Move", AltUI.ButtonBlue))
            {
                HeldSomething = true;
                HeldElementInst = selectedStep;
                HeldElement = null;
                LastHeldElement = null;
                mission.RemoveEvent(selectedStep);
            }
            if (GUILayout.Button("Copy", AltUI.ButtonBlue))
            {
                var clone = selectedStep.CloneDeep();
                mission.EventList.Add(clone);
                HeldElementInst = clone;
            }
            if (GUILayout.Button("Close", AltUI.ButtonGrey))
            {
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Close);
                selectedStep = null;
            }
            if (GUILayout.Button("Delete", AltUI.ButtonRed))
            {
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.PaintTechSkin);
                mission.RemoveEvent(selectedStep);
                OnMissionChange();
                selectedStep = null;
            }
            GUILayout.EndHorizontal();

            if (selectedStep != null)
                selectedStep.ShowGUI();

            GUILayout.EndScrollView();
        }
        internal static void GUIWorldDisplayUpdate()
        {
            if (ShowAllStepsInWorld)
            {
                DebugExtUtilities.DrawDirIndicator(mission.ScenePosition, mission.ScenePosition + new Vector3(0, 16, 0),
                    Color.magenta, Time.deltaTime + 0.01f);
                DebugExtUtilities.DrawDirIndicatorCircle(mission.ScenePosition + new Vector3(0, 16, 0),
                    Vector3.up, Vector3.forward, mission.GetMinimumLoadRange(),
                    Color.magenta, Time.deltaTime + 0.01f);
                foreach (var item in mission.GetAllEventsLinear())
                {
                    item.Update();
                }
            }
            else if (selectedStep != null)
                selectedStep.Update();
            if (lastHoveredActive != null)
                lastHoveredActive.Update();
        }


        // -------------------------------------------------------------------------- 
        private static bool validNew = false;
        private static bool delta = false;

        private static MissionChecklist selectedChecker = null;
        private static Vector2 scrollDrag2 = Vector2.zero;
        private static bool addChecklist = false;
        private static void GUIChecklistBar()
        {
            LBarTitle = "Details";

            SMAutoFill.OneWayButtonLarge("Add New", ref addChecklist);

            if (mission.CheckList != null)
            {
                scrollDrag2 = GUILayout.BeginScrollView(scrollDrag2);
                for (int step = 0; step < mission.CheckList.Count; step++)
                {
                    var item = mission.CheckList[step];
                    if (item == selectedChecker)
                    {
                        GUILayout.Button(item.ListArticle, AltUI.ButtonBlueActive, GUILayout.Height(32));
                    }
                    else if (GUILayout.Button(item.ListArticle, AltUI.ButtonBlue, GUILayout.Height(32)))
                    {
                        selectedChecker = item;
                        addChecklist = false;
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Open);
                    }
                }
                GUILayout.EndScrollView();
            }
            else
            {
                if (GUILayout.Button("Start New\nChecklist", AltUI.ButtonGreen, GUILayout.Height(64)))
                {
                    mission.CheckList = new List<MissionChecklist>();
                }
            }
        }
        private static void GUIChecklistInfo()
        {
            if (mission != null)
            {
                try
                {
                    if (addChecklist)
                        GUIChecklistInfoNew();
                    else
                        GUIChecklistInfoExisting();
                }
                catch (ExitGUIException e) { throw e; }
                catch (Exception e)
                {
                    mission = null;
                    //GUIUtility.ExitGUI();
                    throw new Exception("Error in GUIChecklistInfo()", e);
                }
            }
            else
            {
                GUILayout.Box("Not Selected", AltUI.WindowHeaderBlue, GUILayout.Height(32));
            }
        }

        private static string newChecklist = "";
        private static void GUIChecklistInfoNew()
        {
            GUILayout.Box("New Checklist", AltUI.WindowHeaderBlue, GUILayout.Height(32));
            if (delta || SMAutoFill.AutoTextField("Name", ref newChecklist, 64))
            {
                delta = false;
                if (newChecklist.NullOrEmpty() ||  newChecklist.Length < 3)
                    validNew = false;
                else
                    validNew = true;
            }
            if (validNew)
            {
                if (GUILayout.Button("Create", AltUI.ButtonOrangeLarge, GUILayout.Height(128)))
                {
                    validNew = false;
                    mission.CheckList.Add(new MissionChecklist()
                    {
                        mission = mission,
                        ListArticle = newChecklist,
                        BoolToEnable = 0,
                        GlobalIndex = 0,
                        GlobalIndex2 = 0,
                        ValueType = VarType.Unset,
                    });
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Craft);
                }
            }
            else if (GUILayout.Button("Create", AltUI.ButtonGreyLarge, GUILayout.Height(128))) { }
        }

        private static Vector2 scrollDragI = Vector2.zero;
        private static bool openI = false;
        private static string setCache1 = "";
        private static string setCache2 = "";
        private static void GUIChecklistInfoExisting()
        {
            if (selectedChecker != null)
            {
                GUILayout.Box("Information", AltUI.WindowHeaderBlue, GUILayout.Height(32));
                SMAutoFill.AutoVarBoolField("Enabled Bool Variable", false, mission,
                    ref setCache1, ref selectedChecker.BoolToEnable);
                SMAutoFill.AutoTextField("Caption", ref selectedChecker.ListArticle);
                selectedChecker.ValueType = (VarType)SMAutoFill.AutoFixedOptions("Condition", (int)selectedChecker.ValueType, 
                    ref scrollDragI, ref openI, Enum.GetNames(typeof(VarType)));
                switch (selectedChecker.ValueType)
                {
                    case VarType.Bool:
                        GUILayout.BeginHorizontal();
                        if (selectedChecker.GlobalIndex >= 0 && selectedChecker.GlobalIndex < mission.VarTrueFalseActive.Count)
                        {
                            GUILayout.Label("Boolean");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("State: ");
                            if (mission.VarTrueFalseActive[selectedChecker.GlobalIndex])
                                GUILayout.Label("<color=green><b>✓</b></color>", AltUI.TextfieldBordered);
                            else
                                GUILayout.Label("<color=red><b>X</b></color>", AltUI.TextfieldBordered);
                        }
                        else
                        {
                            GUILayout.Label("Boolean");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("State:");
                            GUILayout.Label("<color=yellow><b>~</b></color>", AltUI.TextfieldBordered);
                        }
                        GUILayout.EndHorizontal();
                        SMAutoFill.AutoVarBoolField("Bool Variable", false, mission,
                            ref setCache1, ref selectedChecker.GlobalIndex);
                        break;
                    case VarType.IntOverInt:
                        GUILayout.BeginHorizontal();
                        if (selectedChecker.GlobalIndex >= 0 && selectedChecker.GlobalIndex < mission.VarIntsActive.Count &&
                            selectedChecker.GlobalIndex2 >= 0 && selectedChecker.GlobalIndex2 < mission.VarIntsActive.Count)
                        {
                            if (mission.VarIntsActive[selectedChecker.GlobalIndex] >= mission.VarIntsActive[selectedChecker.GlobalIndex2])
                            {
                                GUILayout.Label(mission.VarIntsActive[selectedChecker.GlobalIndex].ToString());
                                GUILayout.Label(" <color=green><b>>=</b></color> ");
                                GUILayout.Label(mission.VarIntsActive[selectedChecker.GlobalIndex2].ToString());
                                GUILayout.FlexibleSpace();
                                GUILayout.Label("State: <color=green><b>✓</b></color>", AltUI.TextfieldBordered);
                            }
                            else
                            {
                                GUILayout.Label(mission.VarIntsActive[selectedChecker.GlobalIndex].ToString());
                                GUILayout.Label(" <color=red><b>>=</b></color> ");
                                GUILayout.Label(mission.VarIntsActive[selectedChecker.GlobalIndex2].ToString());
                                GUILayout.FlexibleSpace();
                                GUILayout.Label("State: <color=red><b>X</b></color>", AltUI.TextfieldBordered);
                            }
                        }
                        else
                        {
                            GUILayout.Label("Greater Or Equal");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("State: <color=yellow><b>~</b></color>");
                        }
                        GUILayout.EndHorizontal();
                        SMAutoFill.AutoVarIntField("Bool Variable", mission, 
                            ref setCache1, ref selectedChecker.GlobalIndex);
                        SMAutoFill.AutoVarIntField("Bool Variable", mission,
                            ref setCache2, ref selectedChecker.GlobalIndex2);
                        break;
                    default:
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("State:");
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("<color=green><b>✓</b></color>");
                        GUILayout.EndHorizontal();
                        break;
                }

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove", AltUI.ButtonRed))
                {
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.SendToInventory);
                    mission.CheckList.Remove(selectedChecker);
                    selectedChecker = null;
                }
            }
            else
            {
                GUILayout.Box("Not Selected", AltUI.WindowHeaderBlue, GUILayout.Height(32));
            }
        }



        // -------------------------------------------------------------------------- 
        internal enum ERewardType
        {
            CorpToGiveEXP,
            EXPGain,
            MoneyGain,
            AddProgressX,
            AddProgressY,
            RandomBlocksToSpawn,
            BlocksToSpawn,
        }
        private static ERewardType rewardType = ERewardType.CorpToGiveEXP;
        private static Vector2 scrollDrag3 = Vector2.zero;
        private static void GUIRewardsBar() { LBarTitle = "Details"; }

        private static bool openedCorps = false;
        private static Vector2 scrollDragCorps = Vector2.zero;
        private static string EXPTextFieldCache = "";
        private static string MoneyTextFieldCache = "";
        private static string XTextFieldCache = "";
        private static string YTextFieldCache = "";
        private static string randBlocksCache = "";
        private static bool openedBlocks = false;
        private static void GUIRewardsInfo()
        {
            if (mission != null)
            {
                try
                {
                    GUILayout.Box("Information", AltUI.WindowHeaderBlue, GUILayout.Height(32));
                    SubMissionReward rewards = mission.Rewards;

                    if (rewards != null)
                    {
                        if (mission.Tree != null)
                        {
                            scrollDrag3 = GUILayout.BeginScrollView(scrollDrag3);

                            SMAutoFill.AutoFixedOptions("Corporation", ref rewards.CorpToGiveEXP, ref scrollDragCorps,
                                ref openedCorps, ManSMCCorps.AllCorpNames);
                            SMAutoFill.AutoTextField("EXP Rewarded", ref EXPTextFieldCache, ref rewards.EXPGain);
                            SMAutoFill.AutoTextField("Money Rewarded", ref MoneyTextFieldCache, ref rewards.MoneyGain);
                            SMAutoFill.AutoTextField(mission.Tree.ProgressXName, ref XTextFieldCache, ref rewards.AddProgressX);
                            SMAutoFill.AutoTextField(mission.Tree.ProgressYName, ref YTextFieldCache, ref rewards.AddProgressY);
                            SMAutoFill.AutoTextField("Number of Random Blocks", ref randBlocksCache, ref rewards.RandomBlocksToSpawn);
                            SMAutoFill.AutoTextFields("Blocks To Spawn", ref openedBlocks, rewards.BlocksToSpawn);

                            GUILayout.EndScrollView();
                            GUILayout.FlexibleSpace();
                        }
                        else
                            GUILayout.Box("Tree is missing", AltUI.ButtonOrangeLargeActive, GUILayout.Height(64));
                    }
                    else
                    {
                        GUILayout.Box("RELOADING...", AltUI.ButtonOrangeLargeActive, GUILayout.Height(64));
                    }
                }
                catch (ExitGUIException e) { throw e; }
                catch (Exception e)
                {
                    mission = null;
                    //GUIUtility.ExitGUI();
                    throw new Exception("Error in GUIRewardsInfo()", e);
                }
            }
            else
            {
                GUILayout.Box("Not Selected", AltUI.WindowHeaderBlue, GUILayout.Height(32));
            }
        }



        // -------------------------------------------------------------------------- 
        private static void GUIInformationBar()
        {
            LBarTitle = "Information";

            if (GUILayout.Button("Variables", mgType == MissionGUIType.Variables ? AltUI.ButtonBlueActive : AltUI.ButtonBlue, 
                GUILayout.Height(32)))
                mgType = MissionGUIType.Variables;
            else if (GUILayout.Button("Tracked Blocks", mgType == MissionGUIType.TrackedBlocks ? AltUI.ButtonBlueActive : AltUI.ButtonBlue, 
                GUILayout.Height(32)))
                mgType = MissionGUIType.TrackedBlocks;
            else if (GUILayout.Button("Tracked Techs", mgType == MissionGUIType.TrackedTechs ? AltUI.ButtonBlueActive : AltUI.ButtonBlue, 
                GUILayout.Height(32)))
                mgType = MissionGUIType.TrackedTechs;
            else if (GUILayout.Button("Tracked Monuments", mgType == MissionGUIType.TrackedMonuments ? AltUI.ButtonBlueActive : AltUI.ButtonBlue, 
                GUILayout.Height(32)))
                mgType = MissionGUIType.TrackedMonuments;
        }

        private static Vector2 GUIVariablesBoolScroll = Vector2.zero;
        private static Vector2 GUIVariablesIntScroll = Vector2.zero;
        private static void RemoveAllUnusedVariablesBools()
        {
            List<int> downShiftIndex = new List<int>();
            for (int i = 0; i < mission.VarTrueFalse.Count; i++)
            {
                bool remove = true;
                // We check every step to make sure our value is assigned at least once
                foreach (var item in mission.GetAllEventsLinear())
                {
                    if ((item.VaribleType == EVaribleType.True ||
                        item.VaribleType == EVaribleType.False) &&
                        (item.SetMissionVarIndex1 == i ||
                        item.SetMissionVarIndex2 == i ||
                        item.SetMissionVarIndex3 == i))
                    {
                        remove = false;
                        break;
                    }
                }
                // If there are no assignments, we remove this variable, and mark all items
                //   that may use this to go down one index
                if (remove)
                    downShiftIndex.Add(i);
            }
            int count = downShiftIndex.Count;
            if (count > 0)
            { 
                // We step down every step index to reflect that we removed a variable below this
                foreach (var item in mission.GetAllEventsLinear())
                {
                    if (item.VaribleType == EVaribleType.True ||
                        item.VaribleType == EVaribleType.False)
                    {
                        for (int i = count - 1; i >= 0; i++)
                        {
                            int val = downShiftIndex[i];
                            if (item.SetMissionVarIndex1 >= val)
                                item.SetMissionVarIndex1--;
                            if (item.SetMissionVarIndex2 >= val)
                                item.SetMissionVarIndex2--;
                            if (item.SetMissionVarIndex3 >= val)
                                item.SetMissionVarIndex3--;
                        }
                    }
                }
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Open);
                for (int i = count - 1; i >= 0; i++)
                {
                    mission.VarTrueFalse.RemoveAt(downShiftIndex[i]);
                    mission.VarTrueFalseActive.RemoveAt(downShiftIndex[i]);
                }
                downShiftIndex.Clear();
            }
            else
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
        }
        private static void RemoveAllUnusedVariablesVals()
        {
            List<int> downShiftIndex = new List<int>();
            for (int i = 0; i < mission.VarInts.Count; i++)
            {
                bool remove = true;
                // We check every step to make sure our value is assigned at least once
                foreach (var item in mission.GetAllEventsLinear())
                {
                    if ((item.VaribleType == EVaribleType.Int ||
                        item.VaribleType == EVaribleType.IntLessThan ||
                        item.VaribleType == EVaribleType.IntGreaterThan) &&
                        (item.SetMissionVarIndex1 == i || 
                        item.SetMissionVarIndex2 == i ||
                        item.SetMissionVarIndex3 == i))
                    {
                        remove = false;
                        break;
                    }
                }
                // If there are no assignments, we remove this variable, and mark all items
                //   that may use this to go down one index
                if (remove)  // We didn't find any uses here, we remove
                    downShiftIndex.Add(i);
            }
            int count = downShiftIndex.Count;
            if (count > 0)
            {
                // We step down every step index to reflect that we removed a variable below this
                foreach (var item in mission.GetAllEventsLinear())
                {
                    if (item.VaribleType == EVaribleType.Int ||
                        item.VaribleType == EVaribleType.IntLessThan ||
                        item.VaribleType == EVaribleType.IntGreaterThan)
                    {
                        for (int i = count - 1; i >= 0; i++)
                        {
                            int val = downShiftIndex[i];
                            if (item.SetMissionVarIndex1 >= val)
                                item.SetMissionVarIndex1--;
                            if (item.SetMissionVarIndex2 >= val)
                                item.SetMissionVarIndex2--;
                            if (item.SetMissionVarIndex3 >= val)
                                item.SetMissionVarIndex3--;
                        }
                    }
                }
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Open);
                for (int i = count - 1; i >= 0; i++)
                {
                    mission.VarTrueFalse.RemoveAt(downShiftIndex[i]);
                    mission.VarTrueFalseActive.RemoveAt(downShiftIndex[i]);
                }
                downShiftIndex.Clear();
            }
            else
                ManSFX.inst.PlayUISFX(ManSFX.UISfxType.CheckBox);
        }
        private static void GUIVariablesInfo()
        {
            if (mission != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.Width(360));
                GUILayout.Box("Booleans", AltUI.WindowHeaderBlue, GUILayout.Height(32));
                if (GUILayout.Button("Remove Unused", AltUI.ButtonRed))
                    RemoveAllUnusedVariablesBools();
                GUIVariablesBoolScroll = GUILayout.BeginScrollView(GUIVariablesBoolScroll);
                for (int step = 0; step < mission.VarTrueFalseActive.Count; step++)
                {
                    var item = mission.VarTrueFalseActive[step];
                    GUILayout.BeginHorizontal(GUILayout.Height(32));
                    GUILayout.Label("Index: ");
                    GUILayout.Label(step.ToString());
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Start: ");
                    GUILayout.Label(mission.VarTrueFalse[step].ToString());
                    GUILayout.Label("Active: ");
                    GUILayout.Label(item.ToString());
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Box("Integers", AltUI.WindowHeaderBlue, GUILayout.Height(32));
                if (GUILayout.Button("Remove Unused", AltUI.ButtonRed))
                    RemoveAllUnusedVariablesVals();
                GUIVariablesIntScroll = GUILayout.BeginScrollView(GUIVariablesIntScroll);
                for (int step = 0; step < mission.VarIntsActive.Count; step++)
                {
                    var item = mission.VarIntsActive[step];
                    GUILayout.BeginHorizontal(GUILayout.Height(32));
                    GUILayout.Label("Index: ");
                    GUILayout.Label(step.ToString());
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Start: ");
                    GUILayout.Label(mission.VarInts[step].ToString());
                    GUILayout.Label("Active: ");
                    GUILayout.Label(item.ToString());
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Box("Booleans", AltUI.WindowHeaderBlue, GUILayout.Height(32));
                GUILayout.Box("Integers", AltUI.WindowHeaderBlue, GUILayout.Height(32));
                GUILayout.EndHorizontal();
                GUILayout.Label("Mission is somehow NULL", ManModGUI.styleLabelLargerFont);
            }
        }

        private static void GUITrackedBlocksInfo()
        {
            GUILayout.Box("Tracked Blocks", AltUI.WindowHeaderBlue, GUILayout.Height(32));
            if (mission != null)
            {
                if (mission.TrackedBlocks != null)
                {
                    GUIVariablesBoolScroll = GUILayout.BeginScrollView(GUIVariablesBoolScroll);
                    for (int step = 0; step < mission.TrackedBlocks.Count; step++)
                    {
                        var item = mission.TrackedBlocks[step];
                        if (item != null)
                        {
                            GUILayout.BeginHorizontal(GUILayout.Height(32));
                            GUILayout.Label("[");
                            GUILayout.Label(step.ToString());
                            GUILayout.Label("] ");
                            GUILayout.Label("Name: ");
                            GUILayout.Label(item.BlockName.NullOrEmpty() ? "NULL_NAME" : item.BlockName);
                            GUILayout.FlexibleSpace();
                            if (item.blockInst != null)
                            {
                                GUILayout.Label("Health: ");
                                GUILayout.Label(item.blockInst.visible.damageable.Health.ToString("F"));
                                GUILayout.Label("  Pos: ");
                                GUILayout.Label(item.blockInst.trans.position.ToString("F"));
                            }
                            else
                                GUILayout.Label("Inactive");
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.EndScrollView();
                }
                else
                    GUILayout.Label("No Tracked Blocks?", ManModGUI.styleLabelLargerFont);
            }
            else
            {
                GUILayout.Label("Mission is somehow NULL", ManModGUI.styleLabelLargerFont);
            }
        }

        private static void GUITrackedTechsInfo()
        {
            GUILayout.Box("Tracked Techs", AltUI.WindowHeaderBlue, GUILayout.Height(32));
            if (mission != null)
            {
                if (mission.TrackedTechs != null)
                {
                    GUIVariablesBoolScroll = GUILayout.BeginScrollView(GUIVariablesBoolScroll);
                    for (int step = 0; step < mission.TrackedTechs.Count; step++)
                    {
                        var item = mission.TrackedTechs[step];
                        if (item != null)
                        {
                            GUILayout.BeginHorizontal(GUILayout.Height(32));
                            GUILayout.Label("[");
                            GUILayout.Label(step.ToString());
                            GUILayout.Label("] ");
                            GUILayout.Label("Name: ");
                            GUILayout.Label(item.TechName.NullOrEmpty() ? "NULL_NAME" : item.TechName);
                            GUILayout.FlexibleSpace();
                            if (item.Tech != null)
                            {
                                GUILayout.Label("Temp: ");
                                GUILayout.Label(item.isDisposible.ToString());
                                GUILayout.Label("  Team: ");
                                GUILayout.Label(item.Tech.Team.ToString());
                                GUILayout.Label("  Pos: ");
                                GUILayout.Label(item.Tech.trans.position.ToString("F"));
                            }
                            else
                                GUILayout.Label("Inactive");
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.EndScrollView();
                }
                else
                    GUILayout.Label("No Tracked Techs?", ManModGUI.styleLabelLargerFont);
            }
            else
            {
                GUILayout.Label("Mission is somehow NULL", ManModGUI.styleLabelLargerFont);
            }
        }

        private static void GUITrackedMonumentsInfo()
        {
            GUILayout.Box("Tracked Monuments", AltUI.WindowHeaderBlue, GUILayout.Height(32));
            if (mission != null)
            {
                if (mission.TrackedTechs != null)
                {
                    GUIVariablesBoolScroll = GUILayout.BeginScrollView(GUIVariablesBoolScroll);
                    for (int step = 0; step < mission.TrackedMonuments.Count; step++)
                    {
                        var item = mission.TrackedMonuments[step];
                        if (item != null)
                        {
                            GUILayout.BeginHorizontal(GUILayout.Height(32));
                            GUILayout.Label("[");
                            GUILayout.Label(step.ToString());
                            GUILayout.Label("] ");
                            GUILayout.Label("Name: ");
                            GUILayout.Label(item.Name.NullOrEmpty() ? "NULL_NAME" : item.Name);
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("Aimed Height: ");
                            GUILayout.Label(item.aimedHeight.ToString());
                            GUILayout.Label("  Pos: ");
                            GUILayout.Label(item.transform.position.ToString("F"));
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.EndScrollView();
                }
                else
                    GUILayout.Label("No Tracked Monuments?", ManModGUI.styleLabelLargerFont);
            }
            else
            {
                GUILayout.Label("Mission is somehow NULL", ManModGUI.styleLabelLargerFont);
            }
        }
    }
}
