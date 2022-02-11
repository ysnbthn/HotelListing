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
ServiceExtensions.ConfigureIdentity(builder.Services);
ServiceExtensions.ConfigureJwt(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// kullanýcý kimlik doðrulamasý için ekle
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
