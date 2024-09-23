using API.Data;
using API.Extensions;
using API.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

builder.Services.AddCors(options =>
            {                
                options.AddPolicy("CorsPolicy", 
                builder => builder
                .WithOrigins("http://localhost:4200", "https://localhost:4200")
                .WithMethods("GET", "POST", "PUT", "DELETE")
                .AllowAnyHeader().AllowCredentials().Build());
            });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();
// app.UseCors(x => x.AllowAnyHeader().AllowAnyHeader()
//     .WithMethods("GET", "POST", "PUT", "DELETE")
//     .WithOrigins("http://localhost:4200", "https://localhost:4200"));

app.UseRouting();

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    await Seed.SeedUsers(context);
}
catch (Exception ex) 
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration");
}

app.Run();
