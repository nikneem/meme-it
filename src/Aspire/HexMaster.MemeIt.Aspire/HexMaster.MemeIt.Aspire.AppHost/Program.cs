var builder = DistributedApplication.CreateBuilder(args);


var storage = builder.AddAzureStorage("storage").RunAsEmulator();
var clusteringTable = storage.AddTables("clustering");
var grainStorage = storage.AddBlobs("grain-state");


var orleans = builder.AddOrleans("default")
    .WithClustering(clusteringTable)
    .WithGrainStorage("Default", grainStorage);


builder.AddProject<Projects.HexMaster_MemeIt_OrleansServer>("silo")
    .WithReference(orleans)
    .WithReplicas(3);

builder.AddProject<Projects.HexMaster_MemeIt_Api>("hexmaster-memeit-api")
    .WithReference(orleans.AsClient())
    .WithExternalHttpEndpoints()
    .WithReplicas(3);

builder.Build().Run();
