using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TerraTechETCUtil;
using TAC_AI.AI;
using TAC_AI.Templates;
using TAC_AI.World;
using System.Diagnostics;
using RandomAdditions;
using System.IO;

namespace Sub_Missions
{
    internal class SubMissionsWiki : TinySettings
    {
        public static SubMissionsWiki inst = new SubMissionsWiki();
        public string DirectoryInExtModSettings => KickStart.ModID;
        public bool ShowButtons = false;

        private static string modID => KickStart.ModID;
        private static Sprite nullSprite;

        internal static void InitWiki()
        {
            nullSprite = ManUI.inst.GetSprite(ObjectTypes.Block, -1);
            InitHelpers();
            inst.TryLoadFromDisk(ref inst);
        }
        private static void InitHelpers()
        {
            new WikiPageInfo(modID, "Tools", nullSprite, PageTools);
        }
        internal static void OpenInExplorer(string directory)
        {
            switch (SystemInfo.operatingSystemFamily)
            {
                case OperatingSystemFamily.MacOSX:
                    Process.Start(new ProcessStartInfo("file://" + directory));
                    break;
                case OperatingSystemFamily.Linux:
                case OperatingSystemFamily.Windows:
                    Process.Start(new ProcessStartInfo("explorer.exe", directory));
                    break;
                default:
                    throw new Exception("This operating system is UNSUPPORTED by RandomAdditions");
            }
        }
        internal static void PageTools()
        {
            AltUI.Sprite(nullSprite, AltUI.TextfieldBorderedBlue, GUILayout.Height(128), GUILayout.Width(128));

            GUILayout.BeginVertical(AltUI.TextfieldBlackHuge);
            GUILayout.Label("RawTech Files", AltUI.LabelBlueTitle);

            if (KickStart.Debugger)
            {
                if (GUILayout.Button("Mission Editor", AltUI.ButtonBlueLarge))
                    ManSubMissions.ToggleEditor();
                if (GUILayout.Button("Mission List", AltUI.ButtonBlueLarge))
                    ManSubMissions.ToggleList();
                if (inst.ShowButtons)
                {
                    if (GUILayout.Button("Hide Editor Buttons", AltUI.ButtonBlueLargeActive))
                    {
                        inst.ShowButtons = false;
                        ManSubMissions.UpdateButtonState();
                        inst.TrySaveToDisk();
                    }
                }
                else if (GUILayout.Button("Show Editor Buttons", AltUI.ButtonBlueLarge))
                {
                    inst.ShowButtons = true;
                    ManSubMissions.UpdateButtonState();
                    inst.TrySaveToDisk();
                }
            }
            else
            {
                GUILayout.Button("Mission Editor(Only In Creative/R&D)", AltUI.ButtonGreyLarge);
                GUILayout.Button("Mission List(Only In Creative/R&D)", AltUI.ButtonGreyLarge);
                GUILayout.Button("Show Editor Buttons(Only In Creative/R&D)", AltUI.ButtonBlueLarge);
            }
            GUILayout.EndVertical();
        }


        /*
        internal static LoadingHintsExt.LoadingHint loadHint1 = new LoadingHintsExt.LoadingHint(KickStart.ModID, "ADVANCED AI HINT",
            "Talk to prospectors with " + AltUI.HighlightString("T - Left-Click") + ".\nYou can't talk to " +
            AltUI.SideCharacterString("Prospectors") + " without bases or " + AltUI.EnemyString("Mission Objectives") +
            ".\nBuild Bucks are on the line!");


        // Others
        internal static ExtUsageHint.UsageHint hintADV = new ExtUsageHint.UsageHint(KickStart.ModID, "AIGlobals.hintADV",
            AltUI.HighlightString("Other Prospectors") + " have done their research, and are much more " +
            AltUI.EnemyString("scary") + " this time.  Be careful as they may also " + AltUI.ObjectiveString("gang up") +
            " on you, or maybe they might be " + AIGlobals.FriendlyColor.ToRGBA255().ColorString("Friendly") + "?  " +
            AltUI.ThinkString("Who knows?"), 14);
        */
    }
}
