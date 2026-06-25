namespace HactechTest.Control
{
    partial class QuanLyNhanSu
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            pnlRoot = new Panel();
            pnlNoiDung = new Panel();
            tblNoiDung = new TableLayoutPanel();
            pnlDanhSach = new Panel();
            dgvNhanSu = new DataGridView();
            pnlForm = new Panel();
            tblForm = new TableLayoutPanel();
            lblDangChon = new Label();
            lblTenDangNhap = new Label();
            txtTenDangNhap = new TextBox();
            lblMatKhau = new Label();
            txtMatKhau = new TextBox();
            lblHoTen = new Label();
            txtHoTen = new TextBox();
            lblEmail = new Label();
            txtEmail = new TextBox();
            lblSoDienThoai = new Label();
            txtSoDienThoai = new TextBox();
            lblVaiTro = new Label();
            cboVaiTro = new ComboBox();
            chkHoatDong = new CheckBox();
            pnlNutForm = new Panel();
            btnLamMoiForm = new Button();
            btnCapNhat = new Button();
            btnTaoTaiKhoan = new Button();
            pnlToolbar = new Panel();
            lblTimKiem = new Label();
            txtTimKiem = new TextBox();
            btnTimKiem = new Button();
            btnDatLaiTimKiem = new Button();
            lblTrangThai = new Label();
            btnTaiLai = new Button();
            pnlDauTrang = new Panel();
            lblTieuDe = new Label();
            lblMoTa = new Label();
            pnlRoot.SuspendLayout();
            pnlNoiDung.SuspendLayout();
            tblNoiDung.SuspendLayout();
            pnlDanhSach.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvNhanSu).BeginInit();
            pnlForm.SuspendLayout();
            tblForm.SuspendLayout();
            pnlNutForm.SuspendLayout();
            pnlToolbar.SuspendLayout();
            pnlDauTrang.SuspendLayout();
            SuspendLayout();
            // 
            // pnlRoot
            // 
            pnlRoot.BackColor = Color.FromArgb(244, 245, 247);
            pnlRoot.Controls.Add(pnlNoiDung);
            pnlRoot.Controls.Add(pnlToolbar);
            pnlRoot.Controls.Add(pnlDauTrang);
            pnlRoot.Dock = DockStyle.Fill;
            pnlRoot.Location = new Point(0, 0);
            pnlRoot.Name = "pnlRoot";
            pnlRoot.Padding = new Padding(20);
            pnlRoot.Size = new Size(1664, 793);
            pnlRoot.TabIndex = 0;
            // 
            // pnlNoiDung
            // 
            pnlNoiDung.Controls.Add(tblNoiDung);
            pnlNoiDung.Dock = DockStyle.Fill;
            pnlNoiDung.Location = new Point(20, 128);
            pnlNoiDung.Name = "pnlNoiDung";
            pnlNoiDung.Size = new Size(1624, 645);
            pnlNoiDung.TabIndex = 2;
            // 
            // tblNoiDung
            // 
            tblNoiDung.ColumnCount = 2;
            tblNoiDung.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66F));
            tblNoiDung.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            tblNoiDung.Controls.Add(pnlDanhSach, 0, 0);
            tblNoiDung.Controls.Add(pnlForm, 1, 0);
            tblNoiDung.Dock = DockStyle.Fill;
            tblNoiDung.Location = new Point(0, 0);
            tblNoiDung.Name = "tblNoiDung";
            tblNoiDung.RowCount = 1;
            tblNoiDung.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblNoiDung.Size = new Size(1624, 645);
            tblNoiDung.TabIndex = 0;
            // 
            // pnlDanhSach
            // 
            pnlDanhSach.BackColor = Color.White;
            pnlDanhSach.BorderStyle = BorderStyle.FixedSingle;
            pnlDanhSach.Controls.Add(dgvNhanSu);
            pnlDanhSach.Dock = DockStyle.Fill;
            pnlDanhSach.Location = new Point(0, 0);
            pnlDanhSach.Margin = new Padding(0, 0, 16, 0);
            pnlDanhSach.Name = "pnlDanhSach";
            pnlDanhSach.Size = new Size(1055, 645);
            pnlDanhSach.TabIndex = 0;
            // 
            // dgvNhanSu
            // 
            dgvNhanSu.AllowUserToAddRows = false;
            dgvNhanSu.AllowUserToDeleteRows = false;
            dgvNhanSu.BackgroundColor = Color.White;
            dgvNhanSu.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(235, 240, 247);
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = Color.FromArgb(30, 39, 50);
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dgvNhanSu.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvNhanSu.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.White;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(24, 30, 37);
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(214, 228, 255);
            dataGridViewCellStyle2.SelectionForeColor = Color.FromArgb(20, 32, 45);
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dgvNhanSu.DefaultCellStyle = dataGridViewCellStyle2;
            dgvNhanSu.Dock = DockStyle.Fill;
            dgvNhanSu.EnableHeadersVisualStyles = false;
            dgvNhanSu.GridColor = Color.FromArgb(216, 222, 230);
            dgvNhanSu.Location = new Point(0, 0);
            dgvNhanSu.MultiSelect = false;
            dgvNhanSu.Name = "dgvNhanSu";
            dgvNhanSu.ReadOnly = true;
            dgvNhanSu.RowHeadersVisible = false;
            dgvNhanSu.RowHeadersWidth = 48;
            dgvNhanSu.RowTemplate.Height = 30;
            dgvNhanSu.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvNhanSu.Size = new Size(1053, 643);
            dgvNhanSu.TabIndex = 0;
            // 
            // pnlForm
            // 
            pnlForm.BackColor = Color.White;
            pnlForm.BorderStyle = BorderStyle.FixedSingle;
            pnlForm.Controls.Add(tblForm);
            pnlForm.Dock = DockStyle.Fill;
            pnlForm.Location = new Point(1071, 0);
            pnlForm.Margin = new Padding(0);
            pnlForm.Name = "pnlForm";
            pnlForm.Size = new Size(553, 645);
            pnlForm.TabIndex = 1;
            // 
            // tblForm
            // 
            tblForm.ColumnCount = 1;
            tblForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblForm.Controls.Add(lblDangChon, 0, 0);
            tblForm.Controls.Add(lblTenDangNhap, 0, 1);
            tblForm.Controls.Add(txtTenDangNhap, 0, 2);
            tblForm.Controls.Add(lblMatKhau, 0, 3);
            tblForm.Controls.Add(txtMatKhau, 0, 4);
            tblForm.Controls.Add(lblHoTen, 0, 5);
            tblForm.Controls.Add(txtHoTen, 0, 6);
            tblForm.Controls.Add(lblEmail, 0, 7);
            tblForm.Controls.Add(txtEmail, 0, 8);
            tblForm.Controls.Add(lblSoDienThoai, 0, 9);
            tblForm.Controls.Add(txtSoDienThoai, 0, 10);
            tblForm.Controls.Add(lblVaiTro, 0, 11);
            tblForm.Controls.Add(cboVaiTro, 0, 12);
            tblForm.Controls.Add(chkHoatDong, 0, 13);
            tblForm.Controls.Add(pnlNutForm, 0, 14);
            tblForm.Dock = DockStyle.Fill;
            tblForm.Location = new Point(0, 0);
            tblForm.Name = "tblForm";
            tblForm.Padding = new Padding(20);
            tblForm.RowCount = 15;
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblForm.Size = new Size(551, 643);
            tblForm.TabIndex = 0;
            // 
            // lblDangChon
            // 
            lblDangChon.Dock = DockStyle.Fill;
            lblDangChon.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblDangChon.Location = new Point(20, 20);
            lblDangChon.Margin = new Padding(0);
            lblDangChon.Name = "lblDangChon";
            lblDangChon.Size = new Size(511, 38);
            lblDangChon.TabIndex = 0;
            lblDangChon.Text = "Tạo tài khoản mới";
            lblDangChon.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTenDangNhap
            // 
            lblTenDangNhap.Dock = DockStyle.Fill;
            lblTenDangNhap.Location = new Point(20, 58);
            lblTenDangNhap.Margin = new Padding(0);
            lblTenDangNhap.Name = "lblTenDangNhap";
            lblTenDangNhap.Size = new Size(511, 26);
            lblTenDangNhap.TabIndex = 1;
            lblTenDangNhap.Text = "Tên đăng nhập";
            lblTenDangNhap.TextAlign = ContentAlignment.BottomLeft;
            // 
            // txtTenDangNhap
            // 
            txtTenDangNhap.Dock = DockStyle.Fill;
            txtTenDangNhap.Location = new Point(20, 88);
            txtTenDangNhap.Margin = new Padding(0, 4, 0, 0);
            txtTenDangNhap.Name = "txtTenDangNhap";
            txtTenDangNhap.Size = new Size(511, 27);
            txtTenDangNhap.TabIndex = 0;
            // 
            // lblMatKhau
            // 
            lblMatKhau.Dock = DockStyle.Fill;
            lblMatKhau.Location = new Point(20, 122);
            lblMatKhau.Margin = new Padding(0);
            lblMatKhau.Name = "lblMatKhau";
            lblMatKhau.Size = new Size(511, 26);
            lblMatKhau.TabIndex = 2;
            lblMatKhau.Text = "Mật khẩu";
            lblMatKhau.TextAlign = ContentAlignment.BottomLeft;
            // 
            // txtMatKhau
            // 
            txtMatKhau.Dock = DockStyle.Fill;
            txtMatKhau.Location = new Point(20, 152);
            txtMatKhau.Margin = new Padding(0, 4, 0, 0);
            txtMatKhau.Name = "txtMatKhau";
            txtMatKhau.PasswordChar = '●';
            txtMatKhau.Size = new Size(511, 27);
            txtMatKhau.TabIndex = 1;
            // 
            // lblHoTen
            // 
            lblHoTen.Dock = DockStyle.Fill;
            lblHoTen.Location = new Point(20, 186);
            lblHoTen.Margin = new Padding(0);
            lblHoTen.Name = "lblHoTen";
            lblHoTen.Size = new Size(511, 26);
            lblHoTen.TabIndex = 3;
            lblHoTen.Text = "Họ tên";
            lblHoTen.TextAlign = ContentAlignment.BottomLeft;
            // 
            // txtHoTen
            // 
            txtHoTen.Dock = DockStyle.Fill;
            txtHoTen.Location = new Point(20, 216);
            txtHoTen.Margin = new Padding(0, 4, 0, 0);
            txtHoTen.Name = "txtHoTen";
            txtHoTen.Size = new Size(511, 27);
            txtHoTen.TabIndex = 2;
            // 
            // lblEmail
            // 
            lblEmail.Dock = DockStyle.Fill;
            lblEmail.Location = new Point(20, 250);
            lblEmail.Margin = new Padding(0);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(511, 26);
            lblEmail.TabIndex = 4;
            lblEmail.Text = "Email";
            lblEmail.TextAlign = ContentAlignment.BottomLeft;
            // 
            // txtEmail
            // 
            txtEmail.Dock = DockStyle.Fill;
            txtEmail.Location = new Point(20, 280);
            txtEmail.Margin = new Padding(0, 4, 0, 0);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(511, 27);
            txtEmail.TabIndex = 3;
            // 
            // lblSoDienThoai
            // 
            lblSoDienThoai.Dock = DockStyle.Fill;
            lblSoDienThoai.Location = new Point(20, 314);
            lblSoDienThoai.Margin = new Padding(0);
            lblSoDienThoai.Name = "lblSoDienThoai";
            lblSoDienThoai.Size = new Size(511, 26);
            lblSoDienThoai.TabIndex = 5;
            lblSoDienThoai.Text = "Số điện thoại";
            lblSoDienThoai.TextAlign = ContentAlignment.BottomLeft;
            // 
            // txtSoDienThoai
            // 
            txtSoDienThoai.Dock = DockStyle.Fill;
            txtSoDienThoai.Location = new Point(20, 344);
            txtSoDienThoai.Margin = new Padding(0, 4, 0, 0);
            txtSoDienThoai.Name = "txtSoDienThoai";
            txtSoDienThoai.Size = new Size(511, 27);
            txtSoDienThoai.TabIndex = 4;
            // 
            // lblVaiTro
            // 
            lblVaiTro.Dock = DockStyle.Fill;
            lblVaiTro.Location = new Point(20, 378);
            lblVaiTro.Margin = new Padding(0);
            lblVaiTro.Name = "lblVaiTro";
            lblVaiTro.Size = new Size(511, 26);
            lblVaiTro.TabIndex = 6;
            lblVaiTro.Text = "Quyền";
            lblVaiTro.TextAlign = ContentAlignment.BottomLeft;
            // 
            // cboVaiTro
            // 
            cboVaiTro.Dock = DockStyle.Fill;
            cboVaiTro.DropDownStyle = ComboBoxStyle.DropDownList;
            cboVaiTro.FormattingEnabled = true;
            cboVaiTro.Items.AddRange(new object[] { "nhan_vien", "admin" });
            cboVaiTro.Location = new Point(20, 408);
            cboVaiTro.Margin = new Padding(0, 4, 0, 0);
            cboVaiTro.Name = "cboVaiTro";
            cboVaiTro.Size = new Size(511, 28);
            cboVaiTro.TabIndex = 5;
            // 
            // chkHoatDong
            // 
            chkHoatDong.AutoSize = true;
            chkHoatDong.Checked = true;
            chkHoatDong.CheckState = CheckState.Checked;
            chkHoatDong.Dock = DockStyle.Fill;
            chkHoatDong.Location = new Point(20, 442);
            chkHoatDong.Margin = new Padding(0);
            chkHoatDong.Name = "chkHoatDong";
            chkHoatDong.Size = new Size(511, 42);
            chkHoatDong.TabIndex = 6;
            chkHoatDong.Text = "Tài khoản hoạt động";
            chkHoatDong.UseVisualStyleBackColor = true;
            // 
            // pnlNutForm
            // 
            pnlNutForm.Controls.Add(btnLamMoiForm);
            pnlNutForm.Controls.Add(btnCapNhat);
            pnlNutForm.Controls.Add(btnTaoTaiKhoan);
            pnlNutForm.Dock = DockStyle.Fill;
            pnlNutForm.Location = new Point(20, 484);
            pnlNutForm.Margin = new Padding(0);
            pnlNutForm.Name = "pnlNutForm";
            pnlNutForm.Size = new Size(511, 139);
            pnlNutForm.TabIndex = 7;
            // 
            // btnLamMoiForm
            // 
            btnLamMoiForm.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLamMoiForm.Cursor = Cursors.Hand;
            btnLamMoiForm.FlatStyle = FlatStyle.Flat;
            btnLamMoiForm.Location = new Point(125, 12);
            btnLamMoiForm.Name = "btnLamMoiForm";
            btnLamMoiForm.Size = new Size(120, 34);
            btnLamMoiForm.TabIndex = 2;
            btnLamMoiForm.Text = "Làm mới";
            btnLamMoiForm.UseVisualStyleBackColor = true;
            // 
            // btnCapNhat
            // 
            btnCapNhat.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCapNhat.BackColor = Color.FromArgb(40, 167, 69);
            btnCapNhat.Cursor = Cursors.Hand;
            btnCapNhat.Enabled = false;
            btnCapNhat.FlatAppearance.BorderSize = 0;
            btnCapNhat.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 130, 54);
            btnCapNhat.FlatAppearance.MouseOverBackColor = Color.FromArgb(48, 185, 81);
            btnCapNhat.FlatStyle = FlatStyle.Flat;
            btnCapNhat.ForeColor = Color.White;
            btnCapNhat.Location = new Point(255, 12);
            btnCapNhat.Name = "btnCapNhat";
            btnCapNhat.Size = new Size(120, 34);
            btnCapNhat.TabIndex = 1;
            btnCapNhat.Text = "Cập nhật";
            btnCapNhat.UseVisualStyleBackColor = false;
            // 
            // btnTaoTaiKhoan
            // 
            btnTaoTaiKhoan.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTaoTaiKhoan.BackColor = Color.FromArgb(63, 81, 181);
            btnTaoTaiKhoan.Cursor = Cursors.Hand;
            btnTaoTaiKhoan.FlatAppearance.BorderSize = 0;
            btnTaoTaiKhoan.FlatAppearance.MouseDownBackColor = Color.FromArgb(49, 63, 160);
            btnTaoTaiKhoan.FlatAppearance.MouseOverBackColor = Color.FromArgb(79, 99, 215);
            btnTaoTaiKhoan.FlatStyle = FlatStyle.Flat;
            btnTaoTaiKhoan.ForeColor = Color.White;
            btnTaoTaiKhoan.Location = new Point(385, 12);
            btnTaoTaiKhoan.Name = "btnTaoTaiKhoan";
            btnTaoTaiKhoan.Size = new Size(126, 34);
            btnTaoTaiKhoan.TabIndex = 0;
            btnTaoTaiKhoan.Text = "Tạo tài khoản";
            btnTaoTaiKhoan.UseVisualStyleBackColor = false;
            // 
            // pnlToolbar
            // 
            pnlToolbar.BackColor = Color.White;
            pnlToolbar.BorderStyle = BorderStyle.FixedSingle;
            pnlToolbar.Controls.Add(lblTimKiem);
            pnlToolbar.Controls.Add(txtTimKiem);
            pnlToolbar.Controls.Add(btnTimKiem);
            pnlToolbar.Controls.Add(btnDatLaiTimKiem);
            pnlToolbar.Controls.Add(lblTrangThai);
            pnlToolbar.Controls.Add(btnTaiLai);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(20, 72);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Padding = new Padding(16, 8, 16, 8);
            pnlToolbar.Size = new Size(1624, 56);
            pnlToolbar.TabIndex = 1;
            // 
            // lblTimKiem
            // 
            lblTimKiem.AutoSize = true;
            lblTimKiem.Location = new Point(16, 17);
            lblTimKiem.Name = "lblTimKiem";
            lblTimKiem.Size = new Size(73, 20);
            lblTimKiem.TabIndex = 4;
            lblTimKiem.Text = "Tìm kiếm:";
            // 
            // txtTimKiem
            // 
            txtTimKiem.Location = new Point(96, 14);
            txtTimKiem.Name = "txtTimKiem";
            txtTimKiem.PlaceholderText = "Tên đăng nhập, họ tên, email, SĐT, quyền...";
            txtTimKiem.Size = new Size(440, 27);
            txtTimKiem.TabIndex = 3;
            // 
            // btnTimKiem
            // 
            btnTimKiem.BackColor = Color.FromArgb(63, 81, 181);
            btnTimKiem.Cursor = Cursors.Hand;
            btnTimKiem.FlatAppearance.BorderSize = 0;
            btnTimKiem.FlatAppearance.MouseDownBackColor = Color.FromArgb(49, 63, 160);
            btnTimKiem.FlatAppearance.MouseOverBackColor = Color.FromArgb(79, 99, 215);
            btnTimKiem.FlatStyle = FlatStyle.Flat;
            btnTimKiem.ForeColor = Color.White;
            btnTimKiem.Location = new Point(548, 12);
            btnTimKiem.Name = "btnTimKiem";
            btnTimKiem.Size = new Size(92, 30);
            btnTimKiem.TabIndex = 2;
            btnTimKiem.Text = "Lọc";
            btnTimKiem.UseVisualStyleBackColor = false;
            // 
            // btnDatLaiTimKiem
            // 
            btnDatLaiTimKiem.Cursor = Cursors.Hand;
            btnDatLaiTimKiem.FlatStyle = FlatStyle.Flat;
            btnDatLaiTimKiem.Location = new Point(648, 12);
            btnDatLaiTimKiem.Name = "btnDatLaiTimKiem";
            btnDatLaiTimKiem.Size = new Size(92, 30);
            btnDatLaiTimKiem.TabIndex = 5;
            btnDatLaiTimKiem.Text = "Đặt lại";
            btnDatLaiTimKiem.UseVisualStyleBackColor = true;
            // 
            // lblTrangThai
            // 
            lblTrangThai.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblTrangThai.AutoEllipsis = true;
            lblTrangThai.Location = new Point(760, 13);
            lblTrangThai.Name = "lblTrangThai";
            lblTrangThai.Size = new Size(730, 30);
            lblTrangThai.TabIndex = 1;
            lblTrangThai.Text = "Sẵn sàng.";
            lblTrangThai.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnTaiLai
            // 
            btnTaiLai.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTaiLai.BackColor = Color.FromArgb(63, 81, 181);
            btnTaiLai.Cursor = Cursors.Hand;
            btnTaiLai.FlatAppearance.BorderSize = 0;
            btnTaiLai.FlatAppearance.MouseDownBackColor = Color.FromArgb(49, 63, 160);
            btnTaiLai.FlatAppearance.MouseOverBackColor = Color.FromArgb(79, 99, 215);
            btnTaiLai.FlatStyle = FlatStyle.Flat;
            btnTaiLai.ForeColor = Color.White;
            btnTaiLai.Location = new Point(1508, 12);
            btnTaiLai.Name = "btnTaiLai";
            btnTaiLai.Size = new Size(100, 30);
            btnTaiLai.TabIndex = 0;
            btnTaiLai.Text = "Tải lại";
            btnTaiLai.UseVisualStyleBackColor = false;
            // 
            // pnlDauTrang
            // 
            pnlDauTrang.Controls.Add(lblTieuDe);
            pnlDauTrang.Controls.Add(lblMoTa);
            pnlDauTrang.Dock = DockStyle.Top;
            pnlDauTrang.Location = new Point(20, 20);
            pnlDauTrang.Name = "pnlDauTrang";
            pnlDauTrang.Size = new Size(1624, 52);
            pnlDauTrang.TabIndex = 0;
            // 
            // lblTieuDe
            // 
            lblTieuDe.AutoSize = true;
            lblTieuDe.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTieuDe.Location = new Point(0, 0);
            lblTieuDe.Name = "lblTieuDe";
            lblTieuDe.Size = new Size(245, 41);
            lblTieuDe.TabIndex = 0;
            lblTieuDe.Text = "Quản lý nhân sự";
            // 
            // lblMoTa
            // 
            lblMoTa.AutoSize = true;
            lblMoTa.ForeColor = Color.FromArgb(85, 96, 108);
            lblMoTa.Location = new Point(276, 17);
            lblMoTa.Name = "lblMoTa";
            lblMoTa.Size = new Size(323, 20);
            lblMoTa.TabIndex = 1;
            lblMoTa.Text = "Tạo tài khoản phần mềm cho nhân viên đội test";
            // 
            // QuanLyNhanSu
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlRoot);
            Name = "QuanLyNhanSu";
            Size = new Size(1664, 793);
            pnlRoot.ResumeLayout(false);
            pnlNoiDung.ResumeLayout(false);
            tblNoiDung.ResumeLayout(false);
            pnlDanhSach.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvNhanSu).EndInit();
            pnlForm.ResumeLayout(false);
            tblForm.ResumeLayout(false);
            tblForm.PerformLayout();
            pnlNutForm.ResumeLayout(false);
            pnlToolbar.ResumeLayout(false);
            pnlToolbar.PerformLayout();
            pnlDauTrang.ResumeLayout(false);
            pnlDauTrang.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlRoot;
        private Panel pnlNoiDung;
        private TableLayoutPanel tblNoiDung;
        private Panel pnlDanhSach;
        private DataGridView dgvNhanSu;
        private Panel pnlForm;
        private TableLayoutPanel tblForm;
        private Label lblDangChon;
        private Label lblTenDangNhap;
        private TextBox txtTenDangNhap;
        private Label lblMatKhau;
        private TextBox txtMatKhau;
        private Label lblHoTen;
        private TextBox txtHoTen;
        private Label lblEmail;
        private TextBox txtEmail;
        private Label lblSoDienThoai;
        private TextBox txtSoDienThoai;
        private Label lblVaiTro;
        private ComboBox cboVaiTro;
        private CheckBox chkHoatDong;
        private Panel pnlNutForm;
        private Button btnLamMoiForm;
        private Button btnCapNhat;
        private Button btnTaoTaiKhoan;
        private Panel pnlToolbar;
        private Label lblTimKiem;
        private TextBox txtTimKiem;
        private Button btnTimKiem;
        private Button btnDatLaiTimKiem;
        private Label lblTrangThai;
        private Button btnTaiLai;
        private Panel pnlDauTrang;
        private Label lblTieuDe;
        private Label lblMoTa;
    }
}
