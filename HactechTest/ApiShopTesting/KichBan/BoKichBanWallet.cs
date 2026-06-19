using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
    private static readonly IReadOnlySet<string> WalletSaiToken = Tap("9998", "9995", "9999", "HTTP_401", "HTTP_403");

    private static void ThemKichBanWallet(List<KichBanApi> ds)
    {
        ThemLaySoDuVi(ds);
        ThemLayLichSuSoDuVi(ds);
    }

    private static void ThemLaySoDuVi(List<KichBanApi> ds)
    {
        Them(ds, "WALLET-BALANCE-01", "Wallet", "Lấy số dư ví bằng token sai",
            "Gọi POST /wallets/get_current_balance bằng token hết hạn/sai định dạng.",
            ctx => Req(HttpMethod.Post, "/wallets/get_current_balance", token: ctx.TokenSaiDinhDang),
            WalletSaiToken);

        Them(ds, "WALLET-BALANCE-02", "Wallet", "Lấy số dư ví hiện tại",
            "Đăng nhập tài khoản seed đã đăng ký, gọi API lấy số dư hiện tại và đồng bộ balance vào wallet_seed.",
            async ctx =>
            {
                var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                var request = new YeuCauApi(
                    HttpMethod.Post,
                    "/wallets/get_current_balance",
                    token: await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
                request.Tam["taiKhoan"] = taiKhoan;
                return request;
            },
            Ok,
            KiemTraSoDuVi(),
            async (response, request, ctx) => await ctx.CapNhatDB.DongBoSoDuViSauKhiDatAsync(response, request));
    }

    private static void ThemLayLichSuSoDuVi(List<KichBanApi> ds)
    {
        Them(ds, "WALLET-HISTORY-01", "Wallet", "Lấy lịch sử ví bằng token sai",
            "Token hết hạn/sai định dạng, body dùng index/count dạng chuỗi hợp lệ.",
            ctx => Req(
                HttpMethod.Post,
                "/wallets/get_balance_history",
                Obj(("index", "0"), ("count", "10")),
                ctx.TokenSaiDinhDang),
            WalletSaiToken);

        Them(ds, "WALLET-HISTORY-02", "Wallet", "Lấy lịch sử biến động số dư ví",
            "Token hợp lệ, index/count là numeric string theo DTO hiện tại của API Wallet.",
            async ctx =>
            {
                var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                return new YeuCauApi(
                    HttpMethod.Post,
                    "/wallets/get_balance_history",
                    Obj(("index", "0"), ("count", "10")),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
            },
            Ok,
            DataLaMang());

        Them(ds, "WALLET-HISTORY-03", "Wallet", "Lấy lịch sử ví index/count sai giá trị",
            "DTO wallet hiện convert bằng Number(), nên chuỗi âm/rỗng/không parse được thường trả 1004.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/wallets/get_balance_history",
                Obj(("index", "-1"), ("count", "0")),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "WALLET-HISTORY-04", "Wallet", "Lấy lịch sử ví thiếu index/count",
            "Body rỗng; theo đặc tả Wallet hiện tại thường trả 1004.",
            async ctx => new YeuCauApi(
                HttpMethod.Post,
                "/wallets/get_balance_history",
                Obj(),
                await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraSoDuVi()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonObject data)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của get_current_balance không phải object."));
            }

            foreach (var truong in new[] { "balance", "available_balance", "pending_balance" })
            {
                if (DocDecimalTuNode(data[truong]) is null)
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, $"data.{truong} thiếu hoặc không phải số."));
                }
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

}
