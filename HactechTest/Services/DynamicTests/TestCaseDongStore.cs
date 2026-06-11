using HactechTest.Services.Configuration;
using Microsoft.Data.SqlClient;

namespace HactechTest.Services.DynamicTests
{
    public sealed class TestCaseDong
    {
        public int Id { get; init; }
        public string Ma { get; init; } = "";
        public string Nhom { get; init; } = "Custom";
        public string TenHienThi { get; init; } = "";
        public string MoTa { get; init; } = "";
        public string HttpMethod { get; init; } = "GET";
        public string Endpoint { get; init; } = "";
        public string AuthMode { get; init; } = "none";
        public string? PathParamsJson { get; init; }
        public string? HeadersJson { get; init; }
        public string? BodyJson { get; init; }
        public string? ExpectedCodes { get; init; }
        public int? ExpectedHttpStatus { get; init; }
        public string? ExpectedJsonPath { get; init; }
        public string? ExpectedJsonValue { get; init; }
    }

    public sealed class TestCaseDongStore
    {
        private readonly string _connectionString;

        public TestCaseDongStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<TestCaseDong>> LayDanhSachAsync(CancellationToken ct = default)
        {
            var result = new List<TestCaseDong>();
            await using var conn = await CauHinhUngDung.OpenConnectionAsync(_connectionString, ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT
                    id, ma, nhom, ten_hien_thi, ISNULL(mo_ta, N''),
                    http_method, endpoint, auth_mode,
                    path_params_json, headers_json, body_json, expected_codes, expected_http_status,
                    expected_json_path, expected_json_value
                FROM dbo.test_case_dong
                ORDER BY nhom, ma, id;
                """;

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                result.Add(DocTestCase(reader));
            }

            return result;
        }

        public async Task<TestCaseDong?> LayTheoIdAsync(int id, CancellationToken ct = default)
        {
            await using var conn = await CauHinhUngDung.OpenConnectionAsync(_connectionString, ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT TOP 1
                    id, ma, nhom, ten_hien_thi, ISNULL(mo_ta, N''),
                    http_method, endpoint, auth_mode,
                    path_params_json, headers_json, body_json, expected_codes, expected_http_status,
                    expected_json_path, expected_json_value
                FROM dbo.test_case_dong
                WHERE id = @id;
                """;
            cmd.Parameters.AddWithValue("@id", id);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? DocTestCase(reader) : null;
        }

        public async Task<int> LuuAsync(TestCaseDong testCase, CancellationToken ct = default)
        {
            await using var conn = await CauHinhUngDung.OpenConnectionAsync(_connectionString, ct);
            await using var cmd = conn.CreateCommand();

            if (testCase.Id == 0)
            {
                cmd.CommandText = """
                    INSERT INTO dbo.test_case_dong
                    (ma, nhom, ten_hien_thi, mo_ta, http_method, endpoint, auth_mode,
                     path_params_json, headers_json, body_json, expected_codes, expected_http_status,
                     expected_json_path, expected_json_value)
                    OUTPUT INSERTED.id
                    VALUES
                    (@ma, @nhom, @ten, @mo_ta, @method, @endpoint, @auth,
                     @path_params, @headers, @body, @codes, @http_status,
                     @json_path, @json_value);
                    """;
            }
            else
            {
                cmd.CommandText = """
                    UPDATE dbo.test_case_dong
                    SET ma = @ma,
                        nhom = @nhom,
                        ten_hien_thi = @ten,
                        mo_ta = @mo_ta,
                        http_method = @method,
                        endpoint = @endpoint,
                        auth_mode = @auth,
                        path_params_json = @path_params,
                        headers_json = @headers,
                        body_json = @body,
                        expected_codes = @codes,
                        expected_http_status = @http_status,
                        expected_json_path = @json_path,
                        expected_json_value = @json_value,
                        cap_nhat_luc = SYSDATETIME()
                    WHERE id = @id;
                    SELECT @id;
                    """;
                cmd.Parameters.AddWithValue("@id", testCase.Id);
            }

            GanThamSo(cmd, testCase);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
        }

        public async Task XoaAsync(int id, CancellationToken ct = default)
        {
            await using var conn = await CauHinhUngDung.OpenConnectionAsync(_connectionString, ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM dbo.test_case_dong WHERE id = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        private static TestCaseDong DocTestCase(SqlDataReader reader)
        {
            return new TestCaseDong
            {
                Id = reader.GetInt32(0),
                Ma = reader.GetString(1),
                Nhom = reader.GetString(2),
                TenHienThi = reader.GetString(3),
                MoTa = reader.GetString(4),
                HttpMethod = reader.GetString(5),
                Endpoint = reader.GetString(6),
                AuthMode = reader.GetString(7),
                PathParamsJson = reader.IsDBNull(8) ? null : reader.GetString(8),
                HeadersJson = reader.IsDBNull(9) ? null : reader.GetString(9),
                BodyJson = reader.IsDBNull(10) ? null : reader.GetString(10),
                ExpectedCodes = reader.IsDBNull(11) ? null : reader.GetString(11),
                ExpectedHttpStatus = reader.IsDBNull(12) ? null : reader.GetInt32(12),
                ExpectedJsonPath = reader.IsDBNull(13) ? null : reader.GetString(13),
                ExpectedJsonValue = reader.IsDBNull(14) ? null : reader.GetString(14)
            };
        }

        private static void GanThamSo(SqlCommand cmd, TestCaseDong testCase)
        {
            cmd.Parameters.AddWithValue("@ma", testCase.Ma);
            cmd.Parameters.AddWithValue("@nhom", testCase.Nhom);
            cmd.Parameters.AddWithValue("@ten", testCase.TenHienThi);
            cmd.Parameters.AddWithValue("@mo_ta", (object?)testCase.MoTa ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@method", testCase.HttpMethod);
            cmd.Parameters.AddWithValue("@endpoint", testCase.Endpoint);
            cmd.Parameters.AddWithValue("@auth", testCase.AuthMode);
            cmd.Parameters.AddWithValue("@path_params", (object?)testCase.PathParamsJson ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@headers", (object?)testCase.HeadersJson ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@body", (object?)testCase.BodyJson ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@codes", (object?)testCase.ExpectedCodes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@http_status", (object?)testCase.ExpectedHttpStatus ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@json_path", (object?)testCase.ExpectedJsonPath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@json_value", (object?)testCase.ExpectedJsonValue ?? DBNull.Value);
        }
    }
}
