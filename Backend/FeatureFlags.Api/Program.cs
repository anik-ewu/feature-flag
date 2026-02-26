using FeatureFlags.Application;
using FeatureFlags.Infrastructure;
using FeatureFlags.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, loggerConfig) => 
    loggerConfig.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:4200") // Default Angular port
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// Add Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Auto-Apply EF Core Migrations and Seed Demo Data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();

    var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    var projectId = Guid.Empty;

    var tenantExists = await context.Tenants.AnyAsync(t => t.Id == tenantId);
    if (!tenantExists)
    {
        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO \"Tenants\" (\"Id\", \"Name\", \"ApiKey\", \"IsActive\", \"CreatedAtUtc\") VALUES ({0}, {1}, {2}, {3}, {4})",
            tenantId, "Demo Tenant", "sk_live_demo", true, DateTime.UtcNow);
    }

    var projectExists = await context.Projects.AnyAsync(p => p.Id == projectId);
    if (!projectExists)
    {
        await context.Database.ExecuteSqlRawAsync(
            "INSERT INTO \"Projects\" (\"Id\", \"TenantId\", \"Name\", \"CreatedAtUtc\") VALUES ({0}, {1}, {2}, {3})",
            projectId, tenantId, "Demo Project", DateTime.UtcNow);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
