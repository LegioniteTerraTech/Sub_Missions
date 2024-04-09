using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Sub_Missions
{
    internal static class Debug_SMissions
    {
        internal static bool LogAll = false;
        internal static bool ShouldLog = true;
        private static bool ShouldLogNet = true;
        private static bool LogDev = false;

        internal static void Info(string message)
        {
            if (!ShouldLog || !LogAll)
                return;
            UnityEngine.Debug.Log(message);
        }
        internal static void Log(float message)
        {
            if (!ShouldLog)
                return;
            UnityEngine.Debug.Log(message.ToString());
        }
        internal static void Log(string message)
        {
            if (!ShouldLog)
                return;
            UnityEngine.Debug.Log(message);
        }
        internal static void Log(Exception e)
        {
            if (!ShouldLog)
                return;
            UnityEngine.Debug.Log(e);
        }

        internal static void LogNet(string message)
        {
            if (!ShouldLogNet)
                return;
            UnityEngine.Debug.Log(message);
        }

        internal static void Assert(string message)
        {
            if (!ShouldLog)
                return;
            UnityEngine.Debug.Log(message + "\n" + StackTraceUtility.ExtractStackTrace().ToString());
        }
        internal static void Assert(bool shouldAssert, string message)
        {
            if (!ShouldLog || !shouldAssert)
                return;
            UnityEngine.Debug.Log(message + "\n" + StackTraceUtility.ExtractStackTrace().ToString());
        }
        internal static void LogError(string message)
        {
            if (!ShouldLog)
                return;
            UnityEngine.Debug.Log(message + "\n" + StackTraceUtility.ExtractStackTrace().ToString());
        }
        internal static void LogDevOnly(string message)
        {
            if (!LogDev)
                return;
            UnityEngine.Debug.Log(message + "\n" + StackTraceUtility.ExtractStackTrace().ToString());
        }
        internal static void Exception(string message)
        {
            throw new Exception(KickStart.ModID + ": Exception - ", new Exception(message));
        }
        internal static void FatalError()
        {
            ManUI.inst.ShowErrorPopup(KickStart.ModID + KickStart.ModID + ": ENCOUNTERED CRITICAL ERROR");
            UnityEngine.Debug.Log(KickStart.ModID + ": ENCOUNTERED CRITICAL ERROR");
            UnityEngine.Debug.Log(KickStart.ModID + ": MAY NOT WORK PROPERLY AFTER THIS ERROR, PLEASE REPORT!");
        }
        internal static void FatalError(string e)
        {
            ManUI.inst.ShowErrorPopup(KickStart.ModID + ": ENCOUNTERED CRITICAL ERROR: " + e);
            UnityEngine.Debug.Log(KickStart.ModID + ": ENCOUNTERED CRITICAL ERROR: " + e);
            UnityEngine.Debug.Log(KickStart.ModID + ": MAY NOT WORK PROPERLY AFTER THIS ERROR, PLEASE REPORT!");
        }
    }
}