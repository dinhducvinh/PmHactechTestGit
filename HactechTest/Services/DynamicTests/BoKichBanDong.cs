using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using HactechTest.ApiShopTesting.Core;
using HactechTest.Models.TestCases;

namespace HactechTest.Services.DynamicTests
{
    public static class BoKichBanDong
    {
        private const string MaChapNhanBatKy = "*";

        public static KichBanApi TaoKichBan(TestCaseDong testCase)
        {
            var maChapNhan = TaoTapMaChapNhan(testCase);

            return new KichBanApi
            {
                Ma = testCase.Ma,
                Nhom = testCase.Nhom,
                TenHienThi = testCase.TenHienThi,
                MoTa = TaoMoTa(testCase),
                TaoYeuCauAsync = ctx => TaoYeuCauAsync(testCase, ctx),
                MaChapNhan = maChapNhan,
                KiemTraThemAsync = (response, _, _) => KiemTraThemAsync(testCase, response)
            };
        }

        public static bool LaMaChapNhanBatKy(IReadOnlySet<string> maChapNhan)
        {
            return maChapNhan.Contains(MaChapNhanBatKy);
        }

        private static async Task<YeuCauApi> TaoYeuCauAsync(TestCaseDong testCase, NguCanhKiemThu ctx)
        {
            var endpoint = ApDungPathParams(testCase.Endpoint, testCase.PathParamsJson);
            var body = DocBodyJson(testCase.BodyJson);
            var token = string.Equals(testCase.AuthMode, "bearer_seed", StringComparison.OrdinalIgnoreCase)
                ? await HelperTC.YeuCauTokenHopLeAsync(ctx)
                : null;

            var request = new YeuCauApi(new HttpMethod(testCase.HttpMethod), endpoint, body, token);
            foreach (var (key, value) in DocHeadersJson(testCase.HeadersJson))
            {
                request.Headers[key] = value;
            }

            return request;
        }

        private static Task<KetQuaKiemTraThem> KiemTraThemAsync(TestCaseDong testCase, PhanHoiApi response)
        {
            if (testCase.ExpectedHttpStatus.HasValue &&
                (int)response.HttpStatusCode != testCase.ExpectedHttpStatus.Value)
            {
                return Task.FromResult(new KetQuaKiemTraThem(
                    false,
                    $"HTTP status phải là {testCase.ExpectedHttpStatus.Value}, thực tế là {(int)response.HttpStatusCode}."));
            }

            if (!string.IsNullOrWhiteSpace(testCase.ExpectedJsonPath))
            {
                var node = LayJsonPath(response.Json, testCase.ExpectedJsonPath);
                if (node is null)
                {
                    return Task.FromResult(new KetQuaKiemTraThem(false, $"Response không có JSON path `{testCase.ExpectedJsonPath}`."));
                }

                if (!string.IsNullOrWhiteSpace(testCase.ExpectedJsonValue))
                {
                    var actual = node.ToString();
                    if (!string.Equals(actual, testCase.ExpectedJsonValue, StringComparison.OrdinalIgnoreCase))
                    {
                        return Task.FromResult(new KetQuaKiemTraThem(
                            false,
                            $"JSON path `{testCase.ExpectedJsonPath}` phải bằng `{testCase.ExpectedJsonValue}`, thực tế là `{actual}`."));
                    }
                }
            }

            return Task.FromResult(KetQuaKiemTraThem.ThanhCong);
        }

        private static IReadOnlySet<string> TaoTapMaChapNhan(TestCaseDong testCase)
        {
            var ma = (testCase.ExpectedCodes ?? "")
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            return ma.Length == 0
                ? new HashSet<string>([MaChapNhanBatKy], StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(ma, StringComparer.OrdinalIgnoreCase);
        }

        private static string TaoMoTa(TestCaseDong testCase)
        {
            var moTa = string.IsNullOrWhiteSpace(testCase.MoTa)
                ? "Test case cơ bản do người dùng cấu hình trong phần mềm."
                : testCase.MoTa;

            return $"{moTa}{Environment.NewLine}{testCase.HttpMethod} {testCase.Endpoint}";
        }

        private static object? DocBodyJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonNode.Parse(json);
        }

        private static string ApDungPathParams(string endpoint, string? pathParamsJson)
        {
            var placeholders = Regex.Matches(endpoint, @"\{([A-Za-z_][A-Za-z0-9_]*)\}");
            if (placeholders.Count == 0)
            {
                return endpoint;
            }

            var pathParams = DocPathParamsJson(pathParamsJson);
            var ketQua = endpoint;
            foreach (Match match in placeholders)
            {
                var tenParam = match.Groups[1].Value;
                if (!pathParams.TryGetValue(tenParam, out var giaTri) || string.IsNullOrWhiteSpace(giaTri))
                {
                    throw new InvalidOperationException($"Endpoint `{endpoint}` cần path param `{tenParam}`. Hãy nhập Path params JSON, ví dụ {{ \"{tenParam}\": \"123\" }}.");
                }

                ketQua = ketQua.Replace(match.Value, Uri.EscapeDataString(giaTri));
            }

            return ketQua;
        }

        private static Dictionary<string, string> DocPathParamsJson(string? json)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(json))
            {
                return result;
            }

            var node = JsonNode.Parse(json);
            if (node is not JsonObject obj)
            {
                throw new InvalidOperationException("Path params JSON phải là object, ví dụ { \"id\": \"123\" }.");
            }

            foreach (var item in obj)
            {
                result[item.Key] = item.Value?.ToString() ?? "";
            }

            return result;
        }

        private static Dictionary<string, string> DocHeadersJson(string? json)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(json))
            {
                return result;
            }

            var node = JsonNode.Parse(json);
            if (node is not JsonObject obj)
            {
                throw new InvalidOperationException("Headers JSON phải là object, ví dụ { \"X-Test\": \"1\" }.");
            }

            foreach (var item in obj)
            {
                result[item.Key] = item.Value?.ToString() ?? "";
            }

            return result;
        }

        private static JsonNode? LayJsonPath(JsonNode? root, string path)
        {
            if (root is null || string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var current = root;
            var normalized = path.Trim();
            if (normalized.StartsWith("$."))
            {
                normalized = normalized[2..];
            }
            else if (normalized.StartsWith("$"))
            {
                normalized = normalized[1..].TrimStart('.');
            }

            foreach (var rawPart in normalized.Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                var part = rawPart;
                while (part.Length > 0)
                {
                    var bracketIndex = part.IndexOf('[');
                    var propertyName = bracketIndex >= 0 ? part[..bracketIndex] : part;
                    if (!string.IsNullOrWhiteSpace(propertyName))
                    {
                        current = current?[propertyName];
                    }

                    if (current is null || bracketIndex < 0)
                    {
                        break;
                    }

                    var endBracketIndex = part.IndexOf(']', bracketIndex + 1);
                    if (endBracketIndex < 0 ||
                        !int.TryParse(part[(bracketIndex + 1)..endBracketIndex], out var arrayIndex) ||
                        current is not JsonArray array ||
                        arrayIndex < 0 ||
                        arrayIndex >= array.Count)
                    {
                        return null;
                    }

                    current = array[arrayIndex];
                    part = part[(endBracketIndex + 1)..];
                }

                if (current is null)
                {
                    return null;
                }
            }

            return current;
        }
    }
}

