using System.IO;
using Microsoft.Data.SqlClient;

namespace ApiTest.Services
{
    public sealed class Database
    {
        private readonly DatabaseConfig _config;

        public Database(DatabaseConfig config)
        {
            _config = config;
        }

        public string ConnectionString => _config.ConnectionString;

        public async Task<bool> TestConnectionAsync(CancellationToken ct = default)
        {
            try
            {
                await using var conn = new SqlConnection(_config.ConnectionString);
                await conn.OpenAsync(ct);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Đảm bảo database `ApiTesterDb` tồn tại bằng cách chạy `CreateDatabase.sql`
        /// nếu kết nối hiện tại không thành công. Trả về thông điệp mô tả.
        /// </summary>
        public async Task<string> EnsureDatabaseAsync(CancellationToken ct = default)
        {
            if (await TestConnectionAsync(ct))
            {
                return "Đã kết nối CSDL ApiTesterDb.";
            }

            var sqlPath = Path.Combine(AppContext.BaseDirectory, "Database", "CreateDatabase.sql");
            if (!File.Exists(sqlPath))
            {
                throw new FileNotFoundException(
                    $"Không tìm thấy CreateDatabase.sql tại {sqlPath}. " +
                    "Hãy chắc chắn file đã được copy vào output (PreserveNewest).");
            }

            var sqlText = await File.ReadAllTextAsync(sqlPath, ct);

            await using var conn = new SqlConnection(_config.MasterConnectionString);
            await conn.OpenAsync(ct);

            foreach (var batch in SplitGoBatches(sqlText))
            {
                if (string.IsNullOrWhiteSpace(batch)) continue;
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = batch;
                cmd.CommandTimeout = 60;
                await cmd.ExecuteNonQueryAsync(ct);
            }

            return "Đã tạo lại CSDL ApiTesterDb từ CreateDatabase.sql.";
        }

        private static IEnumerable<string> SplitGoBatches(string sqlText)
        {
            var lines = sqlText.Split('\n');
            var current = new System.Text.StringBuilder();

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r');
                if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
                {
                    yield return current.ToString();
                    current.Clear();
                    continue;
                }
                current.AppendLine(line);
            }

            if (current.Length > 0)
            {
                yield return current.ToString();
            }
        }

        public SqlConnection OpenConnection()
        {
            var conn = new SqlConnection(_config.ConnectionString);
            conn.Open();
            return conn;
        }

        public async Task<SqlConnection> OpenConnectionAsync(CancellationToken ct = default)
        {
            var conn = new SqlConnection(_config.ConnectionString);
            await conn.OpenAsync(ct);
            return conn;
        }
    }

    public sealed class SeedAccountStore
    {
        private readonly Database _db;
        private readonly TestDataConfig _testData;

        public SeedAccountStore(Database db, TestDataConfig testData)
        {
            _db = db;
            _testData = testData;
        }

        /// <summary>
        /// Đảm bảo có ít nhất `mucToiThieu` tài khoản trạng thái `chua_dang_ky`. Nếu thiếu, sinh thêm.
        /// </summary>
        public async Task<int> EnsureUnregisteredSeedAsync(int mucToiThieu, CancellationToken ct = default)
        {
            await using var conn = await _db.OpenConnectionAsync(ct);

            int dangCo;
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT COUNT(*) FROM dbo.tai_khoan_test " +
                    "WHERE loai_tai_khoan = N'sdt' AND trang_thai = N'chua_dang_ky'";
                dangCo = (int)(await cmd.ExecuteScalarAsync(ct) ?? 0);
            }

            if (dangCo >= mucToiThieu)
            {
                return 0;
            }

