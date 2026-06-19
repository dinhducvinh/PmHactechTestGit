namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class CapNhatDB
{
    internal async Task ThemLikeSanPhamAsync(TaiKhoanSignupThanhCongSeed taiKhoan, SanPhamSeed sanPham)
    {
        DuLieu.TaiKhoanThichSanPhamSeed.Add(new TaiKhoanThichSanPhamSeed
        {
            TaiKhoanIdServer = taiKhoan.TaiKhoanIdServer,
            SanPhamIdServer = sanPham.SanPhamIdServer,
            ThichLuc = DateTimeOffset.Now,
            GhiChu = "Tạo bởi testcase PRODUCT-LIKE-01"
        });
        await LuuAsync(BangDuLieuSeed.ThichSanPham);
    }

    internal async Task XoaLikeSanPhamAsync(TaiKhoanThichSanPhamSeed like)
    {
        DuLieu.TaiKhoanThichSanPhamSeed.Remove(like);
        await LuuAsync(BangDuLieuSeed.ThichSanPham);
    }

    internal async Task ThemReportSanPhamAsync(TaiKhoanSignupThanhCongSeed taiKhoan, SanPhamSeed sanPham)
    {
        var daCo = DuLieu.ReportSanPhamSeed.Any(x =>
            x.TaiKhoanIdServer == taiKhoan.TaiKhoanIdServer &&
            x.SanPhamIdServer == sanPham.SanPhamIdServer);
        if (!daCo)
        {
            DuLieu.ReportSanPhamSeed.Add(new ReportSanPhamSeed
            {
                TaiKhoanIdServer = taiKhoan.TaiKhoanIdServer,
                SanPhamIdServer = sanPham.SanPhamIdServer
            });
        }

        await LuuAsync(BangDuLieuSeed.ReportSanPham);
    }

    internal async Task ThemSanPhamSauAddProductAsync(
        TaiKhoanSignupThanhCongSeed seller,
        int sanPhamIdServer,
        int? danhMucIdServer,
        int? thuongHieuIdServer,
        int? diaChiGuiHangIdServer,
        string tenSanPham,
        decimal gia)
    {
        DuLieu.SanPhamSeed.Add(new SanPhamSeed
        {
            SanPhamIdServer = sanPhamIdServer,
            TaiKhoanIdServer = seller.TaiKhoanIdServer,
            DanhMucIdServer = danhMucIdServer,
            ThuongHieuIdServer = thuongHieuIdServer,
            DiaChiGuiHangIdServer = diaChiGuiHangIdServer,
            TenSanPham = tenSanPham,
            Gia = gia,
            TrangThai = "san_sang",
            TaoBoiTest = true,
            TaoLuc = DateTimeOffset.Now,
            XacMinhLuc = DateTimeOffset.Now,
            GhiChu = "Tạo bởi testcase PRODUCT-ADD-01"
        });
        await LuuAsync(BangDuLieuSeed.SanPham);
    }

    internal async Task CapNhatSanPhamSauUpdateAsync(SanPhamSeed sanPham, string tenMoi, decimal giaMoi)
    {
        sanPham.TenSanPham = tenMoi;
        sanPham.Gia = giaMoi;
        sanPham.XacMinhLuc = DateTimeOffset.Now;
        sanPham.GhiChu = "Cập nhật bởi testcase PRODUCT-UPDATE-01";
        await LuuAsync(BangDuLieuSeed.SanPham);
    }

    internal async Task DanhDauSanPhamDaXoaAsync(SanPhamSeed sanPham)
    {
        sanPham.TrangThai = "da_xoa";
        sanPham.XacMinhLuc = DateTimeOffset.Now;
        sanPham.GhiChu = "Xoa boi testcase PRODUCT-DELETE-01";
        await LuuAsync(BangDuLieuSeed.SanPham);
    }
}
