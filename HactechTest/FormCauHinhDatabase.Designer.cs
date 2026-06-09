namespace HactechTest
{
    partial class FormCauHinhDatabase
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
            tblRoot = new TableLayoutPanel();
            lblTieuDe = new Label();
            lblMoTa = new Label();
            grpThongTin = new GroupBox();
            tblForm = new TableLayoutPanel();
            lblConnectionString = new Label();
            txtConnectionString = new TextBox();
            lblGoiY = new Label();
            lblThongBao = new Label();
            pnlButtons = new Panel();
            btnKiemTraKetNoi = new Button();
            btnHuy = new Button();
            btnLuu = new Button();
            pnlRoot.SuspendLayout();
            tblRoot.SuspendLayout();
            grpThongTin.SuspendLayout();
            tblForm.SuspendLayout();
            pnlButtons.SuspendLayout();
            SuspendLayout();
            // 
            // pnlRoot
            // 
            pnlRoot.BackColor = Color.FromArgb(244, 245, 247);
            pnlRoot.Controls.Add(tblRoot);
            pnlRoot.Dock = DockStyle.Fill;
            pnlRoot.Location = new Point(0, 0);
            pnlRoot.Name = "pnlRoot";
            pnlRoot.Padding = new Padding(24);
            pnlRoot.Size = new Size(760, 420);
            pnlRoot.TabIndex = 0;
            // 
            // tblRoot
            // 
            tblRoot.ColumnCount = 1;
            tblRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblRoot.Controls.Add(lblTieuDe, 0, 0);
            tblRoot.Controls.Add(lblMoTa, 0, 1);
            tblRoot.Controls.Add(grpThongTin, 0, 2);
            tblRoot.Controls.Add(lblThongBao, 0, 3);
            tblRoot.Controls.Add(pnlButtons, 0, 4);
            tblRoot.Dock = DockStyle.Fill;
            tblRoot.Location = new Point(24, 24);
            tblRoot.Name = "tblRoot";
            tblRoot.RowCount = 5;
            tblRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            tblRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            tblRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            tblRoot.Size = new Size(712, 372);
            tblRoot.TabIndex = 0;
            // 
            // lblTieuDe
            // 
            lblTieuDe.Dock = DockStyle.Fill;
            lblTieuDe.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTieuDe.ForeColor = Color.FromArgb(31, 43, 56);
            lblTieuDe.Location = new Point(0, 0);
            lblTieuDe.Margin = new Padding(0);
            lblTieuDe.Name = "lblTieuDe";
            lblTieuDe.Size = new Size(712, 46);
            lblTieuDe.TabIndex = 0;
            lblTieuDe.Text = "Cấu hình database";
            lblTieuDe.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblMoTa
            // 
            lblMoTa.Dock = DockStyle.Fill;
            lblMoTa.ForeColor = Color.FromArgb(85, 96, 108);
            lblMoTa.Location = new Point(0, 46);
            lblMoTa.Margin = new Padding(0);
            lblMoTa.Name = "lblMoTa";
            lblMoTa.Size = new Size(712, 46);
            lblMoTa.TabIndex = 1;
            lblMoTa.Text = "Dán connection string SQL Server đã được cấp hoặc tự tạo sau khi chạy các file SQL trong HactechTest\\Database.";
            lblMoTa.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // grpThongTin
            // 
            grpThongTin.Controls.Add(tblForm);
            grpThongTin.Dock = DockStyle.Fill;
            grpThongTin.Location = new Point(0, 92);
            grpThongTin.Margin = new Padding(0);
            grpThongTin.Name = "grpThongTin";
            grpThongTin.Padding = new Padding(16, 12, 16, 16);
            grpThongTin.Size = new Size(712, 198);
            grpThongTin.TabIndex = 2;
            grpThongTin.TabStop = false;
            grpThongTin.Text = "Connection string";
            // 
            // tblForm
            // 
            tblForm.ColumnCount = 1;
            tblForm.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblForm.Controls.Add(lblConnectionString, 0, 0);
            tblForm.Controls.Add(txtConnectionString, 0, 1);
            tblForm.Controls.Add(lblGoiY, 0, 2);
            tblForm.Dock = DockStyle.Fill;
            tblForm.Location = new Point(16, 32);
            tblForm.Name = "tblForm";
            tblForm.RowCount = 3;
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblForm.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblForm.Size = new Size(680, 150);
            tblForm.TabIndex = 0;
            // 
            // lblConnectionString
            // 
            lblConnectionString.Dock = DockStyle.Fill;
            lblConnectionString.Location = new Point(0, 0);
            lblConnectionString.Margin = new Padding(0);
            lblConnectionString.Name = "lblConnectionString";
            lblConnectionString.Size = new Size(680, 30);
            lblConnectionString.TabIndex = 0;
            lblConnectionString.Text = "Nhập connection string:";
            lblConnectionString.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtConnectionString
            // 
            txtConnectionString.AcceptsReturn = true;
            txtConnectionString.AcceptsTab = true;
            txtConnectionString.Dock = DockStyle.Fill;
            txtConnectionString.Font = new Font("Consolas", 10F);
            txtConnectionString.Location = new Point(0, 30);
            txtConnectionString.Margin = new Padding(0);
            txtConnectionString.Multiline = true;
            txtConnectionString.Name = "txtConnectionString";
            txtConnectionString.ScrollBars = ScrollBars.Both;
            txtConnectionString.Size = new Size(680, 86);
            txtConnectionString.TabIndex = 0;
            txtConnectionString.WordWrap = false;
            // 
            // lblGoiY
            // 
            lblGoiY.Dock = DockStyle.Fill;
            lblGoiY.ForeColor = Color.FromArgb(85, 96, 108);
            lblGoiY.Location = new Point(0, 116);
            lblGoiY.Margin = new Padding(0);
            lblGoiY.Name = "lblGoiY";
            lblGoiY.Size = new Size(680, 34);
            lblGoiY.TabIndex = 2;
            lblGoiY.Text = "Ví dụ: Server=.;Database=HactechTestDb;Trusted_Connection=True;TrustServerCertificate=True";
            lblGoiY.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblThongBao
            // 
            lblThongBao.Dock = DockStyle.Fill;
            lblThongBao.ForeColor = Color.FromArgb(220, 53, 69);
            lblThongBao.Location = new Point(0, 290);
            lblThongBao.Margin = new Padding(0);
            lblThongBao.Name = "lblThongBao";
            lblThongBao.Size = new Size(712, 34);
            lblThongBao.TabIndex = 3;
            lblThongBao.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnKiemTraKetNoi);
            pnlButtons.Controls.Add(btnHuy);
            pnlButtons.Controls.Add(btnLuu);
            pnlButtons.Dock = DockStyle.Fill;
            pnlButtons.Location = new Point(0, 324);
            pnlButtons.Margin = new Padding(0);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(712, 48);
            pnlButtons.TabIndex = 4;
            // 
            // btnKiemTraKetNoi
            // 
            btnKiemTraKetNoi.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnKiemTraKetNoi.Cursor = Cursors.Hand;
            btnKiemTraKetNoi.FlatStyle = FlatStyle.Flat;
            btnKiemTraKetNoi.Location = new Point(0, 7);
            btnKiemTraKetNoi.Name = "btnKiemTraKetNoi";
            btnKiemTraKetNoi.Size = new Size(150, 34);
            btnKiemTraKetNoi.TabIndex = 1;
            btnKiemTraKetNoi.Text = "Kiểm tra kết nối";
            btnKiemTraKetNoi.UseVisualStyleBackColor = true;
            // 
            // btnHuy
            // 
            btnHuy.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnHuy.Cursor = Cursors.Hand;
            btnHuy.FlatStyle = FlatStyle.Flat;
            btnHuy.Location = new Point(468, 7);
            btnHuy.Name = "btnHuy";
            btnHuy.Size = new Size(110, 34);
            btnHuy.TabIndex = 2;
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
            btnLuu.Location = new Point(592, 7);
            btnLuu.Name = "btnLuu";
            btnLuu.Size = new Size(120, 34);
            btnLuu.TabIndex = 3;
            btnLuu.Text = "Lưu";
            btnLuu.UseVisualStyleBackColor = false;
            // 
            // FormCauHinhDatabase
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(760, 420);
            Controls.Add(pnlRoot);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormCauHinhDatabase";
            StartPosition = FormStartPosition.CenterParent;
            Text = "HACTECH TEST - Cấu hình database";
            pnlRoot.ResumeLayout(false);
            tblRoot.ResumeLayout(false);
            grpThongTin.ResumeLayout(false);
            tblForm.ResumeLayout(false);
            tblForm.PerformLayout();
            pnlButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlRoot;
        private TableLayoutPanel tblRoot;
        private Label lblTieuDe;
        private Label lblMoTa;
        private GroupBox grpThongTin;
        private TableLayoutPanel tblForm;
        private Label lblConnectionString;
        private TextBox txtConnectionString;
        private Label lblGoiY;
        private Label lblThongBao;
        private Panel pnlButtons;
        private Button btnKiemTraKetNoi;
        private Button btnHuy;
        private Button btnLuu;
    }
}
