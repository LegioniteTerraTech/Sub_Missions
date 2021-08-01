using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;

namespace Sub_Missions.Steps
{
    public class StepCheckHealth : SMissionStep
    {   // keeps track of a single tech's block count and outputs to the assigned Global1
        public override void TrySetup()
        {
        }
        public override void Trigger()
        {
            try
            {
                if (SMUtil.DoesTrackedTechExist(ref SMission, SMission.InputString))
                {
                    TrackedTech tech = SMUtil.GetTrackedTechBase(ref SMission, SMission.InputString);
                    if (tech.destroyed)
                    {
                        Mission.VarInts[SMission.SetGlobalIndex1] = 0;
                    }
                    else if (!tech.Tech)
                    {   // unloaded
                        Mission.VarInts[SMission.SetGlobalIndex1] = 999999;
                    }
                    else
                        Mission.VarInts[SMission.SetGlobalIndex1] = tech.Tech.blockman.blockCount;
                }
                else
                    SMUtil.Assert(true, "SubMissions: Tech not referenced or missing in " + SMission.Mission.Name + " | Step type " + SMission.StepType.ToString() + " - Check your TrackedTechs, Tech names, and missions for consistancy errors");
            }
            catch
            {
                SMUtil.Assert(true, "SubMissions: Error in output [SetGlobalIndex1] in mission " + SMission.Mission.Name + " | Step type " + SMission.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) \nand make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry on the list, 1 counts as the second entry, and so on.");
            }
        }
    }
}
