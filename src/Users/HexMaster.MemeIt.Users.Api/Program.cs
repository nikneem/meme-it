using HexMaster.MemeIt.Users.Abstractions.Application.Commands;
using HexMaster.MemeIt.Users.Abstractions.Application.Users;
using HexMaster.MemeIt.Users.Abstractions.Services;
using HexMaster.MemeIt.Users.Api.Endpoints;
using HexMaster.MemeIt.Users.Application.Users;
using HexMaster.MemeIt.Users.Options;
using HexMaster.MemeIt.Users.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.Configure<UsersJwtOptions>(builder.Configuration.GetSection(UsersJwtOptions.SectionName));
builder.Services.AddSingleton<TimeProvider>(_ => TimeProvider.System);
builder.Services.AddSingleton<IUserTokenService, JwtUserTokenService>();
builder.Services.AddScoped<ICommandHandler<JoinUserCommand, JoinUserResult>, JoinUserCommandHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();
app.MapUsersEndpoints();

app.MapGet("/", () => Results.Ok("Meme-It Users API"))
   .WithName("GetUsersRoot")
   .WithTags("Diagnostics");

app.Run();
