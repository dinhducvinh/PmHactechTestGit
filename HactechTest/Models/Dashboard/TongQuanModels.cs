namespace HactechTest.Models.Dashboard;

public sealed record BoLocTongQuan(DateTime NgayLoc, string? NguoiChay);

public sealed record TongQuanThongKe(int TongTest, int TongDat, int TongKhongDat)
{
    public int TyLeDat => TongTest == 0 ? 0 : (int)Math.Round(100d * TongDat / TongTest);
}
