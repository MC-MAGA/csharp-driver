//
//  Copyright (C) DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;

namespace Dse.Test.Unit.Mapping.Pocos
{
    /// <summary>
    /// Test utility: Represents an application entity with most of single types as properties
    /// </summary>
    public class AllTypesEntity
    {
        public bool BooleanValue { get; set; }
        public DateTime DateTimeValue { get; set; }
        public decimal DecimalValue { get; set; }
        public double DoubleValue { get; set; }
        public Int64 Int64Value { get; set; }
        public int IntValue { get; set; }
        public string StringValue { get; set; }
        public Guid UuidValue { get; set; }
    }
}
