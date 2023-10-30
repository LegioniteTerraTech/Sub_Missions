using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SafeSaves;
using TerraTechETCUtil;

namespace Sub_Missions
{
    [AutoSaveManager]
    public class WorldTerraformer
    {
        [SSManagerInst]
        public static WorldTerraformer inst = new WorldTerraformer();
        [SSaveField]
        public Dictionary<IntVector2, TerrainModifier> TerrainModsSave;


        public static void Init()
        {
            if (inst != null)
                return; 
            inst = new WorldTerraformer();
            ManGameMode.inst.ModeStartEvent.Subscribe(OnModeStart);
        }
        public static void OnModeStart(Mode mode)
        {
            if (inst == null)
                return;
            switch (mode.GetGameType())
            {
                case ManGameMode.GameType.Attract:
                case ManGameMode.GameType.MainGame:
#if DEBUG
                    WorldDeformer.inst.enabled = true;
#else
                    WorldDeformer.inst.enabled = false;
#endif
                    break;
                case ManGameMode.GameType.RaD:
                case ManGameMode.GameType.Creative:
                    WorldDeformer.inst.enabled = true;
                    break;
                default:
                    WorldDeformer.inst.enabled = false;
                    break;
            }
        }
        public static void PrepareForSaving()
        {
            if (inst == null)
                return;
            inst.TerrainModsSave = WorldDeformer.inst.TerrainModsActive;
        }
        public static void FinishedSaving()
        {
            if (inst == null)
                return;
            inst.TerrainModsSave = null;
        }
        public static void FinishedLoading()
        {
            if (inst == null)
                return;
            WorldDeformer.inst.TerrainModsActive = inst.TerrainModsSave;
            inst.TerrainModsSave = null;
        }
    }
}
