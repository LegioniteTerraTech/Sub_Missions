using Sub_Missions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sub_Missions.Steps
{
    public class StepActTimer : SMissionStep
    {
        public override string GetDocumentation()
        {
            return
                "{  // Constantly increments the specified VarInt while true on each SubMission update." +
                  "\n  \"StepType\": \"ActTimer\"," +
                  "\n  \"ProgressID\": 0,             // " + StepDesc +
                  "\n  \"InputNum\": \"0\",       // Increments VarIndex1 by the value here." +
                  "\n    Values at 0 will make this display a countdown timer on HUD.  " +
                  "\n    This timer will OVERRIDE the built-in timer!" +
                  "\n  // Conditions TO CHECK before executing" +
                  "\n  \"VaribleType\": \"None\",       // See the top of this file." +
                  "\n  \"VaribleCheckNum\": 0.0,      // What fixed value to compare VaribleType to." +
                  "\n  \"SetMissionVarIndex1\": -1,       // The index that determines if it should execute." +
                  "\n  // Output Actions" +
                  "\n  \"SetMissionVarIndex2\": -1,       // The VarInt index that is updated while it is true." +
                "\n},";
        }
        public override void InitGUI()
        {
            AddField(ESMSFields.InputNum, "Timer Increment [0 for UI]");
            AddField(ESMSFields.VaribleType, "Condition Mode");
            AddField(ESMSFields.VaribleCheckNum, "Conditional Constant");
            AddField(ESMSFields.SetMissionVarIndex1, "Active Condition");
            AddGlobal(ESMSFields.SetMissionVarIndex2, "Output Timer", EVaribleType.Int);
        }
        public override void OnInit()
        {
            ShownTimer = false;
        }

        public override void OnDeInit()
        {
            ManQuestLog.inst.StopMissionTimer(Mission.FakeEncounter.EncounterDef);
        }
        public override void FirstSetup()
        {
            try
            {
                SMission.SavedInt = (int)SMission.InputNum;
            }
            catch (IndexOutOfRangeException e)
            {
                SMUtil.Assert(true, SMission.LogName, KickStart.ModID + ": Error in output [SetMissionVarIndex1] or [SetMissionVarIndex1] or " +
                    "[SetMissionVarIndex1] in mission " + Mission.Name +
                    " | Step type " + SMission.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) " +
                    "\n and make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry " +
                    "on the list, 1 counts as the second entry, and so on.", e);
            }
            catch (NullReferenceException e)
            {
                SMUtil.Assert(true, SMission.LogName, KickStart.ModID + ": Error in output [SetMissionVarIndex1] or [SetMissionVarIndex1] or " +
                    "[SetMissionVarIndex1] in mission " + Mission.Name +
                    " | Step type " + SMission.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) " +
                    "\n and make sure your referencing an entry you have declared in VarInts or varTrueFalse, depending" +
                    " on the step's set VaribleType.", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException(e);
            }
        }
        private bool ShownTimer = false;
        public override void Trigger()
        {
            try
            {
                switch (SMission.VaribleType)
                {
                    case EVaribleType.True: // tick up when bool True
                        if (Mission.VarTrueFalseActive[SMission.SetMissionVarIndex1])
                        {
                            if (SMission.InputNum == 0)
                            {
                                if (!ShownTimer)
                                {
                                    ShownTimer = true;
                                    ManQuestLog.inst.ShowMissionTimerUI(Mission.FakeEncounter.EncounterDef);
                                    ManQuestLog.inst.StartMissionTimer(Mission.FakeEncounter.EncounterDef, SMission.SavedInt);
                                }
                                Mission.VarIntsActive[SMission.SetMissionVarIndex2] = (int)ManQuestLog.inst.GetMissionTimerDisplayTime(Mission.FakeEncounter.EncounterDef);
                                SMission.SavedInt = Mission.VarIntsActive[SMission.SetMissionVarIndex2];
                            }
                            else
                            {
                                Mission.VarIntsActive[SMission.SetMissionVarIndex2] += (int)SMission.InputNum;
                            }
                        }
                        else
                            ManQuestLog.inst.StopMissionTimer(Mission.FakeEncounter.EncounterDef);
                        break;
                    case EVaribleType.False: // tick up when bool False
                        if (!Mission.VarTrueFalseActive[SMission.SetMissionVarIndex1])
                        {
                            if (SMission.InputNum == 0)
                            {
                                if (!ShownTimer)
                                    if (!ShownTimer)
                                    {
                                        ShownTimer = true;
                                        ManQuestLog.inst.ShowMissionTimerUI(Mission.FakeEncounter.EncounterDef);
                                        ManQuestLog.inst.StartMissionTimer(Mission.FakeEncounter.EncounterDef, SMission.SavedInt);
                                    }
                                Mission.VarIntsActive[SMission.SetMissionVarIndex2] = (int)ManQuestLog.inst.GetMissionTimerDisplayTime(Mission.FakeEncounter.EncounterDef);
                                SMission.SavedInt = Mission.VarIntsActive[SMission.SetMissionVarIndex2];
                            }
                            else
                                Mission.VarIntsActive[SMission.SetMissionVarIndex2] += (int)SMission.InputNum;
                        }
                        break;
                    default:    // tick always
                        if (SMission.InputNum == 0)
                        {
                            if (!ShownTimer)
                            {
                                ShownTimer = true;
                                ManQuestLog.inst.ShowMissionTimerUI(Mission.FakeEncounter.EncounterDef);
                                ManQuestLog.inst.StartMissionTimer(Mission.FakeEncounter.EncounterDef, SMission.SavedInt);
                            }
                            Mission.VarIntsActive[SMission.SetMissionVarIndex2] = (int)ManQuestLog.inst.GetMissionTimerDisplayTime(Mission.FakeEncounter.EncounterDef);
                            SMission.SavedInt = Mission.VarIntsActive[SMission.SetMissionVarIndex2];
                        }
                        else
                            Mission.VarIntsActive[SMission.SetMissionVarIndex2] += (int)SMission.InputNum;
                        break;
                }
            }
            catch (IndexOutOfRangeException e)
            {
                SMUtil.Assert(true, SMission.LogName, KickStart.ModID + ": Error in output [SetMissionVarIndex1] or [SetMissionVarIndex1] or " +
                    "[SetMissionVarIndex1] in mission " + Mission.Name +
                    " | Step type " + SMission.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) " +
                    "\n and make sure your referencing is Zero-Indexed, meaning that 0 counts as the first entry " +
                    "on the list, 1 counts as the second entry, and so on.", e);
            }
            catch (NullReferenceException e)
            {
                SMUtil.Assert(true, SMission.LogName, KickStart.ModID + ": Error in output [SetMissionVarIndex1] or [SetMissionVarIndex1] or " +
                    "[SetMissionVarIndex1] in mission " + Mission.Name +
                    " | Step type " + SMission.StepType.ToString() + " - Check your assigned Vars (VarInts or varTrueFalse) " +
                    "\n and make sure your referencing an entry you have declared in VarInts or varTrueFalse, depending" +
                    " on the step's set VaribleType.", e);
            }
            catch (Exception e)
            {
                throw new MandatoryException(e);
            }
        }
    }
}
