using Koi.Web.Data;
using Koi.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLocalization(options => options.ResourcesPath = "code/Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture("en").AddSupportedCultures("en", "vi").AddSupportedUICultures("en", "vi");
    options.RequestCultureProviders = [new CookieRequestCultureProvider(), new QueryStringRequestCultureProvider(), new AcceptLanguageHeaderRequestCultureProvider()];
});

// Add services to the container.
builder.Services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()))
    .AddViewLocalization()
    .AddRazorOptions(options =>
    {
        // Both public and Admin views live inside the shared code folder.
        options.ViewLocationFormats.Clear();
        options.ViewLocationFormats.Add("/code/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/code/Views/Shared/{0}.cshtml");
        options.AreaViewLocationFormats.Clear();
        options.AreaViewLocationFormats.Add("/code/Views/{2}/{1}/{0}.cshtml");
        options.AreaViewLocationFormats.Add("/code/Views/{2}/Shared/{0}.cshtml");
        options.AreaViewLocationFormats.Add("/code/Views/Shared/{0}.cshtml");
    });
builder.Services.AddScoped<ImageUploads>();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection for PostgreSQL.")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 12;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Account/Login";
    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
    options.Cookie.Name = "Koi.Admin";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

var app = builder.Build();
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Database:Initialize"))
    await DatabaseInitializer.InitializeAsync(app.Services, app.Configuration);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRequestLocalization();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute("areas", "{area:exists}/{controller=News}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Cover}/{id?}")
    .WithStaticAssets();


app.Run();

public partial class Program;
