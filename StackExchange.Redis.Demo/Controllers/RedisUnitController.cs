using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis.GenerateNumbers.Impl;
using StackExchange.Redis.Helper;
using StackExchange.Redis.Helper.Lock;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StackExchange.Redis.Demo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RedisUnitController : ControllerBase
    {
        public readonly ICacheManager _cacheManager;
        public readonly IRedisLock _redisLock;
        public readonly OrderNoGenerator _orderNoGenerator;


        public RedisUnitController(ICacheManager cacheManager, IRedisLock redisLock, OrderNoGenerator orderNoGenerator)
        {
            _cacheManager=cacheManager;
            _redisLock=redisLock;
            _orderNoGenerator=orderNoGenerator; 


        }

        #region Redis单元测试
        /// <summary>
        /// Redis 单元测试
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public dynamic RedisUnitTest()
        {

            #region String

            string str = "123";
            Demo demo = new Demo()
            {
                Id = 1,
                Name = "123"
            };
            var resukt = _cacheManager.StringSet("redis_string_test", str);
            var str1 = _cacheManager.StringGet("redis_string_test");
            _cacheManager.StringSet("redis_string_model", demo);
            var model = _cacheManager.StringGet<Demo>("redis_string_model");

            for (int i = 0; i < 10; i++)
            {
                _cacheManager.StringIncrement("StringIncrement", 2);
            }
            for (int i = 0; i < 10; i++)
            {
                _cacheManager.StringDecrement("StringIncrement");
            }
            _cacheManager.StringSet("redis_string_model1", demo, TimeSpan.FromSeconds(10));

            #endregion String

            #region List

            for (int i = 0; i < 10; i++)
            {
                _cacheManager.ListRightPush("list", i);
            }

            for (int i = 10; i < 20; i++)
            {
                _cacheManager.ListLeftPush("list", i);
            }
            var length = _cacheManager.ListLength("list");

            var leftpop = _cacheManager.ListLeftPop<string>("list");
            var rightPop = _cacheManager.ListRightPop<string>("list");

            var list = _cacheManager.ListRange<int>("list");

            #endregion List

            #region Hash

            _cacheManager.HashSet("user", "u1", "123");
            _cacheManager.HashSet("user", "u2", "1234");
            _cacheManager.HashSet("user", "u3", "1235");
            var news = _cacheManager.HashGet<string>("user", "u2");

            #endregion Hash

            #region 发布订阅

            _cacheManager.Subscribe("Channel1");
            for (int i = 0; i < 10; i++)
            {
                _cacheManager.Publish("Channel1", "msg" + i);
                if (i == 2)
                {
                    _cacheManager.Unsubscribe("Channel1");
                }
            }

            #endregion 发布订阅

            #region 事务

            var tran = _cacheManager.CreateTransaction();

            tran.StringSetAsync("tran_string", "test1");
            tran.StringSetAsync("tran_string1", "test2");
            bool committed = tran.Execute();

            #endregion 事务

            #region Lock

            var db = _cacheManager.GetDatabase();
            RedisValue token = Environment.MachineName;
            if (db.LockTake("lock_test", token, TimeSpan.FromSeconds(10)))
            {
                try
                {
                    //TODO:开始做你需要的事情
                    Thread.Sleep(5000);
                }
                finally
                {
                    db.LockRelease("lock_test", token);
                }
            }

            #endregion Lock

            return Ok();
        }
        #endregion

        #region Redis分布式锁测试
        // 商品总数（模拟数据库商品库存总数）
        public int count = 50;
        /// <summary>
        /// Redis分布式锁测试
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<dynamic> RedisLockTest()
        {

            for (int i = 0; i < 60; i++)
            {
              await Task.Run( async () => {

                  #region 使用分布式锁
                  await _redisLock.ExecuteWithLockAsync("RedisLockKey", TimeSpan.FromMinutes(10), () =>
                   {
                       Console.WriteLine($"当前商品库存：{count}");

                       if (count == 0)
                       {
                           Console.WriteLine($"当前库存不足,秒杀失败：{count}");
                           return;
                       }

                       Console.WriteLine($"恭喜秒杀成功，商品编号：{count}");
                       count--;
                   });
                  #endregion

                  #region 不使用分布式锁
                  //Console.WriteLine($"当前商品库存：{count}");

                  //if (count == 0)
                  //{
                  //    Console.WriteLine($"当前库存不足,秒杀失败：{count}");
                  //    return;
                  //}

                  //Console.WriteLine($"恭喜秒杀成功，商品编号：{count}");
                  //count--;
                  #endregion

              });
            }
            return Ok();
        }
        #endregion

        #region 同一分钟内生成自增订单号
        /// <summary>
        /// 同一分钟内生成自增订单号
        /// </summary>
        /// <returns></returns>
        public async Task<dynamic> RedisOrderNoGeneratorTest()
        {
            var result=await _orderNoGenerator.GeneratorNumberAsync(DateTime.Now);
            return Ok(result);
        }
        #endregion
    }

    public class Demo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
