using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using Microsoft.Data.SqlClient;
using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class CapNhatDB
{
    internal async Task<TaiKhoanSignupThanhCongSeed> LuuTaiKhoanDangKyThanhCongAsync(
        PhanHoiApi response,
        TaiKhoanChuaDangKySeed taiKhoan,
        string matKhauDungDeDangKy)
    {
        var taiKhoanDaDangKy = new TaiKhoanSignupThanhCongSeed
        {
            TaiKhoanIdServer = DocIdSau(response.Data, "id") ?? 0,
            SoDienThoai = taiKhoan.SoDienThoai,
            MatKhauHienTai = matKhauDungDeDangKy,
            DangKyLuc = DateTimeOffset.Now,
            GhiChu = taiKhoan.GhiChu,
            SoThuTu = taiKhoan.SoThuTu,
            UuidThietBi = taiKhoan.UuidThietBi
        };

        if (taiKhoanDaDangKy.TaiKhoanIdServer <= 0)
        {
            throw new LoiChuanBiKiemThuException("API /auth/signup trả thành công nhưng không có id tài khoản.");
        }

        var walletId = response.Data?["wallet_id"]?.ToString();
        if (!string.IsNullOrWhiteSpace(walletId))
        {
            UpsertWalletSauSignup(taiKhoanDaDangKy, walletId);
        }

        DuLieu.TaiKhoanChuaDangKySeed.Remove(taiKhoan);
        DuLieu.TaiKhoanSignupThanhCongSeed.Add(taiKhoanDaDangKy);
        if (string.IsNullOrWhiteSpace(walletId))
        {
            await LuuAsync(BangDuLieuSeed.TaiKhoan);
        }
        else
        {
            await LuuAsync(BangDuLieuSeed.TaiKhoan, BangDuLieuSeed.Wallet);
        }

        return taiKhoanDaDangKy;
    }

    internal async Task CapNhatMatKhauAsync(TaiKhoanSignupThanhCongSeed taiKhoan, string matKhauMoi)
    {
        taiKhoan.MatKhauHienTai = matKhauMoi;
        taiKhoan.DoiMatKhauLuc = DateTimeOffset.Now;
        await LuuAsync(BangDuLieuSeed.TaiKhoan);
    }

    internal async Task LuuTimKiemDaLuuAsync(
        TaiKhoanSignupThanhCongSeed taiKhoan,
        int savedSearchIdServer,
        string keyword)
    {
        DuLieu.TaiKhoanTimKiemSeed.RemoveAll(x =>
            x.SavedSearchIdServer == savedSearchIdServer ||
            (x.TaiKhoanIdServer == taiKhoan.TaiKhoanIdServer &&
             string.Equals(x.Keyword, keyword, StringComparison.OrdinalIgnoreCase)));

        DuLieu.TaiKhoanTimKiemSeed.Add(new TaiKhoanTimKiemSeed
        {
            TaiKhoanIdServer = taiKhoan.TaiKhoanIdServer,
            SavedSearchIdServer = savedSearchIdServer,
            Keyword = keyword,
            TrangThai = "dang_luu",
            TaoBoiTest = true,
            TaoLuc = DateTimeOffset.Now
        });
        await LuuAsync(BangDuLieuSeed.TimKiem);
    }

    internal async Task LuuDiaChiVaoSeedSauKhiDatAsync(PhanHoiApi response, YeuCauApi request)
    {
        var diaChiIdServer = DocIdSau(response.Data, "id");
        if (diaChiIdServer is not > 0)
        {
            return;
        }

        var taiKhoan = (TaiKhoanSignupThanhCongSeed)request.Tam["taiKhoan"]!;
        var duLieu = (HelperTC.DuLieuThemDiaChi)request.Tam["duLieuDiaChi"]!;
        if (duLieu.LaMacDinh)
        {
            foreach (var diaChiCu in DuLieu.DiaChiTaiKhoanSeed.Where(x =>
                         x.TaiKhoanIdServer == taiKhoan.TaiKhoanIdServer &&
                         x.DiaChiIdServer != diaChiIdServer))
            {
                diaChiCu.LaMacDinh = false;
            }
        }

        var hienCo = DuLieu.DiaChiTaiKhoanSeed.FirstOrDefault(x => x.DiaChiIdServer == diaChiIdServer);
        if (hienCo is null)
        {
            DuLieu.DiaChiTaiKhoanSeed.Add(new DiaChiTaiKhoanSeed
            {
                TaiKhoanIdServer = taiKhoan.TaiKhoanIdServer,
                DiaChiIdServer = diaChiIdServer,
                PhuongXaId = duLieu.PhuongXa.PhuongXaId,
                TinhThanhId = duLieu.TinhThanh.TinhThanhId,
                DiaChi = duLieu.DiaChi,
                DiaChiDayDu = duLieu.DiaChi,
                DiaChiChiTiet = duLieu.DiaChiChiTiet,
                ViDo = duLieu.ViDo,
                KinhDo = duLieu.KinhDo,
                TenNguoiNhan = duLieu.TenNguoiNhan,
                SoDienThoaiNguoiNhan = duLieu.SoDienThoaiNguoiNhan,
                LaMacDinh = duLieu.LaMacDinh,
                MucDichSeed = "ca_hai",
                TrangThai = "san_sang",
                TaoLuc = DateTimeOffset.Now,
                XacMinhLuc = DateTimeOffset.Now,
                GhiChu = "Tạo bởi testcase ORDER-ADDR-ADD-02"
            });
        }
        else
        {
            hienCo.TaiKhoanIdServer = taiKhoan.TaiKhoanIdServer;
            hienCo.PhuongXaId = duLieu.PhuongXa.PhuongXaId;
            hienCo.TinhThanhId = duLieu.TinhThanh.TinhThanhId;
            hienCo.DiaChi = duLieu.DiaChi;
            hienCo.DiaChiDayDu = duLieu.DiaChi;
            hienCo.DiaChiChiTiet = duLieu.DiaChiChiTiet;
            hienCo.ViDo = duLieu.ViDo;
            hienCo.KinhDo = duLieu.KinhDo;
            hienCo.TenNguoiNhan = duLieu.TenNguoiNhan;
            hienCo.SoDienThoaiNguoiNhan = duLieu.SoDienThoaiNguoiNhan;
            hienCo.LaMacDinh = duLieu.LaMacDinh;
            hienCo.MucDichSeed = "ca_hai";
            hienCo.TrangThai = "san_sang";
            hienCo.XacMinhLuc = DateTimeOffset.Now;
            hienCo.GhiChu = "Cập nhật bởi testcase ORDER-ADDR-ADD-02";
        }

        await LuuAsync(BangDuLieuSeed.DiaChiTaiKhoan);
    }

    internal async Task XoaDiaChiTaiKhoanAsync(DiaChiTaiKhoanSeed diaChi)
    {
        DuLieu.DiaChiTaiKhoanSeed.RemoveAll(x =>
            x == diaChi ||
            (diaChi.DiaChiIdServer is > 0 && x.DiaChiIdServer == diaChi.DiaChiIdServer) ||
            (diaChi.DiaChiSeedId > 0 && x.DiaChiSeedId == diaChi.DiaChiSeedId));

        await using var connection = new SqlConnection(_chuoiKetNoi);
        await connection.OpenAsync();

        const string sql = """
            DELETE FROM dbo.diachi_tk_seed
            WHERE (@diachi_id_server IS NOT NULL AND diachi_id_server = @diachi_id_server)
               OR (@diachi_seed_id > 0 AND diachi_seed_id = @diachi_seed_id);
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@diachi_id_server", diaChi.DiaChiIdServer is > 0 ? diaChi.DiaChiIdServer.Value : (object)DBNull.Value);
        command.Parameters.AddWithValue("@diachi_seed_id", diaChi.DiaChiSeedId);
        await command.ExecuteNonQueryAsync();
    }

    internal async Task DongBoSoDuViSauKhiDatAsync(PhanHoiApi response, YeuCauApi request)
    {
        if (!LaMaThanhCong(response) ||
            response.Data is not JsonObject data ||
            request.Tam["taiKhoan"] is not TaiKhoanSignupThanhCongSeed taiKhoan)
        {
            return;
        }

        var wallet = DuLieu.WalletSeed.FirstOrDefault(x =>
            x.TaiKhoanIdServer == taiKhoan.TaiKhoanIdServer);
        if (wallet is null)
        {
            return;
        }

        wallet.Balance = DocDecimalTuNode(data["balance"]) ?? wallet.Balance;
        wallet.AvailableBalance = DocDecimalTuNode(data["available_balance"]);
        wallet.PendingBalance = DocDecimalTuNode(data["pending_balance"]);
        wallet.XacMinhLuc = DateTimeOffset.Now;
        wallet.TrangThai ??= "san_sang";
        await LuuAsync(BangDuLieuSeed.Wallet);
    }

    private void UpsertWalletSauSignup(TaiKhoanSignupThanhCongSeed taiKhoan, string walletId)
    {
        var wallet = DuLieu.WalletSeed.FirstOrDefault(x =>
            string.Equals(x.WalletIdServer, walletId, StringComparison.OrdinalIgnoreCase) ||
            x.TaiKhoanIdServer == taiKhoan.TaiKhoanIdServer);

        if (wallet is null)
        {
            DuLieu.WalletSeed.Add(new WalletSeed
            {
                WalletIdServer = walletId,
                TaiKhoanIdServer = taiKhoan.TaiKhoanIdServer,
                Balance = YeuCauDuLieuSeed.SoDuViMacDinhSauSignup,
                TrangThai = "san_sang",
                TaoLuc = DateTimeOffset.Now,
                GhiChu = "Tạo sau signup seed."
            });
            return;
        }

        wallet.WalletIdServer = walletId;
        wallet.TaiKhoanIdServer = taiKhoan.TaiKhoanIdServer;
        wallet.Balance = wallet.Balance <= 0
            ? YeuCauDuLieuSeed.SoDuViMacDinhSauSignup
            : wallet.Balance;
        wallet.TrangThai ??= "san_sang";
        wallet.TaoLuc ??= DateTimeOffset.Now;
    }

}
