using Microsoft.Extensions.Options;
using StackExchange.Redis.Helper;
using StackExchange.Redis.Helper.Lock;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StackExchange.Redis.GenerateNumbers
{
    public abstract class GeneratorNumberBase
    {
        private readonly IRedisLock _redisLock;
        private readonly ICacheManager _cacheManager;

        public GeneratorNumberBase(IRedisLock redisLock, ICacheManager cacheManager)
        {
            _redisLock = redisLock;
            _cacheManager = cacheManager;
        }
        public async virtual Task<string> GeneratorNumberAsync(DateTime nowTime)
        {
            var key = GetKey(nowTime);
            var valueFormat = GetValueFormat(nowTime);
            if (!await _cacheManager.KeyExistsAsync(key))
            {
                var lockResult = await _redisLock.ExecuteWithLockAsync("Demo.Redis:Lock:GeneratorNumber", TimeSpan.FromSeconds(10), () =>
                {
                    if (!_cacheManager.KeyExistsAsync(key).GetAwaiter().GetResult())
                    {
                        var maxNumber = ReadMaxNumberFromSourceAsync(nowTime).GetAwaiter().GetResult();
                        if (GetExpireTime().TotalSeconds > 0)
                        {
                            _cacheManager.StringSetAsync(key, maxNumber, GetExpireTime()).GetAwaiter().GetResult();
                        }
                        else
                        {
                            _cacheManager.StringSetAsync(key, maxNumber).GetAwaiter().GetResult();
                        }
                    }

                });

                if(!lockResult)
                {
                    throw new Exception("获取锁失败");
                }
            }

            var currentNumber = await _cacheManager.StringIncrementAsync(key);
            // TODO 此处有一个逻辑，需要将自增的数据currentNumber 写入数据库中，方便下次生成编号的时候从数据库读取判断
            {
                // 此处逻辑省略
            }

            return string.Format(valueFormat, currentNumber.ToString().PadLeft(ValueLength(), '0'));
        }

        public abstract Task<long> ReadMaxNumberFromSourceAsync(DateTime nowTime);

        public abstract string GetValueFormat(DateTime nowTime);


        public abstract string GetKey(DateTime nowTime);


        public abstract int ValueLength();


        public abstract TimeSpan GetExpireTime();
       

    }
}
