﻿using HotelListing.Configurations;
using HotelListing.Data;
using HotelListing.Entities;
using HotelListing.IRepository;
using HotelListing.Models;
using HotelListing.Repository;
using HotelListing.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
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
            // db context ekle
            builder.Services.AddDbContext<DatabaseContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("sqlConnection"))
            );

            // kullanıcılara kimlik servisi ekle
            builder.Services.AddIdentityCore<ApiUser>(q => q.User.RequireUniqueEmail = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<DatabaseContext>();
           

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

            builder.Services.AddEndpointsApiExplorer();
           
            AddSwaggerDoc(builder.Services);
            // reverse navigation loopa girerse onu görmezden gel
            builder.Services.AddControllers().AddNewtonsoftJson(op =>
                op.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            // hataların dosyada kaydını tutması için logger ayarla
            builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console().WriteTo.File(
                path: "logs\\log-.txt", // dosya yolu aşağıda mesaj formatı var
                outputTemplate: "{Timestamp:dd-MM-yyyy HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day, // loglama aralığı
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
                ));

            // Add services to the container.
            builder.Services.AddControllers();

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
        public static void ConfigureJwt(WebApplicationBuilder builder)
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
    }
}
