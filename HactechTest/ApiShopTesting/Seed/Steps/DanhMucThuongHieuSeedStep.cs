using HactechTest.ApiShopTesting.Core;

namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class ChuanBiSeed
{
    private async Task DongBoDanhMucVaThuongHieuAsync()
    {
        await DongBoDanhMucAsync();
        await DongBoThuongHieuAsync();
        await _nguCanh.CapNhatDB.LuuAsync();
    }

    private async Task DongBoDanhMucAsync()
    {
        var response = await GuiApiSeedAsync(
            HttpMethod.Post,
            "/api/get_categories",
            new Dictionary<string, object?>(),
            mucDich: "đồng bộ danh mục seed");

        foreach (var item in LayObjectDeQuy(response.Data))
        {
            var id = item["id"]?.GetValue<int>();
            if (id is not > 0)
            {
                continue;
            }

            var ten = item["name"]?.ToString() ?? $"Danh muc {id}";
            var hienCo = _nguCanh.CapNhatDB.DuLieu.DanhMucSeed.FirstOrDefault(x => x.DanhMucIdServer == id.Value);
            if (hienCo is null)
            {
                _nguCanh.CapNhatDB.DuLieu.DanhMucSeed.Add(new DanhMucSeed
                {
                    DanhMucIdServer = id.Value,
                    TenDanhMuc = ten,
                    DanhMucChaIdServer = item["parent_id"]?.GetValue<int>(),
                    CoDanhMucCon = item["has_child"]?.GetValue<bool>(),
                    CoThuongHieu = item["has_brand"]?.GetValue<bool>(),
                    CoKichCo = item["has_size"]?.GetValue<bool>(),
                    YeuCauCanNang = item["require_weight"]?.GetValue<bool>(),
                    TrangThai = "san_sang",
                    DongBoLuc = DateTimeOffset.Now
                });
            }
            else
            {
                hienCo.TenDanhMuc = ten;
                hienCo.DanhMucChaIdServer = item["parent_id"]?.GetValue<int>();
                hienCo.CoDanhMucCon = item["has_child"]?.GetValue<bool>();
                hienCo.CoThuongHieu = item["has_brand"]?.GetValue<bool>();
                hienCo.CoKichCo = item["has_size"]?.GetValue<bool>();
                hienCo.YeuCauCanNang = item["require_weight"]?.GetValue<bool>();
                hienCo.TrangThai = "san_sang";
                hienCo.DongBoLuc = DateTimeOffset.Now;
            }
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
            var id = item["id"]?.GetValue<int>();
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
                    DanhMucIdServer = item["category_id"]?.GetValue<int>(),
                    TrangThai = "san_sang",
                    DongBoLuc = DateTimeOffset.Now
                });
            }
            else
            {
                hienCo.TenThuongHieu = ten;
                hienCo.DanhMucIdServer = item["category_id"]?.GetValue<int>();
                hienCo.TrangThai = "san_sang";
                hienCo.DongBoLuc = DateTimeOffset.Now;
            }
        }
    }
}
