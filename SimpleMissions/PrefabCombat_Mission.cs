using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sub_Missions.Steps;
using UnityEngine;

namespace Sub_Missions.SimpleMissions
{
    internal static class PrefabCombat_Mission
    {
        public static SubMission MakeMission()
        {
            SubMission mission2 = new SubMission();
            mission2.Name = "Combat Mission";
            mission2.AltNames = new List<string>
            {
                "Shoot The Target",
                "Weapons Testing",
                "Show Me What You Got",
            };
            mission2.Description = "A nice and simple combat target mission template";
            mission2.AltDescs = new List<string>
            {
                "Let's see how well you can aim pal! \n Shoot the bullseye!",
                "We've set up a test dummy on the field, take it down!",
                "Show me those guns aren't just for show!",
            };
            mission2.GradeRequired = 0;
            mission2.MinProgressX = 1;
            mission2.Faction = "GSO";
            mission2.ClearTechsOnClear = true;
            mission2.ClearModularMonumentsOnClear = false;
            mission2.CheckList = new List<MissionChecklist>
            {
                new MissionChecklist
                {
                    ListArticle = "Destroy TestTarget",
                    ValueType = VarType.Unset,
                    GlobalIndex = 0,
                }
            };
            mission2.VarTrueFalse = new List<bool>
            {
                true,
                true,
            };
            mission2.EventList = new List<SubMissionStep>
            {
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 0,
                    VaribleType = EVaribleType.True, // WARNING - Does only bools!
                    SetMissionVarIndex1 = 0,    // Gets true, shows message
                    SetMissionVarIndex2 = 1,    // sends true on close
                    InputString = "Some Random Feller From GSO",
                    InputStringAux = "Hey it's working now! \n\n    Prospector, could you shoot that target over there?",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.SetupMM,
                    ProgressID = 0,
                    InitPosition = Vector3.up * 2f,//Vector3.forward * 6,
                    Forwards = Vector3.one * 2f,     // Changes the scale
                    InputString = "ModularBrickCube_(636)",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.SetupMM,
                    ProgressID = 0,
                    InitPosition = -Vector3.forward * 9,//Vector3.forward * 6,
                    Forwards = Vector3.one * 1.9f,     // Changes the scale
                    EulerAngles = Quaternion.LookRotation(new Vector3(0, 0.5f, 1).normalized).eulerAngles, // Changes the rotation
                    InputString = "ModularBrickCube_(636)",
                },
                /*
                new SubMissionStep
                {
                    StepType = SMStepType.ActShifter,
                    ProgressID = 0,
                    VaribleType = EVaribleType.True,
                    SetMissionVarIndex1 = 0,            // activates on true
                    RevProgressIDOffset = false,
                },*/
                new SubMissionStep
                {
                    StepType = SMStepType.SetupTech,
                    ProgressID = 0,
                    InputNum = 8,
                    VaribleCheckNum = 21, // Spread for random mass-spawning
                    VaribleType = EVaribleType.None,
                    InputString = "TestTarget.RAWTECH",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.SetupWaypoint,
                    ProgressID = SubMission.alwaysRunValue,// update For all time. Always.
                    InitPosition = new Vector3(2,0,6),
                    VaribleType = EVaribleType.None,
                    InputString = "TestTarget.RAWTECH"
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActAnchor,
                    ProgressID = 0,
                    InputString = "TestTarget.RAWTECH",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.CheckDestroy,
                    ProgressID = 0,
                    SuccessProgressID = 30,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "TestTarget.RAWTECH",
                    InputStringAux = "Tracked Tech",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActWin,
                    ProgressID = 30,
                    VaribleType = EVaribleType.None,
                },
            };
            mission2.Rewards = new SubMissionReward
            {
                MoneyGain = 950,
                EXPGain = 100,
                BlocksToSpawn = new List<string> { BlockTypes.GSOCockpit_111.ToString() }
            };
            return mission2;
        }
    }
}
