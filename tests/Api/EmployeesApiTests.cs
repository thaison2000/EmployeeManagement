using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EmployeeManagement.DTO;
using Xunit;
using Xunit.Abstractions;

namespace EmployeeManagement.Tests;

public sealed class EmployeesApiTests(EmployeeApiFactory factory, ITestOutputHelper output)
    : IClassFixture<EmployeeApiFactory>, IAsyncLifetime
{
    private HttpClient _client = null!;
    private int _departmentId;

    public async Task InitializeAsync()
    {
        _client = factory.CreateClient();
        await factory.ResetDatabaseAsync();
        var departmentResponse = await _client.PostAsJsonAsync(
            "/api/departments",
            new { name = "Engineering" });
        var department = await departmentResponse.Content.ReadFromJsonAsync<DepartmentResponse>()
            ?? throw new InvalidOperationException("The department response had no body.");
        _departmentId = department.Id;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Endpoints_AreAvailableWithoutAuthentication()
    {
        var response = await _client.GetAsync("/api/employees");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateAndGet_ReturnsEmployeeWithDepartment()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/employees",
            new
            {
                name = "  Linh Nguyen  ",
                phone = " 0901234567 ",
                position = "Developer",
                departmentId = _departmentId
            });

        output.WriteLine(
            "Create response: {0} {1}",
            createResponse.StatusCode,
            createResponse.Headers);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);
        var created = await createResponse.Content.ReadFromJsonAsync<EmployeeResponse>()
            ?? throw new InvalidOperationException("The employee response had no body.");
        Assert.Equal("Linh Nguyen", created.Name);
        Assert.Equal("0901234567", created.Phone);
        Assert.Equal(_departmentId, created.DepartmentId);
        Assert.Equal("Engineering", created.DepartmentName);

        var fetched = await _client.GetFromJsonAsync<EmployeeResponse>($"/api/employees/{created.Id}");
        Assert.Equal(created, fetched);
    }
 
    [Fact]
    public async Task Create_ReturnsNotFound_WhenDepartmentDoesNotExist()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/employees",
            new { name = "Sam", phone = "555-0100", position = "Analyst", departmentId = 999 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FilterAndUpdate_UseDepartmentAssignment()
    {
        var otherDepartmentResponse = await _client.PostAsJsonAsync(
            "/api/departments",
            new { name = "Finance" });
        var otherDepartment = await otherDepartmentResponse.Content.ReadFromJsonAsync<DepartmentResponse>()
            ?? throw new InvalidOperationException("The department response had no body.");
        var createResponse = await _client.PostAsJsonAsync(
            "/api/employees",
            new { name = "Sam", phone = "555-0100", position = "Analyst", departmentId = _departmentId });
        var employee = await createResponse.Content.ReadFromJsonAsync<EmployeeResponse>()
            ?? throw new InvalidOperationException("The employee response had no body.");

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/employees/{employee.Id}",
            new
            {
                name = employee.Name,
                phone = employee.Phone,
                position = employee.Position,
                departmentId = otherDepartment.Id
            });
        var filtered = await _client.GetFromJsonAsync<EmployeeResponse[]>(
            $"/api/employees?departmentId={otherDepartment.Id}");

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Single(filtered ?? []);
        Assert.Equal(otherDepartment.Id, filtered![0].DepartmentId);
    }

    [Fact]
    public async Task Update_ReturnsSpecificNotFoundForMissingEmployeeOrDepartment()
    {
        var missingEmployeeResponse = await _client.PutAsJsonAsync(
            "/api/employees/999",
            new { name = "Sam", phone = "555-0100", position = "Analyst", departmentId = _departmentId });

        Assert.Equal(HttpStatusCode.NotFound, missingEmployeeResponse.StatusCode);
        using var missingEmployeeBody = JsonDocument.Parse(
            await missingEmployeeResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            "Employee not found.",
            missingEmployeeBody.RootElement.GetProperty("title").GetString());

        var createResponse = await _client.PostAsJsonAsync(
            "/api/employees",
            new { name = "Sam", phone = "555-0100", position = "Analyst", departmentId = _departmentId });
        var employee = await createResponse.Content.ReadFromJsonAsync<EmployeeResponse>()
            ?? throw new InvalidOperationException("The employee response had no body.");
        var missingDepartmentResponse = await _client.PutAsJsonAsync(
            $"/api/employees/{employee.Id}",
            new { name = employee.Name, phone = employee.Phone, position = employee.Position, departmentId = 999 });

        Assert.Equal(HttpStatusCode.NotFound, missingDepartmentResponse.StatusCode);
        using var missingDepartmentBody = JsonDocument.Parse(
            await missingDepartmentResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            "Department not found.",
            missingDepartmentBody.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Department_CanBeUpdatedAndDeleted_WhenItHasNoEmployees()
    {
        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/departments/{_departmentId}",
            new { name = "Product Engineering" });
        var updated = await updateResponse.Content.ReadFromJsonAsync<DepartmentResponse>();
        var deleteResponse = await _client.DeleteAsync($"/api/departments/{_departmentId}");
        var getResponse = await _client.GetAsync($"/api/departments/{_departmentId}");

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal("Product Engineering", updated?.Name);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DepartmentCannotBeDeletedWhileItHasEmployees()
    {
        await _client.PostAsJsonAsync(
            "/api/employees",
            new { name = "Sam", phone = "555-0100", position = "Analyst", departmentId = _departmentId });

        var response = await _client.DeleteAsync($"/api/departments/{_departmentId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Employee_CanBeDeleted()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/employees",
            new { name = "Sam", phone = "555-0100", position = "Analyst", departmentId = _departmentId });
        var employee = await createResponse.Content.ReadFromJsonAsync<EmployeeResponse>()
            ?? throw new InvalidOperationException("The employee response had no body.");
        var deleteResponse = await _client.DeleteAsync($"/api/employees/{employee.Id}");
        var getResponse = await _client.GetAsync($"/api/employees/{employee.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
