using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using HactechTest.ApiShopTesting.Seed;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
    private static readonly IReadOnlySet<string> OrderSaiToken = Tap("9998", "9995", "HTTP_401", "HTTP_403");

    private static void ThemKichBanOrder(List<KichBanApi> ds)
    {
        ThemAddOrderAddress(ds);
    }

    private static void ThemAddOrderAddress(List<KichBanApi> ds)
    {
        Them(ds, "ORDER-ADDR-ADD-01", "Order", "Thêm địa chỉ nhận hàng với token không hợp lệ",
            "Gọi POST /order/add_order_address bằng token sai định dạng/hết hạn, body hợp lệ.",
            ctx =>
            {
                var duLieu = TaoDuLieuThemDiaChi(ctx, null, "ORDER-ADDR-ADD-01");
                return Req(HttpMethod.Post, "/order/add_order_address", duLieu.Body, ctx.TokenSaiDinhDang);
            },
            OrderSaiToken);

        Them(ds, "ORDER-ADDR-ADD-02", "Order", "Thêm địa chỉ nhận hàng thành công",
            "Token hợp lệ, address và address_id lấy từ Provinces_seed/Wards_seed.",
            async ctx =>
            {
                var taiKhoan = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                var duLieu = TaoDuLieuThemDiaChi(ctx, taiKhoan, "ORDER-ADDR-ADD-02");
                var req = new YeuCauApi(
                    HttpMethod.Post,
                    "/order/add_order_address",
                    duLieu.Body,
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(taiKhoan));
                req.Tam["taiKhoan"] = taiKhoan;
                req.Tam["duLieuDiaChi"] = duLieu;
                return req;
            },
            Ok,
            KiemTraResponseDiaChiCoId(),
            LuuDiaChiVaoSeedSauKhiDatAsync);

        Them(ds, "ORDER-ADDR-ADD-03", "Order", "Thêm địa chỉ với address_id không tồn tại",
            "Token hợp lệ, body hợp lệ nhưng address_id truyền ward/province không tồn tại trong database.",
            async ctx =>
            {
                var taiKhoan = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                var duLieu = TaoDuLieuThemDiaChi(ctx, taiKhoan, "ORDER-ADDR-ADD-03");
                duLieu.Body["address_id"] = new[] { 999999, 999999 };
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/order/add_order_address",
                    duLieu.Body,
                    await ctx.YeuCauTokenCuaTaiKhoanAsync(taiKhoan));
            },
            SaiGiaTri);
    }

    private static DuLieuThemDiaChi TaoDuLieuThemDiaChi(NguCanhKiemThu ctx, TaiKhoanSignupThanhCongSeed? taiKhoan, string maTestCase)
    {
        var ward = ctx.KhoSeed.DuLieu.PhuongXaSeed
            .OrderBy(x => x.PhuongXaId)
            .FirstOrDefault()
            ?? throw new BoQuaKiemThuException("Thiếu Wards_seed để tạo body /order/add_order_address.");
        var province = ctx.KhoSeed.DuLieu.TinhThanhSeed
            .FirstOrDefault(x => x.TinhThanhId == ward.TinhThanhId)
            ?? throw new BoQuaKiemThuException($"Thiếu Provinces_seed id {ward.TinhThanhId} tương ứng với ward {ward.PhuongXaId}.");

        var seedId = taiKhoan?.SoThuTu ?? 0;
        var addressDetail = $"So {Math.Max(seedId, 1)}, duong Test";
        var address = $"{addressDetail}, {ward.TenPhuongXa}, {province.TenTinhThanh}";
        var phone = taiKhoan?.SoDienThoai ?? "0909000001";
        var receiverName = $"Receiver {maTestCase}";
        var lat = 21.0m + Math.Max(seedId, 1) / 10000m;
        var lng = 105.8m + Math.Max(seedId, 1) / 10000m;
        var body = Obj(
            ("address", address),
            ("is_default", true),
            ("address_id", new[] { ward.PhuongXaId, province.TinhThanhId }),
            ("lat", lat),
            ("lng", lng),
            ("receiver_name", receiverName),
            ("phone", phone),
            ("full_address", address),
            ("address_detail", addressDetail));

        return new DuLieuThemDiaChi(ward, province, body, address, addressDetail, lat, lng, receiverName, phone);
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraResponseDiaChiCoId()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            var id = DocIdDiaChi(response.Data);
            return Task.FromResult(string.IsNullOrWhiteSpace(id)
                ? new KetQuaKiemTraThem(false, "Response thêm địa chỉ thành công nhưng thiếu id/address_id.")
                : KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static async Task LuuDiaChiVaoSeedSauKhiDatAsync(PhanHoiApi response, YeuCauApi request, NguCanhKiemThu ctx)
    {
        var diaChiIdServer = DocIdDiaChi(response.Data);
        if (string.IsNullOrWhiteSpace(diaChiIdServer))
        {
            return;
        }

        var taiKhoan = (TaiKhoanSignupThanhCongSeed)request.Tam["taiKhoan"]!;
        var duLieu = (DuLieuThemDiaChi)request.Tam["duLieuDiaChi"]!;
        var hienCo = ctx.KhoSeed.DuLieu.DiaChiTaiKhoanSeed
            .FirstOrDefault(x => x.DiaChiIdServer == diaChiIdServer);
        if (hienCo is null)
        {
            ctx.KhoSeed.DuLieu.DiaChiTaiKhoanSeed.Add(new DiaChiTaiKhoanSeed
            {
                DiaChiSeedId = ctx.KhoSeed.DuLieu.DiaChiTaiKhoanSeed.Select(x => x.DiaChiSeedId).DefaultIfEmpty(0).Max() + 1,
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
                LaMacDinh = true,
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
            hienCo.LaMacDinh = true;
            hienCo.MucDichSeed = "ca_hai";
            hienCo.TrangThai = "san_sang";
            hienCo.XacMinhLuc = DateTimeOffset.Now;
            hienCo.GhiChu = "Cập nhật bởi testcase ORDER-ADDR-ADD-02";
        }

        await ctx.KhoSeed.LuuAsync();
    }

    private static string? DocIdDiaChi(JsonNode? node)
    {
        var trucTiep = TienIchJson.DocChuoi(node, "id", "address_id", "diachi_id");
        if (!string.IsNullOrWhiteSpace(trucTiep))
        {
            return trucTiep;
        }

        if (node is not JsonObject obj)
        {
            return null;
        }

        return TienIchJson.DocChuoi(obj["address"], "id", "address_id", "diachi_id");
    }

    private sealed record DuLieuThemDiaChi(
        PhuongXaSeed PhuongXa,
        TinhThanhSeed TinhThanh,
        Dictionary<string, object?> Body,
        string DiaChi,
        string DiaChiChiTiet,
        decimal ViDo,
        decimal KinhDo,
        string TenNguoiNhan,
        string SoDienThoaiNguoiNhan);
}



