using System;
using System.Collections.Concurrent;

namespace StackExchange.Redis.Helper
{
    public class RedisConnectionHelper
    {
        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> ConnectionCache = new ConcurrentDictionary<string, ConnectionMultiplexer>();
        private static readonly object Locker = new object();
        private static ConnectionMultiplexer _instance;

        public static ConnectionMultiplexer Instance(string connectionString)
        {
            if(_instance==null)
            {
                lock(Locker)
                { 
                    if(_instance==null || !_instance.IsConnected)
                    {
                        _instance = GetManager(connectionString);
                    }
                }
            }

            return _instance;
        }

        public static ConnectionMultiplexer CreateConnect(string connectionString)
        {
            if (!ConnectionCache.ContainsKey(connectionString))
            {
                ConnectionCache[connectionString] = GetManager(connectionString);
            }
            return ConnectionCache[connectionString];
        }

        public static ConnectionMultiplexer CreateConnect(ConfigurationOptions option)
        {
            if (!ConnectionCache.ContainsKey(option.ToString()))
            {
                ConnectionCache[option.ToString()] = GetManager(option);
            }
            return ConnectionCache[option.ToString()];
        }

        private static ConnectionMultiplexer GetManager(string connectionString)
        {
            ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString, null);
            connectionMultiplexer.ConnectionFailed += MuxerConnectionFailed;
            connectionMultiplexer.ConnectionRestored += MuxerConnectionRestored;
            connectionMultiplexer.ErrorMessage += MuxerErrorMessage;
            connectionMultiplexer.ConfigurationChanged += MuxerConfigurationChanged;
            connectionMultiplexer.HashSlotMoved += MuxerHashSlotMoved;
            connectionMultiplexer.InternalError += MuxerInternalError;
            return connectionMultiplexer;
        }

        private static ConnectionMultiplexer GetManager(ConfigurationOptions sentinelConfig)
        {
            ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(sentinelConfig, null);
            connectionMultiplexer.ConnectionFailed += MuxerConnectionFailed;
            connectionMultiplexer.ConnectionRestored += MuxerConnectionRestored;
            connectionMultiplexer.ErrorMessage += MuxerErrorMessage;
            connectionMultiplexer.ConfigurationChanged += MuxerConfigurationChanged;
            connectionMultiplexer.HashSlotMoved += MuxerHashSlotMoved;
            connectionMultiplexer.InternalError += MuxerInternalError;
            return connectionMultiplexer;
        }

        private static void MuxerConfigurationChanged(object sender, EndPointEventArgs e)
        {
            Console.WriteLine("Configuration changed: " + e.EndPoint);
        }

        private static void MuxerErrorMessage(object sender, RedisErrorEventArgs e)
        {
            Console.WriteLine("ErrorMessage: " + e.Message);
        }

        private static void MuxerConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            Console.WriteLine("ConnectionRestored: " + e.EndPoint);
        }

        private static void MuxerConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            Console.WriteLine(string.Concat(new object[]
            {
                "重新连接：Endpoint failed: ",
                e.EndPoint,
                ", ",
                e.FailureType,
                (e.Exception == null) ? "" : (", " + e.Exception.Message)
            }));
        }

        private static void MuxerHashSlotMoved(object sender, HashSlotMovedEventArgs e)
        {
            Console.WriteLine(string.Concat(new object[]
            {
                "HashSlotMoved:NewEndPoint",
                e.NewEndPoint,
                ", OldEndPoint",
                e.OldEndPoint
            }));
        }

        private static void MuxerInternalError(object sender, InternalErrorEventArgs e)
        {
            Console.WriteLine("InternalError:Message" + e.Exception.Message);
        }

    }
}
