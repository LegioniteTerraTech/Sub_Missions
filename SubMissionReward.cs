using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TerraTechETCUtil;

namespace Sub_Missions
{
    public class SubMissionReward
    {   //  
        public sbyte AddProgressX = 0;
        public sbyte AddProgressY = 0;

        public string CorpToGiveEXP = "GSO";
        public int EXPGain = 0;
        public int MoneyGain = 0;


        public int RandomBlocksToSpawn = 0;
        public List<string> BlocksToSpawn = new List<string>();
        private static List<BlockTypes> BTsCache = new List<BlockTypes>();
        public List<BlockTypes> BlockTypesToSpawnIterate {
            get {
                BTsCache.Clear();
                foreach (var item in BlocksToSpawn)
                {
                    _ = BlockIndexer.StringToBlockType(item, out BlockTypes BT);
                    BTsCache.Add(BT);
                }
                return BTsCache;
            }
        }

        public void TryChange(ref sbyte input, sbyte toChangeBy)
        {
            if ((int)input + (int)toChangeBy > sbyte.MaxValue)
                input = sbyte.MaxValue;
            else if ((int)input + (int)toChangeBy < sbyte.MinValue)
                input = sbyte.MinValue;
            else
                input += toChangeBy;
        }
        private static List<BlockTypes> itemsCached = new List<BlockTypes>();
        public void Reward(SubMissionTree tree, SubMission mission)
        {
            TryChange(ref tree.ProgressX, AddProgressX);
            TryChange(ref tree.ProgressY, AddProgressY);
            if (MoneyGain > 0)
            {
                try
                {
                    Singleton.Manager<ManPlayer>.inst.AddMoney(MoneyGain);
                }
                catch { }
            }
            FactionSubTypes FST = tree.GetTreeCorp();
            if ((int)FST != 0 && EXPGain > 0)
            {
                try
                {
                    if (CorpToGiveEXP == null)
                    {
                        if (Singleton.Manager<ManLicenses>.inst.IsLicenseDiscovered(FST))
                            Singleton.Manager<ManLicenses>.inst.AddXP(FST, EXPGain, true);
                        else
                        {
                            if (ManSMCCorps.TryGetSMCCorpLicense((int)FST, out SMCCorpLicense CL))
                            {
                                Debug_SMissions.Log("SubMissions: Unlocking corp ID " + FST + ", name: " + CL.FullName);
                                Singleton.Manager<ManLicenses>.inst.UnlockCorp(FST, true);
                                UICCorpLicenses.ShowFactionLicenseOfficialUI((int)FST);
                            }
                            else
                                Debug_SMissions.Log("SubMissions: Could not unlock corp ID " + FST + ", name: NOT_REGISTERED");
                        }
                    }
                    else
                    {
                        FST = SubMissionTree.GetTreeCorp(CorpToGiveEXP);
                        if (Singleton.Manager<ManLicenses>.inst.IsLicenseDiscovered(FST))
                            Singleton.Manager<ManLicenses>.inst.AddXP(FST, EXPGain, true);
                        else
                        {
                            if (ManSMCCorps.TryGetSMCCorpLicense((int)FST, out SMCCorpLicense CL))
                                Debug_SMissions.Log("SubMissions: Unlocking corp ID " + FST + ", name: " + CL.FullName);
                            else
                                Debug_SMissions.Log("SubMissions: Unlocking corp ID " + FST + ", name: NOT_REGISTERED");
                            Singleton.Manager<ManLicenses>.inst.UnlockCorp(FST, true);
                        }
                    }
                }
                catch (Exception e)
                {
                    SMUtil.Assert(false, "Mission (Reward) ~ " + mission.Name, "SubMissions: Tried to add EXP to a faction " + tree.Faction + 
                        " that doesn't exist!  SubMissionReward of Tree " + tree.TreeName + ", mission " + 
                        mission.Name, e);
                    try
                    {
                        FactionLicense FL = Singleton.Manager<ManLicenses>.inst.GetLicense(FST);
                        Debug_SMissions.Log("instance? " + FL);
                        Debug_SMissions.Log("instance? " + FL.Corporation);
                        Debug_SMissions.Log("instance? " + FL.IsDiscovered);
                        Debug_SMissions.Log("instance? " + FL.CurrentAbsoluteXP);
                    }
                    catch { }
                }
            }
            if (RandomBlocksToSpawn > 0 || BlocksToSpawn.Count > 0)
            {
                SMCCorpLicense CL;
                itemsCached.AddRange(BlockTypesToSpawnIterate);
                //if (ManSMCCorps.IsUnofficialSMCCorpLicense(FST))
                //{
                if (ManSMCCorps.TryGetSMCCorpLicense((int)FST, out CL))
                {
                    itemsCached.AddRange(CL.GetRandomBlocks(Singleton.Manager<ManLicenses>.inst.GetCurrentLevel((FactionSubTypes)CL.ID), RandomBlocksToSpawn));
                }
                    /*
                }
                else
                {
                    try
                    {
                        items.AddRange(Singleton.Manager<ManLicenses>.inst.GetRewardPoolTable().GetRewardBlocks((FactionSubTypes)Enum.Parse(typeof(FactionSubTypes), mission.Faction), RandomBlocksToSpawn).ToList());
                    }
                    catch
                    {
                        SMUtil.Assert(false, "SubMissions: Tried to fetch blocks from a faction that doesn't exist!  SubMissionReward of Tree " + tree.TreeName + ", mission " + mission.Name);
                    }
                }*/

                Vector3 landingPos;
                try
                {
                    landingPos = Singleton.playerPos + (Vector3.forward * Singleton.playerTank.blockBounds.size.magnitude);
                }
                catch 
                {
                    landingPos = Singleton.cameraTrans.position;
                }
                int fireCount = itemsCached.Count;

                for (int step = 0; step < fireCount; step++)
                {   // filter and remove broken blocks
                    if (!ManSpawn.inst.IsBlockAllowedInCurrentGameMode(itemsCached.ElementAt(step)))
                    {
                        itemsCached.RemoveAt(step);
                        fireCount--;
                        step--;
                    }
                }
                if (ManSMCCorps.TryGetSMCCorpLicense((int)FST, out CL))
                {
                    if (CL.HasCratePrefab)
                    {
                        Debug_SMissions.Log("Spawning Set Crate");
                        ManSpawn.inst.RewardSpawner.RewardBlocksByCrate(itemsCached.ToArray(), landingPos, FST);
                    }
                    else
                    {
                        Debug_SMissions.Log("Spawning " + CL.CrateReferenceFaction + " Crate");
                        Singleton.Manager<ManSpawn>.inst.RewardSpawner.RewardBlocksByCrate(itemsCached.ToArray(), landingPos, CL.CrateReferenceFaction);
                    }
                    return;
                }
                Debug_SMissions.Log("Spawning Default Crate");
                Singleton.Manager<ManSpawn>.inst.RewardSpawner.RewardBlocksByCrate(itemsCached.ToArray(), landingPos, FactionSubTypes.GSO);
                itemsCached.Clear();
            }
        }
    }
}
