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
        public Vector2 scrolll = new Vector2(10, 0);
        public Texture CachedPic;

        public void Setup(GUIPopupDisplay display)
        {
            Display = display;
            ManSubMissions.Board = this;
        }
        public void RunGUI(int ID)
        {
            GUI.BeginScrollView(new Rect(40, 40, (Display.Window.width / 2), Display.Window.height - 80), scrolll, new Rect(50, 50, (Display.Window.width / 2), (Display.Window.height / 2)));
            int posLerp = 0;
            if (ManSubMissions.AnonSubMissions.Count > 0)
            {
                if (GUI.Button(new Rect(70 + 50, (posLerp * 55) + 70, 300, 50), "<b>Sub Mission Board</b>", WindowManager.styleHugeFont))
                {
                }
                posLerp++;
                foreach (SubMissionStandby active in ManSubMissions.AnonSubMissions)
                {
                    string buttonText;
                    if (active == ManSubMissions.SelectedAnon)
                    {
                        buttonText = "<b><color=#f23d3dff>" + active.Name + "</color></b>";
                    }
                    else
                        buttonText = "<b>" + active.Name + "</b>";
                    if (GUI.Button(new Rect(70 + 50, (posLerp * 55) + 70, 300, 50), buttonText, WindowManager.styleHugeFont))
                    {   // Select
                        ManSubMissions.SelectedIsAnon = true;
                        ManSubMissions.SelectedAnon = active;
                        ButtonAct.inst.Invoke("UpdateAllWindowsNow", 0);
                    }
                    //GUI.Label(new Rect(40, posLerp * 25, 300, 25), charI.GetCharacterFullName());
                    //GUI.DrawTexture(new Rect(70, (posLerp * 55) + 70, 50, 50), charI.Look);
                    if (GUI.Button(new Rect(420, (posLerp * 55) + 70, 50, 50), "<b>A</b>", WindowManager.styleHugeFont))
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
            if (GUI.Button(new Rect(70 + 50, (posLerp * 55) + 70, 300, 50), "<b>Active Sub Missions</b>", WindowManager.styleHugeFont))
            {   
            }
            posLerp++;
            foreach (SubMission active in ManSubMissions.ActiveSubMissions)
            {
                string buttonText;
                if (active == ManSubMissions.Selected)
                {
                    buttonText = "<b><color=#f23d3dff>" + active.Name + "</color></b>";
                }
                else
                    buttonText = "<b>" + active.Name + "</b>";
                if (GUI.Button(new Rect(70 + 50, (posLerp * 55) + 70, 300, 50), buttonText, WindowManager.styleHugeFont))
                {   // Select
                    ManSubMissions.SelectedIsAnon = false;
                    ManSubMissions.Selected = active;
                    ButtonAct.inst.Invoke("UpdateAllWindowsNow", 0);
                }
                //GUI.Label(new Rect(40, posLerp * 25, 300, 25), charI.GetCharacterFullName());
                //GUI.DrawTexture(new Rect(70, (posLerp * 55) + 70, 50, 50), charI.Look);
                if (GUI.Button(new Rect(420, (posLerp * 55) + 70, 50, 50), "<b>D</b>", WindowManager.styleHugeFont))
                {   // Remove this mission
                    ManSubMissions.SelectedIsAnon = false;
                    ManSubMissions.Selected = active;
                    ButtonAct.inst.Invoke("RequestCancelSMission", 0);
                    ButtonAct.inst.Invoke("UpdateAllWindowsNow", 0);
                }
                posLerp++;
            }
            GUI.EndScrollView();
            scrolll.y = GUI.VerticalScrollbar(new Rect(25, 40, 30, Display.Window.height - 160), scrolll.y, Display.Window.height - 80, 0, posLerp * 55);

            if (GUI.Button(new Rect((Display.Window.width / 2) + 20, 60, 20, (Display.Window.height) - 120), "", WindowManager.styleHugeFont))
            {   // DIVIDER
            }
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
                    GUI.TextField(new Rect((Display.Window.width / 2) + 60, 240, 360, 40), "<b>" + ManSubMissions.SelectedAnon.Name + "</b>", WindowManager.styleLargeFont);
                    GUI.TextField(new Rect((Display.Window.width / 2) + 60, 280, 180, 20), "<b>Faction: " + ManSubMissions.SelectedAnon.Faction + "</b>");
                    GUI.TextField(new Rect((Display.Window.width / 2) + 240, 280, 180, 20), "<b>Grade: " + ManSubMissions.SelectedAnon.GradeRequired + "</b>");
                    GUI.TextField(new Rect((Display.Window.width / 2) + 60, 300, 360, 140), ManSubMissions.SelectedAnon.Desc);
                    if (ManSubMissions.SelectedAnon.Rewards != null)
                    {
                        SubMissionReward Rewards = ManSubMissions.SelectedAnon.Rewards;
                        GUI.TextField(new Rect((Display.Window.width / 2) + 60, 440, 360, 25), "----Rewards----");
                        if (Rewards.EXPGain > 0)
                            GUI.TextField(new Rect((Display.Window.width / 2) + 60, 465, 360, 25), "EXP: " + Rewards.EXPGain );
                        else
                            GUI.TextField(new Rect((Display.Window.width / 2) + 60, 465, 360, 25), "No EXP");

                        if (Rewards.MoneyGain > 0)
                            GUI.TextField(new Rect((Display.Window.width / 2) + 60, 490, 360, 25), "¥¥: " + Rewards.MoneyGain);
                        else
                            GUI.TextField(new Rect((Display.Window.width / 2) + 60, 490, 360, 25), "No ¥¥");

                        int blocks;
                        if (Rewards.BlocksToSpawn != null)
                            blocks = Rewards.RandomBlocksToSpawn + Rewards.BlocksToSpawn.Count;
                        else
                            blocks = Rewards.RandomBlocksToSpawn;
                        if (blocks > 0)
                            GUI.TextField(new Rect((Display.Window.width / 2) + 60, 515, 360, 25), "Blocks: " + blocks);
                        else
                            GUI.TextField(new Rect((Display.Window.width / 2) + 60, 515, 360, 25), "No Blocks");
                        if (Rewards.AddProgressX > 0)
                            GUI.TextField(new Rect((Display.Window.width / 2) + 60, 540, 180, 25), ManSubMissions.SelectedAnon.Tree.ProgressXName + ": " + Rewards.AddProgressX);
                        if (Rewards.AddProgressY > 0)
                            GUI.TextField(new Rect((Display.Window.width / 2) + 240, 540, 180, 25), ManSubMissions.SelectedAnon.Tree.ProgressYName + ": " + Rewards.AddProgressY);
                    }
                }
                else if (!ManSubMissions.SelectedIsAnon)
                {
                    //GUI.DrawTexture(new Rect((Display.Window.width / 2) + 100, 40, 240, 240),);
                    GUI.TextField(new Rect((Display.Window.width / 2) + 60, 240, 360, 40), "<b>" + ManSubMissions.Selected.Name + "</b>", WindowManager.styleLargeFont);
                    GUI.TextField(new Rect((Display.Window.width / 2) + 60, 280, 180, 20), "<b>Faction: " + ManSubMissions.Selected.Faction + "</b>");
                    GUI.TextField(new Rect((Display.Window.width / 2) + 240, 280, 180, 20), "<b>Grade: " + ManSubMissions.Selected.GradeRequired + "</b>");
                    GUI.TextField(new Rect((Display.Window.width / 2) + 60, 300, 360, 140), ManSubMissions.Selected.Description);
                    if (ManSubMissions.Selected.Rewards != null)
                    {
                        SubMissionReward Rewards = ManSubMissions.Selected.Rewards;
                        GUI.TextField(new Rect((Display.Window.width / 2) + 60, 440, 360, 25), "----Rewards----");
                        if (Rewards.EXPGain > 0)
                            GUI.TextField(new Rect((Display.Window.width / 2) + 60, 465, 360, 25), "EXP: " + Rewards.EXPGain);
                        else
                            GUI.TextField(new Rect((Display.Window.width / 2) + 60, 465, 360, 25), "No EXP");

                        if (Rewards.MoneyGain > 0)
                            GUI.TextField(new Rect((Display.Window.width / 2) + 60, 490, 360, 25), "¥¥: " + Rewards.MoneyGain);
                        else
                            GUI.TextField(new Rect((Display.Window.width / 2) + 60, 490, 360, 25), "No ¥¥");

                        int blocks;
                        if (Rewards.BlocksToSpawn != null)
                            blocks = Rewards.RandomBlocksToSpawn + Rewards.BlocksToSpawn.Count;
                        else
                            blocks = Rewards.RandomBlocksToSpawn;
                        if (blocks > 0)
                            GUI.TextField(new Rect((Display.Window.width / 2) + 60, 515, 360, 25), "Blocks: " + blocks);
                        else
                            GUI.TextField(new Rect((Display.Window.width / 2) + 60, 515, 360, 25), "No Blocks");
                        if (Rewards.AddProgressX > 0)
                            GUI.TextField(new Rect((Display.Window.width / 2) + 60, 540, 180, 25), ManSubMissions.Selected.Tree.ProgressXName + ": " + Rewards.AddProgressX);
                        if (Rewards.AddProgressY > 0)
                            GUI.TextField(new Rect((Display.Window.width / 2) + 240, 540, 180, 25), ManSubMissions.Selected.Tree.ProgressYName + ": " + Rewards.AddProgressY);
                    }
                }
                else
                {
                    GUI.TextField(new Rect((Display.Window.width / 2) + 60, 240, 360, 40), "<b>No mission selected</b>", WindowManager.styleLargeFont);
                }
            }
            catch
            {
                GUI.TextField(new Rect((Display.Window.width / 2) + 60, 240, 360, 40), "<b>No mission selected</b>", WindowManager.styleLargeFont);
            }
            GUI.DragWindow();
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
