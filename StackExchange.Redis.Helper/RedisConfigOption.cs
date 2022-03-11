using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.Helper
{
    public class RedisConfigOption
    {
        /// <summary>
        /// 客户端名称
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// 连接地址
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 数据库
        /// </summary>
        public int DefaultDb { get; set; }

        /// <summary>
        /// redis连接超时时间(毫秒)
        /// </summary>
        public long ConnectTimeout { get; set; }

        /// <summary>
        /// 锁的过期时间(毫秒)
        /// 默认 60秒（60000毫秒）
        /// </summary>
        public long ExpiryMilliseconds { get; set; }

        /// <summary>
        /// 轮询重试获取锁，两次重试之间等待的时间（毫秒）
        /// 小于等于0 则默认为间隔5毫秒重试，其它按照指定的值等待
        /// </summary>
        public long RetryTimeForMilliseconds { get; set; }

        /// <summary>
        /// 系统自定义缓存Key前缀
        /// </summary>
        public string SysCustomKey { get; set; }

    }
}
