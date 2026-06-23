using Microsoft.Data.SqlClient;

namespace HactechTest.ApiShopTesting.Seed;

public sealed partial class CapNhatDB
{
    private static readonly BangDuLieuSeed[] TatCaBangCanLuu =
    [
        BangDuLieuSeed.TinhThanh,
        BangDuLieuSeed.PhuongXa,
        BangDuLieuSeed.TaiKhoan,
        BangDuLieuSeed.Wallet,
        BangDuLieuSeed.RewardProof,
        BangDuLieuSeed.TimKiem,
        BangDuLieuSeed.TheoDoi,
        BangDuLieuSeed.Chan,
        BangDuLieuSeed.DiaChiTaiKhoan,
        BangDuLieuSeed.DanhMuc,
        BangDuLieuSeed.ThuongHieu,
        BangDuLieuSeed.SanPham,
        BangDuLieuSeed.GioHang,
        BangDuLieuSeed.DonHang,
        BangDuLieuSeed.ThichSanPham,
        BangDuLieuSeed.ReportSanPham,
        BangDuLieuSeed.TinNhan,
        BangDuLieuSeed.ThongBao
    ];

    private readonly string _chuoiKetNoi;

    public CapNhatDB(string chuoiKetNoi, BoDuLieuSeed duLieu)
    {
        _chuoiKetNoi = chuoiKetNoi;
        DuLieu = duLieu;
    }

    public BoDuLieuSeed DuLieu { get; }

    public Task LuuAsync()
    {
        return LuuAsync(TatCaBangCanLuu);
    }

    internal async Task LuuAsync(params BangDuLieuSeed[] bangCanLuu)
    {
        var danhSachBang = bangCanLuu.Length == 0
            ? TatCaBangCanLuu
            : bangCanLuu.Distinct().ToArray();

        await using var connection = new SqlConnection(_chuoiKetNoi);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        var sqlTransaction = (SqlTransaction)transaction;

        try
        {
            foreach (var bang in danhSachBang)
            {
                await LuuBangAsync(bang, connection, sqlTransaction);
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task LuuBangAsync(
        BangDuLieuSeed bang,
        SqlConnection connection,
        SqlTransaction transaction)
    {
        switch (bang)
        {
            case BangDuLieuSeed.TinhThanh:
                await LuuTinhThanhAsync(connection, transaction);
                break;
            case BangDuLieuSeed.PhuongXa:
                await LuuPhuongXaAsync(connection, transaction);
                break;
            case BangDuLieuSeed.TaiKhoan:
                await LuuTaiKhoanAsync(connection, transaction);
                break;
            case BangDuLieuSeed.Wallet:
                await LuuWalletAsync(connection, transaction);
                break;
            case BangDuLieuSeed.RewardProof:
                await LuuRewardProofAsync(connection, transaction);
                break;
            case BangDuLieuSeed.TimKiem:
                await LuuTimKiemAsync(connection, transaction);
                break;
            case BangDuLieuSeed.TheoDoi:
                await LuuTheoDoiAsync(connection, transaction);
                break;
            case BangDuLieuSeed.Chan:
                await LuuChanAsync(connection, transaction);
                break;
            case BangDuLieuSeed.DiaChiTaiKhoan:
                await LuuDiaChiTaiKhoanAsync(connection, transaction);
                break;
            case BangDuLieuSeed.DanhMuc:
                await LuuDanhMucAsync(connection, transaction);
                break;
            case BangDuLieuSeed.ThuongHieu:
                await LuuThuongHieuAsync(connection, transaction);
                break;
            case BangDuLieuSeed.SanPham:
                await LuuSanPhamAsync(connection, transaction);
                break;
            case BangDuLieuSeed.GioHang:
                await LuuGioHangAsync(connection, transaction);
                break;
            case BangDuLieuSeed.DonHang:
                await LuuDonHangAsync(connection, transaction);
                break;
            case BangDuLieuSeed.ThichSanPham:
                await LuuThichSanPhamAsync(connection, transaction);
                break;
            case BangDuLieuSeed.ReportSanPham:
                await LuuReportSanPhamAsync(connection, transaction);
                break;
            case BangDuLieuSeed.TinNhan:
                await LuuTinNhanAsync(connection, transaction);
                break;
            case BangDuLieuSeed.ThongBao:
                await LuuThongBaoAsync(connection, transaction);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(bang), bang, "Nhóm bảng seed không hợp lệ.");
        }
    }
}