            int maxStt;
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT ISNULL(MAX(so_thu_tu_sinh), 0) FROM dbo.tai_khoan_test";
                maxStt = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct) ?? 0);
            }

            var canThem = mucToiThieu - dangCo;
            int daThem = 0;
            int stt = maxStt;

            while (daThem < canThem)
            {
                stt++;
                var phone = SinhSoDienThoai(stt);

                bool trungTaiKhoan;
                await using (var checkCmd = conn.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT COUNT(*) FROM dbo.tai_khoan_test WHERE tai_khoan = @tk";
                    checkCmd.Parameters.AddWithValue("@tk", phone);
                    trungTaiKhoan = (int)(await checkCmd.ExecuteScalarAsync(ct) ?? 0) > 0;
                }
                if (trungTaiKhoan) continue;

                await using var insert = conn.CreateCommand();
                insert.CommandText =
                    "INSERT INTO dbo.tai_khoan_test " +
                    "(so_thu_tu_sinh, loai_tai_khoan, tai_khoan, mat_khau, trang_thai) " +
                    "VALUES (@stt, N'sdt', @tk, @mk, N'chua_dang_ky')";
                insert.Parameters.AddWithValue("@stt", stt);
                insert.Parameters.AddWithValue("@tk", phone);
                insert.Parameters.AddWithValue("@mk", _testData.DefaultPassword);
                await insert.ExecuteNonQueryAsync(ct);

                daThem++;
            }

            return daThem;
        }

        public async Task<SeedAccount?> LayMotTaiKhoanChuaDangKyAsync(CancellationToken ct = default)
        {
            await using var conn = await _db.OpenConnectionAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT TOP 1 ID, so_thu_tu_sinh, tai_khoan, mat_khau, trang_thai, ID_nguoi_dung_server " +
                "FROM dbo.tai_khoan_test " +
                "WHERE loai_tai_khoan = N'sdt' AND trang_thai = N'chua_dang_ky' " +
                "ORDER BY so_thu_tu_sinh ASC";

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct)) return null;
            return DocSeedTuReader(reader);
        }

        public async Task<SeedAccount?> LayMotTaiKhoanDaDangKyAsync(CancellationToken ct = default)
        {
            await using var conn = await _db.OpenConnectionAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT TOP 1 ID, so_thu_tu_sinh, tai_khoan, mat_khau, trang_thai, ID_nguoi_dung_server " +
                "FROM dbo.tai_khoan_test " +
                "WHERE loai_tai_khoan = N'sdt' AND trang_thai = N'da_dang_ky' " +
                "ORDER BY thoi_diem_cap_nhat DESC";

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct)) return null;
            return DocSeedTuReader(reader);
        }

        public async Task DanhDauDaDangKyAsync(SeedAccount account, string? serverUserId, CancellationToken ct = default)
        {
            await using var conn = await _db.OpenConnectionAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText =
                "UPDATE dbo.tai_khoan_test " +
                "SET trang_thai = N'da_dang_ky', " +
                "    ID_nguoi_dung_server = @sid, " +
                "    thoi_diem_cap_nhat = SYSDATETIME() " +
                "WHERE ID = @id";
            cmd.Parameters.AddWithValue("@sid", (object?)serverUserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", account.Id);
            await cmd.ExecuteNonQueryAsync(ct);

            account.Status = SeedAccountStatus.DaDangKy;
            account.ServerUserId = serverUserId;
        }

        public async Task CapNhatMatKhauAsync(SeedAccount account, string matKhauMoi, CancellationToken ct = default)
        {
            await using var conn = await _db.OpenConnectionAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText =
                "UPDATE dbo.tai_khoan_test " +
                "SET mat_khau = @mk, thoi_diem_cap_nhat = SYSDATETIME() " +
                "WHERE ID = @id";
            cmd.Parameters.AddWithValue("@mk", matKhauMoi);
            cmd.Parameters.AddWithValue("@id", account.Id);
            await cmd.ExecuteNonQueryAsync(ct);
            account.Password = matKhauMoi;
        }

        private string SinhSoDienThoai(int stt)
        {
            var prefix = string.IsNullOrWhiteSpace(_testData.PhoneNumberPrefix) ? "099" : _testData.PhoneNumberPrefix;
            var so = stt.ToString().PadLeft(10 - prefix.Length, '0');
            return prefix + so;
        }

        private static SeedAccount DocSeedTuReader(SqlDataReader reader)
        {
            return new SeedAccount
            {
                Id = reader.GetInt32(0),
                SoThuTuSinh = reader.GetInt32(1),
                PhoneNumber = reader.GetString(2),
                Password = reader.GetString(3),
                Status = ChuyenSangStatus(reader.GetString(4)),
                ServerUserId = reader.IsDBNull(5) ? null : reader.GetString(5)
            };
        }

        private static SeedAccountStatus ChuyenSangStatus(string s) => s switch
        {
            "chua_dang_ky" => SeedAccountStatus.ChuaDangKy,
            "cho_xac_nhan_otp" => SeedAccountStatus.ChoXacNhanOtp,
            "da_dang_ky" => SeedAccountStatus.DaDangKy,
            _ => SeedAccountStatus.ChuaDangKy
        };
    }

    public sealed class PhienChayStore
    {
        private readonly Database _db;

        public PhienChayStore(Database db)
        {
            _db = db;
        }

        public async Task<int> LuuPhienChayAsync(
            string cheDoChay, string cheDoLoi, string baseUrl,
            IReadOnlyList<TestRunResult> ketQua,
            CancellationToken ct = default)
        {
            await using var conn = await _db.OpenConnectionAsync(ct);
            await using var tran = (Microsoft.Data.SqlClient.SqlTransaction)await conn.BeginTransactionAsync(ct);

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

                cmd.Parameters.AddWithValue("@nc", (object?)Environment.UserName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tm", (object?)Environment.MachineName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@os", (object?)Environment.OSVersion.VersionString ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@bst", "UAV API");
                cmd.Parameters.AddWithValue("@mod", "Xác thực & Tài khoản");
                cmd.Parameters.AddWithValue("@cdc", cheDoChay);
                cmd.Parameters.AddWithValue("@cdl", cheDoLoi);
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
                cmd.Parameters.AddWithValue("@mong", DBNull.Value);
                cmd.Parameters.AddWithValue("@thuc", (object?)r.HttpStatus ?? DBNull.Value);
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
    }
}
