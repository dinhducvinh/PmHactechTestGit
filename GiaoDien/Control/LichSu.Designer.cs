namespace ApiTest.Control
{
    partial class LichSu
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblPageTitle = new Label();
            pnlHeader = new Panel();
            btnClearHistory = new Button();
            txtSearch = new TextBox();
            lblSearch = new Label();
            pnlToolbar = new Panel();
            tblToolbarLayout = new TableLayoutPanel();
            lblSelectedSession = new Label();
            colSessAvg = new DataGridViewTextBoxColumn();
            colSessRate = new DataGridViewTextBoxColumn();
            colSessFail = new DataGridViewTextBoxColumn();
            colSessPass = new DataGridViewTextBoxColumn();
            colSessTotal = new DataGridViewTextBoxColumn();
            colSessOs = new DataGridViewTextBoxColumn();
            colSessMachine = new DataGridViewTextBoxColumn();
            colSessUser = new DataGridViewTextBoxColumn();
            colSessAt = new DataGridViewTextBoxColumn();
            gridSessions = new DataGridView();
            splitHistory = new SplitContainer();
            gridSessionDetail = new DataGridView();
            colDetailNo = new DataGridViewTextBoxColumn();
            colDetailName = new DataGridViewTextBoxColumn();
            colDetailResult = new DataGridViewTextBoxColumn();
            colDetailStatus = new DataGridViewTextBoxColumn();
            colDetailTime = new DataGridViewTextBoxColumn();
            colDetailReason = new DataGridViewTextBoxColumn();
            pnlContent = new Panel();
            pnlHeader.SuspendLayout();
            pnlToolbar.SuspendLayout();
            tblToolbarLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridSessions).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitHistory).BeginInit();
            splitHistory.Panel1.SuspendLayout();
            splitHistory.Panel2.SuspendLayout();
            splitHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridSessionDetail).BeginInit();
            pnlContent.SuspendLayout();
            SuspendLayout();
            // 
            // lblPageTitle
            // 
            lblPageTitle.AutoSize = true;
            lblPageTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblPageTitle.Location = new Point(16, 12);
            lblPageTitle.Name = "lblPageTitle";
            lblPageTitle.Size = new Size(180, 41);
            lblPageTitle.TabIndex = 0;
            lblPageTitle.Text = "🕓  History";
            // 
            // pnlHeader
            // 
            pnlHeader.Controls.Add(lblPageTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(20, 20);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1624, 64);
            pnlHeader.TabIndex = 0;
            // 
            // btnClearHistory
            // 
            btnClearHistory.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearHistory.BackColor = Color.FromArgb(220, 53, 69);
            btnClearHistory.FlatAppearance.BorderSize = 0;
            btnClearHistory.FlatStyle = FlatStyle.Flat;
            btnClearHistory.ForeColor = Color.White;
            btnClearHistory.Location = new Point(1446, 16);
            btnClearHistory.Margin = new Padding(16, 0, 0, 0);
            btnClearHistory.Name = "btnClearHistory";
            btnClearHistory.Size = new Size(160, 32);
            btnClearHistory.TabIndex = 2;
            btnClearHistory.Text = "🗑 Xoá lịch sử";
            btnClearHistory.UseVisualStyleBackColor = false;
            // 
            // txtSearch
            // 
            txtSearch.Dock = DockStyle.Fill;
            txtSearch.Location = new Point(101, 16);
            txtSearch.Margin = new Padding(0);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Tìm theo người chạy / tên máy...";
            txtSearch.Size = new Size(1329, 27);
            txtSearch.TabIndex = 1;
            // 
            // lblSearch
            // 
            lblSearch.Anchor = AnchorStyles.Left;
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(16, 25);
            lblSearch.Margin = new Padding(0, 4, 12, 0);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(73, 20);
            lblSearch.TabIndex = 0;
            lblSearch.Text = "Tìm kiếm:";
            // 
            // pnlToolbar
            // 
            pnlToolbar.BackColor = Color.White;
            pnlToolbar.BorderStyle = BorderStyle.FixedSingle;
            pnlToolbar.Controls.Add(tblToolbarLayout);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(20, 84);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Size = new Size(1624, 68);
            pnlToolbar.TabIndex = 1;
            // 
            // tblToolbarLayout
            // 
            tblToolbarLayout.ColumnCount = 3;
            tblToolbarLayout.ColumnStyles.Add(new ColumnStyle());
            tblToolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblToolbarLayout.ColumnStyles.Add(new ColumnStyle());
            tblToolbarLayout.Controls.Add(lblSearch, 0, 0);
            tblToolbarLayout.Controls.Add(txtSearch, 1, 0);
            tblToolbarLayout.Controls.Add(btnClearHistory, 2, 0);
            tblToolbarLayout.Dock = DockStyle.Fill;
            tblToolbarLayout.Location = new Point(0, 0);
            tblToolbarLayout.Margin = new Padding(0);
            tblToolbarLayout.Name = "tblToolbarLayout";
            tblToolbarLayout.Padding = new Padding(16);
            tblToolbarLayout.RowCount = 1;
            tblToolbarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblToolbarLayout.Size = new Size(1622, 66);
            tblToolbarLayout.TabIndex = 0;
            // 
            // lblSelectedSession
            // 
            lblSelectedSession.Dock = DockStyle.Fill;
            lblSelectedSession.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            lblSelectedSession.Location = new Point(0, 0);
            lblSelectedSession.Name = "lblSelectedSession";
            lblSelectedSession.Padding = new Padding(8, 6, 0, 0);
            lblSelectedSession.Size = new Size(1624, 337);
            lblSelectedSession.TabIndex = 0;
            lblSelectedSession.Text = "Chọn 1 phiên ở bảng trên để xem chi tiết";
            // 
            // colSessAvg
            // 
            colSessAvg.HeaderText = "TG TB (ms)";
            colSessAvg.MinimumWidth = 6;
            colSessAvg.Name = "colSessAvg";
            colSessAvg.ReadOnly = true;
            colSessAvg.Width = 125;
            // 
            // colSessRate
            // 
            colSessRate.HeaderText = "Tỷ lệ";
            colSessRate.MinimumWidth = 6;
            colSessRate.Name = "colSessRate";
            colSessRate.ReadOnly = true;
            colSessRate.Width = 80;
            // 
            // colSessFail
            // 
            colSessFail.HeaderText = "K.Đạt";
            colSessFail.MinimumWidth = 6;
            colSessFail.Name = "colSessFail";
            colSessFail.ReadOnly = true;
            colSessFail.Width = 70;
            // 
            // colSessPass
            // 
            colSessPass.HeaderText = "Đạt";
            colSessPass.MinimumWidth = 6;
            colSessPass.Name = "colSessPass";
            colSessPass.ReadOnly = true;
            colSessPass.Width = 60;
            // 
            // colSessTotal
            // 
            colSessTotal.HeaderText = "Tổng";
            colSessTotal.MinimumWidth = 6;
            colSessTotal.Name = "colSessTotal";
            colSessTotal.ReadOnly = true;
            colSessTotal.Width = 70;
            // 
            // colSessOs
            // 
            colSessOs.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colSessOs.HeaderText = "Hệ điều hành";
            colSessOs.MinimumWidth = 6;
            colSessOs.Name = "colSessOs";
            colSessOs.ReadOnly = true;
            // 
            // colSessMachine
            // 
            colSessMachine.HeaderText = "Tên máy";
            colSessMachine.MinimumWidth = 6;
            colSessMachine.Name = "colSessMachine";
            colSessMachine.ReadOnly = true;
            colSessMachine.Width = 140;
            // 
            // colSessUser
            // 
            colSessUser.HeaderText = "Người TH";
            colSessUser.MinimumWidth = 6;
            colSessUser.Name = "colSessUser";
            colSessUser.ReadOnly = true;
            colSessUser.Width = 140;
            // 
            // colSessAt
            // 
            colSessAt.HeaderText = "Thời điểm chạy";
            colSessAt.MinimumWidth = 6;
            colSessAt.Name = "colSessAt";
            colSessAt.ReadOnly = true;
            colSessAt.Width = 160;
            // 
            // gridSessions
            // 
            gridSessions.AllowUserToAddRows = false;
            gridSessions.AllowUserToDeleteRows = false;
            gridSessions.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridSessions.Columns.AddRange(new DataGridViewColumn[] { colSessAt, colSessUser, colSessMachine, colSessOs, colSessTotal, colSessPass, colSessFail, colSessRate, colSessAvg });
            gridSessions.Dock = DockStyle.Fill;
            gridSessions.Location = new Point(0, 0);
            gridSessions.Name = "gridSessions";
            gridSessions.ReadOnly = true;
            gridSessions.RowHeadersWidth = 51;
            gridSessions.RowTemplate.Height = 25;
            gridSessions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridSessions.Size = new Size(1624, 280);
            gridSessions.TabIndex = 0;
            // 
            // splitHistory
            // 
            splitHistory.Dock = DockStyle.Fill;
            splitHistory.Location = new Point(20, 152);
            splitHistory.Name = "splitHistory";
            splitHistory.Orientation = Orientation.Horizontal;
            // 
            // splitHistory.Panel1
            // 
            splitHistory.Panel1.Controls.Add(gridSessions);
            // 
            // splitHistory.Panel2
            // 
            splitHistory.Panel2.Controls.Add(gridSessionDetail);
            splitHistory.Panel2.Controls.Add(lblSelectedSession);
            splitHistory.Size = new Size(1624, 621);
            splitHistory.SplitterDistance = 280;
            splitHistory.TabIndex = 2;
            // 
            // gridSessionDetail
            // 
            gridSessionDetail.AllowUserToAddRows = false;
            gridSessionDetail.AllowUserToDeleteRows = false;
            gridSessionDetail.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridSessionDetail.Columns.AddRange(new DataGridViewColumn[] { colDetailNo, colDetailName, colDetailResult, colDetailStatus, colDetailTime, colDetailReason });
            gridSessionDetail.Dock = DockStyle.Fill;
            gridSessionDetail.Location = new Point(0, 0);
            gridSessionDetail.Name = "gridSessionDetail";
            gridSessionDetail.ReadOnly = true;
            gridSessionDetail.RowHeadersWidth = 51;
            gridSessionDetail.RowTemplate.Height = 25;
            gridSessionDetail.Size = new Size(1624, 337);
            gridSessionDetail.TabIndex = 1;
            // 
            // colDetailNo
            // 
            colDetailNo.HeaderText = "STT";
            colDetailNo.MinimumWidth = 6;
            colDetailNo.Name = "colDetailNo";
            colDetailNo.ReadOnly = true;
            colDetailNo.Width = 50;
            // 
            // colDetailName
            // 
            colDetailName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colDetailName.FillWeight = 38F;
            colDetailName.HeaderText = "Tên Test";
            colDetailName.MinimumWidth = 180;
            colDetailName.Name = "colDetailName";
            colDetailName.ReadOnly = true;
            // 
            // colDetailResult
            // 
            colDetailResult.HeaderText = "Kết quả";
            colDetailResult.MinimumWidth = 6;
            colDetailResult.Name = "colDetailResult";
            colDetailResult.ReadOnly = true;
            colDetailResult.Width = 80;
            // 
            // colDetailStatus
            // 
            colDetailStatus.HeaderText = "Mã TT";
            colDetailStatus.MinimumWidth = 6;
            colDetailStatus.Name = "colDetailStatus";
            colDetailStatus.ReadOnly = true;
            colDetailStatus.Width = 80;
            // 
            // colDetailTime
            // 
            colDetailTime.HeaderText = "TG (ms)";
            colDetailTime.MinimumWidth = 6;
            colDetailTime.Name = "colDetailTime";
            colDetailTime.ReadOnly = true;
            colDetailTime.Width = 80;
            // 
            // colDetailReason
            // 
            colDetailReason.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colDetailReason.FillWeight = 62F;
            colDetailReason.HeaderText = "Lý do";
            colDetailReason.MinimumWidth = 320;
            colDetailReason.Name = "colDetailReason";
            colDetailReason.ReadOnly = true;
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.FromArgb(244, 245, 247);
            pnlContent.Controls.Add(splitHistory);
            pnlContent.Controls.Add(pnlToolbar);
            pnlContent.Controls.Add(pnlHeader);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 0);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(20);
            pnlContent.Size = new Size(1664, 793);
            pnlContent.TabIndex = 2;
            // 
            // LichSu
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlContent);
            Name = "LichSu";
            Size = new Size(1664, 793);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlToolbar.ResumeLayout(false);
            tblToolbarLayout.ResumeLayout(false);
            tblToolbarLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gridSessions).EndInit();
            splitHistory.Panel1.ResumeLayout(false);
            splitHistory.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitHistory).EndInit();
            splitHistory.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridSessionDetail).EndInit();
            pnlContent.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Label lblPageTitle;
        private Panel pnlHeader;
        private Button btnClearHistory;
        private TextBox txtSearch;
        private Label lblSearch;
        private Panel pnlToolbar;
        private TableLayoutPanel tblToolbarLayout;
        private Label lblSelectedSession;
        private DataGridViewTextBoxColumn colSessAvg;
        private DataGridViewTextBoxColumn colSessRate;
        private DataGridViewTextBoxColumn colSessFail;
        private DataGridViewTextBoxColumn colSessPass;
        private DataGridViewTextBoxColumn colSessTotal;
        private DataGridViewTextBoxColumn colSessOs;
        private DataGridViewTextBoxColumn colSessMachine;
        private DataGridViewTextBoxColumn colSessUser;
        private DataGridViewTextBoxColumn colSessAt;
        private DataGridView gridSessions;
        private SplitContainer splitHistory;
        private DataGridView gridSessionDetail;
        private Panel pnlContent;
        private DataGridViewTextBoxColumn colDetailNo;
        private DataGridViewTextBoxColumn colDetailName;
        private DataGridViewTextBoxColumn colDetailResult;
        private DataGridViewTextBoxColumn colDetailStatus;
        private DataGridViewTextBoxColumn colDetailTime;
        private DataGridViewTextBoxColumn colDetailReason;
    }
}
