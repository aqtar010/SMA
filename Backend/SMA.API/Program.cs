using Microsoft.EntityFrameworkCore;
using SMA.API.Configuration;
using SMA.API.Data;
using SMA.API.Hubs;
using SMA.API.Services.ServiceContracts;
using SMA.API.Services.ServiceImplementation;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
DependencyInjectionSwagger.AddSwaggerDocumentation(builder.Services);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Do not use .AllowAnyOrigin() or "*"
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for SignalR
    });
});
// Configure Entity Framework to use PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
DependencyInjectionAuth.AddJwtAuthentication(builder.Services, builder.Configuration);
builder.Services.AddScoped<ITokenService, TokenService>();

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
app.UseCors();
app.UseAuthorization();

app.MapHub<ProductHub>("/hubs/productHub");
app.MapControllers();

app.Run();
