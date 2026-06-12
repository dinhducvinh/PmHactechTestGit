using Microsoft.Data.SqlClient;
namespace HactechTest.ApiShopTesting.Seed
{
    public sealed class SeedLoadCheck
    {
        public event Action<string>? TrangThaiThayDoi;

        private BoDuLieuSeed _duLieuDangTai = new();
        public async Task<BoDuLieuSeed> TaiAsync(string chuoiKetNoi, CancellationToken ct = default)
        {
            await using var connection = new SqlConnection(chuoiKetNoi);
            await connection.OpenAsync(ct);
            _duLieuDangTai = new BoDuLieuSeed();
            await TaiTinhThanhAsync(connection);
            await TaiPhuongXaAsync(connection);
            await TaiTaiKhoanAsync(connection);
            await TaiWalletAsync(connection);
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
            return _duLieuDangTai;
        }

        public async Task<KetQuaKiemTraDuLieuSeed> KiemTraAsync(string chuoiKetNoi, CancellationToken ct = default)
        {
            BaoTrangThai("Đang kết nối database seed...");
            var duLieu = await TaiAsync(chuoiKetNoi, ct);
            ct.ThrowIfCancellationRequested();

            BaoTrangThai("Đang kiểm tra dữ liệu seed trong SQL Server...");
            var thongKe = TaoThongKeSeed(duLieu);
            return new KetQuaKiemTraDuLieuSeed
            {
                DuLieu = duLieu,
                ThongKe = thongKe,
                KeHoach = TaoKeHoachChuanBiDuLieu(thongKe),
                DuLieuConThieu = LietKeDuLieuConThieu(thongKe)
            };
        }

