using System.Threading.Tasks;
using System.Windows.Forms;
using ApiTest.Services;

namespace ApiTest.Control
{
    public partial class BoSuuTap : UserControl
    {
        public event EventHandler<TestCaseSelectedEventArgs>? OpenApiTestRequested;
        public event EventHandler<RunTestRequestedEventArgs>? OpenRunTestRequested;

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
                treCayBoSuuTap.NodeMouseDoubleClick += TreCayBoSuuTap_NodeMouseDoubleClick;
            }
        }

        private void BoSuuTap_Load(object? sender, EventArgs e)
        {
            _ = NapCayAsync();
        }

        public Task NapCayAsync()
        {
            if (_daKhoiTao && treCayBoSuuTap.Nodes.Count > 0)
            {
                return Task.CompletedTask;
            }

            treCayBoSuuTap.BeginUpdate();
            try
            {
                treCayBoSuuTap.Nodes.Clear();

                var nodeBst = new TreeNode("[COL] " + AuthCatalog.TenBoSuuTap)
                {
                    Tag = new NodeTag("BST", 1)
                };
                treCayBoSuuTap.Nodes.Add(nodeBst);

                var nodeMod = new TreeNode("[MOD] " + AuthCatalog.TenModule)
                {
                    Tag = new NodeTag("MD", 11)
                };
                nodeBst.Nodes.Add(nodeMod);

                int idTc = 100;
                foreach (var tc in AuthCatalog.TatCa)
                {
                    var n = new TreeNode($"[TC] [{tc.HttpMethod}] {tc.DisplayName}")
                    {
                        Tag = new NodeTag("TC", idTc++) { ApiTestCaseId = tc.Id }
                    };
                    nodeMod.Nodes.Add(n);
                }

                treCayBoSuuTap.ExpandAll();
                treCayBoSuuTap.SelectedNode = treCayBoSuuTap.Nodes[0];

                _daKhoiTao = true;
            }
            finally
            {
                treCayBoSuuTap.EndUpdate();
            }

            return Task.CompletedTask;
        }

        private void BtnTaoProject_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "Hiện tại bộ sưu tập 'UAV API' được seed sẵn từ catalog code, " +
                "không hỗ trợ tạo dự án mới qua giao diện. Sau khi đặc tả các API khác " +
                "được bổ sung, hãy thêm vào AuthCatalog.cs hoặc tạo catalog mới.",
                "Thông báo");
        }

        private void BtnThemModule_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "Module hiện tại chỉ có 'Xác thực & Tài khoản'. Module khác sẽ được " +
                "thêm tự động khi catalog được mở rộng.",
                "Thông báo");
        }

        private void BtnXoaNode_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "Không hỗ trợ xóa node trong catalog đã định nghĩa sẵn.",
                "Thông báo");
        }

        private void BtnSuaTestCase_Click(object? sender, EventArgs e)
        {
            if (treCayBoSuuTap.SelectedNode?.Tag is NodeTag tag && tag.Loai == "TC")
            {
                OpenApiTestRequested?.Invoke(this, new TestCaseSelectedEventArgs(tag.Id));
                return;
            }
            MessageBox.Show("Chọn một test case để mở giao diện chi tiết.", "Thông báo");
        }

        private void BtnThemApi_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "Test case được seed từ catalog code (Services/AuthCatalog.cs). " +
                "Sau này sẽ bổ sung UI để thêm test case động.",
                "Thông báo");
        }

        private void BtnMoChayTest_Click(object? sender, EventArgs e)
        {
            OpenRunTestRequested?.Invoke(this, new RunTestRequestedEventArgs(1, 11));
        }

        private void TreCayBoSuuTap_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node?.Tag is NodeTag tag && tag.Loai == "TC")
            {
                OpenApiTestRequested?.Invoke(this, new TestCaseSelectedEventArgs(tag.Id));
            }
        }

        private sealed class NodeTag
        {
            public string Loai { get; }
            public int Id { get; }
            public string? ApiTestCaseId { get; init; }

            public NodeTag(string loai, int id)
            {
                Loai = loai;
                Id = id;
            }
        }
    }

    public sealed class TestCaseSelectedEventArgs : EventArgs
    {
        public int IdTestCase { get; }
        public TestCaseSelectedEventArgs(int idTestCase) { IdTestCase = idTestCase; }
    }

    public sealed class RunTestRequestedEventArgs : EventArgs
    {
        public int? IdBoSuuTap { get; }
        public int? IdMoDun { get; }
        public RunTestRequestedEventArgs(int? idBoSuuTap, int? idMoDun)
        {
            IdBoSuuTap = idBoSuuTap;
            IdMoDun = idMoDun;
        }
    }
}
