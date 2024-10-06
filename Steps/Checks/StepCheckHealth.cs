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

        public override void InitGUI()
        {
            AddField(ESMSFields.VaribleCheckNum, "Conditional Constant");
            AddGlobal(ESMSFields.SetMissionVarIndex1, "Output", EVaribleType.Int);
            AddField(ESMSFields.InputString_Tracked_Tech, "Tracked Tech");
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
                        Mission.VarIntsActive[SMission.SetMissionVarIndex1] = Singleton.playerTank.blockman.blockCount;
                    else
                        Mission.VarIntsActive[SMission.SetMissionVarIndex1] = 0;
                }
                if (SMUtil.DoesTrackedTechExist(ref SMission, SMission.InputString))
                {
                    TrackedTech tech = SMUtil.GetTrackedTechBase(ref SMission, SMission.InputString);
                    if (tech.destroyed)
                    {
                        Mission.VarIntsActive[SMission.SetMissionVarIndex1] = 0;
                    }
                    else if (!tech.TechAuto)
                    {   // unloaded
                        Mission.VarIntsActive[SMission.SetMissionVarIndex1] = 262145; // max possible Tech Volume + 1
                    }
                    else
                        Mission.VarIntsActive[SMission.SetMissionVarIndex1] = tech.TechAuto.blockman.blockCount;
                }
                else
                    SMUtil.Error(true, SMission.LogName, 
                        KickStart.ModID + ": Tech not referenced or missing in " + Mission.Name + 
                        " | Step type " + SMission.StepType.ToString() + " - Check your TrackedTechs, Tech names, " +
                        "and missions for consistancy errors");
            }
            catch (IndexOutOfRangeException e)
            {
                SMUtil.Assert(true, SMission.LogName, KickStart.ModID + ": Error in output [SetMissionVarIndex1] in mission " + Mission.Name +
                    " | Step type " + SMission.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) " +
                    "\n and make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry " +
                    "on the list, 1 counts as the second entry, and so on.", e);
            }
            catch (NullReferenceException e)
            {
                SMUtil.Assert(true, SMission.LogName, KickStart.ModID + ": Error in output [SetMissionVarIndex1] in mission " + Mission.Name +
                    " | Step type " + SMission.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) " +
                    "\n and make sure your referencing an entry you have declared in VarInts or varTrueFalse, depending" +
                    " on the step's set VaribleType.", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException(e);
            }
        }
        public override void OnDeInit()
        {
        }
    }
}
