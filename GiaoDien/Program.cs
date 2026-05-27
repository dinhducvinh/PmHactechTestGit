using ApiTest.Services;

namespace ApiTest
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
                    "App vẫn chạy nhưng các thao tác cần CSDL có thể thất bại. " +
                    "Hãy kiểm tra connection string trong appsettings.json.",
                    "API TESTER - Cảnh báo khởi tạo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            Application.Run(new FormMain());
        }
    }
}
