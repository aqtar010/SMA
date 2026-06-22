using Microsoft.EntityFrameworkCore;
using SMA.API.Configuration;
using SMA.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
DependencyInjectionSwagger.AddSwaggerDocumentation(builder.Services);

builder.Services.AddCors(x => x.AddPolicy("allow-all", policy =>
{
    policy.AllowAnyOrigin();
    policy.AllowAnyMethod();
    policy.AllowAnyHeader();
}));

// Configure Entity Framework to use PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
DependencyInjectionAuth.AddJwtAuthentication(builder.Services, builder.Configuration);

var app = builder.Build();

// Apply pending migrations automatically on startup (development only)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        //db.Database.EnsureDeleted();  // Drop the DB
        db.Database.Migrate();        // Recreate it with migrations
    }
}

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Only redirect to HTTPS when not in Development (where certs may not be present)
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
