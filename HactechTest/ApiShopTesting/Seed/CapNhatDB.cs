using Microsoft.Data.SqlClient;
namespace HactechTest.ApiShopTesting.Seed;
public sealed class CapNhatDB
{
    private readonly string _chuoiKetNoi;
    public CapNhatDB(string chuoiKetNoi, BoDuLieuSeed duLieu)
    {
        _chuoiKetNoi = chuoiKetNoi;
        DuLieu = duLieu;
    }
    public BoDuLieuSeed DuLieu { get; }
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

    private async Task LuuTaiKhoanAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sqlChuaDangKy = """
            IF @tk_seed_id > 0 AND EXISTS (SELECT 1 FROM dbo.taikhoan_seed WHERE tk_seed_id = @tk_seed_id)
            BEGIN
                UPDATE dbo.taikhoan_seed
                SET sdt = @sdt,
                    mat_khau_hien_tai = @mat_khau_hien_tai,
                    uuid = @uuid,
                    trang_thai = @trang_thai,
                    ghi_chu = @ghi_chu,
                    cap_nhat_luc = SYSDATETIME()
                WHERE tk_seed_id = @tk_seed_id;

                SELECT @tk_seed_id;
            END
            ELSE IF EXISTS (SELECT 1 FROM dbo.taikhoan_seed WHERE sdt = @sdt)
            BEGIN
                UPDATE dbo.taikhoan_seed
                SET mat_khau_hien_tai = @mat_khau_hien_tai,
                    uuid = @uuid,
                    trang_thai = @trang_thai,
                    ghi_chu = @ghi_chu,
                    cap_nhat_luc = SYSDATETIME()
                WHERE sdt = @sdt;

                SELECT tk_seed_id FROM dbo.taikhoan_seed WHERE sdt = @sdt;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.taikhoan_seed
                (sdt, mat_khau_hien_tai, uuid, trang_thai, ghi_chu)
                OUTPUT INSERTED.tk_seed_id
                VALUES
                (@sdt, @mat_khau_hien_tai, @uuid, @trang_thai, @ghi_chu);
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
            if (item.TaiKhoanIdServer <= 0)
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
            await using var commandChuaDangKy = TaoLenh(sqlChuaDangKy, connection, transaction);
            Them(commandChuaDangKy, "@tk_seed_id", item.SoThuTu);
            Them(commandChuaDangKy, "@sdt", item.SoDienThoai);
            Them(commandChuaDangKy, "@mat_khau_hien_tai", item.MatKhauHienTai);
            Them(commandChuaDangKy, "@uuid", item.UuidThietBi);
            Them(commandChuaDangKy, "@trang_thai", item.TrangThai);
            Them(commandChuaDangKy, "@ghi_chu", item.GhiChu);
            var id = await commandChuaDangKy.ExecuteScalarAsync();
            item.SoThuTu = Convert.ToInt32(id);
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
            if (item.FollowerTaiKhoanIdServer is not > 0 || item.FolloweeTaiKhoanIdServer is not > 0)
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
            IF @tk_timkiem_seed_id > 0 AND EXISTS (SELECT 1 FROM dbo.tk_timkiem_seed WHERE tk_timkiem_seed_id = @tk_timkiem_seed_id)
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

                SELECT @tk_timkiem_seed_id;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.tk_timkiem_seed
                (tk_id_server, saved_search_id_server, keyword, trang_thai, tao_boi_test, tao_luc, xoa_luc, ghi_chu)
                OUTPUT INSERTED.tk_timkiem_seed_id
                VALUES
                (@tk_id_server, @saved_search_id_server, @keyword, @trang_thai, @tao_boi_test, @tao_luc, @xoa_luc, @ghi_chu);
            END
            """;

