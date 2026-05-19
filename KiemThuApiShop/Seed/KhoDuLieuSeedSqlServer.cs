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
        await TaiDanhMucAsync(connection);
        await TaiThuongHieuAsync(connection);
        await TaiDiaChiAsync(connection);
        await TaiSanPhamAsync(connection);
        await TaiBinhLuanAsync(connection);
        await TaiLikeAsync(connection);
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
            await LuuDanhMucAsync(connection, (SqlTransaction)transaction);
            await LuuThuongHieuAsync(connection, (SqlTransaction)transaction);
            await LuuDiaChiAsync(connection, (SqlTransaction)transaction);
            await LuuSanPhamAsync(connection, (SqlTransaction)transaction);
            await LuuBinhLuanAsync(connection, (SqlTransaction)transaction);
            await LuuLikeAsync(connection, (SqlTransaction)transaction);
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

    public DanhMucSeed? LayDanhMucBatKy()
    {
        return DuLieu.DanhMucSeed.FirstOrDefault(x => x.TrangThai == "san_sang");
    }

    public DanhMucSeed? LayDanhMucCoCon()
    {
        return DuLieu.DanhMucSeed.FirstOrDefault(x => x.TrangThai == "san_sang" && x.CoDanhMucCon);
    }

    public DanhMucSeed? LayDanhMucKhongCoCon()
    {
        return DuLieu.DanhMucSeed.FirstOrDefault(x => x.TrangThai == "san_sang" && !x.CoDanhMucCon);
    }

    public DanhMucSeed? LayDanhMucCoThuongHieu()
    {
        return DuLieu.DanhMucSeed.FirstOrDefault(x => x.TrangThai == "san_sang" && x.CoThuongHieu);
    }

    public DanhMucSeed? LayDanhMucKhongCoThuongHieu()
    {
        return DuLieu.DanhMucSeed.FirstOrDefault(x => x.TrangThai == "san_sang" && !x.CoThuongHieu);
    }

    public ThuongHieuSeed? LayThuongHieuBatKy()
    {
        return DuLieu.ThuongHieuSeed.FirstOrDefault(x => x.TrangThai == "san_sang");
    }

    public DiaChiTaiKhoanSeed? LayDiaChiTheoTaiKhoan(int tkSeedId)
    {
        return DuLieu.DiaChiTaiKhoanSeed.FirstOrDefault(x =>
            x.TkSeedId == tkSeedId && x.TrangThai == "san_sang");
    }

    public DiaChiTaiKhoanSeed? LayDiaChiKhacTaiKhoan(int tkSeedId)
    {
        return DuLieu.DiaChiTaiKhoanSeed.FirstOrDefault(x =>
            x.TkSeedId != tkSeedId && x.TrangThai == "san_sang");
    }

    public SanPhamSeed? LaySanPhamBatKy()
    {
        return DuLieu.SanPhamSeed.FirstOrDefault(x => x.TrangThai == "san_sang");
    }

    public SanPhamSeed? LaySanPhamTheoLoai(string loaiSeed)
    {
        return DuLieu.SanPhamSeed.FirstOrDefault(x =>
            x.TrangThai == "san_sang" &&
            string.Equals(x.LoaiSeed, loaiSeed, StringComparison.OrdinalIgnoreCase));
    }

    public SanPhamSeed? LaySanPhamKhac(string spId)
    {
        return DuLieu.SanPhamSeed.FirstOrDefault(x =>
            x.TrangThai == "san_sang" &&
            !string.Equals(x.SpId, spId, StringComparison.OrdinalIgnoreCase));
    }

    public SanPhamSeed? LaySanPhamChuaDuocLikeBoi(int tkSeedId)
    {
        var daLike = DuLieu.TaiKhoanThichSanPhamSeed
            .Where(x => x.TkSeedId == tkSeedId && x.TrangThai == "dang_like")
            .Select(x => x.SpId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return DuLieu.SanPhamSeed.FirstOrDefault(x =>
            x.TrangThai == "san_sang" && !daLike.Contains(x.SpId));
    }

    public BinhLuanSanPhamSeed? LayBinhLuanBatKy()
    {
        return DuLieu.BinhLuanSanPhamSeed.FirstOrDefault(x => x.TrangThai == "san_sang");
    }

    public SanPhamSeed? LaySanPhamChuaCoBinhLuan()
    {
        var daCoBinhLuan = DuLieu.BinhLuanSanPhamSeed
            .Where(x => x.TrangThai == "san_sang")
            .Select(x => x.SpId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return DuLieu.SanPhamSeed.FirstOrDefault(x =>
            x.TrangThai == "san_sang" && !daCoBinhLuan.Contains(x.SpId));
    }

    public TaiKhoanThichSanPhamSeed? LayLikeBatKy()
    {
        return DuLieu.TaiKhoanThichSanPhamSeed.FirstOrDefault(x => x.TrangThai == "dang_like");
    }

    private static async Task KiemTraBangSeedAsync(SqlConnection connection)
    {
        var tenBang = new[]
        {
            "taikhoan_seed",
            "danhmuc_seed",
            "thuonghieu_seed",
            "diachitk_seed",
            "sanpham_seed",
            "binhluan_sp_seed",
            "tk_thich_sanpham_seed",
            "tk_theodoi_seed",
            "tk_chan_seed"
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
            SELECT tk_seed_id, tk_id, sdt, mat_khau_hien_tai, uuid, trang_thai_dang_ky, trang_thai, dang_ky_luc, doi_mk_luc, ghi_chu
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
                TkId = DocChuoiNull(reader, "tk_id"),
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

    private async Task TaiDanhMucAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT dm_id, ten_danh_muc, dm_cha_id, co_danh_muc_con, co_thuong_hieu, co_kich_co, yeu_cau_can_nang, sort, trang_thai, dong_bo_luc
            FROM dbo.danhmuc_seed
            ORDER BY sort, dm_id;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.DanhMucSeed.Add(new DanhMucSeed
            {
                DmId = DocChuoi(reader, "dm_id"),
                TenDanhMuc = DocChuoi(reader, "ten_danh_muc"),
                DmChaId = DocChuoiNull(reader, "dm_cha_id"),
                CoDanhMucCon = DocBool(reader, "co_danh_muc_con"),
                CoThuongHieu = DocBool(reader, "co_thuong_hieu"),
                CoKichCo = DocBool(reader, "co_kich_co"),
                YeuCauCanNang = DocBool(reader, "yeu_cau_can_nang"),
                Sort = DocIntNull(reader, "sort"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                DongBoLuc = DocNgayNull(reader, "dong_bo_luc")
            });
        }
    }

    private async Task TaiThuongHieuAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT thuonghieu_id, ten_thuong_hieu, dm_id, trang_thai, dong_bo_luc
            FROM dbo.thuonghieu_seed
            ORDER BY thuonghieu_id;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.ThuongHieuSeed.Add(new ThuongHieuSeed
            {
                ThuongHieuId = DocChuoi(reader, "thuonghieu_id"),
                TenThuongHieu = DocChuoi(reader, "ten_thuong_hieu"),
                DmId = DocChuoiNull(reader, "dm_id"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                DongBoLuc = DocNgayNull(reader, "dong_bo_luc")
            });
        }
    }

    private async Task TaiDiaChiAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT tk_seed_id, tk_id, diachi_id, ten_nguoi_nhan, sdt, diachi_daydu, phuong_xa_id, vi_do, kinh_do, mac_dinh, trang_thai, tao_luc
            FROM dbo.diachitk_seed
            ORDER BY diachitk_seed_id;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.DiaChiTaiKhoanSeed.Add(new DiaChiTaiKhoanSeed
            {
                TkSeedId = DocInt(reader, "tk_seed_id"),
                TkId = DocChuoiNull(reader, "tk_id"),
                DiaChiId = DocChuoi(reader, "diachi_id"),
                TenNguoiNhan = DocChuoi(reader, "ten_nguoi_nhan"),
                Sdt = DocChuoi(reader, "sdt"),
                DiaChiDayDu = DocChuoi(reader, "diachi_daydu"),
                PhuongXaId = DocIntNull(reader, "phuong_xa_id") ?? 1,
                ViDo = DocDecimalNull(reader, "vi_do") ?? 21.0278m,
                KinhDo = DocDecimalNull(reader, "kinh_do") ?? 105.8342m,
                MacDinh = DocBool(reader, "mac_dinh"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                TaoLuc = DocNgayNull(reader, "tao_luc")
            });
        }
    }

    private async Task TaiSanPhamAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT sp_seed_id, sp_id, seller_tk_seed_id, seller_tk_id, dm_id, thuonghieu_id, diachi_id, ten_sp, gia, trang_thai, loai_seed, tao_boi_test, xac_minh_luc
            FROM dbo.sanpham_seed
            ORDER BY sp_seed_id;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.SanPhamSeed.Add(new SanPhamSeed
            {
                SpSeedId = DocInt(reader, "sp_seed_id"),
                SpId = DocChuoi(reader, "sp_id"),
                SellerTkSeedId = DocInt(reader, "seller_tk_seed_id"),
                SellerTkId = DocChuoiNull(reader, "seller_tk_id"),
                DmId = DocChuoi(reader, "dm_id"),
                ThuongHieuId = DocChuoiNull(reader, "thuonghieu_id"),
                DiaChiId = DocChuoiNull(reader, "diachi_id"),
                TenSp = DocChuoi(reader, "ten_sp"),
                Gia = DocDecimal(reader, "gia"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                LoaiSeed = DocChuoi(reader, "loai_seed"),
                TaoBoiTest = DocBool(reader, "tao_boi_test"),
                XacMinhLuc = DocNgayNull(reader, "xac_minh_luc")
            });
        }
    }

    private async Task TaiBinhLuanAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT bl_seed_id, sp_seed_id, sp_id, tk_seed_id, tk_id, bl_id, noi_dung, trang_thai, tao_luc, ghi_chu
            FROM dbo.binhluan_sp_seed
            ORDER BY bl_seed_id;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.BinhLuanSanPhamSeed.Add(new BinhLuanSanPhamSeed
            {
                BlSeedId = DocInt(reader, "bl_seed_id"),
                SpSeedId = DocInt(reader, "sp_seed_id"),
                SpId = DocChuoi(reader, "sp_id"),
                TkSeedId = DocInt(reader, "tk_seed_id"),
                TkId = DocChuoiNull(reader, "tk_id"),
                BlId = DocChuoiNull(reader, "bl_id"),
                NoiDung = DocChuoi(reader, "noi_dung"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                TaoLuc = DocNgayNull(reader, "tao_luc"),
                GhiChu = DocChuoiNull(reader, "ghi_chu")
            });
        }
    }

    private async Task TaiLikeAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT tk_seed_id, tk_id, sp_seed_id, sp_id, trang_thai, tao_luc
            FROM dbo.tk_thich_sanpham_seed;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.TaiKhoanThichSanPhamSeed.Add(new TaiKhoanThichSanPhamSeed
            {
                TkSeedId = DocInt(reader, "tk_seed_id"),
                TkId = DocChuoiNull(reader, "tk_id"),
                SpSeedId = DocInt(reader, "sp_seed_id"),
                SpId = DocChuoi(reader, "sp_id"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                TaoLuc = DocNgayNull(reader, "tao_luc")
            });
        }
    }

    private async Task TaiTheoDoiAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT tk_seed_id, tk_id, followee_tk_seed_id, followee_tk_id, trang_thai
            FROM dbo.tk_theodoi_seed;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.TaiKhoanTheoDoiSeed.Add(new TaiKhoanTheoDoiSeed
            {
                TkSeedId = DocInt(reader, "tk_seed_id"),
                TkId = DocChuoiNull(reader, "tk_id"),
                FolloweeTkSeedId = DocInt(reader, "followee_tk_seed_id"),
                FolloweeTkId = DocChuoiNull(reader, "followee_tk_id"),
                TrangThai = DocChuoi(reader, "trang_thai")
            });
        }
    }

    private async Task TaiChanAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT chan_tk_seed_id, chan_tk_id, bi_chan_tk_seed_id, bi_chan_tk_id, trang_thai
            FROM dbo.tk_chan_seed;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.TaiKhoanChanSeed.Add(new TaiKhoanChanSeed
            {
                ChanTkSeedId = DocInt(reader, "chan_tk_seed_id"),
                ChanTkId = DocChuoiNull(reader, "chan_tk_id"),
                BiChanTkSeedId = DocInt(reader, "bi_chan_tk_seed_id"),
                BiChanTkId = DocChuoiNull(reader, "bi_chan_tk_id"),
                TrangThai = DocChuoi(reader, "trang_thai")
            });
        }
    }

    private async Task LuuTaiKhoanAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            IF EXISTS (SELECT 1 FROM dbo.taikhoan_seed WHERE tk_seed_id = @tk_seed_id)
            BEGIN
                UPDATE dbo.taikhoan_seed
                SET tk_id = @tk_id,
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
                (tk_seed_id, tk_id, sdt, mat_khau_hien_tai, uuid, trang_thai_dang_ky, trang_thai, dang_ky_luc, doi_mk_luc, ghi_chu)
                VALUES
                (@tk_seed_id, @tk_id, @sdt, @mat_khau_hien_tai, @uuid, @trang_thai_dang_ky, @trang_thai, @dang_ky_luc, @doi_mk_luc, @ghi_chu);
                SET IDENTITY_INSERT dbo.taikhoan_seed OFF;
            END
            """;

        foreach (var item in DuLieu.TaiKhoanSeed)
        {
            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@tk_seed_id", item.TkSeedId);
            Them(command, "@tk_id", item.TkId);
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

    private async Task LuuDanhMucAsync(SqlConnection connection, SqlTransaction transaction)
    {
        await XoaBangAsync(connection, transaction, "dbo.danhmuc_seed");
        const string sql = """
            INSERT INTO dbo.danhmuc_seed
            (dm_id, ten_danh_muc, dm_cha_id, co_danh_muc_con, co_thuong_hieu, co_kich_co, yeu_cau_can_nang, sort, trang_thai, dong_bo_luc)
            VALUES
            (@dm_id, @ten_danh_muc, @dm_cha_id, @co_danh_muc_con, @co_thuong_hieu, @co_kich_co, @yeu_cau_can_nang, @sort, @trang_thai, @dong_bo_luc);
            """;

        foreach (var item in DuLieu.DanhMucSeed)
        {
            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@dm_id", item.DmId);
            Them(command, "@ten_danh_muc", item.TenDanhMuc);
            Them(command, "@dm_cha_id", item.DmChaId);
            Them(command, "@co_danh_muc_con", item.CoDanhMucCon);
            Them(command, "@co_thuong_hieu", item.CoThuongHieu);
            Them(command, "@co_kich_co", item.CoKichCo);
            Them(command, "@yeu_cau_can_nang", item.YeuCauCanNang);
            Them(command, "@sort", item.Sort);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@dong_bo_luc", item.DongBoLuc);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task LuuThuongHieuAsync(SqlConnection connection, SqlTransaction transaction)
    {
        await XoaBangAsync(connection, transaction, "dbo.thuonghieu_seed");
        const string sql = """
            INSERT INTO dbo.thuonghieu_seed
            (thuonghieu_id, ten_thuong_hieu, dm_id, trang_thai, dong_bo_luc)
            VALUES
            (@thuonghieu_id, @ten_thuong_hieu, @dm_id, @trang_thai, @dong_bo_luc);
            """;

        foreach (var item in DuLieu.ThuongHieuSeed)
        {
            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@thuonghieu_id", item.ThuongHieuId);
            Them(command, "@ten_thuong_hieu", item.TenThuongHieu);
            Them(command, "@dm_id", item.DmId);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@dong_bo_luc", item.DongBoLuc);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task LuuDiaChiAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            MERGE dbo.diachitk_seed AS target
            USING (SELECT @diachi_id AS diachi_id) AS source
            ON target.diachi_id = source.diachi_id
            WHEN MATCHED THEN
                UPDATE SET tk_seed_id = @tk_seed_id, tk_id = @tk_id, ten_nguoi_nhan = @ten_nguoi_nhan, sdt = @sdt,
                    diachi_daydu = @diachi_daydu, phuong_xa_id = @phuong_xa_id, vi_do = @vi_do, kinh_do = @kinh_do,
                    mac_dinh = @mac_dinh, trang_thai = @trang_thai, tao_luc = @tao_luc
            WHEN NOT MATCHED THEN
                INSERT (tk_seed_id, tk_id, diachi_id, ten_nguoi_nhan, sdt, diachi_daydu, phuong_xa_id, vi_do, kinh_do, mac_dinh, trang_thai, tao_luc)
                VALUES (@tk_seed_id, @tk_id, @diachi_id, @ten_nguoi_nhan, @sdt, @diachi_daydu, @phuong_xa_id, @vi_do, @kinh_do, @mac_dinh, @trang_thai, @tao_luc);
            """;

        foreach (var item in DuLieu.DiaChiTaiKhoanSeed)
        {
            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@tk_seed_id", item.TkSeedId);
            Them(command, "@tk_id", item.TkId);
            Them(command, "@diachi_id", item.DiaChiId);
            Them(command, "@ten_nguoi_nhan", item.TenNguoiNhan);
            Them(command, "@sdt", item.Sdt);
            Them(command, "@diachi_daydu", item.DiaChiDayDu);
            Them(command, "@phuong_xa_id", item.PhuongXaId);
            Them(command, "@vi_do", item.ViDo);
            Them(command, "@kinh_do", item.KinhDo);
            Them(command, "@mac_dinh", item.MacDinh);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@tao_luc", item.TaoLuc);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task LuuSanPhamAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            IF EXISTS (SELECT 1 FROM dbo.sanpham_seed WHERE sp_seed_id = @sp_seed_id)
            BEGIN
                UPDATE dbo.sanpham_seed
                SET sp_id = @sp_id, seller_tk_seed_id = @seller_tk_seed_id, seller_tk_id = @seller_tk_id,
                    dm_id = @dm_id, thuonghieu_id = @thuonghieu_id, diachi_id = @diachi_id, ten_sp = @ten_sp,
                    gia = @gia, trang_thai = @trang_thai, loai_seed = @loai_seed, tao_boi_test = @tao_boi_test,
                    xac_minh_luc = @xac_minh_luc
                WHERE sp_seed_id = @sp_seed_id;
            END
            ELSE
            BEGIN
                SET IDENTITY_INSERT dbo.sanpham_seed ON;
                INSERT INTO dbo.sanpham_seed
                (sp_seed_id, sp_id, seller_tk_seed_id, seller_tk_id, dm_id, thuonghieu_id, diachi_id, ten_sp, gia, trang_thai, loai_seed, tao_boi_test, xac_minh_luc)
                VALUES
                (@sp_seed_id, @sp_id, @seller_tk_seed_id, @seller_tk_id, @dm_id, @thuonghieu_id, @diachi_id, @ten_sp, @gia, @trang_thai, @loai_seed, @tao_boi_test, @xac_minh_luc);
                SET IDENTITY_INSERT dbo.sanpham_seed OFF;
            END
            """;

        foreach (var item in DuLieu.SanPhamSeed)
        {
            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@sp_seed_id", item.SpSeedId);
            Them(command, "@sp_id", item.SpId);
            Them(command, "@seller_tk_seed_id", item.SellerTkSeedId);
            Them(command, "@seller_tk_id", item.SellerTkId);
            Them(command, "@dm_id", item.DmId);
            Them(command, "@thuonghieu_id", item.ThuongHieuId);
            Them(command, "@diachi_id", item.DiaChiId);
            Them(command, "@ten_sp", item.TenSp);
            Them(command, "@gia", item.Gia);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@loai_seed", item.LoaiSeed);
            Them(command, "@tao_boi_test", item.TaoBoiTest);
            Them(command, "@xac_minh_luc", item.XacMinhLuc);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task LuuBinhLuanAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            IF EXISTS (SELECT 1 FROM dbo.binhluan_sp_seed WHERE bl_seed_id = @bl_seed_id)
            BEGIN
                UPDATE dbo.binhluan_sp_seed
                SET sp_seed_id = @sp_seed_id, sp_id = @sp_id, tk_seed_id = @tk_seed_id, tk_id = @tk_id,
                    bl_id = @bl_id, noi_dung = @noi_dung, trang_thai = @trang_thai, tao_luc = @tao_luc, ghi_chu = @ghi_chu
                WHERE bl_seed_id = @bl_seed_id;
            END
            ELSE
            BEGIN
                SET IDENTITY_INSERT dbo.binhluan_sp_seed ON;
                INSERT INTO dbo.binhluan_sp_seed
                (bl_seed_id, sp_seed_id, sp_id, tk_seed_id, tk_id, bl_id, noi_dung, trang_thai, tao_luc, ghi_chu)
                VALUES
                (@bl_seed_id, @sp_seed_id, @sp_id, @tk_seed_id, @tk_id, @bl_id, @noi_dung, @trang_thai, @tao_luc, @ghi_chu);
                SET IDENTITY_INSERT dbo.binhluan_sp_seed OFF;
            END
            """;

        foreach (var item in DuLieu.BinhLuanSanPhamSeed)
        {
            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@bl_seed_id", item.BlSeedId);
            Them(command, "@sp_seed_id", item.SpSeedId);
            Them(command, "@sp_id", item.SpId);
            Them(command, "@tk_seed_id", item.TkSeedId);
            Them(command, "@tk_id", item.TkId);
            Them(command, "@bl_id", item.BlId);
            Them(command, "@noi_dung", item.NoiDung);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@tao_luc", item.TaoLuc);
            Them(command, "@ghi_chu", item.GhiChu);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task LuuLikeAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            MERGE dbo.tk_thich_sanpham_seed AS target
            USING (SELECT @tk_seed_id AS tk_seed_id, @sp_seed_id AS sp_seed_id) AS source
            ON target.tk_seed_id = source.tk_seed_id AND target.sp_seed_id = source.sp_seed_id
            WHEN MATCHED THEN
                UPDATE SET tk_id = @tk_id, sp_id = @sp_id, trang_thai = @trang_thai, tao_luc = @tao_luc
            WHEN NOT MATCHED THEN
                INSERT (tk_seed_id, tk_id, sp_seed_id, sp_id, trang_thai, tao_luc)
                VALUES (@tk_seed_id, @tk_id, @sp_seed_id, @sp_id, @trang_thai, @tao_luc);
            """;

        foreach (var item in DuLieu.TaiKhoanThichSanPhamSeed)
        {
            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@tk_seed_id", item.TkSeedId);
            Them(command, "@tk_id", item.TkId);
            Them(command, "@sp_seed_id", item.SpSeedId);
            Them(command, "@sp_id", item.SpId);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@tao_luc", item.TaoLuc);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task LuuTheoDoiAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            MERGE dbo.tk_theodoi_seed AS target
            USING (SELECT @tk_seed_id AS tk_seed_id, @followee_tk_seed_id AS followee_tk_seed_id) AS source
            ON target.tk_seed_id = source.tk_seed_id AND target.followee_tk_seed_id = source.followee_tk_seed_id
            WHEN MATCHED THEN
                UPDATE SET tk_id = @tk_id, followee_tk_id = @followee_tk_id, trang_thai = @trang_thai
            WHEN NOT MATCHED THEN
                INSERT (tk_seed_id, tk_id, followee_tk_seed_id, followee_tk_id, trang_thai)
                VALUES (@tk_seed_id, @tk_id, @followee_tk_seed_id, @followee_tk_id, @trang_thai);
            """;

        foreach (var item in DuLieu.TaiKhoanTheoDoiSeed)
        {
            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@tk_seed_id", item.TkSeedId);
            Them(command, "@tk_id", item.TkId);
            Them(command, "@followee_tk_seed_id", item.FolloweeTkSeedId);
            Them(command, "@followee_tk_id", item.FolloweeTkId);
            Them(command, "@trang_thai", item.TrangThai);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task LuuChanAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            MERGE dbo.tk_chan_seed AS target
            USING (SELECT @chan_tk_seed_id AS chan_tk_seed_id, @bi_chan_tk_seed_id AS bi_chan_tk_seed_id) AS source
            ON target.chan_tk_seed_id = source.chan_tk_seed_id AND target.bi_chan_tk_seed_id = source.bi_chan_tk_seed_id
            WHEN MATCHED THEN
                UPDATE SET chan_tk_id = @chan_tk_id, bi_chan_tk_id = @bi_chan_tk_id, trang_thai = @trang_thai
            WHEN NOT MATCHED THEN
                INSERT (chan_tk_seed_id, chan_tk_id, bi_chan_tk_seed_id, bi_chan_tk_id, trang_thai)
                VALUES (@chan_tk_seed_id, @chan_tk_id, @bi_chan_tk_seed_id, @bi_chan_tk_id, @trang_thai);
            """;

        foreach (var item in DuLieu.TaiKhoanChanSeed)
        {
            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@chan_tk_seed_id", item.ChanTkSeedId);
            Them(command, "@chan_tk_id", item.ChanTkId);
            Them(command, "@bi_chan_tk_seed_id", item.BiChanTkSeedId);
            Them(command, "@bi_chan_tk_id", item.BiChanTkId);
            Them(command, "@trang_thai", item.TrangThai);
            await command.ExecuteNonQueryAsync();
        }
    }

    private static async Task XoaBangAsync(SqlConnection connection, SqlTransaction transaction, string tenBang)
    {
        await using var command = TaoLenh($"DELETE FROM {tenBang};", connection, transaction);
        await command.ExecuteNonQueryAsync();
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

    private static int? DocIntNull(SqlDataReader reader, string tenCot)
    {
        return reader[tenCot] == DBNull.Value ? null : Convert.ToInt32(reader[tenCot]);
    }

    private static decimal DocDecimal(SqlDataReader reader, string tenCot)
    {
        return Convert.ToDecimal(reader[tenCot]);
    }

    private static decimal? DocDecimalNull(SqlDataReader reader, string tenCot)
    {
        return reader[tenCot] == DBNull.Value ? null : Convert.ToDecimal(reader[tenCot]);
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
}
