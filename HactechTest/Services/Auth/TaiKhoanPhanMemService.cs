using System.Security.Cryptography;
using System.Text;
using HactechTest.Models.Auth;
using HactechTest.Services.Configuration;
using Microsoft.Data.SqlClient;

namespace HactechTest.Services.Auth
{
    public sealed class TaiKhoanPhanMemService
    {
        private readonly string _connectionString;

        public TaiKhoanPhanMemService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<TaiKhoanPhanMem?> DangNhapAsync(
            string tenDangNhap,
            string matKhau,
            CancellationToken ct = default)
        {
            tenDangNhap = tenDangNhap.Trim();
            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau))
            {
                return null;
            }

            await using var conn = await CauHinhUngDung.OpenConnectionAsync(_connectionString, ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT TOP 1
                    id, ten_dang_nhap, mat_khau_hash, mat_khau_salt,
                    ho_ten, email, so_dien_thoai, vai_tro, trang_thai,
                    tao_luc, cap_nhat_luc, dang_nhap_cuoi_luc
                FROM dbo.taikhoan_phanmemtest
                WHERE ten_dang_nhap = @ten_dang_nhap
                  AND trang_thai = N'hoat_dong';
                """;
            cmd.Parameters.AddWithValue("@ten_dang_nhap", tenDangNhap);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                return null;
            }

            var salt = reader.GetString(3);
            var hashDaLuu = reader.GetString(2);
            if (!string.Equals(hashDaLuu, HashMatKhau(matKhau, salt), StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var taiKhoan = DocTaiKhoan(reader);
            await reader.CloseAsync();
            await CapNhatLanDangNhapAsync(conn, taiKhoan.Id, ct);
            return taiKhoan with { DangNhapCuoiLuc = DateTimeOffset.Now };
        }

        public async Task<IReadOnlyList<TaiKhoanPhanMem>> LayDanhSachAsync(CancellationToken ct = default)
        {
            var ketQua = new List<TaiKhoanPhanMem>();
            await using var conn = await CauHinhUngDung.OpenConnectionAsync(_connectionString, ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT
                    id, ten_dang_nhap, mat_khau_hash, mat_khau_salt,
                    ho_ten, email, so_dien_thoai, vai_tro, trang_thai,
                    tao_luc, cap_nhat_luc, dang_nhap_cuoi_luc
                FROM dbo.taikhoan_phanmemtest
                ORDER BY
                    CASE WHEN vai_tro = N'admin' THEN 0 ELSE 1 END,
                    ten_dang_nhap;
                """;

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                ketQua.Add(DocTaiKhoan(reader));
            }

