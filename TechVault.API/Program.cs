using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TechVault.API.Auth;
using TechVault.API.Data;
using TechVault.API.Inventory;
using TechVault.API.Mapping;
using TechVault.API.Notifications;
using TechVault.API.Payments;
using TechVault.API.Repositories;
using TechVault.API.Repositories.Products;
using TechVault.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

builder.Services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAdminProductService, AdminProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IDomainUserService, DomainUserService>();
builder.Services.AddScoped<ICouponValidationService, CouponValidationService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection(StripeSettings.SectionName));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection(SmtpSettings.SectionName));
builder.Services.AddScoped<IEmailSender, MailKitEmailSender>();
builder.Services.AddScoped<IOrderNotificationService, OrderNotificationService>();
builder.Services.AddScoped<IAdminOrderService, AdminOrderService>();
builder.Services.AddScoped<IAdminStatsService, AdminStatsService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IProductReviewStatsService, ProductReviewStatsService>();
builder.Services.Configure<InventorySettings>(builder.Configuration.GetSection(InventorySettings.SectionName));
builder.Services.AddScoped<ILowStockInventoryService, LowStockInventoryService>();
builder.Services.AddHostedService<LowStockInventoryMonitorHostedService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "TechVault.Cart";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromDays(14);
});

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        // Password policy
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = false;

        // User policy
        options.User.RequireUniqueEmail = true;

        // Lockout policy
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IdentitySeeder>();

builder.Services.AddAutoMapper(typeof(ProductMappingProfile).Assembly);

const string CorsPolicyName = "Frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<IdentitySeedOptions>(builder.Configuration.GetSection(IdentitySeedOptions.SectionName));

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt settings are not configured. Copy appsettings.example.json to appsettings.json and set Jwt:Secret/Issuer/Audience/ExpiryInDays.");

if (string.IsNullOrWhiteSpace(jwtSettings.Secret))
{
    throw new InvalidOperationException("Jwt:Secret is not configured. Copy appsettings.example.json to appsettings.json and set a strong secret.");
}
if (jwtSettings.Secret.Length < 32)
{
    throw new InvalidOperationException("Jwt:Secret must be at least 32 characters for HS256.");
}
if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
{
    throw new InvalidOperationException("Jwt:Issuer is not configured.");
}
if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
{
    throw new InvalidOperationException("Jwt:Audience is not configured.");
}
if (jwtSettings.ExpiryInDays <= 0)
{
    throw new InvalidOperationException("Jwt:ExpiryInDays must be a positive integer.");
}

var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = symmetricKey,
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TechVault API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description =
            "Paste only the JWT value. Swagger sends Authorization as Bearer plus this value."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
    var seedOptions = app.Configuration.GetSection(IdentitySeedOptions.SectionName).Get<IdentitySeedOptions>()
        ?? new IdentitySeedOptions();

    await seeder.SeedAsync(seedOptions);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors(CorsPolicyName);

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
