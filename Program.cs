using HotelListing.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("sqlConnection"))
);

// Add services to the container.
builder.Services.AddControllers();

// kimler api'ya eri�ebilir
builder.Services.AddCors(policy=> {
    policy.AddPolicy("AllowAll", bldr =>
        // �imdilik herkes
        bldr.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
    );
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// hatalar�n dosyada kayd�n� tutmas� i�in logger ayarla
builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console().WriteTo.File(
    path: "logs\\log-.txt", // dosya yolu a�a��da mesaj format� var
    outputTemplate : "{Timestamp:dd-MM-yyyy HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
    rollingInterval: RollingInterval.Day, // loglama aral���
    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
    ));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
