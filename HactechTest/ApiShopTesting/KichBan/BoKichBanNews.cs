using System.Text.Json.Nodes;
using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.KichBan;

public static partial class BoKichBanApi
{
    private static void ThemKichBanNews(List<KichBanApi> ds)
    {
        Them(ds, "NEWS-DETAIL-01", "News", "Lấy chi tiết news hợp lệ",
            "Gọi POST /News/list_news để lấy news id hợp lệ, sau đó gọi GET /News/:id.",
            async ctx =>
            {
                var newsId = await LayNewsIdHopLeAsync(ctx);
                return new YeuCauApi(HttpMethod.Get, $"/News/{newsId}");
            },
            Ok,
            KiemTraChiTietNews());

        Them(ds, "NEWS-DETAIL-02", "News", "Lấy chi tiết news không tồn tại",
            "Gọi GET /News/999999999.",
            _ => Req(HttpMethod.Get, "/News/999999999"),
            SaiGiaTri);

        Them(ds, "NEWS-DETAIL-03", "News", "Lấy chi tiết news id sai kiểu",
            "Gọi GET /News/abc.",
            _ => Req(HttpMethod.Get, "/News/abc"),
            SaiKieu);

        Them(ds, "NEWS-LIST-01", "News", "Lấy toàn bộ danh sách news",
            "Gọi POST /News/list_news với body rỗng.",
            _ => Req(HttpMethod.Post, "/News/list_news", Obj()),
            Ok,
            KiemTraDanhSachNewsKhongPhanTrang());

        Them(ds, "NEWS-LIST-02", "News", "Lấy danh sách news có phân trang",
            "Gọi POST /News/list_news với index = 0, count = 10.",
            _ => Req(HttpMethod.Post, "/News/list_news", Obj(("index", 0), ("count", 10))),
            Ok,
            KiemTraDanhSachNewsCoPhanTrang(10));

        Them(ds, "NEWS-LIST-03", "News", "Lấy news thiếu count",
            "Chỉ truyền index, thiếu count.",
            _ => Req(HttpMethod.Post, "/News/list_news", Obj(("index", 0))),
            ThieuThamSo);

        Them(ds, "NEWS-LIST-04", "News", "Lấy news thiếu index",
            "Chỉ truyền count, thiếu index.",
            _ => Req(HttpMethod.Post, "/News/list_news", Obj(("count", 10))),
            ThieuThamSo);

        Them(ds, "NEWS-LIST-05", "News", "Lấy news index/count sai kiểu",
            "Truyền index = abc, count = xyz.",
            _ => Req(HttpMethod.Post, "/News/list_news", Obj(("index", "abc"), ("count", "xyz"))),
            SaiKieuHoacSaiGiaTri);

        Them(ds, "NEWS-LIST-06", "News", "Lấy news index âm",
            "Truyền index = -1, count = 10.",
            _ => Req(HttpMethod.Post, "/News/list_news", Obj(("index", -1), ("count", 10))),
            SaiGiaTri);

        Them(ds, "NEWS-LIST-07", "News", "Lấy news count âm",
            "Truyền index = 0, count = -1.",
            _ => Req(HttpMethod.Post, "/News/list_news", Obj(("index", 0), ("count", -1))),
            SaiGiaTri);
    }

