namespace KiemThuApiShop.Core;

public static class ManHinhConsole
{
    public static IReadOnlyList<KichBanApi> ChonKichBanCanChay(IReadOnlyList<KichBanApi> tatCa, CauHinhChay cauHinh, string[] args)
    {
        var caseArgs = ThamSoDongLenh.LayNhieuGiaTri(args, "--case");
        if (caseArgs.Count > 0)
        {
            return tatCa
                .Where(x => caseArgs.Contains(x.Ma, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        var group = ThamSoDongLenh.LayGiaTri(args, "--group");
        if (!string.IsNullOrWhiteSpace(group))
        {
            return tatCa
                .Where(x => string.Equals(x.Nhom, group, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (ThamSoDongLenh.CoCo(args, "--all"))
        {
            return tatCa;
        }

        Console.WriteLine();
        Console.WriteLine($"Base URL hiện tại: {cauHinh.BaseUrl}");
        Console.WriteLine("Chọn chế độ chạy:");
        Console.WriteLine("1. Chạy tất cả test case");
        Console.WriteLine("2. Chạy theo nhóm Auth/User/Search/FollowBlock/DevTokenPush");
        Console.WriteLine("3. Chọn mã test case cụ thể");
        Console.WriteLine("4. Chỉ liệt kê test case");
        Console.Write("Nhập lựa chọn: ");
        var luaChon = Console.ReadLine()?.Trim();

        return luaChon switch
        {
            "1" => tatCa,
            "2" => ChonTheoNhom(tatCa),
            "3" => ChonTheoMa(tatCa),
            "4" => InVaTraRong(tatCa),
            _ => tatCa
        };
    }

    public static void InDanhSachKichBan(IReadOnlyList<KichBanApi> tatCa)
    {
        foreach (var nhom in tatCa.GroupBy(x => x.Nhom))
        {
            Console.WriteLine();
            Console.WriteLine($"[{nhom.Key}]");
            foreach (var item in nhom)
            {
                Console.WriteLine($"{item.Ma,-28} {item.TenHienThi}");
            }
        }
    }

    public static void InTongKet(IReadOnlyList<KetQuaChay> ketQua, int soDongKetQuaDaLuu)
    {
        var dat = ketQua.Count(x => x.TrangThai == TrangThaiKetQua.Dat);
        var thatBai = ketQua.Count(x => x.TrangThai == TrangThaiKetQua.ThatBai);
        var boQua = ketQua.Count(x => x.TrangThai == TrangThaiKetQua.BoQua);
        var loiChuanBi = ketQua.Count(x => x.TrangThai == TrangThaiKetQua.LoiChuanBi);
        var loiMoiTruong = ketQua.Count(x => x.TrangThai == TrangThaiKetQua.LoiMoiTruong);

        Console.WriteLine();
        Console.WriteLine("Tổng kết:");
        Console.WriteLine($"- Đạt: {dat}");
        Console.WriteLine($"- Fail logic server/expected: {thatBai}");
        Console.WriteLine($"- Bỏ qua do thiếu điều kiện test: {boQua}");
        Console.WriteLine($"- Lỗi chuẩn bị test: {loiChuanBi}");
        Console.WriteLine($"- Lỗi môi trường/base URL: {loiMoiTruong}");
        Console.WriteLine($"Kết quả đã lưu vào SQL Server: dbo.ketqua_testcase ({soDongKetQuaDaLuu} dòng).");
    }

    private static IReadOnlyList<KichBanApi> ChonTheoNhom(IReadOnlyList<KichBanApi> tatCa)
    {
        Console.Write("Nhập nhóm cần chạy (Auth/User/Search/FollowBlock/DevTokenPush): ");
        var nhom = Console.ReadLine()?.Trim();
        return tatCa.Where(x => string.Equals(x.Nhom, nhom, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private static IReadOnlyList<KichBanApi> ChonTheoMa(IReadOnlyList<KichBanApi> tatCa)
    {
        InDanhSachKichBan(tatCa);
        Console.WriteLine();
        Console.Write("Nhập mã case, cách nhau bằng dấu phẩy: ");
        var ma = Console.ReadLine()?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? [];

        return tatCa.Where(x => ma.Contains(x.Ma, StringComparer.OrdinalIgnoreCase)).ToList();
    }

    private static IReadOnlyList<KichBanApi> InVaTraRong(IReadOnlyList<KichBanApi> tatCa)
    {
        InDanhSachKichBan(tatCa);
        return [];
    }
}
