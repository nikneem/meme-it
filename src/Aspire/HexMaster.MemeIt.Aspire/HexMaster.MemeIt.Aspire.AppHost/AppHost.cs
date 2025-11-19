using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.MongoDB;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis").WithRedisInsight();

var mongo = builder.AddMongoDB("games-mongo")
    .WithLifetime(ContainerLifetime.Persistent);

var gamesDatabase = mongo.AddDatabase("games-db");

var stateStore = builder.AddDaprStateStore("memeit-statestore")
    .WaitFor(redis);

builder.AddProject<Projects.HexMaster_MemeIt_Games_Api>("hexmaster-memeit-games-api")
    .WithReference(gamesDatabase)
    .WithDaprSidecar(sidecar =>
    {
        sidecar.WithReference(stateStore);
    })
    .WaitFor(gamesDatabase);

builder.Build().Run();
