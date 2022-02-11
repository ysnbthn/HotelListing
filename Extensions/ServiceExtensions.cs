using HotelListing.Configurations;
using HotelListing.Data;
using HotelListing.Entities;
using HotelListing.IRepository;
using HotelListing.Models;
using HotelListing.Repository;
using HotelListing.Services;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Net.Http;
using System.Text;

namespace HotelListing.Extensions
{
    public static class ServiceExtensions
    {
        // servisleri program.cs temiz olsun diye ayırdım
        public static void AddServices(WebApplicationBuilder builder)
        {
            ConfigureIdentity(builder.Services);
            // db context ekle
            builder.Services.AddDbContext<DatabaseContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("sqlConnection"))
            );

            // kimler api'ya erişebilir
            builder.Services.AddCors(policy => {
                policy.AddPolicy("AllowAll", bldr =>
                    // şimdilik herkes
                    bldr.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                );
            });
            // mapper ile interfaceler
            builder.Services.AddAutoMapper(typeof(MapperInitilizer));
            builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IAuthManager, AuthManager>();

            // response cache ekle
            ConfigureHttpCacheHeaders(builder);

            builder.Services.AddEndpointsApiExplorer();
            // aşağıdaki swagger documentation'ı ekle
            AddSwaggerDoc(builder.Services);
            // reverse navigation loopa girerse onu görmezden gel
            // add controller içine global caching ekle
            builder.Services.AddControllers( config =>
            {
                // isim ver süresini belirt
                config.CacheProfiles.Add("120SecondsDuration", new CacheProfile
                {
                    Duration = 120
                });
            }).AddNewtonsoftJson(op =>
                op.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            // api versioning fonksiyonunu buraya koy
            ConfigureVersioning(builder);

            // hataların dosyada kaydını tutması için logger ayarla
            builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console().WriteTo.File(
                path: "logs\\log-.txt", // dosya yolu aşağıda mesaj formatı var
                outputTemplate: "{Timestamp:dd-MM-yyyy HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day, // loglama aralığı
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
                ));

            ConfigureJwt(builder);

            // Add services to the container.
            builder.Services.AddControllers();

        }
        // user identity ayarları |  IServiceCollection = WebApplicationBuilder builder.Services
        private static void ConfigureIdentity(this IServiceCollection services)
        {
            var builder = services.AddIdentityCore<ApiUser>(q => { q.User.RequireUniqueEmail = true; });

            builder = new IdentityBuilder(builder.UserType, typeof(IdentityRole), services);
            builder.AddTokenProvider("HotelListingApi", typeof(DataProtectorTokenProvider<ApiUser>));
            builder.AddEntityFrameworkStores<DatabaseContext>().AddDefaultTokenProviders();
        }

        // JWT'yi swagger documentation'a ekle
        private static void AddSwaggerDoc(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                // tepeye token girmen için bölme ekliyor
                c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "0auth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });
        }

        // jwt ayarlaması için fonksiyon
        private static void ConfigureJwt(WebApplicationBuilder builder)
        {
            // sistem yerine dosyadan sistem değeri al
            DotNetEnv.Env.Load();
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = Environment.GetEnvironmentVariable("KEY");
            // jwt ayarla
            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
                {
                    // burası şirkete göre değişebilir
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = false,
                        ValidateLifetime = true, // eğer süresi bittiyse reddeder
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.GetSection("Issuer").Value,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                    };
                });
        }
        // exception middleware | IApplicationBuilder = WebApplicationBuilder 
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            // var olan exception handler'ı override et
            app.UseExceptionHandler( error =>
            {
                error.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        // hata olursa hatayı logla geriye status code ile mesajı döndür.
                        Log.Error($"Something Went Wrong in the { contextFeature.Error }");

                        await context.Response.WriteAsync(new Error
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = "Internal Server Error. Please Try Again Later."
                        }.ToString());
                    }
                });
            });
        }
        
        private static void ConfigureVersioning(WebApplicationBuilder builder)
        {
            builder.Services.AddApiVersioning(opt =>
            {
                opt.ReportApiVersions = true;
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.DefaultApiVersion = new ApiVersion(1, 0);
                opt.ApiVersionReader = new HeaderApiVersionReader("api-version");
            });
        }
        // yeni bilgi eklendiyse cache'i sıfırla
        private static void ConfigureHttpCacheHeaders(WebApplicationBuilder builder)
        {
            builder.Services.AddResponseCaching();
            builder.Services.AddHttpCacheHeaders(exprationOpt =>
            {
                exprationOpt.MaxAge = 120; // cache max süre
                exprationOpt.CacheLocation = CacheLocation.Private; // yeri private
            },
            (validationOpt) =>
            {
                validationOpt.MustRevalidate = true; // data değişince tekrar istek at
            }
            );

        }

    }
}
