using GameVoting.Data;
using GameVoting.Infrastructure;
using GameVoting.Models.Entities;
using GameVoting.Repositories;
using GameVoting.Repositories.Interfaces;
using GameVoting.Services;
using GameVoting.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

string? databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    var uri = new Uri(databaseUrl!);
    var userInfo = uri.UserInfo.Split(':');
    var builderNpgsql = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = uri.UserInfo.Split(':')[0],
        Password = uri.UserInfo.Split(':')[1],
        SslMode = SslMode.Require
    };
    connectionString = builderNpgsql.ToString();
}
else
{
    connectionString = builder.Configuration.GetConnectionString("Postgres")!;
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
    .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddControllersWithViews();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<PortugueseIdentityErrorDescriber>();

builder.Services.AddAuthentication()
    .AddSteam(options =>
    {
        options.ApplicationKey = builder.Configuration["Steam:ApiKey"];
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
});

builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IVoteRepository, VoteRepository>();
builder.Services.AddScoped<IRegistrationTokenRepository, RegistrationTokenRepository>();

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IVoteService, VoteService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ISteamService, SteamService>();
builder.Services.AddScoped<IRegistrationTokenService, RegistrationTokenService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddHttpClient<ISteamService, SteamService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    if (!await roleManager.RoleExistsAsync("Member"))
        await roleManager.CreateAsync(new IdentityRole("Member"));

    var adminUser = await userManager.FindByNameAsync(builder.Configuration["AdminEmail"]!);
    if (adminUser is not null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
        await userManager.AddToRoleAsync(adminUser, "Admin");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Game}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
