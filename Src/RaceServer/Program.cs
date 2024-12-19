using System;
using System.Linq;
using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MqttService;
using MqttService.Abstraction;

using RaceServer;
using RaceServer.Abstraction;
using RaceServer.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .ConfigureServices((context, services) =>
    {
        var configurationRoot = context.Configuration;
        services.Configure<RobotMapping>(configurationRoot.GetSection(nameof(RobotMapping)));
    });

builder.Services
    .AddHostedService<MqttBackgroundService>()
    .AddSingleton<IMqttService, MappingService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

var app = builder.Build();

app.UsePathBase("/inputserver");

// if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add CORS to support Single Page Apps (SPAs)
app.UseCors(b => b.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseHttpsRedirection();

static Info Info()
{
    var ass     = Assembly.GetExecutingAssembly();
    var assName = ass.GetName();

    return new Info()
    {
        Version   = ass.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion!,
        Copyright = ass.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright!,
        Name      = assName.Name!,
        FullName  = assName.FullName,
    };
}

const string InfoTag    = "Info";
const string MappingTag = "Mapping";

app.MapGet("/api/info",           () => Info()).WithOpenApi().WithTags(InfoTag);
app.MapGet("/api/info/version",   () => Info().Version.ToString()).WithOpenApi().WithTags(InfoTag);
app.MapGet("/api/info/name",      () => Info().Name).WithOpenApi().WithTags(InfoTag);
app.MapGet("/api/info/fullname",  () => Info().FullName).WithOpenApi().WithTags(InfoTag);
app.MapGet("/api/info/copyright", () => Info().Copyright).WithOpenApi().WithTags(InfoTag);

app.MapGet("/api/mapping", (IMqttService     service) => ((IMappingService)service).GetMapping().Select(x => new Tuple<string,string>(x.from,x.to))).WithOpenApi().WithTags(MappingTag);
app.MapDelete("/api/mapping/{from}", (string from, IMqttService service) => ((IMappingService)service).RemoveMapping(from)).WithOpenApi().WithTags(MappingTag);
app.MapPut("/api/mapping/{from}", (string from, string to, IMqttService service) => ((IMappingService)service).UpdateMapping(from,to)).WithOpenApi().WithTags(MappingTag);

await app.RunAsync();