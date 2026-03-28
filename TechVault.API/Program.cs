using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Http;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Events;
using TechVault.API.Auth;
using TechVault.API.Caching;
using TechVault.API.Data;
using TechVault.API.Health;
using TechVault.API.Errors;
using TechVault.API.Inventory;
using TechVault.API.Mapping;
using TechVault.API.Middleware;
using TechVault.API.Notifications;
using TechVault.API.Payments;
using TechVault.API.Repositories;
using TechVault.API.RateLimiting;
using TechVault.API.Repositories.Products;
using TechVault.API.Seed;
using TechVault.API.Services;
using TechVault.API.Swagger;
using TechVault.API.Validation;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting TechVault API host");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    if (context.Configuration.GetSection("Serilog").Exists())
    {
        loggerConfiguration.ReadFrom.Configuration(context.Configuration);
    }
    else
    {
        loggerConfiguration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                Path.Combine(context.HostingEnvironment.ContentRootPath, "Logs", "techvault-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                shared: true,
                outputTemplate: "{Timestamp:o} [{Level:u3}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}");
    }

    loggerConfiguration
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "TechVault.API");
});

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("application_database")
    .AddDbContextCheck<AuthDbContext>("auth_database");

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
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAdminStatsService, AdminStatsService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IProductReviewStatsService, ProductReviewStatsService>();
builder.Services.Configure<InventorySettings>(builder.Configuration.GetSection(InventorySettings.SectionName));
builder.Services.AddScoped<ILowStockInventoryService, LowStockInventoryService>();
builder.Services.AddHostedService<LowStockInventoryMonitorHostedService>();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICatalogListCache, CatalogListCache>();
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
builder.Services.AddScoped<CatalogDemoProductSeeder>();

builder.Services.AddAutoMapper(typeof(ProductMappingProfile).Assembly);

const string CorsPolicyName = "Frontend";
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
if (corsOrigins is not { Length: > 0 })
{
    corsOrigins =
    [
        "http://localhost:5173",
        "http://localhost",
        "http://127.0.0.1",
        "http://127.0.0.1:5173"
    ];
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<IdentitySeedOptions>(builder.Configuration.GetSection(IdentitySeedOptions.SectionName));
builder.Services.Configure<CatalogDemoSeedOptions>(builder.Configuration.GetSection(CatalogDemoSeedOptions.SectionName));

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

builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = false;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
        if (!context.HttpContext.Response.HasStarted)
        {
            await context.HttpContext.Response.WriteAsJsonAsync(
                new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status429TooManyRequests,
                    Message = "Too many requests. Please try again later.",
                    Errors = null
                },
                cancellationToken: cancellationToken);
        }
    };

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var path = httpContext.Request.Path;
        if (!VersionedApiRouteParser.IsVersionedApiPath(path))
        {
            return RateLimitPartition.GetNoLimiter("non-api");
        }

        var ip = ClientIpResolver.Resolve(httpContext);
        var isAuthApi = VersionedApiRouteParser.IsVersionedAuthPath(path);
        var partitionKey = $"{ip}\u001f{(isAuthApi ? "auth" : "public")}";

        if (isAuthApi)
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey,
                _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 30,
                    QueueLimit = 0,
                    Window = TimeSpan.FromMinutes(1)
                });
        }

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            });
    });
});

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation(options => options.DisableDataAnnotations = true);
builder.Services.AddValidatorsFromAssemblyContaining<FluentValidationMarker>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value is { Errors.Count: > 0 })
            .ToDictionary(
                static kvp => kvp.Key,
                static kvp => kvp.Value!.Errors
                    .Select(static e => string.IsNullOrEmpty(e.ErrorMessage) ? "The value is invalid." : e.ErrorMessage)
                    .ToArray());

        var body = new ApiErrorResponse
        {
            StatusCode = StatusCodes.Status400BadRequest,
            Message = "One or more validation errors occurred.",
            Errors = errors
        };

        return new BadRequestObjectResult(body);
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(options =>
{
    options.DocInclusionPredicate(static (documentName, apiDescription) =>
    {
        if (string.IsNullOrEmpty(apiDescription.GroupName))
        {
            return string.Equals(documentName, "v1", StringComparison.Ordinal);
        }

        return string.Equals(documentName, apiDescription.GroupName, StringComparison.Ordinal);
    });
    options.OperationFilter<SwaggerDefaultValues>();
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

ApplicationUptime.MarkStarted();

using (var scope = app.Services.CreateScope())
{
    var appDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await EnsureProductImagesTable.ExecuteAsync(appDb);

    var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
    var seedOptions = app.Configuration.GetSection(IdentitySeedOptions.SectionName).Get<IdentitySeedOptions>()
        ?? new IdentitySeedOptions();

    await seeder.SeedAsync(seedOptions);

    var catalogSeeder = scope.ServiceProvider.GetRequiredService<CatalogDemoProductSeeder>();
    var catalogOptions = app.Configuration.GetSection(CatalogDemoSeedOptions.SectionName).Get<CatalogDemoSeedOptions>()
        ?? new CatalogDemoSeedOptions();
    await catalogSeeder.SeedAsync(catalogOptions);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var descriptions = app.Services.GetRequiredService<IApiVersionDescriptionProvider>().ApiVersionDescriptions;
        foreach (var description in descriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"{description.GroupName.ToUpperInvariant()}");
        }
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath}{QueryString} -> {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set(
            "QueryString",
            httpContext.Request.QueryString.HasValue ? httpContext.Request.QueryString.Value : string.Empty);
        diagnosticContext.Set("ClientIP", ClientIpResolver.Resolve(httpContext));
        diagnosticContext.Set(
            "ContentLength",
            httpContext.Request.ContentLength is { } len ? len : (long?)null);
    };
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        var path = httpContext.Request.Path;
        if (path.StartsWithSegments("/images") || path.StartsWithSegments("/swagger"))
        {
            return LogEventLevel.Verbose;
        }

        if (ex is not null)
        {
            return LogEventLevel.Error;
        }

        if (httpContext.Response.StatusCode >= 500)
        {
            return LogEventLevel.Error;
        }

        if (httpContext.Response.StatusCode >= 400)
        {
            return LogEventLevel.Warning;
        }

        return LogEventLevel.Information;
    };
});

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors(CorsPolicyName);

app.UseSession();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
