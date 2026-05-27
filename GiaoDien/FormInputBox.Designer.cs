namespace ApiTest
{
    partial class FormInputBox
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
            tblLayout = new TableLayoutPanel();
            lblPrompt = new Label();
            txtInput = new TextBox();
            pnlButtons = new FlowLayoutPanel();
            btnCancel = new Button();
            btnSubmit = new Button();
            pnlContent.SuspendLayout();
            tblLayout.SuspendLayout();
            pnlButtons.SuspendLayout();
            SuspendLayout();
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.FromArgb(244, 245, 247);
            pnlContent.Controls.Add(tblLayout);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 0);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(20);
            pnlContent.Size = new Size(484, 181);
            pnlContent.TabIndex = 0;
            // 
            // tblLayout
            // 
            tblLayout.ColumnCount = 1;
            tblLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblLayout.Controls.Add(lblPrompt, 0, 0);
            tblLayout.Controls.Add(txtInput, 0, 1);
            tblLayout.Controls.Add(pnlButtons, 0, 2);
            tblLayout.Dock = DockStyle.Fill;
            tblLayout.Location = new Point(20, 20);
            tblLayout.Margin = Padding.Empty;
            tblLayout.Name = "tblLayout";
            tblLayout.RowCount = 3;
            tblLayout.RowStyles.Add(new RowStyle());
            tblLayout.RowStyles.Add(new RowStyle());
            tblLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblLayout.Size = new Size(444, 141);
            tblLayout.TabIndex = 0;
            // 
            // lblPrompt
            // 
            lblPrompt.AutoSize = true;
            lblPrompt.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblPrompt.Location = new Point(0, 0);
            lblPrompt.Margin = Padding.Empty;
            lblPrompt.Name = "lblPrompt";
            lblPrompt.Size = new Size(177, 25);
            lblPrompt.TabIndex = 0;
            lblPrompt.Text = "Nhập nội dung cần tạo";
            // 
            // txtInput
            // 
            txtInput.Dock = DockStyle.Fill;
            txtInput.Location = new Point(0, 37);
            txtInput.Margin = new Padding(0, 12, 0, 0);
            txtInput.Name = "txtInput";
            txtInput.Size = new Size(444, 27);
            txtInput.TabIndex = 1;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Controls.Add(btnSubmit);
            pnlButtons.Dock = DockStyle.Fill;
            pnlButtons.FlowDirection = FlowDirection.RightToLeft;
            pnlButtons.Location = new Point(0, 80);
            pnlButtons.Margin = new Padding(0, 16, 0, 0);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(444, 61);
            pnlButtons.TabIndex = 2;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Location = new Point(349, 0);
            btnCancel.Margin = new Padding(12, 0, 0, 0);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(95, 36);
            btnCancel.TabIndex = 0;
            btnCancel.Text = "Hủy";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnSubmit
            // 
            btnSubmit.BackColor = Color.FromArgb(39, 174, 96);
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.FlatStyle = FlatStyle.Flat;
            btnSubmit.ForeColor = Color.White;
            btnSubmit.Location = new Point(242, 0);
            btnSubmit.Margin = new Padding(12, 0, 0, 0);
            btnSubmit.Name = "btnSubmit";
            btnSubmit.Size = new Size(95, 36);
            btnSubmit.TabIndex = 1;
            btnSubmit.Text = "Tạo";
            btnSubmit.UseVisualStyleBackColor = false;
            btnSubmit.Click += BtnSubmit_Click;
            // 
            // FormInputBox
            // 
            AcceptButton = btnSubmit;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(484, 181);
            Controls.Add(pnlContent);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormInputBox";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Nhập thông tin";
            pnlContent.ResumeLayout(false);
            tblLayout.ResumeLayout(false);
            tblLayout.PerformLayout();
            pnlButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlContent;
        private TableLayoutPanel tblLayout;
        private Label lblPrompt;
        private TextBox txtInput;
        private FlowLayoutPanel pnlButtons;
        private Button btnCancel;
        private Button btnSubmit;
    }
}
