﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TAC_AI.AI;
using TAC_AI.AI.Enemy;
using TAC_AI.Templates;

namespace Sub_Missions.Steps
{
    public class StepChangeAI : SMissionStep
    {
        public override void TrySetup()
        {  

        }
        public override void Trigger()
        {   
            if (SMUtil.BoolOut(ref SMission) && SMission.SavedInt == 0)
            {
                try
                {
                    TrackedTech tTech = SMUtil.GetTrackedTechBase(ref Mission, SMission.InputString);
                    if (tTech.Tech.GetComponent<EnemyMind>())
                    {
                        EnemyMind mind = tTech.Tech.GetComponent<EnemyMind>();
                        IntToEnemyAI(ref mind, (int)SMission.InputNum);
                        try
                        {
                            if (SMission.InputStringAux != "" || SMission.InputStringAux != null)
                            {   // allow the AI to change it's form on demand
                                mind.TechMemor.MemoryToTech(AIERepair.DesignMemory.JSONToTechExternal(SMission.InputStringAux));
                            }
                        }
                        catch { }
                    }
                    else
                    {   // it's allied 
                        AIECore.TankAIHelper help = tTech.Tech.GetComponent<AIECore.TankAIHelper>();
                        if (help.AIState == 1)
                        {   // it's allied 
                            help.DediAI = (AIType)(int)SMission.InputNum;
                            try
                            {
                                if (SMission.InputStringAux != "" || SMission.InputStringAux != null)
                                {   // allow the AI to change it's form on demand
                                    help.TechMemor.MemoryToTech(AIERepair.DesignMemory.JSONToTechExternal(SMission.InputStringAux));
                                }
                            }
                            catch { }
                        }
                        else
                        {   // it's neutral
                            try
                            {
                                if (SMission.InputStringAux != "" || SMission.InputStringAux != null)
                                {   // allow the AI to change it's form on demand
                                    help.TechMemor.MemoryToTech(AIERepair.DesignMemory.JSONToTechExternal(SMission.InputStringAux));
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch //(Exception e)
                {   // Cannot work without TACtical_AI
                    //SMUtil.Assert(true, "SubMissions: StepSetupTech (Infinite) - Failed: COULD NOT FETCH INFORMATION!!!");
                    //Debug.Log("SubMissions: Stack trace - " + StackTraceUtility.ExtractStackTrace());
                    //Debug.Log("SubMissions: Error - " + e);
                }
            }
        }

        public void IntToEnemyAI(ref EnemyMind mind, int input)
        {
            string coding = input.ToString();
            int position = 0;
            foreach (char ch in coding)
            {
                switch (position)
                {
                    case 0: //EvilCommander
                        if (Enum.TryParse(ch.ToString(), out EnemyHandling result))
                            mind.EvilCommander = result;
                        break;
                    case 1: //CommanderAttack
                        if (Enum.TryParse(ch.ToString(), out EnemyAttack result2))
                            mind.CommanderAttack = result2;
                        break;
                    case 2: //CommanderAttitude
                        if (Enum.TryParse(ch.ToString(), out EnemyAttitude result3))
                            mind.CommanderMind = result3;
                        break;
                    case 3: //CommanderSmarts
                        if (Enum.TryParse(ch.ToString(), out EnemySmarts result4))
                            mind.CommanderSmarts = result4;
                        break;
                    case 4: //CommanderSmarts
                        if (Enum.TryParse(ch.ToString(), out EnemyBolts result5))
                            mind.CommanderBolts = result5;
                        break;
                    default:
                        SMUtil.Assert(true, "SubMissions: ChangeAI has more than 5 inputted numbers, beyond that of the options changeable.  Mission " + Mission.Name);
                        break;
                }
                position++;
            }
        }
    }
}