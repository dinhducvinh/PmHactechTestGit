namespace HactechTest.ApiShopTesting.Core;

public static class DinhDangKetQuaKiemThu
{
    public static bool LaKhongDat(TrangThaiKetQua trangThai)
    {
        return trangThai is TrangThaiKetQua.ThatBai
            or TrangThaiKetQua.LoiChuanBi
            or TrangThaiKetQua.LoiMoiTruong;
    }

    public static string TrangThaiHienThi(TrangThaiKetQua trangThai)
    {
        return trangThai switch
        {
            TrangThaiKetQua.Dat => "PASS",
            TrangThaiKetQua.ThatBai => "FAIL",
            TrangThaiKetQua.BoQua => "SKIP",
            TrangThaiKetQua.LoiChuanBi => "SETUP ERROR",
            TrangThaiKetQua.LoiMoiTruong => "ENV ERROR",
            _ => trangThai.ToString()
        };
    }

    public static string TrangThaiLuuDatabase(TrangThaiKetQua trangThai)
    {
        return trangThai switch
        {
            TrangThaiKetQua.Dat => "PASS",
            TrangThaiKetQua.ThatBai => "FAIL",
            TrangThaiKetQua.BoQua => "SKIP",
            TrangThaiKetQua.LoiChuanBi => "ERROR",
            TrangThaiKetQua.LoiMoiTruong => "ERROR",
            _ => "ERROR"
        };
    }
}
