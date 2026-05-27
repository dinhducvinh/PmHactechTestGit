using KiemThuApiShop.Core;
using Microsoft.Data.SqlClient;

namespace KiemThuApiShop.Seed;

public sealed class KhoDuLieuSeedSqlServer
{
    private readonly string _chuoiKetNoi;
    private readonly int _soTaiKhoanMacDinh;

    public KhoDuLieuSeedSqlServer(string chuoiKetNoi, int soTaiKhoanMacDinh)
    {
        _chuoiKetNoi = chuoiKetNoi;
        _soTaiKhoanMacDinh = soTaiKhoanMacDinh;
    }

    public DuLieuSeed DuLieu { get; private set; } = new();

    public async Task TaiAsync()
    {
        await using var connection = new SqlConnection(_chuoiKetNoi);
        await connection.OpenAsync();
        await KiemTraBangSeedAsync(connection);

        DuLieu = new DuLieuSeed();
        await TaiTaiKhoanAsync(connection);
        await TaiTimKiemAsync(connection);
        await TaiTheoDoiAsync(connection);
        await TaiChanAsync(connection);

        var truocKhiBoSung = DuLieu.TaiKhoanSeed.Count;
        DamBaoTaiKhoanSeed(_soTaiKhoanMacDinh);
        if (DuLieu.TaiKhoanSeed.Count != truocKhiBoSung)
        {
            await LuuAsync();
        }
    }

