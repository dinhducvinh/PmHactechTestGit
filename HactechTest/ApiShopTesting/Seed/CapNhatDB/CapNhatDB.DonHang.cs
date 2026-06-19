using HactechTest.ApiShopTesting.Core;
using Microsoft.Data.SqlClient;
using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class CapNhatDB
{
    private async Task LuuDonHangAsync(SqlConnection connection, SqlTransaction transaction)
    {
        await using (var xoaItemCommand = TaoLenh("DELETE FROM dbo.donhang_sanpham_seed;", connection, transaction))
        {
            await xoaItemCommand.ExecuteNonQueryAsync();
        }

        await using (var xoaDonHangCommand = TaoLenh("DELETE FROM dbo.donhang_seed;", connection, transaction))
        {
            await xoaDonHangCommand.ExecuteNonQueryAsync();
        }

        const string sqlDonHang = """
            INSERT INTO dbo.donhang_seed
            (donhang_id_server, buyer_tk_id_server, seller_tk_id_server, diachi_id_server,
             trang_thai, order_source, total_price, shipping_fee, final_price,
             loai_seed, tao_luc, cap_nhat_luc, ghi_chu)
            VALUES
            (@donhang_id_server, @buyer_tk_id_server, @seller_tk_id_server, @diachi_id_server,
             @trang_thai, @order_source, @total_price, @shipping_fee, @final_price,
             @loai_seed, @tao_luc, @cap_nhat_luc, @ghi_chu);
            """;

        var daGhiDonHang = new HashSet<int>();
        foreach (var item in DuLieu.DonHangSeed)
        {
            if (item.DonHangIdServer is not > 0 ||
                item.BuyerTaiKhoanIdServer is not > 0 ||
                item.SellerTaiKhoanIdServer is not > 0 ||
                !daGhiDonHang.Add(item.DonHangIdServer.Value))
            {
                continue;
            }

            await using var command = TaoLenh(sqlDonHang, connection, transaction);
            Them(command, "@donhang_id_server", item.DonHangIdServer);
            Them(command, "@buyer_tk_id_server", item.BuyerTaiKhoanIdServer);
            Them(command, "@seller_tk_id_server", item.SellerTaiKhoanIdServer);
            Them(command, "@diachi_id_server", item.DiaChiIdServer);
            Them(command, "@trang_thai", item.TrangThai);
            Them(command, "@order_source", item.OrderSource);
            Them(command, "@total_price", item.TotalPrice);
            Them(command, "@shipping_fee", item.ShippingFee);
            Them(command, "@final_price", item.FinalPrice);
            Them(command, "@loai_seed", item.LoaiSeed);
            Them(command, "@tao_luc", item.TaoLuc ?? DateTimeOffset.Now);
            Them(command, "@cap_nhat_luc", item.CapNhatLuc ?? item.TaoLuc ?? DateTimeOffset.Now);
            Them(command, "@ghi_chu", item.GhiChu);
            await command.ExecuteNonQueryAsync();
        }

        const string sqlSanPham = """
            INSERT INTO dbo.donhang_sanpham_seed
            (donhang_id_server, sp_id_server, so_luong, don_gia, thanh_tien)
            VALUES
            (@donhang_id_server, @sp_id_server, @so_luong, @don_gia, @thanh_tien);
            """;

        var donHangDaLuu = daGhiDonHang;
        var daGhiSanPham = new HashSet<(int DonHangIdServer, int SanPhamIdServer)>();
        foreach (var item in DuLieu.DonHangSanPhamSeed)
        {
            if (item.DonHangIdServer is not > 0 ||
                item.SanPhamIdServer is not > 0 ||
                item.SoLuong <= 0 ||
                !donHangDaLuu.Contains(item.DonHangIdServer.Value) ||
                !daGhiSanPham.Add((item.DonHangIdServer.Value, item.SanPhamIdServer.Value)))
            {
                continue;
            }

            await using var command = TaoLenh(sqlSanPham, connection, transaction);
            Them(command, "@donhang_id_server", item.DonHangIdServer);
            Them(command, "@sp_id_server", item.SanPhamIdServer);
            Them(command, "@so_luong", item.SoLuong);
            Them(command, "@don_gia", item.DonGia);
            Them(command, "@thanh_tien", item.ThanhTien);
            await command.ExecuteNonQueryAsync();
        }
    }

    internal async Task<DonHangSeed> LuuDonHangSauCreateOrderAsync(PhanHoiApi response, YeuCauApi request)
    {
        if (!LaMaThanhCong(response))
        {
            throw new LoiChuanBiKiemThuException("Không thể lưu donhang_seed vì API create_order không thành công.");
        }

        var buyer = (TaiKhoanSignupThanhCongSeed)request.Tam["buyer"]!;
        var seller = (TaiKhoanSignupThanhCongSeed)request.Tam["seller"]!;
        var diaChi = (DiaChiTaiKhoanSeed)request.Tam["diaChi"]!;
        IReadOnlyList<DonHangSanPhamSeed> itemsSeed =
            request.Tam.TryGetValue("itemsSeed", out var itemsTam) &&
            itemsTam is IReadOnlyList<DonHangSanPhamSeed> danhSach
                ? danhSach
                : Array.Empty<DonHangSanPhamSeed>();
        var orderSource = request.Tam.TryGetValue("orderSource", out var sourceTam) && sourceTam is int source
            ? source
            : 1;

        var orderId =
            DocIntTuNode(response.Data?["order_id"]) ??
            DocIntTuNode(response.Data?["id"]);
        if (orderId is not > 0)
        {
            throw new LoiChuanBiKiemThuException(
                "API /order/create_order đã thành công nhưng response thiếu order_id/id nên không thể lưu donhang_seed.");
        }

        var lucCapNhat = DateTimeOffset.Now;
        var donHang = DuLieu.DonHangSeed.FirstOrDefault(x => x.DonHangIdServer == orderId);
        if (donHang is null)
        {
            donHang = new DonHangSeed
            {
                DonHangIdServer = orderId,
                TaoLuc = lucCapNhat
            };
            DuLieu.DonHangSeed.Add(donHang);
        }

        donHang.BuyerTaiKhoanIdServer = buyer.TaiKhoanIdServer;
        donHang.SellerTaiKhoanIdServer = seller.TaiKhoanIdServer;
        donHang.DiaChiIdServer = DocIntTuNode(response.Data?["address_id"]) ?? diaChi.DiaChiIdServer;
        donHang.TrangThai = DocChuoiTuNode(response.Data?["state"]) ?? DocChuoiTuNode(response.Data?["status"]) ?? "pending";
        donHang.OrderSource = orderSource;
        donHang.TotalPrice = DocDecimalTuNode(response.Data?["total_price"]) ?? itemsSeed.Sum(x => x.ThanhTien ?? 0m);
        donHang.ShippingFee = DocDecimalTuNode(response.Data?["shipping_fee"]) ?? DocDecimalTuNode(response.Data?["ship_fee"]);
        donHang.FinalPrice = DocDecimalTuNode(response.Data?["final_price"]) ??
                             ((donHang.TotalPrice ?? 0m) + (donHang.ShippingFee ?? 0m));
        donHang.LoaiSeed ??= "create_order";
        donHang.CapNhatLuc = lucCapNhat;
        donHang.GhiChu = "Tạo bằng API /order/create_order";

        DuLieu.DonHangSanPhamSeed.RemoveAll(x => x.DonHangIdServer == orderId);
        foreach (var item in itemsSeed)
        {
            if (item.SanPhamIdServer is not > 0 || item.SoLuong <= 0)
            {
                continue;
            }

            DuLieu.DonHangSanPhamSeed.Add(new DonHangSanPhamSeed
            {
                DonHangIdServer = orderId,
                SanPhamIdServer = item.SanPhamIdServer,
                SoLuong = item.SoLuong,
                DonGia = item.DonGia,
                ThanhTien = item.ThanhTien
            });
        }

        await LuuAsync(BangDuLieuSeed.DonHang);
        return donHang;
    }

    internal async Task CapNhatGhiChuDonHangAsync(DonHangSeed donHang, string ghiChu)
    {
        donHang.CapNhatLuc = DateTimeOffset.Now;
        donHang.GhiChu = ghiChu;
        await LuuAsync(BangDuLieuSeed.DonHang);
    }

}
