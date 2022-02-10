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
        public override void TrySetup()
        {
            if (SMission.InputNum != 0)
            {   //track kills
                if (Enum.TryParse(SMission.InputString, out FactionSubTypes result))
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
                    if (Enum.TryParse(SMission.InputString, out FactionSubTypes result))
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
    }
}
