namespace Platform.DataAccess.Postgress;

public static class PlatformDatabaseConnection
{
    public const string ConnectionStringName = "Platform";
    public const string EnvironmentVariableName = "ConnectionStrings__Platform";

    public static string Require(string? connectionString)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
            return connectionString;

        throw new InvalidOperationException(
            $"Database connection string is not configured. " +
            $"Set {EnvironmentVariableName} or run 'source scripts/local-env.sh' from the repository root.");
    }

    public static string RequireFromEnvironment()
    {
        return Require(Environment.GetEnvironmentVariable(EnvironmentVariableName));
    }
}
