using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KiemThuApiShop.Core;

public sealed class MayKhachApi : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly Uri _baseUri;

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
            var json = JsonSerializer.Serialize(yeuCau.Body, TuyChonJson.MacDinh);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
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
                code = TienIchJson.DocChuoi(jsonNode, "code");
                message = TienIchJson.DocChuoi(jsonNode, "message");
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

public static class TienIchJson
{
    public static string? DocChuoi(JsonNode? node, params string[] tenTruong)
    {
        foreach (var ten in tenTruong)
        {
            var giaTri = node?[ten];
            if (giaTri is null)
            {
                continue;
            }

            return giaTri.ToString();
        }

        return null;
    }

    public static long? DocSoNguyen(JsonNode? node, params string[] tenTruong)
    {
        foreach (var ten in tenTruong)
        {
            var text = DocChuoi(node, ten);
            if (long.TryParse(text, out var so))
            {
                return so;
            }
        }

        return null;
    }

    public static bool? DocBool(JsonNode? node, params string[] tenTruong)
    {
        foreach (var ten in tenTruong)
        {
            var giaTri = node?[ten];
            if (giaTri is null)
            {
                continue;
            }

            if (bool.TryParse(giaTri.ToString(), out var boolean))
            {
                return boolean;
            }

            if (long.TryParse(giaTri.ToString(), out var so))
            {
                return so != 0;
            }
        }

        return null;
    }

    public static JsonArray? LayMang(JsonNode? node)
    {
        return node as JsonArray;
    }

    public static bool CoTruong(JsonNode? node, string tenTruong)
    {
        return node is JsonObject obj && obj.ContainsKey(tenTruong);
    }

    public static string RutGon(string? raw, int max = 500)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "";
        }

        var motDong = raw.Replace(Environment.NewLine, " ");
        return motDong.Length <= max ? motDong : motDong[..max] + "...";
    }
}
