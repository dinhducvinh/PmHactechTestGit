namespace ApiTest
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
            btnLichSu = new Button();
            btnBoSuuTap = new Button();
            btnChayTest = new Button();
            btnTongQuan = new Button();
            pnlVienActive = new Panel();
            lblSubtitle = new Label();
            lblLogo = new Label();
            pnlNoiDungChinh = new Panel();
            lichSu1 = new ApiTest.Control.LichSu();
            chayTest1 = new ApiTest.Control.ChayTest();
            boSuuTap1 = new ApiTest.Control.BoSuuTap();
            tongQuan1 = new ApiTest.Control.TongQuan();
            pnlSidebar.SuspendLayout();
            pnlNoiDungChinh.SuspendLayout();
            SuspendLayout();
            // 
            // pnlSidebar
            // 
            pnlSidebar.BackColor = Color.FromArgb(31, 43, 56);
            pnlSidebar.Controls.Add(btnLichSu);
            pnlSidebar.Controls.Add(btnBoSuuTap);
            pnlSidebar.Controls.Add(btnChayTest);
            pnlSidebar.Controls.Add(btnTongQuan);
            pnlSidebar.Controls.Add(pnlVienActive);
            pnlSidebar.Controls.Add(lblSubtitle);
            pnlSidebar.Controls.Add(lblLogo);
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Location = new Point(0, 0);
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Size = new Size(248, 793);
            pnlSidebar.TabIndex = 0;
            // 
            // btnLichSu
            // 
            btnLichSu.BackColor = Color.FromArgb(44, 62, 80);
            btnLichSu.FlatAppearance.BorderSize = 0;
            btnLichSu.FlatStyle = FlatStyle.Flat;
            btnLichSu.Font = new Font("Segoe UI", 10F);
            btnLichSu.ForeColor = Color.WhiteSmoke;
            btnLichSu.Location = new Point(0, 290);
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
            btnBoSuuTap.BackColor = Color.FromArgb(44, 62, 80);
            btnBoSuuTap.FlatAppearance.BorderSize = 0;
            btnBoSuuTap.FlatStyle = FlatStyle.Flat;
            btnBoSuuTap.Font = new Font("Segoe UI", 10F);
            btnBoSuuTap.ForeColor = Color.WhiteSmoke;
            btnBoSuuTap.Location = new Point(0, 244);
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
            btnChayTest.BackColor = Color.FromArgb(44, 62, 80);
            btnChayTest.FlatAppearance.BorderSize = 0;
            btnChayTest.FlatStyle = FlatStyle.Flat;
            btnChayTest.Font = new Font("Segoe UI", 10F);
            btnChayTest.ForeColor = Color.WhiteSmoke;
            btnChayTest.Location = new Point(0, 196);
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
            btnTongQuan.BackColor = Color.FromArgb(52, 73, 94);
            btnTongQuan.FlatAppearance.BorderSize = 0;
            btnTongQuan.FlatStyle = FlatStyle.Flat;
            btnTongQuan.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnTongQuan.ForeColor = Color.WhiteSmoke;
            btnTongQuan.Location = new Point(0, 148);
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
            pnlVienActive.BackColor = Color.FromArgb(46, 204, 113);
            pnlVienActive.Location = new Point(0, 148);
            pnlVienActive.Name = "pnlVienActive";
            pnlVienActive.Size = new Size(4, 48);
            pnlVienActive.TabIndex = 2;
            // 
            // lblSubtitle
            // 
            lblSubtitle.ForeColor = Color.FromArgb(181, 190, 199);
            lblSubtitle.Location = new Point(24, 72);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(192, 42);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Kiểm thử API, chạy test và theo dõi kết quả";
            // 
            // lblLogo
            // 
            lblLogo.AutoSize = true;
            lblLogo.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblLogo.ForeColor = Color.White;
            lblLogo.Location = new Point(24, 28);
            lblLogo.Name = "lblLogo";
            lblLogo.Size = new Size(180, 41);
            lblLogo.TabIndex = 0;
            lblLogo.Text = "API TESTER";
            // 
            // pnlNoiDungChinh
            // 
            pnlNoiDungChinh.BackColor = Color.FromArgb(244, 245, 247);
            pnlNoiDungChinh.Controls.Add(lichSu1);
            pnlNoiDungChinh.Controls.Add(chayTest1);

            pnlNoiDungChinh.Controls.Add(tongQuan1);
            pnlNoiDungChinh.Dock = DockStyle.Fill;
            pnlNoiDungChinh.Location = new Point(248, 0);
            pnlNoiDungChinh.Name = "pnlNoiDungChinh";
            pnlNoiDungChinh.Size = new Size(1676, 793);
            pnlNoiDungChinh.TabIndex = 1;
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
            Text = "API TESTER";
            pnlSidebar.ResumeLayout(false);
            pnlSidebar.PerformLayout();
            pnlNoiDungChinh.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlSidebar;
        private Button btnLichSu;
        private Button btnBoSuuTap;
        private Button btnChayTest;
        private Button btnTongQuan;
        private Panel pnlVienActive;
        private Label lblSubtitle;
        private Label lblLogo;
        private Panel pnlNoiDungChinh;
        private Control.TongQuan tongQuan1;
        private Control.LichSu lichSu1;
        private Control.ChayTest chayTest1;
        private Control.BoSuuTap boSuuTap1;
    }
}
