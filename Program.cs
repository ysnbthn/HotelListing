using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// hatalarýn dosyada kaydýný tutmasý için logger ayarla
builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console().WriteTo.File(
    path: "logs\\log-.txt", // dosya yolu aþaðýda mesaj formatý var
    outputTemplate : "{Timestamp:dd-MM-yyyy HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
    rollingInterval: RollingInterval.Day, // loglama aralýðý
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

app.UseAuthorization();

app.MapControllers();

app.Run();
