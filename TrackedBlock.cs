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
    public class TrackedBlock
    {   //  
        [JsonIgnore]
        public SubMission mission;

        public BlockTypes UnofficialBlockID = BlockTypes.GSOAIController_111;
        public string BlockName = "";
        public int blockID = 0;

        public bool loaded = false;

        [JsonIgnore]
        private TankBlock block;

        [JsonIgnore]
        public TankBlock Block
        {
            get
            {
                if (!loaded)
                {
                    if (!mission.GetBlockPos(BlockName, out Vector3 pos))
                        Debug_SMissions.Log("SubMissions: Block in TrackedBlocks list but was never called in any Step!!!  In " + mission.Name + " of " + mission.Tree.TreeName + ".");
                    //block = SMUtil.SpawnTechAuto(ref mission, pos);
                    loaded = true;
                }
                if (block == null)
                    block = TryFindMatchingBlock();
                return block;
            }
            set
            {
                block = value;
                BlockName = value.name;
                blockID = value.visible.ID;
                loaded = true;
            }
        }
        public bool SpawnBlock(Vector3 scenePos)
        {
            if (Block)
                return true;
            if ((int)UnofficialBlockID < 1)
            {
                BlockName = ManSpawn.inst.GetBlockPrefab(UnofficialBlockID).name;
            }
            else
            {
                UnofficialBlockID = (BlockTypes)ManMods.inst.GetBlockID(BlockName);
            }
            Block = ManLooseBlocks.inst.HostSpawnBlock(UnofficialBlockID, scenePos, Quaternion.identity, true);
            return block;
        }
        public TankBlock TryFindMatchingBlock()
        {
            foreach (Visible vis in Singleton.Manager<ManVisible>.inst.VisiblesTouchingRadius(Singleton.playerPos, 1000, new Bitfield<ObjectTypes>()))
            {
                if (!(bool)vis.block)
                    continue;
                if (vis.name == BlockName && vis.block.visible.ID == blockID)
                {
                    return vis.block;
                }
            }
            Visible blockUnloaded = ManSaveGame.inst.LookupSerializedVisible(blockID);
            if (blockUnloaded)
            {
                if (!(bool)blockUnloaded.block)
                {
                    SMUtil.Assert(false, "SubMissions: TrackedBlock - ID DOES NOT MATCH SUBJECT TARGET");
                    return null;
                }
                if (blockUnloaded == block.visible)
                return blockUnloaded.block;
            }
            return null;
        }
        public void Remove()
        {
            if (block)
            {
                ManLooseBlocks.inst.HostDestroyBlock(block);
            }
        }
    }
}
