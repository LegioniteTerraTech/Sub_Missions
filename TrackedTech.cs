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
        public SubMission mission;

        public string TechName = "";
        public int TechID = 0;

        public bool loaded = false;
        public bool destroyed = false;

        [JsonIgnore]
        public DeliveryBombSpawner delayedSpawn;
        public bool DeliQueued = false;

        [JsonIgnore]
        private Tank tech;

        [JsonIgnore]
        public Tank Tech
        {
            get
            {
                if (!loaded && !DeliQueued)
                {
                    if (!mission.GetTechPosHeading(TechName, out Vector3 pos, out Vector3 direction, out int team))
                        SMUtil.Assert(true, "SubMissions: Tech in TrackedTechs list but was never called in any Step!!!  In " + mission.Name + " of " + mission.Tree.TreeName + ".");
                    tech = SMUtil.SpawnTechAuto(ref mission, pos, team, direction, TechName);
                    SMUtil.Assert(true, "SubMissions: Tech called into world before a StepSetupTech!  Mission: " + mission.Name);
                    loaded = true;
                }
                else if (DeliQueued && delayedSpawn.IsNull())
                {
                    if (!mission.GetTechPosHeading(TechName, out Vector3 pos, out Vector3 direction, out int team))
                        SMUtil.Assert(true, "SubMissions: Tech in TrackedTechs list but was never called in any Step!!!  In " + mission.Name + " of " + mission.Tree.TreeName + ".");
                    tech = SMUtil.SpawnTechAuto(ref mission, pos, team, direction, TechName);
                    SMUtil.Assert(true, "SubMissions: World was saved before bomb touchdown.  Mission: " + mission.Name);
                    loaded = true;
                }
                if (tech == null && !DeliQueued)
                    Tech = TryFindMatchingTech();
                return tech;
            }
            set
            {
                tech = value;
                TechName = value.name;
                TechID = value.visible.ID;
                loaded = true;
            }
        }
        public void SpawnTech(Vector3 pos)
        {
            try
            {
                if (!mission.GetTechPosHeading(TechName, out _, out _, out int team))
                    SMUtil.Assert(true, "SubMissions: Tech in TrackedTechs list but was never called in any Step!!!  In " + mission.Name + " of " + mission.Tree.TreeName + ".");
                Tech = RawTechLoader.SpawnTechExternal(pos, team, (Singleton.playerPos -  pos).normalized, RawTechExporter.LoadTechFromRawJSON(TechName, "Custom SMissions\\" + mission.Tree.TreeName + "\\Raw Techs"));
                DeliQueued = false;
            }
            catch
            {
                SMUtil.Assert(false, "SubMissions: TrackedTech (Delayed) - COULD NOT SPAWN TECH!  Of mission " + mission.Name + ", tree " + mission.Tree.TreeName);
            }
        }
        public void DestroyTech()
        {
            try
            {
                tech.visible.RemoveFromGame();
                destroyed = true;
            }
            catch 
            {
                SMUtil.Assert(false, "SubMissions: TrackedTech - COULD NOT ELIMINATE TECH!  Of mission " + mission.Name + ", tree " + mission.Tree.TreeName);
            }
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
            SMUtil.Assert(false, "SubMissions: TrackedTech - COULD NOT FIND TECH!  Of mission " + mission.Name + ", tree " + mission.Tree.TreeName);
            return null;
        }
        public void CheckWasDestroyed(Tank techIn)
        {
            if (techIn.visible.ID == TechID)//techIn.name == TechName &&
            {
                destroyed = true;
            }
            //else
            //    Debug.Log("SubMissions: Tech does not match " + TechName + " of ID " + TechID + ".");
        }
    }
}
