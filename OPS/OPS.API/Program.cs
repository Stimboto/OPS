using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OPS.Infrastructure.Data;
using System.Text;
using OPS.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<OpsDbContext>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "OPS API",
        Version = "v1",
        Description = "Operations and Incident Management API"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\""
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddSignalR();
builder.Services.AddScoped<OPS.Application.Interfaces.IAuthService, OPS.Infrastructure.Services.AuthService>();
builder.Services.AddScoped<OPS.Application.Interfaces.IIncidentService, OPS.Infrastructure.Services.IncidentService>();
builder.Services.AddScoped<OPS.Application.Interfaces.INotificationService, OPS.Infrastructure.Services.NotificationService>();
builder.Services.AddScoped<OPS.Application.Interfaces.ITeamService, OPS.Infrastructure.Services.TeamService>();
builder.Services.AddScoped<OPS.Application.Interfaces.IAnalyticsService, OPS.Infrastructure.Services.AnalyticsService>();
builder.Services.AddScoped<OPS.Application.Interfaces.IRealtimeNotificationService, OPS.API.Services.RealtimeNotificationService>();
builder.Services.AddScoped<OPS.Application.Interfaces.IIncidentCommentService, OPS.Infrastructure.Services.IncidentCommentService>();
builder.Services.AddScoped<OPS.Application.Interfaces.IAttachmentService, OPS.Infrastructure.Services.AttachmentService>();
builder.Services.AddScoped<OPS.Application.Interfaces.IActivityFeedService, OPS.Infrastructure.Services.ActivityFeedService>();
builder.Services.AddSingleton<OPS.Application.Interfaces.ISlaPolicyProvider, OPS.Infrastructure.Services.SlaPolicyProvider>();
builder.Services.AddHostedService<OPS.Infrastructure.BackgroundServices.SlaMonitoringService>();
// Swagger setup
builder.Services.AddOpenApi();

// Database setup
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<OpsDbContext>(options =>
    options.UseSqlServer(connectionString));

// JWT Authentication setup
var jwtKey = builder.Configuration["JwtSettings:Secret"] ?? "super_secret_key_that_should_be_long_enough_for_hmacsha256";
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "OPS.API";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "OPS.Client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/operations"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ReporterPolicy", policy => policy.RequireRole("Reporter", "Responder", "Manager", "Admin"));
    options.AddPolicy("ResponderPolicy", policy => policy.RequireRole("Responder", "Manager", "Admin"));
    options.AddPolicy("ManagerPolicy", policy => policy.RequireRole("Manager", "Admin"));
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
});

// CORS (allow Angular app)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy => policy.WithOrigins("http://localhost:4200")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngularApp");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<OperationsHub>("/hubs/operations");
app.MapHealthChecks("/health");

app.Run();
