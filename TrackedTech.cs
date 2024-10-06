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

        public string TechName = ""; // WITHOUT file prefix for Techs
        public string FileTechName = ""; // WITH prefix for Techs
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
        public Tank Tech => tech;

        public TrackedTech(string nameWorld, string nameFile, bool isDisposable = false)
        {
            TechName = nameWorld;
            FileTechName = nameFile;
            this.isDisposible = isDisposable;
        }

        [JsonIgnore]
        public Tank TechAuto
        {
            get
            {
                if (!loaded && !DeliQueued)
                {
                    if (!mission.GetTechPosHeading(FileTechName, out Vector3 pos, out Vector3 direction, out int team))
                        SMUtil.Error(true, "Mission (TrackedTech) ~ " + mission.Name + " - " + FileTechName, 
                            KickStart.ModID + ": Tech in TrackedTechs list but was never spawned in any Step!!!  In " + mission.Name + " of " + mission.Tree.TreeName + ".");
                    tech = SMUtil.SpawnTechAuto(ref mission, pos, team, direction, FileTechName);
                    SMUtil.Error(true, "Mission (TrackedTech) ~ " + mission.Name + " - " + FileTechName, 
                        KickStart.ModID + ": Tech called into world before a StepSetupTech!  Mission: " + mission.Name);
                    loaded = true;
                }
                else if (DeliQueued && delayedSpawn.IsNull())
                {
                    if (!mission.GetTechPosHeading(FileTechName, out Vector3 pos, out Vector3 direction, out int team))
                        SMUtil.Error(true, "Mission (TrackedTech) ~ " + mission.Name + " - " + FileTechName, 
                            KickStart.ModID + ": Tech in TrackedTechs list but was never spawned in any Step!!!  In " + 
                            mission.Name + " of " + mission.Tree.TreeName + ".");
                    tech = SMUtil.SpawnTechAuto(ref mission, pos, team, direction, FileTechName);
                    SMUtil.Error(true, "Mission (TrackedTech) ~ " + mission.Name + " - " + FileTechName, 
                        KickStart.ModID + ": World was saved before bomb touchdown.  Mission: " + mission.Name);
                    loaded = true;
                    DeliQueued = false;
                }
                if (tech == null && !DeliQueued)
                    TechAuto = TryFindMatchingTech();
                return tech;
            }
            set
            {
                tech = value;
                TechName = value.name;
                TechID = value.visible.ID;
                loaded = true;
                SMUtil.Log(false, "Tech assigned (TrackedTech) ~ " + mission.Name + " - " + FileTechName + ", name: " + TechID + 
                    ", ID:" + TechID);
            }
        }


        public void SpawnTech(Vector3 pos)
        {
            try
            {
                if (!mission.TrackedTechs.Contains(this))
                    return; // denied as it's been removed from the pool
                if (!mission.GetTechPosHeading(FileTechName, out _, out Vector3 direction, out int team))
                    SMUtil.Error(true, "Mission (TrackedTech) ~ " + mission.Name + " - " + FileTechName, 
                        KickStart.ModID + ": Tech in TrackedTechs list but was never called in any Step!!!  In " + mission.Name + " of " + mission.Tree.TreeName + ".");
                
                TechAuto = SMUtil.SpawnTechAuto(ref mission, pos, team, direction, FileTechName);

                DeliQueued = false;
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "Mission (TrackedTech) ~ " + mission.Name, KickStart.ModID + ": TrackedTech (Delayed) - COULD NOT SPAWN TECH!  Of mission " + 
                    mission.Name + ", tree " + mission.Tree.TreeName, e);
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
                        SMUtil.Error(false, "Mission (TrackedTech) ~ " + mission.Name + " - " + FileTechName, 
                            KickStart.ModID + ": TrackedTech - Found Tech " + TechName + 
                            ", but out-of-play Tech is not loaded correctly!  Of mission " + mission.Name + 
                            ", tree " + mission.Tree.TreeName);
                        SMUtil.Error(false, "Mission (TrackedTech) ~ " + mission.Name + " - " + FileTechName, 
                            KickStart.ModID + ": TrackedTech - COULD NOT ELIMINATE TECH " + TechName + 
                            "!  Of mission " + mission.Name + ", tree " + mission.Tree.TreeName);
                    }
                }
                else
                {
                    if (tech != Singleton.playerTank)
                    {
                        SMUtil.Log(false, KickStart.ModID + ": Removing " + TechName + " | " + StackTraceUtility.ExtractStackTrace());
                        tech.visible.RemoveFromGame();
                    }
                    else
                        SMUtil.Error(false, "Mission (TrackedTech) ~ " + mission.Name + " - " + FileTechName, 
                            KickStart.ModID + ": TrackedTech.DestroyTech - Found Tech " + TechName + 
                            ", but it is NOT VALID because it is the player Tech!  Of mission " + mission.Name + 
                            ", tree " + mission.Tree.TreeName);
                }
                destroyed = true;
            }
            catch (Exception e)
            {
                SMUtil.Assert(false, "Mission (TrackedTech) ~ " + mission.Name + " - " + FileTechName, KickStart.ModID + ": TrackedTech - COULD NOT ELIMINATE TECH " + TechName + 
                    "!  Of mission " + mission.Name + ", tree " + mission.Tree.TreeName, e);
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
                if (tank.name.GetHashCode() == hash || tank.visible.ID == TechID)
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
                SMUtil.Error(false, "Mission (TrackedTech) ~ " + mission.Name + " - " + FileTechName, 
                    KickStart.ModID + ": TrackedTech - Found Tech " + TechName + 
                    ", but out-of-play Tech is not loaded correctly!  Of mission " + mission.Name + 
                    ", tree " + mission.Tree.TreeName);
                return null;
            }
            SMUtil.Error(false, "Mission (TrackedTech) ~ " + mission.Name + " - " + FileTechName, 
                KickStart.ModID + ": TrackedTech - COULD NOT FIND TECH " + TechName + 
                "!  Of mission " + mission.Name + ", tree " + mission.Tree.TreeName);
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
            //    Debug_SMissions.Log(KickStart.ModID + ": Tech does not match " + TechName + " of ID " + TechID + ".");
        }
    }
}
