namespace HactechTest
{
    partial class FormThongBaoChuanBiSeed
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblTieuDe = new Label();
            lblTrangThai = new Label();
            prgDangXuLy = new ProgressBar();
            SuspendLayout();
            // 
            // lblTieuDe
            // 
            lblTieuDe.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTieuDe.Location = new Point(20, 18);
            lblTieuDe.Name = "lblTieuDe";
            lblTieuDe.Size = new Size(480, 28);
            lblTieuDe.TabIndex = 0;
            lblTieuDe.Text = "Đang kiểm tra database seed";
            lblTieuDe.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTrangThai
            // 
            lblTrangThai.Location = new Point(20, 52);
            lblTrangThai.Name = "lblTrangThai";
            lblTrangThai.Size = new Size(480, 34);
            lblTrangThai.TabIndex = 1;
            lblTrangThai.Text = "Đang khởi tạo...";
            lblTrangThai.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // prgDangXuLy
            // 
            prgDangXuLy.Location = new Point(20, 92);
            prgDangXuLy.MarqueeAnimationSpeed = 35;
            prgDangXuLy.Name = "prgDangXuLy";
            prgDangXuLy.Size = new Size(480, 18);
            prgDangXuLy.Style = ProgressBarStyle.Marquee;
            prgDangXuLy.TabIndex = 2;
            // 
            // FormThongBaoChuanBiSeed
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(520, 130);
            ControlBox = false;
            Controls.Add(prgDangXuLy);
            Controls.Add(lblTrangThai);
            Controls.Add(lblTieuDe);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormThongBaoChuanBiSeed";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Chuẩn bị dữ liệu seed";
            ResumeLayout(false);
        }

        #endregion

        private Label lblTieuDe;
        private Label lblTrangThai;
        private ProgressBar prgDangXuLy;
    }
}
