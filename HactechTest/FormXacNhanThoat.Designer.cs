namespace HactechTest
{
    partial class FormXacNhanThoat
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
            lblBieuTuong = new Label();
            lblNoiDung = new Label();
            pnlChan = new Panel();
            btnKiemTraLai = new Button();
            btnTiepTuc = new Button();
            pnlChan.SuspendLayout();
            SuspendLayout();
            // 
            // lblBieuTuong
            // 
            lblBieuTuong.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            lblBieuTuong.ForeColor = Color.FromArgb(220, 160, 20);
            lblBieuTuong.Location = new Point(24, 28);
            lblBieuTuong.Name = "lblBieuTuong";
            lblBieuTuong.Size = new Size(52, 58);
            lblBieuTuong.TabIndex = 0;
            lblBieuTuong.Text = "!";
            lblBieuTuong.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblNoiDung
            // 
            lblNoiDung.Font = new Font("Segoe UI", 10F);
            lblNoiDung.Location = new Point(92, 28);
            lblNoiDung.Name = "lblNoiDung";
            lblNoiDung.Size = new Size(440, 95);
            lblNoiDung.TabIndex = 1;
            lblNoiDung.Text = "Nội dung xác nhận";
            lblNoiDung.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlChan
            // 
            pnlChan.BackColor = Color.FromArgb(245, 245, 245);
            pnlChan.Controls.Add(btnKiemTraLai);
            pnlChan.Controls.Add(btnTiepTuc);
            pnlChan.Dock = DockStyle.Bottom;
            pnlChan.Location = new Point(0, 142);
            pnlChan.Name = "pnlChan";
            pnlChan.Size = new Size(560, 68);
            pnlChan.TabIndex = 2;
            // 
            // btnKiemTraLai
            // 
            btnKiemTraLai.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnKiemTraLai.DialogResult = DialogResult.Cancel;
            btnKiemTraLai.Cursor = Cursors.Hand;
            btnKiemTraLai.FlatStyle = FlatStyle.Flat;
            btnKiemTraLai.Location = new Point(248, 18);
            btnKiemTraLai.Name = "btnKiemTraLai";
            btnKiemTraLai.Size = new Size(126, 32);
            btnKiemTraLai.TabIndex = 0;
            btnKiemTraLai.Text = "Kiểm tra lại";
            btnKiemTraLai.UseVisualStyleBackColor = true;
            // 
            // btnTiepTuc
            // 
            btnTiepTuc.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTiepTuc.BackColor = Color.FromArgb(66, 84, 190);
            btnTiepTuc.Cursor = Cursors.Hand;
            btnTiepTuc.DialogResult = DialogResult.OK;
            btnTiepTuc.FlatAppearance.BorderSize = 0;
            btnTiepTuc.FlatAppearance.MouseDownBackColor = Color.FromArgb(49, 63, 160);
            btnTiepTuc.FlatAppearance.MouseOverBackColor = Color.FromArgb(79, 99, 215);
            btnTiepTuc.FlatStyle = FlatStyle.Flat;
            btnTiepTuc.ForeColor = Color.White;
            btnTiepTuc.Location = new Point(386, 18);
            btnTiepTuc.Name = "btnTiepTuc";
            btnTiepTuc.Size = new Size(150, 32);
            btnTiepTuc.TabIndex = 1;
            btnTiepTuc.Text = "Tiếp tục";
            btnTiepTuc.UseVisualStyleBackColor = false;
            // 
            // FormXacNhanThoat
            // 
            AcceptButton = btnTiepTuc;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnKiemTraLai;
            ClientSize = new Size(560, 210);
            ControlBox = false;
            Controls.Add(pnlChan);
            Controls.Add(lblNoiDung);
            Controls.Add(lblBieuTuong);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormXacNhanThoat";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "HACTECH TEST";
            pnlChan.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Label lblBieuTuong;
        private Label lblNoiDung;
        private Panel pnlChan;
        private Button btnKiemTraLai;
        private Button btnTiepTuc;
    }
}
