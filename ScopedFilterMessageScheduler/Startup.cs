using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using GreenPipes;

namespace ScopedFilterMessageScheduler
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

            services.AddMassTransit(x=>
            {
                x.AddMessageScheduler(new Uri("queue:scheduler"));
                x.UsingInMemory((ctx, cfg) =>
                {
                    cfg.UsePublishFilter(typeof(MyDbContextFilter<>), ctx);
                    cfg.UseSendFilter(typeof(MyDbContextFilter<>), ctx);

                    cfg.UseInMemoryScheduler("scheduler");
                });
            });

            services.AddDbContext<MyDbContext>(opt => opt.UseInMemoryDatabase("mydb"));

            services.AddOpenApiDocument(cfg => cfg.PostProcess = d => d.Info.Title = "Sample-Api");

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, MyDbContext db)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseOpenApi(); // serve OpenAPI/Swagger documents
            app.UseSwaggerUi3(); // serve Swagger UI

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            db.Database.EnsureCreated();
        }

        public class MyDbContextFilter<T> :
    IFilter<PublishContext<T>>,
        IFilter<SendContext<T>>
    where T : class
        {
            private readonly MyDbContext _db;

            public MyDbContextFilter(MyDbContext db)
            {
                _db = db;
            }

            public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
            {
                context.GetOrAddPayload(() => _db);
                await next.Send(context);
            }

            public void Probe(ProbeContext context) { }

            public async Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
            {
                context.GetOrAddPayload(() => _db);
                await next.Send(context);
            }
        }
    }
}
