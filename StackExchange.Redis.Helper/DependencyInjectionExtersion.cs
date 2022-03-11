using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.Helper
{
    public static class DependencyInjectionExtersion
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services
            , Action<RedisConfigOption> setupAction
            , Action<IServiceCollection> registerGeneratorClass)
        {
            if(services==null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if(setupAction==null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.Configure(setupAction);
            registerGeneratorClass(services);
            return services;

        }

    }
}
