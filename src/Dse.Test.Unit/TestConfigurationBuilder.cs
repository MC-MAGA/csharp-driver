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

using System.Collections.Generic;
using Dse.Connections;
using Dse.Connections.Control;
using Dse.ExecutionProfiles;
using Dse.MetadataHelpers;
using Dse.Metrics;
using Dse.Metrics.Providers.Null;
using Dse.Observers;
using Dse.ProtocolEvents;
using Dse.Requests;
using Dse.SessionManagement;
using Dse.Test.Unit.MetadataHelpers.TestHelpers;

namespace Dse.Test.Unit
{
    internal class TestConfigurationBuilder
    {
        public Dse.Policies Policies { get; set; } = Dse.Policies.DefaultPolicies;

        public ProtocolOptions ProtocolOptions { get; set; } = new ProtocolOptions();

        public PoolingOptions PoolingOptions { get; set; } = new PoolingOptions();

        public SocketOptions SocketOptions { get; set; } = new SocketOptions();

        public ClientOptions ClientOptions { get; set; } = new ClientOptions();

        public IAuthProvider AuthProvider { get; set; } = new NoneAuthProvider();

        public IAuthInfoProvider AuthInfoProvider { get; set; } = new SimpleAuthInfoProvider();

        public QueryOptions QueryOptions { get; set; } = new QueryOptions();

        public IAddressTranslator AddressTranslator { get; set; } = new DefaultAddressTranslator();

        public MetadataSyncOptions MetadataSyncOptions { get; set; } = new MetadataSyncOptions();

        public IStartupOptionsFactory StartupOptionsFactory { get; set; } = new StartupOptionsFactory();

        public IRequestOptionsMapper RequestOptionsMapper { get; set; } = new RequestOptionsMapper();

        public ISessionFactoryBuilder<IInternalCluster, IInternalSession> SessionFactoryBuilder { get; set; } = new SessionFactoryBuilder();

        public IReadOnlyDictionary<string, IExecutionProfile> ExecutionProfiles { get; set; } = new Dictionary<string, IExecutionProfile>();

        public IRequestHandlerFactory RequestHandlerFactory { get; set; } = new RequestHandlerFactory();

        public IHostConnectionPoolFactory HostConnectionPoolFactory { get; set; } = new HostConnectionPoolFactory();

        public IRequestExecutionFactory RequestExecutionFactory { get; set; } = new RequestExecutionFactory();

        public IConnectionFactory ConnectionFactory { get; set; } = new ConnectionFactory();

        public IControlConnectionFactory ControlConnectionFactory { get; set; } = new ControlConnectionFactory();

        public IPrepareHandlerFactory PrepareHandlerFactory { get; set; } = new PrepareHandlerFactory();

        public ITimerFactory TimerFactory { get; set; } = new TaskBasedTimerFactory();

        public IEndPointResolver EndPointResolver { get; set; } = new EndPointResolver(new DnsResolver(), new ProtocolOptions());

        public IObserverFactoryBuilder ObserverFactoryBuilder { get; set; } = new MetricsObserverFactoryBuilder();

        public DriverMetricsOptions MetricsOptions { get; set; } = new DriverMetricsOptions();

        public string SessionName { get; set; } 
        
        public IMetadataRequestHandler MetadataRequestHandler { get; set; } = new MetadataRequestHandler();

        public ITopologyRefresherFactory TopologyRefresherFactory { get; set; } = new TopologyRefresherFactory();

        public ISchemaParserFactory SchemaParserFactory { get; set; } = new FakeSchemaParserFactory();
        
        public ISupportedOptionsInitializerFactory SupportedOptionsInitializerFactory { get; set; } = new SupportedOptionsInitializerFactory();

        public IServerEventsSubscriber ServerEventsSubscriber { get; set; } = new ServerEventsSubscriber();
        
        public IProtocolVersionNegotiator ProtocolVersionNegotiator { get; set; } = new ProtocolVersionNegotiator();

        public Configuration Build()
        {
            return new Configuration(
                Policies,
                ProtocolOptions,
                PoolingOptions,
                SocketOptions,
                ClientOptions,
                AuthProvider,
                AuthInfoProvider,
                QueryOptions,
                AddressTranslator,
                StartupOptionsFactory,
                SessionFactoryBuilder,
                ExecutionProfiles,
                RequestOptionsMapper,
                MetadataSyncOptions,
                EndPointResolver,
                new NullDriverMetricsProvider(),
                MetricsOptions,
                SessionName,
                RequestHandlerFactory,
                HostConnectionPoolFactory,
                RequestExecutionFactory,
                ConnectionFactory,
                ControlConnectionFactory,
                PrepareHandlerFactory,
                TimerFactory,
                ObserverFactoryBuilder,
                MetadataRequestHandler,
                TopologyRefresherFactory,
                SchemaParserFactory,
                SupportedOptionsInitializerFactory,
                ProtocolVersionNegotiator,
                ServerEventsSubscriber);
        }
    }
}