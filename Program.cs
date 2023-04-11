using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using WebApplication1.Data;
using WebApplication1.Servises;
using WebApplication1.Servises.Hash;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<DateServise>();
builder.Services.AddScoped<TimeServise>();
builder.Services.AddSingleton<StampServise>();

builder.Services.AddSingleton<IHashServise, MD5HashServise>();

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

var app = builder.Build();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
