//
//      Copyright (C) DataStax Inc.
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

using System;
using System.Collections.Concurrent;
using System.Net;
using Dse.Connections;
using Dse.Observers;
using Dse.Observers.Abstractions;
using Dse.Serialization;
using Moq;

namespace Dse.Test.Unit.Connections.TestHelpers
{
    internal class FakeConnectionFactory : IConnectionFactory
    {
        private readonly Func<IConnectionEndPoint, IConnection> _func;
        public event Action<IConnection> OnCreate;

        public ConcurrentDictionary<IPEndPoint, ConcurrentQueue<IConnection>> CreatedConnections { get; } = new ConcurrentDictionary<IPEndPoint, ConcurrentQueue<IConnection>>();
        
        public FakeConnectionFactory() : this((Func<IConnectionEndPoint, IConnection>)null)
        {
        }
        
        public FakeConnectionFactory(Func<IConnection> func)
        {
            _func = endpoint => func();
        }

        public FakeConnectionFactory(Func<IConnectionEndPoint, IConnection> func)
        {
            if (func == null)
            {
                func = endpoint =>
                {
                    var connection = Mock.Of<IConnection>();
                    Mock.Get(connection).SetupGet(c => c.EndPoint).Returns(endpoint);
                    return connection;
                };
            }

            _func = func;
        }
        
        public FakeConnectionFactory(Func<IPEndPoint, IConnection> func)
        {
            _func = endpoint => func(endpoint.SocketIpEndPoint);
        }

        public IConnection Create(ISerializer serializer, IConnectionEndPoint endpoint, Configuration configuration, IConnectionObserver connectionObserver)
        {
            var connection = _func(endpoint);
            var queue = CreatedConnections.GetOrAdd(endpoint.GetHostIpEndPointWithFallback(), _ => new ConcurrentQueue<IConnection>());
            queue.Enqueue(connection);
            OnCreate?.Invoke(connection);
            return connection;
        }

        public IConnection CreateUnobserved(ISerializer serializer, IConnectionEndPoint endPoint, Configuration configuration)
        {
            return Create(serializer, endPoint, configuration, NullConnectionObserver.Instance);
        }
    }
}