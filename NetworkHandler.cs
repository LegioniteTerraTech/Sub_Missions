using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Sub_Missions
{
    /*
     * Plan: Host is the only one testing for conditions and spawning normal game content
     *   Only the state is sent to clients during the mission
     * 
     * 
     * 
     */
    public class NetworkHandler
    {
        static UnityEngine.Networking.NetworkInstanceId Host;
        static bool HostExists = false;

        const TTMsgType SubMissionComplete = (TTMsgType)4590;
        const TTMsgType SubMissionProgress = (TTMsgType)4591;
        const TTMsgType SubMissionAccepted = (TTMsgType)4592;
        const TTMsgType SubMissionCanceled = (TTMsgType)4593;


        public class SubMissionFinished : UnityEngine.Networking.MessageBase
        {
            public SubMissionFinished() { }
            public SubMissionFinished(int MissionHash, bool Success)
            {
                this.MissionHash = MissionHash;
                this.Success = Success;
            }

            public int MissionHash;
            public bool Success;
        }

        public static void TryBroadcastSubMissionFinished(int MissionHash, bool Success)
        {
            if (HostExists) try
                {
                    Singleton.Manager<ManNetwork>.inst.SendToAllClients(SubMissionComplete, new SubMissionFinished(MissionHash, Success), Host);
                    Debug_SMissions.Log("Sent new AdvancedAI update to all");
                }
                catch { Debug_SMissions.Log("TACtical_AI: Failed to send new AdvancedAI update, shouldn't be too bad in the long run"); }
        }
        public static void OnClientSubMissionFinished(UnityEngine.Networking.NetworkMessage netMsg)
        {
            var reader = new SubMissionFinished();
            netMsg.ReadMessage(reader);
            try
            {
                NetTech find = ManNetTechs.inst.FindTech(reader.netTechID);
                find.tech.GetComponent<AIECore.TankAIHelper>().TrySetAITypeRemote(netMsg.GetSender(), reader.AIType);
                Debug_SMissions.Log("TACtical_AI: Received new AdvancedAI update, changing to " + find.tech.GetComponent<AIECore.TankAIHelper>().DediAI.ToString());
            }
            catch
            {
                Debug_SMissions.Log("TACtical_AI: Receive failiure! Could not decode intake!?");
            }
        }

        public static class Patches
        {
            [HarmonyPatch(typeof(NetPlayer), "OnStartClient")]
            static class OnStartClient
            {
                static void Postfix(NetPlayer __instance)
                {
                    Singleton.Manager<ManNetwork>.inst.SubscribeToClientMessage(__instance.netId, AIADVTypeChange, new ManNetwork.MessageHandler(OnClientChangeNewAIState));
                    Debug_SMissions.Log("Subscribed " + __instance.netId.ToString() + " to AdvancedAI updates from host. Sending current techs");
                }
            }

            [HarmonyPatch(typeof(NetPlayer), "OnStartServer")]
            static class OnStartServer
            {
                static void Postfix(NetPlayer __instance)
                {
                    if (!HostExists)
                    {
                        Debug_SMissions.Log("Host started, hooked AdvancedAI update broadcasting to " + __instance.netId.ToString());
                        Host = __instance.netId;
                        HostExists = true;
                    }
                }
            }
        }
    }
