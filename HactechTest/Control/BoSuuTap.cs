using HactechTest.ApiShopTesting.Core;
using HactechTest.ApiShopTesting.KichBan;
using HactechTest.Services.App;
using HactechTest.Services.DynamicTests;

namespace HactechTest.Control
{
    public partial class BoSuuTap : UserControl
    {
        public event EventHandler? OpenRunTestRequested;

        private readonly IReadOnlyList<KichBanApi> _kichBanCode = BoKichBanApi.TaoTatCaKichBan();
        private List<TestCaseDong> _testCaseDong = new();
        private bool _daKhoiTao;

        public BoSuuTap()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                Load += BoSuuTap_Load;
                btnTaoProject.Click += BtnTaoProject_Click;
                btnThemModule.Click += BtnThemModule_Click;
                btnXoaNode.Click += BtnXoaNode_Click;
            }
        }

        private void BoSuuTap_Load(object? sender, EventArgs e)
        {
            _ = NapCayAsync();
        }

        public async Task NapCayAsync()
        {
            if (_daKhoiTao && treCayBoSuuTap.Nodes.Count > 0)
            {
                return;
            }

            _testCaseDong = await LayDanhSachTestCaseDongAsync();

            treCayBoSuuTap.BeginUpdate();
            try
            {
                treCayBoSuuTap.Nodes.Clear();

                var nodeBst = new TreeNode("[COL] API Shop")
                {
                    Tag = new NodeTag("BST")
                };
                treCayBoSuuTap.Nodes.Add(nodeBst);

                var danhSachNhom = _kichBanCode.Select(x => x.Nhom)
                    .Concat(_testCaseDong.Select(x => x.Nhom))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList();

                foreach (var nhom in danhSachNhom)
                {
                    var nodeMod = new TreeNode("[MOD] " + nhom)
                    {
                        Tag = new NodeTag("MD", Nhom: nhom)
                    };
                    nodeBst.Nodes.Add(nodeMod);

                    foreach (var tc in _kichBanCode.Where(x => string.Equals(x.Nhom, nhom, StringComparison.OrdinalIgnoreCase)))
                    {
                        var n = new TreeNode($"[CODE] {tc.Ma} - {tc.TenHienThi}")
                        {
                            Tag = new NodeTag("TC_CODE", Nhom: tc.Nhom)
                        };
                        nodeMod.Nodes.Add(n);
                    }

                    foreach (var tc in _testCaseDong.Where(x => string.Equals(x.Nhom, nhom, StringComparison.OrdinalIgnoreCase)))
                    {
                        var n = new TreeNode($"[DB] {tc.Ma} - {tc.TenHienThi}")
                        {
                            Tag = new NodeTag("TC_DONG", tc.Id, tc.Nhom)
                        };
                        nodeMod.Nodes.Add(n);
                    }
                }

                treCayBoSuuTap.ExpandAll();
                if (treCayBoSuuTap.Nodes.Count > 0)
                {
                    treCayBoSuuTap.SelectedNode = treCayBoSuuTap.Nodes[0];
                }

                _daKhoiTao = true;
            }
            finally
            {
                treCayBoSuuTap.EndUpdate();
            }
        }

        private void BtnTaoProject_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "Hiện tại phần mềm tập trung vào bộ sưu tập 'API Shop'. " +
                "Test case cơ bản có thể thêm bằng nút '+ Thêm API'. " +
                "Test case phức tạp vẫn viết trong ApiShopTesting/KichBan.",
                "Thông báo");
        }

        private void BtnThemModule_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "Module được sinh theo trường 'Module/Nhóm' của test case. " +
                "Hãy nhập module mới khi thêm test case cơ bản.",
                "Thông báo");
        }

        private async void BtnXoaNode_Click(object? sender, EventArgs e)
        {
            if (treCayBoSuuTap.SelectedNode?.Tag is not NodeTag tag || tag.TestCaseDongId is null)
            {
                MessageBox.Show("Chỉ xóa được test case cơ bản lưu trong database. Test case [CODE] cần sửa trong source.", "Thông báo");
                return;
            }

            var confirm = MessageBox.Show(
                "Xóa test case cơ bản đang chọn khỏi database?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            var store = TaoStoreTestCaseDong();
            if (store is null)
            {
                return;
            }

            try
            {
                await store.XoaAsync(tag.TestCaseDongId.Value);
                _daKhoiTao = false;
                await NapCayAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không xóa được test case: " + ex.Message, "Lỗi");
            }
        }

        private async void BtnSuaTestCase_Click(object? sender, EventArgs e)
        {
            if (treCayBoSuuTap.SelectedNode?.Tag is not NodeTag tag)
            {
                MessageBox.Show("Chọn một test case để sửa.", "Thông báo");
                return;
            }

            if (tag.Loai == "TC_CODE")
            {
                MessageBox.Show(
                    "Đây là test case phức tạp viết bằng C#. " +
                    "Muốn sửa luồng token/upload/logic nhiều bước thì chỉnh trực tiếp trong ApiShopTesting/KichBan.",
                    "Thông báo");
                return;
            }

            if (tag.TestCaseDongId is null)
            {
                MessageBox.Show("Chọn một test case cơ bản lưu trong database để sửa.", "Thông báo");
                return;
            }

            var store = TaoStoreTestCaseDong();
            if (store is null)
            {
                return;
            }

            try
            {
                var testCase = await store.LayTheoIdAsync(tag.TestCaseDongId.Value);
                if (testCase is null)
                {
                    MessageBox.Show("Không tìm thấy test case trong database.", "Thông báo");
                    _daKhoiTao = false;
                    await NapCayAsync();
                    return;
                }

                using var form = new FormTestCaseDong(testCase);
                if (form.ShowDialog(FindForm()) != DialogResult.OK)
                {
                    return;
                }

                await store.LuuAsync(form.TestCase);
                _daKhoiTao = false;
                await NapCayAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không sửa được test case: " + ex.Message, "Lỗi");
            }
        }

        private async void BtnThemApi_Click(object? sender, EventArgs e)
        {
            var store = TaoStoreTestCaseDong();
            if (store is null)
            {
                return;
            }

            var nhomMacDinh = LayNhomDangChon() ?? "Custom";
            using var form = new FormTestCaseDong(nhomMacDinh: nhomMacDinh);
            if (form.ShowDialog(FindForm()) != DialogResult.OK)
            {
                return;
            }

            try
            {
                await store.LuuAsync(form.TestCase);
                _daKhoiTao = false;
                await NapCayAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thêm được test case: " + ex.Message, "Lỗi");
            }
        }

        private void BtnMoChayTest_Click(object? sender, EventArgs e)
        {
            OpenRunTestRequested?.Invoke(this, EventArgs.Empty);
        }

        private async Task<List<TestCaseDong>> LayDanhSachTestCaseDongAsync()
        {
            var store = TaoStoreTestCaseDong(false);
            if (store is null)
            {
                lblGoiY.Text = "Chưa kết nối được database nên chỉ hiển thị test case [CODE].";
                return [];
            }

            try
            {
                var danhSach = await store.LayDanhSachAsync();
                lblGoiY.Text = "Test case [DB] là case cơ bản thêm từ giao diện; test case [CODE] là case phức tạp viết trong source.";
                return danhSach;
            }
            catch (Exception ex)
            {
                lblGoiY.Text = "Không đọc được test case [DB]: " + ex.Message;
                return [];
            }
        }

        private TestCaseDongStore? TaoStoreTestCaseDong(bool thongBaoNeuLoi = true)
        {
            if (!AppHost.IsInitialized || !AppHost.Instance.DatabaseSanSang)
            {
                if (thongBaoNeuLoi)
                {
                    MessageBox.Show("Chưa kết nối được database HactechTestDb nên không thể thao tác test case cơ bản.", "Thông báo");
                }

                return null;
            }

            return new TestCaseDongStore(AppHost.Instance.Database);
        }

        private string? LayNhomDangChon()
        {
            var node = treCayBoSuuTap.SelectedNode;
            while (node != null)
            {
                if (node.Tag is NodeTag tag && !string.IsNullOrWhiteSpace(tag.Nhom))
                {
                    return tag.Nhom;
                }

                node = node.Parent;
            }

            return null;
        }

        private sealed class NodeTag
        {
            public string Loai { get; }
            public int? TestCaseDongId { get; }
            public string? Nhom { get; }

            public NodeTag(string loai, int? testCaseDongId = null, string? Nhom = null)
            {
                Loai = loai;
                TestCaseDongId = testCaseDongId;
                this.Nhom = Nhom;
            }
        }
    }
}
