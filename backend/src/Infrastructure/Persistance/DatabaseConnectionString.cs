using Npgsql;

namespace Infrastructure.Persistance;

public static class DatabaseConnectionString
{
    /// <summary>
    /// Time zone of every database session. Dates are stored as "timestamp without
    /// time zone" in Europe/Madrid wall-clock time (the API also runs in that zone),
    /// so NOW(), date_part() and any implicit conversion must use the same zone on
    /// every environment instead of each server's default.
    /// </summary>
    public const string SessionTimeZone = "Europe/Madrid";

    public static string WithSessionTimeZone(string connectionString) =>
        new NpgsqlConnectionStringBuilder(connectionString) { Timezone = SessionTimeZone }.ConnectionString;
}
