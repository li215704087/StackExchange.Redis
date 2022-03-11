using Microsoft.Extensions.Options;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StackExchange.Redis.Helper.Lock
{
    public class RedisLock: IRedisLock
    {
        private IDistributedLockFactory _distributedLockFactory;

        private readonly TimeSpan _expiryTimeSpan;

        private readonly TimeSpan _retryTimeSpan;

        private readonly string _clientName;

        private static List<RedLockMultiplexer> Multiplexers = new List<RedLockMultiplexer>();

        private readonly System.Threading.SemaphoreSlim semaphoreSlim = new System.Threading.SemaphoreSlim(1);

        private static ConfigurationOptions _configurationOptions;
        private readonly RedisConfigOption _redisConfigOption;

        #region 构造函数
        public RedisLock(IOptions<RedisConfigOption> redisConfigOption)
        {
            _redisConfigOption = redisConfigOption.Value;

            // 用于sentinel模式可使用此连接
            // var redisConnectionString = $"serviceName={_redisConfigOption.Host}:{_redisConfigOption.Port},defaultDatabase={_redisConfigOption.DefaultDb},password={_redisConfigOption.Password},connectTimeout={_redisConfigOption.ConnectTimeout}";
            var redisConnectionString = $"{_redisConfigOption.Host}:{_redisConfigOption.Port},defaultDatabase={_redisConfigOption.DefaultDb},password={_redisConfigOption.Password},connectTimeout={_redisConfigOption.ConnectTimeout}";
            _configurationOptions = ConfigurationOptions.Parse(redisConnectionString);

            var clientName = _redisConfigOption.ClientName?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(clientName) && !string.IsNullOrEmpty(_redisConfigOption.ClientName))
            {
                clientName = _redisConfigOption.ClientName.Trim();
            }

            _expiryTimeSpan = _redisConfigOption.ExpiryMilliseconds <= 0
                        ? TimeSpan.FromSeconds(60)
                        : TimeSpan.FromMilliseconds(_redisConfigOption.ExpiryMilliseconds);

            _retryTimeSpan = _redisConfigOption.RetryTimeForMilliseconds <= 0
                ? TimeSpan.FromMilliseconds(500)
                : TimeSpan.FromMilliseconds(_redisConfigOption.RetryTimeForMilliseconds);

            _clientName = string.IsNullOrEmpty(clientName) ? string.Empty : clientName.Trim();

        }
        #endregion 构造函数

        /// <summary>
        /// 使用分布式锁执行一个方法
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeOut"></param>
        /// <param name="executeAction"></param>
        /// <returns></returns>
        public async Task<bool> ExecuteWithLockAsync(string key, TimeSpan timeOut, Action executeAction)
        {
            if(_distributedLockFactory==null)
            {
                try
                {
                    await semaphoreSlim.WaitAsync();
                    if (_distributedLockFactory == null)
                    {
                        Multiplexers.Add(ConnectionMultiplexer.Connect(_configurationOptions));
                        _distributedLockFactory = RedLockFactory.Create(Multiplexers);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            }


            if(executeAction==null) return false;

            using (var redLock = await _distributedLockFactory.CreateLockAsync($"{(string.IsNullOrEmpty(_clientName) ? "" : $"{ _clientName }:")}{key}", _expiryTimeSpan, timeOut, _retryTimeSpan))
            {
                if (redLock.IsAcquired)
                {
                    executeAction();

                    return true;
                }
            }
            return false;

        }
    }
}
