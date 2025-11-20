using HexMaster.MemeIt.Users.Abstractions.Application.Commands;
using HexMaster.MemeIt.Users.Abstractions.Application.Users;
using HexMaster.MemeIt.Users.Abstractions.Services;
using HexMaster.MemeIt.Users.Api.Endpoints;
using HexMaster.MemeIt.Users.Application.Users;
using HexMaster.MemeIt.Users.Options;
using HexMaster.MemeIt.Users.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("Authorization")
              .AllowCredentials();
    });
});
builder.Services.Configure<UsersJwtOptions>(builder.Configuration.GetSection(UsersJwtOptions.SectionName));
builder.Services.AddSingleton<TimeProvider>(_ => TimeProvider.System);
builder.Services.AddSingleton<IUserTokenService, JwtUserTokenService>();
builder.Services.AddScoped<ICommandHandler<JoinUserCommand, JoinUserResult>, JoinUserCommandHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.Title = "Meme-It Users API");
}

app.UseCors("AllowAngularApp");
app.MapDefaultEndpoints();
app.MapUsersEndpoints();

app.MapGet("/", () => Results.Ok("Meme-It Users API"))
   .WithName("GetUsersRoot")
   .WithTags("Diagnostics");

app.Run();
