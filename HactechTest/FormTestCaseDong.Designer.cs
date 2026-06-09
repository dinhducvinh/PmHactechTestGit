namespace HactechTest
{
    partial class FormTestCaseDong
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            pnlContent = new Panel();
            tblRoot = new TableLayoutPanel();
            pnlHeader = new Panel();
            lblGoiY = new Label();
            lblTieuDe = new Label();
            grpThongTinChung = new GroupBox();
            tblThongTinChung = new TableLayoutPanel();
            lblMa = new Label();
            txtMa = new TextBox();
            lblNhom = new Label();
            txtNhom = new TextBox();
            lblTen = new Label();
            txtTen = new TextBox();
            lblMoTa = new Label();
            txtMoTa = new TextBox();
            lblMethod = new Label();
            cboMethod = new ComboBox();
            lblEndpoint = new Label();
            txtEndpoint = new TextBox();
            lblAuthMode = new Label();
            cboAuthMode = new ComboBox();
            grpRequest = new GroupBox();
            tblRequest = new TableLayoutPanel();
            lblPathParamsJson = new Label();
            txtPathParamsJson = new TextBox();
            lblHeadersJson = new Label();
            txtHeadersJson = new TextBox();
            lblBodyJson = new Label();
            txtBodyJson = new TextBox();
            grpKyVong = new GroupBox();
            tblKyVong = new TableLayoutPanel();
            lblExpectedCodes = new Label();
            txtExpectedCodes = new TextBox();
            lblExpectedHttpStatus = new Label();
            txtExpectedHttpStatus = new TextBox();
            lblExpectedJsonPath = new Label();
            txtExpectedJsonPath = new TextBox();
            lblExpectedJsonValue = new Label();
            txtExpectedJsonValue = new TextBox();
            pnlBottom = new Panel();
            btnHuy = new Button();
            btnLuu = new Button();
            pnlContent.SuspendLayout();
            tblRoot.SuspendLayout();
            pnlHeader.SuspendLayout();
            grpThongTinChung.SuspendLayout();
            tblThongTinChung.SuspendLayout();
            grpRequest.SuspendLayout();
            tblRequest.SuspendLayout();
            grpKyVong.SuspendLayout();
            tblKyVong.SuspendLayout();
            pnlBottom.SuspendLayout();
            SuspendLayout();
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.FromArgb(244, 245, 247);
            pnlContent.Controls.Add(tblRoot);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 0);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(18);
            pnlContent.Size = new Size(1084, 741);
            pnlContent.TabIndex = 0;
            // 
            // tblRoot
            // 
            tblRoot.ColumnCount = 1;
            tblRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblRoot.Controls.Add(pnlHeader, 0, 0);
            tblRoot.Controls.Add(grpThongTinChung, 0, 1);
            tblRoot.Controls.Add(grpRequest, 0, 2);
            tblRoot.Controls.Add(grpKyVong, 0, 3);
            tblRoot.Controls.Add(pnlBottom, 0, 4);
            tblRoot.Dock = DockStyle.Fill;
            tblRoot.Location = new Point(18, 18);
            tblRoot.Margin = new Padding(0);
            tblRoot.Name = "tblRoot";
            tblRoot.RowCount = 5;
            tblRoot.RowStyles.Add(new RowStyle());
            tblRoot.RowStyles.Add(new RowStyle());
            tblRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblRoot.RowStyles.Add(new RowStyle());
            tblRoot.RowStyles.Add(new RowStyle());
            tblRoot.Size = new Size(1048, 705);
            tblRoot.TabIndex = 0;
            // 
            // pnlHeader
            // 
            pnlHeader.Controls.Add(lblGoiY);
            pnlHeader.Controls.Add(lblTieuDe);
            pnlHeader.Dock = DockStyle.Fill;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(0, 0, 0, 12);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1048, 72);
            pnlHeader.TabIndex = 0;
            // 
            // lblGoiY
            // 
            lblGoiY.AutoEllipsis = true;
            lblGoiY.ForeColor = Color.FromArgb(80, 80, 80);
            lblGoiY.Location = new Point(4, 42);
            lblGoiY.Name = "lblGoiY";
            lblGoiY.Size = new Size(1036, 24);
            lblGoiY.TabIndex = 1;
            lblGoiY.Text = "Dùng cho test case cơ bản. Các case cần upload file, nhiều bước phụ thuộc nhau hoặc logic đặc biệt vẫn viết trong ApiShopTesting/KichBan.";
            // 
            // lblTieuDe
            // 
            lblTieuDe.AutoSize = true;
            lblTieuDe.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTieuDe.Location = new Point(0, 0);
            lblTieuDe.Name = "lblTieuDe";
            lblTieuDe.Size = new Size(332, 41);
            lblTieuDe.TabIndex = 0;
            lblTieuDe.Text = "Test case cơ bản từ DB";
            // 
            // grpThongTinChung
            // 
            grpThongTinChung.Controls.Add(tblThongTinChung);
            grpThongTinChung.Dock = DockStyle.Fill;
            grpThongTinChung.Location = new Point(0, 84);
            grpThongTinChung.Margin = new Padding(0, 0, 0, 12);
            grpThongTinChung.MinimumSize = new Size(0, 236);
            grpThongTinChung.Name = "grpThongTinChung";
            grpThongTinChung.Padding = new Padding(12);
            grpThongTinChung.Size = new Size(1048, 236);
            grpThongTinChung.TabIndex = 1;
            grpThongTinChung.TabStop = false;
            grpThongTinChung.Text = "Thông tin chung";
            // 
            // tblThongTinChung
            // 
            tblThongTinChung.ColumnCount = 4;
            tblThongTinChung.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            tblThongTinChung.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblThongTinChung.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            tblThongTinChung.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblThongTinChung.Controls.Add(lblMa, 0, 0);
            tblThongTinChung.Controls.Add(txtMa, 1, 0);
            tblThongTinChung.Controls.Add(lblNhom, 2, 0);
            tblThongTinChung.Controls.Add(txtNhom, 3, 0);
            tblThongTinChung.Controls.Add(lblTen, 0, 1);
            tblThongTinChung.Controls.Add(txtTen, 1, 1);
            tblThongTinChung.Controls.Add(lblMoTa, 0, 2);
            tblThongTinChung.Controls.Add(txtMoTa, 1, 2);
            tblThongTinChung.Controls.Add(lblMethod, 0, 3);
            tblThongTinChung.Controls.Add(cboMethod, 1, 3);
            tblThongTinChung.Controls.Add(lblEndpoint, 2, 3);
            tblThongTinChung.Controls.Add(txtEndpoint, 3, 3);
            tblThongTinChung.Controls.Add(lblAuthMode, 0, 4);
            tblThongTinChung.Controls.Add(cboAuthMode, 1, 4);
            tblThongTinChung.Dock = DockStyle.Fill;
            tblThongTinChung.Location = new Point(12, 32);
            tblThongTinChung.Margin = new Padding(0);
            tblThongTinChung.Name = "tblThongTinChung";
            tblThongTinChung.RowCount = 5;
            tblThongTinChung.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblThongTinChung.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblThongTinChung.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            tblThongTinChung.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblThongTinChung.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            tblThongTinChung.Size = new Size(1024, 192);
            tblThongTinChung.TabIndex = 0;
            // 
            // lblMa
            // 
            lblMa.Dock = DockStyle.Fill;
            lblMa.Location = new Point(0, 0);
            lblMa.Margin = new Padding(0);
            lblMa.Name = "lblMa";
            lblMa.Size = new Size(130, 34);
            lblMa.TabIndex = 0;
            lblMa.Text = "Mã test case:";
            lblMa.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtMa
            // 
            txtMa.Dock = DockStyle.Fill;
            txtMa.Location = new Point(130, 2);
            txtMa.Margin = new Padding(0, 2, 12, 4);
            txtMa.Name = "txtMa";
            txtMa.PlaceholderText = "VD: CUSTOM-LOGIN-01";
            txtMa.Size = new Size(370, 27);
            txtMa.TabIndex = 1;
            // 
            // lblNhom
            // 
            lblNhom.Dock = DockStyle.Fill;
            lblNhom.Location = new Point(512, 0);
            lblNhom.Margin = new Padding(0);
            lblNhom.Name = "lblNhom";
            lblNhom.Size = new Size(130, 34);
            lblNhom.TabIndex = 2;
            lblNhom.Text = "Module/Nhóm:";
            lblNhom.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtNhom
            // 
            txtNhom.Dock = DockStyle.Fill;
            txtNhom.Location = new Point(642, 2);
            txtNhom.Margin = new Padding(0, 2, 0, 4);
            txtNhom.Name = "txtNhom";
            txtNhom.PlaceholderText = "VD: Auth";
            txtNhom.Size = new Size(382, 27);
            txtNhom.TabIndex = 3;
            // 
            // lblTen
            // 
            lblTen.Dock = DockStyle.Fill;
            lblTen.Location = new Point(0, 34);
            lblTen.Margin = new Padding(0);
            lblTen.Name = "lblTen";
            lblTen.Size = new Size(130, 34);
            lblTen.TabIndex = 4;
            lblTen.Text = "Tên hiển thị:";
            lblTen.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtTen
            // 
            tblThongTinChung.SetColumnSpan(txtTen, 3);
            txtTen.Dock = DockStyle.Fill;
            txtTen.Location = new Point(130, 36);
            txtTen.Margin = new Padding(0, 2, 0, 4);
            txtTen.Name = "txtTen";
            txtTen.PlaceholderText = "Tên test case hiển thị trên màn chạy test";
            txtTen.Size = new Size(894, 27);
            txtTen.TabIndex = 5;
            // 
            // lblMoTa
            // 
            lblMoTa.Dock = DockStyle.Fill;
            lblMoTa.Location = new Point(0, 68);
            lblMoTa.Margin = new Padding(0);
            lblMoTa.Name = "lblMoTa";
            lblMoTa.Size = new Size(130, 54);
            lblMoTa.TabIndex = 6;
            lblMoTa.Text = "Mô tả:";
            lblMoTa.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtMoTa
            // 
            tblThongTinChung.SetColumnSpan(txtMoTa, 3);
            txtMoTa.Dock = DockStyle.Fill;
            txtMoTa.Location = new Point(130, 70);
            txtMoTa.Margin = new Padding(0, 2, 0, 4);
            txtMoTa.Multiline = true;
            txtMoTa.Name = "txtMoTa";
            txtMoTa.PlaceholderText = "Mô tả ngắn về dữ liệu và kỳ vọng của test case";
            txtMoTa.ScrollBars = ScrollBars.Vertical;
            txtMoTa.Size = new Size(894, 48);
            txtMoTa.TabIndex = 7;
            // 
            // lblMethod
            // 
            lblMethod.Dock = DockStyle.Fill;
            lblMethod.Location = new Point(0, 122);
            lblMethod.Margin = new Padding(0);
            lblMethod.Name = "lblMethod";
            lblMethod.Size = new Size(130, 34);
            lblMethod.TabIndex = 8;
            lblMethod.Text = "Method:";
            lblMethod.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cboMethod
            // 
            cboMethod.Dock = DockStyle.Fill;
            cboMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMethod.FormattingEnabled = true;
            cboMethod.Items.AddRange(new object[] { "GET", "POST", "PUT", "PATCH", "DELETE" });
            cboMethod.Location = new Point(130, 124);
            cboMethod.Margin = new Padding(0, 2, 12, 4);
            cboMethod.Name = "cboMethod";
            cboMethod.Size = new Size(370, 28);
            cboMethod.SelectedIndex = 0;
            cboMethod.TabIndex = 9;
            // 
            // lblEndpoint
            // 
            lblEndpoint.Dock = DockStyle.Fill;
            lblEndpoint.Location = new Point(512, 122);
            lblEndpoint.Margin = new Padding(0);
            lblEndpoint.Name = "lblEndpoint";
            lblEndpoint.Size = new Size(130, 34);
            lblEndpoint.TabIndex = 10;
            lblEndpoint.Text = "Endpoint:";
            lblEndpoint.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtEndpoint
            // 
            txtEndpoint.Dock = DockStyle.Fill;
            txtEndpoint.Location = new Point(642, 124);
            txtEndpoint.Margin = new Padding(0, 2, 0, 4);
            txtEndpoint.Name = "txtEndpoint";
            txtEndpoint.PlaceholderText = "VD: /auth/login";
            txtEndpoint.Size = new Size(382, 27);
            txtEndpoint.TabIndex = 11;
            // 
            // lblAuthMode
            // 
            lblAuthMode.Dock = DockStyle.Fill;
            lblAuthMode.Location = new Point(0, 156);
            lblAuthMode.Margin = new Padding(0);
            lblAuthMode.Name = "lblAuthMode";
            lblAuthMode.Size = new Size(130, 36);
            lblAuthMode.TabIndex = 12;
            lblAuthMode.Text = "Token:";
            lblAuthMode.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // cboAuthMode
            // 
            tblThongTinChung.SetColumnSpan(cboAuthMode, 3);
            cboAuthMode.Dock = DockStyle.Fill;
            cboAuthMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cboAuthMode.FormattingEnabled = true;
            cboAuthMode.Items.AddRange(new object[] { "Không dùng token", "Bearer token tài khoản seed" });
            cboAuthMode.Location = new Point(130, 158);
            cboAuthMode.Margin = new Padding(0, 2, 0, 4);
            cboAuthMode.Name = "cboAuthMode";
            cboAuthMode.Size = new Size(894, 28);
            cboAuthMode.SelectedIndex = 0;
            cboAuthMode.TabIndex = 13;
            // 
            // grpRequest
            // 
            grpRequest.Controls.Add(tblRequest);
            grpRequest.Dock = DockStyle.Fill;
            grpRequest.Location = new Point(0, 332);
            grpRequest.Margin = new Padding(0, 0, 0, 12);
            grpRequest.Name = "grpRequest";
            grpRequest.Padding = new Padding(12);
            grpRequest.Size = new Size(1048, 154);
            grpRequest.TabIndex = 2;
            grpRequest.TabStop = false;
            grpRequest.Text = "Request";
            // 
            // tblRequest
            // 
            tblRequest.ColumnCount = 3;
            tblRequest.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tblRequest.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tblRequest.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tblRequest.Controls.Add(lblPathParamsJson, 0, 0);
            tblRequest.Controls.Add(txtPathParamsJson, 0, 1);
            tblRequest.Controls.Add(lblHeadersJson, 1, 0);
            tblRequest.Controls.Add(txtHeadersJson, 1, 1);
            tblRequest.Controls.Add(lblBodyJson, 2, 0);
            tblRequest.Controls.Add(txtBodyJson, 2, 1);
            tblRequest.Dock = DockStyle.Fill;
            tblRequest.Location = new Point(12, 32);
            tblRequest.Margin = new Padding(0);
            tblRequest.Name = "tblRequest";
            tblRequest.RowCount = 2;
            tblRequest.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tblRequest.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblRequest.Size = new Size(1024, 110);
            tblRequest.TabIndex = 0;
            // 
            // lblPathParamsJson
            // 
            lblPathParamsJson.Dock = DockStyle.Fill;
            lblPathParamsJson.Location = new Point(0, 0);
            lblPathParamsJson.Margin = new Padding(0);
            lblPathParamsJson.Name = "lblPathParamsJson";
            lblPathParamsJson.Size = new Size(341, 28);
            lblPathParamsJson.TabIndex = 0;
            lblPathParamsJson.Text = "Path params JSON:";
            lblPathParamsJson.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtPathParamsJson
            // 
            txtPathParamsJson.Dock = DockStyle.Fill;
            txtPathParamsJson.Font = new Font("Consolas", 9F);
            txtPathParamsJson.Location = new Point(0, 28);
            txtPathParamsJson.Margin = new Padding(0, 0, 8, 0);
            txtPathParamsJson.Multiline = true;
            txtPathParamsJson.Name = "txtPathParamsJson";
            txtPathParamsJson.PlaceholderText = "{ \"id\": \"123\" }";
            txtPathParamsJson.ScrollBars = ScrollBars.Both;
            txtPathParamsJson.Size = new Size(333, 82);
            txtPathParamsJson.TabIndex = 1;
            txtPathParamsJson.WordWrap = false;
            // 
            // lblHeadersJson
            // 
            lblHeadersJson.Dock = DockStyle.Fill;
            lblHeadersJson.Location = new Point(341, 0);
            lblHeadersJson.Margin = new Padding(0);
            lblHeadersJson.Name = "lblHeadersJson";
            lblHeadersJson.Size = new Size(341, 28);
            lblHeadersJson.TabIndex = 2;
            lblHeadersJson.Text = "Headers JSON:";
            lblHeadersJson.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtHeadersJson
            // 
            txtHeadersJson.Dock = DockStyle.Fill;
            txtHeadersJson.Font = new Font("Consolas", 9F);
            txtHeadersJson.Location = new Point(349, 28);
            txtHeadersJson.Margin = new Padding(8, 0, 8, 0);
            txtHeadersJson.Multiline = true;
            txtHeadersJson.Name = "txtHeadersJson";
            txtHeadersJson.PlaceholderText = "{ \"X-Test\": \"1\" }";
            txtHeadersJson.ScrollBars = ScrollBars.Both;
            txtHeadersJson.Size = new Size(325, 82);
            txtHeadersJson.TabIndex = 3;
            txtHeadersJson.WordWrap = false;
            // 
            // lblBodyJson
            // 
            lblBodyJson.Dock = DockStyle.Fill;
            lblBodyJson.Location = new Point(682, 0);
            lblBodyJson.Margin = new Padding(0);
            lblBodyJson.Name = "lblBodyJson";
            lblBodyJson.Size = new Size(342, 28);
            lblBodyJson.TabIndex = 4;
            lblBodyJson.Text = "Body JSON:";
            lblBodyJson.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtBodyJson
            // 
            txtBodyJson.Dock = DockStyle.Fill;
            txtBodyJson.Font = new Font("Consolas", 9F);
            txtBodyJson.Location = new Point(690, 28);
            txtBodyJson.Margin = new Padding(8, 0, 0, 0);
            txtBodyJson.Multiline = true;
            txtBodyJson.Name = "txtBodyJson";
            txtBodyJson.PlaceholderText = "{ \"phone\": \"...\", \"password\": \"...\" }";
            txtBodyJson.ScrollBars = ScrollBars.Both;
            txtBodyJson.Size = new Size(334, 82);
            txtBodyJson.TabIndex = 5;
            txtBodyJson.WordWrap = false;
            // 
            // grpKyVong
            // 
            grpKyVong.Controls.Add(tblKyVong);
            grpKyVong.Dock = DockStyle.Fill;
            grpKyVong.Location = new Point(0, 498);
            grpKyVong.Margin = new Padding(0, 0, 0, 12);
            grpKyVong.MinimumSize = new Size(0, 152);
            grpKyVong.Name = "grpKyVong";
            grpKyVong.Padding = new Padding(12);
            grpKyVong.Size = new Size(1048, 152);
            grpKyVong.TabIndex = 3;
            grpKyVong.TabStop = false;
            grpKyVong.Text = "Kỳ vọng";
            // 
            // tblKyVong
            // 
            tblKyVong.ColumnCount = 4;
            tblKyVong.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            tblKyVong.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblKyVong.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            tblKyVong.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblKyVong.Controls.Add(lblExpectedCodes, 0, 0);
            tblKyVong.Controls.Add(txtExpectedCodes, 1, 0);
            tblKyVong.Controls.Add(lblExpectedHttpStatus, 2, 0);
            tblKyVong.Controls.Add(txtExpectedHttpStatus, 3, 0);
            tblKyVong.Controls.Add(lblExpectedJsonPath, 0, 1);
            tblKyVong.Controls.Add(txtExpectedJsonPath, 1, 1);
            tblKyVong.Controls.Add(lblExpectedJsonValue, 2, 1);
            tblKyVong.Controls.Add(txtExpectedJsonValue, 3, 1);
            tblKyVong.Dock = DockStyle.Fill;
            tblKyVong.Location = new Point(12, 32);
            tblKyVong.Margin = new Padding(0);
            tblKyVong.Name = "tblKyVong";
            tblKyVong.RowCount = 3;
            tblKyVong.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tblKyVong.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tblKyVong.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblKyVong.Size = new Size(1024, 108);
            tblKyVong.TabIndex = 0;
            // 
            // lblExpectedCodes
            // 
            lblExpectedCodes.Dock = DockStyle.Fill;
            lblExpectedCodes.Location = new Point(0, 0);
            lblExpectedCodes.Margin = new Padding(0);
            lblExpectedCodes.Name = "lblExpectedCodes";
            lblExpectedCodes.Size = new Size(150, 36);
            lblExpectedCodes.TabIndex = 0;
            lblExpectedCodes.Text = "Mã nghiệp vụ:";
            lblExpectedCodes.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtExpectedCodes
            // 
            txtExpectedCodes.Dock = DockStyle.Fill;
            txtExpectedCodes.Location = new Point(150, 3);
            txtExpectedCodes.Margin = new Padding(0, 3, 12, 6);
            txtExpectedCodes.Name = "txtExpectedCodes";
            txtExpectedCodes.PlaceholderText = "VD: 1000, 1002; bỏ trống nếu chỉ kiểm HTTP";
            txtExpectedCodes.Size = new Size(350, 27);
            txtExpectedCodes.TabIndex = 1;
            // 
            // lblExpectedHttpStatus
            // 
            lblExpectedHttpStatus.Dock = DockStyle.Fill;
            lblExpectedHttpStatus.Location = new Point(512, 0);
            lblExpectedHttpStatus.Margin = new Padding(0);
            lblExpectedHttpStatus.Name = "lblExpectedHttpStatus";
            lblExpectedHttpStatus.Size = new Size(150, 36);
            lblExpectedHttpStatus.TabIndex = 2;
            lblExpectedHttpStatus.Text = "HTTP status:";
            lblExpectedHttpStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtExpectedHttpStatus
            // 
            txtExpectedHttpStatus.Dock = DockStyle.Fill;
            txtExpectedHttpStatus.Location = new Point(662, 3);
            txtExpectedHttpStatus.Margin = new Padding(0, 3, 0, 6);
            txtExpectedHttpStatus.Name = "txtExpectedHttpStatus";
            txtExpectedHttpStatus.PlaceholderText = "VD: 200";
            txtExpectedHttpStatus.Size = new Size(362, 27);
            txtExpectedHttpStatus.TabIndex = 3;
            // 
            // lblExpectedJsonPath
            // 
            lblExpectedJsonPath.Dock = DockStyle.Fill;
            lblExpectedJsonPath.Location = new Point(0, 36);
            lblExpectedJsonPath.Margin = new Padding(0);
            lblExpectedJsonPath.Name = "lblExpectedJsonPath";
            lblExpectedJsonPath.Size = new Size(150, 36);
            lblExpectedJsonPath.TabIndex = 4;
            lblExpectedJsonPath.Text = "JSON path:";
            lblExpectedJsonPath.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtExpectedJsonPath
            // 
            txtExpectedJsonPath.Dock = DockStyle.Fill;
            txtExpectedJsonPath.Location = new Point(150, 39);
            txtExpectedJsonPath.Margin = new Padding(0, 3, 12, 6);
            txtExpectedJsonPath.Name = "txtExpectedJsonPath";
            txtExpectedJsonPath.PlaceholderText = "VD: $.data.id";
            txtExpectedJsonPath.Size = new Size(350, 27);
            txtExpectedJsonPath.TabIndex = 5;
            // 
            // lblExpectedJsonValue
            // 
            lblExpectedJsonValue.Dock = DockStyle.Fill;
            lblExpectedJsonValue.Location = new Point(512, 36);
            lblExpectedJsonValue.Margin = new Padding(0);
            lblExpectedJsonValue.Name = "lblExpectedJsonValue";
            lblExpectedJsonValue.Size = new Size(150, 36);
            lblExpectedJsonValue.TabIndex = 6;
            lblExpectedJsonValue.Text = "Giá trị JSON:";
            lblExpectedJsonValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtExpectedJsonValue
            // 
            txtExpectedJsonValue.Dock = DockStyle.Fill;
            txtExpectedJsonValue.Location = new Point(662, 39);
            txtExpectedJsonValue.Margin = new Padding(0, 3, 0, 6);
            txtExpectedJsonValue.Name = "txtExpectedJsonValue";
            txtExpectedJsonValue.PlaceholderText = "Bỏ trống nếu chỉ cần tồn tại path";
            txtExpectedJsonValue.Size = new Size(362, 27);
            txtExpectedJsonValue.TabIndex = 7;
            // 
            // pnlBottom
            // 
            pnlBottom.Controls.Add(btnHuy);
            pnlBottom.Controls.Add(btnLuu);
            pnlBottom.Dock = DockStyle.Fill;
            pnlBottom.Location = new Point(0, 662);
            pnlBottom.Margin = new Padding(0);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new Size(1048, 43);
            pnlBottom.TabIndex = 4;
            // 
            // btnHuy
            // 
            btnHuy.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnHuy.DialogResult = DialogResult.Cancel;
            btnHuy.Cursor = Cursors.Hand;
            btnHuy.FlatStyle = FlatStyle.Flat;
            btnHuy.Location = new Point(806, 4);
            btnHuy.Name = "btnHuy";
            btnHuy.Size = new Size(112, 32);
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
            btnLuu.Location = new Point(928, 4);
            btnLuu.Name = "btnLuu";
            btnLuu.Size = new Size(120, 32);
            btnLuu.TabIndex = 0;
            btnLuu.Text = "Lưu";
            btnLuu.UseVisualStyleBackColor = false;
            btnLuu.Click += BtnLuu_Click;
            // 
            // FormTestCaseDong
            // 
            AcceptButton = btnLuu;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnHuy;
            ClientSize = new Size(1084, 741);
            Controls.Add(pnlContent);
            MinimumSize = new Size(980, 760);
            Name = "FormTestCaseDong";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Test case cơ bản";
            pnlContent.ResumeLayout(false);
            tblRoot.ResumeLayout(false);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            grpThongTinChung.ResumeLayout(false);
            tblThongTinChung.ResumeLayout(false);
            tblThongTinChung.PerformLayout();
            grpRequest.ResumeLayout(false);
            tblRequest.ResumeLayout(false);
            tblRequest.PerformLayout();
            grpKyVong.ResumeLayout(false);
            tblKyVong.ResumeLayout(false);
            tblKyVong.PerformLayout();
            pnlBottom.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlContent;
        private TableLayoutPanel tblRoot;
        private Panel pnlHeader;
        private Label lblGoiY;
        private Label lblTieuDe;
        private GroupBox grpThongTinChung;
        private TableLayoutPanel tblThongTinChung;
        private Label lblMa;
        private TextBox txtMa;
        private Label lblNhom;
        private TextBox txtNhom;
        private Label lblTen;
        private TextBox txtTen;
        private Label lblMoTa;
        private TextBox txtMoTa;
        private Label lblMethod;
        private ComboBox cboMethod;
        private Label lblEndpoint;
        private TextBox txtEndpoint;
        private Label lblAuthMode;
        private ComboBox cboAuthMode;
        private GroupBox grpRequest;
        private TableLayoutPanel tblRequest;
        private Label lblPathParamsJson;
        private TextBox txtPathParamsJson;
        private Label lblHeadersJson;
        private TextBox txtHeadersJson;
        private Label lblBodyJson;
        private TextBox txtBodyJson;
        private GroupBox grpKyVong;
        private TableLayoutPanel tblKyVong;
        private Label lblExpectedCodes;
        private TextBox txtExpectedCodes;
        private Label lblExpectedHttpStatus;
        private TextBox txtExpectedHttpStatus;
        private Label lblExpectedJsonPath;
        private TextBox txtExpectedJsonPath;
        private Label lblExpectedJsonValue;
        private TextBox txtExpectedJsonValue;
        private Panel pnlBottom;
        private Button btnHuy;
        private Button btnLuu;
    }
}
