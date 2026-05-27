namespace ApiTest
{
    public partial class FormInputBox : Form
    {
        public string InputText => txtInput.Text.Trim();

        public FormInputBox(string title, string prompt, string placeholder, string submitText)
        {
            InitializeComponent();

            Text = title;
            lblPrompt.Text = prompt;
            txtInput.PlaceholderText = placeholder;
            btnSubmit.Text = submitText;
        }

        private void BtnSubmit_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text))
            {
                MessageBox.Show(
                    "Vui lòng nhập nội dung trước khi tiếp tục.",
                    "Thiếu thông tin",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtInput.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
