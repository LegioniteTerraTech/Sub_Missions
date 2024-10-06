using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sub_Missions.Steps
{
    public enum ESMSFields
    {
        // - DEFAULTS
        Null,
        //ProgressID,
        //SuccessProgressID,
        Position,
        EulerAngles,
        Forwards,
        TerrainHandling,
        RevProgressIDOffset,
        VaribleType,
        VaribleCheckNum,
        InputNum,
        InputString,
        InputStringAux,
        FolderEventList,
        SetMissionVarIndex1,
        SetMissionVarIndex2,
        SetMissionVarIndex3,

        // - SPECIALS
        VaribleType_Action,
        SuccessProgressID,
        InputNum_int,
        InputNum_radius,

        InputString_large,
        InputStringAux_large,
        InputString_float,
        InputStringAux_float,
        InputString_Tech,
        InputStringAux_Tech,
        InputString_Tracked_Tech,
        InputStringAux_Tracked_Tech,
        InputString_MM,

        // - OTHERS
        SpawnOnly,
    }
}
