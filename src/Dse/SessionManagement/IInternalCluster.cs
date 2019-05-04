﻿//
//      Copyright (C) 2012-2014 DataStax Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//

using System.Collections.Concurrent;
using System.Threading.Tasks;
using Dse.Connections;
using Dse.ExecutionProfiles;
using Dse.Requests;
using Dse.Serialization;

namespace Dse.SessionManagement
{
    /// <inheritdoc />
    internal interface IInternalCluster : ICluster
    {
        bool AnyOpenConnections(Host host);

        /// <summary>
        /// Gets the control connection used by the cluster
        /// </summary>
        IControlConnection GetControlConnection();

        /// <summary>
        /// Gets the the prepared statements cache
        /// </summary>
        ConcurrentDictionary<byte[], PreparedStatement> PreparedQueries { get; }

        /// <summary>
        /// Executes the prepare request on the first host selected by the load balancing policy.
        /// When <see cref="QueryOptions.IsPrepareOnAllHosts"/> is enabled, it prepares on the rest of the hosts in
        /// parallel.
        /// In case the statement was already in the prepared statements cache, logs an warning but prepares it anyway.
        /// </summary>
        Task<PreparedStatement> Prepare(IInternalSession session, Serializer serializer, InternalPrepareRequest request);
    }
}