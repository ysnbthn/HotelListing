using AspNetCoreRateLimit;
using HotelListing.Configurations;
using HotelListing.Data;
using HotelListing.Entities;
using HotelListing.Extensions;
using HotelListing.IRepository;
using HotelListing.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

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

// kullanýcý kimlik doðrulamasý için ekle
app.UseAuthentication();

app.UseAuthorization();

// burayada response cache ekle
app.UseResponseCaching();
// ip rate limit ekle
app.UseIpRateLimiting();
app.UseHttpCacheHeaders();

app.MapControllers();

app.Run();
