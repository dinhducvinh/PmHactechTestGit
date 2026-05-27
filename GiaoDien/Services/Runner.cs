using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ApiTest.Services.Runner
{
    public interface IOtpProvider
    {
        Task<string?> LayOtpAsync(string phoneNumber, ApiResponse? createCodeResponse, CancellationToken ct);
    }

    public sealed class OtpProvider : IOtpProvider
    {
        private readonly TestDataConfig _config;

        public OtpProvider(TestDataConfig config)
        {
            _config = config;
        }

        public Task<string?> LayOtpAsync(string phoneNumber, ApiResponse? createCodeResponse, CancellationToken ct)
        {
            switch ((_config.OtpStrategy ?? "static").ToLowerInvariant())
            {
                case "static":
                    return Task.FromResult<string?>(_config.StaticOtp);

                case "from_response":
                    if (createCodeResponse == null) return Task.FromResult<string?>(null);
                    return Task.FromResult(LayTuJsonPath(createCodeResponse, _config.OtpResponseField));

                default:
                    return Task.FromResult<string?>(null);
            }
        }

        private static string? LayTuJsonPath(ApiResponse response, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            try
            {
                using var doc = JsonDocument.Parse(response.RawBody);
                JsonElement current = doc.RootElement;
                foreach (var part in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (current.ValueKind != JsonValueKind.Object) return null;
                    if (!current.TryGetProperty(part, out var next)) return null;
                    current = next;
                }
                return current.ValueKind switch
                {
                    JsonValueKind.String => current.GetString(),
                    JsonValueKind.Number => current.GetRawText(),
                    _ => current.GetRawText()
                };
            }
            catch
            {
                return null;
            }
        }
    }

    public sealed class ApiTestRunner
    {
        private static readonly HttpClient _http = TaoHttpClient();

        private readonly AppConfig _config;

        public ApiTestRunner(AppConfig config)
        {
            _config = config;
        }

        private static HttpClient TaoHttpClient()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true
            };
            return new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<ApiResponse> ThucThiAsync(
            string httpMethod,
            string baseUrl,
            string pathTemplate,
            TestRequestPlan plan,
            CancellationToken ct)
        {
            var url = GhepUrl(baseUrl, plan.OverridePath ?? pathTemplate, plan.QueryParams);
            using var request = new HttpRequestMessage(new HttpMethod(httpMethod.ToUpperInvariant()), url);

            string? requestBodyJson = null;
            if (plan.JsonBody != null)
            {
                requestBodyJson = JsonSerializer.Serialize(plan.JsonBody, new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
                request.Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");
            }

            if (plan.Headers != null)
            {
                foreach (var (k, v) in plan.Headers)
                {
                    if (!request.Headers.TryAddWithoutValidation(k, v) && request.Content != null)
                    {
                        request.Content.Headers.TryAddWithoutValidation(k, v);
                    }
                }
            }

            var sw = Stopwatch.StartNew();
            HttpResponseMessage? response = null;
            string rawBody = "";
            string? transportError = null;
            int httpStatus = 0;
            var headers = new Dictionary<string, string>();

            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(3, _config.ApiServer.TimeoutSeconds)));
                response = await _http.SendAsync(request, HttpCompletionOption.ResponseContentRead, timeoutCts.Token);
                httpStatus = (int)response.StatusCode;
                rawBody = await response.Content.ReadAsStringAsync(timeoutCts.Token);

                foreach (var h in response.Headers)
                {
                    headers[h.Key] = string.Join(", ", h.Value);
                }
                foreach (var h in response.Content.Headers)
                {
                    headers[h.Key] = string.Join(", ", h.Value);
                }
            }
            catch (Exception ex)
            {
                transportError = ex.GetType().Name + ": " + ex.Message;
            }
            finally
            {
                response?.Dispose();
                sw.Stop();
            }

            var (isJson, code, message, dataElement) = TachJsonResponse(rawBody);
            return new ApiResponse
            {
                HttpStatus = httpStatus,
                RawBody = rawBody,
                AppCode = code,
                Message = message,
                Data = dataElement,
                Headers = headers,
                DurationMs = sw.ElapsedMilliseconds,
                IsJson = isJson,
                TransportError = transportError
            };
        }

        private static string GhepUrl(string baseUrl, string path, Dictionary<string, string?>? query)
        {
            baseUrl = (baseUrl ?? "").TrimEnd('/');
            path = path ?? "";
            string url;
            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = path;
            }
            else
            {
                if (!path.StartsWith("/")) path = "/" + path;
                url = baseUrl + path;
            }

            if (query == null || query.Count == 0) return url;

            var sb = new StringBuilder(url);
            sb.Append(url.Contains('?') ? '&' : '?');
            bool first = true;
            foreach (var (k, v) in query)
            {
                if (!first) sb.Append('&');
                sb.Append(Uri.EscapeDataString(k));
                sb.Append('=');
                sb.Append(Uri.EscapeDataString(v ?? ""));
                first = false;
            }
            return sb.ToString();
        }

        private static (bool IsJson, string? Code, string? Message, JsonElement? Data) TachJsonResponse(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return (false, null, null, null);
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement.Clone();
                string? code = null;
                string? message = null;
                JsonElement? data = null;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("code", out var c))
                    {
                        code = c.ValueKind == JsonValueKind.String ? c.GetString() : c.GetRawText();
                    }
                    if (root.TryGetProperty("message", out var m))
                    {
                        message = m.ValueKind == JsonValueKind.String ? m.GetString() : m.GetRawText();
                    }
                    if (root.TryGetProperty("data", out var d))
                    {
                        data = d;
                    }
                }
                return (true, code, message, data);
            }
            catch
            {
                return (false, null, null, null);
            }
        }
    }

    public static class TestEngine
    {
        /// <summary>
        /// Chạy danh sách test case theo thứ tự, gọi callback sau mỗi test để cập nhật UI.
        /// </summary>
        public static async Task<IReadOnlyList<TestRunResult>> ChayDanhSachAsync(
            IReadOnlyList<TestCaseDefinition> danhSach,
            TestContext context,
            bool dungKhiLoi,
            Action<TestRunResult>? onTestCompleted,
            CancellationToken ct)
        {
            var ketQua = new List<TestRunResult>();
            int stt = 0;

            foreach (var tc in danhSach)
            {
                ct.ThrowIfCancellationRequested();
                stt++;

                var item = await ChayMotTestAsync(tc, context, context.BaseUrl, stt, ct);
                ketQua.Add(item);
                onTestCompleted?.Invoke(item);

                if (dungKhiLoi && item.Result != "PASS")
                {
                    break;
                }
            }

            return ketQua;
        }

        private static async Task<TestRunResult> ChayMotTestAsync(
            TestCaseDefinition tc, TestContext context, string baseUrl, int stt, CancellationToken ct)
        {
            string? requestBodyJson = null;
            try
            {
                TestRequestPlan plan;
                if (tc.PrepareAsync != null)
                {
                    plan = await tc.PrepareAsync(context, ct);
                }
                else
                {
                    plan = new TestRequestPlan();
                }

                if (plan.SkipExecution)
                {
                    return new TestRunResult
                    {
                        SequenceNo = stt,
                        TestCaseId = tc.Id,
                        ApiName = tc.ApiName,
                        DisplayName = tc.DisplayName,
                        HttpMethod = tc.HttpMethod,
                        Url = tc.PathTemplate,
                        ExpectedAppCode = tc.ExpectedAppCode,
                        Result = "SKIP",
                        DurationMs = 0,
                        Reason = plan.SkipReason ?? "Bỏ qua test."
                    };
                }

                if (plan.JsonBody != null)
                {
                    requestBodyJson = JsonSerializer.Serialize(plan.JsonBody, new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });
                }

                var response = await context.Runner.ThucThiAsync(
                    tc.HttpMethod, baseUrl, tc.PathTemplate, plan, ct);

                context.LastResponseByApi[tc.ApiName] = response;

                if (response.TransportError != null)
                {
                    return new TestRunResult
                    {
                        SequenceNo = stt,
                        TestCaseId = tc.Id,
                        ApiName = tc.ApiName,
                        DisplayName = tc.DisplayName,
                        HttpMethod = tc.HttpMethod,
                        Url = TaoUrlHienThi(baseUrl, plan.OverridePath ?? tc.PathTemplate),
                        RequestBodyJson = requestBodyJson,
                        ExpectedAppCode = tc.ExpectedAppCode,
                        ActualAppCode = null,
                        HttpStatus = null,
                        Result = "ERROR",
                        DurationMs = response.DurationMs,
                        Reason = "Không gửi được request: " + response.TransportError,
                        ResponseBody = response.RawBody
                    };
                }

                bool match = string.Equals(response.AppCode, tc.ExpectedAppCode, StringComparison.OrdinalIgnoreCase);
                string ketQua = match ? "PASS" : "FAIL";
                string? lyDo = match
                    ? null
                    : $"Expected code='{tc.ExpectedAppCode}' nhưng nhận code='{response.AppCode ?? "(null)"}', HTTP {response.HttpStatus}";

                string? note = null;
                if (match && tc.PostCheckAsync != null)
                {
                    var outcome = await tc.PostCheckAsync(context, response, ct);
                    if (!outcome.Pass)
                    {
                        ketQua = "FAIL";
                        lyDo = (lyDo == null ? "" : lyDo + " | ") + (outcome.FailReason ?? "Hậu kiểm thất bại.");
                    }
                    note = outcome.Note;
                }

                return new TestRunResult
                {
                    SequenceNo = stt,
                    TestCaseId = tc.Id,
                    ApiName = tc.ApiName,
                    DisplayName = tc.DisplayName,
                    HttpMethod = tc.HttpMethod,
                    Url = TaoUrlHienThi(baseUrl, plan.OverridePath ?? tc.PathTemplate),
                    RequestBodyJson = requestBodyJson,
                    ExpectedAppCode = tc.ExpectedAppCode,
                    ActualAppCode = response.AppCode,
                    HttpStatus = response.HttpStatus,
                    Result = ketQua,
                    DurationMs = response.DurationMs,
                    Reason = lyDo,
                    ResponseBody = response.RawBody,
                    Note = note
                };
            }
            catch (OperationCanceledException)
            {
                return new TestRunResult
                {
                    SequenceNo = stt,
                    TestCaseId = tc.Id,
                    ApiName = tc.ApiName,
                    DisplayName = tc.DisplayName,
                    HttpMethod = tc.HttpMethod,
                    Url = tc.PathTemplate,
                    RequestBodyJson = requestBodyJson,
                    ExpectedAppCode = tc.ExpectedAppCode,
                    Result = "SKIP",
                    DurationMs = 0,
                    Reason = "Đã hủy chạy."
                };
            }
            catch (Exception ex)
            {
                return new TestRunResult
                {
                    SequenceNo = stt,
                    TestCaseId = tc.Id,
                    ApiName = tc.ApiName,
                    DisplayName = tc.DisplayName,
                    HttpMethod = tc.HttpMethod,
                    Url = tc.PathTemplate,
                    RequestBodyJson = requestBodyJson,
                    ExpectedAppCode = tc.ExpectedAppCode,
                    Result = "ERROR",
                    DurationMs = 0,
                    Reason = ex.GetType().Name + ": " + ex.Message
                };
            }
        }

        private static string TaoUrlHienThi(string baseUrl, string path)
        {
            baseUrl = (baseUrl ?? "").TrimEnd('/');
            path = path ?? "";
            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return path;
            if (!path.StartsWith("/")) path = "/" + path;
            return baseUrl + path;
        }
    }
}