    private static async Task<int> LayNewsIdHopLeAsync(NguCanhKiemThu ctx)
    {
        var response = await ctx.Api.GuiAsync(new YeuCauApi(HttpMethod.Post, "/News/list_news", Obj()));
        if (!LaMaThanhCong(response))
        {
            throw new BoQuaKiemThuException($"Không lấy được danh sách news để chuẩn bị NEWS-DETAIL-01. /News/list_news trả {response.MaSoSanh}.");
        }

        var danhSach = LayMangNews(response.Data);
        var newsId = danhSach?
            .OfType<JsonObject>()
            .Select(item => DocIntTuNode(item["id"]))
            .FirstOrDefault(id => id is > 0);

        if (newsId is > 0)
        {
            return newsId.Value;
        }

        throw new BoQuaKiemThuException("Không có news id hợp lệ trong response /News/list_news để chạy NEWS-DETAIL-01.");
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraChiTietNews()
    {
        return (response, request, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonObject data)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của news detail không phải object."));
            }

            var expectedId = DocNewsIdTuDuongDan(request.DuongDan);
            var actualId = DocIntTuNode(data["id"]);
            if (expectedId is > 0 && actualId != expectedId)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"data.id phải bằng {expectedId}, thực tế là {actualId?.ToString() ?? "null"}."));
            }

            return Task.FromResult(KiemTraNewsCoTruongBatBuoc(data));
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraDanhSachNewsKhongPhanTrang()
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonArray danhSach)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của list_news không phân trang phải là mảng News[]."));
            }

            return Task.FromResult(KiemTraMangNews(danhSach));
        };
    }

    private static Func<PhanHoiApi, YeuCauApi, NguCanhKiemThu, Task<KetQuaKiemTraThem>> KiemTraDanhSachNewsCoPhanTrang(int count)
    {
        return (response, _, _) =>
        {
            if (!LaMaThanhCong(response))
            {
                return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
            }

            if (response.Data is not JsonObject data)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data của list_news phân trang phải là object."));
            }

            if (data["list_news"] is not JsonArray danhSach)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data thiếu mảng list_news."));
            }

            if (DocIntTuNode(data["total"]) is null)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, "data.total thiếu hoặc không phải số."));
            }

            if (danhSach.Count > count)
            {
                return Task.FromResult(new KetQuaKiemTraThem(false, $"list_news có {danhSach.Count} phần tử, vượt quá count = {count}."));
            }

            var kiemTraMang = KiemTraMangNews(danhSach);
            if (!kiemTraMang.Dat)
            {
                return Task.FromResult(kiemTraMang);
            }

            return Task.FromResult(KiemTraSapXepNewsIdGiamDan(danhSach));
        };
    }

    private static JsonArray? LayMangNews(JsonNode? data)
    {
        return data switch
        {
            JsonArray mang => mang,
            JsonObject obj when obj["list_news"] is JsonArray mang => mang,
            _ => null
        };
    }

    private static KetQuaKiemTraThem KiemTraMangNews(JsonArray danhSach)
    {
        foreach (var node in danhSach)
        {
            if (node is not JsonObject item)
            {
                return new KetQuaKiemTraThem(false, "Một phần tử news không phải object.");
            }

            var kiemTraItem = KiemTraNewsCoTruongBatBuoc(item);
            if (!kiemTraItem.Dat)
            {
                return kiemTraItem;
            }
        }

        return KetQuaKiemTraThem.ThanhCong;
    }

    private static KetQuaKiemTraThem KiemTraNewsCoTruongBatBuoc(JsonObject item)
    {
        foreach (var truong in new[] { "id", "title", "created_at", "content" })
        {
            if (!item.ContainsKey(truong))
            {
                return new KetQuaKiemTraThem(false, $"news thiếu trường `{truong}`.");
            }
        }

        if (DocIntTuNode(item["id"]) is null)
        {
            return new KetQuaKiemTraThem(false, "news.id thiếu hoặc không phải số.");
        }

        if (DocIntTuNode(item["created_at"]) is null)
        {
            return new KetQuaKiemTraThem(false, "news.created_at thiếu hoặc không phải số.");
        }

        return KetQuaKiemTraThem.ThanhCong;
    }

    private static KetQuaKiemTraThem KiemTraSapXepNewsIdGiamDan(JsonArray danhSach)
    {
        int? idTruoc = null;
        foreach (var node in danhSach.OfType<JsonObject>())
        {
            var id = DocIntTuNode(node["id"]);
            if (id is null)
            {
                continue;
            }

            if (idTruoc is not null && id > idTruoc)
            {
                return new KetQuaKiemTraThem(false, $"list_news không sắp xếp id DESC: id {id} đứng sau {idTruoc}.");
            }

            idTruoc = id;
        }

        return KetQuaKiemTraThem.ThanhCong;
    }

    private static int? DocNewsIdTuDuongDan(string duongDan)
    {
        var phanCuoi = duongDan.TrimEnd('/').Split('/').LastOrDefault();
        return int.TryParse(phanCuoi, out var id) ? id : null;
    }
}
