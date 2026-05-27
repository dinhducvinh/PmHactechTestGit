namespace ApiTest
{
    partial class FormChiTietKetQuaTestCase
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
            pnlContent = new Panel();
            tblRootLayout = new TableLayoutPanel();
            pnlHeader = new Panel();
            lblTenTestCase = new Label();
            lblFormTitle = new Label();
            cardTongQuan = new Panel();
            tblTongQuanLayout = new TableLayoutPanel();
            lblTongQuanTitle = new Label();
            lblTongQuan = new Label();
            lblRequestInfo = new Label();
            cardLyDo = new Panel();
            tblLyDoLayout = new TableLayoutPanel();
            lblLyDoTitle = new Label();
            txtLyDo = new TextBox();
            cardResponseBody = new Panel();
            tblResponseLayout = new TableLayoutPanel();
            lblResponseTitle = new Label();
            rtbResponseBody = new RichTextBox();
            pnlBottomBar = new Panel();
            btnDong = new Button();
            pnlContent.SuspendLayout();
            tblRootLayout.SuspendLayout();
            pnlHeader.SuspendLayout();
            cardTongQuan.SuspendLayout();
            tblTongQuanLayout.SuspendLayout();
            cardLyDo.SuspendLayout();
            tblLyDoLayout.SuspendLayout();
            cardResponseBody.SuspendLayout();
            tblResponseLayout.SuspendLayout();
            pnlBottomBar.SuspendLayout();
            SuspendLayout();
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.FromArgb(244, 245, 247);
            pnlContent.Controls.Add(tblRootLayout);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 0);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(20);
            pnlContent.Size = new Size(1184, 761);
            pnlContent.TabIndex = 0;
            // 
            // tblRootLayout
            // 
            tblRootLayout.ColumnCount = 1;
            tblRootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblRootLayout.Controls.Add(pnlHeader, 0, 0);
            tblRootLayout.Controls.Add(cardTongQuan, 0, 1);
            tblRootLayout.Controls.Add(cardLyDo, 0, 2);
            tblRootLayout.Controls.Add(cardResponseBody, 0, 3);
            tblRootLayout.Controls.Add(pnlBottomBar, 0, 4);
            tblRootLayout.Dock = DockStyle.Fill;
            tblRootLayout.Location = new Point(20, 20);
            tblRootLayout.Margin = Padding.Empty;
            tblRootLayout.Name = "tblRootLayout";
            tblRootLayout.RowCount = 5;
            tblRootLayout.RowStyles.Add(new RowStyle());
            tblRootLayout.RowStyles.Add(new RowStyle());
            tblRootLayout.RowStyles.Add(new RowStyle());
            tblRootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblRootLayout.RowStyles.Add(new RowStyle());
            tblRootLayout.Size = new Size(1144, 721);
            tblRootLayout.TabIndex = 0;
            // 
            // pnlHeader
            // 
            pnlHeader.Controls.Add(lblTenTestCase);
            pnlHeader.Controls.Add(lblFormTitle);
            pnlHeader.Dock = DockStyle.Fill;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(0, 0, 0, 16);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1144, 80);
            pnlHeader.TabIndex = 0;
            // 
            // lblTenTestCase
            // 
            lblTenTestCase.AutoEllipsis = true;
            lblTenTestCase.Font = new Font("Segoe UI", 10.5F);
            lblTenTestCase.ForeColor = Color.FromArgb(63, 81, 181);
            lblTenTestCase.Location = new Point(16, 48);
            lblTenTestCase.Name = "lblTenTestCase";
            lblTenTestCase.Size = new Size(1112, 25);
            lblTenTestCase.TabIndex = 1;
            lblTenTestCase.Text = "Test case";
            // 
            // lblFormTitle
            // 
            lblFormTitle.AutoSize = true;
            lblFormTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblFormTitle.Location = new Point(16, 8);
            lblFormTitle.Name = "lblFormTitle";
            lblFormTitle.Size = new Size(372, 41);
            lblFormTitle.TabIndex = 0;
            lblFormTitle.Text = "Chi tiết kết quả test case";
            // 
            // cardTongQuan
            // 
            cardTongQuan.BackColor = Color.White;
            cardTongQuan.BorderStyle = BorderStyle.FixedSingle;
            cardTongQuan.Controls.Add(tblTongQuanLayout);
            cardTongQuan.Dock = DockStyle.Fill;
            cardTongQuan.Location = new Point(0, 96);
            cardTongQuan.Margin = new Padding(0, 0, 0, 16);
            cardTongQuan.Name = "cardTongQuan";
            cardTongQuan.Size = new Size(1144, 124);
            cardTongQuan.TabIndex = 1;
            // 
            // tblTongQuanLayout
            // 
            tblTongQuanLayout.ColumnCount = 1;
            tblTongQuanLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblTongQuanLayout.Controls.Add(lblTongQuanTitle, 0, 0);
            tblTongQuanLayout.Controls.Add(lblTongQuan, 0, 1);
            tblTongQuanLayout.Controls.Add(lblRequestInfo, 0, 2);
            tblTongQuanLayout.Dock = DockStyle.Fill;
            tblTongQuanLayout.Location = new Point(0, 0);
            tblTongQuanLayout.Margin = Padding.Empty;
            tblTongQuanLayout.Name = "tblTongQuanLayout";
            tblTongQuanLayout.Padding = new Padding(16);
            tblTongQuanLayout.RowCount = 3;
            tblTongQuanLayout.RowStyles.Add(new RowStyle());
            tblTongQuanLayout.RowStyles.Add(new RowStyle());
            tblTongQuanLayout.RowStyles.Add(new RowStyle());
            tblTongQuanLayout.Size = new Size(1142, 122);
            tblTongQuanLayout.TabIndex = 0;
            // 
            // lblTongQuanTitle
            // 
            lblTongQuanTitle.AutoSize = true;
            lblTongQuanTitle.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold);
            lblTongQuanTitle.Location = new Point(16, 16);
            lblTongQuanTitle.Margin = Padding.Empty;
            lblTongQuanTitle.Name = "lblTongQuanTitle";
            lblTongQuanTitle.Size = new Size(98, 25);
            lblTongQuanTitle.TabIndex = 0;
            lblTongQuanTitle.Text = "Tổng quan";
            // 
            // lblTongQuan
            // 
            lblTongQuan.AutoEllipsis = true;
            lblTongQuan.Dock = DockStyle.Fill;
            lblTongQuan.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            lblTongQuan.Location = new Point(16, 49);
            lblTongQuan.Margin = new Padding(0, 8, 0, 0);
            lblTongQuan.Name = "lblTongQuan";
            lblTongQuan.Size = new Size(1110, 22);
            lblTongQuan.TabIndex = 1;
            lblTongQuan.Text = "Kết quả";
            // 
            // lblRequestInfo
            // 
            lblRequestInfo.AutoEllipsis = true;
            lblRequestInfo.Dock = DockStyle.Fill;
            lblRequestInfo.Location = new Point(16, 79);
            lblRequestInfo.Margin = new Padding(0, 8, 0, 0);
            lblRequestInfo.Name = "lblRequestInfo";
            lblRequestInfo.Size = new Size(1110, 27);
            lblRequestInfo.TabIndex = 2;
            lblRequestInfo.Text = "Method / URL";
            // 
            // cardLyDo
            // 
            cardLyDo.BackColor = Color.White;
            cardLyDo.BorderStyle = BorderStyle.FixedSingle;
            cardLyDo.Controls.Add(tblLyDoLayout);
            cardLyDo.Dock = DockStyle.Fill;
            cardLyDo.Location = new Point(0, 236);
            cardLyDo.Margin = new Padding(0, 0, 0, 16);
            cardLyDo.Name = "cardLyDo";
            cardLyDo.Size = new Size(1144, 124);
            cardLyDo.TabIndex = 2;
            // 
            // tblLyDoLayout
            // 
            tblLyDoLayout.ColumnCount = 1;
            tblLyDoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblLyDoLayout.Controls.Add(lblLyDoTitle, 0, 0);
            tblLyDoLayout.Controls.Add(txtLyDo, 0, 1);
            tblLyDoLayout.Dock = DockStyle.Fill;
            tblLyDoLayout.Location = new Point(0, 0);
            tblLyDoLayout.Margin = Padding.Empty;
            tblLyDoLayout.Name = "tblLyDoLayout";
            tblLyDoLayout.Padding = new Padding(16);
            tblLyDoLayout.RowCount = 2;
            tblLyDoLayout.RowStyles.Add(new RowStyle());
            tblLyDoLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblLyDoLayout.Size = new Size(1142, 122);
            tblLyDoLayout.TabIndex = 0;
            // 
            // lblLyDoTitle
            // 
            lblLyDoTitle.AutoSize = true;
            lblLyDoTitle.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold);
            lblLyDoTitle.Location = new Point(16, 16);
            lblLyDoTitle.Margin = Padding.Empty;
            lblLyDoTitle.Name = "lblLyDoTitle";
            lblLyDoTitle.Size = new Size(131, 25);
            lblLyDoTitle.TabIndex = 0;
            lblLyDoTitle.Text = "Lý do / ghi chú";
            // 
            // txtLyDo
            // 
            txtLyDo.BackColor = Color.FromArgb(255, 245, 245);
            txtLyDo.BorderStyle = BorderStyle.FixedSingle;
            txtLyDo.Dock = DockStyle.Fill;
            txtLyDo.ForeColor = Color.FromArgb(170, 40, 40);
            txtLyDo.Location = new Point(16, 49);
            txtLyDo.Margin = new Padding(0, 8, 0, 0);
            txtLyDo.Multiline = true;
            txtLyDo.Name = "txtLyDo";
            txtLyDo.ReadOnly = true;
            txtLyDo.ScrollBars = ScrollBars.Vertical;
            txtLyDo.Size = new Size(1110, 57);
            txtLyDo.TabIndex = 1;
            // 
            // cardResponseBody
            // 
            cardResponseBody.BackColor = Color.White;
            cardResponseBody.BorderStyle = BorderStyle.FixedSingle;
            cardResponseBody.Controls.Add(tblResponseLayout);
            cardResponseBody.Dock = DockStyle.Fill;
            cardResponseBody.Location = new Point(0, 376);
            cardResponseBody.Margin = new Padding(0, 0, 0, 16);
            cardResponseBody.Name = "cardResponseBody";
            cardResponseBody.Size = new Size(1144, 281);
            cardResponseBody.TabIndex = 3;
            // 
            // tblResponseLayout
            // 
            tblResponseLayout.ColumnCount = 1;
            tblResponseLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblResponseLayout.Controls.Add(lblResponseTitle, 0, 0);
            tblResponseLayout.Controls.Add(rtbResponseBody, 0, 1);
            tblResponseLayout.Dock = DockStyle.Fill;
            tblResponseLayout.Location = new Point(0, 0);
            tblResponseLayout.Margin = Padding.Empty;
            tblResponseLayout.Name = "tblResponseLayout";
            tblResponseLayout.Padding = new Padding(16);
            tblResponseLayout.RowCount = 2;
            tblResponseLayout.RowStyles.Add(new RowStyle());
            tblResponseLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblResponseLayout.Size = new Size(1142, 279);
            tblResponseLayout.TabIndex = 0;
            // 
            // lblResponseTitle
            // 
            lblResponseTitle.AutoSize = true;
            lblResponseTitle.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold);
            lblResponseTitle.Location = new Point(16, 16);
            lblResponseTitle.Margin = Padding.Empty;
            lblResponseTitle.Name = "lblResponseTitle";
            lblResponseTitle.Size = new Size(131, 25);
            lblResponseTitle.TabIndex = 0;
            lblResponseTitle.Text = "Response Body";
            // 
            // rtbResponseBody
            // 
            rtbResponseBody.BackColor = Color.FromArgb(30, 30, 30);
            rtbResponseBody.BorderStyle = BorderStyle.FixedSingle;
            rtbResponseBody.Dock = DockStyle.Fill;
            rtbResponseBody.Font = new Font("Consolas", 10F);
            rtbResponseBody.ForeColor = Color.Gainsboro;
            rtbResponseBody.Location = new Point(16, 49);
            rtbResponseBody.Margin = new Padding(0, 8, 0, 0);
            rtbResponseBody.Name = "rtbResponseBody";
            rtbResponseBody.ReadOnly = true;
            rtbResponseBody.Size = new Size(1110, 214);
            rtbResponseBody.TabIndex = 1;
            rtbResponseBody.Text = "";
            rtbResponseBody.WordWrap = false;
            // 
            // pnlBottomBar
            // 
            pnlBottomBar.Controls.Add(btnDong);
            pnlBottomBar.Dock = DockStyle.Fill;
            pnlBottomBar.Location = new Point(0, 673);
            pnlBottomBar.Margin = Padding.Empty;
            pnlBottomBar.Name = "pnlBottomBar";
            pnlBottomBar.Size = new Size(1144, 48);
            pnlBottomBar.TabIndex = 4;
            // 
            // btnDong
            // 
            btnDong.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDong.BackColor = Color.FromArgb(63, 81, 181);
            btnDong.DialogResult = DialogResult.OK;
            btnDong.FlatAppearance.BorderSize = 0;
            btnDong.FlatStyle = FlatStyle.Flat;
            btnDong.ForeColor = Color.White;
            btnDong.Location = new Point(1004, 8);
            btnDong.Name = "btnDong";
            btnDong.Size = new Size(124, 32);
            btnDong.TabIndex = 0;
            btnDong.Text = "Đóng";
            btnDong.UseVisualStyleBackColor = false;
            // 
            // FormChiTietKetQuaTestCase
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnDong;
            ClientSize = new Size(1184, 761);
            Controls.Add(pnlContent);
            MinimumSize = new Size(980, 640);
            Name = "FormChiTietKetQuaTestCase";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Chi tiết kết quả test case";
            pnlContent.ResumeLayout(false);
            tblRootLayout.ResumeLayout(false);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            cardTongQuan.ResumeLayout(false);
            tblTongQuanLayout.ResumeLayout(false);
            tblTongQuanLayout.PerformLayout();
            cardLyDo.ResumeLayout(false);
            tblLyDoLayout.ResumeLayout(false);
            tblLyDoLayout.PerformLayout();
            cardResponseBody.ResumeLayout(false);
            tblResponseLayout.ResumeLayout(false);
            tblResponseLayout.PerformLayout();
            pnlBottomBar.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlContent;
        private TableLayoutPanel tblRootLayout;
        private Panel pnlHeader;
        private Label lblTenTestCase;
        private Label lblFormTitle;
        private Panel cardTongQuan;
        private TableLayoutPanel tblTongQuanLayout;
        private Label lblTongQuanTitle;
        private Label lblTongQuan;
        private Label lblRequestInfo;
        private Panel cardLyDo;
        private TableLayoutPanel tblLyDoLayout;
        private Label lblLyDoTitle;
        private TextBox txtLyDo;
        private Panel cardResponseBody;
        private TableLayoutPanel tblResponseLayout;
        private Label lblResponseTitle;
        private RichTextBox rtbResponseBody;
        private Panel pnlBottomBar;
        private Button btnDong;
    }
}
