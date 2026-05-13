using locamonda.Models;
using locamonda.Data;
using locamonda.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── SERVICES ─────────────────────────────────────

// MVC
builder.Services.AddControllersWithViews();

// DB CONTEXT
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// IDENTITY
builder.Services.AddIdentity<Users, IdentityRole<int>>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// AUTHENTICATION & COOKIE SETTINGS
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Users/Login";
    options.AccessDeniedPath = "/Users/AccessDenied";
});

// SESSION (required for HttpContext.Session)
builder.Services.AddSession();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// SEED DATA
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<Users>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        
        var seeder = new DataSeeder(context, userManager, roleManager);
        seeder.SeedAll().Wait();
    } catch (Exception ex) {
        Console.WriteLine("An error occurred while seeding the database: " + ex.Message);
    }
}

// ─── PIPELINE ─────────────────────────────────────

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();   // MUST be after UseRouting, before auth

app.UseAuthentication(); // Added
app.UseIsActiveCheck();
app.UseAuthorization();

// ROUTES
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
