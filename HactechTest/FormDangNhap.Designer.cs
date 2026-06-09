namespace HactechTest
{
    partial class FormDangNhap
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pnlRoot = new Panel();
            pnlCard = new Panel();
            tblCard = new TableLayoutPanel();
            lblTieuDe = new Label();
            lblMoTa = new Label();
            lblTenDangNhap = new Label();
            txtTenDangNhap = new TextBox();
            lblMatKhau = new Label();
            txtMatKhau = new TextBox();
            lblThongBao = new Label();
            pnlButtons = new Panel();
            btnCauHinhDb = new Button();
            btnThoat = new Button();
            btnDangNhap = new Button();
            pictureBox1 = new PictureBox();
            pnlRoot.SuspendLayout();
            pnlCard.SuspendLayout();
            tblCard.SuspendLayout();
            pnlButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pnlRoot
            // 
            pnlRoot.BackColor = Color.FromArgb(244, 248, 252);
            pnlRoot.BackgroundImage = Properties.Resources._0a60f2b2_68ce_48d1_9f27_151dfc33f644;
            pnlRoot.BackgroundImageLayout = ImageLayout.Stretch;
            pnlRoot.Controls.Add(pnlCard);
            pnlRoot.Dock = DockStyle.Fill;
            pnlRoot.Location = new Point(0, 0);
            pnlRoot.Name = "pnlRoot";
            pnlRoot.Padding = new Padding(64);
            pnlRoot.Size = new Size(1000, 590);
            pnlRoot.TabIndex = 0;
            // 
            // pnlCard
            // 
            pnlCard.Anchor = AnchorStyles.Left;
            pnlCard.BackColor = Color.White;
            pnlCard.BorderStyle = BorderStyle.FixedSingle;
            pnlCard.Controls.Add(tblCard);
            pnlCard.Location = new Point(72, 58);
            pnlCard.Name = "pnlCard";
            pnlCard.Size = new Size(438, 474);
            pnlCard.TabIndex = 0;
            // 
            // tblCard
            // 
            tblCard.ColumnCount = 1;
            tblCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblCard.Controls.Add(lblTieuDe, 0, 1);
            tblCard.Controls.Add(lblMoTa, 0, 2);
            tblCard.Controls.Add(lblTenDangNhap, 0, 3);
            tblCard.Controls.Add(txtTenDangNhap, 0, 4);
            tblCard.Controls.Add(lblMatKhau, 0, 5);
            tblCard.Controls.Add(txtMatKhau, 0, 6);
            tblCard.Controls.Add(lblThongBao, 0, 7);
            tblCard.Controls.Add(pnlButtons, 0, 8);
            tblCard.Controls.Add(pictureBox1, 0, 0);
            tblCard.Dock = DockStyle.Fill;
            tblCard.Location = new Point(0, 0);
            tblCard.Name = "tblCard";
            tblCard.Padding = new Padding(34, 30, 34, 26);
            tblCard.RowCount = 9;
            tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
            tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tblCard.Size = new Size(436, 472);
            tblCard.TabIndex = 0;
            // 
            // lblTieuDe
            // 
            lblTieuDe.Dock = DockStyle.Fill;
            lblTieuDe.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTieuDe.ForeColor = Color.FromArgb(26, 38, 52);
            lblTieuDe.Location = new Point(34, 122);
            lblTieuDe.Margin = new Padding(0);
            lblTieuDe.Name = "lblTieuDe";
            lblTieuDe.Size = new Size(368, 50);
            lblTieuDe.TabIndex = 1;
            lblTieuDe.Text = "Đăng nhập";
            lblTieuDe.TextAlign = ContentAlignment.BottomLeft;
            // 
            // lblMoTa
            // 
            lblMoTa.Dock = DockStyle.Fill;
            lblMoTa.ForeColor = Color.FromArgb(86, 99, 113);
            lblMoTa.Location = new Point(34, 172);
            lblMoTa.Margin = new Padding(0);
            lblMoTa.Name = "lblMoTa";
            lblMoTa.Size = new Size(368, 46);
            lblMoTa.TabIndex = 2;
            lblMoTa.Text = "Đăng nhập bằng tài khoản nhân sự đội test để bắt đầu kiểm thử API.";
            // 
            // lblTenDangNhap
            // 
            lblTenDangNhap.Dock = DockStyle.Fill;
            lblTenDangNhap.Font = new Font("Segoe UI", 9.5F);
            lblTenDangNhap.ForeColor = Color.FromArgb(26, 38, 52);
            lblTenDangNhap.Location = new Point(34, 218);
            lblTenDangNhap.Margin = new Padding(0);
            lblTenDangNhap.Name = "lblTenDangNhap";
            lblTenDangNhap.Size = new Size(368, 30);
            lblTenDangNhap.TabIndex = 3;
            lblTenDangNhap.Text = "Tên đăng nhập";
            lblTenDangNhap.TextAlign = ContentAlignment.BottomLeft;
            // 
            // txtTenDangNhap
            // 
            txtTenDangNhap.Dock = DockStyle.Fill;
            txtTenDangNhap.Font = new Font("Segoe UI", 10F);
            txtTenDangNhap.Location = new Point(34, 252);
            txtTenDangNhap.Margin = new Padding(0, 4, 0, 0);
            txtTenDangNhap.Name = "txtTenDangNhap";
            txtTenDangNhap.Size = new Size(368, 30);
            txtTenDangNhap.TabIndex = 0;
            // 
            // lblMatKhau
            // 
            lblMatKhau.Dock = DockStyle.Fill;
            lblMatKhau.Font = new Font("Segoe UI", 9.5F);
            lblMatKhau.ForeColor = Color.FromArgb(26, 38, 52);
            lblMatKhau.Location = new Point(34, 290);
            lblMatKhau.Margin = new Padding(0);
            lblMatKhau.Name = "lblMatKhau";
            lblMatKhau.Size = new Size(368, 30);
            lblMatKhau.TabIndex = 4;
            lblMatKhau.Text = "Mật khẩu";
            lblMatKhau.TextAlign = ContentAlignment.BottomLeft;
            // 
            // txtMatKhau
            // 
            txtMatKhau.Dock = DockStyle.Fill;
            txtMatKhau.Font = new Font("Segoe UI", 10F);
            txtMatKhau.Location = new Point(34, 324);
            txtMatKhau.Margin = new Padding(0, 4, 0, 0);
            txtMatKhau.Name = "txtMatKhau";
            txtMatKhau.PasswordChar = '●';
            txtMatKhau.Size = new Size(368, 30);
            txtMatKhau.TabIndex = 1;
            // 
            // lblThongBao
            // 
            lblThongBao.Dock = DockStyle.Fill;
            lblThongBao.ForeColor = Color.FromArgb(220, 53, 69);
            lblThongBao.Location = new Point(34, 362);
            lblThongBao.Margin = new Padding(0);
            lblThongBao.Name = "lblThongBao";
            lblThongBao.Size = new Size(368, 50);
            lblThongBao.TabIndex = 5;
            lblThongBao.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnCauHinhDb);
            pnlButtons.Controls.Add(btnThoat);
            pnlButtons.Controls.Add(btnDangNhap);
            pnlButtons.Dock = DockStyle.Fill;
            pnlButtons.Location = new Point(34, 412);
            pnlButtons.Margin = new Padding(0);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(368, 56);
            pnlButtons.TabIndex = 6;
            // 
            // btnCauHinhDb
            // 
            btnCauHinhDb.BackColor = Color.White;
            btnCauHinhDb.Cursor = Cursors.Hand;
            btnCauHinhDb.FlatAppearance.BorderColor = Color.FromArgb(183, 193, 204);
            btnCauHinhDb.FlatAppearance.MouseDownBackColor = Color.FromArgb(230, 236, 242);
            btnCauHinhDb.FlatAppearance.MouseOverBackColor = Color.FromArgb(244, 247, 251);
            btnCauHinhDb.FlatStyle = FlatStyle.Flat;
            btnCauHinhDb.ForeColor = Color.FromArgb(26, 38, 52);
            btnCauHinhDb.Location = new Point(0, 10);
            btnCauHinhDb.Name = "btnCauHinhDb";
            btnCauHinhDb.Size = new Size(122, 36);
            btnCauHinhDb.TabIndex = 2;
            btnCauHinhDb.Text = "Cấu hình DB";
            btnCauHinhDb.UseVisualStyleBackColor = false;
            // 
            // btnThoat
            // 
            btnThoat.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnThoat.BackColor = Color.White;
            btnThoat.Cursor = Cursors.Hand;
            btnThoat.FlatAppearance.BorderColor = Color.FromArgb(183, 193, 204);
            btnThoat.FlatAppearance.MouseDownBackColor = Color.FromArgb(230, 236, 242);
            btnThoat.FlatAppearance.MouseOverBackColor = Color.FromArgb(244, 247, 251);
            btnThoat.FlatStyle = FlatStyle.Flat;
            btnThoat.ForeColor = Color.FromArgb(26, 38, 52);
            btnThoat.Location = new Point(138, 10);
            btnThoat.Name = "btnThoat";
            btnThoat.Size = new Size(96, 36);
            btnThoat.TabIndex = 3;
            btnThoat.Text = "Thoát";
            btnThoat.UseVisualStyleBackColor = false;
            // 
            // btnDangNhap
            // 
            btnDangNhap.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDangNhap.BackColor = Color.FromArgb(228, 30, 45);
            btnDangNhap.Cursor = Cursors.Hand;
            btnDangNhap.FlatAppearance.BorderSize = 0;
            btnDangNhap.FlatAppearance.MouseDownBackColor = Color.FromArgb(185, 20, 34);
            btnDangNhap.FlatAppearance.MouseOverBackColor = Color.FromArgb(242, 54, 67);
            btnDangNhap.FlatStyle = FlatStyle.Flat;
            btnDangNhap.ForeColor = Color.White;
            btnDangNhap.Location = new Point(246, 10);
            btnDangNhap.Name = "btnDangNhap";
            btnDangNhap.Size = new Size(122, 36);
            btnDangNhap.TabIndex = 4;
            btnDangNhap.Text = "Đăng nhập";
            btnDangNhap.UseVisualStyleBackColor = false;
            // 
            // pictureBox1
            // 
            pictureBox1.BackgroundImage = Properties.Resources.logo_hactechtest_crop;
            pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox1.Location = new Point(37, 33);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(322, 86);
            pictureBox1.TabIndex = 7;
            pictureBox1.TabStop = false;
            // 
            // FormDangNhap
            // 
            AcceptButton = btnDangNhap;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnThoat;
            ClientSize = new Size(1000, 590);
            Controls.Add(pnlRoot);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormDangNhap";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "HACTECH TEST - Đăng nhập";
            pnlRoot.ResumeLayout(false);
            pnlCard.ResumeLayout(false);
            tblCard.ResumeLayout(false);
            tblCard.PerformLayout();
            pnlButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlRoot;
        private Panel pnlCard;
        private TableLayoutPanel tblCard;
        private Label lblTieuDe;
        private Label lblMoTa;
        private Label lblTenDangNhap;
        private TextBox txtTenDangNhap;
        private Label lblMatKhau;
        private TextBox txtMatKhau;
        private Label lblThongBao;
        private Panel pnlButtons;
        private Button btnCauHinhDb;
        private Button btnThoat;
        private Button btnDangNhap;
        private PictureBox pictureBox1;
    }
}
