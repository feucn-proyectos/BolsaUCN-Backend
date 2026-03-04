using backend.src.Application.Jobs.Interfaces;
using backend.src.Domain.Constants;
using backend.src.Domain.Models;
using backend.src.Infrastructure.Data;
using Bogus;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace backend.src.Application.Infrastructure.Data
{
    public class DataSeeder
    {
        public static async Task Initialize(
            IConfiguration configuration,
            IServiceProvider serviceProvider
        )
        {
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            var backgroundJobs = serviceProvider.GetRequiredService<IOfferJobs>();
            try
            {
                var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

                Log.Information("DataSeeder: Iniciando la migración de la base de datos...");
                await context.Database.MigrateAsync();
                Log.Information("DataSeeder: Migración de la base de datos completada.");

                if (!await context.Roles.AnyAsync())
                {
                    Log.Information("DataSeeder: No se encontraron roles, creando roles...");
                    var roles = new List<Role>
                    {
                        new Role
                        {
                            Name = RoleNames.Admin,
                            NormalizedName = RoleNames.Admin.ToUpper(),
                        },
                        new Role
                        {
                            Name = RoleNames.Applicant,
                            NormalizedName = RoleNames.Applicant.ToUpper(),
                        },
                        new Role
                        {
                            Name = RoleNames.Offeror,
                            NormalizedName = RoleNames.Offeror.ToUpper(),
                        },
                        new Role
                        {
                            Name = RoleNames.SuperAdmin,
                            NormalizedName = RoleNames.SuperAdmin.ToUpper(),
                        },
                    };

                    foreach (var role in roles)
                    {
                        await roleManager.CreateAsync(role);
                    }
                    Log.Information("DataSeeder: Roles creados exitosamente.");
                }

                if (!await context.Users.AnyAsync())
                {
                    Log.Information(
                        "DataSeeder: No se encontraron usuarios, creando usuarios de prueba..."
                    );
                    await SeedUsers(userManager, context, configuration);
                    Log.Information("DataSeeder: Usuarios de prueba creados exitosamente.");
                }

                if (!await context.Offers.AnyAsync())
                {
                    Log.Information(
                        "DataSeeder: No se encontraron ofertas, creando ofertas de prueba..."
                    );
                    await SeedOffers(context, backgroundJobs, userManager);
                    Log.Information("DataSeeder: Ofertas de prueba creadas exitosamente.");
                }
                if (!await context.BuySells.AnyAsync())
                {
                    Log.Information(
                        "DataSeeder: No hay avisos de compra/venta, creando datos de prueba..."
                    );
                    await SeedBuySells(context, userManager);
                    Log.Information("DataSeeder: Compra/venta de prueba creados.");
                }
                if (!await context.JobApplications.AnyAsync())
                {
                    Log.Information(
                        "DataSeeder: No se encontraron postulaciones, creando postulaciones de prueba..."
                    );
                    await SeedJobApplications(context, userManager);
                    Log.Information("DataSeeder: Postulaciones de prueba creadas exitosamente.");
                }
                if (!await context.Reviews.AnyAsync())
                {
                    Log.Information(
                        "DataSeeder: No se encontraron reviews, creando reviews de prueba..."
                    );
                    await SeedReviews(context, userManager);
                    Log.Information("DataSeeder: Reviews de prueba creadas exitosamente.");
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "DataSeeder: Error al inicializar la base de datos.");
                throw;
            }
        }

        private static async Task SeedUsers(
            UserManager<User> userManager,
            AppDbContext context,
            IConfiguration configuration
        )
        {
            var faker = new Faker("es");

            // ========================================
            // USUARIOS DE PRUEBA CON CREDENCIALES FÁCILES
            // ========================================
            Log.Information("DataSeeder: Creando usuarios de prueba con credenciales fáciles...");

            // 1. ESTUDIANTE DE PRUEBA

            var testStudentUser1 = new User
            {
                UserName = "estudiante",
                Email = "estudiante@alumnos.ucn.cl",
                PhoneNumber = "+56912345678",
                UserType = UserType.Estudiante,
                Rut = "12345678-9",
                EmailConfirmed = true,
                IsBlocked = false,
                Rating = 3.3f,
                FirstName = "Juan",
                LastName = "Pérez Estudiante",
                Disability = Disability.Ninguna,
                CVId = null,
                ProfilePhoto = new UserImage
                {
                    Url =
                        configuration.GetValue<string>("Images:DefaultUserImageUrl")
                        ?? throw new InvalidOperationException(
                            "DefaultUserImageUrl no está configurado"
                        ),
                    PublicId =
                        configuration.GetValue<string>("Images:DefaultUserImagePublicId")
                        ?? throw new InvalidOperationException(
                            "DefaultUserImagePublicId no está configurado"
                        ),
                },
            };
            var studentResult1 = await userManager.CreateAsync(testStudentUser1, "Test123!");
            if (studentResult1.Succeeded)
            {
                await userManager.AddToRolesAsync(
                    testStudentUser1,
                    [RoleNames.Applicant, RoleNames.Offeror]
                );
                Log.Information(
                    "✅ Usuario estudiante creado: estudiante@alumnos.ucn.cl / Test123!"
                );
            }

            // ESTUDIANTE CON REVIEWS PENDIENTES
            var testStudentUser2 = new User
            {
                UserName = "estudiante2",
                Email = "estudiante2@alumnos.ucn.cl",
                PhoneNumber = "+56923456789",
                UserType = UserType.Estudiante,
                AboutMe = "Estudiante con varias evaluaciones pendientes",
                Rut = "22334455-6",
                EmailConfirmed = true,
                IsBlocked = false,
                Rating = 0.0f,
                FirstName = "Pedro",
                LastName = "López Morales",
                Disability = Disability.Ninguna,
                CVId = null,
                ProfilePhoto = new UserImage
                {
                    Url =
                        configuration.GetValue<string>("Images:DefaultUserImageUrl")
                        ?? throw new InvalidOperationException(
                            "DefaultUserImageUrl no está configurado"
                        ),
                    PublicId =
                        configuration.GetValue<string>("Images:DefaultUserImagePublicId")
                        ?? throw new InvalidOperationException(
                            "DefaultUserImagePublicId no está configurado"
                        ),
                },
            };
            var studentResult2 = await userManager.CreateAsync(testStudentUser2, "Test123!");
            if (studentResult2.Succeeded)
            {
                await userManager.AddToRolesAsync(
                    testStudentUser2,
                    [RoleNames.Applicant, RoleNames.Offeror]
                );
                Log.Information(
                    "✅ Usuario estudiante creado: estudiante2@alumnos.ucn.cl / Test123!"
                );
            }

            // 2. EMPRESA DE PRUEBA
            var testCompanyUser = new User
            {
                UserName = "empresa",
                Email = "empresa@techcorp.cl",
                PhoneNumber = "+56987654321",
                UserType = UserType.Empresa,
                AboutMe = "Empresa comprometida con el cambio y progreso de sus trabajadores",
                Rut = "76543210-K",
                EmailConfirmed = true,
                Rating = 5.4f,
                IsBlocked = false,
                FirstName = "Tech Corp SpA",
                LastName = "Tecnología Corporativa SpA",
                ProfilePhoto = new UserImage
                {
                    Url =
                        configuration.GetValue<string>("Images:DefaultUserImageUrl")
                        ?? throw new InvalidOperationException(
                            "DefaultUserImageUrl no está configurado"
                        ),
                    PublicId =
                        configuration.GetValue<string>("Images:DefaultUserImagePublicId")
                        ?? throw new InvalidOperationException(
                            "DefaultUserImagePublicId no está configurado"
                        ),
                },
            };
            var companyResult = await userManager.CreateAsync(testCompanyUser, "Test123!");
            if (companyResult.Succeeded)
            {
                await userManager.AddToRoleAsync(testCompanyUser, RoleNames.Offeror);
                Log.Information("✅ Usuario empresa creado: empresa@techcorp.cl / Test123!");
            }

            // 3. PARTICULAR DE PRUEBA
            var testIndividualUser = new User
            {
                UserName = "particular",
                Email = "particular@ucn.cl",
                PhoneNumber = "+56955555555",
                UserType = UserType.Particular,
                AboutMe = "Emprendedor con 5 años de experiencia",
                Rut = "11222333-4",
                EmailConfirmed = true,
                Rating = 6.0f,
                IsBlocked = false,
                FirstName = "María",
                LastName = "González Particular",
                ProfilePhoto = new UserImage
                {
                    Url =
                        configuration.GetValue<string>("Images:DefaultUserImageUrl")
                        ?? throw new InvalidOperationException(
                            "DefaultUserImageUrl no está configurado"
                        ),
                    PublicId =
                        configuration.GetValue<string>("Images:DefaultUserImagePublicId")
                        ?? throw new InvalidOperationException(
                            "DefaultUserImagePublicId no está configurado"
                        ),
                },
            };
            var individualResult = await userManager.CreateAsync(testIndividualUser, "Test123!");
            if (individualResult.Succeeded)
            {
                await userManager.AddToRoleAsync(testIndividualUser, RoleNames.Offeror);
                Log.Information("✅ Usuario particular creado: particular@ucn.cl / Test123!");
            }

            // 4. ADMIN DE PRUEBA
            var testAdminUser = new User
            {
                UserName = "admin",
                Email = "admin@ucn.cl",
                PhoneNumber = "+56911111111",
                UserType = UserType.Administrador,
                AboutMe = "Administrador del sistema BolsaUcn",
                Rut = "99888777-6",
                EmailConfirmed = true,
                IsBlocked = false,
                FirstName = "Administrador",
                LastName = "Sistema",
                ProfilePhoto = new UserImage
                {
                    Url =
                        configuration.GetValue<string>("Images:DefaultUserImageUrl")
                        ?? throw new InvalidOperationException(
                            "DefaultUserImageUrl no está configurado"
                        ),
                    PublicId =
                        configuration.GetValue<string>("Images:DefaultUserImagePublicId")
                        ?? throw new InvalidOperationException(
                            "DefaultUserImagePublicId no está configurado"
                        ),
                },
            };
            var adminResult = await userManager.CreateAsync(testAdminUser, "Test123!");
            if (adminResult.Succeeded)
            {
                await userManager.AddToRolesAsync(
                    testAdminUser,
                    [RoleNames.Admin, RoleNames.SuperAdmin, RoleNames.Offeror]
                );
                Log.Information("✅ Usuario admin creado: admin@ucn.cl / Test123!");
            }

            Log.Information("DataSeeder: Usuarios de prueba creados exitosamente.");
            Log.Information("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Log.Information("📋 CREDENCIALES DE PRUEBA:");
            Log.Information("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Log.Information("👨‍🎓 ESTUDIANTE:");
            Log.Information("   Email: estudiante@alumnos.ucn.cl");
            Log.Information("   Pass:  Test123!");
            Log.Information("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Log.Information("👨‍🎓 ESTUDIANTE 2 (CON +3 REVIEWS PENDIENTES):");
            Log.Information("   Email: estudiante2@alumnos.ucn.cl");
            Log.Information("   Pass:  Test123!");
            Log.Information("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Log.Information("🏢 EMPRESA:");
            Log.Information("   Email: empresa@techcorp.cl");
            Log.Information("   Pass:  Test123!");
            Log.Information("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Log.Information("👤 PARTICULAR:");
            Log.Information("   Email: particular@ucn.cl");
            Log.Information("   Pass:  Test123!");
            Log.Information("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Log.Information("👑 ADMIN:");
            Log.Information("   Email: admin@ucn.cl");
            Log.Information("   Pass:  Test123!");
            Log.Information("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            // ========================================
            // USUARIOS ALEATORIOS ADICIONALES (Faker)
            // ========================================
            Log.Information("DataSeeder: Creando usuarios aleatorios adicionales...");

            // Seed Random Students
            for (int i = 0; i < 30; i++)
            {
                var studentUser = new User
                {
                    UserName = faker.Internet.UserName(),
                    Email = faker.Internet.Email(),
                    PhoneNumber = faker.Phone.PhoneNumber("+569########"),
                    UserType = UserType.Estudiante,
                    AboutMe = faker.Random.Replace("##################"),
                    Rut = faker.Random.Replace("##.###.###-K"),
                    EmailConfirmed = true,
                    Rating = MathF.Round(faker.Random.Float(1.0f, 6.0f), 1),
                    IsBlocked = faker.Random.Bool(0.3f),
                    FirstName = faker.Name.FirstName(),
                    LastName = faker.Name.LastName(),
                    Disability = faker.PickRandom<Disability>(),
                    ProfilePhoto = new UserImage
                    {
                        Url =
                            configuration.GetValue<string>("Images:DefaultUserImageUrl")
                            ?? throw new InvalidOperationException(
                                "DefaultUserImageUrl no está configurado"
                            ),
                        PublicId =
                            configuration.GetValue<string>("Images:DefaultUserImagePublicId")
                            ?? throw new InvalidOperationException(
                                "DefaultUserImagePublicId no está configurado"
                            ),
                    },
                };
                var result = await userManager.CreateAsync(studentUser, "Password123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(
                        studentUser,
                        [RoleNames.Applicant, RoleNames.Offeror]
                    );
                }
            }

            // Seed Companies
            for (int i = 0; i < 20; i++)
            {
                var companyUser = new User
                {
                    UserName = faker.Internet.UserName(),
                    Email = faker.Internet.Email(),
                    UserType = UserType.Empresa,
                    AboutMe = faker.Random.Replace("##################"),
                    Rut = faker.Random.Replace("##.###.###-K"),
                    EmailConfirmed = true,
                    Rating = MathF.Round(faker.Random.Float(1.0f, 6.0f), 1),
                    IsBlocked = faker.Random.Bool(0.3f),
                    FirstName = faker.Company.CompanyName(),
                    LastName = faker.Company.CompanyName() + " S.A.",
                    ProfilePhoto = new UserImage
                    {
                        Url =
                            configuration.GetValue<string>("Images:DefaultUserImageUrl")
                            ?? throw new InvalidOperationException(
                                "DefaultUserImageUrl no está configurado"
                            ),
                        PublicId =
                            configuration.GetValue<string>("Images:DefaultUserImagePublicId")
                            ?? throw new InvalidOperationException(
                                "DefaultUserImagePublicId no está configurado"
                            ),
                    },
                };
                var result = await userManager.CreateAsync(companyUser, "Password123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(companyUser, RoleNames.Offeror);
                }
            }

            // Seed Individual
            var randomIndividualUser = new User
            {
                UserName = faker.Internet.UserName(),
                Email = faker.Internet.Email(),
                UserType = UserType.Particular,
                Rut = faker.Random.Replace("##.###.###-K"),
                AboutMe = faker.Random.Replace("################"),
                EmailConfirmed = true,
                Rating = MathF.Round(faker.Random.Float(1.0f, 6.0f), 1),
                IsBlocked = faker.Random.Bool(0.9f),
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                ProfilePhoto = new UserImage
                {
                    Url =
                        configuration.GetValue<string>("Images:DefaultUserImageUrl")
                        ?? throw new InvalidOperationException(
                            "DefaultUserImageUrl no está configurado"
                        ),
                    PublicId =
                        configuration.GetValue<string>("Images:DefaultUserImagePublicId")
                        ?? throw new InvalidOperationException(
                            "DefaultUserImagePublicId no está configurado"
                        ),
                },
            };
            var randomIndividualResult = await userManager.CreateAsync(
                randomIndividualUser,
                "Password123!"
            );
            if (randomIndividualResult.Succeeded)
            {
                await userManager.AddToRoleAsync(randomIndividualUser, RoleNames.Offeror);
            }

            await context.SaveChangesAsync();
            Log.Information("DataSeeder: Todos los usuarios creados exitosamente.");
        }

        private static async Task SeedOffers(
            AppDbContext context,
            IOfferJobs backgroundJobs,
            UserManager<User> userManager
        )
        {
            var offerents = await userManager.GetUsersInRoleAsync(RoleNames.Offeror);

            if (offerents.Count == 0)
                return;

            var now = DateTime.UtcNow;

            // Muestras curadas (todo en castellano y con campos útiles)
            var samples = new[]
            {
                new
                {
                    Title = "Apoyo en Feria UCN",
                    Desc = "Logística de stands, orientación a asistentes y apoyo en acreditación.",
                    Rem = 55000,
                    Slots = 5,
                    Type = OfferTypes.Trabajo,
                    Loc = "Campus Antofagasta",
                    Req = "Responsable, trato cordial, disponibilidad el sábado.",
                    Contact = "feucn@ucn.cl",
                    IsCv = false,
                    Deadline = now.AddDays(7),
                    End = now.AddDays(10),
                },
                new
                {
                    Title = "Diseño de flyer (freelance)",
                    Desc = "Diseño de pieza gráfica en formato A4 y versión para RRSS.",
                    Rem = 40000,
                    Slots = 1,
                    Type = OfferTypes.Trabajo,
                    Loc = "Remoto",
                    Req = "Portafolio o muestras previas; entrega en 48h.",
                    Contact = "deportes@ucn.cl",
                    IsCv = false,
                    Deadline = now.AddDays(5),
                    End = now.AddDays(7),
                },
                new
                {
                    Title = "Tutorías de Cálculo I",
                    Desc = "Tutorías grupales (máx. 8) dos veces por semana durante 1 mes.",
                    Rem = 0,
                    Slots = 2,
                    Type = OfferTypes.Voluntariado,
                    Loc = "Campus Coquimbo",
                    Req = "Aprobado Cálculo I/II, ganas de explicar.",
                    Contact = "centro.estudiantes@ucn.cl",
                    IsCv = false,
                    Deadline = now.AddDays(9),
                    End = now.AddDays(30),
                },
                new
                {
                    Title = "Community Manager para evento",
                    Desc = "Cobertura en vivo y publicaciones previas del evento (1 semana).",
                    Rem = 80000,
                    Slots = 1,
                    Type = OfferTypes.Trabajo,
                    Loc = "Remoto / Híbrido",
                    Req = "Manejo de IG y TikTok; redacción básica.",
                    Contact = "comunicaciones@ucn.cl",
                    IsCv = false,
                    Deadline = now.AddDays(6),
                    End = now.AddDays(12),
                },
                new
                {
                    Title = "Asistente de Laboratorio (química)",
                    Desc = "Apoyo en preparación de materiales y registro de datos.",
                    Rem = 120000,
                    Slots = 1,
                    Type = OfferTypes.Trabajo,
                    Loc = "Campus Antofagasta",
                    Req = "Ramos básicos aprobados; EPP al día.",
                    Contact = "lab.quimica@ucn.cl",
                    IsCv = false,
                    Deadline = now.AddDays(10),
                    End = now.AddDays(20),
                },
                new
                {
                    Title = "Mentorías a mechones (Programa Bienestar)",
                    Desc = "Acompañamiento y resolución de dudas generales 1 vez por semana.",
                    Rem = 0,
                    Slots = 3,
                    Type = OfferTypes.Voluntariado,
                    Loc = "Campus Coquimbo",
                    Req = "Segundo año o superior; empatía y responsabilidad.",
                    Contact = "bienestar@ucn.cl",
                    IsCv = false,
                    Deadline = now.AddDays(8),
                    End = now.AddDays(40),
                },
            };

            context.Offers.RemoveRange(context.Offers);
            await context.SaveChangesAsync();

            int i = 0;
            foreach (var s in samples)
            {
                var owner = offerents[i++ % offerents.Count];

                var offer = new Offer
                {
                    UserId = owner.Id,
                    User = owner,

                    Title = s.Title,
                    Description = s.Desc,
                    CreatedAt = now.AddDays(-i % 3), // algunas “recientes”
                    PublicationType = PublicationType.Oferta,
                    ApprovalStatus = ApprovalStatus.Aceptada,

                    EndDate = s.End.ToUniversalTime(),
                    ApplicationDeadline = s.Deadline.ToUniversalTime(),
                    ReviewDeadline = s.End.AddDays(14).ToUniversalTime(), // Hardcoded a 14 dias como ejemplo para pruebas.
                    Remuneration = s.Rem,
                    AvailableSlots = s.Slots,
                    OfferType = s.Type,
                    Location = s.Loc,
                    AdditionalContactEmail = s.Contact,
                    IsCvRequired = s.IsCv,
                };

                context.Offers.Add(offer);
            }

            // <<< INICIO: OFERTA "INPROCESS" SOLICITADA >>>
            var firstOfferent = offerents.First();
            var inProcessOffer = new Offer
            {
                UserId = firstOfferent.Id,
                User = firstOfferent,

                Title = "Práctica Desarrollo .NET (En Revisión)",
                Description =
                    "Se busca estudiante para práctica de 3 meses en desarrollo backend con .NET y Azure. El postulante debe estar en último año. Esta oferta está pendiente de aprobación por la DGE.",
                CreatedAt = now.AddDays(-1),
                PublicationType = PublicationType.Oferta,
                ApprovalStatus = ApprovalStatus.Pendiente, // <- Estado solicitado

                EndDate = now.AddMonths(3),
                ApplicationDeadline = now.AddDays(14),
                ReviewDeadline = now.AddMonths(3).AddDays(14), // Hardcoded a 14 dias despues del end date para pruebas.
                Remuneration = 400000, // Remuneración de práctica
                OfferType = OfferTypes.Trabajo, // Asumiendo que Práctica es un tipo de Trabajo
                Location = "Remoto (Chile)",
                AdditionalContactEmail = "rrhh.pending@techcorp.cl",
                IsCvRequired = true,
            };
            context.Offers.Add(inProcessOffer);

            // Ofertas aleatorias
            var faker = new Faker("es");
            var randomOffersCount = 15;
            Log.Information(
                $"DataSeeder: Creando {randomOffersCount} ofertas aleatorias con Faker..."
            );

            for (int k = 0; k < randomOffersCount; k++)
            {
                var owner = offerents[k % offerents.Count];
                var isVolunteer = faker.Random.Bool(0.3f);
                var offerType = isVolunteer ? OfferTypes.Voluntariado : OfferTypes.Trabajo;
                var remuneration = isVolunteer ? 0 : faker.Random.Int(50000, 500000);
                var isCvRequired = faker.Random.Bool(0.7f);
                var location = faker.PickRandom(
                    "Remoto",
                    "Campus Antofagasta",
                    "Campus Coquimbo",
                    "Híbrido"
                );
                var nowForFaker = DateTime.UtcNow;

                // Fechas aleatorias en el futuro
                var daysSincePost = faker.Random.Int(1, 10);
                var daysUntilDeadline = faker.Random.Int(3, 30);
                var daysUntilEnd = faker.Random.Int(daysUntilDeadline + 7, daysUntilDeadline + 90);
                var deadlineDate = nowForFaker.AddDays(daysUntilDeadline);
                var endDate = nowForFaker.AddDays(daysUntilEnd);
                var publicationDate = nowForFaker.AddDays(-daysSincePost);

                var status = ApprovalStatus.Aceptada;

                if (faker.Random.Bool(0.5f))
                {
                    status = ApprovalStatus.Pendiente;
                }

                if (faker.Random.Bool(0.15f))
                {
                    status = ApprovalStatus.Rechazada;
                }

                var offer = new Offer
                {
                    UserId = owner.Id,
                    User = owner,

                    Title = faker.Name.JobTitle(),
                    Description = faker.Lorem.Paragraph(3),
                    CreatedAt = publicationDate,
                    PublicationType = PublicationType.Oferta,
                    ApprovalStatus = status,

                    EndDate = endDate,
                    ApplicationDeadline = deadlineDate,
                    ReviewDeadline = endDate.AddDays(14), // Hardcoded a 14 dias despues del end date para pruebas.
                    Remuneration = remuneration,
                    OfferType = offerType,
                    Location = location,
                    AdditionalContactEmail = faker.Internet.Email(),
                    IsCvRequired = isCvRequired,
                };

                context.Offers.Add(offer);
            }

            await context.SaveChangesAsync();
            await ScheduleLifetimeTrackingJobs(context, backgroundJobs);

            Log.Information(
                "DataSeeder: Ofertas de ejemplo cargadas ({Count})",
                samples.Length + randomOffersCount + 1
            );
        }

        private static async Task ScheduleLifetimeTrackingJobs(
            AppDbContext context,
            IOfferJobs backgroundJobs
        )
        {
            var activeOffers = await context
                .Offers.Where(o => o.ApprovalStatus != ApprovalStatus.Rechazada)
                .ToListAsync();

            foreach (var offer in activeOffers)
            {
                var closeApplicationsId = BackgroundJob.Schedule(
                    () => backgroundJobs.SetAsCloseForApplicationsAsync(offer.Id),
                    offer.ApplicationDeadline
                );
                var completeAndInitReviewsId = BackgroundJob.Schedule(
                    () => backgroundJobs.SetAsCompleteAndInitializeReviewsAsync(offer.Id),
                    offer.EndDate
                );
                var finalizeAndCloseReviewsId = BackgroundJob.Schedule(
                    () => backgroundJobs.SetAsFinalizedAndCloseReviewsAsync(offer.Id),
                    offer.ReviewDeadline // Hardcoded a 14 dias, solo como ejemplo para pruebas.
                );
                offer.CloseApplicationsJobId = closeApplicationsId;
                offer.FinishWorkAndInitializeReviewsJobId = completeAndInitReviewsId;
                offer.FinalizeAndCloseReviewsJobId = finalizeAndCloseReviewsId;
            }
            await context.SaveChangesAsync();
            Log.Information(
                "DataSeeder: Trabajos programados para seguimiento de ciclo de vida de ofertas."
            );
        }

        private static async Task SeedBuySells(AppDbContext context, UserManager<User> userManager)
        {
            var now = DateTime.UtcNow;

            // Buscamos oferentes (empresa o particular) para asociar publicaciones
            var sellers = await userManager.GetUsersInRoleAsync(RoleNames.Offeror);
            if (sellers.Count == 0)
                return;

            var sellersCount = sellers.Count;
            if (sellersCount == 0)
                return;

            // Muestras curadas (campos útiles y en castellano)
            var items = new[]
            {
                new
                {
                    Title = "Venta libro Cálculo I (Stewart 7ma)",
                    Desc = "En buen estado, pocas marcas.",
                    Price = 12000,
                    Category = Category.Libros,
                    Loc = "Antofagasta",
                    Contact = "ignacio@ucn.cl",
                },
                new
                {
                    Title = "Teclado mecánico Redragon K552",
                    Desc = "Switch blue, 1 año de uso.",
                    Price = 18000,
                    Category = Category.Electronica,
                    Loc = "Coquimbo",
                    Contact = "+56987654321",
                },
                new
                {
                    Title = "Bata laboratorio talla M",
                    Desc = "Lavada y desinfectada, casi nueva.",
                    Price = 8000,
                    Category = Category.Ropa,
                    Loc = "Antofagasta",
                    Contact = "c.labs@ucn.cl",
                },
                new
                {
                    Title = "Calculadora científica Casio fx-82",
                    Desc = "Funciona perfecto, con pilas nuevas.",
                    Price = 9000,
                    Category = Category.Electronica,
                    Loc = "Remoto",
                    Contact = "ventas@ucn.cl",
                },
                new
                {
                    Title = "Pack cuadernos + destacadores",
                    Desc = "5 cuadernos college + 6 destacadores.",
                    Price = 6000,
                    Category = Category.Otros,
                    Loc = "Coquimbo",
                    Contact = "j.vende@ucn.cl",
                },
            };

            int i = 0;
            foreach (var it in items)
            {
                var owner = sellers[i++ % sellers.Count];

                var bs = new BuySell
                {
                    UserId = owner.Id,
                    User = owner,
                    Title = it.Title,
                    Description = it.Desc,
                    CreatedAt = now.AddDays(-(i % 3)),
                    PublicationType = PublicationType.CompraVenta,
                    ApprovalStatus = ApprovalStatus.Aceptada,

                    Price = it.Price,
                    Quantity = 1,
                    Availability = Availability.Disponible,
                    Condition = Condition.Nuevo,
                    Category = it.Category,
                    Location = it.Loc,
                    IsEmailAvailable = true,
                    IsPhoneAvailable = false,
                    AdditionalContactEmail = it.Contact,
                };

                context.BuySells.Add(bs);
            }

            // <<< INICIO: BUYSELL "INPROCESS" SOLICITADO >>>
            var firstSeller = sellers.First();
            var inProcessBuySell = new BuySell
            {
                UserId = firstSeller.Id,
                User = firstSeller,
                Title = "Venta de apuntes",
                Description =
                    "Vendo todos mis apuntes de primer año de ing. civil. Están en PDF. El admin debe revisar que no sea material con copyright.",
                CreatedAt = now.AddDays(-1),
                PublicationType = PublicationType.CompraVenta,
                ApprovalStatus = ApprovalStatus.Pendiente, // <- Estado solicitado

                Price = 15000,
                Quantity = 1,
                Availability = Availability.Disponible,
                Condition = Condition.Nuevo,
                Category = Category.Otros,
                Location = "Digital (PDF)",
                IsEmailAvailable = true,
                IsPhoneAvailable = false,
                AdditionalContactEmail = "apuntes.pendientes@ucn.cl",
            };
            context.BuySells.Add(inProcessBuySell);

            await context.SaveChangesAsync();
            Log.Information("DataSeeder: BuySell de ejemplo cargados ({Count})", items.Length + 1);

            var faker = new Faker("es");
            var randomBuySellsCount = 50;

            var locations = new[] { "Antofagasta", "Digital", "UCN Campus" };
            Log.Information(
                $"DataSeeder: Creando {randomBuySellsCount} publicaciones de compra/venta aleatorias con Faker..."
            );

            for (int k = 0; k < randomBuySellsCount; k++)
            {
                var owner = sellers[k % sellersCount];
                var nowForFaker = DateTime.UtcNow;
                var category = faker.PickRandom(
                    Category.Electronica,
                    Category.Ropa,
                    Category.Hogar,
                    Category.Vehiculos,
                    Category.Deportes,
                    Category.Libros,
                    Category.Musica,
                    Category.Juguetes,
                    Category.Mascotas,
                    Category.Servicios,
                    Category.Otros
                );

                var title =
                    category == Category.Servicios
                        ? $"Servicio de {faker.Commerce.ProductName().ToLower()} (Freelance)"
                        : $"{category}: {faker.Commerce.ProductName()}";

                var availability = Availability.Disponible;
                var status = ApprovalStatus.Aceptada;

                if (faker.Random.Bool(0.5f))
                {
                    status = ApprovalStatus.Pendiente;
                }

                if (faker.Random.Bool(0.15f))
                {
                    status = ApprovalStatus.Rechazada;
                }

                var bs = new BuySell
                {
                    UserId = owner.Id,
                    User = owner,
                    Title = title,
                    Description =
                        faker.Commerce.ProductDescription() + ". " + faker.Lorem.Sentence(5),
                    CreatedAt = nowForFaker.AddDays(-faker.Random.Int(1, 20)),
                    PublicationType = PublicationType.CompraVenta,
                    ApprovalStatus = status,

                    Price = faker.Random.Int(5000, 100000),
                    Quantity = 1,
                    Availability = availability,
                    Condition = Condition.Nuevo,
                    Category = category,
                    Location = faker.PickRandom(locations),
                    IsEmailAvailable = faker.Random.Bool(0.8f),
                    IsPhoneAvailable = faker.Random.Bool(0.5f),
                    AdditionalContactEmail = faker.Random.Bool(0.7f)
                        ? faker.Phone.PhoneNumber("+569########")
                        : faker.Internet.Email(),
                };

                context.BuySells.Add(bs);
            }
        }

        private static async Task SeedJobApplications(
            AppDbContext context,
            UserManager<User> userManager
        )
        {
            var studentUser = await context.Users.FirstOrDefaultAsync(u =>
                u.Email == "estudiante@alumnos.ucn.cl"
            );

            // Obtener ofertas publicadas y activas que aún no expiran (necesarias para postular)
            var offers = await context
                .Offers.Include(o => o.User)
                .Where(o =>
                    o.ApprovalStatus == ApprovalStatus.Aceptada
                    && o.ApplicationDeadline > DateTime.UtcNow
                    && o.PublicationType == PublicationType.Oferta
                )
                .ToListAsync();

            // Obtener todos los estudiantes para postular aleatoriamente
            var allStudents = await userManager.GetUsersInRoleAsync(RoleNames.Applicant);

            if (offers.Count < 5 || allStudents.Count == 0 || studentUser == null)
            {
                Log.Warning(
                    "DataSeeder: No se pueden crear postulaciones suficientes (necesita 5 ofertas, 1 estudiante de prueba y otros)."
                );
                // Devuelve si faltan datos críticos para las 5 postulaciones fijas
                if (studentUser == null || offers.Count < 5)
                    return;
            }

            var studentId = studentUser.Id;
            var applications = new List<JobApplication>();
            var faker = new Faker("es");

            // 1. Postulaciones del estudiante de prueba (las 5 originales)
            // Se asume que las primeras 5 ofertas en la lista 'offers' son las que se usaban antes
            var offersForTestStudent = offers.Take(5).ToList();

            applications.AddRange(
                new List<JobApplication>
                {
                    new JobApplication
                    {
                        StudentId = studentId,
                        Student = studentUser,
                        JobOfferId = offersForTestStudent[4].Id, // offers[4]
                        JobOffer = offersForTestStudent[4],
                        Status = ApplicationStatus.Pendiente,
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                    },
                    new JobApplication
                    {
                        StudentId = studentId,
                        Student = studentUser,
                        JobOfferId = offersForTestStudent[3].Id, // offers[3]
                        JobOffer = offersForTestStudent[3],
                        Status = ApplicationStatus.Pendiente,
                        CreatedAt = DateTime.UtcNow.AddDays(-7),
                    },
                    new JobApplication
                    {
                        StudentId = studentId,
                        Student = studentUser,
                        JobOfferId = offersForTestStudent[0].Id, // offers[0]
                        JobOffer = offersForTestStudent[0],
                        Status = ApplicationStatus.Pendiente,
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                    },
                    new JobApplication
                    {
                        StudentId = studentId,
                        Student = studentUser,
                        JobOfferId = offersForTestStudent[1].Id, // offers[1]
                        JobOffer = offersForTestStudent[1],
                        Status = ApplicationStatus.Pendiente,
                        CreatedAt = DateTime.UtcNow.AddDays(-3),
                    },
                    new JobApplication
                    {
                        StudentId = studentId,
                        Student = studentUser,
                        JobOfferId = offersForTestStudent[2].Id, // offers[2]
                        JobOffer = offersForTestStudent[2],
                        Status = ApplicationStatus.Pendiente,
                        CreatedAt = DateTime.UtcNow.AddDays(-1),
                    },
                }
            );

            var maxPossibleApplications = offers.Count * allStudents.Count;

            var randomApplicationsCount = Math.Min(
                100,
                maxPossibleApplications - applications.Count
            );

            for (int k = 0; k < randomApplicationsCount; k++)
            {
                var student = faker.PickRandom(allStudents);
                var offerToApply = faker.PickRandom(offers);

                // Evitar duplicados
                if (
                    !applications.Any(a =>
                        a.StudentId == student.Id && a.JobOfferId == offerToApply.Id
                    )
                )
                {
                    applications.Add(
                        new JobApplication
                        {
                            StudentId = student.Id,
                            Student = student,
                            JobOfferId = offerToApply.Id,
                            JobOffer = offerToApply,
                            Status = faker.PickRandom<ApplicationStatus>(),
                            CreatedAt = DateTime.UtcNow.AddDays(-faker.Random.Int(1, 30)),
                        }
                    );
                }
            }

            await context.JobApplications.AddRangeAsync(applications);
            await context.SaveChangesAsync();
            Log.Information(
                "DataSeeder: Postulaciones de prueba cargadas ({Count})",
                applications.Count
            );
        }

        #region Reviews
        /// <summary>
        /// Crea 10 reviews manuales de prueba en la base de datos.
        /// - 6 reviews completadas (ambas partes evaluadas, ventana cerrada)
        /// - 4 reviews incompletas (solo oferente evaluó al estudiante, ventana aún abierta)
        ///
        /// NOTA IMPORTANTE - IDs de usuarios en la base de datos:
        /// ESTUDIANTES (Applicant): ID 1 (estudiante@alumnos.ucn.cl), ID 5,6,7 (aleatorios Faker)
        /// OFERENTES (Offerent): ID 2 (empresa@techcorp.cl), ID 3 (particular@ucn.cl), ID 8,9 (aleatorios)
        /// ADMIN: ID 4 (admin@ucn.cl - NO usar en reviews)
        /// PUBLICACIONES: Offers con IDs secuenciales desde 1
        /// </summary>
        private static async Task SeedReviews(AppDbContext context, UserManager<User> userManager)
        {
            var students = await userManager.GetUsersInRoleAsync(RoleNames.Applicant);
            var offerents = await userManager.GetUsersInRoleAsync(RoleNames.Offeror);
            var publications = await context.Offers.ToListAsync();
            var applications = await context.JobApplications.ToListAsync();

            if (students.Count == 0 || offerents.Count == 0 || applications.Count == 0)
            {
                Log.Warning(
                    "DataSeeder: No se pueden crear reviews - faltan usuarios o publicaciones"
                );
                return;
            }

            var now = DateTime.UtcNow;
            var reviews = new List<Review>();

            Log.Information("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Log.Information("📋 USUARIOS DISPONIBLES PARA REVIEWS:");
            Log.Information("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Log.Information("👨‍🎓 ESTUDIANTES (Applicant):");
            foreach (var s in students)
            {
                Log.Information($"   ID {s.Id}: {s.Email}");
            }
            Log.Information("🏢 OFERENTES (Offeror):");
            foreach (var o in offerents)
            {
                var type = o.UserType == UserType.Empresa ? "Empresa" : "Particular";
                Log.Information($"   ID {o.Id}: {o.Email} ({type})");
            }
            Log.Information("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            // REVIEWS COMPLETADAS (6 en total) - Ambas partes evaluadas
            // Esta review esta comentada para que se utilize en el flujo de Postman.
            // reviews.Add(new Review
            // {
            //     StudentId = students[0].Id, Student = students[0],
            //     OfferorId = publications[0].UserId, Offeror = publications[0].User,
            //     PublicationId = publications[0].Id, Publication = publications[0],
            //     RatingForStudent = 5, CommentForStudent = "Excelente estudiante, muy responsable y puntual. Cumplió con todas las expectativas.",
            //     AtTime = true, GoodPresentation = true, IsReviewForStudentCompleted = true,
            //     RatingForOfferor = 5, CommentForOfferor = "Muy buena experiencia laboral. Excelente ambiente de trabajo y aprendí mucho.",
            //     IsReviewForOfferorCompleted = true, IsCompleted = true,
            //     ReviewWindowEndDate = now.AddDays(-15),
            //     HasReviewForStudentBeenDeleted = false, HasReviewForOfferorBeenDeleted = false,
            // });

            reviews.Add(
                new Review
                {
                    ApplicantId = students[0].Id,
                    Applicant = students[0],
                    OfferorId = applications[1].JobOffer!.UserId,
                    Offeror = applications[1].JobOffer!.User,
                    ApplicationId = applications[1].Id,
                    Application = applications[1],
                    OfferorRatingOfApplicant = 4,
                    OfferorCommentForApplicant =
                        "Buen desempeño, aunque tuvo algunos retrasos menores. Muestra potencial.",
                    IsOnTime = true,
                    IsPresentable = true,
                    IsRespectful = true,
                    ApplicantRatingOfOfferor = 4,
                    ApplicantCommentForOfferor =
                        "Buena experiencia en general. Me permitió aplicar conocimientos universitarios.",
                }
            );

            reviews.Add(
                new Review
                {
                    ApplicantId = 5,
                    Applicant = students.FirstOrDefault(s => s.Id == 5),
                    OfferorId = applications[2].JobOffer!.UserId,
                    Offeror = applications[2].JobOffer!.User,
                    ApplicationId = applications[2].Id,
                    Application = applications[2],
                    OfferorRatingOfApplicant = 5,
                    OfferorCommentForApplicant =
                        "Muy comprometido con las tareas asignadas. Excelente actitud de trabajo.",
                    IsOnTime = true,
                    IsPresentable = true,
                    IsRespectful = true,
                    ApplicantRatingOfOfferor = 5,
                    ApplicantCommentForOfferor =
                        "Ambiente profesional y buena coordinación. Aprendí nuevas habilidades.",
                }
            );

            reviews.Add(
                new Review
                {
                    ApplicantId = 6,
                    Applicant = students.FirstOrDefault(s => s.Id == 6),
                    OfferorId = applications[3].JobOffer!.UserId,
                    Offeror = applications[3].JobOffer!.User,
                    ApplicationId = applications[3].Id,
                    Application = applications[3],
                    OfferorRatingOfApplicant = 3,
                    OfferorCommentForApplicant =
                        "Desempeño aceptable pero le faltó proactividad en algunos momentos.",
                    IsOnTime = true,
                    IsPresentable = false,
                    IsRespectful = false,
                    ApplicantRatingOfOfferor = 4,
                    ApplicantCommentForOfferor =
                        "Experiencia positiva. Instrucciones claras y buen trato del equipo.",
                }
            );

            reviews.Add(
                new Review
                {
                    ApplicantId = 7,
                    Applicant = students.FirstOrDefault(s => s.Id == 7),
                    OfferorId = applications[1].JobOffer!.UserId,
                    Offeror = applications[1].JobOffer!.User,
                    ApplicationId = applications[1].Id,
                    Application = applications[1],
                    OfferorRatingOfApplicant = 6,
                    OfferorCommentForApplicant =
                        "Estudiante excepcional. Superó todas las expectativas y mostró gran iniciativa.",
                    IsOnTime = true,
                    IsPresentable = true,
                    IsRespectful = true,
                    ApplicantRatingOfOfferor = 6,
                    ApplicantCommentForOfferor =
                        "Experiencia formativa increíble. Excelente mentoría y ambiente de aprendizaje.",
                }
            );

            reviews.Add(
                new Review
                {
                    ApplicantId = students[1 % students.Count].Id,
                    Applicant = students[1 % students.Count],
                    OfferorId = applications[2].JobOffer!.UserId,
                    Offeror = applications[2].JobOffer!.User,
                    ApplicationId = applications[2].Id,
                    Application = applications[2],
                    OfferorRatingOfApplicant = 6,
                    OfferorCommentForApplicant =
                        "Estudiante sobresaliente. Proactivo, responsable y con excelente actitud.",
                    IsOnTime = true,
                    IsPresentable = true,
                    IsRespectful = true,
                    ApplicantRatingOfOfferor = 5,
                    ApplicantCommentForOfferor =
                        "Excelente oportunidad de aprendizaje. Supervisión clara y buen ambiente.",
                }
            );

            reviews.Add(
                new Review
                {
                    ApplicantId = students[2 % students.Count].Id,
                    Applicant = students[2 % students.Count],
                    OfferorId = applications[3].JobOffer!.UserId,
                    Offeror = applications[3].JobOffer!.User,
                    ApplicationId = applications[3].Id,
                    Application = applications[3],
                    OfferorRatingOfApplicant = 3,
                    OfferorCommentForApplicant =
                        "Cumplió las tareas asignadas, pero faltó más iniciativa y comunicación.",
                    IsOnTime = true,
                    IsPresentable = false,
                    IsRespectful = true,
                    ApplicantRatingOfOfferor = 3,
                    ApplicantCommentForOfferor =
                        "Experiencia aceptable, pero faltó claridad en las instrucciones iniciales.",
                }
            );

            reviews.Add(
                new Review
                {
                    ApplicantId = students[2 % students.Count].Id,
                    Applicant = students[2 % students.Count],
                    OfferorId = applications[4].JobOffer!.UserId,
                    Offeror = applications[4].JobOffer!.User,
                    ApplicationId = applications[4].Id,
                    Application = applications[4],
                    OfferorRatingOfApplicant = 5,
                    OfferorCommentForApplicant =
                        "Muy buen estudiante. Adaptación rápida y trabajo en equipo destacable.",
                    IsOnTime = true,
                    IsPresentable = true,
                    IsRespectful = true,
                    ApplicantRatingOfOfferor = 6,
                    ApplicantCommentForOfferor =
                        "Experiencia excepcional. Organización impecable y excelente mentoría.",
                }
            );

            reviews.Add(
                new Review
                {
                    ApplicantId = students[3 % students.Count].Id,
                    Applicant = students[3 % students.Count],
                    OfferorId = applications[5].JobOffer!.UserId,
                    Offeror = applications[5].JobOffer!.User,
                    ApplicationId = applications[5].Id,
                    Application = applications[5],
                    OfferorRatingOfApplicant = 4,
                    OfferorCommentForApplicant =
                        "Buen nivel técnico y compromiso. Entregó trabajos de calidad.",
                    IsOnTime = true,
                    IsPresentable = true,
                    IsRespectful = true,
                    ApplicantRatingOfOfferor = 4,
                    ApplicantCommentForOfferor =
                        "Buena experiencia. Proyecto interesante y ambiente colaborativo.",
                }
            );

            // REVIEWS INCOMPLETAS (4 en total)
            // Solo oferente evaluo
            reviews.Add(
                new Review
                {
                    ApplicantId = students[0].Id,
                    Applicant = students[0],
                    OfferorId = applications[6 % applications.Count].JobOffer!.UserId,
                    Offeror = applications[6 % applications.Count].JobOffer!.User,
                    ApplicationId = applications[6 % applications.Count].Id,
                    Application = applications[6 % applications.Count],
                    OfferorRatingOfApplicant = 5,
                    OfferorCommentForApplicant =
                        "Estudiante confiable y organizado. Muy buena experiencia trabajando juntos.",
                    IsOnTime = true,
                    IsPresentable = true,
                    IsRespectful = true,
                    ApplicantRatingOfOfferor = null,
                    ApplicantCommentForOfferor = null,
                }
            );

            reviews.Add(
                new Review
                {
                    ApplicantId = students[1 % students.Count].Id,
                    Applicant = students[1 % students.Count],
                    OfferorId = applications[0].JobOffer!.UserId,
                    Offeror = applications[0].JobOffer!.User,
                    ApplicationId = applications[0].Id,
                    Application = applications[0],
                    OfferorRatingOfApplicant = 4,
                    OfferorCommentForApplicant =
                        "Buen trabajo en general. Cumplió plazos y mostró interés genuino.",
                    IsOnTime = false,
                    IsPresentable = true,
                    IsRespectful = true,
                    ApplicantRatingOfOfferor = null,
                    ApplicantCommentForOfferor = null,
                }
            );
            // Solo estudiante evaluo
            reviews.Add(
                new Review
                {
                    ApplicantId = students[3 % students.Count].Id,
                    Applicant = students[3 % students.Count],
                    OfferorId = applications[2].JobOffer!.UserId,
                    Offeror = applications[2].JobOffer!.User,
                    ApplicationId = applications[2].Id,
                    Application = applications[2],
                    ApplicantRatingOfOfferor = 5,
                    ApplicantCommentForOfferor =
                        "Muy buena experiencia. Aprendí mucho y el ambiente fue colaborativo.",
                    IsOnTime = true,
                    IsPresentable = true,
                    IsRespectful = true,
                    OfferorRatingOfApplicant = null,
                    OfferorCommentForApplicant = null,
                }
            );

            reviews.Add(
                new Review
                {
                    ApplicantId = students[2 % students.Count].Id,
                    Applicant = students[2 % students.Count],
                    OfferorId = applications[3].JobOffer!.UserId,
                    Offeror = applications[3].JobOffer!.User,
                    ApplicationId = applications[3].Id,
                    Application = applications[3],
                    OfferorRatingOfApplicant = null,
                    OfferorCommentForApplicant = null,

                    IsOnTime = false,
                    IsPresentable = false,
                    IsRespectful = false,
                    ApplicantRatingOfOfferor = 3,
                    ApplicantCommentForOfferor =
                        "Experiencia regular. Faltó mejor organización en las tareas asignadas.",
                }
            );

            // REVIEWS PENDIENTES PARA ESTUDIANTE2 (4 reviews sin responder del estudiante)
            var estudiante2 = students.FirstOrDefault(s => s.Email == "estudiante2@alumnos.ucn.cl");
            if (estudiante2 != null)
            {
                // Review 1: Solo oferente evaluo, estudiante2 NO ha respondido
                reviews.Add(
                    new Review
                    {
                        ApplicantId = estudiante2.Id,
                        Applicant = estudiante2,
                        OfferorId = applications[7 % applications.Count].JobOffer!.UserId,
                        Offeror = applications[7 % applications.Count].JobOffer!.User,
                        ApplicationId = applications[7 % applications.Count].Id,
                        Application = applications[7 % applications.Count],
                        OfferorRatingOfApplicant = 5,
                        OfferorCommentForApplicant =
                            "Buen trabajo en general, cumplió con las expectativas.",
                        IsOnTime = true,
                        IsPresentable = true,
                        IsRespectful = true,
                        ApplicantRatingOfOfferor = null,
                        ApplicantCommentForOfferor = null,
                    }
                );

                // Review 2: Solo oferente evaluo, estudiante2 NO ha respondido
                reviews.Add(
                    new Review
                    {
                        ApplicantId = estudiante2.Id,
                        Applicant = estudiante2,
                        OfferorId = applications[8 % applications.Count].JobOffer!.UserId,
                        Offeror = applications[8 % applications.Count].JobOffer!.User,
                        ApplicationId = applications[8 % applications.Count].Id,
                        Application = applications[8 % applications.Count],
                        OfferorRatingOfApplicant = 4,
                        OfferorCommentForApplicant =
                            "Mostró compromiso, aunque hubo retrasos menores.",
                        IsOnTime = false,
                        IsPresentable = true,
                        IsRespectful = true,
                        ApplicantRatingOfOfferor = null,
                        ApplicantCommentForOfferor = null,
                    }
                );

                // Review 3: Solo oferente evaluo, estudiante2 NO ha respondido
                reviews.Add(
                    new Review
                    {
                        ApplicantId = estudiante2.Id,
                        Applicant = estudiante2,
                        OfferorId = applications[9 % applications.Count].JobOffer!.UserId,
                        Offeror = applications[9 % applications.Count].JobOffer!.User,
                        ApplicationId = applications[9 % applications.Count].Id,
                        Application = applications[9 % applications.Count],
                        OfferorRatingOfApplicant = 6,
                        OfferorCommentForApplicant =
                            "Excelente desempeño, muy proactivo y responsable.",
                        IsOnTime = true,
                        IsPresentable = true,
                        IsRespectful = true,
                        ApplicantRatingOfOfferor = null,
                        ApplicantCommentForOfferor = null,
                    }
                );

                // Review 4: Solo oferente evaluo, estudiante2 NO ha respondido
                reviews.Add(
                    new Review
                    {
                        ApplicantId = estudiante2.Id,
                        Applicant = estudiante2,
                        OfferorId = applications[10 % applications.Count].JobOffer!.UserId,
                        Offeror = applications[10 % applications.Count].JobOffer!.User,
                        ApplicationId = applications[10 % applications.Count].Id,
                        Application = applications[10 % applications.Count],
                        OfferorRatingOfApplicant = 3,
                        OfferorCommentForApplicant = "Desempeño regular, faltó más comunicación.",
                        IsOnTime = true,
                        IsPresentable = false,
                        IsRespectful = true,
                        ApplicantRatingOfOfferor = null,
                        ApplicantCommentForOfferor = null,
                    }
                );

                Log.Information(
                    "DataSeeder: 4 reviews pendientes creadas para estudiante2@alumnos.ucn.cl"
                );
            }

            // REVIEWS ADICIONALES PARA EMPRESA@TECHCORP.CL (userId=3, publicacionId=60)
            // 4 estudiantes distintos, ninguna completada por el oferente, 2 completadas por estudiantes
            var empresaTechCorp = await context.Users.FirstOrDefaultAsync(u =>
                u.Email == "empresa@techcorp.cl"
            );
            var postulacion60 = await context.JobApplications.FirstOrDefaultAsync(p => p.Id == 60);
            var estudiantesAleatorios = await context
                .Users.Where(u =>
                    u.UserType == UserType.Estudiante
                    && u.Email != "estudiante@alumnos.ucn.cl"
                    && u.Email != "estudiante2@alumnos.ucn.cl"
                )
                .Take(4)
                .ToListAsync();

            if (
                empresaTechCorp != null
                && postulacion60 != null
                && estudiantesAleatorios.Count >= 4
            )
            {
                // Review 1: Estudiante SÍ completó su evaluación, Oferente NO
                reviews.Add(
                    new Review
                    {
                        ApplicantId = estudiantesAleatorios[0].Id,
                        Applicant = estudiantesAleatorios[0],
                        OfferorId = empresaTechCorp.Id,
                        Offeror = empresaTechCorp,
                        ApplicationId = 60,
                        Application = postulacion60,
                        OfferorRatingOfApplicant = null,
                        OfferorCommentForApplicant = null,
                        IsOnTime = false,
                        IsPresentable = false,
                        IsRespectful = false,
                        ApplicantRatingOfOfferor = 5,
                        ApplicantCommentForOfferor =
                            "Excelente experiencia laboral, ambiente muy profesional y buena comunicación.",
                    }
                );

                // Review 2: Estudiante SÍ completó su evaluación, Oferente NO
                reviews.Add(
                    new Review
                    {
                        ApplicantId = estudiantesAleatorios[1].Id,
                        Applicant = estudiantesAleatorios[1],
                        OfferorId = empresaTechCorp.Id,
                        Offeror = empresaTechCorp,
                        ApplicationId = 61,
                        Application = await context.JobApplications.FirstOrDefaultAsync(p =>
                            p.Id == 61
                        ),
                        OfferorRatingOfApplicant = null,
                        OfferorCommentForApplicant = null,
                        IsOnTime = false,
                        IsPresentable = false,
                        IsRespectful = false,
                        ApplicantRatingOfOfferor = 6,
                        ApplicantCommentForOfferor =
                            "Muy buena empresa para trabajar, aprendí mucho y el equipo es muy colaborativo.",
                    }
                );

                // Review 3: Estudiante NO completó su evaluación, Oferente NO
                reviews.Add(
                    new Review
                    {
                        ApplicantId = estudiantesAleatorios[2].Id,
                        Applicant = estudiantesAleatorios[2],
                        OfferorId = empresaTechCorp.Id,
                        Offeror = empresaTechCorp,
                        ApplicationId = 62,
                        Application = await context.JobApplications.FirstOrDefaultAsync(p =>
                            p.Id == 62
                        ),
                        OfferorRatingOfApplicant = null,
                        OfferorCommentForApplicant = null,
                        IsOnTime = false,
                        IsPresentable = false,
                        IsRespectful = false,
                        ApplicantRatingOfOfferor = null,
                        ApplicantCommentForOfferor = null,
                    }
                );

                // Review 4: Estudiante NO completó su evaluación, Oferente NO
                reviews.Add(
                    new Review
                    {
                        ApplicantId = estudiantesAleatorios[3].Id,
                        Applicant = estudiantesAleatorios[3],
                        OfferorId = empresaTechCorp.Id,
                        Offeror = empresaTechCorp,
                        ApplicationId = 63,
                        Application = await context.JobApplications.FirstOrDefaultAsync(p =>
                            p.Id == 63
                        ),
                        OfferorRatingOfApplicant = null,
                        OfferorCommentForApplicant = null,
                        IsOnTime = false,
                        IsPresentable = false,
                        IsRespectful = false,
                        ApplicantRatingOfOfferor = null,
                        ApplicantCommentForOfferor = null,
                    }
                );

                // Review 4: Estudiante NO completó su evaluación, Oferente NO
                reviews.Add(
                    new Review
                    {
                        ApplicantId = estudiantesAleatorios[3].Id,
                        Applicant = estudiantesAleatorios[3],
                        OfferorId = empresaTechCorp.Id,
                        Offeror = empresaTechCorp,
                        ApplicationId = 64,
                        Application = await context.JobApplications.FirstOrDefaultAsync(p =>
                            p.Id == 64
                        ),
                        OfferorRatingOfApplicant = null,
                        OfferorCommentForApplicant = null,
                        IsOnTime = false,
                        IsPresentable = false,
                        IsRespectful = false,
                        ApplicantRatingOfOfferor = null,
                        ApplicantCommentForOfferor = null,
                    }
                );

                Log.Information(
                    "DataSeeder: 4 reviews adicionales creadas para empresa@techcorp.cl en publicación 60"
                );
            }

            await context.Reviews.AddRangeAsync(reviews);
            await context.SaveChangesAsync();
            Log.Information(
                "DataSeeder: {Count} reviews creadas exitosamente (6 completas, 12 incompletas - 4 para estudiante2, 4 para empresa@techcorp.cl)",
                reviews.Count
            );

            // Actualizar ratings de usuarios (normalmente lo hace ReviewService automáticamente)
            Log.Information("DataSeeder: Actualizando ratings de estudiantes y oferentes...");
            var allUserIds = new HashSet<int>();
            foreach (var review in reviews)
            {
                allUserIds.Add(review.ApplicantId);
                allUserIds.Add(review.OfferorId);
            }

            foreach (var userId in allUserIds)
            {
                var user = await context.Users.FindAsync(userId);
                if (user == null)
                    continue;

                double? averageRating = null;
                if (user.UserType == UserType.Estudiante)
                {
                    var studentReviews = await context
                        .Reviews.Where(r =>
                            r.ApplicantId == userId && r.OfferorRatingOfApplicant.HasValue
                        )
                        .ToListAsync();
                    if (studentReviews.Count != 0)
                        averageRating = studentReviews.Average(r =>
                            r.OfferorRatingOfApplicant!.Value
                        );
                }
                else if (user.UserType == UserType.Empresa || user.UserType == UserType.Particular)
                {
                    var offerorReviews = await context
                        .Reviews.Where(r =>
                            r.OfferorId == userId && r.ApplicantRatingOfOfferor.HasValue
                        )
                        .ToListAsync();
                    if (offerorReviews.Count != 0)
                        averageRating = offerorReviews.Average(r =>
                            r.ApplicantRatingOfOfferor!.Value
                        );
                }
                user.Rating = (float?)averageRating ?? 0.0f;
                context.Users.Update(user);
            }

            await context.SaveChangesAsync();
            Log.Information(
                "DataSeeder: Ratings actualizados exitosamente para {Count} usuarios",
                allUserIds.Count
            );
        }
    }
}
        #endregion
