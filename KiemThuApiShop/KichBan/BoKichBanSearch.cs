using System.Text.Json.Nodes;
using KiemThuApiShop.Core;
using KiemThuApiShop.Seed;

namespace KiemThuApiShop.KichBan;

public static partial class BoKichBanApi
{
    private static void ThemKichBanSearch(List<KichBanApi> ds)
    {
        ThemSaveSearch(ds);
        ThemGetListSavedSearch(ds);
    }

    private static void ThemSaveSearch(List<KichBanApi> ds)
    {
        Them(ds, "SEARCH-SAVE-01", "Search", "Lưu tìm kiếm hợp lệ",
            "Token hợp lệ, keyword có tính duy nhất.",
            async ctx =>
            {
                var tk = await ctx.YeuCauTaiKhoanDaDangKyAsync();
                var keyword = $"search_case_{tk.TkSeedId}_{DateTimeOffset.Now:yyyyMMddHHmmss}";
                var req = new YeuCauApi(HttpMethod.Post, "/api/save_search", Obj(("keyword", keyword)), await ctx.YeuCauTokenCuaTaiKhoanAsync(tk));
                req.Tam["taiKhoan"] = tk;
                req.Tam["keyword"] = keyword;
                return req;
            },
            Ok,
            DataCoTruong("keyword"),
            async (response, request, ctx) =>
            {
                var tk = (TaiKhoanSeed)request.Tam["taiKhoan"]!;
                var keyword = TienIchJson.DocChuoi(response.Data, "keyword") ?? (string)request.Tam["keyword"]!;
                ctx.KhoSeed.DuLieu.TaiKhoanTimKiemSeed.Add(new TaiKhoanTimKiemSeed
                {
                    TkTimKiemSeedId = TaoIdTimKiemMoi(ctx),
                    TkSeedId = tk.TkSeedId,
                    TkId = tk.TkId,
                    SavedSearchId = TienIchJson.DocChuoi(response.Data, "id", "saved_search_id"),
                    Keyword = keyword,
                    TrangThai = "dang_luu",
                    TaoBoiTest = true,
                    TaoLuc = DateTimeOffset.Now
                });
                await ctx.KhoSeed.LuuAsync();
            });

        Them(ds, "SEARCH-SAVE-02", "Search", "Lưu tìm kiếm không gửi token",
            "Không kèm Authorization.",
            _ => Req(HttpMethod.Post, "/api/save_search", Obj(("keyword", "search_no_token"))),
            SaiToken);

        Them(ds, "SEARCH-SAVE-03", "Search", "Lưu tìm kiếm thiếu keyword",
            "Body rỗng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/save_search", Obj(), await ctx.YeuCauTokenHopLeAsync()),
            ThieuThamSoHoacSaiGiaTri);

        Them(ds, "SEARCH-SAVE-04", "Search", "Lưu tìm kiếm keyword rỗng",
            "keyword chỉ chứa khoảng trắng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/save_search", Obj(("keyword", "   ")), await ctx.YeuCauTokenHopLeAsync()),
            SaiGiaTri);
    }

    private static void ThemGetListSavedSearch(List<KichBanApi> ds)
    {
        Them(ds, "SEARCH-LIST-01", "Search", "Lấy danh sách tìm kiếm đã lưu",
            "Dùng user có dòng tk_timkiem_seed.trang_thai = dang_luu.",
            async ctx =>
            {
                var timKiem = ctx.KhoSeed.LayTimKiemDangLuu()
                    ?? throw new BoQuaKiemThuException("Thiếu tk_timkiem_seed trạng thái dang_luu.");
                var tk = ctx.KhoSeed.LayTaiKhoanTheoSeedId(timKiem.TkSeedId)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản sở hữu search seed.");
                var req = new YeuCauApi(HttpMethod.Post, "/api/get_list_saved_search", Obj(("index", 0), ("count", 10)), await ctx.YeuCauTokenCuaTaiKhoanAsync(tk));
                req.Tam["keyword"] = timKiem.Keyword;
                return req;
            },
            Ok,
            KiemTraDanhSachCoKeyword());

        Them(ds, "SEARCH-LIST-02", "Search", "Lấy danh sách tìm kiếm không gửi token",
            "Không kèm Authorization.",
            _ => Req(HttpMethod.Post, "/api/get_list_saved_search", Obj(("index", 0), ("count", 10))),
            SaiToken);

        Them(ds, "SEARCH-LIST-03", "Search", "Lấy danh sách tìm kiếm thiếu index/count",
            "Body rỗng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_list_saved_search", Obj(), await ctx.YeuCauTokenHopLeAsync()),
            LoiThamSo);

        Them(ds, "SEARCH-LIST-04", "Search", "Lấy danh sách tìm kiếm index/count sai kiểu",
            "index = abc, count = xyz.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_list_saved_search", Obj(("index", "abc"), ("count", "xyz")), await ctx.YeuCauTokenHopLeAsync()),
            SaiKieuHoacSaiGiaTri);

        Them(ds, "SEARCH-LIST-05", "Search", "Lấy danh sách tìm kiếm count không hợp lệ",
            "count = 0.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_list_saved_search", Obj(("index", 0), ("count", 0)), await ctx.YeuCauTokenHopLeAsync()),
            SaiGiaTri);
    }

    private static int TaoIdTimKiemMoi(NguCanhKiemThu ctx)
    {
        return ctx.KhoSeed.DuLieu.TaiKhoanTimKiemSeed.Count == 0
            ? 1
            : ctx.KhoSeed.DuLieu.TaiKhoanTimKiemSeed.Max(x => x.TkTimKiemSeedId) + 1;
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraDanhSachCoKeyword()
    {
        return (response, request, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonArray mang)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data không phải mảng saved search."));
            }

            var keyword = (string)request.Tam["keyword"]!;
            var coKeyword = mang.Any(item =>
                string.Equals(TienIchJson.DocChuoi(item, "keyword"), keyword, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(coKeyword
                ? KetQuaKiemTraThem.ThanhCong
                : new KetQuaKiemTraThem(false, $"Không tìm thấy keyword `{keyword}` trong danh sách trả về."));
        };
    }
}
