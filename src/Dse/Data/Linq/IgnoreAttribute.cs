//
//  Copyright (C) DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;

namespace Dse.Data.Linq
{
    /// <summary>
    /// Specifies that the field or property should be ignored by Linq
    /// </summary>
    [Obsolete("Linq attributes are deprecated, use mapping attributes defined in Dse.Mapping.Attributes instead.")]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class IgnoreAttribute : Attribute
    {
    }
}
