using locamonda.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── SERVICES ─────────────────────────────────────

// MVC
builder.Services.AddControllersWithViews();

// DB CONTEXT
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// SESSION (required for HttpContext.Session)
builder.Services.AddSession();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

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

app.UseAuthorization();

// ROUTES
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();