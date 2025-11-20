using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.MongoDB;
using Aspire.Hosting.Yarp.Transforms;

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

var gamesApi = builder.AddProject<Projects.HexMaster_MemeIt_Games_Api>("hexmaster-memeit-games-api")
    .WithReference(gamesDatabase)
    .WithDaprSidecar(sidecar =>
    {
        sidecar.WithReference(stateStore)
        .WithReference(pubSub);
    })
    .WaitFor(gamesDatabase);

var usersApi = builder.AddProject<Projects.HexMaster_MemeIt_Users_Api>("hexmaster-memeit-users-api");

var memesApi = builder.AddProject<Projects.HexMaster_MemeIt_Memes_Api>("hexmaster-memeit-memes-api");

var realtimeApi = builder.AddProject<Projects.HexMaster_MemeIt_Realtime_Api>("hexmaster-memeit-realtime-api")
    .WithDaprSidecar(sidecar =>
{
    sidecar.WithReference(stateStore)
    .WithReference(pubSub);
});


// Add YARP gateway
var gateway = builder.AddYarp("gateway")
    .WithHostPort(5000)
    .WithConfiguration(yarp =>
    {
        // Proxy /profielen routes to the Profielen API
        yarp.AddRoute("/games/{**catch-all}", gamesApi)
            .WithTransformPathRemovePrefix("/games")
            .WithTransformPathPrefix("/api/games");

        yarp.AddRoute("/users/{**catch-all}", usersApi)
            .WithTransformPathRemovePrefix("/users")
            .WithTransformPathPrefix("/api/users");

        yarp.AddRoute("/memes/{**catch-all}", memesApi)
            .WithTransformPathRemovePrefix("/memes")
            .WithTransformPathPrefix("/api/memes");

        yarp.AddRoute("/realtime/{**catch-all}", realtimeApi)
            .WithTransformPathRemovePrefix("/realtime")
            .WithTransformPathPrefix("/api/realtime");
    });


var frontEndSourceFolder = Path.GetFullPath(builder.AppHostDirectory + "../../../../MemeItApp");
if (Directory.Exists(frontEndSourceFolder))
{
    var frontend = builder.AddJavaScriptApp("frontend", frontEndSourceFolder)
        .WaitFor(gateway)
        .WithRunScript("start")
        .WithHttpEndpoint(port: 4200, isProxied: false)
        .WithEnvironment("ASPIRE_GATEWAY_URL", gateway.GetEndpoint("http"));
}

builder.Build().Run();
