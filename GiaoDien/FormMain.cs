using ApiTest.Control;

namespace ApiTest
{
    public partial class FormMain : Form
    {
        private static readonly Color DefaultButtonColor = Color.FromArgb(44, 62, 80);
        private static readonly Color ActiveButtonColor = Color.FromArgb(52, 73, 94);

        public FormMain()
        {
            InitializeComponent();
            KhoiTaoDieuHuong();
            Shown += async (_, _) => await MoTongQuanAsync();
        }

        private void KhoiTaoDieuHuong()
        {

            btnTongQuan.Click += async (_, _) => await MoTongQuanAsync();
            btnChayTest.Click += async (_, _) => await MoChayTestAsync(null, null);
            btnBoSuuTap.Click += async (_, _) => await MoBoSuuTapAsync();
 
            btnLichSu.Click += async (_, _) => await MoLichSuAsync();

            boSuuTap1.OpenApiTestRequested += async (_, args) =>
            {
                await chayTest1.DatLaiBoLocAsync(null, null);
                await MoManHinhAsync(chayTest1, btnChayTest, "Chay test");
            };

            boSuuTap1.OpenRunTestRequested += async (_, args) =>
            {
                await chayTest1.DatLaiBoLocAsync(args.IdBoSuuTap, args.IdMoDun);
                await MoManHinhAsync(chayTest1, btnChayTest, "Chay test");
            };

  
        }

        private Task MoTongQuanAsync()
        {
            return MoManHinhAsync(tongQuan1, btnTongQuan, "Tong quan", tongQuan1.NapDuLieuAsync);
        }

        private Task MoKiemThuTuDoAsync()
        {
            return MoChayTestAsync(null, null);
        }

        private Task MoChayTestAsync(int? idBoSuuTap, int? idMoDun)
        {
            return MoManHinhAsync(
                chayTest1,
                btnChayTest,
                "Chay test",
                () => chayTest1.DatLaiBoLocAsync(idBoSuuTap, idMoDun));
        }

        private Task MoBoSuuTapAsync()
        {
            return MoManHinhAsync(boSuuTap1, btnBoSuuTap, "Bo suu tap", boSuuTap1.NapCayAsync);
        }

        private Task MoLichSuAsync()
        {
            return MoManHinhAsync(lichSu1, btnLichSu, "Lich su", lichSu1.NapDanhSachAsync);
        }

        private async Task MoManHinhAsync( System.Windows.Forms.Control manHinh, Button nutDangChon, string tieuDe,
            Func<Task>? taiDuLieu = null)
        {
            if (taiDuLieu != null)
            {
                await taiDuLieu();
            }

            manHinh.Visible = true;
            manHinh.BringToFront();

            btnTongQuan.BackColor = DefaultButtonColor; 
            btnChayTest.BackColor = DefaultButtonColor;
            btnBoSuuTap.BackColor = DefaultButtonColor;
            btnLichSu.BackColor = DefaultButtonColor;
            nutDangChon.BackColor = ActiveButtonColor;

            pnlVienActive.Height = nutDangChon.Height;
            pnlVienActive.Top = nutDangChon.Top;
            pnlVienActive.BringToFront();

            Text = $"API TESTER - {tieuDe}";
        }
    }
}
