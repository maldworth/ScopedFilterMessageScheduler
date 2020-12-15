using GreenPipes;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScopedFilterMessageScheduler.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMessageScheduler _scheduler;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IPublishEndpoint publishEndpoint, IMessageScheduler scheduler)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _scheduler = scheduler;
        }

        [HttpPost("schedule")]
        public async Task<IActionResult> Schedule(int seconds)
        {
            await _scheduler.SchedulePublish(DateTime.Now.AddSeconds(seconds), new MyMessage { Id = NewId.NextGuid(), Timestamp = InVar.Timestamp }, Pipe.Execute<SendContext>(ctx =>
            {
                if (ctx.TryGetPayload<MyDbContext>(out var db))
                {
                    db.Persons.Add(new Person { Name = "TestSchedule" });
                }
            }));

            return Ok();
        }

        [HttpPost("publish")]
        public async Task<IActionResult> Publish()
        {
            await _publishEndpoint.Publish(new MyMessage { Id = NewId.NextGuid(), Timestamp = InVar.Timestamp }, ctx =>
            {
                if (ctx.TryGetPayload<MyDbContext>(out var db))
                {
                    db.Persons.Add(new Person { Name = "TestPublish" });
                }
            });
            return Ok();
        }

        public class MyMessage
        {
            public Guid Id { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}
