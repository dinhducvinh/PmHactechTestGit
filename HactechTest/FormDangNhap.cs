
using HactechTest.Services.App;
using HactechTest.Services.Auth;

namespace HactechTest
{
    public partial class FormDangNhap : Form
    {
        private bool _choPhepDong;

        public TaiKhoanPhanMem? TaiKhoanDangNhap { get; private set; }

        public FormDangNhap()
        {
            InitializeComponent();
            btnDangNhap.Click += BtnDangNhap_Click;
            btnThoat.Click += BtnThoat_Click;
            btnCauHinhDb.Click += async (_, _) => await MoCauHinhDatabaseAsync();
            FormClosing += FormDangNhap_FormClosing;
            CapNhatTrangThaiDatabase();
        }

        private async void BtnDangNhap_Click(object? sender, EventArgs e)
        {
            await DangNhapAsync();
        }

        private async Task DangNhapAsync()
        {
            lblThongBao.Text = string.Empty;
            if (!AppHost.Instance.DatabaseSanSang)
            {
                lblThongBao.Text = "Chưa cấu hình database hoặc schema chưa đầy đủ. Bấm Cấu hình DB.";
                return;
            }

            var tenDangNhap = txtTenDangNhap.Text.Trim();
            var matKhau = txtMatKhau.Text;
            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau))
            {
                lblThongBao.Text = "Nhập tên đăng nhập và mật khẩu.";
                return;
            }

            DatTrangThaiDangXuLy(true);
            try
            {
                var taiKhoan = await AppHost.Instance.TaiKhoanPhanMem.DangNhapAsync(tenDangNhap, matKhau);
                if (taiKhoan is null)
                {
                    lblThongBao.Text = "Tên đăng nhập hoặc mật khẩu không đúng, hoặc tài khoản đã bị khóa.";
                    return;
                }

                TaiKhoanDangNhap = taiKhoan;
                _choPhepDong = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblThongBao.Text = "Không đăng nhập được. " + ex.Message;
            }
            finally
            {
                DatTrangThaiDangXuLy(false);
            }
        }

        private void DatTrangThaiDangXuLy(bool dangXuLy)
        {
            txtTenDangNhap.Enabled = !dangXuLy;
            txtMatKhau.Enabled = !dangXuLy;
            btnDangNhap.Enabled = !dangXuLy;
            btnCauHinhDb.Enabled = !dangXuLy;
            btnThoat.Enabled = !dangXuLy;
            UseWaitCursor = dangXuLy;
        }

        private async Task MoCauHinhDatabaseAsync()
        {
            using var form = new FormCauHinhDatabase();
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                CapNhatTrangThaiDatabase();
                return;
            }

            await AppHost.Instance.NapLaiDatabaseAsync();
            CapNhatTrangThaiDatabase();
        }

        private void CapNhatTrangThaiDatabase()
        {
            btnDangNhap.Enabled = AppHost.Instance.DatabaseSanSang;
            if (AppHost.Instance.DatabaseSanSang)
            {
                lblThongBao.Text = string.Empty;
            }
            else
            {
                lblThongBao.Text = "Chưa cấu hình database hoặc schema chưa đầy đủ. Bấm Cấu hình DB.";
            }
        }

        private void BtnThoat_Click(object? sender, EventArgs e)
        {
            if (!XacNhanDongPhanMem())
            {
                return;
            }

            _choPhepDong = true;
            Close();
        }

        private void FormDangNhap_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_choPhepDong)
            {
                return;
            }

            if (!XacNhanDongPhanMem())
            {
                e.Cancel = true;
                return;
            }

            _choPhepDong = true;
        }

        private static bool XacNhanDongPhanMem()
        {
            return MessageBox.Show(
                "Bạn có muốn đóng phần mềm không?",
                "HACTECH TEST",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }
}
