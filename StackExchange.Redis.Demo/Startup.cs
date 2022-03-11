using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis.GenerateNumbers.Impl;
using StackExchange.Redis.Helper;
using StackExchange.Redis.Helper.Lock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StackExchange.Redis.Demo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // ×¢ÈëredisÅäÖÃÐÅÏ¢
            services.AddRedisCache(config => {

                config.ClientName = Configuration["Redis:ClientName"];
                config.Host = Configuration["Redis:Host"];
                config.Port = int.Parse(Configuration["Redis:Port"]);
                config.Password = Configuration["Redis:Password"];
                config.DefaultDb = int.Parse(Configuration["Redis:DefaultDb"]);
                config.ConnectTimeout = long.Parse(Configuration["Redis:ConnectTimeout"]);
                config.ExpiryMilliseconds = long.Parse(Configuration["Redis:ExpiryMilliseconds"]);
                config.RetryTimeForMilliseconds = long.Parse(Configuration["Redis:RetryTimeForMilliseconds"]);
                config.SysCustomKey = Configuration["Redis:SysCustomKey"];

            }, registerGeneratorClass => {

                registerGeneratorClass.AddSingleton<ICacheManager, CacheManager>();
                registerGeneratorClass.AddSingleton<IRedisLock, RedisLock>();

                registerGeneratorClass.AddSingleton<OrderNoGenerator>();

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
