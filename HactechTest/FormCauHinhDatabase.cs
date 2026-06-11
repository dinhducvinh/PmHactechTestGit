using HactechTest.Services.App;
using HactechTest.Services.Configuration;

namespace HactechTest
{
    public partial class FormCauHinhDatabase : Form
    {
        public FormCauHinhDatabase()
        {
            InitializeComponent();
            NapCauHinhHienTai();

            btnKiemTraKetNoi.Click += async (_, _) => await KiemTraKetNoiAsync(hienThongBaoThanhCong: true);
            btnLuu.Click += async (_, _) => await LuuAsync();
            btnHuy.Click += (_, _) => Close();
        }

        private void NapCauHinhHienTai()
        {
            var connectionString = CauHinhUngDung.Instance.ConnectionString;
            txtConnectionString.Text = string.IsNullOrWhiteSpace(connectionString)
                ? "Server=.;Database=HactechTestDb;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=10"
                : connectionString;
        }

        private string LayConnectionString()
        {
            var connectionString = txtConnectionString.Text.Trim();
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string không được rỗng.");
            }

            return connectionString;
        }

        private async Task<bool> KiemTraKetNoiAsync(bool hienThongBaoThanhCong)
        {
            lblThongBao.Text = string.Empty;
            DatTrangThaiDangXuLy(true);
            try
            {
                var connectionString = LayConnectionString();
                var hopLe = await CauHinhUngDung.KiemTraSchemaCoBanAsync(connectionString);
                if (!hopLe)
                {
                    lblThongBao.ForeColor = Color.FromArgb(220, 53, 69);
                    lblThongBao.Text = "Kết nối được SQL Server nhưng schema chưa đầy đủ.";
                    MessageBox.Show(
                        "Kết nối được SQL Server nhưng database chưa đủ bảng/view của HACTECH TEST.\n\n" +
                        "Hãy chạy các file SQL trong thư mục HactechTest\\Database trước.",
                        "HACTECH TEST - Cấu hình DB",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return false;
                }

                lblThongBao.ForeColor = Color.FromArgb(34, 150, 82);
                lblThongBao.Text = "Kết nối thành công.";

                if (hienThongBaoThanhCong)
                {
                    MessageBox.Show(
                        "Kết nối database thành công. Schema HACTECH TEST đã sẵn sàng.",
                        "HACTECH TEST - Cấu hình DB",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                return true;
            }
            catch (Exception ex)
            {
                lblThongBao.ForeColor = Color.FromArgb(220, 53, 69);
                lblThongBao.Text = "Không kết nối được database.";
                MessageBox.Show(
                    "Không kết nối được database.\n\n" + ex.Message,
                    "HACTECH TEST - Cấu hình DB",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                DatTrangThaiDangXuLy(false);
            }
        }

        private async Task LuuAsync()
        {
            if (!await KiemTraKetNoiAsync(hienThongBaoThanhCong: false))
            {
                return;
            }

            CauHinhUngDung.LuuChuoiKetNoiDatabase(LayConnectionString());
            if (AppHost.IsInitialized)
            {
                await AppHost.Instance.NapLaiDatabaseAsync();
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void DatTrangThaiDangXuLy(bool dangXuLy)
        {
            UseWaitCursor = dangXuLy;
            txtConnectionString.Enabled = !dangXuLy;
            btnKiemTraKetNoi.Enabled = !dangXuLy;
            btnLuu.Enabled = !dangXuLy;
            btnHuy.Enabled = !dangXuLy;
        }
    }
}
