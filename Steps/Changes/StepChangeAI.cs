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
        public override bool ForceUsesVarBool() => false;
        public override bool ForceUsesVarInt() => false;
        public override string GetTooltip() =>
            "Existing Tech AI changer";
        public override string GetDocumentation()
        {
            return
                "{  // " + GetTooltip() +
                  "\n  \"StepType\": \"ChangeAI\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"True\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that is checked against VaribleType to tell if it should execute." +
                  "\n  // Input Parameters" +
                  "\n  \"InputString\": \"TechName\",   // The name of the TrackedTech to change." +
                  "\n  \"InputStringAux\": null,      // Changes mindsets of the AI (ABCDE)" +
                  "\n  // A = EnemyHandling" +
                  "\n  // B = EnemyAttack" +
                  "\n  // C = EnemyAttitude" +
                  "\n  // D = EnemySmarts" +
                  "\n  // E = EnemyBolts" +
                "\n},";
        }
        public override void InitGUI()
        {
            AddField(ESMSFields.VaribleType, "Condition Mode");
            AddField(ESMSFields.VaribleCheckNum, "Conditional Constant");
            AddGlobal(ESMSFields.SetMissionVarIndex1, "Active Condition", EVaribleType.Int);
            AddField(ESMSFields.InputString_Tracked_Tech, "Tracked Tech");
            AddField(ESMSFields.InputStringAux, "AI Modes");
        }

        public override void OnInit() { }

        public override void FirstSetup()
        {
            try
            {
                TrackedTech tTech = SMUtil.GetTrackedTechBase(ref Mission, SMission.InputString);
                if (tTech.TechAuto.GetComponent<EnemyMind>())
                {
                    EnemyMind mind = tTech.TechAuto.GetComponent<EnemyMind>();
                    IntToEnemyAI(ref mind, (int)SMission.InputNum);
                }
            }
            catch //(Exception e)
            {   // Cannot work without TACtical_AI
                //SMUtil.Assert(true, KickStart.ModID + ": StepSetupTech (Infinite) - Failed: COULD NOT FETCH INFORMATION!!!");
                //Debug_SMissions.Log(KickStart.ModID + ": Stack trace - " + StackTraceUtility.ExtractStackTrace());
                //Debug_SMissions.Log(KickStart.ModID + ": Error - " + e);
            }
        }
        public override void Trigger()
        {   
            if (SMUtil.BoolOut(ref SMission) && SMission.SavedInt == 0)
            {
                try
                {
                    TrackedTech tTech = SMUtil.GetTrackedTechBase(ref Mission, SMission.InputString);
                    if (tTech.TechAuto.GetComponent<EnemyMind>())
                    {
                        EnemyMind mind = tTech.TechAuto.GetComponent<EnemyMind>();
                        IntToEnemyAI(ref mind, (int)SMission.InputNum);
                        /*
                        try
                        {
                            if (SMission.InputStringAux != null && SMission.InputStringAux.Length > 0)
                            {   // allow the AI to change it's form on demand
                                mind.TechMemor.MemoryToTech(AIERepair.DesignMemory.JSONToMemoryExternal(SMission.InputStringAux));
                            }
                        }
                        catch { }*/
                    }
                    else
                    {   // it's allied  (OBSOLETE - HANDLED BY StepTransformTech now)
                        /*
                        AIECore.TankAIHelper help = tTech.Tech.GetComponent<AIECore.TankAIHelper>();
                        if (help.AIState == 1)
                        {   // it's allied 
                            help.DediAI = (AIType)(int)SMission.InputNum;
                            try
                            {
                                if (SMission.InputStringAux != null && SMission.InputStringAux.Length > 0)
                                {   // allow the AI to change it's form on demand
                                    help.TechMemor.MemoryToTech(AIERepair.DesignMemory.JSONToMemoryExternal(SMission.InputStringAux));
                                }
                            }
                            catch { }
                        }
                        else
                        {   // it's neutral
                            try
                            {
                                if (SMission.InputStringAux != null && SMission.InputStringAux.Length > 0)
                                {   // allow the AI to change it's form on demand
                                    help.TechMemor.MemoryToTech(AIERepair.DesignMemory.JSONToMemoryExternal(SMission.InputStringAux));
                                }
                            }
                            catch { }
                        }*/
                    }
                }
                catch //(Exception e)
                {   // Cannot work without TACtical_AI
                    //SMUtil.Assert(true, KickStart.ModID + ": StepSetupTech (Infinite) - Failed: COULD NOT FETCH INFORMATION!!!");
                    //Debug_SMissions.Log(KickStart.ModID + ": Stack trace - " + StackTraceUtility.ExtractStackTrace());
                    //Debug_SMissions.Log(KickStart.ModID + ": Error - " + e);
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
                        if (Enum.TryParse(ch.ToString(), out EAttackMode result2))
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
                        SMUtil.Error(true, SMission.LogName, 
                            KickStart.ModID + ": ChangeAI has more than 5 inputted numbers, beyond that " +
                            "of the options changeable.  Mission " + Mission.Name);
                        break;
                }
                position++;
            }
        }
        public override void OnDeInit()
        {
        }
    }
}
