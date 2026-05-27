using System.Text.Json;

namespace ApiTest.Services
{
    public enum SeedAccountStatus
    {
        ChuaDangKy,
        ChoXacNhanOtp,
        DaDangKy
    }

    public sealed class SeedAccount
    {
        public int Id { get; set; }
        public int SoThuTuSinh { get; set; }
        public string PhoneNumber { get; set; } = "";
        public string Password { get; set; } = "";
        public SeedAccountStatus Status { get; set; }
        public string? ServerUserId { get; set; }
    }

    public sealed class TestRequestPlan
    {
        public string? OverridePath { get; init; }
        public Dictionary<string, string?>? QueryParams { get; init; }
        public Dictionary<string, string>? Headers { get; init; }
        public object? JsonBody { get; init; }
        public bool SkipExecution { get; init; }
        public string? SkipReason { get; init; }
    }

    public sealed class ApiResponse
    {
        public int HttpStatus { get; init; }
        public string RawBody { get; init; } = "";
        public string? AppCode { get; init; }
        public string? Message { get; init; }
        public JsonElement? Data { get; init; }
        public Dictionary<string, string> Headers { get; init; } = new();
        public long DurationMs { get; init; }
        public bool IsJson { get; init; }
        public string? TransportError { get; init; }
    }

    public sealed class ValidationOutcome
    {
        public bool Pass { get; init; }
        public string? FailReason { get; init; }
        public string? Note { get; init; }

        public static ValidationOutcome Ok(string? note = null) =>
            new() { Pass = true, Note = note };

        public static ValidationOutcome Fail(string reason) =>
            new() { Pass = false, FailReason = reason };
    }

    public sealed class TestCaseDefinition
    {
        public required string Id { get; init; }
        public required string ApiName { get; init; }
        public required string DisplayName { get; init; }
        public required string HttpMethod { get; init; }
        public required string PathTemplate { get; init; }
        public required string ExpectedAppCode { get; init; }
        public string Description { get; init; } = "";

        public Func<TestContext, CancellationToken, Task<TestRequestPlan>>? PrepareAsync { get; init; }
        public Func<TestContext, ApiResponse, CancellationToken, Task<ValidationOutcome>>? PostCheckAsync { get; init; }
    }

    public sealed class TestContext
    {
        public required AppConfig Config { get; init; }
        public required Database Database { get; init; }
        public required SeedAccountStore SeedStore { get; init; }
        public required Runner.IOtpProvider OtpProvider { get; init; }
        public required Runner.ApiTestRunner Runner { get; init; }
        public required string BaseUrl { get; set; }
        public Dictionary<string, string> Variables { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, ApiResponse> LastResponseByApi { get; } = new();
        public SeedAccount? WorkingAccount { get; set; }
        public string? CurrentLoginToken { get; set; }
        public string? CurrentResetToken { get; set; }
    }

    public sealed class TestRunResult
    {
        public int SequenceNo { get; init; }
        public required string TestCaseId { get; init; }
        public required string ApiName { get; init; }
        public required string DisplayName { get; init; }
        public required string HttpMethod { get; init; }
        public required string Url { get; init; }
        public string? RequestBodyJson { get; init; }
        public required string ExpectedAppCode { get; init; }
        public string? ActualAppCode { get; init; }
        public int? HttpStatus { get; init; }
        public required string Result { get; init; }   // PASS | FAIL | ERROR | SKIP
        public required long DurationMs { get; init; }
        public string? Reason { get; init; }
        public string? ResponseBody { get; init; }
        public string? Note { get; init; }
    }
}
