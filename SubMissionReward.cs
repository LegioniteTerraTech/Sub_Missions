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
            if (tree.Faction != FactionSubTypes.NULL.ToString() && EXPGain > 0)
            {
                try
                {
                    Singleton.Manager<ManLicenses>.inst.AddXP((FactionSubTypes)Enum.Parse(typeof(FactionSubTypes), mission.Faction), EXPGain, true);
                }
                catch
                {
                    SMUtil.Assert(false, "SubMissions: Tried to add EXP to a faction that doesn't exist!  SubMissionReward of Tree " + tree.TreeName + ", mission " + mission.Name);
                }
            }
            if (RandomBlocksToSpawn > 0 || BlocksToSpawn.Count > 0)
            {
                List<BlockTypes> items = new List<BlockTypes>(BlocksToSpawn);
                try
                {
                    items.AddRange(Singleton.Manager<ManLicenses>.inst.GetRewardPoolTable().GetRewardBlocks((FactionSubTypes)Enum.Parse(typeof(FactionSubTypes), mission.Faction), RandomBlocksToSpawn).ToList());
                }
                catch
                {
                    SMUtil.Assert(false, "SubMissions: Tried to fetch blocks from a faction that doesn't exist!  SubMissionReward of Tree " + tree.TreeName + ", mission " + mission.Name);
                }
                /*
                for (int step = 0; step < RandomBlocksToSpawn; step++)
                {
                    BlockTypes RANDtype = BlockTypes.GSOBlock_111;// still pending...
                    items.Add(RANDtype);
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
                int fireCount = items.Count;

                for (int step = 0; step < fireCount; step++)
                {   // filter and remove illegal blocks
                    if (!Singleton.Manager<ManSpawn>.inst.IsValidBlockToSpawn(items.ElementAt(step)))
                    {
                        items.RemoveAt(step);
                        fireCount--;
                        step--;
                    }
                }
                Singleton.Manager<ManSpawn>.inst.RewardSpawner.RewardBlocksByCrate(items.ToArray(), landingPos);
            }
        }
    }
}
