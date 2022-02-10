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
        public override string GetDocumentation()
        {
            return
                "{  // Updates a VarInt based on the specified TrackedTech's Block Count" +
                  "\n  \"StepType\": \"CheckHealth\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  \"SuccessProgressID\": 0,      // The ProgressID the mission will be pushed to if " +
                  "\n  // Input Parameters" +
                  "\n  \"InputString\": \"TechName\",   // The name of the TrackedTech to watch.  Leave empty to watch the Player Tech." +
                  "\n  \"SetMissionVarIndex1\": -1,        // the index to apply the output from this" +
                "\n},";
        }

        public override void OnInit() { }

        public override void FirstSetup()
        {
        }
        public override void Trigger()
        {
            try
            {
                if (SMission.InputString.NullOrEmpty())
                {
                    if (Singleton.playerTank)
                        Mission.VarInts[SMission.SetMissionVarIndex1] = Singleton.playerTank.blockman.blockCount;
                    else
                        Mission.VarInts[SMission.SetMissionVarIndex1] = 0;
                }
                if (SMUtil.DoesTrackedTechExist(ref SMission, SMission.InputString))
                {
                    TrackedTech tech = SMUtil.GetTrackedTechBase(ref SMission, SMission.InputString);
                    if (tech.destroyed)
                    {
                        Mission.VarInts[SMission.SetMissionVarIndex1] = 0;
                    }
                    else if (!tech.Tech)
                    {   // unloaded
                        Mission.VarInts[SMission.SetMissionVarIndex1] = 262145; // max possible Tech Volume + 1
                    }
                    else
                        Mission.VarInts[SMission.SetMissionVarIndex1] = tech.Tech.blockman.blockCount;
                }
                else
                    SMUtil.Assert(true, "SubMissions: Tech not referenced or missing in " + SMission.Mission.Name + " | Step type " + SMission.StepType.ToString() + " - Check your TrackedTechs, Tech names, and missions for consistancy errors");
            }
            catch
            {
                SMUtil.Assert(true, "SubMissions: Error in output [SetMissionVarIndex1] in mission " + SMission.Mission.Name + " | Step type " + SMission.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) \nand make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry on the list, 1 counts as the second entry, and so on.");
            }
        }
        public override void OnDeInit()
        {
        }
    }
}
