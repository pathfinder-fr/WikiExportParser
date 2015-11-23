// -----------------------------------------------------------------------
// <copyright file="ParseException.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace WikiExportParser
{
    [Serializable]
    public class ParseException : ApplicationException
    {
        public ParseException()
        {
        }

        public ParseException(string message)
            : base(message)
        {
        }

        public ParseException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}