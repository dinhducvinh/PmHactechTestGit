using Microsoft.Data.SqlClient;

namespace HactechTest.Repositories;

internal static class KetNoiSql
{
    public static async Task<SqlConnection> MoAsync(string connectionString, CancellationToken ct = default)
    {
        var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        return conn;
    }
}
