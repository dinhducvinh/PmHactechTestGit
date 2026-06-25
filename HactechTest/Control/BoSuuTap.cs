using HactechTest.ApiShopTesting.Core;
using HactechTest.Models.TestCases;
using HactechTest.Services.TestCases;

namespace HactechTest.Control
{
    public partial class BoSuuTap : UserControl
    {
        public event EventHandler? OpenRunTestRequested;

        private readonly BoSuuTapService _boSuuTapService = new();
        private readonly IReadOnlyList<KichBanApi> _kichBanCode;
        private List<TestCaseDong> _testCaseDong = new();
        private bool _daKhoiTao;

        public BoSuuTap()
        {
            _kichBanCode = _boSuuTapService.LayKichBanCode();
            InitializeComponent();
            if (!DesignMode)
            {
                btnXoaNode.Click += BtnXoaNode_Click;
            }
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

            if (!DamBaoCoTheThaoTacTestCaseDong())
            {
                return;
            }

            try
            {
                await _boSuuTapService.XoaTestCaseDongAsync(tag.TestCaseDongId.Value);
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

            if (!DamBaoCoTheThaoTacTestCaseDong())
            {
                return;
            }

            try
            {
                var testCase = await _boSuuTapService.LayTestCaseDongAsync(tag.TestCaseDongId.Value);
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

                await _boSuuTapService.LuuTestCaseDongAsync(form.TestCase);
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
            if (!DamBaoCoTheThaoTacTestCaseDong())
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
                await _boSuuTapService.LuuTestCaseDongAsync(form.TestCase);
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
            if (!_boSuuTapService.CoTheThaoTacTestCaseDong())
            {
                lblGoiY.Text = "Chưa kết nối được database nên chỉ hiển thị test case [CODE].";
                return [];
            }

            try
            {
                var danhSach = await _boSuuTapService.LayDanhSachTestCaseDongAsync();
                lblGoiY.Text = "Test case [DB] là case cơ bản thêm từ giao diện; test case [CODE] là case phức tạp viết trong source.";
                return danhSach;
            }
            catch (Exception ex)
            {
                lblGoiY.Text = "Không đọc được test case [DB]: " + ex.Message;
                return [];
            }
        }

        private bool DamBaoCoTheThaoTacTestCaseDong()
        {
            if (_boSuuTapService.CoTheThaoTacTestCaseDong())
            {
                return true;
            }

            MessageBox.Show("Chưa kết nối được database HactechTestDb nên không thể thao tác test case cơ bản.", "Thông báo");
            return false;
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
