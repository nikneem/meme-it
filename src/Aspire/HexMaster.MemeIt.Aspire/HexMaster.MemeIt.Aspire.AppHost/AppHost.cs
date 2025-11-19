using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.MongoDB;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis").WithRedisInsight();

var mongo = builder.AddMongoDB("games-mongo")
    .WithLifetime(ContainerLifetime.Persistent);
var gamesDatabase = mongo.AddDatabase("games-db");

// # Dapr State Store and PubSub using Redis #
var stateStore = builder.AddDaprStateStore("memeit-statestore")
    .WaitFor(redis);

var redisHost = redis.Resource.PrimaryEndpoint.Property(EndpointProperty.Host);
var redisPort = redis.Resource.PrimaryEndpoint.Property(EndpointProperty.Port);

var pubSub = builder
    .AddDaprPubSub("chatservice-pubsub")
    .WithMetadata(
        "redisHost",
        ReferenceExpression.Create($"{redisHost}:{redisPort}")
    )
    .WaitFor(redis);

builder.AddProject<Projects.HexMaster_MemeIt_Games_Api>("hexmaster-memeit-games-api")
    .WithReference(gamesDatabase)
    .WithDaprSidecar(sidecar =>
    {
        sidecar.WithReference(stateStore)
        .WithReference(pubSub);
    })
    .WaitFor(gamesDatabase);

builder.AddProject<Projects.HexMaster_MemeIt_Users_Api>("hexmaster-memeit-users-api");

builder.AddProject<Projects.HexMaster_MemeIt_Memes_Api>("hexmaster-memeit-memes-api");

builder.Build().Run();
