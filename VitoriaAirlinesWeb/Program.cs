using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using Syncfusion.Licensing;
using System.Text;
using System.Text.Json.Serialization;
using VitoriaAirlinesWeb.Configuration;
using VitoriaAirlinesWeb.Data;
using VitoriaAirlinesWeb.Data.Entities;
using VitoriaAirlinesWeb.Data.Repositories;
using VitoriaAirlinesWeb.Helpers;
using VitoriaAirlinesWeb.Hubs;
using VitoriaAirlinesWeb.Services;

namespace VitoriaAirlinesWeb
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Synfusion License
            SyncfusionLicenseProvider.RegisterLicense(builder.Configuration["Syncfusion:LicenseKey"]);

            builder.Services.ConfigureApplicationCookie(options =>
            {
                // Quando um utilizador não está logado, envia-o para a página de login.
                options.LoginPath = "/Account/Login";

                // Quando um utilizador está logado mas não tem permissão (ex: Role errada),
                // envia-o para a sua nova página de acesso proibido.
                options.AccessDeniedPath = "/Account/NotAuthorized";
            });


            builder.Services
                .AddControllersWithViews()
                .AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                opts.JsonSerializerOptions.WriteIndented = true;
            });



            // SignalR
            builder.Services.AddSignalR();

            // Database Context
            builder.Services.AddDbContext<DataContext>(o =>

            {
                o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

            });

            // ASP.NET Core Identity
            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = false; // TODO change to true in production
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;

                // User settings
                options.User.RequireUniqueEmail = true;

                // SignIn settings
                options.SignIn.RequireConfirmedEmail = true;

                // Token settings
                options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
            })
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<DataContext>();


            // JWT Authentication configuration
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateLifetime = true,
                       ValidateIssuerSigningKey = true,
                       //define o emissor e a audiência validas para o token JWT obtidos da aplicação
                       ValidAudience = builder.Configuration["JWT:Audience"],
                       ValidIssuer = builder.Configuration["JWT:Issuer"],
                       //Define a chave de assinatura usada para assinar e verificar o token JWT.
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!))
                   };
               });


            builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\tempkeys\keys"))
    .SetApplicationName("VitoriaAirlines");

            //        builder.Services.AddDataProtection()
            //.PersistKeysToDbContext<DataContext>()
            //.SetApplicationName("VitoriaAirlines");

            // Swagger + JWT setup
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "VitoriaAirlines API", Version = "v1" });

                // Define JWT Bearer scheme
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                // Apply Bearer scheme globally to all endpoints
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });


            // Razor Pages
            builder.Services.AddRazorPages().AddRazorRuntimeCompilation();



            // Repositories
            builder.Services.AddScoped<ICustomerProfileRepository, CustomerProfileRepository>();
            builder.Services.AddScoped<ICountryRepository, CountryRepository>();
            builder.Services.AddScoped<IAirplaneRepository, AirplaneRepository>();
            builder.Services.AddScoped<IAirportRepository, AirportRepository>();
            builder.Services.AddScoped<IFlightRepository, FlightRepository>();
            builder.Services.AddScoped<ITicketRepository, TicketRepository>();

            // Helpers
            builder.Services.AddScoped<IUserHelper, UserHelper>();
            builder.Services.AddScoped<IMailHelper, MailHelper>();
            builder.Services.AddScoped<IBlobHelper, BlobHelper>();
            builder.Services.AddScoped<IConverterHelper, ConverterHelper>();
            builder.Services.AddScoped<ISeatGeneratorHelper, SeatGeneratorHelper>();
            builder.Services.AddScoped<IFlightHelper, FlightHelper>();
            builder.Services.AddTransient<SeedDb>();

            // Hangfire config
            builder.Services.AddHangfire(config =>
                config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
            GlobalJobFilters.Filters.Add(new JobExpirationFilter(TimeSpan.FromMinutes(15)));
            builder.Services.AddHangfireServer();

            // FlightService
            builder.Services.AddScoped<IFlightService, FlightService>();

            // NotificationService
            builder.Services.AddScoped<INotificationService, NotificationService>();

            // GeminiApiService
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IGeminiApiService, GeminiApiService>();

            // Prompt Services
            builder.Services.AddScoped<IAdminPromptService, AdminPromptService>();
            builder.Services.AddScoped<IEmployeePromptService, EmployeePromptService>();
            builder.Services.AddScoped<ICustomerPromptService, CustomerPromptService>();
            builder.Services.AddScoped<IAnonymousPromptService, AnonymousPromptService>();




            // Stripe config
            builder.Services.Configure<StripeSettings>(
                builder.Configuration.GetSection("Stripe"));

            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            // Stripe Service
            builder.Services.AddScoped<IPaymentService, StripePaymentService>();


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });


            builder.Services.AddSession();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Swagger JSON and UI at /api
            app.UseSwagger(options =>
            {
                options.RouteTemplate = "api/swagger/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "VitoriaAirlines API v1");
                c.RoutePrefix = "api";
            });

            app.UseStatusCodePagesWithReExecute("/Errors/Handle404");


            app.UseSession();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHangfireDashboard(); // localhost/hangfire


            RecurringJob.AddOrUpdate<IFlightService>(
                "update-completed-flights",
                service => service.UpdateFlightStatusAsync(),
                Cron.Minutely);




            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            //pattern: "{controller=Dashboard}/{action=Index}/{id?}");
            app.MapControllers();


            // Maps SignalR endpoint to NotificationHub.
            // Clients will connect to "/notificationHub" for real time communication.
            app.MapHub<NotificationHub>("/notificationHub");

            // Seed the database
            await RunSeeding(app);

            app.Run();
        }

        private static async Task RunSeeding(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var seedDb = services.GetRequiredService<SeedDb>();
            await seedDb.SeedAsync();
        }
    }
}
