using BCryptNet = BCrypt.Net.BCrypt;
using employeesAPI.Models;
using System.Data;
using System.Data.SqlClient;

namespace employeesAPI.Data;

public interface IEmployeesRepository
{
    Task<IReadOnlyList<Employee>> GetEmployeesAsync(CancellationToken cancellationToken = default);
    Task<Employee> CreateEmployeeAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<Employee?> UpdateEmployeeAsync(int employeeId, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteEmployeeAsync(int employeeId, CancellationToken cancellationToken = default);
}

public sealed class SqlEmployeesRepository : IEmployeesRepository
{
    private readonly string _connectionString;

    public SqlEmployeesRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
    }

    public async Task<IReadOnlyList<Employee>> GetEmployeesAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        //await using var command = connection.CreateCommand();
        //command.CommandText = "SELECT EmployeeId, FirstName, LastName, Email, HireDate, IsActive, CreatedAt FROM Employees";

        await using var command = connection.CreateCommand();
        command.CommandText = "sp_GetAllEmployees";
        command.CommandType = CommandType.StoredProcedure;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var results = new List<Employee>();

        var employeeIdOrdinal = reader.GetOrdinal("EmployeeId");
        var firstNameOrdinal = reader.GetOrdinal("FirstName");
        var lastNameOrdinal = reader.GetOrdinal("LastName");
        var emailOrdinal = reader.GetOrdinal("Email");
        var hireDateOrdinal = reader.GetOrdinal("HireDate");
        var isActiveOrdinal = reader.GetOrdinal("IsActive");
        var createdAtOrdinal = reader.GetOrdinal("CreatedAt");

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(MapEmployee(reader, employeeIdOrdinal, firstNameOrdinal, lastNameOrdinal,
                emailOrdinal, hireDateOrdinal, isActiveOrdinal, createdAtOrdinal));
        }

        return results;
    }

    public async Task<Employee> CreateEmployeeAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
//        command.CommandText = @"
//INSERT INTO Employees (FirstName, LastName, Email, HireDate, IsActive, CreatedAt)
//OUTPUT INSERTED.EmployeeId, INSERTED.FirstName, INSERTED.LastName, INSERTED.Email,
//       INSERTED.HireDate, INSERTED.IsActive, INSERTED.CreatedAt
//VALUES (@FirstName, @LastName, @Email, @HireDate, @IsActive, @CreatedAt);";

      
        command.CommandText = "sp_InsertEmployee";
        command.CommandType = CommandType.StoredProcedure;

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("A password is required to create an employee.", nameof(request.Password));
        }

        var hashedPassword = BCryptNet.HashPassword(request.Password);

        command.Parameters.AddWithValue("@FirstName", request.FirstName);
        command.Parameters.AddWithValue("@LastName", request.LastName);
        command.Parameters.AddWithValue("@Email", (object?)request.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("@HireDate", request.HireDate);
        command.Parameters.AddWithValue("@IsActive", (object?)request.IsActive ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAt", (object?)request.CreatedAt ?? DateTime.UtcNow);
        command.Parameters.AddWithValue("@Password", hashedPassword);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return MapEmployee(reader);
        }

        throw new InvalidOperationException("Could not insert employee.");
    }

    public async Task<Employee?> UpdateEmployeeAsync(int employeeId, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        //        command.CommandText = @"
        //UPDATE Employees
        //SET FirstName = @FirstName,
        //    LastName = @LastName,
        //    Email = @Email,
        //    HireDate = @HireDate,
        //    IsActive = @IsActive,
        //    CreatedAt = @CreatedAt
        //OUTPUT INSERTED.EmployeeId, INSERTED.FirstName, INSERTED.LastName, INSERTED.Email,
        //       INSERTED.HireDate, INSERTED.IsActive, INSERTED.CreatedAt
        //WHERE EmployeeId = @EmployeeId;";

        command.CommandText = "sp_UpdateEmployee";
        command.CommandType = CommandType.StoredProcedure;

        string? hashedPassword = null;
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            hashedPassword = BCryptNet.HashPassword(request.Password);
        }

        command.Parameters.AddWithValue("@EmployeeId", employeeId);
        command.Parameters.AddWithValue("@FirstName", request.FirstName);
        command.Parameters.AddWithValue("@LastName", request.LastName);
        command.Parameters.AddWithValue("@Email", (object?)request.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("@HireDate", request.HireDate);
        command.Parameters.AddWithValue("@IsActive", (object?)request.IsActive ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAt", (object?)request.CreatedAt ?? DBNull.Value);
        command.Parameters.AddWithValue("@Password", (object?)hashedPassword ?? DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return MapEmployee(reader);
        }

        return null;
    }

    public async Task<bool> DeleteEmployeeAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        //command.CommandText = "DELETE FROM Employees WHERE EmployeeId = @EmployeeId";
        command.CommandText = "sp_DeleteEmployee";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@EmployeeId", employeeId);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        return affected > 0;
    }

    private static Employee MapEmployee(SqlDataReader reader)
    {
        var employeeIdOrdinal = reader.GetOrdinal("EmployeeId");
        var firstNameOrdinal = reader.GetOrdinal("FirstName");
        var lastNameOrdinal = reader.GetOrdinal("LastName");
        var emailOrdinal = reader.GetOrdinal("Email");
        var hireDateOrdinal = reader.GetOrdinal("HireDate");
        var isActiveOrdinal = reader.GetOrdinal("IsActive");
        var createdAtOrdinal = reader.GetOrdinal("CreatedAt");

        return MapEmployee(reader, employeeIdOrdinal, firstNameOrdinal, lastNameOrdinal, emailOrdinal,
            hireDateOrdinal, isActiveOrdinal, createdAtOrdinal);
    }

    private static Employee MapEmployee(SqlDataReader reader, int employeeIdOrdinal, int firstNameOrdinal,
        int lastNameOrdinal, int emailOrdinal, int hireDateOrdinal, int isActiveOrdinal, int createdAtOrdinal)
    {
        return new Employee
        {
            EmployeeId = reader.GetInt32(employeeIdOrdinal),
            FirstName = reader.GetString(firstNameOrdinal),
            LastName = reader.GetString(lastNameOrdinal),
            Email = reader.IsDBNull(emailOrdinal) ? null : reader.GetString(emailOrdinal),
            HireDate = reader.GetDateTime(hireDateOrdinal),
            IsActive = reader.IsDBNull(isActiveOrdinal) ? null : reader.GetBoolean(isActiveOrdinal),
            CreatedAt = reader.IsDBNull(createdAtOrdinal) ? null : reader.GetDateTime(createdAtOrdinal)
        };
    }
}
