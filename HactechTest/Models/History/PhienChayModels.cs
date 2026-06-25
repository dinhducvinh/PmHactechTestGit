namespace HactechTest.Models.History;

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

public sealed record PhienChayDaLuu(
    int Id,
    DateTime RunAt,
    string NguoiChay,
    string Machine,
    string OperatingSystem,
    string BaseUrl,
    string CheDoChay,
    string CheDoLoi,
    int TotalTests,
    int PassedTests,
    int FailedTests,
    decimal PassRate,
    int AverageDurationMs,
    List<ChiTietPhienChayDaLuu> Details);

public sealed record ChiTietPhienChayDaLuu(
    int SequenceNo,
    string TestCaseName,
    string Result,
    string ActualStatus,
    int DurationMs,
    string Reason,
    string HttpMethod = "",
    string Url = "",
    string ExpectedStatus = "",
    string RequestBody = "",
    string ResponseBody = "");
