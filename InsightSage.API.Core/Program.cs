using InsightSage.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "InsightSage Core API", Version = "v1" });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var tenantId = builder.Configuration["AzureAd:TenantId"];
        var clientId = builder.Configuration["AzureAd:ClientId"];

        options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
        options.Audience = clientId;  // Your SPA App Registration ID
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true
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

#region Application Services

#endregion

#region Connection Strings
builder.Services.AddDbContext<InsightSageDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ChatBotDb")));
#endregion

#region Data Contexts

#endregion

#region Infrastructure Services

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

app.UseAuthorization();

app.MapControllers();
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "InsightSage Core API v1");
    });
}
app.Run();
