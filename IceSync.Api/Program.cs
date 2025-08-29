using Microsoft.EntityFrameworkCore;
using IceSync.Api.Data;
using IceSync.Api.Services;
using IceSync.Api.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddMemoryCache();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<IceSyncDbContext>(options =>
        options.UseSqlServer(connectionString));
}

builder.Services.Configure<UniversalLoaderConfig>(builder.Configuration.GetSection("UniversalLoader"));

builder.Services.AddHttpClient<UniversalLoaderApiService>();

builder.Services.AddScoped<IUniversalLoaderApiService, UniversalLoaderApiService>();
builder.Services.AddScoped<IIceSyncDbContext, IceSyncDbContext>();
builder.Services.AddScoped<IWorkflowsService, WorkflowsService>();

builder.Services.AddHostedService<WorkflowSyncService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers();

if (!string.IsNullOrEmpty(connectionString))
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IceSyncDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }
    }
    catch (InvalidOperationException)
    {
    }
}

app.Run();

public partial class Program { }
