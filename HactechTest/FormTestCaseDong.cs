using System.Text.Json;
using HactechTest.Services.DynamicTests;

namespace HactechTest
{
    public partial class FormTestCaseDong : Form
    {
        private readonly int _id;

        public TestCaseDong TestCase { get; private set; } = new();

        public FormTestCaseDong(TestCaseDong? testCase = null, string? nhomMacDinh = null)
        {
            InitializeComponent();

            if (testCase is null)
            {
                Text = "Thêm test case cơ bản";
                txtNhom.Text = string.IsNullOrWhiteSpace(nhomMacDinh) ? "Custom" : nhomMacDinh;
                return;
            }

            _id = testCase.Id;
            Text = "Sửa test case cơ bản";
            GanDuLieu(testCase);
        }

        private void GanDuLieu(TestCaseDong testCase)
        {
            txtMa.Text = testCase.Ma;
            txtNhom.Text = testCase.Nhom;
            txtTen.Text = testCase.TenHienThi;
            txtMoTa.Text = testCase.MoTa;
            cboMethod.SelectedItem = testCase.HttpMethod;
            txtEndpoint.Text = testCase.Endpoint;
            cboAuthMode.SelectedIndex = string.Equals(testCase.AuthMode, "bearer_seed", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            txtPathParamsJson.Text = testCase.PathParamsJson ?? "";
            txtHeadersJson.Text = testCase.HeadersJson ?? "";
            txtBodyJson.Text = testCase.BodyJson ?? "";
            txtExpectedCodes.Text = testCase.ExpectedCodes ?? "";
            txtExpectedHttpStatus.Text = testCase.ExpectedHttpStatus?.ToString() ?? "";
            txtExpectedJsonPath.Text = testCase.ExpectedJsonPath ?? "";
            txtExpectedJsonValue.Text = testCase.ExpectedJsonValue ?? "";
        }

        private void BtnLuu_Click(object? sender, EventArgs e)
        {
            if (!ThuThapDuLieu(out var testCase))
            {
                return;
            }

            TestCase = testCase;
            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ThuThapDuLieu(out TestCaseDong testCase)
        {
            testCase = new TestCaseDong();

            var ma = txtMa.Text.Trim();
            var nhom = txtNhom.Text.Trim();
            var ten = txtTen.Text.Trim();
            var endpoint = txtEndpoint.Text.Trim();
            var method = cboMethod.SelectedItem?.ToString() ?? "GET";
            var authMode = cboAuthMode.SelectedIndex == 1 ? "bearer_seed" : "none";

            if (string.IsNullOrWhiteSpace(ma))
            {
                BaoLoi("Mã test case không được rỗng.", txtMa);
                return false;
            }

            if (string.IsNullOrWhiteSpace(nhom))
            {
                BaoLoi("Module/Nhóm không được rỗng.", txtNhom);
                return false;
            }

            if (string.IsNullOrWhiteSpace(ten))
            {
                BaoLoi("Tên hiển thị không được rỗng.", txtTen);
                return false;
            }

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                BaoLoi("Endpoint không được rỗng.", txtEndpoint);
                return false;
            }

            var pathParamsJson = LayChuoiRongThanhNull(txtPathParamsJson.Text);
            if (!KiemTraJson(pathParamsJson, true, "Path params JSON", txtPathParamsJson))
            {
                return false;
            }

            if (!KiemTraEndpointCoDuPathParams(endpoint, pathParamsJson))
            {
                return false;
            }

            var headersJson = LayChuoiRongThanhNull(txtHeadersJson.Text);
            if (!KiemTraJson(headersJson, true, "Headers JSON", txtHeadersJson))
            {
                return false;
            }

            var bodyJson = LayChuoiRongThanhNull(txtBodyJson.Text);
            if (!KiemTraJson(bodyJson, false, "Body JSON", txtBodyJson))
            {
                return false;
            }

            int? expectedHttpStatus = null;
            var httpStatusText = txtExpectedHttpStatus.Text.Trim();
            if (!string.IsNullOrWhiteSpace(httpStatusText))
            {
                if (!int.TryParse(httpStatusText, out var status) || status < 100 || status > 599)
                {
                    BaoLoi("Expected HTTP status phải là số từ 100 đến 599.", txtExpectedHttpStatus);
                    return false;
                }

                expectedHttpStatus = status;
            }

            var expectedCodes = LayChuoiRongThanhNull(txtExpectedCodes.Text);
            var expectedJsonPath = LayChuoiRongThanhNull(txtExpectedJsonPath.Text);
            if (expectedCodes is null && expectedHttpStatus is null && expectedJsonPath is null)
            {
                BaoLoi("Cần nhập ít nhất một kỳ vọng: mã nghiệp vụ, HTTP status hoặc JSON path.", txtExpectedCodes);
                return false;
            }

            testCase = new TestCaseDong
            {
                Id = _id,
                Ma = ma,
                Nhom = nhom,
                TenHienThi = ten,
                MoTa = txtMoTa.Text.Trim(),
                HttpMethod = method,
                Endpoint = endpoint,
                AuthMode = authMode,
                PathParamsJson = pathParamsJson,
                HeadersJson = headersJson,
                BodyJson = bodyJson,
                ExpectedCodes = expectedCodes,
                ExpectedHttpStatus = expectedHttpStatus,
                ExpectedJsonPath = expectedJsonPath,
                ExpectedJsonValue = LayChuoiRongThanhNull(txtExpectedJsonValue.Text)
            };
            return true;
        }

        private static string? LayChuoiRongThanhNull(string value)
        {
            var text = value.Trim();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }

        private static bool KiemTraJson(string? json, bool batBuocLaObject, string tenTruong, System.Windows.Forms.Control control)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return true;
            }

            try
            {
                using var document = JsonDocument.Parse(json);
                if (batBuocLaObject && document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    BaoLoi($"{tenTruong} phải là object, ví dụ {{ \"X-Test\": \"1\" }}.", control);
                    return false;
                }

                return true;
            }
            catch (JsonException ex)
            {
                BaoLoi($"{tenTruong} không đúng định dạng JSON: {ex.Message}", control);
                return false;
            }
        }

        private bool KiemTraEndpointCoDuPathParams(string endpoint, string? pathParamsJson)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(endpoint, @"\{([A-Za-z_][A-Za-z0-9_]*)\}");
            if (matches.Count == 0)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(pathParamsJson))
            {
                var tenParam = matches[0].Groups[1].Value;
                BaoLoi($"Endpoint có path param {{{tenParam}}}. Hãy nhập Path params JSON, ví dụ {{ \"{tenParam}\": \"123\" }}.", txtPathParamsJson);
                return false;
            }

            using var document = JsonDocument.Parse(pathParamsJson);
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var tenParam = match.Groups[1].Value;
                if (!document.RootElement.TryGetProperty(tenParam, out var value) ||
                    value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined ||
                    string.IsNullOrWhiteSpace(value.ToString()))
                {
                    BaoLoi($"Path params JSON thiếu giá trị cho `{tenParam}`.", txtPathParamsJson);
                    return false;
                }
            }

            return true;
        }

        private static void BaoLoi(string message, System.Windows.Forms.Control control)
        {
            MessageBox.Show(message, "Dữ liệu chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            control.Focus();
        }
    }
}
