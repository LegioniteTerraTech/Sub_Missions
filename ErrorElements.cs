using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TerraTechETCUtil;
using Sub_Missions.ManWindows;

namespace Sub_Missions
{
    internal abstract class ErrorElement
    {
        public abstract void GUICall();
    }

    internal class ErrorElementString : ErrorElement
    {
        private readonly string info;
        public ErrorElementString(string info)
        {
            this.info = info;
        }
        public override void GUICall()
        {
            GUILayout.Label(info, WindowManager.styleScrollFont);
        }
        public override string ToString()
        {
            return info;
        }
    }

    internal class ErrorElementInfo : ErrorElement
    {
        private readonly string title;
        private readonly string info;
        private bool shown = false;
        public ErrorElementInfo(string title, string info)
        {
            this.title = title;
            this.info = info;
        }
        public override void GUICall()
        {
            if (shown)
            {
                if (GUILayout.Button(title, AltUI.ButtonBlueActive))
                    shown = false;
                GUILayout.Label(info, WindowManager.styleScrollFont);
            }
            else
            {
                if (GUILayout.Button(title, AltUI.ButtonBlue))
                    shown = true;
            }
        }
        public override string ToString()
        {
            return title + " ~ " + info;
        }
    }

    internal class ErrorElementError : ErrorElement
    {
        private readonly string title;
        private readonly string info;
        private bool shown = false;
        public ErrorElementError(string title, string info)
        {
            this.title = title;
            this.info = info;
        }
        public override void GUICall()
        {
            if (shown)
            {
                if (GUILayout.Button(title, AltUI.ButtonRedActive, GUILayout.ExpandWidth(true)))
                    shown = false;
                GUILayout.Label(info, WindowManager.styleScrollFont);
            }
            else
            {
                if (GUILayout.Button(title, AltUI.ButtonRed, GUILayout.ExpandWidth(true)))
                    shown = true;
            }
        }
        public override string ToString()
        {
            return title + " ~ " + info;
        }
    }

    internal class ErrorElementAssert : ErrorElement
    {
        private static StringBuilder SB = new StringBuilder();

        private readonly string title;
        private readonly string info;
        private readonly string StackTrace;
        private bool shown = false;
        private bool exShown = false;
        public ErrorElementAssert(string title, string Description, Exception ex)
        {
            this.title = title;
            StackTrace = StackTraceUtility.ExtractStringFromException(ex);
            SB.AppendLine(Description);
            while (ex != null)
            {
                SB.Append("- ");
                SB.AppendLine(ex.Message);
                ex = ex.InnerException;
            }
            info = SB.ToString();
            SB.Clear();
        }
        public override void GUICall()
        {
            if (shown)
            {
                if (GUILayout.Button(title, AltUI.ButtonOrangeLargeActive, GUILayout.ExpandWidth(true), GUILayout.Height(46)))
                    shown = false;
                GUILayout.Label(info, WindowManager.styleScrollFont);
                if (exShown)
                {
                    if (GUILayout.Button("Stack Trace", AltUI.ButtonGreen))
                        exShown = false;
                    GUILayout.Label(StackTrace, WindowManager.styleScrollFont);
                }
                else
                {
                    if (GUILayout.Button("Stack Trace", AltUI.ButtonRed))
                        exShown = true;
                }
            }
            else
            {
                if (GUILayout.Button(title, AltUI.ButtonOrangeLarge, GUILayout.ExpandWidth(true), GUILayout.Height(46)))
                    shown = true;
            }
        }
        public override string ToString()
        {
            return title + " ~ " + info + "\nStack Trace - " + StackTrace;
        }
    }

}
