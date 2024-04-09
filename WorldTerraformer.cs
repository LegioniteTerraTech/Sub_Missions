using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using SafeSaves;
using TerraTechETCUtil;
using MonoMod.Utils;
using static TerraTechETCUtil.WorldDeformer;
using Newtonsoft.Json;
using System.IO;
using Sub_Missions.Editor;

namespace Sub_Missions
{
    public static class ModifiedTerrainExt
    {

        private static Dictionary<IntVector2, TerrainModifier> ModsTemp =
            new Dictionary<IntVector2, TerrainModifier>();
        public static void ApplyAll(this Dictionary<IntVector2, TerrainModifier> Mods, 
            IntVector2 offsetTileCoord = default)
        {
            foreach (var item in Mods)
            {
                item.Value.FlushApply(1, new WorldPosition(item.Key + offsetTileCoord, Vector3.zero).ScenePosition);
            }
        }
        public static void NudgeAll(this Dictionary<IntVector2, TerrainModifier> Mods, IntVector2 vec)
        {
            foreach (var item in Mods)
            {
                ModsTemp.Add(item.Key + vec, item.Value);
            }
            Mods.Clear();
            foreach (var item in ModsTemp)
            {
                Mods.Add(item.Key, item.Value);
            }
            ModsTemp.Clear();
        }
    }
    [AutoSaveManager]
    public class WorldTerraformer
    {
        [SSManagerInst]
        public static WorldTerraformer inst;
        /// <summary>
        /// This stores EVERY SINGLE Mod Missions terrain modifier active in-scene.
        /// </summary>
        [SSaveField]
        public Dictionary<IntVector2, TerrainModifier> TerrainModsSave;

        public static Dictionary<IntVector2, TerrainModifier> CurrentTerrainMods;
        public static string TerrainDirectory;
        public static WorldTerraTool tool;


        public static void Init()
        {
            if (inst != null)
                return;
            Debug_SMissions.Log("Init WorldTerraformer");
            SetupLowerTerrainClamps();
            AddOceanicBiomes();
            TerrainDirectory = Path.Combine(SMissionJSONLoader.BaseDirectory, "Custom Terrain");
            inst = new WorldTerraformer();
            tool = new GameObject("TerrainToolEditor").AddComponent<WorldTerraTool>();
            TerrainModifier.TileHeight = TerrainOperations.TileHeightRescaled;
            tool.Init();
            CurrentTerrainMods = new Dictionary<IntVector2, TerrainModifier>();
            ManGameMode.inst.ModeStartEvent.Subscribe(OnModeStart);
        }
        private static FieldInfo layers = typeof(MapGenerator).GetField("m_Layers", BindingFlags.NonPublic | BindingFlags.Instance);
        public static HashSet<MapGenerator> RegisteredVanilla = new HashSet<MapGenerator>();
        private static void SetupLowerTerrainClamps()
        {
            foreach (var item in ManWorld.inst.CurrentBiomeMap.IterateBiomes())
            {
                var HMG = item.HeightMapGenerator;
                if (HMG != null)
                {
                    RegisteredVanilla.Add(HMG);
                    /*
                    MapGenerator.Layer[] Layers = (MapGenerator.Layer[])layers.GetValue(HMG);
                    if (layers != null)
                    {
                        foreach (var item2 in Layers)
                        {
                            if (item2.applyOperation.code != MapGenerator.Operation.Code.Null)
                            {
                                Array.Resize(ref item2.operations, item2.operations.Length + 1);
                                item2.applyOperation.index = -3;// Setup clamp to be triggered on index -3
                                Debug_SMissions.Log("Limited Vanilla Biome Terrain " + item.name +
                                    " to " + TerrainOperations.TileYOffsetDelta +
                                    " so that deep oceans below sea level can exist");
                            }
                        }
                    }
                    */
                }
            }
        }
        private static FieldInfo biomesAll = typeof(BiomeMap).GetField("biomes", BindingFlags.NonPublic | BindingFlags.Instance);

