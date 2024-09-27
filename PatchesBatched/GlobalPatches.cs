using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HarmonyLib;

namespace Sub_Missions
{
    internal static class GlobalPatches
    {
        internal static class CursorPatches
        {
            internal static Type target = typeof(GameCursor);
            /*
            // NEW
            AIOrderAttack
            AIOrderEmpty
            AIOrderMove
            AIOrderSelect
            */

            /// <summary>
            /// See CursorChanger for more information
            /// </summary>
            /// <param name="__result"></param>
            private static void GetCursorState_Postfix(ref GameCursor.CursorState __result)
            {
                if (!CursorChanger.AddedNewCursors)
                    return;
                if (ManTerraformTool.tool && ManTerraformTool.tool.ToolARMED)
                {
                    switch (ManTerraformTool.tool.state)
                    {
                        case TerraformerCursorState.None:
                            break;
                        case TerraformerCursorState.Leveling:
                            __result = CursorChanger.CursorIndexCache[0];
                            break;
                        case TerraformerCursorState.Up:
                            __result = CursorChanger.CursorIndexCache[1];
                            break;
                        case TerraformerCursorState.Default:
                            __result = CursorChanger.CursorIndexCache[2];
                            break;
                        case TerraformerCursorState.Down:
                            __result = CursorChanger.CursorIndexCache[3];
                            break;
                        default:
                            break;
                    }
                }
            }

        }
        internal static class ModeAttractPatches
        {
            internal static Type target = typeof(ModeAttract);
            //Subscribe - Setup main menu techs
            private static void SetupTechs_Postfix()
            {
                KickStart.DelayedInit();
                //ManSubMissions.Subscribe();
            }
        }


        internal static class StringLookupPatches
        {
            internal static Type target = typeof(StringLookup);
            //GetCorrectName
            private static bool GetCorporationName_Prefix(ref FactionSubTypes corporation, ref string __result)
            {
                if (ManSMCCorps.IsUnofficialSMCCorpLicense(corporation))
                {
                    if (ManSMCCorps.TryGetSMCCorpLicense((int)corporation, out SMCCorpLicense CL))
                    {
                        __result = CL.FullName;
                        return false;
                    }
                }
                return true;
            }
        }
        internal static class SpriteFetcherPatches
        {
            internal static Type target = typeof(SpriteFetcher);
            //LoadTheRightStuff
            private static bool GetCorpIcon_Prefix(SpriteFetcher __instance, ref FactionSubTypes corp, ref Sprite __result)
            {
                if (ManSMCCorps.IsUnofficialSMCCorpLicense(corp))
                {

                    if (ManSMCCorps.TryGetSMCCorpLicense((int)corp, out SMCCorpLicense CL))
                    {
                        if (CL.SmallCorpIcon)
                        {
                            __result = CL.SmallCorpIcon;
                            return false;
                        }
                        if (CL.SkinReferenceFaction != FactionSubTypes.NULL && (int)CL.SkinReferenceFaction < Enum.GetValues(typeof(FactionSubTypes)).Length)
                        {
                            corp = CL.SkinReferenceFaction;
                            return true;
                        }
                    }

                    corp = FactionSubTypes.GSO;
                }
                return true;
            }
            //LoadTheRightStuff2
            private static bool GetSelectedCorpIcon_Prefix(SpriteFetcher __instance, ref FactionSubTypes corp, ref Sprite __result)
            {
                if (ManSMCCorps.IsUnofficialSMCCorpLicense(corp))
                {
                    if (ManSMCCorps.TryGetSMCCorpLicense((int)corp, out SMCCorpLicense CL))
                    {
                        if (CL.SmallSelectedCorpIcon)
                        {
                            __result = CL.SmallSelectedCorpIcon;
                            return false;
                        }
                        if (CL.SkinReferenceFaction != FactionSubTypes.NULL && (int)CL.SkinReferenceFaction < Enum.GetValues(typeof(FactionSubTypes)).Length)
                        {
                            corp = CL.SkinReferenceFaction;
                            return true;
                        }
                    }
                    corp = FactionSubTypes.GSO;
                }
                return true;
            }
            //LoadTheRightStuff3
            private static bool GetModernCorpIcon_Prefix(SpriteFetcher __instance, ref FactionSubTypes corp, ref Sprite __result)
            {
                if (ManSMCCorps.IsUnofficialSMCCorpLicense(corp))
                {
                    if (ManSMCCorps.TryGetSMCCorpLicense((int)corp, out SMCCorpLicense CL))
                    {
                        if (CL.HighResCorpIcon)
                        {
                            __result = CL.HighResCorpIcon;
                            return false;
                        }
                        if (CL.SkinReferenceFaction != FactionSubTypes.NULL && (int)CL.SkinReferenceFaction < Enum.GetValues(typeof(FactionSubTypes)).Length)
                        {
                            corp = CL.SkinReferenceFaction;
                            return true;
                        }
                    }
                    corp = FactionSubTypes.GSO;
                }
                return true;
            }
        }

    }
}
