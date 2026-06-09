using Microsoft.Data.SqlClient;

namespace HactechTest.ApiShopTesting.Seed;

public sealed class KhoDuLieuSeedSqlServer
{
    private readonly string _chuoiKetNoi;

    public KhoDuLieuSeedSqlServer(string chuoiKetNoi)
    {
        _chuoiKetNoi = chuoiKetNoi;
    }

    public DuLieuSeed DuLieu { get; private set; } = new();

    public async Task TaiAsync()
    {
        await using var connection = new SqlConnection(_chuoiKetNoi);
        await connection.OpenAsync();
        await KiemTraBangSeedDaCoAsync(connection);

        DuLieu = new DuLieuSeed();
        await TaiTinhThanhAsync(connection);
        await TaiPhuongXaAsync(connection);
        await TaiTaiKhoanAsync(connection);
        await TaiTimKiemAsync(connection);
        await TaiTheoDoiAsync(connection);
        await TaiChanAsync(connection);
        await TaiDiaChiTaiKhoanAsync(connection);
        await TaiDanhMucAsync(connection);
        await TaiThuongHieuAsync(connection);
        await TaiSanPhamAsync(connection);
        await TaiThichSanPhamAsync(connection);
        await TaiTinNhanAsync(connection);
        await TaiThongBaoAsync(connection);

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
            await LuuDiaChiTaiKhoanAsync(connection, (SqlTransaction)transaction);
            await LuuDanhMucAsync(connection, (SqlTransaction)transaction);
            await LuuThuongHieuAsync(connection, (SqlTransaction)transaction);
            await LuuSanPhamAsync(connection, (SqlTransaction)transaction);
            await LuuThichSanPhamAsync(connection, (SqlTransaction)transaction);
            await LuuTinNhanAsync(connection, (SqlTransaction)transaction);
            await LuuThongBaoAsync(connection, (SqlTransaction)transaction);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public TaiKhoanSignupThanhCongSeed? LayTaiKhoanDaDangKy()
    {
        return DuLieu.TaiKhoanSignupThanhCongSeed.FirstOrDefault(x =>
            !string.IsNullOrWhiteSpace(x.TaiKhoanIdServer));
    }

    public TaiKhoanChuaDangKySeed? LayTaiKhoanChuaDangKy()
    {
        return DuLieu.TaiKhoanChuaDangKySeed.FirstOrDefault(x => x.TrangThai == "san_sang");
    }

    public TaiKhoanSignupThanhCongSeed? LayTaiKhoanDaDangKyTheoServerId(string? tkIdServer)
    {
        return DuLieu.TaiKhoanSignupThanhCongSeed.FirstOrDefault(x =>
            !string.IsNullOrWhiteSpace(tkIdServer) &&
            x.TaiKhoanIdServer == tkIdServer);
    }

    public (TaiKhoanSignupThanhCongSeed a, TaiKhoanSignupThanhCongSeed b)? LayHaiTaiKhoanDaDangKy()
    {
        var ds = DuLieu.TaiKhoanSignupThanhCongSeed
            .Where(x => !string.IsNullOrWhiteSpace(x.TaiKhoanIdServer))
            .OrderBy(x => x.SoThuTu)
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
        return DuLieu.TaiKhoanChanSeed
            .Where(x => x.TrangThai == "dang_chan")
            .GroupBy(x => x.BlockerTaiKhoanIdServer)
            .OrderByDescending(x => x.Count())
            .SelectMany(x => x.OrderBy(y => y.ChanSeedId))
            .FirstOrDefault();
    }

    private static async Task KiemTraBangSeedDaCoAsync(SqlConnection connection)
    {
        string[] bangBatBuoc =
        [
            "dbo.taikhoan_seed",
            "dbo.taikhoan_signupthanhcong",
            "dbo.tk_timkiem_seed",
            "dbo.tk_theodoi_seed",
            "dbo.tk_chan_seed",
            "dbo.Provinces_seed",
            "dbo.Wards_seed",
            "dbo.diachi_tk_seed",
            "dbo.danhmuc_seed",
            "dbo.thuonghieu_seed",
            "dbo.sanpham_seed",
            "dbo.tk_thich_sanpham_seed",
            "dbo.tinnhan_seed",
            "dbo.thongbao_seed"
        ];

        var thieu = new List<string>();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT CASE WHEN OBJECT_ID(@ten_bang, N'U') IS NULL THEN 0 ELSE 1 END;";
        var parameter = command.Parameters.Add("@ten_bang", System.Data.SqlDbType.NVarChar, 256);

        foreach (var tenBang in bangBatBuoc)
        {
            parameter.Value = tenBang;
            var tonTai = Convert.ToInt32(await command.ExecuteScalarAsync()) == 1;
            if (!tonTai)
            {
                thieu.Add(tenBang);
            }
        }

        if (thieu.Count > 0)
        {
            throw new InvalidOperationException(
                "Thiếu bảng seed trong SQL Server: " + string.Join(", ", thieu) + ". " +
                "Hãy chạy các file SQL trong thư mục HactechTest\\Database trước khi kiểm tra seed.");
        }
    }
    private async Task TaiTinhThanhAsync(SqlConnection connection)
    {
        const string sql = "SELECT id, name FROM dbo.Provinces_seed ORDER BY id;";
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.TinhThanhSeed.Add(new TinhThanhSeed
            {
                TinhThanhId = DocInt(reader, "id"),
                TenTinhThanh = DocChuoi(reader, "name")
            });
        }
    }