    public async Task LuuAsync()
    {
        await using var connection = new SqlConnection(_chuoiKetNoi);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            await LuuTaiKhoanAsync(connection, (SqlTransaction)transaction);
            await LuuTimKiemAsync(connection, (SqlTransaction)transaction);
            await LuuTheoDoiAsync(connection, (SqlTransaction)transaction);
            await LuuChanAsync(connection, (SqlTransaction)transaction);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> GhiKetQuaTestCaseAsync(IReadOnlyList<KetQuaChay> ketQua)
    {
        if (ketQua.Count == 0)
        {
            return 0;
        }

        const string sql = """
            INSERT INTO dbo.ketqua_testcase
            (ma_testcase, nhom, ten_hien_thi, trang_thai, ma_mong_doi, ma_thuc_te, http_status, endpoint, thong_diep, response_rut_gon)
            VALUES
            (@ma_testcase, @nhom, @ten_hien_thi, @trang_thai, @ma_mong_doi, @ma_thuc_te, @http_status, @endpoint, @thong_diep, @response_rut_gon);
            """;

        await using var connection = new SqlConnection(_chuoiKetNoi);
        await connection.OpenAsync();
        await KiemTraBangSeedAsync(connection);
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            foreach (var item in ketQua)
            {
                await using var command = TaoLenh(sql, connection, (SqlTransaction)transaction);
                Them(command, "@ma_testcase", CatChuoi(item.Ma, 80));
                Them(command, "@nhom", CatChuoi(item.Nhom, 50));
                Them(command, "@ten_hien_thi", CatChuoi(item.TenHienThi, 255));
                Them(command, "@trang_thai", item.TrangThai.ToString());
                Them(command, "@ma_mong_doi", CatChuoi(item.MaMongDoi, 100));
                Them(command, "@ma_thuc_te", CatChuoi(item.MaThucTe, 100));
                Them(command, "@http_status", item.HttpStatus);
                Them(command, "@endpoint", CatChuoi(item.Endpoint, 255));
                Them(command, "@thong_diep", item.ThongDiep);
                Them(command, "@response_rut_gon", item.ResponseRutGon);
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return ketQua.Count;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public void DamBaoTaiKhoanSeed(int soLuong)
    {
        var idTiepTheo = DuLieu.TaiKhoanSeed.Count == 0 ? 1 : DuLieu.TaiKhoanSeed.Max(x => x.TkSeedId) + 1;

        while (DuLieu.TaiKhoanSeed.Count < soLuong)
        {
            var id = idTiepTheo++;
            DuLieu.TaiKhoanSeed.Add(new TaiKhoanSeed
            {
                TkSeedId = id,
                Sdt = $"0909{id:000000}",
                MatKhauHienTai = $"Test{id:000000}",
                Uuid = $"thiet-bi-test-{id:000000}",
                TrangThaiDangKy = "chua_dang_ky",
                TrangThai = "san_sang",
                GhiChu = "Tự sinh bởi chương trình kiểm thử API và lưu trong SQL Server"
            });
        }
    }

    public TaiKhoanSeed? LayTaiKhoanDaDangKy()
    {
        return DuLieu.TaiKhoanSeed.FirstOrDefault(x =>
            x.TrangThai == "san_sang" &&
            x.TrangThaiDangKy == "da_dang_ky");
    }

    public TaiKhoanSeed? LayTaiKhoanChuaDangKy()
    {
        return DuLieu.TaiKhoanSeed.FirstOrDefault(x =>
            x.TrangThai == "san_sang" &&
            x.TrangThaiDangKy == "chua_dang_ky");
    }

    public TaiKhoanSeed? LayTaiKhoanTheoSeedId(int tkSeedId)
    {
        return DuLieu.TaiKhoanSeed.FirstOrDefault(x => x.TkSeedId == tkSeedId);
    }

    public TaiKhoanSeed? LayTaiKhoanKhac(int tkSeedId)
    {
        return DuLieu.TaiKhoanSeed.FirstOrDefault(x =>
            x.TkSeedId != tkSeedId &&
            x.TrangThai == "san_sang" &&
            x.TrangThaiDangKy == "da_dang_ky");
    }

    public (TaiKhoanSeed a, TaiKhoanSeed b)? LayHaiTaiKhoanDaDangKy()
    {
        var ds = DuLieu.TaiKhoanSeed
            .Where(x => x.TrangThai == "san_sang" && x.TrangThaiDangKy == "da_dang_ky")
            .Take(2)
            .ToList();

        return ds.Count >= 2 ? (ds[0], ds[1]) : null;
    }

    public TaiKhoanTimKiemSeed? LayTimKiemDangLuu()
    {
        return DuLieu.TaiKhoanTimKiemSeed.FirstOrDefault(x => x.TrangThai == "dang_luu");
    }

    public TaiKhoanTheoDoiSeed? LayTheoDoiDangHoatDong()
    {
        return DuLieu.TaiKhoanTheoDoiSeed.FirstOrDefault(x => x.TrangThai == "dang_theo_doi");
    }

    public TaiKhoanChanSeed? LayChanDangHoatDong()
    {
        return DuLieu.TaiKhoanChanSeed.FirstOrDefault(x => x.TrangThai == "dang_chan");
    }

    private static async Task KiemTraBangSeedAsync(SqlConnection connection)
    {
        var tenBang = new[]
        {
            "taikhoan_seed",
            "tk_timkiem_seed",
            "tk_theodoi_seed",
            "tk_chan_seed",
            "ketqua_testcase"
        };

        foreach (var bang in tenBang)
        {
            await using var command = new SqlCommand("SELECT OBJECT_ID(@ten_bang, 'U')", connection);
            command.Parameters.AddWithValue("@ten_bang", $"dbo.{bang}");
            var objectId = await command.ExecuteScalarAsync();
            if (objectId is null || objectId == DBNull.Value)
            {
                throw new InvalidOperationException($"Thiếu bảng dbo.{bang}. Hãy chạy script Sql/01_tao_database_va_bang_seed.sql trước khi chạy test.");
            }
        }
    }

    private async Task TaiTaiKhoanAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT tk_seed_id, tk_id_server, sdt, mat_khau_hien_tai, uuid, trang_thai_dang_ky, trang_thai, dang_ky_luc, doi_mk_luc, ghi_chu
            FROM dbo.taikhoan_seed
            ORDER BY tk_seed_id;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.TaiKhoanSeed.Add(new TaiKhoanSeed
            {
                TkSeedId = DocInt(reader, "tk_seed_id"),
                TkId = DocChuoiNull(reader, "tk_id_server"),
                Sdt = DocChuoi(reader, "sdt"),
                MatKhauHienTai = DocChuoi(reader, "mat_khau_hien_tai"),
                Uuid = DocChuoi(reader, "uuid"),
                TrangThaiDangKy = DocChuoi(reader, "trang_thai_dang_ky"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                DangKyLuc = DocNgayNull(reader, "dang_ky_luc"),
                DoiMatKhauLuc = DocNgayNull(reader, "doi_mk_luc"),
                GhiChu = DocChuoiNull(reader, "ghi_chu")
            });
        }
    }

    private async Task TaiTheoDoiAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT td_seed_id, follower_tk_seed_id, follower_tk_id_server, followee_tk_seed_id, followee_tk_id_server, theo_doi_luc, trang_thai, ghi_chu
            FROM dbo.tk_theodoi_seed;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.TaiKhoanTheoDoiSeed.Add(new TaiKhoanTheoDoiSeed
            {
                TdSeedId = DocInt(reader, "td_seed_id"),
                TkSeedId = DocInt(reader, "follower_tk_seed_id"),
                TkId = DocChuoiNull(reader, "follower_tk_id_server"),
                FolloweeTkSeedId = DocInt(reader, "followee_tk_seed_id"),
                FolloweeTkId = DocChuoiNull(reader, "followee_tk_id_server"),
                TheoDoiLuc = DocNgayNull(reader, "theo_doi_luc"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                GhiChu = DocChuoiNull(reader, "ghi_chu")
            });
        }
    }

    private async Task TaiTimKiemAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT tk_timkiem_seed_id, tk_seed_id, tk_id_server, saved_search_id_server, keyword, trang_thai, tao_boi_test, tao_luc, xoa_luc, ghi_chu
            FROM dbo.tk_timkiem_seed
            ORDER BY tk_timkiem_seed_id;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.TaiKhoanTimKiemSeed.Add(new TaiKhoanTimKiemSeed
            {
                TkTimKiemSeedId = DocInt(reader, "tk_timkiem_seed_id"),
                TkSeedId = DocInt(reader, "tk_seed_id"),
                TkId = DocChuoiNull(reader, "tk_id_server"),
                SavedSearchId = DocChuoiNull(reader, "saved_search_id_server"),
                Keyword = DocChuoi(reader, "keyword"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                TaoBoiTest = DocBool(reader, "tao_boi_test"),
                TaoLuc = DocNgayNull(reader, "tao_luc"),
                XoaLuc = DocNgayNull(reader, "xoa_luc"),
                GhiChu = DocChuoiNull(reader, "ghi_chu")
            });
        }
    }

