using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;

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
                var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                var duLieu = TaoDuLieuThemDiaChi(ctx, taiKhoan, "ORDER-ADDR-ADD-02");
                var req = new YeuCauApi(
                    HttpMethod.Post,
                    "/order/add_order_address",
                    duLieu.Body,
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
                req.Tam["taiKhoan"] = taiKhoan;
                req.Tam["duLieuDiaChi"] = duLieu;
                return req;
            },
            Ok,
            KiemTraResponseDiaChiCoId(),
            async (response, request, ctx) => await ctx.CapNhatDB.LuuDiaChiVaoSeedSauKhiDatAsync(response, request));

        Them(ds, "ORDER-ADDR-ADD-03", "Order", "Thêm địa chỉ với address_id không tồn tại",
            "Token hợp lệ, body hợp lệ nhưng address_id truyền ward/province không tồn tại trong database.",
            async ctx =>
            {
                var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                var duLieu = TaoDuLieuThemDiaChi(ctx, taiKhoan, "ORDER-ADDR-ADD-03");
                duLieu.Body["address_id"] = new[] { 999999, 999999 };
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/order/add_order_address",
                    duLieu.Body,
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
            },
            SaiGiaTri);
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
            return Task.FromResult(id is not > 0
                ? new KetQuaKiemTraThem(false, "Response thêm địa chỉ thành công nhưng thiếu id/address_id.")
                : KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static int? DocIdDiaChi(JsonNode? node)
    {
        return node?["id"]?.GetValue<int>();
    }
}








