using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StackExchange.Redis.Helper.Lock
{
    public interface IRedisLock
    {
        /// <summary>
        /// 使用分布式锁执行一个方法
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeOut"></param>
        /// <param name="executeAction"></param>
        /// <returns></returns>
        Task<bool> ExecuteWithLockAsync(string key, TimeSpan timeOut, Action executeAction);
    }
}
