using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Data;
using EmployeeManagement.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new() { Title = "Employee Management API", Version = "v1" }));
builder.Services.AddControllersWithViews();
builder.Services.AddProblemDetails();
var connectionString = builder.Configuration.GetConnectionString("EmployeeDatabase");
if (string.IsNullOrWhiteSpace(connectionString) && builder.Environment.IsEnvironment("Testing"))
{
    connectionString = "Host=localhost;Database=employee_management_tests;Username=postgres";
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "PostgreSQL connection string 'EmployeeDatabase' is missing. "
        + "Set ConnectionStrings__EmployeeDatabase or configure it with User Secrets.");
}

builder.Services.AddDbContext<EmployeeDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();
    dbContext.Database.Migrate();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee Management API v1"));
}

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