            return ketQua;
        }

        public async Task TaoTaiKhoanAsync(TaoTaiKhoanPhanMemRequest request, CancellationToken ct = default)
        {
            KiemTraDuLieuTaiKhoan(request.TenDangNhap, request.MatKhau, request.VaiTro);

            var salt = TaoSalt();
            var hash = HashMatKhau(request.MatKhau!, salt);

            await using var conn = await CauHinhUngDung.OpenConnectionAsync(_connectionString, ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                IF EXISTS (SELECT 1 FROM dbo.taikhoan_phanmemtest WHERE ten_dang_nhap = @ten_dang_nhap)
                BEGIN
                    THROW 51000, N'Tên đăng nhập đã tồn tại.', 1;
                END;

                INSERT INTO dbo.taikhoan_phanmemtest
                    (ten_dang_nhap, mat_khau_hash, mat_khau_salt, ho_ten, email, so_dien_thoai, vai_tro, trang_thai)
                VALUES
                    (@ten_dang_nhap, @mat_khau_hash, @mat_khau_salt, @ho_ten, @email, @so_dien_thoai, @vai_tro, @trang_thai);
                """;
            GanThamSoChung(cmd, request.TenDangNhap, request.HoTen, request.Email, request.SoDienThoai, request.VaiTro, request.TrangThai);
            cmd.Parameters.AddWithValue("@mat_khau_hash", hash);
            cmd.Parameters.AddWithValue("@mat_khau_salt", salt);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task CapNhatTaiKhoanAsync(CapNhatTaiKhoanPhanMemRequest request, CancellationToken ct = default)
        {
            if (request.Id <= 0)
            {
                throw new InvalidOperationException("Chưa chọn tài khoản cần cập nhật.");
            }

            KiemTraDuLieuTaiKhoan(request.TenDangNhap, request.MatKhauMoi, request.VaiTro, batBuocMatKhau: false);

            await using var conn = await CauHinhUngDung.OpenConnectionAsync(_connectionString, ct);
            if (request.TrangThai != "hoat_dong" || request.VaiTro != "admin")
            {
                await DamBaoKhongKhoaAdminCuoiAsync(conn, request.Id, ct);
            }

            var salt = string.IsNullOrWhiteSpace(request.MatKhauMoi) ? null : TaoSalt();
            var hash = salt is null ? null : HashMatKhau(request.MatKhauMoi!, salt);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                IF EXISTS (
                    SELECT 1
                    FROM dbo.taikhoan_phanmemtest
                    WHERE ten_dang_nhap = @ten_dang_nhap
                      AND id <> @id
                )
                BEGIN
                    THROW 51000, N'Tên đăng nhập đã tồn tại.', 1;
                END;

                UPDATE dbo.taikhoan_phanmemtest
                SET ten_dang_nhap = @ten_dang_nhap,
                    ho_ten = @ho_ten,
                    email = @email,
                    so_dien_thoai = @so_dien_thoai,
                    vai_tro = @vai_tro,
                    trang_thai = @trang_thai,
                    mat_khau_hash = COALESCE(@mat_khau_hash, mat_khau_hash),
                    mat_khau_salt = COALESCE(@mat_khau_salt, mat_khau_salt),
                    cap_nhat_luc = SYSDATETIME()
                WHERE id = @id;
                """;
            cmd.Parameters.AddWithValue("@id", request.Id);
            GanThamSoChung(cmd, request.TenDangNhap, request.HoTen, request.Email, request.SoDienThoai, request.VaiTro, request.TrangThai);
            cmd.Parameters.AddWithValue("@mat_khau_hash", (object?)hash ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@mat_khau_salt", (object?)salt ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task DoiMatKhauAsync(
            int id,
            string matKhauCu,
            string matKhauMoi,
            CancellationToken ct = default)
        {
            if (id <= 0)
            {
                throw new InvalidOperationException("Tài khoản đăng nhập không hợp lệ.");
            }

            if (string.IsNullOrWhiteSpace(matKhauCu))
            {
                throw new InvalidOperationException("Nhập mật khẩu hiện tại.");
            }

            if (string.IsNullOrWhiteSpace(matKhauMoi) || matKhauMoi.Length < 4)
            {
                throw new InvalidOperationException("Mật khẩu mới phải có ít nhất 4 ký tự.");
            }

            await using var conn = await CauHinhUngDung.OpenConnectionAsync(_connectionString, ct);
            string hashDaLuu;
            string saltDaLuu;

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = """
                    SELECT mat_khau_hash, mat_khau_salt
                    FROM dbo.taikhoan_phanmemtest
                    WHERE id = @id
                      AND trang_thai = N'hoat_dong';
                    """;
                cmd.Parameters.AddWithValue("@id", id);

                await using var reader = await cmd.ExecuteReaderAsync(ct);
                if (!await reader.ReadAsync(ct))
                {
                    throw new InvalidOperationException("Tài khoản không tồn tại hoặc đã bị khóa.");
                }

                hashDaLuu = reader.GetString(0);
                saltDaLuu = reader.GetString(1);
            }

            if (!string.Equals(hashDaLuu, HashMatKhau(matKhauCu, saltDaLuu), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Mật khẩu hiện tại không đúng.");
            }

            var saltMoi = TaoSalt();
            var hashMoi = HashMatKhau(matKhauMoi, saltMoi);
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = """
                    UPDATE dbo.taikhoan_phanmemtest
                    SET mat_khau_hash = @mat_khau_hash,
                        mat_khau_salt = @mat_khau_salt,
                        cap_nhat_luc = SYSDATETIME()
                    WHERE id = @id;
                    """;
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@mat_khau_hash", hashMoi);
                cmd.Parameters.AddWithValue("@mat_khau_salt", saltMoi);
                await cmd.ExecuteNonQueryAsync(ct);
            }
        }

        private static async Task CapNhatLanDangNhapAsync(SqlConnection conn, int id, CancellationToken ct)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                UPDATE dbo.taikhoan_phanmemtest
                SET dang_nhap_cuoi_luc = SYSDATETIME()
                WHERE id = @id;
                """;
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        private static async Task DamBaoKhongKhoaAdminCuoiAsync(SqlConnection conn, int id, CancellationToken ct)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT COUNT(*)
                FROM dbo.taikhoan_phanmemtest
                WHERE vai_tro = N'admin'
                  AND trang_thai = N'hoat_dong'
                  AND id <> @id;
                """;
            cmd.Parameters.AddWithValue("@id", id);
            var soAdminKhac = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
            if (soAdminKhac == 0)
            {
                throw new InvalidOperationException("Không được khóa hoặc hạ quyền admin hoạt động cuối cùng.");
            }
        }

        private static void KiemTraDuLieuTaiKhoan(
            string tenDangNhap,
            string? matKhau,
            string vaiTro,
            bool batBuocMatKhau = true)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap))
            {
                throw new InvalidOperationException("Tên đăng nhập không được để trống.");
            }

            if (batBuocMatKhau && string.IsNullOrWhiteSpace(matKhau))
            {
                throw new InvalidOperationException("Mật khẩu không được để trống.");
            }

            if (!string.IsNullOrWhiteSpace(matKhau) && matKhau.Length < 4)
            {
                throw new InvalidOperationException("Mật khẩu phải có ít nhất 4 ký tự.");
            }

            if (vaiTro is not ("admin" or "nhan_vien"))
            {
                throw new InvalidOperationException("Quyền tài khoản không hợp lệ.");
            }
        }

        private static void GanThamSoChung(
            SqlCommand cmd,
            string tenDangNhap,
            string? hoTen,
            string? email,
            string? soDienThoai,
            string vaiTro,
            string trangThai)
        {
            cmd.Parameters.AddWithValue("@ten_dang_nhap", tenDangNhap.Trim());
            cmd.Parameters.AddWithValue("@ho_ten", LayGiaTriRongThanhNull(hoTen));
            cmd.Parameters.AddWithValue("@email", LayGiaTriRongThanhNull(email));
            cmd.Parameters.AddWithValue("@so_dien_thoai", LayGiaTriRongThanhNull(soDienThoai));
            cmd.Parameters.AddWithValue("@vai_tro", vaiTro);
            cmd.Parameters.AddWithValue("@trang_thai", trangThai == "khoa" ? "khoa" : "hoat_dong");
        }

        private static object LayGiaTriRongThanhNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
        }

        private static TaiKhoanPhanMem DocTaiKhoan(SqlDataReader reader)
        {
            return new TaiKhoanPhanMem(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                reader.IsDBNull(6) ? null : reader.GetString(6),
                reader.GetString(7),
                reader.GetString(8),
                new DateTimeOffset(DateTime.SpecifyKind(reader.GetDateTime(9), DateTimeKind.Local)),
                reader.IsDBNull(10) ? null : new DateTimeOffset(DateTime.SpecifyKind(reader.GetDateTime(10), DateTimeKind.Local)),
                reader.IsDBNull(11) ? null : new DateTimeOffset(DateTime.SpecifyKind(reader.GetDateTime(11), DateTimeKind.Local)));
        }

        private static string TaoSalt()
        {
            return Guid.NewGuid().ToString("N");
        }

        private static string HashMatKhau(string matKhau, string salt)
        {
            var bytes = Encoding.Unicode.GetBytes(salt + matKhau);
            return Convert.ToHexString(SHA256.HashData(bytes));
        }
    }

}
