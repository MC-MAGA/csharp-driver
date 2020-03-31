//
//  Copyright (C) DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Linq;
using Dse.Requests;
using Dse.Serialization;

namespace Dse
{
    /// <summary>
    /// <para>Represents a prepared statement with the parameter values set, ready for execution.</para>
    /// A <see cref="BoundStatement"/> can be created from a <see cref="PreparedStatement"/> instance using the
    /// <c>Bind()</c> method and can be executed using a <see cref="ISession"/> instance.
    /// <seealso cref="PreparedStatement"/>
    /// </summary>
    public class BoundStatement : Statement
    {
        private readonly PreparedStatement _preparedStatement;
        private RoutingKey _routingKey;
        private readonly ISerializer _serializer;
        private readonly string _keyspace;

        /// <summary>
        ///  Gets the prepared statement on which this BoundStatement is based.
        /// </summary>
        public PreparedStatement PreparedStatement
        {
            get { return _preparedStatement; }
        }


        /// <summary>
        ///  Gets the routing key for this bound query. <p> This method will return a
        ///  non-<c>null</c> value if: <ul> <li>either all the TableColumns composing the
        ///  partition key are bound variables of this <c>BoundStatement</c>. The
        ///  routing key will then be built using the values provided for these partition
        ///  key TableColumns.</li> <li>or the routing key has been set through
        ///  <c>PreparedStatement.SetRoutingKey</c> for the
        ///  <see cref="PreparedStatement"/> this statement has been built from.</li> </ul>
        ///  Otherwise, <c>null</c> is returned.</p> <p> Note that if the routing key
        ///  has been set through <link>PreparedStatement.SetRoutingKey</link>, that value
        ///  takes precedence even if the partition key is part of the bound variables.</p>
        /// </summary>
        public override RoutingKey RoutingKey
        {
            get { return _routingKey; }
        }

        /// <summary>
        /// Returns the keyspace this query operates on, based on the <see cref="PreparedStatement"/> metadata.
        /// <para>
        /// The keyspace returned is used as a hint for token-aware routing.
        /// </para>
        /// </summary>
        public override string Keyspace
        {
            get { return _keyspace; }
        }

        /// <summary>
        /// Initializes a new instance of the Cassandra.BoundStatement class
        /// </summary>
        public BoundStatement()
        {
            //Default constructor for client test and mocking frameworks
        }

        /// <summary>
        ///  Creates a new <c>BoundStatement</c> from the provided prepared
        ///  statement.
        /// </summary>
        /// <param name="statement"> the prepared statement from which to create a <c>BoundStatement</c>.</param>
        public BoundStatement(PreparedStatement statement)
        {
            _preparedStatement = statement;
            _routingKey = statement.RoutingKey;
            _keyspace = statement.Keyspace ?? statement.Metadata?.Keyspace;

            SetConsistencyLevel(statement.ConsistencyLevel);
            if (statement.IsIdempotent != null)
            {
                SetIdempotence(statement.IsIdempotent.Value);
            }
        }

        internal BoundStatement(PreparedStatement statement, ISerializer serializer) : this(statement)
        {
            _serializer = serializer;
        }
        
        /// <summary>
        ///  Set the routing key for this query. This method allows to manually
        ///  provide a routing key for this BoundStatement. It is thus optional since the routing
        ///  key is only an hint for token aware load balancing policy but is never
        ///  mandatory.
        /// </summary>
        /// <param name="routingKeyComponents"> the raw (binary) values to compose the routing key.</param>
        public BoundStatement SetRoutingKey(params RoutingKey[] routingKeyComponents)
        {
            _routingKey = RoutingKey.Compose(routingKeyComponents);
            return this;
        }

        internal override void SetValues(object[] values)
        {
            values = ValidateValues(values);
            base.SetValues(values);
        }

        /// <summary>
        /// Validate values using prepared statement metadata,
        /// returning a new instance of values to be used as parameters.
        /// </summary>
        private object[] ValidateValues(object[] values)
        {
            if (_serializer == null)
            {
                throw new DriverInternalError("Serializer can not be null");
            }
            if (values == null)
            {
                return null;
            }
            if (PreparedStatement.Metadata == null || PreparedStatement.Metadata.Columns == null || PreparedStatement.Metadata.Columns.Length == 0)
            {
                return values;
            }
            var paramsMetadata = PreparedStatement.Metadata.Columns;
            if (values.Length > paramsMetadata.Length)
            {
                throw new ArgumentException(
                    string.Format("Provided {0} parameters to bind, expected {1}", values.Length, paramsMetadata.Length));
            }
            for (var i = 0; i < values.Length; i++)
            {
                var p = paramsMetadata[i];
                var value = values[i];
                if (!_serializer.IsAssignableFrom(p, value))
                {
                    throw new InvalidTypeException(
                        string.Format("It is not possible to encode a value of type {0} to a CQL type {1}", value.GetType(), p.TypeCode));
                }
            }
            if (values.Length < paramsMetadata.Length && _serializer.ProtocolVersion.SupportsUnset())
            {
                //Set the result of the unspecified parameters to Unset
                var completeValues = new object[paramsMetadata.Length];
                values.CopyTo(completeValues, 0);
                for (var i = values.Length; i < paramsMetadata.Length; i++)
                {
                    completeValues[i] = Unset.Value;
                }
                values = completeValues;
            }
            return values;
        }

        internal override IQueryRequest CreateBatchRequest(ProtocolVersion protocolVersion)
        {
            // Use the default query options as the individual options of the query will be ignored
            var options = QueryProtocolOptions.CreateForBatchItem(this);
            return new ExecuteRequest(protocolVersion, PreparedStatement.Id, PreparedStatement.Metadata,
                PreparedStatement.ResultMetadataId, IsTracing, options);
        }

        internal void CalculateRoutingKey(bool useNamedParameters, int[] routingIndexes, string[] routingNames, object[] valuesByPosition, object[] rawValues)
        {
            if (_routingKey != null)
            {
                //The routing key was specified by the user
                return;
            }
            if (routingIndexes != null)
            {
                var keys = new RoutingKey[routingIndexes.Length];
                for (var i = 0; i < routingIndexes.Length; i++)
                {
                    var index = routingIndexes[i];
                    var key = _serializer.Serialize(valuesByPosition[index]);
                    if (key == null)
                    {
                        //The partition key can not be null
                        //Get out and let any node reply a Response Error
                        return;
                    }
                    keys[i] = new RoutingKey(key);
                }
                SetRoutingKey(keys);
                return;
            }
            if (routingNames != null && useNamedParameters)
            {
                var keys = new RoutingKey[routingNames.Length];
                var routingValues = Utils.GetValues(routingNames, rawValues[0]).ToArray();
                if (routingValues.Length != keys.Length)
                {
                    //The routing names are not valid
                    return;
                }
                for (var i = 0; i < routingValues.Length; i++)
                {
                    var key = _serializer.Serialize(routingValues[i]);
                    if (key == null)
                    {
                        //The partition key can not be null
                        return;
                    }
                    keys[i] = new RoutingKey(key);
                }
                SetRoutingKey(keys);
            }
        }
    }
}
