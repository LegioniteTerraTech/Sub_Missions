using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.Templates;

namespace Sub_Missions.Steps
{
    public class StepCheckDestroy : SMissionStep
    {
        public override string GetDocumentation()
        {
            return
                "{  // Checks if a certain amount of Techs were destroyed or a TrackedTech was destroyed" +
                  "\n  \"StepType\": \"CheckDestroy\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  \"SuccessProgressID\": 0,      // The ProgressID the mission will be pushed to if the VaribleType is DoProgressID and the subject is destroyed" +
                  "\n  // Input Parameters" +
                  "\n  \"InputNum\": 0.0,             // If this value is greater than zero, this will be treated for multiple Techs" +
                  "\n  \"InputString\": \"TechName\",   // The name of the TrackedTech to watch.  If InputNum is greater than 0, then this tracks the respective corp kills.\n   // Leave empty to track all kills (if InputNum is greater than 0)." +
                  "\n  // Output Results" +
                  "\n  \"VaribleType\": \"True\",       // See the top of this file." +
                  "\n  \"SetMissionVarIndex1\": -1,        // the index to apply the output from this" +
                "\n},";
        }

        public override void OnInit() { }

        public override void FirstSetup()
        {
            if (SMission.InputNum != 0)
            {   //track kills
                if (SubMissionTree.GetTreeCorp(SMission.InputString, out FactionSubTypes result))
                {   // Get the kill count of a certain Corp if applicable
                    SMission.SavedInt = Singleton.Manager<ManStats>.inst.GetNumEnemyTechsDestroyedByFaction(result);
                }
                else
                    SMission.SavedInt = Singleton.Manager<ManStats>.inst.GetTotalNumEnemyTechsDestroyed();
            }
        }
        public override void Trigger()
        {
            if (SMission.InputNum != 0)
            {   //track kills
                try
                {
                    if (SubMissionTree.GetTreeCorp(SMission.InputString, out FactionSubTypes result))
                    {   // Get the kill count of a certain Corp if applicable
                        while (SMission.SavedInt < Singleton.Manager<ManStats>.inst.GetNumEnemyTechsDestroyedByFaction(result))
                        {
                            SMUtil.ConcludeGlobal1(ref SMission);
                            SMission.SavedInt++;
                        }
                    }
                    else
                    {   // Get the general kill count 
                        while (SMission.SavedInt < Singleton.Manager<ManStats>.inst.GetTotalNumEnemyTechsDestroyed())
                        {
                            SMUtil.ConcludeGlobal1(ref SMission);
                            SMission.SavedInt++;
                        }
                    }
                }
                catch { }
            }
            else
            {   //Run the single-tech
                if (SMUtil.GetTrackedTechBase(ref SMission, SMission.InputString).destroyed)
                {   // target destroyed
                    SMUtil.ConcludeGlobal1(ref SMission);
                }
            }
        }
        public override void OnDeInit()
        {
        }
    }
}
