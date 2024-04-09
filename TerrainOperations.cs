using UnityEngine;

namespace Sub_Missions
{
    /// <summary>
    /// Remember, when we rescale the terrain, we only scale the TERRAIN, not the other things!
    /// </summary>
    internal class TerrainOperations
    {
        public const float RescaleFactor = 4;
        public const float TileHeightDefault = 100;
        public const float TileYOffsetDefault = -50;


        public const float RescaleFactorInv = 1 / RescaleFactor;
        public const float TileHeightRescaled = TileHeightDefault * RescaleFactor;
        public const float TileYOffsetRescaled = TileYOffsetDefault * RescaleFactor;
        public const float TileYOffsetDelta = TileYOffsetRescaled - TileYOffsetDefault;

        public const float MaxPercentScalar = (TileHeightRescaled + TileYOffsetDelta) / TileHeightDefault;
        public const float MinPercentScalar = TileYOffsetDelta / TileHeightDefault;
        public const float TileYOffsetDeltaScalar = TileYOffsetDelta / TileHeightRescaled;
        public const float TileYOffsetScalarSeaLevel = -100 / TileHeightRescaled;

        public static float TerraGenRescaled(float input)
        {
            //Debug_SMissions.Log("Rescaled " + input + " to " + (input * RescaleFactorInv));
            return input * RescaleFactorInv;
        }

        public static float LerpToRescaled(float input) => input - TileYOffsetDelta;
        public static float LerpToDefault(float input) => input + TileYOffsetDelta;



        // OBSOLETE
        internal static void AmplifyTerrain(Terrain Terra)
        {
            Debug_SMissions.Info("SubMissions: Amplifying Terrain....");
            TerrainData TD = Terra.terrainData;
            float[,] floats = TD.GetHeights(0, 0, 129, 129);
            TD.size = new Vector3(TD.size.x, TD.size.y * RescaleFactor, TD.size.z);
            for (int stepX = 0; stepX < 129; stepX++)
                for (int stepY = 0; stepY < 129; stepY++)
                    floats.SetValue((floats[stepX, stepY] * RescaleFactorInv) - TileYOffsetDeltaScalar, stepX, stepY);
            TD.SetHeights(0, 0, floats);
            Terra.terrainData = TD;
            Terra.Flush();
            Terra.transform.position = Terra.transform.position.SetY(TileYOffsetDelta + Terra.transform.position.y);
            Debug_SMissions.Info("SubMissions: Amplifying Terrain complete!");
        }
        internal static void LevelTerrain(WorldTile WT)
        {
            Debug_SMissions.Log("SubMissions: Leveling terrain....");
            TerrainData TD = WT.Terrain.terrainData;
            TD.size = new Vector3(TD.size.x, TD.size.y * RescaleFactor, TD.size.z);
            float[,] floats = TD.GetHeights(0, 0, 129, 129);
            double totalheight = 0;
            foreach (float flo in floats)
                totalheight += flo;
            totalheight /= floats.Length;
            float th = (float)totalheight;
            for (int stepX = 1; stepX < 129; stepX++)
                for (int stepY = 1; stepY < 129; stepY++)
                    floats.SetValue(th, stepX, stepY);
            TD.SetHeights(0, 0, floats);
            WT.Terrain.terrainData = TD;
            WT.Terrain.Flush();
            Debug_SMissions.Log("SubMissions: Leveling terrain complete!");
        }


    }
}
