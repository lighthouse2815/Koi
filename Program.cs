using Koi.Web.Data;
using Koi.Web.Middleware;
using Koi.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    using var localFiles = new PhysicalFileProvider(builder.Environment.ContentRootPath, ExclusionFilters.None);
    using var localEnvironment = new ConfigurationManager();
    localEnvironment.AddIniFile(localFiles, ".env", optional: true, reloadOnChange: false);

    var cloudinary = new Dictionary<string, string?>();
    foreach (var name in new[] { "CloudName", "ApiKey", "ApiSecret" })
    {
        var key = $"Cloudinary:{name}";
        if (builder.Configuration[key] is null && localEnvironment[$"Cloudinary__{name}"] is { } value)
            cloudinary[key] = value;
    }

    builder.Configuration.AddInMemoryCollection(cloudinary);
}

builder.Services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()))
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Clear();
        options.ViewLocationFormats.Add("/code/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/code/Views/Shared/{0}.cshtml");
        options.AreaViewLocationFormats.Clear();
        options.AreaViewLocationFormats.Add("/code/Views/{2}/{1}/{0}.cshtml");
        options.AreaViewLocationFormats.Add("/code/Views/{2}/Shared/{0}.cshtml");
        options.AreaViewLocationFormats.Add("/code/Views/Shared/{0}.cshtml");
    });

builder.Services.AddScoped<ImageUploadService>();
builder.Services.AddScoped<NewsArticleService>();
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<DrinkService>();
builder.Services.AddScoped<FeaturedDrinkService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<StoreService>();
builder.Services.AddScoped<StoreCityService>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection for PostgreSQL.")));

builder.Services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(options =>
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
app.UseMiddleware<AppExceptionMiddleware>();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    "legacy-drink-categories",
    "Admin/DrinkCategories/{action=Index}/{id?}",
    new { area = "Admin", controller = "Category" }
).Add(endpoint => endpoint.Metadata.Add(new SuppressLinkGenerationMetadata()));

app.MapControllerRoute(
    "legacy-geography",
    "Admin/Geography/{action=Index}/{id?}",
    new { area = "Admin", controller = "StoreCities" }
).Add(endpoint => endpoint.Metadata.Add(new SuppressLinkGenerationMetadata()));

app.MapControllerRoute("areas", "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
).WithStaticAssets();


app.Run();

public partial class Program;
