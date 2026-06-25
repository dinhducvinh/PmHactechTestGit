using HactechTest.ApiShopTesting.Core;

using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class ChuanBiSeed
{
    private async Task DongBoDanhMucVaThuongHieuAsync()
    {
        await DongBoDanhMucAsync();
        await DongBoThuongHieuAsync();
        await _nguCanh.CapNhatDB.LuuAsync(BangDuLieuSeed.DanhMuc, BangDuLieuSeed.ThuongHieu);
    }

    private async Task DongBoDanhMucAsync()
    {
        const int pageSize = 100;
        var index = 0;
        var idDaXuLy = new HashSet<int>();

        while (index < 10000)
        {
            var body = new Dictionary<string, object?> { ["index"] = index, ["count"] = pageSize };
            var response = await GuiApiSeedAsync(
                HttpMethod.Post,
                "/api/get_categories",
                body,
                mucDich: "đồng bộ danh mục seed");

            if (response.MaSoSanh == "9994")
            {
                break;
            }

            var itemsTrang = LayObjectTuNode(response.Data).ToList();
            if (itemsTrang.Count == 0)
            {
                break;
            }

            var coIdMoiTrongTrang = false;
            foreach (var item in LayObjectDeQuy(response.Data))
            {
                var id = DocIdSau(item, "id");
                if (id is not > 0)
                {
                    continue;
                }

                coIdMoiTrongTrang |= idDaXuLy.Add(id.Value);
                var ten = item["name"]?.ToString() ?? $"Danh muc {id}";
                var hienCo = _nguCanh.CapNhatDB.DuLieu.DanhMucSeed.FirstOrDefault(x => x.DanhMucIdServer == id.Value);
                if (hienCo is null)
                {
                    _nguCanh.CapNhatDB.DuLieu.DanhMucSeed.Add(new DanhMucSeed
                    {
                        DanhMucIdServer = id.Value,
                        TenDanhMuc = ten,
                        DanhMucChaIdServer = DocIdSau(item, "parent_id"),
                        CoDanhMucCon = DocBoolTuNode(item["has_child"]),
                        CoThuongHieu = DocBoolTuNode(item["has_brand"]),
                        CoKichCo = DocBoolTuNode(item["has_size"]),
                        YeuCauCanNang = DocBoolTuNode(item["require_weight"]),
                        TrangThai = "san_sang",
                        DongBoLuc = DateTimeOffset.Now
                    });
                }
                else
                {
                    hienCo.TenDanhMuc = ten;
                    hienCo.DanhMucChaIdServer = DocIdSau(item, "parent_id");
                    hienCo.CoDanhMucCon = DocBoolTuNode(item["has_child"]);
                    hienCo.CoThuongHieu = DocBoolTuNode(item["has_brand"]);
                    hienCo.CoKichCo = DocBoolTuNode(item["has_size"]);
                    hienCo.YeuCauCanNang = DocBoolTuNode(item["require_weight"]);
                    hienCo.TrangThai = "san_sang";
                    hienCo.DongBoLuc = DateTimeOffset.Now;
                }
            }

            if (!coIdMoiTrongTrang)
            {
                break;
            }

            if (itemsTrang.Count < pageSize)
            {
                break;
            }

            index += pageSize;
        }
    }

    private async Task DongBoThuongHieuAsync()
    {
        var body = new Dictionary<string, object?> { ["category_id"] = 0, ["index"] = 0, ["count"] = 1000 };
        var response = await GuiApiSeedAsync(
            HttpMethod.Post,
            "/api/get_list_brands",
            body,
            mucDich: "đồng bộ thương hiệu seed");

        foreach (var item in LayObjectTuNode(response.Data))
        {
            var id = DocIdSau(item, "id");
            if (id is not > 0)
            {
                continue;
            }

            var ten = item["brand_name"]?.ToString() ?? $"Thuong hieu {id}";
            var hienCo = _nguCanh.CapNhatDB.DuLieu.ThuongHieuSeed.FirstOrDefault(x => x.ThuongHieuIdServer == id.Value);
            if (hienCo is null)
            {
                _nguCanh.CapNhatDB.DuLieu.ThuongHieuSeed.Add(new ThuongHieuSeed
                {
                    ThuongHieuIdServer = id.Value,
                    TenThuongHieu = ten,
                    DanhMucIdServer = DocIdSau(item, "category_id"),
                    TrangThai = "san_sang",
                    DongBoLuc = DateTimeOffset.Now
                });
            }
            else
            {
                hienCo.TenThuongHieu = ten;
                hienCo.DanhMucIdServer = DocIdSau(item, "category_id");
                hienCo.TrangThai = "san_sang";
                hienCo.DongBoLuc = DateTimeOffset.Now;
            }
        }
    }
}
