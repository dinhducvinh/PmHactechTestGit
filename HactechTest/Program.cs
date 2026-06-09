using HactechTest.Services.App;

namespace HactechTest
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            try
            {
                AppHost.InitializeAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không khởi tạo được hạ tầng test.\n\n" +
                    "Chi tiết: " + ex.Message + "\n\n" +
                    "App vẫn chạy để bạn có thể mở màn hình cấu hình DB.",
                    "HACTECH TEST - Cảnh báo khởi tạo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            while (true)
            {
                using (var formDangNhap = new FormDangNhap())
                {
                    if (formDangNhap.ShowDialog() != DialogResult.OK ||
                        formDangNhap.TaiKhoanDangNhap is null)
                    {
                        return;
                    }

                    AppHost.Instance.DatTaiKhoanDangNhap(formDangNhap.TaiKhoanDangNhap);
                }

                using var formMain = new FormMain();
                Application.Run(formMain);

                if (!formMain.DaDangXuat)
                {
                    return;
                }
            }
        }
    }
}

