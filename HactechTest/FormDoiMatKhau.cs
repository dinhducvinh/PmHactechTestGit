using HactechTest.Services.Auth;

namespace HactechTest
{
    public partial class FormDoiMatKhau : Form
    {
        private readonly TaiKhoanPhanMem _taiKhoan;
        private readonly TaiKhoanPhanMemService _taiKhoanService;

        public FormDoiMatKhau(TaiKhoanPhanMem taiKhoan, TaiKhoanPhanMemService taiKhoanService)
        {
            _taiKhoan = taiKhoan;
            _taiKhoanService = taiKhoanService;
            InitializeComponent();
            lblTaiKhoan.Text = $"Tài khoản: {_taiKhoan.TenDangNhap}";
            btnLuu.Click += BtnLuu_Click;
            btnHuy.Click += (_, _) => Close();
        }

        private async void BtnLuu_Click(object? sender, EventArgs e)
        {
            await DoiMatKhauAsync();
        }

        private async Task DoiMatKhauAsync()
        {
            lblThongBao.Text = string.Empty;

            if (txtMatKhauMoi.Text != txtXacNhanMatKhau.Text)
            {
                lblThongBao.Text = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                return;
            }

            DatTrangThaiDangXuLy(true);
            try
            {
                await _taiKhoanService.DoiMatKhauAsync(
                    _taiKhoan.Id,
                    txtMatKhauCu.Text,
                    txtMatKhauMoi.Text);

                MessageBox.Show(
                    "Đã đổi mật khẩu.",
                    "HACTECH TEST",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblThongBao.Text = ex.Message;
            }
            finally
            {
                DatTrangThaiDangXuLy(false);
            }
        }

        private void DatTrangThaiDangXuLy(bool dangXuLy)
        {
            txtMatKhauCu.Enabled = !dangXuLy;
            txtMatKhauMoi.Enabled = !dangXuLy;
            txtXacNhanMatKhau.Enabled = !dangXuLy;
            btnLuu.Enabled = !dangXuLy;
            btnHuy.Enabled = !dangXuLy;
            UseWaitCursor = dangXuLy;
        }
    }
}
