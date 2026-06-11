using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;
using HactechTest.ApiShopTesting.Seed;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
    private static readonly IReadOnlySet<string> SearchTokenSaiHoacRoutePublic = Tap("9998", "9995", "HTTP_401", "HTTP_403", "1000", "9994");

    private static void ThemKichBanSearch(List<KichBanApi> ds)
    {
        ThemSearch(ds);
        ThemSaveSearch(ds);
        ThemGetListSavedSearch(ds);
    }

    private static void ThemSearch(List<KichBanApi> ds)
    {
        Them(ds, "SEARCH-API-01", "Search", "Tìm kiếm với filter hợp lệ",
            "Gọi POST /api/search với category_id lấy từ sản phẩm seed, kèm index/count hợp lệ.",
            async ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                return new YeuCauApi(HttpMethod.Post, "/api/search",
                    Obj(("category_id", IdBatBuoc(sp.DanhMucIdServer, "sanpham_seed.dm_id_server")), ("index", 0), ("count", 10)),
                    await YeuCauTokenHopLeAsync(ctx));
            },
            Ok,
            DataLaMang());

        Them(ds, "SEARCH-API-02", "Search", "Tìm kiếm không truyền filter",
            "Gọi POST /api/search chỉ truyền index/count, không truyền keyword/category/brand/price.",
            _ => Req(HttpMethod.Post, "/api/search", Obj(("index", 0), ("count", 10))),
            ThieuThamSo);

        Them(ds, "SEARCH-API-03", "Search", "Tìm kiếm với filter giá mâu thuẫn",
            "Gửi price_min > price_max kèm index/count hợp lệ.",
            _ => Req(HttpMethod.Post, "/api/search", Obj(("price_min", 10000000), ("price_max", 1000), ("index", 0), ("count", 10))),
            Tap("9994"));

        Them(ds, "SEARCH-API-04", "Search", "Tìm kiếm index/count không hợp lệ",
            "Gửi index/count sai kiểu hoặc sai giới hạn.",
            _ => Req(HttpMethod.Post, "/api/search", Obj(("keyword", "seed"), ("index", "abc"), ("count", 0))),
            SaiGiaTri);

        Them(ds, "SEARCH-API-05", "Search", "Tìm kiếm token sai ở route auth",
            "Endpoint /api/search đang trùng định nghĩa; nếu request đi qua nhánh AuthGuard thì token sai phải bị chặn.",
            ctx =>
            {
                var sp = LaySanPhamSanSang(ctx);
                return Req(HttpMethod.Post, "/api/search",
                    Obj(("category_id", IdBatBuoc(sp.DanhMucIdServer, "sanpham_seed.dm_id_server")), ("index", 0), ("count", 10)),
                    ctx.TokenSaiDinhDang);
            },
            SearchTokenSaiHoacRoutePublic,
            DataLaMang());
    }

    private static void ThemSaveSearch(List<KichBanApi> ds)
    {
        Them(ds, "SEARCH-SAVE-01", "Search", "Lưu tìm kiếm hợp lệ",
            "Token hợp lệ, keyword có tính duy nhất. Sau khi thành công phải đồng bộ saved_search_id_server vào tk_timkiem_seed.",
            async ctx =>
            {
                var tk = await YeuCauTaiKhoanDaDangKyAsync(ctx);
                var keyword = $"search_case_{tk.SoThuTu}_{DateTimeOffset.Now:yyyyMMddHHmmss}";
                var req = new YeuCauApi(HttpMethod.Post, "/api/save_search", Obj(("keyword", keyword)), await LayTokenCuaTaiKhoanAsync(ctx, tk));
                req.Tam["taiKhoan"] = tk;
                req.Tam["keyword"] = keyword;
                return req;
            },
            Ok,
            KiemTraSavedSearchTraVeHopLe(),
            async (response, request, ctx) =>
            {
                var tk = (TaiKhoanSignupThanhCongSeed)request.Tam["taiKhoan"]!;
                var keyword = response.Data?["keyword"]?.ToString() ?? (string)request.Tam["keyword"]!;
                var savedSearchIdServer = DocSavedSearchIdServer(response.Data)
                    ?? throw new LoiChuanBiKiemThuException("API /api/save_search trả 1000 nhưng response thiếu id saved search, không thể đồng bộ tk_timkiem_seed.");

                ctx.CapNhatDB.DuLieu.TaiKhoanTimKiemSeed.Add(new TaiKhoanTimKiemSeed
                {
                    TaiKhoanIdServer = tk.TaiKhoanIdServer,
                    SavedSearchIdServer = savedSearchIdServer,
                    Keyword = keyword,
                    TrangThai = "dang_luu",
                    TaoBoiTest = true,
                    TaoLuc = DateTimeOffset.Now
                });
                await ctx.CapNhatDB.LuuAsync();
            });

        Them(ds, "SEARCH-SAVE-02", "Search", "Lưu tìm kiếm không gửi token",
            "Không kèm Authorization.",
            _ => Req(HttpMethod.Post, "/api/save_search", Obj(("keyword", "search_no_token"))),
            SaiToken);

        Them(ds, "SEARCH-SAVE-03", "Search", "Lưu tìm kiếm thiếu keyword",
            "Body rỗng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/save_search", Obj(), await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "SEARCH-SAVE-04", "Search", "Lưu tìm kiếm keyword rỗng",
            "keyword chỉ chứa khoảng trắng; service normalize về rỗng và trả data = null.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/save_search", Obj(("keyword", "   ")), await YeuCauTokenHopLeAsync(ctx)),
            Ok,
            KiemTraDataNullNeuOk());
    }

    private static void ThemGetListSavedSearch(List<KichBanApi> ds)
    {
        Them(ds, "SEARCH-LIST-01", "Search", "Lấy danh sách tìm kiếm đã lưu",
            "Dùng user có dòng tk_timkiem_seed.trang_thai = dang_luu.",
            async ctx =>
            {
                var timKiem = LayTimKiemDangLuu(ctx);
                var tk = LayTaiKhoanTheoServerId(ctx, timKiem.TaiKhoanIdServer)
                    ?? throw new BoQuaKiemThuException("Thiếu tài khoản sở hữu search seed.");
                var req = new YeuCauApi(HttpMethod.Post, "/api/get_list_saved_search", Obj(("index", 0), ("count", 50)), await LayTokenCuaTaiKhoanAsync(ctx, tk));
                req.Tam["keyword"] = timKiem.Keyword;
                req.Tam["savedSearchIdServer"] = timKiem.SavedSearchIdServer;
                return req;
            },
            Ok,
            KiemTraDanhSachCoSearchDaLuu());

        Them(ds, "SEARCH-LIST-02", "Search", "Lấy danh sách tìm kiếm không gửi token",
            "Không kèm Authorization.",
            _ => Req(HttpMethod.Post, "/api/get_list_saved_search", Obj(("index", 0), ("count", 10))),
            SaiToken);

        Them(ds, "SEARCH-LIST-03", "Search", "Lấy danh sách tìm kiếm thiếu index/count",
            "Body rỗng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_list_saved_search", Obj(), await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "SEARCH-LIST-04", "Search", "Lấy danh sách tìm kiếm index/count sai kiểu",
            "Biến thể của testcase thiếu/sai index/count trong tài liệu: index = abc, count = xyz.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_list_saved_search", Obj(("index", "abc"), ("count", "xyz")), await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "SEARCH-LIST-05", "Search", "Lấy danh sách tìm kiếm count không hợp lệ",
            "Biến thể của testcase thiếu/sai index/count trong tài liệu: count = 0.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/get_list_saved_search", Obj(("index", 0), ("count", 0)), await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraSavedSearchTraVeHopLe()
    {
        return (response, request, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is null)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "Response thiếu data saved search."));
            }

            var idServer = DocSavedSearchIdServer(response.Data);
            if (idServer is not > 0)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data thiếu id/saved_search_id để lưu vào tk_timkiem_seed.saved_search_id_server."));
            }

            var keywordMongDoi = (string)request.Tam["keyword"]!;
            var keywordThucTe = response.Data?["keyword"]?.ToString();
            if (!string.Equals(keywordThucTe, keywordMongDoi, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"data.keyword phải là `{keywordMongDoi}`, thực tế là `{keywordThucTe ?? "null"}`."));
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraDataNullNeuOk()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            return Task.FromResult(response.Data is null
                ? KetQuaKiemTraThem.ThanhCong
                : new KetQuaKiemTraThem(false, "data phải là null."));
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraDanhSachCoSearchDaLuu()
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
            var savedSearchIdServer = request.Tam.TryGetValue("savedSearchIdServer", out var id) ? id as int? : null;
            var coSearchDaLuu = mang.Any(item =>
            {
                var cungKeyword = string.Equals(
                    item?["keyword"]?.ToString(),
                    keyword,
                    StringComparison.OrdinalIgnoreCase);

                if (!cungKeyword)
                {
                    return false;
                }

                if (savedSearchIdServer is not > 0)
                {
                    return true;
                }

                return DocSavedSearchIdServer(item) == savedSearchIdServer.Value;
            });

            return Task.FromResult(coSearchDaLuu
                ? KetQuaKiemTraThem.ThanhCong
                : new KetQuaKiemTraThem(false, $"Không tìm thấy saved search `{keyword}` / `{savedSearchIdServer}` trong danh sách trả về."));
        };
    }

    private static int? DocSavedSearchIdServer(JsonNode? node)
    {
        return node?["id"]?.GetValue<int>();
    }
}










