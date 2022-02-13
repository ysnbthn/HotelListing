using AspNetCoreRateLimit;
using HotelListing.Core;

var builder = WebApplication.CreateBuilder(args);
// servisleri ekle
ServiceExtensions.AddServices(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.ConfigureExceptionHandler();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// kullan�c� kimlik do�rulamas� i�in ekle
app.UseAuthentication();

app.UseAuthorization();

// burayada response cache ekle
app.UseResponseCaching();
// ip rate limit ekle
app.UseIpRateLimiting();
app.UseHttpCacheHeaders();

app.MapControllers();

app.Run();
