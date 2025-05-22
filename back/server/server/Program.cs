using server.Services.MongoDb;
using server.Services.Authorization;
using server.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using server.Interfaces;
using server.Services.Embeddings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings")
);

builder.Services.Configure<AtlasApiSettings>(
    builder.Configuration.GetSection("AtlasApiSettings")
);

builder.Services.AddSingleton(sp =>
{
    var settings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>()!;
    return new MongoDbService(settings);
});

builder.Services.AddSingleton(sp =>
{
    var settings = builder.Configuration.GetSection("AtlasApiSettings").Get<AtlasApiSettings>()!;
    return settings;
});

builder.Services.AddSingleton(sp =>
{
    var settings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>()!;
    return new MongoDbService(settings);
});

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(nameof(JwtOptions))
);

builder.Services.AddSingleton<IJwtProvider, MyJwtProvider>();
builder.Services.AddSingleton<IPasswordHasher, MyPasswordHasher>();

builder.Services.AddScoped<ClientsService>();
builder.Services.AddScoped<QuestionsService>();
builder.Services.AddScoped<CustomerService>();

builder.Services.AddScoped<IEmbeddingProviderFactory, EmbeddingProviderFactory>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtOptions = builder.Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>()!;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["access_token"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("JWT помилка: " + context.Exception.Message);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();

var app = builder.Build();


app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
