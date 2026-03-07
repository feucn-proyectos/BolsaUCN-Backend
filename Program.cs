using backend.src.Application.Events.Implements;
using backend.src.Application.Events.Implements.Handlers;
using backend.src.Application.Events.Interfaces;
using backend.src.Application.Jobs.Implements;
using backend.src.Application.Jobs.Interfaces;
using backend.src.Application.Mappers;
using backend.src.Application.Services.Implements;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Models;
using backend.src.Infrastructure.Data;
using backend.src.Infrastructure.Repositories.Implements;
using backend.src.Infrastructure.Repositories.Interfaces;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.PostgreSql;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers; // <<-- para CORS (HeaderNames)
using Resend;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(
        new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build()
    )
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

try
{
    Log.Information("Starting web application");

    // Serilog
    builder.Host.UseSerilog(
        (context, configuration) => configuration.ReadFrom.Configuration(context.Configuration)
    );

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    #region Identity
    // =========================
    // 1) Identity
    // =========================
    builder
        .Services.AddIdentity<User, Role>(options =>
        {
            options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
        })
        .AddRoles<Role>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    #endregion

    #region Auth
    // =========================
    // 2) Auth (JWT)
    // =========================
    builder
        .Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            string? jwtSecret = builder.Configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException("La clave secreta JWT no está configurada.");
            }

            options.TokenValidationParameters =
                new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(jwtSecret)
                    ),
                    ValidateLifetime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero,
                };
        });

    #endregion
    #region CORS
    // =========================
    // 3) CORS (permitimos el front en 3000)
    // =========================
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(
            "Frontend",
            policy =>
            {
                var allowedOrigins =
                    builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ??
                    [
                        "http://localhost:3000", // Valor por defecto si no se encuentra en configuración
                    ];
                policy
                    .WithOrigins(allowedOrigins)
                    .WithHeaders(HeaderNames.ContentType, HeaderNames.Authorization, "Accept")
                    .WithExposedHeaders(HeaderNames.ContentDisposition) // para que el front pueda leer el header Content-Disposition en la respuesta (nombre del archivo al descargar CV)
                    .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
                    .AllowCredentials();
            }
        );
    });
    #endregion

    #region Resend
    // =========================
    // 4) Resend (emails)
    // =========================
    builder.Services.AddOptions();
    builder.Services.AddHttpClient<ResendClient>();
    builder.Services.Configure<ResendClientOptions>(o =>
    {
        o.ApiToken = builder.Configuration.GetValue<string>("ResendApiKey")!;
    });
    builder.Services.AddTransient<IResend, ResendClient>();

    #endregion
    #region PostgreSQL
    // =========================
    // 5) PostgreSQL
    // =========================
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    );
    #endregion

    #region Hangfire
    // =========================
    // 6) Hangfire (background jobs)
    // =========================

    Log.Information("Configurando Hangfire con almacenamiento en PostgreSQL para producción");
    builder.Services.AddHangfire(configuration =>
        configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options =>
            {
                options.UseNpgsqlConnection(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                );
            })
    );

    builder.Services.AddHangfireServer();

    #endregion


    #region DI
    // =========================
    // 7) DI (repos/services/mappers/jobs/events)
    // =========================
    // === Mappers ===
    builder.Services.AddScoped<UserMapper>();
    builder.Services.AddScoped<PublicationMapper>();
    builder.Services.AddScoped<OfferMapper>();
    builder.Services.AddScoped<BuySellMapper>();
    builder.Services.AddScoped<ApplicationMapper>();
    builder.Services.AddScoped<ProfileMapper>();
    builder.Services.AddScoped<ReviewMapper>();

    // === Repositorios ===
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();
    builder.Services.AddScoped<IOfferApplicationRepository, OfferApplicationRepository>();
    builder.Services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();
    builder.Services.AddScoped<IAdminNotificationRepository, AdminNotificationRepository>();
    builder.Services.AddScoped<IFileRepository, FileRepository>();
    builder.Services.AddScoped<IPublicationRepository, PublicationRepository>();
    builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
    builder.Services.AddScoped<ITokenRepository, TokenRepository>();
    builder.Services.AddScoped<IEventDispatcher, EventDispatcher>();

    // === Servicios ===
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IAdminService, AdminService>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IEmailDigestService, EmailDigestService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IOfferApplicationService, OfferApplicationService>();
    builder.Services.AddScoped<IPublicationService, PublicationService>();
    builder.Services.AddScoped<IReviewService, ReviewService>();
    builder.Services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();
    builder.Services.AddScoped<IFileService, FileService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IApprovalService, ApprovalService>();

    // === Jobs ===
    builder.Services.AddScoped<IUserJobs, UserJobs>();
    builder.Services.AddScoped<IOfferJobs, OfferJobs>();
    builder.Services.AddScoped<IWhitelistedTokenJobs, WhitelistedTokenJobs>();
    builder.Services.AddScoped<INotificationJobs, NotificationJobs>();

    // === Eventos y handlers ===
    builder.Services.AddScoped<
        IEventHandler<ApplicationStatusChangedEvent>,
        SendEmailOnApplicationStatusChangedHandler
    >();
    builder.Services.AddScoped<
        IEventHandler<OfferCancelledEvent>,
        SendEmailOnOfferCancelledHandler
    >();
    builder.Services.AddScoped<
        IEventHandler<InitialReviewsCreatedEvent>,
        SendEmailOnInitialReviewCreationHandler
    >();
    builder.Services.AddScoped<
        IEventHandler<PublicationStatusChangedEvent>,
        SendEmailOnPublicationStatusChangedHandler
    >();
    builder.Services.AddScoped<
        IEventHandler<PublicationClosedByAdminEvent>,
        SendEmailOnPublicationClosedByAdminHandler
    >();
    builder.Services.AddScoped<
        IEventHandler<OfferSlotsFilledEvent>,
        CloseOfferOnSlotsFilledHandler
    >();

    builder.Services.AddMapster();

    var app = builder.Build();

    #endregion
    #region Pipeline
    // =========================
    // Pipeline
    // =========================
    #endregion

    #region Hangfire Dashboard + Recurring Jobs
    // Hangfire Dashboard (solo en desarrollo)
    if (app.Environment.IsDevelopment())
    {
        app.UseHangfireDashboard();
        Log.Information("Hangfire dashboard habilitado en modo desarrollo");
    }
    else
    {
        app.UseHangfireDashboard(); // TEST
        Log.Information("Servidor en modo producción, Hangfire dashboard deshabilitado");
    }
    // === Trabajos recurrentes ===
    // User Jobs
    RecurringJob.AddOrUpdate<IUserJobs>(
        nameof(IUserJobs.DeleteUnconfirmedUserAccountsAsync),
        job => job.DeleteUnconfirmedUserAccountsAsync(),
        Cron.Weekly(DayOfWeek.Monday), // Todos los lunes a las 2 AM
        new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Santiago"),
        }
    );
    // Whitelist Jobs
    RecurringJob.AddOrUpdate<IWhitelistedTokenJobs>(
        nameof(IWhitelistedTokenJobs.DeleteExpiredTokensAsync),
        job => job.DeleteExpiredTokensAsync(),
        Cron.Daily(), // Todos los días a medianoche
        new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Santiago"),
        }
    );
    // Notification Jobs
    RecurringJob.AddOrUpdate<INotificationJobs>(
        nameof(INotificationJobs.SendUserDailyNotificationsAsync),
        job => job.SendUserDailyNotificationsAsync(),
        Cron.Daily(8), // Todos los dias a las 8 AM
        new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Santiago"),
        }
    );
    RecurringJob.AddOrUpdate<INotificationJobs>(
        nameof(INotificationJobs.SendAdminDailyNotificationsAsync),
        job => job.SendAdminDailyNotificationsAsync(),
        Cron.Daily(9), // Todos los dias a las 9 AM
        new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Santiago"),
        }
    );
    Log.Information(
        "Trabajos recurrentes de Hangfire (User, Whitelist, Notification) configurados"
    );

    #endregion
    #region Middleware
    // Middleware global de errores (antes de todo)
    app.UseMiddleware<backend.src.API.Middlewares.ErrorHandlingMiddleware.ErrorHandlingMiddleware>();
    #endregion

    // Seed DB + Mapster (al inicio)
    await SeedAndMapDatabase(app);

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        Log.Information("Swagger UI habilitado en modo desarrollo");
    }
    else
    {
        Log.Information("Servidor en modo producción, Swagger UI deshabilitado");
    }

    // Si te genera líos en local (http->https), puedes comentar mientras desarrollas:
    // app.UseHttpsRedirection();

    // CORS debe ir ANTES de auth/authorization
    app.UseCors("Frontend");

    // Muy importante: primero autenticación, luego autorización
    app.UseAuthentication();
    app.UseMiddleware<backend.src.API.Middlewares.BlacklistMiddleware>(); // Middleware para validar tokens en la whitelist
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("Aplicación iniciada correctamente");
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        Console.WriteLine("🔥 SERVIDOR ASP.NET ARRANCÓ CORRECTAMENTE 🔥");
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// =========================
// Helpers
// =========================
async Task SeedAndMapDatabase(IHost app)
{
    using var scope = app.Services.CreateScope();
    var serviceProvider = scope.ServiceProvider;
    var configuration = app.Services.GetRequiredService<IConfiguration>();

    Log.Information("Iniciando seed de base de datos y configuración de mappers");
    await DataSeeder.Initialize(configuration, serviceProvider);
    MapperExtensions.ConfigureMapster(serviceProvider);
    Log.Information("Seed de base de datos y configuración de mappers completados");
}
