using HactechTest.Services.App;
using HactechTest.Services.Configuration;
using Microsoft.Data.SqlClient;

namespace HactechTest.Services.History
{
    public sealed class PhienChayStore
    {
        private readonly string _connectionString;

        public PhienChayStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> LuuPhienChayAsync(
            string cheDoChay,
            string cheDoLoi,
            string baseUrl,
            IReadOnlyList<ChiTietKetQuaTestCase> ketQua,
            CancellationToken ct = default)
        {
            await using var conn = await CauHinhUngDung.OpenConnectionAsync(_connectionString, ct);
            await using var tran = (SqlTransaction)await conn.BeginTransactionAsync(ct);

            var cheDoChayDb = string.Equals(cheDoChay, "selected", StringComparison.OrdinalIgnoreCase)
                ? "selected"
                : "all";
            var cheDoLoiDb = string.Equals(cheDoLoi, "stop_on_fail", StringComparison.OrdinalIgnoreCase)
                ? "stop_on_fail"
                : "continue_on_fail";

            int phienId;
            await using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText =
                    "INSERT INTO dbo.phien_chay " +
                    "(nguoi_chay, ten_may, ten_he_dieu_hanh, ten_bo_suu_tap, ten_mo_dun, " +
                    " che_do_chay, che_do_loi, base_url, " +
                    " tong_so_test, so_dat, so_khong_dat, ty_le_dat, thoi_gian_trung_binh_ms) " +
                    "OUTPUT INSERTED.ID " +
                    "VALUES (@nc, @tm, @os, @bst, @mod, @cdc, @cdl, @bu, @ts, @sd, @sk, @ty, @tb)";

                var tong = ketQua.Count;
                var dat = ketQua.Count(r => r.Result == "PASS");
                var khong = tong - dat;
                var ty = tong == 0 ? 0m : Math.Round(100m * dat / tong, 2);
                var tb = tong == 0 ? 0 : (int)ketQua.Average(r => r.DurationMs);

                cmd.Parameters.AddWithValue("@nc", LayTenDangNhapNguoiChay());
                cmd.Parameters.AddWithValue("@tm", Environment.MachineName);
                cmd.Parameters.AddWithValue("@os", Environment.OSVersion.VersionString);
                cmd.Parameters.AddWithValue("@bst", "API Shop");
                cmd.Parameters.AddWithValue("@mod", "RESTful API thương mại điện tử");
                cmd.Parameters.AddWithValue("@cdc", cheDoChayDb);
                cmd.Parameters.AddWithValue("@cdl", cheDoLoiDb);
                cmd.Parameters.AddWithValue("@bu", (object?)baseUrl ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ts", tong);
                cmd.Parameters.AddWithValue("@sd", dat);
                cmd.Parameters.AddWithValue("@sk", khong);
                cmd.Parameters.AddWithValue("@ty", ty);
                cmd.Parameters.AddWithValue("@tb", tb);

                phienId = (int)(await cmd.ExecuteScalarAsync(ct))!;
            }

            foreach (var r in ketQua)
            {
                await using var cmd = conn.CreateCommand();
                cmd.Transaction = tran;
                cmd.CommandText =
                    "INSERT INTO dbo.chi_tiet_phien_chay " +
                    "(ID_phien_chay, so_thu_tu, ten_test_case, http_method, url, " +
                    " ma_trang_thai_mong_doi, ma_trang_thai_thuc_te, ket_qua, thoi_gian_ms, " +
                    " ly_do, request_body, response_body) " +
                    "VALUES (@pc, @stt, @ten, @met, @url, @mong, @thuc, @kq, @tg, @ld, @rq, @rs)";
                cmd.Parameters.AddWithValue("@pc", phienId);
                cmd.Parameters.AddWithValue("@stt", r.SequenceNo);
                cmd.Parameters.AddWithValue("@ten", r.DisplayName);
                cmd.Parameters.AddWithValue("@met", r.HttpMethod);
                cmd.Parameters.AddWithValue("@url", r.Url);
                cmd.Parameters.AddWithValue("@mong", (object?)r.ExpectedAppCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@thuc", LayMaThucTe(r));
                cmd.Parameters.AddWithValue("@kq", r.Result);
                cmd.Parameters.AddWithValue("@tg", (int)r.DurationMs);
                cmd.Parameters.AddWithValue("@ld", (object?)r.Reason ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@rq", (object?)r.RequestBodyJson ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@rs", (object?)r.ResponseBody ?? DBNull.Value);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            await tran.CommitAsync(ct);
            return phienId;
        }

        public async Task<List<PhienChayDaLuu>> LayDanhSachAsync(string keyword, CancellationToken ct = default)
        {
            var tomTat = new List<PhienChayTomTat>();
            await using var conn = await CauHinhUngDung.OpenConnectionAsync(_connectionString, ct);
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = """
                    SELECT TOP 200
                        ID,
                        thoi_diem_chay,
                        ISNULL(nguoi_chay, N''),
                        ISNULL(ten_may, N''),
                        ISNULL(ten_he_dieu_hanh, N''),
                        ISNULL(base_url, N''),
                        ISNULL(che_do_chay, N''),
                        ISNULL(che_do_loi, N''),
                        tong_so_test,
                        so_dat,
                        so_khong_dat,
                        ty_le_dat,
                        thoi_gian_trung_binh_ms
                    FROM dbo.phien_chay
                    WHERE (@keyword IS NULL
                        OR nguoi_chay LIKE @keyword
                        OR ten_may LIKE @keyword
                        OR ten_he_dieu_hanh LIKE @keyword
                        OR base_url LIKE @keyword)
                    ORDER BY thoi_diem_chay DESC, ID DESC;
                    """;
                cmd.Parameters.AddWithValue("@keyword", string.IsNullOrWhiteSpace(keyword)
                    ? DBNull.Value
                    : "%" + keyword.Trim() + "%");

                await using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    tomTat.Add(new PhienChayTomTat(
                        reader.GetInt32(0),
                        reader.GetDateTime(1),
                        reader.GetString(2),
                        reader.GetString(3),
                        reader.GetString(4),
                        reader.GetString(5),
                        reader.GetString(6),
                        reader.GetString(7),
                        reader.GetInt32(8),
                        reader.GetInt32(9),
                        reader.GetInt32(10),
                        reader.GetDecimal(11),
                        reader.GetInt32(12)));
                }
            }

            var ketQua = new List<PhienChayDaLuu>();
            foreach (var item in tomTat)
            {
                ketQua.Add(new PhienChayDaLuu(
                    item.Id,
                    item.RunAt,
                    item.NguoiChay,
                    item.Machine,
                    item.OperatingSystem,
                    item.BaseUrl,
                    item.CheDoChay,
                    item.CheDoLoi,
                    item.TotalTests,
                    item.PassedTests,
                    item.FailedTests,
                    item.PassRate,
                    item.AverageDurationMs,
                    []));
            }

            return ketQua;
        }

