namespace HactechTest
{
    partial class FormDoiMatKhau
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
            lblTaiKhoan = new Label();
            lblMatKhauCu = new Label();
            txtMatKhauCu = new TextBox();
            lblMatKhauMoi = new Label();
            txtMatKhauMoi = new TextBox();
            lblXacNhanMatKhau = new Label();
            txtXacNhanMatKhau = new TextBox();
            lblThongBao = new Label();
            pnlButtons = new Panel();
            btnHuy = new Button();
            btnLuu = new Button();
            pnlRoot.SuspendLayout();
            pnlCard.SuspendLayout();
            tblCard.SuspendLayout();
            pnlButtons.SuspendLayout();
            SuspendLayout();
            // 
            // pnlRoot
            // 
            pnlRoot.BackColor = Color.FromArgb(244, 245, 247);
            pnlRoot.Controls.Add(pnlCard);
            pnlRoot.Dock = DockStyle.Fill;
            pnlRoot.Location = new Point(0, 0);
            pnlRoot.Name = "pnlRoot";
            pnlRoot.Padding = new Padding(28);
            pnlRoot.Size = new Size(520, 424);
            pnlRoot.TabIndex = 0;
            // 
            // pnlCard
            // 
            pnlCard.Anchor = AnchorStyles.None;
            pnlCard.BackColor = Color.White;
            pnlCard.BorderStyle = BorderStyle.FixedSingle;
            pnlCard.Controls.Add(tblCard);
            pnlCard.Location = new Point(36, 28);
            pnlCard.Name = "pnlCard";
            pnlCard.Size = new Size(448, 368);
            pnlCard.TabIndex = 0;
            // 
            // tblCard
            // 
            tblCard.ColumnCount = 1;
            tblCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblCard.Controls.Add(lblTieuDe, 0, 0);
            tblCard.Controls.Add(lblTaiKhoan, 0, 1);
            tblCard.Controls.Add(lblMatKhauCu, 0, 2);
            tblCard.Controls.Add(txtMatKhauCu, 0, 3);
            tblCard.Controls.Add(lblMatKhauMoi, 0, 4);
            tblCard.Controls.Add(txtMatKhauMoi, 0, 5);
            tblCard.Controls.Add(lblXacNhanMatKhau, 0, 6);
            tblCard.Controls.Add(txtXacNhanMatKhau, 0, 7);
            tblCard.Controls.Add(lblThongBao, 0, 8);
            tblCard.Controls.Add(pnlButtons, 0, 9);
            tblCard.Dock = DockStyle.Fill;
            tblCard.Location = new Point(0, 0);
            tblCard.Name = "tblCard";
            tblCard.Padding = new Padding(28, 24, 28, 20);
            tblCard.RowCount = 10;
            tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            tblCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            tblCard.Size = new Size(446, 366);
            tblCard.TabIndex = 0;
            // 
            // lblTieuDe
            // 
            lblTieuDe.Dock = DockStyle.Fill;
            lblTieuDe.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTieuDe.ForeColor = Color.FromArgb(31, 43, 56);
            lblTieuDe.Location = new Point(28, 24);
            lblTieuDe.Margin = new Padding(0);
            lblTieuDe.Name = "lblTieuDe";
            lblTieuDe.Size = new Size(390, 42);
            lblTieuDe.TabIndex = 0;
            lblTieuDe.Text = "Đổi mật khẩu";
            // 
            // lblTaiKhoan
            // 
            lblTaiKhoan.Dock = DockStyle.Fill;
            lblTaiKhoan.ForeColor = Color.FromArgb(85, 96, 108);
            lblTaiKhoan.Location = new Point(28, 66);
            lblTaiKhoan.Margin = new Padding(0);
            lblTaiKhoan.Name = "lblTaiKhoan";
            lblTaiKhoan.Size = new Size(390, 32);
            lblTaiKhoan.TabIndex = 1;
            lblTaiKhoan.Text = "Tài khoản";
            // 
            // lblMatKhauCu
            // 
            lblMatKhauCu.Dock = DockStyle.Fill;
            lblMatKhauCu.Location = new Point(28, 98);
            lblMatKhauCu.Margin = new Padding(0);
            lblMatKhauCu.Name = "lblMatKhauCu";
            lblMatKhauCu.Size = new Size(390, 28);
            lblMatKhauCu.TabIndex = 2;
            lblMatKhauCu.Text = "Mật khẩu hiện tại";
            lblMatKhauCu.TextAlign = ContentAlignment.BottomLeft;
            // 
            // txtMatKhauCu
            // 
            txtMatKhauCu.Dock = DockStyle.Fill;
            txtMatKhauCu.Location = new Point(28, 128);
            txtMatKhauCu.Margin = new Padding(0, 2, 0, 0);
            txtMatKhauCu.Name = "txtMatKhauCu";
            txtMatKhauCu.PasswordChar = '●';
            txtMatKhauCu.Size = new Size(390, 27);
            txtMatKhauCu.TabIndex = 0;
            // 
            // lblMatKhauMoi
            // 
            lblMatKhauMoi.Dock = DockStyle.Fill;
            lblMatKhauMoi.Location = new Point(28, 164);
            lblMatKhauMoi.Margin = new Padding(0);
            lblMatKhauMoi.Name = "lblMatKhauMoi";
            lblMatKhauMoi.Size = new Size(390, 28);
            lblMatKhauMoi.TabIndex = 3;
            lblMatKhauMoi.Text = "Mật khẩu mới";
            lblMatKhauMoi.TextAlign = ContentAlignment.BottomLeft;
            // 
            // txtMatKhauMoi
            // 
            txtMatKhauMoi.Dock = DockStyle.Fill;
            txtMatKhauMoi.Location = new Point(28, 194);
            txtMatKhauMoi.Margin = new Padding(0, 2, 0, 0);
            txtMatKhauMoi.Name = "txtMatKhauMoi";
            txtMatKhauMoi.PasswordChar = '●';
            txtMatKhauMoi.Size = new Size(390, 27);
            txtMatKhauMoi.TabIndex = 1;
            // 
            // lblXacNhanMatKhau
            // 
            lblXacNhanMatKhau.Dock = DockStyle.Fill;
            lblXacNhanMatKhau.Location = new Point(28, 230);
            lblXacNhanMatKhau.Margin = new Padding(0);
            lblXacNhanMatKhau.Name = "lblXacNhanMatKhau";
            lblXacNhanMatKhau.Size = new Size(390, 28);
            lblXacNhanMatKhau.TabIndex = 4;
            lblXacNhanMatKhau.Text = "Xác nhận mật khẩu mới";
            lblXacNhanMatKhau.TextAlign = ContentAlignment.BottomLeft;
            // 
            // txtXacNhanMatKhau
            // 
            txtXacNhanMatKhau.Dock = DockStyle.Fill;
            txtXacNhanMatKhau.Location = new Point(28, 260);
            txtXacNhanMatKhau.Margin = new Padding(0, 2, 0, 0);
            txtXacNhanMatKhau.Name = "txtXacNhanMatKhau";
            txtXacNhanMatKhau.PasswordChar = '●';
            txtXacNhanMatKhau.Size = new Size(390, 27);
            txtXacNhanMatKhau.TabIndex = 2;
            // 
            // lblThongBao
            // 
            lblThongBao.Dock = DockStyle.Fill;
            lblThongBao.ForeColor = Color.FromArgb(220, 53, 69);
            lblThongBao.Location = new Point(28, 296);
            lblThongBao.Margin = new Padding(0);
            lblThongBao.Name = "lblThongBao";
            lblThongBao.Size = new Size(390, 42);
            lblThongBao.TabIndex = 5;
            lblThongBao.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnHuy);
            pnlButtons.Controls.Add(btnLuu);
            pnlButtons.Dock = DockStyle.Fill;
            pnlButtons.Location = new Point(28, 338);
            pnlButtons.Margin = new Padding(0);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(390, 52);
            pnlButtons.TabIndex = 6;
            // 
            // btnHuy
            // 
            btnHuy.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnHuy.DialogResult = DialogResult.Cancel;
            btnHuy.Cursor = Cursors.Hand;
            btnHuy.FlatStyle = FlatStyle.Flat;
            btnHuy.Location = new Point(154, 8);
            btnHuy.Name = "btnHuy";
            btnHuy.Size = new Size(104, 32);
            btnHuy.TabIndex = 1;
            btnHuy.Text = "Hủy";
            btnHuy.UseVisualStyleBackColor = true;
            // 
            // btnLuu
            // 
            btnLuu.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLuu.BackColor = Color.FromArgb(63, 81, 181);
            btnLuu.Cursor = Cursors.Hand;
            btnLuu.FlatAppearance.BorderSize = 0;
            btnLuu.FlatAppearance.MouseDownBackColor = Color.FromArgb(49, 63, 160);
            btnLuu.FlatAppearance.MouseOverBackColor = Color.FromArgb(79, 99, 215);
            btnLuu.FlatStyle = FlatStyle.Flat;
            btnLuu.ForeColor = Color.White;
            btnLuu.Location = new Point(270, 8);
            btnLuu.Name = "btnLuu";
            btnLuu.Size = new Size(120, 32);
            btnLuu.TabIndex = 0;
            btnLuu.Text = "Lưu";
            btnLuu.UseVisualStyleBackColor = false;
            // 
            // FormDoiMatKhau
            // 
            AcceptButton = btnLuu;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnHuy;
            ClientSize = new Size(520, 424);
            Controls.Add(pnlRoot);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormDoiMatKhau";
            StartPosition = FormStartPosition.CenterParent;
            Text = "HACTECH TEST - Đổi mật khẩu";
            pnlRoot.ResumeLayout(false);
            pnlCard.ResumeLayout(false);
            tblCard.ResumeLayout(false);
            tblCard.PerformLayout();
            pnlButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlRoot;
        private Panel pnlCard;
        private TableLayoutPanel tblCard;
        private Label lblTieuDe;
        private Label lblTaiKhoan;
        private Label lblMatKhauCu;
        private TextBox txtMatKhauCu;
        private Label lblMatKhauMoi;
        private TextBox txtMatKhauMoi;
        private Label lblXacNhanMatKhau;
        private TextBox txtXacNhanMatKhau;
        private Label lblThongBao;
        private Panel pnlButtons;
        private Button btnHuy;
        private Button btnLuu;
    }
}
