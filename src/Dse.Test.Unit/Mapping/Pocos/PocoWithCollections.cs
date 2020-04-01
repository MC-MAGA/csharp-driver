//
//  Copyright (C) DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System.Collections.Generic;

namespace Dse.Test.Unit.Mapping.Pocos
{
    public class PocoWithCollections<T>
    {
        public int Id { get; set; }

        public IEnumerable<T> IEnumerable { get; set; }

        public SortedSet<T> SortedSet { get; set; }

        public T[] Array { get; set; }

        public List<T> List { get; set; }

        public SortedDictionary<T, string> SortedDictionaryTKeyString { get; set; }
        public HashSet<T> HashSet { get; set; }

        public Dictionary<T, string> DictionaryTKeyString { get; set; }
    }
}
