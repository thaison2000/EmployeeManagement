# Employee Management

Ứng dụng quản lý nhân viên và phòng ban, xây dựng bằng ASP.NET Core 10, Entity Framework Core và PostgreSQL trong một project ứng dụng.

## Chức năng

- Quản lý phòng ban: xem, thêm, sửa, xóa.
- Quản lý nhân viên: xem, thêm, sửa, xóa; lọc theo phòng ban.
- Mỗi nhân viên thuộc một phòng ban. Không thể xóa phòng ban khi phòng ban đó còn nhân viên.
- Ứng dụng không có chức năng đăng nhập/đăng ký; API không yêu cầu xác thực.

## Cấu trúc database

Database gồm hai bảng nghiệp vụ:

- `Departments`: `Id`, `Name`.
- `Employees`: `Id`, `Name`, `Phone`, `Position`, `DepartmentId`.

`Employees.DepartmentId` là khóa ngoại đến `Departments.Id`.

## Yêu cầu và chạy ứng dụng

- .NET 10 SDK
- Mã nguồn ứng dụng nằm trực tiếp trong `src`, chia theo `Controllers`, `DTO`, `Models`, `Repositories` và `Data`.

Chạy các lệnh từ thư mục gốc `EmployeeManagement`:

```powershell
dotnet restore .\EmployeeManagement.sln
dotnet build .\EmployeeManagement.sln
dotnet test .\EmployeeManagement.sln
dotnet run --project .\src\EmployeeManagement.csproj
```

Ứng dụng dùng PostgreSQL và tự áp dụng migration `InitialPostgreSqlSchema` khi khởi động. Migration tạo bảng `Departments` và `Employees`.

### Cấu hình PostgreSQL

Cài đặt PostgreSQL và tạo database rỗng tên `employee_management`. Đặt connection string bên ngoài source code bằng một trong các cách sau.

PowerShell cho terminal hiện tại:

```powershell
$env:ConnectionStrings__EmployeeDatabase = "Host=localhost;Port=5432;Database=employee_management;Username=postgres;Password=your-password"
```

Hoặc lưu an toàn bằng .NET User Secrets cho môi trường phát triển:

```powershell
dotnet user-secrets set "ConnectionStrings:EmployeeDatabase" "Host=localhost;Port=5432;Database=employee_management;Username=postgres;Password=your-password" --project .\src\EmployeeManagement.csproj
```

Xem các User Secrets đã lưu cho project:

```powershell
dotnet user-secrets list --project .\src\EmployeeManagement.csproj
```

Khi khởi động, ứng dụng áp dụng migration PostgreSQL và tạo hai bảng `Departments` và `Employees`. Integration tests vẫn dùng SQLite in-memory riêng.

Trang quản lý nhân sự ở `/`; Swagger UI ở `/swagger` trong môi trường Development.

## API

### Departments

- `GET /api/departments`
- `GET /api/departments/{id}`
- `POST /api/departments` với body `{ "name": "Engineering" }`
- `PUT /api/departments/{id}` với body `{ "name": "Product Engineering" }`
- `DELETE /api/departments/{id}` — trả `409 Conflict` nếu phòng ban còn nhân viên.

### Employees

- `GET /api/employees`
- `GET /api/employees/{id}`
- `GET /api/employees?departmentId={id}`
- `POST /api/employees` với body:

```json
{
  "name": "Nguyen Van An",
  "phone": "0901234567",
  "position": "Developer",
  "departmentId": 1
}
```

- `PUT /api/employees/{id}` với cùng các trường.
- `DELETE /api/employees/{id}`

Các request ví dụ đầy đủ nằm trong `src/EmployeeManagement.http`.

## Migrations

Các lệnh EF Core dưới đây dùng để quản lý migration thủ công khi cần:

```powershell
# Khôi phục các .NET tool được khai báo trong manifest của repository.
dotnet tool restore

# Liệt kê các migration hiện có trong project.
dotnet ef migrations list --project .\src\EmployeeManagement.csproj

# Tạo migration mới từ các thay đổi trong model; thay MigrationName bằng tên phù hợp.
dotnet ef migrations add MigrationName --project .\src\EmployeeManagement.csproj

# Áp dụng các migration còn thiếu vào database được cấu hình.
dotnet ef database update --project .\src\EmployeeManagement.csproj
```
