using HactechTest.Models.Auth;
using HactechTest.Services.Auth;
using HactechTest.Services.Configuration;
using Microsoft.Data.SqlClient;

namespace HactechTest.Services.App
{
    /// <summary>
    /// Giữ trạng thái ứng dụng và các service dùng chung cho WinForms.
    /// </summary>
    public sealed class AppHost
    {
        private static AppHost? _instance;

        public TaiKhoanPhanMemService TaiKhoanPhanMem { get; private set; }

        public TaiKhoanPhanMem? TaiKhoanDangNhap { get; private set; }

        public bool LaAdmin => TaiKhoanDangNhap?.LaAdmin == true;

        public bool DatabaseSanSang { get; private set; }

        public static AppHost Instance =>
            _instance ?? throw new InvalidOperationException("AppHost chưa được khởi tạo. Gọi InitializeAsync trước.");

        public static bool IsInitialized => _instance != null;

        public static async Task<AppHost> InitializeAsync(CancellationToken ct = default)
        {
            var host = new AppHost();
            await host.KiemTraDatabaseAsync(ct);
            _instance = host;
            return host;
        }

        private AppHost()
        {
            TaiKhoanPhanMem = new TaiKhoanPhanMemService(CauHinhUngDung.Instance.ConnectionString);
        }

        public void DatTaiKhoanDangNhap(TaiKhoanPhanMem taiKhoan)
        {
            TaiKhoanDangNhap = taiKhoan;
        }

        public async Task<bool> NapLaiDatabaseAsync(CancellationToken ct = default)
        {
            TaiKhoanPhanMem = new TaiKhoanPhanMemService(CauHinhUngDung.Instance.ConnectionString);
            return await KiemTraDatabaseAsync(ct);
        }

        public void DangXuat()
        {
            TaiKhoanDangNhap = null;
        }

        public string ConnectionString => CauHinhUngDung.Instance.ConnectionString;

        public Task<SqlConnection> OpenConnectionAsync(CancellationToken ct = default)
        {
            return CauHinhUngDung.Instance.OpenConnectionAsync(ct);
        }

        public static T? TaoKhiDatabaseSanSang<T>(Func<string, T> tao)
            where T : class
        {
            return IsInitialized && Instance.DatabaseSanSang
                ? tao(Instance.ConnectionString)
                : null;
        }

        public static T TaoBatBuocKhiDatabaseSanSang<T>(Func<string, T> tao, string thongBaoLoi)
            where T : class
        {
            return TaoKhiDatabaseSanSang(tao) ?? throw new InvalidOperationException(thongBaoLoi);
        }

        private async Task<bool> KiemTraDatabaseAsync(CancellationToken ct)
        {
            try
            {
                DatabaseSanSang = await CauHinhUngDung.Instance.KiemTraSchemaCoBanAsync(ct);
            }
            catch
            {
                DatabaseSanSang = false;
            }

            return DatabaseSanSang;
        }
    }
}
