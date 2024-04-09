using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerraTechETCUtil;
using UnityEngine;

namespace Sub_Missions
{
    public enum TerraformerCursorState
    {
        None,
        Leveling,
        Up,
        Default,
        Down
    }
    public class CursorChanger : MonoBehaviour
    {
        public static CursorChangeHelper.CursorChangeCache Cache;
        public static bool AddedNewCursors = false;
        public static CursorChangeHelper.CursorChangeCache CursorIndexCache => Cache.CursorIndexCache;

        public static void AddNewCursors()
        {
            if (AddedNewCursors)
                return;
            if (ResourcesHelper.TryGetModContainer("Mod Missions", out ModContainer MC))
            {
                Cache = CursorChangeHelper.GetCursorChangeCache(SMissionJSONLoader.DLLDirectory, "Terraformer_Icons", MC,
                    "TerrainToolLevel",
                    "TerrainToolUp",
                    "TerrainToolDefault",
                    "TerrainToolDown"
                    );
            }
            else
                Debug_SMissions.Assert(true, "CursorChanger: AddNewCursors - Could not find ModContainer for Mod Missions!");

            AddedNewCursors = true;
        }
    }
}
