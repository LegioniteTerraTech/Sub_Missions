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

        public string TechName = ""; // WITHOUT .png for Techs
        public int TechID = 0;

        public bool loaded = false;
        public bool destroyed = false;
        public bool isDisposible = false;

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
                    DeliQueued = false;
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

        public TrackedTech(string name, bool isDisposable = false)
        {
            TechName = name;
            this.isDisposible = isDisposable;
        }


        public void SpawnTech(Vector3 pos)
        {
            try
            {
                if (!mission.TrackedTechs.Contains(this))
                    return; // denied as it's been removed from the pool
                if (!mission.GetTechPosHeading(TechName, out _, out Vector3 direction, out int team))
                    SMUtil.Assert(true, "SubMissions: Tech in TrackedTechs list but was never called in any Step!!!  In " + mission.Name + " of " + mission.Tree.TreeName + ".");
                
                Tech = SMUtil.SpawnTechAuto(ref mission, pos, team, direction, TechName);
                //Tech = RawTechLoader.SpawnTechExternal(pos, team, (Singleton.playerPos -  pos).normalized, RawTechExporter.LoadTechFromRawJSON(TechName, "Custom SMissions\\" + mission.Tree.TreeName + "\\Raw Techs"));
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
                if (TechName == null)
                    TechName = "Unset";
                if (!tech)
                {
                    Visible techUnloaded = ManSaveGame.inst.LookupSerializedVisible(TechID);
                    if (techUnloaded)
                    {
                        if (techUnloaded.tank)
                        {
                            int hash = TechName.GetHashCode();
                            if (techUnloaded.tank.name.GetHashCode() == hash)
                            {
                                ManSaveGame.inst.GetStoredTile(techUnloaded.tileCache.tile.Coord, false).RemoveSavedVisible(ObjectTypes.Vehicle, TechID);
                            }
                        }
                        SMUtil.Assert(false, "SubMissions: TrackedTech - Found Tech " + TechName + ", but out-of-play Tech is not loaded correctly!  Of mission " + mission.Name + ", tree " + mission.Tree.TreeName);
                        SMUtil.Assert(false, "SubMissions: TrackedTech - COULD NOT ELIMINATE TECH " + TechName + "!  Of mission " + mission.Name + ", tree " + mission.Tree.TreeName);
                    }
                }
                else
                {
                    if (tech != Singleton.playerTank)
                    {
                        Debug_SMissions.Log("SubMissions: Removing " + TechName + " | " + StackTraceUtility.ExtractStackTrace());
                        tech.visible.RemoveFromGame();
                    }
                    else
                        SMUtil.Assert(false, "SubMissions: TrackedTech.DestroyTech - Found Tech " + TechName + " is not valid because it is the player Tech!  Of mission " + mission.Name + ", tree " + mission.Tree.TreeName);
                }
                destroyed = true;
            }
            catch
            {
                SMUtil.Assert(false, "SubMissions: TrackedTech - COULD NOT ELIMINATE TECH " + TechName + "!  Of mission " + mission.Name + ", tree " + mission.Tree.TreeName);
            }
            if (isDisposible)
            {
                mission.TrackedTechs.Remove(this);
            }
        }
        public Tank TryFindMatchingTech()
        {
            if (TechName == null)
                TechName = "Unset";
            int hash = TechName.GetHashCode();
            foreach (Tank tank in Singleton.Manager<ManTechs>.inst.CurrentTechs)
            {
                if (tank.name.GetHashCode() == hash && tank.visible.ID == TechID)
                {
                    return tank;
                }
            }
            Visible techUnloaded = ManSaveGame.inst.LookupSerializedVisible(TechID);
            if (techUnloaded)
            {
                if (techUnloaded.tank)
                {
                    if (techUnloaded.tank.name.GetHashCode() == hash)
                        return techUnloaded.tank;
                }
                SMUtil.Assert(false, "SubMissions: TrackedTech - Found Tech " + TechName + ", but out-of-play Tech is not loaded correctly!  Of mission " + mission.Name + ", tree " + mission.Tree.TreeName);
                return null;
            }
            SMUtil.Assert(true, "SubMissions: TrackedTech - COULD NOT FIND TECH " + TechName + "!  Of mission " + mission.Name + ", tree " + mission.Tree.TreeName);
            return null;
        }
        public void CheckWasDestroyed(Tank techIn)
        {
            if (techIn.visible.ID == TechID)//techIn.name == TechName &&
            {
                destroyed = true;
                if (isDisposible)
                {
                    mission.TrackedTechs.Remove(this);
                }
            }
            //else
            //    Debug_SMissions.Log("SubMissions: Tech does not match " + TechName + " of ID " + TechID + ".");
        }
    }
}
