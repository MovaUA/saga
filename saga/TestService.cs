using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace saga
{
    public class TestService : IHostedService
    {
        private readonly ILogger logger;
        private readonly IMongodbSettings mongodbSettings;

        public TestService(IMongodbSettings mongodbSettings, ILoggerFactory loggerFactory)
        {
            this.mongodbSettings = mongodbSettings;
            this.logger = loggerFactory.CreateLogger<TestService>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"starting test service... DatabaseName:{this.mongodbSettings.DatabaseName}");

            var client = new MongoClient(this.mongodbSettings.ConnectionString);
            var database = client.GetDatabase("api");
            var collection = database.GetCollection<Test>("tests");

            return collection.InsertOneAsync(new Test
            {
                Name = "test",
                Time = DateTimeOffset.Now
            }, cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"stopping test service... DatabaseName:{this.mongodbSettings.DatabaseName}");

            return Task.CompletedTask;
        }
    }
}