using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace Sub_Missions
{
    public class SMCCorpBlockRange
    {
        public BlockTypes BlocksRangeStart = (BlockTypes)5001;
        public BlockTypes BlocksRangeEnd = (BlockTypes)5002;
        public List<BlockTypes> BlocksOutOfRange = new List<BlockTypes>();
        public List<BlockTypes> BlocksToGiveAtEndGrade = new List<BlockTypes>();

        [JsonIgnore]
        public List<BlockTypes> BlocksAvail
        {
            get
            {
                if (blocksAvail == null)
                    blocksAvail = GetAllAvailBlocks();
                return blocksAvail;
            }
        }
        public List<BlockTypes> blocksAvail;

        public BlockTypes[] GetGradeUpBlocks()
        {   //
            try
            {
                if (BlocksToGiveAtEndGrade.Count == 0)
                    return new BlockTypes[3] { GetRandomBlock(), GetRandomBlock(), GetRandomBlock() };
                return BlocksToGiveAtEndGrade.ToArray();
            }
            catch (Exception e)
            {
                Debug_SMissions.Log("SubMissions: GetRandomBlock - BLOCK could not be obtained - " + e);
            }
            return new BlockTypes[3] { GetRandomBlock(), GetRandomBlock(), GetRandomBlock() };
        }
        public BlockTypes GetRandomBlock()
        {   //
            try
            {
                if (BlocksAvail.Count == 0)
                    return BlockTypes.GSOAIController_111;
                return blocksAvail.GetRandomEntry();
            }
            catch (Exception e)
            {
                Debug_SMissions.Log("SubMissions: GetRandomBlock - BLOCK could not be obtained - " + e);
            }
            return BlockTypes.GSOAIController_111;
        }

        private List<BlockTypes> GetAllAvailBlocks()
        {   //
            List<BlockTypes> BTs = new List<BlockTypes>();
            if (BlocksRangeStart > (BlockTypes)5001 && BlocksRangeStart < BlocksRangeEnd)
            {
                int countTo = (int)BlocksRangeEnd;
                for (int step = (int)BlocksRangeStart; step <= countTo; step++)
                {
                    BlockTypes BT = (BlockTypes)step;
                    if (ManSpawn.inst.IsTankBlockLoaded(BT))
                        BTs.Add(BT);
                }
            }
            if (BlocksOutOfRange != null)
            {
                foreach (BlockTypes BTc in BlocksOutOfRange)
                {
                    if (ManSpawn.inst.IsTankBlockLoaded(BTc))
                        BTs.Add(BTc);
                }
            }
            return BTs;
        }
    }
}