    private async Task TaiPhuongXaAsync(SqlConnection connection)
    {
        const string sql = "SELECT id, name, provinces_id FROM dbo.Wards_seed ORDER BY id;";
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.PhuongXaSeed.Add(new PhuongXaSeed
            {
                PhuongXaId = DocInt(reader, "id"),
                TenPhuongXa = DocChuoi(reader, "name"),
                TinhThanhId = DocInt(reader, "provinces_id")
            });
        }
    }

    private async Task TaiTaiKhoanAsync(SqlConnection connection)
    {
        const string sqlChuaDangKy = """
            SELECT tk_seed_id, sdt, mat_khau_hien_tai, uuid, trang_thai, ghi_chu
            FROM dbo.taikhoan_seed
            ORDER BY tk_seed_id;
            """;

        await using var command = new SqlCommand(sqlChuaDangKy, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.TaiKhoanChuaDangKySeed.Add(new TaiKhoanChuaDangKySeed
            {
                TaiKhoanSeedId = DocInt(reader, "tk_seed_id"),
                SoDienThoai = DocChuoi(reader, "sdt"),
                MatKhauHienTai = DocChuoi(reader, "mat_khau_hien_tai"),
                UuidThietBi = DocChuoi(reader, "uuid"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                GhiChu = DocChuoiNull(reader, "ghi_chu")
            });
        }

        await reader.CloseAsync();

        const string sqlDaDangKy = """
            SELECT tk_id_server, sdt, mat_khau_hien_tai, dang_ky_luc, doi_mk_luc, ghi_chu
            FROM dbo.taikhoan_signupthanhcong
            ORDER BY sdt;
            """;

        await using var commandDaDangKy = new SqlCommand(sqlDaDangKy, connection);
        await using var readerDaDangKy = await commandDaDangKy.ExecuteReaderAsync();
        var fallback = 100000;
        while (await readerDaDangKy.ReadAsync())
        {
            var soDienThoai = DocChuoi(readerDaDangKy, "sdt");
            DuLieu.TaiKhoanSignupThanhCongSeed.Add(new TaiKhoanSignupThanhCongSeed
            {
                TaiKhoanIdServer = DocChuoi(readerDaDangKy, "tk_id_server"),
                SoDienThoai = soDienThoai,
                MatKhauHienTai = DocChuoi(readerDaDangKy, "mat_khau_hien_tai"),
                UuidThietBi = TaoUuidDangNhap(soDienThoai),
                SoThuTu = TaoSoThuTuTuSdt(soDienThoai, fallback++),
                DangKyLuc = DocNgayNull(readerDaDangKy, "dang_ky_luc"),
                DoiMatKhauLuc = DocNgayNull(readerDaDangKy, "doi_mk_luc"),
                GhiChu = DocChuoiNull(readerDaDangKy, "ghi_chu")
            });
        }
    }

    private async Task TaiTheoDoiAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT td_seed_id, follower_tk_id_server, followee_tk_id_server, theo_doi_luc, trang_thai, ghi_chu
            FROM dbo.tk_theodoi_seed;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.TaiKhoanTheoDoiSeed.Add(new TaiKhoanTheoDoiSeed
            {
                TheoDoiSeedId = DocInt(reader, "td_seed_id"),
                FollowerTaiKhoanIdServer = DocChuoiNull(reader, "follower_tk_id_server"),
                FolloweeTaiKhoanIdServer = DocChuoiNull(reader, "followee_tk_id_server"),
                TheoDoiLuc = DocNgayNull(reader, "theo_doi_luc"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                GhiChu = DocChuoiNull(reader, "ghi_chu")
            });
        }
    }

    private async Task TaiTimKiemAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT tk_timkiem_seed_id, tk_id_server, saved_search_id_server, keyword, trang_thai, tao_boi_test, tao_luc, xoa_luc, ghi_chu
            FROM dbo.tk_timkiem_seed
            ORDER BY tk_timkiem_seed_id;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.TaiKhoanTimKiemSeed.Add(new TaiKhoanTimKiemSeed
            {
                TaiKhoanTimKiemSeedId = DocInt(reader, "tk_timkiem_seed_id"),
                TaiKhoanIdServer = DocChuoiNull(reader, "tk_id_server"),
                SavedSearchIdServer = DocChuoiNull(reader, "saved_search_id_server"),
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
            SELECT chan_seed_id, blocker_tk_id_server, blocked_tk_id_server, chan_luc, trang_thai, ghi_chu
            FROM dbo.tk_chan_seed;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.TaiKhoanChanSeed.Add(new TaiKhoanChanSeed
            {
                ChanSeedId = DocInt(reader, "chan_seed_id"),
                BlockerTaiKhoanIdServer = DocChuoiNull(reader, "blocker_tk_id_server"),
                BlockedTaiKhoanIdServer = DocChuoiNull(reader, "blocked_tk_id_server"),
                ChanLuc = DocNgayNull(reader, "chan_luc"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                GhiChu = DocChuoiNull(reader, "ghi_chu")
            });
        }
    }

    private async Task TaiDiaChiTaiKhoanAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT diachi_seed_id, tk_id_server, diachi_id_server, ward_id, province_id,
                   address, full_address, address_detail, lat, lng, receiver_name, phone, is_default,
                   muc_dich_seed, trang_thai, tao_luc, xac_minh_luc, ghi_chu
            FROM dbo.diachi_tk_seed
            ORDER BY diachi_seed_id;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.DiaChiTaiKhoanSeed.Add(new DiaChiTaiKhoanSeed
            {
                DiaChiSeedId = DocInt(reader, "diachi_seed_id"),
                TaiKhoanIdServer = DocChuoiNull(reader, "tk_id_server"),
                DiaChiIdServer = DocChuoiNull(reader, "diachi_id_server"),
                PhuongXaId = DocInt(reader, "ward_id"),
                TinhThanhId = DocInt(reader, "province_id"),
                DiaChi = DocChuoi(reader, "address"),
                DiaChiDayDu = DocChuoi(reader, "full_address"),
                DiaChiChiTiet = DocChuoi(reader, "address_detail"),
                ViDo = DocDecimal(reader, "lat"),
                KinhDo = DocDecimal(reader, "lng"),
                TenNguoiNhan = DocChuoi(reader, "receiver_name"),
                SoDienThoaiNguoiNhan = DocChuoi(reader, "phone"),
                LaMacDinh = DocBool(reader, "is_default"),
                MucDichSeed = DocChuoi(reader, "muc_dich_seed"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                TaoLuc = DocNgayNull(reader, "tao_luc"),
                XacMinhLuc = DocNgayNull(reader, "xac_minh_luc"),
                GhiChu = DocChuoiNull(reader, "ghi_chu")
            });
        }
    }

    private async Task TaiDanhMucAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT dm_id_server, ten_danh_muc, dm_cha_id_server, co_danh_muc_con, co_thuong_hieu,
                   co_kich_co, yeu_cau_can_nang, trang_thai, dong_bo_luc
            FROM dbo.danhmuc_seed
            ORDER BY ten_danh_muc;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.DanhMucSeed.Add(new DanhMucSeed
            {
                DanhMucIdServer = DocChuoi(reader, "dm_id_server"),
                TenDanhMuc = DocChuoi(reader, "ten_danh_muc"),
                DanhMucChaIdServer = DocChuoiNull(reader, "dm_cha_id_server"),
                CoDanhMucCon = DocBoolNull(reader, "co_danh_muc_con"),
                CoThuongHieu = DocBoolNull(reader, "co_thuong_hieu"),
                CoKichCo = DocBoolNull(reader, "co_kich_co"),
                YeuCauCanNang = DocBoolNull(reader, "yeu_cau_can_nang"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                DongBoLuc = DocNgayNull(reader, "dong_bo_luc")
            });
        }
    }

    private async Task TaiThuongHieuAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT thuonghieu_id_server, ten_thuong_hieu, dm_id_server, trang_thai, dong_bo_luc
            FROM dbo.thuonghieu_seed
            ORDER BY ten_thuong_hieu;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.ThuongHieuSeed.Add(new ThuongHieuSeed
            {
                ThuongHieuIdServer = DocChuoi(reader, "thuonghieu_id_server"),
                TenThuongHieu = DocChuoi(reader, "ten_thuong_hieu"),
                DanhMucIdServer = DocChuoiNull(reader, "dm_id_server"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                DongBoLuc = DocNgayNull(reader, "dong_bo_luc")
            });
        }
    }

    private async Task TaiSanPhamAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT sp_id_server, tk_id_server, dm_id_server, thuonghieu_id_server,
                   ship_from_id_server, ten_sp, gia, trang_thai, tao_boi_test, tao_luc, xac_minh_luc, ghi_chu
            FROM dbo.sanpham_seed
            ORDER BY sp_id_server;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        var thuTu = 1;
        while (await reader.ReadAsync())
        {
            DuLieu.SanPhamSeed.Add(new SanPhamSeed
            {
                ThuTuNoiBo = thuTu++,
                SanPhamIdServer = DocChuoi(reader, "sp_id_server"),
                TaiKhoanIdServer = DocChuoiNull(reader, "tk_id_server"),
                DanhMucIdServer = DocChuoiNull(reader, "dm_id_server"),
                ThuongHieuIdServer = DocChuoiNull(reader, "thuonghieu_id_server"),
                DiaChiGuiHangIdServer = DocChuoiNull(reader, "ship_from_id_server"),
                TenSanPham = DocChuoi(reader, "ten_sp"),
                Gia = DocDecimal(reader, "gia"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                TaoBoiTest = DocBool(reader, "tao_boi_test"),
                TaoLuc = DocNgayNull(reader, "tao_luc"),
                XacMinhLuc = DocNgayNull(reader, "xac_minh_luc"),
                GhiChu = DocChuoiNull(reader, "ghi_chu")
            });
        }
    }

    private async Task TaiThichSanPhamAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT thich_seed_id, tk_id_server, sp_id_server, thich_luc, trang_thai, ghi_chu
            FROM dbo.tk_thich_sanpham_seed
            ORDER BY thich_seed_id;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.TaiKhoanThichSanPhamSeed.Add(new TaiKhoanThichSanPhamSeed
            {
                ThichSanPhamSeedId = DocInt(reader, "thich_seed_id"),
                TaiKhoanIdServer = DocChuoiNull(reader, "tk_id_server"),
                SanPhamIdServer = DocChuoiNull(reader, "sp_id_server"),
                ThichLuc = DocNgayNull(reader, "thich_luc"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                GhiChu = DocChuoiNull(reader, "ghi_chu")
            });
        }
    }

    private async Task TaiTinNhanAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT tn_seed_id, conversation_id_server, message_id_server, sender_tk_id_server,
                   receiver_tk_id_server, product_id_server, type_message, noi_dung,
                   trang_thai, tao_boi_test, gui_luc, ghi_chu
            FROM dbo.tinnhan_seed
            ORDER BY tn_seed_id;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.TinNhanSeed.Add(new TinNhanSeed
            {
                TinNhanSeedId = DocInt(reader, "tn_seed_id"),
                ConversationIdServer = DocChuoiNull(reader, "conversation_id_server"),
                MessageIdServer = DocChuoiNull(reader, "message_id_server"),
                SenderTaiKhoanIdServer = DocChuoiNull(reader, "sender_tk_id_server"),
                ReceiverTaiKhoanIdServer = DocChuoiNull(reader, "receiver_tk_id_server"),
                SanPhamIdServer = DocChuoiNull(reader, "product_id_server"),
                TypeMessage = DocChuoi(reader, "type_message"),
                NoiDung = DocChuoiNull(reader, "noi_dung"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                TaoBoiTest = DocBool(reader, "tao_boi_test"),
                GuiLuc = DocNgayNull(reader, "gui_luc"),
                GhiChu = DocChuoiNull(reader, "ghi_chu")
            });
        }
    }

    private async Task TaiThongBaoAsync(SqlConnection connection)
    {
        const string sql = """
            SELECT tb_seed_id, notification_id_server, tk_id_server, title, content,
                   object_id_server, notification_type, da_doc, trang_thai, lay_luc, doc_luc, ghi_chu
            FROM dbo.thongbao_seed
            ORDER BY tb_seed_id;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            DuLieu.ThongBaoSeed.Add(new ThongBaoSeed
            {
                ThongBaoSeedId = DocInt(reader, "tb_seed_id"),
                NotificationIdServer = DocChuoiNull(reader, "notification_id_server"),
                TaiKhoanIdServer = DocChuoiNull(reader, "tk_id_server"),
                Title = DocChuoiNull(reader, "title"),
                Content = DocChuoiNull(reader, "content"),
                ObjectIdServer = DocChuoiNull(reader, "object_id_server"),
                NotificationType = DocChuoiNull(reader, "notification_type"),
                DaDoc = DocBoolNull(reader, "da_doc"),
                TrangThai = DocChuoi(reader, "trang_thai"),
                LayLuc = DocNgayNull(reader, "lay_luc"),
                DocLuc = DocNgayNull(reader, "doc_luc"),
                GhiChu = DocChuoiNull(reader, "ghi_chu")
            });
        }
    }

    private async Task LuuTaiKhoanAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sqlChuaDangKy = """
            IF EXISTS (SELECT 1 FROM dbo.taikhoan_seed WHERE tk_seed_id = @tk_seed_id)
            BEGIN
                UPDATE dbo.taikhoan_seed
                SET sdt = @sdt,
                    mat_khau_hien_tai = @mat_khau_hien_tai,
                    uuid = @uuid,
                    trang_thai = @trang_thai,
                    ghi_chu = @ghi_chu,
                    cap_nhat_luc = SYSDATETIME()
                WHERE tk_seed_id = @tk_seed_id;
            END
            ELSE
            BEGIN
                SET IDENTITY_INSERT dbo.taikhoan_seed ON;
                INSERT INTO dbo.taikhoan_seed
                (tk_seed_id, sdt, mat_khau_hien_tai, uuid, trang_thai, ghi_chu)
                VALUES
                (@tk_seed_id, @sdt, @mat_khau_hien_tai, @uuid, @trang_thai, @ghi_chu);
                SET IDENTITY_INSERT dbo.taikhoan_seed OFF;
            END
            """;

        const string sqlDaDangKy = """
            MERGE dbo.taikhoan_signupthanhcong AS target
            USING (SELECT @tk_id_server AS tk_id_server) AS source
            ON target.tk_id_server = source.tk_id_server
            WHEN MATCHED THEN
                UPDATE SET sdt = @sdt,
                    mat_khau_hien_tai = @mat_khau_hien_tai,
                    doi_mk_luc = @doi_mk_luc,
                    ghi_chu = @ghi_chu
            WHEN NOT MATCHED THEN
                INSERT (tk_id_server, sdt, mat_khau_hien_tai, dang_ky_luc, doi_mk_luc, ghi_chu)
                VALUES (@tk_id_server, @sdt, @mat_khau_hien_tai, @dang_ky_luc, @doi_mk_luc, @ghi_chu);

            DELETE FROM dbo.taikhoan_seed WHERE sdt = @sdt;
            """;

        foreach (var item in DuLieu.TaiKhoanSignupThanhCongSeed)
        {
            if (string.IsNullOrWhiteSpace(item.TaiKhoanIdServer))
            {
                continue;
            }

            await using var command = TaoLenh(sqlDaDangKy, connection, transaction);
            Them(command, "@tk_id_server", item.TaiKhoanIdServer);
            Them(command, "@sdt", item.SoDienThoai);
            Them(command, "@mat_khau_hien_tai", item.MatKhauHienTai);
            Them(command, "@dang_ky_luc", item.DangKyLuc ?? DateTimeOffset.Now);
            Them(command, "@doi_mk_luc", item.DoiMatKhauLuc);
            Them(command, "@ghi_chu", item.GhiChu);
            await command.ExecuteNonQueryAsync();
        }

        var sdtDaDangKy = DuLieu.TaiKhoanSignupThanhCongSeed
            .Select(x => x.SoDienThoai)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet();
        DuLieu.TaiKhoanChuaDangKySeed.RemoveAll(x => sdtDaDangKy.Contains(x.SoDienThoai));

        foreach (var item in DuLieu.TaiKhoanChuaDangKySeed)
        {
            if (item.SoThuTu <= 0)
            {
                continue;
            }

            await using var commandChuaDangKy = TaoLenh(sqlChuaDangKy, connection, transaction);
            Them(commandChuaDangKy, "@tk_seed_id", item.SoThuTu);
            Them(commandChuaDangKy, "@sdt", item.SoDienThoai);
            Them(commandChuaDangKy, "@mat_khau_hien_tai", item.MatKhauHienTai);
            Them(commandChuaDangKy, "@uuid", item.UuidThietBi);
            Them(commandChuaDangKy, "@trang_thai", item.TrangThai);
            Them(commandChuaDangKy, "@ghi_chu", item.GhiChu);
            await commandChuaDangKy.ExecuteNonQueryAsync();
        }
    }

    private async Task LuuTheoDoiAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            MERGE dbo.tk_theodoi_seed AS target
            USING (SELECT @follower_tk_id_server AS follower_tk_id_server, @followee_tk_id_server AS followee_tk_id_server) AS source
            ON target.follower_tk_id_server = source.follower_tk_id_server AND target.followee_tk_id_server = source.followee_tk_id_server
            WHEN MATCHED THEN
                UPDATE SET theo_doi_luc = @theo_doi_luc,
                    trang_thai = @trang_thai,
                    ghi_chu = @ghi_chu
            WHEN NOT MATCHED THEN
                INSERT (follower_tk_id_server, followee_tk_id_server, theo_doi_luc, trang_thai, ghi_chu)
                VALUES (@follower_tk_id_server, @followee_tk_id_server, @theo_doi_luc, @trang_thai, @ghi_chu);
            """;

        foreach (var item in DuLieu.TaiKhoanTheoDoiSeed)
        {
            if (string.IsNullOrWhiteSpace(item.FollowerTaiKhoanIdServer) || string.IsNullOrWhiteSpace(item.FolloweeTaiKhoanIdServer))
            {
                continue;
            }

            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@follower_tk_id_server", item.FollowerTaiKhoanIdServer);
            Them(command, "@followee_tk_id_server", item.FolloweeTaiKhoanIdServer);
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
                SET tk_id_server = @tk_id_server,
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
                (tk_timkiem_seed_id, tk_id_server, saved_search_id_server, keyword, trang_thai, tao_boi_test, tao_luc, xoa_luc, ghi_chu)
                VALUES
                (@tk_timkiem_seed_id, @tk_id_server, @saved_search_id_server, @keyword, @trang_thai, @tao_boi_test, @tao_luc, @xoa_luc, @ghi_chu);
                SET IDENTITY_INSERT dbo.tk_timkiem_seed OFF;
            END
            """;

        foreach (var item in DuLieu.TaiKhoanTimKiemSeed)
        {
            if (string.IsNullOrWhiteSpace(item.TaiKhoanIdServer))
            {
                continue;
            }

            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@tk_timkiem_seed_id", item.TaiKhoanTimKiemSeedId);
            Them(command, "@tk_id_server", item.TaiKhoanIdServer);
            Them(command, "@saved_search_id_server", item.SavedSearchIdServer);
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
        await using (var xoaCommand = TaoLenh("DELETE FROM dbo.tk_chan_seed;", connection, transaction))
        {
            await xoaCommand.ExecuteNonQueryAsync();
        }

        var blockerChinhId = DuLieu.TaiKhoanChanSeed
            .Where(x => x.TrangThai == "dang_chan" && !string.IsNullOrWhiteSpace(x.BlockerTaiKhoanIdServer))
            .GroupBy(x => x.BlockerTaiKhoanIdServer!)
            .OrderByDescending(x => x.Count())
            .ThenBy(x => x.Key)
            .Select(x => x.Key)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(blockerChinhId))
        {
            DuLieu.TaiKhoanChanSeed.RemoveAll(x => x.TrangThai != "dang_chan");
            return;
        }

        DuLieu.TaiKhoanChanSeed.RemoveAll(x =>
            x.TrangThai != "dang_chan" ||
            x.BlockerTaiKhoanIdServer != blockerChinhId);

        const string sql = """
            INSERT INTO dbo.tk_chan_seed
            (blocker_tk_id_server, blocked_tk_id_server, chan_luc, trang_thai, ghi_chu)
            OUTPUT INSERTED.chan_seed_id
            VALUES (@blocker_tk_id_server, @blocked_tk_id_server, @chan_luc, @trang_thai, @ghi_chu);
            """;

        var daGhi = new HashSet<(string BlockerTaiKhoanIdServer, string BlockedTaiKhoanIdServer)>();
        foreach (var item in DuLieu.TaiKhoanChanSeed)
        {
            if (string.IsNullOrWhiteSpace(item.BlockerTaiKhoanIdServer) ||
                string.IsNullOrWhiteSpace(item.BlockedTaiKhoanIdServer) ||
                !daGhi.Add((item.BlockerTaiKhoanIdServer, item.BlockedTaiKhoanIdServer)))
            {
                continue;
            }

            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@blocker_tk_id_server", item.BlockerTaiKhoanIdServer);
            Them(command, "@blocked_tk_id_server", item.BlockedTaiKhoanIdServer);
            Them(command, "@chan_luc", item.ChanLuc ?? DateTimeOffset.Now);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@ghi_chu", item.GhiChu);
            var id = await command.ExecuteScalarAsync();
            item.ChanSeedId = Convert.ToInt32(id);
        }
    }

    private async Task LuuDiaChiTaiKhoanAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            IF EXISTS (SELECT 1 FROM dbo.diachi_tk_seed WHERE diachi_seed_id = @diachi_seed_id)
            BEGIN
                UPDATE dbo.diachi_tk_seed
                SET tk_id_server = @tk_id_server,
                    diachi_id_server = @diachi_id_server,
                    ward_id = @ward_id,
                    province_id = @province_id,
                    address = @address,
                    full_address = @full_address,
                    address_detail = @address_detail,
                    lat = @lat,
                    lng = @lng,
                    receiver_name = @receiver_name,
                    phone = @phone,
                    is_default = @is_default,
                    muc_dich_seed = @muc_dich_seed,
                    trang_thai = @trang_thai,
                    tao_luc = @tao_luc,
                    xac_minh_luc = @xac_minh_luc,
                    ghi_chu = @ghi_chu
                WHERE diachi_seed_id = @diachi_seed_id;
            END
            ELSE
            BEGIN
                SET IDENTITY_INSERT dbo.diachi_tk_seed ON;
                INSERT INTO dbo.diachi_tk_seed
                (diachi_seed_id, tk_id_server, diachi_id_server, ward_id, province_id, address, full_address, address_detail, lat, lng, receiver_name, phone, is_default, muc_dich_seed, trang_thai, tao_luc, xac_minh_luc, ghi_chu)
                VALUES
                (@diachi_seed_id, @tk_id_server, @diachi_id_server, @ward_id, @province_id, @address, @full_address, @address_detail, @lat, @lng, @receiver_name, @phone, @is_default, @muc_dich_seed, @trang_thai, @tao_luc, @xac_minh_luc, @ghi_chu);
                SET IDENTITY_INSERT dbo.diachi_tk_seed OFF;
            END
            """;

        foreach (var item in DuLieu.DiaChiTaiKhoanSeed)
        {
            if (item.DiaChiSeedId <= 0 || string.IsNullOrWhiteSpace(item.TaiKhoanIdServer))
            {
                continue;
            }

            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@diachi_seed_id", item.DiaChiSeedId);
            Them(command, "@tk_id_server", item.TaiKhoanIdServer);
            Them(command, "@diachi_id_server", item.DiaChiIdServer);
            Them(command, "@ward_id", item.PhuongXaId);
            Them(command, "@province_id", item.TinhThanhId);
            Them(command, "@address", item.DiaChi);
            Them(command, "@full_address", item.DiaChiDayDu);
            Them(command, "@address_detail", item.DiaChiChiTiet);
            Them(command, "@lat", item.ViDo);
            Them(command, "@lng", item.KinhDo);
            Them(command, "@receiver_name", item.TenNguoiNhan);
            Them(command, "@phone", item.SoDienThoaiNguoiNhan);
            Them(command, "@is_default", item.LaMacDinh);
            Them(command, "@muc_dich_seed", item.MucDichSeed);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@tao_luc", item.TaoLuc);
            Them(command, "@xac_minh_luc", item.XacMinhLuc);
            Them(command, "@ghi_chu", item.GhiChu);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task LuuDanhMucAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            MERGE dbo.danhmuc_seed AS target
            USING (SELECT @dm_id_server AS dm_id_server) AS source
            ON target.dm_id_server = source.dm_id_server
            WHEN MATCHED THEN
                UPDATE SET ten_danh_muc = @ten_danh_muc,
                    dm_cha_id_server = @dm_cha_id_server,
                    co_danh_muc_con = @co_danh_muc_con,
                    co_thuong_hieu = @co_thuong_hieu,
                    co_kich_co = @co_kich_co,
                    yeu_cau_can_nang = @yeu_cau_can_nang,
                    trang_thai = @trang_thai,
                    dong_bo_luc = @dong_bo_luc
            WHEN NOT MATCHED THEN
                INSERT (dm_id_server, ten_danh_muc, dm_cha_id_server, co_danh_muc_con, co_thuong_hieu, co_kich_co, yeu_cau_can_nang, trang_thai, dong_bo_luc)
                VALUES (@dm_id_server, @ten_danh_muc, @dm_cha_id_server, @co_danh_muc_con, @co_thuong_hieu, @co_kich_co, @yeu_cau_can_nang, @trang_thai, @dong_bo_luc);
            """;

        foreach (var item in DuLieu.DanhMucSeed.Where(x => !string.IsNullOrWhiteSpace(x.DanhMucIdServer)))
        {
            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@dm_id_server", item.DanhMucIdServer);
            Them(command, "@ten_danh_muc", item.TenDanhMuc);
            Them(command, "@dm_cha_id_server", item.DanhMucChaIdServer);
            Them(command, "@co_danh_muc_con", item.CoDanhMucCon);
            Them(command, "@co_thuong_hieu", item.CoThuongHieu);
            Them(command, "@co_kich_co", item.CoKichCo);
            Them(command, "@yeu_cau_can_nang", item.YeuCauCanNang);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@dong_bo_luc", item.DongBoLuc);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task LuuThuongHieuAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            MERGE dbo.thuonghieu_seed AS target
            USING (SELECT @thuonghieu_id_server AS thuonghieu_id_server) AS source
            ON target.thuonghieu_id_server = source.thuonghieu_id_server
            WHEN MATCHED THEN
                UPDATE SET ten_thuong_hieu = @ten_thuong_hieu,
                    dm_id_server = @dm_id_server,
                    trang_thai = @trang_thai,
                    dong_bo_luc = @dong_bo_luc
            WHEN NOT MATCHED THEN
                INSERT (thuonghieu_id_server, ten_thuong_hieu, dm_id_server, trang_thai, dong_bo_luc)
                VALUES (@thuonghieu_id_server, @ten_thuong_hieu, @dm_id_server, @trang_thai, @dong_bo_luc);
            """;

        foreach (var item in DuLieu.ThuongHieuSeed.Where(x => !string.IsNullOrWhiteSpace(x.ThuongHieuIdServer)))
        {
            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@thuonghieu_id_server", item.ThuongHieuIdServer);
            Them(command, "@ten_thuong_hieu", item.TenThuongHieu);
            Them(command, "@dm_id_server", item.DanhMucIdServer);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@dong_bo_luc", item.DongBoLuc);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task LuuSanPhamAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            MERGE dbo.sanpham_seed AS target
            USING (SELECT @sp_id_server AS sp_id_server) AS source
            ON target.sp_id_server = source.sp_id_server
            WHEN MATCHED THEN
                UPDATE SET tk_id_server = @tk_id_server,
                    dm_id_server = @dm_id_server,
                    thuonghieu_id_server = @thuonghieu_id_server,
                    ship_from_id_server = @ship_from_id_server,
                    ten_sp = @ten_sp,
                    gia = @gia,
                    trang_thai = @trang_thai,
                    tao_boi_test = @tao_boi_test,
                    tao_luc = @tao_luc,
                    xac_minh_luc = @xac_minh_luc,
                    ghi_chu = @ghi_chu
            WHEN NOT MATCHED THEN
                INSERT (sp_id_server, tk_id_server, dm_id_server, thuonghieu_id_server, ship_from_id_server, ten_sp, gia, trang_thai, tao_boi_test, tao_luc, xac_minh_luc, ghi_chu)
                VALUES (@sp_id_server, @tk_id_server, @dm_id_server, @thuonghieu_id_server, @ship_from_id_server, @ten_sp, @gia, @trang_thai, @tao_boi_test, @tao_luc, @xac_minh_luc, @ghi_chu);
            """;

        foreach (var item in DuLieu.SanPhamSeed)
        {
            if (string.IsNullOrWhiteSpace(item.SanPhamIdServer) || string.IsNullOrWhiteSpace(item.TaiKhoanIdServer))
            {
                continue;
            }

            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@sp_id_server", item.SanPhamIdServer);
            Them(command, "@tk_id_server", item.TaiKhoanIdServer);
            Them(command, "@dm_id_server", item.DanhMucIdServer);
            Them(command, "@thuonghieu_id_server", item.ThuongHieuIdServer);
            Them(command, "@ship_from_id_server", item.DiaChiGuiHangIdServer);
            Them(command, "@ten_sp", item.TenSanPham);
            Them(command, "@gia", item.Gia);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@tao_boi_test", item.TaoBoiTest);
            Them(command, "@tao_luc", item.TaoLuc);
            Them(command, "@xac_minh_luc", item.XacMinhLuc);
            Them(command, "@ghi_chu", item.GhiChu);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task LuuThichSanPhamAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            MERGE dbo.tk_thich_sanpham_seed AS target
            USING (SELECT @tk_id_server AS tk_id_server, @sp_id_server AS sp_id_server) AS source
            ON target.tk_id_server = source.tk_id_server AND target.sp_id_server = source.sp_id_server
            WHEN MATCHED THEN
                UPDATE SET thich_luc = @thich_luc,
                    trang_thai = @trang_thai,
                    ghi_chu = @ghi_chu
            WHEN NOT MATCHED THEN
                INSERT (tk_id_server, sp_id_server, thich_luc, trang_thai, ghi_chu)
                VALUES (@tk_id_server, @sp_id_server, @thich_luc, @trang_thai, @ghi_chu);
            """;

        foreach (var item in DuLieu.TaiKhoanThichSanPhamSeed)
        {
            if (string.IsNullOrWhiteSpace(item.TaiKhoanIdServer) || string.IsNullOrWhiteSpace(item.SanPhamIdServer))
            {
                continue;
            }

            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@tk_id_server", item.TaiKhoanIdServer);
            Them(command, "@sp_id_server", item.SanPhamIdServer);
            Them(command, "@thich_luc", item.ThichLuc ?? DateTimeOffset.Now);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@ghi_chu", item.GhiChu);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task LuuTinNhanAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            IF EXISTS (SELECT 1 FROM dbo.tinnhan_seed WHERE tn_seed_id = @tn_seed_id)
            BEGIN
                UPDATE dbo.tinnhan_seed
                SET conversation_id_server = @conversation_id_server,
                    message_id_server = @message_id_server,
                    sender_tk_id_server = @sender_tk_id_server,
                    receiver_tk_id_server = @receiver_tk_id_server,
                    product_id_server = @product_id_server,
                    type_message = @type_message,
                    noi_dung = @noi_dung,
                    trang_thai = @trang_thai,
                    tao_boi_test = @tao_boi_test,
                    gui_luc = @gui_luc,
                    ghi_chu = @ghi_chu
                WHERE tn_seed_id = @tn_seed_id;
            END
            ELSE
            BEGIN
                SET IDENTITY_INSERT dbo.tinnhan_seed ON;
                INSERT INTO dbo.tinnhan_seed
                (tn_seed_id, conversation_id_server, message_id_server, sender_tk_id_server, receiver_tk_id_server, product_id_server, type_message, noi_dung, trang_thai, tao_boi_test, gui_luc, ghi_chu)
                VALUES
                (@tn_seed_id, @conversation_id_server, @message_id_server, @sender_tk_id_server, @receiver_tk_id_server, @product_id_server, @type_message, @noi_dung, @trang_thai, @tao_boi_test, @gui_luc, @ghi_chu);
                SET IDENTITY_INSERT dbo.tinnhan_seed OFF;
            END
            """;

        foreach (var item in DuLieu.TinNhanSeed)
        {
            if (item.TinNhanSeedId <= 0 ||
                string.IsNullOrWhiteSpace(item.SenderTaiKhoanIdServer) ||
                string.IsNullOrWhiteSpace(item.ReceiverTaiKhoanIdServer))
            {
                continue;
            }

            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@tn_seed_id", item.TinNhanSeedId);
            Them(command, "@conversation_id_server", item.ConversationIdServer);
            Them(command, "@message_id_server", item.MessageIdServer);
            Them(command, "@sender_tk_id_server", item.SenderTaiKhoanIdServer);
            Them(command, "@receiver_tk_id_server", item.ReceiverTaiKhoanIdServer);
            Them(command, "@product_id_server", item.SanPhamIdServer);
            Them(command, "@type_message", item.TypeMessage);
            Them(command, "@noi_dung", item.NoiDung);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@tao_boi_test", item.TaoBoiTest);
            Them(command, "@gui_luc", item.GuiLuc);
            Them(command, "@ghi_chu", item.GhiChu);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task LuuThongBaoAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            MERGE dbo.thongbao_seed AS target
            USING (SELECT @notification_id_server AS notification_id_server) AS source
            ON target.notification_id_server = source.notification_id_server
            WHEN MATCHED THEN
                UPDATE SET tk_id_server = @tk_id_server,
                    title = @title,
                    content = @content,
                    object_id_server = @object_id_server,
                    notification_type = @notification_type,
                    da_doc = @da_doc,
                    trang_thai = @trang_thai,
                    lay_luc = @lay_luc,
                    doc_luc = @doc_luc,
                    ghi_chu = @ghi_chu
            WHEN NOT MATCHED THEN
                INSERT (notification_id_server, tk_id_server, title, content, object_id_server, notification_type, da_doc, trang_thai, lay_luc, doc_luc, ghi_chu)
                VALUES (@notification_id_server, @tk_id_server, @title, @content, @object_id_server, @notification_type, @da_doc, @trang_thai, @lay_luc, @doc_luc, @ghi_chu);
            """;

        foreach (var item in DuLieu.ThongBaoSeed.Where(x =>
            !string.IsNullOrWhiteSpace(x.NotificationIdServer) &&
            !string.IsNullOrWhiteSpace(x.TaiKhoanIdServer)))
        {
            await using var command = TaoLenh(sql, connection, transaction);
            Them(command, "@notification_id_server", item.NotificationIdServer);
            Them(command, "@tk_id_server", item.TaiKhoanIdServer);
            Them(command, "@title", item.Title);
            Them(command, "@content", item.Content);
            Them(command, "@object_id_server", item.ObjectIdServer);
            Them(command, "@notification_type", item.NotificationType);
            Them(command, "@da_doc", item.DaDoc);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@lay_luc", item.LayLuc);
            Them(command, "@doc_luc", item.DocLuc);
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

    private static bool? DocBoolNull(SqlDataReader reader, string tenCot)
    {
        return reader[tenCot] == DBNull.Value ? null : Convert.ToBoolean(reader[tenCot]);
    }

    private static decimal DocDecimal(SqlDataReader reader, string tenCot)
    {
        return Convert.ToDecimal(reader[tenCot]);
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

    private static int TaoSoThuTuTuSdt(string soDienThoai, int fallback)
    {
        var chuSo = new string(soDienThoai.Where(char.IsDigit).ToArray());
        if (chuSo.Length >= 4 &&
            int.TryParse(chuSo[^Math.Min(6, chuSo.Length)..], out var soThuTu))
        {
            return soThuTu;
        }

        return fallback;
    }

    public static string TaoUuidDangNhap(string soDienThoai)
    {
        var chuSo = new string(soDienThoai.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(chuSo)
            ? "thiet-bi-test-seed"
            : "thiet-bi-test-" + chuSo;
    }

}





