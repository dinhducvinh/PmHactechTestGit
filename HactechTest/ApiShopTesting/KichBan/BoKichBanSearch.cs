using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;
using HactechTest.ApiShopTesting.Seed;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
    private static readonly IReadOnlySet<string> SearchTokenSaiHoacRoutePublic = Tap("9998", "9995", "HTTP_401", "HTTP_403", "1000", "9994");
    private static readonly IReadOnlySet<string> SearchSaiToken = Tap("9998", "9995", "HTTP_401", "HTTP_403");

    private static void ThemKichBanSearch(List<KichBanApi> ds)
    {
        ThemSearch(ds);
        ThemSaveSearch(ds);
        ThemGetListSavedSearch(ds);
        ThemDelSavedSearch(ds);
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

                await ctx.CapNhatDB.LuuTimKiemDaLuuAsync(tk, savedSearchIdServer, keyword);
            });

        Them(ds, "SEARCH-SAVE-02", "Search", "Lưu tìm kiếm token không hợp lệ",
            "Không kèm Authorization hoặc token không hợp lệ.",
            ctx => Req(HttpMethod.Post, "/api/save_search", Obj(("keyword", "search_invalid_token")), ctx.TokenSaiDinhDang),
            SearchSaiToken);

        Them(ds, "SEARCH-SAVE-03", "Search", "Lưu tìm kiếm keyword không hợp lệ",
            "Đại diện nhóm thiếu keyword, sai kiểu hoặc keyword là chuỗi rỗng; DTO hiện trả 1004.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/save_search", Obj(), await YeuCauTokenHopLeAsync(ctx)),
            SaiGiaTri);

        Them(ds, "SEARCH-SAVE-04", "Search", "Lưu tìm kiếm keyword chỉ có khoảng trắng",
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
                var (timKiem, tk) = await DamBaoTimKiemDangLuuKemTaiKhoanAsync(ctx);
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

        Them(ds, "SEARCH-LIST-06", "Search", "Lấy danh sách tìm kiếm hết trang",
            "User có saved search nhưng index vượt quá danh sách hiện có.",
            async ctx =>
            {
                var (_, taiKhoan) = await DamBaoTimKiemDangLuuKemTaiKhoanAsync(ctx);
                return new YeuCauApi(HttpMethod.Post, "/api/get_list_saved_search",
                    Obj(("index", 999999), ("count", 10)),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
            },
            Tap("9994"));
    }

    private static void ThemDelSavedSearch(List<KichBanApi> ds)
    {
        Them(ds, "SEARCH-DEL-01", "Search", "Xóa tìm kiếm đã lưu theo id",
            "Tạo saved search tạm rồi xóa theo search_id, không làm hụt tk_timkiem_seed nền.",
            async ctx =>
            {
                var (timKiemTam, taiKhoan) = await TaoTimKiemTamKemTaiKhoanAsync(ctx, "search_del_id");
                return new YeuCauApi(HttpMethod.Post, "/api/del_saved_search",
                    Obj(("search_id", IdBatBuoc(timKiemTam.SavedSearchIdServer, "saved search tạm.saved_search_id_server"))),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
            },
            Ok,
            KiemTraDataNullNeuOk());

        Them(ds, "SEARCH-DEL-02", "Search", "Xóa toàn bộ tìm kiếm đã lưu của user",
            "Tạo saved search tạm ở user không sở hữu seed nền rồi gọi search_id = 0.",
            async ctx =>
            {
                var (_, taiKhoan) = await TaoTimKiemTamKemTaiKhoanAsync(ctx, "search_del_all", uuTienTaiKhoanKhongCoSeed: true);
                return new YeuCauApi(HttpMethod.Post, "/api/del_saved_search",
                    Obj(("search_id", 0)),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
            },
            Ok,
            KiemTraDataNullNeuOk());

        Them(ds, "SEARCH-DEL-03", "Search", "Xóa tìm kiếm đã lưu theo keyword",
            "Tạo saved search tạm rồi xóa bằng keyword exact-match.",
            async ctx =>
            {
                var (timKiemTam, taiKhoan) = await TaoTimKiemTamKemTaiKhoanAsync(ctx, "search_del_keyword");
                return new YeuCauApi(HttpMethod.Post, "/api/del_saved_search",
                    Obj(("keyword", timKiemTam.Keyword)),
                    await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan));
            },
            Ok,
            KiemTraDataNullNeuOk());

        Them(ds, "SEARCH-DEL-04", "Search", "Xóa tìm kiếm thiếu search_id và keyword",
            "Body rỗng.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/del_saved_search", Obj(), await YeuCauTokenHopLeAsync(ctx)),
            ThieuThamSo);

        Them(ds, "SEARCH-DEL-05", "Search", "Xóa tìm kiếm token sai",
            "Token sai định dạng, body có search_id hợp lệ.",
            async ctx =>
            {
                var (timKiem, _) = await DamBaoTimKiemDangLuuKemTaiKhoanAsync(ctx);
                return await Req(HttpMethod.Post, "/api/del_saved_search",
                    Obj(("search_id", IdBatBuoc(timKiem.SavedSearchIdServer, "tk_timkiem_seed.saved_search_id_server"))),
                    ctx.TokenSaiDinhDang);
            },
            Tap("9998", "9995", "HTTP_401", "HTTP_403"));

        Them(ds, "SEARCH-DEL-06", "Search", "Xóa tìm kiếm sai kiểu dữ liệu",
            "search_id = abc.",
            async ctx => new YeuCauApi(HttpMethod.Post, "/api/del_saved_search",
                Obj(("search_id", "abc")),
                await YeuCauTokenHopLeAsync(ctx)),
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
            var savedSearchIdServer = request.Tam.TryGetValue("savedSearchIdServer", out var id)
                ? DocIntTuObject(id)
                : null;
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
        return DocIdSau(node, "id") ?? DocIdSau(node, "saved_search_id");
    }

    private static async Task<(TaiKhoanTimKiemSeed TimKiem, TaiKhoanSignupThanhCongSeed TaiKhoan)> DamBaoTimKiemDangLuuKemTaiKhoanAsync(NguCanhKiemThu ctx)
    {
        var hienCo = LayTimKiemDangLuuKemTaiKhoan(ctx);
        if (hienCo is not null)
        {
            return hienCo.Value;
        }

        var taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
        var keyword = $"search_auto_{taiKhoan.SoThuTu}_{DateTimeOffset.Now:yyyyMMddHHmmssfff}";
        var response = await ctx.Api.GuiAsync(new YeuCauApi(
            HttpMethod.Post,
            "/api/save_search",
            Obj(("keyword", keyword)),
            await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan)));

        if (!LaMaThanhCong(response))
        {
            throw new BoQuaKiemThuException($"Không tạo được saved search tạm để chạy test Search. API /api/save_search trả {response.MaSoSanh}.");
        }

        var savedSearchIdServer = DocSavedSearchIdServer(response.Data);
        if (savedSearchIdServer is not > 0)
        {
            throw new BoQuaKiemThuException("API /api/save_search trả 1000 nhưng thiếu id saved search.");
        }

        var keywordDaLuu = response.Data?["keyword"]?.ToString() ?? keyword;
        await ctx.CapNhatDB.LuuTimKiemDaLuuAsync(taiKhoan, savedSearchIdServer.Value, keywordDaLuu);
        var timKiem = ctx.CapNhatDB.DuLieu.TaiKhoanTimKiemSeed.FirstOrDefault(x => x.SavedSearchIdServer == savedSearchIdServer.Value)
            ?? throw new BoQuaKiemThuException("Không đồng bộ được saved search tạm vào tk_timkiem_seed.");

        return (timKiem, taiKhoan);
    }

    private static async Task<(TaiKhoanTimKiemSeed TimKiem, TaiKhoanSignupThanhCongSeed TaiKhoan)> TaoTimKiemTamKemTaiKhoanAsync(
        NguCanhKiemThu ctx,
        string tienToKeyword,
        bool uuTienTaiKhoanKhongCoSeed = false)
    {
        var taiKhoan = uuTienTaiKhoanKhongCoSeed
            ? LayTaiKhoanKhongCoTimKiemDangLuu(ctx)
            : LayTaiKhoanDaDangKy(ctx);

        if (taiKhoan is null)
        {
            taiKhoan = await YeuCauTaiKhoanDaDangKyAsync(ctx);
        }

        var keyword = $"{tienToKeyword}_{taiKhoan.SoThuTu}_{DateTimeOffset.Now:yyyyMMddHHmmssfff}";
        var response = await ctx.Api.GuiAsync(new YeuCauApi(
            HttpMethod.Post,
            "/api/save_search",
            Obj(("keyword", keyword)),
            await LayTokenCuaTaiKhoanAsync(ctx, taiKhoan)));

        if (!LaMaThanhCong(response))
        {
            throw new BoQuaKiemThuException($"Không tạo được saved search tạm `{tienToKeyword}`. API /api/save_search trả {response.MaSoSanh}.");
        }

        var savedSearchIdServer = DocSavedSearchIdServer(response.Data);
        if (savedSearchIdServer is not > 0)
        {
            throw new BoQuaKiemThuException($"API /api/save_search trả 1000 nhưng thiếu id saved search tạm `{tienToKeyword}`.");
        }

        var keywordDaLuu = response.Data?["keyword"]?.ToString() ?? keyword;
        return (new TaiKhoanTimKiemSeed
        {
            TaiKhoanIdServer = taiKhoan.TaiKhoanIdServer,
            SavedSearchIdServer = savedSearchIdServer,
            Keyword = keywordDaLuu,
            TrangThai = "tam_thoi",
            TaoBoiTest = true,
            TaoLuc = DateTimeOffset.Now
        }, taiKhoan);
    }

    private static (TaiKhoanTimKiemSeed TimKiem, TaiKhoanSignupThanhCongSeed TaiKhoan)? LayTimKiemDangLuuKemTaiKhoan(NguCanhKiemThu ctx)
    {
        foreach (var timKiem in ctx.CapNhatDB.DuLieu.TaiKhoanTimKiemSeed
                     .Where(x => x.TrangThai == "dang_luu" && x.SavedSearchIdServer is > 0)
                     .OrderBy(x => x.TaiKhoanTimKiemSeedId))
        {
            var taiKhoan = LayTaiKhoanTheoServerId(ctx, timKiem.TaiKhoanIdServer);
            if (taiKhoan is not null)
            {
                return (timKiem, taiKhoan);
            }
        }

        return null;
    }

    private static TaiKhoanSignupThanhCongSeed? LayTaiKhoanKhongCoTimKiemDangLuu(NguCanhKiemThu ctx)
    {
        var taiKhoanCoTimKiem = ctx.CapNhatDB.DuLieu.TaiKhoanTimKiemSeed
            .Where(x => x.TrangThai == "dang_luu" && x.TaiKhoanIdServer is > 0)
            .Select(x => x.TaiKhoanIdServer!.Value)
            .ToHashSet();

        return ctx.CapNhatDB.DuLieu.TaiKhoanSignupThanhCongSeed
            .Where(x => x.TaiKhoanIdServer > 0 && !taiKhoanCoTimKiem.Contains(x.TaiKhoanIdServer))
            .OrderBy(x => x.SoThuTu)
            .ThenBy(x => x.TaiKhoanIdServer)
            .FirstOrDefault();
    }

    private static int? DocIntTuObject(object? giaTri)
    {
        return int.TryParse(giaTri?.ToString(), out var so)
            ? so
            : null;
    }
}










