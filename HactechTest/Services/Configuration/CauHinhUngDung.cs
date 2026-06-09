using System.Text.Json;
using System.Text.Json.Serialization;
using HactechTest.ApiShopTesting.Core;

namespace HactechTest.Services.Configuration
{
    public sealed class CauHinhUngDung
    {
        private const string TenThuMucCauHinh = "HactechTest";
        private const string TenFileCauHinh = "cauhinh-ungdung.json";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static CauHinhUngDung? _instance;
        private static readonly object _lock = new();

        public string ConnectionString { get; set; } = "";
        public string BaseUrl { get; set; } = "";
        public int TimeoutGiay { get; set; } = 60;
        public DateTimeOffset CapNhatLuc { get; set; }

        public static CauHinhUngDung Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (_lock)
                {
                    _instance ??= Doc();
                    return _instance;
                }
            }
        }

        public static string ThuMucCauHinh =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), TenThuMucCauHinh);

        public static string DuongDanCauHinh =>
            Path.Combine(ThuMucCauHinh, TenFileCauHinh);

        public static CauHinhUngDung Doc()
        {
            var cauHinh = DocTuFile(DuongDanCauHinh) ?? new CauHinhUngDung();
            if (string.IsNullOrWhiteSpace(cauHinh.ConnectionString))
            {
                cauHinh.ConnectionString = DocConnectionStringCu();
            }

            if (string.IsNullOrWhiteSpace(cauHinh.BaseUrl))
            {
                cauHinh.BaseUrl = DocBaseUrlCu();
            }

            return cauHinh;
        }

        public static void TaiLai()
        {
            lock (_lock)
            {
                _instance = Doc();
            }
        }

        public static void LuuChuoiKetNoiDatabase(string connectionString)
        {
            var cauHinh = Instance;
            cauHinh.ConnectionString = connectionString.Trim();
            cauHinh.Luu();
        }

        public static void LuuBaseUrlApi(string baseUrl)
        {
            var cauHinh = Instance;
            cauHinh.BaseUrl = baseUrl.Trim().TrimEnd('/');
            cauHinh.Luu();
        }

        public void Luu()
        {
            CapNhatLuc = DateTimeOffset.Now;
            Directory.CreateDirectory(ThuMucCauHinh);
            File.WriteAllText(DuongDanCauHinh, JsonSerializer.Serialize(this, JsonOptions));

            lock (_lock)
            {
                _instance = this;
            }
        }

        public CauHinhChay TaoCauHinhChay()
        {
            var cauHinh = new CauHinhChay
            {
                BaseUrl = BaseUrl,
                TimeoutGiay = TimeoutGiay
            };

            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                return cauHinh;
            }

            return cauHinh with
            {
                ConnectionStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["HactechTestDb"] = ConnectionString
                }
            };
        }

        private static CauHinhUngDung? DocTuFile(string duongDan)
        {
            if (!File.Exists(duongDan))
            {
                return null;
            }

            try
            {
                var json = File.ReadAllText(duongDan);
                return JsonSerializer.Deserialize<CauHinhUngDung>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        private static string DocConnectionStringCu()
        {
            var duongDan = Path.Combine(ThuMucCauHinh, "cauhinh-database.json");
            return DocGiaTriChuoiTuJson(duongDan, "ConnectionString");
        }

        private static string DocBaseUrlCu()
        {
            var duongDan = Path.Combine(ThuMucCauHinh, "cauhinh-api.json");
            return DocGiaTriChuoiTuJson(duongDan, "BaseUrl");
        }

        private static string DocGiaTriChuoiTuJson(string duongDan, string tenThuocTinh)
        {
            if (!File.Exists(duongDan))
            {
                return "";
            }

            try
            {
                using var document = JsonDocument.Parse(File.ReadAllText(duongDan));
                if (document.RootElement.TryGetProperty(tenThuocTinh, out var element) &&
                    element.ValueKind == JsonValueKind.String)
                {
                    return element.GetString() ?? "";
                }
            }
            catch
            {
                return "";
            }

            return "";
        }
    }
}
