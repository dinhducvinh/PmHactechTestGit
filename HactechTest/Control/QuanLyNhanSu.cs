using HactechTest.Services.App;
using HactechTest.Services.Auth;

namespace HactechTest.Control
{
    public partial class QuanLyNhanSu : UserControl
    {
        private List<TaiKhoanPhanMem> _danhSach = [];
        private int? _idDangChon;

        public QuanLyNhanSu()
        {
            InitializeComponent();

            if (!DesignMode)
            {
                btnTaiLai.Click += async (_, _) => await NapDanhSachAsync();
                btnTaoTaiKhoan.Click += async (_, _) => await TaoTaiKhoanAsync();
                btnCapNhat.Click += async (_, _) => await CapNhatTaiKhoanAsync();
                btnLamMoiForm.Click += (_, _) => LamMoiFormNhap();
                btnTimKiem.Click += (_, _) => ApDungTimKiem(lamMoiLuaChon: true);
                btnDatLaiTimKiem.Click += (_, _) =>
                {
                    txtTimKiem.Clear();
                    ApDungTimKiem(lamMoiLuaChon: true);
                };
                txtTimKiem.KeyDown += TxtTimKiem_KeyDown;
                dgvNhanSu.SelectionChanged += DgvNhanSu_SelectionChanged;
            }
        }

        public async Task NapDanhSachAsync()
        {
            if (!AppHost.IsInitialized || !AppHost.Instance.DatabaseSanSang)
            {
                HienThiTrangThai("CSDL chưa sẵn sàng.");
                pnlNoiDung.Enabled = false;
                return;
            }

            if (!AppHost.Instance.LaAdmin)
            {
                HienThiTrangThai("Chỉ tài khoản admin được quản lý nhân sự test.");
                pnlNoiDung.Enabled = false;
                return;
            }

            pnlNoiDung.Enabled = true;
            HienThiTrangThai("Đang tải danh sách tài khoản...");
            try
            {
                _danhSach = (await AppHost.Instance.TaiKhoanPhanMem.LayDanhSachAsync()).ToList();
                ApDungTimKiem(lamMoiLuaChon: true);
            }
            catch (Exception ex)
            {
                HienThiTrangThai("Không tải được danh sách nhân sự.");
                MessageBox.Show(
                    "Không tải được danh sách nhân sự.\n\n" + ex.Message,
                    "HACTECH TEST",
                    MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            }
        }

        private void TxtTimKiem_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;
            ApDungTimKiem(lamMoiLuaChon: true);
        }

        private void ApDungTimKiem(bool lamMoiLuaChon = false)
        {
            var tuKhoa = txtTimKiem.Text.Trim();
            var ketQua = string.IsNullOrWhiteSpace(tuKhoa)
                ? _danhSach
                : _danhSach
                    .Where(taiKhoan =>
                        ChuaTuKhoa(taiKhoan.TenDangNhap, tuKhoa) ||
                        ChuaTuKhoa(taiKhoan.HoTen, tuKhoa) ||
                        ChuaTuKhoa(taiKhoan.Email, tuKhoa) ||
                        ChuaTuKhoa(taiKhoan.SoDienThoai, tuKhoa) ||
                        ChuaTuKhoa(taiKhoan.VaiTro, tuKhoa) ||
                        ChuaTuKhoa(taiKhoan.TrangThai, tuKhoa))
                    .ToList();

            dgvNhanSu.DataSource = null;
            dgvNhanSu.DataSource = ketQua;

            HienThiTrangThai(string.IsNullOrWhiteSpace(tuKhoa)
                ? $"Đã tải {_danhSach.Count} tài khoản."
                : $"Tìm thấy {ketQua.Count}/{_danhSach.Count} tài khoản với từ khóa \"{tuKhoa}\".");

            if (lamMoiLuaChon)
            {
                LamMoiFormNhap();
            }
        }

        private static bool ChuaTuKhoa(string? giaTri, string tuKhoa)
        {
            return !string.IsNullOrWhiteSpace(giaTri) &&
                giaTri.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase);
        }

        private async Task TaoTaiKhoanAsync()
        {
            try
            {
                var request = new TaoTaiKhoanPhanMemRequest(
                    txtTenDangNhap.Text,
                    txtMatKhau.Text,
                    txtHoTen.Text,
                    txtEmail.Text,
                    txtSoDienThoai.Text,
                    LayVaiTro(),
                    LayTrangThai());

                await AppHost.Instance.TaiKhoanPhanMem.TaoTaiKhoanAsync(request);
                HienThiTrangThai("Đã tạo tài khoản.");
                await NapDanhSachAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không tạo được tài khoản.\n\n" + ex.Message,
                    "HACTECH TEST",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private async Task CapNhatTaiKhoanAsync()
        {
            if (!_idDangChon.HasValue)
            {
                MessageBox.Show(
                    "Chọn một tài khoản trong danh sách trước khi cập nhật.",
                    "HACTECH TEST",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            try
            {
                var request = new CapNhatTaiKhoanPhanMemRequest(
                    _idDangChon.Value,
                    txtTenDangNhap.Text,
                    string.IsNullOrWhiteSpace(txtMatKhau.Text) ? null : txtMatKhau.Text,
                    txtHoTen.Text,
                    txtEmail.Text,
                    txtSoDienThoai.Text,
                    LayVaiTro(),
                    LayTrangThai());

                await AppHost.Instance.TaiKhoanPhanMem.CapNhatTaiKhoanAsync(request);
                HienThiTrangThai("Đã cập nhật tài khoản.");
                await NapDanhSachAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không cập nhật được tài khoản.\n\n" + ex.Message,
                    "HACTECH TEST",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void DgvNhanSu_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvNhanSu.CurrentRow?.DataBoundItem is not TaiKhoanPhanMem taiKhoan)
            {
                return;
            }

            _idDangChon = taiKhoan.Id;
            lblDangChon.Text = $"Đang chọn: {taiKhoan.TenDangNhap}";
            txtTenDangNhap.Text = taiKhoan.TenDangNhap;
            txtMatKhau.Clear();
            txtHoTen.Text = taiKhoan.HoTen ?? string.Empty;
            txtEmail.Text = taiKhoan.Email ?? string.Empty;
            txtSoDienThoai.Text = taiKhoan.SoDienThoai ?? string.Empty;
            cboVaiTro.SelectedItem = taiKhoan.VaiTro;
            chkHoatDong.Checked = taiKhoan.TrangThai == "hoat_dong";
            btnCapNhat.Enabled = true;
        }

        private void LamMoiFormNhap()
        {
            _idDangChon = null;
            lblDangChon.Text = "Tạo tài khoản mới";
            txtTenDangNhap.Clear();
            txtMatKhau.Clear();
            txtHoTen.Clear();
            txtEmail.Clear();
            txtSoDienThoai.Clear();
            cboVaiTro.SelectedItem = "nhan_vien";
            chkHoatDong.Checked = true;
            btnCapNhat.Enabled = false;
            dgvNhanSu.ClearSelection();
            txtTenDangNhap.Focus();
        }

        private string LayVaiTro()
        {
            return cboVaiTro.SelectedItem?.ToString() == "admin" ? "admin" : "nhan_vien";
        }

        private string LayTrangThai()
        {
            return chkHoatDong.Checked ? "hoat_dong" : "khoa";
        }

        private void HienThiTrangThai(string thongBao)
        {
            lblTrangThai.Text = thongBao;
        }
    }
}
