using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static string StepDesc = " Use -999 to always update.  Else:\n" +
        "  // 0 = Align with Mission Origin\n" +
        "  // 1 = Snap to Terrain if Position Lower\n" +
        "  // 2 = Align with terrain + offset by position\n" +
        "  // 3 = Snap to Terrain";

        public static string TerrainHandlingDesc = "\n" +
        "  // 0 = Align with Mission Origin\n" +
        "  // 1 = Snap to Terrain if Position Lower\n" +
        "  // 2 = Align with terrain + offset by position\n" +
        "  // 3 = Snap to Terrain";

        public abstract string GetDocumentation();

        public abstract void OnInit();
        public abstract void FirstSetup();
        public abstract void Trigger();

        public abstract void OnDeInit();
    }
}
