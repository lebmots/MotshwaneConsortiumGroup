
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using MotshwaneConsortiumGroup.Data;
using MotshwaneConsortiumGroup.Extensions;


var builder = WebApplication.CreateBuilder(args);

// ---------------- MVC ----------------

builder.Services.AddControllersWithViews();

// ---------------- AUTHENTICATION ----------------

builder.Services
.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.LoginPath = "/Customer/Login";
    options.AccessDeniedPath = "/Home/AccessDenied";

    options.Cookie.Name = "MotshwaneConsortium.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;

    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization();

// ---------------- DATABASE ----------------

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(
builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------------- APPLICATION SERVICES ----------------

// Register database-backed business services
builder.Services.AddApplicationServices();


// ---------------- SWAGGER / REST API ----------------

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Motshwane Consortium Group API",
        Version = "v1",
        Description = "REST API for managing customers, units and bookings."
    });
});

<<<<<<< HEAD
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sql => sql.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(10),
        errorNumbersToAdd: null)));

builder.Services.AddApplicationServices();
=======
// ---------------- BUILD APPLICATION ----------------
>>>>>>> f50571c47347a7a67783b2283a6d4128e81a653d

var app = builder.Build();

// ---------------- ERROR HANDLING ----------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ---------------- MIDDLEWARE ----------------

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Authentication must come before authorization
app.UseAuthentication();
app.UseAuthorization();

// ---------------- SWAGGER ----------------

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
    "/swagger/v1/swagger.json",
    "Motshwane Consortium Group API v1");

    options.RoutePrefix = "swagger";
});

// ---------------- SECURITY HEADERS ----------------

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] =
    "strict-origin-when-cross-origin";

    await next();
});

// ---------------- ROUTING ----------------

app.MapControllerRoute(
name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}");

// ---------------- DATABASE SEEDING ----------------

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
    .GetRequiredService<ApplicationDbContext>();

    await DbSeeder.SeedAsync(context);
}

// ---------------- RUN APPLICATION ----------------

app.Run();

