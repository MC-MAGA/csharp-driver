//
//  Copyright (C) DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System.Collections.Generic;

namespace Dse.Test.Unit.Mapping.Pocos
{
    public class CollectionTypesEntity
    {
        public long Id { get; set; }
        public List<int> Scores { get; set; }
        public string[] Tags { get; set; }
        public Dictionary<string, string> Favs { get; set; }
    }
}
