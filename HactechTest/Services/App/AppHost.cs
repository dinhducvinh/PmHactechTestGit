using HactechTest.Services.Auth;
using HactechTest.Services.Configuration;
using HactechTest.Services.Data;

namespace HactechTest.Services.App
{
    /// <summary>
    /// Giữ trạng thái ứng dụng và các service dùng chung cho WinForms.
    /// </summary>
    public sealed class AppHost
    {
        private static AppHost? _instance;

        public Database Database { get; private set; }

        public TaiKhoanPhanMemService TaiKhoanPhanMem { get; private set; }

        public TaiKhoanPhanMem? TaiKhoanDangNhap { get; private set; }

        public bool LaAdmin => TaiKhoanDangNhap?.LaAdmin == true;

        public bool DatabaseSanSang { get; private set; }

        public static AppHost Instance =>
            _instance ?? throw new InvalidOperationException("AppHost chưa được khởi tạo. Gọi InitializeAsync trước.");

        public static bool IsInitialized => _instance != null;

        public static async Task<AppHost> InitializeAsync(CancellationToken ct = default)
        {
            var host = new AppHost(new Database(CauHinhUngDung.Instance.ConnectionString));
            await host.KiemTraDatabaseAsync(ct);
            _instance = host;
            return host;
        }

        private AppHost(Database database)
        {
            Database = database;
            TaiKhoanPhanMem = new TaiKhoanPhanMemService(database);
        }

        public void DatTaiKhoanDangNhap(TaiKhoanPhanMem taiKhoan)
        {
            TaiKhoanDangNhap = taiKhoan;
        }

        public async Task<bool> NapLaiDatabaseAsync(CancellationToken ct = default)
        {
            GanDatabase(new Database(CauHinhUngDung.Instance.ConnectionString));
            return await KiemTraDatabaseAsync(ct);
        }

        public void DangXuat()
        {
            TaiKhoanDangNhap = null;
        }

        private void GanDatabase(Database database)
        {
            Database = database;
            TaiKhoanPhanMem = new TaiKhoanPhanMemService(database);
        }

        private async Task<bool> KiemTraDatabaseAsync(CancellationToken ct)
        {
            try
            {
                DatabaseSanSang = await Database.KiemTraSchemaCoBanAsync(ct);
            }
            catch
            {
                DatabaseSanSang = false;
            }

            return DatabaseSanSang;
        }
    }
}
