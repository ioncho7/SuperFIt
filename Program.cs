using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using SuperFit.Data;
using SuperFit.Data.Seed;
using SuperFit.Models;
using SuperFit.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
// DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddRazorPages();
builder.Services.AddSingleton<IEmailSender, DevEmailSender>();
builder.Services.AddSession();


var bgCulture = new CultureInfo("bg-BG");

CultureInfo.DefaultThreadCurrentCulture = bgCulture;
CultureInfo.DefaultThreadCurrentUICulture = bgCulture;
builder.Services.AddLocalization();


// Identity + Roles
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddErrorDescriber<BulgarianIdentityErrorDescriber>();


var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Seed roles + admin
await IdentitySeed.SeedAsync(app.Services);

app.Run();