        foreach (var item in DuLieu.TaiKhoanTimKiemSeed)
        {
            if (item.TaiKhoanIdServer is not > 0)
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
            var id = await command.ExecuteScalarAsync();
            item.TaiKhoanTimKiemSeedId = Convert.ToInt32(id);
        }
    }

    private async Task LuuChanAsync(SqlConnection connection, SqlTransaction transaction)
    {
        await using (var xoaCommand = TaoLenh("DELETE FROM dbo.tk_chan_seed;", connection, transaction))
        {
            await xoaCommand.ExecuteNonQueryAsync();
        }

        var blockerChinhId = DuLieu.TaiKhoanChanSeed
            .Where(x => x.TrangThai == "dang_chan" && x.BlockerTaiKhoanIdServer is > 0)
            .GroupBy(x => x.BlockerTaiKhoanIdServer!.Value)
            .OrderByDescending(x => x.Count())
            .ThenBy(x => x.Key)
            .Select(x => x.Key)
            .FirstOrDefault();
        if (blockerChinhId <= 0)
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

        var daGhi = new HashSet<(int BlockerTaiKhoanIdServer, int BlockedTaiKhoanIdServer)>();
        foreach (var item in DuLieu.TaiKhoanChanSeed)
        {
            if (item.BlockerTaiKhoanIdServer is not > 0 ||
                item.BlockedTaiKhoanIdServer is not > 0 ||
                !daGhi.Add((item.BlockerTaiKhoanIdServer.Value, item.BlockedTaiKhoanIdServer.Value)))
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
            IF @diachi_seed_id > 0 AND EXISTS (SELECT 1 FROM dbo.diachi_tk_seed WHERE diachi_seed_id = @diachi_seed_id)
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

                SELECT @diachi_seed_id;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.diachi_tk_seed
                (tk_id_server, diachi_id_server, ward_id, province_id, address, full_address, address_detail, lat, lng, receiver_name, phone, is_default, muc_dich_seed, trang_thai, tao_luc, xac_minh_luc, ghi_chu)
                OUTPUT INSERTED.diachi_seed_id
                VALUES
                (@tk_id_server, @diachi_id_server, @ward_id, @province_id, @address, @full_address, @address_detail, @lat, @lng, @receiver_name, @phone, @is_default, @muc_dich_seed, @trang_thai, @tao_luc, @xac_minh_luc, @ghi_chu);
            END
            """;

        foreach (var item in DuLieu.DiaChiTaiKhoanSeed)
        {
            if (item.TaiKhoanIdServer is not > 0)
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
            var id = await command.ExecuteScalarAsync();
            item.DiaChiSeedId = Convert.ToInt32(id);
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

        foreach (var item in DuLieu.DanhMucSeed.Where(x => x.DanhMucIdServer > 0))
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

        foreach (var item in DuLieu.ThuongHieuSeed.Where(x => x.ThuongHieuIdServer > 0))
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
            if (item.SanPhamIdServer is not > 0 || item.TaiKhoanIdServer is not > 0)
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
            if (item.TaiKhoanIdServer is not > 0 || item.SanPhamIdServer is not > 0)
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
            IF @tn_seed_id > 0 AND EXISTS (SELECT 1 FROM dbo.tinnhan_seed WHERE tn_seed_id = @tn_seed_id)
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

                SELECT @tn_seed_id;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.tinnhan_seed
                (conversation_id_server, message_id_server, sender_tk_id_server, receiver_tk_id_server, product_id_server, type_message, noi_dung, trang_thai, tao_boi_test, gui_luc, ghi_chu)
                OUTPUT INSERTED.tn_seed_id
                VALUES
                (@conversation_id_server, @message_id_server, @sender_tk_id_server, @receiver_tk_id_server, @product_id_server, @type_message, @noi_dung, @trang_thai, @tao_boi_test, @gui_luc, @ghi_chu);
            END
            """;

        foreach (var item in DuLieu.TinNhanSeed)
        {
            if (item.SenderTaiKhoanIdServer is not > 0 ||
                item.ReceiverTaiKhoanIdServer is not > 0)
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
            var id = await command.ExecuteScalarAsync();
            item.TinNhanSeedId = Convert.ToInt32(id);
        }
    }

    private async Task LuuThongBaoAsync(SqlConnection connection, SqlTransaction transaction)
    {
        const string sql = """
            IF EXISTS (SELECT 1 FROM dbo.thongbao_seed WHERE notification_id_server = @notification_id_server)
            BEGIN
                UPDATE dbo.thongbao_seed
                SET tk_id_server = @tk_id_server,
                    title = @title,
                    content = @content,
                    object_id_server = @object_id_server,
                    notification_type = @notification_type,
                    da_doc = @da_doc,
                    trang_thai = @trang_thai,
                    lay_luc = @lay_luc,
                    doc_luc = @doc_luc,
                    ghi_chu = @ghi_chu
                WHERE notification_id_server = @notification_id_server;

                SELECT tb_seed_id FROM dbo.thongbao_seed WHERE notification_id_server = @notification_id_server;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.thongbao_seed
                (notification_id_server, tk_id_server, title, content, object_id_server, notification_type, da_doc, trang_thai, lay_luc, doc_luc, ghi_chu)
                OUTPUT INSERTED.tb_seed_id
                VALUES (@notification_id_server, @tk_id_server, @title, @content, @object_id_server, @notification_type, @da_doc, @trang_thai, @lay_luc, @doc_luc, @ghi_chu);
            END
            """;

        foreach (var item in DuLieu.ThongBaoSeed.Where(x =>
            x.NotificationIdServer is > 0 &&
            x.TaiKhoanIdServer is > 0))
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
            var id = await command.ExecuteScalarAsync();
            item.ThongBaoSeedId = Convert.ToInt32(id);
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
}
