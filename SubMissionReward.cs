using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions
{
    public class SubMissionReward
    {   //  
        public sbyte AddProgressX = 0;
        public sbyte AddProgressY = 0;

        public string CorpToGiveEXP;
        public int EXPGain = 0;
        public int MoneyGain = 0;


        public int RandomBlocksToSpawn = 0;
        public List<BlockTypes> BlocksToSpawn = new List<BlockTypes>();

        public void TryChange(ref sbyte input, sbyte toChangeBy)
        {
            if ((int)input + (int)toChangeBy > sbyte.MaxValue)
                input = sbyte.MaxValue;
            else if ((int)input + (int)toChangeBy < sbyte.MinValue)
                input = sbyte.MinValue;
            else
                input += toChangeBy;
        }
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
                                Debug.Log("SubMissions: Unlocking corp ID " + FST + ", name: " + CL.FullName);
                                Singleton.Manager<ManLicenses>.inst.UnlockCorp(FST, true);
                            }
                            else
                                Debug.Log("SubMissions: Could not unlock corp ID " + FST + ", name: NOT_REGISTERED");
                        }
                    }
                    else
                    {
                        FST = tree.GetTreeCorp(CorpToGiveEXP);
                        if (Singleton.Manager<ManLicenses>.inst.IsLicenseDiscovered(FST))
                            Singleton.Manager<ManLicenses>.inst.AddXP(FST, EXPGain, true);
                        else
                        {
                            if (ManSMCCorps.TryGetSMCCorpLicense((int)FST, out SMCCorpLicense CL))
                                Debug.Log("SubMissions: Unlocking corp ID " + FST + ", name: " + CL.FullName);
                            else
                                Debug.Log("SubMissions: Unlocking corp ID " + FST + ", name: NOT_REGISTERED");
                            Singleton.Manager<ManLicenses>.inst.UnlockCorp(FST, true);
                        }
                    }
                }
                catch
                {
                    SMUtil.Assert(false, "SubMissions: Tried to add EXP to a faction " + tree.Faction + " that doesn't exist!  SubMissionReward of Tree " + tree.TreeName + ", mission " + mission.Name);
                }
            }
            if (RandomBlocksToSpawn > 0 || BlocksToSpawn.Count > 0)
            {
                List<BlockTypes> items = new List<BlockTypes>(BlocksToSpawn);
                if ((int)FST > ManSMCCorps.UCorpRange)
                {
                    if (ManSMCCorps.TryGetSMCCorpLicense((int)FST, out SMCCorpLicense CL))
                    {
                        items.AddRange(CL.GetRandomBlocks(Singleton.Manager<ManLicenses>.inst.GetCurrentLevel((FactionSubTypes)CL.ID), RandomBlocksToSpawn));
                    }
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
                }

                Vector3 landingPos;
                try
                {
                    landingPos = Singleton.playerPos + (Vector3.forward * Singleton.playerTank.blockBounds.size.magnitude);
                }
                catch 
                {
                    landingPos = Singleton.cameraTrans.position;
                }
                int fireCount = items.Count;

                for (int step = 0; step < fireCount; step++)
                {   // filter and remove broken blocks
                    if (!Singleton.Manager<ManSpawn>.inst.IsValidBlockToSpawn(items.ElementAt(step)))
                    {
                        items.RemoveAt(step);
                        fireCount--;
                        step--;
                    }
                }
                if ((int)FST > ManSMCCorps.UCorpRange)
                {
                    if (ManSMCCorps.TryGetSMCCorpLicense((int)FST, out SMCCorpLicense CL))
                    {
                        if (CL.HasCratePrefab)
                            Singleton.Manager<ManSpawn>.inst.RewardSpawner.RewardBlocksByCrate(items.ToArray(), landingPos, FST);
                        else
                            Singleton.Manager<ManSpawn>.inst.RewardSpawner.RewardBlocksByCrate(items.ToArray(), landingPos, FactionSubTypes.GSO);
                        return;
                    }
                }
                Singleton.Manager<ManSpawn>.inst.RewardSpawner.RewardBlocksByCrate(items.ToArray(), landingPos, FST);
            }
        }
    }
}
