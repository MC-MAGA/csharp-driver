//
//  Copyright (C) DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Runtime.Serialization;

// ReSharper disable once CheckNamespace
namespace Dse
{
    /// <summary>
    /// Specifies a User defined function execution failure.
    /// </summary>
    public class FunctionFailureException : DriverException
    {
        /// <summary>
        /// Keyspace where the function is defined
        /// </summary>
        public string Keyspace { get; set; }

        /// <summary>
        /// Name of the function
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Name types of the arguments
        /// </summary>
        public string[] ArgumentTypes { get; set; }

        public FunctionFailureException(string message) : base(message)
        {
        }

        public FunctionFailureException(string message, Exception innerException) : base(message, innerException)
        {
        }

#if NET45
        protected FunctionFailureException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
#endif
    }
}
