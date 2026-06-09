namespace HactechTest.Services.History
{
    public sealed class ChiTietKetQuaTestCase
    {
        public int SequenceNo { get; init; }
        public required string DisplayName { get; init; }
        public required string HttpMethod { get; init; }
        public required string Url { get; init; }
        public string? RequestBodyJson { get; init; }
        public required string ExpectedAppCode { get; init; }
        public string? ActualAppCode { get; init; }
        public int? HttpStatus { get; init; }
        public required string Result { get; init; }
        public required long DurationMs { get; init; }
        public string? Reason { get; init; }
        public string? ResponseBody { get; init; }
    }
}

// đây là model tạm thời trong code, còn dbo.chi_tiet_phien_chay là bảng lưu lâu dài trong database.
/*
1. Người dùng bấm Chạy test
2. Mỗi test case chạy xong sinh ra kết quả
3. App gom kết quả đó thành List<TestRunResult> trong RAM
4. Người dùng bấm Lưu phiên chạy
5. PhienChayStore ghi:
   - thông tin tổng phiên vào dbo.phien_chay
   - từng TestRunResult vào dbo.chi_tiet_phien_chay
*/