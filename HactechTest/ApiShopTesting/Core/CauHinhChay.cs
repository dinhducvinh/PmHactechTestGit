using System.Text.Json;
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

public static class TuyChonJson
{
    public static readonly JsonSerializerOptions MacDinh = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}



