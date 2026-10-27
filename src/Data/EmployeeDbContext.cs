using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Models;

namespace EmployeeManagement.Data;

public class EmployeeDbContext(DbContextOptions<EmployeeDbContext> options)
    : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Department> Departments => Set<Department>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.Property(department => department.Name)
                .HasMaxLength(100)
                .IsRequired();
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(employee => employee.Name)
                .HasMaxLength(150)
                .IsRequired();
            entity.Property(employee => employee.Phone)
                .HasMaxLength(30)
                .IsRequired();
            entity.Property(employee => employee.Position)
                .HasMaxLength(100)
                .IsRequired();
            entity.HasOne(employee => employee.Department)
                .WithMany(department => department.Employees)
                .HasForeignKey(employee => employee.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
