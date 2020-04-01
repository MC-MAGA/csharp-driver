//
//  Copyright (C) DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Collections.Generic;

namespace Dse.Test.Unit.Mapping.Pocos
{
    public class UserDifferentPropTypes
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        //Table age column is an int, this property should fail
        public Dictionary<string, string> Age { get; set; }
    }
}
