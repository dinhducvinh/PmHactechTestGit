namespace HactechTest.Models.TestCases;

public sealed class TestCaseDong
{
    public int Id { get; init; }
    public string Ma { get; init; } = "";
    public string Nhom { get; init; } = "Custom";
    public string TenHienThi { get; init; } = "";
    public string MoTa { get; init; } = "";
    public string HttpMethod { get; init; } = "GET";
    public string Endpoint { get; init; } = "";
    public string AuthMode { get; init; } = "none";
    public string? PathParamsJson { get; init; }
    public string? HeadersJson { get; init; }
    public string? BodyJson { get; init; }
    public string? ExpectedCodes { get; init; }
    public int? ExpectedHttpStatus { get; init; }
    public string? ExpectedJsonPath { get; init; }
    public string? ExpectedJsonValue { get; init; }
}
