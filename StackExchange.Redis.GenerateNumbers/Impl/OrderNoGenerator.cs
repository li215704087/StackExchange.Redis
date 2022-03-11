using StackExchange.Redis.Helper;
using StackExchange.Redis.Helper.Lock;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StackExchange.Redis.GenerateNumbers.Impl
{
    public class OrderNoGenerator: GeneratorNumberBase
    {

        private readonly IRedisLock _redisLock;
        private readonly ICacheManager _cacheManager;

        public OrderNoGenerator(IRedisLock redisLock, ICacheManager cacheManager) : base(redisLock, cacheManager)
        {
            _redisLock = redisLock;
            _cacheManager = cacheManager;
        }
        

        public override async Task<long> ReadMaxNumberFromSourceAsync(DateTime nowTime)
        {

            // TODO 此处逻辑应该根据nowTime参数从数据库查最近的一条数据
            // 此处逻辑省略，默认赋值为1
            var maxNumber = 1;
            return await Task.FromResult(maxNumber);
        }

        public override string GetValueFormat(DateTime nowTime)
        {
            return $"{nowTime.ToString("yyyyMMddHHmm")}{{0}}";
        }

        public override string GetKey(DateTime nowTime)
        {
            return $"Demo.Redis:GeneratorNumber:OrderNo_{nowTime.ToString("yyyyMMddHHmm")}";
        }

        public override int ValueLength()
        {
            return 3;
        }
        public override TimeSpan GetExpireTime()
        {
            return TimeSpan.FromHours(1);
        }
    }
}
