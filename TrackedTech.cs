using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;
using Newtonsoft.Json;

namespace Sub_Missions
{
    [Serializable]
    public class TrackedTech
    {   //  
        [JsonIgnore]
        public CustomSubMission mission;

        public string TechName = "";
        public int TechID = 0;

        [JsonIgnore]
        public bool loaded = false;

        [JsonIgnore]
        private Tank tech;

        [JsonIgnore]
        public Tank Tech
        {
            get
            {
                if (!loaded)
                {
                    if (!mission.GetTechPosHeading(TechName, out Vector3 pos, out Vector3 direction, out int team))
                        Debug.Log("SubMissions: Tech in TrackedTechs list but was never called in any Step!!!  In " + mission.Name + " of " + mission.Tree.TreeName + ".");
                    tech = SMUtil.SpawnTechAuto(ref mission, pos, team, direction, TechName);
                    loaded = true;
                }
                if (tech == null)
                    tech = TryFindMatchingTech();
                return tech;
            }
            set
            {
                tech = value;
                TechName = value.name;
                TechID = value.visible.ID;
            }
        }
        public void DestroyTech()
        {
            try
            {
                tech.visible.RemoveFromGame();
            }
            catch { }
        }
        public Tank TryFindMatchingTech()
        {
            foreach (Tank tank in Singleton.Manager<ManTechs>.inst.CurrentTechs)
            {
                if (tank.name == TechName && tank.visible.ID == TechID)
                {
                    return tank;
                }
            }
            return null;
        }
    }
}
