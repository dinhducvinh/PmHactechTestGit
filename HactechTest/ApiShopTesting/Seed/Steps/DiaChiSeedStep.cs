using HactechTest.ApiShopTesting.Core;

namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class ChuanBiSeed
{
    private async Task TaoDiaChiTaiKhoanSeedAsync()
    {
        var taiKhoanDaDangKy = LayTaiKhoanDaDangKySanSang();
        var diaChiSanSang = _nguCanh.CapNhatDB.DuLieu.DiaChiTaiKhoanSeed
            .Where(x => x.TrangThai == "san_sang" && x.DiaChiIdServer is > 0)
            .Select(x => x.TaiKhoanIdServer)
            .Where(x => x is > 0)
            .Select(x => x!.Value)
            .ToHashSet();
        var wards = _nguCanh.CapNhatDB.DuLieu.PhuongXaSeed.OrderBy(x => x.PhuongXaId).ToList();

        if (taiKhoanDaDangKy.Count == 0 || wards.Count == 0)
        {
            return;
        }

        for (var i = 0; i < taiKhoanDaDangKy.Count && diaChiSanSang.Count < YeuCauDuLieuSeed.SoDiaChiTaiKhoanMucTieu; i++)
        {
            var taiKhoan = taiKhoanDaDangKy[i];
            if (taiKhoan.TaiKhoanIdServer is not > 0 || diaChiSanSang.Contains(taiKhoan.TaiKhoanIdServer))
            {
                continue;
            }

            var ward = wards[i % wards.Count];
            var province = _nguCanh.CapNhatDB.DuLieu.TinhThanhSeed.FirstOrDefault(x => x.TinhThanhId == ward.TinhThanhId);
            if (province is null)
            {
                continue;
            }

            var token = await LayTokenSeedAsync(taiKhoan, $"tạo địa chỉ seed cho tài khoản {taiKhoan.SoThuTu}");

            var diaChiChiTiet = $"So {taiKhoan.SoThuTu}, duong Seed";
            var diaChi = $"{diaChiChiTiet}, {ward.TenPhuongXa}, {province.TenTinhThanh}";
            var body = new Dictionary<string, object?>
            {
                ["address"] = diaChi,
                ["is_default"] = true,
                ["address_id"] = new[] { ward.PhuongXaId, province.TinhThanhId },
                ["lat"] = 21.0m + taiKhoan.SoThuTu / 10000m,
                ["lng"] = 105.8m + taiKhoan.SoThuTu / 10000m,
                ["receiver_name"] = $"Seed User {taiKhoan.SoThuTu}",
                ["phone"] = taiKhoan.SoDienThoai,
                ["full_address"] = diaChi,
                ["address_detail"] = diaChiChiTiet
            };

            var response = await GuiApiSeedAsync(
                HttpMethod.Post,
                "/order/add_order_address",
                body,
                token,
                $"tạo địa chỉ seed cho tài khoản {taiKhoan.SoThuTu}");

            var diaChiIdServer = DocIdSau(response.Data, "id");
            if (diaChiIdServer is not > 0)
            {
                throw new LoiChuanBiKiemThuException(
                    $"API /order/add_order_address trả thành công nhưng không có id địa chỉ cho tài khoản seed {taiKhoan.SoThuTu}. Response: {RutGon(response.NoiDungRaw)}");
            }

            _nguCanh.CapNhatDB.DuLieu.DiaChiTaiKhoanSeed.Add(new DiaChiTaiKhoanSeed
            {
                DiaChiSeedId = IdTiepTheo(_nguCanh.CapNhatDB.DuLieu.DiaChiTaiKhoanSeed, x => x.DiaChiSeedId),
                TaiKhoanIdServer = taiKhoan.TaiKhoanIdServer,
                DiaChiIdServer = diaChiIdServer,
                PhuongXaId = ward.PhuongXaId,
                TinhThanhId = province.TinhThanhId,
                DiaChi = diaChi,
                DiaChiDayDu = diaChi,
                DiaChiChiTiet = diaChiChiTiet,
                ViDo = (decimal)body["lat"]!,
                KinhDo = (decimal)body["lng"]!,
                TenNguoiNhan = $"Seed User {taiKhoan.SoThuTu}",
                SoDienThoaiNguoiNhan = taiKhoan.SoDienThoai,
                LaMacDinh = true,
                MucDichSeed = "ca_hai",
                TrangThai = "san_sang",
                TaoLuc = DateTimeOffset.Now,
                XacMinhLuc = DateTimeOffset.Now,
                GhiChu = "Tao bang API /order/add_order_address"
            });
            diaChiSanSang.Add(taiKhoan.TaiKhoanIdServer);
        }

        await _nguCanh.CapNhatDB.LuuAsync();
    }
}
