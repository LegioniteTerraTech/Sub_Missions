using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Sub_Missions.Editor;

namespace Sub_Missions.Steps
{
    public abstract class SMissionStep
    {
        /// <summary>
        /// Mission base
        /// </summary>
        public SubMission Mission;

        /// <summary>
        /// Mission STEP
        /// </summary>
        public SubMissionStep SMission;

        /// <summary>
        /// ProgressID Description
        /// </summary>
        public static string StepDesc = " Use " + SubMission.alwaysRunValue + " to always update.  Else, whatever \n" +
        "  //  number that is ADJACENT TO or directly on SubMission's CurrentProgressID will be triggered.";

        public static string TerrainHandlingDesc = "\n" +
        "  // 0 = Align with Mission Origin\n" +
        "  // 1 = Snap to Terrain if Position Lower\n" +
        "  // 2 = Align with terrain + offset by position\n" +
        "  // 3 = Snap to Terrain";

        public abstract string GetDocumentation();

        public abstract void InitGUI();

        /// <summary>
        /// Called every time the mission is loaded
        /// Called on FirstSetup(), before the included FirstSetup().
        /// </summary>
        public abstract void OnInit();
        /// <summary>
        /// Only called on first mission load
        /// </summary>
        public abstract void FirstSetup();
        public abstract void Trigger();

        public abstract void OnDeInit();

        protected void AddField(ESMSFields type, string name)
        {
            SMStepEditorGUI.AddField(this, type, name);
        }
        protected void AddGlobal(ESMSFields type, string name, EVaribleType varType)
        {
            SMStepEditorGUI.AddGlobal(this, type, name, varType);
        }
        protected void AddOptions(ESMSFields type, string name, string[] options,
            Dictionary<int, KeyValuePair<string, ESMSFields>> ext = null)
        {
            SMStepEditorGUI.AddOptions(this, type, name, options, ext);
        }
    }
}
