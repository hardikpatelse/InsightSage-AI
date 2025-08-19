using InsightSage.Application.Services;
using InsightSage.Data;
using InsightSage.DataContext;
using InsightSage.Infrastructure;
using InsightSage.Shared.Middleware.Extensions;
using InsightSage.Shared.Interfaces.DataContexts;
using InsightSage.Shared.Interfaces.Others;
using InsightSage.Shared.Interfaces.Services;
using InsightSage.Shared.Models.Entities;
using InsightSage.Shared.Models.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "InsightSage Auth API", Version = "v1" });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var tenantId = builder.Configuration["AzureAd:TenantId"];
        var clientId = builder.Configuration["AzureAd:ClientId"];
        
        // TEMPORARY FIX: Accept both access tokens AND id tokens for SPA scenarios
        options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false, // Temporary: Allow flexible audience validation
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5),
            
            // Accept multiple audience types for flexibility
            ValidAudiences = new[]
            {
                clientId, // SPA Client ID (for ID tokens)
                $"api://{clientId}", // API identifier (for access tokens when configured)
                "00000003-0000-0000-c000-000000000000" // Microsoft Graph (fallback)
            },
            ValidIssuers = new[]
            {
                $"https://login.microsoftonline.com/{tenantId}/v2.0",
                $"https://sts.windows.net/{tenantId}/",
                $"https://login.microsoftonline.com/{tenantId}/"
            }
        };

        // Add event handlers for debugging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError("Authentication failed: {Error}", context.Exception.Message);
                
                // Log more details about the token for debugging
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader.Substring("Bearer ".Length);
                    logger.LogDebug("Failed token (first 50 chars): {TokenStart}", token.Substring(0, Math.Min(50, token.Length)));
                }
                
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Authentication challenge: {Error} - {ErrorDescription}", 
                    context.Error, context.ErrorDescription);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Token validated successfully for user: {User}", context.Principal?.Identity?.Name ?? "Unknown");
                
                // Log token claims for debugging
                var claims = context.Principal.Claims.Select(c => new { c.Type, c.Value }).ToList();
                logger.LogDebug("Token claims: {@Claims}", claims);
                
                // Log audience specifically
                var audience = context.Principal.Claims.FirstOrDefault(c => c.Type == "aud")?.Value;
                logger.LogDebug("Token audience: {Audience}", audience);
                
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var token = context.Request.Headers["Authorization"].FirstOrDefault();
                logger.LogDebug("Received token: {HasToken}", !string.IsNullOrEmpty(token));
                return Task.CompletedTask;
            }
        };
    });

// Add CORS policy
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:4200", "https://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalAngular",
        policy => policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()); // Required for SignalR
});

builder.Services.AddHttpContextAccessor(); // Needed for scoped services

#region Application Services
builder.Services.AddScoped<IUserService<UserResponse<List<User>>, UserResponse<User>, User>, UserService>();
#endregion

#region Connection Strings
builder.Services.AddDbContext<InsightSageDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("InsightSageDb")));
#endregion

#region Data Contexts
builder.Services.AddScoped<IUserDataContext, UserDataContext>();
#endregion

#region Infrastructure Services
builder.Services.AddScoped<IUserContext, UserContext>();
#endregion

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowLocalAngular");

app.UseAuthentication();
app.UseAuthorization();

// Add the ResponseStatusMiddleware after authentication and authorization
// This middleware automatically sets HttpStatusCode.OK in BaseResponse.Status
// when there are no errors and no exception details
app.UseResponseStatusMiddleware();

app.MapControllers();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "InsightSage Auth API v1");
    });
}

app.Run();
