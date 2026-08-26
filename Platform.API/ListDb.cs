using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Platform.DataAccess.Postgress;
using Platform.DataAccess.Postgress.Lms;

namespace Platform.API;

public sealed class ListDbConnection
{
    private readonly string _connectionString;

    public ListDbConnection()
        : this(PlatformDatabaseConnection.RequireFromEnvironment())
    {
    }

    public ListDbConnection(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required.", nameof(connectionString));

        _connectionString = connectionString;
    }

    public void connect()
    {
    }

    public async Task<string> GetUserDataJsonAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var options = new DbContextOptionsBuilder<LocalLmsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;
        await using var db = new LocalLmsDbContext(options);

        var student = await db.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken)
            .ConfigureAwait(false);

        if (student is null)
            throw new InvalidOperationException($"Student with id {studentId} was not found.");

        return JsonSerializer.Serialize(
            new
            {
                id = student.Id,
                name = student.Name,
                surname = student.Surname,
                group = student.Group,
            },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public string getUserData(Guid id) => GetUserDataJsonAsync(id).GetAwaiter().GetResult();
}
