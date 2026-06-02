using Aufgabe3_RestfulApi.Cmds;
using Aufgabe3_RestfulApi.Infrastructure;
using Aufgabe3_RestfulApi.Services;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Aufgabe3_RestfulApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.AddValidation();
        builder.Services.AddDbContext<MakerspaceContext>(options => options
            .UseSqlite("Data Source=Makerspace.db"));

        builder.Services.AddScoped<IMakerspaceService, MakerspaceService>();
        
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MakerspaceContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.SeedMakerspace();
        }

        if (app.Environment.IsEnvironment("Testing"))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();

        app.MapGet("/api/devices/{id:int}", async (IMakerspaceService service, int id, CancellationToken ct) =>
        {
            var device = await service.GetDeviceDetail(id, ct);
            return device is null ? Results.NotFound() : Results.Ok(device);
        });

        app.MapPost("/api/devices", async (IMakerspaceService service, CreateDeviceCmd cmd, CancellationToken ct) =>
        {
            try
            {
                var device = await service.CreateDevice(cmd, ct);
                return Results.Created($"/api/devices/{device.Id}", device);
            }
            catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException or InvalidOperationException)
            {
                return ToProblemResult(ex);
            }
        });

        app.MapPatch("/api/devices/{id:int}/status", async (IMakerspaceService service, int id, UpdateDeviceStatusCmd cmd, CancellationToken ct) =>
        {
            try
            {
                var device = await service.UpdateDeviceStatus(id, cmd, ct);
                return Results.Ok(device);
            }
            catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException or InvalidOperationException)
            {
                return ToProblemResult(ex);
            }
        });

        app.MapDelete("/api/devices/{id:int}/status", async (IMakerspaceService service, int id, CancellationToken ct) =>
        {
            try
            {
                var device = await service.DeleteDevice(id, ct);
                return Results.Ok(device);
            }
            catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException or InvalidOperationException)
            {
                return ToProblemResult(ex);
            }
        });
        
        app.Run();
    }

    private static IResult ToProblemResult(Exception ex)
    {
        int statusCode = ex switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            InvalidOperationException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(ex.Message, statusCode: statusCode);
    }
}