        public async Task<List<ChiTietPhienChayDaLuu>> LayChiTietAsync(int phienChayId, CancellationToken ct = default)
        {
            if (phienChayId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(phienChayId), "ID phiên chạy không hợp lệ.");
            }

            var details = new List<ChiTietPhienChayDaLuu>();
            await using var conn = await CauHinhUngDung.OpenConnectionAsync(_connectionString, ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT
                    so_thu_tu,
                    ISNULL(ten_test_case, N''),
                    ISNULL(http_method, N''),
                    ISNULL(url, N''),
                    ISNULL(ma_trang_thai_mong_doi, N''),
                    ket_qua,
                    ISNULL(ma_trang_thai_thuc_te, N''),
                    thoi_gian_ms,
                    ISNULL(ly_do, N''),
                    ISNULL(request_body, N''),
                    ISNULL(response_body, N'')
                FROM dbo.chi_tiet_phien_chay
                WHERE ID_phien_chay = @id
                ORDER BY so_thu_tu;
                """;
            cmd.Parameters.AddWithValue("@id", phienChayId);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                details.Add(new ChiTietPhienChayDaLuu(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(5),
                    reader.GetString(6),
                    reader.GetInt32(7),
                    reader.GetString(8),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetString(9),
                    reader.GetString(10)));
            }

            return details;
        }

        public async Task XoaPhienAsync(int phienChayId, CancellationToken ct = default)
        {
            if (phienChayId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(phienChayId), "ID phiên chạy không hợp lệ.");
            }

            await using var conn = await CauHinhUngDung.OpenConnectionAsync(_connectionString, ct);
            await using var tran = (SqlTransaction)await conn.BeginTransactionAsync(ct);

            await using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = """
                    DELETE FROM dbo.chi_tiet_phien_chay
                    WHERE ID_phien_chay = @id;

                    DELETE FROM dbo.phien_chay
                    WHERE ID = @id;
                    """;
                cmd.Parameters.AddWithValue("@id", phienChayId);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            await tran.CommitAsync(ct);
        }

        private static object LayMaThucTe(ChiTietKetQuaTestCase r)
        {
            if (!string.IsNullOrWhiteSpace(r.ActualAppCode))
            {
                return r.ActualAppCode;
            }

            return r.HttpStatus.HasValue ? $"HTTP_{r.HttpStatus.Value}" : DBNull.Value;
        }

        private static string LayTenDangNhapNguoiChay()
        {
            if (AppHost.IsInitialized && AppHost.Instance.TaiKhoanDangNhap is { } taiKhoan)
            {
                return taiKhoan.TenDangNhap;
            }

            return Environment.UserName;
        }

        private sealed record PhienChayTomTat(
            int Id,
            DateTime RunAt,
            string NguoiChay,
            string Machine,
            string OperatingSystem,
            string BaseUrl,
            string CheDoChay,
            string CheDoLoi,
            int TotalTests,
            int PassedTests,
            int FailedTests,
            decimal PassRate,
            int AverageDurationMs);
    }

    public sealed record PhienChayDaLuu(
        int Id,
        DateTime RunAt,
        string NguoiChay,
        string Machine,
        string OperatingSystem,
        string BaseUrl,
        string CheDoChay,
        string CheDoLoi,
        int TotalTests,
        int PassedTests,
        int FailedTests,
        decimal PassRate,
        int AverageDurationMs,
        List<ChiTietPhienChayDaLuu> Details);

    public sealed record ChiTietPhienChayDaLuu(
        int SequenceNo,
        string TestCaseName,
        string Result,
        string ActualStatus,
        int DurationMs,
        string Reason,
        string HttpMethod = "",
        string Url = "",
        string ExpectedStatus = "",
        string RequestBody = "",
        string ResponseBody = "");
}
