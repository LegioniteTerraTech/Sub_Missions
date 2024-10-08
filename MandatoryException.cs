﻿using System;
using UnityEngine;

namespace Sub_Missions
{
    public class MandatoryException : Exception
    {
        protected const string ReportDest = "in Discord either in the TerraTech Community, channel #modding-help" +
            " or notify user Legionite (@LegioniteTT)";

        protected readonly string name;
        /// <summary>
        /// It MUST nest another exception!
        /// </summary>
        public MandatoryException() : base()
        {
            throw new MandatoryException("MandatoryException() cannot be used standalone, " +
                "Its main purpose is to bypass the user UI bug reporter.  It MUST nest another exception!",
                new NullReferenceException("MandatoryException - Exception argument NULL"));
        }
        /// <summary>
        /// It MUST nest another exception!
        /// </summary>
        public MandatoryException(string Message) : base(Message)
        {
            throw new MandatoryException("MandatoryException(string Message) cannot be used standalone, " +
                "Its main purpose is to bypass the user UI bug reporter.  It MUST nest another exception!", 
                new NullReferenceException("MandatoryException - Exception argument NULL"));
        }
        public MandatoryException(Exception InnerException) : 
            base("MANDATORY: This is the mod developer's issue, please report it " + ReportDest + "!", InnerException)
        {
        }
        public MandatoryException(string Message, Exception InnerException) : 
            base(Message + "\n This is the mod developer's issue, please report it " + ReportDest + "!", InnerException)
        {

        }
        public MandatoryException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            throw new MandatoryException("MandatoryException(System.Runtime.Serialization.SerializationInfo info, " +
                "System.Runtime.Serialization.StreamingContext context) cannot be used as it is not implemented!");
        }
    }

    public class WarningException : Exception
    {
        protected readonly string name;
        public WarningException() : base()
        {
            throw new WarningException("WarningException() cannot be used standalone, " +
                "Its main purpose is to bypass the user UI bug reporter.  It MUST nest another exception!",
                new NullReferenceException("WarningException - Exception argument NULL"));
        }
        public WarningException(string Message) : base(Message)
        {
            throw new WarningException("WarningException(string Message) cannot be used standalone, " +
                "Its main purpose is to bypass the user UI bug reporter.  It MUST nest another exception!",
                new NullReferenceException("WarningException - Exception argument NULL"));
        }
        public WarningException(string Message, Exception InnerException) : base(Message, InnerException)
        {

        }
        public WarningException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            throw new WarningException("WarningException(System.Runtime.Serialization.SerializationInfo info, " +
                "System.Runtime.Serialization.StreamingContext context) cannot be used as it is not implemented!");
        }
    }
}
