using HactechTest.ApiShopTesting.Core;
using static HactechTest.ApiShopTesting.Core.HelperTC;

namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class ChuanBiSeed
{
    private async Task DongBoTinhThanhPhuongXaAsync()
    {
        var taiKhoan = LayTaiKhoanDaDangKySanSang().FirstOrDefault()
            ?? throw new LoiChuanBiKiemThuException("Cần ít nhất một tài khoản đã đăng ký để đồng bộ Provinces_seed/Wards_seed từ API địa chỉ.");
        var token = await LayTokenSeedAsync(taiKhoan, "đồng bộ tỉnh/thành phố và phường/xã seed");

        var responseTinhThanh = await GuiApiSeedAsync(
            HttpMethod.Get,
            "/order/provinces",
            token: token,
            mucDich: "đồng bộ Provinces_seed từ GET /order/provinces");

        var tinhThanh = LayObjectTuNode(responseTinhThanh.Data)
            .Select(item => new TinhThanhSeed
            {
                TinhThanhId = DocIdSau(item, "id") ?? 0,
                TenTinhThanh = DocChuoiTuNode(item["name"]) ?? ""
            })
            .Where(x => x.TinhThanhId > 0 && !string.IsNullOrWhiteSpace(x.TenTinhThanh))
            .GroupBy(x => x.TinhThanhId)
            .Select(x => x.First())
            .OrderBy(x => x.TinhThanhId)
            .ToList();

        _nguCanh.CapNhatDB.DuLieu.TinhThanhSeed.Clear();
        _nguCanh.CapNhatDB.DuLieu.TinhThanhSeed.AddRange(tinhThanh);

        var phuongXa = new List<PhuongXaSeed>();
        foreach (var tinh in tinhThanh)
        {
            var responsePhuongXa = await GuiApiSeedAsync(
                HttpMethod.Get,
                $"/order/wards?province_id={tinh.TinhThanhId}",
                token: token,
                mucDich: $"đồng bộ Wards_seed từ GET /order/wards?province_id={tinh.TinhThanhId}");

            foreach (var item in LayObjectTuNode(responsePhuongXa.Data))
            {
                var id = DocIdSau(item, "id");
                var ten = DocChuoiTuNode(item["name"]);
                var tinhThanhId = DocIdSau(item, "province_id")
                    ?? DocIdSau(item, "provinces_id")
                    ?? tinh.TinhThanhId;

                if (id is not > 0 ||
                    tinhThanhId <= 0 ||
                    string.IsNullOrWhiteSpace(ten))
                {
                    continue;
                }

                phuongXa.Add(new PhuongXaSeed
                {
                    PhuongXaId = id.Value,
                    TenPhuongXa = ten,
                    TinhThanhId = tinhThanhId
                });
            }
        }

        _nguCanh.CapNhatDB.DuLieu.PhuongXaSeed.Clear();
        _nguCanh.CapNhatDB.DuLieu.PhuongXaSeed.AddRange(
            phuongXa
                .GroupBy(x => x.PhuongXaId)
                .Select(x => x.First())
                .OrderBy(x => x.PhuongXaId));

        await _nguCanh.CapNhatDB.LuuAsync(BangDuLieuSeed.TinhThanh, BangDuLieuSeed.PhuongXa);
    }
}