    private async Task TaiChanAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT chan_seed_id, blocker_tk_seed_id, blocker_tk_id_server, blocked_tk_seed_id, blocked_tk_id_server, chan_luc, trang_thai, ghi_chu
            FROM dbo.tk_chan_seed;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.TaiKhoanChanSeed.Add(new TaiKhoanChanSeed
            {
                ChanSeedId = DocInt(reader, "chan_seed_id"),
                ChanTkSeedId = DocInt(reader, "blocker_tk_seed_id"),
                ChanTkId = DocChuoiNull(reader, "blocker_tk_id_server"),
                BiChanTkSeedId = DocInt(reader, "blocked_tk_seed_id"),
                BiChanTkId = DocChuoiNull(reader, "blocked_tk_id_server"),
                ChanLuc = DocNgayNull(reader, "chan_luc"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                GhiChu = DocChuoiNull(reader, "ghi_chu")
            });
        }
    }

    private async Task LuuTaiKhoanAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            IF EXISTS (SELECT 1 FROM dbo.taikhoan_seed WHERE tk_seed_id = @tk_seed_id)
            BEGIN
                UPDATE dbo.taikhoan_seed
                SET tk_id_server = @tk_id_server,
                    sdt = @sdt,
                    mat_khau_hien_tai = @mat_khau_hien_tai,
                    uuid = @uuid,
                    trang_thai_dang_ky = @trang_thai_dang_ky,
                    trang_thai = @trang_thai,
                    dang_ky_luc = @dang_ky_luc,
                    doi_mk_luc = @doi_mk_luc,
                    ghi_chu = @ghi_chu,
                    cap_nhat_luc = SYSDATETIME()
                WHERE tk_seed_id = @tk_seed_id;
            END
            ELSE
            BEGIN
                SET IDENTITY_INSERT dbo.taikhoan_seed ON;
                INSERT INTO dbo.taikhoan_seed
                (tk_seed_id, tk_id_server, sdt, mat_khau_hien_tai, uuid, trang_thai_dang_ky, trang_thai, dang_ky_luc, doi_mk_luc, ghi_chu)
                VALUES
                (@tk_seed_id, @tk_id_server, @sdt, @mat_khau_hien_tai, @uuid, @trang_thai_dang_ky, @trang_thai, @dang_ky_luc, @doi_mk_luc, @ghi_chu);
                SET IDENTITY_INSERT dbo.taikhoan_seed OFF;
            END
            """;

        foreach (var item in DuLieu.TaiKhoanSeed)
        {
            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@tk_seed_id", item.TkSeedId);
            Them(command, "@tk_id_server", item.TkId);
            Them(command, "@sdt", item.Sdt);
            Them(command, "@mat_khau_hien_tai", item.MatKhauHienTai);
            Them(command, "@uuid", item.Uuid);
            Them(command, "@trang_thai_dang_ky", item.TrangThaiDangKy);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@dang_ky_luc", item.DangKyLuc);
            Them(command, "@doi_mk_luc", item.DoiMatKhauLuc);
            Them(command, "@ghi_chu", item.GhiChu);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task LuuTheoDoiAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            MERGE dbo.tk_theodoi_seed AS target
            USING (SELECT @follower_tk_seed_id AS follower_tk_seed_id, @followee_tk_seed_id AS followee_tk_seed_id) AS source
            ON target.follower_tk_seed_id = source.follower_tk_seed_id AND target.followee_tk_seed_id = source.followee_tk_seed_id
            WHEN MATCHED THEN
                UPDATE SET follower_tk_id_server = @follower_tk_id_server,
                    followee_tk_id_server = @followee_tk_id_server,
                    theo_doi_luc = @theo_doi_luc,
                    trang_thai = @trang_thai,
                    ghi_chu = @ghi_chu
            WHEN NOT MATCHED THEN
                INSERT (follower_tk_seed_id, follower_tk_id_server, followee_tk_seed_id, followee_tk_id_server, theo_doi_luc, trang_thai, ghi_chu)
                VALUES (@follower_tk_seed_id, @follower_tk_id_server, @followee_tk_seed_id, @followee_tk_id_server, @theo_doi_luc, @trang_thai, @ghi_chu);
            """;

        foreach (var item in DuLieu.TaiKhoanTheoDoiSeed)
        {
            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@follower_tk_seed_id", item.TkSeedId);
            Them(command, "@follower_tk_id_server", item.TkId);
            Them(command, "@followee_tk_seed_id", item.FolloweeTkSeedId);
            Them(command, "@followee_tk_id_server", item.FolloweeTkId);
            Them(command, "@theo_doi_luc", item.TheoDoiLuc ?? DateTimeOffset.Now);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@ghi_chu", item.GhiChu);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task LuuTimKiemAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            IF EXISTS (SELECT 1 FROM dbo.tk_timkiem_seed WHERE tk_timkiem_seed_id = @tk_timkiem_seed_id)
            BEGIN
                UPDATE dbo.tk_timkiem_seed
                SET tk_seed_id = @tk_seed_id,
                    tk_id_server = @tk_id_server,
                    saved_search_id_server = @saved_search_id_server,
                    keyword = @keyword,
                    trang_thai = @trang_thai,
                    tao_boi_test = @tao_boi_test,
                    tao_luc = @tao_luc,
                    xoa_luc = @xoa_luc,
                    ghi_chu = @ghi_chu
                WHERE tk_timkiem_seed_id = @tk_timkiem_seed_id;
            END
            ELSE
            BEGIN
                SET IDENTITY_INSERT dbo.tk_timkiem_seed ON;
                INSERT INTO dbo.tk_timkiem_seed
                (tk_timkiem_seed_id, tk_seed_id, tk_id_server, saved_search_id_server, keyword, trang_thai, tao_boi_test, tao_luc, xoa_luc, ghi_chu)
                VALUES
                (@tk_timkiem_seed_id, @tk_seed_id, @tk_id_server, @saved_search_id_server, @keyword, @trang_thai, @tao_boi_test, @tao_luc, @xoa_luc, @ghi_chu);
                SET IDENTITY_INSERT dbo.tk_timkiem_seed OFF;
            END
            """;

        foreach (var item in DuLieu.TaiKhoanTimKiemSeed)
        {
            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@tk_timkiem_seed_id", item.TkTimKiemSeedId);
            Them(command, "@tk_seed_id", item.TkSeedId);
            Them(command, "@tk_id_server", item.TkId);
            Them(command, "@saved_search_id_server", item.SavedSearchId);
            Them(command, "@keyword", item.Keyword);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@tao_boi_test", item.TaoBoiTest);
            Them(command, "@tao_luc", item.TaoLuc);
            Them(command, "@xoa_luc", item.XoaLuc);
            Them(command, "@ghi_chu", item.GhiChu);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task LuuChanAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            MERGE dbo.tk_chan_seed AS target
            USING (SELECT @blocker_tk_seed_id AS blocker_tk_seed_id, @blocked_tk_seed_id AS blocked_tk_seed_id) AS source
            ON target.blocker_tk_seed_id = source.blocker_tk_seed_id AND target.blocked_tk_seed_id = source.blocked_tk_seed_id
            WHEN MATCHED THEN
                UPDATE SET blocker_tk_id_server = @blocker_tk_id_server,
                    blocked_tk_id_server = @blocked_tk_id_server,
                    chan_luc = @chan_luc,
                    trang_thai = @trang_thai,
                    ghi_chu = @ghi_chu
            WHEN NOT MATCHED THEN
                INSERT (blocker_tk_seed_id, blocker_tk_id_server, blocked_tk_seed_id, blocked_tk_id_server, chan_luc, trang_thai, ghi_chu)
                VALUES (@blocker_tk_seed_id, @blocker_tk_id_server, @blocked_tk_seed_id, @blocked_tk_id_server, @chan_luc, @trang_thai, @ghi_chu);
            """;

        foreach (var item in DuLieu.TaiKhoanChanSeed)
        {
            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@blocker_tk_seed_id", item.ChanTkSeedId);
            Them(command, "@blocker_tk_id_server", item.ChanTkId);
            Them(command, "@blocked_tk_seed_id", item.BiChanTkSeedId);
            Them(command, "@blocked_tk_id_server", item.BiChanTkId);
            Them(command, "@chan_luc", item.ChanLuc ?? DateTimeOffset.Now);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@ghi_chu", item.GhiChu);
            await command.ExecuteNonQueryAsync();
        }
    }

    private static SqlCommand TaoLenh(string sql, SqlConnection connection, SqlTransaction transaction)
    {
        return new SqlCommand(sql, connection, transaction);
    }

    private static void Them(SqlCommand command, string ten, object? giaTri)
    {
        if (giaTri is DateTimeOffset dto)
        {
            command.Parameters.AddWithValue(ten, dto.LocalDateTime);
            return;
        }

        command.Parameters.AddWithValue(ten, giaTri ?? DBNull.Value);
    }

    private static string DocChuoi(SqlDataReader reader, string tenCot)
    {
        return reader[tenCot]?.ToString() ?? "";
    }

    private static string? DocChuoiNull(SqlDataReader reader, string tenCot)
    {
        return reader[tenCot] == DBNull.Value ? null : reader[tenCot].ToString();
    }

    private static int DocInt(SqlDataReader reader, string tenCot)
    {
        return Convert.ToInt32(reader[tenCot]);
    }

    private static bool DocBool(SqlDataReader reader, string tenCot)
    {
        return Convert.ToBoolean(reader[tenCot]);
    }

    private static DateTimeOffset? DocNgayNull(SqlDataReader reader, string tenCot)
    {
        if (reader[tenCot] == DBNull.Value)
        {
            return null;
        }

        var dateTime = Convert.ToDateTime(reader[tenCot]);
        return new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Local));
    }

    private static string? CatChuoi(string? giaTri, int doDaiToiDa)
    {
        if (string.IsNullOrEmpty(giaTri) || giaTri.Length <= doDaiToiDa)
        {
            return giaTri;
        }

        return giaTri[..doDaiToiDa];
    }
}
