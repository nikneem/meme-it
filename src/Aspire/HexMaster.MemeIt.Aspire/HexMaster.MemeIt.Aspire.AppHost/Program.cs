var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis(AspireConstants.RedisCacheName, 64820)
    .WithLifetime(ContainerLifetime.Persistent);

var orleans = builder.AddOrleans(AspireConstants.MemeItOrleansCluster)
    .WithClustering(redis)
    .WithGrainStorage("games", redis);

builder.AddProject<Projects.HexMaster_MemeIt_Api>(AspireConstants.MemeItApiProjectName)
    .WithReference(orleans)
    .WaitFor(redis)
    .WithReplicas(2)
    .WithExternalHttpEndpoints();


builder.Build().Run();
