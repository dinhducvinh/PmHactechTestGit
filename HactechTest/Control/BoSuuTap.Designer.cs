namespace HactechTest.Control
{
    partial class BoSuuTap
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
            pnlNoiDung = new Panel();
            treCayBoSuuTap = new TreeView();
            pnlThanhCongCu = new Panel();
            tblToolbarLayout = new TableLayoutPanel();
            flpToolbarActions = new FlowLayoutPanel();
            btnThemApi = new Button();
            btnSuaTestCase = new Button();
            btnXoaNode = new Button();
            btnMoChayTest = new Button();
            lblGoiY = new Label();
            pnlDauTrang = new Panel();
            lblTieuDeTrang = new Label();
            pnlNoiDung.SuspendLayout();
            pnlThanhCongCu.SuspendLayout();
            tblToolbarLayout.SuspendLayout();
            flpToolbarActions.SuspendLayout();
            pnlDauTrang.SuspendLayout();
            SuspendLayout();
            // 
            // pnlNoiDung
            // 
            pnlNoiDung.BackColor = Color.FromArgb(244, 245, 247);
            pnlNoiDung.Controls.Add(treCayBoSuuTap);
            pnlNoiDung.Controls.Add(pnlThanhCongCu);
            pnlNoiDung.Controls.Add(pnlDauTrang);
            pnlNoiDung.Dock = DockStyle.Fill;
            pnlNoiDung.Location = new Point(0, 0);
            pnlNoiDung.Name = "pnlNoiDung";
            pnlNoiDung.Padding = new Padding(20);
            pnlNoiDung.Size = new Size(1664, 793);
            pnlNoiDung.TabIndex = 2;
            // 
            // treCayBoSuuTap
            // 
            treCayBoSuuTap.BackColor = Color.White;
            treCayBoSuuTap.BorderStyle = BorderStyle.FixedSingle;
            treCayBoSuuTap.Dock = DockStyle.Fill;
            treCayBoSuuTap.Font = new Font("Segoe UI", 10F);
            treCayBoSuuTap.ForeColor = Color.FromArgb(24, 30, 37);
            treCayBoSuuTap.FullRowSelect = true;
            treCayBoSuuTap.HideSelection = false;
            treCayBoSuuTap.ItemHeight = 28;
            treCayBoSuuTap.Location = new Point(20, 168);
            treCayBoSuuTap.Name = "treCayBoSuuTap";
            treCayBoSuuTap.Size = new Size(1624, 605);
            treCayBoSuuTap.TabIndex = 2;
            // 
            // pnlThanhCongCu
            // 
            pnlThanhCongCu.BackColor = Color.White;
            pnlThanhCongCu.BorderStyle = BorderStyle.FixedSingle;
            pnlThanhCongCu.Controls.Add(tblToolbarLayout);
            pnlThanhCongCu.Dock = DockStyle.Top;
            pnlThanhCongCu.Location = new Point(20, 84);
            pnlThanhCongCu.MinimumSize = new Size(0, 84);
            pnlThanhCongCu.Name = "pnlThanhCongCu";
            pnlThanhCongCu.Size = new Size(1624, 84);
            pnlThanhCongCu.TabIndex = 1;
            // 
            // tblToolbarLayout
            // 
            tblToolbarLayout.ColumnCount = 1;
            tblToolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblToolbarLayout.Controls.Add(flpToolbarActions, 0, 0);
            tblToolbarLayout.Controls.Add(lblGoiY, 0, 1);
            tblToolbarLayout.Dock = DockStyle.Fill;
            tblToolbarLayout.Location = new Point(0, 0);
            tblToolbarLayout.Margin = Padding.Empty;
            tblToolbarLayout.Name = "tblToolbarLayout";
            tblToolbarLayout.Padding = new Padding(16, 12, 16, 12);
            tblToolbarLayout.RowCount = 2;
            tblToolbarLayout.RowStyles.Add(new RowStyle());
            tblToolbarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblToolbarLayout.Size = new Size(1622, 82);
            tblToolbarLayout.TabIndex = 0;
            // 
            // flpToolbarActions
            // 
            flpToolbarActions.AutoSize = true;
            flpToolbarActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpToolbarActions.Controls.Add(btnThemApi);
            flpToolbarActions.Controls.Add(btnSuaTestCase);
            flpToolbarActions.Controls.Add(btnXoaNode);
            flpToolbarActions.Controls.Add(btnMoChayTest);
            flpToolbarActions.Dock = DockStyle.Top;
            flpToolbarActions.Location = new Point(16, 12);
            flpToolbarActions.Margin = Padding.Empty;
            flpToolbarActions.Name = "flpToolbarActions";
            flpToolbarActions.Size = new Size(1590, 40);
            flpToolbarActions.TabIndex = 0;
            flpToolbarActions.WrapContents = true;
            // 
            // btnThemApi
            // 
            btnThemApi.Cursor = Cursors.Hand;
            btnThemApi.FlatStyle = FlatStyle.Flat;
            btnThemApi.Location = new Point(0, 0);
            btnThemApi.Margin = new Padding(0, 0, 8, 8);
            btnThemApi.Name = "btnThemApi";
            btnThemApi.Size = new Size(116, 32);
            btnThemApi.TabIndex = 0;
            btnThemApi.Text = "+ Thêm API";
            btnThemApi.UseVisualStyleBackColor = true;
            btnThemApi.Click += BtnThemApi_Click;
            // 
            // btnSuaTestCase
            // 
            btnSuaTestCase.BackColor = Color.FromArgb(108, 117, 125);
            btnSuaTestCase.Cursor = Cursors.Hand;
            btnSuaTestCase.FlatAppearance.BorderSize = 0;
            btnSuaTestCase.FlatAppearance.MouseDownBackColor = Color.FromArgb(82, 90, 98);
            btnSuaTestCase.FlatAppearance.MouseOverBackColor = Color.FromArgb(126, 136, 146);
            btnSuaTestCase.FlatStyle = FlatStyle.Flat;
            btnSuaTestCase.ForeColor = Color.White;
            btnSuaTestCase.Location = new Point(124, 0);
            btnSuaTestCase.Margin = new Padding(0, 0, 8, 8);
            btnSuaTestCase.Name = "btnSuaTestCase";
            btnSuaTestCase.Size = new Size(132, 32);
            btnSuaTestCase.TabIndex = 1;
            btnSuaTestCase.Text = "⚙ Sửa Test Case";
            btnSuaTestCase.UseVisualStyleBackColor = false;
            btnSuaTestCase.Click += BtnSuaTestCase_Click;
            // 
            // btnXoaNode
            // 
            btnXoaNode.BackColor = Color.FromArgb(220, 53, 69);
            btnXoaNode.Cursor = Cursors.Hand;
            btnXoaNode.FlatAppearance.BorderSize = 0;
            btnXoaNode.FlatAppearance.MouseDownBackColor = Color.FromArgb(184, 36, 49);
            btnXoaNode.FlatAppearance.MouseOverBackColor = Color.FromArgb(230, 72, 87);
            btnXoaNode.FlatStyle = FlatStyle.Flat;
            btnXoaNode.ForeColor = Color.White;
            btnXoaNode.Location = new Point(264, 0);
            btnXoaNode.Margin = new Padding(0, 0, 8, 8);
            btnXoaNode.Name = "btnXoaNode";
            btnXoaNode.Size = new Size(120, 32);
            btnXoaNode.TabIndex = 2;
            btnXoaNode.Text = "✕ Xoá node";
            btnXoaNode.UseVisualStyleBackColor = false;
            // 
            // btnMoChayTest
            // 
            btnMoChayTest.BackColor = Color.FromArgb(40, 167, 69);
            btnMoChayTest.Cursor = Cursors.Hand;
            btnMoChayTest.FlatAppearance.BorderSize = 0;
            btnMoChayTest.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 130, 54);
            btnMoChayTest.FlatAppearance.MouseOverBackColor = Color.FromArgb(48, 185, 81);
            btnMoChayTest.FlatStyle = FlatStyle.Flat;
            btnMoChayTest.ForeColor = Color.White;
            btnMoChayTest.Location = new Point(392, 0);
            btnMoChayTest.Margin = new Padding(0, 0, 8, 8);
            btnMoChayTest.Name = "btnMoChayTest";
            btnMoChayTest.Size = new Size(160, 32);
            btnMoChayTest.TabIndex = 3;
            btnMoChayTest.Text = "▶ Mở Chạy Test";
            btnMoChayTest.UseVisualStyleBackColor = false;
            btnMoChayTest.Click += BtnMoChayTest_Click;
            // 
            // lblGoiY
            // 
            lblGoiY.AutoEllipsis = true;
            lblGoiY.Dock = DockStyle.Fill;
            lblGoiY.Font = new Font("Segoe UI", 8.25F, FontStyle.Italic);
            lblGoiY.ForeColor = Color.Gray;
            lblGoiY.Location = new Point(16, 52);
            lblGoiY.Margin = Padding.Empty;
            lblGoiY.Name = "lblGoiY";
            lblGoiY.Size = new Size(1590, 18);
            lblGoiY.TabIndex = 1;
            lblGoiY.Text = "Chọn test case rồi bấm Sửa Test Case để cấu hình. Bấm Mở Chạy Test khi muốn chuyển sang màn chạy.";
            lblGoiY.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlDauTrang
            // 
            pnlDauTrang.Controls.Add(lblTieuDeTrang);
            pnlDauTrang.Dock = DockStyle.Top;
            pnlDauTrang.Location = new Point(20, 20);
            pnlDauTrang.Name = "pnlDauTrang";
            pnlDauTrang.Size = new Size(1624, 64);
            pnlDauTrang.TabIndex = 0;
            // 
            // lblTieuDeTrang
            // 
            lblTieuDeTrang.AutoSize = true;
            lblTieuDeTrang.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTieuDeTrang.Location = new Point(16, 12);
            lblTieuDeTrang.Name = "lblTieuDeTrang";
            lblTieuDeTrang.Size = new Size(227, 41);
            lblTieuDeTrang.TabIndex = 0;
            lblTieuDeTrang.Text = "📂  Bộ sưu tập";
            // 
            // BoSuuTap
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlNoiDung);
            Name = "BoSuuTap";
            Size = new Size(1664, 793);
            pnlNoiDung.ResumeLayout(false);
            pnlThanhCongCu.ResumeLayout(false);
            tblToolbarLayout.ResumeLayout(false);
            tblToolbarLayout.PerformLayout();
            flpToolbarActions.ResumeLayout(false);
            pnlDauTrang.ResumeLayout(false);
            pnlDauTrang.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlNoiDung;
        private TreeView treCayBoSuuTap;
        private Panel pnlThanhCongCu;
        private TableLayoutPanel tblToolbarLayout;
        private FlowLayoutPanel flpToolbarActions;
        private Button btnThemApi;
        private Button btnSuaTestCase;
        private Button btnXoaNode;
        private Button btnMoChayTest;
        private Label lblGoiY;
        private Panel pnlDauTrang;
        private Label lblTieuDeTrang;
    }
}

