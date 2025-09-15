using AutoMapper;
using ManageLife.Base;
using ManageLife.Data;
using ManageLife.Extentions;
using ManageLife.Helpers;
using ManageLife.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add application services
builder.Services.AddApplicationServices(builder.Configuration);

// Cấu hình Kestrel (request body limit)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = long.MaxValue;
});

var app = builder.Build();
GlobalHttpContext.Configure(app.Services.GetRequiredService<IHttpContextAccessor>());
// ======= SEED DATA =========
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await Seed.SeedData(context);

    // ======= REGISTER PERMISSIONS =========
    var services = scope.ServiceProvider;
    await services.RegisterPermissionsAsync();
}

// Gán IMapper từ DI vào MapperBase
var mapper = app.Services.GetRequiredService<IMapper>();
MapperBase.Configure(mapper);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseMiddleware<JwtAuthenticationMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "admin_default",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}",
    defaults: new { area = "Admin" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
