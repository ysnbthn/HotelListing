using HotelListing.Configurations;
using HotelListing.Data;
using HotelListing.Entities;
using HotelListing.IRepository;
using HotelListing.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HotelListing.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddServices(WebApplicationBuilder builder)
        {

            builder.Services.AddDbContext<DatabaseContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("sqlConnection"))
            );

            builder.Services.AddIdentityCore<ApiUser>(q => q.User.RequireUniqueEmail = true)
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

            builder.Services.AddAutoMapper(typeof(MapperInitilizer));
            builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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
    }
}