        private static FieldInfo biomesBatched = typeof(BiomeMap).GetField("m_BiomeGroups",
            BindingFlags.Instance | BindingFlags.NonPublic),
            biomesInside = typeof(BiomeGroup).GetField("m_Biomes", BindingFlags.Instance | BindingFlags.NonPublic),
            biomesWeights = typeof(BiomeGroup).GetField("m_BiomeWeights", BindingFlags.Instance | BindingFlags.NonPublic);
        private static void AddOceanicBiomes()
        {
            Biome[] biomes = (Biome[])biomesAll.GetValue(ManWorld.inst.CurrentBiomeMap);
            foreach (var item in biomes)
            {
                Debug_SMissions.Log("Biome " + item.name);
            }
            Array.Resize(ref biomes, biomes.Length + 3);

            Biome biomeGrassPond = CopyBiomeTEMP(biomes[1], "MuddyPond");
            SinkBiomeTEMP(biomeGrassPond);
            biomes[biomes.Length - 3] = biomeGrassPond;

            Biome biomeMountainSeas = CopyBiomeTEMP(biomes[1], "JaggedSeas");
            SinkBiomeTEMP(biomeMountainSeas);
            biomes[biomes.Length - 2] = biomeMountainSeas;

            Biome biomeDesertBeaches = CopyBiomeTEMP(biomes[1], "ParadoxalBeaches");
            SinkBiomeTEMP(biomeDesertBeaches);
            biomes[biomes.Length - 1] = biomeDesertBeaches;

            biomesAll.SetValue(ManWorld.inst.CurrentBiomeMap, biomes);


            BiomeGroup[] biomesGrouped = (BiomeGroup[])biomesBatched.GetValue(ManWorld.inst.CurrentBiomeMap);
            foreach (var item in biomesGrouped)
            {
                Debug_SMissions.Log("Biome Group " + item.name);
                try
                {
                    Biome[] biomes2 = (Biome[])biomesInside.GetValue(item);
                    Array.Resize(ref biomes2, biomes2.Length + 3);
                    biomes2[biomes.Length - 3] = biomeGrassPond;
                    biomes2[biomes.Length - 2] = biomeMountainSeas;
                    biomes2[biomes.Length - 1] = biomeDesertBeaches;
                    biomesInside.SetValue(item, biomes2);

                    float[] biomesWeightsCached = (float[])biomesWeights.GetValue(item);
                    Array.Resize(ref biomesWeightsCached, biomesWeightsCached.Length + 3);
                    biomesWeightsCached[biomes.Length - 3] = 30;
                    biomesWeightsCached[biomes.Length - 2] = 30;
                    biomesWeightsCached[biomes.Length - 1] = 30;
                    biomesWeights.SetValue(item, biomesWeightsCached);
                    Debug_SMissions.Log("Biomes added!");
                }
                catch { }
            }
        }
        private static FieldInfo[] MassCopy = null;
        private static FieldInfo[] MassCopy2 = null;
        private static FieldInfo HeightGen = typeof(Biome).GetField("heightMapGenerator",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private static Biome CopyBiomeTEMP(Biome from, string name)
        {
            if (MassCopy == null)
            {
                MassCopy = typeof(Biome).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                MassCopy2 = typeof(MapGenerator).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            }
            Biome copyBiome = ScriptableObject.CreateInstance<Biome>();
            copyBiome.name = name;
            foreach (var item in MassCopy)
            {
                try
                {
                    item.SetValue(copyBiome, item.GetValue(from));
                }
                catch { }
            }
            MapGenerator MG = new GameObject(name).AddComponent<MapGenerator>();
            foreach (var item in MassCopy2)
            {
                try
                {
                    item.SetValue(MG, item.GetValue(from.HeightMapGenerator));
                }
                catch { }
            }
            HeightGen.SetValue(copyBiome, MG);
            Debug_SMissions.Log("Made biome " + name);
            return copyBiome;
        }
        private static void SinkBiomeTEMP(Biome biome)
        {
            MapGenerator.Layer[] Layers = (MapGenerator.Layer[])layers.GetValue(biome.HeightMapGenerator);
            if (layers != null)
            {
                foreach (var item2 in Layers)
                {
                    if (item2.applyOperation.code != MapGenerator.Operation.Code.Null)
                    {
                        Array.Resize(ref item2.operations, item2.operations.Length + 1);
                        item2.operations[item2.operations.Length - 1] = new MapGenerator.Operation()
                        {
                            buffered = false,
                            code = MapGenerator.Operation.Code.Sub,
                            index = item2.operations.Length - 1,
                            param = -0.25f
                        };
                    }
                }
            }
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


        public class WorldTerraTool : MonoBehaviour
        {
            //*
            internal bool ToolARMED = true;
            private bool showGUI = true;
            // */ internal bool ToolARMED = false; private bool showGUI = false;

            public TerraformerCursorState state = 0;
            private static int ToolSize = 9;
            public static Dictionary<TerraformerType, TerrainModifier> TerrainDefaults = new Dictionary<TerraformerType, TerrainModifier>();

            private static int RescaleFactorInt = 100;
            private static int saveRadiusInt = 2;
            private static float RescaleFactor => RescaleFactorInt / (TerrainOperations.RescaleFactor * 100);
            private static float applyStrength => 0.01f * RescaleFactor;
            private static float levelingStrength => 0.1f * RescaleFactor;
            private static TerraformerType ToolMode = TerraformerType.Circle;
            private static float cachedHeight = 0;
            private static float delayTimer = 0;
            private static float delayTimerDelay = 0.0356f;
            private static KeyCode altHotKey = KeyCode.LeftShift;
            private static Vector3 RampStart = Vector3.zero;
            private static Vector3 RampEnd = Vector3.one;
            private static bool sideStart = true;

            internal void Init()
            {
                RecalibrateTools(TerrainDefaults);
            }
            public void Update()
            {
                bool delayTimed;
                bool deltaed = false;
                if (delayTimer <= 0)
                {
                    delayTimer += delayTimerDelay;
                    delayTimed = true;
                }
                else
                {
                    delayTimer -= Time.deltaTime;
                    delayTimed = false;
                }
                if (Singleton.playerTank && ToolARMED)
                {
                    if (ManPointer.inst.DraggingItem != null)
                    {
                        ToolARMED = false;
                        return;
                    }
                    Vector3 terrainPosSpot;
                    if (Input.GetMouseButtonDown(2))
                    {
                        ToolMode = (TerraformerType)Mathf.Repeat((int)ToolMode + 1, Enum.GetValues(typeof(TerraformerType)).Length);
                        UIHelpersExt.BigF5broningBanner("Tool: " + ToolMode, false);
                    }
                    else if (Input.GetMouseButtonDown(0) && Input.GetKey(altHotKey) &&
                        GrabTerrainCursorPos(out terrainPosSpot))
                    {
                        var worldT = ManWorld.inst.TileManager.LookupTile(terrainPosSpot);
                        if (worldT != null)
                        {
                            IntVector2 tilePosInTile = new IntVector2(
                                (terrainPosSpot - worldT.Terrain.transform.position).ToVector2XZ() / TerrainModifier.tilePosToTileScale);
                            cachedHeight = worldT.Terrain.terrainData.GetHeight(tilePosInTile.x, tilePosInTile.y) / TerrainOperations.RescaleFactor;
                        }
                        if (ToolMode == TerraformerType.Slope)
                        {
                            ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Select);
                            if (sideStart)
                                RampStart = terrainPosSpot;
                            else
                                RampEnd = terrainPosSpot;
                            sideStart = !sideStart;
                        }
                    }
                    if (delayTimed && GrabTerrainCursorPos(out terrainPosSpot) &&
                        !UIHelpersExt.MouseIsOverSubMenu(MainWindow) && !ManModGUI.IsMouseOverModGUI)
                    {
                        float SFXtime = 0.75f;
                        Vector3 terrainPosSpotCorrect = terrainPosSpot -
                            TerrainDefaults[TerraformerType.Circle].Position.GameWorldPosition;
                        Vector3 terrainPosSpotCorrectSqr = terrainPosSpot + new Vector3(ToolSize * 2,0, -ToolSize) - 
                            TerrainDefaults[TerraformerType.Square].Position.GameWorldPosition;
                        switch (ToolMode)
                        {
                            case TerraformerType.Circle:
                                if (Input.GetKey(altHotKey))
                                {
                                    state = TerraformerCursorState.Up;
                                    if (Input.GetMouseButton(0))
                                    {
                                        TerrainDefaults[ToolMode].FlushAdd(applyStrength, terrainPosSpotCorrect);
                                        SFXHelpers.TankPlayLooping(Singleton.playerTank, TechAudio.SFXType.Refinery, SFXtime, 1);
                                        deltaed = true;
                                    }
                                    DebugExtUtilities.DrawDirIndicatorCircle(terrainPosSpot + new Vector3(0, 2, 0),
                                        Vector3.up, Vector3.forward, ToolSize, Color.cyan, delayTimerDelay);
                                }
                                else
                                {
                                    state = TerraformerCursorState.Down;
                                    if (Input.GetMouseButton(0))
                                    {
                                        TerrainDefaults[ToolMode].FlushAdd(-applyStrength, terrainPosSpotCorrect);
                                        SFXHelpers.TankPlayLooping(Singleton.playerTank, TechAudio.SFXType.GCPlasmaCutter, SFXtime, 1);
                                        deltaed = true;
                                    }
                                    DebugExtUtilities.DrawDirIndicatorCircle(terrainPosSpot,
                                        Vector3.up, Vector3.forward, ToolSize, Color.cyan, delayTimerDelay);
                                }
                                break;
                            case TerraformerType.Square:
                                if (Input.GetKey(altHotKey))
                                {
                                    state = TerraformerCursorState.Up;
                                    if (Input.GetMouseButton(0))
                                    {
                                        TerrainDefaults[ToolMode].FlushAdd(applyStrength, terrainPosSpotCorrectSqr);
                                        SFXHelpers.TankPlayLooping(Singleton.playerTank, TechAudio.SFXType.Refinery, SFXtime, 1);
                                        deltaed = true;
                                    }
                                    DebugExtUtilities.DrawDirIndicatorRecPriz(terrainPosSpot + new Vector3(0, 2, 0),
                                        new Vector3(ToolSize * 2, 1, ToolSize * 2), Color.cyan, delayTimerDelay);
                                }
                                else
                                {
                                    state = TerraformerCursorState.Down;
                                    if (Input.GetMouseButton(0))
                                    {
                                        TerrainDefaults[ToolMode].FlushAdd(-applyStrength, terrainPosSpotCorrectSqr);
                                        SFXHelpers.TankPlayLooping(Singleton.playerTank, TechAudio.SFXType.GCPlasmaCutter, SFXtime, 1);
                                        deltaed = true;
                                    }
                                    DebugExtUtilities.DrawDirIndicatorRecPriz(terrainPosSpot,
                                        new Vector3(ToolSize * 2, 1, ToolSize * 2), Color.cyan, delayTimerDelay);
                                }
                                break;
                            case TerraformerType.Level:
                                if (Input.GetKey(altHotKey))
                                {
                                    state = TerraformerCursorState.Leveling;
                                    if (Input.GetMouseButton(0))
                                    {
                                        deltaed = true;
                                        var worldT = ManWorld.inst.TileManager.LookupTile(terrainPosSpot);
                                        if (worldT != null)
                                        {
                                            DebugExtUtilities.DrawDirIndicatorCircle(terrainPosSpot + Vector3.up, Vector3.up,
                                            Vector3.forward, ToolSize, Color.black, delayTimerDelay);
                                            TerrainDefaults[TerraformerType.Circle].FlushLevel(2, -applyStrength,
                                                terrainPosSpotCorrect);
                                        }
                                        else
                                            DebugExtUtilities.DrawDirIndicatorCircle(terrainPosSpot + Vector3.up,
                                                Vector3.up, Vector3.forward, ToolSize, Color.red, delayTimerDelay);
                                        SFXHelpers.TankPlayLooping(Singleton.playerTank, TechAudio.SFXType.ComponentFactory, SFXtime, 1);
                                    }
                                    else
                                        DebugExtUtilities.DrawDirIndicatorCircle(terrainPosSpot + Vector3.up, Vector3.up,
                                        Vector3.forward, ToolSize, Color.white, delayTimerDelay);
                                }
                                else
                                {
                                    state = TerraformerCursorState.Leveling;
                                    if (Input.GetMouseButton(0))
                                    {
                                        deltaed = true;
                                        DebugExtUtilities.DrawDirIndicatorCircle(terrainPosSpot + Vector3.up,
                                            Vector3.up, Vector3.forward, ToolSize, Color.yellow, delayTimerDelay);
                                        TerrainDefaults[TerraformerType.Circle].FlushLevel(2, applyStrength, terrainPosSpotCorrect);
                                        SFXHelpers.TankPlayLooping(Singleton.playerTank, TechAudio.SFXType.GSODrillLarge, SFXtime, 1);
                                    }
                                    else
                                        DebugExtUtilities.DrawDirIndicatorCircle(terrainPosSpot + Vector3.up,
                                            Vector3.up, Vector3.forward, ToolSize, Color.magenta, delayTimerDelay);
                                }
                                break;
                            case TerraformerType.Reset:
                                if (Input.GetKey(altHotKey))
                                {
                                    state = TerraformerCursorState.Default;
                                    if (Input.GetMouseButton(0))
                                    {
                                        deltaed = true;
                                        var worldT = ManWorld.inst.TileManager.LookupTile(terrainPosSpot);
                                        if (worldT != null)
                                        {
                                            DebugExtUtilities.DrawDirIndicatorCircle(terrainPosSpot + Vector3.up, Vector3.up,
                                            Vector3.forward, ToolSize, Color.black, delayTimerDelay);
                                            TerrainDefaults[TerraformerType.Circle].FlushReset(-levelingStrength,
                                                terrainPosSpotCorrect);
                                        }
                                        else
                                            DebugExtUtilities.DrawDirIndicatorCircle(terrainPosSpot + Vector3.up,
                                                Vector3.up, Vector3.forward, ToolSize, Color.red, delayTimerDelay);
                                        SFXHelpers.TankPlayLooping(Singleton.playerTank, TechAudio.SFXType.VENFlameThrower, SFXtime, 1);
                                    }
                                    else
                                        DebugExtUtilities.DrawDirIndicatorCircle(terrainPosSpot + Vector3.up, Vector3.up,
                                        Vector3.forward, ToolSize, Color.white, delayTimerDelay);
                                }
                                else
                                {
                                    state = TerraformerCursorState.Default;
                                    if (Input.GetMouseButton(0))
                                    {
                                        deltaed = true;
                                        DebugExtUtilities.DrawDirIndicatorCircle(terrainPosSpot + Vector3.up,
                                            Vector3.up, Vector3.forward, ToolSize, Color.yellow, delayTimerDelay);
                                        TerrainDefaults[TerraformerType.Circle].FlushReset(levelingStrength, terrainPosSpotCorrect);
                                        SFXHelpers.TankPlayLooping(Singleton.playerTank, TechAudio.SFXType.VENFlameThrower, SFXtime, 1);
                                    }
                                    else
                                        DebugExtUtilities.DrawDirIndicatorCircle(terrainPosSpot + Vector3.up,
                                            Vector3.up, Vector3.forward, ToolSize, Color.magenta, delayTimerDelay);
                                }
                                break;
                            case TerraformerType.Slope:
                                DebugExtUtilities.DrawDirIndicatorCircle(RampStart + Vector3.up, Vector3.up,
                                            Vector3.forward, 1, new Color(0, 0, 1, 0.5f), delayTimerDelay);
                                DebugExtUtilities.DrawDirIndicator(RampStart, RampStart + new Vector3(0, 2, 0),
                                             new Color(0, 0, 1, 0.5f), delayTimerDelay);
                                DebugExtUtilities.DrawDirIndicatorCircle(RampEnd + Vector3.up, Vector3.up,
                                            Vector3.forward, 1, new Color(1, 0, 0, 0.5f), delayTimerDelay);
                                DebugExtUtilities.DrawDirIndicator(RampEnd, RampEnd + new Vector3(0, 2, 0),
                                            new Color(1, 0, 0, 0.5f), delayTimerDelay);
                                DebugExtUtilities.DrawDirIndicator(RampStart + new Vector3(0, 2, 0), RampEnd + new Vector3(0, 2, 0),
                                            new Color(1, 0, 1, 0.5f), delayTimerDelay);
                                if (Input.GetKey(altHotKey))
                                {
                                    state = TerraformerCursorState.Default;
                                    if (Input.GetMouseButtonDown(0))
                                    {
                                        deltaed = true;
                                    }
                                    else
                                        DebugExtUtilities.DrawDirIndicatorCircle(terrainPosSpot + Vector3.up,
                                            Vector3.up, Vector3.forward, 2, sideStart ? Color.blue : Color.red, delayTimerDelay);
                                }
                                else
                                {
                                    state = TerraformerCursorState.Default;
                                    if (Input.GetMouseButton(0))
                                    {
                                        deltaed = true;
                                        var worldT = ManWorld.inst.TileManager.LookupTile(terrainPosSpot);
                                        if (worldT != null)
                                        {
                                            DebugExtUtilities.DrawDirIndicatorCircle(terrainPosSpot + Vector3.up, Vector3.up,
                                            Vector3.forward, ToolSize, Color.black, delayTimerDelay);
                                            TerrainDefaults[TerraformerType.Circle].FlushRamp(levelingStrength,
                                               RampStart, RampEnd, terrainPosSpotCorrect);
                                        }
                                        else
                                            DebugExtUtilities.DrawDirIndicatorCircle(terrainPosSpot + Vector3.up,
                                                Vector3.up, Vector3.forward, ToolSize, Color.red, delayTimerDelay);
                                        SFXHelpers.TankPlayLooping(Singleton.playerTank, TechAudio.SFXType.Scrapper, SFXtime, 1);
                                    }
                                    else
                                        DebugExtUtilities.DrawDirIndicatorCircle(terrainPosSpot + Vector3.up, Vector3.up,
                                        Vector3.forward, ToolSize, Color.white, delayTimerDelay);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                if (delayTimed && !deltaed)
                {
                    /*
                    foreach (var item in terrainsDeformed)
                    {
                        OnTerrainDeformed.Send(item);
                        if (TerrainModsActive.TryGetValue(item.Coord, out TerrainModifier TM))
                        {
                            TM.Setup(item);
                        }
                        else
                            TerrainModsActive.Add(item.Coord, new TerrainModifier(item));
                    }
                    terrainsDeformed.Clear();
                    */
                }
            }


            private static void RecalibrateTools(Dictionary<TerraformerType, TerrainModifier> tools)
            {
                int dualSize = (int)(ToolSize * 2f);
                tools.Remove(TerraformerType.Circle);
                var terra = new TerrainModifier(ToolSize);
                terra.AddHeightsAtPositionRadius(ToolSize, 1, true);
                terra.EncapsulateRecenter();
                tools.Add(TerraformerType.Circle, terra);

                tools.Remove(TerraformerType.Square);
                terra = new TerrainModifier(ToolSize);
                terra.AddHeightsAtPosition(Vector3.zero, new Vector2(dualSize, dualSize), 1, true);
                terra.EncapsulateRecenter();
                tools.Add(TerraformerType.Square, terra);
            }



            // -----------------------------------
            //                 GUI
            // -----------------------------------
            private static string setCache = ToolSize.ToString();
            private static string setCache2 = RescaleFactorInt.ToString();
            private static string setCache3 = saveRadiusInt.ToString();
            public bool UtilityShown => showGUI;
            private static Rect MainWindow = new Rect(0, 0, 260, 200);
            private static string[] labels = new string[] {
            TerraformerType.Circle.ToString(),
            TerraformerType.Square.ToString(),
            TerraformerType.Level.ToString(),
            TerraformerType.Reset.ToString(),
            TerraformerType.Slope.ToString(),
        };
            internal void OnGUI()
            {
                if (showGUI)
                    MainWindow = AltUI.Window(26342654, MainWindow, GUIDisplay, "Terrain Tool", CloseGUIDisplay);
            }
            internal void GUIDisplay(int ID)
            {
                ToolARMED = AltUI.Toggle(ToolARMED, "ARMED");

                ToolMode = (TerraformerType)GUILayout.Toolbar((int)ToolMode, labels);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Size");
                GUILayout.FlexibleSpace();
                string set = GUILayout.TextField(setCache, 3, AltUI.TextfieldBlackAdjusted, GUILayout.Width(160));
                if (int.TryParse(set, out int val1))
                {
                    GUILayout.Label("<color=green>O</color>", GUILayout.Width(25));
                    if (set != setCache)
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                        ToolSize = val1;
                        RecalibrateTools(TerrainDefaults);
                    }
                }
                else
                {
                    GUILayout.Label("<color=red>X</color>", GUILayout.Width(25));
                }
                setCache = set;
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.Label("Strength");
                GUILayout.FlexibleSpace();
                string set2 = GUILayout.TextField(setCache2, 4, AltUI.TextfieldBlackAdjusted, GUILayout.Width(160));
                if (int.TryParse(set2, out int val2))
                {
                    GUILayout.Label("<color=green>O</color>", GUILayout.Width(25));
                    if (set2 != setCache2)
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                        RescaleFactorInt = val2;
                    }
                }
                else
                {
                    GUILayout.Label("<color=red>X</color>", GUILayout.Width(25));
                }
                setCache2 = set2;
                GUILayout.EndHorizontal();

                if (Singleton.playerTank)
                {
                    if (GUILayout.Button("Save Tiles"))
                    {
                        Vector3 pos = Singleton.playerTank.boundsCentreWorldNoCheck;
                        var tile = ManWorld.inst.TileManager.LookupTile(pos);
                        if (tile != null)
                        {
                            CurrentTerrainMods.Clear();
                            IntVector2 coord = WorldPosition.FromScenePosition(pos).TileCoord;
                            foreach (var item in coord.IterateRectVolumeCentered(IntVector2.one * saveRadiusInt))
                            {
                                WorldTile WT = ManWorld.inst.TileManager.LookupTile(item);
                                if (WT != null)
                                {
                                    TerrainModifier TM2 = new TerrainModifier(WT, pos);
                                    CurrentTerrainMods.Add(item, TM2);
                                }
                            }
                            ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                        }
                    }
                }
                else
                    GUILayout.Button("Save Tiles", AltUI.ButtonGrey);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("EXPORT " + CurrentTerrainMods.Count))
                {
                    if (!Directory.Exists(TerrainDirectory))
                        Directory.CreateDirectory(TerrainDirectory);
                    var path = Path.Combine(TerrainDirectory, "TerrainDump.json");
                    File.WriteAllText(path, JsonConvert.SerializeObject(CurrentTerrainMods));
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Close);
                }
                if (GUILayout.Button("LOAD"))
                {
                    if (!Directory.Exists(TerrainDirectory))
                        Directory.CreateDirectory(TerrainDirectory);
                    var path = Path.Combine(TerrainDirectory, "TerrainDump.json");
                    if (File.Exists(path))
                    {
                        CurrentTerrainMods = JsonConvert.DeserializeObject<Dictionary<IntVector2, TerrainModifier>>(
                            File.ReadAllText(path));
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Close);
                    }
                    else
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.AnchorFailed);
                }
                if (GUILayout.Button("OPEN"))
                {
                    if (!Directory.Exists(TerrainDirectory))
                        Directory.CreateDirectory(TerrainDirectory);
                    SubMissionsWiki.OpenInExplorer(TerrainDirectory);
                    ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Enter);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Save Radius");
                GUILayout.FlexibleSpace();
                string set3 = GUILayout.TextField(setCache3, 2, AltUI.TextfieldBlackAdjusted, GUILayout.Width(160));
                if (int.TryParse(set3, out int val3))
                {
                    GUILayout.Label("<color=green>O</color>", GUILayout.Width(25));
                    if (set3 != setCache3)
                    {
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Rename);
                        saveRadiusInt = SMAutoFill.ClampInt(val3);
                    }
                }
                else
                {
                    GUILayout.Label("<color=red>X</color>", GUILayout.Width(25));
                }
                setCache3 = set3;
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                if (Singleton.playerTank)
                {
                    if (GUILayout.Button("Default Terrain"))
                    {
                        Vector3 pos = ManWorld.inst.TileManager.CalcTileOriginScene(WorldPosition.FromScenePosition(
                                    Singleton.playerTank.boundsCentreWorldNoCheck).TileCoord);
                        ManWorldTileExt.ReloadTile(pos);
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Back);
                    }
                    if (GUILayout.Button("Apply"))
                    {
                        Vector3 pos = ManWorld.inst.TileManager.CalcTileOriginScene(WorldPosition.FromScenePosition(
                                    Singleton.playerTank.boundsCentreWorldNoCheck).TileCoord);
                        foreach (var item in CurrentTerrainMods)
                        {
                            item.Value.FlushApply(1, pos);
                        }
                        ManSFX.inst.PlayUISFX(ManSFX.UISfxType.Open);
                    }
                }
                else
                {
                    GUILayout.Button("Default Terrain", AltUI.ButtonGrey);
                    GUILayout.Button("Apply", AltUI.ButtonGrey);
                }
                GUILayout.EndHorizontal();
                GUI.DragWindow();
            }
            public void ToggleGUIDisplay()
            {
                UIHelpersExt.ClampMenuToScreen(ref MainWindow, false);
                showGUI = !showGUI;
                if (!showGUI)
                    ToolARMED = false;
            }
            private void CloseGUIDisplay()
            {
                ToolARMED = false; 
                showGUI = false;
            }
        }


        public static void PrepareForSaving()
        {
            if (inst == null)
                return;
            foreach (var item in inst.TerrainModsSave)
            {
                WorldDeformer.inst.TerrainModsActive.Remove(item.Key);
            }
            //inst.TerrainModsSave = WorldDeformer.inst.TerrainModsActive;
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
            WorldDeformer.inst.TerrainModsActive.AddRange(inst.TerrainModsSave);
            inst.TerrainModsSave = null;
        }


    }
}
