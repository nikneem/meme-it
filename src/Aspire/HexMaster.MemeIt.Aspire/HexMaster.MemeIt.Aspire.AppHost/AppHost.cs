var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.HexMaster_MemeIt_Games_Api>("hexmaster-memeit-games-api");

builder.Build().Run();
