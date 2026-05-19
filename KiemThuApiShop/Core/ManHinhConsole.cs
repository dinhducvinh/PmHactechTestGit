using System.Text.Json;

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
        Console.WriteLine("2. Chạy theo nhóm Auth/User/Product");
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

    public static void InTongKet(IReadOnlyList<KetQuaChay> ketQua, string tepBaoCao)
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
        Console.WriteLine($"Báo cáo đã ghi: {tepBaoCao}");
    }

    private static IReadOnlyList<KichBanApi> ChonTheoNhom(IReadOnlyList<KichBanApi> tatCa)
    {
        Console.Write("Nhập nhóm cần chạy (Auth/User/Product): ");
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

public static class BaoCaoKetQua
{
    public static async Task<string> GhiBaoCaoAsync(IReadOnlyList<KetQuaChay> ketQua, string thuMuc)
    {
        Directory.CreateDirectory(thuMuc);
        var tenFile = $"bao-cao-{DateTimeOffset.Now:yyyyMMdd-HHmmss}.json";
        var duongDan = Path.Combine(thuMuc, tenFile);
        var json = JsonSerializer.Serialize(ketQua, TuyChonJson.MacDinh);
        await File.WriteAllTextAsync(duongDan, json);

        var mdPath = Path.ChangeExtension(duongDan, ".md");
        await File.WriteAllTextAsync(mdPath, TaoMarkdown(ketQua));

        return Path.GetFullPath(mdPath);
    }

    private static string TaoMarkdown(IReadOnlyList<KetQuaChay> ketQua)
    {
        var lines = new List<string>
        {
            "# Báo cáo kiểm thử API shop",
            "",
            $"Thời gian: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}",
            "",
            "| Mã | Nhóm | Trạng thái | Mã mong đợi | Mã thực tế | Endpoint | Thông điệp |",
            "| --- | --- | --- | --- | --- | --- | --- |"
        };

        foreach (var item in ketQua)
        {
            lines.Add($"| {Escape(item.Ma)} | {Escape(item.Nhom)} | {item.TrangThai} | {Escape(item.MaMongDoi)} | {Escape(item.MaThucTe)} | {Escape(item.Endpoint)} | {Escape(item.ThongDiep)} |");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string Escape(string? text)
    {
        return (text ?? "").Replace("|", "\\|").Replace(Environment.NewLine, " ");
    }
}
