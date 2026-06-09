namespace HactechTest
{
    public sealed partial class FormXacNhanThoat : Form
    {
        public FormXacNhanThoat(string noiDung, string tenNutTiepTuc)
        {
            InitializeComponent();
            lblNoiDung.Text = noiDung;
            btnTiepTuc.Text = tenNutTiepTuc;
        }

        public static bool XacNhan(IWin32Window owner, string noiDung, string tenNutTiepTuc)
        {
            using var form = new FormXacNhanThoat(noiDung, tenNutTiepTuc);
            return form.ShowDialog(owner) == DialogResult.OK;
        }
    }
}
