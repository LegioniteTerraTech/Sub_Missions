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
        public CustomSubMission Mission;

        /// <summary>
        /// Mission STEP
        /// </summary>
        public SubMissionStep SMission;


        public abstract void TrySetup();
        public abstract void Trigger();
    }
}
