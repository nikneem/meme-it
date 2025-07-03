var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MemeIt_Api>("memeit-api");

builder.Build().Run();
