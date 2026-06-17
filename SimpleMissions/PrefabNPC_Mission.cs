using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sub_Missions.Steps;
using UnityEngine;

namespace Sub_Missions.SimpleMissions
{
    internal static class PrefabNPC_Mission
    {

        public static SubMission MakeMission()
        {
            SubMission mission1 = new SubMission();
            mission1.Name = "NPC Mission";
            mission1.Description = "A complex showcase mission with an NPC involved";
            mission1.GradeRequired = 1;
            mission1.Faction = "GSO";
            mission1.SetStartingPosition = Vector3.forward * 250;
            mission1.Type = SubMissionType.Critical;
            mission1.CannotCancel = true;
            mission1.SpawnPosition = SubMissionPosition.OffsetFromPlayerTechFacing;
            mission1.ClearTechsOnClear = false; // off for now to show the StepActRemove Step
            mission1.VarTrueFalseActive = new List<bool>
            {
                false,  // Range check
                false,  // Choice 1 "No"
                false,  // Choice 2 "Yes"
                false,  // When Player drives away while making the choice
            };
            mission1.VarIntsActive = new List<int>
            {
                0,  // timer value
            };
            mission1.CheckList = new List<MissionChecklist>
            {
                new MissionChecklist
                {
                    ListArticle = "Meet Garrett",
                    ValueType = VarType.Unset,
                    BoolToEnable = 3,
                },
                new MissionChecklist
                {
                    ListArticle = "Speak to Garrett",
                    ValueType = VarType.Unset,
                },
            };
            mission1.EventList = new List<SubMissionStep>
            {
                // Phase 1 - Getting the player to the character
                new SubMissionStep
                {
                    StepType = SMStepType.SetupTech,
                    ProgressID = 0,
                    InitPosition = new Vector3(2,0,6), // needs specific location
                    InputNum = -2, // team
                    VaribleType = EVaribleType.None,
                    InputString = "Garrett Gruntle.RAWTECH",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.SetupWaypoint,
                    ProgressID = SubMission.alwaysRunValue,// update For all time. Always.
                    InitPosition = new Vector3(2,0,6),
                    VaribleType = EVaribleType.None,
                    InputString = "Garrett Gruntle.RAWTECH", // give it a TrackedTech Tech name to assign it to that tech
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 0,
                    SuccessProgressID = 2,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Hello?   <b>Is this thing working!?</b> \n            <b>screEEEEeeeech!</b>\n Henlo there, and thank you for signing up into our Sub-Missions programme!",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 2,
                    VaribleType = EVaribleType.None,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "I've marked where I am on your minimap, come to me so that I can \n       <b>screEEEEeeeech!</b>\n -darn the reception-   finish the briefing!",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActAnchor,
                    ProgressID = 1,
                    InputString = "Garrett Gruntle",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.CheckPlayerDist,
                    ProgressID = 1, // note that this still works even when the CurrentProgressID is 0 or 2 due to the adjecency rule
                    InitPosition = new Vector3(2,0,6), // needs specific location
                    InputNum = 64, // distance before it gets true
                    VaribleType = EVaribleType.True,
                    SetMissionVarIndex1 = 0, //enable
                    InputString = "",
                },  // this basically relays to the below
                new SubMissionStep
                {
                    StepType = SMStepType.ActForward,
                    ProgressID = 1,
                    SuccessProgressID = 10,// Go to Phase 2
                    VaribleType = EVaribleType.True,
                    SetMissionVarIndex1 = 0,
                    InputString = "",
                },


                // Phase 2 - Conversation
                new SubMissionStep
                {
                    StepType = SMStepType.ActMessagePurge,
                    ProgressID = 10,
                    VaribleType = EVaribleType.None,
                    InputNum = 1,// only fire once
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActAnchor,
                    ProgressID = 10,
                    InputString = "Garrett Gruntle",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 10,
                    SuccessProgressID = 15,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Name's <b>Garrett Gruntle</b>.  \n[Otherwise known as GG]",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 15,
                    SuccessProgressID = 20,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Hey there prospector, \nso uhh we're testing the new <b>Sub-Missions</b> protocol. \n   Say, it's a fine day today?",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 20,
                    SuccessProgressID = 25,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "I'll be assigning a series of fairly easy missions for you to try.",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 25,
                    SuccessProgressID = 30,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Say, have you met Agent Pow?    \n      \n   Was she nice?",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActOptions,
                    ProgressID = 30,
                    VaribleType = EVaribleType.True,
                    InputString = "Met Agent Pow?", // title
                    InputStringAux = "Yes", // button label
                    SetMissionVarIndex1 = 0, // value to check before throwing window (disabled on VaribleType: None)
                    SetMissionVarIndex2 = 1, // "No" pressed - Go to Phase 3
                    SetMissionVarIndex3 = 2, // "Other Option [Yes]" pressed - Go to Phase 3.5
                },
                new SubMissionStep
                {
                    StepType = SMStepType.CheckPlayerDist,
                    ProgressID = 30,
                    InitPosition = new Vector3(2,0,6), // needs specific location
                    InputNum = -64, // distance before it gets true - Negative makes this trigger when player outside
                    VaribleType = EVaribleType.True,
                    SetMissionVarIndex1 = 3, //ooop
                    InputString = "",
                },  // When this is triggered - Go to Phase 3.9


                // Phase 3 - Option "No"
                new SubMissionStep
                {
                    StepType = SMStepType.ActForward,
                    ProgressID = 30,
                    SuccessProgressID = 35,
                    VaribleType = EVaribleType.True,
                    SetMissionVarIndex1 = 1, // value to check before moving forward
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 35,
                    SuccessProgressID = 50, // Go to Phase 4
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Figures...    \n   [I wonder what she thinks of Crafty Mike and his shenanigans...] \n      [Wasn't Mike supposedly \"devilishly crafty?\"]",
                },

                // Phase 3.5 - Option "Yes"
                new SubMissionStep
                {
                    StepType = SMStepType.ActForward,
                    ProgressID = 30,
                    SuccessProgressID = 40,
                    VaribleType = EVaribleType.True,
                    SetMissionVarIndex1 = 2, // value to check before moving forward
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 40,
                    SuccessProgressID = 50,// Go to Phase 4
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Oh!  Maybe we CAN be friends after all!    \n      [Skreee you HAVE MAIL]\n   ...",
                },

                // Phase 3.9 - Option "i don't care"
                new SubMissionStep
                {
                    StepType = SMStepType.ActForward,
                    ProgressID = 30,
                    SuccessProgressID = 80,
                    VaribleType = EVaribleType.True,
                    SetMissionVarIndex1 = 3, // value to check before moving forward
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActMessagePurge,
                    ProgressID = 80,
                    VaribleType = EVaribleType.None,
                    InputNum = 1,// only fire once
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 80,
                    SuccessProgressID = 50,// Go to Phase 4
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Uhhh...    [You coming back?]   Figures...  \n          \n  I'll just send the missions over then...",
                },


                // Phase 4 - Letting the NPC leave the scene
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 50,
                    SuccessProgressID = 55,
                    VaribleType = EVaribleType.DoSuccessID,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "[Skreee]... \n\n  Looks like my time has vanished.  \n      Time to go then.",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActSpeak,
                    ProgressID = 55,
                    VaribleType = EVaribleType.None,
                    InputString = "Garrett Gruntle",
                    InputStringAux = "Farewell comrade, may we mee-~  \n       <b>screEEEEeeeech!</b>",
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActBoost,
                    ProgressID = 55,
                    VaribleType = EVaribleType.None,
                    InputString = "Garrett Gruntle.RAWTECH"
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActTimer,
                    ProgressID = 55,
                    VaribleType = EVaribleType.None,// For All Time. Always.
                    SetMissionVarIndex1 = 0,   // check this int (disabled)
                    SetMissionVarIndex2 = 0,   // tick this int up
                    InputNum = 1,          // every second we increase by this value
                },


                // Phase 5 - Cleanup
                //   There's a toggle that allows the mission to clean up Techs automatically
                //     but in the case you want to keep some techs, this is the example.
                new SubMissionStep
                {
                    StepType = SMStepType.ActForward,
                    ProgressID = 55,
                    SuccessProgressID = 60, //
                    VaribleType = EVaribleType.IntGreaterThan,
                    SetMissionVarIndex1 = 0,    // Check this int - A timer in Phase 4 is pushing this along
                    VaribleCheckNum = 7,    // Above this?  Then we go to Phase 5
                },
                new SubMissionStep
                {
                    StepType = SMStepType.ActRemove,
                    ProgressID = 60,
                    VaribleType = EVaribleType.None,
                    InputString = "Garrett Gruntle.RAWTECH"
                },  // The above fires first due to the way the updating works - top to bottom
                new SubMissionStep
                {
                    StepType = SMStepType.ActWin,
                    ProgressID = 60,
                    VaribleType = EVaribleType.None,
                },
            };
            mission1.Rewards = new SubMissionReward
            {
                EXPGain = 250,
                AddProgressX = 2,
            };
            return mission1;
        }
    }
}
