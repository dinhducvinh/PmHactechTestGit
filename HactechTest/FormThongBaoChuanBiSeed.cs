namespace HactechTest
{
    public sealed partial class FormThongBaoChuanBiSeed : Form
    {
        public FormThongBaoChuanBiSeed()
        {
            InitializeComponent();
        }

        public void CapNhatTrangThai(string noiDung)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => CapNhatTrangThai(noiDung));
                return;
            }

            lblTrangThai.Text = noiDung;
            prgDangXuLy.Refresh();
        }
    }
}
