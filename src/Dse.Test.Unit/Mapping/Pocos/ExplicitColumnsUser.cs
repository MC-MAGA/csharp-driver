//
//  Copyright (C) DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using Dse.Mapping.Attributes;

namespace Dse.Test.Unit.Mapping.Pocos
{
    /// <summary>
    /// A user decorated with attributes indicating how it should be mapped, specifically the ExplicitColumnsAttribute.
    /// </summary>
    [Table("users", ExplicitColumns = true)]
    public class ExplicitColumnsUser
    {
        [Column]
        public Guid UserId { get; set; }

        // Should not be mapped since no Column attribute and ExplicitColumns was used
        public string Name { get; set; }

        [Column("age")]
        public int UserAge { get; set; }
    }
}