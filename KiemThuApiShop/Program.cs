using System.Text;
using KiemThuApiShop.Core;
using KiemThuApiShop.KichBan;
using KiemThuApiShop.Seed;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var cauHinh = CauHinhChay.DocTuFile("appsettings.json");
cauHinh = CauHinhChay.GhiDeBangThamSoDongLenh(cauHinh, args);

var danhSachKichBan = BoKichBanApi.TaoTatCaKichBan();

if (ThamSoDongLenh.CoCo(args, "--list"))
{
    ManHinhConsole.InDanhSachKichBan(danhSachKichBan);
    return;
}

var khoSeed = new KhoDuLieuSeedSqlServer(cauHinh.ChuoiKetNoiSqlServer, cauHinh.SoTaiKhoanSeed);
await khoSeed.TaiAsync();

using var mayKhach = new MayKhachApi(cauHinh.BaseUrl, TimeSpan.FromSeconds(cauHinh.TimeoutGiay));
var nguCanh = new NguCanhKiemThu(cauHinh, mayKhach, khoSeed);

if (cauHinh.TuDongChuanBiDuLieu)
{
    var chuanBi = new ChuanBiDuLieuMoi(nguCanh);
    await chuanBi.ChuanBiAsync();
}

var kichBanCanChay = ManHinhConsole.ChonKichBanCanChay(danhSachKichBan, cauHinh, args);
if (kichBanCanChay.Count == 0)
{
    Console.WriteLine("Không có kịch bản nào được chọn.");
    return;
}

var trinhChay = new TrinhChayKiemThu(nguCanh);
var ketQua = await trinhChay.ChayAsync(kichBanCanChay);

var tepBaoCao = await BaoCaoKetQua.GhiBaoCaoAsync(ketQua, cauHinh.ThuMucKetQua);
ManHinhConsole.InTongKet(ketQua, tepBaoCao);
