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


        public byte AddProgressX = 0;
        public byte AddProgressY = 0;

        public int EXPGain = 0;
        public int MoneyGain = 0;


        public int RandomBlocksToSpawn = 0;
        public List<BlockTypes> BlocksToSpawn = new List<BlockTypes>();

        public void Reward(CustomSubMissionTree tree)
        {
            tree.ProgressX += AddProgressX;
            tree.ProgressY += AddProgressY;
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
                    Singleton.Manager<ManLicenses>.inst.AddXP((FactionSubTypes)Enum.Parse(typeof(FactionSubTypes), tree.Faction), EXPGain, true);
                }
                catch
                {
                    Debug.Log("SubMissions: Tried to add EXP to a faction that doesn't exist!  SubMissionReward of Tree " + tree.TreeName);
                }
            }
            if (RandomBlocksToSpawn > 0 || BlocksToSpawn.Count > 0)
            {
                //Crate.Definition crate = new Crate.Definition();
                //crate.m_Locked = false;

                //List<Crate.ItemDefinition> items = new List<Crate.ItemDefinition>();
                List<BlockTypes> items = new List<BlockTypes>(BlocksToSpawn);
                for (int step = 0; step < RandomBlocksToSpawn; step++)
                {
                    BlockTypes RANDtype = BlockTypes.GSOBlock_111;// still pending...
                    //items.Add(new Crate.ItemDefinition { m_BlockType = RANDtype})
                    items.Add(RANDtype);
                }

                //crate.m_Contents = items.ToArray();
                Vector3 landingPos = Singleton.playerPos + (Vector3.forward * Singleton.playerTank.blockBounds.size.magnitude);
                Singleton.Manager<ManSpawn>.inst.RewardSpawner.RewardBlocksByCrate(items.ToArray(), landingPos);
                //Singleton.Manager<ManSpawn>.inst.SpawnCrateRef("GSO_Crate", crate, landingPos, Quaternion.identity, true, true);
            }
        }
    }
}