        private async Task TaiTinhThanhAsync(SqlConnection connection)
        {
            const string sql = "SELECT id, name FROM dbo.Provinces_seed ORDER BY id;";
            await using var command = new SqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                _duLieuDangTai.TinhThanhSeed.Add(new TinhThanhSeed
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
                _duLieuDangTai.PhuongXaSeed.Add(new PhuongXaSeed
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
                _duLieuDangTai.TaiKhoanChuaDangKySeed.Add(new TaiKhoanChuaDangKySeed
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
                _duLieuDangTai.TaiKhoanSignupThanhCongSeed.Add(new TaiKhoanSignupThanhCongSeed
                {
                    TaiKhoanIdServer = DocInt(readerDaDangKy, "tk_id_server"),
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

        private async Task TaiWalletAsync(SqlConnection connection)
        {
            const string sql = """
            SELECT wallet_id_server, tk_id_server, balance, available_balance, pending_balance,
                   trang_thai, tao_luc, xac_minh_luc, ghi_chu
            FROM dbo.wallet_seed
            ORDER BY tk_id_server;
            """;

            await using var command = new SqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                _duLieuDangTai.WalletSeed.Add(new WalletSeed
                {
                    WalletIdServer = DocChuoi(reader, "wallet_id_server"),
                    TaiKhoanIdServer = DocIntNull(reader, "tk_id_server"),
                    Balance = DocDecimal(reader, "balance"),
                    AvailableBalance = DocDecimalNull(reader, "available_balance"),
                    PendingBalance = DocDecimalNull(reader, "pending_balance"),
                    TrangThai = DocChuoiNull(reader, "trang_thai"),
                    TaoLuc = DocNgayNull(reader, "tao_luc"),
                    XacMinhLuc = DocNgayNull(reader, "xac_minh_luc"),
                    GhiChu = DocChuoiNull(reader, "ghi_chu")
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
                _duLieuDangTai.TaiKhoanTheoDoiSeed.Add(new TaiKhoanTheoDoiSeed
                {
                    TheoDoiSeedId = DocInt(reader, "td_seed_id"),
                    FollowerTaiKhoanIdServer = DocIntNull(reader, "follower_tk_id_server"),
                    FolloweeTaiKhoanIdServer = DocIntNull(reader, "followee_tk_id_server"),
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
                _duLieuDangTai.TaiKhoanTimKiemSeed.Add(new TaiKhoanTimKiemSeed
                {
                    TaiKhoanTimKiemSeedId = DocInt(reader, "tk_timkiem_seed_id"),
                    TaiKhoanIdServer = DocIntNull(reader, "tk_id_server"),
                    SavedSearchIdServer = DocIntNull(reader, "saved_search_id_server"),
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
                _duLieuDangTai.TaiKhoanChanSeed.Add(new TaiKhoanChanSeed
                {
                    ChanSeedId = DocInt(reader, "chan_seed_id"),
                    BlockerTaiKhoanIdServer = DocIntNull(reader, "blocker_tk_id_server"),
                    BlockedTaiKhoanIdServer = DocIntNull(reader, "blocked_tk_id_server"),
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
                _duLieuDangTai.DiaChiTaiKhoanSeed.Add(new DiaChiTaiKhoanSeed
                {
                    DiaChiSeedId = DocInt(reader, "diachi_seed_id"),
                    TaiKhoanIdServer = DocIntNull(reader, "tk_id_server"),
                    DiaChiIdServer = DocIntNull(reader, "diachi_id_server"),
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
                _duLieuDangTai.DanhMucSeed.Add(new DanhMucSeed
                {
                    DanhMucIdServer = DocInt(reader, "dm_id_server"),
                    TenDanhMuc = DocChuoi(reader, "ten_danh_muc"),
                    DanhMucChaIdServer = DocIntNull(reader, "dm_cha_id_server"),
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
                _duLieuDangTai.ThuongHieuSeed.Add(new ThuongHieuSeed
                {
                    ThuongHieuIdServer = DocInt(reader, "thuonghieu_id_server"),
                    TenThuongHieu = DocChuoi(reader, "ten_thuong_hieu"),
                    DanhMucIdServer = DocIntNull(reader, "dm_id_server"),
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
                _duLieuDangTai.SanPhamSeed.Add(new SanPhamSeed
                {
                    ThuTuNoiBo = thuTu++,
                    SanPhamIdServer = DocInt(reader, "sp_id_server"),
                    TaiKhoanIdServer = DocIntNull(reader, "tk_id_server"),
                    DanhMucIdServer = DocIntNull(reader, "dm_id_server"),
                    ThuongHieuIdServer = DocIntNull(reader, "thuonghieu_id_server"),
                    DiaChiGuiHangIdServer = DocIntNull(reader, "ship_from_id_server"),
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
            SELECT thich_seed_id, tk_id_server, sp_id_server, thich_luc, ghi_chu
            FROM dbo.tk_thich_sanpham_seed
            ORDER BY thich_seed_id;
            """;

            await using var command = new SqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                _duLieuDangTai.TaiKhoanThichSanPhamSeed.Add(new TaiKhoanThichSanPhamSeed
                {
                    ThichSanPhamSeedId = DocInt(reader, "thich_seed_id"),
                    TaiKhoanIdServer = DocIntNull(reader, "tk_id_server"),
                    SanPhamIdServer = DocIntNull(reader, "sp_id_server"),
                    ThichLuc = DocNgayNull(reader, "thich_luc"),
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
                _duLieuDangTai.TinNhanSeed.Add(new TinNhanSeed
                {
                    TinNhanSeedId = DocInt(reader, "tn_seed_id"),
                    ConversationIdServer = DocIntNull(reader, "conversation_id_server"),
                    MessageIdServer = DocIntNull(reader, "message_id_server"),
                    SenderTaiKhoanIdServer = DocIntNull(reader, "sender_tk_id_server"),
                    ReceiverTaiKhoanIdServer = DocIntNull(reader, "receiver_tk_id_server"),
                    SanPhamIdServer = DocIntNull(reader, "product_id_server"),
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
                _duLieuDangTai.ThongBaoSeed.Add(new ThongBaoSeed
                {
                    ThongBaoSeedId = DocInt(reader, "tb_seed_id"),
                    NotificationIdServer = DocIntNull(reader, "notification_id_server"),
                    TaiKhoanIdServer = DocIntNull(reader, "tk_id_server"),
                    Title = DocChuoiNull(reader, "title"),
                    Content = DocChuoiNull(reader, "content"),
                    ObjectIdServer = DocIntNull(reader, "object_id_server"),
                    NotificationType = DocChuoiNull(reader, "notification_type"),
                    DaDoc = DocBoolNull(reader, "da_doc"),
                    TrangThai = DocChuoi(reader, "trang_thai"),
                    LayLuc = DocNgayNull(reader, "lay_luc"),
                    DocLuc = DocNgayNull(reader, "doc_luc"),
                    GhiChu = DocChuoiNull(reader, "ghi_chu")
                });
            }
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

        private static decimal? DocDecimalNull(SqlDataReader reader, string tenCot)
        {
            return reader[tenCot] == DBNull.Value ? null : Convert.ToDecimal(reader[tenCot]);
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

        private void BaoTrangThai(string noiDung)
        {
            TrangThaiThayDoi?.Invoke(noiDung);
        }

        public static ThongKeDuLieuSeed TaoThongKeSeed(BoDuLieuSeed duLieu)
        {
            return new ThongKeDuLieuSeed
            {
                SoTaiKhoanDaDangKy = DemTaiKhoanDaDangKy(duLieu),
                SoTaiKhoanDangKyCanCo = YeuCauDuLieuSeed.SoTaiKhoanDaDangKyToiThieu,
                SoTinhThanh = duLieu.TinhThanhSeed.Count,
                SoTinhThanhCanCo = YeuCauDuLieuSeed.SoTinhThanhToiThieu,
                SoPhuongXa = duLieu.PhuongXaSeed.Count,
                SoPhuongXaCanCo = YeuCauDuLieuSeed.SoPhuongXaToiThieu,
                SoSavedSearchDangLuu = duLieu.TaiKhoanTimKiemSeed.Count(x => x.TrangThai == "dang_luu"),
                SoSavedSearchCanCo = YeuCauDuLieuSeed.SoSavedSearchToiThieu,
                SoQuanHeFollowDangTheoDoi = duLieu.TaiKhoanTheoDoiSeed.Count(x => x.TrangThai == "dang_theo_doi"),
                SoQuanHeFollowToiDaCuaMotTaiKhoan = DemQuanHeNhieuNhat(duLieu.TaiKhoanTheoDoiSeed, x => x.FollowerTaiKhoanIdServer, x => x.TrangThai == "dang_theo_doi"),
                SoQuanHeFollowCanCoCuaMotTaiKhoan = YeuCauDuLieuSeed.SoQuanHeChinhToiThieu,
                SoQuanHeBlockDangChan = duLieu.TaiKhoanChanSeed.Count(x => x.TrangThai == "dang_chan"),
                SoQuanHeBlockToiDaCuaMotTaiKhoan = DemQuanHeNhieuNhat(duLieu.TaiKhoanChanSeed, x => x.BlockerTaiKhoanIdServer, x => x.TrangThai == "dang_chan"),
                SoQuanHeBlockCanCoCuaMotTaiKhoan = YeuCauDuLieuSeed.SoQuanHeChinhToiThieu,
                SoDiaChiTaiKhoanSanSang = duLieu.DiaChiTaiKhoanSeed.Count(x => x.TrangThai == "san_sang" && x.DiaChiIdServer is > 0),
                SoDiaChiTaiKhoanCanCo = YeuCauDuLieuSeed.SoDiaChiTaiKhoanMucTieu,
                SoDanhMucSanSang = duLieu.DanhMucSeed.Count(x => x.TrangThai == "san_sang"),
                SoDanhMucCanCo = YeuCauDuLieuSeed.SoDanhMucToiThieu,
                SoThuongHieuCanCo = 1,
                SoThuongHieuSanSang = duLieu.ThuongHieuSeed.Count(x => x.TrangThai == "san_sang"),
                SoSanPhamSanSang = duLieu.SanPhamSeed.Count(x => x.TrangThai == "san_sang" && x.SanPhamIdServer is > 0),
                SoSanPhamCanCo = YeuCauDuLieuSeed.SoSanPhamToiThieu,
                SoLikeSanPhamDangLuu = duLieu.TaiKhoanThichSanPhamSeed.Count(x => x.TaiKhoanIdServer is > 0 && x.SanPhamIdServer is > 0),
                SoLikeSanPhamCanCo = YeuCauDuLieuSeed.SoLikeSanPhamMucTieu,
                SoTinNhanDaGui = duLieu.TinNhanSeed.Count(x => x.TrangThai == "da_gui"),
                SoTinNhanCanCo = YeuCauDuLieuSeed.SoTinNhanMucTieu,
                SoThongBaoDangLuu = duLieu.ThongBaoSeed.Count(x => x.TrangThai == "dang_luu")
            };
        }

        public static KeHoachChuanBiDuLieuSeed TaoKeHoachChuanBiDuLieu(ThongKeDuLieuSeed thongKe)
        {
            var hangMuc = new List<HangMucChuanBiDuLieuSeed>();

            if (thongKe.SoTaiKhoanDaDangKy < thongKe.SoTaiKhoanDangKyCanCo)
            {
                hangMuc.Add(HangMucChuanBiDuLieuSeed.TaiKhoanDaDangKy);
            }

            if (thongKe.SoDanhMucSanSang < thongKe.SoDanhMucCanCo ||
                thongKe.SoThuongHieuSanSang < thongKe.SoThuongHieuCanCo)
            {
                hangMuc.Add(HangMucChuanBiDuLieuSeed.DanhMucVaThuongHieu);
            }

            if (thongKe.SoDiaChiTaiKhoanSanSang < thongKe.SoDiaChiTaiKhoanCanCo)
            {
                hangMuc.Add(HangMucChuanBiDuLieuSeed.DiaChiTaiKhoan);
            }

            if (thongKe.SoQuanHeBlockToiDaCuaMotTaiKhoan < thongKe.SoQuanHeBlockCanCoCuaMotTaiKhoan)
            {
                hangMuc.Add(HangMucChuanBiDuLieuSeed.Chan);
            }

            if (thongKe.SoQuanHeFollowToiDaCuaMotTaiKhoan < thongKe.SoQuanHeFollowCanCoCuaMotTaiKhoan)
            {
                hangMuc.Add(HangMucChuanBiDuLieuSeed.TheoDoi);
            }

            if (thongKe.SoSavedSearchDangLuu < thongKe.SoSavedSearchCanCo)
            {
                hangMuc.Add(HangMucChuanBiDuLieuSeed.TimKiem);
            }

            if (thongKe.SoSanPhamSanSang < thongKe.SoSanPhamCanCo)
            {
                hangMuc.Add(HangMucChuanBiDuLieuSeed.SanPham);
            }

            if (thongKe.SoLikeSanPhamDangLuu < thongKe.SoLikeSanPhamCanCo)
            {
                hangMuc.Add(HangMucChuanBiDuLieuSeed.LikeSanPham);
            }

            if (thongKe.SoTinNhanDaGui < thongKe.SoTinNhanCanCo)
            {
                hangMuc.Add(HangMucChuanBiDuLieuSeed.TinNhan);
            }

            if (hangMuc.Count > 0)
            {
                hangMuc.Add(HangMucChuanBiDuLieuSeed.ThongBao);
            }

            return new KeHoachChuanBiDuLieuSeed(hangMuc);
        }

        public static List<string> LietKeDuLieuConThieu(ThongKeDuLieuSeed thongKe)
        {
            var thieu = new List<string>();

            if (thongKe.SoTaiKhoanDaDangKy < thongKe.SoTaiKhoanDangKyCanCo)
            {
                thieu.Add($"Thiếu tài khoản đã đăng ký qua API signup: hiện có {thongKe.SoTaiKhoanDaDangKy}/{thongKe.SoTaiKhoanDangKyCanCo}.");
            }

            if (thongKe.SoTinhThanh < thongKe.SoTinhThanhCanCo)
            {
                thieu.Add($"Thiếu tỉnh/thành phố seed: hiện có {thongKe.SoTinhThanh}/{thongKe.SoTinhThanhCanCo}. Hãy chạy InsertProvinceWardSeed.sql.");
            }

            if (thongKe.SoPhuongXa < thongKe.SoPhuongXaCanCo)
            {
                thieu.Add($"Thiếu phường/xã seed: hiện có {thongKe.SoPhuongXa}/{thongKe.SoPhuongXaCanCo}. Hãy chạy InsertProvinceWardSeed.sql.");
            }

            if (thongKe.SoSavedSearchDangLuu < thongKe.SoSavedSearchCanCo)
            {
                thieu.Add($"Thiếu saved search dang_luu: hiện có {thongKe.SoSavedSearchDangLuu}/{thongKe.SoSavedSearchCanCo}.");
            }

            if (thongKe.SoQuanHeFollowToiDaCuaMotTaiKhoan < thongKe.SoQuanHeFollowCanCoCuaMotTaiKhoan)
            {
                thieu.Add($"Thiếu quan hệ follow dang_theo_doi: hiện có tối đa {thongKe.SoQuanHeFollowToiDaCuaMotTaiKhoan}/{thongKe.SoQuanHeFollowCanCoCuaMotTaiKhoan} cho một tài khoản.");
            }

            if (thongKe.SoQuanHeBlockToiDaCuaMotTaiKhoan < thongKe.SoQuanHeBlockCanCoCuaMotTaiKhoan)
            {
                thieu.Add($"Thiếu quan hệ block dang_chan: hiện có tối đa {thongKe.SoQuanHeBlockToiDaCuaMotTaiKhoan}/{thongKe.SoQuanHeBlockCanCoCuaMotTaiKhoan} cho một tài khoản.");
            }

            if (thongKe.SoDiaChiTaiKhoanSanSang < thongKe.SoDiaChiTaiKhoanCanCo)
            {
                thieu.Add($"Thiếu địa chỉ tài khoản san_sang: hiện có {thongKe.SoDiaChiTaiKhoanSanSang}/{thongKe.SoDiaChiTaiKhoanCanCo}.");
            }

            if (thongKe.SoDanhMucSanSang < thongKe.SoDanhMucCanCo)
            {
                thieu.Add($"Thiếu danh mục seed san_sang: hiện có {thongKe.SoDanhMucSanSang}/{thongKe.SoDanhMucCanCo}.");
            }

            if (thongKe.SoThuongHieuSanSang < thongKe.SoThuongHieuCanCo)
            {
                thieu.Add($"Thiếu thương hiệu seed san_sang: hiện có {thongKe.SoThuongHieuSanSang}/{thongKe.SoThuongHieuCanCo}.");
            }


            if (thongKe.SoSanPhamSanSang < thongKe.SoSanPhamCanCo)
            {
                thieu.Add($"Thiếu sản phẩm seed san_sang: hiện có {thongKe.SoSanPhamSanSang}/{thongKe.SoSanPhamCanCo}.");
            }

            if (thongKe.SoLikeSanPhamDangLuu < thongKe.SoLikeSanPhamCanCo)
            {
                thieu.Add($"Thiếu like sản phẩm seed đang lưu: hiện có {thongKe.SoLikeSanPhamDangLuu}/{thongKe.SoLikeSanPhamCanCo}.");
            }

            if (thongKe.SoTinNhanDaGui < thongKe.SoTinNhanCanCo)
            {
                thieu.Add($"Thiếu tin nhắn seed da_gui: hiện có {thongKe.SoTinNhanDaGui}/{thongKe.SoTinNhanCanCo}.");
            }

            return thieu;
        }

        private static int DemTaiKhoanDaDangKy(BoDuLieuSeed duLieu)
        {
            return duLieu.TaiKhoanSignupThanhCongSeed.Count(x =>
                x.TaiKhoanIdServer is > 0);
        }

        private static int DemQuanHeNhieuNhat<T>(
            IEnumerable<T> danhSach,
            Func<T, int?> layKhoa,
            Func<T, bool> dieuKien)
        {
            return danhSach
                .Where(dieuKien)
                .Select(layKhoa)
                .Where(x => x is > 0)
                .GroupBy(x => x!.Value)
                .Select(x => x.Count())
                .DefaultIfEmpty(0)
                .Max();
        }
    }

    public sealed class KetQuaKiemTraDuLieuSeed
    {
        public required BoDuLieuSeed DuLieu { get; init; }
        public required ThongKeDuLieuSeed ThongKe { get; init; }
        public required KeHoachChuanBiDuLieuSeed KeHoach { get; init; }
        public IReadOnlyList<string> DuLieuConThieu { get; init; } = [];

        public bool DaDuDuLieu => DuLieuConThieu.Count == 0;
    }

    public sealed class ThongKeDuLieuSeed
    {
        public int SoTaiKhoanDaDangKy { get; init; }
        public int SoTaiKhoanDangKyCanCo { get; init; }
        public int SoTinhThanh { get; init; }
        public int SoTinhThanhCanCo { get; init; }
        public int SoPhuongXa { get; init; }
        public int SoPhuongXaCanCo { get; init; }
        public int SoSavedSearchDangLuu { get; init; }
        public int SoSavedSearchCanCo { get; init; }
        public int SoQuanHeFollowDangTheoDoi { get; init; }
        public int SoQuanHeFollowToiDaCuaMotTaiKhoan { get; init; }
        public int SoQuanHeFollowCanCoCuaMotTaiKhoan { get; init; }
        public int SoQuanHeBlockDangChan { get; init; }
        public int SoQuanHeBlockToiDaCuaMotTaiKhoan { get; init; }
        public int SoQuanHeBlockCanCoCuaMotTaiKhoan { get; init; }
        public int SoDiaChiTaiKhoanSanSang { get; init; }
        public int SoDiaChiTaiKhoanCanCo { get; init; }
        public int SoDanhMucSanSang { get; init; }
        public int SoDanhMucCanCo { get; init; }
        public int SoThuongHieuCanCo { get; init; }
        public int SoThuongHieuSanSang { get; init; }
        public int SoSanPhamSanSang { get; init; }
        public int SoSanPhamCanCo { get; init; }
        public int SoLikeSanPhamDangLuu { get; init; }
        public int SoLikeSanPhamCanCo { get; init; }
        public int SoTinNhanDaGui { get; init; }
        public int SoTinNhanCanCo { get; init; }
        public int SoThongBaoDangLuu { get; init; }
    }
}



