using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using WorkScheduleApp.Data;
using WorkScheduleApp.Services;
using Oracle.EntityFrameworkCore; 

var builder = WebApplication.CreateBuilder(args);

// read connection string from appsettings.json
var conn = builder.Configuration.GetConnectionString("Oracle");

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Use Oracle EF Core provider
    options.UseOracle(conn);
});

builder.Services.AddScoped<UserService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var svc = scope.ServiceProvider.GetRequiredService<WorkScheduleApp.Services.UserService>();
    var db = scope.ServiceProvider.GetRequiredService<WorkScheduleApp.Data.AppDbContext>();
    if (!db.AppUsers.Any())
    {
        svc.CreateUser("admin", "StrongPass123!");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// default simple config (not using ASP.NET Core Identity here)
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
