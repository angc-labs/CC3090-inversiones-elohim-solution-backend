using ElohimShop.Infrastructure.Persistence;
using ElohimShop.Application.Auth;
using ElohimShop.Application.Products;
using ElohimShop.Application.Usuario;
using ElohimShop.Application.Catalog;
using ElohimShop.Application.Carrito;
using ElohimShop.Application.Reservacion;
using ElohimShop.Infrastructure.Auth;
using ElohimShop.Infrastructure.Products;
using ElohimShop.Infrastructure.User;
using ElohimShop.Infrastructure.Catalog;
using ElohimShop.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "ElohimShop API",
        Version = "v1",
        Description = "API de autenticación y operaciones para clientes de Elohim Shop"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Ingrese el token JWT con el esquema Bearer. Ejemplo: Bearer {token}"
    });

    options.AddSecurityRequirement(_ => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "http://localhost:5173",
            "http://20.80.105.45:3000",
            "http://20.80.105.45:5000"
        )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenRevocationService, TokenRevocationService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUsuarioService, ElohimShop.Infrastructure.User.UsuarioService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<ICarritoService, CarritoService>();
builder.Services.AddScoped<IReservacionService, ReservacionService>();
builder.Services.AddScoped<IPasswordHashing, PasswordHashingService>();

// Register DbContext with PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ElohimShopDbContext>(options =>
    options.UseNpgsql(connectionString));

// JWT Configuration
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
    ?? builder.Configuration["JWT_KEY"]
    ?? jwtSection["Key"]
    ?? throw new InvalidOperationException("JWT Key no configurada. Configure la variable de entorno JWT_KEY.");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? builder.Configuration["JWT_ISSUER"]
    ?? jwtSection["Issuer"]
    ?? "ElohimShop";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? builder.Configuration["JWT_AUDIENCE"]
    ?? jwtSection["Audience"]
    ?? "ElohimShop.Clients";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var revocationService = context.HttpContext.RequestServices
                    .GetRequiredService<ITokenRevocationService>();
                var jti = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);

                if (string.IsNullOrWhiteSpace(jti))
                {
                    context.Fail("Token sin identificador.");
                    return;
                }

                if (await revocationService.IsTokenRevokedAsync(jti, context.HttpContext.RequestAborted))
                {
                    context.Fail("Token revocado.");
                }
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ElohimShop API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();