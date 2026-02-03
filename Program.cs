using backend.src.Application.Infrastructure.Data;
using backend.src.Application.Mappers;
using backend.src.Application.Services.Implements;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Models;
using backend.src.Infrastructure.Data;
using backend.src.Infrastructure.Repositories.Implements;
using backend.src.Infrastructure.Repositories.Interfaces;
using Hangfire;
using Hangfire.MemoryStorage;
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
                policy
                    .WithOrigins(
                        "http://localhost:3000" // Next.js dev
                    // ,"https://localhost:3000"  // agrega si usas https en front
                    // ,"https://localhost:7129"  // agrega si llamas al backend en https y navegas desde https
                    )
                    .WithHeaders(HeaderNames.ContentType, HeaderNames.Authorization, "Accept")
                    .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
                    .AllowCredentials(); // opcional si luego usas cookies
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
    // Hangfire - usa MemoryStorage por simplicidad
    builder.Services.AddHangfire(configuration => configuration.UseMemoryStorage());
    builder.Services.AddHangfireServer();
    #endregion


    #region DI
    // =========================
    // 6) DI (repos/services/mappers)
    // =========================
    builder.Services.AddScoped<UserMapper>();
    builder.Services.AddScoped<PublicationMapper>();
    builder.Services.AddScoped<OfferMapper>();
    builder.Services.AddScoped<BuySellMapper>();
    builder.Services.AddScoped<ApplicationMapper>();
    builder.Services.AddScoped<ProfileMapper>();

    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IOfferRepository, OfferRepository>();
    builder.Services.AddScoped<IBuySellRepository, BuySellRepository>();
    builder.Services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();
    builder.Services.AddScoped<IOfferApplicationRepository, OfferApplicationRepository>();
    builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
    builder.Services.AddScoped<IAdminNotificationRepository, AdminNotificationRepository>();
    builder.Services.AddScoped<IFileRepository, FileRepository>();
    builder.Services.AddScoped<IPublicationRepository, PublicationRepository>();
    builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
    builder.Services.AddScoped<ITokenRepository, TokenRepository>();

    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IAdminService, AdminService>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IOfferService, OfferService>();
    builder.Services.AddScoped<IOfferApplicationService, OfferApplicationService>();
    builder.Services.AddScoped<IPublicationService, PublicationService>();
    builder.Services.AddScoped<IBuySellService, BuySellService>();
    builder.Services.AddScoped<IReviewService, ReviewService>();
    builder.Services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();
    builder.Services.AddScoped<IFileService, FileService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IApprovalService, ApprovalService>();

    builder.Services.AddMapster();

    var app = builder.Build();

    #endregion
    #region Pipeline
    // =========================
    // Pipeline
    // =========================
    #endregion

    #region Hangfire Dashboard + Recurring Jobs
    // Hangfire dashboard (solo en desarrollo)
    if (app.Environment.IsDevelopment())
    {
        app.UseHangfireDashboard();
        // Registrar job recurrente cada hora para cerrar reviews vencidas
        RecurringJob.AddOrUpdate<IReviewService>(
            "CloseExpiredReviews",
            service => service.CloseExpiredReviewsAsync(),
            Cron.Hourly
        );
        Log.Information(
            "Hangfire dashboard habilitado y job recurrente para cierre de reviews programado. Servidor en: http://localhost:5185/hangfire"
        );
    }

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

    // Si te genera líos en local (http->https), puedes comentar mientras desarrollas:
    // app.UseHttpsRedirection();

    // CORS debe ir ANTES de auth/authorization
    app.UseCors("Frontend");

    // Muy importante: primero autenticación, luego autorización
    app.UseAuthentication();
    app.UseMiddleware<backend.src.API.Middlewares.BlacklistMiddleware>(); // Middleware para validar tokens en blacklist debe ir entre auth y authorization
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
