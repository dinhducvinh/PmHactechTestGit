namespace HactechTest
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pnlSidebar = new Panel();
            pictureBox1 = new PictureBox();
            btnQuanLyNhanSu = new Button();
            btnLichSu = new Button();
            btnBoSuuTap = new Button();
            btnChayTest = new Button();
            btnTongQuan = new Button();
            pnlVienActive = new Panel();
            btnDangXuat = new Button();
            btnCaiDatTaiKhoan = new Button();
            lblSubtitle = new Label();
            pnlNoiDungChinh = new Panel();
            quanLyNhanSu1 = new HactechTest.Control.QuanLyNhanSu();
            lichSu1 = new HactechTest.Control.LichSu();
            chayTest1 = new HactechTest.Control.ChayTest();
            boSuuTap1 = new HactechTest.Control.BoSuuTap();
            tongQuan1 = new HactechTest.Control.TongQuan();
            pnlSidebar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            pnlNoiDungChinh.SuspendLayout();
            SuspendLayout();
            // 
            // pnlSidebar
            // 
            pnlSidebar.BackColor = Color.FromArgb(24, 34, 46);
            pnlSidebar.Controls.Add(pictureBox1);
            pnlSidebar.Controls.Add(btnQuanLyNhanSu);
            pnlSidebar.Controls.Add(btnLichSu);
            pnlSidebar.Controls.Add(btnBoSuuTap);
            pnlSidebar.Controls.Add(btnChayTest);
            pnlSidebar.Controls.Add(btnTongQuan);
            pnlSidebar.Controls.Add(pnlVienActive);
            pnlSidebar.Controls.Add(btnDangXuat);
            pnlSidebar.Controls.Add(btnCaiDatTaiKhoan);
            pnlSidebar.Controls.Add(lblSubtitle);
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Location = new Point(0, 0);
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Size = new Size(248, 793);
            pnlSidebar.TabIndex = 0;
            // 
            // pictureBox1
            // 
            pictureBox1.BackgroundImage = Properties.Resources.logo_hactechtest_crop;
            pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox1.Location = new Point(24, 30);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(192, 58);
            pictureBox1.TabIndex = 12;
            pictureBox1.TabStop = false;
            // 
            // btnQuanLyNhanSu
            // 
            btnQuanLyNhanSu.BackColor = Color.FromArgb(35, 49, 64);
            btnQuanLyNhanSu.Cursor = Cursors.Hand;
            btnQuanLyNhanSu.FlatAppearance.BorderSize = 0;
            btnQuanLyNhanSu.FlatAppearance.MouseDownBackColor = Color.FromArgb(62, 84, 105);
            btnQuanLyNhanSu.FlatAppearance.MouseOverBackColor = Color.FromArgb(49, 65, 82);
            btnQuanLyNhanSu.FlatStyle = FlatStyle.Flat;
            btnQuanLyNhanSu.Font = new Font("Segoe UI", 10F);
            btnQuanLyNhanSu.ForeColor = Color.WhiteSmoke;
            btnQuanLyNhanSu.Location = new Point(0, 364);
            btnQuanLyNhanSu.Name = "btnQuanLyNhanSu";
            btnQuanLyNhanSu.Padding = new Padding(24, 0, 0, 0);
            btnQuanLyNhanSu.Size = new Size(248, 48);
            btnQuanLyNhanSu.TabIndex = 9;
            btnQuanLyNhanSu.Text = "Nhân sự";
            btnQuanLyNhanSu.TextAlign = ContentAlignment.MiddleLeft;
            btnQuanLyNhanSu.UseVisualStyleBackColor = false;
            // 
            // btnLichSu
            // 
            btnLichSu.BackColor = Color.FromArgb(35, 49, 64);
            btnLichSu.Cursor = Cursors.Hand;
            btnLichSu.FlatAppearance.BorderSize = 0;
            btnLichSu.FlatAppearance.MouseDownBackColor = Color.FromArgb(62, 84, 105);
            btnLichSu.FlatAppearance.MouseOverBackColor = Color.FromArgb(49, 65, 82);
            btnLichSu.FlatStyle = FlatStyle.Flat;
            btnLichSu.Font = new Font("Segoe UI", 10F);
            btnLichSu.ForeColor = Color.WhiteSmoke;
            btnLichSu.Location = new Point(0, 316);
            btnLichSu.Name = "btnLichSu";
            btnLichSu.Padding = new Padding(24, 0, 0, 0);
            btnLichSu.Size = new Size(248, 48);
            btnLichSu.TabIndex = 8;
            btnLichSu.Text = "Lịch sử";
            btnLichSu.TextAlign = ContentAlignment.MiddleLeft;
            btnLichSu.UseVisualStyleBackColor = false;
            // 
            // btnBoSuuTap
            // 
            btnBoSuuTap.BackColor = Color.FromArgb(35, 49, 64);
            btnBoSuuTap.Cursor = Cursors.Hand;
            btnBoSuuTap.FlatAppearance.BorderSize = 0;
            btnBoSuuTap.FlatAppearance.MouseDownBackColor = Color.FromArgb(62, 84, 105);
            btnBoSuuTap.FlatAppearance.MouseOverBackColor = Color.FromArgb(49, 65, 82);
            btnBoSuuTap.FlatStyle = FlatStyle.Flat;
            btnBoSuuTap.Font = new Font("Segoe UI", 10F);
            btnBoSuuTap.ForeColor = Color.WhiteSmoke;
            btnBoSuuTap.Location = new Point(0, 268);
            btnBoSuuTap.Name = "btnBoSuuTap";
            btnBoSuuTap.Padding = new Padding(24, 0, 0, 0);
            btnBoSuuTap.Size = new Size(248, 48);
            btnBoSuuTap.TabIndex = 6;
            btnBoSuuTap.Text = "Bộ sưu tập";
            btnBoSuuTap.TextAlign = ContentAlignment.MiddleLeft;
            btnBoSuuTap.UseVisualStyleBackColor = false;
            // 
            // btnChayTest
            // 
            btnChayTest.BackColor = Color.FromArgb(35, 49, 64);
            btnChayTest.Cursor = Cursors.Hand;
            btnChayTest.FlatAppearance.BorderSize = 0;
            btnChayTest.FlatAppearance.MouseDownBackColor = Color.FromArgb(62, 84, 105);
            btnChayTest.FlatAppearance.MouseOverBackColor = Color.FromArgb(49, 65, 82);
            btnChayTest.FlatStyle = FlatStyle.Flat;
            btnChayTest.Font = new Font("Segoe UI", 10F);
            btnChayTest.ForeColor = Color.WhiteSmoke;
            btnChayTest.Location = new Point(0, 220);
            btnChayTest.Name = "btnChayTest";
            btnChayTest.Padding = new Padding(24, 0, 0, 0);
            btnChayTest.Size = new Size(248, 48);
            btnChayTest.TabIndex = 5;
            btnChayTest.Text = "Chạy test";
            btnChayTest.TextAlign = ContentAlignment.MiddleLeft;
            btnChayTest.UseVisualStyleBackColor = false;
            // 
            // btnTongQuan
            // 
            btnTongQuan.BackColor = Color.FromArgb(49, 65, 82);
            btnTongQuan.Cursor = Cursors.Hand;
            btnTongQuan.FlatAppearance.BorderSize = 0;
            btnTongQuan.FlatAppearance.MouseDownBackColor = Color.FromArgb(62, 84, 105);
            btnTongQuan.FlatAppearance.MouseOverBackColor = Color.FromArgb(49, 65, 82);
            btnTongQuan.FlatStyle = FlatStyle.Flat;
            btnTongQuan.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnTongQuan.ForeColor = Color.WhiteSmoke;
            btnTongQuan.Location = new Point(0, 172);
            btnTongQuan.Name = "btnTongQuan";
            btnTongQuan.Padding = new Padding(24, 0, 0, 0);
            btnTongQuan.Size = new Size(248, 48);
            btnTongQuan.TabIndex = 3;
            btnTongQuan.Text = "Tổng quan";
            btnTongQuan.TextAlign = ContentAlignment.MiddleLeft;
            btnTongQuan.UseVisualStyleBackColor = false;
            // 
            // pnlVienActive
            // 
            pnlVienActive.BackColor = Color.FromArgb(79, 99, 215);
            pnlVienActive.Location = new Point(0, 172);
            pnlVienActive.Name = "pnlVienActive";
            pnlVienActive.Size = new Size(4, 48);
            pnlVienActive.TabIndex = 2;
            // 
            // btnDangXuat
            // 
            btnDangXuat.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDangXuat.BackColor = Color.FromArgb(24, 34, 46);
            btnDangXuat.Cursor = Cursors.Hand;
            btnDangXuat.FlatAppearance.BorderColor = Color.FromArgb(85, 96, 108);
            btnDangXuat.FlatAppearance.MouseDownBackColor = Color.FromArgb(62, 84, 105);
            btnDangXuat.FlatAppearance.MouseOverBackColor = Color.FromArgb(35, 49, 64);
            btnDangXuat.FlatStyle = FlatStyle.Flat;
            btnDangXuat.ForeColor = Color.FromArgb(235, 238, 242);
            btnDangXuat.Location = new Point(24, 740);
            btnDangXuat.Name = "btnDangXuat";
            btnDangXuat.Size = new Size(200, 34);
            btnDangXuat.TabIndex = 11;
            btnDangXuat.Text = "Đăng xuất";
            btnDangXuat.UseVisualStyleBackColor = false;
            // 
            // btnCaiDatTaiKhoan
            // 
            btnCaiDatTaiKhoan.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCaiDatTaiKhoan.BackColor = Color.FromArgb(24, 34, 46);
            btnCaiDatTaiKhoan.Cursor = Cursors.Hand;
            btnCaiDatTaiKhoan.FlatAppearance.BorderColor = Color.FromArgb(85, 96, 108);
            btnCaiDatTaiKhoan.FlatAppearance.MouseDownBackColor = Color.FromArgb(62, 84, 105);
            btnCaiDatTaiKhoan.FlatAppearance.MouseOverBackColor = Color.FromArgb(35, 49, 64);
            btnCaiDatTaiKhoan.FlatStyle = FlatStyle.Flat;
            btnCaiDatTaiKhoan.ForeColor = Color.FromArgb(235, 238, 242);
            btnCaiDatTaiKhoan.Location = new Point(24, 696);
            btnCaiDatTaiKhoan.Name = "btnCaiDatTaiKhoan";
            btnCaiDatTaiKhoan.Padding = new Padding(10, 0, 0, 0);
            btnCaiDatTaiKhoan.Size = new Size(200, 38);
            btnCaiDatTaiKhoan.TabIndex = 10;
            btnCaiDatTaiKhoan.Text = "⚙  Tài khoản";
            btnCaiDatTaiKhoan.TextAlign = ContentAlignment.MiddleLeft;
            btnCaiDatTaiKhoan.UseVisualStyleBackColor = false;
            // 
            // lblSubtitle
            // 
            lblSubtitle.ForeColor = Color.FromArgb(181, 190, 199);
            lblSubtitle.Location = new Point(24, 116);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(192, 42);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Kiểm thử API, chạy test và theo dõi kết quả";
            // 
            // pnlNoiDungChinh
            // 
            pnlNoiDungChinh.BackColor = Color.FromArgb(244, 245, 247);
            pnlNoiDungChinh.Controls.Add(quanLyNhanSu1);
            pnlNoiDungChinh.Controls.Add(lichSu1);
            pnlNoiDungChinh.Controls.Add(chayTest1);
            pnlNoiDungChinh.Controls.Add(boSuuTap1);
            pnlNoiDungChinh.Controls.Add(tongQuan1);
            pnlNoiDungChinh.Dock = DockStyle.Fill;
            pnlNoiDungChinh.Location = new Point(248, 0);
            pnlNoiDungChinh.Name = "pnlNoiDungChinh";
            pnlNoiDungChinh.Size = new Size(1676, 793);
            pnlNoiDungChinh.TabIndex = 1;
            // 
            // quanLyNhanSu1
            // 
            quanLyNhanSu1.Dock = DockStyle.Fill;
            quanLyNhanSu1.Location = new Point(0, 0);
            quanLyNhanSu1.Name = "quanLyNhanSu1";
            quanLyNhanSu1.Size = new Size(1676, 793);
            quanLyNhanSu1.TabIndex = 6;
            // 
            // lichSu1
            // 
            lichSu1.Dock = DockStyle.Fill;
            lichSu1.Location = new Point(0, 0);
            lichSu1.Name = "lichSu1";
            lichSu1.Size = new Size(1676, 793);
            lichSu1.TabIndex = 5;
            // 
            // chayTest1
            // 
            chayTest1.Dock = DockStyle.Fill;
            chayTest1.Location = new Point(0, 0);
            chayTest1.Name = "chayTest1";
            chayTest1.Size = new Size(1676, 793);
            chayTest1.TabIndex = 4;
            // 
            // boSuuTap1
            // 
            boSuuTap1.Dock = DockStyle.Fill;
            boSuuTap1.Location = new Point(0, 0);
            boSuuTap1.Name = "boSuuTap1";
            boSuuTap1.Size = new Size(1676, 793);
            boSuuTap1.TabIndex = 3;
            // 
            // tongQuan1
            // 
            tongQuan1.Dock = DockStyle.Fill;
            tongQuan1.Location = new Point(0, 0);
            tongQuan1.Name = "tongQuan1";
            tongQuan1.Size = new Size(1676, 793);
            tongQuan1.TabIndex = 0;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1924, 793);
            Controls.Add(pnlNoiDungChinh);
            Controls.Add(pnlSidebar);
            Name = "FormMain";
            Text = "HACTECH TEST";
            WindowState = FormWindowState.Maximized;
            pnlSidebar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            pnlNoiDungChinh.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlSidebar;
        private Button btnQuanLyNhanSu;
        private Button btnLichSu;
        private Button btnBoSuuTap;
        private Button btnChayTest;
        private Button btnTongQuan;
        private Panel pnlVienActive;
        private Button btnDangXuat;
        private Button btnCaiDatTaiKhoan;
        private Label lblSubtitle;
        private Panel pnlNoiDungChinh;
        private Control.TongQuan tongQuan1;
        private Control.LichSu lichSu1;
        private Control.ChayTest chayTest1;
        private Control.BoSuuTap boSuuTap1;
        private Control.QuanLyNhanSu quanLyNhanSu1;
        private PictureBox pictureBox1;
    }
}

