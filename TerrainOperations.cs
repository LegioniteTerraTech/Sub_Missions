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
        public const float tileScaleToMapGen = 2;


        public const float RescaleFactorInv = 1 / RescaleFactor;
        public const float TileHeightRescaled = TileHeightDefault * RescaleFactor;
        public const float TileYOffsetRescaled = TileYOffsetDefault * RescaleFactor;
        public const float TileYOffsetDelta = TileYOffsetRescaled - TileYOffsetDefault;


        public const float MaxPercentScalar = (TileHeightRescaled + TileYOffsetDelta) / TileHeightDefault;
        public const float MinPercentScalar = TileYOffsetDelta / TileHeightDefault;
        public const float TileYOffsetDefaultScalar = TileYOffsetDefault / TileHeightRescaled;
        public const float TileYOffsetDeltaScalar = TileYOffsetDelta / TileHeightRescaled;
        public const float TileYOffsetScalarSeaLevel = -100 / TileHeightRescaled;
        public const float TileYOffsetScalarSeaLevelSceneryLand = -97 / TileHeightRescaled;
        public const float TileYOffsetScalarSeaLevelScenerySea = -106 / TileHeightRescaled;

        public static float TerraGenRescaled(float input)
        {
            //Debug_SMissions.Log("Rescaled " + input + " to " + (input * RescaleFactorInv));
            return input * RescaleFactorInv;
        }

        public static float LerpToRescaled(float input) => input - TileYOffsetDelta;
        public static float LerpToDefault(float input) => input + TileYOffsetDelta;

    }
}
