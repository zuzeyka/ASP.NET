using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using WebApplication1.Data;
using WebApplication1.Middleware;
using WebApplication1.Servises;
using WebApplication1.Servises.Email;
using WebApplication1.Servises.Hash;
using WebApplication1.Servises.KDF;
using WebApplication1.Servises.Random;
using WebApplication1.Servises.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<DateServise>();
builder.Services.AddScoped<TimeServise>();
builder.Services.AddSingleton<StampServise>();

builder.Services.AddSingleton<IHashServise, MD5HashServise>();
builder.Services.AddSingleton<IRandomServise, RandomServiseV1>();
builder.Services.AddSingleton<IKdfServise, HashKdfService>();
builder.Services.AddSingleton<IValidationService, ValidationServiceV1>();
builder.Services.AddSingleton<IEmailService, GmailService>();

// ����������� ��������� � ������������ � MS SQL Server
/*
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MsDb")
    )
);
*/

// ����������� ��������� � ������������ � MySQL
// ����������� - ��� ��������� ������� ���������� ������ MySQL
// ������� 1 - ���������� ������ � ������ ������
/*
ServerVersion serverVersion = new MySqlServerVersion(new Version(8, 0, 23));
builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(
     builder.Configuration.GetConnectionString("MySQLDb"),
     serverVersion));
*/
// ������� 2 - �������������� ����������� ������, �� ��� ����� �����
// �������������� ������� �����������

String? connectionString = builder.Configuration.GetConnectionString("MySqlDb");
MySqlConnection connection = new(connectionString);
builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(connection, ServerVersion.AutoDetect(connection)));

// Add services to the container.
builder.Services.AddControllersWithViews();

// ��������� ������
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 404)
    {
        context.Request.Path = "/Home/Page404";
        await next();
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// ��������� ��������� ������
app.UseSession();
app.UseSessionAuth();

app.UseMiddleware<SessionAuthMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
