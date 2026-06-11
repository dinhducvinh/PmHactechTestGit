using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace HactechTest.ApiShopTesting.Core;

public sealed record CauHinhChay
{
    [JsonPropertyName("baseUrl")]
    public string BaseUrl { get; init; } = "";

    [JsonPropertyName("timeoutGiay")]
    public int TimeoutGiay { get; init; } = 60;

    [JsonPropertyName("connectionStrings")]
    public Dictionary<string, string> ConnectionStrings { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonIgnore]
    public string ChuoiKetNoiSqlServer =>
        LayChuoiKetNoiSqlServer(ConnectionStrings);

    private static string LayChuoiKetNoiSqlServer(Dictionary<string, string> connectionStrings)
    {
        if (connectionStrings.TryGetValue("HactechTestDb", out var chuoiKetNoi) &&
            !string.IsNullOrWhiteSpace(chuoiKetNoi))
        {
            return chuoiKetNoi;
        }

        throw new InvalidOperationException("Chưa cấu hình database. Hãy cấu hình DB ở màn hình đăng nhập.");
    }

}

public sealed class MayKhachApi : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly Uri _baseUri;

    public MayKhachApi(CauHinhChay cauHinh)
        : this(cauHinh.BaseUrl, TimeSpan.FromSeconds(cauHinh.TimeoutGiay))
    {
    }

    public MayKhachApi(string baseUrl, TimeSpan timeout)
    {
        _baseUri = new Uri(baseUrl.TrimEnd('/') + "/");
        _httpClient = new HttpClient { Timeout = timeout };
    }

    public async Task<PhanHoiApi> GuiAsync(YeuCauApi yeuCau, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(yeuCau.PhuongThuc, TaoUri(yeuCau.DuongDan));

        if (!string.IsNullOrWhiteSpace(yeuCau.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", yeuCau.Token);
        }

        if (yeuCau.Body is not null && yeuCau.PhuongThuc != HttpMethod.Get)
        {
            var json = JsonSerializer.Serialize(yeuCau.Body, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        foreach (var header in yeuCau.Headers)
        {
            if (string.Equals(header.Key, "Authorization", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(yeuCau.Token))
            {
                continue;
            }

            if (string.Equals(header.Key, "Content-Type", StringComparison.OrdinalIgnoreCase) &&
                request.Content is not null)
            {
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(header.Value);
                continue;
            }

            if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value) &&
                request.Content is not null)
            {
                request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        var dongHo = Stopwatch.StartNew();
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        dongHo.Stop();

        JsonNode? jsonNode = null;
        string? code = null;
        string? message = null;
        JsonNode? data = null;

        if (!string.IsNullOrWhiteSpace(raw))
        {
            try
            {
                jsonNode = JsonNode.Parse(raw);
                code = jsonNode?["code"]?.ToString();
                message = jsonNode?["message"]?.ToString();
                data = jsonNode?["data"];
            }
            catch (JsonException)
            {
                message = raw;
            }
        }

        return new PhanHoiApi
        {
            HttpStatusCode = response.StatusCode,
            NoiDungRaw = raw,
            Json = jsonNode,
            Code = code,
            Message = message,
            Data = data,
            ThoiGianPhanHoi = dongHo.Elapsed
        };
    }

    private Uri TaoUri(string duongDan)
    {
        if (Uri.TryCreate(duongDan, UriKind.Absolute, out var uri))
        {
            return uri;
        }

        return new Uri(_baseUri, duongDan.TrimStart('/'));
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}



