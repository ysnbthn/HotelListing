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

ServiceExtensions.AddServices(builder);

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

app.UseAuthentication();

app.MapControllers();

app.Run();
