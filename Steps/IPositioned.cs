using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Sub_Missions.Steps
{
    public interface IPositioned
    {
        Vector3 GetPositionScene { get; }
    }
}
