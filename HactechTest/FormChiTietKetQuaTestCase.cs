using System.Text.Json;

namespace HactechTest
{
    public partial class FormChiTietKetQuaTestCase : Form
    {
        public FormChiTietKetQuaTestCase(TestCaseResultDetailData chiTiet)
        {
            InitializeComponent();
            GanDuLieu(chiTiet);
        }

        private void GanDuLieu(TestCaseResultDetailData chiTiet)
        {
            Text = $"Chi tiết test case - {GiaTriHoacMacDinh(chiTiet.TestCaseName, "Không xác định")}";

            lblTenTestCase.Text =
                $"Test case #{GiaTriHoacMacDinh(chiTiet.SequenceNo, "-")}: {GiaTriHoacMacDinh(chiTiet.TestCaseName, "(chưa có tên)")}";

            lblTongQuan.Text =
                $"Kết quả: {GiaTriHoacMacDinh(chiTiet.Result, "-")} | TT thực: {GiaTriHoacMacDinh(chiTiet.ActualStatus, "-")} | TT mong: {GiaTriHoacMacDinh(chiTiet.ExpectedStatus, "-")} | Thời gian: {GiaTriHoacMacDinh(chiTiet.Duration, "-")} ms";

            lblRequestInfo.Text =
                $"Method: {GiaTriHoacMacDinh(chiTiet.Method, "-")} | URL: {GiaTriHoacMacDinh(chiTiet.Url, "-")}";

            txtLyDo.Text = string.IsNullOrWhiteSpace(chiTiet.Reason)
                ? "Không có lý do hoặc ghi chú chi tiết."
                : chiTiet.Reason;

            rtbResponseBody.Text = DinhDangResponseBody(chiTiet.ResponseBody);
            rtbResponseBody.SelectionStart = 0;
            rtbResponseBody.ScrollToCaret();
        }

        private static string DinhDangResponseBody(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return "(Không có response body)";
            }

            var noiDung = responseBody.Trim();

            if (!noiDung.StartsWith("{") && !noiDung.StartsWith("["))
            {
                return noiDung;
            }

            try
            {
                using var taiLieuJson = JsonDocument.Parse(noiDung);
                return JsonSerializer.Serialize(
                    taiLieuJson.RootElement,
                    new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                return noiDung;
            }
        }

        private static string GiaTriHoacMacDinh(string giaTri, string macDinh)
        {
            return string.IsNullOrWhiteSpace(giaTri) ? macDinh : giaTri;
        }
    }

    public sealed class TestCaseResultDetailData
    {
        public string SequenceNo { get; init; } = string.Empty;
        public string TestCaseName { get; init; } = string.Empty;
        public string Result { get; init; } = string.Empty;
        public string ActualStatus { get; init; } = string.Empty;
        public string ExpectedStatus { get; init; } = string.Empty;
        public string Duration { get; init; } = string.Empty;
        public string Reason { get; init; } = string.Empty;
        public string Method { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
        public string ResponseBody { get; init; } = string.Empty;
    }
}

