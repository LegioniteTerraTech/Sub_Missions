using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.ManWindows
{
    public class GUISMissionsList : IGUIFormat
    {
        public GUIPopupDisplay Display { get; set; }
        private Vector2 scrolll = new Vector2(0, 0);
        public float scrolllSize = 50;
        public Texture CachedPic;
        private bool showAnon = true;
        private bool showActive = true;

        private const int buttonSideSpacing = 40;
        private const int buttonWidth = 350;

        public void Setup(GUIPopupDisplay display)
        {
            Display = display;
            ManSubMissions.Board = this;
        }
        public void RunGUI(int ID)
        {
            scrolll = GUI.BeginScrollView(new Rect(20, 60, (Display.Window.width / 2.1f), Display.Window.height - 120), scrolll, new Rect(0, 0, (Display.Window.width / 3), scrolllSize ));
            int posLerp = 0;
            if (ManSubMissions.AnonSubMissions.Count > 0)
            {
                if (GUI.Button(new Rect(buttonSideSpacing, (posLerp * 55) + 20, buttonWidth, 50), "<b>Sub Mission Board</b>", WindowManager.styleHugeFont))
                {
                    showAnon = !showAnon;
                }
                posLerp++;
                if (showAnon)
                {
                    foreach (SubMissionStandby active in ManSubMissions.AnonSubMissions)
                    {
                        string buttonText;
                        if (active == ManSubMissions.SelectedAnon)
                        {
                            buttonText = "<b><color=#f23d3dff>" + active.AltName + "</color></b>";
                        }
                        else
                            buttonText = "<b>" + active.AltName + "</b>";
                        if (GUI.Button(new Rect(buttonSideSpacing, (posLerp * 55) + 20, buttonWidth, 50), buttonText, WindowManager.styleHugeFont))
                        {   // Select
                            ManSubMissions.SelectedIsAnon = true;
                            ManSubMissions.SelectedAnon = active;
                            ButtonAct.inst.Invoke("UpdateAllWindowsNow", 0);
                        }
                        //GUI.Label(new Rect(40, posLerp * 25, 300, 25), charI.GetCharacterFullName());
                        //GUI.DrawTexture(new Rect(70, (posLerp * 55) + 20, 50, 50), charI.Look);
                        if (GUI.Button(new Rect(buttonSideSpacing + buttonWidth, (posLerp * 55) + 20, 50, 50), "<b>A</b>", WindowManager.styleHugeFont))
                        {   // accept the mission
                            ManSubMissions.SelectedIsAnon = true;
                            ManSubMissions.SelectedAnon = active;
                            ButtonAct.inst.Invoke("AcceptSMission", 0);
                            ButtonAct.inst.Invoke("UpdateAllWindowsNow", 0);
                        }
                        posLerp++;
                    }
                    posLerp++;
                }
            }
            if (GUI.Button(new Rect(buttonSideSpacing, (posLerp * 55) + 20, buttonWidth, 50), "<b>Active Sub Missions</b>", WindowManager.styleHugeFont))
            {
                showActive = !showActive;
            }
            posLerp++;
            if (showActive)
            {
                foreach (SubMission active in ManSubMissions.ActiveSubMissions)
                {
                    string buttonText;
                    if (active == ManSubMissions.Selected)
                    {
                        buttonText = "<b><color=#f23d3dff>" + active.SelectedAltName + "</color></b>";
                    }
                    else
                        buttonText = "<b>" + active.SelectedAltName + "</b>";
                    if (GUI.Button(new Rect(buttonSideSpacing, (posLerp * 55) + 20, buttonWidth, 50), buttonText, WindowManager.styleHugeFont))
                    {   // Select
                        ManSubMissions.SelectedIsAnon = false;
                        ManSubMissions.Selected = active;
                        ButtonAct.inst.Invoke("UpdateAllWindowsNow", 0);
                    }
                    //GUI.Label(new Rect(40, posLerp * 25, 300, 25), charI.GetCharacterFullName());
                    //GUI.DrawTexture(new Rect(70, (posLerp * 55) + 20, 50, 50), charI.Look);
                    if ((active.MissionDist >= ManSubMissions.MaxLoadedSpawnDist && active.Type != SubMissionType.Immedeate) || KickStart.OverrideRestrictions)
                    {
                        if (GUI.Button(new Rect(buttonSideSpacing + buttonWidth, (posLerp * 55) + 20, 50, 50), "<b>D</b>", WindowManager.styleHugeFont))
                        {   // Remove this mission
                            ManSubMissions.SelectedIsAnon = false;
                            ManSubMissions.Selected = active;
                            ButtonAct.inst.Invoke("RequestCancelSMission", 0);
                            ButtonAct.inst.Invoke("UpdateAllWindowsNow", 0);
                        }
                    }
                    posLerp++;
                }
            }
            GUI.EndScrollView();
            float height = (posLerp * 55) + 20;
            scrolllSize = height;

            GUI.Label(new Rect((Display.Window.width / 2) + 20, 60, 20, (Display.Window.height) - 120), "", WindowManager.styleHugeFont);// DIVIDER
            
            if (GUI.Button(new Rect((Display.Window.width / 6) - 70, Display.Window.height - 60, 140, 40), "<b>CHECK</b>", WindowManager.styleHugeFont))
            {
                ButtonAct.inst.Invoke("UpdateMissions", 0);
            }
            //if (int.TryParse(GUI.TextField(new Rect(((Display.Window.width / 6) * 3) - 70, Display.Window.height - 60, 140, 40), Core.BribeValue.ToString()), out int result))
            //    Core.BribeValue = result;
            if (GUI.Button(new Rect(((Display.Window.width / 6) * 2) - 50, Display.Window.height - 60, 140, 40), "<b>CLOSE</b>", WindowManager.styleHugeFont))
            {
                Singleton.Manager<ManSFX>.inst.PlayUISFX(ManSFX.UISfxType.Close);
                WindowManager.HidePopup(Display);
            }

            //--------------------------------------------------------------------
            //                          BEGIN info siding
            try
            {
                if (ManSubMissions.SelectedIsAnon)
                {
                    //GUI.DrawTexture(new Rect((Display.Window.width / 2) + 100, 40, 240, 240),);
                    GUI.Label(new Rect((Display.Window.width / 2) + 60, 200, 360, 40), "<b>" + ManSubMissions.SelectedAnon.AltName + "</b>", WindowManager.styleDescLargeFont);
                    GUI.Label(new Rect((Display.Window.width / 2) + 60, 240, 360, 40), "<b>Distance: Waiting for request...</b>", WindowManager.styleDescLargeFont);
                    GUI.Label(new Rect((Display.Window.width / 2) + 60, 280, 180, 20), "<b>Faction: " + ManSubMissions.SelectedAnon.Faction + "</b>", WindowManager.styleDescFont);
                    GUI.Label(new Rect((Display.Window.width / 2) + 240, 280, 180, 20), "<b>Grade: " + ManSubMissions.SelectedAnon.GradeRequired + "</b>", WindowManager.styleDescFont);
                    GUI.Label(new Rect((Display.Window.width / 2) + 60, 300, 360, 140), ManSubMissions.SelectedAnon.Desc, WindowManager.styleDescFont);
                    if (ManSubMissions.SelectedAnon.Rewards != null)
                    {
                        SubMissionReward Rewards = ManSubMissions.SelectedAnon.Rewards;
                        GUI.Label(new Rect((Display.Window.width / 2) + 60, 440, 360, 25), "----Rewards----", WindowManager.styleDescFont);
                        if (Rewards.EXPGain > 0)
                            GUI.Label(new Rect((Display.Window.width / 2) + 60, 465, 360, 25), "EXP: " + Rewards.EXPGain, WindowManager.styleDescFont);
                        else
                            GUI.Label(new Rect((Display.Window.width / 2) + 60, 465, 360, 25), "No EXP", WindowManager.styleDescFont);

                        if (Rewards.MoneyGain > 0)
                            GUI.Label(new Rect((Display.Window.width / 2) + 60, 490, 360, 25), "¥¥: " + Rewards.MoneyGain, WindowManager.styleDescFont);
                        else
                            GUI.Label(new Rect((Display.Window.width / 2) + 60, 490, 360, 25), "No ¥¥", WindowManager.styleDescFont);

                        int blocks;
                        if (Rewards.BlocksToSpawn != null)
                            blocks = Rewards.RandomBlocksToSpawn + Rewards.BlocksToSpawn.Count;
                        else
                            blocks = Rewards.RandomBlocksToSpawn;
                        if (blocks > 0)
                            GUI.Label(new Rect((Display.Window.width / 2) + 60, 515, 360, 25), "Blocks: " + blocks, WindowManager.styleDescFont);
                        else
                            GUI.Label(new Rect((Display.Window.width / 2) + 60, 515, 360, 25), "No Blocks", WindowManager.styleDescFont);
                        if (Rewards.AddProgressX > 0)
                            GUI.Label(new Rect((Display.Window.width / 2) + 60, 540, 180, 25), ManSubMissions.SelectedAnon.Tree.ProgressXName + ": " + Rewards.AddProgressX, WindowManager.styleDescFont);
                        if (Rewards.AddProgressY > 0)
                            GUI.Label(new Rect((Display.Window.width / 2) + 240, 540, 180, 25), ManSubMissions.SelectedAnon.Tree.ProgressYName + ": " + Rewards.AddProgressY, WindowManager.styleDescFont);
                    }
                }
                else if (!ManSubMissions.SelectedIsAnon)
                {
                    //GUI.DrawTexture(new Rect((Display.Window.width / 2) + 100, 40, 240, 240),);
                    GUI.Label(new Rect((Display.Window.width / 2) + 60, 200, 360, 40), "<b>" + ManSubMissions.Selected.SelectedAltName + "</b>", WindowManager.styleDescLargeFont);
                    GUI.Label(new Rect((Display.Window.width / 2) + 60, 240, 360, 40), "<b>Distance: " + ManSubMissions.Selected.MissionDist + "</b>", WindowManager.styleDescLargeFont);
                    GUI.Label(new Rect((Display.Window.width / 2) + 60, 280, 180, 20), "<b>Faction: " + ManSubMissions.Selected.Faction + "</b>", WindowManager.styleDescFont);
                    GUI.Label(new Rect((Display.Window.width / 2) + 240, 280, 180, 20), "<b>Grade: " + ManSubMissions.Selected.GradeRequired + "</b>", WindowManager.styleDescFont);
                    GUI.Label(new Rect((Display.Window.width / 2) + 60, 300, 360, 140), ManSubMissions.Selected.Description, WindowManager.styleDescFont);
                    if (ManSubMissions.Selected.Rewards != null)
                    {
                        SubMissionReward Rewards = ManSubMissions.Selected.Rewards;
                        GUI.Label(new Rect((Display.Window.width / 2) + 60, 440, 360, 25), "----Rewards----", WindowManager.styleDescFont);
                        if (Rewards.EXPGain > 0)
                            GUI.Label(new Rect((Display.Window.width / 2) + 60, 465, 360, 25), "EXP: " + Rewards.EXPGain, WindowManager.styleDescFont);
                        else
                            GUI.Label(new Rect((Display.Window.width / 2) + 60, 465, 360, 25), "No EXP", WindowManager.styleDescFont);

                        if (Rewards.MoneyGain > 0)
                            GUI.Label(new Rect((Display.Window.width / 2) + 60, 490, 360, 25), "¥¥: " + Rewards.MoneyGain, WindowManager.styleDescFont);
                        else
                            GUI.Label(new Rect((Display.Window.width / 2) + 60, 490, 360, 25), "No ¥¥", WindowManager.styleDescFont);

                        int blocks;
                        if (Rewards.BlocksToSpawn != null)
                            blocks = Rewards.RandomBlocksToSpawn + Rewards.BlocksToSpawn.Count;
                        else
                            blocks = Rewards.RandomBlocksToSpawn;
                        if (blocks > 0)
                            GUI.Label(new Rect((Display.Window.width / 2) + 60, 515, 360, 25), "Blocks: " + blocks, WindowManager.styleDescFont);
                        else
                            GUI.Label(new Rect((Display.Window.width / 2) + 60, 515, 360, 25), "No Blocks", WindowManager.styleDescFont);
                        if (Rewards.AddProgressX > 0)
                            GUI.Label(new Rect((Display.Window.width / 2) + 60, 540, 180, 25), ManSubMissions.Selected.Tree.ProgressXName + ": " + Rewards.AddProgressX, WindowManager.styleDescFont);
                        if (Rewards.AddProgressY > 0)
                            GUI.Label(new Rect((Display.Window.width / 2) + 240, 540, 180, 25), ManSubMissions.Selected.Tree.ProgressYName + ": " + Rewards.AddProgressY, WindowManager.styleDescFont);
                    }
                }
                else
                {
                    GUI.Label(new Rect((Display.Window.width / 2) + 60, 240, 360, 40), "<b>No mission selected</b>", WindowManager.styleLargeFont);
                }
            }
            catch
            {
                GUI.Label(new Rect((Display.Window.width / 2) + 60, 240, 360, 40), "<b>No mission selected</b>", WindowManager.styleLargeFont);
            }
            GUI.DragWindow();
            WindowManager.KeepWithinScreenBoundsNonStrict(Display);
        }
        public void DelayedUpdate()
        {
            BuildStatus();
        }
        public void FastUpdate()
        {
        }

        public void BuildStatus()
        {
        }
        public void OnRemoval() { }
    }
}
