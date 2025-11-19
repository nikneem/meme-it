var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis").WithRedisInsight();

var redisHost = redis.Resource.PrimaryEndpoint.Property(EndpointProperty.Host);
var redisPort = redis.Resource.PrimaryEndpoint.Property(EndpointProperty.Port);

var stateStore = builder.AddDaprStateStore("memeit-statestore")
    .WaitFor(redis);

builder.AddProject<Projects.HexMaster_MemeIt_Games_Api>("hexmaster-memeit-games-api").WithDaprSidecar(sidecar =>
    {
        sidecar.WithReference(stateStore);
    });

builder.Build().Run();
